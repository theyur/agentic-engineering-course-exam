using Bunit;
using Bunit.TestDoubles;
using HT.Application.Goals;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Time;
using HT.Ui.Components.Pages;
using HT.UnitTests.Fakes;
using static HT.UnitTests.Fakes.HabitSamples;

namespace HT.UiTests.Components.Pages;

/// <summary>
/// US2-AS6, US2-AS7, HP-005 (slice 2): the slice-2 state of the detail page for visitors and administrators; slice 4
/// US6-AS3, US6-AS5, HP-006: the registered user's state.
/// </summary>
public sealed class CatalogHabitDetailTests : BunitContext
{
    private readonly BunitAuthorizationContext _authorization;
    private readonly FakeCatalogHabitStore _store;
    private readonly FakeGoalStore _goals;

    public CatalogHabitDetailTests()
    {
        _authorization = AddAuthorization();
        _store = this.AddCatalog();
        _goals = this.AddGoals();
    }

    private IRenderedComponent<CatalogHabitDetail> RenderHabit(CatalogHabit habit) =>
        Render<CatalogHabitDetail>(p => p.Add(x => x.Id, habit.Id.Value));

    private CatalogHabit Reading() => _store.Add(Catalog("Reading", category: Category.Mind, icon: "book-open", description: "Books and articles.",
        goals: Goals(1, DurationDays(15, DurationUnit.Minutes, Weekdays), Count(20, "pages", 3, SchedulePeriod.Week))));

    /// <summary>Stores a goal of <paramref name="owner"/> on <paramref name="habit"/> directly, with its own name or none.</summary>
    private Goal StoreGoal(AccountId owner, CatalogHabit habit, string? name = null)
    {
        var start = new DateOnly(2026, 11, 18);
        Assert.True(GoalTarget.TryCreate(new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo, ScheduleKind = ScheduleKind.Times, Times = 3, Period = SchedulePeriod.Week,
        }, out var target, out _));
        GoalName? ownName = null;
        if (name is not null)
        {
            Assert.True(GoalName.TryCreate(name, out ownName, out _));
        }

