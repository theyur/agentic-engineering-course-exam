using System.Text.RegularExpressions;
using HT.Application.Habits;
using HT.Application.Operator;
using HT.Domain.Accounts;
using HT.Domain.Habits;

namespace HT.AcceptanceTests.Support.F002;

public enum HabitKind
{
    Catalog,
    Custom,
}

/// <summary>A catalog or custom habit a step created, with the fields it was last saved with.</summary>
public sealed class HabitRecord
{
    public HabitRecord(HabitKind kind, HabitId id, string? ownerEmail, HabitSpec spec)
    {
        Kind = kind;
        Id = id;
        OwnerEmail = ownerEmail;
        Spec = spec;
        Names.Add(spec.NameCore ?? spec.Name ?? string.Empty);
    }

    public HabitKind Kind { get; }

    public HabitId Id { get; }

    /// <summary>The owner of a custom habit; null for a catalog habit.</summary>
    public string? OwnerEmail { get; }

    public HabitSpec Spec { get; private set; }

    /// <summary>Every name the habit had in this scenario, so a step can name it by any of them.</summary>
    public List<string> Names { get; } = [];

    /// <summary>An edit of this habit was saved with <paramref name="spec"/>.</summary>
    public void Accept(HabitSpec spec)
    {
        Spec = spec;
        Names.Add(spec.NameCore ?? spec.Name ?? string.Empty);
    }
}

/// <summary>The named outcome, the text and the position of a use-case result of a When step.</summary>
public sealed record RecordedResult(Enum Outcome, string? Message, int? Position, Enum SuccessOutcome, object Raw)
{
    public bool IsSuccess => Outcome.Equals(SuccessOutcome);
}

/// <summary>Per-scenario state shared by the F002 step definitions (Reqnroll context injection).</summary>
public sealed class F002Context
{
    /// <summary>Acting users from successful sign-ins, by email.</summary>
    public Dictionary<string, ActingUser> Accounts { get; } = new(StringComparer.Ordinal);

    /// <summary>The password each account of this scenario signs in with.</summary>
    public Dictionary<string, string> Passwords { get; } = new(StringComparer.Ordinal);

    /// <summary>"the administrator" of the item: the last administrator a Given step named.</summary>
    public string? CurrentAdministratorEmail { get; set; }

    /// <summary>"the registered user" of the item: the last registered user a Given step named.</summary>
    public string? CurrentRegisteredUserEmail { get; set; }

    /// <summary>The acting user of the last step that named one ("then edits it ...").</summary>
    public ActingUser? LastActor { get; set; }

    public List<HabitRecord> Habits { get; } = [];

    /// <summary>The habit "it" refers to: the last habit a step created or acted on.</summary>
    public HabitRecord? CurrentHabit { get; set; }

    /// <summary>The use-case results of the When steps of this scenario, in order.</summary>
    public List<RecordedResult> Results { get; } = [];

    public CatalogBrowseResult? LastBrowse { get; set; }

    public CatalogHabitDetailsResult? LastDetails { get; set; }

    public CategoryFilterResult? LastCategoryFilter { get; set; }

    public HomePreviewResult? LastHomePreview { get; set; }

    public CatalogHabitListResult? LastCatalogList { get; set; }

    public CatalogHabitAdminResult? LastCatalogRead { get; set; }

    /// <summary>The suggested starting goals the last edit step sent.</summary>
    public IReadOnlyList<GoalSpec>? LastAttemptedGoals { get; set; }

    public DemoLoadingResult? LastDemoLoading { get; set; }

    public string? LastDemoPassword { get; set; }

    /// <summary>The administrator's list a Given step remembered (AC-061).</summary>
    public CatalogHabitListResult? RememberedCatalogList { get; set; }

    /// <summary>The identifiers of the demo custom habits a Then step listed (AC-064).</summary>
    public List<HabitId> DemoCustomHabitIds { get; } = [];

    public RecordedResult LastResult =>
        Results.Count > 0 ? Results[^1] : throw new InvalidOperationException("No use case ran in a When step of this scenario.");

    public ActingUser Administrator =>
        Account(CurrentAdministratorEmail ?? throw new InvalidOperationException("No administrator was given in this scenario."));

    public ActingUser RegisteredUser =>
        Account(CurrentRegisteredUserEmail ?? throw new InvalidOperationException("No registered user was given in this scenario."));

