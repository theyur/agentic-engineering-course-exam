using HT.Application.Accounts;
using HT.Domain.Accounts;
using Microsoft.Extensions.DependencyInjection;

namespace HT.IntegrationTests.Accounts;

[Collection(DatabaseCollection.Name)]
public sealed class SignInBlockTests(DatabaseFixture database)
{
    private const string Password = "walking42";

    [Fact]
    public async Task Ten_parallel_wrong_attempts_count_five_and_block_until_exactly_five_minutes_later()
    {
        const string email = "parallel.block@example.com";
        await RegisterAsync(email);
        var now = database.Clock.GetUtcNow();

        var attempts = Enumerable.Range(0, 10).Select(_ => Task.Run(async () =>
        {
            await using var scope = database.CreateScope();
            return await Service(scope).SignInAsync(email, "walking43", CancellationToken.None);
        }));
        var results = await Task.WhenAll(attempts);

        Assert.All(results, r => Assert.Equal(SignInOutcome.Failed, r.Outcome));
        var row = await Sql.ReadUserAsync(database.ConnectionString, email);
        Assert.NotNull(row);
        Assert.Equal(SignInBlock.FailuresToBlock, row["FailedSignInCount"]);
        Assert.Equal(now, Assert.IsType<DateTimeOffset>(row["SignInBlockedSince"]));

        await using (var scope = database.CreateScope())
        {
            var blocked = await Service(scope).SignInAsync(email, Password, CancellationToken.None);
            Assert.Equal(SignInOutcome.Failed, blocked.Outcome);
        }

        database.Clock.Advance(SignInBlock.Duration);
        await using (var scope = database.CreateScope())
        {
            var ended = await Service(scope).SignInAsync(email, Password, CancellationToken.None);
            Assert.Equal(SignInOutcome.SignedIn, ended.Outcome);
        }
    }

    [Fact]
    public async Task US2_AS1_an_upper_case_email_signs_in_to_the_lower_case_account()
    {
        var accountId = await RegisterAsync("ana@example.com");

        await using var scope = database.CreateScope();
        var result = await Service(scope).SignInAsync("ANA@example.com", Password, CancellationToken.None);

        Assert.Equal(SignInOutcome.SignedIn, result.Outcome);
        Assert.Equal(accountId, result.ActingUser!.AccountId);
        Assert.Equal(Role.RegisteredUser, result.ActingUser.Role);
    }

    private static AccountService Service(AsyncServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<AccountService>();

    private async Task<AccountId> RegisterAsync(string email)
    {
        await using var scope = database.CreateScope();
        var result = await Service(scope).RegisterAsync(email, Password, CancellationToken.None);
        return result.AccountId ?? throw new InvalidOperationException($"{email} was not registered: {result.Outcome}.");
    }
}
