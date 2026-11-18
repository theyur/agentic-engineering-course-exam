using HT.Application.Accounts;
using HT.Application.Goals;
using HT.Application.Operator;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using Microsoft.Extensions.DependencyInjection;
using static HT.IntegrationTests.Goals.GoalSamples;

namespace HT.IntegrationTests.Goals;

/// <summary>
/// US3, SC-002, FR-020 to FR-023 over SQL Server: the cases of the unit <c>GoalAccessTests</c>, for every
/// <see cref="GoalService"/> method, with two registered users created through <see cref="AccountService"/> and an
/// administrator created through <see cref="OperatorService"/>, each signed in as the Sign in page does. User A has
/// goals on a catalog habit and on A's own custom habit; user B has a goal and a custom habit. The fixture's clock is
/// 09:00 UTC on Wednesday 11 November 2026 and the accounts are in UTC. Shared database: own prefixes ("it-gp-",
/// "It-Gp"); goal rows are counted per owner, and lists of other users only by exclusion.
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class GoalPrivacyTests(DatabaseFixture database)
{
    private const string Password = "walking42";
    private const string AdministratorPassword = "curator2026";

    /// <summary>Today in UTC at the fixture's clock.</summary>
    private static readonly DateOnly Wed11 = new(2026, 11, 11);

    private static string Suffix() => Guid.NewGuid().ToString("N")[..8];

    private async Task<T> InScopeAsync<TService, T>(Func<TService, Task<T>> call)
        where TService : notnull
    {
        await using var scope = database.CreateScope();
        return await call(scope.ServiceProvider.GetRequiredService<TService>());
    }

    private Task<T> GoalsAsync<T>(Func<GoalService, Task<T>> call) => InScopeAsync(call);

    private async Task<ActingUser> SignInAsync(string email, string password)
    {
        var signedIn = await InScopeAsync<AccountService, SignInResult>(s => s.SignInAsync(email, password, CancellationToken.None));
        Assert.Equal(SignInOutcome.SignedIn, signedIn.Outcome);
        return signedIn.ActingUser!;
    }

    private async Task<ActingUser> RegisterAsync(string email)
    {
        var registered = await InScopeAsync<AccountService, RegistrationResult>(s => s.RegisterAsync(email, Password, CancellationToken.None));
        Assert.Equal(RegistrationOutcome.Registered, registered.Outcome);
        return await SignInAsync(email, Password);
    }

    private async Task<ActingUser> AdministratorAsync(string email)
    {
        var created = await InScopeAsync<OperatorService, AdministratorCreationResult>(s => s.CreateAdministratorAsync(email, AdministratorPassword, CancellationToken.None));
        Assert.Equal(AdministratorCreationOutcome.Created, created.Outcome);
        var administrator = await SignInAsync(email, AdministratorPassword);
        Assert.Equal(Role.Administrator, administrator.Role);
        return administrator;
    }

    private static GoalTargetFields ThreeTimesAWeek => new()
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo, ScheduleKind = ScheduleKind.Times, Times = 3, Period = SchedulePeriod.Week,
    };

    private async Task<GoalId> CreateAsync(ActingUser user, GoalHabit habit, string? name = null)
    {
        var created = await GoalsAsync(s => s.CreateAsync(user, new GoalInput { Habit = habit, Target = ThreeTimesAWeek, StartDate = Wed11, Name = name }, CancellationToken.None));
        Assert.Equal(GoalCreateOutcome.Created, created.Outcome);
        return created.GoalId!.Value;
    }

    /// <summary>Two registered users and their habits and goals, created fresh for one test.</summary>
    private sealed record World(ActingUser A, ActingUser B, GoalHabit Catalog, GoalHabit ACustom, GoalHabit BCustom, IReadOnlyList<GoalId> AGoals, GoalId BGoal);

    private async Task<World> WorldAsync(string suffix)
    {
        var a = await RegisterAsync($"it-gp-a-{suffix}@example.com");
        var b = await RegisterAsync($"it-gp-b-{suffix}@example.com");
        var catalog = new GoalHabit(await CatalogHabitAsync(database.Services, $"It-Gp Reading {suffix}"), HabitKind.Catalog);
        var aCustom = new GoalHabit(await CustomHabitAsync(database.Services, a.AccountId!.Value, $"It-Gp A stretch {suffix}"), HabitKind.Custom);
        var bCustom = new GoalHabit(await CustomHabitAsync(database.Services, b.AccountId!.Value, $"It-Gp B stretch {suffix}"), HabitKind.Custom);
        var aGoals = new[] { await CreateAsync(a, catalog), await CreateAsync(a, aCustom, $"It-Gp A private {suffix}") };
        var bGoal = await CreateAsync(b, catalog);
        return new World(a, b, catalog, aCustom, bCustom, aGoals, bGoal);
    }

    /// <summary>
    /// Every method, each with arguments that fail every later check of FR-023: B's custom habit with a position it does
    /// not have, a creation input that also has no target, a past start date and a long name, A's goal, and a range whose
    /// last date is before its first. Each entry is the outcome's name, its text and whether goal data came back.
    /// </summary>
    private async Task<IReadOnlyList<(string Outcome, string? Message, bool HasData)>> EveryUseCaseAsync(ActingUser user, World world)
    {
        var input = new GoalInput { Habit = world.BCustom, Target = new GoalTargetFields(), StartDate = Wed11.AddDays(-1), Name = new string('x', 61) };
        var goal = world.AGoals[0];
        var started = await GoalsAsync(s => s.StartNewGoalAsync(user, world.BCustom, 9, CancellationToken.None));
        var created = await GoalsAsync(s => s.CreateAsync(user, input, CancellationToken.None));
        var listed = await GoalsAsync(s => s.ListAsync(user, CancellationToken.None));
        var read = await GoalsAsync(s => s.GetAsync(user, goal, CancellationToken.None));
        var calendar = await GoalsAsync(s => s.GetCalendarAsync(user, goal, Wed11, Wed11.AddDays(-1), CancellationToken.None));
        return
        [
            (started.Outcome.ToString(), started.Message, started.StartingValues is not null),
            (created.Outcome.ToString(), created.Message, created.GoalId is not null),
            (listed.Outcome.ToString(), listed.Message, listed.Goals.Count > 0),
            (read.Outcome.ToString(), read.Message, read.Goal is not null),
            (calendar.Outcome.ToString(), calendar.Message, calendar.Calendar is not null),
        ];
    }

    private async Task<(int A, int B)> GoalRowsAsync(World world) =>
        (await GoalCountAsync(database.ConnectionString, world.A.AccountId!.Value), await GoalCountAsync(database.ConnectionString, world.B.AccountId!.Value));

    [Fact]
    public async Task US3_AS4_an_anonymous_visitor_is_not_signed_in_on_every_goal_use_case()
    {
        var world = await WorldAsync(Suffix());
        var rows = await GoalRowsAsync(world);

        var calls = await EveryUseCaseAsync(ActingUser.Anonymous, world);

        Assert.Equal(5, calls.Count);
        Assert.All(calls, call => Assert.Equal(("NotSignedIn", (string?)"You are not signed in.", false), call));
        Assert.Equal(rows, await GoalRowsAsync(world));
    }

    [Fact]
    public async Task US3_AS3_an_administrator_gets_the_role_refusal_and_no_goal_data_on_every_goal_use_case()
    {
        var suffix = Suffix();
        var world = await WorldAsync(suffix);
        var administrator = await AdministratorAsync($"it-gp-admin-{suffix}@example.com");
        var rows = await GoalRowsAsync(world);

        var calls = await EveryUseCaseAsync(administrator, world);

        Assert.Equal(5, calls.Count);
        Assert.All(calls, call => Assert.Equal(("NotForAdministrators", (string?)"Goals are for registered users.", false), call));
        Assert.Equal(rows, await GoalRowsAsync(world));
        Assert.Equal(0, await GoalCountAsync(database.ConnectionString, administrator.AccountId!.Value));
    }

    [Fact]
    public async Task US3_AS1_B_reading_As_goals_or_their_calendars_gets_the_same_result_as_for_an_unknown_identifier()
    {
        var world = await WorldAsync(Suffix());

        foreach (var id in world.AGoals.Append(new GoalId(Guid.NewGuid())))
        {
            var read = await GoalsAsync(s => s.GetAsync(world.B, id, CancellationToken.None));
            var calendar = await GoalsAsync(s => s.GetCalendarAsync(world.B, id, Wed11, Wed11.AddDays(13), CancellationToken.None));
            var badRange = await GoalsAsync(s => s.GetCalendarAsync(world.B, id, Wed11, Wed11.AddDays(-1), CancellationToken.None));

            Assert.Equal(GoalReadOutcome.NotFound, read.Outcome);
            Assert.Equal("This goal does not exist.", read.Message);
            Assert.Null(read.Goal);
            foreach (var refused in new[] { calendar, badRange })
            {
                Assert.Equal(GoalCalendarOutcome.NotFound, refused.Outcome);
                Assert.Equal("This goal does not exist.", refused.Message);
                Assert.Null(refused.Calendar);
            }
        }

        // The owner still reads both.
        foreach (var id in world.AGoals)
        {
            Assert.Equal(GoalReadOutcome.Found, (await GoalsAsync(s => s.GetAsync(world.A, id, CancellationToken.None))).Outcome);
            Assert.Equal(GoalCalendarOutcome.Found, (await GoalsAsync(s => s.GetCalendarAsync(world.A, id, Wed11, Wed11.AddDays(13), CancellationToken.None))).Outcome);
        }
    }

    [Fact]
    public async Task US3_AS2_Bs_list_holds_none_of_As_goals()
    {
        var world = await WorldAsync(Suffix());

        var bList = await GoalsAsync(s => s.ListAsync(world.B, CancellationToken.None));
        var aList = await GoalsAsync(s => s.ListAsync(world.A, CancellationToken.None));

        Assert.Equal(GoalListOutcome.Found, bList.Outcome);
        Assert.Equal([world.BGoal], bList.Goals.Select(g => g.Id));
        Assert.DoesNotContain(bList.Goals, g => world.AGoals.Contains(g.Id));
        Assert.Equal(world.AGoals.ToHashSet(), aList.Goals.Select(g => g.Id).ToHashSet());
        Assert.DoesNotContain(aList.Goals, g => g.Id == world.BGoal);
    }

    [Fact]
    public async Task US3_AS5_A_starting_or_creating_a_goal_on_Bs_custom_habit_gets_This_custom_habit_does_not_exist_and_nothing_is_created()
    {
        var world = await WorldAsync(Suffix());
        var rows = await GoalRowsAsync(world);

        var started = await GoalsAsync(s => s.StartNewGoalAsync(world.A, world.BCustom, null, CancellationToken.None));
        var created = await GoalsAsync(s => s.CreateAsync(world.A, new GoalInput { Habit = world.BCustom, Target = ThreeTimesAWeek, StartDate = Wed11 }, CancellationToken.None));

        Assert.Equal(GoalStartOutcome.CustomHabitNotFound, started.Outcome);
        Assert.Equal("This custom habit does not exist.", started.Message);
        Assert.Null(started.StartingValues);
        Assert.Equal(GoalCreateOutcome.CustomHabitNotFound, created.Outcome);
        Assert.Equal("This custom habit does not exist.", created.Message);
        Assert.Equal(GoalField.Habit, created.Field);
        Assert.Null(created.GoalId);
        Assert.Equal(rows, await GoalRowsAsync(world));
    }
}
