using HT.AcceptanceTests.Support.F004;
using HT.Application.Accounts;
using HT.Application.Goals;
using HT.Application.Operator;

namespace HT.AcceptanceTests.StepDefinitions.F004;

/// <summary>The F004 steps on demo loading and the demo goals (FR-040).</summary>
[Binding]
[Scope(Tag = "F004")]
public sealed class DemoSteps(F004Context context, F004Driver driver, F004Arrange arrange)
{
    private string? _headingEmail;

    [When(@"^demo loading runs$")]
    public async Task WhenDemoLoadingRuns()
    {
        arrange.AtStepTime(null);
        context.Record(await driver.LoadDemoDataAsync(F004Text.DemoPassword));
    }

    [When(@"^at `([^`]+)` demo loading runs again$")]
    public async Task WhenDemoLoadingRunsAgain(string time)
    {
        // What the first run left, to compare with after the second run ("the same as after the first run").
        foreach (var email in F004Text.DemoEmails)
        {
            await arrange.DemoAccountAsync(email);
            context.DemoGoalNamesBefore[email] = (await arrange.ListGoalsAsync(email)).Select(goal => goal.Name).ToList();
        }

        arrange.AtStepTime(time);
        context.Record(await driver.LoadDemoDataAsync(F004Text.DemoPassword));
    }

    [When(@"^each demo account reads its goals and its profile, and `([^`]+)` reads the Home heading$")]
    public async Task WhenEachDemoAccountReadsItsGoalsAndProfile(string headingEmail)
    {
        arrange.AtStepTime(null);
        foreach (var email in F004Text.DemoEmails)
        {
            var user = await arrange.DemoAccountAsync(email);
            var list = await driver.ListGoalsAsync(user);
            context.Record(list, email);
            var details = new List<GoalDetails>();
            foreach (var item in list.Goals)
            {
                var read = await driver.GetGoalAsync(user, item.Id);
                context.Record(read, item.Id);
                if (read.Goal is not null)
                {
                    details.Add(read.Goal);
                }
            }

            context.DemoGoalDetails[email] = details;
            var profile = await driver.GetProfileAsync(user);
            context.Record(profile);
            context.DemoProfiles[email] = profile;
        }

        _headingEmail = headingEmail;
        context.Record(await driver.GetHomeHeadingAsync(await arrange.DemoAccountAsync(headingEmail)));
    }

    // ------------------------------------------------------------ results

    [Then(@"^its results are ""([^""]*)"", ""([^""]*)"" and ""([^""]*)"" for each of `([^`]+)`, `([^`]+)` and `([^`]+)`, and the catalog result ""([^""]*)""$")]
    public void ThenItsResultsAre(string account, string customHabits, string goals, string first, string second, string third, string catalog)
    {
        var loading = Loading();
        foreach (var email in new[] { first, second, third })
        {
            var result = Assert.Single(loading.Accounts, item => item.Email == email);
            Assert.Equal(account, result.Message);
            Assert.NotNull(result.CustomHabits);
            Assert.Equal(customHabits, result.CustomHabits.Message);
            Assert.NotNull(result.Goals);
            Assert.Equal(goals, result.Goals.Message);
        }

        Assert.NotNull(loading.Catalog);
        Assert.Equal(catalog, loading.Catalog.Message);
    }

    [Then(@"^its account results are ""([^""]*)"" for `([^`]+)`, `([^`]+)` and `([^`]+)`, and it gives no goal result$")]
    public void ThenItsAccountResultsAre(string account, string first, string second, string third)
    {
        var loading = Loading();
        foreach (var email in new[] { first, second, third })
        {
            var result = Assert.Single(loading.Accounts, item => item.Email == email);
            Assert.Equal(account, result.Message);
        }

        Assert.All(loading.Accounts, result => Assert.Null(result.Goals));
    }

    [Then(@"^its goal result is ""([^""]*)"" for each of `([^`]+)`, `([^`]+)` and `([^`]+)`$")]
    public void ThenItsGoalResultIs(string goals, string first, string second, string third)
    {
        var loading = Loading();
        foreach (var email in new[] { first, second, third })
        {
            var result = Assert.Single(loading.Accounts, item => item.Email == email);
            Assert.NotNull(result.Goals);
            Assert.Equal(goals, result.Goals.Message);
        }
    }

