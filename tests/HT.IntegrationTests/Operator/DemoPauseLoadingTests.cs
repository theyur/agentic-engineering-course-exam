using HT.Application;
using HT.Application.Accounts;
using HT.Application.Exemptions;
using HT.Application.Habits;
using HT.Application.Operator;
using HT.Application.Records;
using HT.Application.Reports;
using HT.Domain.Accounts;
using HT.Domain.Evaluation;
using HT.Domain.Exemptions;
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
/// Slice 7 US6-AS1 to AS5, FR-030, FR-031, SC-006 and research R-08 on SQL Server: demo pause loading, after the seed
/// catalog and the three earlier demo tasks (demo loading, demo history loading, demo report loading), creates
/// <c>demo6@example.com</c> with the goals, records, notes, excusals and pauses of FR-031 in one transaction, and
/// <c>demo6</c>'s reports read at the run time give FR-031's figures on every weekday. Each test resets
/// <c>HT-IT-SEED</c> with <see cref="HtDatabase.ResetAsync"/> and builds its own provider with its own
/// <see cref="FakeTimeProvider"/>, so <see cref="SeedDatabaseFixture.Clock"/> is never set or moved. The default zone is
/// UTC, so the run date R is the UTC date of the run time. The names this class creates itself have the prefix "It Dp"
/// or "it-dp-".
/// </summary>
[Collection(SeedDatabaseCollection.Name)]
public sealed class DemoPauseLoadingTests(SeedDatabaseFixture database)
{
    private const string DemoPassword = "example2026";
    private const string Demo6 = "demo6@example.com";
    private const string Meditation = "Meditation, 10 min daily";
    private const string Water = "Drinking water, 8 glasses a day";

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
    public async Task US6_AS1_AS2_SC_006_demo6_is_created_and_its_reports_give_the_figures_of_FR_031(DateTimeOffset runTime, DayOfWeek day)
    {
        var r = DateOnly.FromDateTime(runTime.UtcDateTime);
        Assert.Equal(day, r.DayOfWeek);
        await using var services = await ResetAndBuildAsync(runTime);
        await LoadEarlierTasksAsync(services);

        var result = await LoadDemoPausesAsync(services);

        Assert.Equal(DemoPauseLoadingOutcome.Loaded, result.Outcome);
        Assert.Null(result.Message);
        var account = Assert.Single(result.Accounts);
        Assert.Equal(Demo6, account.Email);
        Assert.Equal(DemoAccountOutcome.Created, account.Outcome);
        Assert.Equal("created", account.Message);
        Assert.Equal("demo goals added", account.Goals!.Message);
        Assert.Equal("demo records added", account.Records!.Message);

        var demo6 = await SignInAsync(services, Demo6);
        Assert.Equal(Role.RegisteredUser, demo6.Role);
        var overview = await OverviewAsync(services, demo6);
        var goals = overview.Goals.ToDictionary(g => g.Item.Name, StringComparer.Ordinal);
        Assert.Equal([Water, Meditation], goals.Keys.Order(StringComparer.Ordinal));
        Assert.Null(goals[Meditation].PausedUntil);
        Assert.Equal(r.AddDays(5), goals[Water].PausedUntil);

        var meditation = await ReportAsync(services, demo6, goals[Meditation].Item.Id);
        Assert.Equal("95% (22 of 23)", meditation.Stats.ConsistencyText);
        Assert.Equal(("10 commitments", "12 commitments"), (meditation.Stats.CurrentStreakText, meditation.Stats.LongestStreakText));
        var water = await ReportAsync(services, demo6, goals[Water].Item.Id);
        Assert.Equal("94% (18 of 19)", water.Stats.ConsistencyText);
        Assert.Equal(("2 days", "16 days"), (water.Stats.CurrentStreakText, water.Stats.LongestStreakText));

        // The newest entries: Meditation R − 2 Excused, R − 1 Pending, R Due; Drinking water R − 2 Pending, R − 1 and R Paused.
        Assert.Equal(
            [(r.AddDays(-2), EvaluationState.Excused), (r.AddDays(-1), EvaluationState.Pending), (r, EvaluationState.Due)],
            (await HistoryAsync(services, demo6, goals[Meditation].Item.Id, r.AddDays(-2), r)).Select(e => (e.FirstDate, e.State)));
        Assert.Equal(
            [(r.AddDays(-2), EvaluationState.Pending), (r.AddDays(-1), EvaluationState.Paused), (r, EvaluationState.Paused)],
            (await HistoryAsync(services, demo6, goals[Water].Item.Id, r.AddDays(-2), r)).Select(e => (e.FirstDate, e.State)));

        var dashboard = await DashboardAsync(services, demo6);
        Assert.Equal(new DashboardCounts { Due = 1, Pending = 2, MissedThisWeek = 0, Goals = 1 }, dashboard.Counts);
    }

