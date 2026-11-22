using System.Text.RegularExpressions;
using HT.AcceptanceTests.Support.F009;
using HT.Application.Accounts;
using HT.Application.Exemptions;
using HT.Application.Goals;
using HT.Application.Habits;
using HT.Application.Operator;
using HT.Application.Records;
using HT.Domain.Habits;

namespace HT.AcceptanceTests.StepDefinitions.F009;

/// <summary>
/// The Given steps of F009: accounts, habits and goals, and the changes, endings, restarts, records, notes, excusals
/// and pauses that set an item up. Each must succeed (and give the result it names, when it names one).
/// </summary>
[Binding]
[Scope(Tag = "F009")]
public sealed class GivenSteps(F009Context context, F009Driver driver, F009Arrange arrange)
{
    private const string D = "(" + F009Text.D + ")";
    private const string T = "(" + F009Text.T + ")";
    private const string Value = "(Yes|No|\\d+)";
    private const string O = "(" + F009Text.Outcomes + ")";

    private static readonly Regex CreatedGoal = new(
        "^created the goal `(?<goal>[^`]+)`(?: at (?<time>" + F009Text.T + ") with the start date (?<start>" + F009Text.D + "))?$",
        RegexOptions.CultureInvariant);

    private static readonly Regex CreatedInOrder = new("^created, in this order, (?<goals>the goal `.+)$", RegexOptions.CultureInvariant);

    private static readonly Regex CatalogGoals = new(
        "^at (?<time>" + F009Text.T + ") created (?<goals>a goal on the catalog habit .+?), (?:each with )?the start date (?<start>" + F009Text.D + ") and no name$",
        RegexOptions.CultureInvariant);

    private static readonly Regex CatalogGoal = new(
        "^a goal on the catalog habit \"(?<habit>[^\"]+)\" with (?<settings>.+)$",
        RegexOptions.CultureInvariant);

    // ------------------------------------------------------------ accounts and goals

