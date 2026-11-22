using HT.Application.Accounts;
using HT.Application.Goals;
using HT.Application.Records;
using HT.Application.Reports;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Goals;

/// <summary>
/// Slice 8 (T085; SC-004, SC-005, US3-AS13): the boundaries of <see cref="GoalChangeService.GetNewAttemptValuesAsync"/>
/// and <see cref="GoalChangeService.StartAttemptAsync"/> on <see cref="AttemptWorld"/>: the ending date and the day after
/// it, the account's midnight on both sides, the 365th day ahead, the Kyiv ending of US3-AS13 whose ending date is after
/// the account's today, the independence of a new attempt's figures, and a per-week restart's first week. The user has no
/// stored zone unless a test says otherwise. "Reading, 10 min daily" starts on Monday 2 November 2026 with 10 minutes on 2
/// to 13 November and is ended on Monday 16 November at 10:00Z (ending date Monday 16, last date Sunday 15 November).
/// Every instant is a UTC literal worked out by hand.
/// </summary>
public sealed class StartAttemptBoundaryTests
{
    private const string KyivId = "Europe/Kyiv";
    private const string NotAfterEnding = "A new attempt starts after the day the goal ended.";

    private static readonly CancellationToken None = CancellationToken.None;

    private static readonly DateOnly Mon2 = Date(2026, 11, 2);
    private static readonly DateOnly Mon16 = Date(2026, 11, 16);
    private static readonly DateOnly Tue17 = Date(2026, 11, 17);
    private static readonly DateOnly Wed18 = Date(2026, 11, 18);
    private static readonly DateOnly Fri20 = Date(2026, 11, 20);
    private static readonly DateOnly Sun22 = Date(2026, 11, 22);
    private static readonly DateOnly Mon23 = Date(2026, 11, 23);

    private static readonly DateTimeOffset Created = Utc("2026-11-01T10:00:00Z");
    private static readonly DateTimeOffset EndedAt = Utc("2026-11-16T10:00:00Z");

    private readonly AttemptWorld _world = new();

    private static GoalTargetFields MinutesDaily(int minutes) => DurationDays(minutes, DurationUnit.Minutes, BuildOrReduce.Build, AllDays);

    private static NewAttemptInput Input(GoalTargetFields target, DateOnly? start) => new() { Target = target, StartDate = start };

    private Task<AttemptStartResult> StartAsync(GoalId goal, GoalTargetFields target, DateOnly start, ActingUser? user = null) =>
        _world.Changes.StartAttemptAsync(user ?? _world.User, goal, Input(target, start), None);

    private async Task<DateOnly> StartDateOfValuesAsync(GoalId goal, ActingUser? user = null)
    {
        var result = await _world.Changes.GetNewAttemptValuesAsync(user ?? _world.User, goal, None);
        Assert.Equal(NewAttemptValuesOutcome.Found, result.Outcome);
        return result.Values!.StartDate;
    }

    /// <summary>"Reading, 10 min daily" with 10 minutes on 2 to 13 November, ended at <see cref="EndedAt"/>; the clock stays there.</summary>
    private async Task<GoalId> EndedGoalAsync(GoalTargetFields? target = null)
    {
        var goal = await _world.CreateGoalAsync(target ?? MinutesDaily(10), Mon2, Created);
        if (target is null)
        {
            foreach (var day in Enumerable.Range(0, 12))
            {
                _world.SeedAmount(goal, Mon2.AddDays(day), 10);
            }
        }

        _world.At(EndedAt);
        await _world.EndAsync(goal);
        return goal;
    }

    private async Task<AttemptView> AttemptAsync(GoalId goal, int number)
    {
        var result = await _world.Attempts.GetAttemptsAsync(_world.User, goal, None);
        Assert.Equal(AttemptListOutcome.Found, result.Outcome);
        return result.Attempts.Single(attempt => attempt.Number == number);
    }

    [Fact]
    public async Task SC_004_a_restart_on_the_ending_date_is_refused_and_on_the_day_after_accepted()
    {
        var goal = await EndedGoalAsync();
        _world.At(Utc("2026-11-16T11:00:00Z"));

        var onEnding = await StartAsync(goal, MinutesDaily(10), Mon16);
        var after = await StartAsync(goal, MinutesDaily(10), Tue17);

        Assert.Equal((AttemptStartOutcome.StartDateNotAfterEnding, NotAfterEnding), (onEnding.Outcome, onEnding.Message));
        Assert.Equal((AttemptStartOutcome.Started, (DateOnly?)Tue17), (after.Outcome, after.StartDate));
    }

