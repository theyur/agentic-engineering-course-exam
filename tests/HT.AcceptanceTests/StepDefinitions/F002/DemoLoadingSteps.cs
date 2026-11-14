using HT.AcceptanceTests.Support.F002;
using HT.Application.Habits;
using HT.Application.Operator;
using HT.Domain.Habits;

namespace HT.AcceptanceTests.StepDefinitions.F002;

/// <summary>Demo loading with the seed catalog and the demo custom habits (FR-030, FR-031), an operator capability.</summary>
[Binding]
[Scope(Tag = "F002")]
public sealed class DemoLoadingSteps(F002Context context, F002Driver driver, F002Arrange arrange)
{
    // ------------------------------------------------------------------ Given

    [Given(@"^demo loading ran with the demo password `([^`]+)`$")]
    public async Task GivenDemoLoadingRan(string demoPassword)
    {
        var result = await LoadAsync(demoPassword);
        Assert.Equal(DemoLoadingOutcome.Loaded, result.Outcome);
    }

    [Given(@"^demo loading ran with the demo password `([^`]+)` and its catalog result was ""([^""]*)""$")]
    public async Task GivenDemoLoadingRanWithTheCatalogResult(string demoPassword, string catalogText)
    {
        var result = await LoadAsync(demoPassword);
        Assert.Equal(DemoLoadingOutcome.Loaded, result.Outcome);
        AssertCatalogResult(result, catalogText);
    }

    [Given(@"^the administrator `([^`]+)` listed the catalog habits, with their names, states and total$")]
    public async Task GivenTheAdministratorListedTheCatalogHabits(string email)
    {
        var administrator = await arrange.AdministratorAsync(email);
        var list = await driver.ListCatalogAsync(administrator, null);
        Assert.Equal(CatalogHabitListOutcome.Found, list.Outcome);
        context.RememberedCatalogList = list;
    }

    [Given(@"^`([^`]+)`, signed in with `([^`]+)`, deleted `([^`]+)`$")]
    public async Task GivenSignedInDeletedOne(string email, string password, string name)
    {
        await SignInAndDeleteAsync(email, password, [name]);
    }

    [Given(@"^`([^`]+)`, signed in with `([^`]+)`, deleted `([^`]+)` and `([^`]+)`$")]
    public async Task GivenSignedInDeletedTwo(string email, string password, string firstName, string secondName)
    {
        await SignInAndDeleteAsync(email, password, [firstName, secondName]);
    }

    private async Task SignInAndDeleteAsync(string email, string password, IReadOnlyList<string> names)
    {
        var user = await arrange.SignInAsync(email, password);
        var list = await driver.ListCustomHabitsAsync(user);
        Assert.Equal(CustomHabitListOutcome.Found, list.Outcome);
        foreach (var name in names)
        {
            var habit = Assert.Single(list.Habits, candidate => candidate.Name == name);
            var deleted = await driver.DeleteCustomHabitAsync(user, habit.Id);
            Assert.Equal(CustomHabitDeleteOutcome.Deleted, deleted.Outcome);
        }
    }

    // ------------------------------------------------------------------ When

    [When(@"^demo loading runs with the demo password `([^`]+)`$")]
    public async Task WhenDemoLoadingRuns(string demoPassword)
    {
        context.Record(await LoadAsync(demoPassword));
    }

    [When(@"^demo loading runs again with the demo password `([^`]+)`$")]
    public async Task WhenDemoLoadingRunsAgain(string demoPassword)
    {
        context.Record(await LoadAsync(demoPassword));
    }

    [When(@"^demo loading runs with the demo password (absent|`[^`]*`), where absent means that no value is supplied$")]
    public async Task WhenDemoLoadingRunsWithOptionalPassword(string demoPassword)
    {
        context.Record(await LoadAsync(demoPassword == "absent" ? null : F002Text.ParseTextExpression(demoPassword).Full));
    }

    // ------------------------------------------------------------------ Then

    [Then(@"^the catalog result is ""([^""]*)""$")]
    public void ThenTheCatalogResultIs(string text)
    {
        AssertCatalogResult(Demo(), text);
    }

    [Then(@"^the account result is ""([^""]*)"" and the custom-habit result is ""([^""]*)"" for (.+)$")]
    public void ThenTheAccountResultAndTheCustomHabitResultAre(string accountText, string customHabitsText, string emails)
    {
        var result = Demo();
        var expected = F002Text.ParseBacktickList(emails);
        Assert.NotEmpty(expected);
        foreach (var email in expected)
        {
            AssertAccountResult(result, email, accountText);
            AssertCustomHabitsResult(result, email, customHabitsText);
        }
    }

