using HT.Domain.Evaluation;
using HT.Domain.Exemptions;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using HT.Domain.Reports;
using static HT.UnitTests.Domain.Goals.GoalSamples;

namespace HT.UnitTests.Domain.Reports;

/// <summary>
/// Slice 8 research R-07, "The trend": a settings change within one attempt can put entries of two measurements in one
/// trend. Every bar and week is drawn in the trend's kind; an entry of the other measurement contributes only its
/// result. Monday 9 and 16 November 2026 are Mondays. The instants of the entries do not matter to the builder.
/// </summary>
public class TrendBuilderMeasurementChangeTests
{
    private static readonly RecordValue Yes = RecordValue.FromAnswer(YesNoAnswer.Yes);
    private static readonly RecordValue No = RecordValue.FromAnswer(YesNoAnswer.No);

    private static RecordValue Amount(int amount) => RecordValue.FromAmount(amount);

    private static DateTimeOffset Midnight(DateOnly date) => new(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

    private static CalendarEntry Day(DateOnly date) => new(date, date, Midnight(date), Midnight(date.AddDays(1)));

    private static (CalendarEntry Entry, GoalTarget Target, IReadOnlyList<RecordValue> Records, EvaluationState? Exemption) Item(
        DateOnly date, GoalTarget target, EvaluationState? exemption, params RecordValue[] records) => (Day(date), target, records, exemption);

    private static Trend Build(GoalTarget latest, params (CalendarEntry Entry, GoalTarget Target, IReadOnlyList<RecordValue> Records, EvaluationState? Exemption)[] begun) =>
        TrendBuilder.Of(latest, begun, [], GoalExemptions.None);

    private static GoalTarget Minutes20Daily => Target(DurationDays(20, DurationUnit.Minutes, BuildOrReduce.Build, AllDays));

    private static GoalTarget YesNoDaily => Target(YesNoDays(BuildOrReduce.Build, AllDays));

    [Fact]
    public void Duration_to_yes_no_a_week_counts_one_Yes_per_Met_duration_commitment_never_its_minutes()
    {
        // 20 min until Wednesday 11 November, yes/no from Thursday 12 November.
        var duration = Minutes20Daily;
        var yesNo = YesNoDaily;

        var trend = Build(
            yesNo,
            Item(Date(2026, 11, 9), duration, null, Amount(25)), // Met
            Item(Date(2026, 11, 10), duration, null, Amount(12)), // Failed
            Item(Date(2026, 11, 11), duration, null), // no record
            Item(Date(2026, 11, 12), yesNo, null, Yes),
            Item(Date(2026, 11, 13), yesNo, null, No),
            Item(Date(2026, 11, 16), duration, null, Amount(75)));

        Assert.Equal(TrendKind.RecordCount, trend.Kind);
        Assert.Equal(TrendSpan.Week, trend.Span);
        Assert.Equal(
            [
                new TrendBar(Date(2026, 11, 9), Date(2026, 11, 15), 2, 5),
                new TrendBar(Date(2026, 11, 16), Date(2026, 11, 22), 1, 1),
            ],
            trend.Bars);
        Assert.Equal(1, trend.Target);
    }

    [Fact]
    public void Duration_to_yes_no_a_week_of_only_unmet_duration_commitments_has_the_value_0()
    {
        var trend = Build(YesNoDaily, Item(Date(2026, 11, 9), Minutes20Daily, null, Amount(5)));

        Assert.Equal(0, trend.Bars.Single().Value);
    }

    [Fact]
    public void Yes_no_to_duration_the_earlier_yes_no_commitments_have_no_value_and_no_line()
    {
        var yesNo = YesNoDaily;
        var duration = Minutes20Daily;

        var trend = Build(
            duration,
            Item(Date(2026, 11, 9), yesNo, null, Yes),
            Item(Date(2026, 11, 10), yesNo, null, No),
            Item(Date(2026, 11, 11), duration, null, Amount(30)),
            Item(Date(2026, 11, 12), duration, null));

        Assert.Equal(TrendKind.Minutes, trend.Kind);
        Assert.Equal(TrendSpan.Commitment, trend.Span);
        Assert.Equal(new int?[] { null, null, 30, null }, trend.Bars.Select(bar => bar.Value));
        Assert.Equal(20, trend.Target);
    }

    [Fact]
    public void Count_to_duration_per_day_the_earlier_bars_have_no_value_and_the_line_takes_only_duration_bars()
    {
        // 12 pages a day until Wednesday 11 November, 20 min a day from Thursday 12 November.
        var count = Target(CountTimes(12, "pages", 1, SchedulePeriod.Day));
        var duration = Target(DurationTimes(20, DurationUnit.Minutes, 1, SchedulePeriod.Day));

        var trend = Build(
            duration,
            Item(Date(2026, 11, 10), count, null, Amount(12)),
            Item(Date(2026, 11, 11), count, null, Amount(20)),
            Item(Date(2026, 11, 12), duration, null, Amount(25)),
            Item(Date(2026, 11, 13), duration, null));

        Assert.Equal(TrendSpan.Day, trend.Span);
        Assert.Equal(new int?[] { null, null, 1, null }, trend.Bars.Select(bar => bar.Value));
        Assert.Equal(1, trend.Target);
    }

    [Fact]
    public void The_line_skips_a_later_bar_of_the_other_measurement()
    {
        // A Reduce per-day trend whose latest kept bar has another measurement: the line is the last bar of the trend's own.
        var minutes = Target(DurationTimes(30, DurationUnit.Minutes, 1, SchedulePeriod.Day, BuildOrReduce.Reduce));
        var count = Target(CountTimes(3, "cups", 1, SchedulePeriod.Day, BuildOrReduce.Reduce));

        var trend = Build(
            minutes,
            Item(Date(2026, 11, 10), minutes, null, Amount(10)),
            Item(Date(2026, 11, 11), count, null, Amount(2)));

        Assert.Equal(TrendKind.Minutes, trend.Kind);
        Assert.Equal(new int?[] { 10, null }, trend.Bars.Select(bar => bar.Value));
        Assert.Equal(30, trend.Target);
    }

    [Fact]
    public void A_trend_with_only_bars_of_the_other_measurement_has_no_line()
    {
        var count = Target(CountTimes(12, "pages", 1, SchedulePeriod.Day));
        var duration = Target(DurationTimes(20, DurationUnit.Minutes, 1, SchedulePeriod.Day));

        var trend = Build(duration, Item(Date(2026, 11, 10), count, null, Amount(12)));

        Assert.Null(trend.Bars.Single().Value);
        Assert.Equal(0, trend.Target);
    }

    [Fact]
    public void A_bar_of_the_other_measurement_keeps_its_exemption_mark()
    {
        var count = Target(CountTimes(12, "pages", 1, SchedulePeriod.Day));
        var duration = Target(DurationTimes(20, DurationUnit.Minutes, 1, SchedulePeriod.Day));

        var trend = Build(
            duration,
            Item(Date(2026, 11, 10), count, EvaluationState.Paused, Amount(12)),
            Item(Date(2026, 11, 11), count, EvaluationState.Excused),
            Item(Date(2026, 11, 12), duration, null, Amount(20)));

        Assert.Equal(new TrendMark?[] { TrendMark.Paused, TrendMark.Excused, null }, trend.Bars.Select(bar => bar.Mark));
        Assert.Equal(new int?[] { null, null, 1 }, trend.Bars.Select(bar => bar.Value));
    }

    [Fact]
    public void An_exempt_commitment_of_the_other_measurement_counts_no_Yes_in_its_week()
    {
        var trend = Build(
            YesNoDaily,
            Item(Date(2026, 11, 9), Minutes20Daily, EvaluationState.Excused, Amount(25)),
            Item(Date(2026, 11, 10), YesNoDaily, null, Yes));

        Assert.Equal(new TrendBar(Date(2026, 11, 9), Date(2026, 11, 15), 1, 1), trend.Bars.Single());
    }
}
