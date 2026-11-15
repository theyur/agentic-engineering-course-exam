using HT.Persistence;
using HT.UiTests.Fakes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace HT.UiTests.Host;

/// <summary>
/// A host of its own, on a database of its own, for host tests that need settings or a database state the shared
/// <see cref="HostFixture"/> cannot have (research R-11): <c>HT-UI-SMOKE</c> for the in-process smoke check, whose
/// cases each start from a fresh seed, and <c>HT-UI-KEYS</c> for the session-key tests, which run two hosts in turn
/// over one database. ADR 0006 gives host tests the single database <c>HT-UI</c>; these databases depart from it the
/// way <c>HT-UI-MIGRATE</c> does: the connection string is the host-test one (<c>HT_UI_CONNECTION</c>, or the local
/// default) with only the database name replaced, on the same server, so CI needs no extra variable. Like the shared
/// host it runs in environment <c>Testing</c> (no User Secrets, no migrations at start) with every value set through
/// <c>UseSetting</c>; the schema comes from <see cref="ResetAsync"/> before the host is built.
/// </summary>
public sealed class DerivedDatabaseHost : WebApplicationFactory<Program>
{
    private readonly IReadOnlyDictionary<string, string?> _settings;

    /// <param name="databaseName">The derived database, such as <c>HT-UI-SMOKE</c> or <c>HT-UI-KEYS</c>.</param>
    /// <param name="settings">Host settings, such as the <c>Operator:*</c> values, each set through <c>UseSetting</c>.</param>
    public DerivedDatabaseHost(string databaseName, IReadOnlyDictionary<string, string?> settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ConnectionString = ConnectionStringFor(databaseName);
        DatabaseName = databaseName;
        _settings = settings;
    }

    /// <summary>The derived database's name.</summary>
    public string DatabaseName { get; }

    /// <summary>The connection string of the derived database.</summary>
    public string ConnectionString { get; }

    public ListLoggerProvider Logs { get; } = new();

    /// <summary>
    /// The host-test connection string with the database name replaced. The app's and the other test runs'
    /// databases are refused, and so is the shared host's own database, which a reset here would pull from under it.
    /// </summary>
    public static string ConnectionStringFor(string databaseName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseName);
        HostFixture.EnsureResettable(databaseName);

        var fromEnvironment = Environment.GetEnvironmentVariable("HT_UI_CONNECTION");
        var hostTests = new SqlConnectionStringBuilder(
            string.IsNullOrWhiteSpace(fromEnvironment) ? HostFixture.LocalDefault : fromEnvironment);
        if (string.Equals(hostTests.InitialCatalog, databaseName, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("A derived host never uses the shared host's database.");
        }

        hostTests.InitialCatalog = databaseName;
        return hostTests.ConnectionString;
    }

    /// <summary>Drops the derived database and applies all migrations, before a host over it is built.</summary>
    public static Task ResetAsync(string databaseName) =>
        HtDatabase.ResetAsync(ConnectionStringFor(databaseName), CancellationToken.None);

    /// <summary>
    /// A new client over HTTPS (the cookie is Secure) that keeps its own cookies and does not follow redirects, as
    /// <see cref="HostFixture.CreateBrowser"/>.
    /// </summary>
    public HttpClient CreateBrowser() => CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost"),
        AllowAutoRedirect = false,
        HandleCookies = true,
    });

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:HabitTracker", ConnectionString);
        foreach (var (key, value) in _settings)
        {
            builder.UseSetting(key, value);
        }

        builder.ConfigureLogging(logging => logging.AddProvider(Logs));
    }
}

/// <summary>Hosts over derived databases run one at a time: their cases reset and share those databases.</summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class DerivedDatabaseHostCollection
{
    public const string Name = "DerivedDatabaseHosts";
}
