using System.Globalization;
using HT.Application.Accounts;
using HT.Application.Goals;
using HT.Application.Habits;
using HT.Application.Operator;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;

namespace HT.AcceptanceTests.Support.F004;

/// <summary>A catalog habit a step created, with every name it had in the scenario.</summary>
public sealed class CatalogRecord(HabitId id, CatalogSpec spec)
{
    public HabitId Id { get; } = id;

    public CatalogSpec Spec { get; } = spec;

    public List<string> Names { get; } = [spec.Name];
}

/// <summary>A custom habit a step created, with its owner and every name it had in the scenario.</summary>
public sealed class CustomRecord(string ownerEmail, HabitId id, CustomSpec spec)
{
    public string OwnerEmail { get; } = ownerEmail;

    public HabitId Id { get; } = id;

    public CustomSpec Spec { get; } = spec;

    public List<string> Names { get; } = [spec.Name];
}

/// <summary>A goal a step created, with its owner and the alias a step gave it ("G").</summary>
public sealed record GoalRecord(string OwnerEmail, GoalId Id, string? Alias);

/// <summary>The named outcome and the text of one use-case result of a When step.</summary>
public sealed record RecordedResult(Enum Outcome, string? Message, bool IsSuccess, object Raw);

/// <summary>Per-scenario state shared by the F004 step definitions (Reqnroll context injection).</summary>
public sealed class F004Context
{
    /// <summary>Acting users from successful sign-ins, by email.</summary>
    public Dictionary<string, ActingUser> Accounts { get; } = new(StringComparer.Ordinal);

    /// <summary>The time zone each account of this scenario has, as far as the steps set it.</summary>
    public Dictionary<string, string> TimeZones { get; } = new(StringComparer.Ordinal);

    /// <summary>"the administrator" of the item: the last administrator a Given step named.</summary>
    public string? CurrentAdministratorEmail { get; set; }

    /// <summary>"the registered user" of the item: the last registered user a Given step named.</summary>
    public string? CurrentRegisteredUserEmail { get; set; }

    /// <summary>The latest time an item gave to a step so far (acceptance.md, "Time").</summary>
    public DateTimeOffset LatestTime { get; set; } = F004Driver.DefaultNow;

    public List<CatalogRecord> CatalogHabits { get; } = [];

    public List<CustomRecord> CustomHabits { get; } = [];

    /// <summary>Every goal a step created successfully, in creation order.</summary>
    public List<GoalRecord> Goals { get; } = [];

    /// <summary>The goal-creation results of the When step, in order.</summary>
    public List<GoalCreateResult> WhenCreations { get; } = [];

    /// <summary>The use-case results of the When steps of this scenario, in order.</summary>
    public List<RecordedResult> Results { get; } = [];

    /// <summary>The starting values each account got last, by email.</summary>
    public Dictionary<string, GoalStartingValues> StartingValues { get; } = new(StringComparer.Ordinal);

    public GoalStartResult? LastStart { get; set; }

    public GoalCreateResult? LastCreate { get; set; }

    /// <summary>The name the last goal creation sent.</summary>
    public string? LastSentGoalName { get; set; }

    public GoalListResult? LastList { get; set; }

    public string? LastListEmail { get; set; }

    public GoalReadResult? LastRead { get; set; }

    public GoalCalendarResult? LastCalendar { get; set; }

    /// <summary>The next calendar entry an "in this order:" line checks.</summary>
    public int CalendarCursor { get; set; }

    /// <summary>The next list entry an "in this order:" line checks.</summary>
    public int ListCursor { get; set; }

    public CatalogBrowseResult? LastBrowse { get; set; }

    public CatalogHabitDetailsResult? LastDetails { get; set; }

    public CatalogHabitUnpublishResult? LastUnpublish { get; set; }

    /// <summary>The catalog habit the When step unpublished ("it").</summary>
    public HabitId? LastUnpublishedHabitId { get; set; }

    public CustomHabitDeleteResult? LastDelete { get; set; }

    public Dictionary<string, CustomHabitResult> CustomHabitReads { get; } = new(StringComparer.Ordinal);

