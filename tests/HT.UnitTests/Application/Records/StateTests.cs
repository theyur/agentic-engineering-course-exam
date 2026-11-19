using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Time;
using static HT.UnitTests.Application.Records.RecordWorld;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Records;

/// <summary>
/// The states of US2 (US2-AS1 to AS13, FR-010, FR-012, FR-013, FR-016, HP-010; slice 4 FR-027) through
/// <c>RecordAsync</c> and <c>GetHistoryAsync</c>, over the fakes of <see cref="RecordWorld"/>. Unless a test says
/// otherwise the user has no stored time zone, so the goals are in UTC: a day runs from 00:00Z to 00:00Z the next day
/// and its grace period ends 48 hours after that; a week runs from Monday 00:00Z to the next Monday 00:00Z. The goals
/// start on Monday 16 November 2026 and are created at 08:00Z that day. AS4 to AS10 take one representative final case
/// each; the Domain tests carry the full matrix. Every instant is worked out by hand.
/// </summary>
public sealed class StateTests
{
    private static readonly DateOnly Mon16 = Date(2026, 11, 16);
    private static readonly DateOnly Tue17 = Date(2026, 11, 17);
    private static readonly DateOnly Wed18 = Date(2026, 11, 18);
    private static readonly DateOnly Sun22 = Date(2026, 11, 22);

    private static readonly DateTimeOffset Mon16Morning = Utc("2026-11-16T08:00:00Z");

    private static readonly NamedTimeZone Honolulu = Zone("Pacific/Honolulu");
    private static readonly NamedTimeZone Kiritimati = Zone("Pacific/Kiritimati");

    private readonly RecordWorld _world = new();

    private static GoalTargetFields Reading20MinDaily => DurationDays(20, DurationUnit.Minutes, BuildOrReduce.Build, AllDays);

    private Task<GoalId> GoalAsync(GoalTargetFields target, DateOnly? start = null) =>
        _world.CreateGoalAsync(target, start ?? Mon16, Mon16Morning);

    /// <summary>Records at <paramref name="at"/> and gives the entry after the write.</summary>
    private async Task<HistoryEntry> RecordAtAsync(GoalId goal, DateOnly date, RecordInput input, DateTimeOffset at)
    {
        _world.At(at);
        var result = await _world.RecordAsync(goal, date, input);
        Assert.Equal(RecordWriteOutcome.Recorded, result.Outcome);
        return result.Entry!;
    }

    /// <summary>The entries from <paramref name="first"/> to <paramref name="last"/>, read at <paramref name="at"/>.</summary>
    private async Task<IReadOnlyList<HistoryEntry>> ReadAtAsync(
        GoalId goal, DateOnly first, DateOnly last, DateTimeOffset at, ActingUser? user = null, RecordWorld? world = null)
    {
        world ??= _world;
        world.At(at);
        var result = await world.Service.GetHistoryAsync(user ?? world.User, goal, first, last, CancellationToken.None);
        Assert.Equal(HistoryOutcome.Found, result.Outcome);
        return result.History!.Entries;
    }

    private async Task<HistoryEntry> OneAtAsync(GoalId goal, DateOnly date, DateTimeOffset at) =>
        Assert.Single(await ReadAtAsync(goal, date, date, at));

    private static IEnumerable<EvaluationState> States(IEnumerable<HistoryEntry> entries) => entries.Select(e => e.State);

    // ---- AS1 to AS3: Upcoming, Due, Pending and Missed, with nothing but reads in between (HP-010) ----

