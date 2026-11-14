using HT.Application.Habits;
using HT.Domain.Habits;
using Microsoft.Extensions.DependencyInjection;
using static HT.IntegrationTests.Habits.HabitSamples;

namespace HT.IntegrationTests.Habits;

/// <summary>US2-AS1 and US2-AS4 on SQL Server.</summary>
[Collection(DatabaseCollection.Name)]
public sealed class CatalogDetailsTests(DatabaseFixture database)
{
    private async Task<HabitId> ArrangeAsync(string name, CatalogHabitState state, SuggestedStartingGoals? goals = null)
    {
        await using var scope = database.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<ICatalogHabitStore>();
        var id = (await store.CreateAsync(Definition(name, description: "Books and articles."), goals ?? SuggestedStartingGoals.None, CancellationToken.None)).Id!.Value;
        if (state != CatalogHabitState.Draft)
        {
            await store.TrySetStateAsync(id, CatalogHabitState.Draft, CatalogHabitState.Published, CancellationToken.None);
        }

        if (state == CatalogHabitState.Unpublished)
        {
            await store.TrySetStateAsync(id, CatalogHabitState.Published, CatalogHabitState.Unpublished, CancellationToken.None);
        }

        return id;
    }

    private async Task<CatalogHabitDetailsResult> DetailsAsync(HabitId id)
    {
        await using var scope = database.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<CatalogService>().GetDetailsAsync(id, CancellationToken.None);
    }

    [Fact]
    public async Task US2_AS1_the_details_of_a_Published_habit()
    {
        var id = await ArrangeAsync("It-De Reading", CatalogHabitState.Published,
            Goals(1, Duration(15, DurationUnit.Minutes, Weekdays), Count(20, "pages", 3, SchedulePeriod.Week), YesNo(1, SchedulePeriod.Day)));

        var result = await DetailsAsync(id);

        Assert.Equal(CatalogHabitDetailsOutcome.Found, result.Outcome);
        Assert.Equal("It-De Reading", result.Details!.Name);
        Assert.Equal("Books and articles.", result.Details.Description);
        Assert.Equal(["15 min, 5 days a week", "20 pages, 3 times a week", "once a day"], result.Details.SuggestedStartingGoals.Select(g => g.Text));
        Assert.Equal(["Duration · fixed days Mon–Fri", "Count · flexible", "Yes/no · flexible"], result.Details.SuggestedStartingGoals.Select(g => g.Kind));
        Assert.Equal(2, result.Details.MarkedSuggestedStartingGoal!.Position);
    }

    [Fact]
    public async Task US2_AS4_a_Draft_an_Unpublished_and_an_unknown_habit_are_not_in_the_catalog()
    {
        var draft = await ArrangeAsync("It-De Draft habit", CatalogHabitState.Draft);
        var old = await ArrangeAsync("It-De Old habit", CatalogHabitState.Unpublished);

        foreach (var id in new[] { draft, old, new HabitId(Guid.NewGuid()) })
        {
            var result = await DetailsAsync(id);
            Assert.Equal(CatalogHabitDetailsOutcome.NotInCatalog, result.Outcome);
            Assert.Equal("This habit is not in the catalog.", result.Message);
        }
    }
}
