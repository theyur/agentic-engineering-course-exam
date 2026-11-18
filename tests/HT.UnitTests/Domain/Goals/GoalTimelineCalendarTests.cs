using HT.Domain.Goals;
using HT.Domain.Habits;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Domain.Goals;

/// <summary>
/// SC-003, US4: the evaluation calendar of each schedule kind, its range, the boundaries at midnight ± 1 second and
/// on clock-change days. Kyiv is UTC+2 in November 2026, so its midnight of day d is d − 1 at 22:00 UTC; New York
/// is UTC−5 then.
/// </summary>
public class GoalTimelineCalendarTests
{
    private static readonly DateOnly Wednesday = Date(2026, 11, 18);
    private static readonly DateOnly MondayBefore = Date(2026, 11, 16);
    private static readonly DateOnly MondayAfter = Date(2026, 11, 23);

    private static EvaluationCalendar Calendar(Goal goal, DateOnly first, DateOnly last)
    {
        Assert.True(GoalTimeline.TryCalendar(goal, first, last, out var calendar));
        return calendar;
    }

    [Fact]
    public void US4_AS1_one_commitment_per_scheduled_day_on_or_after_the_start_date()
    {
        var goal = Goal(OnDays(MonWedFri), Wednesday, Kyiv);

        var calendar = Calendar(goal, MondayBefore, MondayBefore.AddDays(13));

        Assert.Equal(GoalScheduleKind.CommitmentBased, calendar.Kind);
        Assert.Empty(calendar.Periods);
        Assert.Equal(
            [
                new Commitment(Date(2026, 11, 18), Utc("2026-11-17T22:00:00Z"), Utc("2026-11-18T22:00:00Z")),
                new Commitment(Date(2026, 11, 20), Utc("2026-11-19T22:00:00Z"), Utc("2026-11-20T22:00:00Z")),
                new Commitment(Date(2026, 11, 23), Utc("2026-11-22T22:00:00Z"), Utc("2026-11-23T22:00:00Z")),
                new Commitment(Date(2026, 11, 25), Utc("2026-11-24T22:00:00Z"), Utc("2026-11-25T22:00:00Z")),
                new Commitment(Date(2026, 11, 27), Utc("2026-11-26T22:00:00Z"), Utc("2026-11-27T22:00:00Z")),
            ],
            calendar.Commitments);
    }

    [Fact]
    public void US4_AS2_one_period_per_day()
    {
        var goal = Goal(PerDay(), Wednesday, NewYork);

        var calendar = Calendar(goal, Wednesday, Wednesday.AddDays(6));

        Assert.Equal(GoalScheduleKind.PeriodBased, calendar.Kind);
        Assert.Empty(calendar.Commitments);
        Assert.Equal(7, calendar.Periods.Count);
        Assert.All(calendar.Periods, period => Assert.Equal(period.FirstDate, period.LastDate));
        Assert.Equal(Enumerable.Range(0, 7).Select(Wednesday.AddDays), calendar.Periods.Select(p => p.FirstDate));
        Assert.Equal(new EvaluationPeriod(Wednesday, Wednesday, Utc("2026-11-18T05:00:00Z"), Utc("2026-11-19T05:00:00Z")), calendar.Periods[0]);
        Assert.Equal(new EvaluationPeriod(Date(2026, 11, 24), Date(2026, 11, 24), Utc("2026-11-24T05:00:00Z"), Utc("2026-11-25T05:00:00Z")), calendar.Periods[6]);
    }

    [Fact]
    public void US4_AS3_one_period_per_week_from_the_first_Monday_on_or_after_the_start_date()
    {
        var goal = Goal(PerWeek(), Wednesday, Kyiv);

        var calendar = Calendar(goal, MondayBefore, Date(2026, 12, 6));

        Assert.Equal(GoalScheduleKind.PeriodBased, calendar.Kind);
        Assert.Equal(
            [
                new EvaluationPeriod(Date(2026, 11, 23), Date(2026, 11, 29), Utc("2026-11-22T22:00:00Z"), Utc("2026-11-29T22:00:00Z")),
                new EvaluationPeriod(Date(2026, 11, 30), Date(2026, 12, 6), Utc("2026-11-29T22:00:00Z"), Utc("2026-12-06T22:00:00Z")),
            ],
            calendar.Periods);
    }

