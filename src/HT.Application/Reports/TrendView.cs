using HT.Domain.Habits;
using HT.Domain.Reports;

namespace HT.Application.Reports;

/// <summary>
/// A goal's trend as the goal page draws it (FR-006, research R-05, R-13): the bars, oldest first, each with its own
/// target, the target of the latest bar, and the card's texts from <see cref="ReportText"/>. Durations are in minutes.
/// </summary>
public sealed record TrendView
{
    /// <summary>What the bar values are: minutes, an amount counted in <see cref="CountUnit"/>, or a number of records.</summary>
    public required TrendKind Kind { get; init; }

    /// <summary>What one bar covers: a commitment, a day or a week.</summary>
    public required TrendSpan Span { get; init; }

    public required Measurement Measurement { get; init; }

    /// <summary>The count unit as stored; null unless the measurement is count.</summary>
    public string? CountUnit { get; init; }

    /// <summary>Oldest first; a null value is a bar with no record.</summary>
    public required IReadOnlyList<TrendBar> Bars { get; init; }

    /// <summary>The latest bar's target, where the dashed line is drawn; 0 with no bar.</summary>
    public required int Target { get; init; }

    /// <summary><see cref="ReportText.TrendTitle"/>: "Minutes per day".</summary>
    public required string Title { get; init; }

    /// <summary><see cref="ReportText.TrendSubLine"/>: "Last 14 days".</summary>
    public required string SubLine { get; init; }

    /// <summary><see cref="ReportText.TargetLine"/>: "Target 20 min", "Target 3".</summary>
    public required string TargetLine { get; init; }

    /// <summary><see cref="ReportText.TrendLabel"/>: "Minutes per day, last 14 days; target 20".</summary>
    public required string Label { get; init; }
}