    /// <summary>
    /// "the registered user `x`[ in `Z`], who …": registration and sign-in run at the earliest time of the item (the
    /// goal creation, 2026-11-02T08:00:00Z unless the item gives another time, or an earlier step's time); then what the
    /// clause names: "created the goal `G`[ at T with the start date D]", "created, in this order, the goal `A`, …",
    /// "has the ended Meditation goal", "has the three goals", or "at T created a goal on the catalog habit "H" with S,
    /// the start date D and no name" (one or more such goals).
    /// </summary>
    [Given("^the registered user `([^`]+)`((?: in `[^`]+`)?), who (.+)$")]
    public async Task GivenTheRegisteredUserWho(string email, string zoneClause, string clause)
    {
        var zone = string.IsNullOrEmpty(zoneClause) ? null : F009Text.Backticked(zoneClause).Single();
        Match match;
        if ((match = CreatedGoal.Match(clause)).Success)
        {
            var creation = match.Groups["time"].Success ? F009Text.Instant(match.Groups["time"].Value) : F009Driver.DefaultGoalCreation;
            var start = match.Groups["start"].Success ? F009Text.Date(match.Groups["start"].Value) : F009Driver.DefaultStartDate;
            await RegisterAtSetupAsync(email, zone, creation);
            arrange.At(creation);
            await arrange.CreateGoalAsync(email, match.Groups["goal"].Value, start);
        }
        else if ((match = CreatedInOrder.Match(clause)).Success)
        {
            await RegisterAtSetupAsync(email, zone, F009Driver.DefaultGoalCreation);
            arrange.At(F009Driver.DefaultGoalCreation);
            foreach (var goal in F009Text.Backticked(match.Groups["goals"].Value))
            {
                await arrange.CreateGoalAsync(email, goal, F009Driver.DefaultStartDate);
            }
        }
        else if (clause == "has the ended Meditation goal")
        {
            await RegisterAtSetupAsync(email, zone, F009Driver.DefaultGoalCreation);
            await arrange.EndedMeditationGoalAsync(email);
        }
        else if (clause == "has the three goals")
        {
            await RegisterAtSetupAsync(email, zone, F009Driver.DefaultGoalCreation);
            await arrange.ThreeGoalsAsync(email);
        }
        else if ((match = CatalogGoals.Match(clause)).Success)
        {
            var creation = F009Text.Instant(match.Groups["time"].Value);
            await RegisterAtSetupAsync(email, zone, creation);
            arrange.At(creation);
            var start = F009Text.Date(match.Groups["start"].Value);
            var goals = Regex.Split(match.Groups["goals"].Value, "(?:, and |, | and )(?=a goal on the catalog habit)");
            foreach (var goal in goals)
            {
                var one = CatalogGoal.Match(goal);
                if (!one.Success)
                {
                    throw new InvalidOperationException($"Cannot read \"{goal}\".");
                }

                await arrange.CreateCatalogGoalAsync(email, one.Groups["habit"].Value, F009Text.Settings(one.Groups["settings"].Value), start);
            }
        }
        else
        {
            throw new InvalidOperationException($"Cannot read \"who {clause}\".");
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

    /// <summary>"the administrator `a`, who at T unpublished the catalog habit "H" with confirmation": it is unpublished.</summary>
    [Given("^the administrator `([^`]+)`, who at " + T + " unpublished the catalog habit \"([^\"]+)\" with confirmation$")]
    public async Task GivenTheAdministratorWhoUnpublished(string email, string time, string habit)
    {
        arrange.AtSetupTime();
        await arrange.AdministratorAsync(email);
        arrange.At(time);
        await UnpublishConfirmedAsync(email, habit);
    }

    /// <summary>
    /// "the administrator `a`, who created and published the catalog habit "H" with the intent I, the category C and
    /// the icon `X`": slice 2's create (no description, no suggested starting goal) and publish use cases; both succeed.
    /// </summary>
    [Given("^the administrator `([^`]+)`, who created and published the catalog habit \"([^\"]+)\" with the intent (Build|Reduce), the category (\\w+) and the icon `([^`]+)`$")]
    public async Task GivenTheAdministratorWhoCreatedAndPublished(string email, string habit, string intent, string category, string icon)
    {
        arrange.AtSetupTime();
        await arrange.AdministratorAsync(email);
        var administrator = await arrange.AccountAsync(email);
        var created = await driver.CreateCatalogHabitAsync(administrator, new CatalogHabitInput
        {
            Name = habit,
            Description = null,
            Category = Enum.Parse<Category>(category),
            Intent = Enum.Parse<Intent>(intent),
            Icon = icon,
            SuggestedStartingGoals = [],
        });
        Assert.True(created.Outcome == CatalogHabitCreateOutcome.Created, $"Creating \"{habit}\" gave {created.Outcome}: \"{created.Message}\".");
        var id = created.HabitId ?? throw new InvalidOperationException($"Creating \"{habit}\" gave no identifier.");
        var published = await driver.PublishAsync(administrator, id);
        Assert.Equal(CatalogHabitPublishOutcome.Published, published.Outcome);
        context.CatalogHabits[habit] = id;
    }

    /// <summary>"`a` unpublished the catalog habit "N" (or the catalog habits "N", "M" and "P") with confirmation": each is unpublished.</summary>
    [Given("^`([^`]+)` unpublished the catalog habits? (\"[^\"]+\"(?:(?:, | and )\"[^\"]+\")*) with confirmation$")]
    public async Task GivenTheAdministratorUnpublished(string email, string names)
    {
        arrange.AtSetupTime();
        foreach (var name in F009Text.Quoted(names))
        {
            await UnpublishConfirmedAsync(email, name);
        }
    }

    // ------------------------------------------------------------ goal changes that set an item up

    /// <summary>"at T `x` changed the amount to A minutes[, which gave "settings changed" with the date D]".</summary>
    [Given("^at " + T + " `([^`]+)` changed the amount to (\\d+) minutes((?:, which gave \"[^\"]+\" with the date " + F009Text.D + ")?)$")]
    public async Task GivenTheUserChangedTheAmount(string time, string email, string amount, string gaveClause)
    {
        arrange.At(time);
        var goal = arrange.OnlyGoalOf(email);
        var input = await arrange.AmountChangeAsync(goal, F009Text.Int(amount));
        var result = await driver.ChangeSettingsAsync(await arrange.AccountAsync(email), goal.Id, input);
        RequireChanged(result, gaveClause);
    }

    /// <summary>"at T `x` changed the settings of the goal to S[ with the name "N"][, which gave "settings changed" with the date D]".</summary>
    [Given("^at " + T + " `([^`]+)` changed the settings of the goal to (.+?)((?: with the name \"[^\"]*\")?)((?:, which gave \"[^\"]+\" with the date " + F009Text.D + ")?)$")]
    public async Task GivenTheUserChangedTheSettings(string time, string email, string settings, string nameClause, string gaveClause)
    {
        arrange.At(time);
        var goal = arrange.OnlyGoalOf(email);
        var name = string.IsNullOrEmpty(nameClause) ? null : F009Text.Quoted(nameClause).Single();
        var result = await driver.ChangeSettingsAsync(
            await arrange.AccountAsync(email),
            goal.Id,
            new GoalSettingsInput { Target = F009Text.Settings(settings).ToFields(), Name = string.IsNullOrEmpty(name) ? null : name });
        RequireChanged(result, gaveClause);
    }

    /// <summary>"at T `x` changed the account's time zone to `Z`": slice 4's time zone change; it succeeds.</summary>
    [Given("^at " + T + " `([^`]+)` changed the account's time zone to `([^`]+)`$")]
    public async Task GivenTheUserChangedTheTimeZone(string time, string email, string zone)
    {
        arrange.At(time);
        var result = await driver.ChangeTimeZoneAsync(await arrange.AccountAsync(email), zone);
        Assert.True(result.Outcome == TimeZoneChangeOutcome.Changed, $"Changing the time zone to {zone} gave {result.Outcome}: \"{result.Message}\".");
    }

    /// <summary>
    /// "at T `x` ended the goal|`G` with the outcome O and the note "N"|and no note[, which gave "goal ended" with the
    /// ending date D and the last date D]".
    /// </summary>
    [Given("^at " + T + " `([^`]+)` ended (the goal|`[^`]+`) with the outcome " + O + " and (no note|the note \"[^\"]*\")((?:, which gave \"[^\"]+\" with the ending date " + F009Text.D + " and the last date " + F009Text.D + ")?)$")]
    public async Task GivenTheUserEnded(string time, string email, string goalPhrase, string outcome, string notePhrase, string gaveClause)
    {
        arrange.At(time);
        var goal = await arrange.GoalOfPhraseAsync(email, goalPhrase);
        var note = notePhrase == "no note" ? null : F009Text.Text(notePhrase["the note ".Length..]);
        var result = await arrange.EndSucceedsAsync(goal, F009Text.OutcomeOf(outcome), note);
        var gave = Regex.Match(gaveClause, "^, which gave \"([^\"]+)\" with the ending date (" + F009Text.D + ") and the last date (" + F009Text.D + ")$");
        if (gave.Success)
        {
            Assert.Equal(gave.Groups[1].Value, result.Message);
            Assert.Equal(F009Text.Date(gave.Groups[2].Value), result.EndingDate);
            Assert.Equal(F009Text.Date(gave.Groups[3].Value), result.LastDate);
        }
    }

    /// <summary>"at T `x` started a new attempt[ of `G`] with S and the start date D[, which gave "attempt started" with the number N]".</summary>
    [Given("^at " + T + " `([^`]+)` started a new attempt((?: of `[^`]+`)?) with (.+) and the start date " + D + "((?:, which gave \"[^\"]+\" with the number \\d+)?)$")]
    public async Task GivenTheUserStartedANewAttempt(string time, string email, string goalClause, string settings, string start, string gaveClause)
    {
        arrange.At(time);
        var goal = string.IsNullOrEmpty(goalClause)
            ? arrange.OnlyGoalOf(email)
            : await arrange.GoalNamedAsync(email, F009Text.Backticked(goalClause).Single());
        var result = await arrange.StartAttemptSucceedsAsync(goal, F009Text.Settings(settings), F009Text.Date(start));
        var gave = Regex.Match(gaveClause, "^, which gave \"([^\"]+)\" with the number (\\d+)$");
        if (gave.Success)
        {
            Assert.Equal(gave.Groups[1].Value, result.Message);
            Assert.Equal(F009Text.Int(gave.Groups[2].Value), result.AttemptNumber);
        }
    }

    // ------------------------------------------------------------ records, notes, excusals and pauses that set an item up

    /// <summary>"at T `x` records V for each date from D1 to D2[ except D, D and D]": each record must succeed.</summary>
    [Given("^at " + T + " `([^`]+)` records " + Value + " for each date from " + D + " to " + D + "((?: except " + F009Text.DateList + ")?)$")]
    public async Task GivenTheUserRecordsForEachDate(string time, string email, string value, string first, string last, string exceptClause)
    {
        arrange.At(time);
        var goal = arrange.OnlyGoalOf(email);
        var skipped = F009Text.Dates(exceptClause ?? string.Empty).ToHashSet();
        foreach (var date in F009Text.Range(F009Text.Date(first), F009Text.Date(last)).Where(date => !skipped.Contains(date)))
        {
            await arrange.RecordSucceedsAsync(goal, date, value);
        }
    }

    [Given("^at " + T + " `([^`]+)` records " + Value + " for each of (" + F009Text.DateList + ")$")]
    public async Task GivenTheUserRecordsForEachOf(string time, string email, string value, string dates)
    {
        arrange.At(time);
        var goal = arrange.OnlyGoalOf(email);
        foreach (var date in F009Text.Dates(dates))
        {
            await arrange.RecordSucceedsAsync(goal, date, value);
        }
    }

    [Given("^at " + T + " `([^`]+)` records " + Value + " for " + D + "$")]
    public async Task GivenTheUserRecordsFor(string time, string email, string value, string date)
    {
        arrange.At(time);
        await arrange.RecordSucceedsAsync(arrange.OnlyGoalOf(email), F009Text.Date(date), value);
    }

    [Given("^at " + T + " `([^`]+)` saved the note \"([^\"]*)\" for " + D + "$")]
    public async Task GivenTheUserSavedTheNote(string time, string email, string note, string date)
    {
        arrange.At(time);
        var goal = arrange.OnlyGoalOf(email);
        var result = await driver.SaveNoteAsync(await arrange.AccountAsync(email), goal.Id, F009Text.Date(date), note);
        Assert.True(result.Outcome == NoteSaveOutcome.Saved, $"Saving the note for {date} gave {result.Outcome}: \"{result.Message}\".");
    }

    [Given("^at " + T + " `([^`]+)` excused " + D + " without a reason$")]
    public async Task GivenTheUserExcused(string time, string email, string date)
    {
        arrange.At(time);
        var goal = arrange.OnlyGoalOf(email);
        var result = await driver.ExcuseAsync(await arrange.AccountAsync(email), goal.Id, F009Text.Date(date), null);
        Assert.True(result.Outcome == ExcuseOutcome.Excused, $"Excusing {date} gave {result.Outcome}: \"{result.Message}\".");
    }

    [Given("^at " + T + " `([^`]+)` paused the goal from " + D + " until " + D + " without a reason$")]
    public async Task GivenTheUserPaused(string time, string email, string first, string last)
    {
        arrange.At(time);
        var goal = arrange.OnlyGoalOf(email);
        var result = await driver.PauseAsync(await arrange.AccountAsync(email), goal.Id, F009Text.Date(first), F009Text.Date(last), null);
        Assert.True(result.Outcome == PauseOutcome.Paused, $"Pausing from {first} until {last} gave {result.Outcome}: \"{result.Message}\".");
    }

    /// <summary>"at T `x` renamed the custom habit `C` to "N"": slice 4's custom habit edit with only the name changed; it succeeds.</summary>
    [Given("^at " + T + " `([^`]+)` renamed the custom habit `([^`]+)` to \"([^\"]+)\"$")]
    public async Task GivenTheUserRenamedTheCustomHabit(string time, string email, string habit, string name)
    {
        arrange.At(time);
        var user = await arrange.AccountAsync(email);
        var id = context.CustomHabits[(email, habit)];
        var read = await driver.GetCustomHabitAsync(user, id);
        Assert.Equal(CustomHabitOutcome.Found, read.Outcome);
        var current = read.Habit ?? throw new InvalidOperationException($"Reading `{habit}` gave no habit.");
        var edited = await driver.EditCustomHabitAsync(user, id, new HabitInput
        {
            Name = name,
            Description = current.Description,
            Category = current.Category,
            Intent = current.Intent,
            Icon = current.Icon,
        });
        Assert.True(edited.Outcome == CustomHabitEditOutcome.Saved, $"Renaming `{habit}` gave {edited.Outcome}: \"{edited.Message}\".");
    }

    // ------------------------------------------------------------ demo tasks and catalog

    [Given("^the catalog is seeded at " + T + "$")]
    public Task GivenTheCatalogIsSeeded(string time) => arrange.CatalogSeededAsync(time);

    [Given("^the earlier demo tasks run at " + T + "$")]
    public Task GivenTheEarlierDemoTasksRun(string time) => arrange.EarlierDemoTasksAsync(time);

    /// <summary>"demo goal-change loading ran at T with the demo password `P`": it loads and creates `demo7@example.com`.</summary>
    [Given("^demo goal-change loading ran at " + T + " with the demo password `([^`]+)`$")]
    public async Task GivenDemoGoalChangeLoadingRan(string time, string password)
    {
        arrange.At(time);
        var result = await driver.LoadDemoGoalChangesAsync(password);
        Assert.Equal(DemoGoalChangeLoadingOutcome.Loaded, result.Outcome);
        var account = Assert.Single(result.Accounts);
        Assert.Equal(DemoGoalChangeService.DemoEmail, account.Email);
        Assert.Equal(HT.Application.Operator.DemoAccountOutcome.Created, account.Outcome);
    }

    // ------------------------------------------------------------ helpers

    private async Task RegisterAtSetupAsync(string email, string? zone, DateTimeOffset creation)
    {
        // Setup that gives no time runs at the earliest time the item gives; an earlier step may have fixed an earlier one.
        arrange.At(context.SetupTime is { } setup && setup < creation ? setup : creation);
        await arrange.RegisteredUserAsync(email, zone);
    }

    private async Task UnpublishConfirmedAsync(string email, string habit)
    {
        var administrator = await arrange.AccountAsync(email);
        var id = await arrange.CatalogHabitIdAsAdministratorAsync(administrator, habit);
        var unpublished = await driver.UnpublishAsync(administrator, id, confirmed: true);
        Assert.True(unpublished.Outcome == CatalogHabitUnpublishOutcome.Unpublished, $"Unpublishing \"{habit}\" gave {unpublished.Outcome}: \"{unpublished.Message}\".");
    }

    private static void RequireChanged(SettingsChangeResult result, string gaveClause)
    {
        Assert.True(result.Outcome == SettingsChangeOutcome.Changed, $"Changing the settings gave {result.Outcome}: \"{result.Message}\".");
        var gave = Regex.Match(gaveClause, "^, which gave \"([^\"]+)\" with the date (" + F009Text.D + ")$");
        if (gave.Success)
        {
            Assert.Equal(gave.Groups[1].Value, result.Message);
            Assert.Equal(F009Text.Date(gave.Groups[2].Value), result.AppliesFrom);
        }
    }
}
