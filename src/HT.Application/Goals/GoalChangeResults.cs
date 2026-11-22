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
