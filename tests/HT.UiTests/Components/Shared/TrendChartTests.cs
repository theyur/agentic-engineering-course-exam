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

    // Slice 7 (T069; FR-016, HP-006; design/components/TrendChart.md, "Slice 7 state"): marked bars.

    private static TrendBar Marked(TrendBar bar, TrendMark mark) => bar with { Mark = mark };

    /// <summary>
    /// Spec US3-AS4's trend, "Meditation, 10 min daily" from Friday 6 to Thursday 19 November 2026: 10 minutes on 6 to 8,
    /// 10, 12 and 16 November; Monday 9 Excused and 13 to 15 November Paused, all four without a value; 11 and 17 to 19
    /// November with no record.
    /// </summary>
    private static TrendView Us3Trend()
    {
        var first = new DateOnly(2026, 11, 6);
        var bars = Enumerable.Range(0, 14).Select(day =>
        {
            var date = first.AddDays(day);
            return date.Day switch
            {
                9 => Marked(Day(date, null, 10), TrendMark.Excused),
                13 or 14 or 15 => Marked(Day(date, null, 10), TrendMark.Paused),
                11 or 17 or 18 or 19 => Day(date, null, 10),
                _ => Day(date, 10, 10),
            };
        }).ToList();
        return View(TrendKind.Minutes, TrendSpan.Commitment, "Minutes per day", "Last 14 days", 10, bars);
    }

    [Fact]
    public void US3_AS4_an_excused_bar_and_paused_bars_are_drawn_dashed_full_height_with_their_titles()
    {
        var cut = RenderChart(Us3Trend());

        var bars = cut.FindAll(".ht-bars > i.ht-bar");
        Assert.Equal(14, bars.Count);
        var excused = bars[3];
        Assert.Equal(["ht-bar", "ht-bar--excused"], excused.ClassList.ToArray());
        Assert.Equal("Mon 9 Nov: Excused", excused.GetAttribute("title"));
        Assert.Null(excused.GetAttribute("style"));
        Assert.Equal(3, cut.FindAll(".ht-bars > i.ht-bar--paused").Count);
        Assert.All(bars.Skip(7).Take(3), bar => Assert.Equal(["ht-bar", "ht-bar--paused"], bar.ClassList.ToArray()));
        Assert.All(bars.Skip(7).Take(3), bar => Assert.Null(bar.GetAttribute("style")));
        Assert.Equal(["Fri 13 Nov: Paused", "Sat 14 Nov: Paused", "Sun 15 Nov: Paused"], bars.Skip(7).Take(3).Select(b => b.GetAttribute("title")));
        Assert.Single(cut.FindAll(".ht-bars > i.ht-bar--excused"));
    }

    [Fact]
    public void HP_006_marked_bars_stay_apart_from_bars_with_no_record_in_their_title_and_the_charts_label()
    {
        var cut = RenderChart(Us3Trend());

        var empty = cut.FindAll(".ht-bars > i.ht-bar--empty");
        Assert.Equal(4, empty.Count);
        Assert.Equal(["Wed 11 Nov: no record", "Tue 17 Nov: no record", "Wed 18 Nov: no record", "Thu 19 Nov: no record"], empty.Select(b => b.GetAttribute("title")));
        Assert.Empty(cut.FindAll(".ht-bar--empty.ht-bar--paused, .ht-bar--empty.ht-bar--excused"));
        var chart = cut.Find("section.ht-card > div.ht-bars");
        Assert.Equal("Minutes per day, last 14 days; target 10; 1 excused, 3 paused", chart.GetAttribute("aria-label"));
        var line = cut.Find(".ht-bars > .ht-bars__target");
        Assert.Equal("Target 10 min", line.TextContent);
        Assert.Equal("bottom:100%", line.GetAttribute("style"));
    }

    [Fact]
    public void FR_016_a_marked_week_reads_week_of_and_its_mark_and_the_line_stays_at_the_latest_unmarked_bars_target()
    {
        // Days done per week: 3 of 3 in the week of 14 September, the weeks of 21 and 28 September all exempt (Paused,
        // then Excused; target 0), 2 of 2 in the week of 5 October. The line is at the latest unmarked bar's target, 2.
        var view = View(TrendKind.RecordCount, TrendSpan.Week, "Days done per week", "Last 12 weeks", 2,
            [
                Week(new DateOnly(2026, 9, 14), 3, 3), Marked(Week(new DateOnly(2026, 9, 21), null, 0), TrendMark.Paused),
                Marked(Week(new DateOnly(2026, 9, 28), null, 0), TrendMark.Excused), Week(new DateOnly(2026, 10, 5), 2, 2),
            ]);

        var cut = RenderChart(view);

        var bars = cut.FindAll(".ht-bars > i.ht-bar");
        Assert.Equal(["week of 14 Sep: 3 of 3", "week of 21 Sep: Paused", "week of 28 Sep: Excused", "week of 5 Oct: 2 of 2"], bars.Select(b => b.GetAttribute("title")));
        Assert.Equal(["height:100%", null, null, "height:67%"], bars.Select(b => b.GetAttribute("style")));
        Assert.Contains("ht-bar--paused", bars[1].ClassList);
        Assert.Contains("ht-bar--excused", bars[2].ClassList);
        var line = cut.Find(".ht-bars__target");
        Assert.Equal("Target 2", line.TextContent);
        Assert.Equal("bottom:67%", line.GetAttribute("style"));
        Assert.Equal(
            "Days done per week, last 12 weeks; target 2; 1 excused, 1 paused; week of 14 Sep: 3 of 3",
            cut.Find(".ht-bars").GetAttribute("aria-label"));
    }

    [Fact]
    public void FR_016_an_unmarked_trend_has_no_marked_bar_and_slice_6s_label()
    {
        var cut = RenderChart(FourteenDays());

        Assert.Empty(cut.FindAll(".ht-bar--paused, .ht-bar--excused"));
        Assert.Equal("Minutes per day, last 14 days; target 20", cut.Find(".ht-bars").GetAttribute("aria-label"));
    }
}
