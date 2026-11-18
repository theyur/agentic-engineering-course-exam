namespace HT.Domain.Goals;

/// <summary>
/// One settings version of a goal (FR-005): its settings and the date from which they apply. A later version adds to
/// the history instead of overwriting it, so every date is evaluated against the version in effect for it.
/// </summary>
public sealed record GoalSettingsVersion
{
    public GoalSettingsVersion(GoalSettings settings, DateOnly appliesFrom)
    {
        ArgumentNullException.ThrowIfNull(settings);
        Settings = settings;
        AppliesFrom = appliesFrom;
    }

    public GoalSettings Settings { get; }

    public DateOnly AppliesFrom { get; }
}
