namespace HT.Domain.Exemptions;

/// <summary>The limits of excusals and pauses (FR-001, FR-006).</summary>
public static class ExemptionLimits
{
    /// <summary>The longest reason of an excusal or a pause, in UTF-16 code units after trimming (FR-001).</summary>
    public const int MaxReasonLength = 200;
}
