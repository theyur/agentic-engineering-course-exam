using Bunit;
using HT.Application.Goals;
using HT.Application.Habits;
using HT.Application.Records;
using HT.Application.Reports;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using HT.Domain.Time;
using HT.UnitTests.Fakes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace HT.UiTests.Components.Pages;

/// <summary>
/// Slice 8 US3's goal in bUnit (T092), made through the real use cases over the registered fakes (<see cref="PageServices"/>)
/// while each step was current, by moving the fake clock forward: "Meditation, 10 min daily" (Build, duration, all seven
/// days) created on Monday 2 November 2026 at 08:00 with that start date, 10 minutes on 2 to 13 November, ended on Monday
/// 16 November at 10:00 with "Abandoned" and "Too busy this month." Its last date is Sunday 15 November, so 14 and 15
/// November are Missed once their grace periods end (at 00:00 on 17 and 18 November). The user has no stored zone: UTC.
/// </summary>
public static class MeditationAttempts
{
    public const string Name = "Meditation, 10 min daily";
    public const string Note = "Too busy this month.";

    public static readonly DateOnly Mon2 = new(2026, 11, 2);
    public static readonly DateOnly Sat14 = new(2026, 11, 14);
    public static readonly DateOnly Sun15 = new(2026, 11, 15);
    public static readonly DateOnly Mon16 = new(2026, 11, 16);
    public static readonly DateOnly Wed18 = new(2026, 11, 18);
    public static readonly DateOnly Mon23 = new(2026, 11, 23);

    public static DateTimeOffset Utc(int day, int hour, int minute = 0) => new(2026, 11, day, hour, minute, 0, TimeSpan.Zero);

    public static GoalTargetFields MinutesDaily(int minutes) => new()
    {
        BuildOrReduce = BuildOrReduce.Build, Measurement = Measurement.Duration, Amount = minutes, DurationUnit = DurationUnit.Minutes,
        ScheduleKind = ScheduleKind.FixedDays, Days = Enum.GetValues<DayOfWeek>().ToHashSet(),
    };

    private static FakeTimeProvider Clock(BunitContext context) => (FakeTimeProvider)context.Services.GetRequiredService<TimeProvider>();

    /// <summary>The ended goal; leaves the clock at Monday 16 November, 10:00, the instant of the ending.</summary>
    public static async Task<GoalId> EndedAsync(BunitContext context, ActingUser user)
    {
        ArgumentNullException.ThrowIfNull(context);
        var catalog = (FakeCatalogHabitStore)context.Services.GetRequiredService<ICatalogHabitStore>();
        var records = (FakeRecordStore)context.Services.GetRequiredService<IRecordStore>();
        var meditation = catalog.Add(HabitSamples.Catalog("Meditation"));
        Clock(context).SetUtcNow(Utc(2, 8));
        GoalId goal;
        await using (var scope = context.Services.CreateAsyncScope())
        {
            var created = await scope.ServiceProvider.GetRequiredService<GoalService>().CreateAsync(user, new GoalInput
            {
                Habit = new GoalHabit(meditation.Id, HabitKind.Catalog), Target = MinutesDaily(10), StartDate = Mon2,
            }, CancellationToken.None);
            Assert.Equal(GoalCreateOutcome.Created, created.Outcome);
            goal = created.GoalId!.Value;
        }

        for (var date = Mon2; date < Sat14; date = date.AddDays(1))
        {
            records.Seed(goal, date, RecordValue.FromAmount(10));
        }

        Clock(context).SetUtcNow(Utc(16, 10));
        await using (var scope = context.Services.CreateAsyncScope())
        {
            var ended = await scope.ServiceProvider.GetRequiredService<GoalChangeService>().EndAsync(
                user, goal, Outcome.Abandoned, Note, CancellationToken.None);
            Assert.Equal((GoalEndOutcome.Ended, (DateOnly?)Mon16, (DateOnly?)Sun15), (ended.Outcome, ended.EndingDate, ended.LastDate));
        }

        return goal;
    }

