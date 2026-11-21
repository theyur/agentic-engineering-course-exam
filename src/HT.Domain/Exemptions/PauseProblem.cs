namespace HT.Domain.Exemptions;

/// <summary>Why a pause's days are refused (FR-006 steps 4 to 7), in the order <see cref="PauseRules.Check"/> checks them.</summary>
public enum PauseProblem
{
    /// <summary>The first or the last day is missing (step 4).</summary>
    DaysMissing,

    /// <summary>The first day is before the pause date (step 5).</summary>
    StartsBeforeToday,

    /// <summary>The last day is before the first day (step 6).</summary>
    LastBeforeFirst,

    /// <summary>The range shares a date with another pause of the goal (step 7).</summary>
    Overlaps,
}
