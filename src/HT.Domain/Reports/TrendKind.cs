namespace HT.Domain.Reports;

/// <summary>What a trend bar's value is (FR-006, research R-05).</summary>
public enum TrendKind
{
    /// <summary>An amount in minutes: a commitment-based duration goal, or a Reduce duration period.</summary>
    Minutes,

    /// <summary>A count amount: a commitment-based count goal, or a Reduce count period.</summary>
    Count,

    /// <summary>A number of records: "Yes" records, or a Build period's records that reach the amount.</summary>
    RecordCount,
}
