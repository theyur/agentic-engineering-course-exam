namespace HT.Domain.Reports;

/// <summary>
/// One bar of a trend (FR-006, research R-05): its first and last date (a week: its Monday and its Sunday), its value
/// and its own target. <paramref name="Value"/> is null when the bar has no record, and given as recorded otherwise,
/// 0 and values above the target included.
/// </summary>
public sealed record TrendBar(DateOnly FirstDate, DateOnly LastDate, int? Value, int Target);
