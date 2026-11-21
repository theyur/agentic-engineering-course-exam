using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Records;

/// <summary>
/// <c>GetHistoryAsync</c> (US5-AS1, AS2, AS4, FR-009, FR-010, FR-014, FR-018; research R-07) over the fakes of
/// <see cref="RecordWorld"/>. The user has no stored time zone, so the goals are in UTC: a day runs from 00:00Z to
/// 00:00Z the next day, and its grace period ends 48 hours after that. Records and notes are seeded straight into the
/// fake store, so the only store call a test sees is the history's own read. Every instant is worked out by hand.
/// </summary>
public sealed class HistoryTests
{
    private const string RangeOutOfBounds = "Choose a range of 1 to 366 days.";
    private const string NotFound = "This goal does not exist.";

    private static readonly DateOnly Mon2 = Date(2026, 11, 2);
    private static readonly DateOnly Tue3 = Date(2026, 11, 3);
    private static readonly DateOnly Wed4 = Date(2026, 11, 4);
    private static readonly DateOnly Thu5 = Date(2026, 11, 5);
    private static readonly DateOnly Fri6 = Date(2026, 11, 6);
    private static readonly DateOnly Sun8 = Date(2026, 11, 8);
    private static readonly DateOnly Mon9 = Date(2026, 11, 9);
    private static readonly DateOnly Wed11 = Date(2026, 11, 11);
    private static readonly DateOnly Fri13 = Date(2026, 11, 13);
    private static readonly DateOnly Sun15 = Date(2026, 11, 15);
    private static readonly DateOnly Mon16 = Date(2026, 11, 16);
    private static readonly DateOnly Tue17 = Date(2026, 11, 17);
    private static readonly DateOnly Wed18 = Date(2026, 11, 18);
    private static readonly DateOnly Thu19 = Date(2026, 11, 19);
    private static readonly DateOnly Sat21 = Date(2026, 11, 21);
    private static readonly DateOnly Sun22 = Date(2026, 11, 22);
    private static readonly DateOnly Mon23 = Date(2026, 11, 23);
    private static readonly DateOnly Sun29 = Date(2026, 11, 29);

    /// <summary>08:00 UTC on Monday 16 November 2026, when most goals are created.</summary>
    private static readonly DateTimeOffset Mon16Morning = Utc("2026-11-16T08:00:00Z");

    /// <summary>10:00 UTC on Friday 13 November 2026, when the 14 days of Reading are read.</summary>
    private static readonly DateTimeOffset Fri13TenOClock = Utc("2026-11-13T10:00:00Z");

    private readonly RecordWorld _world = new();

    private static GoalTargetFields Reading20MinDaily => DurationDays(20, DurationUnit.Minutes, BuildOrReduce.Build, AllDays);

    private static GoalTargetFields YogaThreeTimesAWeek => YesNoTimes(3, SchedulePeriod.Week);

    private static GoalTargetFields Pages20ThreeTimesAWeek => CountTimes(20, "pages", 3, SchedulePeriod.Week);

    private static GoalTargetFields CoffeeAtMost2CupsADay => CountTimes(2, "cups", 1, SchedulePeriod.Day, BuildOrReduce.Reduce);

    private Task<HistoryResult> HistoryAsync(GoalId goal, DateOnly first, DateOnly last, ActingUser? user = null) =>
        _world.Service.GetHistoryAsync(user ?? _world.User, goal, first, last, CancellationToken.None);

    private async Task<GoalHistory> FoundAsync(GoalId goal, DateOnly first, DateOnly last)
    {
        var result = await HistoryAsync(goal, first, last);
        Assert.Equal(HistoryOutcome.Found, result.Outcome);
        Assert.Null(result.Message);
        return result.History!;
    }

    private static NoteText Note(string text)
    {
        Assert.True(NoteText.TryCreate(text, out var note, out _));
        return note!;
    }

