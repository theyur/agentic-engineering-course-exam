using HT.Application.Accounts;
using HT.Application.Goals;
using HT.Application.Reports;
using HT.Domain.Accounts;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Goals;

/// <summary>
/// Slice 8 (T073; FR-008, SC-003, SC-005, US2-AS11, US3-AS13): the boundaries of an ending through
/// <see cref="GoalChangeService.EndAsync"/> on <see cref="AttemptWorld"/>: on the start date, before it and later; a
/// per-week goal ended on a Wednesday, a Sunday and a Monday; with a waiting later version and on the day a change took
/// effect; and in Kyiv with a time zone change on its way. Each ending is checked against the figures read at the same
/// instant just before it: no result of the commitment or period in progress, or later, is counted, and no Failed or
/// Missed is added. The user has no stored zone unless a test says otherwise. Every instant is a UTC literal worked out
/// by hand; dates are November 2026.
/// </summary>
public sealed class EndGoalBoundaryTests
{
    private const string KyivId = "Europe/Kyiv";

    private static readonly CancellationToken None = CancellationToken.None;

    private static readonly DateOnly Sun1 = Date(2026, 11, 1);
    private static readonly DateOnly Mon2 = Date(2026, 11, 2);
    private static readonly DateOnly Wed4 = Date(2026, 11, 4);
    private static readonly DateOnly Tue10 = Date(2026, 11, 10);
    private static readonly DateOnly Wed11 = Date(2026, 11, 11);
    private static readonly DateOnly Thu12 = Date(2026, 11, 12);
    private static readonly DateOnly Sun15 = Date(2026, 11, 15);
    private static readonly DateOnly Mon16 = Date(2026, 11, 16);
    private static readonly DateOnly Tue17 = Date(2026, 11, 17);
    private static readonly DateOnly Wed18 = Date(2026, 11, 18);
    private static readonly DateOnly Sun22 = Date(2026, 11, 22);
    private static readonly DateOnly Mon23 = Date(2026, 11, 23);

    private static readonly DateTimeOffset Created = Utc("2026-11-01T10:00:00Z");

    private readonly AttemptWorld _world = new();

    private static GoalTargetFields MinutesDaily(int minutes) => DurationDays(minutes, DurationUnit.Minutes, BuildOrReduce.Build, AllDays);

    private static GoalTargetFields YogaThreeTimesAWeek => YesNoTimes(3, SchedulePeriod.Week);

    /// <summary>
    /// Ends the goal for <paramref name="user"/> (default the world's user) at the clock's time, expecting "goal ended" with
    /// <paramref name="endingDate"/> and <paramref name="lastDate"/>, and checks SC-003 against the report read at the
    /// same instant just before: the figures are the same and the ended results hold no more Failed or Missed.
    /// </summary>
    private async Task EndAndCompareAsync(GoalId goal, DateOnly endingDate, DateOnly lastDate, ActingUser? user = null)
    {
        var acting = user ?? _world.User;
        var before = await ReportAsync(acting, goal);

        var result = await _world.Changes.EndAsync(acting, goal, Outcome.Abandoned, null, None);

        Assert.Equal((GoalEndOutcome.Ended, (DateOnly?)endingDate, (DateOnly?)lastDate), (result.Outcome, result.EndingDate, result.LastDate));
        var after = await ReportAsync(acting, goal);
        Assert.Equal(before.Stats, after.Stats);
        Assert.True(FailedOrMissed(after) <= FailedOrMissed(before));
    }

    private async Task<GoalReport> ReportAsync(ActingUser user, GoalId goal)
    {
        var result = await _world.Reports.GetGoalReportAsync(user, goal, None);
        Assert.Equal(GoalReportOutcome.Found, result.Outcome);
        return result.Report!;
    }

    private static int FailedOrMissed(GoalReport report) =>
        report.Recent.Results.Count(r => r.State is EvaluationState.Failed or EvaluationState.Missed);

