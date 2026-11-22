namespace HT.Domain.Goals;

/// <summary>
/// What ending the latest attempt does (slice 8 FR-008, FR-009, research R-04), as <see cref="GoalTimeline.EndingOf"/>
/// decides it: the attempt's <paramref name="LastDate"/>, and the versions it keeps, those that apply on or before the
/// last date and always the first. The store writes them as they are.
/// </summary>
public sealed record GoalEndingEffect(DateOnly LastDate, IReadOnlyList<GoalSettingsVersion> KeptVersions);
