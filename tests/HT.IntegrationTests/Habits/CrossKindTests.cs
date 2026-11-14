using HT.Application.Habits;
using HT.Domain.Accounts;
using HT.Domain.Habits;
using Microsoft.Extensions.DependencyInjection;
using static HT.IntegrationTests.Habits.HabitSamples;

namespace HT.IntegrationTests.Habits;

/// <summary>FR-022, FR-024, US1-AS6, US2-AS5, US3-AS13: catalog results never contain a custom habit, and the identifiers do not cross.</summary>
[Collection(DatabaseCollection.Name)]
public sealed class CrossKindTests(DatabaseFixture database)
{
    private static readonly ActingUser Administrator = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.Administrator);

    private async Task<T> RunAsync<TService, T>(Func<TService, Task<T>> call)
        where TService : notnull
    {
        await using var scope = database.CreateScope();
        return await call(scope.ServiceProvider.GetRequiredService<TService>());
    }

    [Fact]
    public async Task Catalog_use_cases_never_see_a_custom_habit_and_the_identifiers_do_not_cross()
    {
        var user = await HabitAccounts.RegisteredUserAsync(database.Services, database.ConnectionString, "it-x-owner@example.com");
        var custom = (await RunAsync<CustomHabitService, CustomHabitCreateResult>(s => s.CreateAsync(user,
            new HabitInput { Name = "It-X Cold shower", Category = Category.Body, Intent = Intent.Build, Icon = "droplet" }, CancellationToken.None))).HabitId!.Value;
        var catalog = (await RunAsync<CatalogAdministrationService, CatalogHabitCreateResult>(s => s.CreateAsync(Administrator,
            new CatalogHabitInput { Name = "It-X Cold water", Category = Category.Health, Intent = Intent.Build, Icon = "droplet" }, CancellationToken.None))).HabitId!.Value;
        await RunAsync<CatalogAdministrationService, CatalogHabitPublishResult>(s => s.PublishAsync(Administrator, catalog, CancellationToken.None));

        var browsed = await RunAsync<CatalogService, CatalogBrowseResult>(s => s.BrowseAsync(new CatalogFilter(), CancellationToken.None));
        var searched = await RunAsync<CatalogService, CatalogBrowseResult>(s => s.BrowseAsync(new CatalogFilter { Search = "It-X" }, CancellationToken.None));
        Assert.Contains(browsed.Habits, h => h.Name == "It-X Cold water");
        Assert.DoesNotContain(browsed.Habits, h => h.Name == "It-X Cold shower" || h.Id == custom);
        Assert.Equal(["It-X Cold water"], searched.Habits.Select(h => h.Name));

        var details = await RunAsync<CatalogService, CatalogHabitDetailsResult>(s => s.GetDetailsAsync(custom, CancellationToken.None));
        Assert.Equal("This habit is not in the catalog.", details.Message);

        var list = await RunAsync<CatalogAdministrationService, CatalogHabitListResult>(s => s.ListAsync(Administrator, "It-X", CancellationToken.None));
        Assert.Equal(["It-X Cold water"], list.Habits.Select(h => h.Name));
        var get = await RunAsync<CatalogAdministrationService, CatalogHabitAdminResult>(s => s.GetAsync(Administrator, custom, CancellationToken.None));
        var edit = await RunAsync<CatalogAdministrationService, CatalogHabitEditResult>(s => s.EditAsync(Administrator, custom,
            new CatalogHabitInput { Name = "It-X Taken", Category = Category.Body, Intent = Intent.Build, Icon = "droplet" }, CancellationToken.None));
        var publish = await RunAsync<CatalogAdministrationService, CatalogHabitPublishResult>(s => s.PublishAsync(Administrator, custom, CancellationToken.None));
        var unpublish = await RunAsync<CatalogAdministrationService, CatalogHabitUnpublishResult>(s => s.UnpublishAsync(Administrator, custom, CancellationToken.None));
        Assert.All(new[] { get.Message, edit.Message, publish.Message, unpublish.Message }, m => Assert.Equal("This catalog habit does not exist.", m));

        var read = await RunAsync<CustomHabitService, CustomHabitResult>(s => s.GetAsync(user, catalog, CancellationToken.None));
        Assert.Equal("This custom habit does not exist.", read.Message);
        Assert.Equal("It-X Cold shower", (await RunAsync<CustomHabitService, CustomHabitResult>(s => s.GetAsync(user, custom, CancellationToken.None))).Habit!.Name);
    }
}
