namespace HT.Domain.Goals;

/// <summary>
/// The stored fact of an attempt's ending (FR-007, FR-026): the ending date E, the <see cref="Goals.Outcome"/> and the
/// optional <see cref="EndingNote"/>. The last date is derived from it (<see cref="GoalTimeline.LastDate"/>), never
/// stored. Two endings with the same values are equal.
/// </summary>
public sealed record GoalEnding
{
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="outcome"/> is not defined.</exception>
    public GoalEnding(DateOnly date, Outcome outcome, EndingNote? note)
    {
        if (!Enum.IsDefined(outcome))
        {
            throw new ArgumentOutOfRangeException(nameof(outcome), "The outcome is not defined.");
        }

        Date = date;
        Outcome = outcome;
        Note = note;
    }

    /// <summary>The ending date E: the goal's current date when it was ended, or the account's today for a goal that had not started.</summary>
    public DateOnly Date { get; }

    public Outcome Outcome { get; }

    /// <summary>The ending note; null when there is none.</summary>
    public EndingNote? Note { get; }
}
