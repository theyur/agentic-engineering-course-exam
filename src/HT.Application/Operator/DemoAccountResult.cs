namespace HT.Application.Operator;

/// <summary>What demo loading did with one demo email (FR-016).</summary>
public enum DemoAccountOutcome
{
    Created,
    AlreadyExists,
}

/// <summary>The result of demo loading for one demo email; the email is in normal form.</summary>
public sealed class DemoAccountResult
{
    private DemoAccountResult(string email, DemoAccountOutcome outcome, string message, DemoCustomHabitsResult? customHabits)
    {
        Email = email;
        Outcome = outcome;
        Message = message;
        CustomHabits = customHabits;
    }

    public string Email { get; }

    public DemoAccountOutcome Outcome { get; }

    /// <summary>The spec's text, written to the operator log.</summary>
    public string Message { get; }

    /// <summary>
    /// The custom-habit result (FR-031): set for <see cref="DemoAccountOutcome.Created"/>, whose custom habits are
    /// created with the account; null for <see cref="DemoAccountOutcome.AlreadyExists"/>, whose data is not touched.
    /// </summary>
    public DemoCustomHabitsResult? CustomHabits { get; }

    internal static DemoAccountResult Created(string email) =>
        new(email, DemoAccountOutcome.Created, OperatorTexts.DemoAccountCreated, DemoCustomHabitsResult.Added);

    internal static DemoAccountResult AlreadyExists(string email) =>
        new(email, DemoAccountOutcome.AlreadyExists, OperatorTexts.DemoAccountAlreadyExists, null);
}
