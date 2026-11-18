using HT.Application.Goals;
using HT.Domain.Goals;
using HT.Domain.Habits;

namespace HT.AcceptanceTests.Support.F004;

/// <summary>
/// Checks the facts a Then step lists against a goal read, a goal list entry, starting values or a calendar.
/// Every fact the step states is asserted; a fact a result type cannot carry fails the step.
/// </summary>
public static class F004Assert
{
    // ------------------------------------------------------------ goal read (FR-016)

    public static void Goal(GoalDetails goal, string factsText, GoalId? expectedId = null)
    {
        var facts = F004Text.Facts(factsText);
        var targetLiteral = facts.LastOrDefault(fact => fact.Kind == FactKind.TargetText)?.Group(1);

        foreach (var fact in facts)
        {
            switch (fact.Kind)
            {
                case FactKind.HabitWithKind:
                    Assert.Equal(fact.Group(1), goal.Habit.Name);
                    Assert.Equal(F004Text.HabitKindOf(fact.Group(2)), goal.Habit.Kind);
                    break;
                case FactKind.HabitName:
                    Assert.Equal(fact.Group(1), goal.Habit.Name);
                    break;
                case FactKind.Kind:
                    Assert.Equal(F004Text.HabitKindOf(fact.Group(1)), goal.Habit.Kind);
                    break;
                case FactKind.Category:
                    Assert.Equal(Enum.Parse<Category>(fact.Group(1)), goal.Habit.Category);
                    break;
                case FactKind.InCatalog:
                    Assert.Equal(!fact.Has(1), goal.Habit.IsInCatalog);
                    break;
                case FactKind.GoalId:
                    Assert.Equal(expectedId ?? throw new InvalidOperationException("No goal identifier is known."), goal.Id);
                    break;
                case FactKind.NameIsOwn:
                    Assert.Equal(!fact.Has(1), goal.HasOwnName);
                    break;
                case FactKind.Name:
                    if (fact.Has(2))
                    {
                        var target = targetLiteral ?? throw new InvalidOperationException("\"that target text\" names no target text.");
                        Assert.Equal(fact.Group(1) + target, goal.Name);
                    }
                    else
                    {
                        Assert.Equal(fact.Group(1), goal.Name);
                    }

                    if (fact.Has(3) || fact.Match.Value.EndsWith("its own", StringComparison.Ordinal))
                    {
                        Assert.Equal(!fact.Has(3), goal.HasOwnName);
                    }

                    break;
                case FactKind.NoOwnName:
                    Assert.False(goal.HasOwnName);
                    break;
                case FactKind.BuildOrReduce:
                    Assert.Equal(Enum.Parse<BuildOrReduce>(fact.Group(1)), goal.BuildOrReduce);
                    break;
                case FactKind.Duration:
                    Assert.Equal(Measurement.Duration, goal.Measurement);
                    Assert.Equal(F004Text.Int(fact.Group(1)), goal.Amount);
                    Assert.Equal(F004Text.DurationUnitOf(fact.Group(2)), goal.DurationUnit);
                    break;
                case FactKind.Count:
                    Assert.Equal(Measurement.Count, goal.Measurement);
                    Assert.Equal(F004Text.Int(fact.Group(1)), goal.Amount);
                    Assert.Equal(F004Text.TextOf(fact.Group(2)), goal.CountUnit);
                    break;
                case FactKind.Measurement:
                    Assert.Equal(MeasurementOf(fact.Group(1)), goal.Measurement);
                    break;
                case FactKind.NoAmountAndNoUnit:
                    Assert.Null(goal.Amount);
                    Assert.Null(goal.CountUnit);
                    Assert.Null(goal.DurationUnit);
                    break;
                case FactKind.NoDurationUnit:
                    Assert.Null(goal.DurationUnit);
                    break;
                case FactKind.NoCountUnit:
                    Assert.Null(goal.CountUnit);
                    break;
                case FactKind.FixedDays:
                    Assert.Equal(ScheduleKind.FixedDays, goal.ScheduleKind);
                    AssertDays(F004Text.Days(fact.Group(1)), goal.Days);
                    break;
                case FactKind.Times:
                    Assert.Equal(ScheduleKind.Times, goal.ScheduleKind);
                    Assert.Equal(F004Text.Int(fact.Group(1)), goal.Times);
                    Assert.Equal(F004Text.Period(fact.Group(2)), goal.Period);
                    break;
                case FactKind.NoFixedDays:
                    Assert.Empty(goal.Days);
                    break;
                case FactKind.NoTimes:
                    Assert.Null(goal.Times);
                    break;
                case FactKind.ScheduleKind:
                    Assert.Equal(F004Text.ScheduleKindOf(fact.Group(1)), goal.GoalScheduleKind);
                    break;
                case FactKind.TargetText:
                    Assert.Equal(fact.Group(1), goal.TargetText);
                    break;
                case FactKind.StartDate:
                    Assert.Equal(F004Text.Date(fact.Group(1)), goal.StartDate);
                    break;
                case FactKind.FirstWeekStarts:
                    Assert.Equal(F004Text.Date(fact.Group(1)), goal.FirstWeekStarts);
                    break;
                case FactKind.NoFirstWeekStarts:
                    Assert.Null(goal.FirstWeekStarts);
                    break;
                case FactKind.TimeZone:
                    Assert.Equal(fact.Group(1), goal.TimeZone);
                    Assert.Equal(F004Text.Date(fact.Group(2)), goal.TimeZoneAppliesFrom);
                    break;
                case FactKind.LaterVersion:
                    Assert.Equal(fact.Group(1), goal.LaterTimeZone);
                    Assert.Equal(F004Text.Date(fact.Group(2)), goal.LaterTimeZoneAppliesFrom);
                    break;
                case FactKind.NoLaterVersion:
                    Assert.Null(goal.LaterTimeZone);
                    Assert.Null(goal.LaterTimeZoneAppliesFrom);
                    break;
                case FactKind.Attempt:
                    Assert.Equal(F004Text.Int(fact.Group(1)), goal.AttemptNumber);
                    Assert.Equal(F004Text.Date(fact.Group(2)), goal.AttemptStartDate);
                    break;
                default:
                    throw new InvalidOperationException($"A goal read does not give \"{fact.Match.Value}\".");
            }
        }
    }

