using HT.AcceptanceTests.Support.F009;
using HT.Application.Accounts;
using HT.Application.Habits;
using HT.Application.Operator;

namespace HT.AcceptanceTests.StepDefinitions.F009;

/// <summary>The Then steps of F009 about the catalog and habits (FR-020) and demo goal-change loading (FR-031).</summary>
[Binding]
[Scope(Tag = "F009")]
public sealed class CatalogDemoThenSteps(F009Context context, F009Driver driver, F009Arrange arrange)
{
    // ------------------------------------------------------------ catalog

    /// <summary>After browsing with "Only habits I track": the habits named are listed.</summary>
    [Then("^it contains (\"[^\"]+\"(?: and \"[^\"]+\")*)$")]
    public void ThenItContainsHabits(string names)
    {
        var browse = WhenBrowse();
        foreach (var name in F009Text.Quoted(names))
        {
            Assert.Contains(browse.Habits, habit => habit.Name == name);
        }
    }

    [Then("^it does not contain \"([^\"]+)\"$")]
    public void ThenItDoesNotContainHabit(string name)
    {
        Assert.DoesNotContain(WhenBrowse().Habits, habit => habit.Name == name);
    }

    /// <summary>"browsing the catalog without that filter gives "H" the goal count N, …": the acting user's browse, not only tracked.</summary>
    [Then("^browsing the catalog without that filter gives (.+)$")]
    public async Task ThenBrowsingWithoutTheFilterGives(string counts)
    {
        arrange.Then();
        var browse = await driver.BrowseAsync(await arrange.AccountAsync(arrange.Reader()), new CatalogFilter(), onlyTracked: false);
        Assert.Equal(CatalogBrowseOutcome.Found, browse.Outcome);
        var pattern = "\"([^\"]+)\" the goal count (\\d+)";
        var matches = System.Text.RegularExpressions.Regex.Matches(counts, pattern);
        F009Text.RequireOnlySeparators(System.Text.RegularExpressions.Regex.Replace(counts, pattern, string.Empty), counts);
        Assert.NotEmpty(matches);
        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            var habit = Assert.Single(browse.Habits, candidate => candidate.Name == match.Groups[1].Value);
            Assert.Equal(F009Text.Int(match.Groups[2].Value), habit.GoalCount);
        }
    }

    /// <summary>"the details of the catalog habit "H" give the goal count N and no goal|exactly the goal `G`" (the acting user's view).</summary>
    [Then("^the details of the catalog habit \"([^\"]+)\" give the goal count (\\d+) and (no goal|exactly the goal `[^`]+`)$")]
    public async Task ThenTheDetailsGive(string habit, string count, string goals)
    {
        arrange.Then();
        var id = await arrange.CatalogHabitIdAsync(habit);
        var details = await driver.GetCatalogDetailsAsync(await arrange.AccountAsync(arrange.Reader()), id);
        Assert.Equal(CatalogHabitDetailsOutcome.Found, details.Outcome);
        var habitDetails = details.Details ?? throw new Xunit.Sdk.XunitException("The details gave no habit.");
        Assert.Equal(F009Text.Int(count), habitDetails.GoalCount);
        var listed = (habitDetails.Goals ?? []).Select(goal => goal.Name).ToList();
        var expected = goals == "no goal" ? new List<string>() : F009Text.Backticked(goals);
        Assert.Equal(expected, listed);
    }

    /// <summary>"the habit is not unpublished": the unpublish use case did not unpublish it, and the administrator's list shows it Published.</summary>
    [Then("^the habit is not unpublished$")]
    public async Task ThenTheHabitIsNotUnpublished()
    {
        var result = context.Outcome?.Unpublish ?? throw new InvalidOperationException("The When step unpublished no habit.");
        Assert.NotEqual(CatalogHabitUnpublishOutcome.Unpublished, result.Outcome);
        arrange.Then();
        var administrator = await arrange.AccountAsync(context.WhenActor!);
        var (name, id) = Assert.Single(context.CatalogHabits);
        var list = await driver.ListCatalogHabitsAsync(administrator, name);
        Assert.Equal(CatalogHabitListOutcome.Found, list.Outcome);
        var habit = Assert.Single(list.Habits, candidate => candidate.Id == id);
        Assert.Equal(HT.Domain.Habits.CatalogHabitState.Published, habit.State);
    }

    [Then("^browsing the catalog as the anonymous visitor contains \"([^\"]+)\"$")]
    public async Task ThenBrowsingAsTheAnonymousVisitorContains(string name)
    {
        arrange.Then();
        var browse = await driver.BrowseAsync(new CatalogFilter { Search = name });
        Assert.Equal(CatalogBrowseOutcome.Found, browse.Outcome);
        Assert.Contains(browse.Habits, habit => habit.Name == name);
    }

    /// <summary>"the custom habit is not deleted": the delete use case did not delete it, and it can still be read.</summary>
    [Then("^the custom habit is not deleted$")]
    public async Task ThenTheCustomHabitIsNotDeleted()
    {
        var result = context.Outcome?.CustomHabitDelete ?? throw new InvalidOperationException("The When step deleted no custom habit.");
        Assert.NotEqual(CustomHabitDeleteOutcome.Deleted, result.Outcome);
        arrange.Then();
        var email = context.WhenActor!;
        var (_, id) = Assert.Single(context.CustomHabits, entry => entry.Key.Owner == email);
        var read = await driver.GetCustomHabitAsync(await arrange.AccountAsync(email), id);
        Assert.Equal(CustomHabitOutcome.Found, read.Outcome);
    }

    [Then("^reading the custom habit `([^`]+)` gives it, with (\\d+) goals? that uses? it$")]
    public async Task ThenReadingTheCustomHabitGivesIt(string habit, string count)
    {
        arrange.Then();
        var email = arrange.Reader();
        var read = await driver.GetCustomHabitAsync(await arrange.AccountAsync(email), context.CustomHabits[(email, habit)]);
        Assert.Equal(CustomHabitOutcome.Found, read.Outcome);
        var view = read.Habit ?? throw new Xunit.Sdk.XunitException("Reading the custom habit gave no habit.");
        Assert.Equal(habit, view.Name);
        Assert.Equal(F009Text.Int(count), view.GoalCount);
    }

    // ------------------------------------------------------------ demo goal-change loading

    [Then("^its results are(?:, in this order,)? (\\(.+)$")]
    public void ThenItsResultsAre(string results)
    {
        var result = context.Outcome?.DemoGoalChange ?? throw new InvalidOperationException("The When step ran no demo goal-change loading.");
        Assert.Equal(F009Text.DemoLines(results), F009Arrange.Lines(result));
    }

    /// <summary>FR-031: the results name the demo email but no goal, habit, note, outcome, unit or amount.</summary>
    [Then("^no result contains (\".+)$")]
    public void ThenNoResultContains(string texts)
    {
        var result = context.Outcome?.DemoGoalChange ?? throw new InvalidOperationException("The When step ran no demo goal-change loading.");
        var lines = F009Arrange.Lines(result);
        Assert.NotEmpty(lines);
        foreach (var text in F009Text.Quoted(texts))
        {
            foreach (var (_, line) in lines)
            {
                Assert.DoesNotContain(text, line, StringComparison.Ordinal);
            }
        }
    }

    [Then("^sign-in with `([^`]+)` and `([^`]+)` (succeeds|does not succeed)$")]
    public async Task ThenSignIn(string email, string password, string outcome)
    {
        var result = await driver.SignInAsync(email, password);
        if (outcome == "succeeds")
        {
            Assert.Equal(SignInOutcome.SignedIn, result.Outcome);
        }
        else
        {
            Assert.NotEqual(SignInOutcome.SignedIn, result.Outcome);
        }
    }

    private CatalogBrowseResult WhenBrowse()
    {
        Assert.Equal(F009ReadKind.Catalog, context.ReadKind);
        return context.Browse ?? throw new InvalidOperationException("The When step did not browse the catalog.");
    }
}
