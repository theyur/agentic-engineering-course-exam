---

description: "Task list for slice 7, excusals and pauses"
---

# Tasks: Excusals and pauses

**Input**: Design documents from `specs/008-excusals-and-pauses/`: [plan.md](plan.md), [spec.md](spec.md),
[research.md](research.md), [data-model.md](data-model.md), [contracts/application-api.md](contracts/application-api.md),
[contracts/host.md](contracts/host.md), [quickstart.md](quickstart.md); [ADR 0024](../../docs/adr/0024-excusal-and-pause-rules.md),
[ADR 0023](../../docs/adr/0023-demo-pause-loading.md), [ADR 0018](../../docs/adr/0018-evaluation-on-read-and-demo-history.md),
[ADR 0020](../../docs/adr/0020-derived-figures-on-read.md).

**Prerequisites**: `acceptance.md` and `tests/HT.AcceptanceTests/Features/F008/` are locked. No task edits them,
the lock files, the run records or anything under `tests/HT.AcceptanceTests`.

**Tests**: required. The constitution (II, III, V) and `CLAUDE.md` make unit, integration, architecture and
selective UI tests part of done, so every story phase has test tasks. xUnit v2 everywhere, hand-written fakes,
`FakeTimeProvider`, no mocking library (ADR 0006). Acceptance step definitions are not tasks here: the owner starts
`/speckit-ac-tests-write` and `/speckit-ac-tests-analyze` after `/speckit-implement` (plan, Work order 9).

**Organization**: Phase 2 holds what several stories share:

- the whole design step (plan, Work order 1);
- the whole of `HT.Domain.Exemptions` and the Domain changes to evaluation and reports, with their boundary tests
  (Work order 2);
- the storage that every story reads: the six `IRecordStore` members, `RecordStore`, the migration and the fake
  (Work order 3 and 4, store part);
- the one shared evaluation path: `EntryViews.Of`, `RecordingService`'s exemption reads and steps 4a to 4c,
  `GoalEvaluation`, `ReportService`'s changed reads, `ReportText`, `ExcusalTexts` and `PauseTexts` (Work order 3,
  shared part);
- `StatusChip`'s and `ConsistencyStrip`'s Excused and Paused, which the goal page, the Goals page and the Home use.

Each story phase then adds its use case, its tests, its components and its page. The plan's rule that steps 3 and
4 are verified and committed together exists so that `ValidateOnBuild` never meets a registered service whose port
has no implementation, and so that `IRecordStore`'s new members never exist without `RecordStore`'s. The tasks keep
that rule at a finer grain: each port lands in the same commit as its implementation (Phase 2 for `IRecordStore`,
US6 for `IDemoPauseStore`), and `ExcusalService` and `PauseService` use only ports that already have one.

The story phases follow the spec's priorities, with one move: US5 (P1, privacy) comes after US4, because its tests
cover all six use cases of US1 and US2 and the reads US3 and US4 change. US6 (P2, demo) comes last, because its
smoke check reads the goal page and the Goals page's Paused tab that US2 to US4 build.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: the task can run in parallel with the other [P] tasks of its group (they touch different files), once
  the tasks it names as dependencies are done.
- **[Story]**: the user story from spec.md (US1 to US6).
- Paths are relative to the repository root.

## Conventions for every task

- Build with `dotnet build HabitTracker.slnx`. `TreatWarningsAsErrors` is on, so a warning is a failure.
- Use the names in [contracts/application-api.md](contracts/application-api.md) and [data-model.md](data-model.md).
  Every use-case method and every port method is `async` and takes a `CancellationToken` last. The Domain functions
  and the pure formatters (`ReportText`, `ExcusalTexts`, `PauseTexts`, `DemoPauses.For`) are synchronous. Services
  use an explicit constructor with private fields, not a primary constructor (an unread primary-constructor
  parameter is CS9113, an error here).
- The design artifacts stay the contract (constitution, Governance: "never by silent divergence"). A task that adds
  a public member, or settles a text or layout the design artifacts leave open, records it in the same task, in the
  document it names.
- Take message texts from the constants: `ExcusalTexts` and `PauseTexts` (new, T032), and the existing `GoalTexts`,
  `RecordTexts`, `AccountTexts`, `OperatorTexts`. Never retype one. Tests assert the literal text from spec.md, so a
  typo in a constant fails a test. "Nothing is scheduled on this date." stays in `RecordTexts`.
- Every state word, "Paused until …", strip, trend, footnote and empty-tab text comes from `ReportText`,
  `ExcusalTexts` or `PauseTexts` (HP-008). No component builds a text or computes a figure. Date texts come only
  from `DateTexts`; numbers use the invariant culture.
- Time and culture stay out of `HT.Domain` and `HT.Application` (constitution II; `ClockReadTests`):
  - time enters only as `TimeProvider` in the services, read once per call as `_clock.GetUtcNow()`, and as an
    instant or date parameter in Domain;
  - **the pause date** (research R-03) is the goal's current date at that instant, or, for a goal that has not
    started, the account's today in its stored zone, else the installation default. Every "today" of FR-006,
    FR-010, FR-011, FR-012, FR-018 and FR-019 is that date, computed in one place per service;
  - never add days in local time. An excusal and a pause are dates on the goal's own timeline (ADR 0016).
- No NodaTime type appears in any public or protected signature of `HT.Domain` or `HT.Application` (ADR 0017).
- Excusals and pauses are private (constitution III, FR-021, FR-022):
  - they are read and written only through the owner-scoped `IRecordStore` members, always passing the acting
    user's `AccountId` as the owner and the goal named in the call. A pause is found only among that goal's pauses;
    an excusal only by that goal and a date. There is no lookup by `PauseId` alone;
  - the checks run in the order: no account id, role, stored account, goal. Administrators get nothing of any
    account;
  - no reason, note, recorded value, unit, goal name, habit name or figure goes into a log template, an exception
    message or an error page (HP-009). `ExcusalService`, `PauseService` and the new store members log nothing.
    `ExemptionReason` does not override `ToString`. A refusal carries no input value.
- Nothing derived is stored or cached (FR-025, ADR 0018, ADR 0020). Excused and Paused are computed on read by
  `GoalExemptions.ExemptionOf`, applied in the one `EntryViews.Of`. The tables hold dates and reasons only.
- **Locked AC code** (FR-023, plan "Risks"): no existing constructor, method signature, positional record or port
  member of `HT.Domain`, `HT.Application` or `HT.Persistence` changes. Only these are added to existing types: the
  enum members of `EvaluationState`, `RecordWriteOutcome` and `RecordStoreWriteOutcome`; init-only properties
  (`TrendBar.Mark`, `HistoryEntry.ExcusalReason`, `HistoryEntry.CanRemoveExcusal`, `OverviewGoal.PausedUntil`,
  `DashboardGoal.PausedUntil`); the overloads `Evaluator.Evaluate` and `TrendBuilder.Of`; the six `IRecordStore`
  members; the static member `RecordStoreWrite.Exempt`, which `Exempt` needs because `RecordStoreWrite` has a private
  constructor (T028); `ReportText` members; the "Excused" and "Paused" arms of `RecordingText.State` (T016); DI
  registrations. New types of the plan's source tree are not changes to existing types; a new public type that
  data-model.md does not list (for example `DemoPauseLoadingOutcome`, T085) is recorded there by the task that
  creates it. If a task seems to need more, stop and ask the owner. The
  locked F001, F002, F004, F005 and F007 AC code must keep compiling, and nobody reads it. The smoke check tool
  (`tools/HT.SmokeCheck`) is outside that rule.
- A new required member of an existing view would break its construction in the locked AC code. So the new members
  of `HistoryEntry`, `OverviewGoal` and `DashboardGoal` are `init` with a default (`null`, `false`), never
  `required`.
- Test instants are UTC literals worked out by hand from the spec's local times, never computed by the code under
  test. The spec's scenarios use zone UTC unless they say otherwise. For example:
  - US1 ("Yoga, 3 days a week", Monday, Wednesday and Friday): Wednesday 11 November ends at
    `2026-11-12T00:00:00Z`, and its grace period ends at `2026-11-14T00:00:00Z`; so it can be excused and its
    excusal removed at `2026-11-13T23:59:59Z`, and not at `2026-11-14T00:00:00Z` or `2026-11-14T00:00:01Z`
    (US1-AS3, AS8, SC-003). Monday 9 November's grace period ends at `2026-11-12T00:00:00Z`, so on Thursday 12
    November it is Missed (US1-AS4);
  - Europe/Kyiv, change back: Friday 23 October 2026 ends at `2026-10-23T21:00:00Z`, and its grace period ends at
    `2026-10-25T21:00:00Z` (Sunday 23:00 Kyiv time), so its excusal can be removed at `2026-10-25T20:59:59Z` and
    not at `2026-10-25T21:00:00Z`;
  - Europe/Kyiv, change forward: Friday 26 March 2027 ends at `2027-03-26T22:00:00Z`, and its grace period ends at
    `2027-03-28T22:00:00Z` (Monday 01:00 Kyiv time);
  - Europe/Berlin has the same weekends one hour later in UTC (Friday 23 October 2026 ends at
    `2026-10-23T22:00:00Z`).
- Goals with past results are created at an earlier supplied time, then recorded, excused, paused and read at later
  ones, by moving a `FakeTimeProvider` forward. A goal's start date is never in the past (slice 4), and a pause never
  starts before the pause date (FR-006), so a past pause in a test is set while it was current or upcoming and then
  read later. Only demo pause loading writes past facts directly (FR-031).
  - In unit tests each test owns its world and its clock.
  - In integration tests, never set or advance the fixture's shared `Clock` (`DatabaseFixture.Clock`,
    `SeedDatabaseFixture.Clock`). A test that needs another time builds its own provider with its own
    `FakeTimeProvider` over the fixture's `ConnectionString`, as `Records/RecordReadTests.BuildServices` does. On
    `HT-IT-SEED`, such a test first resets the database with `HtDatabase.ResetAsync`, as `ResetAndBuildAsync` does.
- Pages use only `design/` tokens and `ht-` classes, and follow the structure and wording of their `design/`
  preview after the design tasks of Phase 2 (constitution IV). Parts of slices 8 and 9 are absent, not disabled
  (ADR 0008): no "Ended" tab, no Edit settings, no End goal, no earlier attempts.
- Interactive components call use cases only through `UseCaseScope` (slice 2 research R-15). An island reads its
  own data in `OnInitializedAsync`, and reads everything again after any write, so the Notice, the cards and the
  figures agree (contracts/host.md "Goal page"). A static page never passes a view into an island (slice 6
  research R-12); it may pass a query value such as the selected tab.
- Every dialog of this slice uses `HtDialog`: the first field gets focus on open, Esc and × close, and focus returns
  to the opener, or, when a re-read removed the opener, to the nearest remaining control (HP-007). After a re-read
  the island finds the child component again by its key and calls its `FocusAsync`, as slice 6's `GoalsList` and
  `TodayPanel` do. bUnit tests check it with `JSInterop.VerifyFocusAsyncInvoke`.
- Never read, print or embed a secret. That includes the `Operator:*` User Secrets values.
- No `InternalsVisibleTo` in `HT.Domain`, `HT.Application` or `HT.Persistence` (ADR 0010; `AcSurfaceTests`).
  `EntryViews`, `GoalEvaluation` and `DemoAccountWriter` are internal and are tested through the public use cases
  and stores.
- Databases:
  - integration tests use `HT-IT` (`DatabaseFixture`), `HT-IT-SEED` (`SeedDatabaseFixture`) and the migration
    fixture (`MigrationDatabaseFixture`);
  - host tests use `HT-UI` and its derived databases;
  - no test touches `HT` or `HT-AC`.

  Tests in shared databases use account emails, habit names and goal names with their own prefix. They check by
  inclusion, exclusion and relative order, never by totals.
