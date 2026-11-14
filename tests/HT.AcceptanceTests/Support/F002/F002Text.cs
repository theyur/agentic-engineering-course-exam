using System.Globalization;
using System.Text.RegularExpressions;
using HT.Application.Habits;
using HT.Application.Operator;
using HT.Domain.Habits;

namespace HT.AcceptanceTests.Support.F002;

/// <summary>
/// Reads the values written in the F002 steps, following the conventions of the acceptance criteria:
/// <c>`"   "`</c> is the text between the quotes, "a text of N characters" is <c>t</c> repeated N times,
/// "a unit of N characters" is <c>u</c> repeated N times, "a name of N characters with the prefix `P`" is
/// P, a space and then <c>x</c> up to N characters, and fields a step does not give keep their defaults.
/// </summary>
public static partial class F002Text
{
    /// <summary>The acting-user phrase of a step: "the anonymous visitor", "the registered user `x`", "the administrator `a`".</summary>
    public const string Actor = "(the anonymous visitor|the registered user(?: `[^`]+`)?|the administrator(?: `[^`]+`)?)";

    public const string AdministratorPassword = "admin2026";

    public const string RegisteredUserPassword = "walking42";

    public const string DefaultIcon = "book-open";

    /// <summary>The days of the week in the order FR-017 writes them, Monday first.</summary>
    public static readonly IReadOnlyList<DayOfWeek> WeekOrder =
    [
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday,
        DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday,
    ];

    // ---------------------------------------------------------------- text values

    /// <summary>
    /// A text written as <c>none</c>, <c>`literal`</c>, <c>`"quoted"`</c>, <c>"quoted"</c>, "a text of N characters",
    /// "a name of N characters with the prefix `P`", "`P` followed by `x` up to N characters", or several of these
    /// joined by " + ", optionally followed by a note in parentheses.
    /// </summary>
    public static TextValue ParseTextExpression(string expression)
    {
        var text = expression.Trim();
        if (text == "none")
        {
            return new TextValue(null, null);
        }

        text = TrailingNote().Replace(text, string.Empty);
        var terms = text.Split(" + ").Select(ParseTextTerm).ToList();
        var start = 0;
        var end = terms.Count;
        while (terms.Count > 1 && start < end && terms[start].Trim().Length == 0)
        {
            start++;
        }

        while (terms.Count > 1 && end > start && terms[end - 1].Trim().Length == 0)
        {
            end--;
        }

        return new TextValue(string.Concat(terms), string.Concat(terms.Skip(start).Take(end - start)));
    }

    /// <summary>A count unit such as "the unit `"  "` followed by a unit of 20 characters and `"  "`".</summary>
    public static string ParseUnitExpression(string expression)
    {
        var text = TrailingNote().Replace(expression.Trim(), string.Empty);
        if (text.StartsWith("the unit ", StringComparison.Ordinal))
        {
            text = text["the unit ".Length..];
        }

        return string.Concat(UnitJoin().Split(text).Select(ParseTextTerm));
    }

    private static string ParseTextTerm(string term)
    {
        var text = term.Trim();
        Match match;
        if ((match = BacktickQuoted().Match(text)).Success)
        {
            return match.Groups[1].Value;
        }

        if ((match = FollowedByUpTo().Match(text)).Success)
        {
            return match.Groups[1].Value.PadRight(Number(match.Groups[3].Value), match.Groups[2].Value[0]);
        }

        if ((match = Backticked().Match(text)).Success && match.Length == text.Length)
        {
            return match.Groups[1].Value;
        }

        if ((match = PlainQuoted().Match(text)).Success)
        {
            return match.Groups[1].Value;
        }

        if ((match = TextOfLength().Match(text)).Success)
        {
            return new string('t', Number(match.Groups[1].Value));
        }

        if ((match = UnitOfLength().Match(text)).Success)
        {
            return new string('u', Number(match.Groups[1].Value));
        }

        if ((match = NameOfLength().Match(text)).Success)
        {
            return (match.Groups[2].Value + " ").PadRight(Number(match.Groups[1].Value), 'x');
        }

        throw new InvalidOperationException($"\"{term}\" is not a text the F002 steps know.");
    }

