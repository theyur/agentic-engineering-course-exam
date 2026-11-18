using HT.Application.Accounts;
using HT.Application.Goals;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;

namespace HT.Application.Habits;

/// <summary>
/// The public catalog for every audience (FR-008 to FR-010). The slice 2 use cases take no acting user: every audience
/// gets the same result (ADR 0005, research R-07). Filtering and ordering run here, over the Published habits the
/// store returns, with the one name key of <see cref="HabitName"/> (research R-03).
/// <para>
/// Slice 4 (FR-019, FR-031, FR-032; research R-09) adds overloads that take the acting user first. They give a
/// registered user their own goal counts and goals, read from <see cref="IGoalStore"/> with the acting user's account
/// as the owner, so another account's goals are never read (constitution III). Every other audience gets the slice 2
/// content with no goal information, and "Only habits I track" is refused for them.
/// </para>
/// </summary>
public sealed class CatalogService
{
    private readonly ICatalogHabitStore _store;
    private readonly IGoalStore _goals;

    public CatalogService(ICatalogHabitStore store, IGoalStore goals)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(goals);
        _store = store;
        _goals = goals;
    }

    /// <summary>
    /// The Published catalog habits that match every given filter — category, intent and the name search — in
    /// name order, each with its marked suggested starting goal (FR-008, FR-009). No goal information (FR-019).
    /// </summary>
    public async Task<CatalogBrowseResult> BrowseAsync(CatalogFilter filter, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);
        return CatalogBrowseResult.Found(await MatchingAsync(filter, cancellationToken).ConfigureAwait(false));
    }

    /// <summary>
    /// Browsing as <paramref name="actingUser"/> (FR-031, FR-032; research R-09), checking in FR-023's order:
    /// <list type="number">
    ///   <item>with <paramref name="onlyTracked"/>, an anonymous visitor gets "You are not signed in." and an administrator "Goals are for registered users.";</item>
    ///   <item>slice 2's filters and order, as <see cref="BrowseAsync(CatalogFilter, CancellationToken)"/> applies them;</item>
    ///   <item>for a registered user, each habit's goal count of the user's own goals, only the habits with at least one
    ///   with <paramref name="onlyTracked"/>, and <see cref="CatalogBrowseResult.TrackedCount"/>.</item>
    /// </list>
    /// Without <paramref name="onlyTracked"/>, a visitor and an administrator get slice 2's content with no goal information.
    /// </summary>
    public async Task<CatalogBrowseResult> BrowseAsync(
        ActingUser actingUser, CatalogFilter filter, bool onlyTracked, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actingUser);
        ArgumentNullException.ThrowIfNull(filter);
        var owner = RegisteredOwner(actingUser);
        if (onlyTracked && owner is null)
        {
            return actingUser.IsAnonymous
                ? CatalogBrowseResult.Refused(CatalogBrowseOutcome.NotSignedIn, AccountTexts.NotSignedIn)
                : CatalogBrowseResult.Refused(CatalogBrowseOutcome.NotForAdministrators, GoalTexts.NotForAdministrators);
        }

        var matching = await MatchingAsync(filter, cancellationToken).ConfigureAwait(false);
        if (owner is not { } account)
        {
            return CatalogBrowseResult.Found(matching);
        }

        var counts = await _goals.CountByCatalogHabitAsync(account, cancellationToken).ConfigureAwait(false);
        var withCounts = matching
            .Select(habit => habit with { GoalCount = counts.GetValueOrDefault(habit.Id) })
            .Where(habit => !onlyTracked || habit.GoalCount >= 1)
            .ToList();
        return CatalogBrowseResult.FoundWithGoals(withCounts);
    }

    /// <summary>
    /// The Home preview (FR-008): the first <see cref="HomePreviewResult.Size"/> habits of browsing with no filters,
    /// fewer when fewer exist, and the number of all Published habits for "See all N →".
    /// </summary>
    public async Task<HomePreviewResult> GetHomePreviewAsync(CancellationToken cancellationToken)
    {
        var all = await BrowseAsync(new CatalogFilter(), cancellationToken).ConfigureAwait(false);
        return HomePreviewResult.Found([.. all.Habits.Take(HomePreviewResult.Size)], all.Count);
    }

    /// <summary>
    /// A Published catalog habit's details with all its suggested starting goals in order (FR-010). A Draft, an
    /// Unpublished habit, a custom habit's identifier and an unknown identifier are all "not in the catalog". No goal
    /// information (FR-019).
    /// </summary>
    public async Task<CatalogHabitDetailsResult> GetDetailsAsync(HabitId id, CancellationToken cancellationToken)
    {
        var habit = await _store.FindPublishedAsync(id, cancellationToken).ConfigureAwait(false);
        return habit is null ? CatalogHabitDetailsResult.NotInCatalog : CatalogHabitDetailsResult.Found(CatalogViews.Details(habit));
    }

    /// <summary>
    /// A Published catalog habit's details as <paramref name="actingUser"/> reads them (FR-031, FR-032): the result of
    /// <see cref="GetDetailsAsync(HabitId, CancellationToken)"/>, and for a registered user also their own goal count
    /// and their goals on the habit, each with its identifier and its derived name (FR-010), in name order (the keys of
    /// <see cref="HabitName.KeyOf"/> compared ordinally), then creation order. Every other audience gets no goal
    /// information.
    /// </summary>
    public async Task<CatalogHabitDetailsResult> GetDetailsAsync(ActingUser actingUser, HabitId id, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actingUser);
        var result = await GetDetailsAsync(id, cancellationToken).ConfigureAwait(false);
        if (result.Details is not { } details || RegisteredOwner(actingUser) is not { } owner)
        {
            return result;
        }

        var stored = await _goals.ListOnCatalogHabitAsync(owner, id, cancellationToken).ConfigureAwait(false);
        var goals = stored
            .Select(goal => new GoalOnHabit(goal.Goal.Id, GoalText.Name(goal.Goal, goal.Habit.Name)))
            .OrderBy(goal => HabitName.KeyOf(goal.Name), StringComparer.Ordinal)
            .ToList();
        return CatalogHabitDetailsResult.Found(details with { GoalCount = goals.Count, Goals = goals });
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

    /// <summary>Slice 2's browsing (FR-008, FR-009): the Published habits matching every filter, in name order.</summary>
    private async Task<IReadOnlyList<CatalogHabitSummary>> MatchingAsync(CatalogFilter filter, CancellationToken cancellationToken)
    {
        var habits = await _store.ListPublishedAsync(cancellationToken).ConfigureAwait(false);
        return habits
            .Where(h => filter.Category is not { } category || h.Definition.Category == category)
            .Where(h => filter.Intent is not { } intent || h.Definition.Intent == intent)
            .Where(h => h.Definition.Name.Matches(filter.Search))
            .OrderBy(h => h.Definition.Name, HabitName.Order)
            .Select(CatalogViews.Summary)
            .ToList();
    }

    /// <summary>The account of a registered user, whose own goals the catalog shows; null for every other audience (FR-032).</summary>
    private static AccountId? RegisteredOwner(ActingUser actingUser) =>
        actingUser.AccountId is { } owner && actingUser.Role == Role.RegisteredUser ? owner : null;
}
