using HT.Application.Records;

namespace HT.Application.Reports;

/// <summary>
/// One attempt of a goal with its recent results and History (slice 8 FR-014, data-model.md "AttemptService views"), all
/// computed at the supplied time. Private to the goal's owner (FR-022).
/// </summary>
public sealed record AttemptDetails
{
    public required AttemptView Attempt { get; init; }

    /// <summary>The attempt's latest 30 ended results, oldest first (slice 6 FR-005 with slice 7 FR-015).</summary>
    public required RecentResults Recent { get; init; }

    /// <summary>
    /// The attempt's History (slice 5 FR-009 with slice 7 FR-017): its commitments or periods from its start date to its
    /// last date, or to the goal's current date while it is not ended, newest first. When that range has more than 366
    /// dates, only the entries that overlap its newest 366 dates. The goal-level settings are that attempt's settings in
    /// effect (<c>GoalTimeline.VersionInEffect(goal, number, now)</c>), and each entry carries its own settings.
    /// </summary>
    public required GoalHistory History { get; init; }
}