    public ActingUser Account(string email) =>
        Accounts.TryGetValue(email, out var user)
            ? user
            : throw new InvalidOperationException($"No account `{email}` signed in in this scenario.");

    /// <summary>
    /// The acting user of a phrase such as "the anonymous visitor", "the administrator", "the registered user `x`".
    /// "the administrator" and "the registered user" without an email are the item's.
    /// </summary>
    public (ActingUser User, string? Email) ResolveActor(string phrase)
    {
        if (phrase == "the anonymous visitor")
        {
            LastActor = ActingUser.Anonymous;
            return (ActingUser.Anonymous, null);
        }

        var match = Regex.Match(phrase, "^the (administrator|registered user)(?: `([^`]+)`)?$");
        if (!match.Success)
        {
            throw new InvalidOperationException($"\"{phrase}\" is not an acting user.");
        }

        var email = match.Groups[2].Success
            ? match.Groups[2].Value
            : match.Groups[1].Value == "administrator"
                ? CurrentAdministratorEmail ?? throw new InvalidOperationException("No administrator was given in this scenario.")
                : CurrentRegisteredUserEmail ?? throw new InvalidOperationException("No registered user was given in this scenario.");
        var user = Account(email);
        LastActor = user;
        return (user, email);
    }

    /// <summary>The habit a step names, optionally only of one kind or one owner.</summary>
    public HabitRecord Habit(string name, HabitKind? kind = null, string? ownerEmail = null)
    {
        var matches = Habits
            .Where(habit => habit.Names.Contains(name, StringComparer.Ordinal))
            .Where(habit => kind is null || habit.Kind == kind)
            .Where(habit => ownerEmail is null || habit.OwnerEmail == ownerEmail)
            .ToList();
        return matches.Count == 1
            ? matches[0]
            : throw new InvalidOperationException($"{matches.Count} habits of this scenario are named `{name}`.");
    }

    public HabitRecord CurrentHabitOrThrow =>
        CurrentHabit ?? throw new InvalidOperationException("No habit was named before \"it\" in this scenario.");

    public void Record(CatalogHabitListResult result)
    {
        LastCatalogList = result;
        Add(result.Outcome, result.Message, null, CatalogHabitListOutcome.Found, result);
    }

    public void Record(CatalogHabitAdminResult result)
    {
        LastCatalogRead = result;
        Add(result.Outcome, result.Message, null, CatalogHabitAdminOutcome.Found, result);
    }

    public void Record(CatalogHabitCreateResult result) =>
        Add(result.Outcome, result.Message, result.Position, CatalogHabitCreateOutcome.Created, result);

    public void Record(CatalogHabitEditResult result) =>
        Add(result.Outcome, result.Message, result.Position, CatalogHabitEditOutcome.Saved, result);

    public void Record(CatalogHabitPublishResult result) =>
        Add(result.Outcome, result.Message, null, CatalogHabitPublishOutcome.Published, result);

    public void Record(CatalogHabitUnpublishResult result) =>
        Add(result.Outcome, result.Message, null, CatalogHabitUnpublishOutcome.Unpublished, result);

    public void Record(CatalogHabitDetailsResult result)
    {
        LastDetails = result;
        Add(result.Outcome, result.Message, null, CatalogHabitDetailsOutcome.Found, result);
    }

    public void Record(CustomHabitListResult result) =>
        Add(result.Outcome, result.Message, null, CustomHabitListOutcome.Found, result);

    public void Record(CustomHabitResult result) =>
        Add(result.Outcome, result.Message, null, CustomHabitOutcome.Found, result);

    public void Record(CustomHabitCreateResult result) =>
        Add(result.Outcome, result.Message, null, CustomHabitCreateOutcome.Created, result);

    public void Record(CustomHabitEditResult result) =>
        Add(result.Outcome, result.Message, null, CustomHabitEditOutcome.Saved, result);

    public void Record(CustomHabitDeleteResult result) =>
        Add(result.Outcome, result.Message, null, CustomHabitDeleteOutcome.Deleted, result);

    public void Record(DemoLoadingResult result)
    {
        LastDemoLoading = result;
        Add(result.Outcome, result.Message, null, DemoLoadingOutcome.Loaded, result);
    }

    private void Add(Enum outcome, string? message, int? position, Enum success, object raw) =>
        Results.Add(new RecordedResult(outcome, message, position, success, raw));
}
