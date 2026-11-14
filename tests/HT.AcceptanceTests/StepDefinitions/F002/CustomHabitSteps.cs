using HT.AcceptanceTests.Support.F002;
using HT.Application.Habits;
using HT.Domain.Accounts;
using HT.Domain.Habits;

namespace HT.AcceptanceTests.StepDefinitions.F002;

/// <summary>
/// Custom habits (FR-020 to FR-025): create, list, read and delete, and the Then steps about a user's custom
/// habits. "their list" is the custom-habit list of the item's registered user, run at that point.
/// Edits are in <see cref="EditSteps"/>.
/// </summary>
[Binding]
[Scope(Tag = "F002")]
public sealed class CustomHabitSteps(F002Context context, F002Driver driver)
{
    private const string Actor = F002Text.Actor;

    // ------------------------------------------------------------------ When

    /// <summary>The " with ..." clause always takes part in the match; an empty capture means the step gives no fields.</summary>
    [When("^" + Actor + " creates the custom habit `([^`]+)`((?: with .+)?)$")]
    public async Task WhenCreatesTheCustomHabit(string actor, string name, string clause)
    {
        var (user, email) = context.ResolveActor(actor);
        var spec = new HabitSpec();
        spec.SetName(TextValue.Of(name));
        if (!string.IsNullOrEmpty(clause))
        {
            F002Text.ApplyFields(spec, clause);
        }

        await CreateAsync(user, email, spec);
    }

    [When("^" + Actor + " creates the custom habit `([^`]+)` and the custom habit `([^`]+)`$")]
    public async Task WhenCreatesTwoCustomHabits(string actor, string firstName, string secondName)
    {
        var (user, email) = context.ResolveActor(actor);
        foreach (var name in new[] { firstName, secondName })
        {
            var spec = new HabitSpec();
            spec.SetName(TextValue.Of(name));
            await CreateAsync(user, email, spec);
        }
    }

    [When("^" + Actor + " creates a custom habit with (.+)$")]
    public async Task WhenCreatesACustomHabitWith(string actor, string fields)
    {
        var (user, email) = context.ResolveActor(actor);
        var spec = new HabitSpec();
        F002Text.ApplyFields(spec, fields);
        await CreateAsync(user, email, spec);
    }

    [When("^" + Actor + " creates a custom habit whose name is (.+), with a description that is (.+)$")]
    public async Task WhenCreatesACustomHabitWhoseNameIs(string actor, string name, string description)
    {
        var (user, email) = context.ResolveActor(actor);
        var spec = new HabitSpec();
        spec.SetName(F002Text.ParseTextExpression(name));
        spec.SetDescription(F002Text.ParseTextExpression(description));
        await CreateAsync(user, email, spec);
    }

    [When("^" + Actor + " lists (?:their )?custom habits$")]
    public async Task WhenListsCustomHabits(string actor)
    {
        var (user, _) = context.ResolveActor(actor);
        context.Record(await driver.ListCustomHabitsAsync(user));
    }

    [When("^" + Actor + " (reads|deletes) `([^`]+)` by its identifier$")]
    public async Task WhenReadsOrDeletesByItsIdentifier(string actor, string verb, string name)
    {
        var (user, _) = context.ResolveActor(actor);
        var record = context.Habit(name, HabitKind.Custom);
        context.CurrentHabit = record;
        await ReadOrDeleteAsync(user, verb, record.Id);
    }

    [When("^" + Actor + " deletes `([^`]+)`$")]
    public async Task WhenDeletes(string actor, string name)
    {
        var (user, _) = context.ResolveActor(actor);
        var record = context.Habit(name, HabitKind.Custom);
        context.CurrentHabit = record;
        await ReadOrDeleteAsync(user, "deletes", record.Id);
    }

    [When("^" + Actor + " edits `([^`]+)` by its identifier, with (.+)$")]
    public async Task WhenEditsByItsIdentifier(string actor, string name, string fields)
    {
        var (user, _) = context.ResolveActor(actor);
        var record = context.Habit(name, HabitKind.Custom);
        await EditAsync(user, record.Id, record.Spec.Clone(), fields);
    }

    [When("^" + Actor + " (reads|deletes) the custom habit `([^`]+)` of `([^`]+)` by its identifier$")]
    public async Task WhenReadsOrDeletesTheCustomHabitOf(string actor, string verb, string name, string ownerEmail)
    {
        var (user, _) = context.ResolveActor(actor);
        await ReadOrDeleteAsync(user, verb, context.Habit(name, HabitKind.Custom, ownerEmail).Id);
    }

    [When("^" + Actor + " edits, with (.+), the custom habit `([^`]+)` of `([^`]+)` by its identifier$")]
    public async Task WhenEditsTheCustomHabitOf(string actor, string fields, string name, string ownerEmail)
    {
        var (user, _) = context.ResolveActor(actor);
        var record = context.Habit(name, HabitKind.Custom, ownerEmail);
        await EditAsync(user, record.Id, record.Spec.Clone(), fields);
    }

