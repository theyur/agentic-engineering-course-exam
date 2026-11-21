using System.Text.RegularExpressions;
using HT.AcceptanceTests.Support.F008;
using HT.Application.Exemptions;
using HT.Application.Habits;
using HT.Application.Operator;
using HT.Application.Records;

namespace HT.AcceptanceTests.StepDefinitions.F008;

/// <summary>
/// The Given steps of F008: accounts and goals, the excusals, pauses, records and notes that set an item up (each must
/// succeed), and the records that give no time (they run at the When step's time).
/// </summary>
[Binding]
[Scope(Tag = "F008")]
public sealed class GivenSteps(F008Context context, F008Driver driver, F008Arrange arrange)
{
    private const string D = "(" + F008Text.D + ")";
    private const string T = "(" + F008Text.T + ")";
    private const string Value = "(Yes|No|\\d+)";

    private static readonly Regex Created = new(
        "^the goal `(?<goal>[^`]+)`(?: at (?<time>" + F008Text.T + ") with the start date (?<start>" + F008Text.D + "))?(?: and the goal `(?<second>[^`]+)`)?$",
        RegexOptions.CultureInvariant);

    // ------------------------------------------------------------ accounts and goals

    /// <summary>
    /// "the registered user `x`[ in `Z`], who created the goal `G`[ at T with the start date D][ and the goal `H`]":
    /// registration and sign-in run at the earliest time of the item, which is the goal creation (by default
    /// 2026-11-02T08:00:00Z with the start date 2026-11-02); then the custom habits and the goals are created.
    /// </summary>
    [Given("^the registered user `([^`]+)`((?: in `[^`]+`)?), who created (.+)$")]
    public async Task GivenTheRegisteredUserWhoCreated(string email, string zoneClause, string goals)
    {
        // The optional " in `Z`" sits in a group that always takes part (empty when not written).
        var zone = BacktickedOrNull(zoneClause);
        var match = Created.Match(goals);
        if (!match.Success)
        {
            throw new InvalidOperationException($"Cannot read \"{goals}\".");
        }

        var creation = match.Groups["time"].Success ? F008Text.Instant(match.Groups["time"].Value) : F008Driver.DefaultGoalCreation;
        var start = match.Groups["start"].Success ? F008Text.Date(match.Groups["start"].Value) : F008Driver.DefaultStartDate;

        // Setup that gives no time runs at the earliest time the item gives; a step before this one may have fixed it.
        arrange.At(context.SetupTime is { } setup && setup < creation ? setup : creation);
        await arrange.RegisteredUserAsync(email, zone);

        arrange.At(creation);
        await arrange.CreateGoalAsync(email, match.Groups["goal"].Value, start);
        if (match.Groups["second"].Success)
        {
            await arrange.CreateGoalAsync(email, match.Groups["second"].Value, start);
        }
    }

    [Given("^the registered user `([^`]+)`$")]
    public async Task GivenTheRegisteredUser(string email)
    {
        arrange.AtSetupTime();
        await arrange.RegisteredUserAsync(email, null);
    }

    [Given("^the administrator `([^`]+)`$")]
    public async Task GivenTheAdministrator(string email)
    {
        arrange.AtSetupTime();
        await arrange.AdministratorAsync(email);
    }

    // ------------------------------------------------------------ excusals and pauses that set an item up

    [Given("^at " + T + " `([^`]+)` excused " + D + "((?: of `[^`]+`)?) (with the reason \"[^\"]*\"|without a reason)$")]
    public async Task GivenTheUserExcused(string time, string email, string date, string goalClause, string reason)
    {
        arrange.At(time);
        var goalName = BacktickedOrNull(goalClause);
        var goal = goalName is null ? arrange.OnlyGoalOf(email) : await arrange.GoalNamedAsync(email, goalName);
        var result = await driver.ExcuseAsync(await arrange.AccountAsync(email), goal.Id, F008Text.Date(date), ReasonOf(reason));
        Assert.True(result.Outcome == ExcuseOutcome.Excused, $"Excusing {date} gave {result.Outcome}: \"{result.Message}\".");
        Assert.Equal(ExcusalTexts.Excused, result.Message);
    }

    /// <summary>
    /// "at T `x` paused the goal|`G` from D1 until D2 with the reason "R"|without a reason": the pause must be set.
    /// </summary>
    [Given("^at " + T + " `([^`]+)` paused (the goal|`[^`]+`) from " + D + " until " + D + " (with the reason \"[^\"]*\"|without a reason)$")]
    public async Task GivenTheUserPaused(string time, string email, string goalPhrase, string first, string last, string reason)
    {
        var (goal, result) = await PauseSucceedsAsync(time, email, goalPhrase, first, last, reason);
        arrange.RememberPause(goal, result);
    }

