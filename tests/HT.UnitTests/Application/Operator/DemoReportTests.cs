using HT.Application.Operator;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;

namespace HT.UnitTests.Application.Operator;

/// <summary>
/// Slice 6 FR-032, research R-09: the three goals of <c>demo5@example.com</c>, with their records and notes relative to
/// the run date R. The run dates are the seven days of the week of Monday 16 November 2026, so the current week's
/// Monday is 16 November for each of them, and week w starts 7 × w days before it: week 1 on 9 November, week 2 on
/// 2 November, week 4 on 19 October, week 7 on 28 September, week 11 on 31 August and week 12 on 24 August, which is
/// R − 84 only for Monday 16.
/// </summary>
public sealed class DemoReportTests
{
    private static readonly DateOnly CurrentMonday = new(2026, 11, 16);

    public static TheoryData<DateOnly, DayOfWeek> RunDates => new()
    {
        { new DateOnly(2026, 11, 16), DayOfWeek.Monday },
        { new DateOnly(2026, 11, 17), DayOfWeek.Tuesday },
        { new DateOnly(2026, 11, 18), DayOfWeek.Wednesday },
        { new DateOnly(2026, 11, 19), DayOfWeek.Thursday },
        { new DateOnly(2026, 11, 20), DayOfWeek.Friday },
        { new DateOnly(2026, 11, 21), DayOfWeek.Saturday },
        { new DateOnly(2026, 11, 22), DayOfWeek.Sunday },
    };

    private static DateOnly WeekMonday(int week) => CurrentMonday.AddDays(-7 * week);

    private static bool IsWalkingDay(DateOnly date) => date.DayOfWeek is DayOfWeek.Monday or DayOfWeek.Wednesday or DayOfWeek.Friday;

