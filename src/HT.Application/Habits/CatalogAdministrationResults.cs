using HT.Domain.Habits;

namespace HT.Application.Habits;

/// <summary>A catalog habit in the administrator's list (FR-012): its name, category, intent and state.</summary>
public sealed record CatalogHabitListItem(HabitId Id, string Name, Category Category, Intent Intent, CatalogHabitState State);

/// <summary>Any catalog habit as the administrator reads it: the details' members and its state (FR-012).</summary>
public sealed record CatalogHabitAdminDetails(
    HabitId Id,
    string Name,
    Category Category,
    Intent Intent,
    string Icon,
    string? Description,
    SuggestedStartingGoalView? MarkedSuggestedStartingGoal,
    IReadOnlyList<SuggestedStartingGoalView> SuggestedStartingGoals,
    CatalogHabitState State);

/// <summary>The outcomes of <see cref="CatalogAdministrationService.ListAsync"/>, in FR-016's check order.</summary>
public enum CatalogHabitListOutcome
{
    NotSignedIn,
    NotAdministrator,
    Found,
}

/// <summary>The administrator's list: every catalog habit that matches the search, in name order, and the total.</summary>
public sealed class CatalogHabitListResult
{
    private CatalogHabitListResult(CatalogHabitListOutcome outcome, string? message, IReadOnlyList<CatalogHabitListItem> habits, int total)
    {
        Outcome = outcome;
        Message = message;
        Habits = habits;
        Total = total;
    }

    public CatalogHabitListOutcome Outcome { get; }

    public string? Message { get; }

    /// <summary>The listed habits; empty for a refusal.</summary>
    public IReadOnlyList<CatalogHabitListItem> Habits { get; }

    /// <summary>The number of all catalog habits, whatever the search; 0 for a refusal.</summary>
    public int Total { get; }

    internal static CatalogHabitListResult Refused(CatalogHabitListOutcome outcome, string message) => new(outcome, message, [], 0);

    internal static CatalogHabitListResult Found(IReadOnlyList<CatalogHabitListItem> habits, int total) =>
        new(CatalogHabitListOutcome.Found, null, habits, total);
}

/// <summary>The outcomes of <see cref="CatalogAdministrationService.GetAsync"/>, in FR-016's check order.</summary>
public enum CatalogHabitAdminOutcome
{
    NotSignedIn,
    NotAdministrator,
    NotFound,
    Found,
}

/// <summary>The result of the administrator's read of one catalog habit in any state.</summary>
public sealed class CatalogHabitAdminResult
{
    private CatalogHabitAdminResult(CatalogHabitAdminOutcome outcome, string? message, CatalogHabitAdminDetails? habit)
    {
        Outcome = outcome;
        Message = message;
        Habit = habit;
    }

    public CatalogHabitAdminOutcome Outcome { get; }

    public string? Message { get; }

    /// <summary>Set only for <see cref="CatalogHabitAdminOutcome.Found"/>.</summary>
    public CatalogHabitAdminDetails? Habit { get; }

    internal static CatalogHabitAdminResult Refused(CatalogHabitAdminOutcome outcome, string message) => new(outcome, message, null);

    internal static CatalogHabitAdminResult Found(CatalogHabitAdminDetails habit) => new(CatalogHabitAdminOutcome.Found, null, habit);
}

/// <summary>The outcomes of <see cref="CatalogAdministrationService.CreateAsync"/>, in FR-016's check order.</summary>
public enum CatalogHabitCreateOutcome
{
    NotSignedIn,
    NotAdministrator,
    NameMissing,
    NameTooLong,
    DescriptionTooLong,
    CategoryMissing,
    IntentMissing,
    IconMissing,
    TooManySuggestedStartingGoals,
    SuggestedStartingGoalNotMarked,
    BuildOrReduceMissing,
    MeasurementMissing,
    YesNoHasAmountOrUnit,
    BuildAmountOutOfRange,
    ReduceAmountOutOfRange,
    CountUnitMissing,
    CountUnitTooLong,
    DurationUnitMissing,
    ScheduleMissing,
    DaysMissing,
    TimesOutOfRange,
    PeriodMissing,
    NameTaken,
    Created,
}

/// <summary>The result of creating a catalog habit; a created habit is a Draft.</summary>
public sealed class CatalogHabitCreateResult
{
    private CatalogHabitCreateResult(CatalogHabitCreateOutcome outcome, string? message, int? position, HabitId? habitId)
    {
        Outcome = outcome;
        Message = message;
        Position = position;
        HabitId = habitId;
    }

