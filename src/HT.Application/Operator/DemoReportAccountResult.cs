namespace HT.Application.Operator;

/// <summary>
/// The result of demo report loading for <see cref="DemoReportService.DemoEmail"/> (FR-032); the email is in normal
/// form. Its texts name the email and the result only, never a goal, habit, note, unit or amount (FR-032, HP-007).
/// </summary>
public sealed class DemoReportAccountResult
{
    private DemoReportAccountResult(
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

    internal static DemoReportAccountResult Created(string email) =>
        new(email, DemoAccountOutcome.Created, OperatorTexts.DemoAccountCreated, DemoGoalsResult.Added, DemoRecordsResult.Added);

    internal static DemoReportAccountResult AlreadyExists(string email) =>
        new(email, DemoAccountOutcome.AlreadyExists, OperatorTexts.DemoAccountAlreadyExists, null, null);
}
