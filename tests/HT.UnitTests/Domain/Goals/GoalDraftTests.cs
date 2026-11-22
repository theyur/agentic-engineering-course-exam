using HT.Domain.Goals;
using HT.Domain.Habits;
using static HT.UnitTests.Domain.Goals.GoalSamples;

namespace HT.UnitTests.Domain.Goals;

/// <summary>FR-006: every refusal in order, both sides of every limit, and the values that are neither refused nor stored.</summary>
public class GoalDraftTests
{
    private static readonly DateOnly Today = Date(2026, 11, 18);
    private static readonly string LongName = new('n', 61);

    private static GoalTargetFields Valid => CountTimes(20, "pages", 3, SchedulePeriod.Week);

    private static GoalProblem Check(GoalTargetFields fields, DateOnly? start = null, string? name = null) =>
        GoalDraft.TryCreate(fields, start ?? Today, Today, name, out _, out _, out _);

    [Fact]
    public void No_habit_is_HabitMissing()
    {
        Assert.Equal(GoalProblem.HabitMissing, GoalDraft.CheckHabit(null));
        Assert.Equal(GoalProblem.HabitMissing, GoalDraft.CheckHabit(new GoalHabit(Habit.Id, (HabitKind)7)));
        Assert.Equal(GoalProblem.None, GoalDraft.CheckHabit(Habit));
        Assert.Equal(GoalProblem.None, GoalDraft.CheckHabit(Habit with { Kind = HabitKind.Custom }));
    }

    /// <summary>Each target refusal, with every earlier field valid and the later start date and name invalid too.</summary>
    public static TheoryData<GoalTargetFields, GoalProblem> TargetRefusals => new()
    {
        { Valid with { BuildOrReduce = null }, GoalProblem.BuildOrReduceMissing },
        { Valid with { Measurement = null }, GoalProblem.MeasurementMissing },
        { YesNoTimes(1, SchedulePeriod.Day) with { Amount = 1 }, GoalProblem.YesNoHasAmountOrUnit },
        { YesNoTimes(1, SchedulePeriod.Day) with { CountUnit = "pages" }, GoalProblem.YesNoHasAmountOrUnit },
        { YesNoTimes(1, SchedulePeriod.Day) with { DurationUnit = DurationUnit.Minutes }, GoalProblem.YesNoHasAmountOrUnit },
        { Valid with { Amount = null }, GoalProblem.BuildAmountOutOfRange },
        { Valid with { BuildOrReduce = BuildOrReduce.Reduce, Amount = null }, GoalProblem.ReduceAmountOutOfRange },
        { Valid with { CountUnit = "   " }, GoalProblem.CountUnitMissing },
        { Valid with { CountUnit = new string('u', 21) }, GoalProblem.CountUnitTooLong },
        { DurationTimes(15, null, 3, SchedulePeriod.Week), GoalProblem.DurationUnitMissing },
        { Valid with { ScheduleKind = null }, GoalProblem.ScheduleMissing },
        { Valid with { ScheduleKind = ScheduleKind.FixedDays, Days = null }, GoalProblem.DaysMissing },
        { Valid with { Times = null }, GoalProblem.TimesOutOfRange },
        { Valid with { Period = null }, GoalProblem.PeriodMissing },
        { CountTimes(2, "cups", 2, SchedulePeriod.Day, BuildOrReduce.Reduce), GoalProblem.ReduceAmountNeedsOneTime },
    };

    [Theory]
    [MemberData(nameof(TargetRefusals))]
    public void A_target_refusal_comes_before_the_start_date_and_the_name(GoalTargetFields fields, GoalProblem expected)
    {
        var problem = GoalDraft.TryCreate(fields, null, Today, LongName, out var target, out var start, out var name);

        Assert.Equal(expected, problem);
        Assert.Null(target);
        Assert.Equal(default, start);
        Assert.Null(name);
    }

