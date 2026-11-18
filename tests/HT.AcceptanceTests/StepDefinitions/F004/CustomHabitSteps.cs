using HT.AcceptanceTests.Support.F004;
using HT.Application.Habits;
using HT.Domain.Habits;

namespace HT.AcceptanceTests.StepDefinitions.F004;

/// <summary>The F004 steps on custom habits that goals use (FR-035).</summary>
[Binding]
[Scope(Tag = "F004")]
public sealed class CustomHabitSteps(F004Context context, F004Driver driver, F004Arrange arrange)
{
    private CustomRecord? _deleted;

    [When(@"^(`[^`]+`) deletes their custom habit `([^`]+)`$")]
    public async Task WhenDeletesTheirCustomHabit(string owner, string habit)
    {
        arrange.AtStepTime(null);
        var email = F004Text.Email(owner);
        _deleted = context.CustomHabit(email, habit);
        context.Record(await driver.DeleteCustomHabitAsync(context.Account(email), _deleted.Id));
    }

    [When(@"^(`[^`]+`) edits their custom habit `([^`]+)` to the name `([^`]+)` and the intent (Build|Reduce)$")]
    public async Task WhenEditsTheirCustomHabit(string owner, string habit, string name, string intent)
    {
        arrange.AtStepTime(null);
        var email = F004Text.Email(owner);
        var record = context.CustomHabit(email, habit);
        record.Spec.Name = name;
        record.Spec.Intent = Enum.Parse<Intent>(intent);
        context.Record(await driver.EditCustomHabitAsync(context.Account(email), record.Id, record.Spec.ToInput()));
        record.Names.Add(name);
    }

    [When(@"^(`[^`]+`) reads their custom habits `([^`]+)` and `([^`]+)`$")]
    public async Task WhenReadsTheirCustomHabits(string owner, string first, string second)
    {
        arrange.AtStepTime(null);
        var email = F004Text.Email(owner);
        foreach (var name in new[] { first, second })
        {
            var read = await driver.GetCustomHabitAsync(context.Account(email), context.CustomHabit(email, name).Id);
            context.Record(read);
            context.CustomHabitReads[name] = read;
        }
    }

    [Then(@"^reading their custom habit `([^`]+)` still gives it$")]
    public async Task ThenReadingTheirCustomHabitStillGivesIt(string habit)
    {
        var record = Deleted(habit);
        var read = await driver.GetCustomHabitAsync(context.Account(record.OwnerEmail), record.Id);
        Assert.Equal(CustomHabitOutcome.Found, read.Outcome);
        Assert.NotNull(read.Habit);
        Assert.Equal(record.Id, read.Habit.Id);
        Assert.Equal(habit, read.Habit.Name);
    }

    [Then(@"^the custom habit is deleted$")]
    public void ThenTheCustomHabitIsDeleted()
    {
        var deleted = context.LastDelete ?? throw new InvalidOperationException("No custom habit was deleted in the When step.");
        Assert.Equal(CustomHabitDeleteOutcome.Deleted, deleted.Outcome);
    }

    [Then(@"^reading it gives ""([^""]*)""$")]
    public async Task ThenReadingItGives(string message)
    {
        var record = _deleted ?? throw new InvalidOperationException("No custom habit was deleted in the When step.");
        var read = await driver.GetCustomHabitAsync(context.Account(record.OwnerEmail), record.Id);
        Assert.Equal(CustomHabitOutcome.NotFound, read.Outcome);
        Assert.Equal(message, read.Message);
        Assert.Null(read.Habit);
    }

    [Then(@"^`([^`]+)` gives the number of goals (\d+) and `([^`]+)` gives (\d+)$")]
    public void ThenTheCustomHabitsGiveTheNumberOfGoals(string first, int firstCount, string second, int secondCount)
    {
        AssertGoalCount(first, firstCount);
        AssertGoalCount(second, secondCount);
    }

    private void AssertGoalCount(string name, int count)
    {
        var read = context.CustomHabitReads.TryGetValue(name, out var known)
            ? known
            : throw new InvalidOperationException($"`{name}` was not read in the When step.");
        Assert.Equal(CustomHabitOutcome.Found, read.Outcome);
        Assert.NotNull(read.Habit);
        Assert.Equal(count, read.Habit.GoalCount);
    }

    private CustomRecord Deleted(string habit) =>
        _deleted is not null && _deleted.Names.Contains(habit, StringComparer.Ordinal)
            ? _deleted
            : throw new InvalidOperationException($"`{habit}` is not the custom habit the When step deleted.");
}
