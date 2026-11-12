namespace HT.Application.Operator;

/// <summary>The outcomes of <see cref="DemoDataService.LoadAsync"/>, in FR-016's check order.</summary>
public enum DemoLoadingOutcome
{
    NoPassword,
    PasswordRuleNotMet,
    Loaded,
}

/// <summary>
/// The result of demo loading (FR-016, ADR 0005). A refusal has its text and no accounts; <c>Loaded</c> has no
/// text of its own and one entry per demo email, in the order of <see cref="DemoDataService.DemoEmails"/>.
/// </summary>
public sealed class DemoLoadingResult
{
    private DemoLoadingResult(DemoLoadingOutcome outcome, string? message, IReadOnlyList<DemoAccountResult> accounts)
    {
        Outcome = outcome;
        Message = message;
        Accounts = accounts;
    }

    public DemoLoadingOutcome Outcome { get; }

    /// <summary>The spec's text for a refusal; null for <see cref="DemoLoadingOutcome.Loaded"/>.</summary>
    public string? Message { get; }

    /// <summary>One result per demo email for <see cref="DemoLoadingOutcome.Loaded"/>; empty for a refusal.</summary>
    public IReadOnlyList<DemoAccountResult> Accounts { get; }

    internal static DemoLoadingResult NoPassword { get; } =
        new(DemoLoadingOutcome.NoPassword, OperatorTexts.DemoPasswordNotConfigured, []);

    internal static DemoLoadingResult PasswordRuleNotMet { get; } =
        new(DemoLoadingOutcome.PasswordRuleNotMet, OperatorTexts.DemoPasswordRule, []);

    internal static DemoLoadingResult Loaded(IReadOnlyList<DemoAccountResult> accounts) =>
        new(DemoLoadingOutcome.Loaded, null, accounts);
}
