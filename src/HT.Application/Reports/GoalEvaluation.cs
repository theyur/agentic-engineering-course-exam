using HT.Application.Goals;
using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Evaluation;
using HT.Domain.Exemptions;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;
using HT.Domain.Reports;

namespace HT.Application.Reports;

/// <summary>
/// One goal evaluated once at one instant (research R-04, data-model.md "Internal: GoalEvaluation"; HP-009, ADR 0020):
/// every commitment or period from the start date through the goal's current date, each evaluated through
/// <see cref="EntryViews.Of"/> with its own settings version's target (FR-009), from one owner-scoped store read.
/// Every figure of a report call derives from it: consistency, streaks, recent results, trend, the Today entries and
/// the notes. Nothing is stored.
/// <para>
/// Slice 8 (research R-07): one evaluation covers one attempt. The figures, results, trend and notes are that
/// attempt's, and the target it is built with is that attempt's settings in effect
/// (<see cref="GoalTimeline.VersionInEffect(Goal, int, DateTimeOffset)"/>). The evaluation of the latest attempt (the
/// report range, FR-013) also gives <see cref="EarlierPending"/>, so what can be recorded now covers the open grace
/// periods of earlier attempts.
/// </para>
/// </summary>
internal sealed class GoalEvaluation
{
    /// <summary>How many days after the window's first date an Upcoming entry may start (research R-06).</summary>
    private const int UpcomingWindowDays = 6;

    private static readonly StoredEntries NoEntries = new([], new Dictionary<DateOnly, NoteText>());

    private readonly StoredGoal _stored;

    private GoalEvaluation(
        StoredGoal stored,
        int attemptNumber,
        string name,
        GoalSettingsVersion version,
        DateOnly? currentDate,
        IReadOnlyList<HistoryEntry> entries,
        IReadOnlyList<HistoryEntry> earlierPending,
        Trend trend,
        GoalExemptions exemptions,
        DateTimeOffset now)
    {
        var latest = version.Settings.Target;
        _stored = stored;
        Goal = stored.Goal;
        AttemptNumber = attemptNumber;
        IsLatestAttempt = attemptNumber == Goal.Attempt.Number;
        Name = name;
        LatestTarget = latest;
        CurrentDate = currentDate;
        Entries = entries;
        EarlierPending = earlierPending;
        Trend = trend;
        Exemptions = exemptions;
        Now = now;
        BuildOrReduce = latest.BuildOrReduce;
        Unit = StreakUnits.Of(latest);
        Consistency = Consistency.Of(entries.Select(entry => entry.State));
        Streaks = Streaks.Of(entries.Select(entry => entry.State));
        Ended = entries.Where(entry => IsEnded(entry.State) || (IsExempt(entry.State) && entry.Ends <= now)).ToList();
        Due = entries.Where(entry => entry.State == EvaluationState.Due).ToList();
        Pending = entries.Where(entry => entry.State == EvaluationState.Pending).ToList();
        Notes = entries.Where(entry => entry.Note is not null).ToList();
        TrendTarget = trend.Bars.Count == 0 ? latest : GoalTimeline.VersionOf(Goal, trend.Bars[^1].FirstDate).Settings.Target;
        RecordableEntries = Due.Concat(Pending).Concat(earlierPending).OrderByDescending(entry => entry.FirstDate).ToList();
        Recordable = EntryViews.HistoryOf(Goal, version, currentDate, RecordableEntries, now);
    }

    public Goal Goal { get; }

    /// <summary>Slice 8: the number of the evaluated attempt.</summary>
    public int AttemptNumber { get; }

    /// <summary>Slice 8: whether the evaluated attempt is the goal's latest attempt.</summary>
    public bool IsLatestAttempt { get; }

    /// <summary>
    /// The goal's own or derived name (slice 4 FR-010): for the latest attempt the name at <see cref="Now"/>
    /// (<see cref="GoalText.Name(Goal, string, DateTimeOffset)"/>), for an earlier one <see cref="GoalText.AttemptName"/>
    /// (slice 8 FR-004, research R-08).
    /// </summary>
    public string Name { get; }

    public BuildOrReduce BuildOrReduce { get; }

    /// <summary>The unit of the goal's streaks (FR-004), from <see cref="LatestTarget"/>.</summary>
    public StreakUnit Unit { get; }

