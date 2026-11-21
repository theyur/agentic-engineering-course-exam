using HT.Domain.Goals;

namespace HT.Application.Goals;

/// <summary>
/// The goal list's item of one stored goal (slice 4 FR-014, FR-015), shared by <see cref="GoalService.ListAsync"/> and
/// the Goals page's overview (slice 6 research R-07). Moved from <see cref="GoalService"/> without a change of
/// behaviour, so both give the same item, field by field, at the same instant.
/// </summary>
internal static class GoalListItems
{
    /// <summary>The list fields of FR-015 and FR-014 at <paramref name="now"/>.</summary>
    internal static GoalListItem Of(StoredGoal stored, DateTimeOffset now)
    {
        var goal = stored.Goal;
        var current = GoalTimeline.CurrentDate(goal, now);
        var target = GoalTimeline.VersionOf(goal, current ?? goal.StartDate).Settings.Target;
        return new GoalListItem
        {
            Id = goal.Id,
            Name = GoalText.Name(goal, stored.Habit.Name),
            HabitName = stored.Habit.Name,
            HabitKind = stored.Habit.Kind,
            BuildOrReduce = target.BuildOrReduce,
            Measurement = target.Measurement,
            TargetText = GoalText.TargetText(target),
            ScheduleText = GoalText.ScheduleText(target),
            ScheduleKind = target.ScheduleKindOf,
            StartDate = current is null ? goal.StartDate : null,
            NextCommitment = GoalTimeline.NextCommitment(goal, now),
            FirstWeekStarts = FirstWeekNotBegun(goal, now),
        };
    }

    /// <summary>A per-week goal's first Monday while its week has not begun at <paramref name="now"/>; null otherwise.</summary>
    private static DateOnly? FirstWeekNotBegun(Goal goal, DateTimeOffset now) =>
        GoalTimeline.FirstWeekStart(goal) is { } monday && GoalTimeline.VersionOf(goal, monday).Settings.TimeZone.StartOfDay(monday) > now
            ? monday
            : null;
}
