namespace HT.Domain.Evaluation;

/// <summary>
/// The state of a commitment or evaluation period at a supplied time (FR-010): Upcoming, Due and Pending before its
/// grace period ends; Met, Failed or Missed, final, after. Excused and Paused come in slice 7.
/// </summary>
public enum EvaluationState
{
    Upcoming,
    Due,
    Pending,
    Met,
    Failed,
    Missed,
}
