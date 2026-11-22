using HT.Domain.Goals;

namespace HT.Application.Goals;

/// <summary>
/// A goal's evaluation calendar for a range (FR-013), mirroring <see cref="EvaluationCalendar"/>: the
/// <see cref="Commitments"/> of its commitment-based attempts and the <see cref="Periods"/> of its period-based ones in
/// the range, each in date order; one of them is empty unless the range spans attempts of both kinds (slice 8 R-02).
/// <see cref="Kind"/> is the kind of the latest attempt with an entry in the range. Every instant is in UTC.
/// </summary>
public sealed record GoalCalendarView(
    GoalScheduleKind Kind, IReadOnlyList<Commitment> Commitments, IReadOnlyList<EvaluationPeriod> Periods);
