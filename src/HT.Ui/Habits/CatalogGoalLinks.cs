using HT.Domain.Habits;

namespace HT.Ui.Habits;

/// <summary>
/// The words and addresses of a registered user's goal parts on the catalog pages (HP-005, HP-006): the count of
/// their goals on a habit, and the New goal page with a catalog habit and, optionally, one of its suggested starting
/// goals (counted from 1; contracts/host.md "Routes").
/// </summary>
internal static class CatalogGoalLinks
{
    /// <summary>"1 goal" or "N goals".</summary>
    public static string GoalsText(int count) => count == 1 ? "1 goal" : $"{count} goals";

    /// <summary>The New goal page with this catalog habit.</summary>
    public static string NewGoal(HabitId habit) => $"/goals/new?habit={habit.Value:D}&kind=catalog";

    /// <summary>The New goal page with this catalog habit and its suggested starting goal at <paramref name="position"/>.</summary>
    public static string NewGoal(HabitId habit, int position) => $"{NewGoal(habit)}&suggestion={position}";
}
