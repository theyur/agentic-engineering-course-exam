using System.Data.Common;
using HT.Application.Habits;
using HT.Domain.Accounts;
using HT.Domain.Habits;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace HT.Persistence.Habits;

/// <summary>
/// The custom habit store (research R-08, R-14). Every statement is scoped by owner and identifier together. A
/// taken name never raises a database error: create and edit are single conditional statements that write no row
/// when the owner already has the name key, under key-range locks (UPDLOCK, HOLDLOCK), so a lost race is "name
/// taken" too. Any database fault of a write becomes an exception that names the SQL error number only, never a
/// value, with no inner exception (HP-015).
/// </summary>
internal sealed class CustomHabitStore : ICustomHabitStore
{
    private readonly HtDbContext _db;

    public CustomHabitStore(HtDbContext db) => _db = db;

    public async Task<IReadOnlyList<CustomHabit>> ListAsync(AccountId owner, CancellationToken cancellationToken)
    {
        var rows = await _db.CustomHabits.AsNoTracking()
            .Where(h => h.OwnerId == owner.Value)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        return rows.Select(HabitMapping.ToCustomHabit).ToList();
    }

    public async Task<CustomHabit?> FindAsync(AccountId owner, HabitId id, CancellationToken cancellationToken)
    {
        var row = await _db.CustomHabits.AsNoTracking()
            .SingleOrDefaultAsync(h => h.OwnerId == owner.Value && h.Id == id.Value, cancellationToken)
            .ConfigureAwait(false);
        return row is null ? null : HabitMapping.ToCustomHabit(row);
    }

    public async Task<CreateCustomHabitOutcome> CreateAsync(AccountId owner, HabitDefinition definition, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(definition);
        var id = Guid.NewGuid();
        var rows = await InsertAsync(_db, id, owner, definition, cancellationToken).ConfigureAwait(false);
        return rows == 1 ? CreateCustomHabitOutcome.Created(new HabitId(id)) : CreateCustomHabitOutcome.NameTaken;
    }

    /// <summary>
    /// The conditional insert of research R-14, in the context's current transaction if it has one: one row, or none
    /// when the owner already has the name key. A database fault names the SQL error number only (HP-015). Shared with
    /// the demo data store (research R-09).
    /// </summary>
    internal static Task<int> InsertAsync(HtDbContext db, Guid id, AccountId owner, HabitDefinition definition, CancellationToken cancellationToken) =>
        WriteAsync(() => db.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO [CustomHabits] ([Id], [OwnerId], [Name], [NameKey], [Description], [Category], [Intent], [Icon])
            SELECT {id}, {owner.Value}, {definition.Name.Value}, {definition.Name.Key}, {definition.Description},
                   {definition.Category.ToString()}, {definition.Intent.ToString()}, {definition.Icon.Name}
            WHERE NOT EXISTS (SELECT 1 FROM [CustomHabits] WITH (UPDLOCK, HOLDLOCK)
                              WHERE [OwnerId] = {owner.Value} AND [NameKey] = {definition.Name.Key})
            """,
            cancellationToken));

    public async Task<UpdateCustomHabitOutcome> UpdateAsync(AccountId owner, HabitId id, HabitDefinition definition, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(definition);
        var rows = await WriteAsync(() => _db.Database.ExecuteSqlInterpolatedAsync(
            $"""
            UPDATE [CustomHabits]
            SET [Name] = {definition.Name.Value}, [NameKey] = {definition.Name.Key}, [Description] = {definition.Description},
                [Category] = {definition.Category.ToString()}, [Intent] = {definition.Intent.ToString()}, [Icon] = {definition.Icon.Name}
            WHERE [Id] = {id.Value} AND [OwnerId] = {owner.Value}
              AND NOT EXISTS (SELECT 1 FROM [CustomHabits] WITH (UPDLOCK, HOLDLOCK)
                              WHERE [OwnerId] = {owner.Value} AND [NameKey] = {definition.Name.Key} AND [Id] <> {id.Value})
            """,
            cancellationToken)).ConfigureAwait(false);
        if (rows == 1)
        {
            return UpdateCustomHabitOutcome.Saved;
        }

        var exists = await _db.CustomHabits.AsNoTracking()
            .AnyAsync(h => h.OwnerId == owner.Value && h.Id == id.Value, cancellationToken)
            .ConfigureAwait(false);
        return exists ? UpdateCustomHabitOutcome.NameTaken : UpdateCustomHabitOutcome.NotFound;
    }

    public async Task<DeleteCustomHabitOutcome> DeleteAsync(AccountId owner, HabitId id, CancellationToken cancellationToken)
    {
        var rows = await WriteAsync(() => _db.CustomHabits
            .Where(h => h.OwnerId == owner.Value && h.Id == id.Value)
            .ExecuteDeleteAsync(cancellationToken)).ConfigureAwait(false);
        return rows == 1 ? DeleteCustomHabitOutcome.Deleted : DeleteCustomHabitOutcome.NotFound;
    }

    public Task<bool> NameKeyExistsAsync(AccountId owner, string key, HabitId? except, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(key);
        var query = _db.CustomHabits.AsNoTracking().Where(h => h.OwnerId == owner.Value && h.NameKey == key);
        if (except is { } excluded)
        {
            query = query.Where(h => h.Id != excluded.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    /// <summary>Runs a write; a database fault becomes an exception with the SQL error number only (HP-015).</summary>
    private static async Task<int> WriteAsync(Func<Task<int>> write)
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

        throw new InvalidOperationException($"Saving a custom habit failed (SQL error {number}).");
    }
}
