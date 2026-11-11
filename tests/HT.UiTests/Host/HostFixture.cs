using HT.Persistence;
using HT.UiTests.Fakes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;

namespace HT.UiTests.Host;

/// <summary>
/// One host per run (research R-16), in the <c>Testing</c> environment, on the host-test database
/// (<c>HT_UI_CONNECTION</c>, or the local <c>HT-UI</c> default). Every value is a host setting, so the top-level
/// code of Program.cs sees it. The database is reset once, before the host starts.
/// </summary>
public sealed class HostFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string AdministratorEmail = "host-admin@example.com";
    public const string AdministratorPassword = "throwaway-admin-7";
    public const string DemoPassword = "throwaway-demo-7";

    private const string LocalDefault =
        @"Server=.\COLL1;Database=HT-UI;Integrated Security=True;TrustServerCertificate=True";

    public HostFixture()
    {
        var fromEnvironment = Environment.GetEnvironmentVariable("HT_UI_CONNECTION");
        ConnectionString = string.IsNullOrWhiteSpace(fromEnvironment) ? LocalDefault : fromEnvironment;
        DatabaseName = new SqlConnectionStringBuilder(ConnectionString).InitialCatalog;
    }

    public string ConnectionString { get; }

    /// <summary>The database name the host must use.</summary>
    public string DatabaseName { get; }

    public ListLoggerProvider Logs { get; } = new();

    /// <summary>
    /// The host's clock: the use cases and the cookie handler read it. It starts at the real time and only moves
    /// when a test advances it.
    /// </summary>
    public FakeTimeProvider Clock { get; } = new(DateTimeOffset.UtcNow);

    public async Task InitializeAsync() => await HtDatabase.ResetAsync(ConnectionString, CancellationToken.None);

    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();

    /// <summary>A client over HTTPS (the cookie is Secure) that keeps cookies and does not follow redirects.</summary>
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
        builder.UseSetting("Operator:AdministratorEmail", AdministratorEmail);
        builder.UseSetting("Operator:AdministratorPassword", AdministratorPassword);
        builder.UseSetting("Operator:DemoPassword", DemoPassword);
        builder.ConfigureLogging(logging => logging.AddProvider(Logs));
        builder.ConfigureServices(services => services.AddSingleton<TimeProvider>(Clock));
    }
}

/// <summary>Host tests share one host and run one at a time.</summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class HostCollection : ICollectionFixture<HostFixture>
{
    public const string Name = "Host";
}
