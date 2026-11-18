namespace HT.Application.Habits;

/// <summary>The outcomes of both <c>BrowseAsync</c> overloads of <see cref="CatalogService"/>.</summary>
public enum CatalogBrowseOutcome
{
    Found,

    /// <summary>"Only habits I track" asked by an anonymous visitor (FR-032); only the acting-user overload gives it.</summary>
    NotSignedIn,

    /// <summary>"Only habits I track" asked by an administrator (FR-021, FR-032); only the acting-user overload gives it.</summary>
    NotForAdministrators,
}

/// <summary>
/// The result of browsing (FR-008, FR-009): the Published habits in name order, and their count. With the acting user
/// passed, a registered user also gets <see cref="TrackedCount"/> and each habit's goal count (FR-031); "Only habits I
/// track" is refused for the other audiences with no habit (FR-032).
/// </summary>
public sealed class CatalogBrowseResult
{
    private CatalogBrowseResult(CatalogBrowseOutcome outcome, string? message, IReadOnlyList<CatalogHabitSummary> habits)
    {
        Outcome = outcome;
        Message = message;
        Habits = habits;
    }

    public CatalogBrowseOutcome Outcome { get; }

    /// <summary>The spec's text of a refusal; null for <see cref="CatalogBrowseOutcome.Found"/>.</summary>
    public string? Message { get; }

    /// <summary>The habits; empty for a refusal.</summary>
    public IReadOnlyList<CatalogHabitSummary> Habits { get; }

    public int Count => Habits.Count;

    /// <summary>
    /// How many of the returned habits the acting user tracks (a goal count of at least 1). Set only for a registered
    /// user with the acting user passed; null otherwise (FR-019, FR-032).
    /// </summary>
    public int? TrackedCount { get; private init; }

    internal static CatalogBrowseResult Found(IReadOnlyList<CatalogHabitSummary> habits) =>
        new(CatalogBrowseOutcome.Found, null, habits);

    internal static CatalogBrowseResult FoundWithGoals(IReadOnlyList<CatalogHabitSummary> habits) =>
        new(CatalogBrowseOutcome.Found, null, habits) { TrackedCount = habits.Count(h => h.GoalCount >= 1) };

    internal static CatalogBrowseResult Refused(CatalogBrowseOutcome outcome, string message) => new(outcome, message, []);
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

/// <summary>The outcomes of both <c>GetDetailsAsync</c> overloads of <see cref="CatalogService"/>.</summary>
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
