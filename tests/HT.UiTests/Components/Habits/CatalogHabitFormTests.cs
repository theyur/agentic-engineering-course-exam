using Bunit;
using HT.Domain.Habits;
using HT.Ui.Components.Habits;
using HT.Ui.Habits;

namespace HT.UiTests.Components.Habits;

/// <summary>HP-009: the administrator's catalog habit form and its suggested starting goal editor.</summary>
public sealed class CatalogHabitFormTests : BunitContext
{
    private readonly HabitFormModel _model = new() { Name = "Reading", Category = Category.Mind, Intent = Intent.Build, Icon = "book-open" };
    private readonly List<SuggestedStartingGoalFormModel> _goals = [];

    private IRenderedComponent<CatalogHabitForm> RenderForm(FormError? error = null) =>
        Render<CatalogHabitForm>(p => p
            .Add(x => x.Model, _model)
            .Add(x => x.Goals, _goals)
            .Add(x => x.Error, error));

    private static void Click(IRenderedComponent<CatalogHabitForm> cut, string text) =>
        cut.FindAll("button").First(b => b.TextContent.Trim() == text).Click();

    [Fact]
    public void The_form_has_the_habit_fields_and_the_24_habit_icons()
    {
        var cut = RenderForm();

        Assert.Equal(["Name", "Description (optional)", "Category", "Usually you want to", "Icon"],
            cut.FindAll(".ht-field__label").Take(5).Select(l => string.Join(" ", l.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))));
        Assert.Equal(HabitIcon.All.Select(i => i.Name), cut.FindAll("button.ht-icon-choice").Select(b => b.GetAttribute("aria-label")));
        Assert.Equal("true", cut.Find("button[aria-label=book-open]").GetAttribute("aria-pressed"));
        Assert.Equal(["Choose a category", "Body", "Mind", "Health", "Money", "Home", "Other"], cut.FindAll("select option").Select(o => o.TextContent));
        Assert.Equal(["Save", "Cancel"], cut.FindAll(".ht-form-actions .ht-btn").Select(b => b.TextContent));
        Assert.Equal("/admin", cut.Find(".ht-form-actions a").GetAttribute("href"));
    }

    [Fact]
    public void Add_remove_move_and_mark_and_no_add_at_five()
    {
        var cut = RenderForm();

        Click(cut, "+ Add suggested starting goal");
        Assert.Single(_goals);
        Assert.True(_goals[0].IsMarked);

        Click(cut, "+ Add suggested starting goal");
        _goals[1].CountUnit = "second";
        cut.Render();
        Assert.Equal(["Suggested starting goal 1", "Suggested starting goal 2"], cut.FindAll(".ht-editor-item h3").Select(h => h.TextContent));
        Assert.DoesNotContain(cut.FindAll(".ht-editor-item")[0].QuerySelectorAll("button"), b => b.TextContent == "Move up");
        Assert.DoesNotContain(cut.FindAll(".ht-editor-item")[1].QuerySelectorAll("button"), b => b.TextContent == "Move down");

        cut.FindAll(".ht-editor-item")[1].QuerySelectorAll("input[type=radio]").Single().Change(true);
        Assert.Equal([false, true], _goals.Select(g => g.IsMarked));

        cut.FindAll(".ht-editor-item")[1].QuerySelectorAll("button").First(b => b.TextContent == "Move up").Click();
        Assert.Equal("second", _goals[0].CountUnit);
        Assert.Equal([true, false], _goals.Select(g => g.IsMarked));

        cut.FindAll(".ht-editor-item")[0].QuerySelectorAll("button").First(b => b.TextContent == "Remove").Click();
        Assert.Single(_goals);
        Assert.True(_goals[0].IsMarked);

        for (var i = 0; i < 4; i++)
        {
            Click(cut, "+ Add suggested starting goal");
        }

        Assert.Equal(5, _goals.Count);
        Assert.DoesNotContain(cut.FindAll("button"), b => b.TextContent.Contains("Add suggested", StringComparison.Ordinal));
        Assert.Contains("A habit can have at most 5 suggested starting goals.", cut.Markup, StringComparison.Ordinal);
        Assert.Equal("Remove suggested starting goal 5", cut.FindAll(".ht-editor-item__head")[4].QuerySelectorAll("button")[^1].GetAttribute("aria-label"));
    }

    [Fact]
    public void Only_the_controls_of_the_chosen_measurement_and_schedule_are_shown()
    {
        _goals.Add(new SuggestedStartingGoalFormModel { IsMarked = true });
        var cut = RenderForm();
        var item = () => cut.Find(".ht-editor-item");

        Assert.Empty(item().QuerySelectorAll("input[type=number]"));
        Assert.Empty(item().QuerySelectorAll(".ht-days"));

        item().QuerySelectorAll("button").First(b => b.TextContent == "Count").Click();
        Assert.Single(item().QuerySelectorAll("input[type=number]"));
        Assert.NotNull(item().QuerySelector("input.ht-unit-input"));
        Assert.Equal("At least", item().QuerySelectorAll(".ht-field")[2].QuerySelector(".ht-inline > span")!.TextContent);

        item().QuerySelectorAll("button").First(b => b.TextContent == "Reduce").Click();
        Assert.Equal("0", item().QuerySelector("input[type=number]")!.GetAttribute("min"));

        item().QuerySelectorAll("button").First(b => b.TextContent == "Duration").Click();
        Assert.Null(item().QuerySelector("input.ht-unit-input"));
        Assert.Contains(item().QuerySelectorAll("button"), b => b.TextContent == "Minutes");

        item().QuerySelectorAll("button").First(b => b.TextContent == "Yes/no").Click();
        Assert.Empty(item().QuerySelectorAll("input[type=number]"));

        item().QuerySelectorAll("button").First(b => b.TextContent == "Fixed days").Click();
        Assert.NotNull(item().QuerySelector(".ht-days"));
        item().QuerySelectorAll("button").First(b => b.TextContent == "A number of times").Click();
        Assert.Null(item().QuerySelector(".ht-days"));
        Assert.Single(item().QuerySelectorAll("input[type=number]"));
        Assert.Contains(item().QuerySelectorAll("button"), b => b.TextContent == "per week");
    }

    [Fact]
    public void Values_of_another_measurement_are_not_sent()
    {
        var model = new SuggestedStartingGoalFormModel
        {
            BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo, Amount = 5, CountUnit = "pages",
            DurationUnit = DurationUnit.Hours, ScheduleKind = ScheduleKind.Times, Times = 2, Period = SchedulePeriod.Week,
            Days = new HashSet<DayOfWeek> { DayOfWeek.Monday },
        };

        var input = model.ToInput();

        Assert.Null(input.Amount);
        Assert.Null(input.CountUnit);
        Assert.Null(input.DurationUnit);
        Assert.Empty(input.Days);
        Assert.Equal(2, input.Times);
    }

    [Fact]
    public void A_refusal_with_position_2_sits_under_the_second_suggested_starting_goal()
    {
        _goals.Add(new SuggestedStartingGoalFormModel { IsMarked = true, Measurement = Measurement.Count, BuildOrReduce = BuildOrReduce.Build });
        _goals.Add(new SuggestedStartingGoalFormModel { Measurement = Measurement.Count, BuildOrReduce = BuildOrReduce.Build });

        var cut = RenderForm(FormError.Of("CountUnitMissing", "Enter a unit, for example pages.", 2));

        var items = cut.FindAll(".ht-editor-item");
        Assert.Empty(items[0].QuerySelectorAll(".ht-field__error"));
        var error = items[1].QuerySelector(".ht-field__error")!;
        Assert.Equal("Enter a unit, for example pages.", error.TextContent);
        Assert.Equal(error.Id, items[1].QuerySelector("input[type=number]")!.GetAttribute("aria-describedby"));
        var unit = items[1].QuerySelector("input.ht-unit-input")!;
        Assert.Equal(error.Id, unit.GetAttribute("aria-describedby"));
        Assert.Equal("true", unit.GetAttribute("aria-invalid"));
        Assert.Null(items[0].QuerySelector("input.ht-unit-input")!.GetAttribute("aria-invalid"));
    }

    [Fact]
    public void The_duration_unit_and_the_period_point_to_the_refusal_they_sit_under()
    {
        _goals.Add(new SuggestedStartingGoalFormModel
        {
            IsMarked = true, Measurement = Measurement.Duration, BuildOrReduce = BuildOrReduce.Build, ScheduleKind = ScheduleKind.Times,
        });

        var unit = RenderForm(FormError.Of("DurationUnitMissing", "Choose minutes or hours.", 1));
        var unitError = unit.Find(".ht-editor-item .ht-field__error");
        Assert.Equal(unitError.Id, unit.Find(".ht-seg[aria-label='Unit, suggested starting goal 1']").GetAttribute("aria-describedby"));

        var period = RenderForm(FormError.Of("PeriodMissing", "Choose per day or per week.", 1));
        var periodError = period.Find(".ht-editor-item .ht-field__error");
        Assert.Equal(periodError.Id, period.Find(".ht-seg[aria-label='Per day or per week, suggested starting goal 1']").GetAttribute("aria-describedby"));
    }

    [Fact]
    public void Each_suggested_starting_goal_has_its_own_control_ids()
    {
        for (var i = 0; i < 2; i++)
        {
            _goals.Add(new SuggestedStartingGoalFormModel
            {
                IsMarked = i == 0, Measurement = Measurement.Count, BuildOrReduce = BuildOrReduce.Build, ScheduleKind = ScheduleKind.Times,
            });
        }

        var cut = RenderForm();

        var ids = cut.FindAll("[id]").Select(e => e.Id).ToList();
        Assert.Equal(ids.Count, ids.Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(["suggested-1-amount", "suggested-1-times", "suggested-2-amount", "suggested-2-times"],
            cut.FindAll(".ht-editor-item input[type=number]").Select(e => e.Id));
    }

    [Fact]
    public void A_list_refusal_sits_above_the_list_and_a_field_refusal_under_its_field()
    {
        var list = RenderForm(FormError.Of("SuggestedStartingGoalNotMarked", "Mark one suggested starting goal for the card."));
        Assert.Equal("Mark one suggested starting goal for the card.", list.Find("p.ht-field__error[role=alert]").TextContent);

        var name = RenderForm(FormError.Of("NameTaken", "A catalog habit with this name already exists."));
        Assert.Equal("A catalog habit with this name already exists.", name.Find("#catalog-habit-name-error").TextContent);
        Assert.Single(name.FindAll(".ht-field__error"));
    }
}
