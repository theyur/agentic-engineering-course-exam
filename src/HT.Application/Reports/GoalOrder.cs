using HT.Domain.Habits;

namespace HT.Application.Reports;

/// <summary>
/// The goal order of the overview and the dashboard (FR-011, research R-06): the goals with a Pending result first,
/// then those with a Due result, then the others; within each group by name (slice 2 FR-004's comparison,
/// <see cref="HabitName.KeyOf"/> compared ordinally, as <c>GoalService.ListAsync</c> sorts) and then in creation order,
/// the order <c>IGoalStore.ListAsync</c> gives. The dashboard also breaks its ties in this order.
/// </summary>
internal static class GoalOrder
{
    /// <summary>
    /// <paramref name="inCreationOrder"/> in the goal order; each item's goal is <paramref name="evaluationOf"/> of it.
    /// The sort is stable, so goals of one group with the same name key keep their creation order.
    /// </summary>
    internal static IReadOnlyList<T> Of<T>(IEnumerable<T> inCreationOrder, Func<T, GoalEvaluation> evaluationOf)
    {
        ArgumentNullException.ThrowIfNull(inCreationOrder);
        ArgumentNullException.ThrowIfNull(evaluationOf);
        return inCreationOrder
            .OrderBy(item => GroupOf(evaluationOf(item)))
            .ThenBy(item => HabitName.KeyOf(evaluationOf(item).Name), StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>0 with a Pending result, 1 with a Due result (and none Pending), 2 otherwise.</summary>
    private static int GroupOf(GoalEvaluation evaluation) =>
        evaluation.Pending.Count > 0 ? 0 : evaluation.Due.Count > 0 ? 1 : 2;
}
