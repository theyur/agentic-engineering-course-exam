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
using GoalOutcome = HT.Domain.Goals.Outcome;

namespace HT.AcceptanceTests.Support.F009;

/// <summary>
/// Arranges what the F009 steps name, as acceptance.md's conventions define it, and keeps the supplied current time:
/// <list type="bullet">
/// <item>a step runs at the time it gives;</item>
/// <item>setup that gives no time (registration, sign-in, administrator creation, custom habits) runs at the earliest
/// time the item gives, counting the default goal-creation time (2026-11-02T08:00:00Z) when the item uses it;</item>
/// <item>Then steps run at the When step's time unless they give their own.</item>
/// </list>
/// </summary>
public sealed class F009Arrange(F009Context context, F009Driver driver)
{
    // ------------------------------------------------------------ time

    /// <summary>A step that gives its time: the earliest such time is the setup time.</summary>
    public void At(DateTimeOffset instant)
    {
        if (context.SetupTime is null || instant < context.SetupTime)
        {
            context.SetupTime = instant;
        }

        driver.SetNow(instant);
    }

    public void At(string time) => At(F009Text.Instant(time));

    /// <summary>
    /// Setup that gives no time: at the earliest time the item gives. When no step has given a time yet, that is the
    /// default goal-creation time, which every such item uses.
    /// </summary>
    public void AtSetupTime() => driver.SetNow(context.SetupTime ?? F009Driver.DefaultGoalCreation);

    /// <summary>The item's When step: at its time, by its actor (none for the anonymous visitor or no actor).</summary>
    public void When(string time, string? actorEmail)
    {
        var instant = F009Text.Instant(time);
        At(instant);
        context.WhenTime = instant;
        context.WhenActor = actorEmail;
    }

    /// <summary>A Then step: at its own time, or at the When step's time.</summary>
    public void Then(string? time = null)
    {
        if (!string.IsNullOrWhiteSpace(time))
        {
            driver.SetNow(F009Text.Instant(time));
            return;
        }

        driver.SetNow(context.WhenTime ?? throw new InvalidOperationException("The item's When step has not run."));
    }

    // ------------------------------------------------------------ accounts

    /// <summary>
    /// "the registered user `x`" (optionally "in `Z`"): the anonymous visitor registers `x` with walking42, the time
    /// zone (none: the default) and no name; registration succeeds; sign-in with `x` and walking42 succeeds.
    /// </summary>
    public async Task RegisteredUserAsync(string email, string? timeZone)
    {
        var registered = await driver.RegisterAsync(email, F009Text.RegisteredUserPassword, timeZone, null);
        Assert.True(registered.Outcome == RegistrationOutcome.Registered, $"Registering `{email}` gave {registered.Outcome}: \"{registered.Message}\".");
        var user = await SignInAsync(email, F009Text.RegisteredUserPassword);
        Assert.Equal(Role.RegisteredUser, user.Role);
    }

    /// <summary>"the administrator `a`": administrator creation with admin2026 creates it, then sign-in succeeds.</summary>
    public async Task AdministratorAsync(string email)
    {
        var created = await driver.CreateAdministratorAsync(email, F009Text.AdministratorPassword);
        Assert.Equal(AdministratorCreationOutcome.Created, created.Outcome);
        var user = await SignInAsync(email, F009Text.AdministratorPassword);
        Assert.Equal(Role.Administrator, user.Role);
    }

    public async Task<ActingUser> SignInAsync(string email, string password)
    {
        var signIn = await driver.SignInAsync(email, password);
        Assert.True(signIn.Outcome == SignInOutcome.SignedIn, $"Signing in `{email}` gave {signIn.Outcome}: \"{signIn.Message}\".");
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

        return Regex.IsMatch(email, @"^demo[1-7]@example\.com$")
            ? await SignInAsync(email, F009Text.DemoPassword)
            : throw new InvalidOperationException($"No account `{email}` signed in in this scenario.");
    }

