using System.Text.RegularExpressions;
using HT.AcceptanceTests.Support.F008;

namespace HT.AcceptanceTests.StepDefinitions.F008;

/// <summary>
/// The Then steps of F008 that check the result of the When step and read the history, what can be recorded now, the
/// pauses, the recent notes and the goals overview (each read changes nothing and runs at the When step's time unless
/// the step gives its own).
/// </summary>
[Binding]
[Scope(Tag = "F008")]
public sealed class ResultThenSteps(F008Context context, F008Arrange arrange)
{
    private const string D = "(" + F008Text.D + ")";
    private const string T = "(" + F008Text.T + ")";

    /// <summary>An optional "at T " with no capture group of its own; wrapped in a capture group it always takes part.</summary>
    private const string AtClause = "(?:at " + F008Text.T + " )?";

    /// <summary>An optional ", read by `x`," with no capture group of its own.</summary>
    private const string ReadByClause = "(?:, read by `[^`]+`,)?";

    private static readonly Regex OverviewGoal = new(
        "`(?<goal>[^`]+)` (?:paused until (?<until>" + F008Text.D + ")|(?<not>not paused))",
        RegexOptions.CultureInvariant);

    // ------------------------------------------------------------ the result of the When step

    /// <summary>
    /// "the result is "TEXT"" (the result's text), optionally "with the date D" (excused), "with the state S" (excusal
    /// removed), "with the first day D1 and the last day D2" (paused); or "the state S, correction no" (the entry state
    /// and correction flag a record, change or delete gave).
    /// </summary>
    [Then("^the result is (.+)$")]
    public void ThenTheResultIs(string result)
    {
        var outcome = context.Outcome ?? throw new InvalidOperationException("The When step called no use case.");
        Match match;
        if ((match = Regex.Match(result, "^the state (\\w+), correction (yes|no)$")).Success)
        {
            var record = outcome.Record ?? throw new InvalidOperationException("The When step did not record, change or delete.");
            var entry = record.Entry ?? throw new Xunit.Sdk.XunitException($"The result \"{record.Message}\" gave no entry.");
            Assert.Equal(F008Text.StateOf(match.Groups[1].Value), entry.State);
            Assert.Equal(match.Groups[2].Value == "yes", record.IsCorrection);
        }
        else if ((match = Regex.Match(result, "^\"([^\"]+)\" with the date " + D + "$")).Success)
        {
            var excuse = outcome.Excuse ?? throw new InvalidOperationException("The When step did not excuse.");
            Assert.Equal(match.Groups[1].Value, excuse.Message);
            Assert.Equal(F008Text.Date(match.Groups[2].Value), excuse.Date);
        }
        else if ((match = Regex.Match(result, "^\"([^\"]+)\" with the state (\\w+)$")).Success)
        {
            var removed = outcome.RemoveExcusal ?? throw new InvalidOperationException("The When step did not remove an excusal.");
            Assert.Equal(match.Groups[1].Value, removed.Message);
            Assert.Equal(F008Text.StateOf(match.Groups[2].Value), removed.State);
        }
        else if ((match = Regex.Match(result, "^\"([^\"]+)\" with the first day " + D + " and the last day " + D + "$")).Success)
        {
            var paused = outcome.Pause ?? throw new InvalidOperationException("The When step did not pause.");
            Assert.Equal(match.Groups[1].Value, paused.Message);
            var pause = paused.Pause ?? throw new Xunit.Sdk.XunitException($"The result \"{paused.Message}\" gave no pause.");
            Assert.Equal(
                (F008Text.Date(match.Groups[2].Value), F008Text.Date(match.Groups[3].Value)),
                (pause.FirstDay, pause.LastDay));
        }
        else if ((match = Regex.Match(result, "^\"([^\"]+)\"$")).Success)
        {
            Assert.Equal(match.Groups[1].Value, outcome.Message);
        }
        else
        {
            throw new InvalidOperationException($"Cannot read the result {result}.");
        }
    }

    // ------------------------------------------------------------ history and what can be recorded now

    /// <summary>"[at T ]the history of the goal|`G` from D1 to D2[, read by `x`,] gives …": slice 5's history use case, read now.</summary>
    /// <remarks>The optional "at T " and ", read by `x`," sit in groups that always take part (empty when not written).</remarks>
    [Then("^(" + AtClause + ")the history of (the goal|`[^`]+`) from " + D + " to " + D + "(" + ReadByClause + ") gives (.+)$")]
    public async Task ThenTheHistoryGives(string atClause, string goalPhrase, string first, string last, string readByClause, string entries)
    {
        arrange.Then(TimeOrNull(atClause));
        var goal = goalPhrase == "the goal" ? arrange.OnlyGoal() : await arrange.GoalNamedAsync(goalPhrase.Trim('`'));
        var history = await arrange.HistoryAsync(goal, F008Text.Date(first), F008Text.Date(last), BacktickedOrNull(readByClause));
        F008Checks.Entries(history.Entries, F008Text.Entries(entries));
    }

    [Then("^what can be recorded now contains (.+)$")]
    public async Task ThenWhatCanBeRecordedNowContains(string entries)
    {
        arrange.Then();
        var recordable = await arrange.RecordableAsync(arrange.OnlyGoal());
        F008Checks.Entries(recordable.Entries, F008Text.Entries(entries));
    }