    /// <summary>Slice 8 FR-003 step 5: the target steps alone give each target refusal, one case per message, in TryCreate's order.</summary>
    [Theory]
    [MemberData(nameof(TargetRefusals))]
    public void CheckTarget_gives_each_target_problem_as_TryCreate_does(GoalTargetFields fields, GoalProblem expected)
    {
        Assert.Equal(expected, GoalDraft.CheckTarget(fields, out var target));
        Assert.Null(target);
        Assert.Equal(expected, GoalDraft.TryCreate(fields, Today, Today, null, out _, out _, out _));
    }

    [Fact]
    public void CheckTarget_gives_the_first_problem_when_several_fields_are_wrong()
    {
        var everythingWrong = new GoalTargetFields { Amount = 0, CountUnit = new string('u', 21), Times = 0 };

        Assert.Equal(GoalProblem.BuildOrReduceMissing, GoalDraft.CheckTarget(everythingWrong, out _));
        Assert.Equal(GoalProblem.MeasurementMissing, GoalDraft.CheckTarget(everythingWrong with { BuildOrReduce = BuildOrReduce.Build }, out _));
        Assert.Equal(GoalProblem.BuildAmountOutOfRange, GoalDraft.CheckTarget(Valid with { Amount = 0, Times = 0, Period = null }, out _));
        Assert.Equal(GoalProblem.TimesOutOfRange, GoalDraft.CheckTarget(Valid with { Times = 0, Period = null }, out _));
    }

    [Fact]
    public void CheckTarget_gives_None_and_the_target_for_a_valid_target()
    {
        Assert.Equal(GoalProblem.None, GoalDraft.CheckTarget(Valid, out var target));
        Assert.Equal(Target(Valid), target);
    }

    [Fact]
    public void CheckTarget_ignores_values_outside_the_chosen_measurement_or_schedule()
    {
        // AC-021: a duration target keeps no count unit, a yes/no one no amount, and N times keeps no days.
        Assert.Equal(GoalProblem.None, GoalDraft.CheckTarget(DurationTimes(20, DurationUnit.Minutes, 1, SchedulePeriod.Day) with { CountUnit = "pages" }, out var duration));
        Assert.Null(duration!.CountUnit);
        Assert.Equal(GoalProblem.None, GoalDraft.CheckTarget(YesNoDays(BuildOrReduce.Build, MonWedFri) with { Times = 0, Period = SchedulePeriod.Week }, out var days));
        Assert.Null(days!.Schedule.Times);
        Assert.Equal(GoalProblem.None, GoalDraft.CheckTarget(Valid with { Days = new HashSet<DayOfWeek>() }, out var times));
        Assert.Empty(times!.Schedule.Days);
        Assert.Throws<ArgumentNullException>(() => GoalDraft.CheckTarget(null!, out _));
    }

    public static TheoryData<GoalTarget, GoalTarget, bool> ScheduleKindPairs => new()
    {
        { OnDays(MonWedFri), OnDays(DayOfWeek.Tuesday), true },
        { OnDays(MonWedFri), Target(DurationDays(30, DurationUnit.Minutes, BuildOrReduce.Reduce, Weekdays)), true },
        { PerDay(), Target(YesNoTimes(2, SchedulePeriod.Day)), true },
        { PerDay(), Target(CountTimes(20, "pages", 1, SchedulePeriod.Day)), true },
        { PerWeek(), Target(YesNoTimes(1, SchedulePeriod.Week)), true },
        { OnDays(AllDays), PerDay(), false },
        { PerDay(), PerWeek(), false },
        { PerWeek(), OnDays(MonWedFri), false },
    };

    [Theory]
    [MemberData(nameof(ScheduleKindPairs))]
    public void HasSameScheduleKind_for_fixed_days_per_day_and_per_week(GoalTarget a, GoalTarget b, bool same)
    {
        Assert.Equal(same, a.HasSameScheduleKind(b));
        Assert.Equal(same, b.HasSameScheduleKind(a));
        Assert.True(a.HasSameScheduleKind(a));
    }

