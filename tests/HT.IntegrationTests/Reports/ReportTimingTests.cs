using System.Diagnostics;
using System.Globalization;
using HT.Application;
using HT.Application.Goals;
using HT.Application.Records;
using HT.Application.Reports;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.IntegrationTests.Habits;
using HT.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Xunit.Abstractions;
using static HT.IntegrationTests.Goals.GoalSamples;

namespace HT.IntegrationTests.Reports;

/// <summary>
/// How long the reports take on SQL Server (SC-007, research R-08): a user with three goals, all created at the test
/// clock's start, one of them daily with 366 daily records added as the clock moves forward a year; the goal report,
/// the goals overview and the dashboard of that user are each returned in under 2 seconds, measured with a
/// <see cref="Stopwatch"/> around the call only. The test builds its own provider with its own
/// <see cref="FakeTimeProvider"/> over the fixture's connection string and moves only that clock; the fixture's shared
/// clock is never moved. The user is in UTC. Shared database: own prefixes ("it-rtm-", "It-Rtm"). A failure is reported
/// as it is, never retried.
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class ReportTimingTests(DatabaseFixture database, ITestOutputHelper output)
{
    /// <summary>The longest any of the three reads may take (SC-007).</summary>
    private static readonly TimeSpan Limit = TimeSpan.FromSeconds(2);

    /// <summary>Monday 3 November 2025, the start date of the goals.</summary>
    private static readonly DateOnly Mon3 = new(2025, 11, 3);

    /// <summary>08:00Z on Monday 3 November 2025, when the test's own clock starts and the goals are created.</summary>
    private static readonly DateTimeOffset ClockStart = new(2025, 11, 3, 8, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// 10:00Z on Thursday 5 November 2026, 367 days after the start date: the 366 recorded days (3 November 2025 to
    /// 3 November 2026) are all ended, the first 365 final, Tuesday 3 November 2026 still Pending.
    /// </summary>
    private static readonly DateTimeOffset ReadAt = new(2026, 11, 5, 10, 0, 0, TimeSpan.Zero);

    private const int RecordedDays = 366;

    private static CancellationToken None => CancellationToken.None;

    private static string Suffix() => Guid.NewGuid().ToString("N")[..8];

    /// <summary>Build, duration, at least 20 min, every day: commitment-based, the goal with a year of records.</summary>
    private static GoalTargetFields Reading20MinDaily => new()
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = 20, DurationUnit = DurationUnit.Minutes,
        ScheduleKind = ScheduleKind.FixedDays, Days = Enum.GetValues<DayOfWeek>().ToHashSet(),
    };

    /// <summary>Build, yes/no, 3 times a week: period-based, never recorded.</summary>
    private static GoalTargetFields Yoga3TimesAWeek => new()
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo, ScheduleKind = ScheduleKind.Times, Times = 3,
        Period = SchedulePeriod.Week,
    };

    /// <summary>Reduce, count, at most 2 coffees every day: commitment-based, never recorded.</summary>
    private static GoalTargetFields Coffee2ADay => new()
    {
        BuildOrReduce = BuildOrReduce.Reduce, Measurement = Measurement.Count, Amount = 2, CountUnit = "coffees",
        ScheduleKind = ScheduleKind.FixedDays, Days = Enum.GetValues<DayOfWeek>().ToHashSet(),
    };

    /// <summary>The minutes recorded on day <paramref name="day"/> after the start date: every 7th day 10 (Failed), every other 20.</summary>
    private static int MinutesOn(int day) => day % 7 == 3 ? 10 : 20;

    [Fact]
    public async Task SC_007_the_goal_report_the_goals_overview_and_the_dashboard_each_return_in_under_2_seconds_with_a_year_of_daily_records()
    {
        var suffix = Suffix();
        var clock = new FakeTimeProvider(ClockStart);
        await using var services = BuildServices(clock);
        var user = await HabitAccounts.RegisteredUserAsync(services, database.ConnectionString, $"it-rtm-{suffix}@example.com");
        var daily = await CreateGoalAsync(services, user, await CatalogHabitAsync(services, $"It-Rtm Reading {suffix}"), Reading20MinDaily);
        await CreateGoalAsync(services, user, await CatalogHabitAsync(services, $"It-Rtm Yoga {suffix}"), Yoga3TimesAWeek);
        await CreateGoalAsync(services, user, await CatalogHabitAsync(services, $"It-Rtm Coffee {suffix}"), Coffee2ADay);

        for (var day = 0; day < RecordedDays; day++)
        {
            clock.SetUtcNow(ClockStart.AddDays(day).AddHours(4));
            await RecordAsync(services, user, daily, Mon3.AddDays(day), new RecordInput { Amount = MinutesOn(day) });
        }

        clock.SetUtcNow(ReadAt);

        var (report, reportTime) = await TimeAsync(services, service => service.GetGoalReportAsync(user, daily, None));
        var (overview, overviewTime) = await TimeAsync(services, service => service.GetGoalsOverviewAsync(user, None));
        var (dashboard, dashboardTime) = await TimeAsync(services, service => service.GetDashboardAsync(user, None));
        output.WriteLine(string.Create(
            CultureInfo.InvariantCulture,
            $"Goal report {reportTime.TotalMilliseconds:0} ms, goals overview {overviewTime.TotalMilliseconds:0} ms, dashboard {dashboardTime.TotalMilliseconds:0} ms"));

        // The reads did the work: 365 final days, every 7th from the 4th Failed (52 of them), the rest Met.
        Assert.Equal(GoalReportOutcome.Found, report.Outcome);
        Assert.Equal("85% (313 of 365)", report.Report!.Stats.ConsistencyText);
        Assert.Equal(GoalsOverviewOutcome.Found, overview.Outcome);
        Assert.Equal(3, overview.Overview!.Goals.Count);
        Assert.Equal(DashboardOutcome.Found, dashboard.Outcome);
        Assert.Equal(3, dashboard.Dashboard!.Counts.Goals);

        Assert.True(reportTime < Limit, $"The goal report took {reportTime.TotalMilliseconds:0} ms.");
        Assert.True(overviewTime < Limit, $"The goals overview took {overviewTime.TotalMilliseconds:0} ms.");
        Assert.True(dashboardTime < Limit, $"The dashboard took {dashboardTime.TotalMilliseconds:0} ms.");
    }

    /// <summary>Runs one read in its own scope and measures only the call.</summary>
    private static async Task<(T Result, TimeSpan Elapsed)> TimeAsync<T>(IServiceProvider services, Func<ReportService, Task<T>> read)
    {
        await using var scope = services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<ReportService>();
        var stopwatch = Stopwatch.StartNew();
        var result = await read(service);
        stopwatch.Stop();
        return (result, stopwatch.Elapsed);
    }

    private static async Task<GoalId> CreateGoalAsync(IServiceProvider services, ActingUser user, HabitId habit, GoalTargetFields target)
    {
        await using var scope = services.CreateAsyncScope();
        var created = await scope.ServiceProvider.GetRequiredService<GoalService>().CreateAsync(
            user, new GoalInput { Habit = new GoalHabit(habit, HabitKind.Catalog), Target = target, StartDate = Mon3 }, None);
        Assert.Equal(GoalCreateOutcome.Created, created.Outcome);
        return created.GoalId!.Value;
    }

    private static async Task RecordAsync(IServiceProvider services, ActingUser user, GoalId goal, DateOnly date, RecordInput input)
    {
        await using var scope = services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<RecordingService>().RecordAsync(user, goal, date, input, None);
        Assert.Equal(RecordWriteOutcome.Recorded, result.Outcome);
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
