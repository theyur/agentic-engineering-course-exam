using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Domain.Evaluation;

/// <summary>One set of records of a <see cref="TargetCase"/>, with its final state and its progress, worked out by hand.</summary>
public sealed record RecordSet(string Name, IReadOnlyList<RecordValue> Records, EvaluationState Final, Progress Progress);

/// <summary>
/// One combination of Build or Reduce × measurement × schedule kind (SC-002), with its records just below, at and
/// just above its target. A yes/no commitment has no "above" (Build) or "below" (Reduce).
/// </summary>
public sealed record TargetCase(string Name, GoalTarget Target, bool PerWeek, Progress NoRecords, IReadOnlyList<RecordSet> Sets)
{
    public DateTimeOffset Starts => PerWeek ? EvaluatorCases.WeekStarts : EvaluatorCases.DayStarts;

    public DateTimeOffset Ends => PerWeek ? EvaluatorCases.WeekEnds : EvaluatorCases.DayEnds;

    public DateTimeOffset GraceEnds => PerWeek ? EvaluatorCases.WeekGraceEnds : EvaluatorCases.DayGraceEnds;

    /// <summary>An instant after the grace end, so the result is final.</summary>
    public DateTimeOffset AfterGrace => PerWeek ? Utc("2026-12-02T09:00:00Z") : Utc("2026-11-21T09:00:00Z");

    public RecordSet Set(string name) => Sets.Single(set => set.Name == name);

    public EntryEvaluation EvaluateAt(DateTimeOffset now, IReadOnlyList<RecordValue> records) =>
        Evaluator.Evaluate(Target, Starts, Ends, records, now);
}

/// <summary>
/// Every combination of SC-002, for <see cref="EvaluatorTargetTests"/> and <see cref="EvaluatorStateTests"/>. The
/// instants are Europe/Kyiv (UTC+2 in November 2026): the day is Wednesday 18 November 2026, the week Monday 23 to
/// Sunday 29 November 2026, each with its grace end 48 hours after its end.
/// </summary>
public static class EvaluatorCases
{
    public const string Below = "below";
    public const string At = "at";
    public const string Above = "above";

    public static readonly DateTimeOffset DayStarts = Utc("2026-11-17T22:00:00Z");
    public static readonly DateTimeOffset DayEnds = Utc("2026-11-18T22:00:00Z");
    public static readonly DateTimeOffset DayGraceEnds = Utc("2026-11-20T22:00:00Z");

    public static readonly DateTimeOffset WeekStarts = Utc("2026-11-22T22:00:00Z");
    public static readonly DateTimeOffset WeekEnds = Utc("2026-11-29T22:00:00Z");
    public static readonly DateTimeOffset WeekGraceEnds = Utc("2026-12-01T22:00:00Z");

    public static readonly RecordValue Yes = RecordValue.FromAnswer(YesNoAnswer.Yes);
    public static readonly RecordValue No = RecordValue.FromAnswer(YesNoAnswer.No);

    private const EvaluationState Met = EvaluationState.Met;
    private const EvaluationState Failed = EvaluationState.Failed;

    public static RecordValue[] Amounts(params int[] amounts) => amounts.Select(RecordValue.FromAmount).ToArray();

