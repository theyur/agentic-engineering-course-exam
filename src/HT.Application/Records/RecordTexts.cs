namespace HT.Application.Records;

/// <summary>
/// The texts of the recording use cases, worded as in the spec (FR-002, FR-005, FR-007; contracts/application-api.md,
/// "Texts"). Texts shared with earlier slices are not repeated here: "You are not signed in." is
/// <c>AccountTexts.NotSignedIn</c>, and "Goals are for registered users.", "This goal does not exist." and "Choose a
/// range of 1 to 366 days." are <c>GoalTexts</c>'. No text names a value, a note, a goal or a habit (HP-006).
/// </summary>
public static class RecordTexts
{
    /// <summary>FR-002: the date belongs to no commitment or evaluation period of the goal.</summary>
    public const string NothingScheduled = "Nothing is scheduled on this date.";

    /// <summary>FR-002: the date's day has not begun on the goal's timeline.</summary>
    public const string DayNotBegun = "You cannot record a day that has not begun.";

    /// <summary>FR-005: a yes/no record with neither "Yes" nor "No".</summary>
    public const string AnswerMissing = "Choose Yes or No.";

    /// <summary>FR-005: a count missing or not a whole number from 0 to 9999.</summary>
    public const string CountOutOfRange = "Enter a whole number from 0 to 9999.";

    /// <summary>FR-005: a duration missing or not a whole number from 0 to 1440.</summary>
    public const string DurationOutOfRange = "Enter a whole number of minutes from 0 to 1440.";

    /// <summary>FR-007: a note longer than 500 characters after trimming.</summary>
    public const string NoteTooLong = "The note can have at most 500 characters.";

    /// <summary>FR-005: adding to an evaluation period that already has 50 records.</summary>
    public const string RecordLimitReached = "This evaluation period already has 50 records.";

    /// <summary>FR-005: a record of another goal, of another account or of nothing.</summary>
    public const string RecordNotFound = "This record does not exist.";
}
