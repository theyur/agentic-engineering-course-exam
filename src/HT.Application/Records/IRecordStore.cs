using HT.Domain.Accounts;
using HT.Domain.Exemptions;
using HT.Domain.Goals;
using HT.Domain.Records;

namespace HT.Application.Records;

/// <summary>
/// The record store port (research R-05): persistence plumbing that only HT.Application calls and HT.Persistence
/// implements (ADR 0004). Not a use case. Public for the AC reference export; an architecture rule keeps every HT.Ui
/// type away from it (ADR 0010).
/// <para>
/// Records and notes are private (constitution III). Every member takes the owner and the goal first and acts only
/// when the goal is one of the owner's, in the same statement, so another owner's goal and no goal look the same.
/// There is no member without an owner, and a record is looked up only among the records of the goal named. Each
/// write runs in one transaction under the goal row's update lock, so the writes on one goal are serialized, and
/// returns the entry's records and note as they are after the write.
/// </para>
/// </summary>
public interface IRecordStore
{
    /// <summary>
    /// Adds a record dated <paramref name="date"/> to the entry <paramref name="entry"/> (FR-003).
    /// <see cref="RecordStoreWrite.GoalNotFound"/>, with nothing written, when the goal is not one of the owner's. For a
    /// commitment (<paramref name="isCommitment"/>), an existing record of the entry keeps its identifier and its place
    /// in the stored order and gets <paramref name="value"/>; otherwise a record is inserted. For a period,
    /// <see cref="RecordStoreWrite.LimitReached"/>, with nothing written, when the entry already has
    /// <paramref name="limit"/> records. Slice 7 (research R-04): <see cref="RecordStoreWrite.Exempt"/>, with nothing
    /// written, when the entry has an excusal on its key date or a pause on one of its dates from
    /// <see cref="EntrySpan.FirstDate"/> to <see cref="EntrySpan.LastDate"/>. <see cref="RecordStoreWrite.GoalNotFound"/>,
    /// <see cref="RecordStoreWrite.Exempt"/> and <see cref="RecordStoreWrite.LimitReached"/> write nothing, the note
    /// included (specs/006-note-editing FR-003).
    /// In the record's transaction, <paramref name="note"/> applies to the note under <see cref="EntrySpan.Key"/>
    /// (FR-001, D-1): <see cref="NoteWrite.Keep"/> leaves it, <see cref="NoteWrite.Remove"/> deletes it, and
    /// <see cref="NoteWrite.Replace"/> adds or replaces it. Written gives the record's identifier.
    /// </summary>
    Task<RecordStoreWrite> AddAsync(
        AccountId owner,
        GoalId goal,
        EntrySpan entry,
        bool isCommitment,
        DateOnly date,
        RecordValue value,
        NoteWrite note,
        int limit,
        CancellationToken cancellationToken);

    /// <summary>The date of the goal's record <paramref name="record"/>; null when the owner's goal has no such record.</summary>
    Task<DateOnly?> FindRecordDateAsync(AccountId owner, GoalId goal, RecordId record, CancellationToken cancellationToken);

