using System.Text.RegularExpressions;
using HT.AcceptanceTests.Support.F005;
using HT.Application.Accounts;
using HT.Application.Goals;
using HT.Application.Habits;
using HT.Application.Operator;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;

namespace HT.AcceptanceTests.StepDefinitions.F005;

/// <summary>The steps of F005 about demo loading and demo history loading (FR-030, FR-031).</summary>
[Binding]
[Scope(Tag = "F005")]
public sealed class DemoSteps(F005Context context, F005Driver driver, F005Arrange arrange, F005Checks checks)
{
    private const string Date = @"(\d{4}-\d{2}-\d{2})";
    /// <summary>The optional "at `T` " prefix; the group always takes part (empty when the step gives no time).</summary>
    private const string At = "((?:at `[^`]+` )?)";

    /// <summary>The demo account with history that the demo items' steps about "each demo goal" name (AC-051).</summary>
    private const string Demo4 = "demo4@example.com";

    // ------------------------------------------------------------ Given

    [Given(@"^the AC database is reset, so no account and no habit exists$")]
    public Task GivenTheAcDatabaseIsReset() => F005Driver.ResetDatabaseAsync();

    [Given("^" + At + "demo loading ran$")]
    public async Task GivenDemoLoadingRan(string time)
    {
        await arrange.BeginStepAsync(F005Text.AtTime(time));
        context.LastDemoLoading = await driver.LoadDemoDataAsync(F005Text.DemoPassword);
        Assert.Equal(DemoLoadingOutcome.Loaded, context.LastDemoLoading.Outcome);
    }

    [Given("^demo loading ran, so the seed catalog was not loaded into the catalog, which was not empty$")]
    public async Task GivenDemoLoadingRanOnACatalogThatWasNotEmpty()
    {
        await arrange.BeginStepAsync(null);
        var loaded = await driver.LoadDemoDataAsync(F005Text.DemoPassword);
        Assert.Equal(DemoLoadingOutcome.Loaded, loaded.Outcome);
        Assert.NotNull(loaded.Catalog);
        Assert.Equal(DemoCatalogOutcome.CatalogNotEmpty, loaded.Catalog.Outcome);
        context.LastDemoLoading = loaded;
    }

    /// <summary>
    /// The parenthesis ("R is …; the goal starts …") explains the dates; the Then steps check them. Its group always
    /// takes part in the match (empty when the step has no parenthesis).
    /// </summary>
    [Given("^" + At + @"demo loading and then demo history loading ran((?: \(.+\))?)$")]
    public async Task GivenDemoLoadingAndDemoHistoryLoadingRan(string time, string explanation)
    {
        _ = explanation;
        await arrange.BeginStepAsync(F005Text.AtTime(time));
        var loaded = await driver.LoadDemoDataAsync(F005Text.DemoPassword);
        Assert.Equal(DemoLoadingOutcome.Loaded, loaded.Outcome);
        var history = await driver.LoadDemoHistoryAsync(F005Text.DemoPassword);
        Assert.Equal(DemoHistoryLoadingOutcome.Loaded, history.Outcome);
        context.LastDemoLoading = loaded;
        context.LastDemoHistory = history;
    }

    // ------------------------------------------------------------ When

    [When("^" + At + "demo history loading runs$")]
    public async Task WhenDemoHistoryLoadingRuns(string time)
    {
        await arrange.BeginWhenAsync(F005Text.AtTime(time));
        context.LastDemoHistory = await driver.LoadDemoHistoryAsync(F005Text.DemoPassword);
    }

    [When("^" + At + "demo loading runs and then demo history loading runs(?: again)?$")]
    public async Task WhenDemoLoadingAndDemoHistoryLoadingRun(string time)
    {
        await arrange.BeginWhenAsync(F005Text.AtTime(time));
        context.LastDemoLoading = await driver.LoadDemoDataAsync(F005Text.DemoPassword);
        context.LastDemoHistory = await driver.LoadDemoHistoryAsync(F005Text.DemoPassword);
    }

