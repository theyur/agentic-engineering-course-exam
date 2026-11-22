using HT.Application.Exemptions;
using HT.Application.Goals;
using HT.Application.Records;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using static HT.UnitTests.Application.Records.RecordWorld;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Goals;

/// <summary>
/// Slice 8 (T074; US2-AS2 to AS5, FR-010, FR-025): the reads and writes of a goal ended through
/// <see cref="GoalChangeService.EndAsync"/>, on <see cref="AttemptWorld"/>. The user has no stored zone, so everything is
/// in UTC. "Reading, 10 min daily" (Build, 10 minutes, all seven days) starts on Monday 2 November 2026, has 10 minutes on
/// each date from 2 to 13 November and nothing later, and is ended at 2026-11-16T10:00:00Z with "Abandoned": ending date
/// Monday 16, last date Sunday 15 November. 14 November's grace period ends at 2026-11-17T00:00:00Z and 15 November's at
/// 2026-11-18T00:00:00Z, so at 2026-11-16T10:01:00Z both are Pending and at 2026-11-18T12:00:00Z both are Missed. Every
/// change shows on the next read (FR-025). Every instant is a UTC literal worked out by hand.
/// </summary>
public sealed class EndedGoalReadTests
{
    private static readonly CancellationToken None = CancellationToken.None;

    private static readonly DateOnly Mon2 = Date(2026, 11, 2);
    private static readonly DateOnly Fri13 = Date(2026, 11, 13);
    private static readonly DateOnly Sat14 = Date(2026, 11, 14);
    private static readonly DateOnly Sun15 = Date(2026, 11, 15);
    private static readonly DateOnly Mon16 = Date(2026, 11, 16);
    private static readonly DateOnly Wed18 = Date(2026, 11, 18);

    private static readonly DateTimeOffset Created = Utc("2026-11-01T10:00:00Z");
    private static readonly DateTimeOffset EndedAt = Utc("2026-11-16T10:00:00Z");
    private static readonly DateTimeOffset JustAfter = Utc("2026-11-16T10:01:00Z");
    private static readonly DateTimeOffset Wednesday = Utc("2026-11-18T12:00:00Z");

    private readonly AttemptWorld _world = new();

    /// <summary>The goal of the class summary, ended through the use case.</summary>
    private async Task<GoalId> EndedMeditationAsync()
    {
        var goal = await _world.CreateGoalAsync(DurationDays(10, DurationUnit.Minutes, BuildOrReduce.Build, AllDays), Mon2, Created);
        foreach (var day in Enumerable.Range(0, 12))
        {
            _world.SeedAmount(goal, Mon2.AddDays(day), 10);
        }

        _world.At(EndedAt);
        var ended = await _world.Changes.EndAsync(_world.User, goal, Outcome.Abandoned, "Too busy this month.", None);
        Assert.Equal((GoalEndOutcome.Ended, (DateOnly?)Mon16, (DateOnly?)Sun15), (ended.Outcome, ended.EndingDate, ended.LastDate));
        return goal;
    }

    [Fact]
    public async Task US2_AS2_what_can_be_recorded_now_gives_the_Pending_14_and_15_November_and_not_16_November()
    {
        var goal = await EndedMeditationAsync();
        _world.At(JustAfter);

        var recordable = await _world.RecordableAsync(goal);

        Assert.Equal([(Sun15, EvaluationState.Pending), (Sat14, EvaluationState.Pending)], recordable.Entries.Select(e => (e.FirstDate, e.State)));
    }

    [Fact]
    public async Task US2_AS3_the_report_after_both_grace_periods_has_14_and_15_November_Missed_and_nothing_from_16_November()
    {
        var goal = await EndedMeditationAsync();
        _world.At(Wednesday);

        var report = await _world.ReportAsync(goal);

        Assert.Equal(("85% (12 of 14)", "0 commitments", "12 commitments"), (report.Stats.ConsistencyText, report.Stats.CurrentStreakText, report.Stats.LongestStreakText));
        var history = await _world.HistoryAsync(goal, Mon2, Wed18);
        Assert.Equal(Enumerable.Range(0, 14).Select(day => Mon2.AddDays(day)), history.Entries.Select(e => e.FirstDate));
        Assert.All(history.Entries.Take(12), e => Assert.Equal(EvaluationState.Met, e.State));
        Assert.Equal([EvaluationState.Missed, EvaluationState.Missed], history.Entries.Skip(12).Select(e => e.State));
        Assert.Empty((await _world.RecordableAsync(goal)).Entries);
    }

    [Fact]
    public async Task US2_AS4_recording_14_November_after_the_end_makes_it_Met_and_the_consistency_92_percent()
    {
        var goal = await EndedMeditationAsync();
        _world.At(Wednesday);

        var result = await _world.RecordAsync(goal, Sat14, Minutes(10));

        Assert.Equal(RecordWriteOutcome.Recorded, result.Outcome);
        Assert.Equal("92% (13 of 14)", (await _world.ReportAsync(goal)).Stats.ConsistencyText);
        var history = await _world.HistoryAsync(goal, Sat14, Sat14);
        Assert.Equal(EvaluationState.Met, Assert.Single(history.Entries).State);
    }

    [Fact]
    public async Task US2_AS5_adding_a_record_for_16_November_gives_Nothing_is_scheduled_on_this_date()
    {
        var goal = await EndedMeditationAsync();
        _world.At(Wednesday);

        var result = await _world.RecordAsync(goal, Mon16, Minutes(10));

        Assert.Equal((RecordWriteOutcome.NothingScheduled, "Nothing is scheduled on this date."), (result.Outcome, result.Message));
        Assert.DoesNotContain(_world.Records.RecordsOfGoal(goal), r => r.Date == Mon16);
    }

    [Fact]
    public async Task FR_010_correcting_a_record_of_the_ended_attempt_still_works()
    {
        var goal = await EndedMeditationAsync();
        _world.At(Wednesday);
        var record = _world.Records.RecordsOfGoal(goal).Single(r => r.Date == Fri13);

        var result = await _world.Recording.ChangeRecordAsync(_world.User, goal, record.Id, Minutes(5), None);

        Assert.Equal(RecordWriteOutcome.Changed, result.Outcome);
        Assert.Equal("78% (11 of 14)", (await _world.ReportAsync(goal)).Stats.ConsistencyText);
    }

    [Fact]
    public async Task FR_010_excusing_a_Pending_commitment_of_the_ended_attempt_and_removing_the_excusal_still_work()
    {
        var goal = await EndedMeditationAsync();
        _world.At(JustAfter);

        var excused = await _world.Excusals.ExcuseAsync(_world.User, goal, Sun15, null, None);

        Assert.Equal(ExcuseOutcome.Excused, excused.Outcome);
        Assert.Equal([Sun15], _world.Records.ExemptionsOfGoal(goal).Excusals.Select(e => e.Date));
        Assert.Equal(EvaluationState.Excused, (await _world.HistoryAsync(goal, Sun15, Sun15)).Entries.Single().State);

        var removed = await _world.Excusals.RemoveExcusalAsync(_world.User, goal, Sun15, None);

        Assert.Equal(RemoveExcusalOutcome.Removed, removed.Outcome);
        Assert.Empty(_world.Records.ExemptionsOfGoal(goal).Excusals);
        Assert.Equal(EvaluationState.Pending, (await _world.HistoryAsync(goal, Sun15, Sun15)).Entries.Single().State);
    }
}
