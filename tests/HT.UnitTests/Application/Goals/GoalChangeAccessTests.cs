using HT.Application.Goals;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Goals;

/// <summary>
/// Slice 8 (T100; US5-AS1 to AS3, FR-022, FR-023, SC-006): who may use which goal, for every
/// <see cref="GoalChangeService"/> method (<c>ChangeSettingsAsync</c>, <c>EndAsync</c>, <c>GetNewAttemptValuesAsync</c>,
/// <c>StartAttemptAsync</c>), on <see cref="AttemptWorld"/>. User A has an active goal "Zq A active goal" and an ended
/// goal "Zq A ended goal" (Reading, 15 min daily, from Monday 2 November 2026, created on Sunday 1 November at 10:00Z;
/// the second ended on Wednesday 11 November at 10:00Z with "Abandoned" and an ending note); user B has a goal of their
/// own. Each method is called twice per acting user: once with inputs that A's call would accept (A's call with them
/// succeeds, see the last test), and once with inputs that fail every later check of its order (an ended goal for a
/// settings change and an ending, an active goal for the new-attempt use cases, missing fields, a name and a note that
/// are too long), so the earlier refusal is shown to win. Each refusal returns no view and changes nothing: no
/// conditional write is made, no record store member is called, and the stored goals stay the same instances (the fake
/// replaces a goal on a write). A refusal before the goal also looks up no goal (FR-023's order). Every instant is a
/// UTC literal; the users have no stored zone, so everything is in UTC.
/// </summary>
public sealed class GoalChangeAccessTests
{
    private const string GoalNotFound = "This goal does not exist.";
    private const string NotForAdministrators = "Goals are for registered users.";
    private const string NotSignedIn = "You are not signed in.";

    private const string ActiveName = "Zq A active goal";
    private const string EndedName = "Zq A ended goal";
    private const string AsEndingNote = "Zq A private ending note.";

    private static readonly CancellationToken None = CancellationToken.None;

    private static readonly DateOnly Mon2 = Date(2026, 11, 2);
    private static readonly DateOnly Thu12 = Date(2026, 11, 12);

    private static readonly DateTimeOffset Created = Utc("2026-11-01T10:00:00Z");
    private static readonly DateTimeOffset Wed11At10 = Utc("2026-11-11T10:00:00Z");

    private static readonly string[] FourUseCases =
    [
        nameof(GoalChangeService.ChangeSettingsAsync),
        nameof(GoalChangeService.EndAsync),
        nameof(GoalChangeService.GetNewAttemptValuesAsync),
        nameof(GoalChangeService.StartAttemptAsync),
    ];

    private readonly AttemptWorld _world = new();

    private static GoalTargetFields MinutesDaily(int minutes) => DurationDays(minutes, DurationUnit.Minutes, BuildOrReduce.Build, AllDays);

    /// <summary>One call of a use case: its name, its outcome's name, its text and whether it returned a view or a field.</summary>
    private sealed record Call(string UseCase, string Outcome, string? Message, bool HasView);

    /// <summary>A's two goals and B's goal; the clock is left at <see cref="Wed11At10"/>, after A's ending.</summary>
    private sealed record Arranged(GoalId Active, GoalId Ended, GoalId Bs);

    /// <summary>What a refusal must leave as it was: the store's counters and the stored goals.</summary>
    private sealed record Snapshot(int Lookups, int ChangeWrites, int RecordCalls, IReadOnlyList<Goal> Stored);

    private async Task<Arranged> ArrangeAsync()
    {
        var active = await _world.CreateNamedGoalAsync(ActiveName, MinutesDaily(15), Mon2, Created);
        var ended = await _world.CreateNamedGoalAsync(EndedName, MinutesDaily(15), Mon2, Created);
        var bs = await _world.CreateNamedGoalAsync("Zq B walk", MinutesDaily(15), Mon2, Created, _world.OtherUser);
        _world.At(Wed11At10);
        var result = await _world.Changes.EndAsync(_world.User, ended, Outcome.Abandoned, AsEndingNote, None);
        Assert.Equal(GoalEndOutcome.Ended, result.Outcome);
        return new Arranged(active, ended, bs);
    }

    private Snapshot Take() => new(_world.Goals.Lookups, _world.Goals.ChangeWriteCount, _world.Records.Calls.Count, _world.Goals.Goals);

