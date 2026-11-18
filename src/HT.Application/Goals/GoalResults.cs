using HT.Domain.Goals;

namespace HT.Application.Goals;

/// <summary>The outcomes of <see cref="GoalService.StartNewGoalAsync"/>, in FR-023's check order (contracts/application-api.md).</summary>
public enum GoalStartOutcome
{
    NotSignedIn,
    NotForAdministrators,
    NotInCatalog,
    CustomHabitNotFound,
    SuggestionNotFound,
    Started,
}

/// <summary>The values a new goal starts with (FR-007), or the refusal. It never echoes an input value (HP-011).</summary>
public sealed class GoalStartResult
{
    private GoalStartResult(GoalStartOutcome outcome, string? message, GoalStartingValues? startingValues)
    {
        Outcome = outcome;
        Message = message;
        StartingValues = startingValues;
    }

    public GoalStartOutcome Outcome { get; }

    /// <summary>The refusal's text; null for <see cref="GoalStartOutcome.Started"/>.</summary>
    public string? Message { get; }

    /// <summary>Set only for <see cref="GoalStartOutcome.Started"/>.</summary>
    public GoalStartingValues? StartingValues { get; }

    internal static GoalStartResult Refused(GoalStartOutcome outcome, string message) => new(outcome, message, null);

    internal static GoalStartResult Started(GoalStartingValues values) => new(GoalStartOutcome.Started, null, values);
}

/// <summary>
/// The outcomes of <see cref="GoalService.CreateAsync"/>, in FR-006 and FR-023's check order (data-model.md): the
/// acting user, the habit, the 12 target outcomes named as <see cref="GoalProblem"/>'s, the Reduce N rule, the start
/// date and the name.
/// </summary>
public enum GoalCreateOutcome
{
    NotSignedIn,
    NotForAdministrators,
    HabitMissing,
    NotInCatalog,
    CustomHabitNotFound,
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
    StartDateTooFar,
    NameTooLong,
    Created,
}

/// <summary>
/// The result of creating a goal: the new goal's identifier, or one refusal with its text and the field it is about
/// (FR-006). A refusal carries no other value, so it never echoes the input (HP-011).
/// </summary>
public sealed class GoalCreateResult
{
    private GoalCreateResult(GoalCreateOutcome outcome, string? message, GoalField? field, GoalId? goalId)
    {
        Outcome = outcome;
        Message = message;
        Field = field;
        GoalId = goalId;
    }

    public GoalCreateOutcome Outcome { get; }

    /// <summary>The refusal's text; null for <see cref="GoalCreateOutcome.Created"/>.</summary>
    public string? Message { get; }

    /// <summary>
    /// The field the refusal is about; null for <see cref="GoalCreateOutcome.Created"/> and for the refusals of the acting
    /// user (<see cref="GoalCreateOutcome.NotSignedIn"/>, <see cref="GoalCreateOutcome.NotForAdministrators"/>).
    /// </summary>
    public GoalField? Field { get; }

    /// <summary>Set only for <see cref="GoalCreateOutcome.Created"/>.</summary>
    public GoalId? GoalId { get; }

    internal static GoalCreateResult Refused(GoalCreateOutcome outcome, string message, GoalField? field) => new(outcome, message, field, null);

    internal static GoalCreateResult Created(GoalId id) => new(GoalCreateOutcome.Created, null, null, id);
}