    /// <summary>
    /// The target of the evaluated attempt's settings in effect at <see cref="Now"/> (slice 8 research R-07): an ended
    /// attempt's last version, a waiting attempt's first version, otherwise the version of the current date.
    /// </summary>
    public GoalTarget LatestTarget { get; }

    /// <summary>The goal's current date at <see cref="Now"/> (slice 8 "Current date"); null before any attempt has begun.</summary>
    public DateOnly? CurrentDate { get; }

    /// <summary>The instant every entry was evaluated at.</summary>
    public DateTimeOffset Now { get; }

    /// <summary>
    /// Every commitment or period of the evaluated attempt through <see cref="CurrentDate"/>, and through its last date
    /// once it is ended, in date order.
    /// </summary>
    public IReadOnlyList<HistoryEntry> Entries { get; }

    public Consistency Consistency { get; }

    public Streaks Streaks { get; }

    /// <summary>
    /// The Pending and final (Met, Failed, Missed) entries, and the exempt results (slice 7: Excused or Paused entries
    /// whose end is at or before <see cref="Now"/>, FR-015), in date order.
    /// </summary>
    public IReadOnlyList<HistoryEntry> Ended { get; }

    /// <summary>The goal's excusals and pauses, read with its records (slice 7 research R-06).</summary>
    public GoalExemptions Exemptions { get; }

    /// <summary>The Due entries, in date order.</summary>
    public IReadOnlyList<HistoryEntry> Due { get; }

    /// <summary>The Pending entries of the evaluated attempt, in date order.</summary>
    public IReadOnlyList<HistoryEntry> Pending { get; }

    /// <summary>
    /// Slice 8 (research R-07, FR-016, FR-019): the Pending entries of the goal's ended attempts other than the latest
    /// whose grace period is open at <see cref="Now"/>, in date order, each with its attempt's number, name and
    /// settings. Only attempts whose last entry ended within the last 48 hours are evaluated for it. Empty for an
    /// evaluation of a named attempt.
    /// </summary>
    public IReadOnlyList<HistoryEntry> EarlierPending { get; }

    /// <summary>
    /// Slice 8 (research R-07): the entries that can be recorded now, newest first: the evaluated attempt's Due and
    /// Pending entries and <see cref="EarlierPending"/>, slice 6's order.
    /// </summary>
    public IReadOnlyList<HistoryEntry> RecordableEntries { get; }

    /// <summary>Slice 8: whether a Pending entry can be recorded now, of the evaluated attempt or of <see cref="EarlierPending"/>.</summary>
    public bool HasPending => Pending.Count > 0 || EarlierPending.Count > 0;

    /// <summary>The entries with a note, in date order, each with its state.</summary>
    public IReadOnlyList<HistoryEntry> Notes { get; }

    /// <summary>The trend of the begun entries (FR-006), from the <c>Of</c> overload of <see cref="TrendBuilder"/> that takes the goal's exemptions.</summary>
    public Trend Trend { get; }

    /// <summary>The target of the latest bar's settings version, which decides the trend's texts (research R-05, R-13).</summary>
    public GoalTarget TrendTarget { get; }

    /// <summary>
    /// What can be recorded now, as <see cref="RecordingService.GetRecordableAsync"/> gives it at the same instant
    /// (research R-07): the evaluated attempt's settings in effect and <see cref="RecordableEntries"/>.
    /// </summary>
    public GoalHistory Recordable { get; }

