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
/// Slice 8 (T065; FR-001 to FR-003, FR-021, research R-05): a settings change through
/// <see cref="GoalChangeService.ChangeSettingsAsync"/> on SQL Server, and its races with a time zone change in both
/// orders, each forced with a second scope that commits between the first write's read and its write. Whichever commits
/// first, the goal ends with one later version that has the new target and the latest zone. Every provider has its own
/// clock, from Monday 16 November 2026 at 12:00 in Kyiv (UTC+2), so the fixture's shared clock is untouched; at
/// Wednesday 18 November, 12:00 in Kyiv, a change applies from Thursday 19 November in both zones. Shared database: own
/// prefixes ("it-gs-", "It-Gs"), checks by inclusion.
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class GoalSettingsChangeTests(DatabaseFixture database)
{
    private const string KyivId = "Europe/Kyiv";
    private const string NewYorkId = "America/New_York";

    private static readonly DateTimeOffset KyivNoonMonday = new(2026, 11, 16, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset KyivNoonWednesday = new(2026, 11, 18, 10, 0, 0, TimeSpan.Zero);

    private static readonly DateOnly Mon16 = new(2026, 11, 16);
    private static readonly DateOnly Thu19 = new(2026, 11, 19);

    private static CancellationToken None => CancellationToken.None;

    private static GoalTargetFields MinutesDaily(int minutes) => new()
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = minutes, DurationUnit = DurationUnit.Minutes,
        ScheduleKind = ScheduleKind.FixedDays, Days = Enum.GetValues<DayOfWeek>().ToHashSet(),
    };

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

    private static Task<SettingsChangeResult> ChangeSettingsAsync(IServiceProvider services, ActingUser user, GoalId goal, GoalSettingsInput input) =>
        InScopeAsync<SettingsChangeResult, GoalChangeService>(services, s => s.ChangeSettingsAsync(user, goal, input, None));

    private static Task<TimeZoneChangeResult> ChangeZoneAsync(IServiceProvider services, ActingUser user, string zone) =>
        InScopeAsync<TimeZoneChangeResult, ProfileService>(services, p => p.ChangeTimeZoneAsync(user, zone, None));

    /// <summary>A registered user stored in Kyiv with "Reading, 15 min daily" from Monday 16 November.</summary>
    private async Task<(ActingUser User, AccountId Owner, GoalId Goal)> ArrangeAsync(IServiceProvider services, string name)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var user = await HabitAccounts.RegisteredUserAsync(services, database.ConnectionString, $"it-gs-{name}-{suffix}@example.com");
        var owner = user.AccountId!.Value;
        await SetZoneAsync(database.ConnectionString, owner, KyivId);
        var habit = await CatalogHabitAsync(services, $"It-Gs {name} {suffix}");
        var created = await InScopeAsync<GoalCreateResult, GoalService>(services, g => g.CreateAsync(
            user, new GoalInput { Habit = new GoalHabit(habit, HabitKind.Catalog), Target = MinutesDaily(15), StartDate = Mon16 }, None));
        Assert.Equal(GoalCreateOutcome.Created, created.Outcome);
        return (user, owner, created.GoalId!.Value);
    }

    /// <summary>The goal as the real store reads it: its own name and each version's (date, zone, amount).</summary>
    private static async Task<(string? Name, (DateOnly From, string Zone, int? Amount)[] Versions)> StoredAsync(IServiceProvider services, AccountId owner, GoalId goal)
    {
        await using var scope = services.CreateAsyncScope();
        var stored = await scope.ServiceProvider.GetRequiredService<IGoalStore>().FindAsync(owner, goal, None);
        Assert.NotNull(stored);
        return (stored.Goal.OwnName?.Value,
            stored.Goal.Versions.Select(v => (v.AppliesFrom, v.Settings.TimeZone.Id, v.Settings.Target.Amount)).ToArray());
    }

    [Fact]
    public async Task A_change_through_the_real_store_adds_one_later_version_and_the_name_and_bumps_the_revision()
    {
        await using var services = BuildServices(out var clock);
        var (user, owner, goal) = await ArrangeAsync(services, "change");
        clock.SetUtcNow(KyivNoonWednesday);
        var (_, revision) = await AccountStateAsync(database.ConnectionString, owner);

        var changed = await ChangeSettingsAsync(services, user, goal, new GoalSettingsInput { Target = MinutesDaily(20), Name = " Morning pages " });

        Assert.Equal((SettingsChangeOutcome.Changed, (DateOnly?)Thu19), (changed.Outcome, changed.AppliesFrom));
        var afterChange = await StoredAsync(services, owner, goal);
        Assert.Equal("Morning pages", afterChange.Name);
        Assert.Equal([(Mon16, KyivId, 15), (Thu19, KyivId, 20)], afterChange.Versions);
        Assert.Equal(((string?)KyivId, revision + 1), await AccountStateAsync(database.ConnectionString, owner));

        // The same settings and name again: nothing is written. Clearing the name writes the name only.
        var unchanged = await ChangeSettingsAsync(services, user, goal, new GoalSettingsInput { Target = MinutesDaily(20), Name = "Morning pages" });
        var cleared = await ChangeSettingsAsync(services, user, goal, new GoalSettingsInput { Target = MinutesDaily(20), Name = "" });

        Assert.Equal(SettingsChangeOutcome.Unchanged, unchanged.Outcome);
        Assert.Equal((SettingsChangeOutcome.Changed, (DateOnly?)null), (cleared.Outcome, cleared.AppliesFrom));
        var afterClear = await StoredAsync(services, owner, goal);
        Assert.Null(afterClear.Name);
        Assert.Equal([(Mon16, KyivId, 15), (Thu19, KyivId, 20)], afterClear.Versions);
        Assert.Equal(((string?)KyivId, revision + 2), await AccountStateAsync(database.ConnectionString, owner));
    }

    [Fact]
    public async Task Another_accounts_goal_does_not_exist_and_is_not_changed()
    {
        await using var services = BuildServices(out var clock);
        var (_, owner, goal) = await ArrangeAsync(services, "owner");
        var (other, _, _) = await ArrangeAsync(services, "other");
        clock.SetUtcNow(KyivNoonWednesday);

        var result = await ChangeSettingsAsync(services, other, goal, new GoalSettingsInput { Target = MinutesDaily(20) });

        Assert.Equal((SettingsChangeOutcome.NotFound, "This goal does not exist."), (result.Outcome, result.Message));
        var untouched = await StoredAsync(services, owner, goal);
        Assert.Null(untouched.Name);
        Assert.Equal([(Mon16, KyivId, 15)], untouched.Versions);
    }

    [Fact]
    public async Task A_settings_change_then_a_time_zone_change_leave_one_later_version_with_the_new_target_and_the_latest_zone()
    {
        await using var services = BuildServices(out var clock);
        var (user, owner, goal) = await ArrangeAsync(services, "then-zone");
        clock.SetUtcNow(KyivNoonWednesday);

        Assert.Equal(SettingsChangeOutcome.Changed, (await ChangeSettingsAsync(services, user, goal, new GoalSettingsInput { Target = MinutesDaily(20) })).Outcome);
        Assert.Equal(TimeZoneChangeOutcome.Changed, (await ChangeZoneAsync(services, user, NewYorkId)).Outcome);

        Assert.Equal([(Mon16, KyivId, 15), (Thu19, NewYorkId, 20)], (await StoredAsync(services, owner, goal)).Versions);
    }

    [Fact]
    public async Task A_time_zone_change_then_a_settings_change_leave_one_later_version_with_the_new_target_and_the_latest_zone()
    {
        await using var services = BuildServices(out var clock);
        var (user, owner, goal) = await ArrangeAsync(services, "zone-then");
        clock.SetUtcNow(KyivNoonWednesday);

        Assert.Equal(TimeZoneChangeOutcome.Changed, (await ChangeZoneAsync(services, user, NewYorkId)).Outcome);
        Assert.Equal(SettingsChangeOutcome.Changed, (await ChangeSettingsAsync(services, user, goal, new GoalSettingsInput { Target = MinutesDaily(20) })).Outcome);

        Assert.Equal([(Mon16, KyivId, 15), (Thu19, NewYorkId, 20)], (await StoredAsync(services, owner, goal)).Versions);
    }

    [Fact]
    public async Task A_settings_change_that_loses_to_a_time_zone_change_reads_again_and_merges_into_the_one_later_version()
    {
        var race = new Race();
        await using var services = BuildServices(out var clock, race);
        var (user, owner, goal) = await ArrangeAsync(services, "race-settings");
        clock.SetUtcNow(KyivNoonWednesday);

        // The zone change commits in another scope after the settings change read the goal and before its write.
        race.BeforeSettingsWrite = provider => provider.GetRequiredService<ProfileService>().ChangeTimeZoneAsync(user, NewYorkId, None);
        var result = await ChangeSettingsAsync(services, user, goal, new GoalSettingsInput { Target = MinutesDaily(20) });

        Assert.Equal((SettingsChangeOutcome.Changed, (DateOnly?)Thu19), (result.Outcome, result.AppliesFrom));
        Assert.Equal(2, race.SettingsWrites);
        Assert.Equal([(Mon16, KyivId, 15), (Thu19, NewYorkId, 20)], (await StoredAsync(services, owner, goal)).Versions);
    }

    [Fact]
    public async Task A_time_zone_change_that_loses_to_a_settings_change_reads_again_and_keeps_the_new_target()
    {
        var race = new Race();
        await using var services = BuildServices(out var clock, race);
        var (user, owner, goal) = await ArrangeAsync(services, "race-zone");
        clock.SetUtcNow(KyivNoonWednesday);

        // The settings change commits in another scope after the zone change read the goals and before its write.
        race.BeforeTimeZoneWrite = provider =>
            provider.GetRequiredService<GoalChangeService>().ChangeSettingsAsync(user, goal, new GoalSettingsInput { Target = MinutesDaily(20) }, None);
        var result = await ChangeZoneAsync(services, user, NewYorkId);

        Assert.Equal(TimeZoneChangeOutcome.Changed, result.Outcome);
        Assert.Equal(2, race.TimeZoneWrites);
        Assert.Equal([(Mon16, KyivId, 15), (Thu19, NewYorkId, 20)], (await StoredAsync(services, owner, goal)).Versions);
    }

    /// <summary>One-shot actions run in another scope before the next settings write or time zone write, and the write counts.</summary>
    private sealed class Race
    {
        private Func<IServiceProvider, Task>? _beforeSettingsWrite;
        private Func<IServiceProvider, Task>? _beforeTimeZoneWrite;
        private int _settingsWrites;
        private int _timeZoneWrites;

        public Func<IServiceProvider, Task>? BeforeSettingsWrite
        {
            set => Volatile.Write(ref _beforeSettingsWrite, value);
        }

        public Func<IServiceProvider, Task>? BeforeTimeZoneWrite
        {
            set => Volatile.Write(ref _beforeTimeZoneWrite, value);
        }

        public int SettingsWrites => Volatile.Read(ref _settingsWrites);

        public int TimeZoneWrites => Volatile.Read(ref _timeZoneWrites);

        public Func<IServiceProvider, Task>? TakeBeforeSettingsWrite()
        {
            Interlocked.Increment(ref _settingsWrites);
            return Interlocked.Exchange(ref _beforeSettingsWrite, null);
        }

        public Func<IServiceProvider, Task>? TakeBeforeTimeZoneWrite()
        {
            Interlocked.Increment(ref _timeZoneWrites);
            return Interlocked.Exchange(ref _beforeTimeZoneWrite, null);
        }
    }

    /// <summary>The real goal store, with the race's actions run in another scope before the two conditional writes.</summary>
    private sealed class RacingGoalStore(IGoalStore inner, IServiceScopeFactory scopes, Race race) : IGoalStore
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
            if (race.TakeBeforeTimeZoneWrite() is { } action)
            {
                await using var scope = scopes.CreateAsyncScope();
                await action(scope.ServiceProvider);
            }

            return await inner.TryChangeTimeZoneAsync(owner, expectedStoredZone, expectedRevision, next, writes, cancellationToken);
        }

        public Task<OwnerGoal?> ReadOwnerGoalAsync(AccountId owner, GoalId goal, CancellationToken cancellationToken) =>
            inner.ReadOwnerGoalAsync(owner, goal, cancellationToken);

        public async Task<GoalChangeWriteOutcome> TryChangeSettingsAsync(
            AccountId owner, string? expectedStoredZone, int expectedRevision, GoalSettingsWrite write, CancellationToken cancellationToken)
        {
            if (race.TakeBeforeSettingsWrite() is { } action)
            {
                await using var scope = scopes.CreateAsyncScope();
                await action(scope.ServiceProvider);
            }

            return await inner.TryChangeSettingsAsync(owner, expectedStoredZone, expectedRevision, write, cancellationToken);
        }

        public Task<GoalChangeWriteOutcome> TryEndAsync(
            AccountId owner, string? expectedStoredZone, int expectedRevision, GoalEndingWrite write, CancellationToken cancellationToken) =>
            inner.TryEndAsync(owner, expectedStoredZone, expectedRevision, write, cancellationToken);

        public Task<GoalChangeWriteOutcome> TryStartAttemptAsync(
            AccountId owner, string? expectedStoredZone, int expectedRevision, GoalAttemptWrite write, CancellationToken cancellationToken) =>
            inner.TryStartAttemptAsync(owner, expectedStoredZone, expectedRevision, write, cancellationToken);

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
