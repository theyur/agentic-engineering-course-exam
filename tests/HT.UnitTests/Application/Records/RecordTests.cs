using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using HT.UnitTests.Fakes;
using static HT.UnitTests.Application.Records.RecordWorld;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Records;

/// <summary>
/// <c>RecordAsync</c> (US1-AS1 to AS8, FR-001 to FR-007, edge cases) over <see cref="FakeRecordStore"/> and the slice-4
/// fakes. The user has no stored time zone, so the goals are in UTC and every local time below is the UTC literal; the
/// Kyiv case is UTC+2 throughout November 2026. Monday 16 November 2026 is the goals' start date unless a test says
/// otherwise; every goal is created at 08:00 on its start date.
/// </summary>
public sealed class RecordTests
{
    private const string NothingScheduled = "Nothing is scheduled on this date.";
    private const string DayNotBegun = "You cannot record a day that has not begun.";
    private const string AnswerMissing = "Choose Yes or No.";
    private const string CountOutOfRange = "Enter a whole number from 0 to 9999.";
    private const string DurationOutOfRange = "Enter a whole number of minutes from 0 to 1440.";
    private const string NoteTooLong = "The note can have at most 500 characters.";
    private const string RecordLimitReached = "This evaluation period already has 50 records.";

    private static readonly DateOnly Sun15 = Date(2026, 11, 15);
    private static readonly DateOnly Mon16 = Date(2026, 11, 16);
    private static readonly DateOnly Tue17 = Date(2026, 11, 17);
    private static readonly DateOnly Wed18 = Date(2026, 11, 18);
    private static readonly DateOnly Sun22 = Date(2026, 11, 22);
    private static readonly DateOnly Mon23 = Date(2026, 11, 23);
    private static readonly DateOnly Sun29 = Date(2026, 11, 29);

    /// <summary>08:00 UTC on Monday 16 November 2026, when the goals are created.</summary>
    private static readonly DateTimeOffset Mon16Morning = Utc("2026-11-16T08:00:00Z");

    /// <summary>10:00 UTC on Monday 16 November 2026, when most records are made.</summary>
    private static readonly DateTimeOffset Mon16TenOClock = Utc("2026-11-16T10:00:00Z");

    private readonly RecordWorld _world = new();

    private static GoalTargetFields Reading20MinDaily => DurationDays(20, DurationUnit.Minutes, BuildOrReduce.Build, AllDays);

    private static GoalTargetFields YogaThreeTimesAWeek => YesNoTimes(3, SchedulePeriod.Week);

    private static GoalTargetFields SugarAtMost1SweetADay => CountTimes(1, "sweet", 1, SchedulePeriod.Day, BuildOrReduce.Reduce);

    private static GoalTargetFields YesNoOnMonWedFri => YesNoDays(BuildOrReduce.Build, MonWedFri);

    private static GoalTargetFields Pages20ThreeTimesAWeek => CountTimes(20, "pages", 3, SchedulePeriod.Week);

    private Task<GoalId> GoalAsync(GoalTargetFields target, DateOnly? start = null, DateTimeOffset? at = null) =>
        _world.CreateGoalAsync(target, start ?? Mon16, at ?? Mon16Morning);

    private static HistoryEntry Recorded(RecordWriteResult result)
    {
        Assert.Equal(RecordWriteOutcome.Recorded, result.Outcome);
        Assert.Null(result.Message);
        Assert.NotNull(result.RecordId);
        return result.Entry!;
    }

    private void AssertRefused(RecordWriteResult result, RecordWriteOutcome outcome, string message)
    {
        Assert.Equal(outcome, result.Outcome);
        Assert.Equal(message, result.Message);
        Assert.Null(result.RecordId);
        Assert.Null(result.Entry);
        Assert.False(result.IsCorrection);
        Assert.DoesNotContain(nameof(IRecordStore.AddAsync), _world.Records.Calls);
    }

