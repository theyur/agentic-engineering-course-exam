namespace HT.Application.Operator;

/// <summary>The outcomes of <see cref="DemoReportService.LoadAsync"/>, in FR-032's check order.</summary>
public enum DemoReportLoadingOutcome
{
    NoPassword,
    PasswordRuleNotMet,
    Loaded,
}

/// <summary>
/// The result of demo report loading (FR-032, ADR 0021). A refusal has its text and no accounts; <c>Loaded</c> has
/// no text of its own and one entry, for <see cref="DemoReportService.DemoEmail"/>.
/// </summary>
public sealed class DemoReportLoadingResult
{
    private DemoReportLoadingResult(DemoReportLoadingOutcome outcome, string? message, IReadOnlyList<DemoReportAccountResult> accounts)
    {
        Outcome = outcome;
        Message = message;
        Accounts = accounts;
    }

    public DemoReportLoadingOutcome Outcome { get; }

    /// <summary>The spec's text for a refusal; null for <see cref="DemoReportLoadingOutcome.Loaded"/>.</summary>
    public string? Message { get; }

    /// <summary>One result, for the demo email, for <see cref="DemoReportLoadingOutcome.Loaded"/>; empty for a refusal.</summary>
    public IReadOnlyList<DemoReportAccountResult> Accounts { get; }

    internal static DemoReportLoadingResult NoPassword { get; } =
        new(DemoReportLoadingOutcome.NoPassword, OperatorTexts.DemoPasswordNotConfigured, []);

    internal static DemoReportLoadingResult PasswordRuleNotMet { get; } =
        new(DemoReportLoadingOutcome.PasswordRuleNotMet, OperatorTexts.DemoPasswordRule, []);

    internal static DemoReportLoadingResult Loaded(DemoReportAccountResult account) =>
        new(DemoReportLoadingOutcome.Loaded, null, [account]);
}
