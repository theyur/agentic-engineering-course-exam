using HT.Application.Goals;
using HT.Application.Records;
using HT.Application.Reports;
using HT.Domain.Accounts;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Reports;
using HT.UnitTests.Application.Goals;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Reports;

/// <summary>
/// Slice 8 (T086; US3-AS3, AS9 to AS12, FR-014, FR-016, FR-019, AC-102): <see cref="AttemptService"/> and the reads after a
/// restart, on <see cref="AttemptWorld"/>. The user has no stored zone, so everything is in UTC. The spec's "Meditation, 10
/// min daily" is "Reading, 10 min daily" here (the world's catalog habit is "Reading"): Build, 10 minutes, all seven days,
/// created on Sunday 1 November at 10:00Z with the start date Monday 2 November, 10 minutes on each date from 2 to 13
/// November, ended on Monday 16 November at 10:00Z with "Abandoned" and "Too busy this month." (ending date Monday 16,
/// last date Sunday 15 November). Attempt 2 of US3-AS2 starts on Wednesday 18 November with the same settings and has 10
/// minutes on 18, 19 and 20 November. Every instant is a UTC literal worked out by hand.
/// </summary>
public sealed class AttemptReadTests
{
    private const string Note = "Too busy this month.";
    private const string Reading10 = "Reading, 10 min daily";

    private static readonly CancellationToken None = CancellationToken.None;

    private static readonly DateOnly Mon2 = Date(2026, 11, 2);
    private static readonly DateOnly Fri13 = Date(2026, 11, 13);
    private static readonly DateOnly Sat14 = Date(2026, 11, 14);
    private static readonly DateOnly Sun15 = Date(2026, 11, 15);
    private static readonly DateOnly Mon16 = Date(2026, 11, 16);
    private static readonly DateOnly Tue17 = Date(2026, 11, 17);
    private static readonly DateOnly Wed18 = Date(2026, 11, 18);
    private static readonly DateOnly Fri20 = Date(2026, 11, 20);

    private static readonly DateTimeOffset Created = Utc("2026-11-01T10:00:00Z");
    private static readonly DateTimeOffset EndedAt = Utc("2026-11-16T10:00:00Z");

    /// <summary>Monday 23 November at 12:00Z.</summary>
    private static readonly DateTimeOffset Mon23At12 = Utc("2026-11-23T12:00:00Z");

    private readonly AttemptWorld _world = new();

    private static GoalTargetFields MinutesDaily(int minutes) => DurationDays(minutes, DurationUnit.Minutes, BuildOrReduce.Build, AllDays);

    private Task<AttemptStartResult> StartAsync(GoalId goal, GoalTargetFields target, DateOnly start, ActingUser? user = null) =>
        _world.Changes.StartAttemptAsync(user ?? _world.User, goal, new NewAttemptInput { Target = target, StartDate = start }, None);

    /// <summary>"Reading, 10 min daily" with 10 minutes on 2 to 13 November, ended at <see cref="EndedAt"/>; the clock stays there.</summary>
    private async Task<GoalId> EndedGoalAsync()
    {
        var goal = await _world.CreateGoalAsync(MinutesDaily(10), Mon2, Created);
        foreach (var day in Enumerable.Range(0, 12))
        {
            _world.SeedAmount(goal, Mon2.AddDays(day), 10);
        }

        _world.At(EndedAt);
        Assert.Equal(GoalEndOutcome.Ended, (await _world.Changes.EndAsync(_world.User, goal, Outcome.Abandoned, Note, None)).Outcome);
        return goal;
    }

