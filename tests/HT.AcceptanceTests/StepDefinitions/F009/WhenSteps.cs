using System.Text.RegularExpressions;
using HT.AcceptanceTests.Support.F009;
using HT.Application.Goals;
using HT.Application.Habits;
using HT.Application.Operator;
using HT.Application.Reports;
using HT.Domain.Accounts;

namespace HT.AcceptanceTests.StepDefinitions.F009;

/// <summary>
/// The When steps of F009: each runs at its time as its actor, calls one use case (or the reads it names) and keeps
/// the result for the Then steps. Nothing here asserts the outcome; the Then steps do.
/// </summary>
[Binding]
[Scope(Tag = "F009")]
public sealed class WhenSteps(F009Context context, F009Driver driver, F009Arrange arrange)
{
    private const string D = "(" + F009Text.D + ")";
    private const string T = "(" + F009Text.T + ")";
    private const string Actor = "(" + F009Text.Actor + ")";
    private const string Value = "(Yes|No|\\d+)";
    private const string GoalOf = "((?: of the goal of `[^`]+`)?)";

    // ------------------------------------------------------------ this slice's use cases

    /// <summary>"changes the settings of the goal|the goal of `a` to S[ with the name N]": FR-001's use case (no name when none is written).</summary>
    [When("^at " + T + " " + Actor + " changes the settings of (the goal|the goal of `[^`]+`) to (.+?)((?: with the name (?:" + F009Text.TextToken + "))?)$")]
    public async Task WhenTheActorChangesTheSettings(string time, string actor, string goalPhrase, string settings, string nameClause)
    {
        var (email, user) = await StartAsync(time, actor);
        var goal = await arrange.GoalOfPhraseAsync(email, goalPhrase);
        var name = string.IsNullOrEmpty(nameClause) ? null : F009Text.Text(nameClause[" with the name ".Length..]);
        var result = await driver.ChangeSettingsAsync(user, goal.Id, new GoalSettingsInput { Target = F009Text.Settings(settings).ToFields(), Name = name });
        context.Outcome = new F009Outcome { Message = result.Message, SettingsChange = result };
    }

    /// <summary>"changes the amount to A minutes": FR-001's use case with the latest settings with the amount A and the goal's name.</summary>
    [When("^at " + T + " " + Actor + " changes the amount to (\\d+) minutes$")]
    public async Task WhenTheActorChangesTheAmount(string time, string actor, string amount)
    {
        var (email, user) = await StartAsync(time, actor);
        var goal = await arrange.GoalOfPhraseAsync(email, "the goal");
        var input = await arrange.AmountChangeAsync(goal, F009Text.Int(amount));
        var result = await driver.ChangeSettingsAsync(user, goal.Id, input);
        context.Outcome = new F009Outcome { Message = result.Message, SettingsChange = result };
    }

    /// <summary>"ends the goal|the goal of `a` with the outcome O|no outcome and the note N|no note": FR-007's use case.</summary>
    [When("^at " + T + " " + Actor + " ends (the goal|the goal of `[^`]+`) with (the outcome (?:" + F009Text.Outcomes + ")|no outcome) and (no note|the note (?:" + F009Text.TextToken + "))$")]
    public async Task WhenTheActorEndsTheGoal(string time, string actor, string goalPhrase, string outcomePhrase, string notePhrase)
    {
        var (email, user) = await StartAsync(time, actor);
        var goal = await arrange.GoalOfPhraseAsync(email, goalPhrase);
        var outcome = outcomePhrase == "no outcome" ? (HT.Domain.Goals.Outcome?)null : F009Text.OutcomeOf(outcomePhrase["the outcome ".Length..]);
        var note = notePhrase == "no note" ? null : F009Text.Text(notePhrase["the note ".Length..]);
        var result = await driver.EndGoalAsync(user, goal.Id, outcome, note);
        context.Outcome = new F009Outcome { Message = result.Message, End = result };
    }

    /// <summary>"reads the starting values of a new attempt[ of the goal of `a`]": FR-011's use case.</summary>
    [When("^at " + T + " " + Actor + " reads the starting values of a new attempt" + GoalOf + "$")]
    public async Task WhenTheActorReadsTheStartingValues(string time, string actor, string goalClause)
    {
        var (email, user) = await StartAsync(time, actor);
        var goal = await GoalAsync(email, goalClause);
        var result = await driver.GetNewAttemptValuesAsync(user, goal.Id);
        context.Outcome = new F009Outcome { Message = result.Message, StartingValues = result };
        context.ReadKind = F009ReadKind.StartingValues;
        context.StartingValues = result.Values;
    }

