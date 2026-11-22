namespace HT.Application.Goals;

/// <summary>
/// The outcome of the conditional goal-change writes of <see cref="IGoalStore"/> (slice 8 research R-05): the store's
/// outcome, not a use-case outcome.
/// </summary>
public enum GoalChangeWriteOutcome
{
    /// <summary>The write was made and the owner's goal revision incremented.</summary>
    Written,

    /// <summary>The stored zone, the goal revision or the write's precondition no longer holds: nothing was written.</summary>
    Stale,

    /// <summary>The goal is not one of the owner's: nothing was written.</summary>
    GoalNotFound,
}
