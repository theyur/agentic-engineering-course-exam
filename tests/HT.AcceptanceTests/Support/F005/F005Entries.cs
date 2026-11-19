using System.Text.RegularExpressions;
using HT.Application.Records;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Records;

namespace HT.AcceptanceTests.Support.F005;

/// <summary>
/// What a check sees of one history entry, or of all entries of a read together (records in the history's order,
/// the notes that are set). Facts about the state, grace end, progress or correction need one entry.
/// </summary>
public sealed record F005View(HistoryEntry? Entry, IReadOnlyList<RecordView> Records, IReadOnlyList<string> Notes, GoalScheduleKind Kind)
{
    public HistoryEntry One => Entry ?? throw new InvalidOperationException("This fact is about one entry, but the read gave several.");

    public static F005View Of(HistoryEntry entry, GoalScheduleKind kind) =>
        new(entry, entry.Records, entry.Note is null ? [] : [entry.Note], kind);

    public static F005View Of(IReadOnlyList<HistoryEntry> entries, GoalScheduleKind kind) =>
        new(
            entries.Count == 1 ? entries[0] : null,
            entries.SelectMany(entry => entry.Records).ToList(),
            entries.Where(entry => entry.Note is not null).Select(entry => entry.Note!).ToList(),
            kind);
}

/// <summary>The date part of an entry as a step writes it: "D" or "D1 to D2".</summary>
public sealed record F005Head(DateOnly First, DateOnly Last);

/// <summary>
/// Reads and checks history entries as acceptance.md ("History entries", "Progress") writes them: the date (a
/// per-week period: its first and last date), "from" its start "to" its end, its state, "grace ends T" or "no grace
/// end", its records as (date, value) in the history's order or "no record", its progress, its note or "no note", and
/// "correction" or "no correction". An item checks only what it writes.
/// </summary>
public static class F005Entries
{
    private const string D = F005Text.D;

    private static readonly Regex Head = new($@"^({D})(?: to ({D}))?(?: from `([^`]+)` to `([^`]+)`)?", RegexOptions.CultureInvariant);
    private static readonly Regex HeadSeparator = new(@"^(?:, and |, |: | with | )", RegexOptions.CultureInvariant);
    private static readonly Regex FactSeparator = new(@"^(?:, and |, | and )", RegexOptions.CultureInvariant);
    private static readonly Regex Tuple = new($@"^({D}), (Yes|No|-?\d+)$", RegexOptions.CultureInvariant);
    private static readonly Regex NoteItem = new(
        $@"^({D})(?: to ({D}))?, ""([^""]*)""(?:, (Upcoming|Due|Pending|Met|Failed|Missed))?$",
        RegexOptions.CultureInvariant);

    private static readonly (Regex Pattern, Func<Match, Action<F005View>> Check)[] Clauses =
    [
        (Clause("(Upcoming|Due|Pending|Met|Failed|Missed)"), m => view =>
            Assert.Equal(Enum.Parse<EvaluationState>(m.Groups[1].Value), view.One.State)),
        (Clause(@"grace ends `([^`]+)`(?: \([^)]*\))?"), m => view =>
            Assert.Equal<DateTimeOffset?>(F005Text.Instant(m.Groups[1].Value), view.One.GraceEnds)),
        (Clause("no grace end"), _ => view => Assert.Null(view.One.GraceEnds)),
        (Clause(@"records (\([^()]*\)(?:, \([^()]*\))*)(?: only)?"), m => view => RecordsAre(m.Groups[1].Value, view.Records)),
        (Clause("no record"), _ => view => Assert.Empty(view.Records)),
        (Clause(@"one record, \(([^()]*)\)(?:, with (no amount|neither Yes nor No))?"), m => view =>
        {
            var record = Assert.Single(view.Records);
            RecordIs(m.Groups[1].Value, record);
            switch (m.Groups[2].Value)
            {
                case "no amount":
                    Assert.Null(record.Amount);
                    break;
                case "neither Yes nor No":
                    Assert.Null(record.Answer);
                    break;
            }
        }),
        (Clause(@"one record of (Yes|No|-?\d+)"), m => view => ValueIs(m.Groups[1].Value, Assert.Single(view.Records))),
        (Clause(@"(\d+) records(?:, exactly one of them \(([^()]*)\))?"), m => view =>
        {
            Assert.Equal(F005Text.Int(m.Groups[1].Value), view.Records.Count);
            if (m.Groups[2].Success)
            {
                Assert.Single(view.Records, record => Matches(m.Groups[2].Value, record));
            }
        }),
        (Clause(@"progress total (\d+) of at most (\d+)"), m => view => ProgressIs(view, ProgressKind.Total, m)),
        (Clause(@"progress (\d+) of at most (\d+)"), m => view => ProgressIs(view, ProgressKind.YesRecords, m)),
        (Clause(@"progress (\d+) of (\d+)(?: minutes| times)?"), m => view => ProgressIs(
            view,
            view.Kind == GoalScheduleKind.CommitmentBased ? ProgressKind.RecordedValue : ProgressKind.RecordsReachingAmount,
            m)),
        (Clause("no note"), _ => view => Assert.Empty(view.Notes)),
        (Clause(@"a text of (\d+) characters as the note"), m => view =>
            Assert.Equal(new string('x', F005Text.Int(m.Groups[1].Value)), Assert.Single(view.Notes))),
        (Clause("(?:the )?note \"([^\"]*)\""), m => view => Assert.Equal(m.Groups[1].Value, Assert.Single(view.Notes))),
        (Clause("no correction"), _ => view => Assert.False(view.One.IsCorrection)),
        (Clause("correction"), _ => view => Assert.True(view.One.IsCorrection)),
    ];

