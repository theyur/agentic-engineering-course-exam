using HT.Application.Exemptions;
using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Exemptions;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Exemptions;

/// <summary>
/// Privacy of excusals and pauses (US5-AS1 to AS4, FR-021, FR-022, SC-005; T080) over the fakes of
/// <see cref="ExemptionWorld"/>: for each of the six use cases of <see cref="ExcusalService"/> and
/// <see cref="PauseService"/>, another registered user, an administrator, an anonymous visitor and an account that is
/// not stored each get their refusal, no view and no record store call; A's excusals, pauses, records and notes stay as
/// they were. Every call is made with arguments that succeed for A, so the refusal comes from the acting user alone
/// (<see cref="The_arguments_of_the_refused_calls_succeed_for_the_owner"/> proves it).
/// <para>
/// User A's goal "Reading, 20 min every day" is in UTC (commitment-based) and starts on Monday 2 November 2026, created
/// at 08:00Z that day. Seeded on it: 25 min with a note on Sunday 15, an excusal of Tuesday 17 with a reason, a current
/// pause from Wednesday 18 to Friday 20 with a reason and an upcoming pause from Monday 23 to Friday 27. The calls are
/// made at 10:00Z on Wednesday 18, the pause date. Then Monday 16 (ended 2026-11-17T00:00:00Z, grace period to
/// 2026-11-19T00:00:00Z) is Pending with no record, so it can be excused; Tuesday 17's excusal is inside its grace period
/// (to 2026-11-20T00:00:00Z), so it can be removed; Saturday 28 to Sunday 29 shares no date with a pause, so it can be
/// paused; the current pause began today, so resuming removes it; the upcoming pause can be cancelled.
/// </para>
/// </summary>
public sealed class ExemptionAccessTests
{
    private const string GoalNotFound = "This goal does not exist.";
    private const string NotForAdministrators = "Goals are for registered users.";
    private const string NotSignedIn = "You are not signed in.";
    private const string PauseNotFound = "This pause does not exist.";

    private const string ExcusalReason = "A's private excusal reason.";
    private const string PauseReason = "A's private pause reason.";
    private const string ANote = "A's private evening note.";

    private const string OtherUser = "another registered user";
    private const string Administrator = "an administrator";
    private const string Anonymous = "an anonymous visitor";
    private const string Unstored = "an account that is not stored";

    private static readonly DateOnly Mon2 = Date(2026, 11, 2);
    private static readonly DateOnly Sun15 = Date(2026, 11, 15);
    private static readonly DateOnly Mon16 = Date(2026, 11, 16);
    private static readonly DateOnly Tue17 = Date(2026, 11, 17);
    private static readonly DateOnly Wed18 = Date(2026, 11, 18);
    private static readonly DateOnly Fri20 = Date(2026, 11, 20);
    private static readonly DateOnly Mon23 = Date(2026, 11, 23);
    private static readonly DateOnly Fri27 = Date(2026, 11, 27);
    private static readonly DateOnly Sat28 = Date(2026, 11, 28);
    private static readonly DateOnly Sun29 = Date(2026, 11, 29);

    private static readonly DateTimeOffset Mon2Morning = Utc("2026-11-02T08:00:00Z");
    private static readonly DateTimeOffset Wed18TenOClock = Utc("2026-11-18T10:00:00Z");

    private static readonly string[] SixUseCases =
    [
        nameof(ExcusalService.ExcuseAsync),
        nameof(ExcusalService.RemoveExcusalAsync),
        nameof(PauseService.PauseAsync),
        nameof(PauseService.ResumeAsync),
        nameof(PauseService.CancelPauseAsync),
        nameof(PauseService.GetPausesAsync),
    ];

    private static readonly string[] RefusedCallers = [OtherUser, Administrator, Anonymous, Unstored];

    private readonly ExemptionWorld _world = new();

    /// <summary>Every use case × every refused caller.</summary>
    public static TheoryData<string, string> UseCasesByCaller
    {
        get
        {
            var data = new TheoryData<string, string>();
            foreach (var useCase in SixUseCases)
            {
                foreach (var caller in RefusedCallers)
                {
                    data.Add(useCase, caller);
                }
            }

            return data;
        }
    }

