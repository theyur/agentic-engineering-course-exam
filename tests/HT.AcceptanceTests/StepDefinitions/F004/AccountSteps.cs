using System.Globalization;
using HT.AcceptanceTests.Support.F004;
using HT.Application.Accounts;
using HT.Domain.Accounts;

namespace HT.AcceptanceTests.StepDefinitions.F004;

/// <summary>The F004 steps on account data: time zone (FR-024 to FR-026), name, profile and Home heading (FR-028 to FR-030).</summary>
[Binding]
[Scope(Tag = "F004")]
public sealed class AccountSteps(F004Context context, F004Driver driver, F004Arrange arrange)
{
    private const string Actor = "(`[^`]+`|the anonymous visitor)";
    /// <summary>The optional "at `T` (…) " prefix; its group always takes part in the match, empty when no time is given.</summary>
    private const string At = F004Text.AtPrefix;

    // ------------------------------------------------------------ time zone

    [When(@"^(`[^`]+`) reads their time zone$")]
    public async Task WhenReadsTheirTimeZone(string owner)
    {
        arrange.AtStepTime(null);
        context.Record(await driver.GetTimeZoneAsync(context.Actor(owner)));
    }

    [When(@"^(`[^`]+`) changes their time zone to `([^`]*)`$")]
    public async Task WhenChangesTheirTimeZone(string owner, string value)
    {
        arrange.AtStepTime(null);
        await ChangeTimeZoneAsync(F004Text.Email(owner), F004Text.TextOf(value));
    }

    [When(@"^at `([^`]+)` (`[^`]+`) changes their time zone to `([^`]+)`, starts a new goal from their custom habit `([^`]+)`, and creates a goal on it with (.+)$")]
    public async Task WhenChangesTheTimeZoneStartsAndCreates(string time, string owner, string value, string habit, string settings)
    {
        arrange.AtStepTime(time);
        var email = F004Text.Email(owner);
        await ChangeTimeZoneAsync(email, value);
        var goalHabit = arrange.Habit($"their custom habit `{habit}`", email);
        var started = await driver.StartNewGoalAsync(context.Account(email), goalHabit, null);
        context.Record(started);
        context.Record(await arrange.CreateGoalAsync(context.Account(email), email, goalHabit, F004Text.Settings(settings)));
    }

    [When(@"^(`[^`]+`) reads their time zone, changes it to `([^`]+)` and reads it again, and the anonymous visitor reads a time zone and changes a time zone to `([^`]+)`$")]
    public async Task WhenTheAdministratorAndTheVisitorUseTheTimeZone(string owner, string value, string visitorValue)
    {
        arrange.AtStepTime(null);
        var user = context.Actor(owner);
        context.Record(await driver.GetTimeZoneAsync(user));
        context.Record(await driver.ChangeTimeZoneAsync(user, value));
        context.Record(await driver.GetTimeZoneAsync(user));
        context.Record(await driver.GetTimeZoneAsync(ActingUser.Anonymous));
        context.Record(await driver.ChangeTimeZoneAsync(ActingUser.Anonymous, visitorValue));
    }

    [When(@"^(`[^`]+`) and (`[^`]+`) each read their time zone$")]
    public async Task WhenEachReadTheirTimeZone(string first, string second)
    {
        arrange.AtStepTime(null);
        context.Record(await driver.GetTimeZoneAsync(context.Actor(first)));
        context.Record(await driver.GetTimeZoneAsync(context.Actor(second)));
    }

    [Then(@"^reading their time zone gives `([^`]+)`$")]
    public async Task ThenReadingTheirTimeZoneGives(string value)
    {
        var read = await driver.GetTimeZoneAsync(context.Account(CurrentUser()));
        Assert.Equal(TimeZoneReadOutcome.Found, read.Outcome);
        Assert.Equal(value, read.TimeZone);
    }

    [Then(@"^the change succeeds and reading their time zone gives `([^`]+)`$")]
    public async Task ThenTheChangeSucceedsAndReadingGives(string value)
    {
        var change = Assert.Single(context.TimeZoneChanges);
        Assert.True(
            change.Outcome is TimeZoneChangeOutcome.Changed or TimeZoneChangeOutcome.Unchanged,
            $"The change gave {change.Outcome}: {change.Message}");
        await ThenReadingTheirTimeZoneGives(value);
    }

