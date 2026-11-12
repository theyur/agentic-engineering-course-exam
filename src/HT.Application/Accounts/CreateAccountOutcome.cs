using HT.Domain.Accounts;

namespace HT.Application.Accounts;

/// <summary>The outcome of <see cref="IAccountStore.CreateAsync"/>: the created account, or the email was taken.</summary>
public sealed class CreateAccountOutcome
{
    private CreateAccountOutcome(Account? account) => Account = account;

    /// <summary>The email already has an account.</summary>
    public static CreateAccountOutcome EmailTaken { get; } = new(null);

    /// <summary>The created account; null when the email was taken.</summary>
    public Account? Account { get; }

    public bool IsEmailTaken => Account is null;

    public static CreateAccountOutcome Created(Account account)
    {
        ArgumentNullException.ThrowIfNull(account);
        return new CreateAccountOutcome(account);
    }
}
