using System.Text.RegularExpressions;
using HT.Application.Reports;
using HT.Domain.Evaluation;
using HT.Domain.Reports;

namespace HT.AcceptanceTests.Support.F007;

/// <summary>
/// Compares what the use cases of this slice returned with what an item writes (acceptance.md, "Figures"): a
/// consistency by its text and, when written, its percentage and numbers; a streak by its text and, when written, its
/// number and unit; results, trend bars, Today entries and recent notes as far as the item gives them.
/// </summary>
public sealed class F007Checks(F007Context context, F007Arrange arrange)
{
    // ------------------------------------------------------------ the reports of the When step

    /// <summary>The one goal report the When step read; it must have been found.</summary>
    public GoalReport OnlyReport
    {
        get
        {
            var report = Assert.Single(context.Reports);
            return Found(report);
        }
    }

    public IReadOnlyList<GoalReport> AllReports
    {
        get
        {
            Assert.NotEmpty(context.Reports);
            return context.Reports.Select(Found).ToList();
        }
    }

    /// <summary>The report the When step read for a goal of the item ("the `Reading` goal", "the first goal").</summary>
    public GoalReport ReportFor(string selector)
    {
        var goal = arrange.Goal(null, selector);
        return Found(Assert.Single(context.Reports, report => report.Goal?.Id == goal.Id));
    }

    /// <summary>The report the When step read for a goal of the given display name (demo goals).</summary>
    public GoalReport ReportNamed(string name) =>
        Assert.Single(AllReports, report => report.Name == name);

    private static GoalReport Found(F007Report report)
    {
        Assert.Equal(GoalReportOutcome.Found, report.Result.Outcome);
        return report.Result.Report ?? throw new InvalidOperationException("The goal report gave no report.");
    }

    public Dashboard Dashboard
    {
        get
        {
            var result = context.Dashboard ?? throw new InvalidOperationException("The When step read no dashboard.");
            Assert.Equal(DashboardOutcome.Found, result.Outcome);
            return result.Dashboard ?? throw new InvalidOperationException("The dashboard gave no dashboard.");
        }
    }

    public GoalsOverview Overview
    {
        get
        {
            var result = context.Overview ?? throw new InvalidOperationException("The When step read no goals overview.");
            Assert.Equal(GoalsOverviewOutcome.Found, result.Outcome);
            return result.Overview ?? throw new InvalidOperationException("The goals overview gave no overview.");
        }
    }

    // ------------------------------------------------------------ figures

    /// <summary>A consistency text, and "with the percentage P(, the successful number S and the counted number C)" or "with no percentage and the counted number 0".</summary>
    public static void Consistency(GoalStatsView stats, string text, string rest = "")
    {
        Assert.Equal(text, stats.ConsistencyText);
        var details = rest.Trim();
        if (details.Length == 0)
        {
            return;
        }

        Match match;
        if ((match = Regex.Match(details, @"^with the percentage (\d+), the successful number (\d+) and the counted number (\d+)$")).Success)
        {
            Assert.Equal(F007Text.Int(match.Groups[1].Value), stats.Consistency.Percentage);
            Assert.Equal(F007Text.Int(match.Groups[2].Value), stats.Consistency.Successful);
            Assert.Equal(F007Text.Int(match.Groups[3].Value), stats.Consistency.Counted);
            return;
        }

        if ((match = Regex.Match(details, @"^with the percentage (\d+)$")).Success)
        {
            Assert.Equal(F007Text.Int(match.Groups[1].Value), stats.Consistency.Percentage);
            return;
        }

        if ((match = Regex.Match(details, @"^with no percentage and the counted number (\d+)$")).Success)
        {
            Assert.Null(stats.Consistency.Percentage);
            Assert.Equal(F007Text.Int(match.Groups[1].Value), stats.Consistency.Counted);
            return;
        }

        throw new InvalidOperationException($"Cannot read \"{rest}\".");
    }

    public static void CurrentStreak(GoalStatsView stats, string text) => Assert.Equal(text, stats.CurrentStreakText);

    public static void LongestStreak(GoalStatsView stats, string text) => Assert.Equal(text, stats.LongestStreakText);

    /// <summary>"the consistency "X"", "the current streak "Y"", "the longest streak "Z"" as an item lists them.</summary>
    public static void Figures(GoalStatsView stats, string text)
    {
        foreach (var (figure, value) in F007Text.Figures(text))
        {
            switch (figure)
            {
                case "consistency":
                    Consistency(stats, value);
                    break;
                case "current streak":
                    CurrentStreak(stats, value);
                    break;
                case "longest streak":
                    LongestStreak(stats, value);
                    break;
            }
        }
    }

    // ------------------------------------------------------------ results

    public static void Result(ResultView actual, F007Text.ResultSpec expected)
    {
        Assert.Equal(expected.First, actual.FirstDate);
        Assert.Equal(expected.Last, actual.LastDate);
        Assert.Equal(expected.State, actual.State);
        if (expected.ProgressValue is { } value)
        {
            Assert.Equal(value, actual.Progress.Value);
            Assert.Equal(expected.ProgressOf, actual.Progress.Of);
        }

        if (expected.NoRecord)
        {
            // A commitment without a record has no recorded value (slice 5 FR-014).
            Assert.Null(actual.Progress.Value);
        }
    }

