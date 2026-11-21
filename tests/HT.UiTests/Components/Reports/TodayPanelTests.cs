using Bunit;
using Bunit.TestDoubles;
using HT.Application.Goals;
using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using HT.Ui.Components.Reports;
using HT.UiTests.Components.Pages;
using HT.UnitTests.Fakes;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace HT.UiTests.Components.Reports;

/// <summary>
/// HP-003, US6, contracts/host.md "Signed-in Home": the TodayPanel island over the real use cases and the fakes, in UTC.
/// Two goals from Monday 9 November 2026, created that day at 08:00Z and read on Tuesday 17 at 10:00Z:
/// <list type="bullet">
/// <item>"Reading, 20 min daily": 20 minutes on 9 to 12; Friday 13 Missed with the note "Away." and Saturday 14 Missed
/// (their grace periods ended on Monday 16 and Tuesday 17 at 00:00Z, this week); Sunday 15 Pending (until Wednesday 18
/// 00:00Z); Monday 16 Pending with 12 minutes (until Thursday 19 00:00Z); Tuesday 17 Due;</item>
/// <item>"Yoga, 20 min, once a week" on Mondays: Monday 16 Pending (until Thursday 19 00:00Z), Monday 23 Upcoming.</item>
/// </list>
/// The Today list is: Reading 15, Reading 16, Yoga 16 (Pending), Reading 17 (Due), Reading 14, Reading 13 (Missed this
/// week), Yoga 23 (Upcoming).
/// </summary>
public sealed class TodayPanelTests : BunitContext
{
    private const string Reading = "Reading, 20 min daily";
    private const string Yoga = "Yoga, 20 min, once a week";

