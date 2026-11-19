using System.Text.RegularExpressions;
using HT.Application.Goals;
using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Records;

namespace HT.AcceptanceTests.Support.F005;

/// <summary>
/// Runs the recording use cases the F005 steps name, as acceptance.md ("Recording use cases", "Use cases of this
/// slice") defines them: the Given steps' records and notes (which must succeed) and the When steps' calls (whose
/// results the Then steps check).
/// </summary>
public sealed class F005Actions(F005Context context, F005Driver driver, F005Arrange arrange)
{
    private const string D = F005Text.D;

    /// <summary>The fixed date and value acceptance.md gives the use cases named in tables.</summary>
    private static readonly DateOnly TableDate = new(2026, 11, 18);

    private const string TableNote = "Changed.";

    private const int TableAmount = 12;

    private const string NoGoal = "a goal identifier that belongs to no goal";

    private static readonly Regex SubActions = new(
        @"(?:,? and then |, then |, (?=(?:records|changes|deletes|saves) )| and (?=(?:records|changes|deletes|saves) ))",
        RegexOptions.CultureInvariant);

    private static readonly Regex Verb = new("^(records|changes|deletes|saves) ", RegexOptions.CultureInvariant);

    private const string GoalClause = "the goal of `[^`]+`|the first goal|the second goal";

    private const string Target =
        $"(?:the record of (?<date>{D})(?: of the (?<ofGoal>first|second) goal)?" +
        $"|the first record of (?<firstDate>{D})" +
        $"|its record of (?<itsDate>{D})" +
        $"|the (?<otherGoal>first|second) goal's record of (?<otherDate>{D})" +
        "|(?<none>a record identifier that belongs to no record)" +
        "|(?<bare>the record))";

    private const string On =
        $"(?: on (?:(?<onGoal>{GoalClause})|their own goal, with the record identifier of `(?<idOwner>[^`]+)`'s record of (?<idDate>{D})))?";

    private static readonly Regex RecordAction = new(
        $"^records (?<value>.+?) for (?<recordDate>{D})(?: with the note (?<note>.+?))?(?: on (?<onGoal>{GoalClause}))?$",
        RegexOptions.CultureInvariant);

    private static readonly Regex ChangeAction = new(
        $"^changes {Target} to (?<value>.+?)(?: with the note (?<note>.+?))?{On}$",
        RegexOptions.CultureInvariant);

    private static readonly Regex DeleteAction = new($"^deletes {Target}{On}$", RegexOptions.CultureInvariant);

    private static readonly Regex SaveAction = new($"^saves the note (?<note>.+) for (?<noteDate>{D})$", RegexOptions.CultureInvariant);

    private static readonly Regex RecordItem = new($@"^(Yes|No|-?\d+)(?: for ({D}))?", RegexOptions.CultureInvariant);

    private static readonly Regex ListSeparator = new(@"^(?:, and |, | and )", RegexOptions.CultureInvariant);

    // ------------------------------------------------------------ Given: records and notes that must succeed

    /// <summary>
    /// "`x` recorded …": "nothing"; "V for D" lists ("Yes, Yes and No for D", "20 for D1, 25 for D2 and 12 for D3");
    /// "V for D N times"; "V for D with the note N"; "… on the first goal and … on the second goal"; "… and saved the
    /// note N for D". Each record is the record use case for the date and value with no note; it must succeed.
    /// </summary>
    public async Task GivenRecordedAsync(string ownerEmail, string body)
    {
        var text = body.Trim();
        if (text == "nothing")
        {
            return;
        }

        Match match;
        if ((match = Regex.Match(text, $"^(.+) and saved the note \"([^\"]*)\" for ({D})$")).Success)
        {
            var goal = arrange.DefaultGoal(ownerEmail);
            await GivenRecordsAsync(ownerEmail, goal, match.Groups[1].Value);
            await GivenNoteAsync(ownerEmail, goal, F005Text.Date(match.Groups[3].Value), match.Groups[2].Value);
            return;
        }

        if ((match = Regex.Match(text, "^(.+?) on the (first|second) goal(?: and (.+?) on the (first|second) goal)?$")).Success)
        {
            await GivenRecordsAsync(ownerEmail, arrange.NthGoal(ownerEmail, F005Arrange.Ordinal(match.Groups[2].Value)), match.Groups[1].Value);
            if (match.Groups[3].Success)
            {
                await GivenRecordsAsync(ownerEmail, arrange.NthGoal(ownerEmail, F005Arrange.Ordinal(match.Groups[4].Value)), match.Groups[3].Value);
            }

            return;
        }

        await GivenRecordsAsync(ownerEmail, arrange.DefaultGoal(ownerEmail), text);
    }

