using System.Globalization;
using System.Text.RegularExpressions;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using HT.Domain.Reports;

namespace HT.AcceptanceTests.Support.F007;

/// <summary>
/// Reads the phrases of the F007 Gherkin as acceptance.md's conventions define them: dates, instants, settings,
/// record lists, results, trend bars, Today entries and recent notes.
/// </summary>
public static class F007Text
{
    public const string AdministratorPassword = "admin2026";
    public const string RegisteredUserPassword = "walking42";
    public const string DemoPassword = "example2026";
    public const string DefaultIcon = "book-open";
    public const string AnonymousVisitor = "the anonymous visitor";

    /// <summary>A date as the Gherkin writes it.</summary>
    public const string D = @"\d{4}-\d{2}-\d{2}";

    /// <summary>An instant as the Gherkin writes it (UTC).</summary>
    public const string T = @"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}Z";

    /// <summary>A date or a per-week period's first and last date ("2026-11-09 – 2026-11-15").</summary>
    public const string Span = @"\d{4}-\d{2}-\d{2}(?: – \d{4}-\d{2}-\d{2})?";

    private const string State = "Upcoming|Due|Pending|Met|Failed|Missed";

    private const string Value = "(?:`Yes`|`No`|Yes|No|none|nothing|-?\\d+)";

    private const string DateList = D + "(?:(?:, and |, | and )" + D + ")*";

    private static readonly Regex SettingsPattern = new(
        @"^(Build|Reduce), (yes/no|count (\d+) `([^`]+)`|duration (\d+) minutes|duration (\d+) in hours), (?:fixed days (.+)|(\d+) times? per (day|week))$",
        RegexOptions.CultureInvariant);

    private static readonly Regex Clause = new(
        "^(?<values>" + Value + "(?: and " + Value + ")*)(?<mult> twice| once| three times)? for (?:" +
        "each date from (?<from>" + D + ") to (?<to>" + D + ")(?: except (?<except>" + DateList + "))?" +
        "|each date of (?:(?<ofNone>no date)|(?<ofFrom>" + D + ") to (?<ofTo>" + D + "))" +
        "|(?<noDate>no date)" +
        "|(?<weekDays>the [A-Za-z, ]+?) of each week from the week of (?<wFrom>" + D + ") to the week of (?<wTo>" + D + ")" +
        "|every (?<everyDays>[A-Za-z, ]+?) from (?<eFrom>" + D + ") to (?<eTo>" + D + ")" +
        "|(?<list>" + DateList + "))",
        RegexOptions.CultureInvariant);

    private static readonly Regex ClauseSeparator = new("^(?:, and |, | and )", RegexOptions.CultureInvariant);

    private const string GoalSelector = "the `[^`]+` goal|`[^`]+`|the (?:first|second) goal";

    private static readonly Regex PrefixGoal = new(
        "(?:^,? ?|, and, )on (?<goal>the `[^`]+` goal|`[^`]+`), ",
        RegexOptions.CultureInvariant);

    private static readonly Regex SuffixSplit = new(
        "(?<= on (?:" + GoalSelector + "))(?:, and | and )",
        RegexOptions.CultureInvariant);

    private static readonly Regex SuffixGoal = new(
        "^(?<body>.+?) on (?<goal>" + GoalSelector + ")$",
        RegexOptions.CultureInvariant);

    // ------------------------------------------------------------ simple values

    public static DateOnly Date(string text) =>
        DateOnly.ParseExact(text.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture);

    public static DateTimeOffset Instant(string text) =>
        DateTimeOffset.Parse(
            text.Trim().Trim('`'),
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);

    public static int Int(string text) => int.Parse(text.Trim(), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);

    public static (DateOnly First, DateOnly Last) SpanOf(string text)
    {
        var dates = Regex.Matches(text, D).Select(match => Date(match.Value)).ToList();
        return dates.Count switch
        {
            1 => (dates[0], dates[0]),
            2 => (dates[0], dates[1]),
            _ => throw new InvalidOperationException($"\"{text}\" is not a date or a period."),
        };
    }

    public static EvaluationState StateOf(string text) => Enum.Parse<EvaluationState>(text.Trim());