    /// <summary>
    /// Evaluates the latest attempt of <paramref name="stored"/> at <paramref name="now"/> (the report range, slice 8
    /// FR-013): its entries through its current date from
    /// <see cref="GoalTimeline.EntriesThrough(Goal, int, DateOnly)"/>, their records and notes from one
    /// <see cref="IRecordStore.ReadAsync"/> for <paramref name="owner"/> over the first entry's first date to the last
    /// entry's last date, and, for a commitment-based yes/no goal, the Upcoming commitments of the current week for its
    /// trend's target. A goal with no current date, or no entry through it, reads no record.
    /// <para>
    /// Slice 7 (research R-06, R-07): the goal's excusals and pauses come from one
    /// <see cref="IRecordStore.ReadExemptionsAsync"/>, read for every goal, also one that has not started (its pauses
    /// decide <see cref="PausedUntil"/> and <see cref="NextUpcoming"/>). Every entry is evaluated through
    /// <see cref="EntryViews.Of"/> with them; an entry has begun for the trend when its start is at or before
    /// <paramref name="now"/>, since an exempt entry has no Upcoming state; the trend gets each begun entry's exemption
    /// and the goal's exemptions for the current week's Upcoming commitments, Paused ones included (FR-016).
    /// </para>
    /// <para>
    /// Slice 8 (research R-07): it also evaluates <see cref="EarlierPending"/>. Its candidates' records come from the same
    /// one read, which then spans them too; a goal with no ended attempt that ended within the last 48 hours reads what
    /// it read in slice 7.
    /// </para>
    /// </summary>
    public static Task<GoalEvaluation> OfAsync(
        IRecordStore records, AccountId owner, StoredGoal stored, DateTimeOffset now, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(stored);
        return EvaluateAsync(records, owner, stored, stored.Goal.Attempt.Number, true, now, cancellationToken);
    }

    /// <summary>
    /// Evaluates attempt <paramref name="attemptNumber"/> of <paramref name="stored"/> at <paramref name="now"/> (slice
    /// 8 research R-07, FR-014): its entries through the earlier of its last date and the goal's current date, from one
    /// record read, with the target of that attempt's settings in effect. <see cref="EarlierPending"/> is empty.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The goal has no attempt with that number.</exception>
    public static Task<GoalEvaluation> OfAsync(
        IRecordStore records, AccountId owner, StoredGoal stored, int attemptNumber, DateTimeOffset now, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(stored);
        return EvaluateAsync(records, owner, stored, attemptNumber, false, now, cancellationToken);
    }

    /// <summary>
    /// The first Upcoming entry at <see cref="Now"/> whose first date is at most 6 days after
    /// <paramref name="windowStart"/> (research R-06), read from its own window
    /// <c>GoalTimeline.TryEntries(goal, windowStart, windowStart + 6)</c>, because <see cref="Entries"/> stops at the
    /// current date. A goal that has not started finds its first entry inside the window. Null when there is none.
    /// Slice 7 (FR-019): the entries are evaluated with the goal's exemptions, so a Paused entry is not Upcoming and is
    /// skipped; the earliest Upcoming entry that is not Paused is given. Slice 8 (FR-019): none for an ended goal and
    /// for an evaluation of an earlier attempt.
    /// </summary>
    public HistoryEntry? NextUpcoming(DateOnly windowStart)
    {
        if (!IsLatestAttempt || Goal.IsEnded
            || !GoalTimeline.TryEntries(Goal, windowStart, windowStart.AddDays(UpcomingWindowDays), out var window))
        {
            return null;
        }

        return window
            .Select(entry => EntryViews.Of(_stored, entry, [], null, Exemptions, Now))
            .FirstOrDefault(entry => entry.State == EvaluationState.Upcoming);
    }

    /// <summary>
    /// Slice 7 (FR-018, FR-019, research R-03): the last day of the goal's current pause at the pause date, the goal's
    /// current date or, for a goal that has not started, <paramref name="accountToday"/>; null when no pause is current
    /// then. A week that is Paused only by an upcoming pause, or after a mid-week resume, does not make the goal paused.
    /// </summary>
    public DateOnly? PausedUntil(DateOnly accountToday) => Exemptions.CurrentPause(CurrentDate ?? accountToday)?.LastDay;

