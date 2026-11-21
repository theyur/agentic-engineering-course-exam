using HT.Domain.Exemptions;

namespace HT.Application.Operator;

/// <summary>
/// One excusal of a demo pause goal (FR-031, research R-08): the commitment's date and its reason or none, stored
/// directly as the fact of FR-025. <see cref="ExemptionReason"/> does not override <see cref="object.ToString"/>, so
/// the excusal's text never shows the reason (HP-009).
/// </summary>
public sealed record DemoExcusal(DateOnly Date, ExemptionReason? Reason);