    /// <summary>
    /// "… paused … from D1 until D2 …, with the result "R" with the first day D and the last day D": the pause must be
    /// set and give that result text and those days.
    /// </summary>
    [Given("^at " + T + " `([^`]+)` paused (the goal|`[^`]+`) from " + D + " until " + D + " (with the reason \"[^\"]*\"|without a reason), with the result \"([^\"]+)\" with the first day " + D + " and the last day " + D + "$")]
    public async Task GivenTheUserPausedWithTheResult(
        string time, string email, string goalPhrase, string first, string last, string reason, string resultText, string resultFirst, string resultLast)
    {
        var (goal, result) = await PauseSucceedsAsync(time, email, goalPhrase, first, last, reason);
        Assert.Equal(resultText, result.Message);
        var pause = result.Pause ?? throw new Xunit.Sdk.XunitException($"The result \"{result.Message}\" gave no pause.");
        Assert.Equal((F008Text.Date(resultFirst), F008Text.Date(resultLast)), (pause.FirstDay, pause.LastDay));
        arrange.RememberPause(goal, result);
    }

    [Given("^at " + T + " `([^`]+)` removed the excusal of " + D + "$")]
    public async Task GivenTheUserRemovedTheExcusal(string time, string email, string date)
    {
        await RemoveExcusalSucceedsAsync(time, email, date);
    }

    /// <summary>"… removed the excusal of D, with the result "R" and the state S": it must give that text and that state.</summary>
    [Given("^at " + T + " `([^`]+)` removed the excusal of " + D + ", with the result \"([^\"]+)\" and the state (\\w+)$")]
    public async Task GivenTheUserRemovedTheExcusalWithTheResult(string time, string email, string date, string resultText, string state)
    {
        var result = await RemoveExcusalSucceedsAsync(time, email, date);
        Assert.Equal(resultText, result.Message);
        Assert.Equal(F008Text.StateOf(state), result.State);
    }

    private async Task<(F008Goal Goal, PauseResult Result)> PauseSucceedsAsync(
        string time, string email, string goalPhrase, string first, string last, string reason)
    {
        arrange.At(time);
        var goal = goalPhrase == "the goal"
            ? arrange.OnlyGoalOf(email)
            : await arrange.GoalNamedAsync(email, goalPhrase.Trim('`'));
        var result = await driver.PauseAsync(
            await arrange.AccountAsync(email), goal.Id, F008Text.Date(first), F008Text.Date(last), ReasonOf(reason));
        Assert.True(result.Outcome == PauseOutcome.Paused, $"Pausing from {first} until {last} gave {result.Outcome}: \"{result.Message}\".");
        Assert.Equal(PauseTexts.Paused, result.Message);
        var pause = result.Pause ?? throw new InvalidOperationException("The \"paused\" result gave no pause.");
        Assert.Equal((F008Text.Date(first), F008Text.Date(last)), (pause.FirstDay, pause.LastDay));
        return (goal, result);
    }

    private async Task<RemoveExcusalResult> RemoveExcusalSucceedsAsync(string time, string email, string date)
    {
        arrange.At(time);
        var goal = arrange.OnlyGoalOf(email);
        var result = await driver.RemoveExcusalAsync(await arrange.AccountAsync(email), goal.Id, F008Text.Date(date));
        Assert.True(result.Outcome == RemoveExcusalOutcome.Removed, $"Removing the excusal of {date} gave {result.Outcome}: \"{result.Message}\".");
        Assert.Equal(ExcusalTexts.Removed, result.Message);
        return result;
    }

    // ------------------------------------------------------------ records and notes that give their time

    [Given("^at " + T + " `([^`]+)` recorded " + Value + " for " + D + "$")]
    public async Task GivenTheUserRecorded(string time, string email, string value, string date)
    {
        arrange.At(time);
        await arrange.RecordSucceedsAsync(arrange.OnlyGoalOf(email), F008Text.Date(date), value);
    }

    [Given("^at " + T + " `([^`]+)` recorded " + Value + " for each date from " + D + " to " + D + "$")]
    public async Task GivenTheUserRecordedEachDate(string time, string email, string value, string first, string last)
    {
        arrange.At(time);
        var goal = arrange.OnlyGoalOf(email);
        foreach (var date in F008Text.Range(F008Text.Date(first), F008Text.Date(last)))
        {
            await arrange.RecordSucceedsAsync(goal, date, value);
        }
    }

    [Given("^at " + T + " `([^`]+)` deleted the record of " + D + "$")]
    public async Task GivenTheUserDeletedTheRecord(string time, string email, string date)
    {
        arrange.At(time);
        var goal = arrange.OnlyGoalOf(email);
        var record = await arrange.RecordIdAsync(goal, F008Text.Date(date));
        var result = await driver.DeleteRecordAsync(await arrange.AccountAsync(email), goal.Id, record);
        Assert.True(result.Outcome == RecordWriteOutcome.Deleted, $"Deleting the record of {date} gave {result.Outcome}: \"{result.Message}\".");
    }

