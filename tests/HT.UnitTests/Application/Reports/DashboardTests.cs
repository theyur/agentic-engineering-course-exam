using HT.Application.Exemptions;
using HT.Application.Records;
using HT.Application.Reports;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using HT.UnitTests.Application.Exemptions;
using HT.UnitTests.Application.Records;
using static HT.UnitTests.Domain.Goals.GoalSamples;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Application.Reports;

/// <summary>
/// The dashboard through <see cref="ReportService.GetDashboardAsync"/> (US6-AS1 to AS8; FR-012, FR-013; research
/// R-06), over the fakes of <see cref="ReportWorld"/>. Unless a test says otherwise the user has no stored time zone,
/// so the account and the goals are in UTC. A day runs from 00:00 to 00:00 the next day and its grace period ends 48
/// hours after it ends (Friday 13 November's at 2026-11-16T00:00:00Z); a week runs from Monday 00:00 and its grace
/// period ends on the Wednesday after it, at 00:00. Every instant is worked out by hand.
/// </summary>
public sealed class DashboardTests
{
    private static readonly DateOnly Mon9 = Date(2026, 11, 9);
    private static readonly DateOnly Fri13 = Date(2026, 11, 13);
    private static readonly DateOnly Sat14 = Date(2026, 11, 14);
    private static readonly DateOnly Sun15 = Date(2026, 11, 15);
    private static readonly DateOnly Mon16 = Date(2026, 11, 16);
    private static readonly DateOnly Tue17 = Date(2026, 11, 17);
    private static readonly DateOnly Wed18 = Date(2026, 11, 18);
    private static readonly DateTimeOffset Mon9Morning = Utc("2026-11-09T08:00:00Z");

    /// <summary>
    /// Tuesday 17 November, 10:00Z; this week started on Monday 16 at 00:00Z. Saturday 14 ended its grace period at 00:00Z
    /// today, Sunday 15's ends on Wednesday 18 and Monday 16's on Thursday 19, both at 00:00Z; the week of 9 November ends
    /// its grace period on Wednesday 18 at 00:00Z.
    /// </summary>
    private static readonly DateTimeOffset Tue17TenOClock = Utc("2026-11-17T10:00:00Z");

    private static readonly IReadOnlyList<HistoryEntry> NoEntries = [];
    private static readonly IReadOnlyList<RecordView> NoRecords = [];

    private readonly ReportWorld _world = new();

    private static GoalTargetFields Reading20MinDaily => DurationDays(20, DurationUnit.Minutes, BuildOrReduce.Build, AllDays);

    private static (GoalId Goal, DateOnly First, DateOnly Last, EvaluationState State) Row(TodayEntry entry) =>
        (entry.GoalId, entry.Entry.FirstDate, entry.Entry.LastDate, entry.Entry.State);

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

    /// <summary>
    /// Three goals from Monday 9 November, created that day at 08:00Z and read on Tuesday 17 at 10:00Z:
    /// <list type="bullet">
    /// <item>"A", 20 min daily: 20 minutes on 9 to 11; Thursday 12 Missed (grace end Sunday 15, before this week); Friday 13
    /// and Saturday 14 Missed this week (grace ends Monday 16 and Tuesday 17 at 00:00Z); Sunday 15 Pending; Monday 16
    /// Pending with 12 minutes; Tuesday 17 Due;</item>
    /// <item>"B", 3 times a week: one "Yes" on Friday 13; the week of 9 November Pending, the week of 16 November Due;</item>
    /// <item>"C", yes/no on Monday, Wednesday and Friday: "Yes" on Monday 9 and Wednesday 11; Friday 13 Missed this week;
    /// Monday 16 Pending; nothing Due on Tuesday, so its Wednesday 18 is Upcoming.</item>
    /// </list>
    /// All three have a Pending result, so the goal order is A, B, C by name.
    /// </summary>
    private async Task<(GoalId A, GoalId B, GoalId C)> ThreeGoalsAsync()
    {
        var c = await _world.CreateNamedGoalAsync("C", YesNoDays(BuildOrReduce.Build, MonWedFri), Mon9, Mon9Morning);
        var b = await _world.CreateNamedGoalAsync("B", YesNoTimes(3, SchedulePeriod.Week), Mon9, Mon9Morning);
        var a = await _world.CreateNamedGoalAsync("A", Reading20MinDaily, Mon9, Mon9Morning);
        foreach (var day in new[] { 9, 10, 11 })
        {
            _world.SeedAmount(a, Date(2026, 11, day), 20);
        }

        _world.SeedAmount(a, Mon16, 12);
        _world.SeedAnswer(b, Fri13, YesNoAnswer.Yes);
        _world.SeedAnswer(c, Mon9, YesNoAnswer.Yes);
        _world.SeedAnswer(c, Date(2026, 11, 11), YesNoAnswer.Yes);
        _world.At(Tue17TenOClock);
        return (a, b, c);
    }

