using System.Reflection;
using System.Text.RegularExpressions;
using HT.AcceptanceTests.Support.F004;
using HT.Application.Habits;
using HT.Domain.Habits;

namespace HT.AcceptanceTests.StepDefinitions.F004;

/// <summary>The F004 steps on the catalog: goal counts (FR-031, FR-032), unpublishing (FR-033) and renaming.</summary>
[Binding]
[Scope(Tag = "F004")]
public sealed class CatalogSteps(F004Context context, F004Driver driver, F004Arrange arrange)
{
    private const string Actor = "(`[^`]+`|the anonymous visitor)";

    // ------------------------------------------------------------ When

    [When("^" + Actor + " browses the catalog as " + Actor + " with (the search `[^`]+`|\"Only habits I track\"(?:, the search `[^`]+` and (?:no category|the category \\w+))?)$")]
    public async Task WhenBrowsesTheCatalogAs(string actor, string actingUser, string options)
    {
        arrange.AtStepTime(null);
        Assert.Equal(actor, actingUser);
        var onlyTracked = options.StartsWith("\"Only habits I track\"", StringComparison.Ordinal);
        var search = Regex.Match(options, "the search `([^`]+)`");
        var category = Regex.Match(options, @"the category (\w+)");
        var filter = new CatalogFilter
        {
            Search = search.Success ? search.Groups[1].Value : null,
            Category = category.Success ? Enum.Parse<Category>(category.Groups[1].Value) : null,
        };
        context.Record(await driver.BrowseAsync(context.Actor(actingUser), filter, onlyTracked));
    }

    [When(@"^(`[^`]+`) browses the catalog as (`[^`]+`) with the search `([^`]+)`, and reads the details of `([^`]+)` as (`[^`]+`)$")]
    public async Task WhenBrowsesAndReadsTheDetailsAs(string actor, string browsingAs, string search, string habit, string detailsAs)
    {
        arrange.AtStepTime(null);
        Assert.Equal(actor, browsingAs);
        Assert.Equal(actor, detailsAs);
        var user = context.Actor(actor);
        context.Record(await driver.BrowseAsync(user, new CatalogFilter { Search = search }, onlyTracked: false));
        context.Record(await driver.GetDetailsAsync(user, context.CatalogHabit(habit).Id));
    }

    [When("^" + Actor + " reads the details of `([^`]+)` as " + Actor + "$")]
    public async Task WhenReadsTheDetailsAs(string actor, string habit, string actingUser)
    {
        arrange.AtStepTime(null);
        Assert.Equal(actor, actingUser);
        context.Record(await driver.GetDetailsAsync(context.Actor(actingUser), context.CatalogHabit(habit).Id));
    }

    [When(@"^the catalog is browsed without an acting user with the search `([^`]+)`, and the details of `([^`]+)` are read without an acting user$")]
    public async Task WhenBrowsedAndReadWithoutAnActingUser(string search, string habit)
    {
        arrange.AtStepTime(null);
        context.Record(await driver.BrowseAsync(new CatalogFilter { Search = search }));
        context.Record(await driver.GetDetailsAsync(context.CatalogHabit(habit).Id));
    }

    [When(@"^(`[^`]+`) unpublishes `([^`]+)` (without confirming|with confirmation)$")]
    public async Task WhenUnpublishes(string administrator, string habit, string confirmation)
    {
        arrange.AtStepTime(null);
        var id = context.CatalogHabit(habit).Id;
        context.LastUnpublishedHabitId = id;
        context.Record(await driver.UnpublishAsync(context.Actor(administrator), id, confirmation == "with confirmation"));
    }

    [When(@"^(`[^`]+`) edits `([^`]+)` to the name `([^`]+)`$")]
    public async Task WhenEditsTheCatalogHabitName(string administrator, string habit, string name)
    {
        arrange.AtStepTime(null);
        var record = context.CatalogHabit(habit);
        record.Spec.Name = name;
        context.Record(await driver.EditCatalogHabitAsync(context.Actor(administrator), record.Id, record.Spec.ToInput()));
        record.Names.Add(name);
    }

    // ------------------------------------------------------------ Then: goal counts in browsing and details

    [Then(@"^browsing gives `([^`]+)` the goal count (\d+) and the tracked count (\d+)$")]
    public void ThenBrowsingGivesTheGoalCount(string habit, int goalCount, int trackedCount)
    {
        var browse = Browse();
        Assert.Equal(goalCount, Single(browse, habit).GoalCount);
        Assert.Equal(trackedCount, browse.TrackedCount);
    }

