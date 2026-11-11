using System.Reflection;
using HT.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HT.UiTests.Host;

/// <summary>
/// Checks the database the host's own context uses, not the configuration (research R-16, "Plan discrepancies"
/// 2). A setting that reached the configuration too late for Program.cs still fails here.
/// </summary>
[Collection(HostCollection.Name)]
public sealed class HostDatabaseGuardTests(HostFixture host)
{
    [Fact]
    public async Task The_host_uses_the_host_test_database_and_never_HT()
    {
        await using var scope = host.Services.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IUserStore<HtUser>>();

        var contextProperty = store.GetType().GetProperty("Context", BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(contextProperty);
        var context = Assert.IsType<DbContext>(contextProperty.GetValue(store), exactMatch: false);
        var database = context.Database.GetDbConnection().Database;

        Assert.Equal(host.DatabaseName, database);
        Assert.NotEqual("HT", database, StringComparer.OrdinalIgnoreCase);
    }
}