    [Fact]
    public async Task US1_AS1_twelve_minutes_today_are_kept_as_one_record_and_the_commitment_is_Due_at_12_of_20()
    {
        var goal = await GoalAsync(Reading20MinDaily);
        _world.At(Mon16TenOClock);

        var result = await _world.RecordAsync(goal, Mon16, Minutes(12));

        var entry = Recorded(result);
        Assert.False(result.IsCorrection);
        Assert.Equal(Mon16, entry.FirstDate);
        Assert.Equal(Mon16, entry.LastDate);
        Assert.Equal(Utc("2026-11-16T00:00:00Z"), entry.Starts);
        Assert.Equal(Utc("2026-11-17T00:00:00Z"), entry.Ends);
        Assert.Equal(EvaluationState.Due, entry.State);
        Assert.Equal(Utc("2026-11-19T00:00:00Z"), entry.GraceEnds);
        var record = Assert.Single(entry.Records);
        Assert.Equal(new RecordView(result.RecordId!.Value, Mon16, null, 12), record);
        Assert.Equal(new Progress(ProgressKind.RecordedValue, 12, 20), entry.Progress);
        Assert.False(entry.IsCorrection);
        Assert.Null(entry.Note);
    }

    [Fact]
    public async Task US1_AS2_recording_again_for_the_commitment_replaces_its_one_record_and_keeps_its_identifier()
    {
        var goal = await GoalAsync(Reading20MinDaily);
        _world.At(Mon16TenOClock);
        var first = await _world.RecordAsync(goal, Mon16, Minutes(12));

        var second = await _world.RecordAsync(goal, Mon16, Minutes(25));

        var entry = Recorded(second);
        Assert.Equal(first.RecordId, second.RecordId);
        var record = Assert.Single(entry.Records);
        Assert.Equal(25, record.Amount);
        Assert.Equal(first.RecordId, record.Id);
        Assert.Equal(new Progress(ProgressKind.RecordedValue, 25, 20), entry.Progress);
        Assert.Single(_world.Records.RecordsOfGoal(goal));
    }

    [Fact]
    public async Task US1_AS3_Yes_on_Monday_and_Wednesday_of_a_3_times_a_week_goal_gives_the_week_two_records_and_2_of_3()
    {
        var goal = await GoalAsync(YogaThreeTimesAWeek);
        _world.At(Mon16TenOClock);
        Recorded(await _world.RecordAsync(goal, Mon16, Yes()));
        _world.At(Utc("2026-11-18T10:00:00Z"));

        var entry = Recorded(await _world.RecordAsync(goal, Wed18, Yes()));

        Assert.Equal(Mon16, entry.FirstDate);
        Assert.Equal(Sun22, entry.LastDate);
        Assert.Equal(Utc("2026-11-23T00:00:00Z"), entry.Ends);
        Assert.Equal(EvaluationState.Due, entry.State);
        Assert.Equal([Mon16, Wed18], entry.Records.Select(r => r.Date));
        Assert.All(entry.Records, r => Assert.Equal(YesNoAnswer.Yes, r.Answer));
        Assert.Equal(new Progress(ProgressKind.RecordsReachingAmount, 2, 3), entry.Progress);
    }

    [Fact]
    public async Task US1_AS4_one_and_then_two_sweets_today_give_the_day_two_records_and_a_total_of_3()
    {
        var goal = await GoalAsync(SugarAtMost1SweetADay);
        _world.At(Mon16TenOClock);
        Recorded(await _world.RecordAsync(goal, Mon16, Amount(1)));

        var entry = Recorded(await _world.RecordAsync(goal, Mon16, Amount(2)));

        Assert.Equal([1, 2], entry.Records.Select(r => r.Amount));
        Assert.Equal(new Progress(ProgressKind.Total, 3, 1), entry.Progress);
        Assert.Equal(EvaluationState.Due, entry.State);
    }

