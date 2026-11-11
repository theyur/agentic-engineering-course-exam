using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HT.Persistence;

/// <summary>
/// Builds the context for <c>dotnet ef</c> (research R-15). Uses <c>HT_DESIGN_CONNECTION</c>, or the local
/// <c>HT</c> database when it is not set. Adding a migration does not connect.
/// </summary>
internal sealed class HtDbContextFactory : IDesignTimeDbContextFactory<HtDbContext>
{
    private const string LocalDefault =
        @"Server=.\COLL1;Database=HT;Integrated Security=True;TrustServerCertificate=True";

    public HtDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("HT_DESIGN_CONNECTION");
        var options = new DbContextOptionsBuilder<HtDbContext>()
            .UseSqlServer(string.IsNullOrWhiteSpace(connectionString) ? LocalDefault : connectionString)
            .Options;
        return new HtDbContext(options);
    }
}
