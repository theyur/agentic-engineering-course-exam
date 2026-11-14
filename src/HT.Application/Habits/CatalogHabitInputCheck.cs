using HT.Domain.Habits;

namespace HT.Application.Habits;

/// <summary>
/// The field outcomes shared by creating and editing a catalog habit, in FR-005, FR-006 and FR-007's order. Each
/// name is also a member of <see cref="CatalogHabitCreateOutcome"/> and <see cref="CatalogHabitEditOutcome"/>.
/// </summary>
internal enum CatalogFieldOutcome
{
    NameMissing,
    NameTooLong,
    DescriptionTooLong,
    CategoryMissing,
    IntentMissing,
    IconMissing,
    TooManySuggestedStartingGoals,
    SuggestedStartingGoalNotMarked,
    BuildOrReduceMissing,
    MeasurementMissing,
    YesNoHasAmountOrUnit,
    BuildAmountOutOfRange,
    ReduceAmountOutOfRange,
    CountUnitMissing,
    CountUnitTooLong,
    DurationUnitMissing,
    ScheduleMissing,
    DaysMissing,
    TimesOutOfRange,
    PeriodMissing,
}

/// <summary>The first field problem of a catalog habit input: its outcome, its text and, for one suggested starting goal, its position.</summary>
internal sealed record CatalogFieldProblem(CatalogFieldOutcome Outcome, string Message, int? Position)
{
    /// <summary>The same-named member of a use case's outcome enum.</summary>
    public TOutcome As<TOutcome>()
        where TOutcome : struct, Enum => Enum.Parse<TOutcome>(Outcome.ToString());
}

/// <summary>
/// Turns a <see cref="CatalogHabitInput"/> into a definition and a list of suggested starting goals with the
/// Domain checks, or into the first problem with its text (research R-04).
/// </summary>
internal static class CatalogHabitInputCheck
{
    public static bool TryCheck(
        CatalogHabitInput input,
        out HabitDefinition definition,
        out SuggestedStartingGoals goals,
        out CatalogFieldProblem? problem)
    {
        ArgumentNullException.ThrowIfNull(input);
        definition = null!;
        goals = null!;
        if (!HabitDefinition.TryCreate(input.Name, input.Description, input.Category, input.Intent, input.Icon, out var checkedDefinition, out var fieldProblem))
        {
            problem = Field(fieldProblem);
            return false;
        }

        var items = (input.SuggestedStartingGoals ?? []).Select(g => (Fields(g), g.IsMarked)).ToList();
        if (!SuggestedStartingGoals.TryCreate(items, out var checkedGoals, out var listProblem))
        {
            problem = List(listProblem);
            return false;
        }

        definition = checkedDefinition;
        goals = checkedGoals;
        problem = null;
        return true;
    }

    /// <summary>The text of a habit field problem (FR-005); shared with the custom habits.</summary>
    public static string TextOf(HabitFieldProblem problem) => problem switch
    {
        HabitFieldProblem.NameMissing => HabitTexts.NameMissing,
        HabitFieldProblem.NameTooLong => HabitTexts.NameTooLong,
        HabitFieldProblem.DescriptionTooLong => HabitTexts.DescriptionTooLong,
        HabitFieldProblem.CategoryMissing => HabitTexts.CategoryMissing,
        HabitFieldProblem.IntentMissing => HabitTexts.IntentMissing,
        HabitFieldProblem.IconMissing => HabitTexts.IconMissing,
        _ => throw new ArgumentOutOfRangeException(nameof(problem), problem, "Not a field problem."),
    };

    private static CatalogFieldProblem Field(HabitFieldProblem problem) =>
        new(Enum.Parse<CatalogFieldOutcome>(problem.ToString()), TextOf(problem), null);

    private static CatalogFieldProblem List(SuggestedStartingGoalsProblem problem) => problem.Kind switch
    {
        SuggestedStartingGoalsProblemKind.TooMany =>
            new(CatalogFieldOutcome.TooManySuggestedStartingGoals, CatalogTexts.TooManySuggestedStartingGoals, null),
        SuggestedStartingGoalsProblemKind.MarkMissing =>
            new(CatalogFieldOutcome.SuggestedStartingGoalNotMarked, CatalogTexts.SuggestedStartingGoalNotMarked, null),
        SuggestedStartingGoalsProblemKind.Item =>
            new(Enum.Parse<CatalogFieldOutcome>(problem.ItemProblem.ToString()), ItemText(problem.ItemProblem), problem.Position),
        _ => throw new ArgumentOutOfRangeException(nameof(problem), problem.Kind, "Not a list problem."),
    };

    private static string ItemText(SuggestedStartingGoalProblem problem) => problem switch
    {
        // The suggested starting goal's Build or Reduce has the habit intent's text, as its own outcome (contract, "Texts").
        SuggestedStartingGoalProblem.BuildOrReduceMissing => HabitTexts.IntentMissing,
        SuggestedStartingGoalProblem.MeasurementMissing => CatalogTexts.MeasurementMissing,
        SuggestedStartingGoalProblem.YesNoHasAmountOrUnit => CatalogTexts.YesNoHasAmountOrUnit,
        SuggestedStartingGoalProblem.BuildAmountOutOfRange => CatalogTexts.BuildAmountOutOfRange,
        SuggestedStartingGoalProblem.ReduceAmountOutOfRange => CatalogTexts.ReduceAmountOutOfRange,
        SuggestedStartingGoalProblem.CountUnitMissing => CatalogTexts.CountUnitMissing,
        SuggestedStartingGoalProblem.CountUnitTooLong => CatalogTexts.CountUnitTooLong,
        SuggestedStartingGoalProblem.DurationUnitMissing => CatalogTexts.DurationUnitMissing,
        SuggestedStartingGoalProblem.ScheduleMissing => CatalogTexts.ScheduleMissing,
        SuggestedStartingGoalProblem.DaysMissing => CatalogTexts.DaysMissing,
        SuggestedStartingGoalProblem.TimesOutOfRange => CatalogTexts.TimesOutOfRange,
        SuggestedStartingGoalProblem.PeriodMissing => CatalogTexts.PeriodMissing,
        _ => throw new ArgumentOutOfRangeException(nameof(problem), problem, "Not an item problem."),
    };

    private static SuggestedStartingGoalFields Fields(SuggestedStartingGoalInput input) => new()
    {
        BuildOrReduce = input.BuildOrReduce,
        Measurement = input.Measurement,
        Amount = input.Amount,
        CountUnit = input.CountUnit,
        DurationUnit = input.DurationUnit,
        ScheduleKind = input.ScheduleKind,
        Days = input.Days,
        Times = input.Times,
        Period = input.Period,
    };
}
