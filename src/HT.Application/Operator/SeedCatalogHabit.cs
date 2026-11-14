using HT.Domain.Habits;

namespace HT.Application.Operator;

/// <summary>
/// One habit of the seed catalog (FR-030, research R-10): its fields, its suggested starting goals and the state it
/// is created with. Immutable; its values have passed the Domain checks, since only Domain makes them.
/// </summary>
public sealed class SeedCatalogHabit
{
    public SeedCatalogHabit(HabitDefinition definition, SuggestedStartingGoals suggestedStartingGoals, CatalogHabitState state)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(suggestedStartingGoals);
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "Not a catalog habit state.");
        }

        Definition = definition;
        SuggestedStartingGoals = suggestedStartingGoals;
        State = state;
    }

    public HabitDefinition Definition { get; }

    public SuggestedStartingGoals SuggestedStartingGoals { get; }

    public CatalogHabitState State { get; }
}
