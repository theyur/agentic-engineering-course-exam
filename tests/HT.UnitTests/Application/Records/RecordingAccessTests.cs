using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using static HT.UnitTests.Application.Records.RecordWorld;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Records;

/// <summary>
/// Recording privacy (US6-AS1 to AS4, SC-004, FR-017, FR-018) over the fakes of <see cref="RecordWorld"/>: for each of
/// the seven <see cref="RecordingService"/> use cases, another registered user, an administrator, an anonymous visitor
/// and an account that is not stored each get their refusal, no data and no record store call; A's records and notes
/// stay as they were. Every call is made with arguments that succeed for A, so the refusal comes from the acting user
/// alone (the last test proves it). User A's goal is in UTC and starts on Monday 16 November 2026, created at 08:00Z
/// that day; A records 25 min with a note on Monday 16 at 10:00Z, and the calls are made on Tuesday 17 at 10:00Z, when
/// Monday 16 and Tuesday 17 have begun.
/// </summary>
public sealed class RecordingAccessTests
{
    private const string GoalNotFound = "This goal does not exist.";
    private const string NotForAdministrators = "Goals are for registered users.";
    private const string NotSignedIn = "You are not signed in.";
    private const string RecordNotFound = "This record does not exist.";

    private const string ANote = "A's private evening note.";

    private static readonly DateOnly Mon16 = Date(2026, 11, 16);
    private static readonly DateOnly Tue17 = Date(2026, 11, 17);

    private static readonly DateTimeOffset Mon16Morning = Utc("2026-11-16T08:00:00Z");
    private static readonly DateTimeOffset Mon16TenOClock = Utc("2026-11-16T10:00:00Z");
    private static readonly DateTimeOffset Tue17TenOClock = Utc("2026-11-17T10:00:00Z");

    private static readonly string[] SevenUseCases =
    [
        nameof(RecordingService.RecordAsync),
        nameof(RecordingService.ChangeRecordAsync),
        nameof(RecordingService.DeleteRecordAsync),
        nameof(RecordingService.SaveNoteAsync),
        nameof(RecordingService.GetHistoryAsync),
        nameof(RecordingService.GetRecordableAsync),
        nameof(RecordingService.GetRecentNotesAsync),
    ];

    private readonly RecordWorld _world = new();

    private static GoalTargetFields Reading20MinDaily => DurationDays(20, DurationUnit.Minutes, BuildOrReduce.Build, AllDays);

    /// <summary>One call of a use case: its name, its outcome's name, its text and whether it returned any data.</summary>
    private sealed record Call(string UseCase, string Outcome, string? Message, bool HasData);

    /// <summary>A's goal with A's record of 25 min on Monday 16 and A's note on it; the clock is left at Tuesday 17, 10:00Z.</summary>
    private async Task<(GoalId Goal, RecordId Record)> AsGoalWithARecordAndANoteAsync()
    {
        var goal = await _world.CreateGoalAsync(Reading20MinDaily, Mon16, Mon16Morning);
        _world.At(Mon16TenOClock);
        var recorded = await _world.RecordAsync(goal, Mon16, Minutes(25, ANote));
        Assert.Equal(RecordWriteOutcome.Recorded, recorded.Outcome);
        _world.At(Tue17TenOClock);
        return (goal, recorded.RecordId!.Value);
    }

