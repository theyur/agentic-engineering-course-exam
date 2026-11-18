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

    /// <summary>
    /// The named time zone identifier (FR-024); null only for accounts created before slice 4 (research R-04). Written
    /// at insert and afterwards only by the stores' conditional statements, never by Identity's full-row update.
    /// </summary>
    public string? TimeZone { get; set; }

    /// <summary>The account's own name (FR-028); null when it has none. Written at insert and by the store only.</summary>
    public string? Name { get; set; }

    /// <summary>When the account was created (FR-030); null for accounts created before slice 4. Written at insert only.</summary>
    public DateTimeOffset? CreatedAt { get; set; }

    /// <summary>
    /// Incremented by every goal creation and time-zone change, so the two serialize (research R-08). Written only by
    /// the goal store.
    /// </summary>
    public int GoalsRevision { get; set; }
}
