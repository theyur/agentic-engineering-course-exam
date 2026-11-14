using System.Diagnostics.CodeAnalysis;

namespace HT.Domain.Habits;

/// <summary>
/// The fields every habit has (FR-001): name, optional description, category, intent and icon. Immutable; only
/// <see cref="TryCreate"/> makes one, so every definition has passed FR-005's checks.
/// </summary>
public sealed class HabitDefinition
{
    public const int DescriptionMaxLength = 500;

    private HabitDefinition(HabitName name, string? description, Category category, Intent intent, HabitIcon icon)
    {
        Name = name;
        Description = description;
        Category = category;
        Intent = intent;
        Icon = icon;
    }

    public HabitName Name { get; }

    /// <summary>The trimmed description; null when there is none.</summary>
    public string? Description { get; }

    public Category Category { get; }

    public Intent Intent { get; }

    public HabitIcon Icon { get; }

    /// <summary>
    /// Checks the fields in FR-005's order and gives the first problem: name missing, name too long,
    /// description too long, category missing, intent missing, icon missing (none, or not a habit icon). A
    /// description that is empty after trimming is no description.
    /// </summary>
    public static bool TryCreate(
        string? name,
        string? description,
        Category? category,
        Intent? intent,
        string? icon,
        [NotNullWhen(true)] out HabitDefinition? definition,
        out HabitFieldProblem problem)
    {
        definition = null;
        if (!HabitName.TryCreate(name, out var habitName, out problem))
        {
            return false;
        }

        var trimmed = description?.Trim();
        if (trimmed is { Length: > DescriptionMaxLength })
        {
            problem = HabitFieldProblem.DescriptionTooLong;
            return false;
        }

        if (category is not { } chosenCategory || !Enum.IsDefined(chosenCategory))
        {
            problem = HabitFieldProblem.CategoryMissing;
            return false;
        }

        if (intent is not { } chosenIntent || !Enum.IsDefined(chosenIntent))
        {
            problem = HabitFieldProblem.IntentMissing;
            return false;
        }

        if (!HabitIcon.TryParse(icon, out var habitIcon))
        {
            problem = HabitFieldProblem.IconMissing;
            return false;
        }

        definition = new HabitDefinition(
            habitName, string.IsNullOrEmpty(trimmed) ? null : trimmed, chosenCategory, chosenIntent, habitIcon);
        problem = HabitFieldProblem.None;
        return true;
    }
}