    [Then(@"^`([^`]+)` has the goal count (\d+), (.+), and the tracked count is (\d+)$")]
    public void ThenHabitsHaveTheGoalCounts(string habit, int goalCount, string others, int trackedCount)
    {
        var browse = Browse();
        Assert.Equal(goalCount, Single(browse, habit).GoalCount);
        var items = Regex.Matches(others, @"`([^`]+)` (\d+)");
        Assert.NotEmpty(items);
        foreach (Match item in items)
        {
            Assert.Equal(F004Text.Int(item.Groups[2].Value), Single(browse, item.Groups[1].Value).GoalCount);
        }

        Assert.Equal(trackedCount, browse.TrackedCount);
    }

    [Then(@"^the result is `([^`]+)` only, with the goal count (\d+), and the tracked count is (\d+)$")]
    public void ThenTheResultIsOnlyWithTheGoalCount(string habit, int goalCount, int trackedCount)
    {
        var browse = Browse();
        var item = Assert.Single(browse.Habits);
        Assert.Equal(habit, item.Name);
        Assert.Equal(goalCount, item.GoalCount);
        Assert.Equal(trackedCount, browse.TrackedCount);
    }

    [Then(@"^the result is `([^`]+)` only$")]
    public void ThenTheResultIsOnly(string habit)
    {
        var item = Assert.Single(Browse().Habits);
        Assert.Equal(habit, item.Name);
    }

    [Then(@"^the result is `([^`]+)` and `([^`]+)`, and nothing else$")]
    public void ThenTheResultIsAndNothingElse(string first, string second)
    {
        var names = Browse().Habits.Select(habit => habit.Name).Order(StringComparer.Ordinal).ToList();
        Assert.Equal(new[] { first, second }.Order(StringComparer.Ordinal), names);
    }

    [Then(@"^the result is no habit$")]
    public void ThenTheResultIsNoHabit()
    {
        Assert.Empty(Browse().Habits);
    }

    [Then(@"^the result contains `([^`]+)` with no goal count, and no tracked count$")]
    [Then(@"^browsing contains `([^`]+)` with no goal count, and no tracked count$")]
    public void ThenBrowsingContainsWithNoGoalCount(string habit)
    {
        var browse = Browse();
        Assert.Null(Single(browse, habit).GoalCount);
        Assert.Null(browse.TrackedCount);
    }

    [Then(@"^the details give the goal count (\d+)$")]
    public void ThenTheDetailsGiveTheGoalCount(int goalCount)
    {
        Assert.Equal(goalCount, Details().GoalCount);
    }

    [Then(@"^the details give the goal count (\d+) and no goals$")]
    public void ThenTheDetailsGiveTheGoalCountAndNoGoals(int goalCount)
    {
        var details = Details();
        Assert.Equal(goalCount, details.GoalCount);
        Assert.True(details.Goals is null || details.Goals.Count == 0, "The details give goals.");
    }

    [Then(@"^the details have no goal count and no goals$")]
    public void ThenTheDetailsHaveNoGoalCount()
    {
        var details = Details();
        Assert.Null(details.GoalCount);
        Assert.True(details.Goals is null || details.Goals.Count == 0, "The details give goals.");
    }

    [Then(@"^the goals (.+), in this order, each with its identifier$")]
    public void ThenTheDetailsGiveTheGoals(string names)
    {
        var details = Details();
        Assert.NotNull(details.Goals);
        var expected = F004Text.QuotedTexts(names);
        Assert.NotEmpty(expected);
        Assert.Equal(expected, details.Goals.Select(goal => goal.Name).ToList());
        var created = context.Goals.Select(goal => goal.Id).ToHashSet();
        Assert.All(details.Goals, goal => Assert.Contains(goal.Id, created));
        Assert.Equal(details.Goals.Count, details.Goals.Select(goal => goal.Id).Distinct().Count());
    }

    [Then(@"^reading the details of `([^`]+)` without an acting user still gives that suggested starting goal the text ""([^""]*)""$")]
    public async Task ThenTheDetailsStillGiveTheSuggestionText(string habit, string text)
    {
        var details = await driver.GetDetailsAsync(context.CatalogHabit(habit).Id);
        Assert.Equal(CatalogHabitDetailsOutcome.Found, details.Outcome);
        Assert.NotNull(details.Details);
        var suggestion = Assert.Single(details.Details.SuggestedStartingGoals);
        Assert.Equal(text, suggestion.Text);
    }

