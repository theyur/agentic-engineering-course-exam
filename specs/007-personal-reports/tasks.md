---

description: "Task list for slice 6, personal reports"
---

# Tasks: Personal reports

**Input**: Design documents from `specs/007-personal-reports/`: [plan.md](plan.md), [spec.md](spec.md),
[research.md](research.md), [data-model.md](data-model.md), [contracts/application-api.md](contracts/application-api.md),
[contracts/host.md](contracts/host.md), [quickstart.md](quickstart.md); [ADR 0020](../../docs/adr/0020-derived-figures-on-read.md),
[ADR 0021](../../docs/adr/0021-demo-report-loading.md), [ADR 0018](../../docs/adr/0018-evaluation-on-read-and-demo-history.md).

**Prerequisites**: `acceptance.md` and `tests/HT.AcceptanceTests/Features/F007/` are locked. No task edits them,
the lock files, the run records or anything under `tests/HT.AcceptanceTests`.

**Tests**: required. The constitution (II, III, V) and `CLAUDE.md` make unit, integration, architecture and
selective UI tests part of done, so every story phase has test tasks. xUnit v2 everywhere, hand-written fakes,
`FakeTimeProvider`, no mocking library (ADR 0006). Acceptance step definitions are not tasks here: the owner starts
`/speckit-ac-tests-write` and `/speckit-ac-tests-analyze` after `/speckit-implement` (plan, Work order 9).

**Organization**: Phase 2 holds what several stories share:

- the whole design step (plan, Work order 1);
- the whole of `HT.Domain.Reports` and `GoalTimeline.EntriesThrough`, with their boundary tests (Work order 2);
- the Application parts every use case shares: the three extractions (`EntryViews`, `GoalListItems`,
  `RecordingText.HoursLeft`), the views and results, `ReportText` and `GoalEvaluation`;
- `StatusChip`'s "Upcoming" and `ConsistencyStrip`, which the goal page, the Goals page, the Home and the visitor
  Home all use.

`ReportService` itself is created in US1, with its first use case and its registration. Its other two use cases
come in US5 and US6. Each story phase then adds its use case, its tests, its components and its page. Inside a
story the plan's layer order holds: Application, then the components, then the page, then the host tests.

The story phases follow the spec's priorities, with two moves, each because a later story tests what an earlier
one builds:

- P1 runs US1, US2, US3, then US4 (P2, recent results and trend), then US5 and US6. US4 moves up because it
  finishes the goal page that US1 to US3 build, and US3 and US4 both edit `GoalDetailBody.razor`.
- US7 (P1, privacy) comes after US6, because its tests cover all three use cases of `ReportService`.
- US8 (P2, demo reports) comes last. Its smoke check reads the goal page, the Goals page and the Home, and its
  demo figures are read through all three use cases.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: the task can run in parallel with the other [P] tasks of its group (they touch different files), once
  the tasks it names as dependencies are done.
- **[Story]**: the user story from spec.md (US1 to US8).
- Paths are relative to the repository root.

## Conventions for every task

- Build with `dotnet build HabitTracker.slnx`. `TreatWarningsAsErrors` is on, so a warning is a failure.
- Use the names in [contracts/application-api.md](contracts/application-api.md) and [data-model.md](data-model.md).
  Every use-case method of a service and every port method is `async` and takes a `CancellationToken` last. The
  pure formatters and fixed data (`ReportText`, `RecordingText.HoursLeft`, `ExampleGoal`, `DemoReport.For`) and the
  Domain functions are synchronous, as the contract gives them. Services use an
  explicit constructor with private fields, not a primary constructor (an unread primary-constructor parameter is
  CS9113, an error here).
- The design artifacts stay the contract (constitution, Governance: "never by silent divergence"). A task that adds
  a public member, or settles a text or layout the design artifacts leave open, records it in the same task, in the
  document it names. The TrendChart's titles and sub lines per goal kind (T005) and the exact Today-row meta lines
  (T007) are settled in `design/` first, and the code follows them.
- Take message texts from the existing constants (`GoalTexts`, `RecordTexts`, `AccountTexts`, `OperatorTexts`).
  Never retype one. Tests assert the literal text from spec.md, so a typo in a constant fails a test.
- Every consistency, streak, unit, target-line, strip and trend text comes from `ReportText` (HP-005). No component
  builds one, and no component computes a figure. Each value renders whole, as one `ReportText` text: "90% (18 of
  20)" and "6 commitments" stay one text in `ht-stat__value`, `ht-goalitem__value` and `ht-goal__pct`. The full
  previews split the count into a `<small>` ("90% <small>(18 of 20)</small>", "6 <small>commitments</small>"), and
  `ReportText` has no member for either part. So every slice-6 state shows the whole text and says so in its `.md`:
  the components of T004, T006 and T008, and the pages built from them, PageGoalDetail (T003), PageGoals (T006),
  PageHomeSignedIn's "Active goals" card (T007) and PageHomeVisitor's hero card (T009). No component splits one.
- Time and culture stay out of `HT.Domain` and `HT.Application` (constitution II; `ClockReadTests`):
  - Time enters only as `TimeProvider` in the services, read once per call as `_clock.GetUtcNow()`, and as an
    instant or date parameter in Domain.
  - "This week" starts at Monday 00:00 in the account's zone, turned into a UTC instant through `NamedTimeZone`.
    Never add days in local time.
  - Date texts come only from `DateTexts` (`ShortDate`, `WeekRange`). Numbers are formatted with the invariant
    culture.
- No NodaTime type appears in any public or protected signature of `HT.Domain` or `HT.Application` (ADR 0017).
- Reports are private (constitution III, FR-014, FR-015):
  - They read goals, records and notes only through the owner-scoped `IGoalStore.FindAsync`, `IGoalStore.ListAsync`
    and `IRecordStore.ReadAsync`, always passing the acting user's `AccountId` as the owner.
  - The checks run in the order: no account id, role, stored account, goal. Administrators get no figure of any
    account.
  - No note, recorded value, unit, goal name, habit name or figure goes into a log template, an exception message
    or an error page (HP-007). `ReportService` logs nothing.
- Nothing derived is stored or cached (HP-009, ADR 0020). Within one call each goal is evaluated once, by one
  `GoalEvaluation`.
- Test instants are UTC literals worked out by hand from the spec's local times, never computed by the code under
  test. For example:
  - "Missed this week", account zone UTC: Friday 13 November 2026's grace period ends at `2026-11-16T00:00:00Z`,
    which is Monday 00:00, so read at that instant it is Missed this week; Thursday 12 November's ends at
    `2026-11-15T00:00:00Z` and is not (US6-AS4);
  - Europe/Kyiv, change back: Friday 23 October 2026 ends at `2026-10-23T21:00:00Z`, and its grace period ends at
    `2026-10-25T21:00:00Z`, which is Sunday 23:00 Kyiv time; the week starts at `2026-10-25T22:00:00Z` (Monday
    00:00), so on that Monday it is not Missed this week;
  - Europe/Kyiv, change forward: Friday 26 March 2027 ends at `2027-03-26T22:00:00Z`, and its grace period ends at
    `2027-03-28T22:00:00Z`, which is Monday 01:00 Kyiv time; the week starts at `2027-03-28T21:00:00Z`, so it is
    still Pending at `2027-03-28T21:30:00Z` and Missed this week from its grace end on;
  - Europe/Berlin has the same weekends one hour later in UTC (Friday 23 October 2026 ends at
    `2026-10-23T22:00:00Z`);
  - D-1, zone UTC: a day of 10 November 2026 with a record that reaches the target is Pending at
    `2026-11-12T23:59:59Z` and Met at `2026-11-13T00:00:00Z`.
- Goals with past results are created at an earlier supplied time, then recorded and read at later ones, by moving
  a `FakeTimeProvider` forward. A goal's start date is never in the past (slice 4).
  - In unit tests each test owns its world and its clock.
  - In integration tests, never set or advance the fixture's shared `Clock`. `DatabaseFixture.Clock`
    (`2026-11-11T09:00Z`) and `SeedDatabaseFixture.Clock` (`2026-11-14T09:00Z`) are each one instance for the whole
    collection. `FakeTimeProvider.SetUtcNow` cannot go back, and existing tests hard-code those dates
    (`GoalCreationTests`, `DemoGoalsTests`, `DemoHistoryLoadingTests`). Moving it would make them fail depending on
    test order.
  - A test that needs another time builds its own provider with its own `FakeTimeProvider` over the fixture's
    `ConnectionString`, as `Records/RecordReadTests.BuildServices` does, and moves only that clock.
  - On `HT-IT-SEED`, such a test first resets the database with `HtDatabase.ResetAsync(database.ConnectionString,
    …)`, as `ResetAndBuildAsync` does, then builds its own provider.
- Pages use only `design/` tokens and `ht-` classes, and follow the structure and wording of their `design/` preview
  after the design tasks of Phase 2 (constitution IV, HP-006). Parts of slices 7 to 9 are absent, not disabled
  (ADR 0008): no Excused or Paused cell, no "Excuse" action, no Tabs, no earlier attempts.
- Interactive components call use cases only through `UseCaseScope` (slice 2 research R-15), as `GoalDetailBody`
  does. Static pages inject the services. An island reads its own data in `OnInitializedAsync`; a static page never
  passes a view into an island (research R-12).
- Every island that opens a dialog gives the focus back to the opener when the dialog closes, saved or not
  (constitution IV). A re-read after a save replaces the rows, so an `ElementReference` taken before it is stale.
  The island passes `FocusOnClose` to `RecordDialog` and `NoteDialog`. After the re-read it finds the child
  component again by its key (a `@ref` per `GoalItem` by goal identifier, per `CommitmentRow` by goal and first
  date) and calls that component's `FocusAsync` (T050, T055, T056), as `GoalDetailBody` does with `FocusRowAsync`,
  `FocusNoteRowAsync` and `HistoryCard.FocusAsync`. bUnit tests check it
  with `JSInterop.VerifyFocusAsyncInvoke`, as `GoalDetailPageTests` does.
- Never read, print or embed a secret. That includes the `Operator:*` User Secrets values.
- No `InternalsVisibleTo` in `HT.Domain`, `HT.Application` or `HT.Persistence` (ADR 0010; `AcSurfaceTests` enforces
  it). Tests use public members only. `GoalEvaluation`, `EntryViews`, `GoalListItems`, `GoalOrder` and
  `DemoAccountWriter` are internal and are tested through the public use cases and stores.
- Databases:
  - integration tests use `HT-IT` (`DatabaseFixture`) and `HT-IT-SEED` (`SeedDatabaseFixture`);
  - host tests use `HT-UI` and its derived databases;
  - no test touches `HT` or `HT-AC`.

  Tests in shared databases use account emails, habit names and goal names with their own prefix. They check by
  inclusion, exclusion and relative order, never by totals.
