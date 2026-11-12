using HT.Ui.Operator;

namespace HT.UiTests.Host;

/// <summary>HP-018: Program.cs runs the startup tasks at all, before the host serves requests.</summary>
[Collection(HostCollection.Name)]
public sealed class StartupLogTests(HostFixture host)
{
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
    public void The_started_host_has_logged_the_three_demo_accounts_after_administrator_creation()
    {
        using var browser = host.CreateBrowser();

        var log = host.Logs.EntriesOf(StartupTasks.LoggerCategory).Select(e => e.Message).ToList();

        var administrator = log.FindIndex(m => m.StartsWith("Administrator creation: ", StringComparison.Ordinal));
        Assert.True(administrator >= 0, "No administrator creation line.");
        var demoLines = log.Skip(administrator + 1).Where(m => m.StartsWith("Demo account ", StringComparison.Ordinal)).ToList();
        Assert.Equal(3, demoLines.Count);
        Assert.Equal(
            ["demo1@example.com", "demo2@example.com", "demo3@example.com"],
            demoLines.Select(m => m["Demo account ".Length..m.IndexOf(':', StringComparison.Ordinal)]));
        Assert.All(log, m => Assert.DoesNotContain(HostFixture.DemoPassword, m, StringComparison.Ordinal));
    }
}
