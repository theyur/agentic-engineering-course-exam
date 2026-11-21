using Bunit;
using HT.Application.Reports;
using HT.Domain.Habits;
using HT.Domain.Reports;
using HT.Ui.Components.Shared;

namespace HT.UiTests.Components.Shared;

/// <summary>
/// TrendChart (HP-001, HP-006; design/components/TrendChart.md, "Slice 6 state"): the card's title and sub line, one
/// ht-bar per bar in order with its height and its title, an empty bar for a bar with no record, the dashed target
/// line with its text, the date labels, and role="img" with the view's label. The views are built here with the texts
/// <see cref="ReportText"/> gives them, as the goal report does.
/// </summary>
public sealed class TrendChartTests : BunitContext
{
    private static TrendView View(TrendKind kind, TrendSpan span, string title, string subLine, int target, IReadOnlyList<TrendBar> bars)
    {
        var view = new TrendView
        {
            Kind = kind,
            Span = span,
            Measurement = kind == TrendKind.Minutes ? Measurement.Duration : Measurement.YesNo,
            Bars = bars,
            Target = target,
            Title = title,
            SubLine = subLine,
            TargetLine = ReportText.TargetLine(kind, target),
            Label = string.Empty,
        };
        return view with { Label = ReportText.TrendLabel(view) };
    }

    private static TrendBar Day(DateOnly date, int? value, int target) => new(date, date, value, target);

    private static TrendBar Week(DateOnly monday, int? value, int target) => new(monday, monday.AddDays(6), value, target);

    private IRenderedComponent<TrendChart> RenderChart(TrendView view) => Render<TrendChart>(p => p.Add(x => x.Trend, view));

    /// <summary>
    /// "Reading, 20 min daily" over 14 days from Thursday 5 November 2026: 20 minutes on each, except 25 on the 6th, none
    /// on Saturday 7 and 40 on Wednesday 18; the largest is 40, so 20 minutes is 50%.
    /// </summary>
    private static TrendView FourteenDays()
    {
        var first = new DateOnly(2026, 11, 5);
        var bars = Enumerable.Range(0, 14).Select(day =>
        {
            var date = first.AddDays(day);
            int? value = date.Day switch { 6 => 25, 7 => null, 18 => 40, _ => 20 };
            return Day(date, value, 20);
        }).ToList();
        return View(TrendKind.Minutes, TrendSpan.Commitment, "Minutes per day", "Last 14 days", 20, bars);
    }

    [Fact]
    public void HP_001_14_bars_in_order_with_their_heights_titles_and_date_labels()
    {
        var cut = RenderChart(FourteenDays());

        Assert.Equal("Minutes per day", cut.Find("section.ht-card > h2.ht-card__title").TextContent);
        Assert.Equal("Last 14 days", cut.Find("section.ht-card > p.ht-card__sub").TextContent);
        var bars = cut.FindAll(".ht-bars > i.ht-bar");
        Assert.Equal(14, bars.Count);
        Assert.Equal("Thu 5 Nov: 20 of 20 min", bars[0].GetAttribute("title"));
        Assert.Equal("height:50%", bars[0].GetAttribute("style"));
        Assert.Equal("Fri 6 Nov: 25 of 20 min", bars[1].GetAttribute("title"));
        Assert.Equal("height:63%", bars[1].GetAttribute("style"));
        Assert.Equal(
            ["5 Nov", "6 Nov", "7 Nov", "8 Nov", "9 Nov", "10 Nov", "11 Nov", "12 Nov", "13 Nov", "14 Nov", "15 Nov", "16 Nov", "17 Nov", "18 Nov"],
            cut.FindAll("section.ht-card > .ht-bars__labels > span").Select(s => s.TextContent));
    }

    [Fact]
    public void FR_006_a_bar_with_no_record_is_drawn_empty_never_as_a_zero_height_bar()
    {
        var cut = RenderChart(FourteenDays());

        var empty = Assert.Single(cut.FindAll(".ht-bars > i.ht-bar--empty"));
        Assert.Equal(["ht-bar", "ht-bar--empty"], empty.ClassList.ToArray());
        Assert.Equal("Sat 7 Nov: no record", empty.GetAttribute("title"));
        Assert.Null(empty.GetAttribute("style"));
        Assert.Same(empty, cut.FindAll(".ht-bars > i.ht-bar")[2]);
    }

