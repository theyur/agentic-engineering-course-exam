using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Exemptions;
using HT.Domain.Goals;
using HT.Domain.Records;

namespace HT.UnitTests.Fakes;

/// <summary>
/// A hand-written in-memory <see cref="IRecordStore"/> (ADR 0006), owner-scoped like the real one, with the outcomes
/// of research R-05. Every member checks the owner against the goals of a <see cref="FakeGoalStore"/>, so another
/// owner's goal and no goal look the same. Records are kept per goal with a stored order; a commitment's record is
/// replaced in place; a period takes no record past the limit; notes are upserted and removed under the entry key, and
/// a record's write applies its <see cref="NoteWrite"/> only after its own checks pass, so a refusal leaves the note.
/// <see cref="Calls"/> names every member called, so a test can prove that a refusal made no store call. Shared with
/// HT.UiTests by a link. Safe to call from parallel tasks; the goal store's lock is never held while this one is.
/// <para>
/// Slice 7 (research R-04; contracts/application-api.md, "Re-checked under the lock"): excusals and pauses are kept per
/// goal with the real store's re-checks and outcomes, a record add is refused with <see cref="RecordStoreWrite.Exempt"/>
/// on an excused or paused entry, and <see cref="ReadExemptionsAsync"/> gives <see cref="GoalExemptions.None"/> for
/// another owner's goal. <see cref="SeedExcusal"/> and <see cref="SeedPause"/> arrange any dates, past ones included,
/// and <see cref="BeforeNextWrite"/> simulates a concurrent write between a use case's checks and its store write.
/// </para>
/// </summary>
public sealed class FakeRecordStore : IRecordStore
{
    private readonly object _gate = new();
    private readonly FakeGoalStore _goals;
    private readonly Dictionary<GoalId, List<StoredRecord>> _records = [];
    private readonly Dictionary<GoalId, SortedDictionary<DateOnly, NoteText>> _notes = [];
    private readonly Dictionary<GoalId, SortedDictionary<DateOnly, ExemptionReason?>> _excusals = [];
    private readonly Dictionary<GoalId, List<Pause>> _pauses = [];
    private readonly List<string> _calls = [];
    private long _sequence;
    private Action? _beforeNextWrite;
    private Action? _afterNextAdd;

