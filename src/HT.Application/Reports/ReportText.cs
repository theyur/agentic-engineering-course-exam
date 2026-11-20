using System.Globalization;
using HT.Application.Accounts;
using HT.Application.Records;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Reports;

namespace HT.Application.Reports;

/// <summary>
/// The one formatter of the report texts (HP-005, research R-13): consistency, streaks and their units, the state
/// words, the meta line's progress and time left, the trend's title, sub line, target line and accessible name, and
/// the strip's accessible name, worded as the design's slice-6 state words them (<c>design/components/GoalStats.md</c>,
/// <c>ConsistencyStrip.md</c>, <c>TrendChart.md</c>, <c>GoalListItem.md</c>, <c>CommitmentRow.md</c>,
/// <c>StatusChip.md</c>). Pure and culture-invariant: numbers use the invariant culture, dates come only from
/// <see cref="DateTexts"/>, and the time is a parameter, never a clock. No text names a goal, a habit or a note.
/// </summary>
public static class ReportText
{
    private const string Separator = " · ";

    /// <summary>The consistency (FR-002): "90% (18 of 20)", "66% (2 of 3)", or "N/A" with no counted result.</summary>
    public static string Consistency(Consistency consistency)
    {
        ArgumentNullException.ThrowIfNull(consistency);
        return consistency.Percentage is { } percentage
            ? Number(percentage) + "% (" + Number(consistency.Successful) + " of " + Number(consistency.Counted) + ")"
            : "N/A";
    }

    /// <summary>The streak unit for <paramref name="count"/> (FR-004): singular for exactly 1, plural otherwise.</summary>
    public static string Unit(StreakUnit unit, int count) => unit switch
    {
        StreakUnit.Commitments => count == 1 ? "commitment" : "commitments",
        StreakUnit.Days => count == 1 ? "day" : "days",
        StreakUnit.Weeks => count == 1 ? "week" : "weeks",
        _ => throw new ArgumentOutOfRangeException(nameof(unit), "The streak unit is not defined."),
    };

    /// <summary>A streak with its unit (FR-004): "6 commitments", "1 day", "0 weeks".</summary>
    public static string Streak(int count, StreakUnit unit) => Number(count) + " " + Unit(unit, count);

    /// <summary>The GoalStats label of the current streak, with the plural unit: "Streak, commitments".</summary>
    public static string StreakLabel(StreakUnit unit) => "Streak, " + Plural(unit);

    /// <summary>The GoalStats label of the longest streak, with the plural unit: "Longest, commitments".</summary>
    public static string LongestLabel(StreakUnit unit) => "Longest, " + Plural(unit);

    /// <summary>The GoalSummary line (HP-003): "Streak 6 commitments · longest 11".</summary>
    public static string SummaryLine(Streaks streaks, StreakUnit unit)
    {
        ArgumentNullException.ThrowIfNull(streaks);
        return "Streak " + Streak(streaks.Current, unit) + Separator + "longest " + Number(streaks.Longest);
    }

    /// <summary>The GoalSummary line with no final result (HP-003): "No finalized commitments yet".</summary>
    public static string NoFinalYet(StreakUnit unit) => "No finalized " + Plural(unit) + " yet";

    /// <summary>
    /// The StatusChip's word (constitution I, research R-12): "Upcoming" for Upcoming, and slice 5's
    /// <see cref="RecordingText.State"/> for every other state.
    /// </summary>
    public static string State(EvaluationState state, GoalScheduleKind kind, SchedulePeriod? period) =>
        state == EvaluationState.Upcoming ? "Upcoming" : RecordingText.State(state, kind, period);

