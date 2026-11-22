using System.Diagnostics.CodeAnalysis;
using HT.Domain.Habits;
using HT.Domain.Time;

namespace HT.Domain.Goals;

/// <summary>
/// A goal's days, weeks, current date, calendar, next commitment, time-zone change, settings change and ending (FR-012
/// to FR-014, FR-026, FR-027; research R-06; slice 8 research R-02 to R-04). A pure function of the goal's attempts,
/// each with its ordered versions and its ending: no clock, no culture and no machine time zone; the instant is a
/// parameter. Every day starts at midnight in the time zone of its own version (<see cref="NamedTimeZone.StartOfDay"/>),
/// so a clock-change day lasts 23 or 25 hours, and at a version switch the old day's end and the new day's start may
/// differ (the gap or overlap, which is kept as it is).
/// <para>
/// Each attempt is timed as slice 4 times a goal, from its own timing date (attempt 1: the goal's start date; a later
/// attempt: its own start date) with its own versions, and cut at its last date once it is ended. Its entries run from
/// its first entry (its timing date's commitment or day, or for a per-week attempt the week of the first Monday on or
/// after it) to its last date. A date in no attempt's entries belongs to none. For a goal with one attempt, no ending
/// and no change, every member gives what it gave in slice 7.
/// </para>
/// </summary>
public static class GoalTimeline
{
    /// <summary>The longest calendar range, in days (FR-013).</summary>
    public const int MaxCalendarDays = 366;

    /// <summary>A day's candidates for the current date lie within this many days of its local date in a version's zone.</summary>
    private const int CandidateSpread = 2;

    /// <summary>The scan for the next commitment looks at most this many days ahead.</summary>
    private const int NextCommitmentScanDays = 15;

    /// <summary>The window of <see cref="EntriesThrough(Goal, DateOnly)"/>, in days: 52 whole weeks, within <see cref="MaxCalendarDays"/>.</summary>
    private const int ReportWindowDays = 364;

    /// <summary>
    /// The version in effect for <paramref name="date"/>: the version of the attempt that contains it (from its timing
    /// date to its last date), the one with the greatest <see cref="GoalSettingsVersion.AppliesFrom"/> on or before it.
    /// A date outside every attempt gets the latest attempt's version in effect then, its first before its start.
    /// </summary>
    public static GoalSettingsVersion VersionOf(Goal goal, DateOnly date)
    {
        ArgumentNullException.ThrowIfNull(goal);
        var lines = Lines(goal);
        return (Array.Find(lines, line => line.Spans(date)) ?? lines[^1]).VersionOf(date);
    }

    /// <summary>
    /// The goal's current date at <paramref name="instant"/> (FR-012; slice 8 "Current date"):
    /// <list type="bullet">
    ///   <item>while the latest attempt has begun and is not ended, its current date: the latest date, on or after its
    ///   timing date, whose day of the attempt has begun;</item>
    ///   <item>otherwise, once any attempt has begun (<see cref="LatestAttemptHasBegun"/>'s rule), the date of the
    ///   instant in the zone of the last version of the latest attempt that has begun;</item>
    ///   <item>otherwise null.</item>
    /// </list>
    /// An attempt's candidates are the local date of the instant in each of its versions' zones, ± 2 days; offsets
    /// differ by at most 26 hours, so the answer is among them, also where the days' starts are not monotonic (FR-027).
    /// </summary>
    public static DateOnly? CurrentDate(Goal goal, DateTimeOffset instant)
    {
        ArgumentNullException.ThrowIfNull(goal);
        var lines = Lines(goal);
        var latest = lines[^1];
        if (!latest.IsEnded && latest.OwnCurrentDate(instant) is { } current)
        {
            return current;
        }

        return Array.FindLast(lines, line => line.HasBegun(instant)) is { } begun
            ? begun.Versions[^1].Settings.TimeZone.DateAt(instant)
            : null;
    }

