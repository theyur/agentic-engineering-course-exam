using HT.Application;
using HT.Application.Goals;
using HT.Application.Habits;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HT.IntegrationTests.Habits;

/// <summary>
/// HP-018 and HP-015: parallel creates and renames with one name give one success and one "name taken", and no
/// log entry or exception text carries the custom habit's name, its key or its description. FR-035 (research R-08): a goal
/// created against a delete, forced with two contexts in both orders and run in parallel, is never left on a deleted
/// habit.
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class CustomHabitRaceTests(DatabaseFixture database)
{
    private const string Description = "It-Cr private description of the morning routine";

    private static HabitInput Input(string name) =>
        new() { Name = name, Description = Description, Category = Category.Body, Intent = Intent.Build, Icon = "droplet" };

    private static async Task<T> RunAsync<T>(IServiceProvider services, Func<CustomHabitService, Task<T>> call)
    {
        await using var scope = services.CreateAsyncScope();
        return await call(scope.ServiceProvider.GetRequiredService<CustomHabitService>());
    }

    [Fact]
    public async Task Two_parallel_creates_with_one_name_give_one_habit_and_log_no_value()
    {
        var capture = new LogCapture();
        await using var services = capture.BuildServices(database.ConnectionString);
        var user = await HabitAccounts.RegisteredUserAsync(services, database.ConnectionString, "it-cr-create@example.com");

        var results = await Task.WhenAll(
            Task.Run(() => RunAsync(services, s => s.CreateAsync(user, Input("It-Cr Cold race"), CancellationToken.None))),
            Task.Run(() => RunAsync(services, s => s.CreateAsync(user, Input("IT-CR COLD RACE"), CancellationToken.None))));

        Assert.Single(results, r => r.Outcome == CustomHabitCreateOutcome.Created);
        Assert.Single(results, r => r.Outcome == CustomHabitCreateOutcome.NameTaken);
        Assert.Equal(1, await Sql.ScalarAsync(database.ConnectionString,
            "SELECT COUNT(*) FROM [CustomHabits] WHERE [OwnerId] = @owner", ("@owner", user.AccountId!.Value.Value)));
        capture.AssertContainsNone("It-Cr Cold race", HabitName.KeyOf("It-Cr Cold race"), Description);
    }

    [Fact]
    public async Task Two_parallel_renames_to_one_name_give_one_rename_and_log_no_value()
    {
        var capture = new LogCapture();
        await using var services = capture.BuildServices(database.ConnectionString);
        var user = await HabitAccounts.RegisteredUserAsync(services, database.ConnectionString, "it-cr-rename@example.com");
        var first = (await RunAsync(services, s => s.CreateAsync(user, Input("It-Cr Rename one"), CancellationToken.None))).HabitId!.Value;
        var second = (await RunAsync(services, s => s.CreateAsync(user, Input("It-Cr Rename two"), CancellationToken.None))).HabitId!.Value;

        var results = await Task.WhenAll(
            Task.Run(() => RunAsync(services, s => s.EditAsync(user, first, Input("It-Cr Renamed race"), CancellationToken.None))),
            Task.Run(() => RunAsync(services, s => s.EditAsync(user, second, Input("it-cr renamed RACE"), CancellationToken.None))));

        Assert.Single(results, r => r.Outcome == CustomHabitEditOutcome.Saved);
        Assert.Single(results, r => r.Outcome == CustomHabitEditOutcome.NameTaken);
        capture.AssertContainsNone("It-Cr Renamed race", HabitName.KeyOf("It-Cr Renamed race"), "It-Cr Rename one", "It-Cr Rename two", Description);
    }

    /// <summary>Today at the fixture's clock (09:00 UTC on Wednesday 11 November 2026), in the test accounts' UTC.</summary>
    private static readonly DateOnly Today = new(2026, 11, 11);

    private static GoalInput DailyGoal(HabitId habit) => new()
    {
        Habit = new GoalHabit(habit, HabitKind.Custom),
        Target = new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo, ScheduleKind = ScheduleKind.FixedDays,
            Days = Enum.GetValues<DayOfWeek>().ToHashSet(),
        },
        StartDate = Today,
    };

    private static async Task<GoalCreateResult> CreateGoalAsync(IServiceProvider services, ActingUser user, HabitId habit)
    {
        await using var scope = services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<GoalService>().CreateAsync(user, DailyGoal(habit), CancellationToken.None);
    }

    private static async Task<CustomHabitDeleteResult> DeleteHabitAsync(IServiceProvider services, ActingUser user, HabitId habit)
    {
        await using var scope = services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<CustomHabitService>().DeleteAsync(user, habit, CancellationToken.None);
    }

    /// <summary>
    /// The application with the fixture's clock, every log entry captured, and a custom habit store that runs
    /// <paramref name="race"/>'s other side in a second scope and context, once, at the step the race names.
    /// </summary>
    private ServiceProvider BuildServices(LogCapture capture, DeleteRace race)
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
        var real = services.Last(d => d.ServiceType == typeof(ICustomHabitStore));
        services.Remove(real);
        services.AddSingleton(race);
        services.AddScoped<ICustomHabitStore>(provider => new RacingCustomHabitStore(
            (ICustomHabitStore)ActivatorUtilities.CreateInstance(provider, real.ImplementationType!),
            provider.GetRequiredService<IServiceProvider>(),
            race));
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
    }

    private Task<object?> HabitRowsAsync(HabitId habit) =>
        Sql.ScalarAsync(database.ConnectionString, "SELECT COUNT(*) FROM [CustomHabits] WHERE [Id] = @id", ("@id", habit.Value));

    private Task<object?> GoalRowsAsync(HabitId habit) =>
        Sql.ScalarAsync(database.ConnectionString, "SELECT COUNT(*) FROM [Goals] WHERE [CustomHabitId] = @id", ("@id", habit.Value));

    [Fact]
    public async Task FR_035_a_delete_that_commits_after_the_goals_lookup_wins_and_the_goal_finds_no_habit()
    {
        const string name = "It-Cr Zq delete wins";
        var capture = new LogCapture();
        var race = new DeleteRace(DeleteRace.Side.DeleteAfterGoalLookup);
        await using var services = BuildServices(capture, race);
        var user = await HabitAccounts.RegisteredUserAsync(services, database.ConnectionString, "it-cr-delete-wins@example.com");
        race.User = user;
        var habit = (await RunAsync(services, s => s.CreateAsync(user, Input(name), CancellationToken.None))).HabitId!.Value;
        race.Habit = habit;

        var result = await CreateGoalAsync(services, user, habit);

        Assert.Equal(DeleteRace.Side.DeleteAfterGoalLookup, race.Ran);
        Assert.Equal(CustomHabitDeleteOutcome.Deleted, race.Deleted!.Outcome);
        Assert.Equal(GoalCreateOutcome.CustomHabitNotFound, result.Outcome);
        Assert.Equal("This custom habit does not exist.", result.Message);
        Assert.Equal(0, await HabitRowsAsync(habit));
        Assert.Equal(0, await GoalRowsAsync(habit));
        capture.AssertContainsNone(name, HabitName.KeyOf(name), Description);
    }

    [Fact]
    public async Task FR_035_a_goal_that_commits_before_the_delete_wins_and_the_delete_gives_used_by_goals()
    {
        const string name = "It-Cr Zq goal wins";
        var capture = new LogCapture();
        var race = new DeleteRace(DeleteRace.Side.GoalBeforeDelete);
        await using var services = BuildServices(capture, race);
        var user = await HabitAccounts.RegisteredUserAsync(services, database.ConnectionString, "it-cr-goal-wins@example.com");
        race.User = user;
        var habit = (await RunAsync(services, s => s.CreateAsync(user, Input(name), CancellationToken.None))).HabitId!.Value;
        race.Habit = habit;

        var result = await DeleteHabitAsync(services, user, habit);

        Assert.Equal(DeleteRace.Side.GoalBeforeDelete, race.Ran);
        Assert.Equal(GoalCreateOutcome.Created, race.Created!.Outcome);
        Assert.Equal(CustomHabitDeleteOutcome.UsedByGoals, result.Outcome);
        Assert.Equal("Goals use this custom habit, so it cannot be deleted.", result.Message);
        Assert.Equal(1, await HabitRowsAsync(habit));
        Assert.Equal(1, await GoalRowsAsync(habit));
        capture.AssertContainsNone(name, HabitName.KeyOf(name), Description);
    }

    [Fact]
    public async Task FR_035_parallel_goal_creations_and_deletes_never_leave_a_goal_on_a_deleted_habit()
    {
        await using var services = new LogCapture().BuildServices(database.ConnectionString);
        var user = await HabitAccounts.RegisteredUserAsync(services, database.ConnectionString, "it-cr-parallel@example.com");
        for (var i = 0; i < 8; i++)
        {
            var habit = (await RunAsync(services, s => s.CreateAsync(user, Input($"It-Cr Parallel {i}"), CancellationToken.None))).HabitId!.Value;
            var created = Task.Run(() => CreateGoalWithClockAsync(services, user, habit));
            var deleted = Task.Run(() => DeleteHabitAsync(services, user, habit));
            await Task.WhenAll(created, deleted);

            var pair = ((await created).Outcome, (await deleted).Outcome);
            Assert.True(
                pair == (GoalCreateOutcome.Created, CustomHabitDeleteOutcome.UsedByGoals)
                || pair == (GoalCreateOutcome.CustomHabitNotFound, CustomHabitDeleteOutcome.Deleted),
                $"Unexpected outcomes: {pair}.");
            Assert.Equal(pair.Item1 == GoalCreateOutcome.Created ? 1 : 0, await GoalRowsAsync(habit));
            Assert.Equal(pair.Item1 == GoalCreateOutcome.Created ? 1 : 0, await HabitRowsAsync(habit));
        }
    }

    /// <summary>A goal creation whose start date is today by the system clock, which <see cref="LogCapture.BuildServices"/> uses.</summary>
    private static async Task<GoalCreateResult> CreateGoalWithClockAsync(IServiceProvider services, ActingUser user, HabitId habit)
    {
        await using var scope = services.CreateAsyncScope();
        var today = DateOnly.FromDateTime(scope.ServiceProvider.GetRequiredService<TimeProvider>().GetUtcNow().UtcDateTime);
        return await scope.ServiceProvider.GetRequiredService<GoalService>().CreateAsync(user, DailyGoal(habit) with { StartDate = today }, CancellationToken.None);
    }

    /// <summary>Which side of the race runs in the second scope, for which user and habit, and what it gave.</summary>
    private sealed class DeleteRace(DeleteRace.Side side)
    {
        private int _started;

        public enum Side
        {
            None,
            DeleteAfterGoalLookup,
            GoalBeforeDelete,
        }

        public ActingUser? User { get; set; }

        public HabitId? Habit { get; set; }

        public Side Ran { get; private set; }

        public CustomHabitDeleteResult? Deleted { get; set; }

        public GoalCreateResult? Created { get; set; }

        /// <summary>True the first time only, and only for <paramref name="step"/> on the race's habit.</summary>
        public bool TryStart(Side step, HabitId habit)
        {
            if (step != side || habit != Habit || Interlocked.Exchange(ref _started, 1) == 1)
            {
                return false;
            }

            Ran = step;
            return true;
        }
    }

    /// <summary>
    /// The real custom habit store, which runs the race's other side through another scope's use case and context:
    /// right after a lookup has found the habit (the goal use case's FR-008 lookup), it deletes the habit; right before a
    /// delete, it creates a goal on it.
    /// </summary>
    private sealed class RacingCustomHabitStore(ICustomHabitStore inner, IServiceProvider services, DeleteRace race) : ICustomHabitStore
    {
        public async Task<CustomHabit?> FindAsync(AccountId owner, HabitId id, CancellationToken cancellationToken)
        {
            var found = await inner.FindAsync(owner, id, cancellationToken);
            if (found is not null && race.TryStart(DeleteRace.Side.DeleteAfterGoalLookup, id))
            {
                race.Deleted = await DeleteHabitAsync(services, race.User!, id);
            }

            return found;
        }

        public async Task<DeleteCustomHabitOutcome> DeleteAsync(AccountId owner, HabitId id, CancellationToken cancellationToken)
        {
            if (race.TryStart(DeleteRace.Side.GoalBeforeDelete, id))
            {
                race.Created = await CreateGoalAsync(services, race.User!, id);
            }

            return await inner.DeleteAsync(owner, id, cancellationToken);
        }

        public Task<IReadOnlyList<CustomHabit>> ListAsync(AccountId owner, CancellationToken cancellationToken) => inner.ListAsync(owner, cancellationToken);

        public Task<CreateCustomHabitOutcome> CreateAsync(AccountId owner, HabitDefinition definition, CancellationToken cancellationToken) =>
            inner.CreateAsync(owner, definition, cancellationToken);

        public Task<UpdateCustomHabitOutcome> UpdateAsync(AccountId owner, HabitId id, HabitDefinition definition, CancellationToken cancellationToken) =>
            inner.UpdateAsync(owner, id, definition, cancellationToken);

        public Task<bool> NameKeyExistsAsync(AccountId owner, string key, HabitId? except, CancellationToken cancellationToken) =>
            inner.NameKeyExistsAsync(owner, key, except, cancellationToken);
    }
}
