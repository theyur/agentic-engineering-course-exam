using HT.Domain.Goals;
using HT.Domain.Habits;

namespace HT.Application.Goals;

/// <summary>
/// The habit of a goal as the goal pages show it: its identifier and kind, its current name and category, and for a
/// catalog habit whether it is in the catalog, false once it is Unpublished (FR-034); null for a custom habit.
/// </summary>
public sealed record GoalHabitView(HabitId Id, HabitKind Kind, string Name, Category Category, bool? IsInCatalog);
