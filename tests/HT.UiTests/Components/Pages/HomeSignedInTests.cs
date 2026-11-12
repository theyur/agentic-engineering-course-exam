using Bunit;
using Bunit.TestDoubles;
using HT.Ui.Components.Pages;

namespace HT.UiTests.Components.Pages;

/// <summary>US6-AS10, HP-013, HP-015: the signed-in Home shows only the heading "Today".</summary>
public sealed class HomeSignedInTests : BunitContext
{
    public HomeSignedInTests()
    {
        var authorization = AddAuthorization();
        authorization.SetAuthorized("ana@example.com");
        authorization.SetRoles("RegisteredUser");
    }

    [Fact]
    public void US6_AS10_a_signed_in_user_sees_the_heading_today()
    {
        var cut = Render<Home>();

        Assert.Equal("Today", Assert.Single(cut.FindAll("h1")).TextContent);
    }

    [Fact]
    public void US6_AS10_the_signed_in_home_has_nothing_but_its_heading()
    {
        var cut = Render<Home>();

        Assert.Equal("Today", cut.Find(".ht-container").TextContent.Trim());
        Assert.Empty(cut.FindAll("h2, h3, p, ul, ol, time, table, .ht-card, .ht-stat, .ht-stats, .ht-hero"));
        Assert.DoesNotContain("ana", cut.Markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Good ", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Welcome", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotMatch(@"\d", cut.Find(".ht-container").TextContent);
    }
}
