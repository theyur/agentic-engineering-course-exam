using HT.Application;
using HT.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace HT.IntegrationTests;

/// <summary>
/// One database per run (ADR 0006): <c>HT_IT_CONNECTION</c>, or the local <c>HT-IT</c> default. Reset once,
/// then the application is built the way the acceptance tests build it, with a fake clock registered last.
/// </summary>
public sealed class DatabaseFixture : IAsyncLifetime
{
    private const string LocalDefault =
        @"Server=.\COLL1;Database=HT-IT;Integrated Security=True;TrustServerCertificate=True";

    public DatabaseFixture()
    {
        var fromEnvironment = Environment.GetEnvironmentVariable("HT_IT_CONNECTION");
        ConnectionString = string.IsNullOrWhiteSpace(fromEnvironment) ? LocalDefault : fromEnvironment;

        var database = new SqlConnectionStringBuilder(ConnectionString).InitialCatalog;
        if (string.Equals(database, "HT", StringComparison.OrdinalIgnoreCase)
            || string.Equals(database, "HT-AC", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Integration tests never use the HT or HT-AC database.");
        }
    }

    public string ConnectionString { get; }

    public FakeTimeProvider Clock { get; } = new(new DateTimeOffset(2026, 11, 11, 9, 0, 0, TimeSpan.Zero));

    public ServiceProvider Services { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await HtDatabase.ResetAsync(ConnectionString, CancellationToken.None);

        var services = new ServiceCollection()
            .AddLogging()
            .AddHtApplication()
            .AddHtPersistence(ConnectionString);
        services.AddSingleton<TimeProvider>(Clock);
        Services = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
    }

    public async Task DisposeAsync()
    {
        if (Services is not null)
        {
            await Services.DisposeAsync();
        }
    }

    /// <summary>A new service scope, as one request or one acceptance scenario would use.</summary>
    public AsyncServiceScope CreateScope() => Services.CreateAsyncScope();
}

/// <summary>Every integration test shares the database and runs one at a time (ADR 0006).</summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
    public const string Name = "Database";
}
