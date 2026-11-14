namespace HT.Domain.Habits;

/// <summary>
/// The result of asking for a state change: the next state (which may equal the current one), or the refusal
/// "not published" for unpublishing a Draft.
/// </summary>
public readonly record struct CatalogHabitTransition
{
    private CatalogHabitTransition(CatalogHabitState? next) => Next = next;

    /// <summary>Unpublishing a Draft: refused, the habit stays Draft (FR-011).</summary>
    public static CatalogHabitTransition NotPublished { get; } = new(null);

    /// <summary>The state after the change; null when the change is refused.</summary>
    public CatalogHabitState? Next { get; }

    public bool IsNotPublished => Next is null;

    public static CatalogHabitTransition To(CatalogHabitState next) => new(next);
}

/// <summary>The state rules of a catalog habit (FR-011), as data-model.md's transition table.</summary>
public static class CatalogHabitStates
{
    /// <summary>Draft and Unpublished become Published; Published stays Published.</summary>
    public static CatalogHabitTransition Publish(CatalogHabitState state) => state switch
    {
        CatalogHabitState.Draft or CatalogHabitState.Unpublished or CatalogHabitState.Published =>
            CatalogHabitTransition.To(CatalogHabitState.Published),
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, "Unknown catalog habit state."),
    };

    /// <summary>Published becomes Unpublished; Unpublished stays Unpublished; a Draft is refused as not published.</summary>
    public static CatalogHabitTransition Unpublish(CatalogHabitState state) => state switch
    {
        CatalogHabitState.Draft => CatalogHabitTransition.NotPublished,
        CatalogHabitState.Published or CatalogHabitState.Unpublished => CatalogHabitTransition.To(CatalogHabitState.Unpublished),
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, "Unknown catalog habit state."),
    };
}
