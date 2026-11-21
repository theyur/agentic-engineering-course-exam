namespace HT.Application.Operator;

/// <summary>
/// One goal of demo pause loading (FR-031, research R-08), as it is passed to <see cref="IDemoPauseStore"/>: a slice 6
/// <see cref="DemoHistoryGoal"/> (the goal on a catalog habit, its records and its notes) beside its excusals and
/// pauses. Immutable. The store writes all of them only when the habit is in one of the goal's accepted states, and
/// leaves all of them out otherwise.
/// </summary>
public sealed class DemoPauseGoal
{
    /// <exception cref="ArgumentException">An excusal or a pause is null.</exception>
    public DemoPauseGoal(DemoHistoryGoal history, IEnumerable<DemoExcusal> excusals, IEnumerable<DemoGoalPause> pauses)
    {
        ArgumentNullException.ThrowIfNull(history);
        ArgumentNullException.ThrowIfNull(excusals);
        ArgumentNullException.ThrowIfNull(pauses);

        DemoExcusal[] excusalList = [.. excusals];
        DemoGoalPause[] pauseList = [.. pauses];
        if (excusalList.Any(e => e is null))
        {
            throw new ArgumentException("A demo pause goal has no null excusal.", nameof(excusals));
        }

        if (pauseList.Any(p => p is null))
        {
            throw new ArgumentException("A demo pause goal has no null pause.", nameof(pauses));
        }

        History = history;
        Excusals = excusalList;
        Pauses = pauseList;
    }

    /// <summary>The goal with its records and notes.</summary>
    public DemoHistoryGoal History { get; }

    /// <summary>The excusals, at most one per commitment date, in date order.</summary>
    public IReadOnlyList<DemoExcusal> Excusals { get; }

    /// <summary>The pauses, which never share a date, in date order.</summary>
    public IReadOnlyList<DemoGoalPause> Pauses { get; }
}
