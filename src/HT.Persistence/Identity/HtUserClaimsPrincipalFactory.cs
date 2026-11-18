using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace HT.Persistence.Identity;

/// <summary>
/// Adds a role claim with the account's role name to Identity's identifier, name and security-stamp claims
/// (research R-07), and <see cref="HtClaimTypes.Name"/> with the account's own name when it has one (HP-017). The
/// name claim of Identity holds the account's normal-form email.
/// </summary>
internal sealed class HtUserClaimsPrincipalFactory(UserManager<HtUser> userManager, IOptions<IdentityOptions> options)
    : UserClaimsPrincipalFactory<HtUser>(userManager, options)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(HtUser user)
    {
        var identity = await base.GenerateClaimsAsync(user).ConfigureAwait(false);
        identity.AddClaim(new Claim(Options.ClaimsIdentity.RoleClaimType, user.Role.ToString()));
        if (!string.IsNullOrEmpty(user.Name))
        {
            identity.AddClaim(new Claim(HtClaimTypes.Name, user.Name));
        }

        return identity;
    }
}
