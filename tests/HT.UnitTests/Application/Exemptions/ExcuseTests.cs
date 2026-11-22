using HT.Application.Exemptions;
using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.UnitTests.Application.Goals;
using HT.Domain.Records;
using static HT.UnitTests.Application.Exemptions.ExemptionWorld;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Exemptions;

/// <summary>
/// Excusing a commitment (FR-001 to FR-003, US1-AS1 to AS7, AS10, AS11, D-2, D-3, SC-003; T043) over the fakes of
/// <see cref="ExemptionWorld"/>. The goal "Yoga, 3 days a week" (Build, yes/no, Monday, Wednesday and Friday) starts on
/// Monday 2 November 2026 and is created at 08:00Z that day. The user has no stored time zone, so the goal is in UTC: a
/// commitment runs from 00:00Z to 00:00Z the next day and its grace period ends 48 hours later. Wednesday 11 November
/// ends at 2026-11-12T00:00:00Z and its grace period ends at 2026-11-14T00:00:00Z; Monday 9 November's grace period ends
/// at 2026-11-12T00:00:00Z. Every instant is worked out by hand.
/// </summary>
public sealed class ExcuseTests
{
    private const string NotSignedIn = "You are not signed in.";
    private const string NotForAdministrators = "Goals are for registered users.";
    private const string GoalNotFound = "This goal does not exist.";
    private const string NotACommitment = "Only commitments can be excused. This goal is evaluated by period.";
    private const string NothingScheduled = "Nothing is scheduled on this date.";
    private const string DatePaused = "This goal is paused on this date.";
    private const string AlreadyExcused = "This commitment is already excused.";
    private const string NotBegun = "You cannot excuse a commitment that has not begun.";
    private const string Final = "This commitment is final and can no longer be excused.";
    private const string HasRecord = "This commitment has a record. Delete the record to excuse it.";
    private const string ReasonTooLong = "Keep the reason to 200 characters or fewer.";

    private static readonly DateOnly Sun1 = Date(2026, 11, 1);
    private static readonly DateOnly Mon2 = Date(2026, 11, 2);
    private static readonly DateOnly Wed4 = Date(2026, 11, 4);
    private static readonly DateOnly Fri6 = Date(2026, 11, 6);
    private static readonly DateOnly Mon9 = Date(2026, 11, 9);
    private static readonly DateOnly Tue10 = Date(2026, 11, 10);
    private static readonly DateOnly Wed11 = Date(2026, 11, 11);
    private static readonly DateOnly Fri13 = Date(2026, 11, 13);

    /// <summary>08:00Z on Monday 2 November 2026, when every goal is created.</summary>
    private static readonly DateTimeOffset Mon2Morning = Utc("2026-11-02T08:00:00Z");

    /// <summary>10:00Z on Wednesday 11 November: Wednesday 11 is Due.</summary>
    private static readonly DateTimeOffset Wed11TenOClock = Utc("2026-11-11T10:00:00Z");

    /// <summary>
    /// 10:00Z on Thursday 12 November: Wednesday 11 is Pending (grace end 2026-11-14T00:00:00Z), Monday 9 is final
    /// (grace end 2026-11-12T00:00:00Z) and Friday 13 is Upcoming (it begins at 2026-11-13T00:00:00Z).
    /// </summary>
    private static readonly DateTimeOffset Thu12TenOClock = Utc("2026-11-12T10:00:00Z");

    private readonly ExemptionWorld _world = new();

    /// <summary>"Yoga, 3 days a week": a yes/no Build goal on Monday, Wednesday and Friday (commitment-based).</summary>
    private static GoalTargetFields YogaOnMonWedFri => YesNoDays(BuildOrReduce.Build, MonWedFri);

    /// <summary>"Yoga, 3 times a week": a yes/no Build goal of three times a week (period-based, per week).</summary>
    private static GoalTargetFields YogaThreeTimesAWeek => YesNoTimes(3, SchedulePeriod.Week);

    /// <summary>A yes/no Build goal of once a day (period-based, per day).</summary>
    private static GoalTargetFields YogaOnceADay => YesNoTimes(1, SchedulePeriod.Day);

