namespace HT.Domain.Time;

/// <summary>The time zones the Profile list offers (HP-015).</summary>
public static class NamedTimeZones
{
    private static readonly Lazy<IReadOnlyList<string>> LazyChoices = new(Build);

    /// <summary>
    /// <c>UTC</c> first, then every canonical tzdb zone (not a link) that is a named time zone, in ordinal order.
    /// The page adds the account's current value when it is not among them.
    /// </summary>
    public static IReadOnlyList<string> Choices => LazyChoices.Value;

    private static IReadOnlyList<string> Build()
    {
        var zones = NamedTimeZone.Source.CanonicalIdMap
            .Where(pair => string.Equals(pair.Key, pair.Value, StringComparison.Ordinal))
            .Select(pair => pair.Key)
            .Where(id => NamedTimeZone.TryParse(id, out _) && !string.Equals(id, NamedTimeZone.Utc.Id, StringComparison.Ordinal))
            .Order(StringComparer.Ordinal);
        return [NamedTimeZone.Utc.Id, .. zones];
    }
}