    /// <summary>The values in backticks of a text such as <c>`a`, `b` and `c`</c>, in the order written.</summary>
    public static IReadOnlyList<string> ParseBacktickList(string text) =>
        Backticked().Matches(text).Select(match => match.Groups[1].Value).ToList();

    /// <summary>The quoted values of a text such as <c>"a", "b" and "c"</c>, in the order written.</summary>
    public static IReadOnlyList<string> ParseQuotedList(string text) =>
        PlainQuotedAnywhere().Matches(text).Select(match => match.Groups[1].Value).ToList();

    /// <summary>
    /// The per-email results of a text such as <c>"created" for `a` and `b` and "already exists" for `c`</c>:
    /// an email takes the quoted result text written before it.
    /// </summary>
    public static IReadOnlyList<(string Email, string Text)> ParsePerEmail(string text)
    {
        var results = new List<(string Email, string Text)>();
        string? current = null;
        foreach (Match match in QuotedOrBackticked().Matches(text))
        {
            if (match.Groups[1].Success)
            {
                current = match.Groups[1].Value;
            }
            else
            {
                results.Add((match.Groups[2].Value,
                    current ?? throw new InvalidOperationException($"No result text before `{match.Groups[2].Value}`.")));
            }
        }

        return results;
    }

    // ---------------------------------------------------------------- habit fields

    /// <summary>The segments of a field list such as "the category Body, the intent Build and the icon `moon`".</summary>
    public static IReadOnlyList<string> SplitClause(string clause) =>
        FieldSeparator().Split(clause.Trim()).Select(segment => segment.Trim()).Where(segment => segment.Length > 0).ToList();

    /// <summary>The Draft, Published and Unpublished catalog habits of a step such as "a Published `N` with ... and a Draft `M`".</summary>
    public static IReadOnlyList<(CatalogHabitState State, string Name, string Clause)> ParseHabitList(string text)
    {
        var list = CreatedInThisOrder().Replace(text.Trim(), string.Empty);
        var matches = HabitStart().Matches(list);
        if (matches.Count == 0)
        {
            throw new InvalidOperationException($"\"{text}\" names no catalog habit.");
        }

        var habits = new List<(CatalogHabitState State, string Name, string Clause)>();
        for (var index = 0; index < matches.Count; index++)
        {
            var match = matches[index];
            var clauseStart = match.Index + match.Length;
            var clauseEnd = index + 1 < matches.Count ? matches[index + 1].Index : list.Length;
            var clause = TrailingSeparator().Replace(list[clauseStart..clauseEnd], string.Empty).Trim();
            habits.Add((ParseState(match.Groups[1].Value), match.Groups[2].Value, clause));
        }

        return habits;
    }

    /// <summary>A clause after a habit name: "with ...", "whose only suggested starting goal is ..." or nothing.</summary>
    public static void ApplyHabitClause(HabitSpec spec, string clause)
    {
        var text = clause.Trim();
        if (text.Length == 0)
        {
            return;
        }

        var only = OnlyGoal().Match(text);
        if (only.Success)
        {
            spec.Goals = [ParseGoal(only.Groups[1].Value, marked: true)];
            return;
        }

        ApplyFields(spec, text);
    }

    /// <summary>What an edit step changes: fields, or a list of suggested starting goals.</summary>
    public static void ApplyEditTarget(HabitSpec spec, string target)
    {
        var text = target.Trim();
        var each = EachGoal().Match(text);
        if (each.Success)
        {
            var count = each.Groups[1].Value == "two" ? 2 : Number(each.Groups[1].Value);
            var goals = Enumerable.Range(0, count).Select(_ => ParseGoal(each.Groups[2].Value, marked: false)).ToList();
            var marks = each.Groups[3].Value;
            if (marks == "both marked")
            {
                goals.ForEach(goal => goal.IsMarked = true);
            }
            else if (marks != "none marked")
            {
                goals[Ordinal(MarkedOrdinal().Match(marks).Groups[1].Value) - 1].IsMarked = true;
            }

            spec.Goals = goals;
            return;
        }

        var two = TwoGoals().Match(text);
        if (two.Success)
        {
            spec.Goals = ParseGoalList(two.Groups[1].Value);
            return;
        }

        ApplyFields(spec, text);
    }

