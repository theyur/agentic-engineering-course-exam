using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Records;
using Microsoft.Extensions.DependencyInjection;
using static HT.IntegrationTests.Records.RecordSamples;

namespace HT.IntegrationTests.Records;

/// <summary>
/// Owner scoping of every <see cref="IRecordStore"/> member on SQL Server (US6, FR-017, SC-004; constitution III,
/// research R-05, R-14). User A has a daily commitment goal and a 3-times-a-week period goal, each with records and a
/// note. Every member, called with another owner for A's goal (user B, who has a goal of B's own, and an owner that no
/// account has), gives <see cref="RecordStoreWriteOutcome.GoalNotFound"/>, null or empty and writes nothing; a record of
/// another goal of the same owner gives <see cref="RecordStoreWriteOutcome.RecordNotFound"/> or null. A's rows, and B's,
/// are checked with plain SQL before and after. The sample goals start on Wednesday 18 November 2026 (the week goal on
/// Monday 16) in UTC. Shared database: own prefixes (<see cref="RecordSamples.EmailPrefix"/>,
/// <see cref="RecordSamples.HabitPrefix"/>); rows are read per owner only.
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class RecordPrivacyTests(DatabaseFixture database)
{
    private static readonly DateOnly Mon16 = new(2026, 11, 16);
    private static readonly DateOnly Wed18 = Start;
    private static readonly DateOnly Thu19 = Start.AddDays(1);
    private static readonly DateOnly Sun22 = new(2026, 11, 22);

    private static CancellationToken None => CancellationToken.None;

    private async Task<T> WithStoreAsync<T>(Func<IRecordStore, Task<T>> call)
    {
        await using var scope = database.CreateScope();
        return await call(scope.ServiceProvider.GetRequiredService<IRecordStore>());
    }

    private static NoteText Note(string text)
    {
        Assert.True(NoteText.TryCreate(text, out var note, out _));
        return note!;
    }

    /// <summary>Every record row of the owner's goals, as one text per row, in a stable order.</summary>
    private async Task<IReadOnlyList<string>> RecordRowsOfOwnerAsync(AccountId owner)
    {
        var rows = await Sql.QueryAsync(database.ConnectionString,
            """
            SELECT r.[Id], r.[GoalId], r.[Date], r.[Answer], r.[Amount], r.[Sequence]
            FROM [GoalRecords] r JOIN [Goals] g ON g.[Id] = r.[GoalId]
            WHERE g.[OwnerId] = @owner
            ORDER BY r.[GoalId], r.[Date], r.[Sequence]
            """,
            ("@owner", owner.Value));
        return rows.Select(r => string.Join("|", r.Values)).ToList();
    }

    /// <summary>Every note row of the owner's goals, as one text per row, in a stable order.</summary>
    private async Task<IReadOnlyList<string>> NoteRowsOfOwnerAsync(AccountId owner)
    {
        var rows = await Sql.QueryAsync(database.ConnectionString,
            """
            SELECT n.[GoalId], n.[EntryDate], n.[Text]
            FROM [GoalNotes] n JOIN [Goals] g ON g.[Id] = n.[GoalId]
            WHERE g.[OwnerId] = @owner
            ORDER BY n.[GoalId], n.[EntryDate]
            """,
            ("@owner", owner.Value));
        return rows.Select(r => string.Join("|", r.Values)).ToList();
    }

    /// <summary>
    /// User A with a daily goal (25 min on Wednesday 18 and a note on it) and a week goal (two records on Wednesday 18
    /// and Thursday 19 and a note under Monday 16), and user B with a goal of B's own with a record and a note.
    /// </summary>
    private async Task<World> ArrangeAsync(string name)
    {
        var suffix = Suffix();
        var a = await OwnerAsync(database, $"{name}-a", suffix);
        var b = await OwnerAsync(database, $"{name}-b", suffix);
        var daily = await GoalAsync(database, a, $"Privacy daily {name}", suffix, Reading20MinDaily());
        var weekly = await GoalAsync(database, a, $"Privacy weekly {name}", suffix, Pages20ThreeTimesAWeek(), Mon16);
        var bsGoal = await GoalAsync(database, b, $"Privacy B {name}", suffix, Reading20MinDaily());

        var dailyRecord = await AddAsync(a, daily, Day(Wed18), true, Wed18, Minutes(25), Note("A's daily note."));
        var weekRecord = await AddAsync(a, weekly, Week(Mon16), false, Wed18, RecordValue.FromAmount(21), Note("A's week note."));
        await AddAsync(a, weekly, Week(Mon16), false, Thu19, RecordValue.FromAmount(22), null);
        await AddAsync(b, bsGoal, Day(Wed18), true, Wed18, Minutes(30), Note("B's own note."));
        return new World(a, b, daily, weekly, bsGoal, dailyRecord, weekRecord);
    }

    private async Task<RecordId> AddAsync(AccountId owner, GoalId goal, EntrySpan entry, bool isCommitment, DateOnly date, RecordValue value, NoteText? note)
    {
        var written = await WithStoreAsync(s => s.AddAsync(
            owner, goal, entry, isCommitment, date, value, note is null ? NoteWrite.Keep : NoteWrite.Replace(note), RecordLimits.MaxRecordsPerPeriod, None));
        Assert.Equal(RecordStoreWriteOutcome.Written, written.Outcome);
        return written.RecordId!.Value;
    }

    private async Task<Snapshot> SnapshotAsync(World world) => new(
        await RecordRowsOfOwnerAsync(world.A),
        await NoteRowsOfOwnerAsync(world.A),
        await RecordRowsOfOwnerAsync(world.B),
        await NoteRowsOfOwnerAsync(world.B));

    private static void AssertRefusedWrite(RecordStoreWrite write, RecordStoreWriteOutcome outcome)
    {
        Assert.Equal(outcome, write.Outcome);
        Assert.Null(write.RecordId);
        Assert.Null(write.Entry);
    }

    /// <summary>
    /// Every member of the store called by <paramref name="owner"/> on A's goals with A's records, with values that would
    /// write if the goals were <paramref name="owner"/>'s: a record on each goal, a change and a delete of each of A's
    /// records, a note added and a note deleted on each goal, a record added and a record changed with
    /// <see cref="NoteWrite.Remove"/>, a read of each goal's range and its recent note keys.
    /// </summary>
    private async Task AssertEveryMemberGivesNothingAsync(AccountId owner, World world)
    {
        var goals = new[] { (world.Daily, Day(Wed18), true, world.DailyRecord), (world.Weekly, Week(Mon16), false, world.WeekRecord) };
        foreach (var (goal, entry, isCommitment, record) in goals)
        {
            AssertRefusedWrite(
                await WithStoreAsync(s => s.AddAsync(
                    owner, goal, entry, isCommitment, Thu19, Minutes(5), NoteWrite.Replace(Note("Not A's note.")), RecordLimits.MaxRecordsPerPeriod, None)),
                RecordStoreWriteOutcome.GoalNotFound);
            Assert.Null(await WithStoreAsync(s => s.FindRecordDateAsync(owner, goal, record, None)));
            AssertRefusedWrite(
                await WithStoreAsync(s => s.ChangeAsync(owner, goal, record, entry, Minutes(5), NoteWrite.Replace(Note("Not A's note.")), None)),
                RecordStoreWriteOutcome.GoalNotFound);
            AssertRefusedWrite(await WithStoreAsync(s => s.DeleteAsync(owner, goal, record, entry, None)), RecordStoreWriteOutcome.GoalNotFound);
            AssertRefusedWrite(await WithStoreAsync(s => s.SaveNoteAsync(owner, goal, entry, Note("Not A's note."), None)), RecordStoreWriteOutcome.GoalNotFound);
            AssertRefusedWrite(await WithStoreAsync(s => s.SaveNoteAsync(owner, goal, entry, null, None)), RecordStoreWriteOutcome.GoalNotFound);

            // The record's own note delete (specs/006-note-editing FR-001): from another owner it gives GoalNotFound too,
            // and leaves A's note, which the callers' snapshots check.
            AssertRefusedWrite(
                await WithStoreAsync(s => s.AddAsync(owner, goal, entry, isCommitment, Thu19, Minutes(5), NoteWrite.Remove, RecordLimits.MaxRecordsPerPeriod, None)),
                RecordStoreWriteOutcome.GoalNotFound);
            AssertRefusedWrite(
                await WithStoreAsync(s => s.ChangeAsync(owner, goal, record, entry, Minutes(5), NoteWrite.Remove, None)),
                RecordStoreWriteOutcome.GoalNotFound);

            var read = await WithStoreAsync(s => s.ReadAsync(owner, goal, Mon16, Sun22, None));
            Assert.Empty(read.Records);
            Assert.Empty(read.Notes);
            Assert.Empty(await WithStoreAsync(s => s.ReadRecentNoteKeysAsync(owner, goal, 5, None)));
        }
    }

    [Fact]
    public async Task US6_AS1_every_store_member_called_by_another_registered_user_for_As_goals_gives_nothing_and_writes_nothing()
    {
        var world = await ArrangeAsync("other-user");
        var before = await SnapshotAsync(world);

        await AssertEveryMemberGivesNothingAsync(world.B, world);

        Assert.Equal(before, await SnapshotAsync(world));
        Assert.Equal(2, before.ARecords.Count(r => r.Contains(world.Weekly.Value.ToString(), StringComparison.OrdinalIgnoreCase)));
        Assert.Equal(2, before.ANotes.Count);
        Assert.Single(before.BRecords);
        Assert.Single(before.BNotes);
    }

    [Fact]
    public async Task FR_017_every_store_member_called_with_an_owner_that_no_account_has_gives_nothing_and_writes_nothing()
    {
        var world = await ArrangeAsync("no-owner");
        var before = await SnapshotAsync(world);

        await AssertEveryMemberGivesNothingAsync(new AccountId(Guid.NewGuid()), world);

        Assert.Equal(before, await SnapshotAsync(world));
    }

    [Fact]
    public async Task US6_AS2_B_finding_changing_or_deleting_As_record_on_Bs_own_goal_gets_RecordNotFound_and_writes_nothing()
    {
        var world = await ArrangeAsync("on-own-goal");
        var before = await SnapshotAsync(world);

        foreach (var record in new[] { world.DailyRecord, world.WeekRecord })
        {
            Assert.Null(await WithStoreAsync(s => s.FindRecordDateAsync(world.B, world.BsGoal, record, None)));
            AssertRefusedWrite(
                await WithStoreAsync(s => s.ChangeAsync(world.B, world.BsGoal, record, Day(Wed18), Minutes(5), NoteWrite.Replace(Note("B's other note.")), None)),
                RecordStoreWriteOutcome.RecordNotFound);
            AssertRefusedWrite(await WithStoreAsync(s => s.DeleteAsync(world.B, world.BsGoal, record, Day(Wed18), None)), RecordStoreWriteOutcome.RecordNotFound);
        }

        Assert.Equal(before, await SnapshotAsync(world));
    }

    [Fact]
    public async Task A_record_of_another_goal_of_the_same_owner_is_not_found_not_changed_and_not_deleted()
    {
        var world = await ArrangeAsync("same-owner");
        var before = await SnapshotAsync(world);

        var pairs = new[] { (world.Weekly, world.DailyRecord, Day(Wed18)), (world.Daily, world.WeekRecord, Week(Mon16)) };
        foreach (var (goal, recordOfTheOtherGoal, entry) in pairs)
        {
            Assert.Null(await WithStoreAsync(s => s.FindRecordDateAsync(world.A, goal, recordOfTheOtherGoal, None)));
            AssertRefusedWrite(
                await WithStoreAsync(s => s.ChangeAsync(world.A, goal, recordOfTheOtherGoal, entry, Minutes(5), NoteWrite.Replace(Note("Wrong goal.")), None)),
                RecordStoreWriteOutcome.RecordNotFound);
            AssertRefusedWrite(await WithStoreAsync(s => s.DeleteAsync(world.A, goal, recordOfTheOtherGoal, entry, None)), RecordStoreWriteOutcome.RecordNotFound);
        }

        Assert.Equal(before, await SnapshotAsync(world));
    }

    [Fact]
    public async Task The_owner_reads_what_the_others_were_refused_so_the_refusals_are_not_for_want_of_data()
    {
        var world = await ArrangeAsync("owner-reads");

        var daily = await WithStoreAsync(s => s.ReadAsync(world.A, world.Daily, Mon16, Sun22, None));
        var weekly = await WithStoreAsync(s => s.ReadAsync(world.A, world.Weekly, Mon16, Sun22, None));

        Assert.Equal(Wed18, await WithStoreAsync(s => s.FindRecordDateAsync(world.A, world.Daily, world.DailyRecord, None)));
        Assert.Equal(25, Assert.Single(daily.Records).Value.Amount);
        Assert.Equal("A's daily note.", daily.Notes[Wed18].Value);
        Assert.Equal([21, 22], weekly.Records.Select(r => r.Value.Amount!.Value));
        Assert.Equal("A's week note.", weekly.Notes[Mon16].Value);
        Assert.Equal([Mon16], await WithStoreAsync(s => s.ReadRecentNoteKeysAsync(world.A, world.Weekly, 5, None)));
    }

    private sealed record World(
        AccountId A, AccountId B, GoalId Daily, GoalId Weekly, GoalId BsGoal, RecordId DailyRecord, RecordId WeekRecord);

    /// <summary>A's and B's record and note rows; equal when every column of every row is equal.</summary>
    private sealed record Snapshot(IReadOnlyList<string> ARecords, IReadOnlyList<string> ANotes, IReadOnlyList<string> BRecords, IReadOnlyList<string> BNotes)
    {
        public bool Equals(Snapshot? other) =>
            other is not null
            && ARecords.SequenceEqual(other.ARecords)
            && ANotes.SequenceEqual(other.ANotes)
            && BRecords.SequenceEqual(other.BRecords)
            && BNotes.SequenceEqual(other.BNotes);

        public override int GetHashCode() => HashCode.Combine(ARecords.Count, ANotes.Count, BRecords.Count, BNotes.Count);
    }
}
