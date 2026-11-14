using System.Globalization;

namespace HT.Domain.Habits;

/// <summary>
/// The one formatter of a suggested starting goal's text and kind (FR-017, ADR 0011, research R-06). Pure: the
/// result depends only on the goal, never on a culture or a clock.
/// </summary>
public static class SuggestedStartingGoalText
{
    private static readonly DayOfWeek[] WeekOrder =
    [
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday,
        DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday,
    ];

    private static readonly string[] DayNames = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];

    /// <summary>The text ("20 pages, 3 times a week") and the kind ("Count · flexible") of a suggested starting goal.</summary>
    public static (string Text, string Kind) Format(SuggestedStartingGoal goal)
    {
        ArgumentNullException.ThrowIfNull(goal);
        return (Text(goal), Kind(goal));
    }

    private static string Text(SuggestedStartingGoal goal)
    {
        var schedule = SchedulePart(goal.Schedule, goal.Measurement == Measurement.YesNo);
        var text = goal.Measurement == Measurement.YesNo
            ? schedule
            : AmountPart(goal) + (schedule is "daily" or "a day" ? " " : ", ") + schedule;
        return goal.BuildOrReduce == BuildOrReduce.Reduce ? "at most " + text : text;
    }

    private static string AmountPart(SuggestedStartingGoal goal)
    {
        var amount = Number(goal.Amount ?? 0);
        return goal.Measurement == Measurement.Count
            ? amount + " " + goal.CountUnit
            : amount + (goal.DurationUnit == DurationUnit.Hours ? " h" : " min");
    }

    private static string SchedulePart(Schedule schedule, bool yesNo)
    {
        if (schedule.Kind == ScheduleKind.FixedDays)
        {
            return schedule.Days.Count switch
            {
                7 => "daily",
                1 => "once a week",
                var n => Number(n) + " days a week",
            };
        }

        var times = schedule.Times ?? 0;
        if (schedule.Period == SchedulePeriod.Day)
        {
            if (times == 1)
            {
                return yesNo ? "once a day" : "a day";
            }

            return Number(times) + " times a day";
        }

        return times == 1 ? "once a week" : Number(times) + " times a week";
    }

    private static string Kind(SuggestedStartingGoal goal)
    {
        var measurement = goal.Measurement switch
        {
            Measurement.YesNo => "Yes/no",
            Measurement.Count => "Count",
            _ => "Duration",
        };
        var schedule = goal.Schedule.Kind == ScheduleKind.Times ? "flexible" : "fixed days " + Days(goal.Schedule.Days);
        return measurement + " · " + schedule;
    }

    /// <summary>Monday first, no wrap from Sunday to Monday; a run of three or more is "first–last".</summary>
    private static string Days(IReadOnlySet<DayOfWeek> days)
    {
        if (days.Count == 7)
        {
            return "every day";
        }

        var chosen = WeekOrder.Select(days.Contains).ToArray();
        var parts = new List<string>();
        var i = 0;
        while (i < chosen.Length)
        {
            if (!chosen[i])
            {
                i++;
                continue;
            }

            var end = i;
            while (end + 1 < chosen.Length && chosen[end + 1])
            {
                end++;
            }

            if (end - i >= 2)
            {
                parts.Add(DayNames[i] + "–" + DayNames[end]);
            }
            else
            {
                for (var day = i; day <= end; day++)
                {
                    parts.Add(DayNames[day]);
                }
            }

            i = end + 1;
        }

        return string.Join(", ", parts);
    }

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);
}