    // ------------------------------------------------------------ goal list entry (FR-014, FR-015)

    public static void ListItem(GoalListItem item, string factsText)
    {
        foreach (var fact in F004Text.Facts(factsText))
        {
            switch (fact.Kind)
            {
                case FactKind.HabitWithKind:
                    Assert.Equal(fact.Group(1), item.HabitName);
                    Assert.Equal(F004Text.HabitKindOf(fact.Group(2)), item.HabitKind);
                    break;
                case FactKind.HabitName:
                    Assert.Equal(fact.Group(1), item.HabitName);
                    break;
                case FactKind.Kind:
                    Assert.Equal(F004Text.HabitKindOf(fact.Group(1)), item.HabitKind);
                    break;
                case FactKind.Name:
                    Assert.Equal(fact.Group(1), item.Name);
                    break;
                case FactKind.BuildOrReduce:
                    Assert.Equal(Enum.Parse<BuildOrReduce>(fact.Group(1)), item.BuildOrReduce);
                    break;
                case FactKind.Measurement:
                    Assert.Equal(MeasurementOf(fact.Group(1)), item.Measurement);
                    break;
                case FactKind.TargetText:
                    Assert.Equal(fact.Group(1), item.TargetText);
                    break;
                case FactKind.ScheduleKind:
                    Assert.Equal(F004Text.ScheduleKindOf(fact.Group(1)), item.ScheduleKind);
                    break;
                case FactKind.StartDate:
                    Assert.Equal(F004Text.Date(fact.Group(1)), item.StartDate);
                    break;
                case FactKind.NoStartDate:
                    Assert.Null(item.StartDate);
                    break;
                case FactKind.NextCommitment:
                    Assert.Equal(F004Text.Date(fact.Group(1)), item.NextCommitment);
                    break;
                case FactKind.NoNextCommitment:
                    Assert.Null(item.NextCommitment);
                    break;
                case FactKind.FirstWeekStarts:
                    Assert.Equal(F004Text.Date(fact.Group(1)), item.FirstWeekStarts);
                    break;
                case FactKind.NoFirstWeekStarts:
                    Assert.Null(item.FirstWeekStarts);
                    break;
                default:
                    throw new InvalidOperationException($"A goal list entry does not give \"{fact.Match.Value}\".");
            }
        }
    }

