using HT.Domain.Records;

namespace HT.Application.Operator;

/// <summary>
/// One record of a demo history goal (FR-030, research R-08): its date and its value, which has passed the Domain
/// checks for the goal's measurement. <see cref="RecordValue"/> does not override <see cref="object.ToString"/>, so
/// the record's text never shows the value (HP-006).
/// </summary>
public sealed record DemoRecord(DateOnly Date, RecordValue Value);
