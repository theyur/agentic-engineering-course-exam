using System.Text.RegularExpressions;
using HT.AcceptanceTests.Support.F007;
using HT.Application.Records;
using HT.Application.Reports;
using HT.Domain.Accounts;
using HT.Domain.Goals;

namespace HT.AcceptanceTests.StepDefinitions.F007;

/// <summary>
/// The When steps of F007 that call this slice's use cases (goal report, goals overview, dashboard), slice 4's goal
/// list, and slice 5's record, change-record and delete-record use cases before a read.
/// </summary>
[Binding]
[Scope(Tag = "F007")]
public sealed class ReadSteps(F007Context context, F007Driver driver, F007Arrange arrange)
{
    private const string D = "(" + F007Text.D + ")";
    private const string T = "(" + F007Text.T + ")";

    private static readonly Regex ReadParts = new(
        "(?:, | and )(?=the (?:report|dashboard|goals overview|goal list))",
        RegexOptions.CultureInvariant);

    /// <summary>
    /// "at T `x` reads …": one or more reads, in the order written ("the report of the first goal, the report of the
    /// second goal and the dashboard", "the report of "A" and of "B"", "the dashboard and the goals overview", …).
    /// </summary>
    [When("^at " + T + " `([^`]+)` reads (.+)$")]
    public async Task WhenTheUserReads(string time, string email, string what)
    {
        await arrange.WhenAsync(time);
        await ReadAsync(email, what);
    }

    [When("^at " + T + " `([^`]+)` records (.+) and reads the report of the goal$")]
    public async Task WhenTheUserRecordsAndReads(string time, string email, string records)
    {
        await arrange.WhenAsync(time);
        await arrange.RecordAllAsync(email, records);
        await ReadAsync(email, "the report of the goal");
    }

    [When("^at " + T + " `([^`]+)` changes the record of " + D + " to (-?\\d+) and reads the report of the goal$")]
    public async Task WhenTheUserChangesARecordAndReads(string time, string email, string date, string amount)
    {
        await arrange.WhenAsync(time);
        var goal = arrange.Goal(email, "the goal");
        var record = await arrange.RecordIdAsync(goal, F007Text.Date(date));
        var changed = await driver.ChangeRecordAsync(
            await arrange.AccountAsync(email),
            goal.Id,
            record,
            new RecordInput { Answer = null, Amount = F007Text.Int(amount), Note = null });
        Assert.Equal(RecordWriteOutcome.Changed, changed.Outcome);
        await ReadAsync(email, "the report of the goal");
    }

    [When("^at " + T + " `([^`]+)` deletes the record of " + D + " and reads the report of the goal$")]
    public async Task WhenTheUserDeletesARecordAndReads(string time, string email, string date)
    {
        await arrange.WhenAsync(time);
        var goal = arrange.Goal(email, "the goal");
        var record = await arrange.RecordIdAsync(goal, F007Text.Date(date));
        var deleted = await driver.DeleteRecordAsync(await arrange.AccountAsync(email), goal.Id, record);
        Assert.Equal(RecordWriteOutcome.Deleted, deleted.Outcome);
        await ReadAsync(email, "the report of the goal");
    }

    /// <summary>"`x` reads the report of the `R` goal and of the `S` goal at T1, and the report of the `C` goal at T2" (AC-027).</summary>
    [When("^`([^`]+)` reads the report of (the `[^`]+` goal) and of (the `[^`]+` goal) at " + T + ", and the report of (the `[^`]+` goal) at " + T + "$")]
    public async Task WhenTheUserReadsReportsAtTwoTimes(string email, string first, string second, string time, string third, string laterTime)
    {
        await arrange.WhenAsync(time);
        await ReadAsync(email, $"the report of {first}");
        await ReadAsync(email, $"the report of {second}");
        await arrange.WhenAsync(laterTime);
        await ReadAsync(email, $"the report of {third}");
    }

