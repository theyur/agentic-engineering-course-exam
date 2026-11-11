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
}
