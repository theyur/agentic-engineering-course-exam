using HT.AcceptanceTests.Support.F008;
using HT.Application.Reports;

namespace HT.AcceptanceTests.StepDefinitions.F008;

/// <summary>
/// The Then steps of F008 on slice 6's reads as this slice extends them: consistency, streaks, recent results, the
/// trend (FR-013 to FR-016) and the dashboard (FR-019, FR-020).
/// </summary>
[Binding]
[Scope(Tag = "F008")]
public sealed class ReportThenSteps(F008Context context, F008Arrange arrange)
{
    // ------------------------------------------------------------ figures of the report the When step read

    [Then("^the consistency is \"([^\"]+)\"$")]
    public void ThenTheConsistencyIs(string consistency) =>
        Assert.Equal(consistency, OnlyReport().Stats.ConsistencyText);

    [Then("^the current streak is \"([^\"]+)\"$")]
    public void ThenTheCurrentStreakIs(string streak) =>
        Assert.Equal(streak, OnlyReport().Stats.CurrentStreakText);

    [Then("^the longest streak is \"([^\"]+)\"$")]
    public void ThenTheLongestStreakIs(string streak) =>
        Assert.Equal(streak, OnlyReport().Stats.LongestStreakText);

    [Then("^the current streak is \"([^\"]+)\" and the longest streak is \"([^\"]+)\"$")]
    public void ThenTheStreaksAre(string current, string longest)
    {
        var stats = OnlyReport().Stats;
        Assert.Equal(current, stats.CurrentStreakText);
        Assert.Equal(longest, stats.LongestStreakText);
    }

    [Then("^`([^`]+)` has the consistency \"([^\"]+)\", the current streak \"([^\"]+)\" and the longest streak \"([^\"]+)\"$")]
    public void ThenTheGoalHasTheFigures(string goalName, string consistency, string current, string longest)
    {
        Assert.True(context.Reports.TryGetValue(goalName, out var report), $"The When step read no report of `{goalName}`.");
        Assert.Equal(consistency, report!.Stats.ConsistencyText);
        Assert.Equal(current, report.Stats.CurrentStreakText);
        Assert.Equal(longest, report.Stats.LongestStreakText);
    }

    /// <summary>"the report of the goal gives the consistency "X"": slice 6's goal report, read now.</summary>
    [Then("^the report of the goal gives the consistency \"([^\"]+)\"$")]
    public async Task ThenTheReportGivesTheConsistency(string consistency)
    {
        arrange.Then();
        var report = await arrange.ReportAsync(arrange.OnlyGoal());
        Assert.Equal(consistency, report.Stats.ConsistencyText);
    }

    // ------------------------------------------------------------ recent results and trend

    [Then("^the recent results are, oldest first: (.+)$")]
    public void ThenTheRecentResultsAre(string results) =>
        F008Checks.States(OnlyReport().Recent.Results, F008Text.ResultSequence(results));

    [Then("^the number of each state among them is (.+)$")]
    public void ThenTheNumberOfEachStateIs(string counts)
    {
        var actual = OnlyReport().Recent.Counts;
        foreach (var (state, count) in F008Text.StateCounts(counts))
        {
            Assert.True(actual.TryGetValue(state, out var value), $"The recent results give no count of {state}.");
            Assert.True(count == value, $"The count of {state} is {value}, expected {count}.");
        }
    }

    [Then("^the trend is, with the target (\\d+): (.+)$")]
    public void ThenTheTrendIsWithTheTarget(string target, string bars) =>
        F008Checks.Trend(OnlyReport().Trend, F008Text.Bars(bars), F008Text.Int(target));

    [Then("^the trend is (\\(.+)$")]
    public void ThenTheTrendIs(string bars) =>
        F008Checks.Trend(OnlyReport().Trend, F008Text.Bars(bars), null);

    [Then("^the trend contains (.+)$")]
    public void ThenTheTrendContains(string bars) =>
        F008Checks.TrendContains(OnlyReport().Trend, F008Text.Bars(bars));

    // ------------------------------------------------------------ the dashboard the When step read

