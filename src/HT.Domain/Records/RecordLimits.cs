namespace HT.Domain.Records;

/// <summary>The limits of records and notes (FR-003 to FR-005, FR-007; D-8).</summary>
public static class RecordLimits
{
    /// <summary>The most records an evaluation period holds (FR-003).</summary>
    public const int MaxRecordsPerPeriod = 50;

    /// <summary>The largest count a record can have (FR-005).</summary>
    public const int MaxCount = 9999;

    /// <summary>The largest duration a record can have, in minutes (FR-005).</summary>
    public const int MaxMinutes = 1440;

    /// <summary>The longest note, in UTF-16 code units after trimming (FR-007).</summary>
    public const int MaxNoteLength = 500;
}