    /// <summary>
    /// The evaluation calendar from <paramref name="first"/> to <paramref name="last"/>, both included (FR-013). False
    /// when <paramref name="last"/> is before <paramref name="first"/> or the range has more than 366 days. Otherwise
    /// every attempt's entries in the range, each list in date order: one commitment per scheduled day of a
    /// commitment-based attempt in <see cref="EvaluationCalendar.Commitments"/>; one period per day of a per-day attempt,
    /// or one per Monday-to-Sunday week of a per-week attempt that overlaps the range, starting with its first week that
    /// begins on or after its timing date, in <see cref="EvaluationCalendar.Periods"/>. Nothing after an attempt's last
    /// date. <see cref="EvaluationCalendar.Kind"/> is the kind of the latest attempt with an entry in the range, else of
    /// the latest attempt whose timing date is on or before <paramref name="last"/>, else of attempt 1 (slice 8 R-02).
    /// </summary>
    public static bool TryCalendar(Goal goal, DateOnly first, DateOnly last, [NotNullWhen(true)] out EvaluationCalendar? calendar)
    {
        ArgumentNullException.ThrowIfNull(goal);
        calendar = null;
        if (last < first || last.DayNumber - first.DayNumber + 1 > MaxCalendarDays)
        {
            return false;
        }

        var lines = Lines(goal);
        var commitments = new List<Commitment>();
        var periods = new List<EvaluationPeriod>();
        AttemptLine? withEntry = null;
        foreach (var line in lines)
        {
            var count = commitments.Count + periods.Count;
            if (line.Kind == GoalScheduleKind.CommitmentBased)
            {
                commitments.AddRange(line.Commitments(first, last));
            }
            else
            {
                periods.AddRange(line.Periods(first, last));
            }

            if (commitments.Count + periods.Count > count)
            {
                withEntry = line;
            }
        }

        var kindLine = withEntry ?? Array.FindLast(lines, line => line.TimingDate <= last) ?? lines[0];
        calendar = new EvaluationCalendar(kindLine.Kind, commitments, periods);
        return true;
    }

    /// <summary>
    /// The commitment or evaluation period that <paramref name="date"/> belongs to (slice 5 FR-002, research R-03),
    /// from <see cref="TryCalendar"/>: the commitment of <paramref name="date"/>, its day, or the week that contains
    /// it. Null for an unscheduled day of a commitment-based attempt, a date before the start date, a date before a
    /// per-week attempt's first week, or a date in no attempt.
    /// </summary>
    public static CalendarEntry? EntryOf(Goal goal, DateOnly date) =>
        TryEntries(goal, date, date, out var entries)
            ? entries.FirstOrDefault(entry => entry.FirstDate <= date && date <= entry.LastDate)
            : null;

    /// <summary>
    /// Whether the day of <paramref name="date"/> has begun at <paramref name="instant"/> (slice 5 FR-002, FR-007; slice
    /// 8 "Current date"): the date belongs to an attempt whose own timeline has a current date on or after it, in that
    /// attempt's own zones. So a date of a waiting attempt has not begun until its own day begins, whatever the goal's
    /// current date. A date in no attempt has begun when the goal has a current date on or after it. For a per-week
    /// goal the date itself must have begun, not only its week.
    /// </summary>
    public static bool HasBegun(Goal goal, DateOnly date, DateTimeOffset instant)
    {
        ArgumentNullException.ThrowIfNull(goal);
        if (Array.Find(Lines(goal), candidate => candidate.Spans(date)) is { } line)
        {
            return line.OwnCurrentDate(instant) is { } own && date <= own;
        }

        return CurrentDate(goal, instant) is { } current && date <= current;
    }

    /// <summary>
    /// The commitments or evaluation periods of <see cref="TryCalendar"/> from <paramref name="first"/> to
    /// <paramref name="last"/> as entries, in date order (slice 5 research R-03), with the same range rule: false when
    /// <paramref name="last"/> is before <paramref name="first"/> or the range has more than 366 days. A range across
    /// attempts of both kinds merges both lists in date order.
    /// </summary>
    public static bool TryEntries(Goal goal, DateOnly first, DateOnly last, [NotNullWhen(true)] out IReadOnlyList<CalendarEntry>? entries)
    {
        entries = null;
        if (!TryCalendar(goal, first, last, out var calendar))
        {
            return false;
        }

        entries = calendar.Commitments.Select(c => new CalendarEntry(c.Date, c.Date, c.Starts, c.Ends))
            .Concat(calendar.Periods.Select(p => new CalendarEntry(p.FirstDate, p.LastDate, p.Starts, p.Ends)))
            .OrderBy(entry => entry.FirstDate)
            .ToArray();
        return true;
    }

