using HT.Domain.Evaluation;

namespace HT.Application.Records;

/// <summary>
/// One commitment or evaluation period of a goal's history (FR-009): its dates and instants, its state at the supplied
/// time (FR-010), its records, its progress (FR-014) and its note. Every instant is in UTC.
/// </summary>
public sealed record HistoryEntry
{
    /// <summary>The commitment's date, the day, or the week's Monday; also the key its note is filed under.</summary>
    public required DateOnly FirstDate { get; init; }

    /// <summary>Equal to <see cref="FirstDate"/> except for a week, where it is the Sunday.</summary>
    public required DateOnly LastDate { get; init; }

    public required DateTimeOffset Starts { get; init; }

    public required DateTimeOffset Ends { get; init; }

    public required EvaluationState State { get; init; }

    /// <summary>The instant its grace period ends (FR-016); set only while it is Due or Pending.</summary>
    public DateTimeOffset? GraceEnds { get; init; }

    /// <summary>Its records, in date order and then in the order they were stored (FR-009).</summary>
    public required IReadOnlyList<RecordView> Records { get; init; }

    public required Progress Progress { get; init; }

    /// <summary>Its note, private to the goal's owner (FR-007); null when it has none.</summary>
    public string? Note { get; init; }

    /// <summary>
    /// True when it is final at the supplied time, so a change now would be a correction (FR-006). False for an Excused
    /// or Paused entry, which is never final (slice 7 FR-017).
    /// </summary>
    public required bool IsCorrection { get; init; }

    /// <summary>
    /// Slice 7 (FR-017): the reason of an Excused commitment's excusal; null when it has none, and for any other state.
    /// Private to the goal's owner, like the note.
    /// </summary>
    public string? ExcusalReason { get; init; }

    /// <summary>
    /// Slice 7 (FR-004, FR-017): true for an Excused commitment whose grace period has not ended at the supplied time,
    /// so its excusal can still be removed; false for any other state.
    /// </summary>
    public bool CanRemoveExcusal { get; init; }
}
