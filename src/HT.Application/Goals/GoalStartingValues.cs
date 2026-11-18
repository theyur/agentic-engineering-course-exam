using HT.Domain.Habits;

namespace HT.Application.Goals;

/// <summary>
/// The values a new goal starts with (FR-007): those of the chosen suggested starting goal, or none; the start date is
/// today in the acting user's time zone. With no habit, only <see cref="StartDate"/> is set (US8-AS4).
/// </summary>
public sealed record GoalStartingValues
{
    /// <summary>The chosen habit; null when none is chosen.</summary>
    public GoalHabitView? Habit { get; init; }

    public BuildOrReduce? BuildOrReduce { get; init; }

    public Measurement? Measurement { get; init; }

    public int? Amount { get; init; }

    public string? CountUnit { get; init; }

    public DurationUnit? DurationUnit { get; init; }

    public ScheduleKind? ScheduleKind { get; init; }

    /// <summary>The scheduled days; empty when the schedule has none.</summary>
    public IReadOnlySet<DayOfWeek> Days { get; init; } = new HashSet<DayOfWeek>();

    public int? Times { get; init; }

    public SchedulePeriod? Period { get; init; }

    /// <summary>Today in the acting user's time zone (FR-012).</summary>
    public required DateOnly StartDate { get; init; }

    /// <summary>Always null: a new goal starts without an own name.</summary>
    public string? Name { get; init; }

    /// <summary>
    /// The suggestion's values as the goal rules write them (<c>GoalText.TargetText</c>); null when no suggested
    /// starting goal was used.
    /// </summary>
    public string? CatalogSuggestionText { get; init; }
}
