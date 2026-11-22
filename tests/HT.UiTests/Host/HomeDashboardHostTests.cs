using System.Net;
using HT.Application.Accounts;
using HT.Application.Exemptions;
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
    public async Task Slice_7_HP_005_the_prerendered_Home_has_Excuse_on_a_Pending_commitment_row_and_not_on_the_Due_one()
    {
        // A goal of 15 min every day from yesterday (UTC, at the host's clock): yesterday is Pending (its grace period ends
        // 48 hours after 00:00 today) with no record, today is Due.
        var habitName = $"Ui-Hd Excuse {Guid.NewGuid():N}"[..28];
        var (browser, account) = await host.SignedInAccountAsync(Email("excuse"), Role.RegisteredUser);
        using (browser)
        {
            DateOnly today;
            await using (var scope = host.Services.CreateAsyncScope())
            {
                var services = scope.ServiceProvider;
                today = NamedTimeZone.Utc.DateAt(services.GetRequiredService<TimeProvider>().GetUtcNow());
                await StoreGoalAsync(services, account, habitName, today.AddDays(-1), recordFirstDay: false);
            }

            var (status, html) = await browser.GetPageAsync("/");

            Assert.Equal(HttpStatusCode.OK, status);
            var text = WebUtility.HtmlDecode(html);
            var name = habitName + ", 15 min daily";
            Assert.Equal(1, Count(text, ">Excuse</button>"));
            Assert.Contains($"aria-label=\"Excuse {name}, {DateTexts.ShortDate(today.AddDays(-1))}\"", text, StringComparison.Ordinal);
            Assert.DoesNotContain($"aria-label=\"Excuse {name}, {DateTexts.ShortDate(today)}\"", text, StringComparison.Ordinal);
            Assert.Contains($"aria-label=\"Record for {name}, {DateTexts.ShortDate(today)}\"", text, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task Slice_7_HP_005_the_prerendered_Home_leaves_a_paused_goal_out_of_the_Active_goals_tile_and_card()
    {
        // Two goals of 15 min every day from today (UTC, at the host's clock; today Due): "… P" paused through the host's
        // PauseService from today, the pause date, for 7 days; "… A" not paused. The tile counts 1, the card lists only "… A",
        // and "… P" has no Today row.
        var prefix = $"Ui-Hd Paused {Guid.NewGuid():N}"[..26];
        var (browser, account) = await host.SignedInAccountAsync(Email("paused"), Role.RegisteredUser);
        using (browser)
        {
            GoalId active, paused;
            await using (var scope = host.Services.CreateAsyncScope())
            {
                var services = scope.ServiceProvider;
                var today = NamedTimeZone.Utc.DateAt(services.GetRequiredService<TimeProvider>().GetUtcNow());
                active = await StoreGoalAsync(services, account, prefix + " A", today, recordFirstDay: false);
                paused = await StoreGoalAsync(services, account, prefix + " P", today, recordFirstDay: false);
                var result = await services.GetRequiredService<PauseService>().PauseAsync(
                    ActingUser.ForAccount(account, Role.RegisteredUser), paused, today, today.AddDays(6), null, CancellationToken.None);
                Assert.Equal(PauseOutcome.Paused, result.Outcome);
            }

            var (status, html) = await browser.GetPageAsync("/");

            Assert.Equal(HttpStatusCode.OK, status);
            var text = WebUtility.HtmlDecode(html);
            foreach (var (value, label) in new[] { ("1", "Due now"), ("0", "Pending · still can record"), ("0", "Missed this week"), ("1", "Active goals") })
            {
                Assert.Contains($"<span class=\"ht-tile__value\">{value}</span><span class=\"ht-tile__label\">{label}</span>", text, StringComparison.Ordinal);
            }

            Assert.Equal(1, Count(text, "<div class=\"ht-row\">"));
            Assert.Equal(1, Count(text, "<a class=\"ht-goal__name\""));
            Assert.Contains($"<a class=\"ht-goal__name\" href=\"/goals/{active.Value}\">{prefix} A, 15 min daily</a>", text, StringComparison.Ordinal);
            Assert.DoesNotContain($"/goals/{paused.Value}", text, StringComparison.Ordinal);
            Assert.DoesNotContain($"{prefix} P", text, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task Slice_8_HP_007_the_prerendered_Home_keeps_the_Pending_rows_of_ended_attempts_with_their_Tags()
    {
        // Three goals of 15 min every day from yesterday (UTC, at the host's clock; yesterday Pending, today Due): "… E" ended
        // today through the host's GoalChangeService (last date yesterday, still Pending: "Ended"); "… R" ended today and
        // restarted from tomorrow (its attempt 1's yesterday: "Attempt 1", its attempt 2's tomorrow Upcoming without a Tag);
        // "… A" not ended. "Pending · still can record"
        // counts the three yesterdays, "Due now" A's today, and "Active goals" A and R; the card lists only those two.
        var prefix = $"Ui-Hd Ended {Guid.NewGuid():N}"[..25];
        var (browser, account) = await host.SignedInAccountAsync(Email("ended"), Role.RegisteredUser);
        using (browser)
        {
            GoalId active, ended, restarted;
            await using (var scope = host.Services.CreateAsyncScope())
            {
                var services = scope.ServiceProvider;
                var user = ActingUser.ForAccount(account, Role.RegisteredUser);
                var today = NamedTimeZone.Utc.DateAt(services.GetRequiredService<TimeProvider>().GetUtcNow());
                active = await StoreGoalAsync(services, account, prefix + " A", today.AddDays(-1), recordFirstDay: false);
                ended = await StoreGoalAsync(services, account, prefix + " E", today.AddDays(-1), recordFirstDay: false);
                restarted = await StoreGoalAsync(services, account, prefix + " R", today.AddDays(-1), recordFirstDay: false);
                var changes = services.GetRequiredService<GoalChangeService>();
                Assert.Equal(GoalEndOutcome.Ended, (await changes.EndAsync(user, ended, Outcome.Abandoned, null, CancellationToken.None)).Outcome);
                Assert.Equal(GoalEndOutcome.Ended, (await changes.EndAsync(user, restarted, Outcome.Abandoned, null, CancellationToken.None)).Outcome);
                var started = await changes.StartAttemptAsync(user, restarted, new NewAttemptInput
                {
                    Target = new GoalTargetFields
                    {
                        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = 15, DurationUnit = DurationUnit.Minutes,
                        ScheduleKind = ScheduleKind.FixedDays, Days = Enum.GetValues<DayOfWeek>().ToHashSet(),
                    },
                    StartDate = today.AddDays(1),
                }, CancellationToken.None);
                Assert.Equal(AttemptStartOutcome.Started, started.Outcome);
            }

            var (status, html) = await browser.GetPageAsync("/");

            Assert.Equal(HttpStatusCode.OK, status);
            var text = WebUtility.HtmlDecode(html);
            foreach (var (value, label) in new[] { ("1", "Due now"), ("3", "Pending · still can record"), ("0", "Missed this week"), ("2", "Active goals") })
            {
                Assert.Contains($"<span class=\"ht-tile__value\">{value}</span><span class=\"ht-tile__label\">{label}</span>", text, StringComparison.Ordinal);
            }

            // The three Pending yesterdays, A's Due today, and R's attempt 2 Upcoming tomorrow (a row of the latest attempt,
            // with no Tag).
            Assert.Equal(5, Count(text, "<div class=\"ht-row\">"));
            Assert.Equal(1, Count(text, $"<div class=\"ht-row__name\">{prefix} R, 15 min daily</div>"));
            Assert.Contains($"<div class=\"ht-row__name\">{prefix} E, 15 min daily <span class=\"ht-chip\">Ended</span></div>", text, StringComparison.Ordinal);
            Assert.Contains($"<div class=\"ht-row__name\">{prefix} R, 15 min daily <span class=\"ht-chip\">Attempt 1</span></div>", text, StringComparison.Ordinal);
            Assert.Equal(2, Count(text, $"<div class=\"ht-row__name\">{prefix} A, 15 min daily</div>"));
            Assert.Equal(2, Count(text, "<a class=\"ht-goal__name\""));
            Assert.Contains($"<a class=\"ht-goal__name\" href=\"/goals/{active.Value}\">", text, StringComparison.Ordinal);
            Assert.Contains($"<a class=\"ht-goal__name\" href=\"/goals/{restarted.Value}\">", text, StringComparison.Ordinal);
            Assert.DoesNotContain($"<a class=\"ht-goal__name\" href=\"/goals/{ended.Value}\">", text, StringComparison.Ordinal);
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
    /// recorded on that day when <paramref name="recordFirstDay"/> is set. Returns the goal's identifier.
    /// </summary>
    private async Task<GoalId> StoreGoalAsync(IServiceProvider services, AccountId account, string habitName, DateOnly first, bool recordFirstDay)
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

        return created.Id!.Value;
    }
}
