using HT.Domain.Goals;
using HT.Domain.Habits;

namespace HT.Application.Goals;

/// <summary>One goal on its page (FR-016, FR-018, FR-020, FR-034, HP-004).</summary>
public sealed record GoalDetails
{
    public required GoalId Id { get; init; }

    /// <summary>The own name, or the derived name (FR-010).</summary>
    public required string Name { get; init; }

    /// <summary>True when the goal has an own name.</summary>
    public required bool HasOwnName { get; init; }

    public required GoalHabitView Habit { get; init; }

    public required BuildOrReduce BuildOrReduce { get; init; }

    public required Measurement Measurement { get; init; }

    /// <summary>The amount for count and duration; null for yes/no.</summary>
    public int? Amount { get; init; }

    public string? CountUnit { get; init; }

    public DurationUnit? DurationUnit { get; init; }

    public required ScheduleKind ScheduleKind { get; init; }

    /// <summary>The scheduled days; empty for N times.</summary>
    public required IReadOnlySet<DayOfWeek> Days { get; init; }

    public int? Times { get; init; }

    public SchedulePeriod? Period { get; init; }

    public required GoalScheduleKind GoalScheduleKind { get; init; }

    /// <summary><c>GoalText.TargetText</c>.</summary>
    public required string TargetText { get; init; }

    /// <summary><c>GoalText.ScheduleText</c>, the Settings card's "Schedule" row.</summary>
    public required string ScheduleText { get; init; }

    /// <summary><c>GoalText.Kind</c>: "Duration · fixed days Mon–Fri".</summary>
    public required string Kind { get; init; }

    public required DateOnly StartDate { get; init; }

    /// <summary>
    /// True when the goal has a current date at the supplied time (FR-012): its start date's day has begun in the zone
    /// of its own version. The goal page says "started" or "starts" from it, without reading a clock (HP-004).
    /// </summary>
    public required bool HasStarted { get; init; }

    /// <summary>The first week's Monday; per-week goals only.</summary>
    public DateOnly? FirstWeekStarts { get; init; }

    /// <summary>The time zone of the current date's version, or of the first version when the goal has no current date.</summary>
    public required string TimeZone { get; init; }

    public required DateOnly TimeZoneAppliesFrom { get; init; }

    /// <summary>The time zone of the one version after <see cref="TimeZone"/>'s; null when there is none.</summary>
    public string? LaterTimeZone { get; init; }

    public DateOnly? LaterTimeZoneAppliesFrom { get; init; }

    public required int AttemptNumber { get; init; }

    public required DateOnly AttemptStartDate { get; init; }
}
