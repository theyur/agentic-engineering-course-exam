using HT.Domain.Habits;

namespace HT.UnitTests.Fakes;

/// <summary>Short ways to build habit values in tests; shared with HT.UiTests by a link, like the fakes.</summary>
public static class HabitSamples
{
    public static HabitDefinition Definition(
        string name, Category category = Category.Mind, Intent intent = Intent.Build, string icon = "book-open", string? description = null)
    {
        if (!HabitDefinition.TryCreate(name, description, category, intent, icon, out var definition, out var problem))
        {
            throw new ArgumentException($"Not a valid habit: {problem}.", nameof(name));
        }

        return definition;
    }

    public static CatalogHabit Catalog(
        string name,
        CatalogHabitState state = CatalogHabitState.Published,
        Category category = Category.Mind,
        Intent intent = Intent.Build,
        string icon = "book-open",
        string? description = null,
        SuggestedStartingGoals? goals = null) =>
        new(new HabitId(Guid.NewGuid()), Definition(name, category, intent, icon, description), goals ?? SuggestedStartingGoals.None, state);

    public static SuggestedStartingGoals Goals(int marked, params SuggestedStartingGoalFields[] fields)
    {
        if (!SuggestedStartingGoals.TryCreate(fields.Select((f, i) => (f, i == marked)).ToList(), out var goals, out var problem))
        {
            throw new ArgumentException($"Not a valid list: {problem}.", nameof(fields));
        }

        return goals;
    }

    public static SuggestedStartingGoalFields YesNo(int times, SchedulePeriod period, BuildOrReduce side = BuildOrReduce.Build) =>
        new() { BuildOrReduce = side, Measurement = Measurement.YesNo, ScheduleKind = ScheduleKind.Times, Times = times, Period = period };

    public static SuggestedStartingGoalFields Count(int amount, string unit, int times, SchedulePeriod period, BuildOrReduce side = BuildOrReduce.Build) =>
        new()
        {
            BuildOrReduce = side, Measurement = Measurement.Count, Amount = amount, CountUnit = unit,
            ScheduleKind = ScheduleKind.Times, Times = times, Period = period,
        };

    public static SuggestedStartingGoalFields DurationDays(int amount, DurationUnit unit, params DayOfWeek[] days) =>
        new()
        {
            BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = amount, DurationUnit = unit,
            ScheduleKind = ScheduleKind.FixedDays, Days = days.ToHashSet(),
        };

    public static SuggestedStartingGoalFields DurationTimes(int amount, DurationUnit unit, int times, SchedulePeriod period, BuildOrReduce side = BuildOrReduce.Build) =>
        new()
        {
            BuildOrReduce = side, Measurement = Measurement.Duration, Amount = amount, DurationUnit = unit,
            ScheduleKind = ScheduleKind.Times, Times = times, Period = period,
        };

    public static readonly DayOfWeek[] Weekdays =
        [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday];
}
