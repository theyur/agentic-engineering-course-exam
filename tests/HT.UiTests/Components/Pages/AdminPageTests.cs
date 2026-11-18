using Bunit;
using Bunit.TestDoubles;
using HT.Application.Goals;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Time;
using HT.Ui.Components.Pages;
using HT.UnitTests.Fakes;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using static HT.UnitTests.Fakes.HabitSamples;

namespace HT.UiTests.Components.Pages;

/// <summary>
/// US3-AS20, HP-009: the Admin page over the real service and the fake stores; slice 4 HP-008: the unpublish
/// confirmation Dialog for a habit that goals use (US7-AS1 to AS4).
/// </summary>
public sealed class AdminPageTests : BunitContext
{
    private const string KeepWorking = "The goals keep working for their owners. No new goal can be based on this habit until you publish it again.";

    private readonly FakeCatalogHabitStore _store;
    private readonly FakeGoalStore _goals;

    public AdminPageTests()
    {
        var authorization = AddAuthorization();
        authorization.SignIn("Administrator", "admin@example.com");
        _store = this.AddCatalog();
        _goals = this.AddGoals();
        _store.Add(Catalog("Cold shower", CatalogHabitState.Draft, Category.Body, icon: "droplet"));
        _store.Add(Catalog("Reading", CatalogHabitState.Published, Category.Mind));
        _store.Add(Catalog("Sugar", CatalogHabitState.Unpublished, Category.Health, Intent.Reduce, "candy"));
    }

    private static List<string> Cells(AngleSharp.Dom.IElement row) => row.QuerySelectorAll("td").Select(td => td.TextContent.Trim()).ToList();

    private CatalogHabit Habit(string name) => _store.Habits.Single(h => h.Definition.Name.Value == name);