    [Fact]
    public void HasSameScheduleKind_needs_a_target()
    {
        Assert.Throws<ArgumentNullException>(() => PerDay().HasSameScheduleKind(null!));
    }

    [Fact]
    public void StartDateMissing_comes_before_the_name()
    {
        Assert.Equal(GoalProblem.StartDateMissing, GoalDraft.TryCreate(Valid, null, Today, LongName, out _, out _, out _));
    }

    [Fact]
    public void StartDateInPast_comes_before_the_name()
    {
        Assert.Equal(GoalProblem.StartDateInPast, Check(Valid, Today.AddDays(-1), LongName));
    }

    [Fact]
    public void StartDateTooFar_comes_before_the_name()
    {
        Assert.Equal(GoalProblem.StartDateTooFar, Check(Valid, Today.AddDays(366), LongName));
    }

    [Fact]
    public void NameTooLong_is_the_last_check()
    {
        var problem = GoalDraft.TryCreate(Valid, Today, Today, LongName, out var target, out _, out var name);

        Assert.Equal(GoalProblem.NameTooLong, problem);
        Assert.Null(target);
        Assert.Null(name);
    }

    [Theory]
    [InlineData(BuildOrReduce.Build, 0, GoalProblem.BuildAmountOutOfRange)]
    [InlineData(BuildOrReduce.Build, 1, GoalProblem.None)]
    [InlineData(BuildOrReduce.Build, 999, GoalProblem.None)]
    [InlineData(BuildOrReduce.Build, 1000, GoalProblem.BuildAmountOutOfRange)]
    [InlineData(BuildOrReduce.Reduce, -1, GoalProblem.ReduceAmountOutOfRange)]
    [InlineData(BuildOrReduce.Reduce, 0, GoalProblem.None)]
    [InlineData(BuildOrReduce.Reduce, 1, GoalProblem.None)]
    [InlineData(BuildOrReduce.Reduce, 999, GoalProblem.None)]
    [InlineData(BuildOrReduce.Reduce, 1000, GoalProblem.ReduceAmountOutOfRange)]
    public void Amount_limits_for_count_and_duration(BuildOrReduce side, int amount, GoalProblem expected)
    {
        Assert.Equal(expected, Check(CountTimes(amount, "pages", 1, SchedulePeriod.Day, side)));
        Assert.Equal(expected, Check(DurationTimes(amount, DurationUnit.Minutes, 1, SchedulePeriod.Day, side)));
    }

    [Theory]
    [InlineData(20, GoalProblem.None)]
    [InlineData(21, GoalProblem.CountUnitTooLong)]
    public void Unit_limit(int length, GoalProblem expected)
    {
        Assert.Equal(expected, Check(CountTimes(20, "  " + new string('u', length) + "  ", 3, SchedulePeriod.Week)));
    }

    [Fact]
    public void The_unit_is_stored_trimmed_with_its_case()
    {
        Assert.Equal(GoalProblem.None, GoalDraft.TryCreate(CountTimes(20, "  Pages  ", 3, SchedulePeriod.Week), Today, Today, null, out var target, out _, out _));
        Assert.Equal("Pages", target!.CountUnit);
        Assert.Equal("pages", Target(CountTimes(20, "  pages  ", 3, SchedulePeriod.Week)).CountUnit);
    }

    [Theory]
    [InlineData(0, GoalProblem.TimesOutOfRange)]
    [InlineData(1, GoalProblem.None)]
    [InlineData(20, GoalProblem.None)]
    [InlineData(21, GoalProblem.TimesOutOfRange)]
    public void Times_limits(int times, GoalProblem expected)
    {
        Assert.Equal(expected, Check(YesNoTimes(times, SchedulePeriod.Week)));
        Assert.Equal(expected, Check(CountTimes(20, "pages", times, SchedulePeriod.Day)));
    }