    [Fact]
    public async Task US6_AS2_the_history_excusals_pauses_and_notes_are_FR_031s()
    {
        var r = new DateOnly(2026, 11, 18);
        await using var services = await ResetAndBuildAsync(Wednesday);
        await LoadEarlierTasksAsync(services);
        await LoadDemoPausesAsync(services);

        var demo6 = await SignInAsync(services, Demo6);
        var goals = (await OverviewAsync(services, demo6)).Goals.ToDictionary(g => g.Item.Name, g => g.Item.Id, StringComparer.Ordinal);

        // Meditation, R − 28 (21 October) to R: Met except the Excused 29 and 30 October ("Travelling."), 10 and 16
        // November (no reason), the Missed 4 November with its note, the Pending 17 November and the Due 18 November.
        var meditation = await HistoryAsync(services, demo6, goals[Meditation], r.AddDays(-28), r);
        Assert.Equal(29, meditation.Count);
        Assert.Equal(
            [
                (new DateOnly(2026, 10, 29), EvaluationState.Excused, "Travelling."),
                (new DateOnly(2026, 10, 30), EvaluationState.Excused, "Travelling."),
                (new DateOnly(2026, 11, 4), EvaluationState.Missed, null),
                (new DateOnly(2026, 11, 10), EvaluationState.Excused, null),
                (new DateOnly(2026, 11, 16), EvaluationState.Excused, null),
                (new DateOnly(2026, 11, 17), EvaluationState.Pending, null),
                (r, EvaluationState.Due, null),
            ],
            meditation.Where(e => e.State != EvaluationState.Met).Select(e => (e.FirstDate, e.State, e.ExcusalReason)));
        Assert.All(meditation.Where(e => e.State == EvaluationState.Met), e => Assert.Equal(10, Assert.Single(e.Records).Amount));
        Assert.All(meditation.Where(e => e.State != EvaluationState.Met), e => Assert.Empty(e.Records));
        Assert.Equal([(new DateOnly(2026, 11, 4), "Overslept.")], meditation.Where(e => e.Note is not null).Select(e => (e.FirstDate, e.Note!)));

        // Drinking water: Paused 31 October to 6 November and 17 to 18 November; Failed 13 November (6, "Busy day.");
        // Pending 16 November with its 8 glasses; Met otherwise.
        var water = await HistoryAsync(services, demo6, goals[Water], r.AddDays(-28), r);
        Assert.Equal(29, water.Count);
        Assert.Equal(
            [
                .. Enumerable.Range(0, 7).Select(i => (new DateOnly(2026, 10, 31).AddDays(i), EvaluationState.Paused, (int?)null)),
                (new DateOnly(2026, 11, 13), EvaluationState.Failed, 6),
                (new DateOnly(2026, 11, 16), EvaluationState.Pending, 8),
                (new DateOnly(2026, 11, 17), EvaluationState.Paused, null),
                (r, EvaluationState.Paused, null),
            ],
            water.Where(e => e.State != EvaluationState.Met).Select(e => (e.FirstDate, e.State, e.Records.SingleOrDefault()?.Amount)));
        Assert.All(water.Where(e => e.State == EvaluationState.Met), e => Assert.Equal(8, Assert.Single(e.Records).Amount));
        Assert.Equal([(new DateOnly(2026, 11, 13), "Busy day.")], water.Where(e => e.Note is not null).Select(e => (e.FirstDate, e.Note!)));

        var pauses = await PausesAsync(services, demo6, goals[Water]);
        Assert.Equal(
            [
                (new DateOnly(2026, 11, 17), new DateOnly(2026, 11, 23), "Family visit.", PauseTiming.Current),
                (new DateOnly(2026, 10, 31), new DateOnly(2026, 11, 6), "Ill with flu.", PauseTiming.Past),
            ],
            pauses.Select(p => (p.FirstDay, p.LastDay, p.Reason, p.Timing)));
        Assert.Empty(await PausesAsync(services, demo6, goals[Meditation]));
    }

