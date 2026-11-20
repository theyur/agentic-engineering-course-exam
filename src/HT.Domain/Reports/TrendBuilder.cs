using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;

namespace HT.Domain.Reports;

/// <summary>
/// Builds a goal's <see cref="Trend"/> from its begun entries (FR-006, research R-05). A pure function of the targets
/// and the records: no clock, no time zone and no culture, and it reads no state, so the records of every state count.
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
    /// A week's target comes from its first begun commitment's target.
    /// </summary>
    /// <exception cref="ArgumentException">A record does not match its target's measurement, or a commitment has more than one record.</exception>
    public static Trend Of(
        GoalTarget latest,
        IReadOnlyList<(CalendarEntry Entry, GoalTarget Target, IReadOnlyList<RecordValue> Records)> begun,
        IReadOnlyList<CalendarEntry> currentWeekUpcoming)
    {
        ArgumentNullException.ThrowIfNull(latest);
        ArgumentNullException.ThrowIfNull(begun);
        ArgumentNullException.ThrowIfNull(currentWeekUpcoming);

        var span = SpanOf(latest);
        var maxBars = span == TrendSpan.Week ? MaxWeekBars : MaxOtherBars;
        var bars = latest.ScheduleKindOf == GoalScheduleKind.CommitmentBased && latest.Measurement == Measurement.YesNo
            ? CommitmentWeeks(begun, currentWeekUpcoming)
            : begun.Select(item => BarOf(item.Entry, item.Target, item.Records)).ToList();

        var kept = bars.Skip(Math.Max(0, bars.Count - maxBars)).ToArray();
        return new Trend(KindOf(latest), span, kept, kept.Length == 0 ? 0 : kept[^1].Target, maxBars);
    }

    /// <summary>A commitment's or a period's bar: its progress, with no value when it has no record.</summary>
    private static TrendBar BarOf(CalendarEntry entry, GoalTarget target, IReadOnlyList<RecordValue> records)
    {
        ArgumentNullException.ThrowIfNull(entry);
        var progress = Evaluator.ProgressOf(target, records);
        return new TrendBar(entry.FirstDate, entry.LastDate, records.Count == 0 ? null : progress.Value, progress.Of);
    }

    private static List<TrendBar> CommitmentWeeks(
        IReadOnlyList<(CalendarEntry Entry, GoalTarget Target, IReadOnlyList<RecordValue> Records)> begun,
        IReadOnlyList<CalendarEntry> currentWeekUpcoming)
    {
        var bars = new List<TrendBar>();
        foreach (var week in begun.GroupBy(item => MondayOf(item.Entry.FirstDate)))
        {
            var monday = week.Key;
            var recorded = week.Where(item => item.Records.Count > 0).ToList();
            int? value = recorded.Count == 0
                ? null
                : recorded.Sum(item => Evaluator.ProgressOf(item.Target, item.Records).Value ?? 0);
            var target = week.First().Target.BuildOrReduce == BuildOrReduce.Build
                ? week.Select(item => item.Entry.FirstDate)
                    .Concat(currentWeekUpcoming.Where(entry => MondayOf(entry.FirstDate) == monday).Select(entry => entry.FirstDate))
                    .Distinct()
                    .Count()
                : 0;
            bars.Add(new TrendBar(monday, monday.AddDays(6), value, target));
        }

        return bars;
    }

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
