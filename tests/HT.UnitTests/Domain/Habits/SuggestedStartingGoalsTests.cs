using HT.Domain.Habits;
using static HT.UnitTests.Domain.Habits.GoalFields;

namespace HT.UnitTests.Domain.Habits;

/// <summary>FR-006: 0 to 5 in order, exactly one mark when not empty, list checks before item checks.</summary>
public class SuggestedStartingGoalsTests
{
    private static readonly SuggestedStartingGoalFields Valid = YesNoTimes(1, SchedulePeriod.Day);
    private static readonly SuggestedStartingGoalFields Invalid = Valid with { Measurement = null };

    private static (SuggestedStartingGoalFields, bool)[] Items(int count, int? marked) =>
        Enumerable.Range(0, count).Select(i => (Valid with { Times = i + 1 }, i == marked)).ToArray();

    [Fact]
    public void An_empty_list_has_no_mark()
    {
        Assert.True(SuggestedStartingGoals.TryCreate([], out var goals, out var problem));
        Assert.Equal(SuggestedStartingGoalsProblem.None, problem);
        Assert.Empty(goals.Items);
        Assert.Null(goals.MarkedIndex);
        Assert.Null(goals.Marked);
    }

    [Fact]
    public void Five_with_one_mark_are_accepted_and_six_are_too_many_before_the_mark()
    {
        Assert.True(SuggestedStartingGoals.TryCreate(Items(5, 4), out var five, out _));
        Assert.Equal(5, five.Count);
        Assert.Equal(4, five.MarkedIndex);
        Assert.Same(five.Items[4], five.Marked);

        Assert.False(SuggestedStartingGoals.TryCreate(Items(6, null), out var six, out var problem));
        Assert.Null(six);
        Assert.Equal(SuggestedStartingGoalsProblemKind.TooMany, problem.Kind);
    }

    [Fact]
    public void A_non_empty_list_needs_exactly_one_mark()
    {
        Assert.False(SuggestedStartingGoals.TryCreate(Items(2, null), out _, out var none));
        Assert.Equal(SuggestedStartingGoalsProblemKind.MarkMissing, none.Kind);

        Assert.False(SuggestedStartingGoals.TryCreate([(Valid, true), (Valid, true)], out _, out var both));
        Assert.Equal(SuggestedStartingGoalsProblemKind.MarkMissing, both.Kind);
    }

    [Fact]
    public void The_mark_is_checked_before_the_items()
    {
        Assert.False(SuggestedStartingGoals.TryCreate([(Invalid, false), (Invalid, false)], out _, out var problem));
        Assert.Equal(SuggestedStartingGoalsProblemKind.MarkMissing, problem.Kind);
    }

    [Fact]
    public void An_item_problem_has_its_position_from_1()
    {
        Assert.False(SuggestedStartingGoals.TryCreate([(Valid, true), (Invalid, false)], out _, out var problem));
        Assert.Equal(SuggestedStartingGoalsProblemKind.Item, problem.Kind);
        Assert.Equal(SuggestedStartingGoalProblem.MeasurementMissing, problem.ItemProblem);
        Assert.Equal(2, problem.Position);
    }

    [Fact]
    public void Problems_in_items_2_and_3_give_the_first()
    {
        var third = Valid with { BuildOrReduce = null };
        Assert.False(SuggestedStartingGoals.TryCreate([(Valid, true), (Invalid, false), (third, false)], out _, out var problem));
        Assert.Equal(2, problem.Position);
        Assert.Equal(SuggestedStartingGoalProblem.MeasurementMissing, problem.ItemProblem);
    }

    [Fact]
    public void Keeps_the_order()
    {
        Assert.True(SuggestedStartingGoals.TryCreate(Items(3, 1), out var goals, out _));
        Assert.Equal([1, 2, 3], goals.Items.Select(g => g.Schedule.Times!.Value));
        Assert.Equal(1, goals.MarkedIndex);
        Assert.Equal(2, goals.Marked!.Schedule.Times);
    }
}