    [Fact]
    public async Task US6_AS1_the_counts_are_the_Due_Pending_and_Missed_this_week_results_and_the_goals()
    {
        await ThreeGoalsAsync();

        var dashboard = await _world.DashboardAsync();

        Assert.Equal(new DashboardCounts { Due = 2, Pending = 4, MissedThisWeek = 3, Goals = 3 }, dashboard.Counts);
        Assert.Equal(Tue17, dashboard.AccountToday);
        Assert.Equal(Utc("2026-11-16T00:00:00Z"), dashboard.WeekStarts);
        Assert.Equal(Tue17TenOClock, dashboard.ReadAt);
    }

    [Fact]
    public async Task US6_AS2_AS3_AS8_the_Today_list_is_Pending_then_Due_then_Missed_this_week_then_Upcoming()
    {
        var (a, b, c) = await ThreeGoalsAsync();

        var today = (await _world.DashboardAsync()).Today;

        Assert.Equal(
            [
                // Pending by grace end: Sunday 15 and the week of 9 November end at Wednesday 18 00:00Z (A before B), then
                // the two Monday 16s at Thursday 19 00:00Z (A before C).
                (a, Sun15, Sun15, EvaluationState.Pending), (b, Mon9, Sun15, EvaluationState.Pending),
                (a, Mon16, Mon16, EvaluationState.Pending), (c, Mon16, Mon16, EvaluationState.Pending),
                // Due in goal order, the week of 16 November among them.
                (a, Tue17, Tue17, EvaluationState.Due), (b, Mon16, Date(2026, 11, 22), EvaluationState.Due),
                // Missed this week, latest grace end first: Saturday 14 (Tuesday 17 00:00Z), then the two Friday 13s (Monday
                // 16 00:00Z, A before C). Thursday 12's grace period ended on Sunday 15, last week.
                (a, Sat14, Sat14, EvaluationState.Missed), (a, Fri13, Fri13, EvaluationState.Missed), (c, Fri13, Fri13, EvaluationState.Missed),
                // Upcoming: only C has no Due entry; its next commitment is Wednesday 18.
                (c, Wed18, Wed18, EvaluationState.Upcoming),
            ],
            today.Select(Row));
        Assert.Equal(
            [Utc("2026-11-18T00:00:00Z"), Utc("2026-11-18T00:00:00Z"), Utc("2026-11-19T00:00:00Z"), Utc("2026-11-19T00:00:00Z")],
            today.Take(4).Select(e => e.Entry.GraceEnds!.Value));
        Assert.All(today.Skip(6), e => Assert.Null(e.Entry.GraceEnds));
        Assert.Equal([false, true, true, false, false, false, false, false, false, false], today.Select(e => e.HasRecord));
        Assert.All(today, e => Assert.False(e.HasNote));
        Assert.Equal(new Progress(ProgressKind.RecordedValue, 12, 20), today[2].Entry.Progress);
        Assert.Equal(new Progress(ProgressKind.RecordsReachingAmount, 1, 3), today[1].Entry.Progress);
        Assert.Equal(["A", "B", "A", "C", "A", "B", "A", "A", "C", "C"], today.Select(e => e.GoalName));
        Assert.All(today, e => Assert.Equal(BuildOrReduce.Build, e.BuildOrReduce));
        Assert.Equal(["Every day", "Flexible · 3 times a week", "Mon · Wed · Fri"], new[] { today[0], today[1], today[3] }.Select(e => e.ScheduleText));
        Assert.Empty(today[^1].Entry.Records);
    }

