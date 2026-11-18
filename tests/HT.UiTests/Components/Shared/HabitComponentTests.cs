using Bunit;
using HT.Application.Goals;
using HT.Domain.Goals;
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

    /// <summary>Wednesday 18 November 2026, the list's today in the GoalItem tests.</summary>
    private static readonly DateOnly GoalToday = new(2026, 11, 18);

    private IRenderedComponent<GoalItem> RenderGoal(GoalListItem item) =>
        Render<GoalItem>(p => p.Add(x => x.Item, item).Add(x => x.Today, GoalToday));

    private static GoalListItem GoalAt(
        string schedule, GoalScheduleKind kind, DateOnly? start = null, DateOnly? next = null, DateOnly? firstWeek = null,
        BuildOrReduce side = BuildOrReduce.Build) => new()
    {
        Id = new GoalId(Guid.Parse("4b1d0c9e-0000-4000-8000-000000000001")),
        Name = "Reading, 15 min, 5 days a week",
        HabitName = "Reading",
        HabitKind = HabitKind.Catalog,
        BuildOrReduce = side,
        Measurement = Measurement.Duration,
        TargetText = "15 min, 5 days a week",
        ScheduleText = schedule,
        ScheduleKind = kind,
        StartDate = start,
        NextCommitment = next,
        FirstWeekStarts = firstWeek,
    };

    /// <summary>Every meta-line variant of HP-003, in its order of precedence, and the schedule alone.</summary>
    public static TheoryData<GoalListItem, string> GoalMetaLines => new()
    {
        {
            GoalAt("Flexible · 2 times a week", GoalScheduleKind.PeriodBased, start: new(2026, 11, 20), firstWeek: new(2026, 11, 23)),
            "Flexible · 2 times a week · first week starts Mon 23 Nov"
        },
        { GoalAt("Flexible · 2 times a week", GoalScheduleKind.PeriodBased, firstWeek: new(2026, 11, 23)), "Flexible · 2 times a week · first week starts Mon 23 Nov" },
        { GoalAt("Mon · Wed · Fri", GoalScheduleKind.CommitmentBased, start: new(2026, 11, 24), next: new(2026, 11, 25)), "Mon · Wed · Fri · starts Tue 24 Nov" },
        { GoalAt("Every day", GoalScheduleKind.PeriodBased, start: new(2026, 11, 18)), "Every day · starts Wed 18 Nov" },
        { GoalAt("Mon–Fri", GoalScheduleKind.CommitmentBased, next: GoalToday), "Mon–Fri · next today" },
        { GoalAt("Mon · Wed · Fri", GoalScheduleKind.CommitmentBased, next: new(2026, 11, 20)), "Mon · Wed · Fri · next Fri 20 Nov" },
        { GoalAt("2 times a day", GoalScheduleKind.PeriodBased), "2 times a day" },
        { GoalAt("Flexible · 3 times a week", GoalScheduleKind.PeriodBased), "Flexible · 3 times a week" },
    };

    [Theory]
    [MemberData(nameof(GoalMetaLines))]
    public void GoalItem_shows_the_name_link_the_tag_and_the_meta_line(GoalListItem item, string meta)
    {
        var cut = RenderGoal(item);

        var root = cut.Find("div.ht-goalitem");
        Assert.Contains("ht-goalitem--reduced", root.ClassList);
        var link = cut.Find("a.ht-goalitem__name");
        Assert.Equal("Reading, 15 min, 5 days a week", link.TextContent);
        Assert.Equal("/goals/4b1d0c9e-0000-4000-8000-000000000001", link.GetAttribute("href"));
        Assert.Equal("Build", cut.Find(".ht-goalitem__meta .ht-chip").TextContent);
        Assert.Equal(meta, cut.Find(".ht-goalitem__meta span:not(.ht-chip)").TextContent);
        foreach (var absent in new[] { "ht-strip", "ht-goalitem__actions", "Record", "%" })
        {
            Assert.DoesNotContain(absent, cut.Markup, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void GoalItem_shows_Reduce_as_its_tag()
    {
        var cut = RenderGoal(GoalAt("Every day", GoalScheduleKind.PeriodBased, side: BuildOrReduce.Reduce));

        Assert.Equal("Reduce", cut.Find(".ht-goalitem__meta .ht-chip").TextContent);
        Assert.Equal("Every day", cut.Find(".ht-goalitem__meta span:not(.ht-chip)").TextContent);
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
