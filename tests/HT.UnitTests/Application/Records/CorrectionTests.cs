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
/// Corrections (US3-AS1 to AS7, FR-003, FR-005, FR-006, FR-007): <c>RecordAsync</c> on final entries, and
/// <c>ChangeRecordAsync</c> and <c>DeleteRecordAsync</c>, over the fakes of <see cref="RecordWorld"/>. Unless a test says
/// otherwise the user has no stored time zone, so the goals are in UTC and start on Monday 16 November 2026, created at
/// 08:00Z that day. A day runs from 00:00Z to 00:00Z the next day and its grace period ends 48 hours later, so Monday
/// 16's ends at 2026-11-19T00:00:00Z; the week of Monday 16 ends at 2026-11-23T00:00:00Z and its grace period at
/// 2026-11-25T00:00:00Z. Every instant is worked out by hand.
/// </summary>
public sealed class CorrectionTests
{
    private const string RecordNotFound = "This record does not exist.";
    private const string AnswerMissing = "Choose Yes or No.";
    private const string CountOutOfRange = "Enter a whole number from 0 to 9999.";
    private const string DurationOutOfRange = "Enter a whole number of minutes from 0 to 1440.";
    private const string NoteTooLong = "The note can have at most 500 characters.";
    private const string DayNotBegun = "You cannot record a day that has not begun.";

    private static readonly DateOnly Mon16 = Date(2026, 11, 16);
    private static readonly DateOnly Tue17 = Date(2026, 11, 17);
    private static readonly DateOnly Wed18 = Date(2026, 11, 18);
    private static readonly DateOnly Thu19 = Date(2026, 11, 19);
    private static readonly DateOnly Fri20 = Date(2026, 11, 20);
    private static readonly DateOnly Sun22 = Date(2026, 11, 22);

    private static readonly DateTimeOffset Mon16Morning = Utc("2026-11-16T08:00:00Z");
    private static readonly DateTimeOffset Mon16TenOClock = Utc("2026-11-16T10:00:00Z");

    /// <summary>The last second before Monday 16's grace period ends.</summary>
    private static readonly DateTimeOffset Mon16GraceEndMinusOne = Utc("2026-11-18T23:59:59Z");

    /// <summary>The instant Monday 16's grace period ends (Wednesday 18 + 24 h, 00:00Z on Thursday 19).</summary>
    private static readonly DateTimeOffset Mon16GraceEnd = Utc("2026-11-19T00:00:00Z");

    /// <summary>Friday 20 November, 10:00Z: Monday 16 is final.</summary>
    private static readonly DateTimeOffset Fri20TenOClock = Utc("2026-11-20T10:00:00Z");

    /// <summary>Thursday 26 November, 10:00Z: the week of Monday 16 is final.</summary>
    private static readonly DateTimeOffset Thu26TenOClock = Utc("2026-11-26T10:00:00Z");

    private readonly RecordWorld _world = new();

    private static GoalTargetFields Reading20MinDaily => DurationDays(20, DurationUnit.Minutes, BuildOrReduce.Build, AllDays);

    private static GoalTargetFields YogaThreeTimesAWeek => YesNoTimes(3, SchedulePeriod.Week);

    private static GoalTargetFields Pages20ThreeTimesAWeek => CountTimes(20, "pages", 3, SchedulePeriod.Week);

    private static GoalTargetFields CupsAtMost2ADay => CountDays(2, "cups", BuildOrReduce.Reduce, AllDays);

    private static GoalTargetFields YesNoOnMonWedFri => YesNoDays(BuildOrReduce.Build, MonWedFri);

    private Task<GoalId> GoalAsync(GoalTargetFields target, ActingUser? user = null) =>
        _world.CreateGoalAsync(target, Mon16, Mon16Morning, user);

