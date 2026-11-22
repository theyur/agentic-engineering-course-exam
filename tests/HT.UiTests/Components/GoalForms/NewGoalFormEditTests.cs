using AngleSharp.Dom;
using Bunit;
using HT.Application.Goals;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Ui.Components.Goals;
using HT.Ui.Goals;
using Microsoft.AspNetCore.Components;

// Not Components/Goals (tasks.md T070): a namespace HT.UiTests.Components.Goals would hide the Goals page from the tests
// in HT.UiTests.Components.Pages (CS0118 in GoalsPageTests), which stay unchanged.
namespace HT.UiTests.Components.GoalForms;

/// <summary>
/// Slice 8 (T070; HP-002, HP-009, SC-008, design/pages/PageEditGoalSettings): <see cref="NewGoalForm"/> in its edit mode,
/// rendered alone with a filled model. The creation mode is covered, unchanged, by <c>NewGoalPageTests</c>.
/// </summary>
public sealed class NewGoalFormEditTests : BunitContext
{
    private static readonly DateOnly Thu12 = new(2026, 11, 12);
    private static readonly GoalHabitView Reading = new(new HabitId(Guid.NewGuid()), HabitKind.Catalog, "Reading", Category.Mind, true);

    private static GoalFormModel ReadingDaily() => new()
    {
        Habit = Reading,
        BuildOrReduce = BuildOrReduce.Build,
        Measurement = Measurement.Duration,
        Amount = 15,
        DurationUnit = DurationUnit.Minutes,
        ScheduleKind = ScheduleKind.FixedDays,
        Days = Enum.GetValues<DayOfWeek>().ToHashSet(),
        StartDate = Thu12,
    };

    private static GoalFormModel YogaPerWeek(SchedulePeriod period) => new()
    {
        Habit = Reading with { Name = "Yoga" },
        BuildOrReduce = BuildOrReduce.Build,
        Measurement = Measurement.YesNo,
        ScheduleKind = ScheduleKind.Times,
        Times = 4,
        Period = period,
        StartDate = new DateOnly(2026, 11, 23),
    };

