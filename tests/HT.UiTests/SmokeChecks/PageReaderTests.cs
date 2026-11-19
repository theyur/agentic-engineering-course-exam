using HT.SmokeCheck;

namespace HT.UiTests.SmokeChecks;

/// <summary>T006: the smoke check's page reading, on fixed HTML (research R-11).</summary>
public sealed class PageReaderTests
{
    [Theory]
    [InlineData("""<form><input type="hidden" name="__RequestVerificationToken" value="tok&#x2B;en" /></form>""")]
    [InlineData("""<form><input value="tok&#x2B;en" type="hidden" name="__RequestVerificationToken" /></form>""")]
    public void ReadToken_reads_the_antiforgery_token_in_either_attribute_order(string html)
    {
        Assert.Equal("tok+en", PageReader.ReadToken(html, "/sign-in"));
    }

    [Fact]
    public void ReadToken_names_the_path_when_the_page_has_no_token()
    {
        var error = Assert.Throws<InvalidOperationException>(() => PageReader.ReadToken("<form></form>", "/sign-in"));

        Assert.Contains("/sign-in", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ReadHandlers_reads_every_handler_in_either_attribute_order()
    {
        const string html = """
            <form><input type="hidden" name="_handler" value="sign-in" /></form>
            <form><input value="sign-out" type="hidden" name="_handler" /></form>
            """;

        Assert.Equal(["sign-in", "sign-out"], PageReader.ReadHandlers(html));
    }

    [Fact]
    public void ReadHandlers_is_empty_without_a_handler()
    {
        Assert.Empty(PageReader.ReadHandlers("<form></form>"));
    }

    [Fact]
    public void ReadFirstHeading_returns_the_decoded_text_of_the_first_h1()
    {
        const string html = """
            <section class="ht-hero"><h1 class="big">Keep the habits you choose &amp; more</h1></section>
            <h1>Second</h1>
            """;

        Assert.Equal("Keep the habits you choose & more", PageReader.ReadFirstHeading(html));
    }

    [Fact]
    public void ReadFirstHeading_is_null_without_an_h1()
    {
        Assert.Null(PageReader.ReadFirstHeading("<h2>Not a page heading</h2>"));
    }

    [Fact]
    public void ReadHabitNames_returns_the_card_names_after_html_decoding()
    {
        const string html = """
            <article class="ht-card ht-habit">
                <div class="ht-habit__head"><h3 class="ht-habit__name">Read &amp; reflect</h3></div>
            </article>
            <article class="ht-card ht-habit">
                <div class="ht-habit__head"><h3 class="ht-habit__name">Don&#x27;t snooze</h3></div>
            </article>
            <h3>Pick a habit</h3>
            """;

        Assert.Equal(["Read & reflect", "Don't snooze"], PageReader.ReadHabitNames(html));
    }

    [Fact]
    public void ReadHabitNames_ignores_goal_items()
    {
        Assert.Empty(PageReader.ReadHabitNames("""<a class="ht-goalitem__name" href="/goals/1">Reading</a>"""));
    }

    [Fact]
    public void ReadGoalNames_returns_the_goal_item_names_after_html_decoding()
    {
        const string html = """
            <div class="ht-goalitem ht-goalitem--reduced">
                <div><a class="ht-goalitem__name" href="/goals/1">Read &amp; reflect, 15 min, 5 days a week</a>
                <div class="ht-goalitem__meta"><span class="ht-tag">Build</span><span>Mon–Fri</span></div></div>
            </div>
            <div class="ht-goalitem ht-goalitem--reduced">
                <div><a class="ht-goalitem__name" href="/goals/2">Don&#x27;t snooze</a></div>
            </div>
            <a href="/goals/new">New goal</a>
            <h3 class="ht-habit__name">A habit card</h3>
            """;

        Assert.Equal(["Read & reflect, 15 min, 5 days a week", "Don't snooze"], PageReader.ReadGoalNames(html));
    }

    [Fact]
    public void ReadGoalNames_reads_nested_markup_and_other_classes_on_the_same_element()
    {
        const string html = """
            <a href="/goals/1" class="ht-link ht-goalitem__name is-current" data-x="1"><span>Morning</span> <em>yoga</em></a>
            <A CLASS="ht-goalitem__name" HREF="/goals/2">Coffee, at most 2 cups a day</A >
            <a class="ht-goalitem__name-extra" href="/goals/3">Not a goal name</a>
            <a class="ht-goalitem__meta" href="/goals/4">Not a goal name either</a>
            """;

        Assert.Equal(["Morning yoga", "Coffee, at most 2 cups a day"], PageReader.ReadGoalNames(html));
    }

    [Fact]
    public void ReadGoalNames_is_empty_without_a_goal_item()
    {
        Assert.Empty(PageReader.ReadGoalNames("<h1>Goals</h1><p>No goals yet.</p>"));
    }

    [Fact]
    public void ReadGoalLinks_pairs_each_goal_item_name_with_its_decoded_href()
    {
        const string html = """
            <div class="ht-goalitem"><div><a class="ht-goalitem__name" href="/goals/7?from=a&amp;b">Read &amp; reflect</a></div></div>
            <a href="/goals/8" class="ht-link ht-goalitem__name"><span>Morning</span> <em>yoga</em></a>
            <a class="ht-goalitem__name">No link</a>
            <a class="ht-goalitem__name-extra" href="/goals/9">Not a goal name</a>
            <a href="/goals/new">New goal</a>
            """;

        Assert.Equal(
            [new GoalLink("Read & reflect", "/goals/7?from=a&b"), new GoalLink("Morning yoga", "/goals/8"), new GoalLink("No link", null)],
            PageReader.ReadGoalLinks(html));
    }

    [Fact]
    public void ReadGoalLinks_is_empty_without_a_goal_item()
    {
        Assert.Empty(PageReader.ReadGoalLinks("<h1>Goals</h1><p>No goals yet.</p>"));
    }

    [Fact]
    public void ReadCardText_reads_only_the_card_with_that_title_and_not_the_settings_card()
    {
        const string html = """
            <section class="ht-card"><div class="ht-card__head"><h2 class="ht-card__title">Settings</h2></div>
                <p>Met Failed Missed Pending Due today 12 min</p></section>
            <section class="ht-card ht-card--wide"><div class="ht-card__head"><h2 class="ht-card__title">History</h2>
                <span class="ht-spacer"></span><p class="ht-card__sub">Corrections are evaluated with the rules of that date.</p></div>
                <table><tr>
                    <td><b>Sat 7</b></td>
                    <td>20 min</td>
                    <td><span class="ht-chip ht-status">Met</span></td>
                    <td><button aria-label="Don&#x27;t stop, edit note of Sat 7">Don&#x27;t
                        stop</button></td>
                </tr></table>
            </section>
            <section class="ht-card"><h2 class="ht-card__title">History</h2><p>A second card</p></section>
            <p>After the card: Missed</p>
            """;

        var text = PageReader.ReadCardText(html, "History");

        Assert.NotNull(text);
        Assert.StartsWith("History Corrections are evaluated with the rules of that date.", text, StringComparison.Ordinal);
        Assert.Contains("Sat 7 20 min Met Don't stop", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Failed", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Missed", text, StringComparison.Ordinal);
        Assert.DoesNotContain("12 min", text, StringComparison.Ordinal);
        Assert.DoesNotContain("A second card", text, StringComparison.Ordinal);
        Assert.DoesNotContain("aria-label", text, StringComparison.Ordinal);
    }

    [Fact]
    public void ReadCardText_reads_a_title_with_nested_markup()
    {
        const string html = """<section class="ht-card"><h2 class="ht-card__title"><span>His</span>tory &amp; more</h2> <p>Met</p></section>""";

        Assert.Equal("History & more Met", PageReader.ReadCardText(html, "History & more"));
        Assert.Null(PageReader.ReadCardText(html, "History"));
    }

    [Theory]
    [InlineData("""<h1>Goal</h1><section class="ht-card"><h2 class="ht-card__title">Settings</h2><p>Met</p></section>""")]
    [InlineData("""<div class="ht-card"><h2 class="ht-card__title">History</h2><p>Met</p></div>""")]
    [InlineData("""<section class="ht-card-extra"><h2 class="ht-card__title">History</h2><p>Met</p></section>""")]
    [InlineData("""<section class="ht-card"><h2>History</h2><p>Met</p></section>""")]
    [InlineData("""<section class="ht-card"><h2 class="ht-card__title">History</h2><p>Met</p>""")]
    public void ReadCardText_is_null_without_a_closed_card_section_of_that_title(string html)
    {
        Assert.Null(PageReader.ReadCardText(html, "History"));
    }

    [Fact]
    public void ReadAssetUrls_returns_stylesheets_and_scripts_in_document_order_without_inline_scripts()
    {
        const string html = """
            <head>
                <link rel="preload" href="font.woff2" />
                <link rel="stylesheet" href="css/tokens.css?v=a&amp;b" />
                <link href="css/ht-components.css" rel="stylesheet" />
                <script type="importmap">{"imports":{}}</script>
            </head>
            <body>
                <script>console.log("inline");</script>
                <script src="_framework/blazor.web.js"></script>
            </body>
            """;

        Assert.Equal(
            ["css/tokens.css?v=a&b", "css/ht-components.css", "_framework/blazor.web.js"],
            PageReader.ReadAssetUrls(html));
    }

    [Fact]
    public void ContainsAll_is_true_when_every_expected_name_is_present_among_extra_names()
    {
        Assert.True(PageReader.ContainsAll(["Reading", "Yoga", "Added by a reviewer"], ["Yoga", "Reading"]));
    }

    [Fact]
    public void ContainsAll_is_false_when_one_expected_name_is_missing()
    {
        Assert.False(PageReader.ContainsAll(["Reading"], ["Reading", "Yoga"]));
    }

    [Fact]
    public void ContainsAll_compares_ordinally()
    {
        Assert.False(PageReader.ContainsAll(["reading"], ["Reading"]));
    }
}
