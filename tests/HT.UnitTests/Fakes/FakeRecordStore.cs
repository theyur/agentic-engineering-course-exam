using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Records;

namespace HT.UnitTests.Fakes;

/// <summary>
/// A hand-written in-memory <see cref="IRecordStore"/> (ADR 0006), owner-scoped like the real one, with the outcomes
/// of research R-05. Every member checks the owner against the goals of a <see cref="FakeGoalStore"/>, so another
/// owner's goal and no goal look the same. Records are kept per goal with a stored order; a commitment's record is
/// replaced in place; a period takes no record past the limit; notes are upserted and removed under the entry key.
/// <see cref="Calls"/> names every member called, so a test can prove that a refusal made no store call. Shared with
/// HT.UiTests by a link. Safe to call from parallel tasks; the goal store's lock is never held while this one is.
/// </summary>
public sealed class FakeRecordStore : IRecordStore
{
    private readonly object _gate = new();
    private readonly FakeGoalStore _goals;
    private readonly Dictionary<GoalId, List<StoredRecord>> _records = [];
    private readonly Dictionary<GoalId, SortedDictionary<DateOnly, NoteText>> _notes = [];
    private readonly List<string> _calls = [];
    private long _sequence;

    /// <summary>Takes the goal store whose goals decide who owns what.</summary>
    public FakeRecordStore(FakeGoalStore goals)
    {
        ArgumentNullException.ThrowIfNull(goals);
        _goals = goals;
    }

    /// <summary>The names of the members called (<c>"AddAsync"</c>, …), in call order, refused ones included.</summary>
    public IReadOnlyList<string> Calls
    {
        get
        {
            lock (_gate)
            {
                return _calls.ToList();
            }
        }
    }

    /// <summary>
    /// Stores a record directly, for arranging a test (for example a period with 50 records), with no ownership check
    /// and no <see cref="Calls"/> entry. It is appended to the stored order.
    /// </summary>
    public RecordId Seed(GoalId goal, DateOnly date, RecordValue value)
    {
        ArgumentNullException.ThrowIfNull(value);
        lock (_gate)
        {
            var id = new RecordId(Guid.NewGuid());
            RecordsOf(goal).Add(new StoredRecord(new GoalRecord(id, date, value), ++_sequence));
            return id;
        }
    }

    /// <summary>Stores a note directly under <paramref name="key"/>, for arranging a test, with no <see cref="Calls"/> entry.</summary>
    public void SeedNote(GoalId goal, DateOnly key, NoteText note)
    {
        ArgumentNullException.ThrowIfNull(note);
        lock (_gate)
        {
            NotesOf(goal)[key] = note;
        }
    }

    /// <summary>Every record of the goal, in date order and then in the stored order; for assertions only.</summary>
    public IReadOnlyList<GoalRecord> RecordsOfGoal(GoalId goal)
    {
        lock (_gate)
        {
            return Ordered(RecordsOf(goal), DateOnly.MinValue, DateOnly.MaxValue);
        }
    }