    /// <summary>"`x` saved the note N for D (and the note N2 for D2 …) (on the first goal)".</summary>
    public async Task GivenSavedNotesAsync(string ownerEmail, string body)
    {
        var text = body.Trim();
        var goal = arrange.DefaultGoal(ownerEmail);
        var onGoal = Regex.Match(text, "^(.+) on the (first|second) goal$");
        if (onGoal.Success)
        {
            goal = arrange.NthGoal(ownerEmail, F005Arrange.Ordinal(onGoal.Groups[2].Value));
            text = onGoal.Groups[1].Value;
        }

        var notes = Regex.Matches(text, $"(?:the note )?\"([^\"]*)\" for ({D})");
        Assert.NotEmpty(notes);
        foreach (Match note in notes)
        {
            await GivenNoteAsync(ownerEmail, goal, F005Text.Date(note.Groups[2].Value), note.Groups[1].Value);
        }
    }

    private async Task GivenRecordsAsync(string ownerEmail, F005Goal goal, string text)
    {
        var withNote = Regex.Match(text, $"^(.+) for ({D}) with the note (.+)$");
        if (withNote.Success)
        {
            var date = F005Text.Date(withNote.Groups[2].Value);
            await GivenRecordAsync(ownerEmail, goal, date, withNote.Groups[1].Value, F005Text.NoteOf(withNote.Groups[3].Value));
            return;
        }

        var repeated = Regex.Match(text, $@"^(.+) for ({D}) (\d+) times$");
        if (repeated.Success)
        {
            var date = F005Text.Date(repeated.Groups[2].Value);
            for (var time = 0; time < F005Text.Int(repeated.Groups[3].Value); time++)
            {
                await GivenRecordAsync(ownerEmail, goal, date, repeated.Groups[1].Value, null);
            }

            return;
        }

        foreach (var (value, date) in RecordList(text))
        {
            await GivenRecordAsync(ownerEmail, goal, date, value, null);
        }
    }

    private async Task GivenRecordAsync(string ownerEmail, F005Goal goal, DateOnly date, string value, string? note)
    {
        var (answer, amount) = F005Text.ValueOf(value);
        var result = await driver.RecordAsync(
            await arrange.AccountAsync(ownerEmail),
            goal.Id,
            date,
            new RecordInput { Answer = answer, Amount = amount, Note = note });
        Assert.Equal(RecordWriteOutcome.Recorded, result.Outcome);
        context.MentionedDates.Add(date);
        context.GivenRecords[goal.Id] = context.GivenRecordCount(goal.Id) + 1;
        if (!string.IsNullOrWhiteSpace(note))
        {
            NoteGiven(goal, date);
        }
    }

    private async Task GivenNoteAsync(string ownerEmail, F005Goal goal, DateOnly date, string note)
    {
        var result = await driver.SaveNoteAsync(await arrange.AccountAsync(ownerEmail), goal.Id, date, note);
        Assert.Equal(NoteSaveOutcome.Saved, result.Outcome);
        context.MentionedDates.Add(date);
        NoteGiven(goal, date);
    }

    private void NoteGiven(F005Goal goal, DateOnly date)
    {
        if (!context.GivenNoteDates.TryGetValue(goal.Id, out var dates))
        {
            dates = [];
            context.GivenNoteDates[goal.Id] = dates;
        }

        dates.Add(date);
    }

    /// <summary>"V1 for D1, V2 for D2 and V3 for D3" or "V1, V2 and V3 for D": each value with the date that follows it.</summary>
    public static List<(string Value, DateOnly Date)> RecordList(string text)
    {
        var records = new List<(string Value, DateOnly Date)>();
        var pending = new List<string>();
        var rest = text.Trim();
        var first = true;
        while (rest.Length > 0)
        {
            if (!first)
            {
                var separator = ListSeparator.Match(rest);
                if (!separator.Success)
                {
                    throw new InvalidOperationException($"Cannot read the records \"{text}\".");
                }

                rest = rest[separator.Length..];
            }

            first = false;
            var item = RecordItem.Match(rest);
            if (!item.Success)
            {
                throw new InvalidOperationException($"Cannot read the records \"{text}\".");
            }

            pending.Add(item.Groups[1].Value);
            rest = rest[item.Length..];
            if (item.Groups[2].Success)
            {
                var date = F005Text.Date(item.Groups[2].Value);
                records.AddRange(pending.Select(value => (value, date)));
                pending.Clear();
            }
        }

        if (pending.Count > 0 || records.Count == 0)
        {
            throw new InvalidOperationException($"\"{text}\" gives a value without a date.");
        }

        return records;
    }

