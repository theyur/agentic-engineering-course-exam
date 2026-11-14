using HT.Application.Accounts;
using HT.Domain.Accounts;
using HT.Domain.Habits;

namespace HT.Application.Habits;

/// <summary>
/// Catalog administration for administrators (FR-011 to FR-016). Every use case takes the acting user first and
/// checks in FR-016's order: signed in, administrator, the identifier, the fields or the state rule, then name
/// uniqueness. Every refusal changes nothing (ADR 0005).
/// </summary>
public sealed class CatalogAdministrationService
{
    /// <summary>State swaps per publish or unpublish; after this many lost races the call fails (research R-08).</summary>
    private const int MaxStateSwaps = 10;

    private readonly ICatalogHabitStore _store;

    public CatalogAdministrationService(ICatalogHabitStore store)
    {
        ArgumentNullException.ThrowIfNull(store);
        _store = store;
    }

    /// <summary>Every catalog habit in any state that matches the search, in name order, with the total (FR-012).</summary>
    public async Task<CatalogHabitListResult> ListAsync(ActingUser actingUser, string? search, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actingUser);
        if (Refusal(actingUser) is { } refusal)
        {
            return CatalogHabitListResult.Refused(refusal.As<CatalogHabitListOutcome>(), refusal.Message);
        }