    /// <summary>
    /// A field list such as "with the category Mind, the intent Build, the icon `book-open`, the description "..."
    /// and the suggested starting goal Build, count 20 `pages`, 3 times per week". The suggested starting goals
    /// always come last.
    /// </summary>
    public static void ApplyFields(HabitSpec spec, string clause)
    {
        var text = clause.Trim();
        if (text.StartsWith("with ", StringComparison.Ordinal))
        {
            text = text["with ".Length..];
        }

        var fields = text;
        var goals = GoalSectionStart().Match(text);
        if (goals.Success)
        {
            ApplyGoalSection(spec, text[goals.Index..]);
            fields = TrailingSeparator().Replace(text[..goals.Index], string.Empty);
        }

        foreach (var segment in SplitClause(fields))
        {
            ApplyField(spec, segment);
        }
    }

    private static void ApplyField(HabitSpec spec, string segment)
    {
        Match match;
        if ((match = Regex.Match(segment, "^the name (.+)$")).Success)
        {
            spec.SetName(ParseTextExpression(match.Groups[1].Value));
        }
        else if (NameOfLength().IsMatch(segment))
        {
            spec.SetName(ParseTextExpression(segment));
        }
        else if ((match = Regex.Match(segment, "^the description \"(.*)\"$")).Success)
        {
            spec.SetDescription(TextValue.Of(match.Groups[1].Value));
        }
        else if ((match = Regex.Match(segment, "^(?:the description|a description that is) (.+)$")).Success)
        {
            spec.SetDescription(ParseTextExpression(match.Groups[1].Value));
        }
        else if (segment == "no description")
        {
            spec.SetDescription(TextValue.Of(null));
        }
        else if ((match = Regex.Match(segment, @"^the category (\w+)$")).Success)
        {
            spec.Category = ParseCategory(match.Groups[1].Value);
        }
        else if (segment == "no category")
        {
            spec.Category = null;
        }
        else if ((match = Regex.Match(segment, @"^the intent (\w+)$")).Success)
        {
            spec.Intent = ParseIntent(match.Groups[1].Value);
        }
        else if (segment == "no intent")
        {
            spec.Intent = null;
        }
        else if ((match = Regex.Match(segment, "^the icon `([^`]*)`$")).Success)
        {
            spec.Icon = match.Groups[1].Value;
        }
        else if (segment is "no icon" or "the icon none")
        {
            spec.Icon = null;
        }
        else
        {
            throw new InvalidOperationException($"\"{segment}\" is not a habit field the F002 steps know.");
        }
    }

    private static void ApplyGoalSection(HabitSpec spec, string section)
    {
        Match match;
        if (section == "no suggested starting goal")
        {
            spec.Goals = [];
        }
        else if ((match = Regex.Match(section, "^the (?:single )?suggested starting goal (.+)$")).Success)
        {
            // A list with one suggested starting goal has it marked (acceptance-criteria conventions).
            spec.Goals = [ParseGoal(match.Groups[1].Value, marked: true)];
        }
        else if ((match = Regex.Match(section, "^the suggested starting goals(?:, in this order:)? (.+)$")).Success)
        {
            spec.Goals = ParseGoalList(match.Groups[1].Value);
        }
        else
        {
            throw new InvalidOperationException($"\"{section}\" is not a list of suggested starting goals the F002 steps know.");
        }
    }

