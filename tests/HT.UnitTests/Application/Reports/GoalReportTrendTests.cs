using HT.Application.Reports;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using HT.Domain.Reports;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Reports;

/// <summary>
/// The trend of a goal through <see cref="ReportService.GetGoalReportAsync"/> (US4-AS3 to AS7; FR-006, Edge Cases;
/// design/components/TrendChart.md, "Slice 6 state"), over the fakes of <see cref="ReportWorld"/>. The user has no
/// stored time zone, so the goals are in UTC; unless a test says otherwise a goal starts on Monday 2 November 2026 and
/// is created at 08:00Z that day. A bar begins with its commitment, day or week, and a week of a commitment-based yes/no
/// goal with its first commitment; the values count the records of every state. Every instant is worked out by hand.
/// </summary>
public sealed class GoalReportTrendTests
{
    private static readonly DateOnly Mon2 = Date(2026, 11, 2);
    private static readonly DateTimeOffset Mon2Morning = Utc("2026-11-02T08:00:00Z");

    private readonly ReportWorld _world = new();

    private static GoalTargetFields Reading20MinDaily => DurationDays(20, DurationUnit.Minutes, BuildOrReduce.Build, AllDays);

    private Task<GoalId> GoalAsync(GoalTargetFields target) => _world.CreateGoalAsync(target, Mon2, Mon2Morning);

    private static TrendBar Day(int month, int day, int? value, int target) => new(Date(2026, month, day), Date(2026, month, day), value, target);

    private static TrendBar Week(DateOnly monday, int? value, int target) => new(monday, monday.AddDays(6), value, target);

    private static void AssertTexts(TrendView trend, string title, string subLine, string targetLine, string label)
    {
        Assert.Equal(title, trend.Title);
        Assert.Equal(subLine, trend.SubLine);
        Assert.Equal(targetLine, trend.TargetLine);
        Assert.Equal(label, trend.Label);
    }

    [Fact]
    public async Task US4_AS3_Reading_20_min_daily_with_20_12_none_and_25_gives_those_values_against_20()
    {
        // 20, 12, nothing and 25 minutes on Monday 2 to Thursday 5 November; read on Thursday 5 at 10:00Z, when Thursday 5
        // is Due with its record.
        var goal = await GoalAsync(Reading20MinDaily);
        _world.SeedAmount(goal, Mon2, 20);
        _world.SeedAmount(goal, Date(2026, 11, 3), 12);
        _world.SeedAmount(goal, Date(2026, 11, 5), 25);
        _world.At(Utc("2026-11-05T10:00:00Z"));

        var trend = (await _world.ReportAsync(goal)).Trend;

        Assert.Equal([Day(11, 2, 20, 20), Day(11, 3, 12, 20), Day(11, 4, null, 20), Day(11, 5, 25, 20)], trend.Bars);
        Assert.Equal((TrendKind.Minutes, TrendSpan.Commitment, Measurement.Duration, 20), (trend.Kind, trend.Span, trend.Measurement, trend.Target));
        AssertTexts(trend, "Minutes per day", "Last 14 days", "Target 20 min", "Minutes per day, last 14 days; target 20");
    }

    [Fact]
    public async Task US4_AS3_a_commitment_based_goal_keeps_its_last_14_begun_commitments()
    {
        // 20 minutes on each of the 20 days from Monday 2 to Saturday 21 November; read on Saturday 21 at 10:00Z.
        var goal = await GoalAsync(Reading20MinDaily);
        for (var date = Mon2; date <= Date(2026, 11, 21); date = date.AddDays(1))
        {
            _world.SeedAmount(goal, date, 20);
        }

        _world.At(Utc("2026-11-21T10:00:00Z"));

        var trend = (await _world.ReportAsync(goal)).Trend;

        Assert.Equal(Enumerable.Range(8, 14).Select(day => Day(11, day, 20, 20)), trend.Bars);
    }

