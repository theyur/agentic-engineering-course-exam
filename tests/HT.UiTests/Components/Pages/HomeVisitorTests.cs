using Bunit;
using Bunit.TestDoubles;
using HT.Ui.Components.Pages;

namespace HT.UiTests.Components.Pages;

/// <summary>US5-AS8, AS9, HP-011: the visitor Home shows the hero and "How it works", and nothing else.</summary>
public sealed class HomeVisitorTests : BunitContext
{
    private readonly BunitAuthorizationContext _authorization;

    public HomeVisitorTests()
    {
        _authorization = AddAuthorization();
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
    public void US5_AS9_register_free_is_the_only_hero_button_and_links_to_register()
    {
        var cut = Render<Home>();

        var button = Assert.Single(cut.FindAll(".ht-hero__cta a, .ht-hero__cta button"));
        Assert.Equal("Register free", button.TextContent);
        Assert.Equal("/register", button.GetAttribute("href"));
        Assert.Contains("ht-btn--primary", button.ClassList);
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
    [InlineData("Browse the catalog")]
    [InlineData("Example goal")]
    [InlineData("Popular in the catalog")]
    [InlineData("Community")]
    [InlineData("participants")]
    [InlineData("Consistency Bands")]
    public void US5_AS8_nothing_else_from_the_preview_is_shown(string absent)
    {
        var cut = Render<Home>();

        Assert.DoesNotContain(absent, cut.Markup.Replace("Community comparison is opt-in.", string.Empty, StringComparison.Ordinal), StringComparison.Ordinal);
        Assert.Empty(cut.FindAll(".ht-card, .ht-habit, .ht-bands, .ht-stats, .ht-strip"));
    }

    [Fact]
    public void HP_009_a_signed_in_user_does_not_get_the_visitor_home()
    {
        _authorization.SetAuthorized("ana@example.com");

        var cut = Render<Home>();

        Assert.Empty(cut.FindAll(".ht-hero"));
        Assert.DoesNotContain("Register free", cut.Markup, StringComparison.Ordinal);
    }
}
