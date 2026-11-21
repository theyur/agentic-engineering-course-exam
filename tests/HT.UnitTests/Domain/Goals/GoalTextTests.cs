using HT.Domain.Goals;
using HT.Domain.Habits;
using static HT.UnitTests.Domain.Goals.GoalSamples;

namespace HT.UnitTests.Domain.Goals;

/// <summary>FR-010 and spec "Target text and kind": the goal text, its one exception, the derived name and the schedule text.</summary>
public class GoalTextTests
{
    private static readonly DateOnly Start = Date(2026, 11, 18);

    public static TheoryData<GoalTargetFields, string, string> Examples => new()
    {
        { DurationDays(15, DurationUnit.Minutes, BuildOrReduce.Build, Weekdays), "15 min, 5 days a week", "Duration · fixed days Mon–Fri" },
        { CountTimes(20, "pages", 3, SchedulePeriod.Week), "20 pages, 3 times a week", "Count · flexible" },
        { CountTimes(1, "sweet", 1, SchedulePeriod.Day, BuildOrReduce.Reduce), "at most 1 sweet a day", "Count · flexible" },
        { CountTimes(2, "cups", 1, SchedulePeriod.Day, BuildOrReduce.Reduce), "at most 2 cups a day", "Count · flexible" },
        { CountTimes(0, "sweets", 1, SchedulePeriod.Day, BuildOrReduce.Reduce), "at most 0 sweets a day", "Count · flexible" },
        { YesNoTimes(3, SchedulePeriod.Week), "3 times a week", "Yes/no · flexible" },
        { YesNoTimes(1, SchedulePeriod.Week), "once a week", "Yes/no · flexible" },
        { YesNoTimes(1, SchedulePeriod.Day), "once a day", "Yes/no · flexible" },
        { YesNoTimes(2, SchedulePeriod.Day), "2 times a day", "Yes/no · flexible" },
        { YesNoTimes(1, SchedulePeriod.Day, BuildOrReduce.Reduce), "at most once a day", "Yes/no · flexible" },
        { YesNoTimes(3, SchedulePeriod.Week, BuildOrReduce.Reduce), "at most 3 times a week", "Yes/no · flexible" },
        { YesNoDays(BuildOrReduce.Build, AllDays), "daily", "Yes/no · fixed days every day" },
        { DurationDays(30, DurationUnit.Minutes, BuildOrReduce.Build, AllDays), "30 min daily", "Duration · fixed days every day" },
        { DurationTimes(2, DurationUnit.Hours, 1, SchedulePeriod.Day, BuildOrReduce.Reduce), "at most 2 h a day", "Duration · flexible" },
        { DurationTimes(20, DurationUnit.Minutes, 2, SchedulePeriod.Day), "20 min, 2 times a day", "Duration · flexible" },
        { DurationDays(1, DurationUnit.Hours, BuildOrReduce.Build, MonWedFri), "1 h, 3 days a week", "Duration · fixed days Mon, Wed, Fri" },
        { CountDays(10, "pages", BuildOrReduce.Build, DayOfWeek.Saturday), "10 pages, once a week", "Count · fixed days Sat" },
        { CountDays(1, "cigarette", BuildOrReduce.Reduce, Weekdays), "at most 1 cigarette, 5 days a week", "Count · fixed days Mon–Fri" },
        {
            YesNoDays(BuildOrReduce.Build, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Saturday),
            "4 days a week", "Yes/no · fixed days Mon–Wed, Sat"
        },
        { YesNoDays(BuildOrReduce.Build, DayOfWeek.Saturday, DayOfWeek.Sunday), "2 days a week", "Yes/no · fixed days Sat, Sun" },
        { YesNoDays(BuildOrReduce.Build, DayOfWeek.Tuesday, DayOfWeek.Thursday), "2 days a week", "Yes/no · fixed days Tue, Thu" },
        {
            CountDays(20, "pages", BuildOrReduce.Build, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday),
            "20 pages, 4 days a week", "Count · fixed days Mon–Thu"
        },
        { YesNoDays(BuildOrReduce.Reduce, Weekdays), "avoided on Mon–Fri", "Yes/no · fixed days Mon–Fri" },
        { YesNoDays(BuildOrReduce.Reduce, DayOfWeek.Saturday, DayOfWeek.Sunday), "avoided on Sat, Sun", "Yes/no · fixed days Sat, Sun" },
        { YesNoDays(BuildOrReduce.Reduce, AllDays), "avoided every day", "Yes/no · fixed days every day" },
        { YesNoDays(BuildOrReduce.Reduce, MonWedFri), "avoided on Mon, Wed, Fri", "Yes/no · fixed days Mon, Wed, Fri" },
        { YesNoDays(BuildOrReduce.Reduce, DayOfWeek.Sunday), "avoided on Sun", "Yes/no · fixed days Sun" },
    };

