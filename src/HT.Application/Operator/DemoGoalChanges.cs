using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;

namespace HT.Application.Operator;

/// <summary>
/// The demo data of <see cref="DemoGoalChangeService.DemoEmail"/> (slice 8 FR-031, research R-09, ADR 0026): three goals
/// on catalog habits of the seed catalog, each accepted only when its habit is Published, with their settings versions,
/// attempts, endings, records and notes relative to the run date R. "R − k" is the date k days before R. The versions,
/// attempts and endings are facts in the past and a change still waiting for its boundary, which the user rules of
/// FR-002, FR-007 and FR-012 would refuse, so only this task writes them. Every record is on a date before R. Built as
/// <see cref="DemoPauses"/> builds demo6's goals. Pure: the data depends only on the date passed in. The smoke check
/// can take its expected texts from here, so no expected text is typed twice.
/// </summary>
public static class DemoGoalChanges
{
    /// <summary>The note of "Reading" on R − 9, which has 15 of 20 minutes (FR-031).</summary>
    public const string ShortOnTimeNote = "Short on time.";

    /// <summary>The ending note of "Meditation"'s attempt 1, ended on R − 21 as Abandoned (FR-031).</summary>
    public const string TooBusyEndingNote = "Too busy.";

    /// <summary>The ending note of "Sugar"'s attempt 1, ended on R − 7 as Achieved (FR-031).</summary>
    public const string SugarEndingNote = "Three weeks without extra sweets.";

    private const int ReadingFirstMinutes = 15;
    private const int ReadingSecondMinutes = 20;
    private const int ReadingWaitingMinutes = 25;
    private const int MeditationFirstMinutes = 10;
    private const int MeditationSecondMinutes = 5;
    private const int Sweets = 0;

    private static readonly CatalogHabitState[] Published = [CatalogHabitState.Published];

    private static readonly DayOfWeek[] EveryDay =
    [
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday,
        DayOfWeek.Saturday, DayOfWeek.Sunday,
    ];

    private static readonly string Reading = SeedNameOf("Reading");
    private static readonly string Meditation = SeedNameOf("Meditation");
    private static readonly string Sugar = SeedNameOf("Sugar");

    private static readonly GoalTarget ReadingFirst = DailyMinutes(ReadingFirstMinutes);
    private static readonly GoalTarget ReadingSecond = DailyMinutes(ReadingSecondMinutes);
    private static readonly GoalTarget ReadingWaiting = DailyMinutes(ReadingWaitingMinutes);
    private static readonly GoalTarget MeditationFirst = DailyMinutes(MeditationFirstMinutes);
    private static readonly GoalTarget MeditationSecond = DailyMinutes(MeditationSecondMinutes);

    private static readonly GoalTarget SugarTarget = TargetOf(new GoalTargetFields
    {
        BuildOrReduce = BuildOrReduce.Reduce,
        Measurement = Measurement.Count,
        Amount = 1,
        CountUnit = "sweet",
        ScheduleKind = ScheduleKind.Times,
        Times = 1,
        Period = SchedulePeriod.Day,
    });

    /// <summary>
    /// The names the Goals page shows for the three goals at the run time, in creation order (FR-004): "Reading, 20 min
    /// daily", "Meditation, 5 min daily" and "Sugar, at most 1 sweet a day".
    /// </summary>
    public static IReadOnlyList<string> GoalNames { get; } =
    [
        NameOf(Reading, ReadingSecond), NameOf(Meditation, MeditationSecond), NameOf(Sugar, SugarTarget),
    ];

    /// <summary>The name of "Meditation"'s ended attempt 1 by its own last settings (FR-004): "Meditation, 10 min daily".</summary>
    public static string EarlierAttemptName { get; } = NameOf(Meditation, MeditationFirst);

    /// <summary>The target text of "Reading"'s change still waiting for R + 1 (FR-005): "25 min daily".</summary>
    public static string WaitingTargetText { get; } = GoalText.TargetText(ReadingWaiting);

    /// <summary>The three goals of FR-031 for the run date <paramref name="runDate"/>, in creation order: Reading, Meditation, Sugar.</summary>
    public static IReadOnlyList<DemoGoalChangeGoal> For(DateOnly runDate) =>
        [ReadingGoal(runDate), MeditationGoal(runDate), SugarGoal(runDate)];

    /// <summary>
    /// One attempt from R − 28, not ended: 15 minutes from R − 28, 20 minutes from R − 13 and 25 minutes from R + 1.
    /// Records: 15 minutes on each date from R − 28 to R − 14; 20 minutes on each date from R − 13 to R − 2, except
    /// R − 9 with 15 minutes and the note "Short on time."; none on R − 1 or R.
    /// </summary>
    private static DemoGoalChangeGoal ReadingGoal(DateOnly runDate)
    {
        var records = new List<DemoRecord>();
        for (var k = 28; k >= 2; k--)
        {
            var minutes = k >= 14 || k == 9 ? ReadingFirstMinutes : ReadingSecondMinutes;
            records.Add(Minutes(runDate.AddDays(-k), minutes));
        }

        DemoVersion[] versions =
        [
            new(runDate.AddDays(-28), ReadingFirst),
            new(runDate.AddDays(-13), ReadingSecond),
            new(runDate.AddDays(1), ReadingWaiting),
        ];
        return new DemoGoalChangeGoal(
            Reading, Published, [new DemoAttempt(GoalAttempt.First, versions, null)], records, [Note(runDate.AddDays(-9), ShortOnTimeNote)]);
    }

