using HT.Application.Accounts;
using HT.Application.Exemptions;
using HT.Application.Records;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using static HT.UnitTests.Application.Exemptions.ExemptionWorld;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Exemptions;

/// <summary>
/// Recording on paused dates and what a pause keeps (FR-009, US2-AS9, AS10, Edge Cases "Excusing and pausing the same
/// date" and "Time zone changes"; T058) over the fakes of <see cref="ExemptionWorld"/>. Every goal starts on Monday 2
/// November 2026 and is created at 08:00Z that day, in UTC (the user has no stored time zone). "Today" is the goal's
/// current date (research R-03). Every instant is worked out by hand.
/// </summary>
public sealed class PauseRecordingTests
{
    private const string DatePaused = "This goal is paused on this date.";

    private static readonly DateOnly Mon2 = Date(2026, 11, 2);
    private static readonly DateOnly Wed11 = Date(2026, 11, 11);
    private static readonly DateOnly Mon16 = Date(2026, 11, 16);
    private static readonly DateOnly Tue17 = Date(2026, 11, 17);
    private static readonly DateOnly Wed18 = Date(2026, 11, 18);
    private static readonly DateOnly Thu19 = Date(2026, 11, 19);
    private static readonly DateOnly Fri20 = Date(2026, 11, 20);
    private static readonly DateOnly Sun22 = Date(2026, 11, 22);
    private static readonly DateOnly Mon23 = Date(2026, 11, 23);
    private static readonly DateOnly Wed25 = Date(2026, 11, 25);

    private static readonly DateTimeOffset Mon2Morning = Utc("2026-11-02T08:00:00Z");
    private static readonly DateTimeOffset Mon16NineOClock = Utc("2026-11-16T09:00:00Z");
    private static readonly DateTimeOffset Wed18EightOClock = Utc("2026-11-18T08:00:00Z");

    private readonly ExemptionWorld _world = new();

    /// <summary>"Drinking water, 8 glasses a day": Build, count at least 8 "glasses", once per day (period-based).</summary>
    private static GoalTargetFields DrinkingWater => CountTimes(8, "glasses", 1, SchedulePeriod.Day);

    /// <summary>"Yoga, 3 days a week": a yes/no Build goal on Monday, Wednesday and Friday (commitment-based).</summary>
    private static GoalTargetFields YogaOnMonWedFri => YesNoDays(BuildOrReduce.Build, MonWedFri);

    private Task<GoalId> GoalAsync(GoalTargetFields? target = null) => _world.CreateGoalAsync(target ?? DrinkingWater, Mon2, Mon2Morning);

    [Fact]
    public async Task US2_AS9_a_record_for_a_paused_date_is_refused_and_nothing_is_stored()
    {
        var goal = await GoalAsync();
        _world.At(Mon16NineOClock);
        Assert.Equal(PauseOutcome.Paused, (await _world.PauseAsync(goal, Mon16, Sun22, "Family visit")).Outcome);

        // 10:00Z on Tuesday 17 November: Tuesday 17 has begun and is in the pause.
        _world.At(Utc("2026-11-17T10:00:00Z"));
        var result = await _world.RecordAsync(goal, Tue17, Amount(8));

        Assert.Equal(RecordWriteOutcome.DatePaused, result.Outcome);
        Assert.Equal(DatePaused, result.Message);
        Assert.Empty(_world.Records.RecordsOfGoal(goal));
    }

    [Fact]
    public async Task US2_AS10_a_record_made_before_the_pause_is_kept_can_be_changed_and_deleted_and_the_day_stays_Paused()
    {
        var goal = await GoalAsync();
        _world.At(Utc("2026-11-16T08:00:00Z"));
        var recorded = await _world.RecordAsync(goal, Mon16, Amount(8));
        Assert.Equal(RecordWriteOutcome.Recorded, recorded.Outcome);
        _world.At(Mon16NineOClock);
        Assert.Equal(PauseOutcome.Paused, (await _world.PauseAsync(goal, Mon16, Sun22, "Family visit")).Outcome);

        var entry = await _world.EntryAsync(goal, Mon16);
        Assert.Equal(EvaluationState.Paused, entry.State);
        Assert.Equal(recorded.RecordId, Assert.Single(entry.Records).Id);

        _world.At(Utc("2026-11-16T10:00:00Z"));
        var changed = await _world.ChangeAsync(goal, recorded.RecordId!.Value, Amount(9));
        Assert.Equal(RecordWriteOutcome.Changed, changed.Outcome);
        Assert.Equal(EvaluationState.Paused, changed.Entry!.State);
        Assert.False(changed.IsCorrection);

        var deleted = await _world.DeleteAsync(goal, recorded.RecordId!.Value);
        Assert.Equal(RecordWriteOutcome.Deleted, deleted.Outcome);
        Assert.Equal(EvaluationState.Paused, deleted.Entry!.State);
        Assert.False(deleted.IsCorrection);

        var after = await _world.EntryAsync(goal, Mon16);
        Assert.Equal(EvaluationState.Paused, after.State);
        Assert.Empty(after.Records);
    }

