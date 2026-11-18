using HT.Domain.Goals;

namespace HT.Application.Goals;

/// <summary>
/// What goal creation writes (research R-08): the checked habit, start date, own name and target. The store adds the
/// identifier, version 1 with the expected time zone, and attempt 1.
/// </summary>
public sealed record NewGoal(GoalHabit Habit, DateOnly StartDate, GoalName? OwnName, GoalTarget Target);
