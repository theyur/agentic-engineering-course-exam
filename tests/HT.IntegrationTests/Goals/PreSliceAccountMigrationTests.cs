using HT.Application;
using HT.Application.Accounts;
using HT.Application.Goals;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using static HT.IntegrationTests.Goals.GoalSamples;

namespace HT.IntegrationTests.Goals;

/// <summary>
/// HP-019 on an account literally created before the slice's migration (research R-14 steps 1 to 6), on its own
/// database <c>HT-IT-MIGRATE</c>. The one reflection of the tests: the internal context resolved by its type name.
/// </summary>
[Collection(MigrationDatabaseCollection.Name)]
public sealed class PreSliceAccountMigrationTests(MigrationDatabaseFixture database)
{
    private const string LastMigrationBeforeSlice4 = "HabitCatalog";
    private const string Email = "it-mi-pre-slice@example.com";

    [Fact]
    public async Task HP_019_an_account_created_before_the_migration_has_no_zone_no_name_no_creation_time_and_revision_0_until_its_first_goal()
    {
        // 1. Compose with AddHtPersistence; the installation's default zone is not UTC.
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton(InstallationTimeZone.From("Europe/Kyiv"))
            .AddHtApplication()
            .AddHtPersistence(database.ConnectionString);
        services.AddSingleton<TimeProvider>(database.Clock);
        await using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });

        // 2. and 3. The internal context by its type name; delete the database and migrate to the last slice 2 migration.
        await using (var scope = provider.CreateAsyncScope())
        {
            var contextType = typeof(PersistenceAssemblyMarker).Assembly.GetType("HT.Persistence.HtDbContext", throwOnError: true)!;
            var db = Assert.IsAssignableFrom<DbContext>(scope.ServiceProvider.GetRequiredService(contextType));
            await db.Database.EnsureDeletedAsync();
            await db.GetService<IMigrator>().MigrateAsync(LastMigrationBeforeSlice4);
        }

        // 4. An account row with slice 1's columns only.
        var id = Guid.NewGuid();
        await Sql.ExecuteAsync(database.ConnectionString,
            """
            INSERT INTO [AspNetUsers] ([Id], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed],
                [SecurityStamp], [ConcurrencyStamp], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnabled],
                [AccessFailedCount], [Role])
            VALUES (@id, @email, @email, @email, @email, 0, @stamp, @stamp, 0, 0, 0, 0, N'RegisteredUser')
            """,
            ("@id", id), ("@email", Email), ("@stamp", Guid.NewGuid().ToString("N")));

        // 5. The remaining migrations through the public entry point.
        Assert.True(await HtDatabase.MigrateAsync(provider, CancellationToken.None) > 0);

        // 6. No zone, no name, no creation time, revision 0; the first goal writes the default zone into the row.
        var owner = new AccountId(id);
        await using (var scope = provider.CreateAsyncScope())
        {
            var account = await scope.ServiceProvider.GetRequiredService<IAccountStore>().FindByIdAsync(owner, CancellationToken.None);
            Assert.NotNull(account);
            Assert.Equal(((string?)null, (string?)null, (DateTimeOffset?)null), (account.StoredTimeZone, account.Name, account.CreatedAt));
        }

        Assert.Equal(((string?)null, 0), await AccountStateAsync(database.ConnectionString, owner));

        var habit = await CatalogHabitAsync(provider, "It-Mi Pre-slice catalog");
        await using (var scope = provider.CreateAsyncScope())
        {
            var outcome = await scope.ServiceProvider.GetRequiredService<IGoalStore>().CreateAsync(
                owner,
                new NewGoal(new GoalHabit(habit, HabitKind.Catalog), new DateOnly(2026, 11, 18), null, YesNoOn(DayOfWeek.Monday)),
                Zone("Europe/Kyiv"),
                Zone("Europe/Kyiv"),
                CancellationToken.None);
            Assert.Equal(GoalWriteOutcomeKind.Created, outcome.Kind);
        }

        Assert.Equal(((string?)"Europe/Kyiv", 1), await AccountStateAsync(database.ConnectionString, owner));
    }
}
