using HT.Domain.Evaluation;

namespace HT.Application.Exemptions;

/// <summary>The outcomes of excusing a commitment, in FR-002's check order (contracts/application-api.md).</summary>
public enum ExcuseOutcome
{
    NotSignedIn,
    NotForAdministrators,
    NotFound,
    NotACommitment,
    NothingScheduled,
    DatePaused,
    AlreadyExcused,
    NotBegun,
    Final,
    HasRecord,
    ReasonTooLong,
    Excused,
}

/// <summary>
/// The result of excusing a commitment (FR-001, FR-002): the excused commitment's date, or one refusal with its text. A
/// refusal carries no input value, so it never echoes a reason or a date (HP-009).
/// </summary>
public sealed class ExcuseResult
{
    private ExcuseResult(ExcuseOutcome outcome, string message, DateOnly? date)
    {
        Outcome = outcome;
        Message = message;
        Date = date;
    }

    public ExcuseOutcome Outcome { get; }

    /// <summary>The refusal's text, or the success word "excused" (<see cref="ExcusalTexts.Excused"/>).</summary>
    public string Message { get; }

    /// <summary>The excused commitment's date; set only for <see cref="ExcuseOutcome.Excused"/>.</summary>
    public DateOnly? Date { get; }

    internal static ExcuseResult Refused(ExcuseOutcome outcome, string message) => new(outcome, message, null);

    internal static ExcuseResult Excused(DateOnly date) => new(ExcuseOutcome.Excused, ExcusalTexts.Excused, date);
}

/// <summary>The outcomes of removing an excusal, in FR-004's check order (contracts/application-api.md).</summary>
public enum RemoveExcusalOutcome
{
    NotSignedIn,
    NotForAdministrators,
    NotFound,
    NotExcused,
    TooLate,
    Removed,
}

/// <summary>
/// The result of removing an excusal (FR-004): the commitment's state after the change, or one refusal with its text.
/// A refusal carries no input value (HP-009).
/// </summary>
public sealed class RemoveExcusalResult
{
    private RemoveExcusalResult(RemoveExcusalOutcome outcome, string message, EvaluationState? state)
    {
        Outcome = outcome;
        Message = message;
        State = state;
    }

    public RemoveExcusalOutcome Outcome { get; }

    /// <summary>The refusal's text, or the success words "excusal removed" (<see cref="ExcusalTexts.Removed"/>).</summary>
    public string Message { get; }

    /// <summary>
    /// The commitment's state after the change, at the same supplied time: Due, Pending, or Paused under a pause; set
    /// only for <see cref="RemoveExcusalOutcome.Removed"/>.
    /// </summary>
    public EvaluationState? State { get; }

    internal static RemoveExcusalResult Refused(RemoveExcusalOutcome outcome, string message) => new(outcome, message, null);

    internal static RemoveExcusalResult Removed(EvaluationState state) => new(RemoveExcusalOutcome.Removed, ExcusalTexts.Removed, state);
}