    // ------------------------------------------------------------ reading

    /// <summary>An entry such as "2026-11-18 from `T1` to `T2`, Due, grace ends `T3`, records (2026-11-18, 12)".</summary>
    public static (F005Head Head, List<Action<F005View>> Facts) Entry(string text)
    {
        var trimmed = text.Trim();
        var head = Head.Match(trimmed);
        if (!head.Success)
        {
            throw new InvalidOperationException($"\"{text}\" does not start with an entry's date.");
        }

        var first = F005Text.Date(head.Groups[1].Value);
        var last = head.Groups[2].Success ? F005Text.Date(head.Groups[2].Value) : first;
        var facts = new List<Action<F005View>>();
        if (head.Groups[3].Success)
        {
            var starts = F005Text.Instant(head.Groups[3].Value);
            var ends = F005Text.Instant(head.Groups[4].Value);
            facts.Add(view => Assert.Equal(starts, view.One.Starts));
            facts.Add(view => Assert.Equal(ends, view.One.Ends));
        }

        var rest = trimmed[head.Length..];
        if (rest.Length > 0)
        {
            var separator = HeadSeparator.Match(rest);
            if (!separator.Success)
            {
                throw new InvalidOperationException($"Cannot read \"{rest}\" of \"{text}\".");
            }

            facts.AddRange(Facts(rest[separator.Length..]));
        }

        return (new F005Head(first, last), facts);
    }

