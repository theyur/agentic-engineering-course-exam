using HT.Application.Operator;
using HT.Domain.Exemptions;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;

namespace HT.UnitTests.Application.Operator;

/// <summary>
/// Slice 7 FR-031, research R-08, R-11: the two goals of <c>demo6@example.com</c>, with their records, notes, excusals
/// and pauses relative to the run date R ("R − k" is the date k days before R). The run dates are the seven days of the
/// week of Monday 16 November 2026, so every weekday of R is covered.
/// </summary>
public sealed class DemoPausesTests
{
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
    public void The_two_goals_are_Meditation_and_Drinking_water_on_Published_habits_starting_28_days_before_R(DateOnly runDate, DayOfWeek day)
    {
        Assert.Equal(day, runDate.DayOfWeek);

        var goals = DemoPauses.For(runDate);

        Assert.Equal(2, goals.Count);
        var start = runDate.AddDays(-28);
        AssertGoal(goals[0].History, "Meditation", start, new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = 10, DurationUnit = DurationUnit.Minutes,
            ScheduleKind = ScheduleKind.FixedDays,
            Days = new HashSet<DayOfWeek>
            {
                DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday,
                DayOfWeek.Saturday, DayOfWeek.Sunday,
            },
        });
        AssertGoal(goals[1].History, "Drinking water", start, new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Count, Amount = 8, CountUnit = "glasses",
            ScheduleKind = ScheduleKind.Times, Times = 1, Period = SchedulePeriod.Day,
        });
    }

    [Theory]
    [MemberData(nameof(RunDates))]
    public void Meditation_has_10_minutes_from_R_minus_28_to_R_minus_3_except_R_minus_20_19_14_and_8_and_its_note_and_excusals(
        DateOnly runDate, DayOfWeek day)
    {
        Assert.Equal(day, runDate.DayOfWeek);

        var meditation = DemoPauses.For(runDate)[0];

        List<(DateOnly, int?)> expected = [];
        for (var k = 28; k >= 3; k--)
        {
            if (k is not (20 or 19 or 14 or 8))
            {
                expected.Add((runDate.AddDays(-k), 10));
            }
        }

        Assert.Equal(expected, meditation.History.Records.Select(r => (r.Date, r.Value.Amount)));
        Assert.Equal(22, meditation.History.Records.Count);
        Assert.All(meditation.History.Records, r => Assert.Null(r.Value.Answer));
        Assert.Equal([(runDate.AddDays(-14), "Overslept.")], meditation.History.Notes.Select(n => (n.EntryDate, n.Text.Value)));
        Assert.Equal(
            [
                (runDate.AddDays(-20), "Travelling."),
                (runDate.AddDays(-19), "Travelling."),
                (runDate.AddDays(-8), null),
                (runDate.AddDays(-2), null),
            ],
            meditation.Excusals.Select(e => (e.Date, e.Reason?.Value)));
        Assert.Empty(meditation.Pauses);
    }

    [Theory]
    [MemberData(nameof(RunDates))]
    public void Drinking_water_has_8_glasses_outside_the_past_pause_6_on_R_minus_5_with_its_note_and_its_past_and_current_pauses(
        DateOnly runDate, DayOfWeek day)
    {
        Assert.Equal(day, runDate.DayOfWeek);

        var water = DemoPauses.For(runDate)[1];

        List<(DateOnly, int?)> expected = [];
        for (var k = 28; k >= 2; k--)
        {
            if (k is < 12 or > 18)
            {
                expected.Add((runDate.AddDays(-k), k == 5 ? 6 : 8));
            }
        }

        Assert.Equal(expected, water.History.Records.Select(r => (r.Date, r.Value.Amount)));
        Assert.Equal(20, water.History.Records.Count);
        Assert.All(water.History.Records, r => Assert.Null(r.Value.Answer));
        Assert.DoesNotContain(water.History.Records, r => r.Date >= runDate.AddDays(-18) && r.Date <= runDate.AddDays(-12));
        Assert.DoesNotContain(water.History.Records, r => r.Date >= runDate.AddDays(-1));
        Assert.Equal([(runDate.AddDays(-5), "Busy day.")], water.History.Notes.Select(n => (n.EntryDate, n.Text.Value)));
        Assert.Empty(water.Excusals);
        Assert.Equal(
            [
                (runDate.AddDays(-18), runDate.AddDays(-12), "Ill with flu."),
                (runDate.AddDays(-1), runDate.AddDays(5), "Family visit."),
            ],
            water.Pauses.Select(p => (p.FirstDay, p.LastDay, p.Reason?.Value)));
    }

    [Fact]
    public void On_Wednesday_18_November_the_dates_are_those_of_the_acceptance_criteria()
    {
        var goals = DemoPauses.For(new DateOnly(2026, 11, 18));

        Assert.Equal(new DateOnly(2026, 10, 21), goals[0].History.Goal.StartDate);
        Assert.Equal(
            [new DateOnly(2026, 10, 29), new DateOnly(2026, 10, 30), new DateOnly(2026, 11, 10), new DateOnly(2026, 11, 16)],
            goals[0].Excusals.Select(e => e.Date));
        Assert.Equal(new DateOnly(2026, 11, 4), Assert.Single(goals[0].History.Notes).EntryDate);
        Assert.Equal(new DateOnly(2026, 11, 15), goals[0].History.Records[^1].Date);
        Assert.Equal(
            [(new DateOnly(2026, 10, 31), new DateOnly(2026, 11, 6)), (new DateOnly(2026, 11, 17), new DateOnly(2026, 11, 23))],
            goals[1].Pauses.Select(p => (p.FirstDay, p.LastDay)));
        Assert.Equal(new DateOnly(2026, 11, 13), Assert.Single(goals[1].History.Notes).EntryDate);
        Assert.Equal(new DateOnly(2026, 11, 16), goals[1].History.Records[^1].Date);
    }

    [Theory]
    [MemberData(nameof(RunDates))]
    public void Every_record_note_and_excusal_is_on_or_after_the_start_and_before_R_in_date_order_and_no_two_share_a_date(
        DateOnly runDate, DayOfWeek day)
    {
        Assert.Equal(day, runDate.DayOfWeek);

        foreach (var goal in DemoPauses.For(runDate))
        {
            var start = goal.History.Goal.StartDate;
            Assert.All(goal.History.Records, r => Assert.InRange(r.Date, start, runDate.AddDays(-1)));
            Assert.All(goal.History.Notes, n => Assert.InRange(n.EntryDate, start, runDate.AddDays(-1)));
            Assert.All(goal.Excusals, e => Assert.InRange(e.Date, start, runDate.AddDays(-1)));
            Assert.All(goal.Pauses, p => Assert.True(p.FirstDay >= start && p.FirstDay <= p.LastDay));
            Assert.Equal(goal.History.Records.Select(r => r.Date).Order(), goal.History.Records.Select(r => r.Date));
            Assert.Equal(goal.Excusals.Select(e => e.Date).Order(), goal.Excusals.Select(e => e.Date));
            Assert.Equal(goal.Pauses.Select(p => p.FirstDay).Order(), goal.Pauses.Select(p => p.FirstDay));
            Assert.Equal(goal.History.Records.Count, goal.History.Records.Select(r => r.Date).Distinct().Count());
            Assert.Equal(goal.Excusals.Count, goal.Excusals.Select(e => e.Date).Distinct().Count());

            // An excused commitment has no record, and no record or excusal lies in a pause (FR-001, FR-005).
            Assert.DoesNotContain(goal.Excusals, e => goal.History.Records.Any(r => r.Date == e.Date));
            Assert.DoesNotContain(goal.Pauses, p => goal.History.Records.Any(r => r.Date >= p.FirstDay && r.Date <= p.LastDay));
            Assert.DoesNotContain(goal.Pauses, p => goal.Excusals.Any(e => e.Date >= p.FirstDay && e.Date <= p.LastDay));
            Assert.DoesNotContain(goal.Pauses, p => goal.Pauses.Any(q => !ReferenceEquals(p, q) && p.FirstDay <= q.LastDay && q.FirstDay <= p.LastDay));
        }
    }

    [Fact]
    public void Each_record_value_passes_the_value_checks_of_its_goals_measurement()
    {
        foreach (var goal in DemoPauses.For(new DateOnly(2026, 11, 16)))
        {
            Assert.All(goal.History.Records, r =>
            {
                Assert.True(RecordValue.TryCreate(goal.History.Goal.Target.Measurement, r.Value.Answer, r.Value.Amount, out var value, out _));
                Assert.Equal(r.Value, value);
            });
        }
    }

    [Fact]
    public void Every_reason_passes_the_reason_checks_unchanged()
    {
        var reasons = DemoPauses.For(new DateOnly(2026, 11, 16))
            .SelectMany(g => g.Excusals.Select(e => e.Reason).Concat(g.Pauses.Select(p => p.Reason)))
            .OfType<ExemptionReason>()
            .ToList();

        Assert.Equal(4, reasons.Count);
        Assert.All(reasons, reason =>
        {
            Assert.True(ExemptionReason.TryCreate(reason.Value, out var again, out var tooLong));
            Assert.False(tooLong);
            Assert.Equal(reason, again);
        });
        foreach (var text in new[] { DemoPauses.TravellingReason, DemoPauses.IllnessReason, DemoPauses.FamilyVisitReason })
        {
            Assert.True(ExemptionReason.TryCreate(text, out var reason, out _));
            Assert.Equal(text, reason!.Value);
        }
    }

    [Fact]
    public void The_goal_names_are_the_display_names_in_creation_order()
    {
        Assert.Equal(["Meditation, 10 min daily", "Drinking water, 8 glasses a day"], DemoPauses.GoalNames);
        Assert.Equal(
            DemoPauses.For(new DateOnly(2026, 11, 18))
                .Select(g => g.History.Goal.CatalogHabitName + ", " + GoalText.TargetText(g.History.Goal.Target)),
            DemoPauses.GoalNames);
    }

    [Fact]
    public void The_reason_and_note_texts_are_those_of_FR_031()
    {
        Assert.Equal("Travelling.", DemoPauses.TravellingReason);
        Assert.Equal("Ill with flu.", DemoPauses.IllnessReason);
        Assert.Equal("Family visit.", DemoPauses.FamilyVisitReason);
        Assert.Equal("Overslept.", DemoPauses.OversleptNote);
        Assert.Equal("Busy day.", DemoPauses.BusyDayNote);
    }

    [Fact]
    public void A_demo_pause_cannot_end_before_it_starts()
    {
        var day = new DateOnly(2026, 11, 16);

        Assert.Throws<ArgumentException>(() => new DemoGoalPause(day, day.AddDays(-1), null));
        Assert.Equal(day, new DemoGoalPause(day, day, null).LastDay);
    }

    [Fact]
    public void A_demo_pause_goal_refuses_a_null_excusal_or_pause()
    {
        var history = DemoPauses.For(new DateOnly(2026, 11, 16))[0].History;

        Assert.Throws<ArgumentException>(() => new DemoPauseGoal(history, [null!], []));
        Assert.Throws<ArgumentException>(() => new DemoPauseGoal(history, [], [null!]));
        Assert.Throws<ArgumentNullException>(() => new DemoPauseGoal(null!, [], []));
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
