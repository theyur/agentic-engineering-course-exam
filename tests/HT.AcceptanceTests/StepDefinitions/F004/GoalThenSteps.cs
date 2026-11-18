using System.Text.RegularExpressions;
using HT.AcceptanceTests.Support.F004;
using HT.Application.Goals;
using HT.Domain.Goals;
using HT.Domain.Habits;

namespace HT.AcceptanceTests.StepDefinitions.F004;

/// <summary>The Then steps of F004 about starting values, created goals, goal lists and goal reads.</summary>
[Binding]
[Scope(Tag = "F004")]
public sealed class GoalThenSteps(F004Context context, F004Driver driver, F004Arrange arrange)
{
    // ------------------------------------------------------------ starting values (FR-007)

    [Then(@"^the starting values (?:are:? |have )(.+)$")]
    public void ThenTheStartingValues(string facts)
    {
        var start = context.LastStart ?? throw new InvalidOperationException("No new goal was started in the When step.");
        Assert.Equal(GoalStartOutcome.Started, start.Outcome);
        Assert.NotNull(start.StartingValues);
        F004Assert.StartingValues(start.StartingValues, facts, CatalogIdOf);
    }

    // ------------------------------------------------------------ creating (FR-001, FR-006)

    [Then(@"^creating succeeds$")]
    [Then(@"^the result is creating succeeds$")]
    [Then(@"^creating gives creating succeeds$")]
    public void ThenCreatingSucceeds()
    {
        AssertCreated(context.LastCreate);
    }

    [Then(@"^creating gives ""([^""]*)""$")]
    public void ThenCreatingGives(string message)
    {
        var created = context.LastCreate ?? throw new InvalidOperationException("No goal was created in the When step.");
        Assert.NotEqual(GoalCreateOutcome.Created, created.Outcome);
        Assert.Equal(message, created.Message);
        Assert.Null(created.GoalId);
    }

    [Then(@"^the result is creating succeeds, and reading the goal gives that name as its own$")]
    public async Task ThenCreatingSucceedsWithThatNameAsItsOwn()
    {
        AssertCreated(context.LastCreate);
        var goal = await arrange.ReadGoalAsync(context.CurrentGoal);
        Assert.Equal(context.LastSentGoalName, goal.Name);
        Assert.True(goal.HasOwnName);
    }

    [Then(@"^the result is creating succeeds, and reading the goal gives the text of (\d+) characters as its own name$")]
    public async Task ThenCreatingSucceedsWithTheTextAsItsOwnName(int length)
    {
        AssertCreated(context.LastCreate);
        var goal = await arrange.ReadGoalAsync(context.CurrentGoal);
        Assert.Equal(F004Text.TextOfLength(length), goal.Name);
        Assert.True(goal.HasOwnName);
    }

    [Then(@"^both creations succeed with identifiers different from each other and from the first goal$")]
    public void ThenBothCreationsSucceed()
    {
        Assert.Equal(2, context.WhenCreations.Count);
        foreach (var created in context.WhenCreations)
        {
            AssertCreated(created);
        }

        var ids = context.WhenCreations.Select(created => created.GoalId!.Value).ToList();
        Assert.NotEqual(ids[0], ids[1]);
        var first = context.Goals[0].Id;
        Assert.DoesNotContain(first, ids);
    }

    [Then(@"^every creation succeeds$")]
    public void ThenEveryCreationSucceeds()
    {
        Assert.NotEmpty(context.SeedCreations);
        foreach (var (start, created) in context.SeedCreations)
        {
            Assert.Equal(GoalStartOutcome.Started, start.Outcome);
            AssertCreated(created);
        }
    }

    // ------------------------------------------------------------ reading a goal (FR-016)

    [Then(@"^reading the (goal|first goal|second goal)(?: still)? gives (.+)$")]
    public async Task ThenReadingTheGoalGives(string which, string facts)
    {
        var record = which switch
        {
            "first goal" => context.Goals[0],
            "second goal" => context.Goals[1],
            _ => context.CurrentGoal,
        };
        var goal = await arrange.ReadGoalAsync(record);
        F004Assert.Goal(goal, facts, record.Id);
    }

    [Then(@"^the result has (.+)$")]
    [Then(@"^reading gives (.+)$")]
    public void ThenTheReadResultHas(string facts)
    {
        F004Assert.Goal(LastReadGoal(), facts, ReadGoalId());
    }

    [Then(@"^((?:Build|Reduce), .+)$")]
    [Then(@"^(no first week starts date)$")]
    [Then(@"^(the time zone `[^`]+` from \d{4}-\d{2}-\d{2} and .+)$")]
    [Then(@"^(attempt \d+, started on \d{4}-\d{2}-\d{2})$")]
    public void ThenTheReadResultAlsoHas(string facts)
    {
        F004Assert.Goal(LastReadGoal(), facts, ReadGoalId());
    }

