using HT.Application.Exemptions;
using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using static HT.UnitTests.Application.Exemptions.ExemptionWorld;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Exemptions;

/// <summary>
/// Resuming a goal early (FR-010, US2-AS5, AS6, AS11, AS12, D-1, SC-003; T056) over the fakes of
/// <see cref="ExemptionWorld"/>. Every goal starts on Monday 2 November 2026 and is created at 08:00Z that day, in UTC
/// (the user has no stored time zone), unless a test says otherwise. "Today" is the goal's current date (research R-03):
/// at 08:00Z on Wednesday 18 November it is Wednesday 18. Every instant is worked out by hand.
/// </summary>
public sealed class ResumeTests
{
    private const string NotSignedIn = "You are not signed in.";
    private const string NotForAdministrators = "Goals are for registered users.";
    private const string GoalNotFound = "This goal does not exist.";
    private const string NotPaused = "This goal is not paused.";
    private const string WeekPaused = "This week is paused.";

    private static readonly DateOnly Mon2 = Date(2026, 11, 2);
    private static readonly DateOnly Wed4 = Date(2026, 11, 4);
    private static readonly DateOnly Mon16 = Date(2026, 11, 16);
    private static readonly DateOnly Tue17 = Date(2026, 11, 17);
    private static readonly DateOnly Wed18 = Date(2026, 11, 18);
    private static readonly DateOnly Thu19 = Date(2026, 11, 19);
    private static readonly DateOnly Fri20 = Date(2026, 11, 20);
    private static readonly DateOnly Sun22 = Date(2026, 11, 22);
    private static readonly DateOnly Mon23 = Date(2026, 11, 23);
    private static readonly DateOnly Wed25 = Date(2026, 11, 25);
    private static readonly DateOnly Fri27 = Date(2026, 11, 27);

    private static readonly DateTimeOffset Mon2Morning = Utc("2026-11-02T08:00:00Z");

    /// <summary>09:00Z on Monday 16 November, when US2-AS1's pause is set.</summary>
    private static readonly DateTimeOffset Mon16NineOClock = Utc("2026-11-16T09:00:00Z");

    /// <summary>08:00Z on Wednesday 18 November: today is Wednesday 18.</summary>
    private static readonly DateTimeOffset Wed18EightOClock = Utc("2026-11-18T08:00:00Z");

    private readonly ExemptionWorld _world = new();

    /// <summary>"Drinking water, 8 glasses a day": Build, count at least 8 "glasses", once per day (period-based).</summary>
    private static GoalTargetFields DrinkingWater => CountTimes(8, "glasses", 1, SchedulePeriod.Day);

    /// <summary>"Yoga, 3 days a week": a yes/no Build goal on Monday, Wednesday and Friday (commitment-based).</summary>
    private static GoalTargetFields YogaOnMonWedFri => YesNoDays(BuildOrReduce.Build, MonWedFri);

    /// <summary>"Yoga, 3 times a week": a yes/no Build goal of three times a week (period-based, per week).</summary>
    private static GoalTargetFields YogaThreeTimesAWeek => YesNoTimes(3, SchedulePeriod.Week);

    private Task<GoalId> GoalAsync(GoalTargetFields? target = null) => _world.CreateGoalAsync(target ?? DrinkingWater, Mon2, Mon2Morning);

    /// <summary>US2-AS1's goal and pause: Drinking water, paused at 09:00Z on Monday 16 from Monday 16 to Sunday 22.</summary>
    private async Task<GoalId> PausedDrinkingWaterAsync()
    {
        var goal = await GoalAsync();
        _world.At(Mon16NineOClock);
        Assert.Equal(PauseOutcome.Paused, (await _world.PauseAsync(goal, Mon16, Sun22, "Family visit")).Outcome);
        return goal;
    }

    private static void AssertRefused(ResumeResult result, ResumeOutcome outcome, string message)
    {
        Assert.Equal(outcome, result.Outcome);
        Assert.Equal(message, result.Message);
    }

    private static void AssertResumed(ResumeResult result)
    {
        Assert.Equal(ResumeOutcome.Resumed, result.Outcome);
        Assert.Equal("resumed", result.Message);
        Assert.Equal(PauseTexts.Resumed, result.Message);
    }

