namespace HT.Domain.Goals;

/// <summary>
/// An attempt of a goal (FR-018; slice 8 FR-007, FR-012): its number, from 1, the date it begins and, once ended, its
/// <see cref="Ending"/>. Two attempts with the same values, the ending included, are equal.
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

    /// <summary>The attempt's ending; null while it is not ended.</summary>
    public GoalEnding? Ending { get; init; }
}
