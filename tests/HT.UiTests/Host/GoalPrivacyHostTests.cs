using System.Net;
using HT.Application.Goals;
using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using HT.Domain.Time;
using Microsoft.Extensions.DependencyInjection;
using static HT.UnitTests.Fakes.HabitSamples;

namespace HT.UiTests.Host;

/// <summary>
/// US3-AS1, FR-020, HP-011 through the real host: a registered user who opens another account's <c>/goals/{id}</c> gets
/// the Not found page with 404, the same page as for an identifier that belongs to nothing. The response contains
/// neither the goal's name nor its habit's name, and no host log entry contains the private ones: the goal's own name
/// and the custom habit's name. The owner has a goal with its own name on a catalog habit and a goal with the derived
/// name on a custom habit. Slice 5 (US6, FR-017, HP-006): another registered user and an administrator who open the
/// page of a goal with records and notes get the same Not found page with 404, and its HTML holds none of the owner's
/// notes or recorded values. Shared database: own prefixes ("ui-gp-", "Ui-Gp").
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

    [Fact]
    public async Task US6_another_registered_user_and_an_administrator_get_Not_found_for_As_goal_page_with_none_of_its_notes_or_values()
    {
        // The goal starts 2 days before today (UTC, at the host's clock), every day, 20 min: its first day has 47 min and
        // a note, its second day 38 min and another note. The owner's own page shows both, so their absence is the refusal.
        var habitName = Unique("Ui-Gp Records");
        var goalName = Unique("Ui-Gp Private record goal");
        var firstNote = $"Uigpnotemark first {Guid.NewGuid():N}";
        var secondNote = $"Uigpnotemark second {Guid.NewGuid():N}";
        var habit = await host.ArrangeCatalogHabitAsync(Definition(habitName));
        var (ownerBrowser, owner) = await host.SignedInAccountAsync(Email("records-owner"), Role.RegisteredUser);
        GoalId goal;
        await using (var scope = host.Services.CreateAsyncScope())
        {
            var services = scope.ServiceProvider;
            var first = NamedTimeZone.Utc.DateAt(services.GetRequiredService<TimeProvider>().GetUtcNow()).AddDays(-2);
            Assert.True(GoalTarget.TryCreate(new GoalTargetFields
            {
                BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = 20, DurationUnit = DurationUnit.Minutes,
                ScheduleKind = ScheduleKind.FixedDays, Days = Enum.GetValues<DayOfWeek>().ToHashSet(),
            }, out var target, out _));
            Assert.True(GoalName.TryCreate(goalName, out var name, out _));
            var created = await services.GetRequiredService<IGoalStore>().CreateAsync(
                owner, new NewGoal(new GoalHabit(habit, HabitKind.Catalog), first, name, target), NamedTimeZone.Utc, NamedTimeZone.Utc, CancellationToken.None);
            Assert.Equal(GoalWriteOutcomeKind.Created, created.Kind);
            goal = created.Id!.Value;
            var records = services.GetRequiredService<IRecordStore>();
            foreach (var (date, minutes, text) in new[] { (first, 47, firstNote), (first.AddDays(1), 38, secondNote) })
            {
                Assert.True(NoteText.TryCreate(text, out var note, out _));
                var written = await records.AddAsync(
                    owner, goal, new EntrySpan(date, date, date), true, date, RecordValue.FromAmount(minutes), NoteWrite.Replace(note!), RecordLimits.MaxRecordsPerPeriod, CancellationToken.None);
                Assert.Equal(RecordStoreWriteOutcome.Written, written.Outcome);
            }
        }

        var address = $"/goals/{goal.Value}";
        using (ownerBrowser)
        {
            var (status, html) = await ownerBrowser.GetPageAsync(address);

            Assert.Equal(HttpStatusCode.OK, status);
            Assert.Contains(firstNote, html, StringComparison.Ordinal);
            Assert.Contains(secondNote, html, StringComparison.Ordinal);
            Assert.Contains("<td>47 min</td>", html, StringComparison.Ordinal);
            Assert.Contains("<td>38 min</td>", html, StringComparison.Ordinal);
        }

        using var other = await host.SignedInBrowserAsync(Email("records-other"), Role.RegisteredUser);
        using var administrator = await host.SignedInBrowserAsync(Email("records-admin"), Role.Administrator);
        foreach (var browser in new[] { other, administrator })
        {
            var (status, html) = await browser.GetPageAsync(address);

            Assert.Equal(HttpStatusCode.NotFound, status);
            Assert.Contains("Page not found", html, StringComparison.Ordinal);
            Assert.DoesNotContain("Uigpnotemark", html, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("47 min", html, StringComparison.Ordinal);
            Assert.DoesNotContain("38 min", html, StringComparison.Ordinal);
            Assert.DoesNotContain(">History</h2>", html, StringComparison.Ordinal);
            Assert.DoesNotContain(goalName, html, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(habitName, html, StringComparison.OrdinalIgnoreCase);
        }

        Assert.DoesNotContain(host.Logs.Entries, entry =>
            new[] { "Uigpnotemark", goalName }.Any(value =>
                entry.Message.Contains(value, StringComparison.OrdinalIgnoreCase)
                || (entry.Exception?.ToString().Contains(value, StringComparison.OrdinalIgnoreCase) ?? false)));
    }
}
