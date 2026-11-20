using Bunit;
using HT.Application.Reports;
using HT.Domain.Goals;
using HT.Domain.Reports;
using HT.Ui.Components.Shared;

namespace HT.UiTests.Components.Shared;

/// <summary>GoalSummary (HP-003, design/components/GoalSummary "Slice 6 state"): the Home's "Active goals" card.</summary>
public sealed class GoalSummaryTests : BunitContext
{
    private static DashboardGoal GoalOf(int n, StreakUnit unit = StreakUnit.Commitments, int successful = 18, int counted = 20, int current = 6, int longest = 11) => new()
    {
        GoalId = new GoalId(Guid.Parse($"5a1d0c9e-0000-4000-8000-{n:D12}")),
        Name = $"Goal {n}",
        Stats = new GoalStatsView
        {
            Consistency = new Consistency(successful, counted),
            ConsistencyText = ReportText.Consistency(new Consistency(successful, counted)),
            Unit = unit,
            CurrentStreak = current,
            CurrentStreakText = ReportText.Streak(current, unit),
            LongestStreak = longest,
            LongestStreakText = ReportText.Streak(longest, unit),
        },
    };

    private IRenderedComponent<GoalSummary> RenderCard(params DashboardGoal[] goals) =>
        Render<GoalSummary>(p => p.Add(x => x.Goals, goals));

    [Fact]
    public void Three_goals_each_have_their_link_their_consistency_and_their_streak_line()
    {
        var cut = RenderCard(GoalOf(1), GoalOf(2, StreakUnit.Weeks, 8, 11, 0, 3), GoalOf(3, StreakUnit.Days, 4, 5, 1, 4));

        Assert.Equal("Active goals", cut.Find("section.ht-card > h2.ht-card__title").TextContent);
        var rows = cut.FindAll(".ht-goal");
        Assert.Equal(
            [
                ("Goal 1", "/goals/5a1d0c9e-0000-4000-8000-000000000001", "90% (18 of 20)", "Streak 6 commitments · longest 11"),
                ("Goal 2", "/goals/5a1d0c9e-0000-4000-8000-000000000002", "72% (8 of 11)", "Streak 0 weeks · longest 3"),
                ("Goal 3", "/goals/5a1d0c9e-0000-4000-8000-000000000003", "80% (4 of 5)", "Streak 1 day · longest 4"),
            ],
            rows.Select(r => (
                r.QuerySelector(".ht-goal__top a.ht-goal__name")!.TextContent,
                r.QuerySelector("a.ht-goal__name")!.GetAttribute("href"),
                r.QuerySelector(".ht-goal__top .ht-goal__pct")!.TextContent,
                r.QuerySelector(".ht-goal__meta")!.TextContent)));
        Assert.Empty(cut.FindAll(".ht-goal__pct small"));
        var all = cut.Find("a.ht-card__link");
        Assert.Equal(("All goals →", "/goals"), (all.TextContent, all.GetAttribute("href")));
    }

    [Fact]
    public void Seven_goals_show_the_first_5_then_All_goals()
    {
        var cut = RenderCard(Enumerable.Range(1, 7).Select(n => GoalOf(n)).ToArray());

        Assert.Equal(["Goal 1", "Goal 2", "Goal 3", "Goal 4", "Goal 5"], cut.FindAll("a.ht-goal__name").Select(a => a.TextContent));
        Assert.Equal("All goals →", cut.Find("a.ht-card__link").TextContent);
    }

    [Fact]
    public void A_goal_with_no_final_result_shows_NA_and_No_finalized_unit_yet()
    {
        var cut = RenderCard(GoalOf(1, StreakUnit.Weeks, 0, 0, 0, 0));

        Assert.Equal(("N/A", "No finalized weeks yet"), (cut.Find(".ht-goal__pct").TextContent, cut.Find(".ht-goal__meta").TextContent));
    }
}
