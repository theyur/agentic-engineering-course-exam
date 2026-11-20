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
/// HP-003, contracts/host.md "Signed-in Home": the prerendered signed-in Home through the real host. The first HTML
/// already has the tiles, the Today rows and the right column (research R-12), which the smoke check reads.
/// </summary>
[Collection(HostCollection.Name)]
public sealed class HomeDashboardHostTests(HostFixture host)
{
    private static string Email(string who) => $"ui-hd-{who}-{Guid.NewGuid():N}@example.com";

    [Fact]
    public async Task The_prerendered_Home_of_a_user_with_goals_has_the_tiles_the_Today_rows_and_Active_goals()
    {
        // Six goals of 15 min every day, in UTC at the host's clock: "… 1" from 4 days ago with 15 min on its first day
        // (Met, then Missed this week, its grace period having ended at 00:00 today, then two Pending, today Due), and five
        // more from today (Due). No note.
        var prefix = $"Ui-Hd {Guid.NewGuid():N}"[..19];
        var (browser, account) = await host.SignedInAccountAsync(Email("owner"), Role.RegisteredUser);
        using (browser)
        {
            await using (var scope = host.Services.CreateAsyncScope())
            {
                var services = scope.ServiceProvider;
                var today = NamedTimeZone.Utc.DateAt(services.GetRequiredService<TimeProvider>().GetUtcNow());
                await StoreGoalAsync(services, account, prefix + " 1", today.AddDays(-4), recordFirstDay: true);
                for (var n = 2; n <= 6; n++)
                {
                    await StoreGoalAsync(services, account, prefix + " " + n, today, recordFirstDay: false);
                }
            }

            var (status, html) = await browser.GetPageAsync("/");

            Assert.Equal(HttpStatusCode.OK, status);
            var text = WebUtility.HtmlDecode(html);
            var home = text.IndexOf("<div class=\"ht-home\">", StringComparison.Ordinal);
            var panel = text.IndexOf("<div class=\"ht-home__panel\">", StringComparison.Ordinal);
            var right = text.IndexOf("<div class=\"ht-stack\">", StringComparison.Ordinal);
            Assert.True(home >= 0 && panel > home && right > panel, "The Home has no ht-home grid with the panel before the right column.");
            foreach (var (value, label) in new[] { ("6", "Due now"), ("2", "Pending · still can record"), ("1", "Missed this week"), ("6", "Active goals") })
            {
                Assert.Contains($"<span class=\"ht-tile__value\">{value}</span><span class=\"ht-tile__label\">{label}</span>", text, StringComparison.Ordinal);
            }

            Assert.Contains("<h2 class=\"ht-card__title\">Today</h2>", text, StringComparison.Ordinal);
            Assert.Equal(2, Count(text, "<span class=\"ht-chip ht-status ht-status--pending\">Pending</span>"));
            Assert.Equal(6, Count(text, "<span class=\"ht-chip ht-status ht-status--due\">Due today</span>"));
            Assert.Equal(1, Count(text, "<span class=\"ht-chip ht-status ht-status--missed\">Missed</span>"));
            Assert.Equal(9, Count(text, "<div class=\"ht-row\">"));
            Assert.Contains("<h2 class=\"ht-card__title\">Active goals</h2>", text, StringComparison.Ordinal);
            Assert.Equal(5, Count(text, "<a class=\"ht-goal__name\""));
            Assert.Contains("<a class=\"ht-card__link\" href=\"/goals\">All goals →</a>", text, StringComparison.Ordinal);
            Assert.DoesNotContain("Recent notes", text, StringComparison.Ordinal);
            Assert.DoesNotContain("No goals yet", text, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task A_user_with_no_goals_gets_the_EmptyState_and_no_tile()
    {
        using var browser = await host.SignedInBrowserAsync(Email("empty"), Role.RegisteredUser);

        var (status, html) = await browser.GetPageAsync("/");

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains("No goals yet", html, StringComparison.Ordinal);
        Assert.DoesNotContain("ht-tile", html, StringComparison.Ordinal);
        Assert.DoesNotContain("ht-home", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task An_administrator_gets_no_tile()
    {
        using var browser = await host.SignedInBrowserAsync(Email("admin"), Role.Administrator);

        var (status, html) = await browser.GetPageAsync("/");

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains("class=\"ht-page-head\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("ht-tile", html, StringComparison.Ordinal);
        Assert.DoesNotContain("ht-home", html, StringComparison.Ordinal);
        Assert.DoesNotContain("No goals yet", html, StringComparison.Ordinal);
    }

    private static int Count(string text, string part) => text.Split(part).Length - 1;

    /// <summary>
    /// A goal of 15 min on every day of <paramref name="account"/>, stored through the host's goal store on a new
    /// Published catalog habit named <paramref name="habitName"/>, from <paramref name="first"/> in UTC, with 15 min
    /// recorded on that day when <paramref name="recordFirstDay"/> is set.
    /// </summary>
    private async Task StoreGoalAsync(IServiceProvider services, AccountId account, string habitName, DateOnly first, bool recordFirstDay)
    {
        var habit = await host.ArrangeCatalogHabitAsync(Definition(habitName));
        Assert.True(GoalTarget.TryCreate(new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = 15, DurationUnit = DurationUnit.Minutes,
            ScheduleKind = ScheduleKind.FixedDays, Days = Enum.GetValues<DayOfWeek>().ToHashSet(),
        }, out var target, out _));
        var created = await services.GetRequiredService<IGoalStore>().CreateAsync(
            account, new NewGoal(new GoalHabit(habit, HabitKind.Catalog), first, null, target), NamedTimeZone.Utc, NamedTimeZone.Utc, CancellationToken.None);
        Assert.Equal(GoalWriteOutcomeKind.Created, created.Kind);
        if (recordFirstDay)
        {
            var written = await services.GetRequiredService<IRecordStore>().AddAsync(
                account, created.Id!.Value, new EntrySpan(first, first, first), true, first, RecordValue.FromAmount(15), NoteWrite.Keep,
                RecordLimits.MaxRecordsPerPeriod, CancellationToken.None);
            Assert.Equal(RecordStoreWriteOutcome.Written, written.Outcome);
        }
    }
}