    [Fact]
    public async Task Edge_an_excusal_kept_under_a_pause_shows_as_Excused_again_after_the_pause_is_cancelled()
    {
        // Friday 20 is excused, and an upcoming pause from Thursday 19 to Sunday 22 covers it.
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.SeedExcusal(goal, Fri20, "Ill");
        var pause = _world.SeedPause(goal, Thu19, Sun22);
        _world.At(Wed18EightOClock);
        Assert.Equal(EvaluationState.Paused, (await _world.EntryAsync(goal, Fri20)).State);

        Assert.Equal(CancelPauseOutcome.Cancelled, (await _world.CancelPauseAsync(goal, pause)).Outcome);

        var entry = await _world.EntryAsync(goal, Fri20);
        Assert.Equal(EvaluationState.Excused, entry.State);
        Assert.Equal("Ill", entry.ExcusalReason);
    }

    [Fact]
    public async Task Edge_an_excusal_kept_under_a_pause_shows_as_Excused_again_after_a_resume_that_ends_the_pause_before_it()
    {
        // Friday 20 is excused; a pause from Wednesday 18 to Sunday 22, set on Wednesday 18, covers it. On Friday 20 the
        // resume ends the pause on Thursday 19.
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.SeedExcusal(goal, Fri20);
        _world.At(Wed18EightOClock);
        Assert.Equal(PauseOutcome.Paused, (await _world.PauseAsync(goal, Wed18, Sun22)).Outcome);
        _world.At(Utc("2026-11-20T08:00:00Z"));
        Assert.Equal(EvaluationState.Paused, (await _world.EntryAsync(goal, Fri20)).State);

        Assert.Equal(ResumeOutcome.Resumed, (await _world.ResumeAsync(goal)).Outcome);

        Assert.Equal(EvaluationState.Excused, (await _world.EntryAsync(goal, Fri20)).State);
        Assert.Equal(EvaluationState.Paused, (await _world.EntryAsync(goal, Wed18)).State);
        Assert.NotNull(_world.Records.ExemptionsOfGoal(goal).ExcusalOf(Fri20));
    }

    [Fact]
    public async Task Edge_a_time_zone_change_moves_no_excusal_and_no_pause()
    {
        // Wednesday 11 is excused on Thursday 12 (Pending); Wednesday 25 is excused ahead (seeded); the goal is paused
        // from Monday 16 to Sunday 22 on Monday 16. Then, at 10:00Z on Monday 16, the account moves from UTC to Kyiv.
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.At(Utc("2026-11-12T10:00:00Z"));
        Assert.Equal(ExcuseOutcome.Excused, (await _world.ExcuseAsync(goal, Wed11, "Ill")).Outcome);
        _world.SeedExcusal(goal, Wed25);
        _world.At(Mon16NineOClock);
        Assert.Equal(PauseOutcome.Paused, (await _world.PauseAsync(goal, Mon16, Sun22)).Outcome);
        _world.At(Utc("2026-11-16T10:00:00Z"));

        var change = await _world.Profiles.ChangeTimeZoneAsync(_world.User, "Europe/Kyiv", CancellationToken.None);
        Assert.Equal(TimeZoneChangeOutcome.Changed, change.Outcome);

        // 12:00Z on Thursday 26 November.
        _world.At(Utc("2026-11-26T12:00:00Z"));
        var wed11 = await _world.EntryAsync(goal, Wed11);
        Assert.Equal(EvaluationState.Excused, wed11.State);
        Assert.Equal("Ill", wed11.ExcusalReason);

        // Wednesday 25 is now a Kyiv day, from 2026-11-24T22:00:00Z, and keeps its excusal.
        var wed25 = await _world.EntryAsync(goal, Wed25);
        Assert.Equal(Utc("2026-11-24T22:00:00Z"), wed25.Starts);
        Assert.Equal(EvaluationState.Excused, wed25.State);

        // The pause still covers Monday 16 to Sunday 22 of the goal's own timeline, and nothing after it.
        Assert.Equal(EvaluationState.Paused, (await _world.EntryAsync(goal, Mon16)).State);
        Assert.Equal(EvaluationState.Paused, (await _world.EntryAsync(goal, Wed18)).State);
        Assert.Equal(EvaluationState.Paused, (await _world.EntryAsync(goal, Fri20)).State);
        Assert.NotEqual(EvaluationState.Paused, (await _world.EntryAsync(goal, Mon23)).State);
        var pause = Assert.Single(_world.Records.ExemptionsOfGoal(goal).Pauses);
        Assert.Equal(Mon16, pause.FirstDay);
        Assert.Equal(Sun22, pause.LastDay);
    }
}
