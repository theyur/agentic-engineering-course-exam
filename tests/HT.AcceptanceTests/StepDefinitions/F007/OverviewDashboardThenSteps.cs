using System.Text.RegularExpressions;
using HT.AcceptanceTests.Support.F007;
using HT.Application.Goals;
using HT.Application.Reports;
using HT.Domain.Evaluation;
using HT.Domain.Goals;
using HT.Domain.Habits;

namespace HT.AcceptanceTests.StepDefinitions.F007;

/// <summary>The Then steps of F007 about the goals overview (FR-011, FR-016) and the dashboard (FR-012 to FR-014).</summary>
[Binding]
[Scope(Tag = "F007")]
public sealed class OverviewDashboardThenSteps(F007Context context, F007Arrange arrange, F007Checks checks)
{
    private const string D = "(" + F007Text.D + ")";
    private const string Span = "(" + F007Text.Span + ")";

    // ------------------------------------------------------------ goals overview

    [Then("^it lists the `([^`]+)` goal first, named \"([^\"]+)\", (Build|Reduce), with the consistency \"([^\"]+)\" and the current streak \"([^\"]+)\"$")]
    public async Task ThenItListsTheGoalFirst(string label, string name, string kind, string consistency, string streak)
    {
        await arrange.ThenAsync();
        var goals = checks.Overview.Goals;
        Assert.NotEmpty(goals);
        var first = goals[0];
        Assert.Equal(arrange.Goal(null, $"the `{label}` goal").Id, first.Item.Id);
        Assert.Equal(name, first.Item.Name);
        Assert.Equal(Enum.Parse<BuildOrReduce>(kind), first.Item.BuildOrReduce);
        F007Checks.Consistency(first.Stats, consistency);
        F007Checks.CurrentStreak(first.Stats, streak);
    }

    /// <summary>"the `R` goal's last 15 ended results are one entry for each date from D1 to D2 in date order, of which D is Missed, D and D are Pending and every other is Met".</summary>
    [Then("^the `([^`]+)` goal's last (\\d+) ended results are one entry for each date from " + D + " to " + D + " in date order, of which (.+)$")]
    public async Task ThenTheGoalsLastEndedResultsAre(string label, string count, string first, string last, string states)
    {
        await arrange.ThenAsync();
        var results = OverviewGoal(label).Recent.Results;
        var dates = F007Text.Range(F007Text.Date(first), F007Text.Date(last));
        Assert.Equal(F007Text.Int(count), dates.Count);
        Assert.Equal(dates, results.Select(result => result.FirstDate).ToList());

        var expected = new Dictionary<DateOnly, EvaluationState>();
        foreach (Match match in Regex.Matches(states, "(?<dates>" + F007Text.D + "(?:(?:, | and )" + F007Text.D + ")*) (?:is|are) (?<state>Upcoming|Due|Pending|Met|Failed|Missed)"))
        {
            foreach (var date in F007Text.Dates(match.Groups["dates"].Value))
            {
                expected[date] = F007Text.StateOf(match.Groups["state"].Value);
            }
        }

        var other = Regex.Match(states, "every other is (Upcoming|Due|Pending|Met|Failed|Missed)$");
        Assert.True(other.Success, $"Cannot read \"{states}\".");
        Assert.NotEmpty(expected);
        foreach (var result in results)
        {
            var state = expected.TryGetValue(result.FirstDate, out var given) ? given : F007Text.StateOf(other.Groups[1].Value);
            Assert.Equal(state, result.State);
        }
    }

    [Then("^the `([^`]+)` goal has a Pending result, something can be recorded now, and its Due entry is " + Span + ", progress (\\d+) of (\\d+)$")]
    public async Task ThenTheGoalHasAPendingResultAndADueEntry(string label, string span, string value, string of)
    {
        await arrange.ThenAsync();
        var goal = OverviewGoal(label);
        Assert.True(goal.HasPending);
        Assert.True(goal.CanRecord);
        DueEntry(goal, span, F007Text.Int(value), F007Text.Int(of));
    }

