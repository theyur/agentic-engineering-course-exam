using System.Text.RegularExpressions;
using HT.Application.Accounts;
using HT.Application.Goals;
using HT.Application.Habits;
using HT.Application.Operator;
using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;

namespace HT.AcceptanceTests.Support.F005;

/// <summary>
/// Arranges what the F005 steps name, as acceptance.md's conventions define it, and keeps the supplied current
/// time. Account and habit setup that gives no time runs at <see cref="F005Driver.DefaultNow"/>, or at the earliest
/// time the item gives when that is earlier: such setup written before the item's first other step is held until
/// that step, whose time (the earliest the item gives in every F005 item) then fixes the setup time.
/// </summary>
public sealed class F005Arrange(F005Context context, F005Driver driver, ScenarioContext scenario)
{
    // ------------------------------------------------------------ Examples cells

    private const string RowPlaceholder = "<n>";

    private const string RowColumn = "n";

    /// <summary>
    /// acceptance.md, Conventions, "Examples": every email of such an item contains <c>&lt;n&gt;</c>, the row's value
    /// in the column <c>n</c>. Reqnroll fills placeholders in step text but not inside an Examples cell, so an actor or
    /// goal phrase that comes from a cell still holds the literal <c>&lt;n&gt;</c>; it is replaced here by the row's
    /// <c>n</c> before any account or goal is looked up.
    /// </summary>
    public string WithRow(string text)
    {
        if (!text.Contains(RowPlaceholder, StringComparison.Ordinal))
        {
            return text;
        }

        var arguments = scenario.ScenarioInfo.Arguments;
        if (arguments is null || !arguments.Contains(RowColumn) || arguments[RowColumn] is not { } value)
        {
            throw new InvalidOperationException($"\"{text}\" holds {RowPlaceholder}, but the scenario has no Examples column `{RowColumn}`.");
        }

        return text.Replace(RowPlaceholder, Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }

    // ------------------------------------------------------------ time

    /// <summary>A Given or When step other than account and habit setup: at its time, or the default or latest time.</summary>
    public async Task BeginStepAsync(string? time)
    {
        DateTimeOffset? instant = string.IsNullOrWhiteSpace(time) ? null : F005Text.Instant(time);
        await ResolveSetupAsync(instant);
        if (instant is { } given)
        {
            if (given > context.LatestTime)
            {
                context.LatestTime = given;
            }

            driver.SetNow(given);
            return;
        }

        driver.SetNow(context.LatestTime > F005Driver.DefaultNow ? context.LatestTime : F005Driver.DefaultNow);
    }

    /// <summary>The item's When step: as <see cref="BeginStepAsync"/>; the Then steps after it run at its time.</summary>
    public async Task BeginWhenAsync(string? time)
    {
        await BeginStepAsync(time);
        context.WhenTime = driver.Now;
    }

    /// <summary>A Then step, and every And or But step after it: at the time of the item's When step.</summary>
    public async Task AtThenTimeAsync()
    {
        await ResolveSetupAsync(null);
        driver.SetNow(context.WhenTime
            ?? (context.LatestTime > F005Driver.DefaultNow ? context.LatestTime : F005Driver.DefaultNow));
    }

    /// <summary>Account and habit setup in a Given step that gives no time.</summary>
    public async Task SetupAsync(Func<Task> setup)
    {
        if (context.SetupResolved)
        {
            await setup();
            return;
        }

        context.PendingSetup.Add(setup);
    }

    private async Task ResolveSetupAsync(DateTimeOffset? firstTime)
    {
        if (context.SetupResolved)
        {
            return;
        }

        context.SetupResolved = true;
        context.SetupTime = firstTime is { } given && given < F005Driver.DefaultNow ? given : F005Driver.DefaultNow;
        foreach (var setup in context.PendingSetup)
        {
            await setup();
        }

        context.PendingSetup.Clear();
    }

    private void AtSetupTime() => driver.SetNow(context.SetupTime);

    // ------------------------------------------------------------ accounts

    /// <summary>"the registered user `x`" (optionally "in `Z`"): registration with walking42, then sign-in.</summary>
    public Task RegisteredUserAsync(string email, string? timeZone) => SetupAsync(async () =>
    {
        AtSetupTime();
        var registered = await driver.RegisterAsync(email, F005Text.RegisteredUserPassword, timeZone, null);
        Assert.Equal(RegistrationOutcome.Registered, registered.Outcome);
        var user = await SignInAsync(email, F005Text.RegisteredUserPassword);
        Assert.Equal(Role.RegisteredUser, user.Role);
    });

    /// <summary>"the administrator `a`": administrator creation with admin2026, then sign-in.</summary>
    public Task AdministratorAsync(string email) => SetupAsync(async () =>
    {
        AtSetupTime();
        var created = await driver.CreateAdministratorAsync(email, F005Text.AdministratorPassword);
        Assert.Equal(AdministratorCreationOutcome.Created, created.Outcome);
        var user = await SignInAsync(email, F005Text.AdministratorPassword);
        Assert.Equal(Role.Administrator, user.Role);
    });

    public async Task<ActingUser> SignInAsync(string email, string password)
    {
        var signIn = await driver.SignInAsync(email, password);
        Assert.Equal(SignInOutcome.SignedIn, signIn.Outcome);
        var user = signIn.ActingUser ?? throw new InvalidOperationException($"Signing in `{email}` gave no acting user.");
        context.Accounts[email] = user;
        return user;
    }

    /// <summary>The acting user of an email: signed in by a step, or a demo account signed in with the demo password.</summary>
    public async Task<ActingUser> AccountAsync(string email)
    {
        email = WithRow(email);
        if (context.Accounts.TryGetValue(email, out var known))
        {
            return known;
        }

        return F005Text.IsDemoEmail(email)
            ? await SignInAsync(email, F005Text.DemoPassword)
            : throw new InvalidOperationException($"No account `{email}` signed in in this scenario.");
    }

    /// <summary>"the anonymous visitor", "the administrator `a`" or "`x`".</summary>
    public async Task<(string? Email, ActingUser User)> ActorAsync(string token)
    {
        var text = WithRow(token).Trim();
        if (text == F005Text.AnonymousVisitor)
        {
            return (null, ActingUser.Anonymous);
        }

        var match = Regex.Match(text, "`([^`]+)`");
        if (!match.Success)
        {
            throw new InvalidOperationException($"\"{token}\" names no actor.");
        }

        var email = match.Groups[1].Value;
        return (email, await AccountAsync(email));
    }

    // ------------------------------------------------------------ habits

    /// <summary>"`x` created the custom habit `C`": intent Build, category Other, icon book-open.</summary>
    public Task CustomHabitAsync(string ownerEmail, string name) => SetupAsync(async () =>
    {
        AtSetupTime();
        var input = new HabitInput
        {
            Name = name,
            Description = null,
            Category = Category.Other,
            Intent = Intent.Build,
            Icon = F005Text.DefaultIcon,
        };
        var created = await driver.CreateCustomHabitAsync(await AccountAsync(ownerEmail), input);
        Assert.Equal(CustomHabitCreateOutcome.Created, created.Outcome);
        context.CustomHabits[(ownerEmail, name)] =
            created.HabitId ?? throw new InvalidOperationException($"Creating `{name}` gave no identifier.");
    });

    /// <summary>"a Published `N`", "a Draft `N`" or "an Unpublished `N`", created by the administrator `a`.</summary>
    public Task CatalogHabitAsync(string administratorEmail, CatalogHabitState state, string name) => SetupAsync(async () =>
    {
        AtSetupTime();
        var administrator = await AccountAsync(administratorEmail);
        var input = new CatalogHabitInput
        {
            Name = name,
            Description = null,
            Category = Category.Other,
            Intent = Intent.Build,
            Icon = F005Text.DefaultIcon,
            SuggestedStartingGoals = [],
        };
        var created = await driver.CreateCatalogHabitAsync(administrator, input);
        Assert.Equal(CatalogHabitCreateOutcome.Created, created.Outcome);
        var id = created.HabitId ?? throw new InvalidOperationException($"Creating `{name}` gave no identifier.");

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

        context.CatalogHabits[name] = id;
    });

    // ------------------------------------------------------------ goals

    /// <summary>
    /// Slice 4's create-goal use case with the settings, the start date and no name, at the step's time; creating
    /// must succeed. The habit is "their custom habit `C`" or "the catalog habit `H`".
    /// </summary>
    public async Task<F005Goal> CreateGoalAsync(string ownerEmail, string habitPhrase, string settingsText, DateOnly startDate)
    {
        var settings = F005Text.Settings(settingsText);
        var input = new GoalInput
        {
            Habit = Habit(ownerEmail, habitPhrase),
            Target = settings.ToFields(),
            StartDate = startDate,
            Name = null,
        };
        var created = await driver.CreateGoalAsync(await AccountAsync(ownerEmail), input);
        Assert.Equal(GoalCreateOutcome.Created, created.Outcome);
        var goal = new F005Goal(
            ownerEmail,
            created.GoalId ?? throw new InvalidOperationException("Creating the goal gave no identifier."),
            settings.Kind);
        context.Goals.Add(goal);
        context.MentionedDates.Add(startDate);
        return goal;
    }

    private GoalHabit Habit(string ownerEmail, string phrase)
    {
        var text = phrase.Trim();
        var custom = Regex.Match(text, "^their custom habit `([^`]+)`$");
        if (custom.Success)
        {
            return new GoalHabit(context.CustomHabits[(ownerEmail, custom.Groups[1].Value)], HabitKind.Custom);
        }

        var catalog = Regex.Match(text, "^the catalog habit `([^`]+)`$");
        if (catalog.Success)
        {
            return new GoalHabit(context.CatalogHabits[catalog.Groups[1].Value], HabitKind.Catalog);
        }

        throw new InvalidOperationException($"\"{phrase}\" names no habit.");
    }

    /// <summary>"the goal": the only goal of the item.</summary>
    public F005Goal OnlyGoal =>
        context.Goals.Count == 1
            ? context.Goals[0]
            : throw new InvalidOperationException($"\"the goal\" is ambiguous: the item created {context.Goals.Count} goals.");

    /// <summary>The goal an account created last.</summary>
    public F005Goal GoalOf(string ownerEmail)
    {
        var owner = WithRow(ownerEmail);
        return context.Goals.LastOrDefault(goal => goal.OwnerEmail == owner)
            ?? throw new InvalidOperationException($"`{owner}` created no goal in this scenario.");
    }

    /// <summary>"the first goal" (0) or "the second goal" (1): of the actor, or of the item when the actor has none.</summary>
    public F005Goal NthGoal(string? actorEmail, int index)
    {
        var own = context.Goals.Where(goal => goal.OwnerEmail == actorEmail).ToList();
        var goals = own.Count > 0 ? own : context.Goals;
        return index < goals.Count
            ? goals[index]
            : throw new InvalidOperationException($"The item has no goal number {index + 1}.");
    }

    public static int Ordinal(string word) => word == "first" ? 0 : word == "second" ? 1 : throw new InvalidOperationException(word);

    /// <summary>"the goal" of an actor: the actor's own goal, or the item's only goal.</summary>
    public F005Goal DefaultGoal(string? actorEmail) =>
        actorEmail is not null && context.Goals.Any(goal => goal.OwnerEmail == actorEmail) ? GoalOf(actorEmail) : OnlyGoal;

    /// <summary>"the goal", "the goal of `x`", "the first goal", "the second goal" or a demo goal's name in quotes.</summary>
    public async Task<F005Goal> GoalPhraseAsync(string phrase, string? actorEmail)
    {
        var text = WithRow(phrase).Trim();
        Match match;
        if (text == "the goal")
        {
            return DefaultGoal(actorEmail);
        }

        if ((match = Regex.Match(text, "^the goal of `([^`]+)`$")).Success)
        {
            return GoalOf(match.Groups[1].Value);
        }

        if ((match = Regex.Match(text, "^the (first|second) goal$")).Success)
        {
            return NthGoal(actorEmail, Ordinal(match.Groups[1].Value));
        }

        if ((match = Regex.Match(text, "^\"([^\"]+)\"$")).Success)
        {
            return await DemoGoalAsync(actorEmail ?? throw new InvalidOperationException("A demo goal needs its account."), match.Groups[1].Value);
        }

        throw new InvalidOperationException($"\"{phrase}\" names no goal.");
    }

    /// <summary>A demo goal by its name, from its account's goal list.</summary>
    public async Task<F005Goal> DemoGoalAsync(string email, string name)
    {
        var key = email + "|" + name;
        if (context.DemoGoals.TryGetValue(key, out var known))
        {
            return known;
        }

        var item = Assert.Single(await ListGoalsAsync(email), goal => goal.Name == name);
        var kind = F005Text.DemoGoalKinds.TryGetValue(name, out var given)
            ? given
            : throw new InvalidOperationException($"\"{name}\" is not a demo goal of the Gherkin.");
        var goal = new F005Goal(email, item.Id, kind);
        context.DemoGoals[key] = goal;
        return goal;
    }

    public async Task<IReadOnlyList<GoalListItem>> ListGoalsAsync(string email)
    {
        var list = await driver.ListGoalsAsync(await AccountAsync(email));
        Assert.Equal(GoalListOutcome.Found, list.Outcome);
        return list.Goals;
    }

    public async Task<GoalDetails> ReadGoalAsync(string email, GoalId id)
    {
        var read = await driver.GetGoalAsync(await AccountAsync(email), id);
        Assert.Equal(GoalReadOutcome.Found, read.Outcome);
        return read.Goal ?? throw new InvalidOperationException("Reading the goal gave no goal.");
    }

    // ------------------------------------------------------------ history and records

    /// <summary>The history of a goal read by an account at the supplied current time; the read must succeed.</summary>
    public async Task<GoalHistory> HistoryAsync(F005Goal goal, string readerEmail, DateOnly first, DateOnly last)
    {
        var read = await driver.GetHistoryAsync(await AccountAsync(readerEmail), goal.Id, first, last);
        Assert.Equal(HistoryOutcome.Found, read.Outcome);
        return read.History ?? throw new InvalidOperationException("Reading the history gave no history.");
    }

    /// <summary>
    /// "the record of D": the identifier of the only record dated D that the goal's history, read by its owner,
    /// gives at that time; "the first record of D": the first of several, in the history's order.
    /// </summary>
    public async Task<RecordId> RecordIdAsync(F005Goal goal, DateOnly date, bool first)
    {
        var history = await HistoryAsync(goal, goal.OwnerEmail, date, date);
        var records = history.Entries.SelectMany(entry => entry.Records).Where(record => record.Date == date).ToList();
        if (first)
        {
            Assert.NotEmpty(records);
            return records[0].Id;
        }

        return Assert.Single(records).Id;
    }

    /// <summary>A demo history loading result as its lines: (email or none, text), in order.</summary>
    public static List<(string? Email, string Text)> Lines(DemoHistoryLoadingResult result)
    {
        var lines = new List<(string? Email, string Text)>();
        if (result.Message is not null)
        {
            lines.Add((null, result.Message));
        }

        foreach (var account in result.Accounts)
        {
            lines.Add((account.Email, account.Message));
            if (account.Goals is not null)
            {
                lines.Add((account.Email, account.Goals.Message));
            }

            if (account.Records is not null)
            {
                lines.Add((account.Email, account.Records.Message));
            }
        }

        return lines;
    }
}