    /// <summary>
    /// Every commitment or evaluation period of the goal's latest attempt (the report range, slice 8 FR-013) from its
    /// timing date (a per-week attempt: from its first week) to the entry that contains <paramref name="last"/>, or to
    /// its last date when that is earlier, in date order, with no range cap (slice 6 FR-001, research R-02). Read in
    /// windows of 364 days that each start on a Monday, from the Monday on or before the attempt's timing date, so a
    /// week never straddles two windows and nothing is listed twice or left out. Empty when <paramref name="last"/> is
    /// before the first entry.
    /// </summary>
    public static IReadOnlyList<CalendarEntry> EntriesThrough(Goal goal, DateOnly last)
    {
        ArgumentNullException.ThrowIfNull(goal);
        return EntriesThrough(Lines(goal)[^1], last);
    }

    /// <summary>
    /// The entries of attempt <paramref name="number"/> through <paramref name="last"/> or its last date, whichever is
    /// earlier, as <see cref="EntriesThrough(Goal, DateOnly)"/> reads them for the latest attempt.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The goal has no attempt with that number.</exception>
    public static IReadOnlyList<CalendarEntry> EntriesThrough(Goal goal, int number, DateOnly last)
    {
        ArgumentNullException.ThrowIfNull(goal);
        return EntriesThrough(LineOf(goal, number), last);
    }

    /// <summary>
    /// The date of the latest attempt's next commitment at <paramref name="instant"/> (FR-014): the first scheduled day
    /// on or after its timing date whose end, by its own instants, is after the instant. For an attempt that has not
    /// begun, its first scheduled day. Null for a period-based attempt and for an ended goal.
    /// </summary>
    public static DateOnly? NextCommitment(Goal goal, DateTimeOffset instant)
    {
        ArgumentNullException.ThrowIfNull(goal);
        var line = Lines(goal)[^1];
        if (line.IsEnded || line.Kind != GoalScheduleKind.CommitmentBased)
        {
            return null;
        }

        // Overlaps after a move east can make yesterday's end later than today's start, so the scan starts two days back.
        // Stryker disable once Equality : when current − 2 equals the timing date both branches give the same date, so >= gives the same
        var scanStart = line.OwnCurrentDate(instant) is { } current && current.AddDays(-CandidateSpread) > line.TimingDate
            ? current.AddDays(-CandidateSpread)
            : line.TimingDate;
        for (var i = 0; i < NextCommitmentScanDays; i++)
        {
            var date = scanStart.AddDays(i);
            if (line.IsScheduled(date) && line.DayEnd(date) > instant)
            {
                return date;
            }
        }

        return null;
    }

    /// <summary>
    /// The date the latest attempt's first week starts, for a per-week attempt: the first Monday on or after its timing
    /// date (FR-013). Null for other attempts and for an ended goal.
    /// </summary>
    public static DateOnly? FirstWeekStart(Goal goal)
    {
        ArgumentNullException.ThrowIfNull(goal);
        var line = Lines(goal)[^1];
        return line.IsEnded ? null : FirstWeekStart(line.Target, line.TimingDate);
    }

    /// <summary>
    /// The same date for a goal not created yet, from its target and start date: the first Monday on or after
    /// <paramref name="startDate"/> for a per-week target (the New goal summary card, HP-002). Null for other targets.
    /// </summary>
    public static DateOnly? FirstWeekStart(GoalTarget target, DateOnly startDate)
    {
        ArgumentNullException.ThrowIfNull(target);
        return target.PeriodLength == SchedulePeriod.Week ? MondayOnOrAfter(startDate) : null;
    }