    [Fact]
    public async Task SC_005_an_ending_on_the_start_date_leaves_the_attempt_with_no_date()
    {
        var goal = await _world.CreateGoalAsync(MinutesDaily(10), Mon2, Created);
        _world.At(Utc("2026-11-02T10:00:00Z"));

        await EndAndCompareAsync(goal, Mon2, Sun1);

        var stored = _world.GoalOf(goal);
        Assert.Null(GoalTimeline.AttemptOf(stored, Mon2));
        Assert.Equal([Mon2], stored.Versions.Select(v => v.AppliesFrom));
        _world.At(Utc("2026-11-10T12:00:00Z"));
        Assert.Equal(("N/A", false), ((await _world.ReportAsync(goal)).Stats.ConsistencyText, (await _world.ReportAsync(goal)).HasBegunEntry));
        Assert.False((await _world.DetailsAsync(goal)).AttemptHasBegun);
    }

    [Fact]
    public async Task SC_005_an_ending_before_the_start_date_uses_the_accounts_today()
    {
        var goal = await _world.CreateGoalAsync(MinutesDaily(10), Wed4, Created);
        _world.At(Utc("2026-11-02T10:00:00Z"));

        await EndAndCompareAsync(goal, Mon2, Sun1);

        Assert.Null(GoalTimeline.AttemptOf(_world.GoalOf(goal), Wed4));
        _world.At(Utc("2026-11-10T12:00:00Z"));
        Assert.Equal("N/A", (await _world.ReportAsync(goal)).Stats.ConsistencyText);
    }

    [Fact]
    public async Task SC_005_a_later_ending_keeps_the_dates_up_to_the_day_before_and_counts_nothing_from_the_ending_date()
    {
        var goal = await _world.CreateGoalAsync(MinutesDaily(10), Mon2, Created);
        _world.SeedAmount(goal, Mon2, 10);
        _world.SeedAmount(goal, Wed4, 5);
        _world.At(Utc("2026-11-04T23:59:59Z"));

        // Wednesday 4 is Due, its 5 minutes below the target: ending now leaves it out, so it never becomes Failed.
        await EndAndCompareAsync(goal, Wed4, Date(2026, 11, 3));

        _world.At(Utc("2026-11-10T12:00:00Z"));
        var report = await _world.ReportAsync(goal);
        Assert.Equal("50% (1 of 2)", report.Stats.ConsistencyText);
        Assert.Equal([EvaluationState.Met, EvaluationState.Missed], report.Recent.Results.Select(r => r.State));
        Assert.Contains(_world.Records.RecordsOfGoal(goal), r => r.Date == Wed4);
    }

    [Fact]
    public async Task US2_AS11_a_per_week_goal_ended_on_a_Wednesday_leaves_the_week_in_progress_out_and_keeps_its_records()
    {
        var goal = await _world.CreateGoalAsync(YogaThreeTimesAWeek, Mon2, Created);
        _world.SeedAnswer(goal, Mon16, YesNoAnswer.Yes);
        _world.SeedAnswer(goal, Tue17, YesNoAnswer.Yes);
        _world.At(Utc("2026-11-18T10:00:00Z"));

        await EndAndCompareAsync(goal, Wed18, Sun15);

        // Read on Wednesday 25 at 12:00, after the grace period of the week of 16 November: had it stayed in the
        // attempt, its two of three would be Failed. It is not counted, so the figures cover the two Missed weeks only.
        _world.At(Utc("2026-11-25T12:00:00Z"));
        var report = await _world.ReportAsync(goal);
        Assert.Equal("0% (0 of 2)", report.Stats.ConsistencyText);
        Assert.Equal([EvaluationState.Missed, EvaluationState.Missed], report.Recent.Results.Select(r => r.State));
        Assert.Null(GoalTimeline.AttemptOf(_world.GoalOf(goal), Mon16));
        Assert.Equal([Mon16, Tue17], _world.Records.RecordsOfGoal(goal).Select(r => r.Date));
    }

    [Fact]
    public async Task SC_005_a_per_week_goal_ended_on_a_Sunday_leaves_that_whole_week_out()
    {
        var goal = await _world.CreateGoalAsync(YogaThreeTimesAWeek, Mon2, Created);
        _world.At(Utc("2026-11-22T23:59:59Z"));

        await EndAndCompareAsync(goal, Sun22, Sun15);

        Assert.Null(GoalTimeline.AttemptOf(_world.GoalOf(goal), Sun22));
    }

