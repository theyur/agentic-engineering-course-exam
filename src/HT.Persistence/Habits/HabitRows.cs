using HT.Domain.Habits;

namespace HT.Persistence.Habits;

/// <summary>A row of <c>CatalogHabits</c> (data-model.md). Only the stores map it to and from domain types.</summary>
internal sealed class CatalogHabitRow
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary><see cref="HabitName.Key"/>, under a binary collation and a unique index.</summary>
    public string NameKey { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Category Category { get; set; }

    public Intent Intent { get; set; }

    public string Icon { get; set; } = string.Empty;

    public CatalogHabitState State { get; set; }

    /// <summary>The owned list, in <see cref="SuggestedStartingGoalRow.Position"/> order once sorted.</summary>
    public List<SuggestedStartingGoalRow> SuggestedStartingGoals { get; set; } = [];
}

/// <summary>A row of <c>CatalogHabitSuggestedStartingGoals</c>, owned by its catalog habit; key (habit, position).</summary>
internal sealed class SuggestedStartingGoalRow
{
    /// <summary>1 to 5, in list order.</summary>
    public int Position { get; set; }

    public bool IsMarked { get; set; }

    public BuildOrReduce BuildOrReduce { get; set; }

    public Measurement Measurement { get; set; }

    public int? Amount { get; set; }

    public string? CountUnit { get; set; }

    public DurationUnit? DurationUnit { get; set; }

    public ScheduleKind ScheduleKind { get; set; }

    /// <summary>A bit mask, Monday 1 … Sunday 64; only for fixed days.</summary>
    public int? Days { get; set; }

    public int? Times { get; set; }

    public SchedulePeriod? Period { get; set; }
}

/// <summary>A row of <c>CustomHabits</c>. Every statement on it is scoped by <see cref="OwnerId"/> (research R-08, R-14).</summary>
internal sealed class CustomHabitRow
{
    public Guid Id { get; set; }

    public Guid OwnerId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string NameKey { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Category Category { get; set; }

    public Intent Intent { get; set; }

    public string Icon { get; set; } = string.Empty;
}

/// <summary>
/// What demo loading reads of a named catalog habit (slice 4 research R-10): its identifier and its stored state, as an
/// unmapped result of a raw locking query, so it never tracks or writes the row. Public catalog data only.
/// </summary>
internal sealed class CatalogHabitStateRow
{
    public Guid Id { get; set; }

    public string State { get; set; } = string.Empty;
}