    /// <summary>Stores <paramref name="count"/> goals on the catalog habit, each of its own owner, named "Morning pages".</summary>
    private void StoreGoals(string habit, int count)
    {
        var catalog = Habit(habit);
        var start = new DateOnly(2026, 11, 18);
        Assert.True(GoalTarget.TryCreate(new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo, ScheduleKind = ScheduleKind.Times, Times = 3, Period = SchedulePeriod.Week,
        }, out var target, out _));
        Assert.True(GoalName.TryCreate("Morning pages", out var name, out _));
        for (var i = 0; i < count; i++)
        {
            var owner = new AccountId(Guid.NewGuid());
            _goals.Add(owner,
                new Goal(new GoalId(Guid.NewGuid()), owner, new GoalHabit(catalog.Id, HabitKind.Catalog), start, name, new GoalAttempt(1, start),
                    [new GoalSettingsVersion(new GoalSettings(target, NamedTimeZone.Utc), start)]),
                new GoalHabitView(catalog.Id, HabitKind.Catalog, habit, catalog.Definition.Category, true));
        }
    }

    private static AngleSharp.Dom.IElement Row(IRenderedComponent<Admin> cut, string habit) =>
        cut.FindAll("tbody tr").Single(r => r.QuerySelector("td b")!.TextContent == habit);

    private static AngleSharp.Dom.IElement StateButton(IRenderedComponent<Admin> cut, string habit) =>
        Row(cut, habit).QuerySelector(".ht-actions button")!;

    private static string ReferenceOf(AngleSharp.Dom.IElement element)
    {
        var reference = element.GetAttribute("blazor:elementReference");
        Assert.False(string.IsNullOrEmpty(reference));
        return reference;
    }

    private List<string> FocusedReferences(int times) =>
        JSInterop.VerifyFocusAsyncInvoke(times).Select(i => Assert.IsType<ElementReference>(i.Arguments[0]).Id).ToList();

    [Fact]
    public void US3_AS20_the_Admin_page_follows_the_slice_2_preview()
    {
        var cut = Render<Admin>();

        Assert.Equal("Administration", cut.Find("h1").TextContent);
        var create = cut.Find(".ht-page-head a.ht-btn");
        Assert.Equal("+ New catalog habit", create.TextContent);
        Assert.Equal("/admin/catalog/new", create.GetAttribute("href"));
        Assert.Equal("Privacy: admins manage public content and accounts. Private goals, records, notes and personal reports are not available here.",
            cut.Find(".ht-notice--neutral span").TextContent);
        Assert.Equal("Search catalog", cut.Find("input[type=search]").GetAttribute("aria-label"));
        Assert.Contains("Showing 3 of 3", cut.Find(".ht-results-bar").TextContent, StringComparison.Ordinal);
        Assert.Equal(["Habit", "Category", "Intent", "Status", ""], cut.FindAll("th").Select(th => th.TextContent));
        Assert.Empty(cut.FindAll(".ht-tabs"));
        Assert.DoesNotContain("Updated", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Suggestions", cut.Markup, StringComparison.Ordinal);

        var rows = cut.FindAll("tbody tr");
        Assert.Equal(["Cold shower", "Reading", "Sugar"], rows.Select(r => r.QuerySelector("td b")!.TextContent));
        Assert.Equal(["Body", "Build", "Draft"], Cells(rows[0]).Skip(1).Take(3));
        Assert.Equal(["Edit", "Publish"], rows[0].QuerySelectorAll(".ht-actions .ht-btn").Select(b => b.TextContent));
        Assert.Equal(["Edit", "Unpublish"], rows[1].QuerySelectorAll(".ht-actions .ht-btn").Select(b => b.TextContent));
        Assert.Contains("ht-chip--strong", rows[1].QuerySelector(".ht-chip")!.ClassList);
        Assert.DoesNotContain("ht-chip--strong", rows[2].QuerySelector(".ht-chip")!.ClassList);
        Assert.Equal("Edit Cold shower", rows[0].QuerySelector(".ht-actions a")!.GetAttribute("aria-label"));
    }

    [Fact]
    public void A_new_q_on_the_open_page_loads_that_search()
    {
        var navigation = Services.GetRequiredService<BunitNavigationManager>();
        navigation.NavigateTo("/admin?q=read");
        var cut = Render<Admin>();
        Assert.Equal(["Reading"], cut.FindAll("tbody tr td b").Select(b => b.TextContent));
        Assert.Equal("read", cut.Find("input[type=search]").GetAttribute("value"));

        // The Admin link in the navigation: same page, no "q".
        navigation.NavigateTo("/admin");

        cut.WaitForAssertion(() => Assert.Equal(["Cold shower", "Reading", "Sugar"], cut.FindAll("tbody tr td b").Select(b => b.TextContent)));
        Assert.True(string.IsNullOrEmpty(cut.Find("input[type=search]").GetAttribute("value")));
        Assert.Contains("Showing 3 of 3", cut.Find(".ht-results-bar").TextContent, StringComparison.Ordinal);
    }

    [Fact]
    public void Publish_and_Unpublish_show_the_new_state_at_once()
    {
        var cut = Render<Admin>();

        cut.FindAll("tbody tr")[0].QuerySelector("button")!.Click();
        Assert.Equal("Published", cut.FindAll("tbody tr")[0].QuerySelector(".ht-chip")!.TextContent);
        Assert.Equal("Unpublish", cut.FindAll("tbody tr")[0].QuerySelector("button")!.TextContent);

        cut.FindAll("tbody tr")[1].QuerySelector("button")!.Click();
        Assert.Equal("Unpublished", cut.FindAll("tbody tr")[1].QuerySelector(".ht-chip")!.TextContent);
        Assert.Equal("Publish", cut.FindAll("tbody tr")[1].QuerySelector("button")!.TextContent);
        Assert.Empty(cut.FindAll("[role=dialog]"));
    }

    [Fact]
    public void The_search_lists_the_matching_habits_of_all()
    {
        var cut = Render<Admin>();

        cut.Find("input[type=search]").Input("read");

        cut.WaitForAssertion(() => Assert.Equal(["Reading"], cut.FindAll("tbody tr td b").Select(b => b.TextContent)), TimeSpan.FromSeconds(3));
        Assert.Contains("Showing 1 of 3", cut.Find(".ht-results-bar").TextContent, StringComparison.Ordinal);
    }

    [Fact]
    public void HP_008_Unpublish_of_a_habit_that_goals_use_opens_the_Dialog_with_the_count_only()
    {
        StoreGoals("Reading", 2);
        var cut = Render<Admin>();

        StateButton(cut, "Reading").Click();

        var dialog = cut.Find("[role=dialog]");
        Assert.Equal("Unpublish this habit?", dialog.QuerySelector("h2")!.TextContent);
        Assert.Equal(dialog.QuerySelector("h2")!.Id, dialog.GetAttribute("aria-labelledby"));
        Assert.Equal("Reading · Published", dialog.QuerySelector(".ht-dialog__sub")!.TextContent);
        Assert.Equal(["2 goals use this habit.", KeepWorking], dialog.QuerySelectorAll(".ht-dialog__body p").Select(p => p.TextContent));
        var actions = dialog.QuerySelectorAll(".ht-dialog__foot .ht-btn");
        Assert.Equal(["Unpublish", "Cancel"], actions.Select(b => b.TextContent));
        Assert.Contains("ht-btn--primary", actions[0].ClassList);
        Assert.Contains("ht-btn--secondary", actions[1].ClassList);
        Assert.DoesNotContain("Morning pages", dialog.TextContent, StringComparison.Ordinal);
        Assert.DoesNotContain("@", dialog.TextContent, StringComparison.Ordinal);
        Assert.Equal("Published", Row(cut, "Reading").QuerySelector(".ht-chip")!.TextContent);
        Assert.Equal(CatalogHabitState.Published, Habit("Reading").State);
        Assert.Equal(0, _store.SwapCount);
    }

    [Fact]
    public void US7_AS3_one_goal_gives_the_singular_text()
    {
        StoreGoals("Reading", 1);
        var cut = Render<Admin>();

        StateButton(cut, "Reading").Click();

        Assert.Equal("1 goal uses this habit.", cut.Find("[role=dialog] .ht-dialog__body p").TextContent);
    }

    [Fact]
    public void US7_AS2_Unpublish_in_the_Dialog_unpublishes_and_the_row_shows_Unpublished()
    {
        StoreGoals("Reading", 2);
        var cut = Render<Admin>();
        StateButton(cut, "Reading").Click();

        cut.Find("[role=dialog] .ht-dialog__foot .ht-btn--primary").Click();

        Assert.Empty(cut.FindAll("[role=dialog]"));
        Assert.Equal("Unpublished", Row(cut, "Reading").QuerySelector(".ht-chip")!.TextContent);
        Assert.Equal("Publish", StateButton(cut, "Reading").TextContent);
        Assert.Equal(CatalogHabitState.Unpublished, Habit("Reading").State);
        Assert.Equal(2, _goals.Goals.Count);
    }

    [Theory]
    [InlineData("Cancel")]
    [InlineData("Escape")]
    [InlineData("Close")]
    public void Cancel_Esc_and_the_close_button_change_nothing_and_give_the_focus_back_to_the_rows_Unpublish(string way)
    {
        StoreGoals("Reading", 2);
        var cut = Render<Admin>();
        var unpublish = ReferenceOf(StateButton(cut, "Reading"));
        StateButton(cut, "Reading").Click();
        var dialog = ReferenceOf(cut.Find("[role=dialog]"));

        switch (way)
        {
            case "Cancel":
                cut.Find("[role=dialog] .ht-dialog__foot .ht-btn--secondary").Click();
                break;
            case "Escape":
                cut.Find("[role=dialog]").KeyDown(new KeyboardEventArgs { Key = "Escape" });
                break;
            default:
                cut.Find("button.ht-dialog__close").Click();
                break;
        }

        Assert.Empty(cut.FindAll("[role=dialog]"));
        Assert.Equal("Published", Row(cut, "Reading").QuerySelector(".ht-chip")!.TextContent);
        Assert.Equal("Unpublish", StateButton(cut, "Reading").TextContent);
        Assert.Equal(CatalogHabitState.Published, Habit("Reading").State);
        Assert.Equal(0, _store.SwapCount);
        Assert.Equal([dialog, unpublish], FocusedReferences(2));
    }

    [Fact]
    public void US7_AS4_a_habit_no_goal_uses_is_unpublished_at_once_while_another_habit_has_goals()
    {
        _store.Add(Catalog("Yoga", CatalogHabitState.Published, Category.Body));
        StoreGoals("Yoga", 1);
        var cut = Render<Admin>();

        StateButton(cut, "Reading").Click();

        Assert.Empty(cut.FindAll("[role=dialog]"));
        Assert.Equal("Unpublished", Row(cut, "Reading").QuerySelector(".ht-chip")!.TextContent);
        Assert.Equal("Published", Row(cut, "Yoga").QuerySelector(".ht-chip")!.TextContent);
    }

    [Fact]
    public void Slice_4_keeps_the_table_without_an_Updated_column()
    {
        StoreGoals("Reading", 2);
        var cut = Render<Admin>();
        StateButton(cut, "Reading").Click();

        Assert.Equal(["Habit", "Category", "Intent", "Status", ""], cut.FindAll("th").Select(th => th.TextContent));
        Assert.DoesNotContain("Updated", cut.Markup, StringComparison.Ordinal);
        Assert.All(cut.FindAll("tbody tr"), r => Assert.Equal(5, r.QuerySelectorAll("td").Length));
    }
}
