namespace HT.Domain.Accounts;

/// <summary>The password rule: at least 8 characters, with at least one digit '0' to '9' (FR-002).</summary>
public static class PasswordRule
{
    public const int MinimumLength = 8;

    /// <summary>True when the password meets the rule. The text is not trimmed.</summary>
    public static bool IsMet(string? password)
    {
        if (password is null || password.Length < MinimumLength)
        {
            return false;
        }

        foreach (var c in password)
        {
            if (c is >= '0' and <= '9')
            {
                return true;
            }
        }

        return false;
    }
}