    [Fact]
    public void A_Reduce_count_or_duration_goal_needs_one_time()
    {
        Assert.Equal(GoalProblem.ReduceAmountNeedsOneTime, Check(CountTimes(2, "cups", 2, SchedulePeriod.Day, BuildOrReduce.Reduce)));
        Assert.Equal(GoalProblem.ReduceAmountNeedsOneTime, Check(CountTimes(2, "cups", 2, SchedulePeriod.Week, BuildOrReduce.Reduce)));
        Assert.Equal(GoalProblem.ReduceAmountNeedsOneTime, Check(DurationTimes(2, DurationUnit.Hours, 2, SchedulePeriod.Day, BuildOrReduce.Reduce)));
        Assert.Equal(GoalProblem.None, Check(CountTimes(2, "cups", 1, SchedulePeriod.Day, BuildOrReduce.Reduce)));
        Assert.Equal(GoalProblem.None, Check(DurationTimes(2, DurationUnit.Hours, 1, SchedulePeriod.Week, BuildOrReduce.Reduce)));
        Assert.Equal(GoalProblem.None, Check(CountDays(2, "cups", BuildOrReduce.Reduce, Weekdays)));
    }

    [Fact]
    public void A_Reduce_yes_no_goal_may_have_several_times()
    {
        Assert.Equal(GoalProblem.None, Check(YesNoTimes(2, SchedulePeriod.Day, BuildOrReduce.Reduce)));
    }

    [Fact]
    public void A_Build_goal_may_have_several_times()
    {
        Assert.Equal(GoalProblem.None, Check(CountTimes(2, "cups", 2, SchedulePeriod.Day)));
    }

    [Theory]
    [InlineData(-1, GoalProblem.StartDateInPast)]
    [InlineData(0, GoalProblem.None)]
    [InlineData(365, GoalProblem.None)]
    [InlineData(366, GoalProblem.StartDateTooFar)]
    public void Start_date_limits(int daysFromToday, GoalProblem expected)
    {
        var problem = GoalDraft.TryCreate(Valid, Today.AddDays(daysFromToday), Today, null, out _, out var start, out _);

        Assert.Equal(expected, problem);
        if (expected == GoalProblem.None)
        {
            Assert.Equal(Today.AddDays(daysFromToday), start);
        }
    }

    [Fact]
    public void Name_of_60_code_units_after_trimming_is_kept()
    {
        var sixty = new string('n', 60);

        Assert.Equal(GoalProblem.None, GoalDraft.TryCreate(Valid, Today, Today, "  " + sixty + "  ", out _, out _, out var name));
        Assert.Equal(sixty, name!.Value.Value);
    }

