using System.Globalization;
using HT.Application.Goals;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.UnitTests.Fakes;
using static HT.UnitTests.Application.Goals.GoalInputs;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Goals;

/// <summary>
/// <c>GetCalendarAsync</c> (US4-AS1 to AS7, FR-012, FR-013, FR-027), through <c>FakeGoalStore</c>: commitments and
/// periods of each schedule kind at the goal's own midnights, whole weeks that overlap the range, clock-change days, an
/// empty calendar and the range check. The clock is 12:00 in Kyiv on Wednesday 18 November 2026 and the owner is in
/// Kyiv. Kyiv is UTC+2 in November 2026, so its midnight of day d is d − 1 at 22:00 UTC; in 2027 it is UTC+3 from
/// 28 March to 30 October. New York is UTC−5 in November 2026. Every instant below is worked out by hand. The refusals of
/// the acting user and the goal identifier, in FR-023's order, are in <c>GoalAccessTests</c>.
/// </summary>
public sealed class GoalCalendarTests
{
    private static readonly DateOnly Mon16 = Date(2026, 11, 16);
    private static readonly DateOnly Wed18 = Date(2026, 11, 18);
    private static readonly DateOnly Thu19 = Date(2026, 11, 19);
    private static readonly DateOnly Fri20 = Date(2026, 11, 20);
    private static readonly DateOnly Sun22 = Date(2026, 11, 22);
    private static readonly DateOnly Mon23 = Date(2026, 11, 23);
    private static readonly DateOnly Tue24 = Date(2026, 11, 24);
    private static readonly DateOnly Wed25 = Date(2026, 11, 25);
    private static readonly DateOnly Thu26 = Date(2026, 11, 26);
    private static readonly DateOnly Sun29 = Date(2026, 11, 29);
    private static readonly DateOnly Mon30 = Date(2026, 11, 30);
    private static readonly DateOnly Tue01 = Date(2026, 12, 1);
    private static readonly DateOnly Sun06 = Date(2026, 12, 6);

    private readonly GoalWorld _world = new();
    private readonly CatalogHabit _reading;
    private readonly ActingUser _user;

    public GoalCalendarTests()
    {
        _reading = _world.CatalogHabits.Add(HabitSamples.Reading());
        _user = RegisteredUser(_world.Accounts);
    }

    private Task<GoalCalendarResult> GetCalendarAsync(ActingUser user, GoalId id, DateOnly first, DateOnly last) =>
        _world.Service.GetCalendarAsync(user, id, first, last, CancellationToken.None);

    private Task<GoalCalendarView> CalendarAsync(GoalId id, DateOnly first, DateOnly last) => CalendarAsync(_user, id, first, last);

    private async Task<GoalCalendarView> CalendarAsync(ActingUser user, GoalId id, DateOnly first, DateOnly last)
    {
        var result = await GetCalendarAsync(user, id, first, last);
        Assert.Equal(GoalCalendarOutcome.Found, result.Outcome);
        Assert.Null(result.Message);
        return result.Calendar!;
    }

    /// <summary>A goal of the owner, in Kyiv, created through the service on a Published catalog habit.</summary>
    private async Task<GoalId> CreateAsync(GoalTargetFields target, DateOnly start)
    {
        var created = await _world.Service.CreateAsync(_user, Input(Catalog(_reading), start, target), CancellationToken.None);
        Assert.Equal(GoalCreateOutcome.Created, created.Outcome);
        return created.GoalId!.Value;
    }

