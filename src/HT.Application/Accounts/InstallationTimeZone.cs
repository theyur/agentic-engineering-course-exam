using HT.Domain.Time;

namespace HT.Application.Accounts;

/// <summary>
/// The installation's default time zone (FR-025, research R-03): the zone of a new account that chose none, and of an
/// account created before slice 4 (research R-04). The host builds it from its configuration and registers it before
/// <c>AddHtApplication</c>, which otherwise registers UTC, so Application never reads configuration (ADR 0005).
/// </summary>
public sealed class InstallationTimeZone
{
    private InstallationTimeZone(NamedTimeZone value, bool isConfiguredValueIgnored)
    {
        Value = value;
        IsConfiguredValueIgnored = isConfiguredValueIgnored;
    }

    /// <summary>The default named time zone.</summary>
    public NamedTimeZone Value { get; }

    /// <summary>
    /// True when a value was configured but is not a named time zone, so UTC is used instead. The host logs that fact,
    /// never the value (contracts/host.md, "Operator log").
    /// </summary>
    public bool IsConfiguredValueIgnored { get; }

    /// <summary>
    /// The configured zone when <see cref="NamedTimeZone.TryParse"/> accepts it; UTC when <paramref name="configured"/>
    /// is absent (null or empty) or not a named time zone.
    /// </summary>
    public static InstallationTimeZone From(string? configured)
    {
        if (string.IsNullOrEmpty(configured))
        {
            return new InstallationTimeZone(NamedTimeZone.Utc, isConfiguredValueIgnored: false);
        }

        return NamedTimeZone.TryParse(configured, out var zone)
            ? new InstallationTimeZone(zone, isConfiguredValueIgnored: false)
            : new InstallationTimeZone(NamedTimeZone.Utc, isConfiguredValueIgnored: true);
    }
}
