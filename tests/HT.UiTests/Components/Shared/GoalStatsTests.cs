using Bunit;
using HT.Application.Reports;
using HT.Domain.Evaluation;
using HT.Domain.Reports;
using HT.Ui.Components.Shared;

namespace HT.UiTests.Components.Shared;

/// <summary>
/// GoalStats (HP-001, HP-004, design/components/GoalStats slice-6 state): the eyebrow and the title, the three stats
/// with the design's classes, the consistency as one whole text ("90% (18 of 20)" or "N/A"), the streak labels with the
/// plural unit, then the ConsistencyStrip of the given states in order with its legend.
/// </summary>
public sealed class GoalStatsTests : BunitContext
{
    private static GoalStatsView Stats(Consistency consistency, int current, int longest, StreakUnit unit) => new()
    {
        Consistency = consistency,
        ConsistencyText = ReportText.Consistency(consistency),
        Unit = unit,
        CurrentStreak = current,
        CurrentStreakText = ReportText.Streak(current, unit),
        LongestStreak = longest,
        LongestStreakText = ReportText.Streak(longest, unit),
    };

    private IRenderedComponent<GoalStats> RenderStats(GoalStatsView view, IReadOnlyList<EvaluationState> strip, string eyebrow = "This attempt", string title = "Consistency and streaks") =>
        Render<GoalStats>(p => p
            .Add(x => x.View, view)
            .Add(x => x.Strip, strip)
            .Add(x => x.Unit, view.Unit)
            .Add(x => x.Eyebrow, eyebrow)
            .Add(x => x.Title, title));

    [Fact]
    public void HP_001_the_card_has_the_eyebrow_the_title_and_the_three_stats_with_their_whole_values()
    {
        var cut = RenderStats(Stats(new Consistency(18, 20), 6, 11, StreakUnit.Commitments), [EvaluationState.Met]);

        Assert.Equal("This attempt", cut.Find("section.ht-card > .ht-eyebrow").TextContent);
        Assert.Equal("Consistency and streaks", cut.Find("section.ht-card > h2.ht-card__title").TextContent);
        var stats = cut.FindAll(".ht-stats > .ht-stat");
        Assert.Equal(3, stats.Count);
        Assert.Equal(["Consistency", "Streak, commitments", "Longest, commitments"], stats.Select(s => s.QuerySelector(".ht-stat__label")!.TextContent));
        Assert.Equal(["90% (18 of 20)", "6", "11"], stats.Select(s => s.QuerySelector(".ht-stat__value")!.TextContent));
        Assert.Contains("ht-stat__value--text", stats[0].QuerySelector(".ht-stat__value")!.ClassList);
        Assert.Empty(cut.FindAll(".ht-stat__value small"));
    }

    [Fact]
    public void FR_002_with_no_counted_result_the_consistency_reads_N_A_and_both_streaks_0()
    {
        var cut = RenderStats(Stats(new Consistency(0, 0), 0, 0, StreakUnit.Days), [EvaluationState.Pending, EvaluationState.Pending]);

        Assert.Equal(["N/A", "0", "0"], cut.FindAll(".ht-stat__value").Select(v => v.TextContent));
        Assert.Equal(["Consistency", "Streak, days", "Longest, days"], cut.FindAll(".ht-stat__label").Select(l => l.TextContent));
    }

    [Fact]
    public void HP_001_the_strip_follows_the_stats_with_its_cells_in_order_its_label_and_its_legend()
    {
        EvaluationState[] strip = [EvaluationState.Met, EvaluationState.Failed, EvaluationState.Met, EvaluationState.Missed, EvaluationState.Pending];

        var cut = RenderStats(Stats(new Consistency(2, 4), 0, 1, StreakUnit.Weeks), strip);

        var cells = cut.FindAll("section.ht-card > .ht-stats ~ div.ht-strip > i");
        Assert.Equal(
            ["ht-cell--met", "ht-cell--failed", "ht-cell--met", "ht-cell--missed", "ht-cell--pending"],
            cells.Select(c => c.ClassList.Single(name => name.StartsWith("ht-cell--", StringComparison.Ordinal))));
        Assert.Equal("Last 5 weeks: 2 met, 1 failed, 1 missed, 1 pending", cut.Find("div.ht-strip").GetAttribute("aria-label"));
        Assert.Equal(["Met", "Failed", "Missed", "Pending"], cut.FindAll("section.ht-card > .ht-legend > span").Select(s => s.TextContent));
        Assert.Empty(cut.FindAll(".ht-strip--mini"));
    }

    [Fact]
    public void HP_004_the_visitor_variant_has_its_own_eyebrow_and_title()
    {
        var cut = RenderStats(Stats(new Consistency(18, 20), 6, 11, StreakUnit.Commitments), [EvaluationState.Met], "Example goal", "Yoga, 3 days a week");

        Assert.Equal("Example goal", cut.Find(".ht-eyebrow").TextContent);
        Assert.Equal("Yoga, 3 days a week", cut.Find("h2.ht-card__title").TextContent);
    }
}
