using HT.Application.Accounts;
using HT.Domain.Exemptions;

namespace HT.Application.Exemptions;

/// <summary>
/// One pause of a goal as the pauses list and the result of pausing give it (FR-006, FR-012): its identifier, first and
/// last day, reason, and timing at the pause date (research R-03).
/// </summary>
public sealed record PauseView
{
    public required PauseId Id { get; init; }

    /// <summary>The first paused day, on the goal's own timeline.</summary>
    public required DateOnly FirstDay { get; init; }

    /// <summary>The last paused day; never before <see cref="FirstDay"/>.</summary>
    public required DateOnly LastDay { get; init; }

    /// <summary>The trimmed reason; null when it has none.</summary>
    public string? Reason { get; init; }

    /// <summary>Current, upcoming or past at the pause date of the call (FR-012).</summary>
    public required PauseTiming Timing { get; init; }

    /// <summary>The pause's dates as the Pauses card shows them (HP-001): "Mon 16 Nov – Sun 22 Nov".</summary>
    public string RangeText => DateTexts.ShortDate(FirstDay) + " – " + DateTexts.ShortDate(LastDay);

    internal static PauseView Of(Pause pause, DateOnly pauseDate) => new()
    {
        Id = pause.Id,
        FirstDay = pause.FirstDay,
        LastDay = pause.LastDay,
        Reason = pause.Reason?.Value,
        Timing = pause.TimingAt(pauseDate),
    };
}

/// <summary>The outcomes of pausing a goal, in FR-006's check order (contracts/application-api.md).</summary>
public enum PauseOutcome
{
    NotSignedIn,
    NotForAdministrators,
    NotFound,
    DaysMissing,
    StartsBeforeToday,
    LastBeforeFirst,
    Overlaps,
    ReasonTooLong,
    Paused,
}

/// <summary>
/// The result of pausing a goal (FR-006): the new pause, or one refusal with its text. A refusal carries no input value,
/// so it never echoes a reason or a date (HP-009).
/// </summary>
public sealed class PauseResult
{
    private PauseResult(PauseOutcome outcome, string message, PauseView? pause)
    {
        Outcome = outcome;
        Message = message;
        Pause = pause;
    }

    public PauseOutcome Outcome { get; }

    /// <summary>The refusal's text, or the success word "paused" (<see cref="PauseTexts.Paused"/>).</summary>
    public string Message { get; }

    /// <summary>The new pause with its identifier, first and last day; set only for <see cref="PauseOutcome.Paused"/>.</summary>
    public PauseView? Pause { get; }

    internal static PauseResult Refused(PauseOutcome outcome, string message) => new(outcome, message, null);

    internal static PauseResult Paused(PauseView pause) => new(PauseOutcome.Paused, PauseTexts.Paused, pause);
}

/// <summary>The outcomes of resuming a goal, in FR-010's check order (contracts/application-api.md).</summary>
public enum ResumeOutcome
{
    NotSignedIn,
    NotForAdministrators,
    NotFound,
    NotPaused,
    Resumed,
}

/// <summary>The result of resuming a goal (FR-010). A refusal carries no input value (HP-009).</summary>
public sealed class ResumeResult
{
    private ResumeResult(ResumeOutcome outcome, string message)
    {
        Outcome = outcome;
        Message = message;
    }

    public ResumeOutcome Outcome { get; }

    /// <summary>The refusal's text, or the success word "resumed" (<see cref="PauseTexts.Resumed"/>).</summary>
    public string Message { get; }

    internal static ResumeResult Refused(ResumeOutcome outcome, string message) => new(outcome, message);

    internal static ResumeResult Resumed() => new(ResumeOutcome.Resumed, PauseTexts.Resumed);
}

/// <summary>The outcomes of cancelling a pause, in FR-011's check order (contracts/application-api.md).</summary>
public enum CancelPauseOutcome
{
    NotSignedIn,
    NotForAdministrators,
    NotFound,
    PauseNotFound,
    HasBegun,
    Cancelled,
}

/// <summary>The result of cancelling an upcoming pause (FR-011). A refusal carries no input value (HP-009).</summary>
public sealed class CancelPauseResult
{
    private CancelPauseResult(CancelPauseOutcome outcome, string message)
    {
        Outcome = outcome;
        Message = message;
    }

    public CancelPauseOutcome Outcome { get; }

    /// <summary>The refusal's text, or the success words "pause cancelled" (<see cref="PauseTexts.Cancelled"/>).</summary>
    public string Message { get; }

    internal static CancelPauseResult Refused(CancelPauseOutcome outcome, string message) => new(outcome, message);

    internal static CancelPauseResult Cancelled() => new(CancelPauseOutcome.Cancelled, PauseTexts.Cancelled);
}

/// <summary>The outcomes of reading a goal's pauses, in FR-012's check order (contracts/application-api.md).</summary>
public enum PausesOutcome
{
    NotSignedIn,
    NotForAdministrators,
    NotFound,
    Found,
}

/// <summary>The result of reading a goal's pauses (FR-012): every pause, the latest first day first, or one refusal.</summary>
public sealed class PausesResult
{
    private PausesResult(PausesOutcome outcome, string? message, IReadOnlyList<PauseView> pauses, DateOnly? pauseDate)
    {
        Outcome = outcome;
        Message = message;
        Pauses = pauses;
        PauseDate = pauseDate;
    }

    public PausesOutcome Outcome { get; }

    /// <summary>The refusal's text; null for <see cref="PausesOutcome.Found"/>.</summary>
    public string? Message { get; }

    /// <summary>Every pause of the goal, the latest first day first; empty on a refusal.</summary>
    public IReadOnlyList<PauseView> Pauses { get; }

    /// <summary>
    /// The pause date of the read (research R-03): the goal's current date, or the account's today for a goal that has
    /// not started; the earliest first day a new pause may have (FR-006 step 5). The goal page gives it to the
    /// PauseDialog as the default and <c>min</c> of "From" (HP-002). Set only for <see cref="PausesOutcome.Found"/>.
    /// </summary>
    public DateOnly? PauseDate { get; }

    internal static PausesResult Refused(PausesOutcome outcome, string message) => new(outcome, message, [], null);

    internal static PausesResult Found(IReadOnlyList<PauseView> pauses, DateOnly pauseDate) =>
        new(PausesOutcome.Found, null, pauses, pauseDate);
}
