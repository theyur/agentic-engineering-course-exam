using HT.Domain.Goals;

namespace HT.Application.Goals;

/// <summary>
/// A settings change as the Edit settings page sends it (slice 8 FR-001, data-model.md "Inputs"): the target fields and
/// the name of goal creation, without the habit or the start date, which never change. Every value may be missing, so
/// every refusal of FR-003 can be expressed.
/// </summary>
public sealed record GoalSettingsInput
{
    /// <summary>The target fields, one nullable value per control, as for goal creation (slice 4 research R-05).</summary>
    public GoalTargetFields Target { get; init; } = new();

    /// <summary>The goal's own name; null, empty or only spaces give no own name (slice 4 FR-009, slice 8 FR-004).</summary>
    public string? Name { get; init; }
}