    [Fact]
    public async Task US2_AS1_to_AS3_a_commitment_is_Upcoming_then_Due_then_Pending_then_Missed_by_the_clock_alone()
    {
        var goal = await GoalAsync(Reading20MinDaily, Wed18);

        var beforeStart = await OneAtAsync(goal, Wed18, Utc("2026-11-17T23:59:59Z"));
        var atStart = await OneAtAsync(goal, Wed18, Utc("2026-11-18T00:00:00Z"));
        var beforeEnd = await OneAtAsync(goal, Wed18, Utc("2026-11-18T23:59:59Z"));
        var atEnd = await OneAtAsync(goal, Wed18, Utc("2026-11-19T00:00:00Z"));
        var beforeGraceEnd = await OneAtAsync(goal, Wed18, Utc("2026-11-20T23:59:59Z"));
        var atGraceEnd = await OneAtAsync(goal, Wed18, Utc("2026-11-21T00:00:00Z"));
        var later = await OneAtAsync(goal, Wed18, Utc("2026-11-30T00:00:00Z"));

        Assert.Equal((Utc("2026-11-18T00:00:00Z"), Utc("2026-11-19T00:00:00Z")), (beforeStart.Starts, beforeStart.Ends));
        Assert.Equal((EvaluationState.Upcoming, (DateTimeOffset?)null), (beforeStart.State, beforeStart.GraceEnds));
        Assert.Equal((EvaluationState.Due, Utc("2026-11-21T00:00:00Z")), (atStart.State, atStart.GraceEnds!.Value));
        Assert.Equal(EvaluationState.Due, beforeEnd.State);
        Assert.Equal((EvaluationState.Pending, Utc("2026-11-21T00:00:00Z")), (atEnd.State, atEnd.GraceEnds!.Value));
        Assert.Equal((EvaluationState.Pending, Utc("2026-11-21T00:00:00Z")), (beforeGraceEnd.State, beforeGraceEnd.GraceEnds!.Value));
        Assert.Equal((EvaluationState.Missed, (DateTimeOffset?)null), (atGraceEnd.State, atGraceEnd.GraceEnds));
        Assert.Equal(EvaluationState.Missed, later.State);
        Assert.All(_world.Records.Calls, call => Assert.Equal(nameof(IRecordStore.ReadAsync), call));
        Assert.Equal(7, _world.Records.Calls.Count);
    }

    // ---- AS4 to AS10: one representative final case each ----

    [Fact]
    public async Task US2_AS4_a_Build_duration_commitment_of_20_minutes_is_Failed_at_19_and_Met_at_20_and_keeps_the_amount()
    {
        var goal = await GoalAsync(Reading20MinDaily);
        await RecordAtAsync(goal, Mon16, Minutes(19), Utc("2026-11-17T10:00:00Z"));
        await RecordAtAsync(goal, Tue17, Minutes(20), Utc("2026-11-17T10:00:01Z"));

        var entries = await ReadAtAsync(goal, Mon16, Tue17, Utc("2026-11-20T00:00:00Z"));

        Assert.Equal([EvaluationState.Failed, EvaluationState.Met], States(entries));
        Assert.Equal([19, 20], entries.Select(e => Assert.Single(e.Records).Amount!.Value));
        Assert.Equal(new Progress(ProgressKind.RecordedValue, 19, 20), entries[0].Progress);
    }

    [Fact]
    public async Task US2_AS5_a_Reduce_count_commitment_of_at_most_2_cups_is_Met_at_2_and_Failed_at_3()
    {
        var goal = await GoalAsync(CountDays(2, "cups", BuildOrReduce.Reduce, AllDays));
        await RecordAtAsync(goal, Mon16, Amount(2), Utc("2026-11-17T10:00:00Z"));
        await RecordAtAsync(goal, Tue17, Amount(3), Utc("2026-11-17T10:00:01Z"));

        var entries = await ReadAtAsync(goal, Mon16, Tue17, Utc("2026-11-20T00:00:00Z"));

        Assert.Equal([EvaluationState.Met, EvaluationState.Failed], States(entries));
    }

    [Fact]
    public async Task US2_AS6_a_Reduce_yes_no_commitment_is_Met_with_No_and_Failed_with_Yes()
    {
        var goal = await GoalAsync(YesNoDays(BuildOrReduce.Reduce, AllDays));
        await RecordAtAsync(goal, Mon16, No(), Utc("2026-11-17T10:00:00Z"));
        await RecordAtAsync(goal, Tue17, Yes(), Utc("2026-11-17T10:00:01Z"));

        var entries = await ReadAtAsync(goal, Mon16, Tue17, Utc("2026-11-20T00:00:00Z"));

        Assert.Equal([EvaluationState.Met, EvaluationState.Failed], States(entries));
    }

