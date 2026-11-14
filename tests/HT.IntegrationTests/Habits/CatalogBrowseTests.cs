using HT.Application.Habits;
using HT.Domain.Habits;
using Microsoft.Extensions.DependencyInjection;
using static HT.IntegrationTests.Habits.HabitSamples;

namespace HT.IntegrationTests.Habits;

/// <summary>US1-AS1 to US1-AS5 through CatalogService on SQL Server, by inclusion, exclusion and relative order.</summary>
[Collection(DatabaseCollection.Name)]
public sealed class CatalogBrowseTests(DatabaseFixture database)
{
    private async Task<HabitId> ArrangeAsync(HabitDefinition definition, CatalogHabitState state, SuggestedStartingGoals? goals = null)
    {
        await using var scope = database.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<ICatalogHabitStore>();
        var created = await store.CreateAsync(definition, goals ?? SuggestedStartingGoals.None, CancellationToken.None);
        var id = created.Id!.Value;
        if (state != CatalogHabitState.Draft)
        {
            Assert.True(await store.TrySetStateAsync(id, CatalogHabitState.Draft, CatalogHabitState.Published, CancellationToken.None));
        }

        if (state == CatalogHabitState.Unpublished)
        {
            Assert.True(await store.TrySetStateAsync(id, CatalogHabitState.Published, CatalogHabitState.Unpublished, CancellationToken.None));
        }

        return id;
    }

    private async Task<IReadOnlyList<CatalogHabitSummary>> BrowseAsync(CatalogFilter filter)
    {
        await using var scope = database.CreateScope();
        return (await scope.ServiceProvider.GetRequiredService<CatalogService>().BrowseAsync(filter, CancellationToken.None)).Habits;
    }

    private async Task<IReadOnlyList<string>> NamesAsync(CatalogFilter filter, string prefix) =>
        (await BrowseAsync(filter)).Select(h => h.Name).Where(n => n.StartsWith(prefix, StringComparison.Ordinal)).ToList();

    [Fact]
    public async Task US1_AS1_only_Published_habits_with_their_marked_suggested_starting_goal()
    {
        await ArrangeAsync(Definition("It-Br1 Reading", Category.Mind, Intent.Build, "book-open", "Books."), CatalogHabitState.Published,
            Goals(0, Count(20, "pages", 3, SchedulePeriod.Week), YesNo(1, SchedulePeriod.Day)));
        await ArrangeAsync(Definition("It-Br1 Yoga", Category.Body, Intent.Build, "person-standing"), CatalogHabitState.Published);
        await ArrangeAsync(Definition("It-Br1 Journaling"), CatalogHabitState.Draft);
        await ArrangeAsync(Definition("It-Br1 Stretching"), CatalogHabitState.Unpublished);

        var habits = (await BrowseAsync(new CatalogFilter())).Where(h => h.Name.StartsWith("It-Br1", StringComparison.Ordinal)).ToList();

        Assert.Equal(["It-Br1 Reading", "It-Br1 Yoga"], habits.Select(h => h.Name));
        Assert.Equal("Books.", habits[0].Description);
        Assert.Equal("book-open", habits[0].Icon);
        Assert.Equal("20 pages, 3 times a week", habits[0].MarkedSuggestedStartingGoal!.Text);
        Assert.Null(habits[1].MarkedSuggestedStartingGoal);
    }

    [Fact]
    public async Task US1_AS2_name_order_ignoring_case()
    {
        await ArrangeAsync(Definition("It-Br2 walking"), CatalogHabitState.Published);
        await ArrangeAsync(Definition("It-Br2 Breathing"), CatalogHabitState.Published);
        await ArrangeAsync(Definition("It-Br2 Cycling"), CatalogHabitState.Published);

        Assert.Equal(["It-Br2 Breathing", "It-Br2 Cycling", "It-Br2 walking"], await NamesAsync(new CatalogFilter(), "It-Br2"));
    }

    [Fact]
    public async Task US1_AS3_to_AS5_the_filters()
    {
        await ArrangeAsync(Definition("It-Br3 Saving", Category.Money), CatalogHabitState.Published);
        await ArrangeAsync(Definition("It-Br3 Running", Category.Body), CatalogHabitState.Published);
        await ArrangeAsync(Definition("It-Br3 Sugar", Category.Health, Intent.Reduce), CatalogHabitState.Published);
        await ArrangeAsync(Definition("It-Br3 Morning pages", Category.Mind), CatalogHabitState.Published);

        Assert.Equal(["It-Br3 Saving"], await NamesAsync(new CatalogFilter { Category = Category.Money }, "It-Br3"));
        Assert.Equal(["It-Br3 Sugar"], await NamesAsync(new CatalogFilter { Intent = Intent.Reduce }, "It-Br3"));
        Assert.Equal(["It-Br3 Morning pages"], await NamesAsync(new CatalogFilter { Search = " it-br3 MORNING " }, "It-Br3"));
    }
}