        return _goals.Add(owner,
            new Goal(new GoalId(Guid.NewGuid()), owner, new GoalHabit(habit.Id, HabitKind.Catalog), start, ownName, new GoalAttempt(1, start),
                [new GoalSettingsVersion(new GoalSettings(target, NamedTimeZone.Utc), start)]),
            new GoalHabitView(habit.Id, HabitKind.Catalog, habit.Definition.Name.Value, habit.Definition.Category, true));
    }

    [Fact]
    public void US2_AS6_a_visitor_sees_the_shipped_parts_and_the_register_button()
    {
        var habit = Reading();
        StoreGoal(new AccountId(Guid.NewGuid()), habit);

        var cut = RenderHabit(habit);

        var back = cut.Find("a.ht-back");
        Assert.Equal("← Catalog", back.TextContent);
        Assert.Equal("/catalog", back.GetAttribute("href"));
        Assert.NotNull(cut.Find(".ht-glyph-lg svg"));
        Assert.Equal("Reading", cut.Find("h1").TextContent);
        Assert.Equal(["Mind", "Build"], cut.FindAll(".ht-page-head .ht-chip").Select(c => c.TextContent));
        Assert.Equal("Books and articles.", cut.Find(".ht-lead").TextContent);
        Assert.Equal("Suggested starting goals", cut.Find(".ht-card__title").TextContent);
        Assert.Equal("Examples to adjust, not prescribed targets.", cut.Find(".ht-card__sub").TextContent);
        Assert.Equal(["15 min, 5 days a week", "20 pages, 3 times a week"], cut.FindAll(".ht-row__name").Select(n => n.TextContent));
        Assert.Equal(["Duration · fixed days Mon–Fri", "Count · flexible"], cut.FindAll(".ht-row__meta").Select(n => n.TextContent));
        var button = cut.Find(".ht-page-head a.ht-btn");
        Assert.Equal("Register to use this habit", button.TextContent);
        Assert.Equal("/register", button.GetAttribute("href"));
        AssertNoLaterSlices(cut);
    }

    [Fact]
    public void US2_AS7_an_administrator_sees_the_same_content_without_the_head_button()
    {
        _authorization.SignIn("Administrator", "admin@example.com");
        var habit = Reading();
        StoreGoal(new AccountId(Guid.NewGuid()), habit);

        var cut = RenderHabit(habit);

        Assert.Equal("Reading", cut.Find("h1").TextContent);
        Assert.Equal(2, cut.FindAll(".ht-row").Count);
        Assert.Empty(cut.FindAll(".ht-page-head .ht-btn"));
        AssertNoLaterSlices(cut);
    }

    [Fact]
    public void US6_AS5_a_registered_user_with_goals_sees_Use_this_habit_the_Tag_Start_with_this_and_their_goals()
    {
        var owner = new AccountId(_authorization.SignIn("RegisteredUser"));
        var habit = Reading();
        var derived = StoreGoal(owner, habit);
        var named = StoreGoal(owner, habit, "Morning pages");
        StoreGoal(new AccountId(Guid.NewGuid()), habit, "Another account's goal");
        var newGoal = $"/goals/new?habit={habit.Id.Value:D}&kind=catalog";

        var cut = RenderHabit(habit);

        var button = cut.Find(".ht-page-head a.ht-btn");
        Assert.Equal("Use this habit", button.TextContent);
        Assert.Equal(newGoal, button.GetAttribute("href"));
        Assert.Contains("ht-btn--primary", button.ClassList);
        var tags = cut.FindAll(".ht-page-head .ht-chip");
        Assert.Equal(["Mind", "Build", "You track this · 2 goals"], tags.Select(c => c.TextContent));
        Assert.Contains("ht-chip--strong", tags[2].ClassList);
        var starts = cut.FindAll(".ht-row a.ht-btn");
        Assert.Equal(["Start with this", "Start with this"], starts.Select(a => a.TextContent));
        Assert.Equal([newGoal + "&suggestion=1", newGoal + "&suggestion=2"], starts.Select(a => a.GetAttribute("href")));
        Assert.All(starts, a => Assert.Contains("ht-btn--secondary", a.ClassList));
        Assert.Equal(["Suggested starting goals", "Your goals on this habit"], cut.FindAll(".ht-card__title").Select(t => t.TextContent));
        var goals = cut.FindAll(".ht-goal a.ht-goal__name");
        Assert.Equal(["Morning pages", "Reading, 3 times a week"], goals.Select(a => a.TextContent));
        Assert.Equal([$"/goals/{named.Id.Value:D}", $"/goals/{derived.Id.Value:D}"], goals.Select(a => a.GetAttribute("href")));
        Assert.DoesNotContain("Another account", cut.Markup, StringComparison.Ordinal);
        foreach (var absent in new[] { "Community", "contributors", "streak", "Streak", "consistency" })
        {
            Assert.DoesNotContain(absent, cut.Markup, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void US6_AS5_one_goal_is_You_track_this_1_goal()
    {
        var owner = new AccountId(_authorization.SignIn("RegisteredUser"));
        var habit = Reading();
        StoreGoal(owner, habit);

        var cut = RenderHabit(habit);

        Assert.Equal("You track this · 1 goal", cut.FindAll(".ht-page-head .ht-chip")[2].TextContent);
    }

    [Fact]
    public void US6_AS5_a_registered_user_without_goals_has_no_Tag_and_no_Your_goals_card()
    {
        _authorization.SignIn("RegisteredUser");
        var habit = Reading();
        StoreGoal(new AccountId(Guid.NewGuid()), habit);

        var cut = RenderHabit(habit);

        Assert.Equal("Use this habit", cut.Find(".ht-page-head a.ht-btn").TextContent);
        Assert.Equal(["Mind", "Build"], cut.FindAll(".ht-page-head .ht-chip").Select(c => c.TextContent));
        Assert.Equal(2, cut.FindAll(".ht-row a.ht-btn").Count);
        Assert.DoesNotContain("Your goals on this habit", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("You track this", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void A_habit_without_suggested_starting_goals_has_no_such_card()
    {
        var cut = RenderHabit(_store.Add(Catalog("Walking", category: Category.Body, icon: "footprints")));

        Assert.Equal("Walking", cut.Find("h1").TextContent);
        Assert.Empty(cut.FindAll(".ht-card"));
        Assert.Empty(cut.FindAll(".ht-lead"));
    }

    [Fact]
    public void A_registered_user_with_goals_on_a_habit_without_suggestions_gets_only_the_Your_goals_card()
    {
        var owner = new AccountId(_authorization.SignIn("RegisteredUser"));
        var walking = _store.Add(Catalog("Walking", category: Category.Body, icon: "footprints"));
        StoreGoal(owner, walking);

        var cut = RenderHabit(walking);

        Assert.Equal(["Your goals on this habit"], cut.FindAll(".ht-card__title").Select(t => t.TextContent));
        Assert.Equal(["Walking, 3 times a week"], cut.FindAll(".ht-goal__name").Select(a => a.TextContent));
    }

    private static void AssertNoLaterSlices(IRenderedComponent<CatalogHabitDetail> cut)
    {
        foreach (var absent in new[] { "Your goals", "Start with this", "Use this habit", "You track this", "Community", "contributors" })
        {
            Assert.DoesNotContain(absent, cut.Markup, StringComparison.Ordinal);
        }
    }
}
