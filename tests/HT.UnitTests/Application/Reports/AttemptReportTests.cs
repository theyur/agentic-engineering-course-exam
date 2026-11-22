using HT.Application.Records;
using HT.Application.Reports;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using HT.Domain.Reports;
using HT.UnitTests.Application.Goals;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Reports;

/// <summary>
/// Slice 8 (T054; FR-013, FR-018, FR-019, research R-07): <see cref="ReportService"/> over goals with attempts and
/// endings, on <see cref="AttemptWorld"/>. The user has no stored zone, so everything is in UTC: a day runs from 00:00Z
/// to 00:00Z, a week from Monday 00:00Z, and a grace period ends 48 hours after its entry. "Reading, 10 min daily"
/// starts on Monday 2 November 2026 and is ended at 2026-11-16T10:00:00Z: its last date is Sunday 15 November, whose
/// grace period ends at 2026-11-18T00:00:00Z, and Saturday 14's at 2026-11-17T00:00:00Z. Goals with one attempt keep
/// slice 7's results, which the existing report tests prove. Every instant is a UTC literal worked out by hand.
/// </summary>
public sealed class AttemptReportTests
{
    private static readonly DateOnly Mon2 = Date(2026, 11, 2);
    private static readonly DateOnly Sat14 = Date(2026, 11, 14);
    private static readonly DateOnly Sun15 = Date(2026, 11, 15);
    private static readonly DateOnly Tue17 = Date(2026, 11, 17);
    private static readonly DateOnly Wed18 = Date(2026, 11, 18);
    private static readonly DateOnly Fri20 = Date(2026, 11, 20);
    private static readonly DateOnly Sat21 = Date(2026, 11, 21);
    private static readonly DateOnly Mon23 = Date(2026, 11, 23);

    private static readonly DateTimeOffset Created = Utc("2026-11-01T10:00:00Z");
    private static readonly DateTimeOffset EndedAt = Utc("2026-11-16T10:00:00Z");
    private static readonly DateTimeOffset Restarted = Utc("2026-11-16T10:01:00Z");

    /// <summary>Tuesday 17 November, 10:00Z: 15 November is Pending, 14 November final, 17 November Due.</summary>
    private static readonly DateTimeOffset Tue17TenOClock = Utc("2026-11-17T10:00:00Z");

    private static readonly IReadOnlyList<HistoryEntry> NoEntries = [];
    private static readonly IReadOnlyList<RecordView> NoRecords = [];

    private readonly AttemptWorld _world = new();

    private static GoalTargetFields TenMinutesDaily => DurationDays(10, DurationUnit.Minutes, BuildOrReduce.Build, AllDays);

    private static GoalTargetFields AtMostOneSweetADay => CountTimes(1, "sweet", 1, SchedulePeriod.Day, BuildOrReduce.Reduce);

    /// <summary>"Reading, 10 min daily" from Monday 2 November, ended at <see cref="EndedAt"/>.</summary>
    private async Task<GoalId> EndedReadingAsync()
    {
        var goal = await _world.CreateGoalAsync(TenMinutesDaily, Mon2, Created);
        _world.At(EndedAt);
        await _world.EndAsync(goal);
        return goal;
    }

    private static void AssertSameHistory(GoalHistory expected, GoalHistory actual)
    {
        Assert.Equal(expected with { Entries = NoEntries }, actual with { Entries = NoEntries });
        Assert.Equal(expected.Entries.Count, actual.Entries.Count);
        foreach (var (wanted, got) in expected.Entries.Zip(actual.Entries))
        {
            Assert.Equal(wanted with { Records = NoRecords }, got with { Records = NoRecords });
            Assert.Equal(wanted.Records, got.Records);
        }
    }

