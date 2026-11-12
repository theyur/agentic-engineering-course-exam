using HT.Application.Accounts;
using HT.Domain.Accounts;

namespace HT.UnitTests.Fakes;

/// <summary>
/// A hand-written in-memory <see cref="IAccountStore"/> (ADR 0006). Accounts are keyed by email, passwords are
/// kept as given. Hooks let a test simulate parallel attempts and registrations; counters let it assert what
/// the use case did. Safe to call from parallel tasks.
/// </summary>
public sealed class FakeAccountStore : IAccountStore
{
    private readonly object _gate = new();
    private readonly Dictionary<Email, Entry> _byEmail = [];
    private int _savesToLose;
    private bool _nextLostSaveIsOne = true;
    private DateTimeOffset? _blockOnNextSave;
    private bool _takeEmailOnNextCreate;
    private string? _passwordAfterNextCheck;

    /// <summary>Calls of <see cref="TrySaveSignInBlockAsync"/>, lost ones included.</summary>
    public int SaveCount { get; private set; }

    /// <summary>Calls of <see cref="TrySaveSignInBlockAsync"/> that changed the stored state.</summary>
    public int SuccessfulSaveCount { get; private set; }

    /// <summary>Calls of <see cref="CheckPasswordAsync"/>.</summary>
    public int PasswordCheckCount { get; private set; }

    /// <summary>Calls of <see cref="ChangePasswordAsync"/>.</summary>
    public int PasswordChangeCount { get; private set; }

    /// <summary>Calls of <see cref="CreateAsync"/>.</summary>
    public int CreateCount { get; private set; }

    public IReadOnlyList<Account> Accounts
    {
        get
        {
            lock (_gate)
            {
                return _byEmail.Values.Select(e => e.Account).ToList();
            }
        }
    }

    /// <summary>
    /// Makes the next <paramref name="count"/> saves lose a race. Before each lost save the stored state moves to
    /// (1, none) or (0, none), alternating, as a parallel failure or success would; then the save returns false.
    /// </summary>
    public void LoseNextSaves(int count)
    {
        lock (_gate)
        {
            _savesToLose = count;
        }
    }

    /// <summary>Makes the next save store (5, <paramref name="at"/>), as a parallel fifth failure would, then return false.</summary>
    public void BlockOnNextSave(DateTimeOffset at)
    {
        lock (_gate)
        {
            _blockOnNextSave = at;
        }
    }

    /// <summary>
    /// Makes the next create store the account, as a parallel registration would, then return EmailTaken.
    /// A lookup before it still finds nothing.
    /// </summary>
    public void TakeEmailOnNextCreate()
    {
        lock (_gate)
        {
            _takeEmailOnNextCreate = true;
        }
    }

    /// <summary>
    /// Makes the next password check return its verdict and then change the password to
    /// <paramref name="newPassword"/>, as a password change in another session right after the check would.
    /// </summary>
    public void ChangePasswordAfterNextCheck(string newPassword)
    {
        lock (_gate)
        {
            _passwordAfterNextCheck = newPassword;
        }
    }

    /// <summary>Adds an account directly, for arranging a test.</summary>
    public Account Add(string email, string password, Role role = Role.RegisteredUser, SignInBlock? block = null)
    {
        if (!Email.TryParse(email, out var parsed))
        {
            throw new ArgumentException("Not a valid email.", nameof(email));
        }

        lock (_gate)
        {
            var account = new Account(new AccountId(Guid.NewGuid()), parsed, role, block ?? SignInBlock.None);
            _byEmail.Add(parsed, new Entry(account, password));
            return account;
        }
    }

    /// <summary>Sets the stored block state, for arranging a test.</summary>
    public void SetSignInBlock(AccountId id, SignInBlock block)
    {
        lock (_gate)
        {
            var entry = FindEntry(id) ?? throw new InvalidOperationException("No such account.");
            entry.Account = entry.Account with { SignInBlock = block };
        }
    }