    // ---- access (FR-010 steps 1 to 3, as FR-002's) ----

    [Fact]
    public async Task FR_010_1_an_anonymous_visitor_is_refused_and_the_pause_stays()
    {
        var goal = await PausedDrinkingWaterAsync();
        _world.At(Wed18EightOClock);
        var calls = _world.Records.Calls.Count;

        AssertRefused(await _world.ResumeAsync(goal, ActingUser.Anonymous), ResumeOutcome.NotSignedIn, NotSignedIn);
        Assert.Equal(calls, _world.Records.Calls.Count);
        Assert.Equal(Sun22, Assert.Single(_world.Records.ExemptionsOfGoal(goal).Pauses).LastDay);
    }

    [Fact]
    public async Task FR_010_2_an_administrator_is_refused_and_the_pause_stays()
    {
        var goal = await PausedDrinkingWaterAsync();
        _world.At(Wed18EightOClock);
        var calls = _world.Records.Calls.Count;

        AssertRefused(await _world.ResumeAsync(goal, _world.Administrator), ResumeOutcome.NotForAdministrators, NotForAdministrators);
        Assert.Equal(calls, _world.Records.Calls.Count);
        Assert.Equal(Sun22, Assert.Single(_world.Records.ExemptionsOfGoal(goal).Pauses).LastDay);
    }

    [Fact]
    public async Task FR_010_3_an_account_that_is_not_stored_is_refused_as_not_signed_in()
    {
        var goal = await PausedDrinkingWaterAsync();
        _world.At(Wed18EightOClock);
        var gone = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.RegisteredUser);

