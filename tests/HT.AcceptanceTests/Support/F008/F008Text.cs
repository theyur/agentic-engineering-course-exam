using System.Globalization;
using System.Text.RegularExpressions;
using HT.Domain.Evaluation;
using HT.Domain.Exemptions;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using HT.Domain.Reports;

namespace HT.AcceptanceTests.Support.F008;

/// <summary>
/// Reads the phrases of the F008 Gherkin as acceptance.md's conventions define them: dates, instants, the goals of the
/// file, reasons, history entries, pauses, trend bars, Today entries, recent notes and demo results.
/// </summary>
public static class F008Text
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
    public const string Span = D + "(?: – " + D + ")?";

    /// <summary>A reason as a When step gives it: a quoted text (kept as written) or "`x` repeated N times".</summary>
    public const string ReasonToken = "\"[^\"]*\"|`[^`]` repeated \\d+ times";

    public const string States = "Upcoming|Due|Pending|Met|Failed|Missed|Excused|Paused";

    /// <summary>A list of dates "D", "D and D", "D, D and D".</summary>
    public const string DateList = D + "(?:(?:, and |, | and )" + D + ")*";

    // ------------------------------------------------------------ simple values

    public static DateOnly Date(string text) =>
        DateOnly.ParseExact(text.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture);

    public static DateTimeOffset Instant(string text) =>
        DateTimeOffset.Parse(
            text.Trim().Trim('`'),
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);

    public static int Int(string text) => int.Parse(text.Trim(), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);

    public static List<DateOnly> Dates(string text) => Regex.Matches(text, D).Select(match => Date(match.Value)).ToList();

    public static (DateOnly First, DateOnly Last) SpanOf(string text)
    {
        var dates = Dates(text);
        return dates.Count switch
        {
            1 => (dates[0], dates[0]),
            2 => (dates[0], dates[1]),
            _ => throw new InvalidOperationException($"\"{text}\" is not a date or a period."),
        };
    }

    public static List<DateOnly> Range(DateOnly first, DateOnly last)
    {
        var dates = new List<DateOnly>();
        for (var date = first; date <= last; date = date.AddDays(1))
        {
            dates.Add(date);
        }

        return dates;
    }

    public static EvaluationState StateOf(string text) => Enum.Parse<EvaluationState>(text.Trim());

    public static PauseTiming TimingOf(string text) => text.Trim() switch
    {
        "current" => PauseTiming.Current,
        "upcoming" => PauseTiming.Upcoming,
        "past" => PauseTiming.Past,
        var other => throw new InvalidOperationException($"\"{other}\" is not current, upcoming or past."),
    };

    /// <summary>The quoted texts in a text, in order.</summary>
    public static List<string> Quoted(string text) =>
        Regex.Matches(text, "\"([^\"]*)\"").Select(match => match.Groups[1].Value).ToList();

    /// <summary>The backticked names in a text, in order.</summary>
    public static List<string> Backticked(string text) =>
        Regex.Matches(text, "`([^`]+)`").Select(match => match.Groups[1].Value).ToList();

    /// <summary>
    /// A reason as a step writes it: "\"Ill\"" is the text between the quotes, kept exactly (spaces included);
    /// "`x` repeated 201 times" is that one character repeated that many times.
    /// </summary>
    public static string Reason(string token)
    {
        var text = token.Trim();
        var repeated = Regex.Match(text, "^`(.)` repeated (\\d+) times$");
        if (repeated.Success)
        {
            return new string(repeated.Groups[1].Value[0], Int(repeated.Groups[2].Value));
        }

        var quoted = Regex.Match(token, "^\\s*\"([^\"]*)\"\\s*$");
        return quoted.Success
            ? quoted.Groups[1].Value
            : throw new InvalidOperationException($"Cannot read the reason {token}.");
    }

    /// <summary>A record value: `Yes` or `No` for a yes/no goal, a whole number for a count or duration goal.</summary>
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

    // ------------------------------------------------------------ the goals of this file

    /// <summary>A goal of acceptance.md ("Habits and goals"): its custom habit and its settings.</summary>
    public sealed record GoalDefinition(string Name, string Habit, GoalTargetFields Fields);

    /// <summary>The settings of a goal named in acceptance.md's list of goals.</summary>
    public static GoalDefinition Goal(string name) => name switch
    {
        "Yoga, 3 days a week" => new(name, "Yoga", FixedDays(BuildOrReduce.Build, Measurement.YesNo, null, null, null, DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday)),
        "Yoga, 3 times a week" => new(name, "Yoga", Times(BuildOrReduce.Build, Measurement.YesNo, null, null, null, 3, SchedulePeriod.Week)),
        "Drinking water, 8 glasses a day" => new(name, "Drinking water", Times(BuildOrReduce.Build, Measurement.Count, 8, "glasses", null, 1, SchedulePeriod.Day)),
        "Meditation, 10 min daily" => new(name, "Meditation", FixedDays(BuildOrReduce.Build, Measurement.Duration, 10, null, DurationUnit.Minutes, Enum.GetValues<DayOfWeek>())),
        "Reading, 20 pages daily" => new(name, "Reading", FixedDays(BuildOrReduce.Build, Measurement.Count, 20, "pages", null, Enum.GetValues<DayOfWeek>())),
        "Sugar, at most 1 sweet a day" => new(name, "Sugar", Times(BuildOrReduce.Reduce, Measurement.Count, 1, "sweet", null, 1, SchedulePeriod.Day)),
        _ => throw new InvalidOperationException($"`{name}` is not a goal of acceptance.md."),
    };

    private static GoalTargetFields FixedDays(
        BuildOrReduce kind, Measurement measurement, int? amount, string? countUnit, DurationUnit? durationUnit, params DayOfWeek[] days) =>
        new()
        {
            BuildOrReduce = kind,
            Measurement = measurement,
            Amount = amount,
            CountUnit = countUnit,
            DurationUnit = durationUnit,
            ScheduleKind = ScheduleKind.FixedDays,
            Days = days.ToHashSet(),
            Times = null,
            Period = null,
        };

    private static GoalTargetFields Times(
        BuildOrReduce kind, Measurement measurement, int? amount, string? countUnit, DurationUnit? durationUnit, int times, SchedulePeriod period) =>
        new()
        {
            BuildOrReduce = kind,
            Measurement = measurement,
            Amount = amount,
            CountUnit = countUnit,
            DurationUnit = durationUnit,
            ScheduleKind = ScheduleKind.Times,
            Days = null,
            Times = times,
            Period = period,
        };

    // ------------------------------------------------------------ history entries

    /// <summary>
    /// One history entry (or result) as an item writes it: its date or period (or, with "D1 to D2 each", every date of
    /// that range), its state, and as far as given its records, grace end, correction flag, reason and removability.
    /// </summary>
    public sealed record EntrySpec(
        string Text,
        IReadOnlyList<(DateOnly First, DateOnly Last)> Spans,
        EvaluationState State,
        IReadOnlyList<(YesNoAnswer? Answer, int? Amount)>? Records,
        bool? NoGraceEnd,
        DateTimeOffset? GraceEnds,
        bool? Correction,
        bool ReasonGiven,
        string? Reason,
        bool? Removable);

    /// <summary>A list of entries with the dates the item says have no entry.</summary>
    public sealed record EntryList(IReadOnlyList<EntrySpec> Entries, IReadOnlyList<(DateOnly First, DateOnly Last)> NoEntry);

    private static readonly Regex NoEntryClause = new(
        "(?:(?:,? and |, )?)no entry for (" + Span + ")",
        RegexOptions.CultureInvariant);

    private static readonly Regex EntrySeparator = new(
        "(?:; |, and |, | and )(?=" + D + ")",
        RegexOptions.CultureInvariant);

    private static readonly Regex EntryHead = new(
        "^(?:(?<from>" + D + ") to (?<to>" + D + ") each|(?<span>" + Span + ")) (?<state>" + States + ")(?<rest>, .+)?$",
        RegexOptions.CultureInvariant);

    private static readonly Regex RecordsFact = new(
        "(?:^|, )records ((?:Yes|No|\\d+)(?:, (?:Yes|No|\\d+))+)",
        RegexOptions.CultureInvariant);

    /// <summary>"2026-11-15 Pending and 2026-11-16 Paused, no grace end", "…, and no entry for 2026-11-17", "A; B; C".</summary>
    public static EntryList Entries(string text)
    {
        var noEntry = new List<(DateOnly, DateOnly)>();
        var rest = NoEntryClause.Replace(text.Trim(), match =>
        {
            noEntry.Add(SpanOf(match.Groups[1].Value));
            return string.Empty;
        }).Trim();

        var entries = rest.Length == 0
            ? []
            : EntrySeparator.Split(rest).Select(Entry).ToList();
        return new EntryList(entries, noEntry);
    }

    public static EntrySpec Entry(string text)
    {
        var match = EntryHead.Match(text.Trim());
        if (!match.Success)
        {
            throw new InvalidOperationException($"Cannot read the history entry \"{text}\".");
        }

        var spans = match.Groups["from"].Success
            ? Range(Date(match.Groups["from"].Value), Date(match.Groups["to"].Value)).Select(date => (date, date)).ToList()
            : [SpanOf(match.Groups["span"].Value)];

        List<(YesNoAnswer?, int?)>? records = null;
        var restText = match.Groups["rest"].Value;
        var many = RecordsFact.Match(restText);
        if (many.Success)
        {
            records = many.Groups[1].Value.Split(", ").Select(ValueOf).ToList();
            restText = restText.Remove(many.Index, many.Length);
        }

        bool? noGrace = null;
        DateTimeOffset? grace = null;
        bool? correction = null;
        var reasonGiven = false;
        string? reason = null;
        bool? removable = null;
        foreach (var fact in Facts(restText))
        {
            Match part;
            if (fact == "no record")
            {
                records = [];
            }
            else if ((part = Regex.Match(fact, "^record (Yes|No|\\d+)$")).Success)
            {
                records = [ValueOf(part.Groups[1].Value)];
            }
            else if (fact == "no grace end")
            {
                noGrace = true;
            }
            else if ((part = Regex.Match(fact, "^grace ends (" + T + ")$")).Success)
            {
                grace = Instant(part.Groups[1].Value);
            }
            else if (fact is "correction no" or "correction yes")
            {
                correction = fact == "correction yes";
            }
            else if (fact == "no reason")
            {
                reasonGiven = true;
                reason = null;
            }
            else if ((part = Regex.Match(fact, "^reason (.+)$")).Success)
            {
                reasonGiven = true;
                reason = Reason(part.Groups[1].Value);
            }
            else if (fact is "removable" or "not removable")
            {
                removable = fact == "removable";
            }
            else
            {
                throw new InvalidOperationException($"Cannot read \"{fact}\" of the history entry \"{text}\".");
            }
        }

        return new EntrySpec(
            text.Trim(),
            spans,
            StateOf(match.Groups["state"].Value),
            records,
            noGrace,
            grace,
            correction,
            reasonGiven,
            reason,
            removable);
    }

    /// <summary>The facts after the state: ", no record, no grace end" gives its two parts.</summary>
    private static IEnumerable<string> Facts(string rest) =>
        rest.Trim().Length == 0
            ? []
            : rest.TrimStart(',', ' ').Split(", ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

    // ------------------------------------------------------------ pauses

    /// <summary>A pause "(first day, last day, reason R or no reason, current, upcoming or past)".</summary>
    public sealed record PauseSpec(string Text, DateOnly FirstDay, DateOnly LastDay, string? Reason, PauseTiming Timing);

    private static readonly Regex PauseTuple = new(
        "\\((" + D + "), (" + D + "), (no reason|reason \"([^\"]*)\"), (current|upcoming|past)\\)",
        RegexOptions.CultureInvariant);

    /// <summary>The pauses of a list "(…), (…) and (…)", in order; "empty" is no pause.</summary>
    public static List<PauseSpec> Pauses(string text)
    {
        var trimmed = text.Trim();
        if (trimmed == "empty")
        {
            return [];
        }

        var pauses = PauseTuple.Matches(trimmed)
            .Select(match => new PauseSpec(
                match.Value,
                Date(match.Groups[1].Value),
                Date(match.Groups[2].Value),
                match.Groups[4].Success && match.Groups[3].Value != "no reason" ? match.Groups[4].Value : null,
                TimingOf(match.Groups[5].Value)))
            .ToList();
        RequireOnlySeparators(PauseTuple.Replace(trimmed, string.Empty), text);
        return pauses.Count > 0 ? pauses : throw new InvalidOperationException($"\"{text}\" names no pause.");
    }

    // ------------------------------------------------------------ trend bars

    /// <summary>A trend bar "(D, V)", "(D – D, V)", "(D, no value)", "(D, Excused)", "(D, Paused)", "(D – D, V, target T)".</summary>
    public sealed record BarSpec(string Text, DateOnly First, DateOnly Last, int? Value, TrendMark? Mark, int? Target);

    private static readonly Regex BarTuple = new(
        "\\((?<span>" + Span + "), (?<value>-?\\d+|no value|Excused|Paused)(?:, target (?<target>\\d+))?\\)",
        RegexOptions.CultureInvariant);

    public static List<BarSpec> Bars(string text)
    {
        var bars = BarTuple.Matches(text)
            .Select(match =>
            {
                var (first, last) = SpanOf(match.Groups["span"].Value);
                var value = match.Groups["value"].Value;
                return new BarSpec(
                    match.Value,
                    first,
                    last,
                    Regex.IsMatch(value, @"^-?\d+$") ? Int(value) : null,
                    value switch
                    {
                        "Excused" => TrendMark.Excused,
                        "Paused" => TrendMark.Paused,
                        _ => null,
                    },
                    match.Groups["target"].Success ? Int(match.Groups["target"].Value) : null);
            })
            .ToList();
        RequireOnlySeparators(BarTuple.Replace(text, string.Empty), text);
        return bars.Count > 0 ? bars : throw new InvalidOperationException($"\"{text}\" names no trend bar.");
    }

    // ------------------------------------------------------------ Today entries

    /// <summary>A Today entry "(`goal`, date, state)".</summary>
    public sealed record TodaySpec(string Text, string Goal, DateOnly First, DateOnly Last, EvaluationState State);

    /// <summary>What a Today-list phrase says: the entries it contains and the entries it must not contain.</summary>
    public sealed record TodayList(
        IReadOnlyList<TodaySpec> Contains,
        IReadOnlyList<(DateOnly First, DateOnly Last)> NoEntryFor,
        IReadOnlyList<(string Goal, DateOnly First, DateOnly Last)> NoEntryOfGoalFor,
        IReadOnlyList<(string Goal, IReadOnlyList<EvaluationState> States)> NoStatesOfGoal);

    private static readonly Regex TodayTuple = new(
        "\\(`(?<goal>[^`]+)`, (?<span>" + Span + "), (?<state>" + States + ")\\)",
        RegexOptions.CultureInvariant);

    private static readonly Regex NoEntryOfGoal = new(
        "no entry of `(?<goal>[^`]+)` for (?<span>" + Span + ")",
        RegexOptions.CultureInvariant);

    private static readonly Regex NoEntryForSpan = new(
        "no entry for (?<span>" + Span + ")",
        RegexOptions.CultureInvariant);

    private static readonly Regex NoStatesOf = new(
        "no (?<first>Due|Upcoming|Pending) and no (?<second>Due|Upcoming|Pending) entry of `(?<goal>[^`]+)`",
        RegexOptions.CultureInvariant);

    public static TodayList Today(string text)
    {
        var rest = text;
        var contains = TodayTuple.Matches(rest)
            .Select(match =>
            {
                var (first, last) = SpanOf(match.Groups["span"].Value);
                return new TodaySpec(match.Value, match.Groups["goal"].Value, first, last, StateOf(match.Groups["state"].Value));
            })
            .ToList();
        rest = TodayTuple.Replace(rest, string.Empty);

        var noOfGoal = NoEntryOfGoal.Matches(rest)
            .Select(match =>
            {
                var (first, last) = SpanOf(match.Groups["span"].Value);
                return (match.Groups["goal"].Value, first, last);
            })
            .ToList();
        rest = NoEntryOfGoal.Replace(rest, string.Empty);

        var noFor = NoEntryForSpan.Matches(rest).Select(match => SpanOf(match.Groups["span"].Value)).ToList();
        rest = NoEntryForSpan.Replace(rest, string.Empty);

        var noStates = NoStatesOf.Matches(rest)
            .Select(match => (match.Groups["goal"].Value,
                (IReadOnlyList<EvaluationState>)[StateOf(match.Groups["first"].Value), StateOf(match.Groups["second"].Value)]))
            .ToList();
        rest = NoStatesOf.Replace(rest, string.Empty);

        RequireOnlySeparators(rest, text);
        return new TodayList(contains, noFor, noOfGoal, noStates);
    }

    // ------------------------------------------------------------ recent notes

    /// <summary>A recent note "(date, "note", state)" or, on the dashboard, "(`goal`, date, "note", state)".</summary>
    public sealed record NoteSpec(string Text, string? Goal, DateOnly First, DateOnly Last, string Note, EvaluationState State);

    private static readonly Regex NoteTuple = new(
        "\\((?:`(?<goal>[^`]+)`, )?(?<span>" + Span + "), \"(?<note>[^\"]*)\", (?<state>" + States + ")\\)",
        RegexOptions.CultureInvariant);

    public static List<NoteSpec> Notes(string text)
    {
        var notes = NoteTuple.Matches(text)
            .Select(match =>
            {
                var (first, last) = SpanOf(match.Groups["span"].Value);
                return new NoteSpec(
                    match.Value,
                    match.Groups["goal"].Success ? match.Groups["goal"].Value : null,
                    first,
                    last,
                    match.Groups["note"].Value,
                    StateOf(match.Groups["state"].Value));
            })
            .ToList();
        RequireOnlySeparators(NoteTuple.Replace(text, string.Empty), text);
        return notes.Count > 0 ? notes : throw new InvalidOperationException($"\"{text}\" names no recent note.");
    }

    // ------------------------------------------------------------ recent results

    /// <summary>
    /// The results of "2026-11-02 to 2026-11-08 each Met; 2026-11-09 Excused; 2026-11-13, 2026-11-14 and 2026-11-15
    /// each Paused; …" as (date, state) in the order written.
    /// </summary>
    public static List<(DateOnly Date, EvaluationState State)> ResultSequence(string text)
    {
        var results = new List<(DateOnly, EvaluationState)>();
        foreach (var segment in text.Split("; ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            var range = Regex.Match(segment, "^(" + D + ") to (" + D + ") each (" + States + ")$");
            if (range.Success)
            {
                var state = StateOf(range.Groups[3].Value);
                results.AddRange(Range(Date(range.Groups[1].Value), Date(range.Groups[2].Value)).Select(date => (date, state)));
                continue;
            }

            var list = Regex.Match(segment, "^(" + DateList + ")(?: each)? (" + States + ")$");
            if (!list.Success)
            {
                throw new InvalidOperationException($"Cannot read the results \"{segment}\".");
            }

            var listState = StateOf(list.Groups[2].Value);
            results.AddRange(Dates(list.Groups[1].Value).Select(date => (date, listState)));
        }

        return results;
    }

    /// <summary>"Met 10, Missed 1, Excused 1, Paused 3, Pending 2".</summary>
    public static List<(EvaluationState State, int Count)> StateCounts(string text) =>
        text.Split(", ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(part =>
            {
                var match = Regex.Match(part, "^(" + States + ") (\\d+)$");
                return match.Success
                    ? (StateOf(match.Groups[1].Value), Int(match.Groups[2].Value))
                    : throw new InvalidOperationException($"Cannot read the count \"{part}\".");
            })
            .ToList();

    // ------------------------------------------------------------ demo results

    /// <summary>The results of a demo task "(email, text), (email, text) and (email, text)" or "(text)", in order.</summary>
    public static List<(string? Email, string Text)> DemoLines(string text)
    {
        var tuples = Regex.Matches(text, @"\(([^()]*)\)").ToList();
        var lines = tuples
            .Select(match =>
            {
                var inner = match.Groups[1].Value;
                var withEmail = Regex.Match(inner, @"^([^\s,]+@[^\s,]+), (.+)$");
                return withEmail.Success
                    ? ((string?)withEmail.Groups[1].Value, withEmail.Groups[2].Value)
                    : ((string?)null, inner);
            })
            .ToList();
        RequireOnlySeparators(Regex.Replace(text, @"\(([^()]*)\)", string.Empty), text);
        return lines.Count > 0 ? lines : throw new InvalidOperationException($"\"{text}\" names no result.");
    }

    /// <summary>Fails when a phrase has more than the parts read from it (only list separators may remain).</summary>
    private static void RequireOnlySeparators(string rest, string text)
    {
        var leftover = Regex.Replace(rest, "(?:,|\\band\\b|\\s)+", string.Empty);
        if (leftover.Length > 0)
        {
            throw new InvalidOperationException($"Cannot read \"{leftover}\" in \"{text}\".");
        }
    }
}
