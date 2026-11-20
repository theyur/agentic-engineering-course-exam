using Bunit;
using Bunit.TestDoubles;
using HT.Application.Goals;
using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using HT.Domain.Time;
using HT.Ui.Components.Pages;
using HT.UnitTests.Fakes;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace HT.UiTests.Components.Pages;

/// <summary>
/// HP-004, US2-AS7, US7-AS8, FR-020, and slice 5 HP-001 and US1-AS9: the goal page over the real GoalService,
/// RecordingService and the fakes, with "Record" when something can be recorded now. Today is Tuesday 17 November
/// 2026 in UTC, the zone of the signed-in user (no stored zone, the UTC default), as in the design's preview.
/// </summary>
public sealed class GoalDetailPageTests : BunitContext
{
    private const string Sentence = "Changes apply from the next commitment. Past results keep their rules.";
    private const string NoticeText = "This habit is no longer in the public catalog. Your goal keeps working.";

    private static readonly DateOnly Today = new(2026, 11, 17);

    private readonly BunitAuthorizationContext _authorization;
    private readonly FakeCatalogHabitStore _catalog;
    private readonly FakeCustomHabitStore _customHabits;
    private readonly FakeGoalStore _goals;
    private readonly FakeRecordStore _records;
    private int _notFound;

    public GoalDetailPageTests()
    {
        _authorization = AddAuthorization();
        _catalog = this.AddCatalog();
        _customHabits = this.AddCustomHabits();
        _goals = this.AddGoals();
        _records = (FakeRecordStore)Services.GetRequiredService<IRecordStore>();
        ((FakeTimeProvider)Services.GetRequiredService<TimeProvider>()).SetUtcNow(new DateTimeOffset(2026, 11, 17, 10, 0, 0, TimeSpan.Zero));
        Services.GetRequiredService<BunitNavigationManager>().OnNotFound += (_, _) => _notFound++;
    }

    private ActingUser SignInUser() => ActingUser.ForAccount(new AccountId(_authorization.SignIn("RegisteredUser")), Role.RegisteredUser);

    private IRenderedComponent<GoalDetail> RenderGoal(GoalId id) => Render<GoalDetail>(p => p.Add(x => x.Id, id.Value));

    private async Task<GoalId> CreateAsync(ActingUser user, GoalHabit habit, GoalTargetFields target, DateOnly start)
    {
        await using var scope = Services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<GoalService>().CreateAsync(
            user, new GoalInput { Habit = habit, Target = target, StartDate = start }, CancellationToken.None);
        Assert.Equal(GoalCreateOutcome.Created, result.Outcome);
        return result.GoalId!.Value;
    }

    private static string Text(AngleSharp.Dom.IElement element) =>
        string.Join(" ", element.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static List<(string Label, string Value)> Rows(IRenderedComponent<GoalDetail> cut) =>
        cut.FindAll("dl.ht-kv dt").Zip(cut.FindAll("dl.ht-kv dd"), (dt, dd) => (Text(dt), Text(dd))).ToList();

    private static GoalTargetFields Reading20MinDaily => new()
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = 20, DurationUnit = DurationUnit.Minutes,
        ScheduleKind = ScheduleKind.FixedDays, Days = Enum.GetValues<DayOfWeek>().ToHashSet(),
    };