    [Fact]
    public async Task US4_AS4_Yoga_3_times_a_week_gives_the_last_12_weeks_with_the_weeks_progress_against_3()
    {
        // From Monday 2 November; read on Wednesday 3 February 2027 at 10:00Z, when 14 weeks have begun (2 November to 1
        // February). Two "Yes" in the week of 16 November, three in the week of 25 January, none in the others.
        var goal = await GoalAsync(YesNoTimes(3, SchedulePeriod.Week));
        foreach (var date in new[] { Date(2026, 11, 17), Date(2026, 11, 18), new DateOnly(2027, 1, 25), new DateOnly(2027, 1, 27), new DateOnly(2027, 1, 29) })
        {
            _world.SeedAnswer(goal, date, YesNoAnswer.Yes);
        }

        _world.At(Utc("2027-02-03T10:00:00Z"));

        var trend = (await _world.ReportAsync(goal)).Trend;

        var mondays = Enumerable.Range(0, 12).Select(week => Date(2026, 11, 16).AddDays(7 * week)).ToList();
        Assert.Equal(
            mondays.Select(monday => Week(monday, monday == Date(2026, 11, 16) ? 2 : monday == new DateOnly(2027, 1, 25) ? 3 : null, 3)),
            trend.Bars);
        Assert.Equal((TrendKind.RecordCount, TrendSpan.Week, 3), (trend.Kind, trend.Span, trend.Target));
        AssertTexts(trend, "Times per week", "Last 12 weeks", "Target 3", "Times per week, last 12 weeks; target 3");
    }

    [Fact]
    public async Task US4_AS5_a_yes_no_goal_on_Monday_Wednesday_and_Friday_gives_weekly_bars_against_its_weeks_commitments()
    {
        // From Wednesday 4 November (created at 08:00Z that day); read on Wednesday 18 November at 10:00Z. The week of 2
        // November has 2 commitments (4 and 6), the week of 9 has 3, the week of 16 has 3 (16 and 18 begun, Friday 20
        // Upcoming). "Yes" on 4, 6, 9, 13 and 16, "No" on 11.
        var goal = await _world.CreateGoalAsync(YesNoDays(BuildOrReduce.Build, MonWedFri), Date(2026, 11, 4), Utc("2026-11-04T08:00:00Z"));
        foreach (var day in new[] { 4, 6, 9, 13, 16 })
        {
            _world.SeedAnswer(goal, Date(2026, 11, day), YesNoAnswer.Yes);
        }

        _world.SeedAnswer(goal, Date(2026, 11, 11), YesNoAnswer.No);
        _world.At(Utc("2026-11-18T10:00:00Z"));

        var trend = (await _world.ReportAsync(goal)).Trend;

        Assert.Equal([Week(Mon2, 2, 2), Week(Date(2026, 11, 9), 2, 3), Week(Date(2026, 11, 16), 1, 3)], trend.Bars);
        Assert.Equal((TrendKind.RecordCount, TrendSpan.Week, 3), (trend.Kind, trend.Span, trend.Target));
        AssertTexts(trend, "Days done per week", "Last 12 weeks", "Target 3", "Days done per week, last 12 weeks; target 3; week of 2 Nov: 2 of 2");
    }

    [Fact]
    public async Task US4_AS5_a_Tuesday_and_Thursday_goal_read_on_a_Monday_has_the_previous_week_as_its_latest_bar()
    {
        // From Tuesday 3 November (created at 08:00Z that day); read on Monday 16 November at 10:00Z, when no commitment of
        // the week of 16 November has begun. "Yes" on Tuesday 10.
        var goal = await _world.CreateGoalAsync(
            YesNoDays(BuildOrReduce.Build, DayOfWeek.Tuesday, DayOfWeek.Thursday), Date(2026, 11, 3), Utc("2026-11-03T08:00:00Z"));
        _world.SeedAnswer(goal, Date(2026, 11, 10), YesNoAnswer.Yes);
        _world.At(Utc("2026-11-16T10:00:00Z"));

        var trend = (await _world.ReportAsync(goal)).Trend;

        Assert.Equal([Week(Mon2, null, 2), Week(Date(2026, 11, 9), 1, 2)], trend.Bars);
        Assert.Equal(2, trend.Target);
    }

