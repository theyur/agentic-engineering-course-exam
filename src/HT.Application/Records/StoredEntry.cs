using HT.Domain.Records;

namespace HT.Application.Records;

/// <summary>
/// One entry's records, in date order and then in the stored order (FR-009), and its note, as the record store reads
/// them after a write.
/// </summary>
public sealed record StoredEntry(IReadOnlyList<GoalRecord> Records, NoteText? Note);