    [Fact]
    public async Task SC_005_the_starting_values_follow_the_accounts_midnight()
    {
        var goal = await EndedGoalAsync();

        _world.At(Utc("2026-11-16T23:59:59Z"));
        Assert.Equal(Tue17, await StartDateOfValuesAsync(goal));
        _world.At(Utc("2026-11-17T00:00:00Z"));
        Assert.Equal(Tue17, await StartDateOfValuesAsync(goal));
        _world.At(Utc("2026-11-17T23:59:59Z"));
        Assert.Equal(Tue17, await StartDateOfValuesAsync(goal));
        _world.At(Utc("2026-11-18T00:00:00Z"));
        Assert.Equal(Wed18, await StartDateOfValuesAsync(goal));
    }

    [Theory]
    [InlineData("2026-11-17T23:59:59Z", AttemptStartOutcome.Started)]
    [InlineData("2026-11-18T00:00:00Z", AttemptStartOutcome.StartDateInPast)]
    public async Task SC_005_a_start_date_of_today_is_accepted_until_the_accounts_midnight_and_in_the_past_from_it(string instant, AttemptStartOutcome expected)
    {
        var goal = await EndedGoalAsync();
        _world.At(Utc(instant));

        Assert.Equal(expected, (await StartAsync(goal, MinutesDaily(10), Tue17)).Outcome);
    }

    [Fact]
    public async Task FR_012_the_365th_day_after_the_accounts_today_is_accepted_and_the_366th_refused()
    {
        var goal = await EndedGoalAsync();
        _world.At(Utc("2026-11-18T08:00:00Z"));

        var tooFar = await StartAsync(goal, MinutesDaily(10), Date(2027, 11, 19));
        var lastDay = await StartAsync(goal, MinutesDaily(10), Date(2027, 11, 18));

        Assert.Equal((AttemptStartOutcome.StartDateTooFar, "Choose a start date within a year from today."), (tooFar.Outcome, tooFar.Message));
        Assert.Equal((AttemptStartOutcome.Started, (DateOnly?)Date(2027, 11, 18)), (lastDay.Outcome, lastDay.StartDate));
    }

    [Fact]
    public async Task US3_AS13_in_Kyiv_the_starting_values_are_the_day_after_the_ending_date_while_the_accounts_today_is_earlier()
    {
        var user = _world.UserIn(KyivId);
        var goal = await _world.CreateGoalAsync(MinutesDaily(10), Mon2, Created, user);
        // Monday 16 November at 23:30Z is Tuesday 17 at 01:30 in Kyiv.
        _world.At(Utc("2026-11-16T23:30:00Z"));
        Assert.Equal(TimeZoneChangeOutcome.Changed, (await _world.Profiles.ChangeTimeZoneAsync(user, "UTC", None)).Outcome);
        _world.At(Utc("2026-11-16T23:40:00Z"));
        var ended = await _world.Changes.EndAsync(user, goal, Outcome.Abandoned, null, None);
        Assert.Equal(((DateOnly?)Tue17, (DateOnly?)Mon16), (ended.EndingDate, ended.LastDate));
        var writes = _world.Goals.ChangeWriteCount;

        Assert.Equal(Wed18, await StartDateOfValuesAsync(goal, user));
        var refused = await StartAsync(goal, MinutesDaily(10), Tue17, user);

        Assert.Equal((AttemptStartOutcome.StartDateNotAfterEnding, NotAfterEnding, (GoalField?)GoalField.StartDate), (refused.Outcome, refused.Message, refused.Field));
        Assert.Equal(writes, _world.Goals.ChangeWriteCount);
        Assert.True(_world.GoalOf(goal).IsEnded);
        var started = await StartAsync(goal, MinutesDaily(10), Wed18, user);
        Assert.Equal((AttemptStartOutcome.Started, (DateOnly?)Wed18), (started.Outcome, started.StartDate));
        Assert.Equal("UTC", _world.GoalOf(goal).Versions[0].Settings.TimeZone.Id);
    }

