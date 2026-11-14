using HT.Application.Habits;
using HT.Domain.Accounts;
using HT.Domain.Habits;
using Microsoft.Extensions.DependencyInjection;

namespace HT.IntegrationTests.Habits;

/// <summary>FR-024 and US1-AS9 on a reset database (HT-IT-SEED): a custom habit's category never reaches the category filter.</summary>
[Collection(SeedDatabaseCollection.Name)]
public sealed class CrossKindEmptyCatalogTests(SeedDatabaseFixture database)
{
    [Fact]
    public async Task The_category_filter_has_only_the_categories_of_Published_habits()
    {
        await using var services = await database.ResetAndBuildAsync();
        var administrator = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.Administrator);
        var user = await HabitAccounts.RegisteredUserAsync(services, database.ConnectionString, "it-x-seed@example.com");

        await using (var scope = services.CreateAsyncScope())
        {
            var admin = scope.ServiceProvider.GetRequiredService<CatalogAdministrationService>();
            var id = (await admin.CreateAsync(administrator,
                new CatalogHabitInput { Name = "Ac27 Lifting", Category = Category.Body, Intent = Intent.Build, Icon = "dumbbell" }, CancellationToken.None)).HabitId!.Value;
            await admin.PublishAsync(administrator, id, CancellationToken.None);
            await scope.ServiceProvider.GetRequiredService<CustomHabitService>().CreateAsync(user,
                new HabitInput { Name = "Ac27 Plants", Category = Category.Home, Intent = Intent.Build, Icon = "sprout" }, CancellationToken.None);
        }

        await using var read = services.CreateAsyncScope();
        var filter = await read.ServiceProvider.GetRequiredService<CatalogService>().GetCategoryFilterAsync(CancellationToken.None);

        Assert.Equal(["All", "Body"], filter.Options.Select(o => o.Label));
    }
}
