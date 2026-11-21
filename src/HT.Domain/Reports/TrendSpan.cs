namespace HT.Domain.Reports;

/// <summary>What one trend bar covers (FR-006, research R-05).</summary>
public enum TrendSpan
{
    /// <summary>One commitment of a commitment-based count or duration goal.</summary>
    Commitment,

    /// <summary>One day of a per-day goal.</summary>
    Day,

    /// <summary>One Monday-to-Sunday week: a per-week goal's period, or a commitment-based yes/no goal's week.</summary>
    Week,
}
