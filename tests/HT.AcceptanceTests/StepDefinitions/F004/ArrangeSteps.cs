using System.Globalization;
using System.Text.RegularExpressions;
using HT.AcceptanceTests.Support.F004;
using HT.Application.Accounts;
using HT.Application.Goals;
using HT.Application.Habits;
using HT.Application.Operator;
using HT.Domain.Habits;

namespace HT.AcceptanceTests.StepDefinitions.F004;

/// <summary>The Given steps of F004: accounts, catalog and custom habits, goals, time zone changes.</summary>
[Binding]
[Scope(Tag = "F004")]
public sealed class ArrangeSteps(F004Context context, F004Driver driver, F004Arrange arrange)
{
    // ------------------------------------------------------------ administrators and catalog habits

    [Given(@"^the administrator `([^`]+)`$")]
    public async Task GivenTheAdministrator(string email)
    {
        await arrange.AdministratorAsync(email);
    }

    [Given(@"^the administrator `([^`]+)` and the registered user `([^`]+)`$")]
    public async Task GivenTheAdministratorAndTheRegisteredUser(string administrator, string user)
    {
        await arrange.AdministratorAsync(administrator);
        await arrange.RegisteredUserAsync(user, null);
    }

    [Given(@"^the administrator `([^`]+)`(?:,| and) (an? (?:Published|Draft|Unpublished) .+)$")]
    public async Task GivenTheAdministratorAndCatalogHabits(string administrator, string habits)
    {
        await arrange.AdministratorAsync(administrator);
        await arrange.CatalogHabitsAsync(habits);
    }

    [Given(@"^the administrator `([^`]+)` created (an? (?:Published|Draft|Unpublished) .+)$")]
    public async Task GivenTheAdministratorCreatedCatalogHabits(string administrator, string habits)
    {
        await arrange.AdministratorAsync(administrator);
        await arrange.CatalogHabitsAsync(habits);
    }

    [Given(@"^(an? (?:Published|Draft|Unpublished) `[^`]+`.*)$")]
    public async Task GivenCatalogHabits(string habits)
    {
        await arrange.CatalogHabitsAsync(habits);
    }

    /// <summary>The " and then published it again" group always takes part in the match; an empty capture means the step does not say it.</summary>
    [Given(@"^the administrator unpublished `([^`]+)` (without confirming|with confirmation)((?: and then published it again)?)$")]
    public async Task GivenTheAdministratorUnpublished(string habit, string confirmation, string again)
    {
        arrange.AtSetupTime();
        var record = context.CatalogHabit(habit);
        var unpublished = await driver.UnpublishAsync(context.Administrator, record.Id, confirmation == "with confirmation");
        Assert.Equal(CatalogHabitUnpublishOutcome.Unpublished, unpublished.Outcome);
        if (!string.IsNullOrEmpty(again))
        {
            var published = await driver.PublishAsync(context.Administrator, record.Id);
            Assert.Equal(CatalogHabitPublishOutcome.Published, published.Outcome);
        }
    }

    [Given(@"^`([^`]+)` unpublished `([^`]+)` without confirming, and the result was ""confirmation needed"" with the number of goals (\d+) and the text ""([^""]*)""$")]
    public async Task GivenUnpublishedWithoutConfirmingAndTheResultWas(string administrator, string habit, int count, string text)
    {
        arrange.AtSetupTime();
        var result = await driver.UnpublishAsync(context.Account(administrator), context.CatalogHabit(habit).Id, confirmed: false);
        Assert.Equal(CatalogHabitUnpublishOutcome.ConfirmationNeeded, result.Outcome);
        Assert.Equal(count, result.GoalCount);
        Assert.Equal(text, result.Message);
    }

