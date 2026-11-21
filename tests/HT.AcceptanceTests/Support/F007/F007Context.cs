using HT.Application.Goals;
using HT.Application.Operator;
using HT.Application.Reports;
using HT.Domain.Accounts;
using HT.Domain.Goals;
using HT.Domain.Habits;

namespace HT.AcceptanceTests.Support.F007;

/// <summary>
/// A goal a step created (or a demo goal a step named), with its owner, the habit it is on, the name the step gave it
/// (none: the goal is named by its habit and target) and its position among its owner's goals in creation order.
/// </summary>
public sealed record F007Goal(string OwnerEmail, GoalId Id, string Habit, string? Name, int Position);

/// <summary>A goal report the When step read, with the goal it was read for (none: an identifier of no goal).</summary>
public sealed record F007Report(F007Goal? Goal, GoalReportResult Result);

/// <summary>A use case of this slice called in a When step, as the refusal checks see it.</summary>
public sealed record F007Call(string? Message, bool ReturnedData, bool Found);

/// <summary>Per-scenario state shared by the F007 step definitions (Reqnroll context injection).</summary>
public sealed class F007Context
{
    /// <summary>Acting users from successful sign-ins, by email.</summary>
    public Dictionary<string, ActingUser> Accounts { get; } = new(StringComparer.Ordinal);

    public Dictionary<(string Owner, string Name), HabitId> CustomHabits { get; } = new();

    public Dictionary<string, HabitId> CatalogHabits { get; } = new(StringComparer.Ordinal);

    /// <summary>Every goal a step created, in creation order.</summary>
    public List<F007Goal> Goals { get; } = [];

    // ------------------------------------------------------------ time (acceptance.md, "Time")

    /// <summary>Whether the time of account and habit setup is known yet (the first step that gives a time fixes it).</summary>
    public bool SetupResolved { get; set; }

    /// <summary>The time account and habit setup that gives no time runs at.</summary>
    public DateTimeOffset SetupTime { get; set; } = F007Driver.DefaultNow;

    /// <summary>Setup written before the first step that gives a time; it runs once that step fixes the setup time.</summary>
    public List<Func<Task>> PendingSetup { get; } = [];

    /// <summary>Given steps that record or save notes and give no time: they run at the time of the When step.</summary>
    public List<Func<Task>> DeferredToWhen { get; } = [];

    /// <summary>The time the item's When step ran at; Then steps run at it unless they give their own.</summary>
    public DateTimeOffset? WhenTime { get; set; }

    // ------------------------------------------------------------ what the When step read

    public List<F007Report> Reports { get; } = [];

    public GoalsOverviewResult? Overview { get; set; }

    public DashboardResult? Dashboard { get; set; }

    public GoalListResult? GoalList { get; set; }

    /// <summary>The goal list a Given step read (AC-063), to compare the When step's goal list with.</summary>
    public IReadOnlyList<GoalListItem>? EarlierGoalList { get; set; }

    /// <summary>The use cases a When step called for the refusal checks, in order.</summary>
    public List<F007Call> Calls { get; } = [];

    public DemoLoadingResult? DemoLoading { get; set; }

    public DemoHistoryLoadingResult? DemoHistory { get; set; }

    public DemoReportLoadingResult? DemoReport { get; set; }
}
