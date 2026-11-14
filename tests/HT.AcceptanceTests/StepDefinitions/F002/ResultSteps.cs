using HT.AcceptanceTests.Support.F002;
using HT.Application.Habits;

namespace HT.AcceptanceTests.StepDefinitions.F002;

/// <summary>
/// "the result" of a When step that ran one use case: its named outcome and its literal text (ADR 0005),
/// and for a suggested starting goal the position it is about (FR-007).
/// </summary>
[Binding]
[Scope(Tag = "F002")]
public sealed class ResultSteps(F002Context context)
{
    [Then(@"^the result is success$")]
    public void ThenTheResultIsSuccess()
    {
        var last = context.LastResult;
        Assert.Equal(last.SuccessOutcome, last.Outcome);
    }

    [Then(@"^both results are success$")]
    public void ThenBothResultsAreSuccess()
    {
        Assert.Equal(2, context.Results.Count);
        Assert.All(context.Results, result => Assert.Equal(result.SuccessOutcome, result.Outcome));
    }

    [Then(@"^the result is ""([^""]*)""$")]
    public void ThenTheResultIs(string text)
    {
        var last = context.LastResult;
        Assert.Equal(text, last.Message);
        Assert.Equal(F002Text.OutcomeFor(last.Outcome.GetType(), text, aboutSuggestedStartingGoal: false), last.Outcome);
    }

    [Then(@"^the result is ""([^""]*)"" with the position (\d+)$")]
    public void ThenTheResultIsWithThePosition(string text, int position)
    {
        var last = context.LastResult;
        Assert.Equal(text, last.Message);
        Assert.Equal(F002Text.OutcomeFor(last.Outcome.GetType(), text, aboutSuggestedStartingGoal: true), last.Outcome);
        Assert.Equal(position, last.Position);
    }

    /// <summary>The refusal, and no custom habit in the result (FR-025: an administrator gets no custom habit).</summary>
    [Then(@"^the result is the refusal ""([^""]*)"" and carries no custom habit$")]
    public void ThenTheResultIsTheRefusalAndCarriesNoCustomHabit(string text)
    {
        var last = context.LastResult;
        Assert.Equal(text, last.Message);
        Assert.Equal(F002Text.OutcomeFor(last.Outcome.GetType(), text, aboutSuggestedStartingGoal: false), last.Outcome);
        switch (last.Raw)
        {
            case CustomHabitListResult list:
                Assert.Empty(list.Habits ?? []);
                break;
            case CustomHabitResult read:
                Assert.Null(read.Habit);
                break;
            case CustomHabitCreateResult created:
                Assert.Null(created.HabitId);
                break;
            case CustomHabitEditResult or CustomHabitDeleteResult:
                // These results have no field that could carry a custom habit; the outcome and text are the refusal.
                break;
            default:
                throw new InvalidOperationException($"{last.Raw.GetType().Name} is not a custom-habit result.");
        }
    }
}
