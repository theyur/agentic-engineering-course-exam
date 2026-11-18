namespace HT.Application.Goals;

/// <summary>
/// The texts of the goal use cases, worded as in the spec (contracts/application-api.md, "Texts"). Texts shared with
/// slices 1 and 2 are not repeated here: "You are not signed in." is <c>AccountTexts.NotSignedIn</c>, "This habit is
/// not in the catalog." is <c>CatalogTexts.NotInCatalog</c>, "This custom habit does not exist." is
/// <c>CustomHabitTexts.NotFound</c>, the other target texts are <c>CatalogTexts</c>' and the goal's name refusal is
/// <c>AccountTexts.NameTooLong</c>.
/// </summary>
public static class GoalTexts
{
    /// <summary>An administrator on a goal use case, and on "Only habits I track".</summary>
    public const string NotForAdministrators = "Goals are for registered users.";
    public const string HabitMissing = "Choose a habit.";
    public const string SuggestionNotFound = "This suggested starting goal does not exist.";
    public const string YesNoHasAmountOrUnit = "A yes/no goal has no amount or unit.";
    public const string ReduceAmountNeedsOneTime = "A Reduce goal with an amount has 1 time per period.";
    public const string StartDateMissing = "Choose a start date.";
    public const string StartDateInPast = "The start date cannot be in the past.";
    public const string StartDateTooFar = "Choose a start date within a year from today.";
    public const string NotFound = "This goal does not exist.";
    public const string RangeOutOfBounds = "Choose a range of 1 to 366 days.";
}
