# Research: Personal reports

Phase 0 of [plan.md](plan.md). The spec leaves no `[NEEDS CLARIFICATION]` marker open: D-1 to D-5 and R3 are
decided in its `## Clarifications`, ADR 0020 and ADR 0021. This file records the technical choices the spec
leaves to the plan, each with its reason and the alternatives rejected. It uses the code of slices 1 to 5 as it is
on `master` at ce4e562.

## R-01 Domain: figures as pure functions over evaluated entries

**Decision**: a new namespace `HT.Domain.Reports` with pure, clock-free functions over the entries of one goal
that slice 5's `Evaluator.Evaluate` has already evaluated:

- `Consistency` (record): `Successful`, `Counted`, and `Percentage` (`int?`: `Successful * 100 / Counted` with
  integer division, which rounds down for non-negative numbers; null when `Counted` is 0). Made by
  `Consistency.Of(IEnumerable<EvaluationState>)`: Met counts as successful; Met, Failed and Missed are counted;
  Upcoming, Due and Pending are skipped (FR-001, FR-002).
- `Streaks` (record): `Current`, `Longest`. Made by `Streaks.Of(IEnumerable<EvaluationState>)` over the states in
  date order. Met adds 1 to the run, Failed or Missed ends it, and Upcoming, Due and Pending are skipped. They
  neither extend nor reset a run (FR-003, D-1).
- `StreakUnit` (enum `Commitments`, `Days`, `Weeks`) and the static class `StreakUnits` with `Of(GoalTarget)`,
  which reads the schedule kind and the period length (FR-004). An enum cannot hold a method, and the repo uses no
  C# 14 extension members yet (CLAUDE.md, Roslynator caveat), so the method sits on its own static class.
- `TrendBar` (record: `FirstDate`, `LastDate`, `int? Value`, `int Target`) and `Trend` (`Kind`, `Span`, `Bars`,
  `Target`), with:
  - `TrendKind`: `Minutes` (durations), `Count` (count amounts) or `RecordCount` (numbers of records);
  - `TrendSpan`: `Commitment`, `Day` or `Week`, what one bar covers.

  They are made by `TrendBuilder.Of(...)` from the begun entries with their records and the target of each entry's
  version (FR-006, R-05).

**Rationale**: constitution II asks for one deterministic function of stored facts and a supplied time. Slice 5
already computes every state that way. Consistency and streaks depend only on the sequence of states, so they are
written as folds over states. Their unit tests then need no goal, calendar or store, and they can cover every
boundary that SC-003 names. Slices 7 and 9 extend the same functions: slice 7 adds states to skip, and slice 9
calls them over a 30-day window.

**Alternatives rejected**:

- Computing in Application. Constitution II and ARCHITECTURE §3 put rules in Domain, and slice 9 needs them
  without Application's use cases.
- A `GoalReport` entity in Domain that reads records itself. That would bring storage concepts into Domain.

## R-02 The report range: every entry from the start date to the current date, with no 366-day cap

**Decision**: a new member `GoalTimeline.EntriesThrough(Goal goal, DateOnly last)` gives every commitment or
period of the goal from its start date (a per-week goal: from its first week) to the entry that contains `last`,
in date order. It walks `TryCalendar` in windows of 364 days (52 weeks), each starting on a Monday. A per-week
period therefore never straddles two windows, and nothing is listed twice. The report reads it with `last` = the
goal's current date at the supplied time. A goal with no current date has no entry.

**Rationale**:

- The report range is the whole attempt (spec "Terms used below"). The Edge Cases also require that "a goal older
  than the 366 days of the History card" counts all its results.
- `TryEntries` refuses ranges over 366 days (slice 5 FR-009). That cap belongs to one history read, not to
  reports.
- Aligning windows to weeks is simpler than removing duplicates after the fact.
- The entries up to the current date include every Due and Pending entry, so the ended results, the begun bars
  and the Today list all come from one list.

**Alternatives rejected**:

- Raising `MaxCalendarDays`. That would change slice 5's refusal ("RangeOutOfBounds"), which F005 tests.
- Reading only the last 366 days. That breaks FR-001 and the edge case above.

## R-03 Application: `ReportService` with three owner-scoped use cases

**Decision**: a new `HT.Application.Reports.ReportService` with three use cases, each taking the `ActingUser`
first:

| Use case | FR | Result | Refusals |
|---|---|---|---|
| `GetGoalReportAsync(actingUser, goal, ct)` | FR-007 | `GoalReportResult` → `GoalReport` | not signed in, administrator, "This goal does not exist." |
| `GetGoalsOverviewAsync(actingUser, ct)` | FR-011 | `GoalsOverviewResult` → `GoalsOverview` | not signed in, administrator |
| `GetDashboardAsync(actingUser, ct)` | FR-012, FR-013 | `DashboardResult` → `Dashboard` | not signed in, administrator |

