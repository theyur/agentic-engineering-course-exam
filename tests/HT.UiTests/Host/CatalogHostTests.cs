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
/// US1-AS10, HP-001, HP-003: /catalog through the real host, prerendered. Slice 4 (US6-AS2, HP-005): "Only habits I
/// track" in the address for a registered user and for a visitor.
/// </summary>
[Collection(HostCollection.Name)]
public sealed class CatalogHostTests(HostFixture host)
{
    [Fact]
    public async Task An_anonymous_visitor_gets_the_prerendered_catalog()
    {
        await host.ArrangeCatalogHabitAsync(Definition("Ui-Br Reading", Category.Mind));
        await host.ArrangeCatalogHabitAsync(Definition("Ui-Br Hidden draft", Category.Mind), CatalogHabitState.Draft);
        using var browser = host.CreateBrowser();

        var (status, html) = await browser.GetPageAsync("/catalog");

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains("<h1>Habit catalog</h1>", html, StringComparison.Ordinal);
        Assert.Contains("Ui-Br Reading", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Ui-Br Hidden draft", html, StringComparison.Ordinal);
        Assert.Contains("See details", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task The_address_filters_are_applied_when_prerendering()
    {
        await host.ArrangeCatalogHabitAsync(Definition("Ui-Br Budget", Category.Money, Intent.Reduce, "wallet"));
        await host.ArrangeCatalogHabitAsync(Definition("Ui-Br Saving", Category.Money, Intent.Build, "piggy-bank"));
        await host.ArrangeCatalogHabitAsync(Definition("Ui-Br Sweets", Category.Health, Intent.Reduce, "candy"));
        using var browser = host.CreateBrowser();

        var (status, html) = await browser.GetPageAsync("/catalog?category=money&intent=reduce&q=ui-br");

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains("Ui-Br Budget", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Ui-Br Saving", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Ui-Br Sweets", html, StringComparison.Ordinal);
    }

    /// <summary>Two Published habits with the suffix, and a goal of a new registered user on the first.</summary>
    private async Task<(string Tracked, string Untracked, HttpClient Browser)> TrackedWorldAsync(string suffix)
    {
        var tracked = $"Ui-Ct Read {suffix}";
        var untracked = $"Ui-Ct Yoga {suffix}";
        var reading = await host.ArrangeCatalogHabitAsync(Definition(tracked, Category.Mind));
        await host.ArrangeCatalogHabitAsync(Definition(untracked, Category.Body));
        var (browser, account) = await host.SignedInAccountAsync($"ui-ct-{suffix}@example.com", Role.RegisteredUser);
        await using var scope = host.Services.CreateAsyncScope();
        var services = scope.ServiceProvider;
        var created = await services.GetRequiredService<GoalService>().CreateAsync(ActingUser.ForAccount(account, Role.RegisteredUser), new GoalInput
        {
            Habit = new GoalHabit(reading, HabitKind.Catalog),
            Target = new GoalTargetFields
            {
                BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo, ScheduleKind = ScheduleKind.Times, Times = 3, Period = SchedulePeriod.Week,
            },
            StartDate = NamedTimeZone.Utc.DateAt(services.GetRequiredService<TimeProvider>().GetUtcNow()),
        }, CancellationToken.None);
        Assert.Equal(GoalCreateOutcome.Created, created.Outcome);
        return (tracked, untracked, browser);
    }

    [Fact]
    public async Task US6_AS2_tracked_true_shows_a_registered_user_only_the_habits_they_track()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tracked, untracked, signedIn) = await TrackedWorldAsync(suffix);
        using var browser = signedIn;

        var (status, html) = await browser.GetPageAsync($"/catalog?tracked=true&q={suffix}");

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains(tracked, html, StringComparison.Ordinal);
        Assert.DoesNotContain(untracked, html, StringComparison.Ordinal);
        Assert.Contains("Only habits I track", html, StringComparison.Ordinal);
        // The middle dot comes from an expression, which the host may write as a character reference.
        Assert.Matches("<span>1 habit (·|&#xB7;) you track 1</span>", html);
        Assert.Contains(">1 goal</span>", html, StringComparison.Ordinal);
        Assert.Contains(">Add another goal</a>", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HP_005_a_visitor_with_tracked_true_gets_slice_2s_list_unfiltered_and_no_Show_option()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tracked, untracked, signedIn) = await TrackedWorldAsync(suffix);
        signedIn.Dispose();
        using var browser = host.CreateBrowser();

        var (status, html) = await browser.GetPageAsync($"/catalog?tracked=true&q={suffix}");

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains(tracked, html, StringComparison.Ordinal);
        Assert.Contains(untracked, html, StringComparison.Ordinal);
        Assert.Contains("<span>2 habits</span>", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Only habits I track", html, StringComparison.Ordinal);
        Assert.DoesNotContain("you track", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Use this habit", html, StringComparison.Ordinal);
        Assert.DoesNotContain("1 goal", html, StringComparison.Ordinal);
        Assert.Contains("See details", html, StringComparison.Ordinal);
    }
}
