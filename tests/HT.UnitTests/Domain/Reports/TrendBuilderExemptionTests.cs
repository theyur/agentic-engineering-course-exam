using HT.Domain.Evaluation;
using HT.Domain.Exemptions;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using HT.Domain.Reports;
using static HT.UnitTests.Domain.Goals.GoalSamples;

namespace HT.UnitTests.Domain.Reports;

/// <summary>
/// Slice 7 FR-016, research R-07, critic-features MINOR-1 and MINOR-2: marked bars have no value and keep their own
/// target; a commitment-based yes/no week counts only its commitments that are neither Excused nor Paused, in its
/// Build target and its value (Build and Reduce), with no value when none of them has a record; an all-exempt week is
/// marked Paused when one of its commitments is Paused, else Excused, with the target 0; the line is the latest
/// unmarked bar's target; with no exemption the new overload equals the old one. Monday 2, 9 and 16 November 2026 are
/// Mondays. The instants of the entries do not matter to the builder; they are each date's midnight in UTC.
/// </summary>
public class TrendBuilderExemptionTests
{
    private const EvaluationState Excused = EvaluationState.Excused;
    private const EvaluationState Paused = EvaluationState.Paused;

    private static readonly RecordValue Yes = RecordValue.FromAnswer(YesNoAnswer.Yes);
    private static readonly RecordValue No = RecordValue.FromAnswer(YesNoAnswer.No);

    private static RecordValue Amount(int amount) => RecordValue.FromAmount(amount);

