using HT.Domain.Accounts;
using HT.Domain.Habits;

namespace HT.Domain.Goals;

/// <summary>
/// A personal goal (FR-001, FR-002; slice 8 research R-01): its owner, its habit, its start date, an optional own name
/// and its attempts, each with its own settings versions and, once ended, its ending. The habit, the start date and the
/// name belong to the goal, not to an attempt or a version. Immutable; the constructors enforce the invariants of
/// data-model.md, so the store and the use cases cannot build an inconsistent goal. No persistence detail: creation
/// order is the order of the store's lists.
/// </summary>
public sealed class Goal
{
    private readonly GoalAttemptVersions[] _attempts;

    /// <summary>
    /// Builds a goal with the one attempt <paramref name="attempt"/> and its <paramref name="versions"/> (slice 4), and
    /// checks the invariants of attempt 1:
    /// <list type="bullet">
    ///   <item><paramref name="versions"/> is non-empty, the first applies from <paramref name="startDate"/>, and the dates strictly increase;</item>
    ///   <item>all versions have the same schedule kind (<see cref="GoalTarget.HasSameScheduleKind"/>; slice 8 D-1);</item>
    ///   <item>a later version of a per-week goal applies from a Monday, where its next week starts (FR-026);</item>
    ///   <item>the attempt does not begin before the start date (FR-018);</item>
    ///   <item>an ended attempt has no version after its last date except its first (slice 8 FR-009).</item>
    /// </list>
    /// </summary>
    /// <exception cref="ArgumentException">An invariant does not hold.</exception>
    public Goal(
        GoalId id,
        AccountId owner,
        GoalHabit habit,
        DateOnly startDate,
        GoalName? ownName,
        GoalAttempt attempt,
        IReadOnlyList<GoalSettingsVersion> versions)
    {
        ArgumentNullException.ThrowIfNull(attempt);
        ArgumentNullException.ThrowIfNull(versions);
        CheckHabit(habit);
        if (attempt.StartDate < startDate)
        {
            throw new ArgumentException("The attempt cannot begin before the goal's start date.", nameof(attempt));
        }

        var copy = versions.ToArray();
        CheckVersions(copy, startDate, nameof(versions));
        CheckEnding(attempt, copy, nameof(versions));

        Id = id;
        Owner = owner;
        Habit = habit;
        StartDate = startDate;
        OwnName = ownName;
        _attempts = [new GoalAttemptVersions(attempt, copy)];
    }

    /// <summary>
    /// Builds a goal from its attempts (slice 8 research R-01) and checks the invariants of data-model.md:
    /// <list type="bullet">
    ///   <item>at least one attempt, numbered 1, 2, … in order; every attempt but the last has an ending;</item>
    ///   <item>each attempt has at least one version, and its dates strictly increase; attempt 1's first version's
    ///   date is the goal's start date, and attempt 1 does not begin before it; every later attempt's first version
    ///   applies from that attempt's start date;</item>
    ///   <item>all versions of one attempt have the same schedule kind (D-1), and a later version of a per-week
    ///   attempt applies from a Monday;</item>
    ///   <item>attempt n + 1 starts after attempt n's ending date (FR-012);</item>
    ///   <item>an ended attempt has no version after its last date (<see cref="GoalTimeline.LastDate"/>) except its
    ///   first (FR-009);</item>
    ///   <item>the habit kind is defined.</item>
    /// </list>
    /// The messages name no value.
    /// </summary>
    /// <exception cref="ArgumentException">An invariant does not hold.</exception>
    public Goal(GoalId id, AccountId owner, GoalHabit habit, GoalName? ownName, IReadOnlyList<GoalAttemptVersions> attempts)
    {
        ArgumentNullException.ThrowIfNull(attempts);
        CheckHabit(habit);
        if (attempts.Count == 0)
        {
            throw new ArgumentException("A goal has at least one attempt.", nameof(attempts));
        }

        var copy = new GoalAttemptVersions[attempts.Count];
        for (var i = 0; i < copy.Length; i++)
        {
            copy[i] = CheckAttempt(attempts[i], i, i == 0 ? null : copy[i - 1].Attempt, nameof(attempts));
        }

        Id = id;
        Owner = owner;
        Habit = habit;
        StartDate = copy[0].Versions[0].AppliesFrom;
        OwnName = ownName;
        _attempts = copy;
    }

    public GoalId Id { get; }

    public AccountId Owner { get; }

    public GoalHabit Habit { get; }

    /// <summary>
    /// The goal's start date (FR-001): attempt 1's first version's date. Not the first date of a later attempt, which
    /// can be earlier (slice 8 AC-035); a reader that means an attempt's first date uses that attempt's.
    /// </summary>
    public DateOnly StartDate { get; }

    /// <summary>The goal's own name; null when it is named by its habit and target (FR-010).</summary>
    public GoalName? OwnName { get; }

    /// <summary>The latest attempt.</summary>
    public GoalAttempt Attempt => _attempts[^1].Attempt;

    /// <summary>
    /// The latest attempt's settings versions, ordered by <see cref="GoalSettingsVersion.AppliesFrom"/>. For attempt 1
    /// the first applies from <see cref="StartDate"/>; for a later attempt from its own start date.
    /// </summary>
    public IReadOnlyList<GoalSettingsVersion> Versions => _attempts[^1].Versions;

    /// <summary>Every attempt with its own settings versions, in number order (slice 8 research R-01).</summary>
    public IReadOnlyList<GoalAttemptVersions> Attempts => _attempts;

