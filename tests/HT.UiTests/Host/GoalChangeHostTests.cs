using System.Net;
using HT.Application.Accounts;
using HT.Application.Goals;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Time;
using Microsoft.Extensions.DependencyInjection;
using static HT.UnitTests.Fakes.HabitSamples;

namespace HT.UiTests.Host;

/// <summary>
/// Slice 8 (T070; HP-001, HP-002, SC-001, contracts/host.md "Routes", "Edit settings page"): the Edit settings page
/// through the real host on HT-UI. Its owner gets the prerendered form; another account, an unknown goal and an
/// administrator get the Not found page; an ended goal redirects to its goal page. Slice 8 US3 (T092; HP-004, HP-014,
/// US3-AS14): the Start new attempt page and the attempt view with their Not found and redirect states, a new attempt in
/// two actions from the goal page's Notice, and the attempt view of attempt 1. Dates come from the host's clock in UTC,
/// the accounts' zone. Shared database: own prefixes ("ui-gc-", "Ui-Gc").
/// </summary>
[Collection(HostCollection.Name)]
public sealed class GoalChangeHostTests(HostFixture host)
{
    private static string Email(string who) => $"ui-gc-{who}-{Guid.NewGuid():N}@example.com";

    private static string EditAddress(GoalId goal) => $"/goals/{goal.Value}/edit";

