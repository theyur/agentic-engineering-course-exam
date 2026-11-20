using HT.Application;
using HT.Application.Accounts;
using HT.Application.Operator;
using HT.Application.Reports;
using HT.Domain.Accounts;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace HT.IntegrationTests.Reports;

/// <summary>
/// Slice 6 US8-AS1 to AS5, AS8, AS11 and AS12, FR-030 to FR-032, on SQL Server: demo loading, demo history loading and
/// demo report loading run at 12:00 UTC on Monday 16, Tuesday 17 or Wednesday 18 November 2026 (default zone UTC, so R
/// is that day), and the demo accounts read their reports and dashboards at that time through
/// <see cref="ReportService"/>. Each day is its own test: it resets <c>HT-IT-SEED</c> with
/// <see cref="HtDatabase.ResetAsync"/> and builds its own provider with its own <see cref="FakeTimeProvider"/>, so
/// <see cref="SeedDatabaseFixture.Clock"/> stays at 14 November for the other tests of the collection.
/// </summary>
[Collection(SeedDatabaseCollection.Name)]
public sealed class DemoReportsTests(SeedDatabaseFixture database)
{
    private const string DemoPassword = "example2026";
    private const string Reading = "Reading, 20 min daily";
    private const string Sugar = "Sugar, at most 1 sweet a day";
    private const string Yoga = "Yoga, 3 times a week";
    private const string Walking = "Walking, 30 min, 3 days a week";
    private const string Strength = "Strength training, 2 times a week";
    private const string ScreenTime = "Screen time, at most 120 min a day";

    private static CancellationToken None => CancellationToken.None;

    [Fact]
    public async Task US8_AS3_AS8_AS11_AS12_a_run_on_Wednesday_18_November()
    {
        var runDate = new DateOnly(2026, 11, 18);
        await using var services = await LoadAllAsync(new DateTimeOffset(2026, 11, 18, 12, 0, 0, TimeSpan.Zero));

        await AssertDemo4Async(services, runDate, ("33% (1 of 3)", 1, 1));
        await AssertDemo1To3Async(services);

        var demo5 = await SignInAsync(services, DemoReportService.DemoEmail);
        var goals = await GoalsByNameAsync(services, demo5);
        Assert.Equal([ScreenTime, Strength, Walking], goals.Keys.Order(StringComparer.Ordinal));
        var screen = await ReportAsync(services, demo5, goals[ScreenTime]);
        var strength = await ReportAsync(services, demo5, goals[Strength]);
        var walking = await ReportAsync(services, demo5, goals[Walking]);
        AssertStats(screen, "93% (77 of 82)", 0, 34);
        AssertStats(strength, "81% (9 of 11)", 3, 4);
        AssertStats(walking, "82% (29 of 35)", 7, 15);
        Assert.Equal([14, 12, 14], new[] { walking, strength, screen }.Select(r => r.Trend.Bars.Count));

        var dashboard = await DashboardAsync(services, demo5);
        Assert.Equal(new DashboardCounts { Due = 3, Pending = 3, MissedThisWeek = 1, Goals = 3 }, dashboard.Counts);
        Assert.Equal(
            [
                (ScreenTime, new DateOnly(2026, 11, 16), new DateOnly(2026, 11, 16), EvaluationState.Pending),
                (Walking, new DateOnly(2026, 11, 16), new DateOnly(2026, 11, 16), EvaluationState.Pending),
                (ScreenTime, new DateOnly(2026, 11, 17), new DateOnly(2026, 11, 17), EvaluationState.Pending),
                (ScreenTime, runDate, runDate, EvaluationState.Due),
                (Walking, runDate, runDate, EvaluationState.Due),
                (Strength, new DateOnly(2026, 11, 16), new DateOnly(2026, 11, 22), EvaluationState.Due),
                (ScreenTime, new DateOnly(2026, 11, 15), new DateOnly(2026, 11, 15), EvaluationState.Missed),
            ],
            dashboard.Today.Select(Row));
    }

