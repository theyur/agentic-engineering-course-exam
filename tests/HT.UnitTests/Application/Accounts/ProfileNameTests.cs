using HT.Application.Accounts;
using HT.Domain.Accounts;
using HT.UnitTests.Application.Goals;

namespace HT.UnitTests.Application.Accounts;

/// <summary>
/// <see cref="ProfileService"/>'s name and profile (US11, FR-028, FR-030; research R-04, R-11): changing the name, reading
/// the name, the email and "member since" in the account's time zone, who may use them, and that nothing is written for
/// a refusal. Kyiv is UTC+2 throughout November and December 2026; every instant below is worked out by hand. US11-AS7's
/// "no use case gives it to another account" is enforced structurally by <c>UseCaseParameterTests</c>: no method takes
/// another account's identifier.
/// </summary>
public sealed class ProfileNameTests
{
    private const string Kyiv = "Europe/Kyiv";

    private readonly GoalWorld _world = new();

    private static CancellationToken None => CancellationToken.None;

    private ProfileService Profiles => _world.Profiles;

    private ActingUser Stored(Role role = Role.RegisteredUser, string? name = null, DateTimeOffset? createdAt = null, string? zone = Kyiv)
    {
        var account = _world.Accounts.Add(
            $"profile.{Guid.NewGuid():N}@example.com", "walking42", role, timeZone: zone, name: name, createdAt: createdAt);
        return ActingUser.ForAccount(account.Id, role);
    }

    private string? StoredNameOf(ActingUser user) => _world.Accounts.FindById(user.AccountId!.Value)!.Name;

    private async Task<ProfileView> ReadAsync(ActingUser user)
    {
        var result = await Profiles.GetProfileAsync(user, None);
        Assert.Equal(ProfileReadOutcome.Found, result.Outcome);
        Assert.Null(result.Message);
        return result.Profile!;
    }

    // ---- Changing the name (US11-AS1, AS2) -------------------------------------------------------------------------

