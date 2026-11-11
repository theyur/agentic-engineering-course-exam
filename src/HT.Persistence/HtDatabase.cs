using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HT.Persistence;

/// <summary>Database entry points (research R-12).</summary>
public static class HtDatabase
{
    /// <summary>Drops the database and applies all migrations. Used only by tests (ADR 0003, ADR 0006).</summary>
    public static async Task ResetAsync(string connectionString, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        var options = new DbContextOptionsBuilder<HtDbContext>().UseSqlServer(connectionString).Options;
        await using var db = new HtDbContext(options);
        await db.Database.EnsureDeletedAsync(cancellationToken).ConfigureAwait(false);
        await db.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Applies the pending migrations and returns how many there were. Used only by the host in Development
    /// (HP-022).
    /// </summary>
    public static async Task<int> MigrateAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(services);

        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<HtDbContext>();
        var pending = (await db.Database.GetPendingMigrationsAsync(cancellationToken).ConfigureAwait(false)).Count();
        if (pending > 0)
        {
            await db.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
        }

        return pending;
    }
}