    /// <summary>"starts a new attempt[ of the goal of `a`] with S and the start date D|no start date": FR-012's use case.</summary>
    [When("^at " + T + " " + Actor + " starts a new attempt((?: of the goal of `[^`]+`)?) with (.+) and (the start date " + F009Text.D + "|no start date)$")]
    public async Task WhenTheActorStartsANewAttempt(string time, string actor, string goalClause, string settings, string datePhrase)
    {
        var (email, user) = await StartAsync(time, actor);
        var goal = await GoalAsync(email, goalClause);
        DateOnly? start = datePhrase == "no start date" ? null : F009Text.Date(datePhrase["the start date ".Length..]);
        var result = await driver.StartAttemptAsync(user, goal.Id, new NewAttemptInput { Target = F009Text.Settings(settings).ToFields(), StartDate = start });
        context.Outcome = new F009Outcome { Message = result.Message, AttemptStart = result };
    }

    /// <summary>"reads the attempts of the goal[ of `a`]": FR-014's use case for every attempt.</summary>
    [When("^at " + T + " " + Actor + " reads the attempts of the goal((?: of `[^`]+`)?)$")]
    public async Task WhenTheActorReadsTheAttempts(string time, string actor, string ofClause)
    {
        var (email, user) = await StartAsync(time, actor);
        var goal = await GoalAsync(email, ofClause.Length == 0 ? string.Empty : " of the goal" + ofClause);
        var result = await driver.GetAttemptsAsync(user, goal.Id);
        context.Outcome = new F009Outcome { Message = result.Message, Attempts = result };
        context.ReadKind = F009ReadKind.Attempts;
        context.ReadGoal = goal;
        context.Attempts = result.Outcome == AttemptListOutcome.Found ? result.Attempts : null;
    }

    /// <summary>"reads attempt N of the goal[ of `a`]": FR-014's use case for one attempt.</summary>
    [When("^at " + T + " " + Actor + " reads attempt (\\d+) of the goal((?: of `[^`]+`)?)$")]
    public async Task WhenTheActorReadsAnAttempt(string time, string actor, string number, string ofClause)
    {
        var (email, user) = await StartAsync(time, actor);
        var goal = await GoalAsync(email, ofClause.Length == 0 ? string.Empty : " of the goal" + ofClause);
        var result = await driver.GetAttemptAsync(user, goal.Id, F009Text.Int(number));
        context.Outcome = new F009Outcome { Message = result.Message, Attempt = result };
        context.ReadKind = F009ReadKind.Attempt;
        context.ReadGoal = goal;
        context.Attempt = result.Details;
    }

    /// <summary>"reads the goal history": FR-015's use case.</summary>
    [When("^at " + T + " " + Actor + " reads the goal history$")]
    public async Task WhenTheActorReadsTheGoalHistory(string time, string actor)
    {
        var (_, user) = await StartAsync(time, actor);
        var result = await driver.GetGoalHistoryAsync(user);
        context.Outcome = new F009Outcome { Message = result.Message, GoalHistory = result };
        context.ReadKind = F009ReadKind.GoalHistory;
        context.GoalHistoryEntries = result.Outcome == GoalHistoryListOutcome.Found ? result.Entries : null;
    }

    // ------------------------------------------------------------ earlier reads

    [When("^at " + T + " " + Actor + " reads (the goal|`[^`]+`)$")]
    public async Task WhenTheActorReadsTheGoal(string time, string actor, string goalPhrase)
    {
        var (email, _) = await StartAsync(time, actor);
        var goal = await arrange.GoalOfPhraseAsync(email, goalPhrase);
        context.ReadKind = F009ReadKind.Goal;
        context.ReadGoal = goal;
        context.GoalRead = await arrange.ReadGoalAsync(goal, email);
    }

    [When("^at " + T + " " + Actor + " reads the history of the goal from " + D + " to " + D + "$")]
    public async Task WhenTheActorReadsTheHistory(string time, string actor, string first, string last)
    {
        var (email, _) = await StartAsync(time, actor);
        var goal = await arrange.GoalOfPhraseAsync(email, "the goal");
        context.ReadKind = F009ReadKind.History;
        context.ReadGoal = goal;
        context.History = await arrange.HistoryAsync(goal, F009Text.Date(first), F009Text.Date(last));
    }

