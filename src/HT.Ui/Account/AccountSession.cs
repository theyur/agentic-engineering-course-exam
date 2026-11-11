using HT.Domain.Accounts;
using HT.Persistence.Identity;
using Microsoft.AspNetCore.Identity;

namespace HT.Ui.Account;

/// <summary>
/// The only code that writes the session cookie (contracts/host.md). Pages call it with the acting user of a
/// successful use case; components never touch <see cref="SignInManager{TUser}"/> themselves (ADR 0004).
/// </summary>
internal sealed class AccountSession(SignInManager<HtUser> signInManager, UserManager<HtUser> userManager)
{
    /// <summary>Signs the account in, with a persistent cookie when <paramref name="keepSignedIn"/> is true.</summary>
    public async Task SignInAsync(ActingUser signedIn, bool keepSignedIn)
    {
        var user = await LoadAsync(signedIn);
        await signInManager.SignInAsync(user, isPersistent: keepSignedIn);
    }

    /// <summary>Re-issues the cookie with the account's current security stamp, keeping its persistence.</summary>
    public async Task RefreshSignInAsync(ActingUser actingUser)
    {
        var user = await LoadAsync(actingUser);
        await signInManager.RefreshSignInAsync(user);
    }

    /// <summary>Ends the cookie.</summary>
    public Task SignOutAsync() => signInManager.SignOutAsync();

    private async Task<HtUser> LoadAsync(ActingUser actingUser)
    {
        var id = actingUser.AccountId
            ?? throw new ArgumentException("The anonymous visitor has no session to write.", nameof(actingUser));
        return await userManager.FindByIdAsync(id.Value.ToString())
            ?? throw new InvalidOperationException("The signed-in account no longer exists.");
    }
}
