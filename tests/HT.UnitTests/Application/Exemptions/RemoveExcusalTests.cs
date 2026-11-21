using HT.Application.Exemptions;
using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using static HT.UnitTests.Application.Exemptions.ExemptionWorld;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Exemptions;

/// <summary>
/// Removing an excusal (FR-003, FR-004, FR-024, US1-AS8, AS9, SC-003; T045) over the fakes of
/// <see cref="ExemptionWorld"/>. The goal "Yoga, 3 days a week" (Build, yes/no, Monday, Wednesday and Friday) starts on
/// Monday 2 November 2026 and is created at 08:00Z that day, in UTC: Wednesday 11 November ends at
/// 2026-11-12T00:00:00Z and its grace period ends at 2026-11-14T00:00:00Z. The time-zone cases use the instants of
/// tasks.md "Conventions": in Europe/Kyiv, Friday 23 October 2026 ends at 2026-10-23T21:00:00Z and its grace period
/// ends at 2026-10-25T21:00:00Z (Sunday 23:00 local, after the change back); Friday 26 March 2027 ends at
/// 2027-03-26T22:00:00Z and its grace period ends at 2027-03-28T22:00:00Z (Monday 01:00 local, after the change
/// forward); in Europe/Berlin, Friday 23 October 2026 ends at 2026-10-23T22:00:00Z and its grace period ends at
/// 2026-10-25T22:00:00Z. Every instant is worked out by hand.
/// </summary>
public sealed class RemoveExcusalTests
{
    private const string NotExcused = "This commitment is not excused.";
    private const string TooLate = "This excusal can no longer be removed.";
    private const string CommitmentExcused = "This commitment is excused. Remove the excusal to record it.";

    private static readonly DateOnly Mon2 = Date(2026, 11, 2);
    private static readonly DateOnly Wed4 = Date(2026, 11, 4);
    private static readonly DateOnly Fri6 = Date(2026, 11, 6);
    private static readonly DateOnly Mon9 = Date(2026, 11, 9);
    private static readonly DateOnly Tue10 = Date(2026, 11, 10);
    private static readonly DateOnly Wed11 = Date(2026, 11, 11);
    private static readonly DateOnly Fri13 = Date(2026, 11, 13);

    /// <summary>08:00Z on Monday 2 November 2026, when the November goals are created.</summary>
    private static readonly DateTimeOffset Mon2Morning = Utc("2026-11-02T08:00:00Z");

    /// <summary>10:00Z on Thursday 12 November: Wednesday 11 is Pending (grace end 2026-11-14T00:00:00Z).</summary>
    private static readonly DateTimeOffset Thu12TenOClock = Utc("2026-11-12T10:00:00Z");

    private readonly ExemptionWorld _world = new();

    /// <summary>"Yoga, 3 days a week": a yes/no Build goal on Monday, Wednesday and Friday (commitment-based).</summary>
    private static GoalTargetFields YogaOnMonWedFri => YesNoDays(BuildOrReduce.Build, MonWedFri);

    private Task<GoalId> GoalAsync() => _world.CreateGoalAsync(YogaOnMonWedFri, Mon2, Mon2Morning);

    private static void AssertRemoved(RemoveExcusalResult result, EvaluationState state)
    {
        Assert.Equal(RemoveExcusalOutcome.Removed, result.Outcome);
        Assert.Equal("excusal removed", result.Message);
        Assert.Equal(state, result.State);
    }

    private static void AssertRefused(RemoveExcusalResult result, RemoveExcusalOutcome outcome, string message)
    {
        Assert.Equal(outcome, result.Outcome);
        Assert.Equal(message, result.Message);
        Assert.Null(result.State);
    }

    private static void AssertExcused(ExcuseResult result) => Assert.Equal(ExcuseOutcome.Excused, result.Outcome);

    /// <summary>The goal with Wednesday 11 excused with "Ill" at Thursday 12, 10:00Z.</summary>
    private async Task<GoalId> GoalWithWed11ExcusedAsync()
    {
        var goal = await GoalAsync();
        _world.At(Thu12TenOClock);
        AssertExcused(await _world.ExcuseAsync(goal, Wed11, "Ill"));
        return goal;
    }

    // ---- US1-AS8 and SC-003 ----

    [Fact]
    public async Task US1_AS8_removing_one_second_before_the_grace_end_makes_it_Pending_again_and_recordable()
    {
        var goal = await GoalWithWed11ExcusedAsync();
        _world.At(Utc("2026-11-13T23:59:59Z"));

        var result = await _world.RemoveExcusalAsync(goal, Wed11);

        AssertRemoved(result, EvaluationState.Pending);
        Assert.Equal(ExcusalTexts.Removed, result.Message);
        Assert.Empty(_world.Records.ExemptionsOfGoal(goal).Excusals);
        var entry = await _world.EntryAsync(goal, Wed11);
        Assert.Equal(EvaluationState.Pending, entry.State);
        Assert.Null(entry.ExcusalReason);
        Assert.Contains(await _world.RecordableAsync(goal), recordable => recordable.FirstDate == Wed11);
        Assert.Equal(RecordWriteOutcome.Recorded, (await _world.RecordAsync(goal, Wed11, Yes())).Outcome);
    }

