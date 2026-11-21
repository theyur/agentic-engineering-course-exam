namespace HT.Domain.Exemptions;

/// <summary>
/// The excusal of one commitment (FR-001): the commitment's date and its optional reason. It is a stored fact with no
/// time of its own (FR-003, FR-025); whether the commitment reads Excused is derived on read by
/// <see cref="GoalExemptions.ExemptionOf"/>.
/// </summary>
public sealed record Excusal(DateOnly Date, ExemptionReason? Reason);
