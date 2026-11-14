namespace HT.Application.Operator;

/// <summary>The outcomes of <see cref="DemoDataService.LoadAsync"/>, in FR-016's check order.</summary>
public enum DemoLoadingOutcome
{
    NoPassword,
    PasswordRuleNotMet,
    Loaded,
}

/// <summary>
/// The result of demo loading (FR-016, FR-030, ADR 0005). A refusal has its text, no accounts and no catalog result;
/// <c>Loaded</c> has no text of its own, one entry per demo email, in the order of
/// <see cref="DemoDataService.DemoEmails"/>, and the catalog result.
/// </summary>
public sealed class DemoLoadingResult
{
    private DemoLoadingResult(
        DemoLoadingOutcome outcome, string? message, IReadOnlyList<DemoAccountResult> accounts, DemoCatalogResult? catalog)
    {
        Outcome = outcome;
        Message = message;
        Accounts = accounts;
        Catalog = catalog;
    }

    public DemoLoadingOutcome Outcome { get; }

    /// <summary>The spec's text for a refusal; null for <see cref="DemoLoadingOutcome.Loaded"/>.</summary>
    public string? Message { get; }

    /// <summary>One result per demo email for <see cref="DemoLoadingOutcome.Loaded"/>; empty for a refusal.</summary>
    public IReadOnlyList<DemoAccountResult> Accounts { get; }

    /// <summary>The seed catalog's result for <see cref="DemoLoadingOutcome.Loaded"/> (FR-030); null for a refusal.</summary>
    public DemoCatalogResult? Catalog { get; }

    internal static DemoLoadingResult NoPassword { get; } =
        new(DemoLoadingOutcome.NoPassword, OperatorTexts.DemoPasswordNotConfigured, [], null);

    internal static DemoLoadingResult PasswordRuleNotMet { get; } =
        new(DemoLoadingOutcome.PasswordRuleNotMet, OperatorTexts.DemoPasswordRule, [], null);

    internal static DemoLoadingResult Loaded(IReadOnlyList<DemoAccountResult> accounts, DemoCatalogResult catalog) =>
        new(DemoLoadingOutcome.Loaded, null, accounts, catalog);
}
