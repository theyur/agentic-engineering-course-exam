namespace HT.Domain.Habits;

/// <summary>
/// The unchecked target fields, one nullable value per form control, shared by a suggested starting goal and a
/// goal (research R-05).
/// </summary>
internal readonly record struct TargetFieldValues(
    BuildOrReduce? BuildOrReduce,
    Measurement? Measurement,
    int? Amount,
    string? CountUnit,
    DurationUnit? DurationUnit,
    ScheduleKind? ScheduleKind,
    IReadOnlySet<DayOfWeek>? Days,
    int? Times,
    SchedulePeriod? Period);

/// <summary>The target checks' problems, in slice 2 FR-007's order. <see cref="None"/> when the fields are accepted.</summary>
internal enum TargetFieldProblem
{
    None,
    BuildOrReduceMissing,
    MeasurementMissing,
    YesNoHasAmountOrUnit,
    BuildAmountOutOfRange,
    ReduceAmountOutOfRange,
    CountUnitMissing,
    CountUnitTooLong,
    DurationUnitMissing,
    ScheduleMissing,
    DaysMissing,
    TimesOutOfRange,
    PeriodMissing,
}

/// <summary>
/// The one implementation of the target checks of slice 2 FR-007, in their order, which both
/// <see cref="SuggestedStartingGoal.TryCreate"/> and a goal's target use (research R-05), and the values an accepted
/// target stores. Values that do not belong to the measurement or the schedule kind are neither refused nor stored.
/// </summary>
internal static class TargetFieldCheck
{
    public const int MaxAmount = 999;
    public const int MinBuildAmount = 1;
    public const int MinReduceAmount = 0;
    public const int CountUnitMaxLength = 20;

    /// <summary>
    /// Checks Build or Reduce, the measurement, a yes/no without amount or unit (a count unit empty after trimming is
    /// no unit), the amount's range for Build (1–999) or Reduce (0–999), the count unit (1–20 after trimming), the
    /// duration unit, the schedule kind, the days, the number of times (missing or outside 1–20) and the period.
    /// </summary>
    public static TargetFieldProblem Check(TargetFieldValues fields)
    {
        var countUnit = TrimmedUnit(fields.CountUnit);
        if (fields.BuildOrReduce is not { } buildOrReduce || !Enum.IsDefined(buildOrReduce))
        {
            return TargetFieldProblem.BuildOrReduceMissing;
        }

        if (fields.Measurement is not { } measurement || !Enum.IsDefined(measurement))
        {
            return TargetFieldProblem.MeasurementMissing;
        }

        var amountProblem = measurement == Measurement.YesNo
            ? CheckYesNo(fields, countUnit)
            : CheckAmountAndUnit(fields, buildOrReduce, measurement, countUnit);
        return amountProblem != TargetFieldProblem.None ? amountProblem : CheckSchedule(fields);
    }

    /// <summary>The amount an accepted target stores: null for yes/no.</summary>
    public static int? StoredAmount(TargetFieldValues fields) =>
        fields.Measurement == Measurement.YesNo ? null : fields.Amount;

    /// <summary>The count unit an accepted target stores: trimmed for count, null otherwise.</summary>
    public static string? StoredCountUnit(TargetFieldValues fields) =>
        fields.Measurement == Measurement.Count ? TrimmedUnit(fields.CountUnit) : null;

    /// <summary>The duration unit an accepted target stores: for duration only.</summary>
    public static DurationUnit? StoredDurationUnit(TargetFieldValues fields) =>
        fields.Measurement == Measurement.Duration ? fields.DurationUnit : null;

    /// <summary>The schedule of accepted fields: the defined days for fixed days, otherwise the times and the period.</summary>
    public static Schedule ScheduleOf(TargetFieldValues fields) =>
        fields.ScheduleKind == ScheduleKind.FixedDays
            ? Schedule.FixedDays(ValidDays(fields.Days))
            : Schedule.TimesPer(fields.Times!.Value, fields.Period!.Value);

    private static string? TrimmedUnit(string? unit)
    {
        var trimmed = unit?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    private static TargetFieldProblem CheckYesNo(TargetFieldValues fields, string? countUnit) =>
        fields.Amount is not null || countUnit is not null || fields.DurationUnit is not null
            ? TargetFieldProblem.YesNoHasAmountOrUnit
            : TargetFieldProblem.None;

    private static TargetFieldProblem CheckAmountAndUnit(
        TargetFieldValues fields, BuildOrReduce buildOrReduce, Measurement measurement, string? countUnit)
    {
        var minimum = buildOrReduce == BuildOrReduce.Build ? MinBuildAmount : MinReduceAmount;
        if (fields.Amount is not { } amount || amount < minimum || amount > MaxAmount)
        {
            return buildOrReduce == BuildOrReduce.Build
                ? TargetFieldProblem.BuildAmountOutOfRange
                : TargetFieldProblem.ReduceAmountOutOfRange;
        }

        if (measurement == Measurement.Count)
        {
            if (countUnit is null)
            {
                return TargetFieldProblem.CountUnitMissing;
            }

            return countUnit.Length > CountUnitMaxLength
                ? TargetFieldProblem.CountUnitTooLong
                : TargetFieldProblem.None;
        }

        return fields.DurationUnit is { } unit && Enum.IsDefined(unit)
            ? TargetFieldProblem.None
            : TargetFieldProblem.DurationUnitMissing;
    }

    private static TargetFieldProblem CheckSchedule(TargetFieldValues fields)
    {
        if (fields.ScheduleKind is not { } kind || !Enum.IsDefined(kind))
        {
            return TargetFieldProblem.ScheduleMissing;
        }

        if (kind == ScheduleKind.FixedDays)
        {
            return ValidDays(fields.Days).Any() ? TargetFieldProblem.None : TargetFieldProblem.DaysMissing;
        }

        if (fields.Times is not { } times || times < Schedule.MinTimes || times > Schedule.MaxTimes)
        {
            return TargetFieldProblem.TimesOutOfRange;
        }

        return fields.Period is { } period && Enum.IsDefined(period)
            ? TargetFieldProblem.None
            : TargetFieldProblem.PeriodMissing;
    }

    private static IEnumerable<DayOfWeek> ValidDays(IReadOnlySet<DayOfWeek>? days) =>
        days is null ? [] : days.Where(Enum.IsDefined);
}
