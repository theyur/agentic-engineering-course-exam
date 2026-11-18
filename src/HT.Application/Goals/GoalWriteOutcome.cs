using HT.Domain.Goals;

namespace HT.Application.Goals;

/// <summary>The kinds of <see cref="GoalWriteOutcome"/>.</summary>
public enum GoalWriteOutcomeKind
{
    Created,
    HabitGone,
    TimeZoneChanged,
}

/// <summary>
/// The outcome of <see cref="IGoalStore.CreateAsync"/> (research R-08): the store's outcome, not a use-case outcome.
/// <see cref="HabitGone"/> when the catalog habit is no longer Published or the owner's custom habit no longer exists;
/// <see cref="TimeZoneChanged"/> when the account's time zone is no longer the expected one. Neither writes anything.
/// </summary>
public sealed class GoalWriteOutcome
{
    private GoalWriteOutcome(GoalWriteOutcomeKind kind, GoalId? id)
    {
        Kind = kind;
        Id = id;
    }

    /// <summary>The habit is gone: nothing was written.</summary>
    public static GoalWriteOutcome HabitGone { get; } = new(GoalWriteOutcomeKind.HabitGone, null);

    /// <summary>The account's time zone changed: nothing was written.</summary>
    public static GoalWriteOutcome TimeZoneChanged { get; } = new(GoalWriteOutcomeKind.TimeZoneChanged, null);

    public GoalWriteOutcomeKind Kind { get; }

    /// <summary>The created goal's identifier; set only for <see cref="GoalWriteOutcomeKind.Created"/>.</summary>
    public GoalId? Id { get; }

    public static GoalWriteOutcome Created(GoalId id) => new(GoalWriteOutcomeKind.Created, id);
}
