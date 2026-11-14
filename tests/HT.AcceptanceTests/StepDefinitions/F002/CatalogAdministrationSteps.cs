using System.Text.RegularExpressions;
using HT.AcceptanceTests.Support.F002;
using HT.Application.Habits;
using HT.Domain.Accounts;
using HT.Domain.Habits;

namespace HT.AcceptanceTests.StepDefinitions.F002;

/// <summary>
/// Catalog administration (FR-011 to FR-014): list, read, create, publish and unpublish, and the Then steps
/// about the administrator's list. "the administrator's list" is the list of the item's administrator with no
/// search, run at that point. Edits are in <see cref="EditSteps"/>.
/// </summary>
[Binding]
[Scope(Tag = "F002")]
public sealed class CatalogAdministrationSteps(F002Context context, F002Driver driver)
{
    private const string Actor = F002Text.Actor;

    // ------------------------------------------------------------------ When

    [When("^" + Actor + " lists the catalog habits as an administrator$")]
    public async Task WhenListsTheCatalogHabitsAsAnAdministrator(string actor)
    {
        var (user, _) = context.ResolveActor(actor);
        context.Record(await driver.ListCatalogAsync(user, null));
    }

    [When("^" + Actor + " lists the catalog habits with the search (`[^`]*`)$")]
    public async Task WhenListsTheCatalogHabitsWithTheSearch(string actor, string search)
    {
        var (user, _) = context.ResolveActor(actor);
        context.Record(await driver.ListCatalogAsync(user, F002Text.ParseTextExpression(search).Full));
    }

    [When("^" + Actor + " reads `([^`]+)`(?: as an administrator)?$")]
    public async Task WhenReadsAsAnAdministrator(string actor, string name)
    {
        var (user, _) = context.ResolveActor(actor);
        var record = context.Habit(name, HabitKind.Catalog);
        context.CurrentHabit = record;
        context.Record(await driver.GetCatalogHabitAsync(user, record.Id));
    }

    /// <summary>
    /// "creates the catalog habit `N`", optionally followed by " with ..." and optionally by " and publishes it".
    /// Both groups always take part in the match (an empty capture means the part is absent), so the binding has
    /// the same number of arguments for every form.
    /// </summary>
    [When("^" + Actor + " creates the catalog habit `([^`]+)`((?: with .+?)?)( and publishes it|)$")]
    public async Task WhenCreatesTheCatalogHabit(string actor, string name, string clause, string andPublish)
    {
        var (user, _) = context.ResolveActor(actor);
        var spec = new HabitSpec();
        spec.SetName(TextValue.Of(name));
        if (!string.IsNullOrEmpty(clause))
        {
            F002Text.ApplyFields(spec, clause);
        }

        var record = await CreateAsync(user, spec);
        if (!string.IsNullOrEmpty(andPublish))
        {
            var id = record?.Id ?? throw new InvalidOperationException($"`{name}` was not created, so it cannot be published.");
            context.Record(await driver.PublishAsync(user, id));
        }
    }

    [When("^" + Actor + " creates a catalog habit with (.+)$")]
    public async Task WhenCreatesACatalogHabitWith(string actor, string fields)
    {
        var (user, _) = context.ResolveActor(actor);
        var spec = new HabitSpec();
        F002Text.ApplyFields(spec, fields);
        await CreateAsync(user, spec);
    }

    [When("^" + Actor + " creates a catalog habit whose name is (.+), with a description that is (.+)$")]
    public async Task WhenCreatesACatalogHabitWhoseNameIs(string actor, string name, string description)
    {
        var (user, _) = context.ResolveActor(actor);
        var spec = new HabitSpec();
        spec.SetName(F002Text.ParseTextExpression(name));
        spec.SetDescription(F002Text.ParseTextExpression(description));
        await CreateAsync(user, spec);
    }

    [When("^" + Actor + " publishes `([^`]+)`$")]
    public async Task WhenPublishes(string actor, string name)
    {
        var (user, _) = context.ResolveActor(actor);
        var record = context.Habit(name, HabitKind.Catalog);
        context.CurrentHabit = record;
        context.Record(await driver.PublishAsync(user, record.Id));
    }

    [When("^" + Actor + " unpublishes `([^`]+)`(?:, as one call with nothing before or after it)?$")]
    public async Task WhenUnpublishes(string actor, string name)
    {
        var (user, _) = context.ResolveActor(actor);
        var record = context.Habit(name, HabitKind.Catalog);
        context.CurrentHabit = record;
        context.Record(await driver.UnpublishAsync(user, record.Id));
    }