    [Given(@"^`([^`]+)` edited `([^`]+)` so that (its only suggested starting goal is .+|it has no suggested starting goal)$")]
    public async Task GivenTheAdministratorEditedTheSuggestions(string administrator, string habit, string change)
    {
        arrange.AtStepTime(null);
        var record = context.CatalogHabit(habit);
        record.Spec.Suggestions.Clear();
        const string Only = "its only suggested starting goal is ";
        if (change.StartsWith(Only, StringComparison.Ordinal))
        {
            record.Spec.Suggestions.Add(F004Text.Settings(change[Only.Length..]).ToSuggestion(marked: true));
        }

        var edited = await driver.EditCatalogHabitAsync(context.Account(administrator), record.Id, record.Spec.ToInput());
        Assert.Equal(CatalogHabitEditOutcome.Saved, edited.Outcome);
    }

    // ------------------------------------------------------------ registered users

    /// <summary>
    /// The " in `Z`" and ", who ..." groups always take part in the match; an empty capture means the step does not
    /// give that part. With no " in `Z`" the user registers with no time zone (acceptance.md's "registered user").
    /// </summary>
    [Given(@"^the registered user `([^`]+)`((?: in `[^`]+`)?)((?:, who .+)?)$")]
    public async Task GivenTheRegisteredUser(string email, string inClause, string whoClause)
    {
        await arrange.RegisteredUserAsync(email, F004Text.BacktickedIn(inClause));
        const string Who = ", who ";
        if (whoClause.Length > 0)
        {
            await WhoAsync(email, whoClause[Who.Length..]);
        }
    }

    [Given(@"^the registered user `([^`]+)` and the administrator `([^`]+)`$")]
    public async Task GivenTheRegisteredUserAndTheAdministrator(string user, string administrator)
    {
        await arrange.RegisteredUserAsync(user, null);
        await arrange.AdministratorAsync(administrator);
    }

    [Given(@"^the registered users `([^`]+)` and `([^`]+)`$")]
    public async Task GivenTheRegisteredUsers(string first, string second)
    {
        await arrange.RegisteredUserAsync(first, null);
        await arrange.RegisteredUserAsync(second, null);
    }

    [Given(@"^the registered users `([^`]+)` and `([^`]+)`, who each created a goal on (`[^`]+`) with (.+)$")]
    public async Task GivenTheRegisteredUsersWhoEachCreatedAGoal(string first, string second, string habit, string settings)
    {
        await GivenTheRegisteredUsers(first, second);
        arrange.AtStepTime(null);
        await arrange.CreatedGoalAsync(first, habit, F004Text.Settings(settings));
        await arrange.CreatedGoalAsync(second, habit, F004Text.Settings(settings));
    }

    [Given(@"^(`[^`]+` created (?:one goal|two goals)(?: and `[^`]+` created (?:one goal|two goals))?), each on (`[^`]+`) with (.+)$")]
    public async Task GivenEachCreatedGoalsOn(string owners, string habit, string settings)
    {
        arrange.AtStepTime(null);
        foreach (Match owner in Regex.Matches(owners, "`([^`]+)` created (one goal|two goals)"))
        {
            var count = owner.Groups[2].Value == "two goals" ? 2 : 1;
            for (var index = 0; index < count; index++)
            {
                await arrange.CreatedGoalAsync(owner.Groups[1].Value, habit, F004Text.Settings(settings));
            }
        }
    }

