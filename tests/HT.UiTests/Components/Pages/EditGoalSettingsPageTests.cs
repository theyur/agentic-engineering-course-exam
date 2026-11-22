using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using HT.Application.Goals;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Time;
using HT.Ui.Components.Pages;
using HT.UnitTests.Fakes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace HT.UiTests.Components.Pages;

/// <summary>
/// Slice 8 (T070; HP-002, HP-009, US1-AS15, contracts/host.md "Edit settings page"): the Edit settings page over the
/// real GoalService, GoalChangeService and the fakes. Today is Tuesday 17 November 2026, 10:00 in UTC, the zone of the
/// signed-in user; "Reading, 15 min daily" starts today, so a change applies from Wednesday 18 November.
/// </summary>
public sealed class EditGoalSettingsPageTests : BunitContext
{
    private static readonly DateOnly Today = new(2026, 11, 17);
    private static readonly DateTimeOffset Now = new(2026, 11, 17, 10, 0, 0, TimeSpan.Zero);

    private readonly BunitAuthorizationContext _authorization;
    private readonly FakeCatalogHabitStore _catalog;
    private readonly FakeGoalStore _goals;
    private int _notFound;

    public EditGoalSettingsPageTests()
    {
        _authorization = AddAuthorization();
        _catalog = this.AddCatalog();
        this.AddCustomHabits();
        _goals = this.AddGoals();
        ((FakeTimeProvider)Services.GetRequiredService<TimeProvider>()).SetUtcNow(Now);
        Navigation.OnNotFound += (_, _) => _notFound++;
    }

    private BunitNavigationManager Navigation => Services.GetRequiredService<BunitNavigationManager>();

