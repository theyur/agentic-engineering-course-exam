using HT.Domain.Accounts;
using HT.Domain.Time;

namespace HT.Application.Accounts;

/// <summary>
/// The profile a new account is created with (research R-04): its own name or none, a concrete named time zone and
/// the supplied creation time. Application supplies all three, so the store never needs to know the default zone.
/// </summary>
public sealed record NewAccountProfile(AccountName? Name, NamedTimeZone TimeZone, DateTimeOffset CreatedAt);
