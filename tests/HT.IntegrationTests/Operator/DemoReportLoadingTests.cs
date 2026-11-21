using HT.Application.Accounts;
using HT.Application.Goals;
using HT.Application.Habits;
using HT.Application.Operator;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using HT.Domain.Time;
using Microsoft.Extensions.DependencyInjection;

namespace HT.IntegrationTests.Operator;

/// <summary>
/// Slice 6 US8-AS7, AS9, AS10, FR-031, FR-032, HP-007 and research R-09 on SQL Server: demo report loading, after demo
/// loading and demo history loading, creates <c>demo5@example.com</c> with the goals, records and notes of FR-032 in
/// one transaction, through the same writer as demo history loading. Each test starts from a reset <c>HT-IT-SEED</c>
/// database with <see cref="SeedDatabaseFixture.ResetAndBuildAsync"/>, whose clock is never moved: 09:00 UTC on Saturday
/// 14 November 2026 with the default zone UTC, so the run date R is 14 November and the goals start on R − 84, Saturday
/// 22 August. The names this class creates itself have the prefix "It Dr" or "it-dr-".
/// </summary>
[Collection(SeedDatabaseCollection.Name)]
public sealed class DemoReportLoadingTests(SeedDatabaseFixture database)
{
    private const string DemoPassword = "example2026";
    private const string Demo5 = "demo5@example.com";

    private static readonly DateOnly Start = new(2026, 8, 22);
    private static readonly DateOnly RunDate = new(2026, 11, 14);
    private static readonly string[] DemoHabits = ["Walking", "Strength training", "Screen time"];

