using HT.Application.Goals;
using HT.Domain.Goals;
using HT.Domain.Habits;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Goals;

/// <summary>
/// Slice 8 (T055; FR-004, FR-005, FR-017, FR-018, AC-035, AC-096 to AC-098): <see cref="GoalService"/>'s reads of goals
/// with settings changes, attempts and endings, on <see cref="AttemptWorld"/>. The user has no stored zone, so
/// everything is in UTC. "Reading, 15 min daily" starts on Monday 2 November 2026; a change at 2026-11-11T10:00:00Z
/// applies from Thursday 12 November, so the goal is named "Reading, 15 min daily" at 2026-11-11T23:59:59Z and "Reading,
/// 20 min daily" at 2026-11-12T00:00:00Z (AC-001, AC-002). Every instant is a UTC literal worked out by hand.
/// </summary>
public sealed class GoalAttemptReadTests
{
    private static readonly DateOnly Mon2 = Date(2026, 11, 2);
    private static readonly DateOnly Mon9 = Date(2026, 11, 9);
    private static readonly DateOnly Tue10 = Date(2026, 11, 10);
    private static readonly DateOnly Wed11 = Date(2026, 11, 11);
    private static readonly DateOnly Thu12 = Date(2026, 11, 12);
    private static readonly DateOnly Fri13 = Date(2026, 11, 13);
    private static readonly DateOnly Sun15 = Date(2026, 11, 15);
    private static readonly DateOnly Mon16 = Date(2026, 11, 16);
    private static readonly DateOnly Tue17 = Date(2026, 11, 17);
    private static readonly DateOnly Wed18 = Date(2026, 11, 18);
    private static readonly DateOnly Sun22 = Date(2026, 11, 22);
    private static readonly DateOnly Mon23 = Date(2026, 11, 23);
    private static readonly DateOnly Sun29 = Date(2026, 11, 29);

    private static readonly DateTimeOffset Created = Utc("2026-11-01T10:00:00Z");
    private static readonly DateTimeOffset ChangedAt = Utc("2026-11-11T10:00:00Z");

    private readonly AttemptWorld _world = new();

    private static GoalTargetFields MinutesDaily(int minutes) => DurationDays(minutes, DurationUnit.Minutes, BuildOrReduce.Build, AllDays);

    [Fact]
    public async Task US1_AS1_AS2_GetAsync_gives_the_settings_in_effect_and_the_full_later_settings_and_names_at_the_instant()
    {
        var goal = await _world.CreateGoalAsync(MinutesDaily(15), Mon2, Created);
        _world.At(ChangedAt);
        Assert.Equal(Thu12, await _world.ChangeSettingsAsync(goal, MinutesDaily(20)));

        _world.At(Utc("2026-11-11T23:59:59Z"));
        var before = await _world.DetailsAsync(goal);

        Assert.Equal(("Reading, 15 min daily", "15 min daily", (int?)15), (before.Name, before.TargetText, before.Amount));
        Assert.Equal((Mon2, Mon2), (before.TimeZoneAppliesFrom, before.StartDate));
        var later = Assert.IsType<GoalSettingsView>(before.Later);
        Assert.Equal((Thu12, "20 min daily", (int?)20, Measurement.Duration, BuildOrReduce.Build), (later.AppliesFrom, later.TargetText, later.Amount, later.Measurement, later.BuildOrReduce));
        Assert.Equal((ScheduleKind.FixedDays, "Every day", before.TimeZone), (later.ScheduleKind, later.ScheduleText, later.TimeZone));
        Assert.Equal(7, later.Days.Count);
        Assert.Equal((later.TimeZone, (DateOnly?)Thu12), (before.LaterTimeZone, before.LaterTimeZoneAppliesFrom));
        Assert.Equal(Thu12, before.ChangesApplyFrom);
        Assert.Equal((1, Mon2, true, true, false), (before.AttemptNumber, before.AttemptStartDate, before.AttemptHasBegun, before.HasStarted, before.IsEnded));

        _world.At(Utc("2026-11-12T00:00:00Z"));
        var after = await _world.DetailsAsync(goal);

        Assert.Equal(("Reading, 20 min daily", "20 min daily", Thu12), (after.Name, after.TargetText, after.TimeZoneAppliesFrom));
        Assert.Null(after.Later);
        Assert.Null(after.LaterTimeZone);
        Assert.Equal(Fri13, after.ChangesApplyFrom);
    }

