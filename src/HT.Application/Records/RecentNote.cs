using HT.Domain.Evaluation;

namespace HT.Application.Records;

/// <summary>
/// One of a goal's recent notes (FR-008): its entry's first and last date (equal except for a week), the note, and
/// the entry's state at the supplied time.
/// </summary>
public sealed record RecentNote
{
    public required DateOnly FirstDate { get; init; }

    public required DateOnly LastDate { get; init; }

    public required string Note { get; init; }

    public required EvaluationState State { get; init; }
}
