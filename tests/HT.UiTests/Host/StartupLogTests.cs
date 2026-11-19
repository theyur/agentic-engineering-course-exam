using HT.Application.Accounts;
using HT.Application.Operator;
using HT.Ui.Operator;
using HT.UiTests.Fakes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HT.UiTests.Host;

/// <summary>
/// HP-018: Program.cs runs the startup tasks at all, before the host serves requests. Slice 4 (research R-03,
/// contracts/host.md "Operator log"): the default time zone's warning, only for a value that is not a named time zone.
/// Slice 5 (HP-007, research R-09): demo history loading's lines for demo4 follow demo loading's.
/// </summary>
[Collection(HostCollection.Name)]
public sealed class StartupLogTests(HostFixture host)
{
    private const string DefaultTimeZoneLine = "Default time zone: not a named time zone, using UTC";

    [Fact]
    public void The_started_host_has_logged_administrator_creation_without_the_password()
    {
        using var browser = host.CreateBrowser();

        var log = host.Logs.EntriesOf(StartupTasks.LoggerCategory);

        Assert.Contains(log, e => e.Message.StartsWith("Administrator creation: ", StringComparison.Ordinal)
            && e.Message.EndsWith($"({HostFixture.AdministratorEmail})", StringComparison.Ordinal));
        Assert.All(log, e => Assert.DoesNotContain(HostFixture.AdministratorPassword, e.Message, StringComparison.Ordinal));
    }

    [Fact]
    public void The_started_host_has_logged_the_three_demo_accounts_and_then_demo4_after_administrator_creation()
    {
        using var browser = host.CreateBrowser();

        var log = host.Logs.EntriesOf(StartupTasks.LoggerCategory).Select(e => e.Message).ToList();

        // Slice 5 HP-007: demo history loading's account line, for demo4, comes after demo loading's three.
        var administrator = log.FindIndex(m => m.StartsWith("Administrator creation: ", StringComparison.Ordinal));
        Assert.True(administrator >= 0, "No administrator creation line.");
        var demoLines = log.Skip(administrator + 1).Where(m => m.StartsWith("Demo account ", StringComparison.Ordinal)).ToList();
        Assert.Equal(4, demoLines.Count);
        Assert.Equal(
            ["demo1@example.com", "demo2@example.com", "demo3@example.com", DemoHistoryService.DemoEmail],
            demoLines.Select(m => m["Demo account ".Length..m.IndexOf(':', StringComparison.Ordinal)]));
        Assert.All(log, m => Assert.DoesNotContain(HostFixture.DemoPassword, m, StringComparison.Ordinal));
    }

    [Fact]
    public void The_started_host_has_logged_demo_history_loading_after_demo_loading_with_no_fault()
    {
        using var browser = host.CreateBrowser();

        var log = host.Logs.EntriesOf(StartupTasks.LoggerCategory).Select(e => e.Message).ToList();

        // The fixture resets its database before the host starts, so every demo account is created.
        var lastDemoLoading = log.IndexOf("Demo goals demo3@example.com: demo goals added");
        Assert.True(lastDemoLoading >= 0, "No demo loading goal line for demo3.");
        Assert.Equal(
            [
                $"Demo account {DemoHistoryService.DemoEmail}: created",
                $"Demo goals {DemoHistoryService.DemoEmail}: demo goals added",
                $"Demo records {DemoHistoryService.DemoEmail}: demo records added",
            ],
            log.Skip(lastDemoLoading + 1).Where(m => m.StartsWith("Demo ", StringComparison.Ordinal)));
        Assert.DoesNotContain(log, m => m.EndsWith("failed; the app starts without it", StringComparison.Ordinal));
    }

    [Fact]
    public void Without_a_configured_default_time_zone_nothing_is_logged_about_it()
    {
        using var browser = host.CreateBrowser();

        Assert.DoesNotContain(host.Logs.EntriesOf(StartupTasks.LoggerCategory), e => e.Message.StartsWith(DefaultTimeZoneLine, StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("Mars/Olympus")]
    [InlineData("FLE Standard Time")]
    public async Task A_configured_value_that_is_not_a_named_time_zone_logs_one_warning_without_the_value(string value)
    {
        await using var zoneHost = new DefaultTimeZoneHost(host.ConnectionString, value);
        using var browser = zoneHost.CreateClient();

        var log = zoneHost.Logs.EntriesOf(StartupTasks.LoggerCategory);

        var line = Assert.Single(log, e => e.Message.StartsWith("Default time zone: ", StringComparison.Ordinal));
        Assert.Equal(DefaultTimeZoneLine, line.Message);
        Assert.Equal(LogLevel.Warning, line.Level);
        Assert.All(log, e => Assert.DoesNotContain(value, e.Message, StringComparison.Ordinal));
        Assert.Equal("UTC", zoneHost.Services.GetRequiredService<InstallationTimeZone>().Value.Id);
    }

    [Fact]
    public async Task A_configured_named_time_zone_is_used_and_logs_nothing()
    {
        await using var zoneHost = new DefaultTimeZoneHost(host.ConnectionString, "Europe/Kyiv");
        using var browser = zoneHost.CreateClient();

        Assert.DoesNotContain(zoneHost.Logs.EntriesOf(StartupTasks.LoggerCategory), e => e.Message.StartsWith("Default time zone: ", StringComparison.Ordinal));
        Assert.Equal("Europe/Kyiv", zoneHost.Services.GetRequiredService<InstallationTimeZone>().Value.Id);
    }

    /// <summary>
    /// A second host over the shared host's database with only <c>Operator:DefaultTimeZone</c> set. Without the
    /// administrator and demo settings its startup tasks stop before any database access, so the shared database is
    /// neither reset nor written.
    /// </summary>
    private sealed class DefaultTimeZoneHost(string connectionString, string defaultTimeZone) : WebApplicationFactory<Program>
    {
        public ListLoggerProvider Logs { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:HabitTracker", connectionString);
            builder.UseSetting(StartupTasks.DefaultTimeZoneKey, defaultTimeZone);
            builder.ConfigureLogging(logging => logging.AddProvider(Logs));
        }
    }
}
