using System.Globalization;
using System.Text.RegularExpressions;
using HT.Application.Habits;
using HT.Domain.Goals;
using HT.Domain.Habits;

namespace HT.AcceptanceTests.Support.F004;

/// <summary>
/// Reads the phrases of the F004 Gherkin as acceptance.md's conventions define them: settings, days, dates,
/// instants, texts (`"   "` is three spaces, "a text of N characters" is <c>x</c> repeated N times, "a unit of N
/// characters" is <c>u</c> repeated N times) and the facts a Then step lists.
/// </summary>
public static partial class F004Text
{
    public const string AdministratorPassword = "admin2026";
    public const string RegisteredUserPassword = "walking42";
    public const string DemoPassword = "example2026";
    public const string DefaultIcon = "book-open";
    public const string AnonymousVisitor = "the anonymous visitor";

    public const string DatePattern = @"\d{4}-\d{2}-\d{2}";
    public const string DayPattern = "(?:Monday|Tuesday|Wednesday|Thursday|Friday|Saturday|Sunday)";
    public static readonly string DayListPattern = $"{DayPattern}(?:, {DayPattern})*(?: and {DayPattern})?";

    /// <summary>A backticked text; group 1 is its content.</summary>
    private const string Unit = "`([^`]*)`";

    /// <summary>Every clause ends at a separator, a closing parenthesis or the end of the phrase.</summary>
    private const string ClauseEnd = @"(?=$|, | and |: |\))";

    private static readonly Regex Separator = new(@"^(?:, and |, | and |: )", RegexOptions.CultureInvariant);

    public static readonly IReadOnlyList<string> DemoEmails = ["demo1@example.com", "demo2@example.com", "demo3@example.com"];

    // ------------------------------------------------------------ simple values

    /// <summary>The email of a token such as <c>`x@example.com`</c>.</summary>
    public static string Email(string token) => token.Trim().Trim('`');

    public static DateOnly Date(string text) =>
        DateOnly.ParseExact(text.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture);

    public static DateTimeOffset Instant(string text) =>
        DateTimeOffset.Parse(
            text.Trim().Trim('`'),
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);