    [Theory]
    [InlineData("Yuri", "Yuri")]
    [InlineData("  Yuri  ", "Yuri")]
    [InlineData("\tAna Maria\n", "Ana Maria")]
    public async Task US11_AS1_the_name_is_trimmed(string entered, string stored)
    {
        var user = Stored();

        var result = await Profiles.ChangeNameAsync(user, entered, None);

        Assert.Equal((NameChangeOutcome.Changed, (string?)null), (result.Outcome, result.Message));
        Assert.Equal(stored, StoredNameOf(user));
        Assert.Equal(stored, (await ReadAsync(user)).Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task US11_AS1_no_name_or_only_spaces_removes_the_name(string? entered)
    {
        var user = Stored(name: "Yuri");

        var result = await Profiles.ChangeNameAsync(user, entered, None);

        Assert.Equal(NameChangeOutcome.Changed, result.Outcome);
        Assert.Null(StoredNameOf(user));
        Assert.Null((await ReadAsync(user)).Name);
    }

    [Fact]
    public async Task US11_AS2_sixty_characters_are_accepted()
    {
        var user = Stored();
        var name = new string('n', 60);

        var result = await Profiles.ChangeNameAsync(user, "  " + name + "  ", None);

        Assert.Equal(NameChangeOutcome.Changed, result.Outcome);
        Assert.Equal(name, StoredNameOf(user));
    }

    [Fact]
    public async Task US11_AS2_sixty_one_characters_give_the_refusal_and_change_nothing()
    {
        var user = Stored(name: "Yuri");
        var writes = _world.Accounts.NameWriteCount;

        var result = await Profiles.ChangeNameAsync(user, new string('n', 61), None);

        Assert.Equal(
            (NameChangeOutcome.NameTooLong, "The name can have at most 60 characters."),
            (result.Outcome, result.Message));
        Assert.Equal(nameof(NameChangeOutcome.NameTooLong), result.Outcome.ToString());
        Assert.Equal("Yuri", StoredNameOf(user));
        Assert.Equal(writes, _world.Accounts.NameWriteCount);
    }

    [Fact]
    public async Task The_refusal_carries_no_input_value()
    {
        var user = Stored();
        var name = "Secret " + new string('n', 60);

        var result = await Profiles.ChangeNameAsync(user, name, None);

        Assert.DoesNotContain("Secret", result.Message, StringComparison.Ordinal);
    }

    // ---- Reading the profile (US11-AS6) ---------------------------------------------------------------------------

    [Fact]
    public async Task GetProfileAsync_gives_the_name_and_the_email()
    {
        var user = Stored(name: "Yuri");
        var email = _world.Accounts.FindById(user.AccountId!.Value)!.Email.Value;

        var profile = await ReadAsync(user);

        Assert.Equal(("Yuri", email), (profile.Name, profile.Email));
    }

    [Fact]
    public async Task US11_AS6_an_account_registered_on_17_November_2026_in_Kyiv_is_a_member_since_November_2026()
    {
        // 12:00 in Kyiv on Tuesday 17 November 2026.
        var user = Stored(createdAt: new DateTimeOffset(2026, 11, 17, 10, 0, 0, TimeSpan.Zero));

        Assert.Equal("member since November 2026", (await ReadAsync(user)).MemberSince);
    }

    [Fact]
    public async Task The_month_of_member_since_is_the_one_in_the_accounts_time_zone()
    {
        // 23:30 UTC on Monday 30 November 2026 is 01:30 on Tuesday 1 December in Kyiv.
        var createdAt = new DateTimeOffset(2026, 11, 30, 23, 30, 0, TimeSpan.Zero);
        var kyiv = Stored(createdAt: createdAt);
        var utc = Stored(createdAt: createdAt, zone: "UTC");

        Assert.Equal("member since December 2026", (await ReadAsync(kyiv)).MemberSince);
        Assert.Equal("member since November 2026", (await ReadAsync(utc)).MemberSince);
    }

    [Fact]
    public async Task An_account_with_no_recorded_creation_time_has_no_member_since()
    {
        var user = Stored(name: "Yuri", createdAt: null);

        var profile = await ReadAsync(user);

        Assert.Null(profile.MemberSince);
        Assert.Equal("Yuri", profile.Name);
    }

    [Fact]
    public async Task Member_since_is_written_in_English_whatever_the_culture()
    {
        var user = Stored(createdAt: new DateTimeOffset(2026, 11, 17, 10, 0, 0, TimeSpan.Zero));
        using var culture = new CultureScope("uk-UA");

        Assert.Equal("member since November 2026", (await ReadAsync(user)).MemberSince);
    }

    // ---- Who may use it (US11-AS7, AS8) ---------------------------------------------------------------------------

    [Fact]
    public async Task US11_AS7_an_administrator_sets_and_reads_their_own_name()
    {
        var admin = Stored(Role.Administrator, createdAt: new DateTimeOffset(2026, 11, 17, 10, 0, 0, TimeSpan.Zero));

        var result = await Profiles.ChangeNameAsync(admin, " Olena ", None);
        var profile = await ReadAsync(admin);

        Assert.Equal(NameChangeOutcome.Changed, result.Outcome);
        Assert.Equal(("Olena", "member since November 2026"), (profile.Name, profile.MemberSince));
    }

    [Fact]
    public async Task US11_AS7_changing_ones_own_name_changes_no_other_account()
    {
        var user = Stored(name: "Yuri");
        var other = Stored(name: "Ana");

        await Profiles.ChangeNameAsync(user, "Bohdan", None);

        Assert.Equal("Ana", StoredNameOf(other));
        Assert.Equal("Ana", (await ReadAsync(other)).Name);
    }

    public static TheoryData<string, ActingUser> NotSignedIn => new()
    {
        { "anonymous visitor", ActingUser.Anonymous },
        { "registered user with no stored account", ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.RegisteredUser) },
        { "administrator with no stored account", ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.Administrator) },
    };

    [Theory]
    [MemberData(nameof(NotSignedIn))]
    public async Task US11_AS8_a_user_who_is_not_signed_in_gets_You_are_not_signed_in_before_any_other_check(string who, ActingUser user)
    {
        Assert.NotNull(who);

        var tooLong = await Profiles.ChangeNameAsync(user, new string('n', 61), None);
        var valid = await Profiles.ChangeNameAsync(user, "Yuri", None);
        var read = await Profiles.GetProfileAsync(user, None);

        foreach (var change in new[] { tooLong, valid })
        {
            Assert.Equal((NameChangeOutcome.NotSignedIn, "You are not signed in."), (change.Outcome, change.Message));
            Assert.Equal(nameof(NameChangeOutcome.NotSignedIn), change.Outcome.ToString());
        }

        Assert.Equal((ProfileReadOutcome.NotSignedIn, "You are not signed in."), (read.Outcome, read.Message));
        Assert.Null(read.Profile);
        Assert.Equal(nameof(ProfileReadOutcome.NotSignedIn), read.Outcome.ToString());
        Assert.Equal(0, _world.Accounts.NameWriteCount);
    }
}
