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
/// </summary>
internal sealed class GoalEvaluation
{
    /// <summary>How many days after the window's first date an Upcoming entry may start (research R-06).</summary>
    private const int UpcomingWindowDays = 6;

    private static readonly StoredEntries NoEntries = new([], new Dictionary<DateOnly, NoteText>());

    private GoalEvaluation(
        Goal goal,
        string name,
        GoalTarget latest,
        DateOnly? currentDate,
        IReadOnlyList<HistoryEntry> entries,
        Trend trend,
        GoalExemptions exemptions,
        DateTimeOffset now)
    {
        Goal = goal;
        Name = name;
        LatestTarget = latest;
        CurrentDate = currentDate;
        Entries = entries;
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
        TrendTarget = trend.Bars.Count == 0 ? latest : GoalTimeline.VersionOf(goal, trend.Bars[^1].FirstDate).Settings.Target;
    }

    public Goal Goal { get; }

    /// <summary>The goal's own or derived name (slice 4 FR-010).</summary>
    public string Name { get; }

    public BuildOrReduce BuildOrReduce { get; }

    /// <summary>The unit of the goal's streaks (FR-004), from <see cref="LatestTarget"/>.</summary>
    public StreakUnit Unit { get; }

    /// <summary>The target of the settings version of <see cref="CurrentDate"/>, or of the start date when there is none.</summary>
    public GoalTarget LatestTarget { get; }

    /// <summary>The goal's current date at <see cref="Now"/>; null before the start date's day begins.</summary>
    public DateOnly? CurrentDate { get; }

    /// <summary>The instant every entry was evaluated at.</summary>
    public DateTimeOffset Now { get; }

    /// <summary>Every commitment or period through <see cref="CurrentDate"/>, in date order.</summary>
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

    /// <summary>The Pending entries, in date order.</summary>
    public IReadOnlyList<HistoryEntry> Pending { get; }

    /// <summary>The entries with a note, in date order, each with its state.</summary>
    public IReadOnlyList<HistoryEntry> Notes { get; }

    /// <summary>The trend of the begun entries (FR-006), from the <c>Of</c> overload of <see cref="TrendBuilder"/> that takes the goal's exemptions.</summary>
    public Trend Trend { get; }

    /// <summary>The target of the latest bar's settings version, which decides the trend's texts (research R-05, R-13).</summary>
    public GoalTarget TrendTarget { get; }

    /// <summary>
    /// What can be recorded now, as <see cref="RecordingService.GetRecordableAsync"/> gives it at the same instant
    /// (research R-07): the goal's settings and its Due and Pending entries, newest first.
    /// </summary>
    public GoalHistory Recordable =>
        EntryViews.HistoryOf(
            Goal,
            CurrentDate,
            Entries.Where(entry => entry.State is EvaluationState.Due or EvaluationState.Pending).OrderByDescending(entry => entry.FirstDate).ToList(),
            Now);

    /// <summary>
    /// Evaluates <paramref name="stored"/> at <paramref name="now"/>: its entries through its current date from
    /// <see cref="GoalTimeline.EntriesThrough"/>, their records and notes from one <see cref="IRecordStore.ReadAsync"/>
    /// for <paramref name="owner"/> over the first entry's first date to the last entry's last date, and, for a
    /// commitment-based yes/no goal, the Upcoming commitments of the current week for its trend's target. A goal with
    /// no current date, or no entry through it, reads no record.
    /// <para>
    /// Slice 7 (research R-06, R-07): the goal's excusals and pauses come from one
    /// <see cref="IRecordStore.ReadExemptionsAsync"/>, read for every goal, also one that has not started (its pauses
    /// decide <see cref="PausedUntil"/> and <see cref="NextUpcoming"/>). Every entry is evaluated through
    /// <see cref="EntryViews.Of"/> with them; an entry has begun for the trend when its start is at or before
    /// <paramref name="now"/>, since an exempt entry has no Upcoming state; the trend gets each begun entry's exemption
    /// and the goal's exemptions for the current week's Upcoming commitments, Paused ones included (FR-016).
    /// </para>
    /// </summary>
    public static async Task<GoalEvaluation> OfAsync(
        IRecordStore records, AccountId owner, StoredGoal stored, DateTimeOffset now, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(records);
        ArgumentNullException.ThrowIfNull(stored);
        var goal = stored.Goal;
        var current = GoalTimeline.CurrentDate(goal, now);
        var latest = GoalTimeline.VersionOf(goal, current ?? goal.StartDate).Settings.Target;
        var calendar = current is { } date ? GoalTimeline.EntriesThrough(goal, date) : [];
        var exemptions = await records.ReadExemptionsAsync(owner, goal.Id, cancellationToken).ConfigureAwait(false);
        var read = calendar.Count == 0
            ? NoEntries
            : await records.ReadAsync(owner, goal.Id, calendar[0].FirstDate, calendar[^1].LastDate, cancellationToken).ConfigureAwait(false);

        var entries = new List<HistoryEntry>(calendar.Count);
        var begun = new List<(CalendarEntry Entry, GoalTarget Target, IReadOnlyList<RecordValue> Records, EvaluationState? Exemption)>(calendar.Count);
        var next = 0;
        foreach (var entry in calendar)
        {
            var own = RecordsOf(read.Records, entry, ref next);
            var view = EntryViews.Of(goal, entry, own, read.Notes.GetValueOrDefault(entry.Key), exemptions, now);
            entries.Add(view);
            if (entry.Starts <= now)
            {
                EvaluationState? exemption = IsExempt(view.State) ? view.State : null;
                begun.Add((entry, EntryViews.TargetOf(goal, entry), own.Select(record => record.Value).ToList(), exemption));
            }
        }

        var trend = TrendBuilder.Of(latest, begun, CurrentWeekUpcoming(goal, latest, current), exemptions);
        return new GoalEvaluation(goal, GoalText.Name(goal, stored.Habit.Name), latest, current, entries, trend, exemptions, now);
    }

    /// <summary>
    /// The first Upcoming entry at <see cref="Now"/> whose first date is at most 6 days after
    /// <paramref name="windowStart"/> (research R-06), read from its own window
    /// <c>GoalTimeline.TryEntries(goal, windowStart, windowStart + 6)</c>, because <see cref="Entries"/> stops at the
    /// current date. A goal that has not started finds its first entry inside the window. Null when there is none.
    /// Slice 7 (FR-019): the entries are evaluated with the goal's exemptions, so a Paused entry is not Upcoming and is
    /// skipped; the earliest Upcoming entry that is not Paused is given.
    /// </summary>
    public HistoryEntry? NextUpcoming(DateOnly windowStart)
    {
        if (!GoalTimeline.TryEntries(Goal, windowStart, windowStart.AddDays(UpcomingWindowDays), out var window))
        {
            return null;
        }

        return window
            .Select(entry => EntryViews.Of(Goal, entry, [], null, Exemptions, Now))
            .FirstOrDefault(entry => entry.State == EvaluationState.Upcoming);
    }

    /// <summary>
    /// Slice 7 (FR-018, FR-019, research R-03): the last day of the goal's current pause at the pause date, the goal's
    /// current date or, for a goal that has not started, <paramref name="accountToday"/>; null when no pause is current
    /// then. A week that is Paused only by an upcoming pause, or after a mid-week resume, does not make the goal paused.
    /// </summary>
    public DateOnly? PausedUntil(DateOnly accountToday) => Exemptions.CurrentPause(CurrentDate ?? accountToday)?.LastDay;

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
