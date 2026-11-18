namespace HT.Domain.Goals;

/// <summary>
/// How a goal is evaluated (FR-003): fixed days are commitment-based (each scheduled day is one commitment); N times
/// per day or per week is period-based (each day or week is one evaluation period).
/// </summary>
public enum GoalScheduleKind
{
    CommitmentBased,
    PeriodBased,
}
