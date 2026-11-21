using HT.Application.Reports;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using HT.Domain.Reports;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Reports;

/// <summary>
/// The consistency of a goal through <see cref="ReportService.GetGoalReportAsync"/> (US1-AS1 to AS7; FR-001, FR-002,
/// FR-010), over the fakes of <see cref="ReportWorld"/>. The user has no stored time zone, so the goals are in UTC.
/// Unless a test says otherwise a goal starts on Monday 2 November 2026 and is created at 08:00Z that day. A day runs
/// from 00:00Z to 00:00Z the next day and its grace period ends 48 hours later, so Saturday 21 November's ends at
/// 2026-11-24T00:00:00Z; a week runs from Monday 00:00Z and its grace period ends on the Wednesday after it, at 00:00Z.
/// Every instant is worked out by hand.
/// </summary>
public sealed class GoalReportConsistencyTests
{
    private static readonly DateOnly Mon2 = Date(2026, 11, 2);
    private static readonly DateTimeOffset Mon2Morning = Utc("2026-11-02T08:00:00Z");

    /// <summary>Saturday 7 November, 10:00Z: Wednesday 4 November is final, Thursday 5 and Friday 6 are Pending.</summary>
    private static readonly DateTimeOffset Sat7TenOClock = Utc("2026-11-07T10:00:00Z");

    /// <summary>Monday 9 November, 10:00Z: Friday 6 November is final (its grace period ended at 00:00Z), Saturday 7 and Sunday 8 are Pending.</summary>
    private static readonly DateTimeOffset Mon9TenOClock = Utc("2026-11-09T10:00:00Z");

    /// <summary>Monday 23 November, 10:00Z: Friday 20 November is final (its grace period ended at 00:00Z).</summary>
    private static readonly DateTimeOffset Mon23TenOClock = Utc("2026-11-23T10:00:00Z");

    /// <summary>
    /// Tuesday 24 November, 10:00Z: Saturday 21 November is final (its grace period ended at 00:00Z), Sunday 22 and
    /// Monday 23 are Pending and Tuesday 24 is Due.
    /// </summary>
    private static readonly DateTimeOffset Tue24TenOClock = Utc("2026-11-24T10:00:00Z");

    /// <summary>Thursday 26 November, 10:00Z: the week of Monday 16 November is final (its grace period ended on Wednesday 25 at 00:00Z).</summary>
    private static readonly DateTimeOffset Thu26TenOClock = Utc("2026-11-26T10:00:00Z");

    private readonly ReportWorld _world = new();

    private static GoalTargetFields Reading20MinDaily => DurationDays(20, DurationUnit.Minutes, BuildOrReduce.Build, AllDays);

    private Task<GoalId> GoalAsync(GoalTargetFields target) => _world.CreateGoalAsync(target, Mon2, Mon2Morning);

    /// <summary>
    /// "Reading, 20 min daily" from Monday 2 to Saturday 21 November: 18 days with 20 minutes, Tuesday 10 with 10 minutes
    /// (Failed) and Sunday 15 with no record (Missed).
    /// </summary>
    private async Task<GoalId> EighteenMetOneFailedOneMissedAsync()
    {
        var goal = await GoalAsync(Reading20MinDaily);
        for (var date = Mon2; date <= Date(2026, 11, 21); date = date.AddDays(1))
        {
            if (date == Date(2026, 11, 10))
            {
                _world.SeedAmount(goal, date, 10);
            }
            else if (date != Date(2026, 11, 15))
            {
                _world.SeedAmount(goal, date, 20);
            }
        }

        return goal;
    }

    [Fact]
    public async Task US1_AS1_18_Met_1_Failed_and_1_Missed_give_90_percent_of_20()
    {
        var goal = await EighteenMetOneFailedOneMissedAsync();
        _world.At(Tue24TenOClock);

        var stats = (await _world.ReportAsync(goal)).Stats;

        Assert.Equal("90% (18 of 20)", stats.ConsistencyText);
        Assert.Equal(90, stats.Consistency.Percentage);
        Assert.Equal(18, stats.Consistency.Successful);
        Assert.Equal(20, stats.Consistency.Counted);
    }

    [Fact]
    public async Task US1_AS2_2_more_Pending_and_1_Due_results_are_not_counted()
    {
        var goal = await EighteenMetOneFailedOneMissedAsync();
        _world.SeedAmount(goal, Date(2026, 11, 22), 20);
        _world.SeedAmount(goal, Date(2026, 11, 24), 25);
        _world.At(Tue24TenOClock);

        var report = await _world.ReportAsync(goal);

        Assert.Equal("90% (18 of 20)", report.Stats.ConsistencyText);
        Assert.Equal(new Consistency(18, 20), report.Stats.Consistency);
        Assert.Equal(2, report.Recent.Counts[EvaluationState.Pending]);
    }