    /// <summary>
    /// Gives the goal's record <paramref name="record"/> the value <paramref name="value"/>; its date, identifier and
    /// place in the stored order stay (FR-003). <see cref="RecordStoreWrite.GoalNotFound"/> or
    /// <see cref="RecordStoreWrite.RecordNotFound"/> write nothing, the note included (specs/006-note-editing FR-003).
    /// In the record's transaction, <paramref name="note"/> applies to the note under <see cref="EntrySpan.Key"/>
    /// (FR-001, D-1): <see cref="NoteWrite.Keep"/> leaves it, <see cref="NoteWrite.Remove"/> deletes it, and
    /// <see cref="NoteWrite.Replace"/> adds or replaces it. Written gives the record's identifier.
    /// </summary>
    Task<RecordStoreWrite> ChangeAsync(
        AccountId owner,
        GoalId goal,
        RecordId record,
        EntrySpan entry,
        RecordValue value,
        NoteWrite note,
        CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the goal's record <paramref name="record"/> (FR-003). <see cref="RecordStoreWrite.GoalNotFound"/> or
    /// <see cref="RecordStoreWrite.RecordNotFound"/> write nothing. It never changes the entry's note, also when it
    /// deletes the entry's only record (specs/006-note-editing FR-004).
    /// </summary>
    Task<RecordStoreWrite> DeleteAsync(AccountId owner, GoalId goal, RecordId record, EntrySpan entry, CancellationToken cancellationToken);

    /// <summary>
    /// Saves the entry's note on its own (FR-007): <paramref name="note"/> adds or replaces it, and null deletes it.
    /// <see cref="RecordStoreWrite.GoalNotFound"/> writes nothing.
    /// </summary>
    Task<RecordStoreWrite> SaveNoteAsync(AccountId owner, GoalId goal, EntrySpan entry, NoteText? note, CancellationToken cancellationToken);

    /// <summary>
    /// The goal's records dated from <paramref name="from"/> to <paramref name="to"/>, in date order and then in the
    /// stored order, and its notes with keys in the same range (research R-05). Callers pass the first entry's first
    /// date and the last entry's last date, so the notes are exactly those of the entries read. Empty when the goal is
    /// not one of the owner's.
    /// </summary>
    Task<StoredEntries> ReadAsync(AccountId owner, GoalId goal, DateOnly from, DateOnly to, CancellationToken cancellationToken);

    /// <summary>
    /// The keys of the goal's <paramref name="count"/> notes with the latest keys, newest first (FR-008). Empty when the
    /// goal is not one of the owner's.
    /// </summary>
    Task<IReadOnlyList<DateOnly>> ReadRecentNoteKeysAsync(AccountId owner, GoalId goal, int count, CancellationToken cancellationToken);

    /// <summary>
    /// Slice 8 (FR-016, research R-05): the keys of the goal's <paramref name="count"/> notes with the latest keys from
    /// <paramref name="from"/> through <paramref name="through"/> (no upper bound when null), newest first. Empty when
    /// the goal is not one of the owner's, and for a <paramref name="count"/> of 0 or less.
    /// </summary>
    Task<IReadOnlyList<DateOnly>> ReadRecentNoteKeysAsync(
        AccountId owner, GoalId goal, DateOnly from, DateOnly? through, int count, CancellationToken cancellationToken);

    /// <summary>
    /// Slice 7 (research R-04): all of the goal's excusals and pauses, in one round trip, owner-scoped in the same
    /// statement. <see cref="GoalExemptions.None"/> when the goal is not one of the owner's. Excused and Paused are
    /// derived from them on read (FR-025).
    /// </summary>
    Task<GoalExemptions> ReadExemptionsAsync(AccountId owner, GoalId goal, CancellationToken cancellationToken);

    /// <summary>
    /// Excuses the goal's commitment of <paramref name="date"/> with an optional reason (FR-001). Re-checked under the
    /// goal lock, in this order: an excusal of the date gives <see cref="ExemptionStoreWrite.AlreadyExcused"/>, a record
    /// dated on it <see cref="ExemptionStoreWrite.HasRecord"/> (D-3), a pause covering it
    /// <see cref="ExemptionStoreWrite.Paused"/>. Every refusal, <see cref="ExemptionStoreWrite.GoalNotFound"/> included,
    /// writes nothing.
    /// </summary>
    Task<ExemptionStoreWrite> ExcuseAsync(
        AccountId owner, GoalId goal, DateOnly date, ExemptionReason? reason, CancellationToken cancellationToken);

    /// <summary>
    /// Removes the excusal of <paramref name="date"/> (FR-004). <see cref="ExemptionStoreWrite.NotExcused"/> when the
    /// date has none. Every refusal writes nothing.
    /// </summary>
    Task<ExemptionStoreWrite> RemoveExcusalAsync(AccountId owner, GoalId goal, DateOnly date, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a pause from <paramref name="firstDay"/> to <paramref name="lastDay"/> (FR-006); the store makes its
    /// identifier, which <see cref="ExemptionStoreWrite.PauseId"/> carries. <see cref="ExemptionStoreWrite.Overlaps"/>
    /// when it would share a date with one of the goal's pauses. Slice 8 (research R-05): re-checked first under the goal
    /// lock, <see cref="ExemptionStoreWrite.GoalEnded"/> when the goal's latest attempt is ended. Every refusal writes
    /// nothing.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="lastDay"/> is before <paramref name="firstDay"/>.</exception>
    Task<ExemptionStoreWrite> AddPauseAsync(
        AccountId owner, GoalId goal, DateOnly firstDay, DateOnly lastDay, ExemptionReason? reason,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gives the goal's pause <paramref name="pause"/> the last day <paramref name="lastDay"/> (FR-010).
    /// <see cref="ExemptionStoreWrite.PauseNotFound"/> when the pause is not among the goal's pauses: it is found by the
    /// goal and the identifier together, never by the identifier alone. Every refusal writes nothing.
    /// </summary>
    Task<ExemptionStoreWrite> SetPauseLastDayAsync(
        AccountId owner, GoalId goal, PauseId pause, DateOnly lastDay, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the goal's pause <paramref name="pause"/> (FR-010, FR-011). <see cref="ExemptionStoreWrite.PauseNotFound"/>
    /// when the pause is not among the goal's pauses. Every refusal writes nothing.
    /// </summary>
    Task<ExemptionStoreWrite> DeletePauseAsync(AccountId owner, GoalId goal, PauseId pause, CancellationToken cancellationToken);
}
