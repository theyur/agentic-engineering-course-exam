using HT.Application.Accounts;
using HT.Domain.Accounts;
using HT.UnitTests.Application.Goals;

namespace HT.UnitTests.Application.Accounts;

/// <summary>
/// <see cref="ProfileService.GetHomeHeadingAsync"/> (US11-AS3 to AS5, AS8, FR-029; research R-11): the greeting by the
/// local time in the account's time zone, with the account's name when it has one, and today's date in English. Kyiv is
/// UTC+2 in November 2026; every instant below is worked out by hand from the Kyiv time in its name.
/// </summary>
public sealed class HomeHeadingTests
{
    private const string Kyiv = "Europe/Kyiv";

    private static CancellationToken None => CancellationToken.None;

    private static DateTimeOffset Utc(int month, int day, int hour, int minute) =>
        new(2026, month, day, hour, minute, 0, TimeSpan.Zero);

    private static async Task<HomeHeading> HeadingAsync(DateTimeOffset now, string? name, string? zone = Kyiv, string? defaultZone = null)
    {
        var world = new GoalWorld(defaultZone, now);
        var account = world.Accounts.Add($"home.{Guid.NewGuid():N}@example.com", "walking42", timeZone: zone, name: name);
        var result = await world.Profiles.GetHomeHeadingAsync(ActingUser.ForAccount(account.Id, account.Role), None);
        Assert.Equal((HomeHeadingOutcome.Found, (string?)null), (result.Outcome, result.Message));
        return result.Heading!;
    }

    public static TheoryData<string, DateTimeOffset, string> AroundTheBoundaries => new()
    {
        { "04:59", Utc(11, 18, 2, 59), "Good evening, Yuri" },
        { "05:00", Utc(11, 18, 3, 0), "Good morning, Yuri" },
        { "11:59", Utc(11, 18, 9, 59), "Good morning, Yuri" },
        { "12:00", Utc(11, 18, 10, 0), "Good afternoon, Yuri" },
        { "17:59", Utc(11, 18, 15, 59), "Good afternoon, Yuri" },
        { "18:00", Utc(11, 18, 16, 0), "Good evening, Yuri" },
    };

    [Theory]
    [MemberData(nameof(AroundTheBoundaries))]
    public async Task US11_AS3_the_greeting_follows_the_local_time_in_Kyiv(string kyivTime, DateTimeOffset now, string greeting)
    {
        Assert.NotNull(kyivTime);

        var heading = await HeadingAsync(now, "Yuri");

        Assert.Equal((greeting, "Wednesday, 18 November"), (heading.Greeting, heading.Date));
    }

    [Theory]
    [MemberData(nameof(AroundTheBoundaries))]
    public async Task The_greeting_and_the_date_are_the_same_inside_a_non_English_culture(string kyivTime, DateTimeOffset now, string greeting)
    {
        Assert.NotNull(kyivTime);
        using var culture = new CultureScope("uk-UA");

        var heading = await HeadingAsync(now, "Yuri");

        Assert.Equal((greeting, "Wednesday, 18 November"), (heading.Greeting, heading.Date));
    }

    [Fact]
    public async Task US11_AS4_without_a_name_the_greeting_has_no_name()
    {
        // 20:00 in Kyiv.
        var heading = await HeadingAsync(Utc(11, 18, 18, 0), name: null);

        Assert.Equal("Good evening", heading.Greeting);
    }

    [Theory]
    [InlineData(3, 0, "Good morning")]
    [InlineData(10, 0, "Good afternoon")]
    public async Task Without_a_name_every_greeting_stands_alone(int hour, int minute, string greeting)
    {
        var heading = await HeadingAsync(Utc(11, 18, hour, minute), name: null);

        Assert.Equal(greeting, heading.Greeting);
    }

    [Fact]
    public async Task US11_AS5_the_date_and_the_greeting_are_those_of_the_account_zone_not_of_UTC()
    {
        // 23:30 UTC on Monday 16 November is 01:30 on Tuesday 17 November in Kyiv.
        var heading = await HeadingAsync(Utc(11, 16, 23, 30), name: null);

        Assert.Equal(("Good evening", "Tuesday, 17 November"), (heading.Greeting, heading.Date));
    }

    [Fact]
    public async Task US11_AS5_inside_a_non_English_culture_the_date_stays_in_English()
    {
        using var culture = new CultureScope("de-DE");

        var heading = await HeadingAsync(Utc(11, 16, 23, 30), name: null);

        Assert.Equal(("Good evening", "Tuesday, 17 November"), (heading.Greeting, heading.Date));
    }

    [Fact]
    public async Task An_account_with_no_stored_zone_uses_the_installations_default()
    {
        // 23:30 UTC on Monday 16 November: still Monday evening in UTC, Tuesday night in Kyiv.
        var utc = await HeadingAsync(Utc(11, 16, 23, 30), name: null, zone: null);
        var kyivDefault = await HeadingAsync(Utc(11, 16, 23, 30), name: null, zone: null, defaultZone: Kyiv);

        Assert.Equal(("Good evening", "Monday, 16 November"), (utc.Greeting, utc.Date));
        Assert.Equal(("Good evening", "Tuesday, 17 November"), (kyivDefault.Greeting, kyivDefault.Date));
    }

    [Fact]
    public async Task An_administrator_gets_their_own_heading()
    {
        var world = new GoalWorld(null, Utc(11, 18, 10, 0));
        var admin = GoalInputs.Administrator(world.Accounts, Kyiv);
        Assert.Equal(NameChangeOutcome.Changed, (await world.Profiles.ChangeNameAsync(admin, "Olena", None)).Outcome);

        var result = await world.Profiles.GetHomeHeadingAsync(admin, None);

        Assert.Equal(("Good afternoon, Olena", "Wednesday, 18 November"), (result.Heading!.Greeting, result.Heading.Date));
    }

    public static TheoryData<string, ActingUser> NotSignedIn => new()
    {
        { "anonymous visitor", ActingUser.Anonymous },
        { "registered user with no stored account", ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.RegisteredUser) },
        { "administrator with no stored account", ActingUser.ForAccount(new AccountId(Guid.NewGuid()), Role.Administrator) },
    };

    [Theory]
    [MemberData(nameof(NotSignedIn))]
    public async Task US11_AS8_a_user_who_is_not_signed_in_gets_You_are_not_signed_in(string who, ActingUser user)
    {
        Assert.NotNull(who);
        var world = new GoalWorld();

        var result = await world.Profiles.GetHomeHeadingAsync(user, None);

        Assert.Equal((HomeHeadingOutcome.NotSignedIn, "You are not signed in."), (result.Outcome, result.Message));
        Assert.Null(result.Heading);
        Assert.Equal(nameof(HomeHeadingOutcome.NotSignedIn), result.Outcome.ToString());
    }
}
