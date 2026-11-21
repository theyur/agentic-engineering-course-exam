# Data model: Personal reports

Phase 1 of [plan.md](plan.md). Every figure here is derived on read and never stored (HP-009, ADR 0020).

## Tables: no schema change

No table, column, index or migration is added. The reports read the existing tables through existing port members:

| Table (slice) | Read by | Through |
|---|---|---|
| `Goals`, `GoalSettingsVersions`, `GoalAttempts` (4) | goal report, overview, dashboard | `IGoalStore.FindAsync`, `IGoalStore.ListAsync` |
| `GoalRecords`, `GoalNotes` (5) | every figure, the notes | `IRecordStore.ReadAsync(owner, goal, from, to)`; the index `(GoalId, Date)` |
| `AspNetUsers` (1, the stored time zone) | the dashboard's "today" and "this week" | `IAccountStore.FindByIdAsync` |

Demo report loading writes one account, its goals, versions, attempts, records and notes into the same tables
through `IDemoReportStore` (R-09).

## Domain: `HT.Domain.Reports`

Pure types and functions with no clock, time zone or culture. Inputs are evaluated states and records.

### `Consistency` (record)

| Member | Type | Rule |
|---|---|---|
| `Successful` | `int` | Met results (FR-001) |
| `Counted` | `int` | Met + Failed + Missed results |
| `Percentage` | `int?` | `Successful * 100 / Counted` (integer division, rounds down; FR-002); null when `Counted` = 0 |
| `Of(IEnumerable<EvaluationState>)` | static | Upcoming, Due and Pending are skipped |

Invariants: `0 ≤ Successful ≤ Counted`. `Percentage` is 100 only when `Successful == Counted > 0`.

### `Streaks` (record)

| Member | Type | Rule |
|---|---|---|
| `Current` | `int` | Met results after the latest Failed or Missed one (all Met when none) (FR-003) |
| `Longest` | `int` | the longest run of consecutive Met results |
| `Of(IEnumerable<EvaluationState>)` | static | states in date order (a week by its Monday); Upcoming, Due and Pending skipped, so they neither extend nor reset a run (D-1) |

Invariants: `0 ≤ Current ≤ Longest`.

### `StreakUnit` (enum) and `StreakUnits` (static class)

`StreakUnit`: `Commitments` (commitment-based), `Days` (per-day), `Weeks` (per-week). `StreakUnits.Of(GoalTarget)`
reads the schedule kind and the period length (FR-004). An enum cannot hold the method (research R-01).

### `TrendKind`, `TrendSpan` (enums), `TrendBar` and `Trend` (records)

| Type | Members | Notes |
|---|---|---|
| `TrendKind` | `Minutes`, `Count`, `RecordCount` | `Minutes`: a commitment-based duration goal, or a Reduce duration period. `Count`: a commitment-based count goal, or a Reduce count period. `RecordCount`: everything else (FR-006, research R-05) |
| `TrendSpan` | `Commitment`, `Day`, `Week` | what one bar covers |
| `TrendBar` | `FirstDate`, `LastDate`, `int? Value`, `int Target` | null `Value`: no record. A week has its Monday and its Sunday. `Target` is the bar's own target |
| `Trend` | `TrendKind Kind`, `TrendSpan Span`, `IReadOnlyList<TrendBar> Bars`, `int Target`, `int MaxBars` | bars oldest first, at most `MaxBars` (12 for weeks, 14 otherwise). `Target` is the latest bar's target, where the page draws its line. Only a commitment-based yes/no Build goal's bars can differ from it (a smaller first week) |
| `TrendBuilder.Of(GoalTarget latest, IReadOnlyList<(CalendarEntry Entry, GoalTarget Target, IReadOnlyList<RecordValue> Records)> begun, IReadOnlyList<CalendarEntry> currentWeekUpcoming)` | static | research R-05's table; durations in minutes |

### `GoalTimeline.EntriesThrough(Goal, DateOnly last)` (new member)

Every commitment or period of the goal from its start date, or its first week, to the entry that contains `last`,
in date order. It reads `TryCalendar` in Monday-aligned 364-day windows, so nothing is doubled or missed
(research R-02). It is empty when `last` is before the first entry.

## Application: `HT.Application.Reports`

Views are sealed records with `required` members. Names and notes appear only in the owner's views.

### Internal: `GoalEvaluation`

For one goal at one instant `now` (research R-04):

