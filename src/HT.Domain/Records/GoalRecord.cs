namespace HT.Domain.Records;

/// <summary>
/// One record of one goal on one date (FR-001, FR-003): its identifier, the date it belongs to (never the instant it
/// was saved, ADR 0016) and its value.
/// </summary>
public sealed record GoalRecord(RecordId Id, DateOnly Date, RecordValue Value);
