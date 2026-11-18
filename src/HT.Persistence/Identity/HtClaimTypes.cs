namespace HT.Persistence.Identity;

/// <summary>The claim types the session cookie carries besides Identity's own (contracts/host.md).</summary>
public static class HtClaimTypes
{
    /// <summary>
    /// The account's own name (FR-028), present only when it has one; the avatar letter reads it (HP-017). Private
    /// account data: it goes only into its own account's cookie, never into a log.
    /// </summary>
    public const string Name = "ht:name";
}
