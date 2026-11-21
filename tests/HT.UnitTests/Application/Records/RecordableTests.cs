using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using static HT.UnitTests.Application.Records.RecordWorld;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Records;

/// <summary>
/// <c>GetRecordableAsync</c> (US5-AS3, FR-011, research R-04): the Due entry and the Pending ones, newest first by first
/// date, each as the history gives it. The user has no stored time zone, so the goals are in UTC; the overlap case is
/// New York (UTC−5) and Tokyo (UTC+9) in November 2026. Every instant is worked out by hand.
/// </summary>
public sealed class RecordableTests
{
    private static readonly DateOnly Mon16 = Date(2026, 11, 16);
    private static readonly DateOnly Tue17 = Date(2026, 11, 17);
    private static readonly DateOnly Wed18 = Date(2026, 11, 18);
    private static readonly DateOnly Thu19 = Date(2026, 11, 19);
    private static readonly DateOnly Sun22 = Date(2026, 11, 22);
    private static readonly DateOnly Mon23 = Date(2026, 11, 23);

    private static readonly DateTimeOffset Mon16Morning = Utc("2026-11-16T08:00:00Z");

    private readonly RecordWorld _world = new();

    private static GoalTargetFields Reading20MinDaily => DurationDays(20, DurationUnit.Minutes, BuildOrReduce.Build, AllDays);

    private static GoalTargetFields YogaThreeTimesAWeek => YesNoTimes(3, SchedulePeriod.Week);

    private async Task<GoalHistory> FoundAsync(GoalId goal, ActingUser? user = null)
    {
        var result = await _world.RecordableAsync(goal, user);
        Assert.Equal(RecordableOutcome.Found, result.Outcome);
        Assert.Null(result.Message);
        return result.History!;
    }

    [Fact]
    public async Task FR_011_the_Due_commitment_and_the_Pending_ones_newest_first()
    {
        var goal = await _world.CreateGoalAsync(Reading20MinDaily, Mon16, Mon16Morning);
        _world.At(Utc("2026-11-18T10:00:00Z"));

        var history = await FoundAsync(goal);

        Assert.Equal(Wed18, history.CurrentDate);
        Assert.Equal(Utc("2026-11-18T10:00:00Z"), history.ReadAt);
        Assert.Equal([Wed18, Tue17, Mon16], history.Entries.Select(e => e.FirstDate));
        Assert.Equal(
            [EvaluationState.Due, EvaluationState.Pending, EvaluationState.Pending],
            history.Entries.Select(e => e.State));
        Assert.Equal(
            [Utc("2026-11-21T00:00:00Z"), Utc("2026-11-20T00:00:00Z"), Utc("2026-11-19T00:00:00Z")],
            history.Entries.Select(e => e.GraceEnds!.Value));
        Assert.All(history.Entries, e => Assert.False(e.IsCorrection));
    }

    [Fact]
    public async Task FR_011_a_commitment_whose_grace_period_has_ended_is_no_longer_offered()
    {
        var goal = await _world.CreateGoalAsync(Reading20MinDaily, Mon16, Mon16Morning);
        _world.At(Utc("2026-11-19T00:00:00Z"));

        var history = await FoundAsync(goal);

        Assert.Equal([Thu19, Wed18, Tue17], history.Entries.Select(e => e.FirstDate));
    }

    [Fact]
    public async Task FR_011_an_entry_carries_its_records_progress_and_note()
    {
        var goal = await _world.CreateGoalAsync(Reading20MinDaily, Mon16, Mon16Morning);
        _world.At(Utc("2026-11-17T10:00:00Z"));
        await _world.RecordAsync(goal, Mon16, Minutes(12, "Back was tired."));

        var history = await FoundAsync(goal);

        var monday = Assert.Single(history.Entries, e => e.FirstDate == Mon16);
        Assert.Equal(12, Assert.Single(monday.Records).Amount);
        Assert.Equal(new Progress(ProgressKind.RecordedValue, 12, 20), monday.Progress);
        Assert.Equal("Back was tired.", monday.Note);
        Assert.Equal(EvaluationState.Pending, monday.State);
    }

