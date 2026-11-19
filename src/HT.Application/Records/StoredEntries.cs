using HT.Domain.Records;

namespace HT.Application.Records;

/// <summary>
/// What <see cref="IRecordStore.ReadAsync"/> reads for a range of dates: the records, in date order and then in the
/// stored order (FR-009), and the notes by their entry key.
/// </summary>
public sealed record StoredEntries(IReadOnlyList<GoalRecord> Records, IReadOnlyDictionary<DateOnly, NoteText> Notes);
