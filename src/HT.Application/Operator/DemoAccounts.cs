using HT.Domain.Time;

namespace HT.Application.Operator;

/// <summary>
/// The profiles of the demo accounts (FR-040, research R-10): <c>demo1@example.com</c> has the account name
/// "Demo One", demo2 and demo3 have none; every demo account has the zone and creation time demo loading passes. The
/// demo emails stay in <see cref="DemoDataService.DemoEmails"/>.
/// </summary>
public static class DemoAccounts
{
    /// <summary>The account name of <c>demo1@example.com</c>, which the smoke check expects on its Profile (HP-018).</summary>
    public const string Demo1Name = "Demo One";

    private static readonly Dictionary<string, string?> NameByEmail = new(StringComparer.Ordinal)
    {
        ["demo1@example.com"] = Demo1Name,
        ["demo2@example.com"] = null,
        ["demo3@example.com"] = null,
    };

    /// <summary>
    /// The account name of one of <see cref="DemoDataService.DemoEmails"/>, in normal form (lower case), or null when it
    /// has none. Any other email is a programming error.
    /// </summary>
    public static string? NameFor(string demoEmail)
    {
        ArgumentNullException.ThrowIfNull(demoEmail);
        return NameByEmail.TryGetValue(demoEmail, out var name)
            ? name
            : throw new ArgumentException("Not a demo email.", nameof(demoEmail));
    }

    /// <summary>
    /// The profile of one of <see cref="DemoDataService.DemoEmails"/>: its account name (<see cref="NameFor"/>), this time
    /// zone and this creation time. Any other email is a programming error.
    /// </summary>
    public static DemoAccountProfile ProfileFor(string demoEmail, NamedTimeZone timeZone, DateTimeOffset createdAt) =>
        new(NameFor(demoEmail), timeZone, createdAt);
}
