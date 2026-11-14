using HT.AcceptanceTests.Support.F002;
using HT.Application.Habits;
using HT.Domain.Habits;

namespace HT.AcceptanceTests.StepDefinitions.F002;

/// <summary>
/// Browsing, the category filter, the Home preview and catalog details (FR-008 to FR-010, FR-017).
/// The public catalog use cases take no acting user: FR-008 gives the same result to every audience, so the
/// acting user a step names is resolved (it must exist in the scenario) and the same use case runs for all.
/// "browsing" in a Then step is the browse use case run at that point with no filter unless the step names one.
/// </summary>
[Binding]
[Scope(Tag = "F002")]
public sealed class BrowsingSteps(F002Context context, F002Driver driver)
{
    private const string Actor = F002Text.Actor;

    // ------------------------------------------------------------------ When

    [When("^" + Actor + " browses the catalog$")]
    public async Task WhenBrowsesTheCatalog(string actor)
    {
        context.ResolveActor(actor);
        context.LastBrowse = await driver.BrowseAsync(new CatalogFilter());
    }

    [When("^" + Actor + " browses the catalog with ((?:(?! and reads the details of ).)+)$")]
    public async Task WhenBrowsesTheCatalogWith(string actor, string filter)
    {
        context.ResolveActor(actor);
        context.LastBrowse = await driver.BrowseAsync(F002Text.ParseFilter(filter));
    }

    [When("^" + Actor + " browses the catalog with (.+) and reads the details of `([^`]+)`$")]
    public async Task WhenBrowsesAndReadsTheDetails(string actor, string filter, string name)
    {
        context.ResolveActor(actor);
        context.LastBrowse = await driver.BrowseAsync(F002Text.ParseFilter(filter));
        var record = context.Habit(name, HabitKind.Catalog);
        context.CurrentHabit = record;
        context.Record(await driver.GetDetailsAsync(record.Id));
    }

    [When("^" + Actor + " asks for the category filter$")]
    public async Task WhenAsksForTheCategoryFilter(string actor)
    {
        context.ResolveActor(actor);
        context.LastCategoryFilter = await driver.GetCategoryFilterAsync();
    }

    [When("^" + Actor + " asks for the Home preview$")]
    public async Task WhenAsksForTheHomePreview(string actor)
    {
        context.ResolveActor(actor);
        context.LastHomePreview = await driver.GetHomePreviewAsync();
    }

    [When("^" + Actor + " reads the details of `([^`]+)`$")]
    public async Task WhenReadsTheDetailsOf(string actor, string name)
    {
        context.ResolveActor(actor);
        var record = context.Habit(name, HabitKind.Catalog);
        context.CurrentHabit = record;
        context.Record(await driver.GetDetailsAsync(record.Id));
    }

    [When("^" + Actor + " reads the catalog details of (`[^`]+`|an unknown identifier)$")]
    public async Task WhenReadsTheCatalogDetailsOf(string actor, string habit)
    {
        context.ResolveActor(actor);
        var id = habit == "an unknown identifier"
            ? new HabitId(Guid.NewGuid())
            : context.Habit(F002Text.ParseBacktickList(habit).Single(), HabitKind.Catalog).Id;
        context.Record(await driver.GetDetailsAsync(id));
    }

    [When("^" + Actor + " reads the catalog details with the identifier of `([^`]+)`$")]
    public async Task WhenReadsTheCatalogDetailsWithTheIdentifierOf(string actor, string name)
    {
        context.ResolveActor(actor);
        context.Record(await driver.GetDetailsAsync(context.Habit(name).Id));
    }

    // ------------------------------------------------------------------ Then: the browse result of the When step

    [Then(@"^the result contains `([^`]+)` with (.+)$")]
    public void ThenTheResultContainsWith(string name, string clause)
    {
        var habit = Assert.Single(Browse().Habits, candidate => candidate.Name == name);
        F002Assert.HabitMatches(clause, habit.Category, habit.Intent, habit.Icon, habit.Description, habit.MarkedSuggestedStartingGoal);
    }

    [Then(@"^the result (contains|does not contain) `([^`]+)` and (contains|does not contain) `([^`]+)`$")]
    public void ThenTheResultContainsAndContains(string firstVerb, string firstName, string secondVerb, string secondName)
    {
        AssertContains(Browse(), firstVerb, firstName);
        AssertContains(Browse(), secondVerb, secondName);
    }

