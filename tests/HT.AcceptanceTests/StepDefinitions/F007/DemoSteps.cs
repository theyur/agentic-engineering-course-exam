using System.Text.RegularExpressions;
using HT.AcceptanceTests.Support.F007;
using HT.Application.Accounts;
using HT.Application.Operator;

namespace HT.AcceptanceTests.StepDefinitions.F007;

/// <summary>The steps of F007 about demo loading, demo history loading and demo report loading (FR-030 to FR-032).</summary>
[Binding]
[Scope(Tag = "F007")]
public sealed class DemoSteps(F007Context context, F007Driver driver, F007Arrange arrange)
{
    private const string T = "(" + F007Text.T + ")";

    // ------------------------------------------------------------ Given

    [Given("^the demo run at " + T + "$")]
    public async Task GivenTheDemoRun(string time)
    {
        await arrange.DemoRunAsync(time);
        Assert.Equal(DemoLoadingOutcome.Loaded, context.DemoLoading!.Outcome);
        Assert.Equal(DemoHistoryLoadingOutcome.Loaded, context.DemoHistory!.Outcome);
        Assert.Equal(DemoReportLoadingOutcome.Loaded, context.DemoReport!.Outcome);
    }

    [Given("^the AC database is reset$")]
    public Task GivenTheAcDatabaseIsReset() => F007Driver.ResetDatabaseAsync();

    [Given("^at " + T + " demo loading and demo history loading run with the demo password `([^`]*)`$")]
    public async Task GivenDemoLoadingAndDemoHistoryLoadingRun(string time, string password)
    {
        await arrange.AtAsync(time);
        context.DemoLoading = await driver.LoadDemoDataAsync(password);
        Assert.Equal(DemoLoadingOutcome.Loaded, context.DemoLoading.Outcome);
        context.DemoHistory = await driver.LoadDemoHistoryAsync(password);
        Assert.Equal(DemoHistoryLoadingOutcome.Loaded, context.DemoHistory.Outcome);
    }

    [Given("^at " + T + " demo loading runs with the demo password `([^`]*)`$")]
    public async Task GivenDemoLoadingRuns(string time, string password)
    {
        await arrange.AtAsync(time);
        context.DemoLoading = await driver.LoadDemoDataAsync(password);
        Assert.Equal(DemoLoadingOutcome.Loaded, context.DemoLoading.Outcome);
    }

    [Given("^the anonymous visitor registered the email `([^`]+)` with the password `([^`]+)`, with no time zone and no name, and registration succeeded$")]
    public Task GivenTheAnonymousVisitorRegistered(string email, string password) => arrange.SetupAsync(async () =>
    {
        var registered = await driver.RegisterAsync(email, password, null, null);
        Assert.Equal(RegistrationOutcome.Registered, registered.Outcome);
    });

    // ------------------------------------------------------------ When

    [When("^the demo run at " + T + "$")]
    public async Task WhenTheDemoRun(string time)
    {
        await arrange.WhenAsync(time);
        await arrange.DemoRunAsync(time);
        context.WhenTime = F007Text.Instant(time);
    }

    [When("^at " + T + " demo report loading runs again with the demo password `([^`]*)`$")]
    public async Task WhenDemoReportLoadingRunsAgain(string time, string password)
    {
        await arrange.WhenAsync(time);
        context.DemoReport = await driver.LoadDemoReportAsync(password);
    }

    /// <summary>"with the demo password `p`", "with no demo password" (no value supplied) or "with the demo password `abc`".</summary>
    [When("^at " + T + " demo report loading runs with (.+)$")]
    public async Task WhenDemoReportLoadingRuns(string time, string password)
    {
        await arrange.WhenAsync(time);
        var text = password.Trim();
        string? value = text == "no demo password"
            ? null
            : Regex.Match(text, "^the demo password `([^`]*)`$") is { Success: true } match
                ? match.Groups[1].Value
                : throw new InvalidOperationException($"Cannot read the demo password \"{password}\".");
        context.DemoReport = await driver.LoadDemoReportAsync(value);
    }