    public static readonly IReadOnlyList<TargetCase> All =
    [
        // Commitment-based (fixed days).
        new("Build yes/no commitment", Target(YesNoDays(BuildOrReduce.Build, AllDays)), false, Value(null, 1),
        [
            new(Below, [No], Failed, Value(0, 1)),
            new(At, [Yes], Met, Value(1, 1)),
        ]),
        new("Build count commitment, at least 20 pages", Target(CountDays(20, "pages", BuildOrReduce.Build, AllDays)), false, Value(null, 20),
        [
            new(Below, Amounts(19), Failed, Value(19, 20)),
            new(At, Amounts(20), Met, Value(20, 20)),
            new(Above, Amounts(21), Met, Value(21, 20)),
        ]),
        new("Build duration commitment, at least 20 min", Target(DurationDays(20, DurationUnit.Minutes, BuildOrReduce.Build, AllDays)), false, Value(null, 20),
        [
            new(Below, Amounts(19), Failed, Value(19, 20)),
            new(At, Amounts(20), Met, Value(20, 20)),
            new(Above, Amounts(21), Met, Value(21, 20)),
        ]),
        new("Build duration commitment, at least 1 h", Target(DurationDays(1, DurationUnit.Hours, BuildOrReduce.Build, AllDays)), false, Value(null, 60),
        [
            new(Below, Amounts(59), Failed, Value(59, 60)),
            new(At, Amounts(60), Met, Value(60, 60)),
            new(Above, Amounts(61), Met, Value(61, 60)),
        ]),
        new("Reduce yes/no commitment", Target(YesNoDays(BuildOrReduce.Reduce, AllDays)), false, Value(null, 1),
        [
            new(At, [No], Met, Value(0, 1)),
            new(Above, [Yes], Failed, Value(1, 1)),
        ]),
        new("Reduce count commitment, at most 2 cups", Target(CountDays(2, "cups", BuildOrReduce.Reduce, AllDays)), false, Value(null, 2),
        [
            new(Below, Amounts(1), Met, Value(1, 2)),
            new(At, Amounts(2), Met, Value(2, 2)),
            new(Above, Amounts(3), Failed, Value(3, 2)),
        ]),
        new("Reduce count commitment, at most 0 cigarettes", Target(CountDays(0, "cigarettes", BuildOrReduce.Reduce, AllDays)), false, Value(null, 0),
        [
            new(At, Amounts(0), Met, Value(0, 0)),
            new(Above, Amounts(1), Failed, Value(1, 0)),
        ]),
        new("Reduce duration commitment, at most 30 min", Target(DurationDays(30, DurationUnit.Minutes, BuildOrReduce.Reduce, AllDays)), false, Value(null, 30),
        [
            new(Below, Amounts(29), Met, Value(29, 30)),
            new(At, Amounts(30), Met, Value(30, 30)),
            new(Above, Amounts(31), Failed, Value(31, 30)),
        ]),

        // Period-based, per day.
        new("Build yes/no, 2 times a day", Target(YesNoTimes(2, SchedulePeriod.Day)), false, Reaching(0, 2),
        [
            new(Below, [Yes, No], Failed, Reaching(1, 2)),
            new(At, [Yes, Yes], Met, Reaching(2, 2)),
            new(Above, [Yes, Yes, Yes], Met, Reaching(3, 2)),
        ]),
        new("Build count, 10 push-ups 2 times a day", Target(CountTimes(10, "push-ups", 2, SchedulePeriod.Day)), false, Reaching(0, 2),
        [
            new(Below, Amounts(10, 9), Failed, Reaching(1, 2)),
            new(At, Amounts(10, 10), Met, Reaching(2, 2)),
            new(Above, Amounts(11, 12), Met, Reaching(2, 2)),
        ]),
        new("Build duration, 20 min once a day", Target(DurationTimes(20, DurationUnit.Minutes, 1, SchedulePeriod.Day)), false, Reaching(0, 1),
        [
            new(Below, Amounts(19), Failed, Reaching(0, 1)),
            new(At, Amounts(20), Met, Reaching(1, 1)),
            new(Above, Amounts(21), Met, Reaching(1, 1)),
        ]),
        new("Reduce yes/no, at most once a day", Target(YesNoTimes(1, SchedulePeriod.Day, BuildOrReduce.Reduce)), false, YesRecords(0, 1),
        [
            new(Below, [No], Met, YesRecords(0, 1)),
            new(At, [Yes], Met, YesRecords(1, 1)),
            new(Above, [Yes, Yes], Failed, YesRecords(2, 1)),
        ]),
        new("Reduce count, at most 1 cigarette a day", Target(CountTimes(1, "cigarette", 1, SchedulePeriod.Day, BuildOrReduce.Reduce)), false, Total(0, 1),
        [
            new(Below, Amounts(0), Met, Total(0, 1)),
            new(At, Amounts(0, 1), Met, Total(1, 1)),
            new(Above, Amounts(1, 1), Failed, Total(2, 1)),
        ]),
        new("Reduce duration, at most 60 min a day", Target(DurationTimes(60, DurationUnit.Minutes, 1, SchedulePeriod.Day, BuildOrReduce.Reduce)), false, Total(0, 60),
        [
            new(Below, Amounts(30, 29), Met, Total(59, 60)),
            new(At, Amounts(30, 30), Met, Total(60, 60)),
            new(Above, Amounts(30, 31), Failed, Total(61, 60)),
        ]),

        // Period-based, per week.
        new("Build yes/no, 3 times a week", Target(YesNoTimes(3, SchedulePeriod.Week)), true, Reaching(0, 3),
        [
            new(Below, [Yes, Yes, No], Failed, Reaching(2, 3)),
            new(At, [Yes, Yes, Yes], Met, Reaching(3, 3)),
            new(Above, [Yes, Yes, Yes, Yes], Met, Reaching(4, 3)),
        ]),
        new("Build count, 20 pages 3 times a week", Target(CountTimes(20, "pages", 3, SchedulePeriod.Week)), true, Reaching(0, 3),
        [
            new(Below, Amounts(20, 25, 12), Failed, Reaching(2, 3)),
            new(At, Amounts(20, 25, 12, 30), Met, Reaching(3, 3)),
            new(Above, Amounts(20, 25, 30, 21), Met, Reaching(4, 3)),
        ]),
        new("Build duration, 30 min 2 times a week", Target(DurationTimes(30, DurationUnit.Minutes, 2, SchedulePeriod.Week)), true, Reaching(0, 2),
        [
            new(Below, Amounts(30, 29), Failed, Reaching(1, 2)),
            new(At, Amounts(30, 30), Met, Reaching(2, 2)),
            new(Above, Amounts(31, 45), Met, Reaching(2, 2)),
        ]),
        new("Reduce yes/no, at most 2 times a week", Target(YesNoTimes(2, SchedulePeriod.Week, BuildOrReduce.Reduce)), true, YesRecords(0, 2),
        [
            new(Below, [Yes, No], Met, YesRecords(1, 2)),
            new(At, [Yes, Yes, No], Met, YesRecords(2, 2)),
            new(Above, [Yes, Yes, Yes], Failed, YesRecords(3, 2)),
        ]),
        new("Reduce count, at most 10 sweets a week", Target(CountTimes(10, "sweets", 1, SchedulePeriod.Week, BuildOrReduce.Reduce)), true, Total(0, 10),
        [
            new(Below, Amounts(4, 5), Met, Total(9, 10)),
            new(At, Amounts(4, 6), Met, Total(10, 10)),
            new(Above, Amounts(4, 7), Failed, Total(11, 10)),
        ]),
        new("Reduce duration, at most 2 h a week", Target(DurationTimes(2, DurationUnit.Hours, 1, SchedulePeriod.Week, BuildOrReduce.Reduce)), true, Total(0, 120),
        [
            new(Below, Amounts(60, 59), Met, Total(119, 120)),
            new(At, Amounts(60, 60), Met, Total(120, 120)),
            new(Above, Amounts(60, 61), Failed, Total(121, 120)),
        ]),
    ];

    public static TargetCase Named(string name) => All.Single(c => c.Name == name);

    public static TheoryData<string> Names() => new(All.Select(c => c.Name));

    public static TheoryData<string, string> NamesAndSets()
    {
        var data = new TheoryData<string, string>();
        foreach (var targetCase in All)
        {
            foreach (var set in targetCase.Sets)
            {
                data.Add(targetCase.Name, set.Name);
            }
        }

        return data;
    }

    private static Progress Value(int? value, int of) => new(ProgressKind.RecordedValue, value, of);

    private static Progress Reaching(int value, int of) => new(ProgressKind.RecordsReachingAmount, value, of);

    private static Progress YesRecords(int value, int of) => new(ProgressKind.YesRecords, value, of);

    private static Progress Total(int value, int of) => new(ProgressKind.Total, value, of);
}
