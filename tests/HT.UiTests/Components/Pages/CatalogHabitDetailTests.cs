using Bunit;
using Bunit.TestDoubles;
using HT.Domain.Habits;
using HT.Ui.Components.Pages;
using HT.UnitTests.Fakes;
using static HT.UnitTests.Fakes.HabitSamples;

namespace HT.UiTests.Components.Pages;

/// <summary>US2-AS6, US2-AS7, HP-005: the slice-2 state of the detail page.</summary>
public sealed class CatalogHabitDetailTests : BunitContext
{
    private readonly BunitAuthorizationContext _authorization;
    private readonly FakeCatalogHabitStore _store;

    public CatalogHabitDetailTests()
    {
        _authorization = AddAuthorization();
        _store = this.AddCatalog();
    }

    private IRenderedComponent<CatalogHabitDetail> RenderHabit(CatalogHabit habit) =>
        Render<CatalogHabitDetail>(p => p.Add(x => x.Id, habit.Id.Value));

    private CatalogHabit Reading() => _store.Add(Catalog("Reading", category: Category.Mind, icon: "book-open", description: "Books and articles.",
        goals: Goals(1, DurationDays(15, DurationUnit.Minutes, Weekdays), Count(20, "pages", 3, SchedulePeriod.Week))));

    [Fact]
    public void US2_AS6_a_visitor_sees_the_shipped_parts_and_the_register_button()
    {
        var cut = RenderHabit(Reading());

        var back = cut.Find("a.ht-back");
        Assert.Equal("← Catalog", back.TextContent);
        Assert.Equal("/catalog", back.GetAttribute("href"));
        Assert.NotNull(cut.Find(".ht-glyph-lg svg"));
        Assert.Equal("Reading", cut.Find("h1").TextContent);
        Assert.Equal(["Mind", "Build"], cut.FindAll(".ht-page-head .ht-chip").Select(c => c.TextContent));
        Assert.Equal("Books and articles.", cut.Find(".ht-lead").TextContent);
        Assert.Equal("Suggested starting goals", cut.Find(".ht-card__title").TextContent);
        Assert.Equal("Examples to adjust, not prescribed targets.", cut.Find(".ht-card__sub").TextContent);
        Assert.Equal(["15 min, 5 days a week", "20 pages, 3 times a week"], cut.FindAll(".ht-row__name").Select(n => n.TextContent));
        Assert.Equal(["Duration · fixed days Mon–Fri", "Count · flexible"], cut.FindAll(".ht-row__meta").Select(n => n.TextContent));
        var button = cut.Find(".ht-page-head a.ht-btn");
        Assert.Equal("Register to use this habit", button.TextContent);
        Assert.Equal("/register", button.GetAttribute("href"));
        AssertNoLaterSlices(cut);
    }

    [Theory]
    [InlineData("RegisteredUser")]
    [InlineData("Administrator")]
    public void US2_AS7_a_signed_in_user_sees_the_same_content_without_the_head_button(string role)
    {
        _authorization.SignIn(role);

        var cut = RenderHabit(Reading());

        Assert.Equal("Reading", cut.Find("h1").TextContent);
        Assert.Equal(2, cut.FindAll(".ht-row").Count);
        Assert.Empty(cut.FindAll(".ht-page-head .ht-btn"));
        AssertNoLaterSlices(cut);
    }

    [Fact]
    public void A_habit_without_suggested_starting_goals_has_no_such_card()
    {
        var cut = RenderHabit(_store.Add(Catalog("Walking", category: Category.Body, icon: "footprints")));

        Assert.Equal("Walking", cut.Find("h1").TextContent);
        Assert.Empty(cut.FindAll(".ht-card"));
        Assert.Empty(cut.FindAll(".ht-lead"));
    }

    private static void AssertNoLaterSlices(IRenderedComponent<CatalogHabitDetail> cut)
    {
        foreach (var absent in new[] { "Your goals", "Start with this", "Use this habit", "You track this", "Community", "contributors" })
        {
            Assert.DoesNotContain(absent, cut.Markup, StringComparison.Ordinal);
        }
    }
}