    [Fact]
    public async Task FR_011_on_an_unscheduled_day_of_a_fixed_days_goal_there_is_no_Due_commitment()
    {
        var goal = await _world.CreateGoalAsync(YesNoDays(BuildOrReduce.Build, MonWedFri), Mon16, Mon16Morning);
        _world.At(Utc("2026-11-17T10:00:00Z"));

        var history = await FoundAsync(goal);

        Assert.Equal(Tue17, history.CurrentDate);
        var entry = Assert.Single(history.Entries);
        Assert.Equal(Mon16, entry.FirstDate);
        Assert.Equal(EvaluationState.Pending, entry.State);
    }

    [Fact]
    public async Task FR_011_a_goal_on_Mondays_has_nothing_to_record_on_its_Thursday()
    {
        var goal = await _world.CreateGoalAsync(YesNoDays(BuildOrReduce.Build, DayOfWeek.Monday), Mon16, Mon16Morning);
        _world.At(Utc("2026-11-19T10:00:00Z"));

        var history = await FoundAsync(goal);

        Assert.Equal(Thu19, history.CurrentDate);
        Assert.Empty(history.Entries);
    }

    [Fact]
    public async Task FR_011_a_goal_before_its_start_has_no_current_date_no_entry_and_no_store_read()
    {
        var goal = await _world.CreateGoalAsync(Reading20MinDaily, Wed18, Mon16Morning);
        _world.At(Utc("2026-11-17T23:59:59Z"));

        var history = await FoundAsync(goal);

        Assert.Null(history.CurrentDate);
        Assert.Empty(history.Entries);
        Assert.Empty(_world.Records.Calls);
    }

    [Fact]
    public async Task FR_011_the_previous_week_is_Pending_until_its_Wednesday_00_00()
    {
        var goal = await _world.CreateGoalAsync(YogaThreeTimesAWeek, Mon16, Mon16Morning);
        _world.At(Utc("2026-11-24T23:59:59Z"));

        var before = await FoundAsync(goal);
        _world.At(Utc("2026-11-25T00:00:00Z"));
        var at = await FoundAsync(goal);

        Assert.Equal([Mon23, Mon16], before.Entries.Select(e => e.FirstDate));
        Assert.Equal([EvaluationState.Due, EvaluationState.Pending], before.Entries.Select(e => e.State));
        Assert.Equal(Sun22, before.Entries[1].LastDate);
        Assert.Equal(Utc("2026-11-25T00:00:00Z"), before.Entries[1].GraceEnds);
        Assert.Equal([Mon23], at.Entries.Select(e => e.FirstDate));
    }

    [Fact]
    public async Task FR_011_two_entries_are_Due_while_the_days_overlap_after_a_move_east()
    {
        // Version 1 in New York from 16 November, version 2 in Tokyo from 18 November. Tuesday 17 runs from 05:00 UTC on
        // the 17th to 05:00 UTC on the 18th; Wednesday 18 starts at 00:00 Tokyo, 15:00 UTC on the 17th. At 20:00 UTC on
        // the 17th both are Due; Monday 16 ended at 05:00 UTC on the 17th and is Pending.
        var goal = _world.AddGoal(_world.User, Target(Reading20MinDaily), (Mon16, NewYork), (Wed18, Tokyo));
        _world.At(Utc("2026-11-17T20:00:00Z"));

        var history = await FoundAsync(goal);

        Assert.Equal(Wed18, history.CurrentDate);
        Assert.Equal([Wed18, Tue17, Mon16], history.Entries.Select(e => e.FirstDate));
        Assert.Equal(
            [EvaluationState.Due, EvaluationState.Due, EvaluationState.Pending],
            history.Entries.Select(e => e.State));
        Assert.Equal(Utc("2026-11-17T15:00:00Z"), history.Entries[0].Starts);
        Assert.Equal(Utc("2026-11-18T05:00:00Z"), history.Entries[1].Ends);
    }

