namespace HT.Domain.Accounts;

/// <summary>
/// An account's own name (FR-028): the trimmed text, 1 to 60 UTF-16 code units; empty after trimming means no name.
/// Private account data, never the public display name of community participation.
/// </summary>
public readonly record struct AccountName
{
    public const int MaxLength = 60;

    private AccountName(string value) => Value = value;

    /// <summary>The trimmed name as entered.</summary>
    public string Value { get; }

    /// <summary>
    /// Trims the text. Null, empty or only spaces give no name (<paramref name="name"/> null) and true; more than 60
    /// code units after trimming give false with <paramref name="tooLong"/> set.
    /// </summary>
    public static bool TryCreate(string? text, out AccountName? name, out bool tooLong)
    {
        name = null;
        tooLong = false;
        var trimmed = text?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
        {
            return true;
        }

        if (trimmed.Length > MaxLength)
        {
            tooLong = true;
            return false;
        }

        name = new AccountName(trimmed);
        return true;
    }

    public override string ToString() => Value ?? string.Empty;
}
