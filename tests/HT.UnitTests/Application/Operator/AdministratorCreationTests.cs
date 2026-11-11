using HT.Application.Accounts;
using HT.Application.Operator;
using HT.Domain.Accounts;
using HT.UnitTests.Fakes;
using Microsoft.Extensions.Time.Testing;

namespace HT.UnitTests.Application.Operator;

public sealed class AdministratorCreationTests
{
    private readonly FakeAccountStore _store = new();
    private readonly OperatorService _operator;
    private readonly AccountService _accounts;

    public AdministratorCreationTests()
    {
        _operator = new OperatorService(_store);
        _accounts = new AccountService(_store, new FakeTimeProvider(new DateTimeOffset(2026, 11, 11, 9, 0, 0, TimeSpan.Zero)));
    }

    [Fact]
    public async Task US4_AS1_a_new_email_creates_an_administrator_who_signs_in()
    {
        var result = await CreateAsync("first.admin@example.com", "curator2026");

        AssertResult(result, AdministratorCreationOutcome.Created, "administrator created");
        var signIn = await _accounts.SignInAsync("first.admin@example.com", "curator2026", CancellationToken.None);
        Assert.Equal(SignInOutcome.SignedIn, signIn.Outcome);
        Assert.Equal(Role.Administrator, signIn.ActingUser!.Role);
    }

    [Fact]
    public async Task US4_AS2_an_existing_administrator_keeps_their_password()
    {
        var admin = _store.Add("second.admin@example.com", "curator2026", Role.Administrator);

        var result = await CreateAsync("second.admin@example.com", "other2026");

        AssertResult(result, AdministratorCreationOutcome.AlreadyExists, "administrator already exists");
        Assert.Equal("curator2026", _store.PasswordOf(admin.Id));
        Assert.Equal(SignInOutcome.SignedIn, (await _accounts.SignInAsync("second.admin@example.com", "curator2026", CancellationToken.None)).Outcome);
        Assert.Equal(SignInOutcome.Failed, (await _accounts.SignInAsync("second.admin@example.com", "other2026", CancellationToken.None)).Outcome);
        Assert.Equal(0, _store.CreateCount);
        Assert.Equal(0, _store.PasswordChangeCount);
    }

    [Fact]
    public async Task US4_AS3_a_registered_user_email_stays_a_registered_user()
    {
        _store.Add("claim@example.com", "walking42");

        var result = await CreateAsync("claim@example.com", "curator2026");

        AssertResult(result, AdministratorCreationOutcome.BelongsToRegisteredUser, "email belongs to a registered user");
        Assert.Equal(Role.RegisteredUser, _store.Find("claim@example.com")!.Role);
        Assert.Equal(0, _store.CreateCount);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("   ", "   ")]
    [InlineData(null, "curator2026")]
    [InlineData("", "curator2026")]
    [InlineData("   ", "curator2026")]
    [InlineData("admin@example.com", null)]
    [InlineData("admin@example.com", "")]
    [InlineData("admin@example.com", "   ")]
    [InlineData("admin.example.com", null)]
    public async Task US4_AS4_a_missing_email_or_password_is_not_configured(string? email, string? password)
    {
        var result = await CreateAsync(email, password);

        AssertResult(result, AdministratorCreationOutcome.NotConfigured, "no administrator configured");
        Assert.Empty(_store.Accounts);
    }

    [Fact]
    public async Task US4_AS5_a_password_of_spaces_is_not_configured_and_the_email_stays_free()
    {
        var result = await CreateAsync("fifth.admin@example.com", "        ");

        AssertResult(result, AdministratorCreationOutcome.NotConfigured, "no administrator configured");
        var registered = await _accounts.RegisterAsync("fifth.admin@example.com", "walking42", CancellationToken.None);
        Assert.Equal(RegistrationOutcome.Registered, registered.Outcome);
    }

    [Fact]
    public async Task US4_AS6_an_invalid_email_is_refused()
    {
        var result = await CreateAsync("admin.example.com", "curator2026");

        AssertResult(result, AdministratorCreationOutcome.InvalidEmail, "configured email is not valid");
        Assert.Empty(_store.Accounts);
    }

    [Fact]
    public async Task An_invalid_email_is_checked_before_the_password_rule()
    {
        var result = await CreateAsync("admin.example.com", "curatorship");

        Assert.Equal(AdministratorCreationOutcome.InvalidEmail, result.Outcome);
    }

    [Fact]
    public async Task US4_AS7_a_password_that_fails_the_rule_is_refused_and_the_email_stays_free()
    {
        var result = await CreateAsync("seventh.admin@example.com", "curatorship");

        AssertResult(result, AdministratorCreationOutcome.PasswordRuleNotMet, "configured password does not meet the password rule");
        Assert.Empty(_store.Accounts);
        var registered = await _accounts.RegisterAsync("seventh.admin@example.com", "walking42", CancellationToken.None);
        Assert.Equal(RegistrationOutcome.Registered, registered.Outcome);
    }

    [Fact]
    public async Task US4_AS8_an_existing_administrator_is_found_before_the_password_rule()
    {
        _store.Add("eighth.admin@example.com", "curator2026", Role.Administrator);

        var result = await CreateAsync("eighth.admin@example.com", "curatorship");

        AssertResult(result, AdministratorCreationOutcome.AlreadyExists, "administrator already exists");
    }

    [Fact]
    public async Task A_registered_user_is_found_before_the_password_rule()
    {
        _store.Add("claim2@example.com", "walking42");

        var result = await CreateAsync("claim2@example.com", "curatorship");

        Assert.Equal(AdministratorCreationOutcome.BelongsToRegisteredUser, result.Outcome);
    }

    [Fact]
    public async Task US4_AS9_a_new_email_adds_a_second_administrator()
    {
        _store.Add("ninth.admin@example.com", "curator2026", Role.Administrator);

        var result = await CreateAsync("tenth.admin@example.com", "curator2026");

        AssertResult(result, AdministratorCreationOutcome.Created, "administrator created");
        Assert.Equal(Role.Administrator, _store.Find("ninth.admin@example.com")!.Role);
        Assert.Equal(Role.Administrator, _store.Find("tenth.admin@example.com")!.Role);
    }

    [Fact]
    public async Task The_email_is_stored_in_normal_form()
    {
        await CreateAsync("  Lead.Admin@Example.COM ", "curator2026");

        var account = Assert.Single(_store.Accounts);
        Assert.Equal("lead.admin@example.com", account.Email.Value);
    }

    [Fact]
    public async Task A_lost_race_for_the_email_creates_nothing_more()
    {
        _store.TakeEmailOnNextCreate();

        var result = await CreateAsync("race.admin@example.com", "curator2026");

        Assert.NotEqual(AdministratorCreationOutcome.Created, result.Outcome);
        Assert.Single(_store.Accounts);
    }

    private Task<AdministratorCreationResult> CreateAsync(string? email, string? password) =>
        _operator.CreateAdministratorAsync(email, password, CancellationToken.None);

    private static void AssertResult(AdministratorCreationResult result, AdministratorCreationOutcome outcome, string message)
    {
        Assert.Equal(outcome, result.Outcome);
        Assert.Equal(message, result.Message);
    }
}
