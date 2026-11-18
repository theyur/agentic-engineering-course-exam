using System.Text.RegularExpressions;
using HT.Application.Accounts;
using HT.Application.Goals;
using HT.Application.Habits;
using HT.Application.Operator;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using NodaTime;

namespace HT.AcceptanceTests.Support.F004;

/// <summary>
/// Arranges what the F004 steps name, as acceptance.md's conventions define it, and keeps the supplied current
/// time: setup steps that give no time run at <see cref="F004Driver.DefaultNow"/>; any other step that gives no
/// time runs at that time or at the latest time of the steps before it, whichever is later.
/// </summary>
public sealed class F004Arrange(F004Context context, F004Driver driver)
{
    // ------------------------------------------------------------ time

    /// <summary>Account and habit setup in a Given step that gives no time.</summary>
    public void AtSetupTime() => driver.SetNow(F004Driver.DefaultNow);

    /// <summary>A Given or When step: at the time it gives, or at the default or latest time before it.</summary>
    public void AtStepTime(string? time)
    {
        if (!string.IsNullOrWhiteSpace(time))
        {
            var instant = F004Text.Instant(time);
            if (instant > context.LatestTime)
            {
                context.LatestTime = instant;
            }

            driver.SetNow(instant);
            return;
        }

        driver.SetNow(context.LatestTime > F004Driver.DefaultNow ? context.LatestTime : F004Driver.DefaultNow);
    }

