namespace HT.Domain.Reports;

/// <summary>The mark of a trend bar whose commitment, day or week is exempt (slice 7 FR-016, research R-07).</summary>
public enum TrendMark
{
    /// <summary>An Excused commitment, or a commitment-based yes/no week whose commitments are all Excused.</summary>
    Excused,

    /// <summary>A Paused commitment, day or week, or a commitment-based yes/no week whose commitments are all exempt, one of them Paused.</summary>
    Paused,
}
