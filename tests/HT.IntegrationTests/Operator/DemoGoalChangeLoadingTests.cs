using HT.Application;
using HT.Application.Accounts;
using HT.Application.Goals;
using HT.Application.Habits;
using HT.Application.Operator;
using HT.Application.Records;
using HT.Application.Reports;
using HT.Domain.Accounts;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using HT.Domain.Time;
using HT.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;

namespace HT.IntegrationTests.Operator;

/// <summary>
/// Slice 8 US6-AS1 to AS5, FR-030, FR-031, SC-007 and research R-09 on SQL Server: demo goal-change loading, after the
/// seed catalog and the four earlier demo tasks (demo loading, demo history loading, demo report loading, demo pause
/// loading), creates <c>demo7@example.com</c> with the goals, settings versions, attempts, endings, records and notes of
/// FR-031 in one transaction, and <c>demo7</c>'s goals, attempts, reports, dashboard and goal history read at the run time
/// give FR-031's figures on every weekday. Each test resets <c>HT-IT-SEED</c> with <see cref="HtDatabase.ResetAsync"/> and
/// builds its own provider with its own <see cref="FakeTimeProvider"/>, so <see cref="SeedDatabaseFixture.Clock"/> is
/// never set or moved. The default zone is UTC, so the run date R is the UTC date of the run time. The names this class
/// creates itself have the prefix "It Dg" or "it-dg-".
/// </summary>
[Collection(SeedDatabaseCollection.Name)]
public sealed class DemoGoalChangeLoadingTests(SeedDatabaseFixture database)
{
    private const string DemoPassword = "example2026";
    private const string Demo7 = "demo7@example.com";
    private const string Reading = "Reading, 20 min daily";
    private const string Meditation = "Meditation, 5 min daily";
    private const string Sugar = "Sugar, at most 1 sweet a day";

    /// <summary>Wednesday 18 November 2026, 10:00 UTC, the run time of the tests that do not vary the weekday.</summary>
    private static readonly DateTimeOffset Wednesday = new(2026, 11, 18, 10, 0, 0, TimeSpan.Zero);