    [Fact]
    public void US4_AS3_three_weeks_from_the_start_date()
    {
        var goal = Goal(PerWeek(), Wednesday, Kyiv);

        var calendar = Calendar(goal, Wednesday, Date(2026, 12, 13));

        Assert.Equal([Date(2026, 11, 23), Date(2026, 11, 30), Date(2026, 12, 7)], calendar.Periods.Select(p => p.FirstDate));
        Assert.Equal([Date(2026, 11, 29), Date(2026, 12, 6), Date(2026, 12, 13)], calendar.Periods.Select(p => p.LastDate));
    }

    [Fact]
    public void Every_week_that_overlaps_the_range_is_given_whole()
    {
        var goal = Goal(PerWeek(), Wednesday, Kyiv);

        var calendar = Calendar(goal, Date(2026, 11, 25), Date(2026, 12, 1));

        Assert.Equal(
            [(Date(2026, 11, 23), Date(2026, 11, 29)), (Date(2026, 11, 30), Date(2026, 12, 6))],
            calendar.Periods.Select(p => (p.FirstDate, p.LastDate)));
    }

    [Fact]
    public void The_days_before_the_first_week_belong_to_no_period()
    {
        var goal = Goal(PerWeek(), Wednesday, Kyiv);

        Assert.Empty(Calendar(goal, Wednesday, Date(2026, 11, 22)).Periods);
    }

    [Fact]
    public void US4_AS6_a_range_that_ends_before_the_start_date_is_empty()
    {
        var last = Wednesday.AddDays(-1);

        var commitments = Calendar(Goal(OnDays(AllDays), Wednesday, Kyiv), Date(2026, 11, 1), last);
        var days = Calendar(Goal(PerDay(), Wednesday, Kyiv), Date(2026, 11, 1), last);
        var weeks = Calendar(Goal(PerWeek(), Wednesday, Kyiv), Date(2026, 11, 1), last);

        Assert.Empty(commitments.Commitments);
        Assert.Equal(GoalScheduleKind.CommitmentBased, commitments.Kind);
        Assert.Empty(days.Periods);
        Assert.Empty(weeks.Periods);
        Assert.Equal(GoalScheduleKind.PeriodBased, weeks.Kind);
    }

    [Fact]
    public void US4_AS7_the_range_has_1_to_366_days()
    {
        var goal = Goal(PerDay(), Wednesday, Kyiv);

        Assert.False(GoalTimeline.TryCalendar(goal, Wednesday, Wednesday.AddDays(-1), out var none));
        Assert.Null(none);
        Assert.False(GoalTimeline.TryCalendar(goal, Wednesday, Date(2027, 11, 19), out _));
        Assert.Single(Calendar(goal, Wednesday, Wednesday).Periods);
        Assert.Equal(366, Calendar(goal, Wednesday, Date(2027, 11, 18)).Periods.Count);
        Assert.Equal(366, GoalTimeline.MaxCalendarDays);
    }

    public static TheoryData<string, string, DateOnly> AroundMidnight => new()
    {
        { "Europe/Kyiv", "2026-11-17T21:59:59Z", Date(2026, 11, 17) },
        { "Europe/Kyiv", "2026-11-17T22:00:00Z", Date(2026, 11, 18) },
        { "Europe/Kyiv", "2026-11-17T22:00:01Z", Date(2026, 11, 18) },
        { "America/New_York", "2026-11-18T04:59:59Z", Date(2026, 11, 17) },
        { "America/New_York", "2026-11-18T05:00:00Z", Date(2026, 11, 18) },
        { "America/New_York", "2026-11-18T05:00:01Z", Date(2026, 11, 18) },
        { "UTC", "2026-11-17T23:59:59Z", Date(2026, 11, 17) },
        { "UTC", "2026-11-18T00:00:00Z", Date(2026, 11, 18) },
        { "UTC", "2026-11-18T00:00:01Z", Date(2026, 11, 18) },
    };

    [Theory]
    [MemberData(nameof(AroundMidnight))]
    public void The_current_date_changes_at_midnight_of_the_goals_zone(string zone, string instant, DateOnly current)
    {
        var goal = Goal(PerDay(), MondayBefore, Zone(zone));

        Assert.Equal(current, GoalTimeline.CurrentDate(goal, Utc(instant)));
    }

    public static TheoryData<string, string, string, string> WednesdayMidnights => new()
    {
        { "Europe/Kyiv", "2026-11-17T22:00:00Z", "2026-11-18T22:00:00Z", "2026-11-19T22:00:00Z" },
        { "America/New_York", "2026-11-18T05:00:00Z", "2026-11-19T05:00:00Z", "2026-11-20T05:00:00Z" },
        { "UTC", "2026-11-18T00:00:00Z", "2026-11-19T00:00:00Z", "2026-11-20T00:00:00Z" },
    };

