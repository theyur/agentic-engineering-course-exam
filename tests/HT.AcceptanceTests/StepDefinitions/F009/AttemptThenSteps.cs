using System.Text.RegularExpressions;
using HT.AcceptanceTests.Support.F009;
using HT.Application.Reports;

namespace HT.AcceptanceTests.StepDefinitions.F009;

/// <summary>The Then steps of F009 about attempts (FR-014) and the goal history (FR-015).</summary>
[Binding]
[Scope(Tag = "F009")]
public sealed class AttemptThenSteps(F009Context context, F009Driver driver, F009Arrange arrange)
{
    private const string D = "(" + F009Text.D + ")";
    private const string AtT = "((?:at " + F009Text.T + " )?)";

    // ------------------------------------------------------------ the attempts of a goal

    /// <summary>"[at T ]the attempts of the goal|`G` are[ exactly][, in this order,] attempt …": read now by the owner.</summary>
    [Then("^" + AtT + "the attempts of (the goal|`[^`]+`) are((?: exactly)?)((?:, in this order,)?) (attempt \\d.*)$")]
    public async Task ThenTheAttemptsAre(string at, string goalPhrase, string exactly, string inOrder, string attempts)
    {
        arrange.Then(TimeOf(at));
        var goal = await arrange.ThenGoalAsync(goalPhrase);
        F009Checks.Attempts(await arrange.AttemptsAsync(goal), attempts, exactly.Length > 0, inOrder.Length > 0);
    }

    /// <summary>"[at T ]the attempts of the goal give attempt N the consistency "X"[, the current streak …][ and attempt M …]".</summary>
    [Then("^" + AtT + "the attempts of the goal give (attempt \\d.+)$")]
    public async Task ThenTheAttemptsGive(string at, string text)
    {
        arrange.Then(TimeOf(at));
        var attempts = await arrange.AttemptsAsync(arrange.OnlyGoal());
        foreach (var segment in Regex.Split(text, "(?:, and | and )(?=attempt \\d)"))
        {
            var head = Regex.Match(segment, "^attempt (\\d+) (.+)$");
            if (!head.Success)
            {
                throw new InvalidOperationException($"Cannot read \"{segment}\".");
            }

            var attempt = F009Checks.AttemptNumbered(attempts, F009Text.Int(head.Groups[1].Value));
            var facts = head.Groups[2].Value;
            var name = Regex.Match(facts, "^the name `([^`]+)`$");
            if (name.Success)
            {
                Assert.Equal(name.Groups[1].Value, attempt.Name);
            }
            else
            {
                F009Checks.Stats(attempt.Stats, facts);
            }
        }
    }

    /// <summary>After "reads the attempts of the goal": "they are[ exactly][, in this order,] attempt …".</summary>
    [Then("^they are((?: exactly)?)((?:, in this order,)?) (attempt \\d.*)$")]
    public void ThenTheyAreAttempts(string exactly, string inOrder, string attempts)
    {
        F009Checks.Attempts(WhenAttempts(), attempts, exactly.Length > 0, inOrder.Length > 0);
    }

    /// <summary>After "reads the attempts of the goal": "attempt N is `name`, start date D, …".</summary>
    [Then("^attempt (\\d+) is (.+)$")]
    public void ThenAttemptIs(string number, string facts)
    {
        var spec = F009Text.Attempt($"attempt {number}, {facts}");
        F009Checks.Attempt(F009Checks.AttemptNumbered(WhenAttempts(), spec.Number), spec);
    }

    // ------------------------------------------------------------ one attempt

    /// <summary>After "reads attempt N of the goal": its fields as written.</summary>
    [Then("^it gives (attempt \\d+.*)$")]
    public void ThenItGivesTheAttempt(string text)
    {
        var spec = F009Text.Attempt(text);
        var attempt = WhenAttempt().Attempt;
        F009Checks.Attempt(attempt, spec);
    }

    [Then("^its History has exactly (\\d+) entries, newest first: (.+)$")]
    public void ThenItsHistoryHasExactlyEntriesNewestFirst(string count, string entries)
    {
        var history = WhenAttempt().History.Entries;
        Assert.Equal(F009Text.Int(count), history.Count);
        F009Checks.OrderedEntries(history, F009Text.Entries(entries));
    }

    [Then("^its History is exactly, newest first, (.+)$")]
    public void ThenItsHistoryIsExactlyNewestFirst(string entries)
    {
        F009Checks.OrderedEntries(WhenAttempt().History.Entries, F009Text.Entries(entries));
    }

    [Then("^its History has exactly (\\d+) entries, each (" + F009Text.States + "), no record$")]
    public void ThenItsHistoryHasExactlyEntriesEach(string count, string state)
    {
        var history = WhenAttempt().History.Entries;
        Assert.Equal(F009Text.Int(count), history.Count);
        var expected = F009Text.StateOf(state);
        Assert.All(history, entry =>
        {
            Assert.Equal(expected, entry.State);
            Assert.Empty(entry.Records);
        });
    }

    [Then("^its History's first entry is " + D + " and its last entry is " + D + "$")]
    public void ThenItsHistorysFirstAndLastEntry(string first, string last)
    {
        var history = WhenAttempt().History.Entries;
        Assert.NotEmpty(history);
        Assert.Equal(F009Text.Date(first), history[0].FirstDate);
        Assert.Equal(F009Text.Date(last), history[^1].FirstDate);
    }

    [Then("^its History has no entry$")]
    public void ThenItsHistoryHasNoEntry() => Assert.Empty(WhenAttempt().History.Entries);

    [Then("^its consistency is \"([^\"]*)\"$")]
    public void ThenItsConsistencyIs(string consistency) =>
        Assert.Equal(consistency, WhenAttempt().Attempt.Stats.ConsistencyText);

