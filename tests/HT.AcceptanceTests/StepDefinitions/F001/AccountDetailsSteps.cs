using HT.AcceptanceTests.Support.F001;
using HT.Application.Accounts;
using HT.Domain.Accounts;

namespace HT.AcceptanceTests.StepDefinitions.F001;

[Binding]
[Scope(Tag = "F001")]
public sealed class AccountDetailsSteps(F001Context context, AccountsDriver driver)
{
    [When(@"^they ask for account details$")]
    public async Task WhenTheyAskForAccountDetails()
    {
        context.Record(await driver.GetAccountDetailsAsync(context.ActingUser));
    }

    [Then(@"^the result is the email ""([^""]*)"" and the role (registered user|administrator)$")]
    public void ThenTheResultIsTheEmailAndTheRole(string email, string role)
    {
        var result = context.LastAccountDetails ?? throw new InvalidOperationException("No account details were asked for in this scenario.");
        AssertDetails(result, email, F001Text.ParseRole(role));
    }

    [Then(@"^the result does not contain ""([^""]*)""$")]
    public void ThenTheResultDoesNotContain(string otherEmail)
    {
        var result = context.LastAccountDetails ?? throw new InvalidOperationException("No account details were asked for in this scenario.");
        var details = Assert.IsType<AccountDetails>(result.AccountDetails);
        Assert.NotEqual(otherEmail, details.Email, StringComparer.OrdinalIgnoreCase);
        Assert.DoesNotContain(otherEmail, details.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(otherEmail, result.Message ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Then(@"^account details for ""([^""]*)"" signed in with ""([^""]*)"" give the email ""([^""]*)"" and the role (registered user|administrator)$")]
    public async Task ThenAccountDetailsForGiveTheEmailAndTheRole(string signInEmail, string password, string email, string role)
    {
        var actingUser = await SignInSuccessfullyAsync(signInEmail, password);
        var result = await driver.GetAccountDetailsAsync(actingUser);
        AssertDetails(result, email, F001Text.ParseRole(role));
    }

    [Then(@"^account details for ""([^""]*)"" signed in with ""([^""]*)"" give the role (registered user|administrator)$")]
    public async Task ThenAccountDetailsForGiveTheRole(string signInEmail, string password, string role)
    {
        var actingUser = await SignInSuccessfullyAsync(signInEmail, password);
        var result = await driver.GetAccountDetailsAsync(actingUser);
        Assert.Equal(AccountDetailsOutcome.Found, result.Outcome);
        var details = Assert.IsType<AccountDetails>(result.AccountDetails);
        Assert.Equal(F001Text.ParseRole(role), details.Role);
    }

    [Then(@"^account details of the signed-in account give the email ""([^""]*)"" and the role (registered user|administrator)$")]
    public async Task ThenAccountDetailsOfTheSignedInAccountGive(string email, string role)
    {
        var signIn = context.LastSignIn ?? throw new InvalidOperationException("No sign-in ran in this scenario.");
        var actingUser = Assert.IsType<ActingUser>(signIn.ActingUser);
        var result = await driver.GetAccountDetailsAsync(actingUser);
        AssertDetails(result, email, F001Text.ParseRole(role));
    }

    private async Task<ActingUser> SignInSuccessfullyAsync(string email, string password)
    {
        var signIn = await driver.SignInAsync(email, password);
        Assert.Equal(SignInOutcome.SignedIn, signIn.Outcome);
        var actingUser = Assert.IsType<ActingUser>(signIn.ActingUser);
        Assert.False(actingUser.IsAnonymous);
        context.ActingUser = actingUser;
        context.SignedInAccounts[email] = actingUser;
        return actingUser;
    }

    private static void AssertDetails(AccountDetailsResult result, string email, Role role)
    {
        Assert.Equal(AccountDetailsOutcome.Found, result.Outcome);
        var details = Assert.IsType<AccountDetails>(result.AccountDetails);
        Assert.Equal(email, details.Email);
        Assert.Equal(role, details.Role);
    }
}