    /// <summary>Every note of the goal by key; for assertions only.</summary>
    public IReadOnlyDictionary<DateOnly, NoteText> NotesOfGoal(GoalId goal)
    {
        lock (_gate)
        {
            return new Dictionary<DateOnly, NoteText>(NotesOf(goal));
        }
    }

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
        var owned = Owns(owner, goal);
        lock (_gate)
        {
            _calls.Add(nameof(AddAsync));
            if (!owned)
            {
                return Task.FromResult(RecordStoreWrite.GoalNotFound);
            }

            var records = RecordsOf(goal);
            var inEntry = records.Where(r => In(r.Record.Date, entry)).ToList();
            RecordId id;
            if (isCommitment && inEntry.Count > 0)
            {
                var existing = inEntry[0];
                id = existing.Record.Id;
                records[records.IndexOf(existing)] = existing with { Record = existing.Record with { Value = value } };
            }
            else
            {
                if (!isCommitment && inEntry.Count >= limit)
                {
                    return Task.FromResult(RecordStoreWrite.LimitReached);
                }

                id = new RecordId(Guid.NewGuid());
                records.Add(new StoredRecord(new GoalRecord(id, date, value), ++_sequence));
            }

            if (note is not null)
            {
                NotesOf(goal)[entry.Key] = note;
            }

            return Task.FromResult(RecordStoreWrite.Written(id, EntryOf(goal, entry)));
        }
    }

    public Task<DateOnly?> FindRecordDateAsync(AccountId owner, GoalId goal, RecordId record, CancellationToken cancellationToken)
    {
        var owned = Owns(owner, goal);
        lock (_gate)
        {
            _calls.Add(nameof(FindRecordDateAsync));
            if (!owned)
            {
                return Task.FromResult<DateOnly?>(null);
            }

            var found = RecordsOf(goal).FirstOrDefault(r => r.Record.Id == record);
            return Task.FromResult(found?.Record.Date);
        }
    }

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
        var owned = Owns(owner, goal);
        lock (_gate)
        {
            _calls.Add(nameof(ChangeAsync));
            if (!owned)
            {
                return Task.FromResult(RecordStoreWrite.GoalNotFound);
            }

            var records = RecordsOf(goal);
            var position = records.FindIndex(r => r.Record.Id == record);
            if (position < 0)
            {
                return Task.FromResult(RecordStoreWrite.RecordNotFound);
            }

            records[position] = records[position] with { Record = records[position].Record with { Value = value } };
            if (note is not null)
            {
                NotesOf(goal)[entry.Key] = note;
            }

            return Task.FromResult(RecordStoreWrite.Written(record, EntryOf(goal, entry)));
        }
    }

    public Task<RecordStoreWrite> DeleteAsync(AccountId owner, GoalId goal, RecordId record, EntrySpan entry, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entry);
        var owned = Owns(owner, goal);
        lock (_gate)
        {
            _calls.Add(nameof(DeleteAsync));
            if (!owned)
            {
                return Task.FromResult(RecordStoreWrite.GoalNotFound);
            }

            if (RecordsOf(goal).RemoveAll(r => r.Record.Id == record) == 0)
            {
                return Task.FromResult(RecordStoreWrite.RecordNotFound);
            }

            return Task.FromResult(RecordStoreWrite.Written(EntryOf(goal, entry)));
        }
    }

    public Task<RecordStoreWrite> SaveNoteAsync(AccountId owner, GoalId goal, EntrySpan entry, NoteText? note, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entry);
        var owned = Owns(owner, goal);
        lock (_gate)
        {
            _calls.Add(nameof(SaveNoteAsync));
            if (!owned)
            {
                return Task.FromResult(RecordStoreWrite.GoalNotFound);
            }

            if (note is null)
            {
                NotesOf(goal).Remove(entry.Key);
            }
            else
            {
                NotesOf(goal)[entry.Key] = note;
            }

            return Task.FromResult(RecordStoreWrite.Written(EntryOf(goal, entry)));
        }
    }

    public Task<StoredEntries> ReadAsync(AccountId owner, GoalId goal, DateOnly from, DateOnly to, CancellationToken cancellationToken)
    {
        var owned = Owns(owner, goal);
        lock (_gate)
        {
            _calls.Add(nameof(ReadAsync));
            if (!owned)
            {
                return Task.FromResult(new StoredEntries([], new Dictionary<DateOnly, NoteText>()));
            }

            IReadOnlyDictionary<DateOnly, NoteText> notes = NotesOf(goal)
                .Where(n => n.Key >= from && n.Key <= to)
                .ToDictionary(n => n.Key, n => n.Value);
            return Task.FromResult(new StoredEntries(Ordered(RecordsOf(goal), from, to), notes));
        }
    }

    public Task<IReadOnlyList<DateOnly>> ReadRecentNoteKeysAsync(AccountId owner, GoalId goal, int count, CancellationToken cancellationToken)
    {
        var owned = Owns(owner, goal);
        lock (_gate)
        {
            _calls.Add(nameof(ReadRecentNoteKeysAsync));
            IReadOnlyList<DateOnly> keys = owned ? NotesOf(goal).Keys.Reverse().Take(count).ToList() : [];
            return Task.FromResult(keys);
        }
    }

    private static bool In(DateOnly date, EntrySpan entry) => date >= entry.FirstDate && date <= entry.LastDate;

    private static List<GoalRecord> Ordered(IEnumerable<StoredRecord> records, DateOnly from, DateOnly to) =>
        records
            .Where(r => r.Record.Date >= from && r.Record.Date <= to)
            .OrderBy(r => r.Record.Date)
            .ThenBy(r => r.Sequence)
            .Select(r => r.Record)
            .ToList();

    /// <summary>Read before this store's lock is taken, so the goal store's lock is never held inside it.</summary>
    private bool Owns(AccountId owner, GoalId goal) => _goals.Goals.Any(g => g.Id == goal && g.Owner == owner);

    private StoredEntry EntryOf(GoalId goal, EntrySpan entry) =>
        new(Ordered(RecordsOf(goal), entry.FirstDate, entry.LastDate), NotesOf(goal).GetValueOrDefault(entry.Key));

    private List<StoredRecord> RecordsOf(GoalId goal)
    {
        if (!_records.TryGetValue(goal, out var records))
        {
            records = [];
            _records[goal] = records;
        }

        return records;
    }

    private SortedDictionary<DateOnly, NoteText> NotesOf(GoalId goal)
    {
        if (!_notes.TryGetValue(goal, out var notes))
        {
            notes = [];
            _notes[goal] = notes;
        }

        return notes;
    }

    /// <summary>A record and its place in the stored order, which a change keeps (FR-003).</summary>
    private sealed record StoredRecord(GoalRecord Record, long Sequence);
}
