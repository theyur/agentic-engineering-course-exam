namespace HT.Domain.Accounts;

/// <summary>
/// An email address in its normal form: trimmed and lower-cased with the invariant culture (FR-003).
/// Two emails are equal when their normal forms are equal, compared ordinally.
/// </summary>
public readonly record struct Email
{
    public const int MaxLength = 254;

    private Email(string value) => Value = value;

    /// <summary>The normal form.</summary>
    public string Value { get; }

    /// <summary>
    /// Accepts the text when, after trimming, it has at most 254 characters, no whitespace character,
    /// exactly one <c>@</c> with at least one character before it, and a domain of at least two
    /// non-empty parts separated by <c>.</c>.
    /// </summary>
    public static bool TryParse(string? text, out Email email)
    {
        email = default;
        if (text is null)
        {
            return false;
        }

        var trimmed = text.Trim();
        if (trimmed.Length == 0 || trimmed.Length > MaxLength)
        {
            return false;
        }

        foreach (var c in trimmed)
        {
            if (char.IsWhiteSpace(c))
            {
                return false;
            }
        }

        var at = trimmed.IndexOf('@');
        if (at <= 0 || trimmed.IndexOf('@', at + 1) >= 0)
        {
            return false;
        }

        var parts = trimmed[(at + 1)..].Split('.');
        if (parts.Length < 2)
        {
            return false;
        }

        foreach (var part in parts)
        {
            if (part.Length == 0)
            {
                return false;
            }
        }

        email = new Email(trimmed.ToLowerInvariant());
        return true;
    }

    public bool Equals(Email other) => string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override int GetHashCode() => Value is null ? 0 : StringComparer.Ordinal.GetHashCode(Value);

    public override string ToString() => Value ?? string.Empty;
}