    [Fact]
    public async Task US6_AS2_each_rows_Goal_is_what_can_be_recorded_now()
    {
        var (a, b, c) = await ThreeGoalsAsync();

        var today = (await _world.DashboardAsync()).Today;

        foreach (var goal in new[] { a, b, c })
        {
            var recordable = await _world.Recording.GetRecordableAsync(_world.User, goal, CancellationToken.None);
            Assert.All(today.Where(e => e.GoalId == goal), e => AssertSameHistory(recordable.History!, e.Goal));
        }
    }

    [Fact]
    public async Task US6_AS4_read_on_Monday_at_00_00_the_previous_Friday_is_Missed_this_week_and_Thursday_is_not()
    {
        var goal = await _world.CreateGoalAsync(Reading20MinDaily, Mon9, Mon9Morning);
        _world.At(Utc("2026-11-16T00:00:00Z"));

        var dashboard = await _world.DashboardAsync();

        Assert.Equal(1, dashboard.Counts.MissedThisWeek);
        var missed = Assert.Single(dashboard.Today, e => e.Entry.State == EvaluationState.Missed);
        Assert.Equal((goal, Fri13, Fri13, EvaluationState.Missed), Row(missed));
        Assert.Equal(Utc("2026-11-16T00:00:00Z"), dashboard.WeekStarts);
    }

    [Fact]
    public async Task US6_AS4_Kyiv_after_the_change_back_Fridays_grace_end_on_Sunday_at_23_00_is_not_this_week()
    {
        // Friday 23 October 2026 in Kyiv ends at 21:00Z and its grace period at 2026-10-25T21:00:00Z, Sunday 23:00 local
        // after the change back; the week starts on Monday 26 October at 00:00 local, 2026-10-25T22:00:00Z.
        var user = _world.UserIn("Europe/Kyiv");
        await _world.CreateGoalAsync(DurationDays(20, DurationUnit.Minutes, BuildOrReduce.Build, DayOfWeek.Friday),
            Date(2026, 10, 16), Utc("2026-10-16T08:00:00Z"), user);
        _world.At(Utc("2026-10-25T22:30:00Z"));

        var dashboard = await _world.DashboardAsync(user);

        Assert.Equal(Utc("2026-10-25T22:00:00Z"), dashboard.WeekStarts);
        Assert.Equal(Date(2026, 10, 26), dashboard.AccountToday);
        Assert.Equal(0, dashboard.Counts.MissedThisWeek);
        Assert.DoesNotContain(dashboard.Today, e => e.Entry.State == EvaluationState.Missed);
    }

