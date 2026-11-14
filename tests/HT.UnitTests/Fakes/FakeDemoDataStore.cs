using HT.Application.Operator;
using HT.Domain.Accounts;
using HT.Domain.Habits;

namespace HT.UnitTests.Fakes;

/// <summary>
/// A hand-written in-memory <see cref="IDemoDataStore"/> (ADR 0006, research R-13). It creates each demo account in
/// the test's <see cref="FakeAccountStore"/>, so sign-in over that fake works and its counters and
/// <see cref="FakeAccountStore.TakeEmailOnNextCreate"/> (the lost race) still apply. It records the custom habits per
/// created account, the seed catalog once while it has no habit, and the order of the calls. Safe to call from
/// parallel tasks.
/// </summary>
public sealed class FakeDemoDataStore(FakeAccountStore accounts) : IDemoDataStore
{
    private readonly object _gate = new();
    private readonly Dictionary<AccountId, IReadOnlyList<HabitDefinition>> _customHabits = [];
    private readonly List<string> _calls = [];
    private readonly List<SeedCatalogHabit> _catalog = [];

    /// <summary>The calls in order: <c>account {email}</c> for each account creation, <c>catalog</c> for each seed attempt.</summary>
    public IReadOnlyList<string> Calls
    {
        get
        {
            lock (_gate)
            {
                return [.. _calls];
            }
        }
    }

    /// <summary>The catalog habits: those the seed created, plus any a test added.</summary>
    public IReadOnlyList<SeedCatalogHabit> Catalog
    {
        get
        {
            lock (_gate)
            {
                return [.. _catalog];
            }
        }
    }

    /// <summary>How many times the seed catalog was created.</summary>
    public int SeedCount { get; private set; }

    /// <summary>Adds a catalog habit directly, as an administrator would, for arranging a non-empty catalog.</summary>
    public void AddCatalogHabit(SeedCatalogHabit habit)
    {
        lock (_gate)
        {
            _catalog.Add(habit);
        }
    }

    /// <summary>The custom habits recorded for this account; empty when it has none.</summary>
    public IReadOnlyList<HabitDefinition> CustomHabitsOf(AccountId id)
    {
        lock (_gate)
        {
            return _customHabits.TryGetValue(id, out var habits) ? habits : [];
        }
    }

    /// <summary>Every account that got custom habits, with them.</summary>
    public IReadOnlyDictionary<AccountId, IReadOnlyList<HabitDefinition>> AllCustomHabits
    {
        get
        {
            lock (_gate)
            {
                return new Dictionary<AccountId, IReadOnlyList<HabitDefinition>>(_customHabits);
            }
        }
    }

    public async Task<CreateDemoAccountOutcome> CreateDemoAccountAsync(
        Email email, string password, IReadOnlyList<HabitDefinition> customHabits, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(customHabits);
        lock (_gate)
        {
            _calls.Add($"account {email.Value}");
        }

        var created = await accounts.CreateAsync(email, Role.RegisteredUser, password, cancellationToken);
        if (created.IsEmailTaken)
        {
            return CreateDemoAccountOutcome.EmailTaken;
        }

        lock (_gate)
        {
            _customHabits.Add(created.Account!.Id, [.. customHabits]);
        }

        return CreateDemoAccountOutcome.Created;
    }

    public Task<bool> TryCreateSeedCatalogAsync(IReadOnlyList<SeedCatalogHabit> habits, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(habits);
        lock (_gate)
        {
            _calls.Add("catalog");
            if (_catalog.Count > 0)
            {
                return Task.FromResult(false);
            }

            _catalog.AddRange(habits);
            SeedCount++;
            return Task.FromResult(true);
        }
    }
}