    [Fact]
    public async Task US6_AS1_the_account_has_the_default_zone_no_name_and_the_run_time_as_its_creation_time()
    {
        await using var services = await ResetAndBuildAsync(Wednesday);
        await LoadEarlierTasksAsync(services);
        await LoadDemoPausesAsync(services);

        await using var scope = services.CreateAsyncScope();
        Assert.True(Email.TryParse(Demo6, out var email));
        var stored = await scope.ServiceProvider.GetRequiredService<IAccountStore>().FindByEmailAsync(email, None);
        Assert.NotNull(stored);
        Assert.Equal(Role.RegisteredUser, stored.Role);
        Assert.Null(stored.Name);
        Assert.Equal("UTC", stored.StoredTimeZone);
        Assert.Equal(Wednesday, stored.CreatedAt);
        Assert.Equal(0, await CountForAsync("CustomHabits", "OwnerId"));
    }

    [Fact]
    public async Task US6_AS3_a_second_run_gives_already_exists_with_no_goal_or_record_result_and_changes_nothing()
    {
        await using var services = await ResetAndBuildAsync(Wednesday);
        await LoadEarlierTasksAsync(services);
        await LoadDemoPausesAsync(services);
        var once = await SnapshotAsync();

        var second = await LoadDemoPausesAsync(services);

        Assert.Equal(DemoPauseLoadingOutcome.Loaded, second.Outcome);
        var account = Assert.Single(second.Accounts);
        Assert.Equal(Demo6, account.Email);
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

        DemoPauseLoadingResult result;
        await using (var scope = services.CreateAsyncScope())
        {
            result = await scope.ServiceProvider.GetRequiredService<DemoPauseService>().LoadAsync(password, None);
        }

        Assert.Equal(message, result.Message);
        Assert.Empty(result.Accounts);
        Assert.Equal(0, await Sql.CountUsersAsync(database.ConnectionString, Demo6));
        Assert.Equal(0, await CountAsync("GoalExcusals"));
        Assert.Equal(0, await CountAsync("GoalPauses"));
    }

    [Fact]
    public async Task A_goal_whose_habit_is_not_Published_is_left_out_with_its_records_notes_excusals_and_pauses()
    {
        await using var services = await ResetAndBuildAsync(Wednesday);
        await CreateCatalogHabitAsync(services, "MEDITATION", Category.Mind, Intent.Build, publish: true);
        await CreateCatalogHabitAsync(services, "Drinking Water", Category.Body, Intent.Build, publish: false);

        var result = await LoadDemoPausesAsync(services);

        var account = Assert.Single(result.Accounts);
        Assert.Equal(DemoAccountOutcome.Created, account.Outcome);
        Assert.Equal("demo goals added", account.Goals!.Message);
        Assert.Equal("demo records added", account.Records!.Message);
        var demo6 = await SignInAsync(services, Demo6);
        Assert.Equal(["MEDITATION, 10 min daily"], (await OverviewAsync(services, demo6)).Goals.Select(g => g.Item.Name));
        var meditation = DemoPauses.For(new DateOnly(2026, 11, 18))[0];
        Assert.Equal(1, await CountAsync("Goals"));
        Assert.Equal(meditation.History.Records.Count, await CountAsync("GoalRecords"));
        Assert.Equal(1, await CountAsync("GoalNotes"));
        Assert.Equal(meditation.Excusals.Count, await CountAsync("GoalExcusals"));
        Assert.Equal(0, await CountAsync("GoalPauses"));
    }

