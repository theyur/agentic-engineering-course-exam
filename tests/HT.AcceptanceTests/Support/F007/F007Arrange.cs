using System.Text.RegularExpressions;
using HT.Application.Accounts;
using HT.Application.Goals;
using HT.Application.Habits;
using HT.Application.Operator;
using HT.Application.Records;
using HT.Application.Reports;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;

namespace HT.AcceptanceTests.Support.F007;

/// <summary>
/// Arranges what the F007 steps name, as acceptance.md's conventions define it, and keeps the supplied current time:
/// <list type="bullet">
/// <item>a step runs at the time it gives;</item>
/// <item>setup that gives no time (accounts, habits, unpublishing) runs at the earliest time the item gives, which in
/// every F007 item is the time of its first step that gives one; such setup written before that step is held until
/// it, or runs at <see cref="F007Driver.DefaultNow"/> when the item gives no time at all;</item>
/// <item>a Given step that records or saves a note and gives no time runs at the time of the When step;</item>
/// <item>Then steps run at the When step's time unless they give their own.</item>
/// </list>
/// </summary>
public sealed class F007Arrange(F007Context context, F007Driver driver)
{
    // ------------------------------------------------------------ time

    /// <summary>A Given step other than setup that runs now: at its time (setup is resolved by it first).</summary>
    public async Task AtAsync(string? time)
    {
        DateTimeOffset? instant = string.IsNullOrWhiteSpace(time) ? null : F007Text.Instant(time);
        await ResolveSetupAsync(instant);
        driver.SetNow(instant ?? context.WhenTime ?? context.SetupTime);
    }

    /// <summary>The item's When step: at its time; the Given records and notes that gave no time run first, at that time.</summary>
    public async Task WhenAsync(string time)
    {
        var instant = F007Text.Instant(time);
        await ResolveSetupAsync(instant);
        driver.SetNow(instant);
        context.WhenTime = instant;
        foreach (var deferred in context.DeferredToWhen)
        {
            driver.SetNow(instant);
            await deferred();
        }

        context.DeferredToWhen.Clear();
        driver.SetNow(instant);
    }

    /// <summary>A Then step: at its own time, or at the When step's time.</summary>
    public async Task ThenAsync(string? time = null)
    {
        DateTimeOffset? instant = string.IsNullOrWhiteSpace(time) ? null : F007Text.Instant(time);
        await ResolveSetupAsync(instant);
        driver.SetNow(instant ?? context.WhenTime ?? context.SetupTime);
    }

    /// <summary>A Given step that records or saves notes: at its time, or (no time) at the When step's time.</summary>
    public async Task RecordingStepAsync(string? time, Func<Task> action)
    {
        if (string.IsNullOrWhiteSpace(time))
        {
            context.DeferredToWhen.Add(action);
            return;
        }

        await AtAsync(time);
        await action();
    }

    /// <summary>Account, habit and unpublishing setup in a Given step that gives no time.</summary>
    public async Task SetupAsync(Func<Task> setup)
    {
        if (context.SetupResolved)
        {
            driver.SetNow(context.SetupTime);
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
        context.SetupTime = firstTime ?? F007Driver.DefaultNow;
        foreach (var setup in context.PendingSetup)
        {
            driver.SetNow(context.SetupTime);
            await setup();
        }

        context.PendingSetup.Clear();
    }

    // ------------------------------------------------------------ accounts

    /// <summary>"the registered user `x`" (optionally "in `Z`"): registration with walking42 and the time zone, then sign-in.</summary>
    public Task RegisteredUserAsync(string email, string? timeZone) => SetupAsync(async () =>
    {
        var registered = await driver.RegisterAsync(email, F007Text.RegisteredUserPassword, timeZone, null);
        Assert.Equal(RegistrationOutcome.Registered, registered.Outcome);
        var user = await SignInAsync(email, F007Text.RegisteredUserPassword);
        Assert.Equal(Role.RegisteredUser, user.Role);
    });

    /// <summary>"the administrator `a`": administrator creation with admin2026, then sign-in.</summary>
    public Task AdministratorAsync(string email) => SetupAsync(AdministratorNowAsync(email));

    public Func<Task> AdministratorNowAsync(string email) => async () =>
    {
        var created = await driver.CreateAdministratorAsync(email, F007Text.AdministratorPassword);
        Assert.Equal(AdministratorCreationOutcome.Created, created.Outcome);
        var user = await SignInAsync(email, F007Text.AdministratorPassword);
        Assert.Equal(Role.Administrator, user.Role);
    };

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
        if (context.Accounts.TryGetValue(email, out var known))
        {
            return known;
        }

        return Regex.IsMatch(email, @"^demo[1-5]@example\.com$")
            ? await SignInAsync(email, F007Text.DemoPassword)
            : throw new InvalidOperationException($"No account `{email}` signed in in this scenario.");
    }

