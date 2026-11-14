using HT.Domain.Habits;
using static HT.UnitTests.Domain.Habits.GoalFields;

namespace HT.UnitTests.Domain.Habits;

/// <summary>FR-007: every row of the check table and both sides of every boundary (constitution II).</summary>
public class SuggestedStartingGoalTests
{
    private static SuggestedStartingGoalProblem Problem(SuggestedStartingGoalFields fields)
    {
        Assert.False(SuggestedStartingGoal.TryCreate(fields, out var goal, out var problem));
        Assert.Null(goal);
        return problem;
    }

    private static readonly SuggestedStartingGoalFields Count = CountTimes(20, "pages", 3, SchedulePeriod.Week);

    [Fact]
    public void Each_check_in_FR_007_order()
    {
        Assert.Equal(SuggestedStartingGoalProblem.BuildOrReduceMissing, Problem(Count with { BuildOrReduce = null }));
        Assert.Equal(SuggestedStartingGoalProblem.MeasurementMissing, Problem(Count with { Measurement = null }));
        Assert.Equal(SuggestedStartingGoalProblem.YesNoHasAmountOrUnit, Problem(YesNoTimes(1, SchedulePeriod.Day) with { Amount = 1 }));
        Assert.Equal(SuggestedStartingGoalProblem.BuildAmountOutOfRange, Problem(Count with { Amount = 0 }));
        Assert.Equal(SuggestedStartingGoalProblem.ReduceAmountOutOfRange, Problem(Count with { BuildOrReduce = BuildOrReduce.Reduce, Amount = -1 }));
        Assert.Equal(SuggestedStartingGoalProblem.CountUnitMissing, Problem(Count with { CountUnit = null }));
        Assert.Equal(SuggestedStartingGoalProblem.CountUnitTooLong, Problem(Count with { CountUnit = new string('u', 21) }));
        Assert.Equal(SuggestedStartingGoalProblem.DurationUnitMissing, Problem(DurationTimes(15, null, 1, SchedulePeriod.Day)));
        Assert.Equal(SuggestedStartingGoalProblem.ScheduleMissing, Problem(Count with { ScheduleKind = null }));
        Assert.Equal(SuggestedStartingGoalProblem.DaysMissing, Problem(CountDays(10, "pages")));
        Assert.Equal(SuggestedStartingGoalProblem.TimesOutOfRange, Problem(Count with { Times = 0 }));
        Assert.Equal(SuggestedStartingGoalProblem.PeriodMissing, Problem(Count with { Period = null }));
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(999, true)]
    [InlineData(1000, false)]
    public void Build_amount_from_1_to_999(int amount, bool accepted)
    {
        var fields = Count with { Amount = amount };
        Assert.Equal(accepted, SuggestedStartingGoal.TryCreate(fields, out _, out var problem));
        Assert.Equal(accepted ? SuggestedStartingGoalProblem.None : SuggestedStartingGoalProblem.BuildAmountOutOfRange, problem);
    }

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(999, true)]
    [InlineData(1000, false)]
    public void Reduce_amount_from_0_to_999(int amount, bool accepted)
    {
        var fields = Count with { BuildOrReduce = BuildOrReduce.Reduce, Amount = amount };
        Assert.Equal(accepted, SuggestedStartingGoal.TryCreate(fields, out _, out var problem));
        Assert.Equal(accepted ? SuggestedStartingGoalProblem.None : SuggestedStartingGoalProblem.ReduceAmountOutOfRange, problem);
    }

    [Fact]
    public void A_missing_amount_is_out_of_range_for_its_side()
    {
        Assert.Equal(SuggestedStartingGoalProblem.BuildAmountOutOfRange, Problem(Count with { Amount = null }));
        Assert.Equal(SuggestedStartingGoalProblem.ReduceAmountOutOfRange, Problem(Count with { BuildOrReduce = BuildOrReduce.Reduce, Amount = null }));
        Assert.Equal(SuggestedStartingGoalProblem.BuildAmountOutOfRange, Problem(DurationTimes(null, DurationUnit.Minutes, 1, SchedulePeriod.Day)));
    }

    [Fact]
    public void Count_unit_from_1_to_20_after_trimming_kept_as_entered()
    {
        Assert.Equal(new string('u', 20), Goal(Count with { CountUnit = " " + new string('u', 20) + " " }).CountUnit);
        Assert.Equal(SuggestedStartingGoalProblem.CountUnitTooLong, Problem(Count with { CountUnit = new string('u', 21) }));
        Assert.Equal(SuggestedStartingGoalProblem.CountUnitMissing, Problem(Count with { CountUnit = "  " }));
        Assert.Equal("pages", Goal(Count with { CountUnit = "  pages  " }).CountUnit);
        Assert.Equal("Pages", Goal(Count with { CountUnit = "Pages" }).CountUnit);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(20, true)]
    [InlineData(21, false)]
    public void Times_from_1_to_20(int times, bool accepted)
    {
        var fields = Count with { Times = times };
        Assert.Equal(accepted, SuggestedStartingGoal.TryCreate(fields, out _, out var problem));
        Assert.Equal(accepted ? SuggestedStartingGoalProblem.None : SuggestedStartingGoalProblem.TimesOutOfRange, problem);
    }

    [Fact]
    public void Missing_times_and_a_missing_period_and_no_days_are_refused()
    {
        Assert.Equal(SuggestedStartingGoalProblem.TimesOutOfRange, Problem(Count with { Times = null }));
        Assert.Equal(SuggestedStartingGoalProblem.PeriodMissing, Problem(Count with { Period = null }));
        Assert.Equal(SuggestedStartingGoalProblem.DaysMissing, Problem(YesNoDays()));
        Assert.Equal(SuggestedStartingGoalProblem.DaysMissing, Problem(YesNoDays() with { Days = null }));
    }

    [Fact]
    public void A_yes_no_with_an_amount_or_a_unit_is_refused()
    {
        var yesNo = YesNoTimes(1, SchedulePeriod.Day);
        Assert.Equal(SuggestedStartingGoalProblem.YesNoHasAmountOrUnit, Problem(yesNo with { Amount = 1 }));
        Assert.Equal(SuggestedStartingGoalProblem.YesNoHasAmountOrUnit, Problem(yesNo with { CountUnit = "x" }));
        Assert.Equal(SuggestedStartingGoalProblem.YesNoHasAmountOrUnit, Problem(yesNo with { DurationUnit = DurationUnit.Minutes }));
    }

    [Fact]
    public void A_yes_no_with_a_blank_count_unit_has_no_unit()
    {
        var goal = Goal(YesNoTimes(1, SchedulePeriod.Day) with { CountUnit = "  " });
        Assert.Null(goal.CountUnit);
        Assert.Null(goal.Amount);
        Assert.Null(goal.DurationUnit);
    }

    [Fact]
    public void Values_of_another_measurement_or_schedule_kind_are_neither_refused_nor_stored()
    {
        var count = Goal(Count with { DurationUnit = DurationUnit.Hours });
        Assert.Null(count.DurationUnit);
        Assert.Equal("pages", count.CountUnit);

        var duration = Goal(DurationTimes(15, DurationUnit.Minutes, 1, SchedulePeriod.Day) with { CountUnit = "pages" });
        Assert.Null(duration.CountUnit);
        Assert.Equal(DurationUnit.Minutes, duration.DurationUnit);

        var times = Goal(Count with { Days = new HashSet<DayOfWeek> { DayOfWeek.Monday } });
        Assert.Empty(times.Schedule.Days);
        Assert.Equal(3, times.Schedule.Times);

        var days = Goal(CountDays(10, "pages", DayOfWeek.Saturday) with { Times = 4, Period = SchedulePeriod.Day });
        Assert.Null(days.Schedule.Times);
        Assert.Null(days.Schedule.Period);
        Assert.Equal([DayOfWeek.Saturday], days.Schedule.Days);
    }

    [Fact]
    public void Gives_the_first_problem_when_several_fields_are_wrong()
    {
        Assert.Equal(SuggestedStartingGoalProblem.BuildOrReduceMissing,
            Problem(new SuggestedStartingGoalFields { Amount = -5, ScheduleKind = ScheduleKind.Times }));
        Assert.Equal(SuggestedStartingGoalProblem.MeasurementMissing,
            Problem(new SuggestedStartingGoalFields { BuildOrReduce = BuildOrReduce.Build, Times = 99 }));
        Assert.Equal(SuggestedStartingGoalProblem.YesNoHasAmountOrUnit,
            Problem(YesNoTimes(0, SchedulePeriod.Day) with { Amount = 3, ScheduleKind = null }));
        Assert.Equal(SuggestedStartingGoalProblem.BuildAmountOutOfRange,
            Problem(Count with { Amount = 0, CountUnit = null, Times = 0 }));
        Assert.Equal(SuggestedStartingGoalProblem.CountUnitMissing,
            Problem(Count with { CountUnit = " ", ScheduleKind = null }));
        Assert.Equal(SuggestedStartingGoalProblem.DurationUnitMissing,
            Problem(DurationTimes(15, null, 0, SchedulePeriod.Day)));
        Assert.Equal(SuggestedStartingGoalProblem.TimesOutOfRange,
            Problem(Count with { Times = 21, Period = null }));
    }

    [Fact]
    public void Keeps_the_accepted_fields()
    {
        var goal = Goal(DurationDays(15, DurationUnit.Minutes, Weekdays));
        Assert.Equal(BuildOrReduce.Build, goal.BuildOrReduce);
        Assert.Equal(Measurement.Duration, goal.Measurement);
        Assert.Equal(15, goal.Amount);
        Assert.Equal(DurationUnit.Minutes, goal.DurationUnit);
        Assert.Equal(ScheduleKind.FixedDays, goal.Schedule.Kind);
        Assert.Equal(Weekdays.ToHashSet(), goal.Schedule.Days.ToHashSet());
    }
}