    [Fact]
    public async Task US2_AS7_a_per_week_Build_yes_no_week_with_2_Yes_and_1_No_is_Failed()
    {
        var goal = await GoalAsync(YesNoTimes(3, SchedulePeriod.Week));
        await RecordAtAsync(goal, Mon16, Yes(), Utc("2026-11-18T10:00:00Z"));
        await RecordAtAsync(goal, Tue17, Yes(), Utc("2026-11-18T10:00:01Z"));
        await RecordAtAsync(goal, Wed18, No(), Utc("2026-11-18T10:00:02Z"));

        var week = await OneAtAsync(goal, Mon16, Utc("2026-11-25T00:00:00Z"));

        Assert.Equal((Mon16, Sun22), (week.FirstDate, week.LastDate));
        Assert.Equal(EvaluationState.Failed, week.State);
        Assert.Equal(new Progress(ProgressKind.RecordsReachingAmount, 2, 3), week.Progress);
    }

    [Fact]
    public async Task US2_AS8_a_per_week_week_of_20_25_12_and_30_pages_is_Met()
    {
        var goal = await GoalAsync(CountTimes(20, "pages", 3, SchedulePeriod.Week));
        await RecordAtAsync(goal, Mon16, Amount(20), Utc("2026-11-18T10:00:00Z"));
        await RecordAtAsync(goal, Tue17, Amount(25), Utc("2026-11-18T10:00:01Z"));
        await RecordAtAsync(goal, Wed18, Amount(12), Utc("2026-11-18T10:00:02Z"));
        await RecordAtAsync(goal, Wed18, Amount(30), Utc("2026-11-18T10:00:03Z"));

        var week = await OneAtAsync(goal, Mon16, Utc("2026-11-25T00:00:00Z"));

        Assert.Equal(EvaluationState.Met, week.State);
        Assert.Equal([20, 25, 12, 30], week.Records.Select(r => r.Amount!.Value));
    }

    [Fact]
    public async Task US2_AS9_a_per_day_Reduce_count_day_of_at_most_1_sweet_is_Met_at_0_and_1_and_Failed_at_1_and_1()
    {
        var goal = await GoalAsync(CountTimes(1, "sweet", 1, SchedulePeriod.Day, BuildOrReduce.Reduce));
        await RecordAtAsync(goal, Mon16, Amount(0), Utc("2026-11-17T10:00:00Z"));
        await RecordAtAsync(goal, Mon16, Amount(1), Utc("2026-11-17T10:00:01Z"));
        await RecordAtAsync(goal, Tue17, Amount(1), Utc("2026-11-17T10:00:02Z"));
        await RecordAtAsync(goal, Tue17, Amount(1), Utc("2026-11-17T10:00:03Z"));

        var entries = await ReadAtAsync(goal, Mon16, Tue17, Utc("2026-11-20T00:00:00Z"));

        Assert.Equal([EvaluationState.Met, EvaluationState.Failed], States(entries));
        Assert.Equal([new Progress(ProgressKind.Total, 1, 1), new Progress(ProgressKind.Total, 2, 1)], entries.Select(e => e.Progress));
    }

    [Fact]
    public async Task US2_AS10_a_per_day_Reduce_yes_no_day_of_at_most_once_is_Failed_with_two_Yes_and_Met_with_one_No()
    {
        var goal = await GoalAsync(YesNoTimes(1, SchedulePeriod.Day, BuildOrReduce.Reduce));
        await RecordAtAsync(goal, Mon16, Yes(), Utc("2026-11-17T10:00:00Z"));
        await RecordAtAsync(goal, Mon16, Yes(), Utc("2026-11-17T10:00:01Z"));
        await RecordAtAsync(goal, Tue17, No(), Utc("2026-11-17T10:00:02Z"));

        var entries = await ReadAtAsync(goal, Mon16, Tue17, Utc("2026-11-20T00:00:00Z"));

        Assert.Equal([EvaluationState.Failed, EvaluationState.Met], States(entries));
    }

