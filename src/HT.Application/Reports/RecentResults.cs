using HT.Domain.Evaluation;

namespace HT.Application.Reports;

/// <summary>
/// One ended result of a goal (FR-005): its first and last date (equal except for a week, its Monday and its Sunday),
/// its state (Met, Failed, Missed or Pending; slice 7 FR-015: or Excused or Paused for an exempt result) and its progress
/// (slice 5 FR-014).
/// </summary>
public sealed record ResultView
{
    public required DateOnly FirstDate { get; init; }

    public required DateOnly LastDate { get; init; }

    public required EvaluationState State { get; init; }

    public required Progress Progress { get; init; }
}

/// <summary>
/// A goal's latest ended results (FR-005): at most 30 on the goal page, 15 in the goals overview, oldest first, with
/// the number of each state among them.
/// </summary>
public sealed record RecentResults
{
    /// <summary>Oldest first.</summary>
    public required IReadOnlyList<ResultView> Results { get; init; }

    /// <summary>
    /// The number of each ended state (Met, Failed, Missed, Pending) among <see cref="Results"/>, 0 included; slice 7
    /// (FR-015): also Excused and Paused, each only when its number is above 0.
    /// </summary>
    public required IReadOnlyDictionary<EvaluationState, int> Counts { get; init; }
}