    private static readonly ActingUser Administrator = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.Administrator);

    [Fact]
    public async Task US8_AS7_after_demo_loading_and_demo_history_loading_demo_report_loading_creates_demo5_with_the_FR_032_goals()
    {
        await using var services = await database.ResetAndBuildAsync();
        await LoadDemoDataAsync(services);
        await LoadDemoHistoryAsync(services);

        var result = await LoadDemoReportAsync(services);

        Assert.Equal(DemoReportLoadingOutcome.Loaded, result.Outcome);
        Assert.Null(result.Message);
        var account = Assert.Single(result.Accounts);
        Assert.Equal(Demo5, account.Email);
        Assert.Equal(DemoAccountOutcome.Created, account.Outcome);
        Assert.Equal("created", account.Message);
        Assert.Equal("demo goals added", account.Goals!.Message);
        Assert.Equal("demo records added", account.Records!.Message);

        var user = await SignInAsync(services);
        Assert.Equal(Role.RegisteredUser, user.Role);
        var goals = await ListAsync(services, user);
        Assert.Equal(InNameOrder([.. DemoReport.GoalNames]), goals.Select(g => g.Name));
        foreach (var item in goals)
        {
            var goal = await GetAsync(services, user, item.Id);
            Assert.Equal(Start, goal.StartDate);
            Assert.Equal(Start, goal.AttemptStartDate);
            Assert.Equal(Start, goal.TimeZoneAppliesFrom);
            Assert.Equal(1, goal.AttemptNumber);
            Assert.Equal("UTC", goal.TimeZone);
            Assert.False(goal.HasOwnName);
            Assert.True(goal.Habit.IsInCatalog);
        }

        await using var scope = services.CreateAsyncScope();
        Assert.True(Email.TryParse(Demo5, out var email));
        var stored = await scope.ServiceProvider.GetRequiredService<IAccountStore>().FindByEmailAsync(email, CancellationToken.None);
        Assert.NotNull(stored);
        Assert.Null(stored.Name);
        Assert.Equal("UTC", stored.StoredTimeZone);
        Assert.Equal(database.Clock.GetUtcNow(), stored.CreatedAt);
    }

    [Fact]
    public async Task US8_AS7_the_records_and_notes_are_FR_032s_in_list_order()
    {
        await using var services = await database.ResetAndBuildAsync();
        await LoadDemoDataAsync(services);
        await LoadDemoHistoryAsync(services);
        await LoadDemoReportAsync(services);

        var expected = DemoReport.For(RunDate);
        foreach (var (habit, goal) in DemoHabits.Zip(expected))
        {
            // The stored order is the list's: by date, as the store wrote them one at a time.
            var rows = await Sql.QueryAsync(
                database.ConnectionString,
                """
                SELECT r.[Date], r.[Answer], r.[Amount] FROM [GoalRecords] r JOIN [Goals] g ON g.[Id] = r.[GoalId]
                JOIN [AspNetUsers] u ON u.[Id] = g.[OwnerId] JOIN [CatalogHabits] h ON h.[Id] = g.[CatalogHabitId]
                WHERE u.[UserName] = @user AND h.[Name] = @habit ORDER BY r.[Sequence]
                """,
                ("@user", Demo5),
                ("@habit", habit));
            Assert.Equal(goal.Records.Select(r => (r.Date, r.Value.Amount)), rows.Select(row => (DateOnly.FromDateTime((DateTime)row["Date"]!), (int?)row["Amount"])));
        }

        var notes = await Sql.QueryAsync(
            database.ConnectionString,
            """
            SELECT n.[EntryDate], n.[Text] FROM [GoalNotes] n JOIN [Goals] g ON g.[Id] = n.[GoalId]
            JOIN [AspNetUsers] u ON u.[Id] = g.[OwnerId] WHERE u.[UserName] = @user ORDER BY n.[EntryDate]
            """,
            ("@user", Demo5));

        // R − 31 is Wednesday 14 October; week 4 starts on Monday 12 October; the latest Walking commitment of R − 49 to
        // R − 43 (26 September to 2 October) is Friday 2 October.
        Assert.Equal(
            [
                (new DateOnly(2026, 10, 2), "On holiday."),
                (new DateOnly(2026, 10, 12), "Sore shoulder."),
                (new DateOnly(2026, 10, 14), "Long train ride."),
            ],
            notes.Select(row => (DateOnly.FromDateTime((DateTime)row["EntryDate"]!), (string)row["Text"]!)));
    }

    [Fact]
    public async Task US8_AS9_a_second_run_gives_already_exists_with_no_goal_or_record_result_and_changes_nothing()
    {
        await using var services = await database.ResetAndBuildAsync();
        await LoadDemoDataAsync(services);
        await LoadDemoHistoryAsync(services);
        await LoadDemoReportAsync(services);
        var once = await SnapshotAsync();

        var second = await LoadDemoReportAsync(services);

        var account = Assert.Single(second.Accounts);
        Assert.Equal(Demo5, account.Email);
        Assert.Equal(DemoAccountOutcome.AlreadyExists, account.Outcome);
        Assert.Equal("already exists", account.Message);
        Assert.Null(account.Goals);
        Assert.Null(account.Records);
        Assert.Equal(once, await SnapshotAsync());
    }

    [Theory]
    [InlineData(null, "no demo password configured")]
    [InlineData("examplepass", "demo password does not meet the password rule")]
    public async Task US8_AS10_no_or_a_weak_demo_password_creates_nothing(string? password, string message)
    {
        await using var services = await database.ResetAndBuildAsync();
        await LoadDemoDataAsync(services);

        DemoReportLoadingResult result;
        await using (var scope = services.CreateAsyncScope())
        {
            result = await scope.ServiceProvider.GetRequiredService<DemoReportService>().LoadAsync(password, CancellationToken.None);
        }

        Assert.Equal(message, result.Message);
        Assert.Empty(result.Accounts);
        Assert.Equal(0, await Sql.CountUsersAsync(database.ConnectionString, Demo5));
    }

    [Fact]
    public async Task A_goal_whose_habit_is_not_Published_is_left_out_with_its_records_and_notes()
    {
        await using var services = await database.ResetAndBuildAsync();
        await CreateCatalogHabitAsync(services, "Walking", Category.Body, Intent.Build, publish: true);
        await CreateCatalogHabitAsync(services, "SCREEN TIME", Category.Mind, Intent.Reduce, publish: false);

        var result = await LoadDemoReportAsync(services);

        // "Strength training" is not in the catalog at all, "Screen time" is a Draft: only Walking is written.
        var account = Assert.Single(result.Accounts);
        Assert.Equal(DemoAccountOutcome.Created, account.Outcome);
        Assert.Equal("demo goals added", account.Goals!.Message);
        Assert.Equal("demo records added", account.Records!.Message);
        Assert.Equal(["Walking, 30 min, 3 days a week"], (await ListAsync(services, await SignInAsync(services))).Select(g => g.Name));
        Assert.Equal(1, await CountAsync("Goals"));
        Assert.Equal(DemoReport.For(RunDate)[0].Records.Count, await CountAsync("GoalRecords"));
        Assert.Equal(1, await CountAsync("GoalNotes"));
    }

    [Fact]
    public async Task A_write_failure_rolls_the_account_back_and_its_exception_and_log_name_no_goal_habit_note_or_amount()
    {
        const string email = "it-dr-rollback@example.com";
        const int amount = 8317;
        await using (var arrange = await database.ResetAndBuildAsync())
        {
            await CreateCatalogHabitAsync(arrange, "It Dr Zqhabit", Category.Mind, Intent.Build, publish: true);
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
        Assert.True(GoalName.TryCreate("It Dr Zqgoal", out var goalName, out _));
        var day = new DateOnly(2026, 11, 10);
        Assert.True(RecordValue.TryCreate(Measurement.Count, null, amount, out var value, out _));

        // Two notes under the same entry key break the GoalNotes primary key, after the account, goal and record are written.
        var goal = new DemoHistoryGoal(
            DemoGoal.OnCatalogHabit("It Dr Zqhabit", [CatalogHabitState.Published], target, goalName, Start),
            [new DemoRecord(day, value)],
            [new DemoNote(day, NoteOf("It Dr Zqnote one")), new DemoNote(day, NoteOf("It Dr Zqnote two"))]);

        InvalidOperationException exception;
        await using (var scope = services.CreateAsyncScope())
        {
            exception = await Assert.ThrowsAsync<InvalidOperationException>(() => scope.ServiceProvider.GetRequiredService<IDemoReportStore>()
                .CreateDemoReportAccountAsync(
                    parsed, DemoPassword, new DemoAccountProfile(null, NamedTimeZone.Utc, database.Clock.GetUtcNow()), [goal],
                    CancellationToken.None));
        }

        Assert.Equal(0, await Sql.CountUsersAsync(database.ConnectionString, email));
        Assert.Equal(0, await CountAsync("Goals"));
        Assert.Equal(0, await CountAsync("GoalSettingsVersions"));
        Assert.Equal(0, await CountAsync("GoalRecords"));
        Assert.Equal(0, await CountAsync("GoalNotes"));
        Assert.Null(exception.InnerException);
        Assert.Contains("(SQL error 2627)", exception.Message, StringComparison.Ordinal);
        string[] values = ["Zq", "zqunits", amount.ToString(System.Globalization.CultureInfo.InvariantCulture)];
        Assert.DoesNotContain(values, v => exception.ToString().Contains(v, StringComparison.OrdinalIgnoreCase));
        capture.AssertContainsNone(values);
    }

    [Fact]
    public async Task FR_031_FR_032_no_result_or_log_entry_names_a_demo5_goal_habit_note_or_amount_and_the_earlier_tasks_never_name_demo5()
    {
        await (await database.ResetAndBuildAsync()).DisposeAsync();
        using var capture = new LogCapture();
        await using var services = capture.BuildServices(database.ConnectionString);

        var demo = await LoadDemoDataAsync(services);
        var history = await LoadDemoHistoryAsync(services);
        var first = await LoadDemoReportAsync(services);
        var second = await LoadDemoReportAsync(services);

        Assert.DoesNotContain(Demo5, DemoDataService.DemoEmails);
        Assert.DoesNotContain(demo.Accounts, a => a.Email.Contains("demo5", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(history.Accounts, a => a.Email.Contains("demo5", StringComparison.OrdinalIgnoreCase));

        // The habit names alone are left out: the demo catalog's own lines are not in this check, and the goal names hold them.
        string[] values =
        [
            .. DemoReport.GoalNames, DemoReport.WalkingNote, DemoReport.StrengthNote, DemoReport.ScreenTimeNote,
            "18 min", "30 min", "35 min", "90 min", "100 min", "110 min", "150 min",
        ];
        capture.AssertContainsNone(values);
        foreach (var result in new[] { first, second })
        {
            var texts = result.Accounts
                .SelectMany(a => new[] { a.Email, a.Message, a.Goals?.Message, a.Records?.Message })
                .Append(result.Message)
                .OfType<string>();
            Assert.All(texts, text => Assert.DoesNotContain(values, value => text.Contains(value, StringComparison.OrdinalIgnoreCase)));
            Assert.All(result.Accounts, a => Assert.Equal(Demo5, a.Email));
        }
    }

    private static async Task<DemoLoadingResult> LoadDemoDataAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<DemoDataService>().LoadAsync(DemoPassword, CancellationToken.None);
    }

    private static async Task<DemoHistoryLoadingResult> LoadDemoHistoryAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<DemoHistoryService>().LoadAsync(DemoPassword, CancellationToken.None);
    }

    private static async Task<DemoReportLoadingResult> LoadDemoReportAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<DemoReportService>().LoadAsync(DemoPassword, CancellationToken.None);
    }

    private static async Task<ActingUser> SignInAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var signIn = await scope.ServiceProvider.GetRequiredService<AccountService>().SignInAsync(Demo5, DemoPassword, CancellationToken.None);
        Assert.Equal(SignInOutcome.SignedIn, signIn.Outcome);
        return signIn.ActingUser!;
    }

    private static async Task<IReadOnlyList<GoalListItem>> ListAsync(IServiceProvider services, ActingUser user)
    {
        await using var scope = services.CreateAsyncScope();
        var list = await scope.ServiceProvider.GetRequiredService<GoalService>().ListAsync(user, CancellationToken.None);
        Assert.Equal(GoalListOutcome.Found, list.Outcome);
        return list.Goals;
    }

    private static async Task<GoalDetails> GetAsync(IServiceProvider services, ActingUser user, GoalId id)
    {
        await using var scope = services.CreateAsyncScope();
        var read = await scope.ServiceProvider.GetRequiredService<GoalService>().GetAsync(user, id, CancellationToken.None);
        Assert.Equal(GoalReadOutcome.Found, read.Outcome);
        return read.Goal!;
    }

    private static NoteText NoteOf(string text)
    {
        Assert.True(NoteText.TryCreate(text, out var note, out _));
        return note!;
    }

    /// <summary>The names in the list's name order: the keys of <see cref="HabitName.KeyOf"/>, compared ordinally.</summary>
    private static List<string> InNameOrder(params string[] names) =>
        [.. names.OrderBy(HabitName.KeyOf, StringComparer.Ordinal)];

    /// <summary>A catalog habit made by the administrator: a Draft, published when <paramref name="publish"/> is true.</summary>
    private static async Task CreateCatalogHabitAsync(IServiceProvider services, string name, Category category, Intent intent, bool publish)
    {
        await using var scope = services.CreateAsyncScope();
        var admin = scope.ServiceProvider.GetRequiredService<CatalogAdministrationService>();
        var created = await admin.CreateAsync(
            Administrator, new CatalogHabitInput { Name = name, Category = category, Intent = intent, Icon = "brain" }, CancellationToken.None);
        Assert.Equal(CatalogHabitCreateOutcome.Created, created.Outcome);
        if (publish)
        {
            var published = await admin.PublishAsync(Administrator, created.HabitId!.Value, CancellationToken.None);
            Assert.Equal(CatalogHabitPublishOutcome.Published, published.Outcome);
        }
    }

    private async Task<int> CountAsync(string table) =>
        (int)(await Sql.ScalarAsync(database.ConnectionString, $"SELECT COUNT(*) FROM [{table}]"))!;

    /// <summary>The goal, record, note and account rows, each as one line of its columns in key order.</summary>
    private async Task<List<string>> SnapshotAsync() =>
    [
        .. await LinesAsync("Goals", "SELECT * FROM [Goals] ORDER BY [Id]"),
        .. await LinesAsync("Versions", "SELECT * FROM [GoalSettingsVersions] ORDER BY [GoalId], [AppliesFrom]"),
        .. await LinesAsync("Attempts", "SELECT * FROM [GoalAttempts] ORDER BY [GoalId], [Number]"),
        .. await LinesAsync("Records", "SELECT * FROM [GoalRecords] ORDER BY [Sequence]"),
        .. await LinesAsync("Notes", "SELECT * FROM [GoalNotes] ORDER BY [GoalId], [EntryDate]"),
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
