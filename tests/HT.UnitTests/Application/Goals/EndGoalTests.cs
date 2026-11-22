using HT.Application.Exemptions;
using HT.Application.Goals;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Goals;

/// <summary>
/// Slice 8 (T072; US2-AS1, AS6 to AS10, AS12, FR-007, FR-009): <see cref="GoalChangeService.EndAsync"/> on
/// <see cref="AttemptWorld"/>. The user has no stored zone, so everything is in UTC. "Reading, 10 min daily" (Build, 10
/// minutes, all seven days; the world's catalog habit is "Reading") is created on Sunday 1 November at 10:00Z with the
/// start date Monday 2 November, has 10 minutes on each date from 2 to 13 November, and is ended on Monday 16 November at
/// 10:00Z, so its ending date is Monday 16 and its last date Sunday 15 November. Every instant is a UTC literal worked
/// out by hand.
/// </summary>
public sealed class EndGoalTests
{
    private const string Note = "Too busy this month.";
    private const string GoalEnded = "This goal has ended.";
    private const string AlreadyEnded = "This goal has already ended.";
    private const string OutcomeMissing = "Choose how the goal ended.";
    private const string NoteTooLong = "The note can have at most 500 characters.";

    private static readonly CancellationToken None = CancellationToken.None;

    private static readonly DateOnly Mon2 = Date(2026, 11, 2);
    private static readonly DateOnly Sat14 = Date(2026, 11, 14);
    private static readonly DateOnly Sun15 = Date(2026, 11, 15);
    private static readonly DateOnly Mon16 = Date(2026, 11, 16);
    private static readonly DateOnly Tue17 = Date(2026, 11, 17);
    private static readonly DateOnly Wed18 = Date(2026, 11, 18);
    private static readonly DateOnly Fri20 = Date(2026, 11, 20);
    private static readonly DateOnly Mon23 = Date(2026, 11, 23);
    private static readonly DateOnly Wed25 = Date(2026, 11, 25);

    private static readonly DateTimeOffset Created = Utc("2026-11-01T10:00:00Z");
    private static readonly DateTimeOffset EndedAt = Utc("2026-11-16T10:00:00Z");

    private readonly AttemptWorld _world = new();

    private static GoalTargetFields TenMinutesDaily => DurationDays(10, DurationUnit.Minutes, BuildOrReduce.Build, AllDays);

    private Task<GoalEndResult> EndAsync(GoalId goal, Outcome? outcome, string? note = null, ActingUser? user = null) =>
        _world.Changes.EndAsync(user ?? _world.User, goal, outcome, note, None);

    /// <summary>"Reading, 10 min daily" from Monday 2 November with 10 minutes on 2 to 13 November; the clock at <see cref="EndedAt"/>.</summary>
    private async Task<GoalId> ReadingAsync()
    {
        var goal = await _world.CreateGoalAsync(TenMinutesDaily, Mon2, Created);
        foreach (var day in Enumerable.Range(0, 12))
        {
            _world.SeedAmount(goal, Mon2.AddDays(day), 10);
        }

        _world.At(EndedAt);
        return goal;
    }

    private static void AssertRefused(GoalEndResult result, GoalEndOutcome outcome, string message)
    {
        Assert.Equal((outcome, message), (result.Outcome, result.Message));
        Assert.Null(result.EndingDate);
        Assert.Null(result.LastDate);
    }

