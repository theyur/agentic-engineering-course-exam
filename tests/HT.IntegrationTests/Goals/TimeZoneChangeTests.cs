using HT.Application;
using HT.Application.Accounts;
using HT.Application.Goals;
using HT.Domain.Accounts;
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
/// The time-zone change on SQL Server (US10, FR-026; research R-08): the account's zone, each goal's versions and the
/// goal revision written together; the read that returns the revision with the goals; the conditional write; the
/// ownership rule of a write; and the races of a change against a goal creation and against another change, each forced
/// with two contexts. Every provider here has its own clock, from Monday 16 November 2026, so the shared fixture's
/// clock is untouched. Kyiv is UTC+2, New York UTC−5 and Tokyo UTC+9 in November and December 2026. Shared database:
/// own prefixes ("it-tz-", "It-Tz"), checks by inclusion.
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class TimeZoneChangeTests(DatabaseFixture database)
{
    private const string KyivId = "Europe/Kyiv";
    private const string NewYorkId = "America/New_York";
    private const string TokyoId = "Asia/Tokyo";

    /// <summary>12:00 in Kyiv on Monday 16 November 2026, when the goals that start that day are created.</summary>
    private static readonly DateTimeOffset KyivNoonMonday = new(2026, 11, 16, 10, 0, 0, TimeSpan.Zero);

    /// <summary>12:00 in Kyiv on Wednesday 18 November 2026 (US10-AS4).</summary>
    private static readonly DateTimeOffset KyivNoonWednesday = new(2026, 11, 18, 10, 0, 0, TimeSpan.Zero);

    /// <summary>12:00 in New York on Thursday 19 November 2026 (US10-AS10).</summary>
    private static readonly DateTimeOffset NewYorkNoonThursday = new(2026, 11, 19, 17, 0, 0, TimeSpan.Zero);

    private static readonly DateOnly Mon16 = new(2026, 11, 16);
    private static readonly DateOnly Wed18 = new(2026, 11, 18);
    private static readonly DateOnly Thu19 = new(2026, 11, 19);
    private static readonly DateOnly Fri20 = new(2026, 11, 20);
    private static readonly DateOnly Mon23 = new(2026, 11, 23);
    private static readonly DateOnly Tue1Dec = new(2026, 12, 1);

    private static readonly GoalTargetFields MonWedFri = new()
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo, ScheduleKind = ScheduleKind.FixedDays,
        Days = new HashSet<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
    };

    private static readonly GoalTargetFields PerDay = new()
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo, ScheduleKind = ScheduleKind.Times, Times = 1, Period = SchedulePeriod.Day,
    };

    private static readonly GoalTargetFields PerWeek = new()
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo, ScheduleKind = ScheduleKind.Times, Times = 3, Period = SchedulePeriod.Week,
    };

    private static string Suffix() => Guid.NewGuid().ToString("N")[..8];

    private static CancellationToken None => CancellationToken.None;

    /// <summary>
    /// The application as the integration tests build it, with its own clock at <see cref="KyivNoonMonday"/>; with
    /// <paramref name="race"/>, every scope's goal store runs the race's actions once, in another scope.
    /// </summary>
    private ServiceProvider BuildServices(out FakeTimeProvider clock, Race? race = null)
    {
        clock = new FakeTimeProvider(KyivNoonMonday);
        var services = new ServiceCollection()
            .AddLogging()
            .AddHtApplication()
            .AddHtPersistence(database.ConnectionString);
        services.AddSingleton<TimeProvider>(clock);
        if (race is not null)
        {
            var real = services.Last(d => d.ServiceType == typeof(IGoalStore));
            services.Remove(real);
            services.AddScoped<IGoalStore>(provider => new RacingGoalStore(
                (IGoalStore)ActivatorUtilities.CreateInstance(provider, real.ImplementationType!),
                provider.GetRequiredService<IServiceScopeFactory>(),
                race));
        }

        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
    }

    private static async Task<T> InScopeAsync<T, TService>(IServiceProvider services, Func<TService, Task<T>> call)
        where TService : notnull
    {
        await using var scope = services.CreateAsyncScope();
        return await call(scope.ServiceProvider.GetRequiredService<TService>());
    }

    private static Task<TimeZoneChangeResult> ChangeAsync(IServiceProvider services, ActingUser user, string zone) =>
        InScopeAsync<TimeZoneChangeResult, ProfileService>(services, p => p.ChangeTimeZoneAsync(user, zone, None));

    private static async Task<GoalId> CreateAsync(IServiceProvider services, ActingUser user, HabitId habit, GoalTargetFields target, DateOnly start)
    {
        var result = await InScopeAsync<GoalCreateResult, GoalService>(services, g => g.CreateAsync(
            user, new GoalInput { Habit = new GoalHabit(habit, HabitKind.Catalog), Target = target, StartDate = start }, None));
        Assert.Equal(GoalCreateOutcome.Created, result.Outcome);
        return result.GoalId!.Value;
    }

    /// <summary>A registered user stored in Kyiv, and a Published catalog habit for its goals.</summary>
    private async Task<(ActingUser User, AccountId Owner, HabitId Habit)> ArrangeKyivUserAsync(IServiceProvider services, string name)
    {
        var suffix = Suffix();
        var user = await HabitAccounts.RegisteredUserAsync(services, database.ConnectionString, $"it-tz-{name}-{suffix}@example.com");
        var owner = user.AccountId!.Value;
        await SetZoneAsync(database.ConnectionString, owner, KyivId);
        var habit = await CatalogHabitAsync(services, $"It-Tz {name} {suffix}");
        return (user, owner, habit);
    }

    /// <summary>The goal's version rows, (applies-from, zone) in date order, read by SQL.</summary>
    private async Task<(DateOnly From, string Zone)[]> VersionsAsync(GoalId goal)
    {
        var rows = await Sql.QueryAsync(database.ConnectionString,
            "SELECT [AppliesFrom], [TimeZone] FROM [GoalSettingsVersions] WHERE [GoalId] = @id ORDER BY [AppliesFrom]", ("@id", goal.Value));
        return rows.Select(r => (DateOnly.FromDateTime((DateTime)r["AppliesFrom"]!), (string)r["TimeZone"]!)).ToArray();
    }

    [Fact]
    public async Task A_change_through_ProfileService_writes_the_zone_each_goals_versions_and_the_revision_plus_one()
    {
        await using var services = BuildServices(out var clock);
        var (user, owner, habit) = await ArrangeKyivUserAsync(services, "change");
        var monWedFri = await CreateAsync(services, user, habit, MonWedFri, Mon16);
        var perDay = await CreateAsync(services, user, habit, PerDay, Mon16);
        var perWeek = await CreateAsync(services, user, habit, PerWeek, Mon16);
        var december = await CreateAsync(services, user, habit, PerDay, Tue1Dec);
        Assert.Equal(((string?)KyivId, 4), await AccountStateAsync(database.ConnectionString, owner));
        clock.SetUtcNow(KyivNoonWednesday);

        var first = await ChangeAsync(services, user, NewYorkId);

        Assert.Equal(TimeZoneChangeOutcome.Changed, first.Outcome);
        Assert.Equal(((string?)NewYorkId, 5), await AccountStateAsync(database.ConnectionString, owner));
        Assert.Equal([(Mon16, KyivId), (Thu19, NewYorkId)], await VersionsAsync(monWedFri));
        Assert.Equal([(Mon16, KyivId), (Thu19, NewYorkId)], await VersionsAsync(perDay));
        Assert.Equal([(Mon16, KyivId), (Mon23, NewYorkId)], await VersionsAsync(perWeek));
        Assert.Equal([(Tue1Dec, NewYorkId)], await VersionsAsync(december));

        // US10-AS10: the pending per-week row is replaced on the same date, the others get a version or a new first zone.
        clock.SetUtcNow(NewYorkNoonThursday);
        var second = await ChangeAsync(services, user, TokyoId);

        Assert.Equal(TimeZoneChangeOutcome.Changed, second.Outcome);
        Assert.Equal(((string?)TokyoId, 6), await AccountStateAsync(database.ConnectionString, owner));
        Assert.Equal([(Mon16, KyivId), (Thu19, NewYorkId), (Fri20, TokyoId)], await VersionsAsync(monWedFri));
        Assert.Equal([(Mon16, KyivId), (Thu19, NewYorkId), (Fri20, TokyoId)], await VersionsAsync(perDay));
        Assert.Equal([(Mon16, KyivId), (Mon23, TokyoId)], await VersionsAsync(perWeek));
        Assert.Equal([(Tue1Dec, TokyoId)], await VersionsAsync(december));

        // Every version keeps the target, and the goal use cases read the result.
        await using var scope = services.CreateAsyncScope();
        foreach (var (goal, fields) in new[] { (monWedFri, MonWedFri), (perDay, PerDay), (perWeek, PerWeek), (december, PerDay) })
        {
            var stored = (await scope.ServiceProvider.GetRequiredService<IGoalStore>().FindAsync(owner, goal, None))!;
            Assert.All(stored.Goal.Versions, v => Assert.Equal(Target(fields), v.Settings.Target));
        }

        // Friday 20 November began in Tokyo at 2026-11-19T15:00:00Z, before the change at 17:00 UTC: after a large move
        // east the new version is in effect at once (FR-027), so it is the time zone in effect and nothing comes later.
        var details = (await scope.ServiceProvider.GetRequiredService<GoalService>().GetAsync(user, monWedFri, None)).Goal!;
        Assert.Equal((TokyoId, Fri20, (string?)null, (DateOnly?)null),
            (details.TimeZone, details.TimeZoneAppliesFrom, details.LaterTimeZone, details.LaterTimeZoneAppliesFrom));
    }

    [Fact]
    public async Task ReadOwnerGoalsAsync_returns_the_stored_zone_and_the_revision_it_read_with_the_owners_goals()
    {
        await using var services = BuildServices(out _);
        var (user, owner, habit) = await ArrangeKyivUserAsync(services, "read");
        var (other, _, _) = await ArrangeKyivUserAsync(services, "read-other");
        var one = await CreateAsync(services, user, habit, PerDay, Mon16);
        var two = await CreateAsync(services, user, habit, PerWeek, Mon16);
        await CreateAsync(services, other, habit, PerDay, Mon16);

        await using var scope = services.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IGoalStore>();
        var read = await store.ReadOwnerGoalsAsync(owner, None);

        Assert.NotNull(read);
        Assert.Equal(((string?)KyivId, 2), (read.StoredTimeZone, read.GoalsRevision));
        Assert.Equal([one, two], read.Goals.Select(g => g.Goal.Id));
        Assert.All(read.Goals, g => Assert.Equal(owner, g.Goal.Owner));
        Assert.Null(await store.ReadOwnerGoalsAsync(new AccountId(Guid.NewGuid()), None));
    }

    [Fact]
    public async Task TryChangeTimeZoneAsync_with_a_stale_revision_or_a_stale_zone_writes_nothing_and_returns_false()
    {
        await using var services = BuildServices(out _);
        var (user, owner, habit) = await ArrangeKyivUserAsync(services, "stale");
        var goal = await CreateAsync(services, user, habit, PerDay, Mon16);
        await using var scope = services.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IGoalStore>();
        var read = (await store.ReadOwnerGoalsAsync(owner, None))!;
        IReadOnlyList<GoalVersionWrite> writes = [new GoalVersionWrite(goal, GoalVersionChange.Add(Thu19, Zone(NewYorkId)))];

        var staleRevision = await store.TryChangeTimeZoneAsync(owner, KyivId, read.GoalsRevision - 1, Zone(NewYorkId), writes, None);
        var staleZone = await store.TryChangeTimeZoneAsync(owner, "UTC", read.GoalsRevision, Zone(NewYorkId), writes, None);
        var noZone = await store.TryChangeTimeZoneAsync(owner, null, read.GoalsRevision, Zone(NewYorkId), writes, None);

        Assert.Equal((false, false, false), (staleRevision, staleZone, noZone));
        Assert.Equal(((string?)KyivId, 1), await AccountStateAsync(database.ConnectionString, owner));
        Assert.Equal([(Mon16, KyivId)], await VersionsAsync(goal));

        // The same write with what was read goes through.
        Assert.True(await store.TryChangeTimeZoneAsync(owner, KyivId, read.GoalsRevision, Zone(NewYorkId), writes, None));
        Assert.Equal(((string?)NewYorkId, 2), await AccountStateAsync(database.ConnectionString, owner));
        Assert.Equal([(Mon16, KyivId), (Thu19, NewYorkId)], await VersionsAsync(goal));
    }

    [Fact]
    public async Task A_write_that_names_another_accounts_goal_or_an_unknown_one_writes_nothing_and_returns_false()
    {
        await using var services = BuildServices(out _);
        var (user, owner, habit) = await ArrangeKyivUserAsync(services, "own");
        var (other, otherOwner, _) = await ArrangeKyivUserAsync(services, "own-other");
        var mine = await CreateAsync(services, user, habit, PerDay, Mon16);
        var theirs = await CreateAsync(services, other, habit, PerDay, Mon16);
        var change = GoalVersionChange.Add(Thu19, Zone(NewYorkId));
        await using var scope = services.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IGoalStore>();

        var withTheirs = await store.TryChangeTimeZoneAsync(
            owner, KyivId, 1, Zone(NewYorkId), [new GoalVersionWrite(mine, change), new GoalVersionWrite(theirs, change)], None);
        var withUnknown = await store.TryChangeTimeZoneAsync(
            owner, KyivId, 1, Zone(NewYorkId), [new GoalVersionWrite(mine, change), new GoalVersionWrite(new GoalId(Guid.NewGuid()), change)], None);
        var twice = await store.TryChangeTimeZoneAsync(
            owner, KyivId, 1, Zone(NewYorkId), [new GoalVersionWrite(mine, change), new GoalVersionWrite(mine, change)], None);

        Assert.Equal((false, false, false), (withTheirs, withUnknown, twice));
        Assert.Equal([(Mon16, KyivId)], await VersionsAsync(mine));
        Assert.Equal([(Mon16, KyivId)], await VersionsAsync(theirs));
        Assert.Equal(((string?)KyivId, 1), await AccountStateAsync(database.ConnectionString, owner));
        Assert.Equal(((string?)KyivId, 1), await AccountStateAsync(database.ConnectionString, otherOwner));
    }

    [Fact]
    public async Task A_goal_whose_creation_loses_to_a_change_gets_the_new_zone_from_its_start()
    {
        var race = new Race();
        await using var services = BuildServices(out var clock, race);
        var (user, owner, habit) = await ArrangeKyivUserAsync(services, "race-create");
        clock.SetUtcNow(KyivNoonWednesday);

        // The change commits in another context after the creation read the account (Kyiv) and before its transaction.
        race.BeforeCreate = provider => provider.GetRequiredService<ProfileService>().ChangeTimeZoneAsync(user, NewYorkId, None);
        var goal = await CreateAsync(services, user, habit, PerDay, Wed18);

        Assert.Equal([(Wed18, NewYorkId)], await VersionsAsync(goal));
        Assert.Equal(((string?)NewYorkId, 2), await AccountStateAsync(database.ConnectionString, owner));
    }

    [Fact]
    public async Task A_goal_created_against_a_change_gets_the_old_zone_and_a_later_version_in_the_new_one()
    {
        var race = new Race();
        await using var services = BuildServices(out var clock, race);
        var (user, owner, habit) = await ArrangeKyivUserAsync(services, "race-change");
        clock.SetUtcNow(KyivNoonWednesday);
        GoalId? created = null;

        // The goal commits in another context after the change read the account's goals and before its transaction.
        race.BeforeTimeZoneWrite = async provider =>
        {
            var result = await provider.GetRequiredService<GoalService>().CreateAsync(
                user, new GoalInput { Habit = new GoalHabit(habit, HabitKind.Catalog), Target = PerDay, StartDate = Wed18 }, None);
            created = result.GoalId;
        };
        var change = await ChangeAsync(services, user, NewYorkId);

        Assert.Equal(TimeZoneChangeOutcome.Changed, change.Outcome);
        Assert.Equal(2, race.TimeZoneWrites);
        Assert.Equal([(Wed18, KyivId), (Thu19, NewYorkId)], await VersionsAsync(created!.Value));
        Assert.Equal(((string?)NewYorkId, 2), await AccountStateAsync(database.ConnectionString, owner));
    }

    [Fact]
    public async Task Two_concurrent_changes_one_retries_and_the_end_state_is_the_later_change_applied_to_the_earlier()
    {
        var race = new Race();
        await using var services = BuildServices(out var clock, race);
        var (user, owner, habit) = await ArrangeKyivUserAsync(services, "race-two");
        var perDay = await CreateAsync(services, user, habit, PerDay, Mon16);
        var perWeek = await CreateAsync(services, user, habit, PerWeek, Mon16);
        clock.SetUtcNow(KyivNoonWednesday);

        // The change to Tokyo commits in another context after the change to New York read the account's goals.
        TimeZoneChangeResult? earlier = null;
        race.BeforeTimeZoneWrite = async provider =>
            earlier = await provider.GetRequiredService<ProfileService>().ChangeTimeZoneAsync(user, TokyoId, None);
        var later = await ChangeAsync(services, user, NewYorkId);

        Assert.Equal(TimeZoneChangeOutcome.Changed, earlier!.Outcome);
        Assert.Equal(TimeZoneChangeOutcome.Changed, later.Outcome);
        Assert.Equal(3, race.TimeZoneWrites);

        // Tokyo first added (Thu 19, Tokyo) and (Mon 23, Tokyo); New York, applied to that, replaced both pending versions.
        Assert.Equal([(Mon16, KyivId), (Thu19, NewYorkId)], await VersionsAsync(perDay));
        Assert.Equal([(Mon16, KyivId), (Mon23, NewYorkId)], await VersionsAsync(perWeek));
        Assert.Equal(((string?)NewYorkId, 4), await AccountStateAsync(database.ConnectionString, owner));
    }

    /// <summary>The actions to run once, in another scope, before the first creation and the first time-zone write; shared by every scope.</summary>
    private sealed class Race
    {
        private int _timeZoneWrites;

        public Func<IServiceProvider, Task>? BeforeCreate { get; set; }

        public Func<IServiceProvider, Task>? BeforeTimeZoneWrite { get; set; }

        /// <summary>Calls of <see cref="IGoalStore.TryChangeTimeZoneAsync"/> in every scope.</summary>
        public int TimeZoneWrites => Volatile.Read(ref _timeZoneWrites);

        public void CountTimeZoneWrite() => Interlocked.Increment(ref _timeZoneWrites);

        public Func<IServiceProvider, Task>? TakeBeforeCreate()
        {
            var action = BeforeCreate;
            BeforeCreate = null;
            return action;
        }

        public Func<IServiceProvider, Task>? TakeBeforeTimeZoneWrite()
        {
            var action = BeforeTimeZoneWrite;
            BeforeTimeZoneWrite = null;
            return action;
        }
    }

    /// <summary>
    /// The real goal store, which runs the race's actions in another scope, with its own context, right before the
    /// first creation's or the first time-zone write's transaction: as another session's commit at that point would.
    /// </summary>
    private sealed class RacingGoalStore(IGoalStore inner, IServiceScopeFactory scopes, Race race) : IGoalStore
    {
        public async Task<GoalWriteOutcome> CreateAsync(
            AccountId owner, NewGoal goal, NamedTimeZone expectedZone, NamedTimeZone defaultZone, CancellationToken cancellationToken)
        {
            if (race.TakeBeforeCreate() is { } action)
            {
                await using var scope = scopes.CreateAsyncScope();
                await action(scope.ServiceProvider);
            }

            return await inner.CreateAsync(owner, goal, expectedZone, defaultZone, cancellationToken);
        }

        public Task<OwnerGoals?> ReadOwnerGoalsAsync(AccountId owner, CancellationToken cancellationToken) =>
            inner.ReadOwnerGoalsAsync(owner, cancellationToken);

        public async Task<bool> TryChangeTimeZoneAsync(
            AccountId owner, string? expectedStoredZone, int expectedRevision, NamedTimeZone next,
            IReadOnlyList<GoalVersionWrite> writes, CancellationToken cancellationToken)
        {
            if (race.TakeBeforeTimeZoneWrite() is { } action)
            {
                await using var scope = scopes.CreateAsyncScope();
                await action(scope.ServiceProvider);
            }

            race.CountTimeZoneWrite();
            return await inner.TryChangeTimeZoneAsync(owner, expectedStoredZone, expectedRevision, next, writes, cancellationToken);
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
    }
}
