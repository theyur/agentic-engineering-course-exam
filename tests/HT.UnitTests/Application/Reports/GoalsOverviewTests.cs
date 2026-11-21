using HT.Application.Exemptions;
using HT.Application.Goals;
using HT.Application.Records;
using HT.Application.Reports;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Reports;
using HT.UnitTests.Application.Exemptions;
using HT.UnitTests.Application.Records;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Reports;

/// <summary>
/// The goals overview through <see cref="ReportService.GetGoalsOverviewAsync"/> (US5-AS1 to AS4; FR-011, FR-013,
/// FR-016; research R-07), over the fakes of <see cref="ReportWorld"/>. The user has no stored time zone, so the goals
/// are in UTC. A day runs from 00:00Z to 00:00Z the next day and its grace period ends 48 hours later; a week runs from
/// Monday 00:00Z and its grace period ends on the Wednesday after it, at 00:00Z. Every instant is worked out by hand.
/// </summary>
public sealed class GoalsOverviewTests
{
    private static readonly DateOnly Mon2 = Date(2026, 11, 2);
    private static readonly DateOnly Mon16 = Date(2026, 11, 16);
    private static readonly DateOnly Tue17 = Date(2026, 11, 17);
    private static readonly DateOnly Wed18 = Date(2026, 11, 18);
    private static readonly DateOnly Wed25 = Date(2026, 11, 25);
    private static readonly DateTimeOffset Mon2Morning = Utc("2026-11-02T08:00:00Z");
    private static readonly DateTimeOffset Mon16Morning = Utc("2026-11-16T08:00:00Z");

    /// <summary>
    /// Tuesday 17 November, 10:00Z: Monday 16's grace period ends on Thursday 19 at 00:00Z, so it is Pending; Tuesday
    /// 17 is Due; the week of Monday 16 is Due.
    /// </summary>
    private static readonly DateTimeOffset Tue17TenOClock = Utc("2026-11-17T10:00:00Z");

    /// <summary>
    /// Tuesday 24 November, 10:00Z: Saturday 21 November is final (its grace period ended at 00:00Z), Sunday 22 and
    /// Monday 23 are Pending and Tuesday 24 is Due.
    /// </summary>
    private static readonly DateTimeOffset Tue24TenOClock = Utc("2026-11-24T10:00:00Z");

    private static readonly IReadOnlyList<HistoryEntry> NoEntries = [];
    private static readonly IReadOnlyList<RecordView> NoRecords = [];

    private readonly ReportWorld _world = new();

    private static GoalTargetFields Reading20MinDaily => DurationDays(20, DurationUnit.Minutes, BuildOrReduce.Build, AllDays);

    /// <summary>
    /// "C" starts on Wednesday 25 November (nothing yet), "A" is daily from Tuesday 17 (Due only on Tuesday 17) and "B" is
    /// on Monday, Wednesday and Friday from Monday 16 (Monday 16 Pending on Tuesday 17, nothing Due), created in that order
    /// on Monday 16 at 08:00Z, so neither the creation order nor the name order is the goal order.
    /// </summary>
    private async Task<(GoalId A, GoalId B, GoalId C)> ThreeGroupsAsync()
    {
        var c = await _world.CreateNamedGoalAsync("C", Reading20MinDaily, Wed25, Mon16Morning);
        var a = await _world.CreateNamedGoalAsync("A", Reading20MinDaily, Tue17, Mon16Morning);
        var b = await _world.CreateNamedGoalAsync("B", YesNoDays(BuildOrReduce.Build, MonWedFri), Mon16, Mon16Morning);
        return (a, b, c);
    }

    /// <summary>
    /// "Reading, 20 min daily" from Monday 2 November: 20 minutes on every day to Saturday 21 except Tuesday 10 (10
    /// minutes, Failed) and Sunday 15 (no record, Missed).
    /// </summary>
    private async Task<GoalId> EighteenMetOneFailedOneMissedAsync()
    {
        var goal = await _world.CreateGoalAsync(Reading20MinDaily, Mon2, Mon2Morning);
        for (var date = Mon2; date <= Date(2026, 11, 21); date = date.AddDays(1))
        {
            if (date == Date(2026, 11, 10))
            {
                _world.SeedAmount(goal, date, 10);
            }
            else if (date != Date(2026, 11, 15))
            {
                _world.SeedAmount(goal, date, 20);
            }
        }

        return goal;
    }

    private static OverviewGoal Of(GoalsOverview overview, GoalId goal) => overview.Goals.Single(g => g.Item.Id == goal);

