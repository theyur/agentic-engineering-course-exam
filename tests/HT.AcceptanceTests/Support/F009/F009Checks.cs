using System.Text.RegularExpressions;
using HT.Application.Goals;
using HT.Application.Records;
using HT.Application.Reports;
using HT.Domain.Evaluation;
using HT.Domain.Records;

namespace HT.AcceptanceTests.Support.F009;

/// <summary>Assertions shared by the F009 Then steps. Every expected value comes from the Gherkin text.</summary>
public static class F009Checks
{
    // ------------------------------------------------------------ settings

    public static F009Text.SettingsShape ShapeOf(GoalDetails goal) => new(
        goal.BuildOrReduce,
        goal.Measurement,
        goal.Amount,
        goal.CountUnit,
        goal.DurationUnit,
        goal.ScheduleKind,
        F009Text.DaysText(goal.Days),
        goal.Times,
        goal.Period);

    public static F009Text.SettingsShape ShapeOf(GoalSettingsView settings) => new(
        settings.BuildOrReduce,
        settings.Measurement,
        settings.Amount,
        settings.CountUnit,
        settings.DurationUnit,
        settings.ScheduleKind,
        F009Text.DaysText(settings.Days),
        settings.Times,
        settings.Period);

    public static F009Text.SettingsShape ShapeOf(NewAttemptValues values) => new(
        values.BuildOrReduce,
        values.Measurement,
        values.Amount,
        values.CountUnit,
        values.DurationUnit,
        values.ScheduleKind,
        F009Text.DaysText(values.Days),
        values.Times,
        values.Period);

    /// <summary>
    /// "the settings in effect S, from D": the settings version in effect on the goal's current date (FR-005), the date
    /// from which it applies, and its time zone when S writes one.
    /// </summary>
    public static void InEffect(GoalDetails goal, string settingsText, DateOnly from)
    {
        var spec = F009Text.Settings(settingsText);
        Assert.Equal(spec.Expected(), ShapeOf(goal));
        Assert.Equal(from, goal.TimeZoneAppliesFrom);
        if (spec.TimeZone is { } zone)
        {
            Assert.Equal(zone, goal.TimeZone);
        }
    }

    /// <summary>"the later settings S, from D": the version that takes effect after the goal's current date (FR-005).</summary>
    public static void Later(GoalDetails goal, string settingsText, DateOnly from)
    {
        var spec = F009Text.Settings(settingsText);
        var later = goal.Later ?? throw new Xunit.Sdk.XunitException($"The goal has no later settings; expected {settingsText}, from {from:yyyy-MM-dd}.");
        Assert.Equal(spec.Expected(), ShapeOf(later));
        Assert.Equal(from, later.AppliesFrom);
        if (spec.TimeZone is { } zone)
        {
            Assert.Equal(zone, later.TimeZone);
        }
    }

    public static void NoLater(GoalDetails goal) =>
        Assert.True(goal.Later is null, $"The goal has later settings from {goal.Later?.AppliesFrom:yyyy-MM-dd}; expected none.");

    /// <summary>"the starting values of a new attempt are S[ and the start date D]" (FR-011).</summary>
    public static void StartingValues(NewAttemptValues values, string settingsText, DateOnly? startDate)
    {
        var spec = F009Text.Settings(settingsText);
        Assert.Equal(spec.Expected(), ShapeOf(values));
        if (startDate is { } start)
        {
            Assert.Equal(start, values.StartDate);
        }
    }

    // ------------------------------------------------------------ the state of a read goal

    private static readonly Regex StateFact = new(
        "\\G(?:, and |, | and | )?(?:with )?(?:" +
        "the ending date (?<ending>" + F009Text.D + ")" +
        "|the last date (?<last>" + F009Text.D + ")" +
        "|the outcome (?<outcome>" + F009Text.Outcomes + ")" +
        "|the note (?<note>\"[^\"]*\"|`[^`]` repeated \\d+ times)" +
        "|(?<nonote>no note)" +
        "|its latest attempt as attempt (?<latestAs>\\d+) with the start date (?<latestStart>" + F009Text.D + ")" +
        "|its latest attempt attempt (?<latest>\\d+)" +
        "|the goal's start date (?<goalStart>" + F009Text.D + ")" +
        "|the name (?<name>`[^`]+`)" +
        ")",
        RegexOptions.CultureInvariant);

