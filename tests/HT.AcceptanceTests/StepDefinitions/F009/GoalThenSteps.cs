using System.Text.RegularExpressions;
using HT.AcceptanceTests.Support.F009;
using HT.Application.Goals;

namespace HT.AcceptanceTests.StepDefinitions.F009;

/// <summary>
/// The Then steps of F009 that read a goal (slice 4's read-goal use case with FR-005 and FR-017) or the starting values
/// of a new attempt (FR-011). "reading the goal …" reads at the step's time as the goal's owner (or as the account the
/// step names); "it gives …" checks the goal the When step read.
/// </summary>
[Binding]
[Scope(Tag = "F009")]
public sealed class GoalThenSteps(F009Context context, F009Arrange arrange)
{
    private const string D = "(" + F009Text.D + ")";
    private const string T = "(" + F009Text.T + ")";
    private const string AtT = "((?:at " + F009Text.T + " )?)";
    private const string Name = "(" + F009Text.NameToken + ")";

    /// <summary>"reading the goal", "reading the goal by `a`", "reading `G`" or "it" (the goal the When step read).</summary>
    private const string Reader = "(reading the goal|reading the goal by `[^`]+`|reading `[^`]+`|it)";

    // ------------------------------------------------------------ settings

    [Then("^" + AtT + Reader + " gives the settings in effect (.+), from " + D + "((?:, and no later settings)?)$")]
    public async Task ThenTheSettingsInEffect(string at, string reader, string settings, string from, string noLater)
    {
        var goal = await GoalAsync(at, reader);
        F009Checks.InEffect(goal, settings, F009Text.Date(from));
        if (noLater.Length > 0)
        {
            F009Checks.NoLater(goal);
        }
    }

    [Then("^" + AtT + Reader + " gives the later settings (.+), from " + D + "$")]
    public async Task ThenTheLaterSettings(string at, string reader, string settings, string from)
    {
        var goal = await GoalAsync(at, reader);
        F009Checks.Later(goal, settings, F009Text.Date(from));
    }

    [Then("^" + AtT + Reader + " gives no later settings$")]
    public async Task ThenNoLaterSettings(string at, string reader)
    {
        F009Checks.NoLater(await GoalAsync(at, reader));
    }

    [Then("^the settings in effect are (.+), from " + D + "$")]
    public void ThenTheSettingsInEffectAre(string settings, string from) =>
        F009Checks.InEffect(WhenRead(), settings, F009Text.Date(from));

    [Then("^it has the later settings (.+), from " + D + "$")]
    public void ThenItHasTheLaterSettings(string settings, string from) =>
        F009Checks.Later(WhenRead(), settings, F009Text.Date(from));

    [Then("^it has no later settings$")]
    public void ThenItHasNoLaterSettings() => F009Checks.NoLater(WhenRead());

    // ------------------------------------------------------------ names

    [Then("^its name is " + Name + "$")]
    public void ThenItsNameIs(string name) => Assert.Equal(F009Text.Text(name), WhenRead().Name);

    /// <summary>"… gives the name N[, which is its own|not its own]" (FR-004: an empty own name is none).</summary>
    [Then("^" + AtT + Reader + " gives the name " + Name + "((?:, which is (?:its own|not its own))?)$")]
    public async Task ThenTheName(string at, string reader, string name, string ownClause)
    {
        var goal = await GoalAsync(at, reader);
        Assert.Equal(F009Text.Text(name), goal.Name);
        if (ownClause.Length > 0)
        {
            Assert.Equal(ownClause == ", which is its own", goal.HasOwnName);
        }
    }

    [Then("^" + AtT + Reader + " gives the name " + Name + " and no later settings$")]
    public async Task ThenTheNameAndNoLaterSettings(string at, string reader, string name)
    {
        var goal = await GoalAsync(at, reader);
        Assert.Equal(F009Text.Text(name), goal.Name);
        F009Checks.NoLater(goal);
    }

    [Then("^reading the goal gives the habit's name `([^`]+)`$")]
    public async Task ThenTheHabitsName(string habit)
    {
        var goal = await GoalAsync(string.Empty, "reading the goal");
        Assert.Equal(habit, goal.Habit.Name);
    }

    /// <summary>AC-065: the goal read by the identifier its creation gave keeps it, its own name and its habit.</summary>
    [Then("^reading the goal by the identifier it was created with gives the name \"([^\"]+)\", which is its own, and the habit's name `([^`]+)`$")]
    public async Task ThenTheGoalByItsIdentifier(string name, string habit)
    {
        arrange.Then();
        var created = arrange.OnlyGoal();
        var goal = await arrange.ReadGoalAsync(created);
        Assert.Equal(created.Id, goal.Id);
        Assert.Equal(name, goal.Name);
        Assert.True(goal.HasOwnName, "The goal's name is not its own.");
        Assert.Equal(habit, goal.Habit.Name);
    }