    /// <summary>Equal field by field: the settings, then each entry with its records (the lists compare by content).</summary>
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
    public async Task US5_AS1_every_goal_has_its_consistency_streak_last_15_results_and_what_can_be_recorded()
    {
        var goal = await EighteenMetOneFailedOneMissedAsync();
        _world.At(Tue24TenOClock);

        var overview = await _world.OverviewAsync();

        var item = Assert.Single(overview.Goals);
        Assert.Equal(goal, item.Item.Id);
        Assert.Equal("90% (18 of 20)", item.Stats.ConsistencyText);
        Assert.Equal(new Consistency(18, 20), item.Stats.Consistency);
        Assert.Equal((6, "6 commitments", StreakUnit.Commitments), (item.Stats.CurrentStreak, item.Stats.CurrentStreakText, item.Stats.Unit));
        // The latest 15 ended results are Monday 9 to Monday 23 November, oldest first: 9 Met, 10 Failed, 11 to 14 Met,
        // 15 Missed, 16 to 21 Met, 22 and 23 Pending.
        EvaluationState[] expected =
        [
            EvaluationState.Met, EvaluationState.Failed, EvaluationState.Met, EvaluationState.Met, EvaluationState.Met,
            EvaluationState.Met, EvaluationState.Missed, EvaluationState.Met, EvaluationState.Met, EvaluationState.Met,
            EvaluationState.Met, EvaluationState.Met, EvaluationState.Met, EvaluationState.Pending, EvaluationState.Pending,
        ];
        Assert.Equal(expected, item.Recent.Results.Select(r => r.State));
        Assert.Equal(Date(2026, 11, 9), item.Recent.Results[0].FirstDate);
        Assert.Equal(Date(2026, 11, 23), item.Recent.Results[^1].LastDate);
        Assert.Equal(
            new Dictionary<EvaluationState, int>
            {
                [EvaluationState.Met] = 11, [EvaluationState.Failed] = 1, [EvaluationState.Missed] = 1, [EvaluationState.Pending] = 2,
            },
            item.Recent.Counts);
        Assert.True(item.HasPending);
        Assert.True(item.CanRecord);
        // Its figures are the goal report's at the same instant.
        Assert.Equal((await _world.ReportAsync(goal)).Stats, item.Stats);
        Assert.Equal(Date(2026, 11, 24), overview.Today);
        Assert.Equal(Tue24TenOClock, overview.ReadAt);
    }

    [Fact]
    public async Task US5_AS2_a_goal_with_a_Pending_result_comes_first_then_a_Due_one_then_the_others()
    {
        var (a, b, c) = await ThreeGroupsAsync();
        _world.At(Tue17TenOClock);

        var overview = await _world.OverviewAsync();

        Assert.Equal([b, a, c], overview.Goals.Select(g => g.Item.Id));
        Assert.Equal(["B", "A", "C"], overview.Goals.Select(g => g.Item.Name));
        Assert.Equal([(true, true), (false, true), (false, false)], overview.Goals.Select(g => (g.HasPending, g.CanRecord)));
        Assert.Equal([EvaluationState.Pending], Of(overview, b).Recent.Results.Select(r => r.State));
        Assert.Empty(Of(overview, a).Recent.Results);
        Assert.Empty(Of(overview, c).Recent.Results);
        Assert.All(overview.Goals, g => Assert.Equal("N/A", g.Stats.ConsistencyText));
        Assert.Equal(["0 commitments", "0 commitments", "0 commitments"], overview.Goals.Select(g => g.Stats.CurrentStreakText));
        Assert.Empty(Of(overview, c).Recordable.Entries);
    }

    [Fact]
    public async Task US5_AS2_within_a_group_the_goals_come_by_name_and_then_in_creation_order()
    {
        // Two goals with a Pending result ("Yoga" created before "Walk") and three that have not started ("beta", then
        // "alpha", then "Alpha", whose names compare equal), all created on Monday 16 at 08:00Z.
        var yoga = await _world.CreateNamedGoalAsync("Yoga", YesNoDays(BuildOrReduce.Build, MonWedFri), Mon16, Mon16Morning);
        var walk = await _world.CreateNamedGoalAsync("Walk", YesNoDays(BuildOrReduce.Build, MonWedFri), Mon16, Mon16Morning);
        var beta = await _world.CreateNamedGoalAsync("beta", Reading20MinDaily, Wed25, Mon16Morning);
        var lower = await _world.CreateNamedGoalAsync("alpha", Reading20MinDaily, Wed25, Mon16Morning);
        var upper = await _world.CreateNamedGoalAsync("Alpha", Reading20MinDaily, Wed25, Mon16Morning);
        _world.At(Tue17TenOClock);

        var overview = await _world.OverviewAsync();

        Assert.Equal([walk, yoga, lower, upper, beta], overview.Goals.Select(g => g.Item.Id));
    }

