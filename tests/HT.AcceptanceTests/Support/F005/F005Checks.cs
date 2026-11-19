using System.Text.RegularExpressions;
using HT.Application.Goals;
using HT.Application.Records;
using HT.Domain.Evaluation;
using HT.Domain.Goals;

namespace HT.AcceptanceTests.Support.F005;

/// <summary>Checks shared by the F005 Then steps: results of the When step, and what a history read gives.</summary>
public sealed class F005Checks(F005Context context, F005Arrange arrange)
{
    private const string D = F005Text.D;

    private static readonly Regex HistorySentence = new(
        $@"^(?:reading )?the history (?:of the goal )?from ({D}) to ({D}) (?:as `([^`]+)` )?gives,? (.+)$",
        RegexOptions.CultureInvariant);

    // ------------------------------------------------------------ results of the When step

    /// <summary>The outcome a successful use case of a kind gives.</summary>
    public static Enum SuccessOutcome(F005Kind kind) => kind switch
    {
        F005Kind.Record => RecordWriteOutcome.Recorded,
        F005Kind.Change => RecordWriteOutcome.Changed,
        F005Kind.Delete => RecordWriteOutcome.Deleted,
        F005Kind.SaveNote => NoteSaveOutcome.Saved,
        F005Kind.History => HistoryOutcome.Found,
        F005Kind.Recordable => RecordableOutcome.Found,
        F005Kind.RecentNotes => RecentNotesOutcome.Found,
        F005Kind.Calendar => GoalCalendarOutcome.Found,
        _ => throw new InvalidOperationException(kind.ToString()),
    };

    /// <summary>"… succeeds with the state S, a correction | not a correction" (FR-006).</summary>
    public static void Succeeded(F005Result result, F005Kind kind, EvaluationState state, bool isCorrection)
    {
        Assert.Equal(kind, result.Kind);
        var write = Assert.IsType<RecordWriteResult>(result.Raw);
        Assert.Equal(SuccessOutcome(kind), write.Outcome);
        Assert.NotNull(write.Entry);
        Assert.Equal(state, write.Entry.State);
        Assert.Equal(isCorrection, write.IsCorrection);
    }

    /// <summary>"… and it gives no record, note or state": a refusal returns no data.</summary>
    public static void GivesNoData(F005Result result)
    {
        switch (result.Raw)
        {
            case RecordWriteResult write:
                Assert.Null(write.Entry);
                Assert.Null(write.RecordId);
                break;
            case NoteSaveResult note:
                Assert.Null(note.Entry);
                break;
            case HistoryResult history:
                Assert.Null(history.History);
                break;
            case RecordableResult recordable:
                Assert.Null(recordable.History);
                break;
            case RecentNotesResult notes:
                Assert.Empty(notes.Notes);
                break;
            default:
                throw new InvalidOperationException($"No data check for {result.Raw.GetType().Name}.");
        }
    }

    /// <summary>
    /// The rest of a result step after its first clause: "", " and nothing is recorded", " and it gives no record,
    /// note or state", or ", and the history from D1 to D2 gives …" (optionally after a parenthesis).
    /// </summary>
    public async Task RestAsync(string rest)
    {
        if (string.IsNullOrWhiteSpace(rest))
        {
            return;
        }

        var match = Regex.Match(rest, @"^(?: \([^)]*\))?,? and (.+)$");
        if (!match.Success)
        {
            throw new InvalidOperationException($"Cannot read \"{rest}\".");
        }

        var clause = match.Groups[1].Value;
        switch (clause)
        {
            case "nothing is recorded":
                await NothingIsRecordedAsync();
                break;
            case "it gives no record, note or state":
                GivesNoData(context.LastResult);
                break;
            default:
                await HistoryAsync(clause);
                break;
        }
    }

    // ------------------------------------------------------------ history reads of Then steps

    /// <summary>
    /// "nothing is recorded": the history of the goal, read by its owner at the same time over the item's dates,
    /// has no record and no note other than those the item's Given steps made.
    /// </summary>
    public async Task NothingIsRecordedAsync()
    {
        var goal = arrange.OnlyGoal;
        var history = await arrange.HistoryAsync(goal, goal.OwnerEmail, context.MentionedDates.Min, context.MentionedDates.Max);
        Assert.Equal(context.GivenRecordCount(goal.Id), history.Entries.Sum(entry => entry.Records.Count));
        Assert.Equal(context.GivenNoteCount(goal.Id), history.Entries.Count(entry => entry.Note is not null));
    }

