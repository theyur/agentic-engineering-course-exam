using HT.AcceptanceTests.Support.F009;
using HT.Application.Exemptions;
using HT.Application.Goals;
using HT.Application.Records;

namespace HT.AcceptanceTests.StepDefinitions.F009;

/// <summary>The Then steps of F009 about the result of the use case the When step called.</summary>
[Binding]
[Scope(Tag = "F009")]
public sealed class ResultThenSteps(F009Context context)
{
    private const string D = "(" + F009Text.D + ")";

    private F009Outcome Outcome => context.Outcome ?? throw new InvalidOperationException("The item's When step called no use case that gives a result.");

    /// <summary>A refusal or a result written by its text only: the result's text is exactly that.</summary>
    [Then("^the result is \"([^\"]*)\"$")]
    public void ThenTheResultIs(string text)
    {
        Assert.Equal(text, Outcome.Message);
        if (Outcome.SettingsChange is { } change && text == GoalChangeTexts.SettingsUnchanged)
        {
            Assert.Equal(SettingsChangeOutcome.Unchanged, change.Outcome);
            Assert.Null(change.AppliesFrom);
        }
    }

    [Then("^the result is \"settings changed\" with the date " + D + "$")]
    public void ThenTheSettingsChangedWithTheDate(string date)
    {
        var result = Outcome.SettingsChange ?? throw new InvalidOperationException("The When step changed no settings.");
        Assert.Equal(SettingsChangeOutcome.Changed, result.Outcome);
        Assert.Equal("settings changed", result.Message);
        Assert.Equal(F009Text.Date(date), result.AppliesFrom);
    }

    [Then("^the result is \"settings changed\" with no date$")]
    public void ThenTheSettingsChangedWithNoDate()
    {
        var result = Outcome.SettingsChange ?? throw new InvalidOperationException("The When step changed no settings.");
        Assert.Equal(SettingsChangeOutcome.Changed, result.Outcome);
        Assert.Equal("settings changed", result.Message);
        Assert.Null(result.AppliesFrom);
    }

    [Then("^the result is \"goal ended\" with the ending date " + D + " and the last date " + D + "$")]
    public void ThenTheGoalEnded(string ending, string last)
    {
        var result = Outcome.End ?? throw new InvalidOperationException("The When step ended no goal.");
        Assert.Equal(GoalEndOutcome.Ended, result.Outcome);
        Assert.Equal("goal ended", result.Message);
        Assert.Equal(F009Text.Date(ending), result.EndingDate);
        Assert.Equal(F009Text.Date(last), result.LastDate);
    }

    [Then("^the result is \"attempt started\" with the number (\\d+) and the start date " + D + "$")]
    public void ThenTheAttemptStarted(string number, string start)
    {
        var result = Outcome.AttemptStart ?? throw new InvalidOperationException("The When step started no attempt.");
        Assert.True(result.Outcome == AttemptStartOutcome.Started, $"Starting the attempt gave {result.Outcome}: \"{result.Message}\".");
        Assert.Equal("attempt started", result.Message);
        Assert.Equal(F009Text.Int(number), result.AttemptNumber);
        Assert.Equal(F009Text.Date(start), result.StartDate);
    }

    /// <summary>"the state S, correction yes|no": the record use case succeeded and gives the entry's state and the correction flag.</summary>
    [Then("^the result is the state (" + F009Text.States + "), correction (yes|no)$")]
    public void ThenTheResultIsTheState(string state, string correction)
    {
        var result = Outcome.Record ?? throw new InvalidOperationException("The When step wrote no record.");
        Assert.True(
            result.Outcome is RecordWriteOutcome.Recorded or RecordWriteOutcome.Changed or RecordWriteOutcome.Deleted,
            $"The record use case gave {result.Outcome}: \"{result.Message}\".");
        var entry = result.Entry ?? throw new Xunit.Sdk.XunitException("The record result gives no entry.");
        Assert.Equal(F009Text.StateOf(state), entry.State);
        Assert.Equal(correction == "yes", result.IsCorrection);
    }

    /// <summary>"it succeeds": the excusal, excusal removal, record change or record deletion of the When step succeeded.</summary>
    [Then("^it succeeds$")]
    public void ThenItSucceeds()
    {
        var outcome = Outcome;
        if (outcome.Excuse is { } excuse)
        {
            Assert.True(excuse.Outcome == ExcuseOutcome.Excused, $"Excusing gave {excuse.Outcome}: \"{excuse.Message}\".");
        }
        else if (outcome.RemoveExcusal is { } removal)
        {
            Assert.True(removal.Outcome == RemoveExcusalOutcome.Removed, $"Removing the excusal gave {removal.Outcome}: \"{removal.Message}\".");
        }
        else if (outcome.Record is { } record)
        {
            Assert.True(
                record.Outcome is RecordWriteOutcome.Changed or RecordWriteOutcome.Deleted or RecordWriteOutcome.Recorded,
                $"The record use case gave {record.Outcome}: \"{record.Message}\".");
        }
        else
        {
            throw new InvalidOperationException("The When step called no use case that \"it succeeds\" can check.");
        }
    }
}