    /// <summary>
    /// "it ended with the ending date D, the last date D, the outcome O and the note "N"", "it not ended, with its latest
    /// attempt attempt 1", "it not ended, its latest attempt as attempt 2 with the start date D, and the goal's start
    /// date D", …: every fact written is checked against the read goal (FR-005, FR-017).
    /// </summary>
    public static void GoalState(GoalDetails goal, string clause)
    {
        var head = Regex.Match(clause.Trim(), "^(?<ended>not ended|ended)(?<rest>.*)$");
        if (!head.Success)
        {
            throw new InvalidOperationException($"Cannot read \"{clause}\".");
        }

        Assert.Equal(head.Groups["ended"].Value == "ended", goal.IsEnded);
        var rest = head.Groups["rest"].Value;
        var position = 0;
        while (position < rest.Length)
        {
            var match = StateFact.Match(rest, position);
            if (!match.Success || match.Length == 0)
            {
                throw new InvalidOperationException($"Cannot read \"{rest[position..]}\" in \"{clause}\".");
            }

            if (match.Groups["ending"].Success)
            {
                Assert.Equal(F009Text.Date(match.Groups["ending"].Value), goal.EndingDate);
            }
            else if (match.Groups["last"].Success)
            {
                Assert.Equal(F009Text.Date(match.Groups["last"].Value), goal.LastDate);
            }
            else if (match.Groups["outcome"].Success)
            {
                Assert.Equal(F009Text.OutcomeOf(match.Groups["outcome"].Value), goal.Outcome);
            }
            else if (match.Groups["note"].Success)
            {
                Assert.Equal(F009Text.Text(match.Groups["note"].Value), goal.EndingNote);
            }
            else if (match.Groups["nonote"].Success)
            {
                Assert.Null(goal.EndingNote);
            }
            else if (match.Groups["latestAs"].Success)
            {
                Assert.Equal(F009Text.Int(match.Groups["latestAs"].Value), goal.AttemptNumber);
                Assert.Equal(F009Text.Date(match.Groups["latestStart"].Value), goal.AttemptStartDate);
            }
            else if (match.Groups["latest"].Success)
            {
                Assert.Equal(F009Text.Int(match.Groups["latest"].Value), goal.AttemptNumber);
            }
            else if (match.Groups["goalStart"].Success)
            {
                Assert.Equal(F009Text.Date(match.Groups["goalStart"].Value), goal.StartDate);
            }
            else if (match.Groups["name"].Success)
            {
                Assert.Equal(F009Text.Text(match.Groups["name"].Value), goal.Name);
            }

            position = match.Index + match.Length;
        }
    }

    // ------------------------------------------------------------ history entries

    private static string RecordsText(IEnumerable<(YesNoAnswer? Answer, int? Amount)> records) =>
        string.Join(", ", records.Select(record => record.Answer?.ToString() ?? record.Amount?.ToString() ?? "?").Order(StringComparer.Ordinal));

    private static string RecordsText(IEnumerable<RecordView> records) =>
        RecordsText(records.Select(record => (record.Answer, record.Amount)));

    /// <summary>The entry of a span, which must exist exactly once.</summary>
    public static HistoryEntry EntryOf(IReadOnlyList<HistoryEntry> entries, DateOnly first, DateOnly last, string what)
    {
        var found = entries.Where(entry => entry.FirstDate == first && entry.LastDate == last).ToList();
        Assert.True(found.Count == 1, $"{what} has {found.Count} entries for {first:yyyy-MM-dd} – {last:yyyy-MM-dd}; expected one.");
        return found[0];
    }

    /// <summary>Every entry written is in the list with its state and, as far as written, its records.</summary>
    public static void Entries(IReadOnlyList<HistoryEntry> entries, IReadOnlyList<F009Text.EntrySpec> specs, string what)
    {
        foreach (var spec in specs)
        {
            foreach (var (first, last) in spec.Spans)
            {
                var entry = EntryOf(entries, first, last, what);
                Assert.True(
                    entry.State == spec.State,
                    $"{what}: {first:yyyy-MM-dd} – {last:yyyy-MM-dd} is {entry.State}; expected {spec.State} (\"{spec.Text}\").");
                if (spec.Records is { } records)
                {
                    Assert.Equal(RecordsText(records), RecordsText(entry.Records));
                }
            }
        }
    }

    /// <summary>The list holds exactly the entries written (no other entry), each as written.</summary>
    public static void ExactEntries(IReadOnlyList<HistoryEntry> entries, IReadOnlyList<F009Text.EntrySpec> specs, string what)
    {
        var expected = specs.SelectMany(spec => spec.Spans).OrderBy(span => span.First).ToList();
        var actual = entries.Select(entry => (entry.FirstDate, entry.LastDate)).OrderBy(span => span.FirstDate).ToList();
        Assert.Equal(expected, actual);
        Entries(entries, specs, what);
    }