    /// <summary>US3-AS2 and AS3's arrangement: attempt 2 from Wednesday 18 November with 10 minutes on 18 to 20 November; the clock at <see cref="Mon23At12"/>.</summary>
    private async Task<GoalId> RestartedGoalAsync()
    {
        var goal = await EndedGoalAsync();
        _world.At(Utc("2026-11-18T08:00:00Z"));
        Assert.Equal(AttemptStartOutcome.Started, (await StartAsync(goal, MinutesDaily(10), Wed18)).Outcome);
        for (var day = Wed18; day <= Fri20; day = day.AddDays(1))
        {
            _world.At(new DateTimeOffset(day.ToDateTime(new TimeOnly(20, 0)), TimeSpan.Zero));
            Assert.Equal(RecordWriteOutcome.Recorded, (await _world.RecordAsync(goal, day, new RecordInput { Amount = 10 })).Outcome);
        }

        _world.At(Mon23At12);
        return goal;
    }

    private async Task<IReadOnlyList<AttemptView>> AttemptsAsync(GoalId goal)
    {
        var result = await _world.Attempts.GetAttemptsAsync(_world.User, goal, None);
        Assert.Equal((AttemptListOutcome.Found, (string?)null), (result.Outcome, result.Message));
        return result.Attempts;
    }

    private async Task<AttemptDetails> AttemptAsync(GoalId goal, int number)
    {
        var result = await _world.Attempts.GetAttemptAsync(_world.User, goal, number, None);
        Assert.Equal((AttemptReadOutcome.Found, (string?)null), (result.Outcome, result.Message));
        return result.Details!;
    }

    [Fact]
    public async Task US3_AS3_attempt_2s_report_on_23_November_counts_only_its_own_three_results()
    {
        var goal = await RestartedGoalAsync();

        var report = await _world.ReportAsync(goal);
        var attempt = (await AttemptAsync(goal, 2)).Attempt.Stats;

        Assert.Equal(("100% (3 of 3)", "3 commitments", "3 commitments"), (report.Stats.ConsistencyText, report.Stats.CurrentStreakText, report.Stats.LongestStreakText));
        Assert.Equal(report.Stats, attempt);
        Assert.DoesNotContain(report.Recent.Results, result => result.FirstDate < Wed18);
    }

    [Fact]
    public async Task US3_AS9_the_attempts_come_latest_first_with_attempt_1s_ending_fields_and_figures()
    {
        var goal = await RestartedGoalAsync();

        var attempts = await AttemptsAsync(goal);

        Assert.Equal([2, 1], attempts.Select(attempt => attempt.Number));
        var second = attempts[0];
        Assert.Equal((Reading10, Wed18, true, (DateOnly?)null, (DateOnly?)null, (Outcome?)null, (string?)null, false),
            (second.Name, second.StartDate, second.IsLatest, second.EndingDate, second.LastDate, second.Outcome, second.EndingNote, second.HasNoDate));
        var first = attempts[1];
        Assert.Equal((Reading10, Mon2, false, (DateOnly?)Mon16, (DateOnly?)Sun15, (Outcome?)Outcome.Abandoned, (string?)Note, false),
            (first.Name, first.StartDate, first.IsLatest, first.EndingDate, first.LastDate, first.Outcome, first.EndingNote, first.HasNoDate));
        Assert.Equal(("85% (12 of 14)", "12 commitments", StreakUnit.Commitments), (first.Stats.ConsistencyText, first.Stats.LongestStreakText, first.Stats.Unit));
        Assert.Equal(new Consistency(12, 14), first.Stats.Consistency);
    }

    [Fact]
    public async Task US3_AS10_a_correction_of_15_November_changes_attempt_1s_figures_only()
    {
        var goal = await RestartedGoalAsync();
        var before = await AttemptsAsync(goal);

        Assert.Equal(RecordWriteOutcome.Recorded, (await _world.RecordAsync(goal, Sun15, new RecordInput { Amount = 10 })).Outcome);

        var after = await AttemptsAsync(goal);
        Assert.Equal("92% (13 of 14)", after[1].Stats.ConsistencyText);
        Assert.Equal(before[0], after[0]);
        Assert.Equal("100% (3 of 3)", (await _world.ReportAsync(goal)).Stats.ConsistencyText);
    }