    [Fact]
    public async Task US2_AS1_ending_gives_goal_ended_with_the_ending_date_and_the_last_date_and_stores_the_ending()
    {
        var goal = await ReadingAsync();

        var result = await EndAsync(goal, Outcome.Abandoned, "  " + Note + "  ");

        Assert.Equal((GoalEndOutcome.Ended, "goal ended", (DateOnly?)Mon16, (DateOnly?)Sun15), (result.Outcome, result.Message, result.EndingDate, result.LastDate));
        var stored = _world.GoalOf(goal);
        var ending = Assert.IsType<GoalEnding>(stored.Attempt.Ending);
        Assert.Equal((Mon16, Outcome.Abandoned, Note), (ending.Date, ending.Outcome, ending.Note!.Value.Value));
        Assert.True(stored.IsEnded);
        Assert.Equal(Sun15, GoalTimeline.LastDate(stored, 1));
        Assert.Equal(1, GoalTimeline.AttemptOf(stored, Sun15));
        Assert.Null(GoalTimeline.AttemptOf(stored, Mon16));
        Assert.Null(GoalTimeline.AttemptOf(stored, Tue17));

        var details = await _world.DetailsAsync(goal);
        Assert.Equal((true, (DateOnly?)Mon16, (DateOnly?)Sun15, (Outcome?)Outcome.Abandoned, Note), (details.IsEnded, details.EndingDate, details.LastDate, details.Outcome, details.EndingNote));
    }

    [Fact]
    public async Task US2_AS1_an_empty_note_stores_no_note()
    {
        var goal = await ReadingAsync();

        Assert.Equal(GoalEndOutcome.Ended, (await EndAsync(goal, Outcome.Achieved, "   ")).Outcome);

        Assert.Null(_world.GoalOf(goal).Attempt.Ending!.Note);
    }

    [Fact]
    public async Task US2_AS6_no_outcome_gives_Choose_how_the_goal_ended_and_nothing_changes()
    {
        var goal = await ReadingAsync();
        var writes = _world.Goals.ChangeWriteCount;

        AssertRefused(await EndAsync(goal, null, Note), GoalEndOutcome.OutcomeMissing, OutcomeMissing);
        AssertRefused(await EndAsync(goal, (Outcome)7, Note), GoalEndOutcome.OutcomeMissing, OutcomeMissing);

        Assert.Equal(writes, _world.Goals.ChangeWriteCount);
        Assert.False(_world.GoalOf(goal).IsEnded);
    }

    [Fact]
    public async Task FR_007_no_outcome_is_checked_before_the_note()
    {
        var goal = await ReadingAsync();

        AssertRefused(await EndAsync(goal, null, new string('n', 501)), GoalEndOutcome.OutcomeMissing, OutcomeMissing);
    }

    [Fact]
    public async Task US2_AS7_a_note_of_501_characters_is_refused_and_nothing_changes()
    {
        var goal = await ReadingAsync();
        var writes = _world.Goals.ChangeWriteCount;

        AssertRefused(await EndAsync(goal, Outcome.Abandoned, new string('n', 501)), GoalEndOutcome.NoteTooLong, NoteTooLong);

        Assert.Equal(writes, _world.Goals.ChangeWriteCount);
        Assert.False(_world.GoalOf(goal).IsEnded);
    }

    [Fact]
    public async Task US2_AS7_a_note_of_500_characters_after_trimming_is_accepted()
    {
        var goal = await ReadingAsync();
        var note = new string('n', 500);

        var result = await EndAsync(goal, Outcome.Abandoned, " " + note + " ");

        Assert.Equal(GoalEndOutcome.Ended, result.Outcome);
        Assert.Equal(note, _world.GoalOf(goal).Attempt.Ending!.Note!.Value.Value);
    }

    [Fact]
    public async Task US2_AS8_ending_again_gives_This_goal_has_already_ended_and_keeps_the_first_ending()
    {
        var goal = await ReadingAsync();
        Assert.Equal(GoalEndOutcome.Ended, (await EndAsync(goal, Outcome.Abandoned, Note)).Outcome);
        _world.At(Utc("2026-11-18T12:00:00Z"));
        var writes = _world.Goals.ChangeWriteCount;

        AssertRefused(await EndAsync(goal, Outcome.Achieved), GoalEndOutcome.AlreadyEnded, AlreadyEnded);

        Assert.Equal(writes, _world.Goals.ChangeWriteCount);
        var ending = _world.GoalOf(goal).Attempt.Ending!;
        Assert.Equal((Mon16, Outcome.Abandoned), (ending.Date, ending.Outcome));
    }

