namespace HT.Application.Habits;

/// <summary>The field texts shared by catalog habits and custom habits (FR-005), worded as in the spec (ADR 0005).</summary>
public static class HabitTexts
{
    public const string NameMissing = "Enter a name.";
    public const string NameTooLong = "The name can have at most 60 characters.";
    public const string DescriptionTooLong = "The description can have at most 500 characters.";
    public const string CategoryMissing = "Choose a category.";
    public const string IntentMissing = "Choose Build or Reduce.";
    public const string IconMissing = "Choose an icon.";
}
