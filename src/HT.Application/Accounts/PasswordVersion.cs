namespace HT.Application.Accounts;

/// <summary>
/// The version of an account's password that a password check was made against. Opaque: only the store reads
/// it. Every password change renews it, so a sign-in whose check is older than a password change can neither save
/// its block state nor keep its session (HP-007).
/// </summary>
public sealed record PasswordVersion
{
    public PasswordVersion(string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(value);
        Value = value;
    }

    public string Value { get; }
}