    /// <summary>
    /// Reading, 20 min daily, from Monday 2 November 2026, read from 2 to 15 November at 10:00Z on Friday 13: 2 to 10
    /// November are final (the 10th's grace period ended at 00:00Z on the 13th), the 11th and 12th Pending, the 13th Due,
    /// the 14th and 15th Upcoming.
    /// </summary>
    private async Task<GoalId> FourteenDaysOfReadingAsync()
    {
        var goal = await _world.CreateGoalAsync(Reading20MinDaily, Mon2, Utc("2026-11-02T08:00:00Z"));
        _world.Records.Seed(goal, Mon2, RecordValue.FromAmount(20));
        _world.Records.Seed(goal, Tue3, RecordValue.FromAmount(12));
        _world.Records.Seed(goal, Thu5, RecordValue.FromAmount(25));
        _world.Records.Seed(goal, Fri6, RecordValue.FromAmount(20));
        _world.Records.Seed(goal, Sun8, RecordValue.FromAmount(5));
        _world.Records.Seed(goal, Mon9, RecordValue.FromAmount(30));
        _world.Records.Seed(goal, Wed11, RecordValue.FromAmount(20));
        _world.Records.SeedNote(goal, Wed4, Note("Away for the weekend."));
        _world.At(Fri13TenOClock);
        return goal;
    }

    [Fact]
    public async Task US5_AS1_fourteen_days_give_every_commitment_in_date_order_each_with_its_state()
    {
        var goal = await FourteenDaysOfReadingAsync();

        var history = await FoundAsync(goal, Mon2, Sun15);

        Assert.Equal(Fri13, history.CurrentDate);
        Assert.Equal(Fri13TenOClock, history.ReadAt);
        Assert.Equal(Enumerable.Range(2, 14).Select(day => Date(2026, 11, day)), history.Entries.Select(e => e.FirstDate));
        Assert.All(history.Entries, e => Assert.Equal(e.FirstDate, e.LastDate));
        Assert.Equal(
            [
                EvaluationState.Met, EvaluationState.Failed, EvaluationState.Missed, EvaluationState.Met, EvaluationState.Met,
                EvaluationState.Missed, EvaluationState.Failed, EvaluationState.Met, EvaluationState.Missed,
                EvaluationState.Pending, EvaluationState.Pending, EvaluationState.Due, EvaluationState.Upcoming,
                EvaluationState.Upcoming,
            ],
            history.Entries.Select(e => e.State));
    }

    [Fact]
    public async Task FR_009_each_entry_has_its_start_and_end_instants()
    {
        var goal = await FourteenDaysOfReadingAsync();

        var history = await FoundAsync(goal, Mon2, Sun15);

        Assert.Equal((Utc("2026-11-02T00:00:00Z"), Utc("2026-11-03T00:00:00Z")), (history.Entries[0].Starts, history.Entries[0].Ends));
        Assert.Equal((Utc("2026-11-11T00:00:00Z"), Utc("2026-11-12T00:00:00Z")), (history.Entries[9].Starts, history.Entries[9].Ends));
        Assert.Equal((Utc("2026-11-13T00:00:00Z"), Utc("2026-11-14T00:00:00Z")), (history.Entries[11].Starts, history.Entries[11].Ends));
        Assert.Equal((Utc("2026-11-15T00:00:00Z"), Utc("2026-11-16T00:00:00Z")), (history.Entries[13].Starts, history.Entries[13].Ends));
    }

    [Fact]
    public async Task FR_009_the_grace_end_is_given_only_while_an_entry_is_Due_or_Pending()
    {
        var goal = await FourteenDaysOfReadingAsync();

        var history = await FoundAsync(goal, Mon2, Sun15);

        Assert.Equal(
            [
                null, null, null, null, null, null, null, null, null,
                Utc("2026-11-14T00:00:00Z"), Utc("2026-11-15T00:00:00Z"), Utc("2026-11-16T00:00:00Z"), null, null,
            ],
            history.Entries.Select(e => e.GraceEnds));
    }