    [Fact]
    public async Task US3_AS11_attempt_1s_History_runs_from_15_back_to_2_November_and_its_recent_results_oldest_first()
    {
        var goal = await RestartedGoalAsync();

        var details = await AttemptAsync(goal, 1);

        var history = details.History;
        Assert.Equal(Enumerable.Range(0, 14).Select(day => Sun15.AddDays(-day)), history.Entries.Select(entry => entry.FirstDate));
        Assert.All(history.Entries.Where(entry => entry.FirstDate <= Fri13), entry => Assert.Equal(EvaluationState.Met, entry.State));
        Assert.Equal([EvaluationState.Missed, EvaluationState.Missed], history.Entries.Take(2).Select(entry => entry.State));
        Assert.All(history.Entries, entry => Assert.Equal((1, false), (entry.AttemptNumber, entry.IsLatestAttempt)));
        Assert.Equal((BuildOrReduce.Build, (int?)10, (DateOnly?)Date(2026, 11, 23)), (history.BuildOrReduce, history.Amount, history.CurrentDate));
        Assert.Equal(Enumerable.Range(0, 14).Select(day => Mon2.AddDays(day)), details.Recent.Results.Select(result => result.FirstDate));
        Assert.Equal((12, 2), (details.Recent.Counts[EvaluationState.Met], details.Recent.Counts[EvaluationState.Missed]));
        Assert.Equal(1, details.Attempt.Number);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task US3_AS11_a_number_that_is_not_one_of_the_goals_attempts_gives_This_attempt_does_not_exist(int number)
    {
        var goal = await RestartedGoalAsync();

        var result = await _world.Attempts.GetAttemptAsync(_world.User, goal, number, None);

        Assert.Equal((AttemptReadOutcome.AttemptNotFound, "This attempt does not exist."), (result.Outcome, result.Message));
        Assert.Null(result.Details);
    }

    [Fact]
    public async Task FR_014_the_latest_attempts_History_runs_to_the_current_date_newest_first()
    {
        var goal = await RestartedGoalAsync();

        var history = (await AttemptAsync(goal, 2)).History;

        Assert.Equal(Enumerable.Range(0, 6).Select(day => Date(2026, 11, 23).AddDays(-day)), history.Entries.Select(entry => entry.FirstDate));
        Assert.Equal(EvaluationState.Due, history.Entries[0].State);
        Assert.All(history.Entries, entry => Assert.Equal((2, true), (entry.AttemptNumber, entry.IsLatestAttempt)));
    }

    [Fact]
    public async Task FR_006_an_earlier_attempt_keeps_commitments_and_Build_when_attempt_2_counts_weeks_as_Reduce()
    {
        var goal = await EndedGoalAsync();
        _world.At(Utc("2026-11-18T08:00:00Z"));
        Assert.Equal(AttemptStartOutcome.Started,
            (await StartAsync(goal, DurationTimes(30, DurationUnit.Minutes, 1, SchedulePeriod.Week, BuildOrReduce.Reduce), Wed18)).Outcome);
        _world.At(Utc("2026-12-08T12:00:00Z"));

        var attempts = await AttemptsAsync(goal);
        var first = await AttemptAsync(goal, 1);
        var second = await AttemptAsync(goal, 2);

        Assert.Equal((StreakUnit.Weeks, StreakUnit.Commitments), (attempts[0].Stats.Unit, attempts[1].Stats.Unit));
        Assert.Equal("85% (12 of 14)", attempts[1].Stats.ConsistencyText);
        Assert.Equal((BuildOrReduce.Build, GoalScheduleKind.CommitmentBased), (first.History.BuildOrReduce, first.History.GoalScheduleKind));
        Assert.All(first.History.Entries, entry => Assert.Equal(BuildOrReduce.Build, first.History.SettingsOf(entry).BuildOrReduce));
        Assert.Equal((BuildOrReduce.Reduce, GoalScheduleKind.PeriodBased), (second.History.BuildOrReduce, second.History.GoalScheduleKind));
        Assert.Equal("Reading, 10 min daily", attempts[1].Name);
        Assert.Equal("Reading, at most 30 min, once a week", attempts[0].Name);
    }

    [Fact]
    public async Task AC_102_an_attempt_ended_before_it_began_has_no_date_an_empty_History_and_NA()
    {
        var goal = await _world.CreateGoalAsync(MinutesDaily(10), Mon2, Created);
        // Ended on Sunday 1 November, the account's today, before the start date: its last date is Saturday 31 October.
        _world.At(Utc("2026-11-01T11:00:00Z"));
        var ended = await _world.Changes.EndAsync(_world.User, goal, Outcome.NoLongerRelevant, null, None);
        Assert.Equal(((DateOnly?)Date(2026, 11, 1), (DateOnly?)Date(2026, 10, 31)), (ended.EndingDate, ended.LastDate));
        Assert.Equal(AttemptStartOutcome.Started, (await StartAsync(goal, MinutesDaily(20), Mon2)).Outcome);
        _world.At(Utc("2026-11-10T12:00:00Z"));

        var details = await AttemptAsync(goal, 1);
        var attempts = await AttemptsAsync(goal);

        Assert.True(details.Attempt.HasNoDate);
        Assert.Empty(details.History.Entries);
        Assert.Empty(details.Recent.Results);
        Assert.Equal(("N/A", "0 commitments"), (details.Attempt.Stats.ConsistencyText, details.Attempt.Stats.LongestStreakText));
        Assert.Equal((Reading10, (DateOnly?)Date(2026, 10, 31), (Outcome?)Outcome.NoLongerRelevant), (details.Attempt.Name, details.Attempt.LastDate, details.Attempt.Outcome));
        Assert.Equal([false, true], attempts.Select(attempt => attempt.HasNoDate));
        Assert.Equal("Reading, 20 min daily", attempts[0].Name);
    }

    [Fact]
    public async Task FR_014_a_History_of_more_than_366_dates_gives_the_newest_366()
    {
        var goal = await _world.CreateGoalAsync(MinutesDaily(10), Mon2, Created);
        _world.At(Utc("2027-12-20T12:00:00Z"));

        var history = (await AttemptAsync(goal, 1)).History;

        Assert.Equal(366, history.Entries.Count);
        Assert.Equal((Date(2027, 12, 20), Date(2026, 12, 20)), (history.Entries[0].FirstDate, history.Entries[^1].FirstDate));
    }

    [Fact]
    public async Task FR_014_an_ended_attempts_History_of_more_than_366_dates_gives_the_newest_366_up_to_its_last_date()
    {
        var goal = await _world.CreateGoalAsync(MinutesDaily(10), Mon2, Created);
        _world.At(Utc("2027-12-20T12:00:00Z"));
        await _world.EndAsync(goal);
        _world.At(Utc("2028-01-10T12:00:00Z"));

        var history = (await AttemptAsync(goal, 1)).History;

        Assert.Equal(366, history.Entries.Count);
        Assert.Equal((Date(2027, 12, 19), Date(2026, 12, 19)), (history.Entries[0].FirstDate, history.Entries[^1].FirstDate));
    }

    [Fact]
    public async Task US3_AS12_after_a_restart_on_17_November_what_can_be_recorded_and_the_dashboard_give_attempt_1s_Pending_15_and_attempt_2s_Due_17()
    {
        var goal = await EndedGoalAsync();
        _world.At(Utc("2026-11-17T09:00:00Z"));
        Assert.Equal(AttemptStartOutcome.Started, (await StartAsync(goal, MinutesDaily(10), Tue17)).Outcome);
        _world.At(Utc("2026-11-17T10:00:00Z"));

        var recordable = await _world.RecordableAsync(goal);
        var dashboard = await _world.DashboardAsync();

        Assert.Equal([(Tue17, EvaluationState.Due, 2, true), (Sun15, EvaluationState.Pending, 1, false)],
            recordable.Entries.Select(entry => (entry.FirstDate, entry.State, entry.AttemptNumber, entry.IsLatestAttempt)));
        Assert.DoesNotContain(recordable.Entries, entry => entry.FirstDate == Sat14);
        var today = dashboard.Today.Where(entry => entry.GoalId == goal).Select(entry => (entry.Entry.FirstDate, entry.Entry.State, entry.Entry.AttemptNumber, entry.Entry.IsLatestAttempt)).ToList();
        Assert.Equal([(Sun15, EvaluationState.Pending, 1, false), (Tue17, EvaluationState.Due, 2, true)], today);
        Assert.Equal((1, 1, 0), (dashboard.Counts.Pending, dashboard.Counts.Due, dashboard.Counts.MissedThisWeek));

        _world.At(Utc("2026-11-17T10:05:00Z"));
        Assert.Equal(RecordWriteOutcome.Recorded, (await _world.RecordAsync(goal, Sun15, new RecordInput { Amount = 10 })).Outcome);

        var fifteenth = (await AttemptAsync(goal, 1)).History.Entries[0];
        Assert.Equal((Sun15, 1), (fifteenth.FirstDate, fifteenth.Records.Count));
        Assert.DoesNotContain((await AttemptAsync(goal, 2)).History.Entries, entry => entry.Records.Count > 0);

        // 15 November turns final when its grace period ends, at 00:00Z on 18 November; 17 November is then still Pending.
        _world.At(Utc("2026-11-18T00:00:00Z"));
        var attempts = await AttemptsAsync(goal);
        Assert.Equal(("N/A", "92% (13 of 14)"), (attempts[0].Stats.ConsistencyText, attempts[1].Stats.ConsistencyText));
        Assert.Equal("N/A", (await _world.ReportAsync(goal)).Stats.ConsistencyText);
    }

    [Fact]
    public async Task FR_022_FR_023_the_attempt_reads_refuse_anonymous_visitors_and_administrators_and_find_no_goal_of_another_user()
    {
        var goal = await RestartedGoalAsync();
        var service = _world.Attempts;

        var lists = new[] { ActingUser.Anonymous, _world.Administrator, _world.OtherUser }
            .Select(user => service.GetAttemptsAsync(user, goal, None).GetAwaiter().GetResult())
            .Select(result => (result.Outcome, result.Message, result.Attempts.Count))
            .ToList();
        var reads = new[] { ActingUser.Anonymous, _world.Administrator, _world.OtherUser }
            .Select(user => service.GetAttemptAsync(user, goal, 1, None).GetAwaiter().GetResult())
            .Select(result => (result.Outcome, result.Message, result.Details is null))
            .ToList();

        Assert.Equal(
            [
                (AttemptListOutcome.NotSignedIn, (string?)"You are not signed in.", 0),
                (AttemptListOutcome.NotForAdministrators, "Goals are for registered users.", 0),
                (AttemptListOutcome.NotFound, "This goal does not exist.", 0),
            ],
            lists);
        Assert.Equal(
            [
                (AttemptReadOutcome.NotSignedIn, (string?)"You are not signed in.", true),
                (AttemptReadOutcome.NotForAdministrators, "Goals are for registered users.", true),
                (AttemptReadOutcome.NotFound, "This goal does not exist.", true),
            ],
            reads);
    }

    [Fact]
    public async Task FR_022_an_unknown_goal_is_not_found_before_its_attempt_number_is_looked_at()
    {
        await RestartedGoalAsync();

        var result = await _world.Attempts.GetAttemptAsync(_world.User, new GoalId(Guid.NewGuid()), 7, None);

        Assert.Equal((AttemptReadOutcome.NotFound, "This goal does not exist."), (result.Outcome, result.Message));
    }

    [Fact]
    public async Task FR_014_the_attempt_list_reads_records_once_per_attempt_with_a_date()
    {
        var goal = await RestartedGoalAsync();
        var reads = _world.Records.Calls.Count(call => call == "ReadAsync");

        await AttemptsAsync(goal);

        Assert.Equal(2, _world.Records.Calls.Count(call => call == "ReadAsync") - reads);
    }
}