    private Task<GoalId> GoalAsync(GoalTargetFields target) => _world.CreateGoalAsync(target, Mon2, Mon2Morning);

    private static void AssertRefused(ExcuseResult result, ExcuseOutcome outcome, string message)
    {
        Assert.Equal(outcome, result.Outcome);
        Assert.Equal(message, result.Message);
        Assert.Null(result.Date);
    }

    private static void AssertExcused(ExcuseResult result, DateOnly date)
    {
        Assert.Equal(ExcuseOutcome.Excused, result.Outcome);
        Assert.Equal("excused", result.Message);
        Assert.Equal(date, result.Date);
    }

    private void AssertNoExcusal(GoalId goal) => Assert.Empty(_world.Records.ExemptionsOfGoal(goal).Excusals);

    // ---- access (FR-002 steps 1 to 3) ----

    [Fact]
    public async Task FR_002_1_an_anonymous_visitor_is_refused_with_no_store_call()
    {
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.At(Thu12TenOClock);

        AssertRefused(await _world.ExcuseAsync(goal, Wed11, "Ill", ActingUser.Anonymous), ExcuseOutcome.NotSignedIn, NotSignedIn);
        Assert.Empty(_world.Records.Calls);
        AssertNoExcusal(goal);
    }

    [Fact]
    public async Task FR_002_2_an_administrator_is_refused_with_no_store_call()
    {
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.At(Thu12TenOClock);

        AssertRefused(
            await _world.ExcuseAsync(goal, Wed11, "Ill", _world.Administrator), ExcuseOutcome.NotForAdministrators, NotForAdministrators);
        Assert.Empty(_world.Records.Calls);
        AssertNoExcusal(goal);
    }

    [Fact]
    public async Task FR_002_3_an_account_that_is_not_stored_is_refused_as_not_signed_in()
    {
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.At(Thu12TenOClock);
        var gone = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.RegisteredUser);