    [Fact]
    public void FR_006_a_bar_with_a_record_of_0_has_its_value_and_a_zero_height()
    {
        var view = View(TrendKind.Count, TrendSpan.Commitment, "Pages per day", "Last 14 days", 5,
            [Day(new DateOnly(2026, 11, 2), 0, 5), Day(new DateOnly(2026, 11, 3), 5, 5)]);

        var bars = RenderChart(view).FindAll(".ht-bars > i.ht-bar");

        Assert.Equal(["height:0%", "height:100%"], bars.Select(b => b.GetAttribute("style")));
        Assert.Equal(["Mon 2 Nov: 0 of 5", "Tue 3 Nov: 5 of 5"], bars.Select(b => b.GetAttribute("title")));
        Assert.Empty(RenderChart(view).FindAll(".ht-bar--empty"));
    }

    [Fact]
    public void HP_001_the_target_line_reads_Target_n_min_for_minutes_at_its_share_of_the_largest_value()
    {
        var cut = RenderChart(FourteenDays());

        var line = cut.Find(".ht-bars > .ht-bars__target");
        Assert.Equal("Target 20 min", line.TextContent);
        Assert.Equal("bottom:50%", line.GetAttribute("style"));
    }

    [Fact]
    public void Edge_case_a_bar_above_the_target_is_drawn_as_recorded_and_tallest()
    {
        var cut = RenderChart(FourteenDays());

        var tallest = cut.FindAll(".ht-bars > i.ht-bar")[13];
        Assert.Equal("height:100%", tallest.GetAttribute("style"));
        Assert.Equal("Wed 18 Nov: 40 of 20 min", tallest.GetAttribute("title"));
    }

    [Fact]
    public void HP_001_12_weekly_bars_in_order_with_the_target_line_Target_n_for_counts()
    {
        // Days done per week from the week of 31 August to the week of 16 November 2026: 2 of 2 in the first week, then
        // 3, 2 and 1 in turn against 3.
        var mondays = Enumerable.Range(0, 12).Select(week => new DateOnly(2026, 8, 31).AddDays(7 * week)).ToList();
        var bars = mondays.Select((monday, index) => index == 0 ? Week(monday, 2, 2) : Week(monday, 3 - (index % 3), 3)).ToList();
        var view = View(TrendKind.RecordCount, TrendSpan.Week, "Days done per week", "Last 12 weeks", 3, bars);

        var cut = RenderChart(view);

        var drawn = cut.FindAll(".ht-bars > i.ht-bar");
        Assert.Equal(12, drawn.Count);
        Assert.Equal(bars.Select(bar => ReportText.BarTitle(view, bar)), drawn.Select(b => b.GetAttribute("title")));
        Assert.Equal("week of 31 Aug: 2 of 2", drawn[0].GetAttribute("title"));
        Assert.Equal("week of 7 Sep: 2 of 3", drawn[1].GetAttribute("title"));
        Assert.Equal("height:67%", drawn[0].GetAttribute("style"));
        Assert.Equal(
            ["31 Aug", "7 Sep", "14 Sep", "21 Sep", "28 Sep", "5 Oct", "12 Oct", "19 Oct", "26 Oct", "2 Nov", "9 Nov", "16 Nov"],
            cut.FindAll(".ht-bars__labels > span").Select(s => s.TextContent));
        var line = cut.Find(".ht-bars__target");
        Assert.Equal("Target 3", line.TextContent);
        Assert.Equal("bottom:100%", line.GetAttribute("style"));
    }

    [Fact]
    public void HP_006_the_chart_is_an_image_named_by_the_views_label()
    {
        var view = View(TrendKind.RecordCount, TrendSpan.Week, "Days done per week", "Last 12 weeks", 3,
            [Week(new DateOnly(2026, 11, 2), 2, 2), Week(new DateOnly(2026, 11, 9), 1, 3)]);

        var chart = RenderChart(view).Find("section.ht-card > div.ht-bars");

        Assert.Equal("img", chart.GetAttribute("role"));
        Assert.Equal("Days done per week, last 12 weeks; target 3; week of 2 Nov: 2 of 2", chart.GetAttribute("aria-label"));
        Assert.Equal(view.Label, chart.GetAttribute("aria-label"));
    }
}