    public CatalogHabitCreateOutcome Outcome { get; }

    public string? Message { get; }

    /// <summary>The position (from 1) of the suggested starting goal a refusal is about; null otherwise.</summary>
    public int? Position { get; }

    /// <summary>Set only for <see cref="CatalogHabitCreateOutcome.Created"/>.</summary>
    public HabitId? HabitId { get; }

    internal static CatalogHabitCreateResult Refused(CatalogHabitCreateOutcome outcome, string message, int? position = null) =>
        new(outcome, message, position, null);

    internal static CatalogHabitCreateResult Created(HabitId id) => new(CatalogHabitCreateOutcome.Created, null, null, id);
}

/// <summary>The outcomes of <see cref="CatalogAdministrationService.EditAsync"/>, in FR-016's check order.</summary>
public enum CatalogHabitEditOutcome
{
    NotSignedIn,
    NotAdministrator,
    NotFound,
    NameMissing,
    NameTooLong,
    DescriptionTooLong,
    CategoryMissing,
    IntentMissing,
    IconMissing,
    TooManySuggestedStartingGoals,
    SuggestedStartingGoalNotMarked,
    BuildOrReduceMissing,
    MeasurementMissing,
    YesNoHasAmountOrUnit,
    BuildAmountOutOfRange,
    ReduceAmountOutOfRange,
    CountUnitMissing,
    CountUnitTooLong,
    DurationUnitMissing,
    ScheduleMissing,
    DaysMissing,
    TimesOutOfRange,
    PeriodMissing,
    NameTaken,
    Saved,
}

/// <summary>The result of editing a catalog habit; an edit never changes the state (FR-012).</summary>
public sealed class CatalogHabitEditResult
{
    private CatalogHabitEditResult(CatalogHabitEditOutcome outcome, string? message, int? position)
    {
        Outcome = outcome;
        Message = message;
        Position = position;
    }

    public CatalogHabitEditOutcome Outcome { get; }

    public string? Message { get; }

    /// <summary>The position (from 1) of the suggested starting goal a refusal is about; null otherwise.</summary>
    public int? Position { get; }

    internal static CatalogHabitEditResult Refused(CatalogHabitEditOutcome outcome, string message, int? position = null) =>
        new(outcome, message, position);

    internal static CatalogHabitEditResult Saved { get; } = new(CatalogHabitEditOutcome.Saved, null, null);
}

/// <summary>The outcomes of <see cref="CatalogAdministrationService.PublishAsync"/>, in FR-016's check order.</summary>
public enum CatalogHabitPublishOutcome
{
    NotSignedIn,
    NotAdministrator,
    NotFound,
    Published,
}

/// <summary>The result of publishing; a Published habit stays Published with this success (FR-011).</summary>
public sealed class CatalogHabitPublishResult
{
    private CatalogHabitPublishResult(CatalogHabitPublishOutcome outcome, string? message)
    {
        Outcome = outcome;
        Message = message;
    }

    public CatalogHabitPublishOutcome Outcome { get; }

    public string? Message { get; }

    internal static CatalogHabitPublishResult Refused(CatalogHabitPublishOutcome outcome, string message) => new(outcome, message);

    internal static CatalogHabitPublishResult Published { get; } = new(CatalogHabitPublishOutcome.Published, null);
}

/// <summary>The outcomes of <see cref="CatalogAdministrationService.UnpublishAsync"/>, in FR-016's check order.</summary>
public enum CatalogHabitUnpublishOutcome
{
    NotSignedIn,
    NotAdministrator,
    NotFound,
    NotPublished,
    Unpublished,
}

/// <summary>The result of unpublishing; an Unpublished habit stays Unpublished with this success, a Draft is refused (FR-011).</summary>
public sealed class CatalogHabitUnpublishResult
{
    private CatalogHabitUnpublishResult(CatalogHabitUnpublishOutcome outcome, string? message)
    {
        Outcome = outcome;
        Message = message;
    }

    public CatalogHabitUnpublishOutcome Outcome { get; }

    public string? Message { get; }

    internal static CatalogHabitUnpublishResult Refused(CatalogHabitUnpublishOutcome outcome, string message) => new(outcome, message);

    internal static CatalogHabitUnpublishResult Unpublished { get; } = new(CatalogHabitUnpublishOutcome.Unpublished, null);
}
