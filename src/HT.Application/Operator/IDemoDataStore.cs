using HT.Domain.Accounts;
using HT.Domain.Habits;

namespace HT.Application.Operator;

/// <summary>
/// The demo data port (research R-09, slice 4 research R-10, ADR 0013). Each member is one database transaction, so a
/// demo account with its profile, its custom habits and its goals, and the seed catalog, are created as a whole or
/// not at all. Its only reads are whether the catalog is empty and the state of the named catalog habits, which is
/// public catalog data; it reads no private data. Public for the AC reference export; architecture rules keep every
/// HT.Ui type, and every HT.Application type outside <c>HT.Application.Operator</c>, away from it (ADR 0010,
/// research R-12).
/// </summary>
public interface IDemoDataStore
{
    /// <summary>
    /// Creates a registered-user account with this email, password and profile (name, time zone, creation time), gives
    /// it these custom habits and then these goals, in one transaction. An email that already has an account gives
    /// <see cref="CreateDemoAccountOutcome.EmailTaken"/> and creates nothing. A catalog-habit goal finds its habit by
    /// name key and is written only when that habit exists in one of the goal's accepted states; otherwise it is
    /// skipped with no error (FR-040). A custom-habit goal uses the identifier just generated for its position in
    /// <paramref name="customHabits"/>, without reading the habit. Each goal is written with settings version 1 in the
    /// profile's time zone and attempt 1, both from its start date. A custom habit or goal that cannot be written, or a
    /// position outside <paramref name="customHabits"/>, is a fault that rolls everything back; its exception names no
    /// habit, goal or value (HP-015, HP-011).
    /// </summary>
    Task<CreateDemoAccountOutcome> CreateDemoAccountAsync(
        Email email,
        string password,
        IReadOnlyList<HabitDefinition> customHabits,
        DemoAccountProfile profile,
        IReadOnlyList<DemoGoal> goals,
        CancellationToken cancellationToken);

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
    /// <summary>The account, its custom habits and its qualifying goals exist.</summary>
    Created,

    /// <summary>The email already had an account; nothing was created.</summary>
    EmailTaken,
}
