using HT.Domain.Time;

namespace HT.Domain.Goals;

/// <summary>
/// The effect of a change of the owner's time zone on one goal (FR-026), as <see cref="GoalTimeline.ChangeOf"/> decides
/// it: <see cref="None"/>, <see cref="ReplaceFirst"/>, <see cref="ReplacePending"/> or <see cref="Add"/>. Two changes
/// with the same values are equal.
/// </summary>
public sealed record GoalVersionChange
{
    private GoalVersionChange(GoalVersionChangeKind kind, NamedTimeZone? timeZone, DateOnly? appliesFrom)
    {
        Kind = kind;
        TimeZone = timeZone;
        AppliesFrom = appliesFrom;
    }

    /// <summary>Nothing changes.</summary>
    public static GoalVersionChange None { get; } = new(GoalVersionChangeKind.None, null, null);

    public GoalVersionChangeKind Kind { get; }

    /// <summary>The new zone; null for <see cref="GoalVersionChangeKind.None"/>.</summary>
    public NamedTimeZone? TimeZone { get; }

    /// <summary>The date E from which the changed or added version applies; null for None and ReplaceFirst.</summary>
    public DateOnly? AppliesFrom { get; }

    /// <summary>The goal has no current date: its first version gets <paramref name="zone"/>.</summary>
    public static GoalVersionChange ReplaceFirst(NamedTimeZone zone) =>
        new(GoalVersionChangeKind.ReplaceFirst, Named(zone), null);

    /// <summary>The goal's version after its current date (its last version) gets <paramref name="zone"/> and <paramref name="appliesFrom"/>.</summary>
    public static GoalVersionChange ReplacePending(NamedTimeZone zone, DateOnly appliesFrom) =>
        new(GoalVersionChangeKind.ReplacePending, Named(zone), appliesFrom);

    /// <summary>A new version with <paramref name="zone"/> and the other settings unchanged applies from <paramref name="appliesFrom"/>.</summary>
    public static GoalVersionChange Add(DateOnly appliesFrom, NamedTimeZone zone) =>
        new(GoalVersionChangeKind.Add, Named(zone), appliesFrom);

    /// <summary>
    /// The goal with this change applied: ReplaceFirst changes the first version's zone, ReplacePending the last
    /// version's zone and date, and Add appends a version with the last version's target. The result is checked by
    /// the <see cref="Goal"/> constructor.
    /// </summary>
    public Goal ApplyTo(Goal goal)
    {
        ArgumentNullException.ThrowIfNull(goal);
        var versions = goal.Versions.ToList();
        switch (Kind)
        {
            case GoalVersionChangeKind.None:
                return goal;
            case GoalVersionChangeKind.ReplaceFirst:
                versions[0] = new GoalSettingsVersion(new GoalSettings(versions[0].Settings.Target, TimeZone!.Value), versions[0].AppliesFrom);
                break;
            case GoalVersionChangeKind.ReplacePending:
                if (versions.Count < 2)
                {
                    throw new InvalidOperationException("The goal has no version after its first to replace.");
                }

                versions[^1] = new GoalSettingsVersion(new GoalSettings(versions[^1].Settings.Target, TimeZone!.Value), AppliesFrom!.Value);
                break;
            default:
                versions.Add(new GoalSettingsVersion(new GoalSettings(versions[^1].Settings.Target, TimeZone!.Value), AppliesFrom!.Value));
                break;
        }

        return goal.WithVersions(versions);
    }

    private static NamedTimeZone Named(NamedTimeZone zone) =>
        zone.Id is null ? throw new ArgumentException("The time zone must be a named time zone.", nameof(zone)) : zone;
}
