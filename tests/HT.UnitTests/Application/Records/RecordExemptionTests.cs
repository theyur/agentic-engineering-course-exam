using HT.Application.Exemptions;
using HT.Application.Records;
using HT.Domain.Evaluation;
using HT.Domain.Exemptions;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using HT.UnitTests.Application.Exemptions;
using static HT.UnitTests.Application.Exemptions.ExemptionWorld;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Records;

/// <summary>
/// Slice 7's record checks (FR-005 steps 4a to 4c, FR-009; plan "Spec discrepancies" 2; T038) over the fakes of
/// <see cref="ExemptionWorld"/>, with excusals and pauses seeded on the fake store. The user has no stored time zone, so
/// the goals are in UTC: a day runs from 00:00Z to 00:00Z the next day and its grace period ends 48 hours later; a week
/// runs from Monday 00:00Z to the next Monday 00:00Z. The goals start on Monday 9 November 2026 and are created at 08:00Z
/// that day. Every instant is worked out by hand.
/// </summary>
public sealed class RecordExemptionTests
{
    private const string NothingScheduled = "Nothing is scheduled on this date.";
    private const string DayNotBegun = "You cannot record a day that has not begun.";
    private const string DatePaused = "This goal is paused on this date.";
    private const string WeekPaused = "This week is paused.";
    private const string CommitmentExcused = "This commitment is excused. Remove the excusal to record it.";

    private static readonly DateOnly Mon9 = Date(2026, 11, 9);
    private static readonly DateOnly Tue10 = Date(2026, 11, 10);
    private static readonly DateOnly Wed11 = Date(2026, 11, 11);
    private static readonly DateOnly Thu12 = Date(2026, 11, 12);
    private static readonly DateOnly Fri13 = Date(2026, 11, 13);
    private static readonly DateOnly Sat14 = Date(2026, 11, 14);
    private static readonly DateOnly Sun15 = Date(2026, 11, 15);

    /// <summary>08:00Z on Monday 9 November 2026, when every goal is created.</summary>
    private static readonly DateTimeOffset Mon9Morning = Utc("2026-11-09T08:00:00Z");

    /// <summary>10:00Z on Wednesday 11 November: Wednesday 11 is Due, Monday 9 is Pending (grace end 2026-11-12T00:00:00Z).</summary>
    private static readonly DateTimeOffset Wed11TenOClock = Utc("2026-11-11T10:00:00Z");

    /// <summary>
    /// 10:00Z on Thursday 12 November: on a daily goal Thursday 12 is Due; Tuesday 10 (grace end 2026-11-13T00:00:00Z)
    /// and Wednesday 11 (grace end 2026-11-14T00:00:00Z) are Pending; Monday 9 (grace end 2026-11-12T00:00:00Z) is final.
    /// </summary>
    private static readonly DateTimeOffset Thu12TenOClock = Utc("2026-11-12T10:00:00Z");

    private readonly ExemptionWorld _world = new();

    /// <summary>"Yoga, 3 days a week": a yes/no Build goal on Monday, Wednesday and Friday (commitment-based).</summary>
    private static GoalTargetFields YogaOnMonWedFri => YesNoDays(BuildOrReduce.Build, MonWedFri);

    /// <summary>A duration Build goal on all seven days (commitment-based).</summary>
    private static GoalTargetFields Reading20MinDaily => DurationDays(20, DurationUnit.Minutes, BuildOrReduce.Build, AllDays);

    /// <summary>A yes/no Build goal of three times a week (period-based, per week).</summary>
    private static GoalTargetFields YogaThreeTimesAWeek => YesNoTimes(3, SchedulePeriod.Week);

    /// <summary>A count Reduce goal of at most 2 cups a day (period-based, per day).</summary>
    private static GoalTargetFields CoffeeAtMost2CupsADay => CountTimes(2, "cups", 1, SchedulePeriod.Day, BuildOrReduce.Reduce);

    private Task<GoalId> GoalAsync(GoalTargetFields target) => _world.CreateGoalAsync(target, Mon9, Mon9Morning);

    private static void AssertRefused(RecordWriteResult result, RecordWriteOutcome outcome, string message)
    {
        Assert.Equal(outcome, result.Outcome);
        Assert.Equal(message, result.Message);
        Assert.Null(result.RecordId);
        Assert.Null(result.Entry);
        Assert.False(result.IsCorrection);
    }

    private void AssertNothingStored(GoalId goal)
    {
        Assert.Empty(_world.Records.RecordsOfGoal(goal));
        Assert.Empty(_world.Records.NotesOfGoal(goal));
    }

    private static HistoryEntry Written(RecordWriteResult result, RecordWriteOutcome outcome)
    {
        Assert.Equal(outcome, result.Outcome);
        Assert.Null(result.Message);
        return result.Entry!;
    }

