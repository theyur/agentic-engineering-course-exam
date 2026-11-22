using HT.Domain.Goals;

namespace HT.Application.Operator;

/// <summary>
/// One attempt of a demo goal-change goal (slice 8 FR-031, research R-09): its number, its settings versions in date
/// order and, once ended, its ending (date, outcome and optional note), stored directly as facts (FR-026). The attempt
/// starts on its first version's date. Immutable; <see cref="DemoGoalChangeGoal"/> checks the attempts of one goal
/// together against the <see cref="Goal"/> invariants.
/// </summary>
public sealed class DemoAttempt
{
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="number"/> is below 1.</exception>
    /// <exception cref="ArgumentException">There is no version, or a version is null.</exception>
    public DemoAttempt(int number, IEnumerable<DemoVersion> versions, GoalEnding? ending)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(number, GoalAttempt.First);
        ArgumentNullException.ThrowIfNull(versions);
        DemoVersion[] list = [.. versions];
        if (list.Length == 0)
        {
            throw new ArgumentException("A demo attempt has at least one settings version.", nameof(versions));
        }

        if (list.Any(v => v is null))
        {
            throw new ArgumentException("A demo attempt has no null settings version.", nameof(versions));
        }

        Number = number;
        Versions = list;
        Ending = ending;
    }

    /// <summary>The attempt's number: 1, 2, … in the goal's order.</summary>
    public int Number { get; }

    /// <summary>The settings versions, in date order.</summary>
    public IReadOnlyList<DemoVersion> Versions { get; }

    /// <summary>The ending; null while the attempt is not ended.</summary>
    public GoalEnding? Ending { get; }

    /// <summary>The date the attempt starts: its first version's date.</summary>
    public DateOnly StartDate => Versions[0].AppliesFrom;
}