    // ------------------------------------------------------------ When: recording, changing, deleting, saving notes

    /// <summary>
    /// The When step's record, change, delete and save-note calls, in the order the step writes them ("A, then B",
    /// "A and then B", "A, B and C"). A part without its own verb takes the one before it ("saves the note N for D1
    /// and then the note M for D2"); "its record" names the goal of the part before.
    /// </summary>
    public async Task RunAsync(string actorToken, string action)
    {
        var (email, user) = await arrange.ActorAsync(actorToken);
        F005Goal? previous = null;
        string? previousVerb = null;
        foreach (var raw in SubActions.Split(action.Trim()))
        {
            var part = raw.Trim();
            if (!Verb.IsMatch(part))
            {
                part = (previousVerb ?? throw new InvalidOperationException($"\"{action}\" starts without a use case.")) + " " + part;
            }

            previousVerb = Verb.Match(part).Groups[1].Value;
            previous = await RunOneAsync(email, user, part, previous);
        }
    }

    private async Task<F005Goal> RunOneAsync(string? email, ActingUser user, string part, F005Goal? previous)
    {
        Match match;
        if ((match = RecordAction.Match(part)).Success)
        {
            var goal = await GoalOfAsync(match, email, previous);
            var date = F005Text.Date(match.Groups["recordDate"].Value);
            var input = Input(match);
            context.MentionedDates.Add(date);
            var result = await driver.RecordAsync(user, goal.Id, date, input);
            context.Add(new F005Result(F005Kind.Record, result.Outcome, result.Message, result.Outcome == RecordWriteOutcome.Recorded, result, goal));
            return goal;
        }

        if ((match = ChangeAction.Match(part)).Success)
        {
            var goal = await GoalOfAsync(match, email, previous);
            var record = await RecordOfAsync(match, email, goal);
            var result = await driver.ChangeRecordAsync(user, goal.Id, record, Input(match));
            context.Add(new F005Result(F005Kind.Change, result.Outcome, result.Message, result.Outcome == RecordWriteOutcome.Changed, result, goal));
            return goal;
        }

        if ((match = DeleteAction.Match(part)).Success)
        {
            var goal = await GoalOfAsync(match, email, previous);
            var record = await RecordOfAsync(match, email, goal);
            var result = await driver.DeleteRecordAsync(user, goal.Id, record);
            context.Add(new F005Result(F005Kind.Delete, result.Outcome, result.Message, result.Outcome == RecordWriteOutcome.Deleted, result, goal));
            return goal;
        }

        if ((match = SaveAction.Match(part)).Success)
        {
            var goal = previous ?? arrange.DefaultGoal(email);
            var date = F005Text.Date(match.Groups["noteDate"].Value);
            context.MentionedDates.Add(date);
            var result = await driver.SaveNoteAsync(user, goal.Id, date, F005Text.NoteOf(match.Groups["note"].Value));
            context.Add(new F005Result(F005Kind.SaveNote, result.Outcome, result.Message, result.Outcome == NoteSaveOutcome.Saved, result, goal));
            return goal;
        }

        throw new InvalidOperationException($"Cannot read the use case \"{part}\".");
    }

    private static RecordInput Input(Match match)
    {
        var (answer, amount) = F005Text.ValueOf(match.Groups["value"].Value);
        var note = match.Groups["note"].Success ? F005Text.NoteOf(match.Groups["note"].Value) : null;
        return new RecordInput { Answer = answer, Amount = amount, Note = note };
    }