    // ---- 4a, 4b and 4c with their texts ----

    [Fact]
    public async Task FR_005_4a_a_commitment_in_a_pause_is_refused_and_nothing_is_stored()
    {
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.SeedPause(goal, Wed11, Fri13);
        _world.At(Wed11TenOClock);

        var result = await _world.RecordAsync(goal, Wed11, Yes("Felt good."));

        AssertRefused(result, RecordWriteOutcome.DatePaused, DatePaused);
        Assert.Equal(PauseTexts.DatePaused, result.Message);
        AssertNothingStored(goal);
    }

    [Fact]
    public async Task FR_005_4a_a_day_of_a_per_day_goal_in_a_pause_is_refused()
    {
        var goal = await GoalAsync(CoffeeAtMost2CupsADay);
        _world.SeedPause(goal, Wed11, Wed11);
        _world.At(Wed11TenOClock);

        AssertRefused(await _world.RecordAsync(goal, Wed11, Amount(1)), RecordWriteOutcome.DatePaused, DatePaused);
        AssertNothingStored(goal);
    }

    [Fact]
    public async Task FR_005_4b_a_date_of_a_Paused_week_that_is_not_in_the_pause_gives_This_week_is_paused()
    {
        var goal = await GoalAsync(YogaThreeTimesAWeek);
        _world.SeedPause(goal, Fri13, Sun15);
        _world.At(Wed11TenOClock);

        var result = await _world.RecordAsync(goal, Wed11, Yes());

        AssertRefused(result, RecordWriteOutcome.WeekPaused, WeekPaused);
        Assert.Equal(PauseTexts.WeekPaused, result.Message);
        AssertNothingStored(goal);
    }

    [Fact]
    public async Task FR_005_4a_a_date_of_a_Paused_week_that_is_itself_in_the_pause_gives_the_pause_text()
    {
        var goal = await GoalAsync(YogaThreeTimesAWeek);
        _world.SeedPause(goal, Fri13, Sun15);

        // Friday 13 begins at 2026-11-13T00:00:00Z.
        _world.At(Utc("2026-11-13T10:00:00Z"));

        AssertRefused(await _world.RecordAsync(goal, Fri13, Yes()), RecordWriteOutcome.DatePaused, DatePaused);
        AssertNothingStored(goal);
    }

    [Fact]
    public async Task FR_005_4c_an_excused_commitment_is_refused_and_nothing_is_stored()
    {
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.SeedExcusal(goal, Wed11, "Ill");
        _world.At(Wed11TenOClock);

        var result = await _world.RecordAsync(goal, Wed11, Yes("Felt good."));

        AssertRefused(result, RecordWriteOutcome.CommitmentExcused, CommitmentExcused);
        Assert.Equal(ExcusalTexts.CommitmentExcused, result.Message);
        AssertNothingStored(goal);
    }

    [Fact]
    public async Task FR_005_a_commitment_that_is_excused_and_paused_gives_4a_as_the_pause_wins()
    {
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.SeedExcusal(goal, Wed11);
        _world.SeedPause(goal, Wed11, Wed11);
        _world.At(Wed11TenOClock);

        AssertRefused(await _world.RecordAsync(goal, Wed11, Yes()), RecordWriteOutcome.DatePaused, DatePaused);
        AssertNothingStored(goal);
    }

    // ---- the order around 4a to 4c ----

    [Fact]
    public async Task FR_005_an_invalid_value_on_a_paused_date_gives_the_pause_text_as_4a_comes_before_the_value()
    {
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.SeedPause(goal, Wed11, Fri13);
        _world.At(Wed11TenOClock);

        AssertRefused(await _world.RecordAsync(goal, Wed11, new RecordInput()), RecordWriteOutcome.DatePaused, DatePaused);
    }

    [Fact]
    public async Task FR_005_a_note_too_long_on_an_excused_commitment_gives_the_excusal_text_as_4c_comes_before_the_note()
    {
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.SeedExcusal(goal, Wed11);
        _world.At(Wed11TenOClock);

        AssertRefused(
            await _world.RecordAsync(goal, Wed11, Yes(new string('a', 501))), RecordWriteOutcome.CommitmentExcused, CommitmentExcused);
    }

    [Fact]
    public async Task FR_005_a_paused_date_that_has_not_begun_gives_the_not_begun_text_as_4a_comes_after_it()
    {
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.SeedPause(goal, Wed11, Fri13);
        _world.At(Wed11TenOClock);

        AssertRefused(await _world.RecordAsync(goal, Fri13, Yes()), RecordWriteOutcome.DayNotBegun, DayNotBegun);
        Assert.Empty(_world.Records.Calls);
    }

