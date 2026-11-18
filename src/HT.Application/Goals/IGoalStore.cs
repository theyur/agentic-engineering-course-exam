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
/// The single exception is <see cref="CountUsingCatalogHabitAsync"/>: a count across owners for the unpublish
/// confirmation, which returns only a number (ADR 0012, FR-033).
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
    /// The owner's stored time zone and goal revision, then all the owner's goals as <see cref="ListAsync"/> reads them,
    /// in that order (research R-08): a goal committed before the account row was read is in the goals read after it,
    /// and one committed later has moved the revision on, so a <see cref="TryChangeTimeZoneAsync"/> with this revision
    /// fails. Null when the owner has no account.
    /// </summary>
    Task<OwnerGoals?> ReadOwnerGoalsAsync(AccountId owner, CancellationToken cancellationToken);

    /// <summary>
    /// Changes the owner's time zone in one transaction under the account row's update lock (FR-026, research R-08).
    /// False, with nothing written, when the stored time zone is no longer <paramref name="expectedStoredZone"/> (null:
    /// none stored) or the goal revision is no longer <paramref name="expectedRevision"/>, and when a write names a goal
    /// that is not one of the owner's (another owner's or an unknown one, constitution III) or names a goal twice.
    /// Otherwise it applies every write to the goal's settings versions:
    /// <see cref="GoalVersionChangeKind.ReplaceFirst"/> gives version 1 the new zone,
    /// <see cref="GoalVersionChangeKind.ReplacePending"/> replaces the last version with one of the new zone and date,
    /// <see cref="GoalVersionChangeKind.Add"/> adds a version with the last version's target, and
    /// <see cref="GoalVersionChangeKind.None"/> writes nothing; then stores <paramref name="next"/> as the account's
    /// zone and increments the goal revision.
    /// </summary>
    Task<bool> TryChangeTimeZoneAsync(
        AccountId owner,
        string? expectedStoredZone,
        int expectedRevision,
        NamedTimeZone next,
        IReadOnlyList<GoalVersionWrite> writes,
        CancellationToken cancellationToken);

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

    /// <summary>
    /// How many goals of all owners are based on the catalog habit <paramref name="habit"/>, whatever its state; 0 for a
    /// custom habit or an unknown identifier. The one member without an owner (ADR 0012, FR-033): a count across owners
    /// for the administrator's unpublish confirmation, so it returns only a number and never a goal, an owner or a name.
    /// It takes no lock: the count is informative, and a goal created after it never stops a confirmed unpublish.
    /// </summary>
    Task<int> CountUsingCatalogHabitAsync(HabitId habit, CancellationToken cancellationToken);
}
