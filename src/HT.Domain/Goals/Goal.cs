using HT.Domain.Accounts;
using HT.Domain.Habits;

namespace HT.Domain.Goals;

/// <summary>
/// A personal goal (FR-001, FR-002): its owner, its habit, its start date, an optional own name, its attempt and its
/// settings versions. The habit, the start date and the name belong to the goal, not to a version. Immutable; the
/// constructor enforces the invariants of data-model.md, so the store and the use cases cannot build an
/// inconsistent goal. No persistence detail: creation order is the order of the store's lists.
/// </summary>
public sealed class Goal
{
    /// <summary>
    /// Builds a goal and checks its invariants:
    /// <list type="bullet">
    ///   <item><paramref name="versions"/> is non-empty, the first applies from <paramref name="startDate"/>, and the dates strictly increase;</item>
    ///   <item>only the time zone differs between versions in this slice (FR-005);</item>
    ///   <item>a later version of a per-week goal applies from a Monday, where its next week starts (FR-026);</item>
    ///   <item>the attempt does not begin before the start date (FR-018).</item>
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
        if (!Enum.IsDefined(habit.Kind))
        {
            throw new ArgumentException("The habit kind is not defined.", nameof(habit));
        }

        if (attempt.StartDate < startDate)
        {
            throw new ArgumentException("The attempt cannot begin before the goal's start date.", nameof(attempt));
        }

        var copy = versions.ToArray();
        CheckVersions(copy, startDate);

        Id = id;
        Owner = owner;
        Habit = habit;
        StartDate = startDate;
        OwnName = ownName;
        Attempt = attempt;
        Versions = copy;
    }

    public GoalId Id { get; }

    public AccountId Owner { get; }

    public GoalHabit Habit { get; }

    public DateOnly StartDate { get; }

    /// <summary>The goal's own name; null when it is named by its habit and target (FR-010).</summary>
    public GoalName? OwnName { get; }

    public GoalAttempt Attempt { get; }

    /// <summary>The settings versions, ordered by <see cref="GoalSettingsVersion.AppliesFrom"/>; the first applies from <see cref="StartDate"/>.</summary>
    public IReadOnlyList<GoalSettingsVersion> Versions { get; }

    /// <summary>The target, which every version of this slice shares (FR-005).</summary>
    internal GoalTarget Target => Versions[0].Settings.Target;

    /// <summary>The same goal with other settings versions, checked again.</summary>
    internal Goal WithVersions(IReadOnlyList<GoalSettingsVersion> versions) =>
        new(Id, Owner, Habit, StartDate, OwnName, Attempt, versions);

    private static void CheckVersions(GoalSettingsVersion[] versions, DateOnly startDate)
    {
        if (versions.Length == 0)
        {
            throw new ArgumentException("A goal has at least one settings version.", nameof(versions));
        }

        if (Array.Exists(versions, version => version is null))
        {
            throw new ArgumentException("A settings version cannot be null.", nameof(versions));
        }

        if (versions[0].AppliesFrom != startDate)
        {
            throw new ArgumentException("The first settings version applies from the start date.", nameof(versions));
        }

        var target = versions[0].Settings.Target;
        var perWeek = target.PeriodLength == SchedulePeriod.Week;
        for (var i = 1; i < versions.Length; i++)
        {
            if (versions[i].AppliesFrom <= versions[i - 1].AppliesFrom)
            {
                throw new ArgumentException("The settings versions' dates must strictly increase.", nameof(versions));
            }

            if (!versions[i].Settings.Target.Equals(target))
            {
                throw new ArgumentException("Only the time zone differs between settings versions.", nameof(versions));
            }

            if (perWeek && versions[i].AppliesFrom.DayOfWeek != DayOfWeek.Monday)
            {
                throw new ArgumentException("A later version of a per-week goal applies from a Monday.", nameof(versions));
            }
        }
    }
}
