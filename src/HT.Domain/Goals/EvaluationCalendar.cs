namespace HT.Domain.Goals;

/// <summary>
/// A goal's evaluation calendar for a range of dates (FR-013): a schedule kind and, in date order, the commitments of
/// its commitment-based attempts and the evaluation periods of its period-based attempts in the range. One list is
/// empty unless the range spans attempts of both kinds (slice 8 research R-02); <see cref="Kind"/> follows
/// <see cref="GoalTimeline.TryCalendar"/>'s rule. No status (slice 5). Made by <see cref="GoalTimeline.TryCalendar"/>.
/// </summary>
public sealed class EvaluationCalendar
{
    internal EvaluationCalendar(GoalScheduleKind kind, IReadOnlyList<Commitment> commitments, IReadOnlyList<EvaluationPeriod> periods)
    {
        Kind = kind;
        Commitments = commitments;
        Periods = periods;
    }

    /// <summary>The kind of the latest attempt with an entry in the range, else of the latest attempt starting on or before its last date, else of attempt 1.</summary>
    public GoalScheduleKind Kind { get; }

    /// <summary>The commitments in date order; empty when the range has no commitment-based attempt.</summary>
    public IReadOnlyList<Commitment> Commitments { get; }

    /// <summary>The evaluation periods in date order; empty when the range has no period-based attempt.</summary>
    public IReadOnlyList<EvaluationPeriod> Periods { get; }
}
