using HT.AcceptanceTests.Support.F002;
using HT.Application.Habits;
using HT.Domain.Accounts;
using HT.Domain.Habits;

namespace HT.AcceptanceTests.StepDefinitions.F002;

/// <summary>
/// Edit steps. A step that names a habit edits it with the use case of its kind: a catalog habit with the
/// catalog administration edit (FR-012), a custom habit with the custom-habit edit (FR-020). An edit keeps every
/// field the step does not change ("keeping its other fields").
/// </summary>
[Binding]
[Scope(Tag = "F002")]
public sealed class EditSteps(F002Context context, F002Driver driver, ScenarioContext scenario)
{
    private const string Actor = F002Text.Actor;

    [When("^" + Actor + " edits `([^`]+)` with the name (.+)$")]
    public async Task WhenEditsWithTheName(string actor, string name, string newName)
    {
        await EditNameAsync(actor, context.Habit(name), newName);
    }

    [When("^" + Actor + " edits `([^`]+)` of `([^`]+)` with the name (.+)$")]
    public async Task WhenEditsOfWithTheName(string actor, string name, string ownerEmail, string newName)
    {
        await EditNameAsync(actor, context.Habit(name, ownerEmail: ownerEmail), newName);
    }

    [When("^" + Actor + " edits `([^`]+)` to ((?:(?!` to ).)+)$")]
    public async Task WhenEditsTo(string actor, string name, string target)
    {
        var record = context.Habit(name);
        var spec = record.Spec.Clone();
        F002Text.ApplyEditTarget(spec, target);
        await EditAsync(actor, record, spec);
    }

    [When("^" + Actor + " edits `([^`]+)` to (.+) and `([^`]+)` to (.+)$")]
    public async Task WhenEditsBoth(string actor, string firstName, string firstTarget, string secondName, string secondTarget)
    {
        foreach (var (name, target) in new[] { (firstName, firstTarget), (secondName, secondTarget) })
        {
            var record = context.Habit(name);
            var spec = record.Spec.Clone();
            F002Text.ApplyEditTarget(spec, target);
            await EditAsync(actor, record, spec);
        }
    }

    [When("^" + Actor + " edits it to (.+?)(?:, keeping its other fields)?$")]
    public async Task WhenEditsItTo(string actor, string target)
    {
        await EditItToAsync(actor, target);
    }

    /// <summary>"then edits it ...": the acting user of the step before edits again.</summary>
    [When("^then edits it to (.+?)(?:, keeping its other fields)?$")]
    public async Task WhenThenEditsItTo(string target)
    {
        await EditItToAsync(null, target);
    }

    [When("^" + Actor + " edits it, keeping the name `([^`]+)`, to (.+)$")]
    public async Task WhenEditsItKeepingTheName(string actor, string name, string target)
    {
        await EditItKeepingTheNameAsync(actor, name, target);
    }

    /// <summary>"then edits it, keeping the name ...": the acting user of the step before edits again.</summary>
    [When("^then edits it, keeping the name `([^`]+)`, to (.+)$")]
    public async Task WhenThenEditsItKeepingTheName(string name, string target)
    {
        await EditItKeepingTheNameAsync(null, name, target);
    }

    [When("^" + Actor + " edits, with (.+), an unknown identifier$")]
    public async Task WhenEditsWithFieldsAnUnknownIdentifier(string actor, string fields)
    {
        var (user, _) = context.ResolveActor(actor);
        var spec = new HabitSpec();
        F002Text.ApplyFields(spec, fields);
        context.Record(await driver.EditCatalogHabitAsync(user, new HabitId(Guid.NewGuid()), spec.ToCatalogInput()));
    }

    [When("^" + Actor + " edits, with (.+), with the identifier of `([^`]+)`$")]
    public async Task WhenEditsWithFieldsWithTheIdentifierOf(string actor, string fields, string name)
    {
        var (user, _) = context.ResolveActor(actor);
        var spec = new HabitSpec();
        F002Text.ApplyFields(spec, fields);
        context.Record(await driver.EditCatalogHabitAsync(user, context.Habit(name).Id, spec.ToCatalogInput()));
    }

    /// <summary>
    /// "edits an unknown identifier with the name ..." is written the same way in AC-039 (catalog administration,
    /// US3-AS18) and AC-057 (custom habits, US4-AS11); the item's tag says which use case the step is about.
    /// </summary>
    [When("^" + Actor + " edits an unknown identifier with the name (.+)$")]
    public async Task WhenEditsAnUnknownIdentifierWithTheName(string actor, string name)
    {
        var (user, _) = context.ResolveActor(actor);
        var spec = new HabitSpec();
        spec.SetName(F002Text.ParseTextExpression(name));
        var unknown = new HabitId(Guid.NewGuid());
        if (scenario.ScenarioInfo.CombinedTags.Contains("AC-057"))
        {
            context.Record(await driver.EditCustomHabitAsync(user, unknown, spec.ToCustomInput()));
        }
        else
        {
            context.Record(await driver.EditCatalogHabitAsync(user, unknown, spec.ToCatalogInput()));
        }
    }

    private async Task EditNameAsync(string actor, HabitRecord record, string newName)
    {
        var spec = record.Spec.Clone();
        spec.SetName(F002Text.ParseTextExpression(newName));
        await EditAsync(actor, record, spec);
    }

    private async Task EditItToAsync(string? actor, string target)
    {
        var record = context.CurrentHabitOrThrow;
        var spec = record.Spec.Clone();
        F002Text.ApplyEditTarget(spec, target);
        await EditAsync(actor, record, spec);
    }

    private async Task EditItKeepingTheNameAsync(string? actor, string name, string target)
    {
        var record = context.CurrentHabitOrThrow;
        var spec = record.Spec.Clone();
        spec.SetName(TextValue.Of(name));
        F002Text.ApplyEditTarget(spec, target);
        await EditAsync(actor, record, spec);
    }

    /// <summary>Edits <paramref name="record"/> as <paramref name="actor"/>, or as the last acting user when it is null ("then edits it").</summary>
    private async Task EditAsync(string? actor, HabitRecord record, HabitSpec spec)
    {
        ActingUser user;
        if (string.IsNullOrEmpty(actor))
        {
            user = context.LastActor ?? throw new InvalidOperationException("No acting user before \"then edits it\".");
        }
        else
        {
            (user, _) = context.ResolveActor(actor);
        }

        context.CurrentHabit = record;
        context.LastAttemptedGoals = spec.Goals;
        if (record.Kind == HabitKind.Catalog)
        {
            var result = await driver.EditCatalogHabitAsync(user, record.Id, spec.ToCatalogInput());
            context.Record(result);
            if (result.Outcome == CatalogHabitEditOutcome.Saved)
            {
                record.Accept(spec);
            }
        }
        else
        {
            var result = await driver.EditCustomHabitAsync(user, record.Id, spec.ToCustomInput());
            context.Record(result);
            if (result.Outcome == CustomHabitEditOutcome.Saved)
            {
                record.Accept(spec);
            }
        }
    }
}
