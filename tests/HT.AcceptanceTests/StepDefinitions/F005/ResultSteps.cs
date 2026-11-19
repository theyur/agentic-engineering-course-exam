using HT.AcceptanceTests.Support.F005;
using HT.Domain.Evaluation;

namespace HT.AcceptanceTests.StepDefinitions.F005;

/// <summary>The Then steps of F005 about the results of the When step's use cases (FR-005, FR-006, FR-018).</summary>
[Binding]
[Scope(Tag = "F005")]
public sealed class ResultSteps(F005Context context, F005Arrange arrange, F005Checks checks)
{
    private const string State = "(Upcoming|Due|Pending|Met|Failed|Missed)";

    /// <summary>
    /// "recording | changing | deleting | it succeeds with the state S, a correction | not a correction", "each recording
    /// succeeds …", "deleting and recording both succeed …", optionally followed by ", and the history … gives …".
    /// </summary>
    [Then("^(recording|changing|deleting|each recording|it|deleting and recording both) succeeds? with the state " + State
        + ", (a correction|not a correction)(.*)$")]
    public async Task ThenTheWriteSucceeds(string which, string state, string correction, string rest)
    {
        await arrange.AtThenTimeAsync();
        var expected = Enum.Parse<EvaluationState>(state);
        var isCorrection = correction == "a correction";
        switch (which)
        {
            case "recording":
                F005Checks.Succeeded(Assert.Single(context.Results), F005Kind.Record, expected, isCorrection);
                break;
            case "changing":
                F005Checks.Succeeded(Assert.Single(context.Results), F005Kind.Change, expected, isCorrection);
                break;
            case "deleting":
                F005Checks.Succeeded(Assert.Single(context.Results), F005Kind.Delete, expected, isCorrection);
                break;
            case "each recording":
                Assert.NotEmpty(context.Results);
                foreach (var result in context.Results)
                {
                    F005Checks.Succeeded(result, F005Kind.Record, expected, isCorrection);
                }

                break;
            case "it":
            {
                var result = Assert.Single(context.Results);
                Assert.Contains(result.Kind, new[] { F005Kind.Record, F005Kind.Change });
                F005Checks.Succeeded(result, result.Kind, expected, isCorrection);
                break;
            }

            case "deleting and recording both":
                Assert.Equal(2, context.Results.Count);
                F005Checks.Succeeded(context.Results[0], F005Kind.Delete, expected, isCorrection);
                F005Checks.Succeeded(context.Results[1], F005Kind.Record, expected, isCorrection);
                break;
            default:
                throw new InvalidOperationException(which);
        }

        await checks.RestAsync(rest);
    }

    [Then("^the three changes succeed with the states " + State + ", " + State + " and " + State + ", each a correction$")]
    public async Task ThenTheThreeChangesSucceed(string first, string second, string third)
    {
        await arrange.AtThenTimeAsync();
        Assert.Equal(3, context.Results.Count);
        F005Checks.Succeeded(context.Results[0], F005Kind.Change, Enum.Parse<EvaluationState>(first), isCorrection: true);
        F005Checks.Succeeded(context.Results[1], F005Kind.Change, Enum.Parse<EvaluationState>(second), isCorrection: true);
        F005Checks.Succeeded(context.Results[2], F005Kind.Delete, Enum.Parse<EvaluationState>(third), isCorrection: true);
    }

    /// <summary>"the result is "M"": the When step's one use case gave the refusal with the text M; then the rest of the step.</summary>
    [Then("^the result is \"([^\"]+)\"(.*)$")]
    public async Task ThenTheResultIs(string message, string rest)
    {
        await arrange.AtThenTimeAsync();
        var result = Assert.Single(context.Results);
        Assert.False(result.IsSuccess, $"The {result.Kind} use case succeeded with {result.Outcome}.");
        Assert.NotEqual(F005Checks.SuccessOutcome(result.Kind), result.Outcome);
        Assert.Equal(message, result.Message);
        await checks.RestAsync(rest);
    }

    [Then(@"^nothing is recorded$")]
    public async Task ThenNothingIsRecorded()
    {
        await arrange.AtThenTimeAsync();
        await checks.NothingIsRecordedAsync();
    }

    /// <summary>"saving the note succeeds" (FR-007), optionally followed by " and the history … gives …".</summary>
    [Then("^saving the note succeeds(.*)$")]
    public async Task ThenSavingTheNoteSucceeds(string rest)
    {
        await arrange.AtThenTimeAsync();
        var result = Assert.Single(context.Results);
        Assert.Equal(F005Kind.SaveNote, result.Kind);
        Assert.Equal(F005Checks.SuccessOutcome(F005Kind.SaveNote), result.Outcome);
        await checks.RestAsync(rest);
    }

    /// <summary>"both succeed": both note saves of the When step succeeded; then the rest of the step.</summary>
    [Then("^both succeed(.*)$")]
    public async Task ThenBothSucceed(string rest)
    {
        await arrange.AtThenTimeAsync();
        Assert.Equal(2, context.Results.Count);
        foreach (var result in context.Results)
        {
            Assert.Equal(F005Kind.SaveNote, result.Kind);
            Assert.Equal(F005Checks.SuccessOutcome(F005Kind.SaveNote), result.Outcome);
        }

        await checks.RestAsync(rest);
    }
}
