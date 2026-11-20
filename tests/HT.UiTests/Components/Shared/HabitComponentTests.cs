using Bunit;
using HT.Application.Goals;
using HT.Application.Records;
using HT.Application.Reports;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using HT.Domain.Reports;
using HT.Ui.Components.Shared;
using static HT.Ui.Components.Shared.HabitCard;

namespace HT.UiTests.Components.Shared;

/// <summary>
/// The slice-2 shared components, with their slice-4 states and slice 6's full GoalItem (design/components/GoalListItem,
/// "Slice 6 state"), against their design files (constitution IV).
/// </summary>
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
    public void The_signed_in_card_without_goal_information_has_Details_only()
    {
        // An administrator's card keeps slice 2's state (HP-005).
        var cut = Card(HabitCardVariant.SignedIn);

        Assert.Equal(["Details"], cut.FindAll(".ht-habit__actions a").Select(a => a.TextContent));
        Assert.DoesNotContain("Use this habit", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("goal", cut.Find(".ht-habit__actions").TextContent, StringComparison.Ordinal);
        Assert.Empty(cut.FindAll(".ht-habit__head .ht-chip"));
    }

    private const string NewGoalHref = "/goals/new?habit=1&kind=catalog";

    private IRenderedComponent<HabitCard> RegisteredCard(string? goalsText) =>
        Render<HabitCard>(p => p
            .Add(x => x.Icon, "book-open")
            .Add(x => x.Name, "Reading")
            .Add(x => x.Category, "Mind")
            .Add(x => x.Intent, "Build")
            .Add(x => x.Variant, HabitCardVariant.SignedIn)
            .Add(x => x.Href, "/catalog/1")
            .Add(x => x.NewGoalHref, NewGoalHref)
            .Add(x => x.GoalsText, goalsText));

    [Fact]
    public void US6_AS5_a_tracked_card_has_the_strong_goal_Tag_Add_another_goal_and_Details()
    {
        var cut = RegisteredCard("2 goals");

        var tag = cut.Find(".ht-habit__head .ht-chip");
        Assert.Equal("2 goals", tag.TextContent);
        Assert.Contains("ht-chip--strong", tag.ClassList);
        Assert.Equal(["Mind", "Build"], cut.FindAll(".ht-habit__tags .ht-chip").Select(c => c.TextContent));
        var actions = cut.FindAll(".ht-habit__actions a");
        Assert.Equal(["Add another goal", "Details"], actions.Select(a => a.TextContent));
        Assert.Equal([NewGoalHref, "/catalog/1"], actions.Select(a => a.GetAttribute("href")));
        Assert.Contains("ht-btn--secondary", actions[0].ClassList);
        Assert.Contains("ht-btn--ghost", actions[1].ClassList);
    }

    [Fact]
    public void US6_AS5_an_untracked_card_has_Use_this_habit_and_Details_and_no_goal_Tag()
    {
        var cut = RegisteredCard(null);

        Assert.Empty(cut.FindAll(".ht-habit__head .ht-chip"));
        var actions = cut.FindAll(".ht-habit__actions a");
        Assert.Equal(["Use this habit", "Details"], actions.Select(a => a.TextContent));
        Assert.Equal(NewGoalHref, actions[0].GetAttribute("href"));
        Assert.Contains("ht-btn--primary", actions[0].ClassList);
    }

    [Fact]
    public void The_visitor_and_custom_habit_cards_ignore_goal_parameters()
    {
        foreach (var variant in new[] { HabitCardVariant.Visitor, HabitCardVariant.CustomHabit })
        {
            var cut = Render<HabitCard>(p => p
                .Add(x => x.Icon, "book-open")
                .Add(x => x.Name, "Reading")
                .Add(x => x.Category, "Mind")
                .Add(x => x.Intent, "Build")
                .Add(x => x.Variant, variant)
                .Add(x => x.Href, "/catalog/1")
                .Add(x => x.NewGoalHref, NewGoalHref)
                .Add(x => x.GoalsText, "2 goals"));

            Assert.Single(cut.FindAll(".ht-habit__actions a"));
            Assert.DoesNotContain("goal", cut.Markup, StringComparison.Ordinal);
        }
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
    public void US6_AS5_FilterPanel_with_ShowTracked_adds_Show_with_Only_habits_I_track()
    {
        bool? tracked = null;
        var cut = Render<FilterPanel>(p => p
            .Add(x => x.Id, "f")
            .Add(x => x.Categories, [new FilterPanel.CategoryOption(null, "All")])
            .Add(x => x.ShowTracked, true)
            .Add(x => x.Tracked, true)
            .Add(x => x.TrackedChanged, (bool t) => tracked = t));

        Assert.Equal(["Category", "Intent", "Show"], cut.FindAll(".ht-filter__title").Select(t => t.TextContent));
        var show = cut.FindAll("[role=group]")[1];
        Assert.Equal("f-show-title", show.GetAttribute("aria-labelledby"));
        Assert.Equal("Only habits I track", show.QuerySelector("label")!.TextContent.Trim());
        var box = show.QuerySelector("input[type=checkbox]")!;
        Assert.True(box.HasAttribute("checked"));

        cut.FindAll("input[type=checkbox]")[2].Change(false);
        Assert.False(tracked);
    }

    /// <summary>Wednesday 18 November 2026, the list's today in the GoalItem tests.</summary>
    private static readonly DateOnly GoalToday = new(2026, 11, 18);

    /// <summary>10:00Z on Wednesday 18 November 2026, the instant the GoalItem tests' overview was read at.</summary>
    private static readonly DateTimeOffset GoalReadAt = new(2026, 11, 18, 10, 0, 0, TimeSpan.Zero);

    private static readonly GoalId ItemId = new(Guid.Parse("4b1d0c9e-0000-4000-8000-000000000001"));

    private IRenderedComponent<GoalItem> RenderGoal(OverviewGoal goal, Action<OverviewGoal>? onRecord = null) =>
        Render<GoalItem>(p => p
            .Add(x => x.Goal, goal)
            .Add(x => x.Today, GoalToday)
            .Add(x => x.OnRecord, (OverviewGoal g) => onRecord?.Invoke(g)));

    private IRenderedComponent<GoalItem> RenderGoal(GoalListItem item) => RenderGoal(OverviewOf(item));

    /// <summary>
    /// The goal of <paramref name="item"/> as the overview gives it: "90% (18 of 20)", a streak of 6 commitments, the
    /// last 15 ended results (Met × 5, Missed, Met × 7, Pending × 2), and <paramref name="recordable"/> as what can be
    /// recorded now ("Reading, 20 min daily", newest first), from which a Pending result and "Record" follow.
    /// </summary>
    private static OverviewGoal OverviewOf(GoalListItem item, params HistoryEntry[] recordable)
    {
        EvaluationState[] strip =
        [
            .. Enumerable.Repeat(EvaluationState.Met, 5), EvaluationState.Missed, .. Enumerable.Repeat(EvaluationState.Met, 7),
            EvaluationState.Pending, EvaluationState.Pending,
        ];
        return new OverviewGoal
        {
            Item = item,
            Stats = new GoalStatsView
            {
                Consistency = new Consistency(18, 20), ConsistencyText = "90% (18 of 20)", Unit = StreakUnit.Commitments,
                CurrentStreak = 6, CurrentStreakText = "6 commitments", LongestStreak = 11, LongestStreakText = "11 commitments",
            },
            Recent = new RecentResults
            {
                Results = strip.Select((state, i) => new ResultView
                {
                    FirstDate = new DateOnly(2026, 11, 2).AddDays(i), LastDate = new DateOnly(2026, 11, 2).AddDays(i), State = state,
                    Progress = new Progress(ProgressKind.RecordedValue, state == EvaluationState.Met ? 20 : null, 20),
                }).ToList(),
                Counts = new Dictionary<EvaluationState, int>
                {
                    [EvaluationState.Met] = 12, [EvaluationState.Failed] = 0, [EvaluationState.Missed] = 1, [EvaluationState.Pending] = 2,
                },
            },
            HasPending = recordable.Any(e => e.State == EvaluationState.Pending),
            CanRecord = recordable.Length > 0,
            Recordable = new GoalHistory
            {
                GoalId = item.Id, GoalScheduleKind = GoalScheduleKind.CommitmentBased, BuildOrReduce = BuildOrReduce.Build,
                Measurement = Measurement.Duration, Amount = 20, DurationUnit = DurationUnit.Minutes, CurrentDate = GoalToday,
                Target = Reading20MinDaily(), ReadAt = GoalReadAt, Entries = recordable,
            },
        };
    }

    private static GoalTarget Reading20MinDaily()
    {
        Assert.True(GoalTarget.TryCreate(new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = 20, DurationUnit = DurationUnit.Minutes,
            ScheduleKind = ScheduleKind.FixedDays, Days = Enum.GetValues<DayOfWeek>().ToHashSet(),
        }, out var target, out _));
        return target;
    }

    /// <summary>A day of "Reading, 20 min daily" in <paramref name="state"/>, with one record of <paramref name="minutes"/> or none.</summary>
    private static HistoryEntry Day(DateOnly date, EvaluationState state, int? minutes = null)
    {
        var starts = new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        return new HistoryEntry
        {
            FirstDate = date, LastDate = date, Starts = starts, Ends = starts.AddDays(1), State = state, GraceEnds = starts.AddDays(3),
            Records = minutes is { } amount ? [new RecordView(new RecordId(Guid.NewGuid()), date, null, amount)] : [],
            Progress = new Progress(ProgressKind.RecordedValue, minutes, 20), IsCorrection = false,
        };
    }

    private static GoalListItem GoalAt(
        string schedule, GoalScheduleKind kind, DateOnly? start = null, DateOnly? next = null, DateOnly? firstWeek = null,
        BuildOrReduce side = BuildOrReduce.Build) => new()
    {
        Id = ItemId,
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
    public void GoalItem_shows_the_name_link_the_tag_and_slice_4s_meta_line(GoalListItem item, string meta)
    {
        var cut = RenderGoal(item);

        var root = cut.Find("div.ht-goalitem");
        Assert.DoesNotContain("ht-goalitem--reduced", root.ClassList);
        Assert.Equal(5, root.Children.Length);
        var link = cut.Find("a.ht-goalitem__name");
        Assert.Equal("Reading, 15 min, 5 days a week", link.TextContent);
        Assert.Equal("/goals/4b1d0c9e-0000-4000-8000-000000000001", link.GetAttribute("href"));
        Assert.Equal(["Build"], cut.FindAll(".ht-goalitem__meta .ht-chip").Select(c => c.TextContent));
        Assert.Equal(meta, cut.Find(".ht-goalitem__meta span:not(.ht-chip)").TextContent);
    }

    [Fact]
    public void GoalItem_shows_Reduce_as_its_tag()
    {
        var cut = RenderGoal(GoalAt("Every day", GoalScheduleKind.PeriodBased, side: BuildOrReduce.Reduce));

        Assert.Equal("Reduce", cut.Find(".ht-goalitem__meta .ht-chip").TextContent);
        Assert.Equal("Every day", cut.Find(".ht-goalitem__meta span:not(.ht-chip)").TextContent);
    }

    [Fact]
    public void GoalItem_adds_the_progress_of_a_Due_entry_with_a_record_and_the_Pending_chip()
    {
        var cut = RenderGoal(OverviewOf(
            GoalAt("Every day", GoalScheduleKind.CommitmentBased, next: GoalToday),
            Day(GoalToday, EvaluationState.Due, 12), Day(new DateOnly(2026, 11, 17), EvaluationState.Pending)));

        var chips = cut.FindAll(".ht-goalitem__meta .ht-chip");
        Assert.Equal(["Build", "Pending"], chips.Select(c => c.TextContent));
        Assert.Equal(["ht-chip", "ht-status", "ht-status--pending"], chips[1].ClassList);
        Assert.Equal("Every day · next today · 12 of 20 min today", cut.Find(".ht-goalitem__meta span:not(.ht-chip)").TextContent);
    }

    [Fact]
    public void GoalItem_keeps_slice_4s_meta_line_for_a_Due_entry_without_a_record_and_has_no_chip_without_a_Pending_result()
    {
        var cut = RenderGoal(OverviewOf(GoalAt("Every day", GoalScheduleKind.CommitmentBased, next: GoalToday), Day(GoalToday, EvaluationState.Due)));

        Assert.Equal(["Build"], cut.FindAll(".ht-goalitem__meta .ht-chip").Select(c => c.TextContent));
        Assert.Equal("Every day · next today", cut.Find(".ht-goalitem__meta span:not(.ht-chip)").TextContent);
    }

    [Fact]
    public void GoalItem_shows_Consistency_and_Current_streak_as_whole_texts_and_the_mini_strip_of_15()
    {
        var cut = RenderGoal(GoalAt("Every day", GoalScheduleKind.CommitmentBased));

        Assert.Equal(
            [("90% (18 of 20)", "Consistency"), ("6 commitments", "Current streak")],
            cut.FindAll(".ht-goalitem__value").Select(v => (v.TextContent, v.NextElementSibling!.TextContent)));
        Assert.Empty(cut.FindAll(".ht-goalitem__value small"));
        var strip = cut.Find(".ht-strip");
        Assert.Equal(["ht-strip", "ht-strip--mini"], strip.ClassList);
        Assert.Equal(("img", "Last 15 commitments: 12 met, 1 missed, 2 pending"), (strip.GetAttribute("role"), strip.GetAttribute("aria-label")));
        Assert.Equal(15, strip.QuerySelectorAll(".ht-strip__cell").Length);
        Assert.Equal("ht-cell--missed", strip.QuerySelectorAll(".ht-strip__cell")[5].ClassList[1]);
        Assert.Empty(cut.FindAll(".ht-legend"));
    }

    /// <summary>"Record": primary with a Due entry, secondary with only Pending ones, absent with nothing to record (HP-002).</summary>
    public static TheoryData<string, string?> RecordActions => new()
    {
        { "due", "ht-btn--primary" },
        { "pending", "ht-btn--secondary" },
        { "none", null },
    };

    [Theory]
    [MemberData(nameof(RecordActions))]
    public void GoalItem_has_Record_as_primary_secondary_or_no_action(string recordable, string? variant)
    {
        HistoryEntry[] entries = recordable switch
        {
            "due" => [Day(GoalToday, EvaluationState.Due), Day(new DateOnly(2026, 11, 17), EvaluationState.Pending)],
            "pending" => [Day(new DateOnly(2026, 11, 17), EvaluationState.Pending)],
            _ => [],
        };

        var cut = RenderGoal(OverviewOf(GoalAt("Every day", GoalScheduleKind.CommitmentBased), entries));

        var actions = cut.Find("div.ht-goalitem__actions");
        if (variant is null)
        {
            Assert.Empty(actions.Children);
            return;
        }

        var button = Assert.Single(actions.QuerySelectorAll("button"));
        Assert.Equal("Record", button.TextContent);
        Assert.Equal(["ht-btn", variant, "ht-btn--sm"], button.ClassList);
        Assert.Equal(("button", "dialog", "Record for Reading, 15 min, 5 days a week"),
            (button.GetAttribute("type"), button.GetAttribute("aria-haspopup"), button.GetAttribute("aria-label")));
    }

    [Fact]
    public void GoalItem_Record_raises_OnRecord_with_its_goal()
    {
        var goal = OverviewOf(GoalAt("Every day", GoalScheduleKind.CommitmentBased), Day(GoalToday, EvaluationState.Due));
        OverviewGoal? chosen = null;
        var cut = RenderGoal(goal, g => chosen = g);

        cut.Find(".ht-goalitem__actions button").Click();

        Assert.Same(goal, chosen);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GoalItem_FocusAsync_focuses_Record_or_the_name_link_without_it(bool canRecord)
    {
        HistoryEntry[] entries = canRecord ? [Day(GoalToday, EvaluationState.Due)] : [];
        var cut = RenderGoal(OverviewOf(GoalAt("Every day", GoalScheduleKind.CommitmentBased), entries));

        await cut.InvokeAsync(() => cut.Instance.FocusAsync().AsTask());

        var focused = Assert.IsType<Microsoft.AspNetCore.Components.ElementReference>(JSInterop.VerifyFocusAsyncInvoke().Arguments[0]);
        var expected = cut.Find(canRecord ? ".ht-goalitem__actions button" : "a.ht-goalitem__name");
        Assert.Equal(expected.GetAttribute("blazor:elementReference"), focused.Id);
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