    [Fact]
    public async Task US6_AS4_Kyiv_after_the_change_forward_Fridays_grace_end_on_Monday_at_01_00_is_this_week()
    {
        // Friday 26 March 2027 in Kyiv ends at 22:00Z and its grace period at 2027-03-28T22:00:00Z, Monday 01:00 local
        // after the change forward; the week starts on Monday 29 March at 00:00 local, 2027-03-28T21:00:00Z.
        var user = _world.UserIn("Europe/Kyiv");
        var goal = await _world.CreateGoalAsync(DurationDays(20, DurationUnit.Minutes, BuildOrReduce.Build, DayOfWeek.Friday),
            Date(2027, 3, 19), Utc("2027-03-19T08:00:00Z"), user);
        _world.At(Utc("2027-03-28T21:30:00Z"));

        var before = await _world.DashboardAsync(user);
        _world.At(Utc("2027-03-28T22:00:00Z"));
        var after = await _world.DashboardAsync(user);

        Assert.Equal(Utc("2027-03-28T21:00:00Z"), before.WeekStarts);
        Assert.Equal((1, 0), (before.Counts.Pending, before.Counts.MissedThisWeek));
        Assert.Equal((goal, Date(2027, 3, 26), Date(2027, 3, 26), EvaluationState.Pending), Row(before.Today[0]));
        Assert.Equal((0, 1), (after.Counts.Pending, after.Counts.MissedThisWeek));
        Assert.Equal((goal, Date(2027, 3, 26), Date(2027, 3, 26), EvaluationState.Missed), Row(after.Today[0]));
    }

    [Theory]
    [InlineData(null, "2026-11-16T00:00:00Z", 0)]
    [InlineData("Europe/Kyiv", "2026-11-15T22:00:00Z", 1)]
    public async Task US6_AS4_this_week_is_the_accounts_not_the_goals_older_version(string? accountZone, string weekStarts, int missedThisWeek)
    {
        // A daily goal kept in Kyiv from Monday 9 November, read on Monday 16 at 00:00Z (02:00 in Kyiv). Its Friday 13 ends
        // its grace period on Sunday 15 at 22:00Z: in this week for an account in Kyiv, last week for one in UTC.
        var user = accountZone is null ? _world.User : _world.UserIn(accountZone);
        _world.AddGoal(user, Target(Reading20MinDaily), (Mon9, Kyiv));
        _world.At(Utc("2026-11-16T00:00:00Z"));

        var dashboard = await _world.DashboardAsync(user);

        Assert.Equal(Utc(weekStarts), dashboard.WeekStarts);
        Assert.Equal(missedThisWeek, dashboard.Counts.MissedThisWeek);
    }

    [Fact]
    public async Task US6_AS8_Upcoming_entries_come_by_start_then_goal_order_within_6_days_and_are_not_counted()
    {
        // Read on Tuesday 17 November at 10:00Z. "Daily" is Due (no Upcoming entry); "Zed" starts on Saturday 21, "Soon"
        // and "Weekly" (per week) on Monday 23, 6 days after today; "Later" on Tuesday 24, 7 days after, so it has none.
        await _world.CreateNamedGoalAsync("Daily", Reading20MinDaily, Tue17, Utc("2026-11-17T08:00:00Z"));
        var weekly = await _world.CreateNamedGoalAsync("Weekly", YesNoTimes(2, SchedulePeriod.Week), Date(2026, 11, 23), Utc("2026-11-17T08:00:00Z"));
        var soon = await _world.CreateNamedGoalAsync("Soon", Reading20MinDaily, Date(2026, 11, 23), Utc("2026-11-17T08:00:00Z"));
        var zed = await _world.CreateNamedGoalAsync("Zed", Reading20MinDaily, Date(2026, 11, 21), Utc("2026-11-17T08:00:00Z"));
        await _world.CreateNamedGoalAsync("Later", Reading20MinDaily, Date(2026, 11, 24), Utc("2026-11-17T08:00:00Z"));
        _world.At(Tue17TenOClock);

        var dashboard = await _world.DashboardAsync();

        var upcoming = dashboard.Today.Where(e => e.Entry.State == EvaluationState.Upcoming).ToList();
        Assert.Equal(
            [
                (zed, Date(2026, 11, 21), Date(2026, 11, 21), EvaluationState.Upcoming),
                (soon, Date(2026, 11, 23), Date(2026, 11, 23), EvaluationState.Upcoming),
                (weekly, Date(2026, 11, 23), Date(2026, 11, 29), EvaluationState.Upcoming),
            ],
            upcoming.Select(Row));
        Assert.Equal(EvaluationState.Due, dashboard.Today[0].Entry.State);
        Assert.Equal(4, dashboard.Today.Count);
        Assert.Equal(new DashboardCounts { Due = 1, Pending = 0, MissedThisWeek = 0, Goals = 5 }, dashboard.Counts);
    }

