using HT.Application.Habits;
using HT.Domain.Habits;

namespace HT.Ui.Habits;

/// <summary>The values of the habit fields while a form is edited. It holds what the user chose, nothing is checked here.</summary>
public sealed class HabitFormModel
{
    public string? Name { get; set; }

    public string? Description { get; set; }

    public Category? Category { get; set; }

    public Intent? Intent { get; set; }

    public string? Icon { get; set; }

    public HabitInput ToHabitInput() => new() { Name = Name, Description = Description, Category = Category, Intent = Intent, Icon = Icon };

    public static HabitFormModel From(string name, string? description, Category category, Intent intent, string icon) =>
        new() { Name = name, Description = description, Category = category, Intent = intent, Icon = icon };
}

/// <summary>
/// One suggested starting goal while the administrator's form is edited. Only the values of the chosen measurement
/// and schedule kind are sent, as only their controls are shown (plan, "Spec discrepancies" 1).
/// </summary>
public sealed class SuggestedStartingGoalFormModel
{
    public BuildOrReduce? BuildOrReduce { get; set; }

    public Measurement? Measurement { get; set; }

    public int? Amount { get; set; }

    public string? CountUnit { get; set; }

    public DurationUnit? DurationUnit { get; set; }

    public ScheduleKind? ScheduleKind { get; set; }

    public IReadOnlySet<DayOfWeek> Days { get; set; } = new HashSet<DayOfWeek>();

    public int? Times { get; set; }

    public SchedulePeriod? Period { get; set; }

    public bool IsMarked { get; set; }

    public SuggestedStartingGoalInput ToInput() => new()
    {
        BuildOrReduce = BuildOrReduce,
        Measurement = Measurement,
        Amount = Measurement is HT.Domain.Habits.Measurement.Count or HT.Domain.Habits.Measurement.Duration ? Amount : null,
        CountUnit = Measurement == HT.Domain.Habits.Measurement.Count ? CountUnit : null,
        DurationUnit = Measurement == HT.Domain.Habits.Measurement.Duration ? DurationUnit : null,
        ScheduleKind = ScheduleKind,
        Days = ScheduleKind == HT.Domain.Habits.ScheduleKind.FixedDays ? Days : new HashSet<DayOfWeek>(),
        Times = ScheduleKind == HT.Domain.Habits.ScheduleKind.Times ? Times : null,
        Period = ScheduleKind == HT.Domain.Habits.ScheduleKind.Times ? Period : null,
        IsMarked = IsMarked,
    };

    public static SuggestedStartingGoalFormModel From(SuggestedStartingGoalView view)
    {
        ArgumentNullException.ThrowIfNull(view);
        return new()
        {
            BuildOrReduce = view.BuildOrReduce,
            Measurement = view.Measurement,
            Amount = view.Amount,
            CountUnit = view.CountUnit,
            DurationUnit = view.DurationUnit,
            ScheduleKind = view.ScheduleKind,
            Days = view.Days.ToHashSet(),
            Times = view.Times,
            Period = view.Period,
            IsMarked = view.IsMarked,
        };
    }
}

/// <summary>Which field of a habit form a refusal belongs under, by its outcome's name (HP-007, HP-009).</summary>
public static class HabitFormErrors
{
    public const string Name = "name";
    public const string Description = "description";
    public const string Category = "category";
    public const string Intent = "intent";
    public const string Icon = "icon";
    public const string List = "list";
    public const string BuildOrReduce = "build-or-reduce";
    public const string Measurement = "measurement";
    public const string Amount = "amount";
    public const string Schedule = "schedule";
    public const string Days = "days";
    public const string Times = "times";

    /// <summary>The field key of an outcome; null for an outcome that belongs to no field.</summary>
    public static string? FieldOf(string outcome) => outcome switch
    {
        "NameMissing" or "NameTooLong" or "NameTaken" => Name,
        "DescriptionTooLong" => Description,
        "CategoryMissing" => Category,
        "IntentMissing" => Intent,
        "IconMissing" => Icon,
        "TooManySuggestedStartingGoals" or "SuggestedStartingGoalNotMarked" => List,
        "BuildOrReduceMissing" => BuildOrReduce,
        "MeasurementMissing" or "YesNoHasAmountOrUnit" => Measurement,
        "BuildAmountOutOfRange" or "ReduceAmountOutOfRange" or "CountUnitMissing" or "CountUnitTooLong" or "DurationUnitMissing" => Amount,
        "ScheduleMissing" => Schedule,
        "DaysMissing" => Days,
        "TimesOutOfRange" or "PeriodMissing" => Times,
        _ => null,
    };
}

/// <summary>A refusal shown in a form: the field it belongs under, its text, and the suggested starting goal's position if any.</summary>
public sealed record FormError(string Field, string Message, int? Position)
{
    /// <summary>The error of a refusal, or null when it belongs to no field.</summary>
    public static FormError? Of(string outcome, string? message, int? position = null) =>
        message is not null && HabitFormErrors.FieldOf(outcome) is { } field ? new FormError(field, message, position) : null;

    /// <summary>The text when this error is about <paramref name="field"/> (and, for a suggested starting goal, its position).</summary>
    public string? For(string field, int? position = null) =>
        string.Equals(Field, field, StringComparison.Ordinal) && Position == position ? Message : null;
}
