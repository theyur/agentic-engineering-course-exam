using HT.Domain.Goals;

namespace HT.Application.Goals;

/// <summary>
/// The write of an ending (slice 8 FR-007 to FR-009, research R-04, R-05) for <see cref="IGoalStore.TryEndAsync"/>: the
/// goal, the number of the attempt ended (its latest), the <see cref="GoalEnding"/>, and the last date and kept versions
/// that <see cref="GoalTimeline.EndingOf"/> decided. The last date is not stored; the store uses it for the cuts.
/// </summary>
public sealed record GoalEndingWrite(
    GoalId Goal, int Attempt, GoalEnding Ending, DateOnly LastDate, IReadOnlyList<GoalSettingsVersion> KeptVersions);
