using System.Security.Claims;
using HT.Persistence.Identity;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace HT.Ui.Account;

/// <summary>
/// Revalidates the session of an open interactive circuit every 30 minutes against the account's security stamp,
/// so a page left open in another browser loses access after a password change (HP-007, research R-09).
/// Requests are checked on every load by the cookie's security-stamp validator; this covers the circuit.
/// </summary>
internal sealed class IdentityRevalidatingAuthenticationStateProvider(
    ILoggerFactory loggerFactory,
    IServiceScopeFactory scopeFactory,
    IOptions<IdentityOptions> identityOptions)
    : RevalidatingServerAuthenticationStateProvider(loggerFactory)
{
    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(30);

    protected override Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState, CancellationToken cancellationToken) =>
        IsSecurityStampCurrentAsync(authenticationState.User, cancellationToken);

    /// <summary>
    /// True when the principal's security-stamp claim equals the account's stored stamp; false for a principal
    /// without a stamp or for an account that no longer exists. Reads in a new scope, as the circuit outlives
    /// any one request.
    /// </summary>
    internal async Task<bool> IsSecurityStampCurrentAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(principal);
        cancellationToken.ThrowIfCancellationRequested();

        await using var scope = scopeFactory.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<HtUser>>();
        if (await users.GetUserAsync(principal) is not { } user)
        {
            return false;
        }

        var claimed = principal.FindFirstValue(identityOptions.Value.ClaimsIdentity.SecurityStampClaimType);
        return claimed is not null
            && string.Equals(claimed, await users.GetSecurityStampAsync(user), StringComparison.Ordinal);
    }
}
