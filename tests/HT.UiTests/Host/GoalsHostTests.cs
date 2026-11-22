using System.Net;
using HT.Application.Accounts;
using HT.Application.Exemptions;
using HT.Application.Goals;
using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Exemptions;
using HT.Domain.Records;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Time;
using HT.UnitTests.Fakes;
using Microsoft.Extensions.DependencyInjection;
using static HT.UnitTests.Fakes.HabitSamples;

namespace HT.UiTests.Host;

/// <summary>
/// contracts/host.md "Routes", HP-001, FR-021: who reaches the goal pages through the real host. US2 adds
/// <c>/goals</c> and <c>/goals/{id}</c>.
/// </summary>
[Collection(HostCollection.Name)]
public sealed class GoalsHostTests(HostFixture host)
{
    private const string NewGoalAddress = "/goals/new";
    private const string GoalsAddress = "/goals";

    /// <summary>The zones of the per-week pause test, in the order it tries them: UTC, then UTC+14, then UTC−11.</summary>
    private static readonly string[] WeekGoalZones = ["UTC", "Pacific/Kiritimati", "Pacific/Pago_Pago"];

    private static string Email(string who) => $"ui-go-{who}-{Guid.NewGuid():N}@example.com";

    [Fact]
    public async Task A_signed_out_visitor_is_sent_to_sign_in_from_the_New_goal_page()
    {
        using var browser = host.CreateBrowser();

        using var response = await browser.GetAsync(new Uri(NewGoalAddress, UriKind.Relative));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("https://localhost/sign-in?returnUrl=" + Uri.EscapeDataString(NewGoalAddress), response.RedirectTarget(browser));
        Assert.DoesNotContain("<h1>New goal</h1>", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task An_administrator_gets_the_Not_found_page_for_the_New_goal_page()
    {
        using var browser = await host.SignedInBrowserAsync(Email("admin"), Role.Administrator);

        var (status, html) = await browser.GetPageAsync(NewGoalAddress);

        Assert.Equal(HttpStatusCode.NotFound, status);
        Assert.Contains("Page not found", html, StringComparison.Ordinal);
        Assert.DoesNotContain("<h1>New goal</h1>", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Create goal", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_registered_user_gets_the_New_goal_page()
    {
        using var browser = await host.SignedInBrowserAsync(Email("user"), Role.RegisteredUser);

        var (status, html) = await browser.GetPageAsync(NewGoalAddress);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains("<h1>New goal</h1>", html, StringComparison.Ordinal);
        Assert.Contains("Start from a catalog habit or one of your custom habits.", html, StringComparison.Ordinal);
        Assert.Contains(">Choose a habit</button>", html, StringComparison.Ordinal);
        Assert.Contains("Complete the form to see your goal here.", html, StringComparison.Ordinal);
        Assert.Contains(">Create goal</button>", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_registered_user_gets_the_New_goal_page_from_a_catalog_habit()
    {
        var name = $"Ui-Go Reading {Guid.NewGuid():N}"[..30];
        var habit = await host.ArrangeCatalogHabitAsync(Definition(name), goals: HabitSamples.Goals(0, DurationDays(15, DurationUnit.Minutes, Weekdays)));
        using var browser = await host.SignedInBrowserAsync(Email("habit"), Role.RegisteredUser);

        var (status, html) = await browser.GetPageAsync($"{NewGoalAddress}?habit={habit.Value}&kind=catalog");

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains($"Based on the catalog habit {name}. Change anything you like.", html, StringComparison.Ordinal);
        Assert.Contains($"href=\"/catalog/{habit.Value}\"", html, StringComparison.Ordinal);
        Assert.Contains($"{name}: at least 15 min, 5 days a week, starting ", html, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(GoalsAddress)]
    [InlineData("/goals/6f9a3c1e-0000-4000-8000-0000000000aa")]
    public async Task A_signed_out_visitor_is_sent_to_sign_in_from_Goals_and_a_goal_page(string address)
    {
        using var browser = host.CreateBrowser();

        using var response = await browser.GetAsync(new Uri(address, UriKind.Relative));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("https://localhost/sign-in?returnUrl=" + Uri.EscapeDataString(address), response.RedirectTarget(browser));
    }

    [Fact]
    public async Task An_administrator_gets_the_Not_found_page_for_Goals_and_for_a_goal_page()
    {
        var (owner, goal, name) = await OwnerWithGoalAsync("admin-owner");
        owner.Dispose();
        using var browser = await host.SignedInBrowserAsync(Email("admin"), Role.Administrator);

        foreach (var address in new[] { GoalsAddress, $"{GoalsAddress}/{goal.Value}" })
        {
            var (status, html) = await browser.GetPageAsync(address);

            Assert.Equal(HttpStatusCode.NotFound, status);
            Assert.Contains("Page not found", html, StringComparison.Ordinal);
            Assert.DoesNotContain("<h1>Goals</h1>", html, StringComparison.Ordinal);
            Assert.DoesNotContain(name, html, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task The_owner_gets_Goals_with_the_goal_and_the_goals_page()
    {
        var (browser, goal, name) = await OwnerWithGoalAsync("owner");
        using (browser)
        {
            var (listStatus, list) = await browser.GetPageAsync(GoalsAddress);
            var (pageStatus, page) = await browser.GetPageAsync($"{GoalsAddress}/{goal.Value}");

            Assert.Equal(HttpStatusCode.OK, listStatus);
            Assert.Contains("<h1>Goals</h1>", list, StringComparison.Ordinal);
            Assert.Contains($"<a class=\"ht-goalitem__name\" href=\"/goals/{goal.Value}\">{name}, 15 min, 5 days a week</a>", list, StringComparison.Ordinal);
            Assert.Equal(HttpStatusCode.OK, pageStatus);
            Assert.Contains($"<h1>{name}, 15 min, 5 days a week</h1>", page, StringComparison.Ordinal);
            Assert.Contains("Changes apply from the next commitment. Past results keep their rules.", page, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task A_user_with_no_goals_gets_the_EmptyState()
    {
        using var browser = await host.SignedInBrowserAsync(Email("empty"), Role.RegisteredUser);

        var (status, html) = await browser.GetPageAsync(GoalsAddress);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains("No goals yet", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task An_unknown_goal_and_another_accounts_goal_get_the_Not_found_page()
    {
        var (owner, goal, name) = await OwnerWithGoalAsync("other-owner");
        owner.Dispose();
        using var browser = await host.SignedInBrowserAsync(Email("other"), Role.RegisteredUser);

        foreach (var address in new[] { $"{GoalsAddress}/{Guid.NewGuid()}", $"{GoalsAddress}/{goal.Value}" })
        {
            var (status, html) = await browser.GetPageAsync(address);

            Assert.Equal(HttpStatusCode.NotFound, status);
            Assert.Contains("Page not found", html, StringComparison.Ordinal);
            Assert.DoesNotContain(name, html, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task Slice_5_the_owners_prerendered_goal_page_has_Record_and_other_accounts_and_administrators_still_get_404()
    {
        var (owner, goal, name) = await OwnerWithGoalAsync("record-owner", everyDay: true);
        using (owner)
        {
            var (status, html) = await owner.GetPageAsync($"{GoalsAddress}/{goal.Value}");

            Assert.Equal(HttpStatusCode.OK, status);
            Assert.Contains($"<h1>{name}, 15 min daily</h1>", html, StringComparison.Ordinal);
            Assert.Contains("aria-haspopup=\"dialog\">Record</button>", html, StringComparison.Ordinal);
        }

        using var other = await host.SignedInBrowserAsync(Email("record-other"), Role.RegisteredUser);
        using var administrator = await host.SignedInBrowserAsync(Email("record-admin"), Role.Administrator);
        foreach (var browser in new[] { other, administrator })
        {
            var (status, html) = await browser.GetPageAsync($"{GoalsAddress}/{goal.Value}");

            Assert.Equal(HttpStatusCode.NotFound, status);
            Assert.Contains("Page not found", html, StringComparison.Ordinal);
            Assert.DoesNotContain(name, html, StringComparison.Ordinal);
            Assert.DoesNotContain(">Record</button>", html, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task Slice_5_the_owners_prerendered_goal_page_has_the_History_card_with_its_first_rows_and_StatusChips()
    {
        // The goal starts 4 days before today (UTC, at the host's clock), every day, with 15 min recorded on its first
        // day: that day is final and Met (its grace period ended at 00:00 yesterday), today is Due, and the days between
        // are Pending or Missed. Plan "Risks": the prerendered HTML is what the smoke check and the first paint see.
        var name = $"Ui-Go History {Guid.NewGuid():N}"[..30];
        var habit = await host.ArrangeCatalogHabitAsync(Definition(name));
        var (browser, account) = await host.SignedInAccountAsync(Email("history-owner"), Role.RegisteredUser);
        using (browser)
        {
            DateOnly first;
            GoalId goal;
            await using (var scope = host.Services.CreateAsyncScope())
            {
                var services = scope.ServiceProvider;
                first = NamedTimeZone.Utc.DateAt(services.GetRequiredService<TimeProvider>().GetUtcNow()).AddDays(-4);
                Assert.True(GoalTarget.TryCreate(new GoalTargetFields
                {
                    BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = 15, DurationUnit = DurationUnit.Minutes,
                    ScheduleKind = ScheduleKind.FixedDays, Days = Enum.GetValues<DayOfWeek>().ToHashSet(),
                }, out var target, out _));
                var created = await services.GetRequiredService<IGoalStore>().CreateAsync(
                    account, new NewGoal(new GoalHabit(habit, HabitKind.Catalog), first, null, target), NamedTimeZone.Utc, NamedTimeZone.Utc, CancellationToken.None);
                Assert.Equal(GoalWriteOutcomeKind.Created, created.Kind);
                goal = created.Id!.Value;
                var written = await services.GetRequiredService<IRecordStore>().AddAsync(
                    account, goal, new EntrySpan(first, first, first), true, first, RecordValue.FromAmount(15), NoteWrite.Keep, RecordLimits.MaxRecordsPerPeriod, CancellationToken.None);
                Assert.Equal(RecordStoreWriteOutcome.Written, written.Outcome);
            }

            var (status, html) = await browser.GetPageAsync($"{GoalsAddress}/{goal.Value}");

            Assert.Equal(HttpStatusCode.OK, status);
            Assert.Contains("<h2 class=\"ht-card__title\">History</h2>", html, StringComparison.Ordinal);
            Assert.Contains("Corrections are evaluated with the rules of that date.", html, StringComparison.Ordinal);
            Assert.Contains($"<td><b>{DateTexts.ShortDate(first)}</b></td>", html, StringComparison.Ordinal);
            Assert.Contains($"<td><b>{DateTexts.ShortDate(first.AddDays(1))}</b></td>", html, StringComparison.Ordinal);
            Assert.Contains("<td>15 min</td>", html, StringComparison.Ordinal);
            Assert.Contains("<span class=\"ht-chip ht-status ht-status--met\">Met</span>", html, StringComparison.Ordinal);
            Assert.Contains("<span class=\"ht-chip ht-status ht-status--due\">Due today</span>", html, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task Slice_7_HP_001_the_owners_prerendered_goal_page_has_an_Excused_History_row_with_its_reason_and_Remove_excusal()
    {
        // Every day from yesterday (UTC, at the host's clock); yesterday, Pending with no record, is excused with "Ill"
        // through the host's ExcusalService, and its grace period is still open, so the excusal can be removed.
        var name = $"Ui-Go Excused {Guid.NewGuid():N}"[..30];
        var (browser, account) = await host.SignedInAccountAsync(Email("excused-owner"), Role.RegisteredUser);
        using (browser)
        {
            DateOnly yesterday;
            GoalId goal;
            await using (var scope = host.Services.CreateAsyncScope())
            {
                var services = scope.ServiceProvider;
                yesterday = NamedTimeZone.Utc.DateAt(services.GetRequiredService<TimeProvider>().GetUtcNow()).AddDays(-1);
                goal = await StoreGoalAsync(services, account, name, Enum.GetValues<DayOfWeek>(), yesterday, recordFirstDay: false);
                var excused = await services.GetRequiredService<ExcusalService>().ExcuseAsync(
                    ActingUser.ForAccount(account, Role.RegisteredUser), goal, yesterday, "Ill", CancellationToken.None);
                Assert.Equal(ExcuseOutcome.Excused, excused.Outcome);
            }

            var (status, html) = await browser.GetPageAsync($"{GoalsAddress}/{goal.Value}");

            Assert.Equal(HttpStatusCode.OK, status);
            var text = WebUtility.HtmlDecode(html);
            Assert.Contains(
                "<span class=\"ht-inline\"><span class=\"ht-chip ht-status ht-status--excused\">Excused</span><span class=\"ht-muted\">Ill</span></span>",
                text, StringComparison.Ordinal);
            Assert.Contains($"aria-label=\"Remove excusal of {DateTexts.ShortDate(yesterday)}\"", text, StringComparison.Ordinal);
            Assert.Contains(">Remove excusal</button>", text, StringComparison.Ordinal);
            Assert.DoesNotContain($"aria-label=\"Add record for {DateTexts.ShortDate(yesterday)}\"", text, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task Slice_7_HP_001_the_owners_prerendered_page_of_a_paused_goal_has_the_Notice_the_Pauses_card_and_no_Pause()
    {
        // Every day from yesterday (UTC, at the host's clock), paused through the host's PauseService from today, the
        // pause date, for 7 days: the pause is current, so the goal is paused until today + 6.
        var name = $"Ui-Go Paused {Guid.NewGuid():N}"[..30];
        var (browser, account) = await host.SignedInAccountAsync(Email("paused-owner"), Role.RegisteredUser);
        using (browser)
        {
            DateOnly today;
            GoalId goal;
            await using (var scope = host.Services.CreateAsyncScope())
            {
                var services = scope.ServiceProvider;
                today = NamedTimeZone.Utc.DateAt(services.GetRequiredService<TimeProvider>().GetUtcNow());
                goal = await StoreGoalAsync(services, account, name, Enum.GetValues<DayOfWeek>(), today.AddDays(-1), recordFirstDay: false);
                var paused = await services.GetRequiredService<PauseService>().PauseAsync(
                    ActingUser.ForAccount(account, Role.RegisteredUser), goal, today, today.AddDays(6), "Family visit", CancellationToken.None);
                Assert.Equal(PauseOutcome.Paused, paused.Outcome);
            }

            var (status, html) = await browser.GetPageAsync($"{GoalsAddress}/{goal.Value}");

            Assert.Equal(HttpStatusCode.OK, status);
            var text = WebUtility.HtmlDecode(html);
            Assert.Contains("<div class=\"ht-notice\">", text, StringComparison.Ordinal);
            Assert.Contains($"<span>Paused until {DateTexts.ShortDate(today.AddDays(6))}</span>", text, StringComparison.Ordinal);
            Assert.Contains(">Resume now</button>", text, StringComparison.Ordinal);
            Assert.DoesNotContain(">Pause</button>", text, StringComparison.Ordinal);
            Assert.Contains("<h2 class=\"ht-card__title\">Pauses</h2>", text, StringComparison.Ordinal);
            Assert.Contains(
                $"<span class=\"ht-goal__name\">{DateTexts.ShortDate(today)} – {DateTexts.ShortDate(today.AddDays(6))}</span>", text, StringComparison.Ordinal);
            Assert.Contains("<div class=\"ht-goal__meta ht-muted\">Family visit</div>", text, StringComparison.Ordinal);
            Assert.Contains("<span class=\"ht-chip ht-status ht-status--paused\">Paused</span>", text, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task Slice_7_Edge_a_per_week_goal_with_an_upcoming_pause_later_in_its_current_week_is_not_paused_but_its_week_is()
    {
        // A per-week goal whose current week has a date after today: its zone is the first of UTC, Kiritimati (UTC+14) and
        // Pago Pago (UTC−11) whose date at the host's clock is not a Sunday (at most one of the three is), and it starts on
        // that week's Monday; the account is moved to that zone first through the host's ProfileService, so the goal store
        // writes the goal's version in it. A pause of tomorrow, set through the host's PauseService, is upcoming, so the
        // goal is not paused (no Notice, "Pause" offered), while its current week has a paused date and is Paused as a
        // whole (D-1).
        var name = $"Ui-Go Week {Guid.NewGuid():N}"[..30];
        var habit = await host.ArrangeCatalogHabitAsync(Definition(name));
        var (browser, account) = await host.SignedInAccountAsync(Email("week-owner"), Role.RegisteredUser);
        using (browser)
        {
            DateOnly monday;
            DateOnly tomorrow;
            await using (var scope = host.Services.CreateAsyncScope())
            {
                var services = scope.ServiceProvider;
                var now = services.GetRequiredService<TimeProvider>().GetUtcNow();
                var (zoneId, zone) = WeekGoalZones
                    .Select(id => (Id: id, Zone: NamedTimeZone.TryParse(id, out var parsed) ? parsed : throw new InvalidOperationException(id)))
                    .First(candidate => candidate.Zone.DateAt(now).DayOfWeek != DayOfWeek.Sunday);
                var moved = await services.GetRequiredService<ProfileService>().ChangeTimeZoneAsync(
                    ActingUser.ForAccount(account, Role.RegisteredUser), zoneId, CancellationToken.None);
                Assert.Contains(moved.Outcome, new[] { TimeZoneChangeOutcome.Changed, TimeZoneChangeOutcome.Unchanged });
                var today = zone.DateAt(now);
                monday = today.AddDays(-(((int)today.DayOfWeek + 6) % 7));
                tomorrow = today.AddDays(1);
                Assert.True(GoalTarget.TryCreate(new GoalTargetFields
                {
                    BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo, ScheduleKind = ScheduleKind.Times, Times = 3,
                    Period = SchedulePeriod.Week,
                }, out var target, out _));
                var created = await services.GetRequiredService<IGoalStore>().CreateAsync(
                    account, new NewGoal(new GoalHabit(habit, HabitKind.Catalog), monday, null, target), zone, NamedTimeZone.Utc, CancellationToken.None);
                Assert.Equal(GoalWriteOutcomeKind.Created, created.Kind);
                var paused = await services.GetRequiredService<PauseService>().PauseAsync(
                    ActingUser.ForAccount(account, Role.RegisteredUser), created.Id!.Value, tomorrow, tomorrow, null, CancellationToken.None);
                Assert.Equal(PauseOutcome.Paused, paused.Outcome);
                Assert.Equal(PauseTiming.Upcoming, paused.Pause!.Timing);

                var (status, html) = await browser.GetPageAsync($"{GoalsAddress}/{created.Id.Value.Value}");

                Assert.Equal(HttpStatusCode.OK, status);
                var text = WebUtility.HtmlDecode(html);
                Assert.DoesNotContain("<div class=\"ht-notice\">", text, StringComparison.Ordinal);
                Assert.DoesNotContain("Paused until", text, StringComparison.Ordinal);
                Assert.Contains("aria-haspopup=\"dialog\">Pause</button>", text, StringComparison.Ordinal);
                Assert.Contains(
                    $"<td><b>{DateTexts.WeekRange(monday, monday.AddDays(6))}</b></td>", text, StringComparison.Ordinal);
                Assert.Contains("<span class=\"ht-chip ht-status ht-status--paused\">Paused</span>", text, StringComparison.Ordinal);
                Assert.Contains(">Cancel pause</button>", text, StringComparison.Ordinal);
            }
        }
    }

    [Fact]
    public async Task Slice_7_US3_the_owners_prerendered_goal_page_has_Excused_and_Paused_cells_and_marked_bars()
    {
        // Every day from 2 days before today (UTC, at the host's clock), 15 min recorded on its first day. Yesterday,
        // Pending with no record, is excused through the host's ExcusalService, and the goal is paused through its
        // PauseService from today, the pause date, until tomorrow. Then the host's clock moves forward 3 days, as other
        // host tests move it, so every date below is taken from the clock first: the first day is Met, the excused day
        // Excused, the two paused days Paused and ended, the next day Pending and the new today Due. The browser signs
        // in after the move.
        var name = $"Ui-Go Exempt {Guid.NewGuid():N}"[..30];
        var email = Email("exempt-owner");
        var account = (await host.CreateAccountAsync(email, "walking42")).Id;
        GoalId goal;
        DateOnly first;
        await using (var scope = host.Services.CreateAsyncScope())
        {
            var services = scope.ServiceProvider;
            var today = NamedTimeZone.Utc.DateAt(services.GetRequiredService<TimeProvider>().GetUtcNow());
            first = today.AddDays(-2);
            goal = await StoreGoalAsync(services, account, name, Enum.GetValues<DayOfWeek>(), first, recordFirstDay: true);
            var user = ActingUser.ForAccount(account, Role.RegisteredUser);
            var excused = await services.GetRequiredService<ExcusalService>().ExcuseAsync(user, goal, today.AddDays(-1), null, CancellationToken.None);
            Assert.Equal(ExcuseOutcome.Excused, excused.Outcome);
            var paused = await services.GetRequiredService<PauseService>().PauseAsync(user, goal, today, today.AddDays(1), null, CancellationToken.None);
            Assert.Equal(PauseOutcome.Paused, paused.Outcome);
        }

        host.Clock.Advance(TimeSpan.FromDays(3));
        using var browser = host.CreateBrowser();
        await browser.SignInAsync(email, "walking42");

        var (status, html) = await browser.GetPageAsync($"{GoalsAddress}/{goal.Value}");

        Assert.Equal(HttpStatusCode.OK, status);
        var text = WebUtility.HtmlDecode(html);
        Assert.Contains("<div class=\"ht-stat__value ht-stat__value--text\">100% (1 of 1)</div>", text, StringComparison.Ordinal);
        Assert.Contains("aria-label=\"Last 5 commitments: 1 met, 1 excused, 2 paused, 1 pending\"", text, StringComparison.Ordinal);
        Assert.Equal(1, text.Split("class=\"ht-strip__cell ht-cell--excused\"").Length - 2);
        Assert.Equal(2, text.Split("class=\"ht-strip__cell ht-cell--paused\"").Length - 2);
        Assert.Contains("<div class=\"ht-bars\" role=\"img\" aria-label=\"Minutes per day, last 14 days; target 15; 1 excused, 2 paused\">", text, StringComparison.Ordinal);
        Assert.Contains($"<i class=\"ht-bar ht-bar--excused\" title=\"{DateTexts.ShortDate(first.AddDays(1))}: Excused\"></i>", text, StringComparison.Ordinal);
        Assert.Contains($"<i class=\"ht-bar ht-bar--paused\" title=\"{DateTexts.ShortDate(first.AddDays(2))}: Paused\"></i>", text, StringComparison.Ordinal);
        Assert.Contains($"<i class=\"ht-bar ht-bar--paused\" title=\"{DateTexts.ShortDate(first.AddDays(3))}: Paused\"></i>", text, StringComparison.Ordinal);
        Assert.Equal(2, text.Split("class=\"ht-bar ht-bar--empty\"").Length - 1);
    }

    [Fact]
    public async Task Slice_6_the_owners_prerendered_goal_page_has_the_GoalStats_card_above_the_History_card()
    {
        // Every day from 4 days before today (UTC, at the host's clock), with 15 min on the first day: the first day is
        // Met and the second Missed (its grace period ended at 00:00 today); the next two are Pending, today is Due.
        var (browser, goal) = await OwnerWithEveryDayGoalAsync("stats-owner", startInDays: -4, recordFirstDay: true);
        using (browser)
        {
            var (status, html) = await browser.GetPageAsync($"{GoalsAddress}/{goal.Value}");

            Assert.Equal(HttpStatusCode.OK, status);
            var stats = html.IndexOf("<h2 class=\"ht-card__title\">Consistency and streaks</h2>", StringComparison.Ordinal);
            var history = html.IndexOf("<h2 class=\"ht-card__title\">History</h2>", StringComparison.Ordinal);
            Assert.True(stats >= 0, "The GoalStats card is missing.");
            Assert.True(history > stats, "The GoalStats card is not above the History card.");
            Assert.Contains("<div class=\"ht-eyebrow\">This attempt</div>", html, StringComparison.Ordinal);
            Assert.Contains("<div class=\"ht-stat__value ht-stat__value--text\">50% (1 of 2)</div><div class=\"ht-stat__label\">Consistency</div>", html, StringComparison.Ordinal);
            Assert.Contains("<div class=\"ht-stat__label\">Streak, commitments</div>", html, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task Slice_6_US4_the_owners_prerendered_goal_page_has_the_TrendChart_card_between_GoalStats_and_History()
    {
        // As above: 15 min on the first of 5 begun commitments, so 1 bar of 15 against 15 and 4 empty bars.
        var (browser, goal) = await OwnerWithEveryDayGoalAsync("trend-owner", startInDays: -4, recordFirstDay: true);
        using (browser)
        {
            var (status, html) = await browser.GetPageAsync($"{GoalsAddress}/{goal.Value}");

            Assert.Equal(HttpStatusCode.OK, status);
            var stats = html.IndexOf("<h2 class=\"ht-card__title\">Consistency and streaks</h2>", StringComparison.Ordinal);
            var trend = html.IndexOf("<h2 class=\"ht-card__title\">Minutes per day</h2>", StringComparison.Ordinal);
            var history = html.IndexOf("<h2 class=\"ht-card__title\">History</h2>", StringComparison.Ordinal);
            Assert.True(trend > stats && stats >= 0, "The TrendChart card is missing or not after the GoalStats card.");
            Assert.True(history > trend, "The TrendChart card is not above the History card.");
            Assert.Contains("<p class=\"ht-card__sub\">Last 14 days</p>", html, StringComparison.Ordinal);
            Assert.Contains("<div class=\"ht-bars\" role=\"img\" aria-label=\"Minutes per day, last 14 days; target 15\">", html, StringComparison.Ordinal);
            Assert.Contains("<span>Target 15 min</span>", html, StringComparison.Ordinal);
            Assert.Equal(4, html.Split("class=\"ht-bar ht-bar--empty\"").Length - 1);
        }
    }

    [Fact]
    public async Task Slice_6_a_goal_not_started_has_neither_the_GoalStats_nor_the_History_card()
    {
        var (browser, goal) = await OwnerWithEveryDayGoalAsync("stats-later", startInDays: 3, recordFirstDay: false);
        using (browser)
        {
            var (status, html) = await browser.GetPageAsync($"{GoalsAddress}/{goal.Value}");

            Assert.Equal(HttpStatusCode.OK, status);
            Assert.Contains("<h2 class=\"ht-card__title\">Settings</h2>", html, StringComparison.Ordinal);
            Assert.DoesNotContain("Consistency and streaks", html, StringComparison.Ordinal);
            Assert.DoesNotContain("ht-stat__label", html, StringComparison.Ordinal);
            Assert.DoesNotContain("ht-bars", html, StringComparison.Ordinal);
            Assert.DoesNotContain("<h2 class=\"ht-card__title\">History</h2>", html, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task Slice_6_US5_the_prerendered_Goals_page_lists_every_goal_in_the_overviews_order_with_its_figures_and_Record()
    {
        // Four goals of 15 min, on new Published habits named "… A" to "… D", in UTC at the host's clock: "… C" every day
        // from 4 days ago with 15 min on its first day (Met, then Missed, then two Pending, today Due); "… D" only on
        // yesterday's weekday from yesterday (Pending only); "… B" every day from today (Due only); "… A" every day from 3
        // days from now (nothing yet). FR-011's order is C, D (Pending, by name), B (Due), A. The first HTML, prerendered,
        // already has the rows (research R-12).
        var prefix = $"Ui-Go Ov {Guid.NewGuid():N}"[..22];
        var (browser, account) = await host.SignedInAccountAsync(Email("overview"), Role.RegisteredUser);
        using (browser)
        {
            GoalId a, b, c, d;
            await using (var scope = host.Services.CreateAsyncScope())
            {
                var services = scope.ServiceProvider;
                var today = NamedTimeZone.Utc.DateAt(services.GetRequiredService<TimeProvider>().GetUtcNow());
                var everyDay = Enum.GetValues<DayOfWeek>();
                a = await StoreGoalAsync(services, account, prefix + " A", everyDay, today.AddDays(3), recordFirstDay: false);
                b = await StoreGoalAsync(services, account, prefix + " B", everyDay, today, recordFirstDay: false);
                c = await StoreGoalAsync(services, account, prefix + " C", everyDay, today.AddDays(-4), recordFirstDay: true);
                d = await StoreGoalAsync(services, account, prefix + " D", [today.AddDays(-1).DayOfWeek], today.AddDays(-1), recordFirstDay: false);
            }

            var (status, html) = await browser.GetPageAsync(GoalsAddress);

            Assert.Equal(HttpStatusCode.OK, status);
            var positions = new[] { c, d, b, a }.Select(goal => html.IndexOf($"<a class=\"ht-goalitem__name\" href=\"/goals/{goal.Value}\">", StringComparison.Ordinal)).ToList();
            Assert.True(positions[0] >= 0, "The first goal is missing.");
            Assert.Equal(positions.Order(), positions);
            Assert.Equal(4, html.Split("<div class=\"ht-goalitem\">").Length - 1);

            var pending = ItemHtml(html, c);
            Assert.Contains($">{prefix} C, 15 min daily</a>", pending, StringComparison.Ordinal);
            Assert.Contains("<span class=\"ht-chip ht-status ht-status--pending\">Pending</span>", pending, StringComparison.Ordinal);
            Assert.Contains("<div class=\"ht-goalitem__value\">50% (1 of 2)</div><div class=\"ht-goalitem__label\">Consistency</div>", pending, StringComparison.Ordinal);
            Assert.Contains("<div class=\"ht-goalitem__value\">0 commitments</div><div class=\"ht-goalitem__label\">Current streak</div>", pending, StringComparison.Ordinal);
            Assert.Contains("<div class=\"ht-strip ht-strip--mini\" role=\"img\" aria-label=\"Last 4 commitments: 1 met, 1 missed, 2 pending\">", pending, StringComparison.Ordinal);
            Assert.Contains("class=\"ht-btn ht-btn--primary ht-btn--sm\"", pending, StringComparison.Ordinal);

            var pendingOnly = ItemHtml(html, d);
            Assert.Contains("<span class=\"ht-chip ht-status ht-status--pending\">Pending</span>", pendingOnly, StringComparison.Ordinal);
            Assert.Contains("class=\"ht-btn ht-btn--secondary ht-btn--sm\"", pendingOnly, StringComparison.Ordinal);
            Assert.Contains($"aria-label=\"Record for {prefix} D, 15 min, once a week\">Record</button>", pendingOnly, StringComparison.Ordinal);

            var due = ItemHtml(html, b);
            Assert.DoesNotContain("ht-status--pending", due, StringComparison.Ordinal);
            Assert.Contains("<div class=\"ht-goalitem__value\">N/A</div>", due, StringComparison.Ordinal);
            Assert.Contains("class=\"ht-btn ht-btn--primary ht-btn--sm\"", due, StringComparison.Ordinal);

            var notStarted = ItemHtml(html, a);
            Assert.Contains("aria-label=\"Last 0 commitments\"", notStarted, StringComparison.Ordinal);
            Assert.Contains("<div class=\"ht-goalitem__actions\"></div>", notStarted, StringComparison.Ordinal);
            Assert.DoesNotContain(">Record</button>", notStarted, StringComparison.Ordinal);

            Assert.Contains(
                "<p class=\"ht-footnote\">Each goal is counted on its own. Consistency counts only finalized results; pending, excused and paused ones are left out.</p>",
                html, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task Slice_7_HP_004_the_prerendered_Paused_tab_lists_the_paused_goal_with_Paused_until_and_Resume_and_Active_the_other()
    {
        // Two goals of 15 min every day from yesterday (UTC, at the host's clock; yesterday Pending, today Due): "… P" paused
        // through the host's PauseService from today, the pause date, for 7 days, so it is paused until today + 6; "… A" not
        // paused. The first HTML of /goals?tab=paused, prerendered, has the Paused tab current and only "… P", in its Paused
        // form; that of /goals has the Active tab current and only "… A". Both have the slice-7 footnote.
        var prefix = $"Ui-Go Tabs {Guid.NewGuid():N}"[..24];
        var (browser, account) = await host.SignedInAccountAsync(Email("tabs"), Role.RegisteredUser);
        using (browser)
        {
            DateOnly today;
            GoalId active, paused;
            await using (var scope = host.Services.CreateAsyncScope())
            {
                var services = scope.ServiceProvider;
                today = NamedTimeZone.Utc.DateAt(services.GetRequiredService<TimeProvider>().GetUtcNow());
                var everyDay = Enum.GetValues<DayOfWeek>();
                active = await StoreGoalAsync(services, account, prefix + " A", everyDay, today.AddDays(-1), recordFirstDay: false);
                paused = await StoreGoalAsync(services, account, prefix + " P", everyDay, today.AddDays(-1), recordFirstDay: false);
                var result = await services.GetRequiredService<PauseService>().PauseAsync(
                    ActingUser.ForAccount(account, Role.RegisteredUser), paused, today, today.AddDays(6), null, CancellationToken.None);
                Assert.Equal(PauseOutcome.Paused, result.Outcome);
            }

            var (pausedStatus, pausedTab) = await browser.GetPageAsync(GoalsAddress + "?tab=paused");
            var (activeStatus, activeTab) = await browser.GetPageAsync(GoalsAddress);

            const string footnote =
                "<p class=\"ht-footnote\">Each goal is counted on its own. Consistency counts only finalized results; pending, excused and paused ones are left out.</p>";
            Assert.Equal(HttpStatusCode.OK, pausedStatus);
            Assert.Contains("<nav class=\"ht-tabs\" aria-label=\"Goal lists\">", pausedTab, StringComparison.Ordinal);
            Assert.Contains("<a class=\"ht-tab\" href=\"/goals\">Active</a>", pausedTab, StringComparison.Ordinal);
            Assert.Contains("<a class=\"ht-tab\" href=\"/goals?tab=paused\" aria-current=\"page\">Paused</a>", pausedTab, StringComparison.Ordinal);
            // Slice 8 (HP-005, T099): the third tab "Ended" is now there (slice 7 asserted it absent).
            Assert.Contains("<a class=\"ht-tab\" href=\"/goals?tab=ended\">Ended</a>", pausedTab, StringComparison.Ordinal);
            Assert.Equal(1, pausedTab.Split("<div class=\"ht-goalitem\">").Length - 1);
            var item = ItemHtml(pausedTab, paused);
            Assert.Contains($">{prefix} P, 15 min daily</a>", item, StringComparison.Ordinal);
            Assert.Contains("<span class=\"ht-chip ht-status ht-status--pending\">Pending</span>", item, StringComparison.Ordinal);
            Assert.Contains($"<span>Paused until {DateTexts.ShortDate(today.AddDays(6))}</span>", item, StringComparison.Ordinal);
            Assert.Contains(
                $"class=\"ht-btn ht-btn--secondary ht-btn--sm\" aria-label=\"Resume {prefix} P, 15 min daily\">Resume</button>", item, StringComparison.Ordinal);
            Assert.DoesNotContain(">Record</button>", item, StringComparison.Ordinal);
            Assert.DoesNotContain($"href=\"/goals/{active.Value}\"", pausedTab, StringComparison.Ordinal);
            Assert.Contains(footnote, pausedTab, StringComparison.Ordinal);

            Assert.Equal(HttpStatusCode.OK, activeStatus);
            Assert.Contains("<a class=\"ht-tab\" href=\"/goals\" aria-current=\"page\">Active</a>", activeTab, StringComparison.Ordinal);
            Assert.Contains("<a class=\"ht-tab\" href=\"/goals?tab=paused\">Paused</a>", activeTab, StringComparison.Ordinal);
            Assert.Equal(1, activeTab.Split("<div class=\"ht-goalitem\">").Length - 1);
            Assert.Contains($"aria-label=\"Record for {prefix} A, 15 min daily\">Record</button>", ItemHtml(activeTab, active), StringComparison.Ordinal);
            Assert.DoesNotContain($"href=\"/goals/{paused.Value}\"", activeTab, StringComparison.Ordinal);
            Assert.DoesNotContain("Paused until", activeTab, StringComparison.Ordinal);
            Assert.Contains(footnote, activeTab, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task Slice_8_HP_005_the_prerendered_Ended_tab_lists_the_ended_goal_in_its_Ended_form_and_Active_the_other()
    {
        // Two goals of 15 min every day from yesterday (UTC, at the host's clock; yesterday Pending, today Due): "… E" ended
        // through the host's GoalChangeService today with "Abandoned" (ending date today, last date yesterday, still
        // Pending); "… A" not ended. The first HTML of /goals?tab=ended, prerendered, has the Ended tab current and only
        // "… E", in its Ended form; that of /goals has only "… A".
        var prefix = $"Ui-Go Ended {Guid.NewGuid():N}"[..25];
        var (browser, account) = await host.SignedInAccountAsync(Email("ended"), Role.RegisteredUser);
        using (browser)
        {
            DateOnly today;
            GoalId active, ended;
            await using (var scope = host.Services.CreateAsyncScope())
            {
                var services = scope.ServiceProvider;
                today = NamedTimeZone.Utc.DateAt(services.GetRequiredService<TimeProvider>().GetUtcNow());
                var everyDay = Enum.GetValues<DayOfWeek>();
                active = await StoreGoalAsync(services, account, prefix + " A", everyDay, today.AddDays(-1), recordFirstDay: false);
                ended = await StoreGoalAsync(services, account, prefix + " E", everyDay, today.AddDays(-1), recordFirstDay: false);
                var result = await services.GetRequiredService<GoalChangeService>().EndAsync(
                    ActingUser.ForAccount(account, Role.RegisteredUser), ended, Outcome.Abandoned, null, CancellationToken.None);
                Assert.Equal((GoalEndOutcome.Ended, (DateOnly?)today), (result.Outcome, result.EndingDate));
            }

            var (endedStatus, endedTab) = await browser.GetPageAsync(GoalsAddress + "?tab=ended");
            var (activeStatus, activeTab) = await browser.GetPageAsync(GoalsAddress);

            Assert.Equal(HttpStatusCode.OK, endedStatus);
            Assert.Contains("<a class=\"ht-tab\" href=\"/goals\">Active</a>", endedTab, StringComparison.Ordinal);
            Assert.Contains("<a class=\"ht-tab\" href=\"/goals?tab=paused\">Paused</a>", endedTab, StringComparison.Ordinal);
            Assert.Contains("<a class=\"ht-tab\" href=\"/goals?tab=ended\" aria-current=\"page\">Ended</a>", endedTab, StringComparison.Ordinal);
            Assert.Equal(1, endedTab.Split("<div class=\"ht-goalitem\">").Length - 1);
            var item = ItemHtml(endedTab, ended);
            Assert.Contains($">{prefix} E, 15 min daily</a>", item, StringComparison.Ordinal);
            Assert.Contains("<span class=\"ht-chip\">Abandoned</span>", item, StringComparison.Ordinal);
            Assert.Contains("<span class=\"ht-chip ht-status ht-status--pending\">Pending</span>", item, StringComparison.Ordinal);
            Assert.Contains($"<span>Ended {DateTexts.ShortDate(today)}</span>", item, StringComparison.Ordinal);
            Assert.Contains(
                $"<a class=\"ht-btn ht-btn--secondary ht-btn--sm\" href=\"/goals/{ended.Value}/new-attempt\" aria-label=\"Start new attempt for {prefix} E, 15 min daily\">Start new attempt</a>",
                item, StringComparison.Ordinal);
            Assert.DoesNotContain(">Record</button>", item, StringComparison.Ordinal);
            Assert.DoesNotContain($"href=\"/goals/{active.Value}\"", endedTab, StringComparison.Ordinal);
            Assert.Contains(
                "<p class=\"ht-footnote\">Each goal is counted on its own. Consistency counts only finalized results; pending, excused and paused ones are left out.</p>",
                endedTab, StringComparison.Ordinal);

            Assert.Equal(HttpStatusCode.OK, activeStatus);
            Assert.Equal(1, activeTab.Split("<div class=\"ht-goalitem\">").Length - 1);
            Assert.Contains($"aria-label=\"Record for {prefix} A, 15 min daily\">Record</button>", ItemHtml(activeTab, active), StringComparison.Ordinal);
            Assert.DoesNotContain($"href=\"/goals/{ended.Value}\"", activeTab, StringComparison.Ordinal);
            Assert.DoesNotContain("Start new attempt", activeTab, StringComparison.Ordinal);
        }
    }

    /// <summary>The HTML of the Goals page's item of <paramref name="goal"/>, up to the next item (or the page's end).</summary>
    private static string ItemHtml(string html, GoalId goal)
    {
        var items = html.Split("<div class=\"ht-goalitem\">");
        return Assert.Single(items.Skip(1), item => item.Contains($"href=\"/goals/{goal.Value}\"", StringComparison.Ordinal));
    }

    /// <summary>
    /// A goal of 15 min on <paramref name="days"/> of <paramref name="account"/>, stored through the host's goal store on
    /// a new Published catalog habit named <paramref name="habitName"/>, from <paramref name="first"/> in UTC, with 15 min
    /// recorded on that day when <paramref name="recordFirstDay"/> is set.
    /// </summary>
    private async Task<GoalId> StoreGoalAsync(
        IServiceProvider services, AccountId account, string habitName, DayOfWeek[] days, DateOnly first, bool recordFirstDay)
    {
        var habit = await host.ArrangeCatalogHabitAsync(Definition(habitName));
        Assert.True(GoalTarget.TryCreate(new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = 15, DurationUnit = DurationUnit.Minutes,
            ScheduleKind = ScheduleKind.FixedDays, Days = days.ToHashSet(),
        }, out var target, out _));
        var created = await services.GetRequiredService<IGoalStore>().CreateAsync(
            account, new NewGoal(new GoalHabit(habit, HabitKind.Catalog), first, null, target), NamedTimeZone.Utc, NamedTimeZone.Utc, CancellationToken.None);
        Assert.Equal(GoalWriteOutcomeKind.Created, created.Kind);
        var goal = created.Id!.Value;
        if (recordFirstDay)
        {
            var written = await services.GetRequiredService<IRecordStore>().AddAsync(
                account, goal, new EntrySpan(first, first, first), true, first, RecordValue.FromAmount(15), NoteWrite.Keep, RecordLimits.MaxRecordsPerPeriod, CancellationToken.None);
            Assert.Equal(RecordStoreWriteOutcome.Written, written.Outcome);
        }

        return goal;
    }

    /// <summary>
    /// A registered user signed in, with a goal of 15 min on every day stored through the host's goal store on a new
    /// Published catalog habit, starting <paramref name="startInDays"/> days from today in UTC (the account's zone) at
    /// the host's clock, and with 15 min recorded on its first day when <paramref name="recordFirstDay"/> is set.
    /// </summary>
    private async Task<(HttpClient Browser, GoalId Goal)> OwnerWithEveryDayGoalAsync(string who, int startInDays, bool recordFirstDay)
    {
        var name = $"Ui-Go Stats {Guid.NewGuid():N}"[..30];
        var habit = await host.ArrangeCatalogHabitAsync(Definition(name));
        var (browser, account) = await host.SignedInAccountAsync(Email(who), Role.RegisteredUser);
        await using var scope = host.Services.CreateAsyncScope();
        var services = scope.ServiceProvider;
        var first = NamedTimeZone.Utc.DateAt(services.GetRequiredService<TimeProvider>().GetUtcNow()).AddDays(startInDays);
        Assert.True(GoalTarget.TryCreate(new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = 15, DurationUnit = DurationUnit.Minutes,
            ScheduleKind = ScheduleKind.FixedDays, Days = Enum.GetValues<DayOfWeek>().ToHashSet(),
        }, out var target, out _));
        var created = await services.GetRequiredService<IGoalStore>().CreateAsync(
            account, new NewGoal(new GoalHabit(habit, HabitKind.Catalog), first, null, target), NamedTimeZone.Utc, NamedTimeZone.Utc, CancellationToken.None);
        Assert.Equal(GoalWriteOutcomeKind.Created, created.Kind);
        var goal = created.Id!.Value;
        if (recordFirstDay)
        {
            var written = await services.GetRequiredService<IRecordStore>().AddAsync(
                account, goal, new EntrySpan(first, first, first), true, first, RecordValue.FromAmount(15), NoteWrite.Keep, RecordLimits.MaxRecordsPerPeriod, CancellationToken.None);
            Assert.Equal(RecordStoreWriteOutcome.Written, written.Outcome);
        }

        return (browser, goal);
    }

    /// <summary>
    /// A registered user signed in, with one goal created through the host's <see cref="GoalService"/> on a new Published
    /// catalog habit: 15 min on Monday to Friday, or on every day with <paramref name="everyDay"/>, from today in UTC (the
    /// account's zone) at the host's clock.
    /// </summary>
    private async Task<(HttpClient Browser, GoalId Goal, string HabitName)> OwnerWithGoalAsync(string who, bool everyDay = false)
    {
        var name = $"Ui-Go Goal {Guid.NewGuid():N}"[..30];
        var habit = await host.ArrangeCatalogHabitAsync(Definition(name));
        var (browser, account) = await host.SignedInAccountAsync(Email(who), Role.RegisteredUser);
        await using var scope = host.Services.CreateAsyncScope();
        var services = scope.ServiceProvider;
        var today = NamedTimeZone.Utc.DateAt(services.GetRequiredService<TimeProvider>().GetUtcNow());
        var created = await services.GetRequiredService<GoalService>().CreateAsync(ActingUser.ForAccount(account, Role.RegisteredUser), new GoalInput
        {
            Habit = new GoalHabit(habit, HabitKind.Catalog),
            Target = new GoalTargetFields
            {
                BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = 15, DurationUnit = DurationUnit.Minutes,
                ScheduleKind = ScheduleKind.FixedDays, Days = everyDay ? Enum.GetValues<DayOfWeek>().ToHashSet() : Weekdays.ToHashSet(),
            },
            StartDate = today,
        }, CancellationToken.None);
        Assert.Equal(GoalCreateOutcome.Created, created.Outcome);
        return (browser, created.GoalId!.Value, name);
    }
}
