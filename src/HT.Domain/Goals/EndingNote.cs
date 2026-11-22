using HT.Domain.Records;

namespace HT.Domain.Goals;

/// <summary>
/// The optional note of an ending (FR-007, FR-027): the trimmed text, 1 to <see cref="RecordLimits.MaxNoteLength"/>
/// UTF-16 code units, the rule of <see cref="GoalName"/>. Not a <see cref="NoteText"/>, because an ending note is not a
/// note of a commitment or period. Private to the goal's owner: <see cref="ToString"/> returns a fixed placeholder,
/// never the text, so it never reaches a log or a message by accident (HP-010). Two notes with the same text are equal
/// (ordinal).
/// </summary>
public readonly record struct EndingNote
{
    /// <summary>What <see cref="ToString"/> returns in place of the text.</summary>
    private const string Placeholder = "[ending note]";

    private EndingNote(string value) => Value = value;

    /// <summary>The trimmed text.</summary>
    public string Value { get; }

    /// <summary>
    /// Trims the text. Null, empty or only spaces give no note (<paramref name="note"/> null) and true; more than 500
    /// code units after trimming give false with <paramref name="tooLong"/> set.
    /// </summary>
    public static bool TryCreate(string? text, out EndingNote? note, out bool tooLong)
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

        note = new EndingNote(trimmed);
        return true;
    }

    /// <summary>A fixed placeholder, never the text (HP-010).</summary>
    public override string ToString() => Placeholder;
}
