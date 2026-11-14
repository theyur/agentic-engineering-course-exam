namespace HT.Domain.Habits;

/// <summary>
/// The unchecked values of a suggested starting goal: one nullable value per control of the form (research R-05),
/// so every "no …" case of FR-007 can be expressed.
/// </summary>
public sealed record SuggestedStartingGoalFields
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
