using System.Text.RegularExpressions;
using HT.AcceptanceTests.Support.F007;
using HT.Application.Reports;
using HT.Domain.Evaluation;

namespace HT.AcceptanceTests.StepDefinitions.F007;

/// <summary>The Then steps of F007 about goal reports: consistency, streaks, recent results and trend (FR-001 to FR-010).</summary>
[Binding]
[Scope(Tag = "F007")]
public sealed class ReportThenSteps(F007Context context, F007Arrange arrange, F007Checks checks)
{
    private const string D = "(" + F007Text.D + ")";
    private const string T = "(" + F007Text.T + ")";

    // ------------------------------------------------------------ consistency and streaks of the report

    /// <summary>
    /// "the consistency is "X"", optionally followed by "with the percentage P, the successful number S and the counted
    /// number C", "with the percentage P", "with no percentage and the counted number 0", or "and the current streak is
    /// "Y"".
    /// </summary>
    [Then("^the consistency is \"([^\"]+)\"(.*)$")]
    public async Task ThenTheConsistencyIs(string text, string rest)
    {
        await arrange.ThenAsync();
        var stats = checks.OnlyReport.Stats;
        var streak = Regex.Match(rest, "^ and the current streak is \"([^\"]+)\"$");
        if (streak.Success)
        {
            F007Checks.Consistency(stats, text);
            F007Checks.CurrentStreak(stats, streak.Groups[1].Value);
            return;
        }

        F007Checks.Consistency(stats, text, rest);
    }

    [Then("^the current streak is \"([^\"]+)\" and the longest streak is \"([^\"]+)\"$")]
    public async Task ThenTheCurrentAndLongestStreakAre(string current, string longest)
    {
        await arrange.ThenAsync();
        var stats = checks.OnlyReport.Stats;
        F007Checks.CurrentStreak(stats, current);
        F007Checks.LongestStreak(stats, longest);
    }

    /// <summary>"the current streak is "3 commitments" with the number 3 and the unit "commitments"".</summary>
    [Then("^the current streak is \"([^\"]+)\" with the number (\\d+) and the unit \"([^\"]+)\"$")]
    public async Task ThenTheCurrentStreakIsWithNumberAndUnit(string text, string number, string unit)
    {
        await arrange.ThenAsync();
        var stats = checks.OnlyReport.Stats;
        F007Checks.CurrentStreak(stats, text);
        Assert.Equal(F007Text.Int(number), stats.CurrentStreak);
        Assert.Equal(F007Text.UnitOf(unit), stats.Unit);
        Assert.Equal(unit, ReportText.Unit(stats.Unit, stats.CurrentStreak));
    }

    [Then("^the longest streak is \"([^\"]+)\"$")]
    public async Task ThenTheLongestStreakIs(string text)
    {
        await arrange.ThenAsync();
        F007Checks.LongestStreak(checks.OnlyReport.Stats, text);
    }

    /// <summary>"the first goal's report has the consistency "X" and the current streak "Y"" (AC-008).</summary>
    [Then("^the (first|second) goal's report has (the consistency .+)$")]
    public async Task ThenTheNthGoalsReportHas(string ordinal, string figures)
    {
        await arrange.ThenAsync();
        F007Checks.Figures(checks.ReportFor($"the {ordinal} goal").Stats, figures);
    }

    /// <summary>"each report has the consistency "X", the current streak "Y" and the longest streak "Z"" (AC-019).</summary>
    [Then("^each report has (the consistency .+)$")]
    public async Task ThenEachReportHas(string figures)
    {
        await arrange.ThenAsync();
        var reports = checks.AllReports;
        Assert.Equal(2, reports.Count);
        foreach (var report in reports)
        {
            F007Checks.Figures(report.Stats, figures);
        }
    }

