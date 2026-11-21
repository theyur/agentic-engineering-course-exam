using Bunit;
using Bunit.TestDoubles;
using HT.Application.Exemptions;
using HT.Application.Goals;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Ui.Components.Pages;
using HT.UnitTests.Fakes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace HT.UiTests.Components.Pages;

/// <summary>
/// HP-003, US2-AS6, US7-AS8: the Goals page over the real GoalService and the fakes. Today is Tuesday 17 November 2026
/// in UTC, the zone of the signed-in user (no stored zone, the UTC default), as in the design's preview.
/// </summary>
public sealed class GoalsPageTests : BunitContext
{
    private static readonly DateOnly Today = new(2026, 11, 17);

    private readonly BunitAuthorizationContext _authorization;
    private readonly FakeCatalogHabitStore _catalog;
    private int _notFound;

    public GoalsPageTests()
    {
        _authorization = AddAuthorization();
        _catalog = this.AddCatalog();
        ((FakeTimeProvider)Services.GetRequiredService<TimeProvider>()).SetUtcNow(new DateTimeOffset(2026, 11, 17, 10, 0, 0, TimeSpan.Zero));
        Services.GetRequiredService<BunitNavigationManager>().OnNotFound += (_, _) => _notFound++;
    }

    private ActingUser SignInUser() => ActingUser.ForAccount(new AccountId(_authorization.SignIn("RegisteredUser")), Role.RegisteredUser);

    private async Task<GoalId> CreateAsync(ActingUser user, CatalogHabit habit, GoalTargetFields target, DateOnly start)
    {
        await using var scope = Services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<GoalService>().CreateAsync(
            user, new GoalInput { Habit = new GoalHabit(habit.Id, HabitKind.Catalog), Target = target, StartDate = start }, CancellationToken.None);
        Assert.Equal(GoalCreateOutcome.Created, result.Outcome);
        return result.GoalId!.Value;
    }

