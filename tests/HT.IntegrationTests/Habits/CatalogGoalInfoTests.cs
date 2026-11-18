using HT.Application.Goals;
using HT.Application.Habits;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.IntegrationTests.Goals;
using Microsoft.Extensions.DependencyInjection;

namespace HT.IntegrationTests.Habits;

/// <summary>
/// US6-AS1 to AS4, US3-AS6, FR-019, FR-031, FR-032 over SQL Server: the catalog's goal information for two registered
/// users with goals on shared catalog habits, through <see cref="CatalogService"/> and <see cref="GoalService"/> as the
/// host composes them. The fixture's clock is 09:00 UTC on Wednesday 11 November 2026 and the accounts are in UTC.
/// Shared database: own prefixes ("it-cg-", "It-Cg"); habits are scoped by a search for the test's suffix, other
/// rows are checked by inclusion and exclusion, and goal counts are compared with plain SQL per owner and habit.
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class CatalogGoalInfoTests(DatabaseFixture database)
{
    private static readonly DateOnly Wed11 = new(2026, 11, 11);

    private static string Suffix() => Guid.NewGuid().ToString("N")[..8];

    private async Task<T> InScopeAsync<TService, T>(Func<TService, Task<T>> call)
        where TService : notnull
    {
        await using var scope = database.CreateScope();
        return await call(scope.ServiceProvider.GetRequiredService<TService>());
    }

    private Task<CatalogBrowseResult> BrowseAsync(ActingUser user, CatalogFilter filter, bool onlyTracked = false) =>
        InScopeAsync<CatalogService, CatalogBrowseResult>(s => s.BrowseAsync(user, filter, onlyTracked, CancellationToken.None));

    private Task<CatalogHabitDetailsResult> DetailsAsync(ActingUser user, HabitId habit) =>
        InScopeAsync<CatalogService, CatalogHabitDetailsResult>(s => s.GetDetailsAsync(user, habit, CancellationToken.None));

    private async Task<GoalId> CreateAsync(ActingUser user, HabitId habit, string? name = null)
    {
        var input = new GoalInput
        {
            Habit = new GoalHabit(habit, HabitKind.Catalog),
            Target = new GoalTargetFields
            {
                BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo, ScheduleKind = ScheduleKind.Times, Times = 3, Period = SchedulePeriod.Week,
            },
            StartDate = Wed11,
            Name = name,
        };
        var created = await InScopeAsync<GoalService, GoalCreateResult>(s => s.CreateAsync(user, input, CancellationToken.None));
        Assert.Equal(GoalCreateOutcome.Created, created.Outcome);
        return created.GoalId!.Value;
    }

    private async Task<int> RowsAsync(ActingUser owner, HabitId habit) =>
        (int)(await Sql.ScalarAsync(database.ConnectionString,
            "SELECT COUNT(*) FROM [Goals] WHERE [OwnerId] = @owner AND [CatalogHabitId] = @habit",
            ("@owner", owner.AccountId!.Value.Value), ("@habit", habit.Value)))!;

    private static int? CountOf(CatalogBrowseResult result, HabitId habit) => Assert.Single(result.Habits, h => h.Id == habit).GoalCount;

    /// <summary>
    /// Users A and B; catalog habits Reading, Yoga and Tidying with the suffix. A has two goals on Reading ("Morning
    /// pages" and one named after the habit) and one on Yoga; B has one on Yoga.
    /// </summary>
    private sealed record World(
        string Suffix, ActingUser A, ActingUser B, HabitId Reading, HabitId Yoga, HabitId Tidying, GoalId AReading, GoalId AMorningPages, GoalId AYoga, GoalId BYoga);

    private async Task<World> WorldAsync()
    {
        var suffix = Suffix();
        var a = await HabitAccounts.RegisteredUserAsync(database.Services, database.ConnectionString, $"it-cg-a-{suffix}@example.com");
        var b = await HabitAccounts.RegisteredUserAsync(database.Services, database.ConnectionString, $"it-cg-b-{suffix}@example.com");
        var reading = await GoalSamples.CatalogHabitAsync(database.Services, $"It-Cg Reading {suffix}");
        var yoga = await GoalSamples.CatalogHabitAsync(database.Services, $"It-Cg Yoga {suffix}", category: Category.Body);
        var tidying = await GoalSamples.CatalogHabitAsync(database.Services, $"It-Cg Tidying {suffix}", category: Category.Home);
        var aReading = await CreateAsync(a, reading);
        var aMorningPages = await CreateAsync(a, reading, $"It-Cg Morning pages {suffix}");
        var aYoga = await CreateAsync(a, yoga);
        var bYoga = await CreateAsync(b, yoga);
        return new World(suffix, a, b, reading, yoga, tidying, aReading, aMorningPages, aYoga, bYoga);
    }

    [Fact]
    public async Task US6_AS1_and_US3_AS6_each_user_gets_their_own_goal_counts_only()
    {
        var world = await WorldAsync();
        var filter = new CatalogFilter { Search = world.Suffix };

        var a = await BrowseAsync(world.A, filter);
        var b = await BrowseAsync(world.B, filter);

        Assert.Equal(CatalogBrowseOutcome.Found, a.Outcome);
        Assert.Equal(2, CountOf(a, world.Reading));
        Assert.Equal(await RowsAsync(world.A, world.Reading), CountOf(a, world.Reading));
        Assert.Equal(1, CountOf(a, world.Yoga));
        Assert.Equal(0, CountOf(a, world.Tidying));
        Assert.Equal(2, a.TrackedCount);
        Assert.Equal(0, CountOf(b, world.Reading));
        Assert.Equal(await RowsAsync(world.B, world.Reading), CountOf(b, world.Reading));
        Assert.Equal(1, CountOf(b, world.Yoga));
        Assert.Equal(0, CountOf(b, world.Tidying));
        Assert.Equal(1, b.TrackedCount);
    }

    [Fact]
    public async Task US6_AS2_only_habits_I_track_returns_the_users_tracked_Published_habits_with_the_other_filters()
    {
        var world = await WorldAsync();

        var a = await BrowseAsync(world.A, new CatalogFilter(), onlyTracked: true);
        var aBody = await BrowseAsync(world.A, new CatalogFilter { Category = Category.Body, Search = world.Suffix }, onlyTracked: true);
        var b = await BrowseAsync(world.B, new CatalogFilter(), onlyTracked: true);

        Assert.Contains(a.Habits, h => h.Id == world.Reading);
        Assert.Contains(a.Habits, h => h.Id == world.Yoga);
        Assert.DoesNotContain(a.Habits, h => h.Id == world.Tidying);
        Assert.All(a.Habits, h => Assert.True(h.GoalCount >= 1));
        Assert.Equal(a.Habits.Count, a.TrackedCount);
        Assert.Equal([world.Yoga], aBody.Habits.Select(h => h.Id));
        Assert.Contains(b.Habits, h => h.Id == world.Yoga);
        Assert.DoesNotContain(b.Habits, h => h.Id == world.Reading);
        Assert.DoesNotContain(b.Habits, h => h.Id == world.Tidying);
    }

    [Fact]
    public async Task US6_AS2_a_goal_on_an_Unpublished_habit_does_not_bring_it_back()
    {
        var world = await WorldAsync();
        await using (var scope = database.CreateScope())
        {
            Assert.True(await scope.ServiceProvider.GetRequiredService<ICatalogHabitStore>()
                .TrySetStateAsync(world.Yoga, CatalogHabitState.Published, CatalogHabitState.Unpublished, CancellationToken.None));
        }

        var a = await BrowseAsync(world.A, new CatalogFilter { Search = world.Suffix }, onlyTracked: true);

        Assert.Equal([world.Reading], a.Habits.Select(h => h.Id));
        Assert.Equal(1, a.TrackedCount);
    }

    [Fact]
    public async Task US6_AS3_and_US3_AS6_the_details_give_the_users_own_goals_in_name_order()
    {
        var world = await WorldAsync();

        var a = (await DetailsAsync(world.A, world.Reading)).Details!;
        var bReading = (await DetailsAsync(world.B, world.Reading)).Details!;
        var bYoga = (await DetailsAsync(world.B, world.Yoga)).Details!;

        Assert.Equal(2, a.GoalCount);
        Assert.Equal(
            [new GoalOnHabit(world.AMorningPages, $"It-Cg Morning pages {world.Suffix}"), new GoalOnHabit(world.AReading, $"It-Cg Reading {world.Suffix}, 3 times a week")],
            a.Goals);
        Assert.Equal(0, bReading.GoalCount);
        Assert.Empty(bReading.Goals!);
        Assert.Equal(1, bYoga.GoalCount);
        Assert.Equal([world.BYoga], bYoga.Goals!.Select(g => g.Id));
        Assert.DoesNotContain(bYoga.Goals!, g => g.Id == world.AYoga);
    }

    [Fact]
    public async Task US6_AS4_and_FR_019_a_visitor_an_administrator_and_the_slice_2_overloads_get_no_goal_information()
    {
        var world = await WorldAsync();
        var filter = new CatalogFilter { Search = world.Suffix };
        var administrator = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.Administrator);

        var visitor = await BrowseAsync(ActingUser.Anonymous, filter);
        var admin = await BrowseAsync(administrator, filter);
        var old = await InScopeAsync<CatalogService, CatalogBrowseResult>(s => s.BrowseAsync(filter, CancellationToken.None));
        var oldDetails = await InScopeAsync<CatalogService, CatalogHabitDetailsResult>(s => s.GetDetailsAsync(world.Reading, CancellationToken.None));
        var visitorDetails = await DetailsAsync(ActingUser.Anonymous, world.Reading);
        var refusedVisitor = await BrowseAsync(ActingUser.Anonymous, filter, onlyTracked: true);
        var refusedAdmin = await BrowseAsync(administrator, filter, onlyTracked: true);

        foreach (var result in new[] { visitor, admin, old })
        {
            Assert.Equal(CatalogBrowseOutcome.Found, result.Outcome);
            Assert.Null(result.TrackedCount);
            Assert.Equal([world.Reading, world.Tidying, world.Yoga], result.Habits.Select(h => h.Id));
            Assert.All(result.Habits, h => Assert.Null(h.GoalCount));
        }

        foreach (var details in new[] { oldDetails, visitorDetails })
        {
            Assert.Equal(CatalogHabitDetailsOutcome.Found, details.Outcome);
            Assert.Null(details.Details!.GoalCount);
            Assert.Null(details.Details.Goals);
        }

        Assert.Equal(("NotSignedIn", (string?)"You are not signed in.", 0), (refusedVisitor.Outcome.ToString(), refusedVisitor.Message, refusedVisitor.Count));
        Assert.Equal(("NotForAdministrators", (string?)"Goals are for registered users.", 0), (refusedAdmin.Outcome.ToString(), refusedAdmin.Message, refusedAdmin.Count));
    }
}