    [Then("^it lists the `([^`]+)` goal second, with the consistency \"([^\"]+)\", the current streak \"([^\"]+)\", no ended result, no Pending result, nothing that can be recorded now and no Due entry$")]
    public async Task ThenItListsTheGoalSecond(string label, string consistency, string streak)
    {
        await arrange.ThenAsync();
        var goals = checks.Overview.Goals;
        Assert.True(goals.Count >= 2, $"The overview lists {goals.Count} goals.");
        var second = goals[1];
        Assert.Equal(arrange.Goal(null, $"the `{label}` goal").Id, second.Item.Id);
        F007Checks.Consistency(second.Stats, consistency);
        F007Checks.CurrentStreak(second.Stats, streak);
        Assert.Empty(second.Recent.Results);
        Assert.False(second.HasPending);
        Assert.False(second.CanRecord);
        Assert.DoesNotContain(second.Recordable.Entries, entry => entry.State == EvaluationState.Due);
    }

    /// <summary>"the goals are, in this order: `B`, `D`, the `A` created third, the `A` created fifth, `C`" (AC-030).</summary>
    [Then("^the goals are, in this order: (.+)$")]
    public async Task ThenTheGoalsAreInThisOrder(string list)
    {
        await arrange.ThenAsync();
        var expected = list.Split(", ").Select(item => OrderedGoal(item).Id).ToList();
        Assert.Equal(expected, checks.Overview.Goals.Select(goal => goal.Item.Id).ToList());
    }

    /// <summary>"`B` and `D` have a Pending result, and `A`, `A` and `C` have none" (AC-030).</summary>
    [Then("^(.+) have a Pending result, and (.+) have none$")]
    public async Task ThenSomeGoalsHaveAPendingResult(string with, string without)
    {
        await arrange.ThenAsync();
        var goals = checks.Overview.Goals;
        foreach (var name in F007Text.Backticked(with).Distinct())
        {
            var ids = context.Goals.Where(goal => goal.Name == name).Select(goal => goal.Id).ToList();
            Assert.NotEmpty(ids);
            Assert.All(ids, id => Assert.True(Assert.Single(goals, goal => goal.Item.Id == id).HasPending));
        }

        foreach (var name in F007Text.Backticked(without).Distinct())
        {
            var ids = context.Goals.Where(goal => goal.Name == name).Select(goal => goal.Id).ToList();
            Assert.NotEmpty(ids);
            Assert.All(ids, id => Assert.False(Assert.Single(goals, goal => goal.Item.Id == id).HasPending));
        }
    }

    [Then("^the overview lists, in this order: (.+)$")]
    public async Task ThenTheOverviewListsInThisOrder(string list)
    {
        await arrange.ThenAsync();
        var expected = F007Text.Backticked(list).Select(name => arrange.Goal(null, $"`{name}`").Id).ToList();
        Assert.Equal(expected, checks.Overview.Goals.Select(goal => goal.Item.Id).ToList());
    }

    /// <summary>"it gives `P` the Due entry D, progress A of B, `Y` the Due entry D1 – D2, progress A of B, and `L` no Due entry" (AC-031).</summary>
    [Then("^it gives (`.+)$")]
    public async Task ThenItGivesDueEntries(string list)
    {
        await arrange.ThenAsync();
        var items = Regex.Matches(list, "`([^`]+)` (?:the Due entry (" + F007Text.Span + "), progress (\\d+) of (\\d+)|(no Due entry))");
        Assert.NotEmpty(items);
        foreach (Match item in items)
        {
            var goal = OverviewGoal(item.Groups[1].Value);
            if (item.Groups[5].Success)
            {
                Assert.DoesNotContain(goal.Recordable.Entries, entry => entry.State == EvaluationState.Due);
            }
            else
            {
                DueEntry(goal, item.Groups[2].Value, F007Text.Int(item.Groups[3].Value), F007Text.Int(item.Groups[4].Value));
            }
        }
    }

