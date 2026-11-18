using HT.Domain.Time;
using static HT.UnitTests.Domain.Time.Instants;

namespace HT.UnitTests.Domain.Time;

/// <summary>FR-024 named time zones and the BCL conversions (research R-01, R-02). Every instant is a hand-worked UTC literal.</summary>
public class NamedTimeZoneTests
{
    [Theory]
    [InlineData("UTC")]
    [InlineData("Europe/Kyiv")]
    [InlineData("America/Argentina/Buenos_Aires")]
    [InlineData("Europe/Kiev")]
    [InlineData("Asia/Calcutta")]
    [InlineData("US/Eastern")]
    public void Accepts_every_named_time_zone_and_keeps_it_as_given(string text)
    {
        Assert.True(NamedTimeZone.TryParse(text, out var zone));
        Assert.Equal(text, zone.Id);
    }

    [Theory]
    [InlineData("FLE Standard Time")]
    [InlineData("Etc/GMT+3")]
    [InlineData("Etc/UTC")]
    [InlineData("EST")]
    [InlineData("CET")]
    [InlineData("GMT")]
    [InlineData("+02:00")]
    [InlineData("Mars/Olympus")]
    [InlineData(" Europe/Kyiv")]
    [InlineData("Europe/Kyiv ")]
    [InlineData("europe/kyiv")]
    [InlineData("utc")]
    [InlineData("")]
    [InlineData(null)]
    public void Refuses_what_is_not_a_named_time_zone(string? text)
    {
        Assert.False(NamedTimeZone.TryParse(text, out var zone));
        Assert.Equal(default, zone);
    }

    [Fact]
    public void Utc_is_UTC()
    {
        Assert.Equal("UTC", NamedTimeZone.Utc.Id);
        Assert.True(NamedTimeZone.TryParse("UTC", out var zone));
        Assert.Equal(NamedTimeZone.Utc, zone);
    }

    [Fact]
    public void A_link_uses_its_zone_rules()
    {
        // Kyiv midnight of Wednesday 18 November 2026 (EET, UTC+2).
        Assert.Equal(Utc("2026-11-17T22:00:00Z"), Zone("Europe/Kiev").StartOfDay(new DateOnly(2026, 11, 18)));
        Assert.Equal(Zone("Europe/Kyiv").StartOfDay(new DateOnly(2026, 11, 18)), Zone("Europe/Kiev").StartOfDay(new DateOnly(2026, 11, 18)));
    }

    public static TheoryData<string, string, DateOnly, int, int> AroundMidnight => new()
    {
        // Kyiv, EET (UTC+2): midnight of 18 Nov 2026 is 2026-11-17T22:00:00Z.
        { "Europe/Kyiv", "2026-11-17T21:59:59Z", new DateOnly(2026, 11, 17), 23, 59 },
        { "Europe/Kyiv", "2026-11-17T22:00:00Z", new DateOnly(2026, 11, 18), 0, 0 },
        { "Europe/Kyiv", "2026-11-17T22:00:01Z", new DateOnly(2026, 11, 18), 0, 0 },
        // New York, EST (UTC-5): midnight of 18 Nov 2026 is 2026-11-18T05:00:00Z.
        { "America/New_York", "2026-11-18T04:59:59Z", new DateOnly(2026, 11, 17), 23, 59 },
        { "America/New_York", "2026-11-18T05:00:00Z", new DateOnly(2026, 11, 18), 0, 0 },
        { "America/New_York", "2026-11-18T05:00:01Z", new DateOnly(2026, 11, 18), 0, 0 },
        // UTC.
        { "UTC", "2026-11-17T23:59:59Z", new DateOnly(2026, 11, 17), 23, 59 },
        { "UTC", "2026-11-18T00:00:00Z", new DateOnly(2026, 11, 18), 0, 0 },
        { "UTC", "2026-11-18T00:00:01Z", new DateOnly(2026, 11, 18), 0, 0 },
    };

    [Theory]
    [MemberData(nameof(AroundMidnight))]
    public void DateAt_and_TimeAt_change_at_local_midnight(string id, string instant, DateOnly date, int hour, int minute)
    {
        var zone = Zone(id);
        var at = Utc(instant);

        Assert.Equal(date, zone.DateAt(at));
        Assert.Equal(hour, zone.TimeAt(at).Hour);
        Assert.Equal(minute, zone.TimeAt(at).Minute);
    }