    /// <summary>The "who ..." part of "the registered user `x`, who ...".</summary>
    private async Task WhoAsync(string email, string who)
    {
        Match match;
        if ((match = Regex.Match(who, "^created the custom habit `([^`]+)`(?: with the intent (Build|Reduce))?$")).Success)
        {
            Intent? intent = match.Groups[2].Success ? Enum.Parse<Intent>(match.Groups[2].Value) : null;
            await arrange.CustomHabitAsync(email, match.Groups[1].Value, intent);
        }
        else if ((match = Regex.Match(who, "^created the custom habits `([^`]+)` and `([^`]+)`$")).Success)
        {
            await arrange.CustomHabitAsync(email, match.Groups[1].Value);
            await arrange.CustomHabitAsync(email, match.Groups[2].Value);
        }
        else if ((match = Regex.Match(who, "^changed their name to \"([^\"]*)\"$")).Success)
        {
            arrange.AtStepTime(null);
            var changed = await driver.ChangeNameAsync(context.Account(email), match.Groups[1].Value);
            Assert.Equal(NameChangeOutcome.Changed, changed.Outcome);
        }
        else if ((match = Regex.Match(who, "^started a new goal from (`[^`]+`), and the starting values were (.+)$")).Success)
        {
            arrange.AtStepTime(null);
            var values = await arrange.StartedAsync(email, match.Groups[1].Value);
            F004Assert.StartingValues(values, match.Groups[2].Value, CatalogIdOf);
        }
        else if ((match = Regex.Match(who, "^(?:then )?created a goal G on (`[^`]+`) with (.+)$")).Success)
        {
            arrange.AtStepTime(null);
            await arrange.CreatedGoalAsync(email, match.Groups[1].Value, F004Text.Settings(match.Groups[2].Value), alias: "G");
        }
        else if ((match = Regex.Match(who, "^(?:then )?created a goal on (`[^`]+`|their custom habit `[^`]+`) with (.+)$")).Success)
        {
            arrange.AtStepTime(null);
            foreach (var (habit, settings) in F004Arrange.GoalChain(match.Groups[1].Value, match.Groups[2].Value))
            {
                await arrange.CreatedGoalAsync(email, habit, settings);
            }
        }
        else if ((match = Regex.Match(who, "^created goals on (`[^`]+`), in this order: (.+), each with (.+)$")).Success)
        {
            arrange.AtStepTime(null);
            foreach (Match item in Regex.Matches(match.Groups[2].Value, "one named \"([^\"]*)\"|one with no name"))
            {
                var settings = F004Text.Settings(match.Groups[3].Value);
                settings.SetName(item.Groups[1].Success ? item.Groups[1].Value : null);
                await arrange.CreatedGoalAsync(email, match.Groups[1].Value, settings);
            }
        }
        else if ((match = Regex.Match(who, "^created ((?:(?:two goals|one goal|a goal) on `[^`]+`(?:, | and )?)+), each with (.+)$")).Success)
        {
            arrange.AtStepTime(null);
            foreach (Match item in Regex.Matches(match.Groups[1].Value, "(two goals|one goal|a goal) on (`[^`]+`)"))
            {
                var count = item.Groups[1].Value == "two goals" ? 2 : 1;
                for (var index = 0; index < count; index++)
                {
                    await arrange.CreatedGoalAsync(email, item.Groups[2].Value, F004Text.Settings(match.Groups[2].Value));
                }
            }
        }
        else if ((match = Regex.Match(who, "^created two goals on (`[^`]+`|their custom habit `[^`]+`) with (.+)$")).Success)
        {
            arrange.AtStepTime(null);
            await arrange.CreatedGoalAsync(email, match.Groups[1].Value, F004Text.Settings(match.Groups[2].Value));
            await arrange.CreatedGoalAsync(email, match.Groups[1].Value, F004Text.Settings(match.Groups[2].Value));
        }
        else
        {
            throw new InvalidOperationException($"Cannot arrange \"who {who}\".");
        }
    }

    // ------------------------------------------------------------ registration by the anonymous visitor