    private static readonly ActingUser Administrator = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.Administrator);

    private static CancellationToken None => CancellationToken.None;

    /// <summary>The seven weekdays of the week of Monday 16 November 2026, at 10:00 UTC.</summary>
    public static TheoryData<DateTimeOffset, DayOfWeek> RunTimes => new()
    {
        { new DateTimeOffset(2026, 11, 16, 10, 0, 0, TimeSpan.Zero), DayOfWeek.Monday },
        { new DateTimeOffset(2026, 11, 17, 10, 0, 0, TimeSpan.Zero), DayOfWeek.Tuesday },
        { new DateTimeOffset(2026, 11, 18, 10, 0, 0, TimeSpan.Zero), DayOfWeek.Wednesday },
        { new DateTimeOffset(2026, 11, 19, 10, 0, 0, TimeSpan.Zero), DayOfWeek.Thursday },
        { new DateTimeOffset(2026, 11, 20, 10, 0, 0, TimeSpan.Zero), DayOfWeek.Friday },
        { new DateTimeOffset(2026, 11, 21, 10, 0, 0, TimeSpan.Zero), DayOfWeek.Saturday },
        { new DateTimeOffset(2026, 11, 22, 10, 0, 0, TimeSpan.Zero), DayOfWeek.Sunday },
    };

    [Theory]
    [MemberData(nameof(RunTimes))]
    public async Task US6_AS1_AS2_SC_007_demo7_is_created_and_its_goals_attempts_reports_dashboard_and_goal_history_give_FR_031s_figures(
        DateTimeOffset runTime, DayOfWeek day)
    {
        var r = DateOnly.FromDateTime(runTime.UtcDateTime);
        Assert.Equal(day, r.DayOfWeek);
        await using var services = await ResetAndBuildAsync(runTime);
        await LoadEarlierTasksAsync(services);

        var result = await LoadDemoGoalChangesAsync(services);

        Assert.Equal(DemoGoalChangeLoadingOutcome.Loaded, result.Outcome);
        Assert.Null(result.Message);
        var account = Assert.Single(result.Accounts);
        Assert.Equal(Demo7, account.Email);
        Assert.Equal(DemoAccountOutcome.Created, account.Outcome);
        Assert.Equal("created", account.Message);
        Assert.Equal("demo goals added", account.Goals!.Message);
        Assert.Equal("demo records added", account.Records!.Message);

        var demo7 = await SignInAsync(services, Demo7);
        Assert.Equal(Role.RegisteredUser, demo7.Role);
        var list = await ListAsync(services, demo7);
        Assert.Equal(
            [(Reading, false), (Meditation, false), (Sugar, true)],
            list.Select(g => (g.Name, g.IsEnded)).OrderBy(g => g.IsEnded).ThenByDescending(g => g.Name, StringComparer.Ordinal));
        var ids = list.ToDictionary(g => g.Name, g => g.Id, StringComparer.Ordinal);

        // Reading: R − 28 to R − 3 counted, R − 9 Failed (15 of 20 minutes) while R − 14 with 15 minutes is Met under
        // version 1; R − 2 and R − 1 Pending, R Due; 20 minutes from R − 13 and the waiting 25 minutes from R + 1.
        var reading = await ReportAsync(services, demo7, ids[Reading]);
        Assert.Equal("96% (25 of 26)", reading.Stats.ConsistencyText);
        Assert.Equal(("6 commitments", "19 commitments"), (reading.Stats.CurrentStreakText, reading.Stats.LongestStreakText));
        var readingHistory = await HistoryAsync(services, demo7, ids[Reading], r.AddDays(-28), r);
        Assert.Equal(29, readingHistory.Count);
        Assert.Equal(
            [(r.AddDays(-9), EvaluationState.Failed), (r.AddDays(-2), EvaluationState.Pending), (r.AddDays(-1), EvaluationState.Pending), (r, EvaluationState.Due)],
            readingHistory.Where(e => e.State != EvaluationState.Met).Select(e => (e.FirstDate, e.State)).OrderBy(e => e.FirstDate));
        Assert.Equal(15, Assert.Single(readingHistory.Single(e => e.FirstDate == r.AddDays(-14)).Records).Amount);
        var shortDay = readingHistory.Single(e => e.FirstDate == r.AddDays(-9));
        Assert.Equal((15, "Short on time."), (Assert.Single(shortDay.Records).Amount, shortDay.Note));
        var readingDetails = await DetailsAsync(services, demo7, ids[Reading]);
        Assert.Equal(("20 min daily", r.AddDays(-13)), (readingDetails.TargetText, readingDetails.TimeZoneAppliesFrom));
        Assert.Equal(("25 min daily", r.AddDays(1)), (readingDetails.Later!.TargetText, readingDetails.LaterTimeZoneAppliesFrom!.Value));
        Assert.Equal(1, readingDetails.AttemptNumber);

        // Meditation: attempt 2 from R − 14 with 12 of 12; attempt 1 ended R − 21, last date R − 22, 11 of 21.
        var meditation = await ReportAsync(services, demo7, ids[Meditation]);
        Assert.Equal("100% (12 of 12)", meditation.Stats.ConsistencyText);
        Assert.Equal(("12 commitments", "12 commitments"), (meditation.Stats.CurrentStreakText, meditation.Stats.LongestStreakText));
        Assert.Equal(
            [(r.AddDays(-2), EvaluationState.Pending), (r.AddDays(-1), EvaluationState.Pending), (r, EvaluationState.Due)],
            (await HistoryAsync(services, demo7, ids[Meditation], r.AddDays(-2), r)).Select(e => (e.FirstDate, e.State)).OrderBy(e => e.FirstDate));
        var meditationDetails = await DetailsAsync(services, demo7, ids[Meditation]);
        Assert.Equal((2, r.AddDays(-14)), (meditationDetails.AttemptNumber, meditationDetails.AttemptStartDate));
        var attempts = await AttemptsAsync(services, demo7, ids[Meditation]);
        Assert.Equal([2, 1], attempts.Select(a => a.Number));
        Assert.Equal((Meditation, r.AddDays(-14), true), (attempts[0].Name, attempts[0].StartDate, attempts[0].IsLatest));
        Assert.Equal("100% (12 of 12)", attempts[0].Stats.ConsistencyText);
        Assert.Equal(("12 commitments", "12 commitments"), (attempts[0].Stats.CurrentStreakText, attempts[0].Stats.LongestStreakText));
        var earlier = attempts[1];
        Assert.Equal("Meditation, 10 min daily", earlier.Name);
        Assert.Equal((r.AddDays(-42), r.AddDays(-21), r.AddDays(-22)), (earlier.StartDate, earlier.EndingDate!.Value, earlier.LastDate!.Value));
        Assert.Equal((Outcome.Abandoned, "Too busy."), (earlier.Outcome!.Value, earlier.EndingNote));
        Assert.Equal(("52% (11 of 21)", "11 commitments"), (earlier.Stats.ConsistencyText, earlier.Stats.LongestStreakText));

        // Sugar: ended R − 7 as Achieved, 23 of 23 days.
        var sugarDetails = await DetailsAsync(services, demo7, ids[Sugar]);
        Assert.True(sugarDetails.IsEnded);
        Assert.Equal((r.AddDays(-7), Outcome.Achieved), (sugarDetails.EndingDate!.Value, sugarDetails.Outcome!.Value));
        Assert.Equal("Three weeks without extra sweets.", sugarDetails.EndingNote);
        var sugar = Assert.Single(await AttemptsAsync(services, demo7, ids[Sugar]));
        Assert.Equal("100% (23 of 23)", sugar.Stats.ConsistencyText);
        Assert.Equal(("23 days", "23 days"), (sugar.Stats.CurrentStreakText, sugar.Stats.LongestStreakText));

        var dashboard = await DashboardAsync(services, demo7);
        Assert.Equal(new DashboardCounts { Due = 2, Pending = 4, MissedThisWeek = 0, Goals = 2 }, dashboard.Counts);

        Assert.Equal(
            [
                (ids[Sugar], Sugar, 1, r.AddDays(-7), Outcome.Achieved, "Three weeks without extra sweets."),
                (ids[Meditation], "Meditation, 10 min daily", 1, r.AddDays(-21), Outcome.Abandoned, "Too busy."),
            ],
            (await GoalHistoryAsync(services, demo7)).Select(e => (e.GoalId, e.Name, e.AttemptNumber, e.EndingDate, e.Outcome, e.EndingNote)));
    }

    [Fact]
    public async Task US6_AS1_the_account_has_the_default_zone_no_name_the_run_time_as_its_creation_time_and_FR_031s_rows()
    {
        await using var services = await ResetAndBuildAsync(Wednesday);
        await LoadEarlierTasksAsync(services);
        await LoadDemoGoalChangesAsync(services);

        await using (var scope = services.CreateAsyncScope())
        {
            Assert.True(Email.TryParse(Demo7, out var email));
            var stored = await scope.ServiceProvider.GetRequiredService<IAccountStore>().FindByEmailAsync(email, None);
            Assert.NotNull(stored);
            Assert.Equal(Role.RegisteredUser, stored.Role);
            Assert.Null(stored.Name);
            Assert.Equal("UTC", stored.StoredTimeZone);
            Assert.Equal(Wednesday, stored.CreatedAt);
        }

        Assert.Equal(0, await CountForAsync("SELECT COUNT(*) FROM [CustomHabits] t JOIN [AspNetUsers] u ON u.[Id] = t.[OwnerId]"));
        Assert.Equal(3, await CountForAsync("SELECT COUNT(*) FROM [Goals] t JOIN [AspNetUsers] u ON u.[Id] = t.[OwnerId]"));
        Assert.Equal(0, await CountForAsync("SELECT COUNT(*) FROM [Goals] t JOIN [AspNetUsers] u ON u.[Id] = t.[OwnerId] WHERE t.[Name] IS NOT NULL"));
        Assert.Equal(4, await CountForAsync(GoalChild("GoalAttempts")));
        Assert.Equal(3 + 2 + 1, await CountForAsync(GoalChild("GoalSettingsVersions")));
        Assert.Equal(6, await CountForAsync(GoalChild("GoalSettingsVersions") + " AND c.[TimeZone] = 'UTC'"));
        Assert.Equal(2, await CountForAsync(GoalChild("GoalAttempts") + " AND c.[EndingDate] IS NOT NULL AND c.[Outcome] IS NOT NULL AND c.[EndingNote] IS NOT NULL"));
        Assert.Equal(27 + 24 + 23, await CountForAsync(GoalChild("GoalRecords")));
        Assert.Equal(1, await CountForAsync(GoalChild("GoalNotes")));
        Assert.Equal(0, await CountForAsync(GoalChild("GoalExcusals")));
        Assert.Equal(0, await CountForAsync(GoalChild("GoalPauses")));
    }

    [Fact]
    public async Task US6_AS3_a_second_run_gives_already_exists_with_no_goal_or_record_result_and_changes_nothing()
    {
        await using var services = await ResetAndBuildAsync(Wednesday);
        await LoadEarlierTasksAsync(services);
        await LoadDemoGoalChangesAsync(services);
        var once = await SnapshotAsync();

        var second = await LoadDemoGoalChangesAsync(services);

        Assert.Equal(DemoGoalChangeLoadingOutcome.Loaded, second.Outcome);
        var account = Assert.Single(second.Accounts);
        Assert.Equal(Demo7, account.Email);
        Assert.Equal(DemoAccountOutcome.AlreadyExists, account.Outcome);
        Assert.Equal("already exists", account.Message);
        Assert.Null(account.Goals);
        Assert.Null(account.Records);
        Assert.Equal(once, await SnapshotAsync());
    }

    [Theory]
    [InlineData(null, "no demo password configured")]
    [InlineData("abc", "demo password does not meet the password rule")]
    public async Task US6_AS4_no_or_a_weak_demo_password_creates_nothing(string? password, string message)
    {
        await using var services = await ResetAndBuildAsync(Wednesday);
        await LoadEarlierTasksAsync(services);
        var before = await SnapshotAsync();

        DemoGoalChangeLoadingResult result;
        await using (var scope = services.CreateAsyncScope())
        {
            result = await scope.ServiceProvider.GetRequiredService<DemoGoalChangeService>().LoadAsync(password, None);
        }

        Assert.Equal(message, result.Message);
        Assert.Empty(result.Accounts);
        Assert.Equal(0, await Sql.CountUsersAsync(database.ConnectionString, Demo7));
        Assert.Equal(before, await SnapshotAsync());
    }

    [Fact]
    public async Task A_goal_whose_habit_is_not_Published_is_left_out_with_its_attempts_versions_records_and_notes()
    {
        await using var services = await ResetAndBuildAsync(Wednesday);
        await CreateCatalogHabitAsync(services, "READING", Category.Mind, Intent.Build, publish: true);
        await CreateCatalogHabitAsync(services, "Meditation", Category.Mind, Intent.Build, publish: false);

        var result = await LoadDemoGoalChangesAsync(services);

        var account = Assert.Single(result.Accounts);
        Assert.Equal(DemoAccountOutcome.Created, account.Outcome);
        Assert.Equal("demo goals added", account.Goals!.Message);
        Assert.Equal("demo records added", account.Records!.Message);
        var demo7 = await SignInAsync(services, Demo7);
        Assert.Equal(["READING, 20 min daily"], (await ListAsync(services, demo7)).Select(g => g.Name));
        var reading = DemoGoalChanges.For(new DateOnly(2026, 11, 18))[0];
        Assert.Equal(1, await CountAsync("Goals"));
        Assert.Equal(1, await CountAsync("GoalAttempts"));
        Assert.Equal(3, await CountAsync("GoalSettingsVersions"));
        Assert.Equal(reading.Records.Count, await CountAsync("GoalRecords"));
        Assert.Equal(1, await CountAsync("GoalNotes"));
    }

    [Fact]
    public async Task A_write_failure_after_the_attempts_versions_records_and_first_note_rolls_the_account_back_and_names_no_goal_habit_note_outcome_or_amount()
    {
        const string email = "it-dg-rollback@example.com";
        const int amount = 7213;
        await using (var arrange = await ResetAndBuildAsync(Wednesday))
        {
            await CreateCatalogHabitAsync(arrange, "It Dg Zqhabit", Category.Mind, Intent.Build, publish: true);
        }

        using var capture = new LogCapture();
        await using var services = capture.BuildServices(database.ConnectionString);
        Assert.True(Email.TryParse(email, out var parsed));
        Assert.True(GoalTarget.TryCreate(
            new GoalTargetFields
            {
                BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Count, Amount = 2, CountUnit = "zqunits",
                ScheduleKind = ScheduleKind.Times, Times = 1, Period = SchedulePeriod.Day,
            },
            out var target,
            out _));
        var start = new DateOnly(2026, 10, 21);
        var day = new DateOnly(2026, 11, 10);
        Assert.True(RecordValue.TryCreate(Measurement.Count, null, amount, out var value, out _));
        Assert.True(NoteText.TryCreate("It Dg Zqnote", out var note, out _));
        Assert.True(EndingNote.TryCreate("It Dg Zqending", out var endingNote, out _));

        // Two notes under the same entry key break the GoalNotes primary key, after the account, the goal with its two
        // attempts (the first ended), their versions, its record and its first note are written.
        var goal = new DemoGoalChangeGoal(
            "It Dg Zqhabit",
            [CatalogHabitState.Published],
            [
                new DemoAttempt(1, [new DemoVersion(start, target)], new GoalEnding(start.AddDays(5), Outcome.Abandoned, endingNote)),
                new DemoAttempt(2, [new DemoVersion(start.AddDays(10), target)], null),
            ],
            [new DemoRecord(day, value)],
            [new DemoNote(day, note!), new DemoNote(day, note!)]);

        InvalidOperationException exception;
        await using (var scope = services.CreateAsyncScope())
        {
            exception = await Assert.ThrowsAsync<InvalidOperationException>(() => scope.ServiceProvider.GetRequiredService<IDemoGoalChangeStore>()
                .CreateDemoGoalChangeAccountAsync(parsed, DemoPassword, new DemoAccountProfile(null, NamedTimeZone.Utc, Wednesday), [goal], None));
        }

        Assert.Equal(0, await Sql.CountUsersAsync(database.ConnectionString, email));
        Assert.Equal(0, await CountAsync("Goals"));
        Assert.Equal(0, await CountAsync("GoalAttempts"));
        Assert.Equal(0, await CountAsync("GoalSettingsVersions"));
        Assert.Equal(0, await CountAsync("GoalRecords"));
        Assert.Equal(0, await CountAsync("GoalNotes"));
        Assert.Null(exception.InnerException);
        Assert.StartsWith("Demo goal-change account creation failed", exception.Message, StringComparison.Ordinal);
        Assert.Contains("(SQL error 2627)", exception.Message, StringComparison.Ordinal);
        string[] values = ["Zq", "zqunits", "Abandoned", amount.ToString(System.Globalization.CultureInfo.InvariantCulture)];
        Assert.DoesNotContain(values, v => exception.ToString().Contains(v, StringComparison.OrdinalIgnoreCase));
        capture.AssertContainsNone(values);
    }

    [Fact]
    public async Task US6_AS5_FR_030_the_earlier_demo_tasks_write_no_ending_second_attempt_or_settings_change_and_their_figures_stay_the_same()
    {
        await using var services = await ResetAndBuildAsync(Wednesday);
        await LoadEarlierTasksAsync(services);
        Assert.Equal(0, await ChangesOfOthersAsync());
        string[] earlier = [.. DemoDataService.DemoEmails, DemoHistoryService.DemoEmail, DemoReportService.DemoEmail, DemoPauseService.DemoEmail];
        var before = await FiguresAsync(services, earlier);

        await LoadDemoGoalChangesAsync(services);

        Assert.Equal(before, await FiguresAsync(services, earlier));
        Assert.Equal(0, await ChangesOfOthersAsync());
    }

    [Fact]
    public async Task FR_031_no_result_or_log_entry_names_a_demo7_goal_habit_note_outcome_or_amount()
    {
        await HtDatabase.ResetAsync(database.ConnectionString, None);
        using var capture = new LogCapture();
        var collection = new ServiceCollection()
            .AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(LogLevel.Trace);
                logging.AddProvider(capture);
            })
            .AddHtApplication()
            .AddHtPersistence(database.ConnectionString);
        collection.AddSingleton<TimeProvider>(new FakeTimeProvider(Wednesday));
        await using var services = collection.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
        await LoadEarlierTasksAsync(services);

        var first = await LoadDemoGoalChangesAsync(services);
        var second = await LoadDemoGoalChangesAsync(services);

        // The habit names alone are left out: the demo catalog's own lines are not in this check, and the goal names hold them.
        string[] values =
        [
            .. DemoGoalChanges.GoalNames, DemoGoalChanges.EarlierAttemptName, DemoGoalChanges.WaitingTargetText,
            DemoGoalChanges.ShortOnTimeNote, DemoGoalChanges.TooBusyEndingNote, DemoGoalChanges.SugarEndingNote,
            "Achieved", "Abandoned", "sweet",
        ];
        capture.AssertContainsNone(values);
        foreach (var result in new[] { first, second })
        {
            var texts = result.Accounts
                .SelectMany(a => new[] { a.Email, a.Message, a.Goals?.Message, a.Records?.Message })
                .Append(result.Message)
                .OfType<string>();
            Assert.All(texts, text => Assert.DoesNotContain(values, value => text.Contains(value, StringComparison.OrdinalIgnoreCase)));
            Assert.All(result.Accounts, a => Assert.Equal(Demo7, a.Email));
        }
    }

    /// <summary>Resets <c>HT-IT-SEED</c> and builds the application over it with a clock of its own at <paramref name="runTime"/>.</summary>
    private async Task<ServiceProvider> ResetAndBuildAsync(DateTimeOffset runTime)
    {
        await HtDatabase.ResetAsync(database.ConnectionString, None);
        var collection = new ServiceCollection()
            .AddLogging()
            .AddHtApplication()
            .AddHtPersistence(database.ConnectionString);
        collection.AddSingleton<TimeProvider>(new FakeTimeProvider(runTime));
        return collection.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
    }

    /// <summary>The seed catalog with demo loading, then demo history, demo report and demo pause loading, each creating its account.</summary>
    private static async Task LoadEarlierTasksAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var provider = scope.ServiceProvider;
        Assert.Equal(DemoLoadingOutcome.Loaded, (await provider.GetRequiredService<DemoDataService>().LoadAsync(DemoPassword, None)).Outcome);
        Assert.Equal(DemoAccountOutcome.Created, Assert.Single((await provider.GetRequiredService<DemoHistoryService>().LoadAsync(DemoPassword, None)).Accounts).Outcome);
        Assert.Equal(DemoAccountOutcome.Created, Assert.Single((await provider.GetRequiredService<DemoReportService>().LoadAsync(DemoPassword, None)).Accounts).Outcome);
        Assert.Equal(DemoAccountOutcome.Created, Assert.Single((await provider.GetRequiredService<DemoPauseService>().LoadAsync(DemoPassword, None)).Accounts).Outcome);
    }

    private static async Task<DemoGoalChangeLoadingResult> LoadDemoGoalChangesAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<DemoGoalChangeService>().LoadAsync(DemoPassword, None);
    }

    /// <summary>
    /// Each account's goals with their consistency and streak texts and whether they are paused or ended, its dashboard
    /// counts and its goal history, as one line each.
    /// </summary>
    private static async Task<List<string>> FiguresAsync(IServiceProvider services, IEnumerable<string> emails)
    {
        var lines = new List<string>();
        foreach (var email in emails)
        {
            var user = await SignInAsync(services, email);
            var overview = await OverviewAsync(services, user);
            lines.AddRange(overview.Goals.Select(g =>
                $"{email} {g.Item.Name}: {g.Stats.ConsistencyText}, {g.Stats.CurrentStreakText}, {g.Stats.LongestStreakText}, {g.PausedUntil}, {g.Item.IsEnded}"));
            var dashboard = await DashboardAsync(services, user);
            lines.Add($"{email} dashboard: {dashboard.Counts}");
            lines.Add($"{email} goal history: {(await GoalHistoryAsync(services, user)).Count}");
        }

        return lines;
    }

    private static async Task<ActingUser> SignInAsync(IServiceProvider services, string email)
    {
        await using var scope = services.CreateAsyncScope();
        var signIn = await scope.ServiceProvider.GetRequiredService<AccountService>().SignInAsync(email, DemoPassword, None);
        Assert.Equal(SignInOutcome.SignedIn, signIn.Outcome);
        return signIn.ActingUser!;
    }

    private static async Task<IReadOnlyList<GoalListItem>> ListAsync(IServiceProvider services, ActingUser user)
    {
        await using var scope = services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<GoalService>().ListAsync(user, None);
        Assert.Equal(GoalListOutcome.Found, result.Outcome);
        return result.Goals;
    }

    private static async Task<GoalDetails> DetailsAsync(IServiceProvider services, ActingUser user, GoalId goal)
    {
        await using var scope = services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<GoalService>().GetAsync(user, goal, None);
        Assert.Equal(GoalReadOutcome.Found, result.Outcome);
        return result.Goal!;
    }

    private static async Task<GoalsOverview> OverviewAsync(IServiceProvider services, ActingUser user)
    {
        await using var scope = services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<ReportService>().GetGoalsOverviewAsync(user, None);
        Assert.Equal(GoalsOverviewOutcome.Found, result.Outcome);
        return result.Overview!;
    }

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

    private static async Task<IReadOnlyList<AttemptView>> AttemptsAsync(IServiceProvider services, ActingUser user, GoalId goal)
    {
        await using var scope = services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<AttemptService>().GetAttemptsAsync(user, goal, None);
        Assert.Equal(AttemptListOutcome.Found, result.Outcome);
        return result.Attempts;
    }

    private static async Task<IReadOnlyList<EndedAttemptView>> GoalHistoryAsync(IServiceProvider services, ActingUser user)
    {
        await using var scope = services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<AttemptService>().GetGoalHistoryAsync(user, None);
        Assert.Equal(GoalHistoryListOutcome.Found, result.Outcome);
        return result.Entries;
    }

    private static async Task<IReadOnlyList<HistoryEntry>> HistoryAsync(
        IServiceProvider services, ActingUser user, GoalId goal, DateOnly first, DateOnly last)
    {
        await using var scope = services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<RecordingService>().GetHistoryAsync(user, goal, first, last, None);
        Assert.Equal(HistoryOutcome.Found, result.Outcome);
        return result.History!.Entries;
    }

    /// <summary>A catalog habit made by the administrator: a Draft, published when <paramref name="publish"/> is true.</summary>
    private static async Task CreateCatalogHabitAsync(IServiceProvider services, string name, Category category, Intent intent, bool publish)
    {
        await using var scope = services.CreateAsyncScope();
        var admin = scope.ServiceProvider.GetRequiredService<CatalogAdministrationService>();
        var created = await admin.CreateAsync(
            Administrator, new CatalogHabitInput { Name = name, Category = category, Intent = intent, Icon = "brain" }, None);
        Assert.Equal(CatalogHabitCreateOutcome.Created, created.Outcome);
        if (publish)
        {
            var published = await admin.PublishAsync(Administrator, created.HabitId!.Value, None);
            Assert.Equal(CatalogHabitPublishOutcome.Published, published.Outcome);
        }
    }

    /// <summary>A count over the rows <c>c</c> of <paramref name="table"/> whose goal <c>t</c> belongs to the account <c>u</c>.</summary>
    private static string GoalChild(string table) =>
        $"SELECT COUNT(*) FROM [{table}] c JOIN [Goals] t ON t.[Id] = c.[GoalId] JOIN [AspNetUsers] u ON u.[Id] = t.[OwnerId] WHERE 1 = 1";

    /// <summary>Runs <paramref name="sql"/>, which joins the account as <c>u</c>, for demo7's account only.</summary>
    private async Task<int> CountForAsync(string sql)
    {
        var scoped = sql.Contains(" WHERE ", StringComparison.Ordinal) ? sql + " AND u.[UserName] = @user" : sql + " WHERE u.[UserName] = @user";
        return (int)(await Sql.ScalarAsync(database.ConnectionString, scoped, ("@user", Demo7)))!;
    }

    /// <summary>The endings, attempts after the first and settings versions after the first of every account but demo7's.</summary>
    private async Task<int> ChangesOfOthersAsync() =>
        (int)(await Sql.ScalarAsync(
            database.ConnectionString,
            """
            SELECT (SELECT COUNT(*) FROM [GoalAttempts] a JOIN [Goals] g ON g.[Id] = a.[GoalId] JOIN [AspNetUsers] u ON u.[Id] = g.[OwnerId]
                    WHERE u.[UserName] <> @user AND (a.[EndingDate] IS NOT NULL OR a.[Outcome] IS NOT NULL OR a.[EndingNote] IS NOT NULL OR a.[Number] > 1))
                 + (SELECT COUNT(*) FROM [GoalSettingsVersions] v JOIN [Goals] g ON g.[Id] = v.[GoalId] JOIN [AspNetUsers] u ON u.[Id] = g.[OwnerId]
                    WHERE u.[UserName] <> @user AND (v.[AttemptNumber] > 1 OR v.[AppliesFrom] <> g.[StartDate]))
            """,
            ("@user", Demo7)))!;

    private async Task<int> CountAsync(string table) =>
        (int)(await Sql.ScalarAsync(database.ConnectionString, $"SELECT COUNT(*) FROM [{table}]"))!;

    /// <summary>The goal, version, attempt, record, note, excusal, pause and account rows, each as one line of its columns in key order.</summary>
    private async Task<List<string>> SnapshotAsync() =>
    [
        .. await LinesAsync("Goals", "SELECT * FROM [Goals] ORDER BY [Id]"),
        .. await LinesAsync("Versions", "SELECT * FROM [GoalSettingsVersions] ORDER BY [GoalId], [AttemptNumber], [AppliesFrom]"),
        .. await LinesAsync("Attempts", "SELECT * FROM [GoalAttempts] ORDER BY [GoalId], [Number]"),
        .. await LinesAsync("Records", "SELECT * FROM [GoalRecords] ORDER BY [Sequence]"),
        .. await LinesAsync("Notes", "SELECT * FROM [GoalNotes] ORDER BY [GoalId], [EntryDate]"),
        .. await LinesAsync("Excusals", "SELECT * FROM [GoalExcusals] ORDER BY [GoalId], [Date]"),
        .. await LinesAsync("Pauses", "SELECT * FROM [GoalPauses] ORDER BY [Id]"),
        .. await LinesAsync("Accounts", "SELECT [UserName], [Name], [TimeZone], [CreatedAt], [GoalsRevision] FROM [AspNetUsers] ORDER BY [UserName]"),
    ];

    private async Task<List<string>> LinesAsync(string table, string sql)
    {
        var rows = await Sql.QueryAsync(database.ConnectionString, sql);
        return rows
            .Select(row => table + " " + string.Join(" | ", row.OrderBy(c => c.Key, StringComparer.Ordinal).Select(c => $"{c.Key}={c.Value}")))
            .ToList();
    }
}
