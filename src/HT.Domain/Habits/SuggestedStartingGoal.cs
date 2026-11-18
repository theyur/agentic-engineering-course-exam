using System.Diagnostics.CodeAnalysis;

namespace HT.Domain.Habits;

/// <summary>
/// An example target shown with a catalog habit (FR-007): Build or Reduce, the measurement, the amount and unit
/// for count and duration, and the schedule. Immutable; only <see cref="TryCreate"/> makes one. Values that do not
/// belong to the measurement or the schedule kind are not stored (research R-05).
/// </summary>
public sealed class SuggestedStartingGoal
{
    public const int MaxAmount = TargetFieldCheck.MaxAmount;
    public const int MinBuildAmount = TargetFieldCheck.MinBuildAmount;
    public const int MinReduceAmount = TargetFieldCheck.MinReduceAmount;
    public const int CountUnitMaxLength = TargetFieldCheck.CountUnitMaxLength;

    private SuggestedStartingGoal(
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

    /// <summary>
    /// Checks the fields in FR-007's order: Build or Reduce, measurement, a yes/no without amount or unit (a count
    /// unit empty after trimming is no unit), the amount's range for Build (1–999) or Reduce (0–999), the count
    /// unit (1–20 after trimming), the duration unit, the schedule kind, the days, the number of times (missing or
    /// outside 1–20) and the period. The checks are shared with a goal's target (research R-05).
    /// </summary>
    public static bool TryCreate(
        SuggestedStartingGoalFields fields,
        [NotNullWhen(true)] out SuggestedStartingGoal? goal,
        out SuggestedStartingGoalProblem problem)
    {
        ArgumentNullException.ThrowIfNull(fields);
        goal = null;
        var values = new TargetFieldValues(
            fields.BuildOrReduce, fields.Measurement, fields.Amount, fields.CountUnit, fields.DurationUnit,
            fields.ScheduleKind, fields.Days, fields.Times, fields.Period);
        problem = ProblemOf(TargetFieldCheck.Check(values));
        if (problem != SuggestedStartingGoalProblem.None)
        {
            return false;
        }

        goal = new SuggestedStartingGoal(
            fields.BuildOrReduce!.Value,
            fields.Measurement!.Value,
            TargetFieldCheck.StoredAmount(values),
            TargetFieldCheck.StoredCountUnit(values),
            TargetFieldCheck.StoredDurationUnit(values),
            TargetFieldCheck.ScheduleOf(values));
        return true;
    }

    private static SuggestedStartingGoalProblem ProblemOf(TargetFieldProblem problem) => problem switch
    {
        TargetFieldProblem.None => SuggestedStartingGoalProblem.None,
        TargetFieldProblem.BuildOrReduceMissing => SuggestedStartingGoalProblem.BuildOrReduceMissing,
        TargetFieldProblem.MeasurementMissing => SuggestedStartingGoalProblem.MeasurementMissing,
        TargetFieldProblem.YesNoHasAmountOrUnit => SuggestedStartingGoalProblem.YesNoHasAmountOrUnit,
        TargetFieldProblem.BuildAmountOutOfRange => SuggestedStartingGoalProblem.BuildAmountOutOfRange,
        TargetFieldProblem.ReduceAmountOutOfRange => SuggestedStartingGoalProblem.ReduceAmountOutOfRange,
        TargetFieldProblem.CountUnitMissing => SuggestedStartingGoalProblem.CountUnitMissing,
        TargetFieldProblem.CountUnitTooLong => SuggestedStartingGoalProblem.CountUnitTooLong,
        TargetFieldProblem.DurationUnitMissing => SuggestedStartingGoalProblem.DurationUnitMissing,
        TargetFieldProblem.ScheduleMissing => SuggestedStartingGoalProblem.ScheduleMissing,
        TargetFieldProblem.DaysMissing => SuggestedStartingGoalProblem.DaysMissing,
        TargetFieldProblem.TimesOutOfRange => SuggestedStartingGoalProblem.TimesOutOfRange,
        TargetFieldProblem.PeriodMissing => SuggestedStartingGoalProblem.PeriodMissing,
        _ => throw new ArgumentOutOfRangeException(nameof(problem), problem, null),
    };
}
