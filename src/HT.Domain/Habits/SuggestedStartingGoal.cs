using System.Diagnostics.CodeAnalysis;

namespace HT.Domain.Habits;

/// <summary>
/// An example target shown with a catalog habit (FR-007): Build or Reduce, the measurement, the amount and unit
/// for count and duration, and the schedule. Immutable; only <see cref="TryCreate"/> makes one. Values that do not
/// belong to the measurement or the schedule kind are not stored (research R-05).
/// </summary>
public sealed class SuggestedStartingGoal
{
    public const int MaxAmount = 999;
    public const int MinBuildAmount = 1;
    public const int MinReduceAmount = 0;
    public const int CountUnitMaxLength = 20;

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
    /// outside 1–20) and the period.
    /// </summary>
    public static bool TryCreate(
        SuggestedStartingGoalFields fields,
        [NotNullWhen(true)] out SuggestedStartingGoal? goal,
        out SuggestedStartingGoalProblem problem)
    {
        ArgumentNullException.ThrowIfNull(fields);
        goal = null;
        problem = Check(fields, out var countUnit);
        if (problem != SuggestedStartingGoalProblem.None)
        {
            return false;
        }

        var measurement = fields.Measurement!.Value;
        var schedule = fields.ScheduleKind == ScheduleKind.FixedDays
            ? Schedule.FixedDays(ValidDays(fields.Days))
            : Schedule.TimesPer(fields.Times!.Value, fields.Period!.Value);
        goal = new SuggestedStartingGoal(
            fields.BuildOrReduce!.Value,
            measurement,
            measurement == Measurement.YesNo ? null : fields.Amount,
            measurement == Measurement.Count ? countUnit : null,
            measurement == Measurement.Duration ? fields.DurationUnit : null,
            schedule);
        return true;
    }

    private static SuggestedStartingGoalProblem Check(SuggestedStartingGoalFields fields, out string? countUnit)
    {
        countUnit = fields.CountUnit?.Trim();
        if (string.IsNullOrEmpty(countUnit))
        {
            countUnit = null;
        }

        if (fields.BuildOrReduce is not { } buildOrReduce || !Enum.IsDefined(buildOrReduce))
        {
            return SuggestedStartingGoalProblem.BuildOrReduceMissing;
        }

        if (fields.Measurement is not { } measurement || !Enum.IsDefined(measurement))
        {
            return SuggestedStartingGoalProblem.MeasurementMissing;
        }

        var amountProblem = measurement == Measurement.YesNo
            ? CheckYesNo(fields, countUnit)
            : CheckAmountAndUnit(fields, buildOrReduce, measurement, countUnit);
        return amountProblem != SuggestedStartingGoalProblem.None ? amountProblem : CheckSchedule(fields);
    }

    private static SuggestedStartingGoalProblem CheckYesNo(SuggestedStartingGoalFields fields, string? countUnit) =>
        fields.Amount is not null || countUnit is not null || fields.DurationUnit is not null
            ? SuggestedStartingGoalProblem.YesNoHasAmountOrUnit
            : SuggestedStartingGoalProblem.None;

    private static SuggestedStartingGoalProblem CheckAmountAndUnit(
        SuggestedStartingGoalFields fields, BuildOrReduce buildOrReduce, Measurement measurement, string? countUnit)
    {
        var minimum = buildOrReduce == BuildOrReduce.Build ? MinBuildAmount : MinReduceAmount;
        if (fields.Amount is not { } amount || amount < minimum || amount > MaxAmount)
        {
            return buildOrReduce == BuildOrReduce.Build
                ? SuggestedStartingGoalProblem.BuildAmountOutOfRange
                : SuggestedStartingGoalProblem.ReduceAmountOutOfRange;
        }

        if (measurement == Measurement.Count)
        {
            if (countUnit is null)
            {
                return SuggestedStartingGoalProblem.CountUnitMissing;
            }

            return countUnit.Length > CountUnitMaxLength
                ? SuggestedStartingGoalProblem.CountUnitTooLong
                : SuggestedStartingGoalProblem.None;
        }

        return fields.DurationUnit is { } unit && Enum.IsDefined(unit)
            ? SuggestedStartingGoalProblem.None
            : SuggestedStartingGoalProblem.DurationUnitMissing;
    }

    private static SuggestedStartingGoalProblem CheckSchedule(SuggestedStartingGoalFields fields)
    {
        if (fields.ScheduleKind is not { } kind || !Enum.IsDefined(kind))
        {
            return SuggestedStartingGoalProblem.ScheduleMissing;
        }

        if (kind == ScheduleKind.FixedDays)
        {
            return ValidDays(fields.Days).Any() ? SuggestedStartingGoalProblem.None : SuggestedStartingGoalProblem.DaysMissing;
        }

        if (fields.Times is not { } times || times < Schedule.MinTimes || times > Schedule.MaxTimes)
        {
            return SuggestedStartingGoalProblem.TimesOutOfRange;
        }

        return fields.Period is { } period && Enum.IsDefined(period)
            ? SuggestedStartingGoalProblem.None
            : SuggestedStartingGoalProblem.PeriodMissing;
    }

    private static IEnumerable<DayOfWeek> ValidDays(IReadOnlySet<DayOfWeek>? days) =>
        days is null ? [] : days.Where(Enum.IsDefined);
}
