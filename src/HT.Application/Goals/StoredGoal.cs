using HT.Domain.Goals;

namespace HT.Application.Goals;

/// <summary>What <see cref="IGoalStore"/> reads: the goal and its habit's current name, category and state.</summary>
public sealed record StoredGoal(Goal Goal, GoalHabitView Habit);
