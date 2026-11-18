using System.Diagnostics.CodeAnalysis;
using HT.Domain.Habits;
using HT.Domain.Time;

namespace HT.Domain.Goals;

/// <summary>
/// A goal's days, weeks, current date, calendar, next commitment and time-zone change (FR-012 to FR-014, FR-026,
/// FR-027; research R-06). A pure function of the goal's start date and its ordered versions: no clock, no culture
/// and no machine time zone; the instant is a parameter. Every day starts at midnight in the time zone of its own
/// version (<see cref="NamedTimeZone.StartOfDay"/>), so a clock-change day lasts 23 or 25 hours, and at a version
/// switch the old day's end and the new day's start may differ (the gap or overlap, which is kept as it is).
/// </summary>
public static class GoalTimeline
{
    /// <summary>The longest calendar range, in days (FR-013).</summary>
    public const int MaxCalendarDays = 366;

    /// <summary>A day's candidates for the current date lie within this many days of its local date in a version's zone.</summary>
    private const int CandidateSpread = 2;

    /// <summary>The scan for the next commitment looks at most this many days ahead.</summary>
    private const int NextCommitmentScanDays = 15;

    /// <summary>
    /// The version in effect for <paramref name="date"/>: the one with the greatest <see cref="GoalSettingsVersion.AppliesFrom"/>
    /// on or before it. A date before the start date gets the first version.
    /// </summary>
    public static GoalSettingsVersion VersionOf(Goal goal, DateOnly date)
    {
        ArgumentNullException.ThrowIfNull(goal);
        var versions = goal.Versions;
        for (var i = versions.Count - 1; i > 0; i--)
        {
            if (versions[i].AppliesFrom <= date)
            {
                return versions[i];
            }
        }

        return versions[0];
    }

    /// <summary>
    /// The goal's current date at <paramref name="instant"/> (FR-012): the latest date, on or after the start date,
    /// whose day of the goal has begun. Null when the start date's day has not begun. The candidates are the local
    /// date of the instant in each version's zone, ± 2 days; offsets differ by at most 26 hours, so the answer is
    /// among them, also where the days' starts are not monotonic (FR-027).
    /// </summary>
    public static DateOnly? CurrentDate(Goal goal, DateTimeOffset instant)
    {
        ArgumentNullException.ThrowIfNull(goal);
        DateOnly? current = null;
        foreach (var zone in goal.Versions.Select(version => version.Settings.TimeZone).Distinct())
        {
            var local = zone.DateAt(instant);
            for (var offset = -CandidateSpread; offset <= CandidateSpread; offset++)
            {
                var candidate = local.AddDays(offset);
                if (candidate >= goal.StartDate && (current is null || candidate > current) && DayStart(goal, candidate) <= instant)
                {
                    current = candidate;
                }
            }
        }

        return current;
    }

    /// <summary>
    /// The evaluation calendar from <paramref name="first"/> to <paramref name="last"/>, both included (FR-013). False
    /// when <paramref name="last"/> is before <paramref name="first"/> or the range has more than 366 days. Otherwise,
    /// in date order: one commitment per scheduled day on or after the start date (commitment-based); one period per
    /// day on or after the start date (per day); or one period per Monday-to-Sunday week that overlaps the range,
    /// starting with the first week that begins on or after the start date (per week).
    /// </summary>
    public static bool TryCalendar(Goal goal, DateOnly first, DateOnly last, [NotNullWhen(true)] out EvaluationCalendar? calendar)
    {
        ArgumentNullException.ThrowIfNull(goal);
        calendar = null;
        if (last < first || last.DayNumber - first.DayNumber + 1 > MaxCalendarDays)
        {
            return false;
        }

        var target = goal.Target;
        if (target.ScheduleKindOf == GoalScheduleKind.CommitmentBased)
        {
            calendar = new EvaluationCalendar(GoalScheduleKind.CommitmentBased, Commitments(goal, first, last), []);
        }
        else
        {
            var periods = target.PeriodLength == SchedulePeriod.Week ? Weeks(goal, first, last) : Days(goal, first, last);
            calendar = new EvaluationCalendar(GoalScheduleKind.PeriodBased, [], periods);
        }

        return true;
    }

    /// <summary>
    /// The date of a commitment-based goal's next commitment at <paramref name="instant"/> (FR-014): the first
    /// scheduled day on or after the start date whose end, by its own instants, is after the instant. For a goal
    /// that has not started, its first scheduled day. Null for a period-based goal.
    /// </summary>
    public static DateOnly? NextCommitment(Goal goal, DateTimeOffset instant)
    {
        ArgumentNullException.ThrowIfNull(goal);
        var target = goal.Target;
        if (target.ScheduleKindOf != GoalScheduleKind.CommitmentBased)
        {
            return null;
        }

        // Overlaps after a move east can make yesterday's end later than today's start, so the scan starts two days back.
        var scanStart = CurrentDate(goal, instant) is { } current && current.AddDays(-CandidateSpread) > goal.StartDate
            ? current.AddDays(-CandidateSpread)
            : goal.StartDate;
        for (var i = 0; i < NextCommitmentScanDays; i++)
        {
            var date = scanStart.AddDays(i);
            if (VersionOf(goal, date).Settings.Target.Schedule.Days.Contains(date.DayOfWeek) && DayEnd(goal, date) > instant)
            {
                return date;
            }
        }

        return null;
    }