    [Fact]
    public async Task SC_004_a_new_attempt_starts_at_NA_and_0_and_recording_in_it_never_changes_the_earlier_attempts_figures()
    {
        var goal = await EndedGoalAsync();
        _world.At(Utc("2026-11-18T08:00:00Z"));
        var before = (await AttemptAsync(goal, 1)).Stats;
        Assert.Equal(AttemptStartOutcome.Started, (await StartAsync(goal, MinutesDaily(10), Wed18)).Outcome);

        var fresh = (await AttemptAsync(goal, 2)).Stats;
        Assert.Equal(("N/A", "0 commitments", "0 commitments"), (fresh.ConsistencyText, fresh.CurrentStreakText, fresh.LongestStreakText));
        Assert.Equal(before, (await AttemptAsync(goal, 1)).Stats);

        for (var day = Wed18; day <= Fri20; day = day.AddDays(1))
        {
            _world.At(new DateTimeOffset(day.ToDateTime(new TimeOnly(20, 0)), TimeSpan.Zero));
            Assert.Equal(RecordWriteOutcome.Recorded, (await _world.RecordAsync(goal, day, new RecordInput { Amount = 10 })).Outcome);
        }

        _world.At(Utc("2026-11-23T12:00:00Z"));
        var second = (await AttemptAsync(goal, 2)).Stats;
        var first = (await AttemptAsync(goal, 1)).Stats;
        Assert.Equal(("100% (3 of 3)", "3 commitments", "3 commitments"), (second.ConsistencyText, second.CurrentStreakText, second.LongestStreakText));
        Assert.Equal(("85% (12 of 14)", "12 commitments"), (first.ConsistencyText, first.LongestStreakText));
        Assert.Equal(before, first);
        var report = await _world.ReportAsync(goal);
        Assert.Equal("100% (3 of 3)", report.Stats.ConsistencyText);
    }

    [Theory]
    [InlineData(18, 23)]
    [InlineData(23, 23)]
    [InlineData(24, 30)]
    public async Task FR_013_restarting_a_per_week_goal_starts_its_first_week_on_the_first_Monday_on_or_after_the_start_date(int startDay, int firstMonday)
    {
        var goal = await EndedGoalAsync(YesNoTimes(3, SchedulePeriod.Week));
        _world.At(Utc("2026-11-18T08:00:00Z"));
        var start = Date(2026, 11, startDay);

        Assert.Equal(AttemptStartOutcome.Started, (await StartAsync(goal, YesNoTimes(2, SchedulePeriod.Week), start)).Outcome);

        var details = await _world.DetailsAsync(goal);
        Assert.Equal(Date(2026, 11, firstMonday), details.FirstWeekStarts);
        _world.At(Utc("2026-12-15T10:00:00Z"));
        var history = (await _world.Attempts.GetAttemptAsync(_world.User, goal, 2, None)).Details!.History;
        Assert.Equal(Date(2026, 11, firstMonday), history.Entries[^1].FirstDate);
        Assert.Equal(Date(2026, 11, firstMonday).AddDays(6), history.Entries[^1].LastDate);
    }

    [Fact]
    public async Task FR_013_a_per_week_attempt_ended_on_a_Monday_and_restarted_that_week_keeps_the_weeks_apart()
    {
        // Ended on Monday 16 November: last date Sunday 15; a restart from Tuesday 17 has its first week from Monday 23,
        // so the week of 16 to 22 November belongs to no attempt.
        var goal = await EndedGoalAsync(YesNoTimes(3, SchedulePeriod.Week));
        _world.At(Utc("2026-11-17T08:00:00Z"));

        Assert.Equal(AttemptStartOutcome.Started, (await StartAsync(goal, YesNoTimes(3, SchedulePeriod.Week), Tue17)).Outcome);

        var stored = _world.GoalOf(goal);
        Assert.Equal(1, GoalTimeline.AttemptOf(stored, Date(2026, 11, 15)));
        Assert.Null(GoalTimeline.AttemptOf(stored, Mon16));
        Assert.Null(GoalTimeline.AttemptOf(stored, Sun22));
        Assert.Equal(2, GoalTimeline.AttemptOf(stored, Mon23));
    }
}