- The checks run in slice 5's order (FR-007, FR-015): no account id gives "You are not signed in."; any role other
  than registered user gives "Goals are for registered users."; an account the store no longer has gives "You are
  not signed in."; then the goal, found only through `IGoalStore.FindAsync(owner, goal)`.
- The time is `TimeProvider.GetUtcNow()`, read once per call. Every entry of the call is evaluated at that
  instant (constitution II, FR-008).
- Its dependencies are the existing ports `IGoalStore` (`FindAsync`, `ListAsync`), `IRecordStore` (`ReadAsync`)
  and `IAccountStore` (`FindByIdAsync`, for the account's time zone), plus `TimeProvider` and
  `InstallationTimeZone`. It adds no port and no port member (R-08).
- Results follow ADR 0005: an outcome enum, a message for a refusal, and the view only when it is found.

**Rationale**: the three reads share their evaluation and their refusals but return different views, as slice 5's
`RecordingService` does. A separate service keeps `RecordingService` and `GoalService`, which the locked F004 and
F005 support code compiles against, unchanged (FR-016). That avoids the risk slice 5's plan named for constructors
and port members.

**Alternatives rejected**:

- Adding the use cases to `GoalService` or `RecordingService`. That changes their constructors, which the locked
  AC support code may build by hand.
- One use case that returns everything. The pages read different subsets, and the goal page needs one goal only.

## R-04 One evaluation per goal per read, shared by every figure

**Decision**: an internal `GoalEvaluation` in `HT.Application.Reports` holds, for one goal at one instant:

- the goal;
- its `CalendarEntry` list from R-02;
- for each entry its records and note (from one `IRecordStore.ReadAsync(owner, goal, firstEntry.FirstDate,
  lastEntry.LastDate)`) and its `EntryEvaluation`, evaluated with the target of `GoalTimeline.VersionOf(goal,
  entry.FirstDate)` (FR-009);
- for a commitment-based yes/no goal, the Upcoming commitments of the current week, from `TryEntries` up to the
  current week's Sunday. They are needed for the week's target in FR-006.

`GoalEvaluation.Of(...)` is built once per goal per call, and every figure of that call derives from it: the
consistency, the streaks, the recent results, the trend, the Today entries and the notes (HP-009, ADR 0020 "Within
one read").

The entry evaluation that slice 5 keeps private in `RecordingService.EntryView` moves to an internal shared helper,
`EntryViews.Of(goal, entry, records, note, now)` in `HT.Application.Records`. Its private `HistoryOf` moves with it
as `EntryViews.HistoryOf(goal, currentDate, entries, now)`, so reports build the same `GoalHistory` (R-06, R-07).
`RecordingService` then calls the helpers. Its behaviour, public members and texts do not change, and its unit tests prove it.

**Rationale**: one store read per goal per call. It is the simplest form of "computed on read" that meets SC-007,
and it gives the history and the reports the same evaluation of an entry.

**Alternative rejected**: a store read per figure. It would read the same rows up to four times.

## R-05 Trend bars (FR-006)

**Decision**: `TrendBuilder.Of` takes the goal's target kind and the begun entries with their records, and gives
the bars below, oldest first:

| Goal | Bar | Last | Value | Target |
|---|---|---|---|---|
| per-week | the period | 12 | the period's progress (slice 5 FR-014) | N (Build, Reduce yes/no) or the amount (Reduce count or duration), in minutes for a duration |
| per-day | the day | 14 | as per-week | as per-week |
| commitment-based count or duration | the commitment | 14 | its recorded amount (minutes for a duration) | the amount (× 60 for hours) |
| commitment-based yes/no | each Monday-to-Sunday week with at least one begun commitment | 12 | the week's commitments recorded "Yes" | Build: the week's commitments on or after the start date, Upcoming ones of the current week included; Reduce: 0 |

- A bar without any record has the value null, also when slice 5's progress gives 0 for a period with no records.
- A bar with at least one record has its value, 0 included.
- Values above the target are given as recorded.
- A bar's target comes from the version of its entry's date (the Monday for a week), so a later change of target
  does not rewrite older bars.
  - In this slice versions differ only in their time zone (slice 4 FR-026). So the bars of every goal except a
    commitment-based yes/no Build goal share one target.
  - A commitment-based yes/no Build goal's weekly target is the number of the week's commitments on or after the
    start date. Its first week can be smaller (AC-025: 2, 3, 3).
  - `Trend.Target` is the latest bar's target. For a yes/no goal that is the full week's count, because the current
    week's Upcoming commitments are included. The page draws its one dashed line at `Trend.Target`.
  - Each bar keeps its own `Target`, which the bar's tooltip and the trend's `aria-label` state ("week of 2 Nov: 2
    of 2"). The design's TrendChart gets this note in step 1 (plan, Spec discrepancies 2).
- The value counts the records of every state, Due and Pending included.
- `Trend.Kind` is `Minutes` for a commitment-based duration goal or a Reduce duration period, `Count` for a
  commitment-based count goal or a Reduce count period, and `RecordCount` everywhere else. `Trend.Span` is
  `Commitment`, `Day` or `Week`. Durations stay in minutes, as stored.
- The page writes "Target {n} min" only for `Minutes`, and "Target {n}" for `Count` and `RecordCount` (HP-001). So
  demo4's "Sugar, at most 1 sweet a day" reads "Target 1", and AC-027's 5-page count goal "Target 5".
- The title and sub line come from `ReportText.TrendTitle(target, kind, span)` and `TrendSubLine(target, span,
  maxBars)`, with the target of the latest bar's settings version (R-13). The goal's target, not the view, tells
  every case apart: its schedule kind, days, measurement and count unit.

**Rationale**: the table is FR-006 line by line. Reading progress from slice 5's `Evaluator.ProgressOf` for
periods keeps one definition of "records that reach the amount".

## R-06 Ordering, groups and the 6-day Upcoming window

**Decision**:

- **Goal order (FR-011).** The groups are: goals with a Pending entry; then goals with a Due entry (and none
  Pending); then the others. Within a group the order is `HabitName.KeyOf(goal name)` compared ordinally (the
  slice 2 FR-004 comparison that `GoalService.ListAsync` already uses), then the store's creation order (the
  order of `IGoalStore.ListAsync`). The dashboard uses this one ordering for its goal list, for tie-breaks in the
  Today list and for tie-breaks among notes.
- **Today list (FR-012).** The list has four parts, in this order:
  1. Pending entries, by grace end ascending, ties in goal order.
  2. Due entries, in goal order. A goal with two Due entries (overlap after a move east) lists them by first date.
  3. Missed entries whose grace end is at or after this week's Monday 00:00 (in the account's zone, as a UTC
     instant) and at or before now, by grace end descending, ties in goal order.
  4. Upcoming entries.
- **Upcoming entries.** For each goal with no Due entry, its earliest entry whose state is Upcoming and whose first
  date is at most 6 days after the goal's current date. For a goal without a current date, the window is at most
  6 days after the account's today, and its first entry is used. The order is first start instant ascending, then
  goal order. They are found with `GoalTimeline.TryEntries(goal, from, from + 6)`.
- **"This week" (FR-012).** The Monday-to-Sunday week that contains now in the account's time zone: the stored
  zone, or the installation default when none is stored, as `GoalService` does. Its start is Monday 00:00 local,
  turned into a UTC instant by NodaTime (`NamedTimeZone`). Only the counts and the Missed part use it.
- **Recent notes (FR-012).** From the notes of every goal's `GoalEvaluation`: ordered by first date descending,
  ties in goal order, and the first 5 kept. Each carries its goal's name, its first and last date, the note and
  the entry's state.

- **What a Today entry carries.** The goal's identifier, name, Build or Reduce and slice 4 schedule text
  (`GoalText.ScheduleText`), plus two slice 5 views:
  - `Entry`: the entry as a `HistoryEntry` (dates, state, grace end, records with their identifiers, progress,
    note);
  - `Goal`: the goal's settings as a `GoalHistory`. Its `Entries` are the goal's Due and Pending entries, as in
    `GetRecordableAsync`.

  So a HP-003 row has everything without another read:
  - `StatusChip` takes the state, the schedule kind and the period;
  - the progress text and bar take the measurement, the unit and the progress;
  - "Record" opens the RecordDialog with `Goal` as `Recordable` and `Entry` as the fixed row;
  - "Edit" adds the row's one `RecordView`;
  - "Add note" and "Edit note" open the NoteDialog with `Goal` and `Entry`.

  An Upcoming entry is a `HistoryEntry` with the state Upcoming, no records and no grace end. FR-012's "whether it
  has a record" and "whether it has a note" are `Entry.Records.Count > 0` and `Entry.Note is not null`.

**Rationale**: each rule is one sentence of FR-011 or FR-012. The acceptance criteria (AC-030, AC-033, AC-034,
AC-038, AC-036) fix the tie-breaks, so no other order is possible. The entry views are slice 5's own types, so the
dialogs get exactly what the goal page gives them and need no second read.

**Detail the spec leaves open (resolved by its words)**: an entry with notes but on a date before the report range
cannot exist, because records and notes are filed only under entries of the goal (slice 5 FR-002, FR-007). So the
notes read with the attempt's records are all of the goal's notes.

## R-07 Overview: "something can be recorded now", the last 15 results and Due progress

**Decision**: a goal can be recorded now when its `GoalEvaluation` has at least one Due or Pending entry. That is
what slice 5's `GetRecordableAsync` returns, because every Due or Pending entry lies within its 9-day window: a
per-week period is Pending until the Wednesday after it.

The overview's goal entry has:

- the slice 4 list item (`GoalListItem`, unchanged), built by the same helper, `GoalService.ListItemOf`, moved to an
  internal shared place;
- the consistency, the current streak and the last 15 ended results (R-04, FR-005);
- `HasPending` and `CanRecord`;
- `Recordable`: a slice 5 `GoalHistory` equal to what `GetRecordableAsync` gives for the goal at the same instant.
  It has the goal's settings (schedule kind, period, measurement, amount, count and duration unit, N) and its Due
  and Pending `HistoryEntry`s, newest first, with their records, progress and grace end. It is built by slice 5's
  `HistoryOf`, which moves to `EntryViews` with `EntryView` (R-04).

The page needs nothing else:

- The meta line's Due progress ("12 of 20 min today", "1 of 3 this week") comes from `ReportText.ProgressText`
  (R-13), applied to `Recordable` and a Due entry of it.
- "Record" opens slice 5's RecordDialog **in its page-head mode**, with `Recordable` and no row. Its "For" select
  lists every entry that can be recorded now, as on the goal page (slice 5 HP-002). The spec's HP-002 says
  "opening the RecordDialog of slice 5 HP-002", and the dialog's fixed mode belongs only to a row (slice 5 HP-002,
  slice 6 HP-003).