    private static async Task<GoalEvaluation> EvaluateAsync(
        IRecordStore records,
        AccountId owner,
        StoredGoal stored,
        int number,
        bool withEarlierPending,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(records);
        var goal = stored.Goal;
        var version = GoalTimeline.VersionInEffect(goal, number, now);
        var latest = version.Settings.Target;
        var isLatest = number == goal.Attempt.Number;
        var current = GoalTimeline.CurrentDate(goal, now);
        var calendar = current is { } date ? GoalTimeline.EntriesThrough(goal, number, date) : [];
        var earlier = withEarlierPending ? AttemptEntries.EarlierOpenOf(goal, now) : [];
        var exemptions = await records.ReadExemptionsAsync(owner, goal.Id, cancellationToken).ConfigureAwait(false);
        var read = calendar.Count == 0 && earlier.Count == 0
            ? NoEntries
            : await records.ReadAsync(
                owner,
                goal.Id,
                earlier.Count > 0 ? earlier[0].Entry.FirstDate : calendar[0].FirstDate,
                calendar.Count > 0 ? calendar[^1].LastDate : earlier[^1].Entry.LastDate,
                cancellationToken).ConfigureAwait(false);

        // Both lists are in date order and the earlier attempts come first, so one walk over the records serves both.
        var next = 0;
        var earlierPending = new List<HistoryEntry>();
        foreach (var (_, entry) in earlier)
        {
            var view = EntryViews.Of(stored, entry, RecordsOf(read.Records, entry, ref next), read.Notes.GetValueOrDefault(entry.Key), exemptions, now);
            if (view.State == EvaluationState.Pending)
            {
                earlierPending.Add(view);
            }
        }

        var entries = new List<HistoryEntry>(calendar.Count);
        var begun = new List<(CalendarEntry Entry, GoalTarget Target, IReadOnlyList<RecordValue> Records, EvaluationState? Exemption)>(calendar.Count);
        foreach (var entry in calendar)
        {
            var own = RecordsOf(read.Records, entry, ref next);
            var view = EntryViews.Of(stored, entry, own, read.Notes.GetValueOrDefault(entry.Key), exemptions, now);
            entries.Add(view);
            if (entry.Starts <= now)
            {
                EvaluationState? exemption = IsExempt(view.State) ? view.State : null;
                begun.Add((entry, EntryViews.TargetOf(goal, entry), own.Select(record => record.Value).ToList(), exemption));
            }
        }

        // Only the latest attempt, while it runs, has commitments after the current date in the current week.
        var upcoming = isLatest && !goal.IsEnded && GoalTimeline.LatestAttemptHasBegun(goal, now) ? CurrentWeekUpcoming(goal, latest, current) : [];
        var trend = TrendBuilder.Of(latest, begun, upcoming, exemptions);
        var name = isLatest ? GoalText.Name(goal, stored.Habit.Name, now) : GoalText.AttemptName(goal, number, stored.Habit.Name);
        return new GoalEvaluation(stored, number, name, version, current, entries, earlierPending, trend, exemptions, now);
    }

    private static bool IsEnded(EvaluationState state) =>
        state is EvaluationState.Pending or EvaluationState.Met or EvaluationState.Failed or EvaluationState.Missed;

    private static bool IsExempt(EvaluationState state) => state is EvaluationState.Excused or EvaluationState.Paused;

    /// <summary>
    /// The records dated in <paramref name="entry"/>, in their stored order, as <see cref="EntryViews.RecordsIn"/>
    /// gives them; <paramref name="next"/> walks the records once, since both the entries and the records are in date
    /// order.
    /// </summary>
    private static List<GoalRecord> RecordsOf(IReadOnlyList<GoalRecord> records, CalendarEntry entry, ref int next)
    {
        while (next < records.Count && records[next].Date < entry.FirstDate)
        {
            next++;
        }

        var own = new List<GoalRecord>();
        while (next < records.Count && records[next].Date <= entry.LastDate)
        {
            own.Add(records[next]);
            next++;
        }

        return own;
    }

    /// <summary>
    /// The commitments of a commitment-based yes/no goal after its current date up to that week's Sunday, which count in
    /// the current week's trend target (FR-006); none for any other goal, or with no current date. Slice 7 (FR-016,
    /// research R-07): Paused ones are kept, since they feed only the trend, which takes their exemptions from the goal's
    /// <see cref="GoalExemptions"/>, leaves them out of the week's Build target and counts them towards an all-exempt week.
    /// </summary>
    private static IReadOnlyList<CalendarEntry> CurrentWeekUpcoming(Goal goal, GoalTarget latest, DateOnly? current)
    {
        if (current is not { } date
            || latest.ScheduleKindOf != GoalScheduleKind.CommitmentBased
            || latest.Measurement != Measurement.YesNo
            || date.DayOfWeek == DayOfWeek.Sunday)
        {
            return [];
        }

        var sunday = date.AddDays(7 - (int)date.DayOfWeek);
        return GoalTimeline.TryEntries(goal, date.AddDays(1), sunday, out var upcoming) ? upcoming : [];
    }
}