    private static GoalTargetFields MinutesDaily(int minutes) => new()
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = minutes, DurationUnit = DurationUnit.Minutes,
        ScheduleKind = ScheduleKind.FixedDays, Days = Enum.GetValues<DayOfWeek>().ToHashSet(),
    };

    private ActingUser SignIn(string role = "RegisteredUser") =>
        ActingUser.ForAccount(new AccountId(_authorization.SignIn(role)), role == "Administrator" ? Role.Administrator : Role.RegisteredUser);

    private async Task<GoalId> CreateAsync(ActingUser user, DateOnly start)
    {
        var reading = _catalog.Add(HabitSamples.Reading());
        await using var scope = Services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<GoalService>().CreateAsync(
            user, new GoalInput { Habit = new GoalHabit(reading.Id, HabitKind.Catalog), Target = MinutesDaily(15), StartDate = start }, CancellationToken.None);
        Assert.Equal(GoalCreateOutcome.Created, result.Outcome);
        return result.GoalId!.Value;
    }

    /// <summary>Ends the goal through the store, as ending will: on its current date, or today when it has not started.</summary>
    private async Task EndAsync(ActingUser user, GoalId goal)
    {
        var owner = user.AccountId!.Value;
        var read = (await _goals.ReadOwnerGoalAsync(owner, goal, CancellationToken.None))!;
        var stored = read.Goal!.Goal;
        var endingDate = GoalTimeline.CurrentDate(stored, Now) ?? NamedTimeZone.Utc.DateAt(Now);
        var effect = GoalTimeline.EndingOf(stored, endingDate);
        var write = new GoalEndingWrite(goal, stored.Attempt.Number, new GoalEnding(endingDate, Outcome.Abandoned, null), effect.LastDate, effect.KeptVersions);
        Assert.Equal(GoalChangeWriteOutcome.Written, await _goals.TryEndAsync(owner, read.StoredTimeZone, read.GoalsRevision, write, CancellationToken.None));
    }

    private IRenderedComponent<EditGoalSettings> RenderPage(GoalId id) => Render<EditGoalSettings>(p => p.Add(x => x.Id, id.Value));

    private static IElement Button(IRenderedComponent<EditGoalSettings> cut, string text) =>
        cut.FindAll("button").First(b => b.TextContent.Trim() == text);

    private static List<string> Errors(IRenderedComponent<EditGoalSettings> cut) =>
        cut.FindAll(".ht-field__error").Select(e => e.TextContent).ToList();

    [Fact]
    public async Task HP_002_the_owner_gets_the_back_link_the_heading_the_filled_form_and_the_line_with_the_date()
    {
        var user = SignIn();
        var goal = await CreateAsync(user, Today);

        var cut = RenderPage(goal);

        Assert.Equal(0, _notFound);
        var back = cut.Find("a.ht-back");
        Assert.Equal("← Reading, 15 min daily", back.TextContent);
        Assert.Equal($"/goals/{goal.Value}", back.GetAttribute("href"));
        Assert.Equal("Edit settings", cut.Find(".ht-page-head h1").TextContent);
        Assert.Empty(cut.FindAll(".ht-page-head .ht-muted"));
        Assert.Equal("15", cut.Find("#edit-goal-amount").GetAttribute("value"));
        Assert.True(string.IsNullOrEmpty(cut.Find("#edit-goal-name").GetAttribute("value")));
        Assert.Equal("Changes apply from Wed 18 Nov. Past results keep their rules.", cut.Find(".ht-notice--neutral").TextContent);
        Assert.Equal("Reading: at least 15 min daily, starting Wed 18 Nov.", cut.Find("aside.ht-summary .ht-card__title").TextContent);
        Assert.Equal($"/goals/{goal.Value}", cut.Find(".ht-form-actions a").GetAttribute("href"));
        Assert.Single(JSInterop.VerifyFocusAsyncInvoke(1));
    }

    [Fact]
    public async Task HP_002_the_form_starts_from_the_later_settings_and_the_own_name()
    {
        var user = SignIn();
        var goal = await CreateAsync(user, Today);
        await using (var scope = Services.CreateAsyncScope())
        {
            var changed = await scope.ServiceProvider.GetRequiredService<GoalChangeService>().ChangeSettingsAsync(
                user, goal, new GoalSettingsInput { Target = MinutesDaily(25), Name = "Morning pages" }, CancellationToken.None);
            Assert.Equal(SettingsChangeOutcome.Changed, changed.Outcome);
        }

        var cut = RenderPage(goal);

        Assert.Equal("← Morning pages", cut.Find("a.ht-back").TextContent);
        Assert.Equal("25", cut.Find("#edit-goal-amount").GetAttribute("value"));
        Assert.Equal("Morning pages", cut.Find("#edit-goal-name").GetAttribute("value"));
    }

    [Fact]
    public async Task US1_AS15_Save_changes_writes_the_change_and_returns_to_the_goal_page()
    {
        var user = SignIn();
        var goal = await CreateAsync(user, Today);
        var cut = RenderPage(goal);

        cut.Find("#edit-goal-amount").Change("20");
        Button(cut, "Save changes").Click();

        Assert.EndsWith($"/goals/{goal.Value}", Navigation.Uri, StringComparison.Ordinal);
        var versions = _goals.Goals.Single(g => g.Id == goal).Versions;
        Assert.Equal([(Today, 15), (Today.AddDays(1), 20)], versions.Select(v => (v.AppliesFrom, v.Settings.Target.Amount!.Value)));
    }

    [Fact]
    public async Task Settings_unchanged_also_returns_to_the_goal_page_and_writes_nothing()
    {
        var user = SignIn();
        var goal = await CreateAsync(user, Today);
        var cut = RenderPage(goal);
        var writes = _goals.ChangeWriteCount;

        Button(cut, "Save changes").Click();

        Assert.EndsWith($"/goals/{goal.Value}", Navigation.Uri, StringComparison.Ordinal);
        Assert.Equal(writes, _goals.ChangeWriteCount);
    }

    [Fact]
    public async Task HP_002_each_refusal_shows_under_its_field_one_at_a_time_with_the_results_message()
    {
        var user = SignIn();
        var goal = await CreateAsync(user, Today);
        var cut = RenderPage(goal);

        cut.Find("#edit-goal-amount").Change("");
        Button(cut, "Save changes").Click();
        Assert.Equal(["Enter a whole number from 1 to 999."], Errors(cut));
        Assert.Equal("true", cut.Find("#edit-goal-amount").GetAttribute("aria-invalid"));

        cut.Find("#edit-goal-amount").Change("20");
        cut.Find("#edit-goal-name").Change(new string('n', 61));
        Button(cut, "Save changes").Click();
        Assert.Equal(["The name can have at most 60 characters."], Errors(cut));
        Assert.Equal("true", cut.Find("#edit-goal-name").GetAttribute("aria-invalid"));
        Assert.Null(cut.Find("#edit-goal-amount").GetAttribute("aria-invalid"));
        Assert.Single(_goals.Goals.Single(g => g.Id == goal).Versions);
    }

    [Fact]
    public async Task A_goal_ended_while_the_page_was_open_shows_This_goal_has_ended_above_the_form()
    {
        var user = SignIn();
        var goal = await CreateAsync(user, Today);
        var cut = RenderPage(goal);
        await EndAsync(user, goal);

        cut.Find("#edit-goal-amount").Change("20");
        Button(cut, "Save changes").Click();

        var alert = cut.Find("form p.ht-field__error[role=alert]");
        Assert.Equal("This goal has ended.", alert.TextContent);
        Assert.Equal(["This goal has ended."], Errors(cut));
    }

    [Fact]
    public async Task HP_002_an_ended_goal_returns_to_its_goal_page()
    {
        var user = SignIn();
        var goal = await CreateAsync(user, Today);
        await EndAsync(user, goal);

        var cut = RenderPage(goal);

        Assert.EndsWith($"/goals/{goal.Value}", Navigation.Uri, StringComparison.Ordinal);
        Assert.Empty(cut.FindAll("form"));
        Assert.Equal(0, _notFound);
    }

    [Fact]
    public async Task HP_002_another_accounts_goal_an_unknown_goal_and_an_administrator_get_the_Not_found_page()
    {
        var owner = SignIn();
        var goal = await CreateAsync(owner, Today);

        SignIn();
        var other = RenderPage(goal);
        var unknown = RenderPage(new GoalId(Guid.NewGuid()));
        SignIn("Administrator");
        var administrator = RenderPage(goal);

        Assert.Equal(3, _notFound);
        foreach (var cut in new[] { other, unknown, administrator })
        {
            Assert.Empty(cut.FindAll("form"));
            Assert.DoesNotContain("Reading", cut.Markup, StringComparison.Ordinal);
        }
    }
}