- Commit only at a checkpoint, with the full suite green (constitution V). These land in the same commit, so
  `ValidateOnBuild` never meets an unregistered service or port (plan, Work order 3 and 4):
  - `ReportService`, its registration and its bUnit registration (US1, T034);
  - `DemoReportService`, `IDemoReportStore`, `DemoAccountWriter`, `DemoReportStore` and their two registrations
    (US8, T072 to T074).

  The bUnit services in `tests/HT.UiTests/Components/Pages/PageServices.cs` are registered by hand and are not
  built with `ValidateOnBuild`. They hold what the pages resolve: `ReportService` joins them, but no operator
  service or demo store does, just as `FakeDemoHistoryStore` is not there.
- No existing constructor, method, port member or result type of `HT.Domain`, `HT.Application` or `HT.Persistence`
  changes (FR-016, plan "Risks"). The only new members of existing types are `GoalTimeline.EntriesThrough` (T016)
  and `RecordingText.HoursLeft` (T026), and the extractions move private code without changing behaviour. If a task seems to need more, stop and ask the owner.
  The locked F001, F002, F004 and F005 AC support code must keep compiling, and nobody reads it. The smoke check
  tool (`tools/HT.SmokeCheck`) is outside that rule.
- A search for callers or tests of a type excludes the AC project:
  `grep -rnw "<Type>" tests --include=*.cs --exclude-dir=HT.AcceptanceTests`. The `--exclude-dir` is required:
  without it the search enters the AC project's off-limits folders (`CLAUDE.md`).
- The pre-commit architecture gate stays on. Never use `--no-verify`.

---

## Phase 1: Setup (shared infrastructure)

**Purpose**: the roadmap state and a baseline of the public API.

- [X] T001 Set slice 6's Status to `implementing` and `updated_at` to today in `docs/roadmap.md`. The row still reads `clarified`. The statuses `acceptance locked`, `features locked`, `planned` and `tasks ready` were not recorded on this branch, against "Updating this file" ("after each step"). The branch's commit history records those steps, and this change moves the row to its current state in one edit
- [X] T002 [P] Save the public API baseline before any code change (plan "Risks", FR-016):
  - run `dotnet restore HabitTracker.slnx`;
  - write `roslynator list-symbols HabitTracker.slnx --projects HT.Application --visibility public --depth member`, and the same for `HT.Domain` and `HT.Persistence`, into three files in the session scratchpad, never in the repo. T084 compares them.

  The scratchpad does not survive a reboot or a later session. If T084 finds the files missing, it regenerates them from the branch's merge base with `master` (`git merge-base HEAD master`) in a temporary `git worktree` under the scratchpad (restore, then the same commands), and removes that worktree afterwards.

**Checkpoint**: `dotnet build HabitTracker.slnx` passes.

---

## Phase 2: Foundational (blocking prerequisites)

**Purpose**: everything several stories need:

- the design changes;
- `HT.Domain.Reports` and `GoalTimeline.EntriesThrough`, with their boundary tests;
- the shared Application types, the three extractions, `ReportText` and `GoalEvaluation`;
- `StatusChip`'s "Upcoming" and `ConsistencyStrip`.

**⚠️ CRITICAL**: no story work starts until this phase is complete.

### Design first (constitution IV, ADR 0008; spec "Design changes before build")

Two kinds of edit, as in slices 1, 2, 4 and 5:

- Texts that disagree with the spec or `CONTEXT.md` are **replaced** everywhere: "Due Mon" on the Upcoming chip
  becomes "Upcoming", the first tile's "Due today" becomes "Due now", and TrendChart's "Use weeks for weekly and
  commitment-based goals" and "Bars show amounts, not verdicts" become the spec's wording.
- A **slice-6 state** is **added** beside the full preview, as a "Slice 6 state" block after it. The full previews
  stay whole, because they are the design source of slices 7 to 11. "No …" in a task below describes the slice-6
  state; it never deletes anything from the full preview.

- [X] T003 [P] Add the slice-6 state to `design/pages/PageGoalDetail.html` and `.md` (HP-001, US1-AS8, US2-AS8, US4-AS8): in the left column, above the History card, the GoalStats card, then the TrendChart card; each stat value one whole text (Conventions); the strip's legend without Excused and Paused; no pause or attempt parts. Note that a goal with no begun entry keeps the slice 5 page without these two cards
- [X] T004 [P] Add the slice-6 state to `design/components/GoalStats.html` / `.md` and `ConsistencyStrip.html` / `.md` (HP-001, HP-006): only Met, Failed, Missed and Pending cells and legend entries; the eyebrow "This attempt", the title "Consistency and streaks", the stats "Consistency", "Streak, {unit}" and "Longest, {unit}" with the plural unit; the strip's `role="img"` and its counting `aria-label` ("Last 28 commitments: 24 met, 1 failed, 1 missed, 2 pending"); each stat value as one whole text ("90% (18 of 20)", no `<small>` part; Conventions); the mini variant `ht-strip--mini` of 15 cells with no legend, whose `aria-label` also counts each state ("Last 15 commitments: 12 met, 1 missed, 2 pending"), replacing the full preview's "Last 15 results" in the slice-6 state (HP-006); the visitor variant labelled "Example goal" with a goal title (HP-004)
- [X] T005 [P] Change `design/components/TrendChart.html` and `.md` (FR-006, HP-001, research R-05, R-13, plan "Spec discrepancies" 2):
  - one preview per bar kind: per-day amount bars for a Reduce duration goal ("Minutes per day · last 14 days", as "Screen time, at most 120 min a day"), per-day record-count bars for a Build goal (its title from the table below), per-week ("Times per week · last 12 weeks"), commitment-based yes/no ("Days done per week · last 12 weeks"), and commitment-based count or duration ("… · last 14 commitments" for a goal on fewer than seven days);
  - a table that fixes the title and the sub line for every case `ReportText.TrendTitle` and `TrendSubLine` must give: Build and Reduce, yes/no, count (with its unit, "Pages per day") and duration (minutes and hours), commitment-based on all seven days or fewer, per-day and per-week. T028 implements exactly this table;
  - an empty bar for no value (not a zero-height bar), drawn with a token-only class; no paused bars until slice 7;
  - the rule "weeks for per-week goals and commitment-based yes/no goals; commitments or days otherwise";
  - the wording "Bars show recorded progress, an amount or a number of records, not verdicts";
  - the dashed target line "Target {n} min" for minutes and "Target {n}" otherwise, drawn at the latest bar's target, and the note that each bar keeps its own target, named in its tooltip and in the chart's `aria-label` ("week of 2 Nov: 2 of 2");
  - the chart's `role="img"` and its `aria-label` ("Minutes per day, last 14 days; target 20")
- [X] T006 [P] Add the slice-6 state to `design/pages/PageGoals.html` / `.md` and `design/components/GoalListItem.html` / `.md` (HP-002): no Tabs; the name link, the Build or Reduce Tag, the Pending StatusChip; slice 4's meta line plus the progress of a Due entry with a record ("12 of 20 min today", "1 of 3 this week"), no "missed Tue"; "Consistency" and "Current streak", each value one whole text ("90% (18 of 20)", "6 commitments"; Conventions); the mini strip `ht-strip--mini` of 15 cells with T004's counting label; the action "Record" as primary, as secondary, or absent; the footnote "Each goal is counted on its own. Consistency counts only finalized results; pending ones are left out."
- [X] T007 [P] Add the slice-6 state to `design/pages/PageHomeSignedIn.html` / `.md`, `design/components/CommitmentRow.html` / `.md` and `StatTile.html` / `.md` (HP-003, HP-006, D-2):
  - four tiles "Due now", "Pending · still can record" (`ht-tile--pending`), "Missed this week" (`ht-tile--failed`) and "Active goals"; the first three are buttons with `aria-pressed` and a visible pressed state, "Active goals" is a link to `/goals`;
  - CommitmentRow per state: a Due row (its schedule without a record, its progress with one), a Pending row ("Mon 16 Nov · 5 h left to record"), a Missed row ("Sun 15 Nov · no record"), and an Upcoming row (its date as the meta line, the chip "Upcoming", no action); a week as "16 – 22 Nov"; the progress bar only for count and duration goals; the "Reduce" Tag;
  - the row actions "Record", "Edit", "Add note" and "Edit note" in place of "Add time" and "+1", and no "Excuse";
  - the sentence "Nothing to record right now." for an empty Today card;
  - the "Recent notes" card: each note in quotes with "{goal name} · {date}" under it;
  - the "Active goals" card (GoalSummary) with each consistency as one whole text in `ht-goal__pct` (Conventions)
- [X] T008 [P] Change `design/components/StatusChip.html` / `.md` and `design/components/GoalSummary.html` / `.md` (HP-003, constitution I):
  - StatusChip: the upcoming chip reads "Upcoming" with the class `ht-status--due`, replacing "Due Mon"; the date belongs to the row's meta line;
  - GoalSummary: each goal's name as a link, its consistency as one whole text in `ht-goal__pct` (Conventions), "Streak {n} {unit} · longest {m}", or "N/A" with "No finalized {unit} yet" per unit; at most 5 goals, then "All goals →"
- [X] T009 [P] Add the slice-6 state to `design/pages/PageHomeVisitor.html` and `.md` (HP-004, D-3): the hero's GoalStats card labelled "Example goal", "Yoga, 3 days a week", "90% (18 of 20)" as one whole text (Conventions), streak 6 and longest 11 commitments, the strip of 21 cells (Met × 11, Missed, Met, Failed, Met × 6, Pending) with its legend; it states that the figures are fixed examples that belong to no account, replacing "a real GoalStats card from demo data". The card is part of the hero, not of the catalog preview, so it shows with or without Published habits
- [X] T010 Update `design/pages.md` and `design/README.md` with the slice-6 structure of the four pages (after T003 to T009). Any class that T003 to T009 add to `design/css/ht-components.css` or `tokens.css` (the empty trend bar, the tile as a button with its pressed state) is copied byte for byte to `src/HT.Ui/wwwroot/css/`. Run `DesignSyncTests`

### Domain: reports (`src/HT.Domain/Reports/`, namespace `HT.Domain.Reports`; research R-01, R-05)