    [Fact]
    public async Task FR_005_a_paused_date_with_nothing_scheduled_gives_the_nothing_scheduled_text()
    {
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.SeedPause(goal, Mon9, Fri13);
        _world.At(Wed11TenOClock);

        AssertRefused(await _world.RecordAsync(goal, Tue10, Yes()), RecordWriteOutcome.NothingScheduled, NothingScheduled);
        Assert.Empty(_world.Records.Calls);
    }

    [Fact]
    public async Task FR_005_a_date_without_an_exemption_is_recorded_as_before_after_one_exemption_read()
    {
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.SeedExcusal(goal, Mon9);
        _world.SeedPause(goal, Fri13, Fri13);
        _world.At(Wed11TenOClock);

        var entry = Written(await _world.RecordAsync(goal, Wed11, Yes()), RecordWriteOutcome.Recorded);

        Assert.Equal(EvaluationState.Due, entry.State);
        Assert.Equal(YesNoAnswer.Yes, Assert.Single(entry.Records).Answer);
        Assert.Equal([nameof(IRecordStore.ReadExemptionsAsync), nameof(IRecordStore.AddAsync)], _world.Records.Calls);
    }

    // ---- a concurrent excusal or pause: the store's Exempt ----

    [Fact]
    public async Task FR_005_an_excusal_added_after_the_checks_gives_the_excusal_text_and_nothing_is_stored()
    {
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.At(Wed11TenOClock);
        _world.Records.BeforeNextWrite = () => _world.SeedExcusal(goal, Wed11);

        var result = await _world.RecordAsync(goal, Wed11, Yes("Felt good."));

        AssertRefused(result, RecordWriteOutcome.CommitmentExcused, CommitmentExcused);
        AssertNothingStored(goal);
        Assert.Equal(
            [nameof(IRecordStore.ReadExemptionsAsync), nameof(IRecordStore.AddAsync), nameof(IRecordStore.ReadExemptionsAsync)],
            _world.Records.Calls);
    }

    [Fact]
    public async Task FR_005_a_pause_added_after_the_checks_gives_the_pause_text_and_nothing_is_stored()
    {
        var goal = await GoalAsync(Reading20MinDaily);
        _world.At(Wed11TenOClock);
        _world.Records.BeforeNextWrite = () => _world.SeedPause(goal, Wed11, Fri13);

        AssertRefused(await _world.RecordAsync(goal, Wed11, Minutes(20)), RecordWriteOutcome.DatePaused, DatePaused);
        AssertNothingStored(goal);
    }

    [Fact]
    public async Task FR_005_a_pause_later_in_the_week_added_after_the_checks_gives_This_week_is_paused()
    {
        var goal = await GoalAsync(YogaThreeTimesAWeek);
        _world.At(Wed11TenOClock);
        _world.Records.BeforeNextWrite = () => _world.SeedPause(goal, Fri13, Sun15);

        AssertRefused(await _world.RecordAsync(goal, Wed11, Yes()), RecordWriteOutcome.WeekPaused, WeekPaused);
        AssertNothingStored(goal);
    }

    [Fact]
    public async Task FR_005_an_excusal_added_and_removed_again_around_the_write_gives_the_excusal_text_for_a_commitment()
    {
        // The store refuses the write as Exempt, and the excusal is gone again when the use case reads the exemptions a
        // second time: a commitment gets step 4c's text, as only a commitment can be excused. Nothing is stored.
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.At(Wed11TenOClock);
        var owner = _world.User.AccountId!.Value;
        _world.Records.BeforeNextWrite = () => _world.SeedExcusal(goal, Wed11);
        _world.Records.AfterNextAdd = () =>
            Assert.Equal(
                ExemptionStoreWriteOutcome.Written,
                _world.Records.RemoveExcusalAsync(owner, goal, Wed11, CancellationToken.None).GetAwaiter().GetResult().Outcome);

        AssertRefused(await _world.RecordAsync(goal, Wed11, Yes()), RecordWriteOutcome.CommitmentExcused, CommitmentExcused);
        AssertNothingStored(goal);
        Assert.Empty(_world.Records.ExemptionsOfGoal(goal).Excusals);
    }

    [Fact]
    public async Task FR_005_a_pause_added_and_removed_again_around_the_write_gives_the_pause_text_for_a_period()
    {
        // The same for a per-day period, which cannot be excused: step 4a's text.
        var goal = await GoalAsync(CoffeeAtMost2CupsADay);
        _world.At(Wed11TenOClock);
        var owner = _world.User.AccountId!.Value;
        PauseId? pause = null;
        _world.Records.BeforeNextWrite = () => pause = _world.SeedPause(goal, Wed11, Fri13);
        _world.Records.AfterNextAdd = () =>
            Assert.Equal(
                ExemptionStoreWriteOutcome.Written,
                _world.Records.DeletePauseAsync(owner, goal, pause!.Value, CancellationToken.None).GetAwaiter().GetResult().Outcome);

        AssertRefused(await _world.RecordAsync(goal, Wed11, Amount(1)), RecordWriteOutcome.DatePaused, DatePaused);
        AssertNothingStored(goal);
        Assert.Empty(_world.Records.ExemptionsOfGoal(goal).Pauses);
    }

