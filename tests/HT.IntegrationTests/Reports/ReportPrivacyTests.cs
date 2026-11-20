using HT.Application;
using HT.Application.Accounts;
using HT.Application.Goals;
using HT.Application.Records;
using HT.Application.Reports;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.IntegrationTests.Accounts;
using HT.IntegrationTests.Habits;
using HT.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using static HT.IntegrationTests.Goals.GoalSamples;

namespace HT.IntegrationTests.Reports;

/// <summary>
/// Report privacy on SQL Server (FR-014, FR-015, SC-005): over the real stores, user A has a goal with records and notes;
/// user B (with a goal of B's own on the same habit), a stored administrator and an anonymous visitor read the three
/// <see cref="ReportService"/> use cases, and none of them gets A's goal, a result, a note or a figure of it. The test
/// builds its own provider with its own <see cref="FakeTimeProvider"/> over the fixture's connection string and moves
/// only that clock; the fixture's shared clock is never moved. The goals are in UTC and start on Monday 2 November 2026,
/// created at 08:00Z that day; the records are made at 12:00Z on their own day, and the reads at 10:00Z on Monday 9
/// November, when 2 to 6 November are final. Shared database: own prefixes ("it-rpv-", "It-Rpv"); every check is by
/// inclusion or exclusion of this test's own goals.
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class ReportPrivacyTests(DatabaseFixture database)
{
    private const string GoalNotFound = "This goal does not exist.";
    private const string NotForAdministrators = "Goals are for registered users.";
    private const string NotSignedIn = "You are not signed in.";

    /// <summary>Monday 2 November 2026, the start date of the goals.</summary>
    private static readonly DateOnly Mon2 = new(2026, 11, 2);

    /// <summary>08:00Z on Monday 2 November 2026, when the test's own clock starts and the goals are created.</summary>
    private static readonly DateTimeOffset ClockStart = new(2026, 11, 2, 8, 0, 0, TimeSpan.Zero);

    /// <summary>10:00Z on Monday 9 November 2026: Friday 6 November is final (its grace period ended at 00:00Z).</summary>
    private static readonly DateTimeOffset ReadAt = new(2026, 11, 9, 10, 0, 0, TimeSpan.Zero);

    private static CancellationToken None => CancellationToken.None;

    /// <summary>Build, duration, at least 20 min, every day: commitment-based.</summary>
    private static GoalTargetFields Reading20MinDaily => new()
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = 20, DurationUnit = DurationUnit.Minutes,
        ScheduleKind = ScheduleKind.FixedDays, Days = Enum.GetValues<DayOfWeek>().ToHashSet(),
    };

    [Fact]
    public async Task FR_014_FR_015_SC_005_B_an_administrator_and_a_visitor_never_get_As_goal_results_notes_or_figures()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var clock = new FakeTimeProvider(ClockStart);
        await using var services = BuildServices(clock);
        var a = await HabitAccounts.RegisteredUserAsync(services, database.ConnectionString, $"it-rpv-a-{suffix}@example.com");
        var b = await HabitAccounts.RegisteredUserAsync(services, database.ConnectionString, $"it-rpv-b-{suffix}@example.com");
        var administrator = await AdministratorAsync(services, $"it-rpv-admin-{suffix}@example.com");
        var habit = await CatalogHabitAsync(services, $"It-Rpv Reading {suffix}");
        var aName = $"It-Rpv A goal {suffix}";
        var bName = $"It-Rpv B goal {suffix}";
        var aGoal = await CreateGoalAsync(services, a, habit, aName);
        var bGoal = await CreateGoalAsync(services, b, habit, bName);

        // A: 25 minutes with a note on 2 to 5 November, 12 minutes with a note on 6 November. B: 20 minutes with a note on
        // 2 November only.
        var aNotes = new List<string>();
        for (var day = 0; day < 5; day++)
        {
            clock.SetUtcNow(ClockStart.AddDays(day).AddHours(4));
            var note = $"It-Rpv A's note {day} {suffix}";
            aNotes.Add(note);
            await RecordAsync(services, a, aGoal, Mon2.AddDays(day), new RecordInput { Amount = day < 4 ? 25 : 12, Note = note });
            if (day == 0)
            {
                await RecordAsync(services, b, bGoal, Mon2, new RecordInput { Amount = 20, Note = $"It-Rpv B's note {suffix}" });
            }
        }

        clock.SetUtcNow(ReadAt);

        // A reads A's own reports: the arrangement is what the other readers must not see.
        var aReport = (await ReportAsync(services, a, aGoal)).Report!;
        Assert.Equal("80% (4 of 5)", aReport.Stats.ConsistencyText);
        var aDashboard = (await DashboardAsync(services, a)).Dashboard!;
        Assert.Equal(aNotes.AsEnumerable().Reverse(), aDashboard.Notes.Select(n => n.Note));

        // B: A's goal is the same as no goal; B's overview and dashboard hold only B's goal, with B's own figures and note.
        var onAsGoal = await ReportAsync(services, b, aGoal);
        var onNoGoal = await ReportAsync(services, b, new GoalId(Guid.NewGuid()));
        foreach (var refused in new[] { onAsGoal, onNoGoal })
        {
            Assert.Equal(GoalReportOutcome.NotFound, refused.Outcome);
            Assert.Equal(GoalNotFound, refused.Message);
            Assert.Null(refused.Report);
        }

        var bReport = (await ReportAsync(services, b, bGoal)).Report!;
        Assert.Equal("20% (1 of 5)", bReport.Stats.ConsistencyText);
        var bOverview = (await OverviewAsync(services, b)).Overview!;
        var bItem = Assert.Single(bOverview.Goals);
        Assert.Equal((bGoal, bName), (bItem.Item.Id, bItem.Item.Name));
        Assert.Equal(bReport.Stats, bItem.Stats);
        Assert.All(bItem.Recordable.Entries, entry => Assert.DoesNotContain(entry.Note, aNotes));
        var bDashboard = (await DashboardAsync(services, b)).Dashboard!;
        Assert.Equal(1, bDashboard.Counts.Goals);
        Assert.Equal([(bGoal, bName)], bDashboard.Goals.Select(g => (g.GoalId, g.Name)));
        Assert.Equal(bReport.Stats, Assert.Single(bDashboard.Goals).Stats);
        Assert.All(bDashboard.Today, row =>
        {
            Assert.Equal(bGoal, row.GoalId);
            Assert.DoesNotContain(row.Entry.Note, aNotes);
        });
        Assert.Equal([$"It-Rpv B's note {suffix}"], bDashboard.Notes.Select(n => n.Note));

        // The administrator and the anonymous visitor: refused by every use case, with no view.
        foreach (var (user, message, overviewOutcome, dashboardOutcome, reportOutcome) in new[]
        {
            (administrator, NotForAdministrators, GoalsOverviewOutcome.NotForAdministrators, DashboardOutcome.NotForAdministrators, GoalReportOutcome.NotForAdministrators),
            (ActingUser.Anonymous, NotSignedIn, GoalsOverviewOutcome.NotSignedIn, DashboardOutcome.NotSignedIn, GoalReportOutcome.NotSignedIn),
        })
        {
            var report = await ReportAsync(services, user, aGoal);
            var overview = await OverviewAsync(services, user);
            var dashboard = await DashboardAsync(services, user);
            Assert.Equal((reportOutcome, message), (report.Outcome, report.Message));
            Assert.Equal((overviewOutcome, message), (overview.Outcome, overview.Message));
            Assert.Equal((dashboardOutcome, message), (dashboard.Outcome, dashboard.Message));
            Assert.Null(report.Report);
            Assert.Null(overview.Overview);
            Assert.Null(dashboard.Dashboard);
        }
    }

    /// <summary>Creates a stored administrator with this email (deleting an earlier one) and returns it as an acting user.</summary>
    private async Task<ActingUser> AdministratorAsync(IServiceProvider services, string email)
    {
        await Sql.DeleteUserAsync(database.ConnectionString, email);
        Assert.True(Email.TryParse(email, out var parsed));
        await using var scope = services.CreateAsyncScope();
        var created = await scope.ServiceProvider.GetRequiredService<IAccountStore>()
            .CreateAsync(parsed, Role.Administrator, HabitAccounts.Password, TestProfiles.Utc, None);
        var account = created.Account ?? throw new InvalidOperationException("The test email is taken.");
        return ActingUser.ForAccount(account.Id, account.Role);
    }

    private static async Task<GoalId> CreateGoalAsync(IServiceProvider services, ActingUser user, HabitId habit, string name)
    {
        await using var scope = services.CreateAsyncScope();
        var created = await scope.ServiceProvider.GetRequiredService<GoalService>().CreateAsync(
            user, new GoalInput { Habit = new GoalHabit(habit, HabitKind.Catalog), Target = Reading20MinDaily, StartDate = Mon2, Name = name }, None);
        Assert.Equal(GoalCreateOutcome.Created, created.Outcome);
        return created.GoalId!.Value;
    }

    private static async Task RecordAsync(IServiceProvider services, ActingUser user, GoalId goal, DateOnly date, RecordInput input)
    {
        await using var scope = services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<RecordingService>().RecordAsync(user, goal, date, input, None);
        Assert.Equal(RecordWriteOutcome.Recorded, result.Outcome);
    }

    private static async Task<GoalReportResult> ReportAsync(IServiceProvider services, ActingUser user, GoalId goal)
    {
        await using var scope = services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ReportService>().GetGoalReportAsync(user, goal, None);
    }

    private static async Task<GoalsOverviewResult> OverviewAsync(IServiceProvider services, ActingUser user)
    {
        await using var scope = services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ReportService>().GetGoalsOverviewAsync(user, None);
    }

    private static async Task<DashboardResult> DashboardAsync(IServiceProvider services, ActingUser user)
    {
        await using var scope = services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ReportService>().GetDashboardAsync(user, None);
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