    public static int Int(string text) => int.Parse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);

    /// <summary>
    /// The regex of an optional "at `T` " or "at `T` (…) " step prefix. Its one capture group always takes part in
    /// the match (Reqnroll passes one argument per group that took part); an empty capture means "no time given".
    /// </summary>
    public const string AtPrefix = @"((?:at `[^`]+`(?: \([^)]*\))? )?)";

    /// <summary>The time of a captured <see cref="AtPrefix"/>; null when the step gives no time.</summary>
    public static string? AtTime(string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
        {
            return null;
        }

        var match = Regex.Match(prefix, "^at `([^`]+)`");
        return match.Success
            ? match.Groups[1].Value
            : throw new InvalidOperationException($"Cannot read the time of \"{prefix}\".");
    }

    /// <summary>The content of the first backticked value in an optional captured clause; null when the clause is empty.</summary>
    public static string? BacktickedIn(string clause)
    {
        if (string.IsNullOrEmpty(clause))
        {
            return null;
        }

        var match = Regex.Match(clause, "`([^`]+)`");
        return match.Success
            ? match.Groups[1].Value
            : throw new InvalidOperationException($"\"{clause}\" has no backticked value.");
    }

    /// <summary>The content of a backticked value: <c>`"   "`</c> is three spaces, <c>`pages`</c> is "pages".</summary>
    public static string TextOf(string content)
    {
        var value = content.Trim('`');
        return value.Length >= 2 && value[0] == '"' && value[^1] == '"' ? value[1..^1] : value;
    }

    public static string TextOfLength(int length) => new('x', length);

    public static string UnitOfLength(int length) => new('u', length);

    public static HashSet<DayOfWeek> Days(string text)
    {
        var trimmed = text.Trim();
        if (trimmed == "on all seven days")
        {
            return [.. Enum.GetValues<DayOfWeek>()];
        }

        if (trimmed == "Monday to Friday")
        {
            return [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday];
        }

        var days = Regex.Matches(trimmed, DayPattern).Select(match => Enum.Parse<DayOfWeek>(match.Value)).ToHashSet();
        return days.Count > 0 ? days : throw new InvalidOperationException($"\"{text}\" names no day.");
    }

    public static SchedulePeriod Period(string text) => text == "day" ? SchedulePeriod.Day : SchedulePeriod.Week;

    public static DurationUnit DurationUnitOf(string text) => text == "minutes" ? DurationUnit.Minutes : DurationUnit.Hours;

    public static HabitKind HabitKindOf(string text) => text == "catalog" ? HabitKind.Catalog : HabitKind.Custom;

    public static GoalScheduleKind ScheduleKindOf(string text) =>
        text == "Commitment-based" ? GoalScheduleKind.CommitmentBased : GoalScheduleKind.PeriodBased;

    /// <summary>The quoted texts of a phrase such as <c>"Alpha", "alpha" and "beta"</c>, in order.</summary>
    public static List<string> QuotedTexts(string text) =>
        Regex.Matches(text, "\"([^\"]*)\"").Select(match => match.Groups[1].Value).ToList();

    // ------------------------------------------------------------ settings

    private static readonly (Regex Pattern, Action<SettingsSpec, Match> Apply)[] SettingsClauses =
    [
        (Clause("no Build or Reduce"), (s, _) => s.BuildOrReduce = null),
        (Clause("(Build|Reduce)"), (s, m) => s.BuildOrReduce = Enum.Parse<BuildOrReduce>(m.Groups[1].Value)),
        (Clause("no measurement"), (s, _) => s.Measurement = null),
        (Clause($@"yes/no(?: with the amount (-?\d+)| with the count unit {Unit})?"), (s, m) =>
        {
            s.Measurement = Measurement.YesNo;
            if (m.Groups[1].Success) s.Amount = Int(m.Groups[1].Value);
            if (m.Groups[2].Success) s.CountUnit = TextOf(m.Groups[2].Value);
        }),
        (Clause($@"count(?: (-?\d+))?(?: {Unit}| with no unit| with the unit {Unit}| with a unit of (\d+) characters| with no amount and the unit {Unit})?(?: with the duration unit (minutes|hours))?"), (s, m) =>
        {
            s.Measurement = Measurement.Count;
            if (m.Groups[1].Success) s.Amount = Int(m.Groups[1].Value);
            if (m.Groups[2].Success) s.CountUnit = TextOf(m.Groups[2].Value);
            if (m.Groups[3].Success) s.CountUnit = TextOf(m.Groups[3].Value);
            if (m.Groups[4].Success) s.CountUnit = UnitOfLength(Int(m.Groups[4].Value));
            if (m.Groups[5].Success) s.CountUnit = TextOf(m.Groups[5].Value);
            if (m.Groups[6].Success) s.DurationUnit = DurationUnitOf(m.Groups[6].Value);
        }),
        (Clause($@"duration(?: (-?\d+))?(?: (minutes|hours)| with no duration unit| with no amount and the unit (minutes|hours))?(?: with the count unit {Unit})?"), (s, m) =>
        {
            s.Measurement = Measurement.Duration;
            if (m.Groups[1].Success) s.Amount = Int(m.Groups[1].Value);
            if (m.Groups[2].Success) s.DurationUnit = DurationUnitOf(m.Groups[2].Value);
            if (m.Groups[3].Success) s.DurationUnit = DurationUnitOf(m.Groups[3].Value);
            if (m.Groups[4].Success) s.CountUnit = TextOf(m.Groups[4].Value);
        }),
        (Clause("no schedule"), (s, _) => s.ScheduleKind = null),
        (Clause("fixed days with no day"), (s, _) =>
        {
            s.ScheduleKind = ScheduleKind.FixedDays;
            s.Days = [];
        }),
        (Clause("fixed days (on all seven days|Monday to Friday)"), (s, m) =>
        {
            s.ScheduleKind = ScheduleKind.FixedDays;
            s.Days = Days(m.Groups[1].Value);
        }),
        (Clause($"fixed days ({DayListPattern})"), (s, m) =>
        {
            s.ScheduleKind = ScheduleKind.FixedDays;
            s.Days = Days(m.Groups[1].Value);
        }),
        (Clause("no number of times, per (day|week)"), (s, m) =>
        {
            s.ScheduleKind = ScheduleKind.Times;
            s.Period = Period(m.Groups[1].Value);
        }),
        (Clause(@"(-?\d+) times with no period"), (s, m) =>
        {
            s.ScheduleKind = ScheduleKind.Times;
            s.Times = Int(m.Groups[1].Value);
        }),
        (Clause(@"(-?\d+) times? per (day|week)"), (s, m) =>
        {
            s.ScheduleKind = ScheduleKind.Times;
            s.Times = Int(m.Groups[1].Value);
            s.Period = Period(m.Groups[2].Value);
        }),
        // Values that do not belong to the chosen schedule: sent, but they leave the schedule kind as it is.
        (Clause(@"with (\d+) times? per (day|week)"), (s, m) =>
        {
            s.Times = Int(m.Groups[1].Value);
            s.Period = Period(m.Groups[2].Value);
        }),
        (Clause($"with the fixed days? ({DayListPattern})"), (s, m) => s.Days = Days(m.Groups[1].Value)),
        (Clause($"the start date ({DatePattern})"), (s, m) =>
        {
            s.StartDateGiven = true;
            s.StartDate = Date(m.Groups[1].Value);
        }),
        (Clause("no start date"), (s, _) =>
        {
            s.StartDateGiven = true;
            s.StartDate = null;
        }),
        (Clause(@"the name two spaces, a text of (\d+) characters and two spaces"), (s, m) =>
            s.SetName("  " + TextOfLength(Int(m.Groups[1].Value)) + "  ")),
        (Clause(@"the name a text of (\d+) characters"), (s, m) => s.SetName(TextOfLength(Int(m.Groups[1].Value)))),
        (Clause(@"a name of (\d+) characters"), (s, m) => s.SetName(TextOfLength(Int(m.Groups[1].Value)))),
        (Clause("the name `([^`]*)`"), (s, m) => s.SetName(TextOf(m.Groups[1].Value))),
        (Clause("the name \"([^\"]*)\""), (s, m) => s.SetName(m.Groups[1].Value)),
        (Clause("no name"), (s, _) => s.SetName(null)),
    ];

    /// <summary>The settings of a phrase such as "Build, duration 15 minutes, fixed days Monday to Friday".</summary>
    public static SettingsSpec Settings(string text)
    {
        var spec = new SettingsSpec();
        ReadClauses(text, SettingsClauses, spec, "settings");
        return spec;
    }

    // ------------------------------------------------------------ facts of Then steps

    private static readonly (Regex Pattern, FactKind Kind)[] FactPatterns =
    [
        (Clause("the habit `([^`]+)` \\((catalog|custom)\\)"), FactKind.HabitWithKind),
        (Clause("(?:the )?habit(?: name)? `([^`]+)`"), FactKind.HabitName),
        (Clause("(?:the kind )?(catalog|custom)"), FactKind.Kind),
        (Clause("the category (Body|Mind|Health|Money|Home|Other)"), FactKind.Category),
        (Clause("that the habit is (not )?in the catalog"), FactKind.InCatalog),
        (Clause("the goal's identifier"), FactKind.GoalId),
        (Clause("the name is (not )?its own"), FactKind.NameIsOwn),
        (Clause("the name \"([^\"]*)\"( followed by that target text)?(?:, which is (not )?its own)?"), FactKind.Name),
        (Clause("no name of its own"), FactKind.NoOwnName),
        (Clause("no name"), FactKind.NoName),
        (Clause("no habit"), FactKind.NoHabit),
        (Clause("nothing else"), FactKind.NothingElse),
        (Clause("no measurement"), FactKind.NoMeasurement),
        (Clause("no amount and no unit"), FactKind.NoAmountAndNoUnit),
        (Clause("no amount"), FactKind.NoAmount),
        (Clause("no unit"), FactKind.NoUnit),
        (Clause("no schedule"), FactKind.NoSchedule),
        (Clause("no duration unit"), FactKind.NoDurationUnit),
        (Clause("no count unit"), FactKind.NoCountUnit),
        (Clause("no fixed days"), FactKind.NoFixedDays),
        (Clause("no number of times"), FactKind.NoTimes),
        (Clause("no catalog suggestion text"), FactKind.NoSuggestionText),
        (Clause("(?:with )?the catalog suggestion text \"([^\"]*)\""), FactKind.SuggestionText),
        (Clause("(Build|Reduce)"), FactKind.BuildOrReduce),
        (Clause(@"duration (\d+) (minutes|hours)"), FactKind.Duration),
        (Clause(@"count (\d+) `([^`]*)`"), FactKind.Count),
        (Clause("(yes/no|count|duration)"), FactKind.Measurement),
        (Clause("fixed days (on all seven days|Monday to Friday)"), FactKind.FixedDays),
        (Clause($"fixed days ({DayListPattern})"), FactKind.FixedDays),
        (Clause(@"(\d+) times? per (day|week)"), FactKind.Times),
        (Clause("(?:the schedule kind )?(Commitment-based|Period-based)"), FactKind.ScheduleKind),
        (Clause("(?:the )?target text \"([^\"]*)\""), FactKind.TargetText),
        (Clause($"(?:the )?start date ({DatePattern})"), FactKind.StartDate),
        (Clause("no start date"), FactKind.NoStartDate),
        (Clause($"first week starts ({DatePattern})"), FactKind.FirstWeekStarts),
        (Clause("no first week starts(?: date)?"), FactKind.NoFirstWeekStarts),
        (Clause($"next commitment ({DatePattern})"), FactKind.NextCommitment),
        (Clause("no next commitment"), FactKind.NoNextCommitment),
        (Clause($"the time zone `([^`]+)` from ({DatePattern})"), FactKind.TimeZone),
        (Clause($"a later version `([^`]+)` from ({DatePattern})"), FactKind.LaterVersion),
        (Clause("no later version"), FactKind.NoLaterVersion),
        (Clause($@"attempt (\d+), started on ({DatePattern})"), FactKind.Attempt),
    ];

    /// <summary>The facts of a phrase such as "Build, duration 15 minutes and the start date 2026-11-18".</summary>
    public static List<Fact> Facts(string text)
    {
        var facts = new List<Fact>();
        var rest = StripUnchanged(text.Trim());
        var first = true;
        while (rest.Length > 0)
        {
            if (!first)
            {
                var separator = Separator.Match(rest);
                if (!separator.Success)
                {
                    throw new InvalidOperationException($"Cannot read the facts \"{rest}\" of \"{text}\".");
                }

                rest = rest[separator.Length..];
            }

            first = false;
            var read = false;
            foreach (var (pattern, kind) in FactPatterns)
            {
                var match = pattern.Match(rest);
                if (match.Success)
                {
                    facts.Add(new Fact(kind, match));
                    rest = rest[match.Length..];
                    read = true;
                    break;
                }
            }

            if (!read)
            {
                throw new InvalidOperationException($"Cannot read the facts \"{rest}\" of \"{text}\".");
            }
        }

        return facts;
    }

    // ------------------------------------------------------------ catalog habits

    /// <summary>The catalog habits of a list such as "a Published `N` with the category Body and a Draft `M`".</summary>
    public static List<(CatalogHabitState State, string Name, string Clause)> HabitList(string text)
    {
        var starts = HabitStart().Matches(text);
        if (starts.Count == 0)
        {
            throw new InvalidOperationException($"\"{text}\" names no catalog habit.");
        }

        var habits = new List<(CatalogHabitState, string, string)>();
        for (var index = 0; index < starts.Count; index++)
        {
            var start = starts[index];
            var end = index + 1 < starts.Count ? starts[index + 1].Index : text.Length;
            var clause = text[(start.Index + start.Length)..end].Trim();
            habits.Add((Enum.Parse<CatalogHabitState>(start.Groups[1].Value), start.Groups[2].Value, clause));
        }

        return habits;
    }

    /// <summary>Applies a clause such as "with the intent Reduce and no suggested starting goal" to a catalog habit.</summary>
    public static void ApplyCatalogClause(CatalogSpec spec, string clause)
    {
        var rest = clause.Trim();
        if (rest.Length == 0)
        {
            return;
        }

        if (!rest.StartsWith("with ", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Cannot read the catalog habit clause \"{clause}\".");
        }

        ApplyCatalogFields(spec, rest["with ".Length..]);
    }

    private static void ApplyCatalogFields(CatalogSpec spec, string text)
    {
        Match match;
        if ((match = Regex.Match(text, "^the intent (Build|Reduce)(?: and (.+))?$")).Success)
        {
            spec.Intent = Enum.Parse<Intent>(match.Groups[1].Value);
            if (match.Groups[2].Success) ApplyCatalogFields(spec, match.Groups[2].Value);
        }
        else if ((match = Regex.Match(text, @"^the category (\w+)(?: and (.+))?$")).Success)
        {
            spec.Category = Enum.Parse<Category>(match.Groups[1].Value);
            if (match.Groups[2].Success) ApplyCatalogFields(spec, match.Groups[2].Value);
        }
        else if (text == "no suggested starting goal")
        {
            spec.Suggestions.Clear();
        }
        else if ((match = Regex.Match(text, "^the suggested starting goals (.+?)(?:, in this order)?$")).Success)
        {
            spec.Suggestions.Clear();
            spec.Suggestions.AddRange(SuggestionList(match.Groups[1].Value));
        }
        else if ((match = Regex.Match(text, "^the suggested starting goal (.+)$")).Success)
        {
            spec.Suggestions.Clear();
            spec.Suggestions.Add(Settings(match.Groups[1].Value).ToSuggestion(marked: true));
        }
        else
        {
            throw new InvalidOperationException($"Cannot read the catalog habit fields \"{text}\".");
        }
    }

    /// <summary>"1. S (marked); 2. S", in the order written.</summary>
    public static List<SuggestedStartingGoalInput> SuggestionList(string text)
    {
        var suggestions = new List<SuggestedStartingGoalInput>();
        foreach (var item in text.Split("; "))
        {
            var match = Regex.Match(item.Trim(), @"^\d+\. (.+?)( \(marked\))?$");
            if (!match.Success)
            {
                throw new InvalidOperationException($"Cannot read the suggested starting goal \"{item}\".");
            }

            suggestions.Add(Settings(match.Groups[1].Value).ToSuggestion(marked: match.Groups[2].Success));
        }

        return suggestions;
    }

    // ------------------------------------------------------------ calendar entries

    /// <summary>"2026-11-18 from `T1` to `T2`" or "2026-11-23 to 2026-11-29 from `T1` to `T2` (167 hours)".</summary>
    public static List<CalendarEntrySpec> CalendarEntries(string text) =>
        CalendarEntry().Matches(text).Select(match => new CalendarEntrySpec(
                Date(match.Groups[1].Value),
                match.Groups[2].Success ? Date(match.Groups[2].Value) : Date(match.Groups[1].Value),
                Instant(match.Groups[3].Value),
                Instant(match.Groups[4].Value),
                match.Groups[5].Success ? Int(match.Groups[5].Value) : null))
            .ToList();

    // ------------------------------------------------------------ helpers

    private static Regex Clause(string pattern) =>
        new("^(?:" + pattern + ")" + ClauseEnd, RegexOptions.CultureInvariant);

    private static string StripUnchanged(string text) =>
        text.EndsWith(", unchanged", StringComparison.Ordinal) ? text[..^", unchanged".Length] : text;

    private static void ReadClauses<T>(string text, (Regex Pattern, Action<T, Match> Apply)[] clauses, T target, string what)
    {
        var rest = text.Trim();
        var first = true;
        while (rest.Length > 0)
        {
            if (!first)
            {
                var separator = Separator.Match(rest);
                if (!separator.Success)
                {
                    throw new InvalidOperationException($"Cannot read the {what} \"{rest}\" of \"{text}\".");
                }

                rest = rest[separator.Length..];
            }

            first = false;
            var read = false;
            foreach (var (pattern, apply) in clauses)
            {
                var match = pattern.Match(rest);
                if (match.Success)
                {
                    apply(target, match);
                    rest = rest[match.Length..];
                    read = true;
                    break;
                }
            }

            if (!read)
            {
                throw new InvalidOperationException($"Cannot read the {what} \"{rest}\" of \"{text}\".");
            }
        }
    }

    [GeneratedRegex("(?:^|, | and )an? (Published|Draft|Unpublished) `([^`]+)`")]
    private static partial Regex HabitStart();

    [GeneratedRegex(@"(\d{4}-\d{2}-\d{2})(?: to (\d{4}-\d{2}-\d{2}))? from `([^`]+)` to `([^`]+)`(?: \((\d+) hours\))?")]
    private static partial Regex CalendarEntry();
}

