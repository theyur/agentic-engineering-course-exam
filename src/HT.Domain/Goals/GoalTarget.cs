using System.Diagnostics.CodeAnalysis;
using HT.Domain.Habits;

namespace HT.Domain.Goals;

/// <summary>
/// A goal's target (FR-002, FR-004): Build or Reduce, the measurement, the amount and unit for count and duration,
/// and the schedule. It reuses slice 2's target types and checks (research R-05). Immutable; only
/// <see cref="TryCreate"/> makes one, so every target satisfies the checks, including the Reduce N rule. Values
/// that do not belong to the measurement or the schedule kind are not stored. Two targets with the same values are
/// equal.
/// </summary>
public sealed class GoalTarget : IEquatable<GoalTarget>
{
    private GoalTarget(
        BuildOrReduce buildOrReduce, Measurement measurement, int? amount, string? countUnit, DurationUnit? durationUnit, Schedule schedule)
    {
        BuildOrReduce = buildOrReduce;
        Measurement = measurement;
        Amount = amount;
        CountUnit = countUnit;
        DurationUnit = durationUnit;
        Schedule = schedule;
    }

    public BuildOrReduce BuildOrReduce { get; }

    public Measurement Measurement { get; }

    /// <summary>The amount for count and duration; null for yes/no.</summary>
    public int? Amount { get; }

    /// <summary>The trimmed unit for count, with its case and spelling unchanged; null otherwise.</summary>
    public string? CountUnit { get; }

    /// <summary>The unit for duration; null otherwise.</summary>
    public DurationUnit? DurationUnit { get; }

    public Schedule Schedule { get; }

    /// <summary>Commitment-based for fixed days, period-based for N times (FR-003).</summary>
    public GoalScheduleKind ScheduleKindOf =>
        Schedule.Kind == ScheduleKind.FixedDays ? GoalScheduleKind.CommitmentBased : GoalScheduleKind.PeriodBased;

    /// <summary>The length of an evaluation period of a period-based goal (day or week); null for commitment-based.</summary>
    public SchedulePeriod? PeriodLength => Schedule.Kind == ScheduleKind.Times ? Schedule.Period : null;

    /// <summary>
    /// Checks the target fields in FR-006's order: slice 2's target checks (<see cref="GoalProblem.BuildOrReduceMissing"/>
    /// to <see cref="GoalProblem.PeriodMissing"/>), then <see cref="GoalProblem.ReduceAmountNeedsOneTime"/>: a Reduce
    /// count or duration target of N times with N other than 1 (FR-004). It also rebuilds a stored target.
    /// </summary>
    public static bool TryCreate(GoalTargetFields fields, [NotNullWhen(true)] out GoalTarget? target, out GoalProblem problem)
    {
        ArgumentNullException.ThrowIfNull(fields);
        target = null;
        var values = new TargetFieldValues(
            fields.BuildOrReduce, fields.Measurement, fields.Amount, fields.CountUnit, fields.DurationUnit,
            fields.ScheduleKind, fields.Days, fields.Times, fields.Period);
        problem = ProblemOf(TargetFieldCheck.Check(values));
        if (problem != GoalProblem.None)
        {
            return false;
        }

        if (fields.BuildOrReduce == BuildOrReduce.Reduce
            && fields.Measurement != Measurement.YesNo
            && fields.ScheduleKind == ScheduleKind.Times
            && fields.Times != 1)
        {
            problem = GoalProblem.ReduceAmountNeedsOneTime;
            return false;
        }

        target = new GoalTarget(
            fields.BuildOrReduce!.Value,
            fields.Measurement!.Value,
            TargetFieldCheck.StoredAmount(values),
            TargetFieldCheck.StoredCountUnit(values),
            TargetFieldCheck.StoredDurationUnit(values),
            TargetFieldCheck.ScheduleOf(values));
        return true;
    }

    public bool Equals(GoalTarget? other) =>
        other is not null
        && BuildOrReduce == other.BuildOrReduce
        && Measurement == other.Measurement
        && Amount == other.Amount
        && string.Equals(CountUnit, other.CountUnit, StringComparison.Ordinal)
        && DurationUnit == other.DurationUnit
        && Schedule.Kind == other.Schedule.Kind
        && Schedule.Days.SetEquals(other.Schedule.Days)
        && Schedule.Times == other.Schedule.Times
        && Schedule.Period == other.Schedule.Period;

    public override bool Equals(object? obj) => Equals(obj as GoalTarget);

    public override int GetHashCode()
    {
        var days = Schedule.Days.Aggregate(0, (mask, day) => mask | (1 << (int)day));
        return HashCode.Combine(
            BuildOrReduce, Measurement, Amount, CountUnit is null ? 0 : StringComparer.Ordinal.GetHashCode(CountUnit),
            DurationUnit, Schedule.Kind, days, HashCode.Combine(Schedule.Times, Schedule.Period));
    }

    private static GoalProblem ProblemOf(TargetFieldProblem problem) => problem switch
    {
        TargetFieldProblem.None => GoalProblem.None,
        TargetFieldProblem.BuildOrReduceMissing => GoalProblem.BuildOrReduceMissing,
        TargetFieldProblem.MeasurementMissing => GoalProblem.MeasurementMissing,
        TargetFieldProblem.YesNoHasAmountOrUnit => GoalProblem.YesNoHasAmountOrUnit,
        TargetFieldProblem.BuildAmountOutOfRange => GoalProblem.BuildAmountOutOfRange,
        TargetFieldProblem.ReduceAmountOutOfRange => GoalProblem.ReduceAmountOutOfRange,
        TargetFieldProblem.CountUnitMissing => GoalProblem.CountUnitMissing,
        TargetFieldProblem.CountUnitTooLong => GoalProblem.CountUnitTooLong,
        TargetFieldProblem.DurationUnitMissing => GoalProblem.DurationUnitMissing,
        TargetFieldProblem.ScheduleMissing => GoalProblem.ScheduleMissing,
        TargetFieldProblem.DaysMissing => GoalProblem.DaysMissing,
        TargetFieldProblem.TimesOutOfRange => GoalProblem.TimesOutOfRange,
        TargetFieldProblem.PeriodMissing => GoalProblem.PeriodMissing,
        _ => throw new ArgumentOutOfRangeException(nameof(problem), problem, null),
    };
}
