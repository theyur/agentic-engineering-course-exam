using System.Text.RegularExpressions;
using HT.AcceptanceTests.Support.F008;
using HT.Application.Exemptions;
using HT.Domain.Accounts;

namespace HT.AcceptanceTests.StepDefinitions.F008;

/// <summary>
/// The When steps of F008 that call this slice's use cases (excuse, remove an excusal, pause, resume, cancel a pause,
/// read the pauses) and slice 5's record, change-record and delete-record use cases. Each keeps the outcome for the
/// Then steps; none asserts it.
/// </summary>
[Binding]
[Scope(Tag = "F008")]
public sealed class WhenSteps(F008Context context, F008Driver driver, F008Arrange arrange)
{
    private const string D = "(" + F008Text.D + ")";
    private const string T = "(" + F008Text.T + ")";
    private const string Reason = "(with the reason (?:" + F008Text.ReasonToken + ")|without a reason)";
    private const string PauseDays = "(from " + F008Text.D + " until " + F008Text.D + "|with no first day until " + F008Text.D +
                                     "|from " + F008Text.D + " with no last day|with no first day and no last day)";

    // ------------------------------------------------------------ the owner's own goal

    [When("^at " + T + " `([^`]+)` excuses " + D + " " + Reason + "$")]
    public async Task WhenTheUserExcuses(string time, string email, string date, string reason)
    {
        await StartAsync(time, email);
        await ExcuseAsync(await arrange.AccountAsync(email), arrange.OnlyGoalOf(email), date, reason);
    }

    [When("^at " + T + " `([^`]+)` removes the excusal of " + D + "$")]
    public async Task WhenTheUserRemovesTheExcusal(string time, string email, string date)
    {
        await StartAsync(time, email);
        await RemoveExcusalAsync(await arrange.AccountAsync(email), arrange.OnlyGoalOf(email), date);
    }

    [When("^at " + T + " `([^`]+)` pauses the goal " + PauseDays + " " + Reason + "$")]
    public async Task WhenTheUserPauses(string time, string email, string days, string reason)
    {
        await StartAsync(time, email);
        await PauseAsync(await arrange.AccountAsync(email), arrange.OnlyGoalOf(email), days, reason);
    }

    [When("^at " + T + " `([^`]+)` resumes the goal$")]
    public async Task WhenTheUserResumes(string time, string email)
    {
        await StartAsync(time, email);
        await ResumeAsync(await arrange.AccountAsync(email), arrange.OnlyGoalOf(email));
    }

    [When("^at " + T + " `([^`]+)` cancels the pause " + D + " – " + D + "$")]
    public async Task WhenTheUserCancelsThePause(string time, string email, string first, string last)
    {
        await StartAsync(time, email);
        var goal = arrange.OnlyGoalOf(email);
        await CancelAsync(await arrange.AccountAsync(email), goal, arrange.PauseOf(goal, F008Text.Date(first), F008Text.Date(last)));
    }

    /// <summary>
    /// AC-051: "`y` cancels the pause D1 – D2 through their goal `G`": the identifier of the pause with those days (a
    /// pause of the item, whoever set it), called through `y`'s own goal `G`.
    /// </summary>
    [When("^at " + T + " `([^`]+)` cancels the pause " + D + " – " + D + " through their goal `([^`]+)`$")]
    public async Task WhenTheUserCancelsThePauseThroughTheirGoal(string time, string email, string first, string last, string goalName)
    {
        await StartAsync(time, email);
        var pause = Assert.Single(context.Pauses, candidate => candidate.FirstDay == F008Text.Date(first) && candidate.LastDay == F008Text.Date(last));
        var through = await arrange.GoalNamedAsync(email, goalName);
        Assert.NotEqual(pause.Goal.Id, through.Id);
        await CancelAsync(await arrange.AccountAsync(email), through, pause);
    }

    // ------------------------------------------------------------ records (slice 5)

    [When("^at " + T + " `([^`]+)` records (Yes|No|\\d+) for " + D + "$")]
    public async Task WhenTheUserRecords(string time, string email, string value, string date)
    {
        await StartAsync(time, email);
        var goal = arrange.OnlyGoalOf(email);
        var result = await driver.RecordAsync(await arrange.AccountAsync(email), goal.Id, F008Text.Date(date), F008Arrange.Input(value));
        context.Outcome = new F008Outcome { Message = result.Message, Record = result };
    }

    [When("^at " + T + " `([^`]+)` changes the record of " + D + " to (Yes|No|\\d+)$")]
    public async Task WhenTheUserChangesTheRecord(string time, string email, string date, string value)
    {
        await StartAsync(time, email);
        var goal = arrange.OnlyGoalOf(email);
        var record = await arrange.RecordIdAsync(goal, F008Text.Date(date));
        var result = await driver.ChangeRecordAsync(await arrange.AccountAsync(email), goal.Id, record, F008Arrange.Input(value));
        context.Outcome = new F008Outcome { Message = result.Message, Record = result };
    }