    public DemoLoadingResult? LastDemoLoading { get; set; }

    /// <summary>Each demo account's goal names before demo loading ran again (AC-065).</summary>
    public Dictionary<string, List<string>> DemoGoalNamesBefore { get; } = new(StringComparer.Ordinal);

    /// <summary>The goals each demo account listed and read in the When step (AC-069), by email.</summary>
    public Dictionary<string, List<GoalDetails>> DemoGoalDetails { get; } = new(StringComparer.Ordinal);

    public Dictionary<string, ProfileReadResult> DemoProfiles { get; } = new(StringComparer.Ordinal);

    public HomeHeadingResult? LastHomeHeading { get; set; }

    public List<RegistrationResult> Registrations { get; } = [];

    public List<TimeZoneReadResult> TimeZoneReads { get; } = [];

    public List<TimeZoneChangeResult> TimeZoneChanges { get; } = [];

    public List<NameChangeResult> NameChanges { get; } = [];

    public List<ProfileReadResult> ProfileReads { get; } = [];

    /// <summary>The culture a step set for the use cases of this scenario (AC-088).</summary>
    public CultureInfo? Culture { get; set; }

    /// <summary>The (start, create) results of AC-020, one per suggested starting goal.</summary>
    public List<(GoalStartResult Start, GoalCreateResult? Create)> SeedCreations { get; } = [];

    public RecordedResult LastResult =>
        Results.Count > 0 ? Results[^1] : throw new InvalidOperationException("No use case ran in a When step of this scenario.");

    public ActingUser Administrator =>
        Account(CurrentAdministratorEmail ?? throw new InvalidOperationException("No administrator was given in this scenario."));

    public ActingUser Account(string email) =>
        Accounts.TryGetValue(email, out var user)
            ? user
            : throw new InvalidOperationException($"No account `{email}` signed in in this scenario.");

    /// <summary>"the anonymous visitor" or a backticked email.</summary>
    public ActingUser Actor(string token) =>
        token.Trim() == F004Text.AnonymousVisitor ? ActingUser.Anonymous : Account(F004Text.Email(token));

    /// <summary>The email of an actor token, or null for the anonymous visitor.</summary>
    public static string? ActorEmail(string token) =>
        token.Trim() == F004Text.AnonymousVisitor ? null : F004Text.Email(token);

    public CatalogRecord CatalogHabit(string name)
    {
        var matches = CatalogHabits.Where(habit => habit.Names.Contains(name, StringComparer.Ordinal)).ToList();
        return matches.Count == 1
            ? matches[0]
            : throw new InvalidOperationException($"{matches.Count} catalog habits of this scenario are named `{name}`.");
    }

    public CustomRecord CustomHabit(string ownerEmail, string name)
    {
        var matches = CustomHabits
            .Where(habit => habit.OwnerEmail == ownerEmail && habit.Names.Contains(name, StringComparer.Ordinal))
            .ToList();
        return matches.Count == 1
            ? matches[0]
            : throw new InvalidOperationException($"`{ownerEmail}` has {matches.Count} custom habits named `{name}` in this scenario.");
    }

    /// <summary>"the goal": the goal the scenario created last.</summary>
    public GoalRecord CurrentGoal =>
        Goals.Count > 0 ? Goals[^1] : throw new InvalidOperationException("No goal was created in this scenario.");

    /// <summary>The goal an account created last ("their goal", "`x`'s goal").</summary>
    public GoalRecord GoalOf(string ownerEmail) =>
        Goals.LastOrDefault(goal => goal.OwnerEmail == ownerEmail)
        ?? throw new InvalidOperationException($"`{ownerEmail}` created no goal in this scenario.");

    public GoalRecord GoalNamed(string alias) =>
        Goals.SingleOrDefault(goal => goal.Alias == alias)
        ?? throw new InvalidOperationException($"No goal of this scenario is called {alias}.");

    public void Record(Enum outcome, string? message, bool isSuccess, object raw) =>
        Results.Add(new RecordedResult(outcome, message, isSuccess, raw));