    /// <summary>The date a per-week goal's first week starts: the first Monday on or after the start date (FR-013). Null for other goals.</summary>
    public static DateOnly? FirstWeekStart(Goal goal)
    {
        ArgumentNullException.ThrowIfNull(goal);
        return goal.Target.PeriodLength == SchedulePeriod.Week ? MondayOnOrAfter(goal.StartDate) : null;
    }

    /// <summary>
    /// What a change of the owner's time zone to <paramref name="zone"/> at <paramref name="instant"/> does to the goal
    /// (FR-026), from the goal's current date D, never from the account's date:
    /// <list type="bullet">
    ///   <item><see cref="GoalVersionChange.None"/>: <paramref name="zone"/> is the zone of every version from D on (or of the only version, without D);</item>
    ///   <item><see cref="GoalVersionChange.ReplaceFirst"/>: no current date; the single version gets the zone;</item>
    ///   <item><see cref="GoalVersionChange.ReplacePending"/>: a version after D exists; it gets the zone and the date E;</item>
    ///   <item><see cref="GoalVersionChange.Add"/>: a new version from E.</item>
    /// </list>
    /// E is D + 1 for commitment-based and per-day goals, and the Monday after D's week for per-week goals. So a goal
    /// never has more than one version after D, never two on one date, and no day that has begun changes its version.
    /// </summary>
    public static GoalVersionChange ChangeOf(Goal goal, DateTimeOffset instant, NamedTimeZone zone)
    {
        ArgumentNullException.ThrowIfNull(goal);
        if (CurrentDate(goal, instant) is not { } current)
        {
            return goal.Versions.All(version => version.Settings.TimeZone == zone)
                ? GoalVersionChange.None
                : GoalVersionChange.ReplaceFirst(zone);
        }

        var fromCurrent = goal.Versions.Where(version => version.AppliesFrom > current).Prepend(VersionOf(goal, current));
        if (fromCurrent.All(version => version.Settings.TimeZone == zone))
        {
            return GoalVersionChange.None;
        }

        var appliesFrom = goal.Target.PeriodLength == SchedulePeriod.Week ? MondayAfterWeekOf(current) : current.AddDays(1);
        return goal.Versions[^1].AppliesFrom > current
            ? GoalVersionChange.ReplacePending(zone, appliesFrom)
            : GoalVersionChange.Add(appliesFrom, zone);
    }

    /// <summary>The instant day <paramref name="date"/> of the goal starts: midnight in the zone of its own version.</summary>
    private static DateTimeOffset DayStart(Goal goal, DateOnly date) => ZoneOf(goal, date).StartOfDay(date);

    /// <summary>The instant day <paramref name="date"/> ends: the next midnight in the zone of the day's own version.</summary>
    private static DateTimeOffset DayEnd(Goal goal, DateOnly date) => ZoneOf(goal, date).StartOfDay(date.AddDays(1));

    private static NamedTimeZone ZoneOf(Goal goal, DateOnly date) => VersionOf(goal, date).Settings.TimeZone;

    private static List<Commitment> Commitments(Goal goal, DateOnly first, DateOnly last)
    {
        var commitments = new List<Commitment>();
        for (var date = Later(first, goal.StartDate); date <= last; date = date.AddDays(1))
        {
            if (VersionOf(goal, date).Settings.Target.Schedule.Days.Contains(date.DayOfWeek))
            {
                commitments.Add(new Commitment(date, DayStart(goal, date), DayEnd(goal, date)));
            }
        }

        return commitments;
    }

    private static List<EvaluationPeriod> Days(Goal goal, DateOnly first, DateOnly last)
    {
        var periods = new List<EvaluationPeriod>();
        for (var date = Later(first, goal.StartDate); date <= last; date = date.AddDays(1))
        {
            periods.Add(new EvaluationPeriod(date, date, DayStart(goal, date), DayEnd(goal, date)));
        }

        return periods;
    }

    /// <summary>Per-week versions start only on Mondays (FR-026), so a whole week has the version of its Monday.</summary>
    private static List<EvaluationPeriod> Weeks(Goal goal, DateOnly first, DateOnly last)
    {
        var periods = new List<EvaluationPeriod>();
        for (var monday = Later(MondayOnOrBefore(first), MondayOnOrAfter(goal.StartDate)); monday <= last; monday = monday.AddDays(7))
        {
            var zone = ZoneOf(goal, monday);
            periods.Add(new EvaluationPeriod(monday, monday.AddDays(6), zone.StartOfDay(monday), zone.StartOfDay(monday.AddDays(7))));
        }

        return periods;
    }

    private static DateOnly Later(DateOnly a, DateOnly b) => a > b ? a : b;

    /// <summary>Days since Monday: 0 for Monday, 6 for Sunday.</summary>
    private static int DaysSinceMonday(DateOnly date) => ((int)date.DayOfWeek + 6) % 7;

    private static DateOnly MondayOnOrBefore(DateOnly date) => date.AddDays(-DaysSinceMonday(date));

    private static DateOnly MondayOnOrAfter(DateOnly date) => date.AddDays((7 - DaysSinceMonday(date)) % 7);

    private static DateOnly MondayAfterWeekOf(DateOnly date) => date.AddDays(7 - DaysSinceMonday(date));
}
