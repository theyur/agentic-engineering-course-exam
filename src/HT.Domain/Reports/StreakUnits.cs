using HT.Domain.Goals;
using HT.Domain.Habits;

namespace HT.Domain.Reports;

/// <summary>The <see cref="StreakUnit"/> of a target (FR-004). An enum cannot hold the method (research R-01).</summary>
public static class StreakUnits
{
    /// <summary>
    /// <see cref="StreakUnit.Commitments"/> for a commitment-based target, <see cref="StreakUnit.Days"/> for a
    /// per-day period and <see cref="StreakUnit.Weeks"/> for a per-week period, for Build and Reduce alike.
    /// </summary>
    public static StreakUnit Of(GoalTarget target)
    {
        ArgumentNullException.ThrowIfNull(target);
        return target.PeriodLength switch
        {
            null => StreakUnit.Commitments,
            SchedulePeriod.Day => StreakUnit.Days,
            SchedulePeriod.Week => StreakUnit.Weeks,
            _ => throw new ArgumentOutOfRangeException(nameof(target), "The period is not defined."),
        };
    }
}
