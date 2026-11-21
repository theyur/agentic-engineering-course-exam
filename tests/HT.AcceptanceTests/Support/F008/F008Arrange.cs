using System.Text.RegularExpressions;
using HT.Application.Accounts;
using HT.Application.Exemptions;
using HT.Application.Goals;
using HT.Application.Habits;
using HT.Application.Operator;
using HT.Application.Records;
using HT.Application.Reports;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;
using HT.Domain.Records;

namespace HT.AcceptanceTests.Support.F008;

/// <summary>
/// Arranges what the F008 steps name, as acceptance.md's conventions define it, and keeps the supplied current time:
/// <list type="bullet">
/// <item>a step runs at the time it gives;</item>
/// <item>setup that gives no time (registration, sign-in, administrator creation, custom habits, unpublishing) runs at
/// the earliest time the item gives; in every F008 item that is the time of its first step that gives one (the goal
/// creation, at 2026-11-02T08:00:00Z unless the item gives another time, or the demo run);</item>
/// <item>a Given step that records and gives no time runs at the time of the When step;</item>
/// <item>Then steps run at the When step's time unless they give their own.</item>
/// </list>
/// </summary>
public sealed class F008Arrange(F008Context context, F008Driver driver)
{
    // ------------------------------------------------------------ time

    /// <summary>A step that gives its time: the first such step fixes the setup time.</summary>
    public void At(DateTimeOffset instant)
    {
        context.SetupTime ??= instant;
        driver.SetNow(instant);
    }

    public void At(string time) => At(F008Text.Instant(time));

    /// <summary>Setup that gives no time: at the earliest time the item gives.</summary>
    public void AtSetupTime() =>
        driver.SetNow(context.SetupTime ?? throw new InvalidOperationException("No step of the item has given a time yet."));

    /// <summary>The item's When step: at its time; the Given steps that record and gave no time run first, at that time.</summary>
    public async Task WhenAsync(string time)
    {
        var instant = F008Text.Instant(time);
        At(instant);
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
    public void Then(string? time = null)
    {
        if (!string.IsNullOrWhiteSpace(time))
        {
            driver.SetNow(F008Text.Instant(time));
            return;
        }

        driver.SetNow(context.WhenTime ?? throw new InvalidOperationException("The item's When step has not run."));
    }

    /// <summary>A Given step that records and gives no time: it runs at the time of the When step.</summary>
    public void DeferToWhen(Func<Task> action) => context.DeferredToWhen.Add(action);

    // ------------------------------------------------------------ accounts

    /// <summary>
    /// "the registered user `x`" (optionally "in `Z`"): the anonymous visitor registers `x` with walking42, the time
    /// zone (none: the default) and no name; registration succeeds; sign-in with `x` and walking42 succeeds.
    /// </summary>
    public async Task RegisteredUserAsync(string email, string? timeZone)
    {
        var registered = await driver.RegisterAsync(email, F008Text.RegisteredUserPassword, timeZone, null);
        Assert.Equal(RegistrationOutcome.Registered, registered.Outcome);
        var user = await SignInAsync(email, F008Text.RegisteredUserPassword);
        Assert.Equal(Role.RegisteredUser, user.Role);
    }

    /// <summary>"the administrator `a`": administrator creation with admin2026 creates it, then sign-in succeeds.</summary>
    public async Task AdministratorAsync(string email)
    {
        var created = await driver.CreateAdministratorAsync(email, F008Text.AdministratorPassword);
        Assert.Equal(AdministratorCreationOutcome.Created, created.Outcome);
        var user = await SignInAsync(email, F008Text.AdministratorPassword);
        Assert.Equal(Role.Administrator, user.Role);
    }

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

        return Regex.IsMatch(email, @"^demo[1-6]@example\.com$")
            ? await SignInAsync(email, F008Text.DemoPassword)
            : throw new InvalidOperationException($"No account `{email}` signed in in this scenario.");
    }

    /// <summary>"the anonymous visitor" or "`x`": the acting user and its email (none for the anonymous visitor).</summary>
    public async Task<(string? Email, ActingUser User)> ActorAsync(string token)
    {
        var text = token.Trim();
        if (text == F008Text.AnonymousVisitor)
        {
            return (null, ActingUser.Anonymous);
        }

        var match = Regex.Match(text, "^`([^`]+)`$");
        if (!match.Success)
        {
            throw new InvalidOperationException($"\"{token}\" names no actor.");
        }

        return (match.Groups[1].Value, await AccountAsync(match.Groups[1].Value));
    }

