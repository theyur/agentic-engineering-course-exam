using HT.Domain.Accounts;

namespace HT.Application.Accounts;

/// <summary>
/// The account use cases for anonymous visitors and signed-in users (ADR 0005). Each use case for a signed-in
/// user takes the acting user first and acts only on its account.
/// </summary>
public sealed class AccountService
{
    private readonly IAccountStore _store;
    private readonly TimeProvider _timeProvider;

    public AccountService(IAccountStore store, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(timeProvider);
        _store = store;
        _timeProvider = timeProvider;
    }

    /// <summary>
    /// Registers an anonymous visitor (FR-001 to FR-004). Checks, in order: a valid email, the password rule,
    /// the email not taken. A registration that loses a race for the email gives <see cref="RegistrationOutcome.EmailTaken"/>.
    /// </summary>
    public async Task<RegistrationResult> RegisterAsync(string? email, string? password, CancellationToken cancellationToken)
    {
        if (!Email.TryParse(email, out var parsed))
        {
            return RegistrationResult.InvalidEmail;
        }

        if (!PasswordRule.IsMet(password))
        {
            return RegistrationResult.PasswordRuleNotMet;
        }

        if (await _store.FindByEmailAsync(parsed, cancellationToken) is not null)
        {
            return RegistrationResult.EmailTaken;
        }

        var created = await _store.CreateAsync(parsed, Role.RegisteredUser, password!, cancellationToken);
        return created.Account is { } account
            ? RegistrationResult.Registered(account.Id)
            : RegistrationResult.EmailTaken;
    }
}