    [Fact]
    public async Task US8_AS3_AS11_a_run_on_Tuesday_17_November()
    {
        var runDate = new DateOnly(2026, 11, 17);
        await using var services = await LoadAllAsync(new DateTimeOffset(2026, 11, 17, 12, 0, 0, TimeSpan.Zero));

        await AssertDemo4Async(services, runDate, ("0% (0 of 2)", 0, 0));
        await AssertDemo1To3Async(services);
        await AssertDemo5StrengthAndMissedThisWeekAsync(services, runDate, ("80% (8 of 10)", 2, 4));
    }

    [Fact]
    public async Task US8_AS3_AS11_a_run_on_Monday_16_November()
    {
        var runDate = new DateOnly(2026, 11, 16);
        await using var services = await LoadAllAsync(new DateTimeOffset(2026, 11, 16, 12, 0, 0, TimeSpan.Zero));

        await AssertDemo4Async(services, runDate, ("33% (1 of 3)", 0, 1));
        await AssertDemo1To3Async(services);
        await AssertDemo5StrengthAndMissedThisWeekAsync(services, runDate, ("81% (9 of 11)", 2, 5));
    }

    /// <summary>
    /// US8-AS1 to AS4: demo4's Reading, Sugar and Yoga figures, its Today rows of Reading and Sugar (Pending on R − 2 and
    /// R − 1, Due on R), no Missed row, "Missed this week" 0, and its notes.
    /// </summary>
    private static async Task AssertDemo4Async(IServiceProvider services, DateOnly runDate, (string Consistency, int Current, int Longest) yoga)
    {
        var demo4 = await SignInAsync(services, DemoHistoryService.DemoEmail);
        var goals = await GoalsByNameAsync(services, demo4);
        AssertStats(await ReportAsync(services, demo4, goals[Reading]), "92% (24 of 26)", 3, 18);
        AssertStats(await ReportAsync(services, demo4, goals[Sugar]), "92% (24 of 26)", 1, 16);
        AssertStats(await ReportAsync(services, demo4, goals[Yoga]), yoga.Consistency, yoga.Current, yoga.Longest);

        var dashboard = await DashboardAsync(services, demo4);
        foreach (var name in new[] { Reading, Sugar })
        {
            Assert.Equal(
                [
                    (name, runDate.AddDays(-2), runDate.AddDays(-2), EvaluationState.Pending),
                    (name, runDate.AddDays(-1), runDate.AddDays(-1), EvaluationState.Pending),
                    (name, runDate, runDate, EvaluationState.Due),
                ],
                dashboard.Today.Select(Row).Where(row => row.Goal == name).OrderBy(row => row.First));
        }

        Assert.DoesNotContain(dashboard.Today, row => row.Entry.State == EvaluationState.Missed);
        Assert.Equal(0, dashboard.Counts.MissedThisWeek);
        Assert.Contains("Fell asleep early.", dashboard.Notes.Select(n => n.Note));
        Assert.Contains("Birthday cake at work.", dashboard.Notes.Select(n => n.Note));
    }

    /// <summary>US8-AS5, FR-031: every slice-4 demo goal of demo1 to demo3 reads "N/A" and streaks of 0 at the run time.</summary>
    private static async Task AssertDemo1To3Async(IServiceProvider services)
    {
        foreach (var email in DemoDataService.DemoEmails)
        {
            var user = await SignInAsync(services, email);
            var overview = await OverviewAsync(services, user);
            Assert.NotEmpty(overview.Goals);
            Assert.All(overview.Goals, goal =>
            {
                Assert.Equal("N/A", goal.Stats.ConsistencyText);
                Assert.Equal((0, 0), (goal.Stats.CurrentStreak, goal.Stats.LongestStreak));
            });
        }
    }

