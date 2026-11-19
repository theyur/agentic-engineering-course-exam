namespace HT.Domain.Evaluation;

/// <summary>What a <see cref="Progress"/> counts (FR-014, research R-01).</summary>
public enum ProgressKind
{
    /// <summary>A commitment's recorded value against its amount, or 1 for yes/no (Yes 1, No 0).</summary>
    RecordedValue,

    /// <summary>A Build period's records that reach the amount (yes/no: its Yes records) against N.</summary>
    RecordsReachingAmount,

    /// <summary>A Reduce yes/no period's Yes records against at most N.</summary>
    YesRecords,

    /// <summary>A Reduce count or duration period's total against at most the amount.</summary>
    Total,
}
