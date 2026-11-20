using System.Net;
using HT.Application.Accounts;
using HT.Application.Goals;
using HT.Application.Records;
using HT.Domain.Accounts;
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