        AssertRefused(await _world.ExcuseAsync(goal, Wed11, "Ill", gone), ExcuseOutcome.NotSignedIn, NotSignedIn);
        Assert.Empty(_world.Records.Calls);
        AssertNoExcusal(goal);
    }

    [Fact]
    public async Task FR_002_3_another_users_goal_and_no_goal_give_This_goal_does_not_exist()
    {
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.At(Thu12TenOClock);

        AssertRefused(await _world.ExcuseAsync(goal, Wed11, "Ill", _world.OtherUser), ExcuseOutcome.NotFound, GoalNotFound);
        AssertRefused(await _world.ExcuseAsync(new GoalId(Guid.NewGuid()), Wed11, "Ill"), ExcuseOutcome.NotFound, GoalNotFound);
        Assert.Empty(_world.Records.Calls);
        AssertNoExcusal(goal);
    }

    // ---- the scenarios ----

    [Fact]
    public async Task US1_AS1_a_Pending_commitment_is_excused_with_its_reason_and_leaves_what_can_be_recorded_now()
    {
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.At(Thu12TenOClock);
        Assert.Contains(await _world.RecordableAsync(goal), entry => entry.FirstDate == Wed11);

        var result = await _world.ExcuseAsync(goal, Wed11, "Ill");

        AssertExcused(result, Wed11);
        Assert.Equal(ExcusalTexts.Excused, result.Message);
        var entry = await _world.EntryAsync(goal, Wed11);
        Assert.Equal(EvaluationState.Excused, entry.State);
        Assert.Equal("Ill", entry.ExcusalReason);
        Assert.DoesNotContain(await _world.RecordableAsync(goal), recordable => recordable.FirstDate == Wed11);
    }

    [Fact]
    public async Task US1_AS2_a_Due_commitment_is_excused_without_a_reason()
    {
        var goal = await GoalAsync(YogaOnMonWedFri);

        // 07:00Z on Friday 13 November: Friday 13 is Due.
        _world.At(Utc("2026-11-13T07:00:00Z"));

        AssertExcused(await _world.ExcuseAsync(goal, Fri13), Fri13);
        var entry = await _world.EntryAsync(goal, Fri13);
        Assert.Equal(EvaluationState.Excused, entry.State);
        Assert.Null(entry.ExcusalReason);
        Assert.Null(_world.Records.ExemptionsOfGoal(goal).ExcusalOf(Fri13)!.Reason);
    }

    [Fact]
    public async Task US1_AS3_an_excused_commitment_stays_Excused_after_its_grace_end_and_a_year_later_and_is_not_counted()
    {
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.SeedAnswer(goal, Mon2, YesNoAnswer.Yes);
        _world.SeedAnswer(goal, Wed4, YesNoAnswer.Yes);
        _world.SeedAnswer(goal, Fri6, YesNoAnswer.Yes);
        _world.SeedAnswer(goal, Mon9, YesNoAnswer.Yes);
        _world.At(Thu12TenOClock);
        AssertExcused(await _world.ExcuseAsync(goal, Wed11, "Ill"), Wed11);

        _world.At(Utc("2026-11-14T00:00:00Z"));
        Assert.Equal(EvaluationState.Excused, (await _world.EntryAsync(goal, Wed11)).State);

        // Monday 2 to Monday 9 are Met, Wednesday 11 is Excused and left out, Friday 13 is Pending: 4 of 4, not 4 of 5.
        var report = await _world.ReportAsync(goal);
        Assert.Equal("100% (4 of 4)", report.Stats.ConsistencyText);
        Assert.Equal(4, report.Stats.CurrentStreak);

        _world.At(Utc("2027-11-14T00:00:00Z"));
        Assert.Equal(EvaluationState.Excused, (await _world.EntryAsync(goal, Wed11)).State);
    }

    [Fact]
    public async Task US1_AS4_a_Missed_commitment_is_refused_as_final_and_stays_Missed()
    {
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.At(Thu12TenOClock);

        AssertRefused(await _world.ExcuseAsync(goal, Mon9, "Ill"), ExcuseOutcome.Final, Final);
        Assert.Equal(EvaluationState.Missed, (await _world.EntryAsync(goal, Mon9)).State);
        AssertNoExcusal(goal);
    }

    [Fact]
    public async Task US1_AS5_an_Upcoming_commitment_is_refused_as_not_begun()
    {
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.At(Thu12TenOClock);

        AssertRefused(await _world.ExcuseAsync(goal, Fri13), ExcuseOutcome.NotBegun, NotBegun);
        AssertNoExcusal(goal);
    }

    [Fact]
    public async Task US1_AS6_a_period_based_goal_is_refused_and_nothing_changes()
    {
        var goal = await GoalAsync(YogaThreeTimesAWeek);
        _world.At(Thu12TenOClock);

        AssertRefused(await _world.ExcuseAsync(goal, Wed11, "Ill"), ExcuseOutcome.NotACommitment, NotACommitment);
        AssertRefused(await _world.ExcuseAsync(goal, Mon9), ExcuseOutcome.NotACommitment, NotACommitment);
        AssertNoExcusal(goal);
        Assert.DoesNotContain(nameof(IRecordStore.ExcuseAsync), _world.Records.Calls);
    }

    [Fact]
    public async Task US1_AS7_a_date_with_no_commitment_gives_Nothing_is_scheduled()
    {
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.At(Thu12TenOClock);

        AssertRefused(await _world.ExcuseAsync(goal, Tue10), ExcuseOutcome.NothingScheduled, NothingScheduled);
        Assert.Equal(RecordTexts.NothingScheduled, NothingScheduled);
        AssertNoExcusal(goal);
    }

    [Fact]
    public async Task US1_AS10_a_Pending_commitment_with_a_record_is_refused_then_excused_after_the_record_is_deleted()
    {
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.At(Wed11TenOClock);
        var recorded = await _world.RecordAsync(goal, Wed11, No());
        Assert.Equal(RecordWriteOutcome.Recorded, recorded.Outcome);
        _world.At(Thu12TenOClock);

        AssertRefused(await _world.ExcuseAsync(goal, Wed11, "Ill"), ExcuseOutcome.HasRecord, HasRecord);
        var kept = await _world.EntryAsync(goal, Wed11);
        Assert.Equal(EvaluationState.Pending, kept.State);
        Assert.Single(kept.Records);
        AssertNoExcusal(goal);

        Assert.Equal(RecordWriteOutcome.Deleted, (await _world.DeleteAsync(goal, recorded.RecordId!.Value)).Outcome);
        AssertExcused(await _world.ExcuseAsync(goal, Wed11, "Ill"), Wed11);
        Assert.Equal(EvaluationState.Excused, (await _world.EntryAsync(goal, Wed11)).State);
    }

    [Fact]
    public async Task US1_AS11_a_reason_of_200_characters_is_accepted()
    {
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.At(Thu12TenOClock);
        var reason = new string('a', 200);

        AssertExcused(await _world.ExcuseAsync(goal, Wed11, reason), Wed11);
        Assert.Equal(reason, (await _world.EntryAsync(goal, Wed11)).ExcusalReason);
    }

    [Fact]
    public async Task US1_AS11_a_reason_of_201_characters_is_refused_and_nothing_changes()
    {
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.At(Thu12TenOClock);

        AssertRefused(await _world.ExcuseAsync(goal, Wed11, new string('a', 201)), ExcuseOutcome.ReasonTooLong, ReasonTooLong);
        AssertNoExcusal(goal);
        Assert.Equal(EvaluationState.Pending, (await _world.EntryAsync(goal, Wed11)).State);
    }

    [Fact]
    public async Task FR_001_a_reason_of_spaces_stores_none_and_a_reason_is_trimmed()
    {
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.At(Thu12TenOClock);

        AssertExcused(await _world.ExcuseAsync(goal, Wed11, "   "), Wed11);
        Assert.Null(_world.Records.ExemptionsOfGoal(goal).ExcusalOf(Wed11)!.Reason);

        // 07:00Z on Friday 13 November: Friday 13 is Due.
        _world.At(Utc("2026-11-13T07:00:00Z"));
        AssertExcused(await _world.ExcuseAsync(goal, Fri13, "  Travelling  "), Fri13);
        Assert.Equal("Travelling", _world.Records.ExemptionsOfGoal(goal).ExcusalOf(Fri13)!.Reason!.Value);
    }

    // ---- the grace end (SC-003) ----

    [Fact]
    public async Task SC_003_excusing_one_second_before_the_grace_end_works()
    {
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.At(Utc("2026-11-13T23:59:59Z"));

        AssertExcused(await _world.ExcuseAsync(goal, Wed11), Wed11);
    }

    [Theory]
    [InlineData("2026-11-14T00:00:00Z")]
    [InlineData("2026-11-14T00:00:01Z")]
    public async Task SC_003_excusing_at_the_grace_end_or_after_it_is_refused_as_final(string at)
    {
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.At(Utc(at));

        AssertRefused(await _world.ExcuseAsync(goal, Wed11), ExcuseOutcome.Final, Final);
        AssertNoExcusal(goal);
    }

    // ---- step 6 and the order of adjacent steps ----

    [Fact]
    public async Task FR_002_6_a_commitment_in_a_pause_gives_This_goal_is_paused_on_this_date()
    {
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.SeedPause(goal, Wed11, Fri13);
        _world.At(Thu12TenOClock);

        AssertRefused(await _world.ExcuseAsync(goal, Wed11), ExcuseOutcome.DatePaused, DatePaused);
        Assert.Equal(PauseTexts.DatePaused, DatePaused);
        AssertNoExcusal(goal);
    }

    [Fact]
    public async Task FR_002_order_a_period_based_goal_on_a_date_with_nothing_scheduled_gives_step_4()
    {
        var goal = await GoalAsync(YogaOnceADay);
        _world.At(Thu12TenOClock);

        // Sunday 1 November is before the start date, so no day of the goal holds it.
        AssertRefused(await _world.ExcuseAsync(goal, Sun1), ExcuseOutcome.NotACommitment, NotACommitment);
    }

    [Fact]
    public async Task FR_002_order_a_date_with_nothing_scheduled_inside_a_pause_gives_step_5()
    {
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.SeedPause(goal, Mon9, Fri13);
        _world.At(Thu12TenOClock);

        AssertRefused(await _world.ExcuseAsync(goal, Tue10), ExcuseOutcome.NothingScheduled, NothingScheduled);
    }

    [Fact]
    public async Task FR_002_order_a_paused_and_excused_date_gives_step_6()
    {
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.SeedExcusal(goal, Wed11, "Ill");
        _world.SeedPause(goal, Wed11, Wed11);
        _world.At(Thu12TenOClock);

        AssertRefused(await _world.ExcuseAsync(goal, Wed11), ExcuseOutcome.DatePaused, DatePaused);
    }

    [Fact]
    public async Task FR_002_order_an_excused_date_whose_grace_period_has_ended_gives_step_7()
    {
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.SeedExcusal(goal, Mon9, "Ill");
        _world.At(Thu12TenOClock);

        AssertRefused(await _world.ExcuseAsync(goal, Mon9), ExcuseOutcome.AlreadyExcused, AlreadyExcused);
        Assert.Equal("Ill", _world.Records.ExemptionsOfGoal(goal).ExcusalOf(Mon9)!.Reason!.Value);
    }

    [Fact]
    public async Task FR_002_order_a_final_commitment_with_a_record_gives_step_8()
    {
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.SeedAnswer(goal, Mon9, YesNoAnswer.Yes);
        _world.At(Thu12TenOClock);

        AssertRefused(await _world.ExcuseAsync(goal, Mon9), ExcuseOutcome.Final, Final);
        AssertNoExcusal(goal);
    }

    [Fact]
    public async Task FR_002_order_a_Pending_commitment_with_a_record_and_a_reason_too_long_gives_step_9()
    {
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.SeedAnswer(goal, Wed11, YesNoAnswer.No);
        _world.At(Thu12TenOClock);

        AssertRefused(await _world.ExcuseAsync(goal, Wed11, new string('a', 201)), ExcuseOutcome.HasRecord, HasRecord);
        AssertNoExcusal(goal);
    }

    // ---- a concurrent write between the checks and the store's write (D-3, research R-04) ----

    [Fact]
    public async Task D_3_a_concurrent_excusal_of_the_same_date_gives_This_commitment_is_already_excused()
    {
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.At(Thu12TenOClock);
        _world.Records.BeforeNextWrite = () => _world.SeedExcusal(goal, Wed11, "First");

        AssertRefused(await _world.ExcuseAsync(goal, Wed11, "Second"), ExcuseOutcome.AlreadyExcused, AlreadyExcused);
        Assert.Equal("First", _world.Records.ExemptionsOfGoal(goal).ExcusalOf(Wed11)!.Reason!.Value);
    }

    [Fact]
    public async Task D_3_a_concurrent_record_gives_This_commitment_has_a_record()
    {
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.At(Thu12TenOClock);
        _world.Records.BeforeNextWrite = () => _world.SeedAnswer(goal, Wed11, YesNoAnswer.Yes);

        AssertRefused(await _world.ExcuseAsync(goal, Wed11), ExcuseOutcome.HasRecord, HasRecord);
        AssertNoExcusal(goal);
        Assert.Single(_world.Records.RecordsOfGoal(goal));
    }

    [Fact]
    public async Task FR_009_a_concurrent_pause_over_the_date_gives_This_goal_is_paused_on_this_date()
    {
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.At(Thu12TenOClock);
        _world.Records.BeforeNextWrite = () => _world.SeedPause(goal, Wed11, Fri13);

        AssertRefused(await _world.ExcuseAsync(goal, Wed11), ExcuseOutcome.DatePaused, DatePaused);
        AssertNoExcusal(goal);
    }

    [Fact]
    public async Task The_excusal_is_written_once_for_the_owner_and_the_goal_after_the_reads()
    {
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.At(Thu12TenOClock);

        AssertExcused(await _world.ExcuseAsync(goal, Wed11, "Ill"), Wed11);
        Assert.Equal(
            [nameof(IRecordStore.ReadExemptionsAsync), nameof(IRecordStore.ReadAsync), nameof(IRecordStore.ExcuseAsync)],
            _world.Records.Calls);
    }

    // ---- Slice 8: the schedule kind of the date's attempt (plan "Spec discrepancies" 2; T056) ----

    [Fact]
    public async Task Slice_8_an_earlier_commitment_based_attempts_date_of_a_goal_now_per_week_can_be_excused()
    {
        // Attempt 1 "10 min daily" from Monday 2 November in UTC, ended on Monday 16 (last date Sunday 15); attempt 2
        // "3 times a week" from Wednesday 18. At 2026-11-17T10:00:00Z, 15 November is Pending.
        var world = new AttemptWorld();
        var goal = await world.CreateGoalAsync(DurationDays(10, DurationUnit.Minutes, BuildOrReduce.Build, AllDays), Date(2026, 11, 2), Utc("2026-11-01T10:00:00Z"));
        world.At(Utc("2026-11-16T10:00:00Z"));
        await world.EndAsync(goal);
        world.At(Utc("2026-11-16T10:01:00Z"));
        await world.StartAttemptAsync(goal, YesNoTimes(3, SchedulePeriod.Week), Date(2026, 11, 18));
        world.At(Utc("2026-11-17T10:00:00Z"));

        var between = await world.Excusals.ExcuseAsync(world.User, goal, Date(2026, 11, 16), null, CancellationToken.None);
        var excused = await world.Excusals.ExcuseAsync(world.User, goal, Date(2026, 11, 15), "Travelling.", CancellationToken.None);

        Assert.Equal((ExcuseOutcome.NothingScheduled, "Nothing is scheduled on this date."), (between.Outcome, between.Message));
        Assert.Equal((ExcuseOutcome.Excused, (DateOnly?)Date(2026, 11, 15)), (excused.Outcome, excused.Date));
        Assert.True(world.Records.ExemptionsOfGoal(goal).IsExcused(Date(2026, 11, 15)));

        world.At(Utc("2026-11-23T10:00:00Z"));
        var week = await world.Excusals.ExcuseAsync(world.User, goal, Date(2026, 11, 23), null, CancellationToken.None);
        Assert.Equal((ExcuseOutcome.NotACommitment, "Only commitments can be excused. This goal is evaluated by period."), (week.Outcome, week.Message));
    }

    [Fact]
    public async Task Slice_8_an_ending_on_the_date_between_the_check_and_the_write_gives_Nothing_is_scheduled_and_writes_nothing()
    {
        // "10 min daily" from Monday 2 November in UTC. At 2026-11-17T10:00:00Z Tuesday 17 is Due; the goal is ended on
        // Tuesday 17 (last date Monday 16) between the use case's checks and the store's write (FR-009).
        var world = new AttemptWorld();
        var goal = await world.CreateGoalAsync(DurationDays(10, DurationUnit.Minutes, BuildOrReduce.Build, AllDays), Date(2026, 11, 2), Utc("2026-11-01T10:00:00Z"));
        world.At(Utc("2026-11-17T10:00:00Z"));
        world.Records.BeforeNextWrite = () => world.EndNow(goal);

        var result = await world.Excusals.ExcuseAsync(world.User, goal, Date(2026, 11, 17), null, CancellationToken.None);

        Assert.Equal((ExcuseOutcome.NothingScheduled, "Nothing is scheduled on this date.", (DateOnly?)null), (result.Outcome, result.Message, result.Date));
        Assert.True(world.GoalOf(goal).IsEnded);
        Assert.False(world.Records.ExemptionsOfGoal(goal).IsExcused(Date(2026, 11, 17)));
    }

    [Fact]
    public async Task Slice_8_an_ending_between_the_check_and_the_write_keeps_the_ended_attempts_Pending_date_excusable()
    {
        // As above, but the excused date is Monday 16, Pending and the ended attempt's last date (FR-010).
        var world = new AttemptWorld();
        var goal = await world.CreateGoalAsync(DurationDays(10, DurationUnit.Minutes, BuildOrReduce.Build, AllDays), Date(2026, 11, 2), Utc("2026-11-01T10:00:00Z"));
        world.At(Utc("2026-11-17T10:00:00Z"));
        world.Records.BeforeNextWrite = () => world.EndNow(goal);

        var result = await world.Excusals.ExcuseAsync(world.User, goal, Date(2026, 11, 16), null, CancellationToken.None);

        Assert.Equal((ExcuseOutcome.Excused, (DateOnly?)Date(2026, 11, 16)), (result.Outcome, result.Date));
        Assert.True(world.GoalOf(goal).IsEnded);
        Assert.True(world.Records.ExemptionsOfGoal(goal).IsExcused(Date(2026, 11, 16)));
    }
}
