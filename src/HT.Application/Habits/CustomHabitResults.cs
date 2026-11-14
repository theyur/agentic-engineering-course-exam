using HT.Domain.Habits;

namespace HT.Application.Habits;

/// <summary>A custom habit as its owner reads it. It carries no owner (contract, "View types").</summary>
public sealed record CustomHabitView(HabitId Id, string Name, string? Description, Category Category, Intent Intent, string Icon);

/// <summary>The outcomes of <see cref="CustomHabitService.ListAsync"/>, in FR-016's check order.</summary>
public enum CustomHabitListOutcome
{
    NotSignedIn,
    NotForAdministrators,
    Found,
}

/// <summary>The acting user's own custom habits in name order.</summary>
public sealed class CustomHabitListResult
{
    private CustomHabitListResult(CustomHabitListOutcome outcome, string? message, IReadOnlyList<CustomHabitView> habits)
    {
        Outcome = outcome;
        Message = message;
        Habits = habits;
    }

    public CustomHabitListOutcome Outcome { get; }

    public string? Message { get; }

    /// <summary>The habits; empty for a refusal.</summary>
    public IReadOnlyList<CustomHabitView> Habits { get; }

    internal static CustomHabitListResult Refused(CustomHabitListOutcome outcome, string message) => new(outcome, message, []);

    internal static CustomHabitListResult Found(IReadOnlyList<CustomHabitView> habits) => new(CustomHabitListOutcome.Found, null, habits);
}

/// <summary>The outcomes of <see cref="CustomHabitService.GetAsync"/>, in FR-016's check order.</summary>
public enum CustomHabitOutcome
{
    NotSignedIn,
    NotForAdministrators,
    NotFound,
    Found,
}

/// <summary>One of the acting user's own custom habits.</summary>
public sealed class CustomHabitResult
{
    private CustomHabitResult(CustomHabitOutcome outcome, string? message, CustomHabitView? habit)
    {
        Outcome = outcome;
        Message = message;
        Habit = habit;
    }

    public CustomHabitOutcome Outcome { get; }

    public string? Message { get; }

    /// <summary>Set only for <see cref="CustomHabitOutcome.Found"/>.</summary>
    public CustomHabitView? Habit { get; }

    internal static CustomHabitResult Refused(CustomHabitOutcome outcome, string message) => new(outcome, message, null);

    internal static CustomHabitResult Found(CustomHabitView habit) => new(CustomHabitOutcome.Found, null, habit);
}

/// <summary>The outcomes of <see cref="CustomHabitService.CreateAsync"/>, in FR-016's check order.</summary>
public enum CustomHabitCreateOutcome
{
    NotSignedIn,
    NotForAdministrators,
    NameMissing,
    NameTooLong,
    DescriptionTooLong,
    CategoryMissing,
    IntentMissing,
    IconMissing,
    NameTaken,
    Created,
}

/// <summary>The result of creating a custom habit.</summary>
public sealed class CustomHabitCreateResult
{
    private CustomHabitCreateResult(CustomHabitCreateOutcome outcome, string? message, HabitId? habitId)
    {
        Outcome = outcome;
        Message = message;
        HabitId = habitId;
    }

    public CustomHabitCreateOutcome Outcome { get; }

    public string? Message { get; }

    /// <summary>Set only for <see cref="CustomHabitCreateOutcome.Created"/>.</summary>
    public HabitId? HabitId { get; }

    internal static CustomHabitCreateResult Refused(CustomHabitCreateOutcome outcome, string message) => new(outcome, message, null);

    internal static CustomHabitCreateResult Created(HabitId id) => new(CustomHabitCreateOutcome.Created, null, id);
}

/// <summary>The outcomes of <see cref="CustomHabitService.EditAsync"/>, in FR-016's check order.</summary>
public enum CustomHabitEditOutcome
{
    NotSignedIn,
    NotForAdministrators,
    NotFound,
    NameMissing,
    NameTooLong,
    DescriptionTooLong,
    CategoryMissing,
    IntentMissing,
    IconMissing,
    NameTaken,
    Saved,
}

/// <summary>The result of editing a custom habit.</summary>
public sealed class CustomHabitEditResult
{
    private CustomHabitEditResult(CustomHabitEditOutcome outcome, string? message)
    {
        Outcome = outcome;
        Message = message;
    }

    public CustomHabitEditOutcome Outcome { get; }

    public string? Message { get; }

    internal static CustomHabitEditResult Refused(CustomHabitEditOutcome outcome, string message) => new(outcome, message);

    internal static CustomHabitEditResult Saved { get; } = new(CustomHabitEditOutcome.Saved, null);
}

/// <summary>The outcomes of <see cref="CustomHabitService.DeleteAsync"/>, in FR-016's check order.</summary>
public enum CustomHabitDeleteOutcome
{
    NotSignedIn,
    NotForAdministrators,
    NotFound,
    Deleted,
}

/// <summary>The result of deleting a custom habit for good.</summary>
public sealed class CustomHabitDeleteResult
{
    private CustomHabitDeleteResult(CustomHabitDeleteOutcome outcome, string? message)
    {
        Outcome = outcome;
        Message = message;
    }

    public CustomHabitDeleteOutcome Outcome { get; }

    public string? Message { get; }

    internal static CustomHabitDeleteResult Refused(CustomHabitDeleteOutcome outcome, string message) => new(outcome, message);

    internal static CustomHabitDeleteResult Deleted { get; } = new(CustomHabitDeleteOutcome.Deleted, null);
}
