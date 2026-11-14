using HT.Domain.Habits;

namespace HT.Application.Habits;

/// <summary>
/// The catalog habit store port: persistence plumbing that only HT.Application calls and HT.Persistence implements
/// (ADR 0004). Not a use case. Public so the AC reference export accepts the assembly; an architecture rule keeps
/// every HT.Ui type away from it (ADR 0010). It has no rule of its own: the services decide, the store reads and
/// writes (research R-08). Reads return the database's current state.
/// </summary>
public interface ICatalogHabitStore
{
    /// <summary>Every Published catalog habit, in no particular order.</summary>
    Task<IReadOnlyList<CatalogHabit>> ListPublishedAsync(CancellationToken cancellationToken);

    /// <summary>Every catalog habit in any state, in no particular order.</summary>
    Task<IReadOnlyList<CatalogHabit>> ListAllAsync(CancellationToken cancellationToken);

    /// <summary>The catalog habit with this identifier, in any state; null when there is none.</summary>
    Task<CatalogHabit?> FindAsync(HabitId id, CancellationToken cancellationToken);

    /// <summary>The catalog habit with this identifier when it is Published; null otherwise.</summary>
    Task<CatalogHabit?> FindPublishedAsync(HabitId id, CancellationToken cancellationToken);

    /// <summary>Creates a Draft catalog habit; a name whose key is taken gives <see cref="CreateCatalogHabitOutcome.NameTaken"/>.</summary>
    Task<CreateCatalogHabitOutcome> CreateAsync(
        HabitDefinition definition, SuggestedStartingGoals suggestedStartingGoals, CancellationToken cancellationToken);

    /// <summary>Replaces every field and the whole list of suggested starting goals, keeping the state.</summary>
    Task<UpdateCatalogHabitOutcome> UpdateAsync(
        HabitId id, HabitDefinition definition, SuggestedStartingGoals suggestedStartingGoals, CancellationToken cancellationToken);

    /// <summary>Sets <paramref name="next"/> only when the stored state is still <paramref name="expected"/>; true when it changed.</summary>
    Task<bool> TrySetStateAsync(HabitId id, CatalogHabitState expected, CatalogHabitState next, CancellationToken cancellationToken);

    /// <summary>True when a catalog habit other than <paramref name="except"/> has this name key (<see cref="HabitName.Key"/>).</summary>
    Task<bool> NameKeyExistsAsync(string key, HabitId? except, CancellationToken cancellationToken);
}

/// <summary>The outcome of <see cref="ICatalogHabitStore.CreateAsync"/>: the new habit's identifier, or the name was taken.</summary>
public sealed class CreateCatalogHabitOutcome
{
    private CreateCatalogHabitOutcome(HabitId? id) => Id = id;

    /// <summary>A catalog habit with this name key exists.</summary>
    public static CreateCatalogHabitOutcome NameTaken { get; } = new(null);

    /// <summary>The created habit's identifier; null when the name was taken.</summary>
    public HabitId? Id { get; }

    public bool IsNameTaken => Id is null;

    public static CreateCatalogHabitOutcome Created(HabitId id) => new(id);
}

/// <summary>The outcome of <see cref="ICatalogHabitStore.UpdateAsync"/>.</summary>
public enum UpdateCatalogHabitOutcome
{
    NotFound,
    NameTaken,
    Saved,
}