    /// <summary>The stored account with this email, or null.</summary>
    public Account? Find(string email)
    {
        if (!Email.TryParse(email, out var parsed))
        {
            return null;
        }

        lock (_gate)
        {
            return _byEmail.TryGetValue(parsed, out var entry) ? entry.Account : null;
        }
    }

    /// <summary>The stored password of this account, or null.</summary>
    public string? PasswordOf(AccountId id)
    {
        lock (_gate)
        {
            return FindEntry(id)?.Password;
        }
    }

    public Task<Account?> FindByEmailAsync(Email email, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            return Task.FromResult(_byEmail.TryGetValue(email, out var entry) ? entry.Account : null);
        }
    }

    public Task<Account?> FindByIdAsync(AccountId id, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            return Task.FromResult(FindEntry(id)?.Account);
        }
    }

    public Task<CreateAccountOutcome> CreateAsync(Email email, Role role, string password, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            CreateCount++;
            if (_byEmail.ContainsKey(email))
            {
                return Task.FromResult(CreateAccountOutcome.EmailTaken);
            }

            var account = new Account(new AccountId(Guid.NewGuid()), email, role, SignInBlock.None);
            _byEmail.Add(email, new Entry(account, password));

            if (_takeEmailOnNextCreate)
            {
                _takeEmailOnNextCreate = false;
                return Task.FromResult(CreateAccountOutcome.EmailTaken);
            }

            return Task.FromResult(CreateAccountOutcome.Created(account));
        }
    }

    public Task<PasswordVersion?> CheckPasswordAsync(AccountId id, string password, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            PasswordCheckCount++;
            var entry = FindEntry(id);
            var version = entry is not null && string.Equals(entry.Password, password, StringComparison.Ordinal)
                ? entry.PasswordVersion
                : null;

            if (entry is not null && _passwordAfterNextCheck is { } changed)
            {
                _passwordAfterNextCheck = null;
                entry.ChangePassword(changed);
            }

            return Task.FromResult(version);
        }
    }

    public Task<bool> ChangePasswordAsync(AccountId id, string currentPassword, string newPassword, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            PasswordChangeCount++;
            var entry = FindEntry(id);
            if (entry is null || !string.Equals(entry.Password, currentPassword, StringComparison.Ordinal))
            {
                return Task.FromResult(false);
            }

            entry.ChangePassword(newPassword);
            return Task.FromResult(true);
        }
    }

    public Task<bool> TrySaveSignInBlockAsync(
        AccountId id, SignInBlock expected, SignInBlock next, PasswordVersion? passwordVersion, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            SaveCount++;
            var entry = FindEntry(id);
            if (entry is null)
            {
                return Task.FromResult(false);
            }

            if (_blockOnNextSave is { } at)
            {
                _blockOnNextSave = null;
                entry.Account = entry.Account with { SignInBlock = new SignInBlock(SignInBlock.FailuresToBlock, at) };
                return Task.FromResult(false);
            }

            if (_savesToLose > 0)
            {
                _savesToLose--;
                var moved = _nextLostSaveIsOne ? new SignInBlock(1, null) : SignInBlock.None;
                _nextLostSaveIsOne = !_nextLostSaveIsOne;
                entry.Account = entry.Account with { SignInBlock = moved };
                return Task.FromResult(false);
            }

            if (entry.Account.SignInBlock != expected
                || (passwordVersion is not null && passwordVersion != entry.PasswordVersion))
            {
                return Task.FromResult(false);
            }

            entry.Account = entry.Account with { SignInBlock = next };
            SuccessfulSaveCount++;
            return Task.FromResult(true);
        }
    }

    private Entry? FindEntry(AccountId id) => _byEmail.Values.FirstOrDefault(e => e.Account.Id == id);

    private sealed class Entry(Account account, string password)
    {
        private int _passwordChanges;

        public Account Account { get; set; } = account;

        public string Password { get; private set; } = password;

        /// <summary>Renewed by every password change, like the security stamp.</summary>
        public PasswordVersion PasswordVersion => new($"{Account.Id.Value:N}-{_passwordChanges}");

        public void ChangePassword(string newPassword)
        {
            Password = newPassword;
            _passwordChanges++;
        }
    }
}
