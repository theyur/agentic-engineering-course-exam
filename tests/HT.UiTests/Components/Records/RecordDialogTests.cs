using Bunit;
using HT.Application.Goals;
using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using HT.Domain.Time;
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

    /// <summary>
    /// Creates the goal, lets <paramref name="arrange"/> seed its records and notes, moves the clock to
    /// <paramref name="at"/> and renders the open dialog with what can be recorded then.
    /// </summary>
    private async Task<(IRenderedComponent<RecordDialog> Cut, GoalId Goal)> OpenAsync(
        GoalTargetFields target, DateTimeOffset at, string name = "Reading, 20 min daily", Action<GoalId>? arrange = null)
    {
        var goal = await GoalAsync(target);
        arrange?.Invoke(goal);
        _clock.SetUtcNow(at);
        var recordable = await RecordableAsync(goal);
        var cut = Render<RecordDialog>(p => p
            .Add(x => x.Open, true)
            .Add(x => x.GoalId, goal)
            .Add(x => x.GoalName, name)
            .Add(x => x.Recordable, recordable)
            .Add(x => x.ActingUser, _user)
            .Add(x => x.OnClose, () => _closed++)
            .Add(x => x.OnSaved, entry => _saved.Add(entry)));
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

    /// <summary>What the note field shows: the textarea's bound value, which bUnit renders as its value attribute.</summary>
    private static string NoteShown(IRenderedComponent<RecordDialog> cut) => cut.Find("textarea").GetAttribute("value") ?? string.Empty;

    private void SeedNote(GoalId goal, DateOnly key, string text)
    {
        Assert.True(NoteText.TryCreate(text, out var note, out _));
        _records.SeedNote(goal, key, note!);
    }

    private string? StoredNote(GoalId goal, DateOnly key) =>
        _records.NotesOfGoal(goal).TryGetValue(key, out var note) ? note.Value : null;

    [Fact]
    public async Task US1_AS9_the_dialog_has_the_goals_title_its_target_the_For_select_the_field_and_no_Excuse_instead_for_a_period_or_an_entry_with_a_record()
    {
        // Slice 7 (HP-003, D-3): "Excuse instead" is for a Due or Pending commitment without a record, so it is absent
        // while "For" names Wednesday 18, which has a record, and for a period-based goal, even with the page handling it.
        var reading = await GoalAsync(Reading20MinDaily);
        var coffee = await GoalAsync(CoffeeAtMost2Cups);
        _records.Seed(reading, new DateOnly(2026, 11, 18), RecordValue.FromAmount(5));
        _clock.SetUtcNow(Utc(18, 2));
        var cut = RenderExcusable(reading, "Reading, 20 min daily", await RecordableAsync(reading));
        var period = RenderExcusable(coffee, "Coffee, at most 2 cups a day", await RecordableAsync(coffee));

        var dialog = cut.Find("[role=dialog]");
        Assert.Equal("Record · Reading, 20 min daily", dialog.QuerySelector(".ht-dialog__head h2")!.TextContent);
        Assert.Equal("Build · at least 20 min · every day", dialog.QuerySelector(".ht-dialog__sub")!.TextContent);
        Assert.Equal(["For", "Time spent", "Note (optional)"], cut.FindAll(".ht-field__label").Select(Text));
        Assert.Equal(["Save record", "Cancel"], cut.FindAll(".ht-dialog__foot .ht-btn").Select(b => b.TextContent));
        Assert.Contains("ht-btn--primary", cut.Find(".ht-dialog__foot .ht-btn").ClassList);
        Assert.DoesNotContain("Excuse instead", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Delete record", cut.Markup, StringComparison.Ordinal);
        Assert.Equal(["Save record", "Cancel"], period.FindAll(".ht-dialog__foot .ht-btn").Select(b => b.TextContent));
        Assert.DoesNotContain("Excuse instead", period.Markup, StringComparison.Ordinal);
    }

    // ---- slice 7: "Excuse instead" (HP-003, D-2, D-3; design/components/RecordDialog, slice 7 state) ----

    private readonly List<HistoryEntry> _excuseInstead = [];

    /// <summary>The open dialog for <paramref name="recordable"/>, with the page handling "Excuse instead".</summary>
    private IRenderedComponent<RecordDialog> RenderExcusable(
        GoalId goal, string name, GoalHistory recordable, HistoryEntry? row = null, RecordView? editing = null) =>
        Render<RecordDialog>(p => p
            .Add(x => x.Open, true)
            .Add(x => x.GoalId, goal)
            .Add(x => x.GoalName, name)
            .Add(x => x.Recordable, recordable)
            .Add(x => x.Row, row)
            .Add(x => x.Editing, editing)
            .Add(x => x.ActingUser, _user)
            .Add(x => x.OnClose, () => _closed++)
            .Add(x => x.OnSaved, entry => _saved.Add(entry))
            .Add(x => x.OnExcuseInstead, entry => _excuseInstead.Add(entry)));

    private static GoalTargetFields YogaOnMonWedFri => new()
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo, ScheduleKind = ScheduleKind.FixedDays,
        Days = new HashSet<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
    };

    [Fact]
    public async Task Slice_7_HP_003_Excuse_instead_shows_for_a_Due_and_for_a_Pending_commitment_without_a_record_and_follows_the_selection()
    {
        // At 02:00 on Wednesday 18 November: Wednesday 18 is Due with a record, Tuesday 17 and Monday 16 are Pending.
        var reading = await GoalAsync(Reading20MinDaily);
        _records.Seed(reading, new DateOnly(2026, 11, 18), RecordValue.FromAmount(5));
        _clock.SetUtcNow(Utc(18, 2));
        var cut = RenderExcusable(reading, "Reading, 20 min daily", await RecordableAsync(reading));
        Assert.DoesNotContain("Excuse instead", cut.Markup, StringComparison.Ordinal);

        Field(cut, "For").QuerySelector("select")!.Change("2026-11-17");

        var buttons = cut.FindAll(".ht-dialog__foot > *");
        Assert.Equal(["Save record", "Cancel", string.Empty, "Excuse instead"], buttons.Select(b => b.TextContent));
        Assert.Contains("ht-spacer", buttons[2].ClassList);
        var excuse = buttons[3];
        Assert.Equal(["ht-btn", "ht-btn--ghost"], excuse.ClassList.ToArray());
        Assert.Equal("dialog", excuse.GetAttribute("aria-haspopup"));
        Assert.Equal("button", excuse.GetAttribute("type"));

        Field(cut, "For").QuerySelector("select")!.Change("2026-11-18");
        Assert.DoesNotContain("Excuse instead", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Slice_7_HP_003_Excuse_instead_shows_for_the_Due_commitment_of_today()
    {
        var yoga = await GoalAsync(YogaOnMonWedFri);

        // 07:00 on Wednesday 18 November: Wednesday 18 is Due, Monday 16 Pending until 00:00 on Thursday 19.
        _clock.SetUtcNow(Utc(18, 7));
        var cut = RenderExcusable(yoga, "Yoga, 3 days a week", await RecordableAsync(yoga));

        // The newest entry is chosen on open.
        Assert.Equal(["Today, Wed 18 Nov", "Mon 16 Nov · Pending, 17 h left"], Options(Field(cut, "For").QuerySelector("select")!));
        Assert.Equal("Excuse instead", cut.FindAll(".ht-dialog__foot .ht-btn")[^1].TextContent);
    }

    [Fact]
    public async Task Slice_7_HP_003_choosing_Excuse_instead_raises_the_chosen_commitment_and_saves_nothing()
    {
        var yoga = await GoalAsync(YogaOnMonWedFri);
        _clock.SetUtcNow(Utc(18, 7));
        var cut = RenderExcusable(yoga, "Yoga, 3 days a week", await RecordableAsync(yoga));
        Field(cut, "For").QuerySelector("select")!.Change("2026-11-16");

        await cut.FindAll(".ht-dialog__foot .ht-btn")[^1].ClickAsync(new MouseEventArgs());

        var chosen = Assert.Single(_excuseInstead);
        Assert.Equal((new DateOnly(2026, 11, 16), EvaluationState.Pending), (chosen.FirstDate, chosen.State));
        Assert.Empty(_saved);
        Assert.Equal(0, _closed);
        Assert.Empty(_records.RecordsOfGoal(yoga));
    }

    [Fact]
    public async Task Slice_7_HP_003_no_Excuse_instead_in_Edit_of_a_record()
    {
        var yoga = await GoalAsync(YogaOnMonWedFri);
        _records.Seed(yoga, new DateOnly(2026, 11, 16), RecordValue.FromAnswer(YesNoAnswer.No));
        _clock.SetUtcNow(Utc(18, 7));
        var recordable = await RecordableAsync(yoga);
        var row = recordable.Entries.Single(e => e.FirstDate == new DateOnly(2026, 11, 16));

        var cut = RenderExcusable(yoga, "Yoga, 3 days a week", recordable, row, row.Records[0]);

        Assert.Contains("Delete record", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Excuse instead", cut.Markup, StringComparison.Ordinal);
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
    public async Task HP_003_the_note_is_optional_and_says_that_clearing_the_field_removes_the_note()
    {
        var (cut, _) = await OpenAsync(Reading20MinDaily, Utc(16, 10));

        var note = Field(cut, "Note");
        Assert.Equal("Note (optional)", Text(note.QuerySelector(".ht-field__label")!));
        var area = note.QuerySelector("textarea.ht-input.ht-textarea")!;
        Assert.True(string.IsNullOrEmpty(area.GetAttribute("value")) && string.IsNullOrEmpty(area.TextContent));
        Assert.Equal("What happened? Only you can see notes.", area.GetAttribute("placeholder"));
        var help = Assert.Single(note.QuerySelectorAll(".ht-field__help"));
        Assert.Equal("Clear the field to remove the note.", help.TextContent);
        Assert.False(string.IsNullOrEmpty(help.Id));
        Assert.Equal(help.Id, area.GetAttribute("aria-describedby"));
        Assert.Null(area.GetAttribute("aria-invalid"));
    }

    // ---- specs/006-note-editing US1: the field shows the entry's note and saving stores what it shows (HP-003) ----

    [Fact]
    public async Task US1_AS1_from_the_page_head_the_field_opens_with_the_first_entrys_note()
    {
        var (cut, _) = await OpenAsync(Reading20MinDaily, Utc(16, 10), arrange: g => SeedNote(g, Mon16, "Fell asleep early."));

        Assert.Equal("Fell asleep early.", NoteShown(cut));
    }

    [Fact]
    public async Task US2_AS5_from_a_rows_Add_record_the_field_opens_with_the_rows_note_and_saving_it_unchanged_keeps_it()
    {
        // At 10:00 on Friday 20 November Monday 16 is Missed, with a note and no record.
        var (cut, goal, row) = await OpenRowAsync(Reading20MinDaily, Utc(20, 10), Mon16, arrange: g => SeedNote(g, Mon16, "Away for the weekend."));
        Assert.Empty(row.Records);

        Assert.Equal("Away for the weekend.", NoteShown(cut));

        cut.Find(".ht-stepper input").Change("20");
        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        Assert.Equal("Away for the weekend.", Assert.Single(_saved).Note);
        Assert.Equal("Away for the weekend.", StoredNote(goal, Mon16));
        Assert.Single(_records.RecordsOfGoal(goal));
    }

    [Fact]
    public async Task US1_AS1_Edit_opens_with_the_note_of_the_records_commitment()
    {
        var (cut, _, _) = await OpenRowAsync(Reading20MinDaily, Utc(20, 10), Mon16, editing: 0, arrange: g =>
        {
            _records.Seed(g, Mon16, RecordValue.FromAmount(12));
            SeedNote(g, Mon16, "Fell asleep early.");
        });

        Assert.Equal("Fell asleep early.", NoteShown(cut));
    }

    [Fact]
    public async Task Choosing_another_entry_in_For_replaces_the_typed_text_with_that_entrys_note_or_empties_it()
    {
        // At 02:00 on Wednesday 18 November "For" lists Wed 18 (Due), then Tue 17 and Mon 16 (Pending); only Tue 17 has a note.
        var (cut, _) = await OpenAsync(Reading20MinDaily, Utc(18, 2), arrange: g => SeedNote(g, new DateOnly(2026, 11, 17), "Back was tired."));
        Assert.Equal(string.Empty, NoteShown(cut));

        cut.Find("textarea").Input("Typed first.");
        Field(cut, "For").QuerySelector("select")!.Change("2026-11-17");
        Assert.Equal("Back was tired.", NoteShown(cut));

        cut.Find("textarea").Input("Typed second.");
        Field(cut, "For").QuerySelector("select")!.Change("2026-11-16");
        Assert.Equal(string.Empty, NoteShown(cut));
    }

    [Fact]
    public async Task Choosing_another_Date_of_the_same_week_keeps_the_fields_content()
    {
        var (cut, _) = await OpenAsync(YogaThreeTimesAWeek, Utc(18, 10), "Yoga, 3 times a week", g => SeedNote(g, Mon16, "Knee was sore."));
        Assert.Equal("Knee was sore.", NoteShown(cut));

        cut.FindAll(".ht-field-pair select")[1].Change("2026-11-17");
        Assert.Equal("Knee was sore.", NoteShown(cut));

        cut.Find("textarea").Input("Typed.");
        cut.FindAll(".ht-field-pair select")[1].Change("2026-11-16");
        Assert.Equal("Typed.", NoteShown(cut));
    }

    [Fact]
    public async Task HP_003_a_note_of_501_characters_is_refused_in_place_of_the_help_and_changes_neither_the_record_nor_the_note()
    {
        var (cut, goal, _) = await OpenRowAsync(Reading20MinDaily, Utc(20, 10), Mon16, editing: 0, arrange: g =>
        {
            _records.Seed(g, Mon16, RecordValue.FromAmount(12));
            SeedNote(g, Mon16, "Fell asleep early.");
        });
        cut.Find(".ht-stepper input").Change("25");
        cut.Find("textarea").Input(new string('a', 501));

        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        var note = Field(cut, "Note");
        var error = Assert.Single(cut.FindAll(".ht-field__error"));
        Assert.Equal("The note can have at most 500 characters.", error.TextContent);
        Assert.Equal(error.Id, note.QuerySelector(".ht-field__error")!.Id);
        Assert.Empty(note.QuerySelectorAll(".ht-field__help"));
        Assert.Equal(error.Id, note.QuerySelector("textarea")!.GetAttribute("aria-describedby"));
        Assert.Empty(_saved);
        Assert.Equal(12, Assert.Single(_records.RecordsOfGoal(goal)).Value.Amount);
        Assert.Equal("Fell asleep early.", StoredNote(goal, Mon16));
    }

    [Fact]
    public async Task US1_AS3_SC_003_saving_a_cleared_field_removes_the_note()
    {
        var (cut, goal, _) = await OpenRowAsync(Reading20MinDaily, Utc(20, 10), Mon16, editing: 0, arrange: g =>
        {
            _records.Seed(g, Mon16, RecordValue.FromAmount(12));
            SeedNote(g, Mon16, "Fell asleep early.");
        });
        Assert.Equal("Fell asleep early.", NoteShown(cut));

        cut.Find("textarea").Input(string.Empty);
        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        Assert.Null(Assert.Single(_saved).Note);
        Assert.Empty(_records.NotesOfGoal(goal));
        Assert.Equal(12, Assert.Single(_records.RecordsOfGoal(goal)).Value.Amount);
    }

    [Fact]
    public async Task US1_AS4_saving_an_unchanged_field_keeps_the_note()
    {
        var (cut, goal, _) = await OpenRowAsync(Reading20MinDaily, Utc(20, 10), Mon16, editing: 0, arrange: g =>
        {
            _records.Seed(g, Mon16, RecordValue.FromAmount(12));
            SeedNote(g, Mon16, "Fell asleep early.");
        });

        cut.Find(".ht-stepper input").Change("25");
        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        Assert.Equal("Fell asleep early.", Assert.Single(_saved).Note);
        Assert.Equal("Fell asleep early.", StoredNote(goal, Mon16));
        Assert.Equal(25, Assert.Single(_records.RecordsOfGoal(goal)).Value.Amount);
    }

    [Fact]
    public async Task US1_AS2_saving_a_changed_field_replaces_the_note()
    {
        var (cut, goal, _) = await OpenRowAsync(Reading20MinDaily, Utc(20, 10), Mon16, editing: 0, arrange: g =>
        {
            _records.Seed(g, Mon16, RecordValue.FromAmount(12));
            SeedNote(g, Mon16, "Fell asleep early.");
        });

        cut.Find("textarea").Input("Slept badly.");
        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        Assert.Equal("Slept badly.", Assert.Single(_saved).Note);
        Assert.Equal("Slept badly.", StoredNote(goal, Mon16));
    }

    [Theory]
    [InlineData("")]
    [InlineData("Slept badly.")]
    public async Task FR_004_Delete_record_leaves_the_note_whatever_the_field_shows(string shown)
    {
        var (cut, goal, _) = await OpenRowAsync(Reading20MinDaily, Utc(20, 10), Mon16, editing: 0, arrange: g =>
        {
            _records.Seed(g, Mon16, RecordValue.FromAmount(12));
            SeedNote(g, Mon16, "Fell asleep early.");
        });
        cut.Find("textarea").Input(shown);

        await cut.FindAll(".ht-dialog__foot .ht-btn")[2].ClickAsync(new MouseEventArgs());

        var saved = Assert.Single(_saved);
        Assert.Empty(saved.Records);
        Assert.Equal("Fell asleep early.", saved.Note);
        Assert.Empty(_records.RecordsOfGoal(goal));
        Assert.Equal("Fell asleep early.", StoredNote(goal, Mon16));
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
    public async Task A_refusal_about_For_sits_under_the_For_select_which_it_describes_and_marks_invalid()
    {
        var (cut, goal) = await OpenAsync(YogaThreeTimesAWeek, Utc(18, 10), "Yoga, 3 times a week");
        Assert.Null(Field(cut, "For").QuerySelector("select")!.GetAttribute("aria-invalid"));
        for (var i = 0; i < RecordLimits.MaxRecordsPerPeriod; i++)
        {
            _records.Seed(goal, Mon16, RecordValue.FromAnswer(YesNoAnswer.Yes));
        }

        cut.FindAll(".ht-seg button")[0].Click();
        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        var error = Assert.Single(cut.FindAll(".ht-field__error"));
        Assert.Equal("This evaluation period already has 50 records.", error.TextContent);
        var select = Field(cut, "For").QuerySelector("select")!;
        Assert.Equal(error.Id, select.GetAttribute("aria-describedby"));
        Assert.Equal("true", select.GetAttribute("aria-invalid"));
        Assert.Empty(_saved);
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
            .Add(x => x.OnSaved, saved => _saved.Add(saved)));
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
        Assert.Equal(string.Empty, NoteShown(cut));

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
        Assert.Equal(string.Empty, NoteShown(cut));

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

    // ---- specs/006-note-editing US3: a period's one note in the record dialog (HP-003) ----

    private const string WeekHelp = "This note is for the whole week. Clear the field to remove it.";

    private static GoalTargetFields SugarAtMost1SweetADay => new()
    {
        BuildOrReduce = BuildOrReduce.Reduce, Measurement = Measurement.Count, Amount = 1, CountUnit = "sweets",
        ScheduleKind = ScheduleKind.Times, Times = 1, Period = SchedulePeriod.Day,
    };

    private static RecordValue Yes => RecordValue.FromAnswer(YesNoAnswer.Yes);

    /// <summary>
    /// The note field shows <paramref name="note"/> and has <paramref name="help"/> as its one help text, which it names
    /// with aria-describedby.
    /// </summary>
    private static void AssertNoteWithHelp(IRenderedComponent<RecordDialog> cut, string note, string help)
    {
        Assert.Equal(note, NoteShown(cut));
        var field = Field(cut, "Note");
        var shown = Assert.Single(field.QuerySelectorAll(".ht-field__help"));
        Assert.Equal(help, shown.TextContent);
        Assert.False(string.IsNullOrEmpty(shown.Id));
        Assert.Equal(shown.Id, field.QuerySelector("textarea")!.GetAttribute("aria-describedby"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(1)]
    public async Task US3_AS1_a_weeks_Add_record_and_Edit_of_any_of_its_records_show_the_weeks_note_with_the_week_help(int? editing)
    {
        // At 10:00 on Friday 20 November the week of 16 November has a Yes on Monday 16 and on Wednesday 18, and its one
        // note under its first date. "Edit" opens with the row and the record, as the record buttons of the Recorded cell do.
        var (cut, _, row) = await OpenRowAsync(YogaThreeTimesAWeek, Utc(20, 10), Mon16, editing, g =>
        {
            _records.Seed(g, Mon16, Yes);
            _records.Seed(g, new DateOnly(2026, 11, 18), Yes);
            SeedNote(g, Mon16, "Knee was sore.");
        }, "Yoga, 3 times a week");
        Assert.Equal(2, row.Records.Count);

        AssertNoteWithHelp(cut, "Knee was sore.", WeekHelp);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    public async Task US3_AS2_a_days_Add_record_and_Edit_of_its_record_show_the_days_note_with_the_day_help(int? editing)
    {
        // At 10:00 on Friday 20 November Monday 16 has one record of 1 sweet and the note "Birthday cake at work.".
        var (cut, _, row) = await OpenRowAsync(SugarAtMost1SweetADay, Utc(20, 10), Mon16, editing, g =>
        {
            _records.Seed(g, Mon16, RecordValue.FromAmount(1));
            SeedNote(g, Mon16, "Birthday cake at work.");
        }, "Sugar, at most 1 sweet a day");
        Assert.Single(row.Records);

        AssertNoteWithHelp(cut, "Birthday cake at work.", "This note is for the whole day. Clear the field to remove it.");
    }

    [Theory]
    [InlineData("Better today.", "Better today.")]
    [InlineData("", null)]
    public async Task US3_AS3_a_Yes_for_another_date_of_the_week_with_a_note_makes_it_the_weeks_note_and_with_an_empty_field_removes_it(
        string typed, string? stored)
    {
        // At 10:00 on Wednesday 18 November the week of 16 November has a Yes on Monday 16 and the note "Knee was sore.";
        // its "Add record" records a Yes for Tuesday 17.
        var (cut, goal, _) = await OpenRowAsync(YogaThreeTimesAWeek, Utc(18, 10), Mon16, arrange: g =>
        {
            _records.Seed(g, Mon16, Yes);
            SeedNote(g, Mon16, "Knee was sore.");
        }, name: "Yoga, 3 times a week");
        Assert.Equal("Knee was sore.", NoteShown(cut));

        cut.Find("select").Change("2026-11-17");
        cut.FindAll(".ht-seg button")[0].Click();
        cut.Find("textarea").Input(typed);
        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        Assert.Equal(stored, Assert.Single(_saved).Note);
        Assert.Equal(stored, StoredNote(goal, Mon16));
        Assert.Equal(stored is null ? 0 : 1, _records.NotesOfGoal(goal).Count);
        Assert.Equal([Mon16, new DateOnly(2026, 11, 17)], _records.RecordsOfGoal(goal).Select(r => r.Date));
    }

    [Fact]
    public async Task Edge_case_Edit_of_one_record_of_a_week_with_50_records_shows_the_weeks_note_with_the_week_help_and_saving_replaces_it()
    {
        // At 10:00 on Wednesday 18 November the week of 16 November has 50 Yes records and the note "Knee was sore.": it
        // offers no "Add record", but each record's "Edit" still edits the week's note.
        var (cut, goal, row) = await OpenRowAsync(YogaThreeTimesAWeek, Utc(18, 10), Mon16, editing: 49, arrange: g =>
        {
            for (var i = 0; i < 50; i++)
            {
                _records.Seed(g, Mon16, Yes);
            }

            SeedNote(g, Mon16, "Knee was sore.");
        }, name: "Yoga, 3 times a week");
        Assert.Equal(50, row.Records.Count);

        AssertNoteWithHelp(cut, "Knee was sore.", WeekHelp);

        cut.Find("textarea").Input("Better today.");
        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        Assert.Equal("Better today.", Assert.Single(_saved).Note);
        Assert.Equal("Better today.", StoredNote(goal, Mon16));
        Assert.Equal(50, _records.RecordsOfGoal(goal).Count);
    }

    // ---- slice 8: the chosen entry's own settings (research R-07, AC-020, AC-080, contracts/host.md; T059) ----

    private const string CommitmentNoteHelp = "Clear the field to remove the note.";
    private const string DayNoteHelp = "This note is for the whole day. Clear the field to remove it.";

    private static readonly DateOnly Mon2 = new(2026, 11, 2);
    private static readonly DateOnly Wed11 = new(2026, 11, 11);
    private static readonly DateOnly Thu12 = new(2026, 11, 12);
    private static readonly DateOnly Sun15 = new(2026, 11, 15);
    private static readonly DateOnly Tue17 = new(2026, 11, 17);

    private static GoalTargetFields Minutes12Daily(BuildOrReduce side) => new()
    {
        BuildOrReduce = side, Measurement = Measurement.Duration, Amount = 12, DurationUnit = DurationUnit.Minutes,
        ScheduleKind = ScheduleKind.FixedDays, Days = Enum.GetValues<DayOfWeek>().ToHashSet(),
    };

    private static GoalTargetFields Meditation10MinDaily => new()
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = 10, DurationUnit = DurationUnit.Minutes,
        ScheduleKind = ScheduleKind.FixedDays, Days = Enum.GetValues<DayOfWeek>().ToHashSet(),
    };

    private static GoalSettingsVersion VersionOf(GoalTargetFields fields, DateOnly from)
    {
        Assert.True(GoalTarget.TryCreate(fields, out var target, out _));
        return new GoalSettingsVersion(new GoalSettings(target, NamedTimeZone.Utc), from);
    }

    /// <summary>
    /// Stores a goal of the user straight in the goal fake, through the attempts constructor, so it can have changes and
    /// earlier attempts made while they were current, as the change, ending and new-attempt use cases will write them.
    /// </summary>
    private GoalId SeedGoal(params GoalAttemptVersions[] attempts)
    {
        var habit = _catalog.Add(HabitSamples.Catalog("Reading"));
        var owner = _user.AccountId!.Value;
        var goal = new Goal(new GoalId(Guid.NewGuid()), owner, new GoalHabit(habit.Id, HabitKind.Catalog), null, attempts);
        ((FakeGoalStore)Services.GetRequiredService<IGoalStore>()).Add(
            owner, goal, new GoalHabitView(habit.Id, HabitKind.Catalog, "Reading", habit.Definition.Category, true));
        return goal.Id;
    }

    private static List<string> Helps(AngleSharp.Dom.IElement field) =>
        field.QuerySelectorAll(".ht-field__help").Select(h => h.TextContent).ToList();

    [Fact]
    public async Task AC_020_after_a_change_to_Reduce_the_row_of_11_Nov_keeps_the_Build_sub_line_field_and_help_and_12_Nov_has_the_Reduce_ones()
    {
        // "12 min daily", Build from Monday 2 November, changed on Wednesday 11 November to Reduce, at most 12 min a day,
        // from Thursday 12 November. At 10:00 on Monday 16 November Wednesday 11 and Thursday 12 are Missed; each History
        // row's "Add record" opens the dialog fixed to it, with the History's goal-level settings the Reduce ones.
        var goal = SeedGoal(new GoalAttemptVersions(new GoalAttempt(GoalAttempt.First, Mon2),
            [VersionOf(Minutes12Daily(BuildOrReduce.Build), Mon2), VersionOf(Minutes12Daily(BuildOrReduce.Reduce), Thu12)]));
        _clock.SetUtcNow(Utc(16, 10));
        await using var scope = Services.CreateAsyncScope();
        var read = await scope.ServiceProvider.GetRequiredService<RecordingService>()
            .GetHistoryAsync(_user, goal, Mon2, Mon16, CancellationToken.None);
        Assert.Equal(HistoryOutcome.Found, read.Outcome);
        var history = read.History!;
        Assert.Equal(BuildOrReduce.Reduce, history.BuildOrReduce);

        var build = RenderExcusable(goal, "12 min daily", history, history.Entries.Single(e => e.FirstDate == Wed11));
        var reduce = RenderExcusable(goal, "12 min daily", history, history.Entries.Single(e => e.FirstDate == Thu12));

        Assert.Equal("Build · at least 12 min · every day", build.Find(".ht-dialog__sub").TextContent);
        Assert.Equal(
            ["Partial time is kept. Below 12 min the day counts as Failed when it is finalized."],
            Helps(Field(build, "Time spent")));
        Assert.Equal("Reduce · at most 12 min daily", reduce.Find(".ht-dialog__sub").TextContent);
        Assert.Equal(
            ["At most 12 min. Enter 0 if you had none: an empty day cannot prove you stayed under the limit."],
            Helps(Field(reduce, "Time spent")));
    }

    /// <summary>
    /// AC-080: "Meditation, 10 min daily" (Build, duration, all seven days) from Monday 2 November, ended on Monday 16
    /// November; attempt 2 from Tuesday 17 November is Reduce, at most 2 cups a day. At 10:00 on Tuesday 17 November what
    /// can be recorded is attempt 2's Due Tuesday 17 and attempt 1's Pending Sunday 15 (its grace period ends at 00:00 on
    /// Wednesday 18); Saturday 14's ended at 00:00 that day.
    /// </summary>
    private async Task<(IRenderedComponent<RecordDialog> Cut, GoalHistory Recordable)> AttemptTwoOfReduceAsync()
    {
        var goal = SeedGoal(
            new GoalAttemptVersions(new GoalAttempt(GoalAttempt.First, Mon2) { Ending = new GoalEnding(Mon16, Outcome.Abandoned, null) },
                [VersionOf(Meditation10MinDaily, Mon2)]),
            new GoalAttemptVersions(new GoalAttempt(2, Tue17), [VersionOf(CoffeeAtMost2Cups, Tue17)]));
        _clock.SetUtcNow(Utc(17, 10));
        var recordable = await RecordableAsync(goal);
        Assert.Equal([Tue17, Sun15], recordable.Entries.Select(e => e.FirstDate).OrderDescending());
        return (RenderExcusable(goal, "Meditation, 10 min daily", recordable), recordable);
    }

    [Fact]
    public async Task AC_080_attempt_1s_row_under_an_attempt_2_of_Reduce_per_day_has_the_Build_sub_line_the_duration_field_and_the_commitment_note_help()
    {
        var (cut, recordable) = await AttemptTwoOfReduceAsync();
        Assert.Equal(GoalScheduleKind.PeriodBased, recordable.GoalScheduleKind);

        Field(cut, "For").QuerySelector("select")!.Change("2026-11-15");

        Assert.Equal("Build · at least 10 min · every day", cut.Find(".ht-dialog__sub").TextContent);
        Assert.Equal(["For", "Time spent", "Note (optional)"], cut.FindAll(".ht-field__label").Select(Text));
        var field = Field(cut, "Time spent");
        Assert.Equal("Time spent", field.QuerySelector(".ht-stepper input")!.GetAttribute("aria-label"));
        Assert.Equal("minutes", field.QuerySelector(".ht-unit")!.TextContent);
        Assert.Equal(["Partial time is kept. Below 10 min the day counts as Failed when it is finalized."], Helps(field));
        AssertNoteWithHelp(cut, string.Empty, CommitmentNoteHelp);
        Assert.Contains("Excuse instead", cut.Markup, StringComparison.Ordinal);

        cut.Find(".ht-stepper input").Change("5");

        Assert.Equal("5 of 10 min", Text(cut.Find(".ht-target")));
    }

    [Fact]
    public async Task AC_080_choosing_attempt_2s_row_switches_the_sub_line_field_unit_help_and_note_help_and_back()
    {
        var (cut, _) = await AttemptTwoOfReduceAsync();
        var select = () => Field(cut, "For").QuerySelector("select")!;
        select().Change("2026-11-15");
        cut.Find(".ht-stepper input").Change("5");

        select().Change("2026-11-17");

        Assert.Equal("Reduce · at most 2 cups a day", cut.Find(".ht-dialog__sub").TextContent);
        Assert.Equal(["For", "Amount", "Note (optional)"], cut.FindAll(".ht-field__label").Select(Text));
        var field = Field(cut, "Amount");
        Assert.Equal("cups", field.QuerySelector(".ht-unit")!.TextContent);
        Assert.Null(field.QuerySelector(".ht-stepper input")!.GetAttribute("value"));
        Assert.Equal("0 of at most 2 cups", Text(field.QuerySelector(".ht-target")!));
        Assert.Equal(["At most 2 cups. Enter 0 if you had none: an empty day cannot prove you stayed under the limit."], Helps(field));
        AssertNoteWithHelp(cut, string.Empty, DayNoteHelp);
        Assert.DoesNotContain("Excuse instead", cut.Markup, StringComparison.Ordinal);

        select().Change("2026-11-15");

        Assert.Equal("Build · at least 10 min · every day", cut.Find(".ht-dialog__sub").TextContent);
        Assert.Equal("minutes", Field(cut, "Time spent").QuerySelector(".ht-unit")!.TextContent);
        AssertNoteWithHelp(cut, string.Empty, CommitmentNoteHelp);
    }

    // ---- slice 8: the attempt view as an opener (HP-014, US3-AS4, US3-AS15, FR-014; T092) ----

    [Fact]
    public async Task HP_014_US3_AS15_from_the_attempt_views_Add_record_the_dialog_uses_attempt_1s_settings_and_the_save_counts_in_attempt_1_only()
    {
        // US3-AS4: "Meditation, 10 min daily" ended on Monday 16 November, restarted with 5 minutes from Monday 23 November;
        // attempt 1's History read on Thursday 19 November at 10:00, as the attempt view reads it.
        var goal = MeditationAttempts.SeedRestarted(this, _user, MeditationAttempts.MinutesDaily(5), MeditationAttempts.Mon23);
        _clock.SetUtcNow(MeditationAttempts.Utc(19, 10));
        var attempt = await MeditationAttempts.AttemptAsync(this, _user, goal, 1);
        var row = attempt.History.Entries.Single(e => e.FirstDate == MeditationAttempts.Sat14);
        Assert.Equal(EvaluationState.Missed, row.State);

        var cut = RenderExcusable(goal, attempt.Attempt.Name, attempt.History, row);

        Assert.Equal("Record · Meditation, 10 min daily", cut.Find(".ht-dialog__head h2").TextContent);
        Assert.Equal("Build · at least 10 min · every day", cut.Find(".ht-dialog__sub").TextContent);
        Assert.Equal("Sat 14 Nov", Text(Field(cut, "For").QuerySelector("span:not(.ht-field__label)")!));
        Assert.Equal("minutes", Field(cut, "Time spent").QuerySelector(".ht-unit")!.TextContent);
        Assert.DoesNotContain("Excuse instead", cut.Markup, StringComparison.Ordinal);

        cut.Find(".ht-stepper input").Change("10");
        Assert.Equal("10 of 10 min", Text(cut.Find(".ht-target")));
        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        var saved = Assert.Single(_saved);
        Assert.Equal((MeditationAttempts.Sat14, EvaluationState.Met, true), (saved.FirstDate, saved.State, saved.IsCorrection));
        Assert.Contains(_records.RecordsOfGoal(goal), r => r.Date == MeditationAttempts.Sat14 && r.Value.Amount == 10);
        Assert.Equal("92% (13 of 14)", (await MeditationAttempts.AttemptAsync(this, _user, goal, 1)).Attempt.Stats.ConsistencyText);
        var latest = (await MeditationAttempts.AttemptsAsync(this, _user, goal))[0];
        Assert.Equal((2, "N/A"), (latest.Number, latest.Stats.ConsistencyText));
    }
}