    [Fact]
    public async Task A_write_failure_after_the_records_and_notes_rolls_the_account_back_and_names_no_goal_habit_note_reason_or_amount()
    {
        const string email = "it-dp-rollback@example.com";
        const int amount = 7213;
        await using (var arrange = await ResetAndBuildAsync(Wednesday))
        {
            await CreateCatalogHabitAsync(arrange, "It Dp Zqhabit", Category.Mind, Intent.Build, publish: true);
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
        Assert.True(GoalName.TryCreate("It Dp Zqgoal", out var goalName, out _));
        var start = new DateOnly(2026, 10, 21);
        var day = new DateOnly(2026, 11, 10);
        Assert.True(RecordValue.TryCreate(Measurement.Count, null, amount, out var value, out _));
        Assert.True(NoteText.TryCreate("It Dp Zqnote", out var note, out _));
        Assert.True(ExemptionReason.TryCreate("It Dp Zqreason", out var reason, out _));

        // Two notes under the same entry key break the GoalNotes primary key, after the account, the goal, its record and
        // its first note are written, and before the excusal and the pause. (Two excusals on the same date, or two pauses
        // that share a date, are refused by DemoPauseGoal before anything is written.)
        var goal = new DemoPauseGoal(
            new DemoHistoryGoal(
                DemoGoal.OnCatalogHabit("It Dp Zqhabit", [CatalogHabitState.Published], target, goalName, start),
                [new DemoRecord(day, value)],
                [new DemoNote(day, note!), new DemoNote(day, note!)]),
            [new DemoExcusal(day.AddDays(1), reason)],
            [new DemoGoalPause(day.AddDays(3), day.AddDays(4), reason)]);

        InvalidOperationException exception;
        await using (var scope = services.CreateAsyncScope())
        {
            exception = await Assert.ThrowsAsync<InvalidOperationException>(() => scope.ServiceProvider.GetRequiredService<IDemoPauseStore>()
                .CreateDemoPauseAccountAsync(parsed, DemoPassword, new DemoAccountProfile(null, NamedTimeZone.Utc, Wednesday), [goal], None));
        }

        Assert.Equal(0, await Sql.CountUsersAsync(database.ConnectionString, email));
        Assert.Equal(0, await CountAsync("Goals"));
        Assert.Equal(0, await CountAsync("GoalSettingsVersions"));
        Assert.Equal(0, await CountAsync("GoalRecords"));
        Assert.Equal(0, await CountAsync("GoalNotes"));
        Assert.Equal(0, await CountAsync("GoalExcusals"));
        Assert.Equal(0, await CountAsync("GoalPauses"));
        Assert.Null(exception.InnerException);
        Assert.StartsWith("Demo pause account creation failed", exception.Message, StringComparison.Ordinal);
        Assert.Contains("(SQL error 2627)", exception.Message, StringComparison.Ordinal);
        string[] values = ["Zq", "zqunits", amount.ToString(System.Globalization.CultureInfo.InvariantCulture)];
        Assert.DoesNotContain(values, v => exception.ToString().Contains(v, StringComparison.OrdinalIgnoreCase));
        capture.AssertContainsNone(values);
    }

    [Fact]
    public async Task US6_AS5_FR_030_the_earlier_demo_tasks_write_no_excusal_or_pause_and_their_figures_stay_the_same()
    {
        await using var services = await ResetAndBuildAsync(Wednesday);
        await LoadEarlierTasksAsync(services);
        Assert.Equal(0, await CountAsync("GoalExcusals"));
        Assert.Equal(0, await CountAsync("GoalPauses"));
        string[] earlier = [.. DemoDataService.DemoEmails, DemoHistoryService.DemoEmail, DemoReportService.DemoEmail];
        var before = await FiguresAsync(services, earlier);

        await LoadDemoPausesAsync(services);

        Assert.Equal(before, await FiguresAsync(services, earlier));
        var foreign = await Sql.ScalarAsync(
            database.ConnectionString,
            """
            SELECT (SELECT COUNT(*) FROM [GoalExcusals] e JOIN [Goals] g ON g.[Id] = e.[GoalId] JOIN [AspNetUsers] u ON u.[Id] = g.[OwnerId] WHERE u.[UserName] <> @user)
                 + (SELECT COUNT(*) FROM [GoalPauses] p JOIN [Goals] g ON g.[Id] = p.[GoalId] JOIN [AspNetUsers] u ON u.[Id] = g.[OwnerId] WHERE u.[UserName] <> @user)
            """,
            ("@user", Demo6));
        Assert.Equal(0, (int)foreign!);
        Assert.Equal(4, await CountAsync("GoalExcusals"));
        Assert.Equal(2, await CountAsync("GoalPauses"));
    }

    [Fact]
    public async Task FR_031_no_result_or_log_entry_names_a_demo6_goal_habit_note_reason_or_amount()
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

        var first = await LoadDemoPausesAsync(services);
        var second = await LoadDemoPausesAsync(services);

        // The habit names alone are left out: the demo catalog's own lines are not in this check, and the goal names hold them.
        string[] values =
        [
            .. DemoPauses.GoalNames, DemoPauses.TravellingReason, DemoPauses.IllnessReason, DemoPauses.FamilyVisitReason,
            DemoPauses.OversleptNote, DemoPauses.BusyDayNote, "glasses", "10 min",
        ];
        capture.AssertContainsNone(values);
        foreach (var result in new[] { first, second })
        {
            var texts = result.Accounts
                .SelectMany(a => new[] { a.Email, a.Message, a.Goals?.Message, a.Records?.Message })
                .Append(result.Message)
                .OfType<string>();
            Assert.All(texts, text => Assert.DoesNotContain(values, value => text.Contains(value, StringComparison.OrdinalIgnoreCase)));
            Assert.All(result.Accounts, a => Assert.Equal(Demo6, a.Email));
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

    /// <summary>The seed catalog with demo loading, then demo history loading and demo report loading, each creating its account.</summary>
    private static async Task LoadEarlierTasksAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var provider = scope.ServiceProvider;
        Assert.Equal(DemoLoadingOutcome.Loaded, (await provider.GetRequiredService<DemoDataService>().LoadAsync(DemoPassword, None)).Outcome);
        Assert.Equal(DemoAccountOutcome.Created, Assert.Single((await provider.GetRequiredService<DemoHistoryService>().LoadAsync(DemoPassword, None)).Accounts).Outcome);
        Assert.Equal(DemoAccountOutcome.Created, Assert.Single((await provider.GetRequiredService<DemoReportService>().LoadAsync(DemoPassword, None)).Accounts).Outcome);
    }

    private static async Task<DemoPauseLoadingResult> LoadDemoPausesAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<DemoPauseService>().LoadAsync(DemoPassword, None);
    }

