using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;

namespace HT.Domain.Evaluation;

/// <summary>
/// Evaluates one commitment or evaluation period (FR-010, FR-012 to FR-016; research R-01). A pure function of the
/// target of the version in effect for the entry's date, the entry's instants, its records and a supplied time: no
/// clock, no time zone and no culture. Nothing is stored, so an entry turns final with no action (HP-010). Not named
/// <c>Evaluation</c>, which is its namespace.
/// </summary>
public static class Evaluator
{
    private const int MinutesPerHour = 60;

    /// <summary>
    /// The entry's state and progress at <paramref name="now"/>: <see cref="EvaluationState.Upcoming"/> when it starts
    /// after <paramref name="now"/>; <see cref="EvaluationState.Due"/> from its start until its end;
    /// <see cref="EvaluationState.Pending"/> from its end until its grace period ends; otherwise final, with that
    /// instant included: <see cref="EvaluationState.Missed"/> with no record, for Build and Reduce alike (FR-013), or
    /// <see cref="EvaluationState.Met"/> or <see cref="EvaluationState.Failed"/> by the target (FR-014). The progress is
    /// given in every state. The order of <paramref name="records"/> does not matter.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// <paramref name="ends"/> has no UTC offset, a record does not match the target's measurement, or a commitment has
    /// more than one record. The messages name no value.
    /// </exception>
    public static EntryEvaluation Evaluate(
        GoalTarget target, DateTimeOffset starts, DateTimeOffset ends, IReadOnlyList<RecordValue> records, DateTimeOffset now)
    {
        var progress = ProgressOf(target, records);
        var graceEnds = GracePeriod.EndOf(ends);
        var state = now < starts ? EvaluationState.Upcoming
            : now < ends ? EvaluationState.Due
            : now < graceEnds ? EvaluationState.Pending
            : records.Count == 0 ? EvaluationState.Missed
            : IsMet(target, progress) ? EvaluationState.Met
            : EvaluationState.Failed;
        return new EntryEvaluation(state, progress, graceEnds, now >= graceEnds);
    }

    /// <summary>
    /// The progress of an entry with <paramref name="records"/> (FR-014), without instants or a clock:
    /// <list type="bullet">
    ///   <item>a commitment: <see cref="ProgressKind.RecordedValue"/>, the amount (or Yes 1, No 0; null with no record) against the amount (or 1);</item>
    ///   <item>a Build period: <see cref="ProgressKind.RecordsReachingAmount"/>, the records reaching the amount (or the Yes records) against N;</item>
    ///   <item>a Reduce yes/no period: <see cref="ProgressKind.YesRecords"/>, the Yes records against at most N;</item>
    ///   <item>a Reduce count or duration period: <see cref="ProgressKind.Total"/>, the total against at most the amount.</item>
    /// </list>
    /// A duration in hours is compared in minutes (the amount × 60).
    /// </summary>
    /// <exception cref="ArgumentException">A record does not match the target's measurement, or a commitment has more than one record.</exception>
    public static Progress ProgressOf(GoalTarget target, IReadOnlyList<RecordValue> records)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(records);
        CheckKinds(target.Measurement, records);
        var yesNo = target.Measurement == Measurement.YesNo;
        var amount = yesNo ? 1 : AmountInRecordUnits(target);

        if (target.ScheduleKindOf == GoalScheduleKind.CommitmentBased)
        {
            if (records.Count > 1)
            {
                throw new ArgumentException("A commitment has at most one record.", nameof(records));
            }

            int? value = records.Count == 0 ? null : NumberOf(records[0]);
            return new Progress(ProgressKind.RecordedValue, value, amount);
        }

        var times = target.Schedule.Times ?? 1;
        if (target.BuildOrReduce == BuildOrReduce.Build)
        {
            var reaching = yesNo ? records.Count(IsYes) : records.Count(record => record.Amount >= amount);
            return new Progress(ProgressKind.RecordsReachingAmount, reaching, times);
        }

        return yesNo
            ? new Progress(ProgressKind.YesRecords, records.Count(IsYes), times)
            : new Progress(ProgressKind.Total, records.Sum(record => record.Amount!.Value), amount);
    }

    /// <summary>Research R-01's table: Build at least, Reduce at most; a Reduce yes/no commitment is met by "No".</summary>
    private static bool IsMet(GoalTarget target, Progress progress)
    {
        var value = progress.Value ?? 0;
        return progress.Kind switch
        {
            ProgressKind.RecordedValue when target.BuildOrReduce == BuildOrReduce.Build => value >= progress.Of,
            ProgressKind.RecordedValue when target.Measurement == Measurement.YesNo => value == 0,
            ProgressKind.RecordedValue => value <= progress.Of,
            ProgressKind.RecordsReachingAmount => value >= progress.Of,
            ProgressKind.YesRecords or ProgressKind.Total => value <= progress.Of,
            _ => throw new ArgumentOutOfRangeException(nameof(progress), "The progress kind is not defined."),
        };
    }

    private static int AmountInRecordUnits(GoalTarget target)
    {
        var amount = target.Amount ?? 0;
        return target.DurationUnit == DurationUnit.Hours ? amount * MinutesPerHour : amount;
    }

    private static int NumberOf(RecordValue record) => record.Answer is { } answer ? (answer == YesNoAnswer.Yes ? 1 : 0) : record.Amount!.Value;

    private static bool IsYes(RecordValue record) => record.Answer == YesNoAnswer.Yes;

    private static void CheckKinds(Measurement measurement, IReadOnlyList<RecordValue> records)
    {
        var yesNo = measurement == Measurement.YesNo;
        foreach (var record in records)
        {
            if (record is null || (record.Answer is not null) != yesNo)
            {
                throw new ArgumentException("A record does not match the target's measurement.", nameof(records));
            }
        }
    }
}
