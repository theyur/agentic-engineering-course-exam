using HT.Application.Records;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.UnitTests.Application.Exemptions;
using static HT.UnitTests.Application.Exemptions.ExemptionWorld;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Records;

/// <summary>
/// The history of Excused and Paused entries (FR-017, FR-020, FR-004; T039) over the fakes of <see cref="ExemptionWorld"/>,
/// with excusals and pauses seeded on the fake store. The user has no stored time zone, so the goals are in UTC; the
/// goals start on Monday 9 November 2026 and are created at 08:00Z that day. Wednesday 11 November ends at
/// 2026-11-12T00:00:00Z and its grace period ends at 2026-11-14T00:00:00Z. Every instant is worked out by hand.
/// </summary>
public sealed class HistoryExemptionTests
{
    private static readonly DateOnly Mon9 = Date(2026, 11, 9);
    private static readonly DateOnly Wed11 = Date(2026, 11, 11);
    private static readonly DateOnly Thu12 = Date(2026, 11, 12);
    private static readonly DateOnly Fri13 = Date(2026, 11, 13);
    private static readonly DateOnly Sat14 = Date(2026, 11, 14);
    private static readonly DateOnly Sun15 = Date(2026, 11, 15);
    private static readonly DateOnly Mon16 = Date(2026, 11, 16);
    private static readonly DateOnly Sun22 = Date(2026, 11, 22);

    private static readonly DateTimeOffset Mon9Morning = Utc("2026-11-09T08:00:00Z");

    private readonly ExemptionWorld _world = new();

    /// <summary>"Yoga, 3 days a week": a yes/no Build goal on Monday, Wednesday and Friday (commitment-based).</summary>
    private static GoalTargetFields YogaOnMonWedFri => YesNoDays(BuildOrReduce.Build, MonWedFri);

    /// <summary>A duration Build goal on all seven days (commitment-based).</summary>
    private static GoalTargetFields Reading20MinDaily => DurationDays(20, DurationUnit.Minutes, BuildOrReduce.Build, AllDays);

    /// <summary>A yes/no Build goal of three times a week (period-based, per week).</summary>
    private static GoalTargetFields YogaThreeTimesAWeek => YesNoTimes(3, SchedulePeriod.Week);

    /// <summary>A count Reduce goal of at most 2 cups a day (period-based, per day).</summary>
    private static GoalTargetFields CoffeeAtMost2CupsADay => CountTimes(2, "cups", 1, SchedulePeriod.Day, BuildOrReduce.Reduce);

    private Task<GoalId> GoalAsync(GoalTargetFields target) => _world.CreateGoalAsync(target, Mon9, Mon9Morning);

    private static void AssertExempt(HistoryEntry entry, EvaluationState state)
    {
        Assert.Equal(state, entry.State);
        Assert.Null(entry.GraceEnds);
        Assert.False(entry.IsCorrection);
    }

    [Fact]
    public async Task FR_017_an_Excused_commitment_has_its_reason_no_grace_end_no_correction_and_can_be_removed_before_its_grace_end()
    {
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.SeedExcusal(goal, Wed11, "Ill");

        _world.At(Utc("2026-11-13T23:59:59Z"));
        var before = await _world.EntryAsync(goal, Wed11);

        AssertExempt(before, EvaluationState.Excused);
        Assert.Equal("Ill", before.ExcusalReason);
        Assert.True(before.CanRemoveExcusal);
        Assert.Empty(before.Records);
        Assert.Equal(new Progress(ProgressKind.RecordedValue, null, 1), before.Progress);
    }

    [Fact]
    public async Task FR_004_FR_017_an_Excused_commitment_can_no_longer_be_removed_at_its_grace_end_and_stays_Excused()
    {
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.SeedExcusal(goal, Wed11, "Ill");

        _world.At(Utc("2026-11-14T00:00:00Z"));
        var atGraceEnd = await _world.EntryAsync(goal, Wed11);
        _world.At(Utc("2027-11-14T00:00:00Z"));
        var aYearLater = await _world.EntryAsync(goal, Wed11);

        foreach (var entry in new[] { atGraceEnd, aYearLater })
        {
            AssertExempt(entry, EvaluationState.Excused);
            Assert.Equal("Ill", entry.ExcusalReason);
            Assert.False(entry.CanRemoveExcusal);
        }
    }

    [Fact]
    public async Task FR_017_an_Excused_commitment_without_a_reason_has_none()
    {
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.SeedExcusal(goal, Wed11);
        _world.At(Utc("2026-11-12T10:00:00Z"));

        var entry = await _world.EntryAsync(goal, Wed11);

        AssertExempt(entry, EvaluationState.Excused);
        Assert.Null(entry.ExcusalReason);
        Assert.True(entry.CanRemoveExcusal);
    }

    [Fact]
    public async Task FR_007_FR_017_an_excused_commitment_in_a_pause_is_Paused_with_no_excusal_reason_or_removal()
    {
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.SeedExcusal(goal, Wed11, "Ill");
        _world.SeedPause(goal, Wed11, Fri13);
        _world.At(Utc("2026-11-12T10:00:00Z"));

        var entry = await _world.EntryAsync(goal, Wed11);

        AssertExempt(entry, EvaluationState.Paused);
        Assert.Null(entry.ExcusalReason);
        Assert.False(entry.CanRemoveExcusal);
    }