    [Then(@"^the result does not contain `([^`]+)` or `([^`]+)`$")]
    public void ThenTheResultDoesNotContainOr(string firstName, string secondName)
    {
        AssertContains(Browse(), "does not contain", firstName);
        AssertContains(Browse(), "does not contain", secondName);
    }

    [Then(@"^the result is (`.+`), in this order(?: \(.*\))?$")]
    public void ThenTheResultIsInThisOrder(string names)
    {
        Assert.Equal(F002Text.ParseBacktickList(names), Browse().Habits.Select(habit => habit.Name).ToList());
    }

    [Then(@"^the result is exactly `([^`]+)`$")]
    public void ThenTheResultIsExactly(string name)
    {
        Assert.Equal(new[] { name }, Browse().Habits.Select(habit => habit.Name).ToList());
    }

    [Then(@"^the result has no habit and the count 0$")]
    public void ThenTheResultHasNoHabitAndTheCount0()
    {
        Assert.Empty(Browse().Habits);
        Assert.Equal(0, Browse().Count);
    }

    [Then(@"^the result has the (\d+) habits (.+), in this order, and the count (\d+)$")]
    public void ThenTheResultHasTheHabitsAndTheCount(int habitCount, string names, int count)
    {
        var expected = F002Text.ParseBacktickList(names);
        Assert.Equal(habitCount, expected.Count);
        Assert.Equal(expected, Browse().Habits.Select(habit => habit.Name).ToList());
        Assert.Equal(count, Browse().Count);
    }

    [Then(@"^browsing gives `([^`]+)` with no suggested starting goal$")]
    public void ThenBrowsingGivesWithNoSuggestedStartingGoal(string name)
    {
        var habit = Assert.Single(Browse().Habits, candidate => candidate.Name == name);
        Assert.Null(habit.MarkedSuggestedStartingGoal);
    }

    // ------------------------------------------------------------------ Then: category filter and Home preview

    [Then(@"^its first entry is ""([^""]*)""$")]
    public void ThenItsFirstEntryIs(string label)
    {
        Assert.Equal(label, CategoryFilterLabels()[0]);
    }

    [Then(@"^it contains ""([^""]*)"" and ""([^""]*)"", with ""([^""]*)"" before ""([^""]*)""$")]
    public void ThenItContainsWithBefore(string first, string second, string earlier, string later)
    {
        var labels = CategoryFilterLabels();
        Assert.Contains(first, labels);
        Assert.Contains(second, labels);
        Assert.True(labels.IndexOf(earlier) < labels.IndexOf(later), $"\"{earlier}\" is not before \"{later}\" in {string.Join(", ", labels)}.");
    }

    [Then(@"^it is exactly ((?:""[^""]*""(?:, )?)+), in this order$")]
    public void ThenTheCategoryFilterIsExactly(string labels)
    {
        Assert.Equal(F002Text.ParseQuotedList(labels), CategoryFilterLabels());
    }

    [Then(@"^it is exactly ((?:`[^`]+`(?:, )?)+), in this order$")]
    public void ThenTheHomePreviewIsExactly(string names)
    {
        var preview = context.LastHomePreview ?? throw new InvalidOperationException("No Home preview was asked for in this scenario.");
        Assert.Equal(F002Text.ParseBacktickList(names), preview.Habits.Select(habit => habit.Name).ToList());
    }

    [Then(@"^the category filter is exactly ((?:""[^""]*""(?:, )?)+), in this order$")]
    public async Task ThenTheCategoryFilterNowIsExactly(string labels)
    {
        var filter = await driver.GetCategoryFilterAsync();
        Assert.Equal(F002Text.ParseQuotedList(labels), filter.Options.Select(option => option.Label).ToList());
    }

    // ------------------------------------------------------------------ Then: details of the When step

    [Then(@"^the result has the name `([^`]+)`, (?!.*the state )(.+)$")]
    public void ThenTheDetailsHaveTheName(string name, string clause)
    {
        var details = Details();
        Assert.Equal(name, details.Name);
        F002Assert.HabitMatches(clause, details.Category, details.Intent, details.Icon, details.Description, details.MarkedSuggestedStartingGoal);
    }

    [Then(@"^it has three suggested starting goals in this order, with the fields given above, and the second is the marked one$")]
    public void ThenItHasThreeSuggestedStartingGoals()
    {
        var details = Details();
        var given = context.Habits.Single(habit => habit.Id == details.Id).Spec.Goals;
        Assert.Equal(3, given.Count);
        Assert.Equal(new[] { false, true, false }, given.Select(goal => goal.IsMarked).ToList());
        Assert.Equal(new[] { false, true, false }, details.SuggestedStartingGoals.Select(goal => goal.IsMarked).ToList());
        F002Assert.GoalsInOrder(given, details.SuggestedStartingGoals, details.MarkedSuggestedStartingGoal);
    }

