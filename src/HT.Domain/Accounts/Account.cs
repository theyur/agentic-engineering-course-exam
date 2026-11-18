namespace HT.Domain.Accounts;

/// <summary>One person's way into the app. Immutable: use cases produce a new value and save it.</summary>
public sealed record Account(AccountId Id, Email Email, Role Role, SignInBlock SignInBlock)
{
    /// <summary>
    /// The stored time zone identifier; null only for accounts created before slice 4, which have the installation's
    /// default time zone (FR-025, research R-04).
    /// </summary>
    public string? StoredTimeZone { get; init; }

    /// <summary>The account's own name (FR-028), trimmed; null when it has none.</summary>
    public string? Name { get; init; }

    /// <summary>When the account was created (FR-030); null for accounts created before slice 4.</summary>
    public DateTimeOffset? CreatedAt { get; init; }
}