    /// <summary>"the anonymous visitor" or "the administrator `a`" or "`x`".</summary>
    public async Task<(string? Email, ActingUser User)> ActorAsync(string token)
    {
        var text = token.Trim();
        if (text == F007Text.AnonymousVisitor)
        {
            return (null, ActingUser.Anonymous);
        }

        var match = Regex.Match(text, "`([^`]+)`");
        if (!match.Success)
        {
            throw new InvalidOperationException($"\"{token}\" names no actor.");
        }

        return (match.Groups[1].Value, await AccountAsync(match.Groups[1].Value));
    }

    // ------------------------------------------------------------ habits

    /// <summary>"`x` created the custom habit `C`": intent Build, category Other, icon book-open.</summary>
    public Task CustomHabitAsync(string ownerEmail, string name) => SetupAsync(async () =>
    {
        var input = new HabitInput
        {
            Name = name,
            Description = null,
            Category = Category.Other,
            Intent = Intent.Build,
            Icon = F007Text.DefaultIcon,
        };
        var created = await driver.CreateCustomHabitAsync(await AccountAsync(ownerEmail), input);
        Assert.Equal(CustomHabitCreateOutcome.Created, created.Outcome);
        context.CustomHabits[(ownerEmail, name)] =
            created.HabitId ?? throw new InvalidOperationException($"Creating `{name}` gave no identifier.");
    });

    /// <summary>"`a` created the catalog habit `N` and published it": no description, Other, Build, book-open, no suggested goal.</summary>
    public async Task CatalogHabitAsync(string administratorEmail, string name)
    {
        var administrator = await AccountAsync(administratorEmail);
        var input = new CatalogHabitInput
        {
            Name = name,
            Description = null,
            Category = Category.Other,
            Intent = Intent.Build,
            Icon = F007Text.DefaultIcon,
            SuggestedStartingGoals = [],
        };
        var created = await driver.CreateCatalogHabitAsync(administrator, input);
        Assert.Equal(CatalogHabitCreateOutcome.Created, created.Outcome);
        var id = created.HabitId ?? throw new InvalidOperationException($"Creating `{name}` gave no identifier.");
        var published = await driver.PublishAsync(administrator, id);
        Assert.Equal(CatalogHabitPublishOutcome.Published, published.Outcome);
        context.CatalogHabits[name] = id;
    }

    /// <summary>"`a` unpublished the catalog habit `N` with confirmation": slice 2's unpublish use case, confirmed; it succeeds.</summary>
    public async Task UnpublishAsync(string administratorEmail, string name)
    {
        var administrator = await AccountAsync(administratorEmail);
        if (!context.CatalogHabits.TryGetValue(name, out var id))
        {
            var list = await driver.ListCatalogHabitsAsync(administrator, name);
            Assert.Equal(CatalogHabitListOutcome.Found, list.Outcome);
            id = Assert.Single(list.Habits, habit => habit.Name == name).Id;
        }

        var unpublished = await driver.UnpublishAsync(administrator, id, confirmed: true);
        Assert.Equal(CatalogHabitUnpublishOutcome.Unpublished, unpublished.Outcome);
    }

    // ------------------------------------------------------------ goals

