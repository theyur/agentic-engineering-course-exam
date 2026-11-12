using System.Security.Claims;
using HT.Domain.Accounts;
using HT.Ui.Account;
using Microsoft.AspNetCore.Components.Authorization;

namespace HT.UiTests.Account;

public sealed class ActingUserAccessorTests
{
    private const string Scheme = "Identity.Application";
    private static readonly Guid Id = Guid.Parse("4b6f2c1e-8d3a-4f5b-9c7e-1a2b3c4d5e6f");

    [Theory]
    [InlineData("RegisteredUser", Role.RegisteredUser)]
    [InlineData("Administrator", Role.Administrator)]
    public void An_authenticated_principal_gives_its_account_and_role(string roleClaim, Role role)
    {
        var actingUser = ActingUserAccessor.FromPrincipal(Principal(Id.ToString(), roleClaim));

        Assert.False(actingUser.IsAnonymous);
        Assert.Equal(new AccountId(Id), actingUser.AccountId);
        Assert.Equal(role, actingUser.Role);
    }

    [Fact]
    public void An_unauthenticated_principal_is_the_anonymous_visitor()
    {
        var unauthenticated = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, Id.ToString()), new Claim(ClaimTypes.Role, "Administrator")]));

        Assert.Same(ActingUser.Anonymous, ActingUserAccessor.FromPrincipal(unauthenticated));
        Assert.Same(ActingUser.Anonymous, ActingUserAccessor.FromPrincipal(new ClaimsPrincipal(new ClaimsIdentity())));
        Assert.Same(ActingUser.Anonymous, ActingUserAccessor.FromPrincipal(null));
    }

    [Theory]
    [InlineData(null, "RegisteredUser")]
    [InlineData("", "RegisteredUser")]
    [InlineData("not-a-guid", "RegisteredUser")]
    [InlineData("00000000-0000-0000-0000-000000000000", "RegisteredUser")]
    [InlineData("4b6f2c1e-8d3a-4f5b-9c7e-1a2b3c4d5e6f", null)]
    [InlineData("4b6f2c1e-8d3a-4f5b-9c7e-1a2b3c4d5e6f", "")]
    [InlineData("4b6f2c1e-8d3a-4f5b-9c7e-1a2b3c4d5e6f", "Admin")]
    [InlineData("4b6f2c1e-8d3a-4f5b-9c7e-1a2b3c4d5e6f", "administrator")]
    [InlineData("4b6f2c1e-8d3a-4f5b-9c7e-1a2b3c4d5e6f", "1")]
    [InlineData("4b6f2c1e-8d3a-4f5b-9c7e-1a2b3c4d5e6f", "7")]
    public void A_missing_or_malformed_id_or_role_is_the_anonymous_visitor(string? id, string? role)
    {
        Assert.Same(ActingUser.Anonymous, ActingUserAccessor.FromPrincipal(Principal(id, role)));
    }

    [Fact]
    public async Task GetAsync_reads_the_principal_of_the_current_authentication_state()
    {
        var accessor = new ActingUserAccessor(new FixedState(Principal(Id.ToString(), "Administrator")));

        var actingUser = await accessor.GetAsync();

        Assert.Equal(new AccountId(Id), actingUser.AccountId);
        Assert.Equal(Role.Administrator, actingUser.Role);
    }

    private static ClaimsPrincipal Principal(string? id, string? role)
    {
        var claims = new List<Claim>();
        if (id is not null)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, id));
        }

        if (role is not null)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme));
    }

    private sealed class FixedState(ClaimsPrincipal principal) : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
            Task.FromResult(new AuthenticationState(principal));
    }
}
