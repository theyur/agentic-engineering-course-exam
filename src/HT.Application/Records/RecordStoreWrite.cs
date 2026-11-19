using HT.Domain.Records;

namespace HT.Application.Records;

/// <summary>The kinds of <see cref="RecordStoreWrite"/>.</summary>
public enum RecordStoreWriteOutcome
{
    GoalNotFound,
    RecordNotFound,
    LimitReached,
    Written,
}

/// <summary>
/// The outcome of a write of <see cref="IRecordStore"/> (research R-05): the store's outcome, not a use-case outcome.
/// <see cref="GoalNotFound"/> when the goal is not one of the owner's; <see cref="RecordNotFound"/> when the goal has
/// no such record; <see cref="LimitReached"/> when a period already has the limit's records. None of them writes
/// anything. A <see cref="RecordStoreWriteOutcome.Written"/> carries the entry as it is after the write.
/// </summary>
public sealed class RecordStoreWrite
{
    private RecordStoreWrite(RecordStoreWriteOutcome outcome, RecordId? recordId, StoredEntry? entry)
    {
        Outcome = outcome;
        RecordId = recordId;
        Entry = entry;
    }

    /// <summary>The goal is not one of the owner's: nothing was written.</summary>
    public static RecordStoreWrite GoalNotFound { get; } = new(RecordStoreWriteOutcome.GoalNotFound, null, null);

    /// <summary>The goal has no such record: nothing was written.</summary>
    public static RecordStoreWrite RecordNotFound { get; } = new(RecordStoreWriteOutcome.RecordNotFound, null, null);

    /// <summary>The period already has the limit's records: nothing was written.</summary>
    public static RecordStoreWrite LimitReached { get; } = new(RecordStoreWriteOutcome.LimitReached, null, null);

    public RecordStoreWriteOutcome Outcome { get; }

    /// <summary>
    /// The record written; set for a <see cref="RecordStoreWriteOutcome.Written"/> of <see cref="IRecordStore.AddAsync"/>
    /// and <see cref="IRecordStore.ChangeAsync"/>, null otherwise.
    /// </summary>
    public RecordId? RecordId { get; }

    /// <summary>The entry's records and note after the write; set only for <see cref="RecordStoreWriteOutcome.Written"/>.</summary>
    public StoredEntry? Entry { get; }

    /// <summary>A record was added or changed: its identifier and the entry after the write.</summary>
    public static RecordStoreWrite Written(RecordId recordId, StoredEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        return new RecordStoreWrite(RecordStoreWriteOutcome.Written, recordId, entry);
    }

    /// <summary>A record was deleted, or a note saved: the entry after the write.</summary>
    public static RecordStoreWrite Written(StoredEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        return new RecordStoreWrite(RecordStoreWriteOutcome.Written, null, entry);
    }
}
