using HT.Application.Habits;
using HT.Domain.Accounts;
using HT.Domain.Habits;
using Microsoft.Extensions.DependencyInjection;
using static HT.IntegrationTests.Habits.HabitSamples;

namespace HT.IntegrationTests.Habits;

/// <summary>US3 through CatalogAdministrationService on SQL Server, browsing after each step (SC-003), and HP-018.</summary>
[Collection(DatabaseCollection.Name)]
public sealed class CatalogAdministrationTests(DatabaseFixture database)
{
    private static readonly ActingUser Administrator = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.Administrator);

    private async Task<T> AdminAsync<T>(Func<CatalogAdministrationService, Task<T>> call)
    {
        await using var scope = database.CreateScope();
        return await call(scope.ServiceProvider.GetRequiredService<CatalogAdministrationService>());
    }

    private async Task<IReadOnlyList<CatalogHabitSummary>> BrowseAsync()
    {
        await using var scope = database.CreateScope();
        return (await scope.ServiceProvider.GetRequiredService<CatalogService>().BrowseAsync(new CatalogFilter(), CancellationToken.None)).Habits;
    }

    private static CatalogHabitInput Input(string name, Category category = Category.Body, params SuggestedStartingGoalInput[] goals) => new()
    {
        Name = name, Category = category, Intent = Intent.Build, Icon = "person-standing", Description = "Gentle stretching.", SuggestedStartingGoals = goals,
    };

    [Fact]
    public async Task US3_AS1_to_AS5_create_publish_edit_unpublish_and_publish_again_with_browsing_after_each_step()
    {
        var created = await AdminAsync(s => s.CreateAsync(Administrator, Input("It-Ad Stretching", goals: HabitSamples.Input(YesNo(1, SchedulePeriod.Day), true)), CancellationToken.None));
        Assert.Equal(CatalogHabitCreateOutcome.Created, created.Outcome);
        var id = created.HabitId!.Value;
        Assert.DoesNotContain(await BrowseAsync(), h => h.Id == id);
        Assert.Contains((await AdminAsync(s => s.ListAsync(Administrator, "It-Ad Stretching", CancellationToken.None))).Habits,
            h => h.Id == id && h.State == CatalogHabitState.Draft);

        Assert.Equal(CatalogHabitPublishOutcome.Published, (await AdminAsync(s => s.PublishAsync(Administrator, id, CancellationToken.None))).Outcome);
        Assert.Contains(await BrowseAsync(), h => h.Id == id && h.Name == "It-Ad Stretching");

        Assert.Equal(CatalogHabitEditOutcome.Saved,
            (await AdminAsync(s => s.EditAsync(Administrator, id, Input("It-Ad Morning stretch", Category.Health), CancellationToken.None))).Outcome);
        var browsed = await BrowseAsync();
        Assert.Contains(browsed, h => h.Id == id && h.Name == "It-Ad Morning stretch" && h.Category == Category.Health);
        Assert.DoesNotContain(browsed, h => h.Name == "It-Ad Stretching");

        Assert.Equal(CatalogHabitUnpublishOutcome.Unpublished, (await AdminAsync(s => s.UnpublishAsync(Administrator, id, CancellationToken.None))).Outcome);
        Assert.DoesNotContain(await BrowseAsync(), h => h.Id == id);

        Assert.Equal(CatalogHabitPublishOutcome.Published, (await AdminAsync(s => s.PublishAsync(Administrator, id, CancellationToken.None))).Outcome);
        Assert.Contains(await BrowseAsync(), h => h.Id == id);
    }

    [Fact]
    public async Task US3_AS9_a_taken_name_is_refused_on_create_and_on_rename()
    {
        await AdminAsync(s => s.CreateAsync(Administrator, Input("It-Ad Reading"), CancellationToken.None));
        var other = (await AdminAsync(s => s.CreateAsync(Administrator, Input("It-Ad Other"), CancellationToken.None))).HabitId!.Value;

        Assert.Equal(CatalogHabitCreateOutcome.NameTaken, (await AdminAsync(s => s.CreateAsync(Administrator, Input(" it-ad reading "), CancellationToken.None))).Outcome);
        Assert.Equal(CatalogHabitEditOutcome.NameTaken, (await AdminAsync(s => s.EditAsync(Administrator, other, Input(" it-ad reading "), CancellationToken.None))).Outcome);
        Assert.Equal("It-Ad Other", (await AdminAsync(s => s.GetAsync(Administrator, other, CancellationToken.None))).Habit!.Name);
    }

    [Fact]
    public async Task FR_012_an_edit_of_a_Draft_keeps_it_Draft_and_out_of_browsing()
    {
        var id = (await AdminAsync(s => s.CreateAsync(Administrator, Input("It-Ad Draft edit"), CancellationToken.None))).HabitId!.Value;

        Assert.Equal(CatalogHabitEditOutcome.Saved, (await AdminAsync(s => s.EditAsync(Administrator, id, Input("It-Ad Draft edited"), CancellationToken.None))).Outcome);

        Assert.Equal(CatalogHabitState.Draft, (await AdminAsync(s => s.GetAsync(Administrator, id, CancellationToken.None))).Habit!.State);
        Assert.DoesNotContain(await BrowseAsync(), h => h.Id == id);
    }

    [Fact]
    public async Task The_list_keeps_name_order_among_its_own_habits()
    {
        foreach (var name in new[] { "It-Ad Order walking", "It-Ad Order Breathing", "It-Ad Order Cycling" })
        {
            await AdminAsync(s => s.CreateAsync(Administrator, Input(name), CancellationToken.None));
        }

        var list = await AdminAsync(s => s.ListAsync(Administrator, "it-ad order", CancellationToken.None));

        Assert.Equal(["It-Ad Order Breathing", "It-Ad Order Cycling", "It-Ad Order walking"], list.Habits.Select(h => h.Name));
        Assert.True(list.Total >= 3);
    }

    [Fact]
    public async Task HP_018_two_administrators_creating_one_name_in_parallel_give_one_habit()
    {
        var results = await Task.WhenAll(
            Task.Run(() => AdminAsync(s => s.CreateAsync(Administrator, Input("It-Ad Race"), CancellationToken.None))),
            Task.Run(() => AdminAsync(s => s.CreateAsync(Administrator, Input("IT-AD RACE"), CancellationToken.None))));

        Assert.Single(results, r => r.Outcome == CatalogHabitCreateOutcome.Created);
        Assert.Single(results, r => r.Outcome == CatalogHabitCreateOutcome.NameTaken);
    }

    [Fact]
    public async Task Two_parallel_publishes_of_one_Draft_both_succeed_and_leave_it_Published()
    {
        var id = (await AdminAsync(s => s.CreateAsync(Administrator, Input("It-Ad Publish race"), CancellationToken.None))).HabitId!.Value;

        var results = await Task.WhenAll(
            Task.Run(() => AdminAsync(s => s.PublishAsync(Administrator, id, CancellationToken.None))),
            Task.Run(() => AdminAsync(s => s.PublishAsync(Administrator, id, CancellationToken.None))));

        Assert.All(results, r => Assert.Equal(CatalogHabitPublishOutcome.Published, r.Outcome));
        Assert.Equal(CatalogHabitState.Published, (await AdminAsync(s => s.GetAsync(Administrator, id, CancellationToken.None))).Habit!.State);
    }
}