    /// <summary>Nothing was written, no record store member was called and every stored goal is the same instance.</summary>
    private void AssertNothingChanged(Snapshot before)
    {
        Assert.Equal(before.ChangeWrites, _world.Goals.ChangeWriteCount);
        Assert.Equal(before.RecordCalls, _world.Records.Calls.Count);
        var after = _world.Goals.Goals;
        Assert.Equal(before.Stored.Count, after.Count);
        Assert.All(before.Stored.Zip(after), pair => Assert.Same(pair.First, pair.Second));
    }

    /// <summary>
    /// The four use cases by <paramref name="user"/> with inputs A's own call would accept: a change to 20 minutes and an
    /// ending of the active goal, the starting values and a new attempt from Thursday 12 November of the ended goal.
    /// </summary>
    private async Task<IReadOnlyList<Call>> AcceptableCallsAsync(ActingUser user, GoalId active, GoalId ended)
    {
        var changes = _world.Changes;
        var changed = await changes.ChangeSettingsAsync(user, active, new GoalSettingsInput { Target = MinutesDaily(20), Name = ActiveName }, None);
        var endedNow = await changes.EndAsync(user, active, Outcome.Achieved, "Zq other note", None);
        var values = await changes.GetNewAttemptValuesAsync(user, ended, None);
        var started = await changes.StartAttemptAsync(user, ended, new NewAttemptInput { Target = MinutesDaily(15), StartDate = Thu12 }, None);
        return CallsOf(changed, endedNow, values, started);
    }

    /// <summary>
    /// The four use cases by <paramref name="user"/> with inputs that fail every later check: a settings change of the
    /// ended goal with no target and a name of 61 characters; an ending of the ended goal with no outcome and a note of
    /// 501 characters; the starting values and a new attempt of the active goal, the latter with no target and no start
    /// date.
    /// </summary>
    private async Task<IReadOnlyList<Call>> FailingCallsAsync(ActingUser user, GoalId active, GoalId ended)
    {
        var changes = _world.Changes;
        var changed = await changes.ChangeSettingsAsync(user, ended, new GoalSettingsInput { Target = new GoalTargetFields(), Name = new string('x', 61) }, None);
        var endedNow = await changes.EndAsync(user, ended, null, new string('n', 501), None);
        var values = await changes.GetNewAttemptValuesAsync(user, active, None);
        var started = await changes.StartAttemptAsync(user, active, new NewAttemptInput { Target = new GoalTargetFields(), StartDate = null }, None);
        return CallsOf(changed, endedNow, values, started);
    }

    private static IReadOnlyList<Call> CallsOf(SettingsChangeResult changed, GoalEndResult ended, NewAttemptValuesResult values, AttemptStartResult started) =>
    [
        new(nameof(GoalChangeService.ChangeSettingsAsync), changed.Outcome.ToString(), changed.Message, changed.AppliesFrom is not null || changed.Field is not null),
        new(nameof(GoalChangeService.EndAsync), ended.Outcome.ToString(), ended.Message, ended.EndingDate is not null || ended.LastDate is not null),
        new(nameof(GoalChangeService.GetNewAttemptValuesAsync), values.Outcome.ToString(), values.Message, values.Values is not null),
        new(nameof(GoalChangeService.StartAttemptAsync), started.Outcome.ToString(), started.Message,
            started.AttemptNumber is not null || started.StartDate is not null || started.Field is not null),
    ];

    private static void AssertEveryUseCaseRefused(IReadOnlyList<Call> calls, string outcome, string message)
    {
        Assert.Equal(FourUseCases, calls.Select(c => c.UseCase));
        Assert.All(calls, call =>
        {
            Assert.Equal(outcome, call.Outcome);
            Assert.Equal(message, call.Message);
            Assert.False(call.HasView);
        });
    }

    /// <summary>
    /// Both call sets by <paramref name="user"/> are refused with <paramref name="outcome"/> and <paramref name="message"/>,
    /// before any goal lookup and with nothing changed.
    /// </summary>
    private async Task AssertRefusedBeforeTheGoalAsync(ActingUser user, Arranged goals, string outcome, string message)
    {
        var before = Take();

        AssertEveryUseCaseRefused(await AcceptableCallsAsync(user, goals.Active, goals.Ended), outcome, message);
        AssertEveryUseCaseRefused(await FailingCallsAsync(user, goals.Active, goals.Ended), outcome, message);

        Assert.Equal(before.Lookups, _world.Goals.Lookups);
        AssertNothingChanged(before);
    }

