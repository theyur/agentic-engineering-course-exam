using System.Data;
using System.Data.Common;
using HT.Application.Operator;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Persistence.Goals;
using HT.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace HT.Persistence.Habits;

/// <summary>
/// The demo data store (research R-09, slice 4 research R-10, ADR 0013). Each member is one transaction on the scoped
/// context, which Identity's user manager uses too. A demo account is inserted with its profile; its custom habits are
/// written with the custom habit store's conditional insert (research R-14), so a taken name raises no database error;
/// an insert of no row is a fault that rolls the account back too and names no habit (HP-015). Its goals follow in
/// the same transaction: a catalog habit is looked up by name key under key-range locks, as goal creation does, and a
/// goal whose habit does not qualify is skipped; a custom habit is the identifier just generated for its position. A
/// goal that cannot be written is a fault that names the SQL error number only (HP-011). The change tracker is
/// cleared before and after each call, as in the other stores.
/// </summary>
internal sealed class DemoDataStore : IDemoDataStore
{
    private const string DuplicateUserNameCode = nameof(IdentityErrorDescriber.DuplicateUserName);
    private const int DeadlockVictim = 1205;

    private readonly HtDbContext _db;
    private readonly UserManager<HtUser> _users;

    public DemoDataStore(HtDbContext db, UserManager<HtUser> users)
    {
        _db = db;
        _users = users;
    }

    public async Task<CreateDemoAccountOutcome> CreateDemoAccountAsync(
        Email email,
        string password,
        IReadOnlyList<HabitDefinition> customHabits,
        DemoAccountProfile profile,
        IReadOnlyList<DemoGoal> goals,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(password);
        ArgumentNullException.ThrowIfNull(customHabits);
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(goals);
        _db.ChangeTracker.Clear();
        try
        {
            // Disposed without a commit, the transaction rolls back: the account, every custom habit and every goal, or nothing.
            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            var user = new HtUser
            {
                Id = Guid.NewGuid(),
                UserName = email.Value,
                Email = email.Value,
                Role = Role.RegisteredUser,

                // Written at insert only, as account creation does (research R-04).
                Name = profile.Name,
                TimeZone = profile.TimeZone.Id,
                CreatedAt = profile.CreatedAt,
            };

            IdentityResult result;
            try
            {
                result = await _users.CreateAsync(user, password).ConfigureAwait(false);
            }
            catch (DbUpdateException exception) when (HabitMapping.IsUniqueIndexViolation(exception))
            {
                return CreateDemoAccountOutcome.EmailTaken;
            }

            if (!result.Succeeded)
            {
                return result.Errors.Any(e => e.Code == DuplicateUserNameCode)
                    ? CreateDemoAccountOutcome.EmailTaken
                    : throw new InvalidOperationException(
                        "Demo account creation failed: " + string.Join(", ", result.Errors.Select(e => e.Code)));
            }

            // Identity's user manager tracks the new user; the goals below must not write it again.
            _db.ChangeTracker.Clear();

            var owner = new AccountId(user.Id);
            var customHabitIds = new List<Guid>(customHabits.Count);
            foreach (var habit in customHabits)
            {
                ArgumentNullException.ThrowIfNull(habit);
                var id = Guid.NewGuid();
                var rows = await CustomHabitStore.InsertAsync(_db, id, owner, habit, cancellationToken).ConfigureAwait(false);
                if (rows != 1)
                {
                    throw new InvalidOperationException("Demo account creation failed: a demo custom habit could not be written.");
                }

                customHabitIds.Add(id);
            }

            foreach (var goal in goals)
            {
                ArgumentNullException.ThrowIfNull(goal);
                if (await HabitOfAsync(goal, customHabitIds, cancellationToken).ConfigureAwait(false) is { } habit)
                {
                    await WriteGoalAsync(owner, habit, goal, profile, cancellationToken).ConfigureAwait(false);
                }
            }

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return CreateDemoAccountOutcome.Created;
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }
    }