    /// <summary>Each account's goals with their consistency and streak texts and whether they are paused, as one line per goal.</summary>
    private static async Task<List<string>> FiguresAsync(IServiceProvider services, IEnumerable<string> emails)
    {
        var lines = new List<string>();
        foreach (var email in emails)
        {
            var user = await SignInAsync(services, email);
            var overview = await OverviewAsync(services, user);
            lines.AddRange(overview.Goals.Select(g =>
                $"{email} {g.Item.Name}: {g.Stats.ConsistencyText}, {g.Stats.CurrentStreakText}, {g.Stats.LongestStreakText}, {g.PausedUntil}"));
            var dashboard = await DashboardAsync(services, user);
            lines.Add($"{email} dashboard: {dashboard.Counts}");
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

    private static async Task<IReadOnlyList<HistoryEntry>> HistoryAsync(
        IServiceProvider services, ActingUser user, GoalId goal, DateOnly first, DateOnly last)
    {
        await using var scope = services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<RecordingService>().GetHistoryAsync(user, goal, first, last, None);
        Assert.Equal(HistoryOutcome.Found, result.Outcome);
        return result.History!.Entries;
    }

    private static async Task<IReadOnlyList<PauseView>> PausesAsync(IServiceProvider services, ActingUser user, GoalId goal)
    {
        await using var scope = services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<PauseService>().GetPausesAsync(user, goal, None);
        Assert.Equal(PausesOutcome.Found, result.Outcome);
        return result.Pauses;
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

    private async Task<int> CountAsync(string table) =>
        (int)(await Sql.ScalarAsync(database.ConnectionString, $"SELECT COUNT(*) FROM [{table}]"))!;

    /// <summary>The rows of <paramref name="table"/> whose <paramref name="ownerColumn"/> is demo6's account.</summary>
    private async Task<int> CountForAsync(string table, string ownerColumn) =>
        (int)(await Sql.ScalarAsync(
            database.ConnectionString,
            $"SELECT COUNT(*) FROM [{table}] t JOIN [AspNetUsers] u ON u.[Id] = t.[{ownerColumn}] WHERE u.[UserName] = @user",
            ("@user", Demo6)))!;

    /// <summary>The goal, record, note, excusal, pause and account rows, each as one line of its columns in key order.</summary>
    private async Task<List<string>> SnapshotAsync() =>
    [
        .. await LinesAsync("Goals", "SELECT * FROM [Goals] ORDER BY [Id]"),
        .. await LinesAsync("Versions", "SELECT * FROM [GoalSettingsVersions] ORDER BY [GoalId], [AppliesFrom]"),
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