    [When("^at " + T + " demo history loading and demo report loading run with the demo password `([^`]*)`$")]
    public async Task WhenDemoHistoryLoadingAndDemoReportLoadingRun(string time, string password)
    {
        await arrange.WhenAsync(time);
        context.DemoHistory = await driver.LoadDemoHistoryAsync(password);
        context.DemoReport = await driver.LoadDemoReportAsync(password);
    }

    // ------------------------------------------------------------ Then: demo task results

    [Then("^every result of demo history loading names `([^`]+)`$")]
    public async Task ThenEveryResultOfDemoHistoryLoadingNames(string email)
    {
        await arrange.ThenAsync();
        var history = context.DemoHistory ?? throw new InvalidOperationException("Demo history loading did not run.");
        Assert.Equal(DemoHistoryLoadingOutcome.Loaded, history.Outcome);
        var lines = F007Arrange.Lines(history);
        Assert.NotEmpty(lines);
        Assert.All(lines, line => Assert.Equal(email, line.Email));
    }

    [Then("^no result of demo loading or of demo history loading names `([^`]+)`$")]
    public async Task ThenNoResultOfDemoLoadingOrDemoHistoryLoadingNames(string email)
    {
        await arrange.ThenAsync();
        var loading = context.DemoLoading ?? throw new InvalidOperationException("Demo loading did not run.");
        var history = context.DemoHistory ?? throw new InvalidOperationException("Demo history loading did not run.");
        var texts = F007Arrange.Texts(loading);
        texts.AddRange(F007Arrange.Lines(history).SelectMany(line => new[] { line.Email ?? string.Empty, line.Text }));
        Assert.NotEmpty(texts);
        Assert.All(texts, text => Assert.DoesNotContain(email, text, StringComparison.OrdinalIgnoreCase));
    }

    [Then("^the results of demo report loading are exactly, in this order: (.+)$")]
    public async Task ThenTheResultsOfDemoReportLoadingAre(string list)
    {
        await arrange.ThenAsync();
        var expected = Pairs(list);
        Assert.NotEmpty(expected);
        Assert.Equal(expected, F007Arrange.Lines(DemoReport()));
    }

    [Then("^its results are exactly (\\(`[^`]+`, \"[^\"]+\"\\))$")]
    public async Task ThenItsResultsAreExactly(string pair)
    {
        await arrange.ThenAsync();
        var result = DemoReport();
        Assert.Equal(DemoReportLoadingOutcome.Loaded, result.Outcome);
        Assert.Equal(Pairs(pair), F007Arrange.Lines(result));
    }

    [Then("^its results are exactly the one result \"([^\"]+)\"$")]
    public async Task ThenItsResultsAreExactlyTheOneResult(string message)
    {
        await arrange.ThenAsync();
        var result = DemoReport();
        Assert.Equal(message, result.Message);
        Assert.Empty(result.Accounts);
        Assert.Equal([((string?)null, message)], F007Arrange.Lines(result));
    }

