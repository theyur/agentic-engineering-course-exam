using HT.Application.Goals;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.UnitTests.Domain.Goals;
using HT.UnitTests.Fakes;

namespace HT.UnitTests.Application.Goals;

/// <summary>
/// Acting users and inputs for the goal use-case tests. Every goal use case reads the acting user's account, so the
/// acting users here are accounts of the given <see cref="FakeAccountStore"/>: with a stored time zone, or with none,
/// which has the installation's default. Habit tests that create goals through <c>GoalService</c> use these users,
/// not <c>CatalogInputs</c>' accounts that no store holds.
/// </summary>
public static class GoalInputs
{
    public const string Kyiv = "Europe/Kyiv";

    /// <summary>A registered user stored with <paramref name="timeZone"/>; null stores no zone (the default).</summary>
    public static ActingUser RegisteredUser(FakeAccountStore accounts, string? timeZone = Kyiv) =>
        Stored(accounts, Role.RegisteredUser, timeZone);

    /// <summary>A registered user stored with no time zone, as an account created before slice 4.</summary>
    public static ActingUser RegisteredUserWithDefaultZone(FakeAccountStore accounts) =>
        Stored(accounts, Role.RegisteredUser, null);

    /// <summary>An administrator stored with <paramref name="timeZone"/>.</summary>
    public static ActingUser Administrator(FakeAccountStore accounts, string? timeZone = Kyiv) =>
        Stored(accounts, Role.Administrator, timeZone);

    public static GoalHabit Catalog(CatalogHabit habit)
    {
        ArgumentNullException.ThrowIfNull(habit);
        return new GoalHabit(habit.Id, HabitKind.Catalog);
    }

    public static GoalHabit Custom(CustomHabit habit)
    {
        ArgumentNullException.ThrowIfNull(habit);
        return new GoalHabit(habit.Id, HabitKind.Custom);
    }

    /// <summary>The marked suggestion of <see cref="HabitSamples.Reading"/>: 15 min, Mon–Fri, Build.</summary>
    public static GoalTargetFields FifteenMinutesOnWeekdays { get; } =
        GoalSamples.DurationDays(15, DurationUnit.Minutes, BuildOrReduce.Build, GoalSamples.Weekdays);

    /// <summary>A goal input; the target defaults to <see cref="FifteenMinutesOnWeekdays"/>.</summary>
    public static GoalInput Input(
        GoalHabit? habit, DateOnly? startDate, GoalTargetFields? target = null, string? name = null) => new()
    {
        Habit = habit,
        Target = target ?? FifteenMinutesOnWeekdays,
        StartDate = startDate,
        Name = name,
    };

    private static ActingUser Stored(FakeAccountStore accounts, Role role, string? timeZone)
    {
        ArgumentNullException.ThrowIfNull(accounts);
        var account = accounts.Add($"goal-user-{Guid.NewGuid():N}@example.com", "secret-pass-1", role, timeZone: timeZone);
        return ActingUser.ForAccount(account.Id, role);
    }
}
