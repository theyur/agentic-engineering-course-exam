using HT.Domain.Accounts;

namespace HT.Application.Accounts;

/// <summary>
/// The outcomes of <c>AccountService.RegisterAsync</c>, in check order (FR-003); <see cref="NameTooLong"/> is appended
/// after slice 1's members and checked after them (FR-028).
/// </summary>
public enum RegistrationOutcome
{
    InvalidEmail,
    PasswordRuleNotMet,
    EmailTaken,
    Registered,
    NameTooLong,
}

/// <summary>The result of a registration. No refusal is an exception (ADR 0005).</summary>
public sealed class RegistrationResult
{
    private RegistrationResult(RegistrationOutcome outcome, string? message, AccountId? accountId)
    {
        Outcome = outcome;
        Message = message;
        AccountId = accountId;
    }

    public RegistrationOutcome Outcome { get; }

    /// <summary>The spec's text for a refusal; null for <see cref="RegistrationOutcome.Registered"/>.</summary>
    public string? Message { get; }

    /// <summary>The new account; set only for <see cref="RegistrationOutcome.Registered"/>.</summary>
    public AccountId? AccountId { get; }

    internal static RegistrationResult InvalidEmail { get; } =
        new(RegistrationOutcome.InvalidEmail, AccountTexts.InvalidEmail, null);

    internal static RegistrationResult PasswordRuleNotMet { get; } =
        new(RegistrationOutcome.PasswordRuleNotMet, AccountTexts.PasswordRule, null);

    internal static RegistrationResult EmailTaken { get; } =
        new(RegistrationOutcome.EmailTaken, AccountTexts.EmailTaken, null);

    internal static RegistrationResult NameTooLong { get; } =
        new(RegistrationOutcome.NameTooLong, AccountTexts.NameTooLong, null);

    internal static RegistrationResult Registered(AccountId accountId) =>
        new(RegistrationOutcome.Registered, null, accountId);
}
