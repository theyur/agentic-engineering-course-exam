namespace HT.Application.Accounts;

/// <summary>The outcomes of <see cref="AccountService.ChangePasswordAsync"/>, in FR-013's check order.</summary>
public enum PasswordChangeOutcome
{
    NotSignedIn,
    CurrentPasswordWrong,
    PasswordRuleNotMet,
    Changed,
}

/// <summary>The result of changing the acting user's own password. No refusal is an exception (ADR 0005).</summary>
public sealed class PasswordChangeResult
{
    private PasswordChangeResult(PasswordChangeOutcome outcome, string? message)
    {
        Outcome = outcome;
        Message = message;
    }

    public PasswordChangeOutcome Outcome { get; }

    /// <summary>The spec's text for a refusal; null for <see cref="PasswordChangeOutcome.Changed"/>.</summary>
    public string? Message { get; }

    internal static PasswordChangeResult NotSignedIn { get; } =
        new(PasswordChangeOutcome.NotSignedIn, AccountTexts.NotSignedIn);

    internal static PasswordChangeResult CurrentPasswordWrong { get; } =
        new(PasswordChangeOutcome.CurrentPasswordWrong, AccountTexts.CurrentPasswordWrong);

    internal static PasswordChangeResult PasswordRuleNotMet { get; } =
        new(PasswordChangeOutcome.PasswordRuleNotMet, AccountTexts.PasswordRule);

    internal static PasswordChangeResult Changed { get; } = new(PasswordChangeOutcome.Changed, null);
}
