using System.Diagnostics.CodeAnalysis;

namespace HT.Domain.Habits;

/// <summary>What kind of problem a list of suggested starting goals has (FR-006, FR-007).</summary>
public enum SuggestedStartingGoalsProblemKind
{
    None,
    TooMany,
    MarkMissing,
    Item,
}

/// <summary>
/// The first problem of a list of suggested starting goals: more than 5, a non-empty list without exactly one mark,
/// or the first item problem with its position counted from 1.
/// </summary>
public readonly record struct SuggestedStartingGoalsProblem(
    SuggestedStartingGoalsProblemKind Kind, SuggestedStartingGoalProblem ItemProblem, int? Position)
{
    public static SuggestedStartingGoalsProblem None { get; } =
        new(SuggestedStartingGoalsProblemKind.None, SuggestedStartingGoalProblem.None, null);

    public static SuggestedStartingGoalsProblem TooMany { get; } =
        new(SuggestedStartingGoalsProblemKind.TooMany, SuggestedStartingGoalProblem.None, null);

    public static SuggestedStartingGoalsProblem MarkMissing { get; } =
        new(SuggestedStartingGoalsProblemKind.MarkMissing, SuggestedStartingGoalProblem.None, null);

    public static SuggestedStartingGoalsProblem ForItem(SuggestedStartingGoalProblem problem, int position) =>
        new(SuggestedStartingGoalsProblemKind.Item, problem, position);
}

/// <summary>
/// A catalog habit's ordered list of 0 to 5 suggested starting goals (FR-006). A non-empty list has exactly one
/// marked suggested starting goal, the one the catalog card shows.
/// </summary>
public sealed class SuggestedStartingGoals
{
    public const int MaxCount = 5;

    private SuggestedStartingGoals(IReadOnlyList<SuggestedStartingGoal> items, int? markedIndex)
    {
        Items = items;
        MarkedIndex = markedIndex;
    }

    /// <summary>The empty list.</summary>
    public static SuggestedStartingGoals None { get; } = new([], null);

    /// <summary>The suggested starting goals in the administrator's order.</summary>
    public IReadOnlyList<SuggestedStartingGoal> Items { get; }

    /// <summary>The index (from 0) of the marked suggested starting goal; null exactly when the list is empty.</summary>
    public int? MarkedIndex { get; }

    /// <summary>The marked suggested starting goal; null when the list is empty.</summary>
    public SuggestedStartingGoal? Marked => MarkedIndex is { } index ? Items[index] : null;

    public int Count => Items.Count;

    /// <summary>
    /// Checks the list in FR-006's order — more than 5, then not exactly one mark on a non-empty list — and then
    /// each item in list order with <see cref="SuggestedStartingGoal.TryCreate"/>.
    /// </summary>
    public static bool TryCreate(
        IReadOnlyList<(SuggestedStartingGoalFields Fields, bool IsMarked)> items,
        [NotNullWhen(true)] out SuggestedStartingGoals? goals,
        out SuggestedStartingGoalsProblem problem)
    {
        ArgumentNullException.ThrowIfNull(items);
        goals = null;
        if (items.Count > MaxCount)
        {
            problem = SuggestedStartingGoalsProblem.TooMany;
            return false;
        }

        if (items.Count == 0)
        {
            goals = None;
            problem = SuggestedStartingGoalsProblem.None;
            return true;
        }

        if (items.Count(i => i.IsMarked) != 1)
        {
            problem = SuggestedStartingGoalsProblem.MarkMissing;
            return false;
        }

        var created = new List<SuggestedStartingGoal>(items.Count);
        int? markedIndex = null;
        for (var i = 0; i < items.Count; i++)
        {
            if (!SuggestedStartingGoal.TryCreate(items[i].Fields, out var goal, out var itemProblem))
            {
                problem = SuggestedStartingGoalsProblem.ForItem(itemProblem, i + 1);
                return false;
            }

            created.Add(goal);
            if (items[i].IsMarked)
            {
                markedIndex = i;
            }
        }

        goals = new SuggestedStartingGoals(created, markedIndex);
        problem = SuggestedStartingGoalsProblem.None;
        return true;
    }
}
