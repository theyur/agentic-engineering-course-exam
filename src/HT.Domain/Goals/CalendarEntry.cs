namespace HT.Domain.Goals;

/// <summary>
/// One commitment (both dates equal) or one evaluation period of a goal (slice 5 research R-03): its first and last
/// date, and the instants (UTC) at which it starts and ends. Made by <see cref="GoalTimeline.EntryOf"/> and
/// <see cref="GoalTimeline.TryEntries"/>.
/// </summary>
public sealed record CalendarEntry(DateOnly FirstDate, DateOnly LastDate, DateTimeOffset Starts, DateTimeOffset Ends)
{
    /// <summary>The date records and notes are filed under: the commitment's date, the day, or the week's Monday.</summary>
    public DateOnly Key => FirstDate;
}