    [Fact]
    public async Task FR_017_a_Paused_day_keeps_its_records_and_progress()
    {
        var goal = await GoalAsync(Reading20MinDaily);
        _world.At(Utc("2026-11-12T10:00:00Z"));
        Assert.Equal(RecordWriteOutcome.Recorded, (await _world.RecordAsync(goal, Thu12, Minutes(12))).Outcome);
        _world.SeedPause(goal, Thu12, Sat14);

        // Read long after Thursday 12's grace end (2026-11-15T00:00:00Z): still Paused, not final.
        _world.At(Utc("2026-11-20T10:00:00Z"));
        var entry = await _world.EntryAsync(goal, Thu12);

        AssertExempt(entry, EvaluationState.Paused);
        Assert.Equal(12, Assert.Single(entry.Records).Amount);
        Assert.Equal(new Progress(ProgressKind.RecordedValue, 12, 20), entry.Progress);
        Assert.Null(entry.ExcusalReason);
        Assert.False(entry.CanRemoveExcusal);
    }

    [Fact]
    public async Task FR_007_FR_017_a_week_with_one_paused_date_is_Paused_with_its_records_and_progress()
    {
        var goal = await GoalAsync(YogaThreeTimesAWeek);
        _world.At(Utc("2026-11-11T10:00:00Z"));
        Assert.Equal(RecordWriteOutcome.Recorded, (await _world.RecordAsync(goal, Mon9, Yes())).Outcome);
        Assert.Equal(RecordWriteOutcome.Recorded, (await _world.RecordAsync(goal, Wed11, Yes())).Outcome);
        _world.SeedPause(goal, Sun15, Mon16);

        // The week of Monday 9 ends at 2026-11-16T00:00:00Z and its grace period at 2026-11-18T00:00:00Z.
        _world.At(Utc("2026-11-25T10:00:00Z"));
        var history = await _world.HistoryAsync(goal, Mon9, Sun22);

        Assert.Equal([(Mon9, Sun15), (Mon16, Sun22)], history.Select(e => (e.FirstDate, e.LastDate)));
        var first = history[0];
        AssertExempt(first, EvaluationState.Paused);
        Assert.Equal([Mon9, Wed11], first.Records.Select(r => r.Date));
        Assert.Equal(new Progress(ProgressKind.RecordsReachingAmount, 2, 3), first.Progress);
        AssertExempt(history[1], EvaluationState.Paused);
    }

    [Fact]
    public async Task FR_003_an_excusal_never_exempts_a_period()
    {
        var goal = await GoalAsync(CoffeeAtMost2CupsADay);
        _world.SeedExcusal(goal, Wed11, "Ill");
        _world.At(Utc("2026-11-12T10:00:00Z"));

        var entry = await _world.EntryAsync(goal, Wed11);

        Assert.Equal(EvaluationState.Pending, entry.State);
        Assert.Equal(Utc("2026-11-14T00:00:00Z"), entry.GraceEnds);
        Assert.Null(entry.ExcusalReason);
        Assert.False(entry.CanRemoveExcusal);
    }

    [Fact]
    public async Task FR_020_a_note_on_an_Excused_and_on_a_Paused_entry_is_in_the_history_with_that_state()
    {
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.SeedExcusal(goal, Wed11, "Ill");
        _world.SeedPause(goal, Fri13, Fri13);
        _world.SeedNote(goal, Wed11, "Stayed in bed.");
        _world.SeedNote(goal, Fri13, "Travelling.");
        _world.At(Utc("2026-11-13T10:00:00Z"));

        var history = await _world.HistoryAsync(goal, Wed11, Fri13);

        Assert.Equal(
            [(Wed11, EvaluationState.Excused, "Stayed in bed."), (Fri13, EvaluationState.Paused, "Travelling.")],
            history.Select(e => (e.FirstDate, e.State, e.Note)));
    }

    [Fact]
    public async Task FR_020_saving_a_note_on_a_Paused_entry_gives_it_with_the_state_Paused()
    {
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.SeedPause(goal, Wed11, Fri13);
        _world.At(Utc("2026-11-11T10:00:00Z"));

        var result = await _world.SaveNoteAsync(goal, Wed11, "Travelling.");

        Assert.Equal(NoteSaveOutcome.Saved, result.Outcome);
        Assert.Equal(EvaluationState.Paused, result.Entry!.State);
        Assert.Equal("Travelling.", result.Entry.Note);
        Assert.False(result.Entry.IsCorrection);
    }

    [Fact]
    public async Task FR_023_entries_without_an_exemption_are_unchanged_beside_exempt_ones()
    {
        // On Friday 13 at 10:00Z: Monday 9 is Missed (grace end 2026-11-12T00:00:00Z), Wednesday 11 is Excused and
        // Friday 13 Due (grace end 2026-11-16T00:00:00Z).
        var goal = await GoalAsync(YogaOnMonWedFri);
        _world.SeedExcusal(goal, Wed11);
        _world.At(Utc("2026-11-13T10:00:00Z"));

        var history = await _world.HistoryAsync(goal, Mon9, Fri13);

        Assert.Equal(
            [(Mon9, EvaluationState.Missed), (Wed11, EvaluationState.Excused), (Fri13, EvaluationState.Due)],
            history.Select(e => (e.FirstDate, e.State)));
        Assert.True(history[0].IsCorrection);
        Assert.Null(history[0].GraceEnds);
        Assert.Equal(Utc("2026-11-16T00:00:00Z"), history[2].GraceEnds);
        Assert.False(history[2].IsCorrection);
        Assert.All([history[0], history[2]], e =>
        {
            Assert.Null(e.ExcusalReason);
            Assert.False(e.CanRemoveExcusal);
        });
    }
}