    [When("^" + Actor + " (reads|publishes|unpublishes) an unknown identifier$")]
    public async Task WhenActsOnAnUnknownIdentifier(string actor, string verb)
    {
        var (user, _) = context.ResolveActor(actor);
        await ActAsync(user, verb, new HabitId(Guid.NewGuid()));
    }

    [When("^" + Actor + " (reads|publishes|unpublishes) with the identifier of `([^`]+)`$")]
    public async Task WhenActsWithTheIdentifierOf(string actor, string verb, string name)
    {
        var (user, _) = context.ResolveActor(actor);
        await ActAsync(user, verb, context.Habit(name).Id);
    }

    // ------------------------------------------------------------------ Then: the administrator's list now

    [Then(@"^the administrator's list contains `([^`]+)` with the state (\w+)$")]
    public async Task ThenTheAdministratorsListContainsWithTheState(string name, string state)
    {
        var habit = Assert.Single((await AdministratorsListAsync()).Habits, item => item.Name == name);
        Assert.Equal(F002Text.ParseState(state), habit.State);
    }

    [Then(@"^the administrator's list gives `([^`]+)` the state (\w+)$")]
    public async Task ThenTheAdministratorsListGivesTheState(string name, string state)
    {
        var habit = Assert.Single((await AdministratorsListAsync()).Habits, item => item.Name == name);
        Assert.Equal(F002Text.ParseState(state), habit.State);
    }

    [Then(@"^the administrator's list gives it the state (\w+)$")]
    public async Task ThenTheAdministratorsListGivesItTheState(string state)
    {
        var record = context.CurrentHabitOrThrow;
        var habit = Assert.Single((await AdministratorsListAsync()).Habits, item => item.Id == record.Id);
        Assert.Equal(F002Text.ParseState(state), habit.State);
    }

    [Then(@"^the administrator's list gives `([^`]+)` the state (\w+) and `([^`]+)` the state (\w+)$")]
    public async Task ThenTheAdministratorsListGivesBothTheirStates(string firstName, string firstState, string secondName, string secondState)
    {
        var list = await AdministratorsListAsync();
        Assert.Equal(F002Text.ParseState(firstState), Assert.Single(list.Habits, item => item.Name == firstName).State);
        Assert.Equal(F002Text.ParseState(secondState), Assert.Single(list.Habits, item => item.Name == secondName).State);
    }

    [Then(@"^the administrator's list has exactly one habit named `([^`]+)` and still has `([^`]+)`$")]
    public async Task ThenTheAdministratorsListHasExactlyOneHabitNamed(string name, string otherName)
    {
        var list = await AdministratorsListAsync();
        // FR-004: a name is the same name as its trimmed form, compared ordinally ignoring case.
        var sameName = list.Habits.Where(item => string.Equals(item.Name.Trim(), name, StringComparison.OrdinalIgnoreCase)).ToList();
        var habit = Assert.Single(sameName);
        Assert.Equal(name, habit.Name);
        Assert.Contains(otherName, list.Habits.Select(item => item.Name));
    }

    [Then(@"^the administrator's list contains no habit whose name starts with `([^`]+)`$")]
    public async Task ThenTheAdministratorsListContainsNoHabitWhoseNameStartsWith(string prefix)
    {
        var list = await AdministratorsListAsync();
        Assert.DoesNotContain(list.Habits, item => item.Name.StartsWith(prefix, StringComparison.Ordinal));
    }

    [Then(@"^the administrator's list contains that name, and reading it gives that 500-character description$")]
    public async Task ThenTheAdministratorsListContainsThatName()
    {
        var record = context.CurrentHabitOrThrow;
        Assert.Contains(record.Spec.NameCore, (await AdministratorsListAsync()).Habits.Select(item => item.Name));
        var habit = await ReadAsAdministratorAsync(record.Id);
        Assert.Equal(record.Spec.DescriptionCore, habit.Description);
        Assert.Equal(500, habit.Description?.Length);
    }

