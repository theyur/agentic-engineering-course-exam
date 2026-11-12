using HT.AcceptanceTests.Support.F001;

namespace HT.AcceptanceTests.StepDefinitions.F001;

[Binding]
[Scope(Tag = "F001")]
public sealed class AdministratorCreationSteps(F001Context context, AccountsDriver driver)
{
    [Given(@"^administrator creation ran with ""([^""]*)"" and ""([^""]*)"" and gave ""([^""]*)""$")]
    public async Task GivenAdministratorCreationRanWith(string email, string password, string message)
    {
        await CreateAdministratorAndExpectAsync(email, password, message);
    }

    [Given(@"^administrator creation ran with the email ""([^""]*)"" and the password ""([^""]*)"" and gave ""([^""]*)""$")]
    public async Task GivenAdministratorCreationRanWithTheEmailAndThePassword(string email, string password, string message)
    {
        await CreateAdministratorAndExpectAsync(email, password, message);
    }

    [Given(@"^the email (absent|""[^""]*"") and the password (absent|""[^""]*""), where absent means that no value is supplied$")]
    public void GivenTheEmailAndThePassword(string email, string password)
    {
        context.ConfiguredAdministratorEmail = F001Text.ParseOptional(email);
        context.ConfiguredAdministratorPassword = F001Text.ParseOptional(password);
        context.AdministratorConfigurationGiven = true;
    }

    // The optional parenthesis is a note in the step, such as "(8 characters, with a digit)".
    [When(@"^administrator creation runs with the email ""([^""]*)"" and the password ""([^""]*)""(?: \([^)]*\))?$")]
    public async Task WhenAdministratorCreationRuns(string email, string password)
    {
        context.Record(await driver.CreateAdministratorAsync(email, password));
    }

    [When(@"^administrator creation runs with the email ""([^""]*)"" and the password absent, that is, with no password supplied$")]
    public async Task WhenAdministratorCreationRunsWithoutPassword(string email)
    {
        context.Record(await driver.CreateAdministratorAsync(email, null));
    }

    [When(@"^administrator creation runs with them$")]
    public async Task WhenAdministratorCreationRunsWithThem()
    {
        Assert.True(context.AdministratorConfigurationGiven, "No administrator email and password were given in this scenario.");
        context.Record(await driver.CreateAdministratorAsync(
            context.ConfiguredAdministratorEmail, context.ConfiguredAdministratorPassword));
    }

    private async Task CreateAdministratorAndExpectAsync(string email, string password, string message)
    {
        var result = await driver.CreateAdministratorAsync(email, password);
        Assert.Equal(message, result.Message);
        Assert.Equal(F001Text.OutcomeFor(ResultKind.AdministratorCreation, message), (Enum)result.Outcome);
    }
}
