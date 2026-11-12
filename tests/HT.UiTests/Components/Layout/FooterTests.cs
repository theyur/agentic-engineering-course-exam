using Bunit;
using HT.Ui.Components.Layout;

namespace HT.UiTests.Components.Layout;

/// <summary>US5-AS6, HP-010: the footer of slice 1.</summary>
public sealed class FooterTests : BunitContext
{
    [Fact]
    public void US5_AS6_the_footer_shows_the_project_line_and_privacy_and_no_send_feedback()
    {
        var cut = Render<Footer>();

        Assert.Equal("Habit Tracker · Agentic Engineering course project", cut.Find(".ht-footer__inner > span").TextContent);
        var link = Assert.Single(cut.FindAll("a"));
        Assert.Equal("Privacy", link.TextContent);
        Assert.Equal("/privacy", link.GetAttribute("href"));
        Assert.DoesNotContain("Send feedback", cut.Markup, StringComparison.Ordinal);
    }
}