    [When("^" + Actor + " (reads|deletes) a custom habit with the identifier of `([^`]+)`$")]
    public async Task WhenReadsOrDeletesACustomHabitWithTheIdentifierOf(string actor, string verb, string name)
    {
        var (user, _) = context.ResolveActor(actor);
        await ReadOrDeleteAsync(user, verb, context.Habit(name).Id);
    }

    [When("^" + Actor + " edits a custom habit with the identifier of `([^`]+)`, with (.+)$")]
    public async Task WhenEditsACustomHabitWithTheIdentifierOf(string actor, string name, string fields)
    {
        var (user, _) = context.ResolveActor(actor);
        await EditAsync(user, context.Habit(name).Id, new HabitSpec(), fields);
    }

    // ------------------------------------------------------------------ Then

    [Then(@"^their list of custom habits contains `([^`]+)` with the category (\w+), the intent (\w+), the icon `([^`]+)` and the description ""([^""]*)""$")]
    public async Task ThenTheirListContains(string name, string category, string intent, string icon, string description)
    {
        var habit = Assert.Single(await ListAsync(context.RegisteredUser), candidate => candidate.Name == name);
        Assert.Equal(F002Text.ParseCategory(category), habit.Category);
        Assert.Equal(F002Text.ParseIntent(intent), habit.Intent);
        Assert.Equal(icon, habit.Icon);
        Assert.Equal(description, habit.Description);
    }

    [Then(@"^reading it gives the same values$")]
    public async Task ThenReadingItGivesTheSameValues()
    {
        var record = context.CurrentHabitOrThrow;
        var habit = await ReadAsync(context.RegisteredUser, record.Id);
        Assert.Equal(record.Spec.NameCore, habit.Name);
        Assert.Equal(record.Spec.Category, habit.Category);
        Assert.Equal(record.Spec.Intent, habit.Intent);
        Assert.Equal(record.Spec.Icon, habit.Icon);
        Assert.Equal(record.Spec.DescriptionCore, habit.Description);
    }

    [Then(@"^reading it gives the name `([^`]+)`, the category (\w+), the intent (\w+), the icon `([^`]+)` and the description ""([^""]*)""$")]
    public async Task ThenReadingItGivesEveryField(string name, string category, string intent, string icon, string description)
    {
        var habit = await ReadAsync(context.RegisteredUser, context.CurrentHabitOrThrow.Id);
        Assert.Equal(name, habit.Name);
        Assert.Equal(F002Text.ParseCategory(category), habit.Category);
        Assert.Equal(F002Text.ParseIntent(intent), habit.Intent);
        Assert.Equal(icon, habit.Icon);
        Assert.Equal(description, habit.Description);
    }

    [Then(@"^reading it gives the name (`[^`]+`|the 60-character name) and (no description|the description ""[^""]*""|the 500-character description)$")]
    public async Task ThenReadingItGivesTheNameAndDescription(string name, string description)
    {
        var record = context.CurrentHabitOrThrow;
        var habit = await ReadAsync(context.RegisteredUser, record.Id);
        var (expectedName, expectedDescription) = CatalogAdministrationSteps.StoredNameAndDescription(record, name, description);
        Assert.Equal(expectedName, habit.Name);
        Assert.Equal(expectedDescription, habit.Description);
    }

    [Then(@"^reading it gives that name and that 500-character description$")]
    public async Task ThenReadingItGivesThatNameAndThatDescription()
    {
        var record = context.CurrentHabitOrThrow;
        var habit = await ReadAsync(context.RegisteredUser, record.Id);
        Assert.Equal(record.Spec.NameCore, habit.Name);
        Assert.Equal(60, habit.Name.Length);
        Assert.Equal(record.Spec.DescriptionCore, habit.Description);
        Assert.Equal(500, habit.Description?.Length);
    }

    [Then(@"^their list is (?:still )?exactly ((?:`[^`]+`(?:, )?)+)$")]
    public async Task ThenTheirListIsExactly(string names)
    {
        Assert.Equal(F002Text.ParseBacktickList(names), (await ListAsync(context.RegisteredUser)).Select(habit => habit.Name).ToList());
    }

    [Then(@"^their list of custom habits is empty$")]
    public async Task ThenTheirListOfCustomHabitsIsEmpty()
    {
        Assert.Empty(await ListAsync(context.RegisteredUser));
    }

    [Then(@"^reading `([^`]+)` by its identifier gives ""([^""]*)""$")]
    public async Task ThenReadingByItsIdentifierGives(string name, string text)
    {
        var result = await driver.GetCustomHabitAsync(context.RegisteredUser, context.Habit(name, HabitKind.Custom).Id);
        Assert.Equal(text, result.Message);
        Assert.Equal(F002Text.OutcomeFor(typeof(CustomHabitOutcome), text, aboutSuggestedStartingGoal: false), result.Outcome);
        Assert.Null(result.Habit);
    }

