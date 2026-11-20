using HT.Application.Reports;
using HT.Domain.Evaluation;
using HT.Domain.Reports;

namespace HT.UnitTests.Application.Reports;

/// <summary>HP-004, D-3: the visitor Home's example card, with fixed figures computed from its fixed strip by the shared rules.</summary>
public sealed class ExampleGoalTests
{
    private const EvaluationState Met = EvaluationState.Met;

    [Fact]
    public void The_title_is_Yoga_3_days_a_week()
    {
        Assert.Equal("Yoga, 3 days a week", ExampleGoal.Title);
    }

    [Fact]
    public void The_strip_is_HP_004s_21_results_oldest_first()
    {
        Assert.Equal(
            [
                Met, Met, Met, Met, Met, Met, Met, Met, Met, Met, Met,
                EvaluationState.Missed, Met, EvaluationState.Failed,
                Met, Met, Met, Met, Met, Met,
                EvaluationState.Pending,
            ],
            ExampleGoal.Strip);
    }

    [Fact]
    public void The_figures_are_90_percent_18_of_20_6_and_11_commitments()
    {
        var stats = ExampleGoal.Stats;

        Assert.Equal("90% (18 of 20)", stats.ConsistencyText);
        Assert.Equal(StreakUnit.Commitments, stats.Unit);
        Assert.Equal((6, 11), (stats.CurrentStreak, stats.LongestStreak));
        Assert.Equal(("6 commitments", "11 commitments"), (stats.CurrentStreakText, stats.LongestStreakText));
    }

    [Fact]
    public void The_figures_equal_the_shared_rules_over_the_strip()
    {
        var consistency = Consistency.Of(ExampleGoal.Strip);
        var streaks = Streaks.Of(ExampleGoal.Strip);

        Assert.Equal(consistency, ExampleGoal.Stats.Consistency);
        Assert.Equal(ReportText.Consistency(consistency), ExampleGoal.Stats.ConsistencyText);
        Assert.Equal((streaks.Current, streaks.Longest), (ExampleGoal.Stats.CurrentStreak, ExampleGoal.Stats.LongestStreak));
    }
}