    private static DateOnly Day(string text) => DateOnly.ParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture);

    [Fact]
    public async Task US4_AS1_a_Mon_Wed_Fri_goal_starting_on_a_Wednesday_has_5_commitments_in_the_14_days_from_that_Monday()
    {
        var goal = await CreateAsync(YesNoDays(BuildOrReduce.Build, MonWedFri), Wed18);

        var calendar = await CalendarAsync(goal, Mon16, Sun29);

        Assert.Equal(GoalScheduleKind.CommitmentBased, calendar.Kind);
        Assert.Empty(calendar.Periods);
        Assert.Equal(
            [
                new Commitment(Wed18, Utc("2026-11-17T22:00:00Z"), Utc("2026-11-18T22:00:00Z")),
                new Commitment(Fri20, Utc("2026-11-19T22:00:00Z"), Utc("2026-11-20T22:00:00Z")),
                new Commitment(Mon23, Utc("2026-11-22T22:00:00Z"), Utc("2026-11-23T22:00:00Z")),
                new Commitment(Wed25, Utc("2026-11-24T22:00:00Z"), Utc("2026-11-25T22:00:00Z")),
                new Commitment(Date(2026, 11, 27), Utc("2026-11-26T22:00:00Z"), Utc("2026-11-27T22:00:00Z")),
            ],
            calendar.Commitments);
    }

    [Fact]
    public async Task US4_AS2_a_goal_of_N_times_per_day_has_one_period_per_day_for_7_days()
    {
        var goal = await CreateAsync(YesNoTimes(2, SchedulePeriod.Day), Wed18);

        var calendar = await CalendarAsync(goal, Wed18, Tue24);

        Assert.Equal(GoalScheduleKind.PeriodBased, calendar.Kind);
        Assert.Empty(calendar.Commitments);
        Assert.Equal(Enumerable.Range(0, 7).Select(Wed18.AddDays), calendar.Periods.Select(p => p.FirstDate));
        Assert.All(calendar.Periods, period => Assert.Equal(period.FirstDate, period.LastDate));
        Assert.Equal(new EvaluationPeriod(Wed18, Wed18, Utc("2026-11-17T22:00:00Z"), Utc("2026-11-18T22:00:00Z")), calendar.Periods[0]);
        Assert.Equal(new EvaluationPeriod(Thu19, Thu19, Utc("2026-11-18T22:00:00Z"), Utc("2026-11-19T22:00:00Z")), calendar.Periods[1]);
        Assert.Equal(new EvaluationPeriod(Tue24, Tue24, Utc("2026-11-23T22:00:00Z"), Utc("2026-11-24T22:00:00Z")), calendar.Periods[6]);
    }

    [Fact]
    public async Task US4_AS3_a_per_week_goal_starting_on_a_Wednesday_has_whole_weeks_from_the_Monday_after_over_three_weeks()
    {
        var goal = await CreateAsync(YesNoTimes(3, SchedulePeriod.Week), Wed18);

        // Three weeks: Mon 16 to Sun 22 (the start date's week, which belongs to no period), Mon 23 to Sun 29, Mon 30 to Sun 6.
        var calendar = await CalendarAsync(goal, Mon16, Sun06);

        Assert.Equal(GoalScheduleKind.PeriodBased, calendar.Kind);
        Assert.Empty(calendar.Commitments);
        Assert.Equal(
            [
                new EvaluationPeriod(Mon23, Sun29, Utc("2026-11-22T22:00:00Z"), Utc("2026-11-29T22:00:00Z")),
                new EvaluationPeriod(Mon30, Sun06, Utc("2026-11-29T22:00:00Z"), Utc("2026-12-06T22:00:00Z")),
            ],
            calendar.Periods);
    }

    [Fact]
    public async Task FR_013_every_week_that_overlaps_the_range_is_given_whole_from_Monday_to_Sunday()
    {
        var goal = await CreateAsync(YesNoTimes(3, SchedulePeriod.Week), Mon23);
        var week23 = new EvaluationPeriod(Mon23, Sun29, Utc("2026-11-22T22:00:00Z"), Utc("2026-11-29T22:00:00Z"));
        var week30 = new EvaluationPeriod(Mon30, Sun06, Utc("2026-11-29T22:00:00Z"), Utc("2026-12-06T22:00:00Z"));

        // Wednesday 25 November to the Tuesday after: both weeks it touches.
        Assert.Equal([week23, week30], (await CalendarAsync(goal, Wed25, Tue01)).Periods);

        // Tuesday 24 to Thursday 26 November, inside one week: that week.
        Assert.Equal([week23], (await CalendarAsync(goal, Tue24, Thu26)).Periods);
    }

    [Theory]
    [InlineData(18, 23)]
    [InlineData(23, 23)]
    public async Task US4_AS4_the_first_period_of_a_per_week_goal_is_the_first_week_that_reading_gives(int startDay, int mondayDay)
    {
        var start = Date(2026, 11, startDay);
        var monday = Date(2026, 11, mondayDay);
        var goal = await CreateAsync(YesNoTimes(3, SchedulePeriod.Week), start);

        var read = await _world.Service.GetAsync(_user, goal, CancellationToken.None);
        var calendar = await CalendarAsync(goal, start, start.AddDays(13));

        Assert.Equal(monday, read.Goal!.FirstWeekStarts);
        Assert.Equal(monday, calendar.Periods[0].FirstDate);
    }

    [Theory]
    // 28 March 2027 in Kyiv lasts 23 hours (EET to EEST at 03:00 local time).
    [InlineData("2027-03-22", "2027-03-27",
        "2027-03-26T22:00:00Z", "2027-03-27T22:00:00Z", "2027-03-28T21:00:00Z", "2027-03-29T21:00:00Z", 24, 23, 24)]
    // 31 October 2027 in Kyiv lasts 25 hours (EEST to EET at 04:00 local time).
    [InlineData("2027-10-25", "2027-10-30",
        "2027-10-29T21:00:00Z", "2027-10-30T21:00:00Z", "2027-10-31T22:00:00Z", "2027-11-01T22:00:00Z", 24, 25, 24)]
    public async Task US4_AS5_a_clock_change_day_still_runs_from_local_midnight_to_local_midnight(
        string start, string first, string day1, string day2, string day3, string day4, int hours1, int hours2, int hours3)
    {
        // Both start dates are within 365 days of Wednesday 18 November 2026, so the service creates the goal.
        var goal = await CreateAsync(YesNoTimes(1, SchedulePeriod.Day), Day(start));
        var firstDate = Day(first);

        var periods = (await CalendarAsync(goal, firstDate, firstDate.AddDays(2))).Periods;

        Assert.Equal(
            [
                new EvaluationPeriod(firstDate, firstDate, Utc(day1), Utc(day2)),
                new EvaluationPeriod(firstDate.AddDays(1), firstDate.AddDays(1), Utc(day2), Utc(day3)),
                new EvaluationPeriod(firstDate.AddDays(2), firstDate.AddDays(2), Utc(day3), Utc(day4)),
            ],
            periods);
        Assert.Equal([hours1, hours2, hours3], periods.Select(p => (int)(p.Ends - p.Starts).TotalHours));
    }

    [Fact]
    public async Task US4_AS5_the_commitment_and_the_week_of_28_March_2027_last_23_and_167_hours_in_Kyiv()
    {
        var monday = Date(2027, 3, 22);
        var sunday = Date(2027, 3, 28);
        var sundays = await CreateAsync(YesNoDays(BuildOrReduce.Build, DayOfWeek.Sunday), monday);
        var weekly = await CreateAsync(YesNoTimes(3, SchedulePeriod.Week), monday);

        var commitment = Assert.Single((await CalendarAsync(sundays, monday, sunday)).Commitments);
        var week = Assert.Single((await CalendarAsync(weekly, sunday, sunday)).Periods);

        Assert.Equal(new Commitment(sunday, Utc("2027-03-27T22:00:00Z"), Utc("2027-03-28T21:00:00Z")), commitment);
        Assert.Equal(TimeSpan.FromHours(23), commitment.Ends - commitment.Starts);
        Assert.Equal(new EvaluationPeriod(monday, sunday, Utc("2027-03-21T22:00:00Z"), Utc("2027-03-28T21:00:00Z")), week);
        Assert.Equal(TimeSpan.FromHours(167), week.Ends - week.Starts);
    }

    [Fact]
    public async Task FR_027_after_a_time_zone_change_each_day_keeps_the_midnights_of_its_own_version()
    {
        var user = RegisteredUser(_world.Accounts, "America/New_York");
        var goal = _world.AddGoal(user.AccountId!.Value, _reading, PerDay(), (Mon16, Zone(GoalInputs.Kyiv)), (Thu19, NewYork));

        var periods = (await CalendarAsync(user, goal.Id, Wed18, Fri20)).Periods;

        // Wednesday ends at midnight in Kyiv and Thursday begins at midnight in New York, 7 hours later: the gap is kept.
        Assert.Equal(
            [
                new EvaluationPeriod(Wed18, Wed18, Utc("2026-11-17T22:00:00Z"), Utc("2026-11-18T22:00:00Z")),
                new EvaluationPeriod(Thu19, Thu19, Utc("2026-11-19T05:00:00Z"), Utc("2026-11-20T05:00:00Z")),
                new EvaluationPeriod(Fri20, Fri20, Utc("2026-11-20T05:00:00Z"), Utc("2026-11-21T05:00:00Z")),
            ],
            periods);
    }

    [Fact]
    public async Task US4_AS6_a_range_that_ends_before_the_start_date_gives_an_empty_calendar()
    {
        var fixedDays = await CreateAsync(YesNoDays(BuildOrReduce.Build, MonWedFri), Wed25);
        var daily = await CreateAsync(YesNoTimes(1, SchedulePeriod.Day), Wed25);

        var commitments = await CalendarAsync(fixedDays, Mon16, Tue24);
        var days = await CalendarAsync(daily, Mon16, Tue24);

        Assert.Equal(GoalScheduleKind.CommitmentBased, commitments.Kind);
        Assert.Empty(commitments.Commitments);
        Assert.Empty(commitments.Periods);
        Assert.Equal(GoalScheduleKind.PeriodBased, days.Kind);
        Assert.Empty(days.Periods);
        Assert.Empty(days.Commitments);
    }

    [Fact]
    public async Task US4_AS6_a_range_that_ends_before_a_per_week_goals_first_week_gives_an_empty_calendar()
    {
        var goal = await CreateAsync(YesNoTimes(3, SchedulePeriod.Week), Wed18);

        // Wednesday 18 to Sunday 22 November: the start date's week, before the first week starts on Monday 23.
        Assert.Empty((await CalendarAsync(goal, Wed18, Sun22)).Periods);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(366)]
    public async Task US4_AS7_a_last_date_before_the_first_or_a_range_of_367_days_is_refused(int lastOffset)
    {
        var goal = await CreateAsync(YesNoTimes(1, SchedulePeriod.Day), Wed18);

        var result = await GetCalendarAsync(_user, goal, Wed18, Wed18.AddDays(lastOffset));

        Assert.Equal(GoalCalendarOutcome.RangeOutOfBounds, result.Outcome);
        Assert.Equal("Choose a range of 1 to 366 days.", result.Message);
        Assert.Null(result.Calendar);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(365, 366)]
    public async Task US4_AS7_ranges_of_1_and_366_days_are_read(int lastOffset, int days)
    {
        var goal = await CreateAsync(YesNoTimes(1, SchedulePeriod.Day), Wed18);

        // Wednesday 18 November 2026 plus 365 days is Thursday 18 November 2027 (2027 is no leap year).
        var calendar = await CalendarAsync(goal, Wed18, Wed18.AddDays(lastOffset));

        Assert.Equal(days, calendar.Periods.Count);
        Assert.Equal(Wed18.AddDays(lastOffset), calendar.Periods[^1].FirstDate);
    }
}
