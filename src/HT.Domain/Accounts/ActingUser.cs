namespace HT.Domain.Accounts;

/// <summary>
/// The account, with its role, on whose behalf a use case runs, or the anonymous visitor. Built by the host
/// from the session and by tests from a successful sign-in result; never from user input.
/// </summary>
public sealed class ActingUser
{
    private ActingUser(AccountId? accountId, Role? role)
    {
        AccountId = accountId;
        Role = role;
    }

    /// <summary>The anonymous visitor: no account and no role.</summary>
    public static ActingUser Anonymous { get; } = new(null, null);

    public AccountId? AccountId { get; }

    public Role? Role { get; }

    public bool IsAnonymous => AccountId is null;

    /// <summary>A signed-in account with its role.</summary>
    public static ActingUser ForAccount(AccountId accountId, Role role) => new(accountId, role);
}
