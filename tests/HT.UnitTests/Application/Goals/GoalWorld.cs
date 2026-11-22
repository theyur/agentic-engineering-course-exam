using HT.Application.Accounts;
using HT.Application.Goals;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Time;
using HT.UnitTests.Fakes;
using Microsoft.Extensions.Time.Testing;

namespace HT.UnitTests.Application.Goals;

/// <summary>
/// The fakes a <see cref="GoalService"/> unit test needs, wired as the host wires the real stores: an account store,
/// both habit stores, a goal store that reads them, a fake clock and the installation's default time zone (UTC unless
/// given).
/// </summary>
public sealed class GoalWorld
{
    /// <summary>12:00 in Kyiv on Wednesday 18 November 2026.</summary>
    public static readonly DateTimeOffset Noon = new(2026, 11, 18, 10, 0, 0, TimeSpan.Zero);

    /// <summary>Wednesday 18 November 2026, today in Kyiv and in UTC at <see cref="Noon"/>.</summary>
    public static readonly DateOnly Today = new(2026, 11, 18);

    public GoalWorld(string? defaultZone = null, DateTimeOffset? now = null)
    {
        Goals = new FakeGoalStore(Accounts, CatalogHabits, CustomHabits);
        DefaultZone = InstallationTimeZone.From(defaultZone);
        Clock = new FakeTimeProvider(now ?? Noon);
    }

    public FakeAccountStore Accounts { get; } = new();

    public FakeCatalogHabitStore CatalogHabits { get; } = new();

    public FakeCustomHabitStore CustomHabits { get; } = new();

    public FakeGoalStore Goals { get; }

    public FakeTimeProvider Clock { get; }

    public InstallationTimeZone DefaultZone { get; }

    public GoalService Service => new(Goals, Accounts, CatalogHabits, CustomHabits, Clock, DefaultZone);

    /// <summary>
    /// Stores a goal on <paramref name="habit"/> directly, as creation on its first date and later time-zone changes
    /// would leave it: the start date and attempt 1 begin on the first version's date, and each version has its own time
    /// zone. For goals that started before the clock's time, which <see cref="Service"/> cannot create.
    /// </summary>
    public Goal AddGoal(AccountId owner, CatalogHabit habit, GoalTarget target, params (DateOnly From, NamedTimeZone Zone)[] versions)
    {
        ArgumentNullException.ThrowIfNull(habit);
        ArgumentNullException.ThrowIfNull(versions);
        var start = versions[0].From;
        var goal = new Goal(new GoalId(Guid.NewGuid()), owner, new GoalHabit(habit.Id, HabitKind.Catalog), start, null, new GoalAttempt(1, start),
            versions.Select(v => new GoalSettingsVersion(new GoalSettings(target, v.Zone), v.From)).ToList());
        return Goals.Add(owner, goal, new GoalHabitView(habit.Id, HabitKind.Catalog, habit.Definition.Name.Value, habit.Definition.Category, true));
    }

    /// <summary>The profile use cases over the same fakes, clock and default time zone (US10).</summary>
    public ProfileService Profiles => new(Accounts, Goals, Clock, DefaultZone);

    /// <summary>The service over the goal store, with <paramref name="beforeFirstCreate"/> run once just before the first creation reaches it.</summary>
    public GoalService ServiceChangingBeforeFirstCreate(Func<Task> beforeFirstCreate) =>
        new(new BeforeFirstCreate(Goals, beforeFirstCreate, null), Accounts, CatalogHabits, CustomHabits, Clock, DefaultZone);

    /// <summary>
    /// The profile use cases over the goal store, with <paramref name="beforeFirstTimeZoneWrite"/> run once just before the
    /// first time-zone write reaches it, after the use case has read the account's goals.
    /// </summary>
    public ProfileService ProfilesChangingBeforeFirstTimeZoneWrite(Func<Task> beforeFirstTimeZoneWrite) =>
        new(Accounts, new BeforeFirstCreate(Goals, null, beforeFirstTimeZoneWrite), Clock, DefaultZone);