    [Theory]
    [MemberData(nameof(Examples))]
    public void Reproduces_every_example(GoalTargetFields fields, string text, string kind)
    {
        var target = Target(fields);

        Assert.Equal(text, GoalText.TargetText(target));
        Assert.Equal(kind, GoalText.Kind(target));
    }

    [Fact]
    public void The_suggestion_keeps_slice_2s_text_for_a_Reduce_yes_no_on_fixed_days()
    {
        var fields = new SuggestedStartingGoalFields
        {
            BuildOrReduce = BuildOrReduce.Reduce, Measurement = Measurement.YesNo, ScheduleKind = ScheduleKind.FixedDays,
            Days = Weekdays.ToHashSet(),
        };
        Assert.True(SuggestedStartingGoal.TryCreate(fields, out var suggestion, out _));

        Assert.Equal(("at most 5 days a week", "Yes/no · fixed days Mon–Fri"), SuggestedStartingGoalText.Format(suggestion));
        Assert.Equal("avoided on Mon–Fri", GoalText.TargetText(Target(YesNoDays(BuildOrReduce.Reduce, Weekdays))));
    }

    public static TheoryData<string, GoalTargetFields, string> DerivedNames => new()
    {
        { "Reading", DurationDays(15, DurationUnit.Minutes, BuildOrReduce.Build, Weekdays), "Reading, 15 min, 5 days a week" },
        { "Cold shower", YesNoDays(BuildOrReduce.Build, AllDays), "Cold shower, daily" },
        { "Sugar", CountTimes(1, "sweet", 1, SchedulePeriod.Day, BuildOrReduce.Reduce), "Sugar, at most 1 sweet a day" },
        { "No phone in bed", YesNoDays(BuildOrReduce.Reduce, AllDays), "No phone in bed, avoided every day" },
        { "Coffee", CountTimes(2, "cups", 1, SchedulePeriod.Day, BuildOrReduce.Reduce), "Coffee, at most 2 cups a day" },
        { "Meditation", DurationDays(10, DurationUnit.Minutes, BuildOrReduce.Build, MonWedFri), "Meditation, 10 min, 3 days a week" },
    };

    [Theory]
    [MemberData(nameof(DerivedNames))]
    public void A_goal_without_its_own_name_is_named_by_its_habit_and_target(string habitName, GoalTargetFields fields, string name)
    {
        Assert.Equal(name, GoalText.Name(Goal(Target(fields), Start, Kyiv), habitName));
    }

    [Fact]
    public void A_renamed_habit_renames_the_goal()
    {
        var goal = Goal(Target(YesNoDays(BuildOrReduce.Build, AllDays)), Start, Kyiv);

        Assert.Equal("Ice bath, daily", GoalText.Name(goal, "Ice bath"));
    }

    [Fact]
    public void An_own_name_wins_over_the_derived_one()
    {
        Assert.True(GoalName.TryCreate("  Morning pages  ", out var own, out _));
        var goal = Goal(Target(CountTimes(20, "pages", 3, SchedulePeriod.Week)), Start, Kyiv, own);

        Assert.Equal("Morning pages", GoalText.Name(goal, "Reading"));
    }

