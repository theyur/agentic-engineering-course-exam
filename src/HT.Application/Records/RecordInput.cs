using HT.Domain.Records;

namespace HT.Application.Records;

/// <summary>
/// A record as the RecordDialog sends it (data-model.md, "Inputs and views"); every value may be missing, so every
/// refusal of FR-005 can be expressed. Values that do not belong to the goal's measurement are dropped (FR-004).
/// </summary>
public sealed record RecordInput
{
    /// <summary>The answer of a yes/no goal; dropped for count and duration.</summary>
    public YesNoAnswer? Answer { get; init; }

    /// <summary>The amount of a count goal, or the minutes of a duration goal; dropped for yes/no.</summary>
    public int? Amount { get; init; }

    /// <summary>
    /// The note given with the record (FR-007, specs/006-note-editing FR-001): null keeps the entry's note; a text empty
    /// after trimming removes it; any other text, trimmed, becomes the note.
    /// </summary>
    public string? Note { get; init; }
}
