using System.Collections.Frozen;
using NodaTime;
using NodaTime.TimeZones;

namespace HT.Domain.Time;

/// <summary>
/// A named time zone (FR-024): exactly <c>UTC</c>, or an identifier of the embedded tzdb, zone or link, that contains
/// <c>/</c> and does not start with <c>Etc/</c>. The identifier is kept as given, so a link stays a link; its rules
/// come from the zone it links to (research R-02). The conversions use BCL types only, so NodaTime stays inside
/// <c>HT.Domain</c> (ADR 0017).
/// </summary>
public readonly record struct NamedTimeZone
{
    private const string UtcId = "UTC";

    private NamedTimeZone(string id) => Id = id;

    /// <summary>The identifier, exactly as given.</summary>
    public string Id { get; }

    /// <summary>The named time zone <c>UTC</c>.</summary>
    public static NamedTimeZone Utc { get; } = new(UtcId);

    /// <summary>
    /// Accepts exactly <c>UTC</c>, or a tzdb identifier that contains <c>/</c> and does not start with <c>Etc/</c>.
    /// Ordinal and case-sensitive; the text is not trimmed.
    /// </summary>
    public static bool TryParse(string? text, out NamedTimeZone zone)
    {
        zone = default;
        if (text is null)
        {
            return false;
        }

        if (string.Equals(text, UtcId, StringComparison.Ordinal))
        {
            zone = Utc;
            return true;
        }

        if (!text.Contains('/', StringComparison.Ordinal)
            || text.StartsWith("Etc/", StringComparison.Ordinal)
            || !TzdbIds.Contains(text))
        {
            return false;
        }

        zone = new NamedTimeZone(text);
        return true;
    }

    /// <summary>The local date at an instant: the account's "today" (FR-012).</summary>
    public DateOnly DateAt(DateTimeOffset instant)
    {
        var local = Instant.FromDateTimeOffset(instant).InZone(Zone).Date;
        return new DateOnly(local.Year, local.Month, local.Day);
    }

    /// <summary>The local time of day at an instant (the Home greeting, FR-029).</summary>
    public TimeOnly TimeAt(DateTimeOffset instant)
    {
        var local = Instant.FromDateTimeOffset(instant).InZone(Zone).TimeOfDay;
        return new TimeOnly(local.TickOfDay);
    }

    /// <summary>
    /// The first instant of a local date, in UTC. Where the clocks jump at midnight and midnight does not exist,
    /// the day starts at its first instant (research R-01).
    /// </summary>
    public DateTimeOffset StartOfDay(DateOnly date) =>
        Zone.AtStartOfDay(new LocalDate(date.Year, date.Month, date.Day)).ToInstant().ToDateTimeOffset();

    public override string ToString() => Id ?? string.Empty;

    internal static TzdbDateTimeZoneSource Source => TzdbDateTimeZoneSource.Default;

    /// <summary>Every identifier of the embedded tzdb, zones and links, compared ordinally.</summary>
    private static FrozenSet<string> TzdbIds => LazyTzdbIds.Value;

    private static readonly Lazy<FrozenSet<string>> LazyTzdbIds =
        new(() => DateTimeZoneProviders.Tzdb.Ids.ToFrozenSet(StringComparer.Ordinal));

    private DateTimeZone Zone =>
        Id is null || string.Equals(Id, UtcId, StringComparison.Ordinal)
            ? DateTimeZone.Utc
            : DateTimeZoneProviders.Tzdb[Id];
}
