using HT.Domain.Goals;
using HT.Domain.Habits;

namespace HT.Application.Records;

/// <summary>
/// Slice 8 (research R-07): the settings of one entry, from the settings version of its date, with the meaning of
/// <see cref="GoalHistory"/>'s goal-level fields of the same names. Every page and component that shows or records one
/// entry reads them through <see cref="GoalHistory.SettingsOf"/> and the <see cref="EntrySettings"/> overloads of
/// <see cref="RecordingText"/>, <see cref="RecordPreview"/> and <c>ReportText</c>, never the goal-level fields: a change
/// within an attempt can switch the measurement or Build and Reduce, and an earlier attempt's entry can have another
/// schedule kind than the latest attempt. It carries no goal or habit name (HP-006).
/// </summary>
public sealed record EntrySettings
{
    public required GoalScheduleKind GoalScheduleKind { get; init; }

    /// <summary>The length of an evaluation period (day or week); null for a commitment-based entry.</summary>
    public SchedulePeriod? Period { get; init; }

    /// <summary>The entry's Build or Reduce; for an attempt that is not the latest, that attempt's (FR-016, FR-019).</summary>
    public required BuildOrReduce BuildOrReduce { get; init; }

    public required Measurement Measurement { get; init; }

    /// <summary>The amount for count and duration; null for yes/no.</summary>
    public int? Amount { get; init; }

    /// <summary>The count unit as stored; null otherwise.</summary>
    public string? CountUnit { get; init; }

    /// <summary>The duration unit; null otherwise.</summary>
    public DurationUnit? DurationUnit { get; init; }

    /// <summary>N of a period-based entry; null for a commitment-based one.</summary>
    public int? Times { get; init; }

    /// <summary>The target of the settings version of the entry's date.</summary>
    public required GoalTarget Target { get; init; }

    /// <summary>The settings of <paramref name="target"/>, each field as <see cref="GoalHistory"/> takes it from a target.</summary>
    internal static EntrySettings Of(GoalTarget target)
    {
        ArgumentNullException.ThrowIfNull(target);
        // Stryker disable once Conditional : a fixed-days schedule has no Times (null), so taking Schedule.Times for every target gives the same
        return new EntrySettings
        {
            GoalScheduleKind = target.ScheduleKindOf,
            Period = target.PeriodLength,
            BuildOrReduce = target.BuildOrReduce,
            Measurement = target.Measurement,
            Amount = target.Amount,
            CountUnit = target.CountUnit,
            DurationUnit = target.DurationUnit,
            Times = target.ScheduleKindOf == GoalScheduleKind.PeriodBased ? target.Schedule.Times : null,
            Target = target,
        };
    }
}
