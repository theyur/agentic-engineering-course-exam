using HT.Application;
using HT.Application.Goals;
using HT.Application.Records;
using HT.Application.Reports;
using HT.Domain.Accounts;
using HT.Domain.Evaluation;
using HT.Domain.Exemptions;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using HT.Domain.Time;
using HT.IntegrationTests.Habits;
using HT.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using static HT.IntegrationTests.Goals.GoalSamples;

namespace HT.IntegrationTests.Goals;

/// <summary>
/// Slice 8 (T087; FR-011, FR-012, FR-014, research R-05): ending, restarting and reading attempts through
/// <see cref="GoalChangeService"/> and <see cref="AttemptService"/> on SQL Server; two starts racing, and an ending and a
/// start racing in both orders, each forced by a goal-store wrapper that runs the other use case in a second scope before
/// the first one's conditional write; and the cost of the attempt list, counted by a record-store wrapper. Every provider
/// has its own clock, so the fixture's shared clock is untouched. "Meditation, 10 min daily" (Build, 10 minutes, all
/// seven days) is created on Sunday 1 November 2026 at 10:00Z with the start date Monday 2 November, in UTC, and ended on
/// Monday 16 November at 10:00Z (ending date Monday 16, last date Sunday 15 November). Shared database: own prefixes
/// ("it-ga-", "It-Ga"), checks per goal.
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class GoalAttemptTests(DatabaseFixture database)
{
    private const string NotEnded = "Only an ended goal can start a new attempt.";

    private static readonly DateTimeOffset Created = new(2026, 11, 1, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset EndedAt = new(2026, 11, 16, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Wed18At8 = new(2026, 11, 18, 8, 0, 0, TimeSpan.Zero);

    private static readonly DateOnly Mon2 = new(2026, 11, 2);
    private static readonly DateOnly Sun15 = new(2026, 11, 15);
    private static readonly DateOnly Mon16 = new(2026, 11, 16);
    private static readonly DateOnly Wed18 = new(2026, 11, 18);
    private static readonly DateOnly Thu19 = new(2026, 11, 19);
    private static readonly DateOnly Fri20 = new(2026, 11, 20);

    private static CancellationToken None => CancellationToken.None;

    private static GoalTargetFields MinutesDaily(int minutes) => new()
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = minutes, DurationUnit = DurationUnit.Minutes,
        ScheduleKind = ScheduleKind.FixedDays, Days = Enum.GetValues<DayOfWeek>().ToHashSet(),
    };

    private ServiceProvider BuildServices(out FakeTimeProvider clock, Hooks? hooks = null)
    {
        clock = new FakeTimeProvider(Created);
        hooks ??= new Hooks();
        var services = new ServiceCollection()
            .AddLogging()
            .AddHtApplication()
            .AddHtPersistence(database.ConnectionString);
        services.AddSingleton<TimeProvider>(clock);
        var realGoals = services.Last(d => d.ServiceType == typeof(IGoalStore));
        services.Remove(realGoals);
        services.AddScoped<IGoalStore>(provider => new HookedGoalStore(
            (IGoalStore)ActivatorUtilities.CreateInstance(provider, realGoals.ImplementationType!),
            provider.GetRequiredService<IServiceScopeFactory>(),
            hooks));
        var realRecords = services.Last(d => d.ServiceType == typeof(IRecordStore));
        services.Remove(realRecords);
        services.AddScoped<IRecordStore>(provider => new CountingRecordStore(
            (IRecordStore)ActivatorUtilities.CreateInstance(provider, realRecords.ImplementationType!), hooks));
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
    }

    private static async Task<T> InScopeAsync<T, TService>(IServiceProvider services, Func<TService, Task<T>> call)
        where TService : notnull
    {
        await using var scope = services.CreateAsyncScope();
        return await call(scope.ServiceProvider.GetRequiredService<TService>());
    }

    private static Task<GoalEndResult> EndAsync(IServiceProvider services, ActingUser user, GoalId goal, string? note = null) =>
        InScopeAsync<GoalEndResult, GoalChangeService>(services, s => s.EndAsync(user, goal, Outcome.Abandoned, note, None));

    private static Task<AttemptStartResult> StartAsync(IServiceProvider services, ActingUser user, GoalId goal, DateOnly start, int minutes = 10) =>
        InScopeAsync<AttemptStartResult, GoalChangeService>(services, s => s.StartAttemptAsync(
            user, goal, new NewAttemptInput { Target = MinutesDaily(minutes), StartDate = start }, None));

    private static Task<AttemptListResult> AttemptsAsync(IServiceProvider services, ActingUser user, GoalId goal) =>
        InScopeAsync<AttemptListResult, AttemptService>(services, s => s.GetAttemptsAsync(user, goal, None));

    private static async Task RecordAsync(IServiceProvider services, FakeTimeProvider clock, ActingUser user, GoalId goal, DateOnly date)
    {
        clock.SetUtcNow(new DateTimeOffset(date.ToDateTime(new TimeOnly(20, 0)), TimeSpan.Zero));
        var result = await InScopeAsync<RecordWriteResult, RecordingService>(services, r => r.RecordAsync(user, goal, date, new RecordInput { Amount = 10 }, None));
        Assert.Equal(RecordWriteOutcome.Recorded, result.Outcome);
    }

    /// <summary>A registered user in UTC with the goal of the class summary.</summary>
    private async Task<(ActingUser User, AccountId Owner, GoalId Goal)> ArrangeAsync(IServiceProvider services, string name)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var user = await HabitAccounts.RegisteredUserAsync(services, database.ConnectionString, $"it-ga-{name}-{suffix}@example.com");
        var owner = user.AccountId!.Value;
        var habit = await CatalogHabitAsync(services, $"It-Ga {name} {suffix}");
        var created = await InScopeAsync<GoalCreateResult, GoalService>(services, g => g.CreateAsync(
            user, new GoalInput { Habit = new GoalHabit(habit, HabitKind.Catalog), Target = MinutesDaily(10), StartDate = Mon2 }, None));
        Assert.Equal(GoalCreateOutcome.Created, created.Outcome);
        return (user, owner, created.GoalId!.Value);
    }

    /// <summary>The goal of the class summary, ended at <see cref="EndedAt"/>; the clock stays there.</summary>
    private async Task<(ActingUser User, AccountId Owner, GoalId Goal)> ArrangeEndedAsync(IServiceProvider services, FakeTimeProvider clock, string name)
    {
        var arranged = await ArrangeAsync(services, name);
        clock.SetUtcNow(EndedAt);
        Assert.Equal(GoalEndOutcome.Ended, (await EndAsync(services, arranged.User, arranged.Goal)).Outcome);
        return arranged;
    }

    private static async Task<Goal> StoredAsync(IServiceProvider services, AccountId owner, GoalId goal)
    {
        await using var scope = services.CreateAsyncScope();
        var stored = await scope.ServiceProvider.GetRequiredService<IGoalStore>().FindAsync(owner, goal, None);
        Assert.NotNull(stored);
        return stored.Goal;
    }

    [Fact]
    public async Task Ending_restarting_and_reading_attempts_through_the_real_stores()
    {
        await using var services = BuildServices(out var clock);
        var (user, owner, goal) = await ArrangeAsync(services, "read");
        for (var date = Mon2; date <= new DateOnly(2026, 11, 13); date = date.AddDays(1))
        {
            await RecordAsync(services, clock, user, goal, date);
        }

        clock.SetUtcNow(EndedAt);
        Assert.Equal(GoalEndOutcome.Ended, (await EndAsync(services, user, goal, "Too busy this month.")).Outcome);
        clock.SetUtcNow(Wed18At8);
        var values = await InScopeAsync<NewAttemptValuesResult, GoalChangeService>(services, s => s.GetNewAttemptValuesAsync(user, goal, None));
        Assert.Equal((NewAttemptValuesOutcome.Found, Wed18, 2, (int?)10), (values.Outcome, values.Values!.StartDate, values.Values.AttemptNumber, values.Values.Amount));

        var started = await StartAsync(services, user, goal, Wed18);

        Assert.Equal((AttemptStartOutcome.Started, "attempt started", (int?)2, (DateOnly?)Wed18), (started.Outcome, started.Message, started.AttemptNumber, started.StartDate));
        var stored = await StoredAsync(services, owner, goal);
        Assert.Equal([(1, Mon2, true), (2, Wed18, false)], stored.Attempts.Select(a => (a.Attempt.Number, a.Attempt.StartDate, a.Attempt.Ending is not null)));
        Assert.Equal([(Wed18, "UTC")], stored.Versions.Select(v => (v.AppliesFrom, v.Settings.TimeZone.Id)));
        for (var date = Wed18; date <= Fri20; date = date.AddDays(1))
        {
            await RecordAsync(services, clock, user, goal, date);
        }

        clock.SetUtcNow(new DateTimeOffset(2026, 11, 23, 12, 0, 0, TimeSpan.Zero));
        var list = await AttemptsAsync(services, user, goal);
        Assert.Equal(AttemptListOutcome.Found, list.Outcome);
        Assert.Equal([2, 1], list.Attempts.Select(a => a.Number));
        Assert.Equal(("100% (3 of 3)", "3 commitments", true), (list.Attempts[0].Stats.ConsistencyText, list.Attempts[0].Stats.CurrentStreakText, list.Attempts[0].IsLatest));
        var first = list.Attempts[1];
        Assert.Equal(("85% (12 of 14)", "12 commitments", (DateOnly?)Mon16, (DateOnly?)Sun15, (Outcome?)Outcome.Abandoned, "Too busy this month."),
            (first.Stats.ConsistencyText, first.Stats.LongestStreakText, first.EndingDate, first.LastDate, first.Outcome, first.EndingNote));

        var one = await InScopeAsync<AttemptReadResult, AttemptService>(services, s => s.GetAttemptAsync(user, goal, 1, None));
        Assert.Equal(AttemptReadOutcome.Found, one.Outcome);
        Assert.Equal(Enumerable.Range(0, 14).Select(day => Sun15.AddDays(-day)), one.Details!.History.Entries.Select(e => e.FirstDate));
        Assert.Equal([EvaluationState.Missed, EvaluationState.Missed], one.Details.History.Entries.Take(2).Select(e => e.State));
        Assert.Equal(14, one.Details.Recent.Results.Count);
        var missing = await InScopeAsync<AttemptReadResult, AttemptService>(services, s => s.GetAttemptAsync(user, goal, 3, None));
        Assert.Equal((AttemptReadOutcome.AttemptNotFound, "This attempt does not exist."), (missing.Outcome, missing.Message));
    }

    [Fact]
    public async Task Two_starts_racing_the_second_write_is_stale_reads_again_and_gives_Only_an_ended_goal()
    {
        var hooks = new Hooks();
        await using var services = BuildServices(out var clock, hooks);
        var (user, owner, goal) = await ArrangeEndedAsync(services, clock, "two-starts");
        clock.SetUtcNow(Wed18At8);

        // The other start commits in another scope after this start read the ended goal and before its write.
        hooks.BeforeStartWrite = async provider => Assert.Equal(AttemptStartOutcome.Started,
            (await provider.GetRequiredService<GoalChangeService>().StartAttemptAsync(
                user, goal, new NewAttemptInput { Target = MinutesDaily(10), StartDate = Wed18 }, None)).Outcome);
        var result = await StartAsync(services, user, goal, Thu19, 5);

        Assert.Equal((AttemptStartOutcome.NotEnded, NotEnded, (int?)null), (result.Outcome, result.Message, result.AttemptNumber));
        Assert.Equal(2, hooks.StartWrites);
        var stored = await StoredAsync(services, owner, goal);
        Assert.Equal([1, 2], stored.Attempts.Select(a => a.Attempt.Number));
        Assert.Equal((Wed18, (int?)10), (stored.Attempt.StartDate, stored.Versions[0].Settings.Target.Amount));
    }

    [Fact]
    public async Task A_start_during_an_endings_write_finds_the_goal_not_ended_and_the_ending_then_commits()
    {
        var hooks = new Hooks();
        await using var services = BuildServices(out var clock, hooks);
        var (user, owner, goal) = await ArrangeAsync(services, "start-in-end");
        clock.SetUtcNow(EndedAt);
        AttemptStartResult? start = null;

        // The start runs in another scope after the ending read the goal and before the ending's write.
        hooks.BeforeEndWrite = async provider => start = await provider.GetRequiredService<GoalChangeService>().StartAttemptAsync(
            user, goal, new NewAttemptInput { Target = MinutesDaily(10), StartDate = new DateOnly(2026, 11, 17) }, None);
        var ended = await EndAsync(services, user, goal);

        Assert.Equal((AttemptStartOutcome.NotEnded, NotEnded), (start!.Outcome, start.Message));
        Assert.Equal((GoalEndOutcome.Ended, (DateOnly?)Mon16), (ended.Outcome, ended.EndingDate));
        var stored = await StoredAsync(services, owner, goal);
        Assert.Single(stored.Attempts);
        Assert.True(stored.IsEnded);
    }

    [Fact]
    public async Task An_ending_committed_during_a_starts_write_makes_the_start_read_again_and_take_the_next_number()
    {
        var hooks = new Hooks();
        await using var services = BuildServices(out var clock, hooks);
        var (user, owner, goal) = await ArrangeEndedAsync(services, clock, "end-in-start");
        clock.SetUtcNow(Wed18At8);

        // In another scope, after this start read the ended goal and before its write: attempt 2 starts on Wednesday 18 and
        // is ended at once (ending date Wednesday 18, so it has no date). This start's write is then stale; on its second
        // read the goal is ended again, its Thursday 19 is after the new ending date, and it becomes attempt 3.
        hooks.BeforeStartWrite = async provider =>
        {
            var changes = provider.GetRequiredService<GoalChangeService>();
            Assert.Equal(AttemptStartOutcome.Started,
                (await changes.StartAttemptAsync(user, goal, new NewAttemptInput { Target = MinutesDaily(10), StartDate = Wed18 }, None)).Outcome);
            Assert.Equal((GoalEndOutcome.Ended, Wed18),
                Pair(await changes.EndAsync(user, goal, Outcome.NoLongerRelevant, null, None)));
        };
        var result = await StartAsync(services, user, goal, Thu19, 5);

        Assert.Equal((AttemptStartOutcome.Started, (int?)3, (DateOnly?)Thu19), (result.Outcome, result.AttemptNumber, result.StartDate));
        Assert.Equal(3, hooks.StartWrites);
        var stored = await StoredAsync(services, owner, goal);
        Assert.Equal([(1, true), (2, true), (3, false)], stored.Attempts.Select(a => (a.Attempt.Number, a.Attempt.Ending is not null)));
        Assert.Equal((Thu19, (int?)5), (stored.Attempt.StartDate, stored.Versions[0].Settings.Target.Amount));

        static (GoalEndOutcome, DateOnly?) Pair(GoalEndResult ended) => (ended.Outcome, ended.EndingDate);
    }

    [Fact]
    public async Task The_attempt_list_costs_one_record_read_per_attempt_with_a_date()
    {
        var hooks = new Hooks();
        await using var services = BuildServices(out var clock, hooks);
        var (user, _, goal) = await ArrangeEndedAsync(services, clock, "cost");
        clock.SetUtcNow(Wed18At8);
        Assert.Equal(AttemptStartOutcome.Started, (await StartAsync(services, user, goal, Wed18)).Outcome);
        Assert.Equal(GoalEndOutcome.Ended, (await EndAsync(services, user, goal)).Outcome);
        Assert.Equal(AttemptStartOutcome.Started, (await StartAsync(services, user, goal, Thu19)).Outcome);
        clock.SetUtcNow(new DateTimeOffset(2026, 11, 23, 12, 0, 0, TimeSpan.Zero));
        var before = hooks.RecordReads;

        var list = await AttemptsAsync(services, user, goal);

        // Attempts 3 and 1 have dates; attempt 2 ended on its start date and has none, so it reads no record.
        Assert.Equal([3, 2, 1], list.Attempts.Select(a => a.Number));
        Assert.Equal([false, true, false], list.Attempts.Select(a => a.HasNoDate));
        Assert.Equal(2, hooks.RecordReads - before);
    }

    /// <summary>One-shot actions run in another scope before the next start or ending write, and the counts of those writes and of record reads.</summary>
    private sealed class Hooks
    {
        private Func<IServiceProvider, Task>? _beforeStartWrite;
        private Func<IServiceProvider, Task>? _beforeEndWrite;
        private int _startWrites;
        private int _recordReads;

        public Func<IServiceProvider, Task>? BeforeStartWrite
        {
            set => Volatile.Write(ref _beforeStartWrite, value);
        }

        public Func<IServiceProvider, Task>? BeforeEndWrite
        {
            set => Volatile.Write(ref _beforeEndWrite, value);
        }

        public int StartWrites => Volatile.Read(ref _startWrites);

        public int RecordReads => Volatile.Read(ref _recordReads);

        public Func<IServiceProvider, Task>? TakeBeforeStartWrite()
        {
            Interlocked.Increment(ref _startWrites);
            return Interlocked.Exchange(ref _beforeStartWrite, null);
        }

        public Func<IServiceProvider, Task>? TakeBeforeEndWrite() => Interlocked.Exchange(ref _beforeEndWrite, null);

        public void CountRecordRead() => Interlocked.Increment(ref _recordReads);
    }

    /// <summary>The real goal store, with the hooks' actions run in another scope before its start and ending writes.</summary>
    private sealed class HookedGoalStore(IGoalStore inner, IServiceScopeFactory scopes, Hooks hooks) : IGoalStore
    {
        public Task<GoalWriteOutcome> CreateAsync(
            AccountId owner, NewGoal goal, NamedTimeZone expectedZone, NamedTimeZone defaultZone, CancellationToken cancellationToken) =>
            inner.CreateAsync(owner, goal, expectedZone, defaultZone, cancellationToken);

        public Task<OwnerGoals?> ReadOwnerGoalsAsync(AccountId owner, CancellationToken cancellationToken) =>
            inner.ReadOwnerGoalsAsync(owner, cancellationToken);

        public Task<bool> TryChangeTimeZoneAsync(
            AccountId owner, string? expectedStoredZone, int expectedRevision, NamedTimeZone next,
            IReadOnlyList<GoalVersionWrite> writes, CancellationToken cancellationToken) =>
            inner.TryChangeTimeZoneAsync(owner, expectedStoredZone, expectedRevision, next, writes, cancellationToken);

        public Task<OwnerGoal?> ReadOwnerGoalAsync(AccountId owner, GoalId goal, CancellationToken cancellationToken) =>
            inner.ReadOwnerGoalAsync(owner, goal, cancellationToken);

        public Task<GoalChangeWriteOutcome> TryChangeSettingsAsync(
            AccountId owner, string? expectedStoredZone, int expectedRevision, GoalSettingsWrite write, CancellationToken cancellationToken) =>
            inner.TryChangeSettingsAsync(owner, expectedStoredZone, expectedRevision, write, cancellationToken);

        public async Task<GoalChangeWriteOutcome> TryEndAsync(
            AccountId owner, string? expectedStoredZone, int expectedRevision, GoalEndingWrite write, CancellationToken cancellationToken)
        {
            await RunAsync(hooks.TakeBeforeEndWrite());
            return await inner.TryEndAsync(owner, expectedStoredZone, expectedRevision, write, cancellationToken);
        }

        public async Task<GoalChangeWriteOutcome> TryStartAttemptAsync(
            AccountId owner, string? expectedStoredZone, int expectedRevision, GoalAttemptWrite write, CancellationToken cancellationToken)
        {
            await RunAsync(hooks.TakeBeforeStartWrite());
            return await inner.TryStartAttemptAsync(owner, expectedStoredZone, expectedRevision, write, cancellationToken);
        }

        public Task<IReadOnlyList<StoredGoal>> ListAsync(AccountId owner, CancellationToken cancellationToken) =>
            inner.ListAsync(owner, cancellationToken);

        public Task<StoredGoal?> FindAsync(AccountId owner, GoalId id, CancellationToken cancellationToken) =>
            inner.FindAsync(owner, id, cancellationToken);

        public Task<int> CountUsingCustomHabitAsync(AccountId owner, HabitId habit, CancellationToken cancellationToken) =>
            inner.CountUsingCustomHabitAsync(owner, habit, cancellationToken);

        public Task<IReadOnlyDictionary<HabitId, int>> CountByCatalogHabitAsync(AccountId owner, CancellationToken cancellationToken) =>
            inner.CountByCatalogHabitAsync(owner, cancellationToken);

        public Task<IReadOnlyList<StoredGoal>> ListOnCatalogHabitAsync(AccountId owner, HabitId habit, CancellationToken cancellationToken) =>
            inner.ListOnCatalogHabitAsync(owner, habit, cancellationToken);

        public Task<int> CountUsingCatalogHabitAsync(HabitId habit, CancellationToken cancellationToken) =>
            inner.CountUsingCatalogHabitAsync(habit, cancellationToken);

        private async Task RunAsync(Func<IServiceProvider, Task>? action)
        {
            if (action is not null)
            {
                await using var scope = scopes.CreateAsyncScope();
                await action(scope.ServiceProvider);
            }
        }
    }

    /// <summary>The real record store, counting its <see cref="IRecordStore.ReadAsync"/> calls.</summary>
    private sealed class CountingRecordStore(IRecordStore inner, Hooks hooks) : IRecordStore
    {
        public Task<RecordStoreWrite> AddAsync(
            AccountId owner, GoalId goal, EntrySpan entry, bool isCommitment, DateOnly date, RecordValue value, NoteWrite note, int limit,
            CancellationToken cancellationToken) =>
            inner.AddAsync(owner, goal, entry, isCommitment, date, value, note, limit, cancellationToken);

        public Task<DateOnly?> FindRecordDateAsync(AccountId owner, GoalId goal, RecordId record, CancellationToken cancellationToken) =>
            inner.FindRecordDateAsync(owner, goal, record, cancellationToken);

        public Task<RecordStoreWrite> ChangeAsync(
            AccountId owner, GoalId goal, RecordId record, EntrySpan entry, RecordValue value, NoteWrite note, CancellationToken cancellationToken) =>
            inner.ChangeAsync(owner, goal, record, entry, value, note, cancellationToken);

        public Task<RecordStoreWrite> DeleteAsync(AccountId owner, GoalId goal, RecordId record, EntrySpan entry, CancellationToken cancellationToken) =>
            inner.DeleteAsync(owner, goal, record, entry, cancellationToken);

        public Task<RecordStoreWrite> SaveNoteAsync(AccountId owner, GoalId goal, EntrySpan entry, NoteText? note, CancellationToken cancellationToken) =>
            inner.SaveNoteAsync(owner, goal, entry, note, cancellationToken);

        public Task<StoredEntries> ReadAsync(AccountId owner, GoalId goal, DateOnly from, DateOnly to, CancellationToken cancellationToken)
        {
            hooks.CountRecordRead();
            return inner.ReadAsync(owner, goal, from, to, cancellationToken);
        }

        public Task<IReadOnlyList<DateOnly>> ReadRecentNoteKeysAsync(AccountId owner, GoalId goal, int count, CancellationToken cancellationToken) =>
            inner.ReadRecentNoteKeysAsync(owner, goal, count, cancellationToken);

        public Task<IReadOnlyList<DateOnly>> ReadRecentNoteKeysAsync(
            AccountId owner, GoalId goal, DateOnly from, DateOnly? through, int count, CancellationToken cancellationToken) =>
            inner.ReadRecentNoteKeysAsync(owner, goal, from, through, count, cancellationToken);

        public Task<GoalExemptions> ReadExemptionsAsync(AccountId owner, GoalId goal, CancellationToken cancellationToken) =>
            inner.ReadExemptionsAsync(owner, goal, cancellationToken);

        public Task<ExemptionStoreWrite> ExcuseAsync(
            AccountId owner, GoalId goal, DateOnly date, ExemptionReason? reason, CancellationToken cancellationToken) =>
            inner.ExcuseAsync(owner, goal, date, reason, cancellationToken);

        public Task<ExemptionStoreWrite> RemoveExcusalAsync(AccountId owner, GoalId goal, DateOnly date, CancellationToken cancellationToken) =>
            inner.RemoveExcusalAsync(owner, goal, date, cancellationToken);

        public Task<ExemptionStoreWrite> AddPauseAsync(
            AccountId owner, GoalId goal, DateOnly firstDay, DateOnly lastDay, ExemptionReason? reason, CancellationToken cancellationToken) =>
            inner.AddPauseAsync(owner, goal, firstDay, lastDay, reason, cancellationToken);

        public Task<ExemptionStoreWrite> SetPauseLastDayAsync(
            AccountId owner, GoalId goal, PauseId pause, DateOnly lastDay, CancellationToken cancellationToken) =>
            inner.SetPauseLastDayAsync(owner, goal, pause, lastDay, cancellationToken);

        public Task<ExemptionStoreWrite> DeletePauseAsync(AccountId owner, GoalId goal, PauseId pause, CancellationToken cancellationToken) =>
            inner.DeletePauseAsync(owner, goal, pause, cancellationToken);
    }
}