    // ---- what can be recorded now ----

    [Fact]
    public async Task FR_005_FR_009_what_can_be_recorded_now_leaves_out_Excused_and_Paused_entries_and_keeps_the_others()
    {
        // On Thursday 12 at 10:00Z: Monday 9 is Missed (grace end 2026-11-12T00:00:00Z); Tuesday 10 (grace end
        // 2026-11-13T00:00:00Z) and Wednesday 11 (grace end 2026-11-14T00:00:00Z) are Pending; Thursday 12 is Due.
        var goal = await GoalAsync(Reading20MinDaily);
        _world.SeedExcusal(goal, Tue10, "Ill");
        _world.SeedPause(goal, Thu12, Sat14);
        _world.At(Thu12TenOClock);

        var recordable = await _world.RecordableAsync(goal);

        var entry = Assert.Single(recordable);
        Assert.Equal((Wed11, EvaluationState.Pending), (entry.FirstDate, entry.State));
    }

    [Fact]
    public async Task FR_009_a_Paused_week_is_left_out_of_what_can_be_recorded_now()
    {
        var goal = await GoalAsync(YogaThreeTimesAWeek);
        _world.SeedPause(goal, Fri13, Sun15);
        _world.At(Wed11TenOClock);

        Assert.Empty(await _world.RecordableAsync(goal));
    }

    // ---- a record kept on a Paused date ----

    [Fact]
    public async Task FR_009_a_record_kept_on_a_Paused_date_is_returned_with_it_and_can_be_changed_and_deleted_never_as_a_correction()
    {
        // Thursday 12 is recorded while Due, then paused from that day on (a pause from the current date, FR-006).
        var goal = await GoalAsync(Reading20MinDaily);
        _world.At(Thu12TenOClock);
        var record = Written(await _world.RecordAsync(goal, Thu12, Minutes(12)), RecordWriteOutcome.Recorded);
        var recordId = Assert.Single(record.Records).Id;
        _world.SeedPause(goal, Thu12, Sat14);

        var kept = await _world.EntryAsync(goal, Thu12);
        Assert.Equal(EvaluationState.Paused, kept.State);
        Assert.Equal(12, Assert.Single(kept.Records).Amount);
        Assert.Equal(new Progress(ProgressKind.RecordedValue, 12, 20), kept.Progress);

        // Long after Thursday 12's grace end (2026-11-15T00:00:00Z) a change is still no correction, as Paused is not final.
        _world.At(Utc("2026-11-20T10:00:00Z"));
        var changed = await _world.ChangeAsync(goal, recordId, Minutes(25));
        var afterChange = Written(changed, RecordWriteOutcome.Changed);
        Assert.Equal(EvaluationState.Paused, afterChange.State);
        Assert.Equal(new Progress(ProgressKind.RecordedValue, 25, 20), afterChange.Progress);
        Assert.False(afterChange.IsCorrection);
        Assert.False(changed.IsCorrection);
        Assert.Null(afterChange.GraceEnds);

        var deleted = await _world.DeleteAsync(goal, recordId);
        var afterDelete = Written(deleted, RecordWriteOutcome.Deleted);
        Assert.Equal(EvaluationState.Paused, afterDelete.State);
        Assert.Empty(afterDelete.Records);
        Assert.False(afterDelete.IsCorrection);
        Assert.False(deleted.IsCorrection);
        Assert.Empty(_world.Records.RecordsOfGoal(goal));
    }

    [Fact]
    public async Task FR_009_a_record_kept_in_a_Paused_week_is_changed_with_Paused_as_the_state_after_the_change()
    {
        var goal = await GoalAsync(YogaThreeTimesAWeek);
        _world.At(Wed11TenOClock);
        var record = Written(await _world.RecordAsync(goal, Mon9, Yes()), RecordWriteOutcome.Recorded);
        _world.SeedPause(goal, Fri13, Sun15);

        var changed = Written(await _world.ChangeAsync(goal, Assert.Single(record.Records).Id, No()), RecordWriteOutcome.Changed);

        Assert.Equal(EvaluationState.Paused, changed.State);
        Assert.Equal((Mon9, Sun15), (changed.FirstDate, changed.LastDate));
        Assert.Equal(YesNoAnswer.No, Assert.Single(changed.Records).Answer);
        Assert.False(changed.IsCorrection);
    }
}
