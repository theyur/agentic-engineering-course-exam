using HT.Domain.Habits;

namespace HT.Domain.Goals;

/// <summary>
/// The unchecked target values of a goal: one nullable value per control of the New goal form (research R-05), so
/// every "no …" case of FR-006 can be expressed. A stored target is rebuilt from the same shape.
/// </summary>
public sealed record GoalTargetFields
{
    public BuildOrReduce? BuildOrReduce { get; init; }

    public Measurement? Measurement { get; init; }

    public int? Amount { get; init; }

    public string? CountUnit { get; init; }

    public DurationUnit? DurationUnit { get; init; }

    public ScheduleKind? ScheduleKind { get; init; }

    /// <summary>The chosen days; null or empty is no day.</summary>
    public IReadOnlySet<DayOfWeek>? Days { get; init; }

    public int? Times { get; init; }

    public SchedulePeriod? Period { get; init; }
}
