using HT.Application.Accounts;
using HT.Domain.Accounts;
using HT.UnitTests.Fakes;
using Microsoft.Extensions.Time.Testing;

namespace HT.UnitTests.Application.Accounts;

/// <summary>Registration with a time zone and a name (US10, US11, FR-024, FR-025, FR-028, FR-030; research R-04).</summary>
public sealed class RegisterProfileTests
{
    /// <summary>12:00 in Kyiv on Wednesday 18 November 2026.</summary>
    private static readonly DateTimeOffset Now = new(2026, 11, 18, 10, 0, 0, TimeSpan.Zero);

    private readonly FakeAccountStore _store = new();
    private readonly FakeTimeProvider _clock = new(Now);

    [Fact]
    public async Task US10_AS1_a_named_time_zone_is_stored()
    {
        var result = await RegisterAsync("kyiv.zone@example.com", timeZone: "Europe/Kyiv");

        Assert.Equal(RegistrationOutcome.Registered, result.Outcome);
        Assert.Equal("Europe/Kyiv", _store.Find("kyiv.zone@example.com")!.StoredTimeZone);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Mars/Olympus")]
    [InlineData("FLE Standard Time")]
    public async Task US10_AS2_no_zone_or_one_that_is_not_named_stores_UTC_without_a_configured_default(string? timeZone)
    {
        var result = await RegisterAsync("utc.default@example.com", timeZone: timeZone);

        Assert.Equal(RegistrationOutcome.Registered, result.Outcome);
        Assert.Null(result.Message);
        Assert.Equal("UTC", _store.Find("utc.default@example.com")!.StoredTimeZone);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Mars/Olympus")]
    [InlineData("FLE Standard Time")]
    public async Task US10_AS2_no_zone_or_one_that_is_not_named_stores_the_configured_default(string? timeZone)
    {
        var result = await RegisterAsync("kyiv.default@example.com", timeZone: timeZone, defaultZone: "Europe/Kyiv");

        Assert.Equal(RegistrationOutcome.Registered, result.Outcome);
        Assert.Equal("Europe/Kyiv", _store.Find("kyiv.default@example.com")!.StoredTimeZone);
    }

    [Fact]
    public async Task US11_AS1_the_name_is_stored_trimmed()
    {
        await RegisterAsync("named@example.com", name: "  Yuri  ");

        Assert.Equal("Yuri", _store.Find("named@example.com")!.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("    ")]
    public async Task US11_AS1_no_name_or_spaces_only_store_none(string? name)
    {
        var result = await RegisterAsync("unnamed@example.com", name: name);

        Assert.Equal(RegistrationOutcome.Registered, result.Outcome);
        Assert.Null(_store.Find("unnamed@example.com")!.Name);
    }

    [Fact]
    public async Task US11_AS2_a_name_of_61_characters_is_refused_and_nothing_is_created()
    {
        var result = await RegisterAsync("long.name@example.com", name: new string('a', 61));

        Assert.Equal(RegistrationOutcome.NameTooLong, result.Outcome);
        Assert.Equal("The name can have at most 60 characters.", result.Message);
        Assert.Null(result.AccountId);
        Assert.Empty(_store.Accounts);
        Assert.Equal(0, _store.CreateCount);
    }

    [Fact]
    public async Task US11_AS2_a_name_of_60_characters_is_accepted()
    {
        var name = new string('a', 60);

        var result = await RegisterAsync("sixty@example.com", name: "  " + name + " ");

        Assert.Equal(RegistrationOutcome.Registered, result.Outcome);
        Assert.Equal(name, _store.Find("sixty@example.com")!.Name);
    }

    [Fact]
    public async Task A_taken_email_with_a_long_name_gives_EmailTaken_and_creates_nothing()
    {
        _store.Add("taken.long@example.com", "walking12");

        var result = await RegisterAsync("taken.long@example.com", name: new string('a', 61));

        Assert.Equal(RegistrationOutcome.EmailTaken, result.Outcome);
        Assert.Equal("An account with this email already exists.", result.Message);
        Assert.Single(_store.Accounts);
        Assert.Equal(0, _store.CreateCount);
    }

    [Fact]
    public async Task The_password_rule_is_checked_before_the_name()
    {
        var result = await Service().RegisterAsync(
            "rule.first@example.com", "walk4", null, new string('a', 61), CancellationToken.None);

        Assert.Equal(RegistrationOutcome.PasswordRuleNotMet, result.Outcome);
        Assert.Empty(_store.Accounts);
    }

    [Fact]
    public async Task The_creation_time_is_the_supplied_time()
    {
        await RegisterAsync("created.at@example.com");

        Assert.Equal(Now, _store.Find("created.at@example.com")!.CreatedAt);
    }

    [Fact]
    public async Task FR019_the_old_overload_gives_slice_1_outcomes_with_the_default_zone_and_no_name()
    {
        var service = Service("Europe/Kyiv");
        _store.Add("old.taken@example.com", "walking12");

        var registered = await service.RegisterAsync("old.overload@example.com", "walking42", CancellationToken.None);
        var invalid = await service.RegisterAsync("old.example.com", "walking42", CancellationToken.None);
        var rule = await service.RegisterAsync("old.rule@example.com", "walk4", CancellationToken.None);
        var taken = await service.RegisterAsync("old.taken@example.com", "walking42", CancellationToken.None);

        Assert.Equal(RegistrationOutcome.Registered, registered.Outcome);
        Assert.Null(registered.Message);
        var account = _store.Find("old.overload@example.com")!;
        Assert.Equal(account.Id, registered.AccountId);
        Assert.Equal(Role.RegisteredUser, account.Role);
        Assert.Equal("Europe/Kyiv", account.StoredTimeZone);
        Assert.Null(account.Name);
        Assert.Equal(Now, account.CreatedAt);
        Assert.Equal((RegistrationOutcome.InvalidEmail, "Enter a valid email address."), (invalid.Outcome, invalid.Message));
        Assert.Equal((RegistrationOutcome.PasswordRuleNotMet, "At least 8 characters, with a number."), (rule.Outcome, rule.Message));
        Assert.Equal((RegistrationOutcome.EmailTaken, "An account with this email already exists."), (taken.Outcome, taken.Message));
    }

    private AccountService Service(string? defaultZone = null) => new(_store, _clock, InstallationTimeZone.From(defaultZone));

    private Task<RegistrationResult> RegisterAsync(
        string email, string? timeZone = null, string? name = null, string? defaultZone = null) =>
        Service(defaultZone).RegisterAsync(email, "walking42", timeZone, name, CancellationToken.None);
}