    private static string Text(AngleSharp.Dom.IElement element) =>
        string.Join(" ", element.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static GoalTargetFields Days(BuildOrReduce side, Measurement measurement, int? amount, string? countUnit, DurationUnit? durationUnit, params DayOfWeek[] days) =>
        new()
        {
            BuildOrReduce = side, Measurement = measurement, Amount = amount, CountUnit = countUnit, DurationUnit = durationUnit,
            ScheduleKind = ScheduleKind.FixedDays, Days = days.ToHashSet(),
        };

    private static GoalTargetFields Times(BuildOrReduce side, Measurement measurement, int? amount, string? countUnit, int times, SchedulePeriod period) =>
        new()
        {
            BuildOrReduce = side, Measurement = measurement, Amount = amount, CountUnit = countUnit,
            ScheduleKind = ScheduleKind.Times, Times = times, Period = period,
        };

    /// <summary>The Goals page's link Tabs as (text, href, aria-current), under nav.ht-tabs labelled "Goal lists".</summary>
    private static (string Text, string? Href, string? Current)[] TabsOf(IRenderedComponent<Goals> cut)
    {
        var nav = cut.Find("nav.ht-tabs");
        Assert.Equal("Goal lists", nav.GetAttribute("aria-label"));
        return nav.QuerySelectorAll("a.ht-tab").Select(a => (a.TextContent, a.GetAttribute("href"), a.GetAttribute("aria-current"))).ToArray();
    }

    /// <summary>Pauses <paramref name="goal"/> through the PauseService at the clock's time, from today until Sunday 22 November.</summary>
    private async Task PauseUntilSundayAsync(ActingUser user, GoalId goal)
    {
        await using var scope = Services.CreateAsyncScope();
        var paused = await scope.ServiceProvider.GetRequiredService<PauseService>().PauseAsync(
            user, goal, Today, new DateOnly(2026, 11, 22), null, CancellationToken.None);
        Assert.Equal(PauseOutcome.Paused, paused.Outcome);
    }

    private const string Footnote =
        "Each goal is counted on its own. Consistency counts only finalized results; pending, excused and paused ones are left out.";

    [Fact]
    public async Task US2_AS6_slice_6_HP_002_each_goal_shows_its_name_link_its_tag_and_its_meta_line_in_the_overviews_order()
    {
        // Slice 6 (HP-002, FR-011): the full GoalListItem, in the overview's order. At 10:00 on Tuesday 17 November,
        // Coffee (per day) and Reading (Mon–Fri) start today and are Due; Meditation starts on 24 November and Running's
        // first week on 23 November, so they come after, each group by name. The slice 4 goal list keeps its name order.
        var user = SignInUser();
        var reading = _catalog.Add(HabitSamples.Reading());
        var running = _catalog.Add(HabitSamples.Catalog("Running", category: Category.Body));
        var meditation = _catalog.Add(HabitSamples.Catalog("Meditation"));
        var coffee = _catalog.Add(HabitSamples.Catalog("Coffee", category: Category.Health, intent: Intent.Reduce));
        var readingId = await CreateAsync(user, reading, Days(BuildOrReduce.Build, Measurement.Duration, 15, null, DurationUnit.Minutes,
            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday), Today);
        await CreateAsync(user, running, Times(BuildOrReduce.Build, Measurement.YesNo, null, null, 2, SchedulePeriod.Week), new DateOnly(2026, 11, 18));
        await CreateAsync(user, meditation, Days(BuildOrReduce.Build, Measurement.Duration, 10, null, DurationUnit.Minutes,
            DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday), new DateOnly(2026, 11, 24));
        await CreateAsync(user, coffee, Times(BuildOrReduce.Reduce, Measurement.Count, 2, "cups", 1, SchedulePeriod.Day), Today);

        var cut = Render<Goals>();

        Assert.Equal(0, _notFound);
        Assert.Equal("Goals", cut.Find(".ht-page-head h1").TextContent);
        // Slice 7 (HP-004): the link Tabs "Active" (current) and "Paused" above the list, with no counts and no "Ended".
        Assert.Equal([("Active", "/goals", "page"), ("Paused", "/goals?tab=paused", null)], TabsOf(cut));
        Assert.Single(cut.FindAll(".ht-list"));
        var items = cut.FindAll(".ht-list > .ht-goalitem");
        Assert.Empty(cut.FindAll(".ht-goalitem--reduced"));
        Assert.Equal(
            ["Coffee, at most 2 cups a day", "Reading, 15 min, 5 days a week", "Meditation, 10 min, 3 days a week", "Running, 2 times a week"],
            items.Select(i => i.QuerySelector("a.ht-goalitem__name")!.TextContent));
        Assert.Equal(["Reduce", "Build", "Build", "Build"], items.Select(i => i.QuerySelector(".ht-chip")!.TextContent));
        Assert.Equal(
            ["Every day", "Mon–Fri · next today", "Mon · Wed · Fri · starts Tue 24 Nov", "Flexible · 2 times a week · first week starts Mon 23 Nov"],
            items.Select(i => Text(i.QuerySelector(".ht-goalitem__meta span:not(.ht-chip)")!)));
        Assert.Equal($"/goals/{readingId.Value}", items[1].QuerySelector("a.ht-goalitem__name")!.GetAttribute("href"));
        Assert.Equal(
            ["Consistency", "Current streak"],
            items[0].QuerySelectorAll(".ht-goalitem__label").Select(l => l.TextContent));
        Assert.Equal(4, cut.FindAll(".ht-strip.ht-strip--mini").Count);
        Assert.Equal(
            ["Record", "Record", null, null],
            items.Select(i => i.QuerySelector(".ht-goalitem__actions button")?.TextContent));
        Assert.Equal(
            "Each goal is counted on its own. Consistency counts only finalized results; pending, excused and paused ones are left out.",
            cut.Find(".ht-list + p.ht-footnote").TextContent);
        Assert.Empty(cut.FindAll(".ht-empty"));
        foreach (var absent in new[] { "Ended", "Excuse" })
        {
            Assert.DoesNotContain(absent, cut.Markup, StringComparison.Ordinal);
        }

        await using var scope = Services.CreateAsyncScope();
        var list = await scope.ServiceProvider.GetRequiredService<GoalService>().ListAsync(user, CancellationToken.None);
        Assert.Equal(
            ["Coffee, at most 2 cups a day", "Meditation, 10 min, 3 days a week", "Reading, 15 min, 5 days a week", "Running, 2 times a week"],
            list.Goals.Select(g => g.Name));
    }

    /// <summary>
    /// Slice 7: "Coffee" (per day) and "Reading" (Mon–Fri), both from today, Tuesday 17 November; "Reading" paused through
    /// the PauseService at 10:00 today from today until Sunday 22 November. Returns Reading's identifier.
    /// </summary>
    private async Task<GoalId> CoffeeAndPausedReadingAsync()
    {
        var user = SignInUser();
        var coffee = _catalog.Add(HabitSamples.Catalog("Coffee", category: Category.Health, intent: Intent.Reduce));
        var reading = _catalog.Add(HabitSamples.Reading());
        await CreateAsync(user, coffee, Times(BuildOrReduce.Reduce, Measurement.Count, 2, "cups", 1, SchedulePeriod.Day), Today);
        var readingId = await CreateAsync(user, reading, Days(BuildOrReduce.Build, Measurement.Duration, 15, null, DurationUnit.Minutes,
            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday), Today);
        await PauseUntilSundayAsync(user, readingId);
        return readingId;
    }

    [Fact]
    public async Task Slice_7_HP_004_US4_AS5_the_Paused_tab_lists_the_paused_goal_with_Paused_until_and_Resume_and_the_footnote()
    {
        var reading = await CoffeeAndPausedReadingAsync();
        Services.GetRequiredService<BunitNavigationManager>().NavigateTo("/goals?tab=paused");

        var cut = Render<Goals>();

        Assert.Equal([("Active", "/goals", null), ("Paused", "/goals?tab=paused", "page")], TabsOf(cut));
        var item = Assert.Single(cut.FindAll(".ht-list > .ht-goalitem"));
        var link = item.QuerySelector("a.ht-goalitem__name")!;
        Assert.Equal(("Reading, 15 min, 5 days a week", $"/goals/{reading.Value}"), (link.TextContent, link.GetAttribute("href")));
        Assert.Equal(["Build"], item.QuerySelectorAll(".ht-goalitem__meta .ht-chip").Select(c => c.TextContent));
        Assert.Equal("Paused until Sun 22 Nov", Text(item.QuerySelector(".ht-goalitem__meta span:not(.ht-chip)")!));
        Assert.Equal(["N/A", "0 commitments"], item.QuerySelectorAll(".ht-goalitem__value").Select(v => v.TextContent));
        Assert.NotNull(item.QuerySelector(".ht-strip.ht-strip--mini"));
        var resume = Assert.Single(item.QuerySelectorAll(".ht-goalitem__actions button"));
        Assert.Equal(("Resume", "Resume Reading, 15 min, 5 days a week"), (resume.TextContent, resume.GetAttribute("aria-label")));
        Assert.Equal(Footnote, cut.Find(".ht-list + p.ht-footnote").TextContent);
        Assert.DoesNotContain("Ended", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Coffee", cut.Markup, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("/goals")]
    [InlineData("/goals?tab=active")]
    [InlineData("/goals?tab=Paused")]
    [InlineData("/goals?tab=ended")]
    [InlineData("/goals?tab=")]
    public async Task Slice_7_HP_004_any_tab_value_but_paused_shows_the_Active_tab_without_the_paused_goal(string address)
    {
        await CoffeeAndPausedReadingAsync();
        Services.GetRequiredService<BunitNavigationManager>().NavigateTo(address);

        var cut = Render<Goals>();

        Assert.Equal([("Active", "/goals", "page"), ("Paused", "/goals?tab=paused", null)], TabsOf(cut));
        var item = Assert.Single(cut.FindAll(".ht-list > .ht-goalitem"));
        Assert.Equal("Coffee, at most 2 cups a day", item.QuerySelector("a.ht-goalitem__name")!.TextContent);
        Assert.Equal("Record", item.QuerySelector(".ht-goalitem__actions button")!.TextContent);
        Assert.Equal(Footnote, cut.Find(".ht-list + p.ht-footnote").TextContent);
        Assert.DoesNotContain("Paused until", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Resume", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Slice_7_US4_AS6_the_Paused_tab_with_no_paused_goal_says_No_paused_goals()
    {
        var user = SignInUser();
        var coffee = _catalog.Add(HabitSamples.Catalog("Coffee", category: Category.Health, intent: Intent.Reduce));
        await CreateAsync(user, coffee, Times(BuildOrReduce.Reduce, Measurement.Count, 2, "cups", 1, SchedulePeriod.Day), Today);
        Services.GetRequiredService<BunitNavigationManager>().NavigateTo("/goals?tab=paused");

        var cut = Render<Goals>();

        Assert.Equal([("Active", "/goals", null), ("Paused", "/goals?tab=paused", "page")], TabsOf(cut));
        Assert.Empty(cut.FindAll(".ht-list"));
        Assert.Equal("No paused goals.", cut.Find(".ht-tabs + .ht-card > p.ht-card__sub").TextContent);
        Assert.Equal(Footnote, cut.Find(".ht-card + p.ht-footnote").TextContent);
        Assert.Empty(cut.FindAll(".ht-empty"));
    }

    [Fact]
    public void Slice_7_a_user_with_no_goals_keeps_the_EmptyState_with_no_tabs_on_the_Paused_tab()
    {
        SignInUser();
        Services.GetRequiredService<BunitNavigationManager>().NavigateTo("/goals?tab=paused");

        var cut = Render<Goals>();

        Assert.Equal("No goals yet", cut.Find(".ht-card.ht-empty h3").TextContent);
        Assert.Empty(cut.FindAll(".ht-tabs"));
        Assert.Empty(cut.FindAll(".ht-footnote"));
        Assert.DoesNotContain("No paused goals.", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public async Task US2_AS6_a_started_Mon_Wed_Fri_goal_shows_its_next_date()
    {
        var user = SignInUser();
        var yoga = _catalog.Add(HabitSamples.Catalog("Yoga", category: Category.Body));
        await CreateAsync(user, yoga, Days(BuildOrReduce.Build, Measurement.YesNo, null, null, null, DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday), Today);

        var cut = Render<Goals>();

        Assert.Equal("Mon · Wed · Fri · next Wed 18 Nov", Text(cut.Find(".ht-goalitem__meta span:not(.ht-chip)")));
    }

    [Fact]
    public async Task US7_AS8_a_goal_on_an_Unpublished_habit_shows_nothing_extra_on_the_list()
    {
        var user = SignInUser();
        var coffee = _catalog.Add(HabitSamples.Catalog("Coffee", category: Category.Health, intent: Intent.Reduce));
        await CreateAsync(user, coffee, Times(BuildOrReduce.Reduce, Measurement.Count, 2, "cups", 1, SchedulePeriod.Day), Today);
        Assert.True(await _catalog.TrySetStateAsync(coffee.Id, CatalogHabitState.Published, CatalogHabitState.Unpublished, CancellationToken.None));

        var cut = Render<Goals>();

        Assert.Equal("Coffee, at most 2 cups a day", cut.Find("a.ht-goalitem__name").TextContent);
        Assert.Equal("Every day", Text(cut.Find(".ht-goalitem__meta span:not(.ht-chip)")));
        Assert.Empty(cut.FindAll(".ht-notice"));
        Assert.DoesNotContain("no longer in the public catalog", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void HP_003_a_user_with_no_goals_sees_the_EmptyState()
    {
        SignInUser();

        var cut = Render<Goals>();

        Assert.Equal(0, _notFound);
        Assert.Equal("Goals", cut.Find(".ht-page-head h1").TextContent);
        Assert.Empty(cut.FindAll(".ht-list"));
        Assert.Empty(cut.FindAll(".ht-footnote"));
        var empty = cut.Find(".ht-card.ht-empty");
        Assert.NotNull(empty.QuerySelector(".ht-empty__icon svg"));
        Assert.Equal("No goals yet", empty.QuerySelector("h3")!.TextContent);
        Assert.Equal("Pick a habit from the catalog or create your own, then set a target and a schedule for it.", empty.QuerySelector("p")!.TextContent);
        var buttons = empty.QuerySelectorAll(".ht-inline a.ht-btn");
        Assert.Equal(["Browse the catalog", "Create custom habit"], buttons.Select(Text));
        Assert.Equal(["/catalog", "/custom-habits/new"], buttons.Select(b => b.GetAttribute("href")));
        Assert.Contains("ht-btn--primary", buttons[0].ClassList);
        Assert.Contains("ht-btn--secondary", buttons[1].ClassList);
    }

    [Fact]
    public void An_administrator_gets_the_Not_found_page()
    {
        _authorization.SignIn("Administrator", "admin@example.com");

        var cut = Render<Goals>();

        Assert.Equal(1, _notFound);
        Assert.Empty(cut.FindAll("h1"));
        Assert.DoesNotContain("No goals yet", cut.Markup, StringComparison.Ordinal);
    }
}
