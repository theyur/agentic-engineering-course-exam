# Research: Excusals and pauses

Phase 0 of [plan.md](plan.md). The spec has no `[NEEDS CLARIFICATION]` marker: D-1 to D-4 are decided in its
`## Clarifications`, [ADR 0023](../../docs/adr/0023-demo-pause-loading.md) and
[ADR 0024](../../docs/adr/0024-excusal-and-pause-rules.md). This file records the technical choices of the plan.
Each has its decision, rationale and the alternatives considered.

The code read for these choices: `HT.Domain.Evaluation` (`Evaluator`, `EvaluationState`, `GracePeriod`),
`HT.Domain.Goals.GoalTimeline`, `HT.Domain.Reports`, `HT.Application.Records` (`RecordingService`, `EntryViews`,
`IRecordStore`), `HT.Application.Reports` (`GoalEvaluation`, `ReportService`), the demo report loading of slice 6 and
`HT.Persistence.Records.RecordStore`.

## R-01 Excused and Paused as two more members of `EvaluationState`

**Decision**: `HT.Domain.Evaluation.EvaluationState` gains `Excused` and `Paused`, after `Missed`. Every reader that
gives an entry's state (history, what can be recorded now, recent results, notes, the state after a write) puts the
exemption in the same `State` field. The enum's doc comment says what it now holds: the evaluation state of an entry,
or Excused or Paused in its place (spec "Terms used below").

**Rationale**:

- The spec asks for the word "in place of" the state everywhere (FR-015, FR-017, FR-020). One field gives that
  without a second field that every reader must check first.
- Slices 5 and 6 planned for it: `EvaluationState`'s doc comment says "Excused and Paused come in slice 7", and slice
  6 research R-01 says "slice 7 adds states to skip".
- **Compatibility with the locked AC code of F001 to F007, which may not be read.** It compiles with warnings as
  errors (the root `Directory.Build.props` reaches the AC project). A switch expression over `EvaluationState` that
  names all six members and has no discard arm already raises CS8524 today, so any switch expression in that code
  must have a discard arm, and two new named members can't make it fail to compile. Switch statements don't warn.
  `HistoryEntry.State`, `ResultView.State`, `RecentNote.State` and `DashboardNote.State` keep their type, so no
  assignment or comparison breaks. At run time, F001 to F007 never create an excusal or a pause, so the new members
  never appear there (FR-023).

**Alternatives considered**:

- *A separate `Exemption` enum, with `State` made nullable (null when exempt)*. Rejected. It changes the type of four
  public properties that the locked AC code reads: `EvaluationState s = entry.State` stops compiling, and a switch
  expression on a nullable enum raises CS8655. Every reader would also have to check two fields.
- *A separate `Exemption` field, with `State` left at the state the entry would have without the exemption*.
  Rejected. An excused commitment would read "Missed" in its `State`, which is the wrong word for a wrong rule
  (constitution I), and every existing reader, pages and AC code included, would show it.

**Cost**: the code type `EvaluationState` now has two members that `CONTEXT.md` says are not evaluation states. The
plan lists this under Complexity Tracking. `CONTEXT.md` stays as decided on 2026-11-21. The doc comment points to
the glossary.

## R-02 Domain: excusals, pauses and the exemption rule as pure types

**Decision**: a new namespace `HT.Domain.Exemptions`. "Exemption" is the spec's own word for an Excused or Paused
result. It groups code and is not a new domain term (see the Constitution Check). It holds:

- `ExemptionReason`: the reason of an excusal or a pause, with the rule of `NoteText`. It is trimmed, empty means
  none, and it has at most 200 UTF-16 code units (FR-001). `ToString` is not overridden, so a reason never reaches a
  log by accident (HP-009).