    [Then("^no result of demo report loading contains (.+)$")]
    public async Task ThenNoResultOfDemoReportLoadingContains(string list)
    {
        await arrange.ThenAsync();
        var words = F007Text.Quoted(list);
        Assert.NotEmpty(words);
        var lines = F007Arrange.Lines(DemoReport());
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

    // ------------------------------------------------------------ Then: demo accounts

    /// <summary>"at T, for each of `demo1@…`, `demo2@…` and `demo3@…`, every goal of its dashboard has the consistency "N/A", …" (AC-046).</summary>
    [Then("^at " + T + ", for each of (.+), every goal of its dashboard has the consistency \"([^\"]+)\", a current streak with the number (\\d+) and a longest streak with the number (\\d+)$")]
    public async Task ThenEveryGoalOfEachDemoDashboardHas(string time, string emails, string consistency, string current, string longest)
    {
        await arrange.ThenAsync(time);
        var accounts = F007Text.Backticked(emails);
        Assert.NotEmpty(accounts);
        var goals = 0;
        foreach (var email in accounts)
        {
            var dashboard = await arrange.DashboardAsync(email);
            foreach (var goal in dashboard.Goals)
            {
                goals++;
                Assert.Equal(consistency, goal.Stats.ConsistencyText);
                Assert.Equal(F007Text.Int(current), goal.Stats.CurrentStreak);
                Assert.Equal(F007Text.Int(longest), goal.Stats.LongestStreak);
            }
        }

        // FR-031: these accounts have slice 4's demo goals, so the check is not vacuous.
        Assert.True(goals > 0, "The demo accounts' dashboards list no goal.");
    }

    [Then("^sign-in with `([^`]+)` and `([^`]+)` succeeds, and its goals overview lists exactly three goals, in this order: (.+)$")]
    public async Task ThenSignInSucceedsAndTheOverviewListsThreeGoals(string email, string password, string list)
    {
        await arrange.ThenAsync();
        await arrange.SignInAsync(email, password);
        var expected = F007Text.Quoted(list);
        Assert.Equal(3, expected.Count);
        Assert.Equal(expected, (await arrange.OverviewAsync(email)).Goals.Select(goal => goal.Item.Name).ToList());
    }

    [Then("^sign-in with `([^`]+)` and `([^`]+)` succeeds, and that account's goals overview is empty$")]
    public async Task ThenSignInSucceedsAndTheOverviewIsEmpty(string email, string password)
    {
        await arrange.ThenAsync();
        await arrange.SignInAsync(email, password);
        Assert.Empty((await arrange.OverviewAsync(email)).Goals);
    }

    [Then("^sign-in with `([^`]+)` and `([^`]+)` fails$")]
    public async Task ThenSignInFails(string email, string password)
    {
        await arrange.ThenAsync();
        var signIn = await driver.SignInAsync(email, password);
        Assert.Equal(SignInOutcome.Failed, signIn.Outcome);
        Assert.Null(signIn.ActingUser);
    }

    [Then("^at " + T + " `([^`]+)`'s goals overview lists exactly the goals (.+)$")]
    public async Task ThenAtTimeTheGoalsOverviewListsExactlyTheGoals(string time, string email, string list)
    {
        await arrange.ThenAsync(time);
        var expected = F007Text.Quoted(list).Order(StringComparer.Ordinal).ToList();
        Assert.NotEmpty(expected);
        var names = (await arrange.OverviewAsync(email)).Goals.Select(goal => goal.Item.Name).Order(StringComparer.Ordinal).ToList();
        Assert.Equal(expected, names);
    }

    [Then("^`([^`]+)`'s goals overview lists exactly two goals, in this order: (.+)$")]
    public async Task ThenTheGoalsOverviewListsExactlyTwoGoals(string email, string list)
    {
        await arrange.ThenAsync();
        var expected = F007Text.Quoted(list);
        Assert.Equal(2, expected.Count);
        Assert.Equal(expected, (await arrange.OverviewAsync(email)).Goals.Select(goal => goal.Item.Name).ToList());
    }

    // ------------------------------------------------------------ helpers

    private DemoReportLoadingResult DemoReport() =>
        context.DemoReport ?? throw new InvalidOperationException("Demo report loading did not run in this scenario.");

    /// <summary>"(`e`, "t"), (`e`, "u")": the (email, text) pairs, in order.</summary>
    private static List<(string? Email, string Text)> Pairs(string list) =>
        Regex.Matches(list, "\\(`([^`]+)`, \"([^\"]+)\"\\)")
            .Select(match => ((string?)match.Groups[1].Value, match.Groups[2].Value))
            .ToList();
}
