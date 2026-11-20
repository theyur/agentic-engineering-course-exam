# Contract: Application API of personal reports

The public surface this slice adds to `HT.Application`. The acceptance tests and the pages call it. Types are in
[data-model.md](../data-model.md). Existing members of slices 1 to 5 are unchanged (FR-016).

## `ReportService` (`HT.Application.Reports`)

```csharp
public sealed class ReportService
{
    public ReportService(
        IGoalStore goals, IRecordStore records, IAccountStore accounts, TimeProvider clock, InstallationTimeZone defaultZone);

    public Task<GoalReportResult> GetGoalReportAsync(ActingUser actingUser, GoalId goal, CancellationToken cancellationToken);
    public Task<GoalsOverviewResult> GetGoalsOverviewAsync(ActingUser actingUser, CancellationToken cancellationToken);
    public Task<DashboardResult> GetDashboardAsync(ActingUser actingUser, CancellationToken cancellationToken);
}
```

### Common rules

- **Check order** (FR-007, FR-015):
  1. no account id → `NotSignedIn`, "You are not signed in.";
  2. role is not registered user → `NotForAdministrators`, "Goals are for registered users.";
  3. account not stored → `NotSignedIn`, "You are not signed in.";
  4. goal report only: the goal not among the owner's goals → `NotFound`, "This goal does not exist.".

  A refusal returns no view.
- **Time**: `TimeProvider.GetUtcNow()`, read once per call. Every entry is evaluated at that instant (FR-008).
- **Owner**: every store call passes the acting user's account as the owner (FR-014).
- **Evaluation**: one `GoalEvaluation` per goal per call, through `GoalTimeline.EntriesThrough(goal,
  currentDate)` and one `IRecordStore.ReadAsync`. Each entry uses its own settings version (FR-009). Nothing is
  stored (HP-009).

### `GetGoalReportAsync` (FR-007)

`Found` carries a `GoalReport`:

| Part | Rule |
|---|---|
| `Stats` | the consistency of FR-001 and FR-002, the streaks of FR-003 and the texts of FR-004 |
| `Recent` | the latest 30 ended entries, oldest first, with the count of each state (FR-005) |
| `Trend` | FR-006 (research R-05) |
| `HasBegunEntry` | false when the goal has no current date, or no entry has begun yet (a per-week goal before its first week) |

A goal that has not started gets "N/A", streaks of 0 in its unit, no recent results and an empty trend (Edge
Cases).

### `GetGoalsOverviewAsync` (FR-011, FR-016)

`Found` carries a `GoalsOverview`. Each `OverviewGoal` has:

- `Item`: equal, field by field, to the entry for the same goal that `GoalService.ListAsync` returns;
- `Stats`;
- `Recent` (the last 15);
- `HasPending`;
- `CanRecord`: at least one Due or Pending entry;
- `Recordable`: equal to `RecordingService.GetRecordableAsync`'s `GoalHistory` for the goal at the same instant. The
  RecordDialog opens with it in its page-head mode, with the "For" select (slice 5 HP-002).

The goals are ordered in three groups (Pending, then Due, then the others). Within a group the order is
`HabitName.KeyOf(name)` compared ordinally, then creation order. `GoalService.ListAsync` keeps its own name order.

### `GetDashboardAsync` (FR-012, FR-013)

`Found` carries a `Dashboard`:

| Part | Rule |
|---|---|
| `WeekStarts` | Monday 00:00 of the week containing now, in the account's zone (the stored one, else the installation default), as a UTC instant |
| `Counts.Due`, `Counts.Pending` | Due and Pending entries of every goal |
| `Counts.MissedThisWeek` | final Missed entries with `WeekStarts ≤ GraceEnds ≤ now` |
| `Counts.Goals` | the number of the user's goals |
| `Today` | in order: Pending by grace end ascending; Due in goal order (by first date within a goal); Missed this week by grace end descending; ties in goal order; then Upcoming (R-06) |
| Upcoming | per goal with no Due entry, the first Upcoming entry whose first date is at most 6 days after the goal's current date, or after the account's today for a goal that has not started; ordered by start instant, then goal order; not counted |
| `Goals` | every goal with `Stats`, in goal order |
| `Notes` | at most 5, by first date descending, ties in goal order |

Each `TodayEntry` carries its goal's `GoalHistory` (settings and its Due and Pending entries, as
`GetRecordableAsync` gives them) and the entry as a slice 5 `HistoryEntry`. So a row's chip, progress and dialogs
need no other read (research R-06).

A user with no goals gets counts of 0 and empty lists, never a refusal (FR-013).

## `ReportText` (`HT.Application.Reports`, HP-005)

