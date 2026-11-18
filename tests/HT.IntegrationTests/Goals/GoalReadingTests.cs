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
/// Listing and reading goals through <see cref="GoalService"/> on SQL Server (US2, US7-AS6, US7-AS7, FR-014 to FR-016,
/// FR-020): name order over the stored creation order, the habit's current name after an administrator renames it,
/// the habit's state after an unpublish, and owner scoping. The fixture's clock is 09:00 UTC on Wednesday 11 November
/// 2026 and the accounts are in UTC. Shared database: own prefixes ("it-gr-", "It-Gr"); each user's own list is
/// checked whole, other lists only by exclusion.
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class GoalReadingTests(DatabaseFixture database)
{
    private static readonly ActingUser Administrator = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.Administrator);

    /// <summary>Today in UTC at the fixture's clock.</summary>
    private static readonly DateOnly Wed11 = new(2026, 11, 11);

    private static readonly DateOnly Mon16 = new(2026, 11, 16);

    private static string Suffix() => Guid.NewGuid().ToString("N")[..8];

    private async Task<T> GoalsAsync<T>(Func<GoalService, Task<T>> call)
    {
        await using var scope = database.CreateScope();
        return await call(scope.ServiceProvider.GetRequiredService<GoalService>());
    }

    private async Task<GoalId> CreateAsync(ActingUser user, HabitId habit, HabitKind kind, GoalTargetFields target, DateOnly start, string? name = null)
    {
        var result = await GoalsAsync(s => s.CreateAsync(user, new GoalInput { Habit = new GoalHabit(habit, kind), Target = target, StartDate = start, Name = name }, CancellationToken.None));
        Assert.Equal(GoalCreateOutcome.Created, result.Outcome);
        return result.GoalId!.Value;
    }

    private static GoalTargetFields FifteenMinutesOnWeekdays => new()
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = 15, DurationUnit = DurationUnit.Minutes,
        ScheduleKind = ScheduleKind.FixedDays, Days = HabitSamples.Weekdays.ToHashSet(),
    };

    private static GoalTargetFields ThreeTimesAWeek => new()
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo, ScheduleKind = ScheduleKind.Times, Times = 3, Period = SchedulePeriod.Week,
    };

    [Fact]
    public async Task The_owner_lists_in_name_order_and_reads_the_habits_current_name_after_an_administrator_renames_it()
    {
        var suffix = Suffix();
        var user = await HabitAccounts.RegisteredUserAsync(database.Services, database.ConnectionString, $"it-gr-owner-{suffix}@example.com");
        var reading = await CatalogHabitAsync(database.Services, $"It-Gr Reading {suffix}");
        var yoga = await CatalogHabitAsync(database.Services, $"It-Gr Yoga {suffix}", category: Category.Body);
        var custom = await CustomHabitAsync(database.Services, user.AccountId!.Value, $"It-Gr Stretch {suffix}");
        var yogaGoal = await CreateAsync(user, yoga, HabitKind.Catalog, ThreeTimesAWeek, Mon16);
        var readingGoal = await CreateAsync(user, reading, HabitKind.Catalog, FifteenMinutesOnWeekdays, Wed11);
        var namedGoal = await CreateAsync(user, custom, HabitKind.Custom, ThreeTimesAWeek, Wed11, $"it-gr apple {suffix}");

        var listed = await GoalsAsync(s => s.ListAsync(user, CancellationToken.None));

        Assert.Equal(GoalListOutcome.Found, listed.Outcome);
        Assert.Equal(Wed11, listed.Today);
        Assert.Equal([namedGoal, readingGoal, yogaGoal], listed.Goals.Select(g => g.Id));
        var readingItem = listed.Goals[1];
        Assert.Equal($"It-Gr Reading {suffix}, 15 min, 5 days a week", readingItem.Name);
        Assert.Equal($"It-Gr Reading {suffix}", readingItem.HabitName);
        Assert.Equal(HabitKind.Catalog, readingItem.HabitKind);
        Assert.Equal("Mon–Fri", readingItem.ScheduleText);
        Assert.Equal(Wed11, readingItem.NextCommitment);
        Assert.Null(readingItem.StartDate);
        var yogaItem = listed.Goals[2];
        Assert.Equal(Mon16, yogaItem.StartDate);
        Assert.Equal(Mon16, yogaItem.FirstWeekStarts);
        Assert.Equal(HabitKind.Custom, listed.Goals[0].HabitKind);

        var edited = await AdminAsync(s => s.EditAsync(Administrator, reading, new CatalogHabitInput
        {
            Name = $"It-Gr Books {suffix}", Category = Category.Mind, Intent = Intent.Build, Icon = "book-open", SuggestedStartingGoals = [],
        }, CancellationToken.None));
        Assert.Equal(CatalogHabitEditOutcome.Saved, edited.Outcome);

        var renamed = await GoalsAsync(s => s.ListAsync(user, CancellationToken.None));
        Assert.Equal([namedGoal, readingGoal, yogaGoal], renamed.Goals.Select(g => g.Id));
        Assert.Equal($"It-Gr Books {suffix}, 15 min, 5 days a week", renamed.Goals[1].Name);
        Assert.Equal($"It-Gr Books {suffix}", renamed.Goals[1].HabitName);
        var read = await GoalsAsync(s => s.GetAsync(user, readingGoal, CancellationToken.None));
        Assert.Equal(GoalReadOutcome.Found, read.Outcome);
        Assert.Equal($"It-Gr Books {suffix}, 15 min, 5 days a week", read.Goal!.Name);
        Assert.Equal(new GoalHabitView(reading, HabitKind.Catalog, $"It-Gr Books {suffix}", Category.Mind, true), read.Goal.Habit);
        Assert.Equal("UTC", read.Goal.TimeZone);
        Assert.Equal(Wed11, read.Goal.TimeZoneAppliesFrom);
        Assert.Null(read.Goal.LaterTimeZone);
        Assert.True(read.Goal.HasStarted);
        Assert.Equal(1, read.Goal.AttemptNumber);
        Assert.Equal(Wed11, read.Goal.AttemptStartDate);
    }

    [Fact]
    public async Task US7_AS6_a_goal_on_an_Unpublished_habit_is_still_listed_and_read_and_says_it_is_not_in_the_catalog()
    {
        var suffix = Suffix();
        var user = await HabitAccounts.RegisteredUserAsync(database.Services, database.ConnectionString, $"it-gr-unpub-{suffix}@example.com");
        var coffee = await CatalogHabitAsync(database.Services, $"It-Gr Coffee {suffix}", category: Category.Health);
        var id = await CreateAsync(user, coffee, HabitKind.Catalog, FifteenMinutesOnWeekdays, Wed11);
        await using (var scope = database.CreateScope())
        {
            Assert.True(await scope.ServiceProvider.GetRequiredService<ICatalogHabitStore>()
                .TrySetStateAsync(coffee, CatalogHabitState.Published, CatalogHabitState.Unpublished, CancellationToken.None));
        }

        var listed = await GoalsAsync(s => s.ListAsync(user, CancellationToken.None));
        var read = await GoalsAsync(s => s.GetAsync(user, id, CancellationToken.None));

        Assert.Equal(id, Assert.Single(listed.Goals).Id);
        Assert.Equal(GoalReadOutcome.Found, read.Outcome);
        Assert.False(read.Goal!.Habit.IsInCatalog);
        Assert.Equal($"It-Gr Coffee {suffix}", read.Goal.Habit.Name);
        Assert.Equal("15 min, 5 days a week", read.Goal.TargetText);
    }

    [Fact]
    public async Task Another_user_neither_lists_nor_reads_the_owners_goals()
    {
        var suffix = Suffix();
        var owner = await HabitAccounts.RegisteredUserAsync(database.Services, database.ConnectionString, $"it-gr-a-{suffix}@example.com");
        var other = await HabitAccounts.RegisteredUserAsync(database.Services, database.ConnectionString, $"it-gr-b-{suffix}@example.com");
        var habit = await CatalogHabitAsync(database.Services, $"It-Gr Scoped {suffix}");
        var ownersGoal = await CreateAsync(owner, habit, HabitKind.Catalog, FifteenMinutesOnWeekdays, Wed11);
        var othersGoal = await CreateAsync(other, habit, HabitKind.Catalog, FifteenMinutesOnWeekdays, Wed11);

        var othersList = await GoalsAsync(s => s.ListAsync(other, CancellationToken.None));
        var read = await GoalsAsync(s => s.GetAsync(other, ownersGoal, CancellationToken.None));
        var unknown = await GoalsAsync(s => s.GetAsync(other, new GoalId(Guid.NewGuid()), CancellationToken.None));

        Assert.Equal(othersGoal, Assert.Single(othersList.Goals).Id);
        foreach (var refused in new[] { read, unknown })
        {
            Assert.Equal(GoalReadOutcome.NotFound, refused.Outcome);
            Assert.Equal("This goal does not exist.", refused.Message);
            Assert.Null(refused.Goal);
        }
    }

    private async Task<T> AdminAsync<T>(Func<CatalogAdministrationService, Task<T>> call)
    {
        await using var scope = database.CreateScope();
        return await call(scope.ServiceProvider.GetRequiredService<CatalogAdministrationService>());
    }
}