    [Fact]
    public async Task An_ended_goal_gives_its_ending_fields_and_its_last_version_with_no_later_settings()
    {
        var goal = await _world.CreateGoalAsync(MinutesDaily(15), Mon2, Created);
        var other = await _world.CreateGoalAsync(MinutesDaily(15), Mon2, Created);
        _world.At(ChangedAt);
        await _world.ChangeSettingsAsync(goal, MinutesDaily(20));
        await _world.ChangeSettingsAsync(other, MinutesDaily(20));

        // Ended at 11:00Z the same day: the ending date is Wednesday 11, the last date Tuesday 10, and the version from
        // Thursday 12 is not kept. The other goal is ended on Monday 16, after its change took effect.
        _world.At(Utc("2026-11-11T11:00:00Z"));
        Assert.Equal((Wed11, Tue10), await _world.EndAsync(goal, Outcome.NoLongerRelevant, "  Moved to another plan.  "));
        _world.At(Utc("2026-11-16T10:00:00Z"));
        await _world.EndAsync(other, Outcome.Achieved);
        _world.At(Utc("2026-11-20T10:00:00Z"));

        var details = await _world.DetailsAsync(goal);

        Assert.Equal((true, (DateOnly?)Wed11, (DateOnly?)Tue10, (Outcome?)Outcome.NoLongerRelevant, "Moved to another plan."), (details.IsEnded, details.EndingDate, details.LastDate, details.Outcome, details.EndingNote));
        Assert.Equal(("Reading, 15 min daily", "15 min daily"), (details.Name, details.TargetText));
        Assert.Null(details.Later);
        Assert.Null(details.LaterTimeZone);
        Assert.Null(details.ChangesApplyFrom);
        Assert.Null(details.FirstWeekStarts);
        Assert.True(details.AttemptHasBegun);

        var endedLater = await _world.DetailsAsync(other);
        Assert.Equal(("Reading, 20 min daily", (string?)null, (DateOnly?)Sun15), (endedLater.Name, endedLater.EndingNote, endedLater.LastDate));
    }

    [Fact]
    public async Task A_waiting_attempt_gives_its_first_version_number_start_date_and_change_date_and_one_ended_before_it_began_has_not_begun()
    {
        var goal = await _world.CreateGoalAsync(MinutesDaily(15), Mon2, Created);
        _world.At(Utc("2026-11-16T10:00:00Z"));
        await _world.EndAsync(goal);
        _world.At(Utc("2026-11-16T10:01:00Z"));
        await _world.StartAttemptAsync(goal, YesNoTimes(3, SchedulePeriod.Week), Wed18);
        _world.At(Utc("2026-11-17T10:00:00Z"));

        var waiting = await _world.DetailsAsync(goal);

        Assert.Equal((2, Wed18, false, true, Mon2), (waiting.AttemptNumber, waiting.AttemptStartDate, waiting.AttemptHasBegun, waiting.HasStarted, waiting.StartDate));
        Assert.Equal(("Reading, 3 times a week", GoalScheduleKind.PeriodBased, Wed18), (waiting.Name, waiting.GoalScheduleKind, waiting.TimeZoneAppliesFrom));
        Assert.Equal(Wed18, waiting.ChangesApplyFrom);
        Assert.Equal(Mon23, waiting.FirstWeekStarts);
        Assert.False(waiting.IsEnded);
        Assert.Null(waiting.LastDate);

        _world.At(Utc("2026-11-18T10:00:00Z"));
        Assert.True((await _world.DetailsAsync(goal)).AttemptHasBegun);

        // Ended on Wednesday 18 before its first full week: its last date is Sunday 15 November, so it has no date.
        Assert.Equal((Wed18, Sun15), await _world.EndAsync(goal));
        var noDate = await _world.DetailsAsync(goal);
        Assert.Equal((true, false, (DateOnly?)Sun15, (DateOnly?)null), (noDate.IsEnded, noDate.AttemptHasBegun, noDate.LastDate, noDate.ChangesApplyFrom));
        Assert.Equal("Reading, 3 times a week", noDate.Name);
    }