    [Fact]
    public async Task US5_AS4_the_item_equals_the_goal_lists_entry_and_the_goal_list_keeps_its_name_order()
    {
        var (a, b, c) = await ThreeGroupsAsync();
        // A per-week goal whose first week (Monday 23 November) has not begun, and one already counted per week.
        var firstWeek = await _world.CreateNamedGoalAsync("D", YesNoTimes(3, SchedulePeriod.Week), Wed18, Mon16Morning);
        var weekly = await _world.CreateNamedGoalAsync("E", YesNoTimes(3, SchedulePeriod.Week), Mon16, Mon16Morning);
        _world.At(Tue17TenOClock);

        var overview = await _world.OverviewAsync();
        var list = await _world.GoalService.ListAsync(_world.User, CancellationToken.None);

        Assert.Equal(GoalListOutcome.Found, list.Outcome);
        Assert.Equal([a, b, c, firstWeek, weekly], list.Goals.Select(g => g.Id));
        Assert.Equal(list.Today, overview.Today);
        Assert.Equal(list.Goals.Count, overview.Goals.Count);
        foreach (var entry in list.Goals)
        {
            Assert.Equal(entry, Of(overview, entry.Id).Item);
        }

        Assert.Equal(Date(2026, 11, 23), Of(overview, firstWeek).Item.FirstWeekStarts);
    }

    [Fact]
    public async Task US5_AS1_Recordable_equals_what_can_be_recorded_now_at_the_same_instant()
    {
        var reading = await EighteenMetOneFailedOneMissedAsync();
        var weekly = await _world.CreateGoalAsync(YesNoTimes(3, SchedulePeriod.Week), Date(2026, 11, 9), Mon2Morning);
        var later = await _world.CreateGoalAsync(Reading20MinDaily, Date(2026, 12, 1), Mon2Morning);
        _world.At(Utc("2026-11-17T09:00:00Z"));
        await _world.RecordAsync(weekly, Date(2026, 11, 13), RecordWorld.Yes("Good week"));
        _world.SeedAmount(reading, Date(2026, 11, 22), 5);
        _world.At(Tue24TenOClock);

        var overview = await _world.OverviewAsync();

        foreach (var goal in new[] { reading, weekly, later })
        {
            var recordable = await _world.Recording.GetRecordableAsync(_world.User, goal, CancellationToken.None);
            AssertSameHistory(recordable.History!, Of(overview, goal).Recordable);
        }

        // Sunday 22 and Monday 23 Pending and Tuesday 24 Due, newest first; the week of 16 November Pending and the week
        // of 23 November Due.
        Assert.Equal([Date(2026, 11, 24), Date(2026, 11, 23), Date(2026, 11, 22)], Of(overview, reading).Recordable.Entries.Select(e => e.FirstDate));
        Assert.Equal([Date(2026, 11, 23), Date(2026, 11, 16)], Of(overview, weekly).Recordable.Entries.Select(e => e.FirstDate));
        Assert.Empty(Of(overview, later).Recordable.Entries);
    }

    [Fact]
    public async Task US5_AS3_a_Due_entry_with_records_gives_its_progress()
    {
        // "Reading, 20 min daily" from Tuesday 17 November with 12 minutes on Tuesday 17, and "3 times a week" from Monday
        // 16 with one "Yes" on Tuesday 17, both recorded at 10:00Z that day.
        var reading = await _world.CreateGoalAsync(Reading20MinDaily, Tue17, Mon16Morning);
        var weekly = await _world.CreateGoalAsync(YesNoTimes(3, SchedulePeriod.Week), Mon16, Mon16Morning);
        _world.At(Tue17TenOClock);
        await _world.RecordAsync(reading, Tue17, RecordWorld.Minutes(12));
        await _world.RecordAsync(weekly, Tue17, RecordWorld.Yes());

        var overview = await _world.OverviewAsync();

        string ProgressOf(GoalId goal)
        {
            var recordable = Of(overview, goal).Recordable;
            var due = Assert.Single(recordable.Entries, e => e.State == EvaluationState.Due);
            return ReportText.ProgressText(recordable, due);
        }

        Assert.Equal("12 of 20 min today", ProgressOf(reading));
        Assert.Equal("1 of 3 this week", ProgressOf(weekly));
    }

