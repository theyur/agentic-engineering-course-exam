using HT.Application.Goals;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Time;

namespace HT.UnitTests.Fakes;

/// <summary>
/// A hand-written in-memory <see cref="IGoalStore"/> (ADR 0006), owner-scoped like the real one, with the outcomes of
/// research R-08. It reads the account's time zone from a <see cref="FakeAccountStore"/> and the habits from a
/// <see cref="FakeCatalogHabitStore"/> and a <see cref="FakeCustomHabitStore"/>, so a read gives each habit's current
/// name, category and state. Shared with HT.UiTests by a link. Safe to call from parallel tasks; its lock is taken
/// before the other fakes' locks, so they never call into it while holding their own.
/// <para>
/// Slice 8 (research R-05): the conditional goal-change writes re-check the zone, the revision and their precondition
/// as the real store does, and <see cref="TryEndAsync"/> applies its pause and excusal cuts to the
/// <see cref="FakeRecordStore"/> attached as <see cref="Records"/> (a record store over this goal store attaches itself),
/// under this store's lock and then the record store's, the usual lock order. Goals with any attempts and endings are
/// arranged through <see cref="Add"/>; <see cref="BeforeNextWrite"/> and <see cref="IncrementRevision"/> simulate a
/// concurrent write or a stale revision.
/// </para>
/// </summary>
public sealed class FakeGoalStore : IGoalStore
{
    private readonly object _gate = new();
    private readonly FakeAccountStore _accounts;
    private readonly FakeCatalogHabitStore _catalogHabits;
    private readonly FakeCustomHabitStore _customHabits;
    private readonly List<StoredGoal> _goals = [];
    private readonly Dictionary<AccountId, int> _revisions = [];
    private int _timeZoneChangesToReport;
    private int _timeZoneConflictsToReport;
    private int _lookups;
    private int _ownerGoalReads;
    private int _changeWriteCount;
    private Action? _beforeNextWrite;
    private FakeRecordStore? _records;

    /// <summary>Takes the fakes it reads, and sets <see cref="FakeCustomHabitStore.Goals"/> to itself.</summary>
    public FakeGoalStore(FakeAccountStore accounts, FakeCatalogHabitStore catalogHabits, FakeCustomHabitStore customHabits)
    {
        ArgumentNullException.ThrowIfNull(accounts);
        ArgumentNullException.ThrowIfNull(catalogHabits);
        ArgumentNullException.ThrowIfNull(customHabits);
        _accounts = accounts;
        _catalogHabits = catalogHabits;
        _customHabits = customHabits;
        customHabits.Goals = this;
    }

    /// <summary>
    /// A goal store with no goals over <paramref name="catalogHabits"/> and new, empty account and custom-habit fakes:
    /// for catalog tests that need a <c>CatalogService</c> and no goal.
    /// </summary>
    public static FakeGoalStore Over(FakeCatalogHabitStore catalogHabits) =>
        new(new FakeAccountStore(), catalogHabits, new FakeCustomHabitStore());

    /// <summary>Calls of <see cref="CreateAsync"/>, refused ones included.</summary>
    public int CreateCount { get; private set; }

    /// <summary>Every stored goal of every owner, in creation order; for assertions only.</summary>
    public IReadOnlyList<Goal> Goals
    {
        get
        {
            lock (_gate)
            {
                return _goals.Select(g => g.Goal).ToList();
            }
        }
    }

    /// <summary>Calls of <see cref="TryChangeTimeZoneAsync"/>, refused ones included.</summary>
    public int TimeZoneWriteCount { get; private set; }

    /// <summary>Calls of <see cref="TryChangeTimeZoneAsync"/> that wrote the change.</summary>
    public int TimeZoneChangeCount { get; private set; }

    /// <summary>
    /// Calls of <see cref="FindAsync"/>, <see cref="ListAsync"/> and (slice 8) <see cref="ReadOwnerGoalAsync"/>, so a test
    /// can prove that a refusal looked up no goal (FR-015's check order).
    /// </summary>
    public int Lookups
    {
        get
        {
            lock (_gate)
            {
                return _lookups;
            }
        }
    }

    /// <summary>Slice 8: calls of <see cref="ReadOwnerGoalAsync"/>.</summary>
    public int OwnerGoalReads
    {
        get
        {
            lock (_gate)
            {
                return _ownerGoalReads;
            }
        }
    }

    /// <summary>
    /// Slice 8: calls of the three conditional goal-change writes (<see cref="TryChangeSettingsAsync"/>,
    /// <see cref="TryEndAsync"/> and <see cref="TryStartAttemptAsync"/>), refused ones included.
    /// </summary>
    public int ChangeWriteCount
    {
        get
        {
            lock (_gate)
            {
                return _changeWriteCount;
            }
        }
    }

