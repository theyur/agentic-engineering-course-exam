using HT.Domain.Accounts;

namespace HT.Application.Accounts;

/// <summary>The outcomes of <see cref="AccountService.SignInAsync"/>.</summary>
public enum SignInOutcome
{
    Failed,
    SignedIn,
}

/// <summary>The result of a sign-in. Every failure has the same text (FR-007); no refusal is an exception (ADR 0005).</summary>
public sealed class SignInResult
{
    private SignInResult(SignInOutcome outcome, string? message, ActingUser? actingUser)
    {
        Outcome = outcome;
        Message = message;
        ActingUser = actingUser;
    }

    public SignInOutcome Outcome { get; }

    /// <summary>The spec's text for a failure; null for <see cref="SignInOutcome.SignedIn"/>.</summary>
    public string? Message { get; }

    /// <summary>The signed-in account with its role; set only for <see cref="SignInOutcome.SignedIn"/>.</summary>
    public ActingUser? ActingUser { get; }

    internal static SignInResult Failed { get; } = new(SignInOutcome.Failed, AccountTexts.SignInFailed, null);

    internal static SignInResult SignedIn(Account account) =>
        new(SignInOutcome.SignedIn, null, ActingUser.ForAccount(account.Id, account.Role));
}