    /// <summary>Facts such as "Met, records (2026-11-18, 20), correction" or "50 records and no note".</summary>
    public static List<Action<F005View>> Facts(string text)
    {
        var facts = new List<Action<F005View>>();
        var rest = text.Trim();
        var first = true;
        while (rest.Length > 0)
        {
            if (!first)
            {
                var separator = FactSeparator.Match(rest);
                if (!separator.Success)
                {
                    throw new InvalidOperationException($"Cannot read the facts \"{rest}\" of \"{text}\".");
                }

                rest = rest[separator.Length..];
            }

            first = false;
            var read = false;
            foreach (var (pattern, check) in Clauses)
            {
                var match = pattern.Match(rest);
                if (match.Success)
                {
                    facts.Add(check(match));
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

    // ------------------------------------------------------------ checking

    /// <summary>Checks one entry against what a step writes about it, its dates included.</summary>
    public static void Check(HistoryEntry entry, string text, GoalScheduleKind kind)
    {
        var (head, facts) = Entry(text);
        Assert.Equal(head.First, entry.FirstDate);
        Assert.Equal(head.Last, entry.LastDate);
        var view = F005View.Of(entry, kind);
        foreach (var fact in facts)
        {
            fact(view);
        }
    }

    /// <summary>The one entry of a read with the dates a step writes, checked against the rest of what it writes.</summary>
    public static void CheckFound(IReadOnlyList<HistoryEntry> entries, string text, GoalScheduleKind kind)
    {
        var (head, _) = Entry(text);
        var entry = Assert.Single(entries, item => item.FirstDate == head.First && item.LastDate == head.Last);
        Check(entry, text, kind);
    }

    /// <summary>"E1; E2; and E3" (or "E1 and E2"): exactly these entries, in this order.</summary>
    public static void CheckOrdered(IReadOnlyList<HistoryEntry> entries, string list, GoalScheduleKind kind)
    {
        var items = F005Text.Items(list);
        Assert.Equal(items.Count, entries.Count);
        for (var index = 0; index < items.Count; index++)
        {
            Check(entries[index], items[index], kind);
        }
    }

    /// <summary>Facts that hold for all entries of a read together (a one-entry read: for that entry).</summary>
    public static void CheckAll(IReadOnlyList<HistoryEntry> entries, string facts, GoalScheduleKind kind)
    {
        var view = F005View.Of(entries, kind);
        foreach (var fact in Facts(facts))
        {
            fact(view);
        }
    }

    /// <summary>
    /// What "the history ... gives" or "it gives" says about a read: "exactly one entry: E", "exactly one entry, D",
    /// "no entry", "the entry E", "in this order: L", "N entries, the first D1 and the last D2", "E1 with … and E2
    /// with …", or facts about all entries together ("no record", "50 records and no note").
    /// </summary>
    public static void CheckBody(IReadOnlyList<HistoryEntry> entries, string body, GoalScheduleKind kind)
    {
        var text = body.Trim();
        Match match;
        if ((match = Regex.Match(text, "^exactly one entry: (.+)$")).Success)
        {
            Check(Assert.Single(entries), match.Groups[1].Value, kind);
            return;
        }

        if ((match = Regex.Match(text, $"^exactly one entry, ({D})$")).Success)
        {
            var date = F005Text.Date(match.Groups[1].Value);
            var entry = Assert.Single(entries);
            Assert.Equal(date, entry.FirstDate);
            Assert.Equal(date, entry.LastDate);
            return;
        }

        if (text == "no entry")
        {
            Assert.Empty(entries);
            return;
        }

        if ((match = Regex.Match(text, "^the entry (.+)$")).Success)
        {
            CheckFound(entries, match.Groups[1].Value, kind);
            return;
        }

        if ((match = Regex.Match(text, "^in this order: (.+)$")).Success)
        {
            CheckOrdered(entries, match.Groups[1].Value, kind);
            return;
        }

        if ((match = Regex.Match(text, $@"^(\d+) entries, the first ({D}) and the last ({D})$")).Success)
        {
            Assert.Equal(F005Text.Int(match.Groups[1].Value), entries.Count);
            Assert.Equal(F005Text.Date(match.Groups[2].Value), entries[0].FirstDate);
            Assert.Equal(F005Text.Date(match.Groups[3].Value), entries[^1].FirstDate);
            return;
        }

        if (Regex.IsMatch(text, $"^{D}"))
        {
            foreach (var item in F005Text.Items(text))
            {
                CheckFound(entries, item, kind);
            }

            return;
        }

        CheckAll(entries, text, kind);
    }

    /// <summary>One recent note as a step writes it: "D, "note", State" or "D1 to D2, "note", State" (state optional).</summary>
    public static void CheckNote(RecentNote note, string text)
    {
        var match = NoteItem.Match(text.Trim());
        if (!match.Success)
        {
            throw new InvalidOperationException($"Cannot read the note \"{text}\".");
        }

        var first = F005Text.Date(match.Groups[1].Value);
        Assert.Equal(first, note.FirstDate);
        Assert.Equal(match.Groups[2].Success ? F005Text.Date(match.Groups[2].Value) : first, note.LastDate);
        Assert.Equal(match.Groups[3].Value, note.Note);
        if (match.Groups[4].Success)
        {
            Assert.Equal(Enum.Parse<EvaluationState>(match.Groups[4].Value), note.State);
        }
    }

    /// <summary>Exactly the notes of a list such as "N1; and N2", in this order.</summary>
    public static void CheckNotes(IReadOnlyList<RecentNote> notes, string list)
    {
        var items = F005Text.Items(list);
        Assert.Equal(items.Count, notes.Count);
        for (var index = 0; index < items.Count; index++)
        {
            CheckNote(notes[index], items[index]);
        }
    }

    // ------------------------------------------------------------ records and progress

    private static void RecordsAre(string list, IReadOnlyList<RecordView> records)
    {
        var tuples = Regex.Matches(list, @"\(([^()]*)\)").Select(match => match.Groups[1].Value).ToList();
        Assert.Equal(tuples.Count, records.Count);
        for (var index = 0; index < tuples.Count; index++)
        {
            RecordIs(tuples[index], records[index]);
        }
    }

    private static void RecordIs(string tuple, RecordView record)
    {
        var match = Tuple.Match(tuple.Trim());
        if (!match.Success)
        {
            throw new InvalidOperationException($"Cannot read the record ({tuple}).");
        }

        Assert.Equal(F005Text.Date(match.Groups[1].Value), record.Date);
        ValueIs(match.Groups[2].Value, record);
    }

    private static void ValueIs(string value, RecordView record)
    {
        switch (value)
        {
            case "Yes":
                Assert.Equal(YesNoAnswer.Yes, record.Answer);
                break;
            case "No":
                Assert.Equal(YesNoAnswer.No, record.Answer);
                break;
            default:
                Assert.Equal(F005Text.Int(value), record.Amount);
                break;
        }
    }

    private static bool Matches(string tuple, RecordView record)
    {
        var match = Tuple.Match(tuple.Trim());
        if (!match.Success)
        {
            throw new InvalidOperationException($"Cannot read the record ({tuple}).");
        }

        var value = match.Groups[2].Value;
        return record.Date == F005Text.Date(match.Groups[1].Value)
            && value switch
            {
                "Yes" => record.Answer == YesNoAnswer.Yes,
                "No" => record.Answer == YesNoAnswer.No,
                _ => record.Amount == F005Text.Int(value),
            };
    }

    private static void ProgressIs(F005View view, ProgressKind kind, Match match)
    {
        var progress = view.One.Progress;
        Assert.Equal(kind, progress.Kind);
        Assert.Equal(F005Text.Int(match.Groups[1].Value), progress.Value);
        Assert.Equal(F005Text.Int(match.Groups[2].Value), progress.Of);
    }

    private static Regex Clause(string pattern) =>
        new("^(?:" + pattern + ")(?=$|, | and )", RegexOptions.CultureInvariant);
}