    /// <summary>
    /// The progress of a Due entry with a record in a meta line (HP-002, HP-003), followed by " today" for a commitment
    /// or a day and " this week" for a week: "12 of 20 min today", "2 of at most 3 cups today" (amounts, as
    /// <see cref="RecordingText.Progress"/> gives them); "1 of 3 this week", "1 of at most 2 today" (numbers of records);
    /// "Yes today" or "No today" for a yes/no commitment, whose value is its answer.
    /// </summary>
    /// <exception cref="ArgumentException">A commitment with no recorded value; the message names no value.</exception>
    public static string ProgressText(GoalHistory goal, HistoryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(goal);
        ArgumentNullException.ThrowIfNull(entry);
        var progress = entry.Progress;
        var when = entry.FirstDate == entry.LastDate ? " today" : " this week";
        var reduce = goal.BuildOrReduce == BuildOrReduce.Reduce;
        switch (progress.Kind)
        {
            case ProgressKind.RecordedValue:
                if (progress.Value is not { } value)
                {
                    throw new ArgumentException("The commitment has no recorded value.", nameof(entry));
                }

                if (goal.Measurement == Measurement.YesNo)
                {
                    return (value > 0 ? "Yes" : "No") + when;
                }

                return RecordingText.Progress(goal, progress) + when;
            case ProgressKind.Total:
                return RecordingText.Progress(goal, progress) + when;
            case ProgressKind.RecordsReachingAmount:
            case ProgressKind.YesRecords:
                return Number(progress.Value ?? 0) + (reduce ? " of at most " : " of ") + Number(progress.Of) + when;
            default:
                throw new ArgumentOutOfRangeException(nameof(entry), "The progress kind is not defined.");
        }
    }

    /// <summary>
    /// A Pending row's time left (HP-003): "5 h left to record", the whole hours to the grace end rounded up, as
    /// <see cref="RecordingText.HoursLeft"/> gives them.
    /// </summary>
    public static string PendingLeft(DateTimeOffset graceEnds, DateTimeOffset now) =>
        RecordingText.HoursLeft(graceEnds, now).ToString(CultureInfo.InvariantCulture) + " h left to record";