    /// <summary>
    /// Every use case of the slice by <paramref name="user"/> on <paramref name="goal"/>, each with arguments that succeed
    /// for the goal's owner: a record on Tuesday 17, a change and a delete of <paramref name="record"/>, a note on
    /// Tuesday 17, the history of Monday 16 to Tuesday 17, what can be recorded now and the recent notes.
    /// </summary>
    private async Task<IReadOnlyList<Call>> EveryUseCaseAsync(ActingUser user, GoalId goal, RecordId record)
    {
        var service = _world.Service;
        var none = CancellationToken.None;
        var recorded = await service.RecordAsync(user, goal, Tue17, Minutes(30, "Someone else's note."), none);
        var changed = await service.ChangeRecordAsync(user, goal, record, Minutes(5, "Someone else's note."), none);
        var deleted = await service.DeleteRecordAsync(user, goal, record, none);
        var noted = await service.SaveNoteAsync(user, goal, Tue17, "Someone else's note.", none);
        var history = await service.GetHistoryAsync(user, goal, Mon16, Tue17, none);
        var recordable = await service.GetRecordableAsync(user, goal, none);
        var notes = await service.GetRecentNotesAsync(user, goal, none);
        return
        [
            Of(nameof(service.RecordAsync), recorded),
            Of(nameof(service.ChangeRecordAsync), changed),
            Of(nameof(service.DeleteRecordAsync), deleted),
            new(nameof(service.SaveNoteAsync), noted.Outcome.ToString(), noted.Message, noted.Entry is not null),
            new(nameof(service.GetHistoryAsync), history.Outcome.ToString(), history.Message, history.History is not null),
            new(nameof(service.GetRecordableAsync), recordable.Outcome.ToString(), recordable.Message, recordable.History is not null),
            new(nameof(service.GetRecentNotesAsync), notes.Outcome.ToString(), notes.Message, notes.Notes.Count > 0),
        ];
    }

    private static Call Of(string useCase, RecordWriteResult result) =>
        new(useCase, result.Outcome.ToString(), result.Message, result.RecordId is not null || result.Entry is not null || result.IsCorrection);

    private static void AssertEveryUseCaseRefused(IReadOnlyList<Call> calls, string outcome, string message)
    {
        Assert.Equal(SevenUseCases, calls.Select(c => c.UseCase));
        Assert.All(calls, call =>
        {
            Assert.Equal(outcome, call.Outcome);
            Assert.Equal(message, call.Message);
            Assert.False(call.HasData);
        });
    }

    /// <summary>
    /// How many record store calls were made so far. Arranging A's record makes one, so a test counts before its calls
    /// and proves afterwards that the count did not grow: no record store call beyond the goal lookup.
    /// </summary>
    private int CallsSoFar() => _world.Records.Calls.Count;

    [Fact]
    public async Task US6_AS1_another_registered_user_gets_This_goal_does_not_exist_from_every_use_case_on_As_goal_and_nothing_changes()
    {
        var (goal, record) = await AsGoalWithARecordAndANoteAsync();
        var arranged = CallsSoFar();

        var calls = await EveryUseCaseAsync(_world.OtherUser, goal, record);

        AssertEveryUseCaseRefused(calls, "NotFound", GoalNotFound);
        Assert.Equal(arranged, CallsSoFar());
        AssertUnchanged(goal, record);
    }

    [Fact]
    public async Task US6_AS1_FR_017_another_users_result_for_As_goal_is_the_same_as_for_a_goal_that_does_not_exist()
    {
        var (goal, record) = await AsGoalWithARecordAndANoteAsync();

        var onAsGoal = await EveryUseCaseAsync(_world.OtherUser, goal, record);
        var onNoGoal = await EveryUseCaseAsync(_world.OtherUser, new GoalId(Guid.NewGuid()), new RecordId(Guid.NewGuid()));

        Assert.Equal(onNoGoal, onAsGoal);
    }

    [Fact]
    public async Task US6_AS3_an_administrator_gets_Goals_are_for_registered_users_and_no_data_from_every_use_case()
    {
        var (goal, record) = await AsGoalWithARecordAndANoteAsync();
        var arranged = CallsSoFar();
        var unstored = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.Administrator);

        foreach (var administrator in new[] { _world.Administrator, unstored })
        {
            var calls = await EveryUseCaseAsync(administrator, goal, record);

            AssertEveryUseCaseRefused(calls, "NotForAdministrators", NotForAdministrators);
        }