    [Fact]
    public async Task US6_AS8_a_Mondays_goal_read_on_Tuesday_ends_the_list_with_next_Monday_6_days_later()
    {
        var goal = await _world.CreateGoalAsync(DurationDays(20, DurationUnit.Minutes, BuildOrReduce.Build, DayOfWeek.Monday), Mon16, Utc("2026-11-16T08:00:00Z"));
        _world.At(Tue17TenOClock);

        var today = (await _world.DashboardAsync()).Today;

        Assert.Equal(
            [(goal, Mon16, Mon16, EvaluationState.Pending), (goal, Date(2026, 11, 23), Date(2026, 11, 23), EvaluationState.Upcoming)],
            today.Select(Row));
    }

    [Fact]
    public async Task US6_AS5_every_goal_has_its_figures_in_goal_order()
    {
        var (a, b, c) = await ThreeGoalsAsync();
        var later = await _world.CreateNamedGoalAsync("Aardvark", Reading20MinDaily, Date(2026, 12, 1), Tue17TenOClock);

        var dashboard = await _world.DashboardAsync();

        Assert.Equal([a, b, c, later], dashboard.Goals.Select(g => g.GoalId));
        Assert.Equal(["A", "B", "C", "Aardvark"], dashboard.Goals.Select(g => g.Name));
        foreach (var goal in dashboard.Goals)
        {
            Assert.Equal((await _world.ReportAsync(goal.GoalId)).Stats, goal.Stats);
        }

        Assert.Equal("50% (3 of 6)", dashboard.Goals[0].Stats.ConsistencyText);
        Assert.Equal("N/A", dashboard.Goals[3].Stats.ConsistencyText);
    }

    [Fact]
    public async Task US6_AS6_the_5_notes_with_the_latest_dates_come_newest_first_ties_in_goal_order()
    {
        // "A" and "B", daily from Monday 9 November, read on Tuesday 17 at 10:00Z: both have Pending results, so A comes
        // before B. Notes on A's 9, 11, 13 and 15 and on B's 10, 13 and 14.
        var b = await _world.CreateNamedGoalAsync("B", Reading20MinDaily, Mon9, Mon9Morning);
        var a = await _world.CreateNamedGoalAsync("A", Reading20MinDaily, Mon9, Mon9Morning);
        foreach (var day in new[] { 9, 11, 13, 15 })
        {
            _world.SeedNote(a, Date(2026, 11, day), $"A on {day}");
        }

        foreach (var day in new[] { 10, 13, 14 })
        {
            _world.SeedNote(b, Date(2026, 11, day), $"B on {day}");
        }

        _world.At(Tue17TenOClock);

        var dashboard = await _world.DashboardAsync();

        Assert.Equal(
            [
                new DashboardNote { GoalId = a, GoalName = "A", FirstDate = Sun15, LastDate = Sun15, Note = "A on 15", State = EvaluationState.Pending },
                new DashboardNote { GoalId = b, GoalName = "B", FirstDate = Sat14, LastDate = Sat14, Note = "B on 14", State = EvaluationState.Missed },
                new DashboardNote { GoalId = a, GoalName = "A", FirstDate = Fri13, LastDate = Fri13, Note = "A on 13", State = EvaluationState.Missed },
                new DashboardNote { GoalId = b, GoalName = "B", FirstDate = Fri13, LastDate = Fri13, Note = "B on 13", State = EvaluationState.Missed },
                new DashboardNote { GoalId = a, GoalName = "A", FirstDate = Date(2026, 11, 11), LastDate = Date(2026, 11, 11), Note = "A on 11", State = EvaluationState.Missed },
            ],
            dashboard.Notes);
        Assert.True(dashboard.Today.Single(e => e.GoalId == a && e.Entry.FirstDate == Sun15).HasNote);
    }

