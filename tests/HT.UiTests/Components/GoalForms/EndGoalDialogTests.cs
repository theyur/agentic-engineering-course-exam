using AngleSharp.Dom;
using Bunit;
using HT.Application.Goals;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Ui.Components.Goals;
using HT.UiTests.Components.Pages;
using HT.UnitTests.Fakes;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace HT.UiTests.Components.GoalForms;

/// <summary>
/// Slice 8 (T078; HP-003, HP-009, SC-008, design/components/EndGoalDialog "Slice 8 state"): the EndGoalDialog over the
/// real GoalChangeService and the fakes. The user has no stored time zone, so the goal is in UTC. "Meditation, 10 min
/// daily" (Build, 10 minutes, all seven days) starts on Monday 2 November 2026, created at 08:00 that day; the dialog is
/// opened at 10:00 on Monday 16 November, so an ending has the ending date Monday 16 and the last date Sunday 15.
/// </summary>
public sealed class EndGoalDialogTests : BunitContext
{
    private const string Effect = "Ending stops new commitments. Your history stays and nothing is counted as failed. To try again later you start a new attempt.";
    private const string OutcomeMissing = "Choose how the goal ended.";
    private const string NoteTooLong = "The note can have at most 500 characters.";
    private const string NotSignedIn = "You are not signed in.";

    private static readonly DateOnly Mon2 = new(2026, 11, 2);

