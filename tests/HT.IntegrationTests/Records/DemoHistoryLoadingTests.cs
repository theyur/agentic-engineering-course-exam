using HT.Application.Accounts;
using HT.Application.Goals;
using HT.Application.Habits;
using HT.Application.Operator;
using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using HT.Domain.Time;
using Microsoft.Extensions.DependencyInjection;

namespace HT.IntegrationTests.Records;

/// <summary>
/// Slice 5 US7-AS1 to AS3 and AS5, FR-030, FR-031, HP-006 and research R-08 on SQL Server: demo history loading, after
/// demo loading, creates <c>demo4@example.com</c> with the goals, records and notes of FR-030 in one transaction, and
/// demo loading stays as slices 1 and 4 specify it. Each test starts from a reset <c>HT-IT-SEED</c> database. The
/// fixture's clock is 09:00 UTC on Saturday 14 November 2026 and the default zone is UTC, so the run date R is 14
/// November and the goals start on R − 28, Saturday 17 October. Yoga's current week starts on Monday 9 November, week
/// 1 on 2 November, week 2 on 26 October and week 3 on 19 October, its first week. At the run time a day's grace
/// period has ended for the days up to R − 3 (11 November, whose grace period ended at 00:00 on 14 November); R − 2
/// and R − 1 are Pending and R is Due. The names this class creates itself have the prefix "It Dh" or "it-dh-".
/// </summary>
[Collection(SeedDatabaseCollection.Name)]
public sealed class DemoHistoryLoadingTests(SeedDatabaseFixture database)
{
    private const string DemoPassword = "example2026";
    private const string Demo4 = "demo4@example.com";

    private static readonly DateOnly Start = new(2026, 10, 17);
    private static readonly DateOnly RunDate = new(2026, 11, 14);

