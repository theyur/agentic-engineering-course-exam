using HT.AcceptanceTests.Support.F004;
using HT.Application.Goals;
using HT.Application.Habits;

namespace HT.AcceptanceTests.StepDefinitions.F004;

/// <summary>"the result is "…"": the named refusal of the last use case of the When step, with its literal text.</summary>
[Binding]
[Scope(Tag = "F004")]
public sealed class ResultSteps(F004Context context)
{
    [Then(@"^the result is ""([^""]*)""$")]
    public void ThenTheResultIs(string message)
    {
        var result = context.LastResult;
        Assert.False(result.IsSuccess, $"The use case succeeded ({result.Outcome}) instead of giving \"{message}\".");
        Assert.Equal(message, result.Message);
    }

    [Then(@"^the result is ""([^""]*)"" and no goal is returned$")]
    public void ThenTheResultIsAndNoGoalIsReturned(string message)
    {
        ThenTheResultIs(message);
        switch (context.LastResult.Raw)
        {
            case GoalStartResult start:
                Assert.Null(start.StartingValues);
                break;
            case GoalCreateResult create:
                Assert.Null(create.GoalId);
                break;
            case GoalListResult list:
                Assert.Empty(list.Goals);
                break;
            case GoalReadResult read:
                Assert.Null(read.Goal);
                break;
            case GoalCalendarResult calendar:
                Assert.Null(calendar.Calendar);
                break;
            case CatalogBrowseResult browse:
                Assert.Null(browse.TrackedCount);
                Assert.All(browse.Habits, habit => Assert.Null(habit.GoalCount));
                break;
            default:
                throw new InvalidOperationException($"{context.LastResult.Raw.GetType().Name} is not a goal use case result.");
        }
    }
}
