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