    /// <summary>
    /// What a change of the owner's time zone to <paramref name="zone"/> at <paramref name="instant"/> does to the goal's
    /// latest attempt (FR-026; slice 8 FR-021), from that attempt's current date D, never from the account's date:
    /// <list type="bullet">
    ///   <item><see cref="GoalVersionChange.None"/>: the goal is ended, or <paramref name="zone"/> is the zone of every version from D on (or of every version, without D);</item>
    ///   <item><see cref="GoalVersionChange.ReplaceFirst"/>: the latest attempt has not begun, also when an earlier attempt gives the goal a current date; its first version gets the zone;</item>
    ///   <item><see cref="GoalVersionChange.ReplacePending"/>: a version after D exists; it gets the zone and the date E;</item>
    ///   <item><see cref="GoalVersionChange.Add"/>: a new version from E.</item>
    /// </list>
    /// E is D + 1 for commitment-based and per-day attempts, and the Monday after D's week for per-week attempts. So an
    /// attempt never has more than one version after D, never two on one date, and no day that has begun changes its
    /// version.
    /// </summary>
    public static GoalVersionChange ChangeOf(Goal goal, DateTimeOffset instant, NamedTimeZone zone)
    {
        ArgumentNullException.ThrowIfNull(goal);
        var line = Lines(goal)[^1];
        if (line.IsEnded)
        {
            return GoalVersionChange.None;
        }

        if (line.OwnCurrentDate(instant) is not { } current)
        {
            return line.Versions.All(version => version.Settings.TimeZone == zone)
                ? GoalVersionChange.None
                : GoalVersionChange.ReplaceFirst(zone);
        }

        // Stryker disable once Linq,Equality : All ignores the order, and a version from D itself is VersionOf(D), already in the list, so Append and >= give the same
        var fromCurrent = line.Versions.Where(version => version.AppliesFrom > current).Prepend(line.VersionOf(current));
        if (fromCurrent.All(version => version.Settings.TimeZone == zone))
        {
            return GoalVersionChange.None;
        }

        var appliesFrom = line.NextBoundary(current);
        return line.Versions[^1].AppliesFrom > current
            ? GoalVersionChange.ReplacePending(zone, appliesFrom)
            : GoalVersionChange.Add(appliesFrom, zone);
    }

    /// <summary>
    /// The last date of attempt <paramref name="number"/> (slice 8 FR-008): null while it is not ended; otherwise, from
    /// its ending date E, E − 1, or for a per-week attempt the Sunday before E's week. A last date before the attempt's
    /// first entry means it has no date: no commitment or period (plan "Spec discrepancies" 3).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The goal has no attempt with that number.</exception>
    public static DateOnly? LastDate(Goal goal, int number)
    {
        ArgumentNullException.ThrowIfNull(goal);
        return LineOf(goal, number).Last;
    }

    /// <summary>The number of the attempt whose entries contain <paramref name="date"/>, or null when it is in none (slice 8 FR-010).</summary>
    public static int? AttemptOf(Goal goal, DateOnly date)
    {
        ArgumentNullException.ThrowIfNull(goal);
        return Array.Find(Lines(goal), line => line.Spans(date) && line.Contains(date))?.Number;
    }

    /// <summary>
    /// The settings in effect of attempt <paramref name="number"/> at <paramref name="instant"/> (slice 8 FR-005,
    /// research R-02): for an ended attempt its last version (the version of its last date, or its first when it has no
    /// date); while it has not begun, its first version; otherwise its version at its current date.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The goal has no attempt with that number.</exception>
    public static GoalSettingsVersion VersionInEffect(Goal goal, int number, DateTimeOffset instant)
    {
        ArgumentNullException.ThrowIfNull(goal);
        return LineOf(goal, number).VersionInEffect(instant);
    }

    /// <summary>
    /// The settings in effect of the latest attempt at <paramref name="instant"/> (slice 8 FR-005), as
    /// <see cref="VersionInEffect(Goal, int, DateTimeOffset)"/> gives them. For a goal with one attempt and no ending it
    /// equals <c>VersionOf(goal, CurrentDate(goal, instant) ?? goal.StartDate)</c>.
    /// </summary>
    public static GoalSettingsVersion VersionInEffect(Goal goal, DateTimeOffset instant)
    {
        ArgumentNullException.ThrowIfNull(goal);
        return Lines(goal)[^1].VersionInEffect(instant);
    }