    [Theory]
    [MemberData(nameof(RunDates))]
    public void The_three_goals_are_Walking_Strength_training_and_Screen_time_on_Published_habits_starting_84_days_before_R(DateOnly runDate, DayOfWeek day)
    {
        Assert.Equal(day, runDate.DayOfWeek);

        var goals = DemoReport.For(runDate);

        Assert.Equal(3, goals.Count);
        var start = runDate.AddDays(-84);
        AssertGoal(goals[0], "Walking", start, new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = 30, DurationUnit = DurationUnit.Minutes,
            ScheduleKind = ScheduleKind.FixedDays, Days = new HashSet<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
        });
        AssertGoal(goals[1], "Strength training", start, new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo,
            ScheduleKind = ScheduleKind.Times, Times = 2, Period = SchedulePeriod.Week,
        });
        AssertGoal(goals[2], "Screen time", start, new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Reduce, Measurement = Measurement.Duration, Amount = 120, DurationUnit = DurationUnit.Minutes,
            ScheduleKind = ScheduleKind.Times, Times = 1, Period = SchedulePeriod.Day,
        });
    }

    [Theory]
    [MemberData(nameof(RunDates))]
    public void Walking_has_30_minutes_on_each_commitment_from_R_minus_84_to_R_minus_3_except_35_18_and_none_and_the_holiday_note(
        DateOnly runDate, DayOfWeek day)
    {
        Assert.Equal(day, runDate.DayOfWeek);

        var walking = DemoReport.For(runDate)[0];

        List<(DateOnly, int?)> expected = [];
        for (var k = 84; k >= 3; k--)
        {
            var date = runDate.AddDays(-k);
            if (!IsWalkingDay(date) || k is >= 43 and <= 49)
            {
                continue;
            }

            expected.Add((date, k switch
            {
                >= 9 and <= 15 => 35,
                >= 20 and <= 26 => 18,
                _ => 30,
            }));
        }

        Assert.Equal(expected, walking.Records.Select(r => (r.Date, r.Value.Amount)));
        Assert.All(walking.Records, r => Assert.Null(r.Value.Answer));
        Assert.DoesNotContain(walking.Records, r => r.Date >= runDate.AddDays(-2));
        var holiday = Enumerable.Range(43, 7).Select(k => runDate.AddDays(-k)).Where(IsWalkingDay).Max();
        Assert.Equal([(holiday, "On holiday.")], walking.Notes.Select(n => (n.EntryDate, n.Text.Value)));
    }

    [Fact]
    public void On_Wednesday_18_November_Walkings_holiday_note_is_on_R_minus_44_and_its_short_and_long_walks_are_those_of_the_spec()
    {
        var runDate = new DateOnly(2026, 11, 18);

        var walking = DemoReport.For(runDate)[0];

        Assert.Equal(new DateOnly(2026, 10, 5), Assert.Single(walking.Notes).EntryDate);
        // R − 21, R − 23 and R − 26 (28, 26 and 23 October) are the 18-minute walks; R − 44, R − 47 and R − 49 have no record.
        Assert.Equal(
            [new DateOnly(2026, 10, 23), new DateOnly(2026, 10, 26), new DateOnly(2026, 10, 28)],
            walking.Records.Where(r => r.Value.Amount == 18).Select(r => r.Date));
        Assert.DoesNotContain(walking.Records, r => r.Date >= new DateOnly(2026, 9, 30) && r.Date <= new DateOnly(2026, 10, 5));
        // 35 final commitments from Wednesday 26 August to Friday 13 November, 3 of them without a record.
        Assert.Equal(32, walking.Records.Count);
    }

    [Theory]
    [MemberData(nameof(RunDates))]
    public void Strength_training_has_Monday_and_Thursday_in_each_week_except_weeks_2_4_and_7_and_week_12_only_on_a_Monday(
        DateOnly runDate, DayOfWeek day)
    {
        Assert.Equal(day, runDate.DayOfWeek);

        var strength = DemoReport.For(runDate)[1];

        List<DateOnly> expected = [];
        foreach (var week in Enumerable.Range(1, 12).Reverse())
        {
            if (week == 12 && day != DayOfWeek.Monday)
            {
                continue;
            }

            var monday = WeekMonday(week);
            expected.AddRange(week switch
            {
                7 => [],
                4 => [monday],
                2 => [monday, monday.AddDays(2), monday.AddDays(3)],
                _ => [monday, monday.AddDays(3)],
            });
        }

        Assert.Equal(expected, strength.Records.Select(r => r.Date));
        Assert.Equal(day == DayOfWeek.Monday ? 22 : 20, strength.Records.Count);
        Assert.All(strength.Records, r =>
        {
            Assert.Equal(YesNoAnswer.Yes, r.Value.Answer);
            Assert.Null(r.Value.Amount);
        });
        Assert.DoesNotContain(strength.Records, r => r.Date >= CurrentMonday);
        Assert.DoesNotContain(strength.Records, r => r.Date >= WeekMonday(7) && r.Date < WeekMonday(6));
        Assert.Equal([(new DateOnly(2026, 10, 19), "Sore shoulder.")], strength.Notes.Select(n => (n.EntryDate, n.Text.Value)));
    }

    [Theory]
    [MemberData(nameof(RunDates))]
    public void Screen_time_has_90_and_110_from_R_minus_84_to_R_minus_4_except_150_and_none_then_100_on_R_minus_2(
        DateOnly runDate, DayOfWeek day)
    {
        Assert.Equal(day, runDate.DayOfWeek);

        var screen = DemoReport.For(runDate)[2];

        List<(DateOnly, int?)> expected = [];
        for (var k = 84; k >= 4; k--)
        {
            if (k != 50)
            {
                expected.Add((runDate.AddDays(-k), k is >= 30 and <= 32 ? 150 : k % 2 == 0 ? 90 : 110));
            }
        }

        expected.Add((runDate.AddDays(-2), 100));
        Assert.Equal(expected, screen.Records.Select(r => (r.Date, r.Value.Amount)));
        Assert.All(screen.Records, r => Assert.Null(r.Value.Answer));
        Assert.DoesNotContain(screen.Records, r => r.Date == runDate.AddDays(-3) || r.Date == runDate.AddDays(-1));
        Assert.Equal([(runDate.AddDays(-31), "Long train ride.")], screen.Notes.Select(n => (n.EntryDate, n.Text.Value)));
    }

    [Theory]
    [MemberData(nameof(RunDates))]
    public void Every_record_is_on_a_date_before_R_and_on_or_after_the_start_in_date_order(DateOnly runDate, DayOfWeek day)
    {
        Assert.Equal(day, runDate.DayOfWeek);

        foreach (var goal in DemoReport.For(runDate))
        {
            Assert.All(goal.Records, r => Assert.InRange(r.Date, goal.Goal.StartDate, runDate.AddDays(-1)));
            Assert.All(goal.Notes, n => Assert.InRange(n.EntryDate, goal.Goal.StartDate, runDate.AddDays(-1)));
            Assert.Equal(goal.Records.Select(r => r.Date).Order(), goal.Records.Select(r => r.Date));
            Assert.Equal(goal.Records.Count, goal.Records.Select(r => r.Date).Distinct().Count());
            Assert.Equal(goal.Notes.Count, goal.Notes.Select(n => n.EntryDate).Distinct().Count());
        }
    }

    [Fact]
    public void Each_record_value_passes_the_value_checks_of_its_goals_measurement()
    {
        foreach (var goal in DemoReport.For(new DateOnly(2026, 11, 16)))
        {
            Assert.All(goal.Records, r =>
            {
                Assert.True(RecordValue.TryCreate(goal.Goal.Target.Measurement, r.Value.Answer, r.Value.Amount, out var value, out _));
                Assert.Equal(r.Value, value);
            });
        }
    }

    [Fact]
    public void The_goal_names_are_the_display_names_in_creation_order()
    {
        Assert.Equal(
            ["Walking, 30 min, 3 days a week", "Strength training, 2 times a week", "Screen time, at most 120 min a day"],
            DemoReport.GoalNames);
        Assert.Equal(
            DemoReport.For(new DateOnly(2026, 11, 18)).Select(g => g.Goal.CatalogHabitName + ", " + GoalText.TargetText(g.Goal.Target)),
            DemoReport.GoalNames);
    }

    [Fact]
    public void The_note_texts_are_those_of_FR_032()
    {
        Assert.Equal("On holiday.", DemoReport.WalkingNote);
        Assert.Equal("Sore shoulder.", DemoReport.StrengthNote);
        Assert.Equal("Long train ride.", DemoReport.ScreenTimeNote);
    }

    private static void AssertGoal(DemoHistoryGoal goal, string habitName, DateOnly start, GoalTargetFields fields)
    {
        Assert.True(GoalTarget.TryCreate(fields, out var target, out _));
        Assert.Equal(habitName, goal.Goal.CatalogHabitName);
        Assert.Null(goal.Goal.CustomHabitPosition);
        Assert.Equal(new[] { CatalogHabitState.Published }, goal.Goal.AcceptedStates);
        Assert.Equal(target, goal.Goal.Target);
        Assert.Null(goal.Goal.OwnName);
        Assert.Equal(start, goal.Goal.StartDate);
    }
}
