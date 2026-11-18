using HT.Domain.Goals;

namespace HT.Application.Goals;

/// <summary>
/// One write of a time-zone change (FR-026, research R-08): the change that <see cref="GoalTimeline.ChangeOf"/> decided
/// for one of the owner's goals.
/// </summary>
public sealed record GoalVersionWrite(GoalId Goal, GoalVersionChange Change);
