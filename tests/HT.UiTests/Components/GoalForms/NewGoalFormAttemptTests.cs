using AngleSharp.Dom;
using Bunit;
using HT.Application.Goals;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Ui.Components.Goals;
using HT.Ui.Goals;
using Microsoft.AspNetCore.Components;

namespace HT.UiTests.Components.GoalForms;

/// <summary>
/// Slice 8 (T092; HP-004, HP-009, SC-008, design/pages/PageNewAttempt): <see cref="NewGoalForm"/> in its attempt mode,
/// rendered alone with the model of the starting values of "Meditation, 10 min daily" on Wednesday 18 November
/// (US3-AS1). The creation mode is covered, unchanged, by <c>NewGoalPageTests</c>, the edit mode by
/// <see cref="NewGoalFormEditTests"/>.
/// </summary>
public sealed class NewGoalFormAttemptTests : BunitContext
{
    private const string GoalAddress = "/goals/6f9a3c1e-0000-4000-8000-0000000000bb";
    private static readonly GoalHabitView Meditation = new(new HabitId(Guid.NewGuid()), HabitKind.Catalog, "Meditation", Category.Mind, true);

    private static NewAttemptValues StartingValues => new()
    {
        BuildOrReduce = BuildOrReduce.Build,
        Measurement = Measurement.Duration,
        Amount = 10,
        DurationUnit = DurationUnit.Minutes,
        ScheduleKind = ScheduleKind.FixedDays,
        Days = Enum.GetValues<DayOfWeek>().ToHashSet(),
        StartDate = new DateOnly(2026, 11, 18),
        AttemptNumber = 2,
    };