**Rationale**: FR-011 names slice 4's entry "name, Build or Reduce, schedule and the start or next information",
and AC-063 compares the overview with the goal list field by field. Building both from one helper makes that
comparison true by construction. `GoalService.ListAsync` keeps its behaviour (FR-016).

## R-08 Storage: no table, no migration, no new read port

**Decision**: nothing is stored (HP-009, ADR 0020). The reports read through existing port members only:

- `IGoalStore.ListAsync(owner)` for the overview and the dashboard;
- `IGoalStore.FindAsync(owner, goal)` for the goal report;
- `IRecordStore.ReadAsync(owner, goal, from, to)`, once per goal, over the whole attempt. It has no range cap. The
  index `(GoalId, Date)` of slice 5 serves it;
- `IAccountStore.FindByIdAsync(owner)` for the time zone.

SC-007's case is 3 goals, one with 366 daily results. A dashboard read costs about 8 round trips:

- the account (`IAccountStore.FindByIdAsync`);
- the goal list;
- per goal, the two queries of `RecordStore.ReadAsync` (records, then notes).

Each returns at most a few hundred rows, followed by in-memory evaluation. The goal report costs 4 (account, goal,
records, notes). An integration test measures each read against SQL Server.

**Alternatives rejected**:

- A new `IRecordStore` member that reads every goal of the owner in one query. Adding a member to an existing port
  breaks any locked AC support code that implements it (slice 5 plan, Risks). The gain is a few queries per page
  at demo scale.
