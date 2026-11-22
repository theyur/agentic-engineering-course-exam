namespace HT.Domain.Goals;

/// <summary>
/// One attempt of a goal with its own settings versions, in date order (research R-01): a code grouping for the
/// <see cref="Goal"/> constructor and <see cref="Goal.Attempts"/>, not a domain term.
/// </summary>
public sealed record GoalAttemptVersions(GoalAttempt Attempt, IReadOnlyList<GoalSettingsVersion> Versions);
