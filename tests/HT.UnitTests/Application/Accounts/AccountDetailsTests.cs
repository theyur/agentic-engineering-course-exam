using System.Reflection;
using HT.Application.Accounts;
using HT.Domain.Accounts;
using HT.UnitTests.Fakes;
using Microsoft.Extensions.Time.Testing;

namespace HT.UnitTests.Application.Accounts;

public sealed class AccountDetailsTests
{
    private readonly FakeAccountStore _store = new();
    private readonly AccountService _service;

    public AccountDetailsTests()
    {
        _service = new AccountService(_store, new FakeTimeProvider(new DateTimeOffset(2026, 11, 11, 9, 0, 0, TimeSpan.Zero)));
    }

    [Fact]
    public async Task US3_AS1_an_anonymous_visitor_is_not_signed_in()
    {
        var result = await _service.GetAccountDetailsAsync(ActingUser.Anonymous, CancellationToken.None);

        Assert.Equal(AccountDetailsOutcome.NotSignedIn, result.Outcome);
        Assert.Equal("You are not signed in.", result.Message);
        Assert.Null(result.AccountDetails);
    }

    [Fact]
    public async Task US3_AS2_a_registered_user_gets_their_email_and_role()
    {
        var account = _store.Add("details@example.com", "walking42");

        var result = await DetailsOf(account);

        Assert.Equal(AccountDetailsOutcome.Found, result.Outcome);
        Assert.Null(result.Message);
        Assert.Equal(new AccountDetails("details@example.com", Role.RegisteredUser), result.AccountDetails);
    }

    [Fact]
    public async Task US3_AS3_an_administrator_gets_their_email_and_role()
    {
        var account = _store.Add("lead.admin@example.com", "walking42", Role.Administrator);

        var result = await DetailsOf(account);

        Assert.Equal(new AccountDetails("lead.admin@example.com", Role.Administrator), result.AccountDetails);
    }

    [Fact]
    public async Task US3_AS4_a_registered_user_gets_only_their_own_details()
    {
        _store.Add("cara@example.com", "walking42");
        var ben = _store.Add("ben@example.com", "walking42");

        var result = await DetailsOf(ben);

        Assert.Equal(new AccountDetails("ben@example.com", Role.RegisteredUser), result.AccountDetails);
    }

    [Fact]
    public async Task US3_AS5_an_administrator_gets_only_their_own_details()
    {
        var admin = _store.Add("audit.admin@example.com", "walking42", Role.Administrator);
        _store.Add("dana@example.com", "walking42");

        var result = await DetailsOf(admin);

        Assert.Equal(new AccountDetails("audit.admin@example.com", Role.Administrator), result.AccountDetails);
    }

    [Fact]
    public async Task An_acting_user_whose_account_no_longer_exists_is_not_signed_in()
    {
        var gone = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.RegisteredUser);

        var result = await _service.GetAccountDetailsAsync(gone, CancellationToken.None);

        Assert.Equal(AccountDetailsOutcome.NotSignedIn, result.Outcome);
        Assert.Equal(AccountTexts.NotSignedIn, result.Message);
        Assert.Null(result.AccountDetails);
    }

    [Fact]
    public void SC_005_no_public_method_of_AccountService_takes_an_account_identifier()
    {
        Type[] identifiers = [typeof(AccountId), typeof(AccountId?), typeof(Guid), typeof(Guid?)];

        var offenders = typeof(AccountService)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .SelectMany(m => m.GetParameters().Select(p => (Method: m.Name, Parameter: p)))
            .Where(x => identifiers.Contains(x.Parameter.ParameterType))
            .Select(x => $"{x.Method}({x.Parameter.Name})")
            .ToList();

        Assert.Empty(offenders);
        Assert.Contains(typeof(AccountService).GetMethods(), m => m.Name == nameof(AccountService.GetAccountDetailsAsync));
    }

    private Task<AccountDetailsResult> DetailsOf(Account account) =>
        _service.GetAccountDetailsAsync(ActingUser.ForAccount(account.Id, account.Role), CancellationToken.None);
}