| Member | Meaning |
|---|---|
| `Goal`, `Name`, `BuildOrReduce`, `Unit` | the goal, its own or derived name (slice 4 FR-010), Build or Reduce, `StreakUnit` |
| `CurrentDate` | `GoalTimeline.CurrentDate(goal, now)`; null before the start date's day |
| `Entries` | the `HistoryEntry` of every entry through `CurrentDate` (via `EntryViews.Of`), in date order |
| `Consistency`, `Streaks` | from the states of `Entries` |
| `Ended` | the Pending and final entries |
| `Due`, `Pending` | the Due and the Pending entries |
| `Trend` | from `TrendBuilder` |
| `Notes` | the entries with a note, with their state |
| `NextUpcoming(DateOnly windowStart)` | the first Upcoming entry whose first date is at most 6 days after `windowStart` |

### `GoalStatsView`

`Consistency` (`Consistency`), `ConsistencyText` ("90% (18 of 20)" or "N/A"), `Unit` (`StreakUnit`),
`CurrentStreak`, `CurrentStreakText` ("6 commitments"), `LongestStreak`, `LongestStreakText`.

### `ResultView`

`FirstDate`, `LastDate`, `State` (`EvaluationState`), `Progress` (slice 5 `Progress`).

### `RecentResults`

`Results` (`IReadOnlyList<ResultView>`, oldest first, at most 30 or 15) and `Counts`
(`IReadOnlyDictionary<EvaluationState, int>`, every ended state present, 0 included) (FR-005).

### `TrendView`

`Kind`, `Span`, `Measurement`, `CountUnit` (null unless count), `Bars` (`TrendBar`), `Target`, and the texts
`Title`, `SubLine`, `TargetLine` and `Label`. `GetGoalReportAsync` fills the texts with `ReportText.TrendTitle(target, kind,
span)`, `TrendSubLine(target, span, maxBars)`, `TargetLine(kind, target)` and `TrendLabel(view)`, where `target` is
the `GoalTarget` of the latest bar's settings version (research R-05, R-13).

### `GoalReport` (FR-007)