    /// <summary>
    /// Whether the latest attempt has begun at <paramref name="instant"/> (slice 8 research R-02, plan "Spec
    /// discrepancies" 5). An attempt that is not ended has begun once the day of its timing date has begun in the zone of
    /// its first version: it has a current date of its own. An ended attempt has begun when it has a date (a commitment
    /// or period up to its last date), so one ended before its first entry never begins. <see cref="CurrentDate"/> uses
    /// the same rule for "the latest attempt that has begun".
    /// </summary>
    public static bool LatestAttemptHasBegun(Goal goal, DateTimeOffset instant)
    {
        ArgumentNullException.ThrowIfNull(goal);
        return Lines(goal)[^1].HasBegun(instant);
    }

    /// <summary>
    /// What a settings change to <paramref name="target"/> at <paramref name="instant"/> does to the latest attempt's
    /// versions (slice 8 FR-002, research R-03). The written version takes <paramref name="zone"/>, the account's current
    /// time zone:
    /// <list type="bullet">
    ///   <item>the attempt has not begun (also when an earlier attempt gives the goal a current date): its first version
    ///   is replaced, keeping its date, which is <see cref="GoalSettingsChange.AppliesFrom"/>;</item>
    ///   <item>it has begun, at its current date D: from the next boundary B (D + 1, or the Monday after D's week for a
    ///   per-week attempt), the one version after D is replaced, or a version is added.</item>
    /// </list>
    /// So no begun date changes its version, and the attempt never has more than one version after D.
    /// </summary>
    /// <exception cref="InvalidOperationException">The goal is ended.</exception>
    /// <exception cref="ArgumentException"><paramref name="target"/> has another schedule kind, or <paramref name="zone"/> is not a named zone.</exception>
    public static GoalSettingsChange SettingsChangeOf(Goal goal, DateTimeOffset instant, GoalTarget target, NamedTimeZone zone)
    {
        ArgumentNullException.ThrowIfNull(goal);
        ArgumentNullException.ThrowIfNull(target);
        var line = Lines(goal)[^1];
        if (line.IsEnded)
        {
            throw new InvalidOperationException("The settings of an ended goal do not change.");
        }

        if (!line.Target.HasSameScheduleKind(target))
        {
            throw new ArgumentException("The target has another schedule kind than the attempt.", nameof(target));
        }

        var settings = new GoalSettings(target, zone);
        var versions = line.Versions.ToList();
        if (line.OwnCurrentDate(instant) is not { } current)
        {
            versions[0] = new GoalSettingsVersion(settings, versions[0].AppliesFrom);
            return new GoalSettingsChange(versions, versions[0].AppliesFrom);
        }

        var appliesFrom = line.NextBoundary(current);
        var changed = new GoalSettingsVersion(settings, appliesFrom);
        if (versions[^1].AppliesFrom > current)
        {
            versions[^1] = changed;
        }
        else
        {
            versions.Add(changed);
        }

        return new GoalSettingsChange(versions, appliesFrom);
    }

    /// <summary>
    /// What ending the latest attempt on <paramref name="endingDate"/> does (slice 8 FR-008, FR-009, research R-04): its
    /// last date, and the versions it keeps: those that apply on or before the last date, and always the first.
    /// </summary>
    /// <exception cref="InvalidOperationException">The goal is already ended.</exception>
    public static GoalEndingEffect EndingOf(Goal goal, DateOnly endingDate)
    {
        ArgumentNullException.ThrowIfNull(goal);
        var line = Lines(goal)[^1];
        if (line.IsEnded)
        {
            throw new InvalidOperationException("The goal is already ended.");
        }

        var last = LastDateOf(line.Target, endingDate);
        var kept = line.Versions.Where((version, index) => index == 0 || version.AppliesFrom <= last).ToArray();
        return new GoalEndingEffect(last, kept);
    }