    [Fact]
    public async Task Edge_case_a_goal_whose_catalog_habit_is_then_Unpublished_keeps_its_item_and_figures()
    {
        var goal = await EighteenMetOneFailedOneMissedAsync();
        _world.At(Tue24TenOClock);
        var before = Of(await _world.OverviewAsync(), goal);

        Assert.True(await _world.CatalogHabits.TrySetStateAsync(_world.Habit.Id, CatalogHabitState.Published, CatalogHabitState.Unpublished, CancellationToken.None));
        var after = Of(await _world.OverviewAsync(), goal);

        Assert.Equal(before.Item, after.Item);
        Assert.Equal(before.Stats, after.Stats);
        Assert.Equal(before.Recent.Results, after.Recent.Results);
        Assert.Equal(before.Recent.Counts, after.Recent.Counts);
        Assert.Equal((before.HasPending, before.CanRecord), (after.HasPending, after.CanRecord));
        AssertSameHistory(before.Recordable, after.Recordable);
    }

    [Fact]
    public async Task FR_013_a_user_with_no_goals_is_found_with_none()
    {
        _world.At(Tue17TenOClock);

        var overview = await _world.OverviewAsync();

        Assert.Empty(overview.Goals);
        Assert.Equal(Tue17, overview.Today);
    }

    [Fact]
    public async Task Today_is_the_date_in_the_accounts_time_zone()
    {
        // 22:30Z on Tuesday 17 November is 00:30 on Wednesday 18 in Kyiv (UTC+2 in winter).
        var user = _world.UserIn("Europe/Kyiv");
        _world.At(Utc("2026-11-17T22:30:00Z"));

        var overview = await _world.OverviewAsync(user);

        Assert.Equal(Wed18, overview.Today);
    }

    // ---- slice 7: paused goals (FR-018, US4-AS1, Edge Cases; T073), over the fakes of ExemptionWorld ----

    /// <summary>"Drinking water, 8 glasses a day": Build, count at least 8 "glasses", once per day (period-based).</summary>
    private static GoalTargetFields DrinkingWater => CountTimes(8, "glasses", 1, SchedulePeriod.Day);

    /// <summary>"Meditation, 10 min daily": Build, duration 10 minutes, all seven days (commitment-based).</summary>
    private static GoalTargetFields Meditation => DurationDays(10, DurationUnit.Minutes, BuildOrReduce.Build, AllDays);

    /// <summary>"Yoga, 3 times a week": a yes/no Build goal of three times a week (period-based, per week).</summary>
    private static GoalTargetFields YogaThreeTimesAWeek => YesNoTimes(3, SchedulePeriod.Week);

    /// <summary>09:00Z on Monday 16 November, when US4's pause is set.</summary>
    private static readonly DateTimeOffset Mon16NineOClock = Utc("2026-11-16T09:00:00Z");

    /// <summary>10:00Z on Monday 16 November, when US4-AS1 reads the overview: today is Monday 16.</summary>
    private static readonly DateTimeOffset Mon16TenOClock = Utc("2026-11-16T10:00:00Z");

    /// <summary>
    /// US4's account: "Drinking water, 8 glasses a day" and "Meditation, 10 min daily", both created on Monday 2 November
    /// at 08:00Z with that start date ("Meditation" first, so the creation order is not the goal order), both on target
    /// on each date from 2 to 13 November and not recorded from 14 November on; "Drinking water" paused at 09:00Z on
    /// Monday 16 from Monday 16 until Sunday 22 November. The clock is left at 09:00Z on Monday 16.
    /// </summary>
    private static async Task<(GoalId Water, GoalId Meditation)> UserStoryFourAsync(ExemptionWorld world)
    {
        var meditation = await world.CreateNamedGoalAsync("Meditation, 10 min daily", Meditation, Mon2, Mon2Morning);
        var water = await world.CreateNamedGoalAsync("Drinking water, 8 glasses a day", DrinkingWater, Mon2, Mon2Morning);
        for (var date = Mon2; date <= Date(2026, 11, 13); date = date.AddDays(1))
        {
            world.SeedAmount(water, date, 8);
            world.SeedAmount(meditation, date, 10);
        }

        world.At(Mon16NineOClock);
        Assert.Equal(PauseOutcome.Paused, (await world.PauseAsync(water, Mon16, Date(2026, 11, 22))).Outcome);
        return (water, meditation);
    }

