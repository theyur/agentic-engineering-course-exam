using HT.Domain.Evaluation;
using HT.Domain.Goals;

namespace HT.Domain.Exemptions;

/// <summary>
/// One goal's excusals and pauses, as the record store reads them (research R-02), and the exemption rule of an entry
/// (FR-003, FR-007, D-1). A pure value of dates: no clock, no time zone and no culture.
/// </summary>
public sealed class GoalExemptions
{
    private readonly Dictionary<DateOnly, Excusal> _excusalsByDate;

    /// <summary>One goal's <paramref name="excusals"/> and <paramref name="pauses"/>, in any order.</summary>
    /// <exception cref="ArgumentException">
    /// Two excusals of the same date, or two pauses that share a date: the store never holds either (R-04).
    /// </exception>
    public GoalExemptions(IEnumerable<Excusal> excusals, IEnumerable<Pause> pauses)
    {
        ArgumentNullException.ThrowIfNull(excusals);
        ArgumentNullException.ThrowIfNull(pauses);

        Excusals = [.. excusals.OrderBy(excusal => excusal.Date)];
        _excusalsByDate = new Dictionary<DateOnly, Excusal>(Excusals.Count);
        foreach (var excusal in Excusals)
        {
            if (!_excusalsByDate.TryAdd(excusal.Date, excusal))
            {
                throw new ArgumentException("A commitment has at most one excusal.", nameof(excusals));
            }
        }

        Pauses = [.. pauses.OrderByDescending(pause => pause.FirstDay)];
        for (var i = 1; i < Pauses.Count; i++)
        {
            if (Pauses[i].LastDay >= Pauses[i - 1].FirstDay)
            {
                throw new ArgumentException("The pauses of a goal never share a date.", nameof(pauses));
            }
        }
    }

    /// <summary>No excusal and no pause: nothing is exempt.</summary>
    public static GoalExemptions None { get; } = new([], []);

    /// <summary>The excusals, by date.</summary>
    public IReadOnlyList<Excusal> Excusals { get; }

    /// <summary>The pauses, the latest first day first (FR-012).</summary>
    public IReadOnlyList<Pause> Pauses { get; }

    /// <summary>Whether a pause covers <paramref name="date"/>.</summary>
    public bool IsPaused(DateOnly date) => Pauses.Any(pause => pause.Covers(date));

    /// <summary>Whether <paramref name="date"/> has an excusal.</summary>
    public bool IsExcused(DateOnly date) => _excusalsByDate.ContainsKey(date);

    /// <summary>The excusal of <paramref name="date"/>, or null.</summary>
    public Excusal? ExcusalOf(DateOnly date) => _excusalsByDate.GetValueOrDefault(date);

    /// <summary>
    /// The pause that is current on <paramref name="today"/> (<see cref="Pause.TimingAt"/>), or null. At most one,
    /// since pauses do not overlap.
    /// </summary>
    public Pause? CurrentPause(DateOnly today) => Pauses.FirstOrDefault(pause => pause.TimingAt(today) == PauseTiming.Current);

    /// <summary>Whether some pause shares a date with the range <paramref name="first"/> to <paramref name="last"/>.</summary>
    public bool Overlaps(DateOnly first, DateOnly last) => Pauses.Any(pause => pause.Overlaps(first, last));

    /// <summary>
    /// The exemption of <paramref name="entry"/> (FR-003, FR-007, D-1): <see cref="EvaluationState.Paused"/> when some
    /// date from its first to its last date is in a pause (a commitment or a day: its one date; a week: any of its seven
    /// dates); else <see cref="EvaluationState.Excused"/> when the goal is commitment-based and the entry's date has an
    /// excusal; else null. So a pause wins over an excusal of the same date, and the excusal is kept. An excusal never
    /// exempts a period.
    /// </summary>
    public EvaluationState? ExemptionOf(CalendarEntry entry, GoalScheduleKind kind)
    {
        ArgumentNullException.ThrowIfNull(entry);
        if (Overlaps(entry.FirstDate, entry.LastDate))
        {
            return EvaluationState.Paused;
        }

        return kind == GoalScheduleKind.CommitmentBased && IsExcused(entry.Key) ? EvaluationState.Excused : null;
    }
}
