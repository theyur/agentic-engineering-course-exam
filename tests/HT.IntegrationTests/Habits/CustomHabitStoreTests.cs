using HT.Application.Habits;
using HT.Domain.Accounts;
using HT.Domain.Habits;
using Microsoft.Extensions.DependencyInjection;
using static HT.IntegrationTests.Habits.HabitSamples;

namespace HT.IntegrationTests.Habits;

/// <summary>The custom habit store on SQL Server: owner scoping of every statement, names per owner, edits and deletes.</summary>
[Collection(DatabaseCollection.Name)]
public sealed class CustomHabitStoreTests(DatabaseFixture database)
{
    private async Task<T> WithStoreAsync<T>(Func<ICustomHabitStore, Task<T>> call)
    {
        await using var scope = database.CreateScope();
        return await call(scope.ServiceProvider.GetRequiredService<ICustomHabitStore>());
    }

    private Task<AccountId> OwnerAsync(string email) =>
        HabitAccounts.RegisteredUserAsync(database.Services, database.ConnectionString, email).ContinueWith(t => t.Result.AccountId!.Value, TaskScheduler.Default);

    private async Task<HabitId> CreatedAsync(AccountId owner, string name, string? description = null)
    {
        var outcome = await WithStoreAsync(s => s.CreateAsync(owner, Definition(name, Category.Body, Intent.Build, "droplet", description), CancellationToken.None));
        Assert.False(outcome.IsNameTaken);
        return outcome.Id!.Value;
    }

    private Task<object?> RowNameAsync(HabitId id) =>
        Sql.ScalarAsync(database.ConnectionString, "SELECT [Name] FROM [CustomHabits] WHERE [Id] = @id", ("@id", id.Value));

    [Fact]
    public async Task Every_statement_is_scoped_by_owner()
    {
        var ownerA = await OwnerAsync("it-cu-scope-a@example.com");
        var ownerB = await OwnerAsync("it-cu-scope-b@example.com");
        var habit = await CreatedAsync(ownerA, "It-Cu Secret", "Private notes.");

        Assert.Null(await WithStoreAsync(s => s.FindAsync(ownerB, habit, CancellationToken.None)));
        Assert.Equal(UpdateCustomHabitOutcome.NotFound,
            await WithStoreAsync(s => s.UpdateAsync(ownerB, habit, Definition("It-Cu Taken over"), CancellationToken.None)));
        Assert.Equal(DeleteCustomHabitOutcome.NotFound, await WithStoreAsync(s => s.DeleteAsync(ownerB, habit, CancellationToken.None)));
        Assert.Empty(await WithStoreAsync(s => s.ListAsync(ownerB, CancellationToken.None)));
        Assert.Equal("It-Cu Secret", await RowNameAsync(habit));

        var found = await WithStoreAsync(s => s.FindAsync(ownerA, habit, CancellationToken.None));
        Assert.NotNull(found);
        Assert.Equal(("It-Cu Secret", "Private notes.", ownerA), (found.Definition.Name.Value, found.Definition.Description, found.Owner));
    }

    [Fact]
    public async Task A_catalog_habits_identifier_is_no_custom_habit()
    {
        var owner = await OwnerAsync("it-cu-catalog@example.com");
        HabitId catalogId;
        await using (var scope = database.CreateScope())
        {
            catalogId = (await scope.ServiceProvider.GetRequiredService<ICatalogHabitStore>()
                .CreateAsync(Definition("It-Cu Catalog one"), SuggestedStartingGoals.None, CancellationToken.None)).Id!.Value;
        }

        Assert.Null(await WithStoreAsync(s => s.FindAsync(owner, catalogId, CancellationToken.None)));
        Assert.Equal(DeleteCustomHabitOutcome.NotFound, await WithStoreAsync(s => s.DeleteAsync(owner, catalogId, CancellationToken.None)));
    }

    [Fact]
    public async Task The_name_key_is_per_owner_and_beside_catalog_habits()
    {
        var ownerA = await OwnerAsync("it-cu-names-a@example.com");
        var ownerB = await OwnerAsync("it-cu-names-b@example.com");
        await CreatedAsync(ownerA, "It-Cu Cold shower");
        await using (var scope = database.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<ICatalogHabitStore>()
                .CreateAsync(Definition("It-Cu Catalog shower"), SuggestedStartingGoals.None, CancellationToken.None);
        }

        Assert.True((await WithStoreAsync(s => s.CreateAsync(ownerA, Definition(" it-cu cold SHOWER "), CancellationToken.None))).IsNameTaken);
        Assert.True(await WithStoreAsync(s => s.NameKeyExistsAsync(ownerA, HabitName.KeyOf(" it-cu cold SHOWER "), null, CancellationToken.None)));
        Assert.False(await WithStoreAsync(s => s.NameKeyExistsAsync(ownerB, HabitName.KeyOf("It-Cu Cold shower"), null, CancellationToken.None)));
        await CreatedAsync(ownerB, "It-Cu Cold shower");
        await CreatedAsync(ownerA, "It-Cu Catalog shower");
    }

    [Fact]
    public async Task Edits_give_name_taken_not_found_or_saved()
    {
        var owner = await OwnerAsync("it-cu-edit@example.com");
        await CreatedAsync(owner, "It-Cu First");
        var second = await CreatedAsync(owner, "It-Cu Second", "Kept.");

        Assert.Equal(UpdateCustomHabitOutcome.NameTaken,
            await WithStoreAsync(s => s.UpdateAsync(owner, second, Definition("IT-CU FIRST"), CancellationToken.None)));
        Assert.Equal("It-Cu Second", await RowNameAsync(second));
        Assert.Equal(UpdateCustomHabitOutcome.NotFound,
            await WithStoreAsync(s => s.UpdateAsync(owner, new HabitId(Guid.NewGuid()), Definition("It-Cu Ghost"), CancellationToken.None)));
        Assert.Equal(UpdateCustomHabitOutcome.Saved,
            await WithStoreAsync(s => s.UpdateAsync(owner, second, Definition("It-Cu Second", Category.Body, Intent.Build, "droplet", "Kept."), CancellationToken.None)));
        Assert.Equal(UpdateCustomHabitOutcome.Saved,
            await WithStoreAsync(s => s.UpdateAsync(owner, second, Definition("It-Cu Second, renamed", Category.Home, Intent.Reduce, "tv"), CancellationToken.None)));

        var read = (await WithStoreAsync(s => s.FindAsync(owner, second, CancellationToken.None)))!;
        Assert.Equal(("It-Cu Second, renamed", Category.Home, Intent.Reduce, "tv", (string?)null),
            (read.Definition.Name.Value, read.Definition.Category, read.Definition.Intent, read.Definition.Icon.Name, read.Definition.Description));
    }

    [Fact]
    public async Task Delete_is_permanent()
    {
        var owner = await OwnerAsync("it-cu-delete@example.com");
        var habit = await CreatedAsync(owner, "It-Cu Gone");

        Assert.Equal(DeleteCustomHabitOutcome.Deleted, await WithStoreAsync(s => s.DeleteAsync(owner, habit, CancellationToken.None)));

        Assert.Equal(0, await Sql.ScalarAsync(database.ConnectionString, "SELECT COUNT(*) FROM [CustomHabits] WHERE [Id] = @id", ("@id", habit.Value)));
        Assert.Equal(DeleteCustomHabitOutcome.NotFound, await WithStoreAsync(s => s.DeleteAsync(owner, habit, CancellationToken.None)));
        await CreatedAsync(owner, "It-Cu Gone");
    }
}