    public async Task<bool> TryCreateSeedCatalogAsync(IReadOnlyList<SeedCatalogHabit> habits, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(habits);
        _db.ChangeTracker.Clear();
        try
        {
            // Serializable: the empty check holds a key-range lock until the commit, so a concurrent start waits, or
            // one of the two is chosen as a deadlock victim and rolls back.
            await using var transaction = await _db.Database
                .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken)
                .ConfigureAwait(false);
            if (await _db.CatalogHabits.AnyAsync(cancellationToken).ConfigureAwait(false))
            {
                return false;
            }

            foreach (var habit in habits)
            {
                ArgumentNullException.ThrowIfNull(habit);
                var row = new CatalogHabitRow
                {
                    Id = Guid.NewGuid(),
                    State = habit.State,
                    SuggestedStartingGoals = HabitMapping.ToRows(habit.SuggestedStartingGoals),
                };
                HabitMapping.CopyDefinition(habit.Definition, row);
                _db.CatalogHabits.Add(row);
            }

            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (DbUpdateException exception) when (HabitMapping.IsUniqueIndexViolation(exception) || IsDeadlockVictim(exception.InnerException))
        {
            // A concurrent start or an administrator created a catalog habit first: the catalog is not empty.
            return false;
        }
        catch (SqlException exception) when (IsDeadlockVictim(exception))
        {
            return false;
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }
    }

    private static bool IsDeadlockVictim(Exception? exception) => exception is SqlException { Number: DeadlockVictim };

    /// <summary>
    /// The goal's habit, or null when it does not qualify. A catalog habit is read by name key with its state, under
    /// the locks of goal creation (UPDLOCK, HOLDLOCK) until the commit, so it cannot be unpublished in between; that
    /// state is the only data this store reads (research R-10). A custom habit is the identifier generated in this
    /// call for its position, not read.
    /// </summary>
    private async Task<GoalHabit?> HabitOfAsync(DemoGoal goal, List<Guid> customHabitIds, CancellationToken cancellationToken)
    {
        if (goal.CustomHabitPosition is { } position)
        {
            return position < customHabitIds.Count
                ? new GoalHabit(new HabitId(customHabitIds[position]), HabitKind.Custom)
                : throw new InvalidOperationException("Demo account creation failed: a demo goal names no demo custom habit of the call.");
        }

        var key = HabitName.KeyOf(goal.CatalogHabitName ?? throw new InvalidOperationException("Demo account creation failed: a demo goal names no habit."));
        var found = await WriteAsync(() => _db.Database.SqlQuery<CatalogHabitStateRow>(
                    $"SELECT [Id], [State] FROM [CatalogHabits] WITH (UPDLOCK, HOLDLOCK) WHERE [NameKey] = {key}")
                .ToListAsync(cancellationToken))
            .ConfigureAwait(false);
        return found is [var row]
               && Enum.TryParse<CatalogHabitState>(row.State, ignoreCase: false, out var state)
               && goal.AcceptedStates.Contains(state)
            ? new GoalHabit(new HabitId(row.Id), HabitKind.Catalog)
            : null;
    }

    /// <summary>The goal row with settings version 1 in the profile's zone and attempt 1, both from the start date.</summary>
    private async Task WriteGoalAsync(
        AccountId owner, GoalHabit habit, DemoGoal goal, DemoAccountProfile profile, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();
        var version = new GoalSettingsVersion(new GoalSettings(goal.Target, profile.TimeZone), goal.StartDate);
        _db.Goals.Add(new GoalRow
        {
            Id = id,
            OwnerId = owner.Value,
            CatalogHabitId = habit.Kind == HabitKind.Catalog ? habit.Id.Value : null,
            CustomHabitId = habit.Kind == HabitKind.Custom ? habit.Id.Value : null,
            StartDate = goal.StartDate,
            Name = goal.OwnName?.Value,
            Versions = [GoalMapping.ToRow(id, version)],
            Attempts = [new GoalAttemptRow { GoalId = id, Number = GoalAttempt.First, StartDate = goal.StartDate }],
        });

        // One save per goal, so the creation order (the identity column) is the order of the list.
        await WriteAsync(() => _db.SaveChangesAsync(cancellationToken)).ConfigureAwait(false);
        _db.ChangeTracker.Clear();
    }

    /// <summary>A database fault of the goal part becomes an exception that names the SQL error number only (HP-011).</summary>
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

        throw new InvalidOperationException($"Demo account creation failed: a demo goal could not be written (SQL error {number}).");
    }
}