    [Then(@"^the account result is (?!.*custom-habit result)(.+)$")]
    public void ThenTheAccountResultIs(string perEmail)
    {
        var result = Demo();
        var expected = F002Text.ParsePerEmail(perEmail);
        Assert.NotEmpty(expected);
        foreach (var (email, text) in expected)
        {
            AssertAccountResult(result, email, text);
        }
    }

    [Then(@"^the custom-habit result is ""([^""]*)"" for (.+), and there is no custom-habit result for `([^`]+)`$")]
    public void ThenTheCustomHabitResultIsAndNoneFor(string text, string emails, string withoutEmail)
    {
        var result = Demo();
        var expected = F002Text.ParseBacktickList(emails);
        Assert.NotEmpty(expected);
        foreach (var email in expected)
        {
            AssertCustomHabitsResult(result, email, text);
        }

        Assert.Null(Account(result, withoutEmail).CustomHabits);
    }

    [Then(@"^there is no custom-habit result for `([^`]+)`$")]
    public void ThenThereIsNoCustomHabitResultFor(string email)
    {
        Assert.Null(Account(Demo(), email).CustomHabits);
    }

    [Then(@"^the result has no custom-habit result for any account$")]
    public void ThenTheResultHasNoCustomHabitResultForAnyAccount()
    {
        var result = Demo();
        Assert.Equal(DemoLoadingOutcome.Loaded, result.Outcome);
        Assert.NotEmpty(result.Accounts);
        Assert.All(result.Accounts, account => Assert.Null(account.CustomHabits));
    }

    [Then(@"^no result text of demo loading contains any of the names (.+)$")]
    public void ThenNoResultTextContainsAnyOfTheNames(string names)
    {
        var result = Demo();
        var texts = new List<string?> { result.Message, result.Catalog?.Message };
        foreach (var account in result.Accounts)
        {
            texts.Add(account.Email);
            texts.Add(account.Message);
            texts.Add(account.CustomHabits?.Message);
        }

        var written = texts.Where(text => text is not null).Select(text => text!).ToList();
        Assert.NotEmpty(written);
        foreach (var name in F002Text.ParseBacktickList(names))
        {
            Assert.DoesNotContain(written, text => text.Contains(name, StringComparison.OrdinalIgnoreCase));
        }
    }

    [Then(@"^signed in with `([^`]+)`, `([^`]+)` lists exactly ((?:`[^`]+`(?:, )?)+)$")]
    public async Task ThenSignedInListsExactly(string password, string email, string names)
    {
        var user = await arrange.SignInAsync(email, password);
        Assert.Equal(F002Text.ParseBacktickList(names), await ListNamesAsync(user));
    }

    [Then(@"^`([^`]+)` lists exactly ((?:`[^`]+`(?:, )?)+)$")]
    public async Task ThenListsExactly(string email, string names)
    {
        var user = await arrange.SignInKnownAsync(email);
        Assert.Equal(F002Text.ParseBacktickList(names), await ListNamesAsync(user));
    }

    [Then(@"^`([^`]+)` lists no custom habit$")]
    public async Task ThenListsNoCustomHabit(string email)
    {
        var user = await arrange.SignInKnownAsync(email);
        Assert.Empty(await ListNamesAsync(user));
    }

    [Then(@"^`([^`]+)`, signed in with `([^`]+)`, lists no custom habit$")]
    public async Task ThenSignedInListsNoCustomHabit(string email, string password)
    {
        var user = await arrange.SignInAsync(email, password);
        Assert.Empty(await ListNamesAsync(user));
    }

    [Then(@"^`([^`]+)`, signed in with `([^`]+)`, listing custom habits gets ""([^""]*)""$")]
    public async Task ThenSignedInListingCustomHabitsGets(string email, string password, string text)
    {
        var user = await arrange.SignInAsync(email, password);
        var list = await driver.ListCustomHabitsAsync(user);
        Assert.Equal(text, list.Message);
        Assert.Equal(F002Text.OutcomeFor(typeof(CustomHabitListOutcome), text, aboutSuggestedStartingGoal: false), list.Outcome);
        Assert.Empty(list.Habits ?? []);
    }