    /// <summary>"reads what can be recorded now": slice 5's use case, for every goal of the actor in the item.</summary>
    [When("^at " + T + " " + Actor + " reads what can be recorded now$")]
    public async Task WhenTheActorReadsWhatCanBeRecordedNow(string time, string actor)
    {
        var (email, _) = await StartAsync(time, actor);
        context.ReadKind = F009ReadKind.Recordable;
        foreach (var goal in context.Goals.Where(goal => goal.OwnerEmail == email).ToList())
        {
            context.Recordable[goal.Id] = await arrange.RecordableAsync(goal);
        }
    }

    [When("^at " + T + " " + Actor + " reads the report of the goal$")]
    public async Task WhenTheActorReadsTheReport(string time, string actor)
    {
        var (email, _) = await StartAsync(time, actor);
        var goal = await arrange.GoalOfPhraseAsync(email, "the goal");
        context.ReadKind = F009ReadKind.Report;
        context.ReadGoal = goal;
        context.Report = await arrange.ReportAsync(goal);
    }

    /// <summary>"reads the reports of `A`, `B` and `C`": slice 6's goal report use case for each goal named.</summary>
    [When("^at " + T + " " + Actor + " reads the reports of (`.+`)$")]
    public async Task WhenTheActorReadsTheReports(string time, string actor, string goals)
    {
        var (email, _) = await StartAsync(time, actor);
        context.ReadKind = F009ReadKind.Reports;
        foreach (var name in F009Text.Backticked(goals))
        {
            var goal = await arrange.GoalNamedAsync(email!, name);
            context.Reports[name] = await arrange.ReportAsync(goal);
        }
    }

    [When("^at " + T + " " + Actor + " reads the recent notes of the goal$")]
    public async Task WhenTheActorReadsTheRecentNotes(string time, string actor)
    {
        var (email, _) = await StartAsync(time, actor);
        var goal = await arrange.GoalOfPhraseAsync(email, "the goal");
        context.ReadKind = F009ReadKind.RecentNotes;
        context.ReadGoal = goal;
        context.RecentNotes = await arrange.RecentNotesAsync(goal);
    }

    [When("^at " + T + " " + Actor + " reads the goals overview$")]
    public async Task WhenTheActorReadsTheGoalsOverview(string time, string actor)
    {
        var (email, _) = await StartAsync(time, actor);
        context.ReadKind = F009ReadKind.Overview;
        context.Overview = await arrange.OverviewAsync(email!);
    }

    [When("^at " + T + " " + Actor + " reads the dashboard$")]
    public async Task WhenTheActorReadsTheDashboard(string time, string actor)
    {
        var (email, _) = await StartAsync(time, actor);
        context.ReadKind = F009ReadKind.Dashboard;
        context.Dashboard = await arrange.DashboardAsync(email!);
    }

    [When("^at " + T + " " + Actor + " reads the evaluation calendar of the goal from " + D + " to " + D + "$")]
    public async Task WhenTheActorReadsTheCalendar(string time, string actor, string first, string last)
    {
        var (email, user) = await StartAsync(time, actor);
        var goal = await arrange.GoalOfPhraseAsync(email, "the goal");
        var result = await driver.GetCalendarAsync(user, goal.Id, F009Text.Date(first), F009Text.Date(last));
        Assert.True(result.Outcome == GoalCalendarOutcome.Found, $"Reading the evaluation calendar gave {result.Outcome}: \"{result.Message}\".");
        context.ReadKind = F009ReadKind.Calendar;
        context.ReadGoal = goal;
        context.Calendar = result.Calendar;
    }

    // ------------------------------------------------------------ records, excusals and pauses

    [When("^at " + T + " " + Actor + " records " + Value + " for " + D + "$")]
    public async Task WhenTheActorRecords(string time, string actor, string value, string date)
    {
        var (email, user) = await StartAsync(time, actor);
        var goal = await arrange.GoalOfPhraseAsync(email, "the goal");
        var result = await driver.RecordAsync(user, goal.Id, F009Text.Date(date), F009Arrange.Input(value));
        context.Outcome = new F009Outcome { Message = result.Message, Record = result };
    }

