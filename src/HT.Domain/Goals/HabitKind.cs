namespace HT.Domain.Goals;

/// <summary>Where a goal's habit is looked up (ADR 0014): one <c>HabitId</c>, and the kind says which table.</summary>
public enum HabitKind
{
    Catalog,
    Custom,
}
