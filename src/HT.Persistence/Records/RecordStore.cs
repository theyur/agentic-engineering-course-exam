using System.Data.Common;
using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Exemptions;
using HT.Domain.Goals;
using HT.Domain.Records;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace HT.Persistence.Records;

/// <summary>
/// The record store (research R-05). Every statement is scoped by owner (constitution III): each write runs in one
/// transaction that first locks the goal row with UPDLOCK, HOLDLOCK where the goal is one of the owner's, so the writes
/// on one goal are serialized and nothing is written for another owner's goal; each read joins <c>Goals</c> on the
/// owner. A record is looked up only among the records of the goal named, never by its identifier alone. Every write
/// reads the entry's records and note in the same transaction and returns them. No statement raises a database error
/// that carries a value: the checks are conditional statements, and the values written already satisfy the Domain's
/// ranges and lengths. Any database fault becomes an exception that names the SQL error number only, with no inner
/// exception (HP-006). Slice 7 (research R-04): the goal's excusals and pauses are written in the same transaction and
/// under the same lock, each write re-checking its own invariant there, and a record add is refused on an excused or
/// paused entry, so D-3 and FR-009 hold under concurrent requests too. They are read in one owner-scoped statement.
/// </summary>
internal sealed class RecordStore : IRecordStore
{
    private const string SaveRecordFailed = "Saving a record failed";
    private const string SaveExemptionFailed = "Saving an excusal or a pause failed";
    private const string ReadExemptionsFailed = "Reading excusals and pauses failed";

    private readonly HtDbContext _db;

    public RecordStore(HtDbContext db) => _db = db;