    [Then(@"^their texts are (.+)$")]
    public void ThenTheirTextsAre(string texts)
    {
        Assert.Equal(F002Text.ParseQuotedList(texts), Details().SuggestedStartingGoals.Select(goal => goal.Text).ToList());
    }

    [Then(@"^their kinds are (.+)$")]
    public void ThenTheirKindsAre(string kinds)
    {
        Assert.Equal(F002Text.ParseQuotedList(kinds), Details().SuggestedStartingGoals.Select(goal => goal.Kind).ToList());
    }

    [Then(@"^the details give `([^`]+)` with no suggested starting goal$")]
    public void ThenTheDetailsGiveWithNoSuggestedStartingGoal(string name)
    {
        var details = Details();
        Assert.Equal(name, details.Name);
        Assert.Empty(details.SuggestedStartingGoals);
        Assert.Null(details.MarkedSuggestedStartingGoal);
    }

    [Then(@"^the suggested starting goal has the text ""([^""]*)"" and the kind ""([^""]*)""$")]
    public void ThenTheSuggestedStartingGoalHasTheTextAndTheKind(string text, string kind)
    {
        var goal = Assert.Single(Details().SuggestedStartingGoals);
        Assert.Equal(text, goal.Text);
        Assert.Equal(kind, goal.Kind);
    }

    // ------------------------------------------------------------------ Then: browsing now

    [Then(@"^browsing (contains|does not contain) `([^`]+)`$")]
    public async Task ThenBrowsingContains(string verb, string name)
    {
        AssertContains(await driver.BrowseAsync(new CatalogFilter()), verb, name);
    }

    [Then(@"^browsing with (the (?:category|intent|search) .+?) (contains|does not contain) `([^`]+)`$")]
    public async Task ThenBrowsingWithContains(string filter, string verb, string name)
    {
        AssertContains(await driver.BrowseAsync(F002Text.ParseFilter(filter)), verb, name);
    }

    [Then(@"^browsing with the search `([^`]+)` gives `([^`]+)` with (.+)$")]
    public async Task ThenBrowsingWithTheSearchGivesWith(string search, string name, string clause)
    {
        var result = await driver.BrowseAsync(new CatalogFilter { Search = search });
        var habit = Assert.Single(result.Habits, candidate => candidate.Name == name);
        F002Assert.HabitMatches(clause, habit.Category, habit.Intent, habit.Icon, habit.Description, habit.MarkedSuggestedStartingGoal);
    }

    [Then(@"^browsing with the search `([^`]+)` gives exactly one habit, `([^`]+)`, with the identifier of the catalog habit$")]
    public async Task ThenBrowsingWithTheSearchGivesExactlyOneHabit(string search, string name)
    {
        var result = await driver.BrowseAsync(new CatalogFilter { Search = search });
        var habit = Assert.Single(result.Habits);
        Assert.Equal(name, habit.Name);
        Assert.Equal(context.Habit(name, HabitKind.Catalog).Id, habit.Id);
    }

    [Then(@"^browsing with the search `([^`]+)` gives no habit$")]
    public async Task ThenBrowsingWithTheSearchGivesNoHabit(string search)
    {
        var result = await driver.BrowseAsync(new CatalogFilter { Search = search });
        Assert.Empty(result.Habits);
    }

    [Then(@"^browsing with the search `([^`]+)` still contains `([^`]+)` with (.+), and contains no `([^`]+)`$")]
    public async Task ThenBrowsingWithTheSearchStillContains(string search, string name, string clause, string absentName)
    {
        var result = await driver.BrowseAsync(new CatalogFilter { Search = search });
        var habit = Assert.Single(result.Habits, candidate => candidate.Name == name);
        F002Assert.HabitMatches(clause, habit.Category, habit.Intent, habit.Icon, habit.Description, habit.MarkedSuggestedStartingGoal);
        AssertContains(result, "does not contain", absentName);
    }

    [Then(@"^the anonymous visitor reading its details gets ""([^""]*)""$")]
    public async Task ThenTheAnonymousVisitorReadingItsDetailsGets(string text)
    {
        var details = await driver.GetDetailsAsync(context.CurrentHabitOrThrow.Id);
        Assert.Equal(text, details.Message);
        Assert.Equal(F002Text.OutcomeFor(typeof(CatalogHabitDetailsOutcome), text, aboutSuggestedStartingGoal: false), details.Outcome);
        Assert.Null(details.Details);
    }

