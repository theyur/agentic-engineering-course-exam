# Implementation Plan: Recording and evaluation

**Branch**: `claude/speckit-roadmap-slice-5-bd9f2d` | **Date**: 2026-11-18 | **Spec**: [spec.md](spec.md)

**Input**:

- the feature specification `specs/005-recording-and-evaluation/spec.md`;
- the acceptance criteria [acceptance.md](acceptance.md) and
  `tests/HT.AcceptanceTests/Features/F005/recording-and-evaluation.feature`, both locked;
- [ADR 0018](../../docs/adr/0018-evaluation-on-read-and-demo-history.md) (evaluation on read, demo history
  loading, the daily refresh), with ADR 0016 and ADR 0017 (time zones).

Revised after [critic-plan-pass-1.md](critic-plan-pass-1.md) and, for its MINOR notes,
[critic-plan-pass-2.md](critic-plan-pass-2.md).

## Summary

A registered user records what happened, for one date of one of their own goals: Yes or No, or a whole amount,
below, at or above the target, optionally with a note. A commitment has one record; a day or week of a
period-based goal has up to 50. Every commitment and period has a state, computed when it is read from its
records, the settings version of its date and the supplied time:

1. Upcoming;
2. Due;
3. Pending for exactly 48 elapsed hours after its end;
4. then Met, Failed or Missed.

A record changed after that is a correction, evaluated by the same function. Nothing runs in the background (ADR
0018). A new startup task, demo history loading, creates `demo4@example.com` with 28 days of results, and the
hosted demo is refreshed daily so those results stay current.

The work splits by layer:

- **`HT.Domain`** gains `Records` (record values, notes, limits) and `Evaluation` (state, grace period, progress).
  Both are pure functions with no clock. `GoalTimeline` gains the entry a date belongs to (research R-01 to R-03).
- **`HT.Application`** adds `RecordingService`, with seven owner-scoped use cases, and the port `IRecordStore`. It
  also adds `DemoHistoryService` with its own port, and one shared formatter, `RecordingText`. No existing
  constructor, method or port changes (FR-019, research R-04, R-08).
- **`HT.Persistence`** adds `GoalRecords` and `GoalNotes` in the migration `Records`. Every write runs in a
  transaction under the goal row's update lock (research R-05, R-06).
- **`HT.Ui`** turns the goal page's body into one interactive island with "Record", History, Recent notes and the
  Record and Note dialogs. The host gets a third startup task, the smoke check a check 6, and the hosting workflow
  a daily schedule. `design/` changes first (constitution IV, ADR 0008).

## Technical Context

**Language/Version**: C# on .NET 10 (`net10.0`), with nullable reference types and implicit usings on, and
warnings as errors. Unchanged.