    /// <summary>The "at `T` " prefix group always takes part in the match; an empty capture means the step gives no time.</summary>
    [Given("^" + F004Text.AtPrefix + "the anonymous visitor registered the email `([^`]+)` with the password `([^`]+)`(?:,| and) (.+), and signed in$")]
    public async Task GivenTheAnonymousVisitorRegistered(string at, string email, string password, string options)
    {
        var time = F004Text.AtTime(at);
        if (string.IsNullOrEmpty(time))
        {
            arrange.AtSetupTime();
        }
        else
        {
            arrange.AtStepTime(time);
        }

        string? timeZone = null;
        string? name = null;
        foreach (var option in options.Split(" and "))
        {
            Match match;
            if ((match = Regex.Match(option, "^the browser time zone `([^`]*)`$")).Success)
            {
                timeZone = F004Text.TextOf(match.Groups[1].Value);
            }
            else if ((match = Regex.Match(option, "^the name \"([^\"]*)\"$")).Success)
            {
                name = match.Groups[1].Value;
            }
            else if ((match = Regex.Match(option, "^the name `([^`]*)`$")).Success)
            {
                name = F004Text.TextOf(match.Groups[1].Value);
            }
            else if (option is not ("no browser time zone" or "no name"))
            {
                throw new InvalidOperationException($"Cannot read the registration option \"{option}\".");
            }
        }

        var registered = await driver.RegisterAsync(email, password, timeZone, name);
        context.Registrations.Add(registered);
        await arrange.SignInAsync(email, password);
        context.CurrentRegisteredUserEmail = email;
        context.TimeZones[email] = F004Arrange.ZoneForToday(timeZone);
    }

    [Given(@"^the current culture of the test is (the invariant culture|`[^`]+`)$")]
    public void GivenTheCurrentCulture(string culture)
    {
        context.Culture = culture == "the invariant culture"
            ? CultureInfo.InvariantCulture
            : CultureInfo.GetCultureInfo(F004Text.Email(culture));
    }

    // ------------------------------------------------------------ custom habits and goals

    [Given(@"^(`[^`]+`) deleted their custom habit `([^`]+)`$")]
    public async Task GivenDeletedTheirCustomHabit(string owner, string habit)
    {
        arrange.AtStepTime(null);
        var email = F004Text.Email(owner);
        var deleted = await driver.DeleteCustomHabitAsync(context.Account(email), context.CustomHabit(email, habit).Id);
        Assert.Equal(CustomHabitDeleteOutcome.Deleted, deleted.Outcome);
    }

    [Given(@"^(`[^`]+`) started a new goal from (`[^`]+`|their custom habit `[^`]+`)$")]
    public async Task GivenStartedANewGoal(string owner, string habit)
    {
        arrange.AtStepTime(null);
        await arrange.StartedAsync(F004Text.Email(owner), habit);
    }

    [Given(@"^at `([^`]+)` \([^)]*\) (`[^`]+`) started a new goal from their custom habit `([^`]+)`, and the starting values had the start date (\d{4}-\d{2}-\d{2})$")]
    public async Task GivenAtTimeStartedANewGoal(string time, string owner, string habit, string startDate)
    {
        arrange.AtStepTime(time);
        var values = await arrange.StartedAsync(F004Text.Email(owner), $"their custom habit `{habit}`");
        Assert.Equal(F004Text.Date(startDate), values.StartDate);
    }

    /// <summary>The "at `T` " prefix group always takes part in the match; an empty capture means the step gives no time.</summary>
    [Given("^" + F004Text.AtPrefix + "(`[^`]+`) created a goal on (`[^`]+`|their custom habit `[^`]+`) with (.+)$")]
    public async Task GivenCreatedAGoal(string at, string owner, string habit, string settings)
    {
        arrange.AtStepTime(F004Text.AtTime(at));
        var email = F004Text.Email(owner);
        foreach (var (habitPhrase, spec) in F004Arrange.GoalChain(habit, settings))
        {
            await arrange.CreatedGoalAsync(email, habitPhrase, spec);
        }
    }

    [Given(@"^(`[^`]+`) created two goals on their custom habit `([^`]+)` with (.+)$")]
    public async Task GivenCreatedTwoGoals(string owner, string habit, string settings)
    {
        arrange.AtStepTime(null);
        var email = F004Text.Email(owner);
        await arrange.CreatedGoalAsync(email, $"their custom habit `{habit}`", F004Text.Settings(settings));
        await arrange.CreatedGoalAsync(email, $"their custom habit `{habit}`", F004Text.Settings(settings));
    }

