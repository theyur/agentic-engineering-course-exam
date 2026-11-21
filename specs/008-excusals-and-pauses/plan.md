# Implementation Plan: Excusals and pauses

**Branch**: `claude/speckit-specify-slice-7-c0f9d5` | **Date**: 2026-11-21 | **Spec**: [spec.md](spec.md)

**Input**:

- the feature specification `specs/008-excusals-and-pauses/spec.md`;
- the acceptance criteria [acceptance.md](acceptance.md) and
  `tests/HT.AcceptanceTests/Features/F008/excusals-and-pauses.feature`, both locked;
- [ADR 0024](../../docs/adr/0024-excusal-and-pause-rules.md) (D-1 to D-3, the early resume and the excusal-removal
  window) and [ADR 0023](../../docs/adr/0023-demo-pause-loading.md) (demo pause loading), with ADR 0018 and ADR 0020
  (evaluation and figures on read).

## Summary

A registered user can excuse one Due or Pending commitment, with an optional reason. They can take the excusal back
while its grace period is open. They can pause any goal for a date range, resume it early and cancel a pause that
has not begun. Excused and Paused entries take the place of the evaluation state:

- consistency and streaks skip them;
- recent results, the history and notes show them;
- the trend marks them;
- the dashboard, the Goals page and the Home stop expecting anything of them.

A fifth startup task, demo pause loading, creates `demo6@example.com` with excused commitments, a past and a current
pause.

The work splits by layer:

- **`HT.Domain`** gains `Exemptions`:
  - the reason, the excusal and the pause;
  - `GoalExemptions`, which decides whether an entry is Excused or Paused (a week by D-1);
  - `PauseRules` (the FR-006 checks and the resume arithmetic).

  `EvaluationState` gains `Excused` and `Paused`, and `Evaluator` an overload that applies an exemption.
  `Consistency` and `Streaks` skip both. `TrendBar` gets a `Mark`, and `TrendBuilder` an overload with FR-016's
  rules (research R-01, R-02, R-07).
- **`HT.Application`** adds `ExcusalService` (excuse, remove) and `PauseService` (pause, resume, cancel, list). Both
  are owner-scoped and take the acting user first (R-05). Slice 5's shared `EntryViews.Of` applies the exemption, so
  every read of slices 5 and 6 agrees: the history, what can be recorded now, notes, reports, the overview and the
  dashboard. `RecordAsync` gets FR-005's steps 4a to 4c. The overview and the dashboard get `PausedUntil` (R-03,
  R-06). Demo pause loading adds `DemoPauseService`, `DemoPauses` and the port `IDemoPauseStore` (R-08).
- **`HT.Persistence`** gets one migration with the tables `GoalExcusals` and `GoalPauses`, and six new
  `IRecordStore` members on `RecordStore`. Each write runs under the goal-row lock and re-checks its invariant there.
  `AddAsync` refuses an exempt entry under the same lock (R-04). `DemoPauseStore` reuses slice 6's
  `DemoAccountWriter` (R-08).
- **`HT.Ui`**:
  - the goal page gets Pause, the paused Notice, the Pauses card, Excused and Paused rows and cells, and marked
    trend bars;
  - there are two new dialogs, PauseDialog and ExcuseDialog, and the RecordDialog gets "Excuse instead";
  - the Goals page gets the Active and Paused tabs;
  - the Home gets "Excuse" and an active-goals count that leaves out paused goals;
  - the host gets the fifth startup task, and the smoke check gets check 8.

  `design/` changes first (constitution IV, ADR 0008; R-09, R-10).

## Technical Context

**Language/Version**: C# on .NET 10 (`net10.0`), with nullable reference types and implicit usings on, and
warnings as errors. Unchanged.

**Primary Dependencies**: unchanged. ASP.NET Core 10 (Blazor static SSR with Interactive Server islands), EF Core 10
with SQL Server, Identity's EF stores, and NodaTime 3.2.x inside `HT.Domain` only (ADR 0017). No new package.

