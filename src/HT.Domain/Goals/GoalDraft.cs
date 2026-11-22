namespace HT.Domain.Goals;

/// <summary>
/// The checks of a new goal, in FR-006's order (research R-05, R-07). Application calls them in two steps, with the
/// habit lookup between: <see cref="CheckHabit"/>, then the lookup, then <see cref="TryCreate"/>.
/// </summary>
public static class GoalDraft
{
    /// <summary>The latest start date is today + 365 days (FR-006).</summary>
    public const int MaxDaysAhead = 365;

    /// <summary><see cref="GoalProblem.HabitMissing"/> when there is no habit (or its kind is undefined); otherwise <see cref="GoalProblem.None"/>.</summary>
    public static GoalProblem CheckHabit(GoalHabit? habit) =>
        habit is { } chosen && Enum.IsDefined(chosen.Kind) ? GoalProblem.None : GoalProblem.HabitMissing;

    /// <summary>
    /// Checks, in order: the target (<see cref="GoalTarget.TryCreate"/>: slice 2's target checks with the goal's
    /// yes/no problem, then <see cref="GoalProblem.ReduceAmountNeedsOneTime"/>); the start date, missing, before
    /// <paramref name="today"/>, or after <paramref name="today"/> + 365; and the own name, at most 60 code units
    /// after trimming. On <see cref="GoalProblem.None"/> the outputs hold the target, the start date and the own
    /// name (null for no name); otherwise they are null or default.
    /// </summary>
    public static GoalProblem TryCreate(
        GoalTargetFields fields,
        DateOnly? startDate,
        DateOnly today,
        string? name,
        out GoalTarget? target,
        out DateOnly start,
        out GoalName? ownName)
    {
        ArgumentNullException.ThrowIfNull(fields);
        start = default;
        ownName = null;
        var problem = CheckTarget(fields, out target);
        if (problem != GoalProblem.None)
        {
            return problem;
        }

        var dateProblem = CheckStartDate(startDate, today);
        if (dateProblem != GoalProblem.None)
        {
            target = null;
            return dateProblem;
        }

        if (!GoalName.TryCreate(name, out var checkedName, out _))
        {
            target = null;
            return GoalProblem.NameTooLong;
        }

        start = startDate!.Value;
        ownName = checkedName;
        return GoalProblem.None;
    }

    /// <summary>
    /// The target steps of <see cref="TryCreate"/> alone, without the start date or the name (slice 8 FR-003 step 5):
    /// the first problem of slice 2's target checks with the goal's yes/no problem, in their order, then
    /// <see cref="GoalProblem.ReduceAmountNeedsOneTime"/>; otherwise <see cref="GoalProblem.None"/> with the target in
    /// <paramref name="target"/>. Values that do not belong to the chosen measurement or schedule kind are ignored.
    /// <see cref="TryCreate"/> calls it, so the order exists once.
    /// </summary>
    public static GoalProblem CheckTarget(GoalTargetFields fields, out GoalTarget? target)
    {
        ArgumentNullException.ThrowIfNull(fields);
        GoalTarget.TryCreate(fields, out target, out var problem);
        return problem;
    }

    /// <summary>The field a refusal is about (FR-006). <see cref="GoalProblem.None"/> has no field.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="problem"/> is <see cref="GoalProblem.None"/> or undefined.</exception>
    public static GoalField FieldOf(GoalProblem problem) => problem switch
    {
        GoalProblem.HabitMissing => GoalField.Habit,
        GoalProblem.BuildOrReduceMissing => GoalField.BuildOrReduce,
        GoalProblem.MeasurementMissing => GoalField.Measurement,
        GoalProblem.YesNoHasAmountOrUnit => GoalField.Measurement,
        GoalProblem.BuildAmountOutOfRange => GoalField.Amount,
        GoalProblem.ReduceAmountOutOfRange => GoalField.Amount,
        GoalProblem.CountUnitMissing => GoalField.Unit,
        GoalProblem.CountUnitTooLong => GoalField.Unit,
        GoalProblem.DurationUnitMissing => GoalField.Unit,
        GoalProblem.ScheduleMissing => GoalField.Schedule,
        GoalProblem.DaysMissing => GoalField.Days,
        GoalProblem.TimesOutOfRange => GoalField.Times,
        GoalProblem.PeriodMissing => GoalField.Period,
        GoalProblem.ReduceAmountNeedsOneTime => GoalField.Times,
        GoalProblem.StartDateMissing => GoalField.StartDate,
        GoalProblem.StartDateInPast => GoalField.StartDate,
        GoalProblem.StartDateNotAfterEnding => GoalField.StartDate,
        GoalProblem.StartDateTooFar => GoalField.StartDate,
        GoalProblem.NameTooLong => GoalField.Name,
        _ => throw new ArgumentOutOfRangeException(nameof(problem), problem, "Only a refusal has a field."),
    };

    private static GoalProblem CheckStartDate(DateOnly? startDate, DateOnly today)
    {
        if (startDate is not { } date)
        {
            return GoalProblem.StartDateMissing;
        }

        if (date < today)
        {
            return GoalProblem.StartDateInPast;
        }

        return date > today.AddDays(MaxDaysAhead) ? GoalProblem.StartDateTooFar : GoalProblem.None;
    }
}
