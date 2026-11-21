using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;

namespace HT.Application.Operator;

/// <summary>
/// The demo data of <see cref="DemoReportService.DemoEmail"/> (FR-032, research R-09, ADR 0021): three goals on catalog
/// habits of the seed catalog, each accepted only when its habit is Published, starting 84 days before the run date R,
/// with their records and notes relative to R. "R − k" is the date k days before R. Every record is on a date before R.
/// Built as <see cref="DemoHistory"/> builds demo4's goals. Pure: the data depends only on the date passed in. Smoke
/// check 7 takes its expected goal names from here (research R-11), so no expected text is typed twice.
/// </summary>
public static class DemoReport
{
    /// <summary>The note of "Walking" on the latest commitment of R − 49 to R − 43 (FR-032).</summary>
    public const string WalkingNote = "On holiday.";

    /// <summary>The note of "Strength training" on week 4, under its Monday (FR-032).</summary>
    public const string StrengthNote = "Sore shoulder.";

    /// <summary>The note of "Screen time" on R − 31 (FR-032).</summary>
    public const string ScreenTimeNote = "Long train ride.";

    private const int DaysBeforeRun = 84;
    private const int DaysPerWeek = 7;
    private const int WalkingMinutes = 30;
    private const int WalkingLongMinutes = 35;
    private const int WalkingShortMinutes = 18;
    private const int ScreenEvenMinutes = 90;
    private const int ScreenOddMinutes = 110;
    private const int ScreenLongMinutes = 150;
    private const int ScreenLateMinutes = 100;

    /// <summary>The weeks of "Strength training" before the current one, at most: week 12 starts on R − 84 when R is a Monday.</summary>
    private const int StrengthWeeks = 12;

    private static readonly CatalogHabitState[] Published = [CatalogHabitState.Published];

