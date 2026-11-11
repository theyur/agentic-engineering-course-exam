using HT.AcceptanceTests.Support.F001;
using HT.Application.Accounts;
using HT.Domain.Accounts;

namespace HT.AcceptanceTests.StepDefinitions.F001;

[Binding]
[Scope(Tag = "F001")]
public sealed class RegistrationSteps(F001Context context, AccountsDriver driver)
{
    [Given(@"^the anonymous visitor$")]
    public void GivenTheAnonymousVisitor()
    {
        context.ActingUser = ActingUser.Anonymous;
    }

    [Given(@"^""([^""]*)"" registered with the password ""([^""]*)""$")]
    public async Task GivenRegisteredWithThePassword(string email, string password)
    {
        await RegisterSuccessfullyAsync(email, password);
    }

    [Given(@"^""([^""]*)"" and ""([^""]*)"" registered with the password ""([^""]*)""$")]
    public async Task GivenBothRegisteredWithThePassword(string firstEmail, string secondEmail, string password)
    {
        await RegisterSuccessfullyAsync(firstEmail, password);
        await RegisterSuccessfullyAsync(secondEmail, password);
    }

    [Given(@"^""([^""]*)"" registered with the password ""([^""]*)"" after them$")]
    public async Task GivenRegisteredWithThePasswordAfterThem(string email, string password)
    {
        await RegisterSuccessfullyAsync(email, password);
    }

    [When(@"^the anonymous visitor registers with the email ""([^""]*)"" and the password ""([^""]*)""$")]
    public async Task WhenTheAnonymousVisitorRegisters(string email, string password)
    {
        context.ActingUser = ActingUser.Anonymous;
        context.Record(await driver.RegisterAsync(email, password));
    }

    [When(@"^they register with the email ""([^""]*)"" and the password ""([^""]*)""$")]
    public async Task WhenTheyRegister(string email, string password)
    {
        Assert.True(context.ActingUser.IsAnonymous, "Registration is done by the anonymous visitor.");
        context.Record(await driver.RegisterAsync(email, password));
    }

    [Then(@"^registration succeeds$")]
    public void ThenRegistrationSucceeds()
    {
        var result = context.LastRegistration ?? throw new InvalidOperationException("No registration ran in this scenario.");
        Assert.Equal(RegistrationOutcome.Registered, result.Outcome);
        Assert.NotNull(result.AccountId);
    }

    [Then(@"^registering ""([^""]*)"" with the password ""([^""]*)"" afterwards succeeds$")]
    public async Task ThenRegisteringAfterwardsSucceeds(string email, string password)
    {
        await RegisterSuccessfullyAsync(email, password);
    }

    private async Task RegisterSuccessfullyAsync(string email, string password)
    {
        var result = await driver.RegisterAsync(email, password);
        Assert.Equal(RegistrationOutcome.Registered, result.Outcome);
        Assert.NotNull(result.AccountId);
    }
}
