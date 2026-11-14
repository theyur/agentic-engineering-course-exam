using HT.Application.Accounts;
using HT.Domain.Accounts;
using Microsoft.Extensions.DependencyInjection;

namespace HT.IntegrationTests.Habits;

/// <summary>Accounts for the custom habit tests, created through the public account store port.</summary>
public static class HabitAccounts
{
    public const string Password = "walking42";

    /// <summary>Creates a registered user with this email (deleting an earlier one) and returns it as an acting user.</summary>
    public static async Task<ActingUser> RegisteredUserAsync(IServiceProvider services, string connectionString, string email)
    {
        await Sql.DeleteUserAsync(connectionString, email);
        Assert.True(Email.TryParse(email, out var parsed));
        await using var scope = services.CreateAsyncScope();
        var created = await scope.ServiceProvider.GetRequiredService<IAccountStore>().CreateAsync(parsed, Role.RegisteredUser, Password, CancellationToken.None);
        var account = created.Account ?? throw new InvalidOperationException("The test email is taken.");
        return ActingUser.ForAccount(account.Id, account.Role);
    }
}
