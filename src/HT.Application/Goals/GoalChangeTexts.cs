namespace HT.Application.Goals;

/// <summary>
/// The texts of the goal-change use cases (slice 8, contracts/application-api.md "Texts"), worded as in the spec
/// (FR-003, FR-007, FR-011, FR-012, FR-014) and the outcome words. Each outcome and the text it maps to share one name:
/// <see cref="GoalEnded"/> is always "This goal has ended." and <see cref="Ended"/> always "goal ended". Texts of
/// earlier slices are not repeated here: "The note can have at most 500 characters." is <c>RecordTexts.NoteTooLong</c>,
/// and the access, target, start date and name texts are those of <see cref="GoalTexts"/>, <c>AccountTexts</c> and
/// <c>CatalogTexts</c>.
/// </summary>
public static class GoalChangeTexts
{
    public const string SettingsChanged = "settings changed";
    public const string SettingsUnchanged = "settings unchanged";

    /// <summary>The success text of an ending (<c>GoalEndResult.Ended</c>).</summary>
    public const string Ended = "goal ended";

    public const string AttemptStarted = "attempt started";

    /// <summary>The refusal of a settings change or a pause on an ended goal (the outcomes named <c>GoalEnded</c>).</summary>
    public const string GoalEnded = "This goal has ended.";

    public const string AlreadyEnded = "This goal has already ended.";

    public const string ScheduleKindChanged =
        "A change keeps the schedule kind: fixed days, times per day or times per week. End the goal and start a new attempt to change it.";

    public const string OutcomeMissing = "Choose how the goal ended.";
    public const string NotEnded = "Only an ended goal can start a new attempt.";
    public const string StartDateNotAfterEnding = "A new attempt starts after the day the goal ended.";
    public const string AttemptNotFound = "This attempt does not exist.";

    /// <summary>The word of <c>Outcome.Achieved</c>.</summary>
    public const string Achieved = "Achieved";

    /// <summary>The word of <c>Outcome.Abandoned</c>.</summary>
    public const string Abandoned = "Abandoned";

    /// <summary>The word of <c>Outcome.NoLongerRelevant</c>.</summary>
    public const string NoLongerRelevant = "No longer relevant";
}
