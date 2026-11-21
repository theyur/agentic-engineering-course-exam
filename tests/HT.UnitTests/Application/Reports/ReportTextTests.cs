using HT.Application.Records;
using HT.Application.Reports;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Reports;
using static HT.UnitTests.Domain.Goals.GoalSamples;

namespace HT.UnitTests.Application.Reports;

/// <summary>
/// The shared report formatter (HP-005, SC-002, research R-13): every consistency, streak, unit, state, progress,
/// time-left, trend and strip text as the design's slice-6 state words it, and the same texts under cultures with
/// another decimal and list separator.
/// </summary>
public sealed class ReportTextTests
{
    private static readonly DateOnly Wednesday = new(2026, 11, 18);
    private static readonly DateOnly WeekMonday = new(2026, 11, 16);
    private static readonly DateOnly WeekSunday = new(2026, 11, 22);
    private static readonly DateTimeOffset Now = new(2026, 11, 18, 19, 0, 0, TimeSpan.Zero);

    private static GoalHistory History(GoalTarget target) => new()
    {
        GoalId = new GoalId(Guid.Parse("0b7e5f2a-0000-4000-8000-000000000006")),
        GoalScheduleKind = target.ScheduleKindOf,
        Period = target.PeriodLength,
        BuildOrReduce = target.BuildOrReduce,
        Measurement = target.Measurement,
        Amount = target.Amount,
        CountUnit = target.CountUnit,
        DurationUnit = target.DurationUnit,
        Times = target.Schedule.Times,
        CurrentDate = Wednesday,
        Target = target,
        ReadAt = Now,
        Entries = [],
    };

