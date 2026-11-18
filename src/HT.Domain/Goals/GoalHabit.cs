using HT.Domain.Habits;

namespace HT.Domain.Goals;

/// <summary>The habit a goal is based on: its identifier and its kind (FR-001, ADR 0014).</summary>
public readonly record struct GoalHabit(HabitId Id, HabitKind Kind);
