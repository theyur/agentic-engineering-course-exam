using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Time.Testing;

namespace HT.IntegrationTests;

/// <summary>
/// The database of the pre-slice migration test (HP-019, slice 4 research R-14): the connection of
/// <see cref="DatabaseFixture"/> with the database name <c>HT-IT-MIGRATE</c>, so CI needs no extra variable. It
/// builds no service provider: the test composes its own, because it migrates in two steps.
/// </summary>
public sealed class MigrationDatabaseFixture
{
    private const string DatabaseName = "HT-IT-MIGRATE";

    private const string LocalDefault =
        @"Server=.\COLL1;Database=HT-IT;Integrated Security=True;TrustServerCertificate=True";

    public MigrationDatabaseFixture()
    {
        var fromEnvironment = Environment.GetEnvironmentVariable("HT_IT_CONNECTION");
        var builder = new SqlConnectionStringBuilder(string.IsNullOrWhiteSpace(fromEnvironment) ? LocalDefault : fromEnvironment)
        {
            InitialCatalog = DatabaseName,
        };
        ConnectionString = builder.ConnectionString;

        var database = new SqlConnectionStringBuilder(ConnectionString).InitialCatalog;
        if (string.Equals(database, "HT", StringComparison.OrdinalIgnoreCase)
            || string.Equals(database, "HT-AC", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Integration tests never use the HT or HT-AC database.");
        }
    }

    public string ConnectionString { get; }

    public FakeTimeProvider Clock { get; } = new(new DateTimeOffset(2026, 11, 18, 10, 0, 0, TimeSpan.Zero));
}

/// <summary>The tests on <c>HT-IT-MIGRATE</c> drop and migrate the database, so they run one at a time.</summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class MigrationDatabaseCollection : ICollectionFixture<MigrationDatabaseFixture>
{
    public const string Name = "MigrationDatabase";
}
