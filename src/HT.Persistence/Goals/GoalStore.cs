using System.Data.Common;
using System.Linq.Expressions;
using HT.Application.Goals;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Time;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace HT.Persistence.Goals;

/// <summary>
/// The goal store (research R-08). Every statement is scoped by owner (constitution III), except the count across
/// owners for the unpublish confirmation, which returns only a number (ADR 0012). Reads are no-tracking and
/// read each goal with its versions, its attempts and its habit's current name, category and state in one query.
/// Creation is one transaction that locks the account row first, then the habit row, with key-range locks (UPDLOCK,
/// HOLDLOCK), so it serializes against time-zone changes (through <c>GoalsRevision</c>), unpublishing and custom-habit
/// deletion. No statement raises a database error that carries a value: the checks are conditional statements, and
/// the values written already satisfy the Domain's lengths. Any database fault becomes an exception that names the SQL
/// error number only, with no inner exception (HP-011).
/// </summary>
internal sealed class GoalStore : IGoalStore
{
    private static readonly string Published = CatalogHabitState.Published.ToString();

    /// <summary>
    /// Slice 8 (FR-020): a goal that is not ended, i.e. whose latest attempt has no ending date. Every goal has an
    /// attempt, so the latest one's ending date is null exactly when the goal is not ended.
    /// </summary>
    private static readonly Expression<Func<GoalRow, bool>> IsNotEnded =
        g => g.Attempts.OrderByDescending(a => a.Number).Select(a => a.EndingDate).FirstOrDefault() == null;

    private readonly HtDbContext _db;

    public GoalStore(HtDbContext db) => _db = db;

    public async Task<GoalWriteOutcome> CreateAsync(
        AccountId owner, NewGoal goal, NamedTimeZone expectedZone, NamedTimeZone defaultZone, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(goal);
        _db.ChangeTracker.Clear();
        try
        {
            return await WriteAsync(() => CreateInTransactionAsync(owner, goal, expectedZone, defaultZone, cancellationToken))
                .ConfigureAwait(false);
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }
    }

    /// <summary>
    /// Research R-08 step 1. The order is fixed: the account row first, then the goals, as two statements under the
    /// database's read committed (locking, or with read committed snapshot). Each sees what was committed when it
    /// started, so a goal committed before the account row was read is in the goals read, and a goal committed after
    /// it has incremented <c>GoalsRevision</c>, which makes the conditional write of <see cref="TryChangeTimeZoneAsync"/>
    /// fail. Do not reorder the two reads.
    /// </summary>
    public async Task<OwnerGoals?> ReadOwnerGoalsAsync(AccountId owner, CancellationToken cancellationToken)
    {
        var account = await _db.Users.AsNoTracking()
            .Where(u => u.Id == owner.Value)
            .Select(u => new { u.TimeZone, u.GoalsRevision })
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
        if (account is null)
        {
            return null;
        }

        var rows = await Read(owner).ToListAsync(cancellationToken).ConfigureAwait(false);
        return new OwnerGoals(account.TimeZone, account.GoalsRevision, rows.Select(GoalMapping.ToStoredGoal).ToList());
    }

    public async Task<bool> TryChangeTimeZoneAsync(
        AccountId owner,
        string? expectedStoredZone,
        int expectedRevision,
        NamedTimeZone next,
        IReadOnlyList<GoalVersionWrite> writes,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(writes);
        _db.ChangeTracker.Clear();
        try
        {
            return await WriteAsync(() => ChangeTimeZoneInTransactionAsync(owner, expectedStoredZone, expectedRevision, next, writes, cancellationToken))
                .ConfigureAwait(false);
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }
    }

    /// <summary>
    /// Slice 8 (research R-05): the read order of <see cref="ReadOwnerGoalsAsync"/> for one goal, the account row first,
    /// then the goal, owner-scoped in the statement. Do not reorder the two reads.
    /// </summary>
    public async Task<OwnerGoal?> ReadOwnerGoalAsync(AccountId owner, GoalId goal, CancellationToken cancellationToken)
    {
        var account = await _db.Users.AsNoTracking()
            .Where(u => u.Id == owner.Value)
            .Select(u => new { u.TimeZone, u.GoalsRevision })
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
        if (account is null)
        {
            return null;
        }

        var row = await Read(owner).Where(r => r.Goal.Id == goal.Value).SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        return new OwnerGoal(account.TimeZone, account.GoalsRevision, row is null ? null : GoalMapping.ToStoredGoal(row));
    }

