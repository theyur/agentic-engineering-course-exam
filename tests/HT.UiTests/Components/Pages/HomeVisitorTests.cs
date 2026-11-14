using Bunit;
using Bunit.TestDoubles;
using HT.Domain.Habits;
using HT.Ui.Components.Pages;
using HT.UnitTests.Fakes;
using static HT.UnitTests.Fakes.HabitSamples;

namespace HT.UiTests.Components.Pages;

/// <summary>
/// Slice 1 US5-AS8, AS9, HP-011, amended by slice 2 HP-012 (US5-AS2): the visitor Home shows the hero with
/// "Register free" and "Browse the catalog", "How it works" and "Popular in the catalog", and nothing else.
/// </summary>
public sealed class HomeVisitorTests : BunitContext
{
    private readonly BunitAuthorizationContext _authorization;
    private readonly FakeCatalogHabitStore _store;

    public HomeVisitorTests()
    {
        _authorization = AddAuthorization();
        _store = this.AddCatalog();
    }

    [Fact]
    public void US5_AS8_the_hero_has_the_headline_its_sentence_register_free_and_the_privacy_line()
    {
        var cut = Render<Home>();

        Assert.Equal("Keep the habits you choose", cut.Find(".ht-hero h1").TextContent);
        Assert.Equal(
            "Set a goal to build or reduce a habit, record what really happened, and see how consistent you are over time.",
            cut.Find(".ht-hero__text").TextContent);
        Assert.Equal("Your goals and notes stay private. Community comparison is opt-in.", cut.Find(".ht-hero__fine").TextContent);
    }

    [Fact]
    public void HP_012_the_hero_buttons_are_register_free_and_browse_the_catalog()
    {
        var cut = Render<Home>();

        var buttons = cut.FindAll(".ht-hero__cta a, .ht-hero__cta button");
        Assert.Equal(["Register free", "Browse the catalog"], buttons.Select(b => b.TextContent));
        Assert.Equal(["/register", "/catalog"], buttons.Select(b => b.GetAttribute("href")));
        Assert.Contains("ht-btn--primary", buttons[0].ClassList);
        Assert.Contains("ht-btn--secondary", buttons[1].ClassList);
    }

    [Fact]
    public void US5_AS8_the_three_how_it_works_steps_follow_the_hero()
    {
        var cut = Render<Home>();

        Assert.Equal("How it works", Assert.Single(cut.FindAll("h2")).TextContent);
        var steps = cut.FindAll(".ht-step");
        Assert.Equal(["1", "2", "3"], steps.Select(s => s.QuerySelector(".ht-step__n")!.TextContent));
        Assert.Equal(["Pick a habit", "Record what happened", "See your consistency"], steps.Select(s => s.QuerySelector("h3")!.TextContent));
        Assert.Equal(
            [
                "Start from the catalog or write your own. Choose build or reduce.",
                "Yes/no, a count or minutes. Partial progress is kept. You have 48 hours to add late records.",
                "Streaks and a clear percentage, like 90% (18 of 20).",
            ],
            steps.Select(s => s.QuerySelector("p")!.TextContent));
    }

    [Theory]
    [InlineData("Example goal")]
    [InlineData("Popular in the catalog")]
    [InlineData("Community")]
    [InlineData("participants")]
    [InlineData("Consistency Bands")]
    public void US5_AS8_HP_012_with_no_Published_habit_nothing_else_from_the_preview_is_shown(string absent)
    {
        _store.Add(Catalog("Hv Draft", CatalogHabitState.Draft));
        _store.Add(Catalog("Hv Unpublished", CatalogHabitState.Unpublished));

        var cut = Render<Home>();

        Assert.DoesNotContain(absent, cut.Markup.Replace("Community comparison is opt-in.", string.Empty, StringComparison.Ordinal), StringComparison.Ordinal);
        Assert.Empty(cut.FindAll(".ht-card, .ht-habit, .ht-bands, .ht-stats, .ht-strip"));
    }

    [Fact]
    public void HP_012_popular_in_the_catalog_follows_how_it_works_with_four_visitor_cards_and_see_all()
    {
        _store.Add(Catalog("Hv Yoga", category: Category.Body, icon: "dumbbell"));
        var reading = _store.Add(Catalog("Hv Reading", description: "Books and articles.",
            goals: Goals(0, Count(20, "pages", 3, SchedulePeriod.Week))));
        _store.Add(Catalog("Hv Sweets", category: Category.Health, intent: Intent.Reduce, icon: "candy"));
        _store.Add(Catalog("Hv Journaling"));
        _store.Add(Catalog("Hv Walking"));
        _store.Add(Catalog("Hv Breathing", CatalogHabitState.Draft));

        var cut = Render<Home>();

        Assert.Equal(["How it works", "Popular in the catalog"], cut.FindAll("h2").Select(h => h.TextContent));
        var seeAll = cut.Find(".ht-section__head a");
        Assert.Equal("See all 5 →", seeAll.TextContent);
        Assert.Equal("/catalog", seeAll.GetAttribute("href"));
        var cards = cut.FindAll(".ht-grid-4 .ht-habit");
        Assert.Equal(["Hv Journaling", "Hv Reading", "Hv Sweets", "Hv Walking"], cards.Select(c => c.QuerySelector(".ht-habit__name")!.TextContent));
        var card = cards[1];
        Assert.Equal(["Mind", "Build"], card.QuerySelectorAll(".ht-habit__tags .ht-chip").Select(t => t.TextContent));
        Assert.Equal("Suggested starting goal: 20 pages, 3 times a week", card.QuerySelector(".ht-habit__start")!.TextContent);
        var details = Assert.Single(card.QuerySelectorAll(".ht-habit__actions a"));
        Assert.Equal("See details", details.TextContent);
        Assert.Equal($"/catalog/{reading.Id.Value}", details.GetAttribute("href"));
        Assert.Null(cards[0].QuerySelector(".ht-habit__start"));
    }

    [Fact]
    public void HP_012_fewer_than_four_Published_habits_are_all_shown()
    {
        _store.Add(Catalog("Hv Reading"));
        _store.Add(Catalog("Hv Yoga"));

        var cut = Render<Home>();

        Assert.Equal(2, cut.FindAll(".ht-grid-4 .ht-habit").Count);
        Assert.Equal("See all 2 →", cut.Find(".ht-section__head a").TextContent);
    }

    [Fact]
    public void HP_009_a_signed_in_user_does_not_get_the_visitor_home()
    {
        _store.Add(Catalog("Hv Reading"));
        _authorization.SetAuthorized("ana@example.com");

        var cut = Render<Home>();

        Assert.Empty(cut.FindAll(".ht-hero"));
        Assert.DoesNotContain("Register free", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Popular in the catalog", cut.Markup, StringComparison.Ordinal);
    }
}