    [Then("^the overview lists exactly one goal, the goal of `([^`]+)`$")]
    public async Task ThenTheOverviewListsExactlyOneGoal(string email)
    {
        await arrange.ThenAsync();
        var goal = Assert.Single(checks.Overview.Goals);
        Assert.Equal(Assert.Single(arrange.GoalsOf(email)).Id, goal.Item.Id);
    }

    [Then("^neither is refused$")]
    public async Task ThenNeitherIsRefused()
    {
        await arrange.ThenAsync();
        Assert.Equal(DashboardOutcome.Found, (context.Dashboard ?? throw new InvalidOperationException("No dashboard was read.")).Outcome);
        Assert.Equal(GoalsOverviewOutcome.Found, (context.Overview ?? throw new InvalidOperationException("No overview was read.")).Outcome);
    }

    [Then("^the goals overview is empty$")]
    public async Task ThenTheGoalsOverviewIsEmpty()
    {
        await arrange.ThenAsync();
        Assert.Empty(checks.Overview.Goals);
    }

    /// <summary>AC-063: the goal list of the When step lists the goals in name order, each entry equal to the earlier read's entry.</summary>
    [Then("^the goal list lists (.+), in this order, and each entry is equal, field by field, to the entry of the same goal that the goal list gave in the earlier read$")]
    public async Task ThenTheGoalListIsUnchanged(string list)
    {
        await arrange.ThenAsync();
        var result = context.GoalList ?? throw new InvalidOperationException("The When step read no goal list.");
        Assert.Equal(GoalListOutcome.Found, result.Outcome);
        var earlier = context.EarlierGoalList ?? throw new InvalidOperationException("No earlier goal list was read.");
        var expected = F007Text.Backticked(list).Select(name => arrange.Goal(null, $"`{name}`").Id).ToList();
        Assert.Equal(expected, result.Goals.Select(item => item.Id).ToList());
        foreach (var item in result.Goals)
        {
            Assert.Equal(Assert.Single(earlier, entry => entry.Id == item.Id), item);
        }
    }

    [Then("^the goals overview lists (.+), in this order, and gives each goal the same name, Build or Reduce, schedule and start or next information as its goal list entry$")]
    public async Task ThenTheGoalsOverviewCarriesTheListEntries(string list)
    {
        await arrange.ThenAsync();
        var expected = F007Text.Backticked(list).Select(name => arrange.Goal(null, $"`{name}`").Id).ToList();
        var overview = checks.Overview.Goals;
        Assert.Equal(expected, overview.Select(goal => goal.Item.Id).ToList());
        var entries = (context.GoalList ?? throw new InvalidOperationException("The When step read no goal list.")).Goals;
        foreach (var goal in overview)
        {
            var entry = Assert.Single(entries, item => item.Id == goal.Item.Id);
            Assert.Equal(entry.Name, goal.Item.Name);
            Assert.Equal(entry.BuildOrReduce, goal.Item.BuildOrReduce);
            Assert.Equal(entry.ScheduleText, goal.Item.ScheduleText);
            Assert.Equal(entry.ScheduleKind, goal.Item.ScheduleKind);
            Assert.Equal(entry.StartDate, goal.Item.StartDate);
            Assert.Equal(entry.NextCommitment, goal.Item.NextCommitment);
            Assert.Equal(entry.FirstWeekStarts, goal.Item.FirstWeekStarts);
        }
    }

    // ------------------------------------------------------------ dashboard: counts and goals

    [Then("^the counts are (\\d+) Due, (\\d+) Pending, (\\d+) Missed this week and (\\d+) goals$")]
    public async Task ThenTheCountsAre(string due, string pending, string missed, string goals)
    {
        await arrange.ThenAsync();
        Counts(checks.Dashboard.Counts, due, pending, missed, goals);
    }

