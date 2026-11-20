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
/// The strict streaks of a goal through <see cref="ReportService.GetGoalReportAsync"/> (US2-AS1 to AS7; FR-003, FR-004,
/// FR-008, D-1), over the fakes of <see cref="ReportWorld"/>. The user has no stored time zone, so the goals are in UTC.
/// Unless a test says otherwise a goal starts on Monday 2 November 2026 and is created at 08:00Z that day. A day runs
/// from 00:00Z to 00:00Z the next day and its grace period ends 48 hours later, so Tuesday 10 November's ends at
/// 2026-11-13T00:00:00Z; a week runs from Monday 00:00Z and its grace period ends on the Wednesday after it, at 00:00Z.
/// "Reading, 20 min daily" (every day of the week) is commitment-based, so its unit is commitments; a goal of 1 time a
/// day is per-day (days) and one of N times a week per-week (weeks). Every instant is worked out by hand.
/// </summary>
public sealed class GoalReportStreakTests
{
    private static readonly DateOnly Mon2 = Date(2026, 11, 2);
    private static readonly DateTimeOffset Mon2Morning = Utc("2026-11-02T08:00:00Z");

    /// <summary>Sunday 8 November, the start of the D-1 goals, and 08:00Z that day, when they are created.</summary>
    private static readonly DateOnly Sun8 = Date(2026, 11, 8);
    private static readonly DateTimeOffset Sun8Morning = Utc("2026-11-08T08:00:00Z");

    /// <summary>Tuesday 10 November, the D-1 day.</summary>
    private static readonly DateOnly Tue10 = Date(2026, 11, 10);

    /// <summary>The last second of Tuesday 10 November's grace period: Monday 9 is final, Tuesday 10 still Pending.</summary>
    private static readonly DateTimeOffset JustBeforeTue10GraceEnd = Utc("2026-11-12T23:59:59Z");

    /// <summary>The end of Tuesday 10 November's grace period: it is final from this instant on.</summary>
    private static readonly DateTimeOffset Tue10GraceEnd = Utc("2026-11-13T00:00:00Z");

    private readonly ReportWorld _world = new();

    private static GoalTargetFields Reading20MinDaily => DurationDays(20, DurationUnit.Minutes, BuildOrReduce.Build, AllDays);

    private Task<GoalId> GoalAsync(GoalTargetFields target) => _world.CreateGoalAsync(target, Mon2, Mon2Morning);

    private static void AssertStreaks(GoalStatsView stats, StreakUnit unit, int current, string currentText, int longest, string longestText)
    {
        Assert.Equal(unit, stats.Unit);
        Assert.Equal(current, stats.CurrentStreak);
        Assert.Equal(currentText, stats.CurrentStreakText);
        Assert.Equal(longest, stats.LongestStreak);
        Assert.Equal(longestText, stats.LongestStreakText);
    }

    private static EvaluationState StateOn(GoalReport report, DateOnly date) =>
        report.Recent.Results.Single(result => result.FirstDate == date).State;

    [Fact]
    public async Task US2_AS1_Met_Met_Failed_Met_Met_Met_gives_3_and_3_commitments()
    {
        // Monday, Wednesday and Friday: "Yes" on 2 and 4, "No" on Friday 6 (Failed), "Yes" on 9, 11 and 13. Read on
        // Monday 16 at 10:00Z: Friday 13 is final (its grace period ended at 00:00Z), Monday 16 is Due.
        var goal = await GoalAsync(YesNoDays(BuildOrReduce.Build, MonWedFri));
        foreach (var day in new[] { 2, 4, 9, 11, 13 })
        {
            _world.SeedAnswer(goal, Date(2026, 11, day), YesNoAnswer.Yes);
        }

        _world.SeedAnswer(goal, Date(2026, 11, 6), YesNoAnswer.No);
        _world.At(Utc("2026-11-16T10:00:00Z"));

        var report = await _world.ReportAsync(goal);

        Assert.Equal(
            [EvaluationState.Met, EvaluationState.Met, EvaluationState.Failed, EvaluationState.Met, EvaluationState.Met, EvaluationState.Met],
            report.Recent.Results.Select(result => result.State));
        AssertStreaks(report.Stats, StreakUnit.Commitments, 3, "3 commitments", 3, "3 commitments");
    }

