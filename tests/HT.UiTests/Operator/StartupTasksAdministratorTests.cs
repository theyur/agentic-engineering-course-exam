using Microsoft.Extensions.Logging;

namespace HT.UiTests.Operator;

/// <summary>HP-018: administrator creation runs at every start, logs its result and never a password.</summary>
[Collection(StartupTasksCollection.Name)]
public sealed class StartupTasksAdministratorTests
{
    private const string Email = "startup.admin@example.com";
    private const string Password = "startup-admin-7";

    [Fact]
    public async Task Configured_values_log_administrator_created_with_the_email()
    {
        var log = await StartupTasksHarness.RunAsync("Development", Configured(Email, Password));

        Assert.Contains(log, e => e.Level == LogLevel.Information
            && e.Message == $"Administrator creation: administrator created ({Email})");
    }

    [Fact]
    public async Task A_second_start_logs_administrator_already_exists()
    {
        await StartupTasksHarness.RunAsync("Development", Configured(Email, Password));

        var log = await StartupTasksHarness.RunWithoutDropAsync("Development", Configured(Email, "other-admin-7"));

        Assert.Contains(log, e => e.Message == $"Administrator creation: administrator already exists ({Email})");
    }

    [Fact]
    public async Task No_configured_values_log_no_administrator_configured_without_parentheses()
    {
        var log = await StartupTasksHarness.RunAsync("Development", new Dictionary<string, string?>());

        Assert.Contains(log, e => e.Message == "Administrator creation: no administrator configured");
    }

    [Fact]
    public async Task No_log_entry_contains_the_configured_password()
    {
        var first = await StartupTasksHarness.RunAsync("Development", Configured(Email, Password));
        var second = await StartupTasksHarness.RunWithoutDropAsync("Development", Configured(Email, Password));
        var refused = await StartupTasksHarness.RunWithoutDropAsync("Development", Configured("other.admin@example.com", "nodigits-here"));

        Assert.All(first.Concat(second).Concat(refused), e =>
        {
            Assert.DoesNotContain(Password, e.Message, StringComparison.Ordinal);
            Assert.DoesNotContain("nodigits-here", e.Message, StringComparison.Ordinal);
            Assert.DoesNotContain(Password, e.Exception?.ToString() ?? string.Empty, StringComparison.Ordinal);
        });
        Assert.Contains(refused, e => e.Message == "Administrator creation: configured password does not meet the password rule (other.admin@example.com)");
    }

    [Fact]
    public async Task An_unreachable_database_is_logged_as_an_error_and_the_start_goes_on()
    {
        const string unreachable =
            "Server=ht-no-such-server.invalid;Database=HT-UI-MIGRATE;Integrated Security=True;TrustServerCertificate=True;Connect Timeout=2";

        var log = await StartupTasksHarness.RunWithoutDropAsync("Development", Configured(Email, Password), unreachable);

        var error = Assert.Single(log, e => e.Message == "Administrator creation failed; the app starts without it");
        Assert.Equal(LogLevel.Error, error.Level);
        Assert.NotNull(error.Exception);
        Assert.All(log, e => Assert.DoesNotContain(Password, e.Message + e.Exception, StringComparison.Ordinal));
    }

    private static Dictionary<string, string?> Configured(string email, string password) => new()
    {
        ["Operator:AdministratorEmail"] = email,
        ["Operator:AdministratorPassword"] = password,
    };
}
