using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Time;

namespace HT.Application.Goals;

/// <summary>
/// The goal store port (research R-08): persistence plumbing that only HT.Application calls and HT.Persistence
/// implements (ADR 0004). Not a use case. Public for the AC reference export; an architecture rule keeps every HT.Ui
/// type away from it (ADR 0010).
/// <para>
/// Goals are private (constitution III). Every member takes the owner first and acts only on that owner's goals, in
/// the same statement, so another owner's goal and no goal look the same; there is no lookup by goal identifier alone.
/// The single exception, added with US7, is <c>CountUsingCatalogHabitAsync(HabitId)</c>: a count across owners for the
/// unpublish confirmation, which returns only a number (ADR 0012, FR-033).
/// </para>
/// </summary>
public interface IGoalStore
{
    /// <summary>
    /// Creates a goal in one transaction (research R-08 steps 2 to 6). <see cref="GoalWriteOutcome.TimeZoneChanged"/>,
    /// with nothing written, when the owner's stored time zone, with none read as <paramref name="defaultZone"/>, is
    /// not <paramref name="expectedZone"/>. <see cref="GoalWriteOutcome.HabitGone"/>, with nothing written, when the
    /// catalog habit is not Published or the owner has no such custom habit. Otherwise it writes the goal, its version 1
    /// with <paramref name="expectedZone"/> applying from the start date, and attempt 1 beginning on the start date;
    /// sets an owner's missing time zone to <paramref name="expectedZone"/>; and increments the owner's goal revision.
    /// </summary>
    Task<GoalWriteOutcome> CreateAsync(
        AccountId owner, NewGoal goal, NamedTimeZone expectedZone, NamedTimeZone defaultZone, CancellationToken cancellationToken);

    /// <summary>
    /// The owner's goals in creation order, each with its habit's current name, category and state, read in one query.
    /// </summary>
    Task<IReadOnlyList<StoredGoal>> ListAsync(AccountId owner, CancellationToken cancellationToken);

    /// <summary>The owner's goal with this identifier; null when the owner has none with it.</summary>
    Task<StoredGoal?> FindAsync(AccountId owner, GoalId id, CancellationToken cancellationToken);

    /// <summary>
    /// How many of the owner's goals are based on the owner's custom habit <paramref name="habit"/> (FR-035); 0 for
    /// another owner's habit, a catalog habit or an unknown identifier.
    /// </summary>
    Task<int> CountUsingCustomHabitAsync(AccountId owner, HabitId habit, CancellationToken cancellationToken);

    /// <summary>
    /// How many of the owner's goals are based on each catalog habit, whatever its state, read in one query (FR-031):
    /// a catalog habit the owner has no goal on is absent. Custom habits are never included.
    /// </summary>
    Task<IReadOnlyDictionary<HabitId, int>> CountByCatalogHabitAsync(AccountId owner, CancellationToken cancellationToken);

    /// <summary>
    /// The owner's goals based on the catalog habit <paramref name="habit"/>, in creation order, each with its habit's
    /// current name, category and state, as <see cref="ListAsync"/> reads them (FR-031); empty for another owner's goals,
    /// a custom habit or an unknown identifier.
    /// </summary>
    Task<IReadOnlyList<StoredGoal>> ListOnCatalogHabitAsync(AccountId owner, HabitId habit, CancellationToken cancellationToken);
}