    [When("^at " + T + " `([^`]+)` deletes the record of " + D + "$")]
    public async Task WhenTheUserDeletesTheRecord(string time, string email, string date)
    {
        await StartAsync(time, email);
        var goal = arrange.OnlyGoalOf(email);
        var record = await arrange.RecordIdAsync(goal, F008Text.Date(date));
        var result = await driver.DeleteRecordAsync(await arrange.AccountAsync(email), goal.Id, record);
        context.Outcome = new F008Outcome { Message = result.Message, Record = result };
    }

    // ------------------------------------------------------------ someone else's goal (AC-072 to AC-074)

    /// <summary>"`y` (or the anonymous visitor) &lt;action&gt; on the goal of `x`": the use case for `x`'s only goal, acting as `y`.</summary>
    [When("^at " + T + " (`[^`]+`|the anonymous visitor) (.+) on the goal of `([^`]+)`$")]
    public async Task WhenSomeoneActsOnTheGoalOf(string time, string actor, string action, string ownerEmail)
    {
        await arrange.WhenAsync(time);
        var (email, user) = await arrange.ActorAsync(actor);
        context.WhenActor = email;
        var goal = arrange.OnlyGoalOf(ownerEmail);
        Match match;
        if ((match = Regex.Match(action, "^excuses " + D + " " + Reason + "$")).Success)
        {
            await ExcuseAsync(user, goal, match.Groups[1].Value, match.Groups[2].Value);
        }
        else if ((match = Regex.Match(action, "^removes the excusal of " + D + "$")).Success)
        {
            await RemoveExcusalAsync(user, goal, match.Groups[1].Value);
        }
        else if ((match = Regex.Match(action, "^pauses the goal " + PauseDays + " " + Reason + "$")).Success)
        {
            await PauseAsync(user, goal, match.Groups[1].Value, match.Groups[2].Value);
        }
        else if (action == "resumes the goal")
        {
            await ResumeAsync(user, goal);
        }
        else if ((match = Regex.Match(action, "^cancels the pause " + D + " – " + D + "$")).Success)
        {
            await CancelAsync(user, goal, arrange.PauseOf(goal, F008Text.Date(match.Groups[1].Value), F008Text.Date(match.Groups[2].Value)));
        }
        else if (action == "reads the pauses of the goal")
        {
            var result = await driver.GetPausesAsync(user, goal.Id);
            context.Outcome = new F008Outcome { Message = result.Message, Pauses = result };
        }
        else
        {
            throw new InvalidOperationException($"\"{action}\" is not a use case of this slice.");
        }
    }

    // ------------------------------------------------------------ the use cases

    private async Task StartAsync(string time, string email)
    {
        await arrange.WhenAsync(time);
        context.WhenActor = email;
    }

    private async Task ExcuseAsync(ActingUser user, F008Goal goal, string date, string reason)
    {
        var reasonText = reason == "without a reason" ? null : F008Text.Reason(reason["with the reason ".Length..]);
        var result = await driver.ExcuseAsync(user, goal.Id, F008Text.Date(date), reasonText);
        context.Outcome = new F008Outcome { Message = result.Message, Excuse = result };
    }

    private async Task RemoveExcusalAsync(ActingUser user, F008Goal goal, string date)
    {
        var result = await driver.RemoveExcusalAsync(user, goal.Id, F008Text.Date(date));
        context.Outcome = new F008Outcome { Message = result.Message, RemoveExcusal = result };
    }

    /// <summary>"from D1 until D2", "with no first day until D", "from D with no last day", "with no first day and no last day".</summary>
    private async Task PauseAsync(ActingUser user, F008Goal goal, string days, string reason)
    {
        DateOnly? first = days.StartsWith("from ", StringComparison.Ordinal) ? F008Text.Dates(days)[0] : null;
        DateOnly? last = days.Contains(" until ", StringComparison.Ordinal) ? F008Text.Dates(days)[^1] : null;
        var reasonText = reason == "without a reason" ? null : F008Text.Reason(reason["with the reason ".Length..]);
        var result = await driver.PauseAsync(user, goal.Id, first, last, reasonText);
        context.Outcome = new F008Outcome { Message = result.Message, Pause = result };
        if (result.Outcome == PauseOutcome.Paused)
        {
            arrange.RememberPause(goal, result);
        }
    }

    private async Task ResumeAsync(ActingUser user, F008Goal goal)
    {
        var result = await driver.ResumeAsync(user, goal.Id);
        context.Outcome = new F008Outcome { Message = result.Message, Resume = result };
    }

    private async Task CancelAsync(ActingUser user, F008Goal goal, F008Pause pause)
    {
        var result = await driver.CancelPauseAsync(user, goal.Id, pause.Id);
        context.Outcome = new F008Outcome { Message = result.Message, CancelPause = result };
    }
}
