using HT.Domain.Goals;
using HT.Domain.Habits;

namespace HT.Application.Goals;

/// <summary>
/// One goal on the Goals page (FR-014, FR-015, HP-003). Amount, unit and days are not list fields: the meta line uses
/// <see cref="TargetText"/> and <see cref="ScheduleText"/>.
/// </summary>
public sealed record GoalListItem
{
    public required GoalId Id { get; init; }

    /// <summary>The own name, or the derived name (FR-010).</summary>
    public required string Name { get; init; }

    /// <summary>The habit's current name.</summary>
    public required string HabitName { get; init; }

    public required HabitKind HabitKind { get; init; }

    public required BuildOrReduce BuildOrReduce { get; init; }

    public required Measurement Measurement { get; init; }

    /// <summary><c>GoalText.TargetText</c>: "15 min, 5 days a week".</summary>
    public required string TargetText { get; init; }

    /// <summary><c>GoalText.ScheduleText</c>: "Mon · Wed · Fri", "Every day", "Flexible · 3 times a week".</summary>
    public required string ScheduleText { get; init; }

    public required GoalScheduleKind ScheduleKind { get; init; }

    /// <summary>
    /// The start date, only while the goal has no current date. Slice 8 (AC-035): the latest attempt's start date while
    /// that attempt has not begun and the goal is not ended; null otherwise.
    /// </summary>
    public DateOnly? StartDate { get; init; }

    /// <summary>The next commitment's date; commitment-based goals only, and none for an ended goal.</summary>
    public DateOnly? NextCommitment { get; init; }

    /// <summary>The first week's Monday; per-week goals only, only while that week has not begun, and none for an ended goal.</summary>
    public DateOnly? FirstWeekStarts { get; init; }

    /// <summary>Slice 8 (FR-018): the latest attempt has ended.</summary>
    public bool IsEnded { get; init; }

    /// <summary>Slice 8 (FR-018): the ending date of an ended goal; null otherwise.</summary>
    public DateOnly? EndingDate { get; init; }

    /// <summary>Slice 8 (FR-018): how an ended goal ended; null otherwise.</summary>
    public Outcome? Outcome { get; init; }
}