    private async Task<F005Goal> GoalOfAsync(Match match, string? email, F005Goal? previous)
    {
        if (match.Groups["onGoal"].Success)
        {
            return await arrange.GoalPhraseAsync(match.Groups["onGoal"].Value, email);
        }

        if (match.Groups["idOwner"].Success)
        {
            return arrange.GoalOf(email ?? throw new InvalidOperationException("\"their own goal\" needs a signed-in actor."));
        }

        if (match.Groups["ofGoal"].Success)
        {
            return arrange.NthGoal(email, F005Arrange.Ordinal(match.Groups["ofGoal"].Value));
        }

        if (match.Groups["itsDate"].Success)
        {
            return previous ?? throw new InvalidOperationException("\"its record\" follows no use case on a goal.");
        }

        return arrange.DefaultGoal(email);
    }

    /// <summary>
    /// The record identifier a change or delete names: "the record of D" (the only record dated D that the goal's
    /// history gives at that time), "the first record of D", "its record of D", "the second goal's record of D", the
    /// record of another account (AC-048), or an identifier that belongs to no record.
    /// </summary>
    private async Task<RecordId> RecordOfAsync(Match match, string? email, F005Goal goal)
    {
        if (match.Groups["date"].Success)
        {
            return await arrange.RecordIdAsync(goal, F005Text.Date(match.Groups["date"].Value), first: false);
        }

        if (match.Groups["firstDate"].Success)
        {
            return await arrange.RecordIdAsync(goal, F005Text.Date(match.Groups["firstDate"].Value), first: true);
        }

        if (match.Groups["itsDate"].Success)
        {
            return await arrange.RecordIdAsync(goal, F005Text.Date(match.Groups["itsDate"].Value), first: false);
        }

        if (match.Groups["otherGoal"].Success)
        {
            var other = arrange.NthGoal(email, F005Arrange.Ordinal(match.Groups["otherGoal"].Value));
            return await arrange.RecordIdAsync(other, F005Text.Date(match.Groups["otherDate"].Value), first: false);
        }

        if (match.Groups["none"].Success)
        {
            return new RecordId(Guid.NewGuid());
        }

        if (match.Groups["bare"].Success && match.Groups["idOwner"].Success)
        {
            var owners = arrange.GoalOf(match.Groups["idOwner"].Value);
            return await arrange.RecordIdAsync(owners, F005Text.Date(match.Groups["idDate"].Value), first: false);
        }

        throw new InvalidOperationException("The step names no record.");
    }

    // ------------------------------------------------------------ When: reads

    /// <summary>"reads the history of …", "reads what can be recorded now for the goal", "reads the recent notes of …".</summary>
    public async Task ReadAsync(string actorToken, string what)
    {
        var (email, user) = await arrange.ActorAsync(actorToken);
        CheckNoCallsSinceGiven();
        var text = what.Trim();
        Match match;
        if ((match = Regex.Match(text, $"^the history of (the goal(?: of `[^`]+`)?|\"[^\"]+\") from ({D}) to ({D})$")).Success)
        {
            var goal = await arrange.GoalPhraseAsync(match.Groups[1].Value, email);
            var result = await driver.GetHistoryAsync(user, goal.Id, F005Text.Date(match.Groups[2].Value), F005Text.Date(match.Groups[3].Value));
            context.Add(new F005Result(F005Kind.History, result.Outcome, result.Message, result.Outcome == HistoryOutcome.Found, result, goal));
            return;
        }

        if (text == "what can be recorded now for the goal")
        {
            var goal = arrange.DefaultGoal(email);
            var result = await driver.GetRecordableAsync(user, goal.Id);
            context.Add(new F005Result(F005Kind.Recordable, result.Outcome, result.Message, result.Outcome == RecordableOutcome.Found, result, goal));
            return;
        }

        if (text == "the recent notes of the goal")
        {
            await RecentNotesAsync(user, arrange.DefaultGoal(email), "the goal");
            return;
        }

        if (text == "the recent notes of the first goal and of the second goal")
        {
            await RecentNotesAsync(user, arrange.NthGoal(email, 0), "first goal");
            await RecentNotesAsync(user, arrange.NthGoal(email, 1), "second goal");
            return;
        }

        if (text == "the recent notes of each of its demo goals")
        {
            var owner = email ?? throw new InvalidOperationException("Demo goals need their account.");
            var goals = await arrange.ListGoalsAsync(owner);
            Assert.NotEmpty(goals);
            foreach (var item in goals)
            {
                await RecentNotesAsync(user, await arrange.DemoGoalAsync(owner, item.Name), item.Name);
            }

            return;
        }

        if ((match = Regex.Match(text, $"^the evaluation calendar of the goal from ({D}) to ({D})$")).Success)
        {
            var goal = arrange.DefaultGoal(email);
            var result = await driver.GetCalendarAsync(user, goal.Id, F005Text.Date(match.Groups[1].Value), F005Text.Date(match.Groups[2].Value));
            context.Add(new F005Result(F005Kind.Calendar, result.Outcome, result.Message, result.Outcome == GoalCalendarOutcome.Found, result, goal));
            return;
        }

        throw new InvalidOperationException($"Cannot read \"reads {what}\".");
    }

