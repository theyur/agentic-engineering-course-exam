using HT.Domain.Accounts;

namespace HT.Application.Accounts;

/// <summary>
/// The account store port: persistence plumbing that only HT.Application calls and HT.Persistence implements
/// (ADR 0004). It is not a use case. Public so the AC reference export accepts the assembly (ADR 0010); an
/// architecture rule keeps every HT.Ui type away from it. Account reads return the database's current state.
/// </summary>
public interface IAccountStore
{
    /// <summary>Finds the account with this email (by its normal form).</summary>
    Task<Account?> FindByEmailAsync(Email email, CancellationToken cancellationToken);

    /// <summary>Finds the account with this identifier. Used only with the acting user's own identifier.</summary>
    Task<Account?> FindByIdAsync(AccountId id, CancellationToken cancellationToken);

    /// <summary>Creates an account. An email that already has an account gives <see cref="CreateAccountOutcome.EmailTaken"/>.</summary>
    Task<CreateAccountOutcome> CreateAsync(Email email, Role role, string password, CancellationToken cancellationToken);

    /// <summary>True when the password is the account's password.</summary>
    Task<bool> CheckPasswordAsync(AccountId id, string password, CancellationToken cancellationToken);

    /// <summary>Changes the password and renews the security stamp; false when the current password no longer matches.</summary>
    Task<bool> ChangePasswordAsync(AccountId id, string currentPassword, string newPassword, CancellationToken cancellationToken);

    /// <summary>Saves <paramref name="next"/> only when the stored state is still <paramref name="expected"/>.</summary>
    Task<bool> TrySaveSignInBlockAsync(AccountId id, SignInBlock expected, SignInBlock next, CancellationToken cancellationToken);
}