    [Fact]
    public async Task US4_AS1_a_paused_goal_gives_the_last_day_of_its_pause_and_the_goal_order_is_unchanged()
    {
        var world = new ExemptionWorld();
        var (water, meditation) = await UserStoryFourAsync(world);
        world.At(Mon16TenOClock);

        var overview = await world.OverviewAsync();

        // Both have Pending results (14 and 15 November), so they come by name.
        Assert.Equal([water, meditation], overview.Goals.Select(g => g.Item.Id));
        Assert.Equal(["Drinking water, 8 glasses a day", "Meditation, 10 min daily"], overview.Goals.Select(g => g.Item.Name));
        Assert.Equal([Date(2026, 11, 22), null], overview.Goals.Select(g => g.PausedUntil));
        Assert.All(overview.Goals, g => Assert.True(g.HasPending));
    }

    [Fact]
    public async Task Edge_a_goal_whose_current_week_is_Paused_only_by_an_upcoming_pause_is_not_paused()
    {
        var world = new ExemptionWorld();
        var goal = await world.CreateGoalAsync(YogaThreeTimesAWeek, Mon2, Mon2Morning);
        world.At(Mon16NineOClock);
        Assert.Equal(PauseOutcome.Paused, (await world.PauseAsync(goal, Wed18, Wed18)).Outcome);
        world.At(Mon16TenOClock);

        var item = Assert.Single((await world.OverviewAsync()).Goals);

        // The week of 16 November has a paused date, so it is Paused as a whole (D-1); the goal itself is not paused.
        Assert.Equal(EvaluationState.Paused, (await world.EntryAsync(goal, Mon16)).State);
        Assert.Null(item.PausedUntil);
    }

    [Fact]
    public async Task Edge_a_goal_resumed_in_the_middle_of_a_week_is_not_paused()
    {
        var world = new ExemptionWorld();
        var goal = await world.CreateGoalAsync(YogaThreeTimesAWeek, Mon2, Mon2Morning);
        world.At(Mon16NineOClock);
        Assert.Equal(PauseOutcome.Paused, (await world.PauseAsync(goal, Mon16, Date(2026, 11, 22))).Outcome);
        Assert.Equal(Date(2026, 11, 22), Assert.Single((await world.OverviewAsync()).Goals).PausedUntil);

        // 08:00Z on Wednesday 18 November: the pause now ends on Tuesday 17, and the week stays Paused.
        world.At(Utc("2026-11-18T08:00:00Z"));
        Assert.Equal(ResumeOutcome.Resumed, (await world.ResumeAsync(goal)).Outcome);

        var item = Assert.Single((await world.OverviewAsync()).Goals);

        Assert.Equal(EvaluationState.Paused, (await world.EntryAsync(goal, Wed18)).State);
        Assert.Null(item.PausedUntil);
    }

    [Fact]
    public async Task Research_R03_a_goal_that_has_not_started_is_paused_by_a_pause_over_the_accounts_today()
    {
        // Created at 08:00Z on Monday 16 with the start date Wednesday 25: it has no current date, so the pause date is
        // the account's today in UTC, Monday 16.
        var world = new ExemptionWorld();
        var goal = await world.CreateGoalAsync(DrinkingWater, Wed25, Mon16Morning);
        world.At(Mon16NineOClock);
        Assert.Equal(PauseOutcome.Paused, (await world.PauseAsync(goal, Mon16, Date(2026, 11, 27))).Outcome);
        world.At(Mon16TenOClock);

        var item = Assert.Single((await world.OverviewAsync()).Goals);

        Assert.Equal(Date(2026, 11, 27), item.PausedUntil);
    }

    [Fact]
    public async Task FR_018_a_past_pause_does_not_make_the_goal_paused()
    {
        var world = new ExemptionWorld();
        var goal = await world.CreateGoalAsync(DrinkingWater, Mon2, Mon2Morning);
        world.At(Mon16NineOClock);
        Assert.Equal(PauseOutcome.Paused, (await world.PauseAsync(goal, Mon16, Tue17)).Outcome);

        // 10:00Z on Wednesday 18 November: the pause ended on Tuesday 17.
        world.At(Utc("2026-11-18T10:00:00Z"));
        var item = Assert.Single((await world.OverviewAsync()).Goals);

        Assert.Null(item.PausedUntil);
        Assert.Equal(EvaluationState.Paused, (await world.EntryAsync(goal, Tue17)).State);
    }
}
