namespace HT.Domain.Records;

/// <summary>
/// Why a record or a note is refused: the date, value, note and limit steps of FR-005, in that order (research R-02).
/// </summary>
public enum RecordProblem
{
    None,
    NothingScheduled,
    DayNotBegun,
    AnswerMissing,
    CountOutOfRange,
    DurationOutOfRange,
    NoteTooLong,
    RecordLimitReached,
}
