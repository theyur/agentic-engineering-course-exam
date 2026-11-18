using Bunit;
using Bunit.TestDoubles;
using HT.Application.Goals;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Time;
using HT.Ui.Components.Pages;
using HT.UnitTests.Fakes;
using Microsoft.Extensions.DependencyInjection;
using static HT.UnitTests.Fakes.HabitSamples;

namespace HT.UiTests.Components.Pages;

/// <summary>
/// US1-AS10, US1-AS11, HP-001 to HP-004, and slice 4 US6-AS2, US6-AS5, HP-005: the Catalog page over the real
/// CatalogService and the fake stores.
/// </summary>
public sealed class CatalogPageTests : BunitContext
{
    private readonly BunitAuthorizationContext _authorization;
    private readonly FakeCatalogHabitStore _store;
    private readonly FakeCustomHabitStore _customHabits;
    private readonly FakeGoalStore _goals;

    public CatalogPageTests()
    {
        _authorization = AddAuthorization();
        _store = this.AddCatalog();
        _store.Add(Catalog("Reading", category: Category.Mind, description: "Books.", goals: Goals(0, Count(20, "pages", 3, SchedulePeriod.Week))));
        _store.Add(Catalog("Saving money", category: Category.Money, icon: "piggy-bank"));
        _store.Add(Catalog("Sugar", category: Category.Health, intent: Intent.Reduce, icon: "candy"));
        _store.Add(Catalog("Old habit", CatalogHabitState.Unpublished, category: Category.Other));
        _customHabits = this.AddCustomHabits();
        _goals = this.AddGoals();
    }

    private BunitNavigationManager Navigation => Services.GetRequiredService<BunitNavigationManager>();

    private IRenderedComponent<Catalog> RenderAt(string address)
    {
        Navigation.NavigateTo(address);
        return Render<Catalog>();
    }

    private static List<string> Names(IRenderedComponent<Catalog> cut) =>
        cut.FindAll(".ht-habit__name").Select(n => n.TextContent).ToList();

    [Fact]
    public void US1_AS10_the_visitor_page_follows_the_preview()
    {
        var cut = RenderAt("/catalog");

        Assert.Equal("Habit catalog", cut.Find("h1").TextContent);
        Assert.Equal("Search habits", cut.Find(".ht-page-head input[type=search]").GetAttribute("aria-label"));
        var notice = cut.Find(".ht-notice");
        Assert.Equal("Ideas to start from. Register to turn any habit into your own goal.", notice.QuerySelector("span")!.TextContent);
        Assert.Equal("/register", notice.QuerySelector("a")!.GetAttribute("href"));
        Assert.Equal(["Category", "Intent"], cut.FindAll(".ht-filter__title").Select(t => t.TextContent));
        Assert.Equal(["All", "Mind", "Health", "Money"], cut.FindAll("[role=radiogroup] label").Select(l => l.TextContent.Trim()));
        Assert.Equal(["3 habits", "Sort: A–Z"], cut.FindAll(".ht-results-bar span").Select(s => s.TextContent).Where(t => t.Length > 0));
        Assert.Empty(cut.FindAll(".ht-results-bar button, .ht-results-bar select"));
        Assert.Equal(["Reading", "Saving money", "Sugar"], Names(cut));
        Assert.All(cut.FindAll(".ht-habit__actions a"), a => Assert.Equal("See details", a.TextContent));
        Assert.StartsWith("/catalog/", cut.Find(".ht-habit__actions a").GetAttribute("href"), StringComparison.Ordinal);
        Assert.Equal("Suggested starting goal: 20 pages, 3 times a week", cut.Find(".ht-habit__start").TextContent);
        Assert.Single(cut.FindAll(".ht-habit__start"));
    }

    [Fact]
    public void US1_AS11_a_category_and_an_intent_change_the_cards_at_once_and_replace_the_address()
    {
        var cut = RenderAt("/catalog");

        cut.FindAll("[role=radiogroup] input")[2].Change(true);
        Assert.Equal(["Sugar"], Names(cut));
        Assert.EndsWith("/catalog?category=health", Navigation.Uri, StringComparison.Ordinal);
        Assert.True(Navigation.History.First().Options.ReplaceHistoryEntry);

        cut.FindAll("[role=radiogroup] input")[0].Change(true);
        cut.FindAll("input[type=checkbox]")[1].Change(true);
        Assert.Equal(["Sugar"], Names(cut));
        Assert.EndsWith("/catalog?intent=reduce", Navigation.Uri, StringComparison.Ordinal);
        Assert.Empty(cut.FindAll("aside button"));
    }

