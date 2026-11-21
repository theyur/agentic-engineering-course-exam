using HT.AcceptanceTests.Support.F008;
using HT.Application.Exemptions;
using HT.Application.Goals;
using HT.Application.Operator;
using HT.Application.Records;

namespace HT.AcceptanceTests.StepDefinitions.F008;

/// <summary>
/// The When steps of F008 that read (a read changes nothing): the history, the pauses, the recent notes, the goal
/// report, the goals overview and the dashboard, including the demo account's reads.
/// </summary>
[Binding]
[Scope(Tag = "F008")]
public sealed class ReadSteps(F008Context context, F008Driver driver, F008Arrange arrange)
{
    private const string D = "(" + F008Text.D + ")";
    private const string T = "(" + F008Text.T + ")";

    [When("^at " + T + " `([^`]+)` reads the history of the goal from " + D + " to " + D + "$")]
    public async Task WhenTheUserReadsTheHistory(string time, string email, string first, string last)
    {
        await StartAsync(time, email);
        context.History = await arrange.HistoryAsync(arrange.OnlyGoalOf(email), F008Text.Date(first), F008Text.Date(last));
        context.ReadKind = F008ReadKind.History;
    }

    [When("^at " + T + " `([^`]+)` reads the history of `([^`]+)` from " + D + " to " + D + "$")]
    public async Task WhenTheUserReadsTheHistoryOf(string time, string email, string goalName, string first, string last)
    {
        await StartAsync(time, email);
        var goal = await arrange.GoalNamedAsync(email, goalName);
        context.History = await arrange.HistoryAsync(goal, F008Text.Date(first), F008Text.Date(last));
        context.ReadKind = F008ReadKind.History;
    }

    [When("^at " + T + " `([^`]+)` reads the pauses of the goal$")]
    public async Task WhenTheUserReadsThePauses(string time, string email)
    {
        await StartAsync(time, email);
        context.PausesRead = await arrange.PausesAsync(arrange.OnlyGoalOf(email));
        context.ReadKind = F008ReadKind.Pauses;
    }

    [When("^at " + T + " `([^`]+)` reads the recent notes of the goal$")]
    public async Task WhenTheUserReadsTheRecentNotes(string time, string email)
    {
        await StartAsync(time, email);
        context.RecentNotes = await arrange.RecentNotesAsync(arrange.OnlyGoalOf(email));
        context.ReadKind = F008ReadKind.RecentNotes;
    }

    [When("^at " + T + " `([^`]+)` reads the report of the goal$")]
    public async Task WhenTheUserReadsTheReport(string time, string email)
    {
        await StartAsync(time, email);
        var goal = arrange.OnlyGoalOf(email);
        context.Reports[goal.Name] = await arrange.ReportAsync(goal);
        context.ReadKind = F008ReadKind.Report;
    }

    [When("^at " + T + " `([^`]+)` reads the report of `([^`]+)` and the report of `([^`]+)`$")]
    public async Task WhenTheUserReadsTwoReports(string time, string email, string first, string second)
    {
        await StartAsync(time, email);
        foreach (var name in new[] { first, second })
        {
            var goal = await arrange.GoalNamedAsync(email, name);
            context.Reports[name] = await arrange.ReportAsync(goal, email);
        }

        context.ReadKind = F008ReadKind.Report;
    }

    [When("^at " + T + " `([^`]+)` reads the goals overview$")]
    public async Task WhenTheUserReadsTheGoalsOverview(string time, string email)
    {
        await StartAsync(time, email);
        context.Overview = await arrange.OverviewAsync(email);
        context.ReadKind = F008ReadKind.Overview;
    }

    [When("^at " + T + " `([^`]+)` reads the dashboard$")]
    public async Task WhenTheUserReadsTheDashboard(string time, string email)
    {
        await StartAsync(time, email);
        context.Dashboard = await arrange.DashboardAsync(email);
        context.ReadKind = F008ReadKind.Dashboard;
    }

    /// <summary>
    /// AC-082: every account that a result of demo loading, demo history loading or demo report loading gives as
    /// created (the named ones among them) signs in with the demo password and reads, for each of its goals, the pauses
    /// and the history of the range.
    /// </summary>
    [When("^at " + T + " each account that a result of demo loading, demo history loading or demo report loading gives as created, (.+) included, signs in and reads the pauses and the history from " + D + " to " + D + " of each of its goals$")]
    public async Task WhenEachEarlierDemoAccountReads(string time, string named, string first, string last)
    {
        await arrange.WhenAsync(time);
        var loading = context.DemoLoading ?? throw new InvalidOperationException("Demo loading has not run.");
        var history = context.DemoHistory ?? throw new InvalidOperationException("Demo history loading has not run.");
        var report = context.DemoReport ?? throw new InvalidOperationException("Demo report loading has not run.");
        var created = loading.Accounts.Where(account => account.Outcome == DemoAccountOutcome.Created).Select(account => account.Email)
            .Concat(history.Accounts.Where(account => account.Outcome == DemoAccountOutcome.Created).Select(account => account.Email))
            .Concat(report.Accounts.Where(account => account.Outcome == DemoAccountOutcome.Created).Select(account => account.Email))
            .Distinct(StringComparer.Ordinal)
            .ToList();
        foreach (var email in F008Text.Backticked(named))
        {
            Assert.Contains(email, created);
        }

        foreach (var email in created)
        {
            var user = await arrange.SignInAsync(email, F008Text.DemoPassword);
            var list = await driver.ListGoalsAsync(user);
            Assert.Equal(GoalListOutcome.Found, list.Outcome);
            foreach (var goal in list.Goals)
            {
                var pauses = await driver.GetPausesAsync(user, goal.Id);
                Assert.Equal(PausesOutcome.Found, pauses.Outcome);
                var read = await driver.GetHistoryAsync(user, goal.Id, F008Text.Date(first), F008Text.Date(last));
                Assert.True(read.Outcome == HistoryOutcome.Found, $"Reading the history of {email}'s goal gave {read.Outcome}: \"{read.Message}\".");
                context.DemoReads.Add((email, goal.Name, pauses.Pauses, read.History!));
            }
        }
    }

    private async Task StartAsync(string time, string email)
    {
        await arrange.WhenAsync(time);
        context.WhenActor = email;
    }
}