    private static readonly DateOnly Mon9 = new(2026, 11, 9);
    private static readonly DateTimeOffset Mon9Morning = new(2026, 11, 9, 8, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Tue17TenOClock = new(2026, 11, 17, 10, 0, 0, TimeSpan.Zero);

    private readonly BunitAuthorizationContext _authorization;
    private readonly FakeCatalogHabitStore _catalog;
    private readonly FakeRecordStore _records;
    private readonly FakeTimeProvider _clock;

    public TodayPanelTests()
    {
        _authorization = AddAuthorization();
        _catalog = this.AddCatalog();
        _records = (FakeRecordStore)Services.GetRequiredService<IRecordStore>();
        _clock = (FakeTimeProvider)Services.GetRequiredService<TimeProvider>();
        _clock.SetUtcNow(Mon9Morning);
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

    private async Task ArrangeAsync()
    {
        var user = SignInUser();
        var reading = await CreateAsync(user, "Reading", MinutesOn(Enum.GetValues<DayOfWeek>()), Mon9);
        await CreateAsync(user, "Yoga", MinutesOn(DayOfWeek.Monday), Mon9);
        foreach (var day in new[] { 9, 10, 11, 12 })
        {
            _records.Seed(reading, new DateOnly(2026, 11, day), RecordValue.FromAmount(20));
        }

        _records.Seed(reading, new DateOnly(2026, 11, 16), RecordValue.FromAmount(12));
        Assert.True(NoteText.TryCreate("Away.", out var note, out _));
        _records.SeedNote(reading, new DateOnly(2026, 11, 13), note!);
        _clock.SetUtcNow(Tue17TenOClock);
    }

    private static string Text(AngleSharp.Dom.IElement element) =>
        string.Join(" ", element.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static List<(string Name, string Meta, string Chip)> Rows(IRenderedComponent<TodayPanel> cut) =>
        cut.FindAll(".ht-row").Select(r => (
            r.QuerySelector(".ht-row__name")!.TextContent,
            r.QuerySelector(".ht-row__meta")!.TextContent,
            r.QuerySelector(".ht-status")!.TextContent)).ToList();

    private static AngleSharp.Dom.IElement ActionOf(IRenderedComponent<TodayPanel> cut, string label) =>
        cut.FindAll(".ht-row button").Single(b => b.GetAttribute("aria-label") == label);

    private static AngleSharp.Dom.IElement Tile(IRenderedComponent<TodayPanel> cut, string label) =>
        cut.FindAll(".ht-home__tiles .ht-tile").Single(t => t.QuerySelector(".ht-tile__label")!.TextContent == label);

    private List<string> FocusedReferences(int count) =>
        JSInterop.VerifyFocusAsyncInvoke(count).Select(i => Assert.IsType<ElementReference>(i.Arguments[0]).Id).ToList();

    [Fact]
    public async Task HP_003_the_four_tiles_have_their_counts_and_the_Today_card_its_rows_in_order()
    {
        await ArrangeAsync();

        var cut = Render<TodayPanel>();

        var panel = cut.Find("div.ht-home__panel");
        Assert.Equal(["ht-grid-4 ht-home__tiles", "ht-card"], panel.Children.Select(c => c.ClassName));
        Assert.Equal(
            [("BUTTON", "Due now", "1"), ("BUTTON", "Pending · still can record", "3"), ("BUTTON", "Missed this week", "2"), ("A", "Active goals", "2")],
            cut.FindAll(".ht-tile").Select(t => (t.TagName, t.QuerySelector(".ht-tile__label")!.TextContent, t.QuerySelector(".ht-tile__value")!.TextContent)));
        Assert.Equal(["ht-card", "ht-tile", "ht-tile--pending"], Tile(cut, "Pending · still can record").ClassList);
        Assert.Equal(["ht-card", "ht-tile", "ht-tile--failed"], Tile(cut, "Missed this week").ClassList);
        Assert.Equal("/goals", Tile(cut, "Active goals").GetAttribute("href"));
        Assert.All(cut.FindAll("button.ht-tile"), t => Assert.Equal("false", t.GetAttribute("aria-pressed")));
        Assert.Equal("Today", cut.Find("section.ht-card > h2.ht-card__title").TextContent);
        Assert.Equal(
            [
                (Reading, "Sun 15 Nov · 14 h left to record", "Pending"),
                (Reading, "Mon 16 Nov · 38 h left to record", "Pending"),
                (Yoga, "Mon 16 Nov · 38 h left to record", "Pending"),
                (Reading, "Every day", "Due today"),
                (Reading, "Sat 14 Nov · no record", "Missed"),
                (Reading, "Fri 13 Nov · no record", "Missed"),
                (Yoga, "Mon 23 Nov", "Upcoming"),
            ],
            Rows(cut));
        Assert.Equal(
            ["Record", "Edit", "Record", "Record", "Add note", "Edit note"],
            cut.FindAll(".ht-row button").Select(b => b.TextContent));
        Assert.Empty(cut.FindAll("[role=dialog]"));
    }

    [Fact]
    public async Task HP_003_a_tile_filters_the_Today_list_and_choosing_it_again_shows_all()
    {
        await ArrangeAsync();
        var cut = Render<TodayPanel>();

        Tile(cut, "Missed this week").Click();

        Assert.Equal("true", Tile(cut, "Missed this week").GetAttribute("aria-pressed"));
        Assert.Equal([(Reading, "Sat 14 Nov · no record", "Missed"), (Reading, "Fri 13 Nov · no record", "Missed")], Rows(cut));

        Tile(cut, "Pending · still can record").Click();

        Assert.Equal(["false", "true", "false"], cut.FindAll("button.ht-tile").Select(t => t.GetAttribute("aria-pressed")));
        Assert.Equal(["Pending", "Pending", "Pending"], Rows(cut).Select(r => r.Chip));

        Tile(cut, "Pending · still can record").Click();

        Assert.All(cut.FindAll("button.ht-tile"), t => Assert.Equal("false", t.GetAttribute("aria-pressed")));
        Assert.Equal(7, Rows(cut).Count);
    }

    [Fact]
    public async Task HP_003_each_action_opens_its_dialog_for_its_row()
    {
        await ArrangeAsync();
        var cut = Render<TodayPanel>();

        await ActionOf(cut, $"Record for {Reading}, Tue 17 Nov").ClickAsync(new MouseEventArgs());
        var record = cut.Find("[role=dialog]");
        Assert.Equal($"Record · {Reading}", record.QuerySelector("h2")!.TextContent);
        Assert.Equal("Tue 17 Nov", Text(record.QuerySelector(".ht-field > span:not(.ht-field__label)")!));
        Assert.Empty(record.QuerySelectorAll("select"));
        await cut.Find(".ht-dialog__foot .ht-btn--secondary").ClickAsync(new MouseEventArgs());

        await ActionOf(cut, $"Edit record of {Reading}, Mon 16 Nov").ClickAsync(new MouseEventArgs());
        Assert.Equal("12", cut.Find(".ht-stepper input").GetAttribute("value"));
        Assert.Contains("Delete record", cut.Find(".ht-dialog__foot").TextContent, StringComparison.Ordinal);
        await cut.Find(".ht-dialog__foot .ht-btn--secondary").ClickAsync(new MouseEventArgs());

        await ActionOf(cut, $"Add note for {Reading}, Sat 14 Nov").ClickAsync(new MouseEventArgs());
        var add = cut.Find("[role=dialog]");
        Assert.Equal($"Add note · {Reading}", add.QuerySelector("h2")!.TextContent);
        Assert.Equal("Sat 14 Nov · Missed", add.QuerySelector(".ht-dialog__sub")!.TextContent);
        await cut.Find(".ht-dialog__foot .ht-btn--secondary").ClickAsync(new MouseEventArgs());

        await ActionOf(cut, $"Edit note of {Reading}, Fri 13 Nov").ClickAsync(new MouseEventArgs());
        Assert.Equal($"Edit note · {Reading}", cut.Find("[role=dialog] h2").TextContent);
        Assert.Equal("Away.", cut.Find("[role=dialog] textarea").GetAttribute("value") ?? cut.Find("[role=dialog] textarea").TextContent);
    }

    [Fact]
    public async Task HP_003_a_save_reads_the_dashboard_again_and_gives_the_focus_back_to_the_rows_action()
    {
        await ArrangeAsync();
        var cut = Render<TodayPanel>();
        var action = ActionOf(cut, $"Record for {Reading}, Tue 17 Nov").GetAttribute("blazor:elementReference");

        await ActionOf(cut, $"Record for {Reading}, Tue 17 Nov").ClickAsync(new MouseEventArgs());
        cut.Find(".ht-stepper input").Change("20");
        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        Assert.Empty(cut.FindAll("[role=dialog]"));
        Assert.Equal((Reading, "20 of 20 min today", "Due today"), Rows(cut)[3]);
        Assert.Equal("Edit", cut.FindAll(".ht-row")[3].QuerySelector("button")!.TextContent);
        Assert.Equal(action, FocusedReferences(2)[1]);
    }

    [Fact]
    public async Task HP_003_a_saved_note_changes_the_rows_action_and_the_focus_goes_back_to_it()
    {
        await ArrangeAsync();
        var cut = Render<TodayPanel>();
        var action = ActionOf(cut, $"Add note for {Reading}, Sat 14 Nov").GetAttribute("blazor:elementReference");

        await ActionOf(cut, $"Add note for {Reading}, Sat 14 Nov").ClickAsync(new MouseEventArgs());
        cut.Find("[role=dialog] textarea").Input("Ill.");
        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        Assert.Empty(cut.FindAll("[role=dialog]"));
        Assert.NotNull(ActionOf(cut, $"Edit note of {Reading}, Sat 14 Nov"));
        Assert.Equal(action, FocusedReferences(2)[1]);
    }

    [Theory]
    [InlineData("record")]
    [InlineData("note")]
    public async Task HP_003_Esc_gives_the_focus_back_to_the_rows_action_for_both_dialogs(string dialog)
    {
        await ArrangeAsync();
        var cut = Render<TodayPanel>();
        var label = dialog == "record" ? $"Record for {Yoga}, Mon 16 Nov" : $"Edit note of {Reading}, Fri 13 Nov";
        var action = ActionOf(cut, label).GetAttribute("blazor:elementReference");

        await ActionOf(cut, label).ClickAsync(new MouseEventArgs());
        cut.Find("[role=dialog]").KeyDown(new KeyboardEventArgs { Key = "Escape" });

        Assert.Empty(cut.FindAll("[role=dialog]"));
        Assert.Equal(action, FocusedReferences(2)[1]);
    }

    [Fact]
    public async Task HP_003_when_the_row_has_left_the_list_the_focus_goes_to_Due_now()
    {
        // Sunday 15 is Pending until Wednesday 18 at 00:00Z. The dialog opens at 23:30Z on Tuesday 17 and is saved at
        // 00:00:30Z on Wednesday 18: the record corrects Sunday 15, which is now final (Met) and leaves the Today list.
        await ArrangeAsync();
        _clock.SetUtcNow(new DateTimeOffset(2026, 11, 17, 23, 30, 0, TimeSpan.Zero));
        var cut = Render<TodayPanel>();
        var dueNow = Tile(cut, "Due now").GetAttribute("blazor:elementReference");

        await ActionOf(cut, $"Record for {Reading}, Sun 15 Nov").ClickAsync(new MouseEventArgs());
        _clock.SetUtcNow(new DateTimeOffset(2026, 11, 18, 0, 0, 30, TimeSpan.Zero));
        cut.Find(".ht-stepper input").Change("20");
        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        Assert.Empty(cut.FindAll("[role=dialog]"));
        Assert.DoesNotContain(cut.FindAll(".ht-row__meta"), m => m.TextContent.StartsWith("Sun 15 Nov", StringComparison.Ordinal));
        Assert.Equal(dueNow, FocusedReferences(2)[1]);
    }

    [Fact]
    public async Task HP_003_when_the_filter_hides_the_row_the_focus_goes_to_Due_now()
    {
        // Reading's Monday 16 is Pending with 12 minutes until Thursday 19 at 00:00Z. With "Pending · still can record"
        // pressed, its "Edit" opens at 23:30Z on Wednesday 18 and "Delete record" is used at 00:00:30Z on Thursday 19:
        // the day is now Missed this week, still in the Today list but hidden by the Pending filter.
        await ArrangeAsync();
        _clock.SetUtcNow(new DateTimeOffset(2026, 11, 18, 23, 30, 0, TimeSpan.Zero));
        var cut = Render<TodayPanel>();
        var dueNow = Tile(cut, "Due now").GetAttribute("blazor:elementReference");
        Tile(cut, "Pending · still can record").Click();

        await ActionOf(cut, $"Edit record of {Reading}, Mon 16 Nov").ClickAsync(new MouseEventArgs());
        _clock.SetUtcNow(new DateTimeOffset(2026, 11, 19, 0, 0, 30, TimeSpan.Zero));
        await cut.FindAll(".ht-dialog__foot .ht-btn")[2].ClickAsync(new MouseEventArgs());

        Assert.Empty(cut.FindAll("[role=dialog]"));
        Assert.DoesNotContain(Rows(cut), r => r.Meta.StartsWith("Mon 16 Nov", StringComparison.Ordinal) && r.Name == Reading);
        Tile(cut, "Pending · still can record").Click();
        Assert.Contains((Reading, "Mon 16 Nov · no record", "Missed"), Rows(cut));
        Assert.Equal(dueNow, FocusedReferences(2)[1]);
    }

    [Fact]
    public async Task HP_003_an_empty_Today_list_and_an_empty_filter_say_Nothing_to_record_right_now()
    {
        var user = SignInUser();
        await CreateAsync(user, "Reading", MinutesOn(Enum.GetValues<DayOfWeek>()), new DateOnly(2026, 12, 1));
        _clock.SetUtcNow(Tue17TenOClock);
        var cut = Render<TodayPanel>();

        Assert.Empty(cut.FindAll(".ht-row"));
        Assert.Equal("Nothing to record right now.", cut.Find("section.ht-card > p.ht-card__sub").TextContent);
        Assert.Equal(["0", "0", "0", "1"], cut.FindAll(".ht-tile__value").Select(v => v.TextContent));

        Tile(cut, "Due now").Click();

        Assert.Equal("Nothing to record right now.", cut.Find("section.ht-card > p.ht-card__sub").TextContent);
    }
}
