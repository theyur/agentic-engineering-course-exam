using HT.Application.Accounts;
using HT.Domain.Accounts;
using Microsoft.Extensions.DependencyInjection;

namespace HT.IntegrationTests.Accounts;

[Collection(DatabaseCollection.Name)]
public sealed class AccountStoreTests(DatabaseFixture database)
{
    private const string Password = "walking12";

    [Fact]
    public async Task Created_account_is_found_by_email_and_by_id()
    {
        await using var scope = database.CreateScope();
        var store = Store(scope);

        var created = await CreateAsync(store, "store-create@example.com", Role.Administrator);
        var byEmail = await store.FindByEmailAsync(EmailOf("store-create@example.com"), CancellationToken.None);
        var byId = await store.FindByIdAsync(created.Id, CancellationToken.None);

        Assert.Equal(created, byEmail);
        Assert.Equal(created, byId);
        Assert.Equal(Role.Administrator, created.Role);
        Assert.Equal(SignInBlock.None, created.SignInBlock);
    }

    [Fact]
    public async Task Every_name_column_holds_the_normal_form()
    {
        await using var scope = database.CreateScope();
        var store = Store(scope);

        await store.CreateAsync(EmailOf("  Store-Normal@Example.COM "), Role.RegisteredUser, Password, CancellationToken.None);
        var row = await Sql.ReadUserAsync(database.ConnectionString, "store-normal@example.com");

        Assert.NotNull(row);
        Assert.Equal("store-normal@example.com", row["UserName"]);
        Assert.Equal("store-normal@example.com", row["Email"]);
        Assert.Equal("store-normal@example.com", row["NormalizedUserName"]);
        Assert.Equal("store-normal@example.com", row["NormalizedEmail"]);
        Assert.Equal("RegisteredUser", row["Role"]);
    }

    [Fact]
    public async Task A_second_create_with_the_same_email_gives_EmailTaken()
    {
        await using var scope = database.CreateScope();
        var store = Store(scope);

        await CreateAsync(store, "store-twice@example.com");
        var second = await store.CreateAsync(EmailOf("STORE-TWICE@example.com"), Role.RegisteredUser, Password, CancellationToken.None);

        Assert.True(second.IsEmailTaken);
        Assert.Equal(1, await Sql.CountUsersAsync(database.ConnectionString, "store-twice@example.com"));
    }

    [Fact]
    public async Task An_apostrophe_email_and_a_254_character_email_are_created()
    {
        await using var scope = database.CreateScope();
        var store = Store(scope);
        var longEmail = new string('l', 254 - "@example.com".Length) + "@example.com";

        var apostrophe = await CreateAsync(store, "o'neil@example.com");
        var longest = await CreateAsync(store, longEmail);

        Assert.Equal("o'neil@example.com", apostrophe.Email.Value);
        Assert.Equal(254, longest.Email.Value.Length);
        Assert.Equal(longest, await store.FindByEmailAsync(EmailOf(longEmail), CancellationToken.None));
    }

    [Fact]
    public async Task CheckPasswordAsync_accepts_only_the_right_password()
    {
        await using var scope = database.CreateScope();
        var store = Store(scope);
        var account = await CreateAsync(store, "store-check@example.com");

        Assert.NotNull(await store.CheckPasswordAsync(account.Id, Password, CancellationToken.None));
        Assert.Null(await store.CheckPasswordAsync(account.Id, "walking13", CancellationToken.None));
    }

    [Fact]
    public async Task TrySaveSignInBlockAsync_saves_on_the_stored_state_and_refuses_a_stale_one()
    {
        await using var scope = database.CreateScope();
        var store = Store(scope);
        var account = await CreateAsync(store, "store-cas@example.com");
        var blockedAt = database.Clock.GetUtcNow();

        Assert.True(await store.TrySaveSignInBlockAsync(account.Id, SignInBlock.None, new SignInBlock(4, null), null, CancellationToken.None));
        Assert.False(await store.TrySaveSignInBlockAsync(account.Id, SignInBlock.None, new SignInBlock(1, null), null, CancellationToken.None));
        Assert.True(await store.TrySaveSignInBlockAsync(account.Id, new SignInBlock(4, null), new SignInBlock(5, blockedAt), null, CancellationToken.None));
        Assert.False(await store.TrySaveSignInBlockAsync(account.Id, new SignInBlock(4, null), SignInBlock.None, null, CancellationToken.None));
        Assert.True(await store.TrySaveSignInBlockAsync(account.Id, new SignInBlock(5, blockedAt), SignInBlock.None, null, CancellationToken.None));
    }

    [Fact]
    public async Task HP_007_a_save_bound_to_a_password_version_is_refused_after_a_password_change()
    {
        await using var scope = database.CreateScope();
        var store = Store(scope);
        var account = await CreateAsync(store, "store-version@example.com");

        var checkedVersion = await store.CheckPasswordAsync(account.Id, Password, CancellationToken.None);
        Assert.NotNull(checkedVersion);
        Assert.True(await store.ChangePasswordAsync(account.Id, Password, "running34", CancellationToken.None));

        Assert.False(await store.TrySaveSignInBlockAsync(
            account.Id, SignInBlock.None, SignInBlock.AfterSuccess(), checkedVersion, CancellationToken.None));
        var newVersion = await store.CheckPasswordAsync(account.Id, "running34", CancellationToken.None);
        Assert.NotEqual(checkedVersion, newVersion);
        Assert.True(await store.TrySaveSignInBlockAsync(
            account.Id, SignInBlock.None, SignInBlock.AfterSuccess(), newVersion, CancellationToken.None));
    }