    [Then(@"^registration succeeded and the time zone is `([^`]+)`$")]
    public void ThenRegistrationSucceededAndTheTimeZoneIs(string value)
    {
        Assert.Equal(RegistrationOutcome.Registered, Assert.Single(context.Registrations).Outcome);
        var read = Assert.Single(context.TimeZoneReads);
        Assert.Equal(TimeZoneReadOutcome.Found, read.Outcome);
        Assert.Equal(value, read.TimeZone);
    }

    [Then(@"^the administrator's first read gives `([^`]+)`, the change succeeds and the second read gives `([^`]+)`$")]
    public void ThenTheAdministratorsReadsAndChange(string first, string second)
    {
        Assert.Equal(3, context.TimeZoneReads.Count);
        Assert.Equal(2, context.TimeZoneChanges.Count);
        Assert.Equal(TimeZoneReadOutcome.Found, context.TimeZoneReads[0].Outcome);
        Assert.Equal(first, context.TimeZoneReads[0].TimeZone);
        Assert.Equal(TimeZoneChangeOutcome.Changed, context.TimeZoneChanges[0].Outcome);
        Assert.Equal(TimeZoneReadOutcome.Found, context.TimeZoneReads[1].Outcome);
        Assert.Equal(second, context.TimeZoneReads[1].TimeZone);
    }

    [Then(@"^both of the anonymous visitor's calls give ""([^""]*)""$")]
    public void ThenBothOfTheVisitorsCallsGive(string message)
    {
        var read = context.TimeZoneReads[^1];
        var change = context.TimeZoneChanges[^1];
        Assert.Equal(TimeZoneReadOutcome.NotSignedIn, read.Outcome);
        Assert.Equal(message, read.Message);
        Assert.Null(read.TimeZone);
        Assert.Equal(TimeZoneChangeOutcome.NotSignedIn, change.Outcome);
        Assert.Equal(message, change.Message);
    }

    [Then(@"^each gets `([^`]+)`, their own, and neither result contains `([^`]+)`$")]
    public void ThenEachGetsTheirOwn(string value, string other)
    {
        Assert.Equal(2, context.TimeZoneReads.Count);
        foreach (var read in context.TimeZoneReads)
        {
            Assert.Equal(TimeZoneReadOutcome.Found, read.Outcome);
            Assert.Equal(value, read.TimeZone);
            Assert.DoesNotContain(other, read.TimeZone ?? string.Empty, StringComparison.Ordinal);
            Assert.DoesNotContain(other, read.Message ?? string.Empty, StringComparison.Ordinal);
        }
    }

    // ------------------------------------------------------------ registration

    [When(@"^the anonymous visitor registers the email `([^`]+)` with the password `([^`]+)` and a name of (\d+) characters$")]
    public async Task WhenTheVisitorRegisters(string email, string password, int length)
    {
        arrange.AtStepTime(null);
        context.Record(await driver.RegisterAsync(email, password, null, F004Text.TextOfLength(length)));
    }

    [When(@"^the anonymous visitor registers the email `([^`]+)` with the password `([^`]+)` and a name of (\d+) characters, then registers the same email with the password `([^`]+)` and a name of (\d+) characters$")]
    public async Task WhenTheVisitorRegistersTwice(string email, string password, int length, string secondPassword, int secondLength)
    {
        await WhenTheVisitorRegisters(email, password, length);
        context.Record(await driver.RegisterAsync(email, secondPassword, null, F004Text.TextOfLength(secondLength)));
    }

    [Then(@"^the first registration gives ""([^""]*)"" and the second registration succeeds$")]
    public void ThenTheFirstRegistrationGives(string message)
    {
        Assert.Equal(2, context.Registrations.Count);
        Assert.Equal(RegistrationOutcome.NameTooLong, context.Registrations[0].Outcome);
        Assert.Equal(message, context.Registrations[0].Message);
        Assert.Equal(RegistrationOutcome.Registered, context.Registrations[1].Outcome);
    }

    [Then(@"^signed in, `([^`]+)`'s profile gives the text of (\d+) characters as the name$")]
    public async Task ThenSignedInTheProfileGivesTheText(string email, int length)
    {
        var user = await arrange.SignInAsync(email, F004Text.RegisteredUserPassword);
        var profile = await driver.GetProfileAsync(user);
        Assert.Equal(ProfileReadOutcome.Found, profile.Outcome);
        Assert.NotNull(profile.Profile);
        Assert.Equal(F004Text.TextOfLength(length), profile.Profile.Name);
    }

