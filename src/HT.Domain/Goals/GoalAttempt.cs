namespace HT.Domain.Goals;

/// <summary>
/// An attempt of a goal (FR-018): its number, from 1, and the date it begins. This slice has only attempt 1, beginning
/// on the goal's start date and not ended.
/// </summary>
public sealed record GoalAttempt
{
    public const int First = 1;

    public GoalAttempt(int number, DateOnly startDate)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(number, First);
        Number = number;
        StartDate = startDate;
    }

    public int Number { get; }

    public DateOnly StartDate { get; }
}
