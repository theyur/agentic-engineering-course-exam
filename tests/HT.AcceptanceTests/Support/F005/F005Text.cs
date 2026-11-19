using System.Globalization;
using System.Text.RegularExpressions;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;

namespace HT.AcceptanceTests.Support.F005;

/// <summary>
/// Reads the phrases of the F005 Gherkin as acceptance.md's conventions define them: dates, instants, settings,
/// record values and note texts (<c>`""`</c> is the empty text, <c>`"   "`</c> three spaces, "a text of N
/// characters" is <c>x</c> repeated N times, "a padded text of 500 characters" is two spaces, 500 <c>x</c> and two
/// spaces).
/// </summary>
public static class F005Text
{
    public const string AdministratorPassword = "admin2026";
    public const string RegisteredUserPassword = "walking42";
    public const string DemoPassword = "example2026";
    public const string DefaultIcon = "book-open";
    public const string AnonymousVisitor = "the anonymous visitor";

    /// <summary>A date as the Gherkin writes it.</summary>
    public const string D = @"\d{4}-\d{2}-\d{2}";

    /// <summary>The goals demo history loading creates, with the schedule kind AC-051 gives for each.</summary>
    public static readonly IReadOnlyDictionary<string, GoalScheduleKind> DemoGoalKinds = new Dictionary<string, GoalScheduleKind>
    {
        ["Reading, 20 min daily"] = GoalScheduleKind.CommitmentBased,
        ["Sugar, at most 1 sweet a day"] = GoalScheduleKind.PeriodBased,
        ["Yoga, 3 times a week"] = GoalScheduleKind.PeriodBased,
    };

    private static readonly Regex SettingsPattern = new(
        @"^(Build|Reduce), (yes/no|count (\d+) `([^`]+)`|duration (\d+) minutes|duration (\d+) in hours), (?:fixed days (.+)|(\d+) times? per (day|week))$",
        RegexOptions.CultureInvariant);

    public static string Email(string token) => token.Trim().Trim('`');

    public static bool IsDemoEmail(string email) => Regex.IsMatch(email, @"^demo[1-4]@example\.com$");

    public static DateOnly Date(string text) =>
        DateOnly.ParseExact(text.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture);

    public static DateTimeOffset Instant(string text) =>
        DateTimeOffset.Parse(
            text.Trim().Trim('`'),
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);

    public static int Int(string text) => int.Parse(text.Trim(), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);

    /// <summary>The dates written in a text, in order.</summary>
    public static List<DateOnly> Dates(string text) => Regex.Matches(text, D).Select(match => Date(match.Value)).ToList();

    /// <summary>The time of a captured "at `T` " prefix; null when the step gives no time.</summary>
    public static string? AtTime(string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
        {
            return null;
        }

        var match = Regex.Match(prefix, "^at `([^`]+)`");
        return match.Success ? match.Groups[1].Value : throw new InvalidOperationException($"Cannot read the time of \"{prefix}\".");
    }

    /// <summary>" in `Europe/Kyiv`", " in Europe/Kyiv" or " in UTC" (a table's UTC: no time zone at registration).</summary>
    public static string? ZoneOf(string clause)
    {
        if (string.IsNullOrWhiteSpace(clause))
        {
            return null;
        }

        var zone = clause.Trim();
        zone = zone.StartsWith("in ", StringComparison.Ordinal) ? zone["in ".Length..] : zone;
        zone = zone.Trim().Trim('`');
        return zone == "UTC" ? null : zone;
    }

    /// <summary>A backticked or quoted text: <c>`""`</c> is empty, <c>`"   "`</c> three spaces, <c>`abc`</c> is "abc".</summary>
    public static string TextOf(string content)
    {
        var value = content.Trim().Trim('`');
        return value.Length >= 2 && value[0] == '"' && value[^1] == '"' ? value[1..^1] : value;
    }

    /// <summary>A note as a step writes it.</summary>
    public static string NoteOf(string phrase)
    {
        var text = phrase.Trim();
        Match match;
        if ((match = Regex.Match(text, @"^a padded text of (\d+) characters$")).Success)
        {
            return "  " + new string('x', Int(match.Groups[1].Value)) + "  ";
        }

        if ((match = Regex.Match(text, @"^a text of (\d+) characters$")).Success)
        {
            return new string('x', Int(match.Groups[1].Value));
        }

        if ((match = Regex.Match(text, "^`\"(.*)\"`$")).Success)
        {
            return match.Groups[1].Value;
        }

        if ((match = Regex.Match(text, "^\"(.*)\"$")).Success)
        {
            return match.Groups[1].Value;
        }

        throw new InvalidOperationException($"Cannot read the note \"{phrase}\".");
    }