    [Then(@"^the administrator's list still gives `([^`]+)` the state (\w+) and contains no `([^`]+)` and no `([^`]+)`$")]
    public async Task ThenTheAdministratorsListStillGivesTheStateAndContainsNo(string name, string state, string firstAbsent, string secondAbsent)
    {
        var list = await AdministratorsListAsync();
        Assert.Equal(F002Text.ParseState(state), Assert.Single(list.Habits, item => item.Name == name).State);
        Assert.DoesNotContain(firstAbsent, list.Habits.Select(item => item.Name));
        Assert.DoesNotContain(secondAbsent, list.Habits.Select(item => item.Name));
    }

    [Then(@"^the administrator's list contains no habit with the identifier of `([^`]+)`$")]
    public async Task ThenTheAdministratorsListContainsNoHabitWithTheIdentifierOf(string name)
    {
        var id = context.Habit(name).Id;
        Assert.DoesNotContain((await AdministratorsListAsync()).Habits, item => item.Id == id);
    }

    [Then(@"^the administrator's list has exactly one habit with the state Draft and exactly one with the state Unpublished, and every other habit has the state Published$")]
    public async Task ThenTheAdministratorsListHasOneDraftAndOneUnpublished()
    {
        var states = (await AdministratorsListAsync()).Habits.Select(item => item.State).ToList();
        Assert.Single(states, state => state == CatalogHabitState.Draft);
        Assert.Single(states, state => state == CatalogHabitState.Unpublished);
        Assert.Equal(states.Count - 2, states.Count(state => state == CatalogHabitState.Published));
    }

    [Then(@"^no two habits of the administrator's list have the same icon$")]
    public async Task ThenNoTwoHabitsOfTheAdministratorsListHaveTheSameIcon()
    {
        var icons = new List<string>();
        foreach (var item in (await AdministratorsListAsync()).Habits)
        {
            icons.Add((await ReadAsAdministratorAsync(item.Id)).Icon);
        }

        Assert.NotEmpty(icons);
        var repeated = icons.GroupBy(icon => icon, StringComparer.Ordinal).Where(group => group.Count() > 1).Select(group => group.Key);
        Assert.Empty(repeated);
    }

    [Then(@"^the administrator's list has exactly the same names with the same states, and the same total$")]
    public async Task ThenTheAdministratorsListIsUnchanged()
    {
        var before = context.RememberedCatalogList ?? throw new InvalidOperationException("No administrator's list was remembered in this scenario.");
        var now = await AdministratorsListAsync();
        Assert.Equal(before.Habits.Select(item => (item.Name, item.State)).ToList(), now.Habits.Select(item => (item.Name, item.State)).ToList());
        Assert.Equal(before.Total, now.Total);
    }

    [Then(@"^the administrator's list is exactly `([^`]+)`, with the total (\d+)$")]
    public async Task ThenTheAdministratorsListIsExactly(string name, int total)
    {
        var list = await AdministratorsListAsync();
        Assert.Equal(new[] { name }, list.Habits.Select(item => item.Name).ToList());
        Assert.Equal(total, list.Total);
    }

    [Then(@"^the administrator's list with no search contains (.+), in this order$")]
    public async Task ThenTheAdministratorsListWithNoSearchContainsInThisOrder(string names)
    {
        var listed = (await AdministratorsListAsync()).Habits.Select(item => item.Name).ToList();
        var expected = F002Text.ParseBacktickList(names);
        var positions = expected.Select(name => listed.IndexOf(name)).ToList();
        Assert.DoesNotContain(-1, positions);
        Assert.Equal(positions.Order(), positions);
    }

    // ------------------------------------------------------------------ Then: the list of the When step

    [Then(@"^the list is exactly `([^`]+)` with the state (\w+) and `([^`]+)` with the state (\w+), in this order$")]
    public void ThenTheListIsExactlyWithTheStates(string firstName, string firstState, string secondName, string secondState)
    {
        var list = SearchedList();
        Assert.Equal(new[] { firstName, secondName }, list.Habits.Select(item => item.Name).ToList());
        Assert.Equal(new[] { F002Text.ParseState(firstState), F002Text.ParseState(secondState) }, list.Habits.Select(item => item.State).ToList());
    }

    [Then(@"^the result gives the total number of catalog habits, which is the same as for the administrator's list with no search$")]
    public async Task ThenTheResultGivesTheTotalNumberOfCatalogHabits()
    {
        var all = await AdministratorsListAsync();
        Assert.Equal(all.Habits.Count, all.Total);
        Assert.Equal(all.Total, SearchedList().Total);
    }

    // ------------------------------------------------------------------ Then: reading as the administrator