    [Fact]
    public async Task FR_007_an_ended_goal_is_checked_before_the_outcome_and_the_note()
    {
        var goal = await ReadingAsync();
        await EndAsync(goal, Outcome.Abandoned);

        AssertRefused(await EndAsync(goal, null, new string('n', 501)), GoalEndOutcome.AlreadyEnded, AlreadyEnded);
    }

    [Fact]
    public async Task US2_AS9_changing_settings_and_pausing_after_the_end_each_give_This_goal_has_ended_and_write_nothing()
    {
        var goal = await ReadingAsync();
        await EndAsync(goal, Outcome.Abandoned, Note);
        _world.At(Utc("2026-11-17T10:00:00Z"));
        var writes = _world.Goals.ChangeWriteCount;

        var change = await _world.Changes.ChangeSettingsAsync(
            _world.User, goal, new GoalSettingsInput { Target = DurationDays(20, DurationUnit.Minutes, BuildOrReduce.Build, AllDays) }, None);
        var pause = await _world.Pauses.PauseAsync(_world.User, goal, Tue17, Fri20, null, None);

        Assert.Equal((SettingsChangeOutcome.GoalEnded, GoalEnded), (change.Outcome, change.Message));
        Assert.Equal((PauseOutcome.GoalEnded, GoalEnded), (pause.Outcome, pause.Message));
        Assert.Equal(writes, _world.Goals.ChangeWriteCount);
        Assert.Single(_world.GoalOf(goal).Versions);
        Assert.Empty(_world.Records.ExemptionsOfGoal(goal).Pauses);
    }

    [Fact]
    public async Task US2_AS10_ending_cuts_the_current_pause_to_the_last_date_and_removes_the_upcoming_one()
    {
        var goal = await _world.CreateGoalAsync(TenMinutesDaily, Mon2, Created);
        _world.At(Utc("2026-11-14T10:00:00Z"));
        Assert.Equal(PauseOutcome.Paused, (await _world.Pauses.PauseAsync(_world.User, goal, Sat14, Fri20, null, None)).Outcome);
        Assert.Equal(PauseOutcome.Paused, (await _world.Pauses.PauseAsync(_world.User, goal, Mon23, Wed25, null, None)).Outcome);
        _world.At(EndedAt);

        Assert.Equal(GoalEndOutcome.Ended, (await EndAsync(goal, Outcome.Abandoned)).Outcome);

        var pause = Assert.Single(_world.Records.ExemptionsOfGoal(goal).Pauses);
        Assert.Equal((Sat14, Sun15), (pause.FirstDay, pause.LastDay));
    }

    [Fact]
    public async Task FR_009_ending_removes_the_excusal_of_the_commitment_in_progress_and_keeps_the_earlier_one()
    {
        var goal = await _world.CreateGoalAsync(TenMinutesDaily, Mon2, Created);
        _world.At(Utc("2026-11-16T09:00:00Z"));
        Assert.Equal(ExcuseOutcome.Excused, (await _world.Excusals.ExcuseAsync(_world.User, goal, Sun15, null, None)).Outcome);
        Assert.Equal(ExcuseOutcome.Excused, (await _world.Excusals.ExcuseAsync(_world.User, goal, Mon16, null, None)).Outcome);
        _world.At(EndedAt);

        Assert.Equal(GoalEndOutcome.Ended, (await EndAsync(goal, Outcome.Abandoned)).Outcome);

        Assert.Equal([Sun15], _world.Records.ExemptionsOfGoal(goal).Excusals.Select(e => e.Date));
    }