    [Fact]
    public void Name_of_61_code_units_after_trimming_is_too_long()
    {
        Assert.Equal(GoalProblem.NameTooLong, Check(Valid, Today, "  " + new string('n', 61) + "  "));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Spaces_only_is_no_name(string? text)
    {
        Assert.Equal(GoalProblem.None, GoalDraft.TryCreate(Valid, Today, Today, text, out var target, out _, out var name));
        Assert.NotNull(target);
        Assert.Null(name);
    }

    [Fact]
    public void An_accepted_goal_gives_its_target_start_and_name()
    {
        var problem = GoalDraft.TryCreate(Valid, Today.AddDays(5), Today, "  Evening reading ", out var target, out var start, out var name);

        Assert.Equal(GoalProblem.None, problem);
        Assert.Equal(Target(Valid), target);
        Assert.Equal(Today.AddDays(5), start);
        Assert.Equal("Evening reading", name!.Value.Value);
    }

    [Fact]
    public void Values_that_do_not_belong_to_the_measurement_are_neither_refused_nor_stored()
    {
        var count = Target(Valid with { DurationUnit = DurationUnit.Hours });
        Assert.Null(count.DurationUnit);
        Assert.Equal("pages", count.CountUnit);

        var duration = Target(DurationTimes(15, DurationUnit.Minutes, 3, SchedulePeriod.Week) with { CountUnit = "pages" });
        Assert.Null(duration.CountUnit);
        Assert.Equal(DurationUnit.Minutes, duration.DurationUnit);

        var yesNo = Target(YesNoTimes(1, SchedulePeriod.Day) with { CountUnit = "   " });
        Assert.Null(yesNo.Amount);
        Assert.Null(yesNo.CountUnit);
        Assert.Null(yesNo.DurationUnit);
    }

    [Fact]
    public void Values_that_do_not_belong_to_the_schedule_are_neither_refused_nor_stored()
    {
        var times = Target(Valid with { Days = MonWedFri.ToHashSet() });
        Assert.Empty(times.Schedule.Days);
        Assert.Equal(3, times.Schedule.Times);

        var days = Target(YesNoDays(BuildOrReduce.Build, MonWedFri) with { Times = 99, Period = SchedulePeriod.Week });
        Assert.Equal(MonWedFri.ToHashSet(), days.Schedule.Days);
        Assert.Null(days.Schedule.Times);
        Assert.Null(days.Schedule.Period);
    }

    [Fact]
    public void Days_with_N_times_do_not_make_a_Reduce_count_goal_need_one_time_any_less()
    {
        Assert.Equal(GoalProblem.ReduceAmountNeedsOneTime,
            Check(CountTimes(2, "cups", 3, SchedulePeriod.Day, BuildOrReduce.Reduce) with { Days = MonWedFri.ToHashSet() }));
    }

    public static TheoryData<GoalProblem, GoalField> Fields => new()
    {
        { GoalProblem.HabitMissing, GoalField.Habit },
        { GoalProblem.BuildOrReduceMissing, GoalField.BuildOrReduce },
        { GoalProblem.MeasurementMissing, GoalField.Measurement },
        { GoalProblem.YesNoHasAmountOrUnit, GoalField.Measurement },
        { GoalProblem.BuildAmountOutOfRange, GoalField.Amount },
        { GoalProblem.ReduceAmountOutOfRange, GoalField.Amount },
        { GoalProblem.CountUnitMissing, GoalField.Unit },
        { GoalProblem.CountUnitTooLong, GoalField.Unit },
        { GoalProblem.DurationUnitMissing, GoalField.Unit },
        { GoalProblem.ScheduleMissing, GoalField.Schedule },
        { GoalProblem.DaysMissing, GoalField.Days },
        { GoalProblem.TimesOutOfRange, GoalField.Times },
        { GoalProblem.PeriodMissing, GoalField.Period },
        { GoalProblem.ReduceAmountNeedsOneTime, GoalField.Times },
        { GoalProblem.StartDateMissing, GoalField.StartDate },
        { GoalProblem.StartDateInPast, GoalField.StartDate },
        { GoalProblem.StartDateNotAfterEnding, GoalField.StartDate },
        { GoalProblem.StartDateTooFar, GoalField.StartDate },
        { GoalProblem.NameTooLong, GoalField.Name },
    };

    [Theory]
    [MemberData(nameof(Fields))]
    public void Every_refusal_is_about_one_field(GoalProblem problem, GoalField field)
    {
        Assert.Equal(field, GoalDraft.FieldOf(problem));
    }

    [Fact]
    public void Every_refusal_has_a_field_and_None_has_none()
    {
        var mapped = Fields.Select(row => (GoalProblem)row[0]).ToHashSet();

        Assert.Equal(Enum.GetValues<GoalProblem>().Where(p => p != GoalProblem.None).ToHashSet(), mapped);
        Assert.Throws<ArgumentOutOfRangeException>(() => GoalDraft.FieldOf(GoalProblem.None));
    }
}
