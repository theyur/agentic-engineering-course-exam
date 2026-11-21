using HT.Domain.Exemptions;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;

namespace HT.Application.Operator;

/// <summary>
/// The demo data of <see cref="DemoPauseService.DemoEmail"/> (FR-031, research R-08, ADR 0023): two goals on catalog
/// habits of the seed catalog, each accepted only when its habit is Published, starting 28 days before the run date R,
/// with their records, notes, excusals and pauses relative to R. "R − k" is the date k days before R. Every record is on
/// a date before R; the excusals and pauses are facts on past dates and a current pause, which the user rules of FR-002
/// and FR-006 would refuse, so only this task writes them. Built as <see cref="DemoReport"/> builds demo5's goals. Pure:
/// the data depends only on the date passed in. Smoke check 8 takes its expected goal names from here, so no expected
/// text is typed twice.
/// </summary>
public static class DemoPauses
{
    /// <summary>The reason of the excusals of "Meditation" on R − 20 and R − 19 (FR-031).</summary>
    public const string TravellingReason = "Travelling.";

    /// <summary>The reason of the past pause of "Drinking water", R − 18 to R − 12 (FR-031).</summary>
    public const string IllnessReason = "Ill with flu.";

    /// <summary>The reason of the current pause of "Drinking water", R − 1 to R + 5 (FR-031).</summary>
    public const string FamilyVisitReason = "Family visit.";

    /// <summary>The note of "Meditation" on R − 14, which has no record (FR-031).</summary>
    public const string OversleptNote = "Overslept.";

    /// <summary>The note of "Drinking water" on R − 5, which has 6 glasses (FR-031).</summary>
    public const string BusyDayNote = "Busy day.";

    private const int DaysBeforeRun = 28;
    private const int MeditationMinutes = 10;
    private const int Glasses = 8;
    private const int BusyDayGlasses = 6;

    private static readonly CatalogHabitState[] Published = [CatalogHabitState.Published];

    private static readonly DayOfWeek[] EveryDay =
    [
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday,
        DayOfWeek.Saturday, DayOfWeek.Sunday,
    ];

    private static readonly Spec Meditation = new(
        "Meditation",
        TargetOf(new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Build,
            Measurement = Measurement.Duration,
            Amount = MeditationMinutes,
            DurationUnit = DurationUnit.Minutes,
            ScheduleKind = ScheduleKind.FixedDays,
            Days = EveryDay.ToHashSet(),
        }));

    private static readonly Spec DrinkingWater = new(
        "Drinking water",
        TargetOf(new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Build,
            Measurement = Measurement.Count,
            Amount = Glasses,
            CountUnit = "glasses",
            ScheduleKind = ScheduleKind.Times,
            Times = 1,
            Period = SchedulePeriod.Day,
        }));

    /// <summary>
    /// The names the Goals page shows for the two goals, in creation order (slice 4 FR-010): "Meditation, 10 min daily"
    /// and "Drinking water, 8 glasses a day".
    /// </summary>
    public static IReadOnlyList<string> GoalNames { get; } =
        [.. new[] { Meditation, DrinkingWater }.Select(spec => spec.HabitName + ", " + GoalText.TargetText(spec.Target))];

    /// <summary>The two goals of FR-031 for the run date <paramref name="runDate"/>, in creation order: Meditation, Drinking water.</summary>
    public static IReadOnlyList<DemoPauseGoal> For(DateOnly runDate)
    {
        var start = runDate.AddDays(-DaysBeforeRun);
        return [MeditationGoal(runDate, start), DrinkingWaterGoal(runDate, start)];
    }

    /// <summary>
    /// 10 minutes on each date R − k with k from 28 down to 3, except no record on R − 20 and R − 19, both excused with
    /// the reason "Travelling."; on R − 14, with the note "Overslept."; on R − 8, excused without a reason. R − 2 has no
    /// record and is excused without a reason; R − 1 and R have no record.
    /// </summary>
    private static DemoPauseGoal MeditationGoal(DateOnly runDate, DateOnly start)
    {
        var records = new List<DemoRecord>();
        for (var k = DaysBeforeRun; k >= 3; k--)
        {
            if (k is not (20 or 19 or 14 or 8))
            {
                records.Add(Amount(Measurement.Duration, runDate.AddDays(-k), MeditationMinutes));
            }
        }

        var travelling = Reason(TravellingReason);
        DemoExcusal[] excusals =
        [
            new(runDate.AddDays(-20), travelling),
            new(runDate.AddDays(-19), travelling),
            new(runDate.AddDays(-8), null),
            new(runDate.AddDays(-2), null),
        ];
        var history = new DemoHistoryGoal(Meditation.GoalFrom(start), records, [Note(runDate.AddDays(-14), OversleptNote)]);
        return new DemoPauseGoal(history, excusals, []);
    }

    /// <summary>
    /// 8 glasses on each date R − k with k from 28 down to 19 and from 11 down to 2, except 6 on R − 5, with the note
    /// "Busy day."; no record from R − 18 to R − 12, on R − 1 or on R. A past pause from R − 18 to R − 12 with the reason
    /// "Ill with flu." and a current pause from R − 1 to R + 5 with the reason "Family visit.".
    /// </summary>
    private static DemoPauseGoal DrinkingWaterGoal(DateOnly runDate, DateOnly start)
    {
        var records = new List<DemoRecord>();
        for (var k = DaysBeforeRun; k >= 2; k--)
        {
            if (k is >= 12 and <= 18)
            {
                continue;
            }

            records.Add(Amount(Measurement.Count, runDate.AddDays(-k), k == 5 ? BusyDayGlasses : Glasses));
        }

        DemoGoalPause[] pauses =
        [
            new(runDate.AddDays(-18), runDate.AddDays(-12), Reason(IllnessReason)),
            new(runDate.AddDays(-1), runDate.AddDays(5), Reason(FamilyVisitReason)),
        ];
        var history = new DemoHistoryGoal(DrinkingWater.GoalFrom(start), records, [Note(runDate.AddDays(-5), BusyDayNote)]);
        return new DemoPauseGoal(history, [], pauses);
    }

    private static DemoRecord Amount(Measurement measurement, DateOnly date, int amount) =>
        RecordValue.TryCreate(measurement, null, amount, out var value, out _)
            ? new DemoRecord(date, value)
            : throw new InvalidOperationException("A demo record fails the value checks.");

    // Stryker disable once Logical : the texts are this class's non-blank constants, which TryCreate accepts with a note, so || gives the same
    private static DemoNote Note(DateOnly entryDate, string text) =>
        NoteText.TryCreate(text, out var note, out _) && note is not null
            ? new DemoNote(entryDate, note)
            : throw new InvalidOperationException("A demo note fails the note checks.");

    // Stryker disable once Logical : the texts are this class's non-blank constants, which TryCreate accepts with a reason, so || gives the same
    private static ExemptionReason Reason(string text) =>
        ExemptionReason.TryCreate(text, out var reason, out _) && reason is not null
            ? reason
            : throw new InvalidOperationException("A demo reason fails the reason checks.");

    private static string SeedNameOf(string habitName)
    {
        var key = HabitName.KeyOf(habitName);
        // Stryker disable once Linq : both names are in the seed catalog exactly once, so Single gives the same habit as SingleOrDefault
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
