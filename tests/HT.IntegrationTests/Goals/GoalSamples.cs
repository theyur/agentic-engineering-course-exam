using HT.Application.Habits;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Time;
using Microsoft.Extensions.DependencyInjection;
using static HT.IntegrationTests.Habits.HabitSamples;

namespace HT.IntegrationTests.Goals;

/// <summary>Short ways to arrange goal data in the integration tests, through the public ports and plain SQL.</summary>
public static class GoalSamples
{
    public static NamedTimeZone Zone(string id)
    {
        Assert.True(NamedTimeZone.TryParse(id, out var zone), "Not a named time zone.");
        return zone;
    }

    public static GoalTarget Target(GoalTargetFields fields)
    {
        Assert.True(GoalTarget.TryCreate(fields, out var target, out var problem), $"Refused: {problem}");
        return target;
    }

    public static GoalName Name(string text)
    {
        Assert.True(GoalName.TryCreate(text, out var name, out _));
        return name!.Value;
    }

    public static GoalTarget YesNoOn(params DayOfWeek[] days) => Target(new GoalTargetFields
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo, ScheduleKind = ScheduleKind.FixedDays, Days = days.ToHashSet(),
    });

    /// <summary>A catalog habit in <paramref name="state"/>, created as Draft and moved through Published.</summary>
    public static async Task<HabitId> CatalogHabitAsync(
        IServiceProvider services, string name, CatalogHabitState state = CatalogHabitState.Published, Category category = Category.Mind)
    {
        await using var scope = services.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<ICatalogHabitStore>();
        var created = await store.CreateAsync(Definition(name, category), SuggestedStartingGoals.None, CancellationToken.None);
        var id = created.Id ?? throw new InvalidOperationException("The test habit name is taken.");
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

    public static async Task<HabitId> CustomHabitAsync(IServiceProvider services, AccountId owner, string name, Category category = Category.Body)
    {
        await using var scope = services.CreateAsyncScope();
        var created = await scope.ServiceProvider.GetRequiredService<ICustomHabitStore>()
            .CreateAsync(owner, Definition(name, category), CancellationToken.None);
        return created.Id ?? throw new InvalidOperationException("The test habit name is taken.");
    }

    /// <summary>Sets the account row's zone by SQL; null writes NULL, as on an account created before slice 4.</summary>
    public static Task SetZoneAsync(string connectionString, AccountId owner, string? zone) =>
        Sql.ExecuteAsync(connectionString, "UPDATE [AspNetUsers] SET [TimeZone] = @zone WHERE [Id] = @id",
            ("@zone", (object?)zone ?? DBNull.Value), ("@id", owner.Value));

    /// <summary>The account row's zone and goal revision.</summary>
    public static async Task<(string? Zone, int Revision)> AccountStateAsync(string connectionString, AccountId owner)
    {
        var row = Assert.Single(await Sql.QueryAsync(connectionString,
            "SELECT [TimeZone], [GoalsRevision] FROM [AspNetUsers] WHERE [Id] = @id", ("@id", owner.Value)));
        return ((string?)row["TimeZone"], (int)row["GoalsRevision"]!);
    }

    /// <summary>How many goal rows the owner has.</summary>
    public static async Task<int> GoalCountAsync(string connectionString, AccountId owner) =>
        (int)(await Sql.ScalarAsync(connectionString, "SELECT COUNT(*) FROM [Goals] WHERE [OwnerId] = @id", ("@id", owner.Value)))!;
}
