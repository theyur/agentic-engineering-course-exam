using HT.Domain.Goals;

namespace HT.Application.Goals;

/// <summary>
/// A new attempt as the Start new attempt page sends it (slice 8 FR-012, data-model.md "Inputs"): the target fields of
/// goal creation, of any schedule kind, and the start date. There is no habit and no name: the goal keeps both. Every
/// value may be missing, so every refusal of FR-012 can be expressed.
/// </summary>
public sealed record NewAttemptInput
{
    /// <summary>The target fields, one nullable value per control, as for goal creation (slice 4 research R-05).</summary>
    public GoalTargetFields Target { get; init; } = new();

    /// <summary>The new attempt's start date; null when none is chosen.</summary>
    public DateOnly? StartDate { get; init; }
}