    /// <summary>
    /// The earliest start date of a new attempt (slice 8 FR-011): the later of <paramref name="accountToday"/> and the
    /// day after the latest attempt's ending date.
    /// </summary>
    /// <exception cref="InvalidOperationException">The goal is not ended.</exception>
    public static DateOnly NewAttemptEarliestStart(Goal goal, DateOnly accountToday)
    {
        ArgumentNullException.ThrowIfNull(goal);
        var ending = goal.Attempt.Ending ?? throw new InvalidOperationException("The goal is not ended.");
        var after = ending.Date.AddDays(1);
        // Stryker disable once Equality : when both dates are equal either branch gives that date, so >= gives the same
        return accountToday > after ? accountToday : after;
    }

    /// <summary>
    /// The last date of an attempt with <paramref name="target"/>'s schedule kind ended on <paramref name="endingDate"/>
    /// (slice 8 FR-008): E − 1, or for a per-week target the Sunday before E's week. The one rule, which
    /// <see cref="LastDate"/> and the <see cref="Goal"/> constructor share.
    /// </summary>
    internal static DateOnly LastDateOf(GoalTarget target, DateOnly endingDate) =>
        target.PeriodLength == SchedulePeriod.Week ? MondayOnOrBefore(endingDate).AddDays(-1) : endingDate.AddDays(-1);

    /// <summary>
    /// The last version of attempt <paramref name="number"/> (slice 8 "Last version"): for an ended attempt the version of
    /// its last date, or its first when it has no date; for an attempt that is not ended its latest version.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The goal has no attempt with that number.</exception>
    internal static GoalSettingsVersion LastVersionOf(Goal goal, int number)
    {
        var line = LineOf(goal, number);
        return line.IsEnded ? line.EndedVersion : line.Versions[^1];
    }

    /// <summary>One view per attempt: attempt 1 timed from the goal's start date, a later attempt from its own start date.</summary>
    private static AttemptLine[] Lines(Goal goal)
    {
        var attempts = goal.Attempts;
        var lines = new AttemptLine[attempts.Count];
        for (var i = 0; i < lines.Length; i++)
        {
            lines[i] = new AttemptLine(attempts[i], i == 0 ? goal.StartDate : attempts[i].Attempt.StartDate);
        }

        return lines;
    }

    private static AttemptLine LineOf(Goal goal, int number)
    {
        var attempt = goal.AttemptAt(number);
        return new AttemptLine(attempt, number == GoalAttempt.First ? goal.StartDate : attempt.Attempt.StartDate);
    }

    private static List<CalendarEntry> EntriesThrough(AttemptLine line, DateOnly last)
    {
        // Stryker disable once Equality : when both dates are equal either branch gives that date, so <= gives the same (> is killed by GoalTimelineEdgeTests)
        var end = line.Last is { } lastDate && lastDate < last ? lastDate : last;
        var entries = new List<CalendarEntry>();
        var first = MondayOnOrBefore(line.TimingDate);
        while (first <= end)
        {
            // Stryker disable once Equality : at exactly 364 days the two reads split at a Monday and list the same entries as one read, so <= gives the same
            var lastWindow = end.DayNumber - first.DayNumber < ReportWindowDays;
            var windowLast = lastWindow ? end : first.AddDays(ReportWindowDays - 1);
            entries.AddRange(line.Entries(first, windowLast));
            if (lastWindow)
            // Stryker disable once Block,Statement : the next window would start after the end, so the loop stops without the break as well
            {
                break;
            }

            first = first.AddDays(ReportWindowDays);
        }

        return entries;
    }

    private static DateOnly Later(DateOnly a, DateOnly b) => a > b ? a : b;

    // Stryker disable once Equality : when both dates are equal either branch gives that date, so <= gives the same
    private static DateOnly Earlier(DateOnly a, DateOnly? b) => b is { } other && other < a ? other : a;

    /// <summary>Days since Monday: 0 for Monday, 6 for Sunday.</summary>
    private static int DaysSinceMonday(DateOnly date) => ((int)date.DayOfWeek + 6) % 7;

    private static DateOnly MondayOnOrBefore(DateOnly date) => date.AddDays(-DaysSinceMonday(date));

    private static DateOnly MondayOnOrAfter(DateOnly date) => date.AddDays((7 - DaysSinceMonday(date)) % 7);