    [When("^demo history loading runs with the demo password (absent|`[^`]*`)(?:, where absent means that no value is supplied)?$")]
    public async Task WhenDemoHistoryLoadingRunsWithThePassword(string password)
    {
        await arrange.BeginWhenAsync(null);
        context.LastDemoHistory = await driver.LoadDemoHistoryAsync(password == "absent" ? null : F005Text.TextOf(password));
    }

    // ------------------------------------------------------------ Then: results

    /// <summary>
    /// The ending group always takes part in the match: empty (AC-060), ", and no other result" (AC-051) or ", and
    /// sign-in with `e` and `p` succeeds" (AC-069). The exact comparison of the result lines covers "no other result"
    /// in every form, since <see cref="F005Arrange.Lines"/> gives every line of the result.
    /// </summary>
    [Then("^its results are, in this order, (.+?), each for `([^`]+)`((?:, and no other result|, and sign-in with `[^`]+` and `[^`]+` succeeds)?)$")]
    public async Task ThenItsResultsAreInThisOrder(string texts, string email, string rest)
    {
        await arrange.AtThenTimeAsync();
        var expected = Regex.Matches(texts, "\"([^\"]+)\"").Select(item => ((string?)email, item.Groups[1].Value)).ToList();
        Assert.Equal(3, expected.Count);
        var result = LastHistory();
        Assert.Equal(DemoHistoryLoadingOutcome.Loaded, result.Outcome);
        Assert.Equal(expected, F005Arrange.Lines(result));

        if (rest.Length == 0 || rest == ", and no other result")
        {
            return;
        }

        var signIn = Regex.Match(rest, "^, and sign-in with `([^`]+)` and `([^`]+)` succeeds$");
        if (!signIn.Success)
        {
            throw new InvalidOperationException($"Cannot read \"{rest}\".");
        }

        await arrange.SignInAsync(signIn.Groups[1].Value, signIn.Groups[2].Value);
    }

    [Then("^(?:its|demo history loading's) results are \"already exists\" for `([^`]+)`, and no goal or record result$")]
    public async Task ThenItsResultsAreAlreadyExists(string email)
    {
        await arrange.AtThenTimeAsync();
        var result = LastHistory();
        Assert.Equal(DemoHistoryLoadingOutcome.Loaded, result.Outcome);
        Assert.Equal(Line(email, "already exists"), F005Arrange.Lines(result));
        var account = Assert.Single(result.Accounts);
        Assert.Equal(DemoAccountOutcome.AlreadyExists, account.Outcome);
        Assert.Null(account.Goals);
        Assert.Null(account.Records);
    }

    /// <summary>
    /// The ending group always takes part in the match: empty (AC-061) or ", and demo history loading run afterwards
    /// with the demo password `p` gives "created" for `e`" (AC-069).
    /// </summary>
    [Then("^its result is \"([^\"]+)\", and no other result((?:, and demo history loading run afterwards .+)?)$")]
    public async Task ThenItsResultIs(string message, string afterwards)
    {
        await arrange.AtThenTimeAsync();
        var result = LastHistory();
        Assert.Equal(message, result.Message);
        Assert.Empty(result.Accounts);
        Assert.Equal(Line(null, message), F005Arrange.Lines(result));
        if (afterwards.Length == 0)
        {
            return;
        }

        var match = Regex.Match(afterwards, "^, and demo history loading run afterwards with the demo password `([^`]+)` gives \"created\" for `([^`]+)`$");
        if (!match.Success)
        {
            throw new InvalidOperationException($"Cannot read \"{afterwards}\".");
        }

        await RunAfterwardsAsync(match.Groups[1].Value, match.Groups[2].Value);
    }

    [Then("^demo history loading run afterwards with the demo password `([^`]+)` gives \"created\" for `([^`]+)`$")]
    public async Task ThenDemoHistoryLoadingRunAfterwardsGivesCreated(string password, string email)
    {
        await arrange.AtThenTimeAsync();
        await RunAfterwardsAsync(password, email);
    }