    [When("^at " + T + " " + Actor + " changes the record of " + D + " to " + Value + "$")]
    public async Task WhenTheActorChangesTheRecord(string time, string actor, string date, string value)
    {
        var (email, user) = await StartAsync(time, actor);
        var goal = await arrange.GoalOfPhraseAsync(email, "the goal");
        var record = await arrange.RecordIdAsync(goal, F009Text.Date(date));
        var result = await driver.ChangeRecordAsync(user, goal.Id, record, F009Arrange.Input(value));
        context.Outcome = new F009Outcome { Message = result.Message, Record = result };
    }

    [When("^at " + T + " " + Actor + " deletes the record of " + D + "$")]
    public async Task WhenTheActorDeletesTheRecord(string time, string actor, string date)
    {
        var (email, user) = await StartAsync(time, actor);
        var goal = await arrange.GoalOfPhraseAsync(email, "the goal");
        var record = await arrange.RecordIdAsync(goal, F009Text.Date(date));
        var result = await driver.DeleteRecordAsync(user, goal.Id, record);
        context.Outcome = new F009Outcome { Message = result.Message, Record = result };
    }

    [When("^at " + T + " " + Actor + " excuses " + D + " without a reason$")]
    public async Task WhenTheActorExcuses(string time, string actor, string date)
    {
        var (email, user) = await StartAsync(time, actor);
        var goal = await arrange.GoalOfPhraseAsync(email, "the goal");
        var result = await driver.ExcuseAsync(user, goal.Id, F009Text.Date(date), null);
        context.Outcome = new F009Outcome { Message = result.Message, Excuse = result };
    }

    [When("^at " + T + " " + Actor + " removes the excusal of " + D + "$")]
    public async Task WhenTheActorRemovesTheExcusal(string time, string actor, string date)
    {
        var (email, user) = await StartAsync(time, actor);
        var goal = await arrange.GoalOfPhraseAsync(email, "the goal");
        var result = await driver.RemoveExcusalAsync(user, goal.Id, F009Text.Date(date));
        context.Outcome = new F009Outcome { Message = result.Message, RemoveExcusal = result };
    }

    [When("^at " + T + " " + Actor + " pauses the goal from " + D + " until " + D + " without a reason$")]
    public async Task WhenTheActorPauses(string time, string actor, string first, string last)
    {
        var (email, user) = await StartAsync(time, actor);
        var goal = await arrange.GoalOfPhraseAsync(email, "the goal");
        var result = await driver.PauseAsync(user, goal.Id, F009Text.Date(first), F009Text.Date(last), null);
        context.Outcome = new F009Outcome { Message = result.Message, Pause = result };
    }

    // ------------------------------------------------------------ accounts, habits and catalog

    [When("^at " + T + " " + Actor + " changes the account's time zone to `([^`]+)`$")]
    public async Task WhenTheActorChangesTheTimeZone(string time, string actor, string zone)
    {
        var (_, user) = await StartAsync(time, actor);
        var result = await driver.ChangeTimeZoneAsync(user, zone);
        context.Outcome = new F009Outcome { Message = result.Message, TimeZoneChange = result };
    }

    [When("^at " + T + " " + Actor + " deletes the custom habit `([^`]+)`$")]
    public async Task WhenTheActorDeletesTheCustomHabit(string time, string actor, string habit)
    {
        var (email, user) = await StartAsync(time, actor);
        var result = await driver.DeleteCustomHabitAsync(user, context.CustomHabits[(email!, habit)]);
        context.Outcome = new F009Outcome { Message = result.Message, CustomHabitDelete = result };
    }

    [When("^at " + T + " " + Actor + " unpublishes the catalog habit \"([^\"]+)\" without confirmation$")]
    public async Task WhenTheAdministratorUnpublishesWithoutConfirmation(string time, string actor, string habit)
    {
        var (_, user) = await StartAsync(time, actor);
        var id = context.CatalogHabits.TryGetValue(habit, out var known) ? known : await arrange.CatalogHabitIdAsAdministratorAsync(user, habit);
        var result = await driver.UnpublishAsync(user, id, confirmed: false);
        context.Outcome = new F009Outcome { Message = result.Message, Unpublish = result };
    }

