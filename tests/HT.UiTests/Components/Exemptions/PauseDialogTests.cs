using AngleSharp.Dom;
using Bunit;
using HT.Application.Goals;
using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Ui.Components.Exemptions;
using HT.UiTests.Components.Pages;
using HT.UnitTests.Fakes;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace HT.UiTests.Components.Exemptions;

/// <summary>
/// The PauseDialog (HP-002, HP-007; design/components/PauseDialog, slice 7 state; T060) over the real PauseService and
/// the fakes. The user has no stored time zone, so the goal is in UTC and every time below is a UTC literal. "Drinking
/// water, 8 glasses a day" (Build, count at least 8 "glasses", once per day) starts on Monday 2 November 2026, created at
/// 08:00 that day. The dialog is opened at 09:00 on Monday 16 November, whose pause date is Monday 16, the page's current
/// date it is given.
/// </summary>
public sealed class PauseDialogTests : BunitContext
{
    private const string DaysMissing = "Choose the first and the last day.";
    private const string StartsBeforeToday = "A pause cannot start before today.";
    private const string LastBeforeFirst = "The last day cannot be before the first day.";
    private const string Overlaps = "This goal already has a pause on these dates.";
    private const string ReasonTooLong = "Keep the reason to 200 characters or fewer.";
    private const string NotSignedIn = "You are not signed in.";

    private static readonly DateOnly Mon2 = new(2026, 11, 2);
    private static readonly DateOnly Mon16 = new(2026, 11, 16);
    private static readonly DateOnly Sun22 = new(2026, 11, 22);

