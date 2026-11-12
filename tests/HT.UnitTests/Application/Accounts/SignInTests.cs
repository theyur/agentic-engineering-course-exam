using HT.Application.Accounts;
using HT.Domain.Accounts;
using HT.UnitTests.Fakes;
using Microsoft.Extensions.Time.Testing;

namespace HT.UnitTests.Application.Accounts;

public sealed class SignInTests
{
    private const string Password = "walking42";
    private const string WrongPassword = "walking43";
    private const string FailedText = "Email or password is not correct.";

    private readonly FakeAccountStore _store = new();
    private readonly FakeTimeProvider _clock = new(At(9, 0, 0));
    private readonly AccountService _service;

    public SignInTests()
    {
        _service = new AccountService(_store, _clock);
    }

    [Fact]
    public async Task US2_AS1_an_email_in_another_form_signs_in_to_the_account_with_its_role()
    {
        var account = _store.Add("ana@example.com", Password, Role.Administrator);

        var result = await SignInAsync("ANA@example.com", Password);

        Assert.Equal(SignInOutcome.SignedIn, result.Outcome);
        Assert.Null(result.Message);
        Assert.NotNull(result.ActingUser);
        Assert.Equal(account.Id, result.ActingUser.AccountId);
        Assert.Equal(Role.Administrator, result.ActingUser.Role);
    }

