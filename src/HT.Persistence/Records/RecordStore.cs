using System.Data.Common;
using HT.Application.Records;
using HT.Domain.Accounts;
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
/// exception (HP-006).
/// </summary>
internal sealed class RecordStore : IRecordStore
{
    private readonly HtDbContext _db;

    public RecordStore(HtDbContext db) => _db = db;

    public Task<RecordStoreWrite> AddAsync(
        AccountId owner,
        GoalId goal,
        EntrySpan entry,
        bool isCommitment,
        DateOnly date,
        RecordValue value,
        NoteText? note,
        int limit,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(value);
        return InGoalTransactionAsync(owner, goal, async () =>
        {
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

            if (note is not null)
            {
                await UpsertNoteAsync(goal.Value, entry.Key, note, cancellationToken).ConfigureAwait(false);
            }

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
        NoteText? note,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(value);
        return InGoalTransactionAsync(owner, goal, async () =>
        {
            if (await SetValueAsync(goal.Value, record.Value, value, cancellationToken).ConfigureAwait(false) == 0)
            {
                return RecordStoreWrite.RecordNotFound;
            }

            if (note is not null)
            {
                await UpsertNoteAsync(goal.Value, entry.Key, note, cancellationToken).ConfigureAwait(false);
            }

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
        return InGoalTransactionAsync(owner, goal, async () =>
        {
            if (note is null)
            {
                await _db.GoalNotes
                    .Where(n => n.GoalId == goal.Value && n.EntryDate == entry.Key)
                    .ExecuteDeleteAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
            else
            {
                await UpsertNoteAsync(goal.Value, entry.Key, note, cancellationToken).ConfigureAwait(false);
            }

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
    /// Research R-05: one transaction that first locks the owner's goal row with UPDLOCK, HOLDLOCK. No row gives
    /// <see cref="RecordStoreWrite.GoalNotFound"/> and writes nothing; the transaction commits only when
    /// <paramref name="write"/> writes, and rolls back for its refusals.
    /// </summary>
    private async Task<RecordStoreWrite> InGoalTransactionAsync(
        AccountId owner, GoalId goal, Func<Task<RecordStoreWrite>> write, CancellationToken cancellationToken)
    {
        _db.ChangeTracker.Clear();
        try
        {
            return await RunWriteAsync(async () =>
            {
                await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
                var locked = await _db.Database.SqlQuery<int>(
                        $"SELECT 1 AS [Value] FROM [Goals] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {goal.Value} AND [OwnerId] = {owner.Value}")
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
                if (locked.Count != 1)
                {
                    await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                    return RecordStoreWrite.GoalNotFound;
                }

                var result = await write().ConfigureAwait(false);
                if (result.Outcome == RecordStoreWriteOutcome.Written)
                {
                    await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                }

                return result;
            }).ConfigureAwait(false);
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

    /// <summary>Runs a write; a database fault becomes an exception with the SQL error number only (HP-006).</summary>
    private static Task<T> RunWriteAsync<T>(Func<Task<T>> write) => RunAsync(write, "Saving a record failed");

    /// <summary>Runs a read; a database fault becomes an exception with the SQL error number only (HP-006).</summary>
    private static Task<T> RunReadAsync<T>(Func<Task<T>> read) => RunAsync(read, "Reading records failed");

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
}
