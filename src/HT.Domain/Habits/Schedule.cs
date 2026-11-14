using System.Collections.Frozen;

namespace HT.Domain.Habits;

/// <summary>
/// The schedule of a suggested starting goal: fixed days (a non-empty set of days) or N times (1 to 20) per day or
/// per week. Immutable; values of the other kind are empty.
/// </summary>
public sealed class Schedule
{
    public const int MinTimes = 1;
    public const int MaxTimes = 20;

    private Schedule(ScheduleKind kind, IReadOnlySet<DayOfWeek> days, int? times, SchedulePeriod? period)
    {
        Kind = kind;
        Days = days;
        Times = times;
        Period = period;
    }

    public ScheduleKind Kind { get; }

    /// <summary>The chosen days for <see cref="ScheduleKind.FixedDays"/>; empty for <see cref="ScheduleKind.Times"/>.</summary>
    public IReadOnlySet<DayOfWeek> Days { get; }

    /// <summary>The number of times for <see cref="ScheduleKind.Times"/>; null for fixed days.</summary>
    public int? Times { get; }

    /// <summary>The period for <see cref="ScheduleKind.Times"/>; null for fixed days.</summary>
    public SchedulePeriod? Period { get; }

    internal static Schedule FixedDays(IEnumerable<DayOfWeek> days) =>
        new(ScheduleKind.FixedDays, days.ToFrozenSet(), null, null);

    internal static Schedule TimesPer(int times, SchedulePeriod period) =>
        new(ScheduleKind.Times, FrozenSet<DayOfWeek>.Empty, times, period);
}
