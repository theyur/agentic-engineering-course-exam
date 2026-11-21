namespace HT.Domain.Exemptions;

/// <summary>
/// The reason of an excusal or a pause (FR-001, FR-006; research R-02): trimmed, 1 to
/// <see cref="ExemptionLimits.MaxReasonLength"/> UTF-16 code units, the rule of <c>NoteText</c>. Private to the goal's
/// owner; <see cref="object.ToString"/> is not overridden, so the text never reaches a log or a message by accident
/// (HP-009). Two reasons with the same text are equal (ordinal).
/// </summary>
public sealed class ExemptionReason : IEquatable<ExemptionReason>
{
    private ExemptionReason(string value) => Value = value;

    /// <summary>The trimmed text.</summary>
    public string Value { get; }

    /// <summary>
    /// Trims the text. Null, empty or only spaces give no reason (<paramref name="reason"/> null) and true; more than
    /// 200 code units after trimming give false with <paramref name="tooLong"/> set.
    /// </summary>
    public static bool TryCreate(string? text, out ExemptionReason? reason, out bool tooLong)
    {
        reason = null;
        tooLong = false;
        var trimmed = text?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
        {
            return true;
        }

        if (trimmed.Length > ExemptionLimits.MaxReasonLength)
        {
            tooLong = true;
            return false;
        }

        reason = new ExemptionReason(trimmed);
        return true;
    }

    public bool Equals(ExemptionReason? other) => other is not null && string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => Equals(obj as ExemptionReason);

    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);
}
