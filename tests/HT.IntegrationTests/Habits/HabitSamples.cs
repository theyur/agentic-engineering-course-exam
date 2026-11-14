using HT.Application.Habits;
using HT.Domain.Habits;

namespace HT.IntegrationTests.Habits;

/// <summary>Short ways to write habit values in the integration tests.</summary>
public static class HabitSamples
{
    public static HabitDefinition Definition(
        string name, Category category = Category.Mind, Intent intent = Intent.Build, string icon = "book-open", string? description = null)
    {
        Assert.True(HabitDefinition.TryCreate(name, description, category, intent, icon, out var definition, out var problem), $"Refused: {problem}");
        return definition;
    }

    public static SuggestedStartingGoalFields YesNo(int times, SchedulePeriod period) =>
        new() { BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo, ScheduleKind = ScheduleKind.Times, Times = times, Period = period };

    public static SuggestedStartingGoalFields Count(int amount, string unit, int times, SchedulePeriod period) =>
        new()
        {
            BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Count, Amount = amount, CountUnit = unit,
            ScheduleKind = ScheduleKind.Times, Times = times, Period = period,
        };

    public static SuggestedStartingGoalFields Duration(int amount, DurationUnit unit, params DayOfWeek[] days) =>
        new()
        {
            BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = amount, DurationUnit = unit,
            ScheduleKind = ScheduleKind.FixedDays, Days = days.ToHashSet(),
        };

    public static SuggestedStartingGoals Goals(int marked, params SuggestedStartingGoalFields[] fields)
    {
        Assert.True(SuggestedStartingGoals.TryCreate(fields.Select((f, i) => (f, i == marked)).ToList(), out var goals, out var problem), $"Refused: {problem}");
        return goals;
    }

    public static SuggestedStartingGoalInput Input(SuggestedStartingGoalFields fields, bool isMarked) => new()
    {
        BuildOrReduce = fields.BuildOrReduce,
        Measurement = fields.Measurement,
        Amount = fields.Amount,
        CountUnit = fields.CountUnit,
        DurationUnit = fields.DurationUnit,
        ScheduleKind = fields.ScheduleKind,
        Days = fields.Days ?? new HashSet<DayOfWeek>(),
        Times = fields.Times,
        Period = fields.Period,
        IsMarked = isMarked,
    };

    public static readonly DayOfWeek[] Weekdays =
        [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday];
}
