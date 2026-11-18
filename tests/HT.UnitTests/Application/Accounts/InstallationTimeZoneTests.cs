using HT.Application;
using HT.Application.Accounts;
using HT.Domain.Time;
using Microsoft.Extensions.DependencyInjection;

namespace HT.UnitTests.Application.Accounts;

/// <summary>The installation's default time zone (FR-025, research R-03).</summary>
public sealed class InstallationTimeZoneTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void An_absent_value_gives_UTC(string? configured)
    {
        var zone = InstallationTimeZone.From(configured);

        Assert.Equal("UTC", zone.Value.Id);
        Assert.False(zone.IsConfiguredValueIgnored);
    }

    [Theory]
    [InlineData("Mars/Olympus")]
    [InlineData("Etc/UTC")]
    [InlineData("FLE Standard Time")]
    [InlineData(" Europe/Kyiv")]
    public void A_value_that_is_not_a_named_time_zone_gives_UTC(string configured)
    {
        var zone = InstallationTimeZone.From(configured);

        Assert.Equal("UTC", zone.Value.Id);
        Assert.True(zone.IsConfiguredValueIgnored);
    }

    [Theory]
    [InlineData("Europe/Kyiv")]
    [InlineData("UTC")]
    [InlineData("Europe/Kiev")]
    public void A_named_time_zone_is_kept_as_given(string configured)
    {
        var zone = InstallationTimeZone.From(configured);

        Assert.Equal(configured, zone.Value.Id);
        Assert.False(zone.IsConfiguredValueIgnored);
    }

    [Fact]
    public void AddHtApplication_registers_UTC_when_nothing_else_is_registered()
    {
        var services = new ServiceCollection().AddHtApplication();

        Assert.Equal(NamedTimeZone.Utc, Registered(services).Value);
    }

    [Fact]
    public void AddHtApplication_keeps_a_zone_registered_before_it()
    {
        var services = new ServiceCollection();
        services.AddSingleton(InstallationTimeZone.From("Europe/Kyiv"));

        services.AddHtApplication();

        Assert.Equal("Europe/Kyiv", Registered(services).Value.Id);
    }

    private static InstallationTimeZone Registered(IServiceCollection services) =>
        Assert.IsType<InstallationTimeZone>(Assert.Single(services, d => d.ServiceType == typeof(InstallationTimeZone)).ImplementationInstance);
}
