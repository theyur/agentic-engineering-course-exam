namespace HT.Application.Goals;

/// <summary>
/// One read for a time-zone change (research R-08): the owner's stored time zone (null only for an account created
/// before slice 4), its goal revision, and all its goals, read in that order by <see cref="IGoalStore.ReadOwnerGoalsAsync"/>.
/// The revision is the token that <see cref="IGoalStore.TryChangeTimeZoneAsync"/> checks.
/// </summary>
public sealed record OwnerGoals(string? StoredTimeZone, int GoalsRevision, IReadOnlyList<StoredGoal> Goals);
