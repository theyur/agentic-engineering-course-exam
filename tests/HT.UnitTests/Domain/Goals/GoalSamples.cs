using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Time;

namespace HT.UnitTests.Domain.Goals;

/// <summary>Short ways to write goal targets, zones and goals in tests, through public members only.</summary>
public static class GoalSamples
{
    public static readonly DayOfWeek[] MonWedFri = [DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday];

    public static readonly DayOfWeek[] Weekdays =
        [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday];

    public static readonly DayOfWeek[] AllDays =
        [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday];

    public static GoalTargetFields YesNoDays(BuildOrReduce side, params DayOfWeek[] days) =>
        new() { BuildOrReduce = side, Measurement = Measurement.YesNo, ScheduleKind = ScheduleKind.FixedDays, Days = days.ToHashSet() };

    public static GoalTargetFields YesNoTimes(int times, SchedulePeriod period, BuildOrReduce side = BuildOrReduce.Build) =>
        new() { BuildOrReduce = side, Measurement = Measurement.YesNo, ScheduleKind = ScheduleKind.Times, Times = times, Period = period };

    public static GoalTargetFields CountDays(int amount, string unit, BuildOrReduce side, params DayOfWeek[] days) =>
        new()
        {
            BuildOrReduce = side, Measurement = Measurement.Count, Amount = amount, CountUnit = unit,
            ScheduleKind = ScheduleKind.FixedDays, Days = days.ToHashSet(),
        };

    public static GoalTargetFields CountTimes(
        int? amount, string? unit, int? times, SchedulePeriod? period, BuildOrReduce side = BuildOrReduce.Build) =>
        new()
        {
            BuildOrReduce = side, Measurement = Measurement.Count, Amount = amount, CountUnit = unit,
            ScheduleKind = ScheduleKind.Times, Times = times, Period = period,
        };

    public static GoalTargetFields DurationDays(int amount, DurationUnit unit, BuildOrReduce side, params DayOfWeek[] days) =>
        new()
        {
            BuildOrReduce = side, Measurement = Measurement.Duration, Amount = amount, DurationUnit = unit,
            ScheduleKind = ScheduleKind.FixedDays, Days = days.ToHashSet(),
        };

    public static GoalTargetFields DurationTimes(
        int? amount, DurationUnit? unit, int times, SchedulePeriod period, BuildOrReduce side = BuildOrReduce.Build) =>
        new()
        {
            BuildOrReduce = side, Measurement = Measurement.Duration, Amount = amount, DurationUnit = unit,
            ScheduleKind = ScheduleKind.Times, Times = times, Period = period,
        };

    public static GoalTarget Target(GoalTargetFields fields)
    {
        Assert.True(GoalTarget.TryCreate(fields, out var target, out var problem), $"Refused: {problem}");
        return target;
    }

    /// <summary>A Build yes/no target on fixed days.</summary>
    public static GoalTarget OnDays(params DayOfWeek[] days) => Target(YesNoDays(BuildOrReduce.Build, days));

    /// <summary>A Build yes/no target of once per day.</summary>
    public static GoalTarget PerDay() => Target(YesNoTimes(1, SchedulePeriod.Day));

    /// <summary>A Build yes/no target of three times per week.</summary>
    public static GoalTarget PerWeek() => Target(YesNoTimes(3, SchedulePeriod.Week));

    public static NamedTimeZone Zone(string id)
    {
        Assert.True(NamedTimeZone.TryParse(id, out var zone), id);
        return zone;
    }

    public static readonly NamedTimeZone Kyiv = Zone("Europe/Kyiv");
    public static readonly NamedTimeZone NewYork = Zone("America/New_York");
    public static readonly NamedTimeZone Tokyo = Zone("Asia/Tokyo");

    public static readonly AccountId Owner = new(Guid.Parse("6f9a3c1e-0000-4000-8000-000000000001"));

    public static readonly GoalHabit Habit = new(new HabitId(Guid.Parse("6f9a3c1e-0000-4000-8000-000000000002")), HabitKind.Catalog);

    /// <summary>A goal with one version, from <paramref name="start"/>, in <paramref name="zone"/>.</summary>
    public static Goal Goal(GoalTarget target, DateOnly start, NamedTimeZone zone, GoalName? ownName = null) =>
        new(new GoalId(Guid.NewGuid()), Owner, Habit, start, ownName, new GoalAttempt(1, start),
            [new GoalSettingsVersion(new GoalSettings(target, zone), start)]);

    public static DateOnly Date(int year, int month, int day) => new(year, month, day);
}