        var habits = await _store.ListAllAsync(cancellationToken).ConfigureAwait(false);
        var listed = habits
            .Where(h => h.Definition.Name.Matches(search))
            .OrderBy(h => h.Definition.Name, HabitName.Order)
            .Select(h => new CatalogHabitListItem(h.Id, h.Definition.Name.Value, h.Definition.Category, h.Definition.Intent, h.State))
            .ToList();
        return CatalogHabitListResult.Found(listed, habits.Count);
    }

    /// <summary>One catalog habit in any state (FR-012); an identifier of no catalog habit is not found (FR-013).</summary>
    public async Task<CatalogHabitAdminResult> GetAsync(ActingUser actingUser, HabitId id, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actingUser);
        if (Refusal(actingUser) is { } refusal)
        {
            return CatalogHabitAdminResult.Refused(refusal.As<CatalogHabitAdminOutcome>(), refusal.Message);
        }

        if (await _store.FindAsync(id, cancellationToken).ConfigureAwait(false) is not { } habit)
        {
            return CatalogHabitAdminResult.Refused(CatalogHabitAdminOutcome.NotFound, CatalogTexts.NotFound);
        }

        var details = CatalogViews.Details(habit);
        return CatalogHabitAdminResult.Found(new CatalogHabitAdminDetails(
            details.Id, details.Name, details.Category, details.Intent, details.Icon, details.Description,
            details.MarkedSuggestedStartingGoal, details.SuggestedStartingGoals, habit.State));
    }

    /// <summary>Creates a Draft catalog habit (FR-004 to FR-007, FR-011).</summary>
    public async Task<CatalogHabitCreateResult> CreateAsync(ActingUser actingUser, CatalogHabitInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actingUser);
        ArgumentNullException.ThrowIfNull(input);
        if (Refusal(actingUser) is { } refusal)
        {
            return CatalogHabitCreateResult.Refused(refusal.As<CatalogHabitCreateOutcome>(), refusal.Message);
        }

        if (!CatalogHabitInputCheck.TryCheck(input, out var definition, out var goals, out var problem))
        {
            return CatalogHabitCreateResult.Refused(problem!.As<CatalogHabitCreateOutcome>(), problem.Message, problem.Position);
        }

        if (await _store.NameKeyExistsAsync(definition.Name.Key, null, cancellationToken).ConfigureAwait(false))
        {
            return CatalogHabitCreateResult.Refused(CatalogHabitCreateOutcome.NameTaken, CatalogTexts.NameTaken);
        }

        var created = await _store.CreateAsync(definition, goals, cancellationToken).ConfigureAwait(false);
        return created.Id is { } id
            ? CatalogHabitCreateResult.Created(id)
            : CatalogHabitCreateResult.Refused(CatalogHabitCreateOutcome.NameTaken, CatalogTexts.NameTaken);
    }

    /// <summary>Replaces every field of a catalog habit in any state; the state never changes (FR-012).</summary>
    public async Task<CatalogHabitEditResult> EditAsync(ActingUser actingUser, HabitId id, CatalogHabitInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actingUser);
        ArgumentNullException.ThrowIfNull(input);
        if (Refusal(actingUser) is { } refusal)
        {
            return CatalogHabitEditResult.Refused(refusal.As<CatalogHabitEditOutcome>(), refusal.Message);
        }

        if (await _store.FindAsync(id, cancellationToken).ConfigureAwait(false) is null)
        {
            return CatalogHabitEditResult.Refused(CatalogHabitEditOutcome.NotFound, CatalogTexts.NotFound);
        }

        if (!CatalogHabitInputCheck.TryCheck(input, out var definition, out var goals, out var problem))
        {
            return CatalogHabitEditResult.Refused(problem!.As<CatalogHabitEditOutcome>(), problem.Message, problem.Position);
        }

        if (await _store.NameKeyExistsAsync(definition.Name.Key, id, cancellationToken).ConfigureAwait(false))
        {
            return CatalogHabitEditResult.Refused(CatalogHabitEditOutcome.NameTaken, CatalogTexts.NameTaken);
        }

        return await _store.UpdateAsync(id, definition, goals, cancellationToken).ConfigureAwait(false) switch
        {
            UpdateCatalogHabitOutcome.Saved => CatalogHabitEditResult.Saved,
            UpdateCatalogHabitOutcome.NameTaken => CatalogHabitEditResult.Refused(CatalogHabitEditOutcome.NameTaken, CatalogTexts.NameTaken),
            _ => CatalogHabitEditResult.Refused(CatalogHabitEditOutcome.NotFound, CatalogTexts.NotFound),
        };
    }

    /// <summary>Publishes a Draft or Unpublished habit; a Published one stays Published with success (FR-011).</summary>
    public async Task<CatalogHabitPublishResult> PublishAsync(ActingUser actingUser, HabitId id, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actingUser);
        if (Refusal(actingUser) is { } refusal)
        {
            return CatalogHabitPublishResult.Refused(refusal.As<CatalogHabitPublishOutcome>(), refusal.Message);
        }

        var outcome = await ChangeStateAsync(id, CatalogHabitStates.Publish, cancellationToken).ConfigureAwait(false);
        return outcome switch
        {
            StateChange.NotFound => CatalogHabitPublishResult.Refused(CatalogHabitPublishOutcome.NotFound, CatalogTexts.NotFound),
            _ => CatalogHabitPublishResult.Published,
        };
    }

    /// <summary>Unpublishes a Published habit; an Unpublished one stays with success; a Draft is not published (FR-011).</summary>
    public async Task<CatalogHabitUnpublishResult> UnpublishAsync(ActingUser actingUser, HabitId id, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actingUser);
        if (Refusal(actingUser) is { } refusal)
        {
            return CatalogHabitUnpublishResult.Refused(refusal.As<CatalogHabitUnpublishOutcome>(), refusal.Message);
        }

        var outcome = await ChangeStateAsync(id, CatalogHabitStates.Unpublish, cancellationToken).ConfigureAwait(false);
        return outcome switch
        {
            StateChange.NotFound => CatalogHabitUnpublishResult.Refused(CatalogHabitUnpublishOutcome.NotFound, CatalogTexts.NotFound),
            StateChange.NotPublished => CatalogHabitUnpublishResult.Refused(CatalogHabitUnpublishOutcome.NotPublished, CatalogTexts.NotPublished),
            _ => CatalogHabitUnpublishResult.Unpublished,
        };
    }

    /// <summary>
    /// Reads the habit, lets Domain decide the transition, and writes it with the store's compare-and-swap; a lost
    /// race is read and decided again, at most <see cref="MaxStateSwaps"/> swaps in all (research R-08).
    /// </summary>
    private async Task<StateChange> ChangeStateAsync(
        HabitId id, Func<CatalogHabitState, CatalogHabitTransition> decide, CancellationToken cancellationToken)
    {
        for (var swap = 0; swap < MaxStateSwaps; swap++)
        {
            if (await _store.FindAsync(id, cancellationToken).ConfigureAwait(false) is not { } habit)
            {
                return StateChange.NotFound;
            }

            if (decide(habit.State).Next is not { } next)
            {
                return StateChange.NotPublished;
            }

            if (next == habit.State
                || await _store.TrySetStateAsync(id, habit.State, next, cancellationToken).ConfigureAwait(false))
            {
                return StateChange.Done;
            }
        }

        throw new InvalidOperationException("Changing a catalog habit's state lost too many races in a row.");
    }

    /// <summary>FR-016 steps 1 and 2: signed in, then an administrator; null when both hold.</summary>
    private static AccessRefusal? Refusal(ActingUser actingUser)
    {
        if (actingUser.IsAnonymous)
        {
            return new AccessRefusal("NotSignedIn", AccountTexts.NotSignedIn);
        }

        return actingUser.Role == Role.Administrator ? null : new AccessRefusal("NotAdministrator", CatalogTexts.NotAdministrator);
    }

    private enum StateChange
    {
        NotFound,
        NotPublished,
        Done,
    }

    private sealed record AccessRefusal(string Outcome, string Message)
    {
        public TOutcome As<TOutcome>()
            where TOutcome : struct, Enum => Enum.Parse<TOutcome>(Outcome);
    }
}