    [Fact]
    public void Both_intent_boxes_or_none_show_both_intents_and_drop_intent_from_the_address()
    {
        var cut = RenderAt("/catalog?intent=build");
        Assert.Equal(["Reading", "Saving money"], Names(cut));

        cut.FindAll("input[type=checkbox]")[1].Change(true);
        Assert.Equal(["Reading", "Saving money", "Sugar"], Names(cut));
        Assert.DoesNotContain("intent", Navigation.Uri, StringComparison.Ordinal);

        cut.FindAll("input[type=checkbox]")[0].Change(false);
        Assert.Equal(["Sugar"], Names(cut));
        cut.FindAll("input[type=checkbox]")[1].Change(false);
        Assert.Equal(["Reading", "Saving money", "Sugar"], Names(cut));
        Assert.DoesNotContain("intent", Navigation.Uri, StringComparison.Ordinal);
    }

    [Fact]
    public void Query_values_set_the_filters_on_open()
    {
        var cut = RenderAt("/catalog?category=money&intent=build&q=sav");

        Assert.Equal(["Saving money"], Names(cut));
        Assert.True(cut.FindAll("[role=radiogroup] input")[3].HasAttribute("checked"));
        Assert.True(cut.FindAll("input[type=checkbox]")[0].HasAttribute("checked"));
        Assert.Equal("sav", cut.Find("input[type=search]").GetAttribute("value"));
    }

    [Fact]
    public void An_unknown_query_value_is_ignored()
    {
        var cut = RenderAt("/catalog?category=Money&intent=both");

        Assert.Equal(["Reading", "Saving money", "Sugar"], Names(cut));
        Assert.True(cut.FindAll("[role=radiogroup] input")[0].HasAttribute("checked"));
    }

    [Fact]
    public void The_search_applies_after_a_pause_and_writes_the_address()
    {
        var cut = RenderAt("/catalog");

        cut.Find("input[type=search]").Input("read");
        Assert.Equal(3, Names(cut).Count);

        cut.WaitForAssertion(() => Assert.Equal(["Reading"], Names(cut)), TimeSpan.FromSeconds(3));
        Assert.EndsWith("/catalog?q=read", Navigation.Uri, StringComparison.Ordinal);
    }

    [Fact]
    public void A_newer_keystroke_cancels_the_pending_search()
    {
        var cut = RenderAt("/catalog");

        cut.Find("input[type=search]").Input("read");
        cut.Find("input[type=search]").Input("sug");

        cut.WaitForAssertion(() => Assert.Equal(["Sugar"], Names(cut)), TimeSpan.FromSeconds(3));
        Assert.EndsWith("/catalog?q=sug", Navigation.Uri, StringComparison.Ordinal);
    }

    [Fact]
    public void HP_004_no_match_shows_the_EmptyState_and_clears_the_filters()
    {
        var cut = RenderAt("/catalog?category=money&intent=reduce");

        Assert.Empty(Names(cut));
        Assert.Equal("0 habits", cut.Find(".ht-results-bar span").TextContent);
        Assert.Equal("No habits match", cut.Find(".ht-empty h3").TextContent);

        cut.Find(".ht-empty button").Click();

        Assert.Equal(["Reading", "Saving money", "Sugar"], Names(cut));
        Assert.EndsWith("/catalog", Navigation.Uri, StringComparison.Ordinal);
    }

    [Fact]
    public void HP_002_an_administrator_gets_Details_only_and_no_register_Notice()
    {
        _authorization.SignIn("Administrator", "admin@example.com");

        var cut = RenderAt("/catalog");

        Assert.Empty(cut.FindAll(".ht-notice"));
        Assert.All(cut.FindAll(".ht-habit__actions a"), a => Assert.Equal("Details", a.TextContent));
        Assert.Empty(cut.FindAll(".ht-habit__head .ht-chip"));
        Assert.Equal("3 habits", cut.Find(".ht-results-bar span").TextContent);
    }

