namespace HT.Domain.Evaluation;

/// <summary>
/// The progress of a commitment or evaluation period (FR-014, research R-01). <paramref name="Value"/> is null only
/// for a commitment without a record; a period without records has 0. <paramref name="Of"/> is the amount (in
/// minutes for a duration in hours), 1 for a yes/no commitment, or N for a period counted in records.
/// </summary>
public sealed record Progress(ProgressKind Kind, int? Value, int Of);