    /// <summary>Records at <paramref name="at"/> and gives the record's identifier.</summary>
    private async Task<RecordId> RecordAtAsync(GoalId goal, DateOnly date, RecordInput input, DateTimeOffset at, ActingUser? user = null)
    {
        _world.At(at);
        var result = await _world.RecordAsync(goal, date, input, user);
        Assert.Equal(RecordWriteOutcome.Recorded, result.Outcome);
        return result.RecordId!.Value;
    }

    private static HistoryEntry Recorded(RecordWriteResult result)
    {
        Assert.Equal(RecordWriteOutcome.Recorded, result.Outcome);
        Assert.Null(result.Message);
        return result.Entry!;
    }

    private static HistoryEntry Changed(RecordWriteResult result, RecordId record)
    {
        Assert.Equal(RecordWriteOutcome.Changed, result.Outcome);
        Assert.Null(result.Message);
        Assert.Equal(record, result.RecordId);
        return result.Entry!;
    }

    private static HistoryEntry Deleted(RecordWriteResult result)
    {
        Assert.Equal(RecordWriteOutcome.Deleted, result.Outcome);
        Assert.Null(result.Message);
        Assert.Null(result.RecordId);
        return result.Entry!;
    }

    private static void AssertRefused(RecordWriteResult result, RecordWriteOutcome outcome, string message)
    {
        Assert.Equal(outcome, result.Outcome);
        Assert.Equal(message, result.Message);
        Assert.Null(result.RecordId);
        Assert.Null(result.Entry);
        Assert.False(result.IsCorrection);
    }

    private void AssertNothingWritten()
    {
        Assert.DoesNotContain(nameof(IRecordStore.ChangeAsync), _world.Records.Calls);
        Assert.DoesNotContain(nameof(IRecordStore.DeleteAsync), _world.Records.Calls);
    }

    // ---- AS1 to AS4: a correction is evaluated as if it had been recorded on time ----

    [Fact]
    public async Task US3_AS1_a_record_reaching_the_target_on_a_Missed_commitment_gives_Met_and_is_a_correction()
    {
        var goal = await GoalAsync(Reading20MinDaily);
        _world.At(Fri20TenOClock);
        Assert.Equal(EvaluationState.Missed, Assert.Single(await _world.HistoryAsync(goal, Mon16, Mon16)).State);

        var result = await _world.RecordAsync(goal, Mon16, Minutes(20));

        var entry = Recorded(result);
        Assert.True(result.IsCorrection);
        Assert.True(entry.IsCorrection);
        Assert.Equal(EvaluationState.Met, entry.State);
        Assert.Null(entry.GraceEnds);
        Assert.Equal(new Progress(ProgressKind.RecordedValue, 20, 20), entry.Progress);
    }

    [Fact]
    public async Task US3_AS2_a_Met_record_changed_below_the_target_gives_Failed_and_is_a_correction()
    {
        var goal = await GoalAsync(Reading20MinDaily);
        var record = await RecordAtAsync(goal, Mon16, Minutes(20), Mon16TenOClock);
        _world.At(Fri20TenOClock);
        Assert.Equal(EvaluationState.Met, Assert.Single(await _world.HistoryAsync(goal, Mon16, Mon16)).State);

        var result = await _world.ChangeAsync(goal, record, Minutes(12));

        var entry = Changed(result, record);
        Assert.True(result.IsCorrection);
        Assert.True(entry.IsCorrection);
        Assert.Equal(EvaluationState.Failed, entry.State);
        Assert.Equal(new RecordView(record, Mon16, null, 12), Assert.Single(entry.Records));
        Assert.Equal(new Progress(ProgressKind.RecordedValue, 12, 20), entry.Progress);
        Assert.Equal(EvaluationState.Failed, Assert.Single(await _world.HistoryAsync(goal, Mon16, Mon16)).State);
    }

