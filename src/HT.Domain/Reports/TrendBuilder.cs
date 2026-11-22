using HT.Domain.Evaluation;
using HT.Domain.Exemptions;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;

namespace HT.Domain.Reports;

/// <summary>
/// Builds a goal's <see cref="Trend"/> from its begun entries (FR-006, research R-05; slice 7 FR-016, research R-07). A
/// pure function of the targets, the records and the exemptions: no clock, no time zone and no culture, and it reads no
/// evaluation state, so the records of every state count unless the entry is exempt.
/// </summary>
public static class TrendBuilder
{
    /// <summary>The most bars of a trend whose bars are weeks.</summary>
    private const int MaxWeekBars = 12;

    /// <summary>The most bars of a trend whose bars are commitments or days.</summary>
    private const int MaxOtherBars = 14;

    /// <summary>
    /// The trend of a goal whose latest target is <paramref name="latest"/>, from its <paramref name="begun"/>
    /// entries in date order, each with the target of its own settings version and its records. Oldest first, the
    /// latest bars kept:
    /// <list type="bullet">
    ///   <item>a per-week or per-day period: one bar per period, the last 12 weeks or 14 days; the value is the
    ///   period's progress from <see cref="Evaluator.ProgressOf"/>, against N (Build, Reduce yes/no) or the amount
    ///   (Reduce count or duration, in minutes for a duration);</item>
    ///   <item>a commitment-based count or duration goal: one bar per commitment, the last 14; the value is the
    ///   recorded amount as stored (minutes for a duration), against the amount (× 60 for an hours target);</item>
    ///   <item>a commitment-based yes/no goal: one bar per Monday-to-Sunday week with at least one begun commitment,
    ///   the last 12; the value is the week's "Yes" records, for Build and Reduce; the target is the week's
    ///   commitments, the current week's <paramref name="currentWeekUpcoming"/> ones included, for Build, and 0 for
    ///   Reduce.</item>
    /// </list>
    /// A bar with no record has no value; a bar with a record has its value, 0 and values above the target included.
    /// A week's target comes from its first begun commitment's target. No bar is marked: this is the overload with
    /// no exemption.
    /// </summary>
    /// <exception cref="ArgumentException">A record does not match its target's measurement, or a commitment has more than one record.</exception>
    public static Trend Of(
        GoalTarget latest,
        IReadOnlyList<(CalendarEntry Entry, GoalTarget Target, IReadOnlyList<RecordValue> Records)> begun,
        IReadOnlyList<CalendarEntry> currentWeekUpcoming)
    {
        ArgumentNullException.ThrowIfNull(begun);
        return Of(
            latest,
            begun.Select(item => (item.Entry, item.Target, item.Records, (EvaluationState?)null)).ToList(),
            currentWeekUpcoming,
            GoalExemptions.None);
    }

