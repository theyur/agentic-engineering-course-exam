using Bunit;
using HT.Ui.Components.Pages;

namespace HT.UiTests.Components.Pages;

/// <summary>US5-AS7, HP-012: the Privacy page states the three promises.</summary>
public sealed class PrivacyTests : BunitContext
{
    [Fact]
    public void HP_012_the_page_states_what_stays_private_opt_in_and_what_administrators_cannot_see()
    {
        var cut = Render<Privacy>();

        Assert.Equal("Privacy", cut.Find("h1").TextContent);
        var cards = cut.FindAll("section.ht-card").ToDictionary(c => c.QuerySelector("h2")!.TextContent, c => c.QuerySelector("p")!.TextContent);
        Assert.Equal(["What stays private", "Community comparison is opt-in", "What administrators can see"], cards.Keys);

        Assert.Contains("Your goals, records, notes and custom habits are private.", cards["What stays private"], StringComparison.Ordinal);
        Assert.Contains("Community comparison is off by default.", cards["Community comparison is opt-in"], StringComparison.Ordinal);
        Assert.Contains("you can turn it off at any time", cards["Community comparison is opt-in"], StringComparison.Ordinal);
        Assert.Contains("Custom habits never appear in community statistics.", cards["Community comparison is opt-in"], StringComparison.Ordinal);
        Assert.Contains("They cannot see your goals, records, notes or personal reports.", cards["What administrators can see"], StringComparison.Ordinal);
    }

    [Fact]
    public void HP_012_the_page_allows_anonymous_access()
    {
        Assert.NotNull(Attribute.GetCustomAttribute(typeof(Privacy), typeof(Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute)));
        Assert.Null(Attribute.GetCustomAttribute(typeof(Privacy), typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute)));
    }
}
