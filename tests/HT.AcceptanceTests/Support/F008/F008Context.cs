using HT.Application.Exemptions;
using HT.Application.Operator;
using HT.Application.Records;
using HT.Application.Reports;
using HT.Domain.Accounts;
using HT.Domain.Exemptions;
using HT.Domain.Goals;
using HT.Domain.Habits;

namespace HT.AcceptanceTests.Support.F008;

/// <summary>A goal a step created (or a demo goal a step named), with its owner and its display name.</summary>
public sealed record F008Goal(string OwnerEmail, string Name, GoalId Id);

/// <summary>A pause a "paused" result gave in this scenario: its goal, its days as created and its identifier.</summary>
public sealed record F008Pause(F008Goal Goal, DateOnly FirstDay, DateOnly LastDay, PauseId Id);

/// <summary>
/// The outcome of the use case the item's When step called: its result text and the values the result carries. Only
/// the field of the use case that was called is set.
/// </summary>
public sealed class F008Outcome
{
    public string? Message { get; init; }

    public ExcuseResult? Excuse { get; init; }

    public RemoveExcusalResult? RemoveExcusal { get; init; }

    public PauseResult? Pause { get; init; }

    public ResumeResult? Resume { get; init; }

    public CancelPauseResult? CancelPause { get; init; }

    public PausesResult? Pauses { get; init; }

    public RecordWriteResult? Record { get; init; }
}

/// <summary>What the item's When step read (a read changes nothing).</summary>
public enum F008ReadKind
{
    None,
    History,
    Pauses,
    Report,
    Overview,
    Dashboard,
    RecentNotes,
}

/// <summary>Per-scenario state shared by the F008 step definitions (Reqnroll context injection).</summary>
public sealed class F008Context
{
    /// <summary>Acting users from successful sign-ins, by email.</summary>
    public Dictionary<string, ActingUser> Accounts { get; } = new(StringComparer.Ordinal);

    /// <summary>The custom habits steps created, by owner and habit name.</summary>
    public Dictionary<(string Owner, string Habit), HabitId> CustomHabits { get; } = new();

    /// <summary>Every goal a step created or named, in that order.</summary>
    public List<F008Goal> Goals { get; } = [];

    /// <summary>Every pause a "paused" result gave in this scenario, in order.</summary>
    public List<F008Pause> Pauses { get; } = [];

    // ------------------------------------------------------------ time (acceptance.md, "Time")

    /// <summary>
    /// The earliest time the item gives: the time of its first step that gives one (in every F008 item that step is
    /// the earliest). Setup that gives no time runs at it.
    /// </summary>
    public DateTimeOffset? SetupTime { get; set; }

    /// <summary>Given steps that record and give no time: they run at the time of the When step, in the order written.</summary>
    public List<Func<Task>> DeferredToWhen { get; } = [];

    /// <summary>The time the item's When step ran at; Then steps run at it unless they give their own.</summary>
    public DateTimeOffset? WhenTime { get; set; }

    /// <summary>The account that acted in the When step (none: the anonymous visitor or no actor).</summary>
    public string? WhenActor { get; set; }

    // ------------------------------------------------------------ what the When step did

    public F008Outcome? Outcome { get; set; }

    public F008ReadKind ReadKind { get; set; }

    public GoalHistory? History { get; set; }

    public IReadOnlyList<PauseView>? PausesRead { get; set; }

    /// <summary>The goal reports the When step read, by goal name.</summary>
    public Dictionary<string, GoalReport> Reports { get; } = new(StringComparer.Ordinal);

    public GoalsOverview? Overview { get; set; }

    public Dashboard? Dashboard { get; set; }

    public IReadOnlyList<RecentNote>? RecentNotes { get; set; }

    // ------------------------------------------------------------ demo tasks

    public DemoLoadingResult? DemoLoading { get; set; }

    public DemoHistoryLoadingResult? DemoHistory { get; set; }

    public DemoReportLoadingResult? DemoReport { get; set; }

    public DemoPauseLoadingResult? DemoPause { get; set; }

    /// <summary>AC-082: the pauses and histories read for every goal of the earlier demo accounts.</summary>
    public List<(string Email, string Goal, IReadOnlyList<PauseView> Pauses, GoalHistory History)> DemoReads { get; } = [];
}
