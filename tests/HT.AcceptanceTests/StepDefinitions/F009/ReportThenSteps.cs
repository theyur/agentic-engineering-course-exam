using System.Text.RegularExpressions;
using HT.AcceptanceTests.Support.F009;
using HT.Application.Reports;
using HT.Domain.Evaluation;

namespace HT.AcceptanceTests.StepDefinitions.F009;

/// <summary>The Then steps of F009 about reports, the goals overview and the dashboard (slice 6, with FR-013, FR-018, FR-019).</summary>
[Binding]
[Scope(Tag = "F009")]
public sealed class ReportThenSteps(F009Context context, F009Arrange arrange)
{
    private const string D = "(" + F009Text.D + ")";
    private const string AtT = "((?:at " + F009Text.T + " )?)";

    // ------------------------------------------------------------ reports

    [Then("^the consistency is \"([^\"]*)\"$")]
    public void ThenTheConsistencyIs(string consistency) =>
        Assert.Equal(consistency, WhenReport().Stats.ConsistencyText);

    [Then("^the current streak is \"([^\"]*)\" and the longest streak is \"([^\"]*)\"$")]
    public void ThenTheStreaksAre(string current, string longest)
    {
        var stats = WhenReport().Stats;
        Assert.Equal(current, stats.CurrentStreakText);
        Assert.Equal(longest, stats.LongestStreakText);
    }

    /// <summary>"[at T ]the report of the goal|`G` gives the consistency "X"[, the current streak "Y"][ and the longest streak "Z"]".</summary>
    [Then("^" + AtT + "the report of (the goal|`[^`]+`) gives (the .+)$")]
    public async Task ThenTheReportGives(string at, string goalPhrase, string figures)
    {
        arrange.Then(TimeOf(at));
        var goal = await arrange.ThenGoalAsync(goalPhrase);
        F009Checks.Stats((await arrange.ReportAsync(goal)).Stats, figures);
    }

    /// <summary>After "reads the reports of …": "`G` has the consistency "X", the current streak "Y" and the longest streak "Z"".</summary>
    [Then("^`([^`]+)` has (the consistency .+)$")]
    public void ThenTheGoalHas(string name, string figures)
    {
        Assert.Equal(F009ReadKind.Reports, context.ReadKind);
        Assert.True(context.Reports.TryGetValue(name, out var report), $"The When step read no report of `{name}`.");
        F009Checks.Stats(report!.Stats, figures);
    }

    [Then("^the recent results of the report of the goal contain no date before " + D + "$")]
    public async Task ThenTheRecentResultsContainNoDateBefore(string date)
    {
        arrange.Then();
        var report = await arrange.ReportAsync(arrange.OnlyGoal());
        var first = F009Text.Date(date);
        Assert.DoesNotContain(report.Recent.Results, result => result.FirstDate < first);
    }

    [Then("^the trend of the report of the goal has no bar for a date before " + D + "$")]
    public async Task ThenTheTrendHasNoBarBefore(string date)
    {
        arrange.Then();
        var report = await arrange.ReportAsync(arrange.OnlyGoal());
        var first = F009Text.Date(date);
        Assert.DoesNotContain(report.Trend.Bars, bar => bar.FirstDate < first);
    }

    // ------------------------------------------------------------ goals overview

    /// <summary>After "reads the goals overview": "it gives `G` ended on D with the outcome O|not ended[, not paused][, with the consistency "X"]".</summary>
    [Then("^it gives `([^`]+)` ((?:not )?ended.*)$")]
    public void ThenItGivesTheOverviewGoal(string name, string clause)
    {
        Assert.Equal(F009ReadKind.Overview, context.ReadKind);
        OverviewGoal(context.Overview ?? throw new InvalidOperationException("The When step read no overview."), name, clause);
    }

    [Then("^the goals overview gives `([^`]+)` ((?:not )?ended(?:(?!, and `).)*)$")]
    public async Task ThenTheGoalsOverviewGives(string name, string clause)
    {
        arrange.Then();
        OverviewGoal(await arrange.OverviewAsync(arrange.Reader()), name, clause);
    }

    /// <summary>AC-090: one goal ended with its outcome, two not ended.</summary>
    [Then("^the goals overview gives `([^`]+)` ended with the outcome (" + F009Text.Outcomes + "), and `([^`]+)` and `([^`]+)` not ended$")]
    public async Task ThenTheGoalsOverviewGivesOneEndedTwoNot(string ended, string outcome, string first, string second)
    {
        arrange.Then();
        var overview = await arrange.OverviewAsync(arrange.Reader());
        OverviewGoal(overview, ended, $"ended with the outcome {outcome}");
        OverviewGoal(overview, first, "not ended");
        OverviewGoal(overview, second, "not ended");
    }