    /// <summary>
    /// The trend as the overload without exemptions gives it, with each begun entry's <c>Exemption</c> (Excused,
    /// Paused or null, as <see cref="GoalExemptions.ExemptionOf"/> gives it) and the goal's
    /// <paramref name="exemptions"/>, from which the exemption of each of <paramref name="currentWeekUpcoming"/>
    /// comes (slice 7 FR-016, research R-07):
    /// <list type="bullet">
    ///   <item>a per-period goal, or a commitment-based count or duration goal: a bar is marked with its entry's
    ///   exemption and has no value; its target is the entry's own;</item>
    ///   <item>a commitment-based yes/no goal: only the week's commitments that are neither Excused nor Paused count,
    ///   the current week's Upcoming ones included, in the Build target (the Reduce target stays 0), and only their
    ///   "Yes" records in the value (Build and Reduce), which has no value when none of them has a record; a week whose
    ///   commitments, the Upcoming ones included, are all Excused or Paused is marked
    ///   <see cref="TrendMark.Paused"/> when one of them is Paused, else <see cref="TrendMark.Excused"/>, with no value
    ///   and the target 0.</item>
    /// </list>
    /// <see cref="Trend.Target"/> is the target of the latest kept bar that is not marked, or 0 when every bar is
    /// marked or there is none. With no exemption anywhere, the result equals the overload without exemptions.
    /// <para>
    /// Every bar and week is drawn in the trend's kind (slice 8 research R-07). An entry whose own target has another
    /// measurement than <paramref name="latest"/> (a settings change within the attempt) contributes only its result:
    /// in a commitment-based yes/no week one "Yes" when it is Met and none otherwise, never its amount; in any other
    /// trend its bar has no value, exactly like an entry without a record, keeps its mark, and is skipped by
    /// <see cref="Trend.Target"/>. A goal without such a change gets exactly slice 7's bars.
    /// </para>
    /// </summary>
    /// <exception cref="ArgumentException">A record does not match its target's measurement, or a commitment has more than one record.</exception>
    /// <exception cref="ArgumentOutOfRangeException">An item's exemption is a state other than Excused, Paused or null.</exception>
    public static Trend Of(
        GoalTarget latest,
        IReadOnlyList<(CalendarEntry Entry, GoalTarget Target, IReadOnlyList<RecordValue> Records, EvaluationState? Exemption)> begun,
        IReadOnlyList<CalendarEntry> currentWeekUpcoming,
        GoalExemptions exemptions)
    {
        ArgumentNullException.ThrowIfNull(latest);
        ArgumentNullException.ThrowIfNull(begun);
        ArgumentNullException.ThrowIfNull(currentWeekUpcoming);
        ArgumentNullException.ThrowIfNull(exemptions);

        var span = SpanOf(latest);
        var maxBars = span == TrendSpan.Week ? MaxWeekBars : MaxOtherBars;
        var bars = latest.ScheduleKindOf == GoalScheduleKind.CommitmentBased && latest.Measurement == Measurement.YesNo
            ? CommitmentWeeks(begun, currentWeekUpcoming, exemptions).Select(bar => (Bar: bar, OfTrend: true)).ToList()
            : begun.Select(item => BarOf(item.Entry, item.Target, item.Records, item.Exemption, latest.Measurement)).ToList();

        var kept = bars.Skip(Math.Max(0, bars.Count - maxBars)).ToArray();
        var line = Array.FindLast(kept, bar => bar.Bar.Mark is null && bar.OfTrend).Bar?.Target ?? 0;
        return new Trend(KindOf(latest), span, [.. kept.Select(bar => bar.Bar)], line, maxBars);
    }

    /// <summary>
    /// A commitment's or a period's bar: its progress, with no value when it has no record; marked, with no value,
    /// when it is exempt. An entry whose own measurement is not <paramref name="trendMeasurement"/> has no value either,
    /// exactly like an entry without a record, and keeps its mark (slice 8 R-07); <c>OfTrend</c> is false for it, so the
    /// trend line skips it.
    /// </summary>
    private static (TrendBar Bar, bool OfTrend) BarOf(
        CalendarEntry entry, GoalTarget target, IReadOnlyList<RecordValue> records, EvaluationState? exemption, Measurement trendMeasurement)
    {
        ArgumentNullException.ThrowIfNull(entry);
        var mark = MarkOf(exemption);
        var progress = Evaluator.ProgressOf(target, records);
        var ofTrend = target.Measurement == trendMeasurement;
        int? value = mark is not null || records.Count == 0 || !ofTrend ? null : progress.Value;
        return (new TrendBar(entry.FirstDate, entry.LastDate, value, progress.Of) { Mark = mark }, ofTrend);
    }