    // ---- AS11: a record that reaches the target stays Pending until the grace end (FR-012) ----

    [Fact]
    public async Task US2_AS11_a_commitment_that_reaches_the_target_is_Pending_with_its_progress_then_Met()
    {
        var goal = await GoalAsync(Reading20MinDaily);
        var recorded = await RecordAtAsync(goal, Mon16, Minutes(20), Utc("2026-11-16T10:00:00Z"));

        var atEnd = await OneAtAsync(goal, Mon16, Utc("2026-11-17T00:00:00Z"));
        var beforeGraceEnd = await OneAtAsync(goal, Mon16, Utc("2026-11-18T23:59:59Z"));
        var atGraceEnd = await OneAtAsync(goal, Mon16, Utc("2026-11-19T00:00:00Z"));

        Assert.Equal(EvaluationState.Due, recorded.State);
        Assert.Equal((EvaluationState.Pending, Utc("2026-11-19T00:00:00Z")), (atEnd.State, atEnd.GraceEnds!.Value));
        Assert.Equal(new Progress(ProgressKind.RecordedValue, 20, 20), atEnd.Progress);
        Assert.Equal(EvaluationState.Pending, beforeGraceEnd.State);
        Assert.Equal(EvaluationState.Met, atGraceEnd.State);
    }

    [Fact]
    public async Task US2_AS11_a_week_that_reaches_the_target_is_Pending_with_its_progress_then_Met()
    {
        var goal = await GoalAsync(YesNoTimes(3, SchedulePeriod.Week));
        await RecordAtAsync(goal, Mon16, Yes(), Utc("2026-11-18T10:00:00Z"));
        await RecordAtAsync(goal, Tue17, Yes(), Utc("2026-11-18T10:00:01Z"));
        await RecordAtAsync(goal, Wed18, Yes(), Utc("2026-11-18T10:00:02Z"));

        var beforeGraceEnd = await OneAtAsync(goal, Mon16, Utc("2026-11-24T23:59:59Z"));
        var atGraceEnd = await OneAtAsync(goal, Mon16, Utc("2026-11-25T00:00:00Z"));

        Assert.Equal((EvaluationState.Pending, Utc("2026-11-25T00:00:00Z")), (beforeGraceEnd.State, beforeGraceEnd.GraceEnds!.Value));
        Assert.Equal(new Progress(ProgressKind.RecordsReachingAmount, 3, 3), beforeGraceEnd.Progress);
        Assert.Equal(EvaluationState.Met, atGraceEnd.State);
    }

    // ---- AS12: nothing recorded is never success (FR-013) ----

    [Fact]
    public async Task US2_AS12_a_Reduce_commitment_with_no_record_is_Missed_and_with_No_Met()
    {
        var goal = await GoalAsync(YesNoDays(BuildOrReduce.Reduce, AllDays));
        await RecordAtAsync(goal, Tue17, No(), Utc("2026-11-17T10:00:00Z"));

        var entries = await ReadAtAsync(goal, Mon16, Tue17, Utc("2026-11-20T00:00:00Z"));

        Assert.Equal([EvaluationState.Missed, EvaluationState.Met], States(entries));
    }

    [Fact]
    public async Task US2_AS12_a_Reduce_period_with_no_record_is_Missed_and_with_0_Met()
    {
        var goal = await GoalAsync(CountTimes(2, "cups", 1, SchedulePeriod.Day, BuildOrReduce.Reduce));
        await RecordAtAsync(goal, Tue17, Amount(0), Utc("2026-11-17T10:00:00Z"));

        var entries = await ReadAtAsync(goal, Mon16, Tue17, Utc("2026-11-20T00:00:00Z"));

        Assert.Equal([EvaluationState.Missed, EvaluationState.Met], States(entries));
        Assert.Equal(new Progress(ProgressKind.Total, 0, 2), entries[0].Progress);
    }

    // ---- AS13: the grace end is 48 elapsed hours, not a local midnight (FR-016) ----

