using HT.Application;
using HT.Application.Accounts;
using HT.Application.Exemptions;
using HT.Application.Goals;
using HT.Application.Records;
using HT.Application.Reports;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.IntegrationTests.Accounts;
using HT.IntegrationTests.Habits;
using HT.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using static HT.IntegrationTests.Goals.GoalSamples;

namespace HT.IntegrationTests.Goals;

/// <summary>
/// Slice 8 (T101; US5-AS1 to AS4, FR-022, FR-023, SC-006) over SQL Server: the cases of the unit
/// <c>GoalChangeAccessTests</c> and <c>AttemptAccessTests</c> through the real stores. User A has three goals on one
/// catalog habit, each "15 min daily" from Monday 2 November 2026 in UTC, created on Sunday 1 November at 10:00Z:
/// <list type="bullet">
///   <item>"active": 25 minutes and a note recorded for Monday 2 (on Tuesday 3 at 10:00Z), Tuesday 3 excused with a reason
///   and a pause from Friday 6 to Sunday 8 (both on Wednesday 4 at 10:00Z);</item>
///   <item>"ended": ended on Wednesday 4 at 10:00Z with "Abandoned" and an ending note;</item>
///   <item>"restarted": ended likewise, then attempt 2 from Thursday 5.</item>
/// </list>
/// User B has a goal of their own, ended with "Achieved". The calls are made on Friday 6 November at 10:00Z. Every row of
/// A's goals (goal, versions, attempts with their endings, records, notes, excusals, pauses) and A's goal revision are
/// read by plain SQL before and after the calls of B, an administrator and an anonymous visitor, and must be the same.
/// The test composes its own provider with its own clock and never touches the fixture's. Shared database: own prefixes
/// ("it-gcp-", "It-Gcp"), checks by inclusion and exclusion.
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class GoalChangePrivacyTests(DatabaseFixture database)
{
    private const string GoalNotFound = "This goal does not exist.";

    private static readonly CancellationToken None = CancellationToken.None;

    private static readonly DateOnly Mon2 = new(2026, 11, 2);
    private static readonly DateOnly Tue3 = new(2026, 11, 3);
    private static readonly DateOnly Thu5 = new(2026, 11, 5);
    private static readonly DateOnly Fri6 = new(2026, 11, 6);
    private static readonly DateOnly Sat7 = new(2026, 11, 7);
    private static readonly DateOnly Sun8 = new(2026, 11, 8);

    private static readonly DateTimeOffset Created = new(2026, 11, 1, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Tue3At10 = new(2026, 11, 3, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Wed4At10 = new(2026, 11, 4, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Fri6At10 = new(2026, 11, 6, 10, 0, 0, TimeSpan.Zero);

    private static readonly string[] GoalTables = ["GoalSettingsVersions", "GoalAttempts", "GoalRecords", "GoalNotes", "GoalExcusals", "GoalPauses"];

    private static GoalTargetFields MinutesDaily(int minutes) => new()
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = minutes, DurationUnit = DurationUnit.Minutes,
        ScheduleKind = ScheduleKind.FixedDays, Days = Enum.GetValues<DayOfWeek>().ToHashSet(),
    };

    /// <summary>The application as the integration tests build it, with its own clock.</summary>
    private ServiceProvider BuildServices(FakeTimeProvider clock)
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddHtApplication()
            .AddHtPersistence(database.ConnectionString);
        services.AddSingleton<TimeProvider>(clock);
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
    }

    private static async Task<T> InScopeAsync<TService, T>(IServiceProvider services, Func<TService, Task<T>> call)
        where TService : notnull
    {
        await using var scope = services.CreateAsyncScope();
        return await call(scope.ServiceProvider.GetRequiredService<TService>());
    }

    private static Task<T> ChangesAsync<T>(IServiceProvider services, Func<GoalChangeService, Task<T>> call) => InScopeAsync(services, call);

    private static Task<T> AttemptsAsync<T>(IServiceProvider services, Func<AttemptService, Task<T>> call) => InScopeAsync(services, call);

    private async Task<ActingUser> AdministratorAsync(IServiceProvider services, string email)
    {
        await Sql.DeleteUserAsync(database.ConnectionString, email);
        Assert.True(Email.TryParse(email, out var parsed));
        await using var scope = services.CreateAsyncScope();
        var created = await scope.ServiceProvider.GetRequiredService<IAccountStore>().CreateAsync(
            parsed, Role.Administrator, HabitAccounts.Password, TestProfiles.Utc, None);
        var account = created.Account ?? throw new InvalidOperationException("The test email is taken.");
        return ActingUser.ForAccount(account.Id, account.Role);
    }

    private static async Task<GoalId> CreateAsync(IServiceProvider services, ActingUser user, HabitId habit, string name)
    {
        var created = await InScopeAsync<GoalService, GoalCreateResult>(services, s => s.CreateAsync(
            user, new GoalInput { Habit = new GoalHabit(habit, HabitKind.Catalog), Target = MinutesDaily(15), StartDate = Mon2, Name = name }, None));
        Assert.Equal(GoalCreateOutcome.Created, created.Outcome);
        return created.GoalId!.Value;
    }

    private sealed record World(
        ActingUser A, ActingUser B, ActingUser Administrator, GoalId Active, GoalId Ended, GoalId Restarted, GoalId BsGoal, string Suffix);

    /// <summary>The users and goals of the class summary; the clock is left at <see cref="Fri6At10"/>.</summary>
    private async Task<World> ArrangeAsync(IServiceProvider services, FakeTimeProvider clock)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var a = await HabitAccounts.RegisteredUserAsync(services, database.ConnectionString, $"it-gcp-a-{suffix}@example.com");
        var b = await HabitAccounts.RegisteredUserAsync(services, database.ConnectionString, $"it-gcp-b-{suffix}@example.com");
        var administrator = await AdministratorAsync(services, $"it-gcp-admin-{suffix}@example.com");
        var habit = await CatalogHabitAsync(services, $"It-Gcp Reading {suffix}");
        var active = await CreateAsync(services, a, habit, $"It-Gcp A active {suffix}");
        var ended = await CreateAsync(services, a, habit, $"It-Gcp A ended {suffix}");
        var restarted = await CreateAsync(services, a, habit, $"It-Gcp A restarted {suffix}");
        var bsGoal = await CreateAsync(services, b, habit, $"It-Gcp B walk {suffix}");

        clock.SetUtcNow(Tue3At10);
        var recorded = await InScopeAsync<RecordingService, RecordWriteResult>(services, s => s.RecordAsync(
            a, active, Mon2, new RecordInput { Amount = 25, Note = $"It-Gcp A note {suffix}" }, None));
        Assert.Equal(RecordWriteOutcome.Recorded, recorded.Outcome);

        clock.SetUtcNow(Wed4At10);
        var excused = await InScopeAsync<ExcusalService, ExcuseResult>(services, s => s.ExcuseAsync(a, active, Tue3, $"It-Gcp A reason {suffix}", None));
        Assert.Equal(ExcuseOutcome.Excused, excused.Outcome);
        var paused = await InScopeAsync<PauseService, PauseResult>(services, s => s.PauseAsync(a, active, Fri6, Sun8, $"It-Gcp A pause {suffix}", None));
        Assert.Equal(PauseOutcome.Paused, paused.Outcome);
        foreach (var goal in new[] { ended, restarted })
        {
            var result = await ChangesAsync(services, s => s.EndAsync(a, goal, Outcome.Abandoned, $"It-Gcp A ending note {suffix}", None));
            Assert.Equal(GoalEndOutcome.Ended, result.Outcome);
        }

        var bEnded = await ChangesAsync(services, s => s.EndAsync(b, bsGoal, Outcome.Achieved, $"It-Gcp B ending note {suffix}", None));
        Assert.Equal(GoalEndOutcome.Ended, bEnded.Outcome);
        var started = await ChangesAsync(services, s => s.StartAttemptAsync(a, restarted, new NewAttemptInput { Target = MinutesDaily(15), StartDate = Thu5 }, None));
        Assert.Equal(AttemptStartOutcome.Started, started.Outcome);

        clock.SetUtcNow(Fri6At10);
        return new World(a, b, administrator, active, ended, restarted, bsGoal, suffix);
    }

    /// <summary>Every row of A's goals and A's goal revision, one text per row, in a stable order.</summary>
    private async Task<IReadOnlyList<string>> RowsOfAAsync(World world)
    {
        var rows = new List<string>
        {
            $"revision {(await AccountStateAsync(database.ConnectionString, world.A.AccountId!.Value)).Revision}",
        };
        foreach (var goal in new[] { world.Active, world.Ended, world.Restarted })
        {
            var goalRows = await Sql.QueryAsync(database.ConnectionString, "SELECT * FROM [Goals] WHERE [Id] = @goal", ("@goal", goal.Value));
            rows.AddRange(goalRows.Select(r => "Goals|" + string.Join("|", r.Values)));
            foreach (var table in GoalTables)
            {
                var tableRows = await Sql.QueryAsync(database.ConnectionString, $"SELECT * FROM [{table}] WHERE [GoalId] = @goal", ("@goal", goal.Value));
                rows.AddRange(tableRows.Select(r => table + "|" + string.Join("|", r.Values)).Order(StringComparer.Ordinal));
            }
        }

        return rows;
    }

    /// <summary>
    /// Every use case by <paramref name="user"/> on A's goals, with inputs A's own call would accept: a settings change and
    /// an ending of each goal that is not ended, the starting values and a new attempt from Saturday 7 of the ended goal,
    /// every goal's attempts and each attempt by number; then the goal history. Each entry is the use case, the outcome's
    /// name, its text and whether a view came back.
    /// </summary>
    private static async Task<IReadOnlyList<(string UseCase, string Outcome, string? Message, bool HasView)>> EveryUseCaseAsync(
        IServiceProvider services, ActingUser user, World world)
    {
        var calls = new List<(string, string, string?, bool)>();
        foreach (var goal in new[] { world.Active, world.Restarted })
        {
            var changed = await ChangesAsync(services, s => s.ChangeSettingsAsync(user, goal, new GoalSettingsInput { Target = MinutesDaily(20) }, None));
            calls.Add((nameof(GoalChangeService.ChangeSettingsAsync), changed.Outcome.ToString(), changed.Message, changed.AppliesFrom is not null || changed.Field is not null));
            var ended = await ChangesAsync(services, s => s.EndAsync(user, goal, Outcome.Achieved, "It-Gcp other note", None));
            calls.Add((nameof(GoalChangeService.EndAsync), ended.Outcome.ToString(), ended.Message, ended.EndingDate is not null));
        }

        var values = await ChangesAsync(services, s => s.GetNewAttemptValuesAsync(user, world.Ended, None));
        calls.Add((nameof(GoalChangeService.GetNewAttemptValuesAsync), values.Outcome.ToString(), values.Message, values.Values is not null));
        var started = await ChangesAsync(services, s => s.StartAttemptAsync(user, world.Ended, new NewAttemptInput { Target = MinutesDaily(15), StartDate = Sat7 }, None));
        calls.Add((nameof(GoalChangeService.StartAttemptAsync), started.Outcome.ToString(), started.Message, started.AttemptNumber is not null));
        foreach (var (goal, numbers) in new[] { (world.Active, new[] { 1 }), (world.Ended, new[] { 1 }), (world.Restarted, new[] { 1, 2 }) })
        {
            var attempts = await AttemptsAsync(services, s => s.GetAttemptsAsync(user, goal, None));
            calls.Add((nameof(AttemptService.GetAttemptsAsync), attempts.Outcome.ToString(), attempts.Message, attempts.Attempts.Count > 0));
            foreach (var number in numbers)
            {
                var attempt = await AttemptsAsync(services, s => s.GetAttemptAsync(user, goal, number, None));
                calls.Add((nameof(AttemptService.GetAttemptAsync), attempt.Outcome.ToString(), attempt.Message, attempt.Details is not null));
            }
        }

        var history = await AttemptsAsync(services, s => s.GetGoalHistoryAsync(user, None));
        calls.Add((nameof(AttemptService.GetGoalHistoryAsync), history.Outcome.ToString(), history.Message, history.Entries.Count > 0));
        return calls;
    }

    [Fact]
    public async Task US5_B_an_administrator_and_an_anonymous_visitor_change_and_read_nothing_of_As_goals()
    {
        var clock = new FakeTimeProvider(Created);
        await using var services = BuildServices(clock);
        var world = await ArrangeAsync(services, clock);
        var before = await RowsOfAAsync(world);
        Assert.Contains(before, row => row.StartsWith("GoalExcusals|", StringComparison.Ordinal));
        Assert.Contains(before, row => row.StartsWith("GoalPauses|", StringComparison.Ordinal));
        Assert.Contains(before, row => row.StartsWith("GoalNotes|", StringComparison.Ordinal));
        Assert.Contains(before, row => row.StartsWith("GoalAttempts|", StringComparison.Ordinal) && row.Contains("It-Gcp A ending note", StringComparison.Ordinal));

        var bCalls = await EveryUseCaseAsync(services, world.B, world);
        var administratorCalls = await EveryUseCaseAsync(services, world.Administrator, world);
        var anonymousCalls = await EveryUseCaseAsync(services, ActingUser.Anonymous, world);

        // B: every goal use case gives "This goal does not exist."; B's own goal history is found (checked below).
        Assert.Equal(14, bCalls.Count);
        Assert.All(bCalls.SkipLast(1), call => Assert.Equal(("NotFound", (string?)GoalNotFound, false), (call.Outcome, call.Message, call.HasView)));
        Assert.All(administratorCalls, call => Assert.Equal(("NotForAdministrators", (string?)"Goals are for registered users.", false), (call.Outcome, call.Message, call.HasView)));
        Assert.All(anonymousCalls, call => Assert.Equal(("NotSignedIn", (string?)"You are not signed in.", false), (call.Outcome, call.Message, call.HasView)));
        Assert.Equal(before, await RowsOfAAsync(world));
    }

    [Fact]
    public async Task US5_AS1_GetAttemptAsync_with_As_goal_and_a_valid_number_gives_This_goal_does_not_exist_to_B_and_the_attempt_to_A()
    {
        var clock = new FakeTimeProvider(Created);
        await using var services = BuildServices(clock);
        var world = await ArrangeAsync(services, clock);

        foreach (var number in new[] { 1, 2 })
        {
            var bs = await AttemptsAsync(services, s => s.GetAttemptAsync(world.B, world.Restarted, number, None));
            var owners = await AttemptsAsync(services, s => s.GetAttemptAsync(world.A, world.Restarted, number, None));

            Assert.Equal((AttemptReadOutcome.NotFound, GoalNotFound), (bs.Outcome, bs.Message));
            Assert.Null(bs.Details);
            Assert.Equal(AttemptReadOutcome.Found, owners.Outcome);
            Assert.Equal(number, owners.Details!.Attempt.Number);
        }
    }

    [Fact]
    public async Task US5_AS4_Bs_goal_history_leaves_out_As_attempts()
    {
        var clock = new FakeTimeProvider(Created);
        await using var services = BuildServices(clock);
        var world = await ArrangeAsync(services, clock);
        var asGoals = new[] { world.Active, world.Ended, world.Restarted };

        var bs = await AttemptsAsync(services, s => s.GetGoalHistoryAsync(world.B, None));
        var owners = await AttemptsAsync(services, s => s.GetGoalHistoryAsync(world.A, None));

        Assert.Equal(GoalHistoryListOutcome.Found, bs.Outcome);
        var entry = Assert.Single(bs.Entries);
        Assert.Equal((world.BsGoal, Outcome.Achieved, (string?)$"It-Gcp B ending note {world.Suffix}"), (entry.GoalId, entry.Outcome, entry.EndingNote));
        Assert.DoesNotContain(bs.Entries, e => asGoals.Contains(e.GoalId) || (e.EndingNote?.Contains("It-Gcp A", StringComparison.Ordinal) ?? false));
        Assert.Equal(
            new[] { (world.Ended, 1), (world.Restarted, 1) }.ToHashSet(),
            owners.Entries.Select(e => (e.GoalId, e.AttemptNumber)).ToHashSet());
        Assert.DoesNotContain(owners.Entries, e => e.GoalId == world.BsGoal);
    }
}