    [Then("^every result names `([^`]+)`$")]
    public async Task ThenEveryResultNames(string email)
    {
        await arrange.AtThenTimeAsync();
        var lines = F005Arrange.Lines(LastHistory());
        Assert.NotEmpty(lines);
        Assert.All(lines, line => Assert.Equal(email, line.Email));
    }

    [Then("^no result contains any of (.+)$")]
    public async Task ThenNoResultContainsAnyOf(string list)
    {
        await arrange.AtThenTimeAsync();
        var words = Regex.Matches(list, "`([^`]+)`").Select(item => item.Groups[1].Value).ToList();
        Assert.NotEmpty(words);
        var lines = F005Arrange.Lines(LastHistory());
        Assert.NotEmpty(lines);
        foreach (var (email, text) in lines)
        {
            foreach (var word in words)
            {
                Assert.DoesNotContain(word, text, StringComparison.Ordinal);
                Assert.DoesNotContain(word, email ?? string.Empty, StringComparison.Ordinal);
            }
        }
    }

    [Then("^demo loading's results name (.+) and no other email, and none names `([^`]+)`$")]
    public async Task ThenDemoLoadingsResultsName(string list, string absent)
    {
        await arrange.AtThenTimeAsync();
        var loading = context.LastDemoLoading ?? throw new InvalidOperationException("Demo loading did not run in this scenario.");
        Assert.Equal(DemoLoadingOutcome.Loaded, loading.Outcome);
        var expected = Regex.Matches(list, "`([^`]+)`").Select(item => item.Groups[1].Value).Order(StringComparer.Ordinal).ToList();
        Assert.NotEmpty(expected);
        Assert.Equal(expected, loading.Accounts.Select(account => account.Email).Order(StringComparer.Ordinal).ToList());

        var texts = new List<string?> { loading.Message, loading.Catalog?.Message };
        foreach (var account in loading.Accounts)
        {
            texts.AddRange([account.Email, account.Message, account.CustomHabits?.Message, account.Goals?.Message]);
        }

        Assert.All(texts, text => Assert.DoesNotContain(absent, text ?? string.Empty, StringComparison.OrdinalIgnoreCase));
    }

    // ------------------------------------------------------------ Then: the demo account with history

    [Then("^sign-in with `([^`]+)` and `([^`]+)` succeeds, as a registered user$")]
    public async Task ThenSignInSucceedsAsARegisteredUser(string email, string password)
    {
        await arrange.AtThenTimeAsync();
        var user = await arrange.SignInAsync(email, password);
        Assert.Equal(Role.RegisteredUser, user.Role);
    }

    [Then("^sign-in with `([^`]+)` and `([^`]+)` succeeds, and its goal list is empty$")]
    public async Task ThenSignInSucceedsWithAnEmptyGoalList(string email, string password)
    {
        await arrange.AtThenTimeAsync();
        await arrange.SignInAsync(email, password);
        Assert.Empty(await arrange.ListGoalsAsync(email));
    }

    [Then("^`([^`]+)`'s goal list has exactly three goals: (.+)$")]
    public async Task ThenTheGoalListHasExactlyThreeGoals(string email, string list)
    {
        await arrange.AtThenTimeAsync();
        var goals = await arrange.ListGoalsAsync(email);
        var items = Regex.Matches(list, "\"([^\"]+)\" \\(([^)]*)\\)");
        Assert.Equal(3, items.Count);
        Assert.Equal(3, goals.Count);
        foreach (Match item in items)
        {
            var goal = Assert.Single(goals, goal => goal.Name == item.Groups[1].Value);
            foreach (var fact in item.Groups[2].Value.Split(", "))
            {
                ListFact(goal, fact);
            }
        }
    }

    [Then("^`([^`]+)`'s goal list still has exactly three goals, each with the start date " + Date + "$")]
    public async Task ThenTheGoalListStillHasThreeGoals(string email, string startDate)
    {
        await arrange.AtThenTimeAsync();
        var goals = await arrange.ListGoalsAsync(email);
        Assert.Equal(3, goals.Count);
        foreach (var goal in goals)
        {
            Assert.Equal(F005Text.Date(startDate), (await arrange.ReadGoalAsync(email, goal.Id)).StartDate);
        }
    }