    [Fact]
    public async Task US4_AS6_a_commitment_with_a_record_of_0_has_0_and_one_with_no_record_no_value()
    {
        // At least 5 pages a day: 0 on Monday 2, nothing on Tuesday 3, 7 on Wednesday 4; read on Wednesday 4 at 10:00Z.
        var goal = await GoalAsync(CountDays(5, "pages", BuildOrReduce.Build, AllDays));
        _world.SeedAmount(goal, Mon2, 0);
        _world.SeedAmount(goal, Date(2026, 11, 4), 7);
        _world.At(Utc("2026-11-04T10:00:00Z"));

        var trend = (await _world.ReportAsync(goal)).Trend;

        Assert.Equal([Day(11, 2, 0, 5), Day(11, 3, null, 5), Day(11, 4, 7, 5)], trend.Bars);
        Assert.Equal((TrendKind.Count, TrendSpan.Commitment), (trend.Kind, trend.Span));
        AssertTexts(trend, "Pages per day", "Last 14 days", "Target 5", "Pages per day, last 14 days; target 5");
    }

    [Fact]
    public async Task US4_AS6_a_Reduce_count_day_with_no_record_has_no_value_and_one_with_0_has_0()
    {
        // At most 2 sweets a day, counted per day: 3 on Monday 2, nothing on Tuesday 3, 0 on Wednesday 4; read on
        // Wednesday 4 at 10:00Z.
        var goal = await GoalAsync(CountTimes(2, "sweets", 1, SchedulePeriod.Day, BuildOrReduce.Reduce));
        _world.SeedAmount(goal, Mon2, 3);
        _world.SeedAmount(goal, Date(2026, 11, 4), 0);
        _world.At(Utc("2026-11-04T10:00:00Z"));

        var trend = (await _world.ReportAsync(goal)).Trend;

        Assert.Equal([Day(11, 2, 3, 2), Day(11, 3, null, 2), Day(11, 4, 0, 2)], trend.Bars);
        Assert.Equal((TrendKind.Count, TrendSpan.Day), (trend.Kind, trend.Span));
        AssertTexts(trend, "Sweets per day", "Last 14 days", "Target 2", "Sweets per day, last 14 days; target 2");
    }

    [Fact]
    public async Task US4_AS7_a_Reduce_yes_no_week_with_one_Yes_and_two_No_gives_1_against_0()
    {
        // Avoided on Monday, Wednesday and Friday: "Yes" on Monday 2, "No" on Wednesday 4 and Friday 6; read on Monday 9
        // at 10:00Z, when the week of 9 November has begun with its Due Monday and no record.
        var goal = await GoalAsync(YesNoDays(BuildOrReduce.Reduce, MonWedFri));
        _world.SeedAnswer(goal, Mon2, YesNoAnswer.Yes);
        _world.SeedAnswer(goal, Date(2026, 11, 4), YesNoAnswer.No);
        _world.SeedAnswer(goal, Date(2026, 11, 6), YesNoAnswer.No);
        _world.At(Utc("2026-11-09T10:00:00Z"));

        var trend = (await _world.ReportAsync(goal)).Trend;

        Assert.Equal([Week(Mon2, 1, 0), Week(Date(2026, 11, 9), null, 0)], trend.Bars);
        Assert.Equal(0, trend.Target);
        AssertTexts(trend, "Days done per week", "Last 12 weeks", "Target 0", "Days done per week, last 12 weeks; target 0");
    }

