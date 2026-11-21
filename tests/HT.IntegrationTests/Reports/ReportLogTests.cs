using System.Globalization;
using System.Text.RegularExpressions;
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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using static HT.IntegrationTests.Goals.GoalSamples;

namespace HT.IntegrationTests.Reports;

/// <summary>
/// HP-007 (constitution III): the three <see cref="ReportService"/> reads run on SQL Server with every log entry captured
/// at <see cref="LogLevel.Trace"/>, for the owner, for other and refused readers and with a cancelled token, and no captured message
/// or exception text, and no thrown exception, contains a note, a recorded value, the custom count unit, a goal name, a
/// habit name or a figure text of the reports. The names, the unit and the notes carry unique markers; the recorded
/// values are numbers no log would hold by chance. The goals are in UTC and start on Monday 2 November 2026, created at
/// 08:00Z that day on the test's own clock (the fixture's shared clock is never moved); the records are made at 12:00Z
/// on their own day and the reads at 10:00Z on Monday 9 November. Shared database: own prefixes ("it-rlg-", "It-Rlg").
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class ReportLogTests(DatabaseFixture database)
{
    private const string GoalMarker = "Zqrepgoalmark";
    private const string CustomHabitMarker = "Zqrepcustommark";
    private const string CatalogHabitMarker = "Zqrepcatalogmark";
    private const string UnitMarker = "zqrepunitmark";
    private const string NoteMarker = "Zqrepnotemark";

    /// <summary>The recorded minutes, each below the 23-hour (1,380-minute) target, so each is a partial amount.</summary>
    private const int Partial = 1373;

    private const int OtherPartial = 1291;

    /// <summary>The recorded counts, over the count goal's target of 999.</summary>
    private const int Count = 7311;

    private const int OtherCount = 6553;

    private static readonly int[] Values = [Partial, OtherPartial, Count, OtherCount];
    private static readonly string[] TextMarkers = [GoalMarker, CustomHabitMarker, CatalogHabitMarker, UnitMarker, NoteMarker];

    /// <summary>Monday 2 November 2026, the start date of the goals.</summary>
    private static readonly DateOnly Mon2 = new(2026, 11, 2);

    /// <summary>08:00Z on Monday 2 November 2026, when the test's own clock starts and the goals are created.</summary>
    private static readonly DateTimeOffset ClockStart = new(2026, 11, 2, 8, 0, 0, TimeSpan.Zero);

    /// <summary>10:00Z on Monday 9 November 2026: 2 to 6 November are final, 7 and 8 are Pending, 9 is Due.</summary>
    private static readonly DateTimeOffset ReadAt = new(2026, 11, 9, 10, 0, 0, TimeSpan.Zero);

    private static CancellationToken None => CancellationToken.None;

    private static string Note(string what) => $"{NoteMarker} {what}";

    [Fact]
    public async Task HP_007_the_reads_log_no_note_value_unit_goal_name_habit_name_or_figure_text()
    {
        var capture = new LogCapture();
        var clock = new FakeTimeProvider(ClockStart);
        await using var services = BuildServices(capture, clock);
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var user = await HabitAccounts.RegisteredUserAsync(services, database.ConnectionString, $"it-rlg-{suffix}@example.com");
        var other = await HabitAccounts.RegisteredUserAsync(services, database.ConnectionString, $"it-rlg-other-{suffix}@example.com");
        var custom = await CustomHabitAsync(services, user.AccountId!.Value, $"It-Rlg {CustomHabitMarker} {suffix}");
        var catalog = await CatalogHabitAsync(services, $"It-Rlg {CatalogHabitMarker} {suffix}");

        // A daily goal of at least 23 h on the custom habit, and a daily count goal of at least 999 units on the catalog
        // habit, each with its own marked name.
        var duration = await CreateGoalAsync(services, user, new GoalHabit(custom, HabitKind.Custom), new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = 23, DurationUnit = DurationUnit.Hours,
            ScheduleKind = ScheduleKind.FixedDays, Days = Enum.GetValues<DayOfWeek>().ToHashSet(),
        }, $"It-Rlg {GoalMarker} duration {suffix}");
        var count = await CreateGoalAsync(services, user, new GoalHabit(catalog, HabitKind.Catalog), new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Count, Amount = 999, CountUnit = UnitMarker,
            ScheduleKind = ScheduleKind.FixedDays, Days = Enum.GetValues<DayOfWeek>().ToHashSet(),
        }, $"It-Rlg {GoalMarker} count {suffix}");

        // Partial minutes with notes on 2, 3, 5 and 6 November (4 November Missed); counts with notes on 2 and 3 November.
        foreach (var day in new[] { 0, 1, 3, 4 })
        {
            clock.SetUtcNow(ClockStart.AddDays(day).AddHours(4));
            var date = Mon2.AddDays(day);
            await RecordAsync(services, user, duration, date, new RecordInput { Amount = day % 2 == 0 ? Partial : OtherPartial, Note = Note($"duration {day}") });
            if (day < 2)
            {
                await RecordAsync(services, user, count, date, new RecordInput { Amount = day == 0 ? Count : OtherCount, Note = Note($"count {day}") });
            }
        }

        clock.SetUtcNow(ReadAt);
        var figures = new List<string>();
        var exceptions = new List<Exception>();

        // The owner's three reads: every figure text is kept to be searched for.
        foreach (var goal in new[] { duration, count })
        {
            var report = await ReadAsync(services, s => s.GetGoalReportAsync(user, goal, None));
            Assert.Equal(GoalReportOutcome.Found, report.Outcome);
            figures.AddRange(FiguresOf(report.Report!.Stats));
            var trend = report.Report.Trend;
            figures.AddRange([trend.Title, trend.SubLine, trend.TargetLine, trend.Label]);
        }

        var overview = await ReadAsync(services, s => s.GetGoalsOverviewAsync(user, None));
        Assert.Equal(GoalsOverviewOutcome.Found, overview.Outcome);
        Assert.Equal(2, overview.Overview!.Goals.Count);
        figures.AddRange(overview.Overview.Goals.SelectMany(g => FiguresOf(g.Stats)));
        var dashboard = await ReadAsync(services, s => s.GetDashboardAsync(user, None));
        Assert.Equal(DashboardOutcome.Found, dashboard.Outcome);
        Assert.NotEmpty(dashboard.Dashboard!.Notes);
        figures.AddRange(dashboard.Dashboard.Goals.SelectMany(g => FiguresOf(g.Stats)));
        Assert.DoesNotContain("N/A", figures);

        // Another user: the goal report is refused, and the overview and dashboard hold none of the owner's goals.
        var results = new List<string?> { (await ReadAsync(services, s => s.GetGoalReportAsync(other, duration, None))).Message };
        Assert.Empty((await ReadAsync(services, s => s.GetGoalsOverviewAsync(other, None))).Overview!.Goals);
        Assert.Empty((await ReadAsync(services, s => s.GetDashboardAsync(other, None))).Dashboard!.Goals);

        // The refused reads of an administrator, an account that is not stored and a visitor.
        foreach (var actingUser in new[]
        {
            ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.Administrator),
            ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.RegisteredUser),
            ActingUser.Anonymous,
        })
        {
            results.Add((await ReadAsync(services, s => s.GetGoalReportAsync(actingUser, duration, None))).Message);
            results.Add((await ReadAsync(services, s => s.GetGoalsOverviewAsync(actingUser, None))).Message);
            results.Add((await ReadAsync(services, s => s.GetDashboardAsync(actingUser, None))).Message);
        }

        Assert.All(results, message => Assert.NotNull(message));

        // The owner's three reads with a cancelled token.
        using var cancelled = new CancellationTokenSource();
        await cancelled.CancelAsync();
        var token = cancelled.Token;
        exceptions.Add(await Assert.ThrowsAnyAsync<OperationCanceledException>(() => ReadAsync(services, s => s.GetGoalReportAsync(user, count, token))));
        exceptions.Add(await Assert.ThrowsAnyAsync<OperationCanceledException>(() => ReadAsync(services, s => s.GetGoalsOverviewAsync(user, token))));
        exceptions.Add(await Assert.ThrowsAnyAsync<OperationCanceledException>(() => ReadAsync(services, s => s.GetDashboardAsync(user, token))));

        Assert.NotEmpty(capture.Entries);
        var protectedTexts = TextMarkers.Concat(figures.Distinct()).ToArray();
        capture.AssertContainsNone(protectedTexts);
        foreach (var entry in capture.Entries)
        {
            Assert.False(HasValue(entry.Message) || HasValue(entry.Exception), $"A log entry of category {entry.Category} at level {entry.Level} contains a recorded value.");
        }

        foreach (var exception in exceptions)
        {
            var text = exception.ToString();
            Assert.False(
                protectedTexts.Any(p => text.Contains(p, StringComparison.OrdinalIgnoreCase)) || HasValue(text),
                $"A thrown {exception.GetType().Name} contains a protected value.");
        }

        foreach (var text in results)
        {
            Assert.False(TextMarkers.Any(m => text!.Contains(m, StringComparison.OrdinalIgnoreCase)) || HasValue(text), "A result text contains a protected value.");
        }
    }

    private static IEnumerable<string> FiguresOf(GoalStatsView stats) => [stats.ConsistencyText, stats.CurrentStreakText, stats.LongestStreakText];

    /// <summary>
    /// Whether the text holds a recorded value as a whole number that is not part of a GUID, a stack trace's line number
    /// or a duration in milliseconds, as <c>RecordLogTests</c> counts it.
    /// </summary>
    private static bool HasValue(string? text) =>
        text is not null && Values.Any(v => ValuePattern(v).IsMatch(text));

    private static Regex ValuePattern(int value) =>
        new($"(?<!:line )(?<![0-9A-Fa-f-]){value.ToString(CultureInfo.InvariantCulture)}(?![0-9A-Fa-f-])(?!ms)", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    private static async Task<T> ReadAsync<T>(IServiceProvider services, Func<ReportService, Task<T>> call)
    {
        await using var scope = services.CreateAsyncScope();
        return await call(scope.ServiceProvider.GetRequiredService<ReportService>());
    }

    private static async Task<GoalId> CreateGoalAsync(IServiceProvider services, ActingUser user, GoalHabit habit, GoalTargetFields target, string name)
    {
        await using var scope = services.CreateAsyncScope();
        var created = await scope.ServiceProvider.GetRequiredService<GoalService>().CreateAsync(
            user, new GoalInput { Habit = habit, Target = target, StartDate = Mon2, Name = name }, None);
        Assert.Equal(GoalCreateOutcome.Created, created.Outcome);
        return created.GoalId!.Value;
    }

    private static async Task RecordAsync(IServiceProvider services, ActingUser user, GoalId goal, DateOnly date, RecordInput input)
    {
        await using var scope = services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<RecordingService>().RecordAsync(user, goal, date, input, None);
        Assert.Equal(RecordWriteOutcome.Recorded, result.Outcome);
    }

    /// <summary>The application as the integration tests build it, with the test's own clock and only <paramref name="capture"/> as its logger, at <see cref="LogLevel.Trace"/>.</summary>
    private ServiceProvider BuildServices(LogCapture capture, FakeTimeProvider clock)
    {
        var services = new ServiceCollection()
            .AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(LogLevel.Trace);
                logging.AddProvider(capture);
            })
            .AddHtApplication()
            .AddHtPersistence(database.ConnectionString);
        services.AddSingleton<TimeProvider>(clock);
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
    }
}
