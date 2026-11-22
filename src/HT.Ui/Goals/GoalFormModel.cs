using HT.Application.Goals;
using HT.Domain.Goals;
using HT.Domain.Habits;

namespace HT.Ui.Goals;

/// <summary>
/// The New goal form's values while it is edited (HP-001). It holds what the user chose; nothing is checked here. Only
/// the values of the chosen measurement and schedule are sent, as only their controls are shown (as the suggested
/// starting goal editor does).
/// </summary>
public sealed class GoalFormModel
{
    /// <summary>The chosen habit; null until one is chosen.</summary>
    public GoalHabitView? Habit { get; set; }

    public BuildOrReduce? BuildOrReduce { get; set; }

    public Measurement? Measurement { get; set; }

    public int? Amount { get; set; }

    public string? CountUnit { get; set; }

    public DurationUnit? DurationUnit { get; set; }

    public ScheduleKind? ScheduleKind { get; set; }

    public IReadOnlySet<DayOfWeek> Days { get; set; } = new HashSet<DayOfWeek>();

    public int? Times { get; set; }

    public SchedulePeriod? Period { get; set; }

    public DateOnly? StartDate { get; set; }

    public string? Name { get; set; }

    /// <summary>The starting values' catalog suggestion text; it stays while the form changes (HP-002).</summary>
    public string? CatalogSuggestionText { get; set; }

    /// <summary>True for count and duration, the measurements with a Target section.</summary>
    public bool HasAmount => Measurement is HT.Domain.Habits.Measurement.Count or HT.Domain.Habits.Measurement.Duration;

    /// <summary>The target fields of the shown controls, for the summary card and the use case.</summary>
    public GoalTargetFields ToTargetFields() => new()
    {
        BuildOrReduce = BuildOrReduce,
        Measurement = Measurement,
        Amount = HasAmount ? Amount : null,
        CountUnit = Measurement == HT.Domain.Habits.Measurement.Count ? CountUnit : null,
        DurationUnit = Measurement == HT.Domain.Habits.Measurement.Duration ? DurationUnit : null,
        ScheduleKind = ScheduleKind,
        Days = ScheduleKind == HT.Domain.Habits.ScheduleKind.FixedDays ? Days : new HashSet<DayOfWeek>(),
        Times = ScheduleKind == HT.Domain.Habits.ScheduleKind.Times ? Times : null,
        Period = ScheduleKind == HT.Domain.Habits.ScheduleKind.Times ? Period : null,
    };

    public GoalInput ToInput() => new()
    {
        Habit = Habit is { } habit ? new GoalHabit(habit.Id, habit.Kind) : null,
        Target = ToTargetFields(),
        StartDate = StartDate,
        Name = Name,
    };

    /// <summary>The Edit settings page's change (slice 8 HP-002): the target fields of the shown controls and the name.</summary>
    public GoalSettingsInput ToSettingsInput() => new()
    {
        Target = ToTargetFields(),
        Name = Name,
    };

    /// <summary>The Start new attempt page's attempt (slice 8 HP-004): the target fields of the shown controls and the start date.</summary>
    public NewAttemptInput ToAttemptInput() => new()
    {
        Target = ToTargetFields(),
        StartDate = StartDate,
    };

    /// <summary>
    /// The Start new attempt form of an ended goal (slice 8 HP-004, contracts/host.md "Start new attempt page"): its
    /// starting values (FR-011) and the goal's habit, kept for the summary card's sentence and never sent. There is no
    /// name: the goal keeps its own.
    /// </summary>
    public static GoalFormModel From(NewAttemptValues values, GoalHabitView habit)
    {
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(habit);
        return new()
        {
            Habit = habit,
            BuildOrReduce = values.BuildOrReduce,
            Measurement = values.Measurement,
            Amount = values.Amount,
            CountUnit = values.CountUnit,
            DurationUnit = values.DurationUnit,
            ScheduleKind = values.ScheduleKind,
            Days = values.Days.ToHashSet(),
            Times = values.Times,
            Period = values.Period,
            StartDate = values.StartDate,
        };
    }

    /// <summary>
    /// The Edit settings form of a goal (slice 8 HP-002, contracts/host.md "Edit settings page"): its latest settings
    /// (<see cref="GoalDetails.Later"/> when there is one, else the settings in effect) and its own name. The habit is
    /// kept for the summary card's sentence, and the start date is the date a change made now applies from
    /// (<see cref="GoalDetails.ChangesApplyFrom"/>), which the summary card's sentence ends with; neither is sent.
    /// </summary>
    public static GoalFormModel From(GoalDetails details)
    {
        ArgumentNullException.ThrowIfNull(details);
        var model = new GoalFormModel
        {
            Habit = details.Habit,
            StartDate = details.ChangesApplyFrom,
            Name = details.HasOwnName ? details.Name : null,
        };
        if (details.Later is { } later)
        {
            model.BuildOrReduce = later.BuildOrReduce;
            model.Measurement = later.Measurement;
            model.Amount = later.Amount;
            model.CountUnit = later.CountUnit;
            model.DurationUnit = later.DurationUnit;
            model.ScheduleKind = later.ScheduleKind;
            model.Days = later.Days.ToHashSet();
            model.Times = later.Times;
            model.Period = later.Period;
            return model;
        }

        model.BuildOrReduce = details.BuildOrReduce;
        model.Measurement = details.Measurement;
        model.Amount = details.Amount;
        model.CountUnit = details.CountUnit;
        model.DurationUnit = details.DurationUnit;
        model.ScheduleKind = details.ScheduleKind;
        model.Days = details.Days.ToHashSet();
        model.Times = details.Times;
        model.Period = details.Period;
        return model;
    }

    /// <summary>The form as <c>StartNewGoalAsync</c> starts it (FR-007).</summary>
    public static GoalFormModel From(GoalStartingValues values)
    {
        ArgumentNullException.ThrowIfNull(values);
        return new()
        {
            Habit = values.Habit,
            BuildOrReduce = values.BuildOrReduce,
            Measurement = values.Measurement,
            Amount = values.Amount,
            CountUnit = values.CountUnit,
            DurationUnit = values.DurationUnit,
            ScheduleKind = values.ScheduleKind,
            Days = values.Days.ToHashSet(),
            Times = values.Times,
            Period = values.Period,
            StartDate = values.StartDate,
            Name = values.Name,
            CatalogSuggestionText = values.CatalogSuggestionText,
        };
    }
}

/// <summary>The one refusal the New goal form shows, under the field it is about (FR-006).</summary>
public sealed record GoalFormError(GoalField Field, string Message)
{
    /// <summary>The text when this error is about <paramref name="field"/>; null otherwise.</summary>
    public string? For(GoalField field) => Field == field ? Message : null;
}