- Commit only at a checkpoint, with the full suite green (constitution V). These land in the same commit, so
  `ValidateOnBuild` and the compiler never meet an unregistered service or an unimplemented port member:
  - the six `IRecordStore` members, `FakeRecordStore`'s, `RecordStore`'s and those of the other implementer in the
    tests, `FaultingRecordStore` in `tests/HT.IntegrationTests/Records/RecordLogTests.cs` (Phase 2, T028 to T031);
  - `ExcusalService`, its registration, its bUnit registration and the widened `IGoalStore` rule (US1, T044);
  - `PauseService`, the same four (US2, T055);
  - `DemoPauseService`, `IDemoPauseStore`, `FakeDemoPauseStore`, `DemoAccountWriter`'s rows, `DemoPauseStore`, their
    two registrations and the port's fences (US6, T085 to T088).

  The bUnit services in `tests/HT.UiTests/Components/Pages/PageServices.cs` are registered by hand and are not built
  with `ValidateOnBuild`. They hold what the pages resolve: `ExcusalService` and `PauseService` join them, but no
  operator service or demo store does, as in slice 6. Plan Work order step 3 also names `FakeDemoPauseStore` there;
  it lives in `HT.UnitTests`, no page resolves it, and slice 6 kept every demo store out, so it is not added.
- A search for callers or tests of a type excludes the AC project:
  `grep -rnw "<Type>" tests --include=*.cs --exclude-dir=HT.AcceptanceTests`. The `--exclude-dir` is required:
  without it the search enters the AC project's off-limits folders (`CLAUDE.md`).
- The pre-commit architecture gate stays on. Never use `--no-verify`.

---

## Phase 1: Setup (shared infrastructure)

**Purpose**: the roadmap state and a baseline of the public API.

- [X] T001 Set slice 7's Status to `implementing` and `updated_at` to today in `docs/roadmap.md`. The row still reads `clarified`; the statuses `acceptance locked`, `features locked`, `planned` and `tasks ready` were not recorded on this branch, against "Updating this file" ("after each step"). The branch's history records those steps, and this change moves the row to its current state in one edit
- [X] T002 [P] Save the public API baseline of `HabitTracker.slnx` before any code change (plan "Risks", FR-023):
  - run `dotnet restore HabitTracker.slnx`;
  - write `roslynator list-symbols HabitTracker.slnx --projects HT.Application --visibility public --depth member`, and the same for `HT.Domain` and `HT.Persistence`, into three files in the session scratchpad, never in the repo. T097 compares them.

  If T097 finds the files missing (a reboot or a later session), it regenerates them from the branch's merge base with `master` (`git merge-base HEAD master`) in a temporary `git worktree` under the scratchpad, and removes that worktree afterwards.

**Checkpoint**: `dotnet build HabitTracker.slnx` passes.

---

## Phase 2: Foundational (blocking prerequisites)

**Purpose**: everything several stories need: the design changes; `HT.Domain.Exemptions` and the Domain changes with
their boundary tests; the storage; the shared evaluation path; `StatusChip` and `ConsistencyStrip`.

**⚠️ CRITICAL**: no story work starts until this phase is complete.

### Design first (constitution IV, ADR 0008; spec "Design changes before build")

Two kinds of edit, as in slices 1, 2, 4, 5 and 6:

- Texts that disagree with the spec or `CONTEXT.md` are **replaced** everywhere (for example a "Provisional" mark on
  "Excuse instead", an Excuse rule that admits Upcoming commitments, against D-2).
- A **slice-7 state** is **added** beside the full preview, as a "Slice 7 state" block after the slice-6 one. The full
  previews stay whole, because they are the design source of slices 8 to 11. "No …" in a task below describes the
  slice-7 state; it never deletes anything from the full preview.

