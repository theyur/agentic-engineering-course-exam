using HT.Application.Records;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;

namespace HT.Application.Reports;

/// <summary>
/// One row of the Home's Today list (FR-012, research R-06): its goal, and the goal's settings and the entry as slice
/// 5's views, so the row's chip, progress and dialogs need no other read.
/// </summary>
public sealed record TodayEntry
{
    public required GoalId GoalId { get; init; }

    /// <summary>
    /// The own name, or the derived name (slice 4 FR-010). Slice 8 (FR-019): for an entry of an attempt that is not the
    /// latest, that attempt's name.
    /// </summary>
    public required string GoalName { get; init; }

    /// <summary>Slice 8 (FR-019): for an entry of an attempt that is not the latest, that attempt's Build or Reduce (<c>Goal.SettingsOf(Entry)</c>).</summary>
    public required BuildOrReduce BuildOrReduce { get; init; }

    /// <summary>
    /// Slice 4's <c>GoalText.ScheduleText</c> ("Mon · Wed · Fri"), the meta line of a Due row without a record. Slice 8
    /// (FR-019): for an entry of an attempt that is not the latest, the schedule of its own settings.
    /// </summary>
    public required string ScheduleText { get; init; }

    /// <summary>
    /// The goal's settings and its Due and Pending entries, as <see cref="RecordingService.GetRecordableAsync"/> gives them.
    /// Slice 8 (research R-07): the row and its dialogs read the entry's own settings through <c>Goal.SettingsOf(Entry)</c>.
    /// </summary>
    public required GoalHistory Goal { get; init; }

    /// <summary>The entry: Pending, Due, Missed or Upcoming. An Upcoming entry has no records and no grace end.</summary>
    public required HistoryEntry Entry { get; init; }

    /// <summary>The entry has at least one record.</summary>
    public bool HasRecord => Entry.Records.Count > 0;

    /// <summary>The entry has a note.</summary>
    public bool HasNote => Entry.Note is not null;

    /// <summary>
    /// Slice 8 (FR-019, HP-007): the entry's goal is ended, so the row shows the "Ended" Tag (unless the entry's attempt is
    /// not the latest, which shows "Attempt n"). A flag, not an outcome: it maps to no text.
    /// </summary>
    public bool GoalEnded { get; init; }
}

/// <summary>The Home's four counts (FR-012). Upcoming entries are not counted.</summary>
public sealed record DashboardCounts
{
    public required int Due { get; init; }

    public required int Pending { get; init; }

    /// <summary>The Missed entries whose grace period ended this week, up to the supplied time.</summary>
    public required int MissedThisWeek { get; init; }

    /// <summary>
    /// The number of the user's goals that are not paused (slice 7 FR-019: no <see cref="DashboardGoal.PausedUntil"/>)
    /// and, from slice 8 (FR-019), not ended.
    /// </summary>
    public required int Goals { get; init; }
}

/// <summary>One goal of the Home's "Active goals" card (FR-012): its name and its figures.</summary>
public sealed record DashboardGoal
{
    public required GoalId GoalId { get; init; }

    public required string Name { get; init; }

    public required GoalStatsView Stats { get; init; }

    /// <summary>
    /// Slice 7 (FR-019, research R-03): the last day of the goal's current pause at the pause date; null when the goal is
    /// not paused.
    /// </summary>
    public DateOnly? PausedUntil { get; init; }
}

/// <summary>One of the Home's recent notes (FR-012): its goal, its entry's dates, the note and the entry's state.</summary>
public sealed record DashboardNote
{
    public required GoalId GoalId { get; init; }

    public required string GoalName { get; init; }

    public required DateOnly FirstDate { get; init; }

    public required DateOnly LastDate { get; init; }

    public required string Note { get; init; }

    public required EvaluationState State { get; init; }
}

/// <summary>The signed-in Home's figures at the supplied time (FR-012, FR-013).</summary>
public sealed record Dashboard
{
    public required DashboardCounts Counts { get; init; }

    /// <summary>Pending, then Due, then Missed this week, then Upcoming (research R-06).</summary>
    public required IReadOnlyList<TodayEntry> Today { get; init; }

    /// <summary>Every goal that is not ended (slice 8 FR-019), in FR-011's order.</summary>
    public required IReadOnlyList<DashboardGoal> Goals { get; init; }

    /// <summary>At most 5, by first date descending, ties in goal order; none of an ended goal (slice 8 FR-019).</summary>
    public required IReadOnlyList<DashboardNote> Notes { get; init; }

    /// <summary>Today in the account's time zone.</summary>
    public required DateOnly AccountToday { get; init; }

    /// <summary>Monday 00:00 of this week in the account's time zone, as a UTC instant.</summary>
    public required DateTimeOffset WeekStarts { get; init; }

    /// <summary>The instant the use case read the clock.</summary>
    public required DateTimeOffset ReadAt { get; init; }
}
