using HT.AcceptanceTests.Support.F008;
using HT.Application.Accounts;
using HT.Domain.Evaluation;

namespace HT.AcceptanceTests.StepDefinitions.F008;

/// <summary>The When and Then steps of F008 on demo pause loading (FR-030, FR-031).</summary>
[Binding]
[Scope(Tag = "F008")]
public sealed class DemoSteps(F008Context context, F008Driver driver, F008Arrange arrange)
{
    private const string T = "(" + F008Text.T + ")";

    /// <summary>"demo pause loading runs at T with the demo password `P`" / "… with no demo password".</summary>
    [When("^demo pause loading runs at " + T + " (with the demo password `[^`]*`|with no demo password)$")]
    public async Task WhenDemoPauseLoadingRuns(string time, string password)
    {
        await arrange.WhenAsync(time);
        var demoPassword = password == "with no demo password" ? null : F008Text.Backticked(password).Single();
        context.DemoPause = await driver.LoadDemoPausesAsync(demoPassword);
    }

    [Then("^its results are(?:, in this order,)? (.+)$")]
    public void ThenItsResultsAre(string results)
    {
        var result = context.DemoPause ?? throw new InvalidOperationException("Demo pause loading has not run.");
        Assert.Equal(F008Text.DemoLines(results), F008Arrange.Lines(result));
    }

    [Then("^no result contains (.+)$")]
    public void ThenNoResultContains(string texts)
    {
        var result = context.DemoPause ?? throw new InvalidOperationException("Demo pause loading has not run.");
        var lines = F008Arrange.Lines(result);
        Assert.NotEmpty(lines);
        foreach (var forbidden in F008Text.Quoted(texts))
        {
            foreach (var (email, text) in lines)
            {
                Assert.DoesNotContain(forbidden, text, StringComparison.Ordinal);
                Assert.DoesNotContain(forbidden, email ?? string.Empty, StringComparison.Ordinal);
            }
        }
    }

    [Then("^sign-in with `([^`]+)` and `([^`]+)` succeeds$")]
    public async Task ThenSignInSucceeds(string email, string password)
    {
        arrange.Then();
        var signIn = await driver.SignInAsync(email, password);
        Assert.Equal(SignInOutcome.SignedIn, signIn.Outcome);
    }

    [Then("^sign-in with `([^`]+)` and `([^`]+)` does not succeed$")]
    public async Task ThenSignInDoesNotSucceed(string email, string password)
    {
        arrange.Then();
        var signIn = await driver.SignInAsync(email, password);
        Assert.NotEqual(SignInOutcome.SignedIn, signIn.Outcome);
    }

    [Then("^no goal of those accounts has a pause$")]
    public void ThenNoGoalHasAPause()
    {
        Assert.NotEmpty(context.DemoReads);
        foreach (var (email, goal, pauses, _) in context.DemoReads)
        {
            Assert.True(pauses.Count == 0, $"`{goal}` of {email} has {pauses.Count} pauses.");
        }
    }

    [Then("^no history entry of those goals is Excused or Paused$")]
    public void ThenNoHistoryEntryIsExempt()
    {
        Assert.NotEmpty(context.DemoReads);
        foreach (var (email, goal, _, history) in context.DemoReads)
        {
            var exempt = history.Entries.Where(entry => entry.State is EvaluationState.Excused or EvaluationState.Paused).ToList();
            Assert.True(
                exempt.Count == 0,
                $"`{goal}` of {email} has exempt entries: {string.Join(", ", exempt.Select(entry => $"{entry.FirstDate:yyyy-MM-dd} {entry.State}"))}.");
        }
    }
}