    [Then("^the number of goals is (\\d+)$")]
    public void ThenTheNumberOfGoalsIs(string count) =>
        Assert.Equal(F008Text.Int(count), WhenDashboard().Counts.Goals);

    [Then("^the Due count is (\\d+) and the Pending count is (\\d+)$")]
    public void ThenTheDueAndPendingCountsAre(string due, string pending)
    {
        var counts = WhenDashboard().Counts;
        Assert.Equal(F008Text.Int(due), counts.Due);
        Assert.Equal(F008Text.Int(pending), counts.Pending);
    }

    [Then("^the Pending count is (\\d+)$")]
    public void ThenThePendingCountIs(string pending) =>
        Assert.Equal(F008Text.Int(pending), WhenDashboard().Counts.Pending);

    [Then("^the Missed count is (\\d+)$")]
    public void ThenTheMissedCountIs(string missed) =>
        Assert.Equal(F008Text.Int(missed), WhenDashboard().Counts.MissedThisWeek);

    [Then("^the Today list contains (.+)$")]
    public void ThenTheTodayListContains(string entries) =>
        F008Checks.Today(WhenDashboard().Today, F008Text.Today(entries));

    [Then("^the summary of `([^`]+)` says it is (paused|not paused)$")]
    public void ThenTheSummarySays(string goalName, string paused) =>
        CheckSummary(WhenDashboard(), goalName, paused);

    [Then("^the summary of `([^`]+)` says it is (paused|not paused), and the summary of `([^`]+)` says it is (paused|not paused)$")]
    public void ThenTheSummariesSay(string firstGoal, string firstPaused, string secondGoal, string secondPaused)
    {
        var dashboard = WhenDashboard();
        CheckSummary(dashboard, firstGoal, firstPaused);
        CheckSummary(dashboard, secondGoal, secondPaused);
    }

    // ------------------------------------------------------------ the dashboard read by a Then step

    [Then("^the dashboard's recent notes are, in this order, (.+)$")]
    public async Task ThenTheDashboardsRecentNotesAre(string notes)
    {
        arrange.Then();
        var dashboard = await arrange.DashboardAsync(arrange.Reader());
        F008Checks.DashboardNotes(dashboard.Notes, F008Text.Notes(notes));
    }

    [Then("^the dashboard's number of goals is (\\d+)$")]
    public async Task ThenTheDashboardsNumberOfGoalsIs(string count)
    {
        arrange.Then();
        var dashboard = await arrange.DashboardAsync(arrange.Reader());
        Assert.Equal(F008Text.Int(count), dashboard.Counts.Goals);
    }

    [Then("^the dashboard gives the Due count (\\d+), the Pending count (\\d+), the Missed count (\\d+) and the number of goals (\\d+)$")]
    public async Task ThenTheDashboardGives(string due, string pending, string missed, string goals)
    {
        arrange.Then();
        var counts = (await arrange.DashboardAsync(arrange.Reader())).Counts;
        Assert.Equal(F008Text.Int(due), counts.Due);
        Assert.Equal(F008Text.Int(pending), counts.Pending);
        Assert.Equal(F008Text.Int(missed), counts.MissedThisWeek);
        Assert.Equal(F008Text.Int(goals), counts.Goals);
    }

    private GoalReport OnlyReport() =>
        context.Reports.Count == 1
            ? context.Reports.Values.Single()
            : throw new InvalidOperationException($"The When step read {context.Reports.Count} reports, not one.");

    private Dashboard WhenDashboard() =>
        context.Dashboard ?? throw new InvalidOperationException("The When step read no dashboard.");

    private static void CheckSummary(Dashboard dashboard, string goalName, string paused)
    {
        var goal = Assert.Single(dashboard.Goals, candidate => candidate.Name == goalName);
        Assert.True(
            (paused == "paused") == goal.PausedUntil.HasValue,
            $"The summary of `{goalName}` gives paused until {goal.PausedUntil?.ToString("yyyy-MM-dd") ?? "none"}; expected {paused}.");
    }
}