    [Then("^the dashboard counts (\\d+) Due, (\\d+) Pending, (\\d+) Missed this week and (\\d+) goals, and its Today list, goal list and recent notes are empty$")]
    public async Task ThenTheDashboardCountsAndIsEmpty(string due, string pending, string missed, string goals)
    {
        await arrange.ThenAsync();
        var dashboard = checks.Dashboard;
        Counts(dashboard.Counts, due, pending, missed, goals);
        Assert.Empty(dashboard.Today);
        Assert.Empty(dashboard.Goals);
        Assert.Empty(dashboard.Notes);
    }

    [Then("^the dashboard counts (\\d+) goals?$")]
    public async Task ThenTheDashboardCountsGoals(string goals)
    {
        await arrange.ThenAsync();
        Assert.Equal(F007Text.Int(goals), checks.Dashboard.Counts.Goals);
    }

    [Then("^the count of Missed this week is (\\d+)$")]
    public async Task ThenTheCountOfMissedThisWeekIs(string missed)
    {
        await arrange.ThenAsync();
        Assert.Equal(F007Text.Int(missed), checks.Dashboard.Counts.MissedThisWeek);
    }

    [Then("^the dashboard lists exactly two goals: the first goal with \"([^\"]+)\", then the second goal with \"([^\"]+)\"$")]
    public async Task ThenTheDashboardListsExactlyTwoGoals(string first, string second)
    {
        await arrange.ThenAsync();
        var goals = checks.Dashboard.Goals;
        Assert.Equal(2, goals.Count);
        Assert.Equal(arrange.Goal(null, "the first goal").Id, goals[0].GoalId);
        F007Checks.Consistency(goals[0].Stats, first);
        Assert.Equal(arrange.Goal(null, "the second goal").Id, goals[1].GoalId);
        F007Checks.Consistency(goals[1].Stats, second);
    }

    /// <summary>"its goals are, in this order:" with rows ""N" with the consistency "X", the current streak "Y" and the longest streak "Z"".</summary>
    [Then("^its goals are, in this order:$")]
    public async Task ThenItsGoalsAreInThisOrder(DataTable table)
    {
        await arrange.ThenAsync();
        var goals = checks.Dashboard.Goals;
        Assert.Equal(table.Rows.Count, goals.Count);
        for (var index = 0; index < table.Rows.Count; index++)
        {
            var match = Regex.Match(table.Rows[index]["goal"], "^\"([^\"]+)\" with (the consistency .+)$");
            Assert.True(match.Success, $"Cannot read \"{table.Rows[index]["goal"]}\".");
            Assert.Equal(match.Groups[1].Value, goals[index].Name);
            F007Checks.Figures(goals[index].Stats, match.Groups[2].Value);
        }
    }

    // ------------------------------------------------------------ dashboard: Today list

    [Then("^the Today list is, in this order:$")]
    public async Task ThenTheTodayListIsInThisOrderTable(DataTable table)
    {
        await arrange.ThenAsync();
        checks.Entries(checks.Dashboard.Today, table.Rows.Select(row => F007Text.Entry(row["entry"])).ToList());
    }

    [Then("^the Today list is, in this order: (\\(.+\\))$")]
    public async Task ThenTheTodayListIsInThisOrder(string entries)
    {
        await arrange.ThenAsync();
        checks.Entries(checks.Dashboard.Today, F007Text.Entries(entries));
    }

