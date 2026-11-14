using HT.Application.Accounts;
using HT.Domain.Accounts;
using HT.Domain.Habits;

namespace HT.Application.Habits;

/// <summary>
/// Private custom habits for registered users (FR-020 to FR-025). Every use case takes the acting user first,
/// checks in FR-016's order — signed in, not an administrator, the identifier, the fields, name uniqueness — and
/// passes the acting user's account as the owner of every store call, so it acts only on that user's own custom
/// habits (constitution III). Every refusal changes nothing.
/// </summary>
public sealed class CustomHabitService
{
    private readonly ICustomHabitStore _store;

    public CustomHabitService(ICustomHabitStore store)
    {
        ArgumentNullException.ThrowIfNull(store);
        _store = store;
    }

    /// <summary>The acting user's custom habits in name order (FR-020).</summary>
    public async Task<CustomHabitListResult> ListAsync(ActingUser actingUser, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actingUser);
        if (Refusal(actingUser, out var owner) is { } refusal)
        {
            return CustomHabitListResult.Refused(refusal.As<CustomHabitListOutcome>(), refusal.Message);
        }

        var habits = await _store.ListAsync(owner, cancellationToken).ConfigureAwait(false);
        return CustomHabitListResult.Found(habits.OrderBy(h => h.Definition.Name, HabitName.Order).Select(View).ToList());
    }

    /// <summary>One of the acting user's custom habits; anything else does not exist (FR-022).</summary>
    public async Task<CustomHabitResult> GetAsync(ActingUser actingUser, HabitId id, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actingUser);
        if (Refusal(actingUser, out var owner) is { } refusal)
        {
            return CustomHabitResult.Refused(refusal.As<CustomHabitOutcome>(), refusal.Message);
        }

        return await _store.FindAsync(owner, id, cancellationToken).ConfigureAwait(false) is { } habit
            ? CustomHabitResult.Found(View(habit))
            : CustomHabitResult.Refused(CustomHabitOutcome.NotFound, CustomHabitTexts.NotFound);
    }

    /// <summary>Creates a custom habit of the acting user (FR-004, FR-005, FR-020).</summary>
    public async Task<CustomHabitCreateResult> CreateAsync(ActingUser actingUser, HabitInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actingUser);
        ArgumentNullException.ThrowIfNull(input);
        if (Refusal(actingUser, out var owner) is { } refusal)
        {
            return CustomHabitCreateResult.Refused(refusal.As<CustomHabitCreateOutcome>(), refusal.Message);
        }

        if (!HabitDefinition.TryCreate(input.Name, input.Description, input.Category, input.Intent, input.Icon, out var definition, out var problem))
        {
            return CustomHabitCreateResult.Refused(Enum.Parse<CustomHabitCreateOutcome>(problem.ToString()), CatalogHabitInputCheck.TextOf(problem));
        }

        if (await _store.NameKeyExistsAsync(owner, definition.Name.Key, null, cancellationToken).ConfigureAwait(false))
        {
            return CustomHabitCreateResult.Refused(CustomHabitCreateOutcome.NameTaken, CustomHabitTexts.NameTaken);
        }

        var created = await _store.CreateAsync(owner, definition, cancellationToken).ConfigureAwait(false);
        return created.Id is { } id
            ? CustomHabitCreateResult.Created(id)
            : CustomHabitCreateResult.Refused(CustomHabitCreateOutcome.NameTaken, CustomHabitTexts.NameTaken);
    }

    /// <summary>Replaces every field of one of the acting user's custom habits (FR-020 to FR-022).</summary>
    public async Task<CustomHabitEditResult> EditAsync(ActingUser actingUser, HabitId id, HabitInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actingUser);
        ArgumentNullException.ThrowIfNull(input);
        if (Refusal(actingUser, out var owner) is { } refusal)
        {
            return CustomHabitEditResult.Refused(refusal.As<CustomHabitEditOutcome>(), refusal.Message);
        }

        if (await _store.FindAsync(owner, id, cancellationToken).ConfigureAwait(false) is null)
        {
            return CustomHabitEditResult.Refused(CustomHabitEditOutcome.NotFound, CustomHabitTexts.NotFound);
        }

        if (!HabitDefinition.TryCreate(input.Name, input.Description, input.Category, input.Intent, input.Icon, out var definition, out var problem))
        {
            return CustomHabitEditResult.Refused(Enum.Parse<CustomHabitEditOutcome>(problem.ToString()), CatalogHabitInputCheck.TextOf(problem));
        }

        if (await _store.NameKeyExistsAsync(owner, definition.Name.Key, id, cancellationToken).ConfigureAwait(false))
        {
            return CustomHabitEditResult.Refused(CustomHabitEditOutcome.NameTaken, CustomHabitTexts.NameTaken);
        }

        return await _store.UpdateAsync(owner, id, definition, cancellationToken).ConfigureAwait(false) switch
        {
            UpdateCustomHabitOutcome.Saved => CustomHabitEditResult.Saved,
            UpdateCustomHabitOutcome.NameTaken => CustomHabitEditResult.Refused(CustomHabitEditOutcome.NameTaken, CustomHabitTexts.NameTaken),
            _ => CustomHabitEditResult.Refused(CustomHabitEditOutcome.NotFound, CustomHabitTexts.NotFound),
        };
    }

    /// <summary>Deletes one of the acting user's custom habits for good (FR-020).</summary>
    public async Task<CustomHabitDeleteResult> DeleteAsync(ActingUser actingUser, HabitId id, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actingUser);
        if (Refusal(actingUser, out var owner) is { } refusal)
        {
            return CustomHabitDeleteResult.Refused(refusal.As<CustomHabitDeleteOutcome>(), refusal.Message);
        }

        return await _store.DeleteAsync(owner, id, cancellationToken).ConfigureAwait(false) == DeleteCustomHabitOutcome.Deleted
            ? CustomHabitDeleteResult.Deleted
            : CustomHabitDeleteResult.Refused(CustomHabitDeleteOutcome.NotFound, CustomHabitTexts.NotFound);
    }

    private static CustomHabitView View(CustomHabit habit) => new(
        habit.Id,
        habit.Definition.Name.Value,
        habit.Definition.Description,
        habit.Definition.Category,
        habit.Definition.Intent,
        habit.Definition.Icon.Name);

    /// <summary>FR-016 steps 1 and 2: signed in, then not an administrator (FR-025); null when both hold.</summary>
    private static AccessRefusal? Refusal(ActingUser actingUser, out AccountId owner)
    {
        owner = default;
        if (actingUser.AccountId is not { } account)
        {
            return new AccessRefusal("NotSignedIn", AccountTexts.NotSignedIn);
        }

        if (actingUser.Role != Role.RegisteredUser)
        {
            return new AccessRefusal("NotForAdministrators", CustomHabitTexts.NotForAdministrators);
        }

        owner = account;
        return null;
    }

    private sealed record AccessRefusal(string Outcome, string Message)
    {
        public TOutcome As<TOutcome>()
            where TOutcome : struct, Enum => Enum.Parse<TOutcome>(Outcome);
    }
}
