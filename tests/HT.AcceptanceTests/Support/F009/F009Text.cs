using System.Globalization;
using System.Text.RegularExpressions;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using GoalOutcome = HT.Domain.Goals.Outcome;

namespace HT.AcceptanceTests.Support.F009;

/// <summary>
/// Reads the phrases of the F009 Gherkin as acceptance.md's conventions define them: dates, instants, names and notes,
/// the goals of the file, settings, history entries, attempts and goal history entries.
/// </summary>
public static class F009Text
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

    /// <summary>A date or a per-week period's first and last date ("2026-11-16 – 2026-11-22").</summary>
    public const string Span = D + "(?: – " + D + ")?";

    public const string States = "Upcoming|Due|Pending|Met|Failed|Missed|Excused|Paused";

    public const string Outcomes = "Achieved|Abandoned|No longer relevant";

    /// <summary>A list of dates "D", "D and D", "D, D and D".</summary>
    public const string DateList = D + "(?:(?:, and |, | and )" + D + ")*";

    /// <summary>The acting user of a step: "`x`" or "the anonymous visitor".</summary>
    public const string Actor = "`[^`]+`|the anonymous visitor";

    /// <summary>A text a step gives: quoted (kept as written), "`x` repeated N times", or that with two spaces around it.</summary>
    public const string TextToken = "\"[^\"]*\"|two spaces, `[^`]` repeated \\d+ times, two spaces|`[^`]` repeated \\d+ times";

    /// <summary>A name a Then step expects: a repeated character, a backticked name or a quoted name.</summary>
    public const string NameToken = "`[^`]` repeated \\d+ times|`[^`]+`|\"[^\"]*\"";

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

    public static GoalOutcome OutcomeOf(string text) => text.Trim() switch
    {
        "Achieved" => GoalOutcome.Achieved,
        "Abandoned" => GoalOutcome.Abandoned,
        "No longer relevant" => GoalOutcome.NoLongerRelevant,
        var other => throw new InvalidOperationException($"\"{other}\" is not an outcome."),
    };

    /// <summary>The quoted texts in a text, in order.</summary>
    public static List<string> Quoted(string text) =>
        Regex.Matches(text, "\"([^\"]*)\"").Select(match => match.Groups[1].Value).ToList();

    /// <summary>The backticked names in a text, in order.</summary>
    public static List<string> Backticked(string text) =>
        Regex.Matches(text, "`([^`]+)`").Select(match => match.Groups[1].Value).ToList();

    /// <summary>
    /// A text token: "\"N\"" is the text between the quotes (spaces kept, "\"\"" is empty); "`x` repeated 61 times" is
    /// that one character repeated that many times; "two spaces, `x` repeated 60 times, two spaces" puts two spaces
    /// before and after it; "`N`" (a name in backticks) is the name.
    /// </summary>
    public static string Text(string token)
    {
        var text = token.Trim();
        var padded = Regex.Match(text, "^two spaces, `(.)` repeated (\\d+) times, two spaces$");
        if (padded.Success)
        {
            return "  " + new string(padded.Groups[1].Value[0], Int(padded.Groups[2].Value)) + "  ";
        }

        var repeated = Regex.Match(text, "^`(.)` repeated (\\d+) times$");
        if (repeated.Success)
        {
            return new string(repeated.Groups[1].Value[0], Int(repeated.Groups[2].Value));
        }

        var quoted = Regex.Match(token, "^\\s*\"([^\"]*)\"\\s*$");
        if (quoted.Success)
        {
            return quoted.Groups[1].Value;
        }

        var backticked = Regex.Match(text, "^`([^`]+)`$");
        return backticked.Success
            ? backticked.Groups[1].Value
            : throw new InvalidOperationException($"Cannot read the text {token}.");
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
    public sealed record GoalDefinition(string Name, string Habit, SettingsSpec Settings);

    /// <summary>The settings of a goal named in acceptance.md's list of goals.</summary>
    public static GoalDefinition Goal(string name) => name switch
    {
        "Reading, 15 min daily" => new(name, "Reading", Settings("Build, duration 15 minutes, fixed days on all seven days")),
        "Meditation, 10 min daily" => new(name, "Meditation", Settings("Build, duration 10 minutes, fixed days on all seven days")),
        "Yoga, 3 times a week" => new(name, "Yoga", Settings("Build, yes/no, 3 times per week")),
        "Sugar, at most 1 sweet a day" => new(name, "Sugar", Settings("Reduce, count 1 `sweet`, 1 time per day")),
        _ => throw new InvalidOperationException($"`{name}` is not a goal of acceptance.md."),
    };

    // ------------------------------------------------------------ settings

    /// <summary>
    /// Goal settings as acceptance.md writes them: Build or Reduce, the measurement, the schedule, and, when an item
    /// checks it, the time zone. Missing or wrong values are kept as written ("no measurement", "fixed days with no
    /// day", …). <see cref="ExtraTimes"/> and <see cref="ExtraPeriod"/> are values given for a schedule they do not
    /// belong to ("with the number of times 21 and the period per week also given").
    /// </summary>
    public sealed record SettingsSpec(
        string Text,
        BuildOrReduce? BuildOrReduce,
        Measurement? Measurement,
        int? Amount,
        string? CountUnit,
        DurationUnit? DurationUnit,
        ScheduleKind? ScheduleKind,
        IReadOnlySet<DayOfWeek>? Days,
        int? Times,
        SchedulePeriod? Period,
        int? ExtraTimes,
        SchedulePeriod? ExtraPeriod,
        string? TimeZone)
    {
        /// <summary>The target fields of the create, change and new-attempt use cases, as written.</summary>
        public GoalTargetFields ToFields() => new()
        {
            BuildOrReduce = BuildOrReduce,
            Measurement = Measurement,
            Amount = Amount,
            CountUnit = CountUnit,
            DurationUnit = DurationUnit,
            ScheduleKind = ScheduleKind,
            Days = Days,
            Times = ExtraTimes ?? Times,
            Period = ExtraPeriod ?? Period,
        };

        /// <summary>
        /// The settings a read must give for this text: values that do not belong to the measurement or the schedule
        /// are absent (a yes/no goal has no amount, a fixed-days schedule has no number of times, …).
        /// </summary>
        public SettingsShape Expected() => new(
            BuildOrReduce,
            Measurement,
            Amount,
            Measurement == HT.Domain.Habits.Measurement.Count ? CountUnit : null,
            Measurement == HT.Domain.Habits.Measurement.Duration ? DurationUnit : null,
            ScheduleKind,
            ScheduleKind == HT.Domain.Habits.ScheduleKind.FixedDays ? DaysText(Days) : string.Empty,
            ScheduleKind == HT.Domain.Habits.ScheduleKind.Times ? Times : null,
            ScheduleKind == HT.Domain.Habits.ScheduleKind.Times ? Period : null);
    }

    /// <summary>Settings in a form that compares field by field (the days as one text in week order).</summary>
    public sealed record SettingsShape(
        BuildOrReduce? BuildOrReduce,
        Measurement? Measurement,
        int? Amount,
        string? CountUnit,
        DurationUnit? DurationUnit,
        ScheduleKind? ScheduleKind,
        string Days,
        int? Times,
        SchedulePeriod? Period);

    public static string DaysText(IEnumerable<DayOfWeek>? days) =>
        days is null ? string.Empty : string.Join(",", days.Distinct().OrderBy(day => ((int)day + 6) % 7));

    private const string DayName = "(?:Monday|Tuesday|Wednesday|Thursday|Friday|Saturday|Sunday)";

    private static readonly Regex SettingsPattern = new(
        "^(?<bor>Build|Reduce|no Build or Reduce), " +
        "(?:(?<yna>yes/no with the amount (?<ynamount>\\d+))|(?<yn>yes/no)|(?<nom>no measurement)" +
        "|count (?<cnoun>\\d+) with no unit|count (?<camount>\\d+) `(?<cunit>[^`]+)`" +
        "|duration (?<dnounit>\\d+) with no time unit|duration (?<damount>\\d+) (?<dunit>minutes|hours)), " +
        "(?:(?<all>fixed days on all seven days)|fixed days on (?<days>" + DayName + "(?:(?:, | and )" + DayName + ")*)" +
        "|(?<noday>fixed days with no day)|(?<nosched>no schedule)" +
        "|(?<tnoperiod>\\d+) times with no period|(?<times>\\d+) times? per (?<period>day|week))" +
        "(?:, with the number of times (?<xtimes>\\d+) and the period per (?<xperiod>day|week) also given)?" +
        "(?:, time zone (?<tz>[A-Za-z_/+-]+))?$",
        RegexOptions.CultureInvariant);

    public static SettingsSpec Settings(string text)
    {
        var match = SettingsPattern.Match(text.Trim());
        if (!match.Success)
        {
            throw new InvalidOperationException($"Cannot read the settings \"{text}\".");
        }

        BuildOrReduce? kind = match.Groups["bor"].Value switch
        {
            "Build" => BuildOrReduce.Build,
            "Reduce" => BuildOrReduce.Reduce,
            _ => null,
        };

        Measurement? measurement = null;
        int? amount = null;
        string? countUnit = null;
        DurationUnit? durationUnit = null;
        if (match.Groups["yna"].Success)
        {
            measurement = Measurement.YesNo;
            amount = Int(match.Groups["ynamount"].Value);
        }
        else if (match.Groups["yn"].Success)
        {
            measurement = Measurement.YesNo;
        }
        else if (match.Groups["cnoun"].Success)
        {
            measurement = Measurement.Count;
            amount = Int(match.Groups["cnoun"].Value);
        }
        else if (match.Groups["camount"].Success)
        {
            measurement = Measurement.Count;
            amount = Int(match.Groups["camount"].Value);
            countUnit = match.Groups["cunit"].Value;
        }
        else if (match.Groups["dnounit"].Success)
        {
            measurement = Measurement.Duration;
            amount = Int(match.Groups["dnounit"].Value);
        }
        else if (match.Groups["damount"].Success)
        {
            measurement = Measurement.Duration;
            amount = Int(match.Groups["damount"].Value);
            durationUnit = match.Groups["dunit"].Value == "minutes" ? DurationUnit.Minutes : DurationUnit.Hours;
        }

        ScheduleKind? schedule = null;
        IReadOnlySet<DayOfWeek>? days = null;
        int? times = null;
        SchedulePeriod? period = null;
        if (match.Groups["all"].Success)
        {
            schedule = ScheduleKind.FixedDays;
            days = Enum.GetValues<DayOfWeek>().ToHashSet();
        }
        else if (match.Groups["days"].Success)
        {
            schedule = ScheduleKind.FixedDays;
            days = Regex.Matches(match.Groups["days"].Value, DayName).Select(day => Enum.Parse<DayOfWeek>(day.Value)).ToHashSet();
        }
        else if (match.Groups["noday"].Success)
        {
            schedule = ScheduleKind.FixedDays;
            days = new HashSet<DayOfWeek>();
        }
        else if (match.Groups["tnoperiod"].Success)
        {
            schedule = ScheduleKind.Times;
            times = Int(match.Groups["tnoperiod"].Value);
        }
        else if (match.Groups["times"].Success)
        {
            schedule = ScheduleKind.Times;
            times = Int(match.Groups["times"].Value);
            period = PeriodOf(match.Groups["period"].Value);
        }

        return new SettingsSpec(
            text.Trim(),
            kind,
            measurement,
            amount,
            countUnit,
            durationUnit,
            schedule,
            days,
            times,
            period,
            match.Groups["xtimes"].Success ? Int(match.Groups["xtimes"].Value) : null,
            match.Groups["xperiod"].Success ? PeriodOf(match.Groups["xperiod"].Value) : null,
            match.Groups["tz"].Success ? match.Groups["tz"].Value : null);
    }

    private static SchedulePeriod PeriodOf(string text) => text == "day" ? SchedulePeriod.Day : SchedulePeriod.Week;

    // ------------------------------------------------------------ history entries and results

    /// <summary>
    /// One history entry (or result) as an item writes it: its dates (one span, or every date of a range written "D1
    /// to D2 each" or "D1 back to D2, each"), its state and, as far as written, its records ("no record" is none).
    /// </summary>
    public sealed record EntrySpec(
        string Text,
        IReadOnlyList<(DateOnly First, DateOnly Last)> Spans,
        EvaluationState State,
        IReadOnlyList<(YesNoAnswer? Answer, int? Amount)>? Records);

    private static readonly Regex EntrySeparator = new(
        "(?:; and |; then |; |, then |, and | and )(?=" + D + ")",
        RegexOptions.CultureInvariant);

    private static readonly Regex BackTo = new(
        "^(?<a>" + D + ") back to (?<b>" + D + "), each (?<state>" + States + ")(?<rest>, .+)?$",
        RegexOptions.CultureInvariant);

    private static readonly Regex ToEach = new(
        "^(?<a>" + D + ") to (?<b>" + D + "),? each (?<state>" + States + ")(?<rest>, .+)?$",
        RegexOptions.CultureInvariant);

    private static readonly Regex Single = new(
        "^(?<span>" + Span + ") (?<state>" + States + ")(?<rest>, .+)?$",
        RegexOptions.CultureInvariant);

    /// <summary>"D1 to D2 each Met, record 15", "D Failed, record 15 and D Failed, record 15", "A; B; and C", "A; then B".</summary>
    public static List<EntrySpec> Entries(string text) =>
        EntrySeparator.Split(text.Trim()).Select(Entry).ToList();

    public static EntrySpec Entry(string text)
    {
        var trimmed = text.Trim();
        Match match;
        List<(DateOnly, DateOnly)> spans;
        if ((match = BackTo.Match(trimmed)).Success)
        {
            var range = Range(Date(match.Groups["b"].Value), Date(match.Groups["a"].Value));
            range.Reverse();
            spans = range.Select(date => (date, date)).ToList();
        }
        else if ((match = ToEach.Match(trimmed)).Success)
        {
            spans = Range(Date(match.Groups["a"].Value), Date(match.Groups["b"].Value)).Select(date => (date, date)).ToList();
        }
        else if ((match = Single.Match(trimmed)).Success)
        {
            spans = [SpanOf(match.Groups["span"].Value)];
        }
        else
        {
            throw new InvalidOperationException($"Cannot read the history entry \"{text}\".");
        }

        return new EntrySpec(trimmed, spans, StateOf(match.Groups["state"].Value), RecordsOf(match.Groups["rest"].Value, text));
    }

    private static List<(YesNoAnswer?, int?)>? RecordsOf(string rest, string text)
    {
        var facts = rest.Trim().TrimStart(',').Trim();
        if (facts.Length == 0)
        {
            return null;
        }

        if (facts == "no record")
        {
            return [];
        }

        var one = Regex.Match(facts, "^record (Yes|No|\\d+)$");
        if (one.Success)
        {
            return [ValueOf(one.Groups[1].Value)];
        }

        var many = Regex.Match(facts, "^records ((?:Yes|No|\\d+)(?:, (?:Yes|No|\\d+))+)$");
        return many.Success
            ? many.Groups[1].Value.Split(", ").Select(ValueOf).ToList()
            : throw new InvalidOperationException($"Cannot read \"{facts}\" of the history entry \"{text}\".");
    }

    /// <summary>A list of results "D1 to D2 each Met, then D Missed and D Missed" as (date, state) in the order written.</summary>
    public static List<(DateOnly Date, EvaluationState State)> ResultSequence(string text) =>
        Entries(text)
            .SelectMany(entry => entry.Spans.Select(span => (span.First, entry.State)))
            .ToList();

    // ------------------------------------------------------------ what can be recorded now and the Today list

    /// <summary>
    /// An entry "(`goal`, date, state)" with, for an ended attempt, "attempt N", "the latest attempt" or "not the latest
    /// attempt", "goal ended" or "goal not ended", and Build or Reduce.
    /// </summary>
    public sealed record TupleSpec(
        string Text,
        string Goal,
        DateOnly First,
        DateOnly Last,
        EvaluationState State,
        int? Attempt,
        bool? Latest,
        bool? GoalEnded,
        BuildOrReduce? BuildOrReduce);

    private static readonly Regex Tuple = new(
        "\\(`(?<goal>[^`]+)`, (?<span>" + Span + "), (?<state>" + States + ")(?<extra>(?:, [^,()]+)*)\\)",
        RegexOptions.CultureInvariant);

    public static List<TupleSpec> Tuples(string text)
    {
        var tuples = Tuple.Matches(text).Select(match =>
        {
            int? attempt = null;
            bool? latest = null;
            bool? goalEnded = null;
            BuildOrReduce? kind = null;
            foreach (var fact in match.Groups["extra"].Value.Split(", ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            {
                var number = Regex.Match(fact, "^attempt (\\d+)$");
                if (number.Success)
                {
                    attempt = Int(number.Groups[1].Value);
                }
                else if (fact is "the latest attempt" or "not the latest attempt")
                {
                    latest = fact == "the latest attempt";
                }
                else if (fact is "goal ended" or "goal not ended")
                {
                    goalEnded = fact == "goal ended";
                }
                else if (fact is "Build" or "Reduce")
                {
                    kind = Enum.Parse<BuildOrReduce>(fact);
                }
                else
                {
                    throw new InvalidOperationException($"Cannot read \"{fact}\" in \"{match.Value}\".");
                }
            }

            var (first, last) = SpanOf(match.Groups["span"].Value);
            return new TupleSpec(match.Value, match.Groups["goal"].Value, first, last, StateOf(match.Groups["state"].Value), attempt, latest, goalEnded, kind);
        }).ToList();
        RequireOnlySeparators(Tuple.Replace(text, string.Empty), text);
        return tuples.Count > 0 ? tuples : throw new InvalidOperationException($"\"{text}\" names no entry.");
    }

    // ------------------------------------------------------------ recent notes and pauses

    public sealed record NoteSpec(string Text, DateOnly First, DateOnly Last, string Note, EvaluationState State);

    private static readonly Regex NoteTuple = new(
        "\\((?<span>" + Span + "), \"(?<note>[^\"]*)\", (?<state>" + States + ")\\)",
        RegexOptions.CultureInvariant);

    public static List<NoteSpec> Notes(string text)
    {
        var notes = NoteTuple.Matches(text)
            .Select(match =>
            {
                var (first, last) = SpanOf(match.Groups["span"].Value);
                return new NoteSpec(match.Value, first, last, match.Groups["note"].Value, StateOf(match.Groups["state"].Value));
            })
            .ToList();
        RequireOnlySeparators(NoteTuple.Replace(text, string.Empty), text);
        return notes.Count > 0 ? notes : throw new InvalidOperationException($"\"{text}\" names no recent note.");
    }

    public sealed record PauseSpec(string Text, DateOnly FirstDay, DateOnly LastDay, string Timing);

    private static readonly Regex PauseTuple = new(
        "\\((" + D + "), (" + D + "), no reason, (current|upcoming|past)\\)",
        RegexOptions.CultureInvariant);

    public static List<PauseSpec> Pauses(string text)
    {
        var pauses = PauseTuple.Matches(text)
            .Select(match => new PauseSpec(match.Value, Date(match.Groups[1].Value), Date(match.Groups[2].Value), match.Groups[3].Value))
            .ToList();
        RequireOnlySeparators(PauseTuple.Replace(text, string.Empty), text);
        return pauses.Count > 0 ? pauses : throw new InvalidOperationException($"\"{text}\" names no pause.");
    }

    // ------------------------------------------------------------ attempts

    /// <summary>An attempt (FR-014) as an item writes it: its number and, as far as written, its fields.</summary>
    public sealed class AttemptSpec
    {
        public required string Text { get; init; }

        public required int Number { get; init; }

        public string? Name { get; set; }

        public DateOnly? StartDate { get; set; }

        public bool NotEnded { get; set; }

        public DateOnly? EndingDate { get; set; }

        public DateOnly? LastDate { get; set; }

        public GoalOutcome? Outcome { get; set; }

        public bool NoteWritten { get; set; }

        public string? Note { get; set; }

        public string? Consistency { get; set; }

        public string? CurrentStreak { get; set; }

        public string? LongestStreak { get; set; }
    }

    private static readonly Regex AttemptHead = new("^attempt (?<n>\\d+)(?<rest>.*)$", RegexOptions.CultureInvariant);

    private static readonly Regex AttemptFact = new(
        "\\G(?:, )?(?:" +
        "`(?<name>[^`]+)`" +
        "|start date (?<start>" + D + ")" +
        "|(?<notended>not ended)" +
        "|ending date (?<ending>" + D + ")" +
        "|last date (?<last>" + D + ")" +
        "|outcome (?<outcome>" + Outcomes + ")" +
        "|(?<nonote>no note)" +
        "|note \"(?<note>[^\"]*)\"" +
        "|consistency \"(?<consistency>[^\"]*)\"" +
        "|current streak \"(?<current>[^\"]*)\"" +
        "|longest streak \"(?<longest>[^\"]*)\"" +
        ")",
        RegexOptions.CultureInvariant);

    private static readonly Regex AttemptSeparator = new(
        "(?:; and |; |, and | and |, )(?=attempt \\d)",
        RegexOptions.CultureInvariant);

    /// <summary>"attempt 2, `N`, start date D, not ended; and attempt 1, …", "attempt 3 (…), attempt 2 (…) and attempt 1 (…)".</summary>
    public static List<AttemptSpec> Attempts(string text) =>
        AttemptSeparator.Split(text.Trim()).Select(Attempt).ToList();

    public static AttemptSpec Attempt(string text)
    {
        var head = AttemptHead.Match(text.Trim());
        if (!head.Success)
        {
            throw new InvalidOperationException($"Cannot read the attempt \"{text}\".");
        }

        var spec = new AttemptSpec { Text = text.Trim(), Number = Int(head.Groups["n"].Value) };
        var rest = head.Groups["rest"].Value;
        var named = Regex.Match(rest, "^ named (" + NameToken + ")$");
        if (named.Success)
        {
            spec.Name = Text(named.Groups[1].Value);
            return spec;
        }

        var parenthesised = Regex.Match(rest, "^ \\((.+)\\)$");
        var facts = parenthesised.Success ? parenthesised.Groups[1].Value : rest;
        var position = 0;
        while (position < facts.Length)
        {
            var match = AttemptFact.Match(facts, position);
            if (!match.Success || match.Length == 0)
            {
                throw new InvalidOperationException($"Cannot read \"{facts[position..]}\" of the attempt \"{text}\".");
            }

            if (match.Groups["name"].Success)
            {
                spec.Name = match.Groups["name"].Value;
            }
            else if (match.Groups["start"].Success)
            {
                spec.StartDate = Date(match.Groups["start"].Value);
            }
            else if (match.Groups["notended"].Success)
            {
                spec.NotEnded = true;
            }
            else if (match.Groups["ending"].Success)
            {
                spec.EndingDate = Date(match.Groups["ending"].Value);
            }
            else if (match.Groups["last"].Success)
            {
                spec.LastDate = Date(match.Groups["last"].Value);
            }
            else if (match.Groups["outcome"].Success)
            {
                spec.Outcome = OutcomeOf(match.Groups["outcome"].Value);
            }
            else if (match.Groups["nonote"].Success)
            {
                spec.NoteWritten = true;
                spec.Note = null;
            }
            else if (match.Groups["note"].Success)
            {
                spec.NoteWritten = true;
                spec.Note = match.Groups["note"].Value;
            }
            else if (match.Groups["consistency"].Success)
            {
                spec.Consistency = match.Groups["consistency"].Value;
            }
            else if (match.Groups["current"].Success)
            {
                spec.CurrentStreak = match.Groups["current"].Value;
            }
            else if (match.Groups["longest"].Success)
            {
                spec.LongestStreak = match.Groups["longest"].Value;
            }

            position = match.Index + match.Length;
        }

        return spec;
    }

    // ------------------------------------------------------------ goal history entries

    /// <summary>A goal history entry (FR-015) as an item writes it: its name, its number and, as far as written, the rest.</summary>
    public sealed record HistoryEntrySpec(
        string Text,
        string Name,
        int? Attempt,
        DateOnly? EndingDate,
        GoalOutcome? Outcome,
        bool NoteWritten,
        string? Note,
        bool? Latest);

    private static readonly Regex HistoryTuple = new(
        "\\(`(?<name>[^`]+)`, attempt (?<n>\\d+), ending date (?<d>" + D + "), (?<o>" + Outcomes + "), " +
        "(?:(?<nonote>no note)|note \"(?<note>[^\"]*)\"), (?<latest>the goal's latest attempt|not the goal's latest attempt)\\)",
        RegexOptions.CultureInvariant);

    private static readonly Regex HistoryItem = new(
        "^`(?<name>[^`]+)`, attempt (?<n>\\d+)(?:, ending date (?<d>" + D + "))?(?:, (?<latest>the goal's latest attempt|not the goal's latest attempt))?$",
        RegexOptions.CultureInvariant);

    private static readonly Regex HistoryByDate = new(
        "`(?<name>[^`]+)` \\(ending date (?<d>" + D + ")\\)",
        RegexOptions.CultureInvariant);

    /// <summary>
    /// A goal history list: "(…) and (…)" tuples, "`N`, attempt 1, …; and `M`, attempt 1, …", "`N` (ending date D), …
    /// and `M` (ending date D)", or "no entry".
    /// </summary>
    public static List<HistoryEntrySpec> HistoryEntries(string text)
    {
        var trimmed = text.Trim();
        if (trimmed == "no entry")
        {
            return [];
        }

        if (trimmed.StartsWith('('))
        {
            var tuples = HistoryTuple.Matches(trimmed).Select(match => new HistoryEntrySpec(
                match.Value,
                match.Groups["name"].Value,
                Int(match.Groups["n"].Value),
                Date(match.Groups["d"].Value),
                OutcomeOf(match.Groups["o"].Value),
                true,
                match.Groups["note"].Success ? match.Groups["note"].Value : null,
                match.Groups["latest"].Value == "the goal's latest attempt")).ToList();
            RequireOnlySeparators(HistoryTuple.Replace(trimmed, string.Empty), text);
            return tuples.Count > 0 ? tuples : throw new InvalidOperationException($"\"{text}\" names no goal history entry.");
        }

        if (trimmed.Contains("(ending date ", StringComparison.Ordinal))
        {
            var byDate = HistoryByDate.Matches(trimmed).Select(match => new HistoryEntrySpec(
                match.Value, match.Groups["name"].Value, null, Date(match.Groups["d"].Value), null, false, null, null)).ToList();
            RequireOnlySeparators(HistoryByDate.Replace(trimmed, string.Empty), text);
            return byDate;
        }

        return trimmed.Split(["; and ", "; "], StringSplitOptions.TrimEntries).Select(item =>
        {
            var match = HistoryItem.Match(item);
            if (!match.Success)
            {
                throw new InvalidOperationException($"Cannot read the goal history entry \"{item}\".");
            }

            return new HistoryEntrySpec(
                item,
                match.Groups["name"].Value,
                Int(match.Groups["n"].Value),
                match.Groups["d"].Success ? Date(match.Groups["d"].Value) : null,
                null,
                false,
                null,
                match.Groups["latest"].Success ? match.Groups["latest"].Value == "the goal's latest attempt" : null);
        }).ToList();
    }

    // ------------------------------------------------------------ demo results

    /// <summary>The results of a demo task "(email, text), (email, text) and (email, text)" or "(text)", in order.</summary>
    public static List<(string? Email, string Text)> DemoLines(string text)
    {
        var lines = Regex.Matches(text, @"\(([^()]*)\)")
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
    public static void RequireOnlySeparators(string rest, string text)
    {
        var leftover = Regex.Replace(rest, "(?:,|;|\\band\\b|\\s)+", string.Empty);
        if (leftover.Length > 0)
        {
            throw new InvalidOperationException($"Cannot read \"{leftover}\" in \"{text}\".");
        }
    }
}
