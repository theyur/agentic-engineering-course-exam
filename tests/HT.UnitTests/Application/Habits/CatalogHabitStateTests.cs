using HT.Application.Habits;
using HT.Domain.Habits;
using HT.UnitTests.Fakes;
using static HT.UnitTests.Application.Habits.CatalogInputs;
using static HT.UnitTests.Fakes.HabitSamples;

namespace HT.UnitTests.Application.Habits;

/// <summary>FR-011 through the service: publish and unpublish, and the race limit on both sides (research R-08).</summary>
public sealed class CatalogHabitStateTests
{
    private readonly FakeCatalogHabitStore _store = new();
    private CatalogAdministrationService Service => new(_store);

    private async Task<bool> BrowsedAsync(HabitId id) =>
        (await new CatalogService(_store).BrowseAsync(new CatalogFilter(), CancellationToken.None)).Habits.Any(h => h.Id == id);

    [Fact]
    public async Task US3_AS2_publishing_a_Draft_makes_it_Published_and_browsed()
    {
        var habit = _store.Add(Catalog("Ac11 Stretching", CatalogHabitState.Draft));

        var result = await Service.PublishAsync(Administrator, habit.Id, CancellationToken.None);

        Assert.Equal(CatalogHabitPublishOutcome.Published, result.Outcome);
        Assert.Null(result.Message);
        Assert.Equal(CatalogHabitState.Published, _store.Get(habit.Id)!.State);
        Assert.True(await BrowsedAsync(habit.Id));
    }

    [Fact]
    public async Task US3_AS4_unpublishing_takes_it_out_of_the_catalog_and_its_details()
    {
        var habit = _store.Add(Catalog("Ac13 Stretching"));

        var result = await Service.UnpublishAsync(Administrator, habit.Id, CancellationToken.None);

        Assert.Equal(CatalogHabitUnpublishOutcome.Unpublished, result.Outcome);
        Assert.Null(result.Message);
        Assert.Equal(CatalogHabitState.Unpublished, _store.Get(habit.Id)!.State);
        Assert.False(await BrowsedAsync(habit.Id));
        var details = await new CatalogService(_store).GetDetailsAsync(habit.Id, CancellationToken.None);
        Assert.Equal("This habit is not in the catalog.", details.Message);
    }

    [Fact]
    public async Task US3_AS5_an_Unpublished_habit_can_be_published_again()
    {
        var habit = _store.Add(Catalog("Ac14 Stretching", CatalogHabitState.Unpublished));

        Assert.Equal(CatalogHabitPublishOutcome.Published, (await Service.PublishAsync(Administrator, habit.Id, CancellationToken.None)).Outcome);
        Assert.True(await BrowsedAsync(habit.Id));
    }

    [Fact]
    public async Task US3_AS6_unpublishing_a_Draft_is_not_published_and_it_stays_Draft()
    {
        var habit = _store.Add(Catalog("Ac15 Stretching", CatalogHabitState.Draft));

        var result = await Service.UnpublishAsync(Administrator, habit.Id, CancellationToken.None);

        Assert.Equal(CatalogHabitUnpublishOutcome.NotPublished, result.Outcome);
        Assert.Equal("This catalog habit is not published.", result.Message);
        Assert.Equal(CatalogHabitState.Draft, _store.Get(habit.Id)!.State);
        Assert.Equal(0, _store.WriteCount);
    }

    [Fact]
    public async Task US3_AS7_publishing_a_Published_habit_succeeds_with_no_write()
    {
        var habit = _store.Add(Catalog("Ac16 Stretching"));

        var result = await Service.PublishAsync(Administrator, habit.Id, CancellationToken.None);

        Assert.Equal(CatalogHabitPublishOutcome.Published, result.Outcome);
        Assert.Equal(0, _store.SwapCount);
        Assert.Equal(CatalogHabitState.Published, _store.Get(habit.Id)!.State);
    }

    [Fact]
    public async Task US3_AS8_unpublishing_an_Unpublished_habit_succeeds_with_no_write()
    {
        var habit = _store.Add(Catalog("Ac16b Stretching", CatalogHabitState.Unpublished));

        var result = await Service.UnpublishAsync(Administrator, habit.Id, CancellationToken.None);

        Assert.Equal(CatalogHabitUnpublishOutcome.Unpublished, result.Outcome);
        Assert.Equal(0, _store.SwapCount);
    }

    [Fact]
    public async Task Nine_lost_races_then_a_won_swap_publish()
    {
        var habit = _store.Add(Catalog("Ac16c Race", CatalogHabitState.Draft));
        _store.FailNextStateSwaps(9);

        var result = await Service.PublishAsync(Administrator, habit.Id, CancellationToken.None);

        Assert.Equal(CatalogHabitPublishOutcome.Published, result.Outcome);
        Assert.Equal(10, _store.SwapCount);
        Assert.Equal(CatalogHabitState.Published, _store.Get(habit.Id)!.State);
    }

    [Fact]
    public async Task Ten_lost_races_end_in_a_fault_with_no_swap_won_on_publish()
    {
        var habit = _store.Add(Catalog("Ac16d Race", CatalogHabitState.Draft));
        _store.FailNextStateSwaps(10);

        await Assert.ThrowsAsync<InvalidOperationException>(() => Service.PublishAsync(Administrator, habit.Id, CancellationToken.None));

        Assert.Equal(10, _store.SwapCount);
        Assert.Equal(CatalogHabitState.Draft, _store.Get(habit.Id)!.State);
        Assert.Equal(0, _store.WriteCount);
    }

    [Fact]
    public async Task Nine_lost_races_then_a_won_swap_unpublish()
    {
        var habit = _store.Add(Catalog("Ac16e Race"));
        _store.FailNextStateSwaps(9);

        var result = await Service.UnpublishAsync(Administrator, habit.Id, CancellationToken.None);

        Assert.Equal(CatalogHabitUnpublishOutcome.Unpublished, result.Outcome);
        Assert.Equal(10, _store.SwapCount);
    }

    [Fact]
    public async Task Ten_lost_races_end_in_a_fault_with_no_swap_won_on_unpublish()
    {
        var habit = _store.Add(Catalog("Ac16f Race"));
        _store.FailNextStateSwaps(10);

        await Assert.ThrowsAsync<InvalidOperationException>(() => Service.UnpublishAsync(Administrator, habit.Id, CancellationToken.None));

        Assert.Equal(CatalogHabitState.Published, _store.Get(habit.Id)!.State);
        Assert.Equal(0, _store.WriteCount);
    }

    [Fact]
    public async Task A_lost_race_whose_re_read_already_has_the_wanted_state_succeeds_with_no_further_write()
    {
        var habit = _store.Add(Catalog("Ac16g Race", CatalogHabitState.Draft));
        _store.FailNextStateSwaps(1);
        _store.ChangeStateOnNextFailedSwap(CatalogHabitState.Published);

        var result = await Service.PublishAsync(Administrator, habit.Id, CancellationToken.None);

        Assert.Equal(CatalogHabitPublishOutcome.Published, result.Outcome);
        Assert.Equal(1, _store.SwapCount);
        Assert.Equal(0, _store.WriteCount);
        Assert.Equal(CatalogHabitState.Published, _store.Get(habit.Id)!.State);
    }
}
