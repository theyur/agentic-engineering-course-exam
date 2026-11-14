using HT.Domain.Accounts;

namespace HT.Domain.Habits;

/// <summary>
/// A behaviour one registered user created for their own goals (FR-020): its fields and its owner. No suggested
/// starting goals (FR-006). Visible only to its owner. Immutable.
/// </summary>
public sealed class CustomHabit
{
    public CustomHabit(HabitId id, AccountId owner, HabitDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        Id = id;
        Owner = owner;
        Definition = definition;
    }

    public HabitId Id { get; }

    public AccountId Owner { get; }

    public HabitDefinition Definition { get; }
}
