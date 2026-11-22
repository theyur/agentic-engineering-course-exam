using System.Collections.ObjectModel;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Time;

namespace HT.Application.Operator;

/// <summary>
/// One goal of demo goal-change loading (slice 8 FR-031, research R-09), as it is passed to
/// <see cref="IDemoGoalChangeStore"/>: a goal on a catalog habit, named by its name and the catalog states it accepts,
/// with its attempts (each with its settings versions and, once ended, its ending), its records in date order and its
/// notes (slice 6's <see cref="DemoRecord"/> and <see cref="DemoNote"/>). It has no own name, so its name follows FR-004.
/// Immutable. The constructor checks the attempts against the <see cref="Goal"/> invariants, so a goal that the store
/// could write but no read could map is refused before anything is written. The store writes the goal with all of
/// them only when the habit is in one of the accepted states, and leaves all of them out otherwise.
/// </summary>
public sealed class DemoGoalChangeGoal
{
    /// <exception cref="ArgumentException">
    /// The habit name is blank, no accepted state is given or one is not defined, an attempt, record or note is null,
    /// or the attempts fail an invariant of <see cref="Goal"/> (numbered 1, 2, … with every attempt but the latest
    /// ended, versions in date order with one schedule kind, a later attempt after the previous ending date, no version
    /// after an ended attempt's last date but its first). The messages name no value.
    /// </exception>
    public DemoGoalChangeGoal(
        string catalogHabitName,
        IEnumerable<CatalogHabitState> acceptedStates,
        IEnumerable<DemoAttempt> attempts,
        IEnumerable<DemoRecord> records,
        IEnumerable<DemoNote> notes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(catalogHabitName);
        ArgumentNullException.ThrowIfNull(acceptedStates);
        ArgumentNullException.ThrowIfNull(attempts);
        ArgumentNullException.ThrowIfNull(records);
        ArgumentNullException.ThrowIfNull(notes);

        var states = acceptedStates.ToHashSet();
        if (states.Count == 0 || !states.All(Enum.IsDefined))
        {
            throw new ArgumentException("A demo goal-change goal needs at least one defined catalog habit state.", nameof(acceptedStates));
        }

        DemoAttempt[] attemptList = [.. attempts];
        DemoRecord[] recordList = [.. records];
        DemoNote[] noteList = [.. notes];
        if (attemptList.Any(a => a is null))
        {
            throw new ArgumentException("A demo goal-change goal has no null attempt.", nameof(attempts));
        }

        if (recordList.Any(r => r is null))
        {
            throw new ArgumentException("A demo goal-change goal has no null record.", nameof(records));
        }

        if (noteList.Any(n => n is null))
        {
            throw new ArgumentException("A demo goal-change goal has no null note.", nameof(notes));
        }

        CatalogHabitName = catalogHabitName;
        AcceptedStates = new ReadOnlySet<CatalogHabitState>(states);
        Attempts = attemptList;
        Records = recordList;
        Notes = noteList;

        // The zone does not take part in any invariant, so UTC stands in for the profile's zone of the write.
        try
        {
            _ = new Goal(
                new GoalId(Guid.Empty),
                new AccountId(Guid.Empty),
                new GoalHabit(new HabitId(Guid.Empty), HabitKind.Catalog),
                null,
                AttemptsIn(NamedTimeZone.Utc));
        }
        catch (ArgumentException)
        {
            throw new ArgumentException("The attempts of a demo goal-change goal fail the goal invariants.", nameof(attempts));
        }
    }

    /// <summary>The name of the catalog habit, found by its name key (slice 2 FR-004).</summary>
    public string CatalogHabitName { get; }

    /// <summary>The catalog habit's states for which the goal is written.</summary>
    public IReadOnlySet<CatalogHabitState> AcceptedStates { get; }

    /// <summary>The attempts, in number order.</summary>
    public IReadOnlyList<DemoAttempt> Attempts { get; }

    /// <summary>The records, in the order the store writes them: date order.</summary>
    public IReadOnlyList<DemoRecord> Records { get; }

    /// <summary>The notes, at most one per entry key.</summary>
    public IReadOnlyList<DemoNote> Notes { get; }

    /// <summary>The goal's start date: attempt 1's first version's date.</summary>
    public DateOnly StartDate => Attempts[0].StartDate;

    /// <summary>
    /// The attempts as the <see cref="Goal"/> constructor takes them, every version in <paramref name="zone"/>: the
    /// store writes them in the profile's zone, and the unit tests build the goal from them.
    /// </summary>
    public IReadOnlyList<GoalAttemptVersions> AttemptsIn(NamedTimeZone zone) =>
    [
        .. Attempts.Select(a => new GoalAttemptVersions(
            new GoalAttempt(a.Number, a.StartDate) { Ending = a.Ending },
            [.. a.Versions.Select(v => new GoalSettingsVersion(new GoalSettings(v.Target, zone), v.AppliesFrom))])),
    ];
}
