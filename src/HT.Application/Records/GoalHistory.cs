using HT.Domain.Goals;
using HT.Domain.Habits;

namespace HT.Application.Records;

/// <summary>
/// A goal's history, or what can be recorded now (FR-009, FR-011): the goal settings the pages need to show its
/// entries, and the entries. It carries no goal or habit name, so a text made from it never names one (HP-006).
/// </summary>
public sealed record GoalHistory
{
    public required GoalId GoalId { get; init; }

    public required GoalScheduleKind GoalScheduleKind { get; init; }

    /// <summary>The length of an evaluation period (day or week); null for a commitment-based goal.</summary>
    public SchedulePeriod? Period { get; init; }

    public required BuildOrReduce BuildOrReduce { get; init; }

    public required Measurement Measurement { get; init; }

    /// <summary>The amount for count and duration; null for yes/no.</summary>
    public int? Amount { get; init; }

    /// <summary>The count unit as stored; null otherwise.</summary>
    public string? CountUnit { get; init; }

    /// <summary>The duration unit; null otherwise.</summary>
    public DurationUnit? DurationUnit { get; init; }

    /// <summary>N of a period-based goal; null for a commitment-based one.</summary>
    public int? Times { get; init; }

    /// <summary>The goal's current date at the supplied time (slice 4 FR-012); null before the start date's day begins.</summary>
    public DateOnly? CurrentDate { get; init; }

    /// <summary>
    /// The target of the settings version of <see cref="CurrentDate"/>, or of the start date when there is none. Only
    /// <c>RecordPreview</c> and <c>RecordingText</c> read it. In this slice every version has the same target (FR-015).
    /// </summary>
    public required GoalTarget Target { get; init; }

    /// <summary>The instant the use case read the clock, so a page formats "N h left" without reading one.</summary>
    public required DateTimeOffset ReadAt { get; init; }

    /// <summary>In date order from <c>GetHistoryAsync</c>; the Due and Pending ones, newest first, from <c>GetRecordableAsync</c>.</summary>
    public required IReadOnlyList<HistoryEntry> Entries { get; init; }
}
