using Bunit;
using HT.Application.Goals;
using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
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

    // ---- US3-AS8: opened from a History row ("Add record") and as "Edit" of one record (HP-002, T063) ----

    /// <summary>The history from <paramref name="first"/> to <paramref name="last"/> at the clock's time, as the goal page reads it.</summary>
    private async Task<GoalHistory> HistoryAsync(GoalId goal, DateOnly first, DateOnly last)
    {
        await using var scope = Services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<RecordingService>().GetHistoryAsync(_user, goal, first, last, CancellationToken.None);
        Assert.Equal(HistoryOutcome.Found, result.Outcome);
        return result.History!;
    }

    /// <summary>
    /// Creates the goal, moves the clock to <paramref name="at"/> after <paramref name="arrange"/> has seeded its records,
    /// and renders the open dialog for the row whose first date is <paramref name="row"/>; with <paramref name="editing"/>,
    /// as "Edit" of the row's record at that index.
    /// </summary>
    private async Task<(IRenderedComponent<RecordDialog> Cut, GoalId Goal, HistoryEntry Row)> OpenRowAsync(
        GoalTargetFields target, DateTimeOffset at, DateOnly row, int? editing = null, Action<GoalId>? arrange = null, string name = "Reading, 20 min daily")
    {
        var goal = await GoalAsync(target);
        arrange?.Invoke(goal);
        _clock.SetUtcNow(at);
        var history = await HistoryAsync(goal, Mon16, new DateOnly(at.Year, at.Month, at.Day));
        var entry = history.Entries.Single(e => e.FirstDate == row);
        var cut = Render<RecordDialog>(p => p
            .Add(x => x.Open, true)
            .Add(x => x.GoalId, goal)
            .Add(x => x.GoalName, name)
            .Add(x => x.Recordable, history)
            .Add(x => x.Row, entry)
            .Add(x => x.Editing, editing is { } index ? entry.Records[index] : null)
            .Add(x => x.ActingUser, _user)
            .Add(x => x.OnClose, () => _closed++)
            .Add(x => x.OnSaved, (HistoryEntry saved) => _saved.Add(saved)));
        return (cut, goal, entry);
    }

    private static string? FocusedId(BunitJSInterop js) =>
        Assert.IsType<ElementReference>(js.VerifyFocusAsyncInvoke().Arguments[0]).Id;

    [Fact]
    public async Task US3_AS8_from_a_Missed_commitments_Add_record_For_is_fixed_to_that_row_and_focus_moves_to_the_value()
    {
        // At 10:00 on Friday 20 November, Monday 16's grace period ended at 00:00 on Thursday 19: it is Missed.
        var (cut, goal, row) = await OpenRowAsync(Reading20MinDaily, Utc(20, 10), Mon16);
        Assert.Equal(EvaluationState.Missed, row.State);

        Assert.Equal(["For", "Time spent", "Note (optional)"], cut.FindAll(".ht-field__label").Select(Text));
        var forField = Field(cut, "For");
        Assert.Equal("DIV", forField.TagName);
        Assert.Equal("Mon 16 Nov", Text(forField.QuerySelector("span:not(.ht-field__label)")!));
        Assert.Empty(cut.FindAll("select"));
        Assert.Equal(["Save record", "Cancel"], cut.FindAll(".ht-dialog__foot .ht-btn").Select(b => b.TextContent));
        Assert.Equal(cut.Find(".ht-stepper input").GetAttribute("blazor:elementReference"), FocusedId(JSInterop));

        cut.Find(".ht-stepper input").Change("20");
        Assert.Equal("20 of 20 min", Text(cut.Find(".ht-target")));
        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        var saved = Assert.Single(_saved);
        Assert.Equal(EvaluationState.Met, saved.State);
        Assert.True(saved.IsCorrection);
        var record = Assert.Single(_records.RecordsOfGoal(goal));
        Assert.Equal((Mon16, 20), (record.Date, record.Value.Amount!.Value));
    }

    [Fact]
    public async Task US3_AS8_from_a_final_weeks_Add_record_For_is_its_date_range_and_Date_lists_all_its_dates_with_the_focus()
    {
        // At 10:00 on Thursday 26 November the week of 16 November ended at 00:00 on Monday 23 and is final.
        var (cut, goal, _) = await OpenRowAsync(YogaThreeTimesAWeek, Utc(26, 10), Mon16, name: "Yoga, 3 times a week");

        var pair = cut.Find(".ht-field-pair");
        Assert.Equal(["For", "Date"], pair.QuerySelectorAll(".ht-field__label").Select(l => l.TextContent));
        Assert.Equal("16 – 22 Nov", Text(pair.QuerySelector("div.ht-field > span:not(.ht-field__label)")!));
        var date = Assert.Single(cut.FindAll("select"));
        Assert.Equal(
            ["Sun 22 Nov", "Sat 21 Nov", "Fri 20 Nov", "Thu 19 Nov", "Wed 18 Nov", "Tue 17 Nov", "Mon 16 Nov"],
            Options(date));
        Assert.Equal(date.GetAttribute("blazor:elementReference"), FocusedId(JSInterop));

        cut.Find("select").Change("2026-11-20");
        cut.FindAll(".ht-seg button")[0].Click();
        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        Assert.True(Assert.Single(_saved).IsCorrection);
        Assert.Equal(new DateOnly(2026, 11, 20), Assert.Single(_records.RecordsOfGoal(goal)).Date);
    }

    [Fact]
    public async Task US3_AS8_from_the_current_weeks_Add_record_Date_lists_its_dates_up_to_the_current_date()
    {
        var (cut, _, _) = await OpenRowAsync(YogaThreeTimesAWeek, Utc(26, 10), new DateOnly(2026, 11, 23), name: "Yoga, 3 times a week");

        Assert.Equal("23 – 29 Nov", Text(cut.Find(".ht-field-pair div.ht-field > span:not(.ht-field__label)")));
        Assert.Equal(["Thu 26 Nov", "Wed 25 Nov", "Tue 24 Nov", "Mon 23 Nov"], Options(cut.Find("select")));
    }

    [Fact]
    public async Task US3_AS8_Edit_has_the_records_date_fixed_its_value_filled_its_preview_and_Delete_record()
    {
        var (cut, goal, row) = await OpenRowAsync(
            Reading20MinDaily, Utc(20, 10), Mon16, editing: 0, arrange: g => _records.Seed(g, Mon16, RecordValue.FromAmount(20)));

        Assert.Equal("Record · Reading, 20 min daily", cut.Find(".ht-dialog__head h2").TextContent);
        Assert.Equal("Mon 16 Nov", Text(Field(cut, "For").QuerySelector("span:not(.ht-field__label)")!));
        Assert.Empty(cut.FindAll("select"));
        var input = cut.Find(".ht-stepper input");
        Assert.Equal("20", input.GetAttribute("value"));
        Assert.Equal("20 of 20 min", Text(cut.Find(".ht-target")));
        Assert.Equal("width:100%", cut.Find(".ht-progress > span").GetAttribute("style"));
        Assert.Equal(["Save record", "Cancel", "Delete record"], cut.FindAll(".ht-dialog__foot .ht-btn").Select(b => b.TextContent));
        var delete = cut.FindAll(".ht-dialog__foot .ht-btn")[2];
        Assert.Equal(["ht-btn", "ht-btn--ghost"], delete.ClassList.ToArray());
        Assert.Equal("ht-spacer", delete.PreviousElementSibling!.ClassName);
        Assert.Equal(input.GetAttribute("blazor:elementReference"), FocusedId(JSInterop));
        Assert.True(string.IsNullOrEmpty(cut.Find("textarea").TextContent));

        cut.Find(".ht-stepper input").Change("12");
        Assert.Equal("12 of 20 min", Text(cut.Find(".ht-target")));
        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        var saved = Assert.Single(_saved);
        Assert.Equal(EvaluationState.Failed, saved.State);
        Assert.True(saved.IsCorrection);
        var record = Assert.Single(_records.RecordsOfGoal(goal));
        Assert.Equal((row.Records[0].Id, Mon16, 12), (record.Id, record.Date, record.Value.Amount!.Value));
    }

    [Fact]
    public async Task US3_AS3_Delete_record_deletes_it_and_raises_saved_with_the_entry_now_Missed()
    {
        var (cut, goal, _) = await OpenRowAsync(
            Reading20MinDaily, Utc(20, 10), Mon16, editing: 0, arrange: g => _records.Seed(g, Mon16, RecordValue.FromAmount(20)));

        await cut.FindAll(".ht-dialog__foot .ht-btn")[2].ClickAsync(new MouseEventArgs());

        var saved = Assert.Single(_saved);
        Assert.Equal(EvaluationState.Missed, saved.State);
        Assert.Empty(saved.Records);
        Assert.Empty(_records.RecordsOfGoal(goal));
    }

    [Fact]
    public async Task Edit_of_a_weeks_record_has_its_own_date_no_Date_select_and_a_preview_without_the_record_it_replaces()
    {
        var (cut, goal, row) = await OpenRowAsync(YogaThreeTimesAWeek, Utc(18, 10), Mon16, editing: 0, arrange: g =>
        {
            _records.Seed(g, Mon16, RecordValue.FromAnswer(YesNoAnswer.Yes));
            _records.Seed(g, new DateOnly(2026, 11, 17), RecordValue.FromAnswer(YesNoAnswer.Yes));
        }, name: "Yoga, 3 times a week");

        Assert.Equal("Mon 16 Nov", Text(Field(cut, "For").QuerySelector("span:not(.ht-field__label)")!));
        Assert.Empty(cut.FindAll(".ht-field-pair"));
        Assert.Empty(cut.FindAll("select"));
        var answers = cut.FindAll(".ht-seg button");
        Assert.Equal(["true", "false"], answers.Select(b => b.GetAttribute("aria-pressed")));
        Assert.Equal("2 of 3 times", Text(cut.Find(".ht-target")));
        Assert.Equal(answers[0].GetAttribute("blazor:elementReference"), FocusedId(JSInterop));

        cut.FindAll(".ht-seg button")[1].Click();
        Assert.Equal("1 of 3 times", Text(cut.Find(".ht-target")));
        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        Assert.Single(_saved);
        Assert.Equal(
            [(row.Records[0].Id, YesNoAnswer.No), (row.Records[1].Id, YesNoAnswer.Yes)],
            _records.RecordsOfGoal(goal).Select(r => (r.Id, r.Value.Answer!.Value)));
    }

    [Fact]
    public async Task Edit_of_a_record_that_no_longer_exists_says_so_under_For()
    {
        var (cut, goal, row) = await OpenRowAsync(
            Reading20MinDaily, Utc(20, 10), Mon16, editing: 0, arrange: g => _records.Seed(g, Mon16, RecordValue.FromAmount(20)));
        await using (var scope = Services.CreateAsyncScope())
        {
            var deleted = await scope.ServiceProvider.GetRequiredService<RecordingService>()
                .DeleteRecordAsync(_user, goal, row.Records[0].Id, CancellationToken.None);
            Assert.Equal(RecordWriteOutcome.Deleted, deleted.Outcome);
        }

        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        var error = Assert.Single(cut.FindAll(".ht-field__error"));
        Assert.Equal("This record does not exist.", error.TextContent);
        Assert.Equal(error.TextContent, Field(cut, "For").QuerySelector(".ht-field__error")!.TextContent);
        Assert.Empty(_saved);
    }

    [Fact]
    public async Task A_value_refusal_in_Edit_sits_under_the_field()
    {
        var (cut, _, _) = await OpenRowAsync(
            CoffeeAtMost2Cups, Utc(16, 10), Mon16, editing: 0, arrange: g => _records.Seed(g, Mon16, RecordValue.FromAmount(1)),
            name: "Coffee, at most 2 cups a day");
        cut.Find(".ht-stepper input").Change("10000");

        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        var error = Assert.Single(cut.FindAll(".ht-field__error"));
        Assert.Equal("Enter a whole number from 0 to 9999.", error.TextContent);
        Assert.Equal(error.TextContent, Field(cut, "Amount").QuerySelector(".ht-field__error")!.TextContent);
        Assert.Empty(_saved);
    }
}
