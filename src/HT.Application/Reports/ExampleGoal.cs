using HT.Domain.Evaluation;
using HT.Domain.Reports;

namespace HT.Application.Reports;

/// <summary>
/// The visitor Home's example GoalStats card (HP-004, D-3): fixed example results that belong to no account. Its figures
/// and texts are computed once, by <see cref="Consistency.Of"/>, <see cref="Streaks.Of"/> and <see cref="ReportText"/>,
/// from the 21 fixed states, so the card cannot disagree with the rules of FR-001 to FR-005. It reads no store and no
/// clock (constitution III).
/// </summary>
public static class ExampleGoal
{
    private const StreakUnit Unit = StreakUnit.Commitments;

    /// <summary>The card's title: "Yoga, 3 days a week".</summary>
    public static string Title => "Yoga, 3 days a week";

    /// <summary>The strip's 21 results, oldest first: Met × 11, Missed, Met, Failed, Met × 6, Pending (HP-004).</summary>
    public static IReadOnlyList<EvaluationState> Strip { get; } =
    [
        .. Enumerable.Repeat(EvaluationState.Met, 11),
        EvaluationState.Missed,
        EvaluationState.Met,
        EvaluationState.Failed,
        .. Enumerable.Repeat(EvaluationState.Met, 6),
        EvaluationState.Pending,
    ];

    /// <summary>The figures of <see cref="Strip"/>: "90% (18 of 20)", current streak 6 and longest 11 commitments.</summary>
    public static GoalStatsView Stats { get; } = StatsOf(Strip);

    private static GoalStatsView StatsOf(IReadOnlyList<EvaluationState> strip)
    {
        var consistency = Consistency.Of(strip);
        var streaks = Streaks.Of(strip);
        return new GoalStatsView
        {
            Consistency = consistency,
            ConsistencyText = ReportText.Consistency(consistency),
            Unit = Unit,
            CurrentStreak = streaks.Current,
            CurrentStreakText = ReportText.Streak(streaks.Current, Unit),
            LongestStreak = streaks.Longest,
            LongestStreakText = ReportText.Streak(streaks.Longest, Unit),
        };
    }
}