    [Then("^the Today list has no entry for " + D + " and no entry for the `([^`]+)` week " + D + " – " + D + "$")]
    public async Task ThenTheTodayListHasNoEntryForTheDateOrTheWeek(string date, string label, string first, string last)
    {
        await arrange.ThenAsync();
        var today = checks.Dashboard.Today;
        var day = F007Text.Date(date);
        Assert.DoesNotContain(today, entry => entry.Entry.FirstDate == day && entry.Entry.LastDate == day);
        var week = arrange.Goal(null, $"the `{label}` goal");
        Assert.DoesNotContain(
            today,
            entry => entry.GoalId == week.Id && entry.Entry.FirstDate == F007Text.Date(first) && entry.Entry.LastDate == F007Text.Date(last));
    }

    [Then("^the Today list has no Upcoming entry for `([^`]+)` and no entry for `([^`]+)`$")]
    public async Task ThenTheTodayListHasNoUpcomingEntryFor(string upcoming, string none)
    {
        await arrange.ThenAsync();
        var today = checks.Dashboard.Today;
        var first = arrange.Goal(null, $"`{upcoming}`").Id;
        var second = arrange.Goal(null, $"`{none}`").Id;
        Assert.DoesNotContain(today, entry => entry.GoalId == first && entry.Entry.State == EvaluationState.Upcoming);
        Assert.DoesNotContain(today, entry => entry.GoalId == second);
    }

    [Then("^the Today list has no Upcoming entry$")]
    public async Task ThenTheTodayListHasNoUpcomingEntry()
    {
        await arrange.ThenAsync();
        Assert.DoesNotContain(checks.Dashboard.Today, entry => entry.Entry.State == EvaluationState.Upcoming);
    }

    [Then("^the Today list contains (\\([^()]+\\))$")]
    public async Task ThenTheTodayListContains(string entry)
    {
        await arrange.ThenAsync();
        var expected = F007Text.Entry(entry);
        Assert.Contains(checks.Dashboard.Today, actual => checks.IsEntry(actual, expected));
    }

    [Then("^it contains (\\([^()]+\\))$")]
    public async Task ThenItContains(string entry)
    {
        await arrange.ThenAsync();
        var expected = F007Text.Entry(entry);
        Assert.Contains(checks.Dashboard.Today, actual => checks.IsEntry(actual, expected));
    }

    /// <summary>"the only Missed entry of the Today list is (…)", optionally ", and D is Pending" (AC-034, AC-053).</summary>
    [Then("^the only Missed entry of the Today list is (\\([^()]+\\))((?:, and " + F007Text.D + " is Pending)?)$")]
    public async Task ThenTheOnlyMissedEntryIs(string entry, string pending)
    {
        await arrange.ThenAsync();
        var today = checks.Dashboard.Today;
        checks.Entry(Assert.Single(today, actual => actual.Entry.State == EvaluationState.Missed), F007Text.Entry(entry));
        if (pending.Length > 0)
        {
            var date = F007Text.Dates(pending).Single();
            var ofDate = Assert.Single(today, actual => actual.Entry.FirstDate == date);
            Assert.Equal(EvaluationState.Pending, ofDate.Entry.State);
        }
    }

    [Then("^every entry gives the identifier of the goal it names, as creating that goal returned it, and Build$")]
    public async Task ThenEveryEntryGivesTheGoalIdentifierAndBuild()
    {
        await arrange.ThenAsync();
        var today = checks.Dashboard.Today;
        Assert.NotEmpty(today);
        foreach (var entry in today)
        {
            var goal = Assert.Single(context.Goals, created => created.Name == entry.GoalName);
            Assert.Equal(goal.Id, entry.GoalId);
            Assert.Equal(BuildOrReduce.Build, entry.BuildOrReduce);
        }
    }

    // ------------------------------------------------------------ dashboard: recent notes and privacy

    [Then("^the recent notes are, in this order:$")]
    public async Task ThenTheRecentNotesAre(DataTable table)
    {
        await arrange.ThenAsync();
        F007Checks.Notes(checks.Dashboard.Notes, table.Rows.Select(row => F007Text.Note(row["recent note"])).ToList());
    }