    [Theory]
    [MemberData(nameof(WednesdayMidnights))]
    public void A_commitment_runs_from_midnight_to_midnight_and_is_next_until_its_last_second(
        string zone, string starts, string ends, string fridayStarts)
    {
        var goal = Goal(OnDays(MonWedFri), MondayBefore, Zone(zone));
        var commitment = Calendar(goal, Wednesday, Wednesday).Commitments.Single();

        Assert.Equal(new Commitment(Wednesday, Utc(starts), Utc(ends)), commitment);
        Assert.Equal(Wednesday, GoalTimeline.NextCommitment(goal, Utc(starts).AddSeconds(-1)));
        Assert.Equal(Wednesday, GoalTimeline.NextCommitment(goal, Utc(ends).AddSeconds(-1)));
        Assert.Equal(Date(2026, 11, 20), GoalTimeline.NextCommitment(goal, Utc(ends)));
        Assert.Equal(Date(2026, 11, 20), GoalTimeline.NextCommitment(goal, Utc(ends).AddSeconds(1)));
        Assert.Equal(Utc(fridayStarts), Calendar(goal, Date(2026, 11, 20), Date(2026, 11, 20)).Commitments.Single().Starts);
    }

    public static TheoryData<string, DateOnly, string, string, int> ClockChangeDays => new()
    {
        // Kyiv: 28 March 2027 has 23 hours (EET to EEST), 31 October 2027 has 25 (EEST to EET).
        { "Europe/Kyiv", Date(2027, 3, 28), "2027-03-27T22:00:00Z", "2027-03-28T21:00:00Z", 23 },
        { "Europe/Kyiv", Date(2027, 10, 31), "2027-10-30T21:00:00Z", "2027-10-31T22:00:00Z", 25 },
        // New York: 14 March 2027 has 23 hours (EST to EDT), 7 November 2027 has 25 (EDT to EST).
        { "America/New_York", Date(2027, 3, 14), "2027-03-14T05:00:00Z", "2027-03-15T04:00:00Z", 23 },
        { "America/New_York", Date(2027, 11, 7), "2027-11-07T04:00:00Z", "2027-11-08T05:00:00Z", 25 },
        // Havana: midnight of 14 March 2027 does not exist (00:00 CST jumps to 01:00 CDT, embedded tzdb 2026b), so
        // the day starts at 01:00 CDT and has 23 hours; the day before it has 24.
        { "America/Havana", Date(2027, 3, 14), "2027-03-14T05:00:00Z", "2027-03-15T04:00:00Z", 23 },
        { "America/Havana", Date(2027, 3, 13), "2027-03-13T05:00:00Z", "2027-03-14T05:00:00Z", 24 },
    };

    [Theory]
    [MemberData(nameof(ClockChangeDays))]
    public void US4_AS5_a_clock_change_day_still_runs_from_local_midnight_to_local_midnight(
        string zone, DateOnly day, string starts, string ends, int hours)
    {
        var start = Date(2027, 3, 1);
        var commitment = Calendar(Goal(OnDays(day.DayOfWeek), start, Zone(zone)), day.AddDays(-3), day.AddDays(3)).Commitments.Single();
        var period = Calendar(Goal(PerDay(), start, Zone(zone)), day, day).Periods.Single();

        Assert.Equal(new Commitment(day, Utc(starts), Utc(ends)), commitment);
        Assert.Equal(new EvaluationPeriod(day, day, Utc(starts), Utc(ends)), period);
        Assert.Equal(TimeSpan.FromHours(hours), period.Ends - period.Starts);
    }

    [Fact]
    public void A_week_with_a_clock_change_runs_from_Monday_midnight_to_Monday_midnight()
    {
        var goal = Goal(PerWeek(), Date(2027, 3, 1), Kyiv);

        var spring = Calendar(goal, Date(2027, 3, 28), Date(2027, 3, 28)).Periods.Single();
        var autumn = Calendar(goal, Date(2027, 10, 31), Date(2027, 10, 31)).Periods.Single();

        Assert.Equal(new EvaluationPeriod(Date(2027, 3, 22), Date(2027, 3, 28), Utc("2027-03-21T22:00:00Z"), Utc("2027-03-28T21:00:00Z")), spring);
        Assert.Equal(TimeSpan.FromHours((7 * 24) - 1), spring.Ends - spring.Starts);
        Assert.Equal(new EvaluationPeriod(Date(2027, 10, 25), Date(2027, 10, 31), Utc("2027-10-24T21:00:00Z"), Utc("2027-10-31T22:00:00Z")), autumn);
        Assert.Equal(TimeSpan.FromHours((7 * 24) + 1), autumn.Ends - autumn.Starts);
    }

