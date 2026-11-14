using HT.Application.Habits;
using HT.Domain.Habits;

namespace HT.AcceptanceTests.Support.F002;

/// <summary>
/// A text a step gives: <see cref="Full"/> is the value sent to the use case, <see cref="Core"/> the same text
/// without the whitespace-only parts the step puts around it (for "the 60-character name" and
/// "the 500-character description" of an expression such as <c>`"   "` + ... + `"   "`</c>).
/// </summary>
public sealed record TextValue(string? Full, string? Core)
{
    public static TextValue Of(string? text) => new(text, text);
}

/// <summary>
/// The fields of a catalog or custom habit as a step gives them. Fields a step does not give are the
/// acceptance-criteria defaults: no description, the category Other, the intent Build, the icon
/// <c>book-open</c> and no suggested starting goal.
/// </summary>
public sealed class HabitSpec
{
    public string? Name { get; private set; }

    /// <summary>The name without the surrounding whitespace-only parts of its expression.</summary>
    public string? NameCore { get; private set; }

    public string? Description { get; private set; }

    /// <summary>The description without the surrounding whitespace-only parts of its expression.</summary>
    public string? DescriptionCore { get; private set; }

    public Category? Category { get; set; } = global::HT.Domain.Habits.Category.Other;

    public Intent? Intent { get; set; } = global::HT.Domain.Habits.Intent.Build;

    public string? Icon { get; set; } = F002Text.DefaultIcon;

    public List<GoalSpec> Goals { get; set; } = [];

    public void SetName(TextValue name)
    {
        Name = name.Full;
        NameCore = name.Core;
    }

    public void SetDescription(TextValue description)
    {
        Description = description.Full;
        DescriptionCore = description.Core;
    }

    public HabitSpec Clone()
    {
        var clone = new HabitSpec
        {
            Category = Category,
            Intent = Intent,
            Icon = Icon,
            Goals = Goals.Select(goal => goal.Clone()).ToList(),
        };
        clone.SetName(new TextValue(Name, NameCore));
        clone.SetDescription(new TextValue(Description, DescriptionCore));
        return clone;
    }

    public CatalogHabitInput ToCatalogInput() => new()
    {
        Name = Name,
        Description = Description,
        Category = Category,
        Intent = Intent,
        Icon = Icon,
        SuggestedStartingGoals = Goals.Select(goal => goal.ToInput()).ToList(),
    };

    public HabitInput ToCustomInput() => new()
    {
        Name = Name,
        Description = Description,
        Category = Category,
        Intent = Intent,
        Icon = Icon,
    };
}

/// <summary>The fields of one suggested starting goal as a step writes them (FR-007).</summary>
public sealed class GoalSpec
{
    public BuildOrReduce? BuildOrReduce { get; set; }

    public Measurement? Measurement { get; set; }

    public int? Amount { get; set; }

    public string? CountUnit { get; set; }

    public DurationUnit? DurationUnit { get; set; }

    public ScheduleKind? ScheduleKind { get; set; }

    public HashSet<DayOfWeek> Days { get; set; } = [];

    public int? Times { get; set; }

    public SchedulePeriod? Period { get; set; }

    public bool IsMarked { get; set; }

    public GoalSpec Clone() => new()
    {
        BuildOrReduce = BuildOrReduce,
        Measurement = Measurement,
        Amount = Amount,
        CountUnit = CountUnit,
        DurationUnit = DurationUnit,
        ScheduleKind = ScheduleKind,
        Days = [.. Days],
        Times = Times,
        Period = Period,
        IsMarked = IsMarked,
    };

    public SuggestedStartingGoalInput ToInput() => new()
    {
        BuildOrReduce = BuildOrReduce,
        Measurement = Measurement,
        Amount = Amount,
        CountUnit = CountUnit,
        DurationUnit = DurationUnit,
        ScheduleKind = ScheduleKind,
        Days = new HashSet<DayOfWeek>(Days),
        Times = Times,
        Period = Period,
        IsMarked = IsMarked,
    };
}
