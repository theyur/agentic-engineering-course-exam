using HT.Application.Operator;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;

namespace HT.UnitTests.Application.Operator;

/// <summary>
/// Slice 5 FR-030, research R-08: the three goals of the demo account with history, with their records and notes
/// relative to the run date R. The run dates are the seven days of the week of Monday 16 November 2026, so the
/// current week's Monday is 16 November for each of them; week 1 then starts on Monday 9 November, week 2 on 2
/// November, week 3 on 26 October, and the week before week 3 on 19 October, which is R − 28 only for Monday 16.
/// </summary>
public sealed class DemoHistoryTests
{
    private static readonly DayOfWeek[] EveryDay =
    [
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday,
        DayOfWeek.Saturday, DayOfWeek.Sunday,
    ];

    /// <summary>Yoga's Yes dates in week 2 (Monday and Wednesday) and week 1 (Monday, Wednesday and Friday).</summary>
    private static readonly DateOnly[] YogaWeeks2And1 =
    [
        new(2026, 11, 2), new(2026, 11, 4),
        new(2026, 11, 9), new(2026, 11, 11), new(2026, 11, 13),
    ];

    /// <summary>Yoga's Yes dates in the week before week 3, written only when R is Monday 16 November.</summary>
    private static readonly DateOnly[] YogaWeekBeforeWeek3 = [new(2026, 10, 19), new(2026, 10, 21), new(2026, 10, 23)];

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