    [Given("^at " + T + " `([^`]+)` saved the note \"([^\"]*)\" for " + D + "$")]
    public async Task GivenTheUserSavedTheNote(string time, string email, string note, string date)
    {
        arrange.At(time);
        var goal = arrange.OnlyGoalOf(email);
        var result = await driver.SaveNoteAsync(await arrange.AccountAsync(email), goal.Id, F008Text.Date(date), note);
        Assert.True(result.Outcome == NoteSaveOutcome.Saved, $"Saving the note for {date} gave {result.Outcome}: \"{result.Message}\".");
    }

    // ------------------------------------------------------------ records that give no time (run at the When step's time)

    /// <summary>"records nothing" calls no use case (acceptance.md, "Recording"); it only names the item's account.</summary>
    [Given("^`([^`]+)` records nothing$")]
    public void GivenTheUserRecordsNothing(string email)
    {
        Assert.True(context.Accounts.ContainsKey(email), $"`{email}` is not an account of this item.");
    }

    [Given("^`([^`]+)` records " + Value + " for (" + F008Text.DateList + ")$")]
    public void GivenTheUserRecordsForDates(string email, string value, string dates)
    {
        var goal = arrange.OnlyGoalOf(email);
        arrange.DeferToWhen(async () =>
        {
            foreach (var date in F008Text.Dates(dates))
            {
                await arrange.RecordSucceedsAsync(goal, date, value);
            }
        });
    }

    /// <summary>
    /// "`x` records V for each date from D1 to D2[ except D, D and D]": V for every date of the range but the dates
    /// written after "except" (the clause sits in a group that always takes part, empty when not written).
    /// </summary>
    [Given("^`([^`]+)` records " + Value + " for each date from " + D + " to " + D + "((?: except " + F008Text.DateList + ")?)$")]
    public void GivenTheUserRecordsForEachDate(string email, string value, string first, string last, string exceptClause)
    {
        var goal = arrange.OnlyGoalOf(email);
        var skipped = F008Text.Dates(exceptClause ?? string.Empty).ToHashSet();
        var dates = F008Text.Range(F008Text.Date(first), F008Text.Date(last)).Where(date => !skipped.Contains(date)).ToList();
        arrange.DeferToWhen(async () =>
        {
            foreach (var date in dates)
            {
                await arrange.RecordSucceedsAsync(goal, date, value);
            }
        });
    }

    [Given("^`([^`]+)` records on target for each date from " + D + " to " + D + ": (\\d+) for `([^`]+)` and (\\d+) for `([^`]+)`$")]
    public void GivenTheUserRecordsOnTarget(string email, string first, string last, string firstValue, string firstGoal, string secondValue, string secondGoal)
    {
        var dates = F008Text.Range(F008Text.Date(first), F008Text.Date(last));
        arrange.DeferToWhen(async () =>
        {
            var one = await arrange.GoalNamedAsync(email, firstGoal);
            var two = await arrange.GoalNamedAsync(email, secondGoal);
            foreach (var date in dates)
            {
                await arrange.RecordSucceedsAsync(one, date, firstValue);
                await arrange.RecordSucceedsAsync(two, date, secondValue);
            }
        });
    }

    // ------------------------------------------------------------ demo tasks and catalog

    [Given("^the earlier demo tasks run at " + T + "$")]
    public Task GivenTheEarlierDemoTasksRun(string time) => arrange.EarlierDemoTasksAsync(time);

    [Given("^the demo run at " + T + "$")]
    public async Task GivenTheDemoRun(string time)
    {
        await arrange.EarlierDemoTasksAsync(time);
        context.DemoPause = await driver.LoadDemoPausesAsync(F008Text.DemoPassword);
        Assert.Equal(DemoPauseService.DemoEmail, Assert.Single(context.DemoPause.Accounts).Email);
    }

    /// <summary>"`a` unpublished the catalog habit "N" (or the catalog habits "N" and "M") with confirmation": slice 2's unpublish use case, confirmed; it succeeds.</summary>
    [Given("^`([^`]+)` unpublished the catalog habits? (\"[^\"]+\"(?: and \"[^\"]+\")?) with confirmation$")]
    public async Task GivenTheAdministratorUnpublished(string email, string names)
    {
        arrange.AtSetupTime();
        var administrator = await arrange.AccountAsync(email);
        foreach (var name in F008Text.Quoted(names))
        {
            var list = await driver.ListCatalogHabitsAsync(administrator, name);
            Assert.Equal(CatalogHabitListOutcome.Found, list.Outcome);
            var habit = Assert.Single(list.Habits, candidate => candidate.Name == name);
            var unpublished = await driver.UnpublishAsync(administrator, habit.Id, confirmed: true);
            Assert.Equal(CatalogHabitUnpublishOutcome.Unpublished, unpublished.Outcome);
        }
    }

    /// <summary>The one backticked name of an optional clause (" in `Z`", " of `G`"), or null when the clause is empty.</summary>
    private static string? BacktickedOrNull(string clause) =>
        string.IsNullOrEmpty(clause) ? null : F008Text.Backticked(clause).Single();

    private static string? ReasonOf(string phrase) =>
        phrase == "without a reason" ? null : F008Text.Reason(phrase["with the reason ".Length..]);
}
