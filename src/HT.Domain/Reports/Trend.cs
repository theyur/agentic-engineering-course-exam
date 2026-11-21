namespace HT.Domain.Reports;

/// <summary>
/// A goal's trend (FR-006, research R-05): what its values are (<paramref name="Kind"/>), what one bar covers
/// (<paramref name="Span"/>), its <paramref name="Bars"/> oldest first and at most <paramref name="MaxBars"/> (12 for
/// weeks, 14 otherwise), and <paramref name="Target"/>, the latest unmarked bar's target, where the page draws its
/// line (0 with no unmarked bar; slice 7 research R-07). Made by <see cref="TrendBuilder"/>.
/// </summary>
public sealed record Trend(TrendKind Kind, TrendSpan Span, IReadOnlyList<TrendBar> Bars, int Target, int MaxBars);