    /// <summary>The list holds exactly the entries written, in the order written (newest first), each as written.</summary>
    public static void OrderedEntries(IReadOnlyList<HistoryEntry> entries, IReadOnlyList<F009Text.EntrySpec> specs)
    {
        var expected = specs.SelectMany(spec => spec.Spans.Select(span => (span.First, span.Last, spec.State, Records: spec.Records is null ? null : RecordsText(spec.Records)))).ToList();
        Assert.Equal(expected.Count, entries.Count);
        for (var index = 0; index < expected.Count; index++)
        {
            var (first, last, state, records) = expected[index];
            var entry = entries[index];
            Assert.Equal((first, last, state), (entry.FirstDate, entry.LastDate, entry.State));
            if (records is not null)
            {
                Assert.Equal(records, RecordsText(entry.Records));
            }
        }
    }

    // ------------------------------------------------------------ attempts

    public static void Attempt(AttemptView attempt, F009Text.AttemptSpec spec)
    {
        Assert.Equal(spec.Number, attempt.Number);
        if (spec.Name is { } name)
        {
            Assert.Equal(name, attempt.Name);
        }

        if (spec.StartDate is { } start)
        {
            Assert.Equal(start, attempt.StartDate);
        }

        if (spec.NotEnded)
        {
            Assert.True(attempt.EndingDate is null, $"Attempt {attempt.Number} ended on {attempt.EndingDate:yyyy-MM-dd}; expected not ended.");
            Assert.Null(attempt.Outcome);
        }

        if (spec.EndingDate is { } ending)
        {
            Assert.Equal(ending, attempt.EndingDate);
        }

        if (spec.LastDate is { } last)
        {
            Assert.Equal(last, attempt.LastDate);
        }

        if (spec.Outcome is { } outcome)
        {
            Assert.Equal(outcome, attempt.Outcome);
        }

        if (spec.NoteWritten)
        {
            Assert.Equal(spec.Note, attempt.EndingNote);
        }

        if (spec.Consistency is { } consistency)
        {
            Assert.Equal(consistency, attempt.Stats.ConsistencyText);
        }

        if (spec.CurrentStreak is { } current)
        {
            Assert.Equal(current, attempt.Stats.CurrentStreakText);
        }

        if (spec.LongestStreak is { } longest)
        {
            Assert.Equal(longest, attempt.Stats.LongestStreakText);
        }
    }

    public static AttemptView AttemptNumbered(IReadOnlyList<AttemptView> attempts, int number) =>
        Assert.Single(attempts, attempt => attempt.Number == number);

    /// <summary>
    /// A list of attempts: with "exactly" the list has no other attempt; with "in this order" the attempts come in the
    /// order written (the latest first, FR-014); each attempt has the fields written.
    /// </summary>
    public static void Attempts(IReadOnlyList<AttemptView> attempts, string text, bool exactly, bool inOrder)
    {
        var specs = F009Text.Attempts(text);
        if (exactly)
        {
            Assert.Equal(specs.Count, attempts.Count);
        }

        if (inOrder)
        {
            var listed = attempts.Select(attempt => attempt.Number).Where(number => specs.Any(spec => spec.Number == number)).ToList();
            Assert.Equal(specs.Select(spec => spec.Number).ToList(), listed);
        }

        foreach (var spec in specs)
        {
            Attempt(AttemptNumbered(attempts, spec.Number), spec);
        }
    }

    // ------------------------------------------------------------ goal history

    public static void GoalHistory(IReadOnlyList<EndedAttemptView> entries, string text, bool exactly, bool inOrder)
    {
        var specs = F009Text.HistoryEntries(text);
        if (exactly)
        {
            Assert.Equal(specs.Count, entries.Count);
        }

        var matched = new List<int>();
        foreach (var spec in specs)
        {
            var index = -1;
            for (var candidate = 0; candidate < entries.Count; candidate++)
            {
                if (entries[candidate].Name == spec.Name
                    && (spec.Attempt is null || entries[candidate].AttemptNumber == spec.Attempt)
                    && !matched.Contains(candidate))
                {
                    index = candidate;
                    break;
                }
            }

            Assert.True(index >= 0, $"The goal history has no entry \"{spec.Text}\"; it has: {string.Join("; ", entries.Select(entry => $"{entry.Name}, attempt {entry.AttemptNumber}"))}.");
            matched.Add(index);
            var entry = entries[index];
            if (spec.EndingDate is { } ending)
            {
                Assert.Equal(ending, entry.EndingDate);
            }

            if (spec.Outcome is { } outcome)
            {
                Assert.Equal(outcome, entry.Outcome);
            }

            if (spec.NoteWritten)
            {
                Assert.Equal(spec.Note, entry.EndingNote);
            }

            if (spec.Latest is { } latest)
            {
                Assert.Equal(latest, entry.IsLatestAttempt);
            }
        }

        if (inOrder)
        {
            Assert.Equal(matched.Order().ToList(), matched);
        }
    }

