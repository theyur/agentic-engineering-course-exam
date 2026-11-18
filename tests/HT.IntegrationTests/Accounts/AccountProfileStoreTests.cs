using HT.Application.Accounts;
using HT.Application.Goals;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using static HT.IntegrationTests.Goals.GoalSamples;

namespace HT.IntegrationTests.Accounts;

/// <summary>
/// The account profile columns on SQL Server (research R-04): written at creation, read back by both lookups, and never
/// rolled back by Identity's full-row update of a stale user. Own prefix "it-ap-".
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class AccountProfileStoreTests(DatabaseFixture database)
{
    private const string Password = "walking42";

    private static Email EmailOf(string text) =>
        Email.TryParse(text, out var email) ? email : throw new ArgumentException("Not a valid email.", nameof(text));

    [Fact]
    public async Task CreateAsync_stores_the_name_the_zone_and_the_creation_time_and_both_lookups_return_them()
    {
        const string email = "it-ap-profile@example.com";
        await Sql.DeleteUserAsync(database.ConnectionString, email);
        Assert.True(AccountName.TryCreate("  It-Ap Yuri  ", out var name, out _));
        var createdAt = new DateTimeOffset(2026, 11, 18, 10, 0, 0, TimeSpan.Zero);
        var profile = new NewAccountProfile(name, Zone("Europe/Kyiv"), createdAt);

        await using var scope = database.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IAccountStore>();
        var created = (await store.CreateAsync(EmailOf(email), Role.RegisteredUser, Password, profile, CancellationToken.None)).Account;

        Assert.NotNull(created);
        Assert.Equal(("Europe/Kyiv", "It-Ap Yuri", (DateTimeOffset?)createdAt), (created.StoredTimeZone, created.Name, created.CreatedAt));
        foreach (var found in new[]
                 {
                     await store.FindByIdAsync(created.Id, CancellationToken.None),
                     await store.FindByEmailAsync(EmailOf(email), CancellationToken.None),
                 })
        {
            Assert.NotNull(found);
            Assert.Equal(("Europe/Kyiv", "It-Ap Yuri", (DateTimeOffset?)createdAt), (found.StoredTimeZone, found.Name, found.CreatedAt));
        }

        var row = (await Sql.ReadUserAsync(database.ConnectionString, email))!;
        Assert.Equal(("Europe/Kyiv", "It-Ap Yuri", (object?)createdAt, (object?)0),
            ((string?)row["TimeZone"], (string?)row["Name"], row["CreatedAt"], row["GoalsRevision"]));
    }

    [Fact]
    public async Task No_name_is_stored_as_null()
    {
        const string email = "it-ap-no-name@example.com";
        await Sql.DeleteUserAsync(database.ConnectionString, email);
        await using var scope = database.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IAccountStore>();
        var created = (await store.CreateAsync(EmailOf(email), Role.RegisteredUser, Password, TestProfiles.Utc, CancellationToken.None)).Account!;

        var found = (await store.FindByIdAsync(created.Id, CancellationToken.None))!;
        Assert.Equal(("UTC", (string?)null), (found.StoredTimeZone, found.Name));
        Assert.Null((await Sql.ReadUserAsync(database.ConnectionString, email))!["Name"]);
    }

    [Theory]
    [InlineData("it-ap-register-kyiv@example.com", "Europe/Kyiv", "Europe/Kyiv")]
    [InlineData("it-ap-register-windows@example.com", "FLE Standard Time", "UTC")]
    public async Task Registration_stores_a_named_zone_and_the_default_for_any_other(string email, string given, string stored)
    {
        await Sql.DeleteUserAsync(database.ConnectionString, email);
        await using var scope = database.CreateScope();
        var now = database.Clock.GetUtcNow();

        var result = await scope.ServiceProvider.GetRequiredService<AccountService>()
            .RegisterAsync(email, Password, given, null, CancellationToken.None);

        Assert.Equal(RegistrationOutcome.Registered, result.Outcome);
        var account = (await scope.ServiceProvider.GetRequiredService<IAccountStore>().FindByIdAsync(result.AccountId!.Value, CancellationToken.None))!;
        Assert.Equal((stored, (string?)null, (DateTimeOffset?)now), (account.StoredTimeZone, account.Name, account.CreatedAt));
    }

    [Fact]
    public async Task A_password_change_of_a_user_loaded_before_a_goal_was_created_keeps_the_zone_and_the_revision()
    {
        const string email = "it-ap-stale@example.com";
        await Sql.DeleteUserAsync(database.ConnectionString, email);
        AccountId owner;
        await using (var scope = database.CreateScope())
        {
            owner = (await scope.ServiceProvider.GetRequiredService<IAccountStore>()
                .CreateAsync(EmailOf(email), Role.RegisteredUser, Password, TestProfiles.Utc, CancellationToken.None)).Account!.Id;
        }

        await SetZoneAsync(database.ConnectionString, owner, null);
        var habit = await CatalogHabitAsync(database.Services, "It-Ap Stale catalog");

        await using var stale = database.CreateScope();
        var users = stale.ServiceProvider.GetRequiredService<UserManager<HtUser>>();
        var user = (await users.FindByIdAsync(owner.Value.ToString()))!;
        Assert.Null(user.TimeZone);
        Assert.Equal(0, user.GoalsRevision);

        await using (var other = database.CreateScope())
        {
            var outcome = await other.ServiceProvider.GetRequiredService<IGoalStore>().CreateAsync(
                owner,
                new NewGoal(new GoalHabit(habit, HabitKind.Catalog), new DateOnly(2026, 11, 18), null, YesNoOn(DayOfWeek.Monday)),
                Zone("Europe/Kyiv"),
                Zone("Europe/Kyiv"),
                CancellationToken.None);
            Assert.Equal(GoalWriteOutcomeKind.Created, outcome.Kind);
        }

        var changed = await users.ChangePasswordAsync(user, Password, "running34");

        Assert.True(changed.Succeeded);
        Assert.Equal(((string?)"Europe/Kyiv", 1), await AccountStateAsync(database.ConnectionString, owner));
        await using var check = database.CreateScope();
        Assert.NotNull(await check.ServiceProvider.GetRequiredService<IAccountStore>().CheckPasswordAsync(owner, "running34", CancellationToken.None));
    }
}
