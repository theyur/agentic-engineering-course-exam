namespace HT.Domain.Goals;

/// <summary>
/// What a settings change does to the latest attempt (slice 8 FR-002, research R-03), as
/// <see cref="GoalTimeline.SettingsChangeOf"/> decides it: the attempt's new <paramref name="Versions"/> in date order,
/// and the date from which the new settings apply. The store writes the versions as they are.
/// </summary>
public sealed record GoalSettingsChange(IReadOnlyList<GoalSettingsVersion> Versions, DateOnly AppliesFrom);