    private static List<TrendBar> CommitmentWeeks(
        IReadOnlyList<(CalendarEntry Entry, GoalTarget Target, IReadOnlyList<RecordValue> Records, EvaluationState? Exemption)> begun,
        IReadOnlyList<CalendarEntry> currentWeekUpcoming,
        GoalExemptions exemptions)
    {
        var bars = new List<TrendBar>();
        foreach (var week in begun.GroupBy(item => MondayOf(item.Entry.FirstDate)))
        {
            var monday = week.Key;

            // Every commitment of the week by its date, the begun ones first, with its mark (null when counted).
            var commitments = new Dictionary<DateOnly, TrendMark?>();
            foreach (var item in week)
            {
                commitments.TryAdd(item.Entry.FirstDate, MarkOf(item.Exemption));
            }

            foreach (var entry in currentWeekUpcoming.Where(entry => MondayOf(entry.FirstDate) == monday))
            {
                commitments.TryAdd(entry.FirstDate, MarkOf(exemptions.ExemptionOf(entry, GoalScheduleKind.CommitmentBased)));
            }

            var recorded = week.Where(item => item.Exemption is null && item.Records.Count > 0).ToList();
            int? value = recorded.Count == 0
                ? null
                : recorded.Sum(item => YesCountOf(item.Target, item.Records));
            // Stryker disable once Linq : a GroupBy group is never empty, so FirstOrDefault gives the same element as First
            var target = week.First().Target.BuildOrReduce == BuildOrReduce.Build
                ? commitments.Values.Count(mark => mark is null)
                : 0;
            TrendMark? weekMark = commitments.Values.All(mark => mark is not null)
                ? commitments.ContainsValue(TrendMark.Paused) ? TrendMark.Paused : TrendMark.Excused
                : null;
            bars.Add(new TrendBar(monday, monday.AddDays(6), value, target) { Mark = weekMark });
        }

        return bars;
    }

    /// <summary>
    /// A commitment's "Yes" count in a commitment-based yes/no week: its "Yes" record (1 or 0) for a yes/no commitment;
    /// for a commitment of another measurement (a change within the attempt, slice 8 R-07) one when it is Met and none
    /// otherwise, never its amount.
    /// </summary>
    private static int YesCountOf(GoalTarget target, IReadOnlyList<RecordValue> records) =>
        target.Measurement == Measurement.YesNo
            ? Evaluator.ProgressOf(target, records).Value ?? 0
            : Evaluator.Meets(target, records) ? 1 : 0;

    /// <summary>The mark of an exemption: none for null, else Excused or Paused.</summary>
    /// <exception cref="ArgumentOutOfRangeException">A state other than Excused, Paused or null.</exception>
    private static TrendMark? MarkOf(EvaluationState? exemption) => exemption switch
    {
        null => null,
        EvaluationState.Excused => TrendMark.Excused,
        EvaluationState.Paused => TrendMark.Paused,
        _ => throw new ArgumentOutOfRangeException(nameof(exemption), "Only Excused or Paused can mark a bar."),
    };

    private static TrendKind KindOf(GoalTarget target) => target.Measurement switch
    {
        Measurement.YesNo => TrendKind.RecordCount,
        _ when target.ScheduleKindOf == GoalScheduleKind.PeriodBased && target.BuildOrReduce == BuildOrReduce.Build => TrendKind.RecordCount,
        Measurement.Duration => TrendKind.Minutes,
        Measurement.Count => TrendKind.Count,
        _ => throw new ArgumentOutOfRangeException(nameof(target), "The measurement is not defined."),
    };

    private static TrendSpan SpanOf(GoalTarget target) => target.PeriodLength switch
    {
        null => target.Measurement == Measurement.YesNo ? TrendSpan.Week : TrendSpan.Commitment,
        SchedulePeriod.Day => TrendSpan.Day,
        SchedulePeriod.Week => TrendSpan.Week,
        _ => throw new ArgumentOutOfRangeException(nameof(target), "The period is not defined."),
    };

    /// <summary>The Monday of the Monday-to-Sunday week of <paramref name="date"/>.</summary>
    private static DateOnly MondayOf(DateOnly date) => date.AddDays(-(((int)date.DayOfWeek + 6) % 7));
}