        AssertRefused(await _world.ResumeAsync(goal, gone), ResumeOutcome.NotSignedIn, NotSignedIn);
        Assert.Equal(Sun22, Assert.Single(_world.Records.ExemptionsOfGoal(goal).Pauses).LastDay);
    }

    [Fact]
    public async Task FR_010_3_another_users_goal_and_no_goal_give_This_goal_does_not_exist()
    {
        var goal = await PausedDrinkingWaterAsync();
        _world.At(Wed18EightOClock);

        AssertRefused(await _world.ResumeAsync(goal, _world.OtherUser), ResumeOutcome.NotFound, GoalNotFound);
        AssertRefused(await _world.ResumeAsync(new GoalId(Guid.NewGuid())), ResumeOutcome.NotFound, GoalNotFound);
        Assert.Equal(Sun22, Assert.Single(_world.Records.ExemptionsOfGoal(goal).Pauses).LastDay);
    }

    // ---- the scenarios ----

    [Fact]
    public async Task US2_AS5_resuming_on_a_later_day_ends_the_pause_on_the_day_before_and_today_is_Due_again()
    {
        var goal = await PausedDrinkingWaterAsync();
        _world.At(Wed18EightOClock);

        AssertResumed(await _world.ResumeAsync(goal));

        var pause = Assert.Single(_world.Records.ExemptionsOfGoal(goal).Pauses);
        Assert.Equal(Mon16, pause.FirstDay);
        Assert.Equal(Tue17, pause.LastDay);
        Assert.Equal("Family visit", pause.Reason!.Value);
        Assert.Equal(EvaluationState.Paused, (await _world.EntryAsync(goal, Mon16)).State);
        Assert.Equal(EvaluationState.Paused, (await _world.EntryAsync(goal, Tue17)).State);
        Assert.Equal(EvaluationState.Due, (await _world.EntryAsync(goal, Wed18)).State);
        Assert.Contains(await _world.RecordableAsync(goal), entry => entry.FirstDate == Wed18);
        Assert.Null(Assert.Single((await _world.OverviewAsync()).Goals).PausedUntil);
        Assert.Equal(RecordWriteOutcome.Recorded, (await _world.RecordAsync(goal, Wed18, Amount(8))).Outcome);
    }

    [Fact]
    public async Task US2_AS6_resuming_on_the_pauses_first_day_removes_it_and_its_records_count_as_never_paused()
    {
        var goal = await GoalAsync();

        // 08:00Z on Monday 16: 8 glasses; then the pause at 09:00Z; then the resume at 18:00Z the same day.
        _world.At(Utc("2026-11-16T08:00:00Z"));
        Assert.Equal(RecordWriteOutcome.Recorded, (await _world.RecordAsync(goal, Mon16, Amount(8))).Outcome);
        _world.At(Mon16NineOClock);
        Assert.Equal(PauseOutcome.Paused, (await _world.PauseAsync(goal, Mon16, Sun22, "Family visit")).Outcome);
        Assert.Equal(EvaluationState.Paused, (await _world.EntryAsync(goal, Mon16)).State);
        _world.At(Utc("2026-11-16T18:00:00Z"));

        AssertResumed(await _world.ResumeAsync(goal));

        Assert.Empty(_world.Records.ExemptionsOfGoal(goal).Pauses);
        var entry = await _world.EntryAsync(goal, Mon16);
        Assert.Equal(EvaluationState.Due, entry.State);
        Assert.Single(entry.Records);

        // Monday 16 ends at 2026-11-17T00:00:00Z; its grace period ends at 2026-11-19T00:00:00Z, and it is final and Met,
        // counted as if the goal had never been paused.
        _world.At(Utc("2026-11-19T00:00:00Z"));
        Assert.Equal(EvaluationState.Met, (await _world.EntryAsync(goal, Mon16)).State);
    }

    [Fact]
    public async Task US2_AS11_a_commitment_based_goal_resumed_mid_pause_keeps_the_earlier_commitment_Paused()
    {
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.At(Wed18EightOClock);
        Assert.Equal(PauseOutcome.Paused, (await _world.PauseAsync(goal, Wed18, Fri20)).Outcome);
        Assert.Equal(EvaluationState.Paused, (await _world.EntryAsync(goal, Wed18)).State);
        Assert.Equal(EvaluationState.Paused, (await _world.EntryAsync(goal, Fri20)).State);

        // 08:00Z on Friday 20 November: today is Friday 20.
        _world.At(Utc("2026-11-20T08:00:00Z"));
        AssertResumed(await _world.ResumeAsync(goal));

        var pause = Assert.Single(_world.Records.ExemptionsOfGoal(goal).Pauses);
        Assert.Equal(Wed18, pause.FirstDay);
        Assert.Equal(Thu19, pause.LastDay);
        Assert.Equal(EvaluationState.Paused, (await _world.EntryAsync(goal, Wed18)).State);
        Assert.Equal(EvaluationState.Due, (await _world.EntryAsync(goal, Fri20)).State);
    }

    [Fact]
    public async Task US2_AS12_a_per_week_goal_keeps_a_partly_paused_week_Paused_and_gets_the_next_week_back()
    {
        var goal = await GoalAsync(YogaThreeTimesAWeek);
        _world.At(Wed18EightOClock);
        Assert.Equal(PauseOutcome.Paused, (await _world.PauseAsync(goal, Thu19, Wed25)).Outcome);

        // The weeks 16–22 and 23–29 November each have a paused date, so both are Paused as a whole (D-1).
        Assert.Equal(EvaluationState.Paused, (await _world.EntryAsync(goal, Wed18)).State);
        Assert.Equal(EvaluationState.Paused, (await _world.EntryAsync(goal, Mon23)).State);

        // 09:00Z on Wednesday 18: the date is not in the pause, but its week is Paused.
        _world.At(Utc("2026-11-18T09:00:00Z"));
        var record = await _world.RecordAsync(goal, Wed18, Yes());
        Assert.Equal(RecordWriteOutcome.WeekPaused, record.Outcome);
        Assert.Equal(WeekPaused, record.Message);
        Assert.Empty(_world.Records.RecordsOfGoal(goal));

        // 10:00Z on Monday 23 November: today is Monday 23.
        _world.At(Utc("2026-11-23T10:00:00Z"));
        AssertResumed(await _world.ResumeAsync(goal));

        var pause = Assert.Single(_world.Records.ExemptionsOfGoal(goal).Pauses);
        Assert.Equal(Thu19, pause.FirstDay);
        Assert.Equal(Sun22, pause.LastDay);
        Assert.Equal(EvaluationState.Paused, (await _world.EntryAsync(goal, Wed18)).State);
        Assert.Equal(EvaluationState.Due, (await _world.EntryAsync(goal, Mon23)).State);
    }

    // ---- FR-010 step 4 ----

    [Fact]
    public async Task FR_010_4_a_goal_with_no_pause_is_not_paused()
    {
        var goal = await GoalAsync();
        _world.At(Wed18EightOClock);

        AssertRefused(await _world.ResumeAsync(goal), ResumeOutcome.NotPaused, NotPaused);
        Assert.DoesNotContain("DeletePauseAsync", _world.Records.Calls);
        Assert.DoesNotContain("SetPauseLastDayAsync", _world.Records.Calls);
    }

    [Fact]
    public async Task FR_010_4_a_past_pause_and_an_upcoming_pause_do_not_make_the_goal_paused()
    {
        var goal = await GoalAsync();
        _world.SeedPause(goal, Mon2, Wed4);
        _world.SeedPause(goal, Mon23, Fri27);
        _world.At(Wed18EightOClock);

        AssertRefused(await _world.ResumeAsync(goal), ResumeOutcome.NotPaused, NotPaused);
        var pauses = _world.Records.ExemptionsOfGoal(goal).Pauses;
        Assert.Equal(2, pauses.Count);
        Assert.Equal(Wed4, pauses.Single(p => p.FirstDay == Mon2).LastDay);
        Assert.Equal(Fri27, pauses.Single(p => p.FirstDay == Mon23).LastDay);
    }

    [Fact]
    public async Task Edge_a_current_week_Paused_only_by_an_upcoming_pause_is_not_a_paused_goal()
    {
        var goal = await GoalAsync(YogaThreeTimesAWeek);
        _world.At(Wed18EightOClock);
        Assert.Equal(PauseOutcome.Paused, (await _world.PauseAsync(goal, Thu19, Wed25)).Outcome);
        Assert.Equal(EvaluationState.Paused, (await _world.EntryAsync(goal, Wed18)).State);

        AssertRefused(await _world.ResumeAsync(goal), ResumeOutcome.NotPaused, NotPaused);
        Assert.Equal(Wed25, Assert.Single(_world.Records.ExemptionsOfGoal(goal).Pauses).LastDay);
    }

    // ---- a goal that has not started (AC-087, research R-03) ----

    [Fact]
    public async Task AC_087_a_goal_that_has_not_started_resumed_on_its_pauses_first_day_loses_the_pause()
    {
        // Created at 09:00Z on Monday 16 with the start date Friday 20: it has no current date, so today is the account's
        // today in UTC, Monday 16.
        var goal = await _world.CreateGoalAsync(DrinkingWater, Fri20, Mon16NineOClock);
        Assert.Equal(PauseOutcome.Paused, (await _world.PauseAsync(goal, Mon16, Sun22)).Outcome);
        _world.At(Utc("2026-11-16T18:00:00Z"));

        AssertResumed(await _world.ResumeAsync(goal));
        Assert.Empty(_world.Records.ExemptionsOfGoal(goal).Pauses);
    }

    [Fact]
    public async Task AC_087_a_goal_that_has_not_started_resumed_later_ends_the_pause_on_the_day_before()
    {
        var goal = await _world.CreateGoalAsync(DrinkingWater, Fri20, Mon16NineOClock);
        Assert.Equal(PauseOutcome.Paused, (await _world.PauseAsync(goal, Mon16, Sun22)).Outcome);
        _world.At(Wed18EightOClock);

        AssertResumed(await _world.ResumeAsync(goal));
        var pause = Assert.Single(_world.Records.ExemptionsOfGoal(goal).Pauses);
        Assert.Equal(Mon16, pause.FirstDay);
        Assert.Equal(Tue17, pause.LastDay);
    }

    [Fact]
    public async Task A_concurrent_cancel_or_resume_before_the_write_gives_This_goal_is_not_paused()
    {
        var goal = await PausedDrinkingWaterAsync();
        _world.At(Wed18EightOClock);
        var pause = Assert.Single(_world.Records.ExemptionsOfGoal(goal).Pauses).Id;
        // The fake store completes synchronously, so the removal has happened when the hook returns.
        _world.Records.BeforeNextWrite = () =>
            _ = _world.Records.DeletePauseAsync(_world.User.AccountId!.Value, goal, pause, CancellationToken.None);

        AssertRefused(await _world.ResumeAsync(goal), ResumeOutcome.NotPaused, NotPaused);
        Assert.Empty(_world.Records.ExemptionsOfGoal(goal).Pauses);
    }
}
