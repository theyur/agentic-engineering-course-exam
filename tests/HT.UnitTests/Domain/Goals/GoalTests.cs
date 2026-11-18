using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Time;
using static HT.UnitTests.Domain.Goals.GoalSamples;

namespace HT.UnitTests.Domain.Goals;

/// <summary>data-model.md's invariants of <see cref="Goal"/>, and <see cref="GoalTarget"/>'s schedule kind and period length.</summary>
public class GoalTests
{
    private static readonly DateOnly Start = Date(2026, 11, 18);
    private static readonly GoalId Id = new(Guid.Parse("6f9a3c1e-0000-4000-8000-0000000000aa"));

    private static GoalSettingsVersion Version(GoalTarget target, DateOnly appliesFrom, NamedTimeZone? zone = null) =>
        new(new GoalSettings(target, zone ?? Kyiv), appliesFrom);

    private static Goal Make(IReadOnlyList<GoalSettingsVersion> versions, GoalAttempt? attempt = null, GoalHabit? habit = null) =>
        new(Id, Owner, habit ?? Habit, Start, null, attempt ?? new GoalAttempt(1, Start), versions);

    [Fact]
    public void A_goal_keeps_what_it_is_given()
    {
        Assert.True(GoalName.TryCreate("Evening reading", out var name, out _));
        var versions = new[] { Version(PerDay(), Start), Version(PerDay(), Start.AddDays(3), NewYork) };

        var goal = new Goal(Id, Owner, Habit, Start, name, new GoalAttempt(1, Start), versions);

        Assert.Equal(Id, goal.Id);
        Assert.Equal(Owner, goal.Owner);
        Assert.Equal(Habit, goal.Habit);
        Assert.Equal(Start, goal.StartDate);
        Assert.Equal("Evening reading", goal.OwnName!.Value.Value);
        Assert.Equal(new GoalAttempt(1, Start), goal.Attempt);
        Assert.Equal(versions, goal.Versions);
    }

    [Fact]
    public void The_versions_are_copied()
    {
        var versions = new List<GoalSettingsVersion> { Version(PerDay(), Start) };
        var goal = Make(versions);

        versions.Add(Version(PerDay(), Start.AddDays(1)));

        Assert.Single(goal.Versions);
    }

    [Fact]
    public void Versions_are_non_empty()
    {
        Assert.Throws<ArgumentException>(() => Make([]));
        Assert.Throws<ArgumentNullException>(() => Make(null!));
        Assert.Throws<ArgumentException>(() => Make([Version(PerDay(), Start), null!]));
    }

    [Fact]
    public void The_first_version_applies_from_the_start_date()
    {
        Assert.Throws<ArgumentException>(() => Make([Version(PerDay(), Start.AddDays(1))]));
        Assert.Throws<ArgumentException>(() => Make([Version(PerDay(), Start.AddDays(-1))]));
    }

    [Fact]
    public void The_dates_strictly_increase()
    {
        Assert.Throws<ArgumentException>(() => Make([Version(PerDay(), Start), Version(PerDay(), Start, NewYork)]));
        Assert.Throws<ArgumentException>(() =>
            Make([Version(PerDay(), Start), Version(PerDay(), Start.AddDays(5), NewYork), Version(PerDay(), Start.AddDays(4), Tokyo)]));
        Assert.Equal(3, Make([Version(PerDay(), Start), Version(PerDay(), Start.AddDays(4), NewYork), Version(PerDay(), Start.AddDays(5), Tokyo)]).Versions.Count);
    }

    [Fact]
    public void Only_the_time_zone_differs_between_versions()
    {
        Assert.Throws<ArgumentException>(() => Make([Version(PerDay(), Start), Version(Target(YesNoTimes(2, SchedulePeriod.Day)), Start.AddDays(1))]));
    }

    [Fact]
    public void A_later_version_of_a_per_week_goal_applies_from_a_Monday()
    {
        // Start is Wednesday 18 November 2026; the next Monday is 23 November.
        Assert.Throws<ArgumentException>(() => Make([Version(PerWeek(), Start), Version(PerWeek(), Start.AddDays(1), NewYork)]));
        Assert.Equal(2, Make([Version(PerWeek(), Start), Version(PerWeek(), Date(2026, 11, 23), NewYork)]).Versions.Count);
    }

