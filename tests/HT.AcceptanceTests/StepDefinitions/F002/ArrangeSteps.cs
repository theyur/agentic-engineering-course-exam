using HT.AcceptanceTests.Support.F002;
using HT.Application.Accounts;
using HT.Application.Habits;
using HT.Application.Operator;

namespace HT.AcceptanceTests.StepDefinitions.F002;

/// <summary>The Given steps that arrange accounts, catalog habits and custom habits.</summary>
[Binding]
[Scope(Tag = "F002")]
public sealed class ArrangeSteps(F002Context context, F002Driver driver, F002Arrange arrange)
{
    [Given(@"^the AC database is reset, so no account and no habit exists$")]
    public async Task GivenTheAcDatabaseIsReset()
    {
        await F002Driver.ResetDatabaseAsync();
    }

    [Given(@"^the administrator `([^`]+)`$")]
    public async Task GivenTheAdministrator(string email)
    {
        await arrange.AdministratorAsync(email);
    }

    [Given(@"^the administrator `([^`]+)` and the registered user `([^`]+)`$")]
    public async Task GivenTheAdministratorAndTheRegisteredUser(string administratorEmail, string userEmail)
    {
        await arrange.AdministratorAsync(administratorEmail);
        await arrange.RegisteredUserAsync(userEmail);
    }

    [Given(@"^the administrator `([^`]+)`(?:,| and) (an? (?:Published|Draft|Unpublished) `.+)$")]
    public async Task GivenTheAdministratorAndCatalogHabits(string email, string habits)
    {
        await arrange.AdministratorAsync(email);
        await arrange.CatalogHabitsAsync(habits);
    }

    [Given(@"^(an? (?:Published|Draft|Unpublished) `.+)$")]
    public async Task GivenCatalogHabits(string habits)
    {
        await arrange.CatalogHabitsAsync(habits);
    }

    [Given(@"^the registered user `([^`]+)`$")]
    public async Task GivenTheRegisteredUser(string email)
    {
        await arrange.RegisteredUserAsync(email);
    }

    /// <summary>The " with ..." clause always takes part in the match; an empty capture means the step gives no fields.</summary>
    [Given(@"^the registered user `([^`]+)`, who created the custom habit `([^`]+)`((?: with .+)?)$")]
    public async Task GivenTheRegisteredUserWhoCreatedTheCustomHabit(string email, string name, string clause)
    {
        await arrange.RegisteredUserAsync(email);
        await arrange.CustomHabitAsync(email, name, string.IsNullOrEmpty(clause) ? null : clause);
    }

    [Given(@"^the registered user `([^`]+)`, who created the custom habits `([^`]+)` and (?:then )?`([^`]+)`$")]
    public async Task GivenTheRegisteredUserWhoCreatedTheCustomHabits(string email, string firstName, string secondName)
    {
        await arrange.RegisteredUserAsync(email);
        await arrange.CustomHabitAsync(email, firstName, null);
        await arrange.CustomHabitAsync(email, secondName, null);
    }

    [Given(@"^the registered user `([^`]+)`, who created and then deleted the custom habit `([^`]+)`$")]
    public async Task GivenTheRegisteredUserWhoCreatedAndThenDeletedTheCustomHabit(string email, string name)
    {
        var user = await arrange.RegisteredUserAsync(email);
        var record = await arrange.CustomHabitAsync(email, name, null);
        var deleted = await driver.DeleteCustomHabitAsync(user, record.Id);
        Assert.Equal(CustomHabitDeleteOutcome.Deleted, deleted.Outcome);
    }

    [Given(@"^the anonymous visitor registered `([^`]+)` with the password `([^`]+)`$")]
    public async Task GivenTheAnonymousVisitorRegistered(string email, string password)
    {
        var registered = await driver.RegisterAsync(email, password);
        Assert.Equal(RegistrationOutcome.Registered, registered.Outcome);
        context.Passwords[email] = password;
    }

    [Given(@"^administrator creation ran with the email `([^`]+)` and the password `([^`]+)` and gave ""([^""]*)""$")]
    public async Task GivenAdministratorCreationRan(string email, string password, string text)
    {
        var created = await driver.CreateAdministratorAsync(email, password);
        Assert.Equal(text, created.Message);
        Assert.Equal(AdministratorCreationOutcome.Created, created.Outcome);
        context.Passwords[email] = password;
    }
}