    [Fact]
    public async Task A_weeks_note_carries_the_weeks_first_and_last_date()
    {
        var goal = await _world.CreateGoalAsync(YesNoTimes(3, SchedulePeriod.Week), Mon9, Mon9Morning);
        _world.SeedNote(goal, Mon9, "Busy week");
        _world.At(Tue17TenOClock);

        var note = Assert.Single((await _world.DashboardAsync()).Notes);

        Assert.Equal((Mon9, Sun15, "Busy week", EvaluationState.Pending), (note.FirstDate, note.LastDate, note.Note, note.State));
    }

    [Fact]
    public async Task Edge_case_a_goal_whose_catalog_habit_is_then_Unpublished_keeps_its_counts_rows_stats_and_notes()
    {
        var (a, _, _) = await ThreeGoalsAsync();
        _world.SeedNote(a, Sat14, "Away");
        var before = await _world.DashboardAsync();

        Assert.True(await _world.CatalogHabits.TrySetStateAsync(_world.Habit.Id, CatalogHabitState.Published, CatalogHabitState.Unpublished, CancellationToken.None));
        var after = await _world.DashboardAsync();

        Assert.Equal(before.Counts, after.Counts);
        Assert.Equal(before.Today.Select(Row), after.Today.Select(Row));
        Assert.Equal(before.Goals, after.Goals);
        Assert.Equal(before.Notes, after.Notes);
        Assert.Single(after.Notes);
    }

    [Fact]
    public async Task FR_013_a_user_with_no_goals_gets_counts_of_0_and_empty_lists()
    {
        _world.At(Tue17TenOClock);

        var dashboard = await _world.DashboardAsync();

        Assert.Equal(new DashboardCounts { Due = 0, Pending = 0, MissedThisWeek = 0, Goals = 0 }, dashboard.Counts);
        Assert.Empty(dashboard.Today);
        Assert.Empty(dashboard.Goals);
        Assert.Equal(0, dashboard.EndedGoalCount);
        Assert.Empty(dashboard.Notes);
    }

    [Fact]
    public async Task A_recorded_note_reaches_the_dashboard_through_RecordingService()
    {
        var goal = await _world.CreateGoalAsync(Reading20MinDaily, Mon16, Utc("2026-11-16T08:00:00Z"));
        _world.At(Tue17TenOClock);
        await _world.RecordAsync(goal, Tue17, RecordWorld.Minutes(5, "Short one"));

        var dashboard = await _world.DashboardAsync();

        var row = Assert.Single(dashboard.Today, e => e.Entry.State == EvaluationState.Due);
        Assert.True(row.HasRecord);
        Assert.True(row.HasNote);
        Assert.Equal("Short one", Assert.Single(dashboard.Notes).Note);
    }

    // ---- slice 7: excused and paused entries, paused goals (FR-019, FR-020, US4-AS2 to AS4; T074), over ExemptionWorld ----

    private static readonly DateOnly Mon2 = Date(2026, 11, 2);
    private static readonly DateOnly Fri20 = Date(2026, 11, 20);
    private static readonly DateOnly Sun22 = Date(2026, 11, 22);
    private static readonly DateTimeOffset Mon2Morning = Utc("2026-11-02T08:00:00Z");

    /// <summary>10:00Z on Monday 16 November: today is Monday 16; Saturday 14 is Pending until Tuesday 17 at 00:00Z, Sunday 15 until Wednesday 18 at 00:00Z.</summary>
    private static readonly DateTimeOffset Mon16TenOClock = Utc("2026-11-16T10:00:00Z");

