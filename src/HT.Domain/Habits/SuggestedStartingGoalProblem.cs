namespace HT.Domain.Habits;

/// <summary>
/// The problems of one suggested starting goal, in FR-007's check order (the two amount problems share step 4).
/// <see cref="None"/> when the fields are accepted.
/// </summary>
public enum SuggestedStartingGoalProblem
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
