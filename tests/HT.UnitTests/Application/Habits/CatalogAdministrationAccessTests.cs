using HT.Application.Habits;
using HT.Domain.Accounts;
using HT.Domain.Habits;
using HT.UnitTests.Fakes;
using static HT.UnitTests.Application.Habits.CatalogInputs;
using static HT.UnitTests.Fakes.HabitSamples;

namespace HT.UnitTests.Application.Habits;

/// <summary>SC-006, FR-013, FR-014, FR-016: who may administer the catalog, and the check order across steps.</summary>
public sealed class CatalogAdministrationAccessTests
{
    private readonly FakeCatalogHabitStore _store = new();
    private readonly CatalogHabit _habit;

    public CatalogAdministrationAccessTests()
    {
        _habit = _store.Add(Catalog("Ac11 Stretching", CatalogHabitState.Draft));
    }

    private CatalogAdministrationService Service => new(_store);

    /// <summary>Each of the six use cases as the given user: (outcome name, message).</summary>
    private async Task<IReadOnlyList<(string Outcome, string? Message)>> AllAsync(ActingUser user, HabitId id)
    {
        var service = Service;
        var list = await service.ListAsync(user, null, CancellationToken.None);
        var get = await service.GetAsync(user, id, CancellationToken.None);
        var create = await service.CreateAsync(user, Input("Ac11 New"), CancellationToken.None);
        var edit = await service.EditAsync(user, id, Input("Ac11 Renamed"), CancellationToken.None);
        var publish = await service.PublishAsync(user, id, CancellationToken.None);
        var unpublish = await service.UnpublishAsync(user, id, CancellationToken.None);
        return
        [
            (list.Outcome.ToString(), list.Message), (get.Outcome.ToString(), get.Message),
            (create.Outcome.ToString(), create.Message), (edit.Outcome.ToString(), edit.Message),
            (publish.Outcome.ToString(), publish.Message), (unpublish.Outcome.ToString(), unpublish.Message),
        ];
    }

    [Fact]
    public async Task US3_AS11_a_registered_user_is_refused_by_every_use_case_and_nothing_changes()
    {
        var results = await AllAsync(RegisteredUser, _habit.Id);

        Assert.All(results, r =>
        {
            Assert.Equal("NotAdministrator", r.Outcome);
            Assert.Equal("Only administrators can manage the catalog.", r.Message);
        });
        Assert.Equal(0, _store.WriteCount);
        Assert.Equal(0, _store.SwapCount);
        Assert.Equal(CatalogHabitState.Draft, _store.Get(_habit.Id)!.State);
        Assert.Single(_store.Habits);
    }

    [Fact]
    public async Task US3_AS11_an_anonymous_visitor_is_refused_by_every_use_case_and_nothing_changes()
    {
        var results = await AllAsync(ActingUser.Anonymous, _habit.Id);

        Assert.All(results, r =>
        {
            Assert.Equal("NotSignedIn", r.Outcome);
            Assert.Equal("You are not signed in.", r.Message);
        });
        Assert.Equal(0, _store.WriteCount);
        Assert.Equal(0, _store.SwapCount);
    }

    [Fact]
    public async Task US3_AS11_refusals_carry_no_list_and_no_habit()
    {
        var list = await Service.ListAsync(RegisteredUser, null, CancellationToken.None);
        Assert.Empty(list.Habits);
        Assert.Equal(0, list.Total);
        Assert.Null((await Service.GetAsync(RegisteredUser, _habit.Id, CancellationToken.None)).Habit);
        Assert.Null((await Service.CreateAsync(ActingUser.Anonymous, Input("Ac11 New"), CancellationToken.None)).HabitId);
    }

    [Fact]
    public async Task US3_AS12_an_unknown_identifier_does_not_exist_for_read_edit_publish_and_unpublish()
    {
        var unknown = new HabitId(Guid.NewGuid());
        var service = Service;

        var get = await service.GetAsync(Administrator, unknown, CancellationToken.None);
        var edit = await service.EditAsync(Administrator, unknown, Input("Ac12 Ghost"), CancellationToken.None);
        var publish = await service.PublishAsync(Administrator, unknown, CancellationToken.None);
        var unpublish = await service.UnpublishAsync(Administrator, unknown, CancellationToken.None);

        Assert.Equal(CatalogHabitAdminOutcome.NotFound, get.Outcome);
        Assert.Equal(CatalogHabitEditOutcome.NotFound, edit.Outcome);
        Assert.Equal(CatalogHabitPublishOutcome.NotFound, publish.Outcome);
        Assert.Equal(CatalogHabitUnpublishOutcome.NotFound, unpublish.Outcome);
        Assert.All(new[] { get.Message, edit.Message, publish.Message, unpublish.Message },
            m => Assert.Equal("This catalog habit does not exist.", m));
        Assert.Equal(0, _store.WriteCount);
    }

    [Fact]
    public async Task US3_AS18_the_role_comes_before_the_fields_and_the_identifier_before_the_fields()
    {
        var create = await Service.CreateAsync(RegisteredUser, Input("   "), CancellationToken.None);
        Assert.Equal(CatalogHabitCreateOutcome.NotAdministrator, create.Outcome);
        Assert.Equal("Only administrators can manage the catalog.", create.Message);

        var anonymous = await Service.CreateAsync(ActingUser.Anonymous, Input("   "), CancellationToken.None);
        Assert.Equal(CatalogHabitCreateOutcome.NotSignedIn, anonymous.Outcome);

        var edit = await Service.EditAsync(Administrator, new HabitId(Guid.NewGuid()), Input("   "), CancellationToken.None);
        Assert.Equal(CatalogHabitEditOutcome.NotFound, edit.Outcome);
        Assert.Equal("This catalog habit does not exist.", edit.Message);
    }
}
