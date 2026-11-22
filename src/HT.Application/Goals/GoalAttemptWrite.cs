using HT.Domain.Goals;

namespace HT.Application.Goals;

/// <summary>
/// The write of a new attempt (slice 8 FR-012, research R-05) for <see cref="IGoalStore.TryStartAttemptAsync"/>: the
/// goal, the new attempt's number (the latest number + 1), and its first version, which applies from its start date.
/// </summary>
public sealed record GoalAttemptWrite(GoalId Goal, int Number, GoalSettingsVersion First);
