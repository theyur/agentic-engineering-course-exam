namespace HT.Domain.Evaluation;

/// <summary>
/// The state of a commitment or evaluation period at a supplied time, or the exemption in its place (slice 5 FR-010;
/// slice 7 research R-01): Upcoming, Due and Pending before its grace period ends; Met, Failed or Missed, final,
/// after; Excused or Paused while an excusal or a pause exempts it, at every supplied time (slice 7 FR-003, FR-007).
/// Only Upcoming to Missed are evaluation states in <c>CONTEXT.md</c>'s sense; Excused and Paused are exemptions that
/// take the state's place, never final and never counted.
/// </summary>
public enum EvaluationState
{
    Upcoming,
    Due,
    Pending,
    Met,
    Failed,
    Missed,
    Excused,
    Paused,
}