    /// <summary>
    /// Takes the goal store whose goals decide who owns what, and (slice 8) attaches itself as its
    /// <see cref="FakeGoalStore.Records"/>, so an ending cuts this store's pauses and excusals.
    /// </summary>
    public FakeRecordStore(FakeGoalStore goals)
    {
        ArgumentNullException.ThrowIfNull(goals);
        _goals = goals;
        goals.Records = this;
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

    /// <summary>
    /// An action run once, at the start of the next write that the exemptions re-check (<see cref="AddAsync"/>,
    /// <see cref="ExcuseAsync"/>, <see cref="RemoveExcusalAsync"/>, <see cref="AddPauseAsync"/>,
    /// <see cref="SetPauseLastDayAsync"/> or <see cref="DeletePauseAsync"/>), before that write's ownership check and
    /// re-checks, and then cleared. A unit test sets it to simulate a concurrent excusal, pause or
    /// record between a use case's checks and its write, for example with <see cref="SeedExcusal"/>,
    /// <see cref="SeedPause"/> or <see cref="Seed"/>. It runs outside this store's lock; members it calls on this store
    /// are recorded in <see cref="Calls"/> before the write's own name.
    /// </summary>
    public Action? BeforeNextWrite
    {
        get => Volatile.Read(ref _beforeNextWrite);
        set => Volatile.Write(ref _beforeNextWrite, value);
    }

    /// <summary>
    /// An action run once, at the end of the next <see cref="AddAsync"/>, after its result is decided and outside this
    /// store's lock, and then cleared. With <see cref="BeforeNextWrite"/>, a unit test simulates an excusal or pause that
    /// is added before the write and removed again before the use case reads the exemptions a second time.
    /// </summary>
    public Action? AfterNextAdd
    {
        get => Volatile.Read(ref _afterNextAdd);
        set => Volatile.Write(ref _afterNextAdd, value);
    }

    /// <summary>
    /// Stores an excusal of <paramref name="date"/> directly, for arranging a test (any date, past ones included), with no
    /// ownership check, no re-check and no <see cref="Calls"/> entry. An existing excusal of the date is replaced.
    /// </summary>
    public void SeedExcusal(GoalId goal, DateOnly date, ExemptionReason? reason = null)
    {
        lock (_gate)
        {
            ExcusalsOf(goal)[date] = reason;
        }
    }

    /// <summary>
    /// Stores a pause directly, for arranging a test (any dates, past ones included, which the use case would refuse),
    /// with no ownership check, no overlap check and no <see cref="Calls"/> entry; its new identifier. Pauses of one goal
    /// must not share a date, or <see cref="ReadExemptionsAsync"/> throws, as <see cref="GoalExemptions"/> does.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="lastDay"/> is before <paramref name="firstDay"/>.</exception>
    public PauseId SeedPause(GoalId goal, DateOnly firstDay, DateOnly lastDay, ExemptionReason? reason = null)
    {
        var pause = new Pause(new PauseId(Guid.NewGuid()), firstDay, lastDay, reason);
        lock (_gate)
        {
            PausesOf(goal).Add(pause);
        }

        return pause.Id;
    }

    /// <summary>Every excusal and pause of the goal, with no ownership check and no <see cref="Calls"/> entry; for assertions only.</summary>
    public GoalExemptions ExemptionsOfGoal(GoalId goal)
    {
        lock (_gate)
        {
            return ExemptionsOf(goal);
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
        NoteWrite note,
        int limit,
        CancellationToken cancellationToken)
    {
        var result = Add(owner, goal, entry, isCommitment, date, value, note, limit);
        Interlocked.Exchange(ref _afterNextAdd, null)?.Invoke();
        return result;
    }

    private Task<RecordStoreWrite> Add(
        AccountId owner,
        GoalId goal,
        EntrySpan entry,
        bool isCommitment,
        DateOnly date,
        RecordValue value,
        NoteWrite note,
        int limit)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(note);
        RunBeforeNextWrite();
        var owned = Owns(owner, goal);
        lock (_gate)
        {
            _calls.Add(nameof(AddAsync));
            if (!owned)
            {
                return Task.FromResult(RecordStoreWrite.GoalNotFound);
            }

            // Slice 7 (research R-04): no record is added on an excused or paused entry; nothing is written.
            var exemptions = ExemptionsOf(goal);
            if (exemptions.IsExcused(entry.Key) || exemptions.Overlaps(entry.FirstDate, entry.LastDate))
            {
                return Task.FromResult(RecordStoreWrite.Exempt);
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

            ApplyNote(goal, entry.Key, note);
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
        NoteWrite note,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(note);
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
            ApplyNote(goal, entry.Key, note);
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

    /// <summary>Slice 8: the newest <paramref name="count"/> note keys from <paramref name="from"/> through <paramref name="through"/> (no upper bound when null).</summary>
    public Task<IReadOnlyList<DateOnly>> ReadRecentNoteKeysAsync(
        AccountId owner, GoalId goal, DateOnly from, DateOnly? through, int count, CancellationToken cancellationToken)
    {
        var owned = Owns(owner, goal);
        lock (_gate)
        {
            _calls.Add(nameof(ReadRecentNoteKeysAsync));
            IReadOnlyList<DateOnly> keys = owned
                ? NotesOf(goal).Keys.Where(k => k >= from && (through is not { } last || k <= last)).Reverse().Take(count).ToList()
                : [];
            return Task.FromResult(keys);
        }
    }

    /// <summary>
    /// Slice 8 (research R-04): the cuts of an ending, called by <see cref="FakeGoalStore.TryEndAsync"/> under its lock:
    /// the goal's pauses that begin after <paramref name="lastDate"/> are deleted, those that cover it end on it, and its
    /// excusals after it are deleted. Records and notes stay. No <see cref="Calls"/> entry.
    /// </summary>
    public void CutAfter(GoalId goal, DateOnly lastDate)
    {
        lock (_gate)
        {
            var pauses = PausesOf(goal);
            pauses.RemoveAll(p => p.FirstDay > lastDate);
            for (var i = 0; i < pauses.Count; i++)
            {
                if (pauses[i].LastDay > lastDate)
                {
                    pauses[i] = new Pause(pauses[i].Id, pauses[i].FirstDay, lastDate, pauses[i].Reason);
                }
            }

            var excusals = ExcusalsOf(goal);
            foreach (var date in excusals.Keys.Where(d => d > lastDate).ToList())
            {
                excusals.Remove(date);
            }
        }
    }

    public Task<GoalExemptions> ReadExemptionsAsync(AccountId owner, GoalId goal, CancellationToken cancellationToken)
    {
        var owned = Owns(owner, goal);
        lock (_gate)
        {
            _calls.Add(nameof(ReadExemptionsAsync));
            return Task.FromResult(owned ? ExemptionsOf(goal) : GoalExemptions.None);
        }
    }

    public Task<ExemptionStoreWrite> ExcuseAsync(
        AccountId owner, GoalId goal, DateOnly date, ExemptionReason? reason, CancellationToken cancellationToken)
    {
        RunBeforeNextWrite();
        var owned = Owns(owner, goal);
        lock (_gate)
        {
            _calls.Add(nameof(ExcuseAsync));
            if (!owned)
            {
                return Task.FromResult(ExemptionStoreWrite.GoalNotFound);
            }

            var excusals = ExcusalsOf(goal);
            if (excusals.ContainsKey(date))
            {
                return Task.FromResult(ExemptionStoreWrite.AlreadyExcused);
            }

            if (RecordsOf(goal).Any(r => r.Record.Date == date))
            {
                return Task.FromResult(ExemptionStoreWrite.HasRecord);
            }

            if (PausesOf(goal).Any(p => p.Covers(date)))
            {
                return Task.FromResult(ExemptionStoreWrite.Paused);
            }

            excusals[date] = reason;
            return Task.FromResult(ExemptionStoreWrite.Written());
        }
    }

    public Task<ExemptionStoreWrite> RemoveExcusalAsync(AccountId owner, GoalId goal, DateOnly date, CancellationToken cancellationToken)
    {
        RunBeforeNextWrite();
        var owned = Owns(owner, goal);
        lock (_gate)
        {
            _calls.Add(nameof(RemoveExcusalAsync));
            if (!owned)
            {
                return Task.FromResult(ExemptionStoreWrite.GoalNotFound);
            }

            return Task.FromResult(ExcusalsOf(goal).Remove(date) ? ExemptionStoreWrite.Written() : ExemptionStoreWrite.NotExcused);
        }
    }

    public Task<ExemptionStoreWrite> AddPauseAsync(
        AccountId owner, GoalId goal, DateOnly firstDay, DateOnly lastDay, ExemptionReason? reason, CancellationToken cancellationToken)
    {
        // The real store throws for a reversed range before it opens its transaction; so does the fake.
        var pause = new Pause(new PauseId(Guid.NewGuid()), firstDay, lastDay, reason);
        RunBeforeNextWrite();
        var owned = Owns(owner, goal);
        var ended = owned && _goals.Goals.Any(g => g.Id == goal && g.IsEnded);
        lock (_gate)
        {
            _calls.Add(nameof(AddPauseAsync));
            if (!owned)
            {
                return Task.FromResult(ExemptionStoreWrite.GoalNotFound);
            }

            // Slice 8 (research R-05): no pause on a goal whose latest attempt is ended; nothing is written.
            if (ended)
            {
                return Task.FromResult(ExemptionStoreWrite.GoalEnded);
            }

            var pauses = PausesOf(goal);
            if (pauses.Any(p => p.Overlaps(firstDay, lastDay)))
            {
                return Task.FromResult(ExemptionStoreWrite.Overlaps);
            }

            pauses.Add(pause);
            return Task.FromResult(ExemptionStoreWrite.Written(pause.Id));
        }
    }

    public Task<ExemptionStoreWrite> SetPauseLastDayAsync(
        AccountId owner, GoalId goal, PauseId pause, DateOnly lastDay, CancellationToken cancellationToken)
    {
        RunBeforeNextWrite();
        var owned = Owns(owner, goal);
        lock (_gate)
        {
            _calls.Add(nameof(SetPauseLastDayAsync));
            if (!owned)
            {
                return Task.FromResult(ExemptionStoreWrite.GoalNotFound);
            }

            var pauses = PausesOf(goal);
            var position = pauses.FindIndex(p => p.Id == pause);
            if (position < 0)
            {
                return Task.FromResult(ExemptionStoreWrite.PauseNotFound);
            }

            var current = pauses[position];
            if (lastDay < current.FirstDay)
            {
                throw new ArgumentException("The last day cannot be before the first day.", nameof(lastDay));
            }

            // As the real store: a later last day that would share a date with another pause is refused.
            if (lastDay > current.LastDay && pauses.Any(p => p.Id != pause && p.Overlaps(current.LastDay.AddDays(1), lastDay)))
            {
                return Task.FromResult(ExemptionStoreWrite.Overlaps);
            }

            pauses[position] = new Pause(current.Id, current.FirstDay, lastDay, current.Reason);
            return Task.FromResult(ExemptionStoreWrite.Written());
        }
    }

    public Task<ExemptionStoreWrite> DeletePauseAsync(AccountId owner, GoalId goal, PauseId pause, CancellationToken cancellationToken)
    {
        RunBeforeNextWrite();
        var owned = Owns(owner, goal);
        lock (_gate)
        {
            _calls.Add(nameof(DeletePauseAsync));
            if (!owned)
            {
                return Task.FromResult(ExemptionStoreWrite.GoalNotFound);
            }

            return Task.FromResult(PausesOf(goal).RemoveAll(p => p.Id == pause) == 0
                ? ExemptionStoreWrite.PauseNotFound
                : ExemptionStoreWrite.Written());
        }
    }

    private static bool In(DateOnly date, EntrySpan entry) => date >= entry.FirstDate && date <= entry.LastDate;

    /// <summary>Takes and clears <see cref="BeforeNextWrite"/>, then runs it, outside this store's lock.</summary>
    private void RunBeforeNextWrite() => Interlocked.Exchange(ref _beforeNextWrite, null)?.Invoke();

    /// <summary>The goal's excusals and pauses as the real store reads them; called under this store's lock.</summary>
    private GoalExemptions ExemptionsOf(GoalId goal) =>
        new(ExcusalsOf(goal).Select(e => new Excusal(e.Key, e.Value)).ToList(), PausesOf(goal).ToList());

    private static List<GoalRecord> Ordered(IEnumerable<StoredRecord> records, DateOnly from, DateOnly to) =>
        records
            .Where(r => r.Record.Date >= from && r.Record.Date <= to)
            .OrderBy(r => r.Record.Date)
            .ThenBy(r => r.Sequence)
            .Select(r => r.Record)
            .ToList();

    /// <summary>Read before this store's lock is taken, so the goal store's lock is never held inside it.</summary>
    private bool Owns(AccountId owner, GoalId goal) => _goals.Goals.Any(g => g.Id == goal && g.Owner == owner);

    /// <summary>A record's note write, as the real store applies it: Keep leaves the note, Remove drops the key, Replace sets it.</summary>
    private void ApplyNote(GoalId goal, DateOnly key, NoteWrite note)
    {
        switch (note.Kind)
        {
            case NoteWriteKind.Keep:
                break;
            case NoteWriteKind.Remove:
                NotesOf(goal).Remove(key);
                break;
            case NoteWriteKind.Replace:
                NotesOf(goal)[key] = note.Text!;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(note), "Not a kind of note write.");
        }
    }

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

    private SortedDictionary<DateOnly, ExemptionReason?> ExcusalsOf(GoalId goal)
    {
        if (!_excusals.TryGetValue(goal, out var excusals))
        {
            excusals = [];
            _excusals[goal] = excusals;
        }

        return excusals;
    }

    private List<Pause> PausesOf(GoalId goal)
    {
        if (!_pauses.TryGetValue(goal, out var pauses))
        {
            pauses = [];
            _pauses[goal] = pauses;
        }

        return pauses;
    }

    /// <summary>A record and its place in the stored order, which a change keeps (FR-003).</summary>
    private sealed record StoredRecord(GoalRecord Record, long Sequence);
}
