using Bunit;
using HT.Domain.Habits;
using HT.Ui.Components.Shared;
using static HT.Ui.Components.Shared.HabitCard;

namespace HT.UiTests.Components.Shared;

/// <summary>The slice-2 shared components against their design files (constitution IV).</summary>
public sealed class HabitComponentTests : BunitContext
{
    private IRenderedComponent<HabitCard> Card(HabitCardVariant variant, string? start = "20 pages, 3 times a week", string? description = "Books.") =>
        Render<HabitCard>(p => p
            .Add(x => x.Icon, "book-open")
            .Add(x => x.Name, "Reading")
            .Add(x => x.Category, "Mind")
            .Add(x => x.Intent, "Build")
            .Add(x => x.Description, description)
            .Add(x => x.SuggestedStartingGoal, start)
            .Add(x => x.Variant, variant)
            .Add(x => x.Href, "/catalog/1"));

    [Fact]
    public void The_visitor_card_shows_the_fields_the_label_and_See_details()
    {
        var cut = Card(HabitCardVariant.Visitor);

        Assert.Equal("Reading", cut.Find(".ht-habit__name").TextContent);
        Assert.Equal(["Mind", "Build"], cut.FindAll(".ht-habit__tags .ht-chip").Select(c => c.TextContent));
        Assert.Equal("Books.", cut.Find(".ht-habit__desc").TextContent);
        Assert.Equal("Suggested starting goal: 20 pages, 3 times a week", cut.Find(".ht-habit__start").TextContent);
        Assert.Equal("20 pages, 3 times a week", cut.Find(".ht-habit__start b").TextContent);
        var action = cut.Find(".ht-habit__actions a");
        Assert.Equal("See details", action.TextContent);
        Assert.Equal("/catalog/1", action.GetAttribute("href"));
        Assert.Single(cut.FindAll(".ht-habit__actions a"));
        Assert.NotNull(cut.Find(".ht-habit__glyph svg.ht-icon--lg"));
    }

