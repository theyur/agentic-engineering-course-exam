using System.Security.Claims;
using HT.Application.Accounts;
using HT.Domain.Accounts;
using HT.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace HT.IntegrationTests.Accounts;

/// <summary>The claims the session cookie carries (research R-07): id, normal-form email, role and security stamp.</summary>
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
            .CreateAsync(parsed, role, "walking42", CancellationToken.None);
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
}
