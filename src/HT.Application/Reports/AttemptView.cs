using HT.Domain.Goals;

namespace HT.Application.Reports;

/// <summary>
/// One attempt of a goal (slice 8 FR-014, data-model.md "AttemptService views"): its number, name and start date, its
/// ending fields once it is ended, and its figures over that attempt up to the supplied time. Private to the goal's owner
/// (FR-022).
/// </summary>
public sealed record AttemptView
{
    public required int Number { get; init; }

    /// <summary>
    /// The attempt's name (FR-004): the goal's name at the supplied time for the latest attempt, and for an earlier one its
    /// name at its last date (the goal's own name, or the habit's current name and the target text of its last version).
    /// </summary>
    public required string Name { get; init; }

    /// <summary>The attempt's own start date; attempt 1's is the goal's start date.</summary>
    public required DateOnly StartDate { get; init; }

    /// <summary>Whether this is the goal's latest attempt.</summary>
    public required bool IsLatest { get; init; }

    /// <summary>The ending date E (FR-007); null while the attempt is not ended.</summary>
    public DateOnly? EndingDate { get; init; }

    /// <summary>The last date that belongs to the attempt (FR-008), before its first entry when it has no date; null while it is not ended.</summary>
    public DateOnly? LastDate { get; init; }

    /// <summary>How the attempt ended; null while it is not ended.</summary>
    public Outcome? Outcome { get; init; }

    /// <summary>The trimmed ending note; null when there is none or the attempt is not ended. Private (HP-010).</summary>
    public string? EndingNote { get; init; }

    /// <summary>
    /// True when the attempt is ended with no commitment or period: its last date is before its first entry (HP-001's
    /// "ended before it began"). Its History is then empty and its consistency "N/A" (AC-102).
    /// </summary>
    public required bool HasNoDate { get; init; }

    /// <summary>The consistency and the current and longest streak with their unit, over this attempt up to the supplied time.</summary>
    public required GoalStatsView Stats { get; init; }
}
