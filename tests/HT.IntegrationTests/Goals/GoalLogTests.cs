using HT.Application;
using HT.Application.Goals;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.IntegrationTests.Habits;
using HT.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using static HT.IntegrationTests.Goals.GoalSamples;

namespace HT.IntegrationTests.Goals;

/// <summary>
/// HP-011 (constitution III, research R-08): every goal use case runs on SQL Server with every log entry captured at
/// <see cref="LogLevel.Trace"/>, and no entry's message or exception text contains the goal's name, the custom habit's
/// name, the unit or the amount. Every <see cref="GoalService"/> method runs: starting values and creation, each with
/// each kind of refusal, listing, reading, the calendar with its refusals, and a read of another user's goal. The
/// values are distinctive, so a hit means a leak. The fixture's clock is 09:00 UTC on Wednesday 11 November 2026.
/// Shared database: own prefixes ("it-gl-", "It-Gl").
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class GoalLogTests(DatabaseFixture database)
{
    private const string GoalName = "It-Gl Zq private evening goal";
    private const string CustomHabitName = "It-Gl Zq private ritual";
    private const string Unit = "zq-private-sips";
    private const int Amount = 983;

    /// <summary>Today in UTC at the fixture's clock.</summary>
    private static readonly DateOnly Wed11 = new(2026, 11, 11);

    private static string Suffix() => Guid.NewGuid().ToString("N")[..8];

    /// <summary>The application as the integration tests build it, with the fixture's clock and only <paramref name="capture"/> as its logger.</summary>
    private ServiceProvider BuildServices(LogCapture capture)
    {
        var services = new ServiceCollection()
            .AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(LogLevel.Trace);
                logging.AddProvider(capture);
            })
            .AddHtApplication()
            .AddHtPersistence(database.ConnectionString);
        services.AddSingleton<TimeProvider>(database.Clock);
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
    }

    private static async Task<T> GoalsAsync<T>(IServiceProvider services, Func<GoalService, Task<T>> call)
    {
        await using var scope = services.CreateAsyncScope();
        return await call(scope.ServiceProvider.GetRequiredService<GoalService>());
    }

    private static GoalTargetFields CountTarget(BuildOrReduce side = BuildOrReduce.Build, int times = 3, SchedulePeriod period = SchedulePeriod.Week, string unit = Unit) => new()
    {
        BuildOrReduce = side, Measurement = Measurement.Count, Amount = Amount, CountUnit = unit,
        ScheduleKind = ScheduleKind.Times, Times = times, Period = period,
    };

    private static GoalInput Input(HabitId habit, HabitKind kind, GoalTargetFields? target = null, DateOnly? start = null, string? name = GoalName) => new()
    {
        Habit = new GoalHabit(habit, kind), Target = target ?? CountTarget(), StartDate = start ?? Wed11, Name = name,
    };

    /// <summary>A result's text must not echo a protected value either.</summary>
    private static void AssertClean(string? message)
    {
        if (message is null)
        {
            return;
        }

        foreach (var value in new[] { GoalName, CustomHabitName, Unit, Amount.ToString(System.Globalization.CultureInfo.InvariantCulture) })
        {
            Assert.DoesNotContain(value, message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task HP_011_no_goal_use_case_logs_a_goal_name_custom_habit_name_unit_or_amount()
    {
        var suffix = Suffix();
        var capture = new LogCapture();
        await using var services = BuildServices(capture);
        var user = await HabitAccounts.RegisteredUserAsync(services, database.ConnectionString, $"it-gl-owner-{suffix}@example.com");
        var other = await HabitAccounts.RegisteredUserAsync(services, database.ConnectionString, $"it-gl-other-{suffix}@example.com");
        var administrator = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.Administrator);
        var custom = await CustomHabitAsync(services, user.AccountId!.Value, $"{CustomHabitName} {suffix}");
        var othersCustom = await CustomHabitAsync(services, other.AccountId!.Value, $"{CustomHabitName} other {suffix}");
        var draft = await CatalogHabitAsync(services, $"It-Gl Draft {suffix}", CatalogHabitState.Draft);

        // Starting values: success on the own custom habit, and each kind of refusal.
        var starts = new (ActingUser User, GoalHabit? Habit, int? Position, GoalStartOutcome Outcome)[]
        {
            (user, new GoalHabit(custom, HabitKind.Custom), null, GoalStartOutcome.Started),
            (ActingUser.Anonymous, new GoalHabit(custom, HabitKind.Custom), null, GoalStartOutcome.NotSignedIn),
            (administrator, new GoalHabit(custom, HabitKind.Custom), null, GoalStartOutcome.NotForAdministrators),
            (user, new GoalHabit(draft, HabitKind.Catalog), null, GoalStartOutcome.NotInCatalog),
            (user, new GoalHabit(othersCustom, HabitKind.Custom), null, GoalStartOutcome.CustomHabitNotFound),
            (user, new GoalHabit(custom, HabitKind.Custom), 1, GoalStartOutcome.SuggestionNotFound),
        };
        foreach (var (actingUser, habit, position, outcome) in starts)
        {
            var started = await GoalsAsync(services, s => s.StartNewGoalAsync(actingUser, habit, position, CancellationToken.None));
            Assert.Equal(outcome, started.Outcome);
            AssertClean(started.Message);
        }

        // Creation: two goals, one with the own name and one with the derived name (which holds the custom habit's name),
        // and a refusal of each kind: the acting user, the habit, a target field, the Reduce N rule, the start date, the name.
        var creations = new (ActingUser User, GoalInput Input, GoalCreateOutcome Outcome)[]
        {
            (user, Input(custom, HabitKind.Custom), GoalCreateOutcome.Created),
            (user, Input(custom, HabitKind.Custom, name: null), GoalCreateOutcome.Created),
            (ActingUser.Anonymous, Input(custom, HabitKind.Custom), GoalCreateOutcome.NotSignedIn),
            (administrator, Input(custom, HabitKind.Custom), GoalCreateOutcome.NotForAdministrators),
            (user, Input(custom, HabitKind.Custom) with { Habit = null }, GoalCreateOutcome.HabitMissing),
            (user, Input(draft, HabitKind.Catalog), GoalCreateOutcome.NotInCatalog),
            (user, Input(othersCustom, HabitKind.Custom), GoalCreateOutcome.CustomHabitNotFound),
            (user, Input(custom, HabitKind.Custom, CountTarget(unit: Unit + "-far-too-long-unit")), GoalCreateOutcome.CountUnitTooLong),
            (user, Input(custom, HabitKind.Custom, CountTarget(BuildOrReduce.Reduce, 2, SchedulePeriod.Day)), GoalCreateOutcome.ReduceAmountNeedsOneTime),
            (user, Input(custom, HabitKind.Custom, start: Wed11.AddDays(-1)), GoalCreateOutcome.StartDateInPast),
            (user, Input(custom, HabitKind.Custom, start: Wed11.AddDays(366)), GoalCreateOutcome.StartDateTooFar),
            (user, Input(custom, HabitKind.Custom, name: GoalName + new string('x', 40)), GoalCreateOutcome.NameTooLong),
        };
        var created = new List<GoalId>();
        foreach (var (actingUser, input, outcome) in creations)
        {
            var result = await GoalsAsync(services, s => s.CreateAsync(actingUser, input, CancellationToken.None));
            Assert.Equal(outcome, result.Outcome);
            AssertClean(result.Message);
            if (result.GoalId is { } id)
            {
                created.Add(id);
            }
        }

        Assert.Equal(2, created.Count);
        var othersGoal = await GoalsAsync(services, s => s.CreateAsync(other, Input(othersCustom, HabitKind.Custom), CancellationToken.None));
        Assert.Equal(GoalCreateOutcome.Created, othersGoal.Outcome);

        // Listing, reading and the calendar, for the owner and for each refusal.
        foreach (var actingUser in new[] { user, ActingUser.Anonymous, administrator })
        {
            AssertClean((await GoalsAsync(services, s => s.ListAsync(actingUser, CancellationToken.None))).Message);
            foreach (var id in created)
            {
                AssertClean((await GoalsAsync(services, s => s.GetAsync(actingUser, id, CancellationToken.None))).Message);
                AssertClean((await GoalsAsync(services, s => s.GetCalendarAsync(actingUser, id, Wed11, Wed11.AddDays(13), CancellationToken.None))).Message);
            }
        }

        var listed = await GoalsAsync(services, s => s.ListAsync(user, CancellationToken.None));
        Assert.Contains(listed.Goals, g => g.Name == GoalName);
        var outOfRange = await GoalsAsync(services, s => s.GetCalendarAsync(user, created[0], Wed11, Wed11.AddDays(366), CancellationToken.None));
        Assert.Equal(GoalCalendarOutcome.RangeOutOfBounds, outOfRange.Outcome);
        var unknown = await GoalsAsync(services, s => s.GetAsync(user, new GoalId(Guid.NewGuid()), CancellationToken.None));
        Assert.Equal(GoalReadOutcome.NotFound, unknown.Outcome);

        // Another user's goal, read and its calendar, by the owner of the first goals.
        var notMine = await GoalsAsync(services, s => s.GetAsync(user, othersGoal.GoalId!.Value, CancellationToken.None));
        var notMyCalendar = await GoalsAsync(services, s => s.GetCalendarAsync(user, othersGoal.GoalId!.Value, Wed11, Wed11, CancellationToken.None));
        Assert.Equal(GoalReadOutcome.NotFound, notMine.Outcome);
        Assert.Equal(GoalCalendarOutcome.NotFound, notMyCalendar.Outcome);
        foreach (var message in new[] { outOfRange.Message, unknown.Message, notMine.Message, notMyCalendar.Message })
        {
            AssertClean(message);
        }

        Assert.NotEmpty(capture.Entries);
        capture.AssertContainsNone(GoalName, CustomHabitName, Unit, $" {Amount}", $"{Amount} ", $"={Amount}", $"'{Amount}'");
    }
}
