using System.Text.RegularExpressions;
using HT.Application.Habits;
using HT.Domain.Habits;

namespace HT.AcceptanceTests.Support.F002;

/// <summary>Assertions shared by the F002 Then steps.</summary>
public static class F002Assert
{
    /// <summary>
    /// The habit has every value a clause such as "the category Body, the intent Build, the icon `moon`,
    /// no description and no suggested starting goal" gives.
    /// </summary>
    public static void HabitMatches(
        string clause,
        Category category,
        Intent intent,
        string icon,
        string? description,
        SuggestedStartingGoalView? marked)
    {
        var segments = F002Text.SplitClause(clause);
        Assert.NotEmpty(segments);
        foreach (var segment in segments)
        {
            Match match;
            if ((match = Regex.Match(segment, @"^the category (\w+)$")).Success)
            {
                Assert.Equal(F002Text.ParseCategory(match.Groups[1].Value), category);
            }
            else if ((match = Regex.Match(segment, @"^the intent (\w+)$")).Success)
            {
                Assert.Equal(F002Text.ParseIntent(match.Groups[1].Value), intent);
            }
            else if ((match = Regex.Match(segment, "^the icon `([^`]+)`$")).Success)
            {
                Assert.Equal(match.Groups[1].Value, icon);
            }
            else if ((match = Regex.Match(segment, "^the description \"(.*)\"$")).Success)
            {
                Assert.Equal(match.Groups[1].Value, description);
            }
            else if (segment == "no description")
            {
                Assert.Null(description);
            }
            else if ((match = Regex.Match(segment, "^the suggested starting goal with the text \"(.*)\" and the kind \"(.*)\"$")).Success)
            {
                Assert.NotNull(marked);
                Assert.Equal(match.Groups[1].Value, marked.Text);
                Assert.Equal(match.Groups[2].Value, marked.Kind);
            }
            else if ((match = Regex.Match(segment, "^the suggested starting goal of text \"(.*)\"$")).Success)
            {
                Assert.NotNull(marked);
                Assert.Equal(match.Groups[1].Value, marked.Text);
            }
            else if (segment == "no suggested starting goal")
            {
                Assert.Null(marked);
            }
            else
            {
                throw new InvalidOperationException($"\"{segment}\" is not a habit value the F002 steps know.");
            }
        }
    }

    /// <summary>The suggested starting goal has the fields of FR-007 that the step gave.</summary>
    public static void GoalFields(GoalSpec expected, SuggestedStartingGoalView actual, bool includingUnit = true)
    {
        Assert.Equal(expected.BuildOrReduce, actual.BuildOrReduce);
        Assert.Equal(expected.Measurement, actual.Measurement);
        Assert.Equal(expected.Amount, actual.Amount);
        if (includingUnit)
        {
            Assert.Equal(expected.CountUnit, actual.CountUnit);
            Assert.Equal(expected.DurationUnit, actual.DurationUnit);
        }

        Assert.Equal(expected.ScheduleKind, actual.ScheduleKind);
        Assert.Equal(expected.Days.Order(), actual.Days.Order());
        Assert.Equal(expected.Times, actual.Times);
        Assert.Equal(expected.Period, actual.Period);
    }

    /// <summary>The goals are those given, in the order given, and only the given one is marked.</summary>
    public static void GoalsInOrder(
        IReadOnlyList<GoalSpec> expected,
        IReadOnlyList<SuggestedStartingGoalView> actual,
        SuggestedStartingGoalView? marked)
    {
        Assert.Equal(expected.Count, actual.Count);
        for (var index = 0; index < expected.Count; index++)
        {
            GoalFields(expected[index], actual[index]);
            Assert.Equal(expected[index].IsMarked, actual[index].IsMarked);
        }

        var markedIndex = expected.ToList().FindIndex(goal => goal.IsMarked);
        Assert.NotNull(marked);
        Assert.True(marked.IsMarked);
        GoalFields(expected[markedIndex], marked);
    }
}
