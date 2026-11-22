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
/// administrator get the Not found page; an ended goal redirects to its goal page. Dates come from the host's clock in
/// UTC, the accounts' zone. Shared database: own prefixes ("ui-gc-", "Ui-Gc").
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
}
