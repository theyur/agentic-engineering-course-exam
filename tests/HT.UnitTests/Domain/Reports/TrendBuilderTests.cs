using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using HT.Domain.Reports;
using static HT.UnitTests.Domain.Goals.GoalSamples;

namespace HT.UnitTests.Domain.Reports;

/// <summary>
/// FR-006, US4-AS3 to AS7, research R-05 and R-14: one case per row of R-05's table, null versus 0, values above the
/// target, minutes for hours, the kinds, the 12 and 14 caps, the current week of a yes/no goal, a smaller first week
/// and unrecorded Reduce periods. Monday 2, 9 and 16 November 2026 are Mondays. The instants of the entries do not
/// matter to the builder; they are each date's midnight in UTC.
/// </summary>
public class TrendBuilderTests
{
    private static readonly RecordValue Yes = RecordValue.FromAnswer(YesNoAnswer.Yes);
    private static readonly RecordValue No = RecordValue.FromAnswer(YesNoAnswer.No);

    private static RecordValue Amount(int amount) => RecordValue.FromAmount(amount);

    private static DateTimeOffset Midnight(DateOnly date) => new(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

    private static CalendarEntry Day(DateOnly date) => new(date, date, Midnight(date), Midnight(date.AddDays(1)));

    private static CalendarEntry Week(DateOnly monday) => new(monday, monday.AddDays(6), Midnight(monday), Midnight(monday.AddDays(7)));

    private static (CalendarEntry Entry, GoalTarget Target, IReadOnlyList<RecordValue> Records) Item(
        CalendarEntry entry, GoalTarget target, params RecordValue[] records) => (entry, target, records);

    private static Trend Build(GoalTarget target, params (CalendarEntry Entry, GoalTarget Target, IReadOnlyList<RecordValue> Records)[] begun) =>
        TrendBuilder.Of(target, begun, []);

    private static GoalTarget Reading20Daily => Target(DurationDays(20, DurationUnit.Minutes, BuildOrReduce.Build, AllDays));

    // Row: commitment-based count or duration.

    [Fact]
    public void US4_AS3_a_daily_duration_goal_has_one_bar_per_commitment_with_its_minutes_against_20()
    {
        var target = Reading20Daily;

        var trend = Build(
            target,
            Item(Day(Date(2026, 11, 12)), target, Amount(20)),
            Item(Day(Date(2026, 11, 13)), target, Amount(12)),
            Item(Day(Date(2026, 11, 14)), target),
            Item(Day(Date(2026, 11, 15)), target, Amount(25)));

        Assert.Equal(TrendKind.Minutes, trend.Kind);
        Assert.Equal(TrendSpan.Commitment, trend.Span);
        Assert.Equal(14, trend.MaxBars);
        Assert.Equal(20, trend.Target);
        Assert.Equal(
            [
                new TrendBar(Date(2026, 11, 12), Date(2026, 11, 12), 20, 20),
                new TrendBar(Date(2026, 11, 13), Date(2026, 11, 13), 12, 20),
                new TrendBar(Date(2026, 11, 14), Date(2026, 11, 14), null, 20),
                new TrendBar(Date(2026, 11, 15), Date(2026, 11, 15), 25, 20),
            ],
            trend.Bars);
    }

    [Fact]
    public void US4_AS6_a_commitment_recorded_0_has_the_value_0_and_one_without_a_record_has_none()
    {
        var target = Target(CountDays(5, "pages", BuildOrReduce.Build, MonWedFri));

        var trend = Build(target, Item(Day(Date(2026, 11, 16)), target, Amount(0)), Item(Day(Date(2026, 11, 18)), target));

        Assert.Equal(new int?[] { 0, null }, trend.Bars.Select(bar => bar.Value));
    }

    [Fact]
    public void Edge_case_an_amount_above_the_target_is_given_as_recorded()
    {
        var target = Reading20Daily;

        var trend = Build(target, Item(Day(Date(2026, 11, 16)), target, Amount(45)));

        Assert.Equal(new TrendBar(Date(2026, 11, 16), Date(2026, 11, 16), 45, 20), Assert.Single(trend.Bars));
    }

    [Fact]
    public void Edge_case_a_goal_in_hours_has_its_values_and_target_in_minutes()
    {
        var target = Target(DurationDays(1, DurationUnit.Hours, BuildOrReduce.Build, MonWedFri));

        var trend = Build(target, Item(Day(Date(2026, 11, 16)), target, Amount(75)), Item(Day(Date(2026, 11, 18)), target, Amount(30)));

        Assert.Equal(TrendKind.Minutes, trend.Kind);
        Assert.Equal(60, trend.Target);
        Assert.Equal(new int?[] { 75, 30 }, trend.Bars.Select(bar => bar.Value));
        Assert.All(trend.Bars, bar => Assert.Equal(60, bar.Target));
    }

    [Theory]
    [InlineData(BuildOrReduce.Build)]
    [InlineData(BuildOrReduce.Reduce)]
    public void A_commitment_based_count_goal_gives_Count(BuildOrReduce side)
    {
        var target = Target(CountDays(5, "pages", side, MonWedFri));

        var trend = Build(target, Item(Day(Date(2026, 11, 16)), target, Amount(3)));

        Assert.Equal(TrendKind.Count, trend.Kind);
        Assert.Equal(TrendSpan.Commitment, trend.Span);
        Assert.Equal(new TrendBar(Date(2026, 11, 16), Date(2026, 11, 16), 3, 5), Assert.Single(trend.Bars));
    }

    [Fact]
    public void Each_bar_has_the_target_of_its_own_entry_and_the_trend_the_latest_bar_s()
    {
        var older = Target(CountDays(5, "pages", BuildOrReduce.Build, MonWedFri));
        var later = Target(CountDays(8, "pages", BuildOrReduce.Build, MonWedFri));

        var trend = Build(later, Item(Day(Date(2026, 11, 16)), older, Amount(5)), Item(Day(Date(2026, 11, 18)), later, Amount(6)));

        Assert.Equal([5, 8], trend.Bars.Select(bar => bar.Target));
        Assert.Equal(8, trend.Target);
    }

    // Row: per-week.

    [Fact]
    public void US4_AS4_a_per_week_goal_has_one_bar_per_week_with_its_progress_against_3()
    {
        var target = PerWeek();

        var trend = Build(
            target,
            Item(Week(Date(2026, 11, 2)), target, Yes, Yes, No),
            Item(Week(Date(2026, 11, 9)), target),
            Item(Week(Date(2026, 11, 16)), target, No));

        Assert.Equal(TrendKind.RecordCount, trend.Kind);
        Assert.Equal(TrendSpan.Week, trend.Span);
        Assert.Equal(12, trend.MaxBars);
        Assert.Equal(3, trend.Target);
        Assert.Equal(
            [
                new TrendBar(Date(2026, 11, 2), Date(2026, 11, 8), 2, 3),
                new TrendBar(Date(2026, 11, 9), Date(2026, 11, 15), null, 3),
                new TrendBar(Date(2026, 11, 16), Date(2026, 11, 22), 0, 3),
            ],
            trend.Bars);
    }

    [Fact]
    public void US4_AS7_a_Build_duration_week_of_30_25_and_40_minutes_against_30_gives_2_against_3()
    {
        var target = Target(DurationTimes(30, DurationUnit.Minutes, 3, SchedulePeriod.Week));

        var trend = Build(target, Item(Week(Date(2026, 11, 9)), target, Amount(30), Amount(25), Amount(40)));

        Assert.Equal(TrendKind.RecordCount, trend.Kind);
        Assert.Equal(new TrendBar(Date(2026, 11, 9), Date(2026, 11, 15), 2, 3), Assert.Single(trend.Bars));
    }

    [Fact]
    public void A_Reduce_yes_no_week_counts_its_Yes_records_against_its_N()
    {
        var target = Target(YesNoTimes(2, SchedulePeriod.Week, BuildOrReduce.Reduce));

        var trend = Build(target, Item(Week(Date(2026, 11, 9)), target, Yes, No, Yes, Yes));

        Assert.Equal(TrendKind.RecordCount, trend.Kind);
        Assert.Equal(new TrendBar(Date(2026, 11, 9), Date(2026, 11, 15), 3, 2), Assert.Single(trend.Bars));
    }

    [Fact]
    public void US4_AS6_an_unrecorded_Reduce_week_has_no_value()
    {
        var target = Target(CountTimes(10, "pages", 1, SchedulePeriod.Week, BuildOrReduce.Reduce));

        var trend = Build(target, Item(Week(Date(2026, 11, 2)), target, Amount(4), Amount(7)), Item(Week(Date(2026, 11, 9)), target));

        Assert.Equal(TrendKind.Count, trend.Kind);
        Assert.Equal(
            [new TrendBar(Date(2026, 11, 2), Date(2026, 11, 8), 11, 10), new TrendBar(Date(2026, 11, 9), Date(2026, 11, 15), null, 10)],
            trend.Bars);
    }

    // Row: per-day.

    [Fact]
    public void A_per_day_Build_goal_has_one_bar_per_day_with_the_records_reaching_the_amount_against_N()
    {
        var target = Target(DurationTimes(20, DurationUnit.Minutes, 2, SchedulePeriod.Day));

        var trend = Build(
            target,
            Item(Day(Date(2026, 11, 16)), target, Amount(20), Amount(25)),
            Item(Day(Date(2026, 11, 17)), target, Amount(19)),
            Item(Day(Date(2026, 11, 18)), target));

        Assert.Equal(TrendKind.RecordCount, trend.Kind);
        Assert.Equal(TrendSpan.Day, trend.Span);
        Assert.Equal(14, trend.MaxBars);
        Assert.Equal(
            [
                new TrendBar(Date(2026, 11, 16), Date(2026, 11, 16), 2, 2),
                new TrendBar(Date(2026, 11, 17), Date(2026, 11, 17), 0, 2),
                new TrendBar(Date(2026, 11, 18), Date(2026, 11, 18), null, 2),
            ],
            trend.Bars);
    }

    [Fact]
    public void US4_AS6_a_Reduce_count_day_has_its_total_against_the_limit_and_an_unrecorded_day_no_value()
    {
        var target = Target(CountTimes(1, "sweets", 1, SchedulePeriod.Day, BuildOrReduce.Reduce));

        var trend = Build(
            target,
            Item(Day(Date(2026, 11, 16)), target, Amount(2)),
            Item(Day(Date(2026, 11, 17)), target),
            Item(Day(Date(2026, 11, 18)), target, Amount(0)));

        Assert.Equal(TrendKind.Count, trend.Kind);
        Assert.Equal(1, trend.Target);
        Assert.Equal(new int?[] { 2, null, 0 }, trend.Bars.Select(bar => bar.Value));
    }

    [Fact]
    public void A_Reduce_duration_day_in_hours_has_its_total_and_its_limit_in_minutes()
    {
        var target = Target(DurationTimes(2, DurationUnit.Hours, 1, SchedulePeriod.Day, BuildOrReduce.Reduce));

        var trend = Build(target, Item(Day(Date(2026, 11, 16)), target, Amount(90), Amount(40)));

        Assert.Equal(TrendKind.Minutes, trend.Kind);
        Assert.Equal(new TrendBar(Date(2026, 11, 16), Date(2026, 11, 16), 130, 120), Assert.Single(trend.Bars));
    }

    // Row: commitment-based yes/no.

    [Fact]
    public void US4_AS5_a_Mon_Wed_Fri_yes_no_goal_has_one_bar_per_week_with_its_Yes_records_against_its_commitments()
    {
        var target = OnDays(MonWedFri);

        var trend = Build(
            target,
            Item(Day(Date(2026, 11, 2)), target, Yes),
            Item(Day(Date(2026, 11, 4)), target, Yes),
            Item(Day(Date(2026, 11, 6)), target, No),
            Item(Day(Date(2026, 11, 9)), target),
            Item(Day(Date(2026, 11, 11)), target),
            Item(Day(Date(2026, 11, 13)), target));

        Assert.Equal(TrendKind.RecordCount, trend.Kind);
        Assert.Equal(TrendSpan.Week, trend.Span);
        Assert.Equal(12, trend.MaxBars);
        Assert.Equal(
            [new TrendBar(Date(2026, 11, 2), Date(2026, 11, 8), 2, 3), new TrendBar(Date(2026, 11, 9), Date(2026, 11, 15), null, 3)],
            trend.Bars);
    }

    [Fact]
    public void US4_AS7_a_Reduce_yes_no_week_with_one_Yes_and_two_No_gives_1_against_0()
    {
        var target = Target(YesNoDays(BuildOrReduce.Reduce, MonWedFri));

        var trend = Build(
            target,
            Item(Day(Date(2026, 11, 9)), target, Yes),
            Item(Day(Date(2026, 11, 11)), target, No),
            Item(Day(Date(2026, 11, 13)), target, No));

        Assert.Equal(new TrendBar(Date(2026, 11, 9), Date(2026, 11, 15), 1, 0), Assert.Single(trend.Bars));
        Assert.Equal(0, trend.Target);
    }

    [Fact]
    public void A_yes_no_week_recorded_only_No_has_the_value_0()
    {
        var target = OnDays(MonWedFri);

        var trend = Build(target, Item(Day(Date(2026, 11, 9)), target, No), Item(Day(Date(2026, 11, 11)), target));

        Assert.Equal(0, Assert.Single(trend.Bars).Value);
    }

    [Fact]
    public void US4_AS5_a_Tuesday_and_Thursday_goal_read_on_a_Monday_has_the_previous_week_as_its_latest_bar()
    {
        var target = OnDays(DayOfWeek.Tuesday, DayOfWeek.Thursday);
        (CalendarEntry Entry, GoalTarget Target, IReadOnlyList<RecordValue> Records)[] begun =
        [
            Item(Day(Date(2026, 11, 3)), target, Yes),
            Item(Day(Date(2026, 11, 5)), target, Yes),
            Item(Day(Date(2026, 11, 10)), target, Yes),
            Item(Day(Date(2026, 11, 12)), target),
        ];

        // Read on Monday 16 November: its Tuesday and Thursday are Upcoming.
        var trend = TrendBuilder.Of(target, begun, [Day(Date(2026, 11, 17)), Day(Date(2026, 11, 19))]);

        Assert.Equal(
            [new TrendBar(Date(2026, 11, 2), Date(2026, 11, 8), 2, 2), new TrendBar(Date(2026, 11, 9), Date(2026, 11, 15), 1, 2)],
            trend.Bars);
        Assert.Equal(2, trend.Target);
    }

    [Fact]
    public void AC_025_a_smaller_first_week_has_its_own_target_and_the_current_week_counts_its_Upcoming_commitments()
    {
        // Started on Wednesday 4 November, read on Monday 16 November: Wednesday 18 and Friday 20 are Upcoming.
        var target = OnDays(MonWedFri);
        (CalendarEntry Entry, GoalTarget Target, IReadOnlyList<RecordValue> Records)[] begun =
        [
            Item(Day(Date(2026, 11, 4)), target, Yes),
            Item(Day(Date(2026, 11, 6)), target, Yes),
            Item(Day(Date(2026, 11, 9)), target, Yes),
            Item(Day(Date(2026, 11, 11)), target, Yes),
            Item(Day(Date(2026, 11, 13)), target, No),
            Item(Day(Date(2026, 11, 16)), target, Yes),
        ];

        var trend = TrendBuilder.Of(target, begun, [Day(Date(2026, 11, 18)), Day(Date(2026, 11, 20))]);

        Assert.Equal(
            [
                new TrendBar(Date(2026, 11, 2), Date(2026, 11, 8), 2, 2),
                new TrendBar(Date(2026, 11, 9), Date(2026, 11, 15), 2, 3),
                new TrendBar(Date(2026, 11, 16), Date(2026, 11, 22), 1, 3),
            ],
            trend.Bars);
        Assert.Equal(3, trend.Target);
    }

    [Fact]
    public void A_Reduce_yes_no_week_s_target_stays_0_with_Upcoming_commitments()
    {
        var target = Target(YesNoDays(BuildOrReduce.Reduce, MonWedFri));

        var trend = TrendBuilder.Of(target, [Item(Day(Date(2026, 11, 16)), target, No)], [Day(Date(2026, 11, 18)), Day(Date(2026, 11, 20))]);

        Assert.Equal(new TrendBar(Date(2026, 11, 16), Date(2026, 11, 22), 0, 0), Assert.Single(trend.Bars));
    }

    // The caps.

    [Fact]
    public void The_14_cap_keeps_the_latest_days()
    {
        var target = PerDay();
        var begun = Enumerable.Range(1, 20).Select(day => Item(Day(Date(2026, 11, day)), target, Yes)).ToArray();

        var trend = Build(target, begun);

        Assert.Equal(14, trend.Bars.Count);
        Assert.Equal(Date(2026, 11, 7), trend.Bars[0].FirstDate);
        Assert.Equal(Date(2026, 11, 20), trend.Bars[^1].FirstDate);
    }

    [Fact]
    public void The_14_cap_keeps_the_latest_commitments()
    {
        var target = Reading20Daily;
        var begun = Enumerable.Range(1, 20).Select(day => Item(Day(Date(2026, 11, day)), target, Amount(day))).ToArray();

        var trend = Build(target, begun);

        Assert.Equal(Enumerable.Range(7, 14).Select(day => (int?)day), trend.Bars.Select(bar => bar.Value));
    }

    [Fact]
    public void The_12_cap_keeps_the_latest_weeks_of_a_per_week_goal()
    {
        // 15 weeks: Monday 10 August to Monday 16 November 2026.
        var target = PerWeek();
        var begun = Enumerable.Range(0, 15).Select(week => Item(Week(Date(2026, 8, 10).AddDays(7 * week)), target, Yes)).ToArray();

        var trend = Build(target, begun);

        Assert.Equal(12, trend.Bars.Count);
        Assert.Equal(Date(2026, 8, 31), trend.Bars[0].FirstDate);
        Assert.Equal(Date(2026, 11, 16), trend.Bars[^1].FirstDate);
    }

    [Fact]
    public void The_12_cap_keeps_the_latest_weeks_of_a_commitment_based_yes_no_goal()
    {
        // The Mondays of the 15 weeks from Monday 10 August to Monday 16 November 2026.
        var target = OnDays(DayOfWeek.Monday);
        var begun = Enumerable.Range(0, 15).Select(week => Item(Day(Date(2026, 8, 10).AddDays(7 * week)), target, Yes)).ToArray();

        var trend = Build(target, begun);

        Assert.Equal(12, trend.Bars.Count);
        Assert.Equal(new TrendBar(Date(2026, 8, 31), Date(2026, 9, 6), 1, 1), trend.Bars[0]);
        Assert.Equal(new TrendBar(Date(2026, 11, 16), Date(2026, 11, 22), 1, 1), trend.Bars[^1]);
    }

    // No begun entry.

    [Theory]
    [InlineData("commitment yes/no", TrendKind.RecordCount, TrendSpan.Week, 12)]
    [InlineData("commitment duration", TrendKind.Minutes, TrendSpan.Commitment, 14)]
    [InlineData("per day", TrendKind.RecordCount, TrendSpan.Day, 14)]
    [InlineData("per week", TrendKind.RecordCount, TrendSpan.Week, 12)]
    public void Edge_case_a_goal_with_no_begun_entry_has_an_empty_trend_with_the_target_0(
        string goal, TrendKind kind, TrendSpan span, int maxBars)
    {
        var target = goal switch
        {
            "commitment yes/no" => OnDays(MonWedFri),
            "commitment duration" => Reading20Daily,
            "per day" => PerDay(),
            _ => PerWeek(),
        };

        var trend = TrendBuilder.Of(target, [], [Day(Date(2026, 11, 18))]);

        Assert.Empty(trend.Bars);
        Assert.Equal(0, trend.Target);
        Assert.Equal(kind, trend.Kind);
        Assert.Equal(span, trend.Span);
        Assert.Equal(maxBars, trend.MaxBars);
    }
}
