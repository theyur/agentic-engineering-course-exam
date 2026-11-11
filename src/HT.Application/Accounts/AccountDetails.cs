using HT.Domain.Accounts;

namespace HT.Application.Accounts;

/// <summary>An account's own details (FR-012): the email in its normal form and the role.</summary>
public sealed record AccountDetails(string Email, Role Role);
