using HT.Application;
using HT.Application.Goals;
using HT.Application.Habits;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.IntegrationTests.Habits;
using HT.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using static HT.IntegrationTests.Goals.GoalSamples;

namespace HT.IntegrationTests.Goals;

/// <summary>
/// Goal creation through <see cref="GoalService"/> on SQL Server (US1, FR-008, FR-034, HP-011; research R-08): the
/// stored goal read back through the port, the race against an unpublish forced with two contexts, and no protected
/// value in any log entry or exception text. Shared database: own prefixes ("it-gc-", "It-Gc"), checks by inclusion.
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class GoalCreationTests(DatabaseFixture database)
{
    private const string GoalName = "It-Gc Zq private morning pages";
    private const string Unit = "zq-private-units";
    private const int Amount = 987;
    private const string CustomHabitName = "It-Gc Zq private habit";

    /// <summary>Today in UTC at the fixture's clock, 09:00 UTC on Wednesday 11 November 2026.</summary>
    private static readonly DateOnly Today = new(2026, 11, 11);

    private static string Suffix() => Guid.NewGuid().ToString("N")[..8];

    private static GoalInput Input(HabitId habit, HabitKind kind, GoalTargetFields? target = null, DateOnly? start = null, string? name = null) => new()
    {
        Habit = new GoalHabit(habit, kind),
        Target = target ?? new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Count, Amount = Amount, CountUnit = Unit,
            ScheduleKind = ScheduleKind.Times, Times = 3, Period = SchedulePeriod.Week,
        },
        StartDate = start ?? Today,
        Name = name,
    };

    private static async Task<T> GoalsAsync<T>(IServiceProvider services, Func<GoalService, Task<T>> call)
    {
        await using var scope = services.CreateAsyncScope();
        return await call(scope.ServiceProvider.GetRequiredService<GoalService>());
    }

    /// <summary>
    /// The application as the integration tests build it, with the fixture's clock, every log entry captured, and,
    /// when <paramref name="unpublishAfterLookup"/> is set, a catalog store that unpublishes that habit in a second
    /// context right after the use case has found it Published.
    /// </summary>
    private ServiceProvider BuildServices(LogCapture capture, HabitId? unpublishAfterLookup = null)
    {
        var services = new ServiceCollection()
            .AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(LogLevel.Trace);
                logging.AddProvider(capture);
            })
            .AddHtApplication()
            .AddHtPersistence(database.ConnectionString);
        services.AddSingleton<TimeProvider>(database.Clock);
        if (unpublishAfterLookup is { } habit)
        {
            var real = services.Last(d => d.ServiceType == typeof(ICatalogHabitStore));
            services.Remove(real);
            var race = new UnpublishRace(habit);
            services.AddScoped<ICatalogHabitStore>(provider => new UnpublishingCatalogHabitStore(
                (ICatalogHabitStore)ActivatorUtilities.CreateInstance(provider, real.ImplementationType!),
                provider.GetRequiredService<IServiceScopeFactory>(),
                race));
        }

        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
    }

    [Fact]
    public async Task A_goal_created_on_a_Published_catalog_habit_reads_back_through_the_store()
    {
        var suffix = Suffix();
        var user = await HabitAccounts.RegisteredUserAsync(database.Services, database.ConnectionString, $"it-gc-read-{suffix}@example.com");
        var owner = user.AccountId!.Value;
        var habit = await CatalogHabitAsync(database.Services, $"It-Gc Reading {suffix}");

        var result = await GoalsAsync(database.Services, s => s.CreateAsync(user, Input(habit, HabitKind.Catalog, name: "  It-Gc Morning pages  "), CancellationToken.None));

        Assert.Equal(GoalCreateOutcome.Created, result.Outcome);
        await using var scope = database.CreateScope();
        var stored = await scope.ServiceProvider.GetRequiredService<IGoalStore>().FindAsync(owner, result.GoalId!.Value, CancellationToken.None);
        Assert.NotNull(stored);
        var goal = stored.Goal;
        Assert.Equal(owner, goal.Owner);
        Assert.Equal(new GoalHabit(habit, HabitKind.Catalog), goal.Habit);
        Assert.Equal(Today, goal.StartDate);
        Assert.Equal("It-Gc Morning pages", goal.OwnName!.Value.Value);
        Assert.Equal(new GoalAttempt(1, Today), goal.Attempt);
        var version = Assert.Single(goal.Versions);
        Assert.Equal(Today, version.AppliesFrom);
        Assert.Equal("UTC", version.Settings.TimeZone.Id);
        Assert.Equal(Target(Input(habit, HabitKind.Catalog).Target), version.Settings.Target);
        Assert.Equal($"It-Gc Reading {suffix}", stored.Habit.Name);
        Assert.True(stored.Habit.IsInCatalog);
    }

    [Fact]
    public async Task A_goal_created_against_an_unpublish_is_not_written_and_the_habit_is_not_in_the_catalog()
    {
        var suffix = Suffix();
        var habit = await CatalogHabitAsync(database.Services, $"It-Gc Race {suffix}");
        var capture = new LogCapture();
        await using var services = BuildServices(capture, habit);
        var user = await HabitAccounts.RegisteredUserAsync(services, database.ConnectionString, $"it-gc-race-{suffix}@example.com");

        var result = await GoalsAsync(services, s => s.CreateAsync(user, Input(habit, HabitKind.Catalog, name: GoalName), CancellationToken.None));

        Assert.Equal(GoalCreateOutcome.NotInCatalog, result.Outcome);
        Assert.Equal("This habit is not in the catalog.", result.Message);
        Assert.Equal(GoalField.Habit, result.Field);
        Assert.Equal(0, await GoalCountAsync(database.ConnectionString, user.AccountId!.Value));
        Assert.Equal(0, (await AccountStateAsync(database.ConnectionString, user.AccountId!.Value)).Revision);
        await using var scope = services.CreateAsyncScope();
        Assert.Equal(CatalogHabitState.Unpublished, (await scope.ServiceProvider.GetRequiredService<ICatalogHabitStore>().FindAsync(habit, CancellationToken.None))!.State);
        capture.AssertContainsNone(GoalName, Unit, $" {Amount}");
    }

    [Fact]
    public async Task HP_011_a_creation_and_every_kind_of_refusal_log_no_goal_name_unit_amount_or_habit_name()
    {
        var suffix = Suffix();
        var capture = new LogCapture();
        await using var services = BuildServices(capture);
        var user = await HabitAccounts.RegisteredUserAsync(services, database.ConnectionString, $"it-gc-log-{suffix}@example.com");
        var other = await HabitAccounts.RegisteredUserAsync(services, database.ConnectionString, $"it-gc-log-other-{suffix}@example.com");
        var owner = user.AccountId!.Value;
        var catalog = await CatalogHabitAsync(services, $"It-Gc Log {suffix}");
        var draft = await CatalogHabitAsync(services, $"It-Gc Log draft {suffix}", CatalogHabitState.Draft);
        var custom = await CustomHabitAsync(services, owner, $"{CustomHabitName} {suffix}");
        var othersCustom = await CustomHabitAsync(services, other.AccountId!.Value, $"{CustomHabitName} other {suffix}");
        var administrator = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.Administrator);
        var longUnit = new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Count, Amount = Amount, CountUnit = Unit + "-way-too-long",
            ScheduleKind = ScheduleKind.Times, Times = 3, Period = SchedulePeriod.Week,
        };
        var reduceTwice = new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Reduce, Measurement = Measurement.Count, Amount = Amount, CountUnit = Unit,
            ScheduleKind = ScheduleKind.Times, Times = 2, Period = SchedulePeriod.Day,
        };
        var cases = new (ActingUser User, GoalInput Input, GoalCreateOutcome Outcome)[]
        {
            (user, Input(catalog, HabitKind.Catalog, name: GoalName), GoalCreateOutcome.Created),
            (user, Input(custom, HabitKind.Custom, name: GoalName), GoalCreateOutcome.Created),
            (ActingUser.Anonymous, Input(catalog, HabitKind.Catalog, name: GoalName), GoalCreateOutcome.NotSignedIn),
            (administrator, Input(catalog, HabitKind.Catalog, name: GoalName), GoalCreateOutcome.NotForAdministrators),
            (user, Input(catalog, HabitKind.Catalog, name: GoalName) with { Habit = null }, GoalCreateOutcome.HabitMissing),
            (user, Input(draft, HabitKind.Catalog, name: GoalName), GoalCreateOutcome.NotInCatalog),
            (user, Input(othersCustom, HabitKind.Custom, name: GoalName), GoalCreateOutcome.CustomHabitNotFound),
            (user, Input(catalog, HabitKind.Catalog, longUnit, name: GoalName), GoalCreateOutcome.CountUnitTooLong),
            (user, Input(custom, HabitKind.Custom, reduceTwice, name: GoalName), GoalCreateOutcome.ReduceAmountNeedsOneTime),
            (user, Input(catalog, HabitKind.Catalog, start: Today.AddDays(-1), name: GoalName), GoalCreateOutcome.StartDateInPast),
            (user, Input(catalog, HabitKind.Catalog, name: GoalName + new string('x', 40)), GoalCreateOutcome.NameTooLong),
        };

        foreach (var (actingUser, input, outcome) in cases)
        {
            var result = await GoalsAsync(services, s => s.CreateAsync(actingUser, input, CancellationToken.None));
            Assert.Equal(outcome, result.Outcome);
            if (result.Message is { } message)
            {
                Assert.DoesNotContain(GoalName, message, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(Unit, message, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(CustomHabitName, message, StringComparison.OrdinalIgnoreCase);
            }
        }

        Assert.Equal(2, await GoalCountAsync(database.ConnectionString, owner));
        Assert.NotEmpty(capture.Entries);
        capture.AssertContainsNone(GoalName, Unit, $" {Amount}", $"{Amount} ", CustomHabitName);
    }

    /// <summary>Which habit to unpublish, and whether it was; shared by every scope of one provider.</summary>
    private sealed class UnpublishRace(HabitId habit)
    {
        private int _done;

        public HabitId Habit { get; } = habit;

        /// <summary>True the first time only.</summary>
        public bool TryStart() => Interlocked.Exchange(ref _done, 1) == 0;
    }

    /// <summary>
    /// The real catalog store, which, right after the use case found the race's habit Published, unpublishes it through
    /// another scope's store and context, as an administrator's commit between the lookup and the goal's transaction would.
    /// </summary>
    private sealed class UnpublishingCatalogHabitStore(ICatalogHabitStore inner, IServiceScopeFactory scopes, UnpublishRace race) : ICatalogHabitStore
    {
        public async Task<CatalogHabit?> FindPublishedAsync(HabitId id, CancellationToken cancellationToken)
        {
            var found = await inner.FindPublishedAsync(id, cancellationToken);
            if (found is not null && id == race.Habit && race.TryStart())
            {
                await using var scope = scopes.CreateAsyncScope();
                var other = scope.ServiceProvider.GetRequiredService<ICatalogHabitStore>();
                Assert.True(await other.TrySetStateAsync(id, CatalogHabitState.Published, CatalogHabitState.Unpublished, cancellationToken));
            }

            return found;
        }

        public Task<IReadOnlyList<CatalogHabit>> ListPublishedAsync(CancellationToken cancellationToken) => inner.ListPublishedAsync(cancellationToken);

        public Task<IReadOnlyList<CatalogHabit>> ListAllAsync(CancellationToken cancellationToken) => inner.ListAllAsync(cancellationToken);

        public Task<CatalogHabit?> FindAsync(HabitId id, CancellationToken cancellationToken) => inner.FindAsync(id, cancellationToken);

        public Task<CreateCatalogHabitOutcome> CreateAsync(
            HabitDefinition definition, SuggestedStartingGoals suggestedStartingGoals, CancellationToken cancellationToken) =>
            inner.CreateAsync(definition, suggestedStartingGoals, cancellationToken);

        public Task<UpdateCatalogHabitOutcome> UpdateAsync(
            HabitId id, HabitDefinition definition, SuggestedStartingGoals suggestedStartingGoals, CancellationToken cancellationToken) =>
            inner.UpdateAsync(id, definition, suggestedStartingGoals, cancellationToken);

        public Task<bool> TrySetStateAsync(HabitId id, CatalogHabitState expected, CatalogHabitState next, CancellationToken cancellationToken) =>
            inner.TrySetStateAsync(id, expected, next, cancellationToken);

        public Task<bool> NameKeyExistsAsync(string key, HabitId? except, CancellationToken cancellationToken) =>
            inner.NameKeyExistsAsync(key, except, cancellationToken);
    }
}