    [Fact]
    public async Task US1_AS5_a_Tuesday_of_a_Mon_Wed_Fri_goal_has_nothing_scheduled_and_nothing_is_recorded()
    {
        var goal = await GoalAsync(YesNoOnMonWedFri);
        _world.At(Utc("2026-11-17T10:00:00Z"));

        var result = await _world.RecordAsync(goal, Tue17, Yes());

        AssertRefused(result, RecordWriteOutcome.NothingScheduled, NothingScheduled);
        Assert.Empty(_world.Records.Calls);
    }

    [Fact]
    public async Task US1_AS5_a_date_before_the_start_date_has_nothing_scheduled()
    {
        var goal = await GoalAsync(Reading20MinDaily);
        _world.At(Mon16TenOClock);

        var result = await _world.RecordAsync(goal, Sun15, Minutes(20));

        AssertRefused(result, RecordWriteOutcome.NothingScheduled, NothingScheduled);
        Assert.Empty(_world.Records.Calls);
    }

    [Fact]
    public async Task US1_AS5_a_day_before_a_per_week_goals_first_week_has_nothing_scheduled()
    {
        var goal = await GoalAsync(YogaThreeTimesAWeek, Wed18, Utc("2026-11-18T08:00:00Z"));
        _world.At(Utc("2026-11-18T10:00:00Z"));

        var result = await _world.RecordAsync(goal, Wed18, Yes());

        AssertRefused(result, RecordWriteOutcome.NothingScheduled, NothingScheduled);
        Assert.Empty(_world.Records.Calls);
    }

    [Fact]
    public async Task US1_AS6_tomorrow_on_an_every_day_goal_has_not_begun()
    {
        var goal = await GoalAsync(Reading20MinDaily);
        _world.At(Mon16TenOClock);

        var result = await _world.RecordAsync(goal, Tue17, Minutes(20));

        AssertRefused(result, RecordWriteOutcome.DayNotBegun, DayNotBegun);
        Assert.Empty(_world.Records.Calls);
    }

    [Fact]
    public async Task US1_AS6_tomorrow_on_a_per_day_goal_has_not_begun()
    {
        var goal = await GoalAsync(SugarAtMost1SweetADay);
        _world.At(Mon16TenOClock);

        var result = await _world.RecordAsync(goal, Tue17, Amount(0));

        AssertRefused(result, RecordWriteOutcome.DayNotBegun, DayNotBegun);
    }

    [Fact]
    public async Task US1_AS6_tomorrow_as_a_Tuesday_of_a_Mon_Wed_Fri_goal_has_nothing_scheduled_which_is_checked_first()
    {
        var goal = await GoalAsync(YesNoOnMonWedFri);
        _world.At(Mon16TenOClock);

        var result = await _world.RecordAsync(goal, Tue17, Yes());

        AssertRefused(result, RecordWriteOutcome.NothingScheduled, NothingScheduled);
    }

    [Fact]
    public async Task US1_AS6_a_day_begins_at_its_midnight_on_the_goals_timeline()
    {
        var goal = await GoalAsync(Reading20MinDaily);
        _world.At(Utc("2026-11-16T23:59:59Z"));
        AssertRefused(await _world.RecordAsync(goal, Tue17, Minutes(20)), RecordWriteOutcome.DayNotBegun, DayNotBegun);

        _world.At(Utc("2026-11-17T00:00:00Z"));

        Recorded(await _world.RecordAsync(goal, Tue17, Minutes(20)));
    }

    [Fact]
    public async Task FR_005_an_anonymous_visitor_is_not_signed_in()
    {
        var goal = await GoalAsync(Reading20MinDaily);

        var result = await _world.RecordAsync(goal, Mon16, Minutes(12), ActingUser.Anonymous);

        AssertRefused(result, RecordWriteOutcome.NotSignedIn, "You are not signed in.");
        Assert.Empty(_world.Records.Calls);
    }

