using HT.Application.Exemptions;
using HT.Application.Reports;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using HT.Domain.Reports;
using HT.UnitTests.Application.Exemptions;
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

    private readonly ExemptionWorld _exemptions = new();

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

    // Slice 7 (T067; FR-016, US3-AS4; design/components/TrendChart.md, "Slice 7 state"): marked bars, over the fakes of
    // ExemptionWorld. Excusals and pauses are made through ExcusalService and PauseService while they are current.

    private static TrendBar Excused(TrendBar bar) => bar with { Mark = TrendMark.Excused };

    private static TrendBar Paused(TrendBar bar) => bar with { Mark = TrendMark.Paused };

    private static GoalTargetFields YogaOnMonWedFri => YesNoDays(BuildOrReduce.Build, MonWedFri);

    private static async Task ExcuseAsync(ExemptionWorld world, GoalId goal, DateTimeOffset at, params int[] days)
    {
        world.At(at);
        foreach (var day in days)
        {
            Assert.Equal(ExcuseOutcome.Excused, (await world.ExcuseAsync(goal, Date(2026, 11, day))).Outcome);
        }
    }

    private static async Task PauseAsync(ExemptionWorld world, GoalId goal, DateTimeOffset at, int firstDay, int lastDay)
    {
        world.At(at);
        Assert.Equal(PauseOutcome.Paused, (await world.PauseAsync(goal, Date(2026, 11, firstDay), Date(2026, 11, lastDay))).Outcome);
    }

    [Fact]
    public async Task US3_AS4_Meditation_10_min_daily_marks_the_excused_and_paused_days_without_a_value()
    {
        // Spec US3's goal: 10 minutes on each date from 2 to 16 November except 9, 11 and 13 to 15; 9 November excused on
        // Tuesday 10 November at 08:00Z; paused on Friday 13 November at 08:00Z from that day until Sunday 15 November;
        // read on Thursday 19 November at 12:00Z, when Thursday 19 is Due.
        var goal = await _exemptions.CreateGoalAsync(DurationDays(10, DurationUnit.Minutes, BuildOrReduce.Build, AllDays), Mon2, Mon2Morning);
        foreach (var day in new[] { 2, 3, 4, 5, 6, 7, 8, 10, 12, 16 })
        {
            _exemptions.SeedAmount(goal, Date(2026, 11, day), 10);
        }

        await ExcuseAsync(_exemptions, goal, Utc("2026-11-10T08:00:00Z"), 9);
        await PauseAsync(_exemptions, goal, Utc("2026-11-13T08:00:00Z"), 13, 15);
        _exemptions.At(Utc("2026-11-19T12:00:00Z"));

        var trend = (await _exemptions.ReportAsync(goal)).Trend;

        Assert.Equal(
            [
                Day(11, 6, 10, 10), Day(11, 7, 10, 10), Day(11, 8, 10, 10), Excused(Day(11, 9, null, 10)), Day(11, 10, 10, 10),
                Day(11, 11, null, 10), Day(11, 12, 10, 10), Paused(Day(11, 13, null, 10)), Paused(Day(11, 14, null, 10)),
                Paused(Day(11, 15, null, 10)), Day(11, 16, 10, 10), Day(11, 17, null, 10), Day(11, 18, null, 10), Day(11, 19, null, 10),
            ],
            trend.Bars);
        Assert.Equal(10, trend.Target);
        AssertTexts(trend, "Minutes per day", "Last 14 days", "Target 10 min", "Minutes per day, last 14 days; target 10; 1 excused, 3 paused");
    }

    [Fact]
    public async Task FR_016_a_yes_no_week_counts_only_its_commitments_that_are_neither_excused_nor_paused()
    {
        // Monday, Wednesday and Friday from Monday 2 November. "Yes" on 2, 4 and 6 November, on Monday 9 and on Friday 13
        // (stored before the pause, so it is kept) and on Monday 16. Wednesday 11 is excused on Thursday 12 at 08:00Z;
        // the goal is paused on Friday 13 at 08:00Z for that day only. Read on Wednesday 18 November at 10:00Z: the week
        // of 9 November has one countable commitment (Monday 9) and its "Yes"; the "Yes" of the Paused Friday 13 is not
        // counted. The week of 16 November counts its Upcoming Friday 20.
        var goal = await _exemptions.CreateGoalAsync(YogaOnMonWedFri, Mon2, Mon2Morning);
        foreach (var day in new[] { 2, 4, 6, 9, 13, 16 })
        {
            _exemptions.SeedAnswer(goal, Date(2026, 11, day), YesNoAnswer.Yes);
        }

        await ExcuseAsync(_exemptions, goal, Utc("2026-11-12T08:00:00Z"), 11);
        await PauseAsync(_exemptions, goal, Utc("2026-11-13T08:00:00Z"), 13, 13);
        _exemptions.At(Utc("2026-11-18T10:00:00Z"));

        var trend = (await _exemptions.ReportAsync(goal)).Trend;

        Assert.Equal([Week(Mon2, 3, 3), Week(Date(2026, 11, 9), 1, 1), Week(Date(2026, 11, 16), 1, 3)], trend.Bars);
        Assert.Equal(3, trend.Target);
        AssertTexts(trend, "Days done per week", "Last 12 weeks", "Target 3", "Days done per week, last 12 weeks; target 3; week of 9 Nov: 1 of 1");
    }

    [Fact]
    public async Task FR_016_a_yes_no_week_whose_only_records_sit_on_paused_commitments_has_no_value_not_0()
    {
        // "Yes" on Monday 16 November, stored before the goal is paused on Monday 16 at 08:00Z for that day only. Read on
        // Monday 23 November at 10:00Z: the week of 16 November counts Wednesday 18 and Friday 20, neither recorded.
        var goal = await _exemptions.CreateGoalAsync(YogaOnMonWedFri, Mon2, Mon2Morning);
        _exemptions.SeedAnswer(goal, Date(2026, 11, 16), YesNoAnswer.Yes);
        await PauseAsync(_exemptions, goal, Utc("2026-11-16T08:00:00Z"), 16, 16);
        _exemptions.At(Utc("2026-11-23T10:00:00Z"));

        var trend = (await _exemptions.ReportAsync(goal)).Trend;

        Assert.Equal(
            [Week(Mon2, null, 3), Week(Date(2026, 11, 9), null, 3), Week(Date(2026, 11, 16), null, 2), Week(Date(2026, 11, 23), null, 3)],
            trend.Bars);
        Assert.Equal("Days done per week, last 12 weeks; target 3; week of 16 Nov: no record of 2", trend.Label);
    }

    [Fact]
    public async Task FR_016_an_all_exempt_past_week_is_marked_Paused_when_one_commitment_is_paused_and_Excused_otherwise()
    {
        // "Yes" on 2, 4 and 6 November. The week of 9 November: Monday 9 excused on Tuesday 10 at 08:00Z, then the goal
        // paused on Wednesday 11 at 08:00Z from that day until Sunday 15. The week of 16 November: Monday 16 and Wednesday
        // 18 excused on Wednesday 18 at 10:00Z, Friday 20 on Friday 20 at 10:00Z. Read on Monday 23 November at 10:00Z,
        // when Monday 23 is Due and the week's Wednesday 25 and Friday 27 are Upcoming.
        var goal = await _exemptions.CreateGoalAsync(YogaOnMonWedFri, Mon2, Mon2Morning);
        foreach (var day in new[] { 2, 4, 6 })
        {
            _exemptions.SeedAnswer(goal, Date(2026, 11, day), YesNoAnswer.Yes);
        }

        await ExcuseAsync(_exemptions, goal, Utc("2026-11-10T08:00:00Z"), 9);
        await PauseAsync(_exemptions, goal, Utc("2026-11-11T08:00:00Z"), 11, 15);
        await ExcuseAsync(_exemptions, goal, Utc("2026-11-18T10:00:00Z"), 16, 18);
        await ExcuseAsync(_exemptions, goal, Utc("2026-11-20T10:00:00Z"), 20);
        _exemptions.At(Utc("2026-11-23T10:00:00Z"));

        var trend = (await _exemptions.ReportAsync(goal)).Trend;

        Assert.Equal(
            [
                Week(Mon2, 3, 3), Paused(Week(Date(2026, 11, 9), null, 0)), Excused(Week(Date(2026, 11, 16), null, 0)),
                Week(Date(2026, 11, 23), null, 3),
            ],
            trend.Bars);
        Assert.Equal(3, trend.Target);
        Assert.Equal("Days done per week, last 12 weeks; target 3; 1 excused, 1 paused", trend.Label);
    }

    [Fact]
    public async Task T036_a_current_week_whose_begun_commitment_is_excused_and_upcoming_ones_paused_is_marked_Paused()
    {
        // "Yes" on every commitment of the weeks of 2 and 9 November. On Monday 16 November at 10:00Z (Monday 16 Due)
        // Monday 16 is excused and the goal paused from Tuesday 17 until Sunday 22, so Wednesday 18 and Friday 20, both
        // Upcoming, are Paused. The line stays at the week of 9 November's target.
        var goal = await _exemptions.CreateGoalAsync(YogaOnMonWedFri, Mon2, Mon2Morning);
        foreach (var day in new[] { 2, 4, 6, 9, 11, 13 })
        {
            _exemptions.SeedAnswer(goal, Date(2026, 11, day), YesNoAnswer.Yes);
        }

        var mon16 = Utc("2026-11-16T10:00:00Z");
        await ExcuseAsync(_exemptions, goal, mon16, 16);
        await PauseAsync(_exemptions, goal, mon16, 17, 22);

        var trend = (await _exemptions.ReportAsync(goal)).Trend;

        Assert.Equal([Week(Mon2, 3, 3), Week(Date(2026, 11, 9), 3, 3), Paused(Week(Date(2026, 11, 16), null, 0))], trend.Bars);
        Assert.Equal(3, trend.Target);
        AssertTexts(trend, "Days done per week", "Last 12 weeks", "Target 3", "Days done per week, last 12 weeks; target 3; 1 paused");
    }

    [Fact]
    public async Task T036_a_current_week_with_an_upcoming_commitment_that_is_not_paused_is_not_marked()
    {
        // As above, but the pause is Wednesday 18 November only: the Upcoming Friday 20 is countable, so the week of 16
        // November is unmarked with the target 1 and no value; the line is drawn at its target.
        var goal = await _exemptions.CreateGoalAsync(YogaOnMonWedFri, Mon2, Mon2Morning);
        foreach (var day in new[] { 2, 4, 6, 9, 11, 13 })
        {
            _exemptions.SeedAnswer(goal, Date(2026, 11, day), YesNoAnswer.Yes);
        }

        var mon16 = Utc("2026-11-16T10:00:00Z");
        await ExcuseAsync(_exemptions, goal, mon16, 16);
        await PauseAsync(_exemptions, goal, mon16, 18, 18);

        var trend = (await _exemptions.ReportAsync(goal)).Trend;

        Assert.Equal([Week(Mon2, 3, 3), Week(Date(2026, 11, 9), 3, 3), Week(Date(2026, 11, 16), null, 1)], trend.Bars);
        Assert.Equal(1, trend.Target);
        AssertTexts(
            trend, "Days done per week", "Last 12 weeks", "Target 1",
            "Days done per week, last 12 weeks; target 1; week of 2 Nov: 3 of 3; week of 9 Nov: 3 of 3");
    }

    [Fact]
    public async Task FR_016_a_per_week_goal_marks_a_week_with_a_paused_date_and_gives_it_no_value_despite_its_records()
    {
        // Three times a week: "Yes" on 2, 4 and 6 November and on Monday 9 and Tuesday 10. Paused on Monday 9 at 08:00Z
        // from Thursday 12 until Friday 13. Read on Wednesday 18 November at 10:00Z.
        var goal = await _exemptions.CreateGoalAsync(YesNoTimes(3, SchedulePeriod.Week), Mon2, Mon2Morning);
        foreach (var day in new[] { 2, 4, 6, 9, 10 })
        {
            _exemptions.SeedAnswer(goal, Date(2026, 11, day), YesNoAnswer.Yes);
        }

        await PauseAsync(_exemptions, goal, Utc("2026-11-09T08:00:00Z"), 12, 13);
        _exemptions.At(Utc("2026-11-18T10:00:00Z"));

        var trend = (await _exemptions.ReportAsync(goal)).Trend;

        Assert.Equal([Week(Mon2, 3, 3), Paused(Week(Date(2026, 11, 9), null, 3)), Week(Date(2026, 11, 16), null, 3)], trend.Bars);
        Assert.Equal(3, trend.Target);
        Assert.Equal("Times per week, last 12 weeks; target 3; 1 paused", trend.Label);
    }

    [Fact]
    public async Task FR_016_with_every_bar_marked_the_line_is_drawn_at_0()
    {
        // Three times a week, paused when it is created, on Monday 2 November at 08:00Z, for its whole first week; read
        // that day at 10:00Z.
        var goal = await _exemptions.CreateGoalAsync(YesNoTimes(3, SchedulePeriod.Week), Mon2, Mon2Morning);
        await PauseAsync(_exemptions, goal, Mon2Morning, 2, 8);
        _exemptions.At(Utc("2026-11-02T10:00:00Z"));

        var trend = (await _exemptions.ReportAsync(goal)).Trend;

        Assert.Equal([Paused(Week(Mon2, null, 3))], trend.Bars);
        Assert.Equal(0, trend.Target);
        AssertTexts(trend, "Times per week", "Last 12 weeks", "Target 0", "Times per week, last 12 weeks; target 0; 1 paused");
    }
}