    /// <summary>A list such as "(1) ...; (2) ... (marked)" or "(1) ... and (2) ..., none marked".</summary>
    public static List<GoalSpec> ParseGoalList(string text)
    {
        var list = text.Trim();
        var noneMarked = list.EndsWith(", none marked", StringComparison.Ordinal);
        if (noneMarked)
        {
            list = list[..^", none marked".Length];
        }

        var goals = GoalListSeparator().Split(list)
            .Where(item => item.Trim().Length > 0)
            .Select(item => ParseGoal(item, marked: false))
            .ToList();
        if (goals.Count == 1 && !noneMarked)
        {
            goals[0].IsMarked = true;
        }

        return goals;
    }

    /// <summary>
    /// One suggested starting goal written as its fields: Build or Reduce, the measurement, then the schedule,
    /// for example "Build, count 20 `pages`, 3 times per week (marked)".
    /// </summary>
    public static GoalSpec ParseGoal(string text, bool marked)
    {
        var goalText = text.Trim();
        var goal = new GoalSpec { IsMarked = marked };
        if (goalText.EndsWith(" (marked)", StringComparison.Ordinal))
        {
            goal.IsMarked = true;
            goalText = goalText[..^" (marked)".Length];
        }

        var first = goalText.IndexOf(", ", StringComparison.Ordinal);
        var second = first < 0 ? -1 : goalText.IndexOf(", ", first + 2, StringComparison.Ordinal);
        if (second < 0)
        {
            throw new InvalidOperationException($"\"{text}\" is not a suggested starting goal the F002 steps know.");
        }

        ApplyDirection(goal, goalText[..first]);
        ApplyMeasurement(goal, goalText[(first + 2)..second]);
        ApplySchedule(goal, goalText[(second + 2)..]);
        return goal;
    }

    private static void ApplyDirection(GoalSpec goal, string text) =>
        goal.BuildOrReduce = text switch
        {
            "Build" => BuildOrReduce.Build,
            "Reduce" => BuildOrReduce.Reduce,
            "no Build or Reduce" => null,
            _ => throw new InvalidOperationException($"\"{text}\" is not Build or Reduce."),
        };

    private static void ApplyMeasurement(GoalSpec goal, string text)
    {
        Match match;
        if (text == "no measurement")
        {
            goal.Measurement = null;
        }
        else if (text == "yes/no")
        {
            goal.Measurement = Measurement.YesNo;
        }
        else if ((match = Regex.Match(text, @"^yes/no with the amount (-?\d+)$")).Success)
        {
            goal.Measurement = Measurement.YesNo;
            goal.Amount = Number(match.Groups[1].Value);
        }
        else if ((match = Regex.Match(text, "^yes/no with the unit (`[^`]*`)$")).Success)
        {
            goal.Measurement = Measurement.YesNo;
            goal.CountUnit = ParseTextExpression(match.Groups[1].Value).Full;
        }
        else if ((match = Regex.Match(text, @"^count (-?\d+) (`[^`]*`)$")).Success)
        {
            goal.Measurement = Measurement.Count;
            goal.Amount = Number(match.Groups[1].Value);
            goal.CountUnit = ParseTextExpression(match.Groups[2].Value).Full;
        }
        else if ((match = Regex.Match(text, "^count with no amount and the unit (`[^`]*`)$")).Success)
        {
            goal.Measurement = Measurement.Count;
            goal.CountUnit = ParseTextExpression(match.Groups[1].Value).Full;
        }
        else if ((match = Regex.Match(text, @"^count (-?\d+) with (.+)$")).Success)
        {
            goal.Measurement = Measurement.Count;
            goal.Amount = Number(match.Groups[1].Value);
            goal.CountUnit = ParseUnitExpression(match.Groups[2].Value);
        }
        else if ((match = Regex.Match(text, @"^duration (-?\d+) (minutes|hours)$")).Success)
        {
            goal.Measurement = Measurement.Duration;
            goal.Amount = Number(match.Groups[1].Value);
            goal.DurationUnit = ParseDurationUnit(match.Groups[2].Value);
        }
        else if ((match = Regex.Match(text, "^duration with no amount and the unit (minutes|hours)$")).Success)
        {
            goal.Measurement = Measurement.Duration;
            goal.DurationUnit = ParseDurationUnit(match.Groups[1].Value);
        }
        else if ((match = Regex.Match(text, @"^duration (-?\d+) with no unit$")).Success)
        {
            goal.Measurement = Measurement.Duration;
            goal.Amount = Number(match.Groups[1].Value);
        }
        else
        {
            throw new InvalidOperationException($"\"{text}\" is not a measurement the F002 steps know.");
        }
    }

