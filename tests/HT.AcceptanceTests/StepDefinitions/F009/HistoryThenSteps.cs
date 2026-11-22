using System.Text.RegularExpressions;
using HT.AcceptanceTests.Support.F009;
using HT.Application.Records;

namespace HT.AcceptanceTests.StepDefinitions.F009;

/// <summary>
/// The Then steps of F009 about a goal's dates: its history (slice 5, with FR-016), what can be recorded now, its recent
/// notes, its evaluation calendar and its pauses.
/// </summary>
[Binding]
[Scope(Tag = "F009")]
public sealed class HistoryThenSteps(F009Context context, F009Arrange arrange)
{
    private const string D = "(" + F009Text.D + ")";
    private const string AtT = "((?:at " + F009Text.T + " )?)";

    // ------------------------------------------------------------ history

    /// <summary>After a history read: the entries written are in it as written.</summary>
    [Then("^it gives (\\d{4}-\\d{2}-\\d{2}.+)$")]
    public void ThenItGivesEntries(string entries)
    {
        F009Checks.Entries(WhenHistory().Entries, F009Text.Entries(entries), "The history");
    }

    /// <summary>After a history read: exactly the entries written; after a calendar read: exactly the dates or periods written.</summary>
    [Then("^it gives exactly (.+)$")]
    public void ThenItGivesExactly(string text)
    {
        switch (context.ReadKind)
        {
            case F009ReadKind.History:
                F009Checks.ExactEntries(WhenHistory().Entries, F009Text.Entries(text), "The history");
                break;
            case F009ReadKind.Calendar:
                var calendar = context.Calendar ?? throw new InvalidOperationException("The When step read no evaluation calendar.");
                var expected = Regex.Matches(text, F009Text.Span).Select(match => F009Text.SpanOf(match.Value)).OrderBy(span => span.First).ToList();
                F009Text.RequireOnlySeparators(Regex.Replace(text, F009Text.Span, string.Empty), text);
                var actual = calendar.Commitments.Select(commitment => (First: commitment.Date, Last: commitment.Date))
                    .Concat(calendar.Periods.Select(period => (First: period.FirstDate, Last: period.LastDate)))
                    .OrderBy(span => span.First)
                    .ToList();
                Assert.Equal(expected, actual);
                break;
            default:
                throw new InvalidOperationException($"\"it gives exactly\" follows no history or calendar read (the When step read {context.ReadKind}).");
        }
    }

    /// <summary>"[at T ]the history of the goal|`G` from D1 to D2 gives no entry|exactly …|…": slice 5's history, read now by the owner.</summary>
    [Then("^" + AtT + "the history of (the goal|`[^`]+`) from " + D + " to " + D + " gives (.+)$")]
    public async Task ThenTheHistoryGives(string at, string goalPhrase, string first, string last, string text)
    {
        arrange.Then(TimeOf(at));
        var goal = await arrange.ThenGoalAsync(goalPhrase);
        var history = await arrange.HistoryAsync(goal, F009Text.Date(first), F009Text.Date(last));
        if (text == "no entry")
        {
            Assert.Empty(history.Entries);
        }
        else if (text.StartsWith("exactly ", StringComparison.Ordinal))
        {
            F009Checks.ExactEntries(history.Entries, F009Text.Entries(text["exactly ".Length..]), "The history");
        }
        else
        {
            F009Checks.Entries(history.Entries, F009Text.Entries(text), "The history");
        }
    }

    /// <summary>FR-027: an ending note is not a note of a commitment or period.</summary>
    [Then("^no entry of the history of the goal from " + D + " to " + D + " carries the note \"([^\"]*)\"$")]
    public async Task ThenNoHistoryEntryCarriesTheNote(string first, string last, string note)
    {
        arrange.Then();
        var history = await arrange.HistoryAsync(arrange.OnlyGoal(), F009Text.Date(first), F009Text.Date(last));
        Assert.NotEmpty(history.Entries);
        Assert.DoesNotContain(history.Entries, entry => entry.Note == note);
    }

    // ------------------------------------------------------------ what can be recorded now

    /// <summary>After "reads what can be recorded now": it contains the entries written.</summary>
    [Then("^it contains (\\(.+)$")]
    public async Task ThenItContains(string tuples)
    {
        Assert.Equal(F009ReadKind.Recordable, context.ReadKind);
        foreach (var spec in F009Text.Tuples(tuples))
        {
            var goal = await arrange.GoalNamedAsync(spec.Goal);
            F009Checks.RecordableEntry(WhenRecordable(goal), spec);
        }
    }

