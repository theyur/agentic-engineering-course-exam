using HT.Domain.Goals;

namespace HT.Application.Reports;

/// <summary>
/// One entry of the goal history (slice 8 FR-015, data-model.md "AttemptService views"): an ended attempt of one of the
/// acting user's goals. Not a goal's History (slice 5 FR-009), which lists one goal's dates. Private to the goal's owner
/// (FR-022).
/// </summary>
public sealed record EndedAttemptView
{
    public required GoalId GoalId { get; init; }

    /// <summary>
    /// The attempt's name (FR-004): the goal's name at the supplied time for the goal's latest attempt, and for an earlier
    /// one its name at its last date (the goal's own name, or the habit's current name and the target text of its last
    /// version).
    /// </summary>
    public required string Name { get; init; }

    public required int AttemptNumber { get; init; }

    /// <summary>The ending date E (FR-007).</summary>
    public required DateOnly EndingDate { get; init; }

    /// <summary>How the attempt ended.</summary>
    public required Outcome Outcome { get; init; }

    /// <summary>The trimmed ending note; null when there is none. Private (HP-010).</summary>
    public string? EndingNote { get; init; }

    /// <summary>Whether this is the goal's latest attempt, so the goal itself is ended.</summary>
    public required bool IsLatestAttempt { get; init; }
}
