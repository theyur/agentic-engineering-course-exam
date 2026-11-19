namespace HT.Domain.Evaluation;

/// <summary>
/// The evaluation of one commitment or evaluation period at a supplied time (FR-010, FR-012 to FR-016): its state,
/// its progress, the instant its grace period ends and whether it is final (the supplied time is at or after that
/// instant).
/// </summary>
public sealed record EntryEvaluation(EvaluationState State, Progress Progress, DateTimeOffset GraceEnds, bool IsFinal);
