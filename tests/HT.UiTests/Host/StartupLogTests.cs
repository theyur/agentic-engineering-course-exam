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
}