    private IRenderedComponent<NewGoalForm> RenderForm(GoalFormError? error = null, string? refusal = null, GoalFormModel? model = null) =>
        Render<NewGoalForm>(p => p
            .Add(x => x.Model, model ?? GoalFormModel.From(StartingValues, Meditation))
            .Add(x => x.AttemptMode, true)
            .Add(x => x.Error, error)
            .Add(x => x.Refusal, refusal)
            .Add(x => x.ActingUser, ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.RegisteredUser))
            .Add(x => x.CancelHref, GoalAddress));

    private static string Text(IElement element) =>
        string.Join(" ", element.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static IElement SectionOf(IRenderedComponent<NewGoalForm> cut, string title) =>
        cut.FindAll(".ht-form-section").First(s => s.QuerySelector("h2")?.TextContent == title);

    [Fact]
    public void HP_004_the_attempt_form_has_the_target_sections_every_schedule_kind_and_the_start_date_with_no_habit_and_no_name()
    {
        var cut = RenderForm();

        Assert.Equal(["Do you want to build or reduce it?", "How do you measure it?", "Target", "When?"],
            cut.FindAll(".ht-form-section__title").Select(t => t.TextContent));
        Assert.DoesNotContain("Choose a habit", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Change habit", cut.Markup, StringComparison.Ordinal);
        Assert.Empty(cut.FindAll("[role=dialog]"));
        Assert.Equal("true", cut.Find(".ht-seg[aria-label='Build or reduce'] button").GetAttribute("aria-pressed"));
        Assert.True(cut.FindAll("label.ht-choice").Single(c => c.QuerySelector(".ht-choice__title")!.TextContent == "Duration").QuerySelector("input")!.HasAttribute("checked"));
        Assert.Equal("10", cut.Find("#new-attempt-amount").GetAttribute("value"));

        // "When?" offers every schedule kind, as on the New goal page, here the goal's fixed days.
        var when = SectionOf(cut, "When?");
        Assert.Equal(["On fixed days", "A number of times"], when.QuerySelectorAll(".ht-choice__title").Select(t => t.TextContent));
        Assert.Equal("new-attempt-when", when.QuerySelector("h2")!.Id);
        Assert.Equal(7, when.QuerySelectorAll(".ht-days button[aria-pressed=true]").Length);

        // "Start date" alone in its field pair: no name.
        var pair = cut.Find(".ht-field-pair");
        var label = Assert.Single(pair.QuerySelectorAll("label.ht-field"));
        Assert.Equal("Start date", Text(label.QuerySelector(".ht-field__label")!));
        var start = label.QuerySelector("input#new-attempt-start")!;
        Assert.Equal(("date", "2026-11-18"), (start.GetAttribute("type"), start.GetAttribute("value")));
        Assert.DoesNotContain("Name", cut.Markup, StringComparison.Ordinal);
        Assert.Empty(cut.FindAll("[id$=-name]"));
    }

    [Fact]
    public void HP_004_Start_attempt_and_Cancel_and_the_summary_with_the_habit_and_the_start_date()
    {
        var cut = RenderForm();

        var actions = cut.FindAll(".ht-form-actions .ht-btn");
        Assert.Equal(["Start attempt", "Cancel"], actions.Select(a => a.TextContent));
        Assert.Equal("submit", actions[0].GetAttribute("type"));
        Assert.Equal(GoalAddress, actions[1].GetAttribute("href"));
        Assert.Equal("Meditation: at least 10 min daily, starting Wed 18 Nov.", cut.Find("aside.ht-summary .ht-card__title").TextContent);
        Assert.Empty(cut.FindAll(".ht-notice--neutral"));
        Assert.Empty(cut.FindAll("[role=alert]"));
    }

    [Fact]
    public void US3_AS5_another_schedule_kind_can_be_chosen_with_its_times_and_period()
    {
        var cut = RenderForm();

        SectionOf(cut, "When?").QuerySelectorAll("label.ht-choice input")[1].Change(true);

        var when = SectionOf(cut, "When?");
        Assert.NotNull(cut.Find("#new-attempt-times"));
        Assert.NotNull(when.QuerySelector(".ht-seg[aria-label='Period']"));
        Assert.Empty(when.QuerySelectorAll(".ht-days"));
    }

    [Fact]
    public void HP_009_the_first_field_Build_gets_the_focus_when_the_form_opens()
    {
        var cut = RenderForm();

        var invocation = Assert.Single(JSInterop.VerifyFocusAsyncInvoke(1));
        var build = cut.Find(".ht-seg[aria-label='Build or reduce'] button");
        Assert.Equal("Build", build.TextContent);
        Assert.Equal(build.GetAttribute("blazor:elementReference"), Assert.IsType<ElementReference>(invocation.Arguments[0]).Id);
    }

    public static TheoryData<GoalField, string, string> FieldRefusals => new()
    {
        { GoalField.StartDate, "Choose a start date.", "#new-attempt-start" },
        { GoalField.StartDate, "The start date cannot be in the past.", "#new-attempt-start" },
        { GoalField.StartDate, "A new attempt starts after the day the goal ended.", "#new-attempt-start" },
        { GoalField.StartDate, "Choose a start date within a year from today.", "#new-attempt-start" },
        { GoalField.Amount, "Enter a whole number from 1 to 999.", "#new-attempt-amount" },
        { GoalField.Days, "Choose at least one day.", ".ht-days" },
        { GoalField.BuildOrReduce, "Choose Build or Reduce.", ".ht-seg[aria-label='Build or reduce']" },
        { GoalField.Measurement, "Choose how it is measured.", ".ht-choices" },
    };

    [Theory]
    [MemberData(nameof(FieldRefusals))]
    public void HP_004_a_refusal_shows_once_under_its_field_and_the_field_points_to_it(GoalField field, string message, string described)
    {
        var cut = RenderForm(new GoalFormError(field, message));

        var error = Assert.Single(cut.FindAll(".ht-field__error"));
        Assert.Equal(message, error.TextContent);
        Assert.Equal($"new-attempt-{field}-error", error.Id);
        Assert.Equal(error.Id, cut.Find(described).GetAttribute("aria-describedby"));
        Assert.Empty(cut.FindAll("[role=alert]"));
    }

    [Fact]
    public void HP_004_a_start_date_refusal_sits_in_the_Start_date_field_and_marks_it_invalid()
    {
        var cut = RenderForm(new GoalFormError(GoalField.StartDate, "A new attempt starts after the day the goal ended."));

        var label = cut.Find(".ht-field-pair label.ht-field");
        Assert.Equal("A new attempt starts after the day the goal ended.", label.QuerySelector(".ht-field__error")!.TextContent);
        Assert.Equal("true", cut.Find("#new-attempt-start").GetAttribute("aria-invalid"));
    }

    [Fact]
    public void HP_004_the_Reduce_times_refusal_sits_under_the_times_Stepper()
    {
        var model = GoalFormModel.From(StartingValues with
        {
            BuildOrReduce = BuildOrReduce.Reduce, ScheduleKind = ScheduleKind.Times, Days = new HashSet<DayOfWeek>(), Times = 2, Period = SchedulePeriod.Day,
        }, Meditation);

        var cut = RenderForm(new GoalFormError(GoalField.Times, "A Reduce goal with an amount has 1 time per period."), model: model);

        var times = cut.Find("#new-attempt-times");
        Assert.Equal("true", times.GetAttribute("aria-invalid"));
        Assert.Equal("new-attempt-Times-error", times.GetAttribute("aria-describedby"));
        Assert.Equal("A Reduce goal with an amount has 1 time per period.", SectionOf(cut, "When?").QuerySelector(".ht-field__error")!.TextContent);
    }

    [Fact]
    public void HP_004_a_refusal_about_no_field_is_an_alert_above_the_first_section()
    {
        var cut = RenderForm(refusal: "Only an ended goal can start a new attempt.");

        var alert = cut.Find("form > p.ht-field__error[role=alert]");
        Assert.Equal("Only an ended goal can start a new attempt.", alert.TextContent);
        Assert.Equal("ht-form-section", alert.NextElementSibling!.ClassName);
        Assert.NotEqual("ht-form-section", alert.PreviousElementSibling?.ClassName);
        Assert.Single(cut.FindAll(".ht-field__error"));
    }
}