    [Fact]
    public async Task US4_AS7_30_min_3_times_a_week_with_30_25_and_40_gives_2_against_3()
    {
        // Cycling, at least 30 min, 3 times a week: rides of 30, 25 and 40 minutes on Monday 2 to Wednesday 4; read on
        // Wednesday 4 at 10:00Z, when the week is Due.
        var goal = await GoalAsync(DurationTimes(30, DurationUnit.Minutes, 3, SchedulePeriod.Week));
        _world.SeedAmount(goal, Mon2, 30);
        _world.SeedAmount(goal, Date(2026, 11, 3), 25);
        _world.SeedAmount(goal, Date(2026, 11, 4), 40);
        _world.At(Utc("2026-11-04T10:00:00Z"));

        var trend = (await _world.ReportAsync(goal)).Trend;

        Assert.Equal([Week(Mon2, 2, 3)], trend.Bars);
        Assert.Equal((TrendKind.RecordCount, TrendSpan.Week, Measurement.Duration), (trend.Kind, trend.Span, trend.Measurement));
        AssertTexts(trend, "Times per week", "Last 12 weeks", "Target 3", "Times per week, last 12 weeks; target 3");
    }

    [Fact]
    public async Task Edge_case_a_goal_in_hours_gives_minutes_and_Target_60_min()
    {
        // At least 1 h a day: 45 minutes on Monday 2 and 90 on Tuesday 3; read on Tuesday 3 at 10:00Z.
        var goal = await GoalAsync(DurationDays(1, DurationUnit.Hours, BuildOrReduce.Build, AllDays));
        _world.SeedAmount(goal, Mon2, 45);
        _world.SeedAmount(goal, Date(2026, 11, 3), 90);
        _world.At(Utc("2026-11-03T10:00:00Z"));

        var trend = (await _world.ReportAsync(goal)).Trend;

        Assert.Equal([Day(11, 2, 45, 60), Day(11, 3, 90, 60)], trend.Bars);
        Assert.Equal((TrendKind.Minutes, 60), (trend.Kind, trend.Target));
        AssertTexts(trend, "Minutes per day", "Last 14 days", "Target 60 min", "Minutes per day, last 14 days; target 60");
    }

    [Fact]
    public async Task Edge_case_a_Reduce_count_commitment_gives_Target_1()
    {
        // Sugar, at most 1 sweet a day: 0 on Monday 2 and 2 on Tuesday 3; read on Tuesday 3 at 10:00Z.
        var goal = await GoalAsync(CountDays(1, "sweet", BuildOrReduce.Reduce, AllDays));
        _world.SeedAmount(goal, Mon2, 0);
        _world.SeedAmount(goal, Date(2026, 11, 3), 2);
        _world.At(Utc("2026-11-03T10:00:00Z"));

        var trend = (await _world.ReportAsync(goal)).Trend;

        Assert.Equal([Day(11, 2, 0, 1), Day(11, 3, 2, 1)], trend.Bars);
        Assert.Equal((TrendKind.Count, 1), (trend.Kind, trend.Target));
        AssertTexts(trend, "Sweet per day", "Last 14 days", "Target 1", "Sweet per day, last 14 days; target 1");
    }

    [Fact]
    public async Task Edge_case_an_amount_above_the_target_is_given_as_recorded()
    {
        // 45 minutes on Monday 2 against 20; read on Thursday 5 at 10:00Z, when Monday 2 is Met (its grace period ended at 00:00Z).
        var goal = await GoalAsync(Reading20MinDaily);
        _world.SeedAmount(goal, Mon2, 45);
        _world.At(Utc("2026-11-05T10:00:00Z"));

        var report = await _world.ReportAsync(goal);

        Assert.Equal(Day(11, 2, 45, 20), report.Trend.Bars[0]);
        Assert.Equal(1, report.Stats.Consistency.Successful);
    }

    [Fact]
    public async Task Edge_case_a_goal_not_started_has_an_empty_trend_and_no_begun_entry()
    {
        var goal = await _world.CreateGoalAsync(Reading20MinDaily, Date(2026, 11, 20), Utc("2026-11-16T08:00:00Z"));
        _world.At(Utc("2026-11-17T10:00:00Z"));

        var report = await _world.ReportAsync(goal);

        Assert.Empty(report.Trend.Bars);
        Assert.Equal(0, report.Trend.Target);
        Assert.False(report.HasBegunEntry);
    }
}
