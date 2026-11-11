using HT.Application.Accounts;
using HT.Domain.Accounts;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace HT.Persistence.Identity;

/// <summary>
/// The account store over Identity's user manager and the EF Core context (research R-05). Reads are
/// no-tracking, so they return the database's current state. The calls that need an <see cref="HtUser"/>
/// clear the change tracker before and after their work, so no instance outlives one call.
/// </summary>
internal sealed class AccountStore : IAccountStore
{
    private const string DuplicateUserNameCode = nameof(IdentityErrorDescriber.DuplicateUserName);
    private const string PasswordMismatchCode = nameof(IdentityErrorDescriber.PasswordMismatch);

    private readonly HtDbContext _db;
    private readonly UserManager<HtUser> _users;

    public AccountStore(HtDbContext db, UserManager<HtUser> users)
    {
        _db = db;
        _users = users;
    }

    public async Task<Account?> FindByEmailAsync(Email email, CancellationToken cancellationToken)
    {
        var user = await _db.Users.AsNoTracking()
            .SingleOrDefaultAsync(u => u.NormalizedUserName == email.Value, cancellationToken)
            .ConfigureAwait(false);
        return user is null ? null : ToAccount(user);
    }

    public async Task<Account?> FindByIdAsync(AccountId id, CancellationToken cancellationToken)
    {
        var user = await _db.Users.AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == id.Value, cancellationToken)
            .ConfigureAwait(false);
        return user is null ? null : ToAccount(user);
    }

    public async Task<CreateAccountOutcome> CreateAsync(Email email, Role role, string password, CancellationToken cancellationToken)
    {
        _db.ChangeTracker.Clear();
        try
        {
            var user = new HtUser
            {
                Id = Guid.NewGuid(),
                UserName = email.Value,
                Email = email.Value,
                Role = role,
            };

            IdentityResult result;
            try
            {
                result = await _users.CreateAsync(user, password).ConfigureAwait(false);
            }
            catch (DbUpdateException exception) when (IsUniqueIndexViolation(exception))
            {
                return CreateAccountOutcome.EmailTaken;
            }

            if (result.Succeeded)
            {
                return CreateAccountOutcome.Created(ToAccount(user));
            }

            if (result.Errors.Any(e => e.Code == DuplicateUserNameCode))
            {
                return CreateAccountOutcome.EmailTaken;
            }

            throw new InvalidOperationException("Account creation failed: " + ErrorCodes(result));
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }
    }

    public async Task<bool> CheckPasswordAsync(AccountId id, string password, CancellationToken cancellationToken)
    {
        _db.ChangeTracker.Clear();
        try
        {
            var user = await _users.FindByIdAsync(id.Value.ToString()).ConfigureAwait(false);
            return user is not null && await _users.CheckPasswordAsync(user, password).ConfigureAwait(false);
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }
    }

    public async Task<bool> ChangePasswordAsync(AccountId id, string currentPassword, string newPassword, CancellationToken cancellationToken)
    {
        _db.ChangeTracker.Clear();
        try
        {
            var user = await _users.FindByIdAsync(id.Value.ToString()).ConfigureAwait(false);
            if (user is null)
            {
                return false;
            }

            var result = await _users.ChangePasswordAsync(user, currentPassword, newPassword).ConfigureAwait(false);
            if (result.Succeeded)
            {
                return true;
            }

            if (result.Errors.Any(e => e.Code == PasswordMismatchCode))
            {
                return false;
            }

            throw new InvalidOperationException("Password change failed: " + ErrorCodes(result));
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }
    }

    public async Task<bool> TrySaveSignInBlockAsync(AccountId id, SignInBlock expected, SignInBlock next, CancellationToken cancellationToken)
    {
        var query = _db.Users.Where(u => u.Id == id.Value && u.FailedSignInCount == expected.FailedCount);
        query = expected.BlockedSince is { } since
            ? query.Where(u => u.SignInBlockedSince == since)
            : query.Where(u => u.SignInBlockedSince == null);

        var rows = await query.ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(u => u.FailedSignInCount, next.FailedCount)
                    .SetProperty(u => u.SignInBlockedSince, next.BlockedSince),
                cancellationToken)
            .ConfigureAwait(false);
        return rows == 1;
    }

    private static Account ToAccount(HtUser user)
    {
        if (!Email.TryParse(user.UserName, out var email))
        {
            throw new InvalidOperationException("A stored account has no valid email.");
        }

        return new Account(
            new AccountId(user.Id),
            email,
            user.Role,
            new SignInBlock(user.FailedSignInCount, user.SignInBlockedSince));
    }

    private static bool IsUniqueIndexViolation(DbUpdateException exception) =>
        exception.InnerException is SqlException { Number: 2601 or 2627 };

    private static string ErrorCodes(IdentityResult result) => string.Join(", ", result.Errors.Select(e => e.Code));
}
