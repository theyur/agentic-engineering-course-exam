using HT.Application.Accounts;
using HT.Domain.Accounts;

namespace HT.Application.Operator;

/// <summary>
/// The operator use cases the host runs at startup (HP-018). No page offers them; an architecture rule keeps
/// every HT.Ui type outside <c>HT.Ui.Operator</c> away from this service (research R-13).
/// </summary>
public sealed class OperatorService
{
    private readonly IAccountStore _store;
    private readonly TimeProvider _timeProvider;
    private readonly InstallationTimeZone _defaultTimeZone;

    public OperatorService(IAccountStore store, TimeProvider timeProvider, InstallationTimeZone defaultTimeZone)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(timeProvider);
        ArgumentNullException.ThrowIfNull(defaultTimeZone);
        _store = store;
        _timeProvider = timeProvider;
        _defaultTimeZone = defaultTimeZone;
    }

    /// <summary>
    /// Creates an administrator with the configured email and password (FR-014), checking in order: both values
    /// configured, a valid email, no administrator with that email, no registered user with that email, the
    /// password rule. An existing account is never changed. A created administrator has the installation's default
    /// time zone, no name and the supplied current time as its creation time (FR-025, FR-030).
    /// </summary>
    public async Task<AdministratorCreationResult> CreateAdministratorAsync(
        string? email, string? password, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return AdministratorCreationResult.NotConfigured;
        }

        if (!Email.TryParse(email, out var parsed))
        {
            return AdministratorCreationResult.InvalidEmail;
        }

        if (await _store.FindByEmailAsync(parsed, cancellationToken) is { } existing)
        {
            return ExistingAccount(existing);
        }

        if (!PasswordRule.IsMet(password))
        {
            return AdministratorCreationResult.PasswordRuleNotMet;
        }

        var profile = new NewAccountProfile(null, _defaultTimeZone.Value, _timeProvider.GetUtcNow());
        var created = await _store.CreateAsync(parsed, Role.Administrator, password, profile, cancellationToken);
        if (!created.IsEmailTaken)
        {
            return AdministratorCreationResult.Created;
        }

        // Another start or a registration took the email in the meantime: report what holds it now.
        return await _store.FindByEmailAsync(parsed, cancellationToken) is { } winner
            ? ExistingAccount(winner)
            : AdministratorCreationResult.AlreadyExists;
    }

    private static AdministratorCreationResult ExistingAccount(Account account) =>
        account.Role == Role.Administrator
            ? AdministratorCreationResult.AlreadyExists
            : AdministratorCreationResult.BelongsToRegisteredUser;
}