    /// <summary>The backticked names in a text, in order.</summary>
    public static List<string> Backticked(string text) =>
        Regex.Matches(text, "`([^`]+)`").Select(match => match.Groups[1].Value).ToList();

    /// <summary>The quoted texts in a text, in order.</summary>
    public static List<string> Quoted(string text) =>
        Regex.Matches(text, "\"([^\"]+)\"").Select(match => match.Groups[1].Value).ToList();

    /// <summary>The unit word of a streak text ("commitments", "day", "weeks") as the report's unit.</summary>
    public static StreakUnit UnitOf(string word) => word switch
    {
        "commitment" or "commitments" => StreakUnit.Commitments,
        "day" or "days" => StreakUnit.Days,
        "week" or "weeks" => StreakUnit.Weeks,
        _ => throw new InvalidOperationException($"\"{word}\" is not a streak unit."),
    };

    // ------------------------------------------------------------ goal settings

    /// <summary>Goal settings such as "Build, duration 20 minutes, fixed days on all seven days", optionally with ", the name `M`".</summary>
    public static (GoalTargetFields Fields, string? Name) Settings(string text)
    {
        var trimmed = text.Trim();
        string? name = null;
        var named = Regex.Match(trimmed, "^(.+), the name `([^`]+)`$");
        if (named.Success)
        {
            trimmed = named.Groups[1].Value;
            name = named.Groups[2].Value;
        }

        var match = SettingsPattern.Match(trimmed);
        if (!match.Success)
        {
            throw new InvalidOperationException($"Cannot read the settings \"{text}\".");
        }

        Measurement measurement;
        int? amount = null;
        string? countUnit = null;
        DurationUnit? durationUnit = null;
        if (match.Groups[2].Value == "yes/no")
        {
            measurement = Measurement.YesNo;
        }
        else if (match.Groups[3].Success)
        {
            measurement = Measurement.Count;
            amount = Int(match.Groups[3].Value);
            countUnit = match.Groups[4].Value;
        }
        else if (match.Groups[5].Success)
        {
            measurement = Measurement.Duration;
            amount = Int(match.Groups[5].Value);
            durationUnit = DurationUnit.Minutes;
        }
        else
        {
            measurement = Measurement.Duration;
            amount = Int(match.Groups[6].Value);
            durationUnit = DurationUnit.Hours;
        }

        var fields = match.Groups[7].Success
            ? new GoalTargetFields
            {
                BuildOrReduce = Enum.Parse<BuildOrReduce>(match.Groups[1].Value),
                Measurement = measurement,
                Amount = amount,
                CountUnit = countUnit,
                DurationUnit = durationUnit,
                ScheduleKind = ScheduleKind.FixedDays,
                Days = Days(match.Groups[7].Value),
                Times = null,
                Period = null,
            }
            : new GoalTargetFields
            {
                BuildOrReduce = Enum.Parse<BuildOrReduce>(match.Groups[1].Value),
                Measurement = measurement,
                Amount = amount,
                CountUnit = countUnit,
                DurationUnit = durationUnit,
                ScheduleKind = ScheduleKind.Times,
                Days = null,
                Times = Int(match.Groups[8].Value),
                Period = match.Groups[9].Value == "day" ? SchedulePeriod.Day : SchedulePeriod.Week,
            };
        return (fields, name);
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

    // ------------------------------------------------------------ records and notes

    /// <summary>One record a step names: the goal selector (none: the step's only goal), the value and the date.</summary>
    public sealed record RecordItem(string? Goal, YesNoAnswer? Answer, int? Amount, DateOnly Date);

    /// <summary>One note a step names: the goal selector (none: the step's only goal), the note and the date.</summary>
    public sealed record NoteItem(string? Goal, string Note, DateOnly Date);

    /// <summary>
    /// The records of a recording phrase: "20 for each date from D1 to D2 except D3 and D4", "12 for D", "V1 for D1,
    /// V2 for D2 and V3 for D3" ("none" and "nothing" record nothing), "`Yes` twice for D", "12 and 8 for D", "`Yes`
    /// for D1, D2 and D3", "`Yes` for the Monday, the Tuesday and the Wednesday of each week from the week of D1 to
    /// the week of D2", "`Yes` for every Monday, Wednesday and Friday from D1 to D2", "each date of no date", per goal
    /// ("… on the `Reading` goal, and … on the `Sugar` goal", ", on the `Reading` goal, … and, on the `Yoga` goal, …",
    /// "… on the first goal and nothing on the second goal").
    /// </summary>
    public static List<RecordItem> Records(string body)
    {
        var text = Regex.Replace(body, ", where \"none\" records nothing$", string.Empty);
        var items = new List<RecordItem>();
        foreach (var (goal, segment) in Segments(text))
        {
            var rest = segment.Trim();
            if (rest is "nothing" or "")
            {
                continue;
            }

            var first = true;
            while (rest.Length > 0)
            {
                if (!first)
                {
                    var separator = ClauseSeparator.Match(rest);
                    if (!separator.Success)
                    {
                        throw new InvalidOperationException($"Cannot read the records \"{body}\" at \"{rest}\".");
                    }

                    rest = rest[separator.Length..];
                }

                first = false;
                var clause = Clause.Match(rest);
                if (!clause.Success)
                {
                    throw new InvalidOperationException($"Cannot read the records \"{body}\" at \"{rest}\".");
                }

                rest = rest[clause.Length..];
                var values = Regex.Matches(clause.Groups["values"].Value, Value).Select(match => match.Value).ToList();
                var times = clause.Groups["mult"].Value.Trim() switch
                {
                    "" or "once" => 1,
                    "twice" => 2,
                    "three times" => 3,
                    var other => throw new InvalidOperationException($"\"{other}\" is not a number of times."),
                };

                foreach (var date in ClauseDates(clause))
                {
                    foreach (var value in values)
                    {
                        if (value is "none" or "nothing")
                        {
                            continue;
                        }

                        var (answer, amount) = ValueOf(value);
                        for (var time = 0; time < times; time++)
                        {
                            items.Add(new RecordItem(goal, answer, amount, date));
                        }
                    }
                }
            }
        }

        return items;
    }

    /// <summary>The notes of a save-note phrase: "the note "N" for D on the `G` goal" or ", on the `G` goal, the note "A" for D1, "B" for D2 …".</summary>
    public static List<NoteItem> Notes(string body)
    {
        var items = new List<NoteItem>();
        foreach (var (goal, segment) in Segments(body))
        {
            var notes = Regex.Matches(segment, "\"([^\"]*)\" for (" + D + ")");
            if (notes.Count == 0)
            {
                throw new InvalidOperationException($"Cannot read the notes \"{body}\".");
            }

            items.AddRange(notes.Select(note => new NoteItem(goal, note.Groups[1].Value, Date(note.Groups[2].Value))));
        }

        return items;
    }

    public static (YesNoAnswer? Answer, int? Amount) ValueOf(string phrase)
    {
        var text = phrase.Trim().Trim('`');
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

    private static List<(string? Goal, string Text)> Segments(string body)
    {
        var text = body.TrimEnd();
        var prefix = PrefixGoal.Matches(text);
        if (prefix.Count > 0 && prefix[0].Index == 0)
        {
            var segments = new List<(string? Goal, string Text)>();
            for (var index = 0; index < prefix.Count; index++)
            {
                var start = prefix[index].Index + prefix[index].Length;
                var end = index + 1 < prefix.Count ? prefix[index + 1].Index : text.Length;
                segments.Add((prefix[index].Groups["goal"].Value, text[start..end]));
            }

            return segments;
        }

        return SuffixSplit.Split(text.Trim())
            .Select(part =>
            {
                var match = SuffixGoal.Match(part.Trim());
                return match.Success
                    ? ((string?)match.Groups["goal"].Value, match.Groups["body"].Value)
                    : ((string?)null, part.Trim());
            })
            .ToList();
    }

    private static IEnumerable<DateOnly> ClauseDates(Match clause)
    {
        if (clause.Groups["from"].Success)
        {
            var except = clause.Groups["except"].Success ? Dates(clause.Groups["except"].Value).ToHashSet() : [];
            return Range(Date(clause.Groups["from"].Value), Date(clause.Groups["to"].Value)).Where(date => !except.Contains(date));
        }

        if (clause.Groups["ofNone"].Success || clause.Groups["noDate"].Success)
        {
            return [];
        }

        if (clause.Groups["ofFrom"].Success)
        {
            return Range(Date(clause.Groups["ofFrom"].Value), Date(clause.Groups["ofTo"].Value));
        }

        if (clause.Groups["weekDays"].Success)
        {
            var days = Days(clause.Groups["weekDays"].Value);
            var first = MondayOf(Date(clause.Groups["wFrom"].Value));
            var last = MondayOf(Date(clause.Groups["wTo"].Value));
            var dates = new List<DateOnly>();
            for (var monday = first; monday <= last; monday = monday.AddDays(7))
            {
                dates.AddRange(Range(monday, monday.AddDays(6)).Where(date => days.Contains(date.DayOfWeek)));
            }

            return dates;
        }

        if (clause.Groups["everyDays"].Success)
        {
            var days = Days(clause.Groups["everyDays"].Value);
            return Range(Date(clause.Groups["eFrom"].Value), Date(clause.Groups["eTo"].Value)).Where(date => days.Contains(date.DayOfWeek));
        }

        return Dates(clause.Groups["list"].Value);
    }

    public static List<DateOnly> Dates(string text) => Regex.Matches(text, D).Select(match => Date(match.Value)).ToList();

    public static List<DateOnly> Range(DateOnly first, DateOnly last)
    {
        var dates = new List<DateOnly>();
        for (var date = first; date <= last; date = date.AddDays(1))
        {
            dates.Add(date);
        }

        return dates;
    }

    private static DateOnly MondayOf(DateOnly date) => date.AddDays(-(((int)date.DayOfWeek + 6) % 7));

    // ------------------------------------------------------------ results, bars, entries and notes

    /// <summary>A result as an item writes it: its date or period, its state, and as far as given its progress and "no record".</summary>
    public sealed record ResultSpec(string Text, DateOnly First, DateOnly Last, EvaluationState State, int? ProgressValue, int? ProgressOf, bool NoRecord);

    /// <summary>A trend bar "(D, V)", "(D – D, V)", "(D, no value)" or "(D – D, V, target T)".</summary>
    public sealed record BarSpec(string Text, DateOnly First, DateOnly Last, int? Value, int? Target);

    /// <summary>A Today entry "(goal, [Build|Reduce, ]date, state, …)".</summary>
    public sealed record EntrySpec(
        string Text,
        string Goal,
        BuildOrReduce? Kind,
        DateOnly First,
        DateOnly Last,
        EvaluationState State,
        int? ProgressValue,
        int? ProgressOf,
        DateTimeOffset? GraceEnds,
        bool? HasRecord,
        bool? HasNote);

    /// <summary>A recent note "(goal, date, note, state)".</summary>
    public sealed record NoteSpec(string Text, string GoalName, DateOnly First, DateOnly Last, string Note, EvaluationState State);

    public static ResultSpec Result(string text)
    {
        var match = Regex.Match(text.Trim(), "^(?<span>" + Span + ") (?<state>" + State + ")(?<rest>(?:, .+)?)$");
        if (!match.Success)
        {
            throw new InvalidOperationException($"Cannot read the result \"{text}\".");
        }

        var (first, last) = SpanOf(match.Groups["span"].Value);
        int? value = null;
        int? of = null;
        var noRecord = false;
        foreach (var fact in Facts(match.Groups["rest"].Value))
        {
            Match progress;
            if ((progress = Regex.Match(fact, @"^progress (\d+) of (\d+)$")).Success)
            {
                value = Int(progress.Groups[1].Value);
                of = Int(progress.Groups[2].Value);
            }
            else if (fact == "no record")
            {
                noRecord = true;
            }
            else
            {
                throw new InvalidOperationException($"Cannot read \"{fact}\" of the result \"{text}\".");
            }
        }

        return new ResultSpec(text.Trim(), first, last, StateOf(match.Groups["state"].Value), value, of, noRecord);
    }

    /// <summary>A list of results "D S, D S, progress A of B and D S", each starting with a date.</summary>
    public static List<ResultSpec> Results(string text) =>
        Regex.Split(text.Trim(), "(?:, and |, | and )(?=" + D + ")")
            .Select(Result)
            .ToList();

    public static List<BarSpec> Bars(string text)
    {
        var bars = Regex.Matches(text, @"\(([^()]*)\)")
            .Select(match =>
            {
                var bar = Regex.Match(match.Groups[1].Value, "^(?<span>" + Span + "), (?<value>-?\\d+|no value)(?:, target (?<target>\\d+))?$");
                if (!bar.Success)
                {
                    throw new InvalidOperationException($"Cannot read the trend bar \"{match.Value}\".");
                }

                var (first, last) = SpanOf(bar.Groups["span"].Value);
                return new BarSpec(
                    match.Value,
                    first,
                    last,
                    bar.Groups["value"].Value == "no value" ? null : Int(bar.Groups["value"].Value),
                    bar.Groups["target"].Success ? Int(bar.Groups["target"].Value) : null);
            })
            .ToList();
        return bars.Count > 0 ? bars : throw new InvalidOperationException($"\"{text}\" names no trend bar.");
    }

    public static EntrySpec Entry(string text)
    {
        var match = Regex.Match(
            text.Trim(),
            "^\\((?<goal>\"[^\"]+\"|`[^`]+`|the goal), (?:(?<kind>Build|Reduce), )?(?<span>" + Span + "), (?<state>" + State + ")(?<rest>(?:, [^()]+)?)\\)$");
        if (!match.Success)
        {
            throw new InvalidOperationException($"Cannot read the Today entry \"{text}\".");
        }

        var (first, last) = SpanOf(match.Groups["span"].Value);
        int? value = null;
        int? of = null;
        DateTimeOffset? grace = null;
        bool? record = null;
        bool? note = null;
        foreach (var fact in Facts(match.Groups["rest"].Value))
        {
            Match part;
            if ((part = Regex.Match(fact, @"^progress (\d+) of (\d+)$")).Success)
            {
                value = Int(part.Groups[1].Value);
                of = Int(part.Groups[2].Value);
            }
            else if ((part = Regex.Match(fact, "^grace ends (" + T + ")$")).Success)
            {
                grace = Instant(part.Groups[1].Value);
            }
            else if (fact is "record" or "no record")
            {
                record = fact == "record";
            }
            else if (fact is "note" or "no note")
            {
                note = fact == "note";
            }
            else
            {
                throw new InvalidOperationException($"Cannot read \"{fact}\" of the Today entry \"{text}\".");
            }
        }

        return new EntrySpec(
            text.Trim(),
            match.Groups["goal"].Value,
            match.Groups["kind"].Success ? Enum.Parse<BuildOrReduce>(match.Groups["kind"].Value) : null,
            first,
            last,
            StateOf(match.Groups["state"].Value),
            value,
            of,
            grace,
            record,
            note);
    }

    /// <summary>Every parenthesised Today entry in a text, in order.</summary>
    public static List<EntrySpec> Entries(string text)
    {
        var entries = Regex.Matches(text, @"\([^()]*\)").Select(match => Entry(match.Value)).ToList();
        return entries.Count > 0 ? entries : throw new InvalidOperationException($"\"{text}\" names no Today entry.");
    }

    public static NoteSpec Note(string text)
    {
        var match = Regex.Match(
            text.Trim(),
            "^\\(\"(?<goal>[^\"]+)\", (?<span>" + Span + "), \"(?<note>[^\"]*)\", (?<state>" + State + ")\\)$");
        if (!match.Success)
        {
            throw new InvalidOperationException($"Cannot read the recent note \"{text}\".");
        }

        var (first, last) = SpanOf(match.Groups["span"].Value);
        return new NoteSpec(text.Trim(), match.Groups["goal"].Value, first, last, match.Groups["note"].Value, StateOf(match.Groups["state"].Value));
    }

    /// <summary>The facts after the state: ", progress 12 of 20, record, no note" gives its three parts.</summary>
    private static IEnumerable<string> Facts(string rest) =>
        rest.Length == 0
            ? []
            : rest.TrimStart(',', ' ').Split(", ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

    /// <summary>The figures an item gives as "the consistency "X"", "the current streak "Y"" and "the longest streak "Z"".</summary>
    public static List<(string Figure, string Text)> Figures(string text)
    {
        var figures = Regex.Matches(text, "the (consistency|current streak|longest streak) \"([^\"]+)\"")
            .Select(match => (match.Groups[1].Value, match.Groups[2].Value))
            .ToList();
        return figures.Count > 0 ? figures : throw new InvalidOperationException($"\"{text}\" names no figure.");
    }
}
