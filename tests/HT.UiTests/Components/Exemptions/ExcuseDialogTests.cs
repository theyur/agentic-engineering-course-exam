using AngleSharp.Dom;
using Bunit;
using HT.Application.Goals;
using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using HT.Ui.Components.Exemptions;
using HT.UiTests.Components.Pages;
using HT.UnitTests.Fakes;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace HT.UiTests.Components.Exemptions;

/// <summary>
/// The Excuse dialog (HP-003, HP-007; design/components/ExcuseDialog, slice 7 state; T047) over the real ExcusalService
/// and the fakes. The user has no stored time zone, so the goal is in UTC and every time below is a UTC literal. "Yoga,
/// 3 days a week" (Build, yes/no, Monday, Wednesday and Friday) starts on Monday 2 November 2026, created at 08:00 that
/// day. The dialog is opened at 02:00 on Friday 13 November for the Pending Wednesday 11 November commitment: it ended at
/// 00:00 on Thursday 12 and its grace period ends at 00:00 on Saturday 14, 22 hours later. The sub line is the
/// RecordDialog's "For" label of that commitment, read from what can be recorded now at that instant.
/// </summary>
public sealed class ExcuseDialogTests : BunitContext
{
    private const string ReasonTooLong = "Keep the reason to 200 characters or fewer.";
    private const string HasRecord = "This commitment has a record. Delete the record to excuse it.";

    private static readonly DateOnly Mon2 = new(2026, 11, 2);
    private static readonly DateOnly Wed11 = new(2026, 11, 11);

