using HT.Application.Habits;
using HT.Domain.Habits;
using Microsoft.EntityFrameworkCore;

namespace HT.Persistence.Habits;

/// <summary>
/// The catalog habit store (research R-08). Reads are no-tracking. Writes clear the change tracker before and
/// after, as slice 1's account store does. Catalog names are public, so a unique-index violation is mapped to
/// "name taken" (research R-14).
/// </summary>
internal sealed class CatalogHabitStore : ICatalogHabitStore
{
    private readonly HtDbContext _db;

    public CatalogHabitStore(HtDbContext db) => _db = db;

    public async Task<IReadOnlyList<CatalogHabit>> ListPublishedAsync(CancellationToken cancellationToken)
    {
        var rows = await _db.CatalogHabits.AsNoTracking()
            .Where(h => h.State == CatalogHabitState.Published)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        return rows.Select(HabitMapping.ToCatalogHabit).ToList();
    }

    public async Task<IReadOnlyList<CatalogHabit>> ListAllAsync(CancellationToken cancellationToken)
    {
        var rows = await _db.CatalogHabits.AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);
        return rows.Select(HabitMapping.ToCatalogHabit).ToList();
    }

    public async Task<CatalogHabit?> FindAsync(HabitId id, CancellationToken cancellationToken)
    {
        var row = await _db.CatalogHabits.AsNoTracking()
            .SingleOrDefaultAsync(h => h.Id == id.Value, cancellationToken)
            .ConfigureAwait(false);
        return row is null ? null : HabitMapping.ToCatalogHabit(row);
    }

    public async Task<CatalogHabit?> FindPublishedAsync(HabitId id, CancellationToken cancellationToken)
    {
        var row = await _db.CatalogHabits.AsNoTracking()
            .SingleOrDefaultAsync(h => h.Id == id.Value && h.State == CatalogHabitState.Published, cancellationToken)
            .ConfigureAwait(false);
        return row is null ? null : HabitMapping.ToCatalogHabit(row);
    }

    public async Task<CreateCatalogHabitOutcome> CreateAsync(
        HabitDefinition definition, SuggestedStartingGoals suggestedStartingGoals, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(suggestedStartingGoals);
        _db.ChangeTracker.Clear();
        try
        {
            var row = new CatalogHabitRow
            {
                Id = Guid.NewGuid(),
                State = CatalogHabitState.Draft,
                SuggestedStartingGoals = HabitMapping.ToRows(suggestedStartingGoals),
            };
            HabitMapping.CopyDefinition(definition, row);
            _db.CatalogHabits.Add(row);
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return CreateCatalogHabitOutcome.Created(new HabitId(row.Id));
        }
        catch (DbUpdateException exception) when (HabitMapping.IsUniqueIndexViolation(exception))
        {
            return CreateCatalogHabitOutcome.NameTaken;
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }
    }

    public async Task<UpdateCatalogHabitOutcome> UpdateAsync(
        HabitId id, HabitDefinition definition, SuggestedStartingGoals suggestedStartingGoals, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(suggestedStartingGoals);
        _db.ChangeTracker.Clear();
        try
        {
            var row = await _db.CatalogHabits
                .SingleOrDefaultAsync(h => h.Id == id.Value, cancellationToken)
                .ConfigureAwait(false);
            if (row is null)
            {
                return UpdateCatalogHabitOutcome.NotFound;
            }

            HabitMapping.CopyDefinition(definition, row);
            ReplaceList(row, HabitMapping.ToRows(suggestedStartingGoals));
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return UpdateCatalogHabitOutcome.Saved;
        }
        catch (DbUpdateException exception) when (HabitMapping.IsUniqueIndexViolation(exception))
        {
            return UpdateCatalogHabitOutcome.NameTaken;
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }
    }

    public async Task<bool> TrySetStateAsync(HabitId id, CatalogHabitState expected, CatalogHabitState next, CancellationToken cancellationToken)
    {
        var rows = await _db.CatalogHabits
            .Where(h => h.Id == id.Value && h.State == expected)
            .ExecuteUpdateAsync(setters => setters.SetProperty(h => h.State, next), cancellationToken)
            .ConfigureAwait(false);
        return rows == 1;
    }

    public Task<bool> NameKeyExistsAsync(string key, HabitId? except, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(key);
        var query = _db.CatalogHabits.AsNoTracking().Where(h => h.NameKey == key);
        if (except is { } excluded)
        {
            query = query.Where(h => h.Id != excluded.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Replaces the owned list in place, position by position, so each (habit, position) key keeps one tracked row:
    /// existing positions are updated, missing ones added, surplus ones removed; one save writes it all.
    /// </summary>
    private static void ReplaceList(CatalogHabitRow row, List<SuggestedStartingGoalRow> replacement)
    {
        var existing = row.SuggestedStartingGoals.OrderBy(g => g.Position).ToList();
        for (var i = 0; i < existing.Count; i++)
        {
            if (existing[i].Position != i + 1)
            {
                throw new InvalidOperationException("A stored catalog habit has suggested starting goals out of position.");
            }
        }

        for (var i = 0; i < replacement.Count; i++)
        {
            if (i < existing.Count)
            {
                HabitMapping.CopyValues(replacement[i], existing[i]);
            }
            else
            {
                row.SuggestedStartingGoals.Add(replacement[i]);
            }
        }

        foreach (var surplus in existing.Skip(replacement.Count))
        {
            row.SuggestedStartingGoals.Remove(surplus);
        }
    }
}
