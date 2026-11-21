namespace HT.Domain.Reports;

/// <summary>What a goal's streak counts (FR-004): commitments, days or weeks, by its schedule kind and period.</summary>
public enum StreakUnit
{
    /// <summary>A commitment-based goal (fixed days).</summary>
    Commitments,

    /// <summary>A per-day goal.</summary>
    Days,

    /// <summary>A per-week goal.</summary>
    Weeks,
}
