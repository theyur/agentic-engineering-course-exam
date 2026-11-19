using Bunit;
using Bunit.TestDoubles;
using HT.Domain.Accounts;
using HT.Domain.Habits;
using HT.Ui.Components.Pages;
using HT.UnitTests.Fakes;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using static HT.UnitTests.Fakes.HabitSamples;

namespace HT.UiTests.Components.Pages;

/// <summary>
/// HP-001, HP-002, HP-012, US1-AS11, US8-AS3: the New goal page over the real GoalService and the fakes. Today is
/// Tuesday 17 November 2026 in UTC, the zone of the signed-in user (no stored zone, the UTC default).
/// </summary>
public sealed class NewGoalPageTests : BunitContext
{
    private readonly BunitAuthorizationContext _authorization;
    private readonly FakeCatalogHabitStore _catalog;
    private readonly FakeCustomHabitStore _customHabits;
    private readonly FakeGoalStore _goals;
    private readonly CatalogHabit _reading;
    private int _notFound;

    public NewGoalPageTests()
    {
        _authorization = AddAuthorization();
        _catalog = this.AddCatalog();
        _customHabits = this.AddCustomHabits();
        _goals = this.AddGoals();
        ((FakeTimeProvider)Services.GetRequiredService<TimeProvider>()).SetUtcNow(new DateTimeOffset(2026, 11, 17, 10, 0, 0, TimeSpan.Zero));
        _reading = _catalog.Add(Reading());
        _catalog.Add(Catalog("Yoga", category: Category.Body, icon: "person-standing", goals: HabitSamples.Goals(0, YesNo(3, SchedulePeriod.Week))));
        _catalog.Add(Catalog("Sugar", category: Category.Health, intent: Intent.Reduce, icon: "candy",
            goals: HabitSamples.Goals(0, Count(1, "sweet", 1, SchedulePeriod.Day, BuildOrReduce.Reduce))));
        _catalog.Add(Catalog("Old habit", CatalogHabitState.Unpublished, category: Category.Other));
        Navigation.OnNotFound += (_, _) => _notFound++;
    }

    private BunitNavigationManager Navigation => Services.GetRequiredService<BunitNavigationManager>();

    private AccountId SignInUser() => new(_authorization.SignIn("RegisteredUser"));

    private IRenderedComponent<NewGoal> RenderAt(string address)
    {
        Navigation.NavigateTo(address);
        return Render<NewGoal>();
    }

    private IRenderedComponent<NewGoal> RenderReading(string extra = "") =>
        RenderAt($"/goals/new?habit={_reading.Id.Value}&kind=catalog{extra}");

