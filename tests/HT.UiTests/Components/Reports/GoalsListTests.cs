using Bunit;
using Bunit.TestDoubles;
using HT.Application.Goals;
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
}
