using HT.Domain.Goals;
using HT.Domain.Habits;

namespace HT.Application.Records;

/// <summary>
/// A goal's History, or what can be recorded now (FR-009, FR-011): the goal-level settings, and the entries. It
/// carries no goal or habit name, so a text made from it never names one (HP-006). Not the *Goal history*, the
/// Profile's list of ended attempts (slice 8 FR-015).
/// <para>
/// Slice 8 (research R-07): the goal-level settings are those in effect for the attempt the History shows, for what is
/// about the goal as a whole (the page head, <see cref="CurrentDate"/>, <see cref="ReadAt"/>). The versions of a goal
/// can differ in target, measurement and Build or Reduce, and an earlier attempt's entry can have another schedule
/// kind, so every view of one entry reads that entry's settings through <see cref="SettingsOf"/>.
/// </para>
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
    /// The goal-level target: of the settings in effect for the attempt shown (slice 8, research R-07). The views of one
    /// entry take the entry's own target from <see cref="SettingsOf"/>.
    /// </summary>
    public required GoalTarget Target { get; init; }

    /// <summary>The instant the use case read the clock, so a page formats "N h left" without reading one.</summary>
    public required DateTimeOffset ReadAt { get; init; }

    /// <summary>In date order from <c>GetHistoryAsync</c>; the Due and Pending ones, newest first, from <c>GetRecordableAsync</c>.</summary>
    public required IReadOnlyList<HistoryEntry> Entries { get; init; }

    /// <summary>
    /// Slice 8 (research R-07): the settings of <paramref name="entry"/>, its own <see cref="HistoryEntry.Settings"/>, or
    /// this History's goal-level settings when it has none (an entry built without them behaves as in slice 7).
    /// </summary>
    public EntrySettings SettingsOf(HistoryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        return entry.Settings ?? GoalLevelSettings();
    }

    /// <summary>This History's goal-level settings as an <see cref="EntrySettings"/>, for the <see cref="GoalHistory"/> members of the formatters.</summary>
    internal EntrySettings GoalLevelSettings() =>
        new()
        {
            GoalScheduleKind = GoalScheduleKind,
            Period = Period,
            BuildOrReduce = BuildOrReduce,
            Measurement = Measurement,
            Amount = Amount,
            CountUnit = CountUnit,
            DurationUnit = DurationUnit,
            Times = Times,
            Target = Target,
        };
}