    private readonly FakeCatalogHabitStore _catalog;
    private readonly FakeGoalStore _goals;
    private readonly FakeTimeProvider _clock;
    private readonly ActingUser _user = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.RegisteredUser);
    private int _saved;
    private int _closed;

    public EndGoalDialogTests()
    {
        _catalog = this.AddCatalog();
        _goals = (FakeGoalStore)Services.GetRequiredService<IGoalStore>();
        _clock = (FakeTimeProvider)Services.GetRequiredService<TimeProvider>();
        _clock.SetUtcNow(new DateTimeOffset(2026, 11, 2, 8, 0, 0, TimeSpan.Zero));
    }

    private static string Text(IElement element) =>
        string.Join(" ", element.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    /// <summary>Creates the goal, moves the clock to 10:00 on Monday 16 November and renders the open dialog.</summary>
    private async Task<(IRenderedComponent<EndGoalDialog> Cut, GoalId Goal)> OpenAsync(ActingUser? actingUser = null)
    {
        var habit = _catalog.Add(HabitSamples.Catalog("Meditation"));
        await using var scope = Services.CreateAsyncScope();
        var created = await scope.ServiceProvider.GetRequiredService<GoalService>().CreateAsync(
            _user,
            new GoalInput
            {
                Habit = new GoalHabit(habit.Id, HabitKind.Catalog),
                Target = new GoalTargetFields
                {
                    BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = 10, DurationUnit = DurationUnit.Minutes,
                    ScheduleKind = ScheduleKind.FixedDays, Days = Enum.GetValues<DayOfWeek>().ToHashSet(),
                },
                StartDate = Mon2,
            },
            CancellationToken.None);
        Assert.Equal(GoalCreateOutcome.Created, created.Outcome);
        var goal = created.GoalId!.Value;
        _clock.SetUtcNow(new DateTimeOffset(2026, 11, 16, 10, 0, 0, TimeSpan.Zero));

        var cut = Render<EndGoalDialog>(p => p
            .Add(x => x.Open, true)
            .Add(x => x.GoalId, goal)
            .Add(x => x.GoalName, "Meditation, 10 min daily")
            .Add(x => x.SubLine, "Attempt 1 · started 2 Nov")
            .Add(x => x.ActingUser, actingUser ?? _user)
            .Add(x => x.OnClose, () => _closed++)
            .Add(x => x.OnSaved, () => _saved++));
        return (cut, goal);
    }

    private static IReadOnlyList<IElement> Radios(IRenderedComponent<EndGoalDialog> cut) => cut.FindAll("input[type=radio]");

    private static IElement Note(IRenderedComponent<EndGoalDialog> cut) => cut.Find("textarea");

    private static Task EndAsync(IRenderedComponent<EndGoalDialog> cut) =>
        cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

    private bool IsEnded(GoalId goal) => _goals.Goals.Single(g => g.Id == goal).IsEnded;

    [Fact]
    public async Task HP_003_the_dialog_has_the_title_the_attempt_line_the_effect_first_the_outcomes_in_one_column_the_note_and_the_buttons()
    {
        var (cut, _) = await OpenAsync();

        var dialog = cut.Find("[role=dialog]");
        Assert.Equal("End goal · Meditation, 10 min daily", dialog.QuerySelector(".ht-dialog__head h2")!.TextContent);
        Assert.Equal("Attempt 1 · started 2 Nov", dialog.QuerySelector(".ht-dialog__sub")!.TextContent);
        var body = cut.Find(".ht-dialog__body");
        Assert.Equal(("P", Effect), (body.FirstElementChild!.TagName, body.FirstElementChild.TextContent));
        var label = cut.Find(".ht-dialog__body .ht-field > span.ht-field__label");
        Assert.Equal("How did it end?", label.TextContent);
        var group = cut.Find("div.ht-choices");
        Assert.Equal(("radiogroup", label.Id, "grid-template-columns:1fr"), (group.GetAttribute("role"), group.GetAttribute("aria-labelledby"), group.GetAttribute("style")));
        Assert.Null(group.GetAttribute("aria-describedby"));
        Assert.Equal(["Achieved", "Abandoned", "No longer relevant"], cut.FindAll(".ht-choice__title").Select(t => t.TextContent));
        Assert.Equal(["I reached what I wanted", "I stopped trying", "My plans changed"], cut.FindAll(".ht-choice__desc").Select(t => t.TextContent));
        Assert.All(Radios(cut), radio => Assert.False(radio.HasAttribute("checked")));
        var noteField = Note(cut).ParentElement!;
        Assert.Equal("Note (optional)", Text(noteField.QuerySelector(".ht-field__label")!));
        Assert.Equal("(optional)", noteField.QuerySelector(".ht-field__label .ht-muted")!.TextContent);
        Assert.Equal("What happened? Only you can see notes.", Note(cut).GetAttribute("placeholder"));
        Assert.Null(Note(cut).GetAttribute("maxlength"));
        Assert.True(string.IsNullOrEmpty(Note(cut).GetAttribute("value")));
        Assert.Empty(cut.FindAll(".ht-field__error"));
        Assert.Equal(["End goal", "Cancel"], cut.FindAll(".ht-dialog__foot .ht-btn").Select(b => b.TextContent));
        Assert.Contains("ht-btn--primary", cut.FindAll(".ht-dialog__foot .ht-btn")[0].ClassList);
        Assert.Contains("ht-btn--secondary", cut.FindAll(".ht-dialog__foot .ht-btn")[1].ClassList);
    }

    [Fact]
    public async Task HP_009_focus_moves_to_the_first_outcome_on_open()
    {
        var (cut, _) = await OpenAsync();

        var focused = JSInterop.VerifyFocusAsyncInvoke().Arguments[0];
        Assert.Equal(Radios(cut)[0].GetAttribute("blazor:elementReference"), Assert.IsType<ElementReference>(focused).Id);
    }

    [Fact]
    public async Task HP_009_Esc_the_close_button_and_Cancel_ask_the_page_to_close_it_and_end_nothing()
    {
        var (cut, goal) = await OpenAsync();
        await Radios(cut)[1].ChangeAsync(new ChangeEventArgs { Value = true });

        cut.Find("[role=dialog]").KeyDown(new KeyboardEventArgs { Key = "Escape" });
        cut.Find("button.ht-dialog__close").Click();
        cut.FindAll(".ht-dialog__foot .ht-btn")[1].Click();

        Assert.Equal(3, _closed);
        Assert.Equal(0, _saved);
        Assert.False(IsEnded(goal));
    }

    [Fact]
    public async Task HP_009_closing_gives_the_focus_back_through_the_pages_callback()
    {
        var (cut, _) = await OpenAsync();
        var givenBack = 0;

        cut.Render(p => p.Add(x => x.FocusOnClose, () =>
        {
            givenBack++;
            return ValueTask.CompletedTask;
        }).Add(x => x.Open, false));

        Assert.Equal(1, givenBack);
        Assert.Empty(cut.FindAll("[role=dialog]"));
    }

    [Fact]
    public async Task HP_003_End_goal_ends_the_goal_with_the_chosen_outcome_and_the_trimmed_note_and_raises_the_saved_callback()
    {
        var (cut, goal) = await OpenAsync();
        await Radios(cut)[1].ChangeAsync(new ChangeEventArgs { Value = true });
        await Note(cut).InputAsync(new ChangeEventArgs { Value = "  Too busy this month.  " });

        await EndAsync(cut);

        Assert.Equal((1, 0), (_saved, _closed));
        var ending = _goals.Goals.Single(g => g.Id == goal).Attempt.Ending!;
        Assert.Equal((new DateOnly(2026, 11, 16), Outcome.Abandoned, "Too busy this month."), (ending.Date, ending.Outcome, ending.Note!.Value.Value));
    }

    [Fact]
    public async Task HP_003_End_goal_with_no_outcome_shows_Choose_how_the_goal_ended_under_How_did_it_end_and_keeps_the_note()
    {
        var (cut, goal) = await OpenAsync();
        await Note(cut).InputAsync(new ChangeEventArgs { Value = "Too busy this month." });

        await EndAsync(cut);

        var error = Assert.Single(cut.FindAll(".ht-field__error"));
        Assert.Equal(OutcomeMissing, error.TextContent);
        var group = cut.Find("div.ht-choices");
        Assert.Same(group.ParentElement, error.ParentElement);
        Assert.Equal("radiogroup", error.PreviousElementSibling!.GetAttribute("role"));
        Assert.Equal(error.Id, group.GetAttribute("aria-describedby"));
        Assert.Empty(cut.FindAll("[aria-invalid=true]"));
        Assert.Empty(cut.FindAll("[role=alert]"));
        Assert.Equal("Too busy this month.", Note(cut).GetAttribute("value"));
        Assert.Equal(0, _saved);
        Assert.False(IsEnded(goal));
    }

    [Fact]
    public async Task HP_003_a_note_of_501_characters_shows_its_refusal_under_the_note_and_keeps_the_choice()
    {
        var (cut, goal) = await OpenAsync();
        var note = new string('n', 501);
        await Radios(cut)[1].ChangeAsync(new ChangeEventArgs { Value = true });
        await Note(cut).InputAsync(new ChangeEventArgs { Value = note });

        await EndAsync(cut);

        var error = Assert.Single(cut.FindAll(".ht-field__error"));
        Assert.Equal(NoteTooLong, error.TextContent);
        Assert.Same(Note(cut).ParentElement, error.ParentElement);
        Assert.Equal("true", Note(cut).GetAttribute("aria-invalid"));
        Assert.Equal(error.Id, Note(cut).GetAttribute("aria-describedby"));
        Assert.Null(cut.Find("div.ht-choices").GetAttribute("aria-describedby"));
        Assert.Equal(note, Note(cut).GetAttribute("value"));
        Assert.Equal([false, true, false], Radios(cut).Select(r => r.HasAttribute("checked")));
        Assert.False(IsEnded(goal));
    }

    [Fact]
    public async Task HP_003_a_refusal_about_no_field_shows_as_an_alert_above_How_did_it_end()
    {
        var (cut, goal) = await OpenAsync(ActingUser.Anonymous);
        await Radios(cut)[0].ChangeAsync(new ChangeEventArgs { Value = true });

        await EndAsync(cut);

        var alert = cut.Find(".ht-dialog__body p.ht-field__error[role=alert]");
        Assert.Equal(NotSignedIn, alert.TextContent);
        Assert.Equal("How did it end?", alert.NextElementSibling!.QuerySelector(".ht-field__label")!.TextContent);
        Assert.Empty(cut.FindAll("[aria-invalid=true]"));
        Assert.Single(cut.FindAll("[role=dialog]"));
        Assert.False(IsEnded(goal));
    }

    [Fact]
    public async Task HP_003_reopening_starts_again_with_no_choice_no_note_and_no_refusal()
    {
        var (cut, _) = await OpenAsync();
        await Note(cut).InputAsync(new ChangeEventArgs { Value = "Too busy this month." });
        await EndAsync(cut);
        Assert.Single(cut.FindAll(".ht-field__error"));
        await Radios(cut)[2].ChangeAsync(new ChangeEventArgs { Value = true });

        cut.Render(p => p.Add(x => x.Open, false));
        cut.Render(p => p.Add(x => x.Open, true));

        Assert.All(Radios(cut), radio => Assert.False(radio.HasAttribute("checked")));
        Assert.True(string.IsNullOrEmpty(Note(cut).GetAttribute("value")));
        Assert.Empty(cut.FindAll(".ht-field__error"));
    }
}
