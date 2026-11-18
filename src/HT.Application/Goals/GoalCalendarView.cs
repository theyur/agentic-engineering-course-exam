using HT.Domain.Goals;

namespace HT.Application.Goals;

/// <summary>
/// A goal's evaluation calendar for a range (FR-013): <see cref="Commitments"/> for a commitment-based goal,
/// <see cref="Periods"/> for a period-based one, the other empty. Every instant is in UTC.
/// </summary>
public sealed record GoalCalendarView(
    GoalScheduleKind Kind, IReadOnlyList<Commitment> Commitments, IReadOnlyList<EvaluationPeriod> Periods);
