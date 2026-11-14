using Bunit;
using HT.Domain.Habits;
using HT.Ui.Components.Shared;

namespace HT.UiTests.Components.Shared;

/// <summary>DataTable, SegmentedControl, Stepper and DayPicker against their design files, labelled for assistive technology.</summary>
public sealed class FormControlTests : BunitContext
{
    [Fact]
    public void DataTable_renders_its_headings_and_rows_in_the_table_wrap()
    {
        var cut = Render<DataTable>(p => p
            .Add(x => x.Columns, ["Habit", "Status", string.Empty])
            .Add(x => x.Label, "Catalog habits")
            .AddChildContent("<tr><td><b>Reading</b></td><td>Draft</td><td class=\"ht-actions\"></td></tr>"));

        Assert.NotNull(cut.Find(".ht-table-wrap > table.ht-table"));
        Assert.Equal("Catalog habits", cut.Find("table").GetAttribute("aria-label"));
        Assert.Equal(["Habit", "Status", ""], cut.FindAll("th").Select(th => th.TextContent));
        Assert.All(cut.FindAll("th"), th => Assert.Equal("col", th.GetAttribute("scope")));
        Assert.Equal("Reading", cut.Find("tbody td b").TextContent);
    }

    [Fact]
    public void SegmentedControl_presses_the_chosen_option_and_reports_a_choice()
    {
        Intent? chosen = null;
        var cut = Render<SegmentedControl<Intent>>(p => p
            .Add(x => x.Label, "Usually you want to")
            .Add(x => x.Options, [(Intent.Build, "Build"), (Intent.Reduce, "Reduce")])
            .Add(x => x.Value, Intent.Build)
            .Add(x => x.ValueChanged, (Intent i) => chosen = i));

        var group = cut.Find(".ht-seg[role=group]");
        Assert.Equal("Usually you want to", group.GetAttribute("aria-label"));
        Assert.Equal(["true", "false"], cut.FindAll("button").Select(b => b.GetAttribute("aria-pressed")));
        Assert.All(cut.FindAll("button"), b => Assert.Equal("button", b.GetAttribute("type")));
        cut.FindAll("button")[1].Click();
        Assert.Equal(Intent.Reduce, chosen);
    }

    [Fact]
    public void SegmentedControl_presses_none_until_the_user_chooses()
    {
        var cut = Render<SegmentedControl<Intent>>(p => p
            .Add(x => x.Label, "Intent")
            .Add(x => x.Options, [(Intent.Build, "Build"), (Intent.Reduce, "Reduce")]));

        Assert.All(cut.FindAll("button"), b => Assert.Equal("false", b.GetAttribute("aria-pressed")));
    }

    [Fact]
    public void Stepper_has_Less_and_More_a_labelled_number_and_its_unit()
    {
        int? value = null;
        var cut = Render<Stepper>(p => p
            .Add(x => x.Label, "Amount")
            .Add(x => x.Value, 1)
            .Add(x => x.Min, 1)
            .Add(x => x.Unit, "pages")
            .Add(x => x.ValueChanged, (int? v) => value = v));

        Assert.Equal(["Less", "More"], cut.FindAll(".ht-stepper button").Select(b => b.GetAttribute("aria-label")));
        var input = cut.Find("input[type=number]");
        Assert.Equal("Amount", input.GetAttribute("aria-label"));
        Assert.Equal("1", input.GetAttribute("min"));
        Assert.Equal("999", input.GetAttribute("max"));
        Assert.Equal("pages", cut.Find(".ht-unit").TextContent);

        cut.FindAll(".ht-stepper button")[1].Click();
        Assert.Equal(2, value);
        cut.FindAll(".ht-stepper button")[0].Click();
        Assert.Equal(1, value);
    }

    [Fact]
    public void Stepper_does_not_go_below_its_minimum_and_passes_typed_values_on()
    {
        int? value = 7;
        var cut = Render<Stepper>(p => p
            .Add(x => x.Label, "Amount")
            .Add(x => x.Value, 0)
            .Add(x => x.Min, 0)
            .Add(x => x.ValueChanged, (int? v) => value = v));

        cut.FindAll(".ht-stepper button")[0].Click();
        Assert.Equal(0, value);
        cut.Find("input").Change("1000");
        Assert.Equal(1000, value);
        cut.Find("input").Change("");
        Assert.Null(value);
    }

    [Fact]
    public void DayPicker_has_the_seven_days_from_Monday_and_toggles_one()
    {
        IReadOnlySet<DayOfWeek>? days = null;
        var cut = Render<DayPicker>(p => p
            .Add(x => x.Days, new HashSet<DayOfWeek> { DayOfWeek.Monday })
            .Add(x => x.DaysChanged, (IReadOnlySet<DayOfWeek> d) => days = d));

        Assert.Equal("Days", cut.Find(".ht-days[role=group]").GetAttribute("aria-label"));
        Assert.Equal(["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"], cut.FindAll("button.ht-day").Select(b => b.TextContent));
        Assert.Equal("true", cut.FindAll("button.ht-day")[0].GetAttribute("aria-pressed"));
        Assert.Equal("false", cut.FindAll("button.ht-day")[6].GetAttribute("aria-pressed"));

        cut.FindAll("button.ht-day")[6].Click();
        Assert.Equal(new HashSet<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Sunday }, days);
        cut.FindAll("button.ht-day")[0].Click();
        Assert.Equal(new HashSet<DayOfWeek>(), days);
    }
}
