using HT.Application.Accounts;
using HT.Application.Operator;
using HT.Domain.Accounts;
using Microsoft.Extensions.DependencyInjection;

namespace HT.IntegrationTests.Operator;

[Collection(DatabaseCollection.Name)]
public sealed class AdministratorCreationTests(DatabaseFixture database)
{
    [Fact]
    public async Task A_created_administrator_signs_in_with_the_administrator_role()
    {
        var created = await CreateAdministratorAsync("it.first.admin@example.com", "curator2026");

        Assert.Equal(AdministratorCreationOutcome.Created, created.Outcome);
        var signIn = await SignInAsync("it.first.admin@example.com", "curator2026");
        Assert.Equal(SignInOutcome.SignedIn, signIn.Outcome);
        Assert.Equal(Role.Administrator, signIn.ActingUser!.Role);
    }

    [Fact]
    public async Task A_second_run_changes_nothing_and_the_first_password_still_signs_in()
    {
        await CreateAdministratorAsync("it.second.admin@example.com", "curator2026");

        var second = await CreateAdministratorAsync("it.second.admin@example.com", "other2026");

        Assert.Equal(AdministratorCreationOutcome.AlreadyExists, second.Outcome);
        Assert.Equal(OperatorTexts.AdministratorAlreadyExists, second.Message);
        Assert.Equal(SignInOutcome.SignedIn, (await SignInAsync("it.second.admin@example.com", "curator2026")).Outcome);
        Assert.Equal(SignInOutcome.Failed, (await SignInAsync("it.second.admin@example.com", "other2026")).Outcome);
        Assert.Equal(1, await Sql.CountUsersAsync(database.ConnectionString, "it.second.admin@example.com"));
    }

    [Fact]
    public async Task A_new_email_adds_a_second_administrator_and_both_keep_the_role()
    {
        await CreateAdministratorAsync("it.ninth.admin@example.com", "curator2026");

        var tenth = await CreateAdministratorAsync("it.tenth.admin@example.com", "curator2026");

        Assert.Equal(AdministratorCreationOutcome.Created, tenth.Outcome);
        Assert.Equal("Administrator", (await Sql.ReadUserAsync(database.ConnectionString, "it.ninth.admin@example.com"))!["Role"]);
        Assert.Equal("Administrator", (await Sql.ReadUserAsync(database.ConnectionString, "it.tenth.admin@example.com"))!["Role"]);
    }

    private async Task<AdministratorCreationResult> CreateAdministratorAsync(string email, string password)
    {
        await using var scope = database.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<OperatorService>()
            .CreateAdministratorAsync(email, password, CancellationToken.None);
    }

    private async Task<SignInResult> SignInAsync(string email, string password)
    {
        await using var scope = database.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<AccountService>()
            .SignInAsync(email, password, CancellationToken.None);
    }
}