    private static void ApplySchedule(GoalSpec goal, string text)
    {
        Match match;
        if (text == "no schedule")
        {
            goal.ScheduleKind = null;
        }
        else if (text == "fixed days with no day")
        {
            goal.ScheduleKind = ScheduleKind.FixedDays;
            goal.Days = [];
        }
        else if ((match = Regex.Match(text, "^fixed days? (.+)$")).Success)
        {
            goal.ScheduleKind = ScheduleKind.FixedDays;
            goal.Days = ParseDays(match.Groups[1].Value);
        }
        else if ((match = Regex.Match(text, @"^(-?\d+) times? per (day|week)$")).Success)
        {
            goal.ScheduleKind = ScheduleKind.Times;
            goal.Times = Number(match.Groups[1].Value);
            goal.Period = match.Groups[2].Value == "day" ? SchedulePeriod.Day : SchedulePeriod.Week;
        }
        else if ((match = Regex.Match(text, @"^(-?\d+) times with no period$")).Success)
        {
            goal.ScheduleKind = ScheduleKind.Times;
            goal.Times = Number(match.Groups[1].Value);
        }
        else
        {
            throw new InvalidOperationException($"\"{text}\" is not a schedule the F002 steps know.");
        }
    }

    /// <summary>Days such as "Monday to Friday", "Monday, Wednesday and Friday" or "Saturday".</summary>
    public static HashSet<DayOfWeek> ParseDays(string text)
    {
        var days = TrailingNote().Replace(text.Trim(), string.Empty);
        var range = Regex.Match(days, @"^(\w+) to (\w+)$");
        if (range.Success)
        {
            var from = WeekOrder.ToList().IndexOf(Enum.Parse<DayOfWeek>(range.Groups[1].Value));
            var to = WeekOrder.ToList().IndexOf(Enum.Parse<DayOfWeek>(range.Groups[2].Value));
            return [.. WeekOrder.Skip(from).Take(to - from + 1)];
        }

        return [.. Regex.Split(days, @",\s*|\s+and\s+").Select(day => Enum.Parse<DayOfWeek>(day.Trim()))];
    }

    public static CatalogFilter ParseFilter(string text)
    {
        Category? category = null;
        Intent? intent = null;
        string? search = null;
        foreach (var segment in SplitClause(text))
        {
            Match match;
            if ((match = Regex.Match(segment, "^the search (.+)$")).Success)
            {
                search = ParseTextExpression(match.Groups[1].Value).Full;
            }
            else if ((match = Regex.Match(segment, @"^the category (\w+)$")).Success)
            {
                category = ParseCategory(match.Groups[1].Value);
            }
            else if ((match = Regex.Match(segment, @"^the intent (\w+)$")).Success)
            {
                intent = ParseIntent(match.Groups[1].Value);
            }
            else
            {
                throw new InvalidOperationException($"\"{segment}\" is not a browse filter the F002 steps know.");
            }
        }

        return new CatalogFilter { Category = category, Intent = intent, Search = search };
    }

    /// <summary>A category of FR-002; "none" means no category.</summary>
    public static Category? ParseCategory(string text) =>
        text == "none" ? null : Enum.Parse<Category>(text, ignoreCase: false);

    /// <summary>An intent; "none" means no intent, and "both" (browse filter) means no intent filter.</summary>
    public static Intent? ParseIntent(string text) =>
        text is "none" or "both" ? null : Enum.Parse<Intent>(text, ignoreCase: false);

    public static CatalogHabitState ParseState(string text) => Enum.Parse<CatalogHabitState>(text, ignoreCase: false);

