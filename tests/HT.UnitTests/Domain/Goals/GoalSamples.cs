using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Time;
using HT.UnitTests.Domain.Time;

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

    // Slice 8: goals with attempts, built through the attempts constructor.

    /// <summary>A settings version of <paramref name="target"/> in <paramref name="zone"/> from <paramref name="from"/>.</summary>
    public static GoalSettingsVersion SettingsFrom(GoalTarget target, DateOnly from, NamedTimeZone zone) =>
        new(new GoalSettings(target, zone), from);

    /// <summary>An ending on <paramref name="date"/>, Abandoned unless said otherwise, with no note.</summary>
    public static GoalEnding EndedOn(DateOnly date, Outcome outcome = Outcome.Abandoned) => new(date, outcome, null);

    /// <summary>Attempt <paramref name="number"/> from <paramref name="start"/>, ended by <paramref name="ending"/> or not, with its versions.</summary>
    public static GoalAttemptVersions AttemptFrom(int number, DateOnly start, GoalEnding? ending, params GoalSettingsVersion[] versions) =>
        new(new GoalAttempt(number, start) { Ending = ending }, versions);

    /// <summary>A goal with these attempts, through the attempts constructor.</summary>
    public static Goal GoalOf(params GoalAttemptVersions[] attempts) => GoalOf(null, attempts);

    /// <summary>A goal with an own name and these attempts, through the attempts constructor.</summary>
    public static Goal GoalOf(GoalName? ownName, params GoalAttemptVersions[] attempts) =>
        new(new GoalId(Guid.NewGuid()), Owner, Habit, ownName, attempts);

    /// <summary>
    /// The one-attempt goals of the slice 4 to 7 timeline cases (<c>GoalTimelineCalendarTests</c>,
    /// <c>GoalTimelineEntryTests</c>, <c>GoalTimelineEntriesThroughTests</c>, <c>GoalTimelineChangeTests</c>), by name,
    /// for slice 8's equivalence of both constructors (FR-024, research R-12). Each is built through the kept constructor.
    /// </summary>
    public static Goal TimelineGoal(string name)
    {
        var monday = Date(2026, 11, 16);
        var wednesday = Date(2026, 11, 18);
        var firstChange = Instants.Utc("2026-11-18T10:00:00Z");
        var secondChange = Instants.Utc("2026-11-19T17:00:00Z");
        static Goal Changed(Goal goal, DateTimeOffset at, NamedTimeZone zone) => GoalTimeline.ChangeOf(goal, at, zone).ApplyTo(goal);
        return name switch
        {
            "Mon/Wed/Fri from Wednesday in Kyiv" => Goal(OnDays(MonWedFri), wednesday, Kyiv),
            "per day from Wednesday in New York" => Goal(PerDay(), wednesday, NewYork),
            "per week from Wednesday in Kyiv" => Goal(PerWeek(), wednesday, Kyiv),
            "per week from Monday in Kyiv" => Goal(PerWeek(), monday, Kyiv),
            "Fridays from 1 December in Kyiv" => Goal(OnDays(DayOfWeek.Friday), Date(2026, 12, 1), Kyiv),
            "Mon/Wed/Fri moved to New York" => Changed(Goal(OnDays(MonWedFri), monday, Kyiv), firstChange, NewYork),
            "per day moved to New York, then Tokyo" => Changed(Changed(Goal(PerDay(), monday, Kyiv), firstChange, NewYork), secondChange, Tokyo),
            "per week moved to New York, then Tokyo" => Changed(Changed(Goal(PerWeek(), monday, Kyiv), firstChange, NewYork), secondChange, Tokyo),
            "every day moved east to Kyiv" => GoalVersionChange.Add(Date(2026, 11, 19), Kyiv).ApplyTo(Goal(OnDays(AllDays), monday, NewYork)),
            "every day moved from Pago Pago to Kiritimati" =>
                Changed(Goal(OnDays(AllDays), monday, Zone("Pacific/Pago_Pago")), Instants.Utc("2026-11-18T12:00:00Z"), Zone("Pacific/Kiritimati")),
            "per day over a clock change in Kyiv" => Goal(PerDay(), Date(2027, 3, 1), Kyiv),
            "per day in Havana" => Goal(PerDay(), Date(2027, 3, 1), Zone("America/Havana")),
            "Mon/Wed/Fri started weeks ago" => Goal(OnDays(MonWedFri), Date(2024, 9, 2), Kyiv),
            "per week across a window edge" => Goal(PerWeek(), Date(2026, 1, 5), Kyiv),
            "per day whose attempt starts after the start date" =>
                new Goal(new GoalId(Guid.NewGuid()), Owner, Habit, wednesday, null, new GoalAttempt(1, wednesday.AddDays(2)),
                    [SettingsFrom(PerDay(), wednesday, Kyiv)]),
            _ => throw new ArgumentOutOfRangeException(nameof(name), name, null),
        };
    }

    /// <summary>The names <see cref="TimelineGoal"/> knows.</summary>
    public static TheoryData<string> TimelineGoals => new()
    {
        "Mon/Wed/Fri from Wednesday in Kyiv",
        "per day from Wednesday in New York",
        "per week from Wednesday in Kyiv",
        "per week from Monday in Kyiv",
        "Fridays from 1 December in Kyiv",
        "Mon/Wed/Fri moved to New York",
        "per day moved to New York, then Tokyo",
        "per week moved to New York, then Tokyo",
        "every day moved east to Kyiv",
        "every day moved from Pago Pago to Kiritimati",
        "per day over a clock change in Kyiv",
        "per day in Havana",
        "Mon/Wed/Fri started weeks ago",
        "per week across a window edge",
        "per day whose attempt starts after the start date",
    };

    /// <summary>The instants of the slice 4 to 7 timeline cases: around midnights, zone moves, gaps, overlaps and clock changes.</summary>
    public static IReadOnlyList<DateTimeOffset> TimelineInstants { get; } =
    [
        .. new[]
        {
            "2024-09-01T00:00:00Z", "2026-01-04T21:59:59Z", "2026-11-01T00:00:00Z", "2026-11-15T21:59:59Z",
            "2026-11-15T22:00:00Z", "2026-11-16T03:00:00Z", "2026-11-17T21:59:59Z", "2026-11-17T22:00:00Z",
            "2026-11-18T10:00:00Z", "2026-11-18T12:00:00Z", "2026-11-18T21:59:59Z", "2026-11-18T23:00:00Z",
            "2026-11-19T01:00:00Z", "2026-11-19T04:59:59Z", "2026-11-19T05:00:00Z", "2026-11-19T10:30:00Z",
            "2026-11-19T11:30:00Z", "2026-11-19T17:00:00Z", "2026-11-20T17:00:00Z", "2026-11-22T21:00:00Z",
            "2026-11-23T10:00:00Z", "2026-12-02T05:00:00Z", "2027-02-28T21:59:59Z", "2027-03-01T05:00:00Z",
            "2027-03-14T05:30:00Z", "2027-03-28T00:30:00Z", "2027-03-28T01:30:00Z", "2027-10-31T01:30:00Z",
        }.Select(Instants.Utc),
    ];

    /// <summary>The same goal rebuilt through the attempts constructor (FR-024's equivalence of both constructors).</summary>
    public static Goal ViaAttempts(Goal goal) =>
        new(goal.Id, goal.Owner, goal.Habit, goal.OwnName, [.. goal.Attempts.Select(a => new GoalAttemptVersions(a.Attempt, [.. a.Versions]))]);
}
