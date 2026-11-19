namespace HT.Domain.Records;

/// <summary>
/// The text of a note (FR-007): trimmed, 1 to <see cref="RecordLimits.MaxNoteLength"/> UTF-16 code units, the rule
/// of <c>AccountName</c> and <c>GoalName</c>. Private to the goal's owner; <see cref="object.ToString"/> is not
/// overridden, so the text never reaches a log or a message by accident (HP-006). Two notes with the same text are
/// equal (ordinal).
/// </summary>
public sealed class NoteText : IEquatable<NoteText>
{
    private NoteText(string value) => Value = value;

    /// <summary>The trimmed text.</summary>
    public string Value { get; }

    /// <summary>
    /// Trims the text. Null, empty or only spaces give no note (<paramref name="note"/> null) and true; more than 500
    /// code units after trimming give false with <paramref name="tooLong"/> set.
    /// </summary>
    public static bool TryCreate(string? text, out NoteText? note, out bool tooLong)
    {
        note = null;
        tooLong = false;
        var trimmed = text?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
        {
            return true;
        }

        if (trimmed.Length > RecordLimits.MaxNoteLength)
        {
            tooLong = true;
            return false;
        }

        note = new NoteText(trimmed);
        return true;
    }

    public bool Equals(NoteText? other) => other is not null && string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => Equals(obj as NoteText);

    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);
}