    [Fact]
    public void The_signed_in_card_has_Details_only()
    {
        var cut = Card(HabitCardVariant.SignedIn);

        Assert.Equal(["Details"], cut.FindAll(".ht-habit__actions a").Select(a => a.TextContent));
        Assert.DoesNotContain("Use this habit", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("goal", cut.Find(".ht-habit__actions").TextContent, StringComparison.Ordinal);
    }

    [Fact]
    public void A_habit_without_a_suggested_starting_goal_has_no_such_line()
    {
        var cut = Card(HabitCardVariant.Visitor, start: null, description: null);

        Assert.Empty(cut.FindAll(".ht-habit__start"));
        Assert.Empty(cut.FindAll(".ht-habit__desc"));
        Assert.DoesNotContain("Suggested", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void The_custom_habit_card_has_the_Private_tag_and_Edit()
    {
        var cut = Card(HabitCardVariant.CustomHabit, start: null);

        var tags = cut.FindAll(".ht-habit__tags .ht-chip");
        Assert.Equal(["Mind", "Build", "Private"], tags.Select(t => t.TextContent));
        Assert.Contains("ht-chip--strong", tags[2].ClassList);
        var edit = cut.Find(".ht-habit__actions a");
        Assert.Equal("Edit", edit.TextContent);
        Assert.Equal("Edit Reading", edit.GetAttribute("aria-label"));
        Assert.DoesNotContain("Suggest for catalog", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void Tag_shows_its_word_in_both_variants()
    {
        Assert.Equal("<span class=\"ht-chip\">Draft</span>", Render<Tag>(p => p.Add(x => x.Text, "Draft")).Markup);
        Assert.Equal("<span class=\"ht-chip ht-chip--strong\">Published</span>",
            Render<Tag>(p => p.Add(x => x.Text, "Published").Add(x => x.Strong, true)).Markup);
    }

    [Fact]
    public void Link_tabs_mark_the_current_page()
    {
        var cut = Render<Tabs>(p => p
            .Add(x => x.Links, true)
            .Add(x => x.Label, "Catalog views")
            .Add(x => x.Items, [new Tabs.TabItem("Public catalog", "/catalog", 22, true), new Tabs.TabItem("My custom habits", "/custom-habits", 2, false)]));

        var nav = cut.Find("nav.ht-tabs");
        Assert.Equal("Catalog views", nav.GetAttribute("aria-label"));
        var links = cut.FindAll("a.ht-tab");
        Assert.Equal(["/catalog", "/custom-habits"], links.Select(l => l.GetAttribute("href")));
        Assert.Equal("page", links[0].GetAttribute("aria-current"));
        Assert.Null(links[1].GetAttribute("aria-current"));
        Assert.Equal(["22", "2"], cut.FindAll(".ht-count").Select(c => c.TextContent));
        Assert.Equal("Public catalog22", links[0].TextContent);
    }

    [Fact]
    public void Button_tabs_report_the_chosen_tab()
    {
        Tabs.TabItem? chosen = null;
        var second = new Tabs.TabItem("Second", null, null, false);
        var cut = Render<Tabs>(p => p
            .Add(x => x.Label, "Sections")
            .Add(x => x.Items, [new Tabs.TabItem("First", null, null, true), second])
            .Add(x => x.OnSelect, (Tabs.TabItem item) => chosen = item));

        var buttons = cut.FindAll("button[role=tab]");
        Assert.Equal("true", buttons[0].GetAttribute("aria-selected"));
        buttons[1].Click();
        Assert.Same(second, chosen);
    }

    [Fact]
    public void SearchField_is_labelled_and_reports_every_change()
    {
        string? value = null;
        var cut = Render<SearchField>(p => p
            .Add(x => x.Label, "Search habits")
            .Add(x => x.ValueChanged, (string v) => value = v));

        var input = cut.Find("input[type=search]");
        Assert.Equal("Search habits", input.GetAttribute("aria-label"));
        Assert.Equal("Search habits", input.GetAttribute("placeholder"));
        input.Input("mor");
        Assert.Equal("mor", value);
    }

    [Fact]
    public void FilterPanel_has_the_Category_radio_group_and_the_two_Intent_checkboxes()
    {
        Category? category = Category.Mind;
        (bool Build, bool Reduce)? intent = null;
        var cut = Render<FilterPanel>(p => p
            .Add(x => x.Categories, [new FilterPanel.CategoryOption(null, "All"), new FilterPanel.CategoryOption(Category.Body, "Body"), new FilterPanel.CategoryOption(Category.Mind, "Mind")])
            .Add(x => x.Category, Category.Mind)
            .Add(x => x.CategoryChanged, (Category? c) => category = c)
            .Add(x => x.Build, true)
            .Add(x => x.IntentChanged, ((bool, bool) i) => intent = i));

        Assert.Equal(["Category", "Intent"], cut.FindAll(".ht-filter__title").Select(t => t.TextContent));
        var radios = cut.FindAll("input[type=radio]");
        Assert.Equal(["All", "Body", "Mind"], cut.FindAll("[role=radiogroup] label").Select(l => l.TextContent.Trim()));
        Assert.True(radios[2].HasAttribute("checked"));
        Assert.Equal(["Build", "Reduce"], cut.FindAll("[role=group] label").Select(l => l.TextContent.Trim()));
        var boxes = cut.FindAll("input[type=checkbox]");
        Assert.True(boxes[0].HasAttribute("checked"));
        Assert.False(boxes[1].HasAttribute("checked"));
        Assert.Empty(cut.FindAll("button"));
        Assert.DoesNotContain("Only habits I track", cut.Markup, StringComparison.Ordinal);

        radios[0].Change(true);
        Assert.Null(category);
        cut.FindAll("input[type=checkbox]")[1].Change(true);
        Assert.Equal((true, true), intent);
    }

    [Fact]
    public void BackLink_names_the_parent_page()
    {
        var link = Render<BackLink>(p => p.Add(x => x.Href, "/catalog").Add(x => x.Text, "Catalog")).Find("a.ht-back");
        Assert.Equal("← Catalog", link.TextContent);
        Assert.Equal("/catalog", link.GetAttribute("href"));
    }

    [Fact]
    public void TextArea_has_its_label_and_its_error_under_the_field()
    {
        var cut = Render<TextArea>(p => p
            .Add(x => x.Id, "description")
            .Add(x => x.Label, "Description")
            .Add(x => x.Optional, true)
            .Add(x => x.Error, "The description can have at most 500 characters."));

        Assert.Equal("Description (optional)", string.Join(" ", cut.Find(".ht-field__label").TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)));
        var area = cut.Find("textarea#description");
        Assert.Contains("ht-textarea", area.ClassList);
        Assert.Equal("description-error", area.GetAttribute("aria-describedby"));
        Assert.Equal("true", area.GetAttribute("aria-invalid"));
        Assert.Equal("The description can have at most 500 characters.", cut.Find("#description-error").TextContent);
    }
}
