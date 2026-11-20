using HT.Application;
using HT.Application.Accounts;
using HT.Application.Goals;
using HT.Application.Records;
using HT.Application.Reports;
using HT.Domain.Accounts;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
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

    private static async Task<GoalId> CreateGoalAsync(IServiceProvider services, ActingUser user, HabitId habit)
    {
        await using var scope = services.CreateAsyncScope();
        var created = await scope.ServiceProvider.GetRequiredService<GoalService>().CreateAsync(
            user, new GoalInput { Habit = new GoalHabit(habit, HabitKind.Catalog), Target = Reading20MinDaily, StartDate = Mon2 }, None);
        Assert.Equal(GoalCreateOutcome.Created, created.Outcome);
        return created.GoalId!.Value;
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
    /// with <paramref name="recorded"/>, and the real goal and account stores.
    /// </summary>
    private static async Task<GoalReport> FakeReportAsync(
        IServiceProvider services, TimeProvider clock, ActingUser user, GoalId goal, IReadOnlyList<(DateOnly Date, int Minutes)> recorded)
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

        var service = new ReportService(
            goals, fakeRecords, scope.ServiceProvider.GetRequiredService<IAccountStore>(), clock,
            scope.ServiceProvider.GetRequiredService<InstallationTimeZone>());
        var result = await service.GetGoalReportAsync(user, goal, None);
        Assert.Equal(GoalReportOutcome.Found, result.Outcome);
        Assert.Equal([nameof(IRecordStore.ReadAsync)], fakeRecords.Calls);
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