    /// <summary>"<goal> has the consistency "X", the current streak "Y", the longest streak "Z" and N trend bars" (AC-048, AC-049).</summary>
    [Then("^\"([^\"]+)\" has the consistency \"([^\"]+)\", the current streak \"([^\"]+)\", the longest streak \"([^\"]+)\" and (\\d+) trend bars$")]
    public async Task ThenTheNamedGoalHas(string name, string consistency, string current, string longest, string bars)
    {
        await arrange.ThenAsync();
        var report = checks.ReportNamed(name);
        F007Checks.Consistency(report.Stats, consistency);
        F007Checks.CurrentStreak(report.Stats, current);
        F007Checks.LongestStreak(report.Stats, longest);
        Assert.Equal(F007Text.Int(bars), report.Trend.Bars.Count);
    }

    /// <summary>
    /// "at T the report of "N" has the consistency "X", the current streak "Y" and the longest streak "Z"" (AC-050):
    /// "N" is a goal of `demo5@example.com`, the account of the item's other steps, read by that account.
    /// </summary>
    [Then("^at " + T + " the report of \"([^\"]+)\" has (the consistency .+)$")]
    public async Task ThenAtTimeTheReportOfTheNamedGoalHas(string time, string name, string figures)
    {
        await arrange.ThenAsync(time);
        const string email = "demo5@example.com";
        var report = await arrange.ReportAsync(email, await arrange.GoalByNameAsync(email, name));
        F007Checks.Figures(report.Stats, figures);
    }

    // ------------------------------------------------------------ recent results

    [Then("^the recent results (.+)$")]
    public async Task ThenTheRecentResults(string what)
    {
        await arrange.ThenAsync();
        var recent = checks.OnlyReport.Recent;
        var results = recent.Results;
        var text = what.Trim();
        Match match;

        if ((match = Regex.Match(text, "^end with (.+?), and contain no entry for " + D + "$")).Success)
        {
            EndWith(results, match.Groups[1].Value);
            var absent = F007Text.Date(match.Groups[2].Value);
            Assert.DoesNotContain(results, result => result.FirstDate <= absent && absent <= result.LastDate);
            return;
        }

        if ((match = Regex.Match(text, "^end with (.+)$")).Success)
        {
            EndWith(results, match.Groups[1].Value);
            return;
        }

        if ((match = Regex.Match(text, "^begin with (.+)$")).Success)
        {
            var expected = F007Text.Results(match.Groups[1].Value);
            Assert.True(results.Count >= expected.Count, $"The recent results have {results.Count} entries.");
            F007Checks.Results(results.Take(expected.Count).ToList(), expected);
            return;
        }

        if (text == "are empty, with 0 results of each state")
        {
            Assert.Empty(results);
            Assert.All(Enum.GetValues<EvaluationState>(), state => Assert.Equal(0, F007Checks.Count(recent, state)));
            return;
        }

        if ((match = Regex.Match(text, "^contain (.+) exactly once$")).Success)
        {
            var expected = F007Text.Result(match.Groups[1].Value);
            var ofDate = results.Where(result => result.FirstDate == expected.First).ToList();
            var single = Assert.Single(ofDate);
            F007Checks.Result(single, expected);
            return;
        }

        if ((match = Regex.Match(text, "^contain (.+)$")).Success)
        {
            foreach (var expected in F007Text.Results(match.Groups[1].Value))
            {
                F007Checks.Contains(results, expected);
            }

            return;
        }

        if ((match = Regex.Match(text, "^are, in this order: (.+)$")).Success)
        {
            F007Checks.Results(results, F007Text.Results(match.Groups[1].Value));
            return;
        }

        if ((match = Regex.Match(text, "^are exactly one entry, (.+)$")).Success)
        {
            F007Checks.Result(Assert.Single(results), F007Text.Result(match.Groups[1].Value));
            return;
        }

        if ((match = Regex.Match(text, @"^are (\d+) entries, one for each date from " + D + " to " + D + ", in date order$")).Success)
        {
            var dates = F007Text.Range(F007Text.Date(match.Groups[2].Value), F007Text.Date(match.Groups[3].Value));
            Assert.Equal(F007Text.Int(match.Groups[1].Value), dates.Count);
            Assert.Equal(dates.Count, results.Count);
            Assert.Equal(dates, results.Select(result => result.FirstDate).ToList());
            Assert.Equal(dates, results.Select(result => result.LastDate).ToList());
            return;
        }

        if ((match = Regex.Match(text, @"^count (\d+) Met, (\d+) Failed, (\d+) Missed and (\d+) Pending$")).Success)
        {
            Assert.Equal(F007Text.Int(match.Groups[1].Value), F007Checks.Count(recent, EvaluationState.Met));
            Assert.Equal(F007Text.Int(match.Groups[2].Value), F007Checks.Count(recent, EvaluationState.Failed));
            Assert.Equal(F007Text.Int(match.Groups[3].Value), F007Checks.Count(recent, EvaluationState.Missed));
            Assert.Equal(F007Text.Int(match.Groups[4].Value), F007Checks.Count(recent, EvaluationState.Pending));
            return;
        }

        throw new InvalidOperationException($"Cannot read \"the recent results {what}\".");
    }

