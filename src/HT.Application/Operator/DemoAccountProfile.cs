using HT.Domain.Accounts;
using HT.Domain.Time;

namespace HT.Application.Operator;

/// <summary>
/// The profile a demo account is created with (FR-040, research R-10): its account name or none, the installation's
/// default time zone and the supplied creation time. Application supplies all three, so the store computes nothing.
/// </summary>
public sealed record DemoAccountProfile
{
    /// <exception cref="ArgumentException">The name fails the account-name checks or is not trimmed, or the zone is the default value.</exception>
    public DemoAccountProfile(string? name, NamedTimeZone timeZone, DateTimeOffset createdAt)
    {
        if (name is not null && (!AccountName.TryCreate(name, out var checkedName, out _) || checkedName?.Value != name))
        {
            throw new ArgumentException("A demo account name must pass the account-name checks.", nameof(name));
        }

        if (timeZone.Id is null)
        {
            throw new ArgumentException("A demo account needs a named time zone.", nameof(timeZone));
        }

        Name = name;
        TimeZone = timeZone;
        CreatedAt = createdAt;
    }

    /// <summary>The account name (FR-028); null when it has none.</summary>
    public string? Name { get; }

    /// <summary>The account's time zone: the installation's default (FR-025).</summary>
    public NamedTimeZone TimeZone { get; }

    /// <summary>When the account is created (FR-030): the time demo loading was given.</summary>
    public DateTimeOffset CreatedAt { get; }
}