    // ------------------------------------------------------------ habits and goals

    /// <summary>
    /// "`x` created the goal `G` at T with the start date D": `x` creates the custom habit of G (unless `x` already has
    /// it: intent Build, category Other, icon book-open), then creates a goal on it with G's settings, the start date
    /// and no name; creating succeeds. Runs at the current supplied time.
    /// </summary>
    public async Task<F008Goal> CreateGoalAsync(string ownerEmail, string goalName, DateOnly startDate)
    {
        var definition = F008Text.Goal(goalName);
        var user = await AccountAsync(ownerEmail);
        if (!context.CustomHabits.TryGetValue((ownerEmail, definition.Habit), out var habitId))
        {
            var created = await driver.CreateCustomHabitAsync(user, new HabitInput
            {
                Name = definition.Habit,
                Description = null,
                Category = Category.Other,
                Intent = Intent.Build,
                Icon = F008Text.DefaultIcon,
            });
            Assert.Equal(CustomHabitCreateOutcome.Created, created.Outcome);
            habitId = created.HabitId ?? throw new InvalidOperationException($"Creating `{definition.Habit}` gave no identifier.");
            context.CustomHabits[(ownerEmail, definition.Habit)] = habitId;
        }

        var goal = await driver.CreateGoalAsync(user, new GoalInput
        {
            Habit = new GoalHabit(habitId, HabitKind.Custom),
            Target = definition.Fields,
            StartDate = startDate,
            Name = null,
        });
        Assert.Equal(GoalCreateOutcome.Created, goal.Outcome);
        var created2 = new F008Goal(
            ownerEmail,
            goalName,
            goal.GoalId ?? throw new InvalidOperationException("Creating the goal gave no identifier."));
        context.Goals.Add(created2);
        return created2;
    }

    /// <summary>"the goal": the only goal of the item.</summary>
    public F008Goal OnlyGoal()
    {
        var owners = context.Goals;
        return owners.Count == 1
            ? owners[0]
            : throw new InvalidOperationException($"\"the goal\" is ambiguous: the item has {owners.Count} goals.");
    }

    /// <summary>The only goal of an account (as "the goal" of a step that names its actor).</summary>
    public F008Goal OnlyGoalOf(string ownerEmail)
    {
        var goals = context.Goals.Where(goal => goal.OwnerEmail == ownerEmail).ToList();
        return goals.Count == 1
            ? goals[0]
            : throw new InvalidOperationException($"`{ownerEmail}` has {goals.Count} goals in this item, not one.");
    }

    /// <summary>A goal of an account by its name: one a step created, or (demo accounts) one from slice 4's goal list.</summary>
    public async Task<F008Goal> GoalNamedAsync(string ownerEmail, string name)
    {
        var known = context.Goals.Where(goal => goal.OwnerEmail == ownerEmail && goal.Name == name).ToList();
        if (known.Count == 1)
        {
            return known[0];
        }

        var list = await driver.ListGoalsAsync(await AccountAsync(ownerEmail));
        Assert.Equal(GoalListOutcome.Found, list.Outcome);
        var item = Assert.Single(list.Goals, goal => goal.Name == name);
        var goal = new F008Goal(ownerEmail, item.Name, item.Id);
        context.Goals.Add(goal);
        return goal;
    }

    /// <summary>A goal named without its owner: the only goal of that name in the item, or the When actor's goal of that name.</summary>
    public async Task<F008Goal> GoalNamedAsync(string name)
    {
        var known = context.Goals.Where(goal => goal.Name == name).ToList();
        if (known.Count == 1)
        {
            return known[0];
        }

        var owner = context.WhenActor ?? throw new InvalidOperationException($"`{name}` names no goal of this item.");
        return await GoalNamedAsync(owner, name);
    }

    // ------------------------------------------------------------ records

    public static RecordInput Input(string value)
    {
        var (answer, amount) = F008Text.ValueOf(value);
        return new RecordInput { Answer = answer, Amount = amount, Note = null };
    }

    /// <summary>Slice 5's record use case for the goal, the date and the value, with no note; it must succeed.</summary>
    public async Task RecordSucceedsAsync(F008Goal goal, DateOnly date, string value)
    {
        var result = await driver.RecordAsync(await AccountAsync(goal.OwnerEmail), goal.Id, date, Input(value));
        Assert.True(
            result.Outcome == RecordWriteOutcome.Recorded,
            $"Recording {value} for {date:yyyy-MM-dd} gave {result.Outcome}: \"{result.Message}\".");
    }

