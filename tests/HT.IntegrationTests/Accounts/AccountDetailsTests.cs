using HT.Application.Accounts;
using HT.Domain.Accounts;
using Microsoft.Extensions.DependencyInjection;

namespace HT.IntegrationTests.Accounts;

[Collection(DatabaseCollection.Name)]
public sealed class AccountDetailsTests(DatabaseFixture database)
{
    private const string Password = "walking42";

    [Fact]
    public async Task US3_AS4_a_registered_user_gets_only_their_own_details()
    {
        await CreateAsync("Cara@Example.com", Role.RegisteredUser);
        var ben = await CreateAsync(" Ben@Example.com ", Role.RegisteredUser);

        var result = await DetailsOfAsync(ben);

        Assert.Equal(AccountDetailsOutcome.Found, result.Outcome);
        Assert.Equal(new AccountDetails("ben@example.com", Role.RegisteredUser), result.AccountDetails);
    }

    [Fact]
    public async Task US3_AS5_an_administrator_gets_only_their_own_details()
    {
        var admin = await CreateAsync("Audit.Admin@example.com", Role.Administrator);
        await CreateAsync("dana@example.com", Role.RegisteredUser);

        var result = await DetailsOfAsync(admin);

        Assert.Equal(AccountDetailsOutcome.Found, result.Outcome);
        Assert.Equal(new AccountDetails("audit.admin@example.com", Role.Administrator), result.AccountDetails);
    }

    private async Task<Account> CreateAsync(string email, Role role)
    {
        Assert.True(Email.TryParse(email, out var parsed));
        await using var scope = database.CreateScope();
        var outcome = await scope.ServiceProvider.GetRequiredService<IAccountStore>()
            .CreateAsync(parsed, role, Password, CancellationToken.None);
        return outcome.Account ?? throw new InvalidOperationException($"{email} already has an account.");
    }

    private async Task<AccountDetailsResult> DetailsOfAsync(Account account)
    {
        await using var scope = database.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<AccountService>()
            .GetAccountDetailsAsync(ActingUser.ForAccount(account.Id, account.Role), CancellationToken.None);
    }
}