    private static DateOnly MondayAfterWeekOf(DateOnly date) => date.AddDays(7 - DaysSinceMonday(date));

    /// <summary>
    /// One attempt timed as slice 4 times a goal (slice 8 research R-02): from its timing date, with its own versions,
    /// cut at its last date once it is ended. The internal technique that keeps every attempt rule inside the timeline.
    /// </summary>
    private sealed class AttemptLine
    {
        public AttemptLine(GoalAttemptVersions attempt, DateOnly timingDate)
        {
            Number = attempt.Attempt.Number;
            Versions = attempt.Versions;
            TimingDate = timingDate;
            Target = Versions[0].Settings.Target;
            Last = attempt.Attempt.Ending is { } ending ? LastDateOf(Target, ending.Date) : null;
        }

        public int Number { get; }

        public IReadOnlyList<GoalSettingsVersion> Versions { get; }

        /// <summary>The date the attempt is timed from: the goal's start date for attempt 1, its own start date otherwise.</summary>
        public DateOnly TimingDate { get; }

        /// <summary>The first version's target; every version of the attempt has its schedule kind.</summary>
        public GoalTarget Target { get; }

        /// <summary>The attempt's last date; null while it is not ended.</summary>
        public DateOnly? Last { get; }

        public bool IsEnded => Last is not null;

        public GoalScheduleKind Kind => Target.ScheduleKindOf;

        private bool PerWeek => Target.PeriodLength == SchedulePeriod.Week;

        /// <summary>
        /// The attempt has no date: it is ended, and its last date is before its first entry, so it has no commitment or
        /// period (plan "Spec discrepancies" 3). The one rule behind <see cref="HasBegun"/> and <see cref="VersionInEffect"/>.
        /// </summary>
        public bool HasNoDate => Last is { } last && !HasEntryThrough(last);

        /// <summary>Whether <paramref name="date"/> lies from the timing date to the last date (no end while not ended).</summary>
        public bool Spans(DateOnly date) => date >= TimingDate && (Last is not { } last || date <= last);

        /// <summary>Whether one of the attempt's entries contains <paramref name="date"/>.</summary>
        // Stryker disable once Logical : every entry of the one-day range contains that day, so || gives the same
        public bool Contains(DateOnly date) => Entries(date, date).Any(entry => entry.FirstDate <= date && date <= entry.LastDate);

        /// <summary>A not-ended attempt has begun when it has a current date of its own; an ended one when it has a date.</summary>
        public bool HasBegun(DateTimeOffset instant) => IsEnded ? !HasNoDate : OwnCurrentDate(instant) is not null;

        public GoalSettingsVersion VersionOf(DateOnly date)
        {
            for (var i = Versions.Count - 1;
                 // Stryker disable once Equality : at i = 0 the loop returns Versions[0], which the method returns after the loop anyway, so >= gives the same
                 i > 0;
                 i--)
            {
                if (Versions[i].AppliesFrom <= date)
                {
                    return Versions[i];
                }
            }

            return Versions[0];
        }

        /// <summary>An ended attempt's last version: the version of its last date, or its first when it has no date.</summary>
        public GoalSettingsVersion EndedVersion => Last is { } last && !HasNoDate ? VersionOf(last) : Versions[0];

        public GoalSettingsVersion VersionInEffect(DateTimeOffset instant)
        {
            if (IsEnded)
            {
                return EndedVersion;
            }

            return OwnCurrentDate(instant) is { } current ? VersionOf(current) : Versions[0];
        }

        /// <summary>
        /// Slice 4's current date of this attempt alone, not cut at its last date: the latest date, on or after the
        /// timing date, whose day has begun at <paramref name="instant"/>; null when the timing date's day has not begun.
        /// </summary>
        public DateOnly? OwnCurrentDate(DateTimeOffset instant)
        {
            DateOnly? current = null;
            foreach (var zone in Versions.Select(version => version.Settings.TimeZone).Distinct())
            {
                var local = zone.DateAt(instant);
                for (var offset = -CandidateSpread;
                     // Stryker disable once Equality : a date after a zone's local date has begun only in a zone whose local date is that late, which offers it at an offset of 0 or less, so < gives the same
                     offset <= CandidateSpread;
                     offset++)
                {
                    var candidate = local.AddDays(offset);
                    if (candidate >= TimingDate
                        && (current is null ||
                            // Stryker disable once Equality : a candidate equal to the current date sets it to the same date, so >= gives the same
                            candidate > current)
                        && DayStart(candidate) <= instant)
                    {
                        current = candidate;
                    }
                }
            }

            return current;
        }