    [Then(@"^the result has the name `([^`]+)`, (.+), the state (\w+) and that suggested starting goal, marked$")]
    public void ThenTheAdministratorsReadHasTheName(string name, string clause, string state)
    {
        var read = context.LastCatalogRead ?? throw new InvalidOperationException("No catalog habit was read as the administrator in this scenario.");
        Assert.Equal(CatalogHabitAdminOutcome.Found, read.Outcome);
        var habit = read.Habit;
        Assert.NotNull(habit);
        Assert.Equal(name, habit.Name);
        F002Assert.HabitMatches(clause, habit.Category, habit.Intent, habit.Icon, habit.Description, habit.MarkedSuggestedStartingGoal);
        Assert.Equal(F002Text.ParseState(state), habit.State);
        var given = Assert.Single(context.Habit(name, HabitKind.Catalog).Spec.Goals);
        var goal = Assert.Single(habit.SuggestedStartingGoals);
        F002Assert.GoalFields(given, goal);
        Assert.True(goal.IsMarked);
        Assert.NotNull(habit.MarkedSuggestedStartingGoal);
        F002Assert.GoalFields(given, habit.MarkedSuggestedStartingGoal);
    }

    [Then(@"^reading it as the administrator gives both suggested starting goals in this order, with the second marked$")]
    public async Task ThenReadingItGivesBothSuggestedStartingGoals()
    {
        var sent = LastAttemptedGoals();
        Assert.Equal(new[] { false, true }, sent.Select(goal => goal.IsMarked).ToList());
        var habit = await ReadAsAdministratorAsync(context.CurrentHabitOrThrow.Id);
        F002Assert.GoalsInOrder(sent, habit.SuggestedStartingGoals, habit.MarkedSuggestedStartingGoal);
    }

    [Then(@"^reading `([^`]+)` as the administrator gives (.+)$")]
    public async Task ThenReadingAsTheAdministratorGives(string name, string expectation)
    {
        var habit = await ReadAsAdministratorAsync(context.Habit(name, HabitKind.Catalog).Id);
        Match match;
        if (expectation == "no suggested starting goal")
        {
            Assert.Empty(habit.SuggestedStartingGoals);
            Assert.Null(habit.MarkedSuggestedStartingGoal);
        }
        else if ((match = Regex.Match(expectation, @"^(\d+) suggested starting goals, the (\w+) marked$")).Success)
        {
            var count = int.Parse(match.Groups[1].Value);
            var marked = F002Text.Ordinal(match.Groups[2].Value);
            Assert.Equal(count, habit.SuggestedStartingGoals.Count);
            Assert.Equal(
                Enumerable.Range(1, count).Select(position => position == marked).ToList(),
                habit.SuggestedStartingGoals.Select(goal => goal.IsMarked).ToList());
            Assert.NotNull(habit.MarkedSuggestedStartingGoal);
            Assert.True(habit.MarkedSuggestedStartingGoal.IsMarked);
        }
        else if ((match = Regex.Match(expectation, "^one suggested starting goal with Build or Reduce, the measurement, the amount and the schedule given above, and the unit (.+)$")).Success)
        {
            var sent = Assert.Single(LastAttemptedGoals());
            var goal = Assert.Single(habit.SuggestedStartingGoals);
            F002Assert.GoalFields(sent, goal, includingUnit: false);
            AssertStoredUnit(match.Groups[1].Value, goal);
        }
        else
        {
            throw new InvalidOperationException($"\"{expectation}\" is not a reading the F002 steps know.");
        }
    }

    [Then(@"^reading them as the administrator gives the categories (\w+) and (\w+)$")]
    public async Task ThenReadingThemGivesTheCategories(string firstCategory, string secondCategory)
    {
        var edited = context.Habits.Where(habit => habit.Kind == HabitKind.Catalog).ToList();
        Assert.Equal(2, edited.Count);
        Assert.Equal(F002Text.ParseCategory(firstCategory), (await ReadAsAdministratorAsync(edited[0].Id)).Category);
        Assert.Equal(F002Text.ParseCategory(secondCategory), (await ReadAsAdministratorAsync(edited[1].Id)).Category);
    }

