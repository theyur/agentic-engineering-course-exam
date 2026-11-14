using HT.Domain.Accounts;
using HT.Domain.Habits;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace HT.Persistence.Habits;

/// <summary>
/// The only mapping between habit rows and domain types (data-model.md). Rows become domain values through the
/// Domain checks; a stored row that fails them is a fault whose message carries no field value (HP-015).
/// </summary>
internal static class HabitMapping
{
    public static CatalogHabit ToCatalogHabit(CatalogHabitRow row)
    {
        if (!HabitDefinition.TryCreate(row.Name, row.Description, row.Category, row.Intent, row.Icon, out var definition, out _))
        {
            throw new InvalidOperationException("A stored catalog habit fails the habit checks.");
        }

        var items = row.SuggestedStartingGoals
            .OrderBy(g => g.Position)
            .Select(g => (ToFields(g), g.IsMarked))
            .ToList();
        if (!SuggestedStartingGoals.TryCreate(items, out var goals, out _))
        {
            throw new InvalidOperationException("A stored catalog habit has suggested starting goals that fail their checks.");
        }

        return new CatalogHabit(new HabitId(row.Id), definition, goals, row.State);
    }

    public static CustomHabit ToCustomHabit(CustomHabitRow row)
    {
        if (!HabitDefinition.TryCreate(row.Name, row.Description, row.Category, row.Intent, row.Icon, out var definition, out _))
        {
            throw new InvalidOperationException("A stored custom habit fails the habit checks.");
        }

        return new CustomHabit(new HabitId(row.Id), new AccountId(row.OwnerId), definition);
    }

    /// <summary>Copies the definition's fields into a catalog habit row; the key is <see cref="HabitName.Key"/>.</summary>
    public static void CopyDefinition(HabitDefinition definition, CatalogHabitRow row)
    {
        row.Name = definition.Name.Value;
        row.NameKey = definition.Name.Key;
        row.Description = definition.Description;
        row.Category = definition.Category;
        row.Intent = definition.Intent;
        row.Icon = definition.Icon.Name;
    }

    /// <summary>The rows of a list of suggested starting goals, positions 1 to n in list order.</summary>
    public static List<SuggestedStartingGoalRow> ToRows(SuggestedStartingGoals goals) =>
        goals.Items.Select((goal, index) => ToRow(goal, index + 1, index == goals.MarkedIndex)).ToList();

    /// <summary>Copies every value of <paramref name="source"/> but the position into <paramref name="target"/>.</summary>
    public static void CopyValues(SuggestedStartingGoalRow source, SuggestedStartingGoalRow target)
    {
        target.IsMarked = source.IsMarked;
        target.BuildOrReduce = source.BuildOrReduce;
        target.Measurement = source.Measurement;
        target.Amount = source.Amount;
        target.CountUnit = source.CountUnit;
        target.DurationUnit = source.DurationUnit;
        target.ScheduleKind = source.ScheduleKind;
        target.Days = source.Days;
        target.Times = source.Times;
        target.Period = source.Period;
    }

    /// <summary>A unique-index violation (SQL errors 2601 and 2627).</summary>
    public static bool IsUniqueIndexViolation(DbUpdateException exception) =>
        exception.InnerException is SqlException { Number: 2601 or 2627 };

    private static SuggestedStartingGoalRow ToRow(SuggestedStartingGoal goal, int position, bool isMarked) => new()
    {
        Position = position,
        IsMarked = isMarked,
        BuildOrReduce = goal.BuildOrReduce,
        Measurement = goal.Measurement,
        Amount = goal.Amount,
        CountUnit = goal.CountUnit,
        DurationUnit = goal.DurationUnit,
        ScheduleKind = goal.Schedule.Kind,
        Days = goal.Schedule.Kind == ScheduleKind.FixedDays ? DayMask(goal.Schedule.Days) : null,
        Times = goal.Schedule.Times,
        Period = goal.Schedule.Period,
    };

    private static SuggestedStartingGoalFields ToFields(SuggestedStartingGoalRow row) => new()
    {
        BuildOrReduce = row.BuildOrReduce,
        Measurement = row.Measurement,
        Amount = row.Amount,
        CountUnit = row.CountUnit,
        DurationUnit = row.DurationUnit,
        ScheduleKind = row.ScheduleKind,
        Days = row.Days is { } mask ? DaysOf(mask) : null,
        Times = row.Times,
        Period = row.Period,
    };

    /// <summary>Monday 1, Tuesday 2 … Sunday 64.</summary>
    private static int DayBit(DayOfWeek day) => 1 << (((int)day + 6) % 7);

    private static int DayMask(IEnumerable<DayOfWeek> days) => days.Aggregate(0, (mask, day) => mask | DayBit(day));

    private static HashSet<DayOfWeek> DaysOf(int mask) =>
        Enum.GetValues<DayOfWeek>().Where(day => (mask & DayBit(day)) != 0).ToHashSet();
}