    /// <summary>Stores goals of <paramref name="owner"/> on <paramref name="habit"/> directly, starting today.</summary>
    private void StoreGoals(AccountId owner, string habit, int count)
    {
        var catalog = _store.Habits.Single(h => h.Definition.Name.Value == habit);
        var start = new DateOnly(2026, 11, 18);
        Assert.True(GoalTarget.TryCreate(new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo, ScheduleKind = ScheduleKind.Times, Times = 3, Period = SchedulePeriod.Week,
        }, out var target, out _));
        for (var i = 0; i < count; i++)
        {
            _goals.Add(owner,
                new Goal(new GoalId(Guid.NewGuid()), owner, new GoalHabit(catalog.Id, HabitKind.Catalog), start, null, new GoalAttempt(1, start),
                    [new GoalSettingsVersion(new GoalSettings(target, NamedTimeZone.Utc), start)]),
                new GoalHabitView(catalog.Id, HabitKind.Catalog, habit, catalog.Definition.Category, true));
        }
    }

    private void SignInRegisteredUserWithGoals()
    {
        var owner = new AccountId(_authorization.SignIn("RegisteredUser"));
        StoreGoals(owner, "Reading", 2);
        StoreGoals(new AccountId(Guid.NewGuid()), "Sugar", 1);
    }

    private static List<(string Name, string? Tag, string Actions)> Cards(IRenderedComponent<Catalog> cut) =>
        cut.FindAll(".ht-habit").Select(card => (
            card.QuerySelector(".ht-habit__name")!.TextContent,
            card.QuerySelector(".ht-habit__head .ht-chip")?.TextContent,
            string.Join(" | ", card.QuerySelectorAll(".ht-habit__actions a").Select(a => a.TextContent)))).ToList();

    [Fact]
    public void US6_AS5_a_registered_user_sees_their_goal_counts_goal_actions_the_Show_option_and_the_tracked_count()
    {
        SignInRegisteredUserWithGoals();
        var reading = _store.Habits.Single(h => h.Definition.Name.Value == "Reading");

        var cut = RenderAt("/catalog");

        Assert.Equal(
            [("Reading", (string?)"2 goals", "Add another goal | Details"), ("Saving money", null, "Use this habit | Details"), ("Sugar", null, "Use this habit | Details")],
            Cards(cut));
        Assert.Contains("ht-chip--strong", cut.Find(".ht-habit__head .ht-chip").ClassList);
        Assert.All(cut.FindAll(".ht-habit__actions a.ht-btn--primary, .ht-habit__actions a.ht-btn--secondary"),
            a => Assert.StartsWith("/goals/new?habit=", a.GetAttribute("href"), StringComparison.Ordinal));
        Assert.Equal($"/goals/new?habit={reading.Id.Value:D}&kind=catalog", cut.Find(".ht-habit__actions a.ht-btn--secondary").GetAttribute("href"));
        Assert.Equal("3 habits · you track 1", cut.Find(".ht-results-bar span").TextContent);
        Assert.Equal(["Category", "Intent", "Show"], cut.FindAll(".ht-filter__title").Select(t => t.TextContent));
        Assert.Contains("Only habits I track", cut.Find("aside").TextContent, StringComparison.Ordinal);
    }

    [Fact]
    public void US6_AS5_one_goal_is_1_goal()
    {
        var owner = new AccountId(_authorization.SignIn("RegisteredUser"));
        StoreGoals(owner, "Sugar", 1);

        var cut = RenderAt("/catalog");

        Assert.Equal(("Sugar", (string?)"1 goal", "Add another goal | Details"), Cards(cut)[2]);
    }

    [Fact]
    public void US6_AS2_Only_habits_I_track_lists_the_tracked_habits_and_is_kept_in_the_address()
    {
        SignInRegisteredUserWithGoals();
        var cut = RenderAt("/catalog");

        cut.FindAll("input[type=checkbox]")[2].Change(true);

        Assert.Equal(["Reading"], Names(cut));
        Assert.Equal("1 habit · you track 1", cut.Find(".ht-results-bar span").TextContent);
        Assert.EndsWith("/catalog?tracked=true", Navigation.Uri, StringComparison.Ordinal);
        Assert.True(Navigation.History.First().Options.ReplaceHistoryEntry);

        cut.FindAll("input[type=checkbox]")[2].Change(false);

        Assert.Equal(["Reading", "Saving money", "Sugar"], Names(cut));
        Assert.EndsWith("/catalog", Navigation.Uri, StringComparison.Ordinal);
    }

    [Fact]
    public void US6_AS2_tracked_true_in_the_address_checks_the_box_with_the_other_filters()
    {
        SignInRegisteredUserWithGoals();

        var cut = RenderAt("/catalog?tracked=true&category=mind");

        Assert.Equal(["Reading"], Names(cut));
        Assert.True(cut.FindAll("input[type=checkbox]")[2].HasAttribute("checked"));

        var empty = RenderAt("/catalog?tracked=true&category=money");
        Assert.Empty(Names(empty));
        Assert.Equal("No habits match", empty.Find(".ht-empty h3").TextContent);

        empty.Find(".ht-empty button").Click();

        Assert.Equal(["Reading", "Saving money", "Sugar"], Names(empty));
        Assert.False(empty.FindAll("input[type=checkbox]")[2].HasAttribute("checked"));
        Assert.EndsWith("/catalog", Navigation.Uri, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Administrator")]
    public void HP_005_visitors_and_administrators_ignore_tracked_true_and_have_no_Show_option(string? role)
    {
        if (role is not null)
        {
            _authorization.SignIn(role, "admin@example.com");
        }

        StoreGoals(new AccountId(Guid.NewGuid()), "Reading", 2);

        var cut = RenderAt("/catalog?tracked=true");

        Assert.Equal(["Reading", "Saving money", "Sugar"], Names(cut));
        Assert.Equal(["Category", "Intent"], cut.FindAll(".ht-filter__title").Select(t => t.TextContent));
        Assert.Equal("3 habits", cut.Find(".ht-results-bar span").TextContent);
        Assert.DoesNotContain("Only habits I track", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("goal", string.Join(" ", cut.FindAll(".ht-habit__actions, .ht-habit__head").Select(e => e.TextContent)), StringComparison.Ordinal);
    }

    [Fact]
    public void US4_AS12_a_registered_user_gets_the_Tabs_with_counts_and_the_custom_habit_button()
    {
        var owner = new HT.Domain.Accounts.AccountId(_authorization.SignIn("RegisteredUser"));
        _customHabits.Add(owner, Definition("Cold shower", Category.Body));
        _customHabits.Add(new HT.Domain.Accounts.AccountId(Guid.NewGuid()), Definition("Someone else's habit"));

        var cut = RenderAt("/catalog?category=money");

        var create = cut.Find(".ht-page-head a.ht-btn");
        Assert.Equal("+ Create custom habit", create.TextContent);
        Assert.Equal("/custom-habits/new", create.GetAttribute("href"));
        Assert.Contains("ht-btn--secondary", create.ClassList);
        var tabs = cut.Find("nav.ht-tabs");
        Assert.Equal("Catalog views", tabs.GetAttribute("aria-label"));
        var links = tabs.QuerySelectorAll("a.ht-tab");
        Assert.Equal(["/catalog", "/custom-habits"], links.Select(a => a.GetAttribute("href")));
        Assert.Equal(["page", null], links.Select(a => a.GetAttribute("aria-current")));
        // "Public catalog" counts every Published habit, whatever the filters; "My custom habits" the user's own.
        Assert.Equal(["Public catalog3", "My custom habits1"], links.Select(a => a.TextContent));
        Assert.Equal("1 habit · you track 0", cut.Find(".ht-results-bar span").TextContent);
        Assert.DoesNotContain("Cold shower", cut.Markup, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Administrator")]
    public void HP_002_visitors_and_administrators_get_no_Tabs_and_no_custom_habit_button(string? role)
    {
        if (role is not null)
        {
            _authorization.SignIn(role, "admin@example.com");
        }

        var cut = RenderAt("/catalog");

        Assert.Empty(cut.FindAll(".ht-tabs"));
        Assert.DoesNotContain("Create custom habit", cut.Markup, StringComparison.Ordinal);
        Assert.Equal(role is null ? 1 : 0, cut.FindAll(".ht-notice").Count);
    }
}
