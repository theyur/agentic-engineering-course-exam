namespace HT.Application.Operator;

/// <summary>What demo loading did with the custom habits of a demo account it created (FR-031).</summary>
public enum DemoCustomHabitsOutcome
{
    Added,
}

/// <summary>
/// The custom-habit result of a demo account; its text names no custom habit (FR-031, ADR 0013). Only an account
/// that demo loading created in the run has one.
/// </summary>
public sealed class DemoCustomHabitsResult
{
    private DemoCustomHabitsResult(DemoCustomHabitsOutcome outcome, string message)
    {
        Outcome = outcome;
        Message = message;
    }

    public DemoCustomHabitsOutcome Outcome { get; }

    /// <summary>The spec's text, written to the operator log.</summary>
    public string Message { get; }

    internal static DemoCustomHabitsResult Added { get; } =
        new(DemoCustomHabitsOutcome.Added, OperatorTexts.DemoCustomHabitsAdded);
}
