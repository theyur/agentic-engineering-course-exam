using HT.Application.Reports;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using static HT.UnitTests.Application.Records.RecordWorld;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Reports;

/// <summary>
/// Corrections change every figure on the next read, as if the records had been entered on time (US3-AS1 to AS5;
/// FR-008, FR-009, SC-004), through <see cref="ReportService.GetGoalReportAsync"/> over the fakes of
/// <see cref="ReportWorld"/>. Unless a test says otherwise the user has no stored time zone, so the goals are in UTC,
/// start on Monday 2 November 2026 and are created at 08:00Z that day. A day runs from 00:00Z to 00:00Z the next day and
/// its grace period ends 48 hours later. "Reading, 20 min daily" is commitment-based. Every instant is worked out by hand.
/// </summary>
public sealed class GoalReportCorrectionTests
{
    private static readonly DateOnly Mon2 = Date(2026, 11, 2);
    private static readonly DateOnly Wed4 = Date(2026, 11, 4);
    private static readonly DateOnly Thu5 = Date(2026, 11, 5);
    private static readonly DateTimeOffset Mon2Morning = Utc("2026-11-02T08:00:00Z");

    /// <summary>
    /// Tuesday 10 November, 10:00Z: Saturday 7 is final (its grace period ended at 00:00Z), Sunday 8 and Monday 9 are
    /// Pending and Tuesday 10 is Due.
    /// </summary>
    private static readonly DateTimeOffset Tue10TenOClock = Utc("2026-11-10T10:00:00Z");

    private readonly ReportWorld _world = new();

    private static GoalTargetFields Reading20MinDaily => DurationDays(20, DurationUnit.Minutes, BuildOrReduce.Build, AllDays);

    private Task<GoalId> GoalAsync() => _world.CreateGoalAsync(Reading20MinDaily, Mon2, Mon2Morning);

    private static void AssertFigures(GoalReport report, string consistency, int current, int longest)
    {
        Assert.Equal(consistency, report.Stats.ConsistencyText);
        Assert.Equal(current, report.Stats.CurrentStreak);
        Assert.Equal(longest, report.Stats.LongestStreak);
    }

    private static EvaluationState StateOn(GoalReport report, DateOnly date) =>
        report.Recent.Results.Single(result => result.FirstDate == date).State;

    /// <summary>20 minutes on Monday 2 to Saturday 7 November; returns the record of Wednesday 4.</summary>
    private async Task<(GoalId Goal, RecordId Wednesday4)> SixMetDaysAsync()
    {
        var goal = await GoalAsync();
        RecordId? wednesday4 = null;
        for (var date = Mon2; date <= Date(2026, 11, 7); date = date.AddDays(1))
        {
            var record = _world.SeedAmount(goal, date, 20);
            if (date == Wed4)
            {
                wednesday4 = record;
            }
        }

        return (goal, wednesday4!.Value);
    }

    [Fact]
    public async Task US3_AS1_a_Missed_day_corrected_with_a_record_that_reaches_the_target_joins_the_runs()
    {
        // 20 minutes on 2, 3 and 4 November, no record on Thursday 5 (Missed), 20 minutes on 6 and 7.
        var goal = await GoalAsync();
        foreach (var day in new[] { 2, 3, 4, 6, 7 })
        {
            _world.SeedAmount(goal, Date(2026, 11, day), 20);
        }

        _world.At(Tue10TenOClock);
        var before = await _world.ReportAsync(goal);

        Assert.Equal(EvaluationState.Missed, StateOn(before, Thu5));
        AssertFigures(before, "83% (5 of 6)", 2, 3);

        await _world.RecordAsync(goal, Thu5, Minutes(20));
        var after = await _world.ReportAsync(goal);

        Assert.Equal(EvaluationState.Met, StateOn(after, Thu5));
        AssertFigures(after, "100% (6 of 6)", 6, 6);
        Assert.Equal(new Progress(ProgressKind.RecordedValue, 20, 20), after.Recent.Results.Single(result => result.FirstDate == Thu5).Progress);
    }

    [Fact]
    public async Task US3_AS2_a_Met_commitment_changed_below_the_target_counts_as_Failed_and_breaks_the_streak_at_its_date()
    {
        var (goal, wednesday4) = await SixMetDaysAsync();
        _world.At(Tue10TenOClock);
        AssertFigures(await _world.ReportAsync(goal), "100% (6 of 6)", 6, 6);

        await _world.ChangeAsync(goal, wednesday4, Minutes(10));
        var after = await _world.ReportAsync(goal);

        Assert.Equal(EvaluationState.Failed, StateOn(after, Wed4));
        AssertFigures(after, "83% (5 of 6)", 3, 3);
        Assert.Equal(1, after.Recent.Counts[EvaluationState.Failed]);
    }

    [Fact]
    public async Task US3_AS3_a_final_commitment_whose_only_record_is_deleted_counts_as_Missed()
    {
        var (goal, wednesday4) = await SixMetDaysAsync();
        _world.At(Tue10TenOClock);

        await _world.DeleteAsync(goal, wednesday4);
        var after = await _world.ReportAsync(goal);

        Assert.Equal(EvaluationState.Missed, StateOn(after, Wed4));
        AssertFigures(after, "83% (5 of 6)", 3, 3);
        Assert.Equal(1, after.Recent.Counts[EvaluationState.Missed]);
    }

