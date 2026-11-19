using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;

namespace HT.Application.Operator;

/// <summary>
/// The demo data of <see cref="DemoHistoryService.DemoEmail"/> (FR-030, research R-08): three goals on catalog habits of
/// the seed catalog, each accepted only when its habit is Published, starting 28 days before the run date R, with their
/// records and notes relative to R. "R − k" is the date k days before R. Every record is on a date before R. Pure: the
/// data depends only on the date passed in. Smoke check 6 takes its expected texts from here (research R-10), so no
/// expected text is typed twice.
/// </summary>
public static class DemoHistory
{
    /// <summary>The partial amount of "Reading" on R − 6, in minutes, below its target of 20 (FR-030).</summary>
    public const int PartialMinutes = 12;

    /// <summary>The note of "Reading" on R − 6 (FR-030).</summary>
    public const string ReadingNote = "Fell asleep early.";

    private const string ReadingAwayNote = "Away for the weekend.";
    private const string SugarNote = "Birthday cake at work.";
    private const string YogaNote = "Knee was sore.";

    private const int DaysBeforeRun = 28;
    private const int ReadingMinutes = 20;
    private const int ReadingLateMinutes = 25;
    private const int SugarCakeCount = 3;
    private const int DaysPerWeek = 7;

    private static readonly CatalogHabitState[] Published = [CatalogHabitState.Published];

    private static readonly DayOfWeek[] EveryDay =
    [
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday,
        DayOfWeek.Saturday, DayOfWeek.Sunday,
    ];