    public static TheoryData<GoalTargetFields, string> Schedules
    {
        get
        {
            var data = new TheoryData<GoalTargetFields, string>();
            var schedules = new (GoalTargetFields Schedule, string Text)[]
            {
                (new() { ScheduleKind = ScheduleKind.FixedDays, Days = MonWedFri.ToHashSet() }, "Mon · Wed · Fri"),
                (new() { ScheduleKind = ScheduleKind.FixedDays, Days = Weekdays.ToHashSet() }, "Mon–Fri"),
                (new()
                {
                    ScheduleKind = ScheduleKind.FixedDays,
                    Days = new HashSet<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Saturday },
                }, "Mon–Wed · Sat"),
                (new() { ScheduleKind = ScheduleKind.FixedDays, Days = new HashSet<DayOfWeek> { DayOfWeek.Saturday, DayOfWeek.Sunday } }, "Sat · Sun"),
                (new() { ScheduleKind = ScheduleKind.FixedDays, Days = AllDays.ToHashSet() }, "Every day"),
                (new() { ScheduleKind = ScheduleKind.Times, Times = 1, Period = SchedulePeriod.Day }, "Every day"),
                (new() { ScheduleKind = ScheduleKind.Times, Times = 2, Period = SchedulePeriod.Day }, "2 times a day"),
                (new() { ScheduleKind = ScheduleKind.Times, Times = 1, Period = SchedulePeriod.Week }, "Flexible · once a week"),
                (new() { ScheduleKind = ScheduleKind.Times, Times = 3, Period = SchedulePeriod.Week }, "Flexible · 3 times a week"),
            };
            var measurements = new GoalTargetFields[]
            {
                new() { Measurement = Measurement.YesNo },
                new() { Measurement = Measurement.Count, Amount = 2, CountUnit = "cups" },
                new() { Measurement = Measurement.Duration, Amount = 15, DurationUnit = DurationUnit.Minutes },
            };
            foreach (var (schedule, text) in schedules)
            {
                foreach (var measurement in measurements)
                {
                    foreach (var side in new[] { BuildOrReduce.Build, BuildOrReduce.Reduce })
                    {
                        var fields = measurement with
                        {
                            BuildOrReduce = side, ScheduleKind = schedule.ScheduleKind, Days = schedule.Days, Times = schedule.Times,
                            Period = schedule.Period,
                        };

                        // A Reduce count or duration goal of N times has N = 1 (FR-004): no such goal exists to describe.
                        if (GoalTarget.TryCreate(fields, out _, out _))
                        {
                            data.Add(fields, text);
                        }
                    }
                }
            }

            return data;
        }
    }

    [Theory]
    [MemberData(nameof(Schedules))]
    public void ScheduleText_for_every_measurement_and_both_intents(GoalTargetFields fields, string text)
    {
        Assert.Equal(text, GoalText.ScheduleText(Target(fields)));
    }

    [Fact]
    public void ScheduleText_covers_every_measurement_and_intent_that_a_goal_can_have()
    {
        // 9 schedules x 3 measurements x 2 intents, less the Reduce count and duration goals of 2 times a day and
        // 3 times a week.
        Assert.Equal((9 * 3 * 2) - (2 * 2), Schedules.Count);
    }

    [Theory]
    [MemberData(nameof(Amounts))]
    public void AmountText_is_the_amount_part_of_the_target_text(GoalTargetFields fields, string? amount)
    {
        Assert.Equal(amount, GoalText.AmountText(Target(fields)));
    }

    public static TheoryData<GoalTargetFields, string?> Amounts => new()
    {
        { DurationDays(15, DurationUnit.Minutes, BuildOrReduce.Build, Weekdays), "15 min" },
        { DurationTimes(1, DurationUnit.Hours, 1, SchedulePeriod.Day, BuildOrReduce.Reduce), "1 h" },
        { CountTimes(20, "pages", 3, SchedulePeriod.Week), "20 pages" },
        { CountTimes(1, "sweet", 1, SchedulePeriod.Day, BuildOrReduce.Reduce), "1 sweet" },
        { CountDays(0, "cigarettes", BuildOrReduce.Reduce, AllDays), "0 cigarettes" },
        { YesNoTimes(3, SchedulePeriod.Week), null },
        { YesNoDays(BuildOrReduce.Reduce, AllDays), null },
    };

    [Fact]
    public void No_culture_is_read()
    {
        using var culture = new CultureScope("ar-SA");

        Assert.Equal("20 pages, 3 times a week", GoalText.TargetText(Target(CountTimes(20, "pages", 3, SchedulePeriod.Week))));
        Assert.Equal("Flexible · 12 times a week", GoalText.ScheduleText(Target(YesNoTimes(12, SchedulePeriod.Week))));
    }
}