    private static GoalTargetFields YesNoOnMonWedFri => new()
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo, ScheduleKind = ScheduleKind.FixedDays,
        Days = new HashSet<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
    };

    [Fact]
    public void US2_AS7_a_commitment_based_goal_that_has_started_shows_the_head_the_attempt_line_and_the_Settings_card()
    {
        var owner = new AccountId(_authorization.SignIn("RegisteredUser"));
        var yoga = _catalog.Add(HabitSamples.Catalog("Yoga", category: Category.Body));
        var start = new DateOnly(2026, 7, 14);
        Assert.True(GoalTarget.TryCreate(YesNoOnMonWedFri, out var target, out _));
        var goal = _goals.Add(owner,
            new Goal(new GoalId(Guid.NewGuid()), owner, new GoalHabit(yoga.Id, HabitKind.Catalog), start, null, new GoalAttempt(1, start),
                [new GoalSettingsVersion(new GoalSettings(target, NamedTimeZone.Utc), start)]),
            new GoalHabitView(yoga.Id, HabitKind.Catalog, "Yoga", Category.Body, true));

        var cut = RenderGoal(goal.Id);

        Assert.Equal(0, _notFound);
        var back = cut.Find("a.ht-back");
        Assert.Equal("← Goals", back.TextContent);
        Assert.Equal("/goals", back.GetAttribute("href"));
        Assert.Equal("Yoga, 3 days a week", cut.Find(".ht-page-head h1").TextContent);
        Assert.Equal(["Body", "Build", "Commitment-based"], cut.FindAll(".ht-head-tags .ht-chip").Select(c => c.TextContent));
        Assert.Equal("Attempt 1 · started 14 Jul", cut.Find(".ht-head-tags .ht-attempt").TextContent);
        // Slice 5 (T055): the History card in the left column, the Settings card in the right one; slice 6 (HP-001): the
        // GoalStats card above the History card.
        var columns = cut.FindAll(".ht-grid-2 > .ht-stack");
        Assert.Equal(2, columns.Count);
        Assert.Equal(["Consistency and streaks", "History"], columns[0].QuerySelectorAll(".ht-card .ht-card__title").Select(Text));
        Assert.Equal("Settings", columns[1].QuerySelector(".ht-card .ht-card__title")!.TextContent);
        Assert.Equal(
            [
                ("Habit", "Yoga · catalog"), ("Build or reduce", "Build"), ("Measure", "Yes / no"), ("Target", "3 days a week"),
                ("Schedule", "Mon · Wed · Fri"), ("Evaluated", "Each scheduled day"), ("Started", "14 Jul 2026"),
            ],
            Rows(cut));
        Assert.Equal(Sentence, cut.Find(".ht-grid-2 > .ht-stack:last-child .ht-card .ht-card__sub").TextContent);
        Assert.Empty(cut.FindAll(".ht-notice"));
        // Tuesday 17 November is not scheduled, but Monday 16 is Pending: "Record" is the page head's one action (HP-001).
        Assert.Equal(["Record"], cut.FindAll(".ht-page-head button").Select(b => b.TextContent));
        foreach (var absent in new[] { "Edit settings", "Pause", "End goal", "Earlier attempts", "Excuse" })
        {
            Assert.DoesNotContain(absent, cut.Markup, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task US2_AS7_a_period_based_goal_on_a_custom_habit_that_starts_later_says_starts_and_its_first_week()
    {
        var user = SignInUser();
        var stretch = _customHabits.Add(user.AccountId!.Value, HabitSamples.Definition("Evening stretch", Category.Body));
        var id = await CreateAsync(user, new GoalHabit(stretch.Id, HabitKind.Custom), new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo, ScheduleKind = ScheduleKind.Times, Times = 3, Period = SchedulePeriod.Week,
        }, new DateOnly(2026, 11, 25));

        var cut = RenderGoal(id);

        Assert.Equal("Evening stretch, 3 times a week", cut.Find(".ht-page-head h1").TextContent);
        Assert.Equal(["Body", "Build", "Period-based"], cut.FindAll(".ht-head-tags .ht-chip").Select(c => c.TextContent));
        Assert.Equal("Attempt 1 · starts 25 Nov", cut.Find(".ht-attempt").TextContent);
        Assert.Equal(
            [
                ("Habit", "Evening stretch · custom"), ("Build or reduce", "Build"), ("Measure", "Yes / no"), ("Target", "3 times a week"),
                ("Schedule", "Flexible · 3 times a week"), ("Evaluated", "Each week"), ("First week", "30 Nov 2026"), ("Starts", "25 Nov 2026"),
            ],
            Rows(cut));
        Assert.Equal(Sentence, cut.Find(".ht-card__sub").TextContent);
        Assert.Empty(cut.FindAll(".ht-notice"));
        Assert.Empty(cut.FindAll(".ht-page-head button"));
        Assert.DoesNotContain(">Record<", cut.Markup, StringComparison.Ordinal);
        // Slice 5 (T055): no current date, so no History card; the Settings card stands alone in ht-grid-2.
        Assert.Equal(["Settings"], cut.FindAll(".ht-card__title").Select(Text));
        Assert.Single(cut.FindAll(".ht-grid-2 > section.ht-card"));
        Assert.Empty(cut.FindAll(".ht-grid-2 > .ht-stack"));
    }

    [Fact]
    public async Task HP_001_a_goal_with_a_Due_commitment_has_the_primary_action_Record_and_no_other_action()
    {
        var user = SignInUser();
        var reading = _catalog.Add(HabitSamples.Reading());
        var id = await CreateAsync(user, new GoalHabit(reading.Id, HabitKind.Catalog), Reading20MinDaily, Today);

        var cut = RenderGoal(id);

        var actions = cut.FindAll(".ht-page-head button");
        var record = Assert.Single(actions);
        Assert.Equal("Record", record.TextContent);
        Assert.Equal(["ht-btn", "ht-btn--primary"], record.ClassList.ToArray());
        Assert.Equal("dialog", record.GetAttribute("aria-haspopup"));
        Assert.Equal("ht-spacer", record.PreviousElementSibling!.ClassName);
        Assert.Empty(cut.FindAll("[role=dialog]"));
    }

    [Fact]
    public async Task HP_001_a_per_week_goal_in_its_first_partial_week_has_no_Record()
    {
        var user = SignInUser();
        var reading = _catalog.Add(HabitSamples.Reading());
        // Tuesday 17 November: the first week starts on Monday 23, so nothing can be recorded yet.
        var id = await CreateAsync(user, new GoalHabit(reading.Id, HabitKind.Catalog), new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo, ScheduleKind = ScheduleKind.Times, Times = 3, Period = SchedulePeriod.Week,
        }, Today);

        var cut = RenderGoal(id);

        Assert.Equal("Attempt 1 · started 17 Nov", cut.Find(".ht-attempt").TextContent);
        Assert.Empty(cut.FindAll(".ht-page-head button"));
        // T055: a current date but no begun entry, so no History card; the Settings card stands alone in ht-grid-2.
        Assert.Equal(["Settings"], cut.FindAll(".ht-card__title").Select(Text));
        Assert.Empty(cut.FindAll(".ht-grid-2 > .ht-stack"));
        Assert.Empty(cut.FindAll(".ht-stats"));
    }

    [Fact]
    public void Slice_6_US1_AS8_HP_001_the_GoalStats_card_shows_the_consistency_above_the_History_card()
    {
        // Reading, 20 min daily, from Tuesday 10 November in UTC. At 10:00 on Tuesday 17 November the days to Saturday 14
        // are final: 10, 11 and 13 Met (20 min), 12 Failed (10 min), 14 Missed; Sunday 15 and Monday 16 are Pending and
        // Tuesday 17 is Due.
        var owner = new AccountId(_authorization.SignIn("RegisteredUser"));
        var reading = _catalog.Add(HabitSamples.Reading());
        var start = new DateOnly(2026, 11, 10);
        Assert.True(GoalTarget.TryCreate(Reading20MinDaily, out var target, out _));
        var goal = _goals.Add(owner,
            new Goal(new GoalId(Guid.NewGuid()), owner, new GoalHabit(reading.Id, HabitKind.Catalog), start, null, new GoalAttempt(1, start),
                [new GoalSettingsVersion(new GoalSettings(target, NamedTimeZone.Utc), start)]),
            new GoalHabitView(reading.Id, HabitKind.Catalog, reading.Definition.Name.Value, reading.Definition.Category, true));
        foreach (var (day, minutes) in new[] { (10, 20), (11, 20), (12, 10), (13, 20) })
        {
            _records.Seed(goal.Id, new DateOnly(2026, 11, day), RecordValue.FromAmount(minutes));
        }

        var cut = RenderGoal(goal.Id);

        var left = cut.FindAll(".ht-grid-2 > .ht-stack")[0];
        var card = left.Children[0];
        Assert.Equal("This attempt", card.QuerySelector(".ht-eyebrow")!.TextContent);
        Assert.Equal("Consistency and streaks", card.QuerySelector("h2.ht-card__title")!.TextContent);
        Assert.Equal(
            [("Consistency", "60% (3 of 5)"), ("Streak, commitments", "0"), ("Longest, commitments", "2")],
            card.QuerySelectorAll(".ht-stat").Select(s => (s.QuerySelector(".ht-stat__label")!.TextContent, s.QuerySelector(".ht-stat__value")!.TextContent)));
        Assert.Equal("Last 7 commitments: 3 met, 1 failed, 1 missed, 2 pending", card.QuerySelector(".ht-strip")!.GetAttribute("aria-label"));
        Assert.Equal("History", left.Children[1].QuerySelector(".ht-card__title")!.TextContent);
    }

    [Fact]
    public async Task Slice_6_HP_001_a_goal_not_started_has_no_GoalStats_card()
    {
        var user = SignInUser();
        var reading = _catalog.Add(HabitSamples.Reading());
        var id = await CreateAsync(user, new GoalHabit(reading.Id, HabitKind.Catalog), Reading20MinDaily, new DateOnly(2026, 11, 25));

        var cut = RenderGoal(id);

        Assert.Equal(["Settings"], cut.FindAll(".ht-card__title").Select(Text));
        Assert.Empty(cut.FindAll(".ht-stats"));
        Assert.DoesNotContain("Consistency", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public async Task US1_AS9_Record_opens_the_RecordDialog_and_a_save_records_today_and_closes_it()
    {
        var user = SignInUser();
        var reading = _catalog.Add(HabitSamples.Reading());
        var id = await CreateAsync(user, new GoalHabit(reading.Id, HabitKind.Catalog), Reading20MinDaily, Today);
        var cut = RenderGoal(id);
        Assert.Equal("—", Text(cut.Find("table[aria-label=History] tbody tr").QuerySelectorAll("td")[1]));

        cut.Find(".ht-page-head button").Click();

        var dialog = cut.Find("[role=dialog]");
        Assert.Equal("Record · Reading, 20 min daily", dialog.QuerySelector("h2")!.TextContent);
        Assert.Equal("Build · at least 20 min · every day", dialog.QuerySelector(".ht-dialog__sub")!.TextContent);
        Assert.Equal(["Today, Tue 17 Nov"], dialog.QuerySelectorAll("select option").Select(o => o.TextContent));
        Assert.Equal("Time spent", dialog.QuerySelector(".ht-stepper input")!.GetAttribute("aria-label"));

        cut.Find(".ht-stepper input").Change("12");
        Assert.Equal("12 of 20 min", Text(cut.Find(".ht-target")));
        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        Assert.Empty(cut.FindAll("[role=dialog]"));
        var record = Assert.Single(_records.RecordsOfGoal(id));
        Assert.Equal((Today, 12), (record.Date, record.Value.Amount!.Value));
        Assert.Equal(["Record"], cut.FindAll(".ht-page-head button").Select(b => b.TextContent));
        // T055: after a save the history is read again.
        var row = Assert.Single(cut.FindAll("table[aria-label=History] tbody tr"));
        Assert.Equal(["Tue 17 Nov", "12 min", "Due today"], row.QuerySelectorAll("td").Take(3).Select(Text));
    }

    [Fact]
    public async Task US3_a_Missed_rows_Add_record_and_then_Edit_open_the_RecordDialog_for_that_row_and_give_the_focus_back_to_it()
    {
        // Reading, 20 min daily, from Tuesday 10 November in UTC; at 10:00 on Tuesday 17 November Saturday 14's grace
        // period ended at 00:00 that day, so it is Missed.
        var owner = new AccountId(_authorization.SignIn("RegisteredUser"));
        var reading = _catalog.Add(HabitSamples.Reading());
        var start = new DateOnly(2026, 11, 10);
        Assert.True(GoalTarget.TryCreate(Reading20MinDaily, out var target, out _));
        var goal = _goals.Add(owner,
            new Goal(new GoalId(Guid.NewGuid()), owner, new GoalHabit(reading.Id, HabitKind.Catalog), start, null, new GoalAttempt(1, start),
                [new GoalSettingsVersion(new GoalSettings(target, NamedTimeZone.Utc), start)]),
            new GoalHabitView(reading.Id, HabitKind.Catalog, reading.Definition.Name.Value, reading.Definition.Category, true));
        var cut = RenderGoal(goal.Id);
        AngleSharp.Dom.IElement Row() =>
            cut.FindAll("table[aria-label=History] tbody tr").Single(r => Text(r.QuerySelector("td")!) == "Sat 14 Nov");
        Assert.Equal(["Sat 14 Nov", "—", "Missed"], Row().QuerySelectorAll("td").Take(3).Select(Text));
        var button = Row().QuerySelector("td.ht-actions > button:last-of-type")!;
        // The row's record control stays its last button, so the focus goes back to the same element after each save
        // (specs/006-note-editing: its "Add note" comes before it while the row has no record; the owner's order,
        // 2026-11-20).
        var buttonId = button.GetAttribute("blazor:elementReference");
        Assert.False(string.IsNullOrEmpty(buttonId));
        Assert.Equal("Add record for Sat 14 Nov", button.GetAttribute("aria-label"));

        await button.ClickAsync(new MouseEventArgs());
        var dialog = cut.Find("[role=dialog]");
        Assert.Equal("Sat 14 Nov", Text(dialog.QuerySelector(".ht-field > span:not(.ht-field__label)")!));
        Assert.Empty(dialog.QuerySelectorAll("select"));
        cut.Find(".ht-stepper input").Change("20");
        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        Assert.Empty(cut.FindAll("[role=dialog]"));
        Assert.Equal(["Sat 14 Nov", "20 min", "Met"], Row().QuerySelectorAll("td").Take(3).Select(Text));
        var edit = Row().QuerySelector("td.ht-actions > button:last-of-type")!;
        Assert.Equal(("Edit", "Edit record of Sat 14 Nov"), (edit.TextContent, edit.GetAttribute("aria-label")));
        var focused = JSInterop.VerifyFocusAsyncInvoke(2).Select(i => Assert.IsType<Microsoft.AspNetCore.Components.ElementReference>(i.Arguments[0]).Id).ToList();
        Assert.Equal(buttonId, focused[1]);

        await edit.ClickAsync(new MouseEventArgs());
        Assert.Equal("20", cut.Find(".ht-stepper input").GetAttribute("value"));
        await cut.FindAll(".ht-dialog__foot .ht-btn")[2].ClickAsync(new MouseEventArgs());

        Assert.Empty(cut.FindAll("[role=dialog]"));
        Assert.Equal(["Sat 14 Nov", "—", "Missed"], Row().QuerySelectorAll("td").Take(3).Select(Text));
        Assert.Equal("Add record", Row().QuerySelector("td.ht-actions > button:last-of-type")!.TextContent);
        Assert.Empty(_records.RecordsOfGoal(goal.Id));
        var refocused = JSInterop.VerifyFocusAsyncInvoke(4).Select(i => Assert.IsType<Microsoft.AspNetCore.Components.ElementReference>(i.Arguments[0]).Id).ToList();
        Assert.Equal(buttonId, refocused[3]);
    }

    [Fact]
    public async Task US4_a_Missed_rows_Add_note_opens_the_NoteDialog_and_a_save_shows_the_note_in_the_row_and_in_Recent_notes()
    {
        // Reading, 20 min daily, from Tuesday 10 November in UTC; at 10:00 on Tuesday 17 November Saturday 14's grace
        // period ended at 00:00 that day, so it is Missed.
        var owner = new AccountId(_authorization.SignIn("RegisteredUser"));
        var reading = _catalog.Add(HabitSamples.Reading());
        var start = new DateOnly(2026, 11, 10);
        Assert.True(GoalTarget.TryCreate(Reading20MinDaily, out var target, out _));
        var goal = _goals.Add(owner,
            new Goal(new GoalId(Guid.NewGuid()), owner, new GoalHabit(reading.Id, HabitKind.Catalog), start, null, new GoalAttempt(1, start),
                [new GoalSettingsVersion(new GoalSettings(target, NamedTimeZone.Utc), start)]),
            new GoalHabitView(reading.Id, HabitKind.Catalog, reading.Definition.Name.Value, reading.Definition.Category, true));
        var cut = RenderGoal(goal.Id);
        AngleSharp.Dom.IElement Row() =>
            cut.FindAll("table[aria-label=History] tbody tr").Single(r => Text(r.QuerySelector("td")!) == "Sat 14 Nov");
        // specs/006-note-editing HP-002: the note control is the first button of the actions column, before "Add record".
        AngleSharp.Dom.IElement NoteButton() => Row().QuerySelectorAll("td.ht-actions > button")[0];
        // No note yet: no Recent notes card, and the right column holds the Settings card alone.
        Assert.Equal(["Consistency and streaks", "History", "Settings"], cut.FindAll(".ht-card__title").Select(Text));
        Assert.Equal(["Add note", "Add record"], Row().QuerySelectorAll("td.ht-actions > button").Select(Text));
        Assert.Equal(("Add note", "Add note for Sat 14 Nov"), (NoteButton().TextContent, NoteButton().GetAttribute("aria-label")));
        Assert.Equal("—", Text(Row().QuerySelectorAll("td")[3]));
        // The note control stays the same element when the save turns it into "Edit note", so the focus goes back to it.
        var buttonId = NoteButton().GetAttribute("blazor:elementReference");
        Assert.False(string.IsNullOrEmpty(buttonId));

        await NoteButton().ClickAsync(new MouseEventArgs());
        var dialog = cut.Find("[role=dialog]");
        Assert.Equal("Add note · Reading, 20 min daily", dialog.QuerySelector("h2")!.TextContent);
        Assert.Equal("Sat 14 Nov · Missed", dialog.QuerySelector(".ht-dialog__sub")!.TextContent);
        cut.Find("textarea").Input("Away for the weekend.");
        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        Assert.Empty(cut.FindAll("[role=dialog]"));
        Assert.Equal("Away for the weekend.", Text(Row().QuerySelectorAll("td")[3]));
        Assert.Empty(Row().QuerySelectorAll("td")[3].Children);
        Assert.Equal(("Edit note", "Edit note of Sat 14 Nov"), (NoteButton().TextContent, NoteButton().GetAttribute("aria-label")));
        Assert.Equal("Missed", Text(Row().QuerySelectorAll("td")[2]));
        var right = cut.FindAll(".ht-grid-2 > .ht-stack")[1];
        Assert.Equal(["Settings", "Recent notes"], right.QuerySelectorAll(".ht-card__title").Select(Text));
        var item = Assert.Single(right.QuerySelectorAll("div.ht-goal"));
        Assert.Equal(["\"Away for the weekend.\"", "Missed"], item.QuerySelectorAll("div.ht-goal__top > span:not(.ht-spacer)").Select(Text));
        Assert.Equal("Sat 14 Nov", Text(item.QuerySelector("div.ht-goal__meta")!));
        // Focus: the TextArea on open, then the row's note control, now "Edit note", on close.
        var focused = JSInterop.VerifyFocusAsyncInvoke(2).Select(i => Assert.IsType<Microsoft.AspNetCore.Components.ElementReference>(i.Arguments[0]).Id).ToList();
        Assert.Equal(buttonId, focused[1]);

        await NoteButton().ClickAsync(new MouseEventArgs());
        Assert.Equal("Edit note · Reading, 20 min daily", cut.Find("[role=dialog] h2").TextContent);
        Assert.Equal("Away for the weekend.", cut.Find("textarea").GetAttribute("value") ?? cut.Find("textarea").TextContent);
        cut.Find("textarea").Input("   ");
        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        Assert.Equal(("Add note", "Add note for Sat 14 Nov"), (NoteButton().TextContent, NoteButton().GetAttribute("aria-label")));
        Assert.Equal("—", Text(Row().QuerySelectorAll("td")[3]));
        Assert.Equal(["Consistency and streaks", "History", "Settings"], cut.FindAll(".ht-card__title").Select(Text));
        Assert.Empty(_records.NotesOfGoal(goal.Id));
    }

    // ---- specs/006-note-editing US2-AS4, US2-AS5, SC-004: the note of a History row ----

    private static readonly DateOnly Sat14 = new(2026, 11, 14);

    /// <summary>
    /// Reading, 20 min daily, from Tuesday 10 November in UTC, owned by the signed-in user; at 10:00 on Tuesday 17
    /// November Saturday 14's grace period ended at 00:00 that day, so Saturday 14 is final (Missed without a record).
    /// With <paramref name="minutes"/>, Saturday 14 has that record; with <paramref name="note"/>, that note.
    /// </summary>
    private GoalId ReadingWithSat14(int? minutes, string? note)
    {
        var owner = new AccountId(_authorization.SignIn("RegisteredUser"));
        var reading = _catalog.Add(HabitSamples.Reading());
        var start = new DateOnly(2026, 11, 10);
        Assert.True(GoalTarget.TryCreate(Reading20MinDaily, out var target, out _));
        var goal = _goals.Add(owner,
            new Goal(new GoalId(Guid.NewGuid()), owner, new GoalHabit(reading.Id, HabitKind.Catalog), start, null, new GoalAttempt(1, start),
                [new GoalSettingsVersion(new GoalSettings(target, NamedTimeZone.Utc), start)]),
            new GoalHabitView(reading.Id, HabitKind.Catalog, reading.Definition.Name.Value, reading.Definition.Category, true));
        if (minutes is { } amount)
        {
            _records.Seed(goal.Id, Sat14, RecordValue.FromAmount(amount));
        }

        if (note is not null)
        {
            Assert.True(NoteText.TryCreate(note, out var text, out _));
            _records.SeedNote(goal.Id, Sat14, text!);
        }

        return goal.Id;
    }

    private static AngleSharp.Dom.IElement Sat14Row(IRenderedComponent<GoalDetail> cut) =>
        cut.FindAll("table[aria-label=History] tbody tr").Single(r => Text(r.QuerySelector("td")!) == "Sat 14 Nov");

    private static List<string> Sat14Actions(IRenderedComponent<GoalDetail> cut) =>
        Sat14Row(cut).QuerySelectorAll("td.ht-actions > button").Select(Text).ToList();

    private static string NoteField(IRenderedComponent<GoalDetail> cut) =>
        cut.Find("[role=dialog] textarea").GetAttribute("value") ?? string.Empty;

    [Fact]
    public async Task US2_AS5_Add_record_on_a_Missed_row_with_a_note_opens_the_RecordDialog_with_the_note_in_its_field()
    {
        var cut = RenderGoal(ReadingWithSat14(null, "Away for the weekend."));
        Assert.Equal(["Edit note", "Add record"], Sat14Actions(cut));

        await Sat14Row(cut).QuerySelectorAll("td.ht-actions > button")[1].ClickAsync(new MouseEventArgs());

        var dialog = cut.Find("[role=dialog]");
        Assert.Equal("Record · Reading, 20 min daily", dialog.QuerySelector("h2")!.TextContent);
        Assert.Equal("Sat 14 Nov", Text(dialog.QuerySelector(".ht-field > span:not(.ht-field__label)")!));
        Assert.Equal("Away for the weekend.", NoteField(cut));
    }

    [Fact]
    public async Task US2_AS4_deleting_the_only_record_of_a_row_with_a_note_shows_its_note_and_Edit_note_again_and_focuses_Add_record()
    {
        var goal = ReadingWithSat14(20, "Away for the weekend.");
        var cut = RenderGoal(goal);
        Assert.Equal(["Sat 14 Nov", "20 min", "Met", "Away for the weekend."], Sat14Row(cut).QuerySelectorAll("td").Take(4).Select(Text));
        Assert.Equal(["Edit"], Sat14Actions(cut));
        var recordControl = Sat14Row(cut).QuerySelector("td.ht-actions > button:last-of-type")!.GetAttribute("blazor:elementReference");

        await Sat14Row(cut).QuerySelector("td.ht-actions > button:last-of-type")!.ClickAsync(new MouseEventArgs());
        Assert.Equal("Away for the weekend.", NoteField(cut));
        cut.Find("[role=dialog] textarea").Input(string.Empty);
        await cut.FindAll(".ht-dialog__foot .ht-btn")[2].ClickAsync(new MouseEventArgs());

        Assert.Empty(cut.FindAll("[role=dialog]"));
        Assert.Equal(["Sat 14 Nov", "—", "Missed", "Away for the weekend."], Sat14Row(cut).QuerySelectorAll("td").Take(4).Select(Text));
        Assert.Equal(["Edit note", "Add record"], Sat14Actions(cut));
        Assert.Equal("Edit note of Sat 14 Nov", Sat14Row(cut).QuerySelectorAll("td.ht-actions > button")[0].GetAttribute("aria-label"));
        Assert.Empty(_records.RecordsOfGoal(goal));
        Assert.Equal("Away for the weekend.", Assert.Single(_records.NotesOfGoal(goal)).Value.Value);
        // Focus: the dialog's first field on open, then the row's "Add record", the same element as its "Edit", on close.
        var focused = JSInterop.VerifyFocusAsyncInvoke(2).Select(i => Assert.IsType<Microsoft.AspNetCore.Components.ElementReference>(i.Arguments[0]).Id).ToList();
        var addRecord = Sat14Row(cut).QuerySelector("td.ht-actions > button:last-of-type")!;
        Assert.Equal(("Add record", recordControl), (addRecord.TextContent, focused[1]));
    }

    [Fact]
    public async Task SC_004_on_a_row_with_a_record_Edit_clearing_the_field_and_Save_record_remove_the_note()
    {
        var goal = ReadingWithSat14(20, "Away for the weekend.");
        var cut = RenderGoal(goal);

        // The three actions: open, edit, save.
        await Sat14Row(cut).QuerySelector("td.ht-actions > button:last-of-type")!.ClickAsync(new MouseEventArgs());
        cut.Find("[role=dialog] textarea").Input(string.Empty);
        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        Assert.Empty(cut.FindAll("[role=dialog]"));
        Assert.Equal(["Sat 14 Nov", "20 min", "Met", "—"], Sat14Row(cut).QuerySelectorAll("td").Take(4).Select(Text));
        Assert.Equal(["Edit"], Sat14Actions(cut));
        Assert.Single(_records.RecordsOfGoal(goal));
        Assert.Empty(_records.NotesOfGoal(goal));
    }

    [Fact]
    public async Task SC_004_on_a_row_without_a_record_Edit_note_changing_the_text_and_Save_note_replace_the_note()
    {
        var goal = ReadingWithSat14(null, "Away for the weekend.");
        var cut = RenderGoal(goal);

        // The three actions: open, edit, save.
        await Sat14Row(cut).QuerySelectorAll("td.ht-actions > button")[0].ClickAsync(new MouseEventArgs());
        cut.Find("[role=dialog] textarea").Input("Visiting family.");
        await cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

        Assert.Empty(cut.FindAll("[role=dialog]"));
        Assert.Equal(["Sat 14 Nov", "—", "Missed", "Visiting family."], Sat14Row(cut).QuerySelectorAll("td").Take(4).Select(Text));
        Assert.Equal(["Edit note", "Add record"], Sat14Actions(cut));
        Assert.Empty(_records.RecordsOfGoal(goal));
        Assert.Equal("Visiting family.", Assert.Single(_records.NotesOfGoal(goal)).Value.Value);
    }

    [Fact]
    public async Task US7_AS8_a_goal_on_an_Unpublished_habit_shows_the_Notice_under_the_head()
    {
        var user = SignInUser();
        var coffee = _catalog.Add(HabitSamples.Catalog("Coffee", category: Category.Health, intent: Intent.Reduce));
        var id = await CreateAsync(user, new GoalHabit(coffee.Id, HabitKind.Catalog), new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Reduce, Measurement = Measurement.Count, Amount = 2, CountUnit = "cups",
            ScheduleKind = ScheduleKind.Times, Times = 1, Period = SchedulePeriod.Day,
        }, Today);
        Assert.True(await _catalog.TrySetStateAsync(coffee.Id, CatalogHabitState.Published, CatalogHabitState.Unpublished, CancellationToken.None));

        var cut = RenderGoal(id);

        Assert.Equal("Coffee, at most 2 cups a day", cut.Find(".ht-page-head h1").TextContent);
        Assert.Equal(["Health", "Reduce", "Period-based"], cut.FindAll(".ht-head-tags .ht-chip").Select(c => c.TextContent));
        Assert.Equal("Attempt 1 · started 17 Nov", cut.Find(".ht-attempt").TextContent);
        var notice = cut.Find(".ht-notice");
        Assert.Contains("ht-notice--neutral", notice.ClassList);
        Assert.Equal(NoticeText, Text(notice));
        Assert.Equal("ht-page-head", notice.PreviousElementSibling!.ClassName?.Split(' ')[0]);
        Assert.Equal(
            [
                ("Habit", "Coffee · catalog"), ("Build or reduce", "Reduce"), ("Measure", "Count"), ("Target", "at most 2 cups a day"),
                ("Schedule", "Every day"), ("Evaluated", "Each day"), ("Started", "17 Nov 2026"),
            ],
            Rows(cut));
    }

    [Fact]
    public async Task A_goal_on_a_Published_habit_has_no_Notice()
    {
        var user = SignInUser();
        var reading = _catalog.Add(HabitSamples.Reading());
        var id = await CreateAsync(user, new GoalHabit(reading.Id, HabitKind.Catalog), YesNoOnMonWedFri, Today);

        var cut = RenderGoal(id);

        Assert.Empty(cut.FindAll(".ht-notice"));
        Assert.DoesNotContain(NoticeText, cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void FR_020_an_unknown_goal_gets_the_Not_found_page()
    {
        SignInUser();

        var cut = RenderGoal(new GoalId(Guid.NewGuid()));

        Assert.Equal(1, _notFound);
        Assert.Empty(cut.FindAll("h1"));
    }

    [Fact]
    public async Task FR_020_another_accounts_goal_gets_the_Not_found_page_and_shows_nothing_of_it()
    {
        var other = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.RegisteredUser);
        var reading = _catalog.Add(HabitSamples.Reading());
        var id = await CreateAsync(other, new GoalHabit(reading.Id, HabitKind.Catalog), YesNoOnMonWedFri, Today);
        SignInUser();

        var cut = RenderGoal(id);

        Assert.Equal(1, _notFound);
        Assert.Empty(cut.FindAll("h1"));
        Assert.DoesNotContain("Reading", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public async Task An_administrator_gets_the_Not_found_page()
    {
        var user = SignInUser();
        var reading = _catalog.Add(HabitSamples.Reading());
        var id = await CreateAsync(user, new GoalHabit(reading.Id, HabitKind.Catalog), YesNoOnMonWedFri, Today);
        _authorization.SignIn("Administrator", "admin@example.com");

        var cut = RenderGoal(id);

        Assert.Equal(1, _notFound);
        Assert.Empty(cut.FindAll("h1"));
        Assert.DoesNotContain("Reading", cut.Markup, StringComparison.Ordinal);
    }
}
