using HT.Application;
using HT.Application.Accounts;
using HT.Application.Exemptions;
using HT.Application.Goals;
using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Exemptions;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Time;
using HT.IntegrationTests.Habits;
using HT.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using static HT.IntegrationTests.Goals.GoalSamples;

namespace HT.IntegrationTests.Goals;

/// <summary>
/// Slice 8 (T075; FR-007 to FR-010, FR-021, research R-04, R-05): an ending through
/// <see cref="GoalChangeService.EndAsync"/> on SQL Server, with its pause and excusal cuts read back through the real
/// stores; and its races with a pause and with a time zone change in both orders, each forced by a store wrapper that
/// runs the other write in a second scope between the first one's read and its write. Every provider has its own clock,
/// so the fixture's shared clock is untouched. "Meditation, 10 min daily" (Build, 10 minutes, all seven days) is created
/// on Sunday 1 November 2026 at 10:00Z with the start date Monday 2 November, in UTC unless a test moves the account to
/// Kyiv (UTC+2); it is ended on Monday 16 November at 10:00Z (12:00 in Kyiv), so its ending date is Monday 16 and its
/// last date Sunday 15 November. Shared database: own prefixes ("it-ge-", "It-Ge"), checks per goal.
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class GoalEndingTests(DatabaseFixture database)
{
    private const string KyivId = "Europe/Kyiv";
    private const string NewYorkId = "America/New_York";
    private const string GoalEnded = "This goal has ended.";

    private static readonly DateTimeOffset Created = new(2026, 11, 1, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset EndedAt = new(2026, 11, 16, 10, 0, 0, TimeSpan.Zero);

    private static readonly DateOnly Mon2 = new(2026, 11, 2);
    private static readonly DateOnly Sat14 = new(2026, 11, 14);
    private static readonly DateOnly Sun15 = new(2026, 11, 15);
    private static readonly DateOnly Mon16 = new(2026, 11, 16);
    private static readonly DateOnly Tue17 = new(2026, 11, 17);
    private static readonly DateOnly Fri20 = new(2026, 11, 20);
    private static readonly DateOnly Mon23 = new(2026, 11, 23);
    private static readonly DateOnly Wed25 = new(2026, 11, 25);

    private static CancellationToken None => CancellationToken.None;

    private static GoalTargetFields TenMinutesDaily => new()
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = 10, DurationUnit = DurationUnit.Minutes,
        ScheduleKind = ScheduleKind.FixedDays, Days = Enum.GetValues<DayOfWeek>().ToHashSet(),
    };

    private ServiceProvider BuildServices(out FakeTimeProvider clock, Hooks? hooks = null)
    {
        clock = new FakeTimeProvider(Created);
        var services = new ServiceCollection()
            .AddLogging()
            .AddHtApplication()
            .AddHtPersistence(database.ConnectionString);
        services.AddSingleton<TimeProvider>(clock);
        if (hooks is not null)
        {
            var real = services.Last(d => d.ServiceType == typeof(IGoalStore));
            services.Remove(real);
            services.AddScoped<IGoalStore>(provider => new HookedGoalStore(
                (IGoalStore)ActivatorUtilities.CreateInstance(provider, real.ImplementationType!),
                provider.GetRequiredService<IServiceScopeFactory>(),
                hooks));
        }

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

    private static Task<PauseResult> PauseAsync(IServiceProvider services, ActingUser user, GoalId goal, DateOnly first, DateOnly last) =>
        InScopeAsync<PauseResult, PauseService>(services, s => s.PauseAsync(user, goal, first, last, null, None));

    private static Task<TimeZoneChangeResult> ChangeZoneAsync(IServiceProvider services, ActingUser user, string zone) =>
        InScopeAsync<TimeZoneChangeResult, ProfileService>(services, p => p.ChangeTimeZoneAsync(user, zone, None));

    /// <summary>A registered user, stored in <paramref name="zone"/> (null: none, so UTC), with the goal of the class summary.</summary>
    private async Task<(ActingUser User, AccountId Owner, GoalId Goal)> ArrangeAsync(IServiceProvider services, string name, string? zone = null)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var user = await HabitAccounts.RegisteredUserAsync(services, database.ConnectionString, $"it-ge-{name}-{suffix}@example.com");
        var owner = user.AccountId!.Value;
        await SetZoneAsync(database.ConnectionString, owner, zone);
        var habit = await CatalogHabitAsync(services, $"It-Ge {name} {suffix}");
        var created = await InScopeAsync<GoalCreateResult, GoalService>(services, g => g.CreateAsync(
            user, new GoalInput { Habit = new GoalHabit(habit, HabitKind.Catalog), Target = TenMinutesDaily, StartDate = Mon2 }, None));
        Assert.Equal(GoalCreateOutcome.Created, created.Outcome);
        return (user, owner, created.GoalId!.Value);
    }

    private static async Task<Goal> StoredAsync(IServiceProvider services, AccountId owner, GoalId goal)
    {
        await using var scope = services.CreateAsyncScope();
        var stored = await scope.ServiceProvider.GetRequiredService<IGoalStore>().FindAsync(owner, goal, None);
        Assert.NotNull(stored);
        return stored.Goal;
    }

    private static async Task<(DateOnly First, DateOnly Last)[]> PausesAsync(IServiceProvider services, AccountId owner, GoalId goal) =>
        (await ExemptionsAsync(services, owner, goal)).Pauses.Select(p => (p.FirstDay, p.LastDay)).OrderBy(p => p.FirstDay).ToArray();

    private static async Task<GoalExemptions> ExemptionsAsync(IServiceProvider services, AccountId owner, GoalId goal)
    {
        await using var scope = services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IRecordStore>().ReadExemptionsAsync(owner, goal, None);
    }

    private static (DateOnly From, string Zone)[] VersionsOf(Goal goal) =>
        goal.Versions.Select(v => (v.AppliesFrom, v.Settings.TimeZone.Id)).ToArray();

    [Fact]
    public async Task An_ending_through_the_real_stores_stores_the_ending_and_cuts_the_current_pause_and_removes_the_upcoming_one()
    {
        await using var services = BuildServices(out var clock);
        var (user, owner, goal) = await ArrangeAsync(services, "pauses");
        clock.SetUtcNow(new DateTimeOffset(2026, 11, 14, 10, 0, 0, TimeSpan.Zero));
        Assert.Equal(PauseOutcome.Paused, (await PauseAsync(services, user, goal, Sat14, Fri20)).Outcome);
        Assert.Equal(PauseOutcome.Paused, (await PauseAsync(services, user, goal, Mon23, Wed25)).Outcome);
        clock.SetUtcNow(EndedAt);
        var (_, revision) = await AccountStateAsync(database.ConnectionString, owner);

        var result = await EndAsync(services, user, goal, "  Too busy this month.  ");

        Assert.Equal((GoalEndOutcome.Ended, "goal ended", (DateOnly?)Mon16, (DateOnly?)Sun15), (result.Outcome, result.Message, result.EndingDate, result.LastDate));
        var ending = Assert.IsType<GoalEnding>((await StoredAsync(services, owner, goal)).Attempt.Ending);
        Assert.Equal((Mon16, Outcome.Abandoned, "Too busy this month."), (ending.Date, ending.Outcome, ending.Note!.Value.Value));
        Assert.Equal([(Sat14, Sun15)], await PausesAsync(services, owner, goal));
        Assert.Equal(revision + 1, (await AccountStateAsync(database.ConnectionString, owner)).Revision);
        var details = await InScopeAsync<GoalReadResult, GoalService>(services, g => g.GetAsync(user, goal, None));
        Assert.Equal((true, (DateOnly?)Mon16, (DateOnly?)Sun15, "Too busy this month."), (details.Goal!.IsEnded, details.Goal.EndingDate, details.Goal.LastDate, details.Goal.EndingNote));
    }

    [Fact]
    public async Task An_ending_through_the_real_stores_removes_the_excusal_after_the_last_date_and_keeps_the_one_before()
    {
        await using var services = BuildServices(out var clock);
        var (user, owner, goal) = await ArrangeAsync(services, "excusals");
        clock.SetUtcNow(new DateTimeOffset(2026, 11, 16, 9, 0, 0, TimeSpan.Zero));
        foreach (var date in new[] { Sun15, Mon16 })
        {
            Assert.Equal(ExcuseOutcome.Excused, (await InScopeAsync<ExcuseResult, ExcusalService>(services, s => s.ExcuseAsync(user, goal, date, null, None))).Outcome);
        }

        clock.SetUtcNow(EndedAt);

        Assert.Equal(GoalEndOutcome.Ended, (await EndAsync(services, user, goal)).Outcome);

        Assert.Equal([Sun15], (await ExemptionsAsync(services, owner, goal)).Excusals.Select(e => e.Date));
    }

    [Fact]
    public async Task A_pause_after_the_ending_gives_This_goal_has_ended_from_the_stores_re_check()
    {
        var hooks = new Hooks();
        await using var services = BuildServices(out var clock, hooks);
        var (user, owner, goal) = await ArrangeAsync(services, "end-then-pause");
        clock.SetUtcNow(EndedAt);

        // The ending commits in another scope after the pause read the goal, which was not ended yet, and before its write.
        hooks.AfterFind = provider => provider.GetRequiredService<GoalChangeService>().EndAsync(user, goal, Outcome.Abandoned, null, None);
        var result = await PauseAsync(services, user, goal, Tue17, Fri20);

        Assert.Equal((PauseOutcome.GoalEnded, GoalEnded), (result.Outcome, result.Message));
        Assert.True((await StoredAsync(services, owner, goal)).IsEnded);
        Assert.Empty(await PausesAsync(services, owner, goal));
    }

    [Fact]
    public async Task A_pause_stored_before_the_endings_write_is_cut_by_it()
    {
        var hooks = new Hooks();
        await using var services = BuildServices(out var clock, hooks);
        var (user, owner, goal) = await ArrangeAsync(services, "pause-then-end");
        clock.SetUtcNow(new DateTimeOffset(2026, 11, 15, 10, 0, 0, TimeSpan.Zero));
        Assert.Equal(PauseOutcome.Paused, (await PauseAsync(services, user, goal, Sun15, Mon16)).Outcome);
        clock.SetUtcNow(EndedAt);

        // An upcoming pause commits in another scope after the ending read the goal and before its write.
        hooks.BeforeEndWrite = async provider =>
            Assert.Equal(PauseOutcome.Paused, (await provider.GetRequiredService<PauseService>().PauseAsync(user, goal, Mon23, Wed25, null, None)).Outcome);
        var result = await EndAsync(services, user, goal);

        Assert.Equal((GoalEndOutcome.Ended, (DateOnly?)Sun15), (result.Outcome, result.LastDate));
        Assert.Equal([(Sun15, Sun15)], await PausesAsync(services, owner, goal));
    }

    [Fact]
    public async Task A_time_zone_change_stored_before_the_endings_write_leaves_no_version_after_the_last_date()
    {
        var hooks = new Hooks();
        await using var services = BuildServices(out var clock, hooks);
        var (user, owner, goal) = await ArrangeAsync(services, "zone-then-end", KyivId);
        clock.SetUtcNow(EndedAt);

        // The zone change commits in another scope after the ending read the goal and before its write.
        hooks.BeforeEndWrite = async provider =>
            Assert.Equal(TimeZoneChangeOutcome.Changed, (await provider.GetRequiredService<ProfileService>().ChangeTimeZoneAsync(user, NewYorkId, None)).Outcome);
        var result = await EndAsync(services, user, goal);

        Assert.Equal((GoalEndOutcome.Ended, (DateOnly?)Mon16, (DateOnly?)Sun15), (result.Outcome, result.EndingDate, result.LastDate));
        Assert.Equal(2, hooks.EndWrites);
        Assert.Equal([(Mon2, KyivId)], VersionsOf(await StoredAsync(services, owner, goal)));
        Assert.Equal((string?)NewYorkId, (await AccountStateAsync(database.ConnectionString, owner)).Zone);
    }

    [Fact]
    public async Task An_ending_stored_before_a_time_zone_changes_write_leaves_no_version_after_the_last_date()
    {
        var hooks = new Hooks();
        await using var services = BuildServices(out var clock, hooks);
        var (user, owner, goal) = await ArrangeAsync(services, "end-then-zone", KyivId);
        clock.SetUtcNow(EndedAt);

        // The ending commits in another scope after the zone change read the goals and before its write.
        hooks.BeforeTimeZoneWrite = async provider =>
            Assert.Equal(GoalEndOutcome.Ended, (await provider.GetRequiredService<GoalChangeService>().EndAsync(user, goal, Outcome.Abandoned, null, None)).Outcome);
        var result = await ChangeZoneAsync(services, user, NewYorkId);

        Assert.Equal(TimeZoneChangeOutcome.Changed, result.Outcome);
        Assert.Equal(2, hooks.TimeZoneWrites);
        var stored = await StoredAsync(services, owner, goal);
        Assert.Equal((Mon16, (DateOnly?)Sun15), (stored.Attempt.Ending!.Date, GoalTimeline.LastDate(stored, 1)));
        Assert.Equal([(Mon2, KyivId)], VersionsOf(stored));
        Assert.Equal((string?)NewYorkId, (await AccountStateAsync(database.ConnectionString, owner)).Zone);
    }

    [Fact]
    public async Task A_time_zone_change_then_an_ending_leave_only_the_first_version_and_a_later_pause_is_refused()
    {
        await using var services = BuildServices(out var clock);
        var (user, owner, zoneFirst) = await ArrangeAsync(services, "zone-first", KyivId);
        clock.SetUtcNow(EndedAt);

        Assert.Equal(TimeZoneChangeOutcome.Changed, (await ChangeZoneAsync(services, user, NewYorkId)).Outcome);
        Assert.Equal(2, (await StoredAsync(services, owner, zoneFirst)).Versions.Count);
        Assert.Equal(GoalEndOutcome.Ended, (await EndAsync(services, user, zoneFirst)).Outcome);

        Assert.Equal([(Mon2, KyivId)], VersionsOf(await StoredAsync(services, owner, zoneFirst)));
        Assert.Equal((PauseOutcome.GoalEnded, GoalEnded), await PauseOutcomeAsync(services, user, zoneFirst));
    }

    private static async Task<(PauseOutcome, string)> PauseOutcomeAsync(IServiceProvider services, ActingUser user, GoalId goal)
    {
        var result = await PauseAsync(services, user, goal, Tue17, Fri20);
        return (result.Outcome, result.Message);
    }

    /// <summary>One-shot actions run in another scope after the next goal read or before the next ending or time zone write, and the write counts.</summary>
    private sealed class Hooks
    {
        private Func<IServiceProvider, Task>? _afterFind;
        private Func<IServiceProvider, Task>? _beforeEndWrite;
        private Func<IServiceProvider, Task>? _beforeTimeZoneWrite;
        private int _endWrites;
        private int _timeZoneWrites;

        public Func<IServiceProvider, Task>? AfterFind
        {
            set => Volatile.Write(ref _afterFind, value);
        }

        public Func<IServiceProvider, Task>? BeforeEndWrite
        {
            set => Volatile.Write(ref _beforeEndWrite, value);
        }

        public Func<IServiceProvider, Task>? BeforeTimeZoneWrite
        {
            set => Volatile.Write(ref _beforeTimeZoneWrite, value);
        }

        public int EndWrites => Volatile.Read(ref _endWrites);

        public int TimeZoneWrites => Volatile.Read(ref _timeZoneWrites);

        public Func<IServiceProvider, Task>? TakeAfterFind() => Interlocked.Exchange(ref _afterFind, null);

        public Func<IServiceProvider, Task>? TakeBeforeEndWrite()
        {
            Interlocked.Increment(ref _endWrites);
            return Interlocked.Exchange(ref _beforeEndWrite, null);
        }

        public Func<IServiceProvider, Task>? TakeBeforeTimeZoneWrite()
        {
            Interlocked.Increment(ref _timeZoneWrites);
            return Interlocked.Exchange(ref _beforeTimeZoneWrite, null);
        }
    }

    /// <summary>The real goal store, with the hooks' actions run in another scope around its reads and conditional writes.</summary>
    private sealed class HookedGoalStore(IGoalStore inner, IServiceScopeFactory scopes, Hooks hooks) : IGoalStore
    {
        public Task<GoalWriteOutcome> CreateAsync(
            AccountId owner, NewGoal goal, NamedTimeZone expectedZone, NamedTimeZone defaultZone, CancellationToken cancellationToken) =>
            inner.CreateAsync(owner, goal, expectedZone, defaultZone, cancellationToken);

        public Task<OwnerGoals?> ReadOwnerGoalsAsync(AccountId owner, CancellationToken cancellationToken) =>
            inner.ReadOwnerGoalsAsync(owner, cancellationToken);

        public async Task<bool> TryChangeTimeZoneAsync(
            AccountId owner, string? expectedStoredZone, int expectedRevision, NamedTimeZone next,
            IReadOnlyList<GoalVersionWrite> writes, CancellationToken cancellationToken)
        {
            await RunAsync(hooks.TakeBeforeTimeZoneWrite());
            return await inner.TryChangeTimeZoneAsync(owner, expectedStoredZone, expectedRevision, next, writes, cancellationToken);
        }

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

        public Task<GoalChangeWriteOutcome> TryStartAttemptAsync(
            AccountId owner, string? expectedStoredZone, int expectedRevision, GoalAttemptWrite write, CancellationToken cancellationToken) =>
            inner.TryStartAttemptAsync(owner, expectedStoredZone, expectedRevision, write, cancellationToken);

        public Task<IReadOnlyList<StoredGoal>> ListAsync(AccountId owner, CancellationToken cancellationToken) =>
            inner.ListAsync(owner, cancellationToken);

        public async Task<StoredGoal?> FindAsync(AccountId owner, GoalId id, CancellationToken cancellationToken)
        {
            var found = await inner.FindAsync(owner, id, cancellationToken);
            await RunAsync(hooks.TakeAfterFind());
            return found;
        }

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
}