    [Then(@"^the list is exactly ((?:`[^`]+`(?:, )?)+), in this order$")]
    public void ThenTheListIsExactly(string names)
    {
        var last = context.LastResult;
        var list = Assert.IsType<CustomHabitListResult>(last.Raw);
        Assert.Equal(CustomHabitListOutcome.Found, list.Outcome);
        Assert.Equal(F002Text.ParseBacktickList(names), list.Habits.Select(habit => habit.Name).ToList());
    }

    [Then(@"^the list of `([^`]+)` is exactly ((?:`[^`]+`(?:, )?)+)$")]
    public async Task ThenTheListOfIsExactly(string email, string names)
    {
        Assert.Equal(F002Text.ParseBacktickList(names), (await ListAsync(context.Account(email))).Select(habit => habit.Name).ToList());
    }

    [Then(@"^the list of `([^`]+)` does not contain `([^`]+)`$")]
    public async Task ThenTheListOfDoesNotContain(string email, string name)
    {
        var habits = await ListAsync(context.Account(email));
        Assert.DoesNotContain(name, habits.Select(habit => habit.Name));
    }

    [Then(@"^`([^`]+)` still reads `([^`]+)` with the category (\w+)$")]
    public async Task ThenStillReadsWithTheCategory(string email, string name, string category)
    {
        var record = context.Habit(name, HabitKind.Custom, email);
        var habit = await ReadAsync(context.Account(email), record.Id);
        Assert.Equal(name, habit.Name);
        Assert.Equal(F002Text.ParseCategory(category), habit.Category);
    }

    [Then(@"^`([^`]+)` still reads `([^`]+)` with the category (\w+) and the intent (\w+)$")]
    public async Task ThenStillReadsWithTheCategoryAndTheIntent(string email, string name, string category, string intent)
    {
        var record = context.Habit(name, HabitKind.Custom, email);
        var habit = await ReadAsync(context.Account(email), record.Id);
        Assert.Equal(name, habit.Name);
        Assert.Equal(F002Text.ParseCategory(category), habit.Category);
        Assert.Equal(F002Text.ParseIntent(intent), habit.Intent);
    }

    [Then(@"^the registered user `([^`]+)` still reads `([^`]+)` with the category (\w+), the intent (\w+) and the icon `([^`]+)`$")]
    public async Task ThenTheRegisteredUserStillReads(string email, string name, string category, string intent, string icon)
    {
        var record = context.Habit(name, HabitKind.Custom, email);
        var habit = await ReadAsync(context.Account(email), record.Id);
        Assert.Equal(name, habit.Name);
        Assert.Equal(F002Text.ParseCategory(category), habit.Category);
        Assert.Equal(F002Text.ParseIntent(intent), habit.Intent);
        Assert.Equal(icon, habit.Icon);
    }

    [Then(@"^`([^`]+)` still lists exactly `([^`]+)`$")]
    public async Task ThenStillListsExactly(string email, string name)
    {
        Assert.Equal(new[] { name }, (await ListAsync(context.Account(email))).Select(habit => habit.Name).ToList());
    }

    // ------------------------------------------------------------------ helpers

    private async Task CreateAsync(ActingUser user, string? email, HabitSpec spec)
    {
        var result = await driver.CreateCustomHabitAsync(user, spec.ToCustomInput());
        context.Record(result);
        if (result.Outcome == CustomHabitCreateOutcome.Created && result.HabitId is { } id)
        {
            var record = new HabitRecord(HabitKind.Custom, id, email, spec);
            context.Habits.Add(record);
            context.CurrentHabit = record;
        }
    }

    private async Task ReadOrDeleteAsync(ActingUser user, string verb, HabitId id)
    {
        switch (verb)
        {
            case "reads":
                context.Record(await driver.GetCustomHabitAsync(user, id));
                break;
            case "deletes":
                context.Record(await driver.DeleteCustomHabitAsync(user, id));
                break;
            default:
                throw new InvalidOperationException($"\"{verb}\" is not a custom-habit action the F002 steps know.");
        }
    }

    private async Task EditAsync(ActingUser user, HabitId id, HabitSpec spec, string fields)
    {
        F002Text.ApplyFields(spec, fields);
        context.Record(await driver.EditCustomHabitAsync(user, id, spec.ToCustomInput()));
    }

    private async Task<IReadOnlyList<CustomHabitView>> ListAsync(ActingUser user)
    {
        var list = await driver.ListCustomHabitsAsync(user);
        Assert.Equal(CustomHabitListOutcome.Found, list.Outcome);
        return list.Habits;
    }

    private async Task<CustomHabitView> ReadAsync(ActingUser user, HabitId id)
    {
        var read = await driver.GetCustomHabitAsync(user, id);
        Assert.Equal(CustomHabitOutcome.Found, read.Outcome);
        Assert.NotNull(read.Habit);
        return read.Habit;
    }
}