    /// <summary>"the first entry is R1, and the last two are R2, and R3" (AC-021).</summary>
    [Then("^the first entry is (.+), and the last two are (.+), and (" + F007Text.D + ".+)$")]
    public async Task ThenTheFirstEntryAndTheLastTwoAre(string first, string beforeLast, string last)
    {
        await arrange.ThenAsync();
        var results = checks.OnlyReport.Recent.Results;
        Assert.True(results.Count >= 3, $"The recent results have {results.Count} entries.");
        F007Checks.Result(results[0], F007Text.Result(first));
        F007Checks.Result(results[^2], F007Text.Result(beforeLast));
        F007Checks.Result(results[^1], F007Text.Result(last));
    }

    /// <summary>"they contain R1, and R2" (AC-021): each result is among the recent results.</summary>
    [Then("^they contain (?!neither )(?!no entry )(.+)$")]
    public async Task ThenTheyContain(string list)
    {
        await arrange.ThenAsync();
        var results = checks.OnlyReport.Recent.Results;
        foreach (var expected in F007Text.Results(list))
        {
            F007Checks.Contains(results, expected);
        }
    }

    [Then("^they contain no entry for " + D + " or " + D + "$")]
    public async Task ThenTheyContainNoEntryFor(string first, string second)
    {
        await arrange.ThenAsync();
        var results = checks.OnlyReport.Recent.Results;
        foreach (var date in new[] { F007Text.Date(first), F007Text.Date(second) })
        {
            Assert.DoesNotContain(results, result => result.FirstDate <= date && date <= result.LastDate);
        }
    }

    /// <summary>"each report's recent results are, in this order: …" (AC-019).</summary>
    [Then("^each report's recent results are, in this order: (.+)$")]
    public async Task ThenEachReportsRecentResultsAre(string list)
    {
        await arrange.ThenAsync();
        var expected = F007Text.Results(list);
        var reports = checks.AllReports;
        Assert.Equal(2, reports.Count);
        foreach (var report in reports)
        {
            F007Checks.Results(report.Recent.Results, expected);
        }
    }

    // ------------------------------------------------------------ trend

