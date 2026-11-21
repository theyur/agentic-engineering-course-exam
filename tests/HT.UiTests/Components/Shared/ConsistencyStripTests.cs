using Bunit;
using HT.Domain.Evaluation;
using HT.Domain.Reports;
using HT.Ui.Components.Shared;

namespace HT.UiTests.Components.Shared;

/// <summary>
/// ConsistencyStrip (HP-001, HP-002, HP-006, design/components/ConsistencyStrip slice-6 and slice-7 states): one cell
/// per ended result, oldest first, with the design's cell classes so no state relies on colour alone; role="img" with
/// the counting label from ReportText; the legend of the six states (Met, Failed, Missed, Excused, Paused, Pending) on
/// the full strip, none on the mini strip.
/// </summary>
public sealed class ConsistencyStripTests : BunitContext
{
    private static readonly EvaluationState[] Order =
        [EvaluationState.Met, EvaluationState.Failed, EvaluationState.Missed, EvaluationState.Excused, EvaluationState.Paused, EvaluationState.Pending];

    private static EvaluationState[] Cycle(int count) => Enumerable.Range(0, count).Select(i => Order[i % Order.Length]).ToArray();

    private static string CellClassOf(EvaluationState state) => state switch
    {
        EvaluationState.Met => "ht-cell--met",
        EvaluationState.Failed => "ht-cell--failed",
        EvaluationState.Missed => "ht-cell--missed",
        EvaluationState.Excused => "ht-cell--excused",
        EvaluationState.Paused => "ht-cell--paused",
        _ => "ht-cell--pending",
    };

    [Theory]
    [InlineData(30, false)]
    [InlineData(15, true)]
    public void Each_state_has_one_cell_in_order_with_its_design_class(int count, bool mini)
    {
        var states = Cycle(count);

        var cut = Render<ConsistencyStrip>(p => p
            .Add(x => x.States, states)
            .Add(x => x.Unit, StreakUnit.Commitments)
            .Add(x => x.Mini, mini));

        var cells = cut.FindAll("div.ht-strip > i");
        Assert.Equal(count, cells.Count);
        for (var i = 0; i < count; i++)
        {
            Assert.Equal(["ht-strip__cell", CellClassOf(states[i])], cells[i].ClassList.ToArray());
        }
    }

    [Fact]
    public void The_full_strip_has_the_counting_label_and_the_legend_of_the_six_states()
    {
        EvaluationState[] states =
        [
            .. Enumerable.Repeat(EvaluationState.Met, 9), EvaluationState.Failed, .. Enumerable.Repeat(EvaluationState.Met, 8),
            EvaluationState.Missed, .. Enumerable.Repeat(EvaluationState.Met, 7), EvaluationState.Pending, EvaluationState.Pending,
        ];

        var cut = Render<ConsistencyStrip>(p => p
            .Add(x => x.States, states)
            .Add(x => x.Unit, StreakUnit.Commitments));

        var strip = cut.Find("div.ht-strip");
        Assert.Equal(["ht-strip"], strip.ClassList.ToArray());
        Assert.Equal("img", strip.GetAttribute("role"));
        Assert.Equal("Last 28 commitments: 24 met, 1 failed, 1 missed, 2 pending", strip.GetAttribute("aria-label"));
        var entries = cut.FindAll(".ht-legend > span");
        Assert.Equal(["Met", "Failed", "Missed", "Excused", "Paused", "Pending"], entries.Select(e => e.TextContent).ToArray());
        Assert.Equal(
            ["ht-cell--met", "ht-cell--failed", "ht-cell--missed", "ht-cell--excused", "ht-cell--paused", "ht-cell--pending"],
            entries.Select(e => e.QuerySelector("i.ht-strip__cell")!.ClassList).Select(c => c[c.Length - 1]).ToArray());

        // Slice 7: the legend is a key, not a count, so Excused and Paused are listed with no such cell in the strip.
        Assert.Contains("Excused", cut.Find(".ht-legend").TextContent, StringComparison.Ordinal);
        Assert.Contains("Paused", cut.Find(".ht-legend").TextContent, StringComparison.Ordinal);
        Assert.Empty(cut.FindAll("div.ht-strip > .ht-cell--excused, div.ht-strip > .ht-cell--paused"));
    }

