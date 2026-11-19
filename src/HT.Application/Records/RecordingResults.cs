using HT.Domain.Records;

namespace HT.Application.Records;

/// <summary>
/// The outcomes of recording, changing and deleting a record, in FR-005's check order (contracts/application-api.md).
/// Each write use case gives a subset of them.
/// </summary>
public enum RecordWriteOutcome
{
    NotSignedIn,
    NotForAdministrators,
    NotFound,
    RecordNotFound,
    NothingScheduled,
    DayNotBegun,
    AnswerMissing,
    CountOutOfRange,
    DurationOutOfRange,
    NoteTooLong,
    RecordLimitReached,
    Recorded,
    Changed,
    Deleted,
}

/// <summary>
/// The result of recording, changing or deleting a record (FR-006): the entry after the write and whether the write was
/// a correction, or one refusal with its text. A refusal carries no input value, so it never echoes a note, a value or
/// a date (HP-006).
/// </summary>
public sealed class RecordWriteResult
{
    private RecordWriteResult(RecordWriteOutcome outcome, string? message, RecordId? recordId, HistoryEntry? entry, bool isCorrection)
    {
        Outcome = outcome;
        Message = message;
        RecordId = recordId;
        Entry = entry;
        IsCorrection = isCorrection;
    }

    public RecordWriteOutcome Outcome { get; }

    /// <summary>The refusal's text; null for <see cref="RecordWriteOutcome.Recorded"/>, <see cref="RecordWriteOutcome.Changed"/> and <see cref="RecordWriteOutcome.Deleted"/>.</summary>
    public string? Message { get; }

    /// <summary>The record written; set only for <see cref="RecordWriteOutcome.Recorded"/> and <see cref="RecordWriteOutcome.Changed"/>.</summary>
    public RecordId? RecordId { get; }

    /// <summary>The entry after the write, as the history gives it at the same time; null for a refusal.</summary>
    public HistoryEntry? Entry { get; }

    /// <summary>True when the entry was final at the supplied time (FR-006); false for a refusal.</summary>
    public bool IsCorrection { get; }

    internal static RecordWriteResult Refused(RecordWriteOutcome outcome, string message) => new(outcome, message, null, null, false);

    internal static RecordWriteResult Recorded(RecordId recordId, HistoryEntry entry, bool isCorrection) =>
        new(RecordWriteOutcome.Recorded, null, recordId, entry, isCorrection);

    internal static RecordWriteResult Changed(RecordId recordId, HistoryEntry entry, bool isCorrection) =>
        new(RecordWriteOutcome.Changed, null, recordId, entry, isCorrection);

    internal static RecordWriteResult Deleted(HistoryEntry entry, bool isCorrection) =>
        new(RecordWriteOutcome.Deleted, null, null, entry, isCorrection);
}

/// <summary>The outcomes of saving a note on its own, in FR-005 and FR-007's check order (contracts/application-api.md).</summary>
public enum NoteSaveOutcome
{
    NotSignedIn,
    NotForAdministrators,
    NotFound,
    NothingScheduled,
    DayNotBegun,
    NoteTooLong,
    Saved,
}

/// <summary>
/// The result of saving a note on its own (FR-007): the entry after the write, or one refusal with its text. A refusal
/// carries no input value (HP-006).
/// </summary>
public sealed class NoteSaveResult
{
    private NoteSaveResult(NoteSaveOutcome outcome, string? message, HistoryEntry? entry)
    {
        Outcome = outcome;
        Message = message;
        Entry = entry;
    }

    public NoteSaveOutcome Outcome { get; }

    /// <summary>The refusal's text; null for <see cref="NoteSaveOutcome.Saved"/>.</summary>
    public string? Message { get; }

    /// <summary>The entry after the write; set only for <see cref="NoteSaveOutcome.Saved"/>.</summary>
    public HistoryEntry? Entry { get; }

    internal static NoteSaveResult Refused(NoteSaveOutcome outcome, string message) => new(outcome, message, null);

    internal static NoteSaveResult Saved(HistoryEntry entry) => new(NoteSaveOutcome.Saved, null, entry);
}

/// <summary>The outcomes of reading a goal's history, in FR-018's check order (contracts/application-api.md).</summary>
public enum HistoryOutcome
{
    NotSignedIn,
    NotForAdministrators,
    NotFound,
    RangeOutOfBounds,
    Found,
}

/// <summary>
/// A goal's history for a range of dates (FR-009), or the refusal. An unknown identifier and another account's goal give
/// the same <see cref="HistoryOutcome.NotFound"/> (FR-017).
/// </summary>
public sealed class HistoryResult
{
    private HistoryResult(HistoryOutcome outcome, string? message, GoalHistory? history)
    {
        Outcome = outcome;
        Message = message;
        History = history;
    }

    public HistoryOutcome Outcome { get; }

    /// <summary>The refusal's text; null for <see cref="HistoryOutcome.Found"/>.</summary>
    public string? Message { get; }

    /// <summary>Set only for <see cref="HistoryOutcome.Found"/>; its entries are in date order.</summary>
    public GoalHistory? History { get; }

    internal static HistoryResult Refused(HistoryOutcome outcome, string message) => new(outcome, message, null);

    internal static HistoryResult Found(GoalHistory history) => new(HistoryOutcome.Found, null, history);
}

/// <summary>The outcomes of reading what can be recorded now, in FR-018's check order (contracts/application-api.md).</summary>
public enum RecordableOutcome
{
    NotSignedIn,
    NotForAdministrators,
    NotFound,
    Found,
}

/// <summary>
/// What can be recorded now (FR-011): the goal's Due and Pending entries, newest first, or the refusal.
/// </summary>
public sealed class RecordableResult
{
    private RecordableResult(RecordableOutcome outcome, string? message, GoalHistory? history)
    {
        Outcome = outcome;
        Message = message;
        History = history;
    }

    public RecordableOutcome Outcome { get; }

    /// <summary>The refusal's text; null for <see cref="RecordableOutcome.Found"/>.</summary>
    public string? Message { get; }

    /// <summary>Set only for <see cref="RecordableOutcome.Found"/>; its entries are the Due and Pending ones, newest first.</summary>
    public GoalHistory? History { get; }

    internal static RecordableResult Refused(RecordableOutcome outcome, string message) => new(outcome, message, null);

    internal static RecordableResult Found(GoalHistory history) => new(RecordableOutcome.Found, null, history);
}

/// <summary>The outcomes of reading a goal's recent notes, in FR-018's check order (contracts/application-api.md).</summary>
public enum RecentNotesOutcome
{
    NotSignedIn,
    NotForAdministrators,
    NotFound,
    Found,
}

/// <summary>A goal's recent notes (FR-008): at most 5, newest first, or the refusal.</summary>
public sealed class RecentNotesResult
{
    private RecentNotesResult(RecentNotesOutcome outcome, string? message, IReadOnlyList<RecentNote> notes)
    {
        Outcome = outcome;
        Message = message;
        Notes = notes;
    }

    public RecentNotesOutcome Outcome { get; }

    /// <summary>The refusal's text; null for <see cref="RecentNotesOutcome.Found"/>.</summary>
    public string? Message { get; }

    /// <summary>At most 5 notes, newest first; empty for a refusal.</summary>
    public IReadOnlyList<RecentNote> Notes { get; }

    internal static RecentNotesResult Refused(RecentNotesOutcome outcome, string message) => new(outcome, message, []);

    internal static RecentNotesResult Found(IReadOnlyList<RecentNote> notes) => new(RecentNotesOutcome.Found, null, notes);
}
