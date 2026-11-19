using System.Text.RegularExpressions;
using HT.AcceptanceTests.Support.F005;
using HT.Application.Accounts;
using HT.Application.Habits;
using HT.Domain.Habits;

namespace HT.AcceptanceTests.StepDefinitions.F005;

/// <summary>The Given steps of F005 that arrange accounts, habits, goals, records, notes and time zones.</summary>
[Binding]
[Scope(Tag = "F005")]
public sealed class GivenSteps(F005Context context, F005Driver driver, F005Arrange arrange, F005Actions actions)
{
    private const string Date = @"(\d{4}-\d{2}-\d{2})";
    /// <summary>
    /// The optional "at `T` " prefix. The group always takes part in the match (an empty capture means the step gives
    /// no time), so the method's parameter count stays equal to the number of captures; <see cref="F005Text.AtTime"/>
    /// reads the time.
    /// </summary>
    private const string At = "((?:at `[^`]+` )?)";
    private const string Habit = "(their custom habit `[^`]+`|the catalog habit `[^`]+`)";

    private static readonly Regex SecondGoal = new(
        "^, and then a second goal on (their custom habit `[^`]+`) with (.+?) and the start date " + Date + "$",
        RegexOptions.CultureInvariant);

    // ------------------------------------------------------------ accounts and habits

    /// <summary>The time zone group always takes part; an empty capture means no time zone was given (UTC).</summary>
    [Given(@"^the registered user `([^`]+)`((?: in `[^`]+`| in [A-Za-z_/]+)?), who created the custom habit `([^`]+)`$")]
    public async Task GivenTheRegisteredUserWhoCreatedTheCustomHabit(string email, string zone, string habit)
    {
        await arrange.RegisteredUserAsync(email, F005Text.ZoneOf(zone));
        await arrange.CustomHabitAsync(email, habit);
    }

    [Given(@"^the registered user `([^`]+)`$")]
    public Task GivenTheRegisteredUser(string email) => arrange.RegisteredUserAsync(email, null);

    [Given(@"^the administrator `([^`]+)`$")]
    public Task GivenTheAdministrator(string email) => arrange.AdministratorAsync(email);

    /// <summary>"the administrator `a`, who created a Published `N`" and "the administrator `a` created a Published `Y`, a Draft `R` and an Unpublished `S`".</summary>
    [Given(@"^the administrator `([^`]+)`(?:, who)? created (an? (?:Published|Draft|Unpublished) `.+)$")]
    public async Task GivenTheAdministratorCreatedCatalogHabits(string email, string habits)
    {
        await arrange.AdministratorAsync(email);
        var items = Regex.Matches(habits, "an? (Published|Draft|Unpublished) `([^`]+)`");
        Assert.NotEmpty(items);
        foreach (Match item in items)
        {
            await arrange.CatalogHabitAsync(email, Enum.Parse<CatalogHabitState>(item.Groups[1].Value), item.Groups[2].Value);
        }
    }

    [Given(@"^at `([^`]+)` `([^`]+)` unpublished `([^`]+)` with confirmation, as .+, and its state is Unpublished$")]
    public async Task GivenTheAdministratorUnpublishedWithConfirmation(string time, string email, string habit)
    {
        await arrange.BeginStepAsync(time);
        var administrator = await arrange.AccountAsync(email);
        var id = context.CatalogHabits[habit];
        var unpublished = await driver.UnpublishAsync(administrator, id, confirmed: true);
        Assert.Equal(CatalogHabitUnpublishOutcome.Unpublished, unpublished.Outcome);
        var read = await driver.GetCatalogHabitAsync(administrator, id);
        Assert.Equal(CatalogHabitAdminOutcome.Found, read.Outcome);
        Assert.NotNull(read.Habit);
        Assert.Equal(CatalogHabitState.Unpublished, read.Habit.State);
    }

    [Given(@"^at `([^`]+)` `([^`]+)` changed their time zone to `([^`]+)`, and the change succeeded$")]
    public async Task GivenTheUserChangedTheirTimeZone(string time, string email, string zone)
    {
        await arrange.BeginStepAsync(time);
        var changed = await driver.ChangeTimeZoneAsync(await arrange.AccountAsync(email), zone);
        Assert.Equal(TimeZoneChangeOutcome.Changed, changed.Outcome);
    }

    // ------------------------------------------------------------ goals

    /// <summary>
    /// One goal, optionally followed by ", and then a second goal on … with … and the start date D" (AC-041). The
    /// second-goal group always takes part in the match; an empty capture means the step creates one goal.
    /// </summary>
    [Given("^" + At + "`([^`]+)` created a goal on " + Habit + " with (.+?) and the start date " + Date
        + "((?:, and then a second goal on their custom habit `[^`]+` with .+? and the start date " + F005Text.D + ")?)$")]
    public async Task GivenTheUserCreatedAGoal(
        string time,
        string email,
        string habit,
        string settings,
        string startDate,
        string second)
    {
        await arrange.BeginStepAsync(F005Text.AtTime(time));
        await arrange.CreateGoalAsync(email, habit, settings, F005Text.Date(startDate));
        if (second.Length == 0)
        {
            return;
        }

        var match = SecondGoal.Match(second);
        if (!match.Success)
        {
            throw new InvalidOperationException($"Cannot read the second goal \"{second}\".");
        }

        await arrange.CreateGoalAsync(email, match.Groups[1].Value, match.Groups[2].Value, F005Text.Date(match.Groups[3].Value));
    }

    [Given("^" + At + "`([^`]+)` created two goals on " + Habit + ", each with (.+) and the start date " + Date + "$")]
    public async Task GivenTheUserCreatedTwoGoals(string time, string email, string habit, string settings, string startDate)
    {
        await arrange.BeginStepAsync(F005Text.AtTime(time));
        await arrange.CreateGoalAsync(email, habit, settings, F005Text.Date(startDate));
        await arrange.CreateGoalAsync(email, habit, settings, F005Text.Date(startDate));
    }

    [Given(@"^no other use case is called for the goal after it was created$")]
    public async Task GivenNoOtherUseCaseIsCalled()
    {
        await arrange.BeginStepAsync(null);
        var goal = arrange.OnlyGoal;
        Assert.Equal(0, driver.Calls(goal.Id));
        context.CallsFrozenUntilWhen.Add((goal.Id, driver.Calls(goal.Id)));
    }

    // ------------------------------------------------------------ records and notes

    [Given("^" + At + "`([^`]+)` recorded (.+)$")]
    public async Task GivenTheUserRecorded(string time, string email, string records)
    {
        await arrange.BeginStepAsync(F005Text.AtTime(time));
        await actions.GivenRecordedAsync(email, records);
    }

    [Given("^" + At + "`([^`]+)` saved(?:, in this order,)? (.+)$")]
    public async Task GivenTheUserSavedNotes(string time, string email, string notes)
    {
        await arrange.BeginStepAsync(F005Text.AtTime(time));
        await actions.GivenSavedNotesAsync(email, notes);
    }
}
