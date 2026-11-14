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

/// <summary>The outcomes of <see cref="CatalogService.GetHomePreviewAsync"/>.</summary>
public enum HomePreviewOutcome
{
    Found,
}

/// <summary>
/// The Home preview (FR-008): the first four habits of browsing with no filters, fewer when fewer exist, and the
/// number of all Published habits.
/// </summary>
public sealed class HomePreviewResult
{
    /// <summary>How many habits the preview holds at most (FR-008).</summary>
    public const int Size = 4;

    private HomePreviewResult(IReadOnlyList<CatalogHabitSummary> habits, int publishedCount)
    {
        Habits = habits;
        PublishedCount = publishedCount;
    }

    public HomePreviewOutcome Outcome => HomePreviewOutcome.Found;

    /// <summary>Always null: the preview has no refusal and the spec gives its success no text.</summary>
    public string? Message => null;

    public IReadOnlyList<CatalogHabitSummary> Habits { get; }

    /// <summary>All Published catalog habits, for "See all N →".</summary>
    public int PublishedCount { get; }

    internal static HomePreviewResult Found(IReadOnlyList<CatalogHabitSummary> habits, int publishedCount) =>
        new(habits, publishedCount);
}

/// <summary>The outcomes of <see cref="CatalogService.GetDetailsAsync"/>.</summary>
public enum CatalogHabitDetailsOutcome
{
    NotInCatalog,
    Found,
}

/// <summary>
/// The result of reading a catalog habit's details (FR-010): the details of a Published habit, or "This habit is
/// not in the catalog." for anything else, the same for every audience.
/// </summary>
public sealed class CatalogHabitDetailsResult
{
    private CatalogHabitDetailsResult(CatalogHabitDetailsOutcome outcome, string? message, CatalogHabitDetails? details)
    {
        Outcome = outcome;
        Message = message;
        Details = details;
    }

    public CatalogHabitDetailsOutcome Outcome { get; }

    /// <summary>The spec's text for <see cref="CatalogHabitDetailsOutcome.NotInCatalog"/>; null for Found.</summary>
    public string? Message { get; }

    /// <summary>Set only for <see cref="CatalogHabitDetailsOutcome.Found"/>.</summary>
    public CatalogHabitDetails? Details { get; }

    internal static CatalogHabitDetailsResult NotInCatalog { get; } =
        new(CatalogHabitDetailsOutcome.NotInCatalog, CatalogTexts.NotInCatalog, null);

    internal static CatalogHabitDetailsResult Found(CatalogHabitDetails details) =>
        new(CatalogHabitDetailsOutcome.Found, null, details);
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
