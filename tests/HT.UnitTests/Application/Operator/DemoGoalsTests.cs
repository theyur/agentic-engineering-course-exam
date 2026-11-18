using HT.Application.Operator;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Time;

namespace HT.UnitTests.Application.Operator;

/// <summary>
/// Slice 4 FR-040, research R-10: the demo goals and the demo account profiles that demo loading passes to the store.
/// </summary>
public sealed class DemoGoalsTests
{
    private const string Demo1 = "demo1@example.com";
    private const string Demo2 = "demo2@example.com";
    private const string Demo3 = "demo3@example.com";

    private static readonly DateOnly Today = new(2026, 11, 18);

    private static readonly DayOfWeek[] Weekdays =
        [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday];

    private static readonly DayOfWeek[] EveryDay = [.. Weekdays, DayOfWeek.Saturday, DayOfWeek.Sunday];

    private static readonly string[] ForbiddenCustomHabits = ["Cold shower", "Evening stretch", "Plan tomorrow"];

    [Fact]
    public void Demo1_has_Reading_on_weekdays_and_No_phone_in_bed_every_day()
    {
        var goals = DemoGoals.For(Demo1, Today);

        Assert.Equal(2, goals.Count);
        AssertCatalogGoal(goals[0], "Reading", [CatalogHabitState.Published], new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = 15, DurationUnit = DurationUnit.Minutes,
            ScheduleKind = ScheduleKind.FixedDays, Days = Weekdays.ToHashSet(),
        }, null, Today);
        AssertCustomGoal(goals[1], Demo1, "No phone in bed", new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Reduce, Measurement = Measurement.YesNo,
            ScheduleKind = ScheduleKind.FixedDays, Days = EveryDay.ToHashSet(),
        }, Today);
    }

    [Fact]
    public void Demo2_has_Morning_yoga_three_times_a_week_and_Meditation_starting_7_days_later()
    {
        var goals = DemoGoals.For(Demo2, Today);

        Assert.Equal(2, goals.Count);
        AssertCatalogGoal(goals[0], "Yoga", [CatalogHabitState.Published], new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo,
            ScheduleKind = ScheduleKind.Times, Times = 3, Period = SchedulePeriod.Week,
        }, "Morning yoga", Today);
        AssertCatalogGoal(goals[1], "Meditation", [CatalogHabitState.Published], new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = 10, DurationUnit = DurationUnit.Minutes,
            ScheduleKind = ScheduleKind.FixedDays, Days = new HashSet<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
        }, null, new DateOnly(2026, 11, 25));
    }

    [Fact]
    public void Demo3_has_Sugar_Late_snacks_and_Coffee_which_also_accepts_an_Unpublished_habit()
    {
        var goals = DemoGoals.For(Demo3, Today);

        Assert.Equal(3, goals.Count);
        AssertCatalogGoal(goals[0], "Sugar", [CatalogHabitState.Published], new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Reduce, Measurement = Measurement.Count, Amount = 1, CountUnit = "sweet",
            ScheduleKind = ScheduleKind.Times, Times = 1, Period = SchedulePeriod.Day,
        }, null, Today);
        AssertCustomGoal(goals[1], Demo3, "Late snacks", new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Reduce, Measurement = Measurement.YesNo,
            ScheduleKind = ScheduleKind.Times, Times = 1, Period = SchedulePeriod.Day,
        }, Today);
        AssertCatalogGoal(goals[2], "Coffee", [CatalogHabitState.Published, CatalogHabitState.Unpublished], new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Reduce, Measurement = Measurement.Count, Amount = 2, CountUnit = "cups",
            ScheduleKind = ScheduleKind.Times, Times = 1, Period = SchedulePeriod.Day,
        }, null, Today);
    }

    [Theory]
    [InlineData(Demo1)]
    [InlineData(Demo2)]
    [InlineData(Demo3)]
    public void Every_demo_goal_passes_the_goal_checks_on_the_day_of_the_run(string email)
    {
        Assert.All(DemoGoals.For(email, Today), goal =>
        {
            var problem = GoalDraft.TryCreate(FieldsOf(goal.Target), goal.StartDate, Today, goal.OwnName?.Value, out var target, out var start, out var ownName);

            Assert.Equal(GoalProblem.None, problem);
            Assert.Equal(goal.Target, target);
            Assert.Equal(goal.StartDate, start);
            Assert.Equal(goal.OwnName, ownName);
        });
    }

    [Fact]
    public void The_display_names_are_those_of_FR_040_under_each_account()
    {
        Assert.Equal(["Reading, 15 min, 5 days a week", "No phone in bed, avoided every day"], DemoGoals.DisplayNames(Demo1));
        Assert.Equal(["Morning yoga", "Meditation, 10 min, 3 days a week"], DemoGoals.DisplayNames(Demo2));
        Assert.Equal(
            ["Sugar, at most 1 sweet a day", "Late snacks, at most once a day", "Coffee, at most 2 cups a day"],
            DemoGoals.DisplayNames(Demo3));
    }

    [Theory]
    [InlineData(Demo1)]
    [InlineData(Demo2)]
    [InlineData(Demo3)]
    public void The_display_names_follow_the_goals_and_their_target_texts(string email)
    {
        var goals = DemoGoals.For(email, Today);

        Assert.Equal(
            goals.Select(g => g.OwnName?.Value ?? HabitNameOf(email, g) + ", " + GoalText.TargetText(g.Target)),
            DemoGoals.DisplayNames(email));
    }

    [Fact]
    public void The_profile_of_demo1_has_the_name_Demo_One_and_demo2_and_demo3_have_none()
    {
        Assert.True(NamedTimeZone.TryParse("Europe/Kyiv", out var kyiv));
        var createdAt = new DateTimeOffset(2026, 11, 18, 10, 0, 0, TimeSpan.Zero);

        var demo1 = DemoAccounts.ProfileFor(Demo1, kyiv, createdAt);

        Assert.Equal("Demo One", demo1.Name);
        Assert.Equal(kyiv, demo1.TimeZone);
        Assert.Equal(createdAt, demo1.CreatedAt);
        Assert.Equal("Demo One", DemoAccounts.NameFor(Demo1));
        Assert.Null(DemoAccounts.ProfileFor(Demo2, kyiv, createdAt).Name);
        Assert.Null(DemoAccounts.ProfileFor(Demo3, kyiv, createdAt).Name);
        Assert.Null(DemoAccounts.NameFor(Demo2));
        Assert.Null(DemoAccounts.NameFor(Demo3));
    }

    [Theory]
    [InlineData(Demo1)]
    [InlineData(Demo2)]
    [InlineData(Demo3)]
    public void No_demo_goal_uses_a_custom_habit_that_slice_2_deletes_after_demo_loading(string email)
    {
        Assert.All(DemoGoals.For(email, Today), goal =>
            Assert.DoesNotContain(HabitNameOf(email, goal), ForbiddenCustomHabits));
    }

    [Fact]
    public void Another_email_is_a_programming_error()
    {
        Assert.Throws<ArgumentException>(() => DemoGoals.For("someone@example.com", Today));
        Assert.Throws<ArgumentException>(() => DemoGoals.DisplayNames("DEMO1@example.com"));
        Assert.Throws<ArgumentException>(() => DemoAccounts.NameFor("someone@example.com"));
    }

    [Fact]
    public void A_demo_profile_refuses_a_name_that_fails_the_account_name_checks_and_a_default_zone()
    {
        var createdAt = new DateTimeOffset(2026, 11, 18, 10, 0, 0, TimeSpan.Zero);

        Assert.Throws<ArgumentException>(() => new DemoAccountProfile(" Demo One", NamedTimeZone.Utc, createdAt));
        Assert.Throws<ArgumentException>(() => new DemoAccountProfile(new string('x', 61), NamedTimeZone.Utc, createdAt));
        Assert.Throws<ArgumentException>(() => new DemoAccountProfile(null, default, createdAt));
    }

    private static void AssertCatalogGoal(
        DemoGoal goal, string habitName, CatalogHabitState[] states, GoalTargetFields fields, string? ownName, DateOnly start)
    {
        Assert.Equal(habitName, goal.CatalogHabitName);
        Assert.Null(goal.CustomHabitPosition);
        Assert.True(goal.AcceptedStates.SetEquals(states));
        Assert.Equal(TargetOf(fields), goal.Target);
        Assert.Equal(ownName, goal.OwnName?.Value);
        Assert.Equal(start, goal.StartDate);
    }

    private static void AssertCustomGoal(DemoGoal goal, string email, string habitName, GoalTargetFields fields, DateOnly start)
    {
        Assert.Null(goal.CatalogHabitName);
        Assert.Empty(goal.AcceptedStates);
        Assert.NotNull(goal.CustomHabitPosition);
        Assert.Equal(habitName, DemoCustomHabits.For(email)[goal.CustomHabitPosition.Value].Name.Value);
        Assert.Equal(TargetOf(fields), goal.Target);
        Assert.Null(goal.OwnName);
        Assert.Equal(start, goal.StartDate);
    }

    private static string HabitNameOf(string email, DemoGoal goal) =>
        goal.CatalogHabitName ?? DemoCustomHabits.For(email)[goal.CustomHabitPosition!.Value].Name.Value;

    private static GoalTarget TargetOf(GoalTargetFields fields)
    {
        Assert.True(GoalTarget.TryCreate(fields, out var target, out var problem), problem.ToString());
        return target;
    }

    private static GoalTargetFields FieldsOf(GoalTarget target) => new()
    {
        BuildOrReduce = target.BuildOrReduce,
        Measurement = target.Measurement,
        Amount = target.Amount,
        CountUnit = target.CountUnit,
        DurationUnit = target.DurationUnit,
        ScheduleKind = target.Schedule.Kind,
        Days = target.Schedule.Kind == ScheduleKind.FixedDays ? target.Schedule.Days.ToHashSet() : null,
        Times = target.Schedule.Times,
        Period = target.Schedule.Period,
    };
}
