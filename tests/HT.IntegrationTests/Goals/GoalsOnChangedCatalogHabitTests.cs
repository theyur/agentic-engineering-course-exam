using HT.Application.Goals;
using HT.Application.Habits;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.IntegrationTests.Habits;
using Microsoft.Extensions.DependencyInjection;
using static HT.IntegrationTests.Goals.GoalSamples;

namespace HT.IntegrationTests.Goals;

/// <summary>
/// US7 on SQL Server (US7-AS1, AS2, AS6, AS7, FR-033, FR-034, ADR 0012): the unpublish confirmation's count across
/// owners, goals that keep working on an Unpublished or renamed catalog habit, publishing again, and a goal created
/// between the count and the confirmed unpublish. The fixture's clock is 09:00 UTC on Wednesday 11 November 2026 and
/// the accounts are in UTC. Shared database: own prefixes ("it-gc-", "It-Gc"); every count is of a habit this test
/// created, and other users' data is checked only by exclusion.
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class GoalsOnChangedCatalogHabitTests(DatabaseFixture database)
{
    private static readonly ActingUser Administrator = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.Administrator);

    /// <summary>Today in UTC at the fixture's clock.</summary>
    private static readonly DateOnly Wed11 = new(2026, 11, 11);

    private static string Suffix() => Guid.NewGuid().ToString("N")[..8];

    private static GoalTargetFields FifteenMinutesOnWeekdays => new()
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = 15, DurationUnit = DurationUnit.Minutes,
        ScheduleKind = ScheduleKind.FixedDays, Days = HabitSamples.Weekdays.ToHashSet(),
    };

    private async Task<T> GoalsAsync<T>(Func<GoalService, Task<T>> call)
    {
        await using var scope = database.CreateScope();
        return await call(scope.ServiceProvider.GetRequiredService<GoalService>());
    }

    private async Task<T> AdminAsync<T>(Func<CatalogAdministrationService, Task<T>> call)
    {
        await using var scope = database.CreateScope();
        return await call(scope.ServiceProvider.GetRequiredService<CatalogAdministrationService>());
    }

    private Task<GoalCreateResult> TryCreateAsync(ActingUser user, HabitId habit) =>
        GoalsAsync(s => s.CreateAsync(user, new GoalInput
        {
            Habit = new GoalHabit(habit, HabitKind.Catalog), Target = FifteenMinutesOnWeekdays, StartDate = Wed11,
        }, CancellationToken.None));

    private async Task<GoalId> CreateAsync(ActingUser user, HabitId habit)
    {
        var result = await TryCreateAsync(user, habit);
        Assert.Equal(GoalCreateOutcome.Created, result.Outcome);
        return result.GoalId!.Value;
    }

    private Task<CatalogHabitUnpublishResult> UnpublishAsync(HabitId habit, bool confirmed) =>
        AdminAsync(s => s.UnpublishAsync(Administrator, habit, confirmed, CancellationToken.None));

    private Task<ActingUser> UserAsync(string role, string suffix) =>
        HabitAccounts.RegisteredUserAsync(database.Services, database.ConnectionString, $"it-gc-{role}-{suffix}@example.com");

    private async Task<int> CountAsync(HabitId habit)
    {
        await using var scope = database.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<IGoalStore>().CountUsingCatalogHabitAsync(habit, CancellationToken.None);
    }

    private async Task<CatalogHabitState> StateAsync(HabitId habit)
    {
        await using var scope = database.CreateScope();
        return (await scope.ServiceProvider.GetRequiredService<ICatalogHabitStore>().FindAsync(habit, CancellationToken.None))!.State;
    }

    /// <summary>The settings a reader sees, as one comparable value.</summary>
    private static string Settings(GoalDetails goal) =>
        string.Join(" | ", goal.BuildOrReduce, goal.Measurement, goal.Amount, goal.DurationUnit, goal.ScheduleKind,
            string.Join(",", goal.Days.Order()), goal.TargetText, goal.ScheduleText, goal.StartDate, goal.TimeZone);

    [Fact]
    public async Task The_count_is_every_owners_goals_on_the_catalog_habit_and_nothing_else()
    {
        var suffix = Suffix();
        var first = await UserAsync("count-a", suffix);
        var second = await UserAsync("count-b", suffix);
        var reading = await CatalogHabitAsync(database.Services, $"It-Gc Count {suffix}");
        var other = await CatalogHabitAsync(database.Services, $"It-Gc Other {suffix}");
        var custom = await CustomHabitAsync(database.Services, first.AccountId!.Value, $"It-Gc Custom {suffix}");
        await CreateAsync(first, reading);
        await CreateAsync(first, reading);
        await CreateAsync(second, reading);
        await CreateAsync(second, other);
        var onCustom = await GoalsAsync(s => s.CreateAsync(first, new GoalInput
        {
            Habit = new GoalHabit(custom, HabitKind.Custom), Target = FifteenMinutesOnWeekdays, StartDate = Wed11,
        }, CancellationToken.None));
        Assert.Equal(GoalCreateOutcome.Created, onCustom.Outcome);

        Assert.Equal(3, await CountAsync(reading));
        Assert.Equal(1, await CountAsync(other));
        Assert.Equal(0, await CountAsync(custom));
        Assert.Equal(0, await CountAsync(new HabitId(Guid.NewGuid())));
    }

    [Fact]
    public async Task US7_AS1_AS2_AS6_a_confirmed_unpublish_keeps_the_goals_working_and_no_new_goal_can_be_based_on_the_habit()
    {
        var suffix = Suffix();
        var owner = await UserAsync("owner", suffix);
        var second = await UserAsync("second", suffix);
        var stranger = await UserAsync("stranger", suffix);
        var coffee = await CatalogHabitAsync(database.Services, $"It-Gc Coffee {suffix}", category: Category.Health);
        var id = await CreateAsync(owner, coffee);
        await CreateAsync(second, coffee);
        var before = (await GoalsAsync(s => s.GetAsync(owner, id, CancellationToken.None))).Goal!;

        var asked = await UnpublishAsync(coffee, confirmed: false);

        Assert.Equal(CatalogHabitUnpublishOutcome.ConfirmationNeeded, asked.Outcome);
        Assert.Equal(2, asked.GoalCount);
        Assert.Equal("2 goals use this habit.", asked.Message);
        Assert.Equal(CatalogHabitState.Published, await StateAsync(coffee));

        var confirmed = await UnpublishAsync(coffee, confirmed: true);

        Assert.Equal(CatalogHabitUnpublishOutcome.Unpublished, confirmed.Outcome);
        Assert.Null(confirmed.GoalCount);
        Assert.Equal(CatalogHabitState.Unpublished, await StateAsync(coffee));

        var listed = await GoalsAsync(s => s.ListAsync(owner, CancellationToken.None));
        var read = await GoalsAsync(s => s.GetAsync(owner, id, CancellationToken.None));
        Assert.Equal(id, Assert.Single(listed.Goals).Id);
        Assert.Equal($"It-Gc Coffee {suffix}", listed.Goals[0].HabitName);
        Assert.Equal(GoalReadOutcome.Found, read.Outcome);
        Assert.False(read.Goal!.Habit.IsInCatalog);
        Assert.True(before.Habit.IsInCatalog);
        Assert.Equal($"It-Gc Coffee {suffix}", read.Goal.Habit.Name);
        Assert.Equal(Settings(before), Settings(read.Goal));
        Assert.Equal(before.Name, read.Goal.Name);

        foreach (var user in new[] { owner, stranger })
        {
            var created = await TryCreateAsync(user, coffee);
            Assert.Equal(GoalCreateOutcome.NotInCatalog, created.Outcome);
            Assert.Equal("This habit is not in the catalog.", created.Message);
            var started = await GoalsAsync(s => s.StartNewGoalAsync(user, new GoalHabit(coffee, HabitKind.Catalog), null, CancellationToken.None));
            Assert.Equal(GoalStartOutcome.NotInCatalog, started.Outcome);
            Assert.Equal("This habit is not in the catalog.", started.Message);
        }

        Assert.Equal(2, await CountAsync(coffee));
        Assert.Empty((await GoalsAsync(s => s.ListAsync(stranger, CancellationToken.None))).Goals);
    }

    [Fact]
    public async Task US7_AS7_an_administrators_rename_shows_in_the_goal_and_its_derived_name()
    {
        var suffix = Suffix();
        var owner = await UserAsync("rename", suffix);
        var reading = await CatalogHabitAsync(database.Services, $"It-Gc Reading {suffix}");
        var id = await CreateAsync(owner, reading);

        var edited = await AdminAsync(s => s.EditAsync(Administrator, reading, new CatalogHabitInput
        {
            Name = $"It-Gc Books {suffix}", Category = Category.Mind, Intent = Intent.Build, Icon = "book-open", SuggestedStartingGoals = [],
        }, CancellationToken.None));
        Assert.Equal(CatalogHabitEditOutcome.Saved, edited.Outcome);

        var read = (await GoalsAsync(s => s.GetAsync(owner, id, CancellationToken.None))).Goal!;
        var listed = Assert.Single((await GoalsAsync(s => s.ListAsync(owner, CancellationToken.None))).Goals);
        Assert.Equal($"It-Gc Books {suffix}", read.Habit.Name);
        Assert.Equal($"It-Gc Books {suffix}, 15 min, 5 days a week", read.Name);
        Assert.Equal($"It-Gc Books {suffix}, 15 min, 5 days a week", listed.Name);
        Assert.Equal($"It-Gc Books {suffix}", listed.HabitName);
    }

    [Fact]
    public async Task FR_034_publishing_again_makes_new_goals_possible_again()
    {
        var suffix = Suffix();
        var owner = await UserAsync("again", suffix);
        var walking = await CatalogHabitAsync(database.Services, $"It-Gc Walking {suffix}", category: Category.Body);
        await CreateAsync(owner, walking);
        Assert.Equal(CatalogHabitUnpublishOutcome.Unpublished, (await UnpublishAsync(walking, confirmed: true)).Outcome);
        Assert.Equal(GoalCreateOutcome.NotInCatalog, (await TryCreateAsync(owner, walking)).Outcome);

        var published = await AdminAsync(s => s.PublishAsync(Administrator, walking, CancellationToken.None));

        Assert.Equal(CatalogHabitPublishOutcome.Published, published.Outcome);
        await CreateAsync(owner, walking);
        var read = await GoalsAsync(s => s.ListAsync(owner, CancellationToken.None));
        Assert.Equal(2, read.Goals.Count);
        Assert.Equal(2, await CountAsync(walking));
    }

    [Fact]
    public async Task A_goal_created_between_the_count_and_the_confirmed_unpublish_does_not_stop_it()
    {
        var suffix = Suffix();
        var owner = await UserAsync("race-a", suffix);
        var latecomer = await UserAsync("race-b", suffix);
        var yoga = await CatalogHabitAsync(database.Services, $"It-Gc Yoga {suffix}", category: Category.Body);
        await CreateAsync(owner, yoga);

        var asked = await UnpublishAsync(yoga, confirmed: false);
        Assert.Equal(1, asked.GoalCount);
        var late = await CreateAsync(latecomer, yoga);
        var confirmed = await UnpublishAsync(yoga, confirmed: true);

        Assert.Equal(CatalogHabitUnpublishOutcome.Unpublished, confirmed.Outcome);
        Assert.Equal(CatalogHabitState.Unpublished, await StateAsync(yoga));
        Assert.Equal(2, await CountAsync(yoga));
        var read = await GoalsAsync(s => s.GetAsync(latecomer, late, CancellationToken.None));
        Assert.Equal(GoalReadOutcome.Found, read.Outcome);
        Assert.False(read.Goal!.Habit.IsInCatalog);
    }
}
