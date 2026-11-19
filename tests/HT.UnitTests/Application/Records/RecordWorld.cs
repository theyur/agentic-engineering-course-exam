using HT.Application.Accounts;
using HT.Application.Goals;
using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using HT.Domain.Time;
using HT.UnitTests.Application.Goals;
using HT.UnitTests.Fakes;
using Microsoft.Extensions.Time.Testing;

namespace HT.UnitTests.Application.Records;

/// <summary>
/// The fakes a <see cref="RecordingService"/> unit test needs, wired as the host wires the real stores (on the pattern
/// of <see cref="GoalWorld"/>): the slice-4 fakes, a <see cref="FakeRecordStore"/> sharing the goal store's goals, a fake
/// clock and the UTC installation default. It holds a registered user in UTC (no stored zone, the default), another
/// registered user and an administrator. Goals are created at a supplied instant through <see cref="GoalService"/>, whose
/// start date cannot be in the past, and then recorded and read at later instants by moving the clock.
/// </summary>
public sealed class RecordWorld
{
    public RecordWorld()
    {
        Goals = new FakeGoalStore(Accounts, CatalogHabits, CustomHabits);
        Records = new FakeRecordStore(Goals);
        User = GoalInputs.RegisteredUserWithDefaultZone(Accounts);
        OtherUser = GoalInputs.RegisteredUserWithDefaultZone(Accounts);
        Administrator = GoalInputs.Administrator(Accounts, timeZone: null);
        Habit = CatalogHabits.Add(HabitSamples.Catalog("Reading"));
    }

    public FakeAccountStore Accounts { get; } = new();

    public FakeCatalogHabitStore CatalogHabits { get; } = new();

    public FakeCustomHabitStore CustomHabits { get; } = new();

    public FakeGoalStore Goals { get; }

    public FakeRecordStore Records { get; }

    /// <summary>The clock, from 00:00 UTC on 1 November 2026; it only moves forward, and every test sets it before it creates a goal.</summary>
    public FakeTimeProvider Clock { get; } = new(new DateTimeOffset(2026, 11, 1, 0, 0, 0, TimeSpan.Zero));

    public InstallationTimeZone DefaultZone { get; } = InstallationTimeZone.From(null);

    /// <summary>A registered user with no stored time zone, so the goals are in UTC.</summary>
    public ActingUser User { get; }

    /// <summary>Another registered user, in UTC.</summary>
    public ActingUser OtherUser { get; }

    public ActingUser Administrator { get; }

    /// <summary>The Published catalog habit "Reading" the goals are based on.</summary>
    public CatalogHabit Habit { get; }

    public RecordingService Service => new(Records, Goals, Accounts, Clock);

    public GoalService GoalService => new(Goals, Accounts, CatalogHabits, CustomHabits, Clock, DefaultZone);

    /// <summary>A registered user stored with <paramref name="timeZone"/>.</summary>
    public ActingUser UserIn(string timeZone) => GoalInputs.RegisteredUser(Accounts, timeZone);

    /// <summary>Sets the clock to <paramref name="instant"/> (UTC).</summary>
    public void At(DateTimeOffset instant) => Clock.SetUtcNow(instant);

    /// <summary>
    /// Creates a goal of <paramref name="user"/> (default <see cref="User"/>) with <paramref name="target"/>, starting on
    /// <paramref name="start"/>, through <see cref="GoalService"/> at <paramref name="at"/>, and leaves the clock there.
    /// </summary>
    public async Task<GoalId> CreateGoalAsync(GoalTargetFields target, DateOnly start, DateTimeOffset at, ActingUser? user = null)
    {
        At(at);
        var result = await GoalService.CreateAsync(
            user ?? User, GoalInputs.Input(GoalInputs.Catalog(Habit), start, target), CancellationToken.None);
        Assert.Equal(GoalCreateOutcome.Created, result.Outcome);
        return result.GoalId!.Value;
    }

    /// <summary>
    /// Stores a goal of <paramref name="owner"/> directly, with one settings version per entry of
    /// <paramref name="versions"/>, as later time-zone changes would leave it; the start date is the first version's.
    /// </summary>
    public GoalId AddGoal(ActingUser owner, GoalTarget target, params (DateOnly From, NamedTimeZone Zone)[] versions)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentNullException.ThrowIfNull(versions);
        var account = owner.AccountId!.Value;
        var start = versions[0].From;
        var goal = new Goal(new GoalId(Guid.NewGuid()), account, GoalInputs.Catalog(Habit), start, null, new GoalAttempt(1, start),
            versions.Select(v => new GoalSettingsVersion(new GoalSettings(target, v.Zone), v.From)).ToList());
        Goals.Add(account, goal, new GoalHabitView(Habit.Id, HabitKind.Catalog, Habit.Definition.Name.Value, Habit.Definition.Category, true));
        return goal.Id;
    }

    /// <summary>Records for <see cref="User"/> at the clock's time.</summary>
    public Task<RecordWriteResult> RecordAsync(GoalId goal, DateOnly date, RecordInput input, ActingUser? user = null) =>
        Service.RecordAsync(user ?? User, goal, date, input, CancellationToken.None);

    /// <summary>What <see cref="User"/> can record now, at the clock's time.</summary>
    public Task<RecordableResult> RecordableAsync(GoalId goal, ActingUser? user = null) =>
        Service.GetRecordableAsync(user ?? User, goal, CancellationToken.None);

    public static RecordInput Minutes(int minutes, string? note = null) => new() { Amount = minutes, Note = note };

    public static RecordInput Amount(int amount, string? note = null) => new() { Amount = amount, Note = note };

    public static RecordInput Yes(string? note = null) => new() { Answer = YesNoAnswer.Yes, Note = note };

    public static RecordInput No(string? note = null) => new() { Answer = YesNoAnswer.No, Note = note };
}
