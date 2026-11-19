using HT.AcceptanceTests.Support.F005;

namespace HT.AcceptanceTests.StepDefinitions.F005;

/// <summary>The When steps of F005 that call the recording use cases (FR-001 to FR-018).</summary>
[Binding]
[Scope(Tag = "F005")]
public sealed class WhenSteps(F005Arrange arrange, F005Actions actions)
{
    /// <summary>The optional "at `T` " prefix; the group always takes part (empty when the step gives no time).</summary>
    private const string At = "((?:at `[^`]+` )?)";
    private const string Actor = "(the anonymous visitor|the administrator `[^`]+`|`[^`]+`)";

    /// <summary>
    /// "records V for D (with the note N) (on G)", "changes the record of D to V", "deletes the record of D",
    /// "saves the note N for D", and sequences of them ("A, then B", "A and then B", "A, B and C").
    /// </summary>
    [When("^" + At + Actor + " ((?:records|changes|deletes|saves) .+)$")]
    public async Task WhenTheActorWrites(string time, string actor, string action)
    {
        await arrange.BeginWhenAsync(F005Text.AtTime(time));
        await actions.RunAsync(actor, action);
    }

    [When("^" + At + Actor + " reads (.+)$")]
    public async Task WhenTheActorReads(string time, string actor, string what)
    {
        await arrange.BeginWhenAsync(F005Text.AtTime(time));
        await actions.ReadAsync(actor, what);
    }

    [When("^" + Actor + " calls (record|change, with the owner's record identifier|delete, with the owner's record identifier|save note|history|recordable now|recent notes) with (the goal of `[^`]+`|a goal identifier that belongs to no goal)$")]
    public async Task WhenTheActorCallsAUseCase(string actor, string useCase, string goal)
    {
        await arrange.BeginWhenAsync(null);
        await actions.CallAsync(actor, useCase, goal);
    }
}
