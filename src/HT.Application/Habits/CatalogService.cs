using HT.Domain.Habits;

namespace HT.Application.Habits;

/// <summary>
/// The public catalog for every audience (FR-008 to FR-010). The use cases take no acting user: every audience
/// gets the same result (ADR 0005, research R-07). Filtering and ordering run here, over the Published habits the
/// store returns, with the one name key of <see cref="HabitName"/> (research R-03).
/// </summary>
public sealed class CatalogService
{
    private readonly ICatalogHabitStore _store;

    public CatalogService(ICatalogHabitStore store)
    {
        ArgumentNullException.ThrowIfNull(store);
        _store = store;
    }

    /// <summary>
    /// The Published catalog habits that match every given filter — category, intent and the name search — in
    /// name order, each with its marked suggested starting goal (FR-008, FR-009).
    /// </summary>
    public async Task<CatalogBrowseResult> BrowseAsync(CatalogFilter filter, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);
        var habits = await _store.ListPublishedAsync(cancellationToken).ConfigureAwait(false);
        var matching = habits
            .Where(h => filter.Category is not { } category || h.Definition.Category == category)
            .Where(h => filter.Intent is not { } intent || h.Definition.Intent == intent)
            .Where(h => h.Definition.Name.Matches(filter.Search))
            .OrderBy(h => h.Definition.Name, HabitName.Order)
            .Select(CatalogViews.Summary)
            .ToList();
        return CatalogBrowseResult.Found(matching);
    }

    /// <summary>"All", then each category that has at least one Published habit, in FR-002 order (FR-009).</summary>
    public async Task<CategoryFilterResult> GetCategoryFilterAsync(CancellationToken cancellationToken)
    {
        var habits = await _store.ListPublishedAsync(cancellationToken).ConfigureAwait(false);
        var used = habits.Select(h => h.Definition.Category).ToHashSet();
        var options = new List<CategoryFilterOption> { new(null, CatalogTexts.AllCategories) };
        options.AddRange(Enum.GetValues<Category>().Where(used.Contains).Select(c => new CategoryFilterOption(c, c.ToString())));
        return CategoryFilterResult.Found(options);
    }
}
