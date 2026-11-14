using HT.Application.Habits;
using HT.Domain.Habits;
using HT.UnitTests.Fakes;
using static HT.UnitTests.Fakes.HabitSamples;

namespace HT.UnitTests.Application.Habits;

/// <summary>US1: browsing returns only Published habits, in name order, filtered (FR-008, FR-009).</summary>
public sealed class CatalogBrowseTests
{
    private readonly FakeCatalogHabitStore _store = new();
    private CatalogService Service => new(_store);

    private async Task<IReadOnlyList<string>> NamesAsync(CatalogFilter? filter = null) =>
        (await Service.BrowseAsync(filter ?? new CatalogFilter(), CancellationToken.None)).Habits.Select(h => h.Name).ToList();

    [Fact]
    public async Task US1_AS1_only_Published_habits_with_their_fields_and_marked_suggested_starting_goal()
    {
        var reading = _store.Add(Catalog("Ac1 Reading", category: Category.Mind, icon: "book-open", description: "Books and articles.",
            goals: Goals(1, YesNo(1, SchedulePeriod.Day), Count(20, "pages", 3, SchedulePeriod.Week))));
        _store.Add(Catalog("Ac1 Yoga", category: Category.Body, icon: "person-standing"));
        _store.Add(Catalog("Ac1 Journaling", CatalogHabitState.Draft));
        _store.Add(Catalog("Ac1 Stretching", CatalogHabitState.Unpublished));

        var result = await Service.BrowseAsync(new CatalogFilter(), CancellationToken.None);

        Assert.Equal(CatalogBrowseOutcome.Found, result.Outcome);
        Assert.Null(result.Message);
        Assert.Equal(["Ac1 Reading", "Ac1 Yoga"], result.Habits.Select(h => h.Name));
        Assert.Equal(2, result.Count);
        var summary = result.Habits[0];
        Assert.Equal(reading.Id, summary.Id);
        Assert.Equal(Category.Mind, summary.Category);
        Assert.Equal(Intent.Build, summary.Intent);
        Assert.Equal("book-open", summary.Icon);
        Assert.Equal("Books and articles.", summary.Description);
        var marked = summary.MarkedSuggestedStartingGoal;
        Assert.NotNull(marked);
        Assert.Equal(2, marked.Position);
        Assert.True(marked.IsMarked);
        Assert.Equal(Measurement.Count, marked.Measurement);
        Assert.Equal(20, marked.Amount);
        Assert.Equal("pages", marked.CountUnit);
        Assert.Equal("20 pages, 3 times a week", marked.Text);
        Assert.Equal("Count · flexible", marked.Kind);
    }

    [Fact]
    public async Task US1_AS2_name_order_ignoring_case_and_ordinal()
    {
        _store.Add(Catalog("Ac2 walking"));
        _store.Add(Catalog("Ac2 Breathing"));
        _store.Add(Catalog("Zest"));
        _store.Add(Catalog("Éclair"));
        _store.Add(Catalog("Ac2 Cycling"));

        Assert.Equal(["Ac2 Breathing", "Ac2 Cycling", "Ac2 walking", "Zest", "Éclair"], await NamesAsync());
    }

    [Fact]
    public async Task US1_AS3_category_filter()
    {
        _store.Add(Catalog("Ac3 Saving", category: Category.Money));
        _store.Add(Catalog("Ac3 Running", category: Category.Body));

        Assert.Equal(["Ac3 Saving"], await NamesAsync(new CatalogFilter { Category = Category.Money }));
    }

    [Fact]
    public async Task US1_AS4_intent_filter()
    {
        _store.Add(Catalog("Ac4 Sugar", category: Category.Health, intent: Intent.Reduce));
        _store.Add(Catalog("Ac4 Water", category: Category.Health, intent: Intent.Build));

        Assert.Equal(["Ac4 Sugar"], await NamesAsync(new CatalogFilter { Intent = Intent.Reduce }));
    }

    [Fact]
    public async Task US1_AS5_search_is_trimmed_ignores_case_and_matches_part_of_the_name()
    {
        _store.Add(Catalog("Ac5 Morning pages"));
        _store.Add(Catalog("Ac5 Evening walk"));

        Assert.Equal(["Ac5 Morning pages"], await NamesAsync(new CatalogFilter { Search = " morning " }));
    }

    [Fact]
    public async Task All_three_filters_apply_together()
    {
        _store.Add(Catalog("Ac5 Coffee less", category: Category.Health, intent: Intent.Reduce));
        _store.Add(Catalog("Ac5 Coffee walk", category: Category.Health, intent: Intent.Build));
        _store.Add(Catalog("Ac5 Coffee budget", category: Category.Money, intent: Intent.Reduce));
        _store.Add(Catalog("Ac5 Sugar less", category: Category.Health, intent: Intent.Reduce));

        Assert.Equal(["Ac5 Coffee less"],
            await NamesAsync(new CatalogFilter { Category = Category.Health, Intent = Intent.Reduce, Search = "coffee" }));
    }

    [Fact]
    public async Task US1_AS7_no_match_is_found_with_an_empty_list_and_count_0()
    {
        _store.Add(Catalog("Ac7 Reading", category: Category.Mind));

        var result = await Service.BrowseAsync(new CatalogFilter { Category = Category.Money, Search = "zzz" }, CancellationToken.None);

        Assert.Equal(CatalogBrowseOutcome.Found, result.Outcome);
        Assert.Empty(result.Habits);
        Assert.Equal(0, result.Count);
    }

    [Fact]
    public async Task US2_AS2_a_habit_without_suggested_starting_goals_has_none_marked()
    {
        _store.Add(Catalog("Ac7b Walking"));

        var summary = Assert.Single((await Service.BrowseAsync(new CatalogFilter(), CancellationToken.None)).Habits);
        Assert.Null(summary.MarkedSuggestedStartingGoal);
    }

    [Fact]
    public async Task US1_AS8_the_category_filter_starts_with_All_and_keeps_FR_002_order()
    {
        _store.Add(Catalog("Ac26 Budget", category: Category.Money));
        _store.Add(Catalog("Ac26 Lifting", category: Category.Body));

        var result = await Service.GetCategoryFilterAsync(CancellationToken.None);

        Assert.Equal(CategoryFilterOutcome.Found, result.Outcome);
        Assert.Equal(["All", "Body", "Money"], result.Options.Select(o => o.Label));
        Assert.Equal([null, Category.Body, Category.Money], result.Options.Select(o => o.Category));
    }

    [Fact]
    public async Task US1_AS9_a_Drafts_category_is_absent_from_the_filter()
    {
        _store.Add(Catalog("Ac27 Lifting", category: Category.Body));
        _store.Add(Catalog("Ac27 Tidying", CatalogHabitState.Draft, category: Category.Home));
        _store.Add(Catalog("Ac27 Old", CatalogHabitState.Unpublished, category: Category.Other));

        Assert.Equal(["All", "Body"], (await Service.GetCategoryFilterAsync(CancellationToken.None)).Options.Select(o => o.Label));
    }

    [Fact]
    public async Task An_empty_catalog_offers_All_only()
    {
        Assert.Equal(["All"], (await Service.GetCategoryFilterAsync(CancellationToken.None)).Options.Select(o => o.Label));
    }
}
