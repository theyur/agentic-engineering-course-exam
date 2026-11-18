using HT.Application.Accounts;
using HT.Domain.Time;

namespace HT.IntegrationTests.Accounts;

/// <summary>Account profiles for tests that create accounts through the public account store port (research R-04).</summary>
public static class TestProfiles
{
    /// <summary>No name, UTC, created at 10:00 UTC on Wednesday 18 November 2026.</summary>
    public static NewAccountProfile Utc { get; } =
        new(null, NamedTimeZone.Utc, new DateTimeOffset(2026, 11, 18, 10, 0, 0, TimeSpan.Zero));
}
