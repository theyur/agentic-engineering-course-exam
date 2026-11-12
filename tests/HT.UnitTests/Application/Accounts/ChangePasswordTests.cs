using HT.Application.Accounts;
using HT.Domain.Accounts;
using HT.UnitTests.Fakes;
using Microsoft.Extensions.Time.Testing;

namespace HT.UnitTests.Application.Accounts;

/// <summary>US6-AS1 to AS7, FR-010, FR-011, FR-013.</summary>
public sealed class ChangePasswordTests
{
    private readonly FakeAccountStore _store = new();
    private readonly AccountService _service;

    public ChangePasswordTests()
    {
        _service = new AccountService(_store, new FakeTimeProvider(new DateTimeOffset(2026, 11, 11, 9, 0, 0, TimeSpan.Zero)));
    }

    [Fact]
    public async Task US6_AS1_after_a_change_only_the_new_password_signs_in()
    {
        var account = _store.Add("change@example.com", "walking42");

        var result = await ChangeAsync(account, "walking42", "running42");

        Assert.Equal(PasswordChangeOutcome.Changed, result.Outcome);
        Assert.Null(result.Message);
        Assert.Equal(SignInOutcome.SignedIn, (await SignInAsync("change@example.com", "running42")).Outcome);
        Assert.Equal(SignInOutcome.Failed, (await SignInAsync("change@example.com", "walking42")).Outcome);
    }

    [Fact]
    public async Task US6_AS2_a_wrong_current_password_changes_nothing()
    {
        var account = _store.Add("wrong.current@example.com", "walking42");

        var result = await ChangeAsync(account, "wrong4242", "running42");

        Assert.Equal(PasswordChangeOutcome.CurrentPasswordWrong, result.Outcome);
        Assert.Equal("The current password is not correct.", result.Message);
        Assert.Equal("walking42", _store.PasswordOf(account.Id));
        Assert.Equal(0, _store.PasswordChangeCount);
    }

    [Fact]
    public async Task US6_AS3_a_new_password_that_breaks_the_rule_changes_nothing()
    {
        var account = _store.Add("weak.new@example.com", "walking42");

        var result = await ChangeAsync(account, "walking42", "runningfar");

        Assert.Equal(PasswordChangeOutcome.PasswordRuleNotMet, result.Outcome);
        Assert.Equal("At least 8 characters, with a number.", result.Message);
        Assert.Equal("walking42", _store.PasswordOf(account.Id));
        Assert.Equal(0, _store.PasswordChangeCount);
    }

    [Fact]
    public async Task US6_AS4_the_current_password_is_checked_before_the_rule()
    {
        var account = _store.Add("both.wrong@example.com", "walking42");

        var result = await ChangeAsync(account, "wrong4242", "run");

        Assert.Equal(PasswordChangeOutcome.CurrentPasswordWrong, result.Outcome);
        Assert.Equal("The current password is not correct.", result.Message);
    }

    [Fact]
    public async Task US6_AS5_an_anonymous_visitor_is_not_signed_in()
    {
        var result = await _service.ChangePasswordAsync(ActingUser.Anonymous, "walking42", "running42", CancellationToken.None);

        Assert.Equal(PasswordChangeOutcome.NotSignedIn, result.Outcome);
        Assert.Equal("You are not signed in.", result.Message);
        Assert.Equal(0, _store.PasswordCheckCount);
    }

    [Fact]
    public async Task Not_signed_in_is_checked_before_the_current_password_and_the_rule()
    {
        var result = await _service.ChangePasswordAsync(ActingUser.Anonymous, "wrong4242", "run", CancellationToken.None);

        Assert.Equal(PasswordChangeOutcome.NotSignedIn, result.Outcome);
    }

    [Fact]
    public async Task An_acting_user_whose_account_no_longer_exists_is_not_signed_in()
    {
        var gone = ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.RegisteredUser);

        var result = await _service.ChangePasswordAsync(gone, "walking42", "running42", CancellationToken.None);

        Assert.Equal(PasswordChangeOutcome.NotSignedIn, result.Outcome);
        Assert.Equal("You are not signed in.", result.Message);
        Assert.Equal(0, _store.PasswordCheckCount);
    }

    [Fact]
    public async Task US6_AS6_a_user_cannot_change_with_another_users_password()
    {
        _store.Add("erin@example.com", "walking42");
        var finn = _store.Add("finn@example.com", "cycling42");

        var result = await ChangeAsync(finn, "walking42", "running42");

        Assert.Equal(PasswordChangeOutcome.CurrentPasswordWrong, result.Outcome);
        Assert.Equal("The current password is not correct.", result.Message);
        Assert.Equal(SignInOutcome.SignedIn, (await SignInAsync("erin@example.com", "walking42")).Outcome);
        Assert.Equal(SignInOutcome.SignedIn, (await SignInAsync("finn@example.com", "cycling42")).Outcome);
    }

    [Fact]
    public async Task US6_AS7_an_administrator_cannot_change_with_a_users_password()
    {
        _store.Add("gale@example.com", "walking42");
        var admin = _store.Add("keeper.admin@example.com", "curator2026", Role.Administrator);

        var result = await ChangeAsync(admin, "walking42", "running42");

        Assert.Equal(PasswordChangeOutcome.CurrentPasswordWrong, result.Outcome);
        Assert.Equal("The current password is not correct.", result.Message);
        Assert.Equal(SignInOutcome.SignedIn, (await SignInAsync("gale@example.com", "walking42")).Outcome);
        Assert.Equal(SignInOutcome.SignedIn, (await SignInAsync("keeper.admin@example.com", "curator2026")).Outcome);
    }

    [Fact]
    public async Task Only_the_acting_users_password_changes()
    {
        var hana = _store.Add("hana@example.com", "walking42");
        var ivo = _store.Add("ivo@example.com", "walking42");

        await ChangeAsync(hana, "walking42", "running42");

        Assert.Equal("running42", _store.PasswordOf(hana.Id));
        Assert.Equal("walking42", _store.PasswordOf(ivo.Id));
    }

    [Fact]
    public async Task A_null_current_password_is_wrong()
    {
        var account = _store.Add("null.current@example.com", "walking42");

        var result = await _service.ChangePasswordAsync(
            ActingUser.ForAccount(account.Id, account.Role), null, "running42", CancellationToken.None);

        Assert.Equal(PasswordChangeOutcome.CurrentPasswordWrong, result.Outcome);
    }

    private Task<PasswordChangeResult> ChangeAsync(Account account, string current, string next) =>
        _service.ChangePasswordAsync(ActingUser.ForAccount(account.Id, account.Role), current, next, CancellationToken.None);

    private Task<SignInResult> SignInAsync(string email, string password) =>
        _service.SignInAsync(email, password, CancellationToken.None);
}