    [Fact]
    public async Task US3_AS3_deleting_the_record_of_a_final_commitment_leaves_it_without_a_record_and_Missed()
    {
        var goal = await GoalAsync(Reading20MinDaily);
        var record = await RecordAtAsync(goal, Mon16, Minutes(20), Mon16TenOClock);
        _world.At(Fri20TenOClock);

        var result = await _world.DeleteAsync(goal, record);

        var entry = Deleted(result);
        Assert.True(result.IsCorrection);
        Assert.Equal(EvaluationState.Missed, entry.State);
        Assert.Empty(entry.Records);
        Assert.Empty(_world.Records.RecordsOfGoal(goal));
        Assert.Equal(EvaluationState.Missed, Assert.Single(await _world.HistoryAsync(goal, Mon16, Mon16)).State);
    }

    [Fact]
    public async Task US3_AS4_adding_the_missing_Yes_to_a_Failed_week_gives_Met()
    {
        var goal = await GoalAsync(YogaThreeTimesAWeek);
        await RecordAtAsync(goal, Mon16, Yes(), Mon16TenOClock);
        await RecordAtAsync(goal, Wed18, Yes(), Utc("2026-11-18T10:00:00Z"));
        _world.At(Thu26TenOClock);
        Assert.Equal(EvaluationState.Failed, Assert.Single(await _world.HistoryAsync(goal, Mon16, Sun22)).State);

        var result = await _world.RecordAsync(goal, Fri20, Yes());

        var entry = Recorded(result);
        Assert.True(result.IsCorrection);
        Assert.Equal((Mon16, Sun22), (entry.FirstDate, entry.LastDate));
        Assert.Equal(EvaluationState.Met, entry.State);
        Assert.Equal([Mon16, Wed18, Fri20], entry.Records.Select(r => r.Date));
        Assert.Equal(new Progress(ProgressKind.RecordsReachingAmount, 3, 3), entry.Progress);
    }

    // ---- AS5 (SC-003): on time and by correction give the same results ----

    [Fact]
    public async Task US3_AS5_commitments_recorded_on_time_and_by_correction_with_the_same_values_have_the_same_states()
    {
        var onTime = await GoalAsync(Reading20MinDaily);
        var corrected = await GoalAsync(Reading20MinDaily);
        await RecordAtAsync(onTime, Mon16, Minutes(12), Utc("2026-11-16T10:00:00Z"));
        await RecordAtAsync(onTime, Tue17, Minutes(20), Utc("2026-11-17T10:00:00Z"));
        await RecordAtAsync(onTime, Thu19, Minutes(25), Utc("2026-11-19T10:00:00Z"));

        // Thursday 19's grace period ends at 2026-11-22T00:00:00Z, so every record below is a correction.
        _world.At(Utc("2026-11-24T10:00:00Z"));
        var corrections = new[]
        {
            await _world.RecordAsync(corrected, Mon16, Minutes(12)),
            await _world.RecordAsync(corrected, Tue17, Minutes(20)),
            await _world.RecordAsync(corrected, Thu19, Minutes(25)),
        };
        var expected = await _world.HistoryAsync(onTime, Mon16, Thu19);
        var actual = await _world.HistoryAsync(corrected, Mon16, Thu19);

        Assert.All(corrections, result => Assert.True(Recorded(result).IsCorrection));
        Assert.Equal(
            [EvaluationState.Failed, EvaluationState.Met, EvaluationState.Missed, EvaluationState.Met],
            expected.Select(e => e.State));
        Assert.Equal(expected.Select(e => e.State), actual.Select(e => e.State));
        Assert.Equal(expected.Select(e => e.Progress), actual.Select(e => e.Progress));
    }

