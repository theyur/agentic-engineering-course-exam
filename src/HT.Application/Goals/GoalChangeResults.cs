using HT.Domain.Goals;

namespace HT.Application.Goals;

/// <summary>
/// The outcomes of <see cref="GoalChangeService.ChangeSettingsAsync"/>, in FR-003's check order (data-model.md "Results
/// and views"): the acting user, the goal, an ended goal, the 12 target outcomes named as <see cref="GoalProblem"/>'s
/// in its order, the Reduce N rule, the schedule kind and the name; then the two successes.
/// </summary>
public enum SettingsChangeOutcome
{
    NotSignedIn,
    NotForAdministrators,
    NotFound,
    GoalEnded,
    BuildOrReduceMissing,
    MeasurementMissing,
    YesNoHasAmountOrUnit,
    BuildAmountOutOfRange,
    ReduceAmountOutOfRange,
    CountUnitMissing,
    CountUnitTooLong,
    DurationUnitMissing,
    ScheduleMissing,
    DaysMissing,
    TimesOutOfRange,
    PeriodMissing,
    ReduceAmountNeedsOneTime,
    ScheduleKindChanged,
    NameTooLong,
    Unchanged,
    Changed,
}

/// <summary>
/// The result of a settings change (slice 8 FR-003): "settings changed" with the date the settings apply from (none for a
/// change of the name only), "settings unchanged", or one refusal with its text and the field it is about. A refusal
/// carries no input value (HP-010).
/// </summary>
public sealed class SettingsChangeResult
{
    private SettingsChangeResult(SettingsChangeOutcome outcome, string message, DateOnly? appliesFrom, GoalField? field)
    {
        Outcome = outcome;
        Message = message;
        AppliesFrom = appliesFrom;
        Field = field;
    }

    public SettingsChangeOutcome Outcome { get; }

    /// <summary>
    /// The refusal's text, or the success words "settings changed" (<see cref="GoalChangeTexts.SettingsChanged"/>) and
    /// "settings unchanged" (<see cref="GoalChangeTexts.SettingsUnchanged"/>).
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// The date from which the new settings apply (FR-002); set only for <see cref="SettingsChangeOutcome.Changed"/>, and
    /// null for a change of the name only, which applies at once.
    /// </summary>
    public DateOnly? AppliesFrom { get; }

    /// <summary>
    /// The field a refusal is about: the target field of a target refusal (<see cref="GoalDraft.FieldOf"/>),
    /// <see cref="GoalField.Schedule"/> for <see cref="SettingsChangeOutcome.ScheduleKindChanged"/> and
    /// <see cref="GoalField.Name"/> for <see cref="SettingsChangeOutcome.NameTooLong"/>; null for the successes and for
    /// the refusals of the acting user, the goal and an ended goal.
    /// </summary>
    public GoalField? Field { get; }

    internal static SettingsChangeResult Refused(SettingsChangeOutcome outcome, string message, GoalField? field) =>
        new(outcome, message, null, field);

    internal static SettingsChangeResult Changed(DateOnly? appliesFrom) =>
        new(SettingsChangeOutcome.Changed, GoalChangeTexts.SettingsChanged, appliesFrom, null);

    internal static SettingsChangeResult Unchanged() =>
        new(SettingsChangeOutcome.Unchanged, GoalChangeTexts.SettingsUnchanged, null, null);
}

/// <summary>
/// The outcomes of <see cref="GoalChangeService.EndAsync"/>, in FR-007's check order (data-model.md "Results and views"):
/// the acting user, the goal, an ended goal, no outcome and the note; then the success.
/// </summary>
public enum GoalEndOutcome
{
    NotSignedIn,
    NotForAdministrators,
    NotFound,
    AlreadyEnded,
    OutcomeMissing,
    NoteTooLong,
    Ended,
}

/// <summary>
/// The result of an ending (slice 8 FR-007, FR-008): "goal ended" with the ending date and the ended attempt's last date,
/// or one refusal with its text. A refusal carries no input value, and neither result carries the note (HP-010).
/// </summary>
public sealed class GoalEndResult
{
    private GoalEndResult(GoalEndOutcome outcome, string message, DateOnly? endingDate, DateOnly? lastDate)
    {
        Outcome = outcome;
        Message = message;
        EndingDate = endingDate;
        LastDate = lastDate;
    }

    public GoalEndOutcome Outcome { get; }

    /// <summary>The refusal's text, or the success words "goal ended" (<see cref="GoalChangeTexts.Ended"/>).</summary>
    public string Message { get; }

    /// <summary>
    /// The ending date E (FR-007): the goal's current date, or the account's today for a goal that has not started; set
    /// only for <see cref="GoalEndOutcome.Ended"/>.
    /// </summary>
    public DateOnly? EndingDate { get; }

