using HT.Application.Accounts;
using HT.Application.Operator;
using HT.Domain.Accounts;

namespace HT.UnitTests.Fakes;

/// <summary>
/// A hand-written in-memory <see cref="IDemoPauseStore"/> (ADR 0006, slice 7 research R-11). It creates the demo account
/// in the test's <see cref="FakeAccountStore"/> with the profile it is given, so sign-in over that fake works and its
/// counters and <see cref="FakeAccountStore.TakeEmailOnNextCreate"/> (the lost race) still apply. It records the
/// profile and the goals it keeps per created account, and the order of the calls. A goal whose catalog habit name is
/// in <see cref="LeftOutHabits"/> is left out with its records, notes, excusals and pauses, as the real store leaves out
/// a goal whose habit is not Published. Safe to call from parallel tasks.
/// </summary>
public sealed class FakeDemoPauseStore(FakeAccountStore accounts) : IDemoPauseStore
{
    private readonly object _gate = new();
    private readonly Dictionary<AccountId, IReadOnlyList<DemoPauseGoal>> _goals = [];
    private readonly Dictionary<AccountId, DemoAccountProfile> _profiles = [];
    private readonly List<string> _calls = [];

    /// <summary>The catalog habit names, compared ignoring case, whose goals the fake leaves out.</summary>
    public ISet<string> LeftOutHabits { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>The calls in order: <c>account {email}</c> for each account creation.</summary>
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

    /// <summary>Every account the fake created, with the goals it kept.</summary>
    public IReadOnlyDictionary<AccountId, IReadOnlyList<DemoPauseGoal>> AllGoals
    {
        get
        {
            lock (_gate)
            {
                return new Dictionary<AccountId, IReadOnlyList<DemoPauseGoal>>(_goals);
            }
        }
    }

    /// <summary>The goals kept for this account; empty when the fake did not create it.</summary>
    public IReadOnlyList<DemoPauseGoal> GoalsOf(AccountId id)
    {
        lock (_gate)
        {
            return _goals.TryGetValue(id, out var goals) ? goals : [];
        }
    }

    /// <summary>The profile this account was created with, or null when the fake did not create it.</summary>
    public DemoAccountProfile? ProfileOf(AccountId id)
    {
        lock (_gate)
        {
            return _profiles.GetValueOrDefault(id);
        }
    }

    public async Task<CreateDemoAccountOutcome> CreateDemoPauseAccountAsync(
        Email email,
        string password,
        DemoAccountProfile profile,
        IReadOnlyList<DemoPauseGoal> goals,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(password);
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(goals);
        lock (_gate)
        {
            _calls.Add($"account {email.Value}");
        }

        if (!AccountName.TryCreate(profile.Name, out var name, out _))
        {
            throw new ArgumentException("The demo profile's name fails the account-name checks.", nameof(profile));
        }

        var created = await accounts.CreateAsync(
            email, Role.RegisteredUser, password, new NewAccountProfile(name, profile.TimeZone, profile.CreatedAt), cancellationToken);
        if (created.IsEmailTaken)
        {
            return CreateDemoAccountOutcome.EmailTaken;
        }

        lock (_gate)
        {
            var id = created.Account!.Id;
            _goals.Add(id, [.. goals.Where(g => !LeftOutHabits.Contains(g.History.Goal.CatalogHabitName!))]);
            _profiles.Add(id, profile);
        }

        return CreateDemoAccountOutcome.Created;
    }
}