    [Theory]
    [InlineData("2026-11-14T00:00:00Z")]
    [InlineData("2026-11-14T00:00:01Z")]
    public async Task US1_AS8_removing_at_the_grace_end_or_after_it_is_refused_and_it_stays_Excused(string at)
    {
        var goal = await GoalWithWed11ExcusedAsync();
        _world.At(Utc(at));

        AssertRefused(await _world.RemoveExcusalAsync(goal, Wed11), RemoveExcusalOutcome.TooLate, TooLate);
        Assert.Equal(ExcusalTexts.TooLateToRemove, TooLate);
        var entry = await _world.EntryAsync(goal, Wed11);
        Assert.Equal(EvaluationState.Excused, entry.State);
        Assert.Equal("Ill", entry.ExcusalReason);
        Assert.False(entry.CanRemoveExcusal);
    }

    [Fact]
    public async Task FR_004_removing_the_excusal_of_a_Due_commitment_gives_Due()
    {
        var goal = await GoalAsync();

        // 07:00Z and 08:00Z on Friday 13 November: Friday 13 is Due.
        _world.At(Utc("2026-11-13T07:00:00Z"));
        AssertExcused(await _world.ExcuseAsync(goal, Fri13));
        _world.At(Utc("2026-11-13T08:00:00Z"));

        AssertRemoved(await _world.RemoveExcusalAsync(goal, Fri13), EvaluationState.Due);
    }

    [Fact]
    public async Task FR_004_removing_an_excusal_under_a_pause_gives_Paused_and_keeps_the_pause()
    {
        var goal = await GoalAsync();
        _world.SeedExcusal(goal, Wed11, "Ill");
        _world.SeedPause(goal, Wed11, Fri13);
        _world.At(Thu12TenOClock);

        AssertRemoved(await _world.RemoveExcusalAsync(goal, Wed11), EvaluationState.Paused);
        var left = _world.Records.ExemptionsOfGoal(goal);
        Assert.Empty(left.Excusals);
        Assert.Single(left.Pauses);
    }

    [Fact]
    public async Task FR_004_4_a_commitment_that_is_not_excused_is_refused()
    {
        var goal = await GoalAsync();
        _world.At(Thu12TenOClock);

        AssertRefused(await _world.RemoveExcusalAsync(goal, Wed11), RemoveExcusalOutcome.NotExcused, NotExcused);
        AssertRefused(await _world.RemoveExcusalAsync(goal, Tue10), RemoveExcusalOutcome.NotExcused, NotExcused);
        Assert.Equal(ExcusalTexts.NotExcused, NotExcused);
        Assert.DoesNotContain(nameof(IRecordStore.RemoveExcusalAsync), _world.Records.Calls);
    }

    [Fact]
    public async Task FR_004_order_an_excusal_that_is_not_there_after_the_grace_end_gives_step_4()
    {
        var goal = await GoalAsync();
        _world.At(Utc("2026-11-14T00:00:00Z"));

        AssertRefused(await _world.RemoveExcusalAsync(goal, Wed11), RemoveExcusalOutcome.NotExcused, NotExcused);
    }

    [Fact]
    public async Task FR_004_a_concurrent_removal_gives_This_commitment_is_not_excused()
    {
        var goal = await GoalWithWed11ExcusedAsync();
        _world.Records.BeforeNextWrite = () => Assert.Equal(
            RemoveExcusalOutcome.Removed,
            _world.Excusals.RemoveExcusalAsync(_world.User, goal, Wed11, CancellationToken.None).GetAwaiter().GetResult().Outcome);

        AssertRefused(await _world.RemoveExcusalAsync(goal, Wed11), RemoveExcusalOutcome.NotExcused, NotExcused);
    }

    [Fact]
    public async Task FR_004_access_another_users_goal_gives_This_goal_does_not_exist_and_keeps_the_excusal()
    {
        var goal = await GoalWithWed11ExcusedAsync();

        AssertRefused(
            await _world.RemoveExcusalAsync(goal, Wed11, _world.OtherUser), RemoveExcusalOutcome.NotFound, "This goal does not exist.");
        AssertRefused(
            await _world.RemoveExcusalAsync(goal, Wed11, _world.Administrator),
            RemoveExcusalOutcome.NotForAdministrators,
            "Goals are for registered users.");
        AssertRefused(
            await _world.RemoveExcusalAsync(goal, Wed11, ActingUser.Anonymous), RemoveExcusalOutcome.NotSignedIn, "You are not signed in.");
        Assert.NotNull(_world.Records.ExemptionsOfGoal(goal).ExcusalOf(Wed11));
    }