    [Then("^" + AtT + "the goals overview of `([^`]+)` lists exactly (.+)$")]
    public async Task ThenTheGoalsOverviewListsExactly(string at, string email, string goals)
    {
        arrange.Then(TimeOf(at));
        var overview = await arrange.OverviewAsync(email);
        var expected = goals == "no goal" ? new List<string>() : F009Text.Backticked(goals);
        if (goals != "no goal")
        {
            F009Text.RequireOnlySeparators(Regex.Replace(goals, "`[^`]+`", string.Empty), goals);
        }

        Assert.Equal(expected.Order(StringComparer.Ordinal).ToList(), overview.Goals.Select(goal => goal.Item.Name).Order(StringComparer.Ordinal).ToList());
    }

    // ------------------------------------------------------------ dashboard

    [Then("^the Today list contains (\\(.+)$")]
    public async Task ThenTheTodayListContains(string tuples) => await TodayContainsAsync(WhenDashboard(), tuples);

    [Then("^the dashboard's Today list contains (\\(.+)$")]
    public async Task ThenTheDashboardsTodayListContains(string tuples)
    {
        arrange.Then();
        await TodayContainsAsync(await arrange.DashboardAsync(arrange.Reader()), tuples);
    }

    [Then("^the Today list contains no (Due|Upcoming|Pending) and no (Due|Upcoming|Pending) entry of `([^`]+)`$")]
    public async Task ThenTheTodayListContainsNoStatesOf(string first, string second, string name)
    {
        var goal = await arrange.GoalNamedAsync(name);
        var states = new[] { F009Text.StateOf(first), F009Text.StateOf(second) };
        Assert.DoesNotContain(WhenDashboard().Today, entry => entry.GoalId == goal.Id && states.Contains(entry.Entry.State));
    }

    [Then("^the Today list contains no entry of `([^`]+)`$")]
    public async Task ThenTheTodayListContainsNoEntryOf(string name)
    {
        var goal = await arrange.GoalNamedAsync(name);
        Assert.DoesNotContain(WhenDashboard().Today, entry => entry.GoalId == goal.Id);
    }

    [Then("^the Today list contains no entry for " + D + "$")]
    public void ThenTheTodayListContainsNoEntryFor(string date) => NoEntryFor(WhenDashboard(), F009Text.Date(date));

    [Then("^the dashboard's Today list contains no entry for " + D + "$")]
    public async Task ThenTheDashboardsTodayListContainsNoEntryFor(string date)
    {
        arrange.Then();
        NoEntryFor(await arrange.DashboardAsync(arrange.Reader()), F009Text.Date(date));
    }

    [Then("^the dashboard has no summary of `([^`]+)`$")]
    public async Task ThenTheDashboardHasNoSummaryOf(string name)
    {
        var goal = await arrange.GoalNamedAsync(name);
        Assert.DoesNotContain(WhenDashboard().Goals, summary => summary.GoalId == goal.Id);
    }

    [Then("^the number of goals is (\\d+)$")]
    public void ThenTheNumberOfGoalsIs(string count) => Assert.Equal(F009Text.Int(count), WhenDashboard().Counts.Goals);

    [Then("^the (Pending|Due|Missed) count is (\\d+)((?: and the (?:Pending|Due|Missed) count is \\d+)?)$")]
    public void ThenTheCountIs(string what, string count, string secondClause)
    {
        var counts = WhenDashboard().Counts;
        AssertCount(counts, what, F009Text.Int(count));
        var second = Regex.Match(secondClause, "^ and the (Pending|Due|Missed) count is (\\d+)$");
        if (second.Success)
        {
            AssertCount(counts, second.Groups[1].Value, F009Text.Int(second.Groups[2].Value));
        }
    }

    /// <summary>"the dashboard gives the Pending count 1, the Due count 1 and the Missed count 0[ and the number of goals 2]".</summary>
    [Then("^the dashboard gives (the .+)$")]
    public async Task ThenTheDashboardGives(string facts)
    {
        arrange.Then();
        var counts = (await arrange.DashboardAsync(arrange.Reader())).Counts;
        var matches = Regex.Matches(facts, "the (Pending|Due|Missed) count (\\d+)|the number of goals (\\d+)");
        F009Text.RequireOnlySeparators(Regex.Replace(facts, "the (Pending|Due|Missed) count (\\d+)|the number of goals (\\d+)", string.Empty), facts);
        Assert.NotEmpty(matches);
        foreach (Match match in matches)
        {
            if (match.Groups[3].Success)
            {
                Assert.Equal(F009Text.Int(match.Groups[3].Value), counts.Goals);
            }
            else
            {
                AssertCount(counts, match.Groups[1].Value, F009Text.Int(match.Groups[2].Value));
            }
        }
    }

