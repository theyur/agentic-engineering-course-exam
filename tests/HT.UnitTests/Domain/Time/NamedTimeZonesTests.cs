using HT.Domain.Time;

namespace HT.UnitTests.Domain.Time;

/// <summary>HP-015: the Profile list is UTC, then the canonical tzdb zones that are named time zones, in ordinal order.</summary>
public class NamedTimeZonesTests
{
    [Fact]
    public void Starts_with_UTC_once()
    {
        Assert.Equal("UTC", NamedTimeZones.Choices[0]);
        Assert.Single(NamedTimeZones.Choices, id => id == "UTC");
    }

    [Fact]
    public void Contains_zones()
    {
        Assert.Contains("Europe/Kyiv", NamedTimeZones.Choices);
        Assert.Contains("America/New_York", NamedTimeZones.Choices);
        Assert.Contains("America/Argentina/Buenos_Aires", NamedTimeZones.Choices);
    }

    [Theory]
    [InlineData("Europe/Kiev")]
    [InlineData("Asia/Calcutta")]
    [InlineData("US/Eastern")]
    public void Contains_no_link(string link)
    {
        Assert.DoesNotContain(link, NamedTimeZones.Choices);
    }

    [Fact]
    public void Contains_no_Etc_identifier_and_no_identifier_without_a_slash_but_UTC()
    {
        Assert.DoesNotContain(NamedTimeZones.Choices, id => id.StartsWith("Etc/", StringComparison.Ordinal));
        Assert.All(NamedTimeZones.Choices.Skip(1), id => Assert.Contains('/', id));
    }

    [Fact]
    public void Is_in_ordinal_order_after_UTC()
    {
        var rest = NamedTimeZones.Choices.Skip(1).ToList();

        Assert.Equal(rest.Order(StringComparer.Ordinal), rest);
        Assert.Equal(rest.Count, rest.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void Every_entry_is_a_named_time_zone()
    {
        Assert.All(NamedTimeZones.Choices, id =>
        {
            Assert.True(NamedTimeZone.TryParse(id, out var zone), id);
            Assert.Equal(id, zone.Id);
        });
    }
}