    private static readonly ActingUser Administrator = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.Administrator);

    [Fact]
    public async Task US7_AS1_after_demo_loading_demo_history_loading_creates_demo4_with_the_FR_030_goals()
    {
        await using var services = await database.ResetAndBuildAsync();
        await LoadDemoDataAsync(services);

        var result = await LoadDemoHistoryAsync(services);

        Assert.Equal(DemoHistoryLoadingOutcome.Loaded, result.Outcome);
        var account = Assert.Single(result.Accounts);
        Assert.Equal(Demo4, account.Email);
        Assert.Equal(DemoAccountOutcome.Created, account.Outcome);
        Assert.Equal("created", account.Message);
        Assert.Equal("demo goals added", account.Goals!.Message);
        Assert.Equal("demo records added", account.Records!.Message);

        var user = await SignInAsync(services);
        Assert.Equal(Role.RegisteredUser, user.Role);
        var goals = await ListAsync(services, user);
        Assert.Equal(InNameOrder([.. DemoHistory.GoalNames]), goals.Select(g => g.Name));
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
        Assert.True(Email.TryParse(Demo4, out var email));
        var stored = await scope.ServiceProvider.GetRequiredService<IAccountStore>().FindByEmailAsync(email, CancellationToken.None);
        Assert.NotNull(stored);
        Assert.Null(stored.Name);
        Assert.Equal("UTC", stored.StoredTimeZone);
        Assert.Equal(database.Clock.GetUtcNow(), stored.CreatedAt);
    }

    [Fact]
    public async Task US7_AS1_the_records_and_notes_are_FR_030s_in_list_order()
    {
        await using var services = await database.ResetAndBuildAsync();
        await LoadDemoDataAsync(services);
        await LoadDemoHistoryAsync(services);

        // Reading: 25 records of R − 28 to R − 3 without R − 10, plus R − 2. Sugar: 25, without R − 12. Yoga: 5.
        Assert.Equal(26, await CountRecordsAsync("Reading, 20 min daily"));
        Assert.Equal(25, await CountRecordsAsync("Sugar, at most 1 sweet a day"));
        Assert.Equal(5, await CountRecordsAsync("Yoga, 3 times a week"));

        // The stored order is the list's: by date, as the store wrote them one at a time.
        var rows = await Sql.QueryAsync(
            database.ConnectionString,
            """
            SELECT r.[Date], r.[Amount] FROM [GoalRecords] r JOIN [Goals] g ON g.[Id] = r.[GoalId]
            JOIN [AspNetUsers] u ON u.[Id] = g.[OwnerId] JOIN [CatalogHabits] h ON h.[Id] = g.[CatalogHabitId]
            WHERE u.[UserName] = @user AND h.[Name] = N'Reading' ORDER BY r.[Sequence]
            """,
            ("@user", Demo4));
        var expected = DemoHistory.For(RunDate)[0].Records.Select(r => (r.Date, r.Value.Amount)).ToList();
        Assert.Equal(expected, rows.Select(row => (DateOnly.FromDateTime((DateTime)row["Date"]!), (int?)row["Amount"])));

        var notes = await Sql.QueryAsync(
            database.ConnectionString,
            """
            SELECT n.[EntryDate], n.[Text] FROM [GoalNotes] n JOIN [Goals] g ON g.[Id] = n.[GoalId]
            JOIN [AspNetUsers] u ON u.[Id] = g.[OwnerId] WHERE u.[UserName] = @user ORDER BY n.[EntryDate]
            """,
            ("@user", Demo4));
        Assert.Equal(
            [
                (new DateOnly(2026, 10, 26), "Knee was sore."),
                (new DateOnly(2026, 11, 4), "Away for the weekend."),
                (new DateOnly(2026, 11, 8), "Fell asleep early."),
                (new DateOnly(2026, 11, 10), "Birthday cake at work."),
            ],
            notes.Select(row => (DateOnly.FromDateTime((DateTime)row["EntryDate"]!), (string)row["Text"]!)));
    }

    [Fact]
    public async Task US7_AS2_Reading_shows_Met_Failed_with_12_minutes_Missed_Pending_and_Due_with_its_notes()
    {
        await using var services = await database.ResetAndBuildAsync();
        await LoadDemoDataAsync(services);
        await LoadDemoHistoryAsync(services);

        var history = await HistoryAsync(services, "Reading, 20 min daily");

        Assert.Equal(29, history.Entries.Count);
        Assert.Equal(RunDate, history.CurrentDate);
        AssertEntry(history, new DateOnly(2026, 10, 17), EvaluationState.Met, [20], null);
        AssertEntry(history, new DateOnly(2026, 11, 4), EvaluationState.Missed, [], "Away for the weekend.");
        AssertEntry(history, new DateOnly(2026, 11, 8), EvaluationState.Failed, [12], "Fell asleep early.");
        AssertEntry(history, new DateOnly(2026, 11, 11), EvaluationState.Met, [20], null);
        AssertEntry(history, new DateOnly(2026, 11, 12), EvaluationState.Pending, [25], null);
        AssertEntry(history, new DateOnly(2026, 11, 13), EvaluationState.Pending, [], null);
        AssertEntry(history, RunDate, EvaluationState.Due, [], null);
        Assert.Equal(24, history.Entries.Count(e => e.State == EvaluationState.Met));

        var failed = Assert.Single(history.Entries, e => e.State == EvaluationState.Failed);
        Assert.Equal("12 min", RecordingText.Recorded(history, Assert.Single(failed.Records)));
        Assert.Equal("Due today", RecordingText.State(EvaluationState.Due, history.GoalScheduleKind, history.Period));
    }

    [Fact]
    public async Task US7_AS2_Sugar_shows_Met_Failed_with_3_sweets_Missed_Pending_and_Due_with_its_note()
    {
        await using var services = await database.ResetAndBuildAsync();
        await LoadDemoDataAsync(services);
        await LoadDemoHistoryAsync(services);

        var history = await HistoryAsync(services, "Sugar, at most 1 sweet a day");

        Assert.Equal(29, history.Entries.Count);

        // R − 28 is 17 October, k = 28 is even: 0 sweets; R − 27, k = 27, 1 sweet: both within the limit.
        AssertEntry(history, new DateOnly(2026, 10, 17), EvaluationState.Met, [0], null);
        AssertEntry(history, new DateOnly(2026, 10, 18), EvaluationState.Met, [1], null);
        AssertEntry(history, new DateOnly(2026, 11, 2), EvaluationState.Missed, [], null);
        AssertEntry(history, new DateOnly(2026, 11, 10), EvaluationState.Failed, [3], "Birthday cake at work.");
        AssertEntry(history, new DateOnly(2026, 11, 12), EvaluationState.Pending, [], null);
        AssertEntry(history, new DateOnly(2026, 11, 13), EvaluationState.Pending, [], null);
        AssertEntry(history, RunDate, EvaluationState.Due, [], null);

        var failed = Assert.Single(history.Entries, e => e.State == EvaluationState.Failed);
        Assert.Equal("3 sweet", RecordingText.Recorded(history, Assert.Single(failed.Records)));
    }

    [Fact]
    public async Task US7_AS2_Yoga_shows_Missed_week_3_Failed_week_2_with_its_note_Met_week_1_and_the_current_week_Due()
    {
        await using var services = await database.ResetAndBuildAsync();
        await LoadDemoDataAsync(services);
        await LoadDemoHistoryAsync(services);

        var history = await HistoryAsync(services, "Yoga, 3 times a week");

        Assert.Equal(
            [new DateOnly(2026, 10, 19), new DateOnly(2026, 10, 26), new DateOnly(2026, 11, 2), new DateOnly(2026, 11, 9)],
            history.Entries.Select(e => e.FirstDate));
        Assert.Equal(
            [EvaluationState.Missed, EvaluationState.Failed, EvaluationState.Met, EvaluationState.Due],
            history.Entries.Select(e => e.State));
        Assert.Equal([new DateOnly(2026, 10, 26), new DateOnly(2026, 10, 28)], history.Entries[1].Records.Select(r => r.Date));
        Assert.Equal("Knee was sore.", history.Entries[1].Note);
        Assert.Equal(
            [new DateOnly(2026, 11, 2), new DateOnly(2026, 11, 4), new DateOnly(2026, 11, 6)],
            history.Entries[2].Records.Select(r => r.Date));
        Assert.All(history.Entries.SelectMany(e => e.Records), r => Assert.Equal(YesNoAnswer.Yes, r.Answer));
        Assert.Empty(history.Entries[0].Records);
        Assert.Empty(history.Entries[3].Records);
        Assert.Equal("Due this week", RecordingText.State(EvaluationState.Due, history.GoalScheduleKind, history.Period));
    }

    [Fact]
    public async Task US7_AS3_a_second_run_gives_already_exists_with_no_goal_or_record_result_and_changes_nothing()
    {
        await using var services = await database.ResetAndBuildAsync();
        await LoadDemoDataAsync(services);
        await LoadDemoHistoryAsync(services);
        var once = await SnapshotAsync();

        var second = await LoadDemoHistoryAsync(services);

        var account = Assert.Single(second.Accounts);
        Assert.Equal(DemoAccountOutcome.AlreadyExists, account.Outcome);
        Assert.Equal("already exists", account.Message);
        Assert.Null(account.Goals);
        Assert.Null(account.Records);
        Assert.Equal(once, await SnapshotAsync());
    }

    [Fact]
    public async Task US7_AS3_a_demo4_registered_by_a_person_is_left_as_it_is()
    {
        await using var services = await database.ResetAndBuildAsync();
        await LoadDemoDataAsync(services);
        await using (var scope = services.CreateAsyncScope())
        {
            var registered = await scope.ServiceProvider.GetRequiredService<AccountService>()
                .RegisterAsync("Demo4@Example.com", "owner2026", CancellationToken.None);
            Assert.Equal(RegistrationOutcome.Registered, registered.Outcome);
        }

        var result = await LoadDemoHistoryAsync(services);

        Assert.Equal(DemoAccountOutcome.AlreadyExists, Assert.Single(result.Accounts).Outcome);
        Assert.Empty(await ListAsync(services, await SignInAsync(services, "owner2026")));
        Assert.Equal(0, await CountRecordsOfAsync(Demo4));
    }

    [Fact]
    public async Task A_goal_whose_habit_is_not_Published_is_left_out_with_its_records_and_notes()
    {
        await using var services = await database.ResetAndBuildAsync();
        await CreateCatalogHabitAsync(services, "Reading", Category.Mind, Intent.Build, publish: true);
        await CreateCatalogHabitAsync(services, "SUGAR", Category.Health, Intent.Reduce, publish: false);

        var result = await LoadDemoHistoryAsync(services);

        var account = Assert.Single(result.Accounts);
        Assert.Equal(DemoAccountOutcome.Created, account.Outcome);
        Assert.Equal("demo goals added", account.Goals!.Message);
        Assert.Equal("demo records added", account.Records!.Message);
        Assert.Equal(["Reading, 20 min daily"], (await ListAsync(services, await SignInAsync(services))).Select(g => g.Name));
        Assert.Equal(1, await CountAsync("Goals"));
        Assert.Equal(26, await CountAsync("GoalRecords"));
        Assert.Equal(2, await CountAsync("GoalNotes"));
    }

    [Fact]
    public async Task A_write_failure_rolls_the_account_back_and_its_exception_and_log_name_no_goal_habit_note_or_amount()
    {
        const string email = "it-dh-rollback@example.com";
        const int amount = 9173;
        await using (var arrange = await database.ResetAndBuildAsync())
        {
            await CreateCatalogHabitAsync(arrange, "It Dh Zqhabit", Category.Mind, Intent.Build, publish: true);
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
        Assert.True(GoalName.TryCreate("It Dh Zqgoal", out var goalName, out _));
        var day = new DateOnly(2026, 11, 10);
        Assert.True(RecordValue.TryCreate(Measurement.Count, null, amount, out var value, out _));

        // Two notes under the same entry key break the GoalNotes primary key, after the account, goal and record are written.
        var goal = new DemoHistoryGoal(
            DemoGoal.OnCatalogHabit("It Dh Zqhabit", [CatalogHabitState.Published], target, goalName, Start),
            [new DemoRecord(day, value)],
            [new DemoNote(day, NoteOf("It Dh Zqnote one")), new DemoNote(day, NoteOf("It Dh Zqnote two"))]);

        InvalidOperationException exception;
        await using (var scope = services.CreateAsyncScope())
        {
            exception = await Assert.ThrowsAsync<InvalidOperationException>(() => scope.ServiceProvider.GetRequiredService<IDemoHistoryStore>()
                .CreateDemoHistoryAccountAsync(
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
    public async Task HP_006_no_result_or_log_entry_of_demo_history_loading_names_a_goal_habit_note_unit_or_amount()
    {
        await (await database.ResetAndBuildAsync()).DisposeAsync();
        using var capture = new LogCapture();
        await using var services = capture.BuildServices(database.ConnectionString);

        await LoadDemoDataAsync(services);
        var first = await LoadDemoHistoryAsync(services);
        var second = await LoadDemoHistoryAsync(services);

        // "Reading" alone is left out: EF Core's own data-reader messages contain the word.
        string[] values =
        [
            .. DemoHistory.GoalNames, "Sugar", "Yoga", DemoHistory.ReadingNote, "Away for the weekend.", "Birthday cake at work.",
            "Knee was sore.", "sweet", "12 min", "20 min", "25 min",
        ];
        capture.AssertContainsNone(values);
        foreach (var result in new[] { first, second })
        {
            var texts = result.Accounts
                .SelectMany(a => new[] { a.Email, a.Message, a.Goals?.Message, a.Records?.Message })
                .Append(result.Message)
                .OfType<string>();
            Assert.All(texts, text => Assert.DoesNotContain(values, value => text.Contains(value, StringComparison.OrdinalIgnoreCase)));
        }
    }

    [Fact]
    public async Task FR_031_demo_loading_is_unchanged_never_names_demo4_and_its_accounts_get_no_records()
    {
        await using var services = await database.ResetAndBuildAsync();

        var demo = await LoadDemoDataAsync(services);
        await LoadDemoHistoryAsync(services);
        var again = await LoadDemoDataAsync(services);

        Assert.Equal(DemoDataService.DemoEmails, demo.Accounts.Select(a => a.Email));
        Assert.Equal(DemoDataService.DemoEmails, again.Accounts.Select(a => a.Email));
        Assert.All(demo.Accounts, a =>
        {
            Assert.Equal(DemoAccountOutcome.Created, a.Outcome);
            Assert.Equal("demo goals added", a.Goals!.Message);
        });
        Assert.All(again.Accounts, a => Assert.Equal(DemoAccountOutcome.AlreadyExists, a.Outcome));
        Assert.DoesNotContain(Demo4, DemoDataService.DemoEmails);
        foreach (var email in DemoDataService.DemoEmails)
        {
            Assert.Equal(InNameOrder([.. DemoGoals.DisplayNames(email)]), (await ListAsync(services, await SignInAsync(services, DemoPassword, email))).Select(g => g.Name));
            Assert.Equal(0, await CountRecordsOfAsync(email));
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

    private static async Task<ActingUser> SignInAsync(IServiceProvider services, string password = DemoPassword, string email = Demo4)
    {
        await using var scope = services.CreateAsyncScope();
        var signIn = await scope.ServiceProvider.GetRequiredService<AccountService>().SignInAsync(email, password, CancellationToken.None);
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

    /// <summary>Demo4's history of the goal with this name from its start date to the run date, at the run time.</summary>
    private static async Task<GoalHistory> HistoryAsync(IServiceProvider services, string goalName)
    {
        var user = await SignInAsync(services);
        var goal = Assert.Single(await ListAsync(services, user), g => g.Name == goalName);
        await using var scope = services.CreateAsyncScope();
        var history = await scope.ServiceProvider.GetRequiredService<RecordingService>()
            .GetHistoryAsync(user, goal.Id, Start, RunDate, CancellationToken.None);
        Assert.Equal(HistoryOutcome.Found, history.Outcome);
        return history.History!;
    }

    private static void AssertEntry(GoalHistory history, DateOnly date, EvaluationState state, int[] amounts, string? note)
    {
        var entry = Assert.Single(history.Entries, e => e.FirstDate == date);
        Assert.Equal(state, entry.State);
        Assert.Equal(amounts, entry.Records.Select(r => r.Amount ?? -1));
        Assert.Equal(note, entry.Note);
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

    /// <summary>The records of demo4's goal whose catalog habit has the name before the first comma of <paramref name="goalName"/>.</summary>
    private async Task<int> CountRecordsAsync(string goalName) =>
        (int)(await Sql.ScalarAsync(
            database.ConnectionString,
            """
            SELECT COUNT(*) FROM [GoalRecords] r JOIN [Goals] g ON g.[Id] = r.[GoalId]
            JOIN [AspNetUsers] u ON u.[Id] = g.[OwnerId] JOIN [CatalogHabits] h ON h.[Id] = g.[CatalogHabitId]
            WHERE u.[UserName] = @user AND h.[Name] = @habit
            """,
            ("@user", Demo4),
            ("@habit", goalName[..goalName.IndexOf(',', StringComparison.Ordinal)])))!;

    /// <summary>The records of every goal of the account with this user name.</summary>
    private async Task<int> CountRecordsOfAsync(string userName) =>
        (int)(await Sql.ScalarAsync(
            database.ConnectionString,
            """
            SELECT COUNT(*) FROM [GoalRecords] r JOIN [Goals] g ON g.[Id] = r.[GoalId]
            JOIN [AspNetUsers] u ON u.[Id] = g.[OwnerId] WHERE u.[UserName] = @user
            """,
            ("@user", userName)))!;

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