    [Then(@"^signed in with `([^`]+)`, each demo account lists exactly these custom habits:$")]
    public async Task ThenEachDemoAccountListsExactlyTheseCustomHabits(string password, DataTable table)
    {
        var rowsByAccount = table.Rows
            .Select(row => (
                Account: F002Text.ParseTextExpression(row["account"]).Full!,
                Name: row["name"],
                Category: F002Text.ParseCategory(row["category"]),
                Intent: F002Text.ParseIntent(row["intent"]),
                Icon: F002Text.ParseTextExpression(row["icon"]).Full!))
            .GroupBy(row => row.Account, StringComparer.Ordinal)
            .ToList();
        Assert.NotEmpty(rowsByAccount);

        foreach (var account in rowsByAccount)
        {
            var user = await arrange.SignInAsync(account.Key, password);
            var list = await driver.ListCustomHabitsAsync(user);
            Assert.Equal(CustomHabitListOutcome.Found, list.Outcome);
            var expected = account.ToList();
            Assert.Equal(expected.Select(row => row.Name).ToList(), list.Habits.Select(habit => habit.Name).ToList());
            for (var index = 0; index < expected.Count; index++)
            {
                Assert.Equal(expected[index].Category, list.Habits[index].Category);
                Assert.Equal(expected[index].Intent, list.Habits[index].Intent);
                Assert.Equal(expected[index].Icon, list.Habits[index].Icon);
                context.DemoCustomHabitIds.Add(list.Habits[index].Id);
            }
        }
    }

    [Then(@"^no identifier of these six custom habits is the identifier of a habit that browsing gives$")]
    public async Task ThenNoDemoCustomHabitIsInBrowsing()
    {
        Assert.Equal(6, context.DemoCustomHabitIds.Count);
        var browsed = (await driver.BrowseAsync(new CatalogFilter())).Habits.Select(habit => habit.Id).ToList();
        foreach (var id in context.DemoCustomHabitIds)
        {
            Assert.DoesNotContain(id, browsed);
        }
    }

    [Then(@"^no identifier of these six custom habits is the identifier of a habit of the administrator's list$")]
    public async Task ThenNoDemoCustomHabitIsInTheAdministratorsList()
    {
        Assert.Equal(6, context.DemoCustomHabitIds.Count);
        var list = await driver.ListCatalogAsync(context.Administrator, null);
        Assert.Equal(CatalogHabitListOutcome.Found, list.Outcome);
        var listed = list.Habits.Select(habit => habit.Id).ToList();
        foreach (var id in context.DemoCustomHabitIds)
        {
            Assert.DoesNotContain(id, listed);
        }
    }

    [Then(@"^demo loading run afterwards with the demo password `([^`]+)` gives ""([^""]*)"" for (.+), ""([^""]*)"" for each of them, and the catalog result ""([^""]*)""$")]
    public async Task ThenDemoLoadingRunAfterwardsGives(string demoPassword, string accountText, string emails, string customHabitsText, string catalogText)
    {
        var result = await LoadAsync(demoPassword);
        Assert.Equal(DemoLoadingOutcome.Loaded, result.Outcome);
        var expected = F002Text.ParseBacktickList(emails);
        Assert.NotEmpty(expected);
        foreach (var email in expected)
        {
            AssertAccountResult(result, email, accountText);
            AssertCustomHabitsResult(result, email, customHabitsText);
        }

        AssertCatalogResult(result, catalogText);
    }

    // ------------------------------------------------------------------ helpers

    private async Task<DemoLoadingResult> LoadAsync(string? demoPassword)
    {
        context.LastDemoPassword = demoPassword;
        var result = await driver.LoadDemoDataAsync(demoPassword);
        context.LastDemoLoading = result;
        return result;
    }

    private DemoLoadingResult Demo() =>
        context.LastDemoLoading ?? throw new InvalidOperationException("No demo loading ran in this scenario.");

    private static DemoAccountResult Account(DemoLoadingResult result, string email)
    {
        Assert.Equal(DemoLoadingOutcome.Loaded, result.Outcome);
        return Assert.Single(result.Accounts, account => account.Email == email);
    }

    private static void AssertAccountResult(DemoLoadingResult result, string email, string text)
    {
        var account = Account(result, email);
        Assert.Equal(text, account.Message);
        Assert.Equal(F002Text.DemoAccountOutcomeFor(text), account.Outcome);
    }

    private static void AssertCustomHabitsResult(DemoLoadingResult result, string email, string text)
    {
        var customHabits = Account(result, email).CustomHabits;
        Assert.NotNull(customHabits);
        Assert.Equal(text, customHabits.Message);
        Assert.Equal(F002Text.DemoCustomHabitsOutcomeFor(text), customHabits.Outcome);
    }

    private static void AssertCatalogResult(DemoLoadingResult result, string text)
    {
        Assert.Equal(DemoLoadingOutcome.Loaded, result.Outcome);
        Assert.NotNull(result.Catalog);
        Assert.Equal(text, result.Catalog.Message);
        Assert.Equal(F002Text.DemoCatalogOutcomeFor(text), result.Catalog.Outcome);
    }

    private async Task<List<string>> ListNamesAsync(HT.Domain.Accounts.ActingUser user)
    {
        var list = await driver.ListCustomHabitsAsync(user);
        Assert.Equal(CustomHabitListOutcome.Found, list.Outcome);
        return list.Habits.Select(habit => habit.Name).ToList();
    }
}
