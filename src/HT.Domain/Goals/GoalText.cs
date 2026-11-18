using HT.Domain.Habits;

namespace HT.Domain.Goals;

/// <summary>
/// The one formatter of a goal's target text, kind, derived name and schedule text (FR-010, slice 2 FR-017 with the
/// goal exception; research R-05). It shares its parts with <see cref="SuggestedStartingGoalText"/>. Pure: the result
/// depends only on its arguments, never on a culture or a clock.
/// </summary>
public static class GoalText
{
    private const string EveryDay = "Every day";

    /// <summary>
    /// The target text ("15 min, 5 days a week", "at most 1 sweet a day"). The one exception to the suggestion's text:
    /// Reduce, yes/no, fixed days gives "avoided on " and the days as <see cref="Kind"/> writes them ("avoided on
    /// Mon–Fri"), or "avoided every day" for all seven days.
    /// </summary>
    public static string TargetText(GoalTarget target)
    {
        ArgumentNullException.ThrowIfNull(target);
        if (target is { BuildOrReduce: BuildOrReduce.Reduce, Measurement: Measurement.YesNo, Schedule.Kind: ScheduleKind.FixedDays })
        {
            return target.Schedule.Days.Count == 7
                ? "avoided every day"
                : "avoided on " + TargetTextParts.Days(target.Schedule.Days);
        }

        return TargetTextParts.Text(
            target.BuildOrReduce, target.Measurement, target.Amount, target.CountUnit, target.DurationUnit, target.Schedule);
    }

    /// <summary>The kind: "Yes/no", "Count" or "Duration", " · ", then "flexible" or "fixed days " and the days ("Duration · fixed days Mon–Fri").</summary>
    public static string Kind(GoalTarget target)
    {
        ArgumentNullException.ThrowIfNull(target);
        return TargetTextParts.Kind(target.Measurement, target.Schedule);
    }

    /// <summary>
    /// The goal's name (FR-010): its own name, or the habit's current name, ", " and the target text, produced when
    /// read so that a renamed habit renames the goal.
    /// </summary>
    public static string Name(Goal goal, string habitName)
    {
        ArgumentNullException.ThrowIfNull(goal);
        ArgumentNullException.ThrowIfNull(habitName);
        return goal.OwnName is { } own ? own.Value : habitName + ", " + TargetText(goal.Target);
    }

    /// <summary>
    /// The schedule of the Goals page's meta line and the Settings card's "Schedule" row (HP-003, HP-004):
    /// <list type="bullet">
    ///   <item>fixed days: "Every day" for all seven, otherwise the day groups of <see cref="Kind"/> joined by " · " ("Mon · Wed · Fri", "Mon–Fri", "Mon–Wed · Sat");</item>
    ///   <item>N times per day: "Every day" for 1 time, otherwise "N times a day";</item>
    ///   <item>N times per week: "Flexible · once a week" or "Flexible · N times a week".</item>
    /// </list>
    /// </summary>
    public static string ScheduleText(GoalTarget target)
    {
        ArgumentNullException.ThrowIfNull(target);
        var schedule = target.Schedule;
        if (schedule.Kind == ScheduleKind.FixedDays)
        {
            return schedule.Days.Count == 7 ? EveryDay : string.Join(" · ", TargetTextParts.DayGroups(schedule.Days));
        }

        var times = schedule.Times ?? 0;
        if (schedule.Period == SchedulePeriod.Day)
        {
            return times == 1 ? EveryDay : TargetTextParts.Number(times) + " times a day";
        }

        return times == 1 ? "Flexible · once a week" : "Flexible · " + TargetTextParts.Number(times) + " times a week";
    }
}
