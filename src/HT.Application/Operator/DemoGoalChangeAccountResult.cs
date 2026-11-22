namespace HT.Application.Operator;

/// <summary>
/// The result of demo goal-change loading for <see cref="DemoGoalChangeService.DemoEmail"/> (slice 8 FR-031); the email is in normal form.
/// Its texts name the email and the result only, never a goal, habit, note, outcome, unit or amount (FR-031, HP-010).
/// </summary>
public sealed class DemoGoalChangeAccountResult
{
    private DemoGoalChangeAccountResult(
        string email, DemoAccountOutcome outcome, string message, DemoGoalsResult? goals, DemoRecordsResult? records)
    {
        Email = email;
        Outcome = outcome;
        Message = message;
        Goals = goals;
        Records = records;
    }

    public string Email { get; }

    public DemoAccountOutcome Outcome { get; }

    /// <summary>The spec's text, "created" or "already exists", written to the operator log.</summary>
    public string Message { get; }

    /// <summary>"demo goals added" for <see cref="DemoAccountOutcome.Created"/>, also when goals were left out; null otherwise.</summary>
    public DemoGoalsResult? Goals { get; }

    /// <summary>"demo records added" for <see cref="DemoAccountOutcome.Created"/>, also when goals were left out; null otherwise.</summary>
    public DemoRecordsResult? Records { get; }

    internal static DemoGoalChangeAccountResult Created(string email) =>
        new(email, DemoAccountOutcome.Created, OperatorTexts.DemoAccountCreated, DemoGoalsResult.Added, DemoRecordsResult.Added);

    internal static DemoGoalChangeAccountResult AlreadyExists(string email) =>
        new(email, DemoAccountOutcome.AlreadyExists, OperatorTexts.DemoAccountAlreadyExists, null, null);
}
