using HT.Domain.Evaluation;
using HT.Domain.Goals;

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

    /// <summary>
    /// Slice 8 (FR-016, FR-019): the number of the attempt the entry belongs to; 1 when it is not set, as for every
    /// entry of slices 5 to 7.
    /// </summary>
    public int AttemptNumber { get; init; } = GoalAttempt.First;

    /// <summary>Slice 8 (FR-016, FR-019): whether the entry belongs to the goal's latest attempt; true when it is not set.</summary>
    public bool IsLatestAttempt { get; init; } = true;

    /// <summary>
    /// Slice 8 (FR-004, FR-016, FR-019): the name of the entry's attempt (<c>GoalText.AttemptName</c>), set only for an
    /// attempt that is not the latest; null otherwise.
    /// </summary>
    public string? AttemptName { get; init; }

    /// <summary>
    /// Slice 8 (research R-07): the settings of the version of the entry's date. Always set by the use cases; null on an
    /// entry built without it, for which <see cref="GoalHistory.SettingsOf"/> gives the history's goal-level settings.
    /// Readers call <see cref="GoalHistory.SettingsOf"/>, never this property.
    /// </summary>
    public EntrySettings? Settings { get; init; }
}