    // ------------------------------------------------------------ starting values (FR-007)

    public static void StartingValues(GoalStartingValues values, string factsText, Func<string, HT.Domain.Habits.HabitId?> habitIdOf)
    {
        var facts = F004Text.Facts(factsText);
        var mentioned = new HashSet<string>(StringComparer.Ordinal);

        foreach (var fact in facts)
        {
            switch (fact.Kind)
            {
                case FactKind.HabitWithKind:
                    mentioned.Add("habit");
                    Assert.NotNull(values.Habit);
                    Assert.Equal(fact.Group(1), values.Habit.Name);
                    Assert.Equal(F004Text.HabitKindOf(fact.Group(2)), values.Habit.Kind);
                    if (habitIdOf(fact.Group(1)) is { } id)
                    {
                        Assert.Equal(id, values.Habit.Id);
                    }

                    break;
                case FactKind.NoHabit:
                    mentioned.Add("habit");
                    Assert.Null(values.Habit);
                    break;
                case FactKind.BuildOrReduce:
                    mentioned.Add("buildOrReduce");
                    Assert.Equal(Enum.Parse<BuildOrReduce>(fact.Group(1)), values.BuildOrReduce);
                    break;
                case FactKind.Duration:
                    mentioned.UnionWith(["measurement", "amount", "unit"]);
                    Assert.Equal(Measurement.Duration, values.Measurement);
                    Assert.Equal(F004Text.Int(fact.Group(1)), values.Amount);
                    Assert.Equal(F004Text.DurationUnitOf(fact.Group(2)), values.DurationUnit);
                    break;
                case FactKind.Count:
                    mentioned.UnionWith(["measurement", "amount", "unit"]);
                    Assert.Equal(Measurement.Count, values.Measurement);
                    Assert.Equal(F004Text.Int(fact.Group(1)), values.Amount);
                    Assert.Equal(F004Text.TextOf(fact.Group(2)), values.CountUnit);
                    break;
                case FactKind.Measurement:
                    mentioned.Add("measurement");
                    Assert.Equal(MeasurementOf(fact.Group(1)), values.Measurement);
                    break;
                case FactKind.NoMeasurement:
                    mentioned.Add("measurement");
                    Assert.Null(values.Measurement);
                    break;
                case FactKind.NoAmount:
                    mentioned.Add("amount");
                    Assert.Null(values.Amount);
                    break;
                case FactKind.NoUnit:
                    mentioned.Add("unit");
                    Assert.Null(values.CountUnit);
                    Assert.Null(values.DurationUnit);
                    break;
                case FactKind.NoAmountAndNoUnit:
                    mentioned.UnionWith(["amount", "unit"]);
                    Assert.Null(values.Amount);
                    Assert.Null(values.CountUnit);
                    Assert.Null(values.DurationUnit);
                    break;
                case FactKind.FixedDays:
                    mentioned.Add("schedule");
                    Assert.Equal(ScheduleKind.FixedDays, values.ScheduleKind);
                    AssertDays(F004Text.Days(fact.Group(1)), values.Days);
                    break;
                case FactKind.Times:
                    mentioned.Add("schedule");
                    Assert.Equal(ScheduleKind.Times, values.ScheduleKind);
                    Assert.Equal(F004Text.Int(fact.Group(1)), values.Times);
                    Assert.Equal(F004Text.Period(fact.Group(2)), values.Period);
                    break;
                case FactKind.NoSchedule:
                    mentioned.Add("schedule");
                    AssertNoSchedule(values);
                    break;
                case FactKind.StartDate:
                    mentioned.Add("startDate");
                    Assert.Equal(F004Text.Date(fact.Group(1)), values.StartDate);
                    break;
                case FactKind.NoName:
                    mentioned.Add("name");
                    Assert.True(string.IsNullOrEmpty(values.Name), $"The starting values have the name \"{values.Name}\".");
                    break;
                case FactKind.SuggestionText:
                    mentioned.Add("suggestion");
                    Assert.Equal(fact.Group(1), values.CatalogSuggestionText);
                    break;
                case FactKind.NoSuggestionText:
                    mentioned.Add("suggestion");
                    Assert.Null(values.CatalogSuggestionText);
                    break;
                case FactKind.NothingElse:
                    break;
                default:
                    throw new InvalidOperationException($"Starting values do not give \"{fact.Match.Value}\".");
            }
        }

        if (facts.Any(fact => fact.Kind == FactKind.NothingElse))
        {
            // "and nothing else": every field the step does not name is empty.
            if (!mentioned.Contains("habit")) Assert.Null(values.Habit);
            if (!mentioned.Contains("buildOrReduce")) Assert.Null(values.BuildOrReduce);
            if (!mentioned.Contains("measurement")) Assert.Null(values.Measurement);
            if (!mentioned.Contains("amount")) Assert.Null(values.Amount);
            if (!mentioned.Contains("unit"))
            {
                Assert.Null(values.CountUnit);
                Assert.Null(values.DurationUnit);
            }

            if (!mentioned.Contains("schedule")) AssertNoSchedule(values);
            if (!mentioned.Contains("name")) Assert.True(string.IsNullOrEmpty(values.Name), $"The starting values have the name \"{values.Name}\".");
            if (!mentioned.Contains("suggestion")) Assert.Null(values.CatalogSuggestionText);
        }
    }