    /// <summary>Every refused caller.</summary>
    public static TheoryData<string> Callers
    {
        get
        {
            var data = new TheoryData<string>();
            foreach (var caller in RefusedCallers)
            {
                data.Add(caller);
            }

            return data;
        }
    }

    private static GoalTargetFields Reading20MinDaily => DurationDays(20, DurationUnit.Minutes, BuildOrReduce.Build, AllDays);

    /// <summary>"Drinking water, 8 glasses a day": Build, count at least 8 "glasses", once per day (period-based).</summary>
    private static GoalTargetFields DrinkingWater => CountTimes(8, "glasses", 1, SchedulePeriod.Day);

    /// <summary>One call of a use case: its outcome's name, its text and whether it returned any view.</summary>
    private sealed record Call(string Outcome, string? Message, bool HasView);

    /// <summary>A's goal, as the class summary gives it; the clock is left at Wednesday 18, 10:00Z.</summary>
    private sealed record AsGoal(GoalId Goal, PauseId Current, PauseId Upcoming);

    private async Task<AsGoal> AsGoalAsync()
    {
        var goal = await _world.CreateGoalAsync(Reading20MinDaily, Mon2, Mon2Morning);
        _world.SeedAmount(goal, Sun15, 25);
        _world.SeedNote(goal, Sun15, ANote);
        _world.SeedExcusal(goal, Tue17, ExcusalReason);
        var current = _world.SeedPause(goal, Wed18, Fri20, PauseReason);
        var upcoming = _world.SeedPause(goal, Mon23, Fri27);
        _world.At(Wed18TenOClock);
        return new AsGoal(goal, current, upcoming);
    }

