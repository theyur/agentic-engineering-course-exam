using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Records;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using static HT.IntegrationTests.Records.RecordSamples;

namespace HT.IntegrationTests.Records;

/// <summary>
/// The two races of research R-05 on SQL Server (research R-14): two concurrent records on one commitment leave one
/// record, and two concurrent adds on a period with 49 records leave 50. Each race is forced: a third connection holds
/// the goal row's update lock while the two writes, each in a context of its own, pass a barrier together and then
/// queue on that lock; once both wait, the lock is released and the store's own lock serializes them. Shared database:
/// own prefixes; rows are read per goal only.
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class RecordRaceTests(DatabaseFixture database)
{
    private static readonly DateOnly Wed18 = Start;

    /// <summary>How long the test waits at most for both writes to queue on the held lock.</summary>
    private static readonly TimeSpan QueueTimeout = TimeSpan.FromSeconds(10);

    /// <summary>The two amounts of the commitment race, in minutes: either write may be the one that is kept.</summary>
    private static readonly int[] RacedMinutes = [12, 25];

    [Fact]
    public async Task Two_concurrent_records_on_one_commitment_leave_one_record()
    {
        var suffix = Suffix();
        var owner = await OwnerAsync(database, "race-commitment", suffix);
        var goal = await GoalAsync(database, owner, "Race commitment", suffix, Reading20MinDaily());

        var writes = await RaceAsync(owner, goal, (store, value) =>
            store.AddAsync(owner, goal, Day(Wed18), true, Wed18, value, NoteWrite.Keep, RecordLimits.MaxRecordsPerPeriod, CancellationToken.None),
            Minutes(RacedMinutes[0]), Minutes(RacedMinutes[1]));

        Assert.All(writes, w => Assert.Equal(RecordStoreWriteOutcome.Written, w.Outcome));
        Assert.Equal(writes[0].RecordId, writes[1].RecordId);
        var row = Assert.Single(await RecordRowsAsync(database.ConnectionString, goal));
        Assert.Contains((int)row["Amount"]!, RacedMinutes);
    }

    [Fact]
    public async Task Two_concurrent_adds_on_a_period_with_49_records_leave_50()
    {
        var suffix = Suffix();
        var owner = await OwnerAsync(database, "race-period", suffix);
        var goal = await GoalAsync(database, owner, "Race period", suffix, CoffeeAtMost2Cups());
        await Sql.ExecuteAsync(database.ConnectionString,
            "INSERT INTO [GoalRecords] ([Id], [GoalId], [Date], [Answer], [Amount]) "
            + "SELECT NEWID(), @goal, @date, NULL, 1 FROM (SELECT TOP (49) 1 AS [X] FROM sys.all_objects) AS [N]",
            ("@goal", goal.Value), ("@date", Wed18.ToDateTime(TimeOnly.MinValue)));

        var writes = await RaceAsync(owner, goal, (store, value) =>
            store.AddAsync(owner, goal, Day(Wed18), false, Wed18, value, NoteWrite.Keep, RecordLimits.MaxRecordsPerPeriod, CancellationToken.None),
            RecordValue.FromAmount(2), RecordValue.FromAmount(3));

        Assert.Single(writes, w => w.Outcome == RecordStoreWriteOutcome.Written);
        Assert.Single(writes, w => w.Outcome == RecordStoreWriteOutcome.LimitReached);
        Assert.Equal(50, (await RecordRowsAsync(database.ConnectionString, goal)).Count);
    }

    /// <summary>
    /// Runs <paramref name="write"/> twice at once, with <paramref name="first"/> and <paramref name="second"/>, each in a
    /// scope (and so a database context) of its own, while another connection holds the goal row's update lock. The two
    /// pass a barrier together; the lock is released once both are blocked by it, or after <see cref="QueueTimeout"/>.
    /// </summary>
    private async Task<RecordStoreWrite[]> RaceAsync(
        AccountId owner, GoalId goal, Func<IRecordStore, RecordValue, Task<RecordStoreWrite>> write, RecordValue first, RecordValue second)
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

        var holderSession = await SessionIdAsync(holder, transaction);
        using var barrier = new Barrier(2);
        var tasks = new[] { first, second }.Select(value => Task.Run(async () =>
        {
            await using var scope = database.CreateScope();
            var store = scope.ServiceProvider.GetRequiredService<IRecordStore>();
            barrier.SignalAndWait(QueueTimeout);
            return await write(store, value);
        })).ToArray();

        await WaitUntilBlockedAsync(holderSession, 2);
        await transaction.CommitAsync();
        return await Task.WhenAll(tasks);
    }

    private static async Task<short> SessionIdAsync(SqlConnection connection, SqlTransaction transaction)
    {
        await using var command = new SqlCommand("SELECT CAST(@@SPID AS smallint)", connection, transaction);
        return (short)(await command.ExecuteScalarAsync())!;
    }

    /// <summary>
    /// Waits until <paramref name="count"/> requests are blocked by <paramref name="session"/>, directly or behind another
    /// request it blocks (a second waiter for the same lock reports the first waiter as its blocker). Without the
    /// permission to see other sessions the count stays 0, and the wait ends after <see cref="QueueTimeout"/>; the writes
    /// still race on the released lock.
    /// </summary>
    private async Task WaitUntilBlockedAsync(short session, int count)
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
                return;
            }

            await Task.Delay(50);
        }
    }
}