    /// <summary>Whether the latest attempt has an ending (slice 8 FR-007).</summary>
    public bool IsEnded => Attempt.Ending is not null;

    /// <summary>The settings versions of attempt <paramref name="number"/>, in date order.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The goal has no attempt with that number.</exception>
    public IReadOnlyList<GoalSettingsVersion> VersionsOf(int number) => AttemptAt(number).Versions;

    /// <summary>The attempt with its versions; the attempts are numbered 1, 2, … in order.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The goal has no attempt with that number.</exception>
    internal GoalAttemptVersions AttemptAt(int number) =>
        number >= GoalAttempt.First && number <= _attempts.Length
            ? _attempts[number - 1]
            : throw new ArgumentOutOfRangeException(nameof(number), "The goal has no attempt with this number.");

    /// <summary>The same goal with other settings versions of its latest attempt, checked again.</summary>
    internal Goal WithVersions(IReadOnlyList<GoalSettingsVersion> versions) =>
        new(Id, Owner, Habit, OwnName, [.. _attempts[..^1], new GoalAttemptVersions(Attempt, versions)]);

    /// <summary>The same goal with another own name (null for none), checked again.</summary>
    internal Goal WithName(GoalName? ownName) => new(Id, Owner, Habit, ownName, _attempts);

    /// <summary>The same goal with its latest attempt ended by <paramref name="ending"/>, keeping <paramref name="keptVersions"/>, checked again.</summary>
    internal Goal WithEnding(GoalEnding ending, IReadOnlyList<GoalSettingsVersion> keptVersions) =>
        new(Id, Owner, Habit, OwnName, [.. _attempts[..^1], new GoalAttemptVersions(Attempt with { Ending = ending }, keptVersions)]);

    /// <summary>The same goal with a new latest attempt and its first version, checked again.</summary>
    internal Goal WithAttempt(GoalAttempt attempt, GoalSettingsVersion first) =>
        new(Id, Owner, Habit, OwnName, [.. _attempts, new GoalAttemptVersions(attempt, [first])]);

    private static void CheckHabit(GoalHabit habit)
    {
        if (!Enum.IsDefined(habit.Kind))
        {
            throw new ArgumentException("The habit kind is not defined.", nameof(habit));
        }
    }

    /// <summary>The checks of one attempt of the attempts constructor; <paramref name="previous"/> is null for attempt 1.</summary>
    private static GoalAttemptVersions CheckAttempt(GoalAttemptVersions? item, int index, GoalAttempt? previous, string attempts)
    {
        if (item?.Attempt is not { } attempt || item.Versions is null)
        {
            throw new ArgumentException("An attempt and its settings versions cannot be null.", attempts);
        }

        if (attempt.Number != index + 1)
        {
            throw new ArgumentException("The attempts are numbered 1, 2, … in order.", attempts);
        }

        if (previous is not null)
        {
            if (previous.Ending is not { } ending)
            {
                throw new ArgumentException("Every attempt but the latest is ended.", attempts);
            }

            if (attempt.StartDate <= ending.Date)
            {
                throw new ArgumentException("An attempt starts after the previous attempt's ending date.", attempts);
            }
        }

        // Attempt 1's first version gives the goal its start date; a later attempt's applies from its own start date.
        var versions = item.Versions.ToArray();
        CheckVersions(versions, previous is null ? null : attempt.StartDate, attempts);
        if (attempt.StartDate < versions[0].AppliesFrom)
        {
            throw new ArgumentException("The attempt cannot begin before the goal's start date.", attempts);
        }

        CheckEnding(attempt, versions, attempts);
        return new GoalAttemptVersions(attempt, versions);
    }

    /// <summary>The version checks of one attempt; with no <paramref name="startDate"/>, the first version's own date is the start.</summary>
    private static void CheckVersions(GoalSettingsVersion[] versions, DateOnly? startDate, string paramName)
    {
        if (versions.Length == 0)
        {
            throw new ArgumentException("A goal has at least one settings version.", paramName);
        }

        if (Array.Exists(versions, version => version is null))
        {
            throw new ArgumentException("A settings version cannot be null.", paramName);
        }

        if (startDate is { } start && versions[0].AppliesFrom != start)
        {
            throw new ArgumentException("The first settings version applies from the start date.", paramName);
        }

        var target = versions[0].Settings.Target;
        var perWeek = target.PeriodLength == SchedulePeriod.Week;
        for (var i = 1; i < versions.Length; i++)
        {
            if (versions[i].AppliesFrom <= versions[i - 1].AppliesFrom)
            {
                throw new ArgumentException("The settings versions' dates must strictly increase.", paramName);
            }

            if (!versions[i].Settings.Target.HasSameScheduleKind(target))
            {
                throw new ArgumentException("The settings versions of an attempt keep one schedule kind.", paramName);
            }

            if (perWeek && versions[i].AppliesFrom.DayOfWeek != DayOfWeek.Monday)
            {
                throw new ArgumentException("A later version of a per-week goal applies from a Monday.", paramName);
            }
        }
    }

    /// <summary>An ended attempt keeps no version after its last date except its first (FR-009).</summary>
    private static void CheckEnding(GoalAttempt attempt, GoalSettingsVersion[] versions, string paramName)
    {
        if (attempt.Ending is not { } ending)
        {
            return;
        }

        var last = GoalTimeline.LastDateOf(versions[0].Settings.Target, ending.Date);
        for (var i = 1; i < versions.Length; i++)
        {
            if (versions[i].AppliesFrom > last)
            {
                throw new ArgumentException("An ended attempt has no settings version after its last date except its first.", paramName);
            }
        }
    }
}