- `Excusal` (date, reason) and `Pause` (identifier, first day, last day, reason), with `PauseId`.
- `PauseTiming` (`Upcoming`, `Current`, `Past`) and `Pause.TimingAt(DateOnly today)` (FR-012).
- `GoalExemptions`: one goal's excusals and pauses. Its methods:
  - `IsPaused(date)`;
  - `ExemptionOf(CalendarEntry entry, GoalScheduleKind kind)`, which gives `Paused` when any date from the entry's
    first to its last date is in a pause (a commitment or a day: its one date; a week: any of its seven dates, D-1),
    else `Excused` when the entry is a commitment with an excusal of its date, else null (FR-003, FR-007);
  - `CurrentPause(today)`;
  - `Overlaps(first, last)`.
- `PauseRules`:
  - `Check(first, last, today, exemptions)` gives the problem of FR-006 steps 4 to 7 in that order: days missing,
    starts before today, last day before first day, overlap;
  - `Resume(pause, today)` gives either the new last day `today − 1` or "remove" when the pause began today
    (FR-010).
- `Evaluator.Evaluate` gets an overload with an `EvaluationState? exemption` argument (`Excused`, `Paused` or null).
  With an exemption, it gives that state, the progress from the records as before (FR-015, FR-017), `IsFinal` false
  and the grace end as computed. Without one, it gives the slice 5 result unchanged. The existing signature stays and
  calls the new one with null.

**Rationale**: constitution II puts evaluation rules in a deterministic function of the stored facts and a supplied
time. Pauses and excusals are stored facts, and the exemption is derived on read (FR-025, ADR 0018, ADR 0020). It
is decided by dates alone, with no clock, so `GoalExemptions` needs no instant. The order of a use case's checks
stays in Application, as in slice 5. The rules themselves (the check order of FR-006, a pause's timing, the resume
arithmetic, the exemption of an entry) are Domain functions, and boundary unit tests can cover them directly (SC-003).

**Alternatives considered**:

- *Applying the exemption only in `EntryViews` (Application)*. Rejected. It puts the D-1 week rule and the
  "pause before excusal" precedence outside the Domain, where slice 9's community figures could not reuse them.
- *Storing the Excused or Paused state per entry*. Rejected. ADR 0018 and ADR 0020 forbid stored derivations, and a
  cancelled or shortened pause would have to rewrite entries (FR-024, FR-025).

## R-03 "Today" for pauses: the goal's current date, or the account's today

**Decision**: every pause rule that compares with "today" uses one date, *the pause date*:

- FR-006 step 5 (a pause cannot start before today);
- FR-010 (resume);
- FR-011 (cancel only an upcoming pause);
- FR-012 (current, upcoming or past);
- FR-018 and FR-019 (whether the goal is paused).

The pause date is the goal's current date at the supplied time (slice 4 FR-012). For a goal that has not started
(no current date) it is the account's today, from the stored time zone or the installation default, as the slice 6
dashboard reads it (Edge Cases; AC-029, AC-086, AC-087). `PauseService` takes `InstallationTimeZone`, as
`ReportService` does. `ReportService` already has the account and the zone in its overview and dashboard.

**Rationale**: one definition keeps "paused" in the overview, the dashboard, the pauses list and the Resume and Cancel
checks consistent. For a goal that has started, a pause is a range of the goal's own dates (ADR 0016), so a time
zone change of the account never moves it (Edge Cases).

**Alternative considered**: *the account's today for every goal*. Rejected. It differs from the goal's current date
around midnight when the goal's version has another zone, and then a pause could start on a date that has already
ended on the goal's timeline, against FR-008.

## R-04 Storage: two tables, one migration, members on `IRecordStore`

**Decision**:

- **Tables.** One EF Core migration, `ExcusalsAndPauses`, adds two tables:
  - `GoalExcusals`: `GoalId`, `Date`, `Reason` (nvarchar(200), null). The key is `(GoalId, Date)`, which enforces at
    most one excusal per commitment. The FK cascades from `Goals`.
  - `GoalPauses`: `Id` (uniqueidentifier, key), `GoalId`, `FirstDay`, `LastDay`, `Reason` (nvarchar(200), null).
    The index is `(GoalId, FirstDay)`, and the FK cascades from `Goals`.

  No other column stores a time or a state (FR-025).