    [Then("^the trend (?!of \")(.+)$")]
    public async Task ThenTheTrend(string what)
    {
        await arrange.ThenAsync();
        var trend = checks.OnlyReport.Trend;
        var text = what.Trim();
        Match match;

        if (text == "has no bar")
        {
            Assert.Empty(trend.Bars);
            return;
        }

        if ((match = Regex.Match(text, @"^contains the bar (\(.+\))$")).Success)
        {
            var expected = Assert.Single(F007Text.Bars(match.Groups[1].Value));
            F007Checks.Bar(Assert.Single(trend.Bars, bar => bar.FirstDate == expected.First), expected, null);
            return;
        }

        if ((match = Regex.Match(text, @"^has (\d+) bars with the target (\d+), in this order: (.+)$")).Success)
        {
            var expected = F007Text.Bars(match.Groups[3].Value);
            Assert.Equal(F007Text.Int(match.Groups[1].Value), expected.Count);
            F007Checks.Bars(trend, expected, F007Text.Int(match.Groups[2].Value));
            return;
        }

        if ((match = Regex.Match(text, @"^has (\d+) bars, in this order: (.+)$")).Success)
        {
            var expected = F007Text.Bars(match.Groups[2].Value);
            Assert.Equal(F007Text.Int(match.Groups[1].Value), expected.Count);
            F007Checks.Bars(trend, expected, null);
            return;
        }

        if ((match = Regex.Match(text, @"^is, with the target (\d+): (.+)$")).Success)
        {
            F007Checks.Bars(trend, F007Text.Bars(match.Groups[2].Value), F007Text.Int(match.Groups[1].Value));
            return;
        }

        throw new InvalidOperationException($"Cannot read \"the trend {what}\".");
    }

    /// <summary>"the `Reading` goal's trend is, with the target 5: …" (AC-027, AC-028, AC-065).</summary>
    [Then("^the `([^`]+)` goal's trend is, with the target (\\d+): (.+)$")]
    public async Task ThenTheGoalsTrendIs(string label, string target, string bars)
    {
        await arrange.ThenAsync();
        F007Checks.Bars(checks.ReportFor($"the `{label}` goal").Trend, F007Text.Bars(bars), F007Text.Int(target));
    }

    /// <summary>"the trend of "N" is, with the target T: …" (AC-066).</summary>
    [Then("^the trend of \"([^\"]+)\" is, with the target (\\d+): (.+)$")]
    public async Task ThenTheTrendOfTheNamedGoalIs(string name, string target, string bars)
    {
        await arrange.ThenAsync();
        F007Checks.Bars(checks.ReportNamed(name).Trend, F007Text.Bars(bars), F007Text.Int(target));
    }

    /// <summary>"each report's trend is, in this order: …, with the target T" (AC-019).</summary>
    [Then("^each report's trend is, in this order: (.+), with the target (\\d+)$")]
    public async Task ThenEachReportsTrendIs(string bars, string target)
    {
        await arrange.ThenAsync();
        var expected = F007Text.Bars(bars);
        var reports = checks.AllReports;
        Assert.Equal(2, reports.Count);
        foreach (var report in reports)
        {
            F007Checks.Bars(report.Trend, expected, F007Text.Int(target));
        }
    }

    // ------------------------------------------------------------ refusals

    [Then("^the result is \"([^\"]+)\" and no figure is returned$")]
    public async Task ThenTheResultIsAndNoFigureIsReturned(string message)
    {
        await arrange.ThenAsync();
        var report = Assert.Single(context.Reports);
        Assert.Equal(GoalReportOutcome.NotFound, report.Result.Outcome);
        Assert.Equal(message, report.Result.Message);
        Assert.Null(report.Result.Report);
    }

    [Then("^the result is \"([^\"]+)\" and no data is returned$")]
    public async Task ThenTheResultIsAndNoDataIsReturned(string message)
    {
        await arrange.ThenAsync();
        var call = Assert.Single(context.Calls);
        Assert.False(call.Found);
        Assert.Equal(message, call.Message);
        Assert.False(call.ReturnedData);
    }

    /// <summary>The recent results end with the listed results, in order.</summary>
    private static void EndWith(IReadOnlyList<ResultView> results, string list)
    {
        var expected = F007Text.Results(list);
        Assert.True(results.Count >= expected.Count, $"The recent results have {results.Count} entries.");
        F007Checks.Results(results.Skip(results.Count - expected.Count).ToList(), expected);
    }
}
