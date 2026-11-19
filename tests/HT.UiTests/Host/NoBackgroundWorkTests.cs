using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HT.UiTests.Host;

/// <summary>
/// HP-010 (research R-14, ADR 0018): nothing runs in the background to evaluate, finalize or store results. A
/// commitment becomes Missed at its grace end because every read evaluates it at the supplied time, so the host runs no
/// hosted service beyond the framework's, the same set that slices 1 to 4 already have.
/// </summary>
[Collection(HostCollection.Name)]
public sealed class NoBackgroundWorkTests(HostFixture host)
{
    /// <summary>
    /// The framework's hosted services that the host has had since slice 1, by full type name, sorted: Data Protection's
    /// key ring loader and the web host itself. No project of this solution registers a hosted service.
    /// </summary>
    private static readonly string[] FrameworkHostedServices =
    [
        "Microsoft.AspNetCore.DataProtection.Internal.DataProtectionHostedService",
        "Microsoft.AspNetCore.Hosting.GenericWebHostService",
    ];

    [Fact]
    public void HP_010_the_host_registers_no_hosted_service_beyond_the_frameworks()
    {
        var hosted = host.Services.GetServices<IHostedService>()
            .Select(service => service.GetType().FullName!)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(FrameworkHostedServices, hosted);
    }
}