    /// <summary>
    /// Slice 4's create-goal use case with the settings (and ", the name `M`" when given), the start date and otherwise
    /// no name, at the supplied time; creating must succeed. The habit is "their custom habit `C`" or "the catalog
    /// habit `N`".
    /// </summary>
    public async Task<F007Goal> CreateGoalAsync(string ownerEmail, string habitPhrase, string settingsText, DateOnly startDate)
    {
        var (fields, name) = F007Text.Settings(settingsText);
        var (habit, habitName) = Habit(ownerEmail, habitPhrase);
        var input = new GoalInput
        {
            Habit = habit,
            Target = fields,
            StartDate = startDate,
            Name = name,
        };
        var created = await driver.CreateGoalAsync(await AccountAsync(ownerEmail), input);
        Assert.Equal(GoalCreateOutcome.Created, created.Outcome);
        var goal = new F007Goal(
            ownerEmail,
            created.GoalId ?? throw new InvalidOperationException("Creating the goal gave no identifier."),
            habitName,
            name,
            context.Goals.Count(other => other.OwnerEmail == ownerEmail));
        context.Goals.Add(goal);
        return goal;
    }

    private (GoalHabit Habit, string Name) Habit(string ownerEmail, string phrase)
    {
        var text = phrase.Trim();
        var custom = Regex.Match(text, "^their custom habit `([^`]+)`$");
        if (custom.Success)
        {
            var name = custom.Groups[1].Value;
            return (new GoalHabit(context.CustomHabits[(ownerEmail, name)], HabitKind.Custom), name);
        }

        var catalog = Regex.Match(text, "^the catalog habit `([^`]+)`$");
        if (catalog.Success)
        {
            var name = catalog.Groups[1].Value;
            return (new GoalHabit(context.CatalogHabits[name], HabitKind.Catalog), name);
        }

        throw new InvalidOperationException($"\"{phrase}\" names no habit.");
    }

    /// <summary>The goals an account created, in creation order.</summary>
    public List<F007Goal> GoalsOf(string ownerEmail) => context.Goals.Where(goal => goal.OwnerEmail == ownerEmail).ToList();

    /// <summary>
    /// A goal of an account by a selector: "the goal" (its only goal), "the first goal", "the second goal", "the `H`
    /// goal" (the goal on the habit `H`, or named `H`), "`M`" (the goal named `M`, or on the habit `M`), "the goal of
    /// `x`" (the only goal of `x`).
    /// </summary>
    public F007Goal Goal(string? ownerEmail, string selector)
    {
        var text = selector.Trim();
        Match match;
        if ((match = Regex.Match(text, "^the goal of `([^`]+)`$")).Success)
        {
            return Assert.Single(GoalsOf(match.Groups[1].Value));
        }

        var goals = ownerEmail is null ? context.Goals : GoalsOf(ownerEmail);
        if (text == "the goal")
        {
            return goals.Count == 1
                ? goals[0]
                : throw new InvalidOperationException($"\"the goal\" is ambiguous: {goals.Count} goals.");
        }

        if ((match = Regex.Match(text, "^the (first|second) goal$")).Success)
        {
            var index = match.Groups[1].Value == "first" ? 0 : 1;
            return index < goals.Count ? goals[index] : throw new InvalidOperationException($"No {match.Groups[1].Value} goal.");
        }

        if ((match = Regex.Match(text, "^the `([^`]+)` goal$")).Success)
        {
            var label = match.Groups[1].Value;
            var onHabit = goals.Where(goal => goal.Habit == label).ToList();
            return onHabit.Count == 1 ? onHabit[0] : Assert.Single(goals, goal => goal.Name == label);
        }

        if ((match = Regex.Match(text, "^`([^`]+)`$")).Success)
        {
            var label = match.Groups[1].Value;
            var named = goals.Where(goal => goal.Name == label).ToList();
            return named.Count == 1 ? named[0] : Assert.Single(goals, goal => goal.Habit == label);
        }

        throw new InvalidOperationException($"\"{selector}\" names no goal.");
    }

    /// <summary>A goal of an account by its display name, from slice 4's goal list (demo goals).</summary>
    public async Task<F007Goal> GoalByNameAsync(string ownerEmail, string name)
    {
        var item = Assert.Single(await ListGoalsAsync(ownerEmail), goal => goal.Name == name);
        return new F007Goal(ownerEmail, item.Id, item.HabitName, item.Name, -1);
    }

    public async Task<IReadOnlyList<GoalListItem>> ListGoalsAsync(string email)
    {
        var list = await driver.ListGoalsAsync(await AccountAsync(email));
        Assert.Equal(GoalListOutcome.Found, list.Outcome);
        return list.Goals;
    }

    // ------------------------------------------------------------ records and notes

