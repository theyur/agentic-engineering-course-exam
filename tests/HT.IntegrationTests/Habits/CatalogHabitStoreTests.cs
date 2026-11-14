using HT.Application.Habits;
using HT.Domain.Habits;
using Microsoft.Extensions.DependencyInjection;
using static HT.IntegrationTests.Habits.HabitSamples;

namespace HT.IntegrationTests.Habits;

/// <summary>The catalog habit store on SQL Server: round trip, the owned list, the binary-collation key, the state swap, HP-018.</summary>
[Collection(DatabaseCollection.Name)]
public sealed class CatalogHabitStoreTests(DatabaseFixture database)
{
    private async Task<T> WithStoreAsync<T>(Func<ICatalogHabitStore, Task<T>> call)
    {
        await using var scope = database.CreateScope();
        return await call(scope.ServiceProvider.GetRequiredService<ICatalogHabitStore>());
    }

    private Task<CreateCatalogHabitOutcome> CreateAsync(HabitDefinition definition, SuggestedStartingGoals? goals = null) =>
        WithStoreAsync(s => s.CreateAsync(definition, goals ?? SuggestedStartingGoals.None, CancellationToken.None));

    private async Task<HabitId> CreatedAsync(HabitDefinition definition, SuggestedStartingGoals? goals = null)
    {
        var outcome = await CreateAsync(definition, goals);
        Assert.False(outcome.IsNameTaken);
        return outcome.Id!.Value;
    }

    [Fact]
    public async Task A_created_habit_is_a_Draft_with_every_field_and_its_list_in_order()
    {
        var goals = Goals(1, Duration(15, DurationUnit.Minutes, Weekdays), Count(20, "pages", 3, SchedulePeriod.Week), YesNo(1, SchedulePeriod.Day));
        var id = await CreatedAsync(Definition("It-Cat Reading", Category.Mind, Intent.Build, "book-open", "Books and articles."), goals);

        var found = await WithStoreAsync(s => s.FindAsync(id, CancellationToken.None));
        Assert.NotNull(found);
        Assert.Equal(CatalogHabitState.Draft, found.State);
        Assert.Equal("It-Cat Reading", found.Definition.Name.Value);
        Assert.Equal("Books and articles.", found.Definition.Description);
        Assert.Equal(Category.Mind, found.Definition.Category);
        Assert.Equal(Intent.Build, found.Definition.Intent);
        Assert.Equal("book-open", found.Definition.Icon.Name);
        Assert.Equal(1, found.SuggestedStartingGoals.MarkedIndex);
        Assert.Equal([Measurement.Duration, Measurement.Count, Measurement.YesNo], found.SuggestedStartingGoals.Items.Select(g => g.Measurement));
        Assert.Equal(Weekdays.ToHashSet(), found.SuggestedStartingGoals.Items[0].Schedule.Days.ToHashSet());
        Assert.Equal(DurationUnit.Minutes, found.SuggestedStartingGoals.Items[0].DurationUnit);
        Assert.Equal("pages", found.SuggestedStartingGoals.Items[1].CountUnit);
        Assert.Equal(3, found.SuggestedStartingGoals.Items[1].Schedule.Times);

        Assert.Null(await WithStoreAsync(s => s.FindPublishedAsync(id, CancellationToken.None)));
        Assert.DoesNotContain(await WithStoreAsync(s => s.ListPublishedAsync(CancellationToken.None)), h => h.Id == id);
        Assert.Contains(await WithStoreAsync(s => s.ListAllAsync(CancellationToken.None)), h => h.Id == id);
    }

    [Fact]
    public async Task Update_replaces_the_list_longer_shorter_reordered_and_empty()
    {
        var a = Count(1, "a", 1, SchedulePeriod.Day);
        var b = Count(2, "b", 1, SchedulePeriod.Day);
        var c = Count(3, "c", 1, SchedulePeriod.Day);
        var id = await CreatedAsync(Definition("It-Cat Lists"), Goals(0, a, b));

        await AssertUpdatedAsync(id, Goals(2, a, b, c), ["a", "b", "c"], 3);
        await AssertUpdatedAsync(id, Goals(0, c), ["c"], 1);
        await AssertUpdatedAsync(id, Goals(1, b, c, a), ["b", "c", "a"], 2);
        await AssertUpdatedAsync(id, SuggestedStartingGoals.None, [], null);
    }

    private async Task AssertUpdatedAsync(HabitId id, SuggestedStartingGoals goals, string[] units, int? markedPosition)
    {
        Assert.Equal(UpdateCatalogHabitOutcome.Saved,
            await WithStoreAsync(s => s.UpdateAsync(id, Definition("It-Cat Lists"), goals, CancellationToken.None)));

        var rows = await Sql.QueryAsync(database.ConnectionString,
            "SELECT [Position], [IsMarked], [CountUnit] FROM [CatalogHabitSuggestedStartingGoals] WHERE [CatalogHabitId] = @id ORDER BY [Position]",
            ("@id", id.Value));
        Assert.Equal(units, rows.Select(r => (string)r["CountUnit"]!));
        Assert.Equal(Enumerable.Range(1, units.Length), rows.Select(r => (int)r["Position"]!));
        Assert.Equal(markedPosition, rows.Where(r => (bool)r["IsMarked"]!).Select(r => (int?)r["Position"]).SingleOrDefault());
    }

