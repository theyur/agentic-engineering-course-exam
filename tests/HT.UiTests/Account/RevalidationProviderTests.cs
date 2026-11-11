using System.Reflection;
using System.Security.Claims;
using HT.Application.Accounts;
using HT.Domain.Accounts;
using HT.Persistence.Identity;
using HT.Ui.Account;
using HT.UiTests.Host;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace HT.UiTests.Account;

/// <summary>HP-007, research R-09: an open circuit is revalidated against the security stamp every 30 minutes.</summary>
[Collection(HostCollection.Name)]
public sealed class RevalidationProviderTests(HostFixture host)
{
    private const string Password = "walking42";

    [Fact]
    public async Task An_unchanged_stamp_is_current()
    {
        var account = await host.CreateAccountAsync("revalidate.same@example.com", Password);
        var principal = await PrincipalOfAsync(account.Id);

        Assert.True(await IsCurrentAsync(principal));
    }

    [Fact]
    public async Task After_a_password_change_the_stamp_is_not_current()
    {
        var account = await host.CreateAccountAsync("revalidate.changed@example.com", Password);
        var principal = await PrincipalOfAsync(account.Id);
        await using (var scope = host.Services.CreateAsyncScope())
        {
            Assert.True(await scope.ServiceProvider.GetRequiredService<IAccountStore>()
                .ChangePasswordAsync(account.Id, Password, "running42", CancellationToken.None));
        }

        Assert.False(await IsCurrentAsync(principal));
    }

    [Fact]
    public async Task After_the_account_is_deleted_the_stamp_is_not_current()
    {
        var account = await host.CreateAccountAsync("revalidate.deleted@example.com", Password);
        var principal = await PrincipalOfAsync(account.Id);

        await HostClient.DeleteUserAsync(host.ConnectionString, "revalidate.deleted@example.com");

        Assert.False(await IsCurrentAsync(principal));
    }

    [Fact]
    public async Task A_principal_without_a_stamp_is_not_current()
    {
        var account = await host.CreateAccountAsync("revalidate.no-stamp@example.com", Password);
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, account.Id.Value.ToString())], IdentityConstants.ApplicationScheme));

        Assert.False(await IsCurrentAsync(principal));
    }

    [Fact]
    public void The_host_registers_it_with_a_30_minute_interval()
    {
        using var scope = host.Services.CreateScope();

        var provider = Assert.IsType<IdentityRevalidatingAuthenticationStateProvider>(
            scope.ServiceProvider.GetRequiredService<AuthenticationStateProvider>());

        // The override stays protected (an override cannot widen its access), so read it by reflection.
        var interval = typeof(IdentityRevalidatingAuthenticationStateProvider)
            .GetProperty("RevalidationInterval", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(provider);
        Assert.Equal(TimeSpan.FromMinutes(30), interval);
    }

    private async Task<ClaimsPrincipal> PrincipalOfAsync(AccountId id)
    {
        await using var scope = host.Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<HtUser>>();
        var user = await users.FindByIdAsync(id.Value.ToString())
            ?? throw new InvalidOperationException("The account was not created.");
        return await scope.ServiceProvider.GetRequiredService<IUserClaimsPrincipalFactory<HtUser>>().CreateAsync(user);
    }

    private async Task<bool> IsCurrentAsync(ClaimsPrincipal principal)
    {
        await using var scope = host.Services.CreateAsyncScope();
        var provider = (IdentityRevalidatingAuthenticationStateProvider)
            scope.ServiceProvider.GetRequiredService<AuthenticationStateProvider>();
        return await provider.IsSecurityStampCurrentAsync(principal, CancellationToken.None);
    }
}