    [Fact]
    public async Task SC_005_a_per_week_goal_ended_on_a_Monday_keeps_the_week_before_which_is_Pending_until_its_grace_ends()
    {
        var goal = await _world.CreateGoalAsync(YogaThreeTimesAWeek, Mon2, Created);
        _world.At(Utc("2026-11-23T00:00:00Z"));

        await EndAndCompareAsync(goal, Mon23, Sun22);

        Assert.Equal(1, GoalTimeline.AttemptOf(_world.GoalOf(goal), Sun22));
        Assert.Null(GoalTimeline.AttemptOf(_world.GoalOf(goal), Mon23));
        _world.At(Utc("2026-11-25T00:00:00Z"));
        Assert.Equal("0% (0 of 3)", (await _world.ReportAsync(goal)).Stats.ConsistencyText);
    }

    [Fact]
    public async Task SC_005_an_ending_with_a_waiting_later_version_removes_it()
    {
        var goal = await _world.CreateGoalAsync(MinutesDaily(15), Mon2, Created);
        _world.At(Utc("2026-11-11T10:00:00Z"));
        Assert.Equal(Thu12, await _world.ChangeSettingsAsync(goal, MinutesDaily(20)));
        _world.At(Utc("2026-11-11T11:00:00Z"));

        await EndAndCompareAsync(goal, Wed11, Tue10);

        var stored = _world.GoalOf(goal);
        Assert.Equal([(Mon2, 15)], stored.Versions.Select(v => (v.AppliesFrom, v.Settings.Target.Amount)));
        Assert.Equal("Reading, 15 min daily", (await _world.DetailsAsync(goal)).Name);
    }

    [Fact]
    public async Task SC_005_an_ending_on_the_day_a_change_took_effect_removes_that_version_and_the_name_and_settings_come_from_the_last_one()
    {
        var goal = await _world.CreateGoalAsync(MinutesDaily(15), Mon2, Created);
        _world.At(Utc("2026-11-11T10:00:00Z"));
        Assert.Equal(Thu12, await _world.ChangeSettingsAsync(goal, MinutesDaily(20)));
        _world.At(Utc("2026-11-12T10:00:00Z"));
        Assert.Equal("Reading, 20 min daily", (await _world.DetailsAsync(goal)).Name);

        await EndAndCompareAsync(goal, Thu12, Wed11);

        Assert.Equal([(Mon2, 15)], _world.GoalOf(goal).Versions.Select(v => (v.AppliesFrom, v.Settings.Target.Amount)));
        var details = await _world.DetailsAsync(goal);
        Assert.Equal(("Reading, 15 min daily", (int?)15, Mon2), (details.Name, details.Amount, details.TimeZoneAppliesFrom));
    }

    [Fact]
    public async Task SC_005_an_ending_on_the_start_date_keeps_the_first_version_even_when_a_change_replaced_it()
    {
        var goal = await _world.CreateGoalAsync(MinutesDaily(15), Wed4, Created);
        _world.At(Utc("2026-11-02T10:00:00Z"));
        Assert.Equal(Wed4, await _world.ChangeSettingsAsync(goal, MinutesDaily(20)));
        _world.At(Utc("2026-11-04T10:00:00Z"));

        await EndAndCompareAsync(goal, Wed4, Date(2026, 11, 3));

        Assert.Equal([(Wed4, 20)], _world.GoalOf(goal).Versions.Select(v => (v.AppliesFrom, v.Settings.Target.Amount)));
        Assert.Equal("Reading, 20 min daily", (await _world.DetailsAsync(goal)).Name);
    }

    [Fact]
    public async Task US3_AS13_an_ending_in_Kyiv_after_a_change_to_UTC_ends_on_the_goals_current_date_and_removes_the_waiting_zone_version()
    {
        var user = _world.UserIn(KyivId);
        var goal = await _world.CreateGoalAsync(MinutesDaily(10), Mon2, Created, user);
        // Monday 16 November at 23:30Z is Tuesday 17 at 01:30 in Kyiv.
        _world.At(Utc("2026-11-16T23:30:00Z"));
        Assert.Equal(TimeZoneChangeOutcome.Changed, (await _world.Profiles.ChangeTimeZoneAsync(user, "UTC", None)).Outcome);
        Assert.Equal(2, _world.GoalOf(goal).Versions.Count);
        _world.At(Utc("2026-11-16T23:40:00Z"));

        await EndAndCompareAsync(goal, Tue17, Mon16, user);

        Assert.Equal([(Mon2, KyivId)], _world.GoalOf(goal).Versions.Select(v => (v.AppliesFrom, v.Settings.TimeZone.Id)));
    }
}
