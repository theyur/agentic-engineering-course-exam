using System.Globalization;

namespace HT.Domain.Habits;

/// <summary>
/// The amount, schedule and day-list parts of a target's text and kind (slice 2 FR-017), shared by
/// <see cref="SuggestedStartingGoalText"/> and the goal text (research R-05). Pure: the result depends only on the
/// values, never on a culture or a clock.
/// </summary>
internal static class TargetTextParts
{
    private static readonly DayOfWeek[] WeekOrder =
    [
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday,
        DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday,
    ];

    private static readonly string[] DayNames = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];

    /// <summary>
    /// The target text: the amount part and the schedule part, joined by a space for "daily" and "a day" and by
    /// ", " otherwise; yes/no is the schedule part alone; Reduce starts with "at most ".
    /// </summary>
    public static string Text(
        BuildOrReduce buildOrReduce, Measurement measurement, int? amount, string? countUnit, DurationUnit? durationUnit, Schedule schedule)
    {
        var yesNo = measurement == Measurement.YesNo;
        var schedulePart = SchedulePart(schedule, yesNo);
        var text = yesNo
            ? schedulePart
            : AmountPart(measurement, amount, countUnit, durationUnit) + (schedulePart is "daily" or "a day" ? " " : ", ") + schedulePart;
        return buildOrReduce == BuildOrReduce.Reduce ? "at most " + text : text;
    }

    /// <summary>Count "N unit"; duration "N min" or "N h".</summary>
    public static string AmountPart(Measurement measurement, int? amount, string? countUnit, DurationUnit? durationUnit)
    {
        var number = Number(amount ?? 0);
        return measurement == Measurement.Count
            ? number + " " + countUnit
            : number + (durationUnit == DurationUnit.Hours ? " h" : " min");
    }

    /// <summary>"daily", "once a week", "N days a week", "a day" ("once a day" for yes/no), "N times a day", "once a week", "N times a week".</summary>
    public static string SchedulePart(Schedule schedule, bool yesNo)
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

    /// <summary>"Yes/no", "Count" or "Duration", " · ", then "flexible" or "fixed days " and the days.</summary>
    public static string Kind(Measurement measurement, Schedule schedule)
    {
        var measurementPart = measurement switch
        {
            Measurement.YesNo => "Yes/no",
            Measurement.Count => "Count",
            _ => "Duration",
        };
        var schedulePart = schedule.Kind == ScheduleKind.Times ? "flexible" : "fixed days " + Days(schedule.Days);
        return measurementPart + " · " + schedulePart;
    }

    /// <summary>"every day" for all seven; otherwise the day groups, comma-separated ("Mon–Wed, Sat").</summary>
    public static string Days(IReadOnlySet<DayOfWeek> days) =>
        days.Count == 7 ? "every day" : string.Join(", ", DayGroups(days));

    /// <summary>
    /// The chosen days in week order, Monday first and not wrapping from Sunday to Monday: each run of three or more
    /// as "first–last", every other day on its own.
    /// </summary>
    public static IReadOnlyList<string> DayGroups(IReadOnlySet<DayOfWeek> days)
    {
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

        return parts;
    }

    /// <summary>A whole number in invariant digits.</summary>
    public static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);
}