    // ---- US1-AS9 ----

    [Fact]
    public async Task US1_AS9_recording_an_excused_commitment_is_refused_and_stores_nothing()
    {
        var goal = await GoalWithWed11ExcusedAsync();

        var result = await _world.RecordAsync(goal, Wed11, Yes("Did it after all."));

        Assert.Equal(RecordWriteOutcome.CommitmentExcused, result.Outcome);
        Assert.Equal(CommitmentExcused, result.Message);
        Assert.Empty(_world.Records.RecordsOfGoal(goal));
        Assert.Empty(_world.Records.NotesOfGoal(goal));
    }

    // ---- the grace end in other time zones (Conventions) ----

    [Theory]
    [InlineData("Europe/Kyiv", "2026-10-22T08:00:00Z", 2026, 10, 23, "2026-10-24T10:00:00Z", "2026-10-25T20:59:59Z", "2026-10-25T21:00:00Z")]
    [InlineData("Europe/Kyiv", "2027-03-25T08:00:00Z", 2027, 3, 26, "2027-03-27T10:00:00Z", "2027-03-28T21:59:59Z", "2027-03-28T22:00:00Z")]
    [InlineData("Europe/Berlin", "2026-10-22T08:00:00Z", 2026, 10, 23, "2026-10-24T10:00:00Z", "2026-10-25T21:59:59Z", "2026-10-25T22:00:00Z")]
    public async Task SC_003_the_grace_end_of_a_Friday_around_a_clock_change_is_48_hours_after_its_end(
        string zone, string createdAt, int year, int month, int day, string pendingAt, string lastSecond, string graceEnd)
    {
        var world = new ExemptionWorld(Utc("2026-10-01T00:00:00Z"));
        var user = world.UserIn(zone);
        var friday = Date(year, month, day);
        var goal = await world.CreateGoalAsync(YogaOnMonWedFri, friday, Utc(createdAt), user);

        world.At(Utc(pendingAt));
        AssertExcused(await world.ExcuseAsync(goal, friday, "Ill", user));

        world.At(Utc(lastSecond));
        AssertRemoved(await world.RemoveExcusalAsync(goal, friday, user), EvaluationState.Pending);
        AssertExcused(await world.ExcuseAsync(goal, friday, "Ill", user));

        world.At(Utc(graceEnd));
        AssertRefused(await world.RemoveExcusalAsync(goal, friday, user), RemoveExcusalOutcome.TooLate, TooLate);
        Assert.NotNull(world.Records.ExemptionsOfGoal(goal).ExcusalOf(friday));
    }

    // ---- FR-024 ----

    [Fact]
    public async Task FR_024_a_commitment_excused_removed_and_then_recorded_reports_as_one_recorded_on_time()
    {
        var onTime = await GoalAsync();
        var late = await GoalAsync();
        foreach (var goal in new[] { onTime, late })
        {
            _world.SeedAnswer(goal, Mon2, YesNoAnswer.Yes);
            _world.SeedAnswer(goal, Wed4, YesNoAnswer.Yes);
            _world.SeedAnswer(goal, Fri6, YesNoAnswer.Yes);
            _world.SeedAnswer(goal, Mon9, YesNoAnswer.Yes);
        }

        // 10:00Z on Wednesday 11: Wednesday 11 is Due.
        _world.At(Utc("2026-11-11T10:00:00Z"));
        Assert.Equal(RecordWriteOutcome.Recorded, (await _world.RecordAsync(onTime, Wed11, Yes())).Outcome);

        _world.At(Thu12TenOClock);
        AssertExcused(await _world.ExcuseAsync(late, Wed11, "Ill"));
        _world.At(Utc("2026-11-12T11:00:00Z"));
        AssertRemoved(await _world.RemoveExcusalAsync(late, Wed11), EvaluationState.Pending);
        _world.At(Utc("2026-11-12T12:00:00Z"));
        Assert.Equal(RecordWriteOutcome.Recorded, (await _world.RecordAsync(late, Wed11, Yes())).Outcome);

        // Saturday 14 at 00:00Z: Monday 2 to Wednesday 11 are Met, Friday 13 is Pending.
        _world.At(Utc("2026-11-14T00:00:00Z"));
        var onTimeReport = await _world.ReportAsync(onTime);
        var lateReport = await _world.ReportAsync(late);
        Assert.Equal("100% (5 of 5)", onTimeReport.Stats.ConsistencyText);
        Assert.Equal(onTimeReport.Stats, lateReport.Stats);
        Assert.Equal(
            (await _world.HistoryAsync(onTime, Mon2, Fri13)).Select(entry => (entry.FirstDate, entry.State, entry.Progress)),
            (await _world.HistoryAsync(late, Mon2, Fri13)).Select(entry => (entry.FirstDate, entry.State, entry.Progress)));
    }
}