    [Then(@"^registration does not succeed, and its result is not ""([^""]*)""$")]
    public void ThenRegistrationDoesNotSucceed(string message)
    {
        var registration = Assert.Single(context.Registrations);
        Assert.NotEqual(RegistrationOutcome.Registered, registration.Outcome);
        Assert.Null(registration.AccountId);
        Assert.NotEqual(message, registration.Message);
    }

    [Then(@"^registering `([^`]+)` afterwards with the password `([^`]+)` and no name succeeds$")]
    public async Task ThenRegisteringAfterwardsSucceeds(string email, string password)
    {
        var registration = await driver.RegisterAsync(email, password, null, null);
        Assert.Equal(RegistrationOutcome.Registered, registration.Outcome);
    }

    // ------------------------------------------------------------ name and profile

    [When(@"^the anonymous visitor changes their name to ""([^""]*)""$")]
    public async Task WhenTheVisitorChangesTheirName(string name)
    {
        arrange.AtStepTime(null);
        context.Record(await driver.ChangeNameAsync(ActingUser.Anonymous, name));
    }

    [When("^" + Actor + " reads their profile$")]
    public async Task WhenReadsTheirProfile(string actor)
    {
        arrange.AtStepTime(null);
        context.Record(await driver.GetProfileAsync(context.Actor(actor)));
    }

    [When(@"^(`[^`]+`) changes their name to a text of (\d+) characters, then reads their profile, then changes their name to a text of (\d+) characters and reads their profile again$")]
    public async Task WhenChangesTheirNameTwice(string owner, int first, int second)
    {
        arrange.AtStepTime(null);
        var user = context.Actor(owner);
        context.Record(await driver.ChangeNameAsync(user, F004Text.TextOfLength(first)));
        context.Record(await driver.GetProfileAsync(user));
        context.Record(await driver.ChangeNameAsync(user, F004Text.TextOfLength(second)));
        context.Record(await driver.GetProfileAsync(user));
    }

    [When(@"^(`[^`]+`) reads their profile, and (`[^`]+`) changes their name to ""([^""]*)"", reads their profile and reads the Home heading$")]
    public async Task WhenOneReadsAndTheOtherChangesTheirName(string reader, string owner, string name)
    {
        arrange.AtStepTime(null);
        context.Record(await driver.GetProfileAsync(context.Actor(reader)));
        var user = context.Actor(owner);
        context.Record(await driver.ChangeNameAsync(user, name));
        context.Record(await driver.GetProfileAsync(user));
        context.Record(await HomeHeadingAsync(user));
    }

    [Then(@"^registration succeeded and the profile gives (the name ""[^""]*""|no name) and the email `([^`]+)`$")]
    public void ThenRegistrationSucceededAndTheProfileGives(string name, string email)
    {
        Assert.Equal(RegistrationOutcome.Registered, Assert.Single(context.Registrations).Outcome);
        var profile = Profile(Assert.Single(context.ProfileReads));
        AssertName(name, profile);
        Assert.Equal(email, profile.Email);
    }

    [Then(@"^the profile gives ""([^""]*)""$")]
    public void ThenTheProfileGivesMemberSince(string memberSince)
    {
        var profile = Profile(Assert.Single(context.ProfileReads));
        Assert.Equal(memberSince, profile.MemberSince);
    }

    [Then(@"^(`[^`]+`)'s first change gives ""([^""]*)"", the first profile read gives ""([^""]*)"", the second change succeeds and the second profile read gives the text of (\d+) characters$")]
    public void ThenTheChangesAndReadsGive(string owner, string message, string firstName, int length)
    {
        Assert.Equal(2, context.NameChanges.Count);
        Assert.Equal(2, context.ProfileReads.Count);
        Assert.Equal(NameChangeOutcome.NameTooLong, context.NameChanges[0].Outcome);
        Assert.Equal(message, context.NameChanges[0].Message);
        Assert.Equal(firstName, Profile(context.ProfileReads[0]).Name);
        Assert.Equal(NameChangeOutcome.Changed, context.NameChanges[1].Outcome);
        Assert.Equal(F004Text.TextOfLength(length), Profile(context.ProfileReads[1]).Name);
        Assert.Equal(F004Text.Email(owner), Profile(context.ProfileReads[1]).Email);
    }

