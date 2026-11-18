using System.Net;
using HT.Application.Goals;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Time;
using Microsoft.Extensions.DependencyInjection;
using static HT.UnitTests.Fakes.HabitSamples;

namespace HT.UiTests.Host;

/// <summary>
/// US3-AS1, FR-020, HP-011 through the real host: a registered user who opens another account's <c>/goals/{id}</c> gets
/// the Not found page with 404, the same page as for an identifier that belongs to nothing. The response contains
/// neither the goal's name nor its habit's name, and no host log entry contains the private ones: the goal's own name
/// and the custom habit's name. The owner has a goal with its own name on a catalog habit and a goal with the derived
/// name on a custom habit. Shared database: own prefixes ("ui-gp-", "Ui-Gp").
/// </summary>
[Collection(HostCollection.Name)]
public sealed class GoalPrivacyHostTests(HostFixture host)
{
    private static string Email(string who) => $"ui-gp-{who}-{Guid.NewGuid():N}@example.com";

    private static string Unique(string prefix) => $"{prefix} {Guid.NewGuid():N}"[..30];

    private async Task<GoalId> CreateAsync(AccountId owner, GoalHabit habit, string? name)
    {
        await using var scope = host.Services.CreateAsyncScope();
        var services = scope.ServiceProvider;
        var today = NamedTimeZone.Utc.DateAt(services.GetRequiredService<TimeProvider>().GetUtcNow());
        var created = await services.GetRequiredService<GoalService>().CreateAsync(ActingUser.ForAccount(owner, Role.RegisteredUser), new GoalInput
        {
            Habit = habit,
            Target = new GoalTargetFields
            {
                BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo, ScheduleKind = ScheduleKind.Times, Times = 3, Period = SchedulePeriod.Week,
            },
            StartDate = today,
            Name = name,
        }, CancellationToken.None);
        Assert.Equal(GoalCreateOutcome.Created, created.Outcome);
        return created.GoalId!.Value;
    }

    [Fact]
    public async Task US3_AS1_another_accounts_goal_page_is_Not_found_and_names_neither_the_goal_nor_its_habit()
    {
        var catalogName = Unique("Ui-Gp Reading");
        var customName = Unique("Ui-Gp Ritual");
        var goalName = Unique("Ui-Gp Private goal");
        var catalog = await host.ArrangeCatalogHabitAsync(Definition(catalogName));
        var (ownerBrowser, owner) = await host.SignedInAccountAsync(Email("owner"), Role.RegisteredUser);
        ownerBrowser.Dispose();
        var custom = await host.ArrangeCustomHabitAsync(owner, Definition(customName, Category.Body));
        var named = await CreateAsync(owner, new GoalHabit(catalog, HabitKind.Catalog), goalName);
        var derived = await CreateAsync(owner, new GoalHabit(custom, HabitKind.Custom), null);
        using var browser = await host.SignedInBrowserAsync(Email("other"), Role.RegisteredUser);

        var (unknownStatus, unknownHtml) = await browser.GetPageAsync($"/goals/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, unknownStatus);
        Assert.Contains("Page not found", unknownHtml, StringComparison.Ordinal);

        foreach (var goal in new[] { named, derived })
        {
            var (status, html) = await browser.GetPageAsync($"/goals/{goal.Value}");

            Assert.Equal(HttpStatusCode.NotFound, status);
            Assert.Contains("Page not found", html, StringComparison.Ordinal);
            Assert.DoesNotContain(goalName, html, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(catalogName, html, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(customName, html, StringComparison.OrdinalIgnoreCase);
        }

        Assert.DoesNotContain(host.Logs.Entries, entry =>
            new[] { goalName, customName }.Any(value =>
                entry.Message.Contains(value, StringComparison.OrdinalIgnoreCase)
                || (entry.Exception?.ToString().Contains(value, StringComparison.OrdinalIgnoreCase) ?? false)));
    }
}