- A new read port. It is not needed while the existing members suffice.

## R-09 Demo report loading: a service, a data builder and its own port

**Decision**:

- **`DemoReportService`** in `HT.Application.Operator` follows `DemoHistoryService` member for member:
  - `DemoEmail = "demo5@example.com"`;
  - `LoadAsync(demoPassword, ct)` checks the password first: "no demo password configured", or "demo password does
    not meet the password rule";
  - it reads the run date R once in the installation's default zone;
  - it looks the email up ignoring case ("already exists" and nothing more);
  - otherwise it creates the account with the goals of `DemoReport.For(R)`.
- **`DemoReport`** builds FR-032's three goals and their records and notes for R, as `DemoHistory` does for
  `demo4`. It reuses `DemoHistoryGoal`, `DemoRecord`, `DemoNote` and `DemoGoal.OnCatalogHabit`. Habits are named
  by their seed names "Walking", "Strength training" and "Screen time". The goals start 84 days before R, and
  "Strength training"'s first full week starts at most 6 days later.
- **Its own port, `IDemoReportStore`**, with one member, `CreateDemoReportAccountAsync(email, password, profile,
  goals, ct)`. It has the same contract as `IDemoHistoryStore`: one transaction; goals on a habit that is not
  Published are left out with their records and notes; faults name no value.