    [Fact]
    public async Task A_report_after_a_restart_counts_only_the_latest_attempt_with_its_own_streak_unit()
    {
        var goal = await _world.CreateGoalAsync(TenMinutesDaily, Mon2, Created);
        for (var day = Mon2; day <= Sun15; day = day.AddDays(1))
        {
            _world.SeedAmount(goal, day, 10);
        }

        _world.At(EndedAt);
        await _world.EndAsync(goal);
        _world.At(Restarted);
        await _world.StartAttemptAsync(goal, YesNoTimes(3, SchedulePeriod.Week), Mon23);
        foreach (var day in new[] { Mon23, Mon23.AddDays(2), Mon23.AddDays(4) })
        {
            _world.SeedAnswer(goal, day, YesNoAnswer.Yes);
        }

        // Wednesday 2 December, 10:00Z: the week of 23 November was finalized at 00:00Z; the week of 30 November is Due.
        _world.At(Utc("2026-12-02T10:00:00Z"));
        var report = await _world.ReportAsync(goal);

        Assert.Equal("100% (1 of 1)", report.Stats.ConsistencyText);
        Assert.Equal((StreakUnit.Weeks, "1 week", "1 week"), (report.Stats.Unit, report.Stats.CurrentStreakText, report.Stats.LongestStreakText));
        Assert.Equal((GoalScheduleKind.PeriodBased, (SchedulePeriod?)SchedulePeriod.Week), (report.ScheduleKind, report.Period));
        Assert.Equal([Mon23], report.Recent.Results.Select(r => r.FirstDate));
        Assert.All(report.Trend.Bars, bar => Assert.True(bar.FirstDate >= Mon23));
    }

    [Fact]
    public async Task An_earlier_attempts_open_Pending_entry_is_in_the_overview_and_equals_what_can_be_recorded_now()
    {
        var restarted = await EndedReadingAsync();
        _world.At(Restarted);
        await _world.StartAttemptAsync(restarted, AtMostOneSweetADay, Wed18);
        var due = await _world.CreateNamedGoalAsync("Alpha", TenMinutesDaily, Tue17, Restarted);
        _world.At(Tue17TenOClock);

        var overview = await _world.OverviewAsync();

        // The restarted goal's attempt 2 has not begun; its only entry that can be recorded is attempt 1's 15 November.
        Assert.Equal([restarted, due], overview.Goals.Select(g => g.Item.Id));
        var item = overview.Goals[0];
        Assert.True(item.HasPending);
        Assert.True(item.CanRecord);
        Assert.Equal([(Sun15, EvaluationState.Pending, 1, false)], item.Recordable.Entries.Select(e => (e.FirstDate, e.State, e.AttemptNumber, e.IsLatestAttempt)));
        Assert.Equal("N/A", item.Stats.ConsistencyText);
        foreach (var goal in new[] { restarted, due })
        {
            var recordable = await _world.Recording.GetRecordableAsync(_world.User, goal, CancellationToken.None);
            AssertSameHistory(recordable.History!, overview.Goals.Single(g => g.Item.Id == goal).Recordable);
        }

        // At its grace end the entry is final and the goal leaves the Pending group.
        _world.At(Utc("2026-11-18T00:00:00Z"));
        var later = (await _world.OverviewAsync()).Goals.Single(g => g.Item.Id == restarted);
        Assert.False(later.HasPending);
        Assert.DoesNotContain(later.Recordable.Entries, e => !e.IsLatestAttempt);
    }

    [Fact]
    public async Task An_attempt_whose_last_entry_ended_more_than_48_hours_ago_is_not_evaluated_and_one_record_read_serves_the_goal()
    {
        var goal = await EndedReadingAsync();
        _world.At(Restarted);
        await _world.StartAttemptAsync(goal, TenMinutesDaily, Tue17);

        // Wednesday 18 November, 00:00Z: 15 November's grace period has just ended, so attempt 1 is not looked at.
        _world.At(Utc("2026-11-18T00:00:00Z"));
        var calls = _world.Records.Calls.Count;
        var overview = await _world.OverviewAsync();

        Assert.Equal([nameof(IRecordStore.ReadExemptionsAsync), nameof(IRecordStore.ReadAsync)], _world.Records.Calls.Skip(calls));
        Assert.All(overview.Goals[0].Recordable.Entries, e => Assert.Equal(2, e.AttemptNumber));

        // Before then, attempt 1's open entry comes from the same one read.
        var world = new AttemptWorld();
        var earlier = await world.CreateGoalAsync(TenMinutesDaily, Mon2, Created);
        world.At(EndedAt);
        await world.EndAsync(earlier);
        world.At(Restarted);
        await world.StartAttemptAsync(earlier, TenMinutesDaily, Tue17);
        world.At(Tue17TenOClock);
        var before = world.Records.Calls.Count;
        var report = await world.OverviewAsync();

        Assert.Equal([nameof(IRecordStore.ReadExemptionsAsync), nameof(IRecordStore.ReadAsync)], world.Records.Calls.Skip(before));
        Assert.Equal([(Tue17, 2), (Sun15, 1)], report.Goals[0].Recordable.Entries.Select(e => (e.FirstDate, e.AttemptNumber)));
    }