    [Then(@"^reading it as the administrator gives the name (`[^`]+`|the 60-character name) and (no description|the description ""[^""]*""|the 500-character description)$")]
    public async Task ThenReadingItAsTheAdministratorGivesTheName(string name, string description)
    {
        var record = context.CurrentHabitOrThrow;
        var habit = await ReadAsAdministratorAsync(record.Id);
        var (expectedName, expectedDescription) = StoredNameAndDescription(record, name, description);
        Assert.Equal(expectedName, habit.Name);
        Assert.Equal(expectedDescription, habit.Description);
    }

    // ------------------------------------------------------------------ helpers

    /// <summary>
    /// The stored name and description a step names: a name in backticks, "the 60-character name" (the name the
    /// step built, without the spaces around it), "no description", the description "..." or "the 500-character
    /// description" (the description the step built, without the spaces around it).
    /// </summary>
    internal static (string Name, string? Description) StoredNameAndDescription(HabitRecord record, string name, string description)
    {
        var expectedName = name == "the 60-character name"
            ? record.Spec.NameCore ?? throw new InvalidOperationException("The step built no name.")
            : F002Text.ParseTextExpression(name).Full!;
        if (name == "the 60-character name")
        {
            Assert.Equal(60, expectedName.Length);
        }

        string? expectedDescription = description switch
        {
            "no description" => null,
            "the 500-character description" => record.Spec.DescriptionCore,
            _ => F002Text.ParseQuotedList(description).Single(),
        };
        if (description == "the 500-character description")
        {
            Assert.Equal(500, expectedDescription?.Length);
        }

        return (expectedName, expectedDescription);
    }

    private async Task<HabitRecord?> CreateAsync(ActingUser user, HabitSpec spec)
    {
        var result = await driver.CreateCatalogHabitAsync(user, spec.ToCatalogInput());
        context.Record(result);
        if (result.Outcome != CatalogHabitCreateOutcome.Created || result.HabitId is not { } id)
        {
            return null;
        }

        var record = new HabitRecord(HabitKind.Catalog, id, null, spec);
        context.Habits.Add(record);
        context.CurrentHabit = record;
        return record;
    }

    private async Task ActAsync(ActingUser user, string verb, HabitId id)
    {
        switch (verb)
        {
            case "reads":
                context.Record(await driver.GetCatalogHabitAsync(user, id));
                break;
            case "publishes":
                context.Record(await driver.PublishAsync(user, id));
                break;
            case "unpublishes":
                context.Record(await driver.UnpublishAsync(user, id));
                break;
            default:
                throw new InvalidOperationException($"\"{verb}\" is not a catalog action the F002 steps know.");
        }
    }

    private async Task<CatalogHabitListResult> AdministratorsListAsync()
    {
        var list = await driver.ListCatalogAsync(context.Administrator, null);
        Assert.Equal(CatalogHabitListOutcome.Found, list.Outcome);
        return list;
    }

    private async Task<CatalogHabitAdminDetails> ReadAsAdministratorAsync(HabitId id)
    {
        var read = await driver.GetCatalogHabitAsync(context.Administrator, id);
        Assert.Equal(CatalogHabitAdminOutcome.Found, read.Outcome);
        Assert.NotNull(read.Habit);
        return read.Habit;
    }

    private CatalogHabitListResult SearchedList()
    {
        var list = context.LastCatalogList ?? throw new InvalidOperationException("No catalog list ran in a When step of this scenario.");
        Assert.Equal(CatalogHabitListOutcome.Found, list.Outcome);
        return list;
    }

    private IReadOnlyList<GoalSpec> LastAttemptedGoals() =>
        context.LastAttemptedGoals ?? throw new InvalidOperationException("No suggested starting goals were sent in this scenario.");

    /// <summary>
    /// The stored unit a step names: a count unit in backticks, minutes or hours, or "that unit of 20 characters"
    /// (<c>u</c> repeated 20 times, acceptance-criteria conventions), with or without the spaces around it.
    /// </summary>
    private static void AssertStoredUnit(string unit, SuggestedStartingGoalView goal)
    {
        switch (unit)
        {
            case "minutes":
                Assert.Equal(DurationUnit.Minutes, goal.DurationUnit);
                break;
            case "hours":
                Assert.Equal(DurationUnit.Hours, goal.DurationUnit);
                break;
            case "that unit of 20 characters":
            case "that unit of 20 characters, without the spaces":
                Assert.Equal(new string('u', 20), goal.CountUnit);
                break;
            default:
                Assert.Equal(F002Text.ParseTextExpression(unit).Full, goal.CountUnit);
                break;
        }
    }
}