    [Fact]
    public async Task US2_independent_test_a_per_day_goal_reads_Streak_3_days_longest_3()
    {
        // Once a day: "Yes" on 2 and 3, "No" on Wednesday 4 (Failed), "Yes" on 5, 6 and 7. Read on Tuesday 10 at 10:00Z:
        // Saturday 7 is final (its grace period ended at 00:00Z), Sunday 8 and Monday 9 are Pending, Tuesday 10 is Due.
        var goal = await GoalAsync(YesNoTimes(1, SchedulePeriod.Day));
        foreach (var day in new[] { 2, 3, 5, 6, 7 })
        {
            _world.SeedAnswer(goal, Date(2026, 11, day), YesNoAnswer.Yes);
        }

        _world.SeedAnswer(goal, Date(2026, 11, 4), YesNoAnswer.No);
        _world.At(Utc("2026-11-10T10:00:00Z"));

        var stats = (await _world.ReportAsync(goal)).Stats;

        AssertStreaks(stats, StreakUnit.Days, 3, "3 days", 3, "3 days");
        Assert.Equal("Streak 3 days · longest 3", ReportText.SummaryLine(new Streaks(stats.CurrentStreak, stats.LongestStreak), stats.Unit));
    }

    [Fact]
    public async Task US2_AS2_Met_11_times_Missed_then_Met_6_times_gives_6_and_11()
    {
        // 20 minutes on 2 to 12 November, no record on Friday 13 (Missed), 20 minutes on 14 to 19. Read on Sunday 22 at
        // 10:00Z: Thursday 19 is final (its grace period ended at 00:00Z), Friday 20 and Saturday 21 are Pending.
        var goal = await GoalAsync(Reading20MinDaily);
        for (var date = Mon2; date <= Date(2026, 11, 19); date = date.AddDays(1))
        {
            if (date != Date(2026, 11, 13))
            {
                _world.SeedAmount(goal, date, 20);
            }
        }

        _world.At(Utc("2026-11-22T10:00:00Z"));

        var report = await _world.ReportAsync(goal);

        Assert.Equal(EvaluationState.Missed, StateOn(report, Date(2026, 11, 13)));
        AssertStreaks(report.Stats, StreakUnit.Commitments, 6, "6 commitments", 11, "11 commitments");
        Assert.Equal("Streak 6 commitments · longest 11", ReportText.SummaryLine(new Streaks(report.Stats.CurrentStreak, report.Stats.LongestStreak), report.Stats.Unit));
    }

