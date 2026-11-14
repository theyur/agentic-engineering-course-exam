using Bunit;
using Bunit.TestDoubles;
using HT.Domain.Accounts;
using HT.Domain.Habits;
using HT.Ui.Components.Pages;
using HT.UnitTests.Fakes;
using Microsoft.Extensions.DependencyInjection;
using static HT.UnitTests.Fakes.HabitSamples;

namespace HT.UiTests.Components.Pages;

/// <summary>US4-AS12, US4-AS15, HP-002: "My custom habits" over the real use cases and the fake stores.</summary>
public sealed class CustomHabitsPageTests : BunitContext
{
    private readonly BunitAuthorizationContext _authorization;
    private readonly FakeCustomHabitStore _store;
    private int _notFound;

    public CustomHabitsPageTests()
    {
        _authorization = AddAuthorization();
        var catalog = this.AddCatalog();
        catalog.Add(Catalog("Reading"));
        catalog.Add(Catalog("Yoga", category: Category.Body));
        catalog.Add(Catalog("Draft habit", CatalogHabitState.Draft));
        _store = this.AddCustomHabits();
        Services.GetRequiredService<BunitNavigationManager>().OnNotFound += (_, _) => _notFound++;
    }

    private IRenderedComponent<CustomHabits> RenderPage()
    {
        Services.GetRequiredService<BunitNavigationManager>().NavigateTo("/custom-habits");
        return Render<CustomHabits>();
    }

    private static List<(string Text, string? Href, string? Current)> Tabs(IRenderedComponent<CustomHabits> cut) =>
        cut.FindAll("nav.ht-tabs a.ht-tab").Select(a => (a.TextContent, a.GetAttribute("href"), a.GetAttribute("aria-current"))).ToList();

    [Fact]
    public void US4_AS12_the_users_custom_habits_are_cards_with_the_Private_Tag_and_Edit_in_name_order()
    {
        var owner = new AccountId(_authorization.SignIn("RegisteredUser"));
        var phone = _store.Add(owner, Definition("No phone in bed", Category.Home, Intent.Reduce, "smartphone", "Phone stays outside the bedroom."));
        var shower = _store.Add(owner, Definition("Cold shower", Category.Body, icon: "droplet"));
        _store.Add(new AccountId(Guid.NewGuid()), Definition("Someone else's habit"));

        var cut = RenderPage();

        Assert.Equal("Habit catalog", cut.Find("h1").TextContent);
        Assert.Empty(cut.FindAll("input[type=search]"));
        var create = cut.Find(".ht-page-head a.ht-btn");
        Assert.Equal("+ Create custom habit", create.TextContent);
        Assert.Equal("/custom-habits/new", create.GetAttribute("href"));
        Assert.Contains("ht-btn--secondary", create.ClassList);
        Assert.Equal("Catalog views", cut.Find("nav.ht-tabs").GetAttribute("aria-label"));
        Assert.Equal([("Public catalog2", "/catalog", null), ("My custom habits2", "/custom-habits", "page")], Tabs(cut));
        Assert.Equal(["2", "2"], cut.FindAll(".ht-tab .ht-count").Select(c => c.TextContent));

        var cards = cut.FindAll("article.ht-habit");
        Assert.Equal(["Cold shower", "No phone in bed"], cards.Select(c => c.QuerySelector(".ht-habit__name")!.TextContent));
        Assert.All(cards, c => Assert.Equal("Private", c.QuerySelector(".ht-chip--strong")!.TextContent));
        Assert.Equal(["Home", "Reduce", "Private"], cards[1].QuerySelectorAll(".ht-chip").Select(t => t.TextContent));
        Assert.Equal("Phone stays outside the bedroom.", cards[1].QuerySelector(".ht-habit__desc")!.TextContent);
        Assert.Empty(cut.FindAll(".ht-habit__start"));
        var edit = cards[0].QuerySelector(".ht-habit__actions a")!;
        Assert.Equal("Edit", edit.TextContent);
        Assert.Equal("Edit Cold shower", edit.GetAttribute("aria-label"));
        Assert.Equal($"/custom-habits/{shower.Id.Value}", edit.GetAttribute("href"));
        Assert.Equal($"/custom-habits/{phone.Id.Value}", cards[1].QuerySelector(".ht-habit__actions a")!.GetAttribute("href"));
        Assert.DoesNotContain("Someone else", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Suggest", cut.Markup, StringComparison.Ordinal);
        Assert.Equal(0, _notFound);
    }

    [Fact]
    public void Without_custom_habits_the_EmptyState_offers_to_create_one()
    {
        _authorization.SignIn("RegisteredUser");

        var cut = RenderPage();

        Assert.Empty(cut.FindAll("article.ht-habit"));
        Assert.Equal("No custom habits yet", cut.Find(".ht-empty h3").TextContent);
        Assert.Equal("Create a habit of your own when the catalog has nothing that fits. Only you can see it.", cut.Find(".ht-empty p").TextContent);
        var create = cut.Find(".ht-empty a.ht-btn");
        Assert.Equal("+ Create custom habit", create.TextContent);
        Assert.Equal("/custom-habits/new", create.GetAttribute("href"));
        Assert.Contains("ht-btn--primary", create.ClassList);
        Assert.Equal(["2", "0"], cut.FindAll(".ht-tab .ht-count").Select(c => c.TextContent));
    }

    [Fact]
    public void US4_AS15_an_administrator_gets_the_Not_found_page()
    {
        _store.Add(new AccountId(Guid.NewGuid()), Definition("Cold shower"));
        _authorization.SignIn("Administrator", "admin@example.com");

        var cut = RenderPage();

        Assert.Equal(1, _notFound);
        Assert.Empty(cut.FindAll(".ht-tabs"));
        Assert.DoesNotContain("Cold shower", cut.Markup, StringComparison.Ordinal);
    }
}
