using HT.Domain.Time;

namespace HT.Domain.Goals;

/// <summary>The settings of one settings version (FR-002, FR-011): the target and the version's named time zone.</summary>
public sealed record GoalSettings
{
    /// <summary>Takes a target and a named time zone; the zone must come from <see cref="NamedTimeZone.TryParse"/> or <see cref="NamedTimeZone.Utc"/>.</summary>
    public GoalSettings(GoalTarget target, NamedTimeZone timeZone)
    {
        ArgumentNullException.ThrowIfNull(target);
        if (timeZone.Id is null)
        {
            throw new ArgumentException("The time zone must be a named time zone.", nameof(timeZone));
        }

        Target = target;
        TimeZone = timeZone;
    }

    public GoalTarget Target { get; }

    public NamedTimeZone TimeZone { get; }
}
