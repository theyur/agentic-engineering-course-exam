using HT.Domain.Records;

namespace HT.Application.Records;

/// <summary>The kinds of <see cref="NoteWrite"/>.</summary>
public enum NoteWriteKind
{
    /// <summary>Leave the entry's note as it is.</summary>
    Keep,

    /// <summary>Delete the entry's note, if there is one.</summary>
    Remove,

    /// <summary>Store <see cref="NoteWrite.Text"/> as the entry's note, adding or replacing it.</summary>
    Replace,
}

/// <summary>
/// What a record's write of <see cref="IRecordStore.AddAsync"/> or <see cref="IRecordStore.ChangeAsync"/> does to its
/// entry's note (FR-001, D-1; research R-01), in the record's own transaction: <see cref="Keep"/> leaves it,
/// <see cref="Remove"/> deletes it, and <see cref="Replace"/> adds or replaces it. Port plumbing, not a use-case type.
/// <see cref="object.ToString"/> is not overridden, so a note never reaches a log or a message (HP-007).
/// </summary>
public sealed class NoteWrite
{
    private NoteWrite(NoteWriteKind kind, NoteText? text)
    {
        Kind = kind;
        Text = text;
    }

    /// <summary>Leave the entry's note as it is: no note was given.</summary>
    public static NoteWrite Keep { get; } = new(NoteWriteKind.Keep, null);

    /// <summary>Delete the entry's note, if there is one: the note given was empty or only spaces.</summary>
    public static NoteWrite Remove { get; } = new(NoteWriteKind.Remove, null);

    public NoteWriteKind Kind { get; }

    /// <summary>The note to store; set only when <see cref="Kind"/> is <see cref="NoteWriteKind.Replace"/>, null otherwise.</summary>
    public NoteText? Text { get; }

    /// <summary>Store <paramref name="text"/> as the entry's note, adding or replacing it.</summary>
    public static NoteWrite Replace(NoteText text)
    {
        ArgumentNullException.ThrowIfNull(text);
        return new NoteWrite(NoteWriteKind.Replace, text);
    }
}
