using HT.Application.Records;
using HT.Domain.Goals;

namespace HT.Application.Goals;

/// <summary>
/// The goal list's item of one stored goal (slice 4 FR-014, FR-015), shared by <see cref="GoalService.ListAsync"/> and
/// the Goals page's overview (slice 6 research R-07). Moved from <see cref="GoalService"/> without a change of
/// behaviour, so both give the same item, field by field, at the same instant.
/// </summary>
internal static class GoalListItems
{
    /// <summary>
    /// The list fields of FR-015 and FR-014 at <paramref name="now"/>. Slice 8 (FR-004, FR-018, AC-035, research R-07,
    /// R-08): the target fields and the name of the latest attempt's settings in effect at <paramref name="now"/>; the
    /// ending fields; the start date of a latest attempt that has not begun, unless the goal is ended; no next commitment
    /// and no first-week date for an ended goal (<see cref="GoalTimeline"/> gives none).
    /// </summary>
    internal static GoalListItem Of(StoredGoal stored, DateTimeOffset now)
    {
        var goal = stored.Goal;
        var target = GoalTimeline.VersionInEffect(goal, now).Settings.Target;
        var ending = goal.Attempt.Ending;
        return new GoalListItem
        {
            Id = goal.Id,
            Name = GoalText.Name(goal, stored.Habit.Name, now),
            HabitName = stored.Habit.Name,
            HabitKind = stored.Habit.Kind,
            BuildOrReduce = target.BuildOrReduce,
            Measurement = target.Measurement,
            TargetText = GoalText.TargetText(target),
            ScheduleText = GoalText.ScheduleText(target),
            ScheduleKind = target.ScheduleKindOf,
            StartDate = ending is null && !GoalTimeline.LatestAttemptHasBegun(goal, now) ? AttemptEntries.TimingDateOf(goal, goal.Attempt.Number) : null,
            NextCommitment = GoalTimeline.NextCommitment(goal, now),
            FirstWeekStarts = FirstWeekNotBegun(goal, now),
            IsEnded = ending is not null,
            EndingDate = ending?.Date,
            Outcome = ending?.Outcome,
        };
    }

    /// <summary>A per-week goal's first Monday while its week has not begun at <paramref name="now"/>; null otherwise.</summary>
    private static DateOnly? FirstWeekNotBegun(Goal goal, DateTimeOffset now) =>
        GoalTimeline.FirstWeekStart(goal) is { } monday && GoalTimeline.VersionOf(goal, monday).Settings.TimeZone.StartOfDay(monday) > now
            ? monday
            : null;
}
