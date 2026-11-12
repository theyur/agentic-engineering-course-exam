using HT.Application.Accounts;
using Microsoft.Extensions.DependencyInjection;

namespace HT.IntegrationTests.Accounts;

[Collection(DatabaseCollection.Name)]
public sealed class RegistrationTests(DatabaseFixture database)
{
    [Fact]
    public async Task Registering_stores_the_email_in_normal_form_with_the_registered_user_role()
    {
        await using var scope = database.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<AccountService>();

        var result = await service.RegisterAsync("Mixed.Case@Example.com", "walking42", CancellationToken.None);

        Assert.Equal(RegistrationOutcome.Registered, result.Outcome);
        Assert.NotNull(result.AccountId);
        var row = await Sql.ReadUserAsync(database.ConnectionString, "mixed.case@example.com");
        Assert.NotNull(row);
        Assert.Equal("mixed.case@example.com", row["Email"]);
        Assert.Equal("RegisteredUser", row["Role"]);
        Assert.Equal(result.AccountId.Value.Value, row["Id"]);
    }

    [Fact]
    public async Task HP_020_two_registrations_of_one_email_started_together_leave_exactly_one_account()
    {
        await using var first = database.CreateScope();
        await using var second = database.CreateScope();
        var one = first.ServiceProvider.GetRequiredService<AccountService>();
        var two = second.ServiceProvider.GetRequiredService<AccountService>();

        var results = await Task.WhenAll(
            Task.Run(() => one.RegisterAsync("parallel.register@example.com", "walking42", CancellationToken.None)),
            Task.Run(() => two.RegisterAsync("parallel.register@example.com", "walking42", CancellationToken.None)));

        Assert.Single(results, r => r.Outcome == RegistrationOutcome.Registered);
        var taken = Assert.Single(results, r => r.Outcome == RegistrationOutcome.EmailTaken);
        Assert.Equal(AccountTexts.EmailTaken, taken.Message);
        Assert.Equal(1, await Sql.CountUsersAsync(database.ConnectionString, "parallel.register@example.com"));
    }
}