    /// <summary>
    /// Attempt 1 from R − 42 with 10 minutes, ended on R − 21 as Abandoned with the note "Too busy."; attempt 2 from
    /// R − 14 with 5 minutes, not ended. Records: 10 minutes on each date from R − 42 to R − 32, none from R − 31 to
    /// R − 15; 5 minutes on each date from R − 14 to R − 2; none on R − 1 or R.
    /// </summary>
    private static DemoGoalChangeGoal MeditationGoal(DateOnly runDate)
    {
        var records = new List<DemoRecord>();
        for (var k = 42; k >= 32; k--)
        {
            records.Add(Minutes(runDate.AddDays(-k), MeditationFirstMinutes));
        }

        for (var k = 14; k >= 2; k--)
        {
            records.Add(Minutes(runDate.AddDays(-k), MeditationSecondMinutes));
        }

        DemoAttempt[] attempts =
        [
            new(
                GoalAttempt.First,
                [new DemoVersion(runDate.AddDays(-42), MeditationFirst)],
                new GoalEnding(runDate.AddDays(-21), Outcome.Abandoned, EndingNoteOf(TooBusyEndingNote))),
            new(GoalAttempt.First + 1, [new DemoVersion(runDate.AddDays(-14), MeditationSecond)], null),
        ];
        return new DemoGoalChangeGoal(Meditation, Published, attempts, records, []);
    }

    /// <summary>
    /// One attempt from R − 30 with at most 1 sweet a day, ended on R − 7 as Achieved with the note "Three weeks
    /// without extra sweets.". Records: 0 sweets on each date from R − 30 to R − 8.
    /// </summary>
    private static DemoGoalChangeGoal SugarGoal(DateOnly runDate)
    {
        var records = new List<DemoRecord>();
        for (var k = 30; k >= 8; k--)
        {
            records.Add(Amount(Measurement.Count, runDate.AddDays(-k), Sweets));
        }

        var attempt = new DemoAttempt(
            GoalAttempt.First,
            [new DemoVersion(runDate.AddDays(-30), SugarTarget)],
            new GoalEnding(runDate.AddDays(-7), Outcome.Achieved, EndingNoteOf(SugarEndingNote)));
        return new DemoGoalChangeGoal(Sugar, Published, [attempt], records, []);
    }

    private static string NameOf(string habitName, GoalTarget target) => habitName + ", " + GoalText.TargetText(target);

    private static DemoRecord Minutes(DateOnly date, int minutes) => Amount(Measurement.Duration, date, minutes);

    private static DemoRecord Amount(Measurement measurement, DateOnly date, int amount) =>
        RecordValue.TryCreate(measurement, null, amount, out var value, out _)
            ? new DemoRecord(date, value)
            : throw new InvalidOperationException("A demo record fails the value checks.");

    // Stryker disable once Logical : the texts are this class's non-blank constants, which TryCreate accepts with a note, so || gives the same
    private static DemoNote Note(DateOnly entryDate, string text) =>
        NoteText.TryCreate(text, out var note, out _) && note is not null
            ? new DemoNote(entryDate, note)
            : throw new InvalidOperationException("A demo note fails the note checks.");

    // Stryker disable once Logical : the texts are this class's non-blank constants, which TryCreate accepts with a note, so || gives the same
    private static EndingNote EndingNoteOf(string text) =>
        EndingNote.TryCreate(text, out var note, out _) && note is { } checkedNote
            ? checkedNote
            : throw new InvalidOperationException("A demo ending note fails the note checks.");

    private static GoalTarget DailyMinutes(int minutes) => TargetOf(new GoalTargetFields
    {
        BuildOrReduce = BuildOrReduce.Build,
        Measurement = Measurement.Duration,
        Amount = minutes,
        DurationUnit = DurationUnit.Minutes,
        ScheduleKind = ScheduleKind.FixedDays,
        Days = EveryDay.ToHashSet(),
    });

    private static GoalTarget TargetOf(GoalTargetFields fields) =>
        GoalTarget.TryCreate(fields, out var target, out _)
            ? target
            : throw new InvalidOperationException("A demo goal fails the target checks.");

    /// <summary>The name as the seed catalog spells it; a name that is not in the seed catalog is a programming error.</summary>
    private static string SeedNameOf(string habitName)
    {
        var key = HabitName.KeyOf(habitName);
        // Stryker disable once Linq : the three names are in the seed catalog exactly once, so Single gives the same habit as SingleOrDefault
        return SeedCatalog.Habits.SingleOrDefault(h => h.Definition.Name.Key == key)?.Definition.Name.Value
            ?? throw new InvalidOperationException("A demo goal names a habit that is not in the seed catalog.");
    }
}
