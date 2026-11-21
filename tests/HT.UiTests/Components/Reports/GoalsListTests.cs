using Bunit;
using Bunit.TestDoubles;
using HT.Application.Exemptions;
using HT.Application.Goals;
using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Ui.Components.Reports;
using HT.UiTests.Components.Pages;
using HT.UnitTests.Fakes;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace HT.UiTests.Components.Reports;

/// <summary>
/// HP-002, US5-AS5, contracts/host.md "Goals page": the GoalsList island over the real GoalService, RecordingService,
/// ReportService and the fakes. The signed-in user has no stored zone, so the goals are in UTC. Goals are created on
/// Monday 16 November 2026 at 08:00Z and the list is read on Tuesday 17 November at 10:00Z: Monday 16 ended at 00:00Z
/// on Tuesday 17 and its grace period ends 48 hours later, on Thursday 19 at 00:00Z (38 hours left), so it is Pending,
/// and Tuesday 17 is Due.
/// </summary>
public sealed class GoalsListTests : BunitContext
{
    private static readonly DateOnly Mon16 = new(2026, 11, 16);
    private static readonly DateOnly Tue17 = new(2026, 11, 17);
    private static readonly DateTimeOffset Mon16Morning = new(2026, 11, 16, 8, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Tue17TenOClock = new(2026, 11, 17, 10, 0, 0, TimeSpan.Zero);

    private readonly BunitAuthorizationContext _authorization;
    private readonly FakeCatalogHabitStore _catalog;
    private readonly FakeTimeProvider _clock;

    public GoalsListTests()
    {
        _authorization = AddAuthorization();
        _catalog = this.AddCatalog();
        _clock = (FakeTimeProvider)Services.GetRequiredService<TimeProvider>();
        _clock.SetUtcNow(Mon16Morning);
    }

    private ActingUser SignInUser() => ActingUser.ForAccount(new AccountId(_authorization.SignIn("RegisteredUser")), Role.RegisteredUser);

    private async Task<GoalId> CreateAsync(ActingUser user, string habitName, GoalTargetFields target, DateOnly start)
    {
        var habit = _catalog.Add(HabitSamples.Catalog(habitName));
        await using var scope = Services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<GoalService>().CreateAsync(
            user, new GoalInput { Habit = new GoalHabit(habit.Id, HabitKind.Catalog), Target = target, StartDate = start }, CancellationToken.None);
        Assert.Equal(GoalCreateOutcome.Created, result.Outcome);
        return result.GoalId!.Value;
    }

    private static GoalTargetFields MinutesOn(params DayOfWeek[] days) => new()
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = 20, DurationUnit = DurationUnit.Minutes,
        ScheduleKind = ScheduleKind.FixedDays, Days = days.ToHashSet(),
    };

    private static GoalTargetFields EveryDay => MinutesOn(Enum.GetValues<DayOfWeek>());

