using HT.Application.Accounts;
using HT.Domain.Accounts;
using Microsoft.Extensions.DependencyInjection;

namespace HT.IntegrationTests.Accounts;

/// <summary>US6-AS1 against SQL Server: the change, the security stamp and the stored failed count (research R-05).</summary>
[Collection(DatabaseCollection.Name)]
public sealed class PasswordChangeTests(DatabaseFixture database)
{
    [Fact]
    public async Task After_a_change_the_new_password_signs_in_and_the_old_one_fails()
    {
        var account = await CreateAsync("it.change@example.com");

        var result = await ChangeAsync(account, "walking42", "running42");

        Assert.Equal(PasswordChangeOutcome.Changed, result.Outcome);
        Assert.Equal(SignInOutcome.SignedIn, (await SignInAsync("it.change@example.com", "running42")).Outcome);
        Assert.Equal(SignInOutcome.Failed, (await SignInAsync("it.change@example.com", "walking42")).Outcome);
    }

    [Fact]
    public async Task A_change_renews_the_security_stamp()
    {
        var account = await CreateAsync("it.stamp@example.com");
        var before = await StampOfAsync("it.stamp@example.com");

        await ChangeAsync(account, "walking42", "running42");

        var after = await StampOfAsync("it.stamp@example.com");
        Assert.NotNull(after);
        Assert.NotEqual(before, after);
    }

    [Fact]
    public async Task A_refused_change_keeps_the_security_stamp()
    {
        var account = await CreateAsync("it.stamp.kept@example.com");
        var before = await StampOfAsync("it.stamp.kept@example.com");

        var result = await ChangeAsync(account, "wrong4242", "running42");

        Assert.Equal(PasswordChangeOutcome.CurrentPasswordWrong, result.Outcome);
        Assert.Equal(before, await StampOfAsync("it.stamp.kept@example.com"));
    }

    [Fact]
    public async Task A_stored_failed_count_of_3_is_still_3_after_a_change()
    {
        var account = await CreateAsync("it.change.count@example.com");
        for (var i = 0; i < 3; i++)
        {
            Assert.Equal(SignInOutcome.Failed, (await SignInAsync("it.change.count@example.com", "wrong4242")).Outcome);
        }

        var result = await ChangeAsync(account, "walking42", "running42");

        Assert.Equal(PasswordChangeOutcome.Changed, result.Outcome);
        var row = await Sql.ReadUserAsync(database.ConnectionString, "it.change.count@example.com");
        Assert.Equal(3, row!["FailedSignInCount"]);
    }

    private async Task<object?> StampOfAsync(string email) =>
        (await Sql.ReadUserAsync(database.ConnectionString, email))!["SecurityStamp"];

    private async Task<Account> CreateAsync(string email)
    {
        Assert.True(Email.TryParse(email, out var parsed));
        await using var scope = database.CreateScope();
        var outcome = await scope.ServiceProvider.GetRequiredService<IAccountStore>()
            .CreateAsync(parsed, Role.RegisteredUser, "walking42", CancellationToken.None);
        return outcome.Account ?? throw new InvalidOperationException($"{email} already has an account.");
    }

    private async Task<PasswordChangeResult> ChangeAsync(Account account, string current, string next)
    {
        await using var scope = database.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<AccountService>()
            .ChangePasswordAsync(ActingUser.ForAccount(account.Id, account.Role), current, next, CancellationToken.None);
    }

    private async Task<SignInResult> SignInAsync(string email, string password)
    {
        await using var scope = database.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<AccountService>()
            .SignInAsync(email, password, CancellationToken.None);
    }
}
