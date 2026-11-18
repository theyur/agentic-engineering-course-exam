using System.Collections.ObjectModel;
using HT.Domain.Goals;
using HT.Domain.Habits;

namespace HT.Application.Operator;

/// <summary>
/// One demo goal of FR-040 (research R-10), as demo loading passes it to <see cref="IDemoDataStore"/>. Its habit is
/// either a catalog habit, named by its name and the catalog states it accepts, or one of the demo custom habits
/// passed in the same call, named by its position in that list. Immutable; the target has passed the Domain checks.
/// The store writes the goal only when its habit qualifies, and skips it otherwise (FR-040).
/// </summary>
public sealed class DemoGoal
{
    private static readonly IReadOnlySet<CatalogHabitState> NoStates = new ReadOnlySet<CatalogHabitState>(new HashSet<CatalogHabitState>());

    private DemoGoal(
        string? catalogHabitName,
        int? customHabitPosition,
        IReadOnlySet<CatalogHabitState> acceptedStates,
        GoalTarget target,
        GoalName? ownName,
        DateOnly startDate)
    {
        CatalogHabitName = catalogHabitName;
        CustomHabitPosition = customHabitPosition;
        AcceptedStates = acceptedStates;
        Target = target;
        OwnName = ownName;
        StartDate = startDate;
    }

    /// <summary>The name of the catalog habit, found by its name key (slice 2 FR-004); null for a custom-habit goal.</summary>
    public string? CatalogHabitName { get; }

    /// <summary>The index into the same call's <c>customHabits</c>; null for a catalog-habit goal.</summary>
    public int? CustomHabitPosition { get; }

    /// <summary>The catalog habit's states for which the goal is written; empty for a custom-habit goal.</summary>
    public IReadOnlySet<CatalogHabitState> AcceptedStates { get; }

    public GoalTarget Target { get; }

    /// <summary>The goal's own name; null when it has none.</summary>
    public GoalName? OwnName { get; }

    /// <summary>The start date of the goal and of its first attempt and settings version.</summary>
    public DateOnly StartDate { get; }

    /// <summary>A goal on the catalog habit with this name, written only when the habit is in one of <paramref name="acceptedStates"/>.</summary>
    public static DemoGoal OnCatalogHabit(
        string habitName, IEnumerable<CatalogHabitState> acceptedStates, GoalTarget target, GoalName? ownName, DateOnly startDate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(habitName);
        ArgumentNullException.ThrowIfNull(acceptedStates);
        ArgumentNullException.ThrowIfNull(target);
        var states = acceptedStates.ToHashSet();
        if (states.Count == 0 || !states.All(Enum.IsDefined))
        {
            throw new ArgumentException("A catalog-habit demo goal needs at least one defined catalog habit state.", nameof(acceptedStates));
        }

        return new DemoGoal(habitName, null, new ReadOnlySet<CatalogHabitState>(states), target, ownName, startDate);
    }

    /// <summary>A goal on the demo custom habit at this position of the same call's <c>customHabits</c>.</summary>
    public static DemoGoal OnCustomHabit(int position, GoalTarget target, GoalName? ownName, DateOnly startDate)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(position);
        ArgumentNullException.ThrowIfNull(target);
        return new DemoGoal(null, position, NoStates, target, ownName, startDate);
    }
}
