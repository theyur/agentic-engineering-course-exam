using HT.Application.Habits;
using HT.Domain.Accounts;
using HT.Domain.Habits;
using Microsoft.Extensions.DependencyInjection;

namespace HT.IntegrationTests.Habits;

/// <summary>
/// FR-008, FR-024 and SC-005 on a reset database (HT-IT-SEED): the Home preview holds the first four catalog habits
/// and never a custom habit, even one whose name sorts first.
/// </summary>
[Collection(SeedDatabaseCollection.Name)]
public sealed class HomePreviewTests(SeedDatabaseFixture database)
{
    [Fact]
    public async Task The_preview_has_four_catalog_habits_no_custom_habit_and_counts_the_Published_ones()
    {
        await using var services = await database.ResetAndBuildAsync();
        var administrator = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.Administrator);
        var user = await HabitAccounts.RegisteredUserAsync(services, database.ConnectionString, "it-hp-seed@example.com");
        string[] published = ["B1 Reading", "B2 Running", "B3 Saving", "B4 Walking", "B5 Yoga"];

        await using (var scope = services.CreateAsyncScope())
        {
            var created = await scope.ServiceProvider.GetRequiredService<CustomHabitService>().CreateAsync(user,
                new HabitInput { Name = "A0 Custom", Category = Category.Home, Intent = Intent.Build, Icon = "sprout" }, CancellationToken.None);
            Assert.Equal(CustomHabitCreateOutcome.Created, created.Outcome);

            var admin = scope.ServiceProvider.GetRequiredService<CatalogAdministrationService>();
            foreach (var name in published)
            {
                var id = (await admin.CreateAsync(administrator,
                    new CatalogHabitInput { Name = name, Category = Category.Mind, Intent = Intent.Build, Icon = "book-open" }, CancellationToken.None)).HabitId!.Value;
                await admin.PublishAsync(administrator, id, CancellationToken.None);
            }
        }

        await using var read = services.CreateAsyncScope();
        var preview = await read.ServiceProvider.GetRequiredService<CatalogService>().GetHomePreviewAsync(CancellationToken.None);

        Assert.Equal(HomePreviewOutcome.Found, preview.Outcome);
        Assert.Equal(published[..4], preview.Habits.Select(h => h.Name));
        Assert.DoesNotContain(preview.Habits, h => h.Name == "A0 Custom");
        Assert.Equal(5, preview.PublishedCount);
    }
}