**Storage**: SQL Server, the databases of slices 1 to 6, plus one migration, `ExcusalsAndPauses`, with two tables
([data-model.md](data-model.md#tables-migration-excusalsandpauses)). They store only the dates and the reason
(FR-025). The reads and writes go through new owner-scoped members of `IRecordStore` (R-04).

**Testing**:

- xUnit v2, ArchUnitNET and reflection rules, bUnit and `WebApplicationFactory` host tests, and `FakeTimeProvider`
  (ADR 0006).
- Stryker for the new Domain and Application code (ADR 0022).
- The acceptance tests go through the AC workflow (ADR 0003).
- Details: [research.md R-11](research.md#r-11-tests).

**Target Platform**: a browser-based web app over HTTPS. Windows developer machines, CI on Ubuntu, and the hosted
demo on Azure App Service, refreshed daily (ADR 0015, ADR 0018). Unchanged.

**Project Type**: a web application (Blazor host) over the layered .NET solution.

**Performance Goals**: each goal costs one more round trip, `ReadExemptionsAsync`, on every read that evaluates it:

- a goal report costs 5 round trips;
- a dashboard or an overview costs about 2 + 3 per goal.

Everything is evaluated in memory. Slice 6's SC-007 timing test (3 goals, one with 366 daily results, each read
under 2 seconds) is re-run unchanged and must still pass. Everything else is demo scale.

**Constraints**:

- No clock, time-zone or culture read in Domain or Application (constitution II). Time enters as `TimeProvider`,
  read once per call.
- Excused and Paused are derived on read. Nothing derived is stored (FR-025, ADR 0018, ADR 0020).
- Every new use case and store member is owner-scoped. A pause is found only among the pauses of the named goal of
  the acting user (constitution III, FR-021).
- No reason, note, recorded value, goal or habit name in any log, exception or error page (HP-009).
- **Locked AC code.** The constructors, method signatures, positional records and port members of slices 1 to 6
  stay as they are, so the locked AC code of F001 to F007 compiles and passes unchanged (FR-023). Only enum members,
  init-only properties, overloads, new port members and new types are added (R-01, R-04).
- `design/` comes first, and the pages use tokens only (constitution IV).

**Scale/Scope**:

- 6 new use cases in 2 services, and 1 operator use case (`DemoPauseService`).
- 6 new port members on `IRecordStore`, 1 new port (`IDemoPauseStore`), 2 new tables and 1 migration.
- 3 changed pages (goal page, Goals, signed-in Home).
- 3 new components: PauseDialog, ExcuseDialog and PausesCard. 4 changed ones: StatusChip, ConsistencyStrip,
  TrendChart and RecordDialog. 2 changed shared items: `CommitmentRow` ("Excuse") and `GoalItem` ("Paused until",
  "Resume"). 2 changed islands: GoalsList and TodayPanel.
- 1 new startup task and 1 new smoke check.
- 2 demo goals for one new demo account.

## Constitution Check

*GATE: checked before Phase 0 and again after Phase 1. Pass, with one justified exception (Complexity Tracking). No
`[NEEDS CLARIFICATION]` marker is open. SPEC §11.2 and §11.3 are decided as D-1 and D-2, D-3 and D-4 are decided,
and all are recorded in ADR 0023 and ADR 0024. Spec discrepancy 1 below goes to the owner. It does not block
planning, because the normative FR-013 decides the behaviour.*

| Principle | Status | How this plan meets it |
|---|---|---|
| I. Ubiquitous Domain Language | met, with one exception | **Names.** Types and members use the glossary: `Excusal`, `Pause`, `ExcusalService`, `PauseService`, `ExcuseAsync`, `RemoveExcusalAsync`, `PauseAsync`, `ResumeAsync`, `CancelPauseAsync`, the states `Excused` and `Paused`, `PausedUntil`, demo pause loading. No "skip", "snooze", "holiday mode" or "exempted day".<br>**Code groupings.** `Exemptions` (namespace), `GoalExemptions`, `ExemptionReason` and `PauseTiming` group code. They are not new domain terms: "exemption" is the spec's own word for an Excused or Paused result.<br>**Glossary.** The glossary entries of HP-012 were added with the spec on 2026-11-21.<br>**Exception.** The code enum `EvaluationState` gains `Excused` and `Paused`, which `CONTEXT.md` says are not evaluation states (R-01, Complexity Tracking). |
| II. Deterministic, Reproducible Evaluation | met | **One function.** An entry's Excused or Paused state is a pure function of its dates and the goal's stored excusals and pauses (`GoalExemptions.ExemptionOf`, D-1 for weeks), applied in the one shared `EntryViews.Of`. Nothing is stored (FR-025).<br>**Next read.** Every change (excuse, remove, pause, resume, cancel, correct) shows on the next read through the same function (FR-024).<br>**Boundaries.** Unit tests cover both sides of each boundary: excusing and removing at the grace end − 1 s, at it and at + 1 s; a pause starting at today − 1, today and today + 1; resuming on the first day and later; a week with one paused date at each end; 200 and 201 characters (SC-003, R-11).<br>**Clock rule.** `ClockReadTests` already covers every Domain and Application type, the new ones included. |
| III. Privacy Enforced Below the UI | met | **Access.** `ExcusalService` and `PauseService` take the acting user first and refuse the anonymous visitor and administrators before the goal (FR-022).<br>**Scoping.** Each store member takes the owner and the goal and acts only when the goal is the owner's, in the same statement. A pause is found only among that goal's pauses, and an excusal by goal and date (FR-021). There is no lookup by pause identifier alone.<br>**Tests.** Each of the six use cases has negative tests for another user, an administrator and an anonymous visitor (SC-005), and an integration test proves store scoping.<br>**Logs.** `ExemptionReason` has no `ToString`, and log capture proves HP-009.<br>**Demo data.** Demo pause loading writes only for the account it creates, through a port fenced to its service (ADR 0013). |
| IV. Design-System Fidelity and Accessible UI | met | **Design first.** `design/` changes first (Work order step 1).<br>**Components.** PauseDialog, ExcuseDialog and PausesCard are each one Razor component, reused. StatusChip, ConsistencyStrip and TrendChart gain the existing `ht-status--excused`, `ht-status--paused`, `ht-cell--excused`, `ht-cell--paused` and `ht-bar--paused` classes, plus the Excused bar's class, added in `design/css` first.<br>**Texts.** All texts come from `ReportText`, `ExcusalTexts` and `PauseTexts` (HP-008).<br>**Accessibility.** States carry a word or a shape. The strip and trend labels count Excused and Paused. The dialogs manage focus through `HtDialog` (HP-006, HP-007). |
| V. Verified, Not Asserted | met | **Verification.** Build and the full suite run before any report of done. Steps 3 and 4 of the Work order are verified and committed together.<br>**Architecture rules.** The new namespaces get their layer rules. The existing `IGoalStore` rule (`ApplicationDependencyTests`) admits `ExcusalService` and `PauseService`, and a new rule fences `IDemoPauseStore` to `DemoPauseService`. Stryker runs over the new code (ADR 0022).<br>**Enforcing mechanism.** The refusal checks that must hold under concurrency (D-3, FR-009) are enforced in the store transaction, not only in the use case.<br>**Unverified.** The hosted smoke check and the daily refresh are labelled unverified, each with a quickstart step.<br>**Gate.** The pre-commit gate stays on. |
| VI. Demonstrable Vertical Slices | met | **Slice.** Domain rules, use cases, storage, pages, tests and demo data ship together.<br>**`demo6`.** It shows four Excused commitments (two with a reason, one still removable), a past and a current pause, a paused goal in the Paused tab, and figures that leave them out, on every weekday (FR-031, SC-006).<br>**Infrastructure.** Nothing beyond one startup task. The daily refresh re-creates it. |
| Decision Discipline | met | **Recorded decisions.** SPEC §11.2 is decided as D-1 and SPEC §11.3 as D-2, together with D-3, the early resume and the removal window (ADR 0024). D-4 is recorded in ADR 0023.<br>**This plan.** Its choices (R-01 to R-12) are local to this slice and reversible. None spans features or rejects a product alternative, so no new ADR. |

## Project Structure

### Documentation (this feature)

```text
specs/008-excusals-and-pauses/
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
│   ├── Exemptions/                     # ExemptionReason, ExemptionLimits, Excusal, PauseId, Pause, PauseTiming,
│   │                                   #   GoalExemptions, PauseRules, PauseProblem, PauseResumption
│   ├── Evaluation/EvaluationState.cs   # + Excused, Paused
│   ├── Evaluation/Evaluator.cs         # + Evaluate overload with an exemption
│   └── Reports/                        # Consistency, Streaks skip both; TrendMark; TrendBar.Mark; TrendBuilder overload
├── HT.Application/
│   ├── Exemptions/                     # ExcusalService, PauseService, results, outcomes, PauseView, ExcusalTexts,
│   │                                   #   PauseTexts, ExemptionStoreWrite
│   ├── Records/                        # IRecordStore + 6 members; RecordStoreWriteOutcome.Exempt; EntryViews.Of with
│   │                                   #   exemptions; HistoryEntry + ExcusalReason, CanRemoveExcusal; RecordingService
│   │                                   #   checks 4a–4c and exemption reads; RecordWriteOutcome + 3 members
│   ├── Reports/                        # GoalEvaluation reads exemptions; OverviewGoal, DashboardGoal + PausedUntil;
│   │                                   #   counts; ReportText + texts
│   ├── Operator/                       # DemoPauseService, DemoPauses, DemoExcusal, DemoGoalPause, DemoPauseGoal,
│   │                                   #   IDemoPauseStore, DemoPauseLoadingResult, DemoPauseAccountResult
│   └── ApplicationServiceCollectionExtensions.cs   # + ExcusalService, PauseService, DemoPauseService
├── HT.Persistence/
│   ├── Records/                        # ExemptionRows, ExemptionMapping; RecordStore + 6 members and the AddAsync re-check
│   ├── Habits/DemoAccountWriter.cs     # + excusal and pause rows
│   ├── Habits/DemoPauseStore.cs        # IDemoPauseStore over DemoAccountWriter
│   ├── HtDbContext.cs                  # + GoalExcusals, GoalPauses
│   ├── Migrations/                     # + <timestamp>_ExcusalsAndPauses
│   └── PersistenceServiceCollectionExtensions.cs   # + IDemoPauseStore
└── HT.Ui/
    ├── Operator/StartupTasks.cs        # + Demo pause loading step and its lines
    └── Components/
        ├── Exemptions/                 # PauseDialog, ExcuseDialog, PausesCard
        ├── Shared/                     # StatusChip, ConsistencyStrip, TrendChart: Excused and Paused;
        │                               #   CommitmentRow ("Excuse"); GoalItem ("Paused until", "Resume")
        ├── Records/                    # GoalDetailBody (Pause, Notice, Pauses card), HistoryCard (rows),
        │                               #   RecordDialog ("Excuse instead")
        ├── Reports/                    # GoalsList (tabs), TodayPanel ("Excuse")
        └── Pages/                      # Goals (?tab=paused), Home (active goals card)

tools/HT.SmokeCheck/                    # check 8 demo pause (demo6); SmokeCheckCoverage + 4 entries;
                                        #   PageReader.ReadGoalMetas

tests/
├── HT.UnitTests/                       # Domain/Exemptions, Domain/Reports, Domain/Evaluation, Application/Exemptions,
│                                       #   Application/Records, Application/Reports, Application/Operator;
│                                       #   Fakes/ (FakeRecordStore + exemptions, FakeDemoPauseStore)
├── HT.ArchitectureTests/               # R-11 rules
├── HT.IntegrationTests/                # Records/ (exemption store), Operator/ (demo pause), migration, log capture
├── HT.UiTests/                         # components, pages, startup and smoke check tests
└── HT.AcceptanceTests/                 # outside the solution; written by the AC workflow

design/                                 # changed first (Work order step 1)
specs/003-demo-hosting/contracts/smoke-check.md   # coverage table + 4 rows, check 8
docs/operations/hosted-demo.md, docs/roadmap.md, README.md   # demo6, status
```

**Structure Decision**: the four production projects stay, and no project is added. Excusals and pauses get their
own namespace in Domain and in Application, so slice 9 can reuse the Domain rules without these use cases. They are
stored through `IRecordStore`, beside records and notes, under the same goal-row lock (R-04). Demo pause loading
sits with the other operator capabilities. Its port is public for the AC reference export, fenced by architecture
rules (ADR 0010, ADR 0013), and implemented internally in `HT.Persistence`.

## Work order

`/speckit-tasks` should follow this order, because later steps need earlier ones:

1. **Design first** (constitution IV, ADR 0008). Make the changes in the spec's "Design changes before build" list:
   - `PageGoalDetail`, `PauseDialog`, the Excuse content of `Dialog` (or an `ExcuseDialog` guide), `RecordDialog`;
   - `PageGoals`, `GoalListItem`;
   - `PageHomeSignedIn`, `CommitmentRow`;
   - `StatusChip`, `ConsistencyStrip`, `GoalStats`, `TrendChart` (the Excused bar class in `design/css`);
   - `design/pages.md` and `design/README.md`.

   Then copy any changed `design/css` file to `src/HT.Ui/wwwroot/css`.
2. **Domain**:
   - `HT.Domain.Exemptions`;
   - the `EvaluationState` members and the `Evaluator` overload;
   - `Consistency`, `Streaks` and the trend changes;
   - the boundary unit tests (R-11);
   - the Domain architecture rules for the new namespace.

   `HT.UiTests` and `HT.Application` code that switches on `EvaluationState` gets arms for the two new members in
   the same commit, so the build stays green.
3. **Application** and 4. **Persistence**, verified and committed **together**. After step 3 alone,
   `AddHtApplication` registers `DemoPauseService`, whose port has no implementation. That fails `ValidateOnBuild`
   in every integration and host test, as in slices 2, 4, 5 and 6. `IRecordStore`'s new members likewise need
   `RecordStore` to compile.
   - Step 3:
     - `ExcusalService`, `PauseService`, their results, `ExcusalTexts`, `PauseTexts`;
     - `EntryViews.Of` with exemptions;
     - `RecordingService`'s exemption reads and steps 4a to 4c;
     - `GoalEvaluation`, `ReportService` and `ReportText` changes;
     - `DemoPauses`, `DemoPauseService` and `IDemoPauseStore`;
     - `FakeRecordStore`'s new members, `FakeDemoPauseStore` and the unit tests;
     - the Application rules, in the same commit as the services: the `IGoalStore` rule
       (`OnlyTheGoalTypes…MayDependOnTheGoalStorePort`) widened to `ExcusalService` and `PauseService`, which
       otherwise fails the pre-commit gate; the new `IDemoPauseStore` fence; `UseCaseParameterTests`. Application has
       no `IRecordStore` rule (only HT.Ui and HT.Persistence have one), so none is widened.

     The bUnit page services (`tests/HT.UiTests/Components/Pages/PageServices.cs`) register `ExcusalService`,
     `PauseService` and `FakeDemoPauseStore` in the same commit.
   - Step 4:
     - the rows, the mapping and the `ExcusalsAndPauses` migration;
     - `RecordStore`'s six members and the `AddAsync` re-check;
     - `DemoAccountWriter`'s excusal and pause rows, `DemoPauseStore` and its registration;
     - the integration tests: scoping, re-checks under the lock, demo pause loading, log capture, the re-run SC-007
       timing.
5. **Host**:
   - the "Demo pause loading" startup step and its lines (HP-010);
   - `StartupLogTests` and `StartupTasksDemoTests` updated to expect `demo6@example.com` after `demo5@example.com`,
     with the new fault test.

   The smoke check waits for step 7.
6. **Pages**:
   - StatusChip, ConsistencyStrip and TrendChart;
   - PauseDialog, ExcuseDialog and PausesCard;
   - the goal page (`GoalDetailBody`, `HistoryCard`, `RecordDialog`);
   - the Goals page with its tabs and `GoalItem`'s Paused form;
   - the Home's `TodayPanel`, `CommitmentRow`'s "Excuse" and the Active goals card.

   Each comes with bUnit and host tests. The tests the spec's "Earlier requirements and tests this slice changes"
   names are updated in the step that changes their page. That includes `RecordDialogTests`' `US1_AS9_…_and_no_Excuse_instead`
   and the component tests that assert the absence of Excused and Paused.
7. **Smoke check** (HP-011), after the pages:
   - `PageReader.ReadGoalMetas` with `PageReaderTests` cases;
   - check 8 and the coverage entries;
   - in the same change:
     - `RunnerBehaviourTests.GoodSite()`;
     - `SmokeCheckHostTests`' exact lines and its "(N of N)";
     - `CoverageTests`;
     - the coverage table and check 8 in `specs/003-demo-hosting/contracts/smoke-check.md`.
8. **Docs**:
   - the roadmap status and the README status line;
   - `docs/operations/hosted-demo.md` (`demo6`, check 8);
   - `CONTEXT.md`, only if implementation sharpens a term (HP-012 is already done).
9. **AC tests**: `/speckit-ac-tests-write` and `/speckit-ac-tests-analyze`, which the owner starts after
   `/speckit-implement`.

## Gates before the next step

No gate is open:

- The spec's decisions are all recorded.
- `sources` and `features` are locked (2026-11-21).
- The features critic approved the translation with 4 MINOR notes. Notes 1 and 2 are covered by unit tests (R-07,
  R-11). Note 3 is spec discrepancy 1 below. Note 4 needs nothing from production code, because the plan keeps
  slice 6's trend and dashboard rules unchanged (R-06).
- No roadmap decision is due before this slice's `/speckit-tasks`.

## Spec discrepancies

1. **Long pauses: "N/A" against FR-013** (Edge Cases "Long pauses"; critic-features MINOR-3, critic-ac MINOR-6).
   - The edge case says a goal paused for months "shows 'N/A' … when it resumes". FR-013 counts every counted result
     of the attempt, before the pause included. So a goal with final results before a long pause shows them, for
     example "100% (5 of 5)" for AC-088's goal, and "N/A" only when nothing was counted before the pause.
   - **The plan's position**: FR-013, the normative requirement, decides the behaviour, and the edge case is read as
     "N/A while no result is counted". No acceptance test checks the "N/A" (AC-088 checks only the streaks, as
     acceptance.md says).
   - **For the owner**: the edge case's wording should be corrected in `spec.md`. That needs the owner, because
     `sources` is locked on this spec's hash and a spec edit triggers the AC tooling's spec-drift check (it asks
     whether the acceptance criteria are still valid).
2. **FR-005's step (4) and "not begun"**. FR-005 places steps 4a to 4c "after its step (4) (the date)". The plan reads
   slice 5's step (4) as both date checks, "Nothing is scheduled on this date." and "You cannot record a day that has
   not begun.", so 4a to 4c come after "not begun", as slice 5 FR-005 lists both date refusals in its step (4). No
   locked acceptance criterion checks this order (AC-022 has rows 01 and 02 only, both on the "after the date"
   side), so unit tests pin it (R-11).
3. **Slice 6 rules cited but not restated** (critic-features MINOR-4). The trend's begun bars and current-week
   target, and the dashboard's Missed-this-week window, are implemented as slice 6 has them. The plan changes them
   only as FR-016 and FR-019 say (R-06, R-07).
4. **HP-010, "the deployment's success check also requires its result"**. As in slice 6, ADR 0015's success check is
   the smoke check. Check 8 needs `demo6`'s goals, so a deployment succeeds only when demo pause loading created
   them (R-09).

## Risks

- **The locked AC code of F001 to F007 must keep compiling and passing, and it may not be read.**
  - The plan adds two enum members to `EvaluationState` and three to `RecordWriteOutcome`, one to
    `RecordStoreWriteOutcome`, init-only properties, overloads, six members on `IRecordStore`, new types and DI
    registrations.
  - Enum members are compile-safe, because an exhaustive switch expression without a discard arm already fails today
    (R-01).
  - New port members break only an implementer outside the solution. The AC code composes the real stores
    (ADR 0003), so none is expected.
  - `RecentResults.Counts` keeps slice 6's four keys for goals without exemptions (R-06).
  - `/speckit-ac-tests-analyze` runs all six features with acceptance tests: F001, F002, F004, F005, F007 and F008
    (F003 demo hosting and F006 note editing have none).
- **F005's and F007's demo scenarios** reset the database and run their own demo tasks. Demo pause loading is a
  separate call that they never make, so their results never name `demo6` (FR-030). Only the host runs all five
  tasks.
- **Concurrency.** Two tabs could excuse and record the same commitment at once. The store's re-checks under the
  goal-row lock decide which write wins, and the other gets its refusal (R-04). An integration test runs both
  orders.
- **Week exemption by one date** (D-1) means a pause that begins later in the current week pauses that week at once,
  while the goal is not paused. The pages must show it this way: Active tab, no Notice, a Paused current week, and
  "Resume" refused with "This goal is not paused.". A host test covers it (Edge Cases).
- **Clock changes.** Excusal removal ends at the grace end, which is 48 elapsed hours (slice 5 FR-016). Unit tests
  cover it in Kyiv and Berlin, where the end reads 23:00 or 01:00 locally.
- **The hosted demo** gains a sixth account with 2 goals: 42 records, 2 notes and 6 exemption rows (4 excusals, 2 pauses), in one transaction,
  which adds well under a second to the start.

## Complexity Tracking

| Exception | Why needed | Simpler alternative rejected because |
|---|---|---|
| The code enum `EvaluationState` holds `Excused` and `Paused`, which `CONTEXT.md` says are not evaluation states (constitution I) | Every reader gives the state "or Excused or Paused in its place" in one field (FR-015, FR-017, FR-020). The locked AC code of F001 to F007 reads `EvaluationState` and four non-nullable `State` properties, which must keep their types (FR-023). Slices 5 and 6 planned for these two members | A separate `Exemption` enum with a nullable `State` changes the type of four public properties, so the locked AC code would stop compiling. A separate field with the unexempted state left in `State` shows an excused commitment as "Missed" to every existing reader. Renaming the enum breaks the locked AC code. The doc comment of `EvaluationState` names the glossary's distinction (R-01) |
