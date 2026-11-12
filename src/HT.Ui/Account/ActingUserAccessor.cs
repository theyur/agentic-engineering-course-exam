using System.Security.Claims;
using HT.Domain.Accounts;
using Microsoft.AspNetCore.Components.Authorization;

namespace HT.Ui.Account;

/// <summary>
/// Turns the session's principal into the domain <see cref="ActingUser"/> that pages pass to the use cases
/// (research R-07). Anything but an authenticated principal with a valid id and role is the anonymous visitor.
/// </summary>
internal sealed class ActingUserAccessor(AuthenticationStateProvider authenticationStateProvider)
{
    /// <summary>The acting user of the current request or circuit.</summary>
    public async Task<ActingUser> GetAsync()
    {
        var state = await authenticationStateProvider.GetAuthenticationStateAsync();
        return FromPrincipal(state.User);
    }

    /// <summary>
    /// <see cref="ActingUser.ForAccount"/> from the <c>NameIdentifier</c> and role claims of an authenticated
    /// principal; <see cref="ActingUser.Anonymous"/> when it is not authenticated or a claim is missing or invalid.
    /// </summary>
    public static ActingUser FromPrincipal(ClaimsPrincipal? principal)
    {
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return ActingUser.Anonymous;
        }

        if (!Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id) || id == Guid.Empty)
        {
            return ActingUser.Anonymous;
        }

        var roleName = principal.FindFirstValue(ClaimTypes.Role);
        if (roleName is null
            || !Enum.TryParse<Role>(roleName, ignoreCase: false, out var role)
            || !Enum.IsDefined(role)
            || !string.Equals(role.ToString(), roleName, StringComparison.Ordinal))
        {
            return ActingUser.Anonymous;
        }

        return ActingUser.ForAccount(new AccountId(id), role);
    }
}
