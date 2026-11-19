using HT.Domain.Records;

namespace HT.Application.Records;

/// <summary>
/// One record of an entry (FR-009): its identifier, its date and its value, an answer for a yes/no goal or an amount
/// (minutes for a duration) for a count or duration goal, exactly one of them set.
/// </summary>
public sealed record RecordView(RecordId Id, DateOnly Date, YesNoAnswer? Answer, int? Amount);
