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

    /// <summary>Slice 8 D-1 (research R-12): changed on purpose from "only the time zone differs between versions".</summary>
    [Fact]
    public void Versions_of_one_attempt_keep_the_schedule_kind()
    {
        Assert.Throws<ArgumentException>(() => Make([Version(PerDay(), Start), Version(OnDays(AllDays), Start.AddDays(1))]));
        Assert.Throws<ArgumentException>(() => Make([Version(PerDay(), Start), Version(PerWeek(), Date(2026, 11, 23))]));
        Assert.Equal(2, Make([Version(PerDay(), Start), Version(Target(YesNoTimes(2, SchedulePeriod.Day)), Start.AddDays(1))]).Versions.Count);
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
    public void Equal_yes_no_targets_have_the_same_hash_code()
    {
        // A yes/no target has no count unit, so the hash code must not hash a null unit.
        var a = Target(YesNoDays(BuildOrReduce.Build, DayOfWeek.Monday, DayOfWeek.Friday));
        var b = Target(YesNoDays(BuildOrReduce.Build, DayOfWeek.Friday, DayOfWeek.Monday));

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
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

    // Slice 8 (research R-01): the attempts constructor. Attempt 1 runs daily from Monday 2 November 2026 and is ended
    // on Monday 16 November (last date 15 November); attempt 2 starts on Wednesday 18 November.
    private static readonly DateOnly FirstStart = Date(2026, 11, 2);
    private static readonly DateOnly FirstEnded = Date(2026, 11, 16);

    private static GoalEnding EndedOn(DateOnly date) => new(date, Outcome.Abandoned, null);

    private static GoalAttemptVersions Attempt(int number, DateOnly start, GoalEnding? ending, params GoalSettingsVersion[] versions) =>
        new(new GoalAttempt(number, start) { Ending = ending }, versions);

    private static GoalAttemptVersions EndedFirst(params GoalSettingsVersion[] versions) =>
        Attempt(1, FirstStart, EndedOn(FirstEnded), versions.Length == 0 ? [Version(PerDay(), FirstStart)] : versions);

    private static Goal MakeAttempts(params GoalAttemptVersions[] attempts) => new(Id, Owner, Habit, null, attempts);

    [Fact]
    public void A_goal_with_two_attempts_keeps_what_it_is_given()
    {
        Assert.True(GoalName.TryCreate("Evening reading", out var name, out _));
        var first = EndedFirst();
        var second = Attempt(2, Start, null, Version(PerWeek(), Start), Version(PerWeek(), Date(2026, 11, 23), NewYork));

        var goal = new Goal(Id, Owner, Habit, name, [first, second]);

        Assert.Equal(Id, goal.Id);
        Assert.Equal(Owner, goal.Owner);
        Assert.Equal(Habit, goal.Habit);
        Assert.Equal("Evening reading", goal.OwnName!.Value.Value);
        Assert.Equal(FirstStart, goal.StartDate);
        Assert.Equal(second.Attempt, goal.Attempt);
        Assert.Equal(second.Versions, goal.Versions);
        Assert.Equal([first.Attempt, second.Attempt], goal.Attempts.Select(a => a.Attempt));
        Assert.Equal(first.Versions, goal.VersionsOf(1));
        Assert.Equal(second.Versions, goal.VersionsOf(2));
        Assert.False(goal.IsEnded);
    }

    [Fact]
    public void A_goal_whose_latest_attempt_has_an_ending_is_ended()
    {
        Assert.True(MakeAttempts(EndedFirst()).IsEnded);
        Assert.False(MakeAttempts(Attempt(1, FirstStart, null, Version(PerDay(), FirstStart))).IsEnded);
        Assert.True(MakeAttempts(EndedFirst(), Attempt(2, Start, EndedOn(Start.AddDays(3)), Version(PerDay(), Start))).IsEnded);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(-1)]
    public void VersionsOf_an_unknown_number_throws(int number)
    {
        var goal = MakeAttempts(EndedFirst(), Attempt(2, Start, null, Version(PerDay(), Start)));

        Assert.Throws<ArgumentOutOfRangeException>(() => goal.VersionsOf(number));
    }

    [Fact]
    public void A_goal_has_at_least_one_attempt()
    {
        Assert.Throws<ArgumentException>(() => MakeAttempts());
        Assert.Throws<ArgumentNullException>(() => new Goal(Id, Owner, Habit, null, null!));
        Assert.Throws<ArgumentException>(() => MakeAttempts(EndedFirst(), null!));
        Assert.Throws<ArgumentException>(() => MakeAttempts(new GoalAttemptVersions(null!, [Version(PerDay(), FirstStart)])));
        Assert.Throws<ArgumentException>(() => MakeAttempts(new GoalAttemptVersions(new GoalAttempt(1, FirstStart), null!)));
    }

    [Fact]
    public void The_attempts_are_numbered_1_2_in_order()
    {
        Assert.Throws<ArgumentException>(() => MakeAttempts(EndedFirst(), Attempt(3, Start, null, Version(PerDay(), Start))));
        Assert.Throws<ArgumentException>(() => MakeAttempts(Attempt(2, FirstStart, null, Version(PerDay(), FirstStart))));
        Assert.Equal(2, MakeAttempts(EndedFirst(), Attempt(2, Start, null, Version(PerDay(), Start))).Attempts.Count);
    }

    [Fact]
    public void Every_attempt_before_the_latest_is_ended()
    {
        var running = Attempt(1, FirstStart, null, Version(PerDay(), FirstStart));

        Assert.Throws<ArgumentException>(() => MakeAttempts(running, Attempt(2, Start, null, Version(PerDay(), Start))));
    }

    [Fact]
    public void Each_attempt_has_at_least_one_version_and_none_is_null()
    {
        Assert.Throws<ArgumentException>(() => MakeAttempts(Attempt(1, FirstStart, null)));
        Assert.Throws<ArgumentException>(() => MakeAttempts(EndedFirst(), Attempt(2, Start, null)));
        Assert.Throws<ArgumentException>(() => MakeAttempts(EndedFirst(), Attempt(2, Start, null, Version(PerDay(), Start), null!)));
    }

    [Fact]
    public void The_dates_of_an_attempt_strictly_increase()
    {
        Assert.Throws<ArgumentException>(() =>
            MakeAttempts(EndedFirst(), Attempt(2, Start, null, Version(PerDay(), Start), Version(PerDay(), Start, NewYork))));
        Assert.Throws<ArgumentException>(() => MakeAttempts(Attempt(
            1, FirstStart, null, Version(PerDay(), FirstStart), Version(PerDay(), FirstStart.AddDays(5)), Version(PerDay(), FirstStart.AddDays(4)))));
    }

    [Fact]
    public void Attempt_1_does_not_begin_before_its_first_version_which_gives_the_goal_its_start_date()
    {
        // As slice 4 refuses an attempt that begins before the goal's start date.
        Assert.Throws<ArgumentException>(() => MakeAttempts(Attempt(1, FirstStart.AddDays(-1), null, Version(PerDay(), FirstStart))));

        var later = MakeAttempts(Attempt(1, FirstStart.AddDays(2), null, Version(PerDay(), FirstStart)));
        Assert.Equal(FirstStart, later.StartDate);
        Assert.Equal(FirstStart.AddDays(2), later.Attempt.StartDate);
    }

    [Fact]
    public void A_later_attempts_first_version_applies_from_its_start_date()
    {
        Assert.Throws<ArgumentException>(() => MakeAttempts(EndedFirst(), Attempt(2, Start, null, Version(PerDay(), Start.AddDays(1)))));
        Assert.Throws<ArgumentException>(() => MakeAttempts(EndedFirst(), Attempt(2, Start, null, Version(PerDay(), Start.AddDays(-1)))));
    }

    [Fact]
    public void All_versions_of_one_attempt_have_the_same_schedule_kind_but_attempts_may_differ()
    {
        Assert.Throws<ArgumentException>(() =>
            MakeAttempts(EndedFirst(), Attempt(2, Start, null, Version(PerDay(), Start), Version(OnDays(AllDays), Start.AddDays(1)))));
        Assert.Throws<ArgumentException>(() =>
            MakeAttempts(EndedFirst(Version(PerDay(), FirstStart), Version(PerWeek(), Date(2026, 11, 9)))));

        var restarted = MakeAttempts(EndedFirst(), Attempt(2, Start, null, Version(PerWeek(), Start)));
        Assert.Equal(GoalScheduleKind.PeriodBased, restarted.Versions[0].Settings.Target.ScheduleKindOf);
    }

    [Fact]
    public void A_later_version_of_a_per_week_attempt_applies_from_a_Monday()
    {
        Assert.Throws<ArgumentException>(() =>
            MakeAttempts(EndedFirst(), Attempt(2, Start, null, Version(PerWeek(), Start), Version(PerWeek(), Date(2026, 11, 24)))));
        Assert.Equal(2, MakeAttempts(EndedFirst(), Attempt(2, Start, null, Version(PerWeek(), Start), Version(PerWeek(), Date(2026, 11, 23)))).Versions.Count);
    }

    [Fact]
    public void The_next_attempt_starts_after_the_previous_attempts_ending_date()
    {
        Assert.Throws<ArgumentException>(() => MakeAttempts(EndedFirst(), Attempt(2, FirstEnded, null, Version(PerDay(), FirstEnded))));
        Assert.Throws<ArgumentException>(() => MakeAttempts(EndedFirst(), Attempt(2, FirstEnded.AddDays(-3), null, Version(PerDay(), FirstEnded.AddDays(-3)))));
        Assert.Equal(2, MakeAttempts(EndedFirst(), Attempt(2, FirstEnded.AddDays(1), null, Version(PerDay(), FirstEnded.AddDays(1)))).Attempts.Count);
    }

    [Fact]
    public void An_ended_attempt_has_no_version_after_its_last_date_except_its_first()
    {
        // Ended 16 November: the last date is 15 November.
        Assert.Throws<ArgumentException>(() => MakeAttempts(EndedFirst(Version(PerDay(), FirstStart), Version(PerDay(), FirstEnded))));
        Assert.Equal(2, MakeAttempts(EndedFirst(Version(PerDay(), FirstStart), Version(PerDay(), FirstEnded.AddDays(-1), NewYork))).VersionsOf(1).Count);

        // A per-week attempt ended on Wednesday 18 November has its last date on Sunday 15 November.
        Assert.Throws<ArgumentException>(() => MakeAttempts(
            Attempt(1, FirstStart, EndedOn(Start), Version(PerWeek(), FirstStart), Version(PerWeek(), Date(2026, 11, 16)))));
        Assert.Equal(2, MakeAttempts(
            Attempt(1, FirstStart, EndedOn(Start), Version(PerWeek(), FirstStart), Version(PerWeek(), Date(2026, 11, 9)))).VersionsOf(1).Count);
    }

    [Fact]
    public void An_ended_attempt_with_no_date_keeps_its_first_version()
    {
        // AC-035: attempt 1 from Monday 23 November ended on Tuesday 17 November, before it began; attempt 2 from 18 November.
        var first = Attempt(1, Date(2026, 11, 23), EndedOn(Date(2026, 11, 17)), Version(PerDay(), Date(2026, 11, 23)));
        var goal = MakeAttempts(first, Attempt(2, Start, null, Version(PerDay(), Start)));

        Assert.Equal(Date(2026, 11, 23), goal.StartDate);
        Assert.Equal(Start, goal.Attempt.StartDate);
        Assert.Throws<ArgumentException>(() => MakeAttempts(
            Attempt(1, Date(2026, 11, 23), EndedOn(Date(2026, 11, 17)), Version(PerDay(), Date(2026, 11, 23)), Version(PerDay(), Date(2026, 11, 24)))));
    }

    [Fact]
    public void The_attempts_constructor_checks_the_habit_kind()
    {
        Assert.Throws<ArgumentException>(() => new Goal(Id, Owner, Habit with { Kind = (HabitKind)9 }, null, [EndedFirst()]));
    }

    [Fact]
    public void The_attempts_and_their_versions_are_copied()
    {
        var versions = new List<GoalSettingsVersion> { Version(PerDay(), FirstStart) };
        var attempts = new List<GoalAttemptVersions> { new(new GoalAttempt(1, FirstStart), versions) };
        var goal = new Goal(Id, Owner, Habit, null, attempts);

        versions.Add(Version(PerDay(), FirstStart.AddDays(1)));
        attempts.Clear();

        Assert.Single(goal.Versions);
        Assert.Single(goal.Attempts);
    }

    [Fact]
    public void The_kept_constructor_gives_the_new_constructors_goal_for_one_attempt()
    {
        var attempt = new GoalAttempt(1, Start.AddDays(1));
        GoalSettingsVersion[] versions = [Version(PerDay(), Start), Version(Target(YesNoTimes(2, SchedulePeriod.Day)), Start.AddDays(4), NewYork)];

        var kept = new Goal(Id, Owner, Habit, Start, null, attempt, versions);
        var built = new Goal(Id, Owner, Habit, null, [new GoalAttemptVersions(attempt, versions)]);

        Assert.Equal(built.StartDate, kept.StartDate);
        Assert.Equal(built.Attempt, kept.Attempt);
        Assert.Equal(built.Versions, kept.Versions);
        Assert.Equal(built.IsEnded, kept.IsEnded);
        Assert.Single(kept.Attempts);
        Assert.Equal(built.Attempts[0].Attempt, kept.Attempts[0].Attempt);
        Assert.Equal(built.VersionsOf(1), kept.VersionsOf(1));
    }

    [Fact]
    public void The_kept_constructor_checks_an_ended_attempt_as_the_new_one_does()
    {
        var ended = new GoalAttempt(1, FirstStart) { Ending = EndedOn(FirstEnded) };

        Assert.Throws<ArgumentException>(() =>
            new Goal(Id, Owner, Habit, FirstStart, null, ended, [Version(PerDay(), FirstStart), Version(PerDay(), FirstEnded)]));
        Assert.True(new Goal(Id, Owner, Habit, FirstStart, null, ended, [Version(PerDay(), FirstStart)]).IsEnded);
    }
}