    /// <summary>"browses the catalog with Only habits I track": the catalog browsing use case with that filter.</summary>
    [When("^at " + T + " " + Actor + " browses the catalog with Only habits I track$")]
    public async Task WhenTheActorBrowsesTrackedHabits(string time, string actor)
    {
        var (_, user) = await StartAsync(time, actor);
        var result = await driver.BrowseAsync(user, new CatalogFilter(), onlyTracked: true);
        Assert.True(result.Outcome == CatalogBrowseOutcome.Found, $"Browsing the catalog gave {result.Outcome}: \"{result.Message}\".");
        context.ReadKind = F009ReadKind.Catalog;
        context.Browse = result;
    }

    // ------------------------------------------------------------ demo goal-change loading

    [When("^demo goal-change loading runs at " + T + " (with the demo password `[^`]+`|with no demo password)$")]
    public async Task WhenDemoGoalChangeLoadingRuns(string time, string passwordPhrase)
    {
        arrange.When(time, null);
        var password = passwordPhrase == "with no demo password" ? null : F009Text.Backticked(passwordPhrase).Single();
        var result = await driver.LoadDemoGoalChangesAsync(password);
        context.Outcome = new F009Outcome { Message = result.Message, DemoGoalChange = result };
    }

    /// <summary>
    /// AC-095: every account that a result of the four earlier demo tasks gives as created signs in with the demo
    /// password and reads the attempts of each of its goals (slice 4's goal list) and its goal history.
    /// </summary>
    [When("^at " + T + " each account that a result of demo loading, demo history loading, demo report loading or demo pause loading gives as created, `demo1@example\\.com` and `demo4@example\\.com` to `demo6@example\\.com` included, signs in and reads the attempts of each of its goals and its goal history$")]
    public async Task WhenEachEarlierDemoAccountReads(string time)
    {
        arrange.When(time, null);
        var created = new List<string>();
        created.AddRange((context.DemoLoading?.Accounts ?? []).Where(account => account.Outcome == DemoAccountOutcome.Created).Select(account => account.Email));
        created.AddRange((context.DemoHistory?.Accounts ?? []).Where(account => account.Outcome == DemoAccountOutcome.Created).Select(account => account.Email));
        created.AddRange((context.DemoReport?.Accounts ?? []).Where(account => account.Outcome == DemoAccountOutcome.Created).Select(account => account.Email));
        created.AddRange((context.DemoPause?.Accounts ?? []).Where(account => account.Outcome == DemoAccountOutcome.Created).Select(account => account.Email));
        var emails = created.Distinct(StringComparer.Ordinal).ToList();
        foreach (var included in new[] { "demo1@example.com", "demo4@example.com", "demo5@example.com", "demo6@example.com" })
        {
            Assert.Contains(included, emails);
        }

        foreach (var email in emails)
        {
            var user = await arrange.SignInAsync(email, F009Text.DemoPassword);
            var list = await driver.ListGoalsAsync(user);
            Assert.True(list.Outcome == GoalListOutcome.Found, $"Listing the goals of `{email}` gave {list.Outcome}.");
            var goals = new List<(string Goal, IReadOnlyList<AttemptView> Attempts)>();
            foreach (var item in list.Goals)
            {
                var attempts = await driver.GetAttemptsAsync(user, item.Id);
                Assert.True(attempts.Outcome == AttemptListOutcome.Found, $"Reading the attempts of `{item.Name}` gave {attempts.Outcome}: \"{attempts.Message}\".");
                goals.Add((item.Name, attempts.Attempts));
            }

            var history = await driver.GetGoalHistoryAsync(user);
            Assert.True(history.Outcome == GoalHistoryListOutcome.Found, $"Reading the goal history of `{email}` gave {history.Outcome}.");
            context.DemoReads.Add((email, goals, history.Entries));
        }
    }

    // ------------------------------------------------------------ helpers

    private async Task<(string? Email, ActingUser User)> StartAsync(string time, string actor)
    {
        var (email, user) = await arrange.ActorAsync(actor);
        arrange.When(time, email);
        return (email, user);
    }

    /// <summary>"" (the actor's only goal) or " of the goal of `a`" (a's only goal).</summary>
    private async Task<F009Goal> GoalAsync(string? actorEmail, string goalClause)
    {
        var of = Regex.Match(goalClause, "^ of the goal of `([^`]+)`$");
        return of.Success ? arrange.OnlyGoalOf(of.Groups[1].Value) : await arrange.GoalOfPhraseAsync(actorEmail, "the goal");
    }
}