    /// <summary>US8-AS11 and FR-032 on a Monday or Tuesday: Strength training's figures, and Screen time's R − 3 Missed this week.</summary>
    private static async Task AssertDemo5StrengthAndMissedThisWeekAsync(
        IServiceProvider services, DateOnly runDate, (string Consistency, int Current, int Longest) strength)
    {
        var demo5 = await SignInAsync(services, DemoReportService.DemoEmail);
        var goals = await GoalsByNameAsync(services, demo5);
        AssertStats(await ReportAsync(services, demo5, goals[Strength]), strength.Consistency, strength.Current, strength.Longest);
        AssertStats(await ReportAsync(services, demo5, goals[ScreenTime]), "93% (77 of 82)", 0, 34);

        var dashboard = await DashboardAsync(services, demo5);
        Assert.Equal(1, dashboard.Counts.MissedThisWeek);
        var missed = Assert.Single(dashboard.Today, row => row.Entry.State == EvaluationState.Missed);
        Assert.Equal((ScreenTime, runDate.AddDays(-3)), (missed.GoalName, missed.Entry.FirstDate));
    }

    private static (string Goal, DateOnly First, DateOnly Last, EvaluationState State) Row(TodayEntry row) =>
        (row.GoalName, row.Entry.FirstDate, row.Entry.LastDate, row.Entry.State);

    private static void AssertStats(GoalReport report, string consistency, int current, int longest)
    {
        Assert.Equal(consistency, report.Stats.ConsistencyText);
        Assert.Equal((current, longest), (report.Stats.CurrentStreak, report.Stats.LongestStreak));
    }

    /// <summary>
    /// Resets <c>HT-IT-SEED</c>, builds the application over it with a clock of its own at <paramref name="runTime"/>,
    /// and runs demo loading, demo history loading and demo report loading at that time.
    /// </summary>
    private async Task<ServiceProvider> LoadAllAsync(DateTimeOffset runTime)
    {
        await HtDatabase.ResetAsync(database.ConnectionString, None);
        var collection = new ServiceCollection()
            .AddLogging()
            .AddHtApplication()
            .AddHtPersistence(database.ConnectionString);
        collection.AddSingleton<TimeProvider>(new FakeTimeProvider(runTime));
        var services = collection.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });

        await using var scope = services.CreateAsyncScope();
        var provider = scope.ServiceProvider;
        Assert.Equal(DemoLoadingOutcome.Loaded, (await provider.GetRequiredService<DemoDataService>().LoadAsync(DemoPassword, None)).Outcome);
        Assert.Equal(DemoAccountOutcome.Created, Assert.Single((await provider.GetRequiredService<DemoHistoryService>().LoadAsync(DemoPassword, None)).Accounts).Outcome);
        Assert.Equal(DemoAccountOutcome.Created, Assert.Single((await provider.GetRequiredService<DemoReportService>().LoadAsync(DemoPassword, None)).Accounts).Outcome);
        return services;
    }

    private static async Task<ActingUser> SignInAsync(IServiceProvider services, string email)
    {
        await using var scope = services.CreateAsyncScope();
        var signIn = await scope.ServiceProvider.GetRequiredService<AccountService>().SignInAsync(email, DemoPassword, None);
        Assert.Equal(SignInOutcome.SignedIn, signIn.Outcome);
        return signIn.ActingUser!;
    }

    private static async Task<GoalsOverview> OverviewAsync(IServiceProvider services, ActingUser user)
    {
        await using var scope = services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<ReportService>().GetGoalsOverviewAsync(user, None);
        Assert.Equal(GoalsOverviewOutcome.Found, result.Outcome);
        return result.Overview!;
    }

    private static async Task<Dictionary<string, GoalId>> GoalsByNameAsync(IServiceProvider services, ActingUser user) =>
        (await OverviewAsync(services, user)).Goals.ToDictionary(g => g.Item.Name, g => g.Item.Id, StringComparer.Ordinal);

    private static async Task<GoalReport> ReportAsync(IServiceProvider services, ActingUser user, GoalId goal)
    {
        await using var scope = services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<ReportService>().GetGoalReportAsync(user, goal, None);
        Assert.Equal(GoalReportOutcome.Found, result.Outcome);
        return result.Report!;
    }

    private static async Task<Dashboard> DashboardAsync(IServiceProvider services, ActingUser user)
    {
        await using var scope = services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<ReportService>().GetDashboardAsync(user, None);
        Assert.Equal(DashboardOutcome.Found, result.Outcome);
        return result.Dashboard!;
    }
}