    [Fact]
    public async Task US3_AS5_weeks_recorded_on_time_and_by_correction_with_the_same_values_have_the_same_states()
    {
        var onTime = await GoalAsync(YogaThreeTimesAWeek);
        var corrected = await GoalAsync(YogaThreeTimesAWeek);
        await RecordAtAsync(onTime, Mon16, Yes(), Utc("2026-11-16T10:00:00Z"));
        await RecordAtAsync(onTime, Wed18, Yes(), Utc("2026-11-18T10:00:00Z"));
        await RecordAtAsync(onTime, Fri20, No(), Utc("2026-11-20T10:00:00Z"));

        _world.At(Thu26TenOClock);
        var corrections = new[]
        {
            await _world.RecordAsync(corrected, Mon16, Yes()),
            await _world.RecordAsync(corrected, Wed18, Yes()),
            await _world.RecordAsync(corrected, Fri20, No()),
        };
        var expected = Assert.Single(await _world.HistoryAsync(onTime, Mon16, Sun22));
        var actual = Assert.Single(await _world.HistoryAsync(corrected, Mon16, Sun22));

        Assert.All(corrections, result => Assert.True(Recorded(result).IsCorrection));
        Assert.Equal(EvaluationState.Failed, expected.State);
        Assert.Equal(expected.State, actual.State);
        Assert.Equal(expected.Progress, actual.Progress);
    }

    // ---- AS6: a commitment of an old settings version keeps its own instants ----

    [Fact]
    public async Task US3_AS6_after_a_time_zone_change_a_commitment_of_the_old_version_keeps_its_start_end_and_grace_end_when_corrected()
    {
        // Version 1 in New York (UTC−5) from Monday 16, version 2 in Tokyo (UTC+9) from Wednesday 18. Tuesday 17 belongs to
        // version 1: it runs from 2026-11-17T05:00Z to 2026-11-18T05:00Z and its grace period ends at 2026-11-20T05:00Z. In
        // Tokyo it would have ended at 2026-11-17T15:00Z, with a grace end at 2026-11-19T15:00Z.
        var goal = _world.AddGoal(_world.User, Target(Reading20MinDaily), (Mon16, NewYork), (Wed18, Tokyo));

        _world.At(Utc("2026-11-20T04:59:59Z"));
        var recorded = await _world.RecordAsync(goal, Tue17, Minutes(12));
        _world.At(Utc("2026-11-20T05:00:00Z"));
        var changed = await _world.ChangeAsync(goal, recorded.RecordId!.Value, Minutes(20));
        _world.At(Utc("2026-11-21T00:00:00Z"));
        var deleted = await _world.DeleteAsync(goal, recorded.RecordId!.Value);

        var pending = Recorded(recorded);
        Assert.False(recorded.IsCorrection);
        Assert.Equal((EvaluationState.Pending, Utc("2026-11-20T05:00:00Z")), (pending.State, pending.GraceEnds!.Value));
        var met = Changed(changed, recorded.RecordId!.Value);
        Assert.True(changed.IsCorrection);
        Assert.Equal(EvaluationState.Met, met.State);
        var missed = Deleted(deleted);
        Assert.True(deleted.IsCorrection);
        Assert.Equal(EvaluationState.Missed, missed.State);
        Assert.All(new[] { pending, met, missed }, entry =>
            Assert.Equal((Utc("2026-11-17T05:00:00Z"), Utc("2026-11-18T05:00:00Z")), (entry.Starts, entry.Ends)));
    }

    // ---- AS7: a record that is not one of that goal's ----

    [Fact]
    public async Task US3_AS7_a_record_of_another_goal_of_another_account_or_of_nothing_does_not_exist_for_a_change()
    {
        var goal = await GoalAsync(Reading20MinDaily);
        var otherGoal = await GoalAsync(Reading20MinDaily);
        var otherAccountsGoal = await GoalAsync(Reading20MinDaily, _world.OtherUser);
        await RecordAtAsync(goal, Mon16, Minutes(20), Mon16TenOClock);
        var ofOtherGoal = await RecordAtAsync(otherGoal, Mon16, Minutes(20), Mon16TenOClock);
        var ofOtherAccount = await RecordAtAsync(otherAccountsGoal, Mon16, Minutes(20), Mon16TenOClock, _world.OtherUser);

        foreach (var record in new[] { ofOtherGoal, ofOtherAccount, new RecordId(Guid.NewGuid()) })
        {
            AssertRefused(await _world.ChangeAsync(goal, record, Minutes(5)), RecordWriteOutcome.RecordNotFound, RecordNotFound);
        }

        AssertNothingWritten();
        Assert.Equal(20, Assert.Single(_world.Records.RecordsOfGoal(goal)).Value.Amount);
        Assert.Equal(20, Assert.Single(_world.Records.RecordsOfGoal(otherGoal)).Value.Amount);
        Assert.Equal(20, Assert.Single(_world.Records.RecordsOfGoal(otherAccountsGoal)).Value.Amount);
    }