    [Given(@"^(`[^`]+`) created goals on their custom habit `([^`]+)` with (.+?), named (.+), in this order$")]
    public async Task GivenCreatedNamedGoalsAtTimes(string owner, string habit, string settings, string names)
    {
        var email = F004Text.Email(owner);
        var items = Regex.Matches(names, "\"([^\"]*)\" at `([^`]+)`");
        Assert.NotEmpty(items);
        foreach (Match item in items)
        {
            arrange.AtStepTime(item.Groups[2].Value);
            var spec = F004Text.Settings(settings);
            spec.SetName(item.Groups[1].Value);
            await arrange.CreatedGoalAsync(email, $"their custom habit `{habit}`", spec);
        }
    }

    [Given(@"^at `([^`]+)` (`[^`]+`) listed their goals, read G, read the evaluation calendar of G from (\d{4}-\d{2}-\d{2}) to (\d{4}-\d{2}-\d{2}), started a new goal from (`[^`]+`) and created a goal on (`[^`]+`) with (.+)$")]
    public async Task GivenUsedEveryGoalUseCase(string time, string owner, string first, string last, string startHabit, string createHabit, string settings)
    {
        arrange.AtStepTime(time);
        var email = F004Text.Email(owner);
        var user = context.Account(email);
        var goal = context.GoalNamed("G");

        var list = await driver.ListGoalsAsync(user);
        Assert.Equal(GoalListOutcome.Found, list.Outcome);
        var read = await driver.GetGoalAsync(user, goal.Id);
        Assert.Equal(GoalReadOutcome.Found, read.Outcome);
        var calendar = await driver.GetCalendarAsync(user, goal.Id, F004Text.Date(first), F004Text.Date(last));
        Assert.Equal(GoalCalendarOutcome.Found, calendar.Outcome);
        await arrange.StartedAsync(email, startHabit);
        await arrange.CreatedGoalAsync(email, createHabit, F004Text.Settings(settings));
    }

    // ------------------------------------------------------------ time zone changes

    [Given(@"^at `([^`]+)`(?: \([^)]*\))? (`[^`]+`) changed their time zone to `([^`]+)`(?:, and the change succeeded|, whose 19 November (?:began|ended) at `[^`]+`)?$")]
    public async Task GivenChangedTheirTimeZone(string time, string owner, string timeZone)
    {
        arrange.AtStepTime(time);
        var email = F004Text.Email(owner);
        var changed = await driver.ChangeTimeZoneAsync(context.Account(email), timeZone);
        Assert.Equal(TimeZoneChangeOutcome.Changed, changed.Outcome);
        context.TimeZones[email] = timeZone;
    }

    // ------------------------------------------------------------ demo data

    [Given(@"^the AC database is reset, so no account and no habit exists$")]
    public async Task GivenTheAcDatabaseIsReset()
    {
        await F004Driver.ResetDatabaseAsync();
    }

    [Given(@"^demo loading ran$")]
    public async Task GivenDemoLoadingRan()
    {
        await LoadDemoDataAsync();
    }

    [Given(@"^demo loading ran and its catalog result was ""([^""]*)""$")]
    public async Task GivenDemoLoadingRanWithTheCatalogResult(string catalogResult)
    {
        var loaded = await LoadDemoDataAsync();
        Assert.NotNull(loaded.Catalog);
        Assert.Equal(catalogResult, loaded.Catalog.Message);
    }

    private async Task<DemoLoadingResult> LoadDemoDataAsync()
    {
        arrange.AtStepTime(null);
        var loaded = await driver.LoadDemoDataAsync(F004Text.DemoPassword);
        Assert.Equal(DemoLoadingOutcome.Loaded, loaded.Outcome);
        context.LastDemoLoading = loaded;
        return loaded;
    }

    // ------------------------------------------------------------ helpers

    private HabitId? CatalogIdOf(string name) =>
        context.CatalogHabits.FirstOrDefault(habit => habit.Names.Contains(name, StringComparer.Ordinal))?.Id;
}
