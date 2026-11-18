namespace HT.Domain.Goals;

/// <summary>
/// A goal's evaluation calendar for a range of dates (FR-013): its schedule kind and, in date order, its commitments
/// (commitment-based) or its evaluation periods (period-based); the other list is empty. No status (slice 5).
/// Made by <see cref="GoalTimeline.TryCalendar"/>.
/// </summary>
public sealed class EvaluationCalendar
{
    internal EvaluationCalendar(GoalScheduleKind kind, IReadOnlyList<Commitment> commitments, IReadOnlyList<EvaluationPeriod> periods)
    {
        Kind = kind;
        Commitments = commitments;
        Periods = periods;
    }

    public GoalScheduleKind Kind { get; }

    /// <summary>The commitments in date order; empty for a period-based goal.</summary>
    public IReadOnlyList<Commitment> Commitments { get; }

    /// <summary>The evaluation periods in date order; empty for a commitment-based goal.</summary>
    public IReadOnlyList<EvaluationPeriod> Periods { get; }
}