    // ------------------------------------------------------------ calendar (FR-013)

    public static IReadOnlyList<CalendarEntrySpec> Entries(GoalCalendarView calendar) =>
        calendar.Kind == GoalScheduleKind.CommitmentBased
            ? calendar.Commitments.Select(entry => new CalendarEntrySpec(entry.Date, entry.Date, entry.Starts, entry.Ends, null)).ToList()
            : calendar.Periods.Select(entry => new CalendarEntrySpec(entry.FirstDate, entry.LastDate, entry.Starts, entry.Ends, null)).ToList();

    public static void Entry(CalendarEntrySpec expected, CalendarEntrySpec actual)
    {
        Assert.Equal(expected.First, actual.First);
        Assert.Equal(expected.Last, actual.Last);
        Assert.Equal(expected.Starts, actual.Starts);
        Assert.Equal(expected.Ends, actual.Ends);
        if (expected.Hours is { } hours)
        {
            Assert.Equal(TimeSpan.FromHours(hours), actual.Ends - actual.Starts);
        }
    }

    /// <summary>A calendar that has exactly the written entries, in the written order.</summary>
    public static void ExactEntries(GoalCalendarView calendar, IReadOnlyList<CalendarEntrySpec> expected)
    {
        Assert.NotEmpty(expected);
        var actual = Entries(calendar);
        Assert.Equal(expected.Count, actual.Count);
        for (var index = 0; index < expected.Count; index++)
        {
            Entry(expected[index], actual[index]);
        }
    }

    public static GoalCalendarView Calendar(GoalCalendarResult? result)
    {
        Assert.NotNull(result);
        Assert.Equal(GoalCalendarOutcome.Found, result.Outcome);
        Assert.NotNull(result.Calendar);
        return result.Calendar;
    }

    /// <summary>"commitments" or "evaluation periods": the kind of calendar the step names.</summary>
    public static void CalendarKind(GoalCalendarView calendar, string word) =>
        Assert.Equal(
            word.StartsWith("commitment", StringComparison.Ordinal) ? GoalScheduleKind.CommitmentBased : GoalScheduleKind.PeriodBased,
            calendar.Kind);

    // ------------------------------------------------------------ helpers

    public static Measurement MeasurementOf(string text) => text switch
    {
        "yes/no" => Measurement.YesNo,
        "count" => Measurement.Count,
        _ => Measurement.Duration,
    };

    private static void AssertDays(IReadOnlySet<DayOfWeek> expected, IReadOnlySet<DayOfWeek>? actual)
    {
        Assert.NotNull(actual);
        Assert.True(
            expected.SetEquals(actual),
            $"Expected the days {string.Join(", ", expected.Order())}, got {string.Join(", ", actual.Order())}.");
    }

    private static void AssertNoSchedule(GoalStartingValues values)
    {
        Assert.Null(values.ScheduleKind);
        Assert.True(values.Days is null || values.Days.Count == 0, "The starting values have days.");
        Assert.Null(values.Times);
        Assert.Null(values.Period);
    }
}
