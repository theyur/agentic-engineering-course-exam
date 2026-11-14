using HT.Application.Habits;
using HT.Domain.Accounts;
using HT.Domain.Habits;
using Microsoft.Extensions.DependencyInjection;

namespace HT.IntegrationTests.Habits;

/// <summary>
/// HP-018 and HP-015: parallel creates and renames with one name give one success and one "name taken", and no
/// log entry or exception text carries the custom habit's name, its key or its description.
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class CustomHabitRaceTests(DatabaseFixture database)
{
    private const string Description = "It-Cr private description of the morning routine";

    private static HabitInput Input(string name) =>
        new() { Name = name, Description = Description, Category = Category.Body, Intent = Intent.Build, Icon = "droplet" };

    private static async Task<T> RunAsync<T>(IServiceProvider services, Func<CustomHabitService, Task<T>> call)
    {
        await using var scope = services.CreateAsyncScope();
        return await call(scope.ServiceProvider.GetRequiredService<CustomHabitService>());
    }

    [Fact]
    public async Task Two_parallel_creates_with_one_name_give_one_habit_and_log_no_value()
    {
        var capture = new LogCapture();
        await using var services = capture.BuildServices(database.ConnectionString);
        var user = await HabitAccounts.RegisteredUserAsync(services, database.ConnectionString, "it-cr-create@example.com");

        var results = await Task.WhenAll(
            Task.Run(() => RunAsync(services, s => s.CreateAsync(user, Input("It-Cr Cold race"), CancellationToken.None))),
            Task.Run(() => RunAsync(services, s => s.CreateAsync(user, Input("IT-CR COLD RACE"), CancellationToken.None))));

        Assert.Single(results, r => r.Outcome == CustomHabitCreateOutcome.Created);
        Assert.Single(results, r => r.Outcome == CustomHabitCreateOutcome.NameTaken);
        Assert.Equal(1, await Sql.ScalarAsync(database.ConnectionString,
            "SELECT COUNT(*) FROM [CustomHabits] WHERE [OwnerId] = @owner", ("@owner", user.AccountId!.Value.Value)));
        capture.AssertContainsNone("It-Cr Cold race", HabitName.KeyOf("It-Cr Cold race"), Description);
    }

    [Fact]
    public async Task Two_parallel_renames_to_one_name_give_one_rename_and_log_no_value()
    {
        var capture = new LogCapture();
        await using var services = capture.BuildServices(database.ConnectionString);
        var user = await HabitAccounts.RegisteredUserAsync(services, database.ConnectionString, "it-cr-rename@example.com");
        var first = (await RunAsync(services, s => s.CreateAsync(user, Input("It-Cr Rename one"), CancellationToken.None))).HabitId!.Value;
        var second = (await RunAsync(services, s => s.CreateAsync(user, Input("It-Cr Rename two"), CancellationToken.None))).HabitId!.Value;

        var results = await Task.WhenAll(
            Task.Run(() => RunAsync(services, s => s.EditAsync(user, first, Input("It-Cr Renamed race"), CancellationToken.None))),
            Task.Run(() => RunAsync(services, s => s.EditAsync(user, second, Input("it-cr renamed RACE"), CancellationToken.None))));

        Assert.Single(results, r => r.Outcome == CustomHabitEditOutcome.Saved);
        Assert.Single(results, r => r.Outcome == CustomHabitEditOutcome.NameTaken);
        capture.AssertContainsNone("It-Cr Renamed race", HabitName.KeyOf("It-Cr Renamed race"), "It-Cr Rename one", "It-Cr Rename two", Description);
    }
}
