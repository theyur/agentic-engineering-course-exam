using HT.Application;
using HT.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace HT.IntegrationTests;

/// <summary>
/// The database of the tests that need an empty catalog (research R-13, "Starting states"): the connection of
/// <see cref="DatabaseFixture"/> with the database name <c>HT-IT-SEED</c>, so CI needs no extra variable. Each
/// test resets it with <see cref="ResetAndBuildAsync"/> at its start and disposes the provider it returns.
/// </summary>
public sealed class SeedDatabaseFixture
{
    private const string DatabaseName = "HT-IT-SEED";

    private const string LocalDefault =
        @"Server=.\COLL1;Database=HT-IT;Integrated Security=True;TrustServerCertificate=True";

    public SeedDatabaseFixture()
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

    public FakeTimeProvider Clock { get; } = new(new DateTimeOffset(2026, 11, 14, 9, 0, 0, TimeSpan.Zero));

    /// <summary>Drops and migrates the database, then builds the application over it as <see cref="DatabaseFixture"/> does.</summary>
    public async Task<ServiceProvider> ResetAndBuildAsync()
    {
        await HtDatabase.ResetAsync(ConnectionString, CancellationToken.None);

        var services = new ServiceCollection()
            .AddLogging()
            .AddHtApplication()
            .AddHtPersistence(ConnectionString);
        services.AddSingleton<TimeProvider>(Clock);
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
    }
}

/// <summary>The tests on <c>HT-IT-SEED</c> reset the database, so they run one at a time.</summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class SeedDatabaseCollection : ICollectionFixture<SeedDatabaseFixture>
{
    public const string Name = "SeedDatabase";
}