    [Then("^its recent results are exactly (\\d+) results, oldest first: (.+)$")]
    public void ThenItsRecentResults(string count, string results)
    {
        var recent = WhenAttempt().Recent.Results;
        Assert.Equal(F009Text.Int(count), recent.Count);
        var expected = F009Text.ResultSequence(results);
        Assert.Equal(expected, recent.Select(result => (result.FirstDate, result.State)).ToList());
    }

    /// <summary>"reading attempt N of the goal gives "X"": FR-014's one-attempt use case refuses with that text.</summary>
    [Then("^reading attempt (\\d+) of the goal gives \"([^\"]*)\"$")]
    public async Task ThenReadingAttemptGives(string number, string message)
    {
        arrange.Then();
        var goal = arrange.OnlyGoal();
        var result = await driver.GetAttemptAsync(await arrange.AccountAsync(goal.OwnerEmail), goal.Id, F009Text.Int(number));
        Assert.Equal(AttemptReadOutcome.AttemptNotFound, result.Outcome);
        Assert.Equal(message, result.Message);
        Assert.Null(result.Details);
    }

    // ------------------------------------------------------------ the goal history

    /// <summary>"[at T ]the goal history[ of `x`] is|lists[ exactly][, in this order,] …" (read now by that account).</summary>
    [Then("^" + AtT + "the goal history((?: of `[^`]+`)?) (?:is|lists)((?: exactly)?)((?:, in this order,)?) ((?:\\(|`|no entry).*)$")]
    public async Task ThenTheGoalHistoryIs(string at, string ofClause, string exactly, string inOrder, string entries)
    {
        arrange.Then(TimeOf(at));
        var email = ofClause.Length > 0 ? F009Text.Backticked(ofClause).Single() : arrange.Reader();
        F009Checks.GoalHistory(await arrange.GoalHistoryAsync(email), entries, exactly.Length > 0, inOrder.Length > 0);
    }

    [Then("^the goal history is empty$")]
    public async Task ThenTheGoalHistoryIsEmpty()
    {
        arrange.Then();
        Assert.Empty(await arrange.GoalHistoryAsync(arrange.Reader()));
    }

    /// <summary>After "reads the goal history": "it is[ exactly][, in this order,] (…) and (…)".</summary>
    [Then("^it is((?: exactly)?)((?:, in this order,)?) (\\(.+)$")]
    public void ThenItIs(string exactly, string inOrder, string entries)
    {
        F009Checks.GoalHistory(WhenGoalHistory(), entries, exactly.Length > 0, inOrder.Length > 0);
    }

    [Then("^its entries are exactly, in this order, (.+)$")]
    public void ThenItsEntriesAreExactly(string entries)
    {
        F009Checks.GoalHistory(WhenGoalHistory(), entries, exactly: true, inOrder: true);
    }

    [Then("^the first entry carries the identifier of `([^`]+)` and the second the identifier of `([^`]+)`$")]
    public async Task ThenTheEntriesCarryTheIdentifiers(string first, string second)
    {
        var entries = WhenGoalHistory();
        Assert.True(entries.Count >= 2, $"The goal history has {entries.Count} entries; expected at least two.");
        Assert.Equal((await arrange.GoalNamedAsync(first)).Id, entries[0].GoalId);
        Assert.Equal((await arrange.GoalNamedAsync(second)).Id, entries[1].GoalId);
    }

    // ------------------------------------------------------------ AC-095: the earlier demo accounts

    [Then("^each goal of those accounts has exactly one attempt, attempt 1, not ended$")]
    public void ThenEachDemoGoalHasOneAttempt()
    {
        Assert.NotEmpty(context.DemoReads);
        Assert.Contains(context.DemoReads, read => read.Goals.Count > 0);
        foreach (var (email, goals, _) in context.DemoReads)
        {
            foreach (var (goal, attempts) in goals)
            {
                var attempt = Assert.Single(attempts);
                Assert.True(attempt.Number == 1, $"`{goal}` of `{email}` has attempt {attempt.Number}; expected attempt 1.");
                Assert.True(attempt.EndingDate is null, $"`{goal}` of `{email}` is ended; expected not ended.");
            }
        }
    }

    [Then("^the goal history of each of those accounts is empty$")]
    public void ThenEachDemoGoalHistoryIsEmpty()
    {
        Assert.NotEmpty(context.DemoReads);
        foreach (var (email, _, history) in context.DemoReads)
        {
            Assert.True(history.Count == 0, $"The goal history of `{email}` has {history.Count} entries; expected none.");
        }
    }

    // ------------------------------------------------------------ helpers

    private IReadOnlyList<AttemptView> WhenAttempts()
    {
        Assert.Equal(F009ReadKind.Attempts, context.ReadKind);
        return context.Attempts
            ?? throw new Xunit.Sdk.XunitException($"Reading the attempts gave no attempts: \"{context.Outcome?.Message}\".");
    }

    private AttemptDetails WhenAttempt()
    {
        Assert.Equal(F009ReadKind.Attempt, context.ReadKind);
        return context.Attempt
            ?? throw new Xunit.Sdk.XunitException($"Reading the attempt gave no attempt: \"{context.Outcome?.Message}\".");
    }

    private IReadOnlyList<EndedAttemptView> WhenGoalHistory()
    {
        Assert.Equal(F009ReadKind.GoalHistory, context.ReadKind);
        return context.GoalHistoryEntries
            ?? throw new Xunit.Sdk.XunitException($"Reading the goal history gave no entries: \"{context.Outcome?.Message}\".");
    }

    private static string? TimeOf(string at)
    {
        var match = Regex.Match(at, F009Text.T);
        return match.Success ? match.Value : null;
    }
}