    [Fact]
    public void Slice_7_Excused_and_Paused_cells_are_drawn_and_counted_in_the_label()
    {
        // Spec US3-AS3: Met × 7, Excused, Met, Missed, Met, Paused × 3, Met, Pending × 2.
        EvaluationState[] states =
        [
            .. Enumerable.Repeat(EvaluationState.Met, 7), EvaluationState.Excused, EvaluationState.Met, EvaluationState.Missed,
            EvaluationState.Met, .. Enumerable.Repeat(EvaluationState.Paused, 3), EvaluationState.Met, EvaluationState.Pending,
            EvaluationState.Pending,
        ];

        var cut = Render<ConsistencyStrip>(p => p
            .Add(x => x.States, states)
            .Add(x => x.Unit, StreakUnit.Commitments));

        var strip = cut.Find("div.ht-strip");
        Assert.Equal("Last 17 commitments: 10 met, 1 missed, 1 excused, 3 paused, 2 pending", strip.GetAttribute("aria-label"));
        var cells = cut.FindAll("div.ht-strip > i");
        Assert.Equal(states.Select(CellClassOf), cells.Select(c => c.ClassList[c.ClassList.Length - 1]));
        Assert.Single(cut.FindAll("div.ht-strip > i.ht-cell--excused"));
        Assert.Equal(3, cut.FindAll("div.ht-strip > i.ht-cell--paused").Count);
    }

    [Fact]
    public void Slice_7_a_mini_strip_of_only_exempt_results_has_their_cells_and_no_legend()
    {
        EvaluationState[] states = [EvaluationState.Excused, .. Enumerable.Repeat(EvaluationState.Paused, 6)];

        var cut = Render<ConsistencyStrip>(p => p
            .Add(x => x.States, states)
            .Add(x => x.Unit, StreakUnit.Commitments)
            .Add(x => x.Mini, true));

        Assert.Equal("Last 7 commitments: 1 excused, 6 paused", cut.Find("div.ht-strip").GetAttribute("aria-label"));
        Assert.Equal(7, cut.FindAll("div.ht-strip > i.ht-cell--excused, div.ht-strip > i.ht-cell--paused").Count);
        Assert.Empty(cut.FindAll(".ht-legend"));
    }

    [Fact]
    public void The_mini_strip_has_the_mini_class_the_same_label_and_no_legend()
    {
        EvaluationState[] states =
        [
            .. Enumerable.Repeat(EvaluationState.Met, 5), EvaluationState.Missed, .. Enumerable.Repeat(EvaluationState.Met, 7),
            EvaluationState.Pending, EvaluationState.Pending,
        ];

        var cut = Render<ConsistencyStrip>(p => p
            .Add(x => x.States, states)
            .Add(x => x.Unit, StreakUnit.Commitments)
            .Add(x => x.Mini, true));

        var strip = cut.Find("div.ht-strip");
        Assert.Equal(["ht-strip", "ht-strip--mini"], strip.ClassList.ToArray());
        Assert.Equal("img", strip.GetAttribute("role"));
        Assert.Equal("Last 15 commitments: 12 met, 1 missed, 2 pending", strip.GetAttribute("aria-label"));
        Assert.Empty(cut.FindAll(".ht-legend"));
    }

    [Fact]
    public void A_strip_with_no_result_has_no_cell_and_the_label_Last_0()
    {
        var cut = Render<ConsistencyStrip>(p => p
            .Add(x => x.States, Array.Empty<EvaluationState>())
            .Add(x => x.Unit, StreakUnit.Weeks)
            .Add(x => x.Mini, true));

        Assert.Empty(cut.FindAll(".ht-strip__cell"));
        Assert.Equal("Last 0 weeks", cut.Find("div.ht-strip").GetAttribute("aria-label"));
    }
}
