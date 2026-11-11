using HT.AcceptanceTests.Support.F001;
using HT.Application.Accounts;
using HT.Domain.Accounts;

namespace HT.AcceptanceTests.StepDefinitions.F001;

[Binding]
[Scope(Tag = "F001")]
public sealed class SignInSteps(F001Context context, AccountsDriver driver)
{
    [Given(@"^sign-ins with ""([^""]*)"" and ""([^""]*)"" failed at (.+), each with ""([^""]*)""$")]
    public async Task GivenSignInsFailedAt(string email, string password, string instants, string message)
    {
        var times = F001Text.ParseInstantList(instants);
        Assert.NotEmpty(times);
        foreach (var time in times)
        {
            await SignInFailsAtAsync(email, password, time, message);
        }
    }

    [Given(@"^sign-in with ""([^""]*)"" and ""([^""]*)"" failed at ""([^""]*)"" with ""([^""]*)""$")]
    public async Task GivenSignInFailedAt(string email, string password, string instant, string message)
    {
        await SignInFailsAtAsync(email, password, F001Text.ParseInstant(instant), message);
    }

    [Given(@"^sign-in with ""([^""]*)"" and ""([^""]*)"" succeeded at ""([^""]*)""$")]
    public async Task GivenSignInSucceededAt(string email, string password, string instant)
    {
        driver.SetNow(F001Text.ParseInstant(instant));
        await SignInSuccessfullyAsync(email, password);
    }

    [Given(@"^""([^""]*)"" is signed in with ""([^""]*)""$")]
    public async Task GivenIsSignedInWith(string email, string password)
    {
        await SignInSuccessfullyAsync(email, password);
    }

    [When(@"^someone signs in with the email ""([^""]*)"" and the password ""([^""]*)""$")]
    public async Task WhenSomeoneSignsIn(string email, string password)
    {
        context.Record(await driver.SignInAsync(email, password));
    }

    [When(@"^someone signs in with ""([^""]*)"" and ""([^""]*)"" at ""([^""]*)""$")]
    public async Task WhenSomeoneSignsInAt(string email, string password, string instant)
    {
        driver.SetNow(F001Text.ParseInstant(instant));
        context.Record(await driver.SignInAsync(email, password));
    }

    [When(@"^at ""([^""]*)"" someone signs in with ""([^""]*)"" and ""([^""]*)"", with ""([^""]*)"" and ""([^""]*)"", and with ""([^""]*)"" and ""([^""]*)""$")]
    public async Task WhenAtSomeoneSignsInThreeTimes(
        string instant,
        string firstEmail, string firstPassword,
        string secondEmail, string secondPassword,
        string thirdEmail, string thirdPassword)
    {
        driver.SetNow(F001Text.ParseInstant(instant));
        var results = new List<SignInResult>
        {
            await driver.SignInAsync(firstEmail, firstPassword),
            await driver.SignInAsync(secondEmail, secondPassword),
            await driver.SignInAsync(thirdEmail, thirdPassword),
        };
        context.Record(results);
    }

    [Then(@"^sign-in succeeds$")]
    public void ThenSignInSucceeds()
    {
        var result = context.LastSignIn ?? throw new InvalidOperationException("No sign-in ran in this scenario.");
        Assert.Equal(SignInOutcome.SignedIn, result.Outcome);
        Assert.NotNull(result.ActingUser);
        Assert.False(result.ActingUser.IsAnonymous);
        Assert.NotNull(result.ActingUser.AccountId);
    }

    [Then(@"^sign-in with ""([^""]*)"" and ""([^""]*)"" succeeds$")]
    public async Task ThenSignInWithSucceeds(string email, string password)
    {
        await SignInSuccessfullyAsync(email, password);
    }

    [Then(@"^sign-in with ""([^""]*)"" and ""([^""]*)"" fails with ""([^""]*)""$")]
    public async Task ThenSignInWithFails(string email, string password, string message)
    {
        var result = await driver.SignInAsync(email, password);
        Assert.Equal(SignInOutcome.Failed, result.Outcome);
        Assert.Equal(message, result.Message);
        Assert.Null(result.ActingUser);
    }

    [Then(@"^the three results are the same: the same named outcome, with the text ""([^""]*)""$")]
    public void ThenTheThreeResultsAreTheSame(string message)
    {
        var results = context.LastSignIns;
        Assert.Equal(3, results.Count);
        var outcome = Assert.IsType<SignInOutcome>(F001Text.OutcomeFor(ResultKind.SignIn, message));
        Assert.All(results, result =>
        {
            Assert.Equal(outcome, result.Outcome);
            Assert.Equal(message, result.Message);
            Assert.Null(result.ActingUser);
        });
        Assert.Equal(results[0].Outcome, results[1].Outcome);
        Assert.Equal(results[0].Outcome, results[2].Outcome);
    }

    private async Task SignInFailsAtAsync(string email, string password, DateTimeOffset time, string message)
    {
        driver.SetNow(time);
        var result = await driver.SignInAsync(email, password);
        Assert.Equal(SignInOutcome.Failed, result.Outcome);
        Assert.Equal(message, result.Message);
    }

    private async Task SignInSuccessfullyAsync(string email, string password)
    {
        var result = await driver.SignInAsync(email, password);
        Assert.Equal(SignInOutcome.SignedIn, result.Outcome);
        var actingUser = Assert.IsType<ActingUser>(result.ActingUser);
        Assert.False(actingUser.IsAnonymous);
        context.ActingUser = actingUser;
        context.SignedInAccounts[email] = actingUser;
    }
}