    [Then(@"^(`[^`]+`) reading their goal gives (.+)$")]
    public async Task ThenOwnerReadingTheirGoalGives(string owner, string facts)
    {
        var record = context.GoalOf(F004Text.Email(owner));
        F004Assert.Goal(await arrange.ReadGoalAsync(record), facts, record.Id);
    }

    [Then(@"^both goals still exist for their owners$")]
    public async Task ThenBothGoalsStillExist()
    {
        Assert.Equal(2, context.Goals.Count);
        Assert.NotEqual(context.Goals[0].OwnerEmail, context.Goals[1].OwnerEmail);
        foreach (var record in context.Goals)
        {
            var goal = await arrange.ReadGoalAsync(record);
            Assert.Equal(record.Id, goal.Id);
        }
    }

    // ------------------------------------------------------------ goal lists (FR-014, FR-015)

    [Then(@"^(`[^`]+`)'s goal list (.+)$")]
    public async Task ThenTheirGoalList(string owner, string claim)
    {
        var email = F004Text.Email(owner);
        if (F004Text.DemoEmails.Contains(email))
        {
            await arrange.DemoAccountAsync(email);
        }

        var goals = await arrange.ListGoalsAsync(email);
        Match match;
        if (claim == "is empty")
        {
            Assert.Empty(goals);
        }
        else if ((match = Regex.Match(claim, "^contains the goal with the name \"([^\"]+)\"$")).Success)
        {
            var created = context.CurrentGoal;
            var item = Assert.Single(goals, goal => goal.Id == created.Id);
            Assert.Equal(match.Groups[1].Value, item.Name);
        }
        else if (claim == "has one goal per creation")
        {
            var created = context.SeedCreations.Select(pair => pair.Create?.GoalId).ToList();
            Assert.NotEmpty(created);
            Assert.All(created, id => Assert.NotNull(id));
            Assert.Equal(created.Count, goals.Count);
            Assert.True(
                created.Select(id => id!.Value).ToHashSet().SetEquals(goals.Select(goal => goal.Id)),
                "The goal list does not have exactly the goals created.");
        }
        else if ((match = Regex.Match(claim, "^is, in this order: (.+)$")).Success)
        {
            AssertDemoList(goals, match.Groups[1].Value);
        }
        else if ((match = Regex.Match(claim, "^is exactly (.+?)(, in this order)?$")).Success)
        {
            var expected = F004Text.QuotedTexts(match.Groups[1].Value);
            Assert.NotEmpty(expected);
            var names = goals.Select(goal => goal.Name).ToList();
            if (match.Groups[2].Success)
            {
                Assert.Equal(expected, names);
            }
            else
            {
                Assert.Equal(expected.Order(StringComparer.Ordinal), names.Order(StringComparer.Ordinal));
            }
        }
        else
        {
            throw new InvalidOperationException($"Cannot check that the goal list {claim}.");
        }
    }

    [Then(@"^the list is, in this order:$")]
    public void ThenTheListIsInThisOrder()
    {
        var list = LastList();
        Assert.NotEmpty(list.Goals);
        context.ListCursor = 0;
    }

    [Then(@"^""([^""]+)"": (.+)$")]
    public void ThenTheNextListEntryIs(string name, string facts)
    {
        var list = LastList();
        Assert.True(context.ListCursor < list.Goals.Count, $"The list has no entry {context.ListCursor + 1}.");
        var item = list.Goals[context.ListCursor++];
        Assert.Equal(name, item.Name);
        F004Assert.ListItem(item, facts);
    }

    [Then(@"^each entry has its goal's identifier$")]
    public void ThenEachEntryHasItsGoalsIdentifier()
    {
        var list = LastList();
        var owner = context.LastListEmail ?? throw new InvalidOperationException("The list has no owner.");
        var created = context.Goals.Where(goal => goal.OwnerEmail == owner).Select(goal => goal.Id).ToHashSet();
        Assert.Equal(context.ListCursor, list.Goals.Count);
        Assert.Equal(list.Goals.Count, list.Goals.Select(goal => goal.Id).Distinct().Count());
        Assert.True(created.SetEquals(list.Goals.Select(goal => goal.Id)), "The entries are not the goals the user created.");
    }

    [Then(@"^the list is empty$")]
    public void ThenTheListIsEmpty()
    {
        Assert.Empty(LastList().Goals);
    }

    [Then(@"^the list is exactly ""([^""]+)""$")]
    public void ThenTheListIsExactly(string name)
    {
        var item = Assert.Single(LastList().Goals);
        Assert.Equal(name, item.Name);
    }

