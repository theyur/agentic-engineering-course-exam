using HT.Domain.Exemptions;

namespace HT.Application.Operator;

/// <summary>
/// One pause of a demo pause goal (FR-031, research R-08): its first and last day, both included, and its reason or
/// none, stored directly as the fact of FR-025. <see cref="ExemptionReason"/> does not override
/// <see cref="object.ToString"/>, so the pause's text never shows the reason (HP-009).
/// </summary>
public sealed record DemoGoalPause
{
    /// <exception cref="ArgumentException"><paramref name="lastDay"/> is before <paramref name="firstDay"/>.</exception>
    public DemoGoalPause(DateOnly firstDay, DateOnly lastDay, ExemptionReason? reason)
    {
        if (lastDay < firstDay)
        {
            throw new ArgumentException("A demo pause cannot end before it starts.", nameof(lastDay));
        }

        FirstDay = firstDay;
        LastDay = lastDay;
        Reason = reason;
    }

    /// <summary>The first paused day.</summary>
    public DateOnly FirstDay { get; }

    /// <summary>The last paused day; never before <see cref="FirstDay"/>.</summary>
    public DateOnly LastDay { get; }

    /// <summary>The reason; null means none.</summary>
    public ExemptionReason? Reason { get; }
}