        /// <summary>The next boundary after <paramref name="current"/>: the next day, or the Monday after its week for a per-week attempt.</summary>
        public DateOnly NextBoundary(DateOnly current) => PerWeek ? MondayAfterWeekOf(current) : current.AddDays(1);

        public bool IsScheduled(DateOnly date) => VersionOf(date).Settings.Target.Schedule.Days.Contains(date.DayOfWeek);

        /// <summary>The instant day <paramref name="date"/> starts: midnight in the zone of its own version.</summary>
        public DateTimeOffset DayStart(DateOnly date) => VersionOf(date).Settings.TimeZone.StartOfDay(date);

        /// <summary>The instant day <paramref name="date"/> ends: the next midnight in the zone of the day's own version.</summary>
        public DateTimeOffset DayEnd(DateOnly date) => VersionOf(date).Settings.TimeZone.StartOfDay(date.AddDays(1));

        public CalendarEntry[] Entries(DateOnly first, DateOnly last) =>
            Kind == GoalScheduleKind.CommitmentBased
                ? Commitments(first, last).Select(c => new CalendarEntry(c.Date, c.Date, c.Starts, c.Ends)).ToArray()
                : Periods(first, last).Select(p => new CalendarEntry(p.FirstDate, p.LastDate, p.Starts, p.Ends)).ToArray();

        public List<Commitment> Commitments(DateOnly first, DateOnly last)
        {
            var commitments = new List<Commitment>();
            for (var date = Later(first, TimingDate); date <= Earlier(last, Last); date = date.AddDays(1))
            {
                if (IsScheduled(date))
                {
                    commitments.Add(new Commitment(date, DayStart(date), DayEnd(date)));
                }
            }

            return commitments;
        }

        public List<EvaluationPeriod> Periods(DateOnly first, DateOnly last) => PerWeek ? Weeks(first, last) : Days(first, last);

        private List<EvaluationPeriod> Days(DateOnly first, DateOnly last)
        {
            var periods = new List<EvaluationPeriod>();
            for (var date = Later(first, TimingDate); date <= Earlier(last, Last); date = date.AddDays(1))
            {
                periods.Add(new EvaluationPeriod(date, date, DayStart(date), DayEnd(date)));
            }

            return periods;
        }

        /// <summary>
        /// Per-week versions start only on Mondays (FR-026), so a whole week has the version of its Monday. Every week
        /// that overlaps the range, from the first week on or after the timing date; an ended attempt's last date is a
        /// Sunday, so its weeks end on or before it.
        /// </summary>
        private List<EvaluationPeriod> Weeks(DateOnly first, DateOnly last)
        {
            var periods = new List<EvaluationPeriod>();
            var end = Earlier(last, Last);
            for (var monday = Later(MondayOnOrBefore(first), MondayOnOrAfter(TimingDate)); monday <= end; monday = monday.AddDays(7))
            {
                var zone = VersionOf(monday).Settings.TimeZone;
                periods.Add(new EvaluationPeriod(monday, monday.AddDays(6), zone.StartOfDay(monday), zone.StartOfDay(monday.AddDays(7))));
            }

            return periods;
        }

        /// <summary>Whether the attempt has a commitment or period that ends on or before <paramref name="last"/>.</summary>
        private bool HasEntryThrough(DateOnly last)
        {
            if (Kind == GoalScheduleKind.PeriodBased)
            {
                return PerWeek ? MondayOnOrAfter(TimingDate).AddDays(6) <= last : TimingDate <= last;
            }

            for (var date = TimingDate; date <= last; date = date.AddDays(1))
            {
                if (IsScheduled(date))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