    [Fact]
    public async Task US5_AS3_an_anonymous_visitor_gets_You_are_not_signed_in_from_every_goal_change_use_case_before_any_lookup()
    {
        var goals = await ArrangeAsync();

        await AssertRefusedBeforeTheGoalAsync(ActingUser.Anonymous, goals, "NotSignedIn", NotSignedIn);
    }

    [Fact]
    public async Task US5_AS2_an_administrator_gets_Goals_are_for_registered_users_from_every_goal_change_use_case_before_any_lookup()
    {
        var goals = await ArrangeAsync();
        var unstored = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.Administrator);

        foreach (var administrator in new[] { _world.Administrator, unstored })
        {
            await AssertRefusedBeforeTheGoalAsync(administrator, goals, "NotForAdministrators", NotForAdministrators);
        }
    }

    [Fact]
    public async Task FR_023_a_registered_user_whose_account_is_not_stored_gets_You_are_not_signed_in_before_any_lookup()
    {
        var goals = await ArrangeAsync();

        await AssertRefusedBeforeTheGoalAsync(ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.RegisteredUser), goals, "NotSignedIn", NotSignedIn);
    }

    [Fact]
    public async Task US5_AS1_B_on_As_goals_gets_This_goal_does_not_exist_from_every_use_case_the_same_as_for_an_unknown_goal_and_nothing_changes()
    {
        var goals = await ArrangeAsync();
        var unknown = new GoalId(Guid.NewGuid());
        var before = Take();

        foreach (var (active, ended) in new[] { (goals.Active, goals.Ended), (unknown, unknown) })
        {
            AssertEveryUseCaseRefused(await AcceptableCallsAsync(_world.OtherUser, active, ended), "NotFound", GoalNotFound);
            AssertEveryUseCaseRefused(await FailingCallsAsync(_world.OtherUser, active, ended), "NotFound", GoalNotFound);
        }

        // Only the owner-scoped reads ran, one per call, each under B's account.
        Assert.Equal(before.Lookups + 16, _world.Goals.Lookups);
        AssertNothingChanged(before);
        Assert.False(_world.GoalOf(goals.Active).IsEnded);
        Assert.Equal([15], _world.GoalOf(goals.Active).Versions.Select(v => v.Settings.Target.Amount!.Value));
        Assert.Single(_world.GoalOf(goals.Ended).Attempts);
        Assert.Equal(AsEndingNote, _world.GoalOf(goals.Ended).Attempt.Ending!.Note?.Value);
    }

    [Fact]
    public async Task US5_AS1_B_changing_ending_or_restarting_their_own_goal_leaves_As_goals_as_they_were()
    {
        var goals = await ArrangeAsync();
        var asGoals = new[] { _world.GoalOf(goals.Active), _world.GoalOf(goals.Ended) };

        var changed = await _world.Changes.ChangeSettingsAsync(_world.OtherUser, goals.Bs, new GoalSettingsInput { Target = MinutesDaily(20) }, None);
        var ended = await _world.Changes.EndAsync(_world.OtherUser, goals.Bs, Outcome.Abandoned, null, None);
        var started = await _world.Changes.StartAttemptAsync(_world.OtherUser, goals.Bs, new NewAttemptInput { Target = MinutesDaily(15), StartDate = Thu12 }, None);

        Assert.Equal(
            (SettingsChangeOutcome.Changed, GoalEndOutcome.Ended, AttemptStartOutcome.Started),
            (changed.Outcome, ended.Outcome, started.Outcome));
        Assert.Same(asGoals[0], _world.GoalOf(goals.Active));
        Assert.Same(asGoals[1], _world.GoalOf(goals.Ended));
    }

    [Fact]
    public async Task The_same_calls_succeed_for_A_so_each_refusal_comes_from_the_acting_user()
    {
        var goals = await ArrangeAsync();
        var writes = _world.Goals.ChangeWriteCount;

        var calls = await AcceptableCallsAsync(_world.User, goals.Active, goals.Ended);

        Assert.Equal(FourUseCases, calls.Select(c => c.UseCase));
        Assert.Equal(["Changed", "Ended", "Found", "Started"], calls.Select(c => c.Outcome));
        Assert.All(calls, call => Assert.True(call.HasView));
        Assert.Equal(writes + 3, _world.Goals.ChangeWriteCount);
    }
}