    [Fact]
    public async Task FR_005_an_administrator_gets_goals_are_for_registered_users()
    {
        var goal = await GoalAsync(Reading20MinDaily);

        var result = await _world.RecordAsync(goal, Mon16, Minutes(12), _world.Administrator);

        AssertRefused(result, RecordWriteOutcome.NotForAdministrators, "Goals are for registered users.");
        Assert.Empty(_world.Records.Calls);
    }

    [Fact]
    public async Task FR_005_another_users_goal_does_not_exist_and_is_checked_before_the_date()
    {
        var goal = await GoalAsync(Reading20MinDaily);

        var result = await _world.RecordAsync(goal, Sun15, new RecordInput(), _world.OtherUser);

        AssertRefused(result, RecordWriteOutcome.NotFound, "This goal does not exist.");
        Assert.Empty(_world.Records.Calls);
    }

    [Fact]
    public async Task FR_005_a_yes_no_record_without_an_answer_gives_choose_yes_or_no()
    {
        var goal = await GoalAsync(YesNoOnMonWedFri);
        _world.At(Mon16TenOClock);

        AssertRefused(await _world.RecordAsync(goal, Mon16, new RecordInput()), RecordWriteOutcome.AnswerMissing, AnswerMissing);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(-1)]
    [InlineData(10000)]
    public async Task FR_005_a_count_missing_or_outside_0_to_9999_is_refused(int? amount)
    {
        var goal = await GoalAsync(SugarAtMost1SweetADay);
        _world.At(Mon16TenOClock);

        var result = await _world.RecordAsync(goal, Mon16, new RecordInput { Amount = amount });

        AssertRefused(result, RecordWriteOutcome.CountOutOfRange, CountOutOfRange);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(9999)]
    public async Task FR_005_a_count_from_0_to_9999_is_accepted(int amount)
    {
        var goal = await GoalAsync(SugarAtMost1SweetADay);
        _world.At(Mon16TenOClock);

        var entry = Recorded(await _world.RecordAsync(goal, Mon16, Amount(amount)));

        Assert.Equal(amount, Assert.Single(entry.Records).Amount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(-1)]
    [InlineData(1441)]
    public async Task FR_005_a_duration_missing_or_outside_0_to_1440_minutes_is_refused(int? minutes)
    {
        var goal = await GoalAsync(Reading20MinDaily);
        _world.At(Mon16TenOClock);

        var result = await _world.RecordAsync(goal, Mon16, new RecordInput { Amount = minutes });

        AssertRefused(result, RecordWriteOutcome.DurationOutOfRange, DurationOutOfRange);
    }

    [Fact]
    public async Task FR_005_a_duration_of_1440_minutes_is_accepted()
    {
        var goal = await GoalAsync(Reading20MinDaily);
        _world.At(Mon16TenOClock);

        var entry = Recorded(await _world.RecordAsync(goal, Mon16, Minutes(1440)));

        Assert.Equal(1440, Assert.Single(entry.Records).Amount);
    }

    [Fact]
    public async Task FR_005_a_note_of_more_than_500_characters_is_refused_and_nothing_is_recorded()
    {
        var goal = await GoalAsync(Reading20MinDaily);
        _world.At(Mon16TenOClock);

        var result = await _world.RecordAsync(goal, Mon16, Minutes(12, new string('a', 501)));

        AssertRefused(result, RecordWriteOutcome.NoteTooLong, NoteTooLong);
        Assert.Empty(_world.Records.Calls);
    }

    [Fact]
    public async Task FR_005_a_note_of_500_characters_after_trimming_is_accepted()
    {
        var goal = await GoalAsync(Reading20MinDaily);
        _world.At(Mon16TenOClock);
        var note = new string('a', 500);

        var entry = Recorded(await _world.RecordAsync(goal, Mon16, Minutes(12, "  " + note + "  ")));

        Assert.Equal(note, entry.Note);
    }

    [Fact]
    public async Task FR_005_the_51st_record_of_a_period_is_refused_and_the_period_keeps_its_50()
    {
        var goal = await GoalAsync(Pages20ThreeTimesAWeek);
        for (var i = 0; i < RecordLimits.MaxRecordsPerPeriod; i++)
        {
            _world.Records.Seed(goal, Mon16, RecordValue.FromAmount(20));
        }

        _world.At(Mon16TenOClock);

        var result = await _world.RecordAsync(goal, Mon16, Amount(25));

        Assert.Equal(RecordWriteOutcome.RecordLimitReached, result.Outcome);
        Assert.Equal(RecordLimitReached, result.Message);
        Assert.Null(result.Entry);
        Assert.Equal(50, _world.Records.RecordsOfGoal(goal).Count);
        Assert.All(_world.Records.RecordsOfGoal(goal), r => Assert.Equal(20, r.Value.Amount));
    }

    [Fact]
    public async Task FR_005_the_50th_record_of_a_period_is_accepted()
    {
        var goal = await GoalAsync(Pages20ThreeTimesAWeek);
        for (var i = 0; i < RecordLimits.MaxRecordsPerPeriod - 1; i++)
        {
            _world.Records.Seed(goal, Mon16, RecordValue.FromAmount(20));
        }

        _world.At(Mon16TenOClock);

        var entry = Recorded(await _world.RecordAsync(goal, Mon16, Amount(25)));

        Assert.Equal(50, entry.Records.Count);
    }

    [Fact]
    public async Task FR_005_a_commitment_takes_a_new_value_whatever_the_limit_because_its_record_is_replaced()
    {
        var goal = await GoalAsync(Reading20MinDaily);
        _world.At(Mon16TenOClock);
        var first = await _world.RecordAsync(goal, Mon16, Minutes(12));

        var changed = Recorded(await _world.RecordAsync(goal, Mon16, Minutes(30)));

        Assert.Equal(first.RecordId, Assert.Single(changed.Records).Id);
    }

    [Fact]
    public async Task FR_005_order_the_date_is_checked_before_the_value()
    {
        var goal = await GoalAsync(YesNoOnMonWedFri);
        _world.At(Utc("2026-11-17T10:00:00Z"));

        AssertRefused(await _world.RecordAsync(goal, Tue17, new RecordInput()), RecordWriteOutcome.NothingScheduled, NothingScheduled);
        AssertRefused(await _world.RecordAsync(goal, Wed18, new RecordInput()), RecordWriteOutcome.DayNotBegun, DayNotBegun);
    }

    [Fact]
    public async Task FR_005_order_the_value_is_checked_before_the_note()
    {
        var goal = await GoalAsync(Reading20MinDaily);
        _world.At(Mon16TenOClock);

        var result = await _world.RecordAsync(goal, Mon16, new RecordInput { Amount = 1441, Note = new string('a', 501) });

        AssertRefused(result, RecordWriteOutcome.DurationOutOfRange, DurationOutOfRange);
    }

    [Fact]
    public async Task FR_005_order_the_note_is_checked_before_the_limit()
    {
        var goal = await GoalAsync(Pages20ThreeTimesAWeek);
        for (var i = 0; i < RecordLimits.MaxRecordsPerPeriod; i++)
        {
            _world.Records.Seed(goal, Mon16, RecordValue.FromAmount(20));
        }

        _world.At(Mon16TenOClock);

        var result = await _world.RecordAsync(goal, Mon16, Amount(25, new string('a', 501)));

        AssertRefused(result, RecordWriteOutcome.NoteTooLong, NoteTooLong);
    }

    [Fact]
    public async Task FR_004_Yes_with_an_amount_on_a_count_goal_keeps_the_amount_only()
    {
        var goal = await GoalAsync(SugarAtMost1SweetADay);
        _world.At(Mon16TenOClock);

        var entry = Recorded(await _world.RecordAsync(goal, Mon16, new RecordInput { Answer = YesNoAnswer.Yes, Amount = 1 }));

        var record = Assert.Single(entry.Records);
        Assert.Null(record.Answer);
        Assert.Equal(1, record.Amount);
    }

    [Fact]
    public async Task FR_004_an_amount_with_Yes_on_a_yes_no_goal_keeps_the_answer_only()
    {
        var goal = await GoalAsync(YesNoOnMonWedFri);
        _world.At(Mon16TenOClock);

        var entry = Recorded(await _world.RecordAsync(goal, Mon16, new RecordInput { Answer = YesNoAnswer.Yes, Amount = 5 }));

        var record = Assert.Single(entry.Records);
        Assert.Equal(YesNoAnswer.Yes, record.Answer);
        Assert.Null(record.Amount);
    }

    [Fact]
    public async Task FR_004_only_an_amount_on_a_yes_no_goal_gives_choose_yes_or_no()
    {
        var goal = await GoalAsync(YesNoOnMonWedFri);
        _world.At(Mon16TenOClock);

        AssertRefused(await _world.RecordAsync(goal, Mon16, Amount(5)), RecordWriteOutcome.AnswerMissing, AnswerMissing);
    }

    [Fact]
    public async Task US1_AS8_a_note_given_with_a_record_is_kept_trimmed()
    {
        var goal = await GoalAsync(Reading20MinDaily);
        _world.At(Mon16TenOClock);

        var entry = Recorded(await _world.RecordAsync(goal, Mon16, Minutes(12, "  Back was tired.  ")));

        Assert.Equal("Back was tired.", entry.Note);
        Assert.Equal("Back was tired.", _world.Records.NotesOfGoal(goal)[Mon16].Value);
    }

    // Amended by specs/006-note-editing D-1: no note keeps the note, and an empty note or one of only spaces removes it.
    [Fact]
    public async Task FR_007_a_record_without_a_note_leaves_the_existing_note()
    {
        var goal = await GoalAsync(Reading20MinDaily);
        _world.At(Mon16TenOClock);
        Recorded(await _world.RecordAsync(goal, Mon16, Minutes(12, "Back was tired.")));

        var entry = Recorded(await _world.RecordAsync(goal, Mon16, Minutes(25, null)));

        Assert.Equal("Back was tired.", entry.Note);
        Assert.Equal("Back was tired.", _world.Records.NotesOfGoal(goal)[Mon16].Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task FR_007_a_record_with_an_empty_note_or_one_of_only_spaces_removes_the_existing_note(string note)
    {
        var goal = await GoalAsync(Reading20MinDaily);
        _world.At(Mon16TenOClock);
        Recorded(await _world.RecordAsync(goal, Mon16, Minutes(12, "Back was tired.")));

        var entry = Recorded(await _world.RecordAsync(goal, Mon16, Minutes(25, note)));

        Assert.Null(entry.Note);
        Assert.Equal(25, Assert.Single(entry.Records).Amount);
        Assert.Empty(_world.Records.NotesOfGoal(goal));
    }

    [Fact]
    public async Task FR_007_a_record_with_a_note_replaces_the_existing_note()
    {
        var goal = await GoalAsync(SugarAtMost1SweetADay);
        _world.At(Mon16TenOClock);
        Recorded(await _world.RecordAsync(goal, Mon16, Amount(1, "Birthday cake at work.")));

        var entry = Recorded(await _world.RecordAsync(goal, Mon16, Amount(1, "Second slice.")));

        Assert.Equal("Second slice.", entry.Note);
        Assert.Single(_world.Records.NotesOfGoal(goal));
    }

    [Fact]
    public async Task Edge_case_on_a_per_week_goal_a_record_falls_in_the_week_of_its_date_not_of_the_instant_of_saving()
    {
        // Kyiv is UTC+2: Sunday 22 November 23:59 is 21:59 UTC, and Monday 23 November 00:00 is 22:00 UTC on Sunday.
        var kyiv = _world.UserIn("Europe/Kyiv");
        var goal = await _world.CreateGoalAsync(YogaThreeTimesAWeek, Mon16, Utc("2026-11-16T06:00:00Z"), kyiv);

        _world.At(Utc("2026-11-22T21:59:00Z"));
        var sundayAt2359 = Recorded(await _world.RecordAsync(goal, Sun22, Yes(), kyiv));
        _world.At(Utc("2026-11-22T22:00:00Z"));
        var sundayAtMidnight = Recorded(await _world.RecordAsync(goal, Sun22, Yes(), kyiv));
        var mondayAtMidnight = Recorded(await _world.RecordAsync(goal, Mon23, Yes(), kyiv));

        Assert.Equal((Mon16, Sun22), (sundayAt2359.FirstDate, sundayAt2359.LastDate));
        Assert.Equal(EvaluationState.Due, sundayAt2359.State);
        Assert.Equal((Mon16, Sun22), (sundayAtMidnight.FirstDate, sundayAtMidnight.LastDate));
        Assert.Equal(EvaluationState.Pending, sundayAtMidnight.State);
        Assert.Equal(2, sundayAtMidnight.Records.Count);
        Assert.Equal((Mon23, Sun29), (mondayAtMidnight.FirstDate, mondayAtMidnight.LastDate));
        Assert.Equal(EvaluationState.Due, mondayAtMidnight.State);
        Assert.Equal([Mon23], mondayAtMidnight.Records.Select(r => r.Date));
    }

    [Fact]
    public async Task FR_006_a_record_for_a_final_commitment_is_a_correction_from_the_instant_its_grace_period_ends()
    {
        var goal = await GoalAsync(Reading20MinDaily);
        _world.At(Utc("2026-11-18T23:59:59Z"));
        var pending = await _world.RecordAsync(goal, Mon16, Minutes(12));
        _world.At(Utc("2026-11-19T00:00:00Z"));

        var final = await _world.RecordAsync(goal, Mon16, Minutes(25));

        Assert.False(pending.IsCorrection);
        Assert.Equal(EvaluationState.Pending, Recorded(pending).State);
        Assert.True(final.IsCorrection);
        var entry = Recorded(final);
        Assert.True(entry.IsCorrection);
        Assert.Equal(EvaluationState.Met, entry.State);
        Assert.Null(entry.GraceEnds);
    }

    [Fact]
    public async Task HP_006_no_refusal_message_contains_an_input_value()
    {
        const string secretNote = "Secret 7319 note";
        var duration = await GoalAsync(Reading20MinDaily);
        var count = await GoalAsync(SugarAtMost1SweetADay);
        var yesNo = await GoalAsync(YesNoOnMonWedFri);
        _world.At(Utc("2026-11-17T10:00:00Z"));
        var longNote = secretNote + new string('x', 500);

        var results = new[]
        {
            await _world.RecordAsync(duration, Date(2026, 11, 27), new RecordInput { Amount = 7319, Note = secretNote }),
            await _world.RecordAsync(yesNo, Tue17, new RecordInput { Amount = 7319, Note = secretNote }),
            await _world.RecordAsync(duration, Tue17, new RecordInput { Amount = 7319, Note = secretNote }),
            await _world.RecordAsync(count, Tue17, new RecordInput { Amount = 17319, Note = secretNote }),
            await _world.RecordAsync(yesNo, Mon16, new RecordInput { Amount = 7319, Note = secretNote }),
            await _world.RecordAsync(count, Tue17, new RecordInput { Amount = 1, Note = longNote }),
            await _world.RecordAsync(duration, Tue17, new RecordInput { Amount = 7319, Note = secretNote }, _world.OtherUser),
        };

        Assert.All(results, result =>
        {
            Assert.NotEqual(RecordWriteOutcome.Recorded, result.Outcome);
            Assert.DoesNotContain("7319", result.Message!, StringComparison.Ordinal);
            Assert.DoesNotContain("Secret", result.Message!, StringComparison.Ordinal);
            Assert.DoesNotContain("27", result.Message!, StringComparison.Ordinal);
            Assert.DoesNotContain("17", result.Message!, StringComparison.Ordinal);
        });
    }
}