    /// <summary>
    /// The zone a test input ("the start date today in the acting user's time zone") is computed in: the zone a step
    /// gave when it is "UTC" or a tzdb identifier with "/" outside "Etc/" (FR-024), otherwise the default UTC.
    /// </summary>
    public static string ZoneForToday(string? timeZone) =>
        timeZone is not null
        && (timeZone == "UTC" || (timeZone.Contains('/') && !timeZone.StartsWith("Etc/", StringComparison.Ordinal)))
        && DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZone) is not null
            ? timeZone
            : "UTC";

    /// <summary>"today" in an account's time zone at the supplied current time (computed with NodaTime's tzdb).</summary>
    public DateOnly TodayFor(string? email)
    {
        var zoneId = email is not null && context.TimeZones.TryGetValue(email, out var known) ? known : "UTC";
        var zone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(zoneId) ?? DateTimeZone.Utc;
        var date = Instant.FromDateTimeOffset(driver.Now).InZone(zone).Date;
        return new DateOnly(date.Year, date.Month, date.Day);
    }

    // ------------------------------------------------------------ accounts

    public async Task<ActingUser> AdministratorAsync(string email)
    {
        AtSetupTime();
        var created = await driver.CreateAdministratorAsync(email, F004Text.AdministratorPassword);
        Assert.Equal(AdministratorCreationOutcome.Created, created.Outcome);
        var user = await SignInAsync(email, F004Text.AdministratorPassword);
        Assert.Equal(Role.Administrator, user.Role);
        context.CurrentAdministratorEmail = email;
        context.TimeZones[email] = "UTC";
        return user;
    }

    /// <summary>"the registered user `x`" (optionally "in `Z`"): registration with walking42, then sign-in.</summary>
    public async Task<ActingUser> RegisteredUserAsync(string email, string? timeZone)
    {
        AtSetupTime();
        var registered = await driver.RegisterAsync(email, F004Text.RegisteredUserPassword, timeZone, null);
        Assert.Equal(RegistrationOutcome.Registered, registered.Outcome);
        var user = await SignInAsync(email, F004Text.RegisteredUserPassword);
        Assert.Equal(Role.RegisteredUser, user.Role);
        context.CurrentRegisteredUserEmail = email;
        context.TimeZones[email] = ZoneForToday(timeZone);
        return user;
    }

    public async Task<ActingUser> SignInAsync(string email, string password)
    {
        var signIn = await driver.SignInAsync(email, password);
        Assert.Equal(SignInOutcome.SignedIn, signIn.Outcome);
        var user = signIn.ActingUser ?? throw new InvalidOperationException($"Signing in `{email}` gave no acting user.");
        context.Accounts[email] = user;
        return user;
    }

    /// <summary>A demo account, signed in with the demo password.</summary>
    public async Task<ActingUser> DemoAccountAsync(string email)
    {
        if (context.Accounts.TryGetValue(email, out var known))
        {
            return known;
        }

        var user = await SignInAsync(email, F004Text.DemoPassword);
        context.TimeZones[email] = "UTC";
        return user;
    }

    // ------------------------------------------------------------ habits

    /// <summary>The catalog habits of a list such as "a Published `N` with the category Body and a Draft `M`".</summary>
    public async Task CatalogHabitsAsync(string habitList)
    {
        foreach (var (state, name, clause) in F004Text.HabitList(habitList))
        {
            var spec = new CatalogSpec(name);
            F004Text.ApplyCatalogClause(spec, clause);
            await CatalogHabitAsync(state, spec);
        }
    }

    public async Task<CatalogRecord> CatalogHabitAsync(CatalogHabitState state, CatalogSpec spec)
    {
        AtSetupTime();
        var administrator = context.Administrator;
        var created = await driver.CreateCatalogHabitAsync(administrator, spec.ToInput());
        Assert.Equal(CatalogHabitCreateOutcome.Created, created.Outcome);
        var id = created.HabitId ?? throw new InvalidOperationException($"Creating `{spec.Name}` gave no identifier.");

        if (state is CatalogHabitState.Published or CatalogHabitState.Unpublished)
        {
            var published = await driver.PublishAsync(administrator, id);
            Assert.Equal(CatalogHabitPublishOutcome.Published, published.Outcome);
        }

        if (state is CatalogHabitState.Unpublished)
        {
            var unpublished = await driver.UnpublishAsync(administrator, id, confirmed: false);
            Assert.Equal(CatalogHabitUnpublishOutcome.Unpublished, unpublished.Outcome);
        }

        var record = new CatalogRecord(id, spec);
        context.CatalogHabits.Add(record);
        return record;
    }

    public async Task<CustomRecord> CustomHabitAsync(string ownerEmail, string name, Intent? intent = null)
    {
        AtSetupTime();
        var spec = new CustomSpec(name);
        if (intent is { } given)
        {
            spec.Intent = given;
        }

        var created = await driver.CreateCustomHabitAsync(context.Account(ownerEmail), spec.ToInput());
        Assert.Equal(CustomHabitCreateOutcome.Created, created.Outcome);
        var id = created.HabitId ?? throw new InvalidOperationException($"Creating `{name}` gave no identifier.");
        var record = new CustomRecord(ownerEmail, id, spec);
        context.CustomHabits.Add(record);
        return record;
    }

    /// <summary>
    /// The habit a phrase names, as the acting user sees it: "`N`" (a catalog habit), "their custom habit `C`",
    /// "`x`'s custom habit `C`", "the identifier of their custom habit `C`", "the identifier of `N`", or "an
    /// identifier that the test creates and that belongs to no habit". The kind is the phrase's own kind.
    /// </summary>
    public GoalHabit Habit(string phrase, string? actorEmail)
    {
        var text = phrase.Trim();
        Match match;
        if (text == "an identifier that the test creates and that belongs to no habit")
        {
            return new GoalHabit(new HabitId(Guid.NewGuid()), HabitKind.Catalog);
        }

        if ((match = Regex.Match(text, "^(?:the identifier of )?their custom habit `([^`]+)`$")).Success)
        {
            var owner = actorEmail ?? throw new InvalidOperationException("The anonymous visitor has no custom habits.");
            return new GoalHabit(context.CustomHabit(owner, match.Groups[1].Value).Id, HabitKind.Custom);
        }

        if ((match = Regex.Match(text, "^`([^`]+)`'s custom habit `([^`]+)`$")).Success)
        {
            return new GoalHabit(context.CustomHabit(match.Groups[1].Value, match.Groups[2].Value).Id, HabitKind.Custom);
        }

        if ((match = Regex.Match(text, "^(?:the identifier of )?`([^`]+)`$")).Success)
        {
            return new GoalHabit(context.CatalogHabit(match.Groups[1].Value).Id, HabitKind.Catalog);
        }

        throw new InvalidOperationException($"\"{phrase}\" names no habit.");
    }

    // ------------------------------------------------------------ goals

    /// <summary>
    /// Goal creation with the settings of a phrase; the start date is today in the acting user's time zone and
    /// there is no name, unless the phrase gives them (acceptance.md, "Goal use cases").
    /// </summary>
    public async Task<GoalCreateResult> CreateGoalAsync(ActingUser actor, string? actorEmail, GoalHabit? habit, SettingsSpec spec, string? alias = null)
    {
        var input = new GoalInput
        {
            Habit = habit,
            Target = spec.ToFields(),
            StartDate = spec.StartDateGiven ? spec.StartDate : TodayFor(actorEmail),
            Name = spec.NameGiven ? spec.Name : null,
        };
        context.LastSentGoalName = input.Name;
        var result = await driver.CreateGoalAsync(actor, input);
        if (result.Outcome == GoalCreateOutcome.Created && result.GoalId is { } id && actorEmail is not null)
        {
            context.Goals.Add(new GoalRecord(actorEmail, id, alias));
        }

        return result;
    }

    /// <summary>Goal creation from starting values, unchanged.</summary>
    public async Task<GoalCreateResult> CreateFromStartingValuesAsync(ActingUser actor, string actorEmail, GoalStartingValues values)
    {
        var input = new GoalInput
        {
            Habit = values.Habit is null ? null : new GoalHabit(values.Habit.Id, values.Habit.Kind),
            Target = new GoalTargetFields
            {
                BuildOrReduce = values.BuildOrReduce,
                Measurement = values.Measurement,
                Amount = values.Amount,
                CountUnit = values.CountUnit,
                DurationUnit = values.DurationUnit,
                ScheduleKind = values.ScheduleKind,
                Days = values.Days,
                Times = values.Times,
                Period = values.Period,
            },
            StartDate = values.StartDate,
            Name = values.Name,
        };
        context.LastSentGoalName = input.Name;
        var result = await driver.CreateGoalAsync(actor, input);
        if (result.Outcome == GoalCreateOutcome.Created && result.GoalId is { } id)
        {
            context.Goals.Add(new GoalRecord(actorEmail, id, null));
        }

        return result;
    }

    /// <summary>A goal a Given step says was created: the creation must succeed.</summary>
    public async Task<GoalId> CreatedGoalAsync(string ownerEmail, string habitPhrase, SettingsSpec spec, string? alias = null)
    {
        var result = await CreateGoalAsync(context.Account(ownerEmail), ownerEmail, Habit(habitPhrase, ownerEmail), spec, alias);
        Assert.Equal(GoalCreateOutcome.Created, result.Outcome);
        return result.GoalId ?? throw new InvalidOperationException("Creating the goal gave no identifier.");
    }

    /// <summary>
    /// A chain such as "S1, then a goal on H2 with S2", "S1, and then a goal on H2 with S2" or "S1, and a second
    /// one with the same settings and the name "N"", read as (habit, settings) pairs in order.
    /// </summary>
    public static List<(string HabitPhrase, SettingsSpec Settings)> GoalChain(string firstHabit, string rest)
    {
        var chain = new List<(string, SettingsSpec)>();
        var second = Regex.Match(rest, "^(.+), and a second one with the same settings and the name \"([^\"]*)\"$");
        if (second.Success)
        {
            chain.Add((firstHabit, F004Text.Settings(second.Groups[1].Value)));
            var named = F004Text.Settings(second.Groups[1].Value);
            named.SetName(second.Groups[2].Value);
            chain.Add((firstHabit, named));
            return chain;
        }

        var link = new Regex(", (?:and )?then a goal on (`[^`]+`|their custom habit `[^`]+`) with ");
        var habit = firstHabit;
        var text = rest;
        while (true)
        {
            var next = link.Match(text);
            if (!next.Success)
            {
                chain.Add((habit, F004Text.Settings(text)));
                return chain;
            }

            chain.Add((habit, F004Text.Settings(text[..next.Index])));
            habit = next.Groups[1].Value;
            text = text[(next.Index + next.Length)..];
        }
    }

    public async Task<GoalStartingValues> StartedAsync(string email, string habitPhrase, int? position = null)
    {
        var started = await driver.StartNewGoalAsync(context.Account(email), Habit(habitPhrase, email), position);
        Assert.Equal(GoalStartOutcome.Started, started.Outcome);
        var values = started.StartingValues ?? throw new InvalidOperationException("Starting a new goal gave no starting values.");
        context.StartingValues[email] = values;
        return values;
    }

    /// <summary>Reads a goal as its owner at the supplied current time; the read must succeed.</summary>
    public async Task<GoalDetails> ReadGoalAsync(GoalRecord goal)
    {
        var read = await driver.GetGoalAsync(context.Account(goal.OwnerEmail), goal.Id);
        Assert.Equal(GoalReadOutcome.Found, read.Outcome);
        return read.Goal ?? throw new InvalidOperationException("Reading the goal gave no goal.");
    }

    /// <summary>Lists an account's goals at the supplied current time; the list must be found.</summary>
    public async Task<IReadOnlyList<GoalListItem>> ListGoalsAsync(string email)
    {
        var list = await driver.ListGoalsAsync(context.Account(email));
        Assert.Equal(GoalListOutcome.Found, list.Outcome);
        return list.Goals;
    }

    /// <summary>A goal reference such as "the goal", "G" or "`x`'s goal".</summary>
    public GoalId GoalReference(string phrase, string? actorEmail)
    {
        var text = phrase.Trim();
        if (text.StartsWith("the goal ", StringComparison.Ordinal))
        {
            text = text["the goal ".Length..];
        }

        if (text is "an identifier that the test creates and that belongs to no goal" or "an identifier that belongs to no goal")
        {
            return new GoalId(Guid.NewGuid());
        }

        if (text == "the goal")
        {
            return actorEmail is not null && context.Goals.Any(goal => goal.OwnerEmail == actorEmail)
                ? context.GoalOf(actorEmail).Id
                : context.CurrentGoal.Id;
        }

        var owner = Regex.Match(text, "^`([^`]+)`'s goal$");
        if (owner.Success)
        {
            return context.GoalOf(owner.Groups[1].Value).Id;
        }

        return context.GoalNamed(text).Id;
    }
}
