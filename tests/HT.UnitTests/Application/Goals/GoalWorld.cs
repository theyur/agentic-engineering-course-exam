using HT.Application.Accounts;
using HT.Application.Goals;
using HT.Domain.Accounts;
using HT.Domain.Goals;
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

    /// <summary>The service over the goal store, with <paramref name="beforeFirstCreate"/> run once just before the first creation reaches it.</summary>
    public GoalService ServiceChangingBeforeFirstCreate(Func<Task> beforeFirstCreate) =>
        new(new BeforeFirstCreate(Goals, beforeFirstCreate), Accounts, CatalogHabits, CustomHabits, Clock, DefaultZone);

    /// <summary>
    /// Runs an action once before the first <see cref="IGoalStore.CreateAsync"/>, as another session's commit between
    /// the use case's reads and the store's transaction would.
    /// </summary>
    private sealed class BeforeFirstCreate(IGoalStore inner, Func<Task> action) : IGoalStore
    {
        private Func<Task>? _action = action;

        public async Task<GoalWriteOutcome> CreateAsync(
            AccountId owner, NewGoal goal, NamedTimeZone expectedZone,
            NamedTimeZone defaultZone, CancellationToken cancellationToken)
        {
            if (Interlocked.Exchange(ref _action, null) is { } pending)
            {
                await pending();
            }

            return await inner.CreateAsync(owner, goal, expectedZone, defaultZone, cancellationToken);
        }

        public Task<IReadOnlyList<StoredGoal>> ListAsync(AccountId owner, CancellationToken cancellationToken) =>
            inner.ListAsync(owner, cancellationToken);

        public Task<StoredGoal?> FindAsync(AccountId owner, GoalId id, CancellationToken cancellationToken) =>
            inner.FindAsync(owner, id, cancellationToken);
    }
}