    [Fact]
    public async Task US2_AS13_a_Kyiv_commitment_on_Saturday_24_October_2026_is_Pending_until_23_00_Kyiv_time_on_Monday_26()
    {
        // Saturday 24 October 2026 in Kyiv (UTC+3) runs from 2026-10-23T21:00Z to 2026-10-24T21:00Z; the clocks go back
        // to UTC+2 at 01:00Z on Sunday 25, so 48 hours later is 2026-10-26T21:00Z, 23:00 Kyiv time on Monday 26.
        var world = new RecordWorld(Utc("2026-10-20T00:00:00Z"));
        var kyiv = world.UserIn("Europe/Kyiv");
        var saturday = Date(2026, 10, 24);
        var goal = await world.CreateGoalAsync(Reading20MinDaily, saturday, Utc("2026-10-24T05:00:00Z"), kyiv);

        var before = Assert.Single(await ReadAtAsync(goal, saturday, saturday, Utc("2026-10-26T20:59:00Z"), kyiv, world));
        var at = Assert.Single(await ReadAtAsync(goal, saturday, saturday, Utc("2026-10-26T21:00:00Z"), kyiv, world));

        Assert.Equal((Utc("2026-10-23T21:00:00Z"), Utc("2026-10-24T21:00:00Z")), (before.Starts, before.Ends));
        Assert.Equal((EvaluationState.Pending, Utc("2026-10-26T21:00:00Z")), (before.State, before.GraceEnds!.Value));
        Assert.False(before.IsCorrection);
        Assert.Equal((EvaluationState.Missed, (DateTimeOffset?)null), (at.State, at.GraceEnds));
        Assert.True(at.IsCorrection);
    }

    // ---- Edge cases: time-zone changes (slice 4 FR-027) ----

    [Fact]
    public async Task Edge_case_after_a_large_move_east_the_new_versions_first_commitment_that_has_already_ended_is_Pending_from_its_own_end()
    {
        // Version 1 in Honolulu (UTC−10) from Monday 16, version 2 in Kiritimati (UTC+14) from Wednesday 18. Wednesday 18 in
        // Kiritimati runs from 2026-11-17T10:00Z to 2026-11-18T10:00Z, so at 12:00Z on the 18th, when the move is made, it
        // has already ended. It is recorded then, is Pending until 2026-11-20T10:00Z, and is Met from then on.
        var goal = _world.AddGoal(_world.User, Target(Reading20MinDaily), (Mon16, Honolulu), (Wed18, Kiritimati));

        var recorded = await RecordAtAsync(goal, Wed18, Minutes(20), Utc("2026-11-18T12:00:00Z"));
        var beforeGraceEnd = await OneAtAsync(goal, Wed18, Utc("2026-11-20T09:59:59Z"));
        var atGraceEnd = await OneAtAsync(goal, Wed18, Utc("2026-11-20T10:00:00Z"));

        Assert.Equal((Utc("2026-11-17T10:00:00Z"), Utc("2026-11-18T10:00:00Z")), (recorded.Starts, recorded.Ends));
        Assert.Equal((EvaluationState.Pending, Utc("2026-11-20T10:00:00Z")), (recorded.State, recorded.GraceEnds!.Value));
        Assert.Equal(EvaluationState.Pending, beforeGraceEnd.State);
        Assert.Equal(EvaluationState.Met, atGraceEnd.State);
    }

    [Fact]
    public async Task Edge_case_across_a_gap_after_a_move_west_two_consecutive_commitments_are_Pending_with_different_grace_ends()
    {
        // Version 1 in Tokyo (UTC+9) from Monday 16, version 2 in New York (UTC−5) from Wednesday 18. Tuesday 17 ends at
        // 2026-11-17T15:00Z; Wednesday 18 runs from 2026-11-18T05:00Z to 2026-11-19T05:00Z, after a 14-hour gap.
        var goal = _world.AddGoal(_world.User, Target(Reading20MinDaily), (Mon16, Tokyo), (Wed18, NewYork));

        var pending = await ReadAtAsync(goal, Tue17, Wed18, Utc("2026-11-19T12:00:00Z"));
        var after = await ReadAtAsync(goal, Tue17, Wed18, Utc("2026-11-19T15:00:00Z"));

        Assert.Equal([EvaluationState.Pending, EvaluationState.Pending], States(pending));
        Assert.Equal([Utc("2026-11-19T15:00:00Z"), Utc("2026-11-21T05:00:00Z")], pending.Select(e => e.GraceEnds!.Value));
        Assert.Equal([EvaluationState.Missed, EvaluationState.Pending], States(after));
    }