    private async Task RecentNotesAsync(ActingUser user, F005Goal goal, string key)
    {
        var result = await driver.GetRecentNotesAsync(user, goal.Id);
        context.RecentNotes[key] = result;
        context.Add(new F005Result(F005Kind.RecentNotes, result.Outcome, result.Message, result.Outcome == RecentNotesOutcome.Found, result, goal));
    }

    /// <summary>"no other use case is called for the goal after it was created": no call between that step and the read.</summary>
    private void CheckNoCallsSinceGiven()
    {
        foreach (var (goal, calls) in context.CallsFrozenUntilWhen)
        {
            Assert.Equal(calls, driver.Calls(goal));
        }
    }

    // ------------------------------------------------------------ When: a use case named in a table

    /// <summary>
    /// "`x` calls U with G": the use case U of acceptance.md's table names ("record": records 12 for 2026-11-18;
    /// "change"/"delete" with the owner's identifier of the record of 2026-11-18; "save note": "Changed." for
    /// 2026-11-18; "history": 2026-11-18 to 2026-11-18; "recordable now"; "recent notes").
    /// </summary>
    public async Task CallAsync(string actorToken, string useCase, string goalPhrase)
    {
        var (email, user) = await arrange.ActorAsync(actorToken);
        F005Goal? goal = goalPhrase.Trim() == NoGoal ? null : await arrange.GoalPhraseAsync(goalPhrase, email);
        var id = goal?.Id ?? new GoalId(Guid.NewGuid());
        switch (useCase.Trim())
        {
            case "record":
            {
                var result = await driver.RecordAsync(user, id, TableDate, new RecordInput { Amount = TableAmount });
                context.Add(new F005Result(F005Kind.Record, result.Outcome, result.Message, result.Outcome == RecordWriteOutcome.Recorded, result, goal));
                break;
            }

            case "change, with the owner's record identifier":
            {
                var record = await arrange.RecordIdAsync(Required(goal), TableDate, first: false);
                var result = await driver.ChangeRecordAsync(user, id, record, new RecordInput { Amount = TableAmount });
                context.Add(new F005Result(F005Kind.Change, result.Outcome, result.Message, result.Outcome == RecordWriteOutcome.Changed, result, goal));
                break;
            }

            case "delete, with the owner's record identifier":
            {
                var record = await arrange.RecordIdAsync(Required(goal), TableDate, first: false);
                var result = await driver.DeleteRecordAsync(user, id, record);
                context.Add(new F005Result(F005Kind.Delete, result.Outcome, result.Message, result.Outcome == RecordWriteOutcome.Deleted, result, goal));
                break;
            }

            case "save note":
            {
                var result = await driver.SaveNoteAsync(user, id, TableDate, TableNote);
                context.Add(new F005Result(F005Kind.SaveNote, result.Outcome, result.Message, result.Outcome == NoteSaveOutcome.Saved, result, goal));
                break;
            }

            case "history":
            {
                var result = await driver.GetHistoryAsync(user, id, TableDate, TableDate);
                context.Add(new F005Result(F005Kind.History, result.Outcome, result.Message, result.Outcome == HistoryOutcome.Found, result, goal));
                break;
            }

            case "recordable now":
            {
                var result = await driver.GetRecordableAsync(user, id);
                context.Add(new F005Result(F005Kind.Recordable, result.Outcome, result.Message, result.Outcome == RecordableOutcome.Found, result, goal));
                break;
            }

            case "recent notes":
            {
                var result = await driver.GetRecentNotesAsync(user, id);
                context.Add(new F005Result(F005Kind.RecentNotes, result.Outcome, result.Message, result.Outcome == RecentNotesOutcome.Found, result, goal));
                break;
            }

            default:
                throw new InvalidOperationException($"\"{useCase}\" is not a use case of this slice.");
        }
    }

    private static F005Goal Required(F005Goal? goal) =>
        goal ?? throw new InvalidOperationException("The owner's record identifier needs the owner's goal.");
}