    [Fact]
    public async Task HP_007_a_sign_in_whose_password_changes_after_the_check_fails_and_saves_nothing()
    {
        await using var scope = database.CreateScope();
        var store = Store(scope);
        var account = await CreateAsync(store, "service-version@example.com");
        await store.TrySaveSignInBlockAsync(account.Id, SignInBlock.None, new SignInBlock(2, null), null, CancellationToken.None);
        var service = new AccountService(new PasswordChangingStore(store, account.Id, Password, "running34"), database.Clock);

        var result = await service.SignInAsync("service-version@example.com", Password, CancellationToken.None);

        Assert.Equal(SignInOutcome.Failed, result.Outcome);
        Assert.Null(result.ActingUser);
        var row = await Sql.ReadUserAsync(database.ConnectionString, "service-version@example.com");
        Assert.Equal(2, row!["FailedSignInCount"]);
    }

    [Fact]
    public async Task A_no_tracking_read_in_the_same_scope_sees_a_saved_block()
    {
        await using var scope = database.CreateScope();
        var store = Store(scope);
        var account = await CreateAsync(store, "store-fresh@example.com");
        var blockedAt = database.Clock.GetUtcNow();

        await store.FindByIdAsync(account.Id, CancellationToken.None);
        await store.TrySaveSignInBlockAsync(account.Id, SignInBlock.None, new SignInBlock(5, blockedAt), null, CancellationToken.None);
        var reread = await store.FindByIdAsync(account.Id, CancellationToken.None);

        Assert.Equal(new SignInBlock(5, blockedAt), reread!.SignInBlock);
    }

    [Fact]
    public async Task ChangePasswordAsync_leaves_the_stored_failure_count()
    {
        await using var scope = database.CreateScope();
        var store = Store(scope);
        var account = await CreateAsync(store, "store-change@example.com");
        await store.TrySaveSignInBlockAsync(account.Id, SignInBlock.None, new SignInBlock(3, null), null, CancellationToken.None);

        Assert.True(await store.ChangePasswordAsync(account.Id, Password, "running34", CancellationToken.None));
        Assert.False(await store.ChangePasswordAsync(account.Id, Password, "running56", CancellationToken.None));

        var row = await Sql.ReadUserAsync(database.ConnectionString, "store-change@example.com");
        Assert.Equal(3, row!["FailedSignInCount"]);
        Assert.NotNull(await store.CheckPasswordAsync(account.Id, "running34", CancellationToken.None));
    }

    [Fact]
    public async Task The_database_has_no_role_table()
    {
        Assert.True(await Sql.TableExistsAsync(database.ConnectionString, "AspNetUsers"));
        Assert.False(await Sql.TableExistsAsync(database.ConnectionString, "AspNetRoles"));
    }

    private static IAccountStore Store(AsyncServiceScope scope) => scope.ServiceProvider.GetRequiredService<IAccountStore>();

    /// <summary>
    /// The real store, with one interleaving forced: right after the first password check returns, the password
    /// is changed in the database, as another session's password change would (HP-007).
    /// </summary>
    private sealed class PasswordChangingStore(IAccountStore inner, AccountId id, string current, string changed) : IAccountStore
    {
        private bool _changed;

        public Task<Account?> FindByEmailAsync(Email email, CancellationToken cancellationToken) =>
            inner.FindByEmailAsync(email, cancellationToken);

        public Task<Account?> FindByIdAsync(AccountId accountId, CancellationToken cancellationToken) =>
            inner.FindByIdAsync(accountId, cancellationToken);

        public Task<CreateAccountOutcome> CreateAsync(Email email, Role role, string password, CancellationToken cancellationToken) =>
            inner.CreateAsync(email, role, password, cancellationToken);

        public async Task<PasswordVersion?> CheckPasswordAsync(AccountId accountId, string password, CancellationToken cancellationToken)
        {
            var version = await inner.CheckPasswordAsync(accountId, password, cancellationToken);
            if (!_changed)
            {
                _changed = true;
                Assert.True(await inner.ChangePasswordAsync(id, current, changed, cancellationToken));
            }

            return version;
        }

        public Task<bool> ChangePasswordAsync(AccountId accountId, string currentPassword, string newPassword, CancellationToken cancellationToken) =>
            inner.ChangePasswordAsync(accountId, currentPassword, newPassword, cancellationToken);

        public Task<bool> TrySaveSignInBlockAsync(
            AccountId accountId, SignInBlock expected, SignInBlock next, PasswordVersion? passwordVersion, CancellationToken cancellationToken) =>
            inner.TrySaveSignInBlockAsync(accountId, expected, next, passwordVersion, cancellationToken);
    }

    private static Email EmailOf(string text) =>
        Email.TryParse(text, out var email) ? email : throw new ArgumentException("Not a valid email.", nameof(text));

    private static async Task<Account> CreateAsync(IAccountStore store, string email, Role role = Role.RegisteredUser)
    {
        var outcome = await store.CreateAsync(EmailOf(email), role, Password, CancellationToken.None);
        return outcome.Account ?? throw new InvalidOperationException("The email was taken.");
    }
}