    [Then(@"^the list contains ""([^""]+)""$")]
    public void ThenTheListContains(string name)
    {
        Assert.Contains(LastList().Goals, goal => goal.Name == name);
    }

    [Then(@"^the names are (.+), in this order$")]
    public void ThenTheNamesAre(string names)
    {
        var expected = F004Text.QuotedTexts(names);
        Assert.NotEmpty(expected);
        Assert.Equal(expected, LastList().Goals.Select(goal => goal.Name).ToList());
    }

    [Then(@"^the list gives the goal (.+)$")]
    [Then(@"^the goal gives (.+)$")]
    public void ThenTheListGivesTheGoal(string facts)
    {
        var goal = context.CurrentGoal;
        var item = Assert.Single(LastList().Goals, entry => entry.Id == goal.Id);
        F004Assert.ListItem(item, facts);
    }

    [Then(@"^""([^""]+)"" gives (.+)$")]
    public void ThenTheNamedEntryGives(string name, string facts)
    {
        var item = Assert.Single(LastList().Goals, entry => entry.Name == name);
        F004Assert.ListItem(item, facts);
    }

    [Then(@"^listed at `([^`]+)`, ""([^""]+)"" gives (.+)$")]
    public async Task ThenListedAtTheNamedEntryGives(string time, string name, string facts)
    {
        driver.SetNow(F004Text.Instant(time));
        var owner = context.LastListEmail ?? context.CurrentRegisteredUserEmail
            ?? throw new InvalidOperationException("No account listed its goals.");
        var goals = await arrange.ListGoalsAsync(owner);
        var item = Assert.Single(goals, entry => entry.Name == name);
        F004Assert.ListItem(item, facts);
    }

    [Then(@"^the goal list has (\d+) goals on `([^`]+)`: (.+)$")]
    public async Task ThenTheGoalListHasGoalsOn(int count, string habit, string names)
    {
        var owner = context.CurrentRegisteredUserEmail ?? throw new InvalidOperationException("No registered user was given.");
        var onHabit = (await arrange.ListGoalsAsync(owner)).Where(goal => goal.HabitName == habit).ToList();
        Assert.Equal(count, onHabit.Count);

        var expected = new List<string>();
        foreach (Match item in Regex.Matches(names, "(one|two|three) named \"([^\"]+)\""))
        {
            var times = item.Groups[1].Value switch { "one" => 1, "two" => 2, _ => 3 };
            expected.AddRange(Enumerable.Repeat(item.Groups[2].Value, times));
        }

        Assert.Equal(count, expected.Count);
        Assert.Equal(expected.Order(StringComparer.Ordinal), onHabit.Select(goal => goal.Name).Order(StringComparer.Ordinal));
    }

    // ------------------------------------------------------------ helpers

    private void AssertDemoList(IReadOnlyList<GoalListItem> goals, string text)
    {
        var each = Regex.Match(text, ", each (Commitment-based|Period-based)$");
        var entries = Regex.Matches(each.Success ? text[..each.Index] : text, "\"([^\"]+)\"(?: \\(([^)]*)\\))?");
        Assert.NotEmpty(entries);
        Assert.Equal(entries.Count, goals.Count);
        for (var index = 0; index < entries.Count; index++)
        {
            Assert.Equal(entries[index].Groups[1].Value, goals[index].Name);
            if (entries[index].Groups[2].Success)
            {
                F004Assert.ListItem(goals[index], entries[index].Groups[2].Value);
            }

            if (each.Success)
            {
                F004Assert.ListItem(goals[index], each.Groups[1].Value);
            }
        }
    }

    private static void AssertCreated(GoalCreateResult? created)
    {
        Assert.NotNull(created);
        Assert.Equal(GoalCreateOutcome.Created, created.Outcome);
        Assert.NotNull(created.GoalId);
    }

    private GoalListResult LastList()
    {
        var list = context.LastList ?? throw new InvalidOperationException("No goals were listed in the When step.");
        Assert.Equal(GoalListOutcome.Found, list.Outcome);
        return list;
    }

    private GoalDetails LastReadGoal()
    {
        var read = context.LastRead ?? throw new InvalidOperationException("No goal was read in the When step.");
        Assert.Equal(GoalReadOutcome.Found, read.Outcome);
        return read.Goal ?? throw new InvalidOperationException("Reading the goal gave no goal.");
    }

    /// <summary>The identifier the When step asked to read ("the goal's identifier").</summary>
    private GoalId? ReadGoalId() => context.LastReadGoalId;

    private HabitId? CatalogIdOf(string name) =>
        context.CatalogHabits.FirstOrDefault(habit => habit.Names.Contains(name, StringComparer.Ordinal))?.Id;
}