    [Fact]
    public async Task AC_035_ListAsync_gives_the_waiting_attempts_start_date_and_the_ending_fields()
    {
        // Attempt 1 from Monday 23 November, ended on Tuesday 17 before it began; attempt 2 from Wednesday 18.
        var goal = await _world.CreateGoalAsync(MinutesDaily(15), Mon23, Utc("2026-11-16T10:00:00Z"));
        _world.At(Utc("2026-11-17T09:00:00Z"));
        Assert.Equal((Tue17, Mon16), await _world.EndAsync(goal));
        var listedEnded = Assert.Single(await _world.ListAsync());
        Assert.Equal((true, (DateOnly?)Tue17, (Outcome?)Outcome.Abandoned), (listedEnded.IsEnded, listedEnded.EndingDate, listedEnded.Outcome));
        Assert.Null(listedEnded.StartDate);
        Assert.Null(listedEnded.NextCommitment);
        Assert.Null(listedEnded.FirstWeekStarts);

        _world.At(Utc("2026-11-17T09:01:00Z"));
        await _world.StartAttemptAsync(goal, MinutesDaily(15), Wed18);
        _world.At(Utc("2026-11-17T12:00:00Z"));
        var waiting = Assert.Single(await _world.ListAsync());
        Assert.Equal(((DateOnly?)Wed18, false, (DateOnly?)null), (waiting.StartDate, waiting.IsEnded, waiting.EndingDate));
        Assert.Equal(Wed18, waiting.NextCommitment);

        _world.At(Utc("2026-11-18T00:00:00Z"));
        Assert.Null(Assert.Single(await _world.ListAsync()).StartDate);
    }

    [Fact]
    public async Task ListAsync_names_and_orders_the_goals_at_the_instant()
    {
        var first = await _world.CreateGoalAsync(MinutesDaily(15), Mon2, Created);
        var second = await _world.CreateGoalAsync(MinutesDaily(20), Mon2, Created);
        _world.At(ChangedAt);
        await _world.ChangeSettingsAsync(first, MinutesDaily(25));

        _world.At(Utc("2026-11-11T23:59:59Z"));
        var before = await _world.ListAsync();
        Assert.Equal([(first, "Reading, 15 min daily"), (second, "Reading, 20 min daily")], before.Select(g => (g.Id, g.Name)));
        Assert.Equal("15 min daily", before[0].TargetText);

        _world.At(Utc("2026-11-12T00:00:00Z"));
        var after = await _world.ListAsync();
        Assert.Equal([(second, "Reading, 20 min daily"), (first, "Reading, 25 min daily")], after.Select(g => (g.Id, g.Name)));
        Assert.Equal("25 min daily", after[1].TargetText);
    }

    [Fact]
    public async Task AC_096_to_AC_098_GetCalendarAsync_over_two_attempts_of_both_kinds()
    {
        var goal = await _world.CreateGoalAsync(MinutesDaily(15), Mon2, Created);
        _world.At(Utc("2026-11-16T10:00:00Z"));
        await _world.EndAsync(goal);
        _world.At(Utc("2026-11-16T10:01:00Z"));
        await _world.StartAttemptAsync(goal, YesNoTimes(3, SchedulePeriod.Week), Wed18);

        var both = await CalendarAsync(goal, Mon9, Sun29);
        Assert.Equal(GoalScheduleKind.PeriodBased, both.Kind);
        Assert.Equal(Enumerable.Range(0, 7).Select(i => Mon9.AddDays(i)), both.Commitments.Select(c => c.Date));
        Assert.Equal([(Mon23, Sun29)], both.Periods.Select(p => (p.FirstDate, p.LastDate)));

        var earlier = await CalendarAsync(goal, Mon9, Sun15);
        Assert.Equal((GoalScheduleKind.CommitmentBased, 7, 0), (earlier.Kind, earlier.Commitments.Count, earlier.Periods.Count));

        var between = await CalendarAsync(goal, Mon16, Sun22);
        Assert.Equal((GoalScheduleKind.PeriodBased, 0, 0), (between.Kind, between.Commitments.Count, between.Periods.Count));
    }

    private async Task<GoalCalendarView> CalendarAsync(GoalId goal, DateOnly first, DateOnly last)
    {
        var result = await _world.GoalService.GetCalendarAsync(_world.User, goal, first, last, CancellationToken.None);
        Assert.Equal(GoalCalendarOutcome.Found, result.Outcome);
        return result.Calendar!;
    }
}