    [Then("^`([^`]+)`'s goal list is (.+)$")]
    public async Task ThenTheGoalListIs(string email, string what)
    {
        await arrange.AtThenTimeAsync();
        var goals = await arrange.ListGoalsAsync(email);
        if (what == "empty")
        {
            Assert.Empty(goals);
            return;
        }

        var match = Regex.Match(what, "^exactly one goal, on the catalog habit `([^`]+)`, (.+)$");
        Assert.True(match.Success, $"Cannot read \"{what}\".");
        var goal = Assert.Single(goals);
        Assert.Equal(match.Groups[1].Value, goal.HabitName);
        Assert.Equal(HabitKind.Catalog, goal.HabitKind);
        var details = await arrange.ReadGoalAsync(email, goal.Id);
        Assert.Equal(match.Groups[1].Value, details.Habit.Name);
        Assert.Equal(HabitKind.Catalog, details.Habit.Kind);
        foreach (var fact in match.Groups[2].Value.Split(", "))
        {
            DetailFact(details, fact);
        }
    }

    [Then("^reading each demo goal gives: (.+)$")]
    public async Task ThenReadingEachDemoGoalGives(string list)
    {
        await arrange.AtThenTimeAsync();
        var items = F005Text.Items(list);
        Assert.Equal(3, items.Count);
        foreach (var item in items)
        {
            var match = Regex.Match(item, "^\"([^\"]+)\" (.+)$");
            Assert.True(match.Success, $"Cannot read \"{item}\".");
            var goal = await arrange.DemoGoalAsync(Demo4, match.Groups[1].Value);
            var details = await arrange.ReadGoalAsync(Demo4, goal.Id);
            foreach (var fact in match.Groups[2].Value.Split(", "))
            {
                DetailFact(details, fact);
            }
        }
    }

    [Then("^reading each demo goal gives the start date " + Date + " and the time zone `([^`]+)`$")]
    public async Task ThenReadingEachDemoGoalGivesTheStartDate(string startDate, string zone)
    {
        await arrange.AtThenTimeAsync();
        var goals = await arrange.ListGoalsAsync(Demo4);
        Assert.NotEmpty(goals);
        foreach (var goal in goals)
        {
            var details = await arrange.ReadGoalAsync(Demo4, goal.Id);
            Assert.Equal(F005Text.Date(startDate), details.StartDate);
            Assert.Equal(zone, details.TimeZone);
        }
    }

    [Then("^`([^`]+)` has no account name and no custom habits$")]
    public async Task ThenTheAccountHasNoNameAndNoCustomHabits(string email)
    {
        await arrange.AtThenTimeAsync();
        var user = await arrange.AccountAsync(email);
        var profile = await driver.GetProfileAsync(user);
        Assert.Equal(ProfileReadOutcome.Found, profile.Outcome);
        Assert.NotNull(profile.Profile);
        Assert.Null(profile.Profile.Name);
        var habits = await driver.ListCustomHabitsAsync(user);
        Assert.Equal(CustomHabitListOutcome.Found, habits.Outcome);
        Assert.Empty(habits.Habits);
    }

    [Then("^reading the goal gives the start date " + Date + "$")]
    public async Task ThenReadingTheGoalGivesTheStartDate(string startDate)
    {
        await arrange.AtThenTimeAsync();
        var goal = checks.LastRead.Goal ?? throw new InvalidOperationException("The When step read no goal of this scenario.");
        Assert.Equal(F005Text.Date(startDate), (await arrange.ReadGoalAsync(goal.OwnerEmail, goal.Id)).StartDate);
    }

    // ------------------------------------------------------------ helpers

    private DemoHistoryLoadingResult LastHistory() =>
        context.LastDemoHistory ?? throw new InvalidOperationException("Demo history loading did not run in this scenario.");

    private async Task RunAfterwardsAsync(string password, string email)
    {
        var again = await driver.LoadDemoHistoryAsync(password);
        Assert.Equal(DemoHistoryLoadingOutcome.Loaded, again.Outcome);
        var account = Assert.Single(again.Accounts, item => item.Email == email);
        Assert.Equal(DemoAccountOutcome.Created, account.Outcome);
        Assert.Equal("created", account.Message);
    }

