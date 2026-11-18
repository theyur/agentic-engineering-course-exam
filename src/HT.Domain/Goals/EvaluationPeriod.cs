namespace HT.Domain.Goals;

/// <summary>
/// One evaluation period of a period-based goal (FR-003, FR-013): a day (first and last date equal) or a Monday-to-Sunday
/// week, with the instants (UTC) at which it starts and ends (FR-012). No status (slice 5).
/// </summary>
public sealed record EvaluationPeriod(DateOnly FirstDate, DateOnly LastDate, DateTimeOffset Starts, DateTimeOffset Ends);
