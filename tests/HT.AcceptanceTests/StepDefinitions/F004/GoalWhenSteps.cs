using HT.AcceptanceTests.Support.F004;
using HT.Application.Goals;
using HT.Domain.Goals;
using HT.Domain.Habits;

namespace HT.AcceptanceTests.StepDefinitions.F004;

/// <summary>The When steps of F004 that call the goal use cases: start, create, list, read, calendar.</summary>
[Binding]
[Scope(Tag = "F004")]
public sealed class GoalWhenSteps(F004Context context, F004Driver driver, F004Arrange arrange)
{
    private const string Actor = "(`[^`]+`|the anonymous visitor)";
    /// <summary>The optional "at `T` (…) " prefix; its group always takes part in the match, empty when no time is given.</summary>
    private const string At = F004Text.AtPrefix;
    private const string Date = @"(\d{4}-\d{2}-\d{2})";

    // ------------------------------------------------------------ start a new goal (FR-007)

    [When("^" + At + Actor + " starts a new goal from (`[^`]+`|their custom habit `[^`]+`)$")]
    public async Task WhenStartsANewGoal(string at, string actor, string habit)
    {
        await StartAsync(at, actor, habit, null);
    }

    [When("^" + At + Actor + " starts a new goal from (`[^`]+`|their custom habit `[^`]+`) with the suggested starting goal position (-?\\d+)$")]
    public async Task WhenStartsANewGoalWithThePosition(string at, string actor, string habit, string position)
    {
        await StartAsync(at, actor, habit, F004Text.Int(position));
    }

    [When(@"^(`[^`]+`) starts a new goal from (.+) given as a (catalog|custom) habit$")]
    public async Task WhenStartsFromAHabitGivenAsAKind(string actor, string habit, string kind)
    {
        arrange.AtStepTime(null);
        var email = F004Text.Email(actor);
        Remember(email, await driver.StartNewGoalAsync(context.Account(email), AsKind(habit, email, kind), null));
    }

    [When(@"^(`[^`]+`) creates a goal with (.+?) on (.+) given as a (catalog|custom) habit$")]
    public async Task WhenCreatesOnAHabitGivenAsAKind(string actor, string settings, string habit, string kind)
    {
        arrange.AtStepTime(null);
        var email = F004Text.Email(actor);
        var asKind = AsKind(habit, email, kind);
        context.Record(await arrange.CreateGoalAsync(context.Account(email), email, asKind, F004Text.Settings(settings)));
    }

    private async Task StartAsync(string at, string actor, string habit, int? position)
    {
        arrange.AtStepTime(F004Text.AtTime(at));
        var email = F004Context.ActorEmail(actor);
        var result = await driver.StartNewGoalAsync(context.Actor(actor), arrange.Habit(habit, email), position);
        Remember(email, result);
    }

    private GoalHabit AsKind(string habit, string email, string kind) =>
        new(arrange.Habit(habit, email).Id, kind == "catalog" ? HabitKind.Catalog : HabitKind.Custom);

    [When(@"^at `([^`]+)` (`[^`]+`) starts a new goal from their custom habit `([^`]+)`, and then creates a goal on it with (.+)$")]
    public async Task WhenStartsAndThenCreatesOnIt(string time, string actor, string habit, string settings)
    {
        arrange.AtStepTime(time);
        var email = F004Text.Email(actor);
        var goalHabit = arrange.Habit($"their custom habit `{habit}`", email);
        Remember(email, await driver.StartNewGoalAsync(context.Account(email), goalHabit, null));
        context.Record(await arrange.CreateGoalAsync(context.Account(email), email, goalHabit, F004Text.Settings(settings)));
    }

    [When(@"^(`[^`]+`) starts a new goal with no habit, and then creates a goal with no habit and (.+)$")]
    public async Task WhenStartsAndCreatesWithNoHabit(string actor, string settings)
    {
        arrange.AtStepTime(null);
        var email = F004Text.Email(actor);
        Remember(email, await driver.StartNewGoalAsync(context.Account(email), null, null));
        context.Record(await arrange.CreateGoalAsync(context.Account(email), email, null, F004Text.Settings(settings)));
    }

    [When(@"^for every catalog habit that browsing the catalog without an acting user returns and for every position of its suggested starting goals from 1 to their number, (`[^`]+`) starts a new goal from that habit with that position and creates a goal with the starting values unchanged$")]
    public async Task WhenEverySeedSuggestionIsCreatedUnchanged(string actor)
    {
        arrange.AtStepTime(null);
        var email = F004Text.Email(actor);
        var user = context.Account(email);
        var browse = await driver.BrowseAsync(new HT.Application.Habits.CatalogFilter());
        Assert.Equal(HT.Application.Habits.CatalogBrowseOutcome.Found, browse.Outcome);
        foreach (var habit in browse.Habits)
        {
            var details = await driver.GetDetailsAsync(habit.Id);
            Assert.Equal(HT.Application.Habits.CatalogHabitDetailsOutcome.Found, details.Outcome);
            Assert.NotNull(details.Details);
            for (var position = 1; position <= details.Details.SuggestedStartingGoals.Count; position++)
            {
                var started = await driver.StartNewGoalAsync(user, new GoalHabit(habit.Id, HabitKind.Catalog), position);
                GoalCreateResult? created = null;
                if (started.StartingValues is { } values)
                {
                    created = await arrange.CreateFromStartingValuesAsync(user, email, values);
                }

                context.SeedCreations.Add((started, created));
            }
        }
    }

    // ------------------------------------------------------------ create a goal (FR-006)

