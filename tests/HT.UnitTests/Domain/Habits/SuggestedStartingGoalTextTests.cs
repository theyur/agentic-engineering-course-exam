using HT.Domain.Habits;
using static HT.UnitTests.Domain.Habits.GoalFields;

namespace HT.UnitTests.Domain.Habits;

/// <summary>FR-017: every example of data-model.md's table (US2-AS1, US2-AS3, US3-AS17) and the run-rule boundaries.</summary>
public class SuggestedStartingGoalTextTests
{
    public static TheoryData<SuggestedStartingGoalFields, string, string> Examples => new()
    {
        { DurationDays(15, DurationUnit.Minutes, Weekdays), "15 min, 5 days a week", "Duration · fixed days Mon–Fri" },
        { CountTimes(20, "pages", 3, SchedulePeriod.Week), "20 pages, 3 times a week", "Count · flexible" },
        { YesNoTimes(1, SchedulePeriod.Day), "once a day", "Yes/no · flexible" },
        { DurationDays(30, DurationUnit.Minutes, AllDays), "30 min daily", "Duration · fixed days every day" },
        { DurationTimes(2, DurationUnit.Hours, 1, SchedulePeriod.Day, BuildOrReduce.Reduce), "at most 2 h a day", "Duration · flexible" },
        { CountTimes(1, "sweet", 1, SchedulePeriod.Day, BuildOrReduce.Reduce), "at most 1 sweet a day", "Count · flexible" },
        { YesNoTimes(1, SchedulePeriod.Week), "once a week", "Yes/no · flexible" },
        { DurationDays(1, DurationUnit.Hours, DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday), "1 h, 3 days a week", "Duration · fixed days Mon, Wed, Fri" },
        { CountDays(10, "pages", DayOfWeek.Saturday), "10 pages, once a week", "Count · fixed days Sat" },
        { YesNoTimes(1, SchedulePeriod.Day, BuildOrReduce.Reduce), "at most once a day", "Yes/no · flexible" },
        { YesNoDays(DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Saturday), "4 days a week", "Yes/no · fixed days Mon–Wed, Sat" },
        { YesNoDays(DayOfWeek.Saturday, DayOfWeek.Sunday), "2 days a week", "Yes/no · fixed days Sat, Sun" },
        { CountTimes(0, "sweets", 1, SchedulePeriod.Day, BuildOrReduce.Reduce), "at most 0 sweets a day", "Count · flexible" },
    };

    [Theory]
    [MemberData(nameof(Examples))]
    public void Reproduces_every_example(SuggestedStartingGoalFields fields, string text, string kind)
    {
        Assert.Equal((text, kind), SuggestedStartingGoalText.Format(Goal(fields)));
    }

    [Fact]
    public void Two_consecutive_days_are_not_a_run()
    {
        Assert.Equal("Yes/no · fixed days Mon, Tue", SuggestedStartingGoalText.Format(Goal(YesNoDays(DayOfWeek.Monday, DayOfWeek.Tuesday))).Kind);
    }

    [Fact]
    public void Three_consecutive_days_are_a_run()
    {
        Assert.Equal("Yes/no · fixed days Mon–Wed",
            SuggestedStartingGoalText.Format(Goal(YesNoDays(DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday))).Kind);
    }

    [Fact]
    public void The_week_does_not_wrap_from_Sunday_to_Monday()
    {
        Assert.Equal("Yes/no · fixed days Mon, Sun", SuggestedStartingGoalText.Format(Goal(YesNoDays(DayOfWeek.Sunday, DayOfWeek.Monday))).Kind);
        Assert.Equal("Yes/no · fixed days Mon, Fri–Sun",
            SuggestedStartingGoalText.Format(Goal(YesNoDays(DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday, DayOfWeek.Monday))).Kind);
    }

    [Fact]
    public void All_seven_days_are_daily_and_every_day()
    {
        Assert.Equal(("daily", "Yes/no · fixed days every day"), SuggestedStartingGoalText.Format(Goal(YesNoDays(AllDays))));
    }

    [Fact]
    public void N_times_a_day_and_the_hours_unit()
    {
        Assert.Equal("3 times a day", SuggestedStartingGoalText.Format(Goal(YesNoTimes(3, SchedulePeriod.Day))).Text);
        Assert.Equal("2 glasses, 3 times a day", SuggestedStartingGoalText.Format(Goal(CountTimes(2, "glasses", 3, SchedulePeriod.Day))).Text);
        Assert.Equal("2 h, 3 times a week", SuggestedStartingGoalText.Format(Goal(DurationTimes(2, DurationUnit.Hours, 3, SchedulePeriod.Week))).Text);
        Assert.Equal("20 min a day", SuggestedStartingGoalText.Format(Goal(DurationTimes(20, DurationUnit.Minutes, 1, SchedulePeriod.Day))).Text);
    }

    [Fact]
    public void Does_not_depend_on_the_current_culture()
    {
        using var culture = new CultureScope("ar-SA");
        Assert.Equal("at most 999 sweets a day", SuggestedStartingGoalText.Format(Goal(CountTimes(999, "sweets", 1, SchedulePeriod.Day, BuildOrReduce.Reduce))).Text);
    }
}
