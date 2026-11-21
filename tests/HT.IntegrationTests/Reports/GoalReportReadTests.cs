using HT.Application;
using HT.Application.Accounts;
using HT.Application.Exemptions;
using HT.Application.Goals;
using HT.Application.Records;
using HT.Application.Reports;
using HT.Domain.Accounts;
using HT.Domain.Evaluation;
using HT.Domain.Exemptions;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using HT.Domain.Reports;
using HT.IntegrationTests.Habits;
using HT.Persistence;
using HT.UnitTests.Fakes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using static HT.IntegrationTests.Goals.GoalSamples;

namespace HT.IntegrationTests.Reports;

/// <summary>
/// Reading a goal's report on SQL Server (FR-007, FR-008, FR-014): <see cref="ReportService.GetGoalReportAsync"/> over
/// the real stores gives the same report as over <see cref="FakeRecordStore"/> with the same records, and never counts
/// the records or notes of another account's goal on the same habit. Each test builds its own provider with its own
/// <see cref="FakeTimeProvider"/> over the fixture's connection string and moves only that clock; the fixture's shared
/// clock is never moved. The users are in UTC. Shared database: own prefixes ("it-rep-", "It-Rep"); rows are read per
/// goal only.
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class GoalReportReadTests(DatabaseFixture database)
{
    /// <summary>Monday 2 November 2026, the start date of the goals.</summary>
    private static readonly DateOnly Mon2 = new(2026, 11, 2);

    /// <summary>08:00Z on Monday 2 November 2026, when the test's own clock starts and the goals are created.</summary>
    private static readonly DateTimeOffset ClockStart = new(2026, 11, 2, 8, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// 10:00Z on Wednesday 2 December 2026: Sunday 29 November is final (its grace period ended at 00:00Z), Monday 30
    /// November and Tuesday 1 December are Pending, Wednesday 2 December is Due.
    /// </summary>
    private static readonly DateTimeOffset ReadAt = new(2026, 12, 2, 10, 0, 0, TimeSpan.Zero);

    private static CancellationToken None => CancellationToken.None;

    private static string Suffix() => Guid.NewGuid().ToString("N")[..8];

    /// <summary>Build, duration, at least 20 min, every day: commitment-based.</summary>
    private static GoalTargetFields Reading20MinDaily => new()
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = 20, DurationUnit = DurationUnit.Minutes,
        ScheduleKind = ScheduleKind.FixedDays, Days = Enum.GetValues<DayOfWeek>().ToHashSet(),
    };

    /// <summary>
    /// The minutes recorded on day <paramref name="day"/> after the start date (0 to 29), or null for none: every 7th day
    /// from the 4th 10 minutes (Failed), every 7th from the 6th no record (Missed), every other 20 minutes.
    /// </summary>
    private static int? MinutesOn(int day) => (day % 7) switch
    {
        3 => 10,
        5 => null,
        _ => 20,
    };

    [Fact]
    public async Task FR_007_FR_008_a_report_over_the_real_stores_equals_the_report_over_the_fakes_with_the_same_records()
    {
        // 30 days recorded as the clock moves forward from 2 November. At ReadAt the 28 days to 29 November are final:
        // 20 Met, 4 Failed (days 3, 10, 17, 24) and 4 Missed (days 5, 12, 19, 26). The longest run is 4 commitments
        // (days 6 to 9), the current one 1 (day 27, after the Missed day 26; the Pending days 28 and 29 are skipped).
        var suffix = Suffix();
        var clock = new FakeTimeProvider(ClockStart);
        await using var services = BuildServices(clock);
        var user = await HabitAccounts.RegisteredUserAsync(services, database.ConnectionString, $"it-rep-read-{suffix}@example.com");
        var habit = await CatalogHabitAsync(services, $"It-Rep Reading {suffix}");
        var goal = await CreateGoalAsync(services, user, habit);

        var recorded = new List<(DateOnly Date, int Minutes)>();
        for (var day = 0; day < 30; day++)
        {
            clock.SetUtcNow(ClockStart.AddDays(day).AddHours(12));
            if (MinutesOn(day) is { } minutes)
            {
                var date = Mon2.AddDays(day);
                await RecordAsync(services, user, goal, date, new RecordInput { Amount = minutes });
                recorded.Add((date, minutes));
            }
        }

        clock.SetUtcNow(ReadAt);
        var real = await ReportAsync(services, user, goal);
        var fake = await FakeReportAsync(services, clock, user, goal, recorded);

        Assert.Equal("71% (20 of 28)", real.Stats.ConsistencyText);
        Assert.Equal(1, real.Stats.CurrentStreak);
        Assert.Equal(4, real.Stats.LongestStreak);
        Assert.Equal(30, real.Recent.Results.Count);
        Assert.Equal(2, real.Recent.Counts[EvaluationState.Pending]);
        Assert.Equal(ReadAt, real.ReadAt);
        AssertSameReport(fake, real);
    }

    [Fact]
    public async Task FR_014_the_records_and_notes_of_another_accounts_goal_on_the_same_habit_are_never_counted()
    {
        // Both users have a daily goal on the same catalog habit. The other user records 20 minutes with a note on every
        // day; the user only on the first three days. At 10:00Z on Sunday 8 November the first four days are final, so
        // the user's Thursday 5 is Missed.
        var suffix = Suffix();
        var clock = new FakeTimeProvider(ClockStart);
        await using var services = BuildServices(clock);
        var user = await HabitAccounts.RegisteredUserAsync(services, database.ConnectionString, $"it-rep-own-{suffix}@example.com");
        var other = await HabitAccounts.RegisteredUserAsync(services, database.ConnectionString, $"it-rep-other-{suffix}@example.com");
        var habit = await CatalogHabitAsync(services, $"It-Rep Shared {suffix}");
        var goal = await CreateGoalAsync(services, user, habit);
        var otherGoal = await CreateGoalAsync(services, other, habit);

        var recorded = new List<(DateOnly Date, int Minutes)>();
        for (var day = 0; day < 6; day++)
        {
            clock.SetUtcNow(ClockStart.AddDays(day).AddHours(12));
            var date = Mon2.AddDays(day);
            await RecordAsync(services, other, otherGoal, date, new RecordInput { Amount = 20, Note = $"It-Rep other note {day}" });
            if (day < 3)
            {
                await RecordAsync(services, user, goal, date, new RecordInput { Amount = 20 });
                recorded.Add((date, 20));
            }
        }

        clock.SetUtcNow(new DateTimeOffset(2026, 11, 8, 10, 0, 0, TimeSpan.Zero));
        var own = await ReportAsync(services, user, goal);
        var otherReport = await ReportAsync(services, other, otherGoal);
        var fake = await FakeReportAsync(services, clock, user, goal, recorded);

        Assert.Equal("75% (3 of 4)", own.Stats.ConsistencyText);
        Assert.Equal("100% (4 of 4)", otherReport.Stats.ConsistencyText);
        AssertSameReport(fake, own);
        await using var scope = services.CreateAsyncScope();
        var refused = await scope.ServiceProvider.GetRequiredService<ReportService>().GetGoalReportAsync(other, goal, None);
        Assert.Equal(GoalReportOutcome.NotFound, refused.Outcome);
        Assert.Equal("This goal does not exist.", refused.Message);
        Assert.Null(refused.Report);
    }

    [Fact]
    public async Task FR_008_SC_004_a_Missed_day_corrected_through_RecordAsync_changes_the_next_reports_consistency_and_streaks()
    {
        // 20 minutes on 2, 3 and 4 November and on 6 and 7, recorded at 12:00Z each day; nothing on Thursday 5. At 10:00Z
        // on Tuesday 10 November Saturday 7 is final (its grace period ended at 00:00Z), so Thursday 5 is Missed: Met × 3,
        // Missed, Met × 2. Then 20 minutes are recorded for Thursday 5 at that time, a correction.
        var suffix = Suffix();
        var clock = new FakeTimeProvider(ClockStart);
        await using var services = BuildServices(clock);
        var user = await HabitAccounts.RegisteredUserAsync(services, database.ConnectionString, $"it-rep-fix-{suffix}@example.com");
        var habit = await CatalogHabitAsync(services, $"It-Rep Correction {suffix}");
        var goal = await CreateGoalAsync(services, user, habit);
        var thursday5 = Mon2.AddDays(3);

        var recorded = new List<(DateOnly Date, int Minutes)>();
        for (var day = 0; day < 6; day++)
        {
            clock.SetUtcNow(ClockStart.AddDays(day).AddHours(4));
            var date = Mon2.AddDays(day);
            if (date != thursday5)
            {
                await RecordAsync(services, user, goal, date, new RecordInput { Amount = 20 });
                recorded.Add((date, 20));
            }
        }

        clock.SetUtcNow(new DateTimeOffset(2026, 11, 10, 10, 0, 0, TimeSpan.Zero));
        var before = await ReportAsync(services, user, goal);

        Assert.Equal(EvaluationState.Missed, before.Recent.Results.Single(result => result.FirstDate == thursday5).State);
        Assert.Equal("83% (5 of 6)", before.Stats.ConsistencyText);
        Assert.Equal((2, 3), (before.Stats.CurrentStreak, before.Stats.LongestStreak));

        await RecordAsync(services, user, goal, thursday5, new RecordInput { Amount = 20 });
        recorded.Add((thursday5, 20));
        var after = await ReportAsync(services, user, goal);
        var onTime = await FakeReportAsync(services, clock, user, goal, recorded.OrderBy(record => record.Date).ToList());

        Assert.Equal(EvaluationState.Met, after.Recent.Results.Single(result => result.FirstDate == thursday5).State);
        Assert.Equal("100% (6 of 6)", after.Stats.ConsistencyText);
        Assert.Equal((6, 6), (after.Stats.CurrentStreak, after.Stats.LongestStreak));
        AssertSameReport(onTime, after);
    }

    [Fact]
    public async Task FR_024_a_report_with_an_excusal_and_a_pause_made_through_the_services_equals_the_report_over_the_fakes()
    {
        // Slice 7 US3's facts on "Reading, 20 min daily": 20 minutes recorded at 12:00Z on each date from 2 to 16 November
        // except 9, 11 and 13 to 15; Monday 9 excused on Tuesday 10 at 08:00Z; the goal paused on Friday 13 at 08:00Z
        // from that day until Sunday 15. Read on Thursday 19 November at 12:00Z: 2 to 16 November are final.
        var suffix = Suffix();
        var clock = new FakeTimeProvider(ClockStart);
        await using var services = BuildServices(clock);
        var user = await HabitAccounts.RegisteredUserAsync(services, database.ConnectionString, $"it-rep-exempt-{suffix}@example.com");
        var habit = await CatalogHabitAsync(services, $"It-Rep Exempt {suffix}");
        var goal = await CreateGoalAsync(services, user, habit);
        var mon9 = Mon2.AddDays(7);
        var fri13 = Mon2.AddDays(11);
        var sun15 = Mon2.AddDays(13);

        var recorded = new List<(DateOnly Date, int Minutes)>();
        for (var day = 0; day < 15; day++)
        {
            var date = Mon2.AddDays(day);
            if (day == 8)
            {
                clock.SetUtcNow(ClockStart.AddDays(day));
                await ExcuseAsync(services, user, goal, mon9);
            }

            if (day == 11)
            {
                clock.SetUtcNow(ClockStart.AddDays(day));
                await PauseAsync(services, user, goal, fri13, sun15);
            }

            clock.SetUtcNow(ClockStart.AddDays(day).AddHours(4));
            if (date != mon9 && date != Mon2.AddDays(9) && (date < fri13 || date > sun15))
            {
                await RecordAsync(services, user, goal, date, new RecordInput { Amount = 20 });
                recorded.Add((date, 20));
            }
        }

        clock.SetUtcNow(new DateTimeOffset(2026, 11, 19, 12, 0, 0, TimeSpan.Zero));
        var real = await ReportAsync(services, user, goal);
        var fake = await FakeReportAsync(services, clock, user, goal, recorded, [mon9], [(fri13, sun15)]);

        Assert.Equal("90% (10 of 11)", real.Stats.ConsistencyText);
        Assert.Equal((2, 8), (real.Stats.CurrentStreak, real.Stats.LongestStreak));
        Assert.Equal((1, 3), (real.Recent.Counts[EvaluationState.Excused], real.Recent.Counts[EvaluationState.Paused]));
        Assert.Equal("Minutes per day, last 14 days; target 20; 1 excused, 3 paused", real.Trend.Label);
        AssertSameReport(fake, real);
    }

    [Fact]
    public async Task FR_024_a_removed_excusal_changes_the_next_report()
    {
        // Two goals with 20 minutes recorded at 12:00Z on Monday 2, Tuesday 3 and Thursday 5 November, nothing on
        // Wednesday 4. Wednesday 4 of both is excused on Friday 6 at 10:00Z (Pending; its grace period ends on Saturday 7
        // at 00:00Z); the excusal of the second goal is removed at 23:00Z that day. Read on Saturday 7 November at
        // 10:00Z: 2 to 4 November are final, so the second goal's Wednesday 4 is Missed.
        var suffix = Suffix();
        var clock = new FakeTimeProvider(ClockStart);
        await using var services = BuildServices(clock);
        var user = await HabitAccounts.RegisteredUserAsync(services, database.ConnectionString, $"it-rep-unexcuse-{suffix}@example.com");
        var habit = await CatalogHabitAsync(services, $"It-Rep Unexcuse {suffix}");
        var kept = await CreateGoalAsync(services, user, habit);
        var removed = await CreateGoalAsync(services, user, habit);
        var wed4 = Mon2.AddDays(2);

        var recorded = new List<(DateOnly Date, int Minutes)>();
        foreach (var day in new[] { 0, 1, 3 })
        {
            clock.SetUtcNow(ClockStart.AddDays(day).AddHours(4));
            var date = Mon2.AddDays(day);
            await RecordAsync(services, user, kept, date, new RecordInput { Amount = 20 });
            await RecordAsync(services, user, removed, date, new RecordInput { Amount = 20 });
            recorded.Add((date, 20));
        }

        clock.SetUtcNow(new DateTimeOffset(2026, 11, 6, 10, 0, 0, TimeSpan.Zero));
        await ExcuseAsync(services, user, kept, wed4);
        await ExcuseAsync(services, user, removed, wed4);
        var excused = await ReportAsync(services, user, removed);
        clock.SetUtcNow(new DateTimeOffset(2026, 11, 6, 23, 0, 0, TimeSpan.Zero));
        await using (var scope = services.CreateAsyncScope())
        {
            var result = await scope.ServiceProvider.GetRequiredService<ExcusalService>().RemoveExcusalAsync(user, removed, wed4, None);
            Assert.Equal(RemoveExcusalOutcome.Removed, result.Outcome);
        }

        clock.SetUtcNow(new DateTimeOffset(2026, 11, 7, 10, 0, 0, TimeSpan.Zero));
        var keptReport = await ReportAsync(services, user, kept);
        var removedReport = await ReportAsync(services, user, removed);

        Assert.Equal(EvaluationState.Excused, excused.Recent.Results.Single(result => result.FirstDate == wed4).State);
        Assert.Equal("100% (2 of 2)", keptReport.Stats.ConsistencyText);
        Assert.Equal(EvaluationState.Excused, keptReport.Recent.Results.Single(result => result.FirstDate == wed4).State);
        Assert.Equal("66% (2 of 3)", removedReport.Stats.ConsistencyText);
        Assert.Equal(EvaluationState.Missed, removedReport.Recent.Results.Single(result => result.FirstDate == wed4).State);
        Assert.False(removedReport.Recent.Counts.ContainsKey(EvaluationState.Excused));
        AssertSameReport(await FakeReportAsync(services, clock, user, kept, recorded, [wed4]), keptReport);
        AssertSameReport(await FakeReportAsync(services, clock, user, removed, recorded), removedReport);
    }

    [Fact]
    public async Task FR_024_a_cancelled_pause_changes_the_next_report()
    {
        // "Yoga, 3 times a week" (yes/no, per week): "Yes" recorded at 12:00Z on Tuesday 3, Wednesday 4 and Thursday 5
        // November. On Monday 9 November at 10:00Z the goal is paused from Thursday 12 until Friday 13, an upcoming
        // pause, so the week of 9 November is Paused and its bar marked. The pause is then cancelled at that time.
        var suffix = Suffix();
        var clock = new FakeTimeProvider(ClockStart);
        await using var services = BuildServices(clock);
        var user = await HabitAccounts.RegisteredUserAsync(services, database.ConnectionString, $"it-rep-cancel-{suffix}@example.com");
        var habit = await CatalogHabitAsync(services, $"It-Rep Cancel {suffix}");
        var target = new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo, ScheduleKind = ScheduleKind.Times,
            Times = 3, Period = SchedulePeriod.Week,
        };
        var goal = await CreateGoalAsync(services, user, habit, target);
        var recorded = new List<DateOnly>();
        foreach (var day in new[] { 1, 2, 3 })
        {
            clock.SetUtcNow(ClockStart.AddDays(day).AddHours(4));
            await RecordAsync(services, user, goal, Mon2.AddDays(day), new RecordInput { Answer = YesNoAnswer.Yes });
            recorded.Add(Mon2.AddDays(day));
        }

        clock.SetUtcNow(new DateTimeOffset(2026, 11, 9, 10, 0, 0, TimeSpan.Zero));
        var thu12 = Mon2.AddDays(10);
        var fri13 = Mon2.AddDays(11);
        var pause = await PauseAsync(services, user, goal, thu12, fri13);
        var paused = await ReportAsync(services, user, goal);
        var pausedOnFakes = await FakeReportAsync(services, clock, user, goal, [], paused: [(thu12, fri13)], answers: recorded);
        await using (var scope = services.CreateAsyncScope())
        {
            var result = await scope.ServiceProvider.GetRequiredService<PauseService>().CancelPauseAsync(user, goal, pause, None);
            Assert.Equal(CancelPauseOutcome.Cancelled, result.Outcome);
        }

        var cancelled = await ReportAsync(services, user, goal);

        Assert.Equal(TrendMark.Paused, paused.Trend.Bars[^1].Mark);
        Assert.Equal("Times per week, last 12 weeks; target 3; 1 paused", paused.Trend.Label);
        Assert.Null(cancelled.Trend.Bars[^1].Mark);
        Assert.Equal("Times per week, last 12 weeks; target 3", cancelled.Trend.Label);
        AssertSameReport(pausedOnFakes, paused);
        AssertSameReport(await FakeReportAsync(services, clock, user, goal, [], answers: recorded), cancelled);
    }

    private static async Task<GoalId> CreateGoalAsync(IServiceProvider services, ActingUser user, HabitId habit, GoalTargetFields? target = null)
    {
        await using var scope = services.CreateAsyncScope();
        var created = await scope.ServiceProvider.GetRequiredService<GoalService>().CreateAsync(
            user, new GoalInput { Habit = new GoalHabit(habit, HabitKind.Catalog), Target = target ?? Reading20MinDaily, StartDate = Mon2 }, None);
        Assert.Equal(GoalCreateOutcome.Created, created.Outcome);
        return created.GoalId!.Value;
    }

    private static async Task ExcuseAsync(IServiceProvider services, ActingUser user, GoalId goal, DateOnly date)
    {
        await using var scope = services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<ExcusalService>().ExcuseAsync(user, goal, date, null, None);
        Assert.Equal(ExcuseOutcome.Excused, result.Outcome);
    }

    private static async Task<PauseId> PauseAsync(IServiceProvider services, ActingUser user, GoalId goal, DateOnly firstDay, DateOnly lastDay)
    {
        await using var scope = services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<PauseService>().PauseAsync(user, goal, firstDay, lastDay, null, None);
        Assert.Equal(PauseOutcome.Paused, result.Outcome);
        return result.Pause!.Id;
    }

    private static async Task RecordAsync(IServiceProvider services, ActingUser user, GoalId goal, DateOnly date, RecordInput input)
    {
        await using var scope = services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<RecordingService>().RecordAsync(user, goal, date, input, None);
        Assert.Equal(RecordWriteOutcome.Recorded, result.Outcome);
    }

    private static async Task<GoalReport> ReportAsync(IServiceProvider services, ActingUser user, GoalId goal)
    {
        await using var scope = services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<ReportService>().GetGoalReportAsync(user, goal, None);
        Assert.Equal(GoalReportOutcome.Found, result.Outcome);
        return result.Report!;
    }

    /// <summary>
    /// The report at the clock's time through a <see cref="ReportService"/> over a <see cref="FakeRecordStore"/> seeded
    /// with <paramref name="recorded"/> (and the "Yes" answers of <paramref name="answers"/>), the excusals of
    /// <paramref name="excused"/> and the pauses of <paramref name="paused"/>, and the real goal and account stores.
    /// </summary>
    private static async Task<GoalReport> FakeReportAsync(
        IServiceProvider services,
        TimeProvider clock,
        ActingUser user,
        GoalId goal,
        IReadOnlyList<(DateOnly Date, int Minutes)> recorded,
        IReadOnlyList<DateOnly>? excused = null,
        IReadOnlyList<(DateOnly FirstDay, DateOnly LastDay)>? paused = null,
        IReadOnlyList<DateOnly>? answers = null)
    {
        await using var scope = services.CreateAsyncScope();
        var goals = scope.ServiceProvider.GetRequiredService<IGoalStore>();
        var owner = user.AccountId!.Value;
        var stored = await goals.FindAsync(owner, goal, None);
        var fakeGoals = new FakeGoalStore(new FakeAccountStore(), new FakeCatalogHabitStore(), new FakeCustomHabitStore());
        fakeGoals.Add(owner, stored!.Goal, stored.Habit);
        var fakeRecords = new FakeRecordStore(fakeGoals);
        foreach (var (date, minutes) in recorded)
        {
            fakeRecords.Seed(goal, date, RecordValue.FromAmount(minutes));
        }

        foreach (var date in answers ?? [])
        {
            fakeRecords.Seed(goal, date, RecordValue.FromAnswer(YesNoAnswer.Yes));
        }

        foreach (var date in excused ?? [])
        {
            fakeRecords.SeedExcusal(goal, date);
        }

        foreach (var (firstDay, lastDay) in paused ?? [])
        {
            fakeRecords.SeedPause(goal, firstDay, lastDay);
        }

        var service = new ReportService(
            goals, fakeRecords, scope.ServiceProvider.GetRequiredService<IAccountStore>(), clock,
            scope.ServiceProvider.GetRequiredService<InstallationTimeZone>());
        var result = await service.GetGoalReportAsync(user, goal, None);
        Assert.Equal(GoalReportOutcome.Found, result.Outcome);
        Assert.Equal([nameof(IRecordStore.ReadExemptionsAsync), nameof(IRecordStore.ReadAsync)], fakeRecords.Calls);
        return result.Report!;
    }

    /// <summary>Every part of the two reports is equal; the lists and the dictionary are compared by their items.</summary>
    private static void AssertSameReport(GoalReport expected, GoalReport actual)
    {
        Assert.Equal(expected.GoalId, actual.GoalId);
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.BuildOrReduce, actual.BuildOrReduce);
        Assert.Equal(expected.ScheduleKind, actual.ScheduleKind);
        Assert.Equal(expected.Period, actual.Period);
        Assert.Equal(expected.Stats, actual.Stats);
        Assert.Equal(expected.Recent.Results, actual.Recent.Results);
        Assert.Equal(expected.Recent.Counts, actual.Recent.Counts);
        Assert.Equal(expected.Trend.Bars, actual.Trend.Bars);
        Assert.Equal(expected.Trend with { Bars = actual.Trend.Bars }, actual.Trend);
        Assert.Equal(expected.HasBegunEntry, actual.HasBegunEntry);
        Assert.Equal(expected.ReadAt, actual.ReadAt);
    }

    /// <summary>The application as the integration tests build it, with the test's own clock.</summary>
    private ServiceProvider BuildServices(FakeTimeProvider clock)
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddHtApplication()
            .AddHtPersistence(database.ConnectionString);
        services.AddSingleton<TimeProvider>(clock);
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
    }
}
