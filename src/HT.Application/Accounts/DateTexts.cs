using System.Globalization;

namespace HT.Application.Accounts;

/// <summary>
/// The one formatter of dates on every page and in every text (research R-11). Day and month names come from fixed
/// English arrays, never from a culture, so a date reads the same whatever the server's or the browser's culture
/// (constitution II, AC-088). Numbers are written with the invariant digits.
/// </summary>
public static class DateTexts
{
    /// <summary>Indexed by <see cref="DayOfWeek"/>: Sunday is 0.</summary>
    private static readonly string[] Weekdays =
        ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];

    /// <summary>Indexed by <see cref="DayOfWeek"/>: Sunday is 0.</summary>
    private static readonly string[] ShortWeekdays = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];

    /// <summary>Indexed by month − 1.</summary>
    private static readonly string[] Months =
    [
        "January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December",
    ];

    /// <summary>Indexed by month − 1.</summary>
    private static readonly string[] ShortMonths =
        ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

    /// <summary>The weekday's name: "Tuesday".</summary>
    public static string Weekday(DateOnly date) => Weekdays[(int)date.DayOfWeek];

    /// <summary>The Home date (FR-029): "Tuesday, 17 November".</summary>
    public static string LongDate(DateOnly date) => Weekday(date) + ", " + Day(date) + " " + Months[date.Month - 1];

    /// <summary>A short date with its weekday: "Tue 17 Nov".</summary>
    public static string ShortDate(DateOnly date) => ShortWeekdays[(int)date.DayOfWeek] + " " + DayMonth(date);

    /// <summary>A day and a short month: "23 Nov".</summary>
    public static string DayMonth(DateOnly date) => Day(date) + " " + ShortMonths[date.Month - 1];

    /// <summary>A day, a short month and the year: "14 Jul 2026" (the Settings card's "Started" row, HP-004).</summary>
    public static string DayMonthYear(DateOnly date) => DayMonth(date) + " " + Year(date);

    /// <summary>A month and its year: "November 2026" ("member since", FR-030).</summary>
    public static string MonthYear(DateOnly date) => Months[date.Month - 1] + " " + Year(date);

    /// <summary>
    /// A week's dates, with an en dash between spaces (slice 5, a per-week entry): "10 – 16 Aug" within a month,
    /// "28 Sep – 4 Oct" across a month end and "28 Dec – 3 Jan" across a year end. No year is written.
    /// </summary>
    public static string WeekRange(DateOnly first, DateOnly last) =>
        (first.Year == last.Year && first.Month == last.Month ? Day(first) : DayMonth(first)) + " – " + DayMonth(last);

    private static string Day(DateOnly date) => date.Day.ToString(CultureInfo.InvariantCulture);

    private static string Year(DateOnly date) => date.Year.ToString("D4", CultureInfo.InvariantCulture);
}
