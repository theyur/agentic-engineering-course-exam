namespace HT.Domain.Exemptions;

/// <summary>
/// A pause of a goal (FR-006): from <paramref name="FirstDay"/> to <paramref name="LastDay"/>, both included, dates on
/// the goal's own timeline (ADR 0016), with an optional reason. A stored fact; whether an entry reads Paused is derived
/// on read by <see cref="GoalExemptions.ExemptionOf"/>.
/// </summary>
/// <exception cref="ArgumentException"><paramref name="LastDay"/> is before <paramref name="FirstDay"/>.</exception>
public sealed record Pause(PauseId Id, DateOnly FirstDay, DateOnly LastDay, ExemptionReason? Reason)
{
    /// <summary>The first paused day.</summary>
    public DateOnly FirstDay { get; } = FirstDay;

    /// <summary>The last paused day; never before <see cref="FirstDay"/>.</summary>
    public DateOnly LastDay { get; } = LastDay >= FirstDay
        ? LastDay
        : throw new ArgumentException("The last day cannot be before the first day.", nameof(LastDay));

    /// <summary>Whether the pause covers <paramref name="date"/>: <c>FirstDay ≤ date ≤ LastDay</c>.</summary>
    public bool Covers(DateOnly date) => FirstDay <= date && date <= LastDay;

    /// <summary>
    /// Whether the pause shares a date with the range <paramref name="first"/> to <paramref name="last"/>:
    /// <c>FirstDay ≤ last &amp;&amp; first ≤ LastDay</c>. Touching ranges (one ends on Sunday, the other starts on Monday)
    /// do not overlap.
    /// </summary>
    public bool Overlaps(DateOnly first, DateOnly last) => FirstDay <= last && first <= LastDay;

    /// <summary>
    /// The pause's timing on <paramref name="today"/> (FR-012): <see cref="PauseTiming.Upcoming"/> when it starts after
    /// it, <see cref="PauseTiming.Past"/> when it ended before it, else <see cref="PauseTiming.Current"/>.
    /// </summary>
    public PauseTiming TimingAt(DateOnly today) =>
        FirstDay > today ? PauseTiming.Upcoming
        : LastDay < today ? PauseTiming.Past
        : PauseTiming.Current;
}
