namespace HT.Domain.Habits;

/// <summary>
/// A public, reusable description of a behaviour, maintained by administrators (FR-001): its fields, its
/// suggested starting goals and its state. Immutable.
/// </summary>
public sealed class CatalogHabit
{
    public CatalogHabit(HabitId id, HabitDefinition definition, SuggestedStartingGoals suggestedStartingGoals, CatalogHabitState state)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(suggestedStartingGoals);
        Id = id;
        Definition = definition;
        SuggestedStartingGoals = suggestedStartingGoals;
        State = state;
    }

    public HabitId Id { get; }

    public HabitDefinition Definition { get; }

    public SuggestedStartingGoals SuggestedStartingGoals { get; }

    public CatalogHabitState State { get; }
}