    [When("^" + At + Actor + " creates a goal on (`[^`]+`|their custom habit `[^`]+`) with (.+)$")]
    public async Task WhenCreatesAGoal(string at, string actor, string habit, string settings)
    {
        arrange.AtStepTime(F004Text.AtTime(at));
        var email = F004Context.ActorEmail(actor);
        foreach (var (habitPhrase, spec) in F004Arrange.GoalChain(habit, settings))
        {
            context.Record(await arrange.CreateGoalAsync(context.Actor(actor), email, arrange.Habit(habitPhrase, email), spec));
        }
    }

    [When(@"^(`[^`]+`) creates a goal with no habit, (.+)$")]
    public async Task WhenCreatesAGoalWithNoHabit(string actor, string settings)
    {
        arrange.AtStepTime(null);
        var email = F004Text.Email(actor);
        context.Record(await arrange.CreateGoalAsync(context.Account(email), email, null, F004Text.Settings(settings)));
    }

    [When(@"^(`[^`]+`) creates a goal with those starting values unchanged$")]
    public async Task WhenCreatesAGoalWithThoseStartingValues(string actor)
    {
        arrange.AtStepTime(null);
        var email = F004Text.Email(actor);
        var values = context.StartingValues.TryGetValue(email, out var known)
            ? known
            : throw new InvalidOperationException($"`{email}` started no new goal in this scenario.");
        context.Record(await arrange.CreateFromStartingValuesAsync(context.Account(email), email, values));
    }

    // ------------------------------------------------------------ list and read (FR-014 to FR-016)

    [When("^" + At + Actor + " lists their goals$")]
    public async Task WhenListsTheirGoals(string at, string actor)
    {
        arrange.AtStepTime(F004Text.AtTime(at));
        context.Record(await driver.ListGoalsAsync(context.Actor(actor)), F004Context.ActorEmail(actor));
    }

    [When(@"^at `([^`]+)` (`[^`]+`) lists their goals and reads the goal$")]
    public async Task WhenListsTheirGoalsAndReadsTheGoal(string time, string actor)
    {
        arrange.AtStepTime(time);
        var email = F004Text.Email(actor);
        context.Record(await driver.ListGoalsAsync(context.Account(email)), email);
        var goal = arrange.GoalReference("the goal", email);
        context.Record(await driver.GetGoalAsync(context.Account(email), goal), goal);
    }

    [When("^" + At + Actor + " reads (the goal|G|`[^`]+`'s goal|the goal `[^`]+`'s goal|the goal an identifier that the test creates and that belongs to no goal)$")]
    public async Task WhenReadsTheGoal(string at, string actor, string goal)
    {
        arrange.AtStepTime(F004Text.AtTime(at));
        var email = F004Context.ActorEmail(actor);
        var id = arrange.GoalReference(goal, email);
        context.Record(await driver.GetGoalAsync(context.Actor(actor), id), id);
    }

    // ------------------------------------------------------------ evaluation calendar (FR-013)

    [When("^" + At + Actor + " reads the evaluation calendar of (the goal|`[^`]+`'s goal) from " + Date + " to " + Date + "$")]
    public async Task WhenReadsTheEvaluationCalendarOf(string at, string actor, string goal, string first, string last)
    {
        arrange.AtStepTime(F004Text.AtTime(at));
        var email = F004Context.ActorEmail(actor);
        context.Record(await driver.GetCalendarAsync(
            context.Actor(actor), arrange.GoalReference(goal, email), F004Text.Date(first), F004Text.Date(last)));
    }

    [When("^" + Actor + " reads the evaluation calendar from " + Date + " to " + Date + " of (`[^`]+`'s goal|an identifier that the test creates and that belongs to no goal|an identifier that belongs to no goal)$")]
    public async Task WhenReadsTheEvaluationCalendarFrom(string actor, string first, string last, string goal)
    {
        arrange.AtStepTime(null);
        var email = F004Context.ActorEmail(actor);
        context.Record(await driver.GetCalendarAsync(
            context.Actor(actor), arrange.GoalReference(goal, email), F004Text.Date(first), F004Text.Date(last)));
    }

    [When("^at `([^`]+)` (`[^`]+`) reads the goal and reads its evaluation calendar from " + Date + " to " + Date + "$")]
    public async Task WhenReadsTheGoalAndItsCalendar(string time, string actor, string first, string last)
    {
        arrange.AtStepTime(time);
        var email = F004Text.Email(actor);
        var goal = arrange.GoalReference("the goal", email);
        context.Record(await driver.GetGoalAsync(context.Account(email), goal), goal);
        context.Record(await driver.GetCalendarAsync(context.Account(email), goal, F004Text.Date(first), F004Text.Date(last)));
    }

    [When("^at `([^`]+)` (`[^`]+`) reads the evaluation calendar of the goal from " + Date + " to " + Date + " and reads the goal$")]
    public async Task WhenReadsTheCalendarAndTheGoal(string time, string actor, string first, string last)
    {
        arrange.AtStepTime(time);
        var email = F004Text.Email(actor);
        var goal = arrange.GoalReference("the goal", email);
        context.Record(await driver.GetCalendarAsync(context.Account(email), goal, F004Text.Date(first), F004Text.Date(last)));
        context.Record(await driver.GetGoalAsync(context.Account(email), goal), goal);
    }

    private void Remember(string? email, GoalStartResult result)
    {
        context.Record(result);
        if (email is not null && result.StartingValues is { } values)
        {
            context.StartingValues[email] = values;
        }
    }
}