    /// <summary>"(reading) the history (of the goal) from D1 to D2 (as `x`) gives …", read at the step's time.</summary>
    public async Task HistoryAsync(string sentence)
    {
        var match = HistorySentence.Match(sentence.Trim());
        if (!match.Success)
        {
            throw new InvalidOperationException($"Cannot read the history check \"{sentence}\".");
        }

        var first = F005Text.Date(match.Groups[1].Value);
        var last = F005Text.Date(match.Groups[2].Value);
        var reader = match.Groups[3].Success ? match.Groups[3].Value : null;
        var body = match.Groups[4].Value;

        var alike = Regex.Match(body, "^for the first goal and for the second goal alike: (.+)$");
        if (alike.Success)
        {
            foreach (var index in new[] { 0, 1 })
            {
                var each = arrange.NthGoal(null, index);
                var read = await arrange.HistoryAsync(each, each.OwnerEmail, first, last);
                F005Entries.CheckOrdered(read.Entries, alike.Groups[1].Value, each.Kind);
            }

            return;
        }

        var twoGoals = Regex.Match(body, "^(.+) for the first goal and (.+) for the second goal$");
        if (twoGoals.Success)
        {
            await AllAsync(arrange.NthGoal(null, 0), first, last, twoGoals.Groups[1].Value);
            await AllAsync(arrange.NthGoal(null, 1), first, last, twoGoals.Groups[2].Value);
            return;
        }

        var twoOwners = Regex.Match(body, "^(.+) for the goal of `([^`]+)` and (.+) for the goal of `([^`]+)`$");
        if (twoOwners.Success)
        {
            await AllAsync(arrange.GoalOf(twoOwners.Groups[2].Value), first, last, twoOwners.Groups[1].Value);
            await AllAsync(arrange.GoalOf(twoOwners.Groups[4].Value), first, last, twoOwners.Groups[3].Value);
            return;
        }

        var goal = reader is not null ? arrange.GoalOf(reader) : arrange.OnlyGoal;
        var history = await arrange.HistoryAsync(goal, reader ?? goal.OwnerEmail, first, last);
        F005Entries.CheckBody(history.Entries, body, goal.Kind);
    }

    private async Task AllAsync(F005Goal goal, DateOnly first, DateOnly last, string facts)
    {
        var history = await arrange.HistoryAsync(goal, goal.OwnerEmail, first, last);
        F005Entries.CheckAll(history.Entries, facts, goal.Kind);
    }

    // ------------------------------------------------------------ the last read of the When step

    public F005Result LastRead =>
        context.LastRead ?? throw new InvalidOperationException("The When step read nothing.");

    public GoalScheduleKind LastReadKind =>
        LastRead.Goal?.Kind ?? throw new InvalidOperationException("The last read names no goal of this scenario.");

    /// <summary>The entries the last history or "what can be recorded now" read gave; the read must succeed.</summary>
    public IReadOnlyList<HistoryEntry> LastEntries()
    {
        switch (LastRead.Raw)
        {
            case HistoryResult history:
                Assert.Equal(HistoryOutcome.Found, history.Outcome);
                Assert.NotNull(history.History);
                return history.History.Entries;
            case RecordableResult recordable:
                Assert.Equal(RecordableOutcome.Found, recordable.Outcome);
                Assert.NotNull(recordable.History);
                return recordable.History.Entries;
            default:
                throw new InvalidOperationException($"The last read gave {LastRead.Raw.GetType().Name}, not entries.");
        }
    }

    /// <summary>The notes the last recent-notes read gave; the read must succeed.</summary>
    public IReadOnlyList<RecentNote> LastNotes()
    {
        var notes = Assert.IsType<RecentNotesResult>(LastRead.Raw);
        Assert.Equal(RecentNotesOutcome.Found, notes.Outcome);
        return notes.Notes;
    }

    /// <summary>The calendar the last evaluation-calendar read gave; the read must succeed.</summary>
    public GoalCalendarView LastCalendar()
    {
        var calendar = Assert.IsType<GoalCalendarResult>(LastRead.Raw);
        Assert.Equal(GoalCalendarOutcome.Found, calendar.Outcome);
        return calendar.Calendar ?? throw new InvalidOperationException("The calendar read gave no calendar.");
    }
}