    public Task<RecordStoreWrite> AddAsync(
        AccountId owner,
        GoalId goal,
        EntrySpan entry,
        bool isCommitment,
        DateOnly date,
        RecordValue value,
        NoteWrite note,
        int limit,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(note);
        return InGoalTransactionAsync(owner, goal, async () =>
        {
            // Slice 7 (research R-04, D-3, FR-009): no record is added on an excused or paused entry, under the lock.
            if (await IsExemptAsync(goal.Value, entry, cancellationToken).ConfigureAwait(false))
            {
                return RecordStoreWrite.Exempt;
            }

            var inEntry = RecordsOf(goal.Value, entry.FirstDate, entry.LastDate);
            RecordId id;
            var existing = isCommitment
                ? await inEntry.Select(r => (Guid?)r.Id).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false)
                : null;
            if (existing is { } existingId)
            {
                // A commitment's record keeps its identifier and its place in the stored order (FR-003).
                await SetValueAsync(goal.Value, existingId, value, cancellationToken).ConfigureAwait(false);
                id = new RecordId(existingId);
            }
            else
            {
                if (!isCommitment && await inEntry.CountAsync(cancellationToken).ConfigureAwait(false) >= limit)
                {
                    return RecordStoreWrite.LimitReached;
                }

                var row = RecordMapping.ToRow(goal.Value, date, value);
                _db.GoalRecords.Add(row);
                await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                id = new RecordId(row.Id);
            }

            await ApplyNoteAsync(goal.Value, entry.Key, note, cancellationToken).ConfigureAwait(false);
            return RecordStoreWrite.Written(id, await ReadEntryAsync(goal.Value, entry, cancellationToken).ConfigureAwait(false));
        }, cancellationToken);
    }

    public Task<DateOnly?> FindRecordDateAsync(AccountId owner, GoalId goal, RecordId record, CancellationToken cancellationToken) =>
        RunReadAsync(() => OwnedRecords(owner, goal)
            .Where(r => r.Id == record.Value)
            .Select(r => (DateOnly?)r.Date)
            .SingleOrDefaultAsync(cancellationToken));

    public Task<RecordStoreWrite> ChangeAsync(
        AccountId owner,
        GoalId goal,
        RecordId record,
        EntrySpan entry,
        RecordValue value,
        NoteWrite note,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(note);
        return InGoalTransactionAsync(owner, goal, async () =>
        {
            if (await SetValueAsync(goal.Value, record.Value, value, cancellationToken).ConfigureAwait(false) == 0)
            {
                return RecordStoreWrite.RecordNotFound;
            }

            await ApplyNoteAsync(goal.Value, entry.Key, note, cancellationToken).ConfigureAwait(false);
            return RecordStoreWrite.Written(record, await ReadEntryAsync(goal.Value, entry, cancellationToken).ConfigureAwait(false));
        }, cancellationToken);
    }

    public Task<RecordStoreWrite> DeleteAsync(AccountId owner, GoalId goal, RecordId record, EntrySpan entry, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entry);
        return InGoalTransactionAsync(owner, goal, async () =>
        {
            var deleted = await _db.GoalRecords
                .Where(r => r.Id == record.Value && r.GoalId == goal.Value)
                .ExecuteDeleteAsync(cancellationToken)
                .ConfigureAwait(false);
            if (deleted == 0)
            {
                return RecordStoreWrite.RecordNotFound;
            }

            return RecordStoreWrite.Written(await ReadEntryAsync(goal.Value, entry, cancellationToken).ConfigureAwait(false));
        }, cancellationToken);
    }

    public Task<RecordStoreWrite> SaveNoteAsync(AccountId owner, GoalId goal, EntrySpan entry, NoteText? note, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entry);
        var write = note is null ? NoteWrite.Remove : NoteWrite.Replace(note);
        return InGoalTransactionAsync(owner, goal, async () =>
        {
            await ApplyNoteAsync(goal.Value, entry.Key, write, cancellationToken).ConfigureAwait(false);
            return RecordStoreWrite.Written(await ReadEntryAsync(goal.Value, entry, cancellationToken).ConfigureAwait(false));
        }, cancellationToken);
    }

    /// <summary>Two no-tracking queries joined to the owner's goal: the records dated in the range, and the notes keyed in it.</summary>
    public Task<StoredEntries> ReadAsync(AccountId owner, GoalId goal, DateOnly from, DateOnly to, CancellationToken cancellationToken) =>
        RunReadAsync(async () =>
        {
            var records = await OwnedRecords(owner, goal)
                .Where(r => r.Date >= from && r.Date <= to)
                .OrderBy(r => r.Date)
                .ThenBy(r => r.Sequence)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            var notes = await OwnedNotes(owner, goal)
                .Where(n => n.EntryDate >= from && n.EntryDate <= to)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            IReadOnlyDictionary<DateOnly, NoteText> byKey = notes.ToDictionary(n => n.EntryDate, RecordMapping.ToNote);
            return new StoredEntries(records.Select(RecordMapping.ToRecord).ToList(), byKey);
        });

    public Task<IReadOnlyList<DateOnly>> ReadRecentNoteKeysAsync(AccountId owner, GoalId goal, int count, CancellationToken cancellationToken)
    {
        if (count <= 0)
        {
            return Task.FromResult<IReadOnlyList<DateOnly>>([]);
        }

        return RunReadAsync<IReadOnlyList<DateOnly>>(async () => await OwnedNotes(owner, goal)
            .OrderByDescending(n => n.EntryDate)
            .Select(n => n.EntryDate)
            .Take(count)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false));
    }

    /// <summary>
    /// Slice 7 (research R-04): every excusal and pause of the owner's goal in one statement, the two tables joined to
    /// <c>Goals</c> on the owner and combined with UNION ALL; an excusal's row has no pause identifier. None when the
    /// goal is not one of the owner's.
    /// </summary>
    public Task<GoalExemptions> ReadExemptionsAsync(AccountId owner, GoalId goal, CancellationToken cancellationToken) =>
        RunAsync(async () =>
        {
            var lines = await _db.Database.SqlQuery<ExemptionLine>(
                    $"""
                    SELECT CAST(NULL AS uniqueidentifier) AS [PauseId], e.[GoalId], e.[Date] AS [FirstDay], e.[Date] AS [LastDay], e.[Reason]
                    FROM [GoalExcusals] AS e INNER JOIN [Goals] AS g ON g.[Id] = e.[GoalId]
                    WHERE g.[Id] = {goal.Value} AND g.[OwnerId] = {owner.Value}
                    UNION ALL
                    SELECT p.[Id] AS [PauseId], p.[GoalId], p.[FirstDay], p.[LastDay], p.[Reason]
                    FROM [GoalPauses] AS p INNER JOIN [Goals] AS g ON g.[Id] = p.[GoalId]
                    WHERE g.[Id] = {goal.Value} AND g.[OwnerId] = {owner.Value}
                    """)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            if (lines.Count == 0)
            {
                return GoalExemptions.None;
            }

            var excusals = lines
                .Where(l => l.PauseId is null)
                .Select(l => ExemptionMapping.ToExcusal(new GoalExcusalRow { GoalId = l.GoalId, Date = l.FirstDay, Reason = l.Reason }));
            var pauses = lines
                .Where(l => l.PauseId is not null)
                .Select(l => ExemptionMapping.ToPause(new GoalPauseRow
                {
                    Id = l.PauseId!.Value, GoalId = l.GoalId, FirstDay = l.FirstDay, LastDay = l.LastDay, Reason = l.Reason,
                }));
            return new GoalExemptions(excusals.ToList(), pauses.ToList());
        }, ReadExemptionsFailed);

    /// <summary>
    /// Excuses the commitment (FR-001), re-checking under the goal lock, in the contract's order: an excusal of the
    /// date, a record dated on it (D-3), a pause covering it (FR-009).
    /// </summary>
    public Task<ExemptionStoreWrite> ExcuseAsync(
        AccountId owner, GoalId goal, DateOnly date, ExemptionReason? reason, CancellationToken cancellationToken) =>
        InGoalExemptionTransactionAsync(owner, goal, async () =>
        {
            if (await _db.GoalExcusals.AsNoTracking()
                    .AnyAsync(e => e.GoalId == goal.Value && e.Date == date, cancellationToken).ConfigureAwait(false))
            {
                return ExemptionStoreWrite.AlreadyExcused;
            }

            if (await _db.GoalRecords.AsNoTracking()
                    .AnyAsync(r => r.GoalId == goal.Value && r.Date == date, cancellationToken).ConfigureAwait(false))
            {
                return ExemptionStoreWrite.HasRecord;
            }

            if (await PausesOf(goal.Value, date, date).AnyAsync(cancellationToken).ConfigureAwait(false))
            {
                return ExemptionStoreWrite.Paused;
            }

            _db.GoalExcusals.Add(ExemptionMapping.ToRow(goal.Value, date, reason));
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return ExemptionStoreWrite.Written();
        }, cancellationToken);

    public Task<ExemptionStoreWrite> RemoveExcusalAsync(AccountId owner, GoalId goal, DateOnly date, CancellationToken cancellationToken) =>
        InGoalExemptionTransactionAsync(owner, goal, async () =>
        {
            var deleted = await _db.GoalExcusals
                .Where(e => e.GoalId == goal.Value && e.Date == date)
                .ExecuteDeleteAsync(cancellationToken)
                .ConfigureAwait(false);
            return deleted == 0 ? ExemptionStoreWrite.NotExcused : ExemptionStoreWrite.Written();
        }, cancellationToken);

    /// <summary>Adds the pause with a new identifier, re-checking under the goal lock that it shares no date with another.</summary>
    public Task<ExemptionStoreWrite> AddPauseAsync(
        AccountId owner, GoalId goal, DateOnly firstDay, DateOnly lastDay, ExemptionReason? reason, CancellationToken cancellationToken)
    {
        var row = ExemptionMapping.ToRow(goal.Value, firstDay, lastDay, reason);
        return InGoalExemptionTransactionAsync(owner, goal, async () =>
        {
            if (await PausesOf(goal.Value, firstDay, lastDay).AnyAsync(cancellationToken).ConfigureAwait(false))
            {
                return ExemptionStoreWrite.Overlaps;
            }

            _db.GoalPauses.Add(row);
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return ExemptionStoreWrite.Written(new PauseId(row.Id));
        }, cancellationToken);
    }

    /// <summary>
    /// Gives the pause, found by the goal and its identifier together, a new last day (FR-010). A last day before its
    /// first day is a caller's fault. A later last day that would share a date with another pause gives
    /// <see cref="ExemptionStoreWrite.Overlaps"/>, so the goal's pauses never share a date (the use case only shortens).
    /// </summary>
    public Task<ExemptionStoreWrite> SetPauseLastDayAsync(
        AccountId owner, GoalId goal, PauseId pause, DateOnly lastDay, CancellationToken cancellationToken) =>
        InGoalExemptionTransactionAsync(owner, goal, async () =>
        {
            var row = await _db.GoalPauses.AsNoTracking()
                .SingleOrDefaultAsync(p => p.Id == pause.Value && p.GoalId == goal.Value, cancellationToken)
                .ConfigureAwait(false);
            if (row is null)
            {
                return ExemptionStoreWrite.PauseNotFound;
            }

            if (lastDay < row.FirstDay)
            {
                throw new ArgumentException("The last day cannot be before the first day.", nameof(lastDay));
            }

            if (lastDay > row.LastDay
                && await PausesOf(goal.Value, row.LastDay.AddDays(1), lastDay).AnyAsync(cancellationToken).ConfigureAwait(false))
            {
                return ExemptionStoreWrite.Overlaps;
            }

            await _db.GoalPauses
                .Where(p => p.Id == pause.Value && p.GoalId == goal.Value)
                .ExecuteUpdateAsync(setters => setters.SetProperty(p => p.LastDay, lastDay), cancellationToken)
                .ConfigureAwait(false);
            return ExemptionStoreWrite.Written();
        }, cancellationToken);

    public Task<ExemptionStoreWrite> DeletePauseAsync(AccountId owner, GoalId goal, PauseId pause, CancellationToken cancellationToken) =>
        InGoalExemptionTransactionAsync(owner, goal, async () =>
        {
            var deleted = await _db.GoalPauses
                .Where(p => p.Id == pause.Value && p.GoalId == goal.Value)
                .ExecuteDeleteAsync(cancellationToken)
                .ConfigureAwait(false);
            return deleted == 0 ? ExemptionStoreWrite.PauseNotFound : ExemptionStoreWrite.Written();
        }, cancellationToken);

    /// <summary>
    /// Research R-05: one transaction that first locks the owner's goal row with UPDLOCK, HOLDLOCK. No row gives
    /// <see cref="RecordStoreWrite.GoalNotFound"/> and writes nothing; the transaction commits only when
    /// <paramref name="write"/> writes, and rolls back for its refusals.
    /// </summary>
    private Task<RecordStoreWrite> InGoalTransactionAsync(
        AccountId owner, GoalId goal, Func<Task<RecordStoreWrite>> write, CancellationToken cancellationToken) =>
        InGoalTransactionAsync(
            owner, goal, write, RecordStoreWrite.GoalNotFound, r => r.Outcome == RecordStoreWriteOutcome.Written, SaveRecordFailed, cancellationToken);

    /// <summary>
    /// Slice 7 (research R-04): the same transaction and lock as the record writes, so the excusal, pause and record
    /// writes on one goal are serialized. No goal row gives <see cref="ExemptionStoreWrite.GoalNotFound"/> and writes
    /// nothing.
    /// </summary>
    private Task<ExemptionStoreWrite> InGoalExemptionTransactionAsync(
        AccountId owner, GoalId goal, Func<Task<ExemptionStoreWrite>> write, CancellationToken cancellationToken) =>
        InGoalTransactionAsync(
            owner, goal, write, ExemptionStoreWrite.GoalNotFound, r => r.Outcome == ExemptionStoreWriteOutcome.Written, SaveExemptionFailed,
            cancellationToken);

    /// <summary>
    /// The one goal-lock transaction of every write: it first locks the owner's goal row with UPDLOCK, HOLDLOCK. No row
    /// gives <paramref name="goalNotFound"/> and writes nothing; the transaction commits only when the result of
    /// <paramref name="write"/> <paramref name="commits"/>, and rolls back for its refusals.
    /// </summary>
    private async Task<T> InGoalTransactionAsync<T>(
        AccountId owner, GoalId goal, Func<Task<T>> write, T goalNotFound, Func<T, bool> commits, string what, CancellationToken cancellationToken)
    {
        _db.ChangeTracker.Clear();
        try
        {
            return await RunAsync(async () =>
            {
                await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
                var locked = await _db.Database.SqlQuery<int>(
                        $"SELECT 1 AS [Value] FROM [Goals] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {goal.Value} AND [OwnerId] = {owner.Value}")
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
                if (locked.Count != 1)
                {
                    await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                    return goalNotFound;
                }

                var result = await write().ConfigureAwait(false);
                if (commits(result))
                {
                    await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                }

                return result;
            }, what).ConfigureAwait(false);
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }
    }

    /// <summary>The goal's record gets the value; its date, identifier and stored order stay. The number of rows changed.</summary>
    private Task<int> SetValueAsync(Guid goal, Guid record, RecordValue value, CancellationToken cancellationToken)
    {
        var answer = RecordMapping.AnswerOf(value);
        var amount = value.Amount;
        return _db.GoalRecords
            .Where(r => r.Id == record && r.GoalId == goal)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(r => r.Answer, answer)
                    .SetProperty(r => r.Amount, amount),
                cancellationToken);
    }

    /// <summary>
    /// Every note write's one path (specs/006-note-editing research R-03), inside the goal's transaction:
    /// <see cref="NoteWriteKind.Keep"/> writes nothing, <see cref="NoteWriteKind.Remove"/> deletes the entry's note if
    /// there is one, and <see cref="NoteWriteKind.Replace"/> adds or replaces it.
    /// </summary>
    private async Task ApplyNoteAsync(Guid goal, DateOnly key, NoteWrite note, CancellationToken cancellationToken)
    {
        switch (note.Kind)
        {
            case NoteWriteKind.Keep:
                break;
            case NoteWriteKind.Remove:
                await _db.GoalNotes
                    .Where(n => n.GoalId == goal && n.EntryDate == key)
                    .ExecuteDeleteAsync(cancellationToken)
                    .ConfigureAwait(false);
                break;
            case NoteWriteKind.Replace:
                await UpsertNoteAsync(goal, key, note.Text!, cancellationToken).ConfigureAwait(false);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(note), "Not a kind of note write.");
        }
    }

    /// <summary>Adds or replaces the entry's note; safe without a merge because every note write holds the goal lock.</summary>
    private async Task UpsertNoteAsync(Guid goal, DateOnly key, NoteText note, CancellationToken cancellationToken)
    {
        var text = note.Value;
        var updated = await _db.GoalNotes
            .Where(n => n.GoalId == goal && n.EntryDate == key)
            .ExecuteUpdateAsync(setters => setters.SetProperty(n => n.Text, text), cancellationToken)
            .ConfigureAwait(false);
        if (updated == 0)
        {
            _db.GoalNotes.Add(RecordMapping.ToRow(goal, key, note));
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>The entry's records (date, then stored order) and its note, read inside the write's transaction.</summary>
    private async Task<StoredEntry> ReadEntryAsync(Guid goal, EntrySpan entry, CancellationToken cancellationToken)
    {
        var records = await RecordsOf(goal, entry.FirstDate, entry.LastDate)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var note = await _db.GoalNotes.AsNoTracking()
            .Where(n => n.GoalId == goal && n.EntryDate == entry.Key)
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
        return new StoredEntry(records.Select(RecordMapping.ToRecord).ToList(), note is null ? null : RecordMapping.ToNote(note));
    }

    /// <summary>A locked goal's records dated in the range, in date order and then in the stored order.</summary>
    private IQueryable<GoalRecordRow> RecordsOf(Guid goal, DateOnly first, DateOnly last) =>
        _db.GoalRecords.AsNoTracking()
            .Where(r => r.GoalId == goal && r.Date >= first && r.Date <= last)
            .OrderBy(r => r.Date)
            .ThenBy(r => r.Sequence);

    /// <summary>The goal's records, joined to the goal on its owner.</summary>
    private IQueryable<GoalRecordRow> OwnedRecords(AccountId owner, GoalId goal) =>
        from record in _db.GoalRecords.AsNoTracking()
        join row in _db.Goals.AsNoTracking() on record.GoalId equals row.Id
        where row.Id == goal.Value && row.OwnerId == owner.Value
        select record;

    /// <summary>The goal's notes, joined to the goal on its owner.</summary>
    private IQueryable<GoalNoteRow> OwnedNotes(AccountId owner, GoalId goal) =>
        from note in _db.GoalNotes.AsNoTracking()
        join row in _db.Goals.AsNoTracking() on note.GoalId equals row.Id
        where row.Id == goal.Value && row.OwnerId == owner.Value
        select note;

    /// <summary>
    /// Slice 7 (research R-04, D-3, FR-009): whether a locked goal's entry has an excusal on its key date or a pause on
    /// one of its dates.
    /// </summary>
    private async Task<bool> IsExemptAsync(Guid goal, EntrySpan entry, CancellationToken cancellationToken) =>
        await _db.GoalExcusals.AsNoTracking()
            .AnyAsync(e => e.GoalId == goal && e.Date == entry.Key, cancellationToken)
            .ConfigureAwait(false)
        || await PausesOf(goal, entry.FirstDate, entry.LastDate).AnyAsync(cancellationToken).ConfigureAwait(false);

    /// <summary>A locked goal's pauses that share a date with the range <paramref name="first"/> to <paramref name="last"/>.</summary>
    private IQueryable<GoalPauseRow> PausesOf(Guid goal, DateOnly first, DateOnly last) =>
        _db.GoalPauses.AsNoTracking().Where(p => p.GoalId == goal && p.FirstDay <= last && first <= p.LastDay);

    /// <summary>Runs a read; a database fault becomes an exception with the SQL error number only (HP-006).</summary>
    private static Task<T> RunReadAsync<T>(Func<Task<T>> read) => RunAsync(read, "Reading records failed");

    /// <summary>Runs <paramref name="action"/>; a database fault becomes an exception that names <paramref name="what"/> and the SQL error number only (HP-006).</summary>
    private static async Task<T> RunAsync<T>(Func<Task<T>> action, string what)
    {
        int number;
        try
        {
            return await action().ConfigureAwait(false);
        }
        catch (SqlException exception)
        {
            number = exception.Number;
        }
        catch (DbUpdateException exception)
        {
            number = exception.InnerException is SqlException sql ? sql.Number : 0;
        }
        catch (DbException)
        {
            number = 0;
        }

        throw new InvalidOperationException($"{what} (SQL error {number}).");
    }

    /// <summary>
    /// One row of <see cref="ReadExemptionsAsync"/>'s statement: an excusal (no <see cref="PauseId"/>, its date as both
    /// days) or a pause. Mapped by column name; never tracked.
    /// </summary>
    private sealed class ExemptionLine
    {
        public Guid? PauseId { get; init; }

        public Guid GoalId { get; init; }

        public DateOnly FirstDay { get; init; }

        public DateOnly LastDay { get; init; }

        public string? Reason { get; init; }
    }
}