    [Fact]
    public async Task US3_AS7_a_record_of_another_goal_of_another_account_or_of_nothing_does_not_exist_for_a_delete()
    {
        var goal = await GoalAsync(Reading20MinDaily);
        var otherGoal = await GoalAsync(Reading20MinDaily);
        var otherAccountsGoal = await GoalAsync(Reading20MinDaily, _world.OtherUser);
        await RecordAtAsync(goal, Mon16, Minutes(20), Mon16TenOClock);
        var ofOtherGoal = await RecordAtAsync(otherGoal, Mon16, Minutes(20), Mon16TenOClock);
        var ofOtherAccount = await RecordAtAsync(otherAccountsGoal, Mon16, Minutes(20), Mon16TenOClock, _world.OtherUser);

        foreach (var record in new[] { ofOtherGoal, ofOtherAccount, new RecordId(Guid.NewGuid()) })
        {
            AssertRefused(await _world.DeleteAsync(goal, record), RecordWriteOutcome.RecordNotFound, RecordNotFound);
        }

        AssertNothingWritten();
        Assert.Single(_world.Records.RecordsOfGoal(goal));
        Assert.Single(_world.Records.RecordsOfGoal(otherGoal));
        Assert.Single(_world.Records.RecordsOfGoal(otherAccountsGoal));
    }

    [Fact]
    public async Task FR_005_access_is_checked_before_the_record_for_a_change_and_a_delete()
    {
        var goal = await GoalAsync(Reading20MinDaily);
        var record = _world.Records.Seed(goal, Mon16, RecordValue.FromAmount(20));
        _world.At(Mon16TenOClock);

        var results = new[]
        {
            (await _world.ChangeAsync(goal, record, Minutes(5), ActingUser.Anonymous), RecordWriteOutcome.NotSignedIn, "You are not signed in."),
            (await _world.DeleteAsync(goal, record, ActingUser.Anonymous), RecordWriteOutcome.NotSignedIn, "You are not signed in."),
            (await _world.ChangeAsync(goal, record, Minutes(5), _world.Administrator), RecordWriteOutcome.NotForAdministrators, "Goals are for registered users."),
            (await _world.DeleteAsync(goal, record, _world.Administrator), RecordWriteOutcome.NotForAdministrators, "Goals are for registered users."),
            (await _world.ChangeAsync(goal, record, Minutes(5), _world.OtherUser), RecordWriteOutcome.NotFound, "This goal does not exist."),
            (await _world.DeleteAsync(goal, record, _world.OtherUser), RecordWriteOutcome.NotFound, "This goal does not exist."),
        };

        Assert.All(results, r => AssertRefused(r.Item1, r.Item2, r.Item3));
        Assert.Empty(_world.Records.Calls);
        Assert.Equal(20, Assert.Single(_world.Records.RecordsOfGoal(goal)).Value.Amount);
    }

    // ---- What a correction is (FR-006) ----