    [Then(@"^every goal result names one demo email and the text ""([^""]*)""$")]
    public void ThenEveryGoalResultNamesOneDemoEmail(string text)
    {
        var withGoals = Loading().Accounts.Where(result => result.Goals is not null).ToList();
        Assert.NotEmpty(withGoals);
        foreach (var result in withGoals)
        {
            Assert.Contains(result.Email, F004Text.DemoEmails);
            Assert.Equal(text, result.Goals!.Message);
        }

        Assert.Equal(withGoals.Count, withGoals.Select(result => result.Email).Distinct().Count());
    }

    [Then(@"^no result contains (.+)$")]
    public void ThenNoResultContains(string words)
    {
        var forbidden = F004Text.QuotedTexts(words);
        Assert.NotEmpty(forbidden);
        var loading = Loading();
        var texts = new List<string?> { loading.Message, loading.Catalog?.Message };
        foreach (var result in loading.Accounts)
        {
            texts.AddRange([result.Email, result.Message, result.CustomHabits?.Message, result.Goals?.Message]);
        }

        foreach (var text in texts.Where(text => text is not null))
        {
            foreach (var word in forbidden)
            {
                Assert.DoesNotContain(word, text!, StringComparison.Ordinal);
            }
        }
    }

    // ------------------------------------------------------------ demo goals

    [Then(@"^reading each demo goal gives: (.+)$")]
    public async Task ThenReadingEachDemoGoalGives(string items)
    {
        var all = await DemoGoalsAsync();
        var entries = items.Split("; ");
        Assert.Equal(all.Count, entries.Length);
        foreach (var entry in entries)
        {
            var match = System.Text.RegularExpressions.Regex.Match(entry, "^\"([^\"]+)\" (.+)$");
            Assert.True(match.Success, $"Cannot read the demo goal \"{entry}\".");
            var (email, item) = Assert.Single(all, goal => goal.Item.Name == match.Groups[1].Value);
            var read = await driver.GetGoalAsync(context.Account(email), item.Id);
            Assert.Equal(GoalReadOutcome.Found, read.Outcome);
            Assert.NotNull(read.Goal);
            F004Assert.Goal(read.Goal, match.Groups[2].Value, item.Id);
        }
    }

    [Then(@"^reading each demo goal gives the start date (\d{4}-\d{2}-\d{2}), except ""([^""]+)"" with (\d{4}-\d{2}-\d{2}), and the time zone `([^`]+)`$")]
    public async Task ThenReadingEachDemoGoalGivesTheStartDateAndTimeZone(string date, string exception, string exceptionDate, string timeZone)
    {
        foreach (var goal in await ReadAllDemoGoalsAsync(date, exception, exceptionDate))
        {
            Assert.Equal(timeZone, goal.TimeZone);
        }
    }

    [Then(@"^reading each demo goal gives the same start date as after the first run: (\d{4}-\d{2}-\d{2}), except ""([^""]+)"" with (\d{4}-\d{2}-\d{2})$")]
    public async Task ThenReadingEachDemoGoalGivesTheSameStartDate(string date, string exception, string exceptionDate)
    {
        await ReadAllDemoGoalsAsync(date, exception, exceptionDate);
    }

    [Then(@"^each demo account's goal list is the same as after the first run: (\d+) goals for `([^`]+)`, (\d+) for `([^`]+)` and (\d+) for `([^`]+)`, with the same names$")]
    public async Task ThenEachDemoAccountsGoalListIsTheSame(int firstCount, string first, int secondCount, string second, int thirdCount, string third)
    {
        foreach (var (email, count) in new[] { (first, firstCount), (second, secondCount), (third, thirdCount) })
        {
            var names = (await arrange.ListGoalsAsync(email)).Select(goal => goal.Name).ToList();
            Assert.Equal(count, names.Count);
            Assert.Equal(context.DemoGoalNamesBefore[email], names);
        }
    }

    [Then(@"^(`[^`]+`) reading the goal ""([^""]+)"" gives (.+)$")]
    public async Task ThenReadingTheNamedGoalGives(string owner, string name, string facts)
    {
        var email = F004Text.Email(owner);
        await arrange.DemoAccountAsync(email);
        var item = Assert.Single(await arrange.ListGoalsAsync(email), goal => goal.Name == name);
        var read = await driver.GetGoalAsync(context.Account(email), item.Id);
        Assert.Equal(GoalReadOutcome.Found, read.Outcome);
        Assert.NotNull(read.Goal);
        F004Assert.Goal(read.Goal, facts, item.Id);
    }

