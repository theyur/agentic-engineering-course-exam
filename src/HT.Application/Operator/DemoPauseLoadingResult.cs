namespace HT.Application.Operator;

/// <summary>The outcomes of <see cref="DemoPauseService.LoadAsync"/>, in FR-031's check order.</summary>
public enum DemoPauseLoadingOutcome
{
    NoPassword,
    PasswordRuleNotMet,
    Loaded,
}

/// <summary>
/// The result of demo pause loading (FR-031, ADR 0023). A refusal has its text and no accounts; <c>Loaded</c> has no
/// text of its own and one entry, for <see cref="DemoPauseService.DemoEmail"/>.
/// </summary>
public sealed class DemoPauseLoadingResult
{
    private DemoPauseLoadingResult(DemoPauseLoadingOutcome outcome, string? message, IReadOnlyList<DemoPauseAccountResult> accounts)
    {
        Outcome = outcome;
        Message = message;
        Accounts = accounts;
    }

    public DemoPauseLoadingOutcome Outcome { get; }

    /// <summary>The spec's text for a refusal; null for <see cref="DemoPauseLoadingOutcome.Loaded"/>.</summary>
    public string? Message { get; }

    /// <summary>One result, for the demo email, for <see cref="DemoPauseLoadingOutcome.Loaded"/>; empty for a refusal.</summary>
    public IReadOnlyList<DemoPauseAccountResult> Accounts { get; }

    internal static DemoPauseLoadingResult NoPassword { get; } =
        new(DemoPauseLoadingOutcome.NoPassword, OperatorTexts.DemoPasswordNotConfigured, []);

    internal static DemoPauseLoadingResult PasswordRuleNotMet { get; } =
        new(DemoPauseLoadingOutcome.PasswordRuleNotMet, OperatorTexts.DemoPasswordRule, []);

    internal static DemoPauseLoadingResult Loaded(DemoPauseAccountResult account) =>
        new(DemoPauseLoadingOutcome.Loaded, null, [account]);
}