    /// <summary>Slice 8 (FR-001 to FR-006): the latest attempt is the one changed and is not ended; then the name and, when given, its versions.</summary>
    public Task<GoalChangeWriteOutcome> TryChangeSettingsAsync(
        AccountId owner, string? expectedStoredZone, int expectedRevision, GoalSettingsWrite write, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(write);
        var goal = write.Goal.Value;
        return ChangeGoalAsync(
            owner,
            expectedStoredZone,
            expectedRevision,
            goal,
            latest => latest.Number == write.Attempt && latest.EndingDate is null,
            async () =>
            {
                var name = write.Name?.Value;
                await _db.Goals
                    .Where(g => g.Id == goal && g.OwnerId == owner.Value)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(g => g.Name, name), cancellationToken)
                    .ConfigureAwait(false);
                if (write.Versions is { } versions)
                {
                    await ReplaceVersionsAsync(goal, write.Attempt, versions, cancellationToken).ConfigureAwait(false);
                }
            },
            cancellationToken);
    }

    /// <summary>
    /// Slice 8 (FR-007 to FR-009, research R-04): the latest attempt is the one ended and is not ended yet; then the
    /// ending, the kept versions, and the cuts of the goal's pauses and excusals after the last date, in the same
    /// transaction. Records and notes stay.
    /// </summary>
    public Task<GoalChangeWriteOutcome> TryEndAsync(
        AccountId owner, string? expectedStoredZone, int expectedRevision, GoalEndingWrite write, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(write);
        ArgumentNullException.ThrowIfNull(write.Ending);
        var goal = write.Goal.Value;
        return ChangeGoalAsync(
            owner,
            expectedStoredZone,
            expectedRevision,
            goal,
            latest => latest.Number == write.Attempt && latest.EndingDate is null,
            async () =>
            {
                var endingDate = write.Ending.Date;
                Outcome? outcome = write.Ending.Outcome;
                var note = write.Ending.Note?.Value;
                await _db.GoalAttempts
                    .Where(a => a.GoalId == goal && a.Number == write.Attempt)
                    .ExecuteUpdateAsync(
                        setters => setters
                            .SetProperty(a => a.EndingDate, endingDate)
                            .SetProperty(a => a.Outcome, outcome)
                            .SetProperty(a => a.EndingNote, note),
                        cancellationToken)
                    .ConfigureAwait(false);
                await ReplaceVersionsAsync(goal, write.Attempt, write.KeptVersions, cancellationToken).ConfigureAwait(false);

                var last = write.LastDate;
                await _db.GoalPauses
                    .Where(p => p.GoalId == goal && p.FirstDay > last)
                    .ExecuteDeleteAsync(cancellationToken)
                    .ConfigureAwait(false);
                await _db.GoalPauses
                    .Where(p => p.GoalId == goal && p.FirstDay <= last && p.LastDay > last)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(p => p.LastDay, last), cancellationToken)
                    .ConfigureAwait(false);
                await _db.GoalExcusals
                    .Where(e => e.GoalId == goal && e.Date > last)
                    .ExecuteDeleteAsync(cancellationToken)
                    .ConfigureAwait(false);
            },
            cancellationToken);
    }

    /// <summary>Slice 8 (FR-012): the latest attempt is ended and numbered one below the new one; then the attempt and its first version.</summary>
    public Task<GoalChangeWriteOutcome> TryStartAttemptAsync(
        AccountId owner, string? expectedStoredZone, int expectedRevision, GoalAttemptWrite write, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(write);
        ArgumentNullException.ThrowIfNull(write.First);
        var goal = write.Goal.Value;
        return ChangeGoalAsync(
            owner,
            expectedStoredZone,
            expectedRevision,
            goal,
            latest => latest.Number == write.Number - 1 && latest.EndingDate is not null,
            () =>
            {
                _db.GoalAttempts.Add(new GoalAttemptRow { GoalId = goal, Number = write.Number, StartDate = write.First.AppliesFrom });
                _db.GoalSettingsVersions.Add(GoalMapping.ToRow(goal, write.Number, write.First));
                return Task.CompletedTask;
            },
            cancellationToken);
    }

    public async Task<IReadOnlyList<StoredGoal>> ListAsync(AccountId owner, CancellationToken cancellationToken)
    {
        var rows = await Read(owner).ToListAsync(cancellationToken).ConfigureAwait(false);
        return rows.Select(GoalMapping.ToStoredGoal).ToList();
    }

    public async Task<StoredGoal?> FindAsync(AccountId owner, GoalId id, CancellationToken cancellationToken)
    {
        var row = await Read(owner).Where(r => r.Goal.Id == id.Value).SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        return row is null ? null : GoalMapping.ToStoredGoal(row);
    }

    public Task<int> CountUsingCustomHabitAsync(AccountId owner, HabitId habit, CancellationToken cancellationToken) =>
        _db.Goals.AsNoTracking()
            .CountAsync(g => g.OwnerId == owner.Value && g.CustomHabitId == habit.Value, cancellationToken);

    /// <summary>Slice 8 (FR-020): goals whose latest attempt has an ending date are left out.</summary>
    public async Task<IReadOnlyDictionary<HabitId, int>> CountByCatalogHabitAsync(AccountId owner, CancellationToken cancellationToken)
    {
        var counts = await _db.Goals.AsNoTracking()
            .Where(g => g.OwnerId == owner.Value && g.CatalogHabitId != null)
            .Where(IsNotEnded)
            .GroupBy(g => g.CatalogHabitId!.Value)
            .Select(group => new { Habit = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        return counts.ToDictionary(c => new HabitId(c.Habit), c => c.Count);
    }

    /// <summary>Slice 8 (FR-020): goals whose latest attempt has an ending date are left out.</summary>
    public async Task<IReadOnlyList<StoredGoal>> ListOnCatalogHabitAsync(AccountId owner, HabitId habit, CancellationToken cancellationToken)
    {
        var onHabit = habit.Value;
        var rows = await Read(owner, _db.Goals.Where(g => g.CatalogHabitId == onHabit).Where(IsNotEnded))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        return rows.Select(GoalMapping.ToStoredGoal).ToList();
    }

    /// <summary>
    /// The one statement not scoped by owner (ADR 0012): a count only. It holds no lock past its own read under read
    /// committed, so it never blocks a goal creation for longer than the read, and the confirmed unpublish's
    /// compare-and-swap does not depend on it.
    /// </summary>
    public Task<int> CountUsingCatalogHabitAsync(HabitId habit, CancellationToken cancellationToken) =>
        _db.Goals.AsNoTracking().CountAsync(g => g.CatalogHabitId == habit.Value, cancellationToken);

    /// <summary>Research R-08 steps 2 to 6, in one transaction; nothing is written unless every check passes.</summary>
    private async Task<GoalWriteOutcome> CreateInTransactionAsync(
        AccountId owner, NewGoal goal, NamedTimeZone expectedZone, NamedTimeZone defaultZone, CancellationToken cancellationToken)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        // Step 2: the account row first, under its update lock. No row means the account is gone; the use case reads
        // the account again and finds that out, so the store answers as for a changed zone.
        var zones = await _db.Database.SqlQuery<string?>(
                $"SELECT [TimeZone] AS [Value] FROM [AspNetUsers] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {owner.Value}")
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        if (zones is not [var storedZone]
            || !string.Equals(storedZone ?? defaultZone.Id, expectedZone.Id, StringComparison.Ordinal))
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return GoalWriteOutcome.TimeZoneChanged;
        }

        // Step 3: the habit, under the same locks until commit.
        if (!await HabitIsUsableAsync(owner, goal.Habit, cancellationToken).ConfigureAwait(false))
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return GoalWriteOutcome.HabitGone;
        }

        // Step 4: a missing zone becomes the expected one, and the revision moves on.
        var zone = expectedZone.Id;
        await _db.Users
            .Where(u => u.Id == owner.Value)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(u => u.TimeZone, u => u.TimeZone ?? zone)
                    .SetProperty(u => u.GoalsRevision, u => u.GoalsRevision + 1),
                cancellationToken)
            .ConfigureAwait(false);

        // Step 5: the goal, version 1 with the expected zone from the start date, and attempt 1 from the start date.
        var id = Guid.NewGuid();
        var version = new GoalSettingsVersion(new GoalSettings(goal.Target, expectedZone), goal.StartDate);
        _db.Goals.Add(new GoalRow
        {
            Id = id,
            OwnerId = owner.Value,
            CatalogHabitId = goal.Habit.Kind == HabitKind.Catalog ? goal.Habit.Id.Value : null,
            CustomHabitId = goal.Habit.Kind == HabitKind.Custom ? goal.Habit.Id.Value : null,
            StartDate = goal.StartDate,
            Name = goal.OwnName?.Value,
            Versions = [GoalMapping.ToRow(id, version)],
            Attempts = [new GoalAttemptRow { GoalId = id, Number = GoalAttempt.First, StartDate = goal.StartDate }],
        });
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return GoalWriteOutcome.Created(new GoalId(id));
    }

    /// <summary>
    /// Research R-08 step 3, in one transaction: the account row under its update lock first, as in creation, then the
    /// owner's goal rows. Nothing is written unless the stored zone and the revision are the expected ones and every write
    /// names a different goal of the owner (constitution III).
    /// </summary>
    private async Task<bool> ChangeTimeZoneInTransactionAsync(
        AccountId owner,
        string? expectedStoredZone,
        int expectedRevision,
        NamedTimeZone next,
        IReadOnlyList<GoalVersionWrite> writes,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        var accounts = await _db.Database.SqlQuery<AccountRevisionRow>(
                $"SELECT [TimeZone], [GoalsRevision] FROM [AspNetUsers] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {owner.Value}")
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        if (accounts is not [var account]
            || !string.Equals(account.TimeZone, expectedStoredZone, StringComparison.Ordinal)
            || account.GoalsRevision != expectedRevision)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return false;
        }

        // Every write names a different goal of the owner; the owner's goal rows stay locked until commit.
        var ownGoals = await _db.Database.SqlQuery<Guid>(
                $"SELECT [Id] AS [Value] FROM [Goals] WITH (UPDLOCK, HOLDLOCK) WHERE [OwnerId] = {owner.Value}")
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var named = writes.Select(w => w.Goal.Value).ToList();
        if (named.Distinct().Count() != named.Count || !named.All(ownGoals.Contains))
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return false;
        }

        foreach (var write in writes.Where(w => w.Change.Kind != GoalVersionChangeKind.None))
        {
            await ApplyAsync(write.Goal.Value, write.Change, cancellationToken).ConfigureAwait(false);
        }

        var zone = next.Id;
        await _db.Users
            .Where(u => u.Id == owner.Value)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(u => u.TimeZone, zone)
                    .SetProperty(u => u.GoalsRevision, u => u.GoalsRevision + 1),
                cancellationToken)
            .ConfigureAwait(false);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    /// <summary>
    /// One goal's version rows for a change, on its latest attempt's versions only (slice 8, FR-021): ReplaceFirst
    /// updates that attempt's first version's zone; ReplacePending deletes its last version's row and adds one with the
    /// new zone and date; Add adds a row with its last version's target. The rows to add are saved by the caller, after
    /// every delete has run.
    /// </summary>
    private async Task ApplyAsync(Guid goal, GoalVersionChange change, CancellationToken cancellationToken)
    {
        var attempt = await _db.GoalAttempts.AsNoTracking()
            .Where(a => a.GoalId == goal)
            .MaxAsync(a => (int?)a.Number, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException("A stored goal has no attempt.");
        var versions = await _db.GoalSettingsVersions.AsNoTracking()
            .Where(v => v.GoalId == goal && v.AttemptNumber == attempt)
            .OrderBy(v => v.AppliesFrom)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        if (versions.Count == 0)
        {
            throw new InvalidOperationException("A stored goal has no settings version.");
        }

        var zone = change.TimeZone!.Value.Id;
        switch (change.Kind)
        {
            case GoalVersionChangeKind.ReplaceFirst:
                var first = versions[0].AppliesFrom;
                await _db.GoalSettingsVersions
                    .Where(v => v.GoalId == goal && v.AttemptNumber == attempt && v.AppliesFrom == first)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(v => v.TimeZone, zone), cancellationToken)
                    .ConfigureAwait(false);
                return;
            case GoalVersionChangeKind.ReplacePending:
                if (versions.Count < 2)
                {
                    throw new InvalidOperationException("A time-zone write replaces a version the goal does not have.");
                }

                var pending = versions[^1].AppliesFrom;
                await _db.GoalSettingsVersions
                    .Where(v => v.GoalId == goal && v.AttemptNumber == attempt && v.AppliesFrom == pending)
                    .ExecuteDeleteAsync(cancellationToken)
                    .ConfigureAwait(false);
                _db.GoalSettingsVersions.Add(CopyOf(versions[^1], change.AppliesFrom!.Value, zone));
                return;
            case GoalVersionChangeKind.Add:
                _db.GoalSettingsVersions.Add(CopyOf(versions[^1], change.AppliesFrom!.Value, zone));
                return;
            default:
                return;
        }
    }

    /// <summary>A version row with the target of <paramref name="row"/>, another date and another zone.</summary>
    private static GoalSettingsVersionRow CopyOf(GoalSettingsVersionRow row, DateOnly appliesFrom, string zone) => new()
    {
        GoalId = row.GoalId,
        AttemptNumber = row.AttemptNumber,
        AppliesFrom = appliesFrom,
        BuildOrReduce = row.BuildOrReduce,
        Measurement = row.Measurement,
        Amount = row.Amount,
        CountUnit = row.CountUnit,
        DurationUnit = row.DurationUnit,
        ScheduleKind = row.ScheduleKind,
        Days = row.Days,
        Times = row.Times,
        Period = row.Period,
        TimeZone = zone,
    };

    /// <summary>A Published catalog habit, or the owner's own custom habit; the row stays locked until commit.</summary>
    private async Task<bool> HabitIsUsableAsync(AccountId owner, GoalHabit habit, CancellationToken cancellationToken)
    {
        var query = habit.Kind switch
        {
            HabitKind.Catalog => _db.Database.SqlQuery<int>(
                $"SELECT 1 AS [Value] FROM [CatalogHabits] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {habit.Id.Value} AND [State] = {Published}"),
            HabitKind.Custom => _db.Database.SqlQuery<int>(
                $"SELECT 1 AS [Value] FROM [CustomHabits] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {habit.Id.Value} AND [OwnerId] = {owner.Value}"),
            _ => null,
        };
        return query is not null && (await query.ToListAsync(cancellationToken).ConfigureAwait(false)).Count == 1;
    }

    /// <summary>
    /// Slice 8 (research R-05), one transaction for each conditional goal-change write: the account row under its
    /// update lock first, as in creation and the time-zone change, checking the stored zone and the revision; then the
    /// owner's goal row under its update lock, then its latest attempt, checked by <paramref name="precondition"/>.
    /// A failed check gives <see cref="GoalChangeWriteOutcome.Stale"/>, no goal of the owner
    /// <see cref="GoalChangeWriteOutcome.GoalNotFound"/>, both with nothing written. Otherwise <paramref name="write"/>
    /// runs, the revision is incremented, and the added rows are saved before commit. Logs nothing; a fault names the SQL
    /// error number only.
    /// </summary>
    private async Task<GoalChangeWriteOutcome> ChangeGoalAsync(
        AccountId owner,
        string? expectedStoredZone,
        int expectedRevision,
        Guid goal,
        Func<LatestAttemptRow, bool> precondition,
        Func<Task> write,
        CancellationToken cancellationToken)
    {
        _db.ChangeTracker.Clear();
        try
        {
            return await WriteAsync(async () =>
            {
                await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

                var accounts = await _db.Database.SqlQuery<AccountRevisionRow>(
                        $"SELECT [TimeZone], [GoalsRevision] FROM [AspNetUsers] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {owner.Value}")
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
                if (accounts is not [var account]
                    || !string.Equals(account.TimeZone, expectedStoredZone, StringComparison.Ordinal)
                    || account.GoalsRevision != expectedRevision)
                {
                    await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                    return GoalChangeWriteOutcome.Stale;
                }

                var locked = await _db.Database.SqlQuery<int>(
                        $"SELECT 1 AS [Value] FROM [Goals] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {goal} AND [OwnerId] = {owner.Value}")
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
                if (locked.Count != 1)
                {
                    await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                    return GoalChangeWriteOutcome.GoalNotFound;
                }

                var latest = await _db.GoalAttempts.AsNoTracking()
                    .Where(a => a.GoalId == goal)
                    .OrderByDescending(a => a.Number)
                    .Select(a => new LatestAttemptRow { Number = a.Number, EndingDate = a.EndingDate })
                    .FirstOrDefaultAsync(cancellationToken)
                    .ConfigureAwait(false);
                if (latest is null || !precondition(latest))
                {
                    await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                    return GoalChangeWriteOutcome.Stale;
                }

                await write().ConfigureAwait(false);
                await _db.Users
                    .Where(u => u.Id == owner.Value)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(u => u.GoalsRevision, u => u.GoalsRevision + 1), cancellationToken)
                    .ConfigureAwait(false);
                await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                return GoalChangeWriteOutcome.Written;
            }).ConfigureAwait(false);
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }
    }

    /// <summary>
    /// Replaces the version rows of the locked goal's attempt <paramref name="attempt"/> with <paramref name="versions"/>;
    /// the new rows are saved by the caller.
    /// </summary>
    private async Task ReplaceVersionsAsync(
        Guid goal, int attempt, IReadOnlyList<GoalSettingsVersion> versions, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(versions);
        await _db.GoalSettingsVersions
            .Where(v => v.GoalId == goal && v.AttemptNumber == attempt)
            .ExecuteDeleteAsync(cancellationToken)
            .ConfigureAwait(false);
        _db.GoalSettingsVersions.AddRange(versions.Select(version => GoalMapping.ToRow(goal, attempt, version)));
    }

    /// <summary>
    /// The owner's goals in creation order, each with its versions, its attempts and its habit's current name,
    /// category and state, as one query with left joins on both habit tables.
    /// </summary>
    private IQueryable<GoalReadRow> Read(AccountId owner) => Read(owner, _db.Goals);

    /// <summary>
    /// The owner's goals among <paramref name="goals"/>, read as <see cref="Read(AccountId)"/> reads them; the filter is
    /// applied to the goal rows before the projection.
    /// </summary>
    private static IQueryable<GoalReadRow> Read(AccountId owner, IQueryable<GoalRow> goals) =>
        goals.AsNoTracking()
            .AsSingleQuery()
            .Where(g => g.OwnerId == owner.Value)
            .OrderBy(g => g.CreationSequence)
            .Select(g => new GoalReadRow
            {
                Goal = g,
                Versions = g.Versions.ToList(),
                Attempts = g.Attempts.ToList(),
                CatalogHabitName = g.CatalogHabit!.Name,
                CatalogHabitCategory = (Category?)g.CatalogHabit!.Category,
                CatalogHabitState = (CatalogHabitState?)g.CatalogHabit!.State,
                CustomHabitName = g.CustomHabit!.Name,
                CustomHabitCategory = (Category?)g.CustomHabit!.Category,
            });

    /// <summary>Runs a write; a database fault becomes an exception with the SQL error number only (HP-011).</summary>
    private static async Task<T> WriteAsync<T>(Func<Task<T>> write)
    {
        int number;
        try
        {
            return await write().ConfigureAwait(false);
        }
        catch (SqlException exception)
        {
            number = exception.Number;
        }
        catch (DbUpdateException exception)
        {
            number = exception.InnerException is SqlException sql ? sql.Number : 0;
        }
        catch (DbException)
        {
            number = 0;
        }

        throw new InvalidOperationException($"Saving a goal failed (SQL error {number}).");
    }
}