    [Fact]
    public void A_later_version_of_a_daily_or_commitment_based_goal_applies_from_any_day()
    {
        Assert.Equal(2, Make([Version(PerDay(), Start), Version(PerDay(), Start.AddDays(1), NewYork)]).Versions.Count);
        Assert.Equal(2, Make([Version(OnDays(MonWedFri), Start), Version(OnDays(MonWedFri), Start.AddDays(2), NewYork)]).Versions.Count);
    }

    [Fact]
    public void The_attempt_does_not_begin_before_the_start_date()
    {
        Assert.Throws<ArgumentException>(() => Make([Version(PerDay(), Start)], new GoalAttempt(1, Start.AddDays(-1))));
        Assert.Throws<ArgumentOutOfRangeException>(() => new GoalAttempt(0, Start));
    }

    [Fact]
    public void The_habit_kind_is_defined()
    {
        Assert.Throws<ArgumentException>(() => Make([Version(PerDay(), Start)], habit: Habit with { Kind = (HabitKind)9 }));
    }

    [Fact]
    public void A_version_has_a_named_time_zone()
    {
        Assert.Throws<ArgumentException>(() => new GoalSettings(PerDay(), default));
        Assert.Throws<ArgumentNullException>(() => new GoalSettings(null!, Kyiv));
        Assert.Throws<ArgumentException>(() => GoalVersionChange.ReplaceFirst(default));
    }

    [Fact]
    public void ApplyTo_checks_the_result()
    {
        var goal = Make([Version(PerWeek(), Start)]);

        Assert.Throws<ArgumentException>(() => GoalVersionChange.Add(Start.AddDays(1), NewYork).ApplyTo(goal));
        Assert.Throws<InvalidOperationException>(() => GoalVersionChange.ReplacePending(NewYork, Date(2026, 11, 23)).ApplyTo(goal));
    }

    [Fact]
    public void Fixed_days_are_commitment_based_and_have_no_period_length()
    {
        var target = OnDays(MonWedFri);

        Assert.Equal(GoalScheduleKind.CommitmentBased, target.ScheduleKindOf);
        Assert.Null(target.PeriodLength);
    }

    [Theory]
    [InlineData(SchedulePeriod.Day)]
    [InlineData(SchedulePeriod.Week)]
    public void N_times_is_period_based_with_its_period_length(SchedulePeriod period)
    {
        var target = Target(YesNoTimes(2, period));

        Assert.Equal(GoalScheduleKind.PeriodBased, target.ScheduleKindOf);
        Assert.Equal(period, target.PeriodLength);
    }

    [Fact]
    public void Targets_with_the_same_values_are_equal()
    {
        var a = Target(CountDays(20, "pages", BuildOrReduce.Build, DayOfWeek.Friday, DayOfWeek.Monday));
        var b = Target(CountDays(20, " pages ", BuildOrReduce.Build, DayOfWeek.Monday, DayOfWeek.Friday) with { DurationUnit = DurationUnit.Hours });

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
        Assert.NotEqual(a, Target(CountDays(20, "Pages", BuildOrReduce.Build, DayOfWeek.Friday, DayOfWeek.Monday)));
        Assert.NotEqual(a, Target(CountDays(20, "pages", BuildOrReduce.Build, DayOfWeek.Friday)));
        Assert.False(a.Equals(null));
    }

    [Fact]
    public void A_stored_target_is_rebuilt_from_its_fields()
    {
        var target = Target(DurationTimes(30, DurationUnit.Minutes, 1, SchedulePeriod.Day, BuildOrReduce.Reduce));
        var fields = new GoalTargetFields
        {
            BuildOrReduce = target.BuildOrReduce, Measurement = target.Measurement, Amount = target.Amount,
            CountUnit = target.CountUnit, DurationUnit = target.DurationUnit, ScheduleKind = target.Schedule.Kind,
            Days = target.Schedule.Days, Times = target.Schedule.Times, Period = target.Schedule.Period,
        };

        Assert.True(GoalTarget.TryCreate(fields, out var rebuilt, out var problem));
        Assert.Equal(GoalProblem.None, problem);
        Assert.Equal(target, rebuilt);
    }

    [Fact]
    public void A_target_with_a_problem_is_not_created()
    {
        Assert.False(GoalTarget.TryCreate(CountTimes(2, "cups", 2, SchedulePeriod.Day, BuildOrReduce.Reduce), out var target, out var problem));
        Assert.Null(target);
        Assert.Equal(GoalProblem.ReduceAmountNeedsOneTime, problem);
        Assert.Throws<ArgumentNullException>(() => GoalTarget.TryCreate(null!, out _, out _));
    }
}