    public static DurationUnit ParseDurationUnit(string text) => text switch
    {
        "minutes" => DurationUnit.Minutes,
        "hours" => DurationUnit.Hours,
        _ => throw new InvalidOperationException($"\"{text}\" is not minutes or hours."),
    };

    /// <summary>"first", "second", ... as a position counted from 1.</summary>
    public static int Ordinal(string text) => text switch
    {
        "first" => 1,
        "second" => 2,
        "third" => 3,
        "fourth" => 4,
        "fifth" => 5,
        "sixth" => 6,
        _ => throw new InvalidOperationException($"\"{text}\" is not an ordinal the F002 steps know."),
    };

    private static int Number(string text) => int.Parse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);

    // ---------------------------------------------------------------- outcomes

    private static readonly Dictionary<string, string> OutcomeNames = new(StringComparer.Ordinal)
    {
        ["You are not signed in."] = "NotSignedIn",
        ["Only administrators can manage the catalog."] = "NotAdministrator",
        ["This catalog habit does not exist."] = "NotFound",
        ["This custom habit does not exist."] = "NotFound",
        ["This habit is not in the catalog."] = "NotInCatalog",
        ["This catalog habit is not published."] = "NotPublished",
        ["A catalog habit with this name already exists."] = "NameTaken",
        ["You already have a custom habit with this name."] = "NameTaken",
        ["Custom habits are for registered users."] = "NotForAdministrators",
        ["Enter a name."] = "NameMissing",
        ["The name can have at most 60 characters."] = "NameTooLong",
        ["The description can have at most 500 characters."] = "DescriptionTooLong",
        ["Choose a category."] = "CategoryMissing",
        ["Choose Build or Reduce."] = "IntentMissing",
        ["Choose an icon."] = "IconMissing",
        ["A habit can have at most 5 suggested starting goals."] = "TooManySuggestedStartingGoals",
        ["Mark one suggested starting goal for the card."] = "SuggestedStartingGoalNotMarked",
        ["Choose how it is measured."] = "MeasurementMissing",
        ["A yes/no suggested starting goal has no amount or unit."] = "YesNoHasAmountOrUnit",
        ["Enter a whole number from 1 to 999."] = "BuildAmountOutOfRange",
        ["Enter a whole number from 0 to 999."] = "ReduceAmountOutOfRange",
        ["Enter a unit, for example pages."] = "CountUnitMissing",
        ["The unit can have at most 20 characters."] = "CountUnitTooLong",
        ["Choose minutes or hours."] = "DurationUnitMissing",
        ["Choose fixed days or how many times."] = "ScheduleMissing",
        ["Choose at least one day."] = "DaysMissing",
        ["Enter a number of times from 1 to 20."] = "TimesOutOfRange",
        ["Choose per day or per week."] = "PeriodMissing",
        ["no demo password configured"] = "NoPassword",
        ["demo password does not meet the password rule"] = "PasswordRuleNotMet",
    };

    /// <summary>
    /// The named outcome of <paramref name="outcomeType"/> that carries <paramref name="text"/> (ADR 0005: every
    /// refusal is a named outcome with the literal text of the spec). "Choose Build or Reduce." is the habit's
    /// intent check, or, for a result about one suggested starting goal (one with a position), its Build or Reduce
    /// check (FR-005, FR-007).
    /// </summary>
    public static Enum OutcomeFor(Type outcomeType, string text, bool aboutSuggestedStartingGoal)
    {
        var name = aboutSuggestedStartingGoal && text == "Choose Build or Reduce."
            ? "BuildOrReduceMissing"
            : OutcomeNames.TryGetValue(text, out var known)
                ? known
                : throw new InvalidOperationException($"The spec gives no result with the text \"{text}\".");
        if (!Enum.IsDefined(outcomeType, name))
        {
            throw new InvalidOperationException($"{outcomeType.Name} has no outcome for \"{text}\".");
        }

        return (Enum)Enum.Parse(outcomeType, name);
    }

    public static DemoAccountOutcome DemoAccountOutcomeFor(string text) => text switch
    {
        "created" => DemoAccountOutcome.Created,
        "already exists" => DemoAccountOutcome.AlreadyExists,
        _ => throw new InvalidOperationException($"FR-016 gives no demo account result with the text \"{text}\"."),
    };

    public static DemoCatalogOutcome DemoCatalogOutcomeFor(string text) => text switch
    {
        "seed catalog loaded" => DemoCatalogOutcome.SeedCatalogLoaded,
        "catalog not empty" => DemoCatalogOutcome.CatalogNotEmpty,
        _ => throw new InvalidOperationException($"FR-030 gives no catalog result with the text \"{text}\"."),
    };

    public static DemoCustomHabitsOutcome DemoCustomHabitsOutcomeFor(string text) => text switch
    {
        "demo custom habits added" => DemoCustomHabitsOutcome.Added,
        _ => throw new InvalidOperationException($"FR-031 gives no custom-habit result with the text \"{text}\"."),
    };

    // ---------------------------------------------------------------- regular expressions

    [GeneratedRegex(@"\s+\([^()`]*\)$")]
    private static partial Regex TrailingNote();

    [GeneratedRegex(@"\s+followed by\s+|\s+and\s+")]
    private static partial Regex UnitJoin();

    [GeneratedRegex("^`\"(.*)\"`$")]
    private static partial Regex BacktickQuoted();

    [GeneratedRegex(@"^`([^`]*)` followed by `(.)` up to (\d+) characters(?: in all)?$")]
    private static partial Regex FollowedByUpTo();

    [GeneratedRegex("`([^`]*)`")]
    private static partial Regex Backticked();

    [GeneratedRegex("^\"(.*)\"$")]
    private static partial Regex PlainQuoted();

    [GeneratedRegex("\"([^\"]*)\"")]
    private static partial Regex PlainQuotedAnywhere();

    [GeneratedRegex("\"([^\"]*)\"|`([^`]+)`")]
    private static partial Regex QuotedOrBackticked();

    [GeneratedRegex(@"^a text of (\d+) characters$")]
    private static partial Regex TextOfLength();

    [GeneratedRegex(@"^a unit of (\d+) characters$")]
    private static partial Regex UnitOfLength();

    [GeneratedRegex(@"^a name of (\d+) characters with the prefix `([^`]*)`$")]
    private static partial Regex NameOfLength();

    [GeneratedRegex(@"(?:,\s*(?:and\s+)?|\s+and\s+)(?=(?:the (?:name|description|category|intent|icon|search|state|suggested starting goal)\b|no (?:category|intent|icon|description|suggested starting goal)\b|a name of\b|a description that\b))")]
    private static partial Regex FieldSeparator();

    [GeneratedRegex(@"(?:the (?:single )?suggested starting goals?|no suggested starting goal)\b")]
    private static partial Regex GoalSectionStart();

    [GeneratedRegex(@"(?:\s*,\s*and|\s*,|\s+and)\s*$")]
    private static partial Regex TrailingSeparator();

    [GeneratedRegex(@",\s*created in this order$")]
    private static partial Regex CreatedInThisOrder();

    [GeneratedRegex(@"\ban? (Published|Draft|Unpublished) `([^`]+)`")]
    private static partial Regex HabitStart();

    [GeneratedRegex("^whose only suggested starting goal is (.+)$")]
    private static partial Regex OnlyGoal();

    [GeneratedRegex(@"^(\d+|two) suggested starting goals, each (.+), with (none marked|both marked|the \w+ marked)$")]
    private static partial Regex EachGoal();

    [GeneratedRegex(@"^the (\w+) marked$")]
    private static partial Regex MarkedOrdinal();

    [GeneratedRegex("^two suggested starting goals: (.+)$")]
    private static partial Regex TwoGoals();

    [GeneratedRegex(@"(?:^|\s*;\s*|\s+and\s+)\(\d+\)\s*")]
    private static partial Regex GoalListSeparator();
}