    [Fact]
    public async Task FR_006_deleting_the_only_record_of_a_Due_or_Pending_commitment_is_not_a_correction()
    {
        var goal = await GoalAsync(Reading20MinDaily);
        var due = await RecordAtAsync(goal, Mon16, Minutes(20), Mon16TenOClock);
        _world.At(Utc("2026-11-16T11:00:00Z"));
        var deletedDue = await _world.DeleteAsync(goal, due);
        var pending = await RecordAtAsync(goal, Tue17, Minutes(20), Utc("2026-11-17T10:00:00Z"));

        // Tuesday 17 ends at 2026-11-18T00:00:00Z and its grace period at 2026-11-20T00:00:00Z.
        _world.At(Utc("2026-11-19T23:59:59Z"));
        var deletedPending = await _world.DeleteAsync(goal, pending);

        Assert.False(deletedDue.IsCorrection);
        Assert.Equal(EvaluationState.Due, Deleted(deletedDue).State);
        Assert.False(deletedPending.IsCorrection);
        var entry = Deleted(deletedPending);
        Assert.Equal(EvaluationState.Pending, entry.State);
        Assert.False(entry.IsCorrection);
        Assert.Equal(Utc("2026-11-20T00:00:00Z"), entry.GraceEnds);
    }

    [Fact]
    public async Task FR_006_a_change_is_a_correction_from_the_instant_the_grace_period_ends()
    {
        var goal = await GoalAsync(Reading20MinDaily);
        var record = await RecordAtAsync(goal, Mon16, Minutes(12), Mon16TenOClock);

        _world.At(Mon16GraceEndMinusOne);
        var before = await _world.ChangeAsync(goal, record, Minutes(15));
        _world.At(Mon16GraceEnd);
        var at = await _world.ChangeAsync(goal, record, Minutes(25));

        Assert.False(before.IsCorrection);
        Assert.Equal(EvaluationState.Pending, Changed(before, record).State);
        Assert.True(at.IsCorrection);
        Assert.Equal(EvaluationState.Met, Changed(at, record).State);
    }

    [Fact]
    public async Task FR_006_a_delete_is_a_correction_from_the_instant_the_grace_period_ends()
    {
        var goal = await GoalAsync(Pages20ThreeTimesAWeek);
        var first = _world.Records.Seed(goal, Mon16, RecordValue.FromAmount(20));
        var second = _world.Records.Seed(goal, Mon16, RecordValue.FromAmount(20));

        // The week of Monday 16 ends at 2026-11-23T00:00:00Z; its grace period ends at 2026-11-25T00:00:00Z.
        _world.At(Utc("2026-11-24T23:59:59Z"));
        var before = await _world.DeleteAsync(goal, first);
        _world.At(Utc("2026-11-25T00:00:00Z"));
        var at = await _world.DeleteAsync(goal, second);

        Assert.False(before.IsCorrection);
        Assert.Equal(EvaluationState.Pending, Deleted(before).State);
        Assert.True(at.IsCorrection);
        Assert.Equal(EvaluationState.Missed, Deleted(at).State);
    }

    // ---- The check order of change and delete (FR-005; contracts/application-api.md) ----

    [Fact]
    public async Task FR_005_a_change_checks_no_date_so_a_record_dated_a_day_that_has_not_begun_is_still_changed()
    {
        // A record cannot be made for Wednesday 18 on Monday 16; one seeded for it is still changed, as the store has it.
        var goal = await GoalAsync(Reading20MinDaily);
        var record = _world.Records.Seed(goal, Wed18, RecordValue.FromAmount(12));
        _world.At(Mon16TenOClock);
        AssertRefused(await _world.RecordAsync(goal, Wed18, Minutes(20)), RecordWriteOutcome.DayNotBegun, DayNotBegun);

        var entry = Changed(await _world.ChangeAsync(goal, record, Minutes(20)), record);

        Assert.Equal(new RecordView(record, Wed18, null, 20), Assert.Single(entry.Records));
        Assert.Equal(EvaluationState.Upcoming, entry.State);
    }

    [Fact]
    public async Task FR_005_a_delete_checks_nothing_after_the_record()
    {
        var goal = await GoalAsync(Reading20MinDaily);
        var record = _world.Records.Seed(goal, Wed18, RecordValue.FromAmount(12));
        _world.At(Mon16TenOClock);

        var entry = Deleted(await _world.DeleteAsync(goal, record));

        Assert.Empty(entry.Records);
        Assert.Equal([nameof(IRecordStore.FindRecordDateAsync), nameof(IRecordStore.DeleteAsync)], _world.Records.Calls);
    }

