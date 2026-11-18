using HT.Application.Accounts;
using HT.Application.Goals;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using Microsoft.Extensions.DependencyInjection;
using static HT.IntegrationTests.Goals.GoalSamples;

namespace HT.IntegrationTests.Goals;

/// <summary>
/// The evaluation calendar through <see cref="GoalService"/> on SQL Server (US10-AS3, US4-AS5, FR-011, FR-012, FR-013):
/// an account registered through <see cref="AccountService"/> with "Europe/Kyiv" creates its goals, which keep that zone
/// in their stored version, so their days run from midnight to midnight in Kyiv, also across 28 March 2027, when Kyiv
/// moves from UTC+2 to UTC+3 and the day lasts 23 hours. The fixture's clock is 09:00 UTC on Wednesday 11 November 2026,
/// so a start date of Monday 22 March 2027 is within a year. Every instant is worked out by hand. Shared database: own
/// prefixes ("it-gcal-", "It-Gcal").
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class GoalCalendarTests(DatabaseFixture database)
{
    private const string Password = "walking42";

    private static readonly DateOnly Mon22 = new(2027, 3, 22);
    private static readonly DateOnly Fri26 = new(2027, 3, 26);
    private static readonly DateOnly Sun28 = new(2027, 3, 28);
    private static readonly DateOnly Tue30 = new(2027, 3, 30);

    private static string Suffix() => Guid.NewGuid().ToString("N")[..8];

    private static DateTimeOffset Utc(int month, int day, int hour) => new(2027, month, day, hour, 0, 0, TimeSpan.Zero);

    private async Task<T> InScopeAsync<TService, T>(Func<TService, Task<T>> call)
        where TService : notnull
    {
        await using var scope = database.CreateScope();
        return await call(scope.ServiceProvider.GetRequiredService<TService>());
    }

    /// <summary>Registers with the browser time zone "Europe/Kyiv" and signs in, as the Register and Sign in pages do.</summary>
    private async Task<ActingUser> RegisterInKyivAsync(string email)
    {
        var registered = await InScopeAsync<AccountService, RegistrationResult>(s => s.RegisterAsync(email, Password, "Europe/Kyiv", null, CancellationToken.None));
        Assert.Equal(RegistrationOutcome.Registered, registered.Outcome);
        var signedIn = await InScopeAsync<AccountService, SignInResult>(s => s.SignInAsync(email, Password, CancellationToken.None));
        Assert.Equal(SignInOutcome.SignedIn, signedIn.Outcome);
        return signedIn.ActingUser!;
    }

    private async Task<GoalId> CreateAsync(ActingUser user, HabitId habit, GoalTargetFields target)
    {
        var created = await InScopeAsync<GoalService, GoalCreateResult>(s => s.CreateAsync(
            user, new GoalInput { Habit = new GoalHabit(habit, HabitKind.Catalog), Target = target, StartDate = Mon22 }, CancellationToken.None));
        Assert.Equal(GoalCreateOutcome.Created, created.Outcome);
        return created.GoalId!.Value;
    }

    private async Task<GoalCalendarView> CalendarAsync(ActingUser user, GoalId goal, DateOnly first, DateOnly last)
    {
        var result = await InScopeAsync<GoalService, GoalCalendarResult>(s => s.GetCalendarAsync(user, goal, first, last, CancellationToken.None));
        Assert.Equal(GoalCalendarOutcome.Found, result.Outcome);
        return result.Calendar!;
    }

    private static GoalTargetFields Times(int times, SchedulePeriod period) => new()
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo, ScheduleKind = ScheduleKind.Times, Times = times, Period = period,
    };

    [Fact]
    public async Task US10_AS3_a_goal_of_a_user_in_Kyiv_has_Kyiv_midnights_across_the_clock_change_of_28_March_2027()
    {
        var suffix = Suffix();
        var user = await RegisterInKyivAsync($"it-gcal-kyiv-{suffix}@example.com");
        var habit = await CatalogHabitAsync(database.Services, $"It-Gcal Reading {suffix}");
        var daily = await CreateAsync(user, habit, Times(1, SchedulePeriod.Day));

        var read = await InScopeAsync<GoalService, GoalReadResult>(s => s.GetAsync(user, daily, CancellationToken.None));
        var calendar = await CalendarAsync(user, daily, Fri26, Tue30);

        Assert.Equal("Europe/Kyiv", read.Goal!.TimeZone);
        Assert.Equal(Mon22, read.Goal.TimeZoneAppliesFrom);
        Assert.Equal(GoalScheduleKind.PeriodBased, calendar.Kind);
        Assert.Empty(calendar.Commitments);
        Assert.Equal(
            [
                new EvaluationPeriod(Fri26, Fri26, Utc(3, 25, 22), Utc(3, 26, 22)),
                new EvaluationPeriod(new(2027, 3, 27), new(2027, 3, 27), Utc(3, 26, 22), Utc(3, 27, 22)),
                new EvaluationPeriod(Sun28, Sun28, Utc(3, 27, 22), Utc(3, 28, 21)),
                new EvaluationPeriod(new(2027, 3, 29), new(2027, 3, 29), Utc(3, 28, 21), Utc(3, 29, 21)),
                new EvaluationPeriod(Tue30, Tue30, Utc(3, 29, 21), Utc(3, 30, 21)),
            ],
            calendar.Periods);
    }

    [Fact]
    public async Task US10_AS3_the_week_and_the_commitment_of_28_March_2027_end_at_Kyiv_midnight_in_summer_time()
    {
        var suffix = Suffix();
        var user = await RegisterInKyivAsync($"it-gcal-week-{suffix}@example.com");
        var habit = await CatalogHabitAsync(database.Services, $"It-Gcal Yoga {suffix}");
        var weekly = await CreateAsync(user, habit, Times(3, SchedulePeriod.Week));
        var sundays = await CreateAsync(user, habit, new GoalTargetFields
        {
            BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.YesNo, ScheduleKind = ScheduleKind.FixedDays, Days = new HashSet<DayOfWeek> { DayOfWeek.Sunday },
        });

        var week = Assert.Single((await CalendarAsync(user, weekly, Sun28, Sun28)).Periods);
        var commitment = Assert.Single((await CalendarAsync(user, sundays, Mon22, Sun28)).Commitments);

        Assert.Equal(new EvaluationPeriod(Mon22, Sun28, Utc(3, 21, 22), Utc(3, 28, 21)), week);
        Assert.Equal(new Commitment(Sun28, Utc(3, 27, 22), Utc(3, 28, 21)), commitment);
    }
}