    /// <summary>Starts a new attempt at <paramref name="at"/>, from <paramref name="start"/> with <paramref name="target"/>; the clock stays at <paramref name="at"/>.</summary>
    public static async Task StartAttemptAsync(
        BunitContext context, ActingUser user, GoalId goal, DateTimeOffset at, GoalTargetFields target, DateOnly start)
    {
        ArgumentNullException.ThrowIfNull(context);
        Clock(context).SetUtcNow(at);
        await using var scope = context.Services.CreateAsyncScope();
        var started = await scope.ServiceProvider.GetRequiredService<GoalChangeService>().StartAttemptAsync(
            user, goal, new NewAttemptInput { Target = target, StartDate = start }, CancellationToken.None);
        Assert.Equal(AttemptStartOutcome.Started, started.Outcome);
    }

    /// <summary>
    /// US3-AS2: the ended goal restarted on Wednesday 18 November at 08:00 with unchanged settings from that date; the clock
    /// is then moved to <paramref name="readAt"/>.
    /// </summary>
    public static async Task<GoalId> RestartedAsync(BunitContext context, ActingUser user, DateTimeOffset readAt)
    {
        var goal = await EndedAsync(context, user);
        await StartAttemptAsync(context, user, goal, Utc(18, 8), MinutesDaily(10), Wed18);
        Clock(context).SetUtcNow(readAt);
        return goal;
    }

    /// <summary>
    /// The same goal, restarted with <paramref name="secondTarget"/> from <paramref name="secondStart"/>, stored straight in
    /// the goal fake through the attempts constructor (as slice 8's seeded goals are), with its records seeded, for a test
    /// whose clock is already past Monday 2 November and cannot go back. Attempt 1 has no ending note.
    /// </summary>
    public static GoalId SeedRestarted(BunitContext context, ActingUser user, GoalTargetFields secondTarget, DateOnly secondStart)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(user);
        var catalog = (FakeCatalogHabitStore)context.Services.GetRequiredService<ICatalogHabitStore>();
        var goals = (FakeGoalStore)context.Services.GetRequiredService<IGoalStore>();
        var records = (FakeRecordStore)context.Services.GetRequiredService<IRecordStore>();
        var meditation = catalog.Add(HabitSamples.Catalog("Meditation"));
        var owner = user.AccountId!.Value;
        var goal = new Goal(new GoalId(Guid.NewGuid()), owner, new GoalHabit(meditation.Id, HabitKind.Catalog), null,
        [
            new GoalAttemptVersions(new GoalAttempt(GoalAttempt.First, Mon2) { Ending = new GoalEnding(Mon16, Outcome.Abandoned, null) },
                [VersionOf(MinutesDaily(10), Mon2)]),
            new GoalAttemptVersions(new GoalAttempt(2, secondStart), [VersionOf(secondTarget, secondStart)]),
        ]);
        goals.Add(owner, goal, new GoalHabitView(meditation.Id, HabitKind.Catalog, "Meditation", meditation.Definition.Category, true));
        for (var date = Mon2; date < Sat14; date = date.AddDays(1))
        {
            records.Seed(goal.Id, date, RecordValue.FromAmount(10));
        }

        return goal.Id;
    }

    private static GoalSettingsVersion VersionOf(GoalTargetFields fields, DateOnly from)
    {
        Assert.True(GoalTarget.TryCreate(fields, out var target, out _));
        return new GoalSettingsVersion(new GoalSettings(target, NamedTimeZone.Utc), from);
    }

    /// <summary>The goal's attempts at the clock's time, the latest first, as the goal page reads them.</summary>
    public static async Task<IReadOnlyList<AttemptView>> AttemptsAsync(BunitContext context, ActingUser user, GoalId goal)
    {
        ArgumentNullException.ThrowIfNull(context);
        await using var scope = context.Services.CreateAsyncScope();
        var read = await scope.ServiceProvider.GetRequiredService<AttemptService>().GetAttemptsAsync(user, goal, CancellationToken.None);
        Assert.Equal(AttemptListOutcome.Found, read.Outcome);
        return read.Attempts;
    }

    /// <summary>One attempt with its History at the clock's time, as the attempt view reads it.</summary>
    public static async Task<AttemptDetails> AttemptAsync(BunitContext context, ActingUser user, GoalId goal, int number)
    {
        ArgumentNullException.ThrowIfNull(context);
        await using var scope = context.Services.CreateAsyncScope();
        var read = await scope.ServiceProvider.GetRequiredService<AttemptService>().GetAttemptAsync(user, goal, number, CancellationToken.None);
        Assert.Equal(AttemptReadOutcome.Found, read.Outcome);
        return read.Details!;
    }
}