    /// <summary>"at T the anonymous visitor (or the administrator `a`) calls U" (AC-041).</summary>
    [When("^at " + T + " (the anonymous visitor|the administrator `[^`]+`) calls (.+)$")]
    public async Task WhenTheActorCalls(string time, string actor, string useCase)
    {
        await arrange.WhenAsync(time);
        var (_, user) = await arrange.ActorAsync(actor);
        var text = useCase.Trim();
        Match match;
        if ((match = Regex.Match(text, "^the goal report for (the goal of `[^`]+`)$")).Success)
        {
            await ReportAsync(user, arrange.Goal(null, match.Groups[1].Value), null);
            return;
        }

        if ((match = Regex.Match(text, "^the goal report for the identifier `([^`]+)`, which belongs to no goal$")).Success)
        {
            await ReportAsync(user, null, new GoalId(Guid.Parse(match.Groups[1].Value)));
            return;
        }

        if (text == "the goals overview")
        {
            await OverviewAsync(user);
            return;
        }

        if (text == "the dashboard")
        {
            await DashboardAsync(user);
            return;
        }

        throw new InvalidOperationException($"\"{useCase}\" is not a use case of this slice.");
    }

    // ------------------------------------------------------------ helpers

    private async Task ReadAsync(string email, string what)
    {
        var user = await arrange.AccountAsync(email);
        var text = what.Trim().Replace(" and of \"", " and the report of \"", StringComparison.Ordinal);
        foreach (var raw in ReadParts.Split(text))
        {
            var part = raw.Trim();
            Match match;
            if (part == "the dashboard")
            {
                await DashboardAsync(user);
            }
            else if (part == "the goals overview")
            {
                await OverviewAsync(user);
            }
            else if (part == "the goal list")
            {
                context.GoalList = await driver.ListGoalsAsync(user);
            }
            else if (part == "the report of each goal")
            {
                var goals = arrange.GoalsOf(email);
                Assert.NotEmpty(goals);
                foreach (var goal in goals)
                {
                    await ReportAsync(user, goal, null);
                }
            }
            else if (part == "the report of each of its goals")
            {
                var items = await arrange.ListGoalsAsync(email);
                Assert.NotEmpty(items);
                foreach (var item in items)
                {
                    await ReportAsync(user, await arrange.GoalByNameAsync(email, item.Name), null);
                }
            }
            else if ((match = Regex.Match(part, "^the report of (?:its goal )?\"([^\"]+)\"$")).Success)
            {
                await ReportAsync(user, await arrange.GoalByNameAsync(email, match.Groups[1].Value), null);
            }
            else if ((match = Regex.Match(part, "^the report of the identifier `([^`]+)`, which belongs to no goal$")).Success)
            {
                await ReportAsync(user, null, new GoalId(Guid.Parse(match.Groups[1].Value)));
            }
            else if ((match = Regex.Match(part, "^the report of (.+)$")).Success)
            {
                await ReportAsync(user, arrange.Goal(email, match.Groups[1].Value), null);
            }
            else
            {
                throw new InvalidOperationException($"Cannot read \"reads {what}\".");
            }
        }
    }

    private async Task ReportAsync(ActingUser user, F007Goal? goal, GoalId? id)
    {
        var result = await driver.GetGoalReportAsync(user, goal?.Id ?? id ?? throw new InvalidOperationException("No goal to read."));
        context.Reports.Add(new F007Report(goal, result));
        context.Calls.Add(new F007Call(result.Message, result.Report is not null, result.Outcome == GoalReportOutcome.Found));
    }

    private async Task OverviewAsync(ActingUser user)
    {
        var result = await driver.GetGoalsOverviewAsync(user);
        context.Overview = result;
        context.Calls.Add(new F007Call(result.Message, result.Overview is not null, result.Outcome == GoalsOverviewOutcome.Found));
    }

    private async Task DashboardAsync(ActingUser user)
    {
        var result = await driver.GetDashboardAsync(user);
        context.Dashboard = result;
        context.Calls.Add(new F007Call(result.Message, result.Dashboard is not null, result.Outcome == DashboardOutcome.Found));
    }
}