    public static void Results(IReadOnlyList<ResultView> actual, IReadOnlyList<F007Text.ResultSpec> expected)
    {
        Assert.Equal(expected.Count, actual.Count);
        for (var index = 0; index < expected.Count; index++)
        {
            Result(actual[index], expected[index]);
        }
    }

    /// <summary>The result of a date (or period) among the results: exactly one, matching what the item writes.</summary>
    public static void Contains(IReadOnlyList<ResultView> actual, F007Text.ResultSpec expected)
    {
        var result = Assert.Single(actual, item => item.FirstDate == expected.First);
        Result(result, expected);
    }

    // ------------------------------------------------------------ trend bars

    public static void Bar(TrendBar actual, F007Text.BarSpec expected, int? target)
    {
        Assert.Equal(expected.First, actual.FirstDate);
        Assert.Equal(expected.Last, actual.LastDate);
        Assert.Equal(expected.Value, actual.Value);
        if ((expected.Target ?? target) is { } barTarget)
        {
            Assert.Equal(barTarget, actual.Target);
        }
    }

    /// <summary>The trend is exactly these bars, oldest first; with a trend target, the trend and every bar have it.</summary>
    public static void Bars(TrendView trend, IReadOnlyList<F007Text.BarSpec> expected, int? target)
    {
        if (target is { } value)
        {
            Assert.Equal(value, trend.Target);
        }

        Assert.Equal(expected.Count, trend.Bars.Count);
        for (var index = 0; index < expected.Count; index++)
        {
            Bar(trend.Bars[index], expected[index], target);
        }
    }

    // ------------------------------------------------------------ Today entries and recent notes

    public void Entry(TodayEntry actual, F007Text.EntrySpec expected)
    {
        var goal = expected.Goal;
        if (goal.StartsWith('"'))
        {
            Assert.Equal(goal.Trim('"'), actual.GoalName);
        }
        else
        {
            Assert.Equal(arrange.Goal(null, goal).Id, actual.GoalId);
        }

        if (expected.Kind is { } kind)
        {
            Assert.Equal(kind, actual.BuildOrReduce);
        }

        Assert.Equal(expected.First, actual.Entry.FirstDate);
        Assert.Equal(expected.Last, actual.Entry.LastDate);
        Assert.Equal(expected.State, actual.Entry.State);
        if (expected.ProgressValue is { } value)
        {
            Assert.Equal(value, actual.Entry.Progress.Value);
            Assert.Equal(expected.ProgressOf, actual.Entry.Progress.Of);
        }

        if (expected.GraceEnds is { } grace)
        {
            Assert.Equal(grace, actual.Entry.GraceEnds);
        }

        if (expected.HasRecord is { } record)
        {
            Assert.Equal(record, actual.HasRecord);
        }

        if (expected.HasNote is { } note)
        {
            Assert.Equal(note, actual.HasNote);
        }
    }

    /// <summary>Whether an entry is the one an item writes (for "contains").</summary>
    public bool IsEntry(TodayEntry actual, F007Text.EntrySpec expected)
    {
        var goal = expected.Goal;
        var sameGoal = goal.StartsWith('"')
            ? actual.GoalName == goal.Trim('"')
            : actual.GoalId == arrange.Goal(null, goal).Id;
        return sameGoal
            && (expected.Kind is null || expected.Kind == actual.BuildOrReduce)
            && actual.Entry.FirstDate == expected.First
            && actual.Entry.LastDate == expected.Last
            && actual.Entry.State == expected.State
            && (expected.ProgressValue is null
                || (actual.Entry.Progress.Value == expected.ProgressValue && actual.Entry.Progress.Of == expected.ProgressOf))
            && (expected.GraceEnds is null || actual.Entry.GraceEnds == expected.GraceEnds)
            && (expected.HasRecord is null || actual.HasRecord == expected.HasRecord)
            && (expected.HasNote is null || actual.HasNote == expected.HasNote);
    }

    public void Entries(IReadOnlyList<TodayEntry> actual, IReadOnlyList<F007Text.EntrySpec> expected)
    {
        Assert.Equal(expected.Count, actual.Count);
        for (var index = 0; index < expected.Count; index++)
        {
            Entry(actual[index], expected[index]);
        }
    }

    public static void Notes(IReadOnlyList<DashboardNote> actual, IReadOnlyList<F007Text.NoteSpec> expected)
    {
        Assert.Equal(expected.Count, actual.Count);
        for (var index = 0; index < expected.Count; index++)
        {
            Assert.Equal(expected[index].GoalName, actual[index].GoalName);
            Assert.Equal(expected[index].First, actual[index].FirstDate);
            Assert.Equal(expected[index].Last, actual[index].LastDate);
            Assert.Equal(expected[index].Note, actual[index].Note);
            Assert.Equal(expected[index].State, actual[index].State);
        }
    }

    /// <summary>The number of results of a state among the recent results (a state that is absent counts 0).</summary>
    public static int Count(RecentResults recent, EvaluationState state) =>
        recent.Counts.TryGetValue(state, out var count) ? count : 0;
}