    private static string Text(AngleSharp.Dom.IElement element) =>
        string.Join(" ", element.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static AngleSharp.Dom.IElement Button(IRenderedComponent<NewGoal> cut, string text) =>
        cut.FindAll("button").First(b => Text(b) == text);

    private static AngleSharp.Dom.IElement Segment(IRenderedComponent<NewGoal> cut, string group, string text) =>
        cut.Find($".ht-seg[aria-label='{group}']").QuerySelectorAll("button").First(b => b.TextContent == text);

    private static AngleSharp.Dom.IElement Choice(IRenderedComponent<NewGoal> cut, string title) =>
        cut.FindAll("label.ht-choice").First(c => c.QuerySelector(".ht-choice__title")!.TextContent == title).QuerySelector("input")!;

    private static string TargetWord(IRenderedComponent<NewGoal> cut) =>
        cut.FindAll(".ht-form-section").First(s => s.QuerySelector("h2")?.TextContent == "Target").QuerySelector(".ht-inline > span")!.TextContent;

    private static string Sentence(IRenderedComponent<NewGoal> cut) => cut.Find("aside.ht-summary .ht-card__title").TextContent;

    private static List<string> SummaryValues(IRenderedComponent<NewGoal> cut) =>
        cut.FindAll("aside.ht-summary .ht-kv dd").Select(d => d.TextContent).ToList();

    private static List<string> Errors(IRenderedComponent<NewGoal> cut) =>
        cut.FindAll(".ht-field__error").Select(e => e.TextContent).ToList();

    private static string ReferenceOf(AngleSharp.Dom.IElement element)
    {
        var reference = element.GetAttribute("blazor:elementReference");
        Assert.False(string.IsNullOrEmpty(reference));
        return reference;
    }

    private List<string> FocusedReferences(int times) =>
        JSInterop.VerifyFocusAsyncInvoke(times).Select(i => Assert.IsType<ElementReference>(i.Arguments[0]).Id).ToList();

    [Fact]
    public void US8_AS3_the_head_line_for_a_catalog_habit_and_the_back_link_to_its_details()
    {
        SignInUser();

        var cut = RenderReading();

        Assert.Equal("New goal", cut.Find("h1").TextContent);
        Assert.Equal("Based on the catalog habit Reading. Change anything you like.", cut.Find(".ht-page-head .ht-muted").TextContent);
        var back = cut.Find("a.ht-back");
        Assert.Equal("← Reading", back.TextContent);
        Assert.Equal($"/catalog/{_reading.Id.Value}", back.GetAttribute("href"));
        Assert.Equal($"/catalog/{_reading.Id.Value}", cut.Find(".ht-form-actions a").GetAttribute("href"));
        var habit = cut.Find(".ht-form-section .ht-inline");
        Assert.Equal("Reading", habit.QuerySelector("b")!.TextContent);
        Assert.Equal(["Mind"], habit.QuerySelectorAll(".ht-chip").Select(c => c.TextContent));
        Assert.NotNull(habit.QuerySelector(".ht-habit__glyph svg"));
        var change = habit.QuerySelector("button")!;
        Assert.Equal("Change", change.TextContent);
        Assert.Equal("Change habit", change.GetAttribute("aria-label"));
        Assert.Equal("dialog", change.GetAttribute("aria-haspopup"));
        Assert.Equal(0, _notFound);
    }

    [Fact]
    public void US8_AS3_the_head_line_for_a_custom_habit_and_the_back_link_to_its_page()
    {
        var owner = SignInUser();
        var shower = _customHabits.Add(owner, Definition("Cold shower", Category.Body, icon: "droplet"));

        var cut = RenderAt($"/goals/new?habit={shower.Id.Value}&kind=custom");

        Assert.Equal("Based on your custom habit Cold shower. Change anything you like.", cut.Find(".ht-page-head .ht-muted").TextContent);
        var back = cut.Find("a.ht-back");
        Assert.Equal("← Cold shower", back.TextContent);
        Assert.Equal($"/custom-habits/{shower.Id.Value}", back.GetAttribute("href"));
        Assert.Equal(["Body", "Private"], cut.FindAll(".ht-form-section .ht-inline .ht-chip").Select(c => c.TextContent));
        Assert.Contains("ht-chip--strong", cut.FindAll(".ht-chip")[1].ClassList);
        Assert.Equal("true", Segment(cut, "Build or reduce", "Build").GetAttribute("aria-pressed"));
        Assert.All(cut.FindAll("input[type=radio]"), r => Assert.False(r.HasAttribute("checked")));
    }

    [Fact]
    public void US8_AS3_with_no_habit_the_Habit_section_says_to_choose_one_and_the_card_is_not_complete()
    {
        SignInUser();

        var cut = RenderAt("/goals/new");

        Assert.Equal("Start from a catalog habit or one of your custom habits.", cut.Find(".ht-page-head .ht-muted").TextContent);
        var back = cut.Find("a.ht-back");
        Assert.Equal("← Goals", back.TextContent);
        Assert.Equal("/goals", back.GetAttribute("href"));
        var habit = cut.FindAll(".ht-form-section")[0];
        Assert.Equal("Choose the habit this goal is for.", habit.QuerySelector(".ht-form-section__help")!.TextContent);
        var choose = habit.QuerySelector("button")!;
        Assert.Equal("Choose a habit", choose.TextContent);
        Assert.Contains("ht-btn--secondary", choose.ClassList);
        Assert.Equal("dialog", choose.GetAttribute("aria-haspopup"));
        Assert.All(cut.FindAll(".ht-seg[aria-label='Build or reduce'] button"), b => Assert.Equal("false", b.GetAttribute("aria-pressed")));
        Assert.Equal("2026-11-17", cut.Find("#new-goal-start").GetAttribute("value"));
        var summary = cut.Find("aside.ht-summary");
        Assert.Equal("Your goal", summary.QuerySelector(".ht-eyebrow")!.TextContent);
        Assert.Equal("Complete the form to see your goal here.", summary.QuerySelector("p")!.TextContent);
        Assert.Null(summary.QuerySelector(".ht-card__title"));
        Assert.Null(summary.QuerySelector(".ht-kv"));
        Assert.Null(summary.QuerySelector(".ht-notice"));
    }

    [Fact]
    public void HP_001_the_sections_and_their_wording_from_the_marked_suggestion()
    {
        SignInUser();

        var cut = RenderReading();

        Assert.Equal(["Habit", "Do you want to build or reduce it?", "How do you measure it?", "Target", "When?"],
            cut.FindAll(".ht-form-section__title").Select(t => t.TextContent));
        Assert.Equal("Build: do it at least the target. Reduce: stay at or below a limit.", cut.FindAll(".ht-form-section__help")[0].TextContent);
        Assert.Equal(["Build", "Reduce"], cut.FindAll(".ht-seg[aria-label='Build or reduce'] button").Select(b => b.TextContent));
        Assert.Equal("true", Segment(cut, "Build or reduce", "Build").GetAttribute("aria-pressed"));
        Assert.Equal(["Yes / no", "Count", "Duration", "On fixed days", "A number of times"],
            cut.FindAll(".ht-choice__title").Select(t => t.TextContent));
        Assert.Equal(["Did it or not", "Times, pages, cups", "Minutes or hours", "For example Mon to Fri", "For example 5 times a week"],
            cut.FindAll(".ht-choice__desc").Select(t => t.TextContent));
        Assert.True(Choice(cut, "Duration").HasAttribute("checked"));
        Assert.True(Choice(cut, "On fixed days").HasAttribute("checked"));
        Assert.Equal("At least", TargetWord(cut));
        var amount = cut.Find("#new-goal-amount");
        Assert.Equal("15", amount.GetAttribute("value"));
        Assert.Equal("1", amount.GetAttribute("min"));
        Assert.Equal("999", amount.GetAttribute("max"));
        Assert.Equal("Amount", amount.GetAttribute("aria-label"));
        Assert.Equal("true", Segment(cut, "Unit", "Minutes").GetAttribute("aria-pressed"));
        Assert.Equal([true, true, true, true, true, false, false],
            cut.FindAll(".ht-days button").Select(d => d.GetAttribute("aria-pressed") == "true"));
        Assert.Equal("Each selected day is one commitment.", cut.Find(".ht-form-section__note").TextContent);
        Assert.Equal(["Start date", "Name (optional)"], cut.FindAll(".ht-field-pair .ht-field__label").Select(Text));
        Assert.Equal("2026-11-17", cut.Find("#new-goal-start").GetAttribute("value"));
        Assert.Equal("date", cut.Find("#new-goal-start").GetAttribute("type"));
        Assert.True(string.IsNullOrEmpty(cut.Find("#new-goal-name").GetAttribute("value")));
        Assert.Equal(["Create goal", "Cancel"], cut.FindAll(".ht-form-actions .ht-btn").Select(b => b.TextContent));
        Assert.Equal("submit", cut.Find(".ht-form-actions button").GetAttribute("type"));
        Assert.Contains("ht-btn--secondary", cut.Find(".ht-form-actions a").ClassList);
        var measure = cut.Find("#new-goal-measure").ParentElement!;
        Assert.Equal(
            "You can record partial amounts; the target decides the result.",
            measure.QuerySelector(".ht-form-section__help")!.TextContent);
        Assert.Equal("ht-form-section__help", cut.Find("#new-goal-measure").NextElementSibling!.ClassName);
        Assert.DoesNotContain("excuse", cut.Markup, StringComparison.OrdinalIgnoreCase);

        Assert.Equal("Reading: at least 15 min, 5 days a week, starting Tue 17 Nov.", Sentence(cut));
        Assert.Equal(["Evaluated", "Met when", "Streak counted in", "Late records"],
            cut.FindAll("aside.ht-summary .ht-kv dt").Select(d => d.TextContent));
        Assert.Equal(["Each scheduled day", "At least 15 min that day", "Commitments", "Up to 48 h after each day"], SummaryValues(cut));
        var notice = cut.Find("aside.ht-summary .ht-notice");
        Assert.Contains("ht-notice--neutral", notice.ClassList);
        Assert.Equal("Catalog suggestion: 15 min, 5 days a week. An example to adjust, not a prescribed target.", notice.TextContent);
    }

    [Fact]
    public void US1_AS11_the_summary_card_rewrites_on_every_change_and_Reduce_says_At_most()
    {
        SignInUser();
        var cut = RenderReading();

        Segment(cut, "Build or reduce", "Reduce").Click();
        Assert.Equal("At most", TargetWord(cut));
        Assert.Equal("0", cut.Find("#new-goal-amount").GetAttribute("min"));
        Assert.Equal("Reading: at most 15 min, 5 days a week, starting Tue 17 Nov.", Sentence(cut));
        Assert.Equal("At most 15 min that day", SummaryValues(cut)[1]);

        Segment(cut, "Build or reduce", "Build").Click();
        Choice(cut, "Count").Change(true);
        cut.Find("#new-goal-amount").Change("20");
        cut.Find("#new-goal-unit").Change("pages");
        Assert.Equal("Reading: at least 20 pages, 5 days a week, starting Tue 17 Nov.", Sentence(cut));

        Choice(cut, "A number of times").Change(true);
        Assert.Equal("Complete the form to see your goal here.", cut.Find("aside.ht-summary p").TextContent);
        cut.Find("#new-goal-times").Change("3");
        Segment(cut, "Period", "Per week").Click();
        Assert.Equal("Each week, Monday to Sunday, is evaluated as a whole.", cut.Find(".ht-form-section__note").TextContent);
        Assert.Equal("times", cut.Find("#new-goal-times").ParentElement!.ParentElement!.QuerySelector(".ht-unit")!.TextContent);
        Assert.Equal("Reading: at least 20 pages, 3 times a week, starting Tue 17 Nov.", Sentence(cut));
        Assert.Equal(["Each week", "20 pages or more, at least 3 times that week", "Weeks", "Up to 48 h after each week"], SummaryValues(cut));
        Assert.Equal("Your first week starts on Mon 23 Nov. Days before it are not evaluated.",
            cut.FindAll("aside.ht-summary .ht-notice")[0].TextContent);
        Assert.Equal("Catalog suggestion: 15 min, 5 days a week. An example to adjust, not a prescribed target.",
            cut.FindAll("aside.ht-summary .ht-notice")[1].TextContent);

        cut.Find("#new-goal-start").Change("2026-11-23");
        Assert.Equal("Reading: at least 20 pages, 3 times a week, starting Mon 23 Nov.", Sentence(cut));
        Assert.Single(cut.FindAll("aside.ht-summary .ht-notice"));

        Choice(cut, "Yes / no").Change(true);
        Assert.Empty(cut.FindAll("#new-goal-amount"));
        Assert.Equal("Reading: 3 times a week, starting Mon 23 Nov.", Sentence(cut));
        Assert.Equal("Done at least 3 times that week", SummaryValues(cut)[1]);

        cut.Find("#new-goal-times").Change("1");
        Assert.Equal("time", cut.Find("#new-goal-times").ParentElement!.ParentElement!.QuerySelector(".ht-unit")!.TextContent);
        cut.Find("#new-goal-start").Change("");
        Assert.Equal("Complete the form to see your goal here.", cut.Find("aside.ht-summary p").TextContent);
    }

    [Fact]
    public void A_Reduce_amount_of_several_times_leaves_the_card_incomplete()
    {
        SignInUser();
        var cut = RenderAt($"/goals/new?habit={_catalog.Habits.Single(h => h.Definition.Name.Value == "Sugar").Id.Value}");

        Assert.Equal("Sugar: at most 1 sweet a day, starting Tue 17 Nov.", Sentence(cut));
        Assert.Equal(["Each day", "At most 1 sweet in total that day", "Days", "Up to 48 h after each day"], SummaryValues(cut));

        cut.Find("#new-goal-times").Change("2");

        Assert.Equal("Complete the form to see your goal here.", cut.Find("aside.ht-summary p").TextContent);
    }

    [Fact]
    public void A_refusal_sits_under_its_field_one_at_a_time()
    {
        SignInUser();
        var cut = RenderReading();

        cut.Find("#new-goal-start").Change("2026-11-16");
        Button(cut, "Create goal").Click();

        Assert.Equal(["The start date cannot be in the past."], Errors(cut));
        var start = cut.Find("#new-goal-start");
        Assert.Equal("true", start.GetAttribute("aria-invalid"));
        Assert.Equal(cut.Find(".ht-field__error").Id, start.GetAttribute("aria-describedby"));
        Assert.Equal("label", cut.Find(".ht-field__error").ParentElement!.LocalName);
        Assert.Empty(_goals.Goals);
    }

    [Fact]
    public void The_form_leaves_the_checks_to_the_use_case_so_its_refusal_text_is_shown()
    {
        // The Stepper's min and max would otherwise let the browser block "Create goal" with its own message
        // (HP-001: the refusal sits under its field, in the spec's words).
        SignInUser();
        var cut = RenderReading();

        Assert.True(cut.Find("form").HasAttribute("novalidate"));
    }

    [Fact]
    public void Refusals_follow_the_fields_the_use_case_names()
    {
        SignInUser();
        var cut = RenderReading();

        cut.Find("#new-goal-amount").Change("");
        Button(cut, "Create goal").Click();
        Assert.Equal(["Enter a whole number from 1 to 999."], Errors(cut));
        Assert.Equal("true", cut.Find("#new-goal-amount").GetAttribute("aria-invalid"));
        Assert.Null(cut.Find("#new-goal-start").GetAttribute("aria-invalid"));

        cut.Find("#new-goal-amount").Change("15");
        foreach (var _ in Enumerable.Range(0, 5))
        {
            cut.FindAll(".ht-days button").First(d => d.GetAttribute("aria-pressed") == "true").Click();
        }

        Button(cut, "Create goal").Click();
        Assert.Equal(["Choose at least one day."], Errors(cut));
        Assert.Equal(cut.Find(".ht-field__error").Id, cut.Find(".ht-days").GetAttribute("aria-describedby"));
        Assert.Null(cut.Find("#new-goal-amount").GetAttribute("aria-invalid"));

        Segment(cut, "Build or reduce", "Reduce").Click();
        Choice(cut, "Count").Change(true);
        cut.Find("#new-goal-unit").Change("cups");
        Choice(cut, "A number of times").Change(true);
        cut.Find("#new-goal-times").Change("2");
        Segment(cut, "Period", "Per day").Click();
        Button(cut, "Create goal").Click();
        Assert.Equal(["A Reduce goal with an amount has 1 time per period."], Errors(cut));
        Assert.Equal("true", cut.Find("#new-goal-times").GetAttribute("aria-invalid"));

        cut.Find("#new-goal-times").Change("1");
        cut.Find("#new-goal-name").Change(new string('n', 61));
        Button(cut, "Create goal").Click();
        Assert.Equal(["The name can have at most 60 characters."], Errors(cut));
        Assert.Equal("true", cut.Find("#new-goal-name").GetAttribute("aria-invalid"));
        Assert.Empty(_goals.Goals);
    }

    [Fact]
    public void With_no_habit_Create_goal_says_to_choose_one_under_Habit()
    {
        SignInUser();
        var cut = RenderAt("/goals/new");

        Button(cut, "Create goal").Click();

        Assert.Equal(["Choose a habit."], Errors(cut));
        var error = cut.Find(".ht-field__error");
        Assert.Equal(error.Id, Button(cut, "Choose a habit").GetAttribute("aria-describedby"));
        Assert.Equal("Habit", error.ParentElement!.QuerySelector("h2")!.TextContent);
    }

    [Fact]
    public void The_habit_chooser_opens_with_focus_in_its_search_lists_both_kinds_and_searches()
    {
        var owner = SignInUser();
        _customHabits.Add(owner, Definition("No phone in bed", Category.Home, Intent.Reduce, "smartphone"));
        _customHabits.Add(owner, Definition("Cold shower", Category.Body, icon: "droplet"));
        var cut = RenderAt("/goals/new");

        Button(cut, "Choose a habit").Click();

        var dialog = cut.Find("[role=dialog]");
        Assert.Equal("Choose a habit", dialog.QuerySelector("h2")!.TextContent);
        Assert.Equal("Catalog habits and your custom habits", dialog.QuerySelector(".ht-dialog__sub")!.TextContent);
        var search = cut.Find("[role=dialog] input[type=search]");
        Assert.Equal("Search habits", search.GetAttribute("placeholder"));
        Assert.Equal([ReferenceOf(search)], FocusedReferences(1));
        Assert.Equal(["Catalog", "Your custom habits"], cut.FindAll("[role=dialog] .ht-eyebrow").Select(e => e.TextContent));
        var lists = cut.FindAll("[role=dialog] ul.ht-pick-list");
        Assert.Equal(["Reading", "Sugar", "Yoga"], lists[0].QuerySelectorAll(".ht-row__name").Select(n => n.TextContent));
        Assert.Equal(["Mind · Build", "Health · Reduce", "Body · Build"], lists[0].QuerySelectorAll(".ht-row__meta").Select(n => n.TextContent));
        Assert.Equal(["Cold shower", "No phone in bed"], lists[1].QuerySelectorAll(".ht-row__name").Select(n => n.TextContent));
        Assert.Equal(["Body · Build", "Home · Reduce"], lists[1].QuerySelectorAll(".ht-row__meta").Select(n => n.TextContent));
        Assert.Equal(["Cancel"], cut.FindAll("[role=dialog] .ht-dialog__foot button").Select(b => b.TextContent));

        cut.Find("[role=dialog] input[type=search]").Input("  PHONE ");
        Assert.Equal(["Your custom habits"], cut.FindAll("[role=dialog] .ht-eyebrow").Select(e => e.TextContent));
        Assert.Equal(["No phone in bed"], cut.FindAll("[role=dialog] .ht-row__name").Select(n => n.TextContent));

        cut.Find("[role=dialog] input[type=search]").Input("rea");
        Assert.Equal(["Reading"], cut.FindAll("[role=dialog] .ht-row__name").Select(n => n.TextContent));

        cut.Find("[role=dialog] input[type=search]").Input("chess");
        Assert.Empty(cut.FindAll("[role=dialog] .ht-pick-list"));
        Assert.Equal("No habit matches your search.", cut.Find("[role=dialog] .ht-dialog__body p").TextContent);
    }

    [Fact]
    public void Esc_closes_the_chooser_with_no_change_and_focus_returns_to_the_opening_button()
    {
        SignInUser();
        var cut = RenderAt("/goals/new");
        var opener = ReferenceOf(Button(cut, "Choose a habit"));
        Button(cut, "Choose a habit").Click();
        var search = ReferenceOf(cut.Find("[role=dialog] input[type=search]"));

        cut.Find("[role=dialog]").KeyDown(new KeyboardEventArgs { Key = "Escape" });

        Assert.Empty(cut.FindAll("[role=dialog]"));
        Assert.Equal([search, opener], FocusedReferences(2));
        Assert.Equal("Start from a catalog habit or one of your custom habits.", cut.Find(".ht-page-head .ht-muted").TextContent);
    }

    [Fact]
    public void Choosing_a_habit_starts_the_form_from_its_values_and_focus_goes_to_Change()
    {
        SignInUser();
        var cut = RenderAt("/goals/new");
        Button(cut, "Choose a habit").Click();

        cut.FindAll("[role=dialog] button.ht-select-row").First(b => b.QuerySelector(".ht-row__name")!.TextContent == "Yoga").Click();

        Assert.Empty(cut.FindAll("[role=dialog]"));
        Assert.Equal("Based on the catalog habit Yoga. Change anything you like.", cut.Find(".ht-page-head .ht-muted").TextContent);
        Assert.Equal("← Goals", cut.Find("a.ht-back").TextContent);
        var change = Button(cut, "Change");
        Assert.Equal(ReferenceOf(change), FocusedReferences(2)[1]);
        Assert.True(Choice(cut, "Yes / no").HasAttribute("checked"));
        Assert.Equal("Yoga: 3 times a week, starting Tue 17 Nov.", Sentence(cut));
        Assert.Equal("Catalog suggestion: 3 times a week. An example to adjust, not a prescribed target.",
            cut.FindAll("aside.ht-summary .ht-notice")[^1].TextContent);
    }

    [Theory]
    [InlineData("?habit={unknown}", "This habit is not in the catalog.")]
    [InlineData("?habit={unknown}&kind=catalog", "This habit is not in the catalog.")]
    [InlineData("?habit=not-a-guid", "This habit is not in the catalog.")]
    [InlineData("?habit={old}&kind=catalog", "This habit is not in the catalog.")]
    [InlineData("?habit={unknown}&kind=custom", "This custom habit does not exist.")]
    [InlineData("?habit={reading}&kind=catalog&suggestion=3", "This suggested starting goal does not exist.")]
    [InlineData("?habit={reading}&kind=catalog&suggestion=two", "This suggested starting goal does not exist.")]
    public void A_refused_address_value_shows_its_text_under_Habit_and_opens_with_no_habit(string query, string message)
    {
        SignInUser();
        var address = "/goals/new" + query
            .Replace("{unknown}", Guid.NewGuid().ToString(), StringComparison.Ordinal)
            .Replace("{reading}", _reading.Id.Value.ToString(), StringComparison.Ordinal)
            .Replace("{old}", _catalog.Habits.Single(h => h.State == CatalogHabitState.Unpublished).Id.Value.ToString(), StringComparison.Ordinal);

        var cut = RenderAt(address);

        Assert.Equal([message], Errors(cut));
        Assert.Equal("Habit", cut.Find(".ht-field__error").ParentElement!.QuerySelector("h2")!.TextContent);
        Assert.Equal("Start from a catalog habit or one of your custom habits.", cut.Find(".ht-page-head .ht-muted").TextContent);
        Assert.Equal(cut.Find(".ht-field__error").Id, Button(cut, "Choose a habit").GetAttribute("aria-describedby"));
        Assert.Equal("← Goals", cut.Find("a.ht-back").TextContent);
        Assert.Equal("2026-11-17", cut.Find("#new-goal-start").GetAttribute("value"));
        Assert.Equal(0, _notFound);
    }

    [Fact]
    public void A_suggestion_position_in_the_address_starts_from_that_suggestion()
    {
        SignInUser();

        var cut = RenderReading("&suggestion=2");

        Assert.Equal("Reading: at least 20 pages, 3 times a week, starting Tue 17 Nov.", Sentence(cut));
        Assert.Equal("pages", cut.Find("#new-goal-unit").GetAttribute("value"));
        Assert.Empty(Errors(cut));
    }

    [Fact]
    public void Create_goal_creates_it_and_opens_its_page()
    {
        var owner = SignInUser();
        var cut = RenderReading();
        cut.Find("#new-goal-name").Change("  Morning pages  ");

        Button(cut, "Create goal").Click();

        var goal = Assert.Single(_goals.Goals);
        Assert.Equal(owner, goal.Owner);
        Assert.Equal("Morning pages", goal.OwnName!.Value.Value);
        Assert.Equal(new DateOnly(2026, 11, 17), goal.StartDate);
        Assert.EndsWith($"/goals/{goal.Id.Value}", Navigation.Uri, StringComparison.Ordinal);
    }

    [Fact]
    public void An_administrator_gets_the_Not_found_page()
    {
        _authorization.SignIn("Administrator", "admin@example.com");

        var cut = RenderReading();

        Assert.Equal(1, _notFound);
        Assert.Empty(cut.FindAll("form"));
        Assert.DoesNotContain("Reading", cut.Markup, StringComparison.Ordinal);
    }
}