    [Then("^it contains no entry of `([^`]+)` for " + D + "$")]
    public async Task ThenItContainsNoEntryOfGoalFor(string name, string date)
    {
        Assert.Equal(F009ReadKind.Recordable, context.ReadKind);
        var goal = await arrange.GoalNamedAsync(name);
        var day = F009Text.Date(date);
        Assert.DoesNotContain(WhenRecordable(goal).Entries, entry => F009Checks.Covers(entry.FirstDate, entry.LastDate, day));
    }

    [Then("^it contains no entry for " + D + "$")]
    public void ThenItContainsNoEntryFor(string date)
    {
        Assert.Equal(F009ReadKind.Recordable, context.ReadKind);
        Assert.NotEmpty(context.Recordable);
        var day = F009Text.Date(date);
        foreach (var recordable in context.Recordable.Values)
        {
            Assert.DoesNotContain(recordable.Entries, entry => F009Checks.Covers(entry.FirstDate, entry.LastDate, day));
        }
    }

    [Then("^" + AtT + "what can be recorded now contains (\\(.+)$")]
    public async Task ThenWhatCanBeRecordedNowContains(string at, string tuples)
    {
        arrange.Then(TimeOf(at));
        foreach (var spec in F009Text.Tuples(tuples))
        {
            var goal = await arrange.GoalNamedAsync(spec.Goal);
            F009Checks.RecordableEntry(await arrange.RecordableAsync(goal), spec);
        }
    }

    [Then("^what can be recorded now contains no entry of `([^`]+)`$")]
    public async Task ThenWhatCanBeRecordedNowContainsNoEntryOf(string name)
    {
        arrange.Then();
        var goal = await arrange.GoalNamedAsync(name);
        Assert.Empty((await arrange.RecordableAsync(goal)).Entries);
    }

    // ------------------------------------------------------------ recent notes

    [Then("^they are exactly (\\(.+)$")]
    public void ThenTheyAreExactlyNotes(string notes)
    {
        Assert.Equal(F009ReadKind.RecentNotes, context.ReadKind);
        ExactNotes(context.RecentNotes ?? throw new InvalidOperationException("The When step read no recent notes."), notes);
    }

    [Then("^the recent notes of (the goal|`[^`]+`) are exactly (\\(.+)$")]
    public async Task ThenTheRecentNotesAreExactly(string goalPhrase, string notes)
    {
        arrange.Then();
        var goal = await arrange.ThenGoalAsync(goalPhrase);
        ExactNotes(await arrange.RecentNotesAsync(goal), notes);
    }

    // ------------------------------------------------------------ pauses

    [Then("^the pauses of the goal are none$")]
    public async Task ThenThePausesAreNone()
    {
        arrange.Then();
        Assert.Empty(await arrange.PausesAsync(arrange.OnlyGoal()));
    }

    [Then("^the pauses of the goal are exactly (\\(.+)$")]
    public async Task ThenThePausesAreExactly(string pauses)
    {
        arrange.Then();
        var actual = await arrange.PausesAsync(arrange.OnlyGoal());
        var expected = F009Text.Pauses(pauses);
        Assert.Equal(expected.Count, actual.Count);
        foreach (var spec in expected)
        {
            var pause = Assert.Single(actual, candidate => candidate.FirstDay == spec.FirstDay && candidate.LastDay == spec.LastDay);
            Assert.Null(pause.Reason);
            Assert.Equal(spec.Timing, pause.Timing.ToString().ToLowerInvariant());
        }
    }

    // ------------------------------------------------------------ helpers

    private GoalHistory WhenHistory()
    {
        Assert.Equal(F009ReadKind.History, context.ReadKind);
        return context.History ?? throw new InvalidOperationException("The When step read no history.");
    }

    private GoalHistory WhenRecordable(F009Goal goal) =>
        context.Recordable.TryGetValue(goal.Id, out var recordable)
            ? recordable
            : throw new InvalidOperationException($"The When step did not read what can be recorded now for `{goal.Label}`.");

    private static void ExactNotes(IReadOnlyList<RecentNote> actual, string text)
    {
        var expected = F009Text.Notes(text);
        Assert.Equal(expected.Count, actual.Count);
        for (var index = 0; index < expected.Count; index++)
        {
            var spec = expected[index];
            var note = actual[index];
            Assert.Equal((spec.First, spec.Last, spec.Note, spec.State), (note.FirstDate, note.LastDate, note.Note, note.State));
        }
    }

    private static string? TimeOf(string at)
    {
        var match = Regex.Match(at, F009Text.T);
        return match.Success ? match.Value : null;
    }
}