    [Fact]
    public async Task US2_AS2_an_unknown_email_fails_and_saves_nothing()
    {
        var result = await SignInAsync("nobody@example.com", Password);

        AssertFailed(result);
        Assert.Equal(0, _store.SaveCount);
        Assert.Equal(0, _store.PasswordCheckCount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("ana@example")]
    public async Task An_invalid_email_fails_and_saves_nothing(string? email)
    {
        var result = await SignInAsync(email, Password);

        AssertFailed(result);
        Assert.Equal(0, _store.SaveCount);
    }

    [Fact]
    public async Task US2_AS3_a_wrong_password_fails_and_counts_one_failure()
    {
        _store.Add("wrong.pw@example.com", Password);

        var result = await SignInAsync("wrong.pw@example.com", WrongPassword);

        AssertFailed(result);
        Assert.Equal(1, _store.PasswordCheckCount);
        Assert.Equal(new SignInBlock(1, null), _store.Find("wrong.pw@example.com")!.SignInBlock);
    }

    [Fact]
    public async Task The_fifth_failure_in_a_row_starts_the_block_at_its_time()
    {
        _store.Add("five@example.com", Password);

        for (var second = 0; second < 5; second++)
        {
            _clock.SetUtcNow(At(10, 0, second));
            AssertFailed(await SignInAsync("five@example.com", WrongPassword));
        }

        Assert.Equal(new SignInBlock(5, At(10, 0, 4)), _store.Find("five@example.com")!.SignInBlock);
    }

    [Fact]
    public async Task US2_AS4_the_correct_password_fails_at_4_minutes_59_seconds_without_a_password_check()
    {
        _store.Add("block1@example.com", Password, block: new SignInBlock(5, At(10, 0, 0)));
        _clock.SetUtcNow(At(10, 4, 59));

        var result = await SignInAsync("block1@example.com", Password);

        AssertFailed(result);
        Assert.Equal(0, _store.PasswordCheckCount);
        Assert.Equal(0, _store.SaveCount);
        Assert.Equal(new SignInBlock(5, At(10, 0, 0)), _store.Find("block1@example.com")!.SignInBlock);
    }

    [Fact]
    public async Task US2_AS5_the_correct_password_succeeds_at_5_minutes_0_seconds()
    {
        _store.Add("block2@example.com", Password, block: new SignInBlock(5, At(10, 0, 0)));
        _clock.SetUtcNow(At(10, 5, 0));

        var result = await SignInAsync("block2@example.com", Password);

        Assert.Equal(SignInOutcome.SignedIn, result.Outcome);
        Assert.Equal(SignInBlock.None, _store.Find("block2@example.com")!.SignInBlock);
    }

    [Fact]
    public async Task US2_AS6_an_attempt_during_the_block_does_not_extend_it()
    {
        _store.Add("block3@example.com", Password, block: new SignInBlock(5, At(10, 0, 0)));
        _clock.SetUtcNow(At(10, 3, 0));
        AssertFailed(await SignInAsync("block3@example.com", WrongPassword));

        _clock.SetUtcNow(At(10, 5, 0));
        var result = await SignInAsync("block3@example.com", Password);

        Assert.Equal(SignInOutcome.SignedIn, result.Outcome);
    }

    [Fact]
    public async Task US2_AS7_the_end_of_the_block_resets_the_count()
    {
        _store.Add("block4@example.com", Password, block: new SignInBlock(5, At(10, 0, 0)));
        foreach (var second in new[] { 0, 10, 20, 30 })
        {
            _clock.SetUtcNow(At(10, 5, second));
            AssertFailed(await SignInAsync("block4@example.com", WrongPassword));
        }

        Assert.Equal(new SignInBlock(4, null), _store.Find("block4@example.com")!.SignInBlock);
        _clock.SetUtcNow(At(10, 6, 0));
        var result = await SignInAsync("block4@example.com", Password);

        Assert.Equal(SignInOutcome.SignedIn, result.Outcome);
    }

    [Fact]
    public async Task US2_AS8_a_successful_sign_in_resets_the_count()
    {
        _store.Add("reset@example.com", Password);
        for (var i = 0; i < 4; i++)
        {
            AssertFailed(await SignInAsync("reset@example.com", WrongPassword));
        }

        Assert.Equal(SignInOutcome.SignedIn, (await SignInAsync("reset@example.com", Password)).Outcome);
        for (var i = 0; i < 4; i++)
        {
            AssertFailed(await SignInAsync("reset@example.com", WrongPassword));
        }

        var result = await SignInAsync("reset@example.com", Password);

        Assert.Equal(SignInOutcome.SignedIn, result.Outcome);
        Assert.Equal(SignInBlock.None, _store.Find("reset@example.com")!.SignInBlock);
    }

    [Fact]
    public async Task One_lost_race_is_retried_with_the_same_password_verdict()
    {
        _store.Add("race1@example.com", Password);
        _store.LoseNextSaves(1);

        var result = await SignInAsync("race1@example.com", WrongPassword);

        AssertFailed(result);
        Assert.Equal(1, _store.PasswordCheckCount);
        Assert.Equal(2, _store.SaveCount);
        Assert.Equal(1, _store.SuccessfulSaveCount);
        // The lost save moved the state to (1, none); the retry counts this failure on top of it.
        Assert.Equal(new SignInBlock(2, null), _store.Find("race1@example.com")!.SignInBlock);
    }

    [Fact]
    public async Task A_re_read_that_finds_the_account_blocked_fails_with_nothing_more_saved()
    {
        _store.Add("race-block@example.com", Password);
        _store.BlockOnNextSave(At(9, 0, 0));

        var result = await SignInAsync("race-block@example.com", Password);

        AssertFailed(result);
        Assert.Equal(1, _store.SaveCount);
        Assert.Equal(0, _store.SuccessfulSaveCount);
        Assert.Equal(new SignInBlock(5, At(9, 0, 0)), _store.Find("race-block@example.com")!.SignInBlock);
    }

    [Fact]
    public async Task Nine_lost_races_then_a_won_save_signs_in()
    {
        _store.Add("race9@example.com", Password);
        _store.LoseNextSaves(9);

        var result = await SignInAsync("race9@example.com", Password);

        Assert.Equal(SignInOutcome.SignedIn, result.Outcome);
        Assert.Equal(10, _store.SaveCount);
        Assert.Equal(1, _store.SuccessfulSaveCount);
        Assert.Equal(1, _store.PasswordCheckCount);
    }

    [Fact]
    public async Task Ten_lost_races_fail_closed_with_no_save_won()
    {
        _store.Add("race10@example.com", Password);
        _store.LoseNextSaves(10);

        var result = await SignInAsync("race10@example.com", Password);

        AssertFailed(result);
        Assert.Equal(10, _store.SaveCount);
        Assert.Equal(0, _store.SuccessfulSaveCount);
        Assert.Equal(1, _store.PasswordCheckCount);
    }

    [Fact]
    public async Task A_sign_in_carries_the_password_version_it_was_checked_against()
    {
        _store.Add("version@example.com", Password);

        var result = await SignInAsync("version@example.com", Password);

        Assert.Equal(SignInOutcome.SignedIn, result.Outcome);
        Assert.NotNull(result.PasswordVersion);
        Assert.Null((await SignInAsync("version@example.com", WrongPassword)).PasswordVersion);
    }

    [Fact]
    public async Task HP_007_a_password_change_right_after_a_correct_check_fails_the_sign_in_closed()
    {
        var account = _store.Add("stale@example.com", Password, block: new SignInBlock(2, null));
        _store.ChangePasswordAfterNextCheck("running34");

        var result = await SignInAsync("stale@example.com", Password);

        AssertFailed(result);
        Assert.Null(result.PasswordVersion);
        Assert.Equal(1, _store.PasswordCheckCount);
        Assert.Equal(0, _store.SuccessfulSaveCount);
        Assert.Equal(new SignInBlock(2, null), _store.Find("stale@example.com")!.SignInBlock);
        Assert.Equal("running34", _store.PasswordOf(account.Id));
    }

    [Fact]
    public async Task SC_004_unknown_email_wrong_password_and_block_give_the_identical_result()
    {
        _store.Add("same-wrong@example.com", Password);
        _store.Add("same-blocked@example.com", Password, block: new SignInBlock(5, At(9, 0, 0)));

        var unknown = await SignInAsync("same-unknown@example.com", Password);
        var wrong = await SignInAsync("same-wrong@example.com", WrongPassword);
        var blocked = await SignInAsync("same-blocked@example.com", Password);

        foreach (var result in new[] { unknown, wrong, blocked })
        {
            Assert.Equal(SignInOutcome.Failed, result.Outcome);
            Assert.Equal(AccountTexts.SignInFailed, result.Message);
            Assert.Null(result.ActingUser);
        }
    }

    private static DateTimeOffset At(int hour, int minute, int second) =>
        new(2026, 11, 11, hour, minute, second, TimeSpan.Zero);

    private static void AssertFailed(SignInResult result)
    {
        Assert.Equal(SignInOutcome.Failed, result.Outcome);
        Assert.Equal(FailedText, result.Message);
        Assert.Null(result.ActingUser);
    }

    private Task<SignInResult> SignInAsync(string? email, string? password) =>
        _service.SignInAsync(email, password, CancellationToken.None);
}