    public void Record(GoalStartResult result)
    {
        LastStart = result;
        Record(result.Outcome, result.Message, result.Outcome == GoalStartOutcome.Started, result);
    }

    public void Record(GoalCreateResult result)
    {
        LastCreate = result;
        WhenCreations.Add(result);
        Record(result.Outcome, result.Message, result.Outcome == GoalCreateOutcome.Created, result);
    }

    public void Record(GoalListResult result, string? email)
    {
        LastList = result;
        LastListEmail = email;
        ListCursor = 0;
        Record(result.Outcome, result.Message, result.Outcome == GoalListOutcome.Found, result);
    }

    /// <summary>The goal identifier the last read of a When step asked for.</summary>
    public GoalId? LastReadGoalId { get; private set; }

    public void Record(GoalReadResult result, GoalId requested)
    {
        LastRead = result;
        LastReadGoalId = requested;
        Record(result.Outcome, result.Message, result.Outcome == GoalReadOutcome.Found, result);
    }

    public void Record(GoalCalendarResult result)
    {
        LastCalendar = result;
        CalendarCursor = 0;
        Record(result.Outcome, result.Message, result.Outcome == GoalCalendarOutcome.Found, result);
    }

    public void Record(CatalogBrowseResult result)
    {
        LastBrowse = result;
        Record(result.Outcome, result.Message, result.Outcome == CatalogBrowseOutcome.Found, result);
    }

    public void Record(CatalogHabitDetailsResult result)
    {
        LastDetails = result;
        Record(result.Outcome, result.Message, result.Outcome == CatalogHabitDetailsOutcome.Found, result);
    }

    public void Record(CatalogHabitUnpublishResult result)
    {
        LastUnpublish = result;
        Record(result.Outcome, result.Message, result.Outcome == CatalogHabitUnpublishOutcome.Unpublished, result);
    }

    public void Record(CatalogHabitEditResult result) =>
        Record(result.Outcome, result.Message, result.Outcome == CatalogHabitEditOutcome.Saved, result);

    public void Record(CustomHabitDeleteResult result)
    {
        LastDelete = result;
        Record(result.Outcome, result.Message, result.Outcome == CustomHabitDeleteOutcome.Deleted, result);
    }

    public void Record(CustomHabitEditResult result) =>
        Record(result.Outcome, result.Message, result.Outcome == CustomHabitEditOutcome.Saved, result);

    public void Record(CustomHabitResult result) =>
        Record(result.Outcome, result.Message, result.Outcome == CustomHabitOutcome.Found, result);

    public void Record(TimeZoneReadResult result)
    {
        TimeZoneReads.Add(result);
        Record(result.Outcome, result.Message, result.Outcome == TimeZoneReadOutcome.Found, result);
    }

    public void Record(TimeZoneChangeResult result)
    {
        TimeZoneChanges.Add(result);
        Record(
            result.Outcome,
            result.Message,
            result.Outcome is TimeZoneChangeOutcome.Changed or TimeZoneChangeOutcome.Unchanged,
            result);
    }

    public void Record(NameChangeResult result)
    {
        NameChanges.Add(result);
        Record(result.Outcome, result.Message, result.Outcome == NameChangeOutcome.Changed, result);
    }

    public void Record(ProfileReadResult result)
    {
        ProfileReads.Add(result);
        Record(result.Outcome, result.Message, result.Outcome == ProfileReadOutcome.Found, result);
    }

    public void Record(HomeHeadingResult result)
    {
        LastHomeHeading = result;
        Record(result.Outcome, result.Message, result.Outcome == HomeHeadingOutcome.Found, result);
    }

    public void Record(RegistrationResult result)
    {
        Registrations.Add(result);
        Record(result.Outcome, result.Message, result.Outcome == RegistrationOutcome.Registered, result);
    }

    public void Record(DemoLoadingResult result)
    {
        LastDemoLoading = result;
        Record(result.Outcome, result.Message, result.Outcome == DemoLoadingOutcome.Loaded, result);
    }
}
