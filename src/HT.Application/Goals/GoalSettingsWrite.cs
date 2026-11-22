using HT.Domain.Goals;

namespace HT.Application.Goals;

/// <summary>
/// The write of a settings change (slice 8 FR-001 to FR-006, research R-05) for
/// <see cref="IGoalStore.TryChangeSettingsAsync"/>: the goal, the number of the attempt changed (its latest), the goal's
/// own name to store (null for none), and the attempt's new versions as <see cref="GoalTimeline.SettingsChangeOf"/>
/// decided them, or null for a change of the name only.
/// </summary>
public sealed record GoalSettingsWrite(GoalId Goal, int Attempt, GoalName? Name, IReadOnlyList<GoalSettingsVersion>? Versions);