    /// <summary>"the record of D": the only record dated D that the goal's history, read by its owner, gives now.</summary>
    public async Task<RecordId> RecordIdAsync(F008Goal goal, DateOnly date)
    {
        var history = await HistoryAsync(goal, date, date);
        return Assert.Single(history.Entries.SelectMany(entry => entry.Records), record => record.Date == date).Id;
    }

    // ------------------------------------------------------------ pauses created in the item

    /// <summary>Keeps the pause a "paused" result gave, so later steps can name it by its days.</summary>
    public void RememberPause(F008Goal goal, PauseResult result)
    {
        var pause = result.Pause ?? throw new InvalidOperationException("The \"paused\" result gave no pause.");
        context.Pauses.Add(new F008Pause(goal, pause.FirstDay, pause.LastDay, pause.Id));
    }

    /// <summary>"the pause D1 – D2" of a goal: the identifier the "paused" result gave for those days.</summary>
    public F008Pause PauseOf(F008Goal goal, DateOnly first, DateOnly last) =>
        Assert.Single(context.Pauses, pause => pause.Goal.Id == goal.Id && pause.FirstDay == first && pause.LastDay == last);

    // ------------------------------------------------------------ reads (a read changes nothing)

    public async Task<GoalHistory> HistoryAsync(F008Goal goal, DateOnly first, DateOnly last, string? readerEmail = null)
    {
        var read = await driver.GetHistoryAsync(await AccountAsync(readerEmail ?? goal.OwnerEmail), goal.Id, first, last);
        Assert.True(read.Outcome == HistoryOutcome.Found, $"Reading the history gave {read.Outcome}: \"{read.Message}\".");
        return read.History ?? throw new InvalidOperationException("Reading the history gave no history.");
    }

    public async Task<GoalHistory> RecordableAsync(F008Goal goal)
    {
        var read = await driver.GetRecordableAsync(await AccountAsync(goal.OwnerEmail), goal.Id);
        Assert.Equal(RecordableOutcome.Found, read.Outcome);
        return read.History ?? throw new InvalidOperationException("Reading what can be recorded now gave no history.");
    }

    public async Task<IReadOnlyList<PauseView>> PausesAsync(F008Goal goal, string? readerEmail = null)
    {
        var read = await driver.GetPausesAsync(await AccountAsync(readerEmail ?? goal.OwnerEmail), goal.Id);
        Assert.True(read.Outcome == PausesOutcome.Found, $"Reading the pauses gave {read.Outcome}: \"{read.Message}\".");
        return read.Pauses;
    }

    public async Task<IReadOnlyList<RecentNote>> RecentNotesAsync(F008Goal goal)
    {
        var read = await driver.GetRecentNotesAsync(await AccountAsync(goal.OwnerEmail), goal.Id);
        Assert.Equal(RecentNotesOutcome.Found, read.Outcome);
        return read.Notes;
    }

    public async Task<GoalReport> ReportAsync(F008Goal goal, string? readerEmail = null)
    {
        var result = await driver.GetGoalReportAsync(await AccountAsync(readerEmail ?? goal.OwnerEmail), goal.Id);
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

    /// <summary>The account a Then step reads as: the When step's actor, or the owner of the item's only goal.</summary>
    public string Reader() =>
        context.WhenActor
        ?? context.Goals.Select(goal => goal.OwnerEmail).Distinct().SingleOrDefault()
        ?? throw new InvalidOperationException("The item names no account to read as.");

    // ------------------------------------------------------------ demo tasks

    /// <summary>"the earlier demo tasks run at T": reset, then demo loading, demo history loading and demo report loading at T with example2026.</summary>
    public async Task EarlierDemoTasksAsync(string time)
    {
        await F008Driver.ResetDatabaseAsync();
        context.Accounts.Clear();
        At(time);
        context.DemoLoading = await driver.LoadDemoDataAsync(F008Text.DemoPassword);
        context.DemoHistory = await driver.LoadDemoHistoryAsync(F008Text.DemoPassword);
        context.DemoReport = await driver.LoadDemoReportAsync(F008Text.DemoPassword);
    }

    /// <summary>The results of demo pause loading as (email or none, text), in order.</summary>
    public static List<(string? Email, string Text)> Lines(DemoPauseLoadingResult result)
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