```csharp
public static class ReportText
{
    public static string Consistency(Consistency consistency);          // "90% (18 of 20)" | "N/A"
    public static string Unit(StreakUnit unit, int count);              // "commitments" | "commitment" | "days" | "day" | "weeks" | "week"
    public static string Streak(int count, StreakUnit unit);            // "6 commitments", "1 day", "0 weeks"
    public static string StreakLabel(StreakUnit unit);                  // "Streak, commitments"
    public static string LongestLabel(StreakUnit unit);                 // "Longest, commitments"
    public static string SummaryLine(Streaks streaks, StreakUnit unit); // "Streak 6 commitments · longest 11"
    public static string NoFinalYet(StreakUnit unit);                   // "No finalized commitments yet"
    public static string State(EvaluationState state, GoalScheduleKind kind, SchedulePeriod? period); // "Upcoming", else RecordingText.State
    public static string ProgressText(GoalHistory goal, HistoryEntry entry); // "12 of 20 min today" | "1 of 3 this week"
    public static string PendingLeft(DateTimeOffset graceEnds, DateTimeOffset now); // "5 h left to record"
    public static string TrendTitle(GoalTarget target, TrendKind kind, TrendSpan span); // "Minutes per day" | "Times per week" | "Days done per week"
    public static string TrendSubLine(GoalTarget target, TrendSpan span, int maxBars);  // "last 14 days" | "last 14 commitments" | "last 12 weeks"
    public static string TargetLine(TrendKind kind, int target);        // "Target 20 min" (Minutes) | "Target 3"
    public static string StripLabel(IReadOnlyList<EvaluationState> strip, StreakUnit unit); // "Last 28 commitments: 24 met, 1 failed, 1 missed, 2 pending"
    public static string TrendLabel(TrendView trend);                   // "Minutes per day, last 14 days; target 20"
}
```

Every text follows `design/brand-book.md`. Pages build no consistency, streak or unit text themselves.
`GetGoalReportAsync` fills `TrendView.Title`, `SubLine`, `TargetLine` and `Label` with these functions.

## `RecordingText.HoursLeft` (`HT.Application.Records`, new public member)

```csharp
public static long HoursLeft(DateTimeOffset graceEnds, DateTimeOffset now); // whole hours, rounded up, 0 at or after the end
```

This is the rule that `ForLabel` already applies, moved out so that `ForLabel` and `ReportText.PendingLeft` share
it. `ForLabel`'s output is unchanged.

## `ExampleGoal` (`HT.Application.Reports`, HP-004)

```csharp
public static class ExampleGoal
{
    public static string Title { get; }                        // "Yoga, 3 days a week"
    public static IReadOnlyList<EvaluationState> Strip { get; } // 21 states, oldest first
    public static GoalStatsView Stats { get; }                 // "90% (18 of 20)", 6 and 11 commitments
}
```

## `DemoReportService` (`HT.Application.Operator`, FR-032)

```csharp
public sealed class DemoReportService
{
    public const string DemoEmail = "demo5@example.com";
    public DemoReportService(IAccountStore accounts, IDemoReportStore demoReport, TimeProvider clock, InstallationTimeZone defaultZone);
    public Task<DemoReportLoadingResult> LoadAsync(string? demoPassword, CancellationToken cancellationToken);
}
```

| Situation | Result |
|---|---|
| no password (null, empty or white space) | `NoPassword`, "no demo password configured"; nothing created |
| password fails the password rule | `PasswordRuleNotMet`, "demo password does not meet the password rule"; nothing created |
| the email has an account (ignoring case) | `Loaded`, account result "already exists", no goal or record result; nothing read or changed |
| otherwise | `Loaded`, account result "created", with "demo goals added" and "demo records added", also when goals were left out |

Run date R is the date of now in the installation's default zone, read once. The results name `demo5@example.com`
and the result texts only.

## `IDemoReportStore` (`HT.Application.Operator`)

```csharp
public interface IDemoReportStore
{
    Task<CreateDemoAccountOutcome> CreateDemoReportAccountAsync(
        Email email, string password, DemoAccountProfile profile, IReadOnlyList<DemoHistoryGoal> goals, CancellationToken cancellationToken);
}
```

It has the same contract as `IDemoHistoryStore.CreateDemoHistoryAccountAsync`:

- one transaction;
- `EmailTaken` creates nothing;
- a goal whose catalog habit is not Published is left out, with its records and notes;
- a fault rolls everything back and names no goal, habit, note or value.

Architecture rules allow only `DemoReportService` to depend on it.

## Unchanged

`GoalService`, `RecordingService`, `DemoHistoryService`, `DemoDataService`, every port member and every result type
of slices 1 to 5. `RecordingText` keeps every member and gains only `HoursLeft`. Three private helpers become
internal and shared, with the same behaviour:

- `RecordingService.EntryView` becomes `EntryViews.Of`;
- `RecordingService.HistoryOf` becomes `EntryViews.HistoryOf`;
- `GoalService.ListItemOf` becomes `GoalListItems.Of`.
