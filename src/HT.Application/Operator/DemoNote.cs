using HT.Domain.Records;

namespace HT.Application.Operator;

/// <summary>
/// One note of a demo history goal (FR-030, research R-08), under its entry key: the day, or a week's Monday.
/// <see cref="NoteText"/> does not override <see cref="object.ToString"/>, so the note's text never shows it (HP-006).
/// </summary>
public sealed record DemoNote(DateOnly EntryDate, NoteText Text);