`GoalId`, `Name`, `BuildOrReduce`, `ScheduleKind`, `Period`, `Stats` (`GoalStatsView`), `Recent` (`RecentResults`,
30), `Trend` (`TrendView`), `HasBegunEntry` (bool, for HP-001's "no cards"), `ReadAt`.

### `OverviewGoal` (FR-011)

| Member | Meaning |
|---|---|
| `Item` | slice 4 `GoalListItem`, unchanged, as `GoalService.ListAsync` gives it |
| `Stats` | `GoalStatsView` |
| `Recent` | `RecentResults`, the last 15 |
| `HasPending`, `CanRecord` | a Pending entry; at least one Due or Pending entry |
| `Recordable` | slice 5 `GoalHistory`, equal to `GetRecordableAsync`'s at the same instant: the goal's settings and its Due and Pending `HistoryEntry`s, newest first (research R-07) |

The meta line's Due progress is `ReportText.ProgressText(Recordable, entry)` for a Due entry of `Recordable` with
a record. "Record" opens the RecordDialog with `Recordable` and no row.

### `GoalsOverview`

`Goals` (`IReadOnlyList<OverviewGoal>`, in FR-011 order), `Today` (the account's today), `ReadAt`.

### `TodayEntry` (FR-012)

| Member | Meaning |
|---|---|
| `GoalId`, `GoalName`, `BuildOrReduce` | its goal |
| `ScheduleText` | slice 4 `GoalText.ScheduleText` ("Mon · Wed · Fri"), for a Due row's meta line without a record |
| `Goal` | slice 5 `GoalHistory`: the goal's settings (schedule kind, period, measurement, amount, units, N) and its Due and Pending entries, as `GetRecordableAsync` gives them |
| `Entry` | slice 5 `HistoryEntry`: first and last date (a week: Monday and Sunday), state (Pending, Due, Missed or Upcoming), grace end (Pending and Due), records with their identifiers, progress, note |
| `HasRecord`, `HasNote` | `Entry.Records.Count > 0`, `Entry.Note is not null` (FR-012) |

A row opens the RecordDialog with `Goal` as `Recordable` and `Entry` as the row (and its one `RecordView` for
"Edit"), or the NoteDialog with `Goal` and `Entry` (research R-06). An Upcoming entry has no records and no grace
end.

### `DashboardCounts`

`Due`, `Pending`, `MissedThisWeek`, `Goals`.

### `DashboardGoal`

`GoalId`, `Name`, `Stats` (`GoalStatsView`).

### `DashboardNote`

`GoalId`, `GoalName`, `FirstDate`, `LastDate`, `Note`, `State`.

### `Dashboard`

`Counts`, `Today` (`IReadOnlyList<TodayEntry>`), `Goals` (`IReadOnlyList<DashboardGoal>`, FR-011 order),
`Notes` (`IReadOnlyList<DashboardNote>`, at most 5), `AccountToday`, `WeekStarts` (the UTC instant of this week's
Monday 00:00 in the account's zone), `ReadAt`.

### Results (ADR 0005)

| Result | Outcomes | Message |
|---|---|---|
| `GoalReportResult` | `Found`, `NotSignedIn`, `NotForAdministrators`, `NotFound` | "You are not signed in.", "Goals are for registered users.", "This goal does not exist." |
| `GoalsOverviewResult` | `Found`, `NotSignedIn`, `NotForAdministrators` | the first two |
| `DashboardResult` | `Found`, `NotSignedIn`, `NotForAdministrators` | the first two |

### `ExampleGoal` (HP-004)

A static `GoalStatsView Stats`, plus `Title` ("Yoga, 3 days a week") and `Strip`
(`IReadOnlyList<EvaluationState>`, 21 states: Met × 11, Missed, Met, Failed, Met × 6, Pending). The GoalStats
component takes a list of strip states, so the goal page passes `Recent.Results`' states and this card passes
`Strip`. One `ReportText.StripLabel` serves both. They are computed by `Consistency.Of`, `Streaks.Of` and `ReportText` from those
states: "90% (18 of 20)", current 6 and longest 11 commitments. No store is read.

## Application: `HT.Application.Operator` (demo report loading)

| Type | Members |
|---|---|
| `IDemoReportStore` | `Task<CreateDemoAccountOutcome> CreateDemoReportAccountAsync(Email email, string password, DemoAccountProfile profile, IReadOnlyList<DemoHistoryGoal> goals, CancellationToken ct)` |
| `DemoReportService` | `const string DemoEmail = "demo5@example.com"`; `Task<DemoReportLoadingResult> LoadAsync(string? demoPassword, CancellationToken ct)` |
| `DemoReport` | `IReadOnlyList<string> GoalNames`; `IReadOnlyList<DemoHistoryGoal> For(DateOnly runDate)`; the note texts as the constants `WalkingNote` ("On holiday."), `StrengthNote` ("Sore shoulder.") and `ScreenTimeNote` ("Long train ride."), named in T072 |
| `DemoReportLoadingResult` | `Outcome` (`NoPassword`, `PasswordRuleNotMet`, `Loaded`), `Message`, `Accounts` (`IReadOnlyList<DemoReportAccountResult>`, one item when loaded) |
| `DemoReportAccountResult` | `Email`, `Outcome` (`DemoAccountOutcome`), `Message` ("created" or "already exists"), `Goals` (`DemoGoalsResult?`), `Records` (`DemoRecordsResult?`); member for member as `DemoHistoryAccountResult` |

### `demo5@example.com`'s data for run date R (FR-032)

All goals start on R − 84, in the installation's default time zone, with settings version 1 and attempt 1 from
that date.

| Goal (derived name) | Target | Records and notes |
|---|---|---|
| "Walking, 30 min, 3 days a week" | Build, duration 30 min, fixed days Mon, Wed, Fri | each commitment R − k, k 3–84: 30 min; k 9–15: 35; k 20–26: 18; k 43–49: none, the latest of these with the note "On holiday."; k 0–2: none |
| "Strength training, 2 times a week" | Build, yes/no, 2 times per week | each full week before the current one: Yes on Mon and Thu; week 2: Mon, Wed, Thu; week 4: Mon only, note "Sore shoulder." on that week; week 7: none; current week: none. Weeks 1–11 always exist, and week 12 when R is a Monday |
| "Screen time, at most 120 min a day" | Reduce, duration at most 120 min, once per day | each date R − k, k 4–84: 90 min (k even) or 110 (k odd); k 30–32: 150, note "Long train ride." on R − 31; k = 50: none; R − 3: none; R − 2: 100; R − 1 and R: none |

A goal whose catalog habit (looked up by name, ignoring case) is not Published is left out with its records and
notes (FR-032).