    private readonly FakeCatalogHabitStore _catalog;
    private readonly FakeRecordStore _records;
    private readonly FakeTimeProvider _clock;
    private readonly ActingUser _user = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.RegisteredUser);
    private int _saved;
    private int _closed;

    public PauseDialogTests()
    {
        _catalog = this.AddCatalog();
        _records = (FakeRecordStore)Services.GetRequiredService<IRecordStore>();
        _clock = (FakeTimeProvider)Services.GetRequiredService<TimeProvider>();
        _clock.SetUtcNow(new DateTimeOffset(2026, 11, 2, 8, 0, 0, TimeSpan.Zero));
    }

    private static string Text(IElement element) =>
        string.Join(" ", element.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static GoalTargetFields DrinkingWater => new()
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Count, Amount = 8, CountUnit = "glasses",
        ScheduleKind = ScheduleKind.Times, Times = 1, Period = SchedulePeriod.Day,
    };

    /// <summary>Creates the goal, moves the clock to 09:00 on Monday 16 November and renders the open dialog.</summary>
    private async Task<(IRenderedComponent<PauseDialog> Cut, GoalId Goal)> OpenAsync(ActingUser? actingUser = null)
    {
        var habit = _catalog.Add(HabitSamples.Catalog("Drinking water"));
        await using var scope = Services.CreateAsyncScope();
        var created = await scope.ServiceProvider.GetRequiredService<GoalService>().CreateAsync(
            _user, new GoalInput { Habit = new GoalHabit(habit.Id, HabitKind.Catalog), Target = DrinkingWater, StartDate = Mon2 },
            CancellationToken.None);
        Assert.Equal(GoalCreateOutcome.Created, created.Outcome);
        var goal = created.GoalId!.Value;
        _clock.SetUtcNow(new DateTimeOffset(2026, 11, 16, 9, 0, 0, TimeSpan.Zero));

        var cut = Render<PauseDialog>(p => p
            .Add(x => x.Open, true)
            .Add(x => x.GoalId, goal)
            .Add(x => x.GoalName, "Drinking water, 8 glasses a day")
            .Add(x => x.Today, Mon16)
            .Add(x => x.ActingUser, actingUser ?? _user)
            .Add(x => x.OnClose, () => _closed++)
            .Add(x => x.OnSaved, () => _saved++));
        return (cut, goal);
    }

    private static IElement From(IRenderedComponent<PauseDialog> cut) => cut.FindAll("input[type=date]")[0];

    private static IElement Until(IRenderedComponent<PauseDialog> cut) => cut.FindAll("input[type=date]")[1];

    private static IElement Reason(IRenderedComponent<PauseDialog> cut) => cut.Find("input[type=text]");

    private static Task PauseAsync(IRenderedComponent<PauseDialog> cut) =>
        cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

    /// <summary>The one refusal shown, which must be under <paramref name="input"/>'s field and described by it.</summary>
    private static void AssertErrorUnder(IRenderedComponent<PauseDialog> cut, IElement input, string message)
    {
        var error = Assert.Single(cut.FindAll(".ht-field__error"));
        Assert.Equal(message, error.TextContent);
        Assert.Same(input.ParentElement, error.ParentElement);
        Assert.Equal("true", input.GetAttribute("aria-invalid"));
        Assert.Equal(error.Id, input.GetAttribute("aria-describedby"));
        Assert.Single(cut.FindAll("[aria-invalid=true]"));
        Assert.Empty(cut.FindAll("[role=alert]"));
    }

    [Fact]
    public async Task HP_002_the_dialog_has_the_title_the_effect_first_From_Until_Reason_and_the_buttons()
    {
        var (cut, _) = await OpenAsync();

        var dialog = cut.Find("[role=dialog]");
        Assert.Equal("Pause · Drinking water, 8 glasses a day", dialog.QuerySelector(".ht-dialog__head h2")!.TextContent);
        Assert.Null(dialog.QuerySelector(".ht-dialog__sub"));
        var body = cut.Find(".ht-dialog__body");
        Assert.Equal("P", body.FirstElementChild!.TagName);
        Assert.Equal("Paused days are not counted. Your streak stays as it is. Final results do not change.", body.FirstElementChild.TextContent);
        var pair = cut.Find(".ht-dialog__body .ht-field-pair");
        Assert.Equal(["From", "Until"], pair.QuerySelectorAll(".ht-field__label").Select(Text));
        Assert.Equal("2026-11-16", From(cut).GetAttribute("value"));
        Assert.Equal("2026-11-16", From(cut).GetAttribute("min"));
        Assert.True(string.IsNullOrEmpty(Until(cut).GetAttribute("value")));
        Assert.Equal("true", Until(cut).GetAttribute("aria-required"));
        Assert.False(Until(cut).HasAttribute("required"));
        Assert.False(Until(cut).HasAttribute("min"));
        var reasonField = Reason(cut).ParentElement!;
        Assert.Equal("Reason (optional)", Text(reasonField.QuerySelector(".ht-field__label")!));
        Assert.Equal("(optional)", reasonField.QuerySelector(".ht-field__label .ht-muted")!.TextContent);
        Assert.True(string.IsNullOrEmpty(Reason(cut).GetAttribute("value")));
        Assert.Null(Reason(cut).GetAttribute("maxlength"));
        Assert.Null(Reason(cut).GetAttribute("placeholder"));
        Assert.Empty(cut.FindAll(".ht-field__error"));
        Assert.Equal(["Pause goal", "Cancel"], cut.FindAll(".ht-dialog__foot .ht-btn").Select(b => b.TextContent));
        Assert.Contains("ht-btn--primary", cut.FindAll(".ht-dialog__foot .ht-btn")[0].ClassList);
        Assert.Contains("ht-btn--secondary", cut.FindAll(".ht-dialog__foot .ht-btn")[1].ClassList);
    }

    [Fact]
    public async Task HP_007_focus_moves_to_From_on_open()
    {
        var (cut, _) = await OpenAsync();

        var focused = JSInterop.VerifyFocusAsyncInvoke().Arguments[0];
        Assert.Equal(From(cut).GetAttribute("blazor:elementReference"), Assert.IsType<ElementReference>(focused).Id);
    }

    [Fact]
    public async Task HP_007_Esc_the_close_button_and_Cancel_ask_the_page_to_close_it_and_pause_nothing()
    {
        var (cut, goal) = await OpenAsync();
        Until(cut).Change("2026-11-22");

        cut.Find("[role=dialog]").KeyDown(new KeyboardEventArgs { Key = "Escape" });
        cut.Find("button.ht-dialog__close").Click();
        cut.FindAll(".ht-dialog__foot .ht-btn")[1].Click();

        Assert.Equal(3, _closed);
        Assert.Equal(0, _saved);
        Assert.Empty(_records.ExemptionsOfGoal(goal).Pauses);
    }

    [Fact]
    public async Task HP_007_closing_gives_the_focus_back_to_the_opener_through_the_pages_callback()
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
    public async Task HP_002_Pause_goal_pauses_the_goal_with_the_trimmed_reason_and_raises_the_saved_callback()
    {
        var (cut, goal) = await OpenAsync();
        Until(cut).Change("2026-11-22");
        Reason(cut).Input("  Family visit  ");

        await PauseAsync(cut);

        Assert.Equal(1, _saved);
        Assert.Equal(0, _closed);
        var pause = Assert.Single(_records.ExemptionsOfGoal(goal).Pauses);
        Assert.Equal((Mon16, Sun22, "Family visit"), (pause.FirstDay, pause.LastDay, pause.Reason!.Value));
        Assert.Contains(nameof(IRecordStore.AddPauseAsync), _records.Calls);
    }

    [Fact]
    public async Task HP_002_Choose_the_first_and_the_last_day_shows_under_Until_when_Until_is_empty()
    {
        var (cut, goal) = await OpenAsync();

        await PauseAsync(cut);

        AssertErrorUnder(cut, Until(cut), DaysMissing);
        Assert.Equal("2026-11-16", From(cut).GetAttribute("value"));
        Assert.Equal(0, _saved);
        Assert.Empty(_records.ExemptionsOfGoal(goal).Pauses);
    }

    [Fact]
    public async Task HP_002_Choose_the_first_and_the_last_day_shows_under_From_when_From_is_empty()
    {
        var (cut, _) = await OpenAsync();
        From(cut).Change(string.Empty);
        Until(cut).Change("2026-11-22");

        await PauseAsync(cut);

        AssertErrorUnder(cut, From(cut), DaysMissing);
        Assert.Equal("2026-11-22", Until(cut).GetAttribute("value"));
    }

    [Fact]
    public async Task HP_002_A_pause_cannot_start_before_today_shows_under_From()
    {
        var (cut, _) = await OpenAsync();
        From(cut).Change("2026-11-15");
        Until(cut).Change("2026-11-22");

        await PauseAsync(cut);

        AssertErrorUnder(cut, From(cut), StartsBeforeToday);
        Assert.Equal("2026-11-15", From(cut).GetAttribute("value"));
    }

    [Fact]
    public async Task HP_002_The_last_day_cannot_be_before_the_first_day_shows_under_Until()
    {
        var (cut, _) = await OpenAsync();
        From(cut).Change("2026-11-20");
        Until(cut).Change("2026-11-19");

        await PauseAsync(cut);

        AssertErrorUnder(cut, Until(cut), LastBeforeFirst);
    }

    [Fact]
    public async Task HP_002_an_overlap_shows_under_Until()
    {
        var (cut, goal) = await OpenAsync();
        _records.SeedPause(goal, new DateOnly(2026, 11, 20), Sun22);
        Until(cut).Change("2026-11-21");

        await PauseAsync(cut);

        AssertErrorUnder(cut, Until(cut), Overlaps);
        Assert.Single(_records.ExemptionsOfGoal(goal).Pauses);
    }

    [Fact]
    public async Task HP_002_a_reason_too_long_shows_under_Reason_and_keeps_the_text()
    {
        var (cut, goal) = await OpenAsync();
        var reason = new string('a', 201);
        Until(cut).Change("2026-11-22");
        Reason(cut).Input(reason);

        await PauseAsync(cut);

        AssertErrorUnder(cut, Reason(cut), ReasonTooLong);
        Assert.Equal(reason, Reason(cut).GetAttribute("value"));
        Assert.Empty(_records.ExemptionsOfGoal(goal).Pauses);
    }

    [Fact]
    public async Task HP_002_a_new_refusal_replaces_the_previous_one_under_its_own_field()
    {
        var (cut, _) = await OpenAsync();
        await PauseAsync(cut);
        AssertErrorUnder(cut, Until(cut), DaysMissing);

        From(cut).Change("2026-11-15");
        Until(cut).Change("2026-11-22");
        await PauseAsync(cut);

        AssertErrorUnder(cut, From(cut), StartsBeforeToday);
        Assert.Null(Until(cut).GetAttribute("aria-invalid"));
    }

    [Fact]
    public async Task HP_002_a_refusal_about_no_field_shows_as_an_alert_above_the_fields()
    {
        var (cut, goal) = await OpenAsync(ActingUser.Anonymous);
        Until(cut).Change("2026-11-22");

        await PauseAsync(cut);

        var alert = cut.Find(".ht-dialog__body p.ht-field__error[role=alert]");
        Assert.Equal(NotSignedIn, alert.TextContent);
        Assert.Contains("ht-field-pair", alert.NextElementSibling!.ClassList);
        Assert.Empty(cut.FindAll("[aria-invalid=true]"));
        Assert.Single(cut.FindAll("[role=dialog]"));
        Assert.Empty(_records.ExemptionsOfGoal(goal).Pauses);
    }

    [Fact]
    public async Task HP_002_reopening_starts_again_from_the_current_date_with_no_refusal()
    {
        var (cut, _) = await OpenAsync();
        From(cut).Change("2026-11-15");
        await PauseAsync(cut);
        Assert.Single(cut.FindAll(".ht-field__error"));

        cut.Render(p => p.Add(x => x.Open, false));
        cut.Render(p => p.Add(x => x.Open, true));

        Assert.Equal("2026-11-16", From(cut).GetAttribute("value"));
        Assert.True(string.IsNullOrEmpty(Until(cut).GetAttribute("value")));
        Assert.Empty(cut.FindAll(".ht-field__error"));
    }
}
