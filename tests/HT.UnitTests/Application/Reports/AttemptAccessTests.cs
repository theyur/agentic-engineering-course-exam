using HT.Application.Goals;
using HT.Application.Reports;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.UnitTests.Application.Goals;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Reports;

/// <summary>
/// Slice 8 (T100; US5-AS1 to AS4, FR-022, FR-023, SC-006): who may read which attempts, for every
/// <see cref="AttemptService"/> method (<c>GetAttemptsAsync</c>, <c>GetAttemptAsync</c>, <c>GetGoalHistoryAsync</c>), on
/// <see cref="AttemptWorld"/>. User A's goal "Zq A private goal" (Reading, 10 min daily, from Monday 2 November 2026,
/// created on Sunday 1 November at 10:00Z) has 10 minutes stored on 2 to 6 November, was ended on Monday 9 November at
/// 10:00Z with "Abandoned" and an ending note, and restarted as attempt 2 from Tuesday 10 November. User B's goal
/// "Zq B walk", with the same settings, was ended at the same time with "Achieved" and a note of B's. The reads are made
/// on Thursday 12 November at 10:00Z. Every refusal returns no view; a refusal before the goal looks up no goal and calls
/// no record store member, and B's read of A's goal reads none of A's records (only the owner-scoped goal lookup runs).
/// No read writes anything. Every instant is a UTC literal; the users have no stored zone, so everything is in UTC.
/// </summary>
public sealed class AttemptAccessTests
{
    private const string GoalNotFound = "This goal does not exist.";
    private const string NotForAdministrators = "Goals are for registered users.";
    private const string NotSignedIn = "You are not signed in.";

    private const string AsGoalName = "Zq A private goal";
    private const string AsNote = "Zq A private ending note.";
    private const string BsGoalName = "Zq B walk";
    private const string BsNote = "Zq B own ending note.";

    private static readonly CancellationToken None = CancellationToken.None;

    private static readonly DateOnly Mon2 = Date(2026, 11, 2);
    private static readonly DateOnly Mon9 = Date(2026, 11, 9);
    private static readonly DateOnly Tue10 = Date(2026, 11, 10);

    private static readonly DateTimeOffset Created = Utc("2026-11-01T10:00:00Z");
    private static readonly DateTimeOffset Mon9At10 = Utc("2026-11-09T10:00:00Z");
    private static readonly DateTimeOffset Thu12At10 = Utc("2026-11-12T10:00:00Z");

    private static readonly string[] ThreeUseCases =
    [
        nameof(AttemptService.GetAttemptsAsync),
        nameof(AttemptService.GetAttemptAsync),
        nameof(AttemptService.GetGoalHistoryAsync),
    ];

    private readonly AttemptWorld _world = new();

    private static GoalTargetFields MinutesDaily(int minutes) => DurationDays(minutes, DurationUnit.Minutes, BuildOrReduce.Build, AllDays);

    /// <summary>One call of a use case: its name, its outcome's name, its text and whether it returned a view.</summary>
    private sealed record Call(string UseCase, string Outcome, string? Message, bool HasView);

    /// <summary>A's restarted goal and B's ended goal; the clock is left at <see cref="Thu12At10"/>.</summary>
    private async Task<(GoalId A, GoalId B)> ArrangeAsync()
    {
        var a = await _world.CreateNamedGoalAsync(AsGoalName, MinutesDaily(10), Mon2, Created);
        var b = await _world.CreateNamedGoalAsync(BsGoalName, MinutesDaily(10), Mon2, Created, _world.OtherUser);
        for (var day = Mon2; day < Mon2.AddDays(5); day = day.AddDays(1))
        {
            _world.SeedAmount(a, day, 10);
            _world.SeedAmount(b, day, 10);
        }

        _world.At(Mon9At10);
        Assert.Equal(GoalEndOutcome.Ended, (await _world.Changes.EndAsync(_world.User, a, Outcome.Abandoned, AsNote, None)).Outcome);
        Assert.Equal(GoalEndOutcome.Ended, (await _world.Changes.EndAsync(_world.OtherUser, b, Outcome.Achieved, BsNote, None)).Outcome);
        var started = await _world.Changes.StartAttemptAsync(_world.User, a, new NewAttemptInput { Target = MinutesDaily(10), StartDate = Tue10 }, None);
        Assert.Equal(AttemptStartOutcome.Started, started.Outcome);
        _world.At(Thu12At10);
        return (a, b);
    }