- [X] T011 [P] Create `src/HT.Domain/Reports/Consistency.cs`: the record `Consistency(int Successful, int Counted)` with `int? Percentage` (`Successful * 100 / Counted`, integer division; null when `Counted` is 0) and `static Consistency Of(IEnumerable<EvaluationState> states)`: Met is successful; Met, Failed and Missed are counted; Upcoming, Due and Pending are skipped (FR-001, FR-002)
- [X] T012 [P] Create `src/HT.Domain/Reports/Streaks.cs`: the record `Streaks(int Current, int Longest)` and `static Streaks Of(IEnumerable<EvaluationState> statesInDateOrder)`: Met adds 1 to the run, Failed or Missed ends it, Upcoming, Due and Pending are skipped and neither extend nor reset a run (FR-003, D-1)
- [X] T013 [P] Create `src/HT.Domain/Reports/StreakUnit.cs` (`Commitments`, `Days`, `Weeks`) and `src/HT.Domain/Reports/StreakUnits.cs` with `static StreakUnit Of(GoalTarget target)`: commitment-based gives `Commitments`, a per-day period `Days`, a per-week period `Weeks` (FR-004)
- [X] T014 [P] Create `src/HT.Domain/Reports/TrendKind.cs` (`Minutes`, `Count`, `RecordCount`), `TrendSpan.cs` (`Commitment`, `Day`, `Week`), `TrendBar.cs` (record `FirstDate`, `LastDate`, `int? Value`, `int Target`) and `Trend.cs` (record `Kind`, `Span`, `IReadOnlyList<TrendBar> Bars`, `int Target` = the latest bar's target or 0 with no bar, `int MaxBars`) (data-model.md)
- [X] T015 Create `src/HT.Domain/Reports/TrendBuilder.cs` with `static Trend Of(GoalTarget latest, IReadOnlyList<(CalendarEntry Entry, GoalTarget Target, IReadOnlyList<RecordValue> Records)> begun, IReadOnlyList<CalendarEntry> currentWeekUpcoming)` (after T014; FR-006, research R-05's table):
  - per-week and per-day periods: the period's progress from `Evaluator.ProgressOf`, against N (Build, Reduce yes/no) or the amount (Reduce count or duration, minutes for durations); the last 12 weeks or 14 days;
  - commitment-based count or duration: the recorded amount as stored (a duration record is already in minutes, `RecordValue`; never convert it) against the target amount, which alone is converted to minutes (× 60 for an hours target, as `Evaluator.AmountInRecordUnits` does); the last 14;
  - commitment-based yes/no: one bar per Monday-to-Sunday week with at least one begun commitment, the "Yes" records counted for Build and Reduce; the target is the week's commitments on or after the start date, `currentWeekUpcoming` included (Build), or 0 (Reduce); the last 12;
  - a bar with no record has a null value; a bar with a record has its value, 0 included; values above the target are kept; records of every state count;
  - each bar's target comes from its own entry's `GoalTarget` (the Monday's for a week); `Kind` is `Minutes` for a commitment-based duration goal or a Reduce duration period, `Count` for a commitment-based count goal or a Reduce count period, `RecordCount` otherwise.

  If `Evaluator.ProgressOf` does not give a period's progress in the form needed, stop and ask the owner rather than change it
- [X] T016 [P] Add `public static IReadOnlyList<CalendarEntry> EntriesThrough(Goal goal, DateOnly last)` to `src/HT.Domain/Goals/GoalTimeline.cs` (FR-001, research R-02): every commitment or period from the start date (a per-week goal: its first week) to the entry that contains `last`, in date order, read through `TryCalendar` in 364-day windows that each start on a Monday, so no week is doubled or missed; empty when `last` is before the first entry. `TryEntries`, `MaxCalendarDays` and every other member are unchanged
- [X] T017 [P] Write `tests/HT.UnitTests/Domain/Reports/ConsistencyTests.cs` (after T011; FR-001, FR-002, US1-AS2 to AS4): 18 Met, 1 Failed, 1 Missed gives 90, 18 and 20; Pending, Due and Upcoming change nothing; no counted state gives a null percentage; 2 of 3 is 66, 1 of 3 is 33, 199 of 200 is 99, n of n is 100; 0 of 2 is 0
- [X] T018 [P] Write `tests/HT.UnitTests/Domain/Reports/StreaksTests.cs` (after T012; FR-003, US2-AS1 to AS5, AS7): Met, Met, Failed, Met, Met, Met gives 3 and 3; Met × 11, Missed, Met × 6 gives 6 and 11; a latest Failed or Missed gives 0 and keeps the longest; Met, Met, Pending, Due gives 2; Pending and Due inside a run do not break it; no final state gives 0 and 0; a Missed between two runs corrected to Met joins them (Edge Cases)
- [X] T019 [P] Write `tests/HT.UnitTests/Domain/Reports/StreakUnitsTests.cs` (after T013; FR-004): a fixed-days goal, an N-per-day goal and an N-per-week goal, Build and Reduce
- [X] T020 [P] Write `tests/HT.UnitTests/Domain/Reports/TrendBuilderTests.cs` (after T015; FR-006, US4-AS3 to AS7, research R-14): one case per row of research R-05; null versus 0; 45 against 20; a Reduce yes/no week with one "Yes" and two "No" gives 1 against 0; "30 min, 3 times a week" with 30, 25 and 40 gives 2 against 3; a goal in hours gives minutes and `Minutes`; a count goal gives `Count`; the 12 and 14 caps keep the latest bars; a Tuesday-and-Thursday goal read on a Monday has the previous week as its latest bar; a smaller first week (2, 3, 3) with `Trend.Target` 3; an unrecorded Reduce day or week has no value
- [X] T021 [P] Write `tests/HT.UnitTests/Domain/Goals/GoalTimelineEntriesThroughTests.cs` (after T016; research R-02): a per-week and a per-day goal across a 364-day window edge (nothing doubled, nothing missed, dates strictly increasing); a goal 800 days old (every entry from its start); a per-week goal started mid-week (the days before its first week belong to no entry); `last` before the first entry gives none; a time zone change with an overlap after a move east keeps both entries in date order (slice 4 FR-027)
- [X] T022 [P] Write `tests/HT.UnitTests/Domain/Reports/ReportBoundaryTests.cs` (after T011, T012, T016; constitution II, SC-003): states from `Evaluator.Evaluate` folded by `Consistency.Of` and `Streaks.Of`, for each schedule kind (commitment-based, per-day, per-week) and for Build and Reduce: just before, at and just after a grace end; at, below and above the target (and for Reduce, at, below and above the limit); the clock-change instants of the Conventions in Europe/Kyiv and Europe/Berlin, change back and change forward, with a 23-hour and a 25-hour day counting once
- [X] T023 [P] Add the Domain rules (after T011 to T016; research R-14): in `tests/HT.ArchitectureTests/DomainDependencyTests.cs`, `HT.Domain.Reports` joins the namespaces that depend only on the Domain and the BCL (`RecordsAndEvaluationDependOnlyOnTheDomainAndTheBcl`, renamed to say so); in `PublicSurfaceTests.cs`, the report types join the rows that check for no NodaTime type in their signatures. `ClockReadTests` needs no change; run it

### Application: shared parts (`src/HT.Application/`; research R-04, R-06, R-07, R-13)

- [X] T024 Move `RecordingService.EntryView` and `RecordingService.HistoryOf`, with the private helpers they need, to a new internal static class `src/HT.Application/Records/EntryViews.cs` as `Of(goal, entry, records, note, now)` and `HistoryOf(goal, currentDate, entries, now)` (research R-04). `RecordingService` calls them; its public members, texts and behaviour do not change. Every existing test under `tests/HT.UnitTests/Application/Records/` passes unchanged
- [X] T025 [P] Move `GoalService.ListItemOf`, with the private helpers it needs, to a new internal static class `src/HT.Application/Goals/GoalListItems.cs` as `Of(stored, now)` (research R-07). `GoalService.ListAsync` calls it and keeps its name order. Every existing test under `tests/HT.UnitTests/Application/Goals/` passes unchanged
- [X] T026 [P] Add `public static long HoursLeft(DateTimeOffset graceEnds, DateTimeOffset now)` to `src/HT.Application/Records/RecordingText.cs`: the whole hours left, rounded up, 0 at or after the end. `ForLabel` calls it, and its output is unchanged. Add `HoursLeft` cases to `tests/HT.UnitTests/Application/Records/RecordingTextTests.cs`: exactly 5 hours, 4 hours and 1 second, 1 second, the end itself, after the end. The existing cases, `Upcoming_has_no_text_in_this_slice` included, stay as they are. Record the member in data-model.md if it is not there
- [X] T027 [P] Create the views and results in `src/HT.Application/Reports/`, sealed records with `required` members as data-model.md gives them (FR-005, FR-007, FR-011, FR-012): `GoalStatsView`, `ResultView`, `RecentResults`, `TrendView`, `GoalReport`, `GoalReportResult` (with `GoalReportOutcome`), `OverviewGoal`, `GoalsOverview`, `GoalsOverviewResult` (with `GoalsOverviewOutcome`), `TodayEntry`, `DashboardCounts`, `DashboardGoal`, `DashboardNote`, `Dashboard` and `DashboardResult` (with `DashboardOutcome`). Results follow ADR 0005: the outcome, the message of a refusal, and the view only when found
- [X] T028 Create `src/HT.Application/Reports/ReportText.cs` with every member of contracts/application-api.md "ReportText" (after T013, T014, T026, T027; HP-005, research R-13): `Consistency`, `Unit`, `Streak`, `StreakLabel`, `LongestLabel`, `SummaryLine`, `NoFinalYet`, `State` ("Upcoming", else `RecordingText.State`), `ProgressText`, `PendingLeft` (through `RecordingText.HoursLeft`), `TrendTitle` and `TrendSubLine` (exactly T005's table), `TargetLine`, `StripLabel` and `TrendLabel` (each bar whose own target differs from `TrendView.Target` named with it). Texts follow `design/brand-book.md`; numbers use the invariant culture
- [X] T029 [P] Write `tests/HT.UnitTests/Application/Reports/ReportTextTests.cs` (after T028; HP-005, SC-002): "90% (18 of 20)", "66% (2 of 3)", "N/A"; every unit at 0, 1 and 2 ("0 days", "1 day", "1 commitment", "1 week"); "Streak, commitments", "Longest, weeks"; "Streak 6 commitments · longest 11"; "No finalized days yet"; `State` gives "Upcoming" for Upcoming and `RecordingText.State`'s word for every other state; `ProgressText` for a duration commitment, a per-week yes/no period and a Reduce count day; `PendingLeft` "5 h left to record"; every row of T005's title and sub line table; `TargetLine` "Target 20 min", "Target 60 min" for an hour, "Target 1" for Sugar and "Target 3"; `StripLabel` "Last 28 commitments: 24 met, 1 failed, 1 missed, 2 pending" (a state with 0 left out, as the design gives it); `TrendLabel` with and without a bar of its own target; a culture with another decimal and list separator gives the same texts (`CultureScope`)
- [X] T030 Create the internal `src/HT.Application/Reports/GoalEvaluation.cs` (after T015, T016, T024, T027; research R-04, data-model.md "Internal: GoalEvaluation"): `Of(...)` for one goal at one instant `now`, from `GoalTimeline.EntriesThrough(goal, currentDate)` and one `StoredEntries` read; each entry evaluated through `EntryViews.Of` with its own version's target (FR-009); `Consistency`, `Streaks`, `Unit`, `Ended`, `Due`, `Pending`, `Notes`, `Trend` (through `TrendBuilder`, with the current week's Upcoming commitments of a commitment-based yes/no goal from `GoalTimeline.TryEntries` up to that week's Sunday) and `NextUpcoming(windowStart)` (the first Upcoming entry whose first date is at most 6 days after `windowStart`). `EntriesThrough` stops at the current date, so `NextUpcoming` does not search `Entries`: it reads its own window `GoalTimeline.TryEntries(goal, windowStart, windowStart + 6)` (research R-06), evaluates those entries at `now` and takes the first Upcoming one. A goal that has not started has no current date, so it has no begun entry and no store read, but `NextUpcoming` still finds its first entry inside the window

### Shared components (`src/HT.Ui/Components/Shared/`)

- [X] T031 Change `src/HT.Ui/Components/Shared/StatusChip.razor` to take its word from `ReportText.State` (after T008, T028; constitution I, research R-12): an Upcoming chip reads "Upcoming" with the class `ht-status--due`; every other state renders exactly as in slice 5. Add the Upcoming case to `tests/HT.UiTests/Components/Shared/StatusChipTests.cs`; its other cases stay
- [X] T032 [P] Create `src/HT.Ui/Components/Shared/ConsistencyStrip.razor` (after T004, T010, T028; HP-001, HP-006): one `ht-strip__cell` per state (Met, Failed, Missed, Pending; the design's cell classes, so no state is shown by colour alone), `role="img"` with its `aria-label` from `ReportText.StripLabel`, and the legend when asked for. It takes the states oldest first, the unit and a `Mini` flag, which adds `ht-strip--mini` and leaves the legend out (T004, T006). The label is `ReportText.StripLabel` in both sizes. Write `tests/HT.UiTests/Components/Shared/ConsistencyStripTests.cs`: 30 and 15 cells in order, each class, the label, the mini class with no legend, the legend's four entries and no Excused or Paused entry

**Checkpoint**: the full suite passes, `RecordingService`'s and `GoalService`'s existing tests unchanged among them. Commit.

---

## Phase 3: User Story 1 - Consistency of a goal (Priority: P1) 🎯 MVP

**Goal**: a registered user reads a goal's consistency with its count, or "N/A", on the goal page.

**Independent Test**: a daily goal with 18 Met, 1 Failed and 1 Missed final days and 2 Pending days reads "90% (18 of 20)".

### Tests for User Story 1

- [X] T033 [P] [US1] Create `tests/HT.UnitTests/Application/Reports/ReportWorld.cs`, building on `tests/HT.UnitTests/Application/Goals/GoalWorld.cs` and `Records/RecordWorld.cs`: goals created through `GoalService` at an earlier supplied time, records added through `RecordingService` or `FakeRecordStore.Seed`, the clock moved forward, reports read through `ReportService`. Write `GoalReportConsistencyTests.cs` with it (after T034; FR-001, FR-002, FR-010, US1-AS1 to AS7): 18 Met, 1 Failed, 1 Missed gives "90% (18 of 20)" with 90, 18 and 20; 2 more Pending and 1 Due change nothing; a goal not started and a goal with only Due and Pending results give "N/A" and no percentage; the four rounding cases; a Reduce goal; a per-week, a commitment-based and a per-day goal each count each final result once; two goals on the same habit each have their own figures; a goal whose catalog habit is then set to Unpublished (`CatalogHabits.TrySetStateAsync`, as `GoalReadTests` does) keeps the same report (spec Edge Cases, slice 4 FR-034)
- [X] T035 [P] [US1] Write `tests/HT.IntegrationTests/Reports/GoalReportReadTests.cs` on `HT-IT` (after T034; FR-007, FR-008): with the real stores and the test's own provider and clock (Conventions; never `database.Clock`), a daily goal created at that clock's start, records added as the clock moves forward over 30 days, then the report read: its figures equal those of the same records on fakes; records and notes of another account's goal on the same habit are never counted
- [X] T036 [P] [US1] Create `src/HT.Ui/Components/Shared/GoalStats.razor` (after T004, T010, T032; HP-001, HP-004, research R-12): the eyebrow, the title, the three stats from a `GoalStatsView` with labels from `ReportText.StreakLabel` and `LongestLabel` (plural unit) and values `ConsistencyText`, `CurrentStreak`, `LongestStreak`, then the `ConsistencyStrip` of a list of states with its legend. It takes the view, the strip states, the unit and the eyebrow and title texts, so the visitor card (T078) uses it too. Write `tests/HT.UiTests/Components/Shared/GoalStatsTests.cs`: "Consistency" with "90% (18 of 20)" and with "N/A", the strip in order, the design's classes (`ht-stats`, `ht-stat__label`, `ht-stat__value`)

### Implementation for User Story 1

- [X] T034 [US1] Create `src/HT.Application/Reports/ReportService.cs` with the constructor of contracts/application-api.md and `GetGoalReportAsync(ActingUser, GoalId, CancellationToken)` (after T028, T030; FR-007, FR-014, FR-015, research R-03):
  - the checks in the contract's order, with the texts from the existing constants;
  - the time read once; the goal found through `IGoalStore.FindAsync(owner, goal)`; one `GoalEvaluation`;
  - the whole `GoalReport`: `Stats` (FR-002 to FR-004), `Recent` (the latest 30 ended, oldest first, with every ended state's count), `Trend` (a `TrendView` with its texts from `ReportText`, where `target` is the latest bar's version's `GoalTarget`), `HasBegunEntry` and `ReadAt`. The view's members are `required`, so the whole report is built here; US2 and US4 add their tests.

  In the same commit: register `ReportService` (scoped) in `src/HT.Application/ApplicationServiceCollectionExtensions.cs`; add it with `TryAddScoped` to `tests/HT.UiTests/Components/Pages/PageServices.cs`; add it to `SignedInServices` in `tests/HT.ArchitectureTests/UseCaseParameterTests.cs`; widen the `IGoalStore` fence in `tests/HT.ArchitectureTests/ApplicationDependencyTests.cs` to admit `HT.Application.Reports.ReportService`, renaming the test to say so (research R-14)
- [X] T037 [US1] Change `src/HT.Ui/Components/Records/GoalDetailBody.razor` (after T034, T036; HP-001, contracts/host.md "Goal page"): one `GetGoalReportAsync` call through `UseCaseScope` next to slice 5's reads; when `HasBegunEntry` is true, the GoalStats card above the History card with the strip of `Recent`. Update `tests/HT.UiTests/Components/Pages/GoalDetailPageTests.cs`, whose slice-5 assertions break once the card is there:
  - the left-column card titles `["History", "Settings"]` (lines 101-103) gain the GoalStats card's title before "History";
  - the absent list (line 114) loses "Consistency" and "streak", which the card now shows, and keeps "Edit settings", "Pause", "End goal", "Earlier attempts" and "Excuse";
  - the exact `.ht-card__title` lists of the note tests (lines 289 and 326) gain the card's title for a goal with a begun entry;
  - before changing them, list every exact card-title or absent-text assertion in the file with `grep -n "ht-card__title\|absent" tests/HT.UiTests/Components/Pages/GoalDetailPageTests.cs`. T047 repeats this for the trend card's title.

  New cases: GoalStats with "Consistency" and its text; no card for a goal not started
- [X] T038 [US1] Add to `tests/HT.UiTests/Host/GoalsHostTests.cs` (after T037): the owner's prerendered goal page has the GoalStats card with the stat label "Consistency" above the History card; a goal not started has neither

**Checkpoint**: the full suite passes; the goal page shows consistency. Commit.

---

## Phase 4: User Story 2 - Strict streaks (Priority: P1)

**Goal**: the current and the longest streak of a goal, in its unit.

**Independent Test**: a daily goal with Met, Met, Failed, Met, Met, Met final days reads "Streak 3 days · longest 3".

The streaks are computed by T012 and returned by T034. This phase proves them through the use case and the page.
A failing test is fixed in the file whose rule it shows wrong.

### Tests for User Story 2

- [ ] T039 [P] [US2] Write `tests/HT.UnitTests/Application/Reports/GoalReportStreakTests.cs` with `ReportWorld` (after T034; FR-003, FR-004, FR-008, US2-AS1 to AS7, D-1): Met, Met, Failed, Met, Met, Met gives 3 and 3 "commitments"; Met × 11, Missed, Met × 6 gives 6 and 11; a latest Failed or Missed gives 0; Met, Met, Pending, Due gives 2; a Pending day with a record that reaches the target gives 2 at `2026-11-12T23:59:59Z` and 3 at `2026-11-13T00:00:00Z` (Conventions); a Pending day without a record gives 0 once its grace period ends, with nothing else done; the units and their singular forms ("1 day", "1 week", "1 commitment", "0 weeks"); no final result gives 0 and 0 in the unit
- [ ] T040 [P] [US2] Add to `tests/HT.UiTests/Components/Shared/GoalStatsTests.cs` (after T036; HP-001, US2-AS8): the labels "Streak, commitments" and "Longest, commitments", "Streak, days", "Streak, weeks", always plural, with their values

**Checkpoint**: the full suite passes. Commit.

---

## Phase 5: User Story 3 - Corrections change the reports (Priority: P1)

**Goal**: a correction changes every figure on the next read, as if recorded on time, and the goal page follows it
without reloading.

**Independent Test**: let a day become Missed in the middle of a run of Met days, then add a record that reaches the
target, and read the report again.

### Tests for User Story 3

- [ ] T041 [P] [US3] Write `tests/HT.UnitTests/Application/Reports/GoalReportCorrectionTests.cs` with `ReportWorld` (after T034; FR-008, FR-009, US3-AS1 to AS5, SC-004): Met × 3, Missed, Met × 2 ("83% (5 of 6)", 2, 3) corrected with a record that reaches the target gives "100% (6 of 6)", 6 and 6; a Met commitment changed below the target counts as Failed and breaks the streak at its date; a final commitment whose only record is deleted counts as Missed; two goals with the same settings, one recorded on time and one corrected later with the same values, give equal `GoalReport`s apart from their identifiers and names (stats, recent results and trend); after a time zone change each result is counted with its own version, in date order, and an overlap after a move east counts both entries
- [ ] T043 [US3] Add to `tests/HT.IntegrationTests/Reports/GoalReportReadTests.cs` (after T035; FR-008, SC-004): on the real stores, with T035's own provider and clock (Conventions), a Missed day corrected through `RecordingService.RecordAsync` changes the next report's consistency and streaks

### Implementation for User Story 3

- [ ] T042 [US3] Change `src/HT.Ui/Components/Records/GoalDetailBody.razor` (after T037; FR-008, contracts/host.md "Goal page"): after a record, a change, a delete or a note is saved in a dialog, the body reads the report again with the history. Add to `tests/HT.UiTests/Components/Pages/GoalDetailPageTests.cs`: after saving a record for a Missed day in the RecordDialog, the GoalStats card shows the new consistency without a page reload

**Checkpoint**: the full suite passes. Commit.

---

## Phase 6: User Story 4 - Recent results and trend of a goal (Priority: P2)

**Goal**: the last 30 results as a strip with their counts, and a trend of recorded progress against the target.

**Independent Test**: a daily duration goal with partial amounts reads its last 14 days of recorded minutes with the
target 20, and its last results with their states.

### Tests for User Story 4

- [ ] T044 [P] [US4] Write `tests/HT.UnitTests/Application/Reports/GoalReportRecentTests.cs` with `ReportWorld` (after T034; FR-005, US4-AS1, AS2, Edge Cases): 40 ended results give the latest 30, oldest first, each with its date (a week: its first and last date), state and progress; Due and Upcoming ones are left out; the counts of every ended state, 0 included; consistency and streaks still count all 40; a goal not started has no recent result
- [ ] T045 [P] [US4] Write `tests/HT.UnitTests/Application/Reports/GoalReportTrendTests.cs` with `ReportWorld` (after T034; FR-006, US4-AS3 to AS7, Edge Cases): "Reading, 20 min daily" with 20, 12, none and 25 gives those values, a null among them, target 20, `Minutes`, and the texts of T005's table with "Target 20 min"; "Yoga, 3 times a week" gives weekly bars with 2 for a week with two "Yes", target 3, at most 12; a yes/no goal on Monday, Wednesday and Friday gives weekly bars against its week's commitments, and a Tuesday-and-Thursday goal read on a Monday has the previous week as its latest bar; 0 and no record; a Reduce yes/no week of 1 against 0; "30 min, 3 times a week" with 30, 25 and 40 gives 2 against 3 and "Target 3"; a goal in hours gives minutes and "Target 60 min"; a Reduce count day gives "Target 1"; 45 against 20; a goal not started has an empty trend and `HasBegunEntry` false

### Implementation for User Story 4

- [ ] T046 [P] [US4] Create `src/HT.Ui/Components/Shared/TrendChart.razor` (after T005, T010, T028; HP-001, HP-006): the card title and sub line from `TrendView`, one `ht-bar` per bar scaled to the largest of the values and the target, the empty-bar class of T005 for a null value, the dashed `ht-bars__target` line at `TrendView.Target` with `TargetLine`, the date labels (`DateTexts`), each bar's tooltip with its own target, and `role="img"` with `aria-label` = `TrendView.Label`. Write `tests/HT.UiTests/Components/Shared/TrendChartTests.cs`: 14 and 12 bars in order, the empty bar, the target line text for minutes and for counts, a bar above the target, the label
- [ ] T047 [US4] Change `src/HT.Ui/Components/Records/GoalDetailBody.razor` (after T042, T046; HP-001): the TrendChart card after GoalStats, above History, only when `HasBegunEntry`. Add to `tests/HT.UiTests/Components/Pages/GoalDetailPageTests.cs`: the exact card-title lists that T037 changed also gain the trend card's title after GoalStats, before "History"; the trend card with its bars and target line; the strip's `aria-label` counting each state; both cards absent with no begun entry. Add to `tests/HT.UiTests/Host/GoalsHostTests.cs`: the prerendered page has the trend card with `role="img"`

**Checkpoint**: the full suite passes; the goal page has GoalStats and TrendChart. Commit.

---

## Phase 7: User Story 5 - My goals at a glance (Priority: P1)

**Goal**: the Goals page lists every goal with its figures, the goals that need action first.

**Independent Test**: a user with three goals, one with a Pending result, one Due and one not started, reads the
overview in that order with each goal's figures.

### Tests for User Story 5

- [ ] T048 [P] [US5] Write `tests/HT.UnitTests/Application/Reports/GoalsOverviewTests.cs` with `ReportWorld` (after T049; FR-011, FR-013, FR-016, US5-AS1 to AS4, research R-07): every goal with its consistency, current streak, last 15 ended results, `HasPending` and `CanRecord`; "B" with a Pending result, "A" with a Due one only and "C" with neither come B, A, C; within a group by name (`HabitName.KeyOf`, ordinal) then creation order; `Item` equals `GoalService.ListAsync`'s entry for the same goal field by field; `Recordable` equals `RecordingService.GetRecordableAsync`'s `GoalHistory` at the same instant; a Due entry with records gives `ReportText.ProgressText` "12 of 20 min today" and "1 of 3 this week"; `GoalService.ListAsync` keeps its entries and its name order; a goal whose catalog habit is set to Unpublished keeps its item and figures; a user with no goals gets `Found` with no goal

### Implementation for User Story 5

- [ ] T049 [US5] Add `GetGoalsOverviewAsync(ActingUser, CancellationToken)` to `src/HT.Application/Reports/ReportService.cs` (after T025, T034; FR-011, research R-06, R-07): the checks without the goal step; `IGoalStore.ListAsync(owner)`; one `GoalEvaluation` per goal; each `OverviewGoal` with `Item` from `GoalListItems.Of`, `Stats`, `Recent` (15), `HasPending`, `CanRecord` and `Recordable` from `EntryViews.HistoryOf` with the Due and Pending entries newest first; `Today` in the account's zone. Put the three-group order in a new internal `src/HT.Application/Reports/GoalOrder.cs`, which US6 reuses
- [ ] T050 [P] [US5] Extend `src/HT.Ui/Components/Shared/GoalItem.razor` to the full GoalListItem, keeping its name (after T006, T010, T031, T032, T049; HP-002, research R-12): it takes an `OverviewGoal`, today and a "Record" callback; the name link (`ht-goalitem__name`, which the smoke check reads), the Tag, a Pending StatusChip when `HasPending`, slice 4's meta line plus `ReportText.ProgressText` for a Due entry of `Recordable` with a record, "Consistency" and "Current streak" with their texts, the mini strip of 15, and "Record" primary (a Due entry), secondary (only Pending) or absent; a public `ValueTask FocusAsync()` that focuses its "Record" button, or its name link when it has none, as `HistoryCard.FocusAsync` does for a History row (T051 calls it). No reduced form remains. In `tests/HT.UiTests/Components/Shared/HabitComponentTests.cs`, replace the reduced GoalListItem test with full-item tests: each meta-line variant, the chip, the figures, the strip, the three action cases
- [ ] T051 [US5] Create the island `src/HT.Ui/Components/Reports/GoalsList.razor` and change `src/HT.Ui/Components/Pages/Goals.razor` (after T050; HP-002, contracts/host.md "Goals page"): the island reads `GetGoalsOverviewAsync` through `UseCaseScope` in `OnInitializedAsync`, renders one `GoalItem` per goal in order, opens slice 5's `RecordDialog` in its page-head mode with `Recordable` (its "For" select) and reads the overview again after a save. When the dialog closes, saved or not, the focus returns to that goal's "Record" button, found by its goal identifier after the re-read; if the goal has no "Record" any more, it goes to the goal's name link (Conventions, constitution IV). The page keeps its static head and slice 4's EmptyState for a user with no goals, renders the island with `InteractiveServer` and prerendering on, and the static footnote under it. Write `tests/HT.UiTests/Components/Reports/GoalsListTests.cs` (order, "Record" opens the dialog with every recordable entry, the list refreshed after a save, the focus back on the goal's "Record" after a save and after Esc, on its name link when the save leaves nothing to record) and update `tests/HT.UiTests/Components/Pages/GoalsPageTests.cs` for its slice-4 state
- [ ] T052 [US5] Add to `tests/HT.UiTests/Host/GoalsHostTests.cs` (after T051): the prerendered `/goals` has every goal in the order of FR-011 with its figures and strip, the footnote, and "Record" primary, secondary or absent; the first HTML already contains the rows

**Checkpoint**: the full suite passes; the Goals page lists the figures. Commit.

---

## Phase 8: User Story 6 - Today on Home (Priority: P1)

**Goal**: the signed-in Home shows the four tiles, the Today list, each goal's summary and the latest notes.

**Independent Test**: a user with a Pending, a Due and a Missed result reads the dashboard and gets the counts and the
Today list in that order.

### Tests for User Story 6

- [ ] T053 [P] [US6] Write `tests/HT.UnitTests/Application/Reports/DashboardTests.cs` with `ReportWorld` (after T054; FR-012, FR-013, US6-AS1 to AS8, research R-06):
  - the counts Due, Pending, Missed this week and goals; a per-week goal's Due week counted and listed;
  - the Today list: Pending by grace end ascending, Due in goal order (two Due entries of one goal by first date), Missed this week by grace end descending, ties in goal order, then Upcoming; each entry's goal, dates, state, progress, `HasRecord`, `HasNote` and grace end; `TodayEntry.Goal` equal to `GetRecordableAsync`'s `GoalHistory`;
  - "this week": the UTC and Europe/Kyiv instants of the Conventions (Friday counted at Monday 00:00, Thursday not; after a change back Sunday 23:00 not counted; after a change forward Monday 01:00 counted, Pending until then); the account's stored zone, else the installation default; an account zone that differs from a goal's older version;
  - Upcoming: a Monday, Wednesday and Friday goal read on a Tuesday ends the list with Wednesday's commitment; a goal with a Due entry and one whose next entry starts more than 6 days after its current date get none; a goal not started gets its first entry when it starts at most 6 days after the account's today; several goals' Upcoming entries come by start instant, and two on the same date in goal order (AC-038's "Weekly" and "Soon"); no count for Upcoming;
  - a goal whose catalog habit is set to Unpublished keeps its counts, rows, stats and notes;
  - goals with their stats in goal order; notes at most 5 by first date descending, ties in goal order, each with its goal's name, dates and state;
  - a user with no goals: counts 0, empty lists, `Found`

### Implementation for User Story 6

- [ ] T054 [US6] Add `GetDashboardAsync(ActingUser, CancellationToken)` to `src/HT.Application/Reports/ReportService.cs` (after T049; FR-012, FR-013, research R-06): the checks without the goal step; the account's zone; `WeekStarts`; one `GoalEvaluation` per goal; the counts, the four parts of the Today list with `GoalOrder` for ties, the Upcoming entries through `NextUpcoming` (no records, no grace end), `Goals`, `Notes`, `AccountToday` and `ReadAt`
- [ ] T055 [P] [US6] Create `src/HT.Ui/Components/Shared/StatTile.razor` (after T007, T010; HP-003, HP-006): a label (`ht-tile__label`, which the smoke check reads), a value, a modifier class, and either a `<button>` with `aria-pressed` and an `OnToggle` callback or a link; a public `ValueTask FocusAsync()` that focuses the button or the link (T059 calls it for "Due now"). Write `tests/HT.UiTests/Components/Shared/StatTileTests.cs`: both forms, the pressed state, keyboard activation, `FocusAsync` focusing the control (`JSInterop.VerifyFocusAsyncInvoke`)
- [ ] T056 [P] [US6] Create `src/HT.Ui/Components/Shared/CommitmentRow.razor` (after T007, T010, T031; HP-003): from a `TodayEntry` and the reading instant, the name, the "Reduce" Tag, the meta line per state (T007), the progress bar for count and duration goals, the StatusChip, and one action with its callback: "Record" (a Due or Pending period, or a commitment without a record), "Edit" (a commitment with a record), "Add note" or "Edit note" (Missed), none (Upcoming); a public `ValueTask FocusAsync()` that focuses its action, and does nothing for an Upcoming row (T059 calls it). Write `tests/HT.UiTests/Components/Shared/CommitmentRowTests.cs`: each state and action, "5 h left to record", "no record", a week's "16 – 22 Nov", the Upcoming row with no action, `FocusAsync` focusing the action (`JSInterop.VerifyFocusAsyncInvoke`)
- [ ] T057 [P] [US6] Create `src/HT.Ui/Components/Shared/GoalSummary.razor` (after T008, T010; HP-003): at most 5 `DashboardGoal`s, each name linking to its page, its consistency, `ReportText.SummaryLine` or "N/A" with `NoFinalYet`, then "All goals →" to `/goals`. Write `tests/HT.UiTests/Components/Shared/GoalSummaryTests.cs`: 3 and 7 goals, a goal with no final result
- [ ] T058 [P] [US6] Create `src/HT.Ui/Components/Reports/RecentNotesList.razor` (after T007, T010; HP-003): the "Recent notes" card with each `DashboardNote` in quotes and "{goal name} · {date}" under it (`DateTexts`); nothing rendered without a note. Write `tests/HT.UiTests/Components/Reports/RecentNotesListTests.cs`. Slice 5's `RecentNotesCard` (goal page) is not changed
- [ ] T059 [US6] Create the island `src/HT.Ui/Components/Reports/TodayPanel.razor` (after T054 to T056; HP-003, research R-12): it reads `GetDashboardAsync` through `UseCaseScope` in `OnInitializedAsync`; the four StatTiles with the counts; choosing Due, Pending or Missed filters the rows and choosing it again shows all; the Today card with one CommitmentRow per entry or "Nothing to record right now."; "Record" opens slice 5's `RecordDialog` fixed to the entry (`Goal` as `Recordable`, `Entry` as the row), "Edit" with the entry's one `RecordView`, "Add note" and "Edit note" open the `NoteDialog`; after a save it reads the dashboard again. When either dialog closes, saved or not, the focus returns to the same row's action, found by its goal and first date after the re-read; if the row has left the list or the filter hides it, the focus goes to the "Due now" tile, which is always there (Conventions, constitution IV). Write `tests/HT.UiTests/Components/Reports/TodayPanelTests.cs`: the tiles and their counts, the filter and `aria-pressed`, each action opening its dialog, the refresh after a save, the focus back on the row's action after a save and after Esc for both dialogs, on "Due now" when the row is gone, the empty sentence
- [ ] T060 [US6] Change `src/HT.Ui/Components/Pages/Home.razor` for a registered user with goals (after T057 to T059; HP-003, contracts/host.md "Signed-in Home"): under slice 4's page head, the `TodayPanel` island (`InteractiveServer`, prerendered), and in the right column the static GoalSummary and Recent notes from the page's own `GetDashboardAsync`; a user with no goals keeps the EmptyState; an administrator keeps the page head only. Update `tests/HT.UiTests/Components/Pages/HomeSignedInTests.cs` for its slice-4 state ("page head only until slice 6") and add the slice-6 cases
- [ ] T061 [US6] Write `tests/HT.UiTests/Host/HomeDashboardHostTests.cs` (after T060): the prerendered Home of a user with goals has the four tile labels, the Today rows with their chips, the "Active goals" card with at most 5 goals and "All goals →", and no "Recent notes" card without a note; a user with no goals gets the EmptyState; an administrator gets no tile

**Checkpoint**: the full suite passes; the Home is the dashboard. Commit.

---

## Phase 9: User Story 7 - Report privacy (Priority: P1)

**Goal**: only the goal's owner reads its reports.

**Independent Test**: user A has a goal with results; user B, an administrator and an anonymous visitor each try to
read A's goal report, and each reads their own dashboard and overview, which never contain A's goals.

### Tests for User Story 7

- [ ] T062 [P] [US7] Write `tests/HT.UnitTests/Application/Reports/ReportAccessTests.cs` (after T054; FR-014, FR-015, US7-AS1 to AS4, SC-005): for each of the three use cases, an anonymous visitor gets "You are not signed in.", an administrator "Goals are for registered users.", an account the store no longer has "You are not signed in.", each with no view and before any goal lookup. To show that, this task adds to `tests/HT.UnitTests/Fakes/FakeGoalStore.cs` a `Lookups` count of `FindAsync` and `ListAsync` calls (test code only; no existing member changes). The tests assert `Lookups` 0 and an empty `FakeRecordStore.Calls`; B reading A's goal by its identifier gets "This goal does not exist.", the same as for an unknown identifier; B's overview and dashboard contain none of A's goals, results or notes
- [ ] T063 [P] [US7] Write `tests/HT.IntegrationTests/Reports/ReportPrivacyTests.cs` on `HT-IT` (after T054; FR-014, SC-005): with the real stores and the test's own provider and clock (Conventions), A's goal with records and notes; B, an administrator and an anonymous visitor read the three use cases, and none returns A's goal, result, note or figure
- [ ] T064 [P] [US7] Write `tests/HT.IntegrationTests/Reports/ReportLogTests.cs` with `LogCapture` (after T054; HP-007), with the test's own provider and clock (Conventions): the three reads of a goal with notes, partial amounts and a custom count unit; no captured log line or exception message contains the note texts, the recorded values, the unit, the goal name, the habit name or a figure text of the reports
- [ ] T065 [P] [US7] Add to `tests/HT.UiTests/Host/GoalPrivacyHostTests.cs` (after T061): another account's goal page is still the Not found page, with no GoalStats or trend; B's `/goals` and Home contain none of A's goal names; an administrator's Home has no tile and no row

**Checkpoint**: the full suite passes. Commit.

---

## Phase 10: User Story 8 - Demo reports (Priority: P2)

**Goal**: `demo4` and `demo5` show non-trivial reports at the run time, `demo5` with full trends and a result Missed
this week; the visitor Home shows the example card; the smoke check confirms both demo accounts' reports.

**Independent Test**: reset the database, run demo loading, demo history loading and demo report loading at a fixed
time, and read both accounts' reports and dashboards at that time.

### Tests for User Story 8

- [ ] T066 [P] [US8] Write `tests/HT.UnitTests/Application/Operator/DemoReportTests.cs` (after T072; FR-032): `DemoReport.For(R)` for R on each weekday (seven theory rows): the three goals on "Walking", "Strength training" and "Screen time", their targets, start R − 84; Walking's records (30, 35 for k 9 to 15, 18 for k 20 to 26, none for k 43 to 49 and 0 to 2) and the note "On holiday." on the latest commitment of k 43 to 49; Strength training's weeks (week 2 Mon, Wed, Thu; week 4 Mon only with "Sore shoulder."; week 7 none; the current week none; week 12 only when R is a Monday); Screen time's records (90 and 110, 150 for k 30 to 32, the note "Long train ride." on R − 31, none for k = 50 and R − 3, 100 on R − 2, none on R − 1 and R); every record on a date before R; `GoalNames`
- [ ] T067 [P] [US8] Write `tests/HT.UnitTests/Application/Operator/DemoReportLoadingTests.cs` with `FakeDemoReportStore` (after T073; FR-032, US8-AS7, AS9, AS10): no password, empty, only spaces, and one without a digit give their refusal and no store call; a valid one gives "created", "demo goals added" and "demo records added" for `demo5@example.com`, also when the store leaves goals out; a taken email (any case) gives "already exists" with no goal or record result and no further store call; R is the date in the default zone; no result text contains a goal, habit, note, unit or amount
- [ ] T068 [P] [US8] Write `tests/HT.IntegrationTests/Operator/DemoReportLoadingTests.cs` on `HT-IT-SEED` (after T074; FR-031, FR-032, US8-AS7, AS9, AS10): after demo loading, demo history loading and demo report loading at the fixture's time (`ResetAndBuildAsync` and its `Clock`, never moved), `demo5@example.com` exists with the three goals, records and notes of FR-032; a second run gives "already exists" and changes nothing; a goal whose habit is not Published is left out with its records and notes; a forced write failure rolls back the account, and with `LogCapture` neither the exception message nor any log line names a goal, habit, note or amount; demo loading's and demo history loading's results never name `demo5@example.com`. The existing `tests/HT.IntegrationTests/Records/DemoHistoryLoadingTests.cs` pass unchanged after T074's extraction
- [ ] T069 [P] [US8] Write `tests/HT.IntegrationTests/Reports/DemoReportsTests.cs` on `HT-IT-SEED` (after T054, T074; FR-030, FR-031, FR-032, US8-AS1 to AS5, AS8, AS11, AS12), with the three tasks run at `2026-11-16T12:00:00Z` (Monday), `2026-11-17T12:00:00Z` (Tuesday) and `2026-11-18T12:00:00Z` (Wednesday) and read at that time. Each day is its own test with its own `FakeTimeProvider` at that instant: it resets `HT-IT-SEED` with `HtDatabase.ResetAsync` and builds its own provider (Conventions). `SeedDatabaseFixture.Clock` stays at 14 November, so `DemoHistoryLoadingTests` and `DemoGoalsTests` keep their run date:
  - `demo4`: Reading "92% (24 of 26)", 3 and 18 commitments; Sugar "92% (24 of 26)", 1 and 16 days; Yoga per weekday ("33% (1 of 3)", 1 and 1 week on Wednesday; "0% (0 of 2)", 0 and 0 on Tuesday; "33% (1 of 3)", 0 and 1 on Monday); its dashboard's Pending and Due rows of Reading and Sugar, no Missed row, "Missed this week" 0, and the notes "Fell asleep early." and "Birthday cake at work.";
  - `demo1` to `demo3`: "N/A" and streaks of 0;
  - `demo5` on Wednesday: Screen time "93% (77 of 82)", 0 and 34 days; Strength training "81% (9 of 11)", 3 and 4 weeks; Walking "82% (29 of 35)", 7 and 15 commitments; trends of 14, 12 and 14 bars; counts 3 Due, 3 Pending, 1 Missed this week, 3 goals; the Today list exactly as US8-AS12 gives it, with no Upcoming entry;
  - `demo5` on Tuesday and Monday: Strength training "80% (8 of 10)", 2 and 4 weeks, and "81% (9 of 11)", 2 and 5 weeks; "Missed this week" 1 with Screen time's R − 3 on each day
- [ ] T070 [P] [US8] Update `tests/HT.UiTests/Host/StartupLogTests.cs` and `tests/HT.UiTests/Operator/StartupTasksDemoTests.cs` (after T076; HP-011, contracts/host.md "Operator log"): demo5's lines come after demo4's, which stay exactly as they are; a second start logs "already exists" for `demo5@example.com`; no or a weak demo password logs three refusal lines, "Demo loading: …", "Demo history loading: …" and "Demo report loading: …"; `A_fault_in_demo_history_loading_is_logged_as_an_error_after_demo_loadings_lines_and_the_start_goes_on` now expects demo5's lines after the fault; a new test: a fault in demo report loading logs "Demo report loading failed; the app starts without it" and the start goes on; `No_log_entry_contains_a_demo_custom_habit_goal_or_account_name` also checks `DemoReport.GoalNames` and FR-032's three notes. These tests compare whole lists, so they fail as soon as T076 lands and change with it: `StartupLogTests`' `Assert.Equal(4, demoLines.Count)` and its account list gain `demo5@example.com` (5 lines); `Every_demo_records_line_holds_the_demo_email_and_the_result_text_only` expects demo4's and demo5's records lines; `Every_demo_goals_line_holds_the_demo_email_and_the_result_text_only` expects `Emails.Append(Demo4).Append(Demo5)`. Before changing them, list every test that asserts the demo lines with `grep -rn "Demo account \|Demo goals \|Demo records " tests/HT.UiTests --include=*.cs`, and update each one that fails
- [ ] T071 [P] [US8] Write `tests/HT.UnitTests/Application/Reports/ExampleGoalTests.cs` (after T077; HP-004): `Title` "Yoga, 3 days a week"; `Strip` of 21 states in HP-004's order; `Stats` "90% (18 of 20)", 6 and 11 commitments, equal to `Consistency.Of` and `Streaks.Of` over `Strip`

### Implementation for User Story 8

- [ ] T072 [US8] Create in `src/HT.Application/Operator/`: `DemoReport.cs` with `For(DateOnly runDate)`, `GoalNames` and the three note texts, built as `DemoHistory` builds demo4's goals (`DemoGoal.OnCatalogHabit`, `RecordValue.TryCreate`, `NoteText.TryCreate`, the existing `DemoHistoryGoal`, `DemoRecord` and `DemoNote`); `DemoReportLoadingResult.cs` (with `DemoReportLoadingOutcome`: `NoPassword`, `PasswordRuleNotMet`, `Loaded`) and `DemoReportAccountResult.cs`, member for member as demo history loading's (research R-09). The texts come from `OperatorTexts`
- [ ] T073 [US8] Create `src/HT.Application/Operator/IDemoReportStore.cs` and `DemoReportService.cs` (`DemoEmail = "demo5@example.com"`, `LoadAsync(string? demoPassword, CancellationToken)` with research R-09's steps), and `tests/HT.UnitTests/Fakes/FakeDemoReportStore.cs` on the pattern of `FakeDemoHistoryStore` (after T072). `DemoHistoryService` and `IDemoHistoryStore` are not touched
- [ ] T074 [US8] Move `src/HT.Persistence/Habits/DemoHistoryStore.cs`'s body to a new internal `src/HT.Persistence/Habits/DemoAccountWriter.cs`, called by `DemoHistoryStore` and a new internal `DemoReportStore.cs` (`IDemoReportStore`), so both write the same way: one transaction, goals on a habit that is not Published left out with their records and notes, `EmailTaken` creating nothing, a fault rolling back with no value in its message (after T073; research R-09). In the same commit: register `IDemoReportStore` in `src/HT.Persistence/PersistenceServiceCollectionExtensions.cs` and `DemoReportService` in `src/HT.Application/ApplicationServiceCollectionExtensions.cs`, both scoped (plan, Work order 3). `PageServices` gets nothing: no page resolves an operator service (Conventions)
- [ ] T075 [P] [US8] Add the demo-report rules (after T074; research R-14): in `tests/HT.ArchitectureTests/ApplicationDependencyTests.cs`, `IDemoReportStore` is used by no HT.Application type other than `HT.Application.Operator.DemoReportService`, and `NoOperatorTypeOfApplicationMayDependOnTheGoalStorePort` stays; in `UiDependencyTests.cs`, no HT.Ui type depends on `IDemoReportStore`, and `DemoReportService` joins the operator services only `HT.Ui.Operator` may use; in `PersistenceDependencyTests.cs`, `DemoAccountWriter` and `DemoReportStore` are internal
- [ ] T076 [US8] Add the "Demo report loading" step to `src/HT.Ui/Operator/StartupTasks.cs` after demo history loading (after T074; HP-011, research R-10): its own scope and `RunStepAsync` with the fault line "Demo report loading failed; the app starts without it"; source-generated log methods for "Demo report loading: {Result}" and the account, goals and records lines in demo history loading's templates; the demo password from the same configuration key; no secret in any line
- [ ] T077 [P] [US8] Create `src/HT.Application/Reports/ExampleGoal.cs` (after T028; HP-004, D-3): `Title`, `Strip` and `Stats`, computed once by `Consistency.Of`, `Streaks.Of` and `ReportText` from HP-004's 21 fixed states; it reads no store
- [ ] T078 [US8] Create `src/HT.Ui/Components/Reports/ExampleGoalCard.razor` (the static `GoalStats` with the eyebrow "Example goal", `ExampleGoal.Title`, `Stats` and `Strip`) and add it to the visitor hero of `src/HT.Ui/Components/Pages/Home.razor` (after T009, T036, T060, T077; HP-004). In `tests/HT.UiTests/Components/Pages/HomeVisitorTests.cs`, `US5_AS8_HP_012_with_no_Published_habit_nothing_else_from_the_preview_is_shown` now asserts only the catalog preview's absence, and a new test checks the example card with its texts and 21 cells. Add to `tests/HT.UiTests/Host/HomeCatalogHostTests.cs`: the visitor Home has the card with and without Published habits, and the same HTML for two visitors
- [ ] T079 [US8] Extend `tools/HT.SmokeCheck/` (after T047, T052, T061, T074, T076; check 7 signs in as `demo5@example.com`, which only T076's startup step creates; HP-008, research R-11, contracts/host.md "Smoke check"):
  - `PageReader.ReadStatLabels`, `ReadTileLabels` and `ReadStatusChips`, each the decoded texts of its leaf elements (`.ht-stat__label`, `.ht-tile__label`, `.ht-status`);
  - check 6 gains "Consistency" and "Streak, commitments" among the stat labels of "Reading, 20 min daily"'s page, and "Due now" among the Home's tile labels, each matched whole with `PageReader.ContainsAll` (ordinal);
  - check 7 `demo report demo5@example.com` in `SmokeCheckRunner.cs` after check 6: sign in on a new client as `DemoReportService.DemoEmail`; on `/goals` the three `DemoReport.GoalNames` (`ReadGoalNames`); on Strength training's page the stat label "Streak, weeks"; on `/` "Missed this week" among the tile labels and "Missed" among the status chips; the output `PASS demo report demo5@example.com (N of N)` or its FAIL line, never a path, goal name, note or figure;
  - `SmokeCheckCoverage.cs` gains the 4 entries of `IDemoReportStore.CreateDemoReportAccountAsync` (email, password, profile, goals), mapped to check 7
- [ ] T080 [US8] Update the smoke-check tests and contract (after T079; spec "Slice 4 and 5 requirements and tests this slice changes"):
  - `tests/HT.UiTests/SmokeChecks/PageReaderTests.cs`: the three readers, including a part of a label that must not count;
  - `RunnerBehaviourTests.cs`: `GoodSite()` scripts demo4's new texts and demo5's sign-in and pages; the exact check lines and their positions; one case per check 7 FAIL reason, none of whose lines contains a scripted name, note or path;
  - `CoverageTests.cs`: `Ports` gains `typeof(IDemoReportStore)`, and the exact entry count goes from 10 to 14;
  - `tests/HT.UiTests/Host/SmokeCheckHostTests.cs`: the list of check names gains `demo report demo5@example.com` with its PASS line; check 6's "(N of N)" grows by 3; the failing-password case also expects check 7's FAIL line; the forbidden-text list gains `DemoReport.GoalNames` and FR-032's three notes;
  - `specs/003-demo-hosting/contracts/smoke-check.md`, with a note citing ADR 0021: check 6's three texts, check 7 with its parts and reasons, and the coverage table's 4 rows
- [ ] T081 [P] [US8] Update `docs/operations/hosted-demo.md` (after T079; HP-011, R-10): `demo5@example.com` among the demo accounts and in the daily refresh, demo report loading as the fourth startup task with its log lines, and check 7. Check 6 changes too:
  - its sample output line becomes `PASS demo history demo4@example.com (14 of 14)`, 11 texts plus HP-008's 3;
  - its description adds the two stat labels on Reading's page and the "Due now" tile label to "3 goal names on Goals and 8 texts in the History cards";
  - the sample output gains check 7's line `PASS demo report demo5@example.com (<found> of <expected>)` after it.

**Checkpoint**: the full suite passes, the startup log shows demo5's lines, and the in-process smoke check passes checks 6 and 7. Commit.

---

## Phase 11: Polish and cross-cutting concerns

**Purpose**: the timing criterion, documentation, the API and analyzer comparisons, the accessibility sweep and the
manual quickstart.

- [ ] T082 [P] Update the slice 6 status line of `README.md` ("Status"). Check `CONTEXT.md`: change an entry only if the implementation sharpened one of HP-010's terms (the entries were added on 2026-11-20)
- [ ] T083 Write `tests/HT.IntegrationTests/Reports/ReportTimingTests.cs` on `HT-IT` (SC-007, research R-08): with the test's own provider and clock (Conventions), a user with three goals, one created at that clock's start with 366 daily records added as the clock moves forward a year; the goal report, the goals overview and the dashboard each return in under 2 seconds, measured with `Stopwatch` around the call. A failure is reported as it is, never retried silently
- [ ] T084 Compare the public API of `HT.Domain`, `HT.Application` and `HT.Persistence` with T002's baseline. The only differences allowed are additions: `HT.Domain.Reports`, `GoalTimeline.EntriesThrough`, `HT.Application.Reports`, `RecordingText.HoursLeft` and the demo report loading types. Report any other difference as a defect (FR-016)
- [ ] T085 [P] Run the analyzers and compare like with like. Never point either tool at `tests/HT.AcceptanceTests`:
  - Run `roslynator analyze HabitTracker.slnx` solution-wide after `dotnet restore HabitTracker.slnx`, the same scope as `CLAUDE.md`'s baseline (2026-11-18: 333, of which 37 are not from the compiler). Run `jb inspectcode HabitTracker.slnx --stdout -f=Text -e=WARNING --caches-home=<scratchpad>/jbcache --no-updates` (baseline 170).
  - Sort every new diagnostic into one of two groups:
    - the expected growth: Razor phantoms (`CS0246`, `CS0103`, `CS1503`, `CS1061`) in the new bUnit tests, and the test-only hints `CLAUDE.md` lists;
    - real findings, in production code or as real test defects.
  - Fix the real findings only. The documented false positives stay.
  - Then update the "Baseline" paragraph of `CLAUDE.md` with the date, the three new counts and "after slice 6", so the next slice compares against them.
- [ ] T086 [P] Sweep the goal page, the Goals page, the signed-in Home and the visitor Home for constitution IV and HP-006 (SC-008): tokens and `ht-` classes only, with no literal colours; visible focus on tiles, links and buttons; states never by colour alone; the strip's and the trend's `role="img"` and labels; the tiles' `aria-pressed`; every dialog opened from the goal page, the Goals page and the Home focusing its first field on open, closing on Esc and ×, and giving the focus back to its opener, or to the fallback T051 and T059 name, after a save; sentence case per `design/brand-book.md`. Then rerun `DesignSyncTests`
- [ ] T087 Run quickstart §3 in the running app after dropping the local database that `HT.Ui`'s connection string names. Report each step's result. The hosted smoke check and the daily refresh (quickstart §4) stay labelled unverified until they run after the merge
- [ ] T088 Run the full build and `dotnet test HabitTracker.slnx`, and report `PASS` with the counts. Leave slice 6's roadmap Status at `implementing`: `AC tests green` comes after the owner's `/speckit-ac-tests-analyze`. Commit

---

## Dependencies and execution order

### Phase dependencies

- **Setup (Phase 1)**: no dependencies.
- **Foundational (Phase 2)**: after Setup. It blocks every story. Design (T003 to T010) comes before every
  component and page task.
- **User stories (Phases 3 to 10)**: after Phase 2, in this order: US1, US2, US3, US4, US5, US6, US7, US8.
- **Polish (Phase 11)**: after the stories that are to ship. T083 needs all three use cases (US6).

### Story dependencies

- **US1**: none beyond Phase 2. It creates `ReportService`.
- **US2**: after US1. Its tests call `GetGoalReportAsync` and extend `GoalStatsTests.cs`.
- **US3**: after US1. It changes `GoalDetailBody.razor` after T037.
- **US4**: after US3. It changes `GoalDetailBody.razor` after T042.
- **US5**: after US1. It adds to `ReportService.cs`. It can run beside US2 to US4, apart from the shared files
  below.
- **US6**: after US5. It reuses `GoalOrder` (T049) and adds to `ReportService.cs` after it.
- **US7**: after US6. Its tests cover all three use cases and the Home.
- **US8**: T072 to T077 need only Phase 2 and US1 (T036 for the card), so they can run beside US2 to US7. T069 needs
  US6 (T054). The smoke check (T079, T080) needs the pages of US4, US5 and US6, and T076's startup step, which
  creates `demo5@example.com`.
- **Shared files** need sequential edits, so their tasks never run in parallel with each other:
  - `ReportService.cs`: T034, T049, T054;
  - `GoalDetailBody.razor`: T037, T042, T047; `GoalDetailPageTests.cs`: T037, T042, T047;
  - `GoalsHostTests.cs`: T038, T047, T052;
  - `GoalStatsTests.cs`: T036, T040;
  - `GoalReportReadTests.cs`: T035, T043;
  - `Home.razor`: T060, T078;
  - `ApplicationServiceCollectionExtensions.cs`: T034, T074;
  - `tests/HT.UiTests/Components/Pages/PageServices.cs`: T034;
  - `tests/HT.UnitTests/Fakes/FakeGoalStore.cs`: T062 (the `Lookups` count);
  - `CLAUDE.md`: T085 (the analyzer baseline);
  - `docs/operations/hosted-demo.md`: T081;
  - `ApplicationDependencyTests.cs`: T034, T075; `UiDependencyTests.cs` and `PersistenceDependencyTests.cs`: T075;
  - `UseCaseParameterTests.cs`: T034;
  - `RecordingTextTests.cs`: T026;
  - `StartupTasks.cs`: T076; `StartupLogTests.cs` and `StartupTasksDemoTests.cs`: T070;
  - `docs/roadmap.md`: T001;
  - `specs/003-demo-hosting/contracts/smoke-check.md`: T080.

### Within each story

- A test task starts once the tasks it names as dependencies are done, usually the use case it calls, because a
  test that does not compile cannot fail meaningfully. It then runs in parallel with the story's remaining work.
  The order of the implementation is: the use case, the components, the page, and the host tests.
- The story's checkpoint runs the full suite before the next story, and is its commit point.

### Parallel opportunities

- Phase 1: T002 with T001.
- Design: T003 to T009 together, then T010.
- Domain: T011 to T014 and T016 together; then T015; then the tests T017 to T023.
- Application: T024, T025, T026 and T027 together; then T028 and T030; then T029.
- Shared components: T032 with T031.
- US1: T036 beside T034; T033 and T035 after T034.
- US2: T039 and T040.
- US4: T044, T045 and T046 together.
- US6: T055 to T058 together after T054; T053 beside them.
- US7: T062 to T065 together.
- US8: T072 to T074 in order, then T075, T076 and T077 together; the test tasks T066 to T071 as their
  dependencies finish.

---

## Parallel example: Phase 2 Domain

```text
Task: "T011 Consistency in src/HT.Domain/Reports/Consistency.cs"
Task: "T012 Streaks in src/HT.Domain/Reports/Streaks.cs"
Task: "T013 StreakUnit and StreakUnits in src/HT.Domain/Reports/"
Task: "T014 Trend types in src/HT.Domain/Reports/"
Task: "T016 EntriesThrough in src/HT.Domain/Goals/GoalTimeline.cs"

# after T015:
Task: "T017 Consistency tests in tests/HT.UnitTests/Domain/Reports/ConsistencyTests.cs"
Task: "T018 Streaks tests in tests/HT.UnitTests/Domain/Reports/StreaksTests.cs"
Task: "T020 TrendBuilder tests in tests/HT.UnitTests/Domain/Reports/TrendBuilderTests.cs"
Task: "T021 EntriesThrough tests in tests/HT.UnitTests/Domain/Goals/GoalTimelineEntriesThroughTests.cs"
Task: "T022 Boundary tests in tests/HT.UnitTests/Domain/Reports/ReportBoundaryTests.cs"
```

## Parallel example: User Story 4

```text
# after T034:
Task: "T044 [US4] Recent tests in tests/HT.UnitTests/Application/Reports/GoalReportRecentTests.cs"
Task: "T045 [US4] Trend tests in tests/HT.UnitTests/Application/Reports/GoalReportTrendTests.cs"
Task: "T046 [US4] TrendChart in src/HT.Ui/Components/Shared/TrendChart.razor"
```

## Parallel example: User Story 6

```text
# after T054:
Task: "T053 [US6] Dashboard tests in tests/HT.UnitTests/Application/Reports/DashboardTests.cs"
Task: "T055 [US6] StatTile in src/HT.Ui/Components/Shared/StatTile.razor"
Task: "T056 [US6] CommitmentRow in src/HT.Ui/Components/Shared/CommitmentRow.razor"
Task: "T057 [US6] GoalSummary in src/HT.Ui/Components/Shared/GoalSummary.razor"
Task: "T058 [US6] RecentNotesList in src/HT.Ui/Components/Reports/RecentNotesList.razor"
```

## Parallel example: User Story 7

```text
Task: "T062 [US7] Access tests in tests/HT.UnitTests/Application/Reports/ReportAccessTests.cs"
Task: "T063 [US7] Store privacy tests in tests/HT.IntegrationTests/Reports/ReportPrivacyTests.cs"
Task: "T064 [US7] HP-007 log tests in tests/HT.IntegrationTests/Reports/ReportLogTests.cs"
Task: "T065 [US7] Host privacy tests in tests/HT.UiTests/Host/GoalPrivacyHostTests.cs"
```

## Parallel example: User Story 8

```text
# after T074:
Task: "T075 [US8] Demo-report rules in tests/HT.ArchitectureTests/"
Task: "T076 [US8] Startup step in src/HT.Ui/Operator/StartupTasks.cs"
Task: "T077 [US8] ExampleGoal in src/HT.Application/Reports/ExampleGoal.cs"
Task: "T068 [US8] Demo report loading on HT-IT-SEED in tests/HT.IntegrationTests/Operator/DemoReportLoadingTests.cs"
```

---

## Implementation strategy

### MVP first (US1 to US3, the goal page's figures)

1. Phase 1 and Phase 2: design first, then Domain with its boundary tests, the Application foundation, StatusChip
   and ConsistencyStrip.
2. US1: `ReportService.GetGoalReportAsync` and the GoalStats card.
3. US2: the streaks proven through the use case and the card.
4. US3: corrections proven, and the goal page refreshing its figures after a save.
5. **Stop and validate**: run the full suite, then check in the browser that correcting a Missed day changes the
   goal page's consistency without a reload.

### Incremental delivery

1. US4: the trend card completes the goal page.
2. US5 and US6: the Goals page and the Home dashboard (SC-001).
3. US7: privacy proven for all three use cases (SC-005).
4. US8: demo report loading, the startup step, the visitor card and smoke check 7.
5. Polish. Then the owner runs `/speckit-ac-tests-write` and `/speckit-ac-tests-analyze` for F007, with F001, F002,
   F004 and F005 expected to pass unchanged (FR-016).

One PR for the slice. Each checkpoint is a commit on the feature branch.

---

## Notes

- [P] tasks touch different files, and start once the tasks they name as dependencies are done (Format).
- The AC project, its features, the locks and the run records are never edited.
- The task order differs from the plan's Work order in one way: the plan orders by layer (Application and
  Persistence, then host, then pages, then the smoke check), these tasks order by story, and keep the layer order
  inside each story. Phase 2 does the plan's steps 1 and 2 in full, and the parts of step 3 that every story
  shares. The plan's pairings hold at every checkpoint: `ReportService` with its registrations (T034), and demo
  report loading's service, port, store and registrations together (T072 to T074). The smoke check still comes
  after the pages it reads (T079).
- Settled while writing the tasks, within the design artifacts:
  - `ReportService` is created in US1 with `GetGoalReportAsync` whole, because `GoalReport`'s members are
    `required`; US2 and US4 add the tests of its streaks, recent results and trend;
  - `ConsistencyStrip` and `StatusChip`'s "Upcoming" are built in Phase 2, because the goal page, the Goals page,
    the Home and the visitor Home all use them;
  - the three-group goal order lives in an internal `GoalOrder` (T049), shared by the overview and the dashboard;
  - the demo figures of FR-030 and FR-032 are tested on `HT-IT-SEED` with the real tasks (T069), because the demo
    store fakes do not fill the goal and record fakes; `DemoReport.For(R)`'s data is unit-tested for every weekday
    (T066);
  - SC-007's timing test sits in Polish (T083), because it measures all three use cases.
- Stop at any checkpoint to validate a story on its own.