    [Theory]
    [InlineData(10, EvaluationState.Failed)]
    [InlineData(null, EvaluationState.Missed)]
    public async Task US2_AS3_a_latest_Failed_or_Missed_result_gives_0_and_keeps_the_longest(int? minutesOnThursday5, EvaluationState thursday5)
    {
        // 20 minutes on 2, 3 and 4 November, then 10 minutes (Failed) or no record (Missed) on Thursday 5. Read on Sunday 8
        // at 10:00Z: Thursday 5 is final (its grace period ended at 00:00Z), Friday 6 and Saturday 7 are Pending.
        var goal = await GoalAsync(Reading20MinDaily);
        foreach (var day in new[] { 2, 3, 4 })
        {
            _world.SeedAmount(goal, Date(2026, 11, day), 20);
        }

        if (minutesOnThursday5 is { } minutes)
        {
            _world.SeedAmount(goal, Date(2026, 11, 5), minutes);
        }

        _world.At(Utc("2026-11-08T10:00:00Z"));

        var report = await _world.ReportAsync(goal);

        Assert.Equal(thursday5, StateOn(report, Date(2026, 11, 5)));
        AssertStreaks(report.Stats, StreakUnit.Commitments, 0, "0 commitments", 3, "3 commitments");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task US2_AS4_Met_Met_Pending_Due_gives_2(bool pendingAndDueReachTheTarget)
    {
        // Monday, Wednesday, Friday and Saturday: "Yes" on 2 and 4; on Friday 6 and Saturday 7 "Yes" or no record. Read on
        // Saturday 7 at 10:00Z: Wednesday 4 is final (its grace period ended at 00:00Z), Friday 6 is Pending and
        // Saturday 7 is Due, so the recent results end with the Pending one.
        var goal = await GoalAsync(YesNoDays(BuildOrReduce.Build, DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday, DayOfWeek.Saturday));
        _world.SeedAnswer(goal, Mon2, YesNoAnswer.Yes);
        _world.SeedAnswer(goal, Date(2026, 11, 4), YesNoAnswer.Yes);
        if (pendingAndDueReachTheTarget)
        {
            _world.SeedAnswer(goal, Date(2026, 11, 6), YesNoAnswer.Yes);
            _world.SeedAnswer(goal, Date(2026, 11, 7), YesNoAnswer.Yes);
        }

        _world.At(Utc("2026-11-07T10:00:00Z"));

        var report = await _world.ReportAsync(goal);

        Assert.Equal([EvaluationState.Met, EvaluationState.Met, EvaluationState.Pending], report.Recent.Results.Select(result => result.State));
        AssertStreaks(report.Stats, StreakUnit.Commitments, 2, "2 commitments", 2, "2 commitments");
    }

    [Fact]
    public async Task US2_AS4_D_1_a_Pending_day_that_reaches_the_target_extends_the_streak_only_at_its_grace_end()
    {
        // From Sunday 8 November: 20 minutes on 8, 9 and 10.
        var goal = await _world.CreateGoalAsync(Reading20MinDaily, Sun8, Sun8Morning);
        foreach (var day in new[] { 8, 9, 10 })
        {
            _world.SeedAmount(goal, Date(2026, 11, day), 20);
        }

        _world.At(JustBeforeTue10GraceEnd);
        var before = await _world.ReportAsync(goal);

        Assert.Equal(EvaluationState.Pending, StateOn(before, Tue10));
        AssertStreaks(before.Stats, StreakUnit.Commitments, 2, "2 commitments", 2, "2 commitments");

        _world.At(Tue10GraceEnd);
        var after = await _world.ReportAsync(goal);

        Assert.Equal(EvaluationState.Met, StateOn(after, Tue10));
        AssertStreaks(after.Stats, StreakUnit.Commitments, 3, "3 commitments", 3, "3 commitments");
    }

    [Fact]
    public async Task US2_AS5_a_Pending_day_without_a_record_resets_the_streak_once_its_grace_period_ends()
    {
        // From Sunday 8 November: 20 minutes on 8 and 9, no record on Tuesday 10; nothing is done between the two reads.
        var goal = await _world.CreateGoalAsync(Reading20MinDaily, Sun8, Sun8Morning);
        _world.SeedAmount(goal, Sun8, 20);
        _world.SeedAmount(goal, Date(2026, 11, 9), 20);

        _world.At(JustBeforeTue10GraceEnd);
        var before = await _world.ReportAsync(goal);

        Assert.Equal(EvaluationState.Pending, StateOn(before, Tue10));
        AssertStreaks(before.Stats, StreakUnit.Commitments, 2, "2 commitments", 2, "2 commitments");

        _world.At(Tue10GraceEnd);
        var after = await _world.ReportAsync(goal);

        Assert.Equal(EvaluationState.Missed, StateOn(after, Tue10));
        AssertStreaks(after.Stats, StreakUnit.Commitments, 0, "0 commitments", 2, "2 commitments");
    }

    /// <summary>
    /// A goal with one result on Monday 2 November (or the week that starts then), Met with a "Yes" or Missed without a
    /// record, read once that result is final: a day's at Thursday 5 at 10:00Z, the week's at Wednesday 11 at 10:00Z.
    /// </summary>
    [Theory]
    [InlineData(StreakUnit.Days, true, "2026-11-05T10:00:00Z", 1, "1 day")]
    [InlineData(StreakUnit.Weeks, true, "2026-11-11T10:00:00Z", 1, "1 week")]
    [InlineData(StreakUnit.Commitments, true, "2026-11-05T10:00:00Z", 1, "1 commitment")]
    [InlineData(StreakUnit.Days, false, "2026-11-05T10:00:00Z", 0, "0 days")]
    [InlineData(StreakUnit.Weeks, false, "2026-11-11T10:00:00Z", 0, "0 weeks")]
    [InlineData(StreakUnit.Commitments, false, "2026-11-05T10:00:00Z", 0, "0 commitments")]
    public async Task US2_AS6_the_unit_follows_the_goal_with_its_singular_for_1(StreakUnit unit, bool yesOnMonday2, string readAt, int streak, string text)
    {
        var goal = await GoalAsync(unit switch
        {
            StreakUnit.Days => YesNoTimes(1, SchedulePeriod.Day),
            StreakUnit.Weeks => YesNoTimes(1, SchedulePeriod.Week),
            _ => YesNoDays(BuildOrReduce.Build, MonWedFri),
        });
        if (yesOnMonday2)
        {
            _world.SeedAnswer(goal, Mon2, YesNoAnswer.Yes);
        }

        _world.At(Utc(readAt));

        var report = await _world.ReportAsync(goal);

        Assert.Equal(yesOnMonday2 ? EvaluationState.Met : EvaluationState.Missed, StateOn(report, Mon2));
        AssertStreaks(report.Stats, unit, streak, text, streak, text);
    }

    [Fact]
    public async Task US2_AS6_more_than_1_is_plural_in_every_unit()
    {
        // Once a week: "Yes" in the weeks of 2 and 9 November. Read on Wednesday 18 at 10:00Z: the week of 9 November is
        // final (its grace period ended at 00:00Z), the week of 16 November is Due.
        var goal = await GoalAsync(YesNoTimes(1, SchedulePeriod.Week));
        _world.SeedAnswer(goal, Mon2, YesNoAnswer.Yes);
        _world.SeedAnswer(goal, Date(2026, 11, 10), YesNoAnswer.Yes);
        _world.At(Utc("2026-11-18T10:00:00Z"));

        var stats = (await _world.ReportAsync(goal)).Stats;

        AssertStreaks(stats, StreakUnit.Weeks, 2, "2 weeks", 2, "2 weeks");
    }

    /// <summary>
    /// A goal whose only begun result, Monday 2 November (or its week), already has a "Yes" but is not final yet: read on
    /// Tuesday 3 at 10:00Z, a day's is Pending and the week's Due.
    /// </summary>
    [Theory]
    [InlineData(StreakUnit.Days, "0 days")]
    [InlineData(StreakUnit.Weeks, "0 weeks")]
    [InlineData(StreakUnit.Commitments, "0 commitments")]
    public async Task US2_AS7_a_goal_with_no_final_result_has_0_and_0_in_its_unit(StreakUnit unit, string text)
    {
        var goal = await GoalAsync(unit switch
        {
            StreakUnit.Days => YesNoTimes(1, SchedulePeriod.Day),
            StreakUnit.Weeks => YesNoTimes(1, SchedulePeriod.Week),
            _ => YesNoDays(BuildOrReduce.Build, MonWedFri),
        });
        _world.SeedAnswer(goal, Mon2, YesNoAnswer.Yes);
        _world.At(Utc("2026-11-03T10:00:00Z"));

        var report = await _world.ReportAsync(goal);

        Assert.True(report.HasBegunEntry);
        Assert.Equal(0, report.Stats.Consistency.Counted);
        AssertStreaks(report.Stats, unit, 0, text, 0, text);
    }
}
