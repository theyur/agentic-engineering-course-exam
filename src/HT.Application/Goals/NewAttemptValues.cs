using HT.Domain.Habits;

namespace HT.Application.Goals;

/// <summary>
/// The values a new attempt of an ended goal starts with (slice 8 FR-011, data-model.md "Results and views"): the target
/// fields of the ended attempt's last version, as <see cref="GoalStartingValues"/> gives target fields, the earliest
/// start date (the later of the account's today and the day after the ending date) and the number the attempt will get.
/// A stored target always has Build or Reduce, a measurement and a schedule kind, so those three are never null.
/// </summary>
public sealed record NewAttemptValues
{
    public required BuildOrReduce BuildOrReduce { get; init; }

    public required Measurement Measurement { get; init; }

    /// <summary>The amount for count and duration; null for yes/no.</summary>
    public int? Amount { get; init; }

    /// <summary>The count unit as stored; null otherwise.</summary>
    public string? CountUnit { get; init; }

    /// <summary>The duration unit; null otherwise.</summary>
    public DurationUnit? DurationUnit { get; init; }

    public required ScheduleKind ScheduleKind { get; init; }

    /// <summary>The scheduled days; empty when the schedule has none.</summary>
    public IReadOnlySet<DayOfWeek> Days { get; init; } = new HashSet<DayOfWeek>();

    /// <summary>N of an N-times schedule; null for fixed days.</summary>
    public int? Times { get; init; }

    /// <summary>The period of an N-times schedule; null for fixed days.</summary>
    public SchedulePeriod? Period { get; init; }

    /// <summary>The later of the account's today and the day after the ending date (FR-011).</summary>
    public required DateOnly StartDate { get; init; }

    /// <summary>The number the new attempt will get: the latest attempt's number + 1 (HP-004's line).</summary>
    public required int AttemptNumber { get; init; }
}