    [Fact]
    public async Task The_dashboard_keeps_Pending_entries_of_ended_attempts_and_leaves_out_everything_else_of_them()
    {
        // An ended goal with a note, a restarted goal (attempt 2 Reduce, from Tuesday 17) with a note, and a goal paused
        // from Tuesday 17 to Friday 20.
        var ended = await _world.CreateGoalAsync(TenMinutesDaily, Mon2, Created);
        var restarted = await _world.CreateGoalAsync(TenMinutesDaily, Mon2, Created);
        _world.SeedNote(ended, Sat14, "Ended goal's note.");
        _world.At(EndedAt);
        await _world.EndAsync(ended);
        await _world.EndAsync(restarted);
        _world.At(Restarted);
        await _world.StartAttemptAsync(restarted, AtMostOneSweetADay, Tue17);
        _world.SeedNote(restarted, Tue17, "Restarted goal's note.");
        var paused = await _world.CreateNamedGoalAsync("Paused goal", TenMinutesDaily, Tue17, Restarted);
        _world.Records.SeedPause(paused, Tue17, Fri20);
        _world.At(Tue17TenOClock);

        var dashboard = await _world.DashboardAsync();

        // Saturday 14's grace periods ended this week, but they are Missed entries of ended attempts.
        Assert.Equal((1, 2, 0, 1), (dashboard.Counts.Due, dashboard.Counts.Pending, dashboard.Counts.MissedThisWeek, dashboard.Counts.Goals));
        Assert.Equal(
            [
                (ended, Sun15, EvaluationState.Pending, true, true),
                (restarted, Sun15, EvaluationState.Pending, false, false),
                (restarted, Tue17, EvaluationState.Due, false, true),
                (paused, Sat21, EvaluationState.Upcoming, false, true),
            ],
            dashboard.Today.Select(t => (t.GoalId, t.Entry.FirstDate, t.Entry.State, t.GoalEnded, t.Entry.IsLatestAttempt)));

        var earlier = dashboard.Today[1];
        Assert.Equal((1, "Reading, 10 min daily", BuildOrReduce.Build, "Every day"), (earlier.Entry.AttemptNumber, earlier.GoalName, earlier.BuildOrReduce, earlier.ScheduleText));
        Assert.Equal(Measurement.Duration, earlier.Goal.SettingsOf(earlier.Entry).Measurement);
        Assert.Equal(BuildOrReduce.Reduce, dashboard.Today[2].BuildOrReduce);
        Assert.Equal(dashboard.Today[2].GoalName, dashboard.Goals.Single(g => g.GoalId == restarted).Name);

        Assert.Equal([restarted, paused], dashboard.Goals.Select(g => g.GoalId));
        Assert.Equal(Fri20, dashboard.Goals.Single(g => g.GoalId == paused).PausedUntil);
        Assert.Equal(["Restarted goal's note."], dashboard.Notes.Select(n => n.Note));
    }

    [Fact]
    public async Task An_ended_goal_in_the_overview_is_never_paused_and_has_no_next_commitment()
    {
        var goal = await _world.CreateGoalAsync(TenMinutesDaily, Mon2, Created);
        _world.At(Utc("2026-11-15T10:00:00Z"));
        _world.Records.SeedPause(goal, Date(2026, 11, 20), Date(2026, 11, 25));
        _world.At(EndedAt);
        await _world.EndAsync(goal, Outcome.Achieved);
        _world.At(Tue17TenOClock);

        var item = Assert.Single((await _world.OverviewAsync()).Goals);

        Assert.Null(item.PausedUntil);
        Assert.Equal((true, (DateOnly?)Date(2026, 11, 16), (Outcome?)Outcome.Achieved), (item.Item.IsEnded, item.Item.EndingDate, item.Item.Outcome));
        Assert.Null(item.Item.NextCommitment);
        Assert.Equal([Sun15], item.Recordable.Entries.Select(e => e.FirstDate));
        Assert.True(item.HasPending);
    }
}