    [Fact]
    public async Task US2_AS12_ending_a_goal_that_has_not_started_uses_the_accounts_today_and_keeps_its_first_version_and_name()
    {
        var goal = await _world.CreateGoalAsync(TenMinutesDaily, Mon23, Utc("2026-11-16T10:00:00Z"));
        _world.At(Utc("2026-11-17T10:00:00Z"));

        var result = await EndAsync(goal, Outcome.NoLongerRelevant);

        Assert.Equal((GoalEndOutcome.Ended, (DateOnly?)Tue17, (DateOnly?)Mon16), (result.Outcome, result.EndingDate, result.LastDate));
        var stored = _world.GoalOf(goal);
        var version = Assert.Single(stored.Versions);
        Assert.Equal((Mon23, 10, Measurement.Duration, BuildOrReduce.Build, 7), (version.AppliesFrom, version.Settings.Target.Amount, version.Settings.Target.Measurement, version.Settings.Target.BuildOrReduce, version.Settings.Target.Schedule.Days.Count));
        var details = await _world.DetailsAsync(goal);
        Assert.Equal(("Reading, 10 min daily", true, false), (details.Name, details.IsEnded, details.AttemptHasBegun));
        _world.At(Utc("2026-11-30T12:00:00Z"));
        var report = await _world.ReportAsync(goal);
        Assert.Equal(("N/A", false), (report.Stats.ConsistencyText, report.HasBegunEntry));
    }

    [Fact]
    public async Task FR_007_ending_a_waiting_attempt_2_after_a_begun_attempt_1_ends_it_before_it_began_on_the_goals_current_date()
    {
        var goal = await ReadingAsync();
        await _world.EndAsync(goal, Outcome.Abandoned, Note);
        _world.At(Utc("2026-11-16T10:01:00Z"));
        await _world.StartAttemptAsync(goal, TenMinutesDaily, Mon23);
        _world.At(Utc("2026-11-18T10:00:00Z"));

        // The goal has started (attempt 1 began), so its current date is Wednesday 18 in attempt 1's zone.
        var result = await EndAsync(goal, Outcome.NoLongerRelevant);

        Assert.Equal((GoalEndOutcome.Ended, (DateOnly?)Wed18, (DateOnly?)Tue17), (result.Outcome, result.EndingDate, result.LastDate));
        var stored = _world.GoalOf(goal);
        Assert.Equal((2, true), (stored.Attempt.Number, stored.IsEnded));
        Assert.Equal([Mon23], stored.Versions.Select(v => v.AppliesFrom));
        Assert.Equal((Mon16, Outcome.Abandoned), (stored.Attempts[0].Attempt.Ending!.Date, stored.Attempts[0].Attempt.Ending!.Outcome));
        var details = await _world.DetailsAsync(goal);
        Assert.Equal((2, Mon23, false, true), (details.AttemptNumber, details.AttemptStartDate, details.AttemptHasBegun, details.IsEnded));
    }

    [Fact]
    public async Task FR_023_the_anonymous_visitor_an_administrator_and_another_user_are_refused_before_the_goal()
    {
        var goal = await ReadingAsync();
        var writes = _world.Goals.ChangeWriteCount;

        AssertRefused(await EndAsync(goal, Outcome.Abandoned, user: ActingUser.Anonymous), GoalEndOutcome.NotSignedIn, "You are not signed in.");
        AssertRefused(await EndAsync(goal, Outcome.Abandoned, user: _world.Administrator), GoalEndOutcome.NotForAdministrators, GoalTexts.NotForAdministrators);
        AssertRefused(await EndAsync(goal, Outcome.Abandoned, user: _world.OtherUser), GoalEndOutcome.NotFound, "This goal does not exist.");
        AssertRefused(await EndAsync(new GoalId(Guid.NewGuid()), Outcome.Abandoned), GoalEndOutcome.NotFound, "This goal does not exist.");

        Assert.Equal(writes, _world.Goals.ChangeWriteCount);
        Assert.False(_world.GoalOf(goal).IsEnded);
    }
}