    private static readonly Spec Reading = new(
        "Reading",
        TargetOf(new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Build,
            Measurement = Measurement.Duration,
            Amount = ReadingMinutes,
            DurationUnit = DurationUnit.Minutes,
            ScheduleKind = ScheduleKind.FixedDays,
            Days = EveryDay.ToHashSet(),
        }));

    private static readonly Spec Sugar = new(
        "Sugar",
        TargetOf(new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Reduce,
            Measurement = Measurement.Count,
            Amount = 1,
            CountUnit = "sweet",
            ScheduleKind = ScheduleKind.Times,
            Times = 1,
            Period = SchedulePeriod.Day,
        }));

    private static readonly Spec Yoga = new(
        "Yoga",
        TargetOf(new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Build,
            Measurement = Measurement.YesNo,
            ScheduleKind = ScheduleKind.Times,
            Times = 3,
            Period = SchedulePeriod.Week,
        }));

    /// <summary>
    /// The names the Goals page shows for the three goals, in creation order (slice 4 FR-010): the habit's name, ", "
    /// and the target text: "Reading, 20 min daily", "Sugar, at most 1 sweet a day", "Yoga, 3 times a week".
    /// </summary>
    public static IReadOnlyList<string> GoalNames { get; } =
        [.. new[] { Reading, Sugar, Yoga }.Select(spec => spec.HabitName + ", " + GoalText.TargetText(spec.Target))];

    /// <summary>The three goals of FR-030 for the run date <paramref name="runDate"/>, in creation order: Reading, Sugar, Yoga.</summary>
    public static IReadOnlyList<DemoHistoryGoal> For(DateOnly runDate)
    {
        var start = runDate.AddDays(-DaysBeforeRun);
        return
        [
            new DemoHistoryGoal(Reading.GoalFrom(start), ReadingRecords(runDate), [Note(runDate.AddDays(-6), ReadingNote), Note(runDate.AddDays(-10), ReadingAwayNote)]),
            new DemoHistoryGoal(Sugar.GoalFrom(start), SugarRecords(runDate), [Note(runDate.AddDays(-4), SugarNote)]),
            YogaGoal(runDate, start),
        ];
    }

    /// <summary>20 minutes on each date from R − 28 to R − 3, except 12 on R − 6 and none on R − 10; then 25 on R − 2.</summary>
    private static IEnumerable<DemoRecord> ReadingRecords(DateOnly runDate)
    {
        for (var k = DaysBeforeRun; k >= 3; k--)
        {
            switch (k)
            {
                case 10:
                    continue;
                case 6:
                    yield return Amount(runDate.AddDays(-k), Measurement.Duration, PartialMinutes);
                    break;
                default:
                    yield return Amount(runDate.AddDays(-k), Measurement.Duration, ReadingMinutes);
                    break;
            }
        }

        yield return Amount(runDate.AddDays(-2), Measurement.Duration, ReadingLateMinutes);
    }

    /// <summary>On each date from R − 28 to R − 3: 0 when k is even and 1 when it is odd, except 3 on R − 4 and none on R − 12.</summary>
    private static IEnumerable<DemoRecord> SugarRecords(DateOnly runDate)
    {
        for (var k = DaysBeforeRun; k >= 3; k--)
        {
            switch (k)
            {
                case 12:
                    continue;
                case 4:
                    yield return Amount(runDate.AddDays(-k), Measurement.Count, SugarCakeCount);
                    break;
                default:
                    yield return Amount(runDate.AddDays(-k), Measurement.Count, k % 2);
                    break;
            }
        }
    }

    /// <summary>
    /// The current week is the Monday-to-Sunday week that contains R; week 1 starts 7 days before its Monday, week 2
    /// 14 days and week 3 21 days, and the week before week 3 28 days. Week 1, and the week before week 3 when its
    /// Monday is on or after R − 28 (only when R is a Monday), have Yes on Monday, Wednesday and Friday; week 2 has Yes
    /// on Monday and Wednesday and the note under its Monday; week 3 and the current week have no record.
    /// </summary>
    private static DemoHistoryGoal YogaGoal(DateOnly runDate, DateOnly start)
    {
        var currentMonday = runDate.AddDays(-(((int)runDate.DayOfWeek + DaysPerWeek - (int)DayOfWeek.Monday) % DaysPerWeek));
        var week1 = currentMonday.AddDays(-DaysPerWeek);
        var week2 = currentMonday.AddDays(-2 * DaysPerWeek);
        var weekBeforeWeek3 = currentMonday.AddDays(-4 * DaysPerWeek);

        var records = new List<DemoRecord>();
        if (weekBeforeWeek3 >= start)
        {
            records.AddRange(MondayWednesdayFriday(weekBeforeWeek3));
        }

        records.Add(Yes(week2));
        records.Add(Yes(week2.AddDays(2)));
        records.AddRange(MondayWednesdayFriday(week1));
        return new DemoHistoryGoal(Yoga.GoalFrom(start), records, [Note(week2, YogaNote)]);
    }

    private static IEnumerable<DemoRecord> MondayWednesdayFriday(DateOnly monday) =>
        [Yes(monday), Yes(monday.AddDays(2)), Yes(monday.AddDays(4))];

    private static DemoRecord Yes(DateOnly date) =>
        RecordValue.TryCreate(Measurement.YesNo, YesNoAnswer.Yes, null, out var value, out _)
            ? new DemoRecord(date, value)
            : throw new InvalidOperationException("A demo record fails the value checks.");

    private static DemoRecord Amount(DateOnly date, Measurement measurement, int amount) =>
        RecordValue.TryCreate(measurement, null, amount, out var value, out _)
            ? new DemoRecord(date, value)
            : throw new InvalidOperationException("A demo record fails the value checks.");

    private static DemoNote Note(DateOnly entryDate, string text) =>
        NoteText.TryCreate(text, out var note, out _) && note is not null
            ? new DemoNote(entryDate, note)
            : throw new InvalidOperationException("A demo note fails the note checks.");

    private static string SeedNameOf(string habitName)
    {
        var key = HabitName.KeyOf(habitName);
        return SeedCatalog.Habits.SingleOrDefault(h => h.Definition.Name.Key == key)?.Definition.Name.Value
            ?? throw new InvalidOperationException("A demo goal names a habit that is not in the seed catalog.");
    }

    private static GoalTarget TargetOf(GoalTargetFields fields) =>
        GoalTarget.TryCreate(fields, out var target, out _)
            ? target
            : throw new InvalidOperationException("A demo goal fails the target checks.");

    /// <summary>One goal before its start date is known: the seed catalog habit's name and the target.</summary>
    private sealed record Spec(string HabitName, GoalTarget Target)
    {
        public DemoGoal GoalFrom(DateOnly start) => DemoGoal.OnCatalogHabit(HabitName, Published, Target, null, start);

        /// <summary>The name as the seed catalog spells it; a name that is not in the seed catalog is a programming error.</summary>
        public string HabitName { get; } = SeedNameOf(HabitName);
    }
}
