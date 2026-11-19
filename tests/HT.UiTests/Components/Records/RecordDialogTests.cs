using Bunit;
using HT.Application.Goals;
using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Ui.Components.Records;
using HT.UiTests.Components.Pages;
using HT.UnitTests.Fakes;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace HT.UiTests.Components.Records;

/// <summary>
/// The RecordDialog opened from "Record" (HP-002, US1-AS9; design/components/RecordDialog and RecordForms, slice 5
/// state), over the real RecordingService and the fakes. The user has no stored time zone, so the goals are in UTC and
/// every time below is a UTC literal. The goals start on Monday 16 November 2026, created at 08:00 that day; what can be
/// recorded is read from the use case at the instant each test names.
/// </summary>
public sealed class RecordDialogTests : BunitContext
{
    private const string NoteHelp = "An empty field keeps the existing note.";

    private static readonly DateOnly Mon16 = new(2026, 11, 16);

    private readonly FakeCatalogHabitStore _catalog;
    private readonly FakeRecordStore _records;
    private readonly FakeTimeProvider _clock;
    private readonly ActingUser _user = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.RegisteredUser);
    private readonly List<HistoryEntry> _saved = [];
    private int _closed;

    public RecordDialogTests()
    {
        _catalog = this.AddCatalog();
        _records = (FakeRecordStore)Services.GetRequiredService<IRecordStore>();
        _clock = (FakeTimeProvider)Services.GetRequiredService<TimeProvider>();
        _clock.SetUtcNow(new DateTimeOffset(2026, 11, 16, 8, 0, 0, TimeSpan.Zero));
    }

    private static DateTimeOffset Utc(int day, int hour, int minute = 0) => new(2026, 11, day, hour, minute, 0, TimeSpan.Zero);

    private static string Text(AngleSharp.Dom.IElement element) =>
        string.Join(" ", element.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private async Task<GoalId> GoalAsync(GoalTargetFields target)
    {
        var habit = _catalog.Add(HabitSamples.Catalog("Reading"));
        await using var scope = Services.CreateAsyncScope();
        var created = await scope.ServiceProvider.GetRequiredService<GoalService>().CreateAsync(
            _user, new GoalInput { Habit = new GoalHabit(habit.Id, HabitKind.Catalog), Target = target, StartDate = Mon16 }, CancellationToken.None);
        Assert.Equal(GoalCreateOutcome.Created, created.Outcome);
        return created.GoalId!.Value;
    }

    private async Task<GoalHistory> RecordableAsync(GoalId goal)
    {
        await using var scope = Services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<RecordingService>().GetRecordableAsync(_user, goal, CancellationToken.None);
        Assert.Equal(RecordableOutcome.Found, result.Outcome);
        return result.History!;
    }

    /// <summary>Creates the goal, moves the clock to <paramref name="at"/> and renders the open dialog with what can be recorded then.</summary>
    private async Task<(IRenderedComponent<RecordDialog> Cut, GoalId Goal)> OpenAsync(GoalTargetFields target, DateTimeOffset at, string name = "Reading, 20 min daily")
    {
        var goal = await GoalAsync(target);
        _clock.SetUtcNow(at);
        var recordable = await RecordableAsync(goal);
        var cut = Render<RecordDialog>(p => p
            .Add(x => x.Open, true)
            .Add(x => x.GoalId, goal)
            .Add(x => x.GoalName, name)
            .Add(x => x.Recordable, recordable)
            .Add(x => x.ActingUser, _user)
            .Add(x => x.OnClose, () => _closed++)
            .Add(x => x.OnSaved, (HistoryEntry entry) => _saved.Add(entry)));
        return (cut, goal);
    }

    private static GoalTargetFields Reading20MinDaily => new()
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = 20, DurationUnit = DurationUnit.Minutes,
        ScheduleKind = ScheduleKind.FixedDays, Days = Enum.GetValues<DayOfWeek>().ToHashSet(),
    };

    private static GoalTargetFields CoffeeAtMost2Cups => new()
    {
        BuildOrReduce = BuildOrReduce.Reduce, Measurement = Measurement.Count, Amount = 2, CountUnit = "cups",
        ScheduleKind = ScheduleKind.Times, Times = 1, Period = SchedulePeriod.Day,
    };

    private static GoalTargetFields YogaThreeTimesAWeek => new()
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo, ScheduleKind = ScheduleKind.Times, Times = 3, Period = SchedulePeriod.Week,
    };

    private static GoalTargetFields NoPhoneInBed => new()
    {
        BuildOrReduce = BuildOrReduce.Reduce, Measurement = Measurement.YesNo, ScheduleKind = ScheduleKind.FixedDays,
        Days = Enum.GetValues<DayOfWeek>().ToHashSet(),
    };

    private static List<string> Options(AngleSharp.Dom.IElement select) =>
        select.QuerySelectorAll("option").Select(o => o.TextContent).ToList();

    private static AngleSharp.Dom.IElement Field(IRenderedComponent<RecordDialog> cut, string label) =>
        cut.FindAll(".ht-field").Single(f => f.QuerySelector(".ht-field__label")!.TextContent.StartsWith(label, StringComparison.Ordinal));

    [Fact]
    public async Task US1_AS9_the_dialog_has_the_goals_title_its_target_the_For_select_the_field_and_no_Excuse_instead()
    {
        var (cut, _) = await OpenAsync(Reading20MinDaily, Utc(18, 2));

        var dialog = cut.Find("[role=dialog]");
        Assert.Equal("Record · Reading, 20 min daily", dialog.QuerySelector(".ht-dialog__head h2")!.TextContent);
        Assert.Equal("Build · at least 20 min · every day", dialog.QuerySelector(".ht-dialog__sub")!.TextContent);
        Assert.Equal(["For", "Time spent", "Note (optional)"], cut.FindAll(".ht-field__label").Select(Text));
        Assert.Equal(["Save record", "Cancel"], cut.FindAll(".ht-dialog__foot .ht-btn").Select(b => b.TextContent));
        Assert.Contains("ht-btn--primary", cut.Find(".ht-dialog__foot .ht-btn").ClassList);
        Assert.DoesNotContain("Excuse", cut.Markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Delete record", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public async Task The_For_select_lists_the_Due_entry_and_the_Pending_ones_newest_first_with_the_hours_left()
    {
        // At 02:00 on Wednesday 18 November: Tuesday 17's grace period ends at 00:00 on Friday 20 (46 h), Monday 16's at
        // 00:00 on Thursday 19 (22 h).
        var (cut, _) = await OpenAsync(Reading20MinDaily, Utc(18, 2));

        var select = Field(cut, "For").QuerySelector("select")!;
        Assert.Equal(["Today, Wed 18 Nov", "Tue 17 Nov · Pending, 46 h left", "Mon 16 Nov · Pending, 22 h left"], Options(select));
        Assert.Contains("ht-input", select.ClassList);
        Assert.Empty(cut.FindAll(".ht-field-pair"));
    }

    [Fact]
    public async Task A_duration_goal_has_Time_spent_in_minutes_the_partial_sentence_and_the_live_progress()
    {
        var (cut, _) = await OpenAsync(Reading20MinDaily, Utc(16, 10));

        var field = Field(cut, "Time spent");
        var input = field.QuerySelector(".ht-stepper input")!;
        Assert.Equal("Time spent", input.GetAttribute("aria-label"));
        Assert.Equal("0", input.GetAttribute("min"));
        Assert.Equal("5", input.GetAttribute("step"));
        Assert.Equal("minutes", field.QuerySelector(".ht-unit")!.TextContent);
        Assert.Null(input.GetAttribute("value"));
        Assert.Empty(cut.FindAll(".ht-target"));
        Assert.Equal(
            "Partial time is kept. Below 20 min the day counts as Failed when it is finalized.",
            field.QuerySelector(".ht-field__help")!.TextContent);

        cut.Find(".ht-stepper input").Change("12");

        var progress = cut.Find(".ht-field .ht-target");
        Assert.Equal("12 of 20 min", Text(progress));
        Assert.Equal("true", progress.QuerySelector(".ht-progress")!.GetAttribute("aria-hidden"));
        Assert.Equal("width:60%", progress.QuerySelector(".ht-progress > span")!.GetAttribute("style"));

        cut.Find(".ht-stepper input").Change("25");

        Assert.Equal("25 of 20 min", Text(cut.Find(".ht-target")));
        Assert.Equal("width:100%", cut.Find(".ht-progress > span").GetAttribute("style"));
    }

    [Fact]
    public async Task A_Reduce_count_goal_has_Amount_with_its_unit_the_progress_and_the_Reduce_help()
    {
        var (cut, _) = await OpenAsync(CoffeeAtMost2Cups, Utc(16, 10), "Coffee, at most 2 cups a day");

        Assert.Equal("Reduce · at most 2 cups a day", cut.Find(".ht-dialog__sub").TextContent);
        var field = Field(cut, "Amount");
        Assert.Equal("Amount", field.QuerySelector(".ht-stepper input")!.GetAttribute("aria-label"));
        Assert.Equal("1", field.QuerySelector(".ht-stepper input")!.GetAttribute("step"));
        Assert.Equal("cups", field.QuerySelector(".ht-unit")!.TextContent);
        Assert.Equal("0 of at most 2 cups", Text(field.QuerySelector(".ht-target")!));
        Assert.Equal(
            ["At most 2 cups. Enter 0 if you had none: an empty day cannot prove you stayed under the limit."],
            field.QuerySelectorAll(".ht-field__help").Select(h => h.TextContent));

        cut.Find(".ht-stepper input").Change("1");

        Assert.Equal("1 of at most 2 cups", Text(cut.Find(".ht-target")));
    }

    [Fact]
    public async Task A_yes_no_goal_has_Did_you_do_it_with_nothing_chosen_and_a_Reduce_goal_asks_for_No()
    {
        var (cut, _) = await OpenAsync(NoPhoneInBed, Utc(16, 10), "No phone in bed, avoided every day");

        Assert.Equal("Reduce · avoided every day", cut.Find(".ht-dialog__sub").TextContent);
        var field = Field(cut, "Did you do it?");
        var group = field.QuerySelector(".ht-seg[role=group]")!;
        Assert.Equal("Did you do it?", group.GetAttribute("aria-label"));
        Assert.Equal(["Yes", "No"], group.QuerySelectorAll("button").Select(b => b.TextContent));
        Assert.All(group.QuerySelectorAll("button"), b => Assert.Equal("false", b.GetAttribute("aria-pressed")));
        Assert.Empty(cut.FindAll(".ht-target"));
        Assert.Equal(
            "Choose No if you avoided it: an empty day cannot prove you stayed under the limit.",
            field.QuerySelector(".ht-field__help")!.TextContent);

        cut.FindAll(".ht-seg button")[1].Click();

        Assert.Equal(["false", "true"], cut.FindAll(".ht-seg button").Select(b => b.GetAttribute("aria-pressed")));
        Assert.Empty(cut.FindAll(".ht-target"));
    }

    [Fact]
    public async Task The_note_is_optional_empty_and_says_that_an_empty_field_keeps_the_existing_note()
    {
        var (cut, _) = await OpenAsync(Reading20MinDaily, Utc(16, 10));

        var note = Field(cut, "Note");
        Assert.Equal("Note (optional)", Text(note.QuerySelector(".ht-field__label")!));
        var area = note.QuerySelector("textarea.ht-input.ht-textarea")!;
        Assert.True(string.IsNullOrEmpty(area.GetAttribute("value")) && string.IsNullOrEmpty(area.TextContent));
        Assert.Equal("What happened? Only you can see notes.", area.GetAttribute("placeholder"));
        Assert.Equal(NoteHelp, note.QuerySelector(".ht-field__help")!.TextContent);
    }

    [Fact]
    public async Task A_per_week_entry_adds_the_Date_select_of_its_dates_up_to_the_current_date_newest_first()
    {
        var (cut, _) = await OpenAsync(YogaThreeTimesAWeek, Utc(18, 10), "Yoga, 3 times a week");

        var pair = cut.Find(".ht-field-pair");
        var selects = pair.QuerySelectorAll("select");
        Assert.Equal(["For", "Date"], pair.QuerySelectorAll(".ht-field__label").Select(l => l.TextContent));
        Assert.Equal(["16 – 22 Nov"], Options(selects[0]));
        Assert.Equal(["Wed 18 Nov", "Tue 17 Nov", "Mon 16 Nov"], Options(selects[1]));
        Assert.Equal("0 of 3 times", Text(cut.Find(".ht-target")));
    }

    [Fact]
    public async Task Choosing_the_Pending_week_lists_all_its_dates()
    {
        // At 10:00 on Tuesday 24 November the week of 16 November ended at 00:00 on Monday 23; its grace period ends at
        // 00:00 on Wednesday 25, 14 h later.
        var (cut, _) = await OpenAsync(YogaThreeTimesAWeek, Utc(24, 10), "Yoga, 3 times a week");
        Assert.Equal(["23 – 29 Nov", "16 – 22 Nov · Pending, 14 h left"], Options(cut.FindAll(".ht-field-pair select")[0]));
        Assert.Equal(["Tue 24 Nov", "Mon 23 Nov"], Options(cut.FindAll(".ht-field-pair select")[1]));

        cut.FindAll(".ht-field-pair select")[0].Change("2026-11-16");

        Assert.Equal(
            ["Sun 22 Nov", "Sat 21 Nov", "Fri 20 Nov", "Thu 19 Nov", "Wed 18 Nov", "Tue 17 Nov", "Mon 16 Nov"],
            Options(cut.FindAll(".ht-field-pair select")[1]));
    }

    [Fact]
    public async Task Saving_records_for_the_chosen_date_and_raises_saved_with_the_entry()
    {
        var (cut, goal) = await OpenAsync(YogaThreeTimesAWeek, Utc(18, 10), "Yoga, 3 times a week");
        cut.FindAll(".ht-field-pair select")[1].Change("2026-11-17");
        cut.FindAll(".ht-seg button")[0].Click();
        cut.Find("textarea").Input("  Felt good.  ");

        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        var entry = Assert.Single(_saved);
        var record = Assert.Single(entry.Records);
        Assert.Equal(new DateOnly(2026, 11, 17), record.Date);
        Assert.Equal("Felt good.", entry.Note);
        Assert.Single(_records.RecordsOfGoal(goal));
    }

    [Fact]
    public async Task One_refusal_at_a_time_sits_under_the_field_it_is_about()
    {
        var (cut, goal) = await OpenAsync(YogaThreeTimesAWeek, Utc(18, 10), "Yoga, 3 times a week");

        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        var error = Assert.Single(cut.FindAll(".ht-field__error"));
        Assert.Equal("Choose Yes or No.", error.TextContent);
        Assert.Equal(error.TextContent, Field(cut, "Did you do it?").QuerySelector(".ht-field__error")!.TextContent);
        Assert.Equal(error.Id, Field(cut, "Did you do it?").QuerySelector(".ht-seg")!.GetAttribute("aria-describedby"));

        cut.FindAll(".ht-seg button")[0].Click();
        cut.Find("textarea").Input(new string('a', 501));
        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        var noteError = Assert.Single(cut.FindAll(".ht-field__error"));
        Assert.Equal("The note can have at most 500 characters.", noteError.TextContent);
        Assert.Equal(noteError.TextContent, Field(cut, "Note").QuerySelector(".ht-field__error")!.TextContent);
        Assert.Empty(_saved);
        Assert.Empty(_records.RecordsOfGoal(goal));
    }

    [Fact]
    public async Task A_count_out_of_range_is_refused_under_the_field()
    {
        var (cut, _) = await OpenAsync(CoffeeAtMost2Cups, Utc(16, 10), "Coffee, at most 2 cups a day");
        cut.Find(".ht-stepper input").Change("10000");

        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        var error = Assert.Single(cut.FindAll(".ht-field__error"));
        Assert.Equal("Enter a whole number from 0 to 9999.", error.TextContent);
        Assert.Equal(error.TextContent, Field(cut, "Amount").QuerySelector(".ht-field__error")!.TextContent);
        Assert.Equal("true", cut.Find(".ht-stepper input").GetAttribute("aria-invalid"));
    }

    [Fact]
    public async Task Focus_moves_to_For_on_open()
    {
        var (cut, _) = await OpenAsync(Reading20MinDaily, Utc(16, 10));

        var select = Field(cut, "For").QuerySelector("select")!;
        var focused = JSInterop.VerifyFocusAsyncInvoke().Arguments[0];
        Assert.Equal(select.GetAttribute("blazor:elementReference"), Assert.IsType<ElementReference>(focused).Id);
    }

    [Fact]
    public async Task Esc_and_the_close_button_ask_the_page_to_close_it()
    {
        var (cut, _) = await OpenAsync(Reading20MinDaily, Utc(16, 10));

        cut.Find("[role=dialog]").KeyDown(new KeyboardEventArgs { Key = "Escape" });
        cut.Find("button.ht-dialog__close").Click();
        cut.FindAll(".ht-dialog__foot .ht-btn")[1].Click();

        Assert.Equal(3, _closed);
        Assert.Empty(_saved);
    }
}
