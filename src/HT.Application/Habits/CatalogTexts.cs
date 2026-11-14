namespace HT.Application.Habits;

/// <summary>The texts of the catalog use cases (FR-006, FR-007, FR-010 to FR-014), worded as in the spec.</summary>
public static class CatalogTexts
{
    public const string NotAdministrator = "Only administrators can manage the catalog.";
    public const string NotFound = "This catalog habit does not exist.";
    public const string NotInCatalog = "This habit is not in the catalog.";
    public const string NotPublished = "This catalog habit is not published.";
    public const string NameTaken = "A catalog habit with this name already exists.";
    public const string TooManySuggestedStartingGoals = "A habit can have at most 5 suggested starting goals.";
    public const string SuggestedStartingGoalNotMarked = "Mark one suggested starting goal for the card.";
    public const string MeasurementMissing = "Choose how it is measured.";
    public const string YesNoHasAmountOrUnit = "A yes/no suggested starting goal has no amount or unit.";
    public const string BuildAmountOutOfRange = "Enter a whole number from 1 to 999.";
    public const string ReduceAmountOutOfRange = "Enter a whole number from 0 to 999.";
    public const string CountUnitMissing = "Enter a unit, for example pages.";
    public const string CountUnitTooLong = "The unit can have at most 20 characters.";
    public const string DurationUnitMissing = "Choose minutes or hours.";
    public const string ScheduleMissing = "Choose fixed days or how many times.";
    public const string DaysMissing = "Choose at least one day.";
    public const string TimesOutOfRange = "Enter a number of times from 1 to 20.";
    public const string PeriodMissing = "Choose per day or per week.";
    public const string AllCategories = "All";
}