    /// <summary>"it gives …": the history the When step read, or (after reading the goals overview) a goal of that overview.</summary>
    [Then("^it gives (.+)$")]
    public void ThenItGives(string what)
    {
        switch (context.ReadKind)
        {
            case F008ReadKind.History:
                F008Checks.Entries(
                    (context.History ?? throw new InvalidOperationException("The When step read no history.")).Entries,
                    F008Text.Entries(what));
                break;
            case F008ReadKind.Overview:
                CheckOverview(context.Overview ?? throw new InvalidOperationException("The When step read no overview."), what);
                break;
            default:
                throw new InvalidOperationException($"\"it gives\" after a When step that read {context.ReadKind}.");
        }
    }

    // ------------------------------------------------------------ pauses and recent notes

    /// <summary>
    /// "[at T ]the pauses of the goal|`G`[ of `x`][, read by `x`,] are[, in this order,] (…) and (…)" or "… are
    /// empty": FR-012's use case, read by the goal's owner (or the reader given).
    /// </summary>
    /// <remarks>
    /// The optional "at T ", " of `x`" and ", read by `x`," sit in groups that always take part (empty when not written).
    /// </remarks>
    [Then("^(" + AtClause + ")the pauses of (the goal|`[^`]+`)((?: of `[^`]+`)?)(" + ReadByClause + ") are(?:, in this order,)? (.+)$")]
    public async Task ThenThePausesAre(string atClause, string goalPhrase, string ownerClause, string readByClause, string pauses)
    {
        arrange.Then(TimeOrNull(atClause));
        var owner = BacktickedOrNull(ownerClause);
        var goal = goalPhrase == "the goal"
            ? arrange.OnlyGoal()
            : owner is null
                ? await arrange.GoalNamedAsync(goalPhrase.Trim('`'))
                : await arrange.GoalNamedAsync(owner, goalPhrase.Trim('`'));
        var read = await arrange.PausesAsync(goal, BacktickedOrNull(readByClause));
        F008Checks.Pauses(read, F008Text.Pauses(pauses), context.Pauses.Where(pause => pause.Goal.Id == goal.Id));
    }

    /// <summary>"they are[, in this order,] …": the pauses or the recent notes the When step read, exactly and in order.</summary>
    [Then("^they are(?:, in this order,)? (.+)$")]
    public void ThenTheyAre(string list)
    {
        switch (context.ReadKind)
        {
            case F008ReadKind.Pauses:
                var read = context.PausesRead ?? throw new InvalidOperationException("The When step read no pauses.");
                var goal = arrange.OnlyGoal();
                F008Checks.Pauses(read, F008Text.Pauses(list), context.Pauses.Where(pause => pause.Goal.Id == goal.Id));
                break;
            case F008ReadKind.RecentNotes:
                F008Checks.RecentNotes(
                    context.RecentNotes ?? throw new InvalidOperationException("The When step read no recent notes."),
                    F008Text.Notes(list));
                break;
            default:
                throw new InvalidOperationException($"\"they are\" after a When step that read {context.ReadKind}.");
        }
    }

    [Then("^the recent notes of `([^`]+)` are (.+)$")]
    public async Task ThenTheRecentNotesOfAre(string goalName, string notes)
    {
        arrange.Then();
        var goal = await arrange.GoalNamedAsync(goalName);
        F008Checks.RecentNotes(await arrange.RecentNotesAsync(goal), F008Text.Notes(notes));
    }

    // ------------------------------------------------------------ goals overview

    /// <summary>"the goals overview gives `G` paused until D" / "… `G` not paused" (several joined by "and"), read now.</summary>
    [Then("^the goals overview gives (.+)$")]
    public async Task ThenTheGoalsOverviewGives(string what)
    {
        arrange.Then();
        CheckOverview(await arrange.OverviewAsync(arrange.Reader()), what);
    }

    [Then("^at " + T + " the goals overview of `([^`]+)` lists exactly (.+)$")]
    public async Task ThenTheGoalsOverviewListsExactly(string time, string email, string goals)
    {
        arrange.Then(time);
        var overview = await arrange.OverviewAsync(email);
        var expected = goals.Trim() == "no goal" ? [] : F008Text.Backticked(goals);
        Assert.Equal(
            expected.OrderBy(name => name, StringComparer.Ordinal).ToList(),
            overview.Goals.Select(goal => goal.Item.Name).OrderBy(name => name, StringComparer.Ordinal).ToList());
    }

    /// <summary>The instant of an optional "at T " clause, or null when the clause is empty.</summary>
    private static string? TimeOrNull(string clause) =>
        string.IsNullOrEmpty(clause) ? null : Regex.Match(clause, F008Text.T).Value;

    /// <summary>The one backticked name of an optional clause (" of `x`", ", read by `x`,"), or null when it is empty.</summary>
    private static string? BacktickedOrNull(string clause) =>
        string.IsNullOrEmpty(clause) ? null : F008Text.Backticked(clause).Single();

    private static void CheckOverview(HT.Application.Reports.GoalsOverview overview, string what)
    {
        var matches = OverviewGoal.Matches(what);
        Assert.True(matches.Count > 0, $"\"{what}\" names no goal of the overview.");
        Assert.True(
            Regex.Replace(OverviewGoal.Replace(what, string.Empty), "(?:,|\\band\\b|\\s)+", string.Empty).Length == 0,
            $"Cannot read all of \"{what}\".");
        foreach (Match match in matches)
        {
            F008Checks.OverviewPaused(
                overview,
                match.Groups["goal"].Value,
                match.Groups["not"].Success ? null : F008Text.Date(match.Groups["until"].Value));
        }
    }
}
