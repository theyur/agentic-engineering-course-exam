using System.Data.Common;
using HT.Application.Operator;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Persistence.Goals;
using HT.Persistence.Identity;
using HT.Persistence.Records;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace HT.Persistence.Habits;

/// <summary>
/// The writer of a demo account with history, shared by <see cref="DemoHistoryStore"/> (slice 5 FR-030, research R-08)
/// and <see cref="DemoReportStore"/> (slice 6 FR-032, research R-09), so both ports write in exactly the same way
/// (ADR 0013, ADR 0021). Its one member is one transaction on the scoped context,
/// which Identity's user manager uses too: the demo account is inserted with its profile, then each goal whose catalog
/// habit, looked up by name key under the key-range locks of goal creation, is in one of the goal's accepted states,
/// with settings version 1 in the profile's zone and attempt 1 from its start date, its records in list order (one
/// save each, so the stored order is the list's) and its notes, mapped by <see cref="RecordMapping"/>. A goal whose
/// habit does not qualify is left out with its records and notes. A goal, record or note that cannot be written is a
/// fault that rolls the account back too; its exception names the SQL error number only, with no inner exception, so
/// no goal, habit, note or value reaches a log (HP-006). The change tracker is cleared before and after the call, as
/// in the other stores. Each fault text starts with the caller's <c>failure</c> prefix, so each store keeps its own.
/// </summary>
internal sealed class DemoAccountWriter
{
    private const string DuplicateUserNameCode = nameof(IdentityErrorDescriber.DuplicateUserName);

    private readonly HtDbContext _db;
    private readonly UserManager<HtUser> _users;
    private readonly string _failure;

    /// <param name="failure">The start of every fault text, such as "Demo history account creation failed"; it names no value.</param>
    public DemoAccountWriter(HtDbContext db, UserManager<HtUser> users, string failure)
    {
        _db = db;
        _users = users;
        _failure = failure;
    }

    public async Task<CreateDemoAccountOutcome> CreateAsync(
        Email email,
        string password,
        DemoAccountProfile profile,
        IReadOnlyList<DemoHistoryGoal> goals,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(password);
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(goals);
        _db.ChangeTracker.Clear();
        try
        {
            // Disposed without a commit, the transaction rolls back: the account, every goal, record and note, or nothing.
            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            if (await CreateUserAsync(email, password, profile).ConfigureAwait(false) is not { } owner)
            {
                return CreateDemoAccountOutcome.EmailTaken;
            }

            // Identity's user manager tracks the new user; the goals below must not write it again.
            _db.ChangeTracker.Clear();

            foreach (var goal in goals)
            {
                ArgumentNullException.ThrowIfNull(goal);
                if (await PublishedHabitOfAsync(goal.Goal, cancellationToken).ConfigureAwait(false) is { } habit)
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

    /// <summary>The new registered user with the profile, written at insert only as account creation does; null when the email is taken.</summary>
    private async Task<AccountId?> CreateUserAsync(Email email, string password, DemoAccountProfile profile)
    {
        var user = new HtUser
        {
            Id = Guid.NewGuid(),
            UserName = email.Value,
            Email = email.Value,
            Role = Role.RegisteredUser,
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
            return null;
        }

        if (result.Succeeded)
        {
            return new AccountId(user.Id);
        }

        return result.Errors.Any(e => e.Code == DuplicateUserNameCode)
            ? null
            : throw new InvalidOperationException(
                _failure + ": " + string.Join(", ", result.Errors.Select(e => e.Code)));
    }

    /// <summary>
    /// The goal's catalog habit, or null when it is absent or not in one of the goal's accepted states. Read by name
    /// key with its state under UPDLOCK, HOLDLOCK until the commit, so it cannot change state in between; that state is
    /// the only data this store reads.
    /// </summary>
    private async Task<Guid?> PublishedHabitOfAsync(DemoGoal goal, CancellationToken cancellationToken)
    {
        var key = HabitName.KeyOf(goal.CatalogHabitName
            ?? throw new InvalidOperationException(_failure + ": a demo goal names no catalog habit."));
        var found = await WriteAsync(() => _db.Database.SqlQuery<CatalogHabitStateRow>(
                    $"SELECT [Id], [State] FROM [CatalogHabits] WITH (UPDLOCK, HOLDLOCK) WHERE [NameKey] = {key}")
                .ToListAsync(cancellationToken))
            .ConfigureAwait(false);
        return found is [var row]
               && Enum.TryParse<CatalogHabitState>(row.State, ignoreCase: false, out var state)
               && goal.AcceptedStates.Contains(state)
            ? row.Id
            : null;
    }

    /// <summary>
    /// The goal row with settings version 1 in the profile's zone and attempt 1, both from the start date; then its
    /// records, one save each in list order, so the identity column keeps that order; then its notes, one save each,
    /// so a second note under the same entry key is a primary-key fault of the database.
    /// </summary>
    private async Task WriteGoalAsync(
        AccountId owner, Guid habitId, DemoHistoryGoal goal, DemoAccountProfile profile, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();
        var demoGoal = goal.Goal;
        var version = new GoalSettingsVersion(new GoalSettings(demoGoal.Target, profile.TimeZone), demoGoal.StartDate);
        await SaveAsync(new GoalRow
        {
            Id = id,
            OwnerId = owner.Value,
            CatalogHabitId = habitId,
            StartDate = demoGoal.StartDate,
            Name = demoGoal.OwnName?.Value,
            Versions = [GoalMapping.ToRow(id, version)],
            Attempts = [new GoalAttemptRow { GoalId = id, Number = GoalAttempt.First, StartDate = demoGoal.StartDate }],
        }, cancellationToken).ConfigureAwait(false);

        foreach (var record in goal.Records)
        {
            await SaveAsync(RecordMapping.ToRow(id, record.Date, record.Value), cancellationToken).ConfigureAwait(false);
        }

        foreach (var note in goal.Notes)
        {
            await SaveAsync(RecordMapping.ToRow(id, note.EntryDate, note.Text), cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>Adds one row and saves it alone, then clears the change tracker.</summary>
    private async Task SaveAsync<TRow>(TRow row, CancellationToken cancellationToken)
        where TRow : class
    {
        _db.Add(row);
        await WriteAsync(() => _db.SaveChangesAsync(cancellationToken)).ConfigureAwait(false);
        _db.ChangeTracker.Clear();
    }

    /// <summary>A database fault becomes an exception that names the SQL error number only, with no inner exception (HP-006).</summary>
    private async Task<T> WriteAsync<T>(Func<Task<T>> write)
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

        throw new InvalidOperationException($"{_failure}: a demo goal could not be written (SQL error {number}).");
    }
}