    /// <summary>"Drinking water, 8 glasses a day": Build, count at least 8 "glasses", once per day (period-based).</summary>
    private static GoalTargetFields DrinkingWater => CountTimes(8, "glasses", 1, SchedulePeriod.Day);

    /// <summary>"Meditation, 10 min daily": Build, duration 10 minutes, all seven days (commitment-based).</summary>
    private static GoalTargetFields Meditation => DurationDays(10, DurationUnit.Minutes, BuildOrReduce.Build, AllDays);

    /// <summary>
    /// US4's account: "Drinking water, 8 glasses a day" and "Meditation, 10 min daily", both created on Monday 2 November
    /// at 08:00Z with that start date, both on target on each date from 2 to 13 November and not recorded from 14
    /// November on; "Drinking water" paused at 09:00Z on Monday 16 from Monday 16 until Sunday 22 November. The clock is
    /// left at 10:00Z on Monday 16.
    /// </summary>
    private static async Task<(GoalId Water, GoalId Meditation)> UserStoryFourAsync(ExemptionWorld world)
    {
        var water = await world.CreateNamedGoalAsync("Drinking water, 8 glasses a day", DrinkingWater, Mon2, Mon2Morning);
        var meditation = await world.CreateNamedGoalAsync("Meditation, 10 min daily", Meditation, Mon2, Mon2Morning);
        for (var date = Mon2; date <= Fri13; date = date.AddDays(1))
        {
            world.SeedAmount(water, date, 8);
            world.SeedAmount(meditation, date, 10);
        }

        world.At(Utc("2026-11-16T09:00:00Z"));
        Assert.Equal(PauseOutcome.Paused, (await world.PauseAsync(water, Mon16, Sun22)).Outcome);
        world.At(Mon16TenOClock);
        return (water, meditation);
    }

    [Fact]
    public async Task US4_AS2_a_paused_goal_has_no_Due_or_Upcoming_entry_keeps_its_Pending_ones_and_is_not_counted()
    {
        var world = new ExemptionWorld();
        var (water, meditation) = await UserStoryFourAsync(world);

        var dashboard = await world.DashboardAsync();

        Assert.Equal(
            [
                // Pending by grace end: Saturday 14 (Tuesday 17 00:00Z), then Sunday 15 (Wednesday 18 00:00Z), each in goal
                // order (both goals have Pending results, so by name: Drinking water, then Meditation).
                (water, Sat14, Sat14, EvaluationState.Pending), (meditation, Sat14, Sat14, EvaluationState.Pending),
                (water, Sun15, Sun15, EvaluationState.Pending), (meditation, Sun15, Sun15, EvaluationState.Pending),
                // Due: only Meditation's Monday 16; Drinking water's Monday 16 is Paused, and 17 to 22 November too, so it
                // has no Upcoming entry.
                (meditation, Mon16, Mon16, EvaluationState.Due),
            ],
            dashboard.Today.Select(Row));
        Assert.Equal(new DashboardCounts { Due = 1, Pending = 4, MissedThisWeek = 0, Goals = 1 }, dashboard.Counts);
        Assert.Equal([water, meditation], dashboard.Goals.Select(g => g.GoalId));
        Assert.Equal([Sun22, null], dashboard.Goals.Select(g => g.PausedUntil));
    }

    [Fact]
    public async Task US4_AS3_an_excused_commitment_is_neither_counted_nor_listed_as_Pending()
    {
        var world = new ExemptionWorld();
        var (water, meditation) = await UserStoryFourAsync(world);
        Assert.Equal(ExcuseOutcome.Excused, (await world.ExcuseAsync(meditation, Sun15)).Outcome);
        world.At(Utc("2026-11-16T10:01:00Z"));

        var dashboard = await world.DashboardAsync();

        Assert.Equal(3, dashboard.Counts.Pending);
        Assert.DoesNotContain(dashboard.Today, e => e.GoalId == meditation && e.Entry.FirstDate == Sun15);
        Assert.Contains(dashboard.Today, e => e.GoalId == meditation && e.Entry.FirstDate == Sat14 && e.Entry.State == EvaluationState.Pending);
        Assert.Contains(dashboard.Today, e => e.GoalId == water && e.Entry.FirstDate == Sun15 && e.Entry.State == EvaluationState.Pending);
    }

