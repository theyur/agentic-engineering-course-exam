using System.Data;
using HT.Application.Operator;
using HT.Domain.Accounts;
using HT.Domain.Habits;
using HT.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace HT.Persistence.Habits;

/// <summary>
/// The demo data store (research R-09, ADR 0013). Each member is one transaction on the scoped context, which
/// Identity's user manager uses too. A demo account's custom habits are written with the custom habit store's
/// conditional insert (research R-14), so a taken name raises no database error; an insert of no row is a fault that
/// rolls the account back too and names no habit (HP-015). The change tracker is cleared before and after each call,
/// as in the other stores.
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
        Email email, string password, IReadOnlyList<HabitDefinition> customHabits, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(password);
        ArgumentNullException.ThrowIfNull(customHabits);
        _db.ChangeTracker.Clear();
        try
        {
            // Disposed without a commit, the transaction rolls back: the account and every custom habit, or nothing.
            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            var user = new HtUser
            {
                Id = Guid.NewGuid(),
                UserName = email.Value,
                Email = email.Value,
                Role = Role.RegisteredUser,
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

            var owner = new AccountId(user.Id);
            foreach (var habit in customHabits)
            {
                ArgumentNullException.ThrowIfNull(habit);
                var rows = await CustomHabitStore.InsertAsync(_db, Guid.NewGuid(), owner, habit, cancellationToken).ConfigureAwait(false);
                if (rows != 1)
                {
                    throw new InvalidOperationException("Demo account creation failed: a demo custom habit could not be written.");
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
}
