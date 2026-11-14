using Bunit;
using Bunit.TestDoubles;
using HT.Domain.Habits;
using HT.Ui.Components.Pages;
using HT.UnitTests.Fakes;
using static HT.UnitTests.Fakes.HabitSamples;

namespace HT.UiTests.Components.Pages;

/// <summary>US3-AS20, HP-009: the slice-2 Admin page over the real service and the fake store.</summary>
public sealed class AdminPageTests : BunitContext
{
    private readonly FakeCatalogHabitStore _store;

    public AdminPageTests()
    {
        var authorization = AddAuthorization();
        authorization.SignIn("Administrator", "admin@example.com");
        _store = this.AddCatalog();
        _store.Add(Catalog("Cold shower", CatalogHabitState.Draft, Category.Body, icon: "droplet"));
        _store.Add(Catalog("Reading", CatalogHabitState.Published, Category.Mind));
        _store.Add(Catalog("Sugar", CatalogHabitState.Unpublished, Category.Health, Intent.Reduce, "candy"));
    }

    private static IReadOnlyList<string> Cells(AngleSharp.Dom.IElement row) => row.QuerySelectorAll("td").Select(td => td.TextContent.Trim()).ToList();

    [Fact]
    public void US3_AS20_the_Admin_page_follows_the_slice_2_preview()
    {
        var cut = Render<Admin>();

        Assert.Equal("Administration", cut.Find("h1").TextContent);
        var create = cut.Find(".ht-page-head a.ht-btn");
        Assert.Equal("+ New catalog habit", create.TextContent);
        Assert.Equal("/admin/catalog/new", create.GetAttribute("href"));
        Assert.Equal("Privacy: admins manage public content and accounts. Private goals, records, notes and personal reports are not available here.",
            cut.Find(".ht-notice--neutral span").TextContent);
        Assert.Equal("Search catalog", cut.Find("input[type=search]").GetAttribute("aria-label"));
        Assert.Contains("Showing 3 of 3", cut.Find(".ht-results-bar").TextContent, StringComparison.Ordinal);
        Assert.Equal(["Habit", "Category", "Intent", "Status", ""], cut.FindAll("th").Select(th => th.TextContent));
        Assert.Empty(cut.FindAll(".ht-tabs"));
        Assert.DoesNotContain("Updated", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Suggestions", cut.Markup, StringComparison.Ordinal);

        var rows = cut.FindAll("tbody tr");
        Assert.Equal(["Cold shower", "Reading", "Sugar"], rows.Select(r => r.QuerySelector("td b")!.TextContent));
        Assert.Equal(["Body", "Build", "Draft"], Cells(rows[0]).Skip(1).Take(3));
        Assert.Equal(["Edit", "Publish"], rows[0].QuerySelectorAll(".ht-actions .ht-btn").Select(b => b.TextContent));
        Assert.Equal(["Edit", "Unpublish"], rows[1].QuerySelectorAll(".ht-actions .ht-btn").Select(b => b.TextContent));
        Assert.Contains("ht-chip--strong", rows[1].QuerySelector(".ht-chip")!.ClassList);
        Assert.DoesNotContain("ht-chip--strong", rows[2].QuerySelector(".ht-chip")!.ClassList);
        Assert.Equal("Edit Cold shower", rows[0].QuerySelector(".ht-actions a")!.GetAttribute("aria-label"));
    }

    [Fact]
    public void Publish_and_Unpublish_show_the_new_state_at_once()
    {
        var cut = Render<Admin>();

        cut.FindAll("tbody tr")[0].QuerySelector("button")!.Click();
        Assert.Equal("Published", cut.FindAll("tbody tr")[0].QuerySelector(".ht-chip")!.TextContent);
        Assert.Equal("Unpublish", cut.FindAll("tbody tr")[0].QuerySelector("button")!.TextContent);

        cut.FindAll("tbody tr")[1].QuerySelector("button")!.Click();
        Assert.Equal("Unpublished", cut.FindAll("tbody tr")[1].QuerySelector(".ht-chip")!.TextContent);
        Assert.Equal("Publish", cut.FindAll("tbody tr")[1].QuerySelector("button")!.TextContent);
        Assert.Empty(cut.FindAll("[role=dialog]"));
    }

    [Fact]
    public void The_search_lists_the_matching_habits_of_all()
    {
        var cut = Render<Admin>();

        cut.Find("input[type=search]").Input("read");

        cut.WaitForAssertion(() => Assert.Equal(["Reading"], cut.FindAll("tbody tr td b").Select(b => b.TextContent)), TimeSpan.FromSeconds(3));
        Assert.Contains("Showing 1 of 3", cut.Find(".ht-results-bar").TextContent, StringComparison.Ordinal);
    }
}