    [Fact]
    public async Task FR_009_a_change_now_is_a_correction_only_on_a_final_entry()
    {
        var goal = await FourteenDaysOfReadingAsync();

        var history = await FoundAsync(goal, Mon2, Sun15);

        Assert.Equal(
            [true, true, true, true, true, true, true, true, true, false, false, false, false, false],
            history.Entries.Select(e => e.IsCorrection));
    }

    [Fact]
    public async Task FR_009_each_entry_has_its_records_and_its_note()
    {
        var goal = await FourteenDaysOfReadingAsync();

        var history = await FoundAsync(goal, Mon2, Sun15);

        Assert.Equal(
            [[20], [12], [], [25], [20], [], [5], [30], [], [20], [], [], [], []],
            history.Entries.Select(e => e.Records.Select(r => r.Amount!.Value).ToArray()));
        Assert.All(history.Entries.SelectMany(e => e.Records.Select(r => (e.FirstDate, r.Date))), d => Assert.Equal(d.FirstDate, d.Date));
        Assert.Equal("Away for the weekend.", Assert.Single(history.Entries, e => e.Note is not null).Note);
        Assert.Equal(Wed4, Assert.Single(history.Entries, e => e.Note is not null).FirstDate);
    }

    [Fact]
    public async Task US5_AS2_a_duration_commitment_with_a_record_of_12_is_12_of_20_minutes()
    {
        var goal = await FourteenDaysOfReadingAsync();

        var history = await FoundAsync(goal, Mon2, Sun15);

        Assert.Equal(new Progress(ProgressKind.RecordedValue, 12, 20), history.Entries[1].Progress);
        Assert.Equal(new Progress(ProgressKind.RecordedValue, null, 20), history.Entries[2].Progress);
    }

    [Fact]
    public async Task US5_AS2_a_per_week_Build_period_of_3_with_2_records_that_reach_the_amount_is_2_of_3()
    {
        var goal = await _world.CreateGoalAsync(Pages20ThreeTimesAWeek, Mon16, Mon16Morning);
        _world.Records.Seed(goal, Mon16, RecordValue.FromAmount(20));
        _world.Records.Seed(goal, Tue17, RecordValue.FromAmount(10));
        _world.Records.Seed(goal, Wed18, RecordValue.FromAmount(25));
        _world.At(Utc("2026-11-19T10:00:00Z"));

        var history = await FoundAsync(goal, Mon16, Sun22);

        var week = Assert.Single(history.Entries);
        Assert.Equal((Mon16, Sun22), (week.FirstDate, week.LastDate));
        Assert.Equal(EvaluationState.Due, week.State);
        Assert.Equal(new Progress(ProgressKind.RecordsReachingAmount, 2, 3), week.Progress);
    }

    [Fact]
    public async Task US5_AS2_a_per_day_Reduce_count_period_gives_its_total_and_the_limit()
    {
        var goal = await _world.CreateGoalAsync(CoffeeAtMost2CupsADay, Mon16, Mon16Morning);
        _world.Records.Seed(goal, Mon16, RecordValue.FromAmount(1));
        _world.Records.Seed(goal, Mon16, RecordValue.FromAmount(2));
        _world.At(Utc("2026-11-16T20:00:00Z"));

        var history = await FoundAsync(goal, Mon16, Mon16);

        var day = Assert.Single(history.Entries);
        Assert.Equal(EvaluationState.Due, day.State);
        Assert.Equal(new Progress(ProgressKind.Total, 3, 2), day.Progress);
    }

