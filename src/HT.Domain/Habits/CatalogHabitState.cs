namespace HT.Domain.Habits;

/// <summary>The state of a catalog habit (FR-011). Only Published habits are visible outside administration.</summary>
public enum CatalogHabitState
{
    Draft,
    Published,
    Unpublished,
}
