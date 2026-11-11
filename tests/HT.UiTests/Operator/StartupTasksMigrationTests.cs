namespace HT.UiTests.Operator;

/// <summary>HP-022: the host applies pending migrations at start in Development only.</summary>
[Collection(StartupTasksCollection.Name)]
public sealed class StartupTasksMigrationTests
{
    private const string MigrationsLine = "Schema migrations applied:";

    [Fact]
    public async Task Development_applies_the_pending_migrations()
    {
        var log = await StartupTasksHarness.RunAsync("Development", new Dictionary<string, string?>());

        var line = Assert.Single(log, e => e.Message.StartsWith(MigrationsLine, StringComparison.Ordinal));
        var count = int.Parse(line.Message[MigrationsLine.Length..].Trim(), System.Globalization.CultureInfo.InvariantCulture);
        Assert.True(count > 0);
        Assert.True(await Host.HostClient.TableExistsAsync(StartupTasksHarness.ConnectionString, "AspNetUsers"));
    }

    [Fact]
    public async Task A_second_Development_start_applies_none()
    {
        await StartupTasksHarness.RunAsync("Development", new Dictionary<string, string?>());

        var log = await StartupTasksHarness.RunWithoutDropAsync("Development", new Dictionary<string, string?>());

        Assert.Contains(log, e => e.Message == MigrationsLine + " 0");
    }

    [Fact]
    public async Task Testing_applies_no_migration()
    {
        var log = await StartupTasksHarness.RunAsync("Testing", new Dictionary<string, string?>());

        Assert.DoesNotContain(log, e => e.Message.StartsWith(MigrationsLine, StringComparison.Ordinal));
        Assert.False(await StartupTasksHarness.DatabaseExistsAsync());
    }
}
