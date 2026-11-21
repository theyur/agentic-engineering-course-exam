namespace HT.Domain.Reports;

/// <summary>
/// A goal's trend (FR-006, research R-05): what its values are (<paramref name="Kind"/>), what one bar covers
/// (<paramref name="Span"/>), its <paramref name="Bars"/> oldest first and at most <paramref name="MaxBars"/> (12 for
/// weeks, 14 otherwise), and <paramref name="Target"/>, the latest bar's target, where the page draws its line (0
/// with no bar). Made by <see cref="TrendBuilder.Of"/>.
/// </summary>
public sealed record Trend(TrendKind Kind, TrendSpan Span, IReadOnlyList<TrendBar> Bars, int Target, int MaxBars);
