using HT.Domain.Goals;

namespace HT.Application.Goals;

/// <summary>
/// A new goal as the New goal form sends it (data-model.md, "Application inputs and views"); every value may be
/// missing, so every refusal of FR-006 can be expressed.
/// </summary>
public sealed record GoalInput
{
    /// <summary>The chosen habit; null when none is chosen.</summary>
    public GoalHabit? Habit { get; init; }

    /// <summary>The target fields, one nullable value per control (research R-05).</summary>
    public GoalTargetFields Target { get; init; } = new();

    public DateOnly? StartDate { get; init; }

    /// <summary>The goal's own name; null, empty or only spaces give no own name (FR-009, FR-010).</summary>
    public string? Name { get; init; }
}