    // ------------------------------------------------------------ Then: unpublishing

    [Then(@"^the result is ""confirmation needed"" with the number of goals (\d+) and the text ""([^""]*)""$")]
    public void ThenTheResultIsConfirmationNeeded(int count, string text)
    {
        var result = context.LastUnpublish ?? throw new InvalidOperationException("No habit was unpublished in the When step.");
        Assert.Equal(CatalogHabitUnpublishOutcome.ConfirmationNeeded, result.Outcome);
        Assert.Equal(count, result.GoalCount);
        Assert.Equal(text, result.Message);
    }

    [Then(@"^the result carries nothing else: no email, no goal name, no goal identifier and no setting$")]
    public void ThenTheResultCarriesNothingElse()
    {
        var result = context.LastUnpublish ?? throw new InvalidOperationException("No habit was unpublished in the When step.");
        var properties = result.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(property => property.Name)
            .Order(StringComparer.Ordinal)
            .ToList();
        Assert.Equal(new[] { "GoalCount", "Message", "Outcome" }, properties);
        Assert.NotNull(result.Message);
        Assert.DoesNotContain("@", result.Message, StringComparison.Ordinal);
        foreach (var goal in context.Goals)
        {
            Assert.DoesNotContain(goal.Id.Value.ToString(), result.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Then(@"^browsing the catalog without an acting user with the search `([^`]+)` (still contains `[^`]+`|does not contain it)$")]
    public async Task ThenBrowsingWithoutAnActingUser(string search, string claim)
    {
        var browse = await driver.BrowseAsync(new CatalogFilter { Search = search });
        Assert.Equal(CatalogBrowseOutcome.Found, browse.Outcome);
        var contains = Regex.Match(claim, "^still contains `([^`]+)`$");
        if (contains.Success)
        {
            Assert.Contains(browse.Habits, habit => habit.Name == contains.Groups[1].Value);
        }
        else
        {
            var unpublished = context.LastUnpublishedHabitId
                ?? throw new InvalidOperationException("No habit was unpublished in the When step.");
            Assert.DoesNotContain(browse.Habits, habit => habit.Id == unpublished);
        }
    }

    [Then(@"^`([^`]+)` is Unpublished$")]
    public async Task ThenTheHabitIsUnpublished(string habit)
    {
        var unpublish = LastUnpublish();
        Assert.Equal(CatalogHabitUnpublishOutcome.Unpublished, unpublish.Outcome);
        await AssertUnpublishedAsync(habit);
    }

    [Then(@"^`([^`]+)` is Unpublished, with no ""confirmation needed"" result$")]
    public async Task ThenTheHabitIsUnpublishedWithNoConfirmationNeeded(string habit)
    {
        var unpublish = LastUnpublish();
        Assert.NotEqual(CatalogHabitUnpublishOutcome.ConfirmationNeeded, unpublish.Outcome);
        Assert.Equal(CatalogHabitUnpublishOutcome.Unpublished, unpublish.Outcome);
        await AssertUnpublishedAsync(habit);
    }

    private CatalogHabitUnpublishResult LastUnpublish() =>
        context.LastUnpublish ?? throw new InvalidOperationException("No habit was unpublished in the When step.");

    private async Task AssertUnpublishedAsync(string habit)
    {
        var read = await driver.GetCatalogHabitAsync(context.Administrator, context.CatalogHabit(habit).Id);
        Assert.Equal(CatalogHabitAdminOutcome.Found, read.Outcome);
        Assert.NotNull(read.Habit);
        Assert.Equal(CatalogHabitState.Unpublished, read.Habit.State);
    }

    // ------------------------------------------------------------ helpers

    private CatalogBrowseResult Browse()
    {
        var browse = context.LastBrowse ?? throw new InvalidOperationException("The catalog was not browsed in the When step.");
        Assert.Equal(CatalogBrowseOutcome.Found, browse.Outcome);
        return browse;
    }

    private static CatalogHabitSummary Single(CatalogBrowseResult browse, string name) =>
        Assert.Single(browse.Habits, habit => habit.Name == name);

    private CatalogHabitDetails Details()
    {
        var details = context.LastDetails ?? throw new InvalidOperationException("No habit details were read in the When step.");
        Assert.Equal(CatalogHabitDetailsOutcome.Found, details.Outcome);
        return details.Details ?? throw new InvalidOperationException("Reading the details gave no details.");
    }
}
