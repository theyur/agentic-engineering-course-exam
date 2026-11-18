using System.Collections.Frozen;
using NodaTime;

namespace HT.Domain.Time;

/// <summary>
/// The identifiers <see cref="NamedTimeZone"/> accepts: <c>UTC</c>, and every identifier of the embedded tzdb. A
/// separate internal type, because a struct's reference assembly keeps its private fields, and the acceptance-test
/// listing holds public members only.
/// </summary>
internal static class TzdbIdentifiers
{
    public const string Utc = "UTC";

    private static readonly Lazy<FrozenSet<string>> LazyAll =
        new(() => DateTimeZoneProviders.Tzdb.Ids.ToFrozenSet(StringComparer.Ordinal));

    /// <summary>Every identifier of the embedded tzdb, zones and links, compared ordinally.</summary>
    public static FrozenSet<string> All => LazyAll.Value;
}