**Primary Dependencies**: unchanged. That is ASP.NET Core 10 (Blazor static SSR, and Interactive Server for the
goal page's body), EF Core 10 with SQL Server, Identity's EF stores, and NodaTime 3.2.x inside `HT.Domain` only
(ADR 0017). No new package.

**Storage**: SQL Server, the databases of slices 1 to 4. The new migration `Records` adds `GoalRecords` and
`GoalNotes` ([data-model.md](data-model.md#tables-htpersistence-migration-records)).

**Testing**:

- xUnit v2, ArchUnitNET and reflection rules, bUnit and `WebApplicationFactory` host tests, and `FakeTimeProvider`
  (ADR 0006).
- The acceptance tests go through the AC workflow (ADR 0003).
- Details: [research.md R-14](research.md#r-14-tests).

**Target Platform**: a browser-based web app over HTTPS. Windows developer machines, CI on Ubuntu, and the hosted
demo on Azure App Service, refreshed daily by GitHub Actions (ADR 0015, ADR 0018).

**Project Type**: a web application (Blazor host) over the layered .NET solution.

**Performance Goals**: demo scale.

- SC-001 (3 actions, under 30 seconds) is a user-task target.
- A history read is one goal read and one store read, for at most 366 entries with up to 50 records each,
  evaluated in memory.
- A write is one short transaction on one goal row.

**Constraints**:

- No clock, time-zone or culture read in Domain or Application (constitution II). Time enters as `TimeProvider`,
  read once per call.
- Records and notes are reached only through owner-scoped calls, with no exception (constitution III, FR-017).
- No note, recorded value, unit, goal name or habit name in any log, exception or error page (HP-006).
- The use cases, constructors and ports of slices 1 to 4 stay as they are, so F001, F002 and F004 pass unchanged
  (FR-019, FR-031).
- `design/` comes first, and the pages use tokens only (constitution IV).

**Scale/Scope**:

- 7 new use cases in `RecordingService`, and 1 operator use case (`DemoHistoryService`).
- 2 new ports and 2 new tables.
- 1 changed page (the goal page), with 7 new components (StatusChip, GoalDetailBody, HistoryCard,
  RecentNotesCard, RecordDialog, RecordFields, NoteDialog), and the New goal help line.
- 1 new startup task, 1 new smoke check and 1 new workflow trigger.
- 3 demo goals for one new demo account.

## Constitution Check

*GATE: checked before Phase 0 and again after Phase 1. Pass, with no exception. No `[NEEDS CLARIFICATION]` marker
is open: D-1 to D-9 are decided in the spec's `## Clarifications` and ADR 0018. The ten details the plan found
open were accepted by the owner on 2026-11-18 and written into the spec (see "Resolved in the spec").*

| Principle | Status | How this plan meets it |
|---|---|---|
| I. Ubiquitous Domain Language | met | Type and member names use the glossary: `GoalRecord`, `RecordValue`, `NoteText`, `EvaluationState` (`Upcoming`, `Due`, `Pending`, `Met`, `Failed`, `Missed`), `GracePeriod`, `IsCorrection`, `DemoHistoryService` (demo history loading). Every one of these terms already has a `CONTEXT.md` entry with a NOT-boundary: they were added with the spec (HP-012, commit dbc5a50). The StatusChip says "Met" for Reduce goals, never "Within limit", "done", "skipped", "late" or "overdue" (HP-004). `CalendarEntry`, `EntrySpan`, `Progress` and `RecordingText` are code helpers, not new domain terms. |
| II. Deterministic, Reproducible Evaluation | met | One pure function, `Evaluator.Evaluate`, gives every state from the records, the target of the version in effect for the entry's date (`GoalTimeline.VersionOf`) and the supplied instant. Nothing is stored or scheduled (ADR 0018, HP-010). The grace end is computed on UTC instants, so it lasts exactly 48 elapsed hours across a clock change (D-1). A correction is computed by the same function as an on-time record. The unit tests prove that the two are equal (SC-003). Boundary tests cover every combination at, below and above its target, and both sides of each start, end and grace end, including both Kyiv clock changes (SC-002, R-14). The clock rules cover the new namespaces (R-13). |
| III. Privacy Enforced Below the UI | met | `RecordingService` takes the acting user first and refuses the anonymous visitor and administrators before any identifier (FR-018). Every `IRecordStore` member takes the owner and the goal and acts only on the owner's goal, in the same statement. A record is found only among that goal's records, never by identifier alone (FR-017, R-05). An unknown goal and another account's goal give the same "This goal does not exist.", and the same holds for records. Every use case has negative tests for another registered user, an administrator and the anonymous visitor (SC-004). Results carry no input value, the stores raise no error with a value, and log capture proves it (HP-006, SC-006). Demo history loading writes only for the account it creates, in one transaction (ADR 0013). |
| IV. Design-System Fidelity and Accessible UI | met | `design/` changes first (Work order step 1, the spec's "Design changes before build"). StatusChip, RecordDialog with RecordForms, NoteDialog, the History DataTable and the Recent notes card are each one Razor component; the existing Stepper, SegmentedControl, HtDialog, TextArea and DataTable are reused. One shared, unit-tested formatter, `RecordingText`, gives every state word, progress text, recorded value and "Pending, N h left" (HP-011). Statuses always carry their text. The dialogs manage focus: the first field on open, Esc and × close, and focus returns to the opener. Icon-only buttons have an `aria-label` (R-12). |
| V. Verified, Not Asserted | met | Build and the full suite run before any report of done. Steps 3 and 4 of the Work order are verified and committed together. The new Domain, Application and Persistence types come with their architecture rules (R-13). HP-006 and HP-010 get executed checks: log capture, and a read after the grace period with nothing in between. Both races are forced in integration tests. The pre-commit gate stays on. What tests cannot cover is labelled unverified, with a quickstart step: the real daily schedule and the hosted smoke check. |
| VI. Demonstrable Vertical Slices | met | Domain rules, use cases, persistence, pages, tests and demo data ship together. `demo4@example.com` shows Met, Failed with partial amounts, Missed, Pending and Due, and notes, relative to the run date (FR-030). The daily refresh keeps them current on the hosted demo until 2027-01-31 (HP-009). After every deployment and refresh, smoke check 6 confirms all of SC-005: Met, Failed, Missed, Pending, Due, the partial 12 minutes and a note. That is HP-008's list plus Due, the amount and the note (research R-10). There is no infrastructure beyond one workflow trigger (ADR 0018). |
| Decision Discipline | met | SPEC §11.1 (second part) and §11.4 are decided in the spec's `## Clarifications` (D-1, D-3). ARCHITECTURE §9's scheduled evaluation and §7's time-relative demo data are decided in ADR 0018 (D-4, D-5). This plan decides nothing that spans features. The ten details in "Resolved in the spec" were accepted by the owner and are now in the spec's FR and HP text, with a `## Clarifications` session "(plan)". |

## Project Structure

### Documentation (this feature)

```text
specs/005-recording-and-evaluation/
├── spec.md              # /speckit-specify, /speckit-clarify, critic revisions
├── acceptance.md        # /speckit-ac-translate (locked)
├── ac/                  # locks and AC run records
├── plan.md              # this file
├── research.md          # Phase 0
├── data-model.md        # Phase 1
├── quickstart.md        # Phase 1
├── contracts/
│   ├── application-api.md
│   └── host.md
└── tasks.md             # /speckit-tasks (not created here)
```

### Source Code (repository root)

```text
src/
├── HT.Domain/
│   ├── Records/                        # RecordId, YesNoAnswer, RecordValue, GoalRecord, NoteText, RecordProblem,
│   │                                   #   RecordLimits
│   ├── Evaluation/                     # EvaluationState, GracePeriod, ProgressKind, Progress, EntryEvaluation,
│   │                                   #   Evaluator
│   └── Goals/                          # + CalendarEntry; GoalTimeline + EntryOf, HasBegun, TryEntries
├── HT.Application/
│   ├── Records/                        # RecordingService, IRecordStore, EntrySpan, StoredEntry, StoredEntries,
│   │                                   #   RecordStoreWrite, RecordInput, RecordView, HistoryEntry, GoalHistory,
│   │                                   #   RecentNote, results, RecordTexts, RecordingText
│   ├── Accounts/DateTexts.cs           # + WeekRange ("10 – 16 Aug")
│   ├── Operator/                       # + DemoHistoryService, DemoHistory, DemoHistoryGoal, DemoRecord, DemoNote,
│   │                                   #   DemoHistoryLoadingResult, DemoHistoryAccountResult, DemoRecordsResult,
│   │                                   #   IDemoHistoryStore; OperatorTexts + DemoRecordsAdded
│   └── ApplicationServiceCollectionExtensions.cs   # + RecordingService, DemoHistoryService
├── HT.Persistence/
│   ├── Records/                        # GoalRecordRow, GoalNoteRow, RecordMapping, RecordStore
│   ├── Habits/DemoHistoryStore.cs      # demo4 with goals, records and notes, one transaction
│   ├── HtDbContext.cs                  # the two tables, keys, index, identity
│   ├── PersistenceServiceCollectionExtensions.cs   # + IRecordStore, IDemoHistoryStore
│   └── Migrations/                     # + Records
└── HT.Ui/
    ├── Operator/StartupTasks.cs        # + Demo history loading step and its lines
    └── Components/
        ├── Shared/StatusChip.razor
        ├── Records/                    # GoalDetailBody, HistoryCard, RecentNotesCard, RecordDialog, RecordFields,
        │                               #   NoteDialog
        ├── Goals/NewGoalForm.razor     # HP-005 help line
        └── Pages/GoalDetail.razor      # static shell, 404, renders GoalDetailBody

tools/HT.SmokeCheck/                    # check 6 (11 texts); SmokeCheckCoverage with a port column, + 4 entries
.github/workflows/deploy-demo.yml       # schedule; job gate (mode, run); job demo needs gate

tests/
├── HT.UnitTests/                       # Domain/Records, Domain/Evaluation, Domain/Goals (entries),
│                                       #   Application/Records, Application/Operator (DemoHistory);
│                                       #   Fakes/ (+FakeRecordStore, FakeDemoHistoryStore)
├── HT.ArchitectureTests/               # R-13 rules
├── HT.IntegrationTests/Records/        # stores, scoping, order, limit, races, migration, demo history, log capture
├── HT.UiTests/                         # components, goal page host tests, StartupLogTests, StartupTasksDemoTests,
│                                       #   smoke check runner and coverage
└── HT.AcceptanceTests/                 # outside the solution; written by the AC workflow

design/                                 # changed first (Work order step 1)
docs/operations/hosted-demo.md          # daily refresh, teardown removes the trigger
specs/003-demo-hosting/contracts/       # deploy-workflow.md, smoke-check.md (coverage table)
docs/roadmap.md, README.md              # status
```

**Structure Decision**: the four production projects stay, and no project is added. Records and evaluation get
their own namespaces in Domain, because slices 6 to 9 count their states without needing goal creation. The
use cases get their own `Records` folder in Application and Persistence. The new ports sit with the services that
use them, public and fenced by architecture rules (ADR 0010). Their implementations are internal to
`HT.Persistence`.

## Work order

`/speckit-tasks` should follow this order, because later steps need earlier ones:

1. **Design first** (constitution IV, ADR 0008). The spec's "Design changes before build" list:
   - `PageGoalDetail`'s slice-5 state, History row actions and Recent notes;
   - RecordDialog and RecordForms (no "Excuse instead", the Reduce rule without "Provisional", "For" per week and
     fixed from a row, the note help, "Edit" with "Delete record");
   - StatusChip ("Met" for Reduce, "Due this week"), together with `CommitmentRow.html` and
     `PageHomeSignedIn.html`;
   - NoteDialog (the "Add note" entry and its `.md`);
   - `RecordDialog.md`'s "For" rule;
   - `PageNewGoal.md`'s slice-4 note;
   - `design/pages.md` and `design/README.md`.

   Then copy any changed `design/css` file to `src/HT.Ui/wwwroot/css`.
2. **Domain**:
   - `HT.Domain.Records` and `HT.Domain.Evaluation`;
   - `CalendarEntry` and the `GoalTimeline` members;
   - the boundary unit tests (R-14), including both Kyiv clock changes;
   - the Domain architecture rules (R-13).
3. **Application** and 4. **Persistence**, verified and committed **together**. After step 3 alone,
   `AddHtApplication` registers `RecordingService` and `DemoHistoryService`, whose ports have no implementation.
   That fails `ValidateOnBuild` in every integration and host test, as in slices 2 and 4.
   - Step 3:
     - `RecordingService`, its inputs, views, results and texts;
     - `RecordingText`;
     - `DemoHistoryService` and `DemoHistory`;
     - `DateTexts.WeekRange`;
     - `FakeRecordStore`, `FakeDemoHistoryStore` and the unit tests;
     - the Application rules, including the widened `IGoalStore` fence in `ApplicationDependencyTests` (research
       R-13), which otherwise fails as soon as `RecordingService` exists.

     The bUnit page services (`tests/HT.UiTests/Components/Pages/PageServices.cs`) register `FakeRecordStore` in
     the same commit, with its `<Compile Include>` link in `HT.UiTests.csproj`.
   - Step 4:
     - the rows, the mapping and `HtDbContext`;
     - `RecordStore`, with R-05's transactions;
     - `DemoHistoryStore`;
     - the migration `Records`;
     - the integration tests, including the races, the migration on `HT-IT-MIGRATE` and HP-006 log capture.
5. **Host**:
   - the "Demo history loading" startup step and its log lines (HP-007);
   - `StartupLogTests` and `StartupTasksDemoTests`, with demo4's lines after demo loading's;
   - smoke check 6 and the coverage port column with its 4 entries (HP-008, SC-005). In the same change:
     - `RunnerBehaviourTests.GoodSite()` scripting `/goals` and the two goal pages for demo4;
     - `SmokeCheckHostTests`' exact list of check names and expected lines;
     - `CoverageTests`;
     - the coverage table in `specs/003-demo-hosting/contracts/smoke-check.md`;
   - the workflow's schedule and `gate` job (HP-009), with `contracts/deploy-workflow.md` and
     `docs/operations/hosted-demo.md`.
6. **Pages**:
   - StatusChip;
   - the goal page's static shell and `GoalDetailBody`;
   - HistoryCard and RecentNotesCard;
   - RecordDialog with RecordFields, and NoteDialog;
   - the New goal help line (HP-005), with `NewGoalPageTests`' `DoesNotContain("partial")` (line 193) replaced by
     a check of that line.

   Each comes with bUnit and host tests. The existing goal-page UI tests of slice 4 are updated for the slice-5
   state (spec "Slice 4 requirements and tests this slice changes"). Research R-14 lists every existing test this
   slice changes.
7. **Docs**:
   - the roadmap status and the README status line;
   - slice 3's texts that the spec lists (the "Demo refresh" entity, SC-006, HP-018 and the roadmap rows), through
     notes that cite ADR 0018;
   - `CONTEXT.md`, only if implementation sharpens a term (HP-012 is already done).
8. **AC tests**: `/speckit-ac-tests-write` and `/speckit-ac-tests-analyze`, which the owner starts after
   `/speckit-implement`.

## Gates before the next step

The plan found ten details the spec's words left open (constitution, Decision Discipline). None changes a locked
acceptance criterion, and none spans features, so no ADR is proposed. **The owner accepted all ten on
2026-11-18, as proposed.** They are written into the spec's FR and HP text and recorded in its `## Clarifications`
as "Session 2026-11-18 (plan)".

That edit came after the `sources` lock. The owner ran `ac_lock.py confirm-spec sources` on 2026-11-18, and
`ac_lock.py verify sources` now passes against the current spec. No gate is open, and no roadmap decision is due
before this slice's `/speckit-tasks`.

## Resolved in the spec

These were found while planning. Each point gives what the spec left open and the position the owner accepted,
which is now in the spec at the requirement named.

1. **FR-005 step (4), the date, for a change.** FR-005 lists the date among the checks of "recording or changing".
   A change never changes a record's date (FR-003). In this slice versions differ only in their time zone
   (FR-015), so a stored date always passes FR-002 again. **Accepted**: change and delete have no date step.
   Their order is the goal, the record, the value and the note (change), or the goal and the record (delete).
   Slice 8, which changes schedules, revisits this.
2. **FR-004: a value of only the wrong kind.** "Such values are not refused and not stored." **Accepted**:
   - with both kinds given, the one that belongs is kept and the other dropped;
   - with only the wrong kind given (an amount on a yes/no goal, or Yes on a count goal), the input is "no value"
     and gets the measurement's message ("Choose Yes or No." or the range message), as FR-005 step (5) says for a
     missing value.

   AC-009 checks only what is stored when both kinds are given.
3. **FR-003: re-recording a commitment.** "Replaces its value." **Accepted**: the existing record keeps its
   identifier and its stored order, and gets the new value. A change likewise keeps the record's order (AC-033 row
   04 does not depend on it).
4. **FR-009: a per-week period that a range covers only in part.** **Accepted**: the entry is listed, as
   slice 4's calendar lists every week that overlaps the range. It gives all of its records, also those with dates
   outside the range, so its progress never depends on the range (research R-07). The acceptance criteria avoid
   this case ("Ranges" convention).
5. **FR-011: "newest first".** **Accepted**: by first date, descending. After a move east, the new day comes
   before the old day.
6. **FR-008: "the latest dates" of a per-week note.** **Accepted**: a per-week period's note is ordered by its
   first date, the Monday, which is also its key.
7. **HP-001: "the commitments or periods that have begun", "Show all N →".** FR-009 limits one history read to 366
   days. **Accepted**: the History card lists the begun entries of the last 366 days up to the goal's current
   date. "Show all N →" expands within those, and older entries are not listed in this slice. At demo scale no
   goal is older than about two months.
8. **HP-002: "Pending, N h left".** **Accepted**: N is the whole hours from the supplied time to the grace end,
   rounded up, so the last hour reads "1 h left".
9. **HP-008: "every new data parameter of the demo data port".** Demo history loading gets its own port,
   `IDemoHistoryStore`, so that `IDemoDataStore` stays unchanged for code outside the solution (research R-08).
   **Accepted**: the coverage rule covers both demo ports, and its map gains a port column (research R-10).
10. **FR-014: the progress of a commitment without a record, and of a yes/no commitment.** "A commitment's recorded
    value against the target" names no number for these cases. **Accepted**:
    - with no record, the value is absent and the target is given;
    - a yes/no commitment counts Yes as 1 and No as 0, against 1.

    The pages show the recorded value, or "—", rather than these numbers. No acceptance item checks them.

## Risks

- **The locked AC support code of F001, F002 and F004 must keep compiling and passing**, and it may not be read.
  This plan changes no existing constructor, method, port member or result type. It adds `RecordingService`,
  `DemoHistoryService`, two ports, two DI registrations and a migration that `ResetAsync` applies. Support code
  that builds services by hand is untouched. Support code that composes `AddHtApplication` with
  `AddHtPersistence` gets working registrations. `/speckit-ac-tests-analyze` runs all four features.
- **F001 and F004 demo scenarios** reset `HT-AC` and run demo loading only. Demo history loading is a separate call
  that they never make, so their demo results and demo accounts are unchanged (FR-031). Only the host runs both.
- **The goal page becomes partly interactive**. The 404 for another account's goal stays in the static shell. The
  interactive body opens a circuit per visit and re-reads the goal in its own scope, with the acting user from the
  revalidating provider, as slice 2's interactive pages do. Prerendering must stay on, or the smoke check and the
  first paint lose the History rows; a host test checks the rows in the first HTML.
- **The daily schedule**: GitHub may delay or drop a scheduled run. Every refresh empties the database and ends
  all sessions, and the hosted demo is down for about 10 minutes a day (ADR 0018). A run that fails after emptying
  the database leaves the demo down until the next run. The Operator guide covers starting one by hand.
  Scheduled runs carry no `inputs`. The `gate` job's mode fallback and date check are the only new logic, and the
  workflow has no offline test (`tools/hosting/selftest.ps1` covers provisioning only).
- **GitHub's 60-day rule**: scheduled workflows of a repository with no activity for 60 days are turned off. The
  schedule lasts to 2027-01-31 only with repository activity on or after 2026-12-02. The exam is on 2026-12-14,
  so that is likely but not certain. The Operator guide says how to turn the schedule back on (research R-11).
- **Clock-change tests** depend on NodaTime's embedded tzdb having the Kyiv rules for 2026 and 2027, which the
  current package has (ADR 0017).
- **Write contention**: every record write on a goal takes that goal row's update lock. Only one user writes a goal,
  so contention needs two tabs saving at the same moment.
- **History size**: a 366-day read of a per-day goal with 50 records a day is 18,300 rows. That is far beyond demo
  data, but still one indexed range read.

## Complexity Tracking

No Constitution Check violation needs justifying.