    /// <summary>
    /// The ended attempt's last date L (FR-008), before its start date when the attempt has no commitment or period; set
    /// only for <see cref="GoalEndOutcome.Ended"/>.
    /// </summary>
    public DateOnly? LastDate { get; }

    internal static GoalEndResult Refused(GoalEndOutcome outcome, string message) => new(outcome, message, null, null);

    internal static GoalEndResult Ended(DateOnly endingDate, DateOnly lastDate) =>
        new(GoalEndOutcome.Ended, GoalChangeTexts.Ended, endingDate, lastDate);
}

/// <summary>
/// The outcomes of <see cref="GoalChangeService.GetNewAttemptValuesAsync"/>, in FR-011's check order (data-model.md
/// "Results and views"): the acting user, the goal and a goal that is not ended; then the success.
/// </summary>
public enum NewAttemptValuesOutcome
{
    NotSignedIn,
    NotForAdministrators,
    NotFound,
    NotEnded,
    Found,
}

/// <summary>
/// The starting values of a new attempt (slice 8 FR-011), or one refusal with its text. A refusal carries no values.
/// </summary>
public sealed class NewAttemptValuesResult
{
    private NewAttemptValuesResult(NewAttemptValuesOutcome outcome, string? message, NewAttemptValues? values)
    {
        Outcome = outcome;
        Message = message;
        Values = values;
    }

    public NewAttemptValuesOutcome Outcome { get; }

    /// <summary>The refusal's text; null for <see cref="NewAttemptValuesOutcome.Found"/>.</summary>
    public string? Message { get; }

    /// <summary>Set only for <see cref="NewAttemptValuesOutcome.Found"/>.</summary>
    public NewAttemptValues? Values { get; }

    internal static NewAttemptValuesResult Refused(NewAttemptValuesOutcome outcome, string message) => new(outcome, message, null);

    internal static NewAttemptValuesResult Found(NewAttemptValues values) => new(NewAttemptValuesOutcome.Found, null, values);
}

/// <summary>
/// The outcomes of <see cref="GoalChangeService.StartAttemptAsync"/>, in FR-012's check order (data-model.md "Results and
/// views"): the acting user, the goal, a goal that is not ended, the 13 target outcomes named as <see cref="GoalProblem"/>'s
/// in its order (the Reduce N rule last), then the four start date outcomes; then the success.
/// </summary>
public enum AttemptStartOutcome
{
    NotSignedIn,
    NotForAdministrators,
    NotFound,
    NotEnded,
    BuildOrReduceMissing,
    MeasurementMissing,
    YesNoHasAmountOrUnit,
    BuildAmountOutOfRange,
    ReduceAmountOutOfRange,
    CountUnitMissing,
    CountUnitTooLong,
    DurationUnitMissing,
    ScheduleMissing,
    DaysMissing,
    TimesOutOfRange,
    PeriodMissing,
    ReduceAmountNeedsOneTime,
    StartDateMissing,
    StartDateInPast,
    StartDateNotAfterEnding,
    StartDateTooFar,
    Started,
}

/// <summary>
/// The result of starting a new attempt (slice 8 FR-012): "attempt started" with the attempt's number and start date, or
/// one refusal with its text and the field it is about. A refusal carries no input value (HP-010).
/// </summary>
public sealed class AttemptStartResult
{
    private AttemptStartResult(AttemptStartOutcome outcome, string message, int? attemptNumber, DateOnly? startDate, GoalField? field)
    {
        Outcome = outcome;
        Message = message;
        AttemptNumber = attemptNumber;
        StartDate = startDate;
        Field = field;
    }

    public AttemptStartOutcome Outcome { get; }

    /// <summary>The refusal's text, or the success words "attempt started" (<see cref="GoalChangeTexts.AttemptStarted"/>).</summary>
    public string Message { get; }

    /// <summary>The new attempt's number, the previous number + 1; set only for <see cref="AttemptStartOutcome.Started"/>.</summary>
    public int? AttemptNumber { get; }

    /// <summary>The new attempt's start date; set only for <see cref="AttemptStartOutcome.Started"/>.</summary>
    public DateOnly? StartDate { get; }

    /// <summary>
    /// The field a refusal is about: the target field of a target refusal and <see cref="GoalField.StartDate"/> for the
    /// four start date refusals (<see cref="GoalDraft.FieldOf"/>); null for the success and for the refusals of the acting
    /// user, the goal and a goal that is not ended.
    /// </summary>
    public GoalField? Field { get; }

    internal static AttemptStartResult Refused(AttemptStartOutcome outcome, string message, GoalField? field) =>
        new(outcome, message, null, null, field);

    internal static AttemptStartResult Started(int attemptNumber, DateOnly startDate) =>
        new(AttemptStartOutcome.Started, GoalChangeTexts.AttemptStarted, attemptNumber, startDate, null);
}