    /// <summary>A record value: Yes, No, a whole number, "no value", or both kinds ("Yes together with the amount 5").</summary>
    public static (YesNoAnswer? Answer, int? Amount) ValueOf(string phrase)
    {
        var text = phrase.Trim();
        Match match;
        if (text == "no value")
        {
            return (null, null);
        }

        if ((match = Regex.Match(text, @"^(Yes|No) together with the amount (-?\d+)$")).Success)
        {
            return (Enum.Parse<YesNoAnswer>(match.Groups[1].Value), Int(match.Groups[2].Value));
        }

        if ((match = Regex.Match(text, @"^the amount (-?\d+) together with (Yes|No)$")).Success)
        {
            return (Enum.Parse<YesNoAnswer>(match.Groups[2].Value), Int(match.Groups[1].Value));
        }

        if (text is "Yes" or "No")
        {
            return (Enum.Parse<YesNoAnswer>(text), null);
        }

        if (Regex.IsMatch(text, @"^-?\d+$"))
        {
            return (null, Int(text));
        }

        throw new InvalidOperationException($"Cannot read the value \"{phrase}\".");
    }

    /// <summary>Goal settings such as "Build, duration 20 minutes, fixed days on all seven days".</summary>
    public static F005Settings Settings(string text)
    {
        var match = SettingsPattern.Match(text.Trim());
        if (!match.Success)
        {
            throw new InvalidOperationException($"Cannot read the settings \"{text}\".");
        }

        var settings = new F005Settings { BuildOrReduce = Enum.Parse<BuildOrReduce>(match.Groups[1].Value) };
        if (match.Groups[2].Value == "yes/no")
        {
            settings.Measurement = Measurement.YesNo;
        }
        else if (match.Groups[3].Success)
        {
            settings.Measurement = Measurement.Count;
            settings.Amount = Int(match.Groups[3].Value);
            settings.CountUnit = match.Groups[4].Value;
        }
        else if (match.Groups[5].Success)
        {
            settings.Measurement = Measurement.Duration;
            settings.Amount = Int(match.Groups[5].Value);
            settings.DurationUnit = DurationUnit.Minutes;
        }
        else
        {
            settings.Measurement = Measurement.Duration;
            settings.Amount = Int(match.Groups[6].Value);
            settings.DurationUnit = DurationUnit.Hours;
        }

        if (match.Groups[7].Success)
        {
            settings.ScheduleKind = ScheduleKind.FixedDays;
            settings.Days = Days(match.Groups[7].Value);
        }
        else
        {
            settings.ScheduleKind = ScheduleKind.Times;
            settings.Times = Int(match.Groups[8].Value);
            settings.Period = match.Groups[9].Value == "day" ? SchedulePeriod.Day : SchedulePeriod.Week;
        }

        return settings;
    }

    public static HashSet<DayOfWeek> Days(string text)
    {
        if (text.Trim() == "on all seven days")
        {
            return [.. Enum.GetValues<DayOfWeek>()];
        }

        var days = Regex.Matches(text, "Monday|Tuesday|Wednesday|Thursday|Friday|Saturday|Sunday")
            .Select(match => Enum.Parse<DayOfWeek>(match.Value))
            .ToHashSet();
        return days.Count > 0 ? days : throw new InvalidOperationException($"\"{text}\" names no day.");
    }

    /// <summary>
    /// The items of a list such as "A; B; and C" or, without semicolons, "A and B" where each item starts with a date.
    /// </summary>
    public static List<string> Items(string text)
    {
        var trimmed = text.Trim();
        var parts = Regex.Split(trimmed, ";(?: and)? ");
        if (parts.Length == 1)
        {
            parts = Regex.Split(trimmed, $" and (?={D})");
        }

        return parts.Select(part => part.Trim()).ToList();
    }
}

/// <summary>Goal settings as a step writes them.</summary>
public sealed class F005Settings
{
    public BuildOrReduce BuildOrReduce { get; set; }

    public Measurement Measurement { get; set; }

    public int? Amount { get; set; }

    public string? CountUnit { get; set; }

    public DurationUnit? DurationUnit { get; set; }

    public ScheduleKind ScheduleKind { get; set; }

    public HashSet<DayOfWeek>? Days { get; set; }

    public int? Times { get; set; }

    public SchedulePeriod? Period { get; set; }

    /// <summary>Fixed days make a commitment-based goal, N times per day or week a period-based one (slice 4).</summary>
    public GoalScheduleKind Kind =>
        ScheduleKind == ScheduleKind.FixedDays ? GoalScheduleKind.CommitmentBased : GoalScheduleKind.PeriodBased;

    public GoalTargetFields ToFields() => new()
    {
        BuildOrReduce = BuildOrReduce,
        Measurement = Measurement,
        Amount = Amount,
        CountUnit = CountUnit,
        DurationUnit = DurationUnit,
        ScheduleKind = ScheduleKind,
        Days = Days,
        Times = Times,
        Period = Period,
    };
}
