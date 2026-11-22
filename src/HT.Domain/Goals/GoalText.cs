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

    /// <summary>
    /// The amount part of the target text: for count "N unit" with the unit as stored ("20 pages", "1 sweet"), for
    /// duration "N min" or "N h"; null for yes/no, which has none. The New goal summary card's "Met when" uses it (HP-002).
    /// </summary>
    public static string? AmountText(GoalTarget target)
    {
        ArgumentNullException.ThrowIfNull(target);
        return target.Measurement == Measurement.YesNo
            ? null
            : TargetTextParts.AmountPart(target.Measurement, target.Amount, target.CountUnit, target.DurationUnit);
    }

    /// <summary>The kind: "Yes/no", "Count" or "Duration", " · ", then "flexible" or "fixed days " and the days ("Duration · fixed days Mon–Fri").</summary>
    public static string Kind(GoalTarget target)
    {
        ArgumentNullException.ThrowIfNull(target);
        return TargetTextParts.Kind(target.Measurement, target.Schedule);
    }

    /// <summary>
    /// The goal's name (FR-010): its own name, or the habit's current name, ", " and the target text of the latest
    /// attempt's first version, produced when read so that a renamed habit renames the goal. For every goal of slices 4
    /// to 7 that is its one target; a goal whose target changes is named at an instant by
    /// <see cref="Name(Goal, string, DateTimeOffset)"/> (slice 8 R-08).
    /// </summary>
    public static string Name(Goal goal, string habitName)
    {
        ArgumentNullException.ThrowIfNull(goal);
        ArgumentNullException.ThrowIfNull(habitName);
        return NameOf(goal, habitName, goal.Versions[0]);
    }

    /// <summary>
    /// The goal's name at <paramref name="instant"/> (slice 8 FR-004, research R-08): its own name, or the habit's
    /// current name, ", " and the target text of the settings in effect (<see cref="GoalTimeline.VersionInEffect(Goal, DateTimeOffset)"/>):
    /// the version of the current date, an ended goal's last version, or the first version of a latest attempt that has
    /// not begun.
    /// </summary>
    public static string Name(Goal goal, string habitName, DateTimeOffset instant)
    {
        ArgumentNullException.ThrowIfNull(goal);
        ArgumentNullException.ThrowIfNull(habitName);
        return NameOf(goal, habitName, GoalTimeline.VersionInEffect(goal, instant));
    }

    /// <summary>
    /// The name of attempt <paramref name="number"/> (slice 8 FR-004, research R-08): the goal's own name, or the
    /// habit's current name, ", " and the target text of that attempt's last version (the version of its last date, or
    /// its first when it has no date). Used for every attempt that is not the latest.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The goal has no attempt with that number.</exception>
    public static string AttemptName(Goal goal, int number, string habitName)
    {
        ArgumentNullException.ThrowIfNull(goal);
        ArgumentNullException.ThrowIfNull(habitName);
        return NameOf(goal, habitName, GoalTimeline.LastVersionOf(goal, number));
    }

    private static string NameOf(Goal goal, string habitName, GoalSettingsVersion version) =>
        goal.OwnName is { } own ? own.Value : habitName + ", " + TargetText(version.Settings.Target);

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