- **Port members.** `IRecordStore` gains six members, with the owner and the goal first like every member it has:
  - `ReadExemptionsAsync` reads all of the goal's excusals and pauses;
  - `ExcuseAsync` and `RemoveExcusalAsync`;
  - `AddPauseAsync`, `SetPauseLastDayAsync` and `DeletePauseAsync`.

  Each write uses the existing transaction that locks the goal row (`UPDLOCK, HOLDLOCK` where the goal is the
  owner's). Each write re-checks its own invariant inside that transaction:
  - `ExcuseAsync` refuses when the date already has an excusal, a record or a pause;
  - `AddPauseAsync` refuses an overlap;
  - `SetPauseLastDayAsync` and `DeletePauseAsync` find the pause only among the goal's own pauses.
- **Record adds.** `AddAsync` also refuses, inside its lock, an entry that has an excusal or a pause on one of its
  dates. A new outcome, `RecordStoreWriteOutcome.Exempt`, reports it, and the use case gives FR-005's message for it
  (R-06).

**Rationale**:

- Excusals and pauses are private facts of one goal, like records and notes. Their writes must be serialized with
  record writes on the same goal, and they all share one lock. The D-3 invariant ("an Excused commitment never has a
  record") and FR-009 ("no record is added on a paused date") then hold under concurrent requests too, not just
  for the checks the use case runs first.
- Adding members to the existing port keeps every existing constructor unchanged: `RecordingService`, `ReportService`
  and every service of slices 1 to 6. The locked AC code builds its services through `AddHtApplication` (ADR 0003), so it never
  implements a port. The unit-test fakes are updated in the same change.
- Reading all of a goal's excusals and pauses in one query costs one round trip per goal. These are small sets (a
  pause covers many dates in one row), and a range query would gain nothing.

**Alternatives considered**:

- *A separate `IExemptionStore` port*. Rejected. `RecordingService` and `ReportService` would need a new
  constructor parameter (the slices' rule is that no existing constructor changes), and the record-add re-check
  would need a second lock owner.
- *Columns on `GoalRecords`*. Rejected. A pause is a range, not a per-date row, and an excusal exists exactly when
  there is no record (D-3).
- *Reading the excusals and pauses inside `ReadAsync`*. Rejected. It would change `StoredEntries`, a public
  positional record, and slice 5's `GetRecentNotesAsync` reads per entry, where a per-range read of pauses would be
  wrong for a week.

## R-05 Application: `ExcusalService` and `PauseService`

**Decision**: two new use-case classes in a new namespace `HT.Application.Exemptions`:

- `ExcusalService`, with `ExcuseAsync(actingUser, goal, date, reason)` (FR-001, FR-002) and
  `RemoveExcusalAsync(actingUser, goal, date)` (FR-004).
- `PauseService`, with `PauseAsync(actingUser, goal, firstDay?, lastDay?, reason)` (FR-006),
  `ResumeAsync(actingUser, goal)` (FR-010), `CancelPauseAsync(actingUser, goal, pause)` (FR-011) and
  `GetPausesAsync(actingUser, goal)` (FR-012).

Each takes the acting user first. Each checks in the spec's order, beginning with slice 5's access steps (signed in,
not an administrator, the stored account, the goal among the owner's own), and then each store call passes the
owner and the goal named in the call (FR-021, FR-022). The time is `TimeProvider.GetUtcNow()`, read once per call.
The texts come from `ExcusalTexts` and `PauseTexts` (HP-008). A refusal carries no input value, so it never echoes a
reason or a date (HP-009). The services are registered in `AddHtApplication`.

The excusal checks, in FR-002's order, are as follows:

- step 4: the goal must be commitment-based;
- step 5: `GoalTimeline.EntryOf` gives a commitment for the date;
- step 6: `GoalExemptions.IsPaused`;
- step 7: the excusal already exists;
- step 8: `Evaluator.Evaluate` without an exemption, which must give Due or Pending;
- step 9: the commitment has a record, from one `ReadAsync` over that date;
- step 10: `ExemptionReason.TryCreate`.

The removal (FR-004) checks that the excusal exists, then that `now < GracePeriod.EndOf(entry.Ends)`. The end
instant itself is too late (US1-AS8, SC-003). The result carries the state after the change, evaluated with the
remaining exemptions, so it is Paused when a pause covers the date.

**Rationale**: slice 5's pattern (ADR 0005). Two classes follow the two glossary terms Excusal and Pause, and each
stays small. `RecordingService` keeps its constructor and its scope.

**Alternative considered**: *new methods on `RecordingService`*. Rejected. It would mix a third concern into a
class of 500 lines, and FR-006 to FR-012 need the account's zone (R-03), which `RecordingService` does not take.

## R-06 Slice 5 and 6 reads and writes: one shared evaluation with exemptions

**Decision**:

- **One evaluation path.** `EntryViews.Of` (internal) gains a `GoalExemptions` argument. It evaluates through the R-02
  overload and sets three things:
  - for an exempt entry, `GraceEnds` null and `IsCorrection` false (FR-017);
  - for an Excused one, the new `HistoryEntry.ExcusalReason` and `HistoryEntry.CanRemoveExcusal` (FR-017);
  - the progress from the records as before.

  Every caller passes the goal's exemptions, read once per call through `ReadExemptionsAsync`. The callers are
  `RecordingService`'s history, what can be recorded now, recent notes, the save-note result and the write results,
  and `GoalEvaluation`.
- **Adding a record** checks FR-005's steps 4a to 4c after the "day has begun" check and before the value. The plan
  reads slice 5's step (4) as both date checks, nothing scheduled and not begun (slice 5 FR-005; see Spec
  discrepancies in the plan). The steps:
  - 4a: a date in a pause, "This goal is paused on this date.";
  - 4b: a week Paused by a date other than this one, "This week is paused.";
  - 4c: an excused commitment, "This commitment is excused. Remove the excusal to record it.".

  `RecordWriteOutcome` gains `DatePaused`, `WeekPaused` and `CommitmentExcused`. The R-01 argument makes new enum members
  compile-safe. When the store reports `Exempt` (a concurrent excusal or pause), the use case reads the exemptions
  again and gives the matching message. Changing and deleting a record skip 4a to 4c, and their result gives Paused
  as the state after the change on a paused date (FR-009).
- **`GoalEvaluation`** reads the exemptions with the records. Then:
  - `Consistency.Of` and `Streaks.Of` skip `Excused` and `Paused` like Pending (FR-013, FR-014; SC-002);
  - `Ended` adds the exempt entries whose end instant is at or before the supplied time (the spec's "exempt result");
  - "begun" for the trend becomes `entry.Starts <= now`, since an exempt entry has no Upcoming state;
  - `NextUpcoming` leaves out Paused entries (FR-019). `CurrentWeekUpcoming` keeps them: it feeds only the trend,
    which takes each Upcoming commitment's exemption from the goal's `GoalExemptions`, leaves a Paused one out of the
    week's Build target and counts it towards an all-exempt week (R-07, FR-016; corrected in T036, critic-tasks pass 1
    MAJOR-4).
- **`RecentResults.Counts`** gets the keys `Excused` and `Paused` only when their count is above 0. A goal without
  exemptions keeps exactly the four keys of slice 6, so F007 is unchanged (FR-023). AC-055 lists only the states it
  has.
- **Overview and dashboard.**
  - `OverviewGoal` and `DashboardGoal` gain `PausedUntil` (the last day of the current pause at the pause date of
    R-03, or null) (FR-018, FR-019).
  - `DashboardCounts.Goals` counts the goals with no `PausedUntil`.
  - Nothing else in the order, groups or windows changes. Exempt entries are never Due, Pending or Missed, so they
    fall out of the Today list and the counts with no extra rule.

**Rationale**: slice 6 research R-04 made `EntryViews.Of` the one place where an entry is evaluated, so the history,
what can be recorded now and every report agree. Applying the exemption there keeps that.

**Alternative considered**: *applying the exemption only in the reports*. Rejected. The history and what can be
recorded now (FR-005, FR-009, FR-017) would then disagree with the reports.

## R-07 Trend marks (FR-016)

**Decision**:

- **Bar mark.** `TrendBar` gains an init-only property `Mark` of a new enum `TrendMark?` (`Excused`, `Paused`).
  It is init-only, not a new positional parameter, so the record's constructor and deconstruction stay
  source-compatible. A marked bar has a null `Value`.
- **Builder input.** `TrendBuilder.Of` gets an overload whose begun items carry the entry's exemption. The old
  signature stays and calls it with none.
- **Per-period goals and commitment-based count and duration goals**: a bar is marked with its entry's exemption.
- **Commitment-based yes/no goals**, one bar per week:
  - the Build target counts the week's commitments, and the current week's Upcoming ones, that are neither Excused
    nor Paused;
  - the value (Build and Reduce) sums the "Yes" records of those commitments only, and it is null when none of them
    has a record;
  - a week is *all exempt* when every one of its commitments is Excused or Paused, the current week's Upcoming
    ones included (they count in FR-016's target too). Such a week is marked `Paused` when one of them is Paused,
    else `Excused`. A week with an Upcoming commitment that is not Paused is not all exempt: it keeps its unmarked
    bar, with its reduced target and no value until a counted commitment has a record;
  - the Reduce target stays 0.
- **Targets of marked bars.** A marked bar's `Target` is computed by the same rules as an unmarked one: the
  entry's own target for a commitment, day or week of the other goals, and for a commitment-based yes/no week the
  count of its counted commitments, which is 0 for an all-exempt week. One rule, no special case.
- **The line.** `Trend.Target` is the target of the latest bar that is not marked, or 0 when every bar is marked or
  there is none. So an all-exempt latest week never pulls the line to 0, and a goal without exemptions keeps slice
  6's line (its latest bar is never marked).

**Rationale**: FR-016 word for word. The two yes/no rules of FR-016 (only counted commitments count, and "no value,
not 0") cover the Reduce half that the features critic noted as untested (critic-features MINOR-1). The plan covers
it with unit tests (R-11).

## R-08 Demo pause loading (FR-031, HP-010, ADR 0023)

**Decision**: a fifth operator capability, following slice 6's demo report loading:

- **`DemoPauseService.LoadAsync(demoPassword)`** (`HT.Application.Operator`):
  - `DemoEmail` = `demo6@example.com`;
  - the same password checks and results as demo report loading;
  - the run date R is the date of the supplied time in the installation's default zone;
  - an existing account gives "already exists", and nothing of it is read (ADR 0013).
- **`DemoPauses.For(R)`** builds FR-031's two goals as `DemoPauseGoal`s, each a slice 6 `DemoHistoryGoal` with its
  `DemoExcusal`s and `DemoGoalPause`s. The demo reasons are constants and pass `ExemptionReason`.
- **`IDemoPauseStore.CreateDemoPauseAccountAsync(email, password, profile, goals)`**: the store's contract is
  `IDemoReportStore`'s, with the excusals and pauses written in the same transaction. Its implementation,
  `DemoPauseStore`, reuses slice 6's internal `DemoAccountWriter`, which gains an internal method for the excusal and
  pause rows. An architecture rule fences the port to its service, as for `IDemoReportStore`.
- **Results**: `DemoPauseLoadingResult` and `DemoPauseAccountResult` give "created", "demo goals added", "demo records
  added" and "already exists", naming only the email.

**Rationale**: ADR 0023 fixes the order and the account. Reusing the writer keeps the fourth and fifth tasks alike.
The task writes the facts directly, because FR-002's and FR-006's user rules forbid past excusals and pauses
(FR-031). The figures of FR-031 hold at the run time for every weekday. The application tests prove them for all
seven weekdays of R, and the acceptance tests at a fixed run time.

**Alternative considered**: *calling `ExcusalService` and `PauseService` from the task*. Rejected. They refuse the
past dates FR-031 needs (D-2, FR-006 step 5).

## R-09 Host and smoke check (HP-010, HP-011)

**Decision**:

- **Startup.** `StartupTasks` runs "Demo pause loading" after "Demo report loading", with the same step wrapper. It
  logs "Demo pause loading: {Result}" when the password is missing or weak, and the account lines as demo report
  loading's are logged. A fault is logged without secrets and the start continues.
- **Check 8.** The smoke check gains check 8, `demo pause <email>`:
  - it signs in as `demo6@example.com`;
  - it finds the StatusChip "Excused" on the goal page of "Meditation, 10 min daily";
  - it finds an item with "Paused until" and "Drinking water, 8 glasses a day" on `/goals?tab=paused`.

  Each text is matched in its element kind, as in check 7 (slice 6 research R-11).
- **Coverage.** `SmokeCheckCoverage` gets an entry for each data parameter of `IDemoPauseStore`, and
  `specs/003-demo-hosting/contracts/smoke-check.md` gets the rows and check 8.
- **Success check.** As in slice 6, ADR 0015's success check is the smoke check, and check 8 needs `demo6`'s goals.
  So a deployment succeeds only when demo pause loading created them. No separate log check is added.

## R-10 Pages (HP-001 to HP-008)

**Decision**:

- **Goal page.** `GoalDetailBody` is already an Interactive Server island. It gains:
  - "Pause" in the head;
  - the paused Notice with "Resume now";
  - the `PausesCard` (from `GetPausesAsync`);
  - Excused and Paused rows in `HistoryCard`, with "Remove excusal" while `CanRemoveExcusal`, no "Add record" on
    exempt rows, and the record and note actions kept;
  - the strip cells and the marked bars.

  After any write it reloads, as slice 6 does.
- **New dialogs.** `PauseDialog` and `ExcuseDialog`, each one Razor component reused everywhere. Both use
  `HtDialog`'s focus handling (HP-007). The RecordDialog gets "Excuse instead" for a commitment-based goal whose
  "For" entry is Due or Pending with no record. Its "For" list already comes from what can be recorded now, which no
  longer holds exempt entries.
- **Goals page.** The existing `Tabs` component shows Active and Paused, and the tab is kept in `?tab=paused`. The
  `GoalsList` island splits the overview's goals by `PausedUntil`, in the overview's order. A Paused item has its
  meta "Paused until Sun 22 Nov" and "Resume". There is the empty text "No paused goals." and the new footnote.
- **Signed-in Home.** `TodayPanel` gains "Excuse" (ghost) on Pending rows of commitment-based goals that can be
  excused. The Active goals tile and the GoalSummary card leave out goals with `PausedUntil`.
- **Shared parts and texts.**
  - `StatusChip` gains "Excused" and "Paused".
  - `ConsistencyStrip` gains their cells, and `TrendChart` the marked bars, each with its `title` and `aria-label`.
  - Every text comes from `ReportText` (the state words, "Paused until …", the strip and trend labels, the
    footnote), `ExcusalTexts` or `PauseTexts` (HP-008).
- **Design first.** `design/` changes before the pages (constitution IV), as the spec's "Design changes before
  build" lists. The CSS classes exist already: `ht-status--excused`, `ht-status--paused`, `ht-cell--excused`,
  `ht-cell--paused`, `ht-bar--paused`. The Excused bar's class (dashed like the paused bar) is added in `design/css`
  first and then copied to `src/HT.Ui/wwwroot/css`.

## R-11 Tests

**Decision**: the test types of ADR 0006, ADR 0022 and slice 6 research R-14.

- **Domain unit tests** (`tests/HT.UnitTests/Domain/Exemptions`, `Domain/Reports`, `Domain/Evaluation`):
  - `ExemptionReason` at 200 and 201 characters, trimmed, empty;
  - `GoalExemptions.ExemptionOf` for a commitment, a day and a week with one paused date at each end, and pause
    before excusal;
  - `PauseRules.Check` with the first day at today − 1, today and today + 1, the last day one day before the first,
    a touching range (allowed) and an overlap by one day;
  - `Resume` on the first day and on a later day;
  - `Pause.TimingAt` at the first day − 1, the first day, the last day and the last day + 1;
  - `Consistency` and `Streaks` with exempt states (the SC-002 equality: the same figures with the exempt entries
    removed);
  - the trend's marks, its yes/no targets and values for Build and Reduce, and the "no value, not 0" week.
- **Application unit tests** (fakes):
  - every check order of FR-002, FR-004, FR-006, FR-010 and FR-011, including the pairs the AC critic named
    (FR-002 steps 5/6, FR-006 steps 6/7, FR-011 steps 4/5);
  - FR-005's steps 4a to 4c, placed after "not begun" and before the value (an invalid value on a paused date gives
    the pause text);
  - excusing and removing at the grace end − 1 s, at it and at + 1 s (SC-003);
  - pausing before, on and after the pause date, and resuming on the first day and later (SC-003);
  - every result that existed when a pause was set keeping its state (SC-004);
  - negative tests for another user, an administrator and the anonymous visitor on each of the six use cases
    (SC-005);
  - `DemoPauses.For` figures for R on all seven weekdays.
- **Integration tests** (SQL Server):
  - the migration;
  - owner scoping of every new store member (another owner's goal and pause behave like none);
  - the in-transaction re-checks (an excusal against a record, a record against an excusal or a pause, overlapping
    pauses);
  - the cascade from an account through its goals to their excusals and pauses, as for records and notes (no goal
    is deleted on its own in v1);
  - demo pause loading as a whole or not at all;
  - log capture proving that no reason, note, value or name is logged (HP-009);
  - slice 6's SC-007 timing test, re-run with the extra read per goal.
- **UI tests** (bUnit and host): the dialogs (focus, Esc, refusal placement), the goal page's Notice, Pauses card and
  rows, the Goals page's tabs and `?tab=paused`, the Home's "Excuse", the strip and trend labels, the startup lines
  and the smoke check. The tests the spec's "Earlier requirements and tests this slice changes" names are updated in
  the step that changes their page.
- **Architecture tests**:
  - `HT.Domain.Exemptions` and `HT.Application.Exemptions` are covered by the layer and clock rules;
  - the existing `IGoalStore` rule of `ApplicationDependencyTests` admits `ExcusalService` and `PauseService` (the
    rule names each service that may use the port; without the change the new services fail it). Application has no
    `IRecordStore` rule to widen;
  - a new fence keeps `IDemoPauseStore` for `DemoPauseService` only;
  - `UseCaseParameterTests` covers the new port members (owner and goal first) and use cases (acting user first).
- **Mutation testing** (ADR 0022): Stryker over `HT.Domain` and `HT.Application` after step 3. A surviving mutant in
  the new code is fixed with a test.
- **Acceptance tests**: written and judged by the AC workflow after `/speckit-implement`, all six features with
  acceptance tests (F001, F002, F004, F005, F007, F008)
  included.

## R-12 Privacy and logging (FR-021, FR-022, HP-009)

**Decision**:

- **Owner scoping.** Every new use case and store member is owner-scoped as R-04 and R-05 describe. A pause is
  looked up only among the pauses of the acting user's goal named in the call. A pause of another goal gives "This
  pause does not exist." (US5-AS4), the same as no pause.
- **No values in faults or logs.** `ExemptionReason`, like `NoteText`, has no `ToString`. The store's faults name
  the SQL error number only, as `RecordStore` does now, and the new services log nothing.
- **Demo account.** Its reasons are written only for the account the task creates.