    [Then(@"^(`[^`]+`)'s profile gives no name, the email `([^`]+)`, and does not contain ""([^""]*)""$")]
    public void ThenTheProfileGivesNoNameAndDoesNotContain(string owner, string email, string text)
    {
        var profile = Profile(context.ProfileReads[0]);
        Assert.Equal(F004Text.Email(owner), email);
        Assert.True(string.IsNullOrEmpty(profile.Name), $"The profile gives the name \"{profile.Name}\".");
        Assert.Equal(email, profile.Email);
        Assert.DoesNotContain(text, profile.Email, StringComparison.Ordinal);
        Assert.DoesNotContain(text, profile.MemberSince ?? string.Empty, StringComparison.Ordinal);
    }

    [Then(@"^the administrator's change succeeds, their profile gives the name ""([^""]*)"" and the email `([^`]+)`, and the Home heading gives the greeting ""([^""]*)"" \(10:00 in `UTC`\)$")]
    public void ThenTheAdministratorsChangeSucceeds(string name, string email, string greeting)
    {
        Assert.Equal(NameChangeOutcome.Changed, Assert.Single(context.NameChanges).Outcome);
        var profile = Profile(context.ProfileReads[^1]);
        Assert.Equal(name, profile.Name);
        Assert.Equal(email, profile.Email);
        Assert.Equal(greeting, Heading().Greeting);
    }

    // ------------------------------------------------------------ Home heading

    [When("^" + At + Actor + " reads the Home heading$")]
    public async Task WhenReadsTheHomeHeading(string at, string actor)
    {
        arrange.AtStepTime(F004Text.AtTime(at));
        context.Record(await HomeHeadingAsync(context.Actor(actor)));
    }

    [Then(@"^the greeting is ""([^""]*)"" and the date is ""([^""]*)""$")]
    public void ThenTheGreetingAndTheDate(string greeting, string date)
    {
        var heading = Heading();
        Assert.Equal(greeting, heading.Greeting);
        Assert.Equal(date, heading.Date);
    }

    [Then(@"^the date is ""([^""]*)"" and the greeting is ""([^""]*)""$")]
    public void ThenTheDateAndTheGreeting(string date, string greeting)
    {
        ThenTheGreetingAndTheDate(greeting, date);
    }

    // ------------------------------------------------------------ helpers

    private async Task ChangeTimeZoneAsync(string email, string value)
    {
        var changed = await driver.ChangeTimeZoneAsync(context.Account(email), value);
        context.Record(changed);
        if (changed.Outcome == TimeZoneChangeOutcome.Changed)
        {
            context.TimeZones[email] = F004Arrange.ZoneForToday(value);
        }
    }

    /// <summary>The Home heading, read with the culture the item set for the test, if any (AC-088).</summary>
    private async Task<HomeHeadingResult> HomeHeadingAsync(ActingUser user)
    {
        var previous = CultureInfo.CurrentCulture;
        var previousUi = CultureInfo.CurrentUICulture;
        if (context.Culture is { } culture)
        {
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }

        try
        {
            return await driver.GetHomeHeadingAsync(user);
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
            CultureInfo.CurrentUICulture = previousUi;
        }
    }

    private string CurrentUser() =>
        context.CurrentRegisteredUserEmail ?? throw new InvalidOperationException("No registered user was given in this scenario.");

    private HomeHeading Heading()
    {
        var result = context.LastHomeHeading ?? throw new InvalidOperationException("No Home heading was read in the When step.");
        Assert.Equal(HomeHeadingOutcome.Found, result.Outcome);
        return result.Heading ?? throw new InvalidOperationException("Reading the Home heading gave no heading.");
    }

    private static ProfileView Profile(ProfileReadResult result)
    {
        Assert.Equal(ProfileReadOutcome.Found, result.Outcome);
        return result.Profile ?? throw new InvalidOperationException("Reading the profile gave no profile.");
    }

    private static void AssertName(string phrase, ProfileView profile)
    {
        if (phrase == "no name")
        {
            Assert.True(string.IsNullOrEmpty(profile.Name), $"The profile gives the name \"{profile.Name}\".");
        }
        else
        {
            Assert.Equal(F004Text.QuotedTexts(phrase).Single(), profile.Name);
        }
    }
}