    [Fact]
    public async Task FR_005_a_change_checks_the_value_for_the_goals_measurement()
    {
        var yesNo = await GoalAsync(YesNoOnMonWedFri);
        var count = await GoalAsync(CupsAtMost2ADay);
        var duration = await GoalAsync(Reading20MinDaily);
        var yesNoRecord = _world.Records.Seed(yesNo, Mon16, RecordValue.FromAnswer(YesNoAnswer.Yes));
        var countRecord = _world.Records.Seed(count, Mon16, RecordValue.FromAmount(1));
        var durationRecord = _world.Records.Seed(duration, Mon16, RecordValue.FromAmount(20));
        _world.At(Mon16TenOClock);

        AssertRefused(await _world.ChangeAsync(yesNo, yesNoRecord, Amount(1)), RecordWriteOutcome.AnswerMissing, AnswerMissing);
        AssertRefused(await _world.ChangeAsync(count, countRecord, Amount(10000)), RecordWriteOutcome.CountOutOfRange, CountOutOfRange);
        AssertRefused(await _world.ChangeAsync(count, countRecord, new RecordInput()), RecordWriteOutcome.CountOutOfRange, CountOutOfRange);
        AssertRefused(await _world.ChangeAsync(duration, durationRecord, Minutes(1441)), RecordWriteOutcome.DurationOutOfRange, DurationOutOfRange);
        AssertRefused(await _world.ChangeAsync(duration, durationRecord, Minutes(-1)), RecordWriteOutcome.DurationOutOfRange, DurationOutOfRange);

        AssertNothingWritten();
        Assert.Equal(YesNoAnswer.Yes, Assert.Single(_world.Records.RecordsOfGoal(yesNo)).Value.Answer);
        Assert.Equal(1, Assert.Single(_world.Records.RecordsOfGoal(count)).Value.Amount);
        Assert.Equal(20, Assert.Single(_world.Records.RecordsOfGoal(duration)).Value.Amount);
    }

    [Fact]
    public async Task FR_005_a_change_checks_the_record_before_the_value_and_the_value_before_the_note()
    {
        var goal = await GoalAsync(Reading20MinDaily);
        var record = _world.Records.Seed(goal, Mon16, RecordValue.FromAmount(20));
        _world.At(Mon16TenOClock);
        var bad = new RecordInput { Amount = 1441, Note = new string('a', 501) };

        AssertRefused(await _world.ChangeAsync(goal, new RecordId(Guid.NewGuid()), bad), RecordWriteOutcome.RecordNotFound, RecordNotFound);
        AssertRefused(await _world.ChangeAsync(goal, record, bad), RecordWriteOutcome.DurationOutOfRange, DurationOutOfRange);
        AssertRefused(await _world.ChangeAsync(goal, record, Minutes(25, new string('a', 501))), RecordWriteOutcome.NoteTooLong, NoteTooLong);

        AssertNothingWritten();
        Assert.Equal(20, Assert.Single(_world.Records.RecordsOfGoal(goal)).Value.Amount);
        Assert.Empty(_world.Records.NotesOfGoal(goal));
    }

    // ---- The note given with a change (FR-007) ----

    [Fact]
    public async Task FR_007_a_change_with_a_note_with_text_replaces_the_entrys_note_trimmed()
    {
        var goal = await GoalAsync(Reading20MinDaily);
        var record = await RecordAtAsync(goal, Mon16, Minutes(12, "Back was tired."), Mon16TenOClock);

        var entry = Changed(await _world.ChangeAsync(goal, record, Minutes(25, "  Finished the chapter.  ")), record);

        Assert.Equal("Finished the chapter.", entry.Note);
        Assert.Equal("Finished the chapter.", Assert.Single(_world.Records.NotesOfGoal(goal)).Value.Value);
    }

