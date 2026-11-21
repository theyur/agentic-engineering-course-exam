namespace HT.Application.Exemptions;

/// <summary>
/// The texts of pausing, resuming and cancelling a pause, and of the record refusals on paused dates, worded as in the
/// spec (FR-002 (6), FR-005, FR-006, FR-009 to FR-011; data-model.md, the texts table; HP-008). Texts shared with
/// earlier slices are not repeated here: "You are not signed in." is <c>AccountTexts.NotSignedIn</c>, and "Goals are
/// for registered users." and "This goal does not exist." are <c>GoalTexts</c>'. No text names a reason, a date, a
/// goal or a habit (HP-009).
/// </summary>
public static class PauseTexts
{
    /// <summary>FR-002 (6), FR-005 (4a), FR-009: a date in a pause of the goal.</summary>
    public const string DatePaused = "This goal is paused on this date.";

    /// <summary>FR-005 (4b), FR-009: a date of a Paused per-week period that is not itself in a pause.</summary>
    public const string WeekPaused = "This week is paused.";

    /// <summary>FR-006 (4): the first or the last day is missing.</summary>
    public const string DaysMissing = "Choose the first and the last day.";

    /// <summary>FR-006 (5): the first day is before the pause date (research R-03).</summary>
    public const string StartsBeforeToday = "A pause cannot start before today.";

    /// <summary>FR-006 (6): the last day is before the first day.</summary>
    public const string LastBeforeFirst = "The last day cannot be before the first day.";

    /// <summary>FR-006 (7): the range shares a date with another pause of the goal.</summary>
    public const string Overlaps = "This goal already has a pause on these dates.";

    /// <summary>FR-006 (8): the reason rule of FR-002 (10); the same text as <see cref="ExcusalTexts.ReasonTooLong"/>.</summary>
    public const string ReasonTooLong = ExcusalTexts.ReasonTooLong;

    /// <summary>FR-010 (4): the goal has no current pause.</summary>
    public const string NotPaused = "This goal is not paused.";

    /// <summary>FR-011 (4): the pause is not one of this goal's.</summary>
    public const string PauseNotFound = "This pause does not exist.";

    /// <summary>FR-011 (5): the pause is current or past.</summary>
    public const string HasBegun = "Only a pause that has not begun can be cancelled. Resume the goal instead.";

    /// <summary>FR-006: the success word of pausing.</summary>
    public const string Paused = "paused";

    /// <summary>FR-010: the success word of resuming.</summary>
    public const string Resumed = "resumed";

    /// <summary>FR-011: the success word of cancelling a pause.</summary>
    public const string Cancelled = "pause cancelled";
}
