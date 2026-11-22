namespace HT.Application.Goals;

/// <summary>
/// One read for a goal change (slice 8 research R-05): the owner's stored time zone (null only for an account created
/// before slice 4), its goal revision, and the goal with all its attempts, versions and endings, read in that order by
/// <see cref="IGoalStore.ReadOwnerGoalAsync"/>. <see cref="Goal"/> is null when the goal is not one of the owner's. The
/// zone and the revision are the tokens that the conditional writes check.
/// </summary>
public sealed record OwnerGoal(string? StoredTimeZone, int GoalsRevision, StoredGoal? Goal);
