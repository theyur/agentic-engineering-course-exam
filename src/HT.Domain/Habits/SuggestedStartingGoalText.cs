namespace HT.Domain.Habits;

/// <summary>
/// The one formatter of a suggested starting goal's text and kind (FR-017, ADR 0011, research R-06). Pure: the
/// result depends only on the goal, never on a culture or a clock. Its parts are shared with the goal text
/// (slice 4 research R-05).
/// </summary>
public static class SuggestedStartingGoalText
{
    /// <summary>The text ("20 pages, 3 times a week") and the kind ("Count · flexible") of a suggested starting goal.</summary>
    public static (string Text, string Kind) Format(SuggestedStartingGoal goal)
    {
        ArgumentNullException.ThrowIfNull(goal);
        return (
            TargetTextParts.Text(goal.BuildOrReduce, goal.Measurement, goal.Amount, goal.CountUnit, goal.DurationUnit, goal.Schedule),
            TargetTextParts.Kind(goal.Measurement, goal.Schedule));
    }
}
