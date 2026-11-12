using HT.AcceptanceTests.Support.F001;
using HT.Application.Accounts;

namespace HT.AcceptanceTests.StepDefinitions.F001;

[Binding]
[Scope(Tag = "F001")]
public sealed class PasswordChangeSteps(F001Context context, AccountsDriver driver)
{
    [Given(@"^""([^""]*)"" changed its password giving ""([^""]*)"" as the current password and ""([^""]*)"" as the new one$")]
    public async Task GivenChangedItsPassword(string email, string currentPassword, string newPassword)
    {
        Assert.True(context.SignedInAccounts.ContainsKey(email), $"\"{email}\" has not signed in in this scenario.");
        var result = await driver.ChangePasswordAsync(context.SignedInAccounts[email], currentPassword, newPassword);
        Assert.Equal(PasswordChangeOutcome.Changed, result.Outcome);
    }

    // The optional parenthesis is a note in the step, such as "(8 characters, with a digit)".
    [When(@"^they change their password giving ""([^""]*)"" as the current password and ""([^""]*)""(?: \([^)]*\))? as the new one$")]
    public async Task WhenTheyChangeTheirPassword(string currentPassword, string newPassword)
    {
        Assert.False(context.ActingUser.IsAnonymous, "\"They\" is the account signed in by an earlier step.");
        context.Record(await driver.ChangePasswordAsync(context.ActingUser, currentPassword, newPassword));
    }

    [When(@"^they change a password giving ""([^""]*)"" as the current password and ""([^""]*)"" as the new one$")]
    public async Task WhenTheyChangeAPassword(string currentPassword, string newPassword)
    {
        Assert.True(context.ActingUser.IsAnonymous, "\"They\" is the anonymous visitor.");
        context.Record(await driver.ChangePasswordAsync(context.ActingUser, currentPassword, newPassword));
    }

    [Then(@"^the change succeeds$")]
    public void ThenTheChangeSucceeds()
    {
        var result = context.LastPasswordChange ?? throw new InvalidOperationException("No password change ran in this scenario.");
        Assert.Equal(PasswordChangeOutcome.Changed, result.Outcome);
    }
}