    private IRenderedComponent<NewGoalForm> RenderForm(GoalFormModel model, GoalFormError? error = null, string? refusal = null) =>
        Render<NewGoalForm>(p => p
            .Add(x => x.Model, model)
            .Add(x => x.EditMode, true)
            .Add(x => x.ChangesApplyFrom, model.StartDate)
            .Add(x => x.Error, error)
            .Add(x => x.Refusal, refusal)
            .Add(x => x.ActingUser, ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.RegisteredUser))
            .Add(x => x.CancelHref, "/goals/6f9a3c1e-0000-4000-8000-0000000000aa"));

    private static string Text(IElement element) =>
        string.Join(" ", element.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static IElement SectionOf(IRenderedComponent<NewGoalForm> cut, string title) =>
        cut.FindAll(".ht-form-section").First(s => s.QuerySelector("h2")?.TextContent == title);

    [Fact]
    public void HP_002_the_edit_form_has_the_target_sections_the_goals_days_and_the_name_with_no_habit_and_no_start_date()
    {
        var cut = RenderForm(ReadingDaily());

        Assert.Equal(["Do you want to build or reduce it?", "How do you measure it?", "Target", "When?"],
            cut.FindAll(".ht-form-section__title").Select(t => t.TextContent));
        Assert.Empty(cut.FindAll("input[type=date]"));
        Assert.DoesNotContain("Choose a habit", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Change habit", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Start date", cut.Markup, StringComparison.Ordinal);
        Assert.Equal("true", cut.Find(".ht-seg[aria-label='Build or reduce'] button").GetAttribute("aria-pressed"));
        Assert.True(cut.FindAll("label.ht-choice").Single(c => c.QuerySelector(".ht-choice__title")!.TextContent == "Duration").QuerySelector("input")!.HasAttribute("checked"));
        Assert.Equal("15", cut.Find("#edit-goal-amount").GetAttribute("value"));
        Assert.Equal("At least", SectionOf(cut, "Target").QuerySelector(".ht-inline > span")!.TextContent);

        // "When?" offers only the goal's kind: the DayPicker, no ChoiceCards for the kind.
        var when = SectionOf(cut, "When?");
        Assert.Empty(when.QuerySelectorAll(".ht-choices"));
        Assert.Equal(7, when.QuerySelectorAll(".ht-days button[aria-pressed=true]").Length);
        Assert.Equal("Each selected day is one commitment.", when.QuerySelector(".ht-form-section__note")!.TextContent);
        Assert.DoesNotContain("On fixed days", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("A number of times", cut.Markup, StringComparison.Ordinal);

        // "Name (optional)" alone in its field pair, labelled.
        var pair = cut.Find(".ht-field-pair");
        var label = Assert.Single(pair.QuerySelectorAll("label.ht-field"));
        Assert.Equal("Name (optional)", Text(label.QuerySelector(".ht-field__label")!));
        Assert.NotNull(label.QuerySelector("input#edit-goal-name"));
    }

    [Fact]
    public void HP_002_the_neutral_Notice_names_the_date_above_Save_changes_and_Cancel_and_the_summary_ends_with_it()
    {
        var cut = RenderForm(ReadingDaily());

        var notice = cut.Find(".ht-notice");
        Assert.Equal("ht-notice ht-notice--neutral", notice.ClassName);
        Assert.Equal("Changes apply from Thu 12 Nov. Past results keep their rules.", notice.TextContent);
        Assert.Equal("ht-form-section", notice.ParentElement!.ClassName);
        Assert.Equal("ht-form-actions", notice.ParentElement.NextElementSibling!.ClassName);
        var actions = cut.FindAll(".ht-form-actions .ht-btn");
        Assert.Equal(["Save changes", "Cancel"], actions.Select(a => a.TextContent));
        Assert.Equal("submit", actions[0].GetAttribute("type"));
        Assert.Equal("/goals/6f9a3c1e-0000-4000-8000-0000000000aa", actions[1].GetAttribute("href"));
        Assert.Equal("Reading: at least 15 min daily, starting Thu 12 Nov.", cut.Find("aside.ht-summary .ht-card__title").TextContent);
    }

    [Theory]
    [InlineData(SchedulePeriod.Week, "Each week, Monday to Sunday, is evaluated as a whole.")]
    [InlineData(SchedulePeriod.Day, "Each day is evaluated as a whole.")]
    public void HP_002_a_goal_counted_per_period_keeps_its_times_and_names_its_period_without_a_period_control(SchedulePeriod period, string note)
    {
        var cut = RenderForm(YogaPerWeek(period));

        var when = SectionOf(cut, "When?");
        Assert.Equal("4", cut.Find("#edit-goal-times").GetAttribute("value"));
        Assert.Equal("times", when.QuerySelector(".ht-unit")!.TextContent);
        Assert.Empty(cut.FindAll(".ht-seg[aria-label='Period']"));
        Assert.Empty(when.QuerySelectorAll(".ht-days"));
        Assert.Equal(note, when.QuerySelector(".ht-form-section__note")!.TextContent);
        Assert.Empty(cut.FindAll("#edit-goal-amount"));
    }

    [Fact]
    public void HP_009_the_first_field_Build_gets_the_focus_when_the_form_opens()
    {
        var cut = RenderForm(ReadingDaily());

        var invocation = Assert.Single(JSInterop.VerifyFocusAsyncInvoke(1));
        var build = cut.Find(".ht-seg[aria-label='Build or reduce'] button");
        Assert.Equal("Build", build.TextContent);
        Assert.Equal(build.GetAttribute("blazor:elementReference"), Assert.IsType<ElementReference>(invocation.Arguments[0]).Id);
    }

    public static TheoryData<GoalField, string, string> FieldRefusals => new()
    {
        { GoalField.Amount, "Enter a whole number from 1 to 999.", "#edit-goal-amount" },
        { GoalField.Days, "Choose at least one day.", ".ht-days" },
        { GoalField.Schedule, "A change keeps the schedule kind: fixed days, times per day or times per week. End the goal and start a new attempt to change it.", ".ht-days" },
        { GoalField.Name, "The name can have at most 60 characters.", "#edit-goal-name" },
        { GoalField.BuildOrReduce, "Choose Build or Reduce.", ".ht-seg[aria-label='Build or reduce']" },
        { GoalField.Measurement, "Choose how it is measured.", ".ht-choices" },
    };

    [Theory]
    [MemberData(nameof(FieldRefusals))]
    public void HP_002_a_refusal_shows_once_under_its_field_and_the_field_points_to_it(GoalField field, string message, string described)
    {
        var cut = RenderForm(ReadingDaily(), new GoalFormError(field, message));

        var error = Assert.Single(cut.FindAll(".ht-field__error"));
        Assert.Equal(message, error.TextContent);
        Assert.Equal($"edit-goal-{field}-error", error.Id);
        Assert.Equal(error.Id, cut.Find(described).GetAttribute("aria-describedby"));
        Assert.Empty(cut.FindAll("[role=alert]"));
    }

    [Fact]
    public void HP_002_the_Reduce_times_refusal_sits_under_the_times_Stepper_of_a_per_day_goal()
    {
        var cut = RenderForm(YogaPerWeek(SchedulePeriod.Day), new GoalFormError(GoalField.Times, "A Reduce goal with an amount has 1 time per period."));

        var times = cut.Find("#edit-goal-times");
        Assert.Equal("true", times.GetAttribute("aria-invalid"));
        Assert.Equal("edit-goal-Times-error", times.GetAttribute("aria-describedby"));
        Assert.Equal("A Reduce goal with an amount has 1 time per period.", SectionOf(cut, "When?").QuerySelector(".ht-field__error")!.TextContent);
    }

    [Fact]
    public void HP_002_a_refusal_about_no_field_is_an_alert_above_the_first_section()
    {
        var cut = RenderForm(ReadingDaily(), refusal: "This goal has ended.");

        var alert = cut.Find("form > p.ht-field__error[role=alert]");
        Assert.Equal("This goal has ended.", alert.TextContent);
        Assert.Equal("ht-form-section", alert.NextElementSibling!.ClassName);
        Assert.NotEqual("ht-form-section", alert.PreviousElementSibling?.ClassName);
        Assert.Single(cut.FindAll(".ht-field__error"));
    }
}