    private static readonly Spec Walking = new(
        "Walking",
        TargetOf(new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Build,
            Measurement = Measurement.Duration,
            Amount = WalkingMinutes,
            DurationUnit = DurationUnit.Minutes,
            ScheduleKind = ScheduleKind.FixedDays,
            Days = new HashSet<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
        }));

    private static readonly Spec Strength = new(
        "Strength training",
        TargetOf(new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Build,
            Measurement = Measurement.YesNo,
            ScheduleKind = ScheduleKind.Times,
            Times = 2,
            Period = SchedulePeriod.Week,
        }));

    private static readonly Spec ScreenTime = new(
        "Screen time",
        TargetOf(new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Reduce,
            Measurement = Measurement.Duration,
            Amount = 120,
            DurationUnit = DurationUnit.Minutes,
            ScheduleKind = ScheduleKind.Times,
            Times = 1,
            Period = SchedulePeriod.Day,
        }));

    /// <summary>
    /// The names the Goals page shows for the three goals, in creation order (slice 4 FR-010): "Walking, 30 min, 3 days
    /// a week", "Strength training, 2 times a week", "Screen time, at most 120 min a day".
    /// </summary>
    public static IReadOnlyList<string> GoalNames { get; } =
        [.. new[] { Walking, Strength, ScreenTime }.Select(spec => spec.HabitName + ", " + GoalText.TargetText(spec.Target))];

    /// <summary>The three goals of FR-032 for the run date <paramref name="runDate"/>, in creation order: Walking, Strength training, Screen time.</summary>
    public static IReadOnlyList<DemoHistoryGoal> For(DateOnly runDate)
    {
        var start = runDate.AddDays(-DaysBeforeRun);
        return [WalkingGoal(runDate, start), StrengthGoal(runDate, start), ScreenTimeGoal(runDate, start)];
    }

    /// <summary>
    /// On each Monday, Wednesday and Friday R − k with k from 84 down to 3: 30 minutes, except 35 for k 9 to 15, 18 for k
    /// 20 to 26 and none for k 43 to 49, the latest of which gets the note.
    /// </summary>
    private static DemoHistoryGoal WalkingGoal(DateOnly runDate, DateOnly start)
    {
        var records = new List<DemoRecord>();
        DateOnly? holiday = null;
        for (var k = DaysBeforeRun; k >= 3; k--)
        {
            var date = runDate.AddDays(-k);
            if (date.DayOfWeek is not (DayOfWeek.Monday or DayOfWeek.Wednesday or DayOfWeek.Friday))
            {
                continue;
            }

            switch (k)
            {
                case >= 43 and <= 49:
                    holiday = date;
                    break;
                case >= 20 and <= 26:
                    records.Add(Amount(date, WalkingShortMinutes));
                    break;
                case >= 9 and <= 15:
                    records.Add(Amount(date, WalkingLongMinutes));
                    break;
                default:
                    records.Add(Amount(date, WalkingMinutes));
                    break;
            }
        }

        var holidayDate = holiday ?? throw new InvalidOperationException("Seven days always hold a demo commitment.");
        return new DemoHistoryGoal(Walking.GoalFrom(start), records, [Note(holidayDate, WalkingNote)]);
    }

    /// <summary>
    /// The current week is the Monday-to-Sunday week that contains R; week w starts 7 × w days before its Monday. Each
    /// week from 12 down to 1 whose Monday is on or after the start date has Yes on Monday and Thursday, except week 2:
    /// Monday, Wednesday and Thursday; week 4: Monday only, with the note under its Monday; week 7: no record. The
    /// current week has no record.
    /// </summary>
    private static DemoHistoryGoal StrengthGoal(DateOnly runDate, DateOnly start)
    {
        var currentMonday = runDate.AddDays(-(((int)runDate.DayOfWeek + DaysPerWeek - (int)DayOfWeek.Monday) % DaysPerWeek));
        var records = new List<DemoRecord>();
        var notes = new List<DemoNote>();
        for (var week = StrengthWeeks; week >= 1; week--)
        {
            var monday = currentMonday.AddDays(-DaysPerWeek * week);
            if (monday < start)
            {
                continue;
            }

            switch (week)
            {
                case 7:
                    break;
                case 4:
                    records.Add(Yes(monday));
                    notes.Add(Note(monday, StrengthNote));
                    break;
                case 2:
                    records.AddRange([Yes(monday), Yes(monday.AddDays(2)), Yes(monday.AddDays(3))]);
                    break;
                default:
                    records.AddRange([Yes(monday), Yes(monday.AddDays(3))]);
                    break;
            }
        }

        return new DemoHistoryGoal(Strength.GoalFrom(start), records, notes);
    }

    /// <summary>
    /// On each date R − k with k from 84 down to 4: 90 minutes when k is even and 110 when it is odd, except 150 for k 30
    /// to 32, with the note on R − 31, and none for k = 50; none on R − 3; 100 on R − 2; none on R − 1 and R.
    /// </summary>
    private static DemoHistoryGoal ScreenTimeGoal(DateOnly runDate, DateOnly start)
    {
        var records = new List<DemoRecord>();
        for (var k = DaysBeforeRun; k >= 4; k--)
        {
            var date = runDate.AddDays(-k);
            switch (k)
            {
                case 50:
                    continue;
                case >= 30 and <= 32:
                    records.Add(Amount(date, ScreenLongMinutes));
                    break;
                default:
                    records.Add(Amount(date, k % 2 == 0 ? ScreenEvenMinutes : ScreenOddMinutes));
                    break;
            }
        }

        records.Add(Amount(runDate.AddDays(-2), ScreenLateMinutes));
        return new DemoHistoryGoal(ScreenTime.GoalFrom(start), records, [Note(runDate.AddDays(-31), ScreenTimeNote)]);
    }

    private static DemoRecord Yes(DateOnly date) =>
        RecordValue.TryCreate(Measurement.YesNo, YesNoAnswer.Yes, null, out var value, out _)
            ? new DemoRecord(date, value)
            : throw new InvalidOperationException("A demo record fails the value checks.");

    private static DemoRecord Amount(DateOnly date, int minutes) =>
        RecordValue.TryCreate(Measurement.Duration, null, minutes, out var value, out _)
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
