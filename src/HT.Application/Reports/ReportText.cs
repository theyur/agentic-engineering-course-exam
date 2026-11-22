using System.Globalization;
using HT.Application.Accounts;
using HT.Application.Goals;
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
    /// <see cref="RecordingText.State"/> for every other state, "Excused" and "Paused" included (slice 7 HP-006, HP-008).
    /// </summary>
    public static string State(EvaluationState state, GoalScheduleKind kind, SchedulePeriod? period) =>
        state == EvaluationState.Upcoming ? "Upcoming" : RecordingText.State(state, kind, period);

    /// <summary>
    /// Slice 7 (HP-001, HP-004, HP-008): the meta line and Notice text of a paused goal, "Paused until Sun 22 Nov", with
    /// <paramref name="lastDay"/>, the last day of its current pause, as <see cref="DateTexts.ShortDate"/> gives it.
    /// </summary>
    public static string PausedUntil(DateOnly lastDay) => "Paused until " + DateTexts.ShortDate(lastDay);

    /// <summary>Slice 7 (HP-004): the Goals page's footnote, worded as the spec gives it.</summary>
    public const string GoalsFootnote =
        "Each goal is counted on its own. Consistency counts only finalized results; pending, excused and paused ones are left out.";

    /// <summary>Slice 7 (HP-004): the Goals page's empty Paused tab.</summary>
    public const string NoPausedGoals = "No paused goals.";

    /// <summary>Slice 8 (HP-005): the Goals page's empty Ended tab.</summary>
    public const string NoEndedGoals = "No ended goals.";

    /// <summary>Slice 8 (HP-006): the Profile's empty Goal history.</summary>
    public const string NoEndedGoalsYet = "No ended goals yet.";

    /// <summary>Slice 8 (HP-007): the Tag after the goal name of a Pending row of an ended goal.</summary>
    public const string EndedTag = "Ended";

    /// <summary>
    /// Slice 8 (FR-007, HP-008): an outcome's word, "Achieved", "Abandoned" or "No longer relevant", from
    /// <see cref="GoalChangeTexts"/>.
    /// </summary>
    public static string OutcomeText(Outcome outcome) => outcome switch
    {
        Outcome.Achieved => GoalChangeTexts.Achieved,
        Outcome.Abandoned => GoalChangeTexts.Abandoned,
        Outcome.NoLongerRelevant => GoalChangeTexts.NoLongerRelevant,
        _ => throw new ArgumentOutOfRangeException(nameof(outcome), "The outcome is not defined."),
    };

    /// <summary>Slice 8 (HP-001, HP-008): the ended goal's Notice, "Ended Mon 16 Nov · Abandoned", the date as <see cref="DateTexts.ShortDate"/> gives it.</summary>
    public static string Ended(DateOnly endingDate, Outcome outcome) => Ended(endingDate) + Separator + OutcomeText(outcome);

    /// <summary>Slice 8 (HP-005, HP-008): an ended goal's meta line, "Ended Mon 16 Nov".</summary>
    public static string Ended(DateOnly endingDate) => "Ended " + DateTexts.ShortDate(endingDate);

    /// <summary>
    /// Slice 8 (HP-007, HP-014): "Attempt 2", the Tag of a Pending row of an attempt that is not the latest and the
    /// eyebrow of the attempt view's GoalStats.
    /// </summary>
    public static string Attempt(int number) => "Attempt " + Number(number);

    /// <summary>Slice 8 (HP-001, HP-008): the attempt line of an attempt that has begun, "Attempt 2 · started 18 Nov".</summary>
    public static string AttemptStarted(int number, DateOnly startDate) => Attempt(number) + Separator + "started " + DateTexts.DayMonth(startDate);

    /// <summary>Slice 8 (HP-001, HP-008): the attempt line of an attempt that has not begun, "Attempt 2 · starts 23 Nov".</summary>
    public static string AttemptStarts(int number, DateOnly startDate) => Attempt(number) + Separator + "starts " + DateTexts.DayMonth(startDate);

    /// <summary>
    /// Slice 8 (HP-001, HP-014): an ended attempt with its first and last date, "Attempt 1 · 2 Nov – 15 Nov", the
    /// dates as <see cref="DateTexts.DayMonth"/> gives them.
    /// </summary>
    public static string AttemptSpan(int number, DateOnly first, DateOnly last) =>
        Attempt(number) + Separator + DateTexts.DayMonth(first) + " – " + DateTexts.DayMonth(last);

    /// <summary>Slice 8 (HP-001, HP-014; plan "Spec discrepancies" 5): an attempt with no date, "Attempt 1 · ended before it began".</summary>
    public static string AttemptWithoutDate(int number) => Attempt(number) + Separator + "ended before it began";

    /// <summary>Slice 8 (FR-005, HP-001): the Settings card's row of the later settings, "From Thu 12 Nov".</summary>
    public static string From(DateOnly appliesFrom) => "From " + DateTexts.ShortDate(appliesFrom);

    /// <summary>Slice 8 (HP-002): the edit page's line above its actions, "Changes apply from Thu 12 Nov. Past results keep their rules.".</summary>
    public static string ChangesApplyFrom(DateOnly appliesFrom) =>
        "Changes apply from " + DateTexts.ShortDate(appliesFrom) + ". Past results keep their rules.";

    /// <summary>Slice 8 (HP-004): the new-attempt page's line, "Attempt 2 starts with its own history and a streak from zero.".</summary>
    public static string NewAttemptLine(int number) => Attempt(number) + " starts with its own history and a streak from zero.";

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
        return ProgressText(goal.SettingsOf(entry), entry);
    }

    /// <summary>
    /// Slice 8 (research R-07): the same progress text with the row's own <paramref name="settings"/>
    /// (<see cref="GoalHistory.SettingsOf"/>): their Build or Reduce, measurement and unit.
    /// </summary>
    /// <exception cref="ArgumentException">A commitment with no recorded value; the message names no value.</exception>
    public static string ProgressText(EntrySettings settings, HistoryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(entry);
        var progress = entry.Progress;
        var when = entry.FirstDate == entry.LastDate ? " today" : " this week";
        var reduce = settings.BuildOrReduce == BuildOrReduce.Reduce;
        switch (progress.Kind)
        {
            case ProgressKind.RecordedValue:
                if (progress.Value is not { } value)
                {
                    throw new ArgumentException("The commitment has no recorded value.", nameof(entry));
                }

                if (settings.Measurement == Measurement.YesNo)
                {
                    return (value > 0 ? "Yes" : "No") + when;
                }

                return RecordingText.Progress(settings, progress) + when;
            case ProgressKind.Total:
                return RecordingText.Progress(settings, progress) + when;
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
    /// The ConsistencyStrip's accessible name (HP-006, <c>design/components/ConsistencyStrip.md</c>, "Slice 6 state" and
    /// "Slice 7 state"): "Last 17 commitments: 10 met, 1 missed, 1 excused, 3 paused, 2 pending", the number of cells
    /// with the unit (singular for exactly 1), then each state in the order met, failed, missed, excused, paused,
    /// pending, a state with 0 left out, so a strip without exempt results keeps slice 6's label; "Last 0 commitments"
    /// with no cell.
    /// </summary>
    /// <exception cref="ArgumentException">A state that is not ended (Due or Upcoming), which has no cell.</exception>
    public static string StripLabel(IReadOnlyList<EvaluationState> strip, StreakUnit unit)
    {
        ArgumentNullException.ThrowIfNull(strip);
        int met = 0, failed = 0, missed = 0, excused = 0, paused = 0, pending = 0;
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
                case EvaluationState.Excused:
                    excused++;
                    break;
                case EvaluationState.Paused:
                    paused++;
                    break;
                case EvaluationState.Pending:
                    pending++;
                    break;
                default:
                    throw new ArgumentException("A strip holds only ended results.", nameof(strip));
            }
        }

        var parts = new List<string>(6);
        AddCount(parts, met, "met");
        AddCount(parts, failed, "failed");
        AddCount(parts, missed, "missed");
        AddCount(parts, excused, "excused");
        AddCount(parts, paused, "paused");
        AddCount(parts, pending, "pending");
        var label = "Last " + Streak(strip.Count, unit);
        return parts.Count == 0 ? label : label + ": " + string.Join(", ", parts);
    }

    /// <summary>
    /// The TrendChart's accessible name (HP-006, <c>design/components/TrendChart.md</c>): "{title}, {sub line with a
    /// lower-case first letter}; target {n}" ("Minutes per day, last 14 days; target 20"), then for each bar whose own
    /// target differs from the line's "; {date}: {value} of {target}" or "; {date}: no record of {target}", {date}
    /// being "Wed 4 Nov" for a commitment or a day and "week of 2 Nov" for a week.
    /// <para>
    /// Slice 7 (FR-016, HP-006; <c>design/components/TrendChart.md</c>, "Slice 7 state"): the marked bars are counted
    /// after the target, "; {a} excused, {b} paused", a count of 0 left out, and nothing is added with no marked bar
    /// ("Minutes per day, last 14 days; target 10; 1 excused, 3 paused"). Bars with no record are not counted, and a
    /// marked bar is never listed among the bars whose own target differs.
    /// </para>
    /// </summary>
    public static string TrendLabel(TrendView trend)
    {
        ArgumentNullException.ThrowIfNull(trend);
        var label = trend.Title + ", " + LowerFirst(trend.SubLine) + "; target " + Number(trend.Target);
        var marks = new List<string>(2);
        AddCount(marks, trend.Bars.Count(bar => bar.Mark == TrendMark.Excused), "excused");
        AddCount(marks, trend.Bars.Count(bar => bar.Mark == TrendMark.Paused), "paused");
        if (marks.Count > 0)
        {
            label += "; " + string.Join(", ", marks);
        }

        foreach (var bar in trend.Bars.Where(bar => bar.Mark is null && bar.Target != trend.Target))
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
    /// commitment or a day and "week of 2 Nov" for a week. The target is the bar's own. Slice 7 (FR-016, HP-006): a marked
    /// bar says its mark in place of a value, "{date}: Excused" or "{date}: Paused" ("Mon 9 Nov: Excused", "week of 21
    /// Sep: Paused").
    /// </summary>
    public static string BarTitle(TrendView trend, TrendBar bar)
    {
        ArgumentNullException.ThrowIfNull(trend);
        ArgumentNullException.ThrowIfNull(bar);
        var date = BarDate(trend.Span, bar);
        if (bar.Mark is { } mark)
        {
            return date + ": " + (mark == TrendMark.Excused ? "Excused" : "Paused");
        }

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