/// <summary>One entry of a calendar as a step writes it.</summary>
public sealed record CalendarEntrySpec(DateOnly First, DateOnly Last, DateTimeOffset Starts, DateTimeOffset Ends, int? Hours);

public enum FactKind
{
    HabitWithKind,
    HabitName,
    Kind,
    Category,
    InCatalog,
    GoalId,
    NameIsOwn,
    Name,
    NoOwnName,
    NoName,
    NoHabit,
    NothingElse,
    NoMeasurement,
    NoAmountAndNoUnit,
    NoAmount,
    NoUnit,
    NoSchedule,
    NoDurationUnit,
    NoCountUnit,
    NoFixedDays,
    NoTimes,
    NoSuggestionText,
    SuggestionText,
    BuildOrReduce,
    Duration,
    Count,
    Measurement,
    FixedDays,
    Times,
    ScheduleKind,
    TargetText,
    StartDate,
    NoStartDate,
    FirstWeekStarts,
    NoFirstWeekStarts,
    NextCommitment,
    NoNextCommitment,
    TimeZone,
    LaterVersion,
    NoLaterVersion,
    Attempt,
}

/// <summary>One fact a Then step states, with the match it was read from.</summary>
public sealed record Fact(FactKind Kind, Match Match)
{
    public string Group(int index) => Match.Groups[index].Value;

