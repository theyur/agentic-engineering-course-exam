using HT.Application.Accounts;
using HT.Application.Habits;
using HT.Application.Operator;
using HT.Domain.Accounts;
using HT.Domain.Habits;
using HT.IntegrationTests.Habits;
using Microsoft.Extensions.DependencyInjection;

namespace HT.IntegrationTests.Operator;

/// <summary>
/// US6-AS1 to US6-AS8, FR-030, FR-031, SC-007 and the rollback of research R-09 on SQL Server. Each test starts from a
/// reset <c>HT-IT-SEED</c> database (research R-13, "Starting states").
/// </summary>
[Collection(SeedDatabaseCollection.Name)]
public sealed class SeedCatalogLoadingTests(SeedDatabaseFixture database)
{
    private const string DemoPassword = "example2026";

    private static readonly ActingUser Administrator = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.Administrator);

    private static readonly string[] DemoCustomHabitNames =
        ["Cold shower", "No phone in bed", "Evening stretch", "Plan tomorrow", "Late snacks", "Water the plants"];

    [Fact]
    public async Task US6_AS1_an_empty_catalog_gets_the_seed_catalog_as_FR_030_describes()
    {
        await using var services = await database.ResetAndBuildAsync();

        var result = await LoadAsync(services, DemoPassword);

        Assert.Equal(DemoCatalogOutcome.SeedCatalogLoaded, result.Catalog!.Outcome);
        Assert.Equal("seed catalog loaded", result.Catalog.Message);

        var published = await BrowseAsync(services);
        Assert.True(published.Count >= 20);
        Assert.Equal(Enum.GetValues<Category>(), published.Select(h => h.Category).Distinct().Order());
        Assert.True(published.Count(h => h.Intent == Intent.Reduce) >= 4);
        Assert.Equal(published.Count, published.Select(h => h.Icon).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(
            SeedCatalog.Habits.Where(h => h.State == CatalogHabitState.Published).Select(h => h.Definition.Name.Value).Order(StringComparer.Ordinal),
            published.Select(h => h.Name).Order(StringComparer.Ordinal));

        var list = await AdministratorListAsync(services);
        Assert.Equal(SeedCatalog.Habits.Count, list.Total);
        Assert.Single(list.Habits, h => h.State == CatalogHabitState.Draft);
        Assert.Single(list.Habits, h => h.State == CatalogHabitState.Unpublished);
    }

    [Fact]
    public async Task US6_AS1_each_seed_habit_is_stored_with_its_fields_its_state_and_its_suggested_starting_goals_in_order()
    {
        await using var services = await database.ResetAndBuildAsync();
        await LoadAsync(services, DemoPassword);

        var list = await AdministratorListAsync(services);
        await using var scope = services.CreateAsyncScope();
        var admin = scope.ServiceProvider.GetRequiredService<CatalogAdministrationService>();
        foreach (var seed in SeedCatalog.Habits)
        {
            var item = Assert.Single(list.Habits, h => h.Name == seed.Definition.Name.Value);
            var details = (await admin.GetAsync(Administrator, item.Id, CancellationToken.None)).Habit!;

            Assert.Equal(seed.State, details.State);
            Assert.Equal(seed.Definition.Category, details.Category);
            Assert.Equal(seed.Definition.Intent, details.Intent);
            Assert.Equal(seed.Definition.Icon.Name, details.Icon);
            Assert.Equal(seed.Definition.Description, details.Description);
            Assert.Equal(
                seed.SuggestedStartingGoals.Items.Select(g => SuggestedStartingGoalText.Format(g).Text),
                details.SuggestedStartingGoals.Select(g => g.Text));
            Assert.Equal(seed.SuggestedStartingGoals.MarkedIndex + 1, details.MarkedSuggestedStartingGoal?.Position);
        }
    }

    [Fact]
    public async Task US6_AS2_SC_007_a_second_run_reports_catalog_not_empty_and_leaves_the_same_rows()
    {
        await using var services = await database.ResetAndBuildAsync();
        await LoadAsync(services, DemoPassword);
        var once = await SnapshotAsync();

        var second = await LoadAsync(services, DemoPassword);

        Assert.Equal(DemoCatalogOutcome.CatalogNotEmpty, second.Catalog!.Outcome);
        Assert.Equal("catalog not empty", second.Catalog.Message);
        var twice = await SnapshotAsync();
        Assert.Equal(once.Catalog, twice.Catalog);
        Assert.Equal(once.Goals, twice.Goals);
        Assert.Equal(once.CustomHabits, twice.CustomHabits);
        Assert.NotEmpty(once.Catalog);
        Assert.NotEmpty(once.Goals);
        Assert.Equal(6, once.CustomHabits.Count);
    }

    [Fact]
    public async Task US6_AS3_a_catalog_with_any_habit_reports_catalog_not_empty_and_keeps_only_that_habit()
    {
        await using var services = await database.ResetAndBuildAsync();
        await using (var scope = services.CreateAsyncScope())
        {
            var created = await scope.ServiceProvider.GetRequiredService<CatalogAdministrationService>().CreateAsync(
                Administrator,
                new CatalogHabitInput { Name = "Ac29 Rowing", Category = Category.Body, Intent = Intent.Build, Icon = "dumbbell" },
                CancellationToken.None);
            Assert.Equal(CatalogHabitCreateOutcome.Created, created.Outcome);
        }

        var result = await LoadAsync(services, DemoPassword);

        Assert.Equal(DemoCatalogOutcome.CatalogNotEmpty, result.Catalog!.Outcome);
        var list = await AdministratorListAsync(services);
        Assert.Equal(1, list.Total);
        Assert.Equal("Ac29 Rowing", Assert.Single(list.Habits).Name);
    }

    [Fact]
    public async Task US6_AS4_no_demo_password_creates_no_account_no_catalog_habit_and_no_custom_habit()
    {
        await using var services = await database.ResetAndBuildAsync();

        var result = await LoadAsync(services, null);

        Assert.Equal(DemoLoadingOutcome.NoPassword, result.Outcome);
        Assert.Equal("no demo password configured", result.Message);
        Assert.Null(result.Catalog);
        Assert.Equal(0, await CountAsync("AspNetUsers"));
        Assert.Equal(0, await CountAsync("CatalogHabits"));
        Assert.Equal(0, await CountAsync("CatalogHabitSuggestedStartingGoals"));
        Assert.Equal(0, await CountAsync("CustomHabits"));
    }

    [Fact]
    public async Task US6_AS5_each_created_demo_account_lists_its_demo_custom_habits_and_no_catalog_view_shows_them()
    {
        await using var services = await database.ResetAndBuildAsync();

        var result = await LoadAsync(services, DemoPassword);

        Assert.All(result.Accounts, a =>
        {
            Assert.Equal(DemoAccountOutcome.Created, a.Outcome);
            Assert.Equal("demo custom habits added", a.CustomHabits!.Message);
        });
        foreach (var email in DemoDataService.DemoEmails)
        {
            var habits = await ListCustomHabitsAsync(services, email, DemoPassword);
            var expected = DemoCustomHabits.For(email)
                .Select(h => (h.Name.Value, (string?)null, h.Category, h.Intent, h.Icon.Name))
                .ToList();
            Assert.Equal(expected, habits.Select(h => (h.Name, h.Description, h.Category, h.Intent, h.Icon)));
        }

        Assert.Equal(["Cold shower", "No phone in bed"], (await ListCustomHabitsAsync(services, "demo1@example.com", DemoPassword)).Select(h => h.Name));
        Assert.Equal(["Evening stretch", "Plan tomorrow"], (await ListCustomHabitsAsync(services, "demo2@example.com", DemoPassword)).Select(h => h.Name));
        Assert.Equal(["Late snacks", "Water the plants"], (await ListCustomHabitsAsync(services, "demo3@example.com", DemoPassword)).Select(h => h.Name));

        var publicNames = (await BrowseAsync(services)).Select(h => h.Name).ToList();
        var adminNames = (await AdministratorListAsync(services)).Habits.Select(h => h.Name).ToList();
        Assert.All(DemoCustomHabitNames, name =>
        {
            Assert.DoesNotContain(name, publicNames);
            Assert.DoesNotContain(name, adminNames);
        });
    }

    [Fact]
    public async Task US6_AS6_a_second_run_has_no_custom_habit_result_and_each_account_lists_the_same_custom_habits()
    {
        await using var services = await database.ResetAndBuildAsync();
        await LoadAsync(services, DemoPassword);
        var before = new Dictionary<string, List<CustomHabitView>>();
        foreach (var email in DemoDataService.DemoEmails)
        {
            before[email] = [.. await ListCustomHabitsAsync(services, email, DemoPassword)];
        }

        var second = await LoadAsync(services, DemoPassword);

        Assert.All(second.Accounts, a =>
        {
            Assert.Equal(DemoAccountOutcome.AlreadyExists, a.Outcome);
            Assert.Null(a.CustomHabits);
        });
        foreach (var email in DemoDataService.DemoEmails)
        {
            Assert.Equal(before[email], await ListCustomHabitsAsync(services, email, DemoPassword));
        }
    }

    [Fact]
    public async Task US6_AS7_deleted_demo_custom_habits_are_never_created_again()
    {
        await using var services = await database.ResetAndBuildAsync();
        await LoadAsync(services, DemoPassword);
        await DeleteCustomHabitsAsync(services, "demo1@example.com", "Cold shower");
        await DeleteCustomHabitsAsync(services, "demo2@example.com", "Evening stretch", "Plan tomorrow");

        await LoadAsync(services, DemoPassword);

        Assert.Equal(["No phone in bed"], (await ListCustomHabitsAsync(services, "demo1@example.com", DemoPassword)).Select(h => h.Name));
        Assert.Empty(await ListCustomHabitsAsync(services, "demo2@example.com", DemoPassword));
    }

    [Fact]
    public async Task US6_AS8_a_demo_email_registered_by_a_person_gets_no_custom_habit()
    {
        await using var services = await database.ResetAndBuildAsync();
        await using (var scope = services.CreateAsyncScope())
        {
            var registered = await scope.ServiceProvider.GetRequiredService<AccountService>()
                .RegisterAsync("demo2@example.com", "owner2026", CancellationToken.None);
            Assert.Equal(RegistrationOutcome.Registered, registered.Outcome);
        }

        var result = await LoadAsync(services, DemoPassword);

        var demo2 = Assert.Single(result.Accounts, a => a.Email == "demo2@example.com");
        Assert.Equal(DemoAccountOutcome.AlreadyExists, demo2.Outcome);
        Assert.Null(demo2.CustomHabits);
        Assert.Empty(await ListCustomHabitsAsync(services, "demo2@example.com", "owner2026"));
    }

    [Fact]
    public async Task A_demo_account_whose_custom_habit_cannot_be_written_is_not_created_and_no_log_names_the_habit()
    {
        const string name = "It Seed Secret Habit";
        const string description = "It seed secret description";
        const string email = "it-seed-rollback@example.com";
        await (await database.ResetAndBuildAsync()).DisposeAsync();

        using var capture = new LogCapture();
        await using var services = capture.BuildServices(database.ConnectionString);
        var habit = HabitDefinitionOf(name, description);
        Assert.True(Email.TryParse(email, out var parsed));

        InvalidOperationException exception;
        await using (var scope = services.CreateAsyncScope())
        {
            exception = await Assert.ThrowsAsync<InvalidOperationException>(() => scope.ServiceProvider.GetRequiredService<IDemoDataStore>()
                .CreateDemoAccountAsync(parsed, HabitAccounts.Password, [habit, habit], CancellationToken.None));
        }

        Assert.Equal(0, await Sql.CountUsersAsync(database.ConnectionString, email));
        Assert.Equal(0, await CountAsync("CustomHabits"));
        foreach (var value in new[] { name, habit.Name.Key, description })
        {
            Assert.DoesNotContain(value, exception.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        Assert.Null(exception.InnerException);
        capture.AssertContainsNone(name, habit.Name.Key, description);
    }

    [Fact]
    public async Task A_seed_catalog_that_cannot_be_written_whole_creates_nothing_and_the_next_run_loads_it()
    {
        await using var services = await database.ResetAndBuildAsync();
        var first = SeedCatalog.Habits[0];

        await using (var scope = services.CreateAsyncScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<IDemoDataStore>();
            Assert.False(await store.TryCreateSeedCatalogAsync([first, first], CancellationToken.None));
        }

        Assert.Equal(0, await CountAsync("CatalogHabits"));
        Assert.Equal(0, await CountAsync("CatalogHabitSuggestedStartingGoals"));

        var result = await LoadAsync(services, DemoPassword);

        Assert.Equal(DemoCatalogOutcome.SeedCatalogLoaded, result.Catalog!.Outcome);
        Assert.Equal(SeedCatalog.Habits.Count, await CountAsync("CatalogHabits"));
    }

    private static async Task<DemoLoadingResult> LoadAsync(IServiceProvider services, string? password)
    {
        await using var scope = services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<DemoDataService>().LoadAsync(password, CancellationToken.None);
    }

    private static async Task<IReadOnlyList<CatalogHabitSummary>> BrowseAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        return (await scope.ServiceProvider.GetRequiredService<CatalogService>().BrowseAsync(new CatalogFilter(), CancellationToken.None)).Habits;
    }

    private static async Task<CatalogHabitListResult> AdministratorListAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var list = await scope.ServiceProvider.GetRequiredService<CatalogAdministrationService>().ListAsync(Administrator, null, CancellationToken.None);
        Assert.Equal(CatalogHabitListOutcome.Found, list.Outcome);
        return list;
    }

    private static async Task<ActingUser> SignInAsync(IServiceProvider services, string email, string password)
    {
        await using var scope = services.CreateAsyncScope();
        var signIn = await scope.ServiceProvider.GetRequiredService<AccountService>().SignInAsync(email, password, CancellationToken.None);
        Assert.Equal(SignInOutcome.SignedIn, signIn.Outcome);
        return signIn.ActingUser!;
    }

    private static async Task<IReadOnlyList<CustomHabitView>> ListCustomHabitsAsync(IServiceProvider services, string email, string password)
    {
        var user = await SignInAsync(services, email, password);
        await using var scope = services.CreateAsyncScope();
        var list = await scope.ServiceProvider.GetRequiredService<CustomHabitService>().ListAsync(user, CancellationToken.None);
        Assert.Equal(CustomHabitListOutcome.Found, list.Outcome);
        return list.Habits;
    }

    private static async Task DeleteCustomHabitsAsync(IServiceProvider services, string email, params string[] names)
    {
        var user = await SignInAsync(services, email, DemoPassword);
        var habits = await ListCustomHabitsAsync(services, email, DemoPassword);
        await using var scope = services.CreateAsyncScope();
        var customHabits = scope.ServiceProvider.GetRequiredService<CustomHabitService>();
        foreach (var name in names)
        {
            var habit = Assert.Single(habits, h => h.Name == name);
            var deleted = await customHabits.DeleteAsync(user, habit.Id, CancellationToken.None);
            Assert.Equal(CustomHabitDeleteOutcome.Deleted, deleted.Outcome);
        }
    }

    private static HabitDefinition HabitDefinitionOf(string name, string description)
    {
        Assert.True(HabitDefinition.TryCreate(name, description, Category.Mind, Intent.Build, "brain", out var definition, out _));
        return definition;
    }

    private async Task<int> CountAsync(string table) =>
        (int)(await Sql.ScalarAsync(database.ConnectionString, $"SELECT COUNT(*) FROM [{table}]"))!;

    /// <summary>The rows of the three tables of FR-030 and FR-031, each as one line of its columns in key order.</summary>
    private async Task<(List<string> Catalog, List<string> Goals, List<string> CustomHabits)> SnapshotAsync() =>
        (await LinesAsync("SELECT * FROM [CatalogHabits] ORDER BY [Id]"),
         await LinesAsync("SELECT * FROM [CatalogHabitSuggestedStartingGoals] ORDER BY [CatalogHabitId], [Position]"),
         await LinesAsync("SELECT * FROM [CustomHabits] ORDER BY [Id]"));

    private async Task<List<string>> LinesAsync(string sql)
    {
        var rows = await Sql.QueryAsync(database.ConnectionString, sql);
        return rows
            .Select(row => string.Join(" | ", row.OrderBy(c => c.Key, StringComparer.Ordinal).Select(c => $"{c.Key}={c.Value}")))
            .ToList();
    }
}
