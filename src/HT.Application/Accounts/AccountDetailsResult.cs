namespace HT.Application.Accounts;

/// <summary>The outcomes of <see cref="AccountService.GetAccountDetailsAsync"/>.</summary>
public enum AccountDetailsOutcome
{
    NotSignedIn,
    Found,
}

/// <summary>The result of reading the acting user's own account details. No refusal is an exception (ADR 0005).</summary>
public sealed class AccountDetailsResult
{
    private AccountDetailsResult(AccountDetailsOutcome outcome, string? message, AccountDetails? accountDetails)
    {
        Outcome = outcome;
        Message = message;
        AccountDetails = accountDetails;
    }

    public AccountDetailsOutcome Outcome { get; }

    /// <summary>The spec's text for a refusal; null for <see cref="AccountDetailsOutcome.Found"/>.</summary>
    public string? Message { get; }

    /// <summary>The acting user's own details; set only for <see cref="AccountDetailsOutcome.Found"/>.</summary>
    public AccountDetails? AccountDetails { get; }

    internal static AccountDetailsResult NotSignedIn { get; } =
        new(AccountDetailsOutcome.NotSignedIn, AccountTexts.NotSignedIn, null);

    internal static AccountDetailsResult Found(AccountDetails accountDetails) =>
        new(AccountDetailsOutcome.Found, null, accountDetails);
}
