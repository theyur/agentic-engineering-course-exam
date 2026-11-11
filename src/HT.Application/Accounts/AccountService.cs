using HT.Domain.Accounts;

namespace HT.Application.Accounts;

/// <summary>
/// The account use cases for anonymous visitors and signed-in users (ADR 0005). Each use case for a signed-in
/// user takes the acting user first and acts only on its account.
/// </summary>
public sealed class AccountService
{
    /// <summary>Save attempts per sign-in; after this many lost races the sign-in fails closed (research R-05).</summary>
    private const int MaxSignInSaves = 10;

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

    /// <summary>
    /// Signs an account in by email and password (FR-005 to FR-008), following research R-05: one time per
    /// attempt, a blocked account is refused without a password check, the password is checked once, and the
    /// next block state is saved with compare-and-swap, re-reading after a lost race. Every failure gives the
    /// same result.
    /// </summary>
    public async Task<SignInResult> SignInAsync(string? email, string? password, CancellationToken cancellationToken)
    {
        var now = _timeProvider.GetUtcNow();

        if (!Email.TryParse(email, out var parsed)
            || await _store.FindByEmailAsync(parsed, cancellationToken) is not { } account)
        {
            return SignInResult.Failed;
        }

        var state = account.SignInBlock.SettledAt(now);
        if (state.IsBlockedAt(now))
        {
            return SignInResult.Failed;
        }

        var passwordIsCorrect = await _store.CheckPasswordAsync(account.Id, password ?? string.Empty, cancellationToken);

        for (var save = 0; save < MaxSignInSaves; save++)
        {
            var next = passwordIsCorrect ? SignInBlock.AfterSuccess() : state.AfterFailureAt(now);
            if (await _store.TrySaveSignInBlockAsync(account.Id, account.SignInBlock, next, cancellationToken))
            {
                return passwordIsCorrect ? SignInResult.SignedIn(account) : SignInResult.Failed;
            }

            if (await _store.FindByIdAsync(account.Id, cancellationToken) is not { } reread)
            {
                return SignInResult.Failed;
            }

            account = reread;
            state = account.SignInBlock.SettledAt(now);
            if (state.IsBlockedAt(now))
            {
                return SignInResult.Failed;
            }
        }

        // Ten lost races in a row: fail closed, nothing saved.
        return SignInResult.Failed;
    }
}
