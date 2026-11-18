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