- In `HT.Persistence`, `DemoHistoryStore`'s body moves to an internal `DemoAccountWriter`. `DemoHistoryStore` and
  a new `DemoReportStore` both call it, so the two ports write in exactly the same way.
- **`DemoReportLoadingResult`** and **`DemoReportAccountResult`** copy `DemoHistoryLoadingResult` and
  `DemoHistoryAccountResult` member for member:
  - the loading result: `Outcome` (`NoPassword`, `PasswordRuleNotMet`, `Loaded`), `Message`, and `Accounts` (an
    `IReadOnlyList<DemoReportAccountResult>` with one item);
  - the account result: `Email`, `Outcome` (`DemoAccountOutcome`), `Message`, `Goals` (`DemoGoalsResult?`, "demo
    goals added") and `Records` (`DemoRecordsResult?`, "demo records added"), the last two only for a created
    account.

  The texts come from `OperatorTexts`, where they already exist for demo history loading. No text names a goal,
  habit, note, unit or amount (FR-032).

**Rationale**:

- F005 AC-051 and AC-059 let demo history loading name `demo4` only (ADR 0021), so the data cannot go into
  `DemoHistory`.
- A separate port is what HP-008 and the spec's test list assume ("its `Ports` list gains demo report loading's
  port"). It also lets the architecture rules fence each port to its own service, as ADR 0013 asks.
- Sharing the Persistence writer avoids duplicating the transaction and the catalog lookup.

**Alternative rejected**: reusing `IDemoHistoryStore`. It would make the coverage rule's port list and the fences
ambiguous, and ADR 0021 calls demo report loading a capability "with the same contract", not the same port.

## R-10 Host: the fourth startup task and its log lines (HP-011)

**Decision**: `StartupTasks` gains a step "Demo report loading" after "Demo history loading", built the same way:

- a `RunStepAsync` that logs a fault as "Demo report loading failed; the app starts without it" (ADR 0007, no
  secret, no value);
- `LoadDemoReportAsync`, which reads the demo password from the same configuration key;
- new `[LoggerMessage]` methods:
  - "Demo report loading: {Result}" for a refusal;
  - an account line for `demo5@example.com`;
  - for a created account, a "Demo goals …" and a "Demo records …" line, each with the email and the result text.

These are the formats of demo history loading's lines. Under ADR 0015 the deployment counts as successful only
when its smoke check passes, and the smoke check needs `demo5` (R-11). So the hosted app's success requires demo
report loading's result without a new workflow step. The daily refresh empties the database and starts the app,
so it re-creates `demo5` (slice 5 HP-009).

## R-11 Smoke check (HP-008)

**Decision**:

- **New `PageReader` members.** The design's GoalStats card is a `div.ht-card` with an `h3` title, and StatTiles
  have no title, so the existing `ReadCardText` (a `section.ht-card` with an `h2`) cannot find either. Three readers
  of leaf elements are added, each with `PageReaderTests` cases:
  - `ReadStatLabels(html)`: the text of every `.ht-stat__label` (GoalStats: "Consistency", "Streak, commitments",
    "Longest, commitments");
  - `ReadTileLabels(html)`: the text of every `.ht-tile__label`;
  - `ReadStatusChips(html)`: the text of every `.ht-status` chip.
- **Matching.** Each expected text must equal a whole label or chip text, compared ordinally with the existing
  `PageReader.ContainsAll`, as checks 1 to 6 do. A unit is therefore checked through the whole stat label that
  carries it: "Streak, commitments" and "Streak, weeks". A part of a label never counts.
- **Check 6 (demo history, `demo4`)** gains three texts, which `SmokeCheckHostTests` counts in its line "(N of N)":
  - the stat labels "Consistency" and "Streak, commitments" (HP-008's word "Consistency" and unit "commitments"),
    matched only among the stat labels of "Reading, 20 min daily"'s page, so the History card cannot supply them;
  - "Due now", matched only among the Home's tile labels.
- **A new check 7, "demo report" (`demo5@example.com`)**, signs in with the demo password and finds:
  - on `/goals`, the name of each of its three goals (`PageReader.ReadGoalNames`);
  - on the page of "Strength training, 2 times a week", the stat label "Streak, weeks" (HP-008's unit "weeks"), so
    the trend's sub line "last 12 weeks" cannot satisfy it;
  - on the Home, "Missed this week" among the tile labels, and "Missed" among the status chips. On the Home only
    Today rows carry chips; GoalSummary and Recent notes have none.
- **Output.** Like check 6, check 7 prints only its name, the demo email and the count: `PASS demo report
  demo5@example.com (N of N)`. It never prints a goal name, a note or a figure (slice 3 smoke-check contract).
  `SmokeCheckHostTests` asserts that line.
- **Coverage.** `SmokeCheckCoverage` gains 4 entries for `IDemoReportStore.CreateDemoReportAccountAsync`: email,
  password, profile and goals, mapped to check 7. `CoverageTests.Ports` gains `typeof(IDemoReportStore)`, and the
  exact entry count grows from 10 to 14.
- **Tests that change with it.** `RunnerBehaviourTests.GoodSite()` scripts demo5's pages, and its exact check
  lines and their positions change. The coverage table in `specs/003-demo-hosting/contracts/smoke-check.md` gains
  the four rows and check 7.
- **Order.** The checks read pages, so they and their host test change only after the pages exist (plan, Work
  order step 7). `SmokeCheckHostTests` runs the real smoke check against a real host, so changing it earlier would
  leave the suite red.

**Rationale**: these are presence checks only (HP-008, SC-006). The figures are fixed by the acceptance tests at
fixed run times. Scoping each text to its card keeps a check from passing on a word that appears elsewhere.

## R-12 Pages: interactive islands where a dialog opens, static where none does

**Decision**:

- **Goal page.** `GoalDetailBody` (already interactive) adds two components above the History card: `GoalStats`
  (with `ConsistencyStrip` and its legend) and `TrendChart`. They come from one `GetGoalReportAsync` call next to
  slice 5's history read. After a record is saved in the dialog, the body reloads the report too, so the figures
  follow a correction on the same page (FR-008). A goal with no begun entry renders neither card (HP-001).
- **Goals page.** It stays a static page with an interactive island, `GoalsList`.
  - The island renders one full GoalListItem per `OverviewGoal`, in the order of FR-011.
  - "Record" opens slice 5's `RecordDialog` in its page-head mode, with `OverviewGoal.Recordable` and its "For"
    select (R-07). After a save the island reads the overview again.
  - The footnote is static.
- **Signed-in Home.**
  - The page head stays static (slice 4).
  - `TodayPanel` is an interactive island that holds the four `StatTile`s, the Today card, `RecordDialog` and
    `NoteDialog`. A tile is a `<button>` with `aria-pressed` that filters the list in the island. Each row opens its
    dialog from the entry's own `Goal` and `Entry` (R-06).
  - **Each island reads its own data.** It takes no data as a parameter: the acting user comes from the
    revalidating authentication state. It calls `GetDashboardAsync` (or `GetGoalsOverviewAsync`) in
    `OnInitializedAsync`, as `GoalDetailBody` calls its reads. A result cannot be passed from the static page into
    an island, because `GoalTarget` inside `GoalHistory` has no public constructor to deserialize.
  - `GoalSummary` and the "Recent notes" card are static. The page renders them from its own `GetDashboardAsync`
    call, so a Home visit reads the dashboard three times: the page, the island's prerender and the island's
    circuit. Each read is a few queries at demo scale (R-08).
  - The island reads the dashboard again after a write. The static cards update on the next page load, which is
    acceptable: FR-008 asks for "the next read".
  - Prerendering stays on, so the first HTML has every row, which the smoke check reads.
- **Visitor Home.** The hero gets a static `GoalStats` card labelled "Example goal". It renders
  `ExampleGoal.Stats` and `ExampleGoal.Strip`, an Application static that runs `Consistency.Of`, `Streaks.Of` and
  `ReportText` over HP-004's fixed 21 states. It reads no store, so it is the same for every visitor (D-3).
- **Shared components**, each implemented once (constitution IV):
  - `GoalStats` takes a `GoalStatsView`, a list of strip states (oldest first) and a unit. The goal page passes
    `Recent.Results`' states, and the visitor card passes `ExampleGoal.Strip`. One label path, `ReportText.StripLabel`,
    serves both.
  - `ConsistencyStrip`, `TrendChart`, `StatTile`, `CommitmentRow`, `GoalSummary`.
  - The full GoalListItem replaces the reduced `GoalItem.razor`: the same component, extended, keeps the name
    `GoalItem`. Slice 4 named it that so the view `HT.Application.Goals.GoalListItem` stays unambiguous, and the
    `GoalsList` island uses both. No reduced form remains, because no slice-6 page renders one. `GoalsList` is its
    only user, and `HabitComponentTests`' reduced test is replaced by full-item tests (spec, tests this slice
    changes).
  - `StatusChip` takes its word from `ReportText.State`, which gives "Upcoming" for Upcoming and slice 5's
    `RecordingText.State` for every other state (R-13). Its class is the design's `ht-status--due`, which
    `design/components/StatusChip.md` already gives the upcoming chip ("nothing to judge yet"). The word tells it
    apart from Due, so no state relies on colour alone.

**Rationale**: slice 5 put interactivity only where a dialog opens (its R-12). The Home and Goals page open
dialogs from rows, so their row lists become islands. The rest stays static, keeping the first paint and the smoke
check simple.

**Alternative rejected**: query-string filters on static pages (`?show=due`). They work for the tiles but not for
the row actions, which open dialogs.

## R-13 Shared formatter `ReportText` (HP-005, constitution IV)

**Decision**: one static class, `HT.Application.Reports.ReportText`, unit-tested, gives every text of this slice:

- `Consistency(Consistency)`: "90% (18 of 20)" or "N/A";
- `Unit(StreakUnit, int n)`: "commitments"/"commitment", "days"/"day", "weeks"/"week";
- `Streak(int n, StreakUnit)`: "6 commitments";
- `StreakLabel(StreakUnit)`: "Streak, commitments", and `LongestLabel`: "Longest, commitments";
- `SummaryLine(Streaks, StreakUnit)`: "Streak 6 commitments · longest 11";
- `NoFinalYet(StreakUnit)`: "No finalized commitments yet";
- `State(EvaluationState, GoalScheduleKind, SchedulePeriod?)`: "Upcoming" for Upcoming, otherwise
  `RecordingText.State`. `RecordingText.State` itself is unchanged, and its test
  `Upcoming_has_no_text_in_this_slice` keeps passing;
- `ProgressText(GoalHistory, HistoryEntry)`: the meta line's progress, "12 of 20 min today", "1 of 3 this week" or
  "2 of at most 3 cups today" (HP-002, HP-003);
- `PendingLeft(DateTimeOffset graceEnds, DateTimeOffset now)`: "{N} h left to record", with N from a new public
  member `RecordingText.HoursLeft(graceEnds, now)`. That member holds the whole-hours, rounded-up rule now inside
  `ForLabel`, and `ForLabel` calls it, so the rule exists once. Its existing tests keep passing, and `HoursLeft`
  gets its own;
- `TrendTitle(GoalTarget, TrendKind, TrendSpan)` and `TrendSubLine(GoalTarget, TrendSpan, int maxBars)`: the
  design's titles and sub lines. The goal's target tells every case apart:
  - its schedule kind separates a per-week yes/no goal ("Times per week") from a commitment-based yes/no goal
    ("Days done per week");
  - its days separate a goal on all seven days ("last 14 days") from one on fewer days ("last 14 commitments");
  - its measurement and count unit give the title of an amount bar ("Minutes per day", "Pages per day").

  The exact wording per case is fixed in the TrendChart design in Work order step 1, and `ReportTextTests` covers
  every case;
- `TargetLine(TrendKind, int target)`: "Target {n} min" for `Minutes`, "Target {n}" otherwise;
- `StripLabel(IReadOnlyList<EvaluationState>, StreakUnit)`: "Last 28 commitments: 24 met, 1 failed, 1 missed, 2
  pending";
- `TrendLabel(TrendView)`: "Minutes per day, last 14 days; target 20", with each bar whose own target differs
  named with it.

`GetGoalReportAsync` fills `TrendView.Title`, `SubLine`, `TargetLine` and `Label` from these functions while
building the view, from the domain `Trend` and the goal's target. Nothing is circular. The use cases also put
`ConsistencyText`, `CurrentStreakText` and `LongestStreakText` into their views (FR-002, FR-004), so the acceptance
tests check the texts without a page. Dates come from the existing `DateTexts` (`ShortDate` "Tue 17 Nov",
`WeekRange` "10 – 16 Aug").

## R-14 Tests

**Unit (`HT.UnitTests`)**:

- `Domain/Reports`:
  - `Consistency.Of`: the rounding cases 2 of 3, 1 of 3, 199 of 200 and n of n; "N/A"; every state skipped or
    counted.
  - `Streaks.Of`: runs, resets, Pending and Due inside a run, the latest Failed or Missed giving 0, and no final
    result.
  - `StreakUnits.Of`.
  - `TrendBuilder` for each row of R-05:
    - null versus 0, and values above the target;
    - Reduce yes/no target 0, and Build duration period records reaching the amount;
    - hours in minutes, `Minutes` versus `Count`;
    - the 12 and 14 caps;
    - the current week of a Tuesday-and-Thursday goal on a Monday;
    - a smaller first-week target.
- `Domain/Goals`: `EntriesThrough` across a 364-day window edge for a per-week and a per-day goal (nothing doubled,
  nothing missed), and a goal 800 days old.
- Boundaries (constitution II): just before, at and after a grace end, at, below and above the target, for each
  schedule kind and for Build and Reduce, including both Kyiv and Berlin clock changes (SC-003).
- `Application/Reports`:
  - `ReportService` with `FakeGoalStore`, `FakeRecordStore`, `FakeAccountStore` and `FakeTimeProvider`: refusal
    order, owner scoping, ordering, the Today list parts, Upcoming, "this week", notes, FR-013;
  - `OverviewGoal.Recordable` and `TodayEntry.Goal` equal to what `GetRecordableAsync` gives at the same instant;
  - correction equals on-time (SC-004);
  - `ReportText`, including `State` for Upcoming and `TargetLine` for a count goal ("Target 1");
  - `ExampleGoal`.
- `Application/Records`:
  - `RecordingText.HoursLeft`;
  - `RecordingService`'s and `RecordingText`'s existing tests pass unchanged after `EntryViews` and `HoursLeft`
    are extracted.
- `Application/Goals`: `GoalService`'s existing list tests pass unchanged after `GoalListItems` is extracted.
- `Application/Operator`: `DemoReport.For(R)` for each weekday, checking the records and notes and FR-032's figures
  through `ReportService` on fakes; `DemoReportService`'s results.

**Architecture (`HT.ArchitectureTests`)**: the existing generic rules already reach the new types, with nothing to
add:

- `ClockReadTests` forbids clock and culture reads in every Domain and Application type.
- `PublicSurfaceTests` keeps NodaTime out of every public signature.
- `DomainDependencyTests` keeps Domain free of the other layers.

New or changed rules:

- `ApplicationDependencyTests`:
  - the `IGoalStore` fence admits `HT.Application.Reports.ReportService`;
  - a new fence keeps `IDemoReportStore` to `HT.Application.Operator.DemoReportService`;
  - `NoOperatorTypeOfApplicationMayDependOnTheGoalStorePort` stays.
- `UiDependencyTests`: HT.Ui stays away from `IDemoReportStore`, and from `DemoReportService` outside
  `HT.Ui.Operator`, as for the demo history port and service.
- `UseCaseParameterTests`: covers `ReportService` (acting user first).

**Integration (`HT.IntegrationTests`)**:

- `Reports`: the three use cases on SQL Server with real stores, scoped by owner. SC-007 is a timed test: 3 goals,
  one created a year earlier at a supplied time, with 366 daily records, each read under 2 seconds.
- `Operator`: demo report loading on an empty and a non-empty database, the not-Published habit left out,
  rollback on fault, "already exists". `DemoHistoryStore`'s existing tests pass unchanged after `DemoAccountWriter`
  is extracted.
- Log capture: no note, value, unit, goal or habit name in any log line of a report read or of demo report
  loading (HP-007).

**UI (`HT.UiTests`)**:

- bUnit for every new component.
- Host tests:
  - the goal page: cards absent with no begun entry;
  - the Goals page: order, footnote, "Record" primary, secondary or absent, the dialog's "For" select;
  - the signed-in Home: tiles, the filter and `aria-pressed`, rows and actions, the empty Today sentence, Upcoming
    rows, GoalSummary at most 5 with "All goals →", the notes card absent with none;
  - the visitor Home: the example card, with and without Published habits.
- These existing tests change as the spec lists:
  - `HabitComponentTests`' reduced GoalListItem test, replaced by full-item tests (R-12);
  - `StatusChipTests`, gaining "Upcoming";
  - `HomeVisitorTests.US5_AS8_HP_012_…`;
  - `HomeSignedInTests` and `GoalsPageTests` for their slice-4 states;
  - `GoalDetailPageTests` for the slice-5 state;
  - `StartupLogTests`;
  - `StartupTasksDemoTests`: three refusal lines, demo5's lines after demo4's, and the history-fault test expecting
    demo5's lines after the fault, plus a new demo-report fault test;
  - `SmokeCheckHostTests`, `RunnerBehaviourTests` and `CoverageTests` (Work order step 7).

**Acceptance**: F007's 67 locked scenarios are written by `/speckit-ac-tests-write` after implementation. The
critic note on AC-027 and AC-040 (supplied time going back between steps) needs nothing from production code: every
use case reads `TimeProvider` once per call.

## R-15 Privacy and logging (FR-014, FR-015, HP-007)

**Decision**:

- Every `ReportService` call resolves the owner from the acting user and passes it to every store call.
- The goal report finds the goal only among the owner's goals. The overview and the dashboard take no identifier
  (FR-014).
- No result carries another account's data, because the stores return only the owner's rows.
- Views carry names, notes and figures only for the owner.
- `ReportService` logs nothing. An exception from a store keeps slice 5's rule: it names no value.
- The visitor Home's card reads no store (D-3).