    /// <summary>
    /// Slice 8: an action run once, at the start of the next conditional goal-change write, before its re-checks and
    /// outside this store's lock, and then cleared. A unit test sets it to simulate a concurrent write between a use
    /// case's read and its write, for example a call of one of the writes or <see cref="IncrementRevision"/>.
    /// </summary>
    public Action? BeforeNextWrite
    {
        get => Volatile.Read(ref _beforeNextWrite);
        set => Volatile.Write(ref _beforeNextWrite, value);
    }

    /// <summary>
    /// Slice 8: the record store whose pauses and excusals <see cref="TryEndAsync"/> cuts; a <see cref="FakeRecordStore"/>
    /// over this store sets it to itself. Null when none is attached, and then an ending cuts nothing.
    /// </summary>
    public FakeRecordStore? Records
    {
        get => Volatile.Read(ref _records);
        set => Volatile.Write(ref _records, value);
    }

    /// <summary>Slice 8: increments the owner's goal revision, as a write committed in another session would.</summary>
    public void IncrementRevision(AccountId owner)
    {
        lock (_gate)
        {
            _revisions[owner] = _revisions.GetValueOrDefault(owner) + 1;
        }
    }

    /// <summary>
    /// The owner's goal revision: incremented by every created goal and every time-zone change (research R-08); 0 before
    /// the first.
    /// </summary>
    public int RevisionOf(AccountId owner)
    {
        lock (_gate)
        {
            return _revisions.GetValueOrDefault(owner);
        }
    }

    /// <summary>
    /// Makes the next <paramref name="count"/> time-zone writes lose: each increments the owner's revision, as a goal
    /// created in another session would, writes nothing else and returns false.
    /// </summary>
    public void ConflictOnNextTimeZoneWrites(int count)
    {
        lock (_gate)
        {
            _timeZoneConflictsToReport = count;
        }
    }

    /// <summary>
    /// Makes the next <paramref name="count"/> creates return <see cref="GoalWriteOutcome.TimeZoneChanged"/> and write
    /// nothing, as time-zone changes committed in another session would.
    /// </summary>
    public void ReportTimeZoneChangedOnNextCreates(int count)
    {
        lock (_gate)
        {
            _timeZoneChangesToReport = count;
        }
    }

    /// <summary>
    /// Stores a goal directly, for arranging a test that needs goals to exist without <c>GoalService</c> and without an
    /// account: neither the account's time zone nor the revision changes. <paramref name="habit"/> is the view that a
    /// read gives when the habit stores no longer have the habit. Slice 8: the goal may be built through either
    /// constructor, with any attempts and endings, also past ones that the use cases would refuse.
    /// </summary>
    public Goal Add(AccountId owner, Goal goal, GoalHabitView habit)
    {
        ArgumentNullException.ThrowIfNull(goal);
        ArgumentNullException.ThrowIfNull(habit);
        if (goal.Owner != owner)
        {
            throw new ArgumentException("The goal belongs to another owner.", nameof(goal));
        }

        if (goal.Habit != new GoalHabit(habit.Id, habit.Kind))
        {
            throw new ArgumentException("The view is of another habit.", nameof(habit));
        }

        lock (_gate)
        {
            _goals.Add(new StoredGoal(goal, habit));
            return goal;
        }
    }

    /// <summary>
    /// Research R-08 steps 2 to 6. A missing account gives <see cref="GoalWriteOutcome.TimeZoneChanged"/>, so the use
    /// case reads the account again and finds it gone.
    /// </summary>
    public Task<GoalWriteOutcome> CreateAsync(
        AccountId owner, NewGoal goal, NamedTimeZone expectedZone, NamedTimeZone defaultZone, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(goal);
        lock (_gate)
        {
            CreateCount++;
            if (_timeZoneChangesToReport > 0)
            {
                _timeZoneChangesToReport--;
                return Task.FromResult(GoalWriteOutcome.TimeZoneChanged);
            }

            if (_accounts.FindById(owner) is not { } account)
            {
                return Task.FromResult(GoalWriteOutcome.TimeZoneChanged);
            }

            var storedZone = account.StoredTimeZone ?? defaultZone.Id;
            if (!string.Equals(storedZone, expectedZone.Id, StringComparison.Ordinal))
            {
                return Task.FromResult(GoalWriteOutcome.TimeZoneChanged);
            }

            if (HabitViewOf(owner, goal.Habit) is not { } habit)
            {
                return Task.FromResult(GoalWriteOutcome.HabitGone);
            }

            if (account.StoredTimeZone is null)
            {
                _accounts.SetStoredTimeZone(owner, expectedZone.Id);
            }

            _revisions[owner] = _revisions.GetValueOrDefault(owner) + 1;
            var created = new Goal(
                new GoalId(Guid.NewGuid()),
                owner,
                goal.Habit,
                goal.StartDate,
                goal.OwnName,
                new GoalAttempt(GoalAttempt.First, goal.StartDate),
                [new GoalSettingsVersion(new GoalSettings(goal.Target, expectedZone), goal.StartDate)]);
            _goals.Add(new StoredGoal(created, habit));
            return Task.FromResult(GoalWriteOutcome.Created(created.Id));
        }
    }