    private static HistoryEntry Entry(DateOnly first, DateOnly last, Progress progress) => new()
    {
        FirstDate = first,
        LastDate = last,
        Starts = new DateTimeOffset(first.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero),
        Ends = new DateTimeOffset(last.AddDays(1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero),
        State = EvaluationState.Due,
        GraceEnds = new DateTimeOffset(last.AddDays(3).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero),
        Records = [],
        Progress = progress,
        IsCorrection = false,
    };

    private static TrendView View(string title, string subLine, TrendSpan span, int target, params TrendBar[] bars) => new()
    {
        Kind = TrendKind.RecordCount,
        Span = span,
        Measurement = Measurement.YesNo,
        Bars = bars,
        Target = target,
        Title = title,
        SubLine = subLine,
        TargetLine = ReportText.TargetLine(TrendKind.RecordCount, target),
        Label = string.Empty,
    };

    private static EvaluationState[] States(params (EvaluationState State, int Count)[] runs) =>
        runs.SelectMany(run => Enumerable.Repeat(run.State, run.Count)).ToArray();

    // Consistency (FR-002)

    [Theory]
    [InlineData(18, 20, "90% (18 of 20)")]
    [InlineData(2, 3, "66% (2 of 3)")]
    [InlineData(1, 3, "33% (1 of 3)")]
    [InlineData(0, 2, "0% (0 of 2)")]
    [InlineData(5, 5, "100% (5 of 5)")]
    [InlineData(0, 0, "N/A")]
    public void Consistency_is_the_percentage_with_its_count_or_NA(int successful, int counted, string expected) =>
        Assert.Equal(expected, ReportText.Consistency(new Consistency(successful, counted)));

    // Units and streaks (FR-004)

    [Theory]
    [InlineData(StreakUnit.Commitments, 0, "commitments")]
    [InlineData(StreakUnit.Commitments, 1, "commitment")]
    [InlineData(StreakUnit.Commitments, 2, "commitments")]
    [InlineData(StreakUnit.Days, 0, "days")]
    [InlineData(StreakUnit.Days, 1, "day")]
    [InlineData(StreakUnit.Days, 2, "days")]
    [InlineData(StreakUnit.Weeks, 0, "weeks")]
    [InlineData(StreakUnit.Weeks, 1, "week")]
    [InlineData(StreakUnit.Weeks, 2, "weeks")]
    public void Unit_is_singular_for_exactly_1(StreakUnit unit, int count, string expected) =>
        Assert.Equal(expected, ReportText.Unit(unit, count));

    [Theory]
    [InlineData(0, StreakUnit.Days, "0 days")]
    [InlineData(1, StreakUnit.Days, "1 day")]
    [InlineData(1, StreakUnit.Commitments, "1 commitment")]
    [InlineData(1, StreakUnit.Weeks, "1 week")]
    [InlineData(6, StreakUnit.Commitments, "6 commitments")]
    [InlineData(0, StreakUnit.Weeks, "0 weeks")]
    public void Streak_is_the_number_with_its_unit(int count, StreakUnit unit, string expected) =>
        Assert.Equal(expected, ReportText.Streak(count, unit));

    [Fact]
    public void The_stat_labels_carry_the_plural_unit()
    {
        Assert.Equal("Streak, commitments", ReportText.StreakLabel(StreakUnit.Commitments));
        Assert.Equal("Streak, days", ReportText.StreakLabel(StreakUnit.Days));
        Assert.Equal("Longest, weeks", ReportText.LongestLabel(StreakUnit.Weeks));
        Assert.Equal("Longest, commitments", ReportText.LongestLabel(StreakUnit.Commitments));
    }

    [Fact]
    public void SummaryLine_names_the_current_streak_with_its_unit_and_the_longest()
    {
        Assert.Equal("Streak 6 commitments · longest 11", ReportText.SummaryLine(new Streaks(6, 11), StreakUnit.Commitments));
        Assert.Equal("Streak 1 week · longest 1", ReportText.SummaryLine(new Streaks(1, 1), StreakUnit.Weeks));
    }

    [Fact]
    public void NoFinalYet_names_the_plural_unit()
    {
        Assert.Equal("No finalized days yet", ReportText.NoFinalYet(StreakUnit.Days));
        Assert.Equal("No finalized commitments yet", ReportText.NoFinalYet(StreakUnit.Commitments));
        Assert.Equal("No finalized weeks yet", ReportText.NoFinalYet(StreakUnit.Weeks));
    }

    // State (constitution I, research R-12)

    [Fact]
    public void State_of_Upcoming_is_Upcoming()
    {
        Assert.Equal("Upcoming", ReportText.State(EvaluationState.Upcoming, GoalScheduleKind.CommitmentBased, null));
        Assert.Equal("Upcoming", ReportText.State(EvaluationState.Upcoming, GoalScheduleKind.PeriodBased, SchedulePeriod.Week));
    }

    [Theory]
    [InlineData(EvaluationState.Met, GoalScheduleKind.CommitmentBased, null)]
    [InlineData(EvaluationState.Failed, GoalScheduleKind.CommitmentBased, null)]
    [InlineData(EvaluationState.Missed, GoalScheduleKind.PeriodBased, SchedulePeriod.Day)]
    [InlineData(EvaluationState.Pending, GoalScheduleKind.PeriodBased, SchedulePeriod.Week)]
    [InlineData(EvaluationState.Due, GoalScheduleKind.CommitmentBased, null)]
    [InlineData(EvaluationState.Due, GoalScheduleKind.PeriodBased, SchedulePeriod.Day)]
    [InlineData(EvaluationState.Due, GoalScheduleKind.PeriodBased, SchedulePeriod.Week)]
    public void State_of_every_other_state_is_the_slice_5_word(EvaluationState state, GoalScheduleKind kind, SchedulePeriod? period) =>
        Assert.Equal(RecordingText.State(state, kind, period), ReportText.State(state, kind, period));

    // Progress and time left (HP-002, HP-003)

    [Fact]
    public void ProgressText_of_a_duration_commitment_is_its_amount_today()
    {
        var history = History(Target(DurationDays(20, DurationUnit.Minutes, BuildOrReduce.Build, AllDays)));

        Assert.Equal("12 of 20 min today", ReportText.ProgressText(history, Entry(Wednesday, Wednesday, new Progress(ProgressKind.RecordedValue, 12, 20))));
    }

    [Fact]
    public void ProgressText_of_a_per_week_yes_no_period_is_its_records_this_week()
    {
        var history = History(Target(YesNoTimes(3, SchedulePeriod.Week)));

        Assert.Equal("1 of 3 this week", ReportText.ProgressText(history, Entry(WeekMonday, WeekSunday, new Progress(ProgressKind.RecordsReachingAmount, 1, 3))));
    }

    [Fact]
    public void ProgressText_of_a_Reduce_count_day_is_its_total_at_most_today()
    {
        var history = History(Target(CountTimes(3, "cups", 1, SchedulePeriod.Day, BuildOrReduce.Reduce)));

        Assert.Equal("2 of at most 3 cups today", ReportText.ProgressText(history, Entry(Wednesday, Wednesday, new Progress(ProgressKind.Total, 2, 3))));
    }

    [Fact]
    public void ProgressText_of_a_Reduce_yes_no_period_and_of_a_yes_no_commitment()
    {
        var reduce = History(Target(YesNoTimes(2, SchedulePeriod.Day, BuildOrReduce.Reduce)));
        var yoga = History(OnDays(MonWedFri));

        Assert.Equal("1 of at most 2 today", ReportText.ProgressText(reduce, Entry(Wednesday, Wednesday, new Progress(ProgressKind.YesRecords, 1, 2))));
        Assert.Equal("Yes today", ReportText.ProgressText(yoga, Entry(Wednesday, Wednesday, new Progress(ProgressKind.RecordedValue, 1, 1))));
        Assert.Equal("No today", ReportText.ProgressText(yoga, Entry(Wednesday, Wednesday, new Progress(ProgressKind.RecordedValue, 0, 1))));
    }

    [Fact]
    public void ProgressText_of_a_commitment_without_a_value_is_refused() =>
        Assert.Throws<ArgumentException>(() => ReportText.ProgressText(
            History(OnDays(MonWedFri)), Entry(Wednesday, Wednesday, new Progress(ProgressKind.RecordedValue, null, 1))));

    [Fact]
    public void PendingLeft_rounds_the_hours_up()
    {
        Assert.Equal("5 h left to record", ReportText.PendingLeft(Now.AddHours(5), Now));
        Assert.Equal("5 h left to record", ReportText.PendingLeft(Now.AddHours(4).AddSeconds(1), Now));
        Assert.Equal("1 h left to record", ReportText.PendingLeft(Now.AddMinutes(1), Now));
    }

    // Trend title and sub line (design/components/TrendChart.md, "Title and sub line")

    public static TheoryData<GoalTarget, string, string> TrendTitles() => new()
    {
        { OnDays(MonWedFri), "Days done per week", "Last 12 weeks" },
        { Target(YesNoDays(BuildOrReduce.Reduce, AllDays)), "Days done per week", "Last 12 weeks" },
        { Target(CountDays(20, "pages", BuildOrReduce.Build, AllDays)), "Pages per day", "Last 14 days" },
        { Target(CountDays(2, "cups", BuildOrReduce.Reduce, AllDays)), "Cups per day", "Last 14 days" },
        { Target(CountDays(5, "pages", BuildOrReduce.Build, Weekdays)), "Pages per day", "Last 14 commitments" },
        { Target(CountDays(1, "sweet", BuildOrReduce.Reduce, MonWedFri)), "Sweet per day", "Last 14 commitments" },
        { Target(DurationDays(20, DurationUnit.Minutes, BuildOrReduce.Build, AllDays)), "Minutes per day", "Last 14 days" },
        { Target(DurationDays(1, DurationUnit.Hours, BuildOrReduce.Reduce, AllDays)), "Minutes per day", "Last 14 days" },
        { Target(DurationDays(30, DurationUnit.Minutes, BuildOrReduce.Build, MonWedFri)), "Minutes per day", "Last 14 commitments" },
        { Target(DurationDays(1, DurationUnit.Hours, BuildOrReduce.Build, Weekdays)), "Minutes per day", "Last 14 commitments" },
        { PerDay(), "Times per day", "Last 14 days" },
        { Target(YesNoTimes(2, SchedulePeriod.Day, BuildOrReduce.Reduce)), "Times per day", "Last 14 days" },
        { Target(CountTimes(20, "pages", 1, SchedulePeriod.Day)), "Times per day", "Last 14 days" },
        { Target(DurationTimes(10, DurationUnit.Minutes, 2, SchedulePeriod.Day)), "Times per day", "Last 14 days" },
        { Target(CountTimes(1, "sweet", 1, SchedulePeriod.Day, BuildOrReduce.Reduce)), "Sweet per day", "Last 14 days" },
        { Target(DurationTimes(120, DurationUnit.Minutes, 1, SchedulePeriod.Day, BuildOrReduce.Reduce)), "Minutes per day", "Last 14 days" },
        { Target(DurationTimes(2, DurationUnit.Hours, 1, SchedulePeriod.Day, BuildOrReduce.Reduce)), "Minutes per day", "Last 14 days" },
        { PerWeek(), "Times per week", "Last 12 weeks" },
        { Target(YesNoTimes(2, SchedulePeriod.Week, BuildOrReduce.Reduce)), "Times per week", "Last 12 weeks" },
        { Target(CountTimes(20, "pages", 3, SchedulePeriod.Week)), "Times per week", "Last 12 weeks" },
        { Target(DurationTimes(30, DurationUnit.Minutes, 3, SchedulePeriod.Week)), "Times per week", "Last 12 weeks" },
        { Target(CountTimes(10, "drinks", 1, SchedulePeriod.Week, BuildOrReduce.Reduce)), "Drinks per week", "Last 12 weeks" },
        { Target(DurationTimes(300, DurationUnit.Minutes, 1, SchedulePeriod.Week, BuildOrReduce.Reduce)), "Minutes per week", "Last 12 weeks" },
        { Target(DurationTimes(5, DurationUnit.Hours, 1, SchedulePeriod.Week, BuildOrReduce.Reduce)), "Minutes per week", "Last 12 weeks" },
    };

    [Theory]
    [MemberData(nameof(TrendTitles))]
    public void TrendTitle_and_TrendSubLine_follow_the_design_table(GoalTarget target, string title, string subLine)
    {
        var trend = TrendBuilder.Of(target, [], []);

        Assert.Equal(title, ReportText.TrendTitle(target, trend.Kind, trend.Span));
        Assert.Equal(subLine, ReportText.TrendSubLine(target, trend.Span, trend.MaxBars));
    }

    // Target line (HP-001)

    [Fact]
    public void TargetLine_names_minutes_only_for_minute_bars()
    {
        var hour = TrendBuilder.Of(Target(DurationDays(1, DurationUnit.Hours, BuildOrReduce.Build, AllDays)), [], []);
        var sugar = TrendBuilder.Of(Target(CountTimes(1, "sweet", 1, SchedulePeriod.Day, BuildOrReduce.Reduce)), [], []);
        var cycling = TrendBuilder.Of(Target(DurationTimes(30, DurationUnit.Minutes, 3, SchedulePeriod.Week)), [], []);

        Assert.Equal("Target 20 min", ReportText.TargetLine(TrendKind.Minutes, 20));
        Assert.Equal("Target 60 min", ReportText.TargetLine(hour.Kind, 60));
        Assert.Equal("Target 1", ReportText.TargetLine(sugar.Kind, 1));
        Assert.Equal("Target 3", ReportText.TargetLine(cycling.Kind, 3));
    }

    // Strip label (HP-006, design/components/ConsistencyStrip.md)

    [Fact]
    public void StripLabel_counts_each_state_in_order_and_leaves_out_a_state_with_0()
    {
        var full = States(
            (EvaluationState.Met, 9), (EvaluationState.Failed, 1), (EvaluationState.Met, 8), (EvaluationState.Missed, 1),
            (EvaluationState.Met, 7), (EvaluationState.Pending, 2));
        var mini = States(
            (EvaluationState.Met, 5), (EvaluationState.Missed, 1), (EvaluationState.Met, 7), (EvaluationState.Pending, 2));

        Assert.Equal("Last 28 commitments: 24 met, 1 failed, 1 missed, 2 pending", ReportText.StripLabel(full, StreakUnit.Commitments));
        Assert.Equal("Last 15 commitments: 12 met, 1 missed, 2 pending", ReportText.StripLabel(mini, StreakUnit.Commitments));
    }

    [Fact]
    public void StripLabel_of_one_cell_and_of_none()
    {
        Assert.Equal("Last 1 day: 1 met", ReportText.StripLabel([EvaluationState.Met], StreakUnit.Days));
        Assert.Equal("Last 11 weeks: 11 pending", ReportText.StripLabel(States((EvaluationState.Pending, 11)), StreakUnit.Weeks));
        Assert.Equal("Last 0 commitments", ReportText.StripLabel([], StreakUnit.Commitments));
    }

    [Fact]
    public void StripLabel_refuses_a_state_that_has_no_cell()
    {
        Assert.Throws<ArgumentException>(() => ReportText.StripLabel([EvaluationState.Met, EvaluationState.Due], StreakUnit.Days));
        Assert.Throws<ArgumentException>(() => ReportText.StripLabel([EvaluationState.Upcoming], StreakUnit.Days));
    }

    // Trend label (HP-006, design/components/TrendChart.md)

    [Fact]
    public void TrendLabel_names_the_title_the_sub_line_and_the_target()
    {
        var view = View(
            "Minutes per day",
            "Last 14 days",
            TrendSpan.Day,
            20,
            new TrendBar(new DateOnly(2026, 11, 16), new DateOnly(2026, 11, 16), 25, 20),
            new TrendBar(new DateOnly(2026, 11, 17), new DateOnly(2026, 11, 17), null, 20));

        Assert.Equal("Minutes per day, last 14 days; target 20", ReportText.TrendLabel(view));
    }

    [Fact]
    public void TrendLabel_names_each_bar_whose_own_target_differs()
    {
        var view = View(
            "Days done per week",
            "Last 12 weeks",
            TrendSpan.Week,
            3,
            new TrendBar(new DateOnly(2026, 10, 26), new DateOnly(2026, 11, 1), null, 1),
            new TrendBar(new DateOnly(2026, 11, 2), new DateOnly(2026, 11, 8), 2, 2),
            new TrendBar(new DateOnly(2026, 11, 9), new DateOnly(2026, 11, 15), 3, 3),
            new TrendBar(new DateOnly(2026, 11, 16), new DateOnly(2026, 11, 22), 1, 3));

        Assert.Equal(
            "Days done per week, last 12 weeks; target 3; week of 26 Oct: no record of 1; week of 2 Nov: 2 of 2",
            ReportText.TrendLabel(view));
    }

    [Fact]
    public void TrendLabel_names_a_commitment_or_a_day_by_its_short_date()
    {
        var view = View(
            "Pages per day",
            "Last 14 commitments",
            TrendSpan.Commitment,
            5,
            new TrendBar(new DateOnly(2026, 11, 4), new DateOnly(2026, 11, 4), 4, 6));

        Assert.Equal("Pages per day, last 14 commitments; target 5; Wed 4 Nov: 4 of 6", ReportText.TrendLabel(view));
    }

    // Bar tooltips (design/components/TrendChart.md, "Target line, tooltips and accessible name")

    [Fact]
    public void BarTitle_names_the_date_the_value_and_the_bars_own_target_with_min_for_minutes()
    {
        var minutes = View("Minutes per day", "Last 14 days", TrendSpan.Commitment, 20) with { Kind = TrendKind.Minutes, Measurement = Measurement.Duration };
        var weeks = View("Days done per week", "Last 12 weeks", TrendSpan.Week, 3);
        var counts = View("Pages per day", "Last 14 commitments", TrendSpan.Commitment, 5) with { Kind = TrendKind.Count, Measurement = Measurement.Count };

        Assert.Equal("Wed 4 Nov: 20 of 20 min", ReportText.BarTitle(minutes, new TrendBar(new DateOnly(2026, 11, 4), new DateOnly(2026, 11, 4), 20, 20)));
        Assert.Equal("Fri 6 Nov: 45 of 20 min", ReportText.BarTitle(minutes, new TrendBar(new DateOnly(2026, 11, 6), new DateOnly(2026, 11, 6), 45, 20)));
        Assert.Equal("week of 2 Nov: 2 of 2", ReportText.BarTitle(weeks, new TrendBar(new DateOnly(2026, 11, 2), new DateOnly(2026, 11, 8), 2, 2)));
        Assert.Equal("Wed 4 Nov: 0 of 5", ReportText.BarTitle(counts, new TrendBar(new DateOnly(2026, 11, 4), new DateOnly(2026, 11, 4), 0, 5)));
    }

    [Fact]
    public void BarTitle_of_a_bar_with_no_record_says_no_record()
    {
        var minutes = View("Minutes per day", "Last 14 days", TrendSpan.Day, 20) with { Kind = TrendKind.Minutes, Measurement = Measurement.Duration };
        var weeks = View("Times per week", "Last 12 weeks", TrendSpan.Week, 3);

        Assert.Equal("Sat 7 Nov: no record", ReportText.BarTitle(minutes, new TrendBar(new DateOnly(2026, 11, 7), new DateOnly(2026, 11, 7), null, 20)));
        Assert.Equal("week of 9 Nov: no record", ReportText.BarTitle(weeks, new TrendBar(new DateOnly(2026, 11, 9), new DateOnly(2026, 11, 15), null, 3)));
    }

    // Culture (constitution II)

    [Fact]
    public void Every_text_is_the_same_under_cultures_with_another_decimal_and_list_separator()
    {
        var cups = History(Target(CountTimes(15, "cups", 1, SchedulePeriod.Day, BuildOrReduce.Reduce)));
        var weekView = View(
            "Days done per week",
            "Last 12 weeks",
            TrendSpan.Week,
            3,
            new TrendBar(new DateOnly(2026, 11, 2), new DateOnly(2026, 11, 8), 1234, 2));
        var strip = States((EvaluationState.Met, 1200), (EvaluationState.Failed, 1), (EvaluationState.Pending, 2));
        var minutes = Target(DurationDays(1, DurationUnit.Hours, BuildOrReduce.Build, Weekdays));

        string[] Texts() =>
        [
            ReportText.Consistency(new Consistency(1199, 1200)),
            ReportText.Streak(1234, StreakUnit.Weeks),
            ReportText.SummaryLine(new Streaks(1000, 2000), StreakUnit.Days),
            ReportText.ProgressText(cups, Entry(Wednesday, Wednesday, new Progress(ProgressKind.Total, 1234, 15))),
            ReportText.PendingLeft(Now.AddHours(1000), Now),
            ReportText.TrendTitle(minutes, TrendKind.Minutes, TrendSpan.Commitment),
            ReportText.TrendSubLine(minutes, TrendSpan.Commitment, 14),
            ReportText.TargetLine(TrendKind.Minutes, 1440),
            ReportText.StripLabel(strip, StreakUnit.Commitments),
            ReportText.TrendLabel(weekView),
        ];

        string[] expected =
        [
            "99% (1199 of 1200)",
            "1234 weeks",
            "Streak 1000 days · longest 2000",
            "1234 of at most 15 cups today",
            "1000 h left to record",
            "Minutes per day",
            "Last 14 commitments",
            "Target 1440 min",
            "Last 1203 commitments: 1200 met, 1 failed, 2 pending",
            "Days done per week, last 12 weeks; target 3; week of 2 Nov: 1234 of 2",
        ];

        Assert.Equal(expected, Texts());
        using (new CultureScope("ar-SA"))
        {
            Assert.Equal(expected, Texts());
        }

        using (new CultureScope("de-DE"))
        {
            Assert.Equal(expected, Texts());
        }

        using (new CultureScope("uk-UA"))
        {
            Assert.Equal(expected, Texts());
        }
    }

    // ---- Slice 7: excusals and pauses (HP-004, HP-006, HP-008; T040) ----

    [Theory]
    [InlineData(EvaluationState.Excused, GoalScheduleKind.CommitmentBased, null, "Excused")]
    [InlineData(EvaluationState.Paused, GoalScheduleKind.CommitmentBased, null, "Paused")]
    [InlineData(EvaluationState.Paused, GoalScheduleKind.PeriodBased, SchedulePeriod.Day, "Paused")]
    [InlineData(EvaluationState.Paused, GoalScheduleKind.PeriodBased, SchedulePeriod.Week, "Paused")]
    public void Slice_7_State_gives_Excused_and_Paused(EvaluationState state, GoalScheduleKind kind, SchedulePeriod? period, string expected) =>
        Assert.Equal(expected, ReportText.State(state, kind, period));

    [Fact]
    public void Slice_7_PausedUntil_names_the_pauses_last_day_by_its_short_date()
    {
        Assert.Equal("Paused until Sun 22 Nov", ReportText.PausedUntil(WeekSunday));
        Assert.Equal("Paused until Fri 1 Jan", ReportText.PausedUntil(new DateOnly(2027, 1, 1)));
    }

    [Fact]
    public void Slice_7_StripLabel_counts_excused_and_paused_after_missed_and_before_pending()
    {
        // Spec US3: Met × 7, Excused, Met, Missed, Met, Paused × 3, Met, Pending × 2.
        var strip = States(
            (EvaluationState.Met, 7),
            (EvaluationState.Excused, 1),
            (EvaluationState.Met, 1),
            (EvaluationState.Missed, 1),
            (EvaluationState.Met, 1),
            (EvaluationState.Paused, 3),
            (EvaluationState.Met, 1),
            (EvaluationState.Pending, 2));

        Assert.Equal("Last 17 commitments: 10 met, 1 missed, 1 excused, 3 paused, 2 pending", ReportText.StripLabel(strip, StreakUnit.Commitments));
    }

    [Fact]
    public void Slice_7_StripLabel_of_only_exempt_cells_and_without_any_keeps_its_words()
    {
        var exemptOnly = States((EvaluationState.Excused, 1), (EvaluationState.Paused, 6));
        var withoutExemptions = States((EvaluationState.Met, 24), (EvaluationState.Failed, 1), (EvaluationState.Missed, 1), (EvaluationState.Pending, 2));

        Assert.Equal("Last 7 commitments: 1 excused, 6 paused", ReportText.StripLabel(exemptOnly, StreakUnit.Commitments));
        Assert.Equal("Last 1 week: 1 paused", ReportText.StripLabel([EvaluationState.Paused], StreakUnit.Weeks));
        Assert.Equal("Last 28 commitments: 24 met, 1 failed, 1 missed, 2 pending", ReportText.StripLabel(withoutExemptions, StreakUnit.Commitments));
    }

    [Fact]
    public void Slice_7_TrendLabel_counts_the_marked_bars_after_the_target_apart_from_bars_with_no_record()
    {
        // Spec US3-AS4: 9 November Excused, 13 to 15 November Paused, 11 November with no record.
        var view = View(
            "Minutes per day",
            "Last 14 days",
            TrendSpan.Day,
            10,
            new TrendBar(new DateOnly(2026, 11, 9), new DateOnly(2026, 11, 9), null, 10) { Mark = TrendMark.Excused },
            new TrendBar(new DateOnly(2026, 11, 10), new DateOnly(2026, 11, 10), 10, 10),
            new TrendBar(new DateOnly(2026, 11, 11), new DateOnly(2026, 11, 11), null, 10),
            new TrendBar(new DateOnly(2026, 11, 13), new DateOnly(2026, 11, 13), null, 10) { Mark = TrendMark.Paused },
            new TrendBar(new DateOnly(2026, 11, 14), new DateOnly(2026, 11, 14), null, 10) { Mark = TrendMark.Paused },
            new TrendBar(new DateOnly(2026, 11, 15), new DateOnly(2026, 11, 15), null, 10) { Mark = TrendMark.Paused });

        Assert.Equal("Minutes per day, last 14 days; target 10; 1 excused, 3 paused", ReportText.TrendLabel(view));
    }

    [Fact]
    public void Slice_7_TrendLabel_leaves_out_a_count_of_0_and_never_lists_a_marked_bar_among_differing_targets()
    {
        // The design's commitment-based yes/no example: two all-exempt Paused weeks (target 0) and two weeks with
        // exempt commitments, whose reduced targets differ from the line's.
        var weeks = View(
            "Days done per week",
            "Last 12 weeks",
            TrendSpan.Week,
            3,
            new TrendBar(new DateOnly(2026, 10, 19), new DateOnly(2026, 10, 25), null, 0) { Mark = TrendMark.Paused },
            new TrendBar(new DateOnly(2026, 10, 26), new DateOnly(2026, 11, 1), 2, 2),
            new TrendBar(new DateOnly(2026, 11, 2), new DateOnly(2026, 11, 8), null, 0) { Mark = TrendMark.Paused },
            new TrendBar(new DateOnly(2026, 11, 9), new DateOnly(2026, 11, 15), 1, 1),
            new TrendBar(new DateOnly(2026, 11, 16), new DateOnly(2026, 11, 22), 3, 3));
        var oneExcused = View(
            "Times per day",
            "Last 14 days",
            TrendSpan.Day,
            1,
            new TrendBar(new DateOnly(2026, 11, 9), new DateOnly(2026, 11, 9), null, 1) { Mark = TrendMark.Excused },
            new TrendBar(new DateOnly(2026, 11, 10), new DateOnly(2026, 11, 10), 1, 1));

        Assert.Equal(
            "Days done per week, last 12 weeks; target 3; 2 paused; week of 26 Oct: 2 of 2; week of 9 Nov: 1 of 1",
            ReportText.TrendLabel(weeks));
        Assert.Equal("Times per day, last 14 days; target 1; 1 excused", ReportText.TrendLabel(oneExcused));
    }

    [Fact]
    public void Slice_7_BarTitle_of_a_marked_bar_says_Excused_or_Paused_in_place_of_a_value()
    {
        var minutes = View("Minutes per day", "Last 14 days", TrendSpan.Day, 10) with { Kind = TrendKind.Minutes, Measurement = Measurement.Duration };
        var weeks = View("Days done per week", "Last 12 weeks", TrendSpan.Week, 3);

        Assert.Equal("Mon 9 Nov: Excused", ReportText.BarTitle(minutes, new TrendBar(new DateOnly(2026, 11, 9), new DateOnly(2026, 11, 9), null, 10) { Mark = TrendMark.Excused }));
        Assert.Equal("Fri 13 Nov: Paused", ReportText.BarTitle(minutes, new TrendBar(new DateOnly(2026, 11, 13), new DateOnly(2026, 11, 13), null, 10) { Mark = TrendMark.Paused }));
        Assert.Equal("week of 21 Sep: Paused", ReportText.BarTitle(weeks, new TrendBar(new DateOnly(2026, 9, 21), new DateOnly(2026, 9, 27), null, 0) { Mark = TrendMark.Paused }));
        Assert.Equal("week of 21 Sep: Excused", ReportText.BarTitle(weeks, new TrendBar(new DateOnly(2026, 9, 21), new DateOnly(2026, 9, 27), null, 0) { Mark = TrendMark.Excused }));
    }

    [Fact]
    public void Slice_7_the_Goals_footnote_and_the_empty_Paused_tab_are_worded_as_the_spec_gives_them()
    {
        Assert.Equal(
            "Each goal is counted on its own. Consistency counts only finalized results; pending, excused and paused ones are left out.",
            ReportText.GoalsFootnote);
        Assert.Equal("No paused goals.", ReportText.NoPausedGoals);
    }

    [Fact]
    public void Slice_7_every_text_is_the_same_under_cultures_with_another_decimal_and_list_separator()
    {
        var strip = States((EvaluationState.Met, 1200), (EvaluationState.Excused, 1001), (EvaluationState.Paused, 3));
        var view = View(
            "Days done per week",
            "Last 12 weeks",
            TrendSpan.Week,
            3,
            new TrendBar(new DateOnly(2026, 11, 2), new DateOnly(2026, 11, 8), null, 0) { Mark = TrendMark.Paused },
            new TrendBar(new DateOnly(2026, 11, 9), new DateOnly(2026, 11, 15), null, 0) { Mark = TrendMark.Excused },
            new TrendBar(new DateOnly(2026, 11, 16), new DateOnly(2026, 11, 22), 1234, 2));

        string[] Texts() =>
        [
            ReportText.State(EvaluationState.Excused, GoalScheduleKind.CommitmentBased, null),
            ReportText.State(EvaluationState.Paused, GoalScheduleKind.PeriodBased, SchedulePeriod.Week),
            ReportText.PausedUntil(WeekSunday),
            ReportText.StripLabel(strip, StreakUnit.Commitments),
            ReportText.TrendLabel(view),
            ReportText.BarTitle(view, view.Bars[0]),
        ];

        string[] expected =
        [
            "Excused",
            "Paused",
            "Paused until Sun 22 Nov",
            "Last 2204 commitments: 1200 met, 1001 excused, 3 paused",
            "Days done per week, last 12 weeks; target 3; 1 excused, 1 paused; week of 16 Nov: 1234 of 2",
            "week of 2 Nov: Paused",
        ];

        Assert.Equal(expected, Texts());
        foreach (var culture in new[] { "ar-SA", "de-DE", "uk-UA" })
        {
            using (new CultureScope(culture))
            {
                Assert.Equal(expected, Texts());
            }
        }
    }
}
