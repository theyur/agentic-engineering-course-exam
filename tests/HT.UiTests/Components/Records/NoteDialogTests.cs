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
/// The NoteDialog (HP-003, US4-AS7; design/components/NoteDialog, slice 5 state), opened for a History row over the real
/// RecordingService and the fakes. The user has no stored time zone, so the goals are in UTC and every time below is a
/// UTC literal. The goals start on Monday 16 November 2026, created at 08:00 that day; the row comes from the history
/// read at the instant each test names. At 10:00 on Friday 20 November, Monday 16's grace period ended at 00:00 on
/// Thursday 19, so it is Missed.
/// </summary>
public sealed class NoteDialogTests : BunitContext
{
    private const string Help = "Notes are private. They do not change the result.";
    private const string NoteTooLong = "The note can have at most 500 characters.";

    private static readonly DateOnly Mon16 = new(2026, 11, 16);

    private readonly FakeCatalogHabitStore _catalog;
    private readonly FakeRecordStore _records;
    private readonly FakeTimeProvider _clock;
    private readonly ActingUser _user = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.RegisteredUser);
    private readonly List<HistoryEntry> _saved = [];
    private int _closed;

    public NoteDialogTests()
    {
        _catalog = this.AddCatalog();
        _records = (FakeRecordStore)Services.GetRequiredService<IRecordStore>();
        _clock = (FakeTimeProvider)Services.GetRequiredService<TimeProvider>();
        _clock.SetUtcNow(new DateTimeOffset(2026, 11, 16, 8, 0, 0, TimeSpan.Zero));
    }

    private static DateTimeOffset Utc(int day, int hour) => new(2026, 11, day, hour, 0, 0, TimeSpan.Zero);

    private static string Text(IElement element) =>
        string.Join(" ", element.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static GoalTargetFields Reading20MinDaily => new()
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = 20, DurationUnit = DurationUnit.Minutes,
        ScheduleKind = ScheduleKind.FixedDays, Days = Enum.GetValues<DayOfWeek>().ToHashSet(),
    };

    private static GoalTargetFields YogaThreeTimesAWeek => new()
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo, ScheduleKind = ScheduleKind.Times, Times = 3, Period = SchedulePeriod.Week,
    };

    private async Task<GoalId> GoalAsync(GoalTargetFields target)
    {
        var habit = _catalog.Add(HabitSamples.Catalog("Reading"));
        await using var scope = Services.CreateAsyncScope();
        var created = await scope.ServiceProvider.GetRequiredService<GoalService>().CreateAsync(
            _user, new GoalInput { Habit = new GoalHabit(habit.Id, HabitKind.Catalog), Target = target, StartDate = Mon16 }, CancellationToken.None);
        Assert.Equal(GoalCreateOutcome.Created, created.Outcome);
        return created.GoalId!.Value;
    }

    private void SeedNote(GoalId goal, DateOnly key, string text)
    {
        Assert.True(NoteText.TryCreate(text, out var note, out _));
        _records.SeedNote(goal, key, note!);
    }

    /// <summary>
    /// Creates the goal, lets <paramref name="arrange"/> seed it, moves the clock to <paramref name="at"/> and renders the
    /// open dialog for the row whose first date is <paramref name="row"/>, as the goal page reads it.
    /// </summary>
    private async Task<(IRenderedComponent<NoteDialog> Cut, GoalId Goal)> OpenAsync(
        GoalTargetFields target, DateTimeOffset at, DateOnly row, Action<GoalId>? arrange = null, string name = "Reading, 20 min daily")
    {
        var goal = await GoalAsync(target);
        arrange?.Invoke(goal);
        _clock.SetUtcNow(at);
        await using var scope = Services.CreateAsyncScope();
        var read = await scope.ServiceProvider.GetRequiredService<RecordingService>()
            .GetHistoryAsync(_user, goal, Mon16, new DateOnly(at.Year, at.Month, at.Day), CancellationToken.None);
        Assert.Equal(HistoryOutcome.Found, read.Outcome);
        var history = read.History!;
        var cut = Render<NoteDialog>(p => p
            .Add(x => x.Open, true)
            .Add(x => x.GoalId, goal)
            .Add(x => x.GoalName, name)
            .Add(x => x.History, history)
            .Add(x => x.Row, history.Entries.Single(e => e.FirstDate == row))
            .Add(x => x.ActingUser, _user)
            .Add(x => x.OnClose, () => _closed++)
            .Add(x => x.OnSaved, (HistoryEntry entry) => _saved.Add(entry)));
        return (cut, goal);
    }

    private static Task SaveAsync(IRenderedComponent<NoteDialog> cut) =>
        cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

    private static string? AreaValue(IRenderedComponent<NoteDialog> cut)
    {
        var area = cut.Find("textarea");
        var value = area.GetAttribute("value");
        return string.IsNullOrEmpty(value) ? area.TextContent : value;
    }

    [Fact]
    public async Task US4_AS7_the_dialog_names_the_goal_the_rows_date_and_state_says_notes_are_private_and_has_Save_note_and_Cancel()
    {
        var (cut, _) = await OpenAsync(Reading20MinDaily, Utc(20, 10), Mon16);

        var dialog = cut.Find("[role=dialog]");
        Assert.Equal("Add note · Reading, 20 min daily", dialog.QuerySelector(".ht-dialog__head h2")!.TextContent);
        Assert.Equal("Mon 16 Nov · Missed", dialog.QuerySelector(".ht-dialog__sub")!.TextContent);
        var field = cut.Find(".ht-dialog__body label.ht-field");
        Assert.Equal("Note", Text(field.QuerySelector(".ht-field__label")!));
        Assert.NotNull(field.QuerySelector("textarea.ht-input.ht-textarea"));
        Assert.Equal(Help, field.QuerySelector(".ht-field__help")!.TextContent);
        Assert.Equal(["Save note", "Cancel"], cut.FindAll(".ht-dialog__foot .ht-btn").Select(b => b.TextContent));
        Assert.Contains("ht-btn--primary", cut.Find(".ht-dialog__foot .ht-btn").ClassList);
        Assert.Contains("ht-btn--secondary", cut.FindAll(".ht-dialog__foot .ht-btn")[1].ClassList);
    }

    [Fact]
    public async Task US4_AS7_a_Due_weeks_sub_line_is_its_date_range_and_Due_this_week()
    {
        var (cut, _) = await OpenAsync(YogaThreeTimesAWeek, Utc(18, 10), Mon16, name: "Yoga, 3 times a week");

        Assert.Equal("Add note · Yoga, 3 times a week", cut.Find(".ht-dialog__head h2").TextContent);
        Assert.Equal("16 – 22 Nov · Due this week", cut.Find(".ht-dialog__sub").TextContent);
    }

    [Fact]
    public async Task Opened_from_Add_note_the_field_starts_empty()
    {
        var (cut, _) = await OpenAsync(Reading20MinDaily, Utc(20, 10), Mon16);

        Assert.True(string.IsNullOrEmpty(AreaValue(cut)));
    }

    [Fact]
    public async Task Opened_from_a_note_the_field_starts_with_it_and_saving_without_a_change_keeps_it()
    {
        var (cut, goal) = await OpenAsync(Reading20MinDaily, Utc(20, 10), Mon16, g => SeedNote(g, Mon16, "Away for the weekend."));
        Assert.Equal("Add note · Reading, 20 min daily", cut.Find(".ht-dialog__head h2").TextContent);
        Assert.Equal("Away for the weekend.", AreaValue(cut));

        await SaveAsync(cut);

        Assert.Equal("Away for the weekend.", Assert.Single(_saved).Note);
        Assert.Equal("Away for the weekend.", Assert.Single(_records.NotesOfGoal(goal)).Value.Value);
    }

    [Fact]
    public async Task US4_AS1_saving_a_note_keeps_the_rows_state_and_raises_saved_with_the_entry()
    {
        var (cut, goal) = await OpenAsync(Reading20MinDaily, Utc(20, 10), Mon16);
        cut.Find("textarea").Input("  Away for the weekend.  ");

        await SaveAsync(cut);

        var entry = Assert.Single(_saved);
        Assert.Equal(Mon16, entry.FirstDate);
        Assert.Equal("Away for the weekend.", entry.Note);
        Assert.Equal(EvaluationState.Missed, entry.State);
        Assert.Equal("Away for the weekend.", Assert.Single(_records.NotesOfGoal(goal)).Value.Value);
        Assert.Empty(_records.RecordsOfGoal(goal));
        Assert.Empty(cut.FindAll(".ht-field__error"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task US4_AS2_saving_it_empty_removes_the_note(string text)
    {
        var (cut, goal) = await OpenAsync(Reading20MinDaily, Utc(20, 10), Mon16, g => SeedNote(g, Mon16, "Away for the weekend."));
        cut.Find("textarea").Input(text);

        await SaveAsync(cut);

        Assert.Null(Assert.Single(_saved).Note);
        Assert.Empty(_records.NotesOfGoal(goal));
    }

    [Fact]
    public async Task A_week_rows_note_is_saved_for_the_week()
    {
        var (cut, goal) = await OpenAsync(YogaThreeTimesAWeek, Utc(18, 10), Mon16, name: "Yoga, 3 times a week");
        cut.Find("textarea").Input("Knee was sore.");

        await SaveAsync(cut);

        var entry = Assert.Single(_saved);
        Assert.Equal((Mon16, new DateOnly(2026, 11, 22)), (entry.FirstDate, entry.LastDate));
        Assert.Equal(Mon16, Assert.Single(_records.NotesOfGoal(goal)).Key);
    }

    [Fact]
    public async Task US4_AS4_a_note_of_501_characters_is_refused_under_the_field_and_changes_nothing()
    {
        var (cut, goal) = await OpenAsync(Reading20MinDaily, Utc(20, 10), Mon16, g => SeedNote(g, Mon16, "Away for the weekend."));
        cut.Find("textarea").Input(new string('a', 501));

        await SaveAsync(cut);

        var error = Assert.Single(cut.FindAll(".ht-field__error"));
        Assert.Equal(NoteTooLong, error.TextContent);
        Assert.Equal(error.Id, cut.Find(".ht-dialog__body label.ht-field > .ht-field__error").Id);
        Assert.Equal("true", cut.Find("textarea").GetAttribute("aria-invalid"));
        Assert.Equal(error.Id, cut.Find("textarea").GetAttribute("aria-describedby"));
        Assert.Empty(cut.FindAll(".ht-field__help"));
        Assert.Empty(_saved);
        Assert.Equal("Away for the weekend.", Assert.Single(_records.NotesOfGoal(goal)).Value.Value);
        Assert.Equal(0, _closed);
    }

    [Fact]
    public async Task US4_AS4_a_note_of_500_characters_is_saved()
    {
        var (cut, goal) = await OpenAsync(Reading20MinDaily, Utc(20, 10), Mon16);
        var note = new string('a', 500);
        cut.Find("textarea").Input(note);

        await SaveAsync(cut);

        Assert.Equal(note, Assert.Single(_saved).Note);
        Assert.Equal(note, Assert.Single(_records.NotesOfGoal(goal)).Value.Value);
    }

    [Fact]
    public async Task Focus_moves_to_the_note_on_open()
    {
        var (cut, _) = await OpenAsync(Reading20MinDaily, Utc(20, 10), Mon16);

        var focused = JSInterop.VerifyFocusAsyncInvoke().Arguments[0];
        Assert.Equal(cut.Find("textarea").GetAttribute("blazor:elementReference"), Assert.IsType<ElementReference>(focused).Id);
    }

    [Fact]
    public async Task Esc_the_close_button_and_Cancel_ask_the_page_to_close_it_and_save_nothing()
    {
        var (cut, goal) = await OpenAsync(Reading20MinDaily, Utc(20, 10), Mon16);
        cut.Find("textarea").Input("Away for the weekend.");

        cut.Find("[role=dialog]").KeyDown(new KeyboardEventArgs { Key = "Escape" });
        cut.Find("button.ht-dialog__close").Click();
        cut.FindAll(".ht-dialog__foot .ht-btn")[1].Click();

        Assert.Equal(3, _closed);
        Assert.Empty(_saved);
        Assert.Empty(_records.NotesOfGoal(goal));
    }

    [Fact]
    public async Task Closing_gives_the_focus_back_through_the_pages_callback()
    {
        var (cut, _) = await OpenAsync(Reading20MinDaily, Utc(20, 10), Mon16);
        var givenBack = 0;

        cut.Render(p => p.Add(x => x.FocusOnClose, () =>
        {
            givenBack++;
            return ValueTask.CompletedTask;
        }).Add(x => x.Open, false));

        Assert.Equal(1, givenBack);
        Assert.Empty(cut.FindAll("[role=dialog]"));
    }
}
