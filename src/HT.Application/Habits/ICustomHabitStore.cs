using HT.Domain.Accounts;
using HT.Domain.Habits;

namespace HT.Application.Habits;

/// <summary>
/// The custom habit store port (research R-08, R-14). Every member takes the owner first and acts only on that
/// owner's custom habits, in the same statement, so another owner's habit and no habit look the same; there is no
/// lookup by identifier alone (constitution III). Public for the AC reference export; an architecture rule keeps
/// every HT.Ui type away from it, and another checks the owner parameter of every member (ADR 0010, R-12).
/// </summary>
public interface ICustomHabitStore
{
    /// <summary>The owner's custom habits, in no particular order.</summary>
    Task<IReadOnlyList<CustomHabit>> ListAsync(AccountId owner, CancellationToken cancellationToken);

    /// <summary>The owner's custom habit with this identifier; null when the owner has none with it.</summary>
    Task<CustomHabit?> FindAsync(AccountId owner, HabitId id, CancellationToken cancellationToken);

    /// <summary>Creates a custom habit; a name whose key the owner already has gives <see cref="CreateCustomHabitOutcome.NameTaken"/>.</summary>
    Task<CreateCustomHabitOutcome> CreateAsync(AccountId owner, HabitDefinition definition, CancellationToken cancellationToken);

    /// <summary>Replaces every field of the owner's custom habit.</summary>
    Task<UpdateCustomHabitOutcome> UpdateAsync(AccountId owner, HabitId id, HabitDefinition definition, CancellationToken cancellationToken);

    /// <summary>Deletes the owner's custom habit for good.</summary>
    Task<DeleteCustomHabitOutcome> DeleteAsync(AccountId owner, HabitId id, CancellationToken cancellationToken);

    /// <summary>True when one of the owner's custom habits other than <paramref name="except"/> has this name key.</summary>
    Task<bool> NameKeyExistsAsync(AccountId owner, string key, HabitId? except, CancellationToken cancellationToken);
}

/// <summary>The outcome of <see cref="ICustomHabitStore.CreateAsync"/>: the new habit's identifier, or the name was taken.</summary>
public sealed class CreateCustomHabitOutcome
{
    private CreateCustomHabitOutcome(HabitId? id) => Id = id;

    /// <summary>The owner already has a custom habit with this name key.</summary>
    public static CreateCustomHabitOutcome NameTaken { get; } = new(null);

    /// <summary>The created habit's identifier; null when the name was taken.</summary>
    public HabitId? Id { get; }

    public bool IsNameTaken => Id is null;

    public static CreateCustomHabitOutcome Created(HabitId id) => new(id);
}

/// <summary>The outcome of <see cref="ICustomHabitStore.UpdateAsync"/>.</summary>
public enum UpdateCustomHabitOutcome
{
    NotFound,
    NameTaken,
    Saved,
}

/// <summary>The outcome of <see cref="ICustomHabitStore.DeleteAsync"/>.</summary>
public enum DeleteCustomHabitOutcome
{
    Deleted,
    NotFound,
}
