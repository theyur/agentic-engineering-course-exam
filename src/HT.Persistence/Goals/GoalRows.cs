using HT.Domain.Goals;
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

/// <summary>
/// A row of <c>GoalSettingsVersions</c>; key (goal, attempt number, applies-from) since slice 8 (research R-05), with a
/// foreign key (goal, attempt number) to <c>GoalAttempts</c>. The target's values and the version's zone.
/// </summary>
internal sealed class GoalSettingsVersionRow
{
    public Guid GoalId { get; set; }

    /// <summary>The number of the attempt the version belongs to (slice 8); 1 for every version written before slice 8.</summary>
    public int AttemptNumber { get; set; }

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

/// <summary>
/// A row of <c>GoalAttempts</c>; key (goal, number). Slice 8 (data-model.md "Tables"): the ending's columns, all null
/// while the attempt is not ended; <see cref="EndingDate"/> and <see cref="Outcome"/> are both set or both null, and
/// <see cref="EndingNote"/> is null without them. The mapping checks it, not a check constraint. The last date is never
/// stored (FR-026).
/// </summary>
internal sealed class GoalAttemptRow
{
    public Guid GoalId { get; set; }

    public int Number { get; set; }

    public DateOnly StartDate { get; set; }

    /// <summary>The ending date E; null while the attempt is not ended.</summary>
    public DateOnly? EndingDate { get; set; }

    public Outcome? Outcome { get; set; }

    /// <summary>The trimmed ending note; null when there is none. Private (HP-010).</summary>
    public string? EndingNote { get; set; }
}

/// <summary>
/// What a time-zone change reads from the locked account row (research R-08): an unmapped result of a raw query, not
/// an entity, so it never tracks or writes the row.
/// </summary>
internal sealed class AccountRevisionRow
{
    public string? TimeZone { get; set; }

    public int GoalsRevision { get; set; }
}

/// <summary>
/// What a goal-change write reads of the locked goal's latest attempt (slice 8, research R-05): its number and its
/// ending date, null while it is not ended. A projection, never tracked.
/// </summary>
internal sealed class LatestAttemptRow
{
    public int Number { get; init; }

    public DateOnly? EndingDate { get; init; }
}
