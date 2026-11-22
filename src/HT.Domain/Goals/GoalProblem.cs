namespace HT.Domain.Goals;

/// <summary>
/// The problems of a new goal, in FR-006's check order: the habit, the 12 target problems in slice 2's order, the
/// Reduce N rule, the start date and the name. <see cref="None"/> when the goal is accepted.
/// <see cref="StartDateNotAfterEnding"/> belongs only to starting a new attempt (slice 8 FR-012); creating a goal never
/// produces it.
/// </summary>
public enum GoalProblem
{
    None,
    HabitMissing,
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
    ReduceAmountNeedsOneTime,
    StartDateMissing,
    StartDateInPast,
    StartDateNotAfterEnding,
    StartDateTooFar,
    NameTooLong,
}