    /// <summary>
    /// The TrendChart's title (<c>design/components/TrendChart.md</c>, "Title and sub line"): "Days done per week" for a
    /// commitment-based yes/no goal; "Minutes per day" or "Minutes per week" when the bars are minutes; the goal's count
    /// unit as stored with its first letter upper-case and " per day" or " per week" when they are an amount ("Pages
    /// per day", "Sweet per day"); "Times per day" or "Times per week" for every other number of records. A commitment
    /// is "per day".
    /// </summary>
    /// <exception cref="ArgumentException">An amount of a count goal without a count unit; the message names no value.</exception>
    public static string TrendTitle(GoalTarget target, TrendKind kind, TrendSpan span)
    {
        ArgumentNullException.ThrowIfNull(target);
        var per = span == TrendSpan.Week ? " per week" : " per day";
        return kind switch
        {
            TrendKind.Minutes => "Minutes" + per,
            TrendKind.Count => Capitalized(target.CountUnit) + per,
            TrendKind.RecordCount => target.ScheduleKindOf == GoalScheduleKind.CommitmentBased ? "Days done per week" : "Times" + per,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), "The trend kind is not defined."),
        };
    }

    /// <summary>
    /// The TrendChart's sub line (<c>design/components/TrendChart.md</c>, "Title and sub line"), naming the most bars
    /// the card can show: "Last 12 weeks" for weeks, "Last 14 days" for days and for the commitments of a goal on all
    /// seven days, "Last 14 commitments" for the commitments of a goal on fewer days.
    /// </summary>
    public static string TrendSubLine(GoalTarget target, TrendSpan span, int maxBars)
    {
        ArgumentNullException.ThrowIfNull(target);
        var unit = span switch
        {
            TrendSpan.Week => StreakUnit.Weeks,
            TrendSpan.Day => StreakUnit.Days,
            TrendSpan.Commitment => target.Schedule.Days.Count == 7 ? StreakUnit.Days : StreakUnit.Commitments,
            _ => throw new ArgumentOutOfRangeException(nameof(span), "The trend span is not defined."),
        };
        return "Last " + Streak(maxBars, unit);
    }

    /// <summary>The dashed target line's label (HP-001): "Target 20 min" when the bars are minutes, "Target 3" otherwise.</summary>
    public static string TargetLine(TrendKind kind, int target) =>
        "Target " + Number(target) + (kind == TrendKind.Minutes ? " min" : string.Empty);

    /// <summary>
    /// The ConsistencyStrip's accessible name (HP-006, <c>design/components/ConsistencyStrip.md</c>, "Slice 6 state"):
    /// "Last 28 commitments: 24 met, 1 failed, 1 missed, 2 pending", the number of cells with the unit (singular for
    /// exactly 1), then each state in the order met, failed, missed, pending, a state with 0 left out; "Last 0
    /// commitments" with no cell.
    /// </summary>
    /// <exception cref="ArgumentException">A state that is not ended (Due or Upcoming), which has no cell.</exception>
    public static string StripLabel(IReadOnlyList<EvaluationState> strip, StreakUnit unit)
    {
        ArgumentNullException.ThrowIfNull(strip);
        int met = 0, failed = 0, missed = 0, pending = 0;
        foreach (var state in strip)
        {
            switch (state)
            {
                case EvaluationState.Met:
                    met++;
                    break;
                case EvaluationState.Failed:
                    failed++;
                    break;
                case EvaluationState.Missed:
                    missed++;
                    break;
                case EvaluationState.Pending:
                    pending++;
                    break;
                default:
                    throw new ArgumentException("A strip holds only ended results.", nameof(strip));
            }
        }

        var parts = new List<string>(4);
        AddCount(parts, met, "met");
        AddCount(parts, failed, "failed");
        AddCount(parts, missed, "missed");
        AddCount(parts, pending, "pending");
        var label = "Last " + Streak(strip.Count, unit);
        return parts.Count == 0 ? label : label + ": " + string.Join(", ", parts);
    }

    /// <summary>
    /// The TrendChart's accessible name (HP-006, <c>design/components/TrendChart.md</c>): "{title}, {sub line with a
    /// lower-case first letter}; target {n}" ("Minutes per day, last 14 days; target 20"), then for each bar whose own
    /// target differs from the line's "; {date}: {value} of {target}" or "; {date}: no record of {target}", {date}
    /// being "Wed 4 Nov" for a commitment or a day and "week of 2 Nov" for a week.
    /// </summary>
    public static string TrendLabel(TrendView trend)
    {
        ArgumentNullException.ThrowIfNull(trend);
        var label = trend.Title + ", " + LowerFirst(trend.SubLine) + "; target " + Number(trend.Target);
        foreach (var bar in trend.Bars.Where(bar => bar.Target != trend.Target))
        {
            var value = bar.Value is { } recorded ? Number(recorded) : "no record";
            label += "; " + BarDate(trend.Span, bar) + ": " + value + " of " + Number(bar.Target);
        }

        return label;
    }

    /// <summary>
    /// A TrendChart bar's tooltip (<c>design/components/TrendChart.md</c>, "Target line, tooltips and accessible name"):
    /// "{date}: {value} of {target}", with " min" after the target when the bars are minutes ("Wed 4 Nov: 20 of 20
    /// min", "week of 2 Nov: 2 of 2"), or "{date}: no record" for a bar with no record; {date} is "Wed 4 Nov" for a
    /// commitment or a day and "week of 2 Nov" for a week. The target is the bar's own.
    /// </summary>
    public static string BarTitle(TrendView trend, TrendBar bar)
    {
        ArgumentNullException.ThrowIfNull(trend);
        ArgumentNullException.ThrowIfNull(bar);
        var date = BarDate(trend.Span, bar);
        return bar.Value is { } value
            ? date + ": " + Number(value) + " of " + Number(bar.Target) + (trend.Kind == TrendKind.Minutes ? " min" : string.Empty)
            : date + ": no record";
    }

    /// <summary>A bar's date in the TrendChart's texts: "Wed 4 Nov" for a commitment or a day, "week of 2 Nov" for a week.</summary>
    private static string BarDate(TrendSpan span, TrendBar bar) =>
        span == TrendSpan.Week ? "week of " + DateTexts.DayMonth(bar.FirstDate) : DateTexts.ShortDate(bar.FirstDate);

    private static string Plural(StreakUnit unit) => Unit(unit, 0);

    private static void AddCount(List<string> parts, int count, string word)
    {
        if (count > 0)
        {
            parts.Add(Number(count) + " " + word);
        }
    }

    private static string Capitalized(string? countUnit) =>
        string.IsNullOrEmpty(countUnit)
            ? throw new ArgumentException("A count goal has a count unit.", nameof(countUnit))
            : char.ToUpperInvariant(countUnit[0]) + countUnit[1..];

    private static string LowerFirst(string text) =>
        text.Length == 0 ? text : char.ToLowerInvariant(text[0]) + text[1..];

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);
}