    [Fact]
    public async Task The_key_is_compared_under_the_binary_collation_in_every_state()
    {
        var first = await CreatedAsync(Definition("It-Cat Key"));
        Assert.True((await CreateAsync(Definition(" it-cat KEY "))).IsNameTaken);

        Assert.True(await WithStoreAsync(s => s.TrySetStateAsync(first, CatalogHabitState.Draft, CatalogHabitState.Published, CancellationToken.None)));
        Assert.True(await WithStoreAsync(s => s.TrySetStateAsync(first, CatalogHabitState.Published, CatalogHabitState.Unpublished, CancellationToken.None)));
        Assert.True((await CreateAsync(Definition("IT-CAT key"))).IsNameTaken);

        await CreatedAsync(Definition("It-Cat ſun"));
        Assert.True((await CreateAsync(Definition("It-Cat Sun"))).IsNameTaken);

        Assert.Equal(UpdateCatalogHabitOutcome.Saved,
            await WithStoreAsync(s => s.UpdateAsync(first, Definition("IT-CAT KEY"), SuggestedStartingGoals.None, CancellationToken.None)));
        Assert.Equal("IT-CAT KEY", (await WithStoreAsync(s => s.FindAsync(first, CancellationToken.None)))!.Definition.Name.Value);
        Assert.Equal(CatalogHabitState.Unpublished, (await WithStoreAsync(s => s.FindAsync(first, CancellationToken.None)))!.State);
    }

    [Fact]
    public async Task Name_key_exists_excludes_the_given_habit()
    {
        var id = await CreatedAsync(Definition("It-Cat Exists"));
        var key = HabitName.KeyOf("It-Cat Exists");

        Assert.True(await WithStoreAsync(s => s.NameKeyExistsAsync(key, null, CancellationToken.None)));
        Assert.False(await WithStoreAsync(s => s.NameKeyExistsAsync(key, id, CancellationToken.None)));
        Assert.False(await WithStoreAsync(s => s.NameKeyExistsAsync(HabitName.KeyOf("It-Cat Nobody"), null, CancellationToken.None)));
    }

    [Fact]
    public async Task An_update_of_an_unknown_identifier_is_not_found()
    {
        Assert.Equal(UpdateCatalogHabitOutcome.NotFound,
            await WithStoreAsync(s => s.UpdateAsync(new HabitId(Guid.NewGuid()), Definition("It-Cat Ghost"), SuggestedStartingGoals.None, CancellationToken.None)));
    }

    [Fact]
    public async Task The_state_swap_writes_only_from_the_expected_state()
    {
        var id = await CreatedAsync(Definition("It-Cat Swap"));

        Assert.False(await WithStoreAsync(s => s.TrySetStateAsync(id, CatalogHabitState.Published, CatalogHabitState.Unpublished, CancellationToken.None)));
        Assert.Equal(CatalogHabitState.Draft, (await WithStoreAsync(s => s.FindAsync(id, CancellationToken.None)))!.State);

        Assert.True(await WithStoreAsync(s => s.TrySetStateAsync(id, CatalogHabitState.Draft, CatalogHabitState.Published, CancellationToken.None)));
        Assert.NotNull(await WithStoreAsync(s => s.FindPublishedAsync(id, CancellationToken.None)));
        Assert.Contains(await WithStoreAsync(s => s.ListPublishedAsync(CancellationToken.None)), h => h.Id == id);
    }

    [Fact]
    public async Task Two_parallel_creates_with_one_name_give_one_habit_HP_018()
    {
        var results = await Task.WhenAll(
            Task.Run(() => CreateAsync(Definition("It-Cat Race"))),
            Task.Run(() => CreateAsync(Definition("IT-CAT RACE"))));

        Assert.Single(results, r => !r.IsNameTaken);
        Assert.Single(results, r => r.IsNameTaken);
        Assert.Equal(1, await Sql.ScalarAsync(database.ConnectionString,
            "SELECT COUNT(*) FROM [CatalogHabits] WHERE [NameKey] = @key", ("@key", HabitName.KeyOf("It-Cat Race"))));
    }

    [Fact]
    public async Task Two_parallel_renames_to_one_name_give_one_rename_HP_018()
    {
        var first = await CreatedAsync(Definition("It-Cat Rename A"));
        var second = await CreatedAsync(Definition("It-Cat Rename B"));

        var results = await Task.WhenAll(
            Task.Run(() => WithStoreAsync(s => s.UpdateAsync(first, Definition("It-Cat Renamed"), SuggestedStartingGoals.None, CancellationToken.None))),
            Task.Run(() => WithStoreAsync(s => s.UpdateAsync(second, Definition("it-cat renamed"), SuggestedStartingGoals.None, CancellationToken.None))));

        Assert.Single(results, r => r == UpdateCatalogHabitOutcome.Saved);
        Assert.Single(results, r => r == UpdateCatalogHabitOutcome.NameTaken);
        Assert.Equal(1, await Sql.ScalarAsync(database.ConnectionString,
            "SELECT COUNT(*) FROM [CatalogHabits] WHERE [NameKey] = @key", ("@key", HabitName.KeyOf("It-Cat Renamed"))));
    }
}