    // ------------------------------------------------------------ figures

    private static readonly Regex StatFact = new(
        "\\G(?:, and |, | and )?the (?<what>consistency|current streak|longest streak) \"(?<value>[^\"]*)\"",
        RegexOptions.CultureInvariant);

    /// <summary>"the consistency "X", the current streak "Y" and the longest streak "Z"" (any of the three, in any order).</summary>
    public static void Stats(GoalStatsView stats, string text)
    {
        var position = 0;
        var checkedAny = false;
        while (position < text.Length)
        {
            var match = StatFact.Match(text, position);
            if (!match.Success || match.Length == 0)
            {
                throw new InvalidOperationException($"Cannot read \"{text[position..]}\" in \"{text}\".");
            }

            var value = match.Groups["value"].Value;
            switch (match.Groups["what"].Value)
            {
                case "consistency":
                    Assert.Equal(value, stats.ConsistencyText);
                    break;
                case "current streak":
                    Assert.Equal(value, stats.CurrentStreakText);
                    break;
                default:
                    Assert.Equal(value, stats.LongestStreakText);
                    break;
            }

            checkedAny = true;
            position = match.Index + match.Length;
        }

        Assert.True(checkedAny, $"\"{text}\" names no figure.");
    }

    // ------------------------------------------------------------ what can be recorded now and the Today list

    /// <summary>An entry of what can be recorded now (a goal's entries) as written.</summary>
    public static void RecordableEntry(GoalHistory recordable, F009Text.TupleSpec spec)
    {
        var entry = EntryOf(recordable.Entries, spec.First, spec.Last, "What can be recorded now");
        Assert.Equal(spec.State, entry.State);
        if (spec.Attempt is { } attempt)
        {
            Assert.Equal(attempt, entry.AttemptNumber);
        }

        if (spec.Latest is { } latest)
        {
            Assert.Equal(latest, entry.IsLatestAttempt);
            if (!latest)
            {
                // FR-016: an entry of an attempt that is not the latest gives that attempt's name.
                Assert.Equal(spec.Goal, entry.AttemptName);
            }
        }

        if (spec.BuildOrReduce is { } kind)
        {
            var settings = entry.Settings ?? throw new Xunit.Sdk.XunitException($"The entry {spec.Text} gives no settings (Build or Reduce).");
            Assert.Equal(kind, settings.BuildOrReduce);
        }
    }

    /// <summary>An entry of the dashboard's Today list as written: the goal field is the entry's goal (or attempt) name.</summary>
    public static void TodayEntry(IReadOnlyList<TodayEntry> today, F009Goal goal, F009Text.TupleSpec spec)
    {
        var found = today.Where(entry => entry.GoalId == goal.Id && entry.Entry.FirstDate == spec.First && entry.Entry.LastDate == spec.Last).ToList();
        Assert.True(found.Count == 1, $"The Today list has {found.Count} entries {spec.Text}; expected one.");
        var item = found[0];
        Assert.Equal(spec.Goal, item.GoalName);
        Assert.Equal(spec.State, item.Entry.State);
        if (spec.Attempt is { } attempt)
        {
            Assert.Equal(attempt, item.Entry.AttemptNumber);
        }

        if (spec.Latest is { } latest)
        {
            Assert.Equal(latest, item.Entry.IsLatestAttempt);
        }

        if (spec.GoalEnded is { } ended)
        {
            Assert.Equal(ended, item.GoalEnded);
        }

        if (spec.BuildOrReduce is { } kind)
        {
            Assert.Equal(kind, item.BuildOrReduce);
        }
    }

    public static bool Covers(DateOnly first, DateOnly last, DateOnly date) => first <= date && date <= last;

    public static EvaluationState StateOf(string text) => F009Text.StateOf(text);
}