    [Then("^the dashboard's number of goals is (\\d+)$")]
    public async Task ThenTheDashboardsNumberOfGoalsIs(string count)
    {
        arrange.Then();
        Assert.Equal(F009Text.Int(count), (await arrange.DashboardAsync(arrange.Reader())).Counts.Goals);
    }

    [Then("^the dashboard's recent notes contain no entry of `([^`]+)`$")]
    public async Task ThenTheDashboardsRecentNotesContainNoEntryOf(string name)
    {
        arrange.Then();
        var goal = await arrange.GoalNamedAsync(name);
        var dashboard = await arrange.DashboardAsync(arrange.Reader());
        Assert.DoesNotContain(dashboard.Notes, note => note.GoalId == goal.Id);
    }

    // ------------------------------------------------------------ helpers

    private GoalReport WhenReport()
    {
        Assert.Equal(F009ReadKind.Report, context.ReadKind);
        return context.Report ?? throw new InvalidOperationException("The When step read no report.");
    }

    private Dashboard WhenDashboard()
    {
        Assert.Equal(F009ReadKind.Dashboard, context.ReadKind);
        return context.Dashboard ?? throw new InvalidOperationException("The When step read no dashboard.");
    }

    private async Task TodayContainsAsync(Dashboard dashboard, string tuples)
    {
        foreach (var spec in F009Text.Tuples(tuples))
        {
            var goal = await arrange.GoalNamedAsync(spec.Goal);
            F009Checks.TodayEntry(dashboard.Today, goal, spec);
        }
    }

    private static void NoEntryFor(Dashboard dashboard, DateOnly date) =>
        Assert.DoesNotContain(dashboard.Today, entry => F009Checks.Covers(entry.Entry.FirstDate, entry.Entry.LastDate, date));

    private static void AssertCount(DashboardCounts counts, string what, int expected)
    {
        var actual = what switch
        {
            "Pending" => counts.Pending,
            "Due" => counts.Due,
            _ => counts.MissedThisWeek,
        };
        Assert.True(actual == expected, $"The {what} count is {actual}; expected {expected}.");
    }

    private static readonly Regex OverviewFact = new(
        "\\G(?:, | )?(?:(?<notended>not ended)|(?<ended>ended)(?: on (?<on>" + F009Text.D + "))?(?: with the outcome (?<outcome>" + F009Text.Outcomes + "))?" +
        "|(?<notpaused>not paused)|(?<paused>paused)|with the consistency \"(?<consistency>[^\"]*)\")",
        RegexOptions.CultureInvariant);

    /// <summary>A goal of the overview, found by its name, with every fact written (FR-018).</summary>
    private static void OverviewGoal(GoalsOverview overview, string name, string clause)
    {
        var goal = Assert.Single(overview.Goals, candidate => candidate.Item.Name == name);
        var position = 0;
        while (position < clause.Length)
        {
            var match = OverviewFact.Match(clause, position);
            if (!match.Success || match.Length == 0)
            {
                throw new InvalidOperationException($"Cannot read \"{clause[position..]}\" in \"{clause}\".");
            }

            if (match.Groups["notended"].Success)
            {
                Assert.False(goal.Item.IsEnded, $"`{name}` is ended; expected not ended.");
            }
            else if (match.Groups["ended"].Success)
            {
                Assert.True(goal.Item.IsEnded, $"`{name}` is not ended; expected ended.");
                if (match.Groups["on"].Success)
                {
                    Assert.Equal(F009Text.Date(match.Groups["on"].Value), goal.Item.EndingDate);
                }

                if (match.Groups["outcome"].Success)
                {
                    Assert.Equal(F009Text.OutcomeOf(match.Groups["outcome"].Value), goal.Item.Outcome);
                }
            }
            else if (match.Groups["notpaused"].Success)
            {
                Assert.True(goal.PausedUntil is null, $"`{name}` is paused until {goal.PausedUntil:yyyy-MM-dd}; expected not paused.");
            }
            else if (match.Groups["paused"].Success)
            {
                Assert.NotNull(goal.PausedUntil);
            }
            else if (match.Groups["consistency"].Success)
            {
                Assert.Equal(match.Groups["consistency"].Value, goal.Stats.ConsistencyText);
            }

            position = match.Index + match.Length;
        }
    }

    private static string? TimeOf(string at)
    {
        var match = Regex.Match(at, F009Text.T);
        return match.Success ? match.Value : null;
    }
}
