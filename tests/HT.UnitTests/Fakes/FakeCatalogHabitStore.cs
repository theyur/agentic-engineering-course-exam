using HT.Application.Habits;
using HT.Domain.Habits;

namespace HT.UnitTests.Fakes;

/// <summary>
/// A hand-written in-memory <see cref="ICatalogHabitStore"/> (ADR 0006), shared with HT.UiTests by a link. Names
/// are unique on <see cref="HabitName.Key"/> over every state, the edited habit excluded. Hooks let a test
/// simulate lost races; <see cref="WriteCount"/> lets it assert that nothing changed. Safe to call from parallel
/// tasks.
/// </summary>
public sealed class FakeCatalogHabitStore : ICatalogHabitStore
{
    private readonly object _gate = new();
    private readonly List<CatalogHabit> _habits = [];
    private bool _refuseNameOnNextWrite;
    private int _swapsToFail;
    private CatalogHabitState? _stateOnNextFailedSwap;

    /// <summary>Writes that changed the store: creates, updates and state swaps.</summary>
    public int WriteCount { get; private set; }

    /// <summary>Calls of <see cref="TrySetStateAsync"/>, failed ones included.</summary>
    public int SwapCount { get; private set; }

    public IReadOnlyList<CatalogHabit> Habits
    {
        get
        {
            lock (_gate)
            {
                return [.. _habits];
            }
        }
    }

    /// <summary>Adds a habit in any state directly, for arranging a test.</summary>
    public CatalogHabit Add(CatalogHabit habit)
    {
        ArgumentNullException.ThrowIfNull(habit);
        lock (_gate)
        {
            _habits.Add(habit);
            return habit;
        }
    }

    /// <summary>The stored habit with this identifier, or null.</summary>
    public CatalogHabit? Get(HabitId id)
    {
        lock (_gate)
        {
            return _habits.FirstOrDefault(h => h.Id == id);
        }
    }

    /// <summary>Makes the next create or update give NameTaken after the service's own check passed (HP-018).</summary>
    public void RefuseNameOnNextWrite()
    {
        lock (_gate)
        {
            _refuseNameOnNextWrite = true;
        }
    }

    /// <summary>Makes the next <paramref name="count"/> state swaps return false and leave the stored state unchanged.</summary>
    public void FailNextStateSwaps(int count)
    {
        lock (_gate)
        {
            _swapsToFail = count;
        }
    }

    /// <summary>
    /// Makes the next failed swap also set the stored state to <paramref name="state"/>, as another administrator's
    /// write between the swap and the re-read would.
    /// </summary>
    public void ChangeStateOnNextFailedSwap(CatalogHabitState state)
    {
        lock (_gate)
        {
            _stateOnNextFailedSwap = state;
        }
    }

    public Task<IReadOnlyList<CatalogHabit>> ListPublishedAsync(CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            return Task.FromResult<IReadOnlyList<CatalogHabit>>(_habits.Where(h => h.State == CatalogHabitState.Published).ToList());
        }
    }

    public Task<IReadOnlyList<CatalogHabit>> ListAllAsync(CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            return Task.FromResult<IReadOnlyList<CatalogHabit>>([.. _habits]);
        }
    }

    public Task<CatalogHabit?> FindAsync(HabitId id, CancellationToken cancellationToken) => Task.FromResult(Get(id));

    public Task<CatalogHabit?> FindPublishedAsync(HabitId id, CancellationToken cancellationToken) =>
        Task.FromResult(Get(id) is { State: CatalogHabitState.Published } habit ? habit : null);

    public Task<CreateCatalogHabitOutcome> CreateAsync(
        HabitDefinition definition, SuggestedStartingGoals suggestedStartingGoals, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            if (TakeRefusal() || KeyTaken(definition.Name.Key, null))
            {
                return Task.FromResult(CreateCatalogHabitOutcome.NameTaken);
            }

            var habit = new CatalogHabit(new HabitId(Guid.NewGuid()), definition, suggestedStartingGoals, CatalogHabitState.Draft);
            _habits.Add(habit);
            WriteCount++;
            return Task.FromResult(CreateCatalogHabitOutcome.Created(habit.Id));
        }
    }

    public Task<UpdateCatalogHabitOutcome> UpdateAsync(
        HabitId id, HabitDefinition definition, SuggestedStartingGoals suggestedStartingGoals, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            var index = _habits.FindIndex(h => h.Id == id);
            if (index < 0)
            {
                return Task.FromResult(UpdateCatalogHabitOutcome.NotFound);
            }

            if (TakeRefusal() || KeyTaken(definition.Name.Key, id))
            {
                return Task.FromResult(UpdateCatalogHabitOutcome.NameTaken);
            }

            _habits[index] = new CatalogHabit(id, definition, suggestedStartingGoals, _habits[index].State);
            WriteCount++;
            return Task.FromResult(UpdateCatalogHabitOutcome.Saved);
        }
    }

    public Task<bool> TrySetStateAsync(HabitId id, CatalogHabitState expected, CatalogHabitState next, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            SwapCount++;
            var index = _habits.FindIndex(h => h.Id == id);
            if (index < 0)
            {
                return Task.FromResult(false);
            }

            var habit = _habits[index];
            if (_swapsToFail > 0)
            {
                _swapsToFail--;
                if (_stateOnNextFailedSwap is { } other)
                {
                    _stateOnNextFailedSwap = null;
                    _habits[index] = new CatalogHabit(habit.Id, habit.Definition, habit.SuggestedStartingGoals, other);
                }

                return Task.FromResult(false);
            }

            if (habit.State != expected)
            {
                return Task.FromResult(false);
            }

            _habits[index] = new CatalogHabit(habit.Id, habit.Definition, habit.SuggestedStartingGoals, next);
            WriteCount++;
            return Task.FromResult(true);
        }
    }

    public Task<bool> NameKeyExistsAsync(string key, HabitId? except, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            return Task.FromResult(KeyTaken(key, except));
        }
    }

    private bool KeyTaken(string key, HabitId? except) =>
        _habits.Any(h => h.Id != except && string.Equals(h.Definition.Name.Key, key, StringComparison.Ordinal));

    private bool TakeRefusal()
    {
        if (!_refuseNameOnNextWrite)
        {
            return false;
        }

        _refuseNameOnNextWrite = false;
        return true;
    }
}
