using HT.AcceptanceTests.Support.F001;

namespace HT.AcceptanceTests.StepDefinitions.F001;

[Binding]
[Scope(Tag = "F001")]
public sealed class ResultSteps(F001Context context)
{
    /// <summary>
    /// The result of the last When step carries exactly this text and the named outcome the spec gives it
    /// (spec Clarifications: every refusal and operator result has a named outcome and its literal text).
    /// </summary>
    [Then(@"^the result is ""([^""]*)""$")]
    public void ThenTheResultIs(string text)
    {
        var last = context.LastResult ?? throw new InvalidOperationException("No use case ran in a When step of this scenario.");
        Assert.Equal(text, last.Message);
        Assert.Equal(F001Text.OutcomeFor(last.Kind, text), last.Outcome);
    }
}