    [Fact]
    public async Task FR_009_records_come_in_date_order_then_in_the_stored_order()
    {
        var goal = await _world.CreateGoalAsync(YogaThreeTimesAWeek, Mon16, Mon16Morning);
        var wednesdayFirst = _world.Records.Seed(goal, Wed18, RecordValue.FromAnswer(YesNoAnswer.Yes));
        var monday = _world.Records.Seed(goal, Mon16, RecordValue.FromAnswer(YesNoAnswer.No));
        var wednesdaySecond = _world.Records.Seed(goal, Wed18, RecordValue.FromAnswer(YesNoAnswer.No));
        _world.At(Utc("2026-11-19T10:00:00Z"));

        var history = await FoundAsync(goal, Mon16, Sun22);

        var week = Assert.Single(history.Entries);
        Assert.Equal([monday, wednesdayFirst, wednesdaySecond], week.Records.Select(r => r.Id));
        Assert.Equal([Mon16, Wed18, Wed18], week.Records.Select(r => r.Date));
        Assert.Equal([YesNoAnswer.No, YesNoAnswer.Yes, YesNoAnswer.No], week.Records.Select(r => r.Answer!.Value));
        Assert.All(week.Records, r => Assert.Null(r.Amount));
    }

    [Fact]
    public async Task FR_009_a_per_week_period_that_overlaps_the_range_gives_all_of_its_records_also_those_outside_it()
    {
        // The range Thursday 19 to Monday 23 November covers the week of 16 to 22 November only in part; its records of
        // Monday 16 and Wednesday 18 and its note, keyed under Monday 16, are outside the range and still given.
        var goal = await _world.CreateGoalAsync(YogaThreeTimesAWeek, Mon16, Mon16Morning);
        _world.Records.Seed(goal, Mon16, RecordValue.FromAnswer(YesNoAnswer.Yes));
        _world.Records.Seed(goal, Wed18, RecordValue.FromAnswer(YesNoAnswer.Yes));
        _world.Records.Seed(goal, Sat21, RecordValue.FromAnswer(YesNoAnswer.Yes));
        _world.Records.SeedNote(goal, Mon16, Note("Knee was sore."));
        _world.At(Utc("2026-11-24T10:00:00Z"));

        var history = await FoundAsync(goal, Thu19, Mon23);

        Assert.Equal([(Mon16, Sun22), (Mon23, Sun29)], history.Entries.Select(e => (e.FirstDate, e.LastDate)));
        var first = history.Entries[0];
        Assert.Equal([Mon16, Wed18, Sat21], first.Records.Select(r => r.Date));
        Assert.Equal(new Progress(ProgressKind.RecordsReachingAmount, 3, 3), first.Progress);
        Assert.Equal(EvaluationState.Pending, first.State);
        Assert.Equal(Utc("2026-11-25T00:00:00Z"), first.GraceEnds);
        Assert.Equal("Knee was sore.", first.Note);
        Assert.Equal(EvaluationState.Due, history.Entries[1].State);
        Assert.Equal([nameof(IRecordStore.ReadExemptionsAsync), nameof(IRecordStore.ReadAsync)], _world.Records.Calls);
    }

    [Fact]
    public async Task US5_AS4_a_range_of_367_days_is_refused()
    {
        var goal = await _world.CreateGoalAsync(Reading20MinDaily, Mon16, Mon16Morning);

        var result = await HistoryAsync(goal, Mon16, Mon16.AddDays(366));

        Assert.Equal((HistoryOutcome.RangeOutOfBounds, RangeOutOfBounds), (result.Outcome, result.Message));
        Assert.Null(result.History);
        Assert.Empty(_world.Records.Calls);
    }

    [Fact]
    public async Task FR_009_a_range_that_ends_before_it_starts_is_refused()
    {
        var goal = await _world.CreateGoalAsync(Reading20MinDaily, Mon16, Mon16Morning);

        var result = await HistoryAsync(goal, Tue17, Mon16);

        Assert.Equal((HistoryOutcome.RangeOutOfBounds, RangeOutOfBounds), (result.Outcome, result.Message));
        Assert.Null(result.History);
    }

