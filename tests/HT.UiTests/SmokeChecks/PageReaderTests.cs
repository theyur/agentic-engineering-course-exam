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
    public void ReadGoalMetas_pairs_a_Paused_item_and_an_Active_item_with_their_meta_texts_without_the_chips()
    {
        // As GoalItem renders them (slice 7 HP-004): the Tag and the Pending StatusChip come before the meta text, with
        // Blazor's comment markers; the figures after the meta line are not part of it.
        const string html = """
            <div class="ht-goalitem"><div>
              <a class="ht-goalitem__name" href="/goals/1">Drinking water, 8 glasses a day</a>
              <div class="ht-goalitem__meta"><!--!--><span class="ht-chip">Build</span>
                <span class="ht-chip ht-status ht-status--pending">Pending</span><!--!-->
                <span>Paused until Sun 22 Nov</span></div></div>
              <div><div class="ht-goalitem__value">94% (18 of 19)</div><div class="ht-goalitem__label">Consistency</div></div>
            </div>
            <div class="ht-goalitem"><div>
              <a class="ht-goalitem__name" href="/goals/2">Read &amp; reflect, 15 min, 5 days a week</a>
              <div class="ht-goalitem__meta"><span class="ht-chip">Build</span><span>Mon&#x2013;Fri &#xB7; next today</span></div></div>
              <div><div class="ht-goalitem__value">N/A</div></div>
            </div>
            """;

        Assert.Equal(
            [
                new GoalMeta("Drinking water, 8 glasses a day", "Paused until Sun 22 Nov"),
                new GoalMeta("Read & reflect, 15 min, 5 days a week", "Mon–Fri · next today"),
            ],
            PageReader.ReadGoalMetas(html));
    }

    [Fact]
    public void ReadGoalMetas_gives_null_for_an_item_without_a_meta_line_and_never_reads_the_next_items_meta()
    {
        const string html = """
            <div class="ht-goalitem"><div><a class="ht-goalitem__name" href="/goals/1">Morning yoga</a></div></div>
            <div class="ht-goalitem"><div><a class="ht-goalitem__name" href="/goals/2">Coffee</a>
              <div class="ht-goalitem__meta-extra"><span>Not a meta line</span></div>
              <div class="ht-goalitem__meta is-muted"><span>Every day</span></div></div></div>
            """;

        Assert.Equal([new GoalMeta("Morning yoga", null), new GoalMeta("Coffee", "Every day")], PageReader.ReadGoalMetas(html));
    }

    [Fact]
    public void ReadGoalMetas_is_empty_without_a_goal_item()
    {
        Assert.Empty(PageReader.ReadGoalMetas("<h1>Goals</h1><p>No paused goals.</p>"));
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
    public void ReadStatLabels_returns_every_GoalStats_label_decoded_and_no_value_or_other_text()
    {
        // Slice 6 research R-11: the GoalStats card as GoalStats.razor renders it, a trend sub line naming "weeks", and a
        // label with another class on the same element.
        const string html = """
            <section class="ht-card"><div class="ht-eyebrow">This attempt</div><h2 class="ht-card__title">Consistency and streaks</h2>
            <div class="ht-stats">
            <div class="ht-stat"><div class="ht-stat__value ht-stat__value--text">90% (18 of 20)</div><div class="ht-stat__label">Consistency</div></div>
            <div class="ht-stat"><div class="ht-stat__value">6</div><div class="ht-stat__label">Streak, commitments</div></div>
            <div class="ht-stat"><div class="ht-stat__value">11</div><div class="extra ht-stat__label">Longest, <b>commitments</b></div></div>
            </div></section>
            <section class="ht-card"><h2 class="ht-card__title">Times per week</h2><p class="ht-card__sub">last 12 weeks</p></section>
            <p class="ht-stat__labels">Streak, weeks</p>
            """;

        var labels = PageReader.ReadStatLabels(html);

        Assert.Equal(["Consistency", "Streak, commitments", "Longest, commitments"], labels);
        Assert.False(PageReader.ContainsAll(labels, ["Streak, weeks"]));
    }

    [Fact]
    public void ReadStatLabels_a_part_of_a_label_does_not_count()
    {
        const string html = """<div class="ht-stat__label">Streak, commitments and more</div>""";

        var labels = PageReader.ReadStatLabels(html);

        Assert.Equal(["Streak, commitments and more"], labels);
        Assert.False(PageReader.ContainsAll(labels, ["Streak, commitments"]));
        Assert.False(PageReader.ContainsAll(labels, ["Consistency"]));
    }

    [Fact]
    public void ReadTileLabels_returns_every_StatTile_label_of_buttons_and_links_decoded()
    {
        // As StatTile.razor renders a filter tile (button) and the "Active goals" tile (link).
        const string html = """
            <div class="ht-tiles">
            <button type="button" class="ht-tile" aria-pressed="false"><span class="ht-tile__value">3</span><span class="ht-tile__label">Due now</span></button>
            <button type="button" class="ht-tile ht-tile--pending" aria-pressed="false"><span class="ht-tile__value">3</span><span class="ht-tile__label">Pending &#xB7; still can record</span></button>
            <button type="button" class="ht-tile ht-tile--failed" aria-pressed="false"><span class="ht-tile__value">1</span><span class="ht-tile__label">Missed this week</span></button>
            <a class="ht-tile" href="/goals"><span class="ht-tile__value">3</span><span class="ht-tile__label">Active goals</span></a>
            </div>
            <span class="ht-tile__value">Missed this week now</span>
            """;

        var labels = PageReader.ReadTileLabels(html);

        Assert.Equal(["Due now", "Pending · still can record", "Missed this week", "Active goals"], labels);
        Assert.True(PageReader.ContainsAll(labels, ["Due now", "Missed this week"]));
        Assert.False(PageReader.ContainsAll(labels, ["Missed"]));
    }

    [Fact]
    public void ReadStatusChips_returns_every_StatusChip_word_and_not_other_chips_or_modifiers_alone()
    {
        // As StatusChip.razor renders it; a plain chip and an element with only a modifier class are not status chips.
        const string html = """
            <div class="ht-row"><span class="ht-chip ht-status ht-status--pending">Pending</span></div>
            <div class="ht-row"><span class="ht-chip ht-status ht-status--missed">Missed</span></div>
            <span class="ht-chip">Missed this week</span>
            <span class="ht-status--missed">Failed</span>
            <span class="ht-statuses">Met</span>
            """;

        var chips = PageReader.ReadStatusChips(html);

        Assert.Equal(["Pending", "Missed"], chips);
        Assert.True(PageReader.ContainsAll(chips, ["Missed"]));
        Assert.False(PageReader.ContainsAll(chips, ["Failed"]));
        Assert.False(PageReader.ContainsAll(chips, ["Missed this week"]));
    }

    [Fact]
    public void The_three_slice_6_readers_are_empty_without_their_elements()
    {
        const string html = "<h1>Today</h1><p>Consistency, Due now, Missed</p>";

        Assert.Empty(PageReader.ReadStatLabels(html));
        Assert.Empty(PageReader.ReadTileLabels(html));
        Assert.Empty(PageReader.ReadStatusChips(html));
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