    /// <summary>
    /// The three use cases by <paramref name="user"/> on <paramref name="goal"/>: its attempts, its attempt
    /// <paramref name="number"/> and the goal history.
    /// </summary>
    private async Task<IReadOnlyList<Call>> EveryUseCaseAsync(ActingUser user, GoalId goal, int number)
    {
        var service = _world.Attempts;
        var attempts = await service.GetAttemptsAsync(user, goal, None);
        var attempt = await service.GetAttemptAsync(user, goal, number, None);
        var history = await service.GetGoalHistoryAsync(user, None);
        return
        [
            new(nameof(service.GetAttemptsAsync), attempts.Outcome.ToString(), attempts.Message, attempts.Attempts.Count > 0),
            new(nameof(service.GetAttemptAsync), attempt.Outcome.ToString(), attempt.Message, attempt.Details is not null),
            new(nameof(service.GetGoalHistoryAsync), history.Outcome.ToString(), history.Message, history.Entries.Count > 0),
        ];
    }

    private static void AssertEveryUseCaseRefused(IReadOnlyList<Call> calls, string outcome, string message)
    {
        Assert.Equal(ThreeUseCases, calls.Select(c => c.UseCase));
        Assert.All(calls, call =>
        {
            Assert.Equal(outcome, call.Outcome);
            Assert.Equal(message, call.Message);
            Assert.False(call.HasView);
        });
    }

    /// <summary>
    /// The three use cases by <paramref name="user"/>, on A's goal with a valid number (1) and with one it does not have (9),
    /// are refused with <paramref name="outcome"/> and <paramref name="message"/>, before any goal lookup, with no record
    /// store call and nothing written.
    /// </summary>
    private async Task AssertRefusedBeforeTheGoalAsync(ActingUser user, GoalId a, string outcome, string message)
    {
        var (lookups, writes, calls, stored) = (_world.Goals.Lookups, _world.Goals.ChangeWriteCount, _world.Records.Calls.Count, _world.GoalOf(a));

        foreach (var number in new[] { 1, 9 })
        {
            AssertEveryUseCaseRefused(await EveryUseCaseAsync(user, a, number), outcome, message);
        }

        Assert.Equal((lookups, writes, calls), (_world.Goals.Lookups, _world.Goals.ChangeWriteCount, _world.Records.Calls.Count));
        Assert.Same(stored, _world.GoalOf(a));
    }

    [Fact]
    public async Task US5_AS3_an_anonymous_visitor_gets_You_are_not_signed_in_from_every_attempt_read_and_the_goal_history_before_any_lookup()
    {
        var (a, _) = await ArrangeAsync();

        await AssertRefusedBeforeTheGoalAsync(ActingUser.Anonymous, a, "NotSignedIn", NotSignedIn);
    }

    [Fact]
    public async Task US5_AS2_an_administrator_gets_Goals_are_for_registered_users_from_every_attempt_read_and_the_goal_history_before_any_lookup()
    {
        var (a, _) = await ArrangeAsync();
        var unstored = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.Administrator);

