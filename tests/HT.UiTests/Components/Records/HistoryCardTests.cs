using AngleSharp.Dom;
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
/// The goal page's History card (HP-001, US5-AS5; specs/006-note-editing HP-001, HP-002, SC-002;
/// design/pages/PageGoalDetail, slice 5 state as amended by specs/006-note-editing), rendered from the
/// history that the real RecordingService reads over the fakes. The user has no stored time zone, so the goals are in
/// UTC and every time below is a UTC literal. Records and notes are seeded straight into the fake record store; each
/// goal is created at 08:00 on its start date, and its history read at the instant the test names.
/// </summary>
public sealed class HistoryCardTests : BunitContext
{
    private const string Sentence = "Corrections are evaluated with the rules of that date.";

    private static readonly DateOnly Mon2Nov = new(2026, 11, 2);
    private static readonly DateOnly Tue3Nov = new(2026, 11, 3);
    private static readonly DateOnly Wed4Nov = new(2026, 11, 4);
    private static readonly DateOnly Fri13Nov = new(2026, 11, 13);
    private static readonly DateOnly Mon16Nov = new(2026, 11, 16);
    private static readonly DateOnly Mon10Aug = new(2026, 8, 10);

    private readonly FakeCatalogHabitStore _catalog;
    private readonly FakeRecordStore _records;
    private readonly FakeTimeProvider _clock;
    private readonly ActingUser _user = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.RegisteredUser);

    public HistoryCardTests()
    {
        _catalog = this.AddCatalog();
        _records = (FakeRecordStore)Services.GetRequiredService<IRecordStore>();
        _clock = (FakeTimeProvider)Services.GetRequiredService<TimeProvider>();
    }

    private static GoalTargetFields Reading20MinDaily => new()
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = 20, DurationUnit = DurationUnit.Minutes,
        ScheduleKind = ScheduleKind.FixedDays, Days = Enum.GetValues<DayOfWeek>().ToHashSet(),
    };

    private static GoalTargetFields YogaThreeTimesAWeek => new()
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo,
        ScheduleKind = ScheduleKind.Times, Times = 3, Period = SchedulePeriod.Week,
    };

    private static string Text(IElement element) =>
        string.Join(" ", element.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    /// <summary>Creates the goal at 08:00 UTC on <paramref name="start"/>, its start date.</summary>
    private async Task<GoalId> GoalAsync(GoalTargetFields target, DateOnly start)
    {
        _clock.SetUtcNow(new DateTimeOffset(start.ToDateTime(new TimeOnly(8, 0)), TimeSpan.Zero));
        var habit = _catalog.Add(HabitSamples.Catalog("Reading"));
        await using var scope = Services.CreateAsyncScope();
        var created = await scope.ServiceProvider.GetRequiredService<GoalService>().CreateAsync(
            _user, new GoalInput { Habit = new GoalHabit(habit.Id, HabitKind.Catalog), Target = target, StartDate = start }, CancellationToken.None);
        Assert.Equal(GoalCreateOutcome.Created, created.Outcome);
        return created.GoalId!.Value;
    }

    /// <summary>The history from <paramref name="first"/> to <paramref name="last"/> at <paramref name="at"/>.</summary>
    private async Task<GoalHistory> HistoryAsync(GoalId goal, DateOnly first, DateOnly last, DateTimeOffset at)
    {
        _clock.SetUtcNow(at);
        await using var scope = Services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<RecordingService>().GetHistoryAsync(_user, goal, first, last, CancellationToken.None);
        Assert.Equal(HistoryOutcome.Found, result.Outcome);
        return result.History!;
    }

    /// <summary>The history of the 366 days up to <paramref name="current"/>, as the goal page reads it, at <paramref name="at"/>.</summary>
    private Task<GoalHistory> PageHistoryAsync(GoalId goal, DateOnly current, DateTimeOffset at) =>
        HistoryAsync(goal, current.AddDays(-365), current, at);

    private IRenderedComponent<HistoryCard> RenderCard(
        GoalHistory history, Action<HistoryCard.RowAction>? onAction = null, Action<HistoryEntry>? onNote = null) =>
        Render<HistoryCard>(p => p
            .Add(x => x.History, history)
            .Add(x => x.OnAction, a => onAction?.Invoke(a))
            .Add(x => x.OnNote, e => onNote?.Invoke(e)));

    private static List<string> Dates(IRenderedComponent<HistoryCard> cut) =>
        cut.FindAll("tbody tr").Select(row => Text(row.QuerySelector("td")!)).ToList();

    private static string Cell(IRenderedComponent<HistoryCard> cut, string date, int column) =>
        Text(cut.FindAll("tbody tr").Single(row => Text(row.QuerySelector("td")!) == date).QuerySelectorAll("td")[column]);

    /// <summary>
    /// Reading, 20 min daily, from Monday 2 November 2026, read at 10:00Z on Friday 13 November: 12 begun days, 20 min on
    /// the 2nd (Met), 12 min on the 3rd (Failed) with the note "Fell asleep early.", nothing since; with
    /// <paramref name="wed4Note"/>, Wednesday 4 (Missed, no record) has that note.
    /// </summary>
    private async Task<IRenderedComponent<HistoryCard>> TwelveDaysAsync(
        Action<HistoryCard.RowAction>? onAction = null, Action<HistoryEntry>? onNote = null, string? wed4Note = null)
    {
        var goal = await GoalAsync(Reading20MinDaily, Mon2Nov);
        _records.Seed(goal, Mon2Nov, RecordValue.FromAmount(20));
        _records.Seed(goal, Tue3Nov, RecordValue.FromAmount(12));
        Assert.True(NoteText.TryCreate("Fell asleep early.", out var note, out _));
        _records.SeedNote(goal, Tue3Nov, note!);
        if (wed4Note is not null)
        {
            Assert.True(NoteText.TryCreate(wed4Note, out var missedNote, out _));
            _records.SeedNote(goal, Wed4Nov, missedNote!);
        }

        return RenderCard(await PageHistoryAsync(goal, Fri13Nov, new DateTimeOffset(2026, 11, 13, 10, 0, 0, TimeSpan.Zero)), onAction, onNote);
    }

    [Fact]
    public async Task The_card_head_has_the_title_History_and_the_sentence_beside_it()
    {
        var cut = await TwelveDaysAsync();

        var head = cut.Find("section.ht-card > div.ht-card__head");
        Assert.Equal("History", Text(head.QuerySelector("h2.ht-card__title")!));
        Assert.NotNull(head.QuerySelector("span.ht-spacer"));
        Assert.Equal(Sentence, Text(head.QuerySelector("p.ht-card__sub")!));
    }

    [Fact]
    public async Task The_table_has_the_five_columns_the_last_one_empty_headed()
    {
        var cut = await TwelveDaysAsync();

        Assert.Equal("History", cut.Find("table.ht-table").GetAttribute("aria-label"));
        Assert.Equal(["Date", "Recorded", "Result", "Note", ""], cut.FindAll("thead th").Select(Text));
    }

    [Fact]
    public async Task The_rows_are_newest_first_with_the_first_10_then_Show_all_N()
    {
        var cut = await TwelveDaysAsync();

        Assert.Equal(
            ["Fri 13 Nov", "Thu 12 Nov", "Wed 11 Nov", "Tue 10 Nov", "Mon 9 Nov", "Sun 8 Nov", "Sat 7 Nov", "Fri 6 Nov", "Thu 5 Nov", "Wed 4 Nov"],
            Dates(cut));
        var showAll = cut.Find("section.ht-card > button.ht-btn.ht-btn--ghost.ht-btn--sm");
        Assert.Equal("Show all 12 →", Text(showAll));
        Assert.Equal("false", showAll.GetAttribute("aria-expanded"));
    }

    [Fact]
    public async Task Show_all_shows_the_rest_in_place()
    {
        var cut = await TwelveDaysAsync();

        cut.Find("section.ht-card > button").Click();

        Assert.Equal(12, cut.FindAll("tbody tr").Count);
        Assert.Equal(["Tue 3 Nov", "Mon 2 Nov"], Dates(cut)[10..]);
        Assert.Empty(cut.FindAll("section.ht-card > button"));
    }

    [Fact]
    public async Task Each_row_has_its_record_its_chip_its_note_and_its_action_in_the_actions_cell()
    {
        var cut = await TwelveDaysAsync();
        cut.Find("section.ht-card > button").Click();

        Assert.Equal("12 min", Cell(cut, "Tue 3 Nov", 1));
        Assert.Equal("Failed", Cell(cut, "Tue 3 Nov", 2));
        Assert.Equal("Fell asleep early.", Cell(cut, "Tue 3 Nov", 3));
        Assert.Equal("20 min", Cell(cut, "Mon 2 Nov", 1));
        Assert.Equal("Met", Cell(cut, "Mon 2 Nov", 2));
        // specs/006-note-editing HP-001: the Note cell is the note as text, or "—", not "Add note".
        Assert.Equal("—", Cell(cut, "Mon 2 Nov", 3));
        Assert.Equal("Due today", Text(cut.Find("tbody tr td span.ht-chip.ht-status.ht-status--due")));
        Assert.All(cut.FindAll("tbody tr"), row =>
        {
            var actions = row.QuerySelectorAll("td")[4];
            Assert.True(actions.ClassList.Contains("ht-actions"));
            // specs/006-note-editing HP-002: a row without a record has its note control, then "Add record", so the
            // record control is the rightmost (the owner's order, 2026-11-20); every other row at most one control.
            if (Text(row.QuerySelectorAll("td")[1]) == "—")
            {
                Assert.Equal(["Add note", "Add record"], actions.Children.Select(Text));
            }
            else
            {
                Assert.Single(actions.Children);
            }
        });
        // A commitment's record is text, not a button (design/pages/PageGoalDetail.md).
        Assert.All(cut.FindAll("tbody tr"), row => Assert.Empty(row.QuerySelectorAll("td")[1].QuerySelectorAll("button")));
    }

    [Fact]
    public async Task A_row_without_a_record_reads_a_dash()
    {
        var cut = await TwelveDaysAsync();

        Assert.Equal("—", Cell(cut, "Fri 13 Nov", 1));
        Assert.Equal("—", Cell(cut, "Wed 4 Nov", 1));
        Assert.Equal("Missed", Cell(cut, "Wed 4 Nov", 2));
    }

    [Fact]
    public async Task Ten_rows_have_no_Show_all()
    {
        var goal = await GoalAsync(Reading20MinDaily, Mon2Nov);

        var cut = RenderCard(await PageHistoryAsync(goal, new DateOnly(2026, 11, 11), new DateTimeOffset(2026, 11, 11, 10, 0, 0, TimeSpan.Zero)));

        Assert.Equal(10, cut.FindAll("tbody tr").Count);
        Assert.Empty(cut.FindAll("section.ht-card > button"));
        Assert.DoesNotContain("Show all", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_week_reads_10_to_16_Aug_and_its_records_sit_side_by_side()
    {
        var goal = await GoalAsync(YogaThreeTimesAWeek, Mon10Aug);
        _records.Seed(goal, new DateOnly(2026, 8, 11), RecordValue.FromAnswer(YesNoAnswer.Yes));
        _records.Seed(goal, new DateOnly(2026, 8, 12), RecordValue.FromAnswer(YesNoAnswer.No));

        var cut = RenderCard(await PageHistoryAsync(goal, new DateOnly(2026, 8, 18), new DateTimeOffset(2026, 8, 18, 10, 0, 0, TimeSpan.Zero)));

        Assert.Equal(["17 – 23 Aug", "10 – 16 Aug"], Dates(cut));
        Assert.Equal("Due this week", Cell(cut, "17 – 23 Aug", 2));
        var week = cut.FindAll("tbody tr")[1];
        Assert.Equal(["Yes", "No"], week.QuerySelectorAll("td")[1].QuerySelectorAll("span.ht-inline > button").Select(Text));
        Assert.Equal("Pending", Cell(cut, "10 – 16 Aug", 2));
    }

    [Fact]
    public async Task Only_begun_entries_have_a_row()
    {
        var goal = await GoalAsync(Reading20MinDaily, Mon16Nov);

        var cut = RenderCard(await HistoryAsync(goal, Mon16Nov, new DateOnly(2026, 11, 20), new DateTimeOffset(2026, 11, 17, 10, 0, 0, TimeSpan.Zero)));

        Assert.Equal(["Tue 17 Nov", "Mon 16 Nov"], Dates(cut));
    }

    [Fact]
    public async Task A_history_of_Upcoming_entries_only_renders_no_card()
    {
        var goal = await GoalAsync(Reading20MinDaily, Mon16Nov);

        var history = await HistoryAsync(goal, new DateOnly(2026, 11, 18), new DateOnly(2026, 11, 20), new DateTimeOffset(2026, 11, 17, 10, 0, 0, TimeSpan.Zero));
        var cut = RenderCard(history);

        Assert.Equal(3, history.Entries.Count);
        Assert.False(HistoryCard.HasRows(history));
        Assert.Equal(string.Empty, cut.Markup.Trim());
    }

    // ---- US3-AS8: the row actions (HP-001, contracts/host.md "Accessible names") ----

    private static IElement Row(IRenderedComponent<HistoryCard> cut, string date) =>
        cut.FindAll("tbody tr").Single(row => Text(row.QuerySelector("td")!) == date);

    private const string AddNote = "Add note";
    private const string EditNote = "Edit note";

    private static bool IsNoteControl(IElement button) => Text(button) is AddNote or EditNote;

    /// <summary>
    /// The row's record control ("Add record" or "Edit"), the last button of its actions cell, so the rightmost (the
    /// owner's order, 2026-11-20). Any button before it is the row's note control (specs/006-note-editing HP-002), so the
    /// row has exactly one record control.
    /// </summary>
    private static IElement Action(IRenderedComponent<HistoryCard> cut, string date)
    {
        var buttons = Row(cut, date).QuerySelectorAll("td.ht-actions > button");
        Assert.InRange(buttons.Length, 1, 2);
        Assert.False(IsNoteControl(buttons[^1]));
        if (buttons.Length == 2)
        {
            Assert.True(IsNoteControl(buttons[0]));
        }

        return buttons[^1];
    }

    /// <summary>
    /// The row's note control ("Add note" or "Edit note"), or null when it has none. It is the first of the two buttons of
    /// the actions cell of a row without a record, before "Add record" (specs/006-note-editing HP-002); the helper asserts
    /// that.
    /// </summary>
    private static IElement? NoteControl(IRenderedComponent<HistoryCard> cut, string date)
    {
        var row = Row(cut, date);
        var buttons = row.QuerySelectorAll("button");
        var notes = buttons.Where(IsNoteControl).ToList();
        if (notes.Count == 0)
        {
            return null;
        }

        var note = Assert.Single(notes);
        var actions = row.QuerySelectorAll("td.ht-actions > button");
        Assert.Equal(2, actions.Length);
        Assert.Same(actions[0], note);
        Assert.Equal("—", Text(row.QuerySelectorAll("td")[1]));
        return note;
    }

    private static List<IElement> RecordButtons(IRenderedComponent<HistoryCard> cut, string date) =>
        Row(cut, date).QuerySelectorAll("td")[1].QuerySelectorAll("span.ht-inline > button").ToList();

    private static void AssertRowControl(IElement button, string text, string name)
    {
        Assert.Equal(text, Text(button));
        Assert.Equal(name, button.GetAttribute("aria-label"));
        Assert.StartsWith(text, name, StringComparison.Ordinal);
        Assert.Equal("button", button.GetAttribute("type"));
        Assert.Equal(["ht-btn", "ht-btn--ghost", "ht-btn--sm"], button.ClassList.ToArray());
        Assert.Equal("dialog", button.GetAttribute("aria-haspopup"));
    }

    /// <summary>
    /// Yoga, 3 times a week, from Monday 3 August 2026, read at 10:00Z on Tuesday 18 August: the week of 17 August is
    /// Due without a record, the week of 10 August Pending (its grace period ends at 00:00Z on Wednesday 19) with Yes on
    /// the 11th and No on the 12th, and the week of 3 August Failed with Yes on the 4th.
    /// </summary>
    private async Task<(IRenderedComponent<HistoryCard> Cut, RecordId Yes11, RecordId No12, RecordId Yes4)> ThreeWeeksAsync(
        Action<HistoryCard.RowAction>? onAction = null)
    {
        var goal = await GoalAsync(YogaThreeTimesAWeek, new DateOnly(2026, 8, 3));
        var yes4 = _records.Seed(goal, new DateOnly(2026, 8, 4), RecordValue.FromAnswer(YesNoAnswer.Yes));
        var yes11 = _records.Seed(goal, new DateOnly(2026, 8, 11), RecordValue.FromAnswer(YesNoAnswer.Yes));
        var no12 = _records.Seed(goal, new DateOnly(2026, 8, 12), RecordValue.FromAnswer(YesNoAnswer.No));
        var history = await PageHistoryAsync(goal, new DateOnly(2026, 8, 18), new DateTimeOffset(2026, 8, 18, 10, 0, 0, TimeSpan.Zero));
        var cut = RenderCard(history, onAction);
        return (cut, yes11, no12, yes4);
    }

    [Fact]
    public async Task US3_AS8_a_commitment_row_offers_Add_record_without_a_record_and_Edit_with_one_whatever_its_state()
    {
        var cut = await TwelveDaysAsync();
        cut.Find("section.ht-card > button").Click();

        AssertRowControl(Action(cut, "Fri 13 Nov"), "Add record", "Add record for Fri 13 Nov");
        AssertRowControl(Action(cut, "Thu 12 Nov"), "Add record", "Add record for Thu 12 Nov");
        Assert.Equal("Missed", Cell(cut, "Tue 10 Nov", 2));
        AssertRowControl(Action(cut, "Tue 10 Nov"), "Add record", "Add record for Tue 10 Nov");
        AssertRowControl(Action(cut, "Wed 4 Nov"), "Add record", "Add record for Wed 4 Nov");
        AssertRowControl(Action(cut, "Tue 3 Nov"), "Edit", "Edit record of Tue 3 Nov");
        AssertRowControl(Action(cut, "Mon 2 Nov"), "Edit", "Edit record of Mon 2 Nov");
        // One record control on each of the 12 rows, plus "Add note" on the 10 rows without a record (Fri 13, Thu 12,
        // Wed 11, Tue 10, Mon 9, Sun 8, Sat 7, Fri 6, Thu 5 and Wed 4 Nov; specs/006-note-editing HP-002).
        Assert.Equal(22, cut.FindAll("td.ht-actions > button").Count);
    }

    [Fact]
    public async Task US3_AS8_a_period_row_offers_Add_record_and_each_record_as_a_button_that_opens_Edit_final_rows_included()
    {
        var (cut, _, _, _) = await ThreeWeeksAsync();

        Assert.Equal(["17 – 23 Aug", "10 – 16 Aug", "3 – 9 Aug"], Dates(cut));
        Assert.Equal(["Due this week", "Pending", "Failed"], cut.FindAll("tbody tr").Select(row => Text(row.QuerySelectorAll("td")[2])));
        AssertRowControl(Action(cut, "17 – 23 Aug"), "Add record", "Add record for 17 – 23 Aug");
        AssertRowControl(Action(cut, "10 – 16 Aug"), "Add record", "Add record for 10 – 16 Aug");
        AssertRowControl(Action(cut, "3 – 9 Aug"), "Add record", "Add record for 3 – 9 Aug");
        Assert.Empty(RecordButtons(cut, "17 – 23 Aug"));
        Assert.Equal("—", Cell(cut, "17 – 23 Aug", 1));
        var week = RecordButtons(cut, "10 – 16 Aug");
        Assert.Equal(2, week.Count);
        AssertRowControl(week[0], "Yes", "Yes, edit record 1 of 10 – 16 Aug");
        AssertRowControl(week[1], "No", "No, edit record 2 of 10 – 16 Aug");
        AssertRowControl(Assert.Single(RecordButtons(cut, "3 – 9 Aug")), "Yes", "Yes, edit record 1 of 3 – 9 Aug");
        Assert.DoesNotContain(cut.FindAll("td.ht-actions > button"), b => Text(b) == "Edit");
    }

    [Theory]
    [InlineData(49, true)]
    [InlineData(50, false)]
    public async Task US3_AS8_a_period_row_offers_Add_record_only_while_it_has_fewer_than_50_records(int records, bool offered)
    {
        var goal = await GoalAsync(YogaThreeTimesAWeek, Mon10Aug);
        for (var i = 0; i < records; i++)
        {
            _records.Seed(goal, new DateOnly(2026, 8, 11), RecordValue.FromAnswer(YesNoAnswer.Yes));
        }

        var cut = RenderCard(await PageHistoryAsync(goal, new DateOnly(2026, 8, 18), new DateTimeOffset(2026, 8, 18, 10, 0, 0, TimeSpan.Zero)));

        Assert.Equal(offered, Row(cut, "10 – 16 Aug").QuerySelectorAll("td.ht-actions > button").Length == 1);
        var buttons = RecordButtons(cut, "10 – 16 Aug");
        Assert.Equal(records, buttons.Count);
        Assert.Equal($"Yes, edit record {records} of 10 – 16 Aug", buttons[^1].GetAttribute("aria-label"));
        Assert.Null(NoteControl(cut, "10 – 16 Aug"));
        // "17 – 23 Aug" has no record: its record control, then its note control (specs/006-note-editing HP-002).
        AssertRowControl(Action(cut, "17 – 23 Aug"), "Add record", "Add record for 17 – 23 Aug");
        AssertRowControl(NoteControl(cut, "17 – 23 Aug")!, AddNote, "Add note for 17 – 23 Aug");
    }

    [Fact]
    public async Task The_row_controls_raise_the_row_and_for_Edit_the_record()
    {
        var actions = new List<HistoryCard.RowAction>();
        var (cut, yes11, no12, _) = await ThreeWeeksAsync(actions.Add);
        var commitments = await TwelveDaysAsync(actions.Add);
        commitments.Find("section.ht-card > button").Click();

        await Action(cut, "10 – 16 Aug").ClickAsync(new MouseEventArgs());
        await RecordButtons(cut, "10 – 16 Aug")[1].ClickAsync(new MouseEventArgs());
        await Action(commitments, "Wed 4 Nov").ClickAsync(new MouseEventArgs());
        await Action(commitments, "Tue 3 Nov").ClickAsync(new MouseEventArgs());

        Assert.Equal(4, actions.Count);
        Assert.Equal((new DateOnly(2026, 8, 10), (RecordId?)null), (actions[0].Entry.FirstDate, actions[0].Record?.Id));
        Assert.Equal((new DateOnly(2026, 8, 10), (RecordId?)no12), (actions[1].Entry.FirstDate, actions[1].Record?.Id));
        Assert.Equal([yes11, no12], actions[1].Entry.Records.Select(r => r.Id));
        Assert.Equal((new DateOnly(2026, 11, 4), (RecordView?)null), (actions[2].Entry.FirstDate, actions[2].Record));
        Assert.Equal((Tue3Nov, 12), (actions[3].Entry.FirstDate, actions[3].Record!.Amount!.Value));
    }

    [Fact]
    public async Task FocusAsync_gives_the_focus_to_the_record_button_while_the_row_shows_it_and_otherwise_to_the_rows_action()
    {
        var (cut, yes11, _, _) = await ThreeWeeksAsync();
        var recordButton = RecordButtons(cut, "10 – 16 Aug")[0].GetAttribute("blazor:elementReference");
        var actionButton = Action(cut, "10 – 16 Aug").GetAttribute("blazor:elementReference");

        await cut.InvokeAsync(() => cut.Instance.FocusAsync(Mon10Aug, yes11).AsTask());
        await cut.InvokeAsync(() => cut.Instance.FocusAsync(Mon10Aug, new RecordId(Guid.NewGuid())).AsTask());
        await cut.InvokeAsync(() => cut.Instance.FocusAsync(Mon10Aug, null).AsTask());
        await cut.InvokeAsync(() => cut.Instance.FocusAsync(new DateOnly(2026, 7, 27), null).AsTask());

        var focused = JSInterop.VerifyFocusAsyncInvoke(3).Select(i => Assert.IsType<ElementReference>(i.Arguments[0]).Id);
        Assert.Equal([recordButton, actionButton, actionButton], focused);
    }

    // ---- US4-AS7 as amended by specs/006-note-editing: the Note cell is text, and only a row without a record has a
    // note control (HP-001, HP-002, contracts/host.md "History card") ----

    private const string Away = "Away for the weekend.";

    [Fact]
    public async Task HP_002_only_a_row_without_a_record_offers_Add_note_or_Edit_note_and_the_Note_cell_is_text()
    {
        var cut = await TwelveDaysAsync(wed4Note: Away);
        cut.Find("section.ht-card > button").Click();

        Assert.Equal("Fell asleep early.", Cell(cut, "Tue 3 Nov", 3));
        Assert.Null(NoteControl(cut, "Tue 3 Nov"));
        Assert.Equal("—", Cell(cut, "Mon 2 Nov", 3));
        Assert.Null(NoteControl(cut, "Mon 2 Nov"));
        Assert.Equal("—", Cell(cut, "Fri 13 Nov", 3));
        AssertRowControl(NoteControl(cut, "Fri 13 Nov")!, AddNote, "Add note for Fri 13 Nov");
        Assert.Equal("Missed", Cell(cut, "Wed 4 Nov", 2));
        Assert.Equal(Away, Cell(cut, "Wed 4 Nov", 3));
        AssertRowControl(NoteControl(cut, "Wed 4 Nov")!, EditNote, "Edit note of Wed 4 Nov");
        Assert.All(cut.FindAll("tbody tr"), row =>
        {
            var note = row.QuerySelectorAll("td")[3];
            Assert.Equal(["ht-muted"], note.ClassList.ToArray());
            Assert.Empty(note.Children);
        });
    }

    [Fact]
    public async Task US4_AS7_a_weeks_Note_cell_names_its_date_range()
    {
        var goal = await GoalAsync(YogaThreeTimesAWeek, Mon10Aug);
        Assert.True(NoteText.TryCreate("Knee was sore.", out var note, out _));
        _records.SeedNote(goal, Mon10Aug, note!);

        var cut = RenderCard(await PageHistoryAsync(goal, new DateOnly(2026, 8, 18), new DateTimeOffset(2026, 8, 18, 10, 0, 0, TimeSpan.Zero)));

        Assert.Equal("Knee was sore.", Cell(cut, "10 – 16 Aug", 3));
        Assert.Empty(Row(cut, "10 – 16 Aug").QuerySelectorAll("td")[3].Children);
        AssertRowControl(NoteControl(cut, "10 – 16 Aug")!, EditNote, "Edit note of 10 – 16 Aug");
        AssertRowControl(NoteControl(cut, "17 – 23 Aug")!, AddNote, "Add note for 17 – 23 Aug");
    }

    [Fact]
    public async Task The_note_controls_raise_their_row()
    {
        var rows = new List<HistoryEntry>();
        var actions = new List<HistoryCard.RowAction>();
        var cut = await TwelveDaysAsync(actions.Add, rows.Add, Away);
        cut.Find("section.ht-card > button").Click();

        await NoteControl(cut, "Fri 13 Nov")!.ClickAsync(new MouseEventArgs());
        await NoteControl(cut, "Wed 4 Nov")!.ClickAsync(new MouseEventArgs());

        Assert.Equal([(Fri13Nov, null), (Wed4Nov, Away)], rows.Select(r => (r.FirstDate, r.Note)));
        Assert.Empty(actions);
    }

    [Fact]
    public async Task FocusNoteAsync_gives_the_focus_to_the_rows_note_control_and_nothing_for_a_row_without_one()
    {
        // TwelveDaysAsync's goal without its note: Tue 3 Nov has a record and is the 11th row, not shown at first.
        var goal = await GoalAsync(Reading20MinDaily, Mon2Nov);
        _records.Seed(goal, Tue3Nov, RecordValue.FromAmount(12));
        var at = new DateTimeOffset(2026, 11, 13, 10, 0, 0, TimeSpan.Zero);
        var cut = RenderCard(await PageHistoryAsync(goal, Fri13Nov, at));
        var noteControl = NoteControl(cut, "Fri 13 Nov")!.GetAttribute("blazor:elementReference");
        Task FocusNoteAsync(DateOnly row) => cut.InvokeAsync(() => cut.Instance.FocusNoteAsync(row).AsTask());

        await FocusNoteAsync(Fri13Nov);
        await FocusNoteAsync(Tue3Nov);
        cut.Find("section.ht-card > button").Click();
        await FocusNoteAsync(Tue3Nov);
        // Fri 13 gets its first record: on the next read its note control is gone, and nothing gets the focus.
        _records.Seed(goal, Fri13Nov, RecordValue.FromAmount(20));
        var history = await PageHistoryAsync(goal, Fri13Nov, at);
        cut.Render(p => p.Add(x => x.History, history));
        Assert.Null(NoteControl(cut, "Fri 13 Nov"));
        await FocusNoteAsync(Fri13Nov);

        var focused = JSInterop.VerifyFocusAsyncInvoke().Arguments[0];
        Assert.Equal(noteControl, Assert.IsType<ElementReference>(focused).Id);
    }

    // ---- SC-002 (specs/006-note-editing, US1-AS7, US2-AS2): at most one control changes a row's note on its own ----

    /// <summary>
    /// Every begun row of one goal, each with <paramref name="records"/> records and, with <paramref name="withNote"/>,
    /// the note "Away for the weekend.". A commitment: Reading, 20 min daily, from Monday 2 November 2026, read at 10:00Z
    /// on Friday 13 November and shown in full: Fri 13 is Due; Thu 12 and Wed 11 are Pending (Wed 11's grace period ends
    /// at 00:00Z on Saturday 14); Tue 10 to Mon 2 are final, as Tue 10's grace period ended at 00:00Z on Friday 13. Its
    /// record is 12 min (Failed when final), and 20 min on Mon 2 (Met). A period: Yoga, 3 times a week, from Monday 20
    /// July 2026, read at 10:00Z on Tuesday 18 August: 17 – 23 Aug is Due; 10 – 16 Aug is Pending (its grace period ends
    /// at 00:00Z on Wednesday 19); the three weeks before are final. Its records are Yes on the week's Tuesday: one is
    /// Failed (1 of 3) when final, 50 are Met.
    /// </summary>
    [Theory]
    [InlineData(false, 0, false, new[] { EvaluationState.Due, EvaluationState.Pending, EvaluationState.Missed })]
    [InlineData(false, 0, true, new[] { EvaluationState.Due, EvaluationState.Pending, EvaluationState.Missed })]
    [InlineData(false, 1, false, new[] { EvaluationState.Due, EvaluationState.Pending, EvaluationState.Met, EvaluationState.Failed })]
    [InlineData(false, 1, true, new[] { EvaluationState.Due, EvaluationState.Pending, EvaluationState.Met, EvaluationState.Failed })]
    [InlineData(true, 0, false, new[] { EvaluationState.Due, EvaluationState.Pending, EvaluationState.Missed })]
    [InlineData(true, 0, true, new[] { EvaluationState.Due, EvaluationState.Pending, EvaluationState.Missed })]
    [InlineData(true, 1, false, new[] { EvaluationState.Due, EvaluationState.Pending, EvaluationState.Failed })]
    [InlineData(true, 1, true, new[] { EvaluationState.Due, EvaluationState.Pending, EvaluationState.Failed })]
    [InlineData(true, 50, false, new[] { EvaluationState.Due, EvaluationState.Pending, EvaluationState.Met })]
    [InlineData(true, 50, true, new[] { EvaluationState.Due, EvaluationState.Pending, EvaluationState.Met })]
    public async Task SC_002_a_row_has_one_note_control_without_a_record_none_with_one_and_its_Note_cell_is_text(
        bool period, int records, bool withNote, EvaluationState[] states)
    {
        var raised = new List<HistoryEntry>();
        var actions = new List<HistoryCard.RowAction>();
        Assert.True(NoteText.TryCreate(Away, out var note, out _));
        GoalHistory history;
        if (period)
        {
            var goal = await GoalAsync(YogaThreeTimesAWeek, new DateOnly(2026, 7, 20));
            for (var monday = new DateOnly(2026, 7, 20); monday <= new DateOnly(2026, 8, 17); monday = monday.AddDays(7))
            {
                for (var i = 0; i < records; i++)
                {
                    _records.Seed(goal, monday.AddDays(1), RecordValue.FromAnswer(YesNoAnswer.Yes));
                }

                if (withNote)
                {
                    _records.SeedNote(goal, monday, note!);
                }
            }

            history = await PageHistoryAsync(goal, new DateOnly(2026, 8, 18), new DateTimeOffset(2026, 8, 18, 10, 0, 0, TimeSpan.Zero));
        }
        else
        {
            var goal = await GoalAsync(Reading20MinDaily, Mon2Nov);
            for (var day = Mon2Nov; day <= Fri13Nov; day = day.AddDays(1))
            {
                for (var i = 0; i < records; i++)
                {
                    _records.Seed(goal, day, RecordValue.FromAmount(day == Mon2Nov ? 20 : 12));
                }

                if (withNote)
                {
                    _records.SeedNote(goal, day, note!);
                }
            }

            history = await PageHistoryAsync(goal, Fri13Nov, new DateTimeOffset(2026, 11, 13, 10, 0, 0, TimeSpan.Zero));
        }

        var cut = RenderCard(history, actions.Add, raised.Add);
        if (!period)
        {
            cut.Find("section.ht-card > button").Click();
        }

        var dates = Dates(cut);
        Assert.Equal(period ? 5 : 12, dates.Count);
        Assert.Equal(
            states.Order(),
            history.Entries.Where(e => e.State != EvaluationState.Upcoming).Select(e => e.State).Distinct().Order());
        var clicks = 0;
        foreach (var date in dates)
        {
            var noteCell = Row(cut, date).QuerySelectorAll("td")[3];
            Assert.Equal(withNote ? Away : "—", Text(noteCell));
            Assert.Equal(["ht-muted"], noteCell.ClassList.ToArray());
            Assert.Empty(noteCell.Children);

            // Every control of the row, clicked in turn: only a note control raises OnNote.
            var before = raised.Count;
            var buttons = Row(cut, date).QuerySelectorAll("button").Length;
            for (var i = 0; i < buttons; i++)
            {
                await Row(cut, date).QuerySelectorAll("button")[i].ClickAsync(new MouseEventArgs());
            }

            clicks += buttons;
            var ofRow = raised.Skip(before).ToList();
            if (records == 0)
            {
                Assert.Equal(date, RecordingText.EntryDate(Assert.Single(ofRow)));
                AssertRowControl(NoteControl(cut, date)!, withNote ? EditNote : AddNote, withNote ? $"Edit note of {date}" : $"Add note for {date}");
            }
            else
            {
                Assert.Empty(ofRow);
                Assert.Null(NoteControl(cut, date));
            }
        }

        Assert.Equal(clicks, actions.Count + raised.Count);
    }

    [Fact]
    public async Task A_goal_without_a_begun_entry_renders_no_card()
    {
        // A per-week goal from Wednesday 19 August 2026 has its first week from Monday 24 August.
        var goal = await GoalAsync(YogaThreeTimesAWeek, new DateOnly(2026, 8, 19));

        var history = await PageHistoryAsync(goal, new DateOnly(2026, 8, 20), new DateTimeOffset(2026, 8, 20, 10, 0, 0, TimeSpan.Zero));
        var cut = RenderCard(history);

        Assert.Empty(history.Entries);
        Assert.False(HistoryCard.HasRows(history));
        Assert.Equal(string.Empty, cut.Markup.Trim());
    }
}
