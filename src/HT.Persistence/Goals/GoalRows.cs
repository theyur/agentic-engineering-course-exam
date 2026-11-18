using HT.Domain.Habits;
using HT.Persistence.Habits;

namespace HT.Persistence.Goals;

/// <summary>
/// A row of <c>Goals</c> (data-model.md "Tables"). Exactly one of <see cref="CatalogHabitId"/> and
/// <see cref="CustomHabitId"/> is set; the mapping checks it, not a check constraint. Every statement on it is scoped
/// by <see cref="OwnerId"/> (constitution III). Only the goal store maps it to and from domain types.
/// </summary>
internal sealed class GoalRow
{
    public Guid Id { get; set; }

    public Guid OwnerId { get; set; }

    public Guid? CatalogHabitId { get; set; }

    public Guid? CustomHabitId { get; set; }

    public DateOnly StartDate { get; set; }

    /// <summary>The goal's own name; null when it has none.</summary>
    public string? Name { get; set; }

    /// <summary>An identity column: the creation order of the owner's goals (FR-015, research R-08).</summary>
    public long CreationSequence { get; set; }

    public CatalogHabitRow? CatalogHabit { get; set; }

    public CustomHabitRow? CustomHabit { get; set; }

    public List<GoalSettingsVersionRow> Versions { get; set; } = [];

    public List<GoalAttemptRow> Attempts { get; set; } = [];
}

/// <summary>A row of <c>GoalSettingsVersions</c>; key (goal, applies-from). The target's values and the version's zone.</summary>
internal sealed class GoalSettingsVersionRow
{
    public Guid GoalId { get; set; }

    public DateOnly AppliesFrom { get; set; }

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

    /// <summary>The version's named time zone identifier, as given (FR-011).</summary>
    public string TimeZone { get; set; } = string.Empty;
}

/// <summary>A row of <c>GoalAttempts</c>; key (goal, number).</summary>
internal sealed class GoalAttemptRow
{
    public Guid GoalId { get; set; }

    public int Number { get; set; }

    public DateOnly StartDate { get; set; }
}
