using HT.Application.Habits;
using HT.Domain.Accounts;
using HT.Domain.Habits;

namespace HT.UnitTests.Fakes;

/// <summary>
/// A hand-written in-memory <see cref="ICustomHabitStore"/> (ADR 0006), owner-scoped like the real one: every call
/// sees only the given owner's habits. Names are unique per owner on <see cref="HabitName.Key"/>. Shared with
/// HT.UiTests by a link. Safe to call from parallel tasks.
/// </summary>
public sealed class FakeCustomHabitStore : ICustomHabitStore
{
    private readonly object _gate = new();
    private readonly List<CustomHabit> _habits = [];
    private bool _refuseNameOnNextWrite;

    /// <summary>Writes that changed the store: creates, updates and deletes.</summary>
    public int WriteCount { get; private set; }

    /// <summary>Every stored habit of every owner, for assertions.</summary>
    public IReadOnlyList<CustomHabit> Habits
    {
        get
        {
            lock (_gate)
            {
                return [.. _habits];
            }
        }
    }

    /// <summary>Adds a habit directly, for arranging a test.</summary>
    public CustomHabit Add(AccountId owner, HabitDefinition definition)
    {
        lock (_gate)
        {
            var habit = new CustomHabit(new HabitId(Guid.NewGuid()), owner, definition);
            _habits.Add(habit);
            return habit;
        }
    }

    /// <summary>The stored habit with this identifier, whoever owns it; for assertions only.</summary>
    public CustomHabit? Get(HabitId id)
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

    public Task<IReadOnlyList<CustomHabit>> ListAsync(AccountId owner, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            return Task.FromResult<IReadOnlyList<CustomHabit>>(_habits.Where(h => h.Owner == owner).ToList());
        }
    }

    public Task<CustomHabit?> FindAsync(AccountId owner, HabitId id, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            return Task.FromResult(_habits.FirstOrDefault(h => h.Owner == owner && h.Id == id));
        }
    }

    public Task<CreateCustomHabitOutcome> CreateAsync(AccountId owner, HabitDefinition definition, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            if (TakeRefusal() || KeyTaken(owner, definition.Name.Key, null))
            {
                return Task.FromResult(CreateCustomHabitOutcome.NameTaken);
            }

            var habit = new CustomHabit(new HabitId(Guid.NewGuid()), owner, definition);
            _habits.Add(habit);
            WriteCount++;
            return Task.FromResult(CreateCustomHabitOutcome.Created(habit.Id));
        }
    }

    public Task<UpdateCustomHabitOutcome> UpdateAsync(AccountId owner, HabitId id, HabitDefinition definition, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            var index = _habits.FindIndex(h => h.Owner == owner && h.Id == id);
            if (index < 0)
            {
                return Task.FromResult(UpdateCustomHabitOutcome.NotFound);
            }

            if (TakeRefusal() || KeyTaken(owner, definition.Name.Key, id))
            {
                return Task.FromResult(UpdateCustomHabitOutcome.NameTaken);
            }

            _habits[index] = new CustomHabit(id, owner, definition);
            WriteCount++;
            return Task.FromResult(UpdateCustomHabitOutcome.Saved);
        }
    }

    public Task<DeleteCustomHabitOutcome> DeleteAsync(AccountId owner, HabitId id, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            var removed = _habits.RemoveAll(h => h.Owner == owner && h.Id == id);
            if (removed == 0)
            {
                return Task.FromResult(DeleteCustomHabitOutcome.NotFound);
            }

            WriteCount++;
            return Task.FromResult(DeleteCustomHabitOutcome.Deleted);
        }
    }

    public Task<bool> NameKeyExistsAsync(AccountId owner, string key, HabitId? except, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            return Task.FromResult(KeyTaken(owner, key, except));
        }
    }

    private bool KeyTaken(AccountId owner, string key, HabitId? except) =>
        _habits.Any(h => h.Owner == owner && h.Id != except && string.Equals(h.Definition.Name.Key, key, StringComparison.Ordinal));

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
