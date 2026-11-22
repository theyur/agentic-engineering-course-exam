namespace HT.Application.Operator;

/// <summary>The outcomes of <see cref="DemoGoalChangeService.LoadAsync"/>, in FR-031's check order.</summary>
public enum DemoGoalChangeLoadingOutcome
{
    NoPassword,
    PasswordRuleNotMet,
    Loaded,
}

/// <summary>
/// The result of demo goal-change loading (slice 8 FR-031, ADR 0026). A refusal has its text and no accounts; <c>Loaded</c> has no
/// text of its own and one entry, for <see cref="DemoGoalChangeService.DemoEmail"/>.
/// </summary>
public sealed class DemoGoalChangeLoadingResult
{
    private DemoGoalChangeLoadingResult(DemoGoalChangeLoadingOutcome outcome, string? message, IReadOnlyList<DemoGoalChangeAccountResult> accounts)
    {
        Outcome = outcome;
        Message = message;
        Accounts = accounts;
    }

    public DemoGoalChangeLoadingOutcome Outcome { get; }

    /// <summary>The spec's text for a refusal; null for <see cref="DemoGoalChangeLoadingOutcome.Loaded"/>.</summary>
    public string? Message { get; }

    /// <summary>One result, for the demo email, for <see cref="DemoGoalChangeLoadingOutcome.Loaded"/>; empty for a refusal.</summary>
    public IReadOnlyList<DemoGoalChangeAccountResult> Accounts { get; }

    internal static DemoGoalChangeLoadingResult NoPassword { get; } =
        new(DemoGoalChangeLoadingOutcome.NoPassword, OperatorTexts.DemoPasswordNotConfigured, []);

    internal static DemoGoalChangeLoadingResult PasswordRuleNotMet { get; } =
        new(DemoGoalChangeLoadingOutcome.PasswordRuleNotMet, OperatorTexts.DemoPasswordRule, []);

    internal static DemoGoalChangeLoadingResult Loaded(DemoGoalChangeAccountResult account) =>
        new(DemoGoalChangeLoadingOutcome.Loaded, null, [account]);
}