    [Then("^the dashboard's recent notes are, in this order:$")]
    public async Task ThenTheDashboardsRecentNotesAre(DataTable table)
    {
        await arrange.ThenAsync();
        F007Checks.Notes(checks.Dashboard.Notes, table.Rows.Select(row => F007Text.Note(row["recent note"])).ToList());
    }

    [Then("^they contain neither \"([^\"]+)\" nor \"([^\"]+)\"$")]
    public async Task ThenTheyContainNeither(string first, string second)
    {
        await arrange.ThenAsync();
        var notes = checks.Dashboard.Notes;
        Assert.DoesNotContain(notes, note => note.Note == first);
        Assert.DoesNotContain(notes, note => note.Note == second);
    }

    [Then("^every entry of the dashboard's Today list, goal list and recent notes belongs to that goal, and no entry contains \"([^\"]+)\" or the goal of `([^`]+)`$")]
    public async Task ThenEveryDashboardEntryBelongsToThatGoal(string note, string ownerEmail)
    {
        await arrange.ThenAsync();
        var dashboard = checks.Dashboard;
        var own = Assert.Single(checks.Overview.Goals).Item.Id;
        var others = arrange.GoalsOf(ownerEmail).Select(goal => goal.Id).ToHashSet();
        Assert.NotEmpty(others);
        Assert.All(dashboard.Today, entry => Assert.Equal(own, entry.GoalId));
        Assert.All(dashboard.Goals, goal => Assert.Equal(own, goal.GoalId));
        Assert.All(dashboard.Notes, item => Assert.Equal(own, item.GoalId));
        Assert.DoesNotContain(dashboard.Notes, item => item.Note.Contains(note, StringComparison.Ordinal));
        Assert.DoesNotContain(dashboard.Today, entry => others.Contains(entry.GoalId) || (entry.Entry.Note ?? string.Empty).Contains(note, StringComparison.Ordinal));
        Assert.DoesNotContain(dashboard.Goals, goal => others.Contains(goal.GoalId));
    }

    // ------------------------------------------------------------ helpers

    private OverviewGoal OverviewGoal(string label)
    {
        var goal = arrange.Goal(null, label.StartsWith("the ", StringComparison.Ordinal) ? label : $"the `{label}` goal");
        return Assert.Single(checks.Overview.Goals, item => item.Item.Id == goal.Id);
    }

    /// <summary>"`B`", "`D`" or "the `A` created third": a goal of the item by its name, or by its name and creation position.</summary>
    private F007Goal OrderedGoal(string item)
    {
        var match = Regex.Match(item.Trim(), "^the `([^`]+)` created (first|second|third|fourth|fifth)$");
        if (match.Success)
        {
            var position = Array.IndexOf(["first", "second", "third", "fourth", "fifth"], match.Groups[2].Value);
            var goal = Assert.Single(context.Goals, created => created.Position == position);
            Assert.Equal(match.Groups[1].Value, goal.Name);
            return goal;
        }

        return arrange.Goal(null, item.Trim());
    }

    /// <summary>The goal's only Due entry among what can be recorded now, with its date or period and progress.</summary>
    private static void DueEntry(OverviewGoal goal, string span, int value, int of)
    {
        var (first, last) = F007Text.SpanOf(span);
        var due = Assert.Single(goal.Recordable.Entries, entry => entry.State == EvaluationState.Due);
        Assert.Equal(first, due.FirstDate);
        Assert.Equal(last, due.LastDate);
        Assert.Equal(value, due.Progress.Value);
        Assert.Equal(of, due.Progress.Of);
    }

    private static void Counts(DashboardCounts counts, string due, string pending, string missed, string goals)
    {
        Assert.Equal(F007Text.Int(due), counts.Due);
        Assert.Equal(F007Text.Int(pending), counts.Pending);
        Assert.Equal(F007Text.Int(missed), counts.MissedThisWeek);
        Assert.Equal(F007Text.Int(goals), counts.Goals);
    }
}
