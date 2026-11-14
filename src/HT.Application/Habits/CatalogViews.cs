using HT.Domain.Habits;

namespace HT.Application.Habits;

/// <summary>
/// A suggested starting goal as the catalog shows it: its fields, its position (from 1), whether it is the one the
/// card shows, and its text and kind from <see cref="SuggestedStartingGoalText"/> (FR-017, ADR 0011).
/// </summary>
public sealed record SuggestedStartingGoalView(
    int Position,
    bool IsMarked,
    BuildOrReduce BuildOrReduce,
    Measurement Measurement,
    int? Amount,
    string? CountUnit,
    DurationUnit? DurationUnit,
    ScheduleKind ScheduleKind,
    IReadOnlySet<DayOfWeek> Days,
    int? Times,
    SchedulePeriod? Period,
    string Text,
    string Kind);

/// <summary>A Published catalog habit as browsing returns it (FR-008).</summary>
public sealed record CatalogHabitSummary(
    HabitId Id,
    string Name,
    Category Category,
    Intent Intent,
    string Icon,
    string? Description,
    SuggestedStartingGoalView? MarkedSuggestedStartingGoal);

/// <summary>A Published catalog habit's details (FR-010): the summary's members and all its suggested starting goals in order.</summary>
public sealed record CatalogHabitDetails(
    HabitId Id,
    string Name,
    Category Category,
    Intent Intent,
    string Icon,
    string? Description,
    SuggestedStartingGoalView? MarkedSuggestedStartingGoal,
    IReadOnlyList<SuggestedStartingGoalView> SuggestedStartingGoals);

/// <summary>One option of the category filter: null with "All", or a category with its name (FR-009).</summary>
public sealed record CategoryFilterOption(Category? Category, string Label);

/// <summary>The one mapping from domain habits to catalog views; texts come from <see cref="SuggestedStartingGoalText"/>.</summary>
internal static class CatalogViews
{
    public static CatalogHabitSummary Summary(CatalogHabit habit)
    {
        var definition = habit.Definition;
        return new CatalogHabitSummary(
            habit.Id,
            definition.Name.Value,
            definition.Category,
            definition.Intent,
            definition.Icon.Name,
            definition.Description,
            Marked(habit.SuggestedStartingGoals));
    }

    public static CatalogHabitDetails Details(CatalogHabit habit)
    {
        var definition = habit.Definition;
        return new CatalogHabitDetails(
            habit.Id,
            definition.Name.Value,
            definition.Category,
            definition.Intent,
            definition.Icon.Name,
            definition.Description,
            Marked(habit.SuggestedStartingGoals),
            Goals(habit.SuggestedStartingGoals));
    }

    public static IReadOnlyList<SuggestedStartingGoalView> Goals(SuggestedStartingGoals goals) =>
        goals.Items.Select((goal, index) => Goal(goal, index + 1, index == goals.MarkedIndex)).ToList();

    private static SuggestedStartingGoalView? Marked(SuggestedStartingGoals goals) =>
        goals.MarkedIndex is { } index ? Goal(goals.Items[index], index + 1, true) : null;

    private static SuggestedStartingGoalView Goal(SuggestedStartingGoal goal, int position, bool isMarked)
    {
        var (text, kind) = SuggestedStartingGoalText.Format(goal);
        return new SuggestedStartingGoalView(
            position,
            isMarked,
            goal.BuildOrReduce,
            goal.Measurement,
            goal.Amount,
            goal.CountUnit,
            goal.DurationUnit,
            goal.Schedule.Kind,
            goal.Schedule.Days,
            goal.Schedule.Times,
            goal.Schedule.Period,
            text,
            kind);
    }
}