    // ------------------------------------------------------------ ending, attempt and start date

    /// <summary>"… gives it ended|not ended[ with …]": every fact written is checked (FR-005, FR-017).</summary>
    [Then("^" + AtT + Reader + " gives it ((?:not )?ended.*)$")]
    public async Task ThenItIsEndedOrNot(string at, string reader, string clause)
    {
        F009Checks.GoalState(await GoalAsync(at, reader), clause);
    }

    [Then("^" + AtT + Reader + " gives its latest attempt as attempt (\\d+) with the start date " + D + "((?:, not ended)?)$")]
    public async Task ThenItsLatestAttempt(string at, string reader, string number, string start, string notEnded)
    {
        var goal = await GoalAsync(at, reader);
        Assert.Equal(F009Text.Int(number), goal.AttemptNumber);
        Assert.Equal(F009Text.Date(start), goal.AttemptStartDate);
        if (notEnded.Length > 0)
        {
            Assert.False(goal.IsEnded, "The goal is ended; expected not ended.");
        }
    }

    /// <summary>AC-090: the goal's latest attempt number, and one attempt of its attempts (FR-014) as written.</summary>
    [Then("^reading `([^`]+)` gives its latest attempt as attempt (\\d+), and its attempts give (attempt .+)$")]
    public async Task ThenTheLatestAttemptAndAnEarlierOne(string name, string number, string attempt)
    {
        arrange.Then();
        var goal = await arrange.GoalNamedAsync(name);
        var read = await arrange.ReadGoalAsync(goal);
        Assert.Equal(F009Text.Int(number), read.AttemptNumber);
        var spec = F009Text.Attempt(attempt);
        F009Checks.Attempt(F009Checks.AttemptNumbered(await arrange.AttemptsAsync(goal), spec.Number), spec);
    }

    // ------------------------------------------------------------ starting values of a new attempt

    [Then("^" + AtT + "the starting values of a new attempt are (.+?)((?: and the start date " + F009Text.D + ")?)$")]
    public async Task ThenTheStartingValues(string at, string settings, string startClause)
    {
        arrange.Then(TimeOf(at));
        var values = await arrange.StartingValuesAsync(arrange.OnlyGoal());
        F009Checks.StartingValues(values, settings, StartOf(startClause));
    }

    [Then("^they are ((?:Build|Reduce|no Build or Reduce), .+) and the start date " + D + "$")]
    public void ThenTheyAre(string settings, string start)
    {
        Assert.Equal(F009ReadKind.StartingValues, context.ReadKind);
        var values = context.StartingValues
            ?? throw new Xunit.Sdk.XunitException($"Reading the starting values gave no values: \"{context.Outcome?.Message}\".");
        F009Checks.StartingValues(values, settings, F009Text.Date(start));
    }

    // ------------------------------------------------------------ helpers

    private GoalDetails WhenRead()
    {
        Assert.Equal(F009ReadKind.Goal, context.ReadKind);
        return context.GoalRead ?? throw new InvalidOperationException("The When step read no goal.");
    }

    /// <summary>The goal the phrase names, read now ("it": the goal the When step read, at its time).</summary>
    private async Task<GoalDetails> GoalAsync(string at, string reader)
    {
        if (reader == "it")
        {
            if (at.Length > 0)
            {
                throw new InvalidOperationException("\"at T it gives\" is not a phrase of this file.");
            }

            return WhenRead();
        }

        arrange.Then(TimeOf(at));
        var by = Regex.Match(reader, "^reading the goal by `([^`]+)`$");
        if (by.Success)
        {
            return await arrange.ReadGoalAsync(arrange.OnlyGoalOf(by.Groups[1].Value));
        }

        var named = Regex.Match(reader, "^reading `([^`]+)`$");
        if (named.Success)
        {
            return await arrange.ReadGoalAsync(await arrange.GoalNamedAsync(named.Groups[1].Value));
        }

        return await arrange.ReadGoalAsync(arrange.OnlyGoal());
    }

    private static string? TimeOf(string at)
    {
        var match = Regex.Match(at, T);
        return match.Success ? match.Groups[1].Value : null;
    }

    private static DateOnly? StartOf(string clause)
    {
        var match = Regex.Match(clause, D);
        return match.Success ? F009Text.Date(match.Groups[1].Value) : null;
    }
}