    private static string Text(AngleSharp.Dom.IElement element) =>
        string.Join(" ", element.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static AngleSharp.Dom.IElement Item(IRenderedComponent<GoalsList> cut, string name) =>
        cut.FindAll(".ht-list > .ht-goalitem").Single(i => i.QuerySelector("a.ht-goalitem__name")!.TextContent == name);

    private static AngleSharp.Dom.IElement? RecordOf(IRenderedComponent<GoalsList> cut, string name) =>
        Item(cut, name).QuerySelector(".ht-goalitem__actions button");

    private List<string> FocusedReferences(int count) =>
        JSInterop.VerifyFocusAsyncInvoke(count).Select(i => Assert.IsType<ElementReference>(i.Arguments[0]).Id).ToList();

    [Fact]
    public async Task HP_002_the_goals_come_in_the_overviews_order_with_their_figures_and_actions()
    {
        // "Walking" starts on Wednesday 25 November (nothing yet), "Reading" is daily from Tuesday 17 (Due only) and
        // "Yoga" is on Mondays from Monday 16 (Monday 16 Pending, nothing Due), created in that order.
        var user = SignInUser();
        await CreateAsync(user, "Walking", EveryDay, new DateOnly(2026, 11, 25));
        await CreateAsync(user, "Reading", EveryDay, Tue17);
        await CreateAsync(user, "Yoga", MinutesOn(DayOfWeek.Monday), Mon16);
        _clock.SetUtcNow(Tue17TenOClock);

        var cut = Render<GoalsList>();

        var items = cut.FindAll(".ht-list > .ht-goalitem");
        Assert.Equal(
            ["Yoga, 20 min, once a week", "Reading, 20 min daily", "Walking, 20 min daily"],
            items.Select(i => i.QuerySelector("a.ht-goalitem__name")!.TextContent));
        Assert.Equal([["Build", "Pending"], ["Build"], ["Build"]], items.Select(i => i.QuerySelectorAll(".ht-goalitem__meta .ht-chip").Select(c => c.TextContent).ToList()));
        Assert.Equal(
            ["N/A", "0 commitments", "N/A", "0 commitments", "N/A", "0 commitments"],
            cut.FindAll(".ht-goalitem__value").Select(v => v.TextContent));
        Assert.Equal(
            ["Last 1 commitment: 1 pending", "Last 0 commitments", "Last 0 commitments"],
            cut.FindAll(".ht-strip.ht-strip--mini").Select(s => s.GetAttribute("aria-label")));
        Assert.Contains("ht-btn--secondary", RecordOf(cut, "Yoga, 20 min, once a week")!.ClassList);
        Assert.Contains("ht-btn--primary", RecordOf(cut, "Reading, 20 min daily")!.ClassList);
        Assert.Null(RecordOf(cut, "Walking, 20 min daily"));
        Assert.Empty(cut.FindAll("[role=dialog]"));
    }

    [Fact]
    public async Task HP_002_Record_opens_the_RecordDialog_with_every_entry_that_can_be_recorded_now()
    {
        var user = SignInUser();
        await CreateAsync(user, "Reading", EveryDay, Mon16);
        _clock.SetUtcNow(Tue17TenOClock);
        var cut = Render<GoalsList>();

        await RecordOf(cut, "Reading, 20 min daily")!.ClickAsync(new MouseEventArgs());

        var dialog = cut.Find("[role=dialog]");
        Assert.Equal("Record · Reading, 20 min daily", dialog.QuerySelector("h2")!.TextContent);
        Assert.Equal(
            ["Today, Tue 17 Nov", "Mon 16 Nov · Pending, 38 h left"],
            dialog.QuerySelectorAll("select option").Select(o => o.TextContent));
    }

    [Fact]
    public async Task HP_002_a_save_reads_the_list_again_and_gives_the_focus_back_to_the_goals_Record()
    {
        var user = SignInUser();
        await CreateAsync(user, "Reading", EveryDay, Mon16);
        _clock.SetUtcNow(Tue17TenOClock);
        var cut = Render<GoalsList>();
        var record = RecordOf(cut, "Reading, 20 min daily")!.GetAttribute("blazor:elementReference");
        Assert.Equal("Every day · next today", Text(Item(cut, "Reading, 20 min daily").QuerySelector(".ht-goalitem__meta span:not(.ht-chip)")!));

        await RecordOf(cut, "Reading, 20 min daily")!.ClickAsync(new MouseEventArgs());
        cut.Find(".ht-stepper input").Change("12");
        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        Assert.Empty(cut.FindAll("[role=dialog]"));
        Assert.Equal(
            "Every day · next today · 12 of 20 min today",
            Text(Item(cut, "Reading, 20 min daily").QuerySelector(".ht-goalitem__meta span:not(.ht-chip)")!));
        Assert.NotNull(RecordOf(cut, "Reading, 20 min daily"));
        Assert.Equal(record, FocusedReferences(2)[1]);
    }

    [Theory]
    [InlineData("Escape")]
    [InlineData("Cancel")]
    public async Task HP_002_closing_the_dialog_without_a_save_gives_the_focus_back_to_the_goals_Record(string way)
    {
        var user = SignInUser();
        await CreateAsync(user, "Reading", EveryDay, Mon16);
        await CreateAsync(user, "Yoga", MinutesOn(DayOfWeek.Monday), Mon16);
        _clock.SetUtcNow(Tue17TenOClock);
        var cut = Render<GoalsList>();
        var record = RecordOf(cut, "Yoga, 20 min, once a week")!.GetAttribute("blazor:elementReference");

        await RecordOf(cut, "Yoga, 20 min, once a week")!.ClickAsync(new MouseEventArgs());
        if (way == "Escape")
        {
            cut.Find("[role=dialog]").KeyDown(new KeyboardEventArgs { Key = "Escape" });
        }
        else
        {
            await cut.Find(".ht-dialog__foot .ht-btn--secondary").ClickAsync(new MouseEventArgs());
        }

        Assert.Empty(cut.FindAll("[role=dialog]"));
        Assert.Equal(record, FocusedReferences(2)[1]);
    }

    [Fact]
    public async Task HP_002_a_save_that_leaves_nothing_to_record_gives_the_focus_to_the_goals_name_link()
    {
        // "Yoga" on Mondays from Monday 16: on Wednesday 18 at 23:00Z only Monday 16 can be recorded (Pending until
        // Thursday 19 at 00:00Z, 48 hours after it ended). The dialog is saved at 00:00:30Z on Thursday 19, after that
        // grace period ended: the record still corrects Monday 16, which is now final, and the next Monday is Upcoming,
        // so "Record" is gone.
        var user = SignInUser();
        await CreateAsync(user, "Yoga", MinutesOn(DayOfWeek.Monday), Mon16);
        _clock.SetUtcNow(new DateTimeOffset(2026, 11, 18, 23, 0, 0, TimeSpan.Zero));
        var cut = Render<GoalsList>();
        var link = Item(cut, "Yoga, 20 min, once a week").QuerySelector("a.ht-goalitem__name")!.GetAttribute("blazor:elementReference");

        await RecordOf(cut, "Yoga, 20 min, once a week")!.ClickAsync(new MouseEventArgs());
        _clock.SetUtcNow(new DateTimeOffset(2026, 11, 19, 0, 0, 30, TimeSpan.Zero));
        cut.Find(".ht-stepper input").Change("20");
        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        Assert.Empty(cut.FindAll("[role=dialog]"));
        Assert.Null(RecordOf(cut, "Yoga, 20 min, once a week"));
        Assert.Equal("100% (1 of 1)", Item(cut, "Yoga, 20 min, once a week").QuerySelector(".ht-goalitem__value")!.TextContent);
        Assert.Equal(link, FocusedReferences(2)[1]);
    }

    [Fact]
    public async Task Slice_7_HP_003_Excuse_instead_excuses_the_commitment_reads_the_list_again_and_gives_the_focus_to_the_name_link()
    {
        // "Yoga" on Mondays from Monday 16: at 10:00Z on Tuesday 17 only Monday 16 can be recorded (Pending, 38 h left).
        // Once it is excused, nothing is left to record, so "Record" is gone and the focus goes to the goal's name link.
        var user = SignInUser();
        var goal = await CreateAsync(user, "Yoga", MinutesOn(DayOfWeek.Monday), Mon16);
        _clock.SetUtcNow(Tue17TenOClock);
        var cut = Render<GoalsList>();
        var link = Item(cut, "Yoga, 20 min, once a week").QuerySelector("a.ht-goalitem__name")!.GetAttribute("blazor:elementReference");

        await RecordOf(cut, "Yoga, 20 min, once a week")!.ClickAsync(new MouseEventArgs());
        var excuseInstead = cut.FindAll(".ht-dialog__foot .ht-btn")[^1];
        Assert.Equal("Excuse instead", excuseInstead.TextContent);
        await excuseInstead.ClickAsync(new MouseEventArgs());
        Assert.Equal("Excuse · Yoga, 20 min, once a week", cut.Find("[role=dialog] h2").TextContent);
        Assert.Equal("Mon 16 Nov · Pending, 38 h left", cut.Find("[role=dialog] .ht-dialog__sub").TextContent);
        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        Assert.Empty(cut.FindAll("[role=dialog]"));
        var records = (FakeRecordStore)Services.GetRequiredService<IRecordStore>();
        Assert.NotNull(records.ExemptionsOfGoal(goal).ExcusalOf(Mon16));
        Assert.Null(RecordOf(cut, "Yoga, 20 min, once a week"));
        Assert.Equal(["Build"], Item(cut, "Yoga, 20 min, once a week").QuerySelectorAll(".ht-goalitem__meta .ht-chip").Select(c => c.TextContent));
        Assert.Equal(link, FocusedReferences(4)[^1]);
    }

    // ---- slice 7: the Tabs "Active" and "Paused" (HP-004, US4-AS5, US4-AS6; contracts/host.md "Goals page") ----

    private const string YogaName = "Yoga, 20 min, once a week";
    private const string ReadingName = "Reading, 20 min daily";
    private const string WalkingName = "Walking, 20 min daily";

    /// <summary>
    /// Three goals created on Monday 16 at 08:00Z: "Walking" daily from Tuesday 17 (Due only), "Reading" daily from Monday
    /// 16 (Monday 16 Pending, Tuesday 17 Due) and "Yoga" on Mondays from Monday 16 (Monday 16 Pending). At 10:00Z on
    /// Tuesday 17 "Walking" and "Yoga" are paused through the PauseService from that day until Sunday 22 November, so
    /// Walking's Tuesday 17 is Paused and it has nothing Pending; the overview's order is then Reading and Yoga (Pending,
    /// by name), then Walking. The clock is left at 10:00Z on Tuesday 17.
    /// </summary>
    private async Task<(ActingUser User, GoalId Reading, GoalId Yoga, GoalId Walking)> TwoPausedGoalsAsync()
    {
        var user = SignInUser();
        var walking = await CreateAsync(user, "Walking", EveryDay, Tue17);
        var reading = await CreateAsync(user, "Reading", EveryDay, Mon16);
        var yoga = await CreateAsync(user, "Yoga", MinutesOn(DayOfWeek.Monday), Mon16);
        _clock.SetUtcNow(Tue17TenOClock);
        await PauseAsync(user, walking);
        await PauseAsync(user, yoga);
        return (user, reading, yoga, walking);
    }

    private async Task PauseAsync(ActingUser user, GoalId goal)
    {
        await using var scope = Services.CreateAsyncScope();
        var paused = await scope.ServiceProvider.GetRequiredService<PauseService>().PauseAsync(
            user, goal, Tue17, new DateOnly(2026, 11, 22), null, CancellationToken.None);
        Assert.Equal(PauseOutcome.Paused, paused.Outcome);
    }

    /// <summary>The item's one action, its "Resume" in the Paused form.</summary>
    private static AngleSharp.Dom.IElement? ResumeOf(IRenderedComponent<GoalsList> cut, string name) => RecordOf(cut, name);

    private IRenderedComponent<GoalsList> RenderTab(string? tab) => Render<GoalsList>(p => p.Add(x => x.Tab, tab));

    private static string[] Names(IRenderedComponent<GoalsList> cut) =>
        cut.FindAll(".ht-list > .ht-goalitem").Select(i => i.QuerySelector("a.ht-goalitem__name")!.TextContent).ToArray();

    private static (string Text, string? Href, string? Current)[] TabsOf(IRenderedComponent<GoalsList> cut)
    {
        var nav = cut.Find("nav.ht-tabs");
        Assert.Equal("Goal lists", nav.GetAttribute("aria-label"));
        return nav.QuerySelectorAll("a.ht-tab").Select(a => (a.TextContent, a.GetAttribute("href"), a.GetAttribute("aria-current"))).ToArray();
    }

    [Fact]
    public async Task Slice_7_HP_004_the_Active_tab_lists_the_goals_that_are_not_paused_under_the_tabs_Active_and_Paused()
    {
        await TwoPausedGoalsAsync();

        var cut = RenderTab(null);

        Assert.Equal([("Active", "/goals", "page"), ("Paused", "/goals?tab=paused", null)], TabsOf(cut));
        Assert.Equal([ReadingName], Names(cut));
        Assert.Equal("Record", RecordOf(cut, ReadingName)!.TextContent);
        Assert.DoesNotContain("Ended", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Paused until", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Slice_7_HP_004_US4_AS5_the_Paused_tab_lists_the_paused_goals_in_the_same_order_with_Paused_until_and_Resume()
    {
        await TwoPausedGoalsAsync();

        var cut = RenderTab("paused");

        Assert.Equal([("Active", "/goals", null), ("Paused", "/goals?tab=paused", "page")], TabsOf(cut));
        Assert.Equal([YogaName, WalkingName], Names(cut));
        Assert.Equal(
            [["Build", "Pending"], ["Build"]],
            cut.FindAll(".ht-list > .ht-goalitem").Select(i => i.QuerySelectorAll(".ht-goalitem__meta .ht-chip").Select(c => c.TextContent).ToList()));
        Assert.Equal(
            ["Paused until Sun 22 Nov", "Paused until Sun 22 Nov"],
            cut.FindAll(".ht-goalitem__meta span:not(.ht-chip)").Select(Text));
        Assert.Equal(
            ["N/A", "0 commitments", "N/A", "0 commitments"],
            cut.FindAll(".ht-goalitem__value").Select(v => v.TextContent));
        Assert.Equal(
            ["Last 1 commitment: 1 pending", "Last 0 commitments"],
            cut.FindAll(".ht-strip.ht-strip--mini").Select(s => s.GetAttribute("aria-label")));
        Assert.Equal(
            [("Resume", "Resume " + YogaName), ("Resume", "Resume " + WalkingName)],
            cut.FindAll(".ht-goalitem__actions button").Select(b => (b.TextContent, b.GetAttribute("aria-label"))));
        Assert.All(cut.FindAll(".ht-goalitem__actions button"), b => Assert.Contains("ht-btn--secondary", b.ClassList));
        Assert.DoesNotContain(ReadingName, cut.Markup, StringComparison.Ordinal);
        Assert.Empty(cut.FindAll("[role=alert]"));
    }

    [Fact]
    public async Task Slice_7_HP_004_Resume_resumes_the_goal_which_moves_to_Active_and_the_focus_goes_to_the_next_Resume_then_to_the_Paused_tab()
    {
        var (_, _, yoga, walking) = await TwoPausedGoalsAsync();
        var cut = RenderTab("paused");
        var walkingResume = ResumeOf(cut, WalkingName)!.GetAttribute("blazor:elementReference");
        var pausedTab = cut.FindAll("nav.ht-tabs a.ht-tab").Single(a => a.TextContent == "Paused").GetAttribute("blazor:elementReference");

        // "Resume" on Yoga, the first paused goal: Walking's "Resume" is the nearest remaining control.
        await ResumeOf(cut, YogaName)!.ClickAsync(new MouseEventArgs());

        Assert.Equal([WalkingName], Names(cut));
        Assert.Equal(walkingResume, FocusedReferences(1)[0]);
        var records = (FakeRecordStore)Services.GetRequiredService<IRecordStore>();
        Assert.Empty(records.ExemptionsOfGoal(yoga).Pauses);

        // "Resume" on Walking, the last paused goal: the Paused tab is left empty, and it gets the focus.
        await ResumeOf(cut, WalkingName)!.ClickAsync(new MouseEventArgs());

        Assert.Empty(cut.FindAll(".ht-list"));
        Assert.Equal("No paused goals.", cut.Find(".ht-tabs + .ht-card > p.ht-card__sub").TextContent);
        Assert.Equal(pausedTab, FocusedReferences(2)[1]);
        Assert.Empty(records.ExemptionsOfGoal(walking).Pauses);
        Assert.Empty(cut.FindAll("[role=alert]"));

        // Both goals are now on the Active tab, in the overview's order.
        var active = RenderTab(null);
        Assert.Equal([ReadingName, YogaName, WalkingName], Names(active));
    }

    [Fact]
    public async Task Slice_7_HP_004_Resume_of_the_last_paused_goal_gives_the_focus_to_the_one_before()
    {
        await TwoPausedGoalsAsync();
        var cut = RenderTab("paused");
        var yogaResume = ResumeOf(cut, YogaName)!.GetAttribute("blazor:elementReference");

        await ResumeOf(cut, WalkingName)!.ClickAsync(new MouseEventArgs());

        Assert.Equal([YogaName], Names(cut));
        Assert.Equal(yogaResume, FocusedReferences(1)[0]);
    }

    [Fact]
    public async Task Slice_7_HP_004_a_refused_Resume_shows_its_text_above_the_list_and_reads_the_list_again()
    {
        // Yoga is resumed elsewhere (through the PauseService) after the tab was rendered; its "Resume" then gets "This goal
        // is not paused.", and the list read again no longer has it.
        var (user, _, yoga, _) = await TwoPausedGoalsAsync();
        var cut = RenderTab("paused");
        await using (var scope = Services.CreateAsyncScope())
        {
            var resumed = await scope.ServiceProvider.GetRequiredService<PauseService>().ResumeAsync(user, yoga, CancellationToken.None);
            Assert.Equal(ResumeOutcome.Resumed, resumed.Outcome);
        }

        await ResumeOf(cut, YogaName)!.ClickAsync(new MouseEventArgs());

        var alert = cut.Find(".ht-tabs + p.ht-field__error[role=alert]");
        Assert.Equal("This goal is not paused.", alert.TextContent);
        Assert.Equal("ht-list", alert.NextElementSibling!.ClassName);
        Assert.Equal([WalkingName], Names(cut));
    }

    [Fact]
    public async Task Slice_7_US4_AS6_the_Paused_tab_with_no_paused_goal_says_No_paused_goals()
    {
        var user = SignInUser();
        await CreateAsync(user, "Reading", EveryDay, Mon16);
        _clock.SetUtcNow(Tue17TenOClock);

        var cut = RenderTab("paused");

        Assert.Equal([("Active", "/goals", null), ("Paused", "/goals?tab=paused", "page")], TabsOf(cut));
        Assert.Empty(cut.FindAll(".ht-list"));
        Assert.Equal("No paused goals.", cut.Find(".ht-tabs + div.ht-card > p.ht-card__sub").TextContent);
    }
}
