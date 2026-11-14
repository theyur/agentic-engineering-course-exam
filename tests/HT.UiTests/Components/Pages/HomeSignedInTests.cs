using Bunit;
using Bunit.TestDoubles;
using HT.Ui.Components.Pages;
using HT.UnitTests.Fakes;
using static HT.UnitTests.Fakes.HabitSamples;

namespace HT.UiTests.Components.Pages;

/// <summary>
/// Slice 1 US6-AS10, HP-013, HP-015, amended by slice 2 HP-013 (US5-AS3): the signed-in Home shows the heading
/// "Today"; a registered user also gets the EmptyState that points to the catalog, an administrator nothing else.
/// </summary>
public sealed class HomeSignedInTests : BunitContext
{
    private readonly BunitAuthorizationContext _authorization;

    public HomeSignedInTests()
    {
        _authorization = AddAuthorization();
        _authorization.SetAuthorized("ana@example.com");
        var store = this.AddCatalog();
        store.Add(Catalog("Hs Reading"));
    }

    [Theory]
    [InlineData("RegisteredUser")]
    [InlineData("Administrator")]
    public void US6_AS10_a_signed_in_user_sees_the_heading_today(string role)
    {
        _authorization.SetRoles(role);

        var cut = Render<Home>();

        Assert.Equal("Today", Assert.Single(cut.FindAll("h1")).TextContent);
        Assert.Empty(cut.FindAll(".ht-hero, .ht-habit"));
        Assert.DoesNotContain("Popular in the catalog", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void HP_013_a_registered_user_gets_an_empty_state_under_today_that_points_to_the_catalog()
    {
        _authorization.SetRoles("RegisteredUser");

        var cut = Render<Home>();

        var container = cut.Find(".ht-container");
        Assert.Equal(["ht-page-head", "ht-card ht-empty"], container.Children.Select(c => c.ClassName));
        var empty = cut.Find(".ht-empty");
        Assert.Single(empty.QuerySelectorAll(".ht-empty__icon svg.ht-icon"));
        Assert.Equal("No goals yet", empty.QuerySelector("h3")!.TextContent);
        Assert.Equal(
            "Goals come in a later version. Until then, browse the catalog for ideas or keep custom habits of your own.",
            empty.QuerySelector("p")!.TextContent);
        var button = Assert.Single(empty.QuerySelectorAll("a, button"));
        Assert.Equal("Browse the catalog", button.TextContent);
        Assert.Equal("/catalog", button.GetAttribute("href"));
        Assert.Contains("ht-btn--primary", button.ClassList);
        Assert.DoesNotContain("ana", cut.Markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotMatch(@"\d", container.TextContent);
    }

    [Fact]
    public void HP_013_an_administrators_home_has_nothing_but_its_heading()
    {
        _authorization.SetRoles("Administrator");

        var cut = Render<Home>();

        Assert.Equal("Today", cut.Find(".ht-container").TextContent.Trim());
        Assert.Empty(cut.FindAll("h2, h3, p, ul, ol, time, table, .ht-card, .ht-stat, .ht-stats, .ht-hero, .ht-empty"));
        Assert.DoesNotContain("ana", cut.Markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Good ", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Welcome", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotMatch(@"\d", cut.Find(".ht-container").TextContent);
    }

    [Fact]
    public void HP_015_a_registered_users_home_has_no_greeting_name_or_date()
    {
        _authorization.SetRoles("RegisteredUser");

        var cut = Render<Home>();

        Assert.Empty(cut.FindAll("h2, ul, ol, time, table, .ht-stat, .ht-stats, .ht-hero"));
        Assert.DoesNotContain("Good ", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Welcome", cut.Markup, StringComparison.Ordinal);
    }
}