    [Fact]
    public async Task US3_AS4_a_goal_corrected_later_has_the_same_report_as_one_recorded_on_time()
    {
        // Both goals "Reading, 20 min daily" from Monday 2 November. Each day from 2 to 9 November at 10:00Z the first goal
        // gets its final value: 20, 12, 25, 20, 20, 5 and 20 minutes, and nothing on Monday 9. The second gets 20 (not 12)
        // on the 3rd, 10 (not 25) on the 4th, nothing on the 5th, 30 (not 20) on the 6th and 20 (not nothing) on the 9th,
        // the same values as the first on the other days. On Sunday 15 at 10:00Z, when 2 to 12 November are final, the
        // second goal is corrected to the first one's values: the 3rd and the 6th changed, the 4th recorded again (a
        // commitment's one record is replaced in place), the 5th recorded late and the 9th's record deleted.
        int?[] onTime = [20, 12, 25, 20, 20, 5, 20, null];
        int?[] first = [20, 20, 10, null, 30, 5, 20, 20];
        var recordedOnTime = await GoalAsync();
        var corrected = await GoalAsync();
        var records = new Dictionary<DateOnly, RecordId>();
        for (var day = 0; day < onTime.Length; day++)
        {
            var date = Mon2.AddDays(day);
            _world.At(new DateTimeOffset(date.ToDateTime(new TimeOnly(10, 0)), TimeSpan.Zero));
            if (onTime[day] is { } value)
            {
                await _world.RecordAsync(recordedOnTime, date, Minutes(value));
            }

            if (first[day] is { } wrong)
            {
                records[date] = await _world.RecordAsync(corrected, date, Minutes(wrong));
            }
        }

        _world.At(Utc("2026-11-15T10:00:00Z"));
        await _world.ChangeAsync(corrected, records[Date(2026, 11, 3)], Minutes(12));
        await _world.RecordAsync(corrected, Wed4, Minutes(25));
        await _world.RecordAsync(corrected, Thu5, Minutes(20));
        await _world.ChangeAsync(corrected, records[Date(2026, 11, 6)], Minutes(20));
        await _world.DeleteAsync(corrected, records[Date(2026, 11, 9)]);

        var expected = await _world.ReportAsync(recordedOnTime);
        var actual = await _world.ReportAsync(corrected);

        Assert.Equal("45% (5 of 11)", expected.Stats.ConsistencyText);
        Assert.Equal(expected.Stats, actual.Stats);
        Assert.Equal(expected.Recent.Results, actual.Recent.Results);
        Assert.Equal(expected.Recent.Counts, actual.Recent.Counts);
        Assert.Equal(expected.Trend.Bars, actual.Trend.Bars);
        Assert.Equal(expected.Trend, actual.Trend with { Bars = expected.Trend.Bars });
        Assert.Equal(expected with { GoalId = actual.GoalId, Name = actual.Name, Stats = actual.Stats, Recent = actual.Recent, Trend = actual.Trend }, actual);
    }

    [Fact]
    public async Task US3_AS5_after_a_time_zone_change_each_result_counts_with_its_own_version_in_date_order_and_an_overlap_counts_both()
    {
        // Version 1 in New York (UTC−5) from Monday 16 November, version 2 in Tokyo (UTC+9) from Wednesday 18, a move east.
        // Monday 16 and Tuesday 17 belong to version 1: Tuesday 17 runs from 2026-11-17T05:00Z to 2026-11-18T05:00Z and its
        // grace period ends at 2026-11-20T05:00Z (in Tokyo it would have ended at 2026-11-19T15:00Z). Wednesday 18 runs
        // from 2026-11-17T15:00Z to 2026-11-18T15:00Z, overlapping Tuesday 17, and its grace period ends at
        // 2026-11-20T15:00Z; Thursday 19's ends at 2026-11-21T15:00Z. 20 minutes on 16, 17 and 19, nothing on Wednesday 18.
        var mon16 = Date(2026, 11, 16);
        var goal = _world.AddGoal(_world.User, Target(Reading20MinDaily), (mon16, NewYork), (Date(2026, 11, 18), Tokyo));
        foreach (var day in new[] { 16, 17, 19 })
        {
            _world.SeedAmount(goal, Date(2026, 11, day), 20);
        }

        // At 04:00Z on Friday 20 Tuesday 17 is still Pending by its own version's grace end; Friday 20 in Tokyo is Due.
        _world.At(Utc("2026-11-20T04:00:00Z"));
        var before = await _world.ReportAsync(goal);

        Assert.Equal(
            [(mon16, EvaluationState.Met), (Date(2026, 11, 17), EvaluationState.Pending), (Date(2026, 11, 18), EvaluationState.Pending), (Date(2026, 11, 19), EvaluationState.Pending)],
            before.Recent.Results.Select(result => (result.FirstDate, result.State)));
        AssertFigures(before, "100% (1 of 1)", 1, 1);

        // At 16:00Z on Saturday 21 Tuesday 17 is Met, Wednesday 18 Missed and Thursday 19 Met; Friday 20 and Saturday 21 are
        // Pending and Sunday 22 is Due.
        _world.At(Utc("2026-11-21T16:00:00Z"));
        var after = await _world.ReportAsync(goal);

        Assert.Equal(
            [
                (mon16, EvaluationState.Met), (Date(2026, 11, 17), EvaluationState.Met), (Date(2026, 11, 18), EvaluationState.Missed),
                (Date(2026, 11, 19), EvaluationState.Met), (Date(2026, 11, 20), EvaluationState.Pending), (Date(2026, 11, 21), EvaluationState.Pending),
            ],
            after.Recent.Results.Select(result => (result.FirstDate, result.State)));
        AssertFigures(after, "75% (3 of 4)", 1, 2);
    }
}