    [Fact]
    public async Task US1_AS3_a_goal_not_started_has_N_A_and_no_percentage()
    {
        var goal = await _world.CreateGoalAsync(Reading20MinDaily, Date(2026, 11, 20), Utc("2026-11-16T08:00:00Z"));
        _world.At(Utc("2026-11-17T10:00:00Z"));

        var report = await _world.ReportAsync(goal);

        Assert.Equal("N/A", report.Stats.ConsistencyText);
        Assert.Null(report.Stats.Consistency.Percentage);
        Assert.Equal(0, report.Stats.Consistency.Counted);
        Assert.Equal(0, report.Stats.CurrentStreak);
        Assert.Equal(0, report.Stats.LongestStreak);
        Assert.Empty(report.Recent.Results);
        Assert.Empty(report.Trend.Bars);
        Assert.False(report.HasBegunEntry);
    }

    [Fact]
    public async Task US1_AS3_a_goal_with_only_Due_and_Pending_results_has_N_A_and_no_percentage()
    {
        // Created on Monday 16 November; read on Tuesday 17 at 10:00Z, Monday 16 (recorded) is Pending and Tuesday 17 Due.
        var goal = await _world.CreateGoalAsync(Reading20MinDaily, Date(2026, 11, 16), Utc("2026-11-16T08:00:00Z"));
        _world.SeedAmount(goal, Date(2026, 11, 16), 20);
        _world.At(Utc("2026-11-17T10:00:00Z"));

        var report = await _world.ReportAsync(goal);

        Assert.Equal("N/A", report.Stats.ConsistencyText);
        Assert.Null(report.Stats.Consistency.Percentage);
        Assert.Equal(0, report.Stats.Consistency.Counted);
        Assert.True(report.HasBegunEntry);
    }

    [Theory]
    [InlineData(2, 1, "66% (2 of 3)", 66)]
    [InlineData(1, 2, "33% (1 of 3)", 33)]
    [InlineData(199, 1, "99% (199 of 200)", 99)]
    [InlineData(5, 0, "100% (5 of 5)", 100)]
    public async Task US1_AS4_the_percentage_is_rounded_down(int met, int failed, string text, int percentage)
    {
        // From Monday 5 January 2026: the Met days first, then the Failed ones (5 of 20 minutes). Read at 10:00Z two days
        // after the day after the last recorded one, when the last recorded day is final and the later days are Pending or Due.
        var start = Date(2026, 1, 5);
        var goal = await _world.CreateGoalAsync(Reading20MinDaily, start, Utc("2026-01-05T08:00:00Z"));
        for (var day = 0; day < met + failed; day++)
        {
            _world.SeedAmount(goal, start.AddDays(day), day < met ? 20 : 5);
        }

        _world.At(new DateTimeOffset(start.AddDays(met + failed + 2).ToDateTime(new TimeOnly(10, 0)), TimeSpan.Zero));

        var stats = (await _world.ReportAsync(goal)).Stats;

        Assert.Equal(text, stats.ConsistencyText);
        Assert.Equal(percentage, stats.Consistency.Percentage);
        Assert.Equal(met + failed, stats.Consistency.Counted);
    }

    [Fact]
    public async Task US1_AS5_a_Reduce_goals_Met_results_are_successful_and_its_Failed_and_Missed_ones_are_not()
    {
        // At most 2 cups a day: 1 and 2 and 0 are Met, 3 is Failed, Friday 6 with no record is Missed.
        var goal = await GoalAsync(CountDays(2, "cups", BuildOrReduce.Reduce, AllDays));
        _world.SeedAmount(goal, Mon2, 1);
        _world.SeedAmount(goal, Date(2026, 11, 3), 2);
        _world.SeedAmount(goal, Date(2026, 11, 4), 3);
        _world.SeedAmount(goal, Date(2026, 11, 5), 0);
        _world.At(Mon9TenOClock);

        var stats = (await _world.ReportAsync(goal)).Stats;

        Assert.Equal("60% (3 of 5)", stats.ConsistencyText);
        Assert.Equal(new Consistency(3, 5), stats.Consistency);
    }

    [Fact]
    public async Task US1_AS6_a_per_week_goal_counts_each_final_week_once()
    {
        // 3 times a week: 3 "Yes" in the week of 2 November (Met), 1 in the week of 9 (Failed), 4 in the week of 16 (Met).
        var goal = await GoalAsync(YesNoTimes(3, SchedulePeriod.Week));
        foreach (var day in new[] { 2, 3, 4, 10, 16, 17, 18, 19 })
        {
            _world.SeedAnswer(goal, Date(2026, 11, day), YesNoAnswer.Yes);
        }

        _world.At(Thu26TenOClock);

        var report = await _world.ReportAsync(goal);

        Assert.Equal("66% (2 of 3)", report.Stats.ConsistencyText);
        Assert.Equal(StreakUnit.Weeks, report.Stats.Unit);
        Assert.Equal(
            [(Mon2, Date(2026, 11, 8)), (Date(2026, 11, 9), Date(2026, 11, 15)), (Date(2026, 11, 16), Date(2026, 11, 22))],
            report.Recent.Results.Select(result => (result.FirstDate, result.LastDate)));
    }