    /// <summary>Slice 5's record use case for each record the phrase names, with no note; each must succeed.</summary>
    public async Task RecordAllAsync(string ownerEmail, string body)
    {
        var user = await AccountAsync(ownerEmail);
        foreach (var item in F007Text.Records(body))
        {
            var goal = Goal(ownerEmail, item.Goal ?? "the goal");
            await RecordOneAsync(user, goal, item.Date, item.Answer, item.Amount);
        }
    }

    public async Task RecordOneAsync(ActingUser user, F007Goal goal, DateOnly date, YesNoAnswer? answer, int? amount)
    {
        var result = await driver.RecordAsync(user, goal.Id, date, new RecordInput { Answer = answer, Amount = amount, Note = null });
        Assert.Equal(RecordWriteOutcome.Recorded, result.Outcome);
    }

    /// <summary>Slice 5's save-note use case for each note the phrase names; each must succeed.</summary>
    public async Task SaveNotesAsync(string ownerEmail, string body)
    {
        var user = await AccountAsync(ownerEmail);
        foreach (var item in F007Text.Notes(body))
        {
            var goal = Goal(ownerEmail, item.Goal ?? "the goal");
            var result = await driver.SaveNoteAsync(user, goal.Id, item.Date, item.Note);
            Assert.Equal(NoteSaveOutcome.Saved, result.Outcome);
        }
    }

    /// <summary>"the record of D": the only record dated D that the goal's history, read by its owner, gives now.</summary>
    public async Task<RecordId> RecordIdAsync(F007Goal goal, DateOnly date)
    {
        var read = await driver.GetHistoryAsync(await AccountAsync(goal.OwnerEmail), goal.Id, date, date);
        Assert.Equal(HistoryOutcome.Found, read.Outcome);
        var history = read.History ?? throw new InvalidOperationException("Reading the history gave no history.");
        return Assert.Single(history.Entries.SelectMany(entry => entry.Records), record => record.Date == date).Id;
    }

    // ------------------------------------------------------------ reads of this slice

    public async Task<GoalReport> ReportAsync(string email, F007Goal goal)
    {
        var result = await driver.GetGoalReportAsync(await AccountAsync(email), goal.Id);
        Assert.Equal(GoalReportOutcome.Found, result.Outcome);
        return result.Report ?? throw new InvalidOperationException("The goal report gave no report.");
    }

    public async Task<GoalsOverview> OverviewAsync(string email)
    {
        var result = await driver.GetGoalsOverviewAsync(await AccountAsync(email));
        Assert.Equal(GoalsOverviewOutcome.Found, result.Outcome);
        return result.Overview ?? throw new InvalidOperationException("The goals overview gave no overview.");
    }

    public async Task<Dashboard> DashboardAsync(string email)
    {
        var result = await driver.GetDashboardAsync(await AccountAsync(email));
        Assert.Equal(DashboardOutcome.Found, result.Outcome);
        return result.Dashboard ?? throw new InvalidOperationException("The dashboard gave no dashboard.");
    }

    // ------------------------------------------------------------ demo tasks

    /// <summary>"the demo run at T": reset, then demo loading, demo history loading and demo report loading at T with example2026.</summary>
    public async Task DemoRunAsync(string time)
    {
        await F007Driver.ResetDatabaseAsync();
        await AtAsync(time);
        context.DemoLoading = await driver.LoadDemoDataAsync(F007Text.DemoPassword);
        context.DemoHistory = await driver.LoadDemoHistoryAsync(F007Text.DemoPassword);
        context.DemoReport = await driver.LoadDemoReportAsync(F007Text.DemoPassword);
    }

    /// <summary>The lines of a demo report loading result: (email or none, text), in order.</summary>
    public static List<(string? Email, string Text)> Lines(DemoReportLoadingResult result)
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

    /// <summary>The lines of a demo history loading result: (email or none, text), in order.</summary>
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

    /// <summary>Every text of a demo loading result (top-level, catalog, accounts and their parts).</summary>
    public static List<string> Texts(DemoLoadingResult result)
    {
        var texts = new List<string?> { result.Message, result.Catalog?.Message };
        foreach (var account in result.Accounts)
        {
            texts.AddRange([account.Email, account.Message, account.CustomHabits?.Message, account.Goals?.Message]);
        }

        return texts.Where(text => text is not null).Select(text => text!).ToList();
    }
}
