using HT.Domain.Habits;

namespace HT.UnitTests.Domain.Habits;

/// <summary>Short ways to write the fields of a suggested starting goal in tests.</summary>
public static class GoalFields
{
    public static SuggestedStartingGoalFields YesNoTimes(int times, SchedulePeriod period, BuildOrReduce side = BuildOrReduce.Build) =>
        new() { BuildOrReduce = side, Measurement = Measurement.YesNo, ScheduleKind = ScheduleKind.Times, Times = times, Period = period };

    public static SuggestedStartingGoalFields YesNoDays(params DayOfWeek[] days) =>
        new() { BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo, ScheduleKind = ScheduleKind.FixedDays, Days = days.ToHashSet() };

    public static SuggestedStartingGoalFields CountTimes(
        int? amount, string? unit, int times, SchedulePeriod period, BuildOrReduce side = BuildOrReduce.Build) =>
        new()
        {
            BuildOrReduce = side, Measurement = Measurement.Count, Amount = amount, CountUnit = unit,
            ScheduleKind = ScheduleKind.Times, Times = times, Period = period,
        };

    public static SuggestedStartingGoalFields CountDays(int amount, string unit, params DayOfWeek[] days) =>
        new()
        {
            BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Count, Amount = amount, CountUnit = unit,
            ScheduleKind = ScheduleKind.FixedDays, Days = days.ToHashSet(),
        };

    public static SuggestedStartingGoalFields DurationTimes(
        int? amount, DurationUnit? unit, int times, SchedulePeriod period, BuildOrReduce side = BuildOrReduce.Build) =>
        new()
        {
            BuildOrReduce = side, Measurement = Measurement.Duration, Amount = amount, DurationUnit = unit,
            ScheduleKind = ScheduleKind.Times, Times = times, Period = period,
        };

    public static SuggestedStartingGoalFields DurationDays(int amount, DurationUnit unit, params DayOfWeek[] days) =>
        new()
        {
            BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = amount, DurationUnit = unit,
            ScheduleKind = ScheduleKind.FixedDays, Days = days.ToHashSet(),
        };

    public static readonly DayOfWeek[] Weekdays =
        [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday];

    public static readonly DayOfWeek[] AllDays =
        [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday];

    public static SuggestedStartingGoal Goal(SuggestedStartingGoalFields fields)
    {
        Assert.True(SuggestedStartingGoal.TryCreate(fields, out var goal, out var problem), $"Refused: {problem}");
        return goal;
    }
}
