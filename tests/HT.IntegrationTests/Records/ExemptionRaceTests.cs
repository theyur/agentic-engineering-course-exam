using HT.Application;
using HT.Application.Exemptions;
using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Records;
using HT.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using static HT.IntegrationTests.Records.RecordSamples;

namespace HT.IntegrationTests.Records;

/// <summary>
/// The race of an excusal and a record of one commitment on SQL Server (D-3, FR-009, plan "Risks" Concurrency; T046):
/// whichever write takes the goal-row lock first is stored, and the other is refused with its use case's text, never
/// both stored. The race is forced as in <see cref="RecordRaceTests"/>: a third connection holds the goal row's update
/// lock while the two use cases, each in a scope of its own, pass their checks (plain reads, which the update lock lets
/// through) and queue on that lock at their writes; the first is started and seen waiting before the second starts, so
/// each order is tried. The use cases run through the test's own provider and clock, never the fixture's shared one:
/// the sample goal "Yoga" (Monday, Wednesday and Friday) starts on Wednesday 18 November 2026 in UTC, and at
/// 2026-11-18T10:00:00Z its Wednesday 18 commitment is Due. Shared database: own prefixes; rows are read per goal only.
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class ExemptionRaceTests(DatabaseFixture database) : IAsyncLifetime
{
    private const string HasRecord = "This commitment has a record. Delete the record to excuse it.";
    private const string CommitmentExcused = "This commitment is excused. Remove the excusal to record it.";
    private const string Overlaps = "This goal already has a pause on these dates.";
    private const string DatePaused = "This goal is paused on this date.";

    private static readonly DateOnly Wed18 = Start;
    private static readonly DateOnly Fri20 = new(2026, 11, 20);
    private static readonly DateOnly Sun22 = new(2026, 11, 22);
    private static readonly DateOnly Tue24 = new(2026, 11, 24);

    /// <summary>How long the test waits at most for a write to queue on the held lock.</summary>
    private static readonly TimeSpan QueueTimeout = TimeSpan.FromSeconds(10);

    private ServiceProvider _services = null!;

    public Task InitializeAsync()
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddHtApplication()
            .AddHtPersistence(database.ConnectionString);
        services.AddSingleton<TimeProvider>(new FakeTimeProvider(new DateTimeOffset(2026, 11, 18, 10, 0, 0, TimeSpan.Zero)));
        _services = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
        return Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _services.DisposeAsync();

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task An_excusal_and_a_record_of_one_commitment_started_together_leave_exactly_one_of_them(bool excuseFirst)
    {
        var suffix = Suffix();
        var owner = await OwnerAsync(database, "exm-race", suffix);
        var goal = await GoalAsync(database, owner, "Exm race", suffix, YogaMonWedFri());
        var user = ActingUser.ForAccount(owner, Role.RegisteredUser);

        Task<ExcuseResult>? excuse = null;
        Task<RecordWriteResult>? record = null;
        var queued = false;
        await HoldingTheGoalRowAsync(owner, goal, async waitUntilBlocked =>
        {
            bool firstQueued;
            if (excuseFirst)
            {
                excuse = ExcuseAsync(user, goal);
                firstQueued = await waitUntilBlocked(1);
                record = RecordAsync(user, goal);
            }
            else
            {
                record = RecordAsync(user, goal);
                firstQueued = await waitUntilBlocked(1);
                excuse = ExcuseAsync(user, goal);
            }

            queued = firstQueued && await waitUntilBlocked(2);
        });

        var excused = await excuse!;
        var recorded = await record!;
        var records = await RecordRowsAsync(database.ConnectionString, goal);
        var excusals = await Sql.QueryAsync(
            database.ConnectionString, "SELECT [Date] FROM [GoalExcusals] WHERE [GoalId] = @goal", ("@goal", goal.Value));
        if (queued)
        {
            // Both writes were seen waiting in their start order; SQL Server grants the update lock first come, first served.
            Assert.Equal(excuseFirst, excused.Outcome == ExcuseOutcome.Excused);
        }

        if (excused.Outcome == ExcuseOutcome.Excused)
        {
            Assert.Equal(RecordWriteOutcome.CommitmentExcused, recorded.Outcome);
            Assert.Equal(CommitmentExcused, recorded.Message);
            Assert.Single(excusals);
            Assert.Empty(records);
        }
        else
        {
            Assert.Equal(RecordWriteOutcome.Recorded, recorded.Outcome);
            Assert.Equal(ExcuseOutcome.HasRecord, excused.Outcome);
            Assert.Equal(HasRecord, excused.Message);
            Assert.Single(records);
            Assert.Empty(excusals);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Two_overlapping_pauses_started_together_leave_exactly_one_of_them(bool earlierFirst)
    {
        var suffix = Suffix();
        var owner = await OwnerAsync(database, "exm-race-p", suffix);
        var goal = await GoalAsync(database, owner, "Exm race pause", suffix, YogaMonWedFri());
        var user = ActingUser.ForAccount(owner, Role.RegisteredUser);

        // Wednesday 18 to Sunday 22, and Friday 20 to Tuesday 24: they share Friday 20 to Sunday 22.
        Task<PauseResult>? earlier = null;
        Task<PauseResult>? later = null;
        var queued = false;
        await HoldingTheGoalRowAsync(owner, goal, async waitUntilBlocked =>
        {
            bool firstQueued;
            if (earlierFirst)
            {
                earlier = PauseAsync(user, goal, Wed18, Sun22);
                firstQueued = await waitUntilBlocked(1);
                later = PauseAsync(user, goal, Fri20, Tue24);
            }
            else
            {
                later = PauseAsync(user, goal, Fri20, Tue24);
                firstQueued = await waitUntilBlocked(1);
                earlier = PauseAsync(user, goal, Wed18, Sun22);
            }

            queued = firstQueued && await waitUntilBlocked(2);
        });

        var results = new[] { await earlier!, await later! };
        var pauses = await PauseRowsAsync(goal);
        if (queued)
        {
            Assert.Equal(earlierFirst, results[0].Outcome == PauseOutcome.Paused);
        }

        var stored = Assert.Single(results, result => result.Outcome == PauseOutcome.Paused);
        var refused = Assert.Single(results, result => result.Outcome != PauseOutcome.Paused);
        Assert.Equal(PauseOutcome.Overlaps, refused.Outcome);
        Assert.Equal(Overlaps, refused.Message);
        var row = Assert.Single(pauses);
        Assert.Equal(stored.Pause!.FirstDay, DateOnly.FromDateTime((DateTime)row["FirstDay"]!));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task A_pause_and_a_record_of_one_date_started_together_end_with_the_record_refused_or_kept_under_the_pause(bool pauseFirst)
    {
        var suffix = Suffix();
        var owner = await OwnerAsync(database, "exm-race-r", suffix);
        var goal = await GoalAsync(database, owner, "Exm race record", suffix, YogaMonWedFri());
        var user = ActingUser.ForAccount(owner, Role.RegisteredUser);

        Task<PauseResult>? pause = null;
        Task<RecordWriteResult>? record = null;
        var queued = false;
        await HoldingTheGoalRowAsync(owner, goal, async waitUntilBlocked =>
        {
            bool firstQueued;
            if (pauseFirst)
            {
                pause = PauseAsync(user, goal, Wed18, Sun22);
                firstQueued = await waitUntilBlocked(1);
                record = RecordAsync(user, goal);
            }
            else
            {
                record = RecordAsync(user, goal);
                firstQueued = await waitUntilBlocked(1);
                pause = PauseAsync(user, goal, Wed18, Sun22);
            }

            queued = firstQueued && await waitUntilBlocked(2);
        });

        var paused = await pause!;
        var recorded = await record!;
        var records = await RecordRowsAsync(database.ConnectionString, goal);

        // A pause never refuses a record already stored (FR-008, FR-009); only the record can lose.
        Assert.Equal(PauseOutcome.Paused, paused.Outcome);
        Assert.Single(await PauseRowsAsync(goal));
        if (queued)
        {
            Assert.Equal(pauseFirst, recorded.Outcome == RecordWriteOutcome.DatePaused);
        }

        if (recorded.Outcome == RecordWriteOutcome.DatePaused)
        {
            Assert.Equal(DatePaused, recorded.Message);
            Assert.Empty(records);
        }
        else
        {
            Assert.Equal(RecordWriteOutcome.Recorded, recorded.Outcome);
            Assert.Single(records);
        }
    }

    private Task<PauseResult> PauseAsync(ActingUser user, GoalId goal, DateOnly firstDay, DateOnly lastDay) => Task.Run(async () =>
    {
        await using var scope = _services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<PauseService>()
            .PauseAsync(user, goal, firstDay, lastDay, null, CancellationToken.None);
    });

    private Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> PauseRowsAsync(GoalId goal) =>
        Sql.QueryAsync(database.ConnectionString, "SELECT [FirstDay] FROM [GoalPauses] WHERE [GoalId] = @goal", ("@goal", goal.Value));

    private Task<ExcuseResult> ExcuseAsync(ActingUser user, GoalId goal) => Task.Run(async () =>
    {
        await using var scope = _services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ExcusalService>().ExcuseAsync(user, goal, Wed18, "Ill", CancellationToken.None);
    });

    private Task<RecordWriteResult> RecordAsync(ActingUser user, GoalId goal) => Task.Run(async () =>
    {
        await using var scope = _services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<RecordingService>()
            .RecordAsync(user, goal, Wed18, new RecordInput { Answer = YesNoAnswer.Yes }, CancellationToken.None);
    });

    /// <summary>
    /// Holds the goal row's update lock on another connection while <paramref name="start"/> starts the writes, then
    /// releases it. <paramref name="start"/> gets a wait that ends, true, once that many requests are blocked by the holder,
    /// or, false, after <see cref="QueueTimeout"/>.
    /// </summary>
    private async Task HoldingTheGoalRowAsync(AccountId owner, GoalId goal, Func<Func<int, Task<bool>>, Task> start)
    {
        await using var holder = new SqlConnection(database.ConnectionString);
        await holder.OpenAsync();
        await using var transaction = (SqlTransaction)await holder.BeginTransactionAsync();
        await using (var lockGoal = new SqlCommand(
            "SELECT 1 FROM [Goals] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = @goal AND [OwnerId] = @owner", holder, transaction))
        {
            lockGoal.Parameters.AddWithValue("@goal", goal.Value);
            lockGoal.Parameters.AddWithValue("@owner", owner.Value);
            Assert.Equal(1, await lockGoal.ExecuteScalarAsync());
        }

        short session;
        await using (var spid = new SqlCommand("SELECT CAST(@@SPID AS smallint)", holder, transaction))
        {
            session = (short)(await spid.ExecuteScalarAsync())!;
        }

        await start(count => WaitUntilBlockedAsync(session, count));
        await transaction.CommitAsync();
    }

    /// <summary>
    /// Waits until <paramref name="count"/> requests are blocked by <paramref name="session"/>, directly or behind another
    /// request it blocks. Without the permission to see other sessions the count stays 0, and the wait ends after
    /// <see cref="QueueTimeout"/>; the writes still race on the released lock.
    /// </summary>
    private async Task<bool> WaitUntilBlockedAsync(short session, int count)
    {
        const string blockedChain =
            "WITH [Chain] AS ("
            + " SELECT [session_id] FROM sys.dm_exec_requests WHERE [blocking_session_id] = @session"
            + " UNION ALL"
            + " SELECT [r].[session_id] FROM sys.dm_exec_requests AS [r] JOIN [Chain] AS [c] ON [r].[blocking_session_id] = [c].[session_id]"
            + ") SELECT COUNT(DISTINCT [session_id]) FROM [Chain] OPTION (MAXRECURSION 10)";
        var deadline = DateTime.UtcNow + QueueTimeout;
        while (DateTime.UtcNow < deadline)
        {
            var blocked = await Sql.ScalarAsync(database.ConnectionString, blockedChain, ("@session", session));
            if (blocked is int found && found >= count)
            {
                return true;
            }

            await Task.Delay(50);
        }

        return false;
    }
}
