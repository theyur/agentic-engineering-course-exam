namespace HT.Domain.Habits;

/// <summary>The problems of a habit's fields, in FR-005's check order. <see cref="None"/> when the fields are accepted.</summary>
public enum HabitFieldProblem
{
    None,
    NameMissing,
    NameTooLong,
    DescriptionTooLong,
    CategoryMissing,
    IntentMissing,
    IconMissing,
}