    [Then(@"^browsing gives a count of (\d+)$")]
    public async Task ThenBrowsingGivesACountOf(int count)
    {
        var result = await driver.BrowseAsync(new CatalogFilter());
        Assert.Equal(count, result.Count);
    }

    [Then(@"^browsing gives a count of at least (\d+), and every habit it gives has a description$")]
    public async Task ThenBrowsingGivesACountOfAtLeastWithDescriptions(int minimum)
    {
        var result = await driver.BrowseAsync(new CatalogFilter());
        Assert.True(result.Count >= minimum, $"Browsing gives a count of {result.Count}, fewer than {minimum}.");
        Assert.All(result.Habits, habit => Assert.False(string.IsNullOrWhiteSpace(habit.Description), $"`{habit.Name}` has no description."));
    }

    [Then(@"^browsing with the intent (\w+) gives a count of at least (\d+)$")]
    public async Task ThenBrowsingWithTheIntentGivesACountOfAtLeast(string intent, int minimum)
    {
        var result = await driver.BrowseAsync(new CatalogFilter { Intent = F002Text.ParseIntent(intent) });
        Assert.True(result.Count >= minimum, $"Browsing with the intent {intent} gives a count of {result.Count}, fewer than {minimum}.");
    }

    [Then(@"^among the suggested starting goals in the details of the Published habits there is at least one yes/no, one count and one duration, at least one with fixed days and one with N times$")]
    public async Task ThenAmongTheSuggestedStartingGoalsThereIsEveryKind()
    {
        var goals = new List<SuggestedStartingGoalView>();
        foreach (var habit in (await driver.BrowseAsync(new CatalogFilter())).Habits)
        {
            goals.AddRange(await PublishedDetailsGoalsAsync(habit.Id));
        }

        Assert.Contains(goals, goal => goal.Measurement == Measurement.YesNo);
        Assert.Contains(goals, goal => goal.Measurement == Measurement.Count);
        Assert.Contains(goals, goal => goal.Measurement == Measurement.Duration);
        Assert.Contains(goals, goal => goal.ScheduleKind == ScheduleKind.FixedDays);
        Assert.Contains(goals, goal => goal.ScheduleKind == ScheduleKind.Times);
    }

    [Then(@"^at least one Published habit has no suggested starting goal, and at least one has three or more$")]
    public async Task ThenAtLeastOnePublishedHabitHasNoneAndOneHasThreeOrMore()
    {
        var counts = new List<int>();
        foreach (var habit in (await driver.BrowseAsync(new CatalogFilter())).Habits)
        {
            counts.Add((await PublishedDetailsGoalsAsync(habit.Id)).Count);
        }

        Assert.Contains(0, counts);
        Assert.Contains(counts, count => count >= 3);
    }

    // ------------------------------------------------------------------ helpers

    private CatalogBrowseResult Browse() =>
        context.LastBrowse ?? throw new InvalidOperationException("No browsing ran in a When step of this scenario.");

    private CatalogHabitDetails Details()
    {
        var result = context.LastDetails ?? throw new InvalidOperationException("No details were read in a When step of this scenario.");
        Assert.Equal(CatalogHabitDetailsOutcome.Found, result.Outcome);
        Assert.NotNull(result.Details);
        return result.Details;
    }

    private List<string> CategoryFilterLabels()
    {
        var filter = context.LastCategoryFilter ?? throw new InvalidOperationException("No category filter was asked for in this scenario.");
        return filter.Options.Select(option => option.Label).ToList();
    }

    private async Task<IReadOnlyList<SuggestedStartingGoalView>> PublishedDetailsGoalsAsync(HabitId id)
    {
        var details = await driver.GetDetailsAsync(id);
        Assert.Equal(CatalogHabitDetailsOutcome.Found, details.Outcome);
        Assert.NotNull(details.Details);
        return details.Details.SuggestedStartingGoals;
    }

    private static void AssertContains(CatalogBrowseResult result, string verb, string name)
    {
        var names = result.Habits.Select(habit => habit.Name).ToList();
        switch (verb)
        {
            case "contains":
                Assert.Contains(name, names);
                break;
            case "does not contain":
                Assert.DoesNotContain(name, names);
                break;
            default:
                throw new InvalidOperationException($"\"{verb}\" is neither contains nor does not contain.");
        }
    }
}
