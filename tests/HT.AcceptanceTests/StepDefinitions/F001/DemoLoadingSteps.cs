using HT.AcceptanceTests.Support.F001;
using HT.Application.Operator;

namespace HT.AcceptanceTests.StepDefinitions.F001;

[Binding]
[Scope(Tag = "F001")]
public sealed class DemoLoadingSteps(F001Context context, AccountsDriver driver)
{
    [Given(@"^the AC database is reset, so no account exists$")]
    public async Task GivenTheAcDatabaseIsReset()
    {
        await AccountsDriver.ResetDatabaseAsync();
    }

    [Given(@"^demo loading ran with the demo password ""([^""]*)"" and gave (.+)$")]
    public async Task GivenDemoLoadingRanAndGave(string demoPassword, string accountResults)
    {
        var result = await driver.LoadDemoDataAsync(demoPassword);
        AssertAccountResults(result, F001Text.ParseDemoAccountResults(accountResults));
    }

    [When(@"^demo loading runs with the demo password (absent|""[^""]*""), where absent means that no value is supplied$")]
    public async Task WhenDemoLoadingRunsWithOptionalPassword(string demoPassword)
    {
        context.Record(await driver.LoadDemoDataAsync(F001Text.ParseOptional(demoPassword)));
    }

    // The optional parenthesis is a note in the step, such as "(8 characters, with a digit)".
    [When(@"^demo loading runs with the demo password ""([^""]*)""(?: \([^)]*\))?$")]
    public async Task WhenDemoLoadingRuns(string demoPassword)
    {
        context.Record(await driver.LoadDemoDataAsync(demoPassword));
    }

    [When(@"^demo loading runs again with the demo password ""([^""]*)""$")]
    public async Task WhenDemoLoadingRunsAgain(string demoPassword)
    {
        context.Record(await driver.LoadDemoDataAsync(demoPassword));
    }

    [Then(@"^the result is (.+), and names no other email$")]
    public void ThenTheResultIsPerEmailAndNamesNoOtherEmail(string accountResults)
    {
        var result = context.LastDemoLoading ?? throw new InvalidOperationException("No demo loading ran in this scenario.");
        var expected = F001Text.ParseDemoAccountResults(accountResults);
        AssertAccountResults(result, expected);
        Assert.Equal(
            expected.Select(account => account.Email).Order(StringComparer.Ordinal),
            result.Accounts.Select(account => account.Email).Order(StringComparer.Ordinal));
    }

    [Then(@"^demo loading run afterwards with the demo password ""([^""]*)"" gives (.+)$")]
    public async Task ThenDemoLoadingRunAfterwardsGives(string demoPassword, string accountResults)
    {
        var result = await driver.LoadDemoDataAsync(demoPassword);
        AssertAccountResults(result, F001Text.ParseDemoAccountResults(accountResults));
    }

    private static void AssertAccountResults(DemoLoadingResult result, IReadOnlyList<(string Email, string OutcomeText)> expected)
    {
        Assert.NotEmpty(expected);
        Assert.Equal(DemoLoadingOutcome.Loaded, result.Outcome);
        foreach (var (email, outcomeText) in expected)
        {
            var account = Assert.Single(result.Accounts, candidate => candidate.Email == email);
            Assert.Equal(outcomeText, account.Message);
            Assert.Equal(F001Text.DemoAccountOutcomeFor(outcomeText), account.Outcome);
        }
    }
}
