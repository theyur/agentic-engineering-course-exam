namespace HT.Domain.Habits;

/// <summary>
/// The identifier of a catalog habit or a custom habit (ADR 0014). Created by the stores; each kind has its own
/// table, so an identifier names at most one habit of either kind.
/// </summary>
public readonly record struct HabitId(Guid Value)
{
    public override string ToString() => Value.ToString();
}
