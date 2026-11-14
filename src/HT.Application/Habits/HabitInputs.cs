using HT.Domain.Habits;

namespace HT.Application.Habits;

/// <summary>The fields of a habit as a form sends them; every value may be missing (data-model.md, "Application inputs").</summary>
public sealed record HabitInput
{
    public string? Name { get; init; }

    public string? Description { get; init; }

    public Category? Category { get; init; }

    public Intent? Intent { get; init; }

    /// <summary>The icon's Lucide name.</summary>
    public string? Icon { get; init; }
}

/// <summary>The fields of a catalog habit: those of <see cref="HabitInput"/> and its suggested starting goals, in order.</summary>
public sealed record CatalogHabitInput
{
    public string? Name { get; init; }

    public string? Description { get; init; }

    public Category? Category { get; init; }

    public Intent? Intent { get; init; }

    public string? Icon { get; init; }

    /// <summary>The suggested starting goals in the administrator's order; may be empty.</summary>
    public IReadOnlyList<SuggestedStartingGoalInput> SuggestedStartingGoals { get; init; } = [];
}

/// <summary>One suggested starting goal as the form sends it: one nullable value per control, and its mark (research R-05).</summary>
public sealed record SuggestedStartingGoalInput
{
    public BuildOrReduce? BuildOrReduce { get; init; }

    public Measurement? Measurement { get; init; }

    public int? Amount { get; init; }

    public string? CountUnit { get; init; }

    public DurationUnit? DurationUnit { get; init; }

    public ScheduleKind? ScheduleKind { get; init; }

    /// <summary>The chosen days; may be empty.</summary>
    public IReadOnlySet<DayOfWeek> Days { get; init; } = new HashSet<DayOfWeek>();

    public int? Times { get; init; }

    public SchedulePeriod? Period { get; init; }

    /// <summary>True for the one suggested starting goal the catalog card shows.</summary>
    public bool IsMarked { get; init; }
}

/// <summary>The filters of browsing (FR-009); a null value applies no filter.</summary>
public sealed record CatalogFilter
{
    /// <summary>One category; null is all.</summary>
    public Category? Category { get; init; }

    /// <summary>One intent; null is both.</summary>
    public Intent? Intent { get; init; }

    /// <summary>The search text; null or blank matches every name.</summary>
    public string? Search { get; init; }
}