    [Fact]
    public async Task US1_AS6_a_commitment_based_goal_counts_each_final_commitment_once()
    {
        // Monday, Wednesday and Friday: 9 final commitments from 2 to 20 November; Wednesday 11 "No" (Failed), Friday 13
        // no record (Missed), the others "Yes". Monday 23 is Due.
        var goal = await GoalAsync(YesNoDays(BuildOrReduce.Build, MonWedFri));
        foreach (var day in new[] { 2, 4, 6, 9, 16, 18, 20 })
        {
            _world.SeedAnswer(goal, Date(2026, 11, day), YesNoAnswer.Yes);
        }

        _world.SeedAnswer(goal, Date(2026, 11, 11), YesNoAnswer.No);
        _world.At(Mon23TenOClock);

        var report = await _world.ReportAsync(goal);

        Assert.Equal("77% (7 of 9)", report.Stats.ConsistencyText);
        Assert.Equal(StreakUnit.Commitments, report.Stats.Unit);
        Assert.Equal(9, report.Recent.Results.Count);
    }

    [Fact]
    public async Task US1_AS6_a_per_day_goal_counts_each_final_day_once()
    {
        // 2 times a day: Monday 2 two "Yes" (Met), Tuesday 3 one (Failed), Wednesday 4 three (Met), Thursday 5 none
        // (Missed), Friday 6 two (Met). Saturday 7 and Sunday 8 are Pending, Monday 9 is Due.
        var goal = await GoalAsync(YesNoTimes(2, SchedulePeriod.Day));
        foreach (var day in new[] { 2, 2, 3, 4, 4, 4, 6, 6 })
        {
            _world.SeedAnswer(goal, Date(2026, 11, day), YesNoAnswer.Yes);
        }

        _world.At(Mon9TenOClock);

        var report = await _world.ReportAsync(goal);

        Assert.Equal("60% (3 of 5)", report.Stats.ConsistencyText);
        Assert.Equal(StreakUnit.Days, report.Stats.Unit);
        Assert.Equal(
            [EvaluationState.Met, EvaluationState.Failed, EvaluationState.Met, EvaluationState.Missed, EvaluationState.Met, EvaluationState.Pending, EvaluationState.Pending],
            report.Recent.Results.Select(result => result.State));
    }

    [Fact]
    public async Task US1_AS7_two_goals_on_the_same_habit_each_have_their_own_figures()
    {
        // Both on "Reading": the first with 20 minutes on 2, 3 and 4 November; the second with 5 minutes (Failed), then
        // 20 (Met), then no record (Missed).
        var first = await GoalAsync(Reading20MinDaily);
        var second = await GoalAsync(DurationDays(15, DurationUnit.Minutes, BuildOrReduce.Build, AllDays));
        foreach (var day in new[] { 2, 3, 4 })
        {
            _world.SeedAmount(first, Date(2026, 11, day), 20);
        }

        _world.SeedAmount(second, Mon2, 5);
        _world.SeedAmount(second, Date(2026, 11, 3), 20);
        _world.At(Sat7TenOClock);

        var firstReport = await _world.ReportAsync(first);
        var secondReport = await _world.ReportAsync(second);

        Assert.Equal(first, firstReport.GoalId);
        Assert.Equal("100% (3 of 3)", firstReport.Stats.ConsistencyText);
        Assert.Equal(second, secondReport.GoalId);
        Assert.Equal("33% (1 of 3)", secondReport.Stats.ConsistencyText);
    }

    [Fact]
    public async Task Edge_case_a_goal_whose_catalog_habit_is_then_Unpublished_keeps_the_same_report()
    {
        var goal = await EighteenMetOneFailedOneMissedAsync();
        _world.At(Tue24TenOClock);
        var before = await _world.ReportAsync(goal);

        Assert.True(await _world.CatalogHabits.TrySetStateAsync(_world.Habit.Id, CatalogHabitState.Published, CatalogHabitState.Unpublished, CancellationToken.None));
        var after = await _world.ReportAsync(goal);

        Assert.Equal("90% (18 of 20)", after.Stats.ConsistencyText);
        Assert.Equal(before.Name, after.Name);
        Assert.Equal(before.Stats, after.Stats);
        Assert.Equal(before.Recent.Results, after.Recent.Results);
        Assert.Equal(before.Recent.Counts, after.Recent.Counts);
        Assert.Equal(before.Trend.Bars, after.Trend.Bars);
        Assert.Equal(before.Trend with { Bars = after.Trend.Bars }, after.Trend);
        Assert.True(after.HasBegunEntry);
    }
}
