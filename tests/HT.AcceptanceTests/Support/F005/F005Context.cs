using HT.Application.Goals;
using HT.Application.Operator;
using HT.Application.Records;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;

namespace HT.AcceptanceTests.Support.F005;

/// <summary>A goal a step created (or a demo goal a step named), with its owner and its schedule kind.</summary>
public sealed record F005Goal(string OwnerEmail, GoalId Id, GoalScheduleKind Kind);

/// <summary>The use cases of this slice (and slice 4's evaluation calendar) whose results the Then steps check.</summary>
public enum F005Kind
{
    Record,
    Change,
    Delete,
    SaveNote,
    History,
    Recordable,
    RecentNotes,
    Calendar,
}

/// <summary>One use-case result of a When step: its named outcome, its text and the result itself.</summary>
public sealed record F005Result(F005Kind Kind, Enum Outcome, string? Message, bool IsSuccess, object Raw, F005Goal? Goal);

/// <summary>How the "And" lines after "it gives N entries, in this order:" and similar steps are checked.</summary>
public enum F005ListMode
{
    None,
    OrderedEntries,
    EntriesByDate,
    OrderedNotes,
}

/// <summary>Per-scenario state shared by the F005 step definitions (Reqnroll context injection).</summary>
public sealed class F005Context
{
    /// <summary>Acting users from successful sign-ins, by email.</summary>
    public Dictionary<string, ActingUser> Accounts { get; } = new(StringComparer.Ordinal);

    public Dictionary<(string Owner, string Name), HabitId> CustomHabits { get; } = new();

    public Dictionary<string, HabitId> CatalogHabits { get; } = new(StringComparer.Ordinal);

    /// <summary>Every goal a step created, in creation order.</summary>
    public List<F005Goal> Goals { get; } = [];

    /// <summary>Demo goals named by a step, by "email|name".</summary>
    public Dictionary<string, F005Goal> DemoGoals { get; } = new(StringComparer.Ordinal);

    // ------------------------------------------------------------ time (acceptance.md, "Time")

    /// <summary>The latest time an item gave to a step so far.</summary>
    public DateTimeOffset LatestTime { get; set; } = F005Driver.DefaultNow;

    /// <summary>The time the item's When step ran at; Then steps run at it.</summary>
    public DateTimeOffset? WhenTime { get; set; }

    /// <summary>Whether the time of account and habit setup is known yet (the first other step fixes it).</summary>
    public bool SetupResolved { get; set; }

    /// <summary>The time account and habit setup that gives no time runs at.</summary>
    public DateTimeOffset SetupTime { get; set; } = F005Driver.DefaultNow;

    /// <summary>Setup written before the first other step; it runs once that step fixes the setup time.</summary>
    public List<Func<Task>> PendingSetup { get; } = [];

    // ------------------------------------------------------------ what the steps did

    /// <summary>Every date the item's goal and recording steps name ("over the item's dates").</summary>
    public SortedSet<DateOnly> MentionedDates { get; } = [];

    /// <summary>How many records the Given steps made, by goal.</summary>
    public Dictionary<GoalId, int> GivenRecords { get; } = new();

    /// <summary>The dates the Given steps saved notes for, by goal.</summary>
    public Dictionary<GoalId, HashSet<DateOnly>> GivenNoteDates { get; } = new();

    /// <summary>Goals for which "no other use case is called" until the When step, with the calls made then.</summary>
    public List<(GoalId Goal, int Calls)> CallsFrozenUntilWhen { get; } = [];

    /// <summary>The use-case results of the When step, in order.</summary>
    public List<F005Result> Results { get; } = [];

    /// <summary>The last read (history, recordable now, recent notes, calendar) of the When step.</summary>
    public F005Result? LastRead { get; set; }

    /// <summary>Recent-notes results of the When step by the goal they were read for ("first goal", demo goal name).</summary>
    public Dictionary<string, RecentNotesResult> RecentNotes { get; } = new(StringComparer.Ordinal);

    public DemoLoadingResult? LastDemoLoading { get; set; }

    public DemoHistoryLoadingResult? LastDemoHistory { get; set; }

    // ------------------------------------------------------------ lines after a list header

    public F005ListMode ListMode { get; set; }

    public int ListCursor { get; set; }

    public F005Result LastResult =>
        Results.Count > 0 ? Results[^1] : throw new InvalidOperationException("No use case ran in the When step of this scenario.");

    public void Add(F005Result result)
    {
        Results.Add(result);
        if (result.Kind is F005Kind.History or F005Kind.Recordable or F005Kind.RecentNotes or F005Kind.Calendar)
        {
            LastRead = result;
        }
    }

    public int GivenRecordCount(GoalId goal) => GivenRecords.TryGetValue(goal, out var count) ? count : 0;

    public int GivenNoteCount(GoalId goal) => GivenNoteDates.TryGetValue(goal, out var dates) ? dates.Count : 0;
}
