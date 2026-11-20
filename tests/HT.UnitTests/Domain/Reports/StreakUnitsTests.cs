using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Reports;
using static HT.UnitTests.Domain.Goals.GoalSamples;

namespace HT.UnitTests.Domain.Reports;

/// <summary>FR-004: a streak counts commitments, days or weeks, by the goal's schedule kind and period, for Build and Reduce.</summary>
public class StreakUnitsTests
{
    public static TheoryData<string, StreakUnit> Targets => new()
    {
        { "Build yes/no on Mon, Wed, Fri", StreakUnit.Commitments },
        { "Reduce yes/no on Mon, Wed, Fri", StreakUnit.Commitments },
        { "Build 20 min on every day", StreakUnit.Commitments },
        { "Reduce at most 2 sweets on weekdays", StreakUnit.Commitments },
        { "Build yes/no 2 times per day", StreakUnit.Days },
        { "Reduce at most 120 min once per day", StreakUnit.Days },
        { "Build 30 min 3 times per week", StreakUnit.Weeks },
        { "Reduce yes/no at most 2 times per week", StreakUnit.Weeks },
        { "Reduce at most 10 pages once per week", StreakUnit.Weeks },
    };

    private static GoalTarget TargetNamed(string name) => name switch
    {
        "Build yes/no on Mon, Wed, Fri" => Target(YesNoDays(BuildOrReduce.Build, MonWedFri)),
        "Reduce yes/no on Mon, Wed, Fri" => Target(YesNoDays(BuildOrReduce.Reduce, MonWedFri)),
        "Build 20 min on every day" => Target(DurationDays(20, DurationUnit.Minutes, BuildOrReduce.Build, AllDays)),
        "Reduce at most 2 sweets on weekdays" => Target(CountDays(2, "sweets", BuildOrReduce.Reduce, Weekdays)),
        "Build yes/no 2 times per day" => Target(YesNoTimes(2, SchedulePeriod.Day)),
        "Reduce at most 120 min once per day" => Target(DurationTimes(120, DurationUnit.Minutes, 1, SchedulePeriod.Day, BuildOrReduce.Reduce)),
        "Build 30 min 3 times per week" => Target(DurationTimes(30, DurationUnit.Minutes, 3, SchedulePeriod.Week)),
        "Reduce yes/no at most 2 times per week" => Target(YesNoTimes(2, SchedulePeriod.Week, BuildOrReduce.Reduce)),
        "Reduce at most 10 pages once per week" => Target(CountTimes(10, "pages", 1, SchedulePeriod.Week, BuildOrReduce.Reduce)),
        _ => throw new ArgumentOutOfRangeException(nameof(name)),
    };

    [Theory]
    [MemberData(nameof(Targets))]
    public void FR_004_the_unit_follows_the_schedule_kind_and_the_period(string target, StreakUnit unit)
    {
        Assert.Equal(unit, StreakUnits.Of(TargetNamed(target)));
    }
}