    [Theory]
    [MemberData(nameof(RunDates))]
    public void The_three_goals_are_Reading_Sugar_and_Yoga_on_Published_habits_starting_28_days_before_R(DateOnly runDate, DayOfWeek day)
    {
        Assert.Equal(day, runDate.DayOfWeek);

        var goals = DemoHistory.For(runDate);

        Assert.Equal(3, goals.Count);
        var start = runDate.AddDays(-28);
        AssertGoal(goals[0], "Reading", start, new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = 20, DurationUnit = DurationUnit.Minutes,
            ScheduleKind = ScheduleKind.FixedDays, Days = EveryDay.ToHashSet(),
        });
        AssertGoal(goals[1], "Sugar", start, new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Reduce, Measurement = Measurement.Count, Amount = 1, CountUnit = "sweet",
            ScheduleKind = ScheduleKind.Times, Times = 1, Period = SchedulePeriod.Day,
        });
        AssertGoal(goals[2], "Yoga", start, new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo,
            ScheduleKind = ScheduleKind.Times, Times = 3, Period = SchedulePeriod.Week,
        });
    }

    [Theory]
    [MemberData(nameof(RunDates))]
    public void Reading_has_20_minutes_from_R_minus_28_to_R_minus_3_except_12_on_R_minus_6_and_none_on_R_minus_10_then_25_on_R_minus_2(
        DateOnly runDate, DayOfWeek day)
    {
        Assert.Equal(day, runDate.DayOfWeek);

        var reading = DemoHistory.For(runDate)[0];

        List<(DateOnly, int?)> expected = [];
        for (var k = 28; k >= 3; k--)
        {
            if (k != 10)
            {
                expected.Add((runDate.AddDays(-k), k == 6 ? 12 : 20));
            }
        }

        expected.Add((runDate.AddDays(-2), 25));
        Assert.Equal(expected, reading.Records.Select(r => (r.Date, r.Value.Amount)));
        Assert.All(reading.Records, r => Assert.Null(r.Value.Answer));
        Assert.Equal(
            [(runDate.AddDays(-6), "Fell asleep early."), (runDate.AddDays(-10), "Away for the weekend.")],
            reading.Notes.Select(n => (n.EntryDate, n.Text.Value)));
    }

    [Theory]
    [MemberData(nameof(RunDates))]
    public void Sugar_has_0_on_even_and_1_on_odd_k_from_R_minus_28_to_R_minus_3_except_3_on_R_minus_4_and_none_on_R_minus_12(
        DateOnly runDate, DayOfWeek day)
    {
        Assert.Equal(day, runDate.DayOfWeek);

        var sugar = DemoHistory.For(runDate)[1];

        List<(DateOnly, int?)> expected = [];
        for (var k = 28; k >= 3; k--)
        {
            if (k != 12)
            {
                expected.Add((runDate.AddDays(-k), k == 4 ? 3 : k % 2));
            }
        }

        Assert.Equal(expected, sugar.Records.Select(r => (r.Date, r.Value.Amount)));
        Assert.All(sugar.Records, r => Assert.Null(r.Value.Answer));
        Assert.Equal([(runDate.AddDays(-4), "Birthday cake at work.")], sugar.Notes.Select(n => (n.EntryDate, n.Text.Value)));
    }

    [Theory]
    [MemberData(nameof(RunDates))]
    public void Yoga_has_Yes_in_weeks_2_and_1_the_note_under_week_2_and_the_week_before_week_3_only_when_R_is_a_Monday(
        DateOnly runDate, DayOfWeek day)
    {
        Assert.Equal(day, runDate.DayOfWeek);

        var yoga = DemoHistory.For(runDate)[2];

        DateOnly[] expected = day == DayOfWeek.Monday ? [.. YogaWeekBeforeWeek3, .. YogaWeeks2And1] : YogaWeeks2And1;
        Assert.Equal(expected, yoga.Records.Select(r => r.Date));
        Assert.All(yoga.Records, r =>
        {
            Assert.Equal(YesNoAnswer.Yes, r.Value.Answer);
            Assert.Null(r.Value.Amount);
        });
        Assert.Equal([(new DateOnly(2026, 11, 2), "Knee was sore.")], yoga.Notes.Select(n => (n.EntryDate, n.Text.Value)));
    }

    [Theory]
    [MemberData(nameof(RunDates))]
    public void Every_record_is_on_a_date_before_R_and_on_or_after_the_start_in_date_order(DateOnly runDate, DayOfWeek day)
    {
        Assert.Equal(day, runDate.DayOfWeek);

        foreach (var goal in DemoHistory.For(runDate))
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
        foreach (var goal in DemoHistory.For(new DateOnly(2026, 11, 16)))
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
        Assert.Equal(["Reading, 20 min daily", "Sugar, at most 1 sweet a day", "Yoga, 3 times a week"], DemoHistory.GoalNames);
        Assert.Equal(
            DemoHistory.For(new DateOnly(2026, 11, 18)).Select(g => g.Goal.CatalogHabitName + ", " + GoalText.TargetText(g.Goal.Target)),
            DemoHistory.GoalNames);
    }

    [Fact]
    public void The_partial_minutes_and_the_reading_note_are_those_of_R_minus_6()
    {
        Assert.Equal(12, DemoHistory.PartialMinutes);
        Assert.Equal("Fell asleep early.", DemoHistory.ReadingNote);

        var runDate = new DateOnly(2026, 11, 18);
        var reading = DemoHistory.For(runDate)[0];
        Assert.Equal(DemoHistory.PartialMinutes, Assert.Single(reading.Records, r => r.Date == runDate.AddDays(-6)).Value.Amount);
        Assert.Equal(DemoHistory.ReadingNote, Assert.Single(reading.Notes, n => n.EntryDate == runDate.AddDays(-6)).Text.Value);
    }

    [Fact]
    public void A_demo_history_goal_is_on_a_catalog_habit()
    {
        Assert.True(GoalTarget.TryCreate(
            new GoalTargetFields
            {
                BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo,
                ScheduleKind = ScheduleKind.Times, Times = 2, Period = SchedulePeriod.Week,
            },
            out var target,
            out _));
        var custom = DemoGoal.OnCustomHabit(0, target, null, new DateOnly(2026, 11, 18));

        Assert.Throws<ArgumentException>(() => new DemoHistoryGoal(custom, [], []));
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