    /// <summary>A fact of a goal list item: "catalog habit `H`", "Build", "duration", "Commitment-based", ….</summary>
    private static void ListFact(GoalListItem goal, string fact)
    {
        Match match;
        if ((match = Regex.Match(fact, "^catalog habit `([^`]+)`$")).Success)
        {
            Assert.Equal(match.Groups[1].Value, goal.HabitName);
            Assert.Equal(HabitKind.Catalog, goal.HabitKind);
            return;
        }

        switch (fact)
        {
            case "Build":
                Assert.Equal(BuildOrReduce.Build, goal.BuildOrReduce);
                return;
            case "Reduce":
                Assert.Equal(BuildOrReduce.Reduce, goal.BuildOrReduce);
                return;
            case "yes/no":
                Assert.Equal(Measurement.YesNo, goal.Measurement);
                return;
            case "count":
                Assert.Equal(Measurement.Count, goal.Measurement);
                return;
            case "duration":
                Assert.Equal(Measurement.Duration, goal.Measurement);
                return;
            case "Commitment-based":
                Assert.Equal(GoalScheduleKind.CommitmentBased, goal.ScheduleKind);
                return;
            case "Period-based":
                Assert.Equal(GoalScheduleKind.PeriodBased, goal.ScheduleKind);
                return;
            default:
                throw new InvalidOperationException($"Cannot read the goal fact \"{fact}\".");
        }
    }

    /// <summary>
    /// A fact of a goal read: "Build", "Reduce", "yes/no", "duration 20 minutes", "count 1 `sweet`", "fixed days on
    /// all seven days", "3 times per week".
    /// </summary>
    private static void DetailFact(GoalDetails goal, string fact)
    {
        switch (fact)
        {
            case "Build":
                Assert.Equal(BuildOrReduce.Build, goal.BuildOrReduce);
                return;
            case "Reduce":
                Assert.Equal(BuildOrReduce.Reduce, goal.BuildOrReduce);
                return;
            case "yes/no":
                Assert.Equal(Measurement.YesNo, goal.Measurement);
                return;
        }

        Match match;
        if ((match = Regex.Match(fact, @"^duration (\d+) minutes$")).Success)
        {
            Assert.Equal(Measurement.Duration, goal.Measurement);
            Assert.Equal(F005Text.Int(match.Groups[1].Value), goal.Amount);
            Assert.Equal(DurationUnit.Minutes, goal.DurationUnit);
            return;
        }

        if ((match = Regex.Match(fact, @"^count (\d+) `([^`]+)`$")).Success)
        {
            Assert.Equal(Measurement.Count, goal.Measurement);
            Assert.Equal(F005Text.Int(match.Groups[1].Value), goal.Amount);
            Assert.Equal(match.Groups[2].Value, goal.CountUnit);
            return;
        }

        if ((match = Regex.Match(fact, "^fixed days (.+)$")).Success)
        {
            Assert.Equal(ScheduleKind.FixedDays, goal.ScheduleKind);
            Assert.True(goal.Days.SetEquals(F005Text.Days(match.Groups[1].Value)), $"The goal's days are {string.Join(", ", goal.Days)}.");
            return;
        }

        if ((match = Regex.Match(fact, @"^(\d+) times? per (day|week)$")).Success)
        {
            Assert.Equal(ScheduleKind.Times, goal.ScheduleKind);
            Assert.Equal(F005Text.Int(match.Groups[1].Value), goal.Times);
            Assert.Equal(match.Groups[2].Value == "day" ? SchedulePeriod.Day : SchedulePeriod.Week, goal.Period);
            return;
        }

        throw new InvalidOperationException($"Cannot read the goal fact \"{fact}\".");
    }

    /// <summary>One result line (email or none, text), as <see cref="F005Arrange.Lines"/> gives them.</summary>
    private static List<(string? Email, string Text)> Line(string? email, string text) => [(email, text)];
}
