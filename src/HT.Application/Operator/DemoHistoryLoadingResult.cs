namespace HT.Application.Operator;

/// <summary>The outcomes of <see cref="DemoHistoryService.LoadAsync"/>, in FR-030's check order.</summary>
public enum DemoHistoryLoadingOutcome
{
    NoPassword,
    PasswordRuleNotMet,
    Loaded,
}

/// <summary>
/// The result of demo history loading (FR-030, ADR 0005). A refusal has its text and no accounts; <c>Loaded</c> has
/// no text of its own and one entry, for <see cref="DemoHistoryService.DemoEmail"/>.
/// </summary>
public sealed class DemoHistoryLoadingResult
{
    private DemoHistoryLoadingResult(DemoHistoryLoadingOutcome outcome, string? message, IReadOnlyList<DemoHistoryAccountResult> accounts)
    {
        Outcome = outcome;
        Message = message;
        Accounts = accounts;
    }

    public DemoHistoryLoadingOutcome Outcome { get; }

    /// <summary>The spec's text for a refusal; null for <see cref="DemoHistoryLoadingOutcome.Loaded"/>.</summary>
    public string? Message { get; }

    /// <summary>One result, for the demo email, for <see cref="DemoHistoryLoadingOutcome.Loaded"/>; empty for a refusal.</summary>
    public IReadOnlyList<DemoHistoryAccountResult> Accounts { get; }

    internal static DemoHistoryLoadingResult NoPassword { get; } =
        new(DemoHistoryLoadingOutcome.NoPassword, OperatorTexts.DemoPasswordNotConfigured, []);

    internal static DemoHistoryLoadingResult PasswordRuleNotMet { get; } =
        new(DemoHistoryLoadingOutcome.PasswordRuleNotMet, OperatorTexts.DemoPasswordRule, []);

    internal static DemoHistoryLoadingResult Loaded(DemoHistoryAccountResult account) =>
        new(DemoHistoryLoadingOutcome.Loaded, null, [account]);
}
