using HT.Domain.Reports;

namespace HT.Application.Reports;

/// <summary>
/// The figures of one goal (FR-001 to FR-004): its consistency and its current and longest streak, each with the text
/// <see cref="ReportText"/> gives it, so no page builds one (HP-005).
/// </summary>
public sealed record GoalStatsView
{
    public required Consistency Consistency { get; init; }

    /// <summary><see cref="ReportText.Consistency"/>: "90% (18 of 20)", or "N/A" with no counted result.</summary>
    public required string ConsistencyText { get; init; }

    /// <summary>The unit of both streaks: commitments, days or weeks (FR-004).</summary>
    public required StreakUnit Unit { get; init; }

    public required int CurrentStreak { get; init; }

    /// <summary><see cref="ReportText.Streak"/>: "6 commitments", "1 day", "0 weeks".</summary>
    public required string CurrentStreakText { get; init; }

    public required int LongestStreak { get; init; }

    /// <summary><see cref="ReportText.Streak"/>: "11 commitments".</summary>
    public required string LongestStreakText { get; init; }
}