    /// <summary>"the anonymous visitor" or "`x`": the acting user and its email (none for the anonymous visitor).</summary>
    public async Task<(string? Email, ActingUser User)> ActorAsync(string token)
    {
        var text = token.Trim();
        if (text == F009Text.AnonymousVisitor)
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

    // ------------------------------------------------------------ habits

    /// <summary>"`x` created the custom habit `C`": intent Build, category Other, icon book-open (unless `x` already has it).</summary>
    public async Task<HabitId> CustomHabitAsync(string ownerEmail, string habit)
    {
        if (context.CustomHabits.TryGetValue((ownerEmail, habit), out var known))
        {
            return known;
        }

        var created = await driver.CreateCustomHabitAsync(await AccountAsync(ownerEmail), new HabitInput
        {
            Name = habit,
            Description = null,
            Category = Category.Other,
            Intent = Intent.Build,
            Icon = F009Text.DefaultIcon,
        });
        Assert.True(created.Outcome == CustomHabitCreateOutcome.Created, $"Creating the custom habit `{habit}` gave {created.Outcome}: \"{created.Message}\".");
        var id = created.HabitId ?? throw new InvalidOperationException($"Creating `{habit}` gave no identifier.");
        context.CustomHabits[(ownerEmail, habit)] = id;
        return id;
    }

    /// <summary>A Published catalog habit by its name, from the public catalog (the anonymous visitor's view).</summary>
    public async Task<HabitId> CatalogHabitIdAsync(string name)
    {
        if (context.CatalogHabits.TryGetValue(name, out var known))
        {
            return known;
        }

        var browse = await driver.BrowseAsync(new CatalogFilter { Search = name });
        Assert.Equal(CatalogBrowseOutcome.Found, browse.Outcome);
        var habit = Assert.Single(browse.Habits, candidate => candidate.Name == name);
        context.CatalogHabits[name] = habit.Id;
        return habit.Id;
    }

    /// <summary>A catalog habit by its name, as an administrator lists it (any state).</summary>
    public async Task<HabitId> CatalogHabitIdAsAdministratorAsync(ActingUser administrator, string name)
    {
        var list = await driver.ListCatalogHabitsAsync(administrator, name);
        Assert.Equal(CatalogHabitListOutcome.Found, list.Outcome);
        return Assert.Single(list.Habits, candidate => candidate.Name == name).Id;
    }

    // ------------------------------------------------------------ goals

    /// <summary>
    /// "`x` created the goal `G`": `x` creates the custom habit of G (unless `x` already has it), then creates a goal on
    /// it with G's settings, the start date and no name; creating succeeds. Runs at the current supplied time.
    /// </summary>
    public async Task<F009Goal> CreateGoalAsync(string ownerEmail, string goalName, DateOnly startDate)
    {
        var definition = F009Text.Goal(goalName);
        var habitId = await CustomHabitAsync(ownerEmail, definition.Habit);
        var id = await CreateGoalOnAsync(ownerEmail, new GoalHabit(habitId, HabitKind.Custom), definition.Settings, startDate);
        var goal = new F009Goal(ownerEmail, goalName, id, habitId, definition.Habit);
        context.Goals.Add(goal);
        return goal;
    }

    /// <summary>"created a goal on the catalog habit "H" with S, the start date D and no name": creating succeeds.</summary>
    public async Task<F009Goal> CreateCatalogGoalAsync(string ownerEmail, string habitName, F009Text.SettingsSpec settings, DateOnly startDate)
    {
        var habitId = await CatalogHabitIdAsync(habitName);
        var id = await CreateGoalOnAsync(ownerEmail, new GoalHabit(habitId, HabitKind.Catalog), settings, startDate);
        var goal = new F009Goal(ownerEmail, null, id, habitId, habitName);
        context.Goals.Add(goal);
        return goal;
    }

    private async Task<GoalId> CreateGoalOnAsync(string ownerEmail, GoalHabit habit, F009Text.SettingsSpec settings, DateOnly startDate)
    {
        var created = await driver.CreateGoalAsync(await AccountAsync(ownerEmail), new GoalInput
        {
            Habit = habit,
            Target = settings.ToFields(),
            StartDate = startDate,
            Name = null,
        });
        Assert.True(created.Outcome == GoalCreateOutcome.Created, $"Creating the goal gave {created.Outcome}: \"{created.Message}\".");
        return created.GoalId ?? throw new InvalidOperationException("Creating the goal gave no identifier.");
    }

    /// <summary>"the goal": the only goal of the item.</summary>
    public F009Goal OnlyGoal()
    {
        var goals = context.Goals;
        return goals.Count == 1
            ? goals[0]
            : throw new InvalidOperationException($"\"the goal\" is ambiguous: the item has {goals.Count} goals.");
    }

    /// <summary>The only goal of an account (as "the goal" of a step that names its actor).</summary>
    public F009Goal OnlyGoalOf(string ownerEmail)
    {
        var goals = context.Goals.Where(goal => goal.OwnerEmail == ownerEmail).ToList();
        return goals.Count == 1
            ? goals[0]
            : throw new InvalidOperationException($"`{ownerEmail}` has {goals.Count} goals in this item, not one.");
    }

    /// <summary>A goal of an account by its name: one a step created with that name, or one from slice 4's goal list.</summary>
    public async Task<F009Goal> GoalNamedAsync(string ownerEmail, string name)
    {
        var known = context.Goals.Where(goal => goal.OwnerEmail == ownerEmail && goal.Label == name).ToList();
        if (known.Count == 1)
        {
            return known[0];
        }

        var list = await driver.ListGoalsAsync(await AccountAsync(ownerEmail));
        Assert.Equal(GoalListOutcome.Found, list.Outcome);
        var item = Assert.Single(list.Goals, goal => goal.Name == name);
        var existing = context.Goals.FindIndex(goal => goal.Id == item.Id);
        if (existing >= 0)
        {
            var named = context.Goals[existing] with { Label = name };
            context.Goals[existing] = named;
            return named;
        }

        var listed = new F009Goal(ownerEmail, name, item.Id, default, item.HabitName);
        context.Goals.Add(listed);
        return listed;
    }

    /// <summary>A goal named without its owner: the only goal of that name in the item, or the When actor's goal of that name.</summary>
    public async Task<F009Goal> GoalNamedAsync(string name)
    {
        var known = context.Goals.Where(goal => goal.Label == name).ToList();
        if (known.Count == 1)
        {
            return known[0];
        }

        var owner = context.WhenActor
            ?? context.Goals.Select(goal => goal.OwnerEmail).Distinct().SingleOrDefault()
            ?? throw new InvalidOperationException($"`{name}` names no goal of this item.");
        return await GoalNamedAsync(owner, name);
    }

    /// <summary>"the goal" (the actor's only goal), "the goal of `a`" or "`G`" (a goal of the actor by name).</summary>
    public async Task<F009Goal> GoalOfPhraseAsync(string? actorEmail, string phrase)
    {
        var text = phrase.Trim();
        var of = Regex.Match(text, "^the goal of `([^`]+)`$");
        if (of.Success)
        {
            return OnlyGoalOf(of.Groups[1].Value);
        }

        if (text == "the goal")
        {
            return actorEmail is null ? OnlyGoal() : OnlyGoalOf(actorEmail);
        }

        var named = Regex.Match(text, "^`([^`]+)`$");
        if (named.Success)
        {
            return actorEmail is null ? await GoalNamedAsync(named.Groups[1].Value) : await GoalNamedAsync(actorEmail, named.Groups[1].Value);
        }

        throw new InvalidOperationException($"\"{phrase}\" names no goal.");
    }

    /// <summary>The goal a Then step names: "the goal" (the item's only goal) or "`G`" (by name).</summary>
    public async Task<F009Goal> ThenGoalAsync(string phrase) =>
        phrase.Trim() == "the goal" ? OnlyGoal() : await GoalNamedAsync(phrase.Trim().Trim('`'));

    // ------------------------------------------------------------ use cases that set an item up (each must succeed)

    public static RecordInput Input(string value)
    {
        var (answer, amount) = F009Text.ValueOf(value);
        return new RecordInput { Answer = answer, Amount = amount, Note = null };
    }

    /// <summary>Slice 5's record use case for the goal, the date and the value, with no note; it must succeed.</summary>
    public async Task RecordSucceedsAsync(F009Goal goal, DateOnly date, string value)
    {
        var result = await driver.RecordAsync(await AccountAsync(goal.OwnerEmail), goal.Id, date, Input(value));
        Assert.True(
            result.Outcome == RecordWriteOutcome.Recorded,
            $"Recording {value} for {date:yyyy-MM-dd} gave {result.Outcome}: \"{result.Message}\".");
    }

    /// <summary>"the record of D": the only record dated D that the goal's history, read by its owner, gives now.</summary>
    public async Task<RecordId> RecordIdAsync(F009Goal goal, DateOnly date)
    {
        var history = await HistoryAsync(goal, date, date);
        return Assert.Single(history.Entries.SelectMany(entry => entry.Records), record => record.Date == date).Id;
    }

    /// <summary>FR-007's use case; it must give "goal ended".</summary>
    public async Task<GoalEndResult> EndSucceedsAsync(F009Goal goal, GoalOutcome outcome, string? note)
    {
        var result = await driver.EndGoalAsync(await AccountAsync(goal.OwnerEmail), goal.Id, outcome, note);
        Assert.True(result.Outcome == GoalEndOutcome.Ended, $"Ending the goal gave {result.Outcome}: \"{result.Message}\".");
        Assert.Equal(GoalChangeTexts.Ended, result.Message);
        return result;
    }

    /// <summary>FR-012's use case; it must give "attempt started".</summary>
    public async Task<AttemptStartResult> StartAttemptSucceedsAsync(F009Goal goal, F009Text.SettingsSpec settings, DateOnly startDate)
    {
        var result = await driver.StartAttemptAsync(
            await AccountAsync(goal.OwnerEmail), goal.Id, new NewAttemptInput { Target = settings.ToFields(), StartDate = startDate });
        Assert.True(result.Outcome == AttemptStartOutcome.Started, $"Starting a new attempt gave {result.Outcome}: \"{result.Message}\".");
        Assert.Equal(GoalChangeTexts.AttemptStarted, result.Message);
        return result;
    }

    /// <summary>
    /// "changes the amount to A minutes": FR-001's input with the goal's latest settings (those of its latest settings
    /// version, read now) with the amount A, and the name it has (its own name, or none).
    /// </summary>
    public async Task<GoalSettingsInput> AmountChangeAsync(F009Goal goal, int amount)
    {
        var read = await ReadGoalAsync(goal);
        var fields = read.Later is { } later
            ? new GoalTargetFields
            {
                BuildOrReduce = later.BuildOrReduce,
                Measurement = later.Measurement,
                Amount = amount,
                CountUnit = later.CountUnit,
                DurationUnit = later.DurationUnit,
                ScheduleKind = later.ScheduleKind,
                Days = later.Days,
                Times = later.Times,
                Period = later.Period,
            }
            : new GoalTargetFields
            {
                BuildOrReduce = read.BuildOrReduce,
                Measurement = read.Measurement,
                Amount = amount,
                CountUnit = read.CountUnit,
                DurationUnit = read.DurationUnit,
                ScheduleKind = read.ScheduleKind,
                Days = read.Days,
                Times = read.Times,
                Period = read.Period,
            };
        return new GoalSettingsInput { Target = fields, Name = read.HasOwnName ? read.Name : null };
    }

    /// <summary>"`x` has the ended Meditation goal" (acceptance.md, "Shared setups").</summary>
    public async Task<F009Goal> EndedMeditationGoalAsync(string email)
    {
        At(F009Driver.DefaultGoalCreation);
        var goal = await CreateGoalAsync(email, "Meditation, 10 min daily", F009Driver.DefaultStartDate);
        At("2026-11-14T08:00:00Z");
        foreach (var date in F009Text.Range(new DateOnly(2026, 11, 2), new DateOnly(2026, 11, 13)))
        {
            await RecordSucceedsAsync(goal, date, "10");
        }

        At("2026-11-16T10:00:00Z");
        await EndSucceedsAsync(goal, GoalOutcome.Abandoned, "Too busy this month.");
        return goal;
    }

    /// <summary>"`x` has the three goals" (acceptance.md, "Shared setups").</summary>
    public async Task ThreeGoalsAsync(string email)
    {
        At(F009Driver.DefaultGoalCreation);
        var reading = await CreateGoalAsync(email, "Reading, 15 min daily", F009Driver.DefaultStartDate);
        var meditation = await CreateGoalAsync(email, "Meditation, 10 min daily", F009Driver.DefaultStartDate);
        var sugar = await CreateGoalAsync(email, "Sugar, at most 1 sweet a day", F009Driver.DefaultStartDate);

        At("2026-11-14T08:00:00Z");
        foreach (var date in F009Text.Range(new DateOnly(2026, 11, 2), new DateOnly(2026, 11, 13)))
        {
            await RecordSucceedsAsync(reading, date, "15");
            await RecordSucceedsAsync(meditation, date, "10");
            await RecordSucceedsAsync(sugar, date, "0");
        }

        At("2026-11-14T09:00:00Z");
        await EndSucceedsAsync(sugar, GoalOutcome.Achieved, "Three weeks without extra sweets.");
        At("2026-11-16T09:00:00Z");
        await StartAttemptSucceedsAsync(sugar, F009Text.Settings("Reduce, count 1 `sweet`, 1 time per day"), new DateOnly(2026, 11, 16));
        At("2026-11-16T10:00:00Z");
        await EndSucceedsAsync(meditation, GoalOutcome.Abandoned, null);
    }

    // ------------------------------------------------------------ demo tasks and catalog

    /// <summary>"the AC database is reset": the reset entry point; the accounts and habits of the item are gone.</summary>
    public async Task ResetAsync()
    {
        await F009Driver.ResetDatabaseAsync();
        context.Accounts.Clear();
        context.CustomHabits.Clear();
        context.CatalogHabits.Clear();
        context.Goals.Clear();
    }

    /// <summary>"the catalog is seeded at T": reset, then demo loading at T with example2026.</summary>
    public async Task CatalogSeededAsync(string time)
    {
        await ResetAsync();
        At(time);
        context.DemoLoading = await driver.LoadDemoDataAsync(F009Text.DemoPassword);
        Assert.Equal(DemoLoadingOutcome.Loaded, context.DemoLoading.Outcome);
    }

    /// <summary>"the earlier demo tasks run at T": reset, then the four earlier demo tasks at T with example2026.</summary>
    public async Task EarlierDemoTasksAsync(string time)
    {
        await ResetAsync();
        At(time);
        context.DemoLoading = await driver.LoadDemoDataAsync(F009Text.DemoPassword);
        context.DemoHistory = await driver.LoadDemoHistoryAsync(F009Text.DemoPassword);
        context.DemoReport = await driver.LoadDemoReportAsync(F009Text.DemoPassword);
        context.DemoPause = await driver.LoadDemoPausesAsync(F009Text.DemoPassword);
        Assert.Equal(DemoLoadingOutcome.Loaded, context.DemoLoading.Outcome);
        Assert.Equal(DemoHistoryLoadingOutcome.Loaded, context.DemoHistory.Outcome);
        Assert.Equal(DemoReportLoadingOutcome.Loaded, context.DemoReport.Outcome);
        Assert.Equal(DemoPauseLoadingOutcome.Loaded, context.DemoPause.Outcome);
    }

    /// <summary>The results of demo goal-change loading as (email or none, text), in order.</summary>
    public static List<(string? Email, string Text)> Lines(DemoGoalChangeLoadingResult result)
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

    // ------------------------------------------------------------ reads (a read changes nothing)

    /// <summary>The account a Then step reads as: the When step's actor, or the owner of the item's goals.</summary>
    public string Reader() =>
        context.WhenActor
        ?? context.Goals.Select(goal => goal.OwnerEmail).Distinct().SingleOrDefault()
        ?? throw new InvalidOperationException("The item names no account to read as.");

    public async Task<GoalDetails> ReadGoalAsync(F009Goal goal, string? readerEmail = null)
    {
        var read = await driver.GetGoalAsync(await AccountAsync(readerEmail ?? goal.OwnerEmail), goal.Id);
        Assert.True(read.Outcome == GoalReadOutcome.Found, $"Reading the goal gave {read.Outcome}: \"{read.Message}\".");
        return read.Goal ?? throw new InvalidOperationException("Reading the goal gave no goal.");
    }

    public async Task<GoalHistory> HistoryAsync(F009Goal goal, DateOnly first, DateOnly last)
    {
        var read = await driver.GetHistoryAsync(await AccountAsync(goal.OwnerEmail), goal.Id, first, last);
        Assert.True(read.Outcome == HistoryOutcome.Found, $"Reading the history gave {read.Outcome}: \"{read.Message}\".");
        return read.History ?? throw new InvalidOperationException("Reading the history gave no history.");
    }

    public async Task<GoalHistory> RecordableAsync(F009Goal goal)
    {
        var read = await driver.GetRecordableAsync(await AccountAsync(goal.OwnerEmail), goal.Id);
        Assert.True(read.Outcome == RecordableOutcome.Found, $"Reading what can be recorded now gave {read.Outcome}: \"{read.Message}\".");
        return read.History ?? throw new InvalidOperationException("Reading what can be recorded now gave no history.");
    }

    public async Task<GoalReport> ReportAsync(F009Goal goal)
    {
        var result = await driver.GetGoalReportAsync(await AccountAsync(goal.OwnerEmail), goal.Id);
        Assert.True(result.Outcome == GoalReportOutcome.Found, $"Reading the report gave {result.Outcome}: \"{result.Message}\".");
        return result.Report ?? throw new InvalidOperationException("The goal report gave no report.");
    }

    public async Task<IReadOnlyList<RecentNote>> RecentNotesAsync(F009Goal goal)
    {
        var read = await driver.GetRecentNotesAsync(await AccountAsync(goal.OwnerEmail), goal.Id);
        Assert.True(read.Outcome == RecentNotesOutcome.Found, $"Reading the recent notes gave {read.Outcome}: \"{read.Message}\".");
        return read.Notes;
    }

    public async Task<IReadOnlyList<PauseView>> PausesAsync(F009Goal goal)
    {
        var read = await driver.GetPausesAsync(await AccountAsync(goal.OwnerEmail), goal.Id);
        Assert.True(read.Outcome == PausesOutcome.Found, $"Reading the pauses gave {read.Outcome}: \"{read.Message}\".");
        return read.Pauses;
    }

    public async Task<GoalsOverview> OverviewAsync(string email)
    {
        var result = await driver.GetGoalsOverviewAsync(await AccountAsync(email));
        Assert.True(result.Outcome == GoalsOverviewOutcome.Found, $"Reading the goals overview gave {result.Outcome}: \"{result.Message}\".");
        return result.Overview ?? throw new InvalidOperationException("The goals overview gave no overview.");
    }

    public async Task<Dashboard> DashboardAsync(string email)
    {
        var result = await driver.GetDashboardAsync(await AccountAsync(email));
        Assert.True(result.Outcome == DashboardOutcome.Found, $"Reading the dashboard gave {result.Outcome}: \"{result.Message}\".");
        return result.Dashboard ?? throw new InvalidOperationException("The dashboard gave no dashboard.");
    }

    public async Task<IReadOnlyList<AttemptView>> AttemptsAsync(F009Goal goal)
    {
        var result = await driver.GetAttemptsAsync(await AccountAsync(goal.OwnerEmail), goal.Id);
        Assert.True(result.Outcome == AttemptListOutcome.Found, $"Reading the attempts gave {result.Outcome}: \"{result.Message}\".");
        return result.Attempts;
    }

    public async Task<IReadOnlyList<EndedAttemptView>> GoalHistoryAsync(string email)
    {
        var result = await driver.GetGoalHistoryAsync(await AccountAsync(email));
        Assert.True(result.Outcome == GoalHistoryListOutcome.Found, $"Reading the goal history gave {result.Outcome}: \"{result.Message}\".");
        return result.Entries;
    }

    public async Task<NewAttemptValues> StartingValuesAsync(F009Goal goal)
    {
        var result = await driver.GetNewAttemptValuesAsync(await AccountAsync(goal.OwnerEmail), goal.Id);
        Assert.True(result.Outcome == NewAttemptValuesOutcome.Found, $"Reading the starting values gave {result.Outcome}: \"{result.Message}\".");
        return result.Values ?? throw new InvalidOperationException("Reading the starting values gave no values.");
    }
}
