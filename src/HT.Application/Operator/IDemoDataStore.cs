using HT.Domain.Accounts;
using HT.Domain.Habits;

namespace HT.Application.Operator;

/// <summary>
/// The demo data port (research R-09, ADR 0013). Each member is one database transaction, so a demo account and its
/// custom habits, and the seed catalog, are created as a whole or not at all. Its only read is whether the catalog is
/// empty; it reads no private data. Public for the AC reference export; architecture rules keep every HT.Ui type, and
/// every HT.Application type outside <c>HT.Application.Operator</c>, away from it (ADR 0010, research R-12).
/// </summary>
public interface IDemoDataStore
{
    /// <summary>
    /// Creates a registered-user account with this email and password and gives it these custom habits, in one
    /// transaction. An email that already has an account gives <see cref="CreateDemoAccountOutcome.EmailTaken"/> and
    /// creates nothing. A custom habit that cannot be written is a fault that rolls everything back; its exception
    /// names no habit (HP-015).
    /// </summary>
    Task<CreateDemoAccountOutcome> CreateDemoAccountAsync(
        Email email, string password, IReadOnlyList<HabitDefinition> customHabits, CancellationToken cancellationToken);

    /// <summary>
    /// Creates every habit with its state, in one serializable transaction, only when no catalog habit exists; returns
    /// false and creates nothing otherwise, also when a concurrent writer wins (a unique-index violation or a
    /// deadlock).
    /// </summary>
    Task<bool> TryCreateSeedCatalogAsync(IReadOnlyList<SeedCatalogHabit> habits, CancellationToken cancellationToken);
}

/// <summary>The outcome of <see cref="IDemoDataStore.CreateDemoAccountAsync"/>.</summary>
public enum CreateDemoAccountOutcome
{
    /// <summary>The account and its custom habits exist.</summary>
    Created,

    /// <summary>The email already had an account; nothing was created.</summary>
    EmailTaken,
}
