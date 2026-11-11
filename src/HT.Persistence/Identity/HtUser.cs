using HT.Domain.Accounts;
using Microsoft.AspNetCore.Identity;

namespace HT.Persistence.Identity;

/// <summary>
/// The Identity user type of an account (ADR 0004). <c>UserName</c>, <c>Email</c> and their normalized columns
/// all hold the domain normal form of the email. Only HT.Persistence and the host's composition code and
/// account endpoints use this type.
/// </summary>
public sealed class HtUser : IdentityUser<Guid>
{
    /// <summary>The account's role (FR-009).</summary>
    public Role Role { get; set; }

    /// <summary>The count of failed sign-ins (<see cref="SignInBlock.FailedCount"/>).</summary>
    public int FailedSignInCount { get; set; }

    /// <summary>The instant the current block began (<see cref="SignInBlock.BlockedSince"/>).</summary>
    public DateTimeOffset? SignInBlockedSince { get; set; }
}
