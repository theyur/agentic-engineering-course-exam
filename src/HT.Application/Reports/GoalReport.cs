using HT.Domain.Goals;
using HT.Domain.Habits;

namespace HT.Application.Reports;

/// <summary>
/// The report of one of the acting user's goals at the supplied time (FR-007): its figures, its latest 30 ended
/// results and its trend. Private to the goal's owner (FR-014).
/// </summary>
public sealed record GoalReport
{
    public required GoalId GoalId { get; init; }

    /// <summary>The own name, or the derived name (slice 4 FR-010).</summary>
    public required string Name { get; init; }

    public required BuildOrReduce BuildOrReduce { get; init; }

    public required GoalScheduleKind ScheduleKind { get; init; }

    /// <summary>The length of an evaluation period (day or week); null for a commitment-based goal.</summary>
    public SchedulePeriod? Period { get; init; }

    public required GoalStatsView Stats { get; init; }

    /// <summary>The latest 30 ended results, oldest first (FR-005).</summary>
    public required RecentResults Recent { get; init; }

    public required TrendView Trend { get; init; }

    /// <summary>
    /// False when the goal has no current date, or none of its commitments or periods has begun yet (a per-week goal
    /// before its first week); the goal page then shows neither the GoalStats nor the TrendChart card (HP-001).
    /// </summary>
    public required bool HasBegunEntry { get; init; }

    /// <summary>The instant the use case read the clock.</summary>
    public required DateTimeOffset ReadAt { get; init; }
}
