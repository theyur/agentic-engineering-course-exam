namespace HT.Domain.Goals;

/// <summary>
/// One scheduled day of a commitment-based goal (FR-003, FR-013): its date, and the instants (UTC) at which it starts
/// and ends, at midnight in its own version's time zone (FR-012). No status (slice 5).
/// </summary>
public sealed record Commitment(DateOnly Date, DateTimeOffset Starts, DateTimeOffset Ends);
