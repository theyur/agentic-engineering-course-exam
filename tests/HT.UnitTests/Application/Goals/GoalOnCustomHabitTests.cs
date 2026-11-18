using HT.Application.Goals;
using HT.Application.Habits;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.UnitTests.Fakes;
using static HT.UnitTests.Application.Goals.GoalInputs;
using static HT.UnitTests.Domain.Goals.GoalSamples;

namespace HT.UnitTests.Application.Goals;

/// <summary>
/// US5 (FR-007, FR-008, FR-010, FR-035): a goal on one's own custom habit, from its starting values to its name, and
/// an edit of the habit that renames the goal without touching its settings.
/// </summary>
public sealed class GoalOnCustomHabitTests
{
    private readonly GoalWorld _world = new();

    private CustomHabitService CustomHabits => new(_world.CustomHabits, _world.Goals);

    private CustomHabit ColdShower(ActingUser user) =>
        _world.CustomHabits.Add(user.AccountId!.Value, HabitSamples.Definition("Cold shower", Category.Body, Intent.Build, "droplet"));

    private async Task<GoalId> CreatedDailyAsync(ActingUser user, CustomHabit habit, string? name = null)
    {
        var result = await _world.Service.CreateAsync(user, Input(Custom(habit), GoalWorld.Today, YesNoDays(BuildOrReduce.Build, AllDays), name), CancellationToken.None);
        Assert.Equal(GoalCreateOutcome.Created, result.Outcome);
        return result.GoalId!.Value;
    }

    private async Task<GoalDetails> ReadAsync(ActingUser user, GoalId id)
    {
        var result = await _world.Service.GetAsync(user, id, CancellationToken.None);
        Assert.Equal(GoalReadOutcome.Found, result.Outcome);
        return result.Goal!;
    }

    private static HabitInput Edited(string name, Intent intent) =>
        new() { Name = name, Category = Category.Body, Intent = intent, Icon = "droplet" };

    [Fact]
    public async Task US5_AS1_the_starting_values_are_the_custom_habit_Build_and_today_and_nothing_else()
    {
        var user = RegisteredUser(_world.Accounts);
        var habit = ColdShower(user);

        var result = await _world.Service.StartNewGoalAsync(user, Custom(habit), null, CancellationToken.None);

        Assert.Equal(GoalStartOutcome.Started, result.Outcome);
        var values = result.StartingValues!;
        Assert.Equal(new GoalHabitView(habit.Id, HabitKind.Custom, "Cold shower", Category.Body, null) { Icon = "droplet" }, values.Habit);
        Assert.Equal(BuildOrReduce.Build, values.BuildOrReduce);
        Assert.Equal(GoalWorld.Today, values.StartDate);
        Assert.Null(values.Measurement);
        Assert.Null(values.Amount);
        Assert.Null(values.CountUnit);
        Assert.Null(values.DurationUnit);
        Assert.Null(values.ScheduleKind);
        Assert.Empty(values.Days);
        Assert.Null(values.Times);
        Assert.Null(values.Period);
        Assert.Null(values.Name);
        Assert.Null(values.CatalogSuggestionText);
    }

    [Fact]
    public async Task US5_AS2_a_yes_no_goal_on_all_seven_days_is_based_on_the_custom_habit_and_named_Cold_shower_daily()
    {
        var user = RegisteredUser(_world.Accounts);
        var habit = ColdShower(user);

        var id = await CreatedDailyAsync(user, habit);

        var stored = Assert.Single(_world.Goals.Goals);
        Assert.Equal(new GoalHabit(habit.Id, HabitKind.Custom), stored.Habit);
        var goal = await ReadAsync(user, id);
        Assert.Equal("Cold shower, daily", goal.Name);
        Assert.False(goal.HasOwnName);
        Assert.Equal((habit.Id, HabitKind.Custom, "Cold shower", (bool?)null), (goal.Habit.Id, goal.Habit.Kind, goal.Habit.Name, goal.Habit.IsInCatalog));
        var listed = Assert.Single((await _world.Service.ListAsync(user, CancellationToken.None)).Goals);
        Assert.Equal(("Cold shower, daily", "Cold shower", HabitKind.Custom), (listed.Name, listed.HabitName, listed.HabitKind));
    }

    [Fact]
    public async Task US5_AS5_a_rename_renames_the_goal_and_neither_it_nor_a_change_of_intent_changes_the_settings()
    {
        var user = RegisteredUser(_world.Accounts);
        var habit = ColdShower(user);
        var derived = await CreatedDailyAsync(user, habit);
        var named = await CreatedDailyAsync(user, habit, "Zq My own name");
        var before = _world.Goals.Goals.Single(g => g.Id == derived);

        var renamed = await CustomHabits.EditAsync(user, habit.Id, Edited("Cold water", Intent.Build), CancellationToken.None);
        Assert.Equal(CustomHabitEditOutcome.Saved, renamed.Outcome);

        var afterRename = await ReadAsync(user, derived);
        Assert.Equal("Cold water, daily", afterRename.Name);
        Assert.Equal("Cold water", afterRename.Habit.Name);
        Assert.Equal("Zq My own name", (await ReadAsync(user, named)).Name);

        var reduced = await CustomHabits.EditAsync(user, habit.Id, Edited("Cold water", Intent.Reduce), CancellationToken.None);
        Assert.Equal(CustomHabitEditOutcome.Saved, reduced.Outcome);

        var afterIntent = await ReadAsync(user, derived);
        Assert.Equal(BuildOrReduce.Build, afterIntent.BuildOrReduce);
        Assert.Equal(Measurement.YesNo, afterIntent.Measurement);
        Assert.Equal(ScheduleKind.FixedDays, afterIntent.ScheduleKind);
        Assert.True(afterIntent.Days.SetEquals(AllDays));
        Assert.Equal("Cold water, daily", afterIntent.Name);
        var after = _world.Goals.Goals.Single(g => g.Id == derived);
        Assert.Equal(before.StartDate, after.StartDate);
        Assert.Equal(before.Attempt, after.Attempt);
        Assert.Equal(before.Versions, after.Versions);
    }
}
