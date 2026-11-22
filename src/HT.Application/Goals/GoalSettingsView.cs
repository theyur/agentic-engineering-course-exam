using HT.Domain.Habits;

namespace HT.Application.Goals;

/// <summary>
/// Slice 8 (FR-005, data-model.md "Changed views"): one settings version of a goal's latest attempt with its date: the
/// target fields of <see cref="GoalDetails"/>, its texts, its time zone and the date it applies from.
/// <see cref="GoalDetails.Later"/> gives the version after the settings in effect, so the Settings card shows "From
/// {date}" with its target text. It carries no goal or habit name.
/// </summary>
public sealed record GoalSettingsView
{
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

    /// <summary><c>GoalText.TargetText</c>: "20 min daily".</summary>
    public required string TargetText { get; init; }

    /// <summary><c>GoalText.ScheduleText</c>.</summary>
    public required string ScheduleText { get; init; }

    /// <summary>The version's time zone (an IANA identifier).</summary>
    public required string TimeZone { get; init; }

    /// <summary>The date the version applies from, on the goal's own timeline.</summary>
    public required DateOnly AppliesFrom { get; init; }
}