    [Fact]
    public async Task FR_011_a_per_week_goal_in_its_first_partial_week_has_a_current_date_but_no_entry_and_no_store_read()
    {
        var goal = await _world.CreateGoalAsync(YogaThreeTimesAWeek, Wed18, Utc("2026-11-18T08:00:00Z"));
        _world.At(Utc("2026-11-19T10:00:00Z"));

        var history = await FoundAsync(goal);

        Assert.Equal(Thu19, history.CurrentDate);
        Assert.Empty(history.Entries);
        Assert.Empty(_world.Records.Calls);
    }

    [Fact]
    public async Task FR_011_the_history_carries_the_goals_settings_and_no_name()
    {
        var goal = await _world.CreateGoalAsync(CountTimes(20, "pages", 3, SchedulePeriod.Week), Mon16, Mon16Morning);
        _world.At(Utc("2026-11-17T10:00:00Z"));

        var history = await FoundAsync(goal);

        Assert.Equal(goal, history.GoalId);
        Assert.Equal(GoalScheduleKind.PeriodBased, history.GoalScheduleKind);
        Assert.Equal(SchedulePeriod.Week, history.Period);
        Assert.Equal(BuildOrReduce.Build, history.BuildOrReduce);
        Assert.Equal(Measurement.Count, history.Measurement);
        Assert.Equal(20, history.Amount);
        Assert.Equal("pages", history.CountUnit);
        Assert.Equal(3, history.Times);
        Assert.Equal(Target(CountTimes(20, "pages", 3, SchedulePeriod.Week)), history.Target);
        var week = Assert.Single(history.Entries);
        Assert.Equal(new Progress(ProgressKind.RecordsReachingAmount, 0, 3), week.Progress);
    }

    [Fact]
    public async Task FR_018_the_refusals_come_in_order_and_carry_no_history()
    {
        var goal = await _world.CreateGoalAsync(Reading20MinDaily, Mon16, Mon16Morning);

        var anonymous = await _world.RecordableAsync(goal, ActingUser.Anonymous);
        var administrator = await _world.RecordableAsync(goal, _world.Administrator);
        var other = await _world.RecordableAsync(goal, _world.OtherUser);
        var unknown = await _world.RecordableAsync(new GoalId(Guid.NewGuid()));

        Assert.Equal((RecordableOutcome.NotSignedIn, "You are not signed in."), (anonymous.Outcome, anonymous.Message));
        Assert.Equal((RecordableOutcome.NotForAdministrators, "Goals are for registered users."), (administrator.Outcome, administrator.Message));
        Assert.Equal((RecordableOutcome.NotFound, "This goal does not exist."), (other.Outcome, other.Message));
        Assert.Equal((RecordableOutcome.NotFound, "This goal does not exist."), (unknown.Outcome, unknown.Message));
        Assert.All(new[] { anonymous, administrator, other, unknown }, r => Assert.Null(r.History));
        Assert.Empty(_world.Records.Calls);
    }

    [Fact]
    public async Task FR_011_one_store_read_covers_the_whole_window()
    {
        var goal = await _world.CreateGoalAsync(Reading20MinDaily, Mon16, Mon16Morning);
        _world.At(Utc("2026-11-18T10:00:00Z"));

        await FoundAsync(goal);

        Assert.Equal([nameof(IRecordStore.ReadExemptionsAsync), nameof(IRecordStore.ReadAsync)], _world.Records.Calls);
    }

    [Fact]
    public async Task FR_011_the_records_of_an_entry_outside_the_window_are_not_mixed_in()
    {
        var goal = await _world.CreateGoalAsync(Reading20MinDaily, Mon16, Mon16Morning);
        _world.Records.Seed(goal, Mon16, RecordValue.FromAmount(30));
        _world.At(Utc("2026-11-17T10:00:00Z"));

        var history = await FoundAsync(goal);

        Assert.Empty(Assert.Single(history.Entries, e => e.FirstDate == Tue17).Records);
        Assert.Single(Assert.Single(history.Entries, e => e.FirstDate == Mon16).Records);
    }
}
