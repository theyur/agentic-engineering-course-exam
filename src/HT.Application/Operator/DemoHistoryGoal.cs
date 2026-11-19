namespace HT.Application.Operator;

/// <summary>
/// One goal of demo history loading (FR-030, research R-08), as it is passed to <see cref="IDemoHistoryStore"/>: a
/// slice-4 <see cref="DemoGoal"/> on a catalog habit, with its records in date order and its notes. Immutable. The
/// store writes the goal, its records and its notes only when the habit is in one of the goal's accepted states, and
/// leaves all three out otherwise.
/// </summary>
public sealed class DemoHistoryGoal
{
    /// <exception cref="ArgumentException">The goal is not on a catalog habit, or a record or note is null.</exception>
    public DemoHistoryGoal(DemoGoal goal, IEnumerable<DemoRecord> records, IEnumerable<DemoNote> notes)
    {
        ArgumentNullException.ThrowIfNull(goal);
        ArgumentNullException.ThrowIfNull(records);
        ArgumentNullException.ThrowIfNull(notes);
        if (goal.CatalogHabitName is null)
        {
            throw new ArgumentException("A demo history goal is on a catalog habit.", nameof(goal));
        }

        DemoRecord[] recordList = [.. records];
        DemoNote[] noteList = [.. notes];
        if (recordList.Any(r => r is null))
        {
            throw new ArgumentException("A demo history goal has no null record.", nameof(records));
        }

        if (noteList.Any(n => n is null))
        {
            throw new ArgumentException("A demo history goal has no null note.", nameof(notes));
        }

        Goal = goal;
        Records = recordList;
        Notes = noteList;
    }

    /// <summary>The goal: its catalog habit, accepted states, target and start date.</summary>
    public DemoGoal Goal { get; }

    /// <summary>The records, in the order the store writes them: date order.</summary>
    public IReadOnlyList<DemoRecord> Records { get; }

    /// <summary>The notes, at most one per entry key.</summary>
    public IReadOnlyList<DemoNote> Notes { get; }
}