    private static DateTimeOffset Midnight(DateOnly date) => new(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

    private static CalendarEntry Day(DateOnly date) => new(date, date, Midnight(date), Midnight(date.AddDays(1)));

    private static CalendarEntry Week(DateOnly monday) => new(monday, monday.AddDays(6), Midnight(monday), Midnight(monday.AddDays(7)));

    private static (CalendarEntry Entry, GoalTarget Target, IReadOnlyList<RecordValue> Records, EvaluationState? Exemption) Item(
        CalendarEntry entry, GoalTarget target, EvaluationState? exemption, params RecordValue[] records) => (entry, target, records, exemption);

    private static Trend Build(
        GoalTarget target, params (CalendarEntry Entry, GoalTarget Target, IReadOnlyList<RecordValue> Records, EvaluationState? Exemption)[] begun) =>
        TrendBuilder.Of(target, begun, [], GoalExemptions.None);

    private static TrendBar Bar(DateOnly first, DateOnly last, int? value, int target, TrendMark? mark = null) =>
        new(first, last, value, target) { Mark = mark };

    private static GoalExemptions PausedFrom(DateOnly first, DateOnly last) => new([], [new Pause(new PauseId(Guid.NewGuid()), first, last, null)]);

    /// <summary>"Meditation, 10 min daily": Build, duration 10 minutes, all seven days (commitment-based).</summary>
    private static GoalTarget Meditation10Daily => Target(DurationDays(10, DurationUnit.Minutes, BuildOrReduce.Build, AllDays));

    // Per-period goals and commitment-based count and duration goals.

    [Fact]
    public void US3_AS4_a_daily_duration_goal_marks_the_Excused_and_Paused_days_with_no_value()
    {
        // Begun 2 to 19 November, read on Thursday 19 November at 12:00: 10 minutes on each date from 2 to 16
        // November except 9 (Excused), 11 (Missed) and 13 to 15 (Paused); nothing from 17 on.
        var target = Meditation10Daily;
        var begun = Enumerable.Range(2, 18).Select(day =>
        {
            var date = Date(2026, 11, day);
            EvaluationState? exemption = day switch
            {
                9 => Excused,
                >= 13 and <= 15 => Paused,
                _ => null,
            };
            return day is 9 or 11 or (>= 13 and <= 15) or >= 17
                ? Item(Day(date), target, exemption)
                : Item(Day(date), target, exemption, Amount(10));
        }).ToArray();

        var trend = Build(target, begun);

        Assert.Equal(14, trend.Bars.Count);
        Assert.Equal(Date(2026, 11, 6), trend.Bars[0].FirstDate);
        Assert.Equal(Date(2026, 11, 19), trend.Bars[^1].FirstDate);
        Assert.Equal(10, trend.Target);
        Assert.All(trend.Bars, bar => Assert.Equal(10, bar.Target));

        var byDay = trend.Bars.ToDictionary(bar => bar.FirstDate.Day);
        Assert.Equal(TrendMark.Excused, byDay[9].Mark);
        Assert.All([13, 14, 15], day => Assert.Equal(TrendMark.Paused, byDay[day].Mark));
        Assert.All([9, 13, 14, 15], day => Assert.Null(byDay[day].Value));
        Assert.All([11, 17, 18, 19], day => Assert.Null(byDay[day].Mark));
        Assert.All([11, 17, 18, 19], day => Assert.Null(byDay[day].Value));
        Assert.All([6, 7, 8, 10, 12, 16], day => Assert.Equal(10, byDay[day].Value));
        Assert.All([6, 7, 8, 10, 12, 16], day => Assert.Null(byDay[day].Mark));
    }

    [Fact]
    public void A_per_week_goal_marks_its_Paused_week_with_no_value_whatever_its_records()
    {
        var target = PerWeek();

        var trend = Build(
            target,
            Item(Week(Date(2026, 11, 2)), target, null, Yes, Yes, Yes),
            Item(Week(Date(2026, 11, 9)), target, Paused, Yes, Yes),
            Item(Week(Date(2026, 11, 16)), target, null, Yes));

        Assert.Equal(
            [
                Bar(Date(2026, 11, 2), Date(2026, 11, 8), 3, 3),
                Bar(Date(2026, 11, 9), Date(2026, 11, 15), null, 3, TrendMark.Paused),
                Bar(Date(2026, 11, 16), Date(2026, 11, 22), 1, 3),
            ],
            trend.Bars);
        Assert.Equal(3, trend.Target);
    }

    [Fact]
    public void A_Paused_day_with_a_kept_record_has_no_value()
    {
        var target = Target(CountTimes(1, "sweets", 1, SchedulePeriod.Day, BuildOrReduce.Reduce));

        var trend = Build(target, Item(Day(Date(2026, 11, 16)), target, Paused, Amount(4)), Item(Day(Date(2026, 11, 17)), target, null, Amount(0)));

        Assert.Equal(
            [Bar(Date(2026, 11, 16), Date(2026, 11, 16), null, 1, TrendMark.Paused), Bar(Date(2026, 11, 17), Date(2026, 11, 17), 0, 1)],
            trend.Bars);
    }

    [Fact]
    public void A_marked_commitment_keeps_the_target_of_its_own_version()
    {
        var older = Target(CountDays(5, "pages", BuildOrReduce.Build, MonWedFri));
        var later = Target(CountDays(8, "pages", BuildOrReduce.Build, MonWedFri));

        var trend = Build(later, Item(Day(Date(2026, 11, 16)), older, Excused), Item(Day(Date(2026, 11, 18)), later, null, Amount(6)));

        Assert.Equal(
            [Bar(Date(2026, 11, 16), Date(2026, 11, 16), null, 5, TrendMark.Excused), Bar(Date(2026, 11, 18), Date(2026, 11, 18), 6, 8)],
            trend.Bars);
        Assert.Equal(8, trend.Target);
    }

    [Fact]
    public void A_latest_marked_bar_leaves_the_line_at_the_previous_unmarked_bar_s_target()
    {
        var older = Target(CountDays(5, "pages", BuildOrReduce.Build, MonWedFri));
        var later = Target(CountDays(8, "pages", BuildOrReduce.Build, MonWedFri));

        var trend = Build(later, Item(Day(Date(2026, 11, 16)), older, null, Amount(5)), Item(Day(Date(2026, 11, 18)), later, Paused));

        Assert.Equal(5, trend.Target);
    }

    [Fact]
    public void Every_bar_marked_gives_the_line_0()
    {
        var target = Meditation10Daily;

        var trend = Build(target, Item(Day(Date(2026, 11, 16)), target, Excused), Item(Day(Date(2026, 11, 17)), target, Paused, Amount(10)));

        Assert.Equal(0, trend.Target);
        Assert.All(trend.Bars, bar => Assert.NotNull(bar.Mark));
        Assert.All(trend.Bars, bar => Assert.Equal(10, bar.Target));
    }

    // Commitment-based yes/no weeks.

    [Fact]
    public void A_Build_week_with_one_Excused_and_one_Paused_commitment_of_three_counts_the_third_only()
    {
        var target = OnDays(MonWedFri);

        var trend = Build(
            target,
            Item(Day(Date(2026, 11, 9)), target, Excused),
            Item(Day(Date(2026, 11, 11)), target, Paused),
            Item(Day(Date(2026, 11, 13)), target, null, Yes));

        Assert.Equal(Bar(Date(2026, 11, 9), Date(2026, 11, 15), 1, 1), Assert.Single(trend.Bars));
        Assert.Equal(1, trend.Target);
    }

    [Fact]
    public void FR_016_a_Yes_kept_on_a_Paused_commitment_is_not_counted()
    {
        var target = OnDays(MonWedFri);

        var trend = Build(
            target,
            Item(Day(Date(2026, 11, 9)), target, null, Yes),
            Item(Day(Date(2026, 11, 11)), target, null, No),
            Item(Day(Date(2026, 11, 13)), target, Paused, Yes));

        Assert.Equal(Bar(Date(2026, 11, 9), Date(2026, 11, 15), 1, 2), Assert.Single(trend.Bars));
    }

    [Fact]
    public void FR_016_a_week_whose_only_records_sit_on_Paused_commitments_has_no_value_not_0()
    {
        var target = OnDays(MonWedFri);

        var trend = Build(
            target,
            Item(Day(Date(2026, 11, 9)), target, null),
            Item(Day(Date(2026, 11, 11)), target, Paused, Yes),
            Item(Day(Date(2026, 11, 13)), target, Paused, No));

        Assert.Equal(Bar(Date(2026, 11, 9), Date(2026, 11, 15), null, 1), Assert.Single(trend.Bars));
    }

    [Fact]
    public void Critic_MINOR_1_a_Reduce_week_counts_the_Yes_of_its_counted_commitments_only_against_0()
    {
        var target = Target(YesNoDays(BuildOrReduce.Reduce, MonWedFri));

        var trend = Build(
            target,
            Item(Day(Date(2026, 11, 9)), target, Excused, Yes),
            Item(Day(Date(2026, 11, 11)), target, null, Yes),
            Item(Day(Date(2026, 11, 13)), target, null, No));

        Assert.Equal(Bar(Date(2026, 11, 9), Date(2026, 11, 15), 1, 0), Assert.Single(trend.Bars));
        Assert.Equal(0, trend.Target);
    }

    [Fact]
    public void A_Reduce_week_whose_only_record_sits_on_a_Paused_commitment_has_no_value()
    {
        var target = Target(YesNoDays(BuildOrReduce.Reduce, MonWedFri));

        var trend = Build(
            target,
            Item(Day(Date(2026, 11, 9)), target, Paused, Yes),
            Item(Day(Date(2026, 11, 11)), target, null),
            Item(Day(Date(2026, 11, 13)), target, null));

        Assert.Equal(Bar(Date(2026, 11, 9), Date(2026, 11, 15), null, 0), Assert.Single(trend.Bars));
    }

    [Fact]
    public void An_all_exempt_week_with_a_Paused_commitment_is_marked_Paused_with_the_target_0()
    {
        var target = OnDays(MonWedFri);

        var trend = Build(
            target,
            Item(Day(Date(2026, 11, 9)), target, Excused),
            Item(Day(Date(2026, 11, 11)), target, Paused, Yes),
            Item(Day(Date(2026, 11, 13)), target, Paused));

        Assert.Equal(Bar(Date(2026, 11, 9), Date(2026, 11, 15), null, 0, TrendMark.Paused), Assert.Single(trend.Bars));
        Assert.Equal(0, trend.Target);
    }

    [Fact]
    public void An_all_exempt_week_with_no_Paused_commitment_is_marked_Excused_with_the_target_0()
    {
        var target = OnDays(MonWedFri);

        var trend = Build(
            target,
            Item(Day(Date(2026, 11, 9)), target, Excused),
            Item(Day(Date(2026, 11, 11)), target, Excused),
            Item(Day(Date(2026, 11, 13)), target, Excused));

        Assert.Equal(Bar(Date(2026, 11, 9), Date(2026, 11, 15), null, 0, TrendMark.Excused), Assert.Single(trend.Bars));
    }

    [Fact]
    public void A_current_week_with_an_Excused_commitment_and_an_Upcoming_one_that_is_not_Paused_keeps_an_unmarked_bar()
    {
        // Read on Tuesday 17 November: Monday 16 is Excused, Wednesday 18 is Upcoming and not paused.
        var target = OnDays(DayOfWeek.Monday, DayOfWeek.Wednesday);

        var trend = TrendBuilder.Of(target, [Item(Day(Date(2026, 11, 16)), target, Excused)], [Day(Date(2026, 11, 18))], GoalExemptions.None);

        Assert.Equal(Bar(Date(2026, 11, 16), Date(2026, 11, 22), null, 1), Assert.Single(trend.Bars));
        Assert.Equal(1, trend.Target);
    }

    [Fact]
    public void A_current_week_with_an_Excused_begun_commitment_and_Paused_Upcoming_ones_is_marked_Paused()
    {
        // Read on Tuesday 17 November: Monday 16 is Excused; Wednesday 18 and Friday 20 are Upcoming and paused.
        var target = OnDays(MonWedFri);
        var exemptions = PausedFrom(Date(2026, 11, 18), Date(2026, 11, 22));

        var trend = TrendBuilder.Of(
            target, [Item(Day(Date(2026, 11, 16)), target, Excused)], [Day(Date(2026, 11, 18)), Day(Date(2026, 11, 20))], exemptions);

        Assert.Equal(Bar(Date(2026, 11, 16), Date(2026, 11, 22), null, 0, TrendMark.Paused), Assert.Single(trend.Bars));
    }

    [Fact]
    public void A_Paused_Upcoming_commitment_of_the_current_week_leaves_the_Build_target()
    {
        // Read on Tuesday 17 November: Monday 16 is recorded; Wednesday 18 is Upcoming and not paused, Friday 20 paused.
        var target = OnDays(MonWedFri);
        var exemptions = PausedFrom(Date(2026, 11, 20), Date(2026, 11, 20));

        var trend = TrendBuilder.Of(
            target, [Item(Day(Date(2026, 11, 16)), target, null, Yes)], [Day(Date(2026, 11, 18)), Day(Date(2026, 11, 20))], exemptions);

        Assert.Equal(Bar(Date(2026, 11, 16), Date(2026, 11, 22), 1, 2), Assert.Single(trend.Bars));
    }

    [Fact]
    public void A_latest_all_exempt_week_leaves_the_line_at_the_previous_week_s_target()
    {
        var target = OnDays(MonWedFri);

        var trend = Build(
            target,
            Item(Day(Date(2026, 11, 2)), target, null, Yes),
            Item(Day(Date(2026, 11, 4)), target, null, Yes),
            Item(Day(Date(2026, 11, 6)), target, null, No),
            Item(Day(Date(2026, 11, 9)), target, Paused),
            Item(Day(Date(2026, 11, 11)), target, Paused),
            Item(Day(Date(2026, 11, 13)), target, Paused));

        Assert.Equal(
            [Bar(Date(2026, 11, 2), Date(2026, 11, 8), 2, 3), Bar(Date(2026, 11, 9), Date(2026, 11, 15), null, 0, TrendMark.Paused)],
            trend.Bars);
        Assert.Equal(3, trend.Target);
    }

    [Fact]
    public void Every_week_all_exempt_gives_the_line_0()
    {
        var target = OnDays(DayOfWeek.Monday);

        var trend = Build(target, Item(Day(Date(2026, 11, 2)), target, Excused), Item(Day(Date(2026, 11, 9)), target, Paused));

        Assert.Equal(0, trend.Target);
        Assert.Equal([TrendMark.Excused, TrendMark.Paused], trend.Bars.Select(bar => bar.Mark));
    }

    // Refused input.

    [Theory]
    [InlineData(EvaluationState.Due, "per day")]
    [InlineData(EvaluationState.Pending, "commitment yes/no")]
    [InlineData(EvaluationState.Met, "per day")]
    [InlineData(EvaluationState.Missed, "commitment yes/no")]
    public void Only_Excused_or_Paused_can_mark_a_bar(EvaluationState exemption, string goal)
    {
        var target = goal == "per day" ? PerDay() : OnDays(MonWedFri);

        Assert.Throws<ArgumentOutOfRangeException>(() => Build(target, Item(Day(Date(2026, 11, 16)), target, exemption, Yes)));
    }

    // The old overload.

    /// <summary>The begun entries and the current week's Upcoming ones of each case of <see cref="TrendBuilderTests"/>.</summary>
    private static (GoalTarget Latest, (CalendarEntry Entry, GoalTarget Target, IReadOnlyList<RecordValue> Records)[] Begun, CalendarEntry[] Upcoming) Case(string name)
    {
        static (CalendarEntry, GoalTarget, IReadOnlyList<RecordValue>) Old(CalendarEntry entry, GoalTarget target, params RecordValue[] records) =>
            (entry, target, records);

        var reading = Target(DurationDays(20, DurationUnit.Minutes, BuildOrReduce.Build, AllDays));
        var pagesBuild = Target(CountDays(5, "pages", BuildOrReduce.Build, MonWedFri));
        var pagesLater = Target(CountDays(8, "pages", BuildOrReduce.Build, MonWedFri));
        var pagesReduce = Target(CountDays(5, "pages", BuildOrReduce.Reduce, MonWedFri));
        var hours = Target(DurationDays(1, DurationUnit.Hours, BuildOrReduce.Build, MonWedFri));
        var perWeek = PerWeek();
        var weekDuration = Target(DurationTimes(30, DurationUnit.Minutes, 3, SchedulePeriod.Week));
        var reduceYesNoWeek = Target(YesNoTimes(2, SchedulePeriod.Week, BuildOrReduce.Reduce));
        var reduceCountWeek = Target(CountTimes(10, "pages", 1, SchedulePeriod.Week, BuildOrReduce.Reduce));
        var dayDuration = Target(DurationTimes(20, DurationUnit.Minutes, 2, SchedulePeriod.Day));
        var sweets = Target(CountTimes(1, "sweets", 1, SchedulePeriod.Day, BuildOrReduce.Reduce));
        var reduceHoursDay = Target(DurationTimes(2, DurationUnit.Hours, 1, SchedulePeriod.Day, BuildOrReduce.Reduce));
        var monWedFri = OnDays(MonWedFri);
        var reduceYesNoDays = Target(YesNoDays(BuildOrReduce.Reduce, MonWedFri));
        var tueThu = OnDays(DayOfWeek.Tuesday, DayOfWeek.Thursday);
        var monday = OnDays(DayOfWeek.Monday);
        var perDay = PerDay();

        return name switch
        {
            "daily duration" => (reading,
                [Old(Day(Date(2026, 11, 12)), reading, Amount(20)), Old(Day(Date(2026, 11, 13)), reading, Amount(12)),
                    Old(Day(Date(2026, 11, 14)), reading), Old(Day(Date(2026, 11, 15)), reading, Amount(25))], []),
            "count 0 and none" => (pagesBuild, [Old(Day(Date(2026, 11, 16)), pagesBuild, Amount(0)), Old(Day(Date(2026, 11, 18)), pagesBuild)], []),
            "above target" => (reading, [Old(Day(Date(2026, 11, 16)), reading, Amount(45))], []),
            "hours" => (hours, [Old(Day(Date(2026, 11, 16)), hours, Amount(75)), Old(Day(Date(2026, 11, 18)), hours, Amount(30))], []),
            "count build" => (pagesBuild, [Old(Day(Date(2026, 11, 16)), pagesBuild, Amount(3))], []),
            "count reduce" => (pagesReduce, [Old(Day(Date(2026, 11, 16)), pagesReduce, Amount(3))], []),
            "own targets" => (pagesLater, [Old(Day(Date(2026, 11, 16)), pagesBuild, Amount(5)), Old(Day(Date(2026, 11, 18)), pagesLater, Amount(6))], []),
            "per week" => (perWeek,
                [Old(Week(Date(2026, 11, 2)), perWeek, Yes, Yes, No), Old(Week(Date(2026, 11, 9)), perWeek), Old(Week(Date(2026, 11, 16)), perWeek, No)], []),
            "week duration" => (weekDuration, [Old(Week(Date(2026, 11, 9)), weekDuration, Amount(30), Amount(25), Amount(40))], []),
            "reduce yes/no week" => (reduceYesNoWeek, [Old(Week(Date(2026, 11, 9)), reduceYesNoWeek, Yes, No, Yes, Yes)], []),
            "reduce count week" => (reduceCountWeek,
                [Old(Week(Date(2026, 11, 2)), reduceCountWeek, Amount(4), Amount(7)), Old(Week(Date(2026, 11, 9)), reduceCountWeek)], []),
            "per day duration" => (dayDuration,
                [Old(Day(Date(2026, 11, 16)), dayDuration, Amount(20), Amount(25)), Old(Day(Date(2026, 11, 17)), dayDuration, Amount(19)),
                    Old(Day(Date(2026, 11, 18)), dayDuration)], []),
            "reduce count day" => (sweets,
                [Old(Day(Date(2026, 11, 16)), sweets, Amount(2)), Old(Day(Date(2026, 11, 17)), sweets), Old(Day(Date(2026, 11, 18)), sweets, Amount(0))], []),
            "reduce hours day" => (reduceHoursDay, [Old(Day(Date(2026, 11, 16)), reduceHoursDay, Amount(90), Amount(40))], []),
            "yes/no weeks" => (monWedFri,
                [Old(Day(Date(2026, 11, 2)), monWedFri, Yes), Old(Day(Date(2026, 11, 4)), monWedFri, Yes), Old(Day(Date(2026, 11, 6)), monWedFri, No),
                    Old(Day(Date(2026, 11, 9)), monWedFri), Old(Day(Date(2026, 11, 11)), monWedFri), Old(Day(Date(2026, 11, 13)), monWedFri)], []),
            "reduce yes/no days" => (reduceYesNoDays,
                [Old(Day(Date(2026, 11, 9)), reduceYesNoDays, Yes), Old(Day(Date(2026, 11, 11)), reduceYesNoDays, No),
                    Old(Day(Date(2026, 11, 13)), reduceYesNoDays, No)], []),
            "only No" => (monWedFri, [Old(Day(Date(2026, 11, 9)), monWedFri, No), Old(Day(Date(2026, 11, 11)), monWedFri)], []),
            "Tuesday and Thursday" => (tueThu,
                [Old(Day(Date(2026, 11, 3)), tueThu, Yes), Old(Day(Date(2026, 11, 5)), tueThu, Yes), Old(Day(Date(2026, 11, 10)), tueThu, Yes),
                    Old(Day(Date(2026, 11, 12)), tueThu)],
                [Day(Date(2026, 11, 17)), Day(Date(2026, 11, 19))]),
            "smaller first week" => (monWedFri,
                [Old(Day(Date(2026, 11, 4)), monWedFri, Yes), Old(Day(Date(2026, 11, 6)), monWedFri, Yes), Old(Day(Date(2026, 11, 9)), monWedFri, Yes),
                    Old(Day(Date(2026, 11, 11)), monWedFri, Yes), Old(Day(Date(2026, 11, 13)), monWedFri, No), Old(Day(Date(2026, 11, 16)), monWedFri, Yes)],
                [Day(Date(2026, 11, 18)), Day(Date(2026, 11, 20))]),
            "reduce upcoming" => (reduceYesNoDays, [Old(Day(Date(2026, 11, 16)), reduceYesNoDays, No)], [Day(Date(2026, 11, 18)), Day(Date(2026, 11, 20))]),
            "14 days" => (perDay, [.. Enumerable.Range(1, 20).Select(day => Old(Day(Date(2026, 11, day)), perDay, Yes))], []),
            "14 commitments" => (reading, [.. Enumerable.Range(1, 20).Select(day => Old(Day(Date(2026, 11, day)), reading, Amount(day)))], []),
            "12 weeks" => (perWeek, [.. Enumerable.Range(0, 15).Select(week => Old(Week(Date(2026, 8, 10).AddDays(7 * week)), perWeek, Yes))], []),
            "12 yes/no weeks" => (monday, [.. Enumerable.Range(0, 15).Select(week => Old(Day(Date(2026, 8, 10).AddDays(7 * week)), monday, Yes))], []),
            "none, yes/no" => (monWedFri, [], [Day(Date(2026, 11, 18))]),
            "none, duration" => (reading, [], [Day(Date(2026, 11, 18))]),
            "none, per day" => (perDay, [], [Day(Date(2026, 11, 18))]),
            "none, per week" => (perWeek, [], [Day(Date(2026, 11, 18))]),
            _ => throw new ArgumentOutOfRangeException(nameof(name)),
        };
    }

    public static TheoryData<string> OldCases() => new()
    {
        "daily duration", "count 0 and none", "above target", "hours", "count build", "count reduce", "own targets",
        "per week", "week duration", "reduce yes/no week", "reduce count week",
        "per day duration", "reduce count day", "reduce hours day",
        "yes/no weeks", "reduce yes/no days", "only No", "Tuesday and Thursday", "smaller first week", "reduce upcoming",
        "14 days", "14 commitments", "12 weeks", "12 yes/no weeks",
        "none, yes/no", "none, duration", "none, per day", "none, per week",
    };

    [Theory]
    [MemberData(nameof(OldCases))]
    public void The_old_overload_gives_the_bars_of_the_new_one_with_no_exemption(string name)
    {
        var (latest, begun, upcoming) = Case(name);

        var old = TrendBuilder.Of(latest, begun, upcoming);
        var unmarked = TrendBuilder.Of(
            latest, begun.Select(item => (item.Entry, item.Target, item.Records, (EvaluationState?)null)).ToArray(), upcoming, GoalExemptions.None);

        Assert.Equal(old.Kind, unmarked.Kind);
        Assert.Equal(old.Span, unmarked.Span);
        Assert.Equal(old.MaxBars, unmarked.MaxBars);
        Assert.Equal(old.Target, unmarked.Target);
        Assert.Equal(old.Bars, unmarked.Bars);
        Assert.All(old.Bars, bar => Assert.Null(bar.Mark));
    }

    [Fact]
    public void Exemptions_of_dates_outside_the_current_week_s_Upcoming_commitments_change_nothing_in_begun_items()
    {
        // The goal's exemptions are read only for the Upcoming commitments: a begun item's own exemption decides its bar.
        var target = OnDays(MonWedFri);
        var exemptions = PausedFrom(Date(2026, 11, 9), Date(2026, 11, 9));

        var trend = TrendBuilder.Of(target, [Item(Day(Date(2026, 11, 9)), target, null, Yes)], [], exemptions);

        Assert.Equal(Bar(Date(2026, 11, 9), Date(2026, 11, 15), 1, 1), Assert.Single(trend.Bars));
    }
}