    [Fact]
    public void An_instant_with_an_offset_is_the_same_instant()
    {
        // 12:00 Kyiv on Wednesday 18 November 2026, written with Kyiv's offset.
        var at = new DateTimeOffset(2026, 11, 18, 12, 0, 0, TimeSpan.FromHours(2));

        Assert.Equal(new DateOnly(2026, 11, 18), Zone("Europe/Kyiv").DateAt(at));
        Assert.Equal(new TimeOnly(12, 0), Zone("Europe/Kyiv").TimeAt(at));
        Assert.Equal(new TimeOnly(5, 0), Zone("America/New_York").TimeAt(at));
    }

    public static TheoryData<string, DateOnly, string> StartsOfDays => new()
    {
        // Kyiv spring forward on Sunday 28 March 2027 (03:00 EET -> 04:00 EEST): the day lasts 23 hours.
        { "Europe/Kyiv", new DateOnly(2027, 3, 28), "2027-03-27T22:00:00Z" },
        { "Europe/Kyiv", new DateOnly(2027, 3, 29), "2027-03-28T21:00:00Z" },
        // Kyiv fall back on Sunday 31 October 2027 (04:00 EEST -> 03:00 EET): the day lasts 25 hours.
        { "Europe/Kyiv", new DateOnly(2027, 10, 31), "2027-10-30T21:00:00Z" },
        { "Europe/Kyiv", new DateOnly(2027, 11, 1), "2027-10-31T22:00:00Z" },
        // Havana springs forward at midnight on Sunday 14 March 2027 (00:00 CST -> 01:00 CDT; checked in the
        // embedded tzdb 2026b): midnight does not exist, so the day starts at 01:00 CDT, 05:00 UTC.
        { "America/Havana", new DateOnly(2027, 3, 13), "2027-03-13T05:00:00Z" },
        { "America/Havana", new DateOnly(2027, 3, 14), "2027-03-14T05:00:00Z" },
        { "America/Havana", new DateOnly(2027, 3, 15), "2027-03-15T04:00:00Z" },
        { "UTC", new DateOnly(2027, 3, 28), "2027-03-28T00:00:00Z" },
    };

    [Theory]
    [MemberData(nameof(StartsOfDays))]
    public void StartOfDay_is_the_first_instant_of_the_local_date_in_UTC(string id, DateOnly date, string start)
    {
        var actual = Zone(id).StartOfDay(date);

        Assert.Equal(Utc(start), actual);
        Assert.Equal(TimeSpan.Zero, actual.Offset);
    }

    [Fact]
    public void A_skipped_midnight_still_starts_its_day_on_its_date()
    {
        var start = Zone("America/Havana").StartOfDay(new DateOnly(2027, 3, 14));

        Assert.Equal(new DateOnly(2027, 3, 14), Zone("America/Havana").DateAt(start));
        Assert.Equal(new TimeOnly(1, 0), Zone("America/Havana").TimeAt(start));
        Assert.Equal(new DateOnly(2027, 3, 13), Zone("America/Havana").DateAt(start.AddSeconds(-1)));
    }

    [Fact]
    public void No_culture_is_read()
    {
        using var culture = new CultureScope("ar-SA");

        Assert.True(NamedTimeZone.TryParse("Europe/Kyiv", out var kyiv));
        Assert.Equal(new DateOnly(2026, 11, 18), kyiv.DateAt(Utc("2026-11-18T10:00:00Z")));
        Assert.Equal(new TimeOnly(12, 0), kyiv.TimeAt(new DateTimeOffset(2026, 11, 18, 10, 0, 0, TimeSpan.Zero)));
        Assert.Equal(new DateTimeOffset(2026, 11, 17, 22, 0, 0, TimeSpan.Zero), kyiv.StartOfDay(new DateOnly(2026, 11, 18)));
        Assert.Equal("UTC", NamedTimeZones.Choices[0]);
        Assert.False(NamedTimeZone.TryParse("europe/kyiv", out _));
    }

    private static NamedTimeZone Zone(string id)
    {
        Assert.True(NamedTimeZone.TryParse(id, out var zone), id);
        return zone;
    }
}
