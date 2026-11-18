using System.Security.Claims;
using HT.Persistence.Identity;

namespace HT.Ui.Account;

/// <summary>Reads the session's own account claims for the pages, so components never name a persistence type.</summary>
internal static class AccountClaims
{
    /// <summary>The account's own name from the session cookie (HP-017); null without one or for a visitor.</summary>
    public static string? NameOf(ClaimsPrincipal? principal)
    {
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var name = principal.FindFirstValue(HtClaimTypes.Name);
        return string.IsNullOrEmpty(name) ? null : name;
    }
}