    [Then(@"^(`[^`]+`)'s goal ""([^""]+)"" exists, and reading it gives (.+)$")]
    public void ThenTheDemoGoalExistsAndReadingItGives(string owner, string name, string facts)
    {
        var goal = Assert.Single(DemoDetails(owner), details => details.Name == name);
        F004Assert.Goal(goal, facts, goal.Id);
    }

    [Then(@"^(`[^`]+`)'s goal on `([^`]+)` has (.+)$")]
    public void ThenTheDemoGoalOnHabitHas(string owner, string habit, string facts)
    {
        var goal = Assert.Single(DemoDetails(owner), details => details.Habit.Name == habit);
        F004Assert.Goal(goal, facts, goal.Id);
    }

    [Then(@"^(`[^`]+`)'s profile gives the name ""([^""]*)""; the profiles of (`[^`]+`) and (`[^`]+`) give no name$")]
    public void ThenTheDemoProfilesGive(string named, string name, string second, string third)
    {
        Assert.Equal(name, DemoProfile(named).Name);
        foreach (var email in new[] { second, third })
        {
            var profile = DemoProfile(email);
            Assert.True(string.IsNullOrEmpty(profile.Name), $"{F004Text.Email(email)} has the name \"{profile.Name}\".");
        }
    }

    [Then(@"^(`[^`]+`)'s Home heading gives the greeting ""([^""]*)""$")]
    public void ThenTheDemoHomeHeadingGives(string owner, string greeting)
    {
        Assert.Equal(_headingEmail, F004Text.Email(owner));
        var heading = context.LastHomeHeading ?? throw new InvalidOperationException("No Home heading was read in the When step.");
        Assert.Equal(HomeHeadingOutcome.Found, heading.Outcome);
        Assert.NotNull(heading.Heading);
        Assert.Equal(greeting, heading.Heading.Greeting);
    }

    // ------------------------------------------------------------ helpers

    private DemoLoadingResult Loading()
    {
        var loading = context.LastDemoLoading ?? throw new InvalidOperationException("Demo loading did not run in the When step.");
        Assert.Equal(DemoLoadingOutcome.Loaded, loading.Outcome);
        return loading;
    }

    private async Task<List<(string Email, GoalListItem Item)>> DemoGoalsAsync()
    {
        var all = new List<(string, GoalListItem)>();
        foreach (var email in F004Text.DemoEmails)
        {
            await arrange.DemoAccountAsync(email);
            all.AddRange((await arrange.ListGoalsAsync(email)).Select(item => (email, item)));
        }

        Assert.NotEmpty(all);
        return all;
    }

    private async Task<List<GoalDetails>> ReadAllDemoGoalsAsync(string date, string exception, string exceptionDate)
    {
        var goals = new List<GoalDetails>();
        foreach (var (email, item) in await DemoGoalsAsync())
        {
            var read = await driver.GetGoalAsync(context.Account(email), item.Id);
            Assert.Equal(GoalReadOutcome.Found, read.Outcome);
            Assert.NotNull(read.Goal);
            var expected = read.Goal.Name == exception ? exceptionDate : date;
            Assert.Equal(F004Text.Date(expected), read.Goal.StartDate);
            goals.Add(read.Goal);
        }

        Assert.Contains(goals, goal => goal.Name == exception);
        return goals;
    }

    private List<GoalDetails> DemoDetails(string owner) =>
        context.DemoGoalDetails.TryGetValue(F004Text.Email(owner), out var details)
            ? details
            : throw new InvalidOperationException($"{owner} did not read its goals in the When step.");

    private ProfileView DemoProfile(string owner)
    {
        var result = context.DemoProfiles.TryGetValue(F004Text.Email(owner), out var profile)
            ? profile
            : throw new InvalidOperationException($"{owner} did not read its profile in the When step.");
        Assert.Equal(ProfileReadOutcome.Found, result.Outcome);
        return result.Profile ?? throw new InvalidOperationException("Reading the profile gave no profile.");
    }
}
