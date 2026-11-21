using HT.Application.Reports;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Reports;

/// <summary>
/// The recent results of a goal through <see cref="ReportService.GetGoalReportAsync"/> (US4-AS1, AS2; FR-005, Edge
/// Cases), over the fakes of <see cref="ReportWorld"/>. The user has no stored time zone, so the goals are in UTC,
/// start on Monday 2 November 2026 and are created at 08:00Z that day. A day runs from 00:00Z to 00:00Z the next day and
/// its grace period ends 48 hours later; a week runs from Monday 00:00Z and its grace period ends on the Wednesday after
/// it, at 00:00Z. Every instant is worked out by hand.
/// </summary>
public sealed class GoalReportRecentTests
{
    private static readonly DateOnly Mon2 = Date(2026, 11, 2);
    private static readonly DateTimeOffset Mon2Morning = Utc("2026-11-02T08:00:00Z");

    private readonly ReportWorld _world = new();

    private static GoalTargetFields Reading20MinDaily => DurationDays(20, DurationUnit.Minutes, BuildOrReduce.Build, AllDays);

    /// <summary>
    /// "Reading, 20 min daily" from Monday 2 November, read on Saturday 12 December at 10:00Z: 2 November to 9 December
    /// are final (38 days), 10 and 11 December Pending and 12 December Due, so the goal has 40 ended results. Wednesday 4
    /// November has no record (Missed), Tuesday 1 December 10 minutes (Failed), 10 December 20 minutes, 11 December
    /// none, 12 December 25 minutes (Due), every other day 20 minutes (Met). The latest 30 run from 12 November to 11
    /// December: 27 Met, 1 Failed, no Missed and 2 Pending.
    /// </summary>
    private async Task<GoalId> FortyEndedResultsAsync()
    {
        var goal = await _world.CreateGoalAsync(Reading20MinDaily, Mon2, Mon2Morning);
        for (var date = Mon2; date <= Date(2026, 12, 12); date = date.AddDays(1))
        {
            if (date == Date(2026, 11, 4) || date == Date(2026, 12, 11))
            {
                continue;
            }

            _world.SeedAmount(goal, date, date == Date(2026, 12, 1) ? 10 : date == Date(2026, 12, 12) ? 25 : 20);
        }

        _world.At(Utc("2026-12-12T10:00:00Z"));
        return goal;
    }

    [Fact]
    public async Task US4_AS1_40_ended_results_give_the_latest_30_oldest_first_with_their_date_state_and_progress()
    {
        var goal = await FortyEndedResultsAsync();

        var results = (await _world.ReportAsync(goal)).Recent.Results;

        Assert.Equal(30, results.Count);
        Assert.Equal(Enumerable.Range(0, 30).Select(day => Date(2026, 11, 12).AddDays(day)), results.Select(result => result.FirstDate));
        Assert.All(results, result => Assert.Equal(result.FirstDate, result.LastDate));
        Assert.Equal(
            new ResultView { FirstDate = Date(2026, 11, 12), LastDate = Date(2026, 11, 12), State = EvaluationState.Met, Progress = new Progress(ProgressKind.RecordedValue, 20, 20) },
            results[0]);
        Assert.Equal(
            new ResultView { FirstDate = Date(2026, 12, 1), LastDate = Date(2026, 12, 1), State = EvaluationState.Failed, Progress = new Progress(ProgressKind.RecordedValue, 10, 20) },
            results[19]);
        Assert.Equal(
            [
                new ResultView { FirstDate = Date(2026, 12, 10), LastDate = Date(2026, 12, 10), State = EvaluationState.Pending, Progress = new Progress(ProgressKind.RecordedValue, 20, 20) },
                new ResultView { FirstDate = Date(2026, 12, 11), LastDate = Date(2026, 12, 11), State = EvaluationState.Pending, Progress = new Progress(ProgressKind.RecordedValue, null, 20) },
            ],
            results.TakeLast(2));
        Assert.DoesNotContain(results, result => result.FirstDate == Date(2026, 12, 12));
        Assert.DoesNotContain(results, result => result.State is EvaluationState.Due or EvaluationState.Upcoming);
    }

    [Fact]
    public async Task US4_AS2_the_report_counts_each_ended_state_among_the_recent_results_0_included()
    {
        var goal = await FortyEndedResultsAsync();

        var counts = (await _world.ReportAsync(goal)).Recent.Counts;

        Assert.Equal(
            new Dictionary<EvaluationState, int>
            {
                [EvaluationState.Met] = 27, [EvaluationState.Failed] = 1, [EvaluationState.Missed] = 0, [EvaluationState.Pending] = 2,
            },
            counts);
    }

    [Fact]
    public async Task Edge_case_consistency_and_streaks_still_count_all_40_results()
    {
        // 38 final results: 36 Met, the Missed 4 November and the Failed 1 December. The longest run is 5 to 30 November
        // (26 commitments), longer than the 19 of that run among the recent results; the current one 2 to 9 December.
        var goal = await FortyEndedResultsAsync();

        var stats = (await _world.ReportAsync(goal)).Stats;

        Assert.Equal("94% (36 of 38)", stats.ConsistencyText);
        Assert.Equal((8, 26), (stats.CurrentStreak, stats.LongestStreak));
    }

    [Fact]
    public async Task FR_005_a_per_week_result_has_its_first_and_last_date()
    {
        // Once a week: "Yes" on Tuesday 3 and Tuesday 17 November. Read on Tuesday 24 November at 10:00Z: the weeks of 2
        // and 9 November are final (Met, then Missed), the week of 16 is Pending (its grace period ends on Wednesday 25)
        // and the week of 23 is Due.
        var goal = await _world.CreateGoalAsync(YesNoTimes(1, SchedulePeriod.Week), Mon2, Mon2Morning);
        _world.SeedAnswer(goal, Date(2026, 11, 3), YesNoAnswer.Yes);
        _world.SeedAnswer(goal, Date(2026, 11, 17), YesNoAnswer.Yes);
        _world.At(Utc("2026-11-24T10:00:00Z"));

        var results = (await _world.ReportAsync(goal)).Recent.Results;

        Assert.Equal(
            [
                (Mon2, Date(2026, 11, 8), EvaluationState.Met),
                (Date(2026, 11, 9), Date(2026, 11, 15), EvaluationState.Missed),
                (Date(2026, 11, 16), Date(2026, 11, 22), EvaluationState.Pending),
            ],
            results.Select(result => (result.FirstDate, result.LastDate, result.State)));
        Assert.Equal(new Progress(ProgressKind.RecordsReachingAmount, 1, 1), results[0].Progress);
    }

    [Fact]
    public async Task Edge_case_a_goal_not_started_has_no_recent_result()
    {
        var goal = await _world.CreateGoalAsync(Reading20MinDaily, Date(2026, 11, 20), Utc("2026-11-16T08:00:00Z"));
        _world.At(Utc("2026-11-17T10:00:00Z"));

        var recent = (await _world.ReportAsync(goal)).Recent;

        Assert.Empty(recent.Results);
        Assert.All(recent.Counts.Values, count => Assert.Equal(0, count));
        Assert.Equal(
            new[] { EvaluationState.Met, EvaluationState.Failed, EvaluationState.Missed, EvaluationState.Pending }.Order(),
            recent.Counts.Keys.Order());
    }
}