    [Fact]
    public async Task Edge_case_across_an_overlap_after_a_move_east_two_consecutive_commitments_are_Pending_with_different_grace_ends()
    {
        // Version 1 in New York (UTC−5) from Monday 16, version 2 in Tokyo (UTC+9) from Wednesday 18. Tuesday 17 runs from
        // 2026-11-17T05:00Z to 2026-11-18T05:00Z; Wednesday 18 from 2026-11-17T15:00Z to 2026-11-18T15:00Z.
        var goal = _world.AddGoal(_world.User, Target(Reading20MinDaily), (Mon16, NewYork), (Wed18, Tokyo));

        var pending = await ReadAtAsync(goal, Tue17, Wed18, Utc("2026-11-18T20:00:00Z"));
        var between = await ReadAtAsync(goal, Tue17, Wed18, Utc("2026-11-20T05:00:00Z"));
        var after = await ReadAtAsync(goal, Tue17, Wed18, Utc("2026-11-20T15:00:00Z"));

        Assert.Equal([EvaluationState.Pending, EvaluationState.Pending], States(pending));
        Assert.Equal([Utc("2026-11-20T05:00:00Z"), Utc("2026-11-20T15:00:00Z")], pending.Select(e => e.GraceEnds!.Value));
        Assert.Equal([EvaluationState.Missed, EvaluationState.Pending], States(between));
        Assert.Equal([EvaluationState.Missed, EvaluationState.Missed], States(after));
    }

    // ---- The independent test of the story ----

    [Fact]
    public async Task Independent_test_nothing_on_day_1_a_partial_amount_on_day_2_and_the_full_amount_on_day_3()
    {
        // Days 1 to 3 are Monday 16 to Wednesday 18; their grace periods end at 00:00Z on the 19th, 20th and 21st.
        var goal = await GoalAsync(Reading20MinDaily);
        await RecordAtAsync(goal, Tue17, Minutes(12), Utc("2026-11-17T10:00:00Z"));
        await RecordAtAsync(goal, Wed18, Minutes(20), Utc("2026-11-18T10:00:00Z"));

        var states = new List<EvaluationState[]>();
        foreach (var at in new[]
                 {
                     "2026-11-18T23:59:59Z", "2026-11-19T00:00:00Z",
                     "2026-11-19T23:59:59Z", "2026-11-20T00:00:00Z",
                     "2026-11-20T23:59:59Z", "2026-11-21T00:00:00Z",
                 })
        {
            states.Add(States(await ReadAtAsync(goal, Mon16, Wed18, Utc(at))).ToArray());
        }

        Assert.Equal(
            [
                [EvaluationState.Pending, EvaluationState.Pending, EvaluationState.Due],
                [EvaluationState.Missed, EvaluationState.Pending, EvaluationState.Pending],
                [EvaluationState.Missed, EvaluationState.Pending, EvaluationState.Pending],
                [EvaluationState.Missed, EvaluationState.Failed, EvaluationState.Pending],
                [EvaluationState.Missed, EvaluationState.Failed, EvaluationState.Pending],
                [EvaluationState.Missed, EvaluationState.Failed, EvaluationState.Met],
            ],
            states);
        var final = await ReadAtAsync(goal, Mon16, Wed18, Utc("2026-11-21T00:00:01Z"));
        Assert.Equal(
            [new Progress(ProgressKind.RecordedValue, null, 20), new Progress(ProgressKind.RecordedValue, 12, 20), new Progress(ProgressKind.RecordedValue, 20, 20)],
            final.Select(e => e.Progress));
    }
}
