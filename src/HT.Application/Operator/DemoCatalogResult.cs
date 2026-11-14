namespace HT.Application.Operator;

/// <summary>What demo loading did with the seed catalog (FR-030).</summary>
public enum DemoCatalogOutcome
{
    SeedCatalogLoaded,
    CatalogNotEmpty,
}

/// <summary>The catalog result of demo loading; every outcome has its text (FR-030, ADR 0005).</summary>
public sealed class DemoCatalogResult
{
    private DemoCatalogResult(DemoCatalogOutcome outcome, string message)
    {
        Outcome = outcome;
        Message = message;
    }

    public DemoCatalogOutcome Outcome { get; }

    /// <summary>The spec's text, written to the operator log.</summary>
    public string Message { get; }

    internal static DemoCatalogResult SeedCatalogLoaded { get; } =
        new(DemoCatalogOutcome.SeedCatalogLoaded, OperatorTexts.SeedCatalogLoaded);

    internal static DemoCatalogResult CatalogNotEmpty { get; } =
        new(DemoCatalogOutcome.CatalogNotEmpty, OperatorTexts.CatalogNotEmpty);
}
