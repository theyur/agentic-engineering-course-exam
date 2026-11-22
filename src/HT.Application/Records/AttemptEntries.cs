using HT.Domain.Evaluation;
using HT.Domain.Goals;

namespace HT.Application.Records;

/// <summary>
/// Slice 8 (research R-02, R-07): the attempt facts the read path needs that <see cref="GoalTimeline"/> gives only
/// through its public members, derived in this one place. HT.Domain keeps its own "no date" rule internal and shares
/// no internals with this layer (ADR 0010), so the rule here is stated through <see cref="GoalTimeline.LastDate"/> and
/// <see cref="GoalTimeline.EntriesThrough(Goal, int, DateOnly)"/> and gives the same answer.
/// </summary>
internal static class AttemptEntries
{
    /// <summary>
    /// How many days after an attempt's timing date its first entry ends at the latest: a commitment-based attempt
    /// schedules at least one weekday, and a per-week attempt's first week starts on the first Monday on or after it.
    /// </summary>
    private const int FirstEntryDays = 13;

    /// <summary>
    /// The number of the attempt whose entries contain <paramref name="entry"/>: the latest attempt for a goal with one
    /// attempt, without a timeline call, so slice 7's reads cost what they did.
    /// </summary>
    internal static int NumberOf(Goal goal, CalendarEntry entry) =>
        goal.Attempts.Count == 1 ? goal.Attempt.Number : GoalTimeline.AttemptOf(goal, entry.FirstDate) ?? goal.Attempt.Number;

    /// <summary>
    /// The date attempt <paramref name="number"/> is timed from: its first version's date, which is the goal's start
    /// date for attempt 1 and the attempt's own start date for a later one (research R-01).
    /// </summary>
    internal static DateOnly TimingDateOf(Goal goal, int number) => goal.VersionsOf(number)[0].AppliesFrom;

    /// <summary>
    /// The entries of attempt <paramref name="number"/> that matter for its first and last entry: every entry through
    /// its last date for an ended attempt (empty when it has no date: its last date is before its first entry, plan
    /// "Spec discrepancies" 3), and its first entries for an attempt that is not ended.
    /// </summary>
    internal static IReadOnlyList<CalendarEntry> BoundsOf(Goal goal, int number) =>
        GoalTimeline.LastDate(goal, number) is { } last
            ? GoalTimeline.EntriesThrough(goal, number, last)
            : GoalTimeline.EntriesThrough(goal, number, TimingDateOf(goal, number).AddDays(FirstEntryDays));

    /// <summary>Whether attempt <paramref name="number"/> is ended with no commitment or period (HP-001's "ended before it began").</summary>
    internal static bool HasNoDate(Goal goal, int number) =>
        GoalTimeline.LastDate(goal, number) is not null && BoundsOf(goal, number).Count == 0;

    /// <summary>
    /// The entries of the ended attempts other than the latest whose grace period is open at <paramref name="now"/>
    /// (research R-07, the candidates of <c>EarlierPending</c>), in date order. Only an attempt whose last entry's grace
    /// period is open at <paramref name="now"/>, i.e. whose last entry ended within the last 48 hours, is looked at; the
    /// state of each candidate is decided by <see cref="EntryViews.Of"/> with its records and the goal's exemptions.
    /// </summary>
    internal static IReadOnlyList<(int Number, CalendarEntry Entry)> EarlierOpenOf(Goal goal, DateTimeOffset now)
    {
        var open = new List<(int, CalendarEntry)>();
        foreach (var attempt in goal.Attempts.Take(goal.Attempts.Count - 1))
        {
            var number = attempt.Attempt.Number;
            var entries = BoundsOf(goal, number);
            if (entries.Count == 0 || GracePeriod.EndOf(entries[^1].Ends) <= now)
            {
                continue;
            }

            open.AddRange(entries.Where(entry => GracePeriod.EndOf(entry.Ends) > now).Select(entry => (number, entry)));
        }

        return open;
    }
}