    [Fact]
    public void A_day_whose_midnight_does_not_exist_begins_at_its_first_instant()
    {
        var goal = Goal(PerDay(), Date(2027, 3, 1), Zone("America/Havana"));

        Assert.Equal(Date(2027, 3, 13), GoalTimeline.CurrentDate(goal, Utc("2027-03-14T04:59:59Z")));
        Assert.Equal(Date(2027, 3, 14), GoalTimeline.CurrentDate(goal, Utc("2027-03-14T05:00:00Z")));
    }

    [Fact]
    public void FirstWeekStart_is_the_next_Monday_from_a_Wednesday_and_the_start_date_from_a_Monday()
    {
        Assert.Equal(MondayAfter, GoalTimeline.FirstWeekStart(Goal(PerWeek(), Wednesday, Kyiv)));
        Assert.Equal(MondayAfter, GoalTimeline.FirstWeekStart(Goal(PerWeek(), MondayAfter, Kyiv)));
        Assert.Equal(MondayAfter, GoalTimeline.FirstWeekStart(Goal(PerWeek(), Date(2026, 11, 22), Kyiv)));
    }

    [Fact]
    public void Only_a_per_week_goal_has_a_first_week()
    {
        Assert.Null(GoalTimeline.FirstWeekStart(Goal(PerDay(), Wednesday, Kyiv)));
        Assert.Null(GoalTimeline.FirstWeekStart(Goal(OnDays(MonWedFri), Wednesday, Kyiv)));
    }

    [Fact]
    public void Only_a_commitment_based_goal_has_a_next_commitment()
    {
        Assert.Null(GoalTimeline.NextCommitment(Goal(PerDay(), Wednesday, Kyiv), Utc("2026-11-18T10:00:00Z")));
        Assert.Null(GoalTimeline.NextCommitment(Goal(PerWeek(), Wednesday, Kyiv), Utc("2026-11-18T10:00:00Z")));
    }

    [Fact]
    public void A_goal_that_has_not_started_gives_its_first_scheduled_day_as_next()
    {
        var goal = Goal(OnDays(DayOfWeek.Friday), Wednesday, Kyiv);

        Assert.Equal(Date(2026, 11, 20), GoalTimeline.NextCommitment(goal, Utc("2026-11-10T10:00:00Z")));
    }

    [Fact]
    public void VersionOf_gives_the_version_in_effect_for_a_date()
    {
        var goal = GoalVersionChange.Add(Date(2026, 11, 19), NewYork).ApplyTo(Goal(PerDay(), MondayBefore, Kyiv));

        Assert.Equal(Kyiv, GoalTimeline.VersionOf(goal, MondayBefore).Settings.TimeZone);
        Assert.Equal(Kyiv, GoalTimeline.VersionOf(goal, Wednesday).Settings.TimeZone);
        Assert.Equal(NewYork, GoalTimeline.VersionOf(goal, Date(2026, 11, 19)).Settings.TimeZone);
        Assert.Equal(NewYork, GoalTimeline.VersionOf(goal, Date(2027, 1, 1)).Settings.TimeZone);
        Assert.Same(goal.Versions[0], GoalTimeline.VersionOf(goal, Date(2026, 1, 1)));
    }

    [Fact]
    public void No_culture_is_read()
    {
        using var culture = new CultureScope("ar-SA");
        var goal = Goal(OnDays(MonWedFri), Wednesday, Kyiv);

        Assert.Equal(5, Calendar(goal, MondayBefore, MondayBefore.AddDays(13)).Commitments.Count);
        Assert.Equal(Wednesday, GoalTimeline.CurrentDate(goal, Utc("2026-11-18T10:00:00Z")));
        Assert.Equal(Wednesday, GoalTimeline.NextCommitment(goal, Utc("2026-11-18T10:00:00Z")));
        Assert.Equal(GoalVersionChange.Add(Date(2026, 11, 19), NewYork), GoalTimeline.ChangeOf(goal, Utc("2026-11-18T10:00:00Z"), NewYork));
    }

    [Fact]
    public void The_schedule_of_a_period_based_goal_ignores_days()
    {
        var goal = Goal(Target(YesNoTimes(2, SchedulePeriod.Day) with { Days = MonWedFri.ToHashSet() }), Wednesday, Kyiv);

        Assert.Equal(7, Calendar(goal, Wednesday, Wednesday.AddDays(6)).Periods.Count);
    }
}