    private readonly FakeCatalogHabitStore _catalog;
    private readonly FakeRecordStore _records;
    private readonly FakeTimeProvider _clock;
    private readonly ActingUser _user = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.RegisteredUser);
    private int _saved;
    private int _closed;

    public ExcuseDialogTests()
    {
        _catalog = this.AddCatalog();
        _records = (FakeRecordStore)Services.GetRequiredService<IRecordStore>();
        _clock = (FakeTimeProvider)Services.GetRequiredService<TimeProvider>();
        _clock.SetUtcNow(new DateTimeOffset(2026, 11, 2, 8, 0, 0, TimeSpan.Zero));
    }

    private static string Text(IElement element) =>
        string.Join(" ", element.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static GoalTargetFields YogaOnMonWedFri => new()
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo, ScheduleKind = ScheduleKind.FixedDays,
        Days = new HashSet<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
    };

    /// <summary>Creates the goal, moves the clock to 02:00 on Friday 13 November and renders the open dialog for Wednesday 11.</summary>
    private async Task<(IRenderedComponent<ExcuseDialog> Cut, GoalId Goal)> OpenAsync()
    {
        var habit = _catalog.Add(HabitSamples.Catalog("Yoga"));
        await using var scope = Services.CreateAsyncScope();
        var created = await scope.ServiceProvider.GetRequiredService<GoalService>().CreateAsync(
            _user, new GoalInput { Habit = new GoalHabit(habit.Id, HabitKind.Catalog), Target = YogaOnMonWedFri, StartDate = Mon2 },
            CancellationToken.None);
        Assert.Equal(GoalCreateOutcome.Created, created.Outcome);
        var goal = created.GoalId!.Value;

        _clock.SetUtcNow(new DateTimeOffset(2026, 11, 13, 2, 0, 0, TimeSpan.Zero));
        var read = await scope.ServiceProvider.GetRequiredService<RecordingService>().GetRecordableAsync(_user, goal, CancellationToken.None);
        var recordable = read.History!;
        var entry = recordable.Entries.Single(e => e.FirstDate == Wed11);
        var subLine = RecordingText.ForLabel(entry, recordable.CurrentDate!.Value, recordable.ReadAt);

        var cut = Render<ExcuseDialog>(p => p
            .Add(x => x.Open, true)
            .Add(x => x.GoalId, goal)
            .Add(x => x.Date, Wed11)
            .Add(x => x.GoalName, "Yoga, 3 days a week")
            .Add(x => x.SubLine, subLine)
            .Add(x => x.ActingUser, _user)
            .Add(x => x.OnClose, () => _closed++)
            .Add(x => x.OnSaved, () => _saved++));
        return (cut, goal);
    }

    private static Task ExcuseAsync(IRenderedComponent<ExcuseDialog> cut) =>
        cut.Find(".ht-dialog__foot .ht-btn--primary").ClickAsync(new MouseEventArgs());

    [Fact]
    public async Task HP_003_the_dialog_names_the_goal_the_commitment_and_its_state_the_effect_the_reason_and_the_buttons()
    {
        var (cut, _) = await OpenAsync();

        var dialog = cut.Find("[role=dialog]");
        Assert.Equal("Excuse · Yoga, 3 days a week", dialog.QuerySelector(".ht-dialog__head h2")!.TextContent);
        Assert.Equal("Wed 11 Nov · Pending, 22 h left", dialog.QuerySelector(".ht-dialog__sub")!.TextContent);
        Assert.Equal("An excused commitment is not counted. Your streak stays as it is.", cut.Find(".ht-dialog__body p").TextContent);
        var field = cut.Find(".ht-dialog__body label.ht-field");
        Assert.Equal("Reason (optional)", Text(field.QuerySelector(".ht-field__label")!));
        Assert.Equal("(optional)", field.QuerySelector(".ht-field__label .ht-muted")!.TextContent);
        var input = field.QuerySelector("input.ht-input[type=text]")!;
        Assert.True(string.IsNullOrEmpty(input.GetAttribute("value")));
        Assert.Null(input.GetAttribute("maxlength"));
        Assert.Null(input.GetAttribute("placeholder"));
        Assert.Empty(cut.FindAll(".ht-field__error"));
        Assert.Equal(["Excuse commitment", "Cancel"], cut.FindAll(".ht-dialog__foot .ht-btn").Select(b => b.TextContent));
        Assert.Contains("ht-btn--primary", cut.FindAll(".ht-dialog__foot .ht-btn")[0].ClassList);
        Assert.Contains("ht-btn--secondary", cut.FindAll(".ht-dialog__foot .ht-btn")[1].ClassList);
    }

    [Fact]
    public async Task HP_007_focus_moves_to_the_reason_on_open()
    {
        var (cut, _) = await OpenAsync();

        var focused = JSInterop.VerifyFocusAsyncInvoke().Arguments[0];
        Assert.Equal(cut.Find("input.ht-input").GetAttribute("blazor:elementReference"), Assert.IsType<ElementReference>(focused).Id);
    }

    [Fact]
    public async Task HP_007_Esc_the_close_button_and_Cancel_ask_the_page_to_close_it_and_excuse_nothing()
    {
        var (cut, goal) = await OpenAsync();
        cut.Find("input.ht-input").Input("Ill");

        cut.Find("[role=dialog]").KeyDown(new KeyboardEventArgs { Key = "Escape" });
        cut.Find("button.ht-dialog__close").Click();
        cut.FindAll(".ht-dialog__foot .ht-btn")[1].Click();

        Assert.Equal(3, _closed);
        Assert.Equal(0, _saved);
        Assert.Empty(_records.ExemptionsOfGoal(goal).Excusals);
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
    public async Task HP_003_Excuse_commitment_excuses_it_with_the_trimmed_reason_and_raises_the_saved_callback()
    {
        var (cut, goal) = await OpenAsync();
        cut.Find("input.ht-input").Input("  Ill  ");

        await ExcuseAsync(cut);

        Assert.Equal(1, _saved);
        Assert.Equal(0, _closed);
        Assert.Equal("Ill", _records.ExemptionsOfGoal(goal).ExcusalOf(Wed11)!.Reason!.Value);
        Assert.Contains(nameof(IRecordStore.ExcuseAsync), _records.Calls);
    }

    [Fact]
    public async Task HP_003_a_reason_too_long_shows_under_the_field_keeps_the_text_and_keeps_the_dialog_open()
    {
        var (cut, goal) = await OpenAsync();
        var reason = new string('a', 201);
        cut.Find("input.ht-input").Input(reason);

        await ExcuseAsync(cut);

        var input = cut.Find("input.ht-input");
        var error = cut.Find("label.ht-field .ht-field__error");
        Assert.Equal(ReasonTooLong, error.TextContent);
        Assert.Equal("true", input.GetAttribute("aria-invalid"));
        Assert.Equal(error.Id, input.GetAttribute("aria-describedby"));
        Assert.Equal(reason, input.GetAttribute("value"));
        Assert.Empty(cut.FindAll("[role=alert]"));
        Assert.Single(cut.FindAll("[role=dialog]"));
        Assert.Equal(0, _saved);
        Assert.Empty(_records.ExemptionsOfGoal(goal).Excusals);
    }

    [Fact]
    public async Task HP_003_a_refusal_about_the_commitment_shows_as_an_alert_in_the_dialog_and_keeps_it_open()
    {
        var (cut, goal) = await OpenAsync();
        _records.Seed(goal, Wed11, RecordValue.FromAnswer(YesNoAnswer.No));
        cut.Find("input.ht-input").Input("Ill");

        await ExcuseAsync(cut);

        var alert = cut.Find(".ht-dialog__body p.ht-field__error[role=alert]");
        Assert.Equal(HasRecord, alert.TextContent);
        var input = cut.Find("input.ht-input");
        Assert.Null(input.GetAttribute("aria-invalid"));
        Assert.Equal("Ill", input.GetAttribute("value"));
        Assert.Single(cut.FindAll("[role=dialog]"));
        Assert.Equal(0, _saved);
        Assert.Empty(_records.ExemptionsOfGoal(goal).Excusals);
    }
}
