using HT.Domain.Goals;

namespace HT.Application.Operator;

/// <summary>
/// One settings version of a demo attempt (slice 8 FR-031, research R-09): the date it applies from and its target,
/// which has passed the Domain checks. The store writes it in the profile's time zone, as every demo goal's version is
/// written, so it carries no zone of its own. Two versions with the same values are equal.
/// </summary>
public sealed record DemoVersion
{
    public DemoVersion(DateOnly appliesFrom, GoalTarget target)
    {
        ArgumentNullException.ThrowIfNull(target);
        AppliesFrom = appliesFrom;
        Target = target;
    }

    /// <summary>The first date the version applies to.</summary>
    public DateOnly AppliesFrom { get; }

    public GoalTarget Target { get; }
}
