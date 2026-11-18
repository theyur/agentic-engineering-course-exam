using System.Security.Claims;
using HT.Application.Accounts;
using HT.Domain.Accounts;
using HT.Domain.Time;
using HT.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace HT.IntegrationTests.Accounts;

/// <summary>
/// The claims the session cookie carries (research R-07): id, normal-form email, role and security stamp, and the
/// account's own name as <c>ht:name</c> when it has one (HP-017). Own prefix "it-cp-" for the slice 4 tests.
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class ClaimsPrincipalFactoryTests(DatabaseFixture database)
{
    [Theory]
    [InlineData("Claims.User@Example.com", Role.RegisteredUser, "RegisteredUser")]
    [InlineData("Claims.Admin@Example.com", Role.Administrator, "Administrator")]
    public async Task The_principal_carries_the_id_the_normal_form_email_the_role_and_the_security_stamp(
        string email, Role role, string roleClaim)
    {
        Assert.True(Email.TryParse(email, out var parsed));
        await using var scope = database.CreateScope();
        var created = await scope.ServiceProvider.GetRequiredService<IAccountStore>()
            .CreateAsync(parsed, role, "walking42", TestProfiles.Utc, CancellationToken.None);
        var id = created.Account!.Id.Value.ToString();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<HtUser>>();
        var user = await userManager.FindByIdAsync(id);
        Assert.NotNull(user);

        var principal = await scope.ServiceProvider.GetRequiredService<IUserClaimsPrincipalFactory<HtUser>>()
            .CreateAsync(user);

        Assert.Equal(id, principal.FindFirstValue(ClaimTypes.NameIdentifier));
        Assert.Equal(parsed.Value, principal.FindFirstValue(ClaimTypes.Name));
        Assert.Equal(roleClaim, Assert.Single(principal.FindAll(ClaimTypes.Role)).Value);
        var stamp = principal.FindFirstValue(new IdentityOptions().ClaimsIdentity.SecurityStampClaimType);
        Assert.False(string.IsNullOrEmpty(stamp));
        Assert.Equal(await userManager.GetSecurityStampAsync(user), stamp);
    }

    [Fact]
    public async Task HP_017_the_principal_carries_ht_name_when_the_account_has_a_name()
    {
        var principal = await PrincipalAsync("it-cp-named@example.com", " It-Cp Yuri ");

        Assert.Equal("It-Cp Yuri", Assert.Single(principal.FindAll("ht:name")).Value);
        Assert.Equal("it-cp-named@example.com", principal.FindFirstValue(ClaimTypes.Name));
    }

    [Fact]
    public async Task HP_017_the_principal_has_no_ht_name_without_a_name()
    {
        var principal = await PrincipalAsync("it-cp-unnamed@example.com", null);

        Assert.Empty(principal.FindAll("ht:name"));
    }

    [Fact]
    public async Task HP_017_ht_name_follows_a_name_change_when_the_principal_is_made_again()
    {
        const string email = "it-cp-renamed@example.com";
        await Sql.DeleteUserAsync(database.ConnectionString, email);
        Assert.True(Email.TryParse(email, out var parsed));
        await using var scope = database.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IAccountStore>();
        var id = (await store.CreateAsync(parsed, Role.RegisteredUser, "walking42", TestProfiles.Utc, CancellationToken.None)).Account!.Id;
        Assert.True(AccountName.TryCreate("It-Cp Olena", out var name, out _));
        Assert.True(await store.TrySetNameAsync(id, name, CancellationToken.None));

        await using var fresh = database.CreateScope();
        var user = (await fresh.ServiceProvider.GetRequiredService<UserManager<HtUser>>().FindByIdAsync(id.Value.ToString()))!;
        var principal = await fresh.ServiceProvider.GetRequiredService<IUserClaimsPrincipalFactory<HtUser>>().CreateAsync(user);

        Assert.Equal("It-Cp Olena", principal.FindFirstValue("ht:name"));
    }

    private async Task<ClaimsPrincipal> PrincipalAsync(string email, string? name)
    {
        await Sql.DeleteUserAsync(database.ConnectionString, email);
        Assert.True(Email.TryParse(email, out var parsed));
        Assert.True(AccountName.TryCreate(name, out var accountName, out _));
        await using var scope = database.CreateScope();
        var profile = new NewAccountProfile(accountName, NamedTimeZone.Utc, new DateTimeOffset(2026, 11, 18, 10, 0, 0, TimeSpan.Zero));
        var created = await scope.ServiceProvider.GetRequiredService<IAccountStore>()
            .CreateAsync(parsed, Role.RegisteredUser, "walking42", profile, CancellationToken.None);
        var user = (await scope.ServiceProvider.GetRequiredService<UserManager<HtUser>>().FindByIdAsync(created.Account!.Id.Value.ToString()))!;
        return await scope.ServiceProvider.GetRequiredService<IUserClaimsPrincipalFactory<HtUser>>().CreateAsync(user);
    }
}