- [X] T003 [P] Add the slice-7 state to `design/pages/PageGoalDetail.html` and `.md` (HP-001, contracts/host.md "Goal page"): the secondary "Pause" (`aria-haspopup="dialog"`) before "Record" in the head, absent while paused; the Notice "Paused until Sun 22 Nov · Resume now" under the head, "Resume now" a button; the "Pauses" card after Settings in the right column (each pause as "Mon 16 Nov – Sun 22 Nov", its reason in `ht-muted`, "Resume now" on the current pause, "Cancel pause" on an upcoming one, no action on a past one), absent with no pause; History rows with the StatusChips "Excused" and "Paused", an Excused row's reason after its chip in `ht-muted` and the ghost "Remove excusal", no "Add record" on either, a record kept on a Paused row with its edit action, both with their note actions; the GoalStats strip and legend Met, Failed, Missed, Excused, Paused, Pending; marked trend bars. Still no Edit settings, End goal or earlier attempts
- [X] T004 [P] Change `design/components/PauseDialog.html` and `.md` (HP-002, HP-007): the title "Pause · {goal name}"; the effect first, "Paused days are not counted. Your streak stays as it is. Final results do not change."; the fields "From" (the page's current date by default, `min` today), "Until" (required) and "Reason (optional)"; the primary "Pause goal" and "Cancel"; one refusal at a time under its field with the result's message as it is: "Choose the first and the last day." under the empty field, "A pause cannot start before today." under "From", "The last day cannot be before the first day." and "This goal already has a pause on these dates." under "Until", "Keep the reason to 200 characters or fewer." under "Reason"
- [X] T005 [P] Create `design/components/ExcuseDialog.html` and `.md` (HP-003, HP-007), a guide of its own because the code has a component of that name (plan, Work order 1, takes the `ExcuseDialog` guide in place of a `Dialog` variant; record the choice in `design/components/Dialog.md` with a link): the title "Excuse · {goal name}"; the sub line "Wed 11 Nov · Pending, 22 h left" (date and state as `RecordingText` gives them); the effect "An excused commitment is not counted. Your streak stays as it is."; "Reason (optional)"; the primary "Excuse commitment" and "Cancel"; a refusal shown in the dialog with the result's message; the two openers (the Home's "Excuse", the RecordDialog's "Excuse instead")
- [X] T006 [P] Change `design/components/RecordDialog.html` and `.md` (HP-003, D-2): "Excuse instead" confirmed (ghost, right) and its "Provisional" mark removed; shown only for a commitment-based goal whose selected "For" entry is Due or Pending with no record; the "For" list holds no Excused or Paused entry
- [X] T007 [P] Add the slice-7 state to `design/pages/PageGoals.html` / `.md` and `design/components/GoalListItem.html` / `.md` (HP-004): the Tabs "Active" and "Paused" (no "Ended"), the selected tab in `?tab=paused`; the Paused item with the meta "Paused until Sun 22 Nov", the Pending StatusChip when it has a Pending result, its figures and mini strip, and the secondary "Resume"; the empty Paused tab "No paused goals."; the footnote "Each goal is counted on its own. Consistency counts only finalized results; pending, excused and paused ones are left out."; a user with no goals keeps slice 4's EmptyState in place of the tabs
- [X] T008 [P] Add the slice-7 state to `design/pages/PageHomeSignedIn.html` / `.md` and `design/components/CommitmentRow.html` / `.md` (HP-005): the ghost "Excuse" on Pending rows of commitment-based goals with no record, next to "Record"; the "Active goals" tile and card counting and listing only goals that are not paused; nothing else changes
- [X] T009 [P] Add the slice-7 state to `design/components/StatusChip.html` / `.md`, `ConsistencyStrip.html` / `.md` and `GoalStats.html` / `.md` (HP-006): the chips "Excused" (`ht-status--excused`) and "Paused" (`ht-status--paused`), each with its word; the cells `ht-cell--excused` and `ht-cell--paused` with their shapes, so no state is shown by colour alone; the strip's `aria-label` "Last 17 commitments: 10 met, 1 missed, 1 excused, 3 paused, 2 pending" (a state with 0 left out, as in slice 6); the legend Met, Failed, Missed, Excused, Paused, Pending, always all six on the full strip (it is a key, not a count). The visitor Home's example card uses the same shared `GoalStats` (slice 6 HP-004), so it shows the same six: add a "Slice 7 state" block to `design/pages/PageHomeVisitor.html` and `.md` after the slice-6 one, whose legend lists the six entries (today the slice-6 preview, near line 130 of the `.html`, shows four), with the strip, figures and label unchanged. Spec "Earlier requirements and tests this slice changes" does not list slice 6 HP-004; the change follows from HP-001 and HP-006 through the shared component, and the `.md` block says so
- [X] T010 [P] Change `design/components/TrendChart.html` and `.md` (FR-016, HP-006, research R-07): the Paused bar (`ht-bar--paused`) and a new Excused bar, dashed like it, with a class added to `design/css/ht-components.css` from tokens only; each marked bar's `title` says "Excused" or "Paused" in place of a value; the chart's `aria-label` counts marked bars apart from bars with no record; the target line is drawn at the latest unmarked bar's target
- [X] T011 Update `design/pages.md` and `design/README.md` with the slice-7 structure of the goal page, the Goals page, the signed-in Home and the visitor Home's legend, and the two new guides (after T003 to T010). Copy every changed file of `design/css/` byte for byte to `src/HT.Ui/wwwroot/css/`. Run `DesignSyncTests` (`tests/HT.UiTests/DesignSyncTests.cs`)

### Domain: exemptions (`src/HT.Domain/Exemptions/`, namespace `HT.Domain.Exemptions`; research R-02)

- [X] T012 [P] Create `src/HT.Domain/Exemptions/ExemptionLimits.cs` (`MaxReasonLength` = 200) and `ExemptionReason.cs` (data-model.md): a sealed class with `Value` and `static bool TryCreate(string? text, out ExemptionReason? reason, out bool tooLong)`: null, empty or blank gives `true` and no reason; the trimmed text of 1 to 200 UTF-16 code units gives `true` and the reason; more than 200 after trimming gives `false` and `tooLong`. Ordinal equality. `ToString` not overridden (HP-009). Follow `src/HT.Domain/Records/NoteText.cs`
- [X] T013 Create `src/HT.Domain/Exemptions/Excusal.cs` (sealed record `Excusal(DateOnly Date, ExemptionReason? Reason)`), `PauseId.cs` (readonly record struct over `Guid`), `PauseTiming.cs` (`Upcoming`, `Current`, `Past`) and `Pause.cs` (sealed record `Pause(PauseId Id, DateOnly FirstDay, DateOnly LastDay, ExemptionReason? Reason)`, whose constructor throws `ArgumentException` when `LastDay < FirstDay`, with `Covers`, `Overlaps` (touching ranges do not overlap) and `TimingAt(DateOnly today)` as data-model.md gives them) (after T012; FR-001, FR-012)
- [X] T014 Create `src/HT.Domain/Exemptions/GoalExemptions.cs` (after T013; FR-003, FR-007, D-1): a sealed class over the excusals (by date) and pauses (latest `FirstDay` first), with `None`, `IsPaused`, `IsExcused`, `ExcusalOf`, `CurrentPause(today)` and `Overlaps(first, last)`, and `EvaluationState? ExemptionOf(CalendarEntry entry, GoalScheduleKind kind)`: `Paused` when any date from `entry.FirstDate` to `entry.LastDate` is in a pause (a week: any of its seven dates); else `Excused` when `kind` is commitment-based and the entry's date has an excusal; else null. A pause wins over an excusal of the same date, and the excusal is kept. Read `CalendarEntry` and `GoalScheduleKind` in `src/HT.Domain/Goals/` for the exact member names first
- [X] T015 Create `src/HT.Domain/Exemptions/PauseProblem.cs` (`DaysMissing`, `StartsBeforeToday`, `LastBeforeFirst`, `Overlaps`), `PauseResumption.cs` (sealed record, `bool Removes`, `DateOnly? NewLastDay`) and `PauseRules.cs` (after T014; FR-006, FR-010): `static PauseProblem? Check(DateOnly? first, DateOnly? last, DateOnly today, GoalExemptions exemptions)` gives FR-006 steps 4 to 7 in that order, or null; `static PauseResumption Resume(Pause current, DateOnly today)` gives `Removes` when `current.FirstDay == today`, else `NewLastDay = today − 1`, and throws `ArgumentException` when `current.TimingAt(today)` is not `Current`

### Domain: evaluation and reports (research R-01, R-02, R-07)

- [X] T016 Add `Excused` and `Paused` after `Missed` to `src/HT.Domain/Evaluation/EvaluationState.cs`, with the doc comment of data-model.md: the evaluation state of an entry, or the exemption in its place; only `Upcoming` to `Missed` are evaluation states in `CONTEXT.md`'s sense (Complexity Tracking). In the same commit, review every switch and comparison over `EvaluationState` in `src/` and `tools/`, also those whose discard arm throws: list them first with `grep -rn "EvaluationState\." src tools --include=*.cs --include=*.razor`; the grep's output is the list (among them `RecordingText`, `EntryViews`, `RecordingService`, `ExampleGoal`, `GoalEvaluation`, `ReportService`, `ReportText`, `Evaluator`, `Consistency`, `Streaks`, `HistoryCard`, `GoalItem`, `TodayPanel`, `CommitmentRow`, `ConsistencyStrip`, `StatusChip`, `SmokeCheckRunner`):
  - `RecordingText.State` in `src/HT.Application/Records/RecordingText.cs`, the one source of state words (`ReportText.State`, `StatusChip` through it, and `NoteDialog`'s sub line in `src/HT.Ui/Components/Records/NoteDialog.razor` call it), gets the final arms "Excused" and "Paused"; Upcoming still throws there. Add the two words to `tests/HT.UnitTests/Application/Records/RecordingTextTests.cs`; its Upcoming throw case stays;
  - a switch without a discard arm gets arms so the build stays green: one that gives a word gives "Excused" or "Paused", one that decides counting treats both as skipped, like Pending;
  - a switch whose discard arm throws (`StatusChip.Modifier`, `ConsistencyStrip`'s cell class) stays for T042, which gives it its final arms.

  Every existing test passes unchanged
- [X] T017 Add the overload `Evaluate(target, starts, ends, records, now, EvaluationState? exemption)` to `src/HT.Domain/Evaluation/Evaluator.cs` (after T016; R-02, data-model.md): with `Excused` or `Paused`, `State` is the exemption, `Progress` comes from the records as before (FR-015, FR-017), `GraceEnds` is computed as before and `IsFinal` is false; any other non-null value throws `ArgumentOutOfRangeException`. The existing overload calls it with null, and its results do not change
- [X] T018 [P] Change `src/HT.Domain/Reports/Consistency.cs` and `Streaks.cs` (after T016; FR-013, FR-014): `Excused` and `Paused` are skipped like Pending; they neither count, nor extend, nor reset
- [X] T019 Create `src/HT.Domain/Reports/TrendMark.cs` (`Excused`, `Paused`); add `TrendMark? Mark { get; init; }` to `TrendBar.cs` (init-only, not a positional parameter); add the overload of `TrendBuilder.Of` whose begun items carry an `EvaluationState? Exemption` (after T016; FR-016, research R-07). The old overload calls it with none and gives slice 6's bars unchanged. The rules, exactly as R-07 gives them:
  - per-period goals and commitment-based count and duration goals: a bar is marked with its entry's exemption and has a null value;
  - commitment-based yes/no weeks: the Build target counts the week's commitments, the current week's Upcoming ones included, that are neither Excused nor Paused; the value (Build and Reduce) sums the "Yes" records of those commitments only, and is null when none of them has a record; the Reduce target stays 0;
  - a week is all exempt when every one of its commitments, the current week's Upcoming ones included, is Excused or Paused; it is marked `Paused` when one of them is Paused, else `Excused`, and its target is 0;
  - a marked bar's `Target` follows the same rules as an unmarked one;
  - `Trend.Target` is the latest unmarked bar's target, or 0 when every bar is marked or there is none. Change `Trend.cs` only if it computes the target itself; otherwise set it in the builder

  The Upcoming commitments of the current week must carry their exemption too: pass them with it, or pass the goal's `GoalExemptions` to the overload. Record the overload's final signature in data-model.md

### Domain tests (constitution II, SC-002, SC-003; research R-11)

- [X] T020 [P] Write `tests/HT.UnitTests/Domain/Exemptions/ExemptionReasonTests.cs` (after T012; FR-001): null, empty and blank give no reason; "  Ill  " gives "Ill"; 200 characters accepted, 201 refused with `tooLong`, 201 with surrounding spaces that trim to 200 accepted; a surrogate pair counts as two units; `ToString` does not return the text
- [X] T021 [P] Write `tests/HT.UnitTests/Domain/Exemptions/PauseTests.cs` (after T013; FR-012): `TimingAt` at the first day − 1 (`Upcoming`), the first day and the last day (`Current`), the last day + 1 (`Past`); a one-day pause; `Covers` at both ends and outside; `Overlaps` by one day at each end; touching ranges (Sunday and Monday) do not overlap; `LastDay < FirstDay` throws
- [X] T022 [P] Write `tests/HT.UnitTests/Domain/Exemptions/GoalExemptionsTests.cs` (after T014; FR-003, FR-007, D-1): a commitment and a day in a pause are Paused, the dates before and after are not; a week with one paused date on its Monday, on its Sunday and in the middle is Paused, the weeks around it are not; an excused commitment is Excused; an excused commitment inside a pause is Paused and `ExcusalOf` still finds the excusal; an excusal never exempts a per-day or per-week entry; `CurrentPause` with an upcoming, a current and a past pause; `None` exempts nothing
- [X] T023 [P] Write `tests/HT.UnitTests/Domain/Exemptions/PauseRulesTests.cs` (after T015; FR-006, FR-010, SC-003): a missing first day, a missing last day and both missing give `DaysMissing`; the first day at today − 1 gives `StartsBeforeToday`, today and today + 1 pass; the last day one day before the first gives `LastBeforeFirst`; an overlap by one day gives `Overlaps`, a touching range passes; the order of two problems at once (a first day before today and a last day before it gives `StartsBeforeToday`; a reversed range that also overlaps gives `LastBeforeFirst`); `Resume` on the first day removes, on the second day ends on the first day, on the last day ends on the day before; `Resume` of an upcoming or past pause throws
- [X] T024 [P] Write `tests/HT.UnitTests/Domain/Evaluation/EvaluatorExemptionTests.cs` (after T017; FR-003, FR-009): with `Excused` and with `Paused`, a commitment, a day and a week give that state at its start, before its end, after its grace end and long after, never Pending, Met, Failed or Missed; the progress of a kept record is the same as without the exemption; `IsFinal` is false; `Due` as the exemption throws; null gives exactly the 5-argument result for every case of `EvaluatorCases.cs`
- [X] T025 [P] Add to `tests/HT.UnitTests/Domain/Reports/ConsistencyTests.cs` and `StreaksTests.cs` (after T018; FR-013, FR-014, SC-002): US3's sequence (Met × 7, Excused, Met, Missed, Met, Paused × 3, Met, Pending × 2, Due) gives 10 of 11 (90), current 2 and longest 8; every case gives the same figures as the same sequence with its Excused and Paused states removed (a theory over several sequences); only exempt states give a null percentage and 0 and 0; an Excused between a Failed and a Met does not join runs across the Failed
- [X] T026 [P] Write `tests/HT.UnitTests/Domain/Reports/TrendBuilderExemptionTests.cs` (after T019; FR-016, critic-features MINOR-1 and MINOR-2): US3-AS4's daily duration goal (14 bars, 9 November marked Excused, 13 to 15 November marked Paused, no value on marked bars or on the Missed and unrecorded ones, 10 on the others, target 10); a per-week goal with a Paused week; a commitment-based yes/no Build week with one Excused and one Paused commitment of three (target 1, value from the third only); a "Yes" kept on a Paused commitment not counted; a week whose only records sit on Paused commitments has no value, not 0; a Reduce yes/no week with a "Yes" on an Excused commitment and a "Yes" on a counted one gives 1 against 0; an all-exempt week marked `Paused` with a Paused commitment, `Excused` with none, target 0; a current week with one Excused commitment and one Upcoming commitment that is not Paused is not all exempt and keeps an unmarked bar; a latest all-exempt week leaves `Trend.Target` at the previous unmarked bar's target; every bar marked gives 0; the old overload's bars equal the new one's with no exemption (every case of `TrendBuilderTests.cs`)
- [X] T027 Add the Domain rules (after T012 to T019; research R-11): in `tests/HT.ArchitectureTests/DomainDependencyTests.cs`, `HT.Domain.Exemptions` joins the namespaces that depend only on the Domain and the BCL; in `PublicSurfaceTests.cs`, the exemption types join the rows that check for no NodaTime type in their signatures. `ClockReadTests` needs no change; run it

**Checkpoint (Domain)**: the full suite passes, every existing test unchanged. Commit.

### Storage (`HT.Application.Records` port, `HT.Persistence`; research R-04)

- [X] T028 Create `src/HT.Application/Records/ExemptionStoreWrite.cs` (`ExemptionStoreWriteOutcome`: `GoalNotFound`, `AlreadyExcused`, `HasRecord`, `Paused`, `NotExcused`, `Overlaps`, `PauseNotFound`, `Written`; and the `PauseId?` written). Add the six members of contracts/application-api.md "IRecordStore: new members" to `src/HT.Application/Records/IRecordStore.cs`, each with the owner and the goal first, and `Exempt` to `RecordStoreWriteOutcome` with its static member `RecordStoreWrite.Exempt`, like `GoalNotFound`, in `src/HT.Application/Records/RecordStoreWrite.cs` (after T014; FR-021, FR-025). Record `RecordStoreWrite.Exempt` in data-model.md "Application: changed types". In the same commit, add the six members to `FaultingRecordStore` in `tests/HT.IntegrationTests/Records/RecordLogTests.cs`, each delegating to the inner store behind the same fault switch as its other members, so `HT.IntegrationTests` keeps compiling. Lands in one commit with T029 to T031
- [X] T029 Implement the six members in `tests/HT.UnitTests/Fakes/FakeRecordStore.cs` (after T028), with the same re-checks and outcomes as the contract's table ("Re-checked under the lock"), owner scoping (another owner's goal behaves like none; `ReadExemptionsAsync` gives `GoalExemptions.None`), the `Exempt` refusal in its `AddAsync`, and `Seed` helpers for excusals and pauses with any dates, so tests can build past pauses that the use case would refuse. Add a hook that runs once inside the next write, before its re-check (for example `Action? BeforeNextWrite`), so a unit test can simulate a concurrent excusal, pause or record between a use case's checks and its write (T038, T043, T054). Every new member appends its name to `Calls`, like the existing ones. The existing members do not change
- [X] T030 [P] Create `src/HT.Persistence/Records/ExemptionRows.cs` and `ExemptionMapping.cs` for `GoalExcusals` and `GoalPauses` exactly as data-model.md "Tables" gives them (keys, `nvarchar(200)` reasons, the index `IX_GoalPauses_GoalId_FirstDay`, the cascading FKs from `Goals`); add both sets to `src/HT.Persistence/HtDbContext.cs`. Add the migration with `dotnet ef migrations add ExcusalsAndPauses --project src/HT.Persistence`, as slice 5 added `Records`. Read the generated `Up` and `Down`, check them against data-model.md, and check that `HtDbContextModelSnapshot.cs` changed only by the two tables. Leave the `CA1861` hints in the migration code (`CLAUDE.md`)
- [X] T031 Implement the six members on `src/HT.Persistence/Records/RecordStore.cs` (after T028, T030; R-04, D-3, FR-009): each write in the existing transaction that locks the goal row (`UPDLOCK, HOLDLOCK` where the goal is the owner's), with `GoalNotFound` writing nothing; the re-checks of the contract's table inside the transaction; `AddPauseAsync` makes the `PauseId`; `SetPauseLastDayAsync` and `DeletePauseAsync` find the pause by goal and identifier together; `ReadExemptionsAsync` reads all of the goal's excusals and pauses in one round trip, owner-scoped in the same statement. In `AddAsync`, under its lock, refuse with `Exempt` an entry with an excusal on its key date or a pause on any of its dates (read the entry's first and last date from what `AddAsync` already receives; if it does not receive them, stop and ask the owner rather than change the member's signature). Faults name the SQL error number only, as now
- [X] T032 [P] Create `src/HT.Application/Exemptions/ExcusalTexts.cs` and `PauseTexts.cs` with every text of data-model.md's texts table, word for word, and the success words (HP-008)

### Application: the shared evaluation path (research R-06)

- [X] T033 Change the internal `src/HT.Application/Records/EntryViews.cs` (after T017, T028): `Of` and `HistoryOf` take the goal's `GoalExemptions`, find the entry's exemption with `ExemptionOf` and evaluate through T017's overload; for an exempt entry `GraceEnds` is null and `IsCorrection` false; for an Excused one the new `HistoryEntry.ExcusalReason` (the reason's text) and `CanRemoveExcusal` (`now < GracePeriod.EndOf(entry.Ends)`); records, progress and note as before. Add the two init-only members with defaults to `src/HT.Application/Records/HistoryEntry.cs` (Conventions)
- [X] T034 Change `src/HT.Application/Records/RecordingService.cs` (after T031, T032, T033; FR-005, FR-009, FR-017, FR-020, research R-06):
  - every read and every write result passes the goal's exemptions, read once per call through `ReadExemptionsAsync`, to `EntryViews`: the history, what can be recorded now (which leaves out every Excused or Paused entry), recent notes, the save-note result and the record write results;
  - `RecordAsync` checks, after "You cannot record a day that has not begun." and before the value: 4a a date in a pause (`DatePaused`, "This goal is paused on this date."), 4b a date of a Paused week that is not itself in a pause (`WeekPaused`, "This week is paused."), 4c an excused commitment (`CommitmentExcused`, "This commitment is excused. Remove the excusal to record it."); add the three members to `RecordWriteOutcome` in `src/HT.Application/Records/RecordingResults.cs`;
  - a store `Exempt` reads the exemptions again and gives the matching step;
  - `ChangeRecordAsync`, `DeleteRecordAsync` and `SaveNoteAsync` add no check; their state after the change is Paused on a paused date, never a correction.

  The constructor and every public signature stay as they are. Where the exemption read goes: once per call, after the access and goal checks, and only when the call has an entry or date to evaluate. A read with no entry (a goal before its start, unscheduled days only, a per-week goal's days before its first week) reads nothing, as in slice 5, so `HistoryTests.cs`' and `RecordableTests.cs`' "…no store read" tests keep their empty call lists unchanged. `RecordAsync` reads them right after "not begun", before the value and the note (4a to 4c need them).

  The exact `FakeRecordStore.Calls` assertions of slice 5 change to include `ReadExemptionsAsync` where this task adds it, and nothing else: `tests/HT.UnitTests/Application/Records/HistoryTests.cs` (the two `[ReadAsync]` lists), `RecentNotesTests.cs`, `RecordableTests.cs` (the `[ReadAsync]` list), `StateTests.cs` (the "all `ReadAsync`, 7 calls" pair), `CorrectionTests.cs`, `RecordTests.cs` (the note-too-long refusal's empty list, today near line 336, becomes `[ReadExemptionsAsync]`, since the note is checked after 4a to 4c), `RecordingAccessTests.cs` (if its arranged calls include a read this task adds), and `tests/HT.IntegrationTests/Records/RecordReadTests.cs`. List them first with `grep -rn "Calls" tests/HT.UnitTests tests/HT.IntegrationTests --include=*.cs --exclude-dir=HT.AcceptanceTests`. A test whose access refusal comes before the goal keeps its empty call list, which proves no exemption is read for a refused caller
- [X] T035 Add to `src/HT.Application/Reports/ReportText.cs` (after T016; HP-004, HP-006, HP-008, data-model.md): `State` gives "Excused" and "Paused" through `RecordingText.State` (T016); `PausedUntil(DateOnly)` ("Paused until Sun 22 Nov", through `DateTexts`); `StripLabel` counts excused and paused ("Last 17 commitments: 10 met, 1 missed, 1 excused, 3 paused, 2 pending", a state with 0 left out); `TrendLabel` and a bar title that name marked bars apart from bars with no record; the Goals footnote; "No paused goals.". Record each new member in data-model.md and contracts/host.md if missing
- [X] T036 Change the internal `src/HT.Application/Reports/GoalEvaluation.cs` and `src/HT.Application/Reports/ReportService.cs` (after T019, T034, T035; FR-013 to FR-020, research R-06):
  - `GoalEvaluation` reads the exemptions with the records and evaluates every entry through `EntryViews.Of` with them; `Consistency.Of` and `Streaks.Of` get the exempt states and skip them; `Ended` adds exempt entries whose end instant is at or before `now`; "begun" for the trend is `entry.Starts <= now`; the trend goes through T019's overload, with the exemption of each begun entry and of each of the current week's Upcoming commitments, Paused ones included (`CurrentWeekUpcoming` feeds only the trend, and T019's all-exempt mark needs them: a current week of an Excused begun commitment and Paused upcoming ones is marked `Paused`); `NextUpcoming` alone leaves out Paused entries (FR-019). research.md R-06 still says `CurrentWeekUpcoming` leaves them out too, against R-07 and FR-016 (critic-tasks pass 1 MAJOR-4): correct that bullet of R-06 in this task, citing R-07;
  - `RecentResults.Counts` gets the keys `Excused` and `Paused` only when their count is above 0, so a goal without exemptions keeps slice 6's four keys (FR-023);
  - `OverviewGoal` and `DashboardGoal` (in `src/HT.Application/Reports/GoalsOverview.cs` and `Dashboard.cs`) get the init-only `DateOnly? PausedUntil`: the last day of the current pause at the pause date (Conventions), else null; `DashboardCounts.Goals` counts only goals with no `PausedUntil`;
  - order, groups and windows unchanged; exempt entries are never Due, Pending or Missed, so they fall out of the Today list and the counts.

  The constructor and every public signature stay as they are. The exact `[ReadAsync]` call list in `tests/HT.IntegrationTests/Reports/GoalReportReadTests.cs` gains `ReadExemptionsAsync` (one read per goal more, plan "Performance Goals"); any other exact call list of `tests/HT.UnitTests/Application/Reports/` likewise

### Application and storage tests (shared)

- [X] T037 [P] Create `tests/HT.UnitTests/Application/Exemptions/ExemptionWorld.cs`, building on `tests/HT.UnitTests/Application/Records/RecordWorld.cs` and `Reports/ReportWorld.cs`: goals created at an earlier supplied time, records added through `RecordingService`, excusals and pauses either seeded on `FakeRecordStore` (T029) or, once US1 and US2 exist, made through `ExcusalService` and `PauseService`, the clock moved forward, reads through `RecordingService` and `ReportService` (after T029)
- [X] T038 [P] Write `tests/HT.UnitTests/Application/Records/RecordExemptionTests.cs` with `ExemptionWorld` (after T034, T037; FR-005, FR-009, plan "Spec discrepancies" 2): on a seeded pause, an excusal and a pause over the same date, and a Paused week: 4a, 4b and 4c with their texts and nothing stored; an excused and paused date gives 4a; an invalid value on a paused date gives the pause text (4a before the value); a date not begun gives "You cannot record a day that has not begun." even when paused (4a after "not begun"); a date with nothing scheduled gives `RecordTexts`' text; a store `Exempt` (an excusal or a pause added through T029's hook after the use case's checks) gives the matching text; what can be recorded now leaves out Excused and Paused entries and keeps the others; a record kept on a Paused date is returned with it and can be changed and deleted, with Paused as the state after the change and `IsCorrection` false
- [X] T039 [P] Write `tests/HT.UnitTests/Application/Records/HistoryExemptionTests.cs` with `ExemptionWorld` and add to `RecentNotesTests.cs` (after T034, T037; FR-017, FR-020): an Excused commitment in the history with its reason, no grace end, `IsCorrection` false and `CanRemoveExcusal` true before its grace end and false at it; one without a reason; a Paused day and a Paused week with their records and progress; a note on an Excused and on a Paused entry, in the history and in recent notes with that state; entries without exemptions unchanged
- [X] T040 [P] Add to `tests/HT.UnitTests/Application/Reports/ReportTextTests.cs` (after T035; HP-006, HP-008): "Excused", "Paused"; "Paused until Sun 22 Nov"; the strip label of T009 and one without excused or paused (slice 6's text unchanged); the trend label and bar titles of marked bars; the footnote and "No paused goals." word for word; a culture with another separator gives the same texts (`CultureScope`)
- [X] T041 [P] Write `tests/HT.IntegrationTests/Records/ExemptionStoreTests.cs` and `ExemptionsMigrationTests.cs` (after T031; FR-021, FR-025, research R-04, R-11): on `MigrationDatabaseFixture`, the two tables with their keys, index, nullable reasons and cascading FKs, and no column for a time or a state (follow `RecordsMigrationTests.cs`); on `HT-IT`, with the test's own provider and clock (Conventions): each member's happy path; each re-check of the contract's table (an excusal of an excused date, of a dated record and of a paused date; a removal with no excusal; an overlapping pause; `SetPauseLastDayAsync` and `DeletePauseAsync` with another goal's pause give `PauseNotFound`); `AddAsync` gives `Exempt` on an excused date, a paused date and a week with one paused date; another owner's goal gives `GoalNotFound` and `ReadExemptionsAsync` gives none; deleting an account removes its goals' excusals and pauses, as for records and notes
- [X] T042 Change `src/HT.Ui/Components/Shared/StatusChip.razor` and `ConsistencyStrip.razor` (after T009, T011, T035; HP-006): the chips "Excused" (`ht-status--excused`) and "Paused" (`ht-status--paused`) with their words from `ReportText.State`; the cells `ht-cell--excused` and `ht-cell--paused`; the label from `ReportText.StripLabel`; the legend lists Excused and Paused after Missed. Update `tests/HT.UiTests/Components/Shared/StatusChipTests.cs` and `ConsistencyStripTests.cs`: the two chips and cells, the label with them, the legend's six entries in order; the slice-6 assertions that Excused and Paused are absent (spec "Earlier requirements and tests this slice changes") become assertions of their presence; list them first with `grep -n "Excused\|Paused\|excused\|paused" tests/HT.UiTests/Components/Shared/*.cs`.

  The full strip's legend is on every goal page (through `GoalStats`) and on the visitor Home's example card, so these change in the same task, or the checkpoint is red:
  - `tests/HT.UiTests/Components/Shared/GoalStatsTests.cs` and `tests/HT.UiTests/Components/Pages/HomeVisitorTests.cs`: the exact legend `["Met", "Failed", "Missed", "Pending"]` (today near lines 72 and 116) becomes the six entries;
  - `tests/HT.UiTests/Components/Pages/GoalDetailPageTests.cs`: the absent list `{ "Edit settings", "Pause", "End goal", "Earlier attempts", "Excuse" }` (today near line 116) checks the whole markup by substring, and the legend's "Paused" and "Excused" now contain "Pause" and "Excuse". It keeps "Edit settings", "End goal" and "Earlier attempts"; "Pause" and "Excuse" leave it, and the test instead asserts that no button reads "Pause", "Excuse instead" or "Remove excusal" (the page-head list `["Record"]` already proves there is no "Pause" button until T064).

  Find every legend or substring check that the six entries reach with `grep -rn "ht-legend\|\"Pause\"\|\"Excuse\"" tests/HT.UiTests --include=*.cs`

**Checkpoint (shared)**: the full suite passes. The existing tests of slices 5 and 6 are unchanged apart from three
named kinds of edit: the exact store-call lists of T034 and T036 (one `ReadExemptionsAsync` more), `FaultingRecordStore`'s
new members (T028), and the legend and absence assertions of T042 (shared components, `GoalStatsTests`,
`HomeVisitorTests`, `GoalDetailPageTests`' absent list). Commit (T028 to T031 in this commit, Conventions).

---

## Phase 3: User Story 1 - Excuse a commitment (Priority: P1) 🎯 MVP

**Goal**: a registered user excuses one Due or Pending commitment without a record, with an optional reason, and
removes the excusal while its grace period is open.

**Independent Test**: create "Yoga, 3 days a week" at an earlier supplied time, excuse the Pending Wednesday 11
November commitment on Thursday 12 November at 10:00 with "Ill", and read its history and report on Saturday 14
November at 00:00: it is Excused, not Missed, and not counted.

### Tests for User Story 1

- [X] T043 [P] [US1] Write `tests/HT.UnitTests/Application/Exemptions/ExcuseTests.cs` with `ExemptionWorld` (after T044; FR-001 to FR-003, US1-AS1 to AS7, AS10, AS11, D-2, D-3, SC-003): every step of FR-002 with its text, and the order of each adjacent pair that one input can trigger together (a period-based goal on a date with nothing scheduled gives step 4; a date with nothing scheduled inside a pause gives step 5; a paused and excused date gives step 6; an excused date whose grace period has ended gives step 7; a final commitment with a record gives step 8; a Pending commitment with a record and a 201-character reason gives step 9); AS1 "excused" with the date, the history Excused with "Ill", and what can be recorded now without it; AS2 a Due commitment at 07:00 without a reason; AS3 still Excused at `2026-11-14T00:00:00Z` and a year later; AS4 Missed refused and still Missed; AS5 Upcoming refused; AS6 a period-based goal refused, nothing changes; AS7 Tuesday refused; AS10 a Pending commitment with a "No" record refused and still Pending with its record, then excused after the record is deleted; AS11 200 characters accepted, 201 refused; excusing at grace end − 1 s works, at it and at + 1 s gives "This commitment is final and can no longer be excused."; a reason of spaces stores none; nothing is stored on any refusal; a concurrent write simulated through T029's hook: an excusal of the same date gives "This commitment is already excused.", a record gives "This commitment has a record. Delete the record to excuse it.", a pause over the date gives "This goal is paused on this date." (the store's `AlreadyExcused`, `HasRecord` and `Paused`)
- [X] T045 [P] [US1] Write `tests/HT.UnitTests/Application/Exemptions/RemoveExcusalTests.cs` with `ExemptionWorld` (after T044; FR-003, FR-004, FR-024, US1-AS8, AS9, SC-003): AS8 removal at `2026-11-13T23:59:59Z` gives "excusal removed" and Pending, then the commitment can be recorded; at `2026-11-14T00:00:00Z` and `00:00:01Z` "This excusal can no longer be removed." and still Excused; a Due commitment's removal gives Due; a removal under a seeded pause gives Paused; "This commitment is not excused."; AS9 recording an excused commitment gives "This commitment is excused. Remove the excusal to record it." and stores nothing; the Kyiv and Berlin grace ends of Conventions (23:00 and 01:00 local); a commitment excused and removed, then recorded, gives the same report as one recorded on time (FR-024)
- [X] T046 [P] [US1] Write `tests/HT.IntegrationTests/Records/ExemptionRaceTests.cs` on `HT-IT` (after T044; D-3, FR-009, plan "Risks" Concurrency): with the real stores and the test's own provider and clock, an excusal and a record of the same commitment started together, in both orders, end with exactly one of them stored and the other refused with its text (`HasRecord` → "This commitment has a record. Delete the record to excuse it."; `Exempt` → "This commitment is excused. Remove the excusal to record it."); follow `RecordRaceTests.cs`
- [X] T047 [P] [US1] Write `tests/HT.UiTests/Components/Exemptions/ExcuseDialogTests.cs` (after T048; HP-003, HP-007): the title "Excuse · Yoga, 3 days a week", the sub line "Wed 11 Nov · Pending, 22 h left", the effect, "Reason (optional)", the buttons; focus on the reason field on open; Esc and × close and return focus to the opener; "Excuse commitment" calls `ExcuseAsync` with the trimmed reason and raises the saved callback; a refusal shows its message in the dialog and keeps it open

### Implementation for User Story 1

- [X] T044 [US1] Create `src/HT.Application/Exemptions/ExcusalService.cs` with the constructor of contracts/application-api.md and `ExcuseAsync` and `RemoveExcusalAsync`, and `ExcuseOutcome`, `ExcuseResult`, `RemoveExcusalOutcome`, `RemoveExcusalResult` in `src/HT.Application/Exemptions/` (after T036; FR-001 to FR-004, FR-021, FR-022, research R-05):
  - the checks in the contract's tables and order, the texts from `ExcusalTexts`, `RecordTexts` and the access constants; the time read once; the goal found through `IGoalStore.FindAsync(owner, goal)`; the commitment through `GoalTimeline.EntryOf`; its state through `Evaluator.Evaluate` without an exemption; its records through one `ReadAsync` over that date; the reason through `ExemptionReason.TryCreate`;
  - a store `AlreadyExcused`, `HasRecord` or `Paused` gives step 7's, 9's or 6's refusal; `GoalNotFound` gives "This goal does not exist.";
  - `RemoveExcusalAsync`'s `State` is evaluated after the change with the remaining exemptions.

  In the same commit: register it (scoped) in `src/HT.Application/ApplicationServiceCollectionExtensions.cs`; add it with `TryAddScoped` to `tests/HT.UiTests/Components/Pages/PageServices.cs`; add it to `SignedInServices` in `tests/HT.ArchitectureTests/UseCaseParameterTests.cs`; widen the `IGoalStore` rule in `tests/HT.ArchitectureTests/ApplicationDependencyTests.cs` (`OnlyTheGoalTypes…MayDependOnTheGoalStorePort`) to admit `HT.Application.Exemptions.ExcusalService`, renaming the test and its `Because` to say so (research R-11); `HT.Application.Exemptions` joins the Application layer rules there if they list namespaces
- [X] T048 [P] [US1] Create `src/HT.Ui/Components/Exemptions/ExcuseDialog.razor` on `HtDialog` (after T005, T011, T044; HP-003, HP-007, contracts/host.md "Excuse dialog"): it takes the goal, the date, the goal name, the state text (from `RecordingText`), the opener's `FocusOnClose` and a saved callback; calls `ExcuseAsync` through `UseCaseScope`; shows a refusal in the dialog
- [X] T049 [US1] Change `src/HT.Ui/Components/Records/HistoryCard.razor` (after T042; HP-001, FR-017): an Excused row has its StatusChip, its reason after the chip in `ht-muted`, the ghost "Remove excusal" while `CanRemoveExcusal`, no "Add record", and its note actions; "Remove excusal" calls `RemoveExcusalAsync` through the parent's callback. Add to `tests/HT.UiTests/Components/Records/HistoryCardTests.cs`: the row with and without a reason, "Remove excusal" present and absent, no "Add record", the note action kept. Add to `tests/HT.UiTests/Components/Records/NoteDialogTests.cs`: "Add note" on an Excused row opens the NoteDialog with the sub line "Wed 11 Nov · Excused" (HP-001, Edge Cases "Notes"; `NoteDialog.razor` takes its state word from `RecordingText.State`, T016)
- [X] T050 [US1] Change `src/HT.Ui/Components/Records/RecordDialog.razor` (after T006, T048; HP-003, SC-001): the ghost "Excuse instead" on the right, shown only for a commitment-based goal whose selected "For" entry is Due or Pending with no record; it closes the RecordDialog and opens the ExcuseDialog for that entry. In `tests/HT.UiTests/Components/Records/RecordDialogTests.cs`, change `US1_AS9_…_and_no_Excuse_instead`, which asserts that the dialog has no "Excuse" (spec "Earlier requirements and tests this slice changes"), to assert that the dialog has no "Excuse instead" for a period-based goal and for an entry with a record, and add: shown for a Due and for a Pending commitment with no record; choosing it opens the ExcuseDialog
- [X] T051 [US1] Change `src/HT.Ui/Components/Shared/CommitmentRow.razor` and `src/HT.Ui/Components/Reports/TodayPanel.razor` (after T008, T048; HP-005, US1-AS12, SC-001): the ghost "Excuse" on Pending rows of commitment-based goals with no record; it opens the ExcuseDialog for that row's goal and date; after "excused" the panel reads the dashboard again, the row leaves the list, the "Pending · still can record" tile drops by 1, and focus goes to the nearest remaining row or the tile (Conventions). Existing assertions that change: `CommitmentRowTests.cs`' single-button checks (`FindAll("button").SingleOrDefault()`, today near lines 83 and 141) and its `Parts` tuple for a Pending commitment row with no record, and `TodayPanelTests.cs`' exact row-button list (today near line 135), all of which now include "Excuse" on such rows; find them with `grep -n "SingleOrDefault\|\"Record\", \"Edit\"" tests/HT.UiTests/Components/Shared/CommitmentRowTests.cs tests/HT.UiTests/Components/Reports/TodayPanelTests.cs`. Add to `tests/HT.UiTests/Components/Shared/CommitmentRowTests.cs` and `tests/HT.UiTests/Components/Reports/TodayPanelTests.cs`: "Excuse" on a Pending commitment row, not on a Due row, a period-based row or a row with a record; after excusing, the row and one Pending count are gone and the focus moves
- [X] T052 [US1] Change `src/HT.Ui/Components/Records/GoalDetailBody.razor` (after T049, T050; FR-024, contracts/host.md "Goal page"): it opens the ExcuseDialog from the RecordDialog's "Excuse instead", handles "Remove excusal" and shows a refusal in place, and after either reads everything again (history, report, recordable). Add to `tests/HT.UiTests/Components/Pages/GoalDetailPageTests.cs`: after excusing through "Excuse instead" the History shows Excused and the GoalStats card's consistency changes without a reload; after "Remove excusal" the row is Pending with "Add record". Before changing the page's absent-text assertions, list them with `grep -n "absent\|ht-card__title" tests/HT.UiTests/Components/Pages/GoalDetailPageTests.cs`: T042 already took "Excuse" out of the substring absent list (the legend's "Excused" contains it), so a test here checks the "Excuse instead" and "Remove excusal" buttons, never the bare word
- [X] T053 [US1] Add to `tests/HT.UiTests/Host/HomeDashboardHostTests.cs` and `GoalsHostTests.cs` (after T051, T052): the owner's prerendered Home has "Excuse" on a Pending commitment row; the owner's goal page shows an Excused History row with its reason and "Remove excusal"

**Checkpoint**: the full suite passes; a commitment can be excused from the Home and from the RecordDialog, and the
excusal removed on the goal page. Commit.

---

## Phase 4: User Story 2 - Pause and resume a goal (Priority: P1)

**Goal**: a registered user pauses any goal for a date range, resumes it early, cancels an upcoming pause and reads
its pauses.

**Independent Test**: create "Drinking water, 8 glasses a day" at an earlier supplied time, pause it on Monday 16
November from that day until Sunday 22 November, and read its history and report during and after the pause: the
paused days are Paused and not counted, and the days before keep their results.

### Tests for User Story 2

- [X] T054 [P] [US2] Write `tests/HT.UnitTests/Application/Exemptions/PauseAsyncTests.cs` with `ExemptionWorld` (after T055; FR-006 to FR-008, US2-AS1 to AS4, AS8, SC-003, SC-004): every step of FR-006 with its text and the order pairs (a missing day with a first day before today gives step 4; a first day before today with a reversed range gives step 5; a reversed range that overlaps gives step 6; an overlap with a 201-character reason gives step 7); AS1 "paused" with the identifier and days, Monday 16 November Paused and not recordable, Sunday 15 November still Pending and recordable; AS2 from Sunday 15 refused; AS3; AS4; a first day of today − 1, today and today + 1; a touching pause accepted; a goal that has not started uses the account's today in its zone (a goal starting tomorrow paused from today is accepted; from yesterday refused); a goal whose version zone differs from the account's uses the goal's current date (R-03); AS8 Friday 13 November stays Failed; every Pending and final result before the pause keeps its state (SC-004, a theory over Met, Failed, Missed and Pending entries); a Due day with a record above a Reduce limit becomes Paused, its record kept (FR-008); a pause of several months is accepted and leaves the streak before it unchanged when read after it (AC-088, Edge Cases "Long pauses"; the consistency follows FR-013, plan "Spec discrepancies" 1); a concurrent overlapping pause simulated through T029's hook gives "This goal already has a pause on these dates."; nothing is stored on any refusal
- [X] T056 [P] [US2] Write `tests/HT.UnitTests/Application/Exemptions/ResumeTests.cs` with `ExemptionWorld` (after T055; FR-010, US2-AS5, AS6, AS11, AS12, D-1, SC-003): AS5 resume on Wednesday 18 November gives "resumed", the pause runs 16 to 17 November, those stay Paused, 18 November is Due and recordable, the goal is not paused; AS6 resume on the first day removes the pause and Monday 16 November's records count as never paused; AS11 the yes/no goal; AS12 the per-week goal: both weeks Paused, a record on Wednesday 18 November (not in the pause) gives "This week is paused.", resume on Monday 23 November leaves 16–22 November Paused and makes 23–29 November Due; "This goal is not paused." with no pause, a past pause, an upcoming pause, and a current week Paused only by an upcoming pause (Edge Cases); a goal that has not started resumed from a pause current at the account's today (AC-087, research R-03): on the pause's first day the pause is removed, later it ends on the day before
- [X] T057 [P] [US2] Write `tests/HT.UnitTests/Application/Exemptions/CancelPauseTests.cs` and `PausesListTests.cs` with `ExemptionWorld` (after T055; FR-011, FR-012, US2-AS7, AS13, US5-AS4): AS7 cancel an upcoming pause gives "pause cancelled" and no pause on those dates; a current pause and a past pause give "Only a pause that has not begun can be cancelled. Resume the goal instead."; an unknown `PauseId` and a pause of another of the user's goals give "This pause does not exist."; the order pair (another goal's current pause gives step 4, not step 5); cancelling the pause that pauses the current week makes it Due again; AS13 the list with first day, last day, reason "Family visit" and `Current`, with past and upcoming pauses, latest first day first, each `RangeText` ("Mon 16 Nov – Sun 22 Nov"); a goal that has not started lists its pauses as current or past by the account's today in its zone (AC-086, research R-03)
- [X] T058 [P] [US2] Write `tests/HT.UnitTests/Application/Exemptions/PauseRecordingTests.cs` with `ExemptionWorld` (after T055; FR-009, US2-AS9, AS10, Edge Cases): AS9 a record on Tuesday 17 November during the pause gives "This goal is paused on this date."; AS10 a record of Monday 16 November at 08:00, then the pause at 09:00: the record is kept and shown with the Paused day, can be changed and deleted, and the day stays Paused; an excusal kept under a pause shows as Excused again after the pause is cancelled, and after a resume that ends the pause before it; after the account's time zone changes (slice 4 FR-026, through `ProfileService` as slice 4's tests change it), an excused commitment keeps its excusal on the same date, a commitment moved by the change keeps it, and a pause still covers the same dates of the goal's timeline (Edge Cases "Time zone changes")
- [X] T059 [P] [US2] Add to `tests/HT.IntegrationTests/Records/ExemptionRaceTests.cs` (after T055; R-04): two overlapping pauses started together end with exactly one stored; a pause and a record of the same date started together, in both orders, end with the record refused (`Exempt` → "This goal is paused on this date.") or kept under the pause
- [X] T060 [P] [US2] Write `tests/HT.UiTests/Components/Exemptions/PauseDialogTests.cs` (after T062; HP-002, HP-007): the title "Pause · Drinking water, 8 glasses a day", the effect first, "From" with the current date and `min` today, "Until" required, "Reason (optional)", the buttons; focus on "From" on open; Esc and × close and return focus; each refusal under its field, one at a time (the five of T004); "Pause goal" calls `PauseAsync` and raises the saved callback
- [X] T061 [P] [US2] Write `tests/HT.UiTests/Components/Exemptions/PausesCardTests.cs` (after T063; HP-001): one row per pause with its `RangeText`, the reason in `ht-muted` when it has one, "Resume now" on the current pause, "Cancel pause" on an upcoming one, no action on a past one; no card with no pause; the actions raise their callbacks

### Implementation for User Story 2

- [X] T055 [US2] Create `src/HT.Application/Exemptions/PauseService.cs` with the constructor and the four methods of contracts/application-api.md, and `PauseOutcome`, `PauseResult`, `ResumeOutcome`, `ResumeResult`, `CancelPauseOutcome`, `CancelPauseResult`, `PausesOutcome`, `PausesResult` and `PauseView` (with `RangeText` from `DateTexts`) in `src/HT.Application/Exemptions/` (after T036; FR-006 to FR-012, FR-021, FR-022, research R-03, R-05):
  - the pause date computed once per call (Conventions), from the goal's current date or the account's today;
  - `PauseAsync`: steps 4 to 7 from `PauseRules.Check`, step 8 from `ExemptionReason.TryCreate`, then `AddPauseAsync`; a store `Overlaps` gives step 7's refusal;
  - `ResumeAsync`: `CurrentPause(pauseDate)`, then `PauseRules.Resume`: `DeletePauseAsync` or `SetPauseLastDayAsync(pauseDate − 1)`;
  - `CancelPauseAsync`: the pause among this goal's (`ReadExemptionsAsync`), its timing, then `DeletePauseAsync`; a store `PauseNotFound` gives step 4's refusal;
  - `GetPausesAsync`: every pause, latest first day first, with its timing at the pause date.

  In the same commit: register it (scoped); add it to `PageServices.cs` and to `SignedInServices`; widen the `IGoalStore` rule of T044 to admit `HT.Application.Exemptions.PauseService` too, renaming it to say so
- [X] T062 [P] [US2] Create `src/HT.Ui/Components/Exemptions/PauseDialog.razor` on `HtDialog` (after T004, T011, T055; HP-002, HP-007, contracts/host.md "PauseDialog"): "From" defaults to the page's current date; each refusal under its field by its outcome; calls `PauseAsync` through `UseCaseScope`
- [X] T063 [P] [US2] Create `src/HT.Ui/Components/Exemptions/PausesCard.razor` on `HtCard` (after T003, T011, T055; HP-001): it takes the `PauseView`s and two callbacks (resume, cancel) and renders as T061 describes
- [X] T064 [US2] Change `src/HT.Ui/Components/Records/GoalDetailBody.razor` and `HistoryCard.razor` (after T052, T062, T063; HP-001, FR-009, US2-AS14):
  - `GoalDetailBody` adds the read `GetPausesAsync`; the secondary "Pause" (`aria-haspopup="dialog"`) before "Record", absent while a pause is `Current`, opens the PauseDialog; the Notice under the head while a pause is `Current`, "Paused until Sun 22 Nov · Resume now" (`ReportText.PausedUntil`), whose "Resume now" calls `ResumeAsync` and shows a refusal in the Notice's place; the Pauses card after Settings; after any of these it reads everything again and gives focus back (Conventions);
  - `HistoryCard`: a Paused row has its StatusChip, no "Add record", the edit action of a kept record and its note actions.

  Update `tests/HT.UiTests/Components/Pages/GoalDetailPageTests.cs`: the exact page-head button lists `["Record"]` (today lines 115 and 308) become `["Pause", "Record"]` for a goal that is not paused, and T042's "no button reads 'Pause'" check goes (the absent list keeps "Edit settings", "End goal" and "Earlier attempts"); the right column's card titles gain "Pauses" for a goal with a pause; new cases for the Notice, the hidden "Pause", "Resume now" and its refusal, the Pauses card's actions, and the page after saving the PauseDialog (US2-AS14). Add the Paused row cases to `tests/HT.UiTests/Components/Records/HistoryCardTests.cs`, and to `NoteDialogTests.cs` "Add note" on a Paused row with no record, whose sub line ends "· Paused"
- [X] T065 [US2] Add to `tests/HT.UiTests/Host/GoalsHostTests.cs` (after T064; HP-001, Edge Cases "A Paused week of a goal that is not paused"): the owner's prerendered page of a paused goal has the Notice, no "Pause" and the Pauses card; a per-week goal with an upcoming pause later in its current week has no Notice, its current week Paused in the History, and "Pause" present

**Checkpoint**: the full suite passes; a goal can be paused, resumed early and its upcoming pause cancelled on the goal
page. Commit.

---

## Phase 5: User Story 3 - Reports leave excused and paused expectations out (Priority: P1)

**Goal**: consistency, streaks, recent results and the trend treat Excused and Paused results as exempt.

**Independent Test**: build "Meditation, 10 min daily" with Met, Missed, Excused, Paused and Pending results at known
supplied times and read its report on Thursday 19 November at 12:00: "90% (10 of 11)", streaks 2 and 8.

The rules are T018, T019 and T036. This phase proves them through the use case and the page. A failing test is fixed
in the file whose rule it shows wrong.

### Tests for User Story 3

- [X] T066 [P] [US3] Write `tests/HT.UnitTests/Application/Reports/GoalReportExemptionTests.cs` with `ExemptionWorld`, through `ExcusalService` and `PauseService` (after T044, T055; FR-013 to FR-015, FR-024, US3-AS1 to AS3, AS5 to AS7, SC-002): US3's goal (9 November excused on 10 November, 13 to 15 November paused on 13 November at 08:00, read on 19 November at 12:00): "90% (10 of 11)", current 2 and longest 8 commitments; its 17 recent results oldest first with 10 Met, 1 Missed, 1 Excused, 3 Paused, 2 Pending and those counts; AS5 the excusal removed on 10 November at 12:00 and 9 November recorded with 4 minutes gives "83% (10 of 12)" and longest 7, equal to the same goal recorded on time; AS6 another account's goal with only exempt ended results gives "N/A" and 0 and 0 commitments; AS7 the per-week goal gives "100% (2 of 2)" and 2 weeks; for each case, the figures equal those of the same goal with the exempt results removed from the sequence (SC-002)
- [X] T067 [P] [US3] Add to `tests/HT.UnitTests/Application/Reports/GoalReportTrendTests.cs` and `GoalReportRecentTests.cs` with `ExemptionWorld` (after T044, T055; FR-015, FR-016, US3-AS4): US3-AS4's 14 bars from 6 to 19 November with their marks, null values and the six values 10; a commitment-based yes/no goal with an Excused and a Paused commitment in a week (Build target and value); the "no value, not 0" week; an all-exempt past week marked; a current week whose begun commitment is Excused and whose upcoming commitments are Paused marked `Paused` (T036, FR-016); a current week with an Upcoming commitment that is not Paused left unmarked; `TrendView.Target` from the latest unmarked bar; `RecentResults.Counts` has the keys `Excused` and `Paused` only when above 0, and exactly slice 6's four keys for a goal without exemptions (FR-023)
- [X] T068 [P] [US3] Add to `tests/HT.IntegrationTests/Reports/GoalReportReadTests.cs` (after T044, T055; FR-024, constitution II): on the real stores, with the test's own provider and clock, a goal with an excusal and a pause made through the services: the report's figures equal those of the same facts on fakes; a cancelled pause and a removed excusal change the next report. Re-run `ReportTimingTests.cs` (slice 6 SC-007) unchanged; it must still pass with the extra read per goal
- [X] T069 [P] [US3] Add to `tests/HT.UiTests/Components/Shared/GoalStatsTests.cs` and `TrendChartTests.cs` (after T070, T071; HP-001, HP-006, US3-AS8): the strip with Excused and Paused cells (the legend's six entries are already asserted by T042), the label "Last 17 commitments: 10 met, 1 missed, 1 excused, 3 paused, 2 pending"; a marked bar with `ht-bar--paused` or the Excused class, its `title` "Excused" or "Paused", the chart's `aria-label` counting them apart from bars with no record; an unmarked trend renders as in slice 6 (`TrendChartTests.cs` has no absence assertion to change today)

### Implementation for User Story 3

- [X] T070 [US3] Change `src/HT.Ui/Components/Shared/GoalStats.razor` if it lists the legend itself (after T042): the legend Met, Failed, Missed, Excused, Paused, Pending, through `ConsistencyStrip` (HP-001). If the legend comes only from `ConsistencyStrip` (T042), change nothing and say so in the task's note
- [X] T071 [US3] Change `src/HT.Ui/Components/Shared/TrendChart.razor` (after T010, T011, T035; FR-016, HP-006): a bar with `Mark` is drawn as the design's Paused bar (`ht-bar--paused`) or Excused bar (T010's class), with its `title` and the chart's `aria-label` from `ReportText`; the target line at `TrendView.Target`
- [X] T072 [US3] Add to `tests/HT.UiTests/Components/Pages/GoalDetailPageTests.cs` and `tests/HT.UiTests/Host/GoalsHostTests.cs` (after T064, T070, T071; HP-001, FR-024): the goal page of US3's goal shows Excused and Paused cells and marked bars; after pausing in the PauseDialog, the GoalStats card and the trend change without a reload

**Checkpoint**: the full suite passes; the goal page's figures leave excused and paused results out. Commit.

---

## Phase 6: User Story 4 - Paused goals on the Goals page and Home (Priority: P2)

**Goal**: a paused goal moves to the Goals page's Paused tab with "Paused until …" and "Resume", and the Home stops
expecting anything of it.

**Independent Test**: pause one of two goals and read the goals overview and the dashboard: the paused goal has its
`PausedUntil`, no Due entry, and is not counted among active goals.

### Tests for User Story 4

- [X] T073 [P] [US4] Add to `tests/HT.UnitTests/Application/Reports/GoalsOverviewTests.cs` with `ExemptionWorld` (after T055; FR-018, US4-AS1, Edge Cases): AS1 "Drinking water" with `PausedUntil` 22 November and "Meditation" with none, in slice 6's order; a goal whose current week is Paused only by an upcoming pause, and one after a mid-week resume, have none; a goal that has not started with a current pause at the account's today has it; a past pause gives none
- [X] T074 [P] [US4] Add to `tests/HT.UnitTests/Application/Reports/DashboardTests.cs` with `ExemptionWorld` (after T044, T055; FR-019, FR-020, US4-AS2 to AS4): AS2 "Drinking water" with no Due and no Upcoming entry, its Pending 14 and 15 November listed, "Meditation" Due on 16 November, `Counts.Goals` 1, `PausedUntil` on the paused goal's summary; AS3 the excused 15 November commitment in neither the Pending count nor the Today list, 14 November still there; AS4 the Upcoming entry Friday 20 November, past the Paused Wednesday 18; the Missed-this-week count leaves out exempt entries; a note on a Paused entry in the recent notes with that state
- [X] T075 [P] [US4] Add to `tests/HT.UiTests/Components/Reports/GoalsListTests.cs`, `tests/HT.UiTests/Components/Shared/HabitComponentTests.cs` (the `GoalItem` cases) and `tests/HT.UiTests/Components/Pages/GoalsPageTests.cs` (after T076, T077; HP-004, US4-AS5, AS6): the tabs "Active" and "Paused" and no "Ended"; Active lists the goals with no `PausedUntil`, Paused the others in the same order, each with "Paused until Sun 22 Nov", the Pending chip when it has one, figures, strip and "Resume"; "Resume" calls `ResumeAsync` and the goal moves to Active; the empty Paused tab "No paused goals."; `?tab=paused` selects Paused and any other value Active; a user with no goals keeps the EmptyState; the footnote word for word. Existing assertions that change in `GoalsPageTests.cs`: the slice-6 footnote (today near line 104) becomes the new one, and the absent list `{ "Paused", "Ended", "Excuse" }` (today near line 107) loses "Paused", which the tab now shows, and keeps "Ended" and "Excuse"

### Implementation for User Story 4

- [X] T076 [US4] Change `src/HT.Ui/Components/Shared/GoalItem.razor` (after T007, T011, T035; HP-004): a Paused form with the meta "Paused until …" (`ReportText.PausedUntil`) and the secondary "Resume", raising a callback; its other parts as slice 6
- [X] T077 [US4] Change `src/HT.Ui/Components/Reports/GoalsList.razor` and `src/HT.Ui/Components/Pages/Goals.razor` (after T055, T076; HP-004, contracts/host.md "Goals page"): `Goals.razor` reads `tab` from the query and passes it to the island; `GoalsList` reads the overview once, shows the existing `Tabs` component with "Active" and "Paused" as links (`/goals`, `/goals?tab=paused`), so the tab works prerendered; Active lists goals with no `PausedUntil`, Paused the rest; "Resume" calls `ResumeAsync` through `UseCaseScope`, reads the overview again and gives focus to the nearest remaining item or the tab (Conventions); "No paused goals."; the new footnote from `ReportText`; the EmptyState with no goals
- [X] T078 [US4] Change `src/HT.Ui/Components/Pages/Home.razor` (and the component that renders the "Active goals" card, if it is not `Home.razor`) (after T036; HP-005): the "Active goals" tile shows `Counts.Goals`; the card lists only the `DashboardGoal`s with no `PausedUntil`. Add to `tests/HT.UiTests/Components/Pages/HomeSignedInTests.cs`: a paused goal is missing from the card and the tile counts 1
- [X] T079 [US4] Add to `tests/HT.UiTests/Host/GoalsHostTests.cs` and `HomeDashboardHostTests.cs` (after T077, T078): the prerendered `/goals?tab=paused` lists the paused goal with "Paused until" and "Resume", and `/goals` lists the other; the Home's Active goals card leaves the paused goal out. The exact slice-6 footnote HTML in `GoalsHostTests.cs` (today near line 347) becomes the new footnote

**Checkpoint**: the full suite passes; paused goals sit on their own tab and the Home leaves them out. Commit.

---

## Phase 7: User Story 5 - Privacy of excusals and pauses (Priority: P1)

**Goal**: excusals, pauses and their reasons are owner-only, administrators included.

**Independent Test**: try each of the six use cases as another registered user, an administrator and an anonymous
visitor on someone else's goal; each is refused and nothing changes.

### Tests for User Story 5

- [X] T080 [P] [US5] Write `tests/HT.UnitTests/Application/Exemptions/ExemptionAccessTests.cs` with `ExemptionWorld` (after T044, T055; FR-021, FR-022, US5-AS1 to AS4, SC-005): a theory over the six use cases × user B ("This goal does not exist."), an administrator ("Goals are for registered users."), the anonymous visitor ("You are not signed in.") and an account id that is not stored ("You are not signed in."), each on user A's goal with an excusal and a pause: the refusal text, no view (`Pauses` empty), and the store's excusals, pauses and records unchanged; the access steps come before every other check (an administrator with a period-based goal and a missing day gets the access text); user A cancelling A's pause through A's other goal and user B through B's goal get "This pause does not exist."
- [X] T081 [P] [US5] Write `tests/HT.IntegrationTests/Records/ExemptionPrivacyTests.cs` on `HT-IT` (after T044, T055; FR-021, constitution III): with the real stores, user B's calls on user A's goal and user A's `PauseId` change no row (counted with `Sql.cs` before and after) and return nothing of A's; an administrator likewise; follow `RecordPrivacyTests.cs`
- [X] T082 [P] [US5] Write `tests/HT.IntegrationTests/Records/ExemptionLogTests.cs` with `LogCapture` (after T044, T055; HP-009, US5-AS5): excusing, removing, pausing, resuming, cancelling and reading with a reason, a note and recorded values that carry a unique marker, and a store fault forced as `RecordLogTests.cs` forces one: no captured log line or exception message contains the marker, a reason, a goal name or a habit name; follow `RecordLogTests.cs`
- [X] T083 [P] [US5] Add to `tests/HT.UiTests/Host/GoalPrivacyHostTests.cs` (after T064, T077): user B opening user A's goal page gets the not-found page with no Pause, Notice, Pauses card, reason or Excused row of A's; `/goals?tab=paused` as B lists none of A's goals; an administrator likewise

**Checkpoint**: the full suite passes, the architecture rules included. Commit.

---

## Phase 8: User Story 6 - Demo excusals and pauses (Priority: P2)

**Goal**: a fifth startup task creates `demo6@example.com` with Excused results, a past and a current pause, and
figures that leave them out; the smoke check finds them on the hosted demo.

**Independent Test**: run demo pause loading on a reset database at a fixed run time and read `demo6`'s reports:
"95% (22 of 23)" and "94% (18 of 19)", the second goal paused until R + 5, the dashboard 1 Due, 2 Pending, 0 Missed
this week, 1 goal.

### Implementation for User Story 6 (data and port)

- [X] T084 [P] [US6] Create `src/HT.Application/Operator/DemoExcusal.cs`, `DemoGoalPause.cs`, `DemoPauseGoal.cs` and `DemoPauses.cs` as data-model.md gives them (after T012; FR-031, research R-08): `DemoPauses.For(DateOnly runDate)` builds FR-031's two goals as `DemoPauseGoal`s, each wrapping a `DemoHistoryGoal` with its records and notes (see `DemoReport.cs` and `DemoHistory.cs` for that shape) beside its `DemoExcusal`s and `DemoGoalPause`s, every date as R − k; the reason and note constants "Travelling.", "Ill with flu.", "Family visit.", "Overslept.", "Busy day."; `GoalNames`
- [X] T085 [US6] Create `src/HT.Application/Operator/IDemoPauseStore.cs`, `DemoPauseLoadingResult.cs`, `DemoPauseAccountResult.cs` and `DemoPauseService.cs` (after T084; FR-031, contracts/application-api.md "DemoPauseService"): follow `DemoReportService.cs` and its results; the run date R is `defaultZone.DateAt(now)`; the password checks and the "already exists" rule (ignoring case, either role, or the store's `EmailTaken`) with no read of the existing account (ADR 0013); a goal whose catalog habit is not Published (by name, ignoring case) is left out with its records, notes, excusals and pauses; the texts from `OperatorTexts`; the results name only the email. `DemoPauseLoadingResult` gets its own outcome enum `DemoPauseLoadingOutcome`, as `DemoReportLoadingResult` has `DemoReportLoadingOutcome` (the host switches on it, T091); record it in data-model.md "Application: demo pause loading"
- [X] T086 [P] [US6] Create `tests/HT.UnitTests/Fakes/FakeDemoPauseStore.cs` (after T085), following `FakeDemoReportStore.cs`
- [X] T087 [US6] Add an internal method for the excusal and pause rows to `src/HT.Persistence/Habits/DemoAccountWriter.cs`, written in the same transaction after the goal's records and notes, and create `src/HT.Persistence/Habits/DemoPauseStore.cs`, an `internal sealed` class implementing `IDemoPauseStore` over it, following `DemoReportStore.cs` (after T030, T085; R-08)
- [X] T088 [US6] In the same commit as T085 to T087: register `DemoPauseService` in `src/HT.Application/ApplicationServiceCollectionExtensions.cs` and `IDemoPauseStore` in `src/HT.Persistence/PersistenceServiceCollectionExtensions.cs`; add the fence in `tests/HT.ArchitectureTests/ApplicationDependencyTests.cs` that keeps `IDemoPauseStore` for `DemoPauseService` only, as the `IDemoReportStore` rule does; in `PersistenceDependencyTests.cs`, add `[InlineData("HT.Persistence.Habits.DemoPauseStore")]` to the theory `TheDemoAccountWriterAndTheDemoReportStoreAreInternal`, renaming it and its doc comment to name the pause store too; in `UiDependencyTests.cs`, add a rule that no UI type may depend on `IDemoPauseStore`, as `NoUiTypeMayDependOnTheDemoReportStorePort` does, and add `DemoPauseService` to the operator services that only the host's operator types may use (`OnlyTheHostsOperatorTypesMayDependOnTheOperatorServices`). Check every place with `grep -n "IDemoReportStore\|DemoReportService" tests/HT.ArchitectureTests/*.cs`

### Tests for User Story 6

- [X] T089 [P] [US6] Write `tests/HT.UnitTests/Application/Operator/DemoPausesTests.cs` and `DemoPauseLoadingTests.cs` (after T085, T086; FR-031, US6-AS1, AS3, AS4, research R-11): `DemoPauses.For(R)` gives exactly FR-031's dates, values, notes, excusals, pauses and reasons for R on each of the seven weekdays; every reason passes `ExemptionReason`; loading with no password, a blank one and a weak one gives its result and creates nothing; "created", "demo goals added", "demo records added" for `demo6@example.com`, also with one or both habits not Published; "already exists" for an existing account, any case, either role, and for the store's `EmailTaken`; no result names a goal, habit, note, reason, unit or amount; follow `DemoReportLoadingTests.cs` and `DemoReportTests.cs`
- [X] T090 [P] [US6] Write `tests/HT.IntegrationTests/Operator/DemoPauseLoadingTests.cs` on `HT-IT-SEED` (after T088; FR-030, FR-031, US6-AS1 to AS5, SC-006): after a reset, the seed catalog and the three earlier demo tasks (demo loading, demo history loading, demo report loading), with the test's own provider at a run time on each of the seven weekdays: `demo6`'s reports read at the run time give FR-031's figures ("95% (22 of 23)", 10 and 12 commitments, R − 2 Excused, R − 1 Pending, R Due; "94% (18 of 19)", 2 and 16 days, R − 2 Pending, R − 1 and R Paused, `PausedUntil` R + 5; the dashboard 1 Due, 2 Pending, 0 Missed this week, 1 goal); a fault in the middle of the write leaves no row of the account (as a whole or not at all); a second run gives "already exists" and changes nothing; demo loading, demo history loading and demo report loading leave no excusal or pause row and their accounts' figures unchanged (FR-030); follow `DemoReportLoadingTests.cs` and `tests/HT.IntegrationTests/Reports/DemoReportsTests.cs`

### Host and smoke check (HP-010, HP-011)

- [X] T091 [US6] Add the step "Demo pause loading" after "Demo report loading" to `src/HT.Ui/Operator/StartupTasks.cs`, with the same step wrapper and a `LoggerMessage` "Demo pause loading: {Result}" for a missing or weak password, the account lines as demo report loading logs them, and the fault line "Demo pause loading failed; the app starts without it" with no secret or value (after T088; HP-010, contracts/host.md "Operator log"). Update `tests/HT.UiTests/Host/StartupLogTests.cs` and `tests/HT.UiTests/Operator/StartupTasksDemoTests.cs`, whose exact "Demo " lines now expect `demo6@example.com` after `demo5@example.com` (spec "Earlier requirements and tests this slice changes"), and add the missing-password, weak-password and fault cases (the fault does not stop the start)
- [X] T092 [P] [US6] Add `PageReader.ReadGoalMetas` to `tools/HT.SmokeCheck/PageReader.cs` (the meta text of each goal item, paired with its goal name) and its cases to `tests/HT.UiTests/SmokeChecks/PageReaderTests.cs`: a Paused item's "Paused until Sun 22 Nov", an Active item's meta, an empty list (after T077; HP-011)
- [X] T093 [US6] Add check 8 "demo pause" to `tools/HT.SmokeCheck/SmokeCheckRunner.cs` and its four `SmokeCheckCoverage` entries for `IDemoPauseStore.CreateDemoPauseAccountAsync` (email, password, profile, goals) to `tools/HT.SmokeCheck/SmokeCheckCoverage.cs` (after T091, T092; HP-011, contracts/host.md "Smoke check"): signed in as `demo6@example.com`, "Excused" among the status chips (`PageReader.ReadStatusChips`) of "Meditation, 10 min daily"'s page, and on `/goals?tab=paused` the goal name "Drinking water, 8 glasses a day" with a meta starting "Paused until" in the same item; it prints `PASS demo pause demo6@example.com (N of N)` or its FAIL line, never a goal name, reason, note or figure. In the same change: `RunnerBehaviourTests.GoodSite()`, `SmokeCheckHostTests`' exact lines and its "(N of N)", `CoverageTests` (`IDemoPauseStore` joins its `Ports` list, today near line 20, and the class's doc comment, so `Every_coverage_entry_matches_a_data_parameter_of_its_demo_data_port` accepts the four entries and `Every_data_parameter_of_a_demo_data_port_has_a_coverage_entry` checks the new port; 14 to 18 entries; its count of the entries that are not `IDemoReportStore`'s, today `Assert.Equal(10, …)` near line 96, becomes 14, or the test also leaves out `IDemoPauseStore` and keeps 10, whichever states its rule), and the coverage table and check 8 in `specs/003-demo-hosting/contracts/smoke-check.md`

**Checkpoint**: the full suite passes; the host starts with five startup tasks and the smoke check has check 8. Commit.

---

## Phase 9: Polish & cross-cutting concerns

- [X] T094 [P] Update `docs/operations/hosted-demo.md` (`demo6@example.com`, the fifth startup task, check 8) and the Status section of `README.md` (slice 7's line, as slice 6's reads) (plan, Work order 8)
- [X] T095 [P] Update `CONTEXT.md` only if implementation sharpened a term; HP-012's entries are already there. Record "no change" in the task's note otherwise
- [X] T096 Run Stryker from `tests/HT.UnitTests`: `dotnet stryker --project HT.Domain.csproj` and `dotnet stryker --project HT.Application.csproj`, the whole project (in a worktree `--since` finds nothing; ADR 0022). Fix every surviving mutant in the new and changed code (`HT.Domain.Exemptions`, `Evaluator`, `Consistency`, `Streaks`, `TrendBuilder`, `HT.Application.Exemptions`, `EntryViews`, `RecordingService`'s new checks, `GoalEvaluation`, `ReportService`'s changed reads, `ReportText`, `DemoPauses`, `DemoPauseService`) with a test; a `// Stryker disable` comment only for an equivalent mutant, with its reason. Report the scores
- [X] T097 Compare the public API of `HabitTracker.slnx` with T002's baseline (FR-023): run the three `roslynator list-symbols` commands again and diff them. Only additions are allowed, and only those listed in data-model.md and the Conventions; a changed or removed member is a failure to fix, not to accept
- [X] T098 [P] Run `roslynator analyze HabitTracker.slnx` and `jb inspectcode HabitTracker.slnx --stdout -f=Text -e=WARNING --caches-home=<scratchpad>/jbcache --no-updates` and compare with the `CLAUDE.md` baseline (579 diagnostics, 52 not from the compiler; 209 `jb` warnings). Fix every new finding in this slice's code that is not one of the known false positives; update the baseline numbers and date in `CLAUDE.md`, saying where the change comes from
- [X] T099 Run `dotnet build HabitTracker.slnx` and `dotnet test HabitTracker.slnx --nologo -v q --logger "console;verbosity=minimal"`: no warning, every test passes. Then walk quickstart.md section 3 against the running app as far as the session allows (sign-in as `demo6@example.com` uses the demo password from User Secrets, which is never read or printed; if it cannot be typed by the owner, label that part unverified). The hosted steps of quickstart.md section 4 stay unverified until the deployment (constitution V)
- [X] T100 Keep slice 7's Status in `docs/roadmap.md` at `implementing` and set its `updated_at` to today, as slice 6 did at this point; the roadmap's status values have no "implemented". The next status, `AC tests green`, is set after `/speckit-ac-tests-analyze`, which the owner starts

---

## Dependencies & execution order

### Phase dependencies

- **Setup (Phase 1)**: no dependency.
- **Foundational (Phase 2)**: after Setup. Blocks every story. Inside it: design (T003 to T011) and Domain (T012 to
  T027) can run side by side; the storage (T028 to T031) needs T014; the shared evaluation path (T033 to T036) needs
  T017, T019 and T028; T042 needs T009, T011 and T035.
- **US1 (Phase 3)**: after Phase 2.
- **US2 (Phase 4)**: after Phase 2. Its Application work (T054 to T059) can run beside US1's; its page work (T064)
  follows T052, because both edit `GoalDetailBody.razor` and `HistoryCard.razor`.
- **US3 (Phase 5)**: after US1 and US2, whose services build its test data (T066 to T068), and after T064 for the
  page test (T072).
- **US4 (Phase 6)**: after US2 (`ResumeAsync`, `PausedUntil` through pauses); T074 also needs US1.
- **US5 (Phase 7)**: after US1, US2 and US4 (its host test reads the Paused tab).
- **US6 (Phase 8)**: after Phase 2 for T084 to T090; T092 and T093 after US4 (the Paused tab) and US1 (the Excused
  chip on the goal page).
- **Polish (Phase 9)**: after every story.

### User story dependencies

- **US1 (P1)**: Phase 2 only. MVP.
- **US2 (P1)**: Phase 2 only, apart from the shared page file with US1.
- **US3 (P1)**: needs US1 and US2 for realistic data; its rules are in Phase 2.
- **US4 (P2)**: needs US2.
- **US5 (P1)**: needs US1, US2 and US4.
- **US6 (P2)**: needs US1, US2 and US4 for the smoke check.

### Within each story

Application first, then its tests (which name the task they follow), then the components, then the page, then the
host tests. Tests written before their implementation task fail first, then pass.

## Parallel opportunities

- Phase 2: T003 to T010 (eight design files); T012 with the design tasks; T018 beside T017 and T019; T020 to T026
  (seven test files); T030 and T032 beside T029; T037, T040 and T041 side by side, then T038 and T039 (they use
  T037's world).
- US1: T043, T045, T046 and T048 once T044 is done; T047 once T048 is done.
- US2: T054, T056, T057, T058 and T059 once T055 is done; T062 and T063 side by side, then T060 and T061.
- US3: T066, T067 and T068 side by side.
- US4: T073 and T074 side by side.
- US5: T080 to T083 side by side.
- US6: T084 beside the earlier phases' tests; T086 and T089 once T085 is done; T092 beside T091.

### Parallel example: User Story 2

```text
After T055 (PauseService):
  T054 PauseAsyncTests.cs      T056 ResumeTests.cs        T057 CancelPauseTests.cs + PausesListTests.cs
  T058 PauseRecordingTests.cs  T059 ExemptionRaceTests.cs (pauses)
  T062 PauseDialog.razor       T063 PausesCard.razor
Then T060, T061 (component tests), then T064 (GoalDetailBody), then T065 (host).
```

### Parallel example: Phase 2 tests

```text
After T012 to T019:
  T020 ExemptionReasonTests.cs   T021 PauseTests.cs          T022 GoalExemptionsTests.cs
  T023 PauseRulesTests.cs        T024 EvaluatorExemptionTests.cs
  T025 Consistency/Streaks       T026 TrendBuilderExemptionTests.cs
```

## Implementation strategy

### MVP first (User Story 1)

1. Phase 1 and Phase 2: the design, the Domain rules, the storage and the shared evaluation path. After Phase 2
   every read already shows Excused and Paused for seeded facts.
2. Phase 3 (US1): excusing from the Home and the RecordDialog, removing on the goal page.
3. **Stop and validate**: excuse a Pending commitment, read its history and report after its grace end.

### Incremental delivery

1. US1, then US2: both exemptions usable from the pages.
2. US3: the figures proved through the use case and shown on the goal page.
3. US4: the Paused tab and the Home.
4. US5: privacy across all six use cases.
5. US6: the demo account, the startup task and check 8.
6. Polish: Stryker, the API comparison, the analyzers, the docs and the full run.

Each checkpoint leaves the full suite green and is committed. The acceptance tests of all six features run after
`/speckit-implement`, when the owner starts `/speckit-ac-tests-write` and `/speckit-ac-tests-analyze`.
