namespace HT.Domain.Goals;

/// <summary>What a change of the owner's time zone does to one goal's settings versions (FR-026).</summary>
public enum GoalVersionChangeKind
{
    /// <summary>Nothing: the new zone is already the zone of every version from the current date on.</summary>
    None,

    /// <summary>The goal has no current date: its first (and only) version gets the new zone.</summary>
    ReplaceFirst,

    /// <summary>The version after the current date (the goal's last) gets the new zone and the date E.</summary>
    ReplacePending,

    /// <summary>A new version, with the new zone and all other settings unchanged, applies from the date E.</summary>
    Add,
}
