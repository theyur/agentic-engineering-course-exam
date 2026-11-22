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

namespace HT.AcceptanceTests.Support.F009;

/// <summary>
/// A goal a step created (or a goal a step named): its owner, the name the item gives it (none for a goal created on
/// a catalog habit until a step names it), its identifier and its habit.
/// </summary>
public sealed record F009Goal(string OwnerEmail, string? Label, GoalId Id, HabitId HabitId, string HabitName);

/// <summary>
/// The outcome of the use case the item's When step called: its result text and the result itself. Only the field of
/// the use case that was called is set.
/// </summary>
public sealed class F009Outcome
{
    public string? Message { get; init; }

    public SettingsChangeResult? SettingsChange { get; init; }

    public GoalEndResult? End { get; init; }

    public NewAttemptValuesResult? StartingValues { get; init; }

    public AttemptStartResult? AttemptStart { get; init; }

    public AttemptListResult? Attempts { get; init; }

    public AttemptReadResult? Attempt { get; init; }

    public GoalHistoryListResult? GoalHistory { get; init; }

    public RecordWriteResult? Record { get; init; }

    public PauseResult? Pause { get; init; }

    public ExcuseResult? Excuse { get; init; }

    public RemoveExcusalResult? RemoveExcusal { get; init; }

    public TimeZoneChangeResult? TimeZoneChange { get; init; }

    public CustomHabitDeleteResult? CustomHabitDelete { get; init; }

    public CatalogHabitUnpublishResult? Unpublish { get; init; }

    public DemoGoalChangeLoadingResult? DemoGoalChange { get; init; }
}

/// <summary>What the item's When step read (a read changes nothing).</summary>
public enum F009ReadKind
{
    None,
    Goal,
    History,
    Recordable,
    Report,
    Reports,
    RecentNotes,
    Overview,
    Dashboard,
    Calendar,
    Attempts,
    Attempt,
    GoalHistory,
    StartingValues,
    Catalog,
}

/// <summary>Per-scenario state shared by the F009 step definitions (Reqnroll context injection).</summary>
public sealed class F009Context
{
    /// <summary>Acting users from successful sign-ins, by email.</summary>
    public Dictionary<string, ActingUser> Accounts { get; } = new(StringComparer.Ordinal);

    /// <summary>The custom habits steps created, by owner and habit name.</summary>
    public Dictionary<(string Owner, string Habit), HabitId> CustomHabits { get; } = new();

    /// <summary>The catalog habits an administrator of the item created, by name.</summary>
    public Dictionary<string, HabitId> CatalogHabits { get; } = new(StringComparer.Ordinal);

    /// <summary>Every goal a step created or named, in that order.</summary>
    public List<F009Goal> Goals { get; } = [];

    // ------------------------------------------------------------ time (acceptance.md, "Time")

    /// <summary>The earliest time the item gives: setup that gives no time runs at it.</summary>
    public DateTimeOffset? SetupTime { get; set; }

    /// <summary>The time the item's When step ran at; Then steps run at it unless they give their own.</summary>
    public DateTimeOffset? WhenTime { get; set; }

    /// <summary>The account that acted in the When step (none: the anonymous visitor or no actor).</summary>
    public string? WhenActor { get; set; }

    // ------------------------------------------------------------ what the When step did

    public F009Outcome? Outcome { get; set; }

    public F009ReadKind ReadKind { get; set; }

    public F009Goal? ReadGoal { get; set; }

    public GoalDetails? GoalRead { get; set; }

    public GoalHistory? History { get; set; }

    /// <summary>What can be recorded now, read in the When step for every goal of its actor, by goal.</summary>
    public Dictionary<GoalId, GoalHistory> Recordable { get; } = new();

    public GoalReport? Report { get; set; }

    /// <summary>The goal reports the When step read, by goal name.</summary>
    public Dictionary<string, GoalReport> Reports { get; } = new(StringComparer.Ordinal);

    public IReadOnlyList<RecentNote>? RecentNotes { get; set; }

    public GoalsOverview? Overview { get; set; }

    public Dashboard? Dashboard { get; set; }

    public GoalCalendarView? Calendar { get; set; }

    public IReadOnlyList<AttemptView>? Attempts { get; set; }

    public AttemptDetails? Attempt { get; set; }

    public IReadOnlyList<EndedAttemptView>? GoalHistoryEntries { get; set; }

    public NewAttemptValues? StartingValues { get; set; }

    public CatalogBrowseResult? Browse { get; set; }

    // ------------------------------------------------------------ demo tasks

    public DemoLoadingResult? DemoLoading { get; set; }

    public DemoHistoryLoadingResult? DemoHistory { get; set; }

    public DemoReportLoadingResult? DemoReport { get; set; }

    public DemoPauseLoadingResult? DemoPause { get; set; }

    /// <summary>AC-095: the attempts of every goal and the goal history of each earlier demo account.</summary>
    public List<(string Email, IReadOnlyList<(string Goal, IReadOnlyList<AttemptView> Attempts)> Goals, IReadOnlyList<EndedAttemptView> History)> DemoReads { get; } = [];
}