    [Fact]
    public async Task FR_009_a_range_of_366_days_is_read()
    {
        var goal = await _world.CreateGoalAsync(Reading20MinDaily, Mon16, Mon16Morning);
        _world.At(Utc("2026-11-17T10:00:00Z"));

        var history = await FoundAsync(goal, Tue17.AddDays(-365), Tue17);

        Assert.Equal([Mon16, Tue17], history.Entries.Select(e => e.FirstDate));
    }

    [Fact]
    public async Task FR_018_the_range_is_checked_after_the_goal()
    {
        var unknown = await HistoryAsync(new GoalId(Guid.NewGuid()), Mon16, Mon16.AddDays(366));

        Assert.Equal((HistoryOutcome.NotFound, NotFound), (unknown.Outcome, unknown.Message));
        Assert.Null(unknown.History);
    }

    [Fact]
    public async Task FR_018_the_refusals_come_in_order_and_carry_no_history()
    {
        var goal = await _world.CreateGoalAsync(Reading20MinDaily, Mon16, Mon16Morning);

        var anonymous = await HistoryAsync(goal, Mon16, Mon16, ActingUser.Anonymous);
        var administrator = await HistoryAsync(goal, Mon16, Mon16, _world.Administrator);
        var other = await HistoryAsync(goal, Mon16, Mon16.AddDays(366), _world.OtherUser);

        Assert.Equal((HistoryOutcome.NotSignedIn, "You are not signed in."), (anonymous.Outcome, anonymous.Message));
        Assert.Equal((HistoryOutcome.NotForAdministrators, "Goals are for registered users."), (administrator.Outcome, administrator.Message));
        Assert.Equal((HistoryOutcome.NotFound, NotFound), (other.Outcome, other.Message));
        Assert.All(new[] { anonymous, administrator, other }, r => Assert.Null(r.History));
        Assert.Empty(_world.Records.Calls);
    }

    [Fact]
    public async Task FR_009_one_store_read_per_call_with_entries()
    {
        var goal = await FourteenDaysOfReadingAsync();

        await FoundAsync(goal, Mon2, Sun15);

        Assert.Equal([nameof(IRecordStore.ReadExemptionsAsync), nameof(IRecordStore.ReadAsync)], _world.Records.Calls);
    }

    [Fact]
    public async Task FR_009_unscheduled_days_only_give_no_entries_and_no_store_read()
    {
        var goal = await _world.CreateGoalAsync(YesNoDays(BuildOrReduce.Build, DayOfWeek.Monday), Mon16, Mon16Morning);
        _world.At(Utc("2026-11-19T10:00:00Z"));

        var history = await FoundAsync(goal, Tue17, Sun22);

        Assert.Equal(Thu19, history.CurrentDate);
        Assert.Empty(history.Entries);
        Assert.Empty(_world.Records.Calls);
    }

    [Fact]
    public async Task FR_009_days_before_the_start_date_give_no_entries_and_no_store_read()
    {
        var goal = await _world.CreateGoalAsync(Reading20MinDaily, Wed18, Mon16Morning);
        _world.At(Utc("2026-11-17T10:00:00Z"));

        var history = await FoundAsync(goal, Mon16, Tue17);

        Assert.Null(history.CurrentDate);
        Assert.Empty(history.Entries);
        Assert.Empty(_world.Records.Calls);
    }

    [Fact]
    public async Task FR_009_a_per_week_goals_days_before_its_first_week_give_no_entries_and_no_store_read()
    {
        var goal = await _world.CreateGoalAsync(YogaThreeTimesAWeek, Wed18, Utc("2026-11-18T08:00:00Z"));
        _world.At(Utc("2026-11-19T10:00:00Z"));

        var history = await FoundAsync(goal, Wed18, Sun22);

        Assert.Equal(Thu19, history.CurrentDate);
        Assert.Empty(history.Entries);
        Assert.Empty(_world.Records.Calls);
    }
}
