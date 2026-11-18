using HT.Application.Accounts;
using HT.Application.Goals;
using HT.Application.Habits;
using HT.Application.Operator;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Time;
using Microsoft.Extensions.DependencyInjection;

namespace HT.IntegrationTests.Operator;

/// <summary>
/// Slice 4 US9-AS1 to US9-AS5, FR-040 and research R-10 on SQL Server: demo loading gives each demo account it creates
/// its profile and its demo goals, in the account's transaction, after the seed catalog. Each test starts from a reset
/// <c>HT-IT-SEED</c> database. The fixture's clock is 09:00 UTC on Saturday 14 November 2026 and the default zone is
/// UTC, so the day of the run is 14 November.
/// </summary>
[Collection(SeedDatabaseCollection.Name)]
public sealed class DemoGoalsTests(SeedDatabaseFixture database)
{
    private const string DemoPassword = "example2026";
    private const string Demo1 = "demo1@example.com";
    private const string Demo2 = "demo2@example.com";
    private const string Demo3 = "demo3@example.com";

    private static readonly DateOnly DayOfTheRun = new(2026, 11, 14);

    private static readonly ActingUser Administrator = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.Administrator);

    [Fact]
    public async Task US9_AS1_each_created_demo_account_lists_its_FR_040_goals_and_has_demo_goals_added()
    {
        await using var services = await database.ResetAndBuildAsync();

        var result = await LoadAsync(services);

        Assert.Equal(DemoCatalogOutcome.SeedCatalogLoaded, result.Catalog!.Outcome);
        Assert.All(result.Accounts, a =>
        {
            Assert.Equal(DemoAccountOutcome.Created, a.Outcome);
            Assert.Equal(DemoGoalsOutcome.Added, a.Goals!.Outcome);
            Assert.Equal("demo goals added", a.Goals.Message);
        });
        Assert.Equal(InNameOrder("Reading, 15 min, 5 days a week", "No phone in bed, avoided every day"), await GoalNamesAsync(services, Demo1));
        Assert.Equal(InNameOrder("Morning yoga", "Meditation, 10 min, 3 days a week"), await GoalNamesAsync(services, Demo2));
        Assert.Equal(
            InNameOrder("Sugar, at most 1 sweet a day", "Late snacks, at most once a day", "Coffee, at most 2 cups a day"),
            await GoalNamesAsync(services, Demo3));
        foreach (var email in DemoDataService.DemoEmails)
        {
            Assert.Equal(InNameOrder([.. DemoGoals.DisplayNames(email)]), await GoalNamesAsync(services, email));
        }
    }

    [Fact]
    public async Task US9_AS1_each_demo_goal_has_version_1_in_the_default_zone_attempt_1_and_its_start_date()
    {
        await using var services = await database.ResetAndBuildAsync();
        await LoadAsync(services);

        foreach (var email in DemoDataService.DemoEmails)
        {
            var user = await SignInAsync(services, email);
            foreach (var item in await ListAsync(services, user))
            {
                var goal = await GetAsync(services, user, item.Id);
                var expectedStart = item.Name.StartsWith("Meditation", StringComparison.Ordinal) ? new DateOnly(2026, 11, 21) : DayOfTheRun;
                Assert.Equal(expectedStart, goal.StartDate);
                Assert.Equal(expectedStart, goal.AttemptStartDate);
                Assert.Equal(expectedStart, goal.TimeZoneAppliesFrom);
                Assert.Equal(1, goal.AttemptNumber);
                Assert.Equal("UTC", goal.TimeZone);
                Assert.Null(goal.LaterTimeZone);
            }
        }

        Assert.Equal(7, await CountAsync("Goals"));
        Assert.Equal(7, await CountAsync("GoalSettingsVersions"));
        Assert.Equal(7, await CountAsync("GoalAttempts"));
    }

    [Fact]
    public async Task US9_AS5_Coffee_is_no_longer_in_the_catalog_Morning_yoga_is_on_Yoga_and_demo1_is_named_Demo_One()
    {
        await using var services = await database.ResetAndBuildAsync();
        await LoadAsync(services);

        var demo3 = await SignInAsync(services, Demo3);
        var demo3Goals = await ListAsync(services, demo3);
        var coffee = await GetAsync(services, demo3, Assert.Single(demo3Goals, g => g.Name == "Coffee, at most 2 cups a day").Id);
        Assert.Equal(HabitKind.Catalog, coffee.Habit.Kind);
        Assert.Equal("Coffee", coffee.Habit.Name);
        Assert.False(coffee.Habit.IsInCatalog);
        var sugar = await GetAsync(services, demo3, Assert.Single(demo3Goals, g => g.Name == "Sugar, at most 1 sweet a day").Id);
        Assert.True(sugar.Habit.IsInCatalog);

        // The custom-habit goal is on the account's own "Late snacks", by the identifier written in the same transaction.
        var lateSnacks = await GetAsync(services, demo3, Assert.Single(demo3Goals, g => g.Name == "Late snacks, at most once a day").Id);
        Assert.Equal(HabitKind.Custom, lateSnacks.Habit.Kind);
        var ownHabits = await ListCustomHabitsAsync(services, demo3);
        Assert.Equal(Assert.Single(ownHabits, h => h.Name == "Late snacks").Id, lateSnacks.Habit.Id);

        var demo2 = await SignInAsync(services, Demo2);
        var yoga = await GetAsync(services, demo2, Assert.Single(await ListAsync(services, demo2), g => g.Name == "Morning yoga").Id);
        Assert.True(yoga.HasOwnName);
        Assert.Equal("Yoga", yoga.Habit.Name);
        Assert.Equal(GoalScheduleKind.PeriodBased, yoga.GoalScheduleKind);

        await using var scope = services.CreateAsyncScope();
        var accounts = scope.ServiceProvider.GetRequiredService<IAccountStore>();
        var now = database.Clock.GetUtcNow();
        foreach (var (email, name) in new[] { (Demo1, "Demo One"), (Demo2, (string?)null), (Demo3, null) })
        {
            Assert.True(Email.TryParse(email, out var parsed));
            var account = await accounts.FindByEmailAsync(parsed, CancellationToken.None);
            Assert.NotNull(account);
            Assert.Equal(name, account.Name);
            Assert.Equal("UTC", account.StoredTimeZone);
            Assert.Equal(now, account.CreatedAt);
        }
    }

    [Fact]
    public async Task US9_AS2_a_second_run_gives_no_goal_result_and_leaves_the_goals_unchanged()
    {
        await using var services = await database.ResetAndBuildAsync();
        await LoadAsync(services);
        var once = await SnapshotAsync();

        var second = await LoadAsync(services);

        Assert.All(second.Accounts, a =>
        {
            Assert.Equal(DemoAccountOutcome.AlreadyExists, a.Outcome);
            Assert.Null(a.Goals);
        });
        Assert.Equal(once, await SnapshotAsync());
        Assert.Equal(7, once.Count(line => line.StartsWith("Goals ", StringComparison.Ordinal)));
    }

    [Fact]
    public async Task US9_AS2_a_demo_email_registered_by_a_person_gets_no_goal_and_no_profile_change()
    {
        await using var services = await database.ResetAndBuildAsync();
        await using (var scope = services.CreateAsyncScope())
        {
            var registered = await scope.ServiceProvider.GetRequiredService<AccountService>()
                .RegisterAsync(Demo2, "owner2026", CancellationToken.None);
            Assert.Equal(RegistrationOutcome.Registered, registered.Outcome);
        }

        var result = await LoadAsync(services);

        var demo2 = Assert.Single(result.Accounts, a => a.Email == Demo2);
        Assert.Equal(DemoAccountOutcome.AlreadyExists, demo2.Outcome);
        Assert.Null(demo2.Goals);
        Assert.Empty(await ListAsync(services, await SignInAsync(services, Demo2, "owner2026")));
        Assert.Equal(InNameOrder([.. DemoGoals.DisplayNames(Demo1)]), await GoalNamesAsync(services, Demo1));
    }

    [Fact]
    public async Task US9_AS3_with_an_administrators_catalog_only_the_goals_on_qualifying_habits_are_written()
    {
        await using var services = await database.ResetAndBuildAsync();
        await CreateCatalogHabitAsync(services, "Yoga", Category.Body, Intent.Build, publish: true);
        await CreateCatalogHabitAsync(services, "Meditation", Category.Mind, Intent.Build, publish: true);
        await CreateCatalogHabitAsync(services, "Sugar", Category.Health, Intent.Reduce, publish: true);
        await CreateCatalogHabitAsync(services, "Reading", Category.Mind, Intent.Build, publish: false);
        await CreateCatalogHabitAsync(services, "Coffee", Category.Health, Intent.Reduce, publish: false);

        var result = await LoadAsync(services);

        Assert.Equal(DemoCatalogOutcome.CatalogNotEmpty, result.Catalog!.Outcome);
        Assert.All(result.Accounts, a => Assert.Equal("demo goals added", a.Goals!.Message));
        Assert.Equal(["No phone in bed, avoided every day"], await GoalNamesAsync(services, Demo1));
        Assert.Equal(InNameOrder("Morning yoga", "Meditation, 10 min, 3 days a week"), await GoalNamesAsync(services, Demo2));
        Assert.Equal(InNameOrder("Sugar, at most 1 sweet a day", "Late snacks, at most once a day"), await GoalNamesAsync(services, Demo3));
    }

    [Fact]
    public async Task A_catalog_habit_is_found_by_its_name_ignoring_case_and_a_goal_on_an_absent_habit_is_skipped()
    {
        await using var services = await database.ResetAndBuildAsync();
        await CreateCatalogHabitAsync(services, "MEDITATION", Category.Mind, Intent.Build, publish: true);

        await LoadAsync(services);

        Assert.Equal(["MEDITATION, 10 min, 3 days a week"], await GoalNamesAsync(services, Demo2));
        Assert.Equal(["No phone in bed, avoided every day"], await GoalNamesAsync(services, Demo1));
        Assert.Equal(["Late snacks, at most once a day"], await GoalNamesAsync(services, Demo3));
    }

    [Fact]
    public async Task US9_AS4_no_result_and_no_log_entry_names_a_goal_a_habit_an_account_name_or_a_setting()
    {
        await (await database.ResetAndBuildAsync()).DisposeAsync();
        using var capture = new LogCapture();
        await using var services = capture.BuildServices(database.ConnectionString);

        var first = await LoadAsync(services);
        var second = await LoadAsync(services);

        // "Reading" alone is left out: EF Core's own data-reader messages contain the word.
        string[] values =
        [
            .. DemoDataService.DemoEmails.SelectMany(DemoGoals.DisplayNames),
            "Morning yoga", "No phone in bed", "Late snacks", "Yoga", "Meditation", "Sugar", "Coffee", "Demo One",
            "15 min", "10 min", "sweet", "cups", "avoided every day", "at most once a day", "5 days a week", "3 days a week",
        ];
        capture.AssertContainsNone(values);
        foreach (var result in new[] { first, second })
        {
            var texts = result.Accounts
                .SelectMany(a => new[] { a.Email, a.Message, a.CustomHabits?.Message, a.Goals?.Message })
                .Append(result.Catalog?.Message)
                .OfType<string>();
            Assert.All(texts, text => Assert.DoesNotContain(values, value => text.Contains(value, StringComparison.OrdinalIgnoreCase)));
        }
    }

    [Fact]
    public async Task A_goal_on_a_position_outside_the_custom_habits_rolls_the_account_back_and_names_no_value()
    {
        const string email = "it-demo-goal-rollback@example.com";
        await using var services = await database.ResetAndBuildAsync();
        Assert.True(Email.TryParse(email, out var parsed));
        Assert.True(HabitDefinition.TryCreate("It Demo Zq Ritual", null, Category.Mind, Intent.Build, "brain", out var habit, out _));
        Assert.True(GoalTarget.TryCreate(
            new GoalTargetFields
            {
                BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo,
                ScheduleKind = ScheduleKind.Times, Times = 2, Period = SchedulePeriod.Week,
            },
            out var target,
            out _));
        Assert.True(GoalName.TryCreate("It Demo Zq private goal", out var goalName, out _));
        var goals = new[] { DemoGoal.OnCustomHabit(0, target, goalName, DayOfTheRun), DemoGoal.OnCustomHabit(1, target, null, DayOfTheRun) };

        InvalidOperationException exception;
        await using (var scope = services.CreateAsyncScope())
        {
            exception = await Assert.ThrowsAsync<InvalidOperationException>(() => scope.ServiceProvider.GetRequiredService<IDemoDataStore>()
                .CreateDemoAccountAsync(
                    parsed, DemoPassword, [habit], new DemoAccountProfile("It Demo Zq Name", NamedTimeZone.Utc, database.Clock.GetUtcNow()), goals,
                    CancellationToken.None));
        }

        Assert.Equal(0, await Sql.CountUsersAsync(database.ConnectionString, email));
        Assert.Equal(0, await CountAsync("CustomHabits"));
        Assert.Equal(0, await CountAsync("Goals"));
        Assert.Null(exception.InnerException);
        Assert.DoesNotContain("Zq", exception.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task The_store_writes_a_catalog_goal_only_in_an_accepted_state()
    {
        const string email = "it-demo-goal-states@example.com";
        await using var services = await database.ResetAndBuildAsync();
        await CreateCatalogHabitAsync(services, "It Demo Zq Draft", Category.Mind, Intent.Build, publish: false);
        await CreateCatalogHabitAsync(services, "It Demo Zq Published", Category.Mind, Intent.Build, publish: true);
        Assert.True(Email.TryParse(email, out var parsed));
        Assert.True(GoalTarget.TryCreate(
            new GoalTargetFields
            {
                BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo,
                ScheduleKind = ScheduleKind.Times, Times = 2, Period = SchedulePeriod.Week,
            },
            out var target,
            out _));
        DemoGoal[] goals =
        [
            DemoGoal.OnCatalogHabit("it demo zq draft", [CatalogHabitState.Published], target, null, DayOfTheRun),
            DemoGoal.OnCatalogHabit("It Demo Zq Draft", [CatalogHabitState.Draft], target, null, DayOfTheRun),
            DemoGoal.OnCatalogHabit("It Demo Zq Published", [CatalogHabitState.Unpublished], target, null, DayOfTheRun),
            DemoGoal.OnCatalogHabit("It Demo Zq Absent", [CatalogHabitState.Published], target, null, DayOfTheRun),
        ];

        await using (var scope = services.CreateAsyncScope())
        {
            Assert.Equal(CreateDemoAccountOutcome.Created, await scope.ServiceProvider.GetRequiredService<IDemoDataStore>()
                .CreateDemoAccountAsync(parsed, DemoPassword, [], new DemoAccountProfile(null, NamedTimeZone.Utc, database.Clock.GetUtcNow()), goals, CancellationToken.None));
        }

        Assert.Equal(["It Demo Zq Draft, 2 times a week"], await GoalNamesAsync(services, email));
    }

    private static async Task<DemoLoadingResult> LoadAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<DemoDataService>().LoadAsync(DemoPassword, CancellationToken.None);
    }

    private static async Task<ActingUser> SignInAsync(IServiceProvider services, string email, string password = DemoPassword)
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

    /// <summary>The goal names as the Goals page lists them: in name order (FR-015).</summary>
    private static async Task<List<string>> GoalNamesAsync(IServiceProvider services, string email) =>
        [.. (await ListAsync(services, await SignInAsync(services, email))).Select(g => g.Name)];

    /// <summary>The names in the list's name order: the keys of <see cref="HabitName.KeyOf"/>, compared ordinally.</summary>
    private static List<string> InNameOrder(params string[] names) =>
        [.. names.OrderBy(HabitName.KeyOf, StringComparer.Ordinal)];

    private static async Task<IReadOnlyList<CustomHabitView>> ListCustomHabitsAsync(IServiceProvider services, ActingUser user)
    {
        await using var scope = services.CreateAsyncScope();
        var list = await scope.ServiceProvider.GetRequiredService<CustomHabitService>().ListAsync(user, CancellationToken.None);
        Assert.Equal(CustomHabitListOutcome.Found, list.Outcome);
        return list.Habits;
    }

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

    /// <summary>The goal rows and the demo accounts' profile columns, each as one line of its columns in key order.</summary>
    private async Task<List<string>> SnapshotAsync() =>
    [
        .. await LinesAsync("Goals", "SELECT * FROM [Goals] ORDER BY [Id]"),
        .. await LinesAsync("Versions", "SELECT * FROM [GoalSettingsVersions] ORDER BY [GoalId], [AppliesFrom]"),
        .. await LinesAsync("Attempts", "SELECT * FROM [GoalAttempts] ORDER BY [GoalId], [Number]"),
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
