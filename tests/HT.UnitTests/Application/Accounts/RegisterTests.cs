using HT.Application.Accounts;
using HT.Domain.Accounts;
using HT.UnitTests.Fakes;
using Microsoft.Extensions.Time.Testing;

namespace HT.UnitTests.Application.Accounts;

public sealed class RegisterTests
{
    private readonly FakeAccountStore _store = new();
    private readonly AccountService _service;

    public RegisterTests()
    {
        _service = new AccountService(_store, new FakeTimeProvider(new DateTimeOffset(2026, 11, 11, 9, 0, 0, TimeSpan.Zero)));
    }

    [Fact]
    public async Task US1_AS1_a_new_email_and_a_valid_password_create_a_registered_user()
    {
        var result = await RegisterAsync("new.user@example.com", "walking42");

        Assert.Equal(RegistrationOutcome.Registered, result.Outcome);
        Assert.Null(result.Message);
        var account = _store.Find("new.user@example.com");
        Assert.NotNull(account);
        Assert.Equal(account.Id, result.AccountId);
        Assert.Equal(Role.RegisteredUser, account.Role);
        Assert.Equal("walking42", _store.PasswordOf(account.Id));
    }

    [Fact]
    public async Task US1_AS2_a_taken_email_in_another_form_is_refused()
    {
        _store.Add("taken@example.com", "walking12");

        var result = await RegisterAsync(" Taken@Example.com ", "walking42");

        Assert.Equal(RegistrationOutcome.EmailTaken, result.Outcome);
        Assert.Equal("An account with this email already exists.", result.Message);
        Assert.Null(result.AccountId);
        Assert.Single(_store.Accounts);
        Assert.Equal(0, _store.CreateCount);
    }

    [Theory]
    [InlineData("short.pw@example.com", "walk4")]
    [InlineData("no.digit@example.com", "walkingfar")]
    public async Task US1_AS3_AS4_a_password_that_fails_the_rule_is_refused(string email, string password)
    {
        var result = await RegisterAsync(email, password);

        Assert.Equal(RegistrationOutcome.PasswordRuleNotMet, result.Outcome);
        Assert.Equal("At least 8 characters, with a number.", result.Message);
        Assert.Null(result.AccountId);
        Assert.Empty(_store.Accounts);
    }

    [Theory]
    [InlineData("ana.example.com")]
    [InlineData("ana@")]
    [InlineData("ana@example")]
    [InlineData("ana@.")]
    [InlineData("ana @example.com")]
    [InlineData("")]
    [InlineData(null)]
    public async Task US1_AS5_an_invalid_email_is_refused(string? email)
    {
        var result = await RegisterAsync(email, "walking42");

        Assert.Equal(RegistrationOutcome.InvalidEmail, result.Outcome);
        Assert.Equal("Enter a valid email address.", result.Message);
        Assert.Null(result.AccountId);
        Assert.Empty(_store.Accounts);
    }

    [Fact]
    public async Task US1_AS6_the_password_rule_is_checked_before_the_taken_email()
    {
        _store.Add("taken2@example.com", "walking12");

        var result = await RegisterAsync("taken2@example.com", "walk4");

        Assert.Equal(RegistrationOutcome.PasswordRuleNotMet, result.Outcome);
        Assert.Equal("At least 8 characters, with a number.", result.Message);
    }

    [Fact]
    public async Task An_invalid_email_is_checked_before_the_password_rule()
    {
        var result = await RegisterAsync("ana@example", "walk4");

        Assert.Equal(RegistrationOutcome.InvalidEmail, result.Outcome);
    }

    [Fact]
    public async Task A_new_account_is_stored_in_normal_form_as_a_registered_user()
    {
        var result = await RegisterAsync("  Mixed.Case@Example.COM ", "walking42");

        Assert.Equal(RegistrationOutcome.Registered, result.Outcome);
        var account = Assert.Single(_store.Accounts);
        Assert.Equal("mixed.case@example.com", account.Email.Value);
        Assert.Equal(Role.RegisteredUser, account.Role);
        Assert.Equal(SignInBlock.None, account.SignInBlock);
    }

    [Fact]
    public async Task An_email_of_254_characters_is_accepted_and_one_of_255_refused()
    {
        var domain = "@example.com";
        var accepted = new string('a', Email.MaxLength - domain.Length) + domain;
        var refused = new string('b', Email.MaxLength + 1 - domain.Length) + domain;

        var first = await RegisterAsync(accepted, "walking42");
        var second = await RegisterAsync(refused, "walking42");

        Assert.Equal(RegistrationOutcome.Registered, first.Outcome);
        Assert.Equal(RegistrationOutcome.InvalidEmail, second.Outcome);
        Assert.Equal("Enter a valid email address.", second.Message);
    }

    [Fact]
    public async Task A_lost_race_gives_EmailTaken_and_leaves_one_account()
    {
        _store.TakeEmailOnNextCreate();

        var result = await RegisterAsync("race@example.com", "walking42");

        Assert.Equal(RegistrationOutcome.EmailTaken, result.Outcome);
        Assert.Equal("An account with this email already exists.", result.Message);
        Assert.Null(result.AccountId);
        Assert.Single(_store.Accounts, a => a.Email.Value == "race@example.com");
    }

    private Task<RegistrationResult> RegisterAsync(string? email, string? password) =>
        _service.RegisterAsync(email, password, CancellationToken.None);
}
