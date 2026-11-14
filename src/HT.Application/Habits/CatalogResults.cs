namespace HT.Application.Habits;

/// <summary>The outcomes of <see cref="CatalogService.BrowseAsync"/>.</summary>
public enum CatalogBrowseOutcome
{
    Found,
}

/// <summary>The result of browsing (FR-008, FR-009): the Published habits in name order, and their count.</summary>
public sealed class CatalogBrowseResult
{
    private CatalogBrowseResult(IReadOnlyList<CatalogHabitSummary> habits)
    {
        Habits = habits;
    }

    public CatalogBrowseOutcome Outcome => CatalogBrowseOutcome.Found;

    /// <summary>Always null: browsing has no refusal and the spec gives its success no text.</summary>
    public string? Message => null;

    public IReadOnlyList<CatalogHabitSummary> Habits { get; }

    public int Count => Habits.Count;

    internal static CatalogBrowseResult Found(IReadOnlyList<CatalogHabitSummary> habits) => new(habits);
}

/// <summary>The outcomes of <see cref="CatalogService.GetCategoryFilterAsync"/>.</summary>
public enum CategoryFilterOutcome
{
    Found,
}

/// <summary>The category filter (FR-009): "All" first, then each category with a Published habit, in FR-002 order.</summary>
public sealed class CategoryFilterResult
{
    private CategoryFilterResult(IReadOnlyList<CategoryFilterOption> options) => Options = options;

    public CategoryFilterOutcome Outcome => CategoryFilterOutcome.Found;

    public string? Message => null;

    public IReadOnlyList<CategoryFilterOption> Options { get; }

    internal static CategoryFilterResult Found(IReadOnlyList<CategoryFilterOption> options) => new(options);
}
