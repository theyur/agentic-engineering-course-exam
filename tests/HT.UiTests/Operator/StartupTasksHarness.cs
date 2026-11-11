using HT.Application;
using HT.Persistence;
using HT.Ui.Operator;
using HT.UiTests.Fakes;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HT.UiTests.Operator;

/// <summary>The <c>StartupTasks</c> tests share one database, so they run one at a time.</summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class StartupTasksCollection
{
    public const string Name = "StartupTasks";
}

/// <summary>
/// Runs <see cref="StartupTasks.RunAsync"/> without a host (research R-16), against <c>HT-UI-MIGRATE</c>: the
/// host-test connection string with its database name replaced, so CI needs no extra variable.
/// </summary>
public static class StartupTasksHarness
{
    public const string DatabaseName = "HT-UI-MIGRATE";

    private const string LocalDefault =
        @"Server=.\COLL1;Database=HT-UI;Integrated Security=True;TrustServerCertificate=True";

    /// <summary>The connection string of <c>HT-UI-MIGRATE</c>.</summary>
    public static string ConnectionString { get; } = BuildConnectionString();

    /// <summary>Drops <c>HT-UI-MIGRATE</c>, then runs the startup tasks and returns the operator log.</summary>
    public static async Task<IReadOnlyList<LogEntry>> RunAsync(
        string environmentName, IDictionary<string, string?> operatorValues, string? connectionString = null)
    {
        await DropDatabaseAsync();
        return await RunWithoutDropAsync(environmentName, operatorValues, connectionString);
    }

    /// <summary>Runs the startup tasks on the database as it is (for a second start).</summary>
    public static async Task<IReadOnlyList<LogEntry>> RunWithoutDropAsync(
        string environmentName, IDictionary<string, string?> operatorValues, string? connectionString = null)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(operatorValues).Build();
        var services = new ServiceCollection()
            .AddLogging()
            .AddHtApplication()
            .AddHtPersistence(connectionString ?? ConnectionString)
            .AddSingleton<IConfiguration>(configuration);

        await using var provider = services.BuildServiceProvider();
        var logger = new ListLogger(StartupTasks.LoggerCategory);
        await StartupTasks.RunAsync(provider, new FakeHostEnvironment(environmentName), logger);
        return logger.Entries;
    }

    /// <summary>True when <c>HT-UI-MIGRATE</c> exists.</summary>
    public static async Task<bool> DatabaseExistsAsync()
    {
        await using var connection = new SqlConnection(MasterConnectionString());
        await connection.OpenAsync();
        await using var command = new SqlCommand($"SELECT DB_ID(N'{DatabaseName}')", connection);
        return await command.ExecuteScalarAsync() is not (null or DBNull);
    }

    private static async Task DropDatabaseAsync()
    {
        // Earlier runs in this process still pool connections to the database.
        SqlConnection.ClearAllPools();
        await using var connection = new SqlConnection(MasterConnectionString());
        await connection.OpenAsync();
        await using var command = new SqlCommand(
            $"IF DB_ID(N'{DatabaseName}') IS NOT NULL BEGIN ALTER DATABASE [{DatabaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{DatabaseName}]; END",
            connection);
        await command.ExecuteNonQueryAsync();
    }

    private static string MasterConnectionString() =>
        new SqlConnectionStringBuilder(ConnectionString) { InitialCatalog = "master" }.ConnectionString;

    private static string BuildConnectionString()
    {
        var fromEnvironment = Environment.GetEnvironmentVariable("HT_UI_CONNECTION");
        var hostTests = string.IsNullOrWhiteSpace(fromEnvironment) ? LocalDefault : fromEnvironment;
        return new SqlConnectionStringBuilder(hostTests) { InitialCatalog = DatabaseName }.ConnectionString;
    }
}