    // Amended by specs/006-note-editing D-1: no note keeps the note, and an empty note or one of only spaces removes it.
    [Fact]
    public async Task FR_007_a_change_without_a_note_leaves_the_entrys_note()
    {
        var goal = await GoalAsync(Reading20MinDaily);
        var record = await RecordAtAsync(goal, Mon16, Minutes(12, "Back was tired."), Mon16TenOClock);

        var entry = Changed(await _world.ChangeAsync(goal, record, Minutes(25, null)), record);

        Assert.Equal("Back was tired.", entry.Note);
        Assert.Equal(25, Assert.Single(entry.Records).Amount);
        Assert.Equal("Back was tired.", Assert.Single(_world.Records.NotesOfGoal(goal)).Value.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task FR_007_US1_AS3_a_change_with_an_empty_note_or_one_of_only_spaces_removes_the_entrys_note(string note)
    {
        var goal = await GoalAsync(Reading20MinDaily);
        var record = await RecordAtAsync(goal, Mon16, Minutes(12, "Back was tired."), Mon16TenOClock);

        var entry = Changed(await _world.ChangeAsync(goal, record, Minutes(25, note)), record);

        Assert.Null(entry.Note);
        Assert.Equal(25, Assert.Single(entry.Records).Amount);
        Assert.Equal(25, Assert.Single(_world.Records.RecordsOfGoal(goal)).Value.Amount);
        Assert.Empty(_world.Records.NotesOfGoal(goal));
    }

    [Fact]
    public async Task FR_007_a_delete_leaves_the_entrys_note()
    {
        var goal = await GoalAsync(Reading20MinDaily);
        var record = await RecordAtAsync(goal, Mon16, Minutes(12, "Back was tired."), Mon16TenOClock);

        var entry = Deleted(await _world.DeleteAsync(goal, record));

        Assert.Equal("Back was tired.", entry.Note);
    }

    // ---- The limit of 50 records concerns adding only ----

    [Fact]
    public async Task A_period_at_50_records_still_takes_a_change_and_a_delete()
    {
        var goal = await GoalAsync(Pages20ThreeTimesAWeek);
        var records = Enumerable.Range(0, RecordLimits.MaxRecordsPerPeriod)
            .Select(_ => _world.Records.Seed(goal, Mon16, RecordValue.FromAmount(20)))
            .ToList();
        _world.At(Mon16TenOClock);

        var changed = Changed(await _world.ChangeAsync(goal, records[0], Amount(25)), records[0]);
        var deleted = Deleted(await _world.DeleteAsync(goal, records[1]));

        Assert.Equal(50, changed.Records.Count);
        Assert.Equal(new RecordView(records[0], Mon16, null, 25), changed.Records[0]);
        Assert.Equal(49, deleted.Records.Count);
        Assert.DoesNotContain(deleted.Records, r => r.Id == records[1]);
        Assert.Equal(records[0], deleted.Records[0].Id);
    }

    [Fact]
    public async Task FR_003_a_change_keeps_the_records_date_identifier_and_place_in_the_stored_order()
    {
        var goal = await GoalAsync(Pages20ThreeTimesAWeek);
        var first = await RecordAtAsync(goal, Mon16, Amount(10), Mon16TenOClock);
        var second = await RecordAtAsync(goal, Mon16, Amount(20), Mon16TenOClock);
        var third = await RecordAtAsync(goal, Mon16, Amount(30), Mon16TenOClock);

        var entry = Changed(await _world.ChangeAsync(goal, second, Amount(5)), second);

        Assert.Equal(
            [new RecordView(first, Mon16, null, 10), new RecordView(second, Mon16, null, 5), new RecordView(third, Mon16, null, 30)],
            entry.Records);
        Assert.Equal(new Progress(ProgressKind.RecordsReachingAmount, 1, 3), entry.Progress);
    }
}