    /// <summary>The account's stored zone and revision, then its goals, read together under this store's lock; null without an account.</summary>
    public Task<OwnerGoals?> ReadOwnerGoalsAsync(AccountId owner, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            if (_accounts.FindById(owner) is not { } account)
            {
                return Task.FromResult<OwnerGoals?>(null);
            }

            var goals = _goals.Where(g => g.Goal.Owner == owner).Select(Current).ToList();
            return Task.FromResult<OwnerGoals?>(new OwnerGoals(account.StoredTimeZone, _revisions.GetValueOrDefault(owner), goals));
        }
    }

    /// <summary>
    /// Research R-08 step 3, all or nothing: false when the stored zone or the revision changed, or when a write names a
    /// goal that is not one of the owner's or names a goal twice. Otherwise every change is applied through
    /// <see cref="GoalVersionChange.ApplyTo"/>, the account's zone is set and the revision incremented.
    /// </summary>
    public Task<bool> TryChangeTimeZoneAsync(
        AccountId owner,
        string? expectedStoredZone,
        int expectedRevision,
        NamedTimeZone next,
        IReadOnlyList<GoalVersionWrite> writes,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(writes);
        lock (_gate)
        {
            TimeZoneWriteCount++;
            if (_timeZoneConflictsToReport > 0)
            {
                _timeZoneConflictsToReport--;
                _revisions[owner] = _revisions.GetValueOrDefault(owner) + 1;
                return Task.FromResult(false);
            }

            if (_accounts.FindById(owner) is not { } account
                || !string.Equals(account.StoredTimeZone, expectedStoredZone, StringComparison.Ordinal)
                || _revisions.GetValueOrDefault(owner) != expectedRevision)
            {
                return Task.FromResult(false);
            }

            var positions = new List<int>();
            foreach (var write in writes)
            {
                var position = _goals.FindIndex(g => g.Goal.Owner == owner && g.Goal.Id == write.Goal);
                if (position < 0 || positions.Contains(position))
                {
                    return Task.FromResult(false);
                }

                positions.Add(position);
            }

            var changed = positions.Select((position, i) => _goals[position] with { Goal = writes[i].Change.ApplyTo(_goals[position].Goal) }).ToList();
            for (var i = 0; i < positions.Count; i++)
            {
                _goals[positions[i]] = changed[i];
            }

            _accounts.SetStoredTimeZone(owner, next.Id);
            _revisions[owner] = _revisions.GetValueOrDefault(owner) + 1;
            TimeZoneChangeCount++;
            return Task.FromResult(true);
        }
    }

    /// <summary>Slice 8: the account's stored zone and revision, then the goal, read together under this store's lock; null without an account.</summary>
    public Task<OwnerGoal?> ReadOwnerGoalAsync(AccountId owner, GoalId goal, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            _lookups++;
            _ownerGoalReads++;
            if (_accounts.FindById(owner) is not { } account)
            {
                return Task.FromResult<OwnerGoal?>(null);
            }

            var stored = _goals.FirstOrDefault(g => g.Goal.Owner == owner && g.Goal.Id == goal);
            return Task.FromResult<OwnerGoal?>(
                new OwnerGoal(account.StoredTimeZone, _revisions.GetValueOrDefault(owner), stored is null ? null : Current(stored)));
        }
    }

    /// <summary>
    /// Slice 8 (research R-05): <see cref="GoalChangeWriteOutcome.Stale"/> unless the latest attempt is
    /// <see cref="GoalSettingsWrite.Attempt"/> and not ended; then the own name and, when given, that attempt's versions
    /// are replaced.
    /// </summary>
    public Task<GoalChangeWriteOutcome> TryChangeSettingsAsync(
        AccountId owner, string? expectedStoredZone, int expectedRevision, GoalSettingsWrite write, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(write);
        return ConditionalWrite(owner, expectedStoredZone, expectedRevision, write.Goal, goal =>
        {
            if (goal.IsEnded || goal.Attempt.Number != write.Attempt)
            {
                return null;
            }

            var attempts = goal.Attempts.ToList();
            if (write.Versions is { } versions)
            {
                attempts[^1] = attempts[^1] with { Versions = versions };
            }

            return new Goal(goal.Id, goal.Owner, goal.Habit, write.Name, attempts);
        });
    }

    /// <summary>
    /// Slice 8 (research R-04, R-05): <see cref="GoalChangeWriteOutcome.Stale"/> unless the latest attempt is
    /// <see cref="GoalEndingWrite.Attempt"/> and not ended; then the ending and the kept versions are written, and the
    /// pauses and excusals after the last date are cut in the attached <see cref="Records"/>.
    /// </summary>
    public Task<GoalChangeWriteOutcome> TryEndAsync(
        AccountId owner, string? expectedStoredZone, int expectedRevision, GoalEndingWrite write, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(write);
        return ConditionalWrite(
            owner,
            expectedStoredZone,
            expectedRevision,
            write.Goal,
            goal =>
            {
                if (goal.IsEnded || goal.Attempt.Number != write.Attempt)
                {
                    return null;
                }

                var attempts = goal.Attempts.ToList();
                attempts[^1] = new GoalAttemptVersions(attempts[^1].Attempt with { Ending = write.Ending }, write.KeptVersions);
                return new Goal(goal.Id, goal.Owner, goal.Habit, goal.OwnName, attempts);
            },
            () => Records?.CutAfter(write.Goal, write.LastDate));
    }

    /// <summary>
    /// Slice 8 (research R-05): <see cref="GoalChangeWriteOutcome.Stale"/> unless the latest attempt is ended and
    /// numbered <see cref="GoalAttemptWrite.Number"/> − 1; then the attempt, beginning on its first version's date, is
    /// added with that version.
    /// </summary>
    public Task<GoalChangeWriteOutcome> TryStartAttemptAsync(
        AccountId owner, string? expectedStoredZone, int expectedRevision, GoalAttemptWrite write, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(write);
        return ConditionalWrite(owner, expectedStoredZone, expectedRevision, write.Goal, goal =>
        {
            if (!goal.IsEnded || goal.Attempt.Number != write.Number - 1)
            {
                return null;
            }

            var attempt = new GoalAttemptVersions(new GoalAttempt(write.Number, write.First.AppliesFrom), [write.First]);
            return new Goal(goal.Id, goal.Owner, goal.Habit, goal.OwnName, [.. goal.Attempts, attempt]);
        });
    }

    public Task<IReadOnlyList<StoredGoal>> ListAsync(AccountId owner, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            _lookups++;
            return Task.FromResult<IReadOnlyList<StoredGoal>>(_goals.Where(g => g.Goal.Owner == owner).Select(Current).ToList());
        }
    }

    public Task<StoredGoal?> FindAsync(AccountId owner, GoalId id, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            _lookups++;
            var stored = _goals.FirstOrDefault(g => g.Goal.Owner == owner && g.Goal.Id == id);
            return Task.FromResult(stored is null ? null : Current(stored));
        }
    }

    public Task<int> CountUsingCustomHabitAsync(AccountId owner, HabitId habit, CancellationToken cancellationToken)
    {
        var onHabit = new GoalHabit(habit, HabitKind.Custom);
        lock (_gate)
        {
            return Task.FromResult(_goals.Count(g => g.Goal.Owner == owner && g.Goal.Habit == onHabit));
        }
    }

    public Task<IReadOnlyDictionary<HabitId, int>> CountByCatalogHabitAsync(AccountId owner, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            IReadOnlyDictionary<HabitId, int> counts = _goals
                .Where(g => g.Goal.Owner == owner && g.Goal.Habit.Kind == HabitKind.Catalog && !g.Goal.IsEnded)
                .GroupBy(g => g.Goal.Habit.Id)
                .ToDictionary(group => group.Key, group => group.Count());
            return Task.FromResult(counts);
        }
    }

    public Task<IReadOnlyList<StoredGoal>> ListOnCatalogHabitAsync(AccountId owner, HabitId habit, CancellationToken cancellationToken)
    {
        var onHabit = new GoalHabit(habit, HabitKind.Catalog);
        lock (_gate)
        {
            return Task.FromResult<IReadOnlyList<StoredGoal>>(
                _goals.Where(g => g.Goal.Owner == owner && g.Goal.Habit == onHabit && !g.Goal.IsEnded).Select(Current).ToList());
        }
    }

    /// <summary>The one count across owners (ADR 0012): every owner's goals on the catalog habit, whatever its state.</summary>
    public Task<int> CountUsingCatalogHabitAsync(HabitId habit, CancellationToken cancellationToken)
    {
        var onHabit = new GoalHabit(habit, HabitKind.Catalog);
        lock (_gate)
        {
            return Task.FromResult(_goals.Count(g => g.Goal.Habit == onHabit));
        }
    }

    /// <summary>
    /// Runs <paramref name="action"/> under this store's lock with whether any goal is based on the custom habit
    /// <paramref name="habit"/>, so no goal can be created on it in between. For <see cref="FakeCustomHabitStore"/>'s
    /// conditional delete (FR-035, research R-08), which takes its own lock inside the action: this store's lock comes
    /// first, as everywhere else.
    /// </summary>
    public T WithGoalsOnCustomHabit<T>(HabitId habit, Func<bool, T> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        var onHabit = new GoalHabit(habit, HabitKind.Custom);
        lock (_gate)
        {
            return action(_goals.Any(g => g.Goal.Habit == onHabit));
        }
    }

    /// <summary>
    /// The one path of the three conditional goal-change writes, all or nothing: <see cref="BeforeNextWrite"/> runs
    /// first; then, under this store's lock, a stored zone or revision other than the expected ones gives
    /// <see cref="GoalChangeWriteOutcome.Stale"/>, a goal that is not one of the owner's
    /// <see cref="GoalChangeWriteOutcome.GoalNotFound"/>, and a null from <paramref name="change"/> (its precondition
    /// failed) <see cref="GoalChangeWriteOutcome.Stale"/>. Otherwise the changed goal replaces the stored one,
    /// <paramref name="after"/> runs, and the owner's revision is incremented.
    /// </summary>
    private Task<GoalChangeWriteOutcome> ConditionalWrite(
        AccountId owner, string? expectedStoredZone, int expectedRevision, GoalId goal, Func<Goal, Goal?> change, Action? after = null)
    {
        Interlocked.Exchange(ref _beforeNextWrite, null)?.Invoke();
        lock (_gate)
        {
            _changeWriteCount++;
            if (_accounts.FindById(owner) is not { } account
                || !string.Equals(account.StoredTimeZone, expectedStoredZone, StringComparison.Ordinal)
                || _revisions.GetValueOrDefault(owner) != expectedRevision)
            {
                return Task.FromResult(GoalChangeWriteOutcome.Stale);
            }

            var position = _goals.FindIndex(g => g.Goal.Owner == owner && g.Goal.Id == goal);
            if (position < 0)
            {
                return Task.FromResult(GoalChangeWriteOutcome.GoalNotFound);
            }

            if (change(_goals[position].Goal) is not { } changed)
            {
                return Task.FromResult(GoalChangeWriteOutcome.Stale);
            }

            _goals[position] = _goals[position] with { Goal = changed };
            after?.Invoke();
            _revisions[owner] = _revisions.GetValueOrDefault(owner) + 1;
            return Task.FromResult(GoalChangeWriteOutcome.Written);
        }
    }

    /// <summary>The habit a new goal may use: a Published catalog habit, or the owner's own custom habit; null otherwise.</summary>
    private GoalHabitView? HabitViewOf(AccountId owner, GoalHabit habit) => habit.Kind switch
    {
        HabitKind.Catalog => _catalogHabits.Get(habit.Id) is { State: CatalogHabitState.Published } catalog ? View(catalog) : null,
        HabitKind.Custom => _customHabits.Get(habit.Id) is { } custom && custom.Owner == owner ? View(custom) : null,
        _ => null,
    };

    /// <summary>The stored goal with its habit's current name, category and state, as the real store's join gives them.</summary>
    private StoredGoal Current(StoredGoal stored)
    {
        var habit = stored.Goal.Habit;
        GoalHabitView? current = habit.Kind switch
        {
            HabitKind.Catalog => _catalogHabits.Get(habit.Id) is { } catalog ? View(catalog) : null,
            HabitKind.Custom => _customHabits.Get(habit.Id) is { } custom ? View(custom) : null,
            _ => null,
        };
        return current is null ? stored : stored with { Habit = current };
    }

    private static GoalHabitView View(CatalogHabit habit) =>
        new(habit.Id, HabitKind.Catalog, habit.Definition.Name.Value, habit.Definition.Category, habit.State == CatalogHabitState.Published);

    private static GoalHabitView View(CustomHabit habit) =>
        new(habit.Id, HabitKind.Custom, habit.Definition.Name.Value, habit.Definition.Category, null);
}