    private static GoalTargetFields MinutesDaily(int minutes) => new()
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = minutes, DurationUnit = DurationUnit.Minutes,
        ScheduleKind = ScheduleKind.FixedDays, Days = Enum.GetValues<DayOfWeek>().ToHashSet(),
    };

    /// <summary>
    /// A registered user signed in, with "{habit}, 15 min daily" created through the host's GoalService on a new Published
    /// catalog habit, from today in UTC at the host's clock; returns today too.
    /// </summary>
    private async Task<(HttpClient Browser, ActingUser User, GoalId Goal, string Habit, DateOnly Today)> OwnerWithGoalAsync(string who)
    {
        var name = $"Ui-Gc Goal {Guid.NewGuid():N}"[..30];
        var habit = await host.ArrangeCatalogHabitAsync(Definition(name));
        var (browser, account) = await host.SignedInAccountAsync(Email(who), Role.RegisteredUser);
        var user = ActingUser.ForAccount(account, Role.RegisteredUser);
        await using var scope = host.Services.CreateAsyncScope();
        var services = scope.ServiceProvider;
        var today = NamedTimeZone.Utc.DateAt(services.GetRequiredService<TimeProvider>().GetUtcNow());
        var created = await services.GetRequiredService<GoalService>().CreateAsync(user, new GoalInput
        {
            Habit = new GoalHabit(habit, HabitKind.Catalog), Target = MinutesDaily(15), StartDate = today,
        }, CancellationToken.None);
        Assert.Equal(GoalCreateOutcome.Created, created.Outcome);
        return (browser, user, created.GoalId!.Value, name, today);
    }

    [Fact]
    public async Task HP_002_the_owner_gets_the_prerendered_Edit_settings_page()
    {
        var (browser, _, goal, habit, today) = await OwnerWithGoalAsync("owner");
        using (browser)
        {
            var (status, html) = await browser.GetPageAsync(EditAddress(goal));

            Assert.Equal(HttpStatusCode.OK, status);
            var text = WebUtility.HtmlDecode(html);
            Assert.Contains($"<a class=\"ht-back\" href=\"/goals/{goal.Value}\">← {habit}, 15 min daily</a>", text, StringComparison.Ordinal);
            Assert.Contains("<h1>Edit settings</h1>", text, StringComparison.Ordinal);
            Assert.Contains("id=\"edit-goal-amount\"", text, StringComparison.Ordinal);
            Assert.Contains($"Changes apply from {DateTexts.ShortDate(today.AddDays(1))}. Past results keep their rules.", text, StringComparison.Ordinal);
            Assert.Contains(">Save changes</button>", text, StringComparison.Ordinal);
            Assert.DoesNotContain("type=\"date\"", text, StringComparison.Ordinal);
            Assert.DoesNotContain("Choose a habit", text, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task HP_002_another_account_an_unknown_goal_and_an_administrator_get_the_Not_found_page()
    {
        var (owner, _, goal, habit, _) = await OwnerWithGoalAsync("nf-owner");
        owner.Dispose();
        using var other = await host.SignedInBrowserAsync(Email("nf-other"), Role.RegisteredUser);
        using var administrator = await host.SignedInBrowserAsync(Email("nf-admin"), Role.Administrator);

        foreach (var (browser, address) in new[]
                 {
                     (other, EditAddress(goal)), (other, EditAddress(new GoalId(Guid.NewGuid()))), (administrator, EditAddress(goal)),
                 })
        {
            var (status, html) = await browser.GetPageAsync(address);

            Assert.Equal(HttpStatusCode.NotFound, status);
            Assert.Contains("Page not found", html, StringComparison.Ordinal);
            Assert.DoesNotContain(habit, html, StringComparison.Ordinal);
            Assert.DoesNotContain("Save changes", html, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task HP_002_a_signed_out_visitor_is_sent_to_sign_in()
    {
        using var browser = host.CreateBrowser();
        var address = EditAddress(new GoalId(Guid.NewGuid()));

        using var response = await browser.GetAsync(new Uri(address, UriKind.Relative));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("https://localhost/sign-in?returnUrl=" + Uri.EscapeDataString(address), response.RedirectTarget(browser));
    }

    [Fact]
    public async Task HP_002_an_ended_goal_redirects_to_its_goal_page()
    {
        var (browser, user, goal, _, _) = await OwnerWithGoalAsync("ended");
        using (browser)
        {
            // Ended through the host's goal store, as ending will (FR-007, FR-008): on its current date.
            await using (var scope = host.Services.CreateAsyncScope())
            {
                var services = scope.ServiceProvider;
                var store = services.GetRequiredService<IGoalStore>();
                var owner = user.AccountId!.Value;
                var read = (await store.ReadOwnerGoalAsync(owner, goal, CancellationToken.None))!;
                var stored = read.Goal!.Goal;
                var now = services.GetRequiredService<TimeProvider>().GetUtcNow();
                var endingDate = GoalTimeline.CurrentDate(stored, now) ?? NamedTimeZone.Utc.DateAt(now);
                var effect = GoalTimeline.EndingOf(stored, endingDate);
                var write = new GoalEndingWrite(goal, stored.Attempt.Number, new GoalEnding(endingDate, Outcome.Abandoned, null), effect.LastDate, effect.KeptVersions);
                Assert.Equal(GoalChangeWriteOutcome.Written, await store.TryEndAsync(owner, read.StoredTimeZone, read.GoalsRevision, write, CancellationToken.None));
            }

            using var response = await browser.GetAsync(new Uri(EditAddress(goal), UriKind.Relative));

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal($"https://localhost/goals/{goal.Value}", response.RedirectTarget(browser));
        }
    }

    [Fact]
    public async Task SC_001_a_change_in_three_actions_from_the_goal_page_shows_in_the_Settings_card()
    {
        var (browser, user, goal, _, today) = await OwnerWithGoalAsync("sc001");
        using (browser)
        {
            // 1. "Edit settings" on the goal page.
            var (pageStatus, page) = await browser.GetPageAsync($"/goals/{goal.Value}");
            Assert.Equal(HttpStatusCode.OK, pageStatus);
            Assert.Contains($"<a class=\"ht-btn ht-btn--ghost\" href=\"{EditAddress(goal)}\">Edit settings</a>", page, StringComparison.Ordinal);

            // 2. The amount, and 3. "Save changes", on the Edit settings page: the field and the button are there; the
            // interactive save sends the form's GoalSettingsInput to GoalChangeService, which this test calls as it does.
            var (editStatus, edit) = await browser.GetPageAsync(EditAddress(goal));
            Assert.Equal(HttpStatusCode.OK, editStatus);
            Assert.Contains("id=\"edit-goal-amount\"", edit, StringComparison.Ordinal);
            Assert.Contains(">Save changes</button>", edit, StringComparison.Ordinal);
            await using (var scope = host.Services.CreateAsyncScope())
            {
                var changed = await scope.ServiceProvider.GetRequiredService<GoalChangeService>().ChangeSettingsAsync(
                    user, goal, new GoalSettingsInput { Target = MinutesDaily(20) }, CancellationToken.None);
                Assert.Equal((SettingsChangeOutcome.Changed, (DateOnly?)today.AddDays(1)), (changed.Outcome, changed.AppliesFrom));
            }

            // The goal page's Settings card shows the later settings with their date, after the Target row.
            var (afterStatus, after) = await browser.GetPageAsync($"/goals/{goal.Value}");
            Assert.Equal(HttpStatusCode.OK, afterStatus);
            var rows = string.Concat(WebUtility.HtmlDecode(after).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
            Assert.Contains(
                $"<dt>Target</dt><dd>15mindaily</dd><dt>From{DateTexts.ShortDate(today.AddDays(1)).Replace(" ", "", StringComparison.Ordinal)}</dt><dd>20mindaily</dd>",
                rows, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task SC_001_an_ending_in_three_actions_from_the_goal_page_shows_the_ended_Notice()
    {
        // Slice 8 (T078; HP-001, HP-003, US2-AS13).
        var (browser, user, goal, _, today) = await OwnerWithGoalAsync("sc001-end");
        using (browser)
        {
            // 1. "End goal" in the goal page's head, after "Edit settings".
            var (pageStatus, page) = await browser.GetPageAsync($"/goals/{goal.Value}");
            Assert.Equal(HttpStatusCode.OK, pageStatus);
            var head = WebUtility.HtmlDecode(page);
            Assert.Contains("<button type=\"button\" class=\"ht-btn ht-btn--ghost\" aria-haspopup=\"dialog\">End goal</button>", head, StringComparison.Ordinal);
            Assert.True(head.IndexOf(">Edit settings</a>", StringComparison.Ordinal) < head.IndexOf(">End goal</button>", StringComparison.Ordinal));

            // 2. The outcome, and 3. "End goal", in the EndGoalDialog: the interactive dialog sends them to GoalChangeService,
            // which this test calls as it does.
            await using (var scope = host.Services.CreateAsyncScope())
            {
                var ended = await scope.ServiceProvider.GetRequiredService<GoalChangeService>().EndAsync(
                    user, goal, Outcome.Abandoned, null, CancellationToken.None);
                Assert.Equal((GoalEndOutcome.Ended, (DateOnly?)today), (ended.Outcome, ended.EndingDate));
            }

            // The goal page now shows the ended Notice with "Start new attempt", and no head actions.
            var (afterStatus, after) = await browser.GetPageAsync($"/goals/{goal.Value}");
            Assert.Equal(HttpStatusCode.OK, afterStatus);
            var text = WebUtility.HtmlDecode(after);
            Assert.Contains($"<span>Ended {DateTexts.ShortDate(today)} · Abandoned</span>", text, StringComparison.Ordinal);
            Assert.Contains($"<a class=\"ht-btn ht-btn--primary ht-btn--sm\" href=\"/goals/{goal.Value}/new-attempt\">Start new attempt</a>", text, StringComparison.Ordinal);
            foreach (var absent in new[] { ">Edit settings<", ">End goal<", ">Pause<", ">Record<" })
            {
                Assert.DoesNotContain(absent, text, StringComparison.Ordinal);
            }
        }
    }

    // ---- slice 8 US3: the Start new attempt page and the attempt view (T092; HP-001, HP-004, HP-014, US3-AS14) ----

    private static string NewAttemptAddress(GoalId goal) => $"/goals/{goal.Value}/new-attempt";

    private static string AttemptAddress(GoalId goal, int number) => $"/goals/{goal.Value}/attempts/{number}";

    /// <summary>Ends the goal through the host's GoalChangeService, today, with "Abandoned" (as the EndGoalDialog does).</summary>
    private async Task EndAsync(ActingUser user, GoalId goal)
    {
        await using var scope = host.Services.CreateAsyncScope();
        var ended = await scope.ServiceProvider.GetRequiredService<GoalChangeService>().EndAsync(
            user, goal, Outcome.Abandoned, null, CancellationToken.None);
        Assert.Equal(GoalEndOutcome.Ended, ended.Outcome);
    }

    /// <summary>
    /// Starts attempt 2 through the host's GoalChangeService with the starting values unchanged, as "Start attempt" sends
    /// the form the page filled with them; returns its start date.
    /// </summary>
    private async Task<DateOnly> StartWithStartingValuesAsync(ActingUser user, GoalId goal)
    {
        await using var scope = host.Services.CreateAsyncScope();
        var changes = scope.ServiceProvider.GetRequiredService<GoalChangeService>();
        var values = (await changes.GetNewAttemptValuesAsync(user, goal, CancellationToken.None)).Values!;
        var input = new NewAttemptInput
        {
            Target = new GoalTargetFields
            {
                BuildOrReduce = values.BuildOrReduce, Measurement = values.Measurement, Amount = values.Amount, DurationUnit = values.DurationUnit,
                ScheduleKind = values.ScheduleKind, Days = values.Days,
            },
            StartDate = values.StartDate,
        };
        var started = await changes.StartAttemptAsync(user, goal, input, CancellationToken.None);
        Assert.Equal((AttemptStartOutcome.Started, (int?)2), (started.Outcome, started.AttemptNumber));
        return started.StartDate!.Value;
    }

    [Fact]
    public async Task HP_004_HP_014_a_goal_not_ended_redirects_from_Start_new_attempt_and_its_attempt_numbers_give_Not_found()
    {
        var (browser, user, goal, habit, _) = await OwnerWithGoalAsync("us3-states");
        using (browser)
        {
            using (var response = await browser.GetAsync(new Uri(NewAttemptAddress(goal), UriKind.Relative)))
            {
                Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
                Assert.Equal($"https://localhost/goals/{goal.Value}", response.RedirectTarget(browser));
            }

            // The latest attempt's number (the goal page shows it) and an unknown number, before and after the ending.
            await AssertNotFoundAsync(browser, habit, AttemptAddress(goal, 1), AttemptAddress(goal, 2));
            await EndAsync(user, goal);
            await AssertNotFoundAsync(browser, habit, AttemptAddress(goal, 1), AttemptAddress(goal, 0));
        }
    }

    private static async Task AssertNotFoundAsync(HttpClient browser, string habit, params string[] addresses)
    {
        foreach (var address in addresses)
        {
            var (status, html) = await browser.GetPageAsync(address);

            Assert.Equal(HttpStatusCode.NotFound, status);
            Assert.Contains("Page not found", html, StringComparison.Ordinal);
            Assert.DoesNotContain(habit, html, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task HP_004_HP_014_another_account_an_unknown_goal_and_an_administrator_get_the_Not_found_page_on_the_new_routes()
    {
        var (owner, user, goal, habit, _) = await OwnerWithGoalAsync("us3-nf-owner");
        owner.Dispose();
        await EndAsync(user, goal);
        await StartWithStartingValuesAsync(user, goal);
        using var other = await host.SignedInBrowserAsync(Email("us3-nf-other"), Role.RegisteredUser);
        using var administrator = await host.SignedInBrowserAsync(Email("us3-nf-admin"), Role.Administrator);
        var unknown = new GoalId(Guid.NewGuid());

        foreach (var (browser, address) in new[]
                 {
                     (other, NewAttemptAddress(goal)), (other, NewAttemptAddress(unknown)), (administrator, NewAttemptAddress(goal)),
                     (other, AttemptAddress(goal, 1)), (other, AttemptAddress(unknown, 1)), (administrator, AttemptAddress(goal, 1)),
                 })
        {
            var (status, html) = await browser.GetPageAsync(address);

            Assert.Equal(HttpStatusCode.NotFound, status);
            Assert.Contains("Page not found", html, StringComparison.Ordinal);
            Assert.DoesNotContain(habit, html, StringComparison.Ordinal);
            Assert.DoesNotContain("Start attempt", html, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task SC_001_US3_AS14_a_new_attempt_in_two_actions_from_the_goal_pages_Notice_opens_the_goal_page_with_attempt_2()
    {
        var (browser, user, goal, habit, today) = await OwnerWithGoalAsync("us3-start");
        using (browser)
        {
            await EndAsync(user, goal);

            // 1. "Start new attempt" in the goal page's ended Notice.
            var (pageStatus, page) = await browser.GetPageAsync($"/goals/{goal.Value}");
            Assert.Equal(HttpStatusCode.OK, pageStatus);
            Assert.Contains($"href=\"{NewAttemptAddress(goal)}\">Start new attempt</a>", page, StringComparison.Ordinal);

            // 2. "Start attempt" on the Start new attempt page, which opens with the starting values: the day after the
            // ending date (today) as the start date, and the line with the next number. The interactive save sends them
            // to GoalChangeService, which this test calls as it does.
            var (startStatus, start) = await browser.GetPageAsync(NewAttemptAddress(goal));
            Assert.Equal(HttpStatusCode.OK, startStatus);
            var text = WebUtility.HtmlDecode(start);
            Assert.Contains($"<a class=\"ht-back\" href=\"/goals/{goal.Value}\">← {habit}, 15 min daily</a>", text, StringComparison.Ordinal);
            Assert.Contains("<h1>Start new attempt</h1>", text, StringComparison.Ordinal);
            Assert.Contains("<span class=\"ht-muted\">Attempt 2 starts with its own history and a streak from zero.</span>", text, StringComparison.Ordinal);
            var tomorrow = today.AddDays(1).ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            Assert.Contains($"id=\"new-attempt-start\" value=\"{tomorrow}\"", text, StringComparison.Ordinal);
            Assert.Contains(">Start attempt</button>", text, StringComparison.Ordinal);
            Assert.DoesNotContain("Choose a habit", text, StringComparison.Ordinal);
            Assert.DoesNotContain("id=\"new-attempt-name\"", text, StringComparison.Ordinal);
            var startDate = await StartWithStartingValuesAsync(user, goal);
            Assert.Equal(today.AddDays(1), startDate);

            // The goal page now shows attempt 2, not ended, with attempt 1 in the Earlier attempts card.
            var (afterStatus, after) = await browser.GetPageAsync($"/goals/{goal.Value}");
            Assert.Equal(HttpStatusCode.OK, afterStatus);
            var goalPage = WebUtility.HtmlDecode(after);
            Assert.Contains($"<span class=\"ht-attempt\">Attempt 2 · starts {DateTexts.DayMonth(startDate)}</span>", goalPage, StringComparison.Ordinal);
            Assert.DoesNotContain("Start new attempt", goalPage, StringComparison.Ordinal);
            Assert.Contains("<h2 class=\"ht-card__title\">Earlier attempts</h2>", goalPage, StringComparison.Ordinal);
            Assert.Contains($"<a class=\"ht-goal__name\" href=\"{AttemptAddress(goal, 1)}\">Attempt 1 · ended before it began</a>", goalPage, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task SC_001_HP_005_a_new_attempt_in_two_actions_from_the_Ended_tab_moves_the_goal_back_to_Active()
    {
        // Slice 8 US4 (T099): the same two actions starting from the Goals page's Ended tab.
        var (browser, user, goal, habit, today) = await OwnerWithGoalAsync("us4-start");
        using (browser)
        {
            await EndAsync(user, goal);

            // 1. "Start new attempt" of the goal's item on the Ended tab.
            var (endedStatus, endedTab) = await browser.GetPageAsync("/goals?tab=ended");
            Assert.Equal(HttpStatusCode.OK, endedStatus);
            Assert.Contains(
                $"<a class=\"ht-btn ht-btn--secondary ht-btn--sm\" href=\"{NewAttemptAddress(goal)}\" aria-label=\"Start new attempt for {habit}, 15 min daily\">Start new attempt</a>",
                WebUtility.HtmlDecode(endedTab), StringComparison.Ordinal);

            // 2. "Start attempt" on the Start new attempt page, with its starting values (sent as the interactive save does).
            var (startStatus, start) = await browser.GetPageAsync(NewAttemptAddress(goal));
            Assert.Equal(HttpStatusCode.OK, startStatus);
            Assert.Contains(">Start attempt</button>", start, StringComparison.Ordinal);
            Assert.Equal(today.AddDays(1), await StartWithStartingValuesAsync(user, goal));

            // The goal has left the Ended tab and is on the Active tab again.
            var (_, endedAfter) = await browser.GetPageAsync("/goals?tab=ended");
            var (_, activeAfter) = await browser.GetPageAsync("/goals");
            Assert.DoesNotContain($"href=\"/goals/{goal.Value}\"", endedAfter, StringComparison.Ordinal);
            Assert.Contains("<p class=\"ht-card__sub\">No ended goals.</p>", endedAfter, StringComparison.Ordinal);
            Assert.Contains($"href=\"/goals/{goal.Value}\"", activeAfter, StringComparison.Ordinal);
            Assert.DoesNotContain("Start new attempt", activeAfter, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task HP_014_the_attempt_view_of_attempt_1_is_prerendered_with_its_head_and_figures_and_no_goal_actions()
    {
        var (browser, user, goal, habit, _) = await OwnerWithGoalAsync("us3-view");
        using (browser)
        {
            await EndAsync(user, goal);
            await StartWithStartingValuesAsync(user, goal);

            var (status, html) = await browser.GetPageAsync(AttemptAddress(goal, 1));

            Assert.Equal(HttpStatusCode.OK, status);
            var text = WebUtility.HtmlDecode(html);
            Assert.Contains($"<a class=\"ht-back\" href=\"/goals/{goal.Value}\">← {habit}, 15 min daily</a>", text, StringComparison.Ordinal);
            // Ended on its start date, attempt 1 has no date (AC-102).
            Assert.Contains("<h1>Attempt 1 · ended before it began</h1>", text, StringComparison.Ordinal);
            Assert.Contains($"<span class=\"ht-attempt\">{habit}, 15 min daily</span>", text, StringComparison.Ordinal);
            Assert.Contains("<span class=\"ht-chip\">Abandoned</span>", text, StringComparison.Ordinal);
            Assert.Contains("<div class=\"ht-eyebrow\">Attempt 1</div>", text, StringComparison.Ordinal);
            Assert.Contains(">N/A</div>", text, StringComparison.Ordinal);
            foreach (var absent in new[] { ">Edit settings<", ">End goal<", ">Pause<", ">Record<", "Earlier attempts", "aria-label=\"History\"" })
            {
                Assert.DoesNotContain(absent, text, StringComparison.Ordinal);
            }

            // The latest attempt, now 2, is the goal page's.
            var (latestStatus, _) = await browser.GetPageAsync(AttemptAddress(goal, 2));
            Assert.Equal(HttpStatusCode.NotFound, latestStatus);
        }
    }
}