        Assert.Equal(arranged, CallsSoFar());
        AssertUnchanged(goal, record);
    }

    [Fact]
    public async Task US6_AS4_an_anonymous_visitor_gets_You_are_not_signed_in_from_every_use_case()
    {
        var (goal, record) = await AsGoalWithARecordAndANoteAsync();
        var arranged = CallsSoFar();

        var calls = await EveryUseCaseAsync(ActingUser.Anonymous, goal, record);

        AssertEveryUseCaseRefused(calls, "NotSignedIn", NotSignedIn);
        Assert.Equal(arranged, CallsSoFar());
        AssertUnchanged(goal, record);
    }

    [Fact]
    public async Task FR_018_a_registered_user_whose_account_is_not_stored_gets_You_are_not_signed_in_from_every_use_case()
    {
        var (goal, record) = await AsGoalWithARecordAndANoteAsync();
        var arranged = CallsSoFar();

        // A's own goal and record, under an account identifier that no stored account has.
        var calls = await EveryUseCaseAsync(ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.RegisteredUser), goal, record);

        AssertEveryUseCaseRefused(calls, "NotSignedIn", NotSignedIn);
        Assert.Equal(arranged, CallsSoFar());
        AssertUnchanged(goal, record);
    }

    [Fact]
    public async Task US6_AS2_B_changing_or_deleting_As_record_on_Bs_own_goal_gets_This_record_does_not_exist()
    {
        var bsGoal = await _world.CreateGoalAsync(Reading20MinDaily, Mon16, Mon16Morning, _world.OtherUser);
        var (goal, record) = await AsGoalWithARecordAndANoteAsync();
        var arranged = CallsSoFar();

        var changed = await _world.ChangeAsync(bsGoal, record, Minutes(5, "B's note."), _world.OtherUser);
        var deleted = await _world.DeleteAsync(bsGoal, record, _world.OtherUser);

        foreach (var result in new[] { changed, deleted })
        {
            Assert.Equal(RecordWriteOutcome.RecordNotFound, result.Outcome);
            Assert.Equal(RecordNotFound, result.Message);
            Assert.Null(result.RecordId);
            Assert.Null(result.Entry);
            Assert.False(result.IsCorrection);
        }

        // Only the record lookup ran, once per call, among the records of B's goal; no write was made.
        Assert.Equal(
            [nameof(IRecordStore.FindRecordDateAsync), nameof(IRecordStore.FindRecordDateAsync)],
            _world.Records.Calls.Skip(arranged));
        Assert.Empty(_world.Records.RecordsOfGoal(bsGoal));
        Assert.Empty(_world.Records.NotesOfGoal(bsGoal));
        AssertUnchanged(goal, record);
    }

    [Fact]
    public async Task The_arguments_of_the_refused_calls_succeed_for_the_owner_so_each_refusal_comes_from_the_acting_user()
    {
        var (goal, record) = await AsGoalWithARecordAndANoteAsync();

        var calls = await EveryUseCaseAsync(_world.User, goal, record);

        Assert.Equal(SevenUseCases, calls.Select(c => c.UseCase));
        Assert.Equal(["Recorded", "Changed", "Deleted", "Saved", "Found", "Found", "Found"], calls.Select(c => c.Outcome));
        Assert.All(calls, call =>
        {
            Assert.Null(call.Message);
            Assert.True(call.HasData);
        });
    }

    /// <summary>A's goal still has exactly its one record of 25 min on Monday 16 and its one note.</summary>
    private void AssertUnchanged(GoalId goal, RecordId record)
    {
        var stored = Assert.Single(_world.Records.RecordsOfGoal(goal));
        Assert.Equal(new GoalRecord(record, Mon16, RecordValue.FromAmount(25)), stored);
        var note = Assert.Single(_world.Records.NotesOfGoal(goal));
        Assert.Equal((Mon16, ANote), (note.Key, note.Value.Value));
    }
}