    [Fact]
    public async Task US4_AS4_the_Upcoming_entry_is_the_earliest_Upcoming_commitment_that_is_not_Paused()
    {
        // Another account's "Yoga, 3 days a week" (Monday, Wednesday and Friday) from Monday 2 November, with no records,
        // paused at 08:00Z on Monday 16 from Wednesday 18 until Wednesday 18; read on Tuesday 17 at 10:00Z (no commitment
        // that day, so no Due entry).
        var world = new ExemptionWorld();
        var yoga = await world.CreateNamedGoalAsync(
            "Yoga, 3 days a week", YesNoDays(BuildOrReduce.Build, MonWedFri), Mon2, Mon2Morning, world.OtherUser);
        world.At(Utc("2026-11-16T08:00:00Z"));
        Assert.Equal(PauseOutcome.Paused, (await world.PauseAsync(yoga, Wed18, Wed18, user: world.OtherUser)).Outcome);
        world.At(Tue17TenOClock);

        var dashboard = await world.DashboardAsync(world.OtherUser);

        Assert.DoesNotContain(dashboard.Today, e => e.Entry.State == EvaluationState.Due);
        var upcoming = Assert.Single(dashboard.Today, e => e.Entry.State == EvaluationState.Upcoming);
        Assert.Equal((yoga, Fri20, Fri20, EvaluationState.Upcoming), Row(upcoming));
        Assert.Null(Assert.Single(dashboard.Goals).PausedUntil);
        Assert.Equal(1, dashboard.Counts.Goals);
    }

    [Fact]
    public async Task FR_019_Missed_this_week_leaves_out_excused_and_paused_entries()
    {
        // "Meditation" from Monday 9 November with no records; Friday 13 excused and Saturday 14 paused, stored directly.
        // Read on Tuesday 17 at 10:00Z, Friday 13 (grace end Monday 16 00:00Z) and Saturday 14 (Tuesday 17 00:00Z) would be
        // Missed this week; Thursday 12's grace period ended on Sunday 15, last week.
        var world = new ExemptionWorld();
        var goal = await world.CreateGoalAsync(Meditation, Mon9, Mon9Morning);
        world.SeedExcusal(goal, Fri13);
        world.SeedPause(goal, Sat14, Sat14);
        world.At(Tue17TenOClock);

        var dashboard = await world.DashboardAsync();

        Assert.Equal(new DashboardCounts { Due = 1, Pending = 2, MissedThisWeek = 0, Goals = 1 }, dashboard.Counts);
        Assert.Equal(
            [(goal, Sun15, Sun15, EvaluationState.Pending), (goal, Mon16, Mon16, EvaluationState.Pending), (goal, Tue17, Tue17, EvaluationState.Due)],
            dashboard.Today.Select(Row));
    }

    [Fact]
    public async Task FR_020_a_note_on_a_Paused_or_Excused_entry_comes_with_that_state()
    {
        var world = new ExemptionWorld();
        var (water, meditation) = await UserStoryFourAsync(world);
        Assert.Equal(ExcuseOutcome.Excused, (await world.ExcuseAsync(meditation, Sun15)).Outcome);
        world.SeedNote(water, Mon16, "Visiting family");
        world.SeedNote(meditation, Sun15, "Too tired");

        var notes = (await world.DashboardAsync()).Notes;

        Assert.Equal(
            [(water, Mon16, "Visiting family", EvaluationState.Paused), (meditation, Sun15, "Too tired", EvaluationState.Excused)],
            notes.Select(n => (n.GoalId, n.FirstDate, n.Note, n.State)));
    }
}
