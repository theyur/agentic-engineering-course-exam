using HT.Application.Goals;
using HT.Application.Records;

namespace HT.Application.Reports;

/// <summary>
/// One goal of the goals overview (FR-011, research R-07): slice 4's list item, its figures, its last 15 ended results,
/// and what can be recorded now.
/// </summary>
public sealed record OverviewGoal
{
    /// <summary>Equal, field by field, to the item <see cref="GoalService.ListAsync"/> gives for the goal at the same instant.</summary>
    public required GoalListItem Item { get; init; }

    public required GoalStatsView Stats { get; init; }

    /// <summary>The last 15 ended results, oldest first (FR-005).</summary>
    public required RecentResults Recent { get; init; }

    /// <summary>The goal has a Pending commitment or period.</summary>
    public required bool HasPending { get; init; }

    /// <summary>The goal has at least one Due or Pending commitment or period, so "Record" is shown.</summary>
    public required bool CanRecord { get; init; }

    /// <summary>
    /// Equal to the <see cref="GoalHistory"/> that <see cref="RecordingService.GetRecordableAsync"/> gives for the goal
    /// at the same instant: its settings and its Due and Pending entries, newest first.
    /// </summary>
    public required GoalHistory Recordable { get; init; }

    /// <summary>
    /// Slice 7 (FR-018, research R-03): the last day of the goal's current pause at the pause date (the goal's current
    /// date, or the account's today for a goal that has not started); null when the goal is not paused.
    /// </summary>
    public DateOnly? PausedUntil { get; init; }
}

/// <summary>Every goal of the acting user with its figures, in FR-011's order (Pending, then Due, then the others).</summary>
public sealed record GoalsOverview
{
    public required IReadOnlyList<OverviewGoal> Goals { get; init; }

    /// <summary>Today in the account's time zone at the supplied time.</summary>
    public required DateOnly Today { get; init; }

    /// <summary>The instant the use case read the clock.</summary>
    public required DateTimeOffset ReadAt { get; init; }
}