    /// <summary>
    /// Runs an action once before the first <see cref="IGoalStore.CreateAsync"/>, and another once before the first
    /// <see cref="IGoalStore.TryChangeTimeZoneAsync"/>, as another session's commit between the use case's reads and the
    /// store's transaction would. Every other member is forwarded as it is.
    /// </summary>
    private sealed class BeforeFirstCreate(IGoalStore inner, Func<Task>? beforeCreate, Func<Task>? beforeTimeZoneWrite) : IGoalStore
    {
        private Func<Task>? _beforeCreate = beforeCreate;
        private Func<Task>? _beforeTimeZoneWrite = beforeTimeZoneWrite;

        public async Task<GoalWriteOutcome> CreateAsync(
            AccountId owner, NewGoal goal, NamedTimeZone expectedZone,
            NamedTimeZone defaultZone, CancellationToken cancellationToken)
        {
            if (Interlocked.Exchange(ref _beforeCreate, null) is { } pending)
            {
                await pending();
            }

            return await inner.CreateAsync(owner, goal, expectedZone, defaultZone, cancellationToken);
        }

        public Task<OwnerGoals?> ReadOwnerGoalsAsync(AccountId owner, CancellationToken cancellationToken) =>
            inner.ReadOwnerGoalsAsync(owner, cancellationToken);

        public async Task<bool> TryChangeTimeZoneAsync(
            AccountId owner, string? expectedStoredZone, int expectedRevision, NamedTimeZone next,
            IReadOnlyList<GoalVersionWrite> writes, CancellationToken cancellationToken)
        {
            if (Interlocked.Exchange(ref _beforeTimeZoneWrite, null) is { } pending)
            {
                await pending();
            }

            return await inner.TryChangeTimeZoneAsync(owner, expectedStoredZone, expectedRevision, next, writes, cancellationToken);
        }

        public Task<OwnerGoal?> ReadOwnerGoalAsync(AccountId owner, GoalId goal, CancellationToken cancellationToken) =>
            inner.ReadOwnerGoalAsync(owner, goal, cancellationToken);

        public Task<GoalChangeWriteOutcome> TryChangeSettingsAsync(
            AccountId owner, string? expectedStoredZone, int expectedRevision, GoalSettingsWrite write, CancellationToken cancellationToken) =>
            inner.TryChangeSettingsAsync(owner, expectedStoredZone, expectedRevision, write, cancellationToken);

        public Task<GoalChangeWriteOutcome> TryEndAsync(
            AccountId owner, string? expectedStoredZone, int expectedRevision, GoalEndingWrite write, CancellationToken cancellationToken) =>
            inner.TryEndAsync(owner, expectedStoredZone, expectedRevision, write, cancellationToken);

        public Task<GoalChangeWriteOutcome> TryStartAttemptAsync(
            AccountId owner, string? expectedStoredZone, int expectedRevision, GoalAttemptWrite write, CancellationToken cancellationToken) =>
            inner.TryStartAttemptAsync(owner, expectedStoredZone, expectedRevision, write, cancellationToken);

        public Task<IReadOnlyList<StoredGoal>> ListAsync(AccountId owner, CancellationToken cancellationToken) =>
            inner.ListAsync(owner, cancellationToken);

        public Task<StoredGoal?> FindAsync(AccountId owner, GoalId id, CancellationToken cancellationToken) =>
            inner.FindAsync(owner, id, cancellationToken);

        public Task<int> CountUsingCustomHabitAsync(AccountId owner, HabitId habit, CancellationToken cancellationToken) =>
            inner.CountUsingCustomHabitAsync(owner, habit, cancellationToken);

        public Task<IReadOnlyDictionary<HabitId, int>> CountByCatalogHabitAsync(AccountId owner, CancellationToken cancellationToken) =>
            inner.CountByCatalogHabitAsync(owner, cancellationToken);

        public Task<IReadOnlyList<StoredGoal>> ListOnCatalogHabitAsync(AccountId owner, HabitId habit, CancellationToken cancellationToken) =>
            inner.ListOnCatalogHabitAsync(owner, habit, cancellationToken);

        public Task<int> CountUsingCatalogHabitAsync(HabitId habit, CancellationToken cancellationToken) =>
            inner.CountUsingCatalogHabitAsync(habit, cancellationToken);
    }
}
