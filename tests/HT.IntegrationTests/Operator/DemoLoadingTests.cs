using HT.Application.Accounts;
using HT.Application.Operator;
using HT.Domain.Accounts;
using Microsoft.Extensions.DependencyInjection;

namespace HT.IntegrationTests.Operator;

/// <summary>US7-AS1, US7-AS2, SC-006 on the real database.</summary>
[Collection(DatabaseCollection.Name)]
public sealed class DemoLoadingTests(DatabaseFixture database)
{
    private const string DemoPassword = "example2026";

    [Fact]
    public async Task A_second_load_changes_nothing_and_a_changed_password_still_signs_in()
    {
        // The demo emails are fixed, so clear them first: the test must not depend on other tests.
        foreach (var email in DemoDataService.DemoEmails)
        {
            await Sql.DeleteUserAsync(database.ConnectionString, email);
        }

        var first = await LoadAsync();
        Assert.Equal(DemoLoadingOutcome.Loaded, first.Outcome);
        Assert.All(first.Accounts, a => Assert.Equal(DemoAccountOutcome.Created, a.Outcome));
        foreach (var email in DemoDataService.DemoEmails)
        {
            var signIn = await SignInAsync(email, DemoPassword);
            Assert.Equal(SignInOutcome.SignedIn, signIn.Outcome);
            Assert.Equal(Role.RegisteredUser, signIn.ActingUser!.Role);
        }

        await ChangePasswordAsync("demo3@example.com", DemoPassword, "changed2026");

        var second = await LoadAsync();

        Assert.Equal(DemoLoadingOutcome.Loaded, second.Outcome);
        Assert.Equal(DemoDataService.DemoEmails, second.Accounts.Select(a => a.Email));
        Assert.All(second.Accounts, a =>
        {
            Assert.Equal(DemoAccountOutcome.AlreadyExists, a.Outcome);
            Assert.Equal(OperatorTexts.DemoAccountAlreadyExists, a.Message);
        });
        foreach (var email in DemoDataService.DemoEmails)
        {
            Assert.Equal(1, await Sql.CountUsersAsync(database.ConnectionString, email));
        }

        Assert.Equal(SignInOutcome.SignedIn, (await SignInAsync("demo3@example.com", "changed2026")).Outcome);
        Assert.Equal(SignInOutcome.Failed, (await SignInAsync("demo3@example.com", DemoPassword)).Outcome);
        Assert.Equal(SignInOutcome.SignedIn, (await SignInAsync("demo1@example.com", DemoPassword)).Outcome);
    }

    private async Task<DemoLoadingResult> LoadAsync()
    {
        await using var scope = database.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<DemoDataService>().LoadAsync(DemoPassword, CancellationToken.None);
    }

    private async Task<SignInResult> SignInAsync(string email, string password)
    {
        await using var scope = database.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<AccountService>()
            .SignInAsync(email, password, CancellationToken.None);
    }

    private async Task ChangePasswordAsync(string email, string current, string next)
    {
        await using var scope = database.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IAccountStore>();
        Assert.True(Email.TryParse(email, out var parsed));
        var account = await store.FindByEmailAsync(parsed, CancellationToken.None);
        Assert.NotNull(account);
        Assert.True(await store.ChangePasswordAsync(account.Id, current, next, CancellationToken.None));
    }
}