    private ActingUser CallerOf(string caller) => caller switch
    {
        OtherUser => _world.OtherUser,
        Administrator => _world.Administrator,
        Anonymous => ActingUser.Anonymous,
        Unstored => ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.RegisteredUser),
        _ => throw new ArgumentOutOfRangeException(nameof(caller), caller, null),
    };

    private static (string Outcome, string Message) RefusalOf(string caller) => caller switch
    {
        OtherUser => ("NotFound", GoalNotFound),
        Administrator => ("NotForAdministrators", NotForAdministrators),
        _ => ("NotSignedIn", NotSignedIn),
    };

    /// <summary>
    /// <paramref name="useCase"/> by <paramref name="user"/> on <paramref name="goal"/>, with the arguments that succeed
    /// for A on A's goal at Wednesday 18, 10:00Z.
    /// </summary>
    private async Task<Call> CallAsync(string useCase, ActingUser user, AsGoal goal)
    {
        var none = CancellationToken.None;
        var excusals = _world.Excusals;
        var pauses = _world.Pauses;
        switch (useCase)
        {
            case nameof(ExcusalService.ExcuseAsync):
                var excused = await excusals.ExcuseAsync(user, goal.Goal, Mon16, "Someone else's reason.", none);
                return new Call(excused.Outcome.ToString(), excused.Message, excused.Date is not null);
            case nameof(ExcusalService.RemoveExcusalAsync):
                var removed = await excusals.RemoveExcusalAsync(user, goal.Goal, Tue17, none);
                return new Call(removed.Outcome.ToString(), removed.Message, removed.State is not null);
            case nameof(PauseService.PauseAsync):
                var paused = await pauses.PauseAsync(user, goal.Goal, Sat28, Sun29, "Someone else's reason.", none);
                return new Call(paused.Outcome.ToString(), paused.Message, paused.Pause is not null);
            case nameof(PauseService.ResumeAsync):
                var resumed = await pauses.ResumeAsync(user, goal.Goal, none);
                return new Call(resumed.Outcome.ToString(), resumed.Message, false);
            case nameof(PauseService.CancelPauseAsync):
                var cancelled = await pauses.CancelPauseAsync(user, goal.Goal, goal.Upcoming, none);
                return new Call(cancelled.Outcome.ToString(), cancelled.Message, false);
            case nameof(PauseService.GetPausesAsync):
                var read = await pauses.GetPausesAsync(user, goal.Goal, none);
                return new Call(read.Outcome.ToString(), read.Message, read.Pauses.Count > 0 || read.PauseDate is not null);
            default:
                throw new ArgumentOutOfRangeException(nameof(useCase), useCase, null);
        }
    }

    [Theory]
    [MemberData(nameof(UseCasesByCaller))]
    public async Task US5_AS1_to_AS3_FR_022_a_caller_who_is_not_the_owner_is_refused_gets_no_view_and_nothing_changes(
        string useCase, string caller)
    {
        var goal = await AsGoalAsync();
        var arranged = _world.Records.Calls.Count;

        var call = await CallAsync(useCase, CallerOf(caller), goal);

        var (outcome, message) = RefusalOf(caller);
        Assert.Equal(new Call(outcome, message, false), call);
        Assert.Equal(arranged, _world.Records.Calls.Count);
        AssertUnchanged(goal);
    }

    [Fact]
    public async Task US5_AS1_FR_021_another_users_result_for_As_goal_is_the_same_as_for_a_goal_that_does_not_exist()
    {
        var goal = await AsGoalAsync();
        var noGoal = new AsGoal(new GoalId(Guid.NewGuid()), new PauseId(Guid.NewGuid()), new PauseId(Guid.NewGuid()));

        foreach (var useCase in SixUseCases)
        {
            var onAsGoal = await CallAsync(useCase, _world.OtherUser, goal);
            var onNoGoal = await CallAsync(useCase, _world.OtherUser, noGoal);

            Assert.Equal(onNoGoal, onAsGoal);
        }

        AssertUnchanged(goal);
    }

    [Theory]
    [MemberData(nameof(Callers))]
    public async Task FR_022_the_access_steps_come_before_every_other_check(string caller)
    {
        var water = await _world.CreateGoalAsync(DrinkingWater, Mon2, Mon2Morning);
        var goal = await AsGoalAsync();
        var arranged = _world.Records.Calls.Count;
        var user = CallerOf(caller);
        var (outcome, message) = RefusalOf(caller);
        var none = CancellationToken.None;
        var tooLong = new string('x', 201);

        // Each call would be refused by a later step for A: a period-based goal, a date with no excusal, a missing day,
        // a first day before today, a reason over 200 characters, a goal that is not paused (the water goal) and a pause
        // that is not the goal's.
        var excuseOnPeriodGoal = await _world.Excusals.ExcuseAsync(user, water, Mon16, null, none);
        var removeNotExcused = await _world.Excusals.RemoveExcusalAsync(user, goal.Goal, Mon16, none);
        var pauseMissingDay = await _world.Pauses.PauseAsync(user, water, null, Sun29, null, none);
        var pauseInThePast = await _world.Pauses.PauseAsync(user, water, Mon16, Tue17, null, none);
        var pauseReasonTooLong = await _world.Pauses.PauseAsync(user, water, Sat28, Sun29, tooLong, none);
        var resumeNotPaused = await _world.Pauses.ResumeAsync(user, water, none);
        var cancelUnknown = await _world.Pauses.CancelPauseAsync(user, goal.Goal, new PauseId(Guid.NewGuid()), none);

        Assert.All(
            new[]
            {
                (excuseOnPeriodGoal.Outcome.ToString(), excuseOnPeriodGoal.Message),
                (removeNotExcused.Outcome.ToString(), removeNotExcused.Message),
                (pauseMissingDay.Outcome.ToString(), pauseMissingDay.Message),
                (pauseInThePast.Outcome.ToString(), pauseInThePast.Message),
                (pauseReasonTooLong.Outcome.ToString(), pauseReasonTooLong.Message),
                (resumeNotPaused.Outcome.ToString(), resumeNotPaused.Message),
                (cancelUnknown.Outcome.ToString(), cancelUnknown.Message),
            },
            result => Assert.Equal((outcome, message), result));
        Assert.Equal(arranged, _world.Records.Calls.Count);
        AssertUnchanged(goal);
        var waterExemptions = _world.Records.ExemptionsOfGoal(water);
        Assert.Empty(waterExemptions.Excusals);
        Assert.Empty(waterExemptions.Pauses);
    }

    [Fact]
    public async Task US5_AS4_A_cancelling_As_pause_through_As_other_goal_gets_This_pause_does_not_exist()
    {
        var other = await _world.CreateGoalAsync(DrinkingWater, Mon2, Mon2Morning);
        var goal = await AsGoalAsync();

        foreach (var pause in new[] { goal.Upcoming, goal.Current })
        {
            var result = await _world.CancelPauseAsync(other, pause);

            Assert.Equal(CancelPauseOutcome.PauseNotFound, result.Outcome);
            Assert.Equal(PauseNotFound, result.Message);
        }

        Assert.DoesNotContain(nameof(IRecordStore.DeletePauseAsync), _world.Records.Calls);
        Assert.Empty(_world.Records.ExemptionsOfGoal(other).Pauses);
        AssertUnchanged(goal);
    }

    [Fact]
    public async Task US5_AS4_B_cancelling_As_pause_through_Bs_own_goal_gets_This_pause_does_not_exist()
    {
        var bsGoal = await _world.CreateGoalAsync(DrinkingWater, Mon2, Mon2Morning, _world.OtherUser);
        var goal = await AsGoalAsync();

        foreach (var pause in new[] { goal.Upcoming, goal.Current })
        {
            var result = await _world.CancelPauseAsync(bsGoal, pause, _world.OtherUser);

            Assert.Equal(CancelPauseOutcome.PauseNotFound, result.Outcome);
            Assert.Equal(PauseNotFound, result.Message);
        }

        // Only B's own goal was read: every exemption call passed B's goal, and no write was made.
        Assert.DoesNotContain(nameof(IRecordStore.DeletePauseAsync), _world.Records.Calls);
        Assert.Empty(_world.Records.ExemptionsOfGoal(bsGoal).Pauses);
        AssertUnchanged(goal);
    }

    [Fact]
    public async Task US5_AS1_B_reading_the_pauses_of_Bs_own_goal_gets_none_of_As()
    {
        var bsGoal = await _world.CreateGoalAsync(DrinkingWater, Mon2, Mon2Morning, _world.OtherUser);
        var goal = await AsGoalAsync();

        var pauses = await _world.PausesAsync(bsGoal, _world.OtherUser);

        Assert.Empty(pauses);
        AssertUnchanged(goal);
    }

    [Theory]
    [InlineData(nameof(ExcusalService.ExcuseAsync), "Excused")]
    [InlineData(nameof(ExcusalService.RemoveExcusalAsync), "Removed")]
    [InlineData(nameof(PauseService.PauseAsync), "Paused")]
    [InlineData(nameof(PauseService.ResumeAsync), "Resumed")]
    [InlineData(nameof(PauseService.CancelPauseAsync), "Cancelled")]
    [InlineData(nameof(PauseService.GetPausesAsync), "Found")]
    public async Task The_arguments_of_the_refused_calls_succeed_for_the_owner(string useCase, string outcome)
    {
        var goal = await AsGoalAsync();

        var call = await CallAsync(useCase, _world.User, goal);

        Assert.Equal(outcome, call.Outcome);
    }

    /// <summary>A's goal still has exactly its excusal, its two pauses, its one record and its one note.</summary>
    private void AssertUnchanged(AsGoal goal)
    {
        var exemptions = _world.Records.ExemptionsOfGoal(goal.Goal);
        var excusal = Assert.Single(exemptions.Excusals);
        Assert.Equal(new Excusal(Tue17, ExemptionWorld.Reason(ExcusalReason)), excusal);
        Assert.Equal(
            new[]
            {
                new Pause(goal.Current, Wed18, Fri20, ExemptionWorld.Reason(PauseReason)),
                new Pause(goal.Upcoming, Mon23, Fri27, null),
            }.OrderBy(pause => pause.Id.Value),
            exemptions.Pauses.OrderBy(pause => pause.Id.Value));
        var record = Assert.Single(_world.Records.RecordsOfGoal(goal.Goal));
        Assert.Equal((Sun15, RecordValue.FromAmount(25)), (record.Date, record.Value));
        var note = Assert.Single(_world.Records.NotesOfGoal(goal.Goal));
        Assert.Equal((Sun15, ANote), (note.Key, note.Value.Value));
    }
}