        foreach (var administrator in new[] { _world.Administrator, unstored })
        {
            await AssertRefusedBeforeTheGoalAsync(administrator, a, "NotForAdministrators", NotForAdministrators);
        }
    }

    [Fact]
    public async Task FR_023_a_registered_user_whose_account_is_not_stored_gets_You_are_not_signed_in_before_any_lookup()
    {
        var (a, _) = await ArrangeAsync();

        await AssertRefusedBeforeTheGoalAsync(ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.RegisteredUser), a, "NotSignedIn", NotSignedIn);
    }

    [Fact]
    public async Task US5_AS1_B_reading_As_attempts_or_one_of_them_gets_This_goal_does_not_exist_the_same_as_for_an_unknown_goal()
    {
        var (a, _) = await ArrangeAsync();
        var unknown = new GoalId(Guid.NewGuid());
        var (lookups, writes, stored) = (_world.Goals.Lookups, _world.Goals.ChangeWriteCount, _world.GoalOf(a));

        foreach (var goal in new[] { a, unknown })
        {
            var attempts = await _world.Attempts.GetAttemptsAsync(_world.OtherUser, goal, None);
            Assert.Equal((AttemptListOutcome.NotFound, GoalNotFound), (attempts.Outcome, attempts.Message));
            Assert.Empty(attempts.Attempts);

            // Attempt 1 (ended) and 2 (the latest) exist on A's goal; 9 does not. The goal is checked before the number.
            foreach (var number in new[] { 1, 2, 9 })
            {
                var attempt = await _world.Attempts.GetAttemptAsync(_world.OtherUser, goal, number, None);
                Assert.Equal((AttemptReadOutcome.NotFound, GoalNotFound), (attempt.Outcome, attempt.Message));
                Assert.Null(attempt.Details);
            }
        }

        // Only the owner-scoped goal lookups ran, one per call; no record or note of A's goal was read, nothing was written.
        Assert.Equal(lookups + 8, _world.Goals.Lookups);
        Assert.Empty(_world.Records.Calls);
        Assert.Equal(writes, _world.Goals.ChangeWriteCount);
        Assert.Same(stored, _world.GoalOf(a));
    }

    [Fact]
    public async Task US5_AS4_Bs_goal_history_contains_Bs_ended_attempt_and_none_of_As()
    {
        var (a, b) = await ArrangeAsync();

        var history = await _world.Attempts.GetGoalHistoryAsync(_world.OtherUser, None);

        Assert.Equal(GoalHistoryListOutcome.Found, history.Outcome);
        var entry = Assert.Single(history.Entries);
        Assert.Equal(
            (b, BsGoalName, 1, Mon9, Outcome.Achieved, (string?)BsNote, true),
            (entry.GoalId, entry.Name, entry.AttemptNumber, entry.EndingDate, entry.Outcome, entry.EndingNote, entry.IsLatestAttempt));
        Assert.DoesNotContain(history.Entries, e => e.GoalId == a || e.Name == AsGoalName || e.EndingNote == AsNote);
        Assert.Empty(_world.Records.Calls);
    }

    [Fact]
    public async Task The_same_reads_succeed_for_A_so_each_refusal_comes_from_the_acting_user()
    {
        var (a, b) = await ArrangeAsync();

        var calls = await EveryUseCaseAsync(_world.User, a, 1);

        Assert.Equal(ThreeUseCases, calls.Select(c => c.UseCase));
        Assert.All(calls, call =>
        {
            Assert.Equal("Found", call.Outcome);
            Assert.Null(call.Message);
            Assert.True(call.HasView);
        });
        var history = await _world.Attempts.GetGoalHistoryAsync(_world.User, None);
        var entry = Assert.Single(history.Entries);
        Assert.Equal((a, 1, (string?)AsNote, false), (entry.GoalId, entry.AttemptNumber, entry.EndingNote, entry.IsLatestAttempt));
        Assert.DoesNotContain(history.Entries, e => e.GoalId == b || e.EndingNote == BsNote);
        Assert.Equal([2, 1], (await _world.Attempts.GetAttemptsAsync(_world.User, a, None)).Attempts.Select(v => v.Number));
    }
}
