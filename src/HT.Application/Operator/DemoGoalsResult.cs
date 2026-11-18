namespace HT.Application.Operator;

/// <summary>What demo loading did with the goals of a demo account it created (FR-040).</summary>
public enum DemoGoalsOutcome
{
    Added,
}

/// <summary>
/// The goal result of a demo account; its text names no goal, habit, account name or setting (FR-040, HP-013,
/// ADR 0013). Only an account that demo loading created in the run has one.
/// </summary>
public sealed class DemoGoalsResult
{
    private DemoGoalsResult(DemoGoalsOutcome outcome, string message)
    {
        Outcome = outcome;
        Message = message;
    }

    public DemoGoalsOutcome Outcome { get; }

    /// <summary>The spec's text, written to the operator log.</summary>
    public string Message { get; }

    internal static DemoGoalsResult Added { get; } = new(DemoGoalsOutcome.Added, OperatorTexts.DemoGoalsAdded);
}
