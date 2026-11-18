using System.Globalization;

namespace HT.UnitTests.Domain.Time;

/// <summary>Writes test instants as UTC literals ("2026-11-18T10:00:00Z"), parsed without the current culture.</summary>
public static class Instants
{
    public static DateTimeOffset Utc(string literal) =>
        DateTimeOffset.ParseExact(
            literal, "yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
}
