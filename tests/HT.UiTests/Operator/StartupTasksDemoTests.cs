using HT.Application.Operator;
using HT.UiTests.Fakes;
using Microsoft.Extensions.Logging;

namespace HT.UiTests.Operator;

/// <summary>
/// Slice 1 HP-021: demo loading runs at every start, right after administrator creation, and never logs the password.
/// Slice 2 HP-017: its custom-habit and catalog results are logged too, never with a custom-habit name.
/// </summary>
[Collection(StartupTasksCollection.Name)]
public sealed class StartupTasksDemoTests
{
    private const string DemoPassword = "startup-demo-7";
    private const string AdministratorLine = "Administrator creation:";

    private static readonly string[] Emails = ["demo1@example.com", "demo2@example.com", "demo3@example.com"];

    [Fact]
    public async Task A_demo_password_logs_created_for_each_demo_email_after_administrator_creation()
    {
        var log = await StartupTasksHarness.RunAsync("Development", Configured(DemoPassword));

        // HP-017: each account line is followed by its custom-habit line; the catalog line comes once, last.
        AssertDemoLines(
            log,
            [
                .. Emails.SelectMany(email => new[]
                {
                    $"Demo account {email}: created",
                    $"Demo custom habits {email}: demo custom habits added",
                }),
                "Demo catalog: seed catalog loaded",
            ]);
    }

    [Fact]
    public async Task A_second_start_logs_already_exists_for_each_demo_email()
    {
        await StartupTasksHarness.RunAsync("Development", Configured(DemoPassword));

        var log = await StartupTasksHarness.RunWithoutDropAsync("Development", Configured(DemoPassword));

        // HP-017: an existing account has no custom-habit line; the catalog is not empty any more.
        AssertDemoLines(
            log,
            [.. Emails.Select(email => $"Demo account {email}: already exists"), "Demo catalog: catalog not empty"]);
    }

    [Fact]
    public async Task No_log_entry_contains_a_demo_custom_habit_name()
    {
        var first = await StartupTasksHarness.RunAsync("Development", Configured(DemoPassword));
        var second = await StartupTasksHarness.RunWithoutDropAsync("Development", Configured(DemoPassword));
        var names = Emails.SelectMany(DemoCustomHabits.For).Select(h => h.Name.Value).ToList();

        Assert.All(first.Concat(second), e => Assert.DoesNotContain(
            names, name => (e.Message + e.Exception).Contains(name, StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public async Task No_demo_password_logs_no_demo_password_configured()
    {
        var log = await StartupTasksHarness.RunAsync("Development", new Dictionary<string, string?>());

        var line = Assert.Single(log, e => e.Message.StartsWith("Demo ", StringComparison.Ordinal));
        Assert.Equal(LogLevel.Information, line.Level);
        Assert.Equal("Demo loading: no demo password configured", line.Message);
    }

    [Fact]
    public async Task A_password_that_fails_the_rule_logs_its_refusal_and_creates_nothing()
    {
        var log = await StartupTasksHarness.RunAsync("Development", Configured("startup-demo"));

        Assert.Contains(log, e => e.Message == "Demo loading: demo password does not meet the password rule");
        Assert.DoesNotContain(log, e => e.Message.StartsWith("Demo account ", StringComparison.Ordinal));
        Assert.DoesNotContain(log, e => e.Message.StartsWith("Demo custom habits ", StringComparison.Ordinal));
        Assert.DoesNotContain(log, e => e.Message.StartsWith("Demo catalog", StringComparison.Ordinal));
        foreach (var email in Emails)
        {
            Assert.Equal(0, await Host.HostClient.CountUsersAsync(StartupTasksHarness.ConnectionString, email));
        }
    }

    [Fact]
    public async Task No_log_entry_contains_the_demo_password()
    {
        var first = await StartupTasksHarness.RunAsync("Development", Configured(DemoPassword));
        var second = await StartupTasksHarness.RunWithoutDropAsync("Development", Configured(DemoPassword));
        var refused = await StartupTasksHarness.RunWithoutDropAsync("Development", Configured("startup-demo"));

        Assert.All(first.Concat(second).Concat(refused), e =>
        {
            Assert.DoesNotContain(DemoPassword, e.Message, StringComparison.Ordinal);
            Assert.DoesNotContain("startup-demo", e.Message, StringComparison.Ordinal);
            Assert.DoesNotContain("startup-demo", e.Exception?.ToString() ?? string.Empty, StringComparison.Ordinal);
        });
    }

    [Fact]
    public async Task A_fault_in_demo_loading_is_logged_as_an_error_and_the_start_goes_on()
    {
        const string unreachable =
            "Server=ht-no-such-server.invalid;Database=HT-UI-MIGRATE;Integrated Security=True;TrustServerCertificate=True;Connect Timeout=2";

        // Testing applies no migration and, with no administrator values, administrator creation never touches
        // the store, so the demo step is the one that meets the unreachable database.
        var log = await StartupTasksHarness.RunWithoutDropAsync("Testing", Configured(DemoPassword), unreachable);

        Assert.Contains(log, e => e.Message == "Administrator creation: no administrator configured");
        var error = Assert.Single(log, e => e.Message == "Demo loading failed; the app starts without it");
        Assert.Equal(LogLevel.Error, error.Level);
        Assert.NotNull(error.Exception);
        Assert.All(log, e => Assert.DoesNotContain(DemoPassword, e.Message + e.Exception, StringComparison.Ordinal));
    }

    private static void AssertDemoLines(IReadOnlyList<LogEntry> log, IReadOnlyList<string> expected)
    {
        var messages = log.Select(e => e.Message).ToList();
        var administrator = messages.FindIndex(m => m.StartsWith(AdministratorLine, StringComparison.Ordinal));
        Assert.True(administrator >= 0, "No administrator creation line.");

        var demoLines = log.Skip(administrator + 1).Where(e => e.Message.StartsWith("Demo ", StringComparison.Ordinal)).ToList();
        Assert.Equal(expected, demoLines.Select(e => e.Message));
        Assert.All(demoLines, e => Assert.Equal(LogLevel.Information, e.Level));
        Assert.DoesNotContain(log.Take(administrator), e => e.Message.StartsWith("Demo ", StringComparison.Ordinal));
    }

    private static Dictionary<string, string?> Configured(string demoPassword) => new()
    {
        ["Operator:DemoPassword"] = demoPassword,
    };
}