    public bool Has(int index) => Match.Groups[index].Success;
}

/// <summary>Goal settings as a step writes them; null fields are the ones the step leaves empty.</summary>
public sealed class SettingsSpec
{
    public BuildOrReduce? BuildOrReduce { get; set; }

    public Measurement? Measurement { get; set; }

    public int? Amount { get; set; }

    public string? CountUnit { get; set; }

    public DurationUnit? DurationUnit { get; set; }

    public ScheduleKind? ScheduleKind { get; set; }

    public HashSet<DayOfWeek>? Days { get; set; }

    public int? Times { get; set; }

    public SchedulePeriod? Period { get; set; }

    /// <summary>The step gives a start date or says "no start date"; otherwise the start date is today.</summary>
    public bool StartDateGiven { get; set; }

    public DateOnly? StartDate { get; set; }

    /// <summary>The step gives a name or says "no name"; otherwise there is no name.</summary>
    public bool NameGiven { get; set; }

    public string? Name { get; set; }

    public void SetName(string? name)
    {
        NameGiven = true;
        Name = name;
    }

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

    public SuggestedStartingGoalInput ToSuggestion(bool marked) => new()
    {
        BuildOrReduce = BuildOrReduce,
        Measurement = Measurement,
        Amount = Amount,
        CountUnit = CountUnit,
        DurationUnit = DurationUnit,
        ScheduleKind = ScheduleKind,
        Days = Days ?? [],
        Times = Times,
        Period = Period,
        IsMarked = marked,
    };
}

/// <summary>The fields of a catalog habit a step created, as acceptance.md's defaults complete them.</summary>
public sealed class CatalogSpec(string name)
{
    public string Name { get; set; } = name;

    public Category Category { get; set; } = Category.Other;

    public Intent Intent { get; set; } = Intent.Build;

    public string Icon { get; set; } = F004Text.DefaultIcon;

    public List<SuggestedStartingGoalInput> Suggestions { get; } = [];

    public CatalogHabitInput ToInput() => new()
    {
        Name = Name,
        Description = null,
        Category = Category,
        Intent = Intent,
        Icon = Icon,
        SuggestedStartingGoals = Suggestions.ToList(),
    };
}

/// <summary>The fields of a custom habit a step created, as acceptance.md's defaults complete them.</summary>
public sealed class CustomSpec(string name)
{
    public string Name { get; set; } = name;

    public Category Category { get; set; } = Category.Other;

    public Intent Intent { get; set; } = Intent.Build;

    public string Icon { get; set; } = F004Text.DefaultIcon;

    public HabitInput ToInput() => new()
    {
        Name = Name,
        Description = null,
        Category = Category,
        Intent = Intent,
        Icon = Icon,
    };
}
